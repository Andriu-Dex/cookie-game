using Juego_Galleta.Domain.Entities;
using Juego_Galleta.Domain.Interfaces;

namespace Juego_Galleta.Application.AI;

/// <summary>
/// Heuristic evaluator for the Dots and Boxes (Galleta) game.
/// Implements the Strategy pattern for game state evaluation.
/// 
/// The evaluation considers:
/// - Material: difference in captured cells
/// - Almost-complete cells: cells with 3 sides drawn (dangerous to create)
/// - Safe moves: moves that don't create 3-sided cells
/// - Two-sided cells: potential for creating chains
/// </summary>
public sealed class SimpleDotsEvaluator : IEvaluator
{
    // Weight constants for the heuristic components
    private const int MaterialWeight = 100;      // W1: Captured cells are most important
    private const int AlmostWeight = 20;         // W2: Cells with 3 sides (opponent can capture)
    private const int SafeMovesWeight = 5;       // W3: Moves that don't gift cells
    private const int TwoSidedWeight = 2;        // W4: Cells with 2 sides (chain potential)

    /// <summary>
    /// Evaluates the game state from the maximizing player's perspective.
    /// </summary>
    public int Evaluate(GameState state, int maximizingPlayer)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));
        if (maximizingPlayer < 0 || maximizingPlayer > 1)
            throw new ArgumentException("Maximizing player must be 0 or 1.", nameof(maximizingPlayer));

        // If game is over, return a definitive score
        if (state.IsTerminal())
        {
            return EvaluateTerminal(state, maximizingPlayer);
        }

        int minimizingPlayer = 1 - maximizingPlayer;

        // Calculate features for both players
        var maxFeatures = CalculateFeatures(state, maximizingPlayer);
        var minFeatures = CalculateFeatures(state, minimizingPlayer);

        // Combine weighted features
        int material = (state.Scores[maximizingPlayer] - state.Scores[minimizingPlayer]) * MaterialWeight;
        
        // For "almost" cells, having fewer is better (we count opponent's - ours)
        // because opponent's almost cells are opportunities for them
        int almost = (minFeatures.AlmostCells - maxFeatures.AlmostCells) * AlmostWeight;
        
        // More safe moves is better
        int safeMoves = (maxFeatures.SafeMoves - minFeatures.SafeMoves) * SafeMovesWeight;
        
        // More two-sided cells can be good (chain potential)
        int twoSided = (maxFeatures.TwoSidedCells - minFeatures.TwoSidedCells) * TwoSidedWeight;

        return material + almost + safeMoves + twoSided;
    }

    /// <summary>
    /// Evaluates a terminal (game over) state.
    /// Returns a very large positive or negative value based on the winner.
    /// </summary>
    private int EvaluateTerminal(GameState state, int maximizingPlayer)
    {
        int winner = state.GetWinner();
        
        if (winner == maximizingPlayer)
            return 10000; // Large positive value for win
        else if (winner == 1 - maximizingPlayer)
            return -10000; // Large negative value for loss
        else
            return 0; // Tie
    }

    /// <summary>
    /// Calculates strategic features for a given player.
    /// </summary>
    private PlayerFeatures CalculateFeatures(GameState state, int player)
    {
        int almostCells = 0;    // Cells with 3 sides drawn
        int twoSidedCells = 0;  // Cells with 2 sides drawn
        int oneSidedCells = 0;  // Cells with 1 side drawn
        int emptyCells = 0;     // Cells with 0 sides drawn

        // Count cells by number of drawn sides
        for (int cellId = 0; cellId < state.Board.Cells.Count; cellId++)
        {
            // Skip already captured cells
            if (state.CellsOwned[cellId])
                continue;

            int drawnSides = state.CountDrawnEdges(cellId);

            switch (drawnSides)
            {
                case 3:
                    almostCells++;
                    break;
                case 2:
                    twoSidedCells++;
                    break;
                case 1:
                    oneSidedCells++;
                    break;
                case 0:
                    emptyCells++;
                    break;
            }
        }

        // Count safe moves (moves that don't create a 3-sided cell)
        int safeMoves = CountSafeMoves(state);

        return new PlayerFeatures
        {
            AlmostCells = almostCells,
            TwoSidedCells = twoSidedCells,
            OneSidedCells = oneSidedCells,
            EmptyCells = emptyCells,
            SafeMoves = safeMoves
        };
    }

    /// <summary>
    /// Counts how many available moves are "safe" (don't create a 3-sided cell).
    /// A move is safe if it doesn't turn a 2-sided cell into a 3-sided cell.
    /// </summary>
    private int CountSafeMoves(GameState state)
    {
        int safeCount = 0;

        foreach (var move in state.GenerateMoves())
        {
            bool isSafe = true;

            // Check all cells affected by this edge
            int[] affectedCells = state.Board.EdgesToCells[move.EdgeId];

            foreach (int cellId in affectedCells)
            {
                // Skip already owned cells
                if (state.CellsOwned[cellId])
                    continue;

                int currentSides = state.CountDrawnEdges(cellId);

                // If this cell currently has 2 sides, drawing this edge would make it 3-sided
                // This is dangerous because opponent can capture it next turn
                if (currentSides == 2)
                {
                    isSafe = false;
                    break;
                }
            }

            if (isSafe)
                safeCount++;
        }

        return safeCount;
    }

    /// <summary>
    /// Determines if a move would complete at least one cell.
    /// </summary>
    public bool IsCapturingMove(GameState state, Move move)
    {
        if (move.EdgeId < 0 || move.EdgeId >= state.Board.Edges.Count)
            return false;

        int[] affectedCells = state.Board.EdgesToCells[move.EdgeId];

        foreach (int cellId in affectedCells)
        {
            if (state.CellsOwned[cellId])
                continue;

            int currentSides = state.CountDrawnEdges(cellId);
            
            // If cell has 3 sides, this move would complete it
            if (currentSides == 3)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if a move is "safe" (doesn't create a 3-sided cell).
    /// </summary>
    public bool IsSafeMove(GameState state, Move move)
    {
        if (move.EdgeId < 0 || move.EdgeId >= state.Board.Edges.Count)
            return false;

        int[] affectedCells = state.Board.EdgesToCells[move.EdgeId];

        foreach (int cellId in affectedCells)
        {
            if (state.CellsOwned[cellId])
                continue;

            int currentSides = state.CountDrawnEdges(cellId);
            
            // If cell has 2 sides, this move would make it 3-sided (dangerous)
            if (currentSides == 2)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets detailed evaluation breakdown for debugging/analysis.
    /// </summary>
    public EvaluationBreakdown GetDetailedEvaluation(GameState state, int maximizingPlayer)
    {
        if (state.IsTerminal())
        {
            return new EvaluationBreakdown
            {
                TotalScore = EvaluateTerminal(state, maximizingPlayer),
                IsTerminal = true
            };
        }

        int minimizingPlayer = 1 - maximizingPlayer;
        var maxFeatures = CalculateFeatures(state, maximizingPlayer);
        var minFeatures = CalculateFeatures(state, minimizingPlayer);

        int material = (state.Scores[maximizingPlayer] - state.Scores[minimizingPlayer]) * MaterialWeight;
        int almost = (minFeatures.AlmostCells - maxFeatures.AlmostCells) * AlmostWeight;
        int safeMoves = (maxFeatures.SafeMoves - minFeatures.SafeMoves) * SafeMovesWeight;
        int twoSided = (maxFeatures.TwoSidedCells - minFeatures.TwoSidedCells) * TwoSidedWeight;

        return new EvaluationBreakdown
        {
            TotalScore = material + almost + safeMoves + twoSided,
            MaterialScore = material,
            AlmostScore = almost,
            SafeMovesScore = safeMoves,
            TwoSidedScore = twoSided,
            MaxPlayerFeatures = maxFeatures,
            MinPlayerFeatures = minFeatures,
            IsTerminal = false
        };
    }

    /// <summary>
    /// Internal struct to hold calculated features for a player.
    /// </summary>
    public struct PlayerFeatures
    {
        public int AlmostCells { get; set; }
        public int TwoSidedCells { get; set; }
        public int OneSidedCells { get; set; }
        public int EmptyCells { get; set; }
        public int SafeMoves { get; set; }

        public override string ToString() =>
            $"Almost:{AlmostCells}, TwoSided:{TwoSidedCells}, OneSided:{OneSidedCells}, Empty:{EmptyCells}, Safe:{SafeMoves}";
    }

    /// <summary>
    /// Detailed breakdown of evaluation for analysis.
    /// </summary>
    public struct EvaluationBreakdown
    {
        public int TotalScore { get; set; }
        public int MaterialScore { get; set; }
        public int AlmostScore { get; set; }
        public int SafeMovesScore { get; set; }
        public int TwoSidedScore { get; set; }
        public PlayerFeatures MaxPlayerFeatures { get; set; }
        public PlayerFeatures MinPlayerFeatures { get; set; }
        public bool IsTerminal { get; set; }

        public override string ToString()
        {
            if (IsTerminal)
                return $"Terminal: {TotalScore}";

            return $"Total:{TotalScore} = Material:{MaterialScore} + Almost:{AlmostScore} + Safe:{SafeMovesScore} + TwoSided:{TwoSidedScore}";
        }
    }
}

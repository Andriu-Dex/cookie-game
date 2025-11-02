using Juego_Galleta.Domain.Entities;
using Juego_Galleta.Domain.Interfaces;

namespace Juego_Galleta.Application.AI;

/// <summary>
/// Implements the Minimax algorithm with Alpha-Beta pruning for the Galleta game.
/// This is the core AI decision-making algorithm that searches the game tree
/// to find the best move.
/// 
/// Key features:
/// - Alpha-beta pruning for efficiency
/// - Move ordering (captures -> safe -> dangerous) for better pruning
/// - Handles extra turns when cells are captured
/// - Variable depth search
/// </summary>
public sealed class MinimaxAlphaBeta : ISearchStrategy
{
    private readonly IEvaluator _evaluator;
    private int _nodesExplored;

    public MinimaxAlphaBeta(IEvaluator evaluator)
    {
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    /// <summary>
    /// Gets the number of nodes explored in the last search.
    /// Useful for performance analysis.
    /// </summary>
    public int NodesExplored => _nodesExplored;

    /// <summary>
    /// Finds the best move for the current player using Minimax with Alpha-Beta pruning.
    /// </summary>
    public Move GetBestMove(GameState state, int maximizingPlayer, int depth)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));
        if (maximizingPlayer < 0 || maximizingPlayer > 1)
            throw new ArgumentException("Maximizing player must be 0 or 1.", nameof(maximizingPlayer));
        if (depth < 1)
            throw new ArgumentException("Depth must be at least 1.", nameof(depth));

        _nodesExplored = 0;

        Move bestMove = default;
        int bestScore = int.MinValue;
        int alpha = int.MinValue + 1;
        int beta = int.MaxValue - 1;

        var orderedMoves = OrderMoves(state, state.GenerateMoves());

        foreach (var move in orderedMoves)
        {
            var result = state.Apply(move);
            
            // Calculate the score for this move
            // Note: we negate because we're looking at opponent's perspective after our move
            int score = -AlphaBeta(state, 1 - maximizingPlayer, GetNextDepth(depth, result), -beta, -alpha);
            
            state.Undo(result);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }

            alpha = Math.Max(alpha, score);
        }

        return bestMove;
    }

    /// <summary>
    /// Recursive Minimax with Alpha-Beta pruning (Negamax variant).
    /// </summary>
    /// <param name="state">Current game state.</param>
    /// <param name="maximizingPlayer">Player to maximize.</param>
    /// <param name="depth">Remaining search depth.</param>
    /// <param name="alpha">Alpha value for pruning.</param>
    /// <param name="beta">Beta value for pruning.</param>
    /// <returns>The evaluation score for this position.</returns>
    private int AlphaBeta(GameState state, int maximizingPlayer, int depth, int alpha, int beta)
    {
        _nodesExplored++;

        // Terminal conditions: game over or depth limit reached
        if (state.IsTerminal() || depth <= 0)
        {
            return _evaluator.Evaluate(state, maximizingPlayer);
        }

        int bestScore = int.MinValue;
        var orderedMoves = OrderMoves(state, state.GenerateMoves());

        foreach (var move in orderedMoves)
        {
            var result = state.Apply(move);
            
            // Recursively search
            // Negamax formulation: we negate the score and swap alpha/beta
            int score = -AlphaBeta(state, 1 - maximizingPlayer, GetNextDepth(depth, result), -beta, -alpha);
            
            state.Undo(result);

            bestScore = Math.Max(bestScore, score);
            alpha = Math.Max(alpha, score);

            // Beta cutoff (pruning)
            if (alpha >= beta)
            {
                break; // Prune remaining moves
            }
        }

        return bestScore;
    }

    /// <summary>
    /// Determines the next search depth.
    /// If cells were captured (extra turn), we don't reduce depth as much,
    /// allowing the algorithm to "see" longer capture chains.
    /// </summary>
    /// <param name="currentDepth">Current remaining depth.</param>
    /// <param name="result">Result of the last move applied.</param>
    /// <returns>The depth for the next search level.</returns>
    private int GetNextDepth(int currentDepth, AppliedResult result)
    {
        // If cells were captured, the same player continues
        // We still reduce depth, but this allows seeing capture chains better
        // Alternative: return currentDepth if result.CapturedCount > 0 (don't reduce depth)
        return currentDepth - 1;
    }

    /// <summary>
    /// Orders moves to improve alpha-beta pruning efficiency.
    /// Priority:
    /// 1. Capturing moves (complete cells)
    /// 2. Safe moves (don't create 3-sided cells)
    /// 3. Dangerous moves (create opportunities for opponent)
    /// </summary>
    private IEnumerable<Move> OrderMoves(GameState state, IEnumerable<Move> moves)
    {
        var capturing = new List<Move>();
        var safe = new List<Move>();
        var dangerous = new List<Move>();

        foreach (var move in moves)
        {
            bool completesCell = false;
            bool createsThirdSide = false;

            int[] affectedCells = state.Board.EdgesToCells[move.EdgeId];

            foreach (int cellId in affectedCells)
            {
                if (state.CellsOwned[cellId])
                    continue;

                int sides = state.CountDrawnEdges(cellId);

                if (sides == 3)
                {
                    completesCell = true;
                }
                else if (sides == 2)
                {
                    createsThirdSide = true;
                }
            }

            if (completesCell)
            {
                capturing.Add(move);
            }
            else if (!createsThirdSide)
            {
                safe.Add(move);
            }
            else
            {
                dangerous.Add(move);
            }
        }

        // Return in priority order
        foreach (var move in capturing)
            yield return move;
        foreach (var move in safe)
            yield return move;
        foreach (var move in dangerous)
            yield return move;
    }

    /// <summary>
    /// Gets statistics about the last search performed.
    /// </summary>
    public SearchStatistics GetLastSearchStatistics()
    {
        return new SearchStatistics
        {
            NodesExplored = _nodesExplored
        };
    }

    /// <summary>
    /// Statistics about a search operation.
    /// </summary>
    public struct SearchStatistics
    {
        public int NodesExplored { get; set; }

        public override string ToString() => $"Nodes explored: {NodesExplored}";
    }
}

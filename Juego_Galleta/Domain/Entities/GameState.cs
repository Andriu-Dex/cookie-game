using System.Collections;

namespace Juego_Galleta.Domain.Entities;

/// <summary>
/// Represents the current state of the game.
/// Mutable class that supports applying and undoing moves (Command pattern).
/// Follows SRP: responsible only for game state management.
/// </summary>
public sealed class GameState
{
    /// <summary>
    /// The board structure (immutable).
    /// </summary>
    public Board Board { get; }

    /// <summary>
    /// BitArray tracking which edges have been drawn.
    /// True if edge is taken, false otherwise.
    /// </summary>
    public BitArray EdgesTaken { get; }

    /// <summary>
    /// BitArray tracking which cells have been captured.
    /// True if cell is owned, false otherwise.
    /// </summary>
    public BitArray CellsOwned { get; }

    /// <summary>
    /// Tracks which player owns each cell.
    /// -1 means not owned, 0 or 1 indicates the player.
    /// </summary>
    public int[] CellOwners { get; }

    /// <summary>
    /// Score for each player (index 0 and 1).
    /// </summary>
    public int[] Scores { get; }

    /// <summary>
    /// Current player (0 or 1).
    /// </summary>
    public int CurrentPlayer { get; private set; }

    /// <summary>
    /// Number of edges remaining to be drawn.
    /// </summary>
    public int RemainingEdges { get; private set; }

    public GameState(Board board, int startingPlayer = 0)
    {
        if (board == null)
            throw new ArgumentNullException(nameof(board));
        if (startingPlayer < 0 || startingPlayer > 1)
            throw new ArgumentException("Starting player must be 0 or 1.", nameof(startingPlayer));

        Board = board;
        EdgesTaken = new BitArray(board.Edges.Count, false);
        CellsOwned = new BitArray(board.Cells.Count, false);
        CellOwners = Enumerable.Repeat(-1, board.Cells.Count).ToArray();
        Scores = new int[2];
        CurrentPlayer = startingPlayer;
        RemainingEdges = board.Edges.Count;
    }

    /// <summary>
    /// Checks if the game has ended (all edges are drawn).
    /// </summary>
    public bool IsTerminal() => RemainingEdges == 0;

    /// <summary>
    /// Generates all legal moves from the current state.
    /// A move is legal if the edge hasn't been drawn yet.
    /// </summary>
    public IEnumerable<Move> GenerateMoves()
    {
        for (int i = 0; i < Board.Edges.Count; i++)
        {
            if (!EdgesTaken[i])
            {
                yield return new Move(i);
            }
        }
    }

    /// <summary>
    /// Applies a move to the game state.
    /// Updates edges, checks for completed cells, updates scores,
    /// and determines if the player gets another turn.
    /// Returns AppliedResult for potential undo operation.
    /// </summary>
    public AppliedResult Apply(Move move)
    {
        if (move.EdgeId < 0 || move.EdgeId >= Board.Edges.Count)
            throw new ArgumentException($"Invalid edge ID: {move.EdgeId}", nameof(move));

        if (EdgesTaken[move.EdgeId])
            throw new InvalidOperationException($"Edge {move.EdgeId} has already been drawn.");

        // Mark the edge as taken
        EdgesTaken[move.EdgeId] = true;
        RemainingEdges--;

        // Check which cells this edge completes
        var capturedCells = new List<int>();
        int[] affectedCells = Board.EdgesToCells[move.EdgeId];

        foreach (int cellId in affectedCells)
        {
            // Skip if cell is already owned
            if (CellsOwned[cellId])
                continue;

            // Count how many edges of this cell are drawn
            int[] cellEdgeIds = Board.CellEdges[cellId];
            int drawnEdges = 0;

            foreach (int edgeId in cellEdgeIds)
            {
                if (EdgesTaken[edgeId])
                    drawnEdges++;
            }

            // If all 4 edges are drawn, the cell is captured
            if (drawnEdges == 4)
            {
                CellsOwned[cellId] = true;
                CellOwners[cellId] = CurrentPlayer;
                Scores[CurrentPlayer]++;
                capturedCells.Add(cellId);
            }
        }

        int capturedCount = capturedCells.Count;
        var result = new AppliedResult(move, capturedCount, capturedCells.ToArray(), CurrentPlayer);

        // If no cells were captured, switch to the other player
        // If cells were captured, the same player continues (extra turn rule)
        if (capturedCount == 0)
        {
            CurrentPlayer = 1 - CurrentPlayer;
        }

        return result;
    }

    /// <summary>
    /// Undoes a previously applied move.
    /// Restores the state to before the move was made.
    /// Uses the AppliedResult to know what to undo.
    /// </summary>
    public void Undo(AppliedResult result)
    {
        // Restore the edge
        EdgesTaken[result.Move.EdgeId] = false;
        RemainingEdges++;

        // Restore captured cells
        foreach (int cellId in result.CapturedCellIds)
        {
            CellsOwned[cellId] = false;
            CellOwners[cellId] = -1;
            Scores[result.Player]--;
        }

        // Restore the current player
        // If cells were captured, the player didn't change, so restore to the same player
        // If no cells were captured, we switched players, so switch back
        if (result.CapturedCount == 0)
        {
            CurrentPlayer = result.Player;
        }
        // else: CurrentPlayer is already correct (same as result.Player)
    }

    /// <summary>
    /// Gets the winner of the game.
    /// Returns 0 or 1 for the winning player, or -1 for a tie.
    /// Should only be called when IsTerminal() is true.
    /// </summary>
    public int GetWinner()
    {
        if (!IsTerminal())
            throw new InvalidOperationException("Cannot determine winner before game ends.");

        if (Scores[0] > Scores[1])
            return 0;
        if (Scores[1] > Scores[0])
            return 1;
        return -1; // Tie
    }

    /// <summary>
    /// Counts how many edges of a cell are already drawn.
    /// </summary>
    public int CountDrawnEdges(int cellId)
    {
        if (cellId < 0 || cellId >= Board.Cells.Count)
            throw new ArgumentException($"Invalid cell ID: {cellId}", nameof(cellId));

        int count = 0;
        int[] cellEdgeIds = Board.CellEdges[cellId];

        foreach (int edgeId in cellEdgeIds)
        {
            if (EdgesTaken[edgeId])
                count++;
        }

        return count;
    }

    /// <summary>
    /// Creates a deep copy of the current game state.
    /// Useful for AI search algorithms.
    /// </summary>
    public GameState Clone()
    {
        var clone = new GameState(Board, CurrentPlayer)
        {
            RemainingEdges = this.RemainingEdges
        };

        // Copy BitArrays
        for (int i = 0; i < EdgesTaken.Count; i++)
        {
            clone.EdgesTaken[i] = EdgesTaken[i];
        }

        for (int i = 0; i < CellsOwned.Count; i++)
        {
            clone.CellsOwned[i] = CellsOwned[i];
        }

        // Copy arrays
        Array.Copy(CellOwners, clone.CellOwners, CellOwners.Length);
        Array.Copy(Scores, clone.Scores, Scores.Length);

        return clone;
    }

    public override string ToString() => 
        $"GameState: Player {CurrentPlayer}, Scores [{Scores[0]}, {Scores[1]}], Remaining Edges: {RemainingEdges}";
}

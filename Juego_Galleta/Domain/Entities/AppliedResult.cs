namespace Juego_Galleta.Domain.Entities;

/// <summary>
/// Result of applying a move to the game state.
/// Contains all information needed to undo the move (Command pattern).
/// This is an immutable struct.
/// </summary>
public readonly struct AppliedResult
{
    /// <summary>
    /// The move that was applied.
    /// </summary>
    public Move Move { get; }

    /// <summary>
    /// Number of cells captured by this move.
    /// </summary>
    public int CapturedCount { get; }

    /// <summary>
    /// IDs of the cells that were captured by this move.
    /// Can be empty if no cells were completed.
    /// </summary>
    public int[] CapturedCellIds { get; }

    /// <summary>
    /// The player who made this move.
    /// </summary>
    public int Player { get; }

    public AppliedResult(Move move, int capturedCount, int[] capturedCellIds, int player)
    {
        if (capturedCount < 0)
            throw new ArgumentException("Captured count cannot be negative.", nameof(capturedCount));
        if (capturedCellIds == null)
            throw new ArgumentNullException(nameof(capturedCellIds));
        if (capturedCount != capturedCellIds.Length)
            throw new ArgumentException("Captured count must match the number of captured cell IDs.");
        if (player < 0 || player > 1)
            throw new ArgumentException("Player must be 0 or 1.", nameof(player));

        Move = move;
        CapturedCount = capturedCount;
        CapturedCellIds = (int[])capturedCellIds.Clone();
        Player = player;
    }

    public override string ToString() => 
        CapturedCount > 0 
            ? $"Applied {Move} by Player {Player}: Captured {CapturedCount} cell(s)" 
            : $"Applied {Move} by Player {Player}: No captures";
}

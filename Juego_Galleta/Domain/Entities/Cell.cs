namespace Juego_Galleta.Domain.Entities;

/// <summary>
/// Represents a cell (square/box) on the board.
/// A cell is complete when all four of its edges are drawn.
/// This is an immutable struct.
/// </summary>
public readonly struct Cell : IEquatable<Cell>
{
    /// <summary>
    /// Unique identifier for this cell.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Array of edge IDs that form the four sides of this cell.
    /// Always contains exactly 4 edge IDs (top, right, bottom, left).
    /// </summary>
    public int[] EdgeIds { get; }

    public Cell(int id, int[] edgeIds)
    {
        if (edgeIds == null || edgeIds.Length != 4)
            throw new ArgumentException("A cell must have exactly 4 edges.", nameof(edgeIds));

        Id = id;
        // Create a copy to ensure immutability
        EdgeIds = (int[])edgeIds.Clone();
    }

    public bool Equals(Cell other) => Id == other.Id;

    public override bool Equals(object? obj) => obj is Cell cell && Equals(cell);

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() => $"Cell {Id}: [{string.Join(", ", EdgeIds)}]";

    public static bool operator ==(Cell left, Cell right) => left.Equals(right);

    public static bool operator !=(Cell left, Cell right) => !left.Equals(right);
}

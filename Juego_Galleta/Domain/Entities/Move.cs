namespace Juego_Galleta.Domain.Entities;

/// <summary>
/// Represents a move in the game: drawing a specific edge.
/// This is an immutable struct.
/// </summary>
public readonly struct Move : IEquatable<Move>
{
    /// <summary>
    /// The ID of the edge to be drawn.
    /// </summary>
    public int EdgeId { get; }

    public Move(int edgeId)
    {
        if (edgeId < 0)
            throw new ArgumentException("Edge ID must be non-negative.", nameof(edgeId));

        EdgeId = edgeId;
    }

    public bool Equals(Move other) => EdgeId == other.EdgeId;

    public override bool Equals(object? obj) => obj is Move move && Equals(move);

    public override int GetHashCode() => EdgeId.GetHashCode();

    public override string ToString() => $"Move: Edge {EdgeId}";

    public static bool operator ==(Move left, Move right) => left.Equals(right);

    public static bool operator !=(Move left, Move right) => !left.Equals(right);
}

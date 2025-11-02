namespace Juego_Galleta.Domain.Entities;

/// <summary>
/// Represents an edge (line) between two vertices on the board.
/// This is an immutable struct representing a possible connection between two points.
/// </summary>
public readonly struct Edge : IEquatable<Edge>
{
    /// <summary>
    /// Unique identifier for this edge.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// First vertex index.
    /// </summary>
    public int VertexA { get; }

    /// <summary>
    /// Second vertex index.
    /// </summary>
    public int VertexB { get; }

    /// <summary>
    /// Indicates whether this edge is horizontal (true) or vertical (false).
    /// </summary>
    public bool IsHorizontal { get; }

    public Edge(int id, int vertexA, int vertexB, bool isHorizontal)
    {
        if (vertexA == vertexB)
            throw new ArgumentException("An edge must connect two different vertices.");

        Id = id;
        VertexA = vertexA;
        VertexB = vertexB;
        IsHorizontal = isHorizontal;
    }

    public bool Equals(Edge other) => Id == other.Id;

    public override bool Equals(object? obj) => obj is Edge edge && Equals(edge);

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() => $"Edge {Id}: {VertexA} -> {VertexB} ({(IsHorizontal ? "H" : "V")})";

    public static bool operator ==(Edge left, Edge right) => left.Equals(right);

    public static bool operator !=(Edge left, Edge right) => !left.Equals(right);
}

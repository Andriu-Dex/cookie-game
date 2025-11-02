namespace Juego_Galleta.Domain.Entities;

/// <summary>
/// Represents a 2D point with integer coordinates.
/// Used for board generation to define vertex positions.
/// </summary>
public readonly struct Point2D : IEquatable<Point2D>
{
    public int X { get; }
    public int Y { get; }

    public Point2D(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Calculates the Manhattan distance from this point to another.
    /// </summary>
    public int ManhattanDistance(Point2D other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

    public bool Equals(Point2D other) => X == other.X && Y == other.Y;

    public override bool Equals(object? obj) => obj is Point2D point && Equals(point);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public override string ToString() => $"({X}, {Y})";

    public static bool operator ==(Point2D left, Point2D right) => left.Equals(right);

    public static bool operator !=(Point2D left, Point2D right) => !left.Equals(right);
}

using Juego_Galleta.Domain.Interfaces;

namespace Juego_Galleta.Domain.Entities;

/// <summary>
/// Factory for creating "Galleta" (cookie/diamond) shaped boards.
/// Implements the Factory Method pattern.
/// The board is created as a diamond shape using Manhattan distance.
/// </summary>
public sealed class GalletaShapeFactory : IBoardShape
{
    private readonly int _radius;

    /// <summary>
    /// Creates a factory for generating a galleta-shaped board.
    /// </summary>
    /// <param name="radius">The radius of the diamond shape (minimum 2).</param>
    public GalletaShapeFactory(int radius)
    {
        if (radius < 2)
            throw new ArgumentException("Radius must be at least 2 to create a valid board.", nameof(radius));

        _radius = radius;
    }

    /// <summary>
    /// Builds the board with a diamond (galleta) shape.
    /// Points are included if their Manhattan distance from center is <= radius.
    /// </summary>
    public Board Build()
    {
        // Step 1: Generate valid points (vertices) within the diamond shape
        var validPoints = GenerateValidPoints();

        // Step 2: Create a mapping from Point2D to vertex index
        var pointToIndex = new Dictionary<Point2D, int>();
        for (int i = 0; i < validPoints.Count; i++)
        {
            pointToIndex[validPoints[i]] = i;
        }

        // Step 3: Generate edges (horizontal and vertical connections between adjacent valid points)
        var edges = GenerateEdges(validPoints, pointToIndex);

        // Step 4: Generate cells (squares formed by 4 edges)
        var cells = GenerateCells(validPoints, pointToIndex, edges);

        // Step 5: Create and return the board
        return new Board(validPoints.Count, edges, cells);
    }

    /// <summary>
    /// Generates all valid points within the diamond shape.
    /// Creates a proper diamond/cookie shape with an extra row in the middle for the galleta shape.
    /// For radius N, creates a diamond with width/height of 2*N+2 cells (including the extra middle row).
    /// </summary>
    private List<Point2D> GenerateValidPoints()
    {
        var points = new List<Point2D>();
        
        // Generate points row by row - top half (including middle)
        for (int y = -_radius; y <= 0; y++)
        {
            // Calculate how many cells wide this row should be
            int distFromCenter = Math.Abs(y);
            int halfWidth = _radius - distFromCenter;
            
            // Generate points from left edge to right edge of the diamond at this row
            for (int x = -halfWidth; x <= halfWidth + 1; x++)
            {
                points.Add(new Point2D(x, y));
            }
            
            // If this is the widest row (center), add an extra row below it
            if (y == 0)
            {
                for (int x = -halfWidth; x <= halfWidth + 1; x++)
                {
                    points.Add(new Point2D(x, y + 1));
                }
            }
        }
        
        // Generate points row by row - bottom half (starting after the extra row)
        for (int y = 2; y <= _radius + 1; y++)
        {
            // Calculate how many cells wide this row should be
            int distFromCenter = y - 1; // Distance from the extra middle row
            int halfWidth = _radius - distFromCenter;
            
            // Generate points from left edge to right edge of the diamond at this row
            for (int x = -halfWidth; x <= halfWidth + 1; x++)
            {
                points.Add(new Point2D(x, y));
            }
        }
        
        return points;
    }

    /// <summary>
    /// Generates all valid edges (horizontal and vertical lines between adjacent points).
    /// </summary>
    private List<Edge> GenerateEdges(List<Point2D> validPoints, Dictionary<Point2D, int> pointToIndex)
    {
        var edges = new List<Edge>();
        var edgeSet = new HashSet<(int, int)>(); // To avoid duplicate edges
        int edgeId = 0;

        foreach (var point in validPoints)
        {
            int vertexA = pointToIndex[point];

            // Check horizontal neighbor (right)
            var rightNeighbor = new Point2D(point.X + 1, point.Y);
            if (pointToIndex.TryGetValue(rightNeighbor, out int vertexB))
            {
                var edgeKey = (Math.Min(vertexA, vertexB), Math.Max(vertexA, vertexB));
                if (edgeSet.Add(edgeKey))
                {
                    edges.Add(new Edge(edgeId++, vertexA, vertexB, isHorizontal: true));
                }
            }

            // Check vertical neighbor (down)
            var downNeighbor = new Point2D(point.X, point.Y + 1);
            if (pointToIndex.TryGetValue(downNeighbor, out vertexB))
            {
                var edgeKey = (Math.Min(vertexA, vertexB), Math.Max(vertexA, vertexB));
                if (edgeSet.Add(edgeKey))
                {
                    edges.Add(new Edge(edgeId++, vertexA, vertexB, isHorizontal: false));
                }
            }
        }

        return edges;
    }

    /// <summary>
    /// Generates all valid cells (squares) from the edges.
    /// A cell exists if all 4 corners are valid points and all 4 edges exist.
    /// </summary>
    private List<Cell> GenerateCells(List<Point2D> validPoints, Dictionary<Point2D, int> pointToIndex, List<Edge> edges)
    {
        var cells = new List<Cell>();
        var edgeLookup = BuildEdgeLookup(edges);
        int cellId = 0;

        // For each point, try to create a cell with it as the top-left corner
        foreach (var topLeft in validPoints)
        {
            var topRight = new Point2D(topLeft.X + 1, topLeft.Y);
            var bottomLeft = new Point2D(topLeft.X, topLeft.Y + 1);
            var bottomRight = new Point2D(topLeft.X + 1, topLeft.Y + 1);

            // Check if all 4 corners exist
            if (!pointToIndex.ContainsKey(topLeft) ||
                !pointToIndex.ContainsKey(topRight) ||
                !pointToIndex.ContainsKey(bottomLeft) ||
                !pointToIndex.ContainsKey(bottomRight))
            {
                continue;
            }

            int v1 = pointToIndex[topLeft];
            int v2 = pointToIndex[topRight];
            int v3 = pointToIndex[bottomLeft];
            int v4 = pointToIndex[bottomRight];

            // Find the 4 edges that form this cell
            int? topEdge = FindEdge(edgeLookup, v1, v2);
            int? rightEdge = FindEdge(edgeLookup, v2, v4);
            int? bottomEdge = FindEdge(edgeLookup, v3, v4);
            int? leftEdge = FindEdge(edgeLookup, v1, v3);

            // If all 4 edges exist, create the cell
            if (topEdge.HasValue && rightEdge.HasValue && bottomEdge.HasValue && leftEdge.HasValue)
            {
                var cellEdges = new[] { topEdge.Value, rightEdge.Value, bottomEdge.Value, leftEdge.Value };
                cells.Add(new Cell(cellId++, cellEdges));
            }
        }

        return cells;
    }

    /// <summary>
    /// Builds a lookup dictionary for quick edge finding.
    /// Maps (vertexA, vertexB) -> edgeId.
    /// </summary>
    private Dictionary<(int, int), int> BuildEdgeLookup(List<Edge> edges)
    {
        var lookup = new Dictionary<(int, int), int>();

        foreach (var edge in edges)
        {
            int min = Math.Min(edge.VertexA, edge.VertexB);
            int max = Math.Max(edge.VertexA, edge.VertexB);
            lookup[(min, max)] = edge.Id;
        }

        return lookup;
    }

    /// <summary>
    /// Finds an edge connecting two vertices.
    /// </summary>
    private int? FindEdge(Dictionary<(int, int), int> edgeLookup, int v1, int v2)
    {
        int min = Math.Min(v1, v2);
        int max = Math.Max(v1, v2);

        return edgeLookup.TryGetValue((min, max), out int edgeId) ? edgeId : null;
    }

    /// <summary>
    /// Gets information about the board that will be created.
    /// </summary>
    public string GetBoardInfo()
    {
        var points = GenerateValidPoints();
        return $"Galleta Board (radius={_radius}): ~{points.Count} vertices, complexity level: {GetComplexityLevel()}";
    }

    /// <summary>
    /// Returns a descriptive complexity level based on radius.
    /// </summary>
    private string GetComplexityLevel()
    {
        return _radius switch
        {
            2 => "Muy Fácil (Tutorial)",
            3 => "Fácil",
            4 => "Medio",
            5 => "Difícil",
            >= 6 => "Muy Difícil",
            _ => "Personalizado"
        };
    }
}

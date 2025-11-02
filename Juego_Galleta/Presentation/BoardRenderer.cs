using Juego_Galleta.Domain.Entities;

namespace Juego_Galleta.Presentation;

/// <summary>
/// Handles the visual representation of the game board in the console.
/// Responsible for rendering the diamond-shaped board with edges and cells.
/// </summary>
public sealed class BoardRenderer
{
    private readonly Board _board;
    private readonly Dictionary<int, Point2D> _vertexPositions;
    private readonly int _radius;

    public BoardRenderer(Board board, int radius)
    {
        _board = board ?? throw new ArgumentNullException(nameof(board));
        _radius = radius;
        _vertexPositions = new Dictionary<int, Point2D>();
        BuildVertexPositions();
    }

    /// <summary>
    /// Builds the mapping of vertex IDs to their 2D positions.
    /// </summary>
    private void BuildVertexPositions()
    {
        var points = new List<Point2D>();
        var center = new Point2D(0, 0);

        // Generate points in diamond shape
        for (int y = -_radius; y <= _radius; y++)
        {
            for (int x = -_radius; x <= _radius; x++)
            {
                var point = new Point2D(x, y);
                if (point.ManhattanDistance(center) <= _radius)
                {
                    points.Add(point);
                }
            }
        }

        // Sort for consistent ordering
        points.Sort((a, b) =>
        {
            int yCompare = a.Y.CompareTo(b.Y);
            return yCompare != 0 ? yCompare : a.X.CompareTo(b.X);
        });

        for (int i = 0; i < points.Count; i++)
        {
            _vertexPositions[i] = points[i];
        }
    }

    /// <summary>
    /// Renders the complete board with current game state.
    /// </summary>
    public void Render(GameState state)
    {
        Console.Clear();
        RenderHeader(state);
        RenderBoard(state);
        RenderLegend();
    }

    private void RenderHeader(GameState state)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              🍪 JUEGO DE LA GALLETA 🍪                         ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.WriteLine();
        Console.Write("  Jugador 1: ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"█ {state.Scores[0]} celdas");
        Console.ResetColor();

        Console.Write("        Jugador 2: ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"█ {state.Scores[1]} celdas");
        Console.ResetColor();

        Console.Write("  Turno actual: ");
        Console.ForegroundColor = state.CurrentPlayer == 0 ? ConsoleColor.Red : ConsoleColor.Blue;
        Console.WriteLine($"Jugador {state.CurrentPlayer + 1}");
        Console.ResetColor();

        Console.WriteLine();
    }

    private void RenderBoard(GameState state)
    {
        int minX = _vertexPositions.Values.Min(p => p.X);
        int maxX = _vertexPositions.Values.Max(p => p.X);
        int minY = _vertexPositions.Values.Min(p => p.Y);
        int maxY = _vertexPositions.Values.Max(p => p.Y);

        const int scale = 4; // Spacing between points

        for (int y = minY; y <= maxY; y++)
        {
            // Draw horizontal edges and vertices
            for (int row = 0; row < 2; row++)
            {
                Console.Write("  ");

                for (int x = minX; x <= maxX; x++)
                {
                    var point = new Point2D(x, y);
                    int? vertexId = GetVertexIdAt(point);

                    if (vertexId.HasValue)
                    {
                        if (row == 0)
                        {
                            // Draw vertex
                            DrawVertex(vertexId.Value, state);

                            // Draw horizontal edge to the right
                            var rightPoint = new Point2D(x + 1, y);
                            int? rightVertexId = GetVertexIdAt(rightPoint);
                            if (rightVertexId.HasValue)
                            {
                                int? edgeId = FindEdgeBetween(vertexId.Value, rightVertexId.Value);
                                if (edgeId.HasValue)
                                {
                                    DrawHorizontalEdge(edgeId.Value, state);
                                }
                                else
                                {
                                    Console.Write("   ");
                                }
                            }
                        }
                        else
                        {
                            // Draw vertical edge below
                            var downPoint = new Point2D(x, y + 1);
                            int? downVertexId = GetVertexIdAt(downPoint);
                            if (downVertexId.HasValue)
                            {
                                int? edgeId = FindEdgeBetween(vertexId.Value, downVertexId.Value);
                                if (edgeId.HasValue)
                                {
                                    DrawVerticalEdge(edgeId.Value, state);
                                }
                                else
                                {
                                    Console.Write(" ");
                                }
                            }
                            else
                            {
                                Console.Write(" ");
                            }

                            // Draw cell if exists
                            var cellId = GetCellAt(point);
                            if (cellId.HasValue)
                            {
                                DrawCell(cellId.Value, state);
                            }
                            else
                            {
                                Console.Write("  ");
                            }
                        }
                    }
                    else
                    {
                        if (row == 0)
                            Console.Write("    ");
                        else
                            Console.Write("   ");
                    }
                }
                Console.WriteLine();
            }
        }
    }

    private void DrawVertex(int vertexId, GameState state)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("●");
        Console.ResetColor();
    }

    private void DrawHorizontalEdge(int edgeId, GameState state)
    {
        if (state.EdgesTaken[edgeId])
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("━━━");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"{edgeId,2} ");
        }
        Console.ResetColor();
    }

    private void DrawVerticalEdge(int edgeId, GameState state)
    {
        if (state.EdgesTaken[edgeId])
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("┃");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"{edgeId,1}");
        }
        Console.ResetColor();
    }

    private void DrawCell(int cellId, GameState state)
    {
        if (state.CellsOwned[cellId])
        {
            int owner = state.CellOwners[cellId];
            Console.ForegroundColor = owner == 0 ? ConsoleColor.Red : ConsoleColor.Blue;
            Console.Write("█");
        }
        else
        {
            Console.Write(" ");
        }
        Console.ResetColor();
    }

    private void RenderLegend()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Leyenda:");
        Console.Write("  ● = Punto   ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("━━ / ┃ = Línea dibujada   ");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("12 = Número de línea   ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("█ = Celda J1   ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("█ = Celda J2");
        Console.ResetColor();
    }

    private int? GetVertexIdAt(Point2D point)
    {
        foreach (var kvp in _vertexPositions)
        {
            if (kvp.Value.Equals(point))
                return kvp.Key;
        }
        return null;
    }

    private int? FindEdgeBetween(int v1, int v2)
    {
        foreach (var edge in _board.Edges)
        {
            if ((edge.VertexA == v1 && edge.VertexB == v2) ||
                (edge.VertexA == v2 && edge.VertexB == v1))
            {
                return edge.Id;
            }
        }
        return null;
    }

    private int? GetCellAt(Point2D topLeft)
    {
        var topRight = new Point2D(topLeft.X + 1, topLeft.Y);
        var bottomLeft = new Point2D(topLeft.X, topLeft.Y + 1);
        var bottomRight = new Point2D(topLeft.X + 1, topLeft.Y + 1);

        var v1 = GetVertexIdAt(topLeft);
        var v2 = GetVertexIdAt(topRight);
        var v3 = GetVertexIdAt(bottomLeft);
        var v4 = GetVertexIdAt(bottomRight);

        if (!v1.HasValue || !v2.HasValue || !v3.HasValue || !v4.HasValue)
            return null;

        // Find cell that has all 4 vertices
        foreach (var cell in _board.Cells)
        {
            var edgeIds = cell.EdgeIds;
            var vertices = new HashSet<int>();

            foreach (var edgeId in edgeIds)
            {
                var edge = _board.Edges[edgeId];
                vertices.Add(edge.VertexA);
                vertices.Add(edge.VertexB);
            }

            if (vertices.Contains(v1.Value) && vertices.Contains(v2.Value) &&
                vertices.Contains(v3.Value) && vertices.Contains(v4.Value))
            {
                return cell.Id;
            }
        }

        return null;
    }
}

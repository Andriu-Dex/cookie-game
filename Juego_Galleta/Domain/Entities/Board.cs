namespace Juego_Galleta.Domain.Entities;

/// <summary>
/// Represents the game board with its vertices, edges, and cells.
/// This class is immutable once constructed and contains all structural information
/// and precomputed mappings for efficient game state operations.
/// Follows SRP: responsible only for board structure.
/// </summary>
public sealed class Board
{
    /// <summary>
    /// Total number of vertices (points) on the board.
    /// </summary>
    public int VertexCount { get; }

    /// <summary>
    /// Read-only list of all edges (lines) on the board.
    /// </summary>
    public IReadOnlyList<Edge> Edges { get; }

    /// <summary>
    /// Read-only list of all cells (boxes/squares) on the board.
    /// </summary>
    public IReadOnlyList<Cell> Cells { get; }

    /// <summary>
    /// Precomputed mapping: for each edge ID, which cells it belongs to.
    /// EdgesToCells[edgeId] returns an array of cell IDs that include this edge.
    /// This can be 0, 1, or 2 cells (edges on the border belong to fewer cells).
    /// </summary>
    public int[][] EdgesToCells { get; }

    /// <summary>
    /// Precomputed mapping: for each cell ID, which edges form its boundaries.
    /// CellEdges[cellId] returns an array of 4 edge IDs.
    /// This is redundant with Cell.EdgeIds but kept for performance.
    /// </summary>
    public int[][] CellEdges { get; }

    /// <summary>
    /// Constructor that validates and stores board structure with precomputed mappings.
    /// </summary>
    /// <param name="vertexCount">Total number of vertices on the board</param>
    /// <param name="edges">List of all edges</param>
    /// <param name="cells">List of all cells</param>
    public Board(int vertexCount, IReadOnlyList<Edge> edges, IReadOnlyList<Cell> cells)
    {
        if (vertexCount <= 0)
            throw new ArgumentException("Vertex count must be positive.", nameof(vertexCount));
        if (edges == null || edges.Count == 0)
            throw new ArgumentException("Board must have at least one edge.", nameof(edges));
        if (cells == null)
            throw new ArgumentNullException(nameof(cells));

        VertexCount = vertexCount;
        Edges = edges;
        Cells = cells;

        // Precompute mappings for efficient lookups
        EdgesToCells = BuildEdgesToCellsMapping(edges.Count, cells);
        CellEdges = BuildCellEdgesMapping(cells);

        ValidateBoard();
    }

    /// <summary>
    /// Builds the EdgesToCells mapping.
    /// For each edge, determines which cells contain it.
    /// </summary>
    private static int[][] BuildEdgesToCellsMapping(int edgeCount, IReadOnlyList<Cell> cells)
    {
        // First pass: count how many cells each edge belongs to
        var edgeCellCounts = new int[edgeCount];
        foreach (var cell in cells)
        {
            foreach (var edgeId in cell.EdgeIds)
            {
                edgeCellCounts[edgeId]++;
            }
        }

        // Initialize jagged array
        var edgesToCells = new int[edgeCount][];
        for (int i = 0; i < edgeCount; i++)
        {
            edgesToCells[i] = new int[edgeCellCounts[i]];
        }

        // Second pass: fill in the cell IDs
        var currentIndices = new int[edgeCount];
        foreach (var cell in cells)
        {
            foreach (var edgeId in cell.EdgeIds)
            {
                edgesToCells[edgeId][currentIndices[edgeId]++] = cell.Id;
            }
        }

        return edgesToCells;
    }

    /// <summary>
    /// Builds the CellEdges mapping.
    /// For each cell, stores its 4 edge IDs.
    /// </summary>
    private static int[][] BuildCellEdgesMapping(IReadOnlyList<Cell> cells)
    {
        var cellEdges = new int[cells.Count][];
        for (int i = 0; i < cells.Count; i++)
        {
            cellEdges[i] = (int[])cells[i].EdgeIds.Clone();
        }
        return cellEdges;
    }

    /// <summary>
    /// Validates the board structure to ensure data integrity.
    /// </summary>
    private void ValidateBoard()
    {
        // Validate edge IDs are sequential
        for (int i = 0; i < Edges.Count; i++)
        {
            if (Edges[i].Id != i)
                throw new InvalidOperationException($"Edge at index {i} has incorrect ID {Edges[i].Id}.");
        }

        // Validate cell IDs are sequential
        for (int i = 0; i < Cells.Count; i++)
        {
            if (Cells[i].Id != i)
                throw new InvalidOperationException($"Cell at index {i} has incorrect ID {Cells[i].Id}.");
        }

        // Validate edge references in cells
        foreach (var cell in Cells)
        {
            foreach (var edgeId in cell.EdgeIds)
            {
                if (edgeId < 0 || edgeId >= Edges.Count)
                    throw new InvalidOperationException($"Cell {cell.Id} references invalid edge {edgeId}.");
            }
        }

        // Validate vertex references in edges
        foreach (var edge in Edges)
        {
            if (edge.VertexA < 0 || edge.VertexA >= VertexCount ||
                edge.VertexB < 0 || edge.VertexB >= VertexCount)
            {
                throw new InvalidOperationException($"Edge {edge.Id} references invalid vertices.");
            }
        }
    }

    public override string ToString() =>
        $"Board: {VertexCount} vertices, {Edges.Count} edges, {Cells.Count} cells";
}

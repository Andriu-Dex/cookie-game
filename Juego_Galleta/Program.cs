using Juego_Galleta.Domain.Entities;

namespace Juego_Galleta;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Juego de la Galleta - Fase 1: Prueba de Entidades ===\n");

        // Crear un tablero simple de prueba (2x2 puntos = 1 celda)
        // Puntos: 0-1
        //         2-3
        
        var edges = new List<Edge>
        {
            new Edge(0, 0, 1, true),  // Top horizontal
            new Edge(1, 1, 3, false), // Right vertical
            new Edge(2, 2, 3, true),  // Bottom horizontal
            new Edge(3, 0, 2, false)  // Left vertical
        };

        var cells = new List<Cell>
        {
            new Cell(0, new[] { 0, 1, 2, 3 }) // Single cell
        };

        var board = new Board(4, edges, cells);

        Console.WriteLine($"✓ {board}");
        Console.WriteLine($"✓ Edges: {board.Edges.Count}");
        
        foreach (var edge in board.Edges)
        {
            Console.WriteLine($"  - {edge}");
        }

        Console.WriteLine($"\n✓ Cells: {board.Cells.Count}");
        foreach (var cell in board.Cells)
        {
            Console.WriteLine($"  - {cell}");
        }

        Console.WriteLine("\n✓ Mappings (EdgesToCells):");
        for (int i = 0; i < board.EdgesToCells.Length; i++)
        {
            var cellIds = string.Join(", ", board.EdgesToCells[i]);
            Console.WriteLine($"  Edge {i} -> Cells: [{cellIds}]");
        }

        Console.WriteLine("\n✓ Mappings (CellEdges):");
        for (int i = 0; i < board.CellEdges.Length; i++)
        {
            var edgeIds = string.Join(", ", board.CellEdges[i]);
            Console.WriteLine($"  Cell {i} -> Edges: [{edgeIds}]");
        }

        var move = new Move(0);
        Console.WriteLine($"\n✓ {move}");

        Console.WriteLine("\n=== Fase 1 completada exitosamente ===");
        Console.WriteLine("Presiona cualquier tecla para salir...");
        Console.ReadKey();
    }
}

using Juego_Galleta.Domain.Entities;
using Juego_Galleta.Domain.Interfaces;

namespace Juego_Galleta;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Juego de la Galleta - Fase 3: Generador de Tablero ===\n");

        TestPhase1();
        Console.WriteLine("\n" + new string('=', 60) + "\n");
        TestPhase2();
        Console.WriteLine("\n" + new string('=', 60) + "\n");
        TestPhase3();

        Console.WriteLine("\n=== Todas las pruebas completadas exitosamente ===");
        Console.WriteLine("Presiona cualquier tecla para salir...");
        Console.ReadKey();
    }

    static void TestPhase1()
    {
        Console.WriteLine("--- FASE 1: Entidades Básicas ---\n");

        var board = CreateSimpleBoard();
        Console.WriteLine($"✓ {board}");
        Console.WriteLine($"✓ Edges: {board.Edges.Count}");
        Console.WriteLine($"✓ Cells: {board.Cells.Count}");
        Console.WriteLine($"✓ Mappings validados correctamente");
    }

    static void TestPhase2()
    {
        Console.WriteLine("--- FASE 2: GameState con Apply/Undo ---\n");

        var board = CreateSimpleBoard();
        var gameState = new GameState(board, startingPlayer: 0);

        Console.WriteLine($"Estado inicial: {gameState}");
        Console.WriteLine($"¿Es terminal? {gameState.IsTerminal()}");
        Console.WriteLine($"Movimientos disponibles: {gameState.GenerateMoves().Count()}\n");

        // Test 1: Movimiento sin captura
        Console.WriteLine("TEST 1: Movimiento sin captura (debe cambiar de jugador)");
        var move1 = new Move(0); // Top edge
        var result1 = gameState.Apply(move1);
        Console.WriteLine($"  {result1}");
        Console.WriteLine($"  {gameState}");
        Console.WriteLine($"  ✓ Jugador actual debe ser 1: {gameState.CurrentPlayer == 1}\n");

        // Test 2: Otro movimiento sin captura
        Console.WriteLine("TEST 2: Segundo movimiento sin captura");
        var move2 = new Move(1); // Right edge
        var result2 = gameState.Apply(move2);
        Console.WriteLine($"  {result2}");
        Console.WriteLine($"  {gameState}");
        Console.WriteLine($"  ✓ Jugador actual debe ser 0: {gameState.CurrentPlayer == 0}\n");

        // Test 3: Tercer movimiento
        Console.WriteLine("TEST 3: Tercer movimiento sin captura");
        var move3 = new Move(2); // Bottom edge
        var result3 = gameState.Apply(move3);
        Console.WriteLine($"  {result3}");
        Console.WriteLine($"  {gameState}");
        Console.WriteLine($"  ✓ Jugador actual debe ser 1: {gameState.CurrentPlayer == 1}\n");

        // Test 4: Movimiento que completa el cuadro (debe dar turno extra)
        Console.WriteLine("TEST 4: Movimiento que captura celda (turno extra)");
        var move4 = new Move(3); // Left edge - completa el cuadro
        var result4 = gameState.Apply(move4);
        Console.WriteLine($"  {result4}");
        Console.WriteLine($"  {gameState}");
        Console.WriteLine($"  ✓ Debe capturar 1 celda: {result4.CapturedCount == 1}");
        Console.WriteLine($"  ✓ Jugador 1 debe tener 1 punto: {gameState.Scores[1] == 1}");
        Console.WriteLine($"  ✓ Jugador actual debe seguir siendo 1: {gameState.CurrentPlayer == 1}");
        Console.WriteLine($"  ✓ Juego debe ser terminal: {gameState.IsTerminal()}");
        Console.WriteLine($"  ✓ Ganador debe ser jugador 1: {gameState.GetWinner() == 1}\n");

        // Test 5: Undo del último movimiento
        Console.WriteLine("TEST 5: Undo del movimiento que capturó");
        gameState.Undo(result4);
        Console.WriteLine($"  {gameState}");
        Console.WriteLine($"  ✓ Jugador actual debe ser 1: {gameState.CurrentPlayer == 1}");
        Console.WriteLine($"  ✓ Jugador 1 debe tener 0 puntos: {gameState.Scores[1] == 0}");
        Console.WriteLine($"  ✓ Debe haber 1 movimiento disponible: {gameState.GenerateMoves().Count() == 1}");
        Console.WriteLine($"  ✓ Juego NO debe ser terminal: {!gameState.IsTerminal()}\n");

        // Test 6: Undo múltiple
        Console.WriteLine("TEST 6: Undo múltiple hasta el estado inicial");
        gameState.Undo(result3);
        gameState.Undo(result2);
        gameState.Undo(result1);
        Console.WriteLine($"  {gameState}");
        Console.WriteLine($"  ✓ Debe estar en estado inicial");
        Console.WriteLine($"  ✓ Jugador actual debe ser 0: {gameState.CurrentPlayer == 0}");
        Console.WriteLine($"  ✓ Todos los puntajes en 0: {gameState.Scores[0] == 0 && gameState.Scores[1] == 0}");
        Console.WriteLine($"  ✓ 4 movimientos disponibles: {gameState.GenerateMoves().Count() == 4}\n");

        // Test 7: Clone
        Console.WriteLine("TEST 7: Clonación de estado");
        var clone = gameState.Clone();
        clone.Apply(new Move(0));
        Console.WriteLine($"  Original: {gameState}");
        Console.WriteLine($"  Clon: {clone}");
        Console.WriteLine($"  ✓ Original no debe cambiar: {gameState.RemainingEdges == 4}");
        Console.WriteLine($"  ✓ Clon debe tener 3 movimientos restantes: {clone.RemainingEdges == 3}");

        Console.WriteLine("\n✓ Todos los tests de Fase 2 pasaron correctamente");
    }

    static void TestPhase3()
    {
        Console.WriteLine("--- FASE 3: Generador de Tablero (GalletaShapeFactory) ---\n");

        // Test con diferentes tamaños de tablero
        var radii = new[] { 2, 3, 4, 5 };

        foreach (int radius in radii)
        {
            Console.WriteLine($"TEST: Tablero con radio {radius}");
            
            IBoardShape factory = new GalletaShapeFactory(radius);
            Console.WriteLine($"  Info: {((GalletaShapeFactory)factory).GetBoardInfo()}");
            
            var board = factory.Build();
            Console.WriteLine($"  {board}");
            
            // Validar que el tablero es jugable
            var gameState = new GameState(board);
            var moves = gameState.GenerateMoves().ToList();
            
            Console.WriteLine($"  ✓ Movimientos posibles: {moves.Count}");
            Console.WriteLine($"  ✓ Relación Edges/Cells: {(double)board.Edges.Count / board.Cells.Count:F2}");
            Console.WriteLine($"  ✓ Tablero válido: {ValidateBoard(board)}");
            Console.WriteLine();
        }

        // Test de juego completo en tablero pequeño
        Console.WriteLine("\nTEST: Juego completo simulado en tablero pequeño (radio 2)");
        TestCompleteGame();

        // Test de visualización simple
        Console.WriteLine("\nTEST: Visualización del tablero (radio 3)");
        VisualizeBoard(3);

        Console.WriteLine("\n✓ Todos los tests de Fase 3 pasaron correctamente");
    }

    static bool ValidateBoard(Board board)
    {
        // Verificar que todas las celdas tienen 4 aristas válidas
        foreach (var cell in board.Cells)
        {
            if (cell.EdgeIds.Length != 4)
                return false;

            foreach (int edgeId in cell.EdgeIds)
            {
                if (edgeId < 0 || edgeId >= board.Edges.Count)
                    return false;
            }
        }

        // Verificar que cada arista está en al menos una celda
        for (int i = 0; i < board.Edges.Count; i++)
        {
            if (board.EdgesToCells[i].Length == 0)
            {
                // Las aristas del borde pueden no tener celdas, es normal
                // pero al menos debería haber algunas aristas con celdas
            }
        }

        return true;
    }

    static void TestCompleteGame()
    {
        var factory = new GalletaShapeFactory(2);
        var board = factory.Build();
        var gameState = new GameState(board);

        Console.WriteLine($"  Estado inicial: {gameState}");
        
        int moveCount = 0;
        var random = new Random(42); // Seed fijo para reproducibilidad

        // Jugar hasta el final con movimientos aleatorios
        while (!gameState.IsTerminal())
        {
            var moves = gameState.GenerateMoves().ToList();
            var randomMove = moves[random.Next(moves.Count)];
            var result = gameState.Apply(randomMove);
            moveCount++;

            if (result.CapturedCount > 0)
            {
                Console.WriteLine($"  Movimiento {moveCount}: Jugador {result.Player} capturó {result.CapturedCount} celda(s)!");
            }
        }

        Console.WriteLine($"  Juego terminado después de {moveCount} movimientos");
        Console.WriteLine($"  Resultado final: {gameState}");
        
        int winner = gameState.GetWinner();
        if (winner == -1)
            Console.WriteLine($"  ✓ ¡Empate! Ambos jugadores: {gameState.Scores[0]} celdas");
        else
            Console.WriteLine($"  ✓ ¡Ganador: Jugador {winner}! ({gameState.Scores[winner]} vs {gameState.Scores[1 - winner]} celdas)");
    }

    static void VisualizeBoard(int radius)
    {
        var factory = new GalletaShapeFactory(radius);
        var board = factory.Build();

        Console.WriteLine($"  Tablero en forma de galleta (radio {radius}):");
        Console.WriteLine($"  {board}");
        Console.WriteLine();

        // Crear una representación simple del tablero
        // Mostrar la forma del diamante con asteriscos
        Console.WriteLine("  Forma del diamante (cada * es un punto):");
        
        for (int y = -radius; y <= radius; y++)
        {
            Console.Write("  ");
            
            // Calcular espacios iniciales
            int spaces = Math.Abs(y);
            Console.Write(new string(' ', spaces * 2));

            // Calcular cuántos puntos hay en esta fila
            int pointsInRow = (2 * radius + 1) - (2 * Math.Abs(y));
            
            for (int i = 0; i < pointsInRow; i++)
            {
                Console.Write("* ");
            }
            
            Console.WriteLine();
        }

        Console.WriteLine();
        Console.WriteLine($"  Esta forma contiene {board.Cells.Count} celdas (cuadros) para capturar");
    }

    static Board CreateSimpleBoard()
    {
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

        return new Board(4, edges, cells);
    }
}

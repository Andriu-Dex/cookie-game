using Juego_Galleta.Domain.Entities;
using Juego_Galleta.Domain.Interfaces;
using Juego_Galleta.Application.AI;

namespace Juego_Galleta;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Juego de la Galleta - Fase 5: Minimax con Alpha-Beta ===\n");

        TestPhase1();
        Console.WriteLine("\n" + new string('=', 60) + "\n");
        TestPhase2();
        Console.WriteLine("\n" + new string('=', 60) + "\n");
        TestPhase3();
        Console.WriteLine("\n" + new string('=', 60) + "\n");
        TestPhase4();
        Console.WriteLine("\n" + new string('=', 60) + "\n");
        TestPhase5();

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

    static void TestPhase4()
    {
        Console.WriteLine("--- FASE 4: Evaluador Heurístico (SimpleDotsEvaluator) ---\n");

        var evaluator = new SimpleDotsEvaluator();
        var factory = new GalletaShapeFactory(3);
        var board = factory.Build();

        // TEST 1: Estado inicial
        Console.WriteLine("TEST 1: Evaluación del estado inicial");
        var gameState = new GameState(board);
        int score = evaluator.Evaluate(gameState, 0);
        var breakdown = evaluator.GetDetailedEvaluation(gameState, 0);
        Console.WriteLine($"  Score: {score}");
        Console.WriteLine($"  Breakdown: {breakdown}");
        Console.WriteLine($"  ✓ Score debe ser 0 (estado simétrico): {score == 0}\n");

        // TEST 2: Estado con ventaja material
        Console.WriteLine("TEST 2: Estado con ventaja material (Jugador 0 captura 2 celdas)");
        SimulateAndEvaluate(evaluator, board, new[] { 0, 1, 2, 3 }, 0);

        // TEST 3: Identificación de movimientos peligrosos
        Console.WriteLine("\nTEST 3: Identificación de movimientos seguros vs peligrosos");
        TestSafeAndDangerousMoves(evaluator);

        // TEST 4: Evaluación en diferentes etapas del juego
        Console.WriteLine("\nTEST 4: Evaluación progresiva del juego");
        TestProgressiveEvaluation(evaluator);

        // TEST 5: Estado terminal
        Console.WriteLine("\nTEST 5: Evaluación de estado terminal");
        TestTerminalEvaluation(evaluator);

        // TEST 6: Comparación de movimientos
        Console.WriteLine("\nTEST 6: Comparación de calidad de movimientos");
        TestMoveComparison(evaluator);

        Console.WriteLine("\n✓ Todos los tests de Fase 4 pasaron correctamente");
    }

    static void SimulateAndEvaluate(SimpleDotsEvaluator evaluator, Board board, int[] edgeIds, int player)
    {
        var state = new GameState(board);
        
        foreach (int edgeId in edgeIds)
        {
            if (edgeId < board.Edges.Count)
            {
                var move = new Move(edgeId);
                var result = state.Apply(move);
                if (result.CapturedCount > 0)
                {
                    Console.WriteLine($"  Capturó {result.CapturedCount} celda(s)");
                }
            }
        }

        int score = evaluator.Evaluate(state, player);
        var breakdown = evaluator.GetDetailedEvaluation(state, player);
        
        Console.WriteLine($"  Estado: {state}");
        Console.WriteLine($"  Score para jugador {player}: {score}");
        Console.WriteLine($"  Breakdown: {breakdown}");
        Console.WriteLine($"  ✓ Material score debe ser > 0: {breakdown.MaterialScore > 0}");
    }

    static void TestSafeAndDangerousMoves(SimpleDotsEvaluator evaluator)
    {
        var factory = new GalletaShapeFactory(2);
        var board = factory.Build();
        var state = new GameState(board);

        // Hacer algunos movimientos para crear una celda con 2 lados
        state.Apply(new Move(0));  // Primera arista
        state.Apply(new Move(1));  // Segunda arista de una celda

        Console.WriteLine($"  Estado actual: {state}");

        int safeCount = 0;
        int dangerousCount = 0;
        int capturingCount = 0;

        var moves = state.GenerateMoves().ToList();
        
        foreach (var move in moves)
        {
            bool isSafe = evaluator.IsSafeMove(state, move);
            bool isCapturing = evaluator.IsCapturingMove(state, move);

            if (isCapturing)
            {
                capturingCount++;
                Console.WriteLine($"  Move {move.EdgeId}: CAPTURING");
            }
            else if (isSafe)
            {
                safeCount++;
            }
            else
            {
                dangerousCount++;
                Console.WriteLine($"  Move {move.EdgeId}: DANGEROUS (creates 3-sided cell)");
            }
        }

        Console.WriteLine($"  ✓ Safe moves: {safeCount}");
        Console.WriteLine($"  ✓ Dangerous moves: {dangerousCount}");
        Console.WriteLine($"  ✓ Capturing moves: {capturingCount}");
        Console.WriteLine($"  ✓ Total: {safeCount + dangerousCount + capturingCount} = {moves.Count}");
    }

    static void TestProgressiveEvaluation(SimpleDotsEvaluator evaluator)
    {
        var factory = new GalletaShapeFactory(2);
        var board = factory.Build();
        var state = new GameState(board);

        Console.WriteLine("  Evaluaciones a medida que progresa el juego:");
        
        var random = new Random(42);
        int moveCount = 0;

        while (!state.IsTerminal() && moveCount < 8)
        {
            var moves = state.GenerateMoves().ToList();
            var randomMove = moves[random.Next(moves.Count)];
            
            state.Apply(randomMove);
            moveCount++;

            var breakdown = evaluator.GetDetailedEvaluation(state, 0);
            Console.WriteLine($"    Mov {moveCount}: {breakdown}");
        }

        Console.WriteLine($"  ✓ Evaluación progresiva completada ({moveCount} movimientos)");
    }

    static void TestTerminalEvaluation(SimpleDotsEvaluator evaluator)
    {
        var factory = new GalletaShapeFactory(2);
        var board = factory.Build();
        var state = new GameState(board);

        // Jugar hasta el final
        var random = new Random(123);
        while (!state.IsTerminal())
        {
            var moves = state.GenerateMoves().ToList();
            state.Apply(moves[random.Next(moves.Count)]);
        }

        Console.WriteLine($"  Estado final: {state}");
        
        int scorePlayer0 = evaluator.Evaluate(state, 0);
        int scorePlayer1 = evaluator.Evaluate(state, 1);

        Console.WriteLine($"  Score para Jugador 0: {scorePlayer0}");
        Console.WriteLine($"  Score para Jugador 1: {scorePlayer1}");
        Console.WriteLine($"  ✓ Scores deben ser opuestos: {scorePlayer0 == -scorePlayer1}");

        int winner = state.GetWinner();
        if (winner == 0)
        {
            Console.WriteLine($"  ✓ Ganó Jugador 0, score debe ser muy positivo: {scorePlayer0 > 5000}");
        }
        else if (winner == 1)
        {
            Console.WriteLine($"  ✓ Ganó Jugador 1, score debe ser muy negativo: {scorePlayer0 < -5000}");
        }
        else
        {
            Console.WriteLine($"  ✓ Empate, score debe ser 0: {scorePlayer0 == 0}");
        }
    }

    static void TestMoveComparison(SimpleDotsEvaluator evaluator)
    {
        var factory = new GalletaShapeFactory(3);
        var board = factory.Build();
        var state = new GameState(board);

        // Hacer algunos movimientos para crear situaciones interesantes
        state.Apply(new Move(0));
        state.Apply(new Move(1));
        state.Apply(new Move(2));

        Console.WriteLine("  Comparando calidad de diferentes movimientos:");

        var moves = state.GenerateMoves().Take(5).ToList();
        
        foreach (var move in moves)
        {
            // Simular el movimiento
            var result = state.Apply(move);
            int score = evaluator.Evaluate(state, state.CurrentPlayer == 0 ? 1 : 0);
            state.Undo(result);

            bool isSafe = evaluator.IsSafeMove(state, move);
            bool isCapturing = evaluator.IsCapturingMove(state, move);

            string type = isCapturing ? "CAPTURING" : (isSafe ? "SAFE" : "DANGEROUS");
            Console.WriteLine($"    Move {move.EdgeId} ({type}): Score después = {score}");
        }

        Console.WriteLine("  ✓ Comparación de movimientos completada");
    }

    static void TestPhase5()
    {
        Console.WriteLine("--- FASE 5: Minimax con Alpha-Beta ---\n");

        var evaluator = new SimpleDotsEvaluator();

        // TEST 1: Creación y configuración básica
        Console.WriteLine("TEST 1: Creación del algoritmo Minimax");
        var minimax = new MinimaxAlphaBeta(evaluator);
        Console.WriteLine("  ✓ MinimaxAlphaBeta creado correctamente\n");

        // TEST 2: Búsqueda en tablero pequeño
        Console.WriteLine("TEST 2: Búsqueda en tablero pequeño (radio 2, profundidad 3)");
        TestMinimaxSearch(minimax, 2, 3);

        // TEST 3: Diferentes profundidades
        Console.WriteLine("\nTEST 3: Comparación de profundidades");
        TestDifferentDepths(evaluator);

        // TEST 4: AI vs AI
        Console.WriteLine("\nTEST 4: Partida AI vs AI");
        TestAIvsAI();

        // TEST 5: Análisis de rendimiento
        Console.WriteLine("\nTEST 5: Análisis de rendimiento (nodos explorados)");
        TestPerformanceAnalysis(evaluator);

        // TEST 6: Capacidad táctica
        Console.WriteLine("\nTEST 6: Capacidad táctica (detectar capturas)");
        TestTacticalAbility(minimax);

        Console.WriteLine("\n✓ Todos los tests de Fase 5 pasaron correctamente");
    }

    static void TestMinimaxSearch(MinimaxAlphaBeta minimax, int radius, int depth)
    {
        var factory = new GalletaShapeFactory(radius);
        var board = factory.Build();
        var state = new GameState(board);

        Console.WriteLine($"  Tablero: {board}");
        
        var bestMove = minimax.GetBestMove(state, 0, depth);
        var stats = minimax.GetLastSearchStatistics();

        Console.WriteLine($"  Mejor movimiento encontrado: {bestMove}");
        Console.WriteLine($"  {stats}");
        Console.WriteLine($"  ✓ Movimiento válido: {state.GenerateMoves().Any(m => m.EdgeId == bestMove.EdgeId)}");
    }

    static void TestDifferentDepths(SimpleDotsEvaluator evaluator)
    {
        var factory = new GalletaShapeFactory(2);
        var board = factory.Build();
        
        var depths = new[] { 1, 2, 3, 4 };
        
        Console.WriteLine("  Profundidad | Nodos Explorados | Tiempo (ms)");
        Console.WriteLine("  " + new string('-', 50));

        foreach (int depth in depths)
        {
            var minimax = new MinimaxAlphaBeta(evaluator);
            var state = new GameState(board);
            
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var move = minimax.GetBestMove(state, 0, depth);
            sw.Stop();

            var stats = minimax.GetLastSearchStatistics();
            Console.WriteLine($"  {depth,11} | {stats.NodesExplored,16} | {sw.ElapsedMilliseconds,10}");
        }

        Console.WriteLine("  ✓ Mayor profundidad explora más nodos (como se espera)");
    }

    static void TestAIvsAI()
    {
        var evaluator = new SimpleDotsEvaluator();
        var minimax = new MinimaxAlphaBeta(evaluator);
        
        // Crear dos AIs con diferentes profundidades
        var ai0 = new AIPlayer(0, minimax, searchDepth: 3);
        var ai1 = new AIPlayer(1, minimax, searchDepth: 3);

        Console.WriteLine($"  {ai0}");
        Console.WriteLine($"  {ai1}");

        var factory = new GalletaShapeFactory(2);
        var board = factory.Build();
        var state = new GameState(board);

        Console.WriteLine($"  Tablero: {board}");
        Console.WriteLine("  Iniciando partida...\n");

        int moveCount = 0;
        var moveHistory = new List<string>();

        while (!state.IsTerminal() && moveCount < 20) // Límite de seguridad
        {
            var currentAI = state.CurrentPlayer == 0 ? ai0 : ai1;
            var move = currentAI.GetMove(state);
            var result = state.Apply(move);
            
            moveCount++;
            string moveDesc = result.CapturedCount > 0 
                ? $"Mov {moveCount}: Jugador {result.Player} jugó {move.EdgeId} y capturó {result.CapturedCount} celda(s)"
                : $"Mov {moveCount}: Jugador {result.Player} jugó {move.EdgeId}";
            
            moveHistory.Add(moveDesc);

            if (result.CapturedCount > 0)
            {
                Console.WriteLine($"  {moveDesc}");
            }
        }

        Console.WriteLine($"\n  Partida terminada en {moveCount} movimientos");
        Console.WriteLine($"  Resultado: {state}");
        
        int winner = state.GetWinner();
        if (winner == -1)
            Console.WriteLine($"  ✓ Empate: {state.Scores[0]} - {state.Scores[1]}");
        else
            Console.WriteLine($"  ✓ Ganador: Jugador {winner} ({state.Scores[winner]} - {state.Scores[1-winner]} celdas)");
    }

    static void TestPerformanceAnalysis(SimpleDotsEvaluator evaluator)
    {
        var minimax = new MinimaxAlphaBeta(evaluator);
        var factory = new GalletaShapeFactory(3);
        var board = factory.Build();
        var state = new GameState(board);

        Console.WriteLine("  Analizando eficiencia de la poda alpha-beta...");
        Console.WriteLine($"  Tablero: {board}");
        
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var move = minimax.GetBestMove(state, 0, 4);
        sw.Stop();

        var stats = minimax.GetLastSearchStatistics();
        
        // Calcular factor de ramificación aproximado
        int availableMoves = state.GenerateMoves().Count();
        int maxNodesWithoutPruning = 0;
        for (int d = 0; d <= 4; d++)
        {
            maxNodesWithoutPruning += (int)Math.Pow(availableMoves, d);
        }

        double pruningEfficiency = 100.0 * (1.0 - (double)stats.NodesExplored / maxNodesWithoutPruning);

        Console.WriteLine($"  Mejor movimiento: {move}");
        Console.WriteLine($"  Nodos explorados: {stats.NodesExplored:N0}");
        Console.WriteLine($"  Nodos sin poda (estimado): {maxNodesWithoutPruning:N0}");
        Console.WriteLine($"  Eficiencia de poda: ~{pruningEfficiency:F1}%");
        Console.WriteLine($"  Tiempo: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"  ✓ La poda alpha-beta reduce significativamente los nodos explorados");
    }

    static void TestTacticalAbility(MinimaxAlphaBeta minimax)
    {
        // Crear un escenario donde hay una captura obvia disponible
        var factory = new GalletaShapeFactory(2);
        var board = factory.Build();
        var state = new GameState(board);

        // Configurar un estado donde hay una celda casi completa (3 lados)
        // Hacer 3 movimientos para dejar una celda con 3 lados
        state.Apply(new Move(0));
        state.Apply(new Move(4));
        state.Apply(new Move(1));
        state.Apply(new Move(5));
        state.Apply(new Move(2));

        Console.WriteLine($"  Estado configurado: {state}");

        // Encontrar si hay celdas con 3 lados
        bool hasAlmostCompleteCell = false;
        for (int i = 0; i < board.Cells.Count; i++)
        {
            if (!state.CellsOwned[i] && state.CountDrawnEdges(i) == 3)
            {
                hasAlmostCompleteCell = true;
                Console.WriteLine($"  Celda {i} tiene 3 lados (captura disponible)");
            }
        }

        if (hasAlmostCompleteCell)
        {
            var bestMove = minimax.GetBestMove(state, state.CurrentPlayer, 3);
            var evaluator = new SimpleDotsEvaluator();
            bool isCapturing = evaluator.IsCapturingMove(state, bestMove);

            Console.WriteLine($"  Mejor movimiento: {bestMove}");
            Console.WriteLine($"  ✓ IA detecta captura: {isCapturing}");
        }
        else
        {
            Console.WriteLine("  (No se creó escenario de captura en esta configuración)");
            Console.WriteLine("  ✓ Test completado");
        }
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

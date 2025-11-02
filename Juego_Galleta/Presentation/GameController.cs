using Juego_Galleta.Domain.Entities;
using Juego_Galleta.Domain.Interfaces;
using Juego_Galleta.Application.AI;

namespace Juego_Galleta.Presentation;

/// <summary>
/// Main game controller that orchestrates the game flow.
/// Handles user input, game logic, and rendering.
/// </summary>
public sealed class GameController
{
    private GameState? _gameState;
    private Board? _board;
    private BoardRenderer? _renderer;
    private AIPlayer? _aiPlayer;
    private int _radius;
    private int _aiDepth;
    private GameMode _gameMode;

    private enum GameMode
    {
        HumanVsAI,
        AIVsAI,
        HumanVsHuman
    }

    public void Run()
    {
        ShowWelcome();
        
        while (true)
        {
            var choice = ShowMainMenu();

            switch (choice)
            {
                case "1":
                    ConfigureAndStartGame(GameMode.HumanVsAI);
                    break;
                case "2":
                    ConfigureAndStartGame(GameMode.AIVsAI);
                    break;
                case "3":
                    ConfigureAndStartGame(GameMode.HumanVsHuman);
                    break;
                case "4":
                    ShowInstructions();
                    break;
                case "5":
                    Console.WriteLine("\n¡Gracias por jugar! ¡Hasta pronto! 👋");
                    return;
                default:
                    Console.WriteLine("\nOpción inválida. Presione cualquier tecla...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private void ShowWelcome()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                                ║");
        Console.WriteLine("║              🍪 JUEGO DE LA GALLETA 🍪                         ║");
        Console.WriteLine("║                                                                ║");
        Console.WriteLine("║           Inteligencia Artificial con Minimax                 ║");
        Console.WriteLine("║                  Poda Alpha-Beta                               ║");
        Console.WriteLine("║                                                                ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        Console.ReadKey();
    }

    private string ShowMainMenu()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("═══════════════════ MENÚ PRINCIPAL ═══════════════════");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("  1. 🎮 Humano vs IA");
        Console.WriteLine("  2. 🤖 IA vs IA");
        Console.WriteLine("  3. 👥 Humano vs Humano");
        Console.WriteLine("  4. 📖 Ver instrucciones");
        Console.WriteLine("  5. 🚪 Salir");
        Console.WriteLine();
        Console.Write("Seleccione una opción: ");
        return Console.ReadLine()?.Trim() ?? "";
    }

    private void ConfigureAndStartGame(GameMode mode)
    {
        _gameMode = mode;

        // Configurar tamaño del tablero
        Console.Clear();
        Console.WriteLine("═══════════════ CONFIGURACIÓN DEL JUEGO ═══════════════");
        Console.WriteLine();
        Console.WriteLine("Seleccione el tamaño del tablero:");
        Console.WriteLine("  1. Muy Fácil (Radio 2 - 4 celdas)");
        Console.WriteLine("  2. Fácil (Radio 3 - 12 celdas)");
        Console.WriteLine("  3. Medio (Radio 4 - 24 celdas)");
        Console.WriteLine("  4. Difícil (Radio 5 - 40 celdas)");
        Console.Write("\nOpción: ");
        
        var sizeChoice = Console.ReadLine()?.Trim();
        _radius = sizeChoice switch
        {
            "1" => 2,
            "2" => 3,
            "3" => 4,
            "4" => 5,
            _ => 3
        };

        // Configurar dificultad de la IA si aplica
        if (mode == GameMode.HumanVsAI || mode == GameMode.AIVsAI)
        {
            Console.WriteLine();
            Console.WriteLine("Seleccione la dificultad de la IA:");
            Console.WriteLine("  1. Fácil (Profundidad 2)");
            Console.WriteLine("  2. Normal (Profundidad 3)");
            Console.WriteLine("  3. Difícil (Profundidad 4)");
            Console.WriteLine("  4. Experto (Profundidad 5)");
            Console.Write("\nOpción: ");

            var difficultyChoice = Console.ReadLine()?.Trim();
            _aiDepth = difficultyChoice switch
            {
                "1" => 2,
                "2" => 3,
                "3" => 4,
                "4" => 5,
                _ => 3
            };
        }

        StartGame();
    }

    private void StartGame()
    {
        // Crear tablero
        var factory = new GalletaShapeFactory(_radius);
        _board = factory.Build();
        _gameState = new GameState(_board);
        _renderer = new BoardRenderer(_board, _radius);

        // Crear IA si es necesario
        if (_gameMode == GameMode.HumanVsAI || _gameMode == GameMode.AIVsAI)
        {
            var evaluator = new SimpleDotsEvaluator();
            var minimax = new MinimaxAlphaBeta(evaluator);
            _aiPlayer = new AIPlayer(1, minimax, _aiDepth); // IA juega como jugador 2
        }

        // Bucle principal del juego
        GameLoop();
    }

    private void GameLoop()
    {
        while (!_gameState!.IsTerminal())
        {
            _renderer!.Render(_gameState);

            bool isAITurn = (_gameMode == GameMode.HumanVsAI && _gameState.CurrentPlayer == 1) ||
                           _gameMode == GameMode.AIVsAI;

            if (isAITurn)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  IA está pensando...");
                Console.ResetColor();
                
                System.Threading.Thread.Sleep(500); // Pausa para que se vea la animación

                var aiMove = _aiPlayer!.GetMove(_gameState);
                var result = _gameState.Apply(aiMove);

                if (result.CapturedCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ¡IA capturó {result.CapturedCount} celda(s)! ¡Turno extra!");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(1000);
                }
            }
            else
            {
                // Turno humano
                bool validMove = false;
                while (!validMove)
                {
                    Console.WriteLine();
                    Console.Write("  Ingrese el número de línea a dibujar (o 'q' para rendirse): ");
                    var input = Console.ReadLine()?.Trim().ToLower();

                    if (input == "q")
                    {
                        Console.WriteLine("\n  ¿Está seguro que desea rendirse? (s/n): ");
                        if (Console.ReadLine()?.Trim().ToLower() == "s")
                        {
                            ShowGameOver(true);
                            return;
                        }
                        continue;
                    }

                    if (int.TryParse(input, out int edgeId))
                    {
                        if (edgeId >= 0 && edgeId < _board!.Edges.Count && !_gameState.EdgesTaken[edgeId])
                        {
                            var move = new Move(edgeId);
                            var result = _gameState.Apply(move);
                            validMove = true;

                            if (result.CapturedCount > 0)
                            {
                                _renderer.Render(_gameState);
                                Console.WriteLine();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"  ¡Excelente! ¡Capturaste {result.CapturedCount} celda(s)! ¡Turno extra!");
                                Console.ResetColor();
                                Console.WriteLine("\n  Presione cualquier tecla para continuar...");
                                Console.ReadKey();
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("  ❌ Línea inválida o ya dibujada. Intente de nuevo.");
                            Console.ResetColor();
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("  ❌ Entrada inválida. Ingrese un número.");
                        Console.ResetColor();
                        System.Threading.Thread.Sleep(1000);
                    }

                    if (!validMove)
                    {
                        _renderer.Render(_gameState);
                    }
                }
            }
        }

        ShowGameOver(false);
    }

    private void ShowGameOver(bool surrendered)
    {
        _renderer!.Render(_gameState!);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                      FIN DEL JUEGO                             ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();

        if (surrendered)
        {
            Console.WriteLine("  El jugador se rindió.");
        }
        else
        {
            int winner = _gameState.GetWinner();

            Console.Write("  Puntaje final: ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"Jugador 1: {_gameState.Scores[0]}");
            Console.ResetColor();
            Console.Write("  -  ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Jugador 2: {_gameState.Scores[1]}");
            Console.ResetColor();
            Console.WriteLine();

            if (winner == -1)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  🤝 ¡EMPATE! Ambos jugadores tienen la misma cantidad de celdas.");
            }
            else
            {
                Console.ForegroundColor = winner == 0 ? ConsoleColor.Red : ConsoleColor.Blue;
                Console.WriteLine($"  🏆 ¡GANADOR: JUGADOR {winner + 1}!");
                Console.WriteLine($"     {_gameState.Scores[winner]} celdas vs {_gameState.Scores[1 - winner]} celdas");
            }
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.WriteLine("  Presione cualquier tecla para volver al menú principal...");
        Console.ReadKey();
    }

    private void ShowInstructions()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                     INSTRUCCIONES                              ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("OBJETIVO:");
        Console.WriteLine("  Capturar más celdas (cuadros) que tu oponente.");
        Console.WriteLine();
        Console.WriteLine("CÓMO JUGAR:");
        Console.WriteLine("  1. Los jugadores se turnan dibujando líneas entre puntos adyacentes");
        Console.WriteLine("  2. Cuando completas los 4 lados de un cuadro, lo capturas");
        Console.WriteLine("  3. Si capturas un cuadro, obtienes un turno extra");
        Console.WriteLine("  4. El juego termina cuando todas las líneas están dibujadas");
        Console.WriteLine("  5. Gana quien tenga más cuadros capturados");
        Console.WriteLine();
        Console.WriteLine("EN LA PANTALLA:");
        Console.WriteLine("  • Los números representan líneas disponibles para dibujar");
        Console.WriteLine("  • Las líneas amarillas (━━ / ┃) están ya dibujadas");
        Console.WriteLine("  • Los cuadros rojos son del Jugador 1");
        Console.WriteLine("  • Los cuadros azules son del Jugador 2");
        Console.WriteLine();
        Console.WriteLine("ESTRATEGIA:");
        Console.WriteLine("  • Evita dejar cuadros con 3 lados (tu oponente los capturará)");
        Console.WriteLine("  • Intenta crear cadenas de capturas múltiples");
        Console.WriteLine("  • En el final del juego, cuenta cuidadosamente");
        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para volver al menú...");
        Console.ReadKey();
    }
}

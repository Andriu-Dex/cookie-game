## 🤖 Aplicación de la IA en el Proyecto

### 1. **Algoritmo Principal: Minimax con Poda Alpha-Beta**

**Archivo:** MinimaxAlphaBeta.cs

La IA usa el algoritmo **Minimax** que es un método de búsqueda adversarial. Piensa como un árbol de decisiones donde:

```
                    [Estado Actual]
                    /      |      \
            [Mov 1]   [Mov 2]   [Mov 3]  ← IA elige
            /    \      /   \      /   \
      [Resp1][Resp2][Resp3][Resp4]...   ← Oponente responde
```

#### **Cómo funciona paso a paso:**

**Paso 1: Búsqueda del mejor movimiento** (Línea 32-53)

```csharp
public Move GetBestMove(GameState state, int maximizingPlayer, int depth)
{
    Move bestMove = default;
    int bestScore = int.MinValue;

    // 1. Genera todos los movimientos posibles
    var orderedMoves = OrderMoves(state, state.GenerateMoves());

    // 2. Evalúa cada movimiento
    foreach (var move in orderedMoves)
    {
        // 3. Aplica el movimiento temporalmente
        var result = state.Apply(move);

        // 4. Busca recursivamente la mejor respuesta del oponente
        int score = -AlphaBeta(state, 1 - maximizingPlayer,
                              GetNextDepth(depth, result), -beta, -alpha);

        // 5. Deshace el movimiento (solo fue simulación)
        state.Undo(result);

        // 6. Guarda el mejor movimiento encontrado
        if (score > bestScore)
        {
            bestScore = score;
            bestMove = move;
        }
    }
    return bestMove;
}
```

**Paso 2: Búsqueda recursiva con poda** (Línea 70-90)

```csharp
private int AlphaBeta(GameState state, int maximizingPlayer, int depth,
                     int alpha, int beta)
{
    // Caso base: llegó al límite o juego terminó
    if (state.IsTerminal() || depth <= 0)
        return _evaluator.Evaluate(state, maximizingPlayer);

    int bestScore = int.MinValue;

    foreach (var move in orderedMoves)
    {
        var result = state.Apply(move);

        // Recursión: simula el juego hacia adelante
        int score = -AlphaBeta(state, 1 - maximizingPlayer,
                              GetNextDepth(depth, result), -beta, -alpha);

        state.Undo(result);
        bestScore = Math.Max(bestScore, score);
        alpha = Math.Max(alpha, score);

        // PODA: Si esta rama es mala, no sigas explorando
        if (alpha >= beta)
            break;  // ⚡ Esto ahorra ~98% de nodos
    }
    return bestScore;
}
```

### 2. **Función de Evaluación (Cerebro de la IA)**

**Archivo:** SimpleDotsEvaluator.cs

La IA evalúa qué tan buena es una posición usando esta fórmula:

```
Puntaje = 100×(MisCeldas - SusCeldas)           [Material]
        + 20×(SusCeldasCasi - MisCeldasCasi)     [Peligro]
        + 5×(MisMovSeguros - SusMovSeguros)      [Seguridad]
        + 2×(MisCeldas2Lados - SusCeldas2Lados)  [Potencial]
```

**Implementación** (Línea 29-53):

```csharp
public int Evaluate(GameState state, int maximizingPlayer)
{
    // 1. Cuenta celdas capturadas (lo más importante)
    int material = (state.Scores[maximizingPlayer] - state.Scores[minimizingPlayer])
                   * 100;

    // 2. Cuenta celdas con 3 lados (peligrosas)
    int almost = (minFeatures.AlmostCells - maxFeatures.AlmostCells) * 20;

    // 3. Cuenta movimientos seguros disponibles
    int safeMoves = (maxFeatures.SafeMoves - minFeatures.SafeMoves) * 5;

    // 4. Cuenta celdas con 2 lados (potencial futuro)
    int twoSided = (maxFeatures.TwoSidedCells - minFeatures.TwoSidedCells) * 2;

    return material + almost + safeMoves + twoSided;
}
```

### 3. **Optimización: Ordenamiento de Movimientos**

La IA ordena los movimientos por prioridad para explorar primero los mejores (Línea 109-160):

```csharp
private IEnumerable<Move> OrderMoves(GameState state, IEnumerable<Move> moves)
{
    var capturing = new List<Move>();    // 🏆 Prioridad 1: Capturas
    var safe = new List<Move>();         // ✅ Prioridad 2: Seguros
    var dangerous = new List<Move>();    // ⚠️ Prioridad 3: Peligrosos

    foreach (var move in moves)
    {
        // Clasifica cada movimiento
        if (completesCell)
            capturing.Add(move);      // Completa un cuadrado
        else if (!createsThirdSide)
            safe.Add(move);           // No regala cuadrados
        else
            dangerous.Add(move);      // Crea oportunidad para oponente
    }

    // Retorna en orden de prioridad
    return capturing.Concat(safe).Concat(dangerous);
}
```

### 4. **Integración en el Juego**

#### **En la Consola** (GameController.cs):

**Configuración** (Línea 178-179):

```csharp
var evaluator = new SimpleDotsEvaluator();
var minimax = new MinimaxAlphaBeta(evaluator);
_aiPlayer = new AIPlayer(1, minimax, _aiDepth);  // Profundidad 2-5
```

**Uso en el turno de la IA** (Línea 244-256):

```csharp
if (isAITurn)
{
    Console.WriteLine("IA está pensando...");

    // La IA piensa y elige el mejor movimiento
    var aiMove = _aiPlayer.GetMove(_gameState);

    // Aplica el movimiento
    var result = _gameState.Apply(aiMove);

    if (result.CapturedCount > 0)
        Console.WriteLine("¡IA capturó celdas! ¡Turno extra!");
}
```

#### **En la Web** (Game.razor):

**Configuración** (Línea 235-239):

```csharp
var evaluator = new SimpleDotsEvaluator();
var minimax = new MinimaxAlphaBeta(evaluator);
_searchStrategy = minimax;
```

**Uso asíncrono** (Línea 340-365):

```csharp
private async Task MakeAIMove()
{
    _isThinking = true;  // Muestra badge "IA pensando"
    StateHasChanged();

    await Task.Delay(500);  // Pausa visual

    // IA calcula mejor movimiento (puede tomar 0.1-10 segundos)
    var move = _searchStrategy.GetBestMove(_gameState, _gameState.CurrentPlayer, _selectedDepth);

    MakeMove(move);

    _isThinking = false;
    StateHasChanged();

    // Si capturó celdas, la IA juega de nuevo
    if (!_gameOver && IsCurrentPlayerAI())
    {
        await Task.Delay(800);
        await MakeAIMove();  // Recursión para turno extra
    }
}
```

### 5. **Niveles de Dificultad**

La profundidad determina qué tan adelante "piensa" la IA:

| Nivel   | Profundidad | Movimientos Adelante | Nodos Explorados | Tiempo   |
| ------- | ----------- | -------------------- | ---------------- | -------- |
| Fácil   | 2           | 2 turnos             | ~100-500         | < 0.1s   |
| Normal  | 3           | 3 turnos             | ~1,000-5,000     | 0.1-0.5s |
| Difícil | 4           | 4 turnos             | ~10,000-50,000   | 0.5-2s   |
| Experto | 5           | 5 turnos             | ~100,000-500,000 | 2-10s    |

**Configuración:**

- Consola: GameController.cs línea 115-126
- Web: Game.razor línea 61-68

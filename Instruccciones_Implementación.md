## 🧩 **Reglamento del Juego de la Galleta**

### **Descripción general**

El _Juego de la Galleta_ es un pasatiempo estratégico tradicional que se juega sobre un papel cuadriculado. Su dinámica se basa en la conexión de puntos para formar cuadros dentro de una figura cerrada con forma de galleta:
![alt text](<ChatGPT Image 1 nov 2025, 15_10_46.png>)

Cada jugador compite por capturar la mayor cantidad posible de cuadros mediante la selección óptima de líneas, aplicando razonamiento lógico y toma de decisiones.

---

### **Reglas del juego**

1. **Tablero de juego**
   El juego se dibuja sobre un **papel cuadriculado**, formando un **tablero con forma de galleta** compuesto por **puntos equidistantes**.

   - Los puntos representan las posibles intersecciones donde los jugadores pueden trazar líneas.
   - La figura debe ser cerrada y simétrica (generalmente en forma de rombo dentado o diamante).

2. **Jugadores**
   Participan dos jugadores:

   - El **Jugador 1** (por ejemplo, símbolo ⭕).
   - El **Jugador 2** (por ejemplo, símbolo ✖️).
     En la versión con IA, uno de los jugadores puede ser controlado por el computador mediante un algoritmo de decisión.

3. **Objetivo del juego**
   Capturar la mayor cantidad de cuadros posibles dentro del tablero al completar los cuatro lados de cada cuadrado.

   - Cada cuadro capturado se marca con el símbolo del jugador correspondiente.
   - Gana quien obtenga **más cuadros capturados** al finalizar la partida.

4. **Dinámica de juego**

   - Los jugadores se turnan para trazar una línea entre **dos puntos adyacentes** (horizontal o vertical, nunca diagonal).
   - Si un jugador al trazar una línea **completa los cuatro lados de un cuadrado**, ese cuadro le pertenece y debe marcarlo con su símbolo.
   - Cuando un jugador captura un cuadro, **tiene derecho a realizar una jugada adicional** inmediatamente.
   - Si no forma un cuadro, el turno pasa al oponente.

5. **Finalización del juego**
   El juego concluye cuando **todas las líneas posibles han sido trazadas**, es decir, no quedan conexiones disponibles entre puntos.
   El ganador será el jugador con **mayor cantidad de cuadros capturados**.
   En caso de empate, se declarará un resultado igualado.

6. **Tamaño del tablero**
   El tablero puede variar en tamaño dependiendo del nivel de complejidad:

   - **Básico**: figura pequeña (ej. 5×5 puntos).
   - **Intermedio**: figura media (ej. 7×7 puntos).
   - **Avanzado**: figura grande (ej. 9×9 o superior).
     El tamaño del tablero influye directamente en la cantidad de posibles movimientos y en la dificultad del algoritmo.

---

### **Adaptación para Inteligencia Artificial**

Para su implementación en C#, el juego se modelará como un **problema de búsqueda adversarial**, donde:

- Cada **estado del juego** representa la configuración actual de líneas y cuadros capturados.
- Cada **acción posible** es trazar una línea disponible entre dos puntos adyacentes.
- La **función de utilidad o evaluación heurística** mide la ventaja de un jugador considerando:

  - Cuadros capturados.
  - Oportunidades de crear nuevos cuadros.
  - Riesgo de dejar cuadros listos para el oponente.

- El **algoritmo de decisión** recomendado es **Minimax** con **poda alfa-beta**, permitiendo al agente seleccionar movimientos óptimos anticipando las respuestas del oponente.

---

**Tablero (grafo)**

- Conjunto de puntos V (nodos) y líneas E (aristas) horizontales/verticales válidas dentro de la **forma de galleta**.
- Conjunto de celdas C; cada celda c∈C es un conjunto de 4 aristas (los 4 lados de un cuadrado).

**Estado**

- `edgesBitset` (líneas ya dibujadas).
- `scores[2]` (cuadros capturados por jugador).
- `currentPlayer` (0 o 1).
- `remainingEdges = |E| - edgesBitset.count()`.

**Acciones**

- Trazar una línea disponible `e ∈ E` tal que `e` aún no esté en `edgesBitset`.

**Transición**

- Al aplicar `e`, marcar celdas completadas (0, 1 o más).
- Si se completó ≥1 celda, el turno **no cambia** (regla de jugada adicional); si no, cambia de jugador.

**Prueba terminal**

- `remainingEdges == 0`.

**Utilidad**

- `U(estado) = score(maxPlayer) – score(minPlayer)` (para minimax).

---

# Heurística (evaluador)

Pensada para dots-and-boxes/“galleta”, simple pero efectiva y sin sobreingeniería:

- `material = (myBoxes - oppBoxes) * W1`
- `almost = (opponentAlmostBoxes - myAlmostBoxes) * W2`
  (donde _almost_ = celdas con 3 lados ya trazados)
- `safeties = (mySafeMoves - oppSafeMoves) * W3`
  (_safe_ = trazar una línea que NO deje una celda con 3 lados)
- `twos = (myTwos - oppTwos) * W4`
  (celdas con 2 lados: potenciales cadenas; valoramos crear “largas” para el final)

Valores típicos: `W1=100, W2=20, W3=5, W4=2`. (Ajustables.)

Orden de movimientos para la búsqueda:

1. Movimientos que **capturan** (cierran cuadro).
2. **Safe moves**.
3. Resto (los que crean “3 lados” y suelen regalar puntos → al final del turno o como sacrificio controlado).

---

# Algoritmo de decisión

**Minimax con poda α–β** (búsqueda adversarial), con:

- **Profundidad fija** (p.ej., 5–7) e **iterative deepening** opcional.
- **Ordenamiento** de movimientos (arriba).
- **Detección de turno repetido**: si la jugada captura, el mismo jugador continúa (en la recursión, no inviertes el “maximiza/minimiza”).

> Nota: α–β ya es un “bound” y encaja bien con las ideas de _branch & bound_ del libro; la **generación de sucesores** se hace con un **enumerador/iterador** (patrón de Mellender) para mantener limpio el diseño.

---

# Arquitectura (Clean, SOLID y patrones)

**Capas**

- **Domain**: reglas del juego (agnóstico a UI).
- **Application**: IA (estrategias de búsqueda) y orquestación.
- **Infrastructure**: utilidades (tiempo, logging opcional).
- **Presentation (UI)**: WPF/WinForms/MAUI (textos en **español**).

**Interfaces (DIP, ISP)**

```csharp
public interface IBoardShape
{
    Board Build();
}

public interface IEvaluator
{
    int Evaluate(GameState state, int maximizingPlayer);
}

public interface ISearchStrategy
{
    Move GetBestMove(GameState state, int maximizingPlayer, int depth);
}
```

**Entidades principales (SRP, OCP)**

```csharp
public sealed class Board
{
    public int VertexCount { get; }
    public IReadOnlyList<Edge> Edges { get; }
    public IReadOnlyList<Cell> Cells { get; }
    // Precalculos: edges→cells, cell→edges
    public int[][] EdgesToCells { get; }
    public int[][] CellEdges { get; }
}

public readonly struct Edge { public int Id; public int A, B; public bool IsHorizontal; }
public readonly struct Cell { public int Id; public int[] EdgeIds; }

public readonly struct Move { public int EdgeId; }

public sealed class GameState
{
    public Board Board { get; }
    public BitArray EdgesTaken { get; } // o un UInt64 si cabe
    public int[] Scores { get; } = new int[2];
    public int CurrentPlayer { get; private set; }

    public bool IsTerminal() => /* remainingEdges == 0 */;
    public IEnumerable<Move> GenerateMoves(); // Enumerator limpio
    public AppliedResult Apply(Move move);     // Command: ejecutar
    public void Undo(AppliedResult tag);       // Command: deshacer rápido
}

public readonly struct AppliedResult
{
    public Move Move;
    public int CapturedCount;     // para revertir puntajes y turno
    public int[] CapturedCellIds; // opcional para UI/animación
}
```

**Patrones utilizados (justo lo necesario, sin sobreingeniería)**

- **Strategy**: `ISearchStrategy` (MinimaxAlphaBeta).
- **Factory Method / Abstract Factory**: `IBoardShape` (p.ej., `GalletaShapeFactory`).
- **Command (lightweight)**: `Apply/Undo` de movimientos.
- **Enumerator** para sucesores (alineado al libro).
- (Opcional) **MVVM** en WPF para UI limpia.

---

# Generación de la “galleta” (tablero)

**Idea**: diamante dentro de un grid de puntos (distancia Manhattan).

- Define un grid `N x N` de puntos con centro `(cx, cy)`.
- Un punto `(x,y)` pertenece si `|x - cx| + |y - cy| <= R`.
- Crea aristas horizontales/verticales solo entre puntos **válidos** y **adyacentes**.
- Crea celdas donde existan los 4 vértices válidos y sus 4 aristas.

```csharp
public sealed class GalletaShapeFactory : IBoardShape
{
    private readonly int _radius; // controla tamaño (complejidad)

    public GalletaShapeFactory(int radius) { _radius = Math.Max(2, radius); }

    public Board Build()
    {
        // 1) Generar puntos válidos (diamante)
        // 2) Generar aristas H/V entre puntos válidos adyacentes
        // 3) Generar celdas (cuadritos) totalmente dentro de la forma
        // 4) Precalcular mapeos: edges→cells y cell→edges
        // 5) Devolver Board
    }
}
```

---

# Evaluador (heurística) – ejemplo claro

```csharp
public sealed class SimpleDotsEvaluator : IEvaluator
{
    private const int W1 = 100; // material
    private const int W2 = 20;  // casi-cuadros
    private const int W3 = 5;   // movimientos seguros
    private const int W4 = 2;   // celdas con 2 lados

    public int Evaluate(GameState s, int maxPlayer)
    {
        int minPlayer = 1 - maxPlayer;

        (int myAlmost, int mySafes, int myTwos) = Features(s, maxPlayer);
        (int opAlmost, int opSafes, int opTwos) = Features(s, minPlayer);

        int material = (s.Scores[maxPlayer] - s.Scores[minPlayer]) * W1;
        int almost   = (opAlmost - myAlmost) * W2;
        int safes    = (mySafes - opSafes) * W3;
        int twos     = (myTwos - opTwos) * W4;

        return material + almost + safes + twos;
    }

    private static (int almost, int safes, int twos) Features(GameState s, int player)
    {
        int almost = 0, safes = 0, twos = 0;

        // contar celdas por número de lados ya dibujados
        foreach (var cell in s.Board.Cells)
        {
            int sides = CountTaken(s, cell.EdgeIds);
            if (sides == 3) almost++;
            if (sides == 2) twos++;
        }

        // contar “safe moves”
        foreach (var m in s.GenerateMoves())
        {
            bool createsThirdSide = false;
            foreach (var cellId in s.Board.EdgesToCells[m.EdgeId])
            {
                var eids = s.Board.CellEdges[cellId];
                int sides = CountTaken(s, eids);
                if (sides == 2) createsThirdSide = true; // pasarías a 3 lados
            }
            if (!createsThirdSide) safes++;
        }

        return (almost, safes, twos);
    }

    private static int CountTaken(GameState s, int[] edgeIds)
    {
        int count = 0;
        foreach (var e in edgeIds) if (s.EdgesTaken.Get(e)) count++;
        return count;
    }
}
```

---

# Minimax + α–β (núcleo)

```csharp
public sealed class MinimaxAlphaBeta : ISearchStrategy
{
    private readonly IEvaluator _evaluator;

    public MinimaxAlphaBeta(IEvaluator evaluator) => _evaluator = evaluator;

    public Move GetBestMove(GameState state, int maximizingPlayer, int depth)
    {
        Move best = default;
        int bestScore = int.MinValue;

        foreach (var m in Ordered(state)) // capturas → seguras → resto
        {
            var tag = state.Apply(m);
            int score = -AlphaBeta(state, 1 - maximizingPlayer, depth - NextDepth(tag), int.MinValue + 1, int.MaxValue - 1);
            state.Undo(tag);

            if (score > bestScore) { bestScore = score; best = m; }
        }
        return best;
    }

    private int AlphaBeta(GameState s, int maximizingPlayer, int depth, int alpha, int beta)
    {
        if (s.IsTerminal() || depth <= 0) return _evaluator.Evaluate(s, maximizingPlayer);

        foreach (var m in Ordered(s))
        {
            var tag = s.Apply(m);
            int nextDepth = depth - NextDepth(tag);
            int val = -AlphaBeta(s, 1 - maximizingPlayer, nextDepth, -beta, -alpha);
            s.Undo(tag);

            if (val > alpha) alpha = val;
            if (alpha >= beta) break; // poda
        }
        return alpha;
    }

    // Si capturas cuadros, no reduces profundidad (te permite “mirar” cadenas)
    private static int NextDepth(AppliedResult tag) => (tag.CapturedCount > 0) ? 0 : 1;

    private static IEnumerable<Move> Ordered(GameState s)
    {
        // 1) capturas, 2) safe, 3) resto
        var captures = new List<Move>();
        var safes    = new List<Move>();
        var others   = new List<Move>();

        foreach (var m in s.GenerateMoves())
        {
            bool completesCell = false, createsThird = false;
            foreach (var cellId in s.Board.EdgesToCells[m.EdgeId])
            {
                int sides = CountTaken(s, s.Board.CellEdges[cellId]);
                if (sides == 3) completesCell = true;
                if (sides == 2) createsThird = true;
            }
            if (completesCell) captures.Add(m);
            else if (!createsThird) safes.Add(m);
            else others.Add(m);
        }

        foreach (var m in captures) yield return m;
        foreach (var m in safes)    yield return m;
        foreach (var m in others)   yield return m;
    }

    private static int CountTaken(GameState s, int[] edgeIds)
    {
        int count = 0; foreach (var e in edgeIds) if (s.EdgesTaken.Get(e)) count++; return count;
    }
}
```

---

# UI (en español)

- Ventana: **“Juego de la Galleta”**.
- Controles: **Nuevo juego**, **Tamaño**, **Forma: Galleta**, **Jugador actual**, **Puntajes**.
- Render del tablero con hover y click en líneas habilitadas.
- Mensajes: “Turno extra”, “Fin de partida”, “Empate”, “Gana Jugador X”.

---

# Plan de implementación (paso a paso)

1. **Board/Shape**: `GalletaShapeFactory(radius)` + `Board` con precálculos (`EdgesToCells`, `CellEdges`).
2. **GameState**: bitset de aristas, `Apply/Undo`, `GenerateMoves()`.
3. **Evaluator**: `SimpleDotsEvaluator` (arriba).
4. **Search**: `MinimaxAlphaBeta` con ordenamiento.
5. **AIPlayer**: orquestra `ISearchStrategy` + `IEvaluator` + `depth`.
6. **UI**: dibujo, clicks, textos en **español**; modelo de dominio **independiente** de UI.
7. **Pruebas**: tableros pequeños (R=3/4). Comprobar que la IA evita “regalar” cuadros y encadena capturas al final.

---

Perfecto 💪 — aquí tienes la **lista completa de requisitos** que debe cumplir tu proyecto del **Juego de la Galleta**, tomando en cuenta:

- el **enunciado original del docente**,
- los **principios y patrones** exigidos (Clean Code, SOLID, patrones de diseño, sin sobreingeniería),
- la **base teórica** del libro _“Design Patterns for Searching in C#”_ de Fred Mellender,
- y las **buenas prácticas de desarrollo académico** en C# que tú estás aplicando.

---

## 🧾 **Requisitos del Proyecto: Juego de la Galleta con IA**

### 🔹 1. Requisitos funcionales

1. **Interfaz de usuario (en español)**

   - Debe permitir al usuario humano jugar contra la computadora.
   - La interfaz mostrará:

     - Tablero en forma de galleta (figura cerrada con puntos equidistantes).
     - Puntos y líneas disponibles para seleccionar.
     - Indicadores de turno actual, puntaje de cada jugador y mensajes (“Turno extra”, “Fin del juego”, etc.).

   - Botones principales:

     - **Nuevo juego**
     - **Seleccionar tamaño del tablero (5×5, 7×7, 9×9...)**
     - **Salir**

2. **Mecánica del juego**

   - Los jugadores trazan líneas horizontales o verticales entre puntos adyacentes.
   - Si un jugador completa un cuadro, obtiene un punto y realiza otra jugada.
   - El juego termina cuando todas las líneas están trazadas.
   - Gana el jugador con más cuadros capturados.

3. **Juego contra IA**

   - El usuario puede enfrentarse a una **IA que toma decisiones autónomas**.
   - La IA debe analizar el estado del juego y seleccionar la mejor jugada posible aplicando **métodos de búsqueda**.

---

### 🔹 2. Requisitos técnicos (implementación en C#)

1. **Lenguaje:**

   - Código fuente en **C# (versión moderna, .NET 6 o superior)**.
   - Nombres de clases, variables y métodos en **inglés**.
   - Interfaz de usuario (labels, botones, textos) en **español**.

2. **Arquitectura:**

   - Seguir una **arquitectura limpia (Clean Architecture)** con separación de capas:

     - **Domain:** reglas del juego, entidades (`Board`, `GameState`, etc.).
     - **Application:** lógica de IA, estrategias de búsqueda y evaluación.
     - **Presentation:** interfaz gráfica (WPF, WinForms o MAUI).
     - **Infrastructure:** utilidades o servicios complementarios.

3. **Principios SOLID:**

   - **S (SRP)**: cada clase debe tener una única responsabilidad.
   - **O (OCP)**: permitir ampliar componentes (p. ej., agregar otra forma de tablero) sin modificar el código base.
   - **L (LSP)**: respetar la sustitución entre interfaces (`ISearchStrategy`, `IEvaluator`, etc.).
   - **I (ISP)**: interfaces pequeñas y específicas (por ejemplo, `IBoardShape`, `ISearchStrategy`).
   - **D (DIP)**: las dependencias deben invertirse; las capas superiores dependen de abstracciones, no de implementaciones concretas.

4. **Patrones de diseño:**

   - **Strategy**: para la IA (`ISearchStrategy` implementada por `MinimaxAlphaBeta`).
   - **Factory Method**: para crear la figura de la galleta (`GalletaShapeFactory`).
   - **Command**: para aplicar y deshacer movimientos (`Apply/Undo`).
   - **Iterator / Enumerator**: para generar sucesores del estado (como propone el libro).
   - (Opcional) **MVVM**: si la interfaz usa WPF.

5. **Inteligencia artificial:**

   - Algoritmo de decisión: **Minimax con poda alfa-beta**.
   - Uso de heurística evaluadora (`SimpleDotsEvaluator`) basada en:

     - Diferencia de cuadros capturados.
     - Riesgo de dejar cuadros listos al oponente.
     - Movimientos seguros y cadenas potenciales.

   - Profundidad de búsqueda configurable (por ejemplo, 5 niveles).
   - Uso de **ordenamiento de movimientos** para mejorar rendimiento.

6. **Basado en el libro de Fred Mellender:**

   - Implementar la **enumeración de sucesores** como iterador, siguiendo el patrón descrito en _Design Patterns for Searching in C#_.
   - Aplicar **estructuras de búsqueda** y **abstracción de estado** como se detalla en el capítulo de _Depth-First Search (DFS)_ y _Branch and Bound_.
   - Mantener la arquitectura modular y extensible sin sobreingeniería.

7. **Buenas prácticas (Clean Code):**

   - Nombres descriptivos (verbos para métodos, sustantivos para clases).
   - Métodos cortos y legibles.
   - Comentarios solo cuando el código no sea autoexplicativo.
   - Evitar duplicación de lógica (principio DRY).
   - Convenciones de estilo C# (PascalCase, camelCase, etc.).
   - Uso consistente de excepciones, sin abusar de condicionales anidados.

---

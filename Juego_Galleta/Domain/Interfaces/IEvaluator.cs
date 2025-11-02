using Juego_Galleta.Domain.Entities;

namespace Juego_Galleta.Domain.Interfaces;

/// <summary>
/// Strategy pattern: defines the interface for evaluating game states.
/// Different evaluators can implement different heuristics without modifying the search algorithm.
/// </summary>
public interface IEvaluator
{
    /// <summary>
    /// Evaluates the current game state from the perspective of the maximizing player.
    /// Higher values indicate better positions for the maximizing player.
    /// </summary>
    /// <param name="state">The game state to evaluate.</param>
    /// <param name="maximizingPlayer">The player to maximize (0 or 1).</param>
    /// <returns>An evaluation score. Positive favors maximizing player, negative favors opponent.</returns>
    int Evaluate(GameState state, int maximizingPlayer);
}

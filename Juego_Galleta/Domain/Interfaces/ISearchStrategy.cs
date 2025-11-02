using Juego_Galleta.Domain.Entities;

namespace Juego_Galleta.Domain.Interfaces;

/// <summary>
/// Strategy pattern: defines the interface for search algorithms.
/// Different search strategies can be implemented without modifying game logic.
/// </summary>
public interface ISearchStrategy
{
    /// <summary>
    /// Gets the best move for the current game state.
    /// </summary>
    /// <param name="state">The current game state.</param>
    /// <param name="maximizingPlayer">The player to maximize (0 or 1).</param>
    /// <param name="depth">Maximum search depth.</param>
    /// <returns>The best move found.</returns>
    Move GetBestMove(GameState state, int maximizingPlayer, int depth);
}

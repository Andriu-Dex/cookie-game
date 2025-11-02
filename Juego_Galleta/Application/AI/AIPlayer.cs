using Juego_Galleta.Domain.Entities;
using Juego_Galleta.Domain.Interfaces;

namespace Juego_Galleta.Application.AI;

/// <summary>
/// AI Player that combines search strategy and evaluation function.
/// This orchestrates the AI decision-making process.
/// </summary>
public sealed class AIPlayer
{
    private readonly ISearchStrategy _searchStrategy;
    private readonly int _searchDepth;
    private readonly int _playerNumber;

    /// <summary>
    /// Creates an AI player.
    /// </summary>
    /// <param name="playerNumber">Player number (0 or 1).</param>
    /// <param name="searchStrategy">The search algorithm to use.</param>
    /// <param name="searchDepth">Maximum search depth (typically 3-7).</param>
    public AIPlayer(int playerNumber, ISearchStrategy searchStrategy, int searchDepth = 5)
    {
        if (playerNumber < 0 || playerNumber > 1)
            throw new ArgumentException("Player number must be 0 or 1.", nameof(playerNumber));
        if (searchDepth < 1)
            throw new ArgumentException("Search depth must be at least 1.", nameof(searchDepth));

        _playerNumber = playerNumber;
        _searchStrategy = searchStrategy ?? throw new ArgumentNullException(nameof(searchStrategy));
        _searchDepth = searchDepth;
    }

    /// <summary>
    /// Gets the player number.
    /// </summary>
    public int PlayerNumber => _playerNumber;

    /// <summary>
    /// Gets the search depth.
    /// </summary>
    public int SearchDepth => _searchDepth;

    /// <summary>
    /// Calculates and returns the best move for the current game state.
    /// </summary>
    public Move GetMove(GameState state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));

        if (state.IsTerminal())
            throw new InvalidOperationException("Cannot get move for terminal state.");

        // Verify it's this player's turn
        if (state.CurrentPlayer != _playerNumber)
            throw new InvalidOperationException($"It's not player {_playerNumber}'s turn.");

        return _searchStrategy.GetBestMove(state, _playerNumber, _searchDepth);
    }

    /// <summary>
    /// Gets statistics about the AI player.
    /// </summary>
    public string GetInfo()
    {
        return $"AI Player {_playerNumber}: {_searchStrategy.GetType().Name}, Depth={_searchDepth}";
    }

    public override string ToString() => GetInfo();
}

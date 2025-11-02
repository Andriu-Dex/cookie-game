using Juego_Galleta.Domain.Entities;

namespace Juego_Galleta.Domain.Interfaces;

/// <summary>
/// Factory Method pattern: defines the interface for creating board shapes.
/// This allows different board configurations to be created without modifying existing code (OCP).
/// </summary>
public interface IBoardShape
{
    /// <summary>
    /// Builds and returns a complete board with all vertices, edges, and cells configured.
    /// </summary>
    /// <returns>A fully constructed Board instance.</returns>
    Board Build();
}

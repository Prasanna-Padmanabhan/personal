using SharpJackApi.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.Tests
{
    /// <summary>
    /// Additional operations necessary for validation.
    /// </summary>
    /// <remarks>
    /// Provides an abstraction for client specific implementations
    /// of operations necessary for validation purposes.
    /// </remarks>
    public interface ITestGameClient : IGameClient
    {
        /// <summary>
        /// Take necessary steps to trigger evaluation of a game.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Nothing</returns>
        public Task TriggerEvaluationAsync(int gameId, CancellationToken token);
    }
}
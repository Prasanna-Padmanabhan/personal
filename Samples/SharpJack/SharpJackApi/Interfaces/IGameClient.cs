using SharpJackApi.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.Interfaces
{
    /// <summary>
    /// Represents the operations possible on a game.
    /// </summary>
    /// <remarks>
    /// By making this an interface, we can have multiple implementations.
    /// One is the API controller which is what will be used in production.
    /// Second is the Service which implements the business logic behind the API.
    /// Third and fourth are the encapsulations of the above two for testing purposes.
    /// </remarks>
    public interface IGameClient : IDisposable
    {
        Task<Player> AddPlayerAsync(string playerName, CancellationToken token);

        Task<Player> GetPlayerAsync(int playerId, CancellationToken token);

        Task<Game> CreateGameAsync(GameOptions options, CancellationToken token);

        Task<Game> GetGameAsync(int gameId, CancellationToken token);

        Task JoinOrStartGameAsync(int gameId, Player player, CancellationToken token);

        Task AskQuestionAsync(int gameId, Question question, CancellationToken token);

        Task<Question> GetActiveQuestionAsync(int gameId, Player player, CancellationToken token);

        Task<Answer> SubmitAnswerAsync(int gameId, Answer answer, CancellationToken token);

        Task<LeaderBoard> GetBoardAsync(int gameId, CancellationToken token);

        Task EndGameAsync(int gameId, CancellationToken token);
    }
}
using Microsoft.EntityFrameworkCore;
using SharpJackApi.Contracts;
using SharpJackApi.Data;
using SharpJackApi.Interfaces;
using SharpJackApi.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.Tests
{
    public class SharpJackServiceClient : IDisposable, IGameClient
    {
        /// <summary>
        /// Database connection string.
        /// </summary>
        /// <remarks>
        /// Tests are executed against a real database (SQL Express LocalDB) to make sure EF Core + LINQ
        /// commands will work as closely to a production database as possible.
        /// </remarks>
        private const string ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=sharpjacktest;Trusted_Connection=True;MultipleActiveResultSets=true";

        /// <summary>
        /// The GameService instance to test against.
        /// </summary>
        private GameService service;

        /// <summary>
        /// The database context.
        /// </summary>
        /// <remarks>
        /// Make the database context available for create/delete operations
        /// as well as additional validation.
        /// </remarks>
        public GameContext Context => service.Context;

        /// <summary>
        /// The time encapsulation.
        /// </summary>
        /// <remarks>
        /// Make the time encapsulation available for manipulation by tests.
        /// </remarks>
        public DateTime CurrentTime
        {
            get
            {
                return service.TimeService.CurrentTime;
            }
            set
            {
                service.TimeService.CurrentTime = value;
            }
        }

        /// <summary>
        /// Initialize resources.
        /// </summary>
        public SharpJackServiceClient()
        {
            var context = new GameContext(new DbContextOptionsBuilder<GameContext>().UseSqlServer(ConnectionString).Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            service = new GameService(context, null);
            service.TimeService.CurrentTime = DateTime.UtcNow;
        }

        public Task<Player> AddPlayerAsync(string playerName, CancellationToken token)
        {
            return service.AddPlayerAsync(playerName, token);
        }

        public Task<Player> GetPlayerAsync(int playerId, CancellationToken token)
        {
            return service.GetPlayerAsync(playerId, token);
        }

        public Task<Game> CreateGameAsync(GameOptions options, CancellationToken token)
        {
            return service.CreateGameAsync(options, token);
        }

        public Task<Game> GetGameAsync(int gameId, CancellationToken token)
        {
            return service.GetGameAsync(gameId, token);
        }

        public Task JoinOrStartGameAsync(int gameId, Player player, CancellationToken token)
        {
            return service.JoinOrStartGameAsync(gameId, player, token);
        }

        public Task AskQuestionAsync(int gameId, Question question, CancellationToken token)
        {
            return service.AskQuestionAsync(gameId, question, token);
        }

        public Task<Question> GetActiveQuestionAsync(int gameId, Player player, CancellationToken token)
        {
            return service.GetActiveQuestionAsync(gameId, player, token);
        }

        public Task<Answer> SubmitAnswerAsync(int gameId, Answer answer, CancellationToken token)
        {
            return service.SubmitAnswerAsync(gameId, answer, token);
        }

        public Task<LeaderBoard> GetBoardAsync(int gameId, CancellationToken token)
        {
            return service.GetBoardAsync(gameId, token);
        }

        public Task EndGameAsync(int gameId, CancellationToken token)
        {
            return service.Context.Database.EnsureDeletedAsync();
        }

        /// <summary>
        /// Release resources.
        /// </summary>
        public void Dispose()
        {
            if (service != null)
            {
                service.Dispose();
                service = null;
            }
        }
    }
}
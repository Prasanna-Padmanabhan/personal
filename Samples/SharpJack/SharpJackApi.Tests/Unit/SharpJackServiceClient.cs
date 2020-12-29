using Microsoft.EntityFrameworkCore;
using SharpJackApi.Data;
using SharpJackApi.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.Tests
{
    public class SharpJackServiceClient : GameService, ITestGameClient
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
        /// The database context.
        /// </summary>
        /// <remarks>
        /// Make the database context available for create/delete operations
        /// as well as additional validation.
        /// </remarks>
        public new GameContext Context => base.Context;

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
                return TimeService.CurrentTime;
            }
            set
            {
                TimeService.CurrentTime = value;
            }
        }

        /// <summary>
        /// Initialize resources.
        /// </summary>
        public SharpJackServiceClient()
            : base(new GameContext(new DbContextOptionsBuilder<GameContext>().UseSqlServer(ConnectionString).Options), null)
        {
            Context.Database.EnsureDeleted();
            Context.Database.EnsureCreated();
            CurrentTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Delete the database for predictable test results.
        /// </summary>
        /// <param name="gameId">The ID of the game to end.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Nothing</returns>
        public override async Task<Contracts.Game> EndGameAsync(int gameId, Contracts.Player player, CancellationToken token)
        {
            var game = await base.EndGameAsync(gameId, player, token);

            await Context.Database.EnsureDeletedAsync();

            return game;
        }

        /// <summary>
        /// Take actions necessary to trigger game engine evaluation.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Nothing</returns>
        public async Task TriggerEvaluationAsync(int gameId, CancellationToken token)
        {
            var game = await Context.GetGameAsync(gameId, token);
            // Advance the time so the game engine can evaluate results
            CurrentTime += TimeSpan.FromSeconds(game.Options.MaxAnswerTime);
        }

        public async Task ExecuteAsync(int? gameId, CancellationToken token)
        {

        }
    }
}
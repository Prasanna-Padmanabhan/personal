using Microsoft.EntityFrameworkCore;
using SharpJackApi.Models;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.Data
{
    /// <summary>
    /// Useful utility methods.
    /// </summary>
    public static class GameContextExtensions
    {
        /// <summary>
        /// Get the details of a given player.
        /// </summary>
        /// <param name="context">The context to get details from.</param>
        /// <param name="playerId">The ID of the player to get.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The player details.</returns>
        public static Task<Player> GetPlayerAsync(this GameContext context, int playerId, CancellationToken token)
        {
            return context.Players.FirstAsync(p => p.Id == playerId, token);
        }

        /// <summary>
        /// Get the details of a given game.
        /// </summary>
        /// <param name="context">The context to get details from.</param>
        /// <param name="gameId">The ID of the game to get.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The game details.</returns>
        public static Task<Game> GetGameAsync(this GameContext context, int gameId, CancellationToken token)
        {
            // load all related entities so business logic is simplified
            // can have a performance impact which we will have to assess
            return context.Games
                .Include(g => g.ActiveQuestion)
                .Include(g => g.Answers)
                .Include(g => g.Players)
                .Include(g => g.Board)
                    .ThenInclude(b => b.Rows)
                        .ThenInclude(r => r.Player)
                .FirstAsync(g => g.Id == gameId, token);
        }

        /// <summary>
        /// Explicitly 'touch' game entity state to force EF to track and save all changes.
        /// </summary>
        /// <param name="context">The context to save.</param>
        /// <param name="game">The game object.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The number of entries saved.</returns>
        public static Task<int> SaveAsync(this GameContext context, Game game, CancellationToken token)
        {
            context.Update(game);
            return context.SaveChangesAsync(token);
        }
    }
}
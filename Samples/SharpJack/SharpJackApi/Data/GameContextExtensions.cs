using SharpJackApi.Models;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SharpJackApi.Data
{
    public static class GameContextExtensions
    {
        public static Task<Player> GetPlayerAsync(this GameContext context, int playerId, CancellationToken token)
        {
            return context.Players.FirstAsync(p => p.Id == playerId, token);
        }

        public static Task<Game> GetGameAsync(this GameContext context, int gameId, CancellationToken token)
        {
            return context.Games.FirstAsync(g => g.Id == gameId, token);
        }
    }
}
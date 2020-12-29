using SharpJackApi.Contracts;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.Tests
{
    public class SharpJackApiClient : ITestGameClient
    {
        /// <summary>
        /// The endpoint to test against.
        /// </summary>
        private const string Endpoint = "https://sharpjackapi.azurewebsites.net";

        /// <summary>
        /// Extra time to let things catch up.
        /// </summary>
        private const int GraceTime = 1;

        /// <summary>
        /// The HTTP client to perform operations with.
        /// </summary>
        private HttpClient client;

        public SharpJackApiClient()
        {
            client = new HttpClient
            {
                BaseAddress = new Uri(Endpoint)
            };
        }

        public Task<Player> AddPlayerAsync(string playerName, CancellationToken token)
        {
            return client.PostAsJsonAsync<string, Player>("players", playerName, token);
        }

        public Task<Player> GetPlayerAsync(int playerId, CancellationToken token)
        {
            return client.GetAsJsonAsync<Player>($"players/{playerId}", token);
        }

        public Task<Game> CreateGameAsync(GameOptions options, CancellationToken token)
        {
            return client.PostAsJsonAsync<GameOptions, Game>("game", options, token);
        }

        public Task<Game> GetGameAsync(int gameId, CancellationToken token)
        {
            return client.GetAsJsonAsync<Game>($"game/{gameId}", token);
        }

        public Task JoinOrStartGameAsync(int gameId, Player player, CancellationToken token)
        {
            return client.PutAsJsonAsync($"game/{gameId}", player, token);
        }

        public Task AskQuestionAsync(int gameId, Question question, CancellationToken token)
        {
            return client.PostAsJsonAsync($"game/{gameId}/questions", question, token);
        }

        public Task<Question> GetActiveQuestionAsync(int gameId, Player player, CancellationToken token)
        {
            return client.GetAsJsonAsync<Player, Question>($"game/{gameId}/questions", player, token);
        }

        public Task<Answer> SubmitAnswerAsync(int gameId, Answer answer, CancellationToken token)
        {
            return client.PutAsJsonAsync<Answer, Answer>($"game/{gameId}/questions", answer, token);
        }

        public Task<LeaderBoard> GetBoardAsync(int gameId, CancellationToken token)
        {
            return client.GetAsJsonAsync<LeaderBoard>($"game/{gameId}/board", token);
        }

        public Task<Game> EndGameAsync(int gameId, Player player, CancellationToken token)
        {
            return client.PostAsJsonAsync<Player, Game>($"game/{gameId}", player, token);
        }

        public async Task TriggerEvaluationAsync(int gameId, CancellationToken token)
        {
            var game = await GetGameAsync(gameId, token);
            // there is no way to force evaluation, we just have to wait
            // until the time has elapsed for the game engine to run
            Thread.Sleep(game.Options.MaxAnswerTime + GraceTime);
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Dispose();
                client = null;
            }
        }

    }
}
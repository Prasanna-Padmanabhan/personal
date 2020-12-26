using SharpJackApi.Contracts;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.IntegrationTests
{
    public class SharpJackApiClient : IDisposable
    {
        private HttpClient client;

        public SharpJackApiClient(string endpoint)
        {
            client = new HttpClient
            {
                BaseAddress = new Uri(endpoint)
            };
        }

        public Task<Player> AddPlayerAsync(string playerName)
        {
            return client.PostAsJsonAsync<string, Player>("players", playerName);
        }

        public Task<Player> GetPlayerAsync(int playerId)
        {
            return client.GetAsJsonAsync<Player>($"players/{playerId}");
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
using SharpJackApi.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.Tests
{
    public interface ITestGameClient : IGameClient
    {
        public Task TriggerEvaluationAsync(int gameId, CancellationToken token);
    }
}
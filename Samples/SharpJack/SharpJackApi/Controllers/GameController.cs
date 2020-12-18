using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharpJackApi.Models;
using SharpJackApi.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.Controllers
{
    [ApiController]
    [Route("game")]
    public class GameController : ControllerBase
    {
        private readonly GameService gameService = new GameService();
        private readonly ILogger<GameController> _logger;

        public GameController(ILogger<GameController> logger)
        {
            _logger = logger;
            Task.Run(() => gameService.RunAsync(CancellationToken.None));
        }

        [Route("players")]
        [HttpPost]
        public async Task<Player> AddPlayerAsync([FromBody] string playerName)
        {
            return await gameService.AddPlayerAsync(playerName);
        }

        [Route("players/{playerId}")]
        [HttpPost]
        public async Task<Player> GetPlayerAsync([FromRoute] int playerId)
        {
            return await gameService.GetPlayerAsync(playerId);
        }

        [HttpPost]
        public async Task<Game> CreateGameAsync([FromBody] GameOptions options)
        {
            return await gameService.CreateGameAsync(options);
        }

        [Route("{gameId}")]
        [HttpGet]
        public async Task<Game> GetGameAsync([FromRoute] int gameId)
        {
            return await gameService.GetGameAsync(gameId);
        }

        [Route("{gameId}")]
        [HttpPut]
        public async Task JoinOrStartGameAsync([FromRoute] int gameId, [FromBody] Player player)
        {
            await gameService.JoinOrStartGameAsync(gameId, player);
        }

        [Route("{gameId}/questions")]
        [HttpGet]
        public async Task<Question> GetActiveQuestionAsync([FromRoute] int gameId, [FromBody] Player player)
        {
            return await gameService.GetActiveQuestionAsync(gameId, player);
        }

        [Route("{gameId}/questions")]
        [HttpPost]
        public async Task AskQuestionAsync([FromRoute] int gameId, [FromBody] Question question)
        {
            await gameService.AskQuestionAsync(gameId, question);
        }

        [Route("{gameId}/questions")]
        [HttpPut]
        public async Task<Answer> SubmitAnswerAsync([FromRoute] int gameId, [FromBody] Answer answer)
        {
            return await gameService.SubmitAnswerAsync(gameId, answer);
        }

        [Route("{gameId}/board")]
        [HttpGet]
        public async Task<LeaderBoard> GetBoardAsync([FromRoute] int gameId)
        {
            return await gameService.GetBoardAsync(gameId);
        }
    }
}
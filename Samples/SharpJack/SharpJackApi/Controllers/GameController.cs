using GameOptions = SharpJackApi.Contracts.GameOptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharpJackApi.Models;
using SharpJackApi.Services;
using System.Threading;
using System.Threading.Tasks;
using SharpJackApi.Data;

namespace SharpJackApi.Controllers
{
    [ApiController]
    [Route("game")]
    public class GameController : ControllerBase
    {
        private readonly GameService gameService;
        private readonly ILogger<GameController> _logger;

        public GameController(ILogger<GameController> logger, GameContext context)
        {
            _logger = logger;
            gameService = new GameService(context);
        }

        [Route("players")]
        [HttpPost]
        public Task<Player> AddPlayerAsync([FromBody] string playerName, CancellationToken token)
        {
            return gameService.AddPlayerAsync(playerName, token);
        }

        [Route("players/{playerId}")]
        [HttpGet]
        public Task<Player> GetPlayerAsync([FromRoute] int playerId, CancellationToken token)
        {
            return gameService.GetPlayerAsync(playerId, token);
        }

        [HttpPost]
        public Task<Game> CreateGameAsync([FromBody] GameOptions options, CancellationToken token)
        {
            return gameService.CreateGameAsync(options, token);
        }

        [Route("{gameId}")]
        [HttpGet]
        public Task<Game> GetGameAsync([FromRoute] int gameId, CancellationToken token)
        {
            return gameService.GetGameAsync(gameId, token);
        }

        [Route("{gameId}")]
        [HttpPut]
        public Task JoinOrStartGameAsync([FromRoute] int gameId, [FromBody] Player player, CancellationToken token)
        {
            return gameService.JoinOrStartGameAsync(gameId, player, token);
        }

        [Route("{gameId}/questions")]
        [HttpGet]
        public Task<Question> GetActiveQuestionAsync([FromRoute] int gameId, [FromBody] Player player, CancellationToken token)
        {
            return gameService.GetActiveQuestionAsync(gameId, player, token);
        }

        [Route("{gameId}/questions")]
        [HttpPost]
        public Task AskQuestionAsync([FromRoute] int gameId, [FromBody] Question question, CancellationToken token)
        {
            return gameService.AskQuestionAsync(gameId, question, token);
        }

        [Route("{gameId}/questions")]
        [HttpPut]
        public Task<Answer> SubmitAnswerAsync([FromRoute] int gameId, [FromBody] Answer answer, CancellationToken token)
        {
            return gameService.SubmitAnswerAsync(gameId, answer, token);
        }

        [Route("{gameId}/board")]
        [HttpGet]
        public Task<LeaderBoard> GetBoardAsync([FromRoute] int gameId, CancellationToken token)
        {
            return gameService.GetBoardAsync(gameId, token);
        }
    }
}
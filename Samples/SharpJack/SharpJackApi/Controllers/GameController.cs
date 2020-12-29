using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharpJackApi.Contracts;
using SharpJackApi.Data;
using SharpJackApi.Interfaces;
using SharpJackApi.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.Controllers
{
    /// <summary>
    /// The main controller serving the API requests.
    /// </summary>
    [ApiController]
    [Route("game")]
    public class GameController : ControllerBase, IGameClient
    {
        private readonly ILogger<GameController> _logger;
        private GameService gameService;

        /// <summary>
        /// Initializes the controller.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        /// <param name="context">The game context to act on.</param>
        public GameController(ILogger<GameController> logger, GameContext context)
        {
            _logger = logger;
            gameService = new GameService(context, logger);
        }

        [Route("~/")]
        public Task GetHomePageAsync()
        {
            return Task.FromResult(string.Empty);
        }

        /// <summary>
        /// Add a new player.
        /// </summary>
        /// <param name="playerName">The name of the player to add.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The newly added player.</returns>
        [Route("~/players")]
        [HttpPost]
        public Task<Player> AddPlayerAsync([FromBody] string playerName, CancellationToken token)
        {
            return gameService.AddPlayerAsync(playerName, token);
        }

        /// <summary>
        /// Get details of a given player.
        /// </summary>
        /// <param name="playerId">The ID of the player to fetch.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The player asked for.</returns>
        [Route("~/players/{playerId}")]
        [HttpGet]
        public Task<Player> GetPlayerAsync([FromRoute] int playerId, CancellationToken token)
        {
            return gameService.GetPlayerAsync(playerId, token);
        }

        /// <summary>
        /// Creates a new game.
        /// </summary>
        /// <param name="options">The options for the game.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The newly created game.</returns>
        [HttpPost]
        public Task<Game> CreateGameAsync([FromBody] GameOptions options, CancellationToken token)
        {
            return gameService.CreateGameAsync(options, token);
        }

        /// <summary>
        /// Get details of a given game.
        /// </summary>
        /// <param name="gameId">The ID of the game to fetch.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The game asked for.</returns>
        [Route("{gameId}")]
        [HttpGet]
        public Task<Game> GetGameAsync([FromRoute] int gameId, CancellationToken token)
        {
            return gameService.GetGameAsync(gameId, token);
        }

        /// <summary>
        /// Join or start an existing game.
        /// </summary>
        /// <remarks>
        /// If this is called by the player who created the game, then the game will be started (and state will become Active).
        /// If this is called by any other player, then the player will be added to the game (i.e. joined) and the state will remain Created.
        /// </remarks>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="player">The player joining or starting the game.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Nothing</returns>
        [Route("{gameId}")]
        [HttpPut]
        public Task JoinOrStartGameAsync([FromRoute] int gameId, [FromBody] Player player, CancellationToken token)
        {
            return gameService.JoinOrStartGameAsync(gameId, player, token);
        }

        /// <summary>
        /// Get the currently active question of a given game.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="player">The player requesting the question.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The active question.</returns>
        [Route("{gameId}/questions")]
        [HttpGet]
        public Task<Question> GetActiveQuestionAsync([FromRoute] int gameId, [FromBody] Player player, CancellationToken token)
        {
            return gameService.GetActiveQuestionAsync(gameId, player, token);
        }

        /// <summary>
        /// Submit a new question to the game.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="question">The question being asked.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Nothing</returns>
        [Route("{gameId}/questions")]
        [HttpPost]
        public Task AskQuestionAsync([FromRoute] int gameId, [FromBody] Question question, CancellationToken token)
        {
            return gameService.AskQuestionAsync(gameId, question, token);
        }

        /// <summary>
        /// Submit an answer to the currently active question.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="answer">The answer.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The correct answer.</returns>
        /// <remarks>
        /// Since this method will return the correct answer, it implies that a player can only submit one answer to a question.
        /// </remarks>
        [Route("{gameId}/questions")]
        [HttpPut]
        public Task<Answer> SubmitAnswerAsync([FromRoute] int gameId, [FromBody] Answer answer, CancellationToken token)
        {
            return gameService.SubmitAnswerAsync(gameId, answer, token);
        }

        /// <summary>
        /// Get the leader board for a given game.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The leader board.</returns>
        [Route("{gameId}/board")]
        [HttpGet]
        public Task<LeaderBoard> GetBoardAsync([FromRoute] int gameId, CancellationToken token)
        {
            return gameService.GetBoardAsync(gameId, token);
        }

        public Task EndGameAsync(int gameId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (gameService != null)
            {
                gameService.Dispose();
                gameService = null;
            }
        }
    }
}
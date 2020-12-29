using Microsoft.Extensions.Logging;
using SharpJackApi.Data;
using SharpJackApi.Interfaces;
using SharpJackApi.Models;
using SharpJackApi.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GameOptions = SharpJackApi.Contracts.GameOptions;
using GameState = SharpJackApi.Contracts.GameState;

// needed by unit tests to access GameContext
[assembly: InternalsVisibleTo("SharpJackApi.Tests")]

namespace SharpJackApi.Services
{
    /// <summary>
    /// Contains the business logic of the game.
    /// </summary>
    /// <remarks>
    /// This is implemented separately from the controller so it can be unit tested thoroughly in isolation.
    /// </remarks>
    public class GameService : IGameClient
    {
        /// <summary>
        /// Minimum number of players for a game.
        /// </summary>
        const int MinimumPlayers = 2;

        /// <summary>
        /// The database context to use to read/write state.
        /// </summary>
        /// <remarks>
        /// This is accessed by unit tests to do validation.
        /// </remarks>
        internal GameContext Context { get; private set; }

        /// <summary>
        /// The time service to use to keep track of time.
        /// </summary>
        /// <remarks>
        /// This abstraction is introduced so tests can manipulate current time for predictable results.
        /// </remarks>
        internal TimeService TimeService { get; private set; }

        private readonly ILogger logger;

        /// <summary>
        /// Initialize with the given context.
        /// </summary>
        /// <param name="context">The database context.</param>
        public GameService(GameContext context, ILogger logger)
        {
            Context = context;
            TimeService = new TimeService();
            this.logger = logger;
        }

        /// <summary>
        /// Add a new player.
        /// </summary>
        /// <param name="playerName">The name of the player to add.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The newly added player.</returns>
        public async Task<Contracts.Player> AddPlayerAsync(string playerName, CancellationToken token)
        {
            var player = await Context.Players.AddAsync(new Player { Name = playerName }, token);
            await Context.SaveChangesAsync(token);
            return player.Entity.ToContract();
        }

        /// <summary>
        /// Get details of a given player.
        /// </summary>
        /// <param name="playerId">The ID of the player to fetch.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The player asked for.</returns>
        public async Task<Contracts.Player> GetPlayerAsync(int playerId, CancellationToken token)
        {
            var p = await Context.GetPlayerAsync(playerId, token);
            return p.ToContract();
        }

        /// <summary>
        /// Creates a new game.
        /// </summary>
        /// <param name="options">The options for the game.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The newly created game.</returns>
        public async Task<Contracts.Game> CreateGameAsync(GameOptions options, CancellationToken token)
        {
            var player = await Context.GetPlayerAsync(options.PlayerId, token);
            var game = new Game { Options = options, State = GameState.Created };
            game.Players.Add(player);
            game.Board.Rows.Add(new Row { Player = player, PlayerScore = 0 });
            var g = await Context.Games.AddAsync(game, token);
            await Context.SaveChangesAsync(token);
            return g.Entity.ToContract();
        }

        /// <summary>
        /// Get details of a given game.
        /// </summary>
        /// <param name="gameId">The ID of the game to fetch.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The game asked for.</returns>
        public async Task<Contracts.Game> GetGameAsync(int gameId, CancellationToken token)
        {
            var g = await Context.GetGameAsync(gameId, token);
            return g.ToContract();
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
        public async Task JoinOrStartGameAsync(int gameId, Contracts.Player player, CancellationToken token)
        {
            var p = await Context.GetPlayerAsync(player.Id, token);

            var game = await Context.GetGameAsync(gameId, token);
            if (game.State == GameState.Completed)
            {
                throw new InvalidOperationException("Game over");
            }
            else if (game.Players.Count > game.Options.MaxPlayers)
            {
                throw new InvalidOperationException("Game full");
            }
            else if (game.Options.PlayerId == p.Id)
            {
                // can't start a game without minimum players
                if (game.Players.Count < MinimumPlayers)
                {
                    throw new InvalidOperationException("Too few players");
                }

                game.ActivePlayer = p.Id;
                game.ActiveUntil = TimeService.CurrentTime.AddSeconds(game.Options.MaxQuestionTime);
                game.ActiveQuestion = null;
                game.Answers.Clear();
                game.CurrentRound = 0;
                game.State = GameState.Active;
            }
            else if (!game.Players.Contains(p))
            {
                game.Players.Add(p);
                game.Board.Rows.Add(new Row { Player = p, PlayerScore = 0 });
            }

            await Context.SaveChangesAsync(token);
        }

        /// <summary>
        /// Get the currently active question of a given game.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="player">The player requesting the question.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The active question.</returns>
        public async Task<Contracts.Question> GetActiveQuestionAsync(int gameId, Contracts.Player player, CancellationToken token)
        {
            var p = await Context.GetPlayerAsync(player.Id, token);

            var game = await Context.GetGameAsync(gameId, token);
            if (!game.Players.Contains(p))
            {
                throw new InvalidOperationException("Not your game");
            }
            if (game.State != GameState.Active)
            {
                throw new InvalidOperationException("Not an active game");
            }

            var question = game.ActiveQuestion;
            if (question == null)
            {
                throw new KeyNotFoundException("No active question");
            }

            return new Contracts.Question { Title = question.Title };
        }

        /// <summary>
        /// Submit a new question to the game.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="question">The question being asked.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Nothing</returns>
        public async Task AskQuestionAsync(int gameId, Contracts.Question question, CancellationToken token)
        {
            var player = await Context.GetPlayerAsync(question.PlayerId, token);

            var game = await Context.GetGameAsync(gameId, token);

            if (!game.Players.Exists(p => p.Id == question.PlayerId))
            {
                throw new InvalidOperationException("Not your game");
            }
            if (game.State != GameState.Active)
            {
                throw new InvalidOperationException("Not an active game");
            }

            var now = TimeService.CurrentTime;
            if (game.ActivePlayer != question.PlayerId || now > game.ActiveUntil)
            {
                throw new InvalidOperationException("Not your turn");
            }

            game.ActiveQuestion = question.ToModel();
            game.ActiveUntil = now.AddSeconds(game.Options.MaxAnswerTime);

            await Context.SaveChangesAsync(token);
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
        public async Task<Contracts.Answer> SubmitAnswerAsync(int gameId, Contracts.Answer answer, CancellationToken token)
        {
            var player = await Context.GetPlayerAsync(answer.PlayerId, token);

            var game = await Context.GetGameAsync(gameId, token);

            if (!game.Players.Exists(p => p.Id == answer.PlayerId))
            {
                throw new InvalidOperationException("Not your game");
            }
            if (game.State != GameState.Active)
            {
                throw new InvalidOperationException("Not an active game");
            }

            var now = TimeService.CurrentTime;
            if (game.ActivePlayer != answer.PlayerId)
            {
                if (now > game.ActiveUntil)
                {
                    throw new InvalidOperationException("Too late");
                }
                else
                {
                    // TODO: enforce only one answer per player per question
                    var a = answer.ToModel();
                    a.SubmitTime = now;
                    game.Answers.Add(a);
                }
            }

            await Context.SaveChangesAsync(token);

            // return the correct answer
            return new Contracts.Answer { PlayerId = game.ActivePlayer, Value = game.ActiveQuestion.Answer };
        }

        /// <summary>
        /// Get the leader board for a given game.
        /// </summary>
        /// <param name="gameId">The ID of the game.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The leader board.</returns>
        public async Task<Contracts.LeaderBoard> GetBoardAsync(int gameId, CancellationToken token)
        {
            var g = await Context.GetGameAsync(gameId, token);
            await EvaluateAsync(g, token);
            return g.Board.ToContract();
        }

        /// <summary>
        /// The game engine evaluating the answers and computing scores.
        /// </summary>
        /// <param name="game">The game to evaluate.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Nothing</returns>
        public async Task EvaluateAsync(Game game, CancellationToken token)
        {
            // time to evaluate
            if (game.State == GameState.Active && TimeService.CurrentTime >= game.ActiveUntil)
            {
                // time to evaluate answers to the active question
                if (game.ActiveQuestion != null)
                {
                    // evaluate answers and assign scores
                    var orderedAnswers = game.Answers.OrderBy(a => Math.Abs(a.Value - game.ActiveQuestion.Answer));
                    int count = game.Answers.Count;
                    foreach (var answer in orderedAnswers)
                    {
                        answer.Score = count * 2;
                        --count;
                    }

                    // update leaderboard
                    foreach (var row in game.Board.Rows)
                    {
                        // except for the person asking the question
                        if (row.Player.Id != game.ActivePlayer)
                        {
                            row.PlayerScore += orderedAnswers.First(a => a.PlayerId == row.Player.Id).Score;
                        }
                        else
                        {
                            row.PlayerScore += game.Answers.Count;
                        }
                    }

                    // clear existing answers
                    game.Answers.Clear();

                    // reset active question
                    game.ActiveQuestion = null;
                }

                // get the position of the current player
                var index = game.Players.FindIndex(p => p.Id == game.ActivePlayer);

                // move to the next player
                game.ActivePlayer = game.Players[(index + 1) % game.Players.Count].Id;

                // reset ActiveUntil
                game.ActiveUntil = TimeService.CurrentTime.AddSeconds(game.Options.MaxQuestionTime);

                // advance to next round if necessary
                if (game.ActivePlayer == game.Players[0].Id)
                {
                    ++game.CurrentRound;
                }

                // set Game to completed state if necessary
                if (game.CurrentRound == game.Options.MaxRounds)
                {
                    game.State = GameState.Completed;
                }

                await Context.SaveChangesAsync(token);
            }
        }

        public virtual Task EndGameAsync(int gameId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Release resources.
        /// </summary>
        public void Dispose()
        {
            if (Context != null)
            {
                Context.Dispose();
                Context = null;
            }
        }
    }
}
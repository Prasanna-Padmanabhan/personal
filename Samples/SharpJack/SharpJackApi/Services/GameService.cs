using GameOptions = SharpJackApi.Contracts.GameOptions;
using GameState = SharpJackApi.Contracts.GameState;
using SharpJackApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpJackApi.Data;
using Microsoft.EntityFrameworkCore;

namespace SharpJackApi.Services
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

    public class GameService : IDisposable
    {
        const int MinimumPlayers = 2;

        GameContext context;

        public TimeService TimeService { get; private set; }

        public GameService(GameContext context)
        {
            this.context = context;
            TimeService = new TimeService();
        }

        public async Task<Player> AddPlayerAsync(string playerName, CancellationToken token)
        {
            var player = await context.Players.AddAsync(new Player { Name = playerName }, token);
            await context.SaveChangesAsync(token);
            return player.Entity;
        }

        public Task<Player> GetPlayerAsync(int playerId, CancellationToken token)
        {
            return context.GetPlayerAsync(playerId, token);
        }

        public async Task<Game> CreateGameAsync(GameOptions options, CancellationToken token)
        {
            var player = await context.GetPlayerAsync(options.PlayerId, token);
            var game = new Game { Options = options, State = GameState.Created };
            game.Players.Add(player);
            game.Board.Rows.Add(new Row { PlayerId = player.Id, PlayerScore = 0 });
            var g = await context.Games.AddAsync(game, token);
            await context.SaveChangesAsync(token);
            return g.Entity;
        }

        public Task<Game> GetGameAsync(int gameId, CancellationToken token)
        {
            return context.GetGameAsync(gameId, token);
        }

        public async Task JoinOrStartGameAsync(int gameId, Player player, CancellationToken token)
        {
            player = await context.GetPlayerAsync(player.Id, token);

            var game = await context.GetGameAsync(gameId, token);
            if (game.State == GameState.Completed)
            {
                throw new InvalidOperationException("Game over");
            }
            else if (game.Players.Count > game.Options.MaxPlayers)
            {
                throw new InvalidOperationException("Game full");
            }
            else if (game.Options.PlayerId == player.Id)
            {
                // can't start a game without minimum players
                if (game.Players.Count < MinimumPlayers)
                {
                    throw new InvalidOperationException("Too few players");
                }

                game.ActivePlayer = player.Id;
                game.ActiveUntil = TimeService.CurrentTime.AddSeconds(game.Options.MaxQuestionTime);
                game.ActiveQuestion = null;
                game.Answers.Clear();
                game.CurrentRound = 0;
                // done at the end to avoid race conditions with the engine which checks for this state
                game.State = GameState.Active;
            }
            else if (!game.Players.Contains(player))
            {
                game.Players.Add(player);
                game.Board.Rows.Add(new Row { PlayerId = player.Id, PlayerScore = 0 });
            }

            await context.SaveChangesAsync(token);
        }

        public async Task<Question> GetActiveQuestionAsync(int gameId, Player player, CancellationToken token)
        {
            player = await context.GetPlayerAsync(player.Id, token);

            var game = await context.GetGameAsync(gameId, token);
            if (!game.Players.Contains(player))
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

            return new Question { Title = question.Title };
        }

        public async Task AskQuestionAsync(int gameId, Question question, CancellationToken token)
        {
            var player = await context.GetPlayerAsync(question.PlayerId, token);

            var game = await context.GetGameAsync(gameId, token);

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

            game.ActiveQuestion = question;
            game.ActiveUntil = now.AddSeconds(game.Options.MaxAnswerTime);

            await context.SaveChangesAsync(token);
        }

        public async Task<Answer> SubmitAnswerAsync(int gameId, Answer answer, CancellationToken token)
        {
            var player = await context.GetPlayerAsync(answer.PlayerId, token);

            var game = await context.GetGameAsync(gameId, token);

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
                    answer.SubmitTime = now;
                    game.Answers.Add(answer);
                }
            }

            await context.SaveChangesAsync(token);

            // return the correct answer
            return new Answer { PlayerId = game.ActivePlayer, Value = game.ActiveQuestion.Answer };
        }

        public async Task<LeaderBoard> GetBoardAsync(int gameId, CancellationToken token)
        {
            await EvaluateAsync(token);
            var g = await context.GetGameAsync(gameId, token);
            return g.Board;
        }

        public async Task EvaluateAsync(CancellationToken token)
        {
            await foreach (var game in context.Games.AsAsyncEnumerable())
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
                            answer.Score = count + (int)(game.ActiveUntil - answer.SubmitTime).TotalSeconds;
                            --count;
                        }

                        // update leaderboard
                        foreach (var row in game.Board.Rows)
                        {
                            // except for the person asking the question
                            if (row.PlayerId != game.ActivePlayer)
                            {
                                row.PlayerScore += orderedAnswers.First(a => a.PlayerId == row.PlayerId).Score;
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
                }
            }

            await context.SaveChangesAsync(token);
        }

        public void Dispose()
        {
            if (context != null)
            {
                context.Dispose();
                context = null;
            }
        }
    }
}
using GameOptions = SharpJackApi.Contracts.GameOptions;
using GameState = SharpJackApi.Contracts.GameState;
using SharpJackApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.Services
{
    public class GameService
    {
        const int MinimumPlayers = 2;
        readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
        readonly List<Game> games = new List<Game>();
        readonly List<Player> players = new List<Player>();
        readonly TimeService timeService = new TimeService();

        public TimeService TimeService => timeService;

        public event EventHandler<Game> EvaluationCompleted;

        public async Task<Player> AddPlayerAsync(string playerName)
        {
            var player = new Player { Id = players.Count, Name = playerName };
            players.Add(player);
            return await Task.FromResult(player);
        }

        public async Task<Player> GetPlayerAsync(int playerId)
        {
            return await Task.FromResult(players[playerId]);
        }

        public async Task<Game> CreateGameAsync(GameOptions options)
        {
            var player = players.FirstOrDefault(p => p.Id == options.PlayerId);
            if (player == null)
            {
                throw new InvalidOperationException("Player not found");
            }

            var game = new Game { Id = games.Count, Options = options, State = GameState.Created };
            game.Players.Add(player.Id);
            game.Board.Rows.Add(new Row { PlayerId = player.Id, PlayerScore = 0 });
            games.Add(game);
            return await Task.FromResult(game);
        }

        public async Task<Game> GetGameAsync(int gameId)
        {
            return await Task.FromResult(games[gameId]);
        }

        public async Task JoinOrStartGameAsync(int gameId, Player player)
        {
            if (players.FirstOrDefault(p => p.Id == player.Id) == null)
            {
                throw new InvalidOperationException("Player not found");
            }

            var game = games[gameId];
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
                game.ActiveUntil = timeService.CurrentTime.AddSeconds(game.Options.MaxQuestionTime);
                game.ActiveQuestion = null;
                game.Answers.Clear();
                game.CurrentRound = 0;
                // done at the end to avoid race conditions with the engine which checks for this state
                game.State = GameState.Active;
            }
            else if (!game.Players.Contains(player.Id))
            {
                game.Players.Add(player.Id);
                game.Board.Rows.Add(new Row { PlayerId = player.Id, PlayerScore = 0 });
            }

            await Task.CompletedTask;
        }

        public async Task<Question> GetActiveQuestionAsync(int gameId, Player player)
        {
            if (players.FirstOrDefault(p => p.Id == player.Id) == null)
            {
                throw new InvalidOperationException("Player not found");
            }

            var game = games[gameId];
            if (!game.Players.Contains(player.Id))
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

            var now = timeService.CurrentTime;

            // only return the answer after the time has elapsed
            var result = now < game.ActiveUntil ? new Question { Title = question.Title } : question;

            return await Task.FromResult(result);
        }

        public async Task AskQuestionAsync(int gameId, Question question)
        {
            if (players.FirstOrDefault(p => p.Id == question.PlayerId) == null)
            {
                throw new InvalidOperationException("Player not found");
            }

            var game = games[gameId];
            if (!game.Players.Contains(question.PlayerId))
            {
                throw new InvalidOperationException("Not your game");
            }
            if (game.State != GameState.Active)
            {
                throw new InvalidOperationException("Not an active game");
            }

            var now = timeService.CurrentTime;
            if (game.ActivePlayer != question.PlayerId || now > game.ActiveUntil)
            {
                throw new InvalidOperationException("Not your turn");
            }

            game.ActiveQuestion = question;
            game.ActiveUntil = now.AddSeconds(game.Options.MaxAnswerTime);

            await Task.CompletedTask;
        }

        public async Task<Answer> SubmitAnswerAsync(int gameId, Answer answer)
        {
            if (players.FirstOrDefault(p => p.Id == answer.PlayerId) == null)
            {
                throw new InvalidOperationException("Player not found");
            }

            var game = games[gameId];
            if (!game.Players.Contains(answer.PlayerId))
            {
                throw new InvalidOperationException("Not your game");
            }
            if (game.State != GameState.Active)
            {
                throw new InvalidOperationException("Not an active game");
            }

            var now = timeService.CurrentTime;
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

            // return the correct answer
            var result = new Answer { PlayerId = game.ActivePlayer, Value = game.ActiveQuestion.Answer };

            return await Task.FromResult(result);
        }

        public async Task<LeaderBoard> GetBoardAsync(int gameId)
        {
            return await Task.FromResult(games[gameId].Board);
        }

        public Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                timeService.Sleep(OneSecond);
                foreach (var game in games)
                {
                    // time to evaluate
                    if (game.State == GameState.Active && timeService.CurrentTime >= game.ActiveUntil)
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

                        // move to the next player
                        game.ActivePlayer = game.Players[(game.ActivePlayer + 1) % game.Players.Count];

                        // reset ActiveUntil
                        game.ActiveUntil = timeService.CurrentTime.AddSeconds(game.Options.MaxQuestionTime);

                        // advance to next round if necessary
                        if (game.ActivePlayer == game.Players[0])
                        {
                            ++game.CurrentRound;
                        }

                        // set Game to completed state if necessary
                        if (game.CurrentRound == game.Options.MaxRounds)
                        {
                            game.State = GameState.Completed;
                        }

                        // signal that the evaluation is now complete
                        EvaluationCompleted?.Invoke(this, game);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
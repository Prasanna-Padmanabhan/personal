using GameOptions = SharpJackApi.Contracts.GameOptions;
using GameState = SharpJackApi.Contracts.GameState;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJackApi.Models;
using SharpJackApi.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.UnitTests
{
    public class SimpleGame : IDisposable
    {
        private static Func<SimpleGame, Player, SimplePlayer> newSimplePlayer;
        private readonly GameService service;
        private CancellationTokenSource source;
        private Task engine;
        private Game game;
        private Player creator;

        private SimpleGame()
        {
            service = new GameService();
            service.TimeService.CurrentTime = DateTime.UtcNow;
            // would ideally do nothing, but that would cause a busy loop, so just do enough to yield and not hammer CPU
            service.TimeService.SleepAction = span => Task.Yield();
            source = new CancellationTokenSource();
            engine = Task.Run(() => service.RunAsync(source.Token));
            service.EvaluationCompleted += Evaluation.OnEvaluationCompleted;

            // trigger static constructor
            SimplePlayer.Touch();
        }

        public static (SimpleGame, SimplePlayer) Create(string player, int maxPlayers, int maxQuestionTime = 1, int maxAnswerTime = 1, int maxRounds = 1)
        {
            var g = new SimpleGame();

            g.creator = g.service.AddPlayerAsync(player).Result;
            var options = new GameOptions { PlayerId = g.creator.Id, MaxPlayers = maxPlayers, MaxQuestionTime = maxQuestionTime, MaxAnswerTime = maxAnswerTime, MaxRounds = maxRounds };
            g.game = g.service.CreateGameAsync(options).Result;

            return (g, newSimplePlayer(g, g.creator));
        }

        public void Start()
        {
            service.JoinOrStartGameAsync(game.Id, creator).Wait();
            Assert.AreEqual(GameState.Active, game.State);
        }

        public SimplePlayer Join(string player)
        {
            var p = service.AddPlayerAsync(player).Result;
            service.JoinOrStartGameAsync(game.Id, p).Wait();
            Assert.AreEqual(GameState.Created, game.State);
            return newSimplePlayer(this, p);
        }

        public void Ask(Player player, string question, int answer)
        {
            service.AskQuestionAsync(game.Id, new Question { PlayerId = player.Id, Title = question, Answer = answer }).Wait();
            Assert.AreEqual(answer, game.ActiveQuestion.Answer);
            Assert.AreEqual(0, game.Answers.Count);
        }

        public void Answer(Player player, int answer)
        {
            var count = game.Answers.Count;
            var result = service.SubmitAnswerAsync(game.Id, new Answer { PlayerId = player.Id, Value = answer }).Result;
            Assert.AreEqual(count + 1, game.Answers.Count);
            Assert.AreEqual(answer, game.Answers[count].Value);
            Assert.AreEqual(player.Id, game.Answers[count].PlayerId);
            Assert.AreEqual(game.ActiveQuestion.Answer, result.Value);
        }

        public void Score(Player player, int score)
        {
            // trigger evaluation when all answers are in
            if (game.Answers.Count == game.Players.Count - 1)
            {
                // Advance the time so the game engine can evaluate results
                service.TimeService.CurrentTime += TimeSpan.FromSeconds(game.Options.MaxAnswerTime);

                Evaluation.CompletedAsync(game.Id).Wait();
            }

            // Check if the leaderboard is updated
            var board = service.GetBoardAsync(game.Id).Result;
            Assert.AreEqual(game.Players.Count, board.Rows.Count);
            Assert.AreEqual(score, board.Rows.First(r => r.PlayerId == player.Id).PlayerScore);
        }

        public void End()
        {
            Assert.AreEqual(GameState.Completed, game.State);
            source.Cancel();
            engine.Wait();
        }

        public void Dispose()
        {
            if (engine != null)
            {
                engine.Dispose();
                engine = null;
            }

            if (source != null)
            {
                source.Dispose();
                source = null;
            }
        }

        public class SimplePlayer
        {
            private readonly SimpleGame game;
            private readonly Player player;

            static SimplePlayer()
            {
                newSimplePlayer = (g, p) => new SimplePlayer(g, p);
            }

            public static void Touch()
            {
                //a dummy method to force execution of the static constructor
            }

            private SimplePlayer(SimpleGame game, Player player)
            {
                this.game = game;
                this.player = player;
            }

            public void Asks(string question, int answer)
            {
                game.Ask(player, question, answer);
            }

            public void Answers(int answer)
            {
                game.Answer(player, answer);
            }

            public void Scores(int score)
            {
                game.Score(player, score);
            }
        }
    }
}
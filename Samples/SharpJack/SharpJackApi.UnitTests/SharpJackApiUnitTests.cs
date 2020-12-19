using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJackApi.Models;
using SharpJackApi.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.UnitTests
{
    public class TGame : IDisposable
    {
        private readonly GameService service;
        private Task engine;
        private Game game;
        private Player creator;

        private TGame()
        {
            service = new GameService();
            service.TimeService.CurrentTime = DateTime.UtcNow;
            // would ideally do nothing, but that would cause a busy loop, so just do enough to yield and not hammer CPU
            service.TimeService.SleepAction = span => Thread.Sleep(TimeSpan.FromMilliseconds(1));
            engine = Task.Run(() => service.RunAsync(CancellationToken.None));
        }

        public static (TGame, Player) Create(string player, int maxPlayers, int maxQuestionTime = 1, int maxAnswerTime = 1, int maxRounds = 1)
        {
            var g = new TGame();

            g.creator = g.service.AddPlayerAsync(player).Result;
            var options = new GameOptions { PlayerId = g.creator.Id, MaxPlayers = maxPlayers, MaxQuestionTime = maxQuestionTime, MaxAnswerTime = maxAnswerTime, MaxRounds = maxRounds };
            g.game = g.service.CreateGameAsync(options).Result;
            PlayerExtensions.Game = g;

            return (g, g.creator);
        }

        public void Start()
        {
            service.JoinOrStartGameAsync(game.Id, creator).Wait();
            Assert.AreEqual(GameState.Active, game.State);
        }

        public Player Join(string player)
        {
            var p = service.AddPlayerAsync(player).Result;
            service.JoinOrStartGameAsync(game.Id, p).Wait();
            Assert.AreEqual(GameState.Created, game.State);
            return p;
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
            // Advance the time so the game engine can evaluate results
            service.TimeService.CurrentTime += TimeSpan.FromSeconds(game.Options.MaxAnswerTime);

            service.EvaluationCompleted += Evaluation.OnEvaluationCompleted;
            Evaluation.CompletedAsync(game.Id).Wait();

            // Check if the leaderboard is updated
            var board = service.GetBoardAsync(game.Id).Result;
            Assert.AreEqual(game.Players.Count, board.Rows.Count);
            Assert.AreEqual(score, board.Rows.First(r => r.PlayerId == player.Id).PlayerScore);
        }

        public void Dispose()
        {
            if (engine != null)
            {
                engine.Dispose();
                engine = null;
            }
        }
    }

    public static class PlayerExtensions
    {
        public static TGame Game { get; set; }

        public static void Ask(this Player player, string question, int answer)
        {
            Game.Ask(player, question, answer);
        }

        public static void Answer(this Player player, int answer)
        {
            Game.Answer(player, answer);
        }

        public static void Score(this Player player, int score)
        {
            Game.Score(player, score);
        }
    }

    [TestClass]
    public class SharpJackApiUnitTests
    {
        [TestMethod]
        public void TwoPlayers()
        {
            (var game, var ben) = TGame.Create("Benjamin Sisko", 2);
            var jake = game.Join("Jake Sisko");
            game.Start();
            ben.Ask("How many seasons of Star Trek: Deep Space Nine?", 7);
            jake.Answer(5);
            jake.Score(2);
            jake.Ask("How many episodes of DS9?", 176);
            ben.Answer(180);
            ben.Score(2);
        }
        
        [TestMethod]
        public async Task MultiPlayerAsync()
        {
            var service = new GameService();
            service.TimeService.CurrentTime = DateTime.UtcNow;
            // would ideally do nothing, but that would cause a busy loop, so just do enough to yield and not hammer CPU
            service.TimeService.SleepAction = span => Thread.Sleep(TimeSpan.FromMilliseconds(1));
            using var source = new CancellationTokenSource();
            using var task = Task.Run(() => service.RunAsync(source.Token));

            // New Player Ben
            var ben = await service.AddPlayerAsync("Benjamin Sisko");

            // Ben creates a new game
            var options = new GameOptions { PlayerId = ben.Id, MaxPlayers = 2, MaxQuestionTime = 1, MaxAnswerTime = 1, MaxRounds = 1 };
            var game = await service.CreateGameAsync(options);

            // New Player Jake
            var jake = await service.AddPlayerAsync("Jake Sisko");

            // Jake joins Ben's game
            await service.JoinOrStartGameAsync(game.Id, jake);
            Assert.AreEqual(GameState.Created, game.State);

            // Ben starts the game
            await service.JoinOrStartGameAsync(game.Id, ben);
            Assert.AreEqual(GameState.Active, game.State);
            Assert.AreEqual(ben.Id, game.ActivePlayer);
            Assert.IsNull(game.ActiveQuestion);

            // Ben asks a question
            await service.AskQuestionAsync(game.Id, new Question { PlayerId = ben.Id, Title = "How many seasons of Star Trek: Deep Space Nine have aired?", Answer = 7 });
            Assert.AreEqual(7, game.ActiveQuestion.Answer);
            Assert.AreEqual(0, game.Answers.Count);

            // Jake posts an answer
            await service.SubmitAnswerAsync(game.Id, new Answer { PlayerId = jake.Id, Value = 5 });
            Assert.AreEqual(1, game.Answers.Count);
            Assert.AreEqual(5, game.Answers[0].Value);
            Assert.AreEqual(jake.Id, game.Answers[0].PlayerId);

            // Advance the time so the game engine can evaluate results
            service.TimeService.CurrentTime += TimeSpan.FromSeconds(game.Options.MaxAnswerTime);

            service.EvaluationCompleted += Evaluation.OnEvaluationCompleted;
            await Evaluation.CompletedAsync(game.Id);

            // Check if the leaderboard is updated
            var board = await service.GetBoardAsync(game.Id);
            Assert.AreEqual(2, board.Rows.Count);
            Assert.AreEqual(2, board.Rows.First(r => r.PlayerId == jake.Id).PlayerScore);

            // Jake is now the active player
            Assert.AreEqual(jake.Id, game.ActivePlayer);
            Assert.IsNull(game.ActiveQuestion);

            // Jake asks a question
            await service.AskQuestionAsync(game.Id, new Question { PlayerId = jake.Id, Title = "How many episodes of Star Trek: Deep Space Nine have aired?", Answer = 176 });
            Assert.AreEqual(176, game.ActiveQuestion.Answer);
            Assert.AreEqual(0, game.Answers.Count);

            // Ben submits an answer
            await service.SubmitAnswerAsync(game.Id, new Answer { PlayerId = ben.Id, Value = 180 });
            Assert.AreEqual(1, game.Answers.Count);
            Assert.AreEqual(180, game.Answers[0].Value);
            Assert.AreEqual(ben.Id, game.Answers[0].PlayerId);

            // Advance the time so the game engine can do its thing
            service.TimeService.CurrentTime += TimeSpan.FromSeconds(game.Options.MaxAnswerTime);

            // wait until evaluation has been completed
            await Evaluation.CompletedAsync(game.Id);
            Assert.AreEqual(GameState.Completed, game.State);

            // Check the leaderboard
            board = await service.GetBoardAsync(game.Id);
            Assert.AreEqual(2, board.Rows.Count);
            Assert.AreEqual(2, board.Rows.First(r => r.PlayerId == jake.Id).PlayerScore);
            Assert.AreEqual(2, board.Rows.First(r => r.PlayerId == ben.Id).PlayerScore);

            // Terminate the game engine
            source.Cancel();
            await task;
        }
    }

    public static class Evaluation
    {
        private static readonly ConcurrentDictionary<int, bool> completions = new ConcurrentDictionary<int, bool>();

        public static void OnEvaluationCompleted(object sender, Game e)
        {
            completions.AddOrUpdate(e.Id, true, (id, val) => true);
        }

        public static async Task CompletedAsync(int id)
        {
            while (!completions.GetValueOrDefault(id))
            {
                await Task.Yield();
            }
            completions.AddOrUpdate(id, false, (id, val) => false);
        }
    }
}
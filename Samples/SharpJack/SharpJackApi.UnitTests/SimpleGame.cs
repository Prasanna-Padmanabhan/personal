using GameOptions = SharpJackApi.Contracts.GameOptions;
using GameState = SharpJackApi.Contracts.GameState;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJackApi.Models;
using SharpJackApi.Services;
using System;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using SharpJackApi.Data;

namespace SharpJackApi.UnitTests
{
    public class SimpleGame : IDisposable
    {
        private const string ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=sharpjacktest;Trusted_Connection=True;MultipleActiveResultSets=true";
        private static Func<SimpleGame, Player, SimplePlayer> newSimplePlayer;
        private GameService service;
        private Game game;
        private Player creator;

        private SimpleGame()
        {
            var context = new GameContext(new DbContextOptionsBuilder<GameContext>().UseSqlServer(ConnectionString).Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            service = new GameService(context);
            service.TimeService.CurrentTime = DateTime.UtcNow;

            // trigger static constructor
            SimplePlayer.Touch();
        }

        public static (SimpleGame, SimplePlayer) Create(string player, int maxPlayers, int maxQuestionTime = 1, int maxAnswerTime = 1, int maxRounds = 1)
        {
            var g = new SimpleGame();

            g.creator = g.service.AddPlayerAsync(player, CancellationToken.None).Result;
            var options = new GameOptions { PlayerId = g.creator.Id, MaxPlayers = maxPlayers, MaxQuestionTime = maxQuestionTime, MaxAnswerTime = maxAnswerTime, MaxRounds = maxRounds };
            g.game = g.service.CreateGameAsync(options, CancellationToken.None).Result;

            return (g, newSimplePlayer(g, g.creator));
        }

        public void Start()
        {
            service.JoinOrStartGameAsync(game.Id, creator, CancellationToken.None).Wait();
            Assert.AreEqual(GameState.Active, game.State);
        }

        public SimplePlayer Join(string player)
        {
            var p = service.AddPlayerAsync(player, CancellationToken.None).Result;
            service.JoinOrStartGameAsync(game.Id, p, CancellationToken.None).Wait();
            Assert.AreEqual(GameState.Created, game.State);
            return newSimplePlayer(this, p);
        }

        public void Ask(Player player, string question, int answer)
        {
            service.AskQuestionAsync(game.Id, new Question { PlayerId = player.Id, Title = question, Answer = answer }, CancellationToken.None).Wait();
            Assert.AreEqual(answer, game.ActiveQuestion.Answer);
            Assert.AreEqual(0, game.Answers.Count);
        }

        public void Answer(Player player, int answer)
        {
            var count = game.Answers.Count;
            var result = service.SubmitAnswerAsync(game.Id, new Answer { PlayerId = player.Id, Value = answer }, CancellationToken.None).Result;
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
            }

            // Check if the leaderboard is updated
            var board = service.GetBoardAsync(game.Id, CancellationToken.None).Result;
            Assert.AreEqual(game.Players.Count, board.Rows.Count);
            Assert.AreEqual(score, board.Rows.First(r => r.PlayerId == player.Id).PlayerScore);
        }

        public void End()
        {
            Assert.AreEqual(GameState.Completed, game.State);
        }

        public void Dispose()
        {
            if (service != null)
            {
                service.Dispose();
                service = null;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJackApi.Contracts;
using SharpJackApi.Data;
using SharpJackApi.Services;
using System;
using System.Linq;
using System.Threading;

namespace SharpJackApi.UnitTests
{
    /// <summary>
    /// Represents a simplified version of the game used for testing.
    /// </summary>
    /// <remarks>
    /// Abstracts common ways to play the game and also adds validation
    /// as appropriate after each step to minimize the actual test code
    /// and make it more readable.
    /// </remarks>
    public class TestGame : IDisposable
    {
        /// <summary>
        /// Database connection string.
        /// </summary>
        /// <remarks>
        /// Tests are executed against a real database (SQL Express LocalDB) to make sure EF Core + LINQ
        /// commands will work as closely to a production database as possible.
        /// </remarks>
        private const string ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=sharpjacktest;Trusted_Connection=True;MultipleActiveResultSets=true";

        /// <summary>
        /// Delegate used to construct an instance of a TestPlayer.
        /// </summary>
        /// <remarks>
        /// We want only TestGame to have the ability to construct TestPlayer instances
        /// and this is the easiest way to simulate that.
        /// </remarks>
        /// <seealso cref="TestPlayer"/>
        private static Func<TestGame, Player, TestPlayer> newTestPlayer;

        /// <summary>
        /// The GameService instance to test against.
        /// </summary>
        private GameService service;

        /// <summary>
        /// The Game to test against.
        /// </summary>
        private Game game;

        /// <summary>
        /// The player creating the game.
        /// </summary>
        private Player creator;

        /// <summary>
        /// Private constructor to prevent initialization from outside the class.
        /// </summary>
        /// <seealso cref="Create(string, int, int, int, int)"/>
        private TestGame()
        {
            var context = new GameContext(new DbContextOptionsBuilder<GameContext>().UseSqlServer(ConnectionString).Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            service = new GameService(context, null);
            service.TimeService.CurrentTime = DateTime.UtcNow;

            // trigger static constructor
            TestPlayer.Touch();
        }

        /// <summary>
        /// Create a new player, and a new game with the given options.
        /// </summary>
        /// <param name="player">The name of the player.</param>
        /// <param name="maxPlayers">The maximum number of players.</param>
        /// <param name="maxQuestionTime">The maximum time to ask a question.</param>
        /// <param name="maxAnswerTime">The maximum time to answer a question.</param>
        /// <param name="maxRounds">The maximum number of rounds.</param>
        /// <returns>The newly created player and game.</returns>
        public static (TestGame, TestPlayer) Create(string player, int maxPlayers, int maxQuestionTime = 1, int maxAnswerTime = 1, int maxRounds = 1)
        {
            var g = new TestGame();

            g.creator = g.service.AddPlayerAsync(player, CancellationToken.None).Result;
            var options = new GameOptions { PlayerId = g.creator.Id, MaxPlayers = maxPlayers, MaxQuestionTime = maxQuestionTime, MaxAnswerTime = maxAnswerTime, MaxRounds = maxRounds };
            g.game = g.service.CreateGameAsync(options, CancellationToken.None).Result;

            return (g, newTestPlayer(g, g.creator));
        }

        /// <summary>
        /// Start the game.
        /// </summary>
        public void Start()
        {
            service.JoinOrStartGameAsync(game.Id, creator, CancellationToken.None).Wait();
            var g = service.Context.GetGameAsync(game.Id, CancellationToken.None).Result;
            Assert.AreEqual(GameState.Active, g.State);
        }

        /// <summary>
        /// Create a new player and have them join the game.
        /// </summary>
        /// <param name="player">The name of the player.</param>
        /// <returns>The newly created player.</returns>
        public TestPlayer Join(string player)
        {
            var p = service.AddPlayerAsync(player, CancellationToken.None).Result;
            service.JoinOrStartGameAsync(game.Id, p, CancellationToken.None).Wait();
            var g = service.Context.GetGameAsync(game.Id, CancellationToken.None).Result;
            Assert.AreEqual(GameState.Created, g.State);
            return newTestPlayer(this, p);
        }

        /// <summary>
        /// Submit a question from the player.
        /// </summary>
        /// <param name="player">The player asking the question.</param>
        /// <param name="question">The question being asked.</param>
        /// <param name="answer">The answer to the question.</param>
        public void Ask(Player player, string question, int answer)
        {
            service.AskQuestionAsync(game.Id, new Question { PlayerId = player.Id, Title = question, Answer = answer }, CancellationToken.None).Wait();
            var g = service.Context.GetGameAsync(game.Id, CancellationToken.None).Result;
            Assert.AreEqual(answer, g.ActiveQuestion.Answer);
            Assert.AreEqual(0, g.Answers.Count);
        }

        /// <summary>
        /// Submit an answer to the currently active question.
        /// </summary>
        /// <param name="player">The player submitting the answer.</param>
        /// <param name="answer">The answer being submitted.</param>
        public void Answer(Player player, int answer)
        {
            var g = service.Context.GetGameAsync(game.Id, CancellationToken.None).Result;
            var count = g.Answers.Count;
            var result = service.SubmitAnswerAsync(game.Id, new Answer { PlayerId = player.Id, Value = answer }, CancellationToken.None).Result;
            Assert.AreEqual(count + 1, g.Answers.Count);
            Assert.AreEqual(answer, g.Answers[count].Value);
            Assert.AreEqual(player.Id, g.Answers[count].PlayerId);
            Assert.AreEqual(g.ActiveQuestion.Answer, result.Value);
        }

        /// <summary>
        /// Validate that the game gave the given player the given score.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="score">The score to validate.</param>
        public void Score(Player player, int score)
        {
            var g = service.Context.GetGameAsync(game.Id, CancellationToken.None).Result;

            // trigger evaluation when all answers are in
            if (g.Answers.Count == g.Players.Count - 1)
            {
                // Advance the time so the game engine can evaluate results
                service.TimeService.CurrentTime += TimeSpan.FromSeconds(game.Options.MaxAnswerTime);
            }

            // Check if the leaderboard is updated
            var board = service.GetBoardAsync(game.Id, CancellationToken.None).Result;
            Assert.AreEqual(g.Players.Count, board.Rows.Count);
            Assert.AreEqual(score, board.Rows.First(r => r.PlayerName == player.Name).PlayerScore);
        }

        /// <summary>
        /// End the game.
        /// </summary>
        public void End()
        {
            var g = service.Context.GetGameAsync(game.Id, CancellationToken.None).Result;
            Assert.AreEqual(GameState.Completed, g.State);

            // Clean up the database
            service.Context.Database.EnsureDeleted();
        }

        /// <summary>
        /// Release resources.
        /// </summary>
        public void Dispose()
        {
            if (service != null)
            {
                service.Dispose();
                service = null;
            }
        }

        /// <summary>
        /// Represents a player used for testing purposes.
        /// </summary>
        /// <remarks>
        /// Exists solely so tests can be written as 'Player.Asks' instead of 'Game.Ask(Player, ...)'
        /// for better readability.
        /// </remarks>
        public class TestPlayer
        {
            /// <summary>
            /// The game the player is playing.
            /// </summary>
            private readonly TestGame game;

            /// <summary>
            /// The underlying player.
            /// </summary>
            private readonly Player player;

            /// <summary>
            /// Initialize static resources.
            /// </summary>
            /// <remarks>
            /// We want only the parent TestGame class to be able to create TestPlayer instances.
            /// Therefore this static constructor will initialize the delegate that is part of the parent
            /// class that it can use when creating players. This is the best way to simulate a private
            /// constructor accessible only to the parent class.
            /// </remarks>
            static TestPlayer()
            {
                newTestPlayer = (g, p) => new TestPlayer(g, p);
            }

            /// <summary>
            /// A dummy method to force execution of the static constructor.
            /// </summary>
            public static void Touch()
            {
            }

            /// <summary>
            /// Creates a new TestPlayer.
            /// </summary>
            /// <param name="game">The game to be a part of.</param>
            /// <param name="player">The underlying player.</param>
            private TestPlayer(TestGame game, Player player)
            {
                this.game = game;
                this.player = player;
            }

            /// <summary>
            /// Ask a question.
            /// </summary>
            /// <param name="question">The question to ask.</param>
            /// <param name="answer">The correct answer.</param>
            public void Asks(string question, int answer)
            {
                game.Ask(player, question, answer);
            }

            /// <summary>
            /// Answer a question.
            /// </summary>
            /// <param name="answer">The answer.</param>
            public void Answers(int answer)
            {
                game.Answer(player, answer);
            }

            /// <summary>
            /// The score to validate.
            /// </summary>
            /// <param name="score">The score.</param>
            public void Scores(int score)
            {
                game.Score(player, score);
            }
        }
    }
}
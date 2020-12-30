using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJackApi.Contracts;
using System;
using System.Linq;
using System.Threading;

namespace SharpJackApi.Tests
{
    /// <summary>
    /// Represents a simplified version of the game used for testing.
    /// </summary>
    /// <remarks>
    /// Abstracts common ways to play the game and also adds validation
    /// as appropriate after each step to minimize the actual test code
    /// and make it more readable.
    /// </remarks>
    public class TestGame<TClient> : IDisposable where TClient : ITestGameClient, new()
    {
        /// <summary>
        /// Maximum time to play, in seconds.
        /// </summary>
        private static readonly int MaxTime = 10;

        /// <summary>
        /// Delegate used to construct an instance of a TestPlayer.
        /// </summary>
        /// <remarks>
        /// We want only TestGame to have the ability to construct TestPlayer instances
        /// and this is the easiest way to simulate that.
        /// </remarks>
        /// <seealso cref="TestPlayer"/>
        private static Func<TestGame<TClient>, Player, TestPlayer> newTestPlayer;

        /// <summary>
        /// Default token.
        /// </summary>
        private static readonly CancellationToken Token = CancellationToken.None;

        /// <summary>
        /// The client instance to test against.
        /// </summary>
        private TClient client;

        /// <summary>
        /// The Game to test against.
        /// </summary>
        private Game game;

        /// <summary>
        /// The player creating the game.
        /// </summary>
        private Player creator;

        /// <summary>
        /// The current leader board.
        /// </summary>
        private LeaderBoard board;

        /// <summary>
        /// Private constructor to prevent initialization from outside the class.
        /// </summary>
        /// <seealso cref="Create(string, int, int, int, int)"/>
        private TestGame()
        {
            client = new TClient();
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
        public static (TestGame<TClient>, TestPlayer) Create(string player, int maxPlayers, int maxRounds = 1)
        {
            var g = new TestGame<TClient>();

            g.creator = g.client.AddPlayerAsync(player, Token).Result;
            Assert.AreEqual(player, g.creator.Name);

            var options = new GameOptions { PlayerId = g.creator.Id, MaxPlayers = maxPlayers, MaxQuestionTime = MaxTime, MaxAnswerTime = MaxTime, MaxRounds = maxRounds };
            g.game = g.client.CreateGameAsync(options, Token).Result;
            Assert.AreEqual(options.MaxActiveTime, g.game.Options.MaxActiveTime);
            Assert.AreEqual(options.MaxAnswerTime, g.game.Options.MaxAnswerTime);
            Assert.AreEqual(options.MaxQuestionTime, g.game.Options.MaxQuestionTime);
            Assert.AreEqual(options.MaxPlayers, g.game.Options.MaxPlayers);
            Assert.AreEqual(options.MaxRounds, g.game.Options.MaxRounds);
            Assert.AreEqual(options.PlayerId, g.game.Options.PlayerId);
            Assert.AreEqual(GameState.Created, g.game.State);

            return (g, newTestPlayer(g, g.creator));
        }

        /// <summary>
        /// Start the game.
        /// </summary>
        public void Start()
        {
            client.JoinOrStartGameAsync(game.Id, creator, Token).Wait();

            var g = client.GetGameAsync(game.Id, Token).Result;
            Assert.AreEqual(GameState.Active, g.State);

            var b = client.GetBoardAsync(game.Id, Token).Result;
            Assert.IsTrue(b.Rows.Exists(r => r.PlayerName == creator.Name && r.PlayerScore == 0));
        }

        /// <summary>
        /// Create a new player and have them join the game.
        /// </summary>
        /// <param name="player">The name of the player.</param>
        /// <returns>The newly created player.</returns>
        public TestPlayer Join(string player)
        {
            var p = client.AddPlayerAsync(player, Token).Result;
            Assert.AreEqual(player, p.Name);

            client.JoinOrStartGameAsync(game.Id, p, Token).Wait();
            var g = client.GetGameAsync(game.Id, Token).Result;
            Assert.AreEqual(GameState.Created, g.State);

            var b = client.GetBoardAsync(game.Id, Token).Result;
            Assert.IsTrue(b.Rows.Exists(r => r.PlayerName == player && r.PlayerScore == 0));

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
            // reset the board
            board = null;

            client.AskQuestionAsync(game.Id, new Question { PlayerId = player.Id, Title = question, Answer = answer }, Token).Wait();
        }

        /// <summary>
        /// Submit an answer to the currently active question.
        /// </summary>
        /// <param name="player">The player submitting the answer.</param>
        /// <param name="answer">The answer being submitted.</param>
        public void Answer(Player player, int answer)
        {
            client.SubmitAnswerAsync(game.Id, new Answer { PlayerId = player.Id, Value = answer }, Token).Wait();
        }

        /// <summary>
        /// Validate that the game gave the given player the given score.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="score">The score to validate.</param>
        public void Score(Player player, int score)
        {
            if (board == null)
            {
                client.TriggerEvaluationAsync(game.Id, Token).Wait();

                // Retrieve the leaderboard
                board = client.GetBoardAsync(game.Id, Token).Result;
            }

            // validate that the player scored as expected
            Assert.AreEqual(score, board.Rows.First(r => r.PlayerName == player.Name).PlayerScore);
        }

        /// <summary>
        /// End the game.
        /// </summary>
        public void End()
        {
            var g = client.EndGameAsync(game.Id, creator, Token).Result;
            Assert.AreEqual(GameState.Completed, g.State);
        }

        /// <summary>
        /// Release resources.
        /// </summary>
        public void Dispose()
        {
            if (client != null)
            {
                client.Dispose();
                client = default;
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
            private readonly TestGame<TClient> game;

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
            private TestPlayer(TestGame<TClient> game, Player player)
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
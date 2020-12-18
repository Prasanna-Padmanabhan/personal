using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJackApi.Models;
using SharpJackApi.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpJackApi.UnitTests
{
    [TestClass]
    public class SharpJackApiUnitTests
    {
        [TestMethod]
        public async Task SimpleGameAsync()
        {
            var service = new GameService();
            service.TimeService.CurrentTime = DateTime.UtcNow;
            // would ideally do nothing, but that would cause a busy loop, so just do enough to yield and not hammer CPU
            service.TimeService.SleepAction = span => { Thread.Sleep(TimeSpan.FromMilliseconds(50)); };
            using var source = new CancellationTokenSource();
            var task = Task.Run(() => service.RunAsync(source.Token));

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

            // wait until evaluation has completed
            while (game.ActiveQuestion != null)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }

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

            // wait until evaluation has completed
            while (game.ActiveQuestion != null)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }

            Assert.AreEqual(GameState.Completed, game.State);

            // Check the leaderboard
            board = await service.GetBoardAsync(game.Id);
            Assert.AreEqual(2, board.Rows.Count);
            Assert.AreEqual(2, board.Rows.First(r => r.PlayerId == jake.Id).PlayerScore);
            Assert.AreEqual(2, board.Rows.First(r => r.PlayerId == ben.Id).PlayerScore);

            // Terminate the game engine
            source.Cancel();
        }
    }
}
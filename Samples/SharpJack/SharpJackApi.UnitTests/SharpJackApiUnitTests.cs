using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpJackApi.UnitTests
{
    [TestClass]
    public class SharpJackApiUnitTests
    {
        [TestMethod]
        public void TwoPlayers()
        {
            (var game, var ben) = TestGame.Create("Benjamin Sisko", 2);
            using (game)
            {
                var jake = game.Join("Jake Sisko");
                game.Start();

                ben.Asks("How many seasons of Star Trek: Deep Space Nine?", 7);
                jake.Answers(5);
                jake.Scores(2);

                jake.Asks("How many episodes of DS9?", 176);
                ben.Answers(180);
                ben.Scores(2);

                game.End();
            }
        }
        
        [TestMethod]
        public void ManyPlayers()
        {
            (var game, var frasier) = TestGame.Create("Frasier Crane", 5);
            using (game)
            {
                var niles = game.Join("Niles Crane");
                var martin = game.Join("Martin Crane");
                var daphne = game.Join("Daphne Moon");
                var fred = game.Join("Frederick Crane");
                game.Start();

                frasier.Asks("How many operas did Puccini write?", 4);
                niles.Answers(4);
                martin.Answers(0);
                daphne.Answers(3);
                fred.Answers(5);
                frasier.Scores(0);
                niles.Scores(5);
                daphne.Scores(4);
                fred.Scores(3);
                martin.Scores(2);

                niles.Asks("How many points is my IQ higher than Frasier's?", 27);
                frasier.Answers(0);
                martin.Answers(27);
                daphne.Answers(30);
                fred.Answers(10);
                frasier.Scores(2);
                niles.Scores(5);
                martin.Scores(7);
                daphne.Scores(8);
                fred.Scores(6);

                martin.Asks("At what age did I join the army?", 19);
                frasier.Answers(18);
                niles.Answers(20);
                daphne.Answers(19);
                fred.Answers(14);
                frasier.Scores(6);
                niles.Scores(8);
                martin.Scores(7);
                daphne.Scores(13);
                fred.Scores(8);

                daphne.Asks("How many brothers do I have?", 8);
                frasier.Answers(7);
                niles.Answers(8);
                martin.Answers(10);
                fred.Answers(6);
                frasier.Scores(10);
                niles.Scores(13);
                martin.Scores(10);
                daphne.Scores(13);
                fred.Scores(10);

                fred.Asks("How many episodes have I appeared in?", 9);
                frasier.Answers(9);
                niles.Answers(8);
                martin.Answers(10);
                daphne.Answers(6);
                frasier.Scores(15);
                niles.Scores(17);
                martin.Scores(13);
                daphne.Scores(15);
                fred.Scores(10);

                game.End();
            }
        }
    }
}
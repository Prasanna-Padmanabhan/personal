namespace SharpJackApi.Tests
{
    public static class Tests<TClient> where TClient : ITestGameClient, new()
    {
        public static void TwoPlayersOneRound()
        {
            (var game, var ben) = TestGame<TClient>.Create("Benjamin Sisko", 2);
            using (game)
            {
                var jake = game.Join("Jake Sisko");
                game.Start();

                ben.Asks("How many seasons of Star Trek: Deep Space Nine?", 7);
                jake.Answers(5);
                jake.Scores(2);
                ben.Scores(1);

                jake.Asks("How many episodes of DS9?", 176);
                ben.Answers(180);
                ben.Scores(3);
                jake.Scores(3);

                game.End();
            }
        }
        
        public static void ManyPlayersOneRound()
        {
            (var game, var frasier) = TestGame<TClient>.Create("Frasier Crane", 5);
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
                frasier.Scores(4);
                niles.Scores(8);
                daphne.Scores(6);
                fred.Scores(4);
                martin.Scores(2);

                niles.Asks("How many points is my IQ higher than Frasier's?", 27);
                frasier.Answers(0);
                martin.Answers(27);
                daphne.Answers(30);
                fred.Answers(10);
                frasier.Scores(6);
                niles.Scores(12);
                martin.Scores(10);
                daphne.Scores(12);
                fred.Scores(8);

                martin.Asks("At what age did I join the army?", 19);
                frasier.Answers(18);
                niles.Answers(20);
                daphne.Answers(19);
                fred.Answers(14);
                frasier.Scores(12);
                niles.Scores(16);
                martin.Scores(14);
                daphne.Scores(20);
                fred.Scores(10);

                daphne.Asks("How many brothers do I have?", 8);
                frasier.Answers(7);
                niles.Answers(8);
                martin.Answers(10);
                fred.Answers(6);
                frasier.Scores(18);
                niles.Scores(24);
                martin.Scores(18);
                daphne.Scores(24);
                fred.Scores(12);

                fred.Asks("How many episodes have I appeared in?", 9);
                frasier.Answers(9);
                niles.Answers(8);
                martin.Answers(10);
                daphne.Answers(6);
                frasier.Scores(26);
                niles.Scores(30);
                martin.Scores(22);
                daphne.Scores(26);
                fred.Scores(16);

                game.End();
            }
        }
    }
}
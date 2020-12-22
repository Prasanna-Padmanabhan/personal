namespace SharpJackApi.Contracts
{
    public class GameOptions
    {
        public int PlayerId { get; set; }

        public int MaxPlayers { get; set; }

        public int MaxQuestionTime { get; set; }

        public int MaxAnswerTime { get; set; }

        public int MaxRounds { get; set; }

        public int MaxActiveTime { get; set; }
    }
}

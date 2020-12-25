namespace SharpJackApi.Contracts
{
    /// <summary>
    /// Represents the various options available to configure a game.
    /// </summary>
    public class GameOptions
    {
        /// <summary>
        /// The ID of the player creating the game.
        /// </summary>
        public int PlayerId { get; set; }

        /// <summary>
        /// The maximum number of players allowed.
        /// </summary>
        public int MaxPlayers { get; set; }

        /// <summary>
        /// The maximum time, in seconds, available to ask a question.
        /// </summary>
        public int MaxQuestionTime { get; set; }

        /// <summary>
        /// The maximum time, in seconds, available to answer a question.
        /// </summary>
        public int MaxAnswerTime { get; set; }

        /// <summary>
        /// The maximum number of rounds.
        /// </summary>
        /// <remarks>
        /// One round is complete when all players have had a chance to ask at least one question.
        /// </remarks>
        public int MaxRounds { get; set; }

        /// <summary>
        /// The maximum time, in seconds, to keep the game active.
        /// </summary>
        /// <remarks>
        /// The game can no longer be played after this time expires. Currently unused.
        /// </remarks>
        public int MaxActiveTime { get; set; }
    }
}

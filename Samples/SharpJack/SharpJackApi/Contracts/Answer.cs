namespace SharpJackApi.Contracts
{
    /// <summary>
    /// Represents an Answer provided by a player in response to a question.
    /// </summary>
    public struct Answer
    {
        /// <summary>
        /// The ID of the player providing the answer.
        /// </summary>
        public int PlayerId { get; set; }

        /// <summary>
        /// The value of the answer.
        /// </summary>
        public int Value { get; set; }
    }
}

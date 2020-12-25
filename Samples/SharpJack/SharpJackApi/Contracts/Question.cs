namespace SharpJackApi.Contracts
{
    /// <summary>
    /// Represents a question.
    /// </summary>
    public class Question
    {
        /// <summary>
        /// The title of the question (i.e. the question itself).
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The answer to the question.
        /// </summary>
        public int Answer { get; set; }

        /// <summary>
        /// The ID of the player asking the question.
        /// </summary>
        public int PlayerId { get; set; }
    }
}
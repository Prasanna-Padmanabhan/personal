using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpJackApi.Models
{
    /// <summary>
    /// Represents an answer as stored in the database.
    /// </summary>
    public class Answer
    {
        /// <summary>
        /// The ID of the answer, generated automatically by the database.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The ID of the question that this is an answer to.
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// The ID of the player submitting the answer.
        /// </summary>
        public int PlayerId { get; set; }

        /// <summary>
        /// The actual answer.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The time the answer was submitted.
        /// </summary>
        public DateTime SubmitTime { get; set; }

        /// <summary>
        /// The score associated with the answer.
        /// </summary>
        public int Score { get; set; }
    }
}

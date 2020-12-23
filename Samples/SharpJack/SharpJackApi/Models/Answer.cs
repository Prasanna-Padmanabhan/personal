using System;

namespace SharpJackApi.Models
{
    public class Answer
    {
        public int QuestionId { get; set; }

        public int PlayerId { get; set; }

        public int Value { get; set; }

        public DateTime SubmitTime { get; set; }

        public int Score { get; set; }
    }
}

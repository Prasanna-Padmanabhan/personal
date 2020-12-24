using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpJackApi.Models
{
    public class Answer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int QuestionId { get; set; }

        public int PlayerId { get; set; }

        public int Value { get; set; }

        public DateTime SubmitTime { get; set; }

        public int Score { get; set; }
    }
}

using System;
using System.Runtime.Serialization;

namespace SharpJackApi.Models
{
    public class Answer
    {
        public int PlayerId { get; set; }

        public int Value { get; set; }

        [IgnoreDataMember]
        public DateTime SubmitTime { get; set; }

        [IgnoreDataMember]
        public int Score { get; set; }
    }
}

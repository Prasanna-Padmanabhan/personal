using System;
using System.Runtime.Serialization;

namespace SharpJackApi.Contracts
{
    public class Answer
    {
        public int PlayerId { get; set; }

        public int Value { get; set; }
    }
}

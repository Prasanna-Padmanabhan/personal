using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SharpJackApi.Models
{
    public class Row
    {
        public string PlayerName { get; set; }

        public int PlayerScore { get; set; }

        [IgnoreDataMember]
        public int PlayerId { get; set; }
    }

    public class LeaderBoard
    {
        public List<Row> Rows { get; set; }

        public LeaderBoard()
        {
            Rows = new List<Row>();
        }
    }
}

using System.Collections.Generic;

namespace SharpJackApi.Models
{
    public class Row
    {
        public int PlayerScore { get; set; }

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

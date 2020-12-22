using System.Collections.Generic;

namespace SharpJackApi.Contracts
{
    public class Row
    {
        public string PlayerName { get; set; }

        public int PlayerScore { get; set; }
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

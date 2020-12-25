using System.Collections.Generic;

namespace SharpJackApi.Contracts
{
    /// <summary>
    /// Represents each row of the leader board.
    /// </summary>
    public class Row
    {
        /// <summary>
        /// The name of the player.
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// The score of the player.
        /// </summary>
        public int PlayerScore { get; set; }
    }

    /// <summary>
    /// Represents a leader board for a game.
    /// </summary>
    public class LeaderBoard
    {
        /// <summary>
        /// The list of rows in the board.
        /// </summary>
        public List<Row> Rows { get; set; }

        /// <summary>
        /// Initialize the rows.
        /// </summary>
        public LeaderBoard()
        {
            Rows = new List<Row>();
        }
    }
}

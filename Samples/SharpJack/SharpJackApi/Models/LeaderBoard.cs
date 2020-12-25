using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpJackApi.Models
{
    /// <summary>
    /// Represents a row as stored in the database.
    /// </summary>
    public class Row
    {
        /// <summary>
        /// The ID of the player.
        /// </summary>
        public int PlayerId { get; set; }

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
    /// Represents a leader board as stored in the database.
    /// </summary>
    public class LeaderBoard
    {
        /// <summary>
        /// The ID, generated automatically by the database.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The list of rows.
        /// </summary>
        public List<Row> Rows { get; set; }

        /// <summary>
        /// Initializer.
        /// </summary>
        public LeaderBoard()
        {
            Rows = new List<Row>();
        }
    }
}

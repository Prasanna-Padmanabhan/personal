using System.ComponentModel.DataAnnotations.Schema;

namespace SharpJackApi.Models
{
    /// <summary>
    /// Represents a player as stored in the database.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// The ID of the player, generated automatically by the database.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The name of the player.
        /// </summary>
        public string Name { get; set; }
    }
}

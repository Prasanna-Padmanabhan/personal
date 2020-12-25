using Microsoft.EntityFrameworkCore;
using SharpJackApi.Models;

namespace SharpJackApi.Data
{
    /// <summary>
    /// Represents a session with the database.
    /// </summary>
    public class GameContext : DbContext
    {
        /// <summary>
        /// Initializes the game context.
        /// </summary>
        /// <param name="options">Initialization Options</param>
        public GameContext(DbContextOptions<GameContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configure non-obvious relationships within the data models.
        /// </summary>
        /// <param name="modelBuilder">The model builder to configure.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>().OwnsOne(g => g.Options);
            modelBuilder.Entity<LeaderBoard>().OwnsOne(b => b.Rows);
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// The set of games in the database.
        /// </summary>
        public DbSet<Game> Games { get; set; }

        /// <summary>
        /// The set of players in the database.
        /// </summary>
        public DbSet<Player> Players { get; set; }
    }
}

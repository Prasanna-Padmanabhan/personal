using Microsoft.EntityFrameworkCore;
using SharpJackApi.Models;

namespace SharpJackApi.Data
{
    public class GameContext : DbContext
    {
        public GameContext(DbContextOptions<GameContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>().OwnsOne(g => g.Options);
            modelBuilder.Entity<LeaderBoard>().OwnsOne(b => b.Rows);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Game> Games { get; set; }

        public DbSet<Player> Players { get; set; }
    }
}

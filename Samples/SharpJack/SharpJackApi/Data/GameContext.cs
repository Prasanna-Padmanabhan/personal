using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        }

        /// <summary>
        /// The set of games in the database.
        /// </summary>
        public DbSet<Game> Games { get; set; }

        /// <summary>
        /// The set of players in the database.
        /// </summary>
        public DbSet<Player> Players { get; set; }

        /// <summary>
        /// The set of leader board rows in the database.
        /// </summary>
        public DbSet<Row> Rows { get; set; }

        /// <summary>
        /// The set of leader boards in the database.
        /// </summary>
        public DbSet<LeaderBoard> Boards { get; set; }

        /// <summary>
        /// The set of questions in the database.
        /// </summary>
        public DbSet<Question> Questions { get; set; }

        /// <summary>
        /// The set of answers in the database.
        /// </summary>
        public DbSet<Answer> Answers { get; set; }

        private static ILogger Logger;

        public void LogEvents(ILogger logger)
        {
            Logger = logger;
            SavingChanges += HandleSavingChanges;
            SavedChanges += HandleSavedChanges;
            SaveChangesFailed += HandleSaveChangesFailed;
        }

        private static void HandleSaveChangesFailed(object sender, SaveChangesFailedEventArgs e)
        {
            Logger?.LogError(e.Exception.ToString());
        }

        private static void HandleSavedChanges(object sender, SavedChangesEventArgs e)
        {
            Logger?.LogInformation(e.EntitiesSavedCount.ToString());
        }

        private static void HandleSavingChanges(object sender, SavingChangesEventArgs e)
        {
            Logger?.LogInformation(e.ToString());
        }
    }
}
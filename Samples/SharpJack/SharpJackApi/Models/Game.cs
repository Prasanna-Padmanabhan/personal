using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GameOptions = SharpJackApi.Contracts.GameOptions;
using GameState = SharpJackApi.Contracts.GameState;

namespace SharpJackApi.Models
{
    /// <summary>
    /// Represents a game as stored in the database.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// The ID of the game, generated automatically by the database.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The options configured for the game.
        /// </summary>
        public GameOptions Options { get; set; }

        /// <summary>
        /// The current state of the game.
        /// </summary>
        public GameState State { get; set; }

        /// <summary>
        /// The current active player.
        /// </summary>
        public int ActivePlayer { get; set; }

        /// <summary>
        /// Time available for the active player to ask a question.
        /// </summary>
        public DateTime ActiveUntil { get; set; }

        /// <summary>
        /// The players playing the game.
        /// </summary>
        public List<Player> Players { get; set; }

        /// <summary>
        /// The leader board for the game.
        /// </summary>
        public LeaderBoard Board { get; set; }

        /// <summary>
        /// The current active question.
        /// </summary>
        public Question ActiveQuestion { get; set; }

        /// <summary>
        /// The list of answers submitted for the active question.
        /// </summary>
        public List<Answer> Answers { get; set; }

        /// <summary>
        /// The current round of the game.
        /// </summary>
        public int CurrentRound { get; set; }

        /// <summary>
        /// Initializes the fields.
        /// </summary>
        public Game()
        {
            Players = new List<Player>();
            Answers = new List<Answer>();
            Board = new LeaderBoard();
        }
    }
}
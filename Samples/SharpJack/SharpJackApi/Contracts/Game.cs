using System;

namespace SharpJackApi.Contracts
{
    /// <summary>
    /// The various possible states of the game.
    /// </summary>
    public enum GameState
    {
        Created,
        Active,
        Completed
    }

    /// <summary>
    /// Represents a game.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// The ID of the game.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The configured options for the game.
        /// </summary>
        public GameOptions Options { get; set; }

        /// <summary>
        /// The current state of the game.
        /// </summary>
        public GameState State { get; set; }

        /// <summary>
        /// The ID of the currently active player.
        /// </summary>
        /// <remarks>
        /// The active player is responsible for asking the next question.
        /// </remarks>
        public int ActivePlayer { get; set; }

        /// <summary>
        /// The active player has time until this point to ask the next question.
        /// </summary>
        public DateTime ActiveUntil { get; set; }
    }
}
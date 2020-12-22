using System;

namespace SharpJackApi.Contracts
{
    public enum GameState
    {
        Created,
        Active,
        Completed
    }

    public class Game
    {
        public int Id { get; set; }

        public GameOptions Options { get; set; }

        public GameState State { get; set; }

        public int ActivePlayer { get; set; }

        public DateTime ActiveUntil { get; set; }
    }
}
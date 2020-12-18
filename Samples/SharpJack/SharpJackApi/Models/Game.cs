using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SharpJackApi.Models
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

        public string Link { get; set; }

        public GameOptions Options { get; set; }

        public GameState State { get; set; }

        public int ActivePlayer { get; set; }

        public DateTime ActiveUntil { get; set; }

        [IgnoreDataMember]
        public List<int> Players { get; set; }

        [IgnoreDataMember]
        public LeaderBoard Board { get; set; }

        [IgnoreDataMember]
        public Question ActiveQuestion { get; set; }

        [IgnoreDataMember]
        public List<Answer> Answers { get; set; }

        [IgnoreDataMember]
        public int CurrentRound { get; set; }

        public Game()
        {
            Players = new List<int>();
            Answers = new List<Answer>();
            Board = new LeaderBoard();
        }
    }
}

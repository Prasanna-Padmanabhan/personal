using GameOptions = SharpJackApi.Contracts.GameOptions;
using GameState = SharpJackApi.Contracts.GameState;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpJackApi.Models
{
    public class Game
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public GameOptions Options { get; set; }

        public GameState State { get; set; }

        public int ActivePlayer { get; set; }

        public DateTime ActiveUntil { get; set; }

        public List<int> Players { get; set; }

        public LeaderBoard Board { get; set; }

        public Question ActiveQuestion { get; set; }

        public List<Answer> Answers { get; set; }

        public int CurrentRound { get; set; }

        public Game()
        {
            Players = new List<int>();
            Answers = new List<Answer>();
            Board = new LeaderBoard();
        }
    }
}
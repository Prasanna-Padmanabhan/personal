using System.Linq;

/// <summary>
/// Utility methods to convert between the Contract (i.e. used for communication between API and client)
/// and Model (i.e. used for persisting state in the database) versions of various entities.
/// </summary>
namespace SharpJackApi.Utilities
{
    public static class AnswerExtensions
    {
        public static Contracts.Answer ToContract(this Models.Answer answer)
        {
            return new Contracts.Answer { PlayerId = answer.PlayerId, Value = answer.Value };
        }

        public static Models.Answer ToModel(this Contracts.Answer answer)
        {
            return new Models.Answer { PlayerId = answer.PlayerId, Value = answer.Value };
        }
    }

    public static class GameExtensions
    {
        public static Contracts.Game ToContract(this Models.Game game)
        {
            return new Contracts.Game { 
                ActivePlayer = game.ActivePlayer, 
                ActiveUntil = game.ActiveUntil, 
                Id = game.Id, 
                Options = game.Options, 
                State = game.State
            };
        }
    }

    public static class LeaderBoardExtensions
    {
        public static Contracts.LeaderBoard ToContract(this Models.LeaderBoard board)
        {
            return new Contracts.LeaderBoard { Rows = board.Rows.Select(r => new Contracts.Row { PlayerName = r.Player.Name, PlayerScore = r.PlayerScore}).ToList() };
        }
    }

    public static class QuestionExtensions
    {
        public static Contracts.Question ToContract(this Models.Question question)
        {
            return new Contracts.Question { Answer = question.Answer, PlayerId = question.PlayerId, Title = question.Title };
        }

        public static Models.Question ToModel(this Contracts.Question question)
        {
            return new Models.Question { Answer = question.Answer, PlayerId = question.PlayerId, Title = question.Title };
        }
    }

    public static class PlayerExtensions
    {
        public static Contracts.Player ToContract(this Models.Player player)
        {
            return new Contracts.Player { Id = player.Id, Name = player.Name };
        }
    }
}

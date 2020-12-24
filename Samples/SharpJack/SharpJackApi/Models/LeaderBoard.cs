using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpJackApi.Models
{
    public class Row
    {
        public int PlayerScore { get; set; }

        public int PlayerId { get; set; }
    }

    public class LeaderBoard
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public List<Row> Rows { get; set; }

        public LeaderBoard()
        {
            Rows = new List<Row>();
        }
    }
}

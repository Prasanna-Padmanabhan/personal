using System.ComponentModel.DataAnnotations.Schema;

namespace SharpJackApi.Models
{
    public class Question
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Title { get; set; }

        public int Answer { get; set; }

        public int PlayerId { get; set; }
    }
}

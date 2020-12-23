using System.ComponentModel.DataAnnotations.Schema;

namespace SharpJackApi
{
    public class Player
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}

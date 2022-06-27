using System.ComponentModel.DataAnnotations;

namespace PopeAI.Models
{
    public class Help
    {
        [Key]
        public int Id { get; set; }

        [VarChar(256)]
        public string Message {get; set;}
    }
}
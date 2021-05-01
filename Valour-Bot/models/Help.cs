using System.ComponentModel.DataAnnotations;

namespace PopeAI.Models
{
    public class Help
    {
        [Key]
        public string Message {get; set;}
    }
}
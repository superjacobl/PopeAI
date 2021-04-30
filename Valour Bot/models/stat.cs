using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PopeAI.Models
{
    public class Help
    {
        [Key]
        public string message {get; set;}
    }
}
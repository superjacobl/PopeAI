using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PopeAI.Models
{
    public class LotteryTicket
    {
        [Key]
        public string Id {get; set;}
        public ulong PlanetId {get; set;}
        public ulong UserId {get; set;}
        public ulong Tickets {get; set;}
    }
}
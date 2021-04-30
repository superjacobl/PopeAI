using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PopeAI.Models
{
    public class Stat
    {
        [Key]
        public ulong Id {get; set;}
        public ulong PlanetId {get; set;}
        public double NewCoins {get; set;}
        public ulong MessagesUsersSent {get; set;}
        public ulong MessagesSent {get; set;}
        public DateTime Time {get; set;}

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PopeAI.Models
{
    public class ShopReward
    {
        [Key]
        public ulong Id {get; set;}
        public double Cost {get; set;}

        public ulong RoleId {get; set;}
        public ulong PlanetId {get; set;}
        public string RoleName {get; set;}

    }
}
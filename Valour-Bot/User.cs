using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System;
using Valour.Net;
using Valour.Net.Models;

namespace PopeAI
{
    public class User
    {
        [Key]
        public ulong Id { get; set; }
        public ulong UserId { get; set; }

        public double Xp { get; set; }
        public ulong PlanetId { get; set; }
        public double Coins { get; set; }
        public DateTime LastHourly { get; set; }

        public async Task<PlanetMember> GetAuthor(ulong Planet_Id) {
            PlanetMember planetUser = await Cache.GetPlanetMember(UserId, Planet_Id);

            return planetUser;
        }

    }
}
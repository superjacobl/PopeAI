using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PopeAI
{
    public class User
    {
        [Key]
        public ulong Id { get; set; }
        public ulong UserId { get; set; }

<<<<<<< HEAD:Valour Bot/User.cs
        public double Xp {get; set;}
        public ulong PlanetId {get; set;}
        public double Coins {get; set;}
        public DateTime LastHourly {get; set;}
=======
        public double Xp { get; set; }
        public ulong PlanetId { get; set; }
        public double Coins { get; set; }
>>>>>>> 21c70f288fe6f6c852deb9d95db2d905c5250cd6:Valour-Bot/User.cs

        public async Task<ClientPlanetUser> GetAuthor(ulong Planet_Id) {
            ClientPlanetUser planetUser = await PlanetUserCache.GetPlanetUserAsync(UserId, Planet_Id);

            return planetUser;
        }

    }
}
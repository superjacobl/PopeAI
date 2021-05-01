using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PopeAI
{
    public class User
    {
        [Key]
        public ulong Id {get; set;}
        public ulong UserId {get; set;}

        public double Xp {get; set;}
        public ulong PlanetId {get; set;}
        public double Coins {get; set;}

        public async Task<ClientPlanetUser> GetAuthor(ulong Planet_Id) {
            ClientPlanetUser planetUser = await PlanetUserCache.GetPlanetUserAsync(UserId, Planet_Id);

            return planetUser;
        }

    }
}
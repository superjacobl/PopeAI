using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PopeAI
{
    /*  Valour - A free and secure chat client
     *  Copyright (C) 2020 Vooper Media LLC
     *  This program is subject to the GNU Affero General Public license
     *  A copy of the license should be included - if not, see <http://www.gnu.org/licenses/>
     */

    /// <summary>
    /// A cache used on the client to prevent the need to repeatedly hit Valour servers
    /// for user data.
    /// </summary>
    public static class PlanetUserCache
    {
        private static ConcurrentDictionary<string, ClientPlanetUser> Cache = new ConcurrentDictionary<string, ClientPlanetUser>();

        /// <summary>
        /// Returns a user from the given id
        /// </summary>
        public static async Task<ClientPlanetUser> GetPlanetUserAsync(ulong userid, ulong planet_id)
        {
            if (userid == 0)
            {
                return new ClientPlanetUser()
                {
                    Id = 0,
                    JoinDateTime = DateTime.UtcNow,
                    Planet_Id = planet_id,
                    Nickname = "Valour AI"
                };
            }

            string key = $"{planet_id}-{userid}";

            // Attempt to retrieve from cache
            if (Cache.ContainsKey(key))
            {
                return Cache[key];
            }

            // Retrieve from server
            ClientPlanetUser user = await ClientPlanetUser.GetClientPlanetMemberAsync(userid, planet_id);

            await user.UpdateMemberRoles();

            if (user == null)
            {
                Console.WriteLine($"Failed to fetch planet user with user id {userid} and planet id {planet_id}.");
                return null;
            }

            Console.WriteLine($"Fetched planet user {userid} for planet {planet_id}");

            // Add to cache
            Cache.TryAdd(key, user);

            return user;

        }
    }
}
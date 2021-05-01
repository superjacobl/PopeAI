using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace PopeAI
{
    public class Planet
    {
        /// <summary>
        /// The ID of the planet
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// The Id of the owner of this planet
        /// </summary>
        public ulong Owner_Id { get; set; }

        /// <summary>
        /// The name of the planet
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The image url for the planet 
        /// </summary>
        public string Image_Url { get; set; }

        /// <summary>
        /// The description of the planet
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// If the server requires express allowal to join a planet
        /// </summary>
        public bool Public { get; set; }

        /// <summary>
        /// The amount of members on the planet
        /// </summary>
        public uint Member_Count { get; set; }

        /// <summary>
        /// The default role for the planet
        /// </summary>
        public ulong Default_Role_Id { get; set; }

        /// <summary>
        /// The id of the main channel of the planet
        /// </summary>
        public ulong Main_Channel_Id { get; set; }

        static HttpClient client = new HttpClient();

        public async Task<List<Channel>> GetChannelsAsync() {
            HttpClient client = new HttpClient();
            string json = await client.GetStringAsync($"https://valour.gg/Channel/GetPlanetChannels?planet_id={Id}&token={Client.Config.AuthKey}");

            TaskResult<List<Channel>> result = JsonConvert.DeserializeObject<TaskResult<List<Channel>>>(json);
            return result.Data;
        }
        // memberid: list of roleids
        public async Task<List<PlanetMemberInfo>> GetMembers() {
            string json = await client.GetStringAsync($"https://valour.gg/Planet/GetPlanetMemberInfo?planet_id={Id}&token={Client.Config.AuthKey}");

            TaskResult<List<PlanetMemberInfo>> result = JsonConvert.DeserializeObject<TaskResult<List<PlanetMemberInfo>>>(json);
            

            if (result == null)
            {
                Console.WriteLine("A fatal error occurred retrieving a planet member from the server.");
                return null;
            }

            if (!result.Success)
            {
                Console.WriteLine(result.ToString());
                return null;
            }

            return result.Data;
        }
        public async Task<ClientRole> GetRoleAsync(string RoleName) {
            string json = await client.GetStringAsync($"https://valour.gg/Planet/GetPlanetRoles?planet_id={Id}&token={Client.Config.AuthKey}");

            TaskResult<List<ClientRole>> result = JsonConvert.DeserializeObject<TaskResult<List<ClientRole>>>(json);

            if (result == null)
            {
                Console.WriteLine("A fatal error occurred retrieving a planet member from the server.");
                return null;
            }

            if (!result.Success)
            {
                Console.WriteLine(result.ToString());
                return null;
            }

            ClientRole role = result.Data.FirstOrDefault(x => x.Name == RoleName);

            return role;
        }
    }
}
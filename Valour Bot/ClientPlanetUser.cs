using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using PopeAI;

namespace PopeAI
{
    public class ClientPlanetUser
    {

        public ulong Planet_Id { get; set;}
        public ulong Id { get; set; }

        /// <summary>
        /// The main display name for the user
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// The url for the user's profile picture
        /// </summary>
        public string Pfp_Url { get; set; }

        /// <summary>
        /// The Date and Time that the user joined Valour
        /// </summary>
        public DateTime Join_DateTime { get; set; }

        /// <summary>
        /// True if the user is a bot
        /// </summary>
        public bool Bot { get; set; }

        /// <summary>
        /// True if the account has been disabled
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// True if this user is a member of the Valour official staff team. Falsely modifying this 
        /// through a client modification to present non-official staff as staff is a breach of our
        /// license. Don't do that.
        /// </summary>
        public bool Valour_Staff { get; set; }

        public List<ClientRole> Roles = new List<ClientRole>();

        public string GetState()
        {
            return "Currently browsing";
        }

        public string GetMainRoleColor()
        {
            return "#00FAFF";
        }

        static HttpClient client = new System.Net.Http.HttpClient();


        public static async Task<ClientPlanetUser> GetClientPlanetMemberAsync(ulong user_id, ulong planet_id)
        {

            Console.WriteLine($"https://valour.gg/Planet/GetPlanetMember?user_id={user_id}&planet_id={planet_id}&auth={Client.config.authkey}");

            string json = await client.GetStringAsync($"https://valour.gg/Planet/GetPlanetMember?user_id={user_id}&planet_id={planet_id}&auth={Client.config.authkey}");

            TaskResult<ClientPlanetUser> result = JsonConvert.DeserializeObject<TaskResult<ClientPlanetUser>>(json);

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

        public async Task<int> GiveRole(string RoleName) {

            string json = await client.GetStringAsync($"https://valour.gg/Planet/GetPlanetRoles?planet_id={Planet_Id}&token={Client.config.authkey}");

            TaskResult<List<ClientRole>> result = JsonConvert.DeserializeObject<TaskResult<List<ClientRole>>>(json);

            if (result == null)
            {
                Console.WriteLine("A fatal error occurred retrieving a planet member from the server.");
                return 0;
            }

            if (!result.Success)
            {
                Console.WriteLine(result.ToString());
                return 0;
            }

            ClientRole role = result.Data.FirstOrDefault(x => x.Name == RoleName);

            if (role != null) {
                json = await client.GetStringAsync($"https://valour.gg/Planet/SetMemberRoleMembership?role_id={role.Id}&member_id={Id}&planet_id={Planet_Id}&value=true&token={Client.config.authkey}");
                
                TaskResult Otherresult = JsonConvert.DeserializeObject<TaskResult>(json);
                
                if (Otherresult == null)
                {
                    Console.WriteLine("A fatal error occurred retrieving a planet member from the server.");
                    return 0;
                }

                if (!Otherresult.Success)
                {
                    Console.WriteLine(Otherresult.ToString());
                    return 0;
                }
                Console.WriteLine(Otherresult.ToString());
            }


            return 0;
        }

        public async Task UpdateMemberRoles() {
            string json = await client.GetStringAsync($"https://valour.gg/Planet/GetMemberRoles?member_id={Id}&token={Client.config.authkey}");
                
            TaskResult<List<ClientRole>> result = JsonConvert.DeserializeObject<TaskResult<List<ClientRole>>>(json);

            Roles = result.Data;
            
            if (result == null)
            {
                Console.WriteLine("A fatal error occurred retrieving a planet member from the server.");
                return;
            }

            if (!result.Success)
            {
                Console.WriteLine(result.ToString());
                return;
            }
        }

        public async Task<uint> GetMemberAuthority() {
            string json = await client.GetStringAsync($"https://valour.gg/Planet/GetMemberAuthority?member_id={Id}&token={Client.config.authkey}");
                
            TaskResult<uint> result = JsonConvert.DeserializeObject<TaskResult<uint>>(json);
            
            if (result == null)
            {
                Console.WriteLine("A fatal error occurred retrieving a planet member from the server.");
                return 0;
            }

            if (!result.Success)
            {
                Console.WriteLine(result.ToString());
                return 0;
            }
            return result.Data;
        }

        public async Task<bool> IsOwner() {
            if (await GetMemberAuthority() == uint.MaxValue || Nickname == "superjacobl") {
                return true;
            }
            else {
                return false;
            }
        }
        

    }
}

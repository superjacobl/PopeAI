using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using Newtonsoft.Json;

namespace PopeAI
{
    public class Channel
    {
        /// <summary>
        /// The Id of this channel
        /// </summary>
        public ulong Id { get; set; }

        /// <summary>
        /// The name of this channel
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Id of the planet this channel belongs to
        /// </summary>
        public ulong PlanetId { get; set; }

        /// <summary>
        /// The amount of messages ever sent in the channel
        /// </summary>
        public ulong MessageCount { get; set; }

        /// <summary>
        /// The id of the parent category, is null if theres no parent
        /// </summary>
        public ulong? ParentId { get; set;}

        /// <summary>
        /// Is the position in the category/channel list
        /// </summary>
        public ushort Position { get; set; }

        /// <summary>
        /// The description of the channel
        /// </summary>
        public string Description { get; set; }

        public static async Task<bool> CreateChannel(string name, ulong Parent_Id, ulong PlanetId) {
            HttpClient client = new HttpClient();
            
            name = Client.UrlEncodeExtended(name);

            string json = await client.GetStringAsync($"https://valour.gg/Channel/CreateChannel?planet_id={PlanetId}&name={name}&user_id={Client.Config.BotId}&token={Client.Config.AuthKey}&parentid={Parent_Id}");

            TaskResult<ulong> result = JsonConvert.DeserializeObject<TaskResult<ulong>>(json);

            if (result.Success) {
                await Client.hubConnection.SendAsync("JoinChannel", result.Data, Client.Config.AuthKey);
                return true;
            }

            else {
                return false;
            }
        }
    }
}
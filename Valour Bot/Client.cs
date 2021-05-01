using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using PopeAI.Database;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.IO;

namespace PopeAI
{
    public class Config
    {
        public static Config instance;

        [JsonProperty]
        public string authkey { get; set; }

        [JsonProperty]
        public string BotId { get; set; }

        [JsonProperty]
        public string Host { get; set; }

        [JsonProperty]
        public string Password { get; set; }

        [JsonProperty]
        public string Username { get; set; }

        [JsonProperty]
        public string Database { get; set; }
        [JsonProperty]
        public string CommandSign { get; set; }


        public Config()
        {
            // Set main instance to the most recently created config
            instance = this;
        }
    }
    class Client
    {
        public static PopeAIDB Context = new PopeAIDB(PopeAIDB.DBOptions);
        public static Config config {get; set;}

        public static HubConnection hubConnection = new HubConnectionBuilder()
            .WithUrl("https://valour.gg/planethub")
            .WithAutomaticReconnect()
            .Build();
        
        public static bool Check() {
            config = null;

            // Create directory if it doesn't exist
            if (!Directory.Exists("PopeAIConfig/"))
            {
                Directory.CreateDirectory("PopeAIConfig/");
            }

            if (File.Exists("PopeAIConfig/config.json"))
            {
                // If there is a config, read it
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("PopeAIConfig/config.json"));
            }
            else
            {
                // Otherwise create a config with default values and write it to the location
                config = new Config()
                {
                    authkey = "authkey",
                    BotId = "bot's userid",
                    Database = "database",
                    Host = "host",
                    Password = "password",
                    Username = "user"
                };

                File.WriteAllText("PopeAIConfig/config.json", JsonConvert.SerializeObject(config));
                Console.WriteLine("Error: No config was found. Creating file...");
            }
            return true;
        }

        public static string UrlEncodeExtended( string value )
        {
            char[] chars = value.ToCharArray();
            StringBuilder encodedValue = new StringBuilder();
            foreach (char c in chars)
            {
                encodedValue.Append( "%" + ( (int)c ).ToString( "X2" ) );
            }
            return encodedValue.ToString();
        }
    }
}
using System;
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
        public string AuthKey { get; set; }

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
        [JsonProperty]
        public string BotPassword {get; set;}
        [JsonProperty]
        public string Email { get; set; }


        public Config()
        {
            // Set main instance to the most recently created config
            instance = this;
        }
    }
    class Client
    {
        public static PopeAIDB DBContext = new PopeAIDB(PopeAIDB.DBOptions);
        public static Config Config {get; set;}

        public static HubConnection hubConnection = new HubConnectionBuilder()
            .WithUrl("https://valour.gg/planethub")
            .WithAutomaticReconnect()
            .Build();
        
        public static bool Check() {
            Config = null;

            // Create directory if it doesn't exist
            if (!Directory.Exists("PopeAIConfig/"))
            {
                Directory.CreateDirectory("PopeAIConfig/");
            }

            if (File.Exists("PopeAIConfig/config.json"))
            {
                // If there is a config, read it
                Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("PopeAIConfig/config.json"));
            }
            else
            {
                // Otherwise create a config with default values and write it to the location
                Config = new Config()
                {
                    AuthKey = "authkey",
                    BotId = "bot's userid",
                    Database = "database",
                    Host = "host",
                    Password = "password",
                    Username = "user",
                    BotPassword = "BotPassword",
                    Email = "Bot's Email"
                };

                File.WriteAllText("PopeAIConfig/config.json", JsonConvert.SerializeObject(Config));
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
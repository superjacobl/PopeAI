using System;
using System.Text;
using PopeAI.Database;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace PopeAI.Database.Config;

public class Config
{
    public static Config instance;

    [JsonInclude]
    public string AuthKey { get; set; }

    [JsonInclude]
    public string BotId { get; set; }

    [JsonInclude]
    public string Host { get; set; }

    [JsonInclude]
    public string Password { get; set; }

    [JsonInclude]
    public string Username { get; set; }

    [JsonInclude]
    public string Database { get; set; }
    [JsonInclude]
    public string CommandSign { get; set; }
    [JsonInclude]
    public string BotPassword {get; set;}
    [JsonInclude]
    public string Email { get; set; }
    [JsonInclude]
    public string Token { get; set; }


    public Config()
    {
        // Set main instance to the most recently created config
        instance = this;
    }
}
public class Client
{
    public static PopeAIDB DBContext = new(PopeAIDB.DBOptions);
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
            Config = JsonSerializer.Deserialize<Config>(File.ReadAllText("PopeAIConfig/config.json"));
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

            File.WriteAllText("PopeAIConfig/config.json", JsonSerializer.Serialize(Config));
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
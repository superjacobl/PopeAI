global using Valour.Api.Items.Planets;
global using Valour.Api.Items.Planets.Members;
global using Valour.Api.Items.Planets.Channels;
global using Valour.Api.Items.Messages;
global using Valour.Api.Items.Users;
global using Valour.Shared.Items.Messages.Embeds;
global using Valour.Shared.Authorization;
global using System.Net.Http.Json;
global using Valour.Net.Client;
global using PopeAI.Models;
global using PopeAI.Database;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using Microsoft.EntityFrameworkCore;
global using Valour.Net.CommandHandling;
global using Valour.Net.CommandHandling.Attributes;
global using System.Collections.Concurrent;
global using Valour.Api.Client;
global using PopeAI.Database.Models.Users;
global using PopeAI.Database.Managers;
global using PopeAI.Database.Models.Elements;
global using PopeAI.Database.Config;
global using PopeAI.Database.Caching;
global using PopeAI.Database.Models.Bot;
global using PopeAI.Bot.Managers;
global using PopeAI.Database.Models.Planets;
global using Microsoft.AspNetCore;
global using PopeAI.Database.Models.Messaging;
global using System.IO;

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace PopeAI;

class Program
{
    static HttpClient client = new();

    public static byte[] StringToByteArray(string hex) {
        return Enumerable.Range(0, hex.Length)
                        .Where(x => x % 2 == 0)
                        .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                        .ToArray();
    }

    public static async Task Main(string[] args)
    {
        // load in the config file
        ConfigManger.Load();

        PopeAIDB.DbFactory = PopeAIDB.GetDbFactory();

        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();

        string sql = PopeAIDB.GenerateSQL();

        try {
            await File.WriteAllTextAsync("../Database/Definitions.sql", sql);
        }
        catch (Exception e)
        {
            
        }

        PopeAIDB.RawSqlQuery<string>(sql, null, true);

        await DBCache.Load();

        ValourNetClient.AddPrefix("/");
        //ValourNetClient.ExecuteMessagesInParallel = true;
        ValourNetClient.BaseUrl = "https://localhost:3001/";
        
        StatManager.selfstat = await BotStat.GetAsync(1);
        PopeAIDB.botTime = await BotTime.GetAsync(1);

        int worker = 0;
        int io = 0;
        ThreadPool.GetAvailableThreads(out worker, out io);
        
        Console.WriteLine("Thread pool threads available at startup: ");
        Console.WriteLine("   Worker threads: {0:N0}", worker);
        Console.WriteLine("   Asynchronous I/O threads: {0:N0}", io);

        await ValourNetClient.Start(ConfigManger.Config.Email,ConfigManger.Config.BotPassword);

        UpdateHourly();

        MessageManager.Run();

        while (true)
        {
            try
            {
                await DBCache.SaveAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            #if DEBUG
            await Task.Delay(1000);
            #else
            await Task.Delay(60000);
            #endif
        }
    }

    static async Task UpdateHourly() {
        while (true) {
            try
            {
                await StatManager.CheckStats();
                await DailyTaskManager.UpdateDailyTasks();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            #if DEBUG
            await Task.Delay(1000);
            #else
            await Task.Delay(60000);
            #endif
        }
    }
}

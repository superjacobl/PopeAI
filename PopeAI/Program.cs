﻿global using Valour.Sdk.Models;
global using Valour.Sdk.Models.Messages.Embeds;
global using Valour.Sdk.Models.Messages.Embeds.Items;
global using Valour.Sdk.Models.Messages.Embeds.Styles;
global using Valour.Sdk.Models.Messages.Embeds.Styles.Basic;
//global using Valour.Sdk.Models.Messages.Embeds.Styles.Bootstrap;
global using Valour.Sdk.Models.Messages.Embeds.Styles.Flex;
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
global using Valour.Sdk.Client;
global using PopeAI.Database.Models.Users;
global using PopeAI.Database.Managers;
global using PopeAI.Database.Models.Elements;
global using PopeAI.Database.Config;
global using PopeAI.Database.Caching;
global using PopeAI.Database.Models.Bot;
global using PopeAI.Bot.Managers;
global using PopeAI.Database.Models.Planets;
global using Microsoft.AspNetCore;
global using System.IO;

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using Database.Managers;
using Valour_Bot.Commands.EggCoopGame;

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

        ValourClient.OnMessageDeleted += async (message) =>
        {

        };

		ValourNetClient.AddPrefix("/");
        ValourNetClient.ExecuteMessagesInParallel = false;
        ValourNetClient.ExecuteInteractionsInParallel = true;
#if DEBUG
        ValourNetClient.OnlyRunCommandsIfFromThisUserId = 12201879245422592;
#endif

		//ValourNetClient.BaseUrl = "http://localhost:5000/";

		StatManager.selfstat = await BotStat.GetAsync(1);
        PopeAIDB.botTime = await BotTime.GetAsync(1);

        int worker = 0;
        int io = 0;
        ThreadPool.GetAvailableThreads(out worker, out io);
        
        Console.WriteLine("Thread pool threads available at startup: ");
        Console.WriteLine("   Worker threads: {0:N0}", worker);
        Console.WriteLine("   Asynchronous I/O threads: {0:N0}", io);

        Valour.Shared.Logger.OnLog += async (message, color) => {
            Console.WriteLine(message);
        };

		await ValourNetClient.Start(ConfigManger.Config.Email,ConfigManger.Config.BotPassword);

        MessageManager.Run();

        await MessageQueueForChannelConversationsManager.StartAsync();
		await MessageVCRewardManager.StartAsync();

		await EggCoopGame.StartAsync();

        while (true)
        {
            try
            {
                await DBCache.SaveAsync();
				await StatManager.CheckStats();
				await DailyTaskManager.UpdateDailyTasks();
			}
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            ValourClient.Self.Status = $"Storing {StatManager.selfstat.StoredMessages} messages";
#if !DEBUG
            await Valour.Sdk.Items.LiveModel.UpdateAsync(ValourClient.Self);
#endif
#if DEBUG
            await Task.Delay(1000);
#else
            await Task.Delay(60000);
#endif
        }
    }
}

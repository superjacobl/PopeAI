using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using PopeAI.Database;
using Microsoft.EntityFrameworkCore;
using PopeAI.Models;
using Valour.Net;
using Valour.Net.Models;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;

namespace PopeAI.Commands.Generic
{
    public class Stats : CommandModuleBase
    {
        public static Dictionary<ulong, string> ScrambledWords = new Dictionary<ulong, string>();
        static Random rnd = new Random();

        [Group("stats")]
        public class StatsGroup : CommandModuleBase
        {
            PopeAIDB DBContext = new PopeAIDB(PopeAIDB.DBOptions);

            [Command("")]
            [Summary("Shows available commands.")]
            public async Task StatsHelp(CommandContext ctx)
            {
                await ctx.ReplyAsync("Available Commands: /stats messages, /stats coins");
            }

            [Command("coins")]
            [Summary("Shows available commands.")]
            public async Task StatsMessages(CommandContext ctx)
            {
                List<Stat> stats = await Task.Run(() => Client.DBContext.Stats.Where(x => x.PlanetId == ctx.Planet.Id).OrderByDescending(x => x.Time).Take(6).ToList());
                List<int> data = new List<int>();
                foreach (Stat stat in stats)
                {
                    data.Add((int)stat.NewCoins);
                }
                data.Reverse();
                await PostGraph(ctx, data, "coins");
            }

            [Command("messages")]
            [Summary("Shows available commands.")]
            public async Task StatsCoins(CommandContext ctx)
            {
                List<Stat> stats = await Task.Run(() => Client.DBContext.Stats.Where(x => x.PlanetId == ctx.Planet.Id).OrderByDescending(x => x.Time).Take(6).ToList());
                List<int> data = new List<int>();
                foreach (Stat stat in stats)
                {
                    data.Add((int)stat.MessagesSent);
                }
                data.Reverse();
                await PostGraph(ctx, data, "messages");
            }
        }

        static async Task PostGraph(CommandContext ctx, List<int> data, string dataname)
        {
            string content = "";
            int maxvalue = data.Max();

            // make sure that the max-y is 10

            double muit = 10 / (double)maxvalue;

            List<int> newdata = new List<int>();

            foreach (int num in data)
            {
                double n = (double)num * muit;
                if (n < 0) {
                    n = 0;
                }
                newdata.Add((int)n);
            }

            data = newdata;

            List<string> rows = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                rows.Add("");
            }
            string space = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            foreach (int num in data)
            {
                for (int i = 0; i < num; i++)
                {
                    rows[i] += "⬜";
                }
                for (int i = num; i < 10; i++)
                {
                    rows[i] += space;
                    Console.WriteLine(i);
                }
            }

            // build the bar graph

            rows.Reverse();
            foreach (string row in rows)
            {
                content += $"{row}\n";
            }

            // build the x-axis labels

            content += " ";

            for (int i = data.Count(); i > 0; i--)
            {
                content += $"{i}h&nbsp;";
            }

            content += "\n";

            // build the how much does 1 box equal

            content += $"⬜ = {(int)maxvalue / 10} {dataname}";
            await ctx.ReplyAsync(content);
        }
    }
}
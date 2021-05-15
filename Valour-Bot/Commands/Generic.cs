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

/*
stats
roll
*/

namespace PopeAI.Commands.Generic
{
    public class Generic : CommandModuleBase
    {
        public static Dictionary<ulong, string> ScrambledWords = new Dictionary<ulong, string>();
        static Random rnd = new Random();

        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            TimeSpan diff = DateTime.UtcNow-ctx.Message.TimeSent;
            int milli = diff.Milliseconds;
            await ctx.ReplyAsync($"Pong {milli}ms");
        }

        [Command("help")]
        [Summary("Returns all commands")]
        public async Task HelpPage(CommandContext ctx, int page)
        {
            int skip = page*10;
            string content = "| command |\n| :-: |\n";
            foreach (Help help in Client.DBContext.Helps.Skip(skip).Take(10))
            {
                content += $"| {help.Message} |\n";
            }
            await ctx.ReplyAsync(content);
        }

        [Command("help")]
        [Summary("Returns all commands")]
        public async Task Help(CommandContext ctx)
        {
            string content = "| command |\n| :-: |\n";
            foreach (Help help in Client.DBContext.Helps.Take(10))
            {
                content += $"| {help.Message} |\n";
            }
            await ctx.ReplyAsync(content);
        }

        [Command("isdiscordgood")]
        [Summary("Determines if discord is good or bad.")]
        public async Task IsDiscordGood(CommandContext ctx)
        {
            await ctx.ReplyAsync("no, dickcord is bad!");
        }
    }
}
using System.Diagnostics;
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

            Stopwatch sw = new();
            sw.Start();
            await ValourClient.Http.GetStringAsync("https://valour.gg/api/ping");
            sw.Stop();
            await ctx.ReplyAsync($"Pong {(int)sw.ElapsedMilliseconds}ms\n");
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
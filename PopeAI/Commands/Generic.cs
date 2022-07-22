using System.Diagnostics;
using StringMath;

namespace PopeAI.Commands.Generic
{
    public class Generic : CommandModuleBase
    {
        public static Dictionary<long, string> ScrambledWords = new Dictionary<long, string>();
        static Random rnd = new Random();
        public static Calculator calculator = new();

        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            Task.Run(async () => {
                Stopwatch sw = new();
                sw.Start();
                await ValourClient.Http.GetStringAsync("https://app.valour.gg/api/ping");
                sw.Stop();
                ctx.ReplyAsync($"Pong {(int)sw.ElapsedMilliseconds}ms\n");
            });
        }

        [Command("longping")]
        public async Task LongPing(CommandContext ctx)
        {
            Task.Run(async () => {
                Stopwatch sw = new();
                sw.Start();
                for (int i = 0; i < 20; i++)
                {
                    await ValourClient.Http.GetStringAsync("https://app.valour.gg/api/ping");
                }
                sw.Stop();
                ctx.ReplyAsync($"Pong {(int)(sw.ElapsedMilliseconds/20)}ms\n");
            });
        }

        [Command("pfp")]
        public async Task GetPfp(CommandContext ctx)
        {
            string pfp = await ctx.Member.GetPfpUrlAsync();
            if (pfp == "/media/icon-512.png") {
                ctx.ReplyAsync("https://app.valour.gg/_content/Valour.Client/icon-512.png");
                return;
            }
            ctx.ReplyAsync(pfp);
        }

        [Command("pfp")]
        public async Task GetPfp(CommandContext ctx, PlanetMember member)
        {
            string pfp = await member.GetPfpUrlAsync();
            if (pfp == "/media/icon-512.png") {
                ctx.ReplyAsync("https://app.valour.gg/_content/Valour.Client/icon-512.png");
                return;
            }
            ctx.ReplyAsync(pfp);
        }

        [Command("calc")]
        public async Task Calc(CommandContext ctx, [Remainder] string content) 
        {
            ctx.ReplyAsync($"The result is: {calculator.Evaluate(content)}");
        }

        [Command("help")]
        [Summary("Returns all commands")]
        public async Task HelpPage(CommandContext ctx, int page)
        {
            using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
            int skip = page*10;
            string content = "| command |\n| :-: |\n";
            foreach (Help help in dbctx.Helps.Skip(skip).Take(10))
            {
                content += $"| {help.Message} |\n";
            }
            ctx.ReplyAsync(content);
        }

        [Command("help")]
        [Summary("Returns all commands")]
        public async Task Help(CommandContext ctx)
        {
            using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
            string content = "| command |\n| :-: |\n";
            foreach (Help help in dbctx.Helps.Take(10))
            {
                content += $"| {help.Message} |\n";
            }
            ctx.ReplyAsync(content);
        }

        [Command("isdiscordgood")]
        [Summary("Determines if discord is good or bad.")]
        public Task IsDiscordGood(CommandContext ctx)
        {
            return ctx.ReplyAsync("no, dickcord is bad!");
        }
    }
}
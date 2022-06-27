using Database.Models.Planets;

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
                List<ulong> data = new List<ulong>();
                foreach (Stat stat in stats)
                {
                    data.Add((ulong)stat.NewCoins);
                }
                data.Reverse();
                await PostGraph(ctx, data, "coins");
            }

            [Command("messages")]
            [Summary("Shows available commands.")]
            public async Task StatsCoins(CommandContext ctx)
            {
                List<Stat> stats = await Task.Run(() => Client.DBContext.Stats.Where(x => x.PlanetId == ctx.Planet.Id).OrderByDescending(x => x.Time).Take(6).ToList());
                List<ulong> data = new List<ulong>();
                foreach (Stat stat in stats)
                {
                    data.Add(stat.MessagesSent);
                }
                data.Reverse();
                await PostGraph(ctx, data, "messages");
            }
        }

        static async Task PostGraph(CommandContext ctx, List<ulong> data, string dataname)
        {
            string content = "";
            ulong maxvalue = data.Max();

            // make sure that the max-y is 10

            double muit = 10 / (double)maxvalue;

            List<ulong> newdata = new List<ulong>();

            foreach (ulong num in data)
            {
                double n = (double)num * muit;
                if (n < 0) {
                    n = 0;
                }
                newdata.Add((ulong)n);
            }

            data = newdata;

            List<string> rows = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                rows.Add("");
            }
            string space = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            //string space = "&ensp;&ensp;&nbsp;&nbsp;";
            foreach (int num in data)
            {
                for (int i = 0; i < num; i++)
                {
                    rows[i] += "⬜";
                }
                for (int i = num; i < 10; i++)
                {
                    rows[i] += space;
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
            Console.WriteLine($"chars: {content.Length}");
            await ctx.ReplyAsync(content);
        }
    }
}
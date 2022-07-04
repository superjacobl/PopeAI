namespace PopeAI.Commands.Stats;

public class Stats : CommandModuleBase
{
    public static Dictionary<long, string> ScrambledWords = new();
    static Random rnd = new();

    [Group("stats")]
    public class StatsGroup : CommandModuleBase
    {
        [Command("")]
        [Summary("Shows available commands.")]
        public Task StatsHelp(CommandContext ctx)
        {
            return ctx.ReplyAsync("Available Commands: /stats messages, /stats coins");
        }

        [Command("coins")]
        [Summary("Shows available commands.")]
        public async Task StatsMessages(CommandContext ctx)
        {
            using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
            List<Stat> stats = await dbctx.Stats
                .Where(x => x.PlanetId == ctx.Planet.Id)
                .OrderByDescending(x => x.Time)
                .Take(6)
                .ToListAsync();
            List<int> data = new();
            foreach (Stat stat in stats)
            {
                data.Add(stat.NewCoins);
            }
            data.Reverse();
            await PostGraph(ctx, data, "coins");
        }

        [Command("messages")]
        [Summary("Shows available commands.")]
        public async Task StatsCoins(CommandContext ctx)
        {
            using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
            List<Stat> stats = await dbctx.Stats
                .Where(x => x.PlanetId == ctx.Planet.Id)
                .OrderByDescending(x => x.Time)
                .Take(6)
                .ToListAsync();
            List<int> data = new();
            foreach (Stat stat in stats)
            {
                data.Add(stat.MessagesSent);
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

        List<int> newdata = new();

        foreach (int num in data)
        {
            double n = num * muit;
            if (n < 0) {
                n = 0;
            }
            newdata.Add((int)n);
        }

        data = newdata;

        List<string> rows = new();
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

        content += $"⬜ = {maxvalue / 10} {dataname}";
        Console.WriteLine($"chars: {content.Length}");
        await ctx.ReplyAsync(content);
    }
}
using Valour.Api.Items.Messages.Embeds.Styles.Flex;

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
                .Take(7)
                .ToListAsync();
            List<int> data = new();
			List<string> xaxisdata = new();
			foreach (Stat stat in stats)
            {
                data.Add(stat.NewCoins);
				xaxisdata.Add(stat.Time.ToString("MMM dd"));
			}
            data.Reverse();
            data.Add((await CurrentStat.GetAsync(ctx.Planet.Id)).NewCoins);
			xaxisdata.Reverse();
			xaxisdata.Add(DateTime.UtcNow.ToString("MMM dd"));
			await PostGraph(ctx, xaxisdata, data, "coins");
        }

        [Command("messages")]
        [Summary("Shows available commands.")]
        public async Task StatsCoins(CommandContext ctx)
        {
            using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
            List<Stat> stats = await dbctx.Stats
                .Where(x => x.PlanetId == ctx.Planet.Id)
                .OrderByDescending(x => x.Time)
                .Take(7)
                .ToListAsync();
            List<int> data = new();
            List<string> xaxisdata = new();
            foreach (Stat stat in stats)
            {
                data.Add(stat.MessagesSent);
                xaxisdata.Add(stat.Time.ToString("MMM dd"));
            }
            data.Reverse();
            data.Add((await CurrentStat.GetAsync(ctx.Planet.Id)).MessagesSent);
            xaxisdata.Reverse();
			xaxisdata.Add(DateTime.UtcNow.ToString("MMM dd"));
            await PostGraph(ctx, xaxisdata, data, "messages");
        }
    }

	static async Task OldPostGraph(CommandContext ctx, List<int> data, string dataname)
	{

		// TODO: do this
		// --
		//    \             /
		//     \    -- -- -- 
		//      \  /
		//       -- 
		// well actually for right now, use a bar graph

		string content = "";
		int maxvalue = data.Max();

		// make sure that the max-y is 10

		double muit = 10 / (double)maxvalue;

		List<int> newdata = new();

		foreach (int num in data)
		{
			double n = num * muit;
			if (n < 0)
			{
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
			content += $"{i}d&nbsp;";
		}

		content += "\n";

		// build the how much does 1 box equal

		content += $"⬜ = {maxvalue / 10} {dataname}";
		Console.WriteLine($"chars: {content.Length}");
		ctx.ReplyAsync(content);
	}

	static async Task PostGraph(CommandContext ctx, List<string> xaxisdata, List<int> data, string dataname)
    {

        // TODO: do this
        // --
        //    \             /
        //     \    -- -- -- 
        //      \  /
        //       -- 
        // well actually for right now, use a bar graph

        string content = "";
        int maxvalue = data.Max();
        Console.WriteLine($"Max Value: {maxvalue}");

		// make sure that the max-y is 175px

		double muit = (double)175 / (double)maxvalue;

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

        var embed = new EmbedBuilder()
            .AddPage($"Graph of {ctx.Planet.Name}'s {dataname}")
                .WithStyles(
                    new Width(new Size(Unit.Pixels, 360)),
					new Height(new Size(Unit.Pixels, 250))
				)
                .AddRow()
                    .WithStyles(
                        FlexDirection.Row,
						new Width(new Size(Unit.Pixels, 330)),
					    new Height(new Size(Unit.Pixels, 175))
                       
					);

		// build y-axis label 

		// use 5 labels
		for (int i = 1; i < 6; i++)
		{
            embed.AddText($"{(int)((5-i)*(maxvalue/5))}")
                .WithStyles(
                    new Position(left: new Size(Unit.Pixels, 7), top: new Size(Unit.Pixels, (i - 1) * (200 / 5) + 38))
                );
		}

        bool first = true;
		foreach (int num in data)
        {
			int h = (int)(num * muit);
			if (h > 175)
				h = 175;

			embed.WithRow();
            if (first)
			{
				embed.WithStyles(
					new Width(new Size(Unit.Pixels, 330 / data.Count)),
					new Height(new Size(Unit.Pixels, h)),
					new BackgroundColor(new Color(255, 255, 255)),
					new Margin(left: new Size(Unit.Pixels, 11), right: new Size(Unit.Pixels, 3), top: new Size(Unit.Auto))
				);
			}
			else
			{
				embed.WithStyles(
					new Width(new Size(Unit.Pixels, 330 / data.Count)),
					new Height(new Size(Unit.Pixels, h)),
					new BackgroundColor(new Color(255, 255, 255)),
					new Margin(right: new Size(Unit.Pixels, 3), top: new Size(Unit.Auto))
				);
			}
            embed.CloseRow();
            first = false;
		}

        // build x-axis label 

        embed.AddRow();
		// use 5 labels
		int moveoverleft = 25;
		for (int i = 1; i < data.Count+1; i++)
		{
            embed.AddText($"{xaxisdata[i-1]}")
				.WithStyles(
					new Position(left: new Size(Unit.Pixels, (i - 1) * (330 / data.Count) + moveoverleft), top: new Size(Unit.Pixels, 225)),
					new FontSize(new Size(Unit.Pixels, 12))
				);
			moveoverleft -= 1;
		}

		ctx.ReplyAsync(embed);
    }
}
using System;
using System.Collections.Generic;

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
            return ctx.ReplyAsync("Available Commands: /stats messages, /stats coins, /stats totalmessages");
        }

		[Command("totalmessages")]
		public async Task StatsTotalMessages(CommandContext ctx)
		{
			await using var user = await DBUser.GetAsync(ctx.Member.Id, true);
			using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
			var stats = await dbctx.Stats
				.Where(x => x.PlanetId == ctx.Planet.Id && x.StatType == StatType.Hourly)
				.OrderByDescending(x => x.Time)
				.Take(7*24)
				.ToListAsync();

			List<int> data = new();
			List<DateTime> protoxaxisdata = new();

			var last = stats.First().Time;
			foreach (var stat in stats)
			{
				data.Add(stat.TotalMessagesSent);
				protoxaxisdata.Add(stat.Time);
			}

			data.Reverse();
			data.Add((await CurrentStat.GetAsync(ctx.Planet.Id, _readonly: true)).TotalMessagesSent);

			protoxaxisdata.Reverse();
			if (last.Day == DateTime.UtcNow.Day)
				protoxaxisdata.Add(last.AddDays(1));
			else
				protoxaxisdata.Add(DateTime.UtcNow);

			List<string> xaxisdata = new();
			double muit = ((double)protoxaxisdata.Count()) / 5;
			for (int i = 0; i < 5; i++)
			{
				int x = (int)(i * muit);
				long lerped = Stats.linear(x, 0, protoxaxisdata.Count, protoxaxisdata[0].Ticks, protoxaxisdata.Last().Ticks);
				var date = new DateTime(lerped);
				xaxisdata.Add(String.Format("{0:M/d/yyyy} {0:t}", date));
			}
			await Stats.PostLineGraph(ctx, xaxisdata, data, $"{ctx.Planet.Name}'s Total Messages Over Time", true);
		}

		[Command("coins")]
        [Summary("Shows available commands.")]
        public async Task StatsMessages(CommandContext ctx)
        {
            using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
            List<Stat> stats = await dbctx.Stats
                .Where(x => x.PlanetId == ctx.Planet.Id && x.StatType == StatType.Daily)
                .OrderByDescending(x => x.Time)
                .Take(14)
                .ToListAsync();
            List<int> data = new();
			List<string> xaxisdata = new();
			var last = stats.First().Time;
			foreach (Stat stat in stats)
            {
                data.Add(stat.NewCoins);
				xaxisdata.Add(stat.Time.ToString("M/dd"));
			}
            data.Reverse();
            data.Add((await CurrentStat.GetAsync(ctx.Planet.Id)).DailyNewCoins);
			xaxisdata.Reverse();
			if (last.Day == DateTime.UtcNow.Day)
				xaxisdata.Add(last.AddDays(1).ToString("M/dd"));
			else
				xaxisdata.Add(DateTime.UtcNow.ToString("M/dd"));
			await PostBarGraph(ctx, xaxisdata, data, $"Graph of {ctx.Planet.Name} daily coin gain");
        }

        [Command("messages")]
        [Summary("Shows available commands.")]
        public async Task StatsCoins(CommandContext ctx)
        {
            using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
            List<Stat> stats = await dbctx.Stats
                .Where(x => x.PlanetId == ctx.Planet.Id && x.StatType == StatType.Daily)
                .Take(14)
                .ToListAsync();
            List<int> data = new();
            List<string> xaxisdata = new();
			var last = stats.First().Time;
			foreach (Stat stat in stats)
            {
                data.Add(stat.MessagesSent);
                xaxisdata.Add(stat.Time.ToString("M/dd"));
			}
            data.Reverse();
            data.Add((await CurrentStat.GetAsync(ctx.Planet.Id)).DailyMessagesSent);
            xaxisdata.Reverse();
			if (last.Day == DateTime.UtcNow.Day)
				xaxisdata.Add(last.AddDays(1).ToString("M/dd"));
			else
				xaxisdata.Add(DateTime.UtcNow.ToString("M/dd"));
            await PostBarGraph(ctx, xaxisdata, data, $"Graph of {ctx.Planet.Name}'s messages");
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

	static public int linear(int x, int x0, int x1, int y0, int y1)
	{
		if ((x1 - x0) == 0)
		{
			return (y0 + y1) / 2;
		}
		return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
	}

	static public long linear(long x, long x0, long x1, long y0, long y1)
	{
		if ((x1 - x0) == 0)
		{
			return (y0 + y1) / 2;
		}
		return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
	}

	static public double linear(double x, double x0, double x1, double y0, double y1)
	{
		if ((x1 - x0) == 0)
		{
			return (y0 + y1) / 2;
		}
		return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
	}

	public static async Task PostLineGraph(CommandContext ctx, List<string> xaxisdata, List<int> xdata, string graphname = "", bool startfrommin = false)
	{
		int height = 250;
		int width = 365;
		var embed = new EmbedBuilder()
			.AddPage(graphname)
				.WithStyles(
					new Width(new Size(Unit.Pixels, width)),
					new Height(new Size(Unit.Pixels, height))
				);

		int maxvalue = xdata.Max();
		int minvalue = xdata.Min() - 3;
		if (minvalue < 0)
			minvalue = 0;

		// make sure that the max-y is 175px
		double muit = 0;

		if (startfrommin)
		{
			muit = 175 / ((double)maxvalue - (double)minvalue);
		}
		else
			muit = 175 / (double)maxvalue;

		// each "x" is 7px wide and ~14px high
		double neededxvalues = (width-70) / 7;

		List<double> newxdata = new();
		double i = 0;
		double eachdataequalsi = (double)neededxvalues / (double)(xdata.Count-1);

		// if xdata contains less than *neededxvalues*, then we need to fill in the data
		if (xdata.Count < neededxvalues)
		{
			i = 1;
			int dataindex = 1;
			double prevdatavalue = xdata[0];
			for (int j = 0; j < neededxvalues; j++)
			{
				if (i >= eachdataequalsi && dataindex+1 <= xdata.Count-1)
				{
					i = 0;
					prevdatavalue = xdata[dataindex];
					dataindex += 1;
				}

				double progressToNextValue = i / eachdataequalsi;
				double value = prevdatavalue * (1 - progressToNextValue);
				value += xdata[dataindex] * (progressToNextValue);
				//double value = linear((double)j, (((double)dataindex) - 1) * eachdataequalsi, dataindex * eachdataequalsi, prevdatavalue, xdata[dataindex]);
				newxdata.Add(value);
				i += 1;
			}
		}

		Console.WriteLine(String.Join(", ", xdata));
		Console.WriteLine("NewxData: "+String.Join(", ", newxdata));

		//newxdata.Reverse();
		i = 0;
		foreach (var value in newxdata)
		{
			int x = (int)(i * 7 + 50);
			if (x > 340)
				x = 340;
			int y = 0;
			if (startfrommin)
				y = (int)((value - minvalue) * muit);
			else
				y = (int)(value * muit);
			if (y > 205)
				y = 205;
			y = (205 - y);
			y += 8;

			embed
				.AddText("x")
					.WithStyles(
						new Position(left: new Size(Unit.Pixels, x), top: new Size(Unit.Pixels, y))
					);
			i += 1;
		}

		// build y-axis label 

		// use 5 labels
		for (int j = 1; j < 6; j++)
		{
			int l = 0;
			if (startfrommin)
				l = (int)((5 - j + 1) * ((maxvalue - minvalue) / 5) + minvalue);
			else
				l = (int)((5 - j + 1) * (maxvalue / 5));
			embed.AddText($"{l}")
				.WithStyles(
					new Position(left: new Size(Unit.Pixels, 7), top: new Size(Unit.Pixels, (j - 1) * (200 / 5) + 38))
				);
		}

		// build x-axis label 

		// use 5 labels
		for (int j = 1; j < xaxisdata.Count + 1; j++)
		{
			embed.AddText($"{xaxisdata[j - 1]}")
			.WithStyles(
					new Position(left: new Size(Unit.Pixels, (j - 1) * (300 / xaxisdata.Count) + 50), top: new Size(Unit.Pixels, 225)),
					new FontSize(new Size(Unit.Pixels, 11))
				);
		}

		ctx.ReplyAsync(embed);
	}

	public static async Task PostBarGraph(CommandContext ctx, List<string> xaxisdata, List<int> data, string graphname = "", bool startfrommin = false)
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
		int minvalue = data.Min() - 3;
		if (minvalue < 0)
			minvalue = 0;

		Console.WriteLine($"Max Value: {maxvalue}");

		// make sure that the max-y is 175px
		double muit = 0;

		if (startfrommin)
		{
			muit = 175 / ((double)maxvalue - (double)minvalue);
		}
		else
			muit = 175 / (double)maxvalue;

        var embed = new EmbedBuilder()
            .AddPage(graphname)
                .WithStyles(
                    new Width(new Size(Unit.Pixels, 360)),
					new Height(new Size(Unit.Pixels, 250))
				)
                .AddRow()
                    .WithStyles(
                        FlexDirection.Row,
						new Width(new Size(Unit.Pixels, 325)),
					    new Height(new Size(Unit.Pixels, 175))
					);

		// build y-axis label 

		// use 5 labels
		for (int i = 1; i < 6; i++)
		{
			int l = 0;
			if (startfrommin)
				l = (int)((5 - i + 1) * ((maxvalue - minvalue) / 5) + minvalue);
			else
				l = (int)((5 - i + 1) * (maxvalue / 5));
			embed.AddText($"{l}")
                .WithStyles(
                    new Position(left: new Size(Unit.Pixels, 7), top: new Size(Unit.Pixels, (i - 1) * (200 / 5) + 38))
                );
		}

        bool first = true;
		foreach (int num in data)
        {
			int h = 0;
			if (startfrommin)
				h = (int)((num - minvalue) * muit);
			else
				h = (int)(num * muit);
			if (h > 175)
				h = 175;

			embed.WithRow();
            if (first)
			{
				embed.WithStyles(
					new Width(new Size(Unit.Pixels, 325 / data.Count)),
					new Height(new Size(Unit.Pixels, h)),
					new BackgroundColor(new Color(255, 255, 255)),
					new Margin(left: new Size(Unit.Pixels, 34), right: new Size(Unit.Pixels, 6), top: new Size(Unit.Auto))
				);
			}
			else
			{
				embed.WithStyles(
					new Width(new Size(Unit.Pixels, 325 / data.Count)),
					new Height(new Size(Unit.Pixels, h)),
					new BackgroundColor(new Color(255, 255, 255)),
					new Margin(right: new Size(Unit.Pixels, 6), top: new Size(Unit.Auto))
				);
			}
            embed.CloseRow();
            first = false;
		}

        // build x-axis label 

        embed.AddRow();
		// use 5 labels
		int moveoverleft = 50;
		for (int i = 1; i < data.Count+1; i++)
		{
            embed.AddText($"{xaxisdata[i-1]}")
				.WithStyles(
					new Position(left: new Size(Unit.Pixels, (i - 1) * (300 / data.Count) + moveoverleft), top: new Size(Unit.Pixels, 225)),
					new FontSize(new Size(Unit.Pixels, 11))
				);
			moveoverleft -= 1;
		}

		ctx.ReplyAsync(embed);
    }
}
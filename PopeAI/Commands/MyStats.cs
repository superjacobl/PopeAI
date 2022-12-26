using System.Collections.Generic;
using System.Security.Cryptography;
using Valour.Api.Items.Messages.Embeds.Styles.Flex;

namespace PopeAI.Commands.Stats;

public class MyStats : CommandModuleBase
{
    public static Dictionary<long, string> ScrambledWords = new();
    static Random rnd = new();

    [Group("mystats")]
    public class StatsGroup : CommandModuleBase
    {
        [Command("")]
        public Task StatsHelp(CommandContext ctx)
        {
            return ctx.ReplyAsync("Available Commands: /mystats messages, /mystats xp");
        }

        [Command("messages")]
        public async Task StatsCoins(CommandContext ctx)
        {
			await using var user = await DBUser.GetAsync(ctx.Member.Id, true);
			using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
            var stats = await dbctx.UserStats
                .Where(x => x.MemberId == ctx.Member.Id)
                .OrderByDescending(x => x.Date)
                .Take(14)
                .ToListAsync();

            List<int> data = new();
			List<int> protoxaxisdata = new();

			var last = stats.First().Date;
			foreach (var stat in stats)
            {
                data.Add(stat.TotalMessages);
				protoxaxisdata.Add(stat.Date.DayNumber);
			}

            data.Reverse();
            data.Add(user.Messages);

			protoxaxisdata.Reverse();
			if (last.Day == DateTime.UtcNow.Day)
				protoxaxisdata.Add(last.AddDays(1).DayNumber);
			else
				protoxaxisdata.Add(DateOnly.FromDateTime(DateTime.UtcNow).DayNumber);

			List<string> xaxisdata = new();
			double muit = ((double)protoxaxisdata.Count()) / 5;
			for (int i = 0; i < 5; i++)
			{
				int x = (int)(i * muit);
				int lerped = Stats.linear(x, 0, protoxaxisdata.Count, protoxaxisdata[0], protoxaxisdata.Last());
				xaxisdata.Add(DateOnly.FromDayNumber(lerped).ToString("MMM dd"));
			}
			await Stats.PostLineGraph(ctx, xaxisdata, data, $"{ctx.Member.Nickname}'s Messages Over Time", true);
		}

		[Command("xp")]
		public async Task StatsXp(CommandContext ctx)
		{
			await using var user = await DBUser.GetAsync(ctx.Member.Id, true);
			using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
			var stats = await dbctx.UserStats
				.Where(x => x.MemberId == ctx.Member.Id)
				.OrderByDescending(x => x.Date)
				.Take(14)
				.ToListAsync();

			List<int> data = new();
			List<int> protoxaxisdata = new();

			var last = stats.First().Date;
			foreach (var stat in stats)
			{
				data.Add((int)stat.TotalXp);
				protoxaxisdata.Add(stat.Date.DayNumber);
			}
			data.Reverse();
			data.Add((int)user.Xp);
			protoxaxisdata.Reverse();
			if (last.Day == DateTime.UtcNow.Day)
				protoxaxisdata.Add(last.AddDays(1).DayNumber);
			else
				protoxaxisdata.Add(DateOnly.FromDateTime(DateTime.UtcNow).DayNumber);

			List<string> xaxisdata = new();
			double muit = ((double)protoxaxisdata.Count()) / 5;
			for (int i = 0; i < 5; i++)
			{
				int x = (int)(i * muit);
				int lerped = Stats.linear(x, 0, protoxaxisdata.Count, protoxaxisdata[0], protoxaxisdata.Last());
				xaxisdata.Add(DateOnly.FromDayNumber(lerped).ToString("MMM dd"));
			}

			await Stats.PostLineGraph(ctx, xaxisdata, data, $"{ctx.Member.Nickname}'s Xp Over Time", true);
		}
	}
}
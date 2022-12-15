using System.Collections.Generic;
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
                .Take(7)
                .ToListAsync();
            List<int> data = new();
            List<string> xaxisdata = new();
			var last = stats.First().Date;
			foreach (var stat in stats)
            {
                data.Add(stat.TotalMessages);
                xaxisdata.Add(stat.Date.ToString("MMM dd"));
			}
            data.Reverse();
            data.Add(user.Messages);
            xaxisdata.Reverse();
			if (last.Day == DateTime.UtcNow.Day)
				xaxisdata.Add(last.AddDays(1).ToString("MMM dd"));
			else
				xaxisdata.Add(DateTime.UtcNow.ToString("MMM dd"));
            await Stats.PostGraph(ctx, xaxisdata, data, $"Graph of {ctx.Member.Nickname}'s Messages Over Time");
        }

		[Command("xp")]
		public async Task StatsXp(CommandContext ctx)
		{
			await using var user = await DBUser.GetAsync(ctx.Member.Id, true);
			using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
			var stats = await dbctx.UserStats
				.Where(x => x.MemberId == ctx.Member.Id)
				.OrderByDescending(x => x.Date)
				.Take(7)
				.ToListAsync();
			List<int> data = new();
			List<string> xaxisdata = new();
			var last = stats.First().Date;
			foreach (var stat in stats)
			{
				data.Add((int)stat.TotalXp);
				xaxisdata.Add(stat.Date.ToString("MMM dd"));
			}
			data.Reverse();
			data.Add((int)user.Xp);
			xaxisdata.Reverse();
			if (last.Day == DateTime.UtcNow.Day)
				xaxisdata.Add(last.AddDays(1).ToString("MMM dd"));
			else
				xaxisdata.Add(DateTime.UtcNow.ToString("MMM dd"));
			await Stats.PostGraph(ctx, xaxisdata, data, $"Graph of {ctx.Member.Nickname}'s Xp Over Time");
		}
	}
}
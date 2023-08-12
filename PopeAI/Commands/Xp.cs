using Database.Managers;
using PopeAI.Commands.Banking;
using PopeAI.Database.Models.Planets;
using System.Collections.Concurrent;

namespace PopeAI.Commands.Xp;

public class Xp : CommandModuleBase
{
    [Event(EventType.AfterCommand)]
    public void AfterCommand(CommandContext ctx)
    {
        StatManager.selfstat.TimeTakenTotal += (long)(DateTime.UtcNow - ctx.CommandStarted).TotalMilliseconds;
        StatManager.selfstat.Commands += 1;
    }

    [Event(EventType.Message)]
    public async Task OnMessage(CommandContext ctx)
    {
        // put this message in queue for storage
        MessageManager.AddToQueue(ctx.Message);

        await StatManager.AddStat(CurrentStatType.Message, 1, ctx.Member.PlanetId);

        if (ctx.Message.AuthorUserId == long.Parse(ConfigManger.Config.BotId))
        {
            StatManager.selfstat.MessagesSentSelf += 1;
        }

        if (ctx.Message.AuthorUserId == long.Parse(ConfigManger.Config.BotId) || (await ctx.Member.GetUserAsync()).Bot) {
            return;
        }

		var info = await PlanetInfo.GetAsync(ctx.Planet.Id, _readonly: true);

		var user = await DBUser.GetAsync(ctx.Member.Id);

        if (user == null)
        {
            user = new(ctx.Member);
            user.DailyTasks = DailyTaskManager.GenerateNewDailyTasks(user.Id).ToList();
            DBCache.AddNew(user.Id, user);
        }

        if (DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - user.LastUpdatedDailyTasks.DayNumber >= 1) {
            DailyTaskManager.UpdateTasks(user);
            user.LastUpdatedDailyTasks = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        await StatManager.AddStat(CurrentStatType.UserMessage, 1, ctx.Member.PlanetId);
        await DailyTaskManager.DidTask(DailyTaskType.Messages, ctx.Member.Id, ctx, user);

		if (info is not null)
			user.NewMessage(ctx.Message, info);

        await user.UpdateDB();

        MessageQueueForChannelConversationsManager.AddToQueue(ctx.Message);
    }

    [Command("xp")]
    [Summary("Gives the user who sent the command their xp.")]
    public async Task SendXp(CommandContext ctx)
	{
		var info = await PlanetInfo.GetAsync(ctx.Planet.Id, _readonly: true);
        if (info is null || !info.HasEnabled(ModuleType.Xp))
            return;

        var user = await DBUser.GetAsync(ctx.Member.Id, true);

        var embed = new EmbedBuilder().AddPage($"{ctx.Member.Nickname}'s xp")
            .WithStyles(new FontSize(new Size(Unit.Pixels, 14)))
            .AddRow()
                .AddText("Message", Functions.Format(user.MessageXp))
                .AddText("Elemental", Functions.Format(user.ElementalXp))
                .AddText("Gaming", Functions.Format(user.GameXp))
            .AddRow()
                .AddText("Total Xp", Functions.Format(user.Xp));

		//embed.CurrentPage.FooterStyles = new() { new Width(new Size(Unit.Pixels, 225)) };
		//embed.CurrentPage.Footer = $"Ad: Youtube is the world's leading video sharing website, checkout it out now at https://youtube.com";

		ctx.ReplyAsync(embed);

        //ctx.ReplyAsync($"{ctx.Member.Nickname}'s xp: {(long)user.Xp} (msg xp: {(long)user.MessageXp}, elemental xp: {(long)user.ElementalXp})");
    }

    [Command("leaderboard")]
    [Alias("lb")]
    [Summary("Returns the leaderboard of the users with the most xp.")]
    public async Task Leaderboard(CommandContext ctx)
    {
		var info = await PlanetInfo.GetAsync(ctx.Planet.Id, _readonly: true);
		if (info is null || !info.HasEnabled(ModuleType.Xp))
			return;

		using var dbctx = PopeAIDB.DbFactory.CreateDbContext();

        List<DBUser> users = await dbctx.Users
            .Where(x => x.PlanetId == ctx.Planet.Id)
            .OrderByDescending(x => x.Xp)
            .Take(30)
            .ToListAsync();

        var embed = new EmbedBuilder().AddPage("Users ordered by Xp").AddRow();
        int i = 1;
        foreach (DBUser user in users)
        {
            PlanetMember member = await PlanetMember.FindAsync(user.Id, ctx.Planet.Id);
            embed.AddText(text:$"({i}) {member.Nickname} - {(long)user.Xp}xp").AddRow();
            i += 1;
            if (embed.CurrentPage.Children.Count > 10) {
                embed.AddPage("Users ordered by Xp").AddRow();
            }
        }
        ctx.ReplyAsync(embed);
    }

	public void LeaderboardDetailedAddTopRow(EmbedBuilder embed)
	{
        embed
            .AddRow()
				.WithStyles(
					FlexDirection.Column,
					FlexJustifyContent.SpaceBetween,
					new FlexAlignItems(AlignItem.Stretch),
                    new FontSize(new Size(Unit.Pixels, 14))
				)
			.WithRow()
				.WithStyles(
					FlexJustifyContent.SpaceBetween
				)
				.AddText("Place")
					.WithStyles(new Width(new Size(Unit.Pixels, 40)))
				.AddText("Name")
					.WithStyles(new Width(new Size(Unit.Pixels, 130)))
				.AddText("Xp")
					.WithStyles(new Width(new Size(Unit.Pixels, 50)))
				.AddText("Msg Xp")
					.WithStyles(new Width(new Size(Unit.Pixels, 60)))
				.AddText("Game Xp")
					.WithStyles(new Width(new Size(Unit.Pixels, 42)))
				.AddText("Minutes Active")
					.WithStyles(new Width(new Size(Unit.Pixels, 60)))
				.AddText("Avg Msg Length")
					.WithStyles(new Width(new Size(Unit.Pixels, 62)))
				.AddText("Messages")
					.WithStyles(new Width(new Size(Unit.Pixels, 62)))
			.CloseRow();
	}

	[Command("leaderboard_detailed")]
	[Alias("lbd")]
	[Summary("Returns the leaderboard of the users with the most xp.")]
	public async Task LeaderboardDetailed(CommandContext ctx)
	{
		var info = await PlanetInfo.GetAsync(ctx.Planet.Id, _readonly: true);
		if (info is null || !info.HasEnabled(ModuleType.Xp))
			return;

		using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
		List<DBUser> users = await dbctx.Users
			.Where(x => x.PlanetId == ctx.Planet.Id)
			.OrderByDescending(x => x.Xp)
			.Take(30)
			.ToListAsync();

        var embed = new EmbedBuilder()
            .AddPage("Users ordered by Xp");
        LeaderboardDetailedAddTopRow(embed);
		int i = 1;
		foreach (DBUser user in users)
		{
			PlanetMember member = await PlanetMember.FindAsync(user.Id, ctx.Planet.Id);
            string color = await member.GetRoleColorAsync();
            embed
                .WithRow()
                    .WithStyles(
                        FlexJustifyContent.SpaceBetween
                     )
                    .AddText(i.ToString())
						.WithStyles(new Width(new Size(Unit.Pixels, 40)))
					.AddText(member.Nickname)
						.WithStyles(
                            new Width(new Size(Unit.Pixels, 130)),
                            new TextColor(color)
                         )
					.AddText(((long)user.Xp).ToString())
						.WithStyles(new Width(new Size(Unit.Pixels, 50)))
                    .AddText(((long)user.MessageXp).ToString())
						.WithStyles(new Width(new Size(Unit.Pixels, 60)))
					.AddText(((long)user.GameXp).ToString())
						.WithStyles(new Width(new Size(Unit.Pixels, 42)))
					.AddText(user.ActiveMinutes.ToString())
						.WithStyles(new Width(new Size(Unit.Pixels, 60)))
					.AddText(user.AvgMessageLength.ToString())
						.WithStyles(new Width(new Size(Unit.Pixels, 62)))
					.AddText(user.Messages.ToString())
						.WithStyles(new Width(new Size(Unit.Pixels, 62)))
				.CloseRow();
			i += 1;
			if (embed.CurrentPage.Children[0].Children.Count() > 10)
			{
                embed.AddPage("Users ordered by Xp");
				LeaderboardDetailedAddTopRow(embed);
			}
		}
		ctx.ReplyAsync(embed);
	}
}
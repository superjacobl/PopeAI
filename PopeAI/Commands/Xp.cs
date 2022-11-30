using PopeAI.Database.Models.Planets;
using Valour.Api.Items.Messages.Embeds.Styles.Flex;

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

        var user = await DBUser.GetAsync(ctx.Member.Id);

        if (user == null)
        {
            using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
            user = new(ctx.Member);
            user.DailyTasks = DailyTaskManager.GenerateNewDailyTasks(user.Id).ToList();
            DBCache.Put(user.Id, user);
            dbctx.Users.Add(user);
            await dbctx.SaveChangesAsync();
        }

        if ((DateTime.UtcNow-user.LastSentMessage).TotalDays >= 1.5) {
            using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
            DailyTaskManager.UpdateTasks(user, dbctx);
            await dbctx.SaveChangesAsync();
        }

        user.NewMessage(ctx.Message);

        await StatManager.AddStat(CurrentStatType.UserMessage, 1, ctx.Member.PlanetId);
        await DailyTaskManager.DidTask(DailyTaskType.Messages, ctx.Member.Id, ctx, user);

        await user.UpdateDB();
    }

    [Command("xp")]
    [Summary("Gives the user who sent the command their xp.")]
    public async Task SendXp(CommandContext ctx)
    {
        var user = await DBUser.GetAsync(ctx.Member.Id, true);


        var embed = new EmbedBuilder().AddPage($"{ctx.Member.Nickname}'s xp")
            .AddRow()
                .AddText("Message", Functions.Format(user.MessageXp))
                .AddText("Elemental", Functions.Format(user.ElementalXp))
                .AddText("Gaming", Functions.Format(user.GameXp))
            .AddRow()
                .AddText("Total Xp", Functions.Format(user.Xp));

        ctx.ReplyAsync(embed);

        //ctx.ReplyAsync($"{ctx.Member.Nickname}'s xp: {(long)user.Xp} (msg xp: {(long)user.MessageXp}, elemental xp: {(long)user.ElementalXp})");
    }

    [Command("leaderboard")]
    [Alias("lb")]
    [Summary("Returns the leaderboard of the users with the most xp.")]
    public async Task Leaderboard(CommandContext ctx)
    {
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

	[Command("leaderboard_detailed")]
	[Alias("lbd")]
	[Summary("Returns the leaderboard of the users with the most xp.")]
	public async Task LeaderboardDetailed(CommandContext ctx)
	{
		using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
		List<DBUser> users = await dbctx.Users
			.Where(x => x.PlanetId == ctx.Planet.Id)
			.OrderByDescending(x => x.Xp)
			.Take(30)
			.ToListAsync();

        var embed = new EmbedBuilder()
            .AddPage("Users ordered by Xp")
                .AddRow()
                    .WithStyles(
                        FlexDirection.Column,
                        FlexJustifyContent.SpaceBetween,
                        new FlexAlignItems(AlignItem.Stretch)
                    )
                .WithRow()
                    .WithStyles(
						FlexJustifyContent.SpaceBetween
					)
                    .AddText("Place")
                        .WithStyles(new Width(new Size(Unit.Pixels, 40)))
                    .AddText("Name")
						.WithStyles(new Width(new Size(Unit.Pixels, 150)))
					.AddText("Xp")
						.WithStyles(new Width(new Size(Unit.Pixels, 50)))
				    .AddText("Message Xp")
						.WithStyles(new Width(new Size(Unit.Pixels, 90)))
					.AddText("Game Xp")
						.WithStyles(new Width(new Size(Unit.Pixels, 70)))
					.AddText("Minutes Active")
						.WithStyles(new Width(new Size(Unit.Pixels, 120)))
					.AddText("Avg Msg Length")
						.WithStyles(new Width(new Size(Unit.Pixels, 120)))
					.AddText("Messages")
						.WithStyles(new Width(new Size(Unit.Pixels, 70)))
				.CloseRow();
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
                            new Width(new Size(Unit.Pixels, 150)),
                            new TextColor(color)
                         )
					.AddText(((long)user.Xp).ToString())
						.WithStyles(new Width(new Size(Unit.Pixels, 50)))
                    .AddText(((long)user.MessageXp).ToString())
						.WithStyles(new Width(new Size(Unit.Pixels, 90)))
					.AddText(((long)user.GameXp).ToString())
						.WithStyles(new Width(new Size(Unit.Pixels, 70)))
					.AddText(user.ActiveMinutes.ToString())
						.WithStyles(new Width(new Size(Unit.Pixels, 120)))
					.AddText(user.AvgMessageLength.ToString())
						.WithStyles(new Width(new Size(Unit.Pixels, 120)))
					.AddText(user.Messages.ToString())
						.WithStyles(new Width(new Size(Unit.Pixels, 70)))
				.CloseRow();
			i += 1;
			if (embed.CurrentPage.Children.Count > 10)
			{
                break;
                //embed.AddPage("Users ordered by Xp").AddRow();
			}
		}
		ctx.ReplyAsync(embed);
	}
}
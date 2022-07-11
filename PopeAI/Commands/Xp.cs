using PopeAI.Database.Models.Planets;
using Valour.Shared.Items.Messages.Embeds;

namespace PopeAI.Commands.Xp;

public class Xp : CommandModuleBase
{
    [Event(EventType.AfterCommand)]
    public void AfterCommand(CommandContext ctx)
    {
        #if DEBUG
        StatManager.selfstat.TimeTakenTotal += (long)(DateTime.UtcNow - ctx.CommandStarted).TotalMilliseconds;
        StatManager.selfstat.Commands += 1;
        #endif
    }

    [Event(EventType.Message)]
    public async Task OnMessage(CommandContext ctx)
    {
        // put this message in queue for storage
        MessageManager.AddToQueue(ctx.Message);

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

        await StatManager.AddStat(CurrentStatType.Message, 1, ctx.Member.PlanetId);
        await DailyTaskManager.DidTask(DailyTaskType.Messages, ctx.Member.Id, ctx, user);

        await user.UpdateDB();
    }

    [Command("xp")]
    [Summary("Gives the user who sent the command their xp.")]
    public async Task SendXp(CommandContext ctx)
    {
        var user = await DBUser.GetAsync(ctx.Member.Id, true);

        // was way too ugly
        /*
        EmbedBuilder embed = new();
        var page = new EmbedPageBuilder()
            .AddText($"{ctx.Member.Nickname}'s Xp")
            .AddText(text:"&nbsp;")
            .AddText("Message Xp", ((long)user.MessageXp).ToString())
            .AddText("Elemental Xp", ((long)user.ElementalXp).ToString())
            .AddText("Total Xp", ((long)user.Xp).ToString());

        embed.AddPage(page);
        ctx.ReplyAsync(embed); */

        ctx.ReplyAsync($"{ctx.Member.Nickname}'s xp: {(long)user.Xp} (msg xp: {(long)user.MessageXp}, elemental xp: {(long)user.ElementalXp})");
    }

    [Command("leaderboard")]
    [Alias("lb")]
    [Summary("Returns the leaderboard of the users with the most xp.")]
    public async Task Leaderboard(CommandContext ctx)
    {
        List<DBUser> users = DBCache.GetAll<DBUser>()
            .Where(x => x.PlanetId == ctx.Planet.Id)
            .OrderByDescending(x => x.Xp)
            .Take(10)
            .ToList();

        EmbedBuilder embed = new();
        EmbedPageBuilder page = new();
        int i = 1;
        foreach (DBUser user in users)
        {
            PlanetMember member = await PlanetMember.FindAsync(user.Id, ctx.Planet.Id);
            page.AddText(text:$"({i}) {member.Nickname} - {(long)user.Xp}xp");
            i += 1;
            if (page.Items.Count() > 10) {
                embed.AddPage(page);
                page = new();
            }
        }
        embed.AddPage(page);
        ctx.ReplyAsync(embed);
    }
}
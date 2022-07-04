using PopeAI.Database.Models.Planets;
using Valour.Shared.Items.Messages.Embeds;

namespace PopeAI.Commands.Xp;

public class Xp : CommandModuleBase
{
    [Event(EventType.AfterCommand)]
    public void AfterCommand(CommandContext ctx)
    {
        StatManager.selfstat.TimeTakenTotal += (long)(DateTime.UtcNow - ctx.TimeReceived).TotalMilliseconds;
        StatManager.selfstat.Commands += 1;
    }

    [Event(EventType.Message)]
    public async Task OnMessage(CommandContext ctx)
    {
        // put this message in queue for storage
        MessageManager.AddToQueue(ctx.Message);

        if (ctx.Message.AuthorId == long.Parse(ConfigManger.Config.BotId) || (await ctx.Member.GetUserAsync()).Bot) {
            return;
        }

        DBUser user = await DBUser.GetAsync(ctx.Member.Id);

        if (user == null)
        {
            using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
            user = new(ctx.Member)
            {
                DailyTasks = DailyTaskManager.GenerateNewDailyTasks(user.Id).ToList()
            };
            DBCache.Put(user.Id, user);
            dbctx.Users.Add(user);
            await dbctx.SaveChangesAsync();
        }

        user.NewMessage(ctx.Message);

        await user.UpdateDB();

        StatManager.AddStat(CurrentStatType.Message, 1, ctx.Member.PlanetId);
        await DailyTaskManager.DidTask(DailyTaskType.Messages, ctx.Member.Id, ctx);
    }

    [Command("xp")]
    [Summary("Gives the user who sent the command their xp.")]
    public async Task SendXp(CommandContext ctx)
    {
        var user = await DBUser.GetAsync(ctx.Member.Id);
        EmbedBuilder embed = new();
        var page = new EmbedPageBuilder()
            .AddText($"{ctx.Member.Nickname}'s Xp")
            .AddText("Message Xp", ((long)user.MessageXp).ToString())
            .AddText("Elemental Xp", ((long)user.ElementalXp).ToString())
            .AddText("Total Xp", ((long)user.Xp).ToString());

        embed.AddPage(page);
        ctx.ReplyAsync(embed);
        user.UpdateDB();
    }

    [Command("leaderboard")]
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
            PlanetMember member = await PlanetMember.FindAsync(ctx.Planet.Id, user.UserId);
            page.AddText(text:$"({i}) {member.Nickname} - {(long)user.Xp}xp");
            i += 1;
            if (page.Items.Count() > 10) {
                embed.AddPage(page);
                page = new();
            }
        }
        embed.AddPage(page);
        await ctx.ReplyAsync(embed);
    }
}
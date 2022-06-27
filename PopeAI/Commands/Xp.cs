using PopeAI.Database.Models.Planets;

namespace PopeAI.Commands.Xp;

public class Xp : CommandModuleBase
{

    [Event(EventType.AfterCommand)]
    public async Task AfterCommand(CommandContext ctx)
    {
        StatManager.selfstat.TimeTakenTotal += (ulong)(DateTime.UtcNow - ctx.TimeReceived).TotalMilliseconds;
        StatManager.selfstat.Commands += 1;
    }

    [Event(EventType.Message)]
    public async Task OnMessage(CommandContext ctx)
    {
        // put this message in queue for storage
        MessageManager.AddToQueue(ctx.Message);

        if (ctx.Message.Author_Id == ulong.Parse(Client.Config.BotId) || (await ctx.Member.GetUserAsync()).Bot) {
            return;
        }

        // add this as a planet setting eventually

        await StatManager.AddStat(CurrentStatType.Message, 1, ctx.Message.Planet_Id);

        DBUser user = DBCache.Get<DBUser>(ctx.Member.Id);

        if (user == null)
        {
            user = new(ctx.Member);
        }

        user.NewMessage(ctx.Message);
    }

    [Command("xp")]
    [Summary("Gives the user who sent the command their xp.")]
    public async Task SendXp(CommandContext ctx)
    {
        DBUser DBUser = await Client.DBContext.Users.FirstOrDefaultAsync(x => x.UserId == ctx.Message.Author_Id && x.PlanetId == ctx.Planet.Id);
        await ctx.ReplyAsync($"{ctx.Member.Nickname}'s xp: {(ulong)DBUser.Xp}");
    }

    [Command("leaderboard")]
    [Summary("Returns the leaderboard of the users with the most xp.")]
    public async Task Leaderboard(CommandContext ctx)
    {
        List<DBUser> users = await Task.Run(() => Client.DBContext.Users.Where(x => x.PlanetId == ctx.Planet.Id).OrderByDescending(x => x.Xp).Take(10).ToList());
        EmbedBuilder embed = new EmbedBuilder();
        EmbedPageBuilder page = new EmbedPageBuilder();
        int i = 1;
        foreach (DBUser USER in users)
        {
            PlanetMember member = await PlanetMember.FindAsync(ctx.Planet.Id, USER.UserId);
            //page.AddText($"({i}) {member.Nickname}",  $"{(ulong)USER.Xp}xp");
            page.AddText(text:$"({i}) {member.Nickname} - {(ulong)USER.Xp}xp");
            //content += $"{member.Nickname} | {(ulong)USER.Xp} xp\n";
            i += 1;
            if (page.Items.Count() > 10) {
                embed.AddPage(page);
                page = new EmbedPageBuilder();
            }
        }
        embed.AddPage(page);
        await ctx.ReplyAsync(embed);
    }
}
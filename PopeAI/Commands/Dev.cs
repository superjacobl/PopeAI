namespace PopeAI.Commands.Dev;

public class Dev : CommandModuleBase
{
    [Command("userid")]
    [Summary("The user Id of the user who entered the command.")]
    public Task UserId(CommandContext ctx)
    {
        return ctx.ReplyAsync($"Your UserId is {ctx.Member.UserId}");
    }

    [Command("memberid")]
    [Summary("The Member Id of the user who entered the command.")]
    public Task MemberId(CommandContext ctx)
    {
        return ctx.ReplyAsync($"Your MemberId is {ctx.Member.Id}");
    }

    [Command("planetid")]
    [Summary("The Planet Id of the channel where the command was entered.")]
    public Task PlanetId(CommandContext ctx)
    {
        return ctx.ReplyAsync($"This planet's id is {ctx.Planet.Id}");
    }

    [Command("channelid")]
    [Summary("The Channel Id of the channel where the command was entered.")]
    public Task ChannelId(CommandContext ctx)
    {
        return ctx.ReplyAsync($"This channel's id is {ctx.Channel.Id}");
    }

    [Command("devinfo")]
    public async Task DevInfo(CommandContext ctx, PlanetMember member = null)
    {
        PlanetMember _member = null;
        if (member is null) 
            _member = ctx.Member;
        else
            _member = member;
        var embed = new EmbedBuilder().AddPage($"{_member.GetNameAsync()}'s Info")
            .AddRow()
                .AddText("User Id", _member.UserId.ToString())
                .AddText("Member Id", _member.Id.ToString())
            .AddRow()
                .AddText("Channel Id", ctx.Channel.Id.ToString())
                .AddText("Planet Id", ctx.Planet.Id.ToString())
            .AddRow()
                .AddText("Roles");
        foreach(var role in await _member.GetRolesAsync()) {
            embed.AddRow()
                .AddText(text: role.Name)
                    .WithStyles(new TextColor(new Color(role.Color)))
                .AddText(text: role.Id.ToString());
        }
        await ctx.ReplyAsync(embed);
    }

    [Command("dbtext")]
    public static async Task DatabaseInfoAsTextAynsc(CommandContext ctx)
    {
        if (ctx.Member.UserId != 12201879245422592)
        {
            return;
        }
        //string query = $"select (data_length + index_length) as Size, COUNT(Id), ((data_length + index_length)/COUNT(Id)) as avg_row_size from popeai.Messages, information_schema.tables where table_name = 'messages';";
        string query = $"SELECT pg_total_relation_size('messages');";
        long bytes = PopeAIDB.RawSqlQuery<List<long>>(query, x => new List<long> { Convert.ToInt64(x[0]) }).First().First();

        var content = "PopeAI's Database Info:";
        BotStat stat = StatManager.selfstat;
        content += $"\nMessage Table Size: {FormatManager.Format(bytes, FormatType.Bytes)}";
        content += $"\nMessages Stored: {FormatManager.Format(StatManager.selfstat.StoredMessages, FormatType.Commas)}";
        var secondpart = FormatManager.Format(bytes / StatManager.selfstat.StoredMessages, FormatType.Commas) + " bytes";
        content += $"\nAvg Message Size: {secondpart}";
        ctx.ReplyAsync(content);
    }

    [Command("database")]
    [Alias("db")]
    public static async Task DatabaseInfoAynsc(CommandContext ctx) 
    {
        if (ctx.Member.UserId != 12201879245422592) {
            return;
        }
        //string query = $"select (data_length + index_length) as Size, COUNT(Id), ((data_length + index_length)/COUNT(Id)) as avg_row_size from popeai.Messages, information_schema.tables where table_name = 'messages';";
        string query = $"SELECT pg_total_relation_size('messages');";
        long bytes = PopeAIDB.RawSqlQuery<List<long>>(query, x => new List<long> {Convert.ToInt64(x[0])}).First().First();

        var embed = new EmbedBuilder().AddPage().AddRow();
        embed.CurrentPage.Title = "PopeAI's Database Info";
        embed.CurrentPage.Footer = $"{DateTime.UtcNow.ToShortDateString()}";
        BotStat stat = StatManager.selfstat;
        embed.AddText("Message Table Size", FormatManager.Format(bytes, FormatType.Bytes)).AddRow();
        embed.AddText("Messages Stored", FormatManager.Format(StatManager.selfstat.StoredMessages, FormatType.Commas)).AddRow();
        embed.AddText("Avg Message Size", FormatManager.Format(bytes/StatManager.selfstat.StoredMessages, FormatType.Commas)+" bytes").AddRow();
        ctx.ReplyAsync(embed);
    }
}
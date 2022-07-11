namespace PopeAI.Commands.Dev;

public class Dev : CommandModuleBase
{
    [Command("userid")]
    [Summary("The user Id of the user who entered the command.")]
    public async Task UserId(CommandContext ctx)
    {
        ctx.ReplyAsync($"Your UserId is {ctx.Member.UserId}");
    }

    [Command("memberid")]
    [Summary("The Member Id of the user who entered the command.")]
    public async Task MemberId(CommandContext ctx)
    {
        ctx.ReplyAsync($"Your MemberId is {ctx.Member.Id}");
    }

    [Command("planetid")]
    [Summary("The Planet Id of the channel where the command was entered.")]
    public async Task PlanetId(CommandContext ctx)
    {
        ctx.ReplyAsync($"This planet's id is {ctx.Planet.Id}");
    }

    [Command("channelid")]
    [Summary("The Channel Id of the channel where the command was entered.")]
    public async Task ChannelId(CommandContext ctx)
    {
        ctx.ReplyAsync($"This channel's id is {ctx.Channel.Id}");
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

        EmbedBuilder embed = new EmbedBuilder();
        EmbedPageBuilder page = new EmbedPageBuilder();
        BotStat stat = StatManager.selfstat;
        page.AddText("Message Table Size", FormatManager.Format(bytes, FormatType.Bytes));
        page.AddText("Messages Stored", FormatManager.Format(StatManager.selfstat.StoredMessages, FormatType.Numbers));
        page.AddText("Avg Message Size", FormatManager.Format(bytes/StatManager.selfstat.StoredMessages, FormatType.Commas)+" bytes");
        embed.AddPage(page);
        ctx.ReplyAsync(embed);
    }
}
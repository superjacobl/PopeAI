namespace PopeAI.Commands.Dev;

public class Dev : CommandModuleBase
{
    [Command("userid")]
    [Summary("The user Id of the user who entered the command.")]
    public async Task UserId(CommandContext ctx)
    {
        await ctx.ReplyAsync($"Your UserId is {ctx.Member.UserId}");
    }

    [Command("memberid")]
    [Summary("The Member Id of the user who entered the command.")]
    public async Task MemberId(CommandContext ctx)
    {
        await ctx.ReplyAsync($"Your MemberId is {ctx.Member.Id}");
    }

    [Command("planetid")]
    [Summary("The Planet Id of the channel where the command was entered.")]
    public async Task PlanetId(CommandContext ctx)
    {
        await ctx.ReplyAsync($"This planet's id is {ctx.Planet.Id}");
    }

    [Command("channelid")]
    [Summary("The Channel Id of the channel where the command was entered.")]
    public async Task ChannelId(CommandContext ctx)
    {
        await ctx.ReplyAsync($"This channel's id is {ctx.Channel.Id}");
    }

    [Command("database")]
    [Alias("db")]
    public static async Task DatabaseInfoAynsc(CommandContext ctx) 
    {
        if (ctx.Member.UserId != 735182334984193) {
            return;
        }
        //string query = $"select (data_length + index_length) as Size, COUNT(Id), ((data_length + index_length)/COUNT(Id)) as avg_row_size from popeai.Messages, information_schema.tables where table_name = 'messages';";
        string query = $"select (data_length + index_length) as Size from information_schema.tables where table_name = 'messages';";
        List<ulong> data = PopeAIDB.RawSqlQuery<List<ulong>>(query, x => new List<ulong> {Convert.ToUInt64(x[0])}).First();

        EmbedBuilder embed = new EmbedBuilder();
        EmbedPageBuilder page = new EmbedPageBuilder();
        BotStat stat = StatManager.selfstat;
        page.AddText("Message Table Size", FormatManager.Format(data[0], FormatType.Bytes));
        page.AddText("Messages Stored", FormatManager.Format(stat.StoredMessages, FormatType.Numbers));
        page.AddText("Avg Message Size (bytes)", FormatManager.Format(data[0]/stat.StoredMessages, FormatType.Bytes));
        embed.AddPage(page);
        await ctx.ReplyAsync(embed);
    }
}
namespace PopeAI.Commands.Banking;

public class Info : CommandModuleBase
{
    Random rnd = new Random();
    [Group("info")]
    public class InfoGroup : CommandModuleBase
    {
        [Command("xp")]
        public Task XpInfo(CommandContext ctx)
        {
            var embed = new EmbedBuilder(EmbedItemPlacementType.RowBased).AddPage().AddRow()
                .AddText("Message Xp", "The more chars (numbers, letters, etc) you type in a given minute, the more xp you earn. However, each additional char adds a little less xp.").AddRow()
                .AddText("Element Xp", "By combining elements, you will earn xp depending on how difficult the combination was.");
            return ctx.ReplyAsync(embed);
        }

        [Command("elements")]
        public Task ElementsInfo(CommandContext ctx)
        {
            var embed = new EmbedBuilder(EmbedItemPlacementType.RowBased).AddPage().AddRow()
                .AddText("Elements", "By combining elements, you will earn xp depending on how difficult the combination was.");
            return ctx.ReplyAsync(embed);
        }
    }

    [Command("info")]
    public Task GetInfoAsync(CommandContext ctx)
    {
        return ctx.ReplyAsync("Available Commands: /info xp, /info elements");
    }
}
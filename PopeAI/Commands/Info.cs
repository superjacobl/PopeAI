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
            var embed = new EmbedBuilder().AddPage().AddRow()
                .AddText("Message Xp", "The more chars (numbers, letters, etc) you type in a given minute, the more xp you earn. However, each additional char adds a little less xp.").AddRow()
                .AddText("Element Xp", "By combining elements, you will earn xp depending on how difficult the combination was.");
            return ctx.ReplyAsync(embed);
        }

        [Command("elements")]
        public Task ElementsInfo(CommandContext ctx)
        {
            var embed = new EmbedBuilder().AddPage().AddRow()
                .AddText("Elements", "By combining elements, you will earn xp depending on how difficult the combination was.");
            return ctx.ReplyAsync(embed);
        }
    }

    [Command("info")]
    public Task GetInfoAsync(CommandContext ctx)
    {
        var embed = new EmbedBuilder().AddPage("Info about PopeAI")
            .AddRow()
                .AddText("Creator", "Superjacobl")
            .AddRow()
                .AddText("Version", "1.2.2")
            .AddRow()
                .AddText("Currently in", $"{ValourCache.GetAll<Planet>().Count()} Planets");
        return ctx.ReplyAsync(embed);
    }
}
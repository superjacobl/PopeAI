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
            EmbedBuilder embed = new();
            var page = new EmbedPageBuilder()
                .AddText("Message Xp", "The more chars (numbers, letters, etc) you type in a given minute, the more xp you earn. However, each additional char adds a little less xp.")
                .AddText("Element Xp", "By combining elements, you will earn xp depending on how difficult the combination was.");
            embed.AddPage(page);
            return ctx.ReplyAsync(embed);
        }

        [Command("elements")]
        public Task ElementsInfo(CommandContext ctx)
        {
            EmbedBuilder embed = new();
            EmbedPageBuilder page = new EmbedPageBuilder()
                .AddText("Elements", "By combining elements, you will earn xp depending on how difficult the combination was.");

            embed.AddPage(page);
            return ctx.ReplyAsync(embed);
        }
    }

    [Command("info")]
    public Task GetInfoAsync(CommandContext ctx)
    {
        return ctx.ReplyAsync("Available Commands: /info xp, /info elements");
    }
}
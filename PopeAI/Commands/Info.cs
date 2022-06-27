/*

 */

namespace PopeAI.Commands.Banking
{
    public class Info : CommandModuleBase
    {
        Random rnd = new Random();
        [Group("info")]
        public class Bank : CommandModuleBase
        {

            [Command("xp")]
            public async Task xp(CommandContext ctx)
            {
                EmbedBuilder embed = new EmbedBuilder();
                EmbedPageBuilder page = new EmbedPageBuilder();
                
                page.AddText("Messages", "For every minute that you chat, you earn 1xp. The longer you keep chatting for, the more xp you earn per message! For example, after 10 minutes, you will earn 4.4xp per minute. The streak goes away if you do not send a message in 5 minutes.");
                page.AddText("Elements", "By combining elements, you will earn xp depending on how difficult the combination was.");

                embed.AddPage(page);
                await ctx.ReplyAsync(embed);
            }
            [Command("elements")]
            public async Task elements(CommandContext ctx)
            {
                EmbedBuilder embed = new EmbedBuilder();
                EmbedPageBuilder page = new EmbedPageBuilder();
                
                page.AddText("Elements", "By combining elements, you will earn xp depending on how difficult the combination was.");

                embed.AddPage(page);
                await ctx.ReplyAsync(embed);
            }
        }

        [Command("info")]
        public async Task infoasync(CommandContext ctx)
        {
            await ctx.ReplyAsync("Available Commands: /info xp, /info elements");
        }


    }
}
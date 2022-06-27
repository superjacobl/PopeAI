using System.Diagnostics;

/*
 * testgraph
 */

namespace PopeAI.Commands.Tests
{
    public class Tests : CommandModuleBase
    {
        // ~say hello world -> hello world
        [Command("say")]
        [Summary("Echoes a message.")]
        public async Task SayAsync(CommandContext ctx)
        {
            await ctx.ReplyAsync("Command Handling kinda works!");
        }

        [Command("embed")]
        public async Task EmbedAsync(CommandContext ctx)
        {
            EmbedBuilder embed = new EmbedBuilder();
            EmbedPageBuilder page = new EmbedPageBuilder();
            page.AddText("Update complete", "Your new stats are below!");
            page.AddText(null, "`Total`");
            page.AddText("\ud83d\udcbe Coinz", "\ud83d\udd3a Coinz:\n4 \u0e3f", true);
            page.AddText("\ud83c\udfe6 Balance", "\ud83d\udd3a Balance:\n8,286,460,501,158,150 THR", true);
            page.AddText("\ud83d\udc6a Population", "\ud83d\udd3a Population:\n1,225,108,632,771", true);
            page.AddText(null, "", false);
            page.AddInputBox(id: "houses", inline: false, name: "Buy Houses", size: EmbedItemSize.Short);
            page.AddText(null, "", false);
            page.AddInputBox(id: "land", inline: false, name: "Buy Land", size: EmbedItemSize.Short);
            page.AddText(null, "", false);
            page.AddInputBox(id: "eggs", inline: false, name: "Buy Eggs", size: EmbedItemSize.Short);
            page.AddText(null, "", false);
            page.AddInputBox(id: "factories", inline: false, name: "Buy Factories", size: EmbedItemSize.Short);
            page.AddButton("Test Button", "Click Me");
            embed.AddPage(page);
            page = new EmbedPageBuilder();
            page.AddText("Update complete", "Your new stats are below!");
            page.AddText(null, "`Total`");
            page.AddText("\ud83d\udcbe Coinz", "\ud83d\udd3a Coinz:\n4 \u0e3f", true);
            page.AddText("\ud83c\udfe6 Balance", "\ud83d\udd3a Balance:\n8,286,460,501,158,150 THR", true);
            page.AddText("\ud83d\udc6a Population", "\ud83d\udd3a Population:\n1,225,108,632,771", true);
            page.AddButton("Test Button", "Click Me");
            embed.AddPage(page);
            await ctx.ReplyAsync(embed);
        }

        [Command("e")]
        public async Task EAsync(CommandContext ctx)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.AddText("Update complete", "Your new stats are below!");
            embed.AddText(null, "`Total`");
            embed.AddText("\ud83d\udcbe Coinz", "\ud83d\udd3a Coinz:\n4 \u0e3f", true);
            embed.AddText("\ud83c\udfe6 Balance", "\ud83d\udd3a Balance:\n8,286,460,501,158,150 THR", true);
            embed.AddText("\ud83d\udc6a Population", "\ud83d\udd3a Population:\n1,225,108,632,771", true);
            embed.AddText(null, "", false);
            embed.AddInputBox(id: "houses", inline: false, name: "Buy Houses", size: EmbedItemSize.Short);
            embed.AddText(null, "", false);
            embed.AddInputBox(id: "land", inline: false, name: "Buy Land", size: EmbedItemSize.Short);
            embed.AddText(null, "", false);
            embed.AddInputBox(id: "eggs", inline: false, name: "Buy Eggs", size: EmbedItemSize.Short);
            embed.AddText(null, "", false);
            embed.AddInputBox(id: "factories", inline: false, name: "Buy Factories", size: EmbedItemSize.Short);
            embed.AddButton("Test Button", "Click Me");
            await ctx.ReplyAsync(embed);
        }

        [Command("mention")]
        [Summary("")]
        public async Task MetionAsync(CommandContext ctx)
        {
          //  string s = "";
          //  for (int i = 0; i < 1; i++)
         //   {
          //      s += ctx.Member.Mention;
         //   }
            // await ctx.ReplyAsync($"Test {ctx.Member}");
        }

        [Command("fastcount")]
        public async Task FastCountAsync(CommandContext ctx, int times, int delay, int makebigger)
        {
            if (!(ctx.Member.User_Id == ctx.Planet.Owner_Id)) {
                return;
            }
            if (times > 10000) {
                times = 10000;
            }
            await Task.Delay(delay);
            Stopwatch sw = new();
            sw.Start();
            for (int i = 0; i < times; i++)
            {
                string str = i.ToString();
                for (int j = 0; j < makebigger;j++) {
                    str += " "+i.ToString();
                }
                ctx.ReplyAsync(str);
            }
            sw.Stop();
            Console.WriteLine($"Time taken: {sw.ElapsedMilliseconds}ms\nPer Message: {Math.Round((double)sw.ElapsedMilliseconds/times, 2)}ms");
        }

        [Command("count")]
        public async Task CountAsync(CommandContext ctx, int times, int delay)
        {
            if (!(ctx.Member.User_Id == ctx.Planet.Owner_Id)) {
                return;
            }
            if (times > 1000) {
                times = 1000;
            }
            for (int i = 0; i < times; i++)
            {
                ctx.ReplyAsync(i.ToString());
                await Task.Delay(delay);
            }
        }

        [Command("say")]
        [Alias("echo")]
        //[Summary("Echoes a message.")]
        public async Task EchoAsync(CommandContext ctx, int times, [Remainder] string echo)
        {
            if (!(ctx.Member.User_Id == ctx.Planet.Owner_Id)) {
                return;
            }
            if (times > 1000) {
                times = 1000;
            }
            for (int i = 0; i < times; i++)
            {
                ctx.ReplyAsync(echo);
            }
        }


        [Command("say")]
        [Alias("echo")]
        //[Summary("Echoes a message.")]
        public async Task EchoAsync(CommandContext ctx, [Remainder] string echo)
        {
            await ctx.ReplyAsync(echo);
        }

        [Command("testcommand")]
        [OnlyRole("Egg")]
        public async Task TestAsync(CommandContext ctx)
        {
            await ctx.ReplyAsync("Your can use this command, because you have the Egg role");
        }

        [Command("double")]
        //[Summary("Echoes a message.")]
        public async Task DoubleAsync(CommandContext ctx, double num)
        {
            await ctx.ReplyAsync($"{num * 2}");
        }

        [Event("User Lacks the Role To Use This Command")]
        public async Task UserCantUseCommandAsync(CommandContext ctx, string commandname)
        {
            await ctx.ReplyAsync($"You can't use this command!");
        }

        [Group("othertest")]
        public class TestModule : CommandModuleBase
        {

        }
    }
}
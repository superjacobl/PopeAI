using System.Text.Json;
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
        public Task SayAsync(CommandContext ctx)
        {
            return ctx.ReplyAsync("Command Handling kinda works!");
        }

        [Command("embed")]
        public Task EmbedAsync(CommandContext ctx)
        {
            var embed = new EmbedBuilder().AddPage().AddRow();
            embed.CurrentPage.Title = "Update complete, Your new stats are below!";
            embed.AddText(null, "`Total`").AddRow();
            embed.AddText("\ud83d\udcbe Coinz", "\ud83d\udd3a Coinz:\n4 \u0e3f");
            embed.AddText("\ud83c\udfe6 Balance", "\ud83d\udd3a Balance:\n8,286,460,501,158,150 THR");
            embed.AddText("\ud83d\udc6a Population", "\ud83d\udd3a Population:\n1,225,108,632,771").AddRow();
            //embed.AddInputBox(id: "houses", name: "Buy Houses", size: EmbedItemSize.Small);
            //embed.AddInputBox(id: "land", name: "Buy Land", size: EmbedItemSize.Small);
            //embed.AddInputBox(id: "eggs", name: "Buy Eggs", size: EmbedItemSize.Small);
            //embed.AddInputBox(id: "factories", name: "Buy Factories", size: EmbedItemSize.Small);
            //embed.AddButton("Test Button", "Click Me");
            //embed.AddButton("Test Button", "Click Me");
            return ctx.ReplyAsync(embed);
        }

        [Command("mention")]
        [Summary("")]
        public Task MetionAsync(CommandContext ctx)
        {
            string s = "";
            for (int i = 0; i < 1; i++)
            {
               s += $"«@m-{ctx.Member.Id}»";
            }
            return ctx.ReplyAsync($"Test {s}");
        }

        [Command("fastcount")]
        public async Task FastCountAsync(CommandContext ctx, int times, int delay, int makebigger)
        {
            if (ctx.Member.UserId != 12201879245422592) {
                return;
            }
            if (times > 10000) {
                times = 10000;
            }
            await Task.Delay(delay);
            Stopwatch sw = new();
            sw.Start();
            List<Task> tasks = new();
            for (int i = 0; i < times; i++)
            {
                string str = i.ToString();
                for (int j = 0; j < makebigger;j++) {
                    str += " "+i.ToString();
                }
                tasks.Add(ctx.ReplyAsync(str));
            }
            Task.WaitAll(tasks.ToArray());
            sw.Stop();
            ctx.ReplyAsync($"Time taken: {sw.ElapsedMilliseconds}ms\nPer Message: {Math.Round((double)sw.ElapsedMilliseconds/times, 2)}ms");
        }

        [Command("count")]
        public async Task CountAsync(CommandContext ctx, int times, int delay)
        {
            if (ctx.Member.UserId != 12201879245422592) {
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
        public async Task EchoAsync(CommandContext ctx, int times, int delay, [Remainder] string echo)
        {
            if (ctx.Member.UserId != 12201879245422592) {
                return;
            }
            if (times > 1000) {
                times = 1000;
            }

            Task.Run(async () => {
                for (int i = 0; i < times; i++)
                {
                    ctx.ReplyAsync(echo);
                    await Task.Delay(delay);
                }
            });
        }

        [Command("say")]
        [Alias("echo")]
        //[Summary("Echoes a message.")]
        public async Task EchoAsync(CommandContext ctx, int times, [Remainder] string echo)
        {
            if (ctx.Member.UserId != 12201879245422592) {
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
            if (ctx.Member.UserId != 12201879245422592) {
                return;
            }
            ctx.ReplyAsync(echo);
        }

        [Command("testcommand")]
        [OnlyRole("Egg")]
        public Task TestAsync(CommandContext ctx)
        {
            return ctx.ReplyAsync("Your can use this command, because you have the Egg role");
        }

        [Command("double")]
        //[Summary("Echoes a message.")]
        public Task DoubleAsync(CommandContext ctx, double num)
        {
            return ctx.ReplyAsync($"{num * 2}");
        }

        //[Event(EventType.)]
        public Task UserCantUseCommandAsync(CommandContext ctx, string commandname)
        {
            return ctx.ReplyAsync($"You can't use this command!");
        }

        [Command("defaulttest")]
        public Task defaultTest(CommandContext ctx, [SwitchInput("test")] bool Test = false) 
        {
            if (Test) {
                return ctx.ReplyAsync("True!");
            }
            else {
                return ctx.ReplyAsync("False!");
            }
        }

        [Group("embed")]
        public class TestModule : CommandModuleBase
        {
            [Command("list")]
            public Task EmbedListTest(CommandContext ctx)
            {
                var embed = new EmbedBuilder().AddPage().AddRow();
                embed.AddText("Test", "* 1\n* 2\n* 3");
                return ctx.ReplyAsync(embed); 
            }

            [Command("free")]
            public Task EmbedListTestfree(CommandContext ctx)
            {
                var embed = new EmbedBuilder().AddPage(embedType:EmbedItemPlacementType.FreelyBased, width:400, height:200);
                embed.AddText("Test", "420", x: 200-14, y: 100-23);
                //embed.AddText("Test", "* 1\n* 2\n* 3");
                return ctx.ReplyAsync(embed); 
            }

            [Command("goto2")]
            public Task EmbedGoToTest2(CommandContext ctx)
            {
                var Embed = new EmbedBuilder()
                    .AddPage("Home")
                        .AddRow()
                            .AddGoToPage(1)
                                .AddButton(text:"Go To Page 2")
                                .AddText(text:"Click me!")
                            .EndGoTo()
                            .AddGoToPage(2)
                                .AddButton(text:"Go To Page 3")
                            .EndGoTo()
                    .AddPage("2nd Page")
                        .AddRow()
                            .AddGoToPage(0)
                                .AddButton(text:"Go Back")
                            .EndGoTo()
                            .AddText(text:"g32323ggr")
                    .AddPage("3rd Page")
                        .AddRow()
                            .AddGoToPage(0)
                                .AddButton(text:"Go Back")
                            .EndGoTo()
                            .AddText(text:"32323232");
                return ctx.ReplyAsync(Embed);
            }

            [Command("goto")]
            public Task EmbedGoToTest(CommandContext ctx)
            {
                var Embed = new EmbedBuilder()
                    .AddPage("Home")
                        .AddRow()
                            .AddGoToPage(1)
                                .AddButton(text:"Go To Page 2")
                            .EndGoTo()
                            .AddGoToPage(2)
                                .AddButton(text:"Go To Page 3")
                            .EndGoTo()
                    .AddPage("2nd Page")
                        .AddRow()
                            .AddGoToPage(0)
                                .AddButton(text:"Go Back")
                            .EndGoTo()
                            .AddText(text:"g32323ggr")
                    .AddPage("3rd Page")
                        .AddRow()
                            .AddGoToPage(0)
                                .AddButton(text:"Go Back")
                            .EndGoTo()
                            .AddText(text:"32323232");
                return ctx.ReplyAsync(Embed);
            }

            [Command("input")]
            public Task EmbedInputTest(CommandContext ctx)
            {
                var Embed = new EmbedBuilder()
                    .AddPage()
                        .AddRow()
                            .AddForm(EmbedItemPlacementType.RowBased, "testinput")
                                .AddRow()
                                    .AddInputBox("Username", "Username", "username")
                                .AddRow()
                                    .AddButton("submit", "Submit", isSubmitButton: true)
                            .EndForm();
                return ctx.ReplyAsync(Embed);
            }

          // [Interaction(EmbedIteractionEventType.FormSubmitted, "testinput")]
//            public Task FormTest(InteractionContext ctx) 
         //   {
                //var str = JsonSerializer.Serialize(ctx.Event.FormData, options: new JsonSerializerOptions() {WriteIndented = true});
               // Console.WriteLine(str);
                //return ctx.ReplyAsync("You inputted: "+ctx.Event.FormData.FirstOrDefault(x => x.ElementId == "Username").Value);
           // }
        }
    }
}
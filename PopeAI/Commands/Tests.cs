using System.Text.Json;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Valour.Net.EmbedMenu;
using System.Text.Json.Serialization;
using Valour.Net.Client.MessageHelper;


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

        [Command("map")]
        public Task MapTestModule(CommandContext ctx)
        {
            // 7 by 7
            string map = """
0,0,1,1,1,1,0;
0,0,1,2,2,1,0;
0,0,1,2,1,0,0;
0,0,1,2,1,1,0;
0,0,1,2,2,2,1;
0,0,1,2,2,2,1;
0,0,1,1,1,1,1
""";
            List<List<int>> MapData = new();
            
            foreach(var row in map.Split(";"))
            {
                var r = new List<int>();
                foreach (var col in row.Split(","))
                    r.Add(int.Parse(col));
                MapData.Add(r);
            }

            var embed = new EmbedBuilder()
                .AddPage()
				    .WithStyles(
					    new Width(new Size(Unit.Pixels, 360)),
					    new Height(new Size(Unit.Pixels, 190))
				    )
				    .AddRow()
					    .WithStyles(
						    FlexDirection.Column,
						    new Width(new Size(Unit.Pixels, 325)),
						    new Height(new Size(Unit.Pixels, 190))
					    );



			foreach (var row in MapData)
			{
                embed.WithRow()
						.WithStyles(
							FlexDirection.Row,
							new Width(new Size(Unit.Pixels, 325)),
							new Height(new Size(Unit.Pixels, 24))
						);
				foreach (var cell in row)
                {
                    Color c = cell switch
                    {
                        0 => new(61, 102, 204),
                        1 => new(194, 178, 128),
                        2 => new(76, 143, 59)
                    };
                    embed.WithRow()
                        .WithStyles(
                            new Width(new Size(Unit.Pixels, 24)),
                            new Height(new Size(Unit.Pixels, 24)),
                            new BackgroundColor(c),
                            new Margin(right: new Size(Unit.Pixels, 0))
                        )
                        .CloseRow();
				}
                embed.CloseRow();
            }
            return ctx.ReplyAsync(embed);
        }

	    [Group("embed")]
        public class TestModule : CommandModuleBase
        {
            [Command("media")]
            public async Task EmbedMediaTest(CommandContext ctx) 
            {
                var url = "https://cdn.valour.gg/content/Profile/12201879245422592/4a0419b8f298d6da533e791c6697eae82ea98081b2fba1d12841ef4603140c7c.png";
                // "comment" out because not gonna work till images working in embeds get's pushed to live
                var notgonnaupdatetillvalourupdate = """
                var embed = new EmbedBuilder().AddPage()
                    .AddRow()
                        .AddMedia(0, 0, "image/png", "test.png", url)
                            .WithStyles(new Width(new Size(Unit.Pixels, 32)), new Height(new Size(Unit.Pixels, 32)), new Margin(null, new Size(Unit.Pixels, 0), null, null))
                        .AddMedia(0, 0, "image/png", "test.png", url)
                            .WithStyles(new Width(new Size(Unit.Pixels, 32)), new Height(new Size(Unit.Pixels, 32)), new Margin(null, new Size(Unit.Pixels, 0), null, null))
                        .AddMedia(0, 0, "image/png", "test.png", url)
                            .WithStyles(new Width(new Size(Unit.Pixels, 32)), new Height(new Size(Unit.Pixels, 32)), new Margin(null, new Size(Unit.Pixels, 0), null, null))
                    .AddRow()
                        .AddMedia(0, 0, "image/png", "test.png", url)
                            .WithStyles(new Width(new Size(Unit.Pixels, 32)), new Height(new Size(Unit.Pixels, 32)), new Margin(null, new Size(Unit.Pixels, 0), null, null))
                        .AddMedia(0, 0, "image/png", "test.png", "https://cdn.valour.gg/content/Profile/12447386915569664/8acc088af65693faafdd57b8fd86ee06f8c2b74df690930351f5acdedddf26f6.png")
                            .WithStyles(new Width(new Size(Unit.Pixels, 32)), new Height(new Size(Unit.Pixels, 32)), new Margin(null, new Size(Unit.Pixels, 0), null, null))
                        .AddMedia(0, 0, "image/png", "test.png", url)
                            .WithStyles(new Width(new Size(Unit.Pixels, 32)), new Height(new Size(Unit.Pixels, 32)), new Margin(null, new Size(Unit.Pixels, 0), null, null))
                    .AddRow()
                        .AddMedia(0, 0, "image/png", "test.png", url)
                            .WithStyles(new Width(new Size(Unit.Pixels, 32)), new Height(new Size(Unit.Pixels, 32)), new Margin(null, new Size(Unit.Pixels, 0), null, null))
                        .AddMedia(0, 0, "image/png", "test.png", url)
                            .WithStyles(new Width(new Size(Unit.Pixels, 32)), new Height(new Size(Unit.Pixels, 32)), new Margin(null, new Size(Unit.Pixels, 0), null, null))
                        .AddMedia(0, 0, "image/png", "test.png", url)
                            .WithStyles(new Width(new Size(Unit.Pixels, 32)), new Height(new Size(Unit.Pixels, 32)), new Margin(null, new Size(Unit.Pixels, 0), null, null));
               
                return ctx.ReplyAsync(embed); 
                """;
            }

            [Command("list")]
            public Task EmbedListTest(CommandContext ctx)
            {
                var embed = new EmbedBuilder().AddPage().AddRow();
                embed.AddText("Test", "* 1\n* 2\n* 3");
                return ctx.ReplyAsync(embed); 
            }

            [Command("newsystem")]
            public Task NewSystemTest(CommandContext ctx)
            {
                var Embed = new EmbedBuilder()
                    .AddPage("Home")
                        .AddButton("Click me!")
                            .WithStyles(new BackgroundColor(new Color(100, 0, 0)))
                            .OnCLickGoToLink("https://youtube.com")
                        .AddText("That button when you click it will take you to youtube")
                            .WithStyles(new TextColor(new Color(255, 255, 255, 0.75f)))
                            .WithName("What does that button do?")
                                .WithStyles(new TextColor(new Color(255, 0, 0)), FontWeight.Bold);
                
                return ctx.ReplyAsync(Embed);
            }

            [Command("goto")]
            public Task EmbedGoToTest(CommandContext ctx)
            {
                var Embed = new EmbedBuilder()
                    .AddPage("Home")
                        .AddRow()
                            .AddButton(text:"Go To Page 2")
                                .OnClickGoToEmbedPage(1)
                            .AddButton(text:"Go To Page 3")
                                .OnClickGoToEmbedPage(2)
                    .AddPage("2nd Page")
                        .AddRow()
                            .AddButton(text:"Go Back")
                                .OnClickGoToEmbedPage(0)
                            .AddText(text:"g32323ggr")
                    .AddPage("3rd Page")
                        .AddRow()
                            .AddButton(text:"Go Back")
                                .OnClickGoToEmbedPage(0)
                            .AddText(text:"32323232");
                return ctx.ReplyAsync(Embed);
            }

            [Command("time")]
            public Task EmbedTimeEdittingMessageTest(CommandContext ctx)
            {
                var embed = new EmbedBuilder()
                    .AddPage()
                        .AddRow()
                            .AddText("Time", DateTime.UtcNow.ToString())
                        .AddRow()
                            .AddButton("Update")
                                .OnClick(UpdatedEmbedTimeTest);
				return ctx.ReplyAsync(embed);
			}

            [EmbedMenuFunc]
			public static async ValueTask UpdatedEmbedTimeTest(InteractionContext ctx)
            {
				var embed = new EmbedBuilder()
					.AddPage()
						.AddRow()
							.AddText("Time", DateTime.UtcNow.ToString())
						.AddRow()
							.AddButton("Update")
								.OnClick(UpdatedEmbedTimeTest);
                var msg = await PlanetMessage.FindAsync(ctx.Event.MessageId, ctx.Channel.Id, ctx.Planet.Id);
				JsonSerializerOptions options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };
				msg.EmbedData = JsonSerializer.Serialize(embed.embed, options);
                msg.embedParsed = true;
				var result = await msg.EditMessageAsync();
                Console.WriteLine(result.Message);
                Console.WriteLine(result.Success);


				//MessageHelpers.GenerateForPost(message);
			}

			[Command("input")]
            public Task EmbedInputTest(CommandContext ctx)
            {
                var Embed = new EmbedBuilder()
                    .AddPage()
                        .AddRow()
                            .AddForm("testinput")
                                .AddRow()
                                    .AddInputBox("Username", "Username", "username")
                                .AddRow()
                                    .AddButton("Submit")
                                        .OnClickSubmitForm("Submit")
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
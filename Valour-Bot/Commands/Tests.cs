using System.Threading.Tasks;
using Valour.Net;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;

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

        [Command("say")]
        [Alias("echo")]
        //[Summary("Echoes a message.")]
        public async Task EchoAsync(CommandContext ctx, [Remainder] string echo)
        {
            await ctx.ReplyAsync(echo);
        }

        [Command("testcommand")]
        [OnlyRole("Egg")]
        public async Task TestAsync(CommandContext ctx, string commandname)
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
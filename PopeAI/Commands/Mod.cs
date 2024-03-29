/*
stats
roll
*/

namespace PopeAI.Commands.Mod
{
    public class Mod : CommandModuleBase
    {
        [Command("purge")]
        public async Task PurgeAsync(CommandContext ctx, int messages)
        {
            if (ctx.Member.UserId != ctx.Planet.OwnerId && ctx.Member.UserId != 12201879245422592) {
                return;
            }
            //if (!(await ctx.Channel.HasPermissionAsync(ctx.Member.Id, ChatChannelPermissions.ManageMessages))) {
            //    return;
            //}
            if (messages > 64) {
                messages = 64;
            }
            List<Message> ChannelMessages = await ctx.Channel.GetLastMessagesAsync(count: messages);
            ChannelMessages.Reverse();
            ctx.ReplyAsync($"Purging {messages} messages from this channel!");
            int i = 0;
            foreach(Message message in ChannelMessages) {
                if ((await message.GetAuthorUserAsync()).Id == ctx.Planet.OwnerId && false) {
                    continue;
                }
                Console.WriteLine(message.Content);
                if (!(await message.DeleteAsync()).Success) {
                    Console.WriteLine(await JsonContent.Create(message).ReadAsStringAsync());
                }
                i += 1;
                if (i >= messages) {
                    return;
                }
            }
        }

        [Group("fliterword")]
        public class FliterWordGroup : CommandModuleBase
        {
        }
    }
}
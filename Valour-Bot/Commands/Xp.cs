using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PopeAI.Database;
using Microsoft.EntityFrameworkCore;
using Valour.Net.Models;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;

namespace PopeAI.Commands.Xp
{
    public class Xp : CommandModuleBase
    {
        PopeAIDB DBContext = new PopeAIDB(PopeAIDB.DBOptions);

        [Command("xp")]
        [Summary("Gives the user who sent the command their xp.")]
        public async Task SendXp(CommandContext ctx)
        {
            User DBUser = await DBContext.Users.FirstOrDefaultAsync(x => x.UserId == ctx.Message.Author_Id && x.PlanetId == ctx.Message.Planet_Id);
            await ctx.ReplyAsync($"{ctx.Member.Nickname}'s xp: {(ulong)DBUser.Xp}");
        }

        [Command("leaderboard")]
        [Summary("Returns the leaderboard of the users with the most xp.")]
        public async Task Leaderboard(CommandContext ctx)
        {
            List<User> users = await Task.Run(() => DBContext.Users.Where(x => x.PlanetId == ctx.Message.Planet_Id).OrderByDescending(x => x.Xp).Take(10).ToList());
            string content = "| nickname | xp |\n| :- | :-\n";
            foreach (User USER in users)
            {
                PlanetMember member = await ctx.Planet.GetMember(USER.UserId, USER.PlanetId);
                content += $"{member.Nickname} | {(ulong)USER.Xp} xp\n";
            }
            await ctx.ReplyAsync(content);
        }
    }
}
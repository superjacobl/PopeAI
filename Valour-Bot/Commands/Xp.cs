using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PopeAI.Database;
using Microsoft.EntityFrameworkCore;
using Valour.Net.Models;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using System;

namespace PopeAI.Commands.Xp
{    
    public class Xp : CommandModuleBase
    {

        public static Dictionary<ulong, DateTime> timesincelastMessage = new Dictionary<ulong, DateTime>();
        public Random rnd = new Random();

        static Dictionary<ulong, ulong> MessagesPerMinuteInARow = new Dictionary<ulong, ulong>();

        [Event("Message")]
        public async Task OnMessage(CommandContext ctx)
        {
            bool IsVaild = false;

            if (timesincelastMessage.ContainsKey(ctx.Member.Id)) {
                if (timesincelastMessage[ctx.Member.Id].AddSeconds(60) < DateTime.UtcNow) {
                    if (timesincelastMessage[ctx.Member.Id].AddMinutes(5) < DateTime.UtcNow) {
                        MessagesPerMinuteInARow[ctx.Member.Id] = 0;
                    }
                    else {
                        if (MessagesPerMinuteInARow[ctx.Member.Id] < 60) {
                            MessagesPerMinuteInARow[ctx.Member.Id] += 1;
                        }
                    }
                    IsVaild = true;
                    timesincelastMessage[ctx.Member.Id] = DateTime.UtcNow;
                }
            }
            else {
                IsVaild = true;
                timesincelastMessage.Add(ctx.Member.Id, DateTime.UtcNow);
                MessagesPerMinuteInARow.Add(ctx.Member.Id, 1);
            }

            await Client.DBContext.AddStat("Message", 1, ctx.Message.Planet_Id, Client.DBContext);

            if (ctx.Message.Author_Id == ulong.Parse(Client.Config.BotId)) {
                return;
            }

            User user = await Client.DBContext.Users.FirstOrDefaultAsync(x => x.UserId == ctx.Message.Author_Id && x.PlanetId == ctx.Message.Planet_Id);

            ulong xp = 1 + MessagesPerMinuteInARow[ctx.Member.Id];

            await Client.DBContext.AddStat("UserMessage", 1, ctx.Message.Planet_Id, Client.DBContext);

            if (user == null) {
                user = new User();
                ulong num = (ulong)rnd.Next(1,int.MaxValue);
                while (await Client.DBContext.Users.FirstOrDefaultAsync(x => x.Id == num) != null) {
                    num = (ulong)rnd.Next(1,int.MaxValue);
                }
                user.Id = num;
                user.PlanetId = ctx.Message.Planet_Id;
                user.UserId = ctx.Message.Author_Id;
                user.Coins += xp*2;
                user.Xp += xp;
                await Client.DBContext.AddStat("Coins", (double)xp*2, ctx.Message.Planet_Id, Client.DBContext);
                await Client.DBContext.Users.AddAsync(user);
                await Client.DBContext.SaveChangesAsync();
            }

            if (IsVaild) {
                user.Xp += xp;
                user.Coins += xp*2;
                await Client.DBContext.AddStat("Coins", (double)xp*2, ctx.Message.Planet_Id, Client.DBContext);
                await Client.DBContext.SaveChangesAsync();
            }
        }

        [Command("xp")]
        [Summary("Gives the user who sent the command their xp.")]
        public async Task SendXp(CommandContext ctx)
        {
            User DBUser = await Client.DBContext.Users.FirstOrDefaultAsync(x => x.UserId == ctx.Message.Author_Id && x.PlanetId == ctx.Message.Planet_Id);
            await ctx.ReplyAsync($"{ctx.Member.Nickname}'s xp: {(ulong)DBUser.Xp}");
        }

        [Command("leaderboard")]
        [Summary("Returns the leaderboard of the users with the most xp.")]
        public async Task Leaderboard(CommandContext ctx)
        {
            List<User> users = await Task.Run(() => Client.DBContext.Users.Where(x => x.PlanetId == ctx.Message.Planet_Id).OrderByDescending(x => x.Xp).Take(10).ToList());
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
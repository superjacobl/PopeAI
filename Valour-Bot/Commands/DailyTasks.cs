using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PopeAI.Database;
using Microsoft.EntityFrameworkCore;
using Valour.Net.Models;
using PopeAI.Models;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using System;

namespace PopeAI.Commands.DailyTasks
{    
    public class DailyTasks : CommandModuleBase
    {
        public Random rnd = new Random();

        [Event("Message")]
        public async Task OnMessage(CommandContext ctx)
        {
            // check if the member has a daily task 
            DailyTask task = await Client.DBContext.DailyTasks.FirstOrDefaultAsync(x => x.MemberId == ctx.Member.Id && x.TaskType == "Messages");
            if (task != null) {
                if (task.Done < task.Goal) {
                    task.Done += 1;
                    if (task.Done == task.Goal) {
                        User DBUser = await Client.DBContext.Users.FirstOrDefaultAsync(x => x.PlanetId == ctx.Planet.Id && x.UserId == ctx.Message.Author_Id);
                        DBUser.Coins += task.Reward;
                        await Client.DBContext.AddStat("Coins", task.Reward, ctx.Message.Planet_Id, Client.DBContext);
                        await ctx.ReplyAsync($"Your {task.TaskType} daily task is done! You get {task.Reward} coins.");
                    }
                    await Client.DBContext.SaveChangesAsync();
                }
            }
        }

        [Command("dailytasks")]
        public async Task dailytasks(CommandContext ctx)
        {
            string content = "";
            foreach(DailyTask task in Client.DBContext.DailyTasks.Where(x => x.MemberId == ctx.Member.Id)) {
                content += $"\n[^{task.Done}^/~{task.Goal}~] -> {task.TaskType} today ({task.Reward} coins)";
            }
            await ctx.ReplyAsync(content);
        }
    }
}
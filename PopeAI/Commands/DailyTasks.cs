using Database.Models.Users;

namespace PopeAI.Commands.DailyTasks
{
    public class DailyTasks : CommandModuleBase
    {
        public Random rnd = new Random();

        [Event(EventType.Message)]
        public async Task OnMessage(CommandContext ctx)
        {
            await Client.DBContext.DidTask(Client.DBContext, DailyTaskType.Messages, ctx.Member.Id, ctx);
        }

        [Command("dailytasks")]
        [Alias("tasks")]
        public async Task dailytasks(CommandContext ctx)
        {
            string content = "";
            foreach(DailyTask task in Client.DBContext.DailyTasks.Where(x => x.MemberId == ctx.Member.Id)) {
                content += $"\n[^{task.Done}^/~{task.Goal}~] -> {task.TaskType.ToString().Replace("_", " ")} today ({task.Reward} coins)";
            }
            await ctx.ReplyAsync(content);
        }
    }
}
namespace PopeAI.Commands.DailyTasks;

public class DailyTasks : CommandModuleBase
{
    public Random rnd = new Random();

    [Command("dailytasks")]
    [Alias("tasks")]
    public async Task ViewDailyTasks(CommandContext ctx)
    {
        var user = DBUser.GetAsync(ctx.Member.Id, true);
        string content = "";
        foreach(var task in DBCache.GetAll<DailyTask>().Where(x => x.MemberId == ctx.Member.Id)) {
            content += $"\n[^{task.Done}^/~{task.Goal}~] -> {task.TaskType.ToString().Replace("_", " ")} today ({task.Reward} coins)";
        }
        await ctx.ReplyAsync(content);
    }
}
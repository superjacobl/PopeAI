namespace PopeAI.Bot.Managers;

public static class DailyTaskManager
{
    public static Random rnd = new();
    public static IdManager idManager = new();

    public static async Task DidTask(DailyTaskType TaskType, ulong MemberId, CommandContext ctx = null)
    {
        var user = await DBUser.GetAsync(MemberId);
        DailyTask task = user.DailyTasks.FirstOrDefault(x => x.TaskType == TaskType);
        if (task != null)
        {
            if (task.Done < task.Goal)
            {
                task.Done += 1;
                if (task.Done == task.Goal)
                {
                    user.Coins += task.Reward;
                    StatManager.AddStat(CurrentStatType.Coins, task.Reward, user.PlanetId);
                    if (ctx != null)
                    {
                        string content = $"{ctx.Member.Nickname}, your {task.TaskType.ToString().Replace("_", " ")} daily task is done! You get {task.Reward} coins.";
                        await ctx.ReplyWithMessagesAsync(4000, new List<string>() { content });
                    }
                }
            }
        }
        await user.UpdateDB();
    }
    public static DailyTaskType RandomTask()
    {
        return rnd.Next(0, 5) switch
        {
            0 => DailyTaskType.Dice_Games_Played,
            1 => DailyTaskType.Hourly_Claims,
            2 => DailyTaskType.Gamble_Games_Played,
            3 => DailyTaskType.Messages,
            4 => DailyTaskType.Combined_Elements,
            _ => 0
        };
    }

    public static int Choice(int[] list)
    {
        return list[rnd.Next(0, list.Length)];
    }

    public static IEnumerable<DailyTask> GenerateNewDailyTasks(ulong Memberid)
    {
        List<DailyTask> toadd = new();

        for (int i = 0; i < 3; i++)
        {
            DailyTaskType tasktype = RandomTask();
            while (toadd.Any(x => x.TaskType == tasktype))
            {
                tasktype = RandomTask();
            }

            DailyTask task = new()
            {
                TaskType = tasktype,
                Id = idManager.Generate(),
                LastDayUpdated = DateTime.UtcNow,
                MemberId = Memberid,
                Done = 0
            };
            switch (tasktype)
            {
                case DailyTaskType.Messages:
                    task.Goal = Choice(new int[] { 10, 15, 20, 25, 30, 35, 40, 45, 50 });
                    task.Reward = Choice(new int[] { 50, 75, 100, 125, 150, 175, 200, 225, 250, 275, 300 });
                    break;
                case DailyTaskType.Hourly_Claims:
                    task.Goal = Choice(new int[] { 3, 4, 5 });
                    task.Reward = Choice(new int[] { 50, 75, 100, 125, 150, 175, 200, 225, 250 });
                    break;
                case DailyTaskType.Gamble_Games_Played:
                    task.Goal = Choice(new int[] { 5, 6, 7, 8, 9, 10 });
                    task.Reward = Choice(new int[] { 50, 75, 100, 125, 150, 175, 200 });
                    break;
                case DailyTaskType.Dice_Games_Played:
                    task.Goal = Choice(new int[] { 5, 6, 7, 8, 9, 10 });
                    task.Reward = Choice(new int[] { 50, 75, 100, 125, 150, 175, 200, 225, 250, 275, 300 });
                    break;
                case DailyTaskType.Combined_Elements:
                    task.Goal = Choice(new int[] { 2, 3, 4, 5, 6 });
                    task.Reward = Choice(new int[] { 100, 125, 150, 175, 200, 225, 250, 275, 300 });
                    break;
            }
            toadd.Add(task);
        }
        return toadd;
    }
    public static async Task UpdateDailyTasks()
    {
        // only replace dailytasks if the day is different

        if (DBCache.GetAll<DailyTask>().FirstOrDefault() is not null)
        {
            if (DBCache.GetAll<DailyTask>().FirstOrDefault().LastDayUpdated.Day == DateTime.UtcNow.Day)
            {
                return;
            }
        }

        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();

        DailyTask task = null;

        // in future process this in chunks of like 10k because we would run out of memory
        // no sense in updating daily tasks for a user that is inactive
        DateTime time = DateTime.UtcNow.AddDays(-2);
        foreach (DBUser user in dbctx.Users.Include(x => x.DailyTasks).Where(x => x.LastSentMessage > time))
        {
            List<DailyTask> tasks = GenerateNewDailyTasks(user.Id).ToList();

            foreach (var oldtask in user.DailyTasks)
            {
                task = tasks[0];
                tasks.RemoveAt(0);
                oldtask.Done = 0;
                oldtask.Reward = task.Reward;
                oldtask.LastDayUpdated = task.LastDayUpdated;
                oldtask.TaskType = task.TaskType;
            }
            if (tasks.Count > 0)
            {
                await dbctx.DailyTasks.AddRangeAsync(tasks);
            }
        }
        await dbctx.SaveChangesAsync();
    }
}
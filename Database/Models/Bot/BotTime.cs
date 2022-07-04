namespace PopeAI.Database.Models.Bot;

public class BotTime
{
    [Key]
    public long Id { get; set; }

    public DateTime LastDailyTasksUpdate { get; set; }
}

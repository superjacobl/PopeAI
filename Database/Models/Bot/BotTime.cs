namespace PopeAI.Database.Models.Bot;

public class BotTime : DBItem<BotTime>
{
    [Key]
    public long Id { get; set; }

    public DateTime LastDailyTasksUpdate { get; set; }
    public DateTime LastStatUpdate { get; set; }
    public DateTime LastPlanetStatUpdate { get; set; }
    //public DateTime LastUserStatUpdate { get; set; }
}

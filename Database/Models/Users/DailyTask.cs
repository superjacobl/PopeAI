namespace PopeAI.Database.Models.Users;

public enum DailyTaskType
{
    Messages = 1,
    Hourly_Claims = 2,
    Gamble_Games_Played = 3,
    Dice_Games_Played = 4,
    Combined_Elements = 5
}

public class DailyTask
{
    [Key]
    public ulong Id { get; set; }
    public ulong MemberId { get; set; }
    public int Reward { get; set; }
    public DailyTaskType TaskType { get; set; }
    public int Goal { get; set; }
    public int Done { get; set; }
    public DateTime LastDayUpdated { get; set; }
}
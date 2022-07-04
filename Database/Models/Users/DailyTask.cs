namespace PopeAI.Database.Models.Users;

public enum DailyTaskType
{
    Messages = 1,
    Hourly_Claims = 2,
    Gamble_Games_Played = 3,
    Dice_Games_Played = 4,
    Combined_Elements = 5
}

[Index(nameof(MemberId))]
public class DailyTask : DBItem<DailyTask>
{
    [Key]
    public long Id { get; set; }
    public long MemberId { get; set; }
    public int Reward { get; set; }
    public DailyTaskType TaskType { get; set; }
    public int Goal { get; set; }
    public int Done { get; set; }

    [ForeignKey("MemberId")]
    public virtual DBUser User { get; set; }
}
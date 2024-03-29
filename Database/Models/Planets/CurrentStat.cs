namespace PopeAI.Database.Models.Planets;

public enum CurrentStatType
{
    Coins,
    UserMessage,
    Message
}

public class CurrentStat : DBItem<CurrentStat>
{
    [Key]
    public long PlanetId { get; set; }
    public int DailyNewCoins { get; set; }
    public int DailyMessagesSent { get; set; }
    public int DailyMessagesUsersSent { get; set; }

	public int HourlyNewCoins { get; set; }
	public int HourlyMessagesSent { get; set; }
	public int HourlyMessagesUsersSent { get; set; }

	public int TotalCoins { get; set; }
	public int TotalMessagesUsersSent { get; set; }
	public int TotalMessagesSent { get; set; }

	public DateTime LastStatUpdate { get; set; }

    public CurrentStat()
    {

    }

    public CurrentStat(long planetid)
    {
        PlanetId = planetid;
        DailyNewCoins = 0;
        DailyMessagesSent = 0;
        DailyMessagesUsersSent = 0;
        LastStatUpdate = new DateTime();
    }
}
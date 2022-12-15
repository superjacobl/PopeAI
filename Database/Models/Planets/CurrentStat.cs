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
    public int NewCoins { get; set; }
    public int MessagesSent { get; set; }
    public int MessagesUsersSent { get; set; }
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
        NewCoins = 0;
        MessagesSent = 0;
        MessagesUsersSent = 0;
        LastStatUpdate = new DateTime();
    }
}
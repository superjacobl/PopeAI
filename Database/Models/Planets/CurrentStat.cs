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
    public ulong PlanetId { get; set; }
    public int NewCoins { get; set; }
    public int MessagesSent { get; set; }
    public int MessagesUsersSent { get; set; }
    public DateTime LastStatUpdate { get; set; }

    public CurrentStat(ulong planetid)
    {
        PlanetId = planetid;
        NewCoins = 0;
        MessagesSent = 0;
        MessagesUsersSent = 0;
        LastStatUpdate = new DateTime();
    }
}
namespace PopeAI.Database.Models.Planets;

public enum StatType
{
    Daily = 0,
    Hourly = 1
}

[Index(nameof(PlanetId))]
[Index(nameof(Time))]
public class Stat
{

    // bytes per record
    // int 6x      = 24
    // long 2x    = 16
    // datetime 1x = 8
    // total       = 48

    [Key]
    public long Id { get; set; }
    public long PlanetId { get; set; }
    public int NewCoins { get; set; }
    public int MessagesUsersSent { get; set; }
    public int MessagesSent { get; set; }
	public int TotalCoins { get; set; }
	public int TotalMessagesUsersSent { get; set; }
	public int TotalMessagesSent { get; set; }
	public DateTime Time { get; set; }
    public StatType StatType { get; set; }
}
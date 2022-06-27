namespace PopeAI.Database.Models.Planets;

public class Stat
{

    // bytes per record
    // int 3x      = 12
    // ulong 2x    = 16
    // datetime 1x = 8
    // total       = 36

    [Key]
    public ulong Id { get; set; }
    public ulong PlanetId { get; set; }
    public int NewCoins { get; set; }
    public int MessagesUsersSent { get; set; }
    public int MessagesSent { get; set; }
    public DateTime Time { get; set; }
}
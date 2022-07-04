namespace PopeAI.Database.Models.Planets;

[Index(nameof(PlanetId))]
[Index(nameof(Time))]
public class Stat
{

    // bytes per record
    // int 3x      = 12
    // long 2x    = 16
    // datetime 1x = 8
    // total       = 36

    [Key]
    public long Id { get; set; }
    public long PlanetId { get; set; }
    public int NewCoins { get; set; }
    public int MessagesUsersSent { get; set; }
    public int MessagesSent { get; set; }
    public DateTime Time { get; set; }
}
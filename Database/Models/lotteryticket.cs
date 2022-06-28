namespace PopeAI.Models.Lottery;

public class LotteryTicket
{
    [Key]
    public string Id {get; set;}
    public ulong PlanetId {get; set;}
    public ulong UserId {get; set;}
    public ulong Tickets {get; set;}
}
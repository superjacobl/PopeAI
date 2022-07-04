namespace PopeAI.Models.Lottery;

public class LotteryTicket
{
    [Key]
    public string Id {get; set;}
    public long PlanetId {get; set;}
    public long UserId {get; set;}
    public long Tickets {get; set;}
}
namespace PopeAI.Database.Models.Users;

[Index(nameof(MemberId))]
[Index(nameof(Date))]
public class UserStat : DBItem<UserStat>
{
	// bytes per record
	// long 2x     = 16
	// int 5x      = 20
	// decimal 1x  = 16
	// dateonly 1x = 4
	// total       = 54

	[Key]
    public long Id { get; set; }
    public long MemberId { get; set; }

    [ForeignKey("MemberId")]
    public virtual DBUser User { get; set; }

	public int TotalCoins { get; set; }

	public int TotalPoints { get; set; }
	public int TotalChars { get; set; }	
	public int TotalActiveMinutes { get; set; }
	public int TotalMessages { get; set; }

	[DecimalType]
	public decimal TotalXp { get; set; }
	public DateOnly Date { get; set; }

	[NotMapped]
	public int AvgMessageLength
	{
		get
		{
			return (int)Math.Round(TotalChars / ((decimal)TotalMessages));
		}
	}
}
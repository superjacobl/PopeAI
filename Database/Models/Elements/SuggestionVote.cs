namespace PopeAI.Database.Models.Elements;

public class SuggestionVote
{
    [Key]
    public long Id { get; set; }
    public long UserId { get; set; }
    public long SuggestionId { get; set; }
    public bool VotedFor { get; set; }
}
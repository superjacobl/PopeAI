namespace PopeAI.Database.Models.Elements;

public class SuggestionVote
{
    [Key]
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public ulong SuggestionId { get; set; }
}
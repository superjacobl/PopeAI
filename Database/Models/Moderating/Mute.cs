namespace PopeAI.Database.Models.Moderating;

public class Mute
{
    [Key]
    public long Id { get; set; }

    public DateTime Expire { get; set; }

    public long MinutesMutedFor { get; set; }

    /// <summary>
    /// The id of the member that muted the person
    /// </summary>
    public long Muter { get; set; }

    /// <summary>
    /// The id of the member who is muted
    /// </summary>
    public long MutedId { get; set; }
}
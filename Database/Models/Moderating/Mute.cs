namespace PopeAI.Database.Models.Moderating;

public class Mute
{
    [Key]
    public ulong Id { get; set; }

    public DateTime Expire { get; set; }

    public ulong MinutesMutedFor { get; set; }

    /// <summary>
    /// The id of the member that muted the person
    /// </summary>
    public ulong Muter { get; set; }

    /// <summary>
    /// The id of the member who is muted
    /// </summary>
    public ulong MutedId { get; set; }
}
namespace PopeAI.Database.Models.Moderating;

public enum FliterWordType
{
    Delete,
    Warn,
    Mute
}

public class FliterWord
{
    [Key]
    public long Id { get; set; }
    public FliterWordType fliterWordType { get; set; }
    public int? MinutesToMuteFor { get; set; }

    [VarChar(64)]
    public string Word { get; set; }
}
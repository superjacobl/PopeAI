namespace PopeAI.Database.Models.Planets;

public enum FliterWordType
{
    Delete,
    Warn,
    Mute
}

public class FliterWord
{
    [Key]
    public ulong Id { get; set; }
    public FliterWordType fliterWordType { get; set; }
    public int? SecondsMutedFor { get; set; }
    public string Word { get; set; }
}
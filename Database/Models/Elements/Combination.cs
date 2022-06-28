namespace PopeAI.Database.Models.Elements;

public class Combination
{
    [Key]
    public ulong Id { get; set; }

    [VarChar(16)]
    public string Element1 { get; set; }

    [VarChar(16)]
    public string Element2 { get; set; }

    [VarChar(16)]
    public string? Element3 { get; set; }

    [VarChar(16)]
    public string Result { get; set; }
    public DateTime TimeCreated { get; set; }
    public int Difficulty { get; set; }
}
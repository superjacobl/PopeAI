namespace PopeAI.Database.Models.Elements;

public class Combination
{
    [Key]
    public ulong Id { get; set; }
    public string Element1 { get; set; }
    public string Element2 { get; set; }
    public string Element3 { get; set; }
    public string Result { get; set; }
    public DateTime Time_Created { get; set; }
    public int Difficulty { get; set; }
}
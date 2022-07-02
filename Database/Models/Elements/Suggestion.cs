namespace PopeAI.Database.Models.Elements;

public class Suggestion
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
    public ulong UserId { get; set; }
    public DateTime TimeSuggested { get; set; }
    public int Ayes { get; set; }
    public int Nays { get; set; }

    public Suggestion()
    {
        Ayes = 0;
        Nays = 0;
        TimeSuggested = DateTime.UtcNow;
    }

    public Suggestion(ulong id, string element1, string element2, string element3, string result, ulong userId, DateTime timeSuggested, int ayes, int nays)
    {
        Id = id;
        Element1 = element1;
        Element2 = element2;
        Element3 = element3;
        Result = result;
        UserId = userId;
        TimeSuggested = timeSuggested;
        Ayes = ayes;
        Nays = nays;
    }
}
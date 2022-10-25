namespace PopeAI.Database.Models.Elements;

[Index(nameof(Element1), IsUnique = false)]
[Index(nameof(Element2), IsUnique = false)]
[Index(nameof(Element3), IsUnique = false)]
[Index(nameof(Result), IsUnique = false)]
public class Combination
{
    [Key]
    public long Id { get; set; }

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

    public async ValueTask<int> GetDifficulty(string element, List<string> baseelements, PopeAIDB dbctx)
    {
        if (baseelements.Contains(element))
        {
            return 1;
        }
        return (await dbctx.Combinations.FirstOrDefaultAsync(x => x.Result == element))!.Difficulty;
    }

    public async ValueTask<int> CalcDifficulty()
    {
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
        List<string> baseelements = new() { "fire", "earth", "air", "water" };

        List<int> Difficulties = new();

        Difficulties.Add(await GetDifficulty(Element1, baseelements, dbctx));
        Difficulties.Add(await GetDifficulty(Element2, baseelements, dbctx));
        if (Element3 is not null)
        {
            Difficulties.Add(await GetDifficulty(Element3, baseelements, dbctx));
        }

        return Difficulties.Max() + 1;
    }
}
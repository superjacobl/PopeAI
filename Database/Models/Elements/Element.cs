namespace PopeAI.Database.Models.Elements;

[Index(nameof(Name))]
public class Element
{
    [Key]
    public long Id { get; set; }

    [VarChar(16)]
    public string Name { get; set; }

    /// <summary>
    /// Number of users who have found this element
    /// </summary>
    public int Found { get; set; }

    /// <summary>
    /// The user id of the person who found this first.
    /// </summary>
    public long Finder_Id { get; set; }
    public DateTime Time_Created { get; set; }
}
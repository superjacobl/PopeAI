namespace PopeAI.Database.Models.Elements;

[Index(nameof(UserId))]
[Index(nameof(Element))]
public class UserInvItem
{
    [Key]
    public ulong Id { get; set; }
    public ulong UserId { get; set; }

    [VarChar(16)]
    public string Element { get; set; }
    public DateTime TimeFound { get; set; }

    public UserInvItem(ulong id, ulong userId, string element)
    {
        Id = id;
        UserId = userId;
        Element = element;
        TimeFound = DateTime.UtcNow;
    }

    public UserInvItem()
    {

    }
}
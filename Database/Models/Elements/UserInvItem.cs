namespace PopeAI.Database.Models.Elements;

[Index(nameof(UserId))]
[Index(nameof(Element))]
public class UserInvItem
{
    [Key]
    public long Id { get; set; }
    public long UserId { get; set; }

    [VarChar(16)]
    public string Element { get; set; }
    public DateTime TimeFound { get; set; }

    public UserInvItem(long id, long userId, string element)
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
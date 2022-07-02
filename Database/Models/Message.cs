using System.Security.Cryptography;
using System.Text;

namespace PopeAI.Database.Models.Messaging;

[Index(nameof(AuthorId))]
[Index(nameof(MemberId))]
[Index(nameof(PlanetId))]
[Index(nameof(PlanetIndex))]
[Index(nameof(TimeSent))]
[Index(nameof(MessageIndex))]
[Index(nameof(Hash))]
public class Message
{
    public ulong Id { get; set; }

    /// <summary>
    /// The user's ID
    /// </summary>
    public ulong AuthorId { get; set; }
    public ulong MemberId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong PlanetId { get; set; }

    // a planet will NEVER have more than 4 billion messages
    public int PlanetIndex {get; set;}

    [Text]
    public string? Content { get; set; }

    /// <summary>
    /// The time the message was sent (in UTC)
    /// </summary>
    public DateTime TimeSent { get; set; }

    /// <summary>
    /// Index of the message
    /// </summary>
    public ulong MessageIndex { get; set; }
    public string? EmbedData {get; set;}
    public string? MentionsData { get; set; }

    public byte[] Hash {get; set;}

    // this is used to help staff of a planet in case someone deleted their rule breaking messages
    public bool IsDeleted { get; set; }

    public NpgsqlTsVector SearchVector { get; set; }

    public byte[] GetHash()
    {
        using (SHA256 sha = SHA256.Create())
        {
            string conc = $"{AuthorId}{Content}{TimeSent}{ChannelId}{MessageIndex}{EmbedData}{MentionsData}";

            byte[] buffer = Encoding.Unicode.GetBytes(conc);

            return sha.ComputeHash(buffer);
        }
    }
}    

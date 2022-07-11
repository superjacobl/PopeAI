namespace PopeAI.Database.Models.Bot;

// once per hour
public class BotStat : DBItem<BotStat>
{
    [Key]
    public long Id { get; set; }

    public DateTime Time { get; set; }
    
    public long MessagesSent { get; set; }

    public long MessagesSentSelf { get; set; }
    public long StoredMessages { get; set; }
    public long StoredMessageTotalSize { get; set; }
    public long Commands { get; set; }
    public long UserCount { get; set; }

    // TODO: add cache hit rate for DBUsers

    /// <summary>
    /// in ms
    /// </summary>
    public long TimeTakenTotal { get; set; }

    [NotMapped]
    public long AvgTime
    {
        get
        {
            return TimeTakenTotal / Commands;
        }
    }

    [NotMapped]
    public long AvgStoredMessageSize
    {
        get
        {
            return StoredMessageTotalSize / StoredMessages;
        }
    }

    public BotStat(long id, DateTime time, long messagessent, long storedMessages, long storedMessageTotalSize, long commands, long timeTakenTotal, long messagessentself)
    {
        Id = id;
        Time = time;
        MessagesSent = messagessent;
        StoredMessages = storedMessages;
        StoredMessageTotalSize = storedMessageTotalSize;
        Commands = commands;
        TimeTakenTotal = timeTakenTotal;
        MessagesSentSelf = messagessentself;
    }

    public BotStat()
    {
        Id = StatManager.idManager.Generate();
        Time = DateTime.UtcNow;
    }
}
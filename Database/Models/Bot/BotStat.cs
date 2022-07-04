namespace PopeAI.Database.Models.Bot;

// once per hour
public class BotStat
{
    [Key]
    public long Id { get; set; }

    public DateTime Time { get; set; }
    
    public long MessagesSent { get; set; }

    public long MessagesSentSelf { get; set; }
    public long StoredMessages { get; set; }
    public long StoredMessageTotalSize { get; set; }
    public long Commands { get; set; }

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

    public static async Task<BotStat> GetCurrent()
    {
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
        BotStat? stat = await dbctx.BotStats.OrderBy(x => x.Time).FirstOrDefaultAsync();
        if (stat == null)
        {
            stat = new()
            {
                MessagesSent = 0,
                StoredMessages = 0,
                Commands = 0,
                TimeTakenTotal = 0
            };
            await dbctx.BotStats.AddAsync(stat);
        }
        return stat;
    }
}
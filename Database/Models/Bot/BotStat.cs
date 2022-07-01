namespace PopeAI.Database.Models.Bot;

// once per hour
public class BotStat
{
    [Key]
    public ulong Id { get; set; }

    public DateTime Time { get; set; }
    public ulong MessagesSent { get; set; }

    public ulong MessagesSentSelf { get; set; }
    public ulong StoredMessages { get; set; }
    public ulong StoredMessageTotalSize { get; set; }
    public ulong Commands { get; set; }

    /// <summary>
    /// in ms
    /// </summary>
    public ulong TimeTakenTotal { get; set; }

    [NotMapped]
    public ulong AvgTime
    {
        get
        {
            return TimeTakenTotal / Commands;
        }
    }

    [NotMapped]
    public ulong AvgStoredMessageSize
    {
        get
        {
            return StoredMessageTotalSize / StoredMessages;
        }
    }

    public BotStat(ulong id, DateTime time, ulong messagessent, ulong storedMessages, ulong storedMessageTotalSize, ulong commands, ulong timeTakenTotal, ulong messagessentself)
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
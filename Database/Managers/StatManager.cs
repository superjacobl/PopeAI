using PopeAI.Database.Models.Planets;

namespace PopeAI.Database.Managers;

public static class StatManager
{

    public static readonly IdManager idManager = new();

    public static BotStat selfstat = BotStat.GetCurrent().GetAwaiter().GetResult();

    public static void AddStat(CurrentStatType type, int value, long PlanetId)
    {
        CurrentStat? current = CurrentStat.GetAsync(PlanetId).AsTask().Result;
        if (current is null)
        {
            current = CurrentStat.GetAsync(PlanetId).AsTask().GetAwaiter().GetResult();
            if (current is null)
            {
                current = new(PlanetId);
                DBCache.Put(current.PlanetId, current);
                using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
                dbctx.CurrentStats.Add(current);
                dbctx.SaveChanges();
            }
        }
        switch (type)
        {
            case CurrentStatType.Coins:
                current.NewCoins += value;
                break;
            case CurrentStatType.UserMessage:
                current.MessagesUsersSent += value;
                current.MessagesSent += value;
                break;
            case CurrentStatType.Message:
                current.MessagesSent += value;
                break;
        }
        current.UpdateDB();
    }

    public static async Task CheckStats()
    {
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
        CurrentStat? first = await dbctx.CurrentStats.Where(x => x.MessagesSent != 0).FirstOrDefaultAsync();
        if (first is null) return;

        if (DateTime.UtcNow > first.LastStatUpdate.AddHours(24))
        {
            DBCache.DeleteAll<CurrentStat>();
            foreach (CurrentStat currentstat in dbctx.CurrentStats.Where(x => x.MessagesSent != 0))
            {
                Stat stat = new()
                {
                    Id = idManager.Generate(),
                    PlanetId = currentstat.PlanetId,
                    NewCoins = currentstat.NewCoins,
                    MessagesUsersSent = currentstat.MessagesUsersSent,
                    MessagesSent = currentstat.MessagesSent,
                    Time = DateTime.UtcNow
                };
                dbctx.Add(stat);
                currentstat.NewCoins = 0;
                currentstat.MessagesSent = 0;
                currentstat.MessagesUsersSent = 0;
                currentstat.LastStatUpdate = DateTime.UtcNow;
                DBCache.Put(currentstat.PlanetId, currentstat);
            }
        }

        // check bot stat now
        if (DateTime.Now > selfstat.Time.AddHours(1))
        {
            string query = $"select (data_length + index_length) as Size from information_schema.tables where table_name = 'messages';";
            long Size = PopeAIDB.RawSqlQuery(query, x => new List<long> { Convert.ToUInt64(x[0]) }).First().First();
            BotStat stat = new()
            {
                MessagesSent = selfstat.MessagesSent,
                StoredMessages = selfstat.StoredMessages,
                StoredMessageTotalSize = Size,
                Commands = selfstat.Commands,
                TimeTakenTotal = selfstat.TimeTakenTotal
            };
            dbctx.BotStats.Add(stat);

            selfstat = new()
            {
                MessagesSent = 0,
                StoredMessages = selfstat.StoredMessages,
                StoredMessageTotalSize = 0,
                TimeTakenTotal = 0
            };
        }
        await dbctx.SaveChangesAsync();
    }
}

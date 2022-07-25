using PopeAI.Database.Models.Planets;

namespace PopeAI.Database.Managers;

public static class StatManager
{

    public static readonly IdManager idManager = new();

    public static BotStat selfstat;

    public static async ValueTask AddStat(CurrentStatType type, int value, long PlanetId)
    {
        CurrentStat? current = await CurrentStat.GetAsync(PlanetId);
        if (current is null)
        {
            current = new(PlanetId);
            DBCache.Put(current.PlanetId, current);
            using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
            dbctx.CurrentStats.Add(current);
            dbctx.SaveChanges();
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
        await current.UpdateDB();
    }

    public static async Task CheckStats()
    {
        await selfstat.UpdateDB(false);
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
        if (DateTime.UtcNow > PopeAIDB.botTime.LastStatUpdate.AddHours(24))
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
            PopeAIDB.botTime.LastStatUpdate = DateTime.UtcNow;
            await PopeAIDB.botTime.UpdateDB(false);
        }

        // check bot stat now
        if (DateTime.UtcNow > selfstat.Time.AddHours(1))
        {
            string query = $"SELECT pg_total_relation_size('messages');";
            long Size = PopeAIDB.RawSqlQuery(query, x => new List<long> { Convert.ToInt64(x[0]) }).First().First();
            BotStat stat = new()
            {
                MessagesSent = selfstat.MessagesSent,
                StoredMessages = selfstat.StoredMessages,
                StoredMessageTotalSize = Size,
                Commands = selfstat.Commands,
                TimeTakenTotal = selfstat.TimeTakenTotal,
                UserCount = (long)await dbctx.Users.CountAsync(),
                HeapSize = GC.GetTotalMemory(true),
            };
            dbctx.BotStats.Add(stat);

            selfstat.MessagesSent = 0;
            selfstat.StoredMessages = selfstat.StoredMessages;
            selfstat.StoredMessageTotalSize = 0;
            selfstat.TimeTakenTotal = 0;
            selfstat.Commands = 0;
            selfstat.Time = DateTime.UtcNow;
            await selfstat.UpdateDB(false);
        }
        await dbctx.SaveChangesAsync();
    }
}

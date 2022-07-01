using PopeAI.Database.Models.Planets;

namespace PopeAI.Database.Managers;

public static class StatManager
{

    public static readonly IdManager idManager = new();

    public static BotStat selfstat = BotStat.GetCurrent().GetAwaiter().GetResult();

    public static void AddStat(CurrentStatType type, int value, ulong PlanetId)
    {
        CurrentStat? current = DBCache.Get<CurrentStat>(PlanetId);
        if (current is null)
        {
            current = new CurrentStat(PlanetId);
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
    }

    public static async Task CheckStats()
    {
        CurrentStat? first = DBCache.GetAll<CurrentStat>().FirstOrDefault();
        if (first is null) return;

        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();

        if (DateTime.UtcNow > first.LastStatUpdate.AddHours(24))
        {
            foreach (CurrentStat currentstat in DBCache.GetAll<CurrentStat>().Where(x => x.MessagesSent != 0))
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
            }
        }

        // check bot stat now
        if (DateTime.Now > selfstat.Time.AddHours(1))
        {
            string query = $"select (data_length + index_length) as Size from information_schema.tables where table_name = 'messages';";
            ulong Size = PopeAIDB.RawSqlQuery(query, x => new List<ulong> { Convert.ToUInt64(x[0]) }).First().First();
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

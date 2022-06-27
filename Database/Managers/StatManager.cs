using PopeAI.Database.Models.Planets;

namespace PopeAI.Database.Managers;

public static class StatManager
{

    public static readonly IdManager idManager = new();
    static public PopeAIDB dbctx = new(PopeAIDB.DBOptions);

    public static BotStat selfstat = await BotStat.GetCurrent();

    public static async Task AddStat(CurrentStatType type, int value, ulong PlanetId)
    {
        CurrentStat? current = DBCache.Get<CurrentStat>(PlanetId);
        if (current is null)
        {
            current = new CurrentStat(PlanetId)
            {
                LastStatUpdate = DateTime.UtcNow
            };
            await DBCache.Put(current.PlanetId, current);
            await dbctx.CurrentStats.AddAsync(current);
            await dbctx.SaveChangesAsync();
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

        if (DateTime.UtcNow > first.LastStatUpdate.AddHours(24))
        {
            foreach (CurrentStat currentstat in DBCache.GetAll<CurrentStat>())
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
                await dbctx.AddAsync(stat);
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
            ulong Size = PopeAIDB.RawSqlQuery<List<ulong>>(query, x => new List<ulong> { Convert.ToUInt64(x[0]) }).First().First();
            BotStat stat = new()
            {
                MessagesSent = selfstat.MessagesSent,
                StoredMessages = selfstat.StoredMessages,
                StoredMessageTotalSize = Size,
                Commands = selfstat.Commands,
                TimeTakenTotal = selfstat.TimeTakenTotal
            };
            await dbctx.BotStats.AddAsync(stat);
            await dbctx.SaveChangesAsync();

            selfstat = new()
            {
                MessagesSent = 0,
                StoredMessages = 0,
                StoredMessageTotalSize = 0,
                TimeTakenTotal = 0
            };
        }


        await dbctx.SaveChangesAsync();
    }
}

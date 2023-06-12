using PopeAI.Database.Models.Planets;

namespace PopeAI.Database.Managers;

public static class StatManager
{

    public static readonly IdManager idManager = new();

    public static BotStat selfstat;
    public static bool DoingStatsUpdate = false;

    public static async ValueTask AddStat(CurrentStatType type, int value, long PlanetId)
    {
        CurrentStat? current = await CurrentStat.GetAsync(PlanetId);
        if (current is null)
        {
            current = new(PlanetId);
            DBCache.AddNew(current.PlanetId, current);
        }
        switch (type)
        {
            case CurrentStatType.Coins:
                current.DailyNewCoins += value;
                current.TotalCoins += value;
				current.HourlyNewCoins += value;
				break;
            case CurrentStatType.UserMessage:
                current.DailyMessagesUsersSent += value;
                current.DailyMessagesSent += value;
                current.TotalMessagesUsersSent += value;
                current.TotalMessagesSent += value;
				current.HourlyMessagesUsersSent += value;
				current.HourlyMessagesSent += value;
				break;
            case CurrentStatType.Message:
                current.DailyMessagesSent += value;
                current.TotalMessagesSent += value;
				current.HourlyMessagesSent += value;
				break;
        }
        await current.UpdateDB();
    }

    public static async Task CheckStats()
    {
        DoingStatsUpdate = true;
		await selfstat.UpdateDB(false);
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
		int idsgenerated = 0;
		if (DateTime.UtcNow > PopeAIDB.botTime.LastPlanetStatUpdate.AddHours(1))
        {
            DBCache.DeleteAll<CurrentStat>();
            foreach (CurrentStat currentstat in DBCache.dbctx.CurrentStats.Where(x => x.DailyMessagesSent != 0))
            {
                Stat stat = new()
                {
                    Id = idManager.Generate(),
                    PlanetId = currentstat.PlanetId,
                    NewCoins = currentstat.HourlyNewCoins,
                    MessagesUsersSent = currentstat.HourlyMessagesUsersSent,
                    MessagesSent = currentstat.HourlyMessagesSent,
                    TotalCoins = currentstat.TotalCoins,
                    TotalMessagesSent = currentstat.TotalMessagesSent,
                    TotalMessagesUsersSent = currentstat.TotalMessagesUsersSent,
                    Time = DateTime.UtcNow,
                    StatType = StatType.Hourly
                };
                dbctx.Add(stat);

                currentstat.HourlyNewCoins = 0;
                currentstat.HourlyMessagesSent = 0;
                currentstat.HourlyMessagesUsersSent = 0;
                currentstat.LastStatUpdate = DateTime.UtcNow;
                DBCache.Put(currentstat.PlanetId, currentstat);

                idsgenerated += 1;
                // stop snowflake ids from running out of seq ids
                if (idsgenerated >= 255)
                {
                    idsgenerated = 0;
                    await Task.Delay(1);
                }
            }

			PopeAIDB.botTime.LastPlanetStatUpdate = DateTime.UtcNow;
			await PopeAIDB.botTime.UpdateDB(false);
		}

		if (DateTime.UtcNow > PopeAIDB.botTime.LastStatUpdate.AddHours(24))
		{
			DBCache.DeleteAll<CurrentStat>();
			foreach (CurrentStat currentstat in DBCache.dbctx.CurrentStats.Where(x => x.DailyMessagesSent != 0))
			{
				Stat stat = new()
				{
					Id = idManager.Generate(),
					PlanetId = currentstat.PlanetId,
					NewCoins = currentstat.DailyNewCoins,
					MessagesUsersSent = currentstat.DailyMessagesUsersSent,
					MessagesSent = currentstat.DailyMessagesSent,
					TotalCoins = currentstat.TotalCoins,
					TotalMessagesSent = currentstat.TotalMessagesSent,
					TotalMessagesUsersSent = currentstat.TotalMessagesUsersSent,
					Time = DateTime.UtcNow,
					StatType = StatType.Daily
				};
				dbctx.Add(stat);

				currentstat.DailyNewCoins = 0;
				currentstat.DailyMessagesSent = 0;
				currentstat.DailyMessagesUsersSent = 0;
				currentstat.LastStatUpdate = DateTime.UtcNow;
				DBCache.Put(currentstat.PlanetId, currentstat);

				idsgenerated += 1;
				// stop snowflake ids from running out of seq ids
				if (idsgenerated >= 255)
				{
					idsgenerated = 0;
					await Task.Delay(1);
				}
			}

			// do userstats now
			var currentdateonly = DateOnly.FromDateTime(DateTime.UtcNow);
            var PrevStats = await dbctx.UserStats.Where(x => x.Date.AddDays(1) > currentdateonly).ToListAsync();
            var newstats = new List<UserStat>();
            foreach(var user in DBCache.GetAll<DBUser>())
            {
                var prevstat = PrevStats.FirstOrDefault(x => x.MemberId == user.Id);
                // only save stats if the user has sent a message in the last day
                if (prevstat is not null && prevstat.TotalMessages == user.Messages)
                    continue;

                var newstat = new UserStat()
                {
                    Id = idManager.Generate(),
                    MemberId = user.Id,
                    TotalCoins = user.Coins,
                    TotalPoints = user.TotalPoints,
                    TotalChars = user.TotalChars,
                    TotalActiveMinutes = user.ActiveMinutes,
                    TotalMessages = user.Messages,
                    TotalXp = user.Xp,
                    Date = currentdateonly
                };

                newstats.Add(newstat);

                idsgenerated += 1;
				// stop snowflake ids from running out of seq ids
				if (idsgenerated >= 255)
				{
					idsgenerated = 0;
					await Task.Delay(1);
				}
			}

            dbctx.AddRange(newstats);

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
        DoingStatsUpdate = false;
	}
}

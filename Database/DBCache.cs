namespace PopeAI.Database.Caching;

public class DBCache
{

    // in the future, due to memory, we will only beable to store pretty much planets, roles, mutes, and bans in this cache
    // everything else would need to be stored on the DB
    // but for right now we can just store the entire db in cache for SPEED

    /// <summary>
    /// The high level cache object which contains the lower level caches
    /// </summary>
    public static Dictionary<Type, ConcurrentDictionary<long, object>> HCache = new();

    public static void DeleteAll<T>() where T : class
    {
        var type = typeof(T);

        if (!HCache.ContainsKey(typeof(T)))
            return;

        HCache[typeof(T)].Clear();
    }

    public static IEnumerable<T> GetAll<T>() where T : class
    {
        var type = typeof(T);

        if (!HCache.ContainsKey(type))
            yield break;

        foreach (T item in HCache[type].Values)
            yield return item;
    }

    /// <summary>
    /// Returns true if the cache contains the item
    /// </summary>
    public static bool Contains<T>(long Id) where T : class
    {
        var type = typeof(T);

        if (!HCache.ContainsKey(typeof(T)))
            return false;

        return HCache[type].ContainsKey(Id);
    }

    /// <summary>
    /// Places an item into the cache
    /// </summary>
    public static void Put<T>(long Id, T? obj) where T : class
    {
        // Empty object is ignored
        if (obj == null)
            return;

        // Get the type of the item
        var type = typeof(T);

        // If there isn't a cache for this type, create one
        if (!HCache.ContainsKey(type))
            HCache.Add(type, new ConcurrentDictionary<long, object>());

        if (!HCache[type].ContainsKey(Id))
        {
            HCache[type][Id] = obj;
        }
    }

    /// <summary>
    /// Returns the item for the given id, or null if it does not exist
    /// </summary>
    public static T? Get<T>(long Id) where T : class
    {
        var type = typeof(T);

        if (HCache.ContainsKey(type))
            if (HCache[type].ContainsKey(Id))
                return HCache[type][Id] as T;

        return null;
    }

    public static async Task Load()
    {
        //#if !DEBUG
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
        IEnumerable<DBUser> UsersToCache = await dbctx.Users
            .Where(x => x.LastSentMessage.AddDays(1) > DateTime.UtcNow)
            .OrderByDescending(x => x.Messages).Take(50000)
            .Include(x => x.DailyTasks)
            .ToListAsync();
        foreach (var _obj in UsersToCache)
        {
            Put(_obj.Id, _obj);
        }
        foreach (var _obj in dbctx.CurrentStats.Where(x => x.MessagesSent > 0))
        {
            Put(_obj.PlanetId, _obj);
        }
        foreach (var _obj in dbctx.DailyTasks)
        {
            Put(_obj.Id, _obj);
        }
        foreach (var _obj in dbctx.PlanetInfos)
        {
            Put(_obj.PlanetId, _obj);
        }
        //#endif
    }

    public static async Task SaveAsync()
    {
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
        dbctx.Users.UpdateRange(GetAll<DBUser>());
        dbctx.CurrentStats.UpdateRange(GetAll<CurrentStat>());
        dbctx.DailyTasks.UpdateRange(GetAll<DailyTask>());
        dbctx.PlanetInfos.UpdateRange(GetAll<PlanetInfo>());
        await dbctx.SaveChangesAsync();
    }
}
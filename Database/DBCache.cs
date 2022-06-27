namespace PopeAI.Database.Caching;

public class DBCache
{

    // in the future, due to memory, we will only beable to store pretty much planets, roles, mutes, and bans in this cache
    // everything else would need to be stored on the DB
    // but for right now we can just store the entire db in cache for SPEED

    /// <summary>
    /// The high level cache object which contains the lower level caches
    /// </summary>
    public static Dictionary<Type, ConcurrentDictionary<ulong, object>> HCache = new();

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
    public static bool Contains<T>(ulong Id) where T : class
    {
        var type = typeof(T);

        if (!HCache.ContainsKey(typeof(T)))
            return false;

        return HCache[type].ContainsKey(Id);
    }

    /// <summary>
    /// Places an item into the cache
    /// </summary>
    public static async Task Put<T>(ulong Id, T? obj) where T : class
    {
        // Empty object is ignored
        if (obj == null)
            return;

        // Get the type of the item
        var type = typeof(T);

        // If there isn't a cache for this type, create one
        if (!HCache.ContainsKey(type))
            HCache.Add(type, new ConcurrentDictionary<ulong, object>());

        if (!HCache[type].ContainsKey(Id))
        {
            HCache[type][Id] = obj;
        }
    }

    /// <summary>
    /// Returns the item for the given id, or null if it does not exist
    /// </summary>
    public static T? Get<T>(ulong Id) where T : class
    {
        var type = typeof(T);

        if (HCache.ContainsKey(type))
            if (HCache[type].ContainsKey(Id))
                return HCache[type][Id] as T;

        return null;
    }

    public static async Task LoadAsync()
    {
        //#if !DEBUG

        List<Task> tasks = new();
        foreach (var _obj in PopeAIDB.Instance.Users)
        {
            tasks.Add(Put(_obj.Id, _obj));
        }
        foreach (var _obj in PopeAIDB.Instance.CurrentStats)
        {
            tasks.Add(Put(_obj.PlanetId, _obj));
        }
        foreach (var _obj in PopeAIDB.Instance.DailyTasks)
        {
            tasks.Add(Put(_obj.Id, _obj));
        }
        foreach (var _obj in PopeAIDB.Instance.PlanetInfos)
        {
            tasks.Add(Put(_obj.PlanetId, _obj));
        }
        await Task.WhenAll(tasks);

        //#endif
    }

    public static async Task SaveAsync()
    {
        PopeAIDB.Instance.Users.UpdateRange(GetAll<DBUser>());
        PopeAIDB.Instance.CurrentStats.UpdateRange(GetAll<CurrentStat>());
        PopeAIDB.Instance.DailyTasks.UpdateRange(GetAll<DailyTask>());
        PopeAIDB.Instance.PlanetInfos.UpdateRange(GetAll<PlanetInfo>());
        await PopeAIDB.Instance.SaveChangesAsync();
    }
}
using Database.Models.Users;
using System.Text.Json;

namespace PopeAI.Database.Caching;

public class DBCacheItemAddition
{
    public Type Type { get; set; }
    public object Item { get; set; }

    public void AddToDB()
    {
        if (Type == typeof(DBUser))
            DBCache.dbctx.Add((DBUser)Item);
        else if (Type == typeof(CurrentStat))
            DBCache.dbctx.Add((CurrentStat)Item);
        else if (Type == typeof(DailyTask))
            DBCache.dbctx.Add((DailyTask)Item);
        else if (Type == typeof(PlanetInfo))
            DBCache.dbctx.Add((PlanetInfo)Item);
        else if (Type == typeof(UserEmbedState))
            DBCache.dbctx.Add((UserEmbedState)Item);
    }
}

public class DBCache
{
    // in the future, due to memory, we will only beable to store pretty much planets, roles, mutes, and bans in this cache
    // everything else would need to be stored on the DB
    // but for right now we can just store the entire db in cache for SPEED

    /// <summary>
    /// The high level cache object which contains the lower level caches
    /// </summary>
    public static ConcurrentDictionary<Type, ConcurrentDictionary<long, object>> HCache = new();

    public static ConcurrentQueue<DBCacheItemAddition> ItemQueue = new();

    public static PopeAIDB dbctx { get; set; }

    public static void AddNew<T>(long Id, T? obj, bool AddToCache = true) where T : class
    {
        if (AddToCache)
            Put(Id, obj);
        ItemQueue.Enqueue(new() { Type = typeof(T), Item = obj });
    }

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
    public static void Remove<T>(long Id) where T : class
    {

        // Get the type of the item
        var type = typeof(T);

        // If there isn't a cache for this type, create one
        if (!HCache.ContainsKey(type))
            HCache.TryAdd(type, new ConcurrentDictionary<long, object>());

        if (!HCache[type].ContainsKey(Id)) {
            HCache[type].Remove(Id, out _);
        }
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
            HCache.TryAdd(type, new ConcurrentDictionary<long, object>());

        if (!HCache[type].ContainsKey(Id)) {
            HCache[type].TryAdd(Id, obj);
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
        dbctx = PopeAIDB.DbFactory.CreateDbContext();
        List<DBUser> UsersToCache = await dbctx.Users
            .Where(x => x.LastSentMessage.AddDays(99999) > DateTime.UtcNow)
            .OrderByDescending(x => x.Messages).Take(50000)
            .Include(x => x.DailyTasks)
            .ToListAsync();
        List<long> UserIds = UsersToCache.Select(x => x.Id).ToList();
        foreach (var _obj in UsersToCache)
        {
            Put(_obj.Id, _obj);
        }
        foreach (var _obj in dbctx.CurrentStats.Where(x => x.MessagesSent > 0))
        {
            Put(_obj.PlanetId, _obj);
        }
        foreach (var _obj in dbctx.UserEmbedStates.Where(x => UserIds.Contains(x.MemberId)))
        {
            _obj.Data = JsonSerializer.Deserialize<UserEmbedStateData>(_obj.StringData);
            Put(_obj.MemberId, _obj);
        }

        //foreach (var user in UsersToCache)
        //{
        //     foreach (var _obj in user.DailyTasks)
        //    {
        //        Put(_obj.Id, _obj);
        //    }
        //  }
        foreach (var _obj in dbctx.PlanetInfos)
        {
            if (_obj.Modules is null)
                _obj.Modules = new();
            Put(_obj.PlanetId, _obj);
        }
        //#endif
    }

    public static async Task SaveAsync()
    {
        while (ItemQueue.Count > 0)
        {
            if (ItemQueue.TryDequeue(out var item))
                item.AddToDB();
        }
        foreach (var item in GetAll<UserEmbedState>())
        {
            dbctx.Entry(item).Property(b => b.StringData).IsModified = true;
            var text = JsonSerializer.Serialize(item.Data);
            item.StringData = text;
            //Console.WriteLine(text);
        }
        await dbctx.SaveChangesAsync();
    }

    public static async Task SaveAsync_Old()
    {
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
        dbctx.Users.UpdateRange(GetAll<DBUser>());
        dbctx.CurrentStats.UpdateRange(GetAll<CurrentStat>());
        dbctx.DailyTasks.UpdateRange(GetAll<DailyTask>());
        dbctx.PlanetInfos.UpdateRange(GetAll<PlanetInfo>());
        await dbctx.SaveChangesAsync();
    }
}
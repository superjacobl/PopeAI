using System;

namespace PopeAI.Database.Models;

public interface IDBItem
{
    [NotMapped]
    public PopeAIDB? dbctx { get; set; }

    [NotMapped]
    public bool FromDB { get; set; }
}

public abstract class DBItem<T> : IAsyncDisposable, IDisposable, IDBItem where T : class, IDBItem
{
    [NotMapped]
    public bool FromDB { get; set; }

    [NotMapped]
    public PopeAIDB? dbctx { get; set; }

    /// <summary>
    /// Call this function after the object is not used anymore.
    /// This is intended to save to DB if the object came from the db
    /// </summary>
    public async ValueTask UpdateDB()
    {
        if (FromDB)
        {
            await dbctx!.SaveChangesAsync();
            dbctx.Dispose();
            GC.SuppressFinalize(dbctx);
        }
    }

    public void Dispose()
    {
        UpdateDB().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        await UpdateDB();
    }

    /// <summary>
    /// Gets the object that matches the id and the type.
    /// Unless you set _readonly to true, make sure you call UpdateDB() on the object after you are done using it!
    /// </summary>
    /// <param name="id">The Primary key of the object</param>
    /// <param name="_readonly">True if the item being returned will not be changed.</param>
    public static async ValueTask<T?> GetAsync(long id, bool _readonly = false)
    {
        T? item = DBCache.Get<T>(id);
        if (item is null)
        {
            if (_readonly)
            {
                using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
                item = await dbctx.FindAsync<T>(id);
                return item;
            }
            else
            {
                var dbctx = PopeAIDB.DbFactory.CreateDbContext();
                item = await dbctx.FindAsync<T>(id);
                item!.FromDB = true;
                item.dbctx = dbctx;
                return item;
            }
        }
        return item;
    }
}
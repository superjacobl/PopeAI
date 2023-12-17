using Valour.Shared;
using Valour.Api.Client;

namespace PopeAI.Database.Managers;

public static class MessageManager
{
    static public ConcurrentQueue<Message> messageQueue = new();
    
    public static ConcurrentBag<long> MessagesFromHistoryIds = new();
    public static PopeAIDB dbctx = null;
    public static DateTime TimeSinceLastSave = DateTime.UtcNow;

    public static async ValueTask<TaskResult> SaveMessage(Message message)
    {
        try
        {
            var info = await PlanetInfo.GetAsync((long)message.PlanetId, _readonly: true);
            if (info == null)
            {
                info = new()
                {
                    PlanetId = (long)message.PlanetId,
                    Modules = new() {
                        ModuleType.Xp,
                        ModuleType.Coins
                    }
                };
                DBCache.AddNew(info.PlanetId, info);
            }

            StatManager.selfstat.StoredMessages += 1;
            if (message.AuthorUserId == ValourClient.Self.Id)
            {
                StatManager.selfstat.MessagesSentSelf += 1;
            }

            return new TaskResult(true, "");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return new TaskResult(false, "");
    }

    public static void AddToQueue(Message msg)
    {
        StatManager.selfstat.MessagesSent += 1;
        if (msg.Content.Length > 8)
        {
            if (msg.Content.Substring(0, 7).Contains("/search") || msg.Content.Substring(0, 7).Contains("/view"))
            {
                return;
            }
        }
        messageQueue.Enqueue(msg);
    }

    public static async Task<bool> Run()
    {
		dbctx = PopeAIDB.DbFactory.CreateDbContext();
		while (true)
        {
            if (messageQueue.IsEmpty)
            {
                await Task.Delay(10);
                continue;
            }

            bool dequeued = messageQueue.TryDequeue(out Message msg);

            if (!dequeued)
            {
                continue;
            }

			TaskResult result = await SaveMessage(msg!);

            string success = "SUCC";
            if (!result.Success) success = "FAIL";

            Console.WriteLine($"[{success}] Processed Message [{result.Message}].");
		}
    }
}

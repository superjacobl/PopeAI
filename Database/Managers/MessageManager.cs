using Valour.Shared;
using Valour.Api.Client;

namespace PopeAI.Database.Managers;

public static class MessageManager
{
    static public ConcurrentQueue<PlanetMessage> messageQueue = new();
    public static PopeAIDB dbctx = new(PopeAIDB.DBOptions);

    public static async ValueTask<TaskResult> SaveMessage(PlanetMessage message)
    {
        try
        {
            Message msg = new()
            {
                Id = message.Id,
                AuthorId = message.AuthorId,
                MemberId = message.MemberId,
                Content = message.Content,
                TimeSent = message.TimeSent,
                ChannelId = message.ChannelId,
                MessageIndex = message.MessageIndex,
                PlanetId = message.PlanetId,
                EmbedData = message.EmbedData,
                MentionsData = message.MentionsData
            };
            msg.Hash = msg.GetHash();

            string result = BitConverter.ToString(msg.Hash).Replace("-", string.Empty).Replace("A", "a").Replace("B", "b").Replace("C", "c").Replace("D", "d").Replace("E", "e").Replace("F", "f");

            dbctx.Messages.Add(msg);

            var info = await PlanetInfo.GetAsync(msg.PlanetId);
            if (info == null)
            {
                info = new()
                {
                    PlanetId = message.PlanetId
                };
                DBCache.Put(info.PlanetId, info);
                dbctx.PlanetInfos.Add(info);
                await dbctx.SaveChangesAsync();
            }

            msg.PlanetIndex = info.MessagesStored;
            info.MessagesStored += 1;

            await info.UpdateDB();

            StatManager.selfstat.StoredMessages += 1;
            if (msg.AuthorId == ValourClient.Self.Id)
            {
                StatManager.selfstat.MessagesSentSelf += 1;
            }

            // if message queue is getting too long, then stop saving
            if (messageQueue.Count > 10)
            {

                await dbctx.SaveChangesAsync();

            }

            return new TaskResult(true, result);
        }
        catch (SystemException ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return new TaskResult(false, "");
    }

    public static void AddToQueue(PlanetMessage msg)
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
        while (true)
        {
            if (messageQueue.IsEmpty)
            {
                await Task.Delay(1);
                continue;
            }

            bool dequeued = messageQueue.TryDequeue(out PlanetMessage msg);

            if (!dequeued)
            {
                continue;
            }

            TaskResult result = await SaveMessage(msg!);

            string success = "SUCC";
            if (!result.Success) success = "FAIL";

            Console.WriteLine($"[{success}] Processed Message [{result.Message}].");

            return true;
        }

    }
}

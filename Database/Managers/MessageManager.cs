using PopeAI.Models;
using Valour.Shared;
using Valour.Api.Client;

namespace PopeAI.Database.Managers;

public static class MessageManager
{
    static public ConcurrentQueue<PlanetMessage> messageQueue = new();
    public static PopeAIDB dbctx = new PopeAIDB(PopeAIDB.DBOptions);

    static public async Task<TaskResult> SaveMessage(PlanetMessage message)
    {
        try
        {
            Message msg = new()
            {
                Id = message.Id,
                Author_Id = message.Author_Id,
                Member_Id = message.Member_Id,
                Content = message.Content,
                TimeSent = message.TimeSent,
                Channel_Id = message.Channel_Id,
                MessageIndex = message.MessageIndex,
                Planet_Id = message.Planet_Id,
                EmbedData = message.EmbedData,
                MentionsData = message.MentionsData
            };
            msg.Hash = msg.GetHash();

            // print hash
            string result = BitConverter.ToString(msg.Hash).Replace("-", string.Empty).Replace("A", "a").Replace("B", "b").Replace("C", "c").Replace("D", "d").Replace("E", "e").Replace("F", "f");
            Console.WriteLine(result);

            await dbctx.Messages.AddAsync(msg);
            PlanetInfo? info = DBCache.Get<PlanetInfo>(msg.Planet_Id);
            if (info == null)
            {
                info = new PlanetInfo();
                info.PlanetId = message.Planet_Id;
                await DBCache.Put(info.PlanetId, info);
                await dbctx.PlanetInfos.AddAsync(info);
                await dbctx.SaveChangesAsync();
            }

            if (false)
            {
                Message? last = await dbctx.Messages.Where(x => x.Planet_Id == msg.Planet_Id).OrderByDescending(x => x.Planet_Index).FirstOrDefaultAsync();
                if (last != null)
                {
                    msg.Planet_Index = last.Planet_Index + 1;
                }
                else
                {
                    msg.Planet_Index = 0;
                }
            }
            msg.Planet_Index = info.MessagesStored;
            info.MessagesStored += 1;

            StatManager.selfstat.MessagesSent += 1;
            StatManager.selfstat.StoredMessages += 1;
            if (msg.Author_Id == ValourClient.Self.Id)
            {
                StatManager.selfstat.MessagesSentSelf += 1;
            }

            await dbctx.SaveChangesAsync();
        }
        catch (SystemException ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return new TaskResult(true, "");
    }

    static public void AddToQueue(PlanetMessage msg)
    {
        if (msg.Content.Length > 8)
        {
            if (msg.Content.Substring(0, 7).Contains("/search") || msg.Content.Substring(0, 7).Contains("/view"))
            {
                return;
            }
        }
        messageQueue.Enqueue(msg);
    }

    static public async Task<bool> Run()
    {
        while (true)
        {
            if (messageQueue.IsEmpty)
            {
                await Task.Delay(1);
                continue;
            }

            PlanetMessage msg;
            bool dequeued = messageQueue.TryDequeue(out msg);

            if (!dequeued)
            {
                continue;
            }

            TaskResult result = await SaveMessage(msg);

            string success = "SUCC";
            if (!result.Success) success = "FAIL";

            Console.WriteLine($"[{success}] Processed Message.");

            return true;
        }

    }
}

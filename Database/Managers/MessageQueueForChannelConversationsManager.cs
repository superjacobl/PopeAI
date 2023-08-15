using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valour.Net.Client;
using Valour.Shared;

namespace Database.Managers;

public enum ConversationType
{
    None,
    Small,
    Medium,
    Large
}

public class EntryData
{
    public long Minutes { get; set; }
    public int Messages { get; set; }
}

public class ChannelConversation
{
    public long ChannelId { get; set; }
    public long PlanetId { get; set; }

    public List<DBUser> DBUserCurrentlyParticipating = new();

    public Dictionary<long, List<EntryData>> MessagesSentPerMinuteByDBUserIdLast5Minutes = new();
    public ConversationType ConversationType { get; set; }

    public void SendMessage(string content)
    {
        ValourNetClient.PostMessage(ChannelId, PlanetId, content);
    } 

    public void UpdateConversationType()
    {
        // check for if the conversation is active
        int participating = DBUserCurrentlyParticipating.Count;
        int msgs_in_last_5_minutes = MessagesSentPerMinuteByDBUserIdLast5Minutes.Sum(x => x.Value.Sum(x => x.Messages));
        var lasttype = ConversationType;
        if (participating >= 30 && msgs_in_last_5_minutes >= 300)
            ConversationType = ConversationType.Large;
        else if (participating >= 10 && msgs_in_last_5_minutes >= 100)
            ConversationType = ConversationType.Medium;
        else if (participating >= 3 && msgs_in_last_5_minutes >= 20)
            ConversationType = ConversationType.Small;
        else
            ConversationType = ConversationType.None;

        if (ConversationType != lasttype)
        {
            if (ConversationType == ConversationType.None)
                SendMessage(":disappointed_relieved: The conversation has ended");
            else if (ConversationType < lasttype)
                SendMessage(":small_red_triangle_down: Due to the lack of users participating and/or not enough messages being sent in the last few minutes, " +
                    $"the conversation has been downgraded to {ConversationType}({MessageQueueForChannelConversationsManager.GetBonus(ConversationType)}x xp and coin gain)");
            else
            {
                var secondpart = lasttype == ConversationType.None ? "" : $"from {lasttype}";
                SendMessage(":tada: Due to the number of users participating and lots of messages being sent, " +
                    $"this conversation has been upgraded to {ConversationType} ({MessageQueueForChannelConversationsManager.GetBonus(ConversationType)}x xp and coin gain) {secondpart} :tada:");
            }
        }
    }
}

public static class MessageQueueForChannelConversationsManager
{
    public static ConcurrentDictionary<long, ChannelConversation> ChannelConversations = new();
    public static BlockingCollection<PlanetMessage> MessageQueue = new(new ConcurrentQueue<PlanetMessage>());
    public static bool CurrentlyCheckingConversationsForNotActiveOnes = false;
    public static bool QueueConsumerIsRunning = true;

    public static double GetBonus(ConversationType conversationType)
    {
        return conversationType switch
        {
            ConversationType.None => 1,
            ConversationType.Small => 2,
            ConversationType.Medium => 3,
            ConversationType.Large => 4
        };
    }

    /// <summary>
    /// Holds the long-running queue task
    /// </summary>
    private static Task _queueTask;

    // Timer for executing timed tasks
    private static Timer _timer;

    public static ConversationType GetChannelConversationType(long channelid)
    {
        if (!ChannelConversations.ContainsKey(channelid))
            return ConversationType.None;
        return ChannelConversations[channelid].ConversationType;
    }

    public static Task StartAsync()
    {
        Console.WriteLine("Starting Message Queue Worker For Channel Conversations");

        // Start the queue task
        _queueTask = Task.Run(ConsumeMessageQueue);

        _timer = new Timer(CheckConversationsForNonActiveOnes, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(10));

        return Task.CompletedTask;
    }

    public static void AddToQueue(PlanetMessage msg)
    {
        MessageQueue.Add(msg);
    }

    public static async void CheckConversationsForNonActiveOnes(object? state)
    {
        // First check if queue task is running
        if (_queueTask.IsCompleted)
        {
            // If not, restart it
            _queueTask = Task.Run(ConsumeMessageQueue);

            Console.WriteLine($@"Planet Message Worker queue task stopped at: {DateTime.UtcNow}
                                                 Restarting queue task.");
        }

        CurrentlyCheckingConversationsForNotActiveOnes = true;
        while (QueueConsumerIsRunning)
            await Task.Delay(10);

        long CurrentMinute = (long)Math.Ceiling(DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalMinutes);

        foreach (var channelid in ChannelConversations.Keys)
        {
            var conversation = ChannelConversations[channelid];
            foreach (var pair in conversation.MessagesSentPerMinuteByDBUserIdLast5Minutes.ToList())
            {
                foreach (var entry in pair.Value.ToList())
                {
                    if (CurrentMinute - entry.Minutes >= 5)
                        pair.Value.Remove(entry);
                }

                if (pair.Value.Count == 0)
                {
                    conversation.DBUserCurrentlyParticipating.Remove(conversation.DBUserCurrentlyParticipating.First(x => x.Id == pair.Key));
                    conversation.MessagesSentPerMinuteByDBUserIdLast5Minutes.Remove(pair.Key);
                }
            }
            conversation.UpdateConversationType();
        }

        CurrentlyCheckingConversationsForNotActiveOnes = false;
    }

    /// <summary>
    /// This task should run forever and consume messages from
    /// the queue.
    /// </summary>
    public static async Task ConsumeMessageQueue()
    {
        // This is a stream and will run forever
        foreach (var msg in MessageQueue.GetConsumingEnumerable())
        {
            while (CurrentlyCheckingConversationsForNotActiveOnes)
            {
                QueueConsumerIsRunning = false;
                await Task.Delay(10);
            }
            QueueConsumerIsRunning = true;

            // handle channel conversations
            if (!ChannelConversations.ContainsKey(msg.ChannelId))
            {
                // create one
                ChannelConversation newconversation = new()
                {
                    ChannelId = msg.ChannelId,
                    DBUserCurrentlyParticipating = new(),
                    MessagesSentPerMinuteByDBUserIdLast5Minutes = new(),
                    ConversationType = ConversationType.None,
                    PlanetId = msg.PlanetId
                };
                ChannelConversations.TryAdd(msg.ChannelId, newconversation);
            }

            var conversation = ChannelConversations[msg.ChannelId];

            var user = await DBUser.GetAsync(msg.AuthorMemberId, true);
            if (user is null) continue;
            
            if (!conversation.DBUserCurrentlyParticipating.Any(x => x.Id == msg.AuthorMemberId))
                conversation.DBUserCurrentlyParticipating.Add(user);

            if (!conversation.MessagesSentPerMinuteByDBUserIdLast5Minutes.ContainsKey(user.Id))
                conversation.MessagesSentPerMinuteByDBUserIdLast5Minutes[user.Id] = new();

            long CurrentMinute = (long)Math.Ceiling(DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalMinutes);

            var entry = conversation.MessagesSentPerMinuteByDBUserIdLast5Minutes[user.Id];
            if (!entry.Any(x => x.Minutes == CurrentMinute))
            {
                EntryData pair = new() {
                    Minutes = CurrentMinute,
                    Messages = 0
                };
                entry.Add(pair);
                if (entry.Count >= 6)
                    entry.RemoveAt(0);
                    //entry.RemoveAt(5);
                foreach (var item in entry.ToList())
                {
                    if (CurrentMinute-item.Minutes > 5)
                    {
                        entry.Remove(item);
                    }
                }
            }
            conversation.MessagesSentPerMinuteByDBUserIdLast5Minutes[user.Id].First(x => x.Minutes == CurrentMinute).Messages += 1;
            QueueConsumerIsRunning = false;
        }
    }
}

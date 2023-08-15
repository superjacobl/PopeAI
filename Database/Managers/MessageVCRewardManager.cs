using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valour.Net.Client;
using Valour.Shared;
using Valour.Net.PlanetEconomy;
using Valour.Api.Models.Economy;
using Valour.Shared.Models;
using Valour.Api.Client;

namespace Database.Managers;

public static class MessageVCRewardManager
{
    public static Random Rng = new Random();
    public static BlockingCollection<PlanetMessage> MessageQueue = new(new ConcurrentQueue<PlanetMessage>());
    public static Dictionary<long, long> LastWinnerByPlanetId = new();
    public static EcoAccount BotEcoAccountForVC = null;
    public static Dictionary<long, long> UserIdToVCEcoAccountId = new();
    public static Dictionary<long, EntryData> EntryDataForUsers = new();

    /// <summary>
    /// Holds the long-running queue task
    /// </summary>
    private static Task _queueTask;

    // Timer for executing timed tasks
    private static Timer _timer;

    public static async Task StartAsync()
    {
        Console.WriteLine("Starting Message Queue Worker For Giving Out VC Reward Drops");


        BotEcoAccountForVC = await EcoAccount.GetSelfGlobalAccountAsync();

		// Start the queue task
		_queueTask = Task.Run(ConsumeMessageQueue);

		_timer = new Timer(Tick, null, TimeSpan.Zero,
			TimeSpan.FromSeconds(10));

        //return Task.CompletedTask;
    }

    public static void AddToQueue(PlanetMessage msg)
    {
        MessageQueue.Add(msg);
    }

    public static async void Tick(object? state)
    {
        // First check if queue task is running
        if (_queueTask.IsCompleted)
        {
            // If not, restart it
            _queueTask = Task.Run(ConsumeMessageQueue);

            Console.WriteLine($@"Message Queue Worker For Giving Out VC Reward Drops queue task stopped at: {DateTime.UtcNow}
                                                 Restarting queue task.");
        }
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
            if (msg.PlanetId != ISharedPlanet.ValourCentralId)
				continue;

			long CurrentMinute = (long)Math.Ceiling(DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalMinutes);

			if (!EntryDataForUsers.ContainsKey(msg.AuthorUserId))
			{
				EntryData pair = new()
				{
					Minutes = CurrentMinute,
					Messages = 0
				};
				EntryDataForUsers[msg.AuthorUserId] = pair;
			}
			if (CurrentMinute != EntryDataForUsers[msg.AuthorUserId].Minutes)
			{
				EntryDataForUsers[msg.AuthorUserId].Minutes = CurrentMinute;
				EntryDataForUsers[msg.AuthorUserId].Messages = 0;
			}
			EntryDataForUsers[msg.AuthorUserId].Messages += 1;

            // only 8 messages from a user per minute can entered in to win
			if (EntryDataForUsers[msg.AuthorUserId].Messages >= 5)
				continue;

			// 1 out of 200 chance
			// 0.5% chance
			//if (Rng.Next(1, 201) >= 1)
			if (Rng.Next(1, 201) == 1)
            {
				if (!LastWinnerByPlanetId.ContainsKey(msg.PlanetId))
                    LastWinnerByPlanetId[msg.PlanetId] = 0;

                // same user can not get reward twice in a row per planet
                if (LastWinnerByPlanetId[msg.PlanetId] == msg.AuthorUserId)
					continue;

				var user = await User.FindAsync(msg.AuthorUserId);
				if (!UserIdToVCEcoAccountId.ContainsKey(msg.AuthorUserId))
                {
					var result = await EcoAccount.FindGlobalIdByNameAsync(user.NameAndTag);
					UserIdToVCEcoAccountId[msg.AuthorUserId] = result.AccountId;
				}

                long EcoAccountId = UserIdToVCEcoAccountId[msg.AuthorUserId];

				var value = Rng.Next(1, 1001);
				decimal amount = 0.0m;
                // 50%
				if (value <= 500) amount = 20;
                // 35%
				else if (value <= 750) amount = 30;
                // 10%
				else if (value <= 950) amount = 50;
                // 5%
				else if (value <= 1000) amount = 100;
				else amount = 20;

				Transaction tran = new Transaction()
                {
                    PlanetId = ISharedPlanet.ValourCentralId,
					UserFromId = ValourNetClient.BotId,
                    AccountFromId = BotEcoAccountForVC.Id,
                    UserToId = msg.AuthorUserId,
                    AccountToId = EcoAccountId,
                    TimeStamp = DateTime.UtcNow,
                    Description = "Random VC Drop From Sending a Message",
                    Fingerprint = Guid.NewGuid().ToString(),
                    Amount = amount
				};

                var tran_result = await Transaction.SendTransactionAsync(tran);

                Console.WriteLine($"VC Reward Drop of {amount}");
				ValourNetClient.PostMessage(msg.ChannelId, msg.PlanetId, $":tada: {user.Name}, you have received random VC Drop of {amount}! (0.5% chance of this happening per message sent)");
                LastWinnerByPlanetId[msg.PlanetId] = msg.AuthorUserId;
			}
        }
    }
}

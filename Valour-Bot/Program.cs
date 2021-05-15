﻿using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using PopeAI.Database;
using Microsoft.EntityFrameworkCore;
using PopeAI.Models;
using Valour.Net;
using Valour.Net.Models;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;

namespace PopeAI
{
    class Program
    {

        static HttpClient client = new HttpClient();

        public static Dictionary<string, DateTime> timesincelastmessage = new Dictionary<string, DateTime>();

        static Dictionary<string, ulong> MessagesPerMinuteInARow = new Dictionary<string, ulong>();

        static PopeAIDB Context = new PopeAIDB(PopeAIDB.DBOptions);

        static ulong TotalMessages = 0;

        static List<List<ulong>> MessageCount {get; set;}

        static Random rnd = new Random();

        static List<Planet> planets {get; set;}
        public static Dictionary<ulong, int> MessagesThisMinute = new Dictionary<ulong, int>();

        // cache of lotteries

        static Dictionary<ulong, Lottery> lotterycache = new Dictionary<ulong, Lottery>();


        public static byte[] StringToByteArray(string hex) {
            return Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();
        }

        public static async Task Main(string[] args)
        {

            ValourClient.BotPrefix = "/";



            await ValourClient.Start(Client.Config.Email,Client.Config.BotPassword);

            ValourClient.RegisterModules();
            
            ValourClient.OnMessage += async (message) => {
                await OnMessageRecieve(message);
            };

            Console.WriteLine("Hello World!");
            foreach (Lottery lottery in Context.Lotteries) {
                lotterycache.Add(lottery.PlanetId, lottery);
            }

            Task task = Task.Run(async () => UpdateHourly( Context));

            while (true)
            {
                Console.ReadLine();
            }

        }

        static async Task UpdateHourly(PopeAIDB Context) {
            while (true) {
                await Context.UpdateLotteries(lotterycache, Context);
                await Context.UpdateStats(Context);
                await Context.UpdateRoleIncomes(planets, false, Context);
                await Task.Delay(60000);
            }
        }

        static async Task OnMessageRecieve(PlanetMessage message)
        {

            string dictkey = $"{message.Author_Id}-{message.Planet_Id}";

            bool IsVaild = false;

            PlanetMember ClientUser = null;
            ClientUser = await message.GetAuthorAsync();

            if (MessagesThisMinute.ContainsKey(ClientUser.Id)) {
                MessagesThisMinute[ClientUser.Id] += 1;
            }
            else {
                MessagesThisMinute.Add(ClientUser.Id, 1);
            }

            if (timesincelastmessage.ContainsKey(dictkey)) {
                if (timesincelastmessage[dictkey].AddSeconds(60) < DateTime.UtcNow) {
                    if (timesincelastmessage[dictkey].AddMinutes(5) < DateTime.UtcNow) {
                        MessagesPerMinuteInARow[dictkey] = 0;
                    }
                    else {
                        if (MessagesPerMinuteInARow[dictkey] < 60) {
                            MessagesPerMinuteInARow[dictkey] += 1;
                        }
                    }
                    MessagesThisMinute[ClientUser.Id] = 1;
                    IsVaild = true;
                    timesincelastmessage[dictkey] = DateTime.UtcNow;
                }
            }
            else {
                IsVaild = true;
                timesincelastmessage.Add(dictkey, DateTime.UtcNow);
                MessagesPerMinuteInARow.Add(dictkey, 1);
            }

            await Context.AddStat("Message", 1, message.Planet_Id, Context);

            if (message.Author_Id == ulong.Parse(Client.Config.BotId)) {
                return;
            }

            if (MessagesThisMinute[ClientUser.Id] > 30) {
                await PostMessage(message.Channel_Id, message.Planet_Id, $"Stop spamming {ClientUser.Nickname}!");
            }

            User user = await Context.Users.FirstOrDefaultAsync(x => x.UserId == message.Author_Id && x.PlanetId == message.Planet_Id);

            ulong xp = 1 + MessagesPerMinuteInARow[dictkey];

            await Context.AddStat("UserMessage", 1, message.Planet_Id, Context);

            if (user == null) {
                user = new User();
                ulong num = (ulong)rnd.Next(1,int.MaxValue);
                while (await Context.Users.FirstOrDefaultAsync(x => x.Id == num) != null) {
                    num = (ulong)rnd.Next(1,int.MaxValue);
                }
                user.Id = num;
                user.PlanetId = message.Planet_Id;
                user.UserId = message.Author_Id;
                user.Coins += xp*2;
                user.Xp += xp;
                await Context.AddStat("Coins", (double)xp*2, message.Planet_Id, Context);
                await Context.Users.AddAsync(user);
                await Context.SaveChangesAsync();
            }

            if (lotterycache.ContainsKey(message.Planet_Id)) {
                if (lotterycache[message.Planet_Id].Type == "message") {
                    Lottery lottery = await Context.Lotteries.FirstOrDefaultAsync(x => x.PlanetId == message.Planet_Id);
                    lottery.Jackpot += lottery.JackpotIncreasePerMesage;
                    await lottery.AddTickets(message.Author_Id, 1, message.Planet_Id, Context);
                }
            }

            if (IsVaild) {
                user.Xp += xp;
                user.Coins += xp*2;
                await Context.AddStat("Coins", (double)xp*2, message.Planet_Id, Context);
                await Context.SaveChangesAsync();
            
            }

            if (message.Content.Substring(0,1) == Client.Config.CommandSign) {
                string command = message.Content.Split(" ")[0];
                command = command.Replace("\n", "");
                List<string> ops = message.Content.Split(" ").ToList();
                command = command.Replace(Client.Config.CommandSign,"");

                if (command == "roll") {
                    if (ops.Count < 3) {
                        await PostMessage(message.Channel_Id, message.Planet_Id, "Command Format: /roll <from> <to>");
                        return;
                    }
                    int from = int.Parse(ops[1]);
                    int to = int.Parse(ops[2]);
                    int num = rnd.Next(from, to);
                    await PostMessage(message.Channel_Id, message.Planet_Id, $"Roll: {num}");
                }
                if (command == "eco") {
                    if (ops.Count == 1) {
                        ops.Add("");
                    }
                    switch (ops[1])
                    {
                        case "cap":
                            int total = 0;
                            foreach(User USER in Context.Users) {
                                total += (int)USER.Coins;
                            }
                            await PostMessage(message.Channel_Id, message.Planet_Id, $"Eco cap: {total} coins");
                            break;
                        default:
                            await PostMessage(message.Channel_Id, message.Planet_Id, "Available Commands: /eco cap");
                            break;
                    }
                }

                if (command == "forcerolepayout") {
                    if (await ClientUser.IsOwner() != true) {
                        await PostMessage(message.Channel_Id, message.Planet_Id, $"Only the owner of this server can use this command!");
                        return;
                    }
                    await Context.UpdateRoleIncomes(planets, true, Context);

                    await PostMessage(message.Channel_Id, message.Planet_Id, "Successfully forced a role payout.");

                }
                if (command == "lottery") {
                    if (ops.Count() == 1) {
                        ops.Add("");
                    }
                    switch (ops[1])
                    {
                        case "timeleft":
                            Lottery lottery = await Context.Lotteries.FirstOrDefaultAsync(x => x.PlanetId == message.Planet_Id);
                            if (lottery == null) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, $"There is no lottery currently going on!");
                                break;
                            }
                            TimeSpan timeleft = lottery.EndDate.Subtract(DateTime.UtcNow);
                            await PostMessage(message.Channel_Id, message.Planet_Id, $"{timeleft.Hours} hours left");
                            break;
                        case "tickets":
                            LotteryTicket ticket = await Context.LotteryTickets.FirstOrDefaultAsync(x => x.PlanetId == message.Planet_Id && x.UserId == message.Author_Id);
                            if (ticket == null) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, $"There is no lottery currently going on!");
                                break;
                            }
                            await PostMessage(message.Channel_Id, message.Planet_Id, $"You have {ticket.Tickets} tickets");
                            break;
                        case "jackpot":
                            lottery = await Context.Lotteries.FirstOrDefaultAsync(x => x.PlanetId == message.Planet_Id);
                            if (lottery == null) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, $"There is no lottery currently going on!");
                                break;
                            }
                            await PostMessage(message.Channel_Id, message.Planet_Id, $"The current jackpot is {lottery.Jackpot}");
                            break;
                        case "create":

                            if (await ClientUser.IsOwner() != true) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, $"Only the owner of this server can use this command!");
                                break;
                            }

                            if (ops.Count() < 4) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, "Command Format: /lottery create coin <hours> or /lottery create message <how many coins will each message add> <hours>");
                                break;
                            }

                            string type = ops[2];

                            int HoursToLast = 0;

                            if (type == "message") {
                                if (ops.Count() < 5) {
                                    await PostMessage(message.Channel_Id, message.Planet_Id, "Command Format: /lottery create message <how many coins will each message add> <hours>");
                                    break;
                                }
                                HoursToLast = int.Parse(ops[4]);
                            }
                            else {
                                HoursToLast = int.Parse(ops[3]);
                            }

                            if (HoursToLast > 24) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, "You can not have a lottery last more than 24 hours!");
                                break;
                            }

                            if (type != "coin" && type != "message") {
                                await PostMessage(message.Channel_Id, message.Planet_Id, "Type must either by coin or message!");
                                break;
                            }

                            // check if the planet is areadly doing a lottery

                            lottery = await Context.Lotteries.FirstOrDefaultAsync(x => x.PlanetId == message.Planet_Id);

                            if (lottery != null) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, "You can not have two lotteries running at the same time!");
                                break;
                            }

                            lottery = new Lottery();
                            
                            lottery.StartDate = DateTime.UtcNow;
                            lottery.Type = type;
                            lottery.PlanetId = message.Planet_Id;
                            lottery.EndDate = DateTime.UtcNow.AddHours(HoursToLast);
                            lottery.Jackpot = 0;
                            lottery.ChannelId = message.Channel_Id;

                            if (type == "message") {
                                lottery.JackpotIncreasePerMesage = (double)int.Parse(ops[3]);
                            }
                            else {
                                lottery.JackpotIncreasePerMesage = 0;
                            }
                            await Context.Lotteries.AddAsync(lottery);
                            lotterycache.Add(lottery.PlanetId, lottery);
                            await PostMessage(message.Channel_Id, message.Planet_Id, "Successfully created a lottery.");
                            await Context.SaveChangesAsync();
                            break;
                        default:
                            await PostMessage(message.Channel_Id, message.Planet_Id, "Command Format: /lottery create coin <hours> or /lottery create message <how many coins will each message add> <hours>");
                            break;
                    }
                }

                if (command == "roleincome") {
                    if (ops.Count == 1) {
                        ops.Add("");
                    }
                    switch (ops[1])
                    {
                        case "set":

                            if (await ClientUser.IsOwner() != true) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, $"Only the owner of this server can use this command!");
                                break;
                            }

                            if (ops.Count() < 4) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, "Command Format: /roleincome set <hourly income/cost> <rolename>");
                                break;
                            }

                            string rolename = message.Content.Replace($"{Client.Config.CommandSign}roleincome set {ops[2]} ", "");

                            RoleIncomes roleincome = await Context.RoleIncomes.FirstOrDefaultAsync(x => x.RoleName == rolename && x.PlanetId == message.Planet_Id);

                            if (roleincome == null) {

                                ClientRole clientrole = await planets.FirstOrDefault(x => x.Id == message.Planet_Id).GetRoleAsync(rolename);

                                if (clientrole == null) {
                                    await PostMessage(message.Channel_Id, message.Planet_Id, $"Could not find role {rolename}!");
                                    break;
                                }

                                roleincome = new RoleIncomes();

                                roleincome.Income = double.Parse(ops[2]);
                                roleincome.RoleId = clientrole.Id;
                                roleincome.PlanetId = message.Planet_Id;
                                roleincome.RoleName = clientrole.Name;
                                roleincome.LastPaidOut = DateTime.UtcNow;

                                Context.RoleIncomes.Add(roleincome);

                                Context.SaveChanges();

                                await PostMessage(message.Channel_Id, message.Planet_Id, $"Set {rolename}'s hourly income/cost to {roleincome.Income} coins!");

                                break;
                            }

                            else {
                                roleincome.Income = double.Parse(ops[2]);
                                await Context.SaveChangesAsync();
                                await PostMessage(message.Channel_Id, message.Planet_Id, $"Set {rolename}'s hourly income/cost to {roleincome.Income} coins!");
                            }

                            break;



                        default:

                            if (ops[1] == "") {
                                await PostMessage(message.Channel_Id, message.Planet_Id, "Commands:\n/roleincome set <hourly income/cost> <rolename>\n/roleincome <rolename>");
                                break;
                            }
                        
                            rolename = message.Content.Replace($"{Client.Config.CommandSign}roleincome ", "");

                            ClientRole role = await planets.FirstOrDefault(x => x.Id == message.Planet_Id).GetRoleAsync(rolename);

                            if (role == null) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, $"Could not find role {rolename}");
                                break;
                            }

                            roleincome = await Context.RoleIncomes.FirstOrDefaultAsync(x => x.RoleName == rolename && x.PlanetId == message.Planet_Id);

                            if (roleincome == null) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, $"Hourly Income/Cost has not been set for role {rolename}");
                                break;
                            }

                            await PostMessage(message.Channel_Id, message.Planet_Id, $"Hourly Income/Cost for {rolename} is {roleincome.Income} coins");

                            break;

                    }
                }

                if (command == "shop") {
                    if (ops.Count == 1) {
                        ops.Add("");
                    }
                    switch (ops[1])
                    {
                        case "addrole":

                            if (ops.Count < 3) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, "Command Format: /shop addrole <cost> <rolename>");
                                break;
                            }

                            if (await ClientUser.IsOwner() != true) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, $"Only the owner of this server can use this command!");
                                break;
                            }

                            string rolename = message.Content.Replace($"{Client.Config.CommandSign}shop addrole {ops[2]} ", "");

                            ClientRole role = await planets.FirstOrDefault(x => x.Id == message.Planet_Id).GetRoleAsync(rolename);

                            if (role == null) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, $"Could not find role {rolename}!");
                                break;
                            }

                            ShopReward reward = new ShopReward();

                            ulong num = (ulong)rnd.Next(1,int.MaxValue);
                            while (await Context.ShopRewards.FirstOrDefaultAsync(x => x.Id == num) != null) {
                                num = (ulong)rnd.Next(1,int.MaxValue);
                            }

                            reward.Id = num;
                            reward.Cost = double.Parse(ops[2]);
                            reward.PlanetId = message.Planet_Id;
                            reward.RoleId = role.Id;
                            reward.RoleName = rolename;

                            Context.ShopRewards.Add(reward);

                            Context.SaveChanges();

                            await PostMessage(message.Channel_Id, message.Planet_Id, $"Added {rolename} to the shop!");

                            break;

                        case "buy":
                            if (ops.Count < 2) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, "Command Format: /shop buy <rolename>");
                                break;
                            }

                            rolename = message.Content.Replace($"{Client.Config.CommandSign}shop buy ", "");

                            reward = await Context.ShopRewards.FirstOrDefaultAsync(x => x.RoleName == rolename && x.PlanetId == message.Planet_Id);

                            if (reward == null) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, $"Could not find shopreward {rolename}!");
                                break;
                            }

                            User User = await Context.Users.FirstOrDefaultAsync(x => x.UserId == message.Author_Id && x.PlanetId == message.Planet_Id);

                            if (User.Coins < reward.Cost) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, $"You need {reward.Cost-User.Coins} more coins to buy this role!");
                                break;
                            }

                            User.Coins -= reward.Cost;

                            await Context.AddStat("Coins", 0-reward.Cost, message.Planet_Id, Context);

                            //await ClientUser.GiveRole(reward.RoleName);

                            await PostMessage(message.Channel_Id, message.Planet_Id, $"Gave you {reward.RoleName}!");

                            await Context.SaveChangesAsync();

                            break;



                        default:
                            string content = "| Rolename | Cost |\n| :- | :-\n";

                            List<ShopReward> rewards = await Task.Run(() => Context.ShopRewards.Where(x => x.PlanetId == message.Planet_Id).OrderByDescending(x => x.Cost).ToList());

                            foreach (ShopReward Reward in rewards) {
                                content += $"{Reward.RoleName} | {(ulong)Reward.Cost} xp\n";
                            }

                            await PostMessage(message.Channel_Id, message.Planet_Id, content);

                            break;

                    }
                }
            }

            Console.WriteLine(message.Content);
        }

        static async Task SlowMessages(List<string> data, ulong Channel_Id, ulong Planet_Id) {
            foreach (string content in data) {
                await PostMessage(Channel_Id, Planet_Id, content);
                await Task.Delay(1500);
            }
        }

        public static async Task PostMessage(ulong channelid, ulong planetid, string msg)
        {
            ClientPlanetMessage message = new ClientPlanetMessage()
            {
                Channel_Id = channelid,
                Content = msg,
                TimeSent = DateTime.UtcNow,
                Author_Id = ulong.Parse(Client.Config.BotId),
                Planet_Id = planetid
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(message);

            Console.WriteLine("SEND: \n" + json);

            HttpResponseMessage httpresponse = await client.PostAsJsonAsync<ClientPlanetMessage>($"https://valour.gg/Channel/PostMessage?token={Client.Config.AuthKey}", message);
            
            TaskResult response = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskResult>(await httpresponse.Content.ReadAsStringAsync());

            Console.WriteLine("Sending Message!");

            Console.WriteLine(response.ToString());
        }
    }
}

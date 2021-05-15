using System;
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

            Client.Check();

            ValourClient.BotPrefix = "/";

            await ValourClient.Start("jacoblower26@gmail.com",Client.Config.BotPassword);

            ValourClient.RegisterModules();
            
            ValourClient.OnMessage += async (message) => {
                await OnMessageRecieve(message);
            };

            Console.WriteLine("Hello World!");
            foreach (Lottery lottery in Context.Lotteries) {
                lotterycache.Add(lottery.PlanetId, lottery);
            }

            Task task = Task.Run(async () => UpdateHourly( Client.DBContext));

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
                await Context.UpdateDailyTasks(Context);
                await Task.Delay(60000);
            }
        }

        static async Task OnMessageRecieve(PlanetMessage message)
        {

            string dictkey = $"{message.Author_Id}-{message.Planet_Id}";

            PlanetMember ClientUser = null;
            ClientUser = await message.GetAuthorAsync();

            if (MessagesThisMinute.ContainsKey(ClientUser.Id)) {
                MessagesThisMinute[ClientUser.Id] += 1;
            }
            else {
                MessagesThisMinute.Add(ClientUser.Id, 1);
            }

            if (message.Author_Id == ulong.Parse(Client.Config.BotId)) {
                return;
            }

            User user = await Context.Users.FirstOrDefaultAsync(x => x.UserId == message.Author_Id && x.PlanetId == message.Planet_Id);

            if (lotterycache.ContainsKey(message.Planet_Id)) {
                if (lotterycache[message.Planet_Id].Type == "message") {
                    Lottery lottery = await Context.Lotteries.FirstOrDefaultAsync(x => x.PlanetId == message.Planet_Id);
                    lottery.Jackpot += lottery.JackpotIncreasePerMesage;
                    await lottery.AddTickets(message.Author_Id, 1, message.Planet_Id, Context);
                }
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
            PlanetMessage message = new PlanetMessage()
            {
                Channel_Id = channelid,
                Content = msg,
                TimeSent = DateTime.UtcNow,
                Author_Id = ulong.Parse(Client.Config.BotId),
                Planet_Id = planetid
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(message);

            Console.WriteLine("SEND: \n" + json);

            HttpResponseMessage httpresponse = await client.PostAsJsonAsync<PlanetMessage>($"https://valour.gg/Channel/PostMessage?token={Client.Config.AuthKey}", message);
            
            TaskResult response = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskResult>(await httpresponse.Content.ReadAsStringAsync());

            Console.WriteLine("Sending Message!");

            Console.WriteLine(response.ToString());
        }
    }
}

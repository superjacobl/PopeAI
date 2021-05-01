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

        public static Dictionary<ulong, string> ScrambledWords = new Dictionary<ulong, string>(); 

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
            await Client.hubConnection.StartAsync();

            //Get all the planets that we are in

            string json = await client.GetStringAsync($"https://valour.gg/Planet/GetPlanetMembership?user_id={Client.Config.BotId}&token={Client.Config.AuthKey}");

            TaskResult<List<Planet>> result = JsonConvert.DeserializeObject<TaskResult<List<Planet>>>(json);

            planets = result.Data;

            foreach(Planet planet in planets) {
                await Client.hubConnection.SendAsync("JoinPlanet", planet.Id, Client.Config.AuthKey);
                foreach(Channel channel in await planet.GetChannelsAsync()) {
                    await Client.hubConnection.SendAsync("JoinChannel", channel.Id, Client.Config.AuthKey);
                }
            }
            Client.hubConnection.On<string>("Relay", OnMessageRecieve);
            //await Channel.CreateChannel("Coding", 735703679107073, 735703679107072);
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

        static string ScrambleWord(string word) 
        { 
            char[] chars = new char[word.Length]; 
            Random rand = new Random(); 
            int index = 0; 
            while (word.Length > 0) 
            { // Get a random number between 0 and the length of the word. 
                int next = rand.Next(0, word.Length - 1); // Take the character from the random position 
                                                        //and add to our char array. 
                chars[index] = word[next];                // Remove the character from the word. 
                word = word.Substring(0, next) + word.Substring(next + 1); 
                ++index; 
            } 
            return new String(chars); 
        }  

        static async Task OnMessageRecieve(string json)
        {
            ClientPlanetMessage message = JsonConvert.DeserializeObject<ClientPlanetMessage>(json);

            string dictkey = $"{message.Author_Id}-{message.Planet_Id}";

            bool IsVaild = false;

            ClientPlanetUser ClientUser = null;
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

            if (ScrambledWords.ContainsKey(ClientUser.Id)) {
                if (ScrambledWords[ClientUser.Id] == message.Content.ToLower()) {
                    double reward = (double)rnd.Next(1,20);
                    await Context.AddStat("Coins", reward, message.Planet_Id, Context);
                    user.Coins += reward;
                    await Context.SaveChangesAsync();
                    await PostMessage(message.Channel_Id, message.Planet_Id, $"Correct! Your reward is {reward} coins.");
                }
                else {
                    await PostMessage(message.Channel_Id, message.Planet_Id, $"Incorrect. The correct word was {ScrambledWords[ClientUser.Id]}");
                }
                ScrambledWords.Remove(ClientUser.Id);
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

                if (command == "help") {
                    int skip = 0;
                    if (ops.Count == 2) {
                        skip = int.Parse(ops[1]);
                        skip *= 10;
                    }
                    string content = "| command |\n| :-: |\n";
                    foreach (Help help in Context.Helps.Skip(skip).Take(10)) {
                        content += $"| {help.Message} |\n";
                    }
                    await PostMessage(message.Channel_Id, message.Planet_Id, content);
                }
                if (command == "isdiscordgood") {
                    await PostMessage(message.Channel_Id, message.Planet_Id, $"no, dickcord is bad!");
                }
                if (command == "xp") {
                    user = await Context.Users.FirstOrDefaultAsync(x => x.UserId == message.Author_Id && x.PlanetId == message.Planet_Id);
                    await PostMessage(message.Channel_Id, message.Planet_Id, $"{ClientUser.Nickname}'s xp: {(ulong)user.Xp}");
                }

                if (command == "unscramble") {
                    List<string> words = new List<string>();
                    words.AddRange("people,history,way,art,world,information,map,two,family,government,health,system,computer,meat,year,thanks,music,person,reading,method,data,food,understanding,theory,law,bird,problem,software,control,power,love,internet,phone,television,science,library,nature,fact,product,idea,temperature,investment,area,society,story,activity,industry".Split(","));
                    string pickedword = words[rnd.Next(0,words.Count())];
                    string scrambed = ScrambleWord(pickedword);
                    ScrambledWords.Add(ClientUser.Id, pickedword);
                    await PostMessage(message.Channel_Id, message.Planet_Id, $"Unscramble {scrambed} for a reward! (reply with the unscrambed word)");
                }

                if (command == "coins") {
                    user = await Context.Users.FirstOrDefaultAsync(x => x.UserId == message.Author_Id && x.PlanetId == message.Planet_Id);
                    await PostMessage(message.Channel_Id, message.Planet_Id, $"{ClientUser.Nickname}'s coins: {(ulong)user.Coins}");
                }

                if (command == "stats") {
                    if (ops.Count() == 1) {
                        ops.Add("");
                    }
                    switch (ops[1])
                    {
                        case "coins":
                            List<Stat> stats = await Task.Run(() => Context.Stats.Where(x => x.PlanetId == message.Planet_Id).OrderByDescending(x => x.Time).Take(8).ToList());
                            List<int> data = new List<int>();
                            foreach (Stat stat in stats) {
                                data.Add((int)stat.NewCoins);
                            }
                            await PostGraph(message.Channel_Id, message.Planet_Id, data, "coins");
                            break;
                        case "messages":
                            stats = await Task.Run(() => Context.Stats.Where(x => x.PlanetId == message.Planet_Id).OrderByDescending(x => x.Time).Take(8).ToList());
                            data = new List<int>();
                            foreach (Stat stat in stats) {
                                data.Add((int)stat.MessagesSent);
                            }
                            await PostGraph(message.Channel_Id, message.Planet_Id, data, "messages");
                            break;
                        default:
                            await PostMessage(message.Channel_Id, message.Planet_Id, $"Available Commands: /stats messages, /stats coins");
                            break;
                    }
                }

                if (command == "hourly") {
                    user = await Context.Users.FirstOrDefaultAsync(x => x.UserId == message.Author_Id && x.PlanetId == message.Planet_Id);
                    int minutesleft = (user.LastHourly.AddHours(1).Subtract(DateTime.UtcNow)).Minutes;
                    if (minutesleft <= 0) {
                        double payout = (double)rnd.Next(30,50);
                        user.Coins += payout;
                        user.LastHourly = DateTime.UtcNow;
                        await Context.AddStat("Coins", payout, message.Planet_Id, Context);
                        await Context.SaveChangesAsync();
                        await PostMessage(message.Channel_Id, message.Planet_Id, $"Your got {payout} coins!");
                    }
                    else {
                        await PostMessage(message.Channel_Id, message.Planet_Id, $"You must wait {minutesleft} minutes before you can get another payout");
                        return;
                    }
                }

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


                if (command == "leaderboard") {
                    List<User> users = await Task.Run(() => Context.Users.Where(x => x.PlanetId == message.Planet_Id).OrderByDescending(x => x.Xp).Take(10).ToList());
                    string content = "| nickname | xp |\n| :- | :-\n";
                    foreach(User USER in users) {
                        ClientPlanetUser clientuser = await USER.GetAuthor(message.Planet_Id);
                        content += $"{clientuser.Nickname} | {(ulong)USER.Xp} xp\n";
                    }
                    await PostMessage(message.Channel_Id, message.Planet_Id, content);
                }

                if (command == "richest") {
                    List<User> users = await Task.Run(() => Context.Users.Where(x => x.PlanetId == message.Planet_Id).OrderByDescending(x => x.Coins).Take(10).ToList());
                    string content = "| nickname | coins |\n| :- | :-\n";
                    foreach(User USER in users) {
                        ClientPlanetUser clientuser = await USER.GetAuthor(message.Planet_Id);
                        content += $"{clientuser.Nickname} | {(ulong)USER.Coins} coins\n";
                    }
                    await PostMessage(message.Channel_Id, message.Planet_Id, content);
                }

                if (command == "testgraph") {
                    List<int> data = new List<int>();
                    data.Add(163);
                    data.Add(308);
                    data.Add(343);
                    data.Add(378);
                    data.Add(436);
                    data.Add(454);
                    data.Add(455);
                    data.Add(460);
                    data.Add(516);
                    data.Add(594);
                    await PostGraph(message.Channel_Id, message.Planet_Id, data, "Messages");
                }

                if (command == "userid") {
                    await PostMessage(message.Channel_Id, message.Planet_Id, $"Your UserId is {message.Author_Id}");
                }

                if (command == "planetid") {
                    await PostMessage(message.Channel_Id, message.Planet_Id, $"This Planet's Id is {message.Planet_Id}");
                }
                
                if (command == "channelid") {
                    await PostMessage(message.Channel_Id, message.Planet_Id, $"This Channel's id is {message.Channel_Id}");
                }

                if (command == "memberid") {
                    await PostMessage(message.Channel_Id, message.Planet_Id, $"Your MemberId is {ClientUser.Id}");
                }

                if (command == "gamble") {
                    if (ops.Count == 1) {
                        ops.Add("");
                    }
                    switch (ops[1])
                    {

                        case "Red": case "Blue": case "Green": case "Black":
                            if (ops.Count < 3) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, "Command Useage: /gamble <color> <bet>");
                                break;
                            }
                            User User = await Context.Users.FirstOrDefaultAsync(x => x.UserId == message.Author_Id && x.PlanetId == message.Planet_Id);
                            double bet = (double)ulong.Parse(ops[2]);
                            if (user.Coins < (double)bet) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, "Bet must not be above your coins!");
                                break;
                            }
                            if (bet == 0) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, "Bet must not be 0!");
                                break;
                            }
                            ulong choice = 0;
                            switch (ops[1])
                            {
                                case "Red":
                                    choice = 0;
                                    break;
                                case "Blue":
                                    choice = 1;
                                    break;
                                case "Green":
                                    choice = 2;
                                    break;
                                case "Black":
                                    choice = 3;
                                    break;
                                default:
                                    choice = 0;
                                    break;
                            }
                            ulong Winner = 0;
                            int num = rnd.Next(1, 101);
                            double muit = 3.2;
                            string colorwon = "";
                            switch (num)
                            {
                                case <= 35:
                                    Winner = 0;
                                    colorwon = "Red";
                                    break;
                                case <= 70:
                                    Winner = 1;
                                    colorwon = "Blue";
                                    break;
                                case <= 90:
                                    muit = 6.5;
                                    Winner = 2;
                                    colorwon = "Green";
                                    break;
                                default:
                                    Winner = 3;
                                    muit = 15;
                                    colorwon = "Black";
                                    break;
                            }
                            double amount = bet*muit;
                            User.Coins -= bet;
                            List<string> data = new List<string>();
                            data.Add($"You picked {ops[1]}");
                            data.Add($"The color drawn is {colorwon}");
                            if (Winner == choice) {
                                User.Coins += amount;
                                data.Add($"You won {Math.Round(amount-bet)} coins!");
                                await Context.AddStat("Coins", amount-bet, message.Planet_Id, Context);
                            }
                            else {
                                data.Add($"You did not win.");
                                await Context.AddStat("Coins", 0-bet, message.Planet_Id, Context);
                            }
                            
                            

                            Task task = Task.Run(async () => SlowMessages( data,message.Channel_Id, message.Planet_Id));

                            await Context.SaveChangesAsync();

                            break;

                        default:
                            string content = "| Color | Chance | Reward   |\n|-------|--------|----------|\n| Red   | 35%    | 3.2x bet |\n| Blue  | 35%    | 3.2x bet |\n| Green | 20%    | 6.5x bet   |\n| Black | 10%     | 15x bet  |";
                            await PostMessage(message.Channel_Id, message.Planet_Id, content);
                            break;
                    }
                }

                if (command == "charity") {
                    if (ops.Count == 1) {
                        await PostMessage(message.Channel_Id, message.Planet_Id, "Command Format: /charity <amount to give>");
                        return;
                    }
                    int amount = int.Parse(ops[1]);
                    if (amount > user.Coins) {
                        await PostMessage(message.Channel_Id, message.Planet_Id, "You can not donate more coins than you currently have!");
                        return;
                    }
                    user.Coins -= amount;
                    double CoinsPer = amount/Context.Users.Count();
                    foreach(User USER in Context.Users) {
                        USER.Coins += CoinsPer;
                    }
                    await PostMessage(message.Channel_Id, message.Planet_Id, $"Gave {Math.Round((decimal)CoinsPer, 2)} coins to every user!");
                    await Context.SaveChangesAsync();
                    
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

                if (command == "dice") {
                    if (ops.Count == 1) {
                        await PostMessage(message.Channel_Id, message.Planet_Id, "Command Format: /dice <bet>");
                        return;
                    }
                    else {
                        ulong bet = ulong.Parse(ops[1]);
                        if (user.Coins < (double)bet) {
                            await PostMessage(message.Channel_Id, message.Planet_Id, "Bet must not be above your coins!");
                            return;
                        }
                        int usernum1 = rnd.Next(1, 6);
                        int usernum2 = rnd.Next(1, 6);
                        int opnum1 = rnd.Next(1, 6);
                        int opnum2 = rnd.Next(1, 6);
                        List<string> data = new List<string>();
                        data.Add($"You throw the dice");
                        data.Add($"You get **{usernum1}** and **{usernum2}**");
                        data.Add($"Your opponent throws their dice, and gets **{opnum1}** and **{opnum2}**");

                        // check for a tie

                        if (usernum1+usernum2 == opnum1+opnum2) {
                            data.Add($"It's a tie");
                        }
                        else {

                            // user won
                            if (usernum1+usernum2 > opnum1+opnum2) {
                                data.Add($"You won {bet} coins!");
                                user.Coins += bet;
                                await Context.AddStat("Coins", bet, message.Planet_Id, Context);
                            }
                            else {
                                data.Add($"You lost {bet} coins.");
                                user.Coins -= bet;
                                await Context.AddStat("Coins", 0-bet, message.Planet_Id, Context);
                            }
                        }

                        await Context.SaveChangesAsync();

                        Task task = Task.Run(async () => SlowMessages( data,message.Channel_Id, message.Planet_Id));
                    }
                    
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

                            await ClientUser.GiveRole(reward.RoleName);

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

        static async Task PostGraph(ulong channelid, ulong planetid, List<int> data, string dataname) {
            string content = "";
            int maxvalue = data.Max();

            // make sure that the max-y is 10

            double muit = 10/(double)maxvalue;

            List<int> newdata = new List<int>();

            foreach(int num in data) {
                double n = (double)num*muit;
                newdata.Add((int)n);
            }

            data = newdata;

            List<string> rows = new List<string>();
            for(int i = 0; i < 10; i++) {
                rows.Add("");
            }
            string space = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            foreach(int num in data) {
                for(int i = 0; i < num;i++) {
                    rows[i] += "⬜";
                    Console.WriteLine($"box: {i}");
                }
                for(int i = num; i < 10;i++) {
                    rows[i] += space;
                    Console.WriteLine(i);
                }
            }

            // build the bar graph

            rows.Reverse();
            foreach(string row in rows) {
                content += $"{row}\n";
            }

            // build the x-axis labels

            content += " ";

            for(int i = data.Count(); i > 0;i--) {
                content += $"{i}h&nbsp;";
            }

            content += "\n";

            // build the how much does 1 box equal

            content += $"⬜ = {(int)maxvalue/10} {dataname}";
            await PostMessage(channelid, planetid, content);
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

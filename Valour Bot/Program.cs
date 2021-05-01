using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using SpookVooper.Api;
using SpookVooper.Api.Economy;
using SpookVooper.Api.Entities;
using PopeAI.Database;
using Microsoft.EntityFrameworkCore;
using Cryptography.ECDSA;
using System.Text.Encodings;
using System.Security.Cryptography;
using System.Text;
using System.Numerics;
using Secp256k1Net;
using System.Net.Http;
using PopeAI.Models;

namespace PopeAI
{
    class Program
    {

        static HttpClient client = new System.Net.Http.HttpClient();

        static Dictionary<string, DateTime> timesincelastmessage = new Dictionary<string, DateTime>();

        static Dictionary<String, ulong> MessagesPerMinuteInARow = new Dictionary<string, ulong>();

        static PopeAIDB Context = new PopeAIDB(PopeAIDB.DBOptions);

        static ulong TotalMessages = 0;

        static List<List<ulong>> MessageCount {get; set;}

        static Random rnd = new Random();

        static List<Planet> planets {get; set;}


        public static byte[] StringToByteArray(string hex) {
            return Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();
        }

        static async Task Main(string[] args)
        {
            await Client.hubConnection.StartAsync();

            //Get all the planets that we are in

            string json = await client.GetStringAsync($"https://valour.gg/Planet/GetPlanetMembership?user_id={Client.config.BotId}&token={Client.config.authkey}");

            TaskResult<List<Planet>> result = JsonConvert.DeserializeObject<TaskResult<List<Planet>>>(json);

            planets = result.Data;

            foreach(Planet planet in planets) {
                await Client.hubConnection.SendAsync("JoinPlanet", planet.Id, Client.config.authkey);
                foreach(Channel channel in await planet.GetChannelsAsync()) {
                    await Client.hubConnection.SendAsync("JoinChannel", channel.Id, Client.config.authkey);
                }
            }
            Client.hubConnection.On<string>("Relay", OnMessageRecieve);
            //await Channel.CreateChannel("Coding", 735703679107073, 735703679107072);
            Console.WriteLine("Hello World!");

            Task task = Task.Run(async () => UpdateHourly( Context));

            while (true)
            {
                Console.ReadLine();
            }

        }

        static async Task UpdateHourly(PopeAIDB Context) {
            while (true) {
                await Context.UpdateStats(Context);
                await Context.UpdateRoleIncomes(planets, false, Context);
                await Task.Delay(60000);
            }
        }

        static async Task OnMessageRecieve(string json)
        {
            ClientPlanetMessage message = Newtonsoft.Json.JsonConvert.DeserializeObject<ClientPlanetMessage>(json);

            string dictkey = $"{message.Author_Id}-{message.Planet_Id}";

            bool IsVaild = false;

            ClientPlanetUser ClientUser = null;
            ClientUser = await message.GetAuthorAsync();

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

            if (message.Author_Id == ulong.Parse(Client.config.BotId)) {
                return;
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

            if (IsVaild) {
                user.Xp += xp;
                user.Coins += xp*2;
                await Context.AddStat("Coins", (double)xp*2, message.Planet_Id, Context);
                await Context.SaveChangesAsync();
            
            }

            if (message.Content.Substring(0,1) == Client.config.CommandSign) {
                string command = message.Content.Split(" ")[0];
                command = command.Replace("\n", "");
                List<string> ops = message.Content.Split(" ").ToList();
                command = command.Replace(Client.config.CommandSign,"");

                if (command == "help") {
                    int skip = 0;
                    if (ops.Count() == 2) {
                        skip = int.Parse(ops[1]);
                        skip *= 10;
                    }
                    string content = "| command |\n| :-: |\n";
                    foreach (Help help in Context.Helps.Skip(skip).Take(10)) {
                        content += $"| {help.message} |\n";
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

                if (command == "coins") {
                    user = await Context.Users.FirstOrDefaultAsync(x => x.UserId == message.Author_Id && x.PlanetId == message.Planet_Id);
                    await PostMessage(message.Channel_Id, message.Planet_Id, $"{ClientUser.Nickname}'s coins: {(ulong)user.Coins}");
                }

                if (command == "roll") {
                    if (ops.Count() < 3) {
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
                    if (ops.Count() == 1) {
                        ops.Add("");
                    }
                    switch (ops[1])
                    {

                        case "Red": case "Blue": case "Green": case "Black":
                            if (ops.Count() < 3) {
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
                    if (ops.Count() == 1) {
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
                    if (ops.Count() == 1) {
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
                    if (ops.Count() == 1) {
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

                if (command == "roleincome") {
                    if (ops.Count() == 1) {
                        ops.Add("");
                    }
                    switch (ops[1])
                    {
                        case "set":

                            if (await ClientUser.IsOwner() != true) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, $"Only the owner of this server can use this command!");
                                break;
                            }

                            if (ops.Count() < 3) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, "Command Format: /roleincome set <hourly income/cost> <rolename>");
                                break;
                            }

                            string rolename = message.Content.Replace($"{Client.config.CommandSign}roleincome set {ops[2]} ", "");

                            RoleIncome roleincome = await Context.RoleIncomes.FirstOrDefaultAsync(x => x.RoleName == rolename && x.PlanetId == message.Planet_Id);

                            if (roleincome == null) {

                                ClientRole clientrole = await planets.FirstOrDefault(x => x.Id == message.Planet_Id).GetRoleAsync(rolename);

                                if (clientrole == null) {
                                    await PostMessage(message.Channel_Id, message.Planet_Id, $"Could not find role {rolename}!");
                                    break;
                                }

                                roleincome = new RoleIncome();

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
                        
                            rolename = message.Content.Replace($"{Client.config.CommandSign}roleincome ", "");

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
                    if (ops.Count() == 1) {
                        ops.Add("");
                    }
                    switch (ops[1])
                    {
                        case "addrole":

                            if (ops.Count() < 3) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, "Command Format: /shop addrole <cost> <rolename>");
                                break;
                            }

                            if (await ClientUser.IsOwner() != true) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, $"Only the owner of this server can use this command!");
                                break;
                            }

                            string rolename = message.Content.Replace($"{Client.config.CommandSign}shop addrole {ops[2]} ", "");

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
                            if (ops.Count() < 2) {
                                await PostMessage(message.Channel_Id, message.Planet_Id, "Command Format: /shop buy <rolename>");
                                break;
                            }

                            rolename = message.Content.Replace($"{Client.config.CommandSign}shop buy ", "");

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
            for(int i = 0; i < data.Count(); i++) {
                rows.Add("");
            }
            string space = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            foreach(int num in data) {
                for(int i = 0; i < num;i++) {
                    rows[i] += "⬜";
                }
                for(int i = num; i < data.Count();i++) {
                    rows[i] += space;
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

        static async Task PostMessage(ulong channelid, ulong planetid, string msg)
        {
            ClientPlanetMessage message = new ClientPlanetMessage()
            {
                Channel_Id = channelid,
                Content = msg,
                TimeSent = DateTime.UtcNow,
                Author_Id = ulong.Parse(Client.config.BotId),
                Planet_Id = planetid
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(message);

            Console.WriteLine("SEND: \n" + json);

            HttpResponseMessage httpresponse = await client.PostAsJsonAsync<ClientPlanetMessage>($"https://valour.gg/Channel/PostMessage?token={Client.config.authkey}", message);

            TaskResult response = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskResult>(await httpresponse.Content.ReadAsStringAsync());

            Console.WriteLine("Sending Message!");

            Console.WriteLine(response.ToString());
        }
    }
}

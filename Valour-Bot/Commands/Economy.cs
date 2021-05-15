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
using PopeAI;

/*
gamble
hourly
richest
coins
charity
eco
forcerolepayout
lottery
roleincome
shop
 */

namespace PopeAI.Commands.Economy
{
    public class Economy : CommandModuleBase
    {
        Random rnd = new Random();
        [Command("hourly")]
        [Alias("h")]
        public async Task hourly(CommandContext ctx)
        {
            User DBUser = await Client.DBContext.Users.FirstOrDefaultAsync(x => x.UserId ==  ctx.Message.Author_Id && x.PlanetId ==  ctx.Message.Planet_Id);
            int minutesleft = (DBUser.LastHourly.AddHours(1).Subtract(DateTime.UtcNow)).Minutes;
            if (minutesleft <= 0) {
                // check if the member has a daily task 
                DailyTask task = await Client.DBContext.DailyTasks.FirstOrDefaultAsync(x => x.MemberId == ctx.Member.Id && x.TaskType == "Hourly Claims");
                if (task != null) {
                    if (task.Done < task.Goal) {
                        task.Done += 1;
                        if (task.Done == task.Goal) {
                            DBUser.Coins += task.Reward;
                            await Client.DBContext.AddStat("Coins", task.Reward, ctx.Message.Planet_Id, Client.DBContext);
                            await ctx.ReplyAsync($"Your {task.TaskType} daily task is done! You get {task.Reward} coins.");
                        }
                    }
                }
                double payout = (double)rnd.Next(30,50);
                DBUser.Coins += payout;
                DBUser.LastHourly = DateTime.UtcNow;
                await Client.DBContext.AddStat("Coins", payout,  ctx.Message.Planet_Id, Client.DBContext);
                await Client.DBContext.SaveChangesAsync();
                await ctx.ReplyAsync($"Your got {payout} coins!");
            }
            else {
                await ctx.ReplyAsync($"You must wait {minutesleft} minutes before you can get another payout");
                return;
            }
        }
        [Command("richest")]
        [Alias("r")]
        public async Task richest(CommandContext ctx)
        {
            List<User> DBUsers = await Task.Run(() => Client.DBContext.Users.Where(x => x.PlanetId ==  ctx.Message.Planet_Id).OrderByDescending(x => x.Coins).Take(10).ToList());
            string content = "| nickname | coins |\n| :- | :-\n";
            foreach(User user in DBUsers) {
                PlanetMember clientuser = await user.GetAuthor();
                content += $"{clientuser.Nickname} | {(ulong)user.Coins} coins\n";
            }
            await ctx.ReplyAsync(content);
        }
        [Command("coins")]
        [Alias("c")]
        public async Task coins(CommandContext ctx)
        {
            User DBUser = await Client.DBContext.Users.FirstOrDefaultAsync(x => x.UserId ==  ctx.Message.Author_Id && x.PlanetId ==  ctx.Message.Planet_Id);
            await ctx.ReplyAsync($"{ctx.Member.Nickname}'s coins: {(ulong)DBUser.Coins}");
        }

        [Command("charity")]
        [Alias("donate")]
        public async Task Charity(CommandContext ctx, double amount)
        {
            User DBUser = await Client.DBContext.Users.FirstOrDefaultAsync(x => x.UserId ==  ctx.Message.Author_Id && x.PlanetId ==  ctx.Message.Planet_Id);
            if (amount > DBUser.Coins) {
                await ctx.ReplyAsync("You can not donate more coins than you currently have!");
                return;
            }
            if (amount <= 0) {
                await ctx.ReplyAsync("Amount must be above 0!");
                return;
            }
            DBUser.Coins -= amount;
            double CoinsPer = amount/Client.DBContext.Users.Count();
            foreach(User user in Client.DBContext.Users) {
                user.Coins += CoinsPer;
            }
            await ctx.ReplyAsync($"Gave {Math.Round((decimal)CoinsPer, 2)} coins to every user!");
            await Client.DBContext.SaveChangesAsync();
        }

        [Command("charity")]
        [Alias("donate")]
        public async Task Charity(CommandContext ctx)
        {
            await ctx.ReplyAsync("Correct usage: /charity <amount to give>");
        }

        [Command("dice")]
        public async Task Dice(CommandContext ctx, double bet)
        {
            User DBUser = await Client.DBContext.Users.FirstOrDefaultAsync(x => x.UserId ==  ctx.Message.Author_Id && x.PlanetId ==  ctx.Message.Planet_Id);
            if (DBUser.Coins < bet) {
                await ctx.ReplyAsync("Bet must not be above your coins!");
                return;
            }

            // check if the member has a daily task 
            DailyTask task = await Client.DBContext.DailyTasks.FirstOrDefaultAsync(x => x.MemberId == ctx.Member.Id && x.TaskType == "Dice Games Played");
            if (task != null) {
                if (task.Done < task.Goal) {
                    task.Done += 1;
                    if (task.Done == task.Goal) {
                        DBUser.Coins += task.Reward;
                        await Client.DBContext.AddStat("Coins", task.Reward, ctx.Message.Planet_Id, Client.DBContext);
                        await ctx.ReplyAsync($"Your {task.TaskType} daily task is done! You get {task.Reward} coins.");
                    }
                    await Client.DBContext.SaveChangesAsync();
                }
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
                    DBUser.Coins += bet;
                    await Client.DBContext.AddStat("Coins", bet,  ctx.Message.Planet_Id, Client.DBContext);
                }
                else {
                    data.Add($"You lost {bet} coins.");
                    DBUser.Coins -= bet;
                    await Client.DBContext.AddStat("Coins", 0-bet,  ctx.Message.Planet_Id, Client.DBContext);
                }
            }

            await Client.DBContext.SaveChangesAsync();

            ctx.ReplyWithMessagesAsync(1750, data);
        } 

        [Command("dice")]
        public async Task Dice(CommandContext ctx)
        {
            await ctx.ReplyAsync("Correct Useage: /dice <bet>");
        }

        [Command("gamble")]
        public async Task Gamble(CommandContext ctx)
        {
            string content = "| Color | Chance | Reward   |\n|-------|--------|----------|\n| Red   | 35%    | 3.2x bet |\n| Blue  | 35%    | 3.2x bet |\n| Green | 20%    | 6.5x bet   |\n| Black | 10%     | 15x bet  |";
            await ctx.ReplyAsync(content);
        }
        
        [Command("gamble")]
        public async Task Gamble(CommandContext ctx, string t)
        {
            await ctx.ReplyAsync("Command Useage: /gamble <color> <bet>");
        }

        [Command("gamble")]
        public async Task Gamble(CommandContext ctx, string color, double bet)
        {
            if (color != "Red" && color != "Blue" && color != "Green" && color != "Black") {
                await ctx.ReplyAsync("The color must be Red, Blue, Green, or Black");
                return;
            }

            User DBUser = await Client.DBContext.Users.FirstOrDefaultAsync(x => x.UserId ==  ctx.Message.Author_Id && x.PlanetId ==  ctx.Message.Planet_Id);
            if (DBUser.Coins < bet) {
                await ctx.ReplyAsync("Bet must not be above your coins!");
                return;
            }
            if (bet == 0) {
                await ctx.ReplyAsync("Bet must not be 0!");
                return;
            }
            ulong choice = 0;
            switch (color)
            {
                case "Blue":
                    choice = 1;
                    break;
                case "Green":
                    choice = 2;
                    break;
                case "Black":
                    choice = 3;
                    break;
            }
            ulong Winner = 0;
            int num = rnd.Next(1, 101);
            double muit = 3.2;
            string colorwon = "Red";
            switch (num)
            {
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
            DBUser.Coins -= bet;
            List<string> data = new List<string>();
            data.Add($"You picked {color}");
            data.Add($"The color drawn is {colorwon}");
            if (Winner == choice) {
                DBUser.Coins += amount;
                data.Add($"You won {Math.Round(amount-bet)} coins!");
                await Client.DBContext.AddStat("Coins", amount-bet,  ctx.Message.Planet_Id, Client.DBContext);
            }
            else {
                data.Add($"You did not win.");
                await Client.DBContext.AddStat("Coins", 0-bet,  ctx.Message.Planet_Id, Client.DBContext);
            }

            await Client.DBContext.SaveChangesAsync();

             ctx.ReplyWithMessagesAsync(1750, data);
        }

        [Group("roleincome")]
        public class RoleIncomeGroup : CommandModuleBase
        {
            PopeAIDB DBContext = new PopeAIDB(PopeAIDB.DBOptions);

            [Command("set")]
            public async Task SetAsync(CommandContext ctx, double rate, [Remainder] string rolename)
            {
                if (await ctx.Member.IsOwner() != true) {
                    await ctx.ReplyAsync($"Only the owner of this server can use this command!");
                    return;
                }

                RoleIncomes roleincome = await Client.DBContext.RoleIncomes.FirstOrDefaultAsync(x => x.RoleName == rolename && x.PlanetId == ctx.Message.Planet_Id);

                if (roleincome == null) {

                    PlanetRole role = await (await Cache.GetPlanet(ctx.Planet.Id)).GetRole(rolename);

                    if (role == null) {
                        await ctx.ReplyAsync($"Could not find role {rolename}!");
                        return;
                    }

                    roleincome = new RoleIncomes();

                    roleincome.Income = rate;
                    roleincome.RoleId = role.Id;
                    roleincome.PlanetId = ctx.Message.Planet_Id;
                    roleincome.RoleName = role.Name;
                    roleincome.LastPaidOut = DateTime.UtcNow;

                    Client.DBContext.RoleIncomes.Add(roleincome);

                    Client.DBContext.SaveChanges();

                    await ctx.ReplyAsync($"Set {rolename}'s hourly income/cost to {roleincome.Income} coins!");
                }

                else {
                    roleincome.Income = rate;
                    await Client.DBContext.SaveChangesAsync();
                    await ctx.ReplyAsync($"Set {rolename}'s hourly income/cost to {roleincome.Income} coins!");
                }
            }
            [Command("")]
            public async Task ViewAsync(CommandContext ctx, [Remainder] string rolename)
            {
                PlanetRole role = await (await Cache.GetPlanet(ctx.Planet.Id)).GetRole(rolename);

                if (role == null) {
                    await ctx.ReplyAsync($"Could not find role {rolename}");
                    return;
                }

                RoleIncomes roleincome = await Client.DBContext.RoleIncomes.FirstOrDefaultAsync(x => x.RoleName == rolename && x.PlanetId == ctx.Message.Planet_Id);

                if (roleincome == null) {
                    await ctx.ReplyAsync($"Hourly Income/Cost has not been set for role {rolename}");
                    return;
                }

                await ctx.ReplyAsync($"Hourly Income/Cost for {rolename} is {roleincome.Income} coins");
            }
            // add way to view roleincome as table
        }


    }
}
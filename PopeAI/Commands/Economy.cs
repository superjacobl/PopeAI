namespace PopeAI.Commands.Economy;

public class Economy : CommandModuleBase
{
    Random rnd = new Random();
    [Command("hourly")]
    [Alias("h")]
    public async Task hourly(CommandContext ctx)
    {
        DBUser user = await Client.DBContext.Users.FindAsync(ctx.Member.Id);
        int minutesleft = (user.LastHourly.AddHours(1).Subtract(DateTime.UtcNow)).Minutes;
        if (minutesleft <= 0) {
            await DailyTaskManager.DidTask(DailyTaskType.Hourly_Claims, ctx.Member.Id, ctx);

            double payout = rnd.Next(30, 50);
            user.Coins += payout;
            user.LastHourly = DateTime.UtcNow;

            await StatManager.AddStat(CurrentStatType.Coins, (int)payout,  ctx.Planet.Id);

            await Client.DBContext.SaveChangesAsync();
            await ctx.ReplyAsync($"You got {payout} coins!");
        }
        else {
            await ctx.ReplyAsync($"You must wait {minutesleft} minutes before you can get another payout!");
        }
    }
    [Command("richest")]
    [Alias("r")]
    public async Task richest(CommandContext ctx)
    {
        List<DBUser> DBUsers = await Task.Run(() => Client.DBContext.Users.Where(x => x.PlanetId ==  ctx.Planet.Id).OrderByDescending(x => x.Coins).Take(10).ToList());
        EmbedBuilder embed = new EmbedBuilder();
        EmbedPageBuilder page = new EmbedPageBuilder();
        int i = 1;
        foreach (DBUser USER in DBUsers)
        {
            PlanetMember member = await PlanetMember.FindAsync(ctx.Planet.Id, USER.UserId);
            //page.AddText($"({i}) {member.Nickname}",  $"{(ulong)USER.Xp}xp");
            page.AddText(text:$"({i}) {member.Nickname} - {(ulong)USER.Coins} coins");
            //content += $"{member.Nickname} | {(ulong)USER.Xp} xp\n";
            i += 1;
            if (page.Items.Count() > 10) {
                embed.AddPage(page);
                page = new EmbedPageBuilder();
            }
        }
        embed.AddPage(page);
        await ctx.ReplyAsync(embed);
    }
    [Command("coins")]
    [Alias("c")]
    public async Task coins(CommandContext ctx)
    {
        DBUser DBUser = await Client.DBContext.Users.FindAsync(ctx.Member.Id);
        await ctx.ReplyAsync($"{ctx.Member.Nickname}'s coins: {(ulong)DBUser.Coins}");
    }

    [Command("pay")]
    [Alias("send")]
    public async Task PayAsync(CommandContext ctx, double amount, PlanetMember member)
    {
        DBUser DBUser = await Client.DBContext.Users.FindAsync(ctx.Member.Id);
        DBUser ToUser = await Client.DBContext.Users.FindAsync(member.Id);
        if (amount > DBUser.Coins) {
            await ctx.ReplyAsync("You can not send more coins than you currently have!");
            return;
        }
        if (amount <= 0) {
            await ctx.ReplyAsync("Amount must be above 0!");
            return;
        }
        DBUser.Coins -= amount;
        ToUser.Coins += amount;
        await ctx.ReplyAsync($"Sent {amount} coins to {member.Nickname}!");
        await Client.DBContext.SaveChangesAsync();
    }

    [Command("charity")]
    [Alias("donate")]
    public async Task Charity(CommandContext ctx, double amount)
    {
        DBUser DBUser = await Client.DBContext.Users.FindAsync(ctx.Member.Id);
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
        foreach(DBUser user in Client.DBContext.Users) {
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
    public async Task Dice(CommandContext ctx, int bet)
    {
        DBUser user = await Client.DBContext.Users.FindAsync(ctx.Member.Id);
        if (user.Coins < bet) {
            await ctx.ReplyAsync("Bet must not be above your coins!");
            return;
        }

        await DailyTaskManager.DidTask(DailyTaskType.Dice_Games_Played, ctx.Member.Id, ctx);

        int usernum1 = rnd.Next(1, 6);
        int usernum2 = rnd.Next(1, 6);
        int opnum1 = rnd.Next(1, 6);
        int opnum2 = rnd.Next(1, 6);
        List<string> data = new();
        data.Add($"You throw the dice");
        data.Add($"You get **{usernum1}** and **{usernum2}**");
        data.Add($"Your opponent throws their dice, and gets **{opnum1}** and **{opnum2}**");

        // check for a tie

        if (usernum1+usernum2 == opnum1+opnum2) {
            data.Add($"It's a tie");
            user.Coins -= (double)bet;
        }
        else {

            // user won
            if (usernum1+usernum2 > opnum1+opnum2) {
                data.Add($"You won {bet} coins!");
                user.Coins += (double)bet;
                await StatManager.AddStat(CurrentStatType.Coins, bet,  ctx.Planet.Id);
            }
            else {
                data.Add($"You lost {bet} coins.");
                user.Coins -= (double)bet;
                await StatManager.AddStat(CurrentStatType.Coins, 0-bet, ctx.Planet.Id);
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
        string content = "| Color | Chance | Reward   |\n|-------|--------|----------|\n| Red   | 35%    | 2.86x bet |\n| Blue  | 35%    | 2.86x bet |\n| Green | 20%    | 5x bet   |\n| Black | 10%     | 10x bet  |";
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

        var user = DBCache.Get<DBUser>(ctx.Member.Id);
        if (user.Coins < bet) {
            await ctx.ReplyAsync("Bet must not be above your coins!");
            return;
        }
        if (bet == 0) {
            await ctx.ReplyAsync("Bet must not be 0!");
            return;
        }
        await DailyTaskManager.DidTask(DailyTaskType.Gamble_Games_Played, ctx.Member.Id, ctx);

        ulong choice = color switch
        {
            "Blue" => 1,
            "Green" => 2,
            "Black" => 3,
            "Red" => 0,
            _ => throw new NotImplementedException()
        };
        ulong Winner = 0;
        int num = rnd.Next(1, 101);
        double muit = 2.86;
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
                muit = 5;
                Winner = 2;
                colorwon = "Green";
                break;
            default:
                Winner = 3;
                muit = 10;
                colorwon = "Black";
                break;
        }
        double amount = bet*muit*0.99;
        user.Coins -= bet;
        List<string> data = new();
        data.Add($"You picked {color}");
        data.Add($"The color drawn is {colorwon}");
        if (Winner == choice)
        {
            user.Coins += amount;
            data.Add($"You won {Math.Round(amount - bet)} coins!");
            await StatManager.AddStat(CurrentStatType.Coins, (int)amount - (int)bet, ctx.Planet.Id);
        }
        else
        {
            data.Add($"You did not win.");
            await StatManager.AddStat(CurrentStatType.Coins, 0-(int)bet, ctx.Planet.Id);
        }

        await ctx.ReplyWithMessagesAsync(1750, data);
    }
    
    // roleincome & lotteries will be updated eventually

    /*
    [Group("roleincome")]
    public class RoleIncomeGroup : CommandModuleBase
    {
        PopeAIDB DBContext = new PopeAIDB(PopeAIDB.DBOptions);

        [Command("set")]
        public async Task SetAsync(CommandContext ctx, int rate, [Remainder] string rolename)
        {
            if (ctx.Member.UserId != ctx.Planet.Owner_Id) {
                await ctx.ReplyAsync($"Only the owner of this server can use this command!");
                return;
            }

            RoleIncomes roleincome = await Client.DBContext.RoleIncomes.FirstOrDefaultAsync(x => x.RoleName == rolename && x.PlanetId == ctx.Planet.Id);

            if (roleincome == null) {

                Planet planet = await Planet.FindAsync(ctx.Planet.Id);

                PlanetRole role = (await planet.GetRolesAsync()).FirstOrDefault(x => x.Name == rolename);

                if (role == null) {
                    await ctx.ReplyAsync($"Could not find role {rolename}!");
                    return;
                }

                roleincome = new RoleIncomes();

                roleincome.Income = rate;
                roleincome.RoleId = role.Id;
                roleincome.PlanetId = ctx.Planet.Id;
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
            PlanetRole role = ((NetPlanet)(await NetPlanet.FindAsync(ctx.Planet.Id))).GetRole(rolename);

            if (role == null) {
                await ctx.ReplyAsync($"Could not find role {rolename}");
                return;
            }

            RoleIncomes roleincome = await Client.DBContext.RoleIncomes.FirstOrDefaultAsync(x => x.RoleName == rolename && x.PlanetId == ctx.Planet.Id);

            if (roleincome == null) {
                await ctx.ReplyAsync($"Hourly Income/Cost has not been set for role {rolename}");
                return;
            }

            await ctx.ReplyAsync($"Hourly Income/Cost for {rolename} is {roleincome.Income} coins");
        }
        // add way to view roleincome as table
    }
*/

}
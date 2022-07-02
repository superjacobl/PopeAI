namespace PopeAI.Commands.Economy;

public class Economy : CommandModuleBase
{
    Random rnd = new Random();

    [Command("hourly")]
    [Alias("h")]
    public async Task Hourly(CommandContext ctx)
    {
        var user = await DBUser.GetAsync(ctx.Member.Id);
        int minutesleft = (user.LastHourly.AddHours(1).Subtract(DateTime.UtcNow)).Minutes;
        if (minutesleft <= 0) {
            await DailyTaskManager.DidTask(DailyTaskType.Hourly_Claims, ctx.Member.Id, ctx);

            double payout = rnd.Next(30, 50);
            user.Coins += payout;
            user.LastHourly = DateTime.UtcNow;

            StatManager.AddStat(CurrentStatType.Coins, (int)payout,  ctx.Planet.Id);
            await ctx.ReplyAsync($"You got {payout} coins!");
        }
        else {
            await ctx.ReplyAsync($"You must wait {minutesleft} minutes before you can get another payout!");
        }
        user.UpdateDB();
    }

    [Command("richest")]
    [Alias("r")]
    public async Task ViewRichest(CommandContext ctx)
    {
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
        List<DBUser> users = dbctx.Users
            .Where(x => x.PlanetId ==  ctx.Planet.Id)
            .OrderByDescending(x => x.Coins)
            .Take(10)
            .ToList();
        EmbedBuilder embed = new EmbedBuilder();
        EmbedPageBuilder page = new EmbedPageBuilder();
        int i = 1;
        foreach (DBUser user in users)
        {
            PlanetMember member = await PlanetMember.FindAsync(ctx.Planet.Id, user.UserId);
            page.AddText(text:$"({i}) {member.Nickname} - {(ulong)user.Coins} coins");
            i += 1;
            if (page.Items.Count > 10) {
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
        var user = await DBUser.GetAsync(ctx.Member.Id);
        ctx.ReplyAsync($"{ctx.Member.Nickname}'s coins: {(ulong)user.Coins}");
        user.UpdateDB();
    }

    [Command("pay")]
    [Alias("send")]
    public async Task PayAsync(CommandContext ctx, double amount, PlanetMember member)
    {
        var fromUser = await DBUser.GetAsync(ctx.Member.Id);
        var toUser = await DBUser.GetAsync(member.Id);
        if (amount > fromUser.Coins) {
            await ctx.ReplyAsync("You can not send more coins than you currently have!");
            return;
        }
        if (amount <= 0) {
            await ctx.ReplyAsync("Amount must be above 0!");
            return;
        }
        fromUser.Coins -= amount;
        toUser.Coins += amount;
        ctx.ReplyAsync($"Sent {amount} coins to {member.Nickname}!");

        fromUser.UpdateDB();
        toUser.UpdateDB();
    }

    [Command("charity")]
    [Alias("donate")]
    public async Task Charity(CommandContext ctx, double amount)
    {
        var user = await DBUser.GetAsync(ctx.Member.Id);
        if (amount > user.Coins) {
            await ctx.ReplyAsync("You can not donate more coins than you currently have!");
            return;
        }
        if (amount <= 0) {
            await ctx.ReplyAsync("Amount must be above 0!");
            return;
        }

        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();

        user.Coins -= amount;

        double CoinsPer = amount / dbctx.Users.Where(x => x.PlanetId == user.PlanetId).Count();
        foreach(var _user in DBCache.GetAll<DBUser>().Where(x => x.PlanetId == user.PlanetId)) {
            user.Coins += CoinsPer;
        }
        ctx.ReplyAsync($"Gave {Math.Round(CoinsPer, 2)} coins to every user!");

        user.UpdateDB();
    }

    [Command("charity")]
    [Alias("donate")]
    public Task Charity(CommandContext ctx)
    {
        return ctx.ReplyAsync("Correct usage: /charity <amount to give>");
    }

    [Command("dice")]
    public async Task Dice(CommandContext ctx, int bet)
    {
        var user = await DBUser.GetAsync(ctx.Member.Id);
        if (user.Coins < bet) {
            await ctx.ReplyAsync("Bet must not be above your coins!");
            return;
        }

        await DailyTaskManager.DidTask(DailyTaskType.Dice_Games_Played, ctx.Member.Id, ctx);

        int usernum1 = rnd.Next(1, 6);
        int usernum2 = rnd.Next(1, 6);
        int opnum1 = rnd.Next(1, 6);
        int opnum2 = rnd.Next(1, 6);
        List<string> data = new()
        {
            $"You throw the dice",
            $"You get **{usernum1}** and **{usernum2}**",
            $"Your opponent throws their dice, and gets **{opnum1}** and **{opnum2}**"
        };

        // check for a tie

        if (usernum1+usernum2 == opnum1+opnum2) {
            data.Add($"It's a tie");
            user.Coins -= bet;
        }
        else {

            // user won
            if (usernum1+usernum2 > opnum1+opnum2) {
                data.Add($"You won {bet} coins!");
                user.Coins += bet;
                StatManager.AddStat(CurrentStatType.Coins, bet,  ctx.Planet.Id);
            }
            else {
                data.Add($"You lost {bet} coins.");
                user.Coins -= bet;
                StatManager.AddStat(CurrentStatType.Coins, 0-bet, ctx.Planet.Id);
            }
        }

        await ctx.ReplyWithMessagesAsync(1750, data);

        user.UpdateDB();
    } 

    [Command("dice")]
    public Task Dice(CommandContext ctx)
    {
        return ctx.ReplyAsync("Correct Useage: /dice <bet>");
    }

    [Command("gamble")]
    public Task Gamble(CommandContext ctx)
    {
        string content = "| Color | Chance | Reward   |\n|-------|--------|----------|\n| Red   | 35%    | 2.86x bet |\n| Blue  | 35%    | 2.86x bet |\n| Green | 20%    | 5x bet   |\n| Black | 10%     | 10x bet  |";
        return ctx.ReplyAsync(content);
    }
    
    [Command("gamble")]
    public Task Gamble(CommandContext ctx, string t)
    {
        return ctx.ReplyAsync("Command Useage: /gamble <color> <bet>");
    }

    [Command("gamble")]
    public async Task Gamble(CommandContext ctx, string color, double bet)
    {
        if (color != "Red" && color != "Blue" && color != "Green" && color != "Black") {
            await ctx.ReplyAsync("The color must be Red, Blue, Green, or Black");
            return;
        }

        var user = await DBUser.GetAsync(ctx.Member.Id);
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
        int num = rnd.Next(1, 101);
        double muit = 2.86;
        string colorwon;
        ulong Winner;
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
            StatManager.AddStat(CurrentStatType.Coins, (int)amount - (int)bet, ctx.Planet.Id);
        }
        else
        {
            data.Add($"You did not win.");
            StatManager.AddStat(CurrentStatType.Coins, 0-(int)bet, ctx.Planet.Id);
        }

        await ctx.ReplyWithMessagesAsync(1750, data);

        user.UpdateDB();
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
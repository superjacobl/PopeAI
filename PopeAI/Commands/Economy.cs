using Valour.Api.Items.Users;

namespace PopeAI.Commands.Economy;

public class Economy : CommandModuleBase
{
    Random rnd = new Random();
    
    [Command("commands")]
    public Task ListCommands(CommandContext ctx) 
    {
        var embed = new EmbedBuilder(EmbedItemPlacementType.RowBased)
            .AddPage("PopeAI Commands")
            .AddRow(new EmbedTextItem("Economy", "/pay, /hourly or /h, /richest or /r, /coins or /c, /dice <bet>, /gamble <color> <bet>, /unscramble or /un"))
            .AddRow(new EmbedTextItem("Xp", "/xp, /info xp, /leaderboard or /lb"))
            .AddRow(new EmbedTextItem("Daily Tasks", "/tasks"))
            .AddRow(new EmbedTextItem("Element Combining", "/suggest <result>, /c or /combine <element 1> <element 2> <optional element 3>, /inv, /vote, /element count, /element mycount"));
        return ctx.ReplyAsync(embed);
    } 

    [Command("hourly")]
    [Alias("h")]
    public async Task Hourly(CommandContext ctx)
    {
        await using var user = await DBUser.GetAsync(ctx.Member.Id, true);
        int minutesleft = (user.LastHourly.AddHours(1).Subtract(DateTime.UtcNow)).Minutes;
        if (minutesleft <= 0) {
            await DailyTaskManager.DidTask(DailyTaskType.Hourly_Claims, ctx.Member.Id, ctx);

            int payout = rnd.Next(30, 50);
            user.Coins += payout;
            user.LastHourly = DateTime.UtcNow;

            await StatManager.AddStat(CurrentStatType.Coins, payout,  ctx.Planet.Id);
            ctx.ReplyAsync($"You got {payout} coins!");
        }
        else {
            ctx.ReplyAsync($"You must wait {minutesleft} minutes before you can get another payout!");
        }
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
        var embed = new EmbedBuilder(EmbedItemPlacementType.RowBased).AddPage("Users ordered by coins");
        int i = 1;
        foreach (DBUser user in users)
        {
            PlanetMember member = await PlanetMember.FindAsync(user.Id, ctx.Planet.Id);
            embed.AddText(text:$"({i}) {member.Nickname} - {(long)user.Coins} coins");
            i += 1;
            if (embed.CurrentPage.Items.Count > 10) {
                embed.AddPage("Users ordered by coins");
            }
        }
        ctx.ReplyAsync(embed);
    }

    [Command("coins")]
    [Alias("c")]
    public async Task coins(CommandContext ctx)
    {
        var user = await DBUser.GetAsync(ctx.Member.Id, true);
        ctx.ReplyAsync($"{ctx.Member.Nickname}'s coins: {user.Coins}");
    }

    [Command("pay")]
    [Alias("send")]
    public async Task PayAsync(CommandContext ctx, PlanetMember member, int amount)
    {
        await using var fromUser = await DBUser.GetAsync(ctx.Member.Id);
        await using var toUser = await DBUser.GetAsync(member.Id);
        
        if (amount > fromUser.Coins) {
            ctx.ReplyAsync("You can not send more coins than you currently have!");
            return;
        }
        if (amount <= 0) {
            ctx.ReplyAsync("Amount must be above 0!");
            return;
        }
        
        fromUser.Coins -= amount;
        toUser.Coins += amount;
        ctx.ReplyAsync($"Sent {amount} coins to {member.Nickname}!");
    }

    [Command("pay")]
    [Alias("send")]
    public async Task PayAsync(CommandContext ctx, int amount, PlanetMember member)
    {
        await using var fromUser = await DBUser.GetAsync(ctx.Member.Id);
        await using var toUser = await DBUser.GetAsync(member.Id);
        
        if (amount > fromUser.Coins) {
            ctx.ReplyAsync("You can not send more coins than you currently have!");
            return;
        }
        if (amount <= 0) {
            ctx.ReplyAsync("Amount must be above 0!");
            return;
        }
        
        fromUser.Coins -= amount;
        toUser.Coins += amount;
        ctx.ReplyAsync($"Sent {amount} coins to {member.Nickname}!");
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
        await using var user = await DBUser.GetAsync(ctx.Member.Id);

        if (user.Coins < bet) {
            ctx.ReplyAsync("Bet must not be above your coins!");
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
                await StatManager.AddStat(CurrentStatType.Coins, bet,  ctx.Planet.Id);
            }
            else {
                data.Add($"You lost {bet} coins.");
                user.Coins -= bet;
                await StatManager.AddStat(CurrentStatType.Coins, 0-bet, ctx.Planet.Id);
            }
        }

        ctx.ReplyWithMessagesAsync(1750, data);
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
    public async Task Gamble(CommandContext ctx, string color, int bet)
    {
        color = color.Replace("red", "Red").Replace("blue", "Blue").Replace("green", "Green").Replace("black", "Black");
        if (color != "Red" && color != "Blue" && color != "Green" && color != "Black") {
            ctx.ReplyAsync("The color must be Red, Blue, Green, or Black");
            return;
        }

        await using var user = await DBUser.GetAsync(ctx.Member.Id);
        if (user.Coins < bet) {
            ctx.ReplyAsync("Bet must not be above your coins!");
            return;
        }
        if (bet == 0) {
            ctx.ReplyAsync("Bet must not be 0!");
            return;
        }
        await DailyTaskManager.DidTask(DailyTaskType.Gamble_Games_Played, ctx.Member.Id, ctx);

        long choice = color switch
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
        long Winner;
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
            user.Coins += (int)Math.Ceiling(amount);
            data.Add($"You won {Math.Round(amount - bet)} coins!");
            await StatManager.AddStat(CurrentStatType.Coins, (int)amount - bet, ctx.Planet.Id);
        }
        else
        {
            data.Add($"You did not win.");
            await StatManager.AddStat(CurrentStatType.Coins, 0-bet, ctx.Planet.Id);
        }

        ctx.ReplyWithMessagesAsync(1750, data);
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
                ctx.ReplyAsync($"Only the owner of this server can use this command!");
                return;
            }

            RoleIncomes roleincome = await Client.DBContext.RoleIncomes.FirstOrDefaultAsync(x => x.RoleName == rolename && x.PlanetId == ctx.Planet.Id);

            if (roleincome == null) {

                Planet planet = await Planet.FindAsync(ctx.Planet.Id);

                PlanetRole role = (await planet.GetRolesAsync()).FirstOrDefault(x => x.Name == rolename);

                if (role == null) {
                    ctx.ReplyAsync($"Could not find role {rolename}!");
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

                ctx.ReplyAsync($"Set {rolename}'s hourly income/cost to {roleincome.Income} coins!");
            }

            else {
                roleincome.Income = rate;
                await Client.DBContext.SaveChangesAsync();
                ctx.ReplyAsync($"Set {rolename}'s hourly income/cost to {roleincome.Income} coins!");
            }
        }
        [Command("")]
        public async Task ViewAsync(CommandContext ctx, [Remainder] string rolename)
        {
            PlanetRole role = ((NetPlanet)(await NetPlanet.FindAsync(ctx.Planet.Id))).GetRole(rolename);

            if (role == null) {
                ctx.ReplyAsync($"Could not find role {rolename}");
                return;
            }

            RoleIncomes roleincome = await Client.DBContext.RoleIncomes.FirstOrDefaultAsync(x => x.RoleName == rolename && x.PlanetId == ctx.Planet.Id);

            if (roleincome == null) {
                ctx.ReplyAsync($"Hourly Income/Cost has not been set for role {rolename}");
                return;
            }

            ctx.ReplyAsync($"Hourly Income/Cost for {rolename} is {roleincome.Income} coins");
        }
        // add way to view roleincome as table
    }
*/

}
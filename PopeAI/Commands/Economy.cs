namespace PopeAI.Commands.Economy;

public class Economy : CommandModuleBase
{
    Random rnd = new Random();

    public static ConcurrentDictionary<long, byte> AlreadyDoing = new();
    
    [Command("commands")]
    [Alias("help")]
    public Task ListCommands(CommandContext ctx) 
    {
        var embed = new EmbedBuilder()
            .AddPage("PopeAI Commands")
            .AddRow()
                .AddText("Economy", "/pay, /hourly or /h, /richest or /r, /coins or /c, /dice <bet>, /gamble <color> <bet>, /unscramble or /un")
            .AddRow()
                .AddText("Xp", "/xp, /info xp, /leaderboard or /lb")
            .AddRow()
                .AddText("Daily Tasks", "/tasks")
            .AddRow()
			    .AddText("Element Combining", "/suggest <result>, /c or /combine <element 1> <element 2> <optional element 3>, /inv, /vote, /element count, /element mycount")
                //.AddText("Element Combining", "Currently being rewritten!")
            .AddRow()
                .AddText("Planet", "/stats messages, /stats coins, /stats totalmessages, /settings")
            .AddRow()
                .AddText("MyStats", "/mystats messages, /mystats xp");
			
		return ctx.ReplyAsync(embed);
    } 

    [Command("hourly")]
    [Alias("h")]
    public async Task Hourly(CommandContext ctx)
    {
		var info = await PlanetInfo.GetAsync(ctx.Planet.Id, _readonly: true);
		if (info is null || !info.HasEnabled(ModuleType.Coins))
			return;

		await using var user = await DBUser.GetAsync(ctx.Member.Id, true);
        int minutesleft = (user.LastHourly.AddHours(1).Subtract(DateTime.UtcNow)).Minutes;
        if (minutesleft <= 0) {
            await DailyTaskManager.DidTask(DailyTaskType.Hourly_Claims, ctx.Member.Id, ctx);

            int payout = rnd.Next(20, 75);
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
		var info = await PlanetInfo.GetAsync(ctx.Planet.Id, _readonly: true);
		if (info is null || !info.HasEnabled(ModuleType.Coins))
			return;

		using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
        List<DBUser> users = dbctx.Users
            .Where(x => x.PlanetId ==  ctx.Planet.Id)
            .OrderByDescending(x => x.Coins)
            .Take(30)
            .ToList();
        var embed = new EmbedBuilder().AddPage("Users ordered by coins").AddRow();
        int i = 1;
        foreach (DBUser user in users)
        {
            PlanetMember member = await PlanetMember.FindAsync(user.Id, ctx.Planet.Id);
            embed.AddText(text:$"({i}) {member.Nickname} - {(long)user.Coins} coins").AddRow();
            i += 1;
            if (embed.embed.Pages.Last().Children.Count > 10) {
                embed.AddPage("Users ordered by coins").AddRow();
            }
        }
        ctx.ReplyAsync(embed);
    }

    [Command("coins")]
    [Alias("c")]
    public async Task coins(CommandContext ctx)
    {
		var info = await PlanetInfo.GetAsync(ctx.Planet.Id, _readonly: true);
		if (info is null || !info.HasEnabled(ModuleType.Coins))
			return;

		var user = await DBUser.GetAsync(ctx.Member.Id, true);
        ctx.ReplyAsync($"{ctx.Member.Nickname}'s coins: {user.Coins}");
    }

    [Command("inflate")]
    public async Task CountAsync(CommandContext ctx, int coins, PlanetMember target)
    {
        if (ctx.Member.UserId != 12201879245422592)
        {
            return;
        }

        await using var user = await DBUser.GetAsync(target.Id);
        user.Coins += coins;
        await ctx.ReplyAsync($"Successfully inflated {target.Nickname}'s coins");
    }

    [Command("pay")]
    [Alias("send")]
    public async Task PayAsync(CommandContext ctx, PlanetMember member, int amount)
    {
		var info = await PlanetInfo.GetAsync(ctx.Planet.Id, _readonly: true);
		if (info is null || !info.HasEnabled(ModuleType.Coins))
			return;

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
		var info = await PlanetInfo.GetAsync(ctx.Planet.Id, _readonly: true);
		if (info is null || !info.HasEnabled(ModuleType.Coins))
			return;
        
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
    public async Task GetDiceAsync(CommandContext ctx)
	{
		var info = await PlanetInfo.GetAsync(ctx.Planet.Id, _readonly: true);
		if (info is null || !info.HasEnabled(ModuleType.Coins))
			return;

		EmbedBuilder embed = new EmbedBuilder().AddPage("Dice Game")
            .AddRow()
                .AddButton("Load Embed")
                    .OnClickSendInteractionEvent("Dice-Load");
		ctx.ReplyAsync(embed);
	}

    [Interaction(EmbedIteractionEventType.ItemClicked, interactionElementId:"Dice-Load")]
	public async Task OnDiceLoad(InteractionContext ctx)
	{
        await using var user = await DBUser.GetAsync(ctx.Member.Id, _readonly: true);
		ctx.UpdateEmbedForUser(await GetDiceEmbedAsync(ctx, user), ctx.Member.UserId);
	}

    public async Task<EmbedBuilder> GetDiceEmbedAsync(IContext ctx, DBUser user)
    {
        EmbedBuilder embed = new EmbedBuilder().AddPage("Dice")
            .AddRow()
                .AddText(text:$"Your Coins: {user.Coins}")
            .AddRow()
                .AddForm("Dice")
                    .AddRow()
                        .AddInputBox("Bet", placeholder:"Your Bet")
                    .AddRow()
                        .AddButton(text:"Roll")
                            .OnClickSubmitForm()
                .EndForm();
        return embed;
    }

    [Interaction(EmbedIteractionEventType.FormSubmitted, "Dice")]
    public async Task DiceFormSubmitted(InteractionContext ctx) 
    {
        await using var user = await DBUser.GetAsync(ctx.Member.Id);

        int bet = (int)Math.Floor(double.Parse(ctx.Event.FormData[0].Value));

        var embed = await GetDiceEmbedAsync(ctx, user);
        embed.AddRow();

        if (user.Coins < bet) {
            embed.AddText(text:"Bet must not be above your coins!")
                .WithStyles(new TextColor(new Color(255, 0, 0)));
            ctx.UpdateEmbedForUser(embed, ctx.Member.UserId);
            return;
        }
        if (bet <= 0) {
            embed.AddText(text:"Bet must be above 0!")
                .WithStyles(new TextColor(new Color(255, 0, 0)));
            ctx.UpdateEmbedForUser(embed, ctx.Member.UserId);
            return;
        }

        if (AlreadyDoing.TryGetValue(ctx.Member.Id, out byte _byte))
            return;

        AlreadyDoing.TryAdd(ctx.Member.Id, 0x0);
        

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
            //user.Coins -= bet;
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

        Task.Run(async () => {
            foreach(var content in data) {
                embed.AddRow().AddText(text:content);
                ctx.UpdateEmbedForUser(embed, ctx.Member.UserId);
                await Task.Delay(1750);
            }
            var item = (EmbedTextItem)embed.embed.Pages[0].Children[0].Children[0];
            item.Text = $"Your Coins: {user.Coins}";
            ctx.UpdateEmbedForUser(embed, ctx.Member.UserId);
            AlreadyDoing.TryRemove(ctx.Member.Id, out _);
        });

        //Task.Run(async () => {
        //    await Task.Delay(5000);
        //    AlreadyDoing.TryRemove(ctx.Member.Id, out _);
        //});
    } 

    [Command("gamblerates")]
    public async Task GambleInfo(CommandContext ctx)
    {
		var info = await PlanetInfo.GetAsync(ctx.Planet.Id, _readonly: true);
		if (info is null || !info.HasEnabled(ModuleType.Coins))
			return;

		string content = "| Color | Chance | Reward   |\n|-------|--------|----------|\n| Red   | 35%    | 2.86x bet |\n| Blue  | 35%    | 2.86x bet |\n| Green | 20%    | 5x bet   |\n| Black | 10%     | 10x bet  |";
        ctx.ReplyAsync(content);
    }

    [Command("gamble")]
    public async Task GetGambleAsync(CommandContext ctx)
	{
		var info = await PlanetInfo.GetAsync(ctx.Planet.Id, _readonly: true);
		if (info is null || !info.HasEnabled(ModuleType.Coins))
			return;

		EmbedBuilder embed = new EmbedBuilder().AddPage("Gambling Game").AddRow().AddButton("Load Embed").OnClickSendInteractionEvent("Gamble-Load");
		ctx.ReplyAsync(embed);
	}

    [Interaction(EmbedIteractionEventType.ItemClicked, interactionElementId:"Gamble-Load")]
	public async Task OnGambleLoad(InteractionContext ctx)
	{
        await using var user = await DBUser.GetAsync(ctx.Member.Id, _readonly: true);
        try {
		    await ctx.UpdateEmbedForUser(GetGambleEmbed(ctx, user), ctx.Member.UserId);
        }
        catch (Exception e) {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
	}

    public EmbedBuilder GetGambleEmbed(IContext ctx, DBUser user)
    {
        EmbedBuilder embed = new EmbedBuilder().AddPage("Gambling")
            .AddRow()
                .AddText(text:$"Your Coins: {user.Coins}")
            .AddRow()
                .AddForm("Gamble")
                    .AddRow()
                        .AddDropDownMenu("Color", value:"Pick a Color")
                            .WithDropDownItem()
                                .WithText("Red")
                                    .WithStyles(new TextColor(new Color(255, 0, 0)))
                            .WithDropDownItem()
                                .WithText("Blue")
                                    .WithStyles(new TextColor(new Color(0, 0, 170)))
                            .WithDropDownItem()
                                .WithText("Green")
                                    .WithStyles(new TextColor(new Color(0, 128, 0)))
                            .WithDropDownItem()
                                .WithText("Black")
                                    .WithStyles(new TextColor(new Color(0, 0, 0)))
                        .EndDropDownMenu()
                    .AddRow()
                        .AddInputBox("Bet", placeholder:"Your Bet")
                            .WithStyles(new Margin(Size.Zero, Size.Zero, new Size(Unit.Pixels, 6), Size.Zero))
                    .AddRow()
                        .AddButton("Gamble")
                            .OnClickSubmitForm()
                .EndForm();
        return embed;
    }

    [Interaction(EmbedIteractionEventType.FormSubmitted, "Gamble")]
    public async Task GambleFormSubmitted(InteractionContext ctx) 
    {
        await using var user = await DBUser.GetAsync(ctx.Member.Id);

        if (AlreadyDoing.TryGetValue(ctx.Member.Id, out byte _byte))
        {
            return;
        }

        var embed = GetGambleEmbed(ctx, user);
        string color = ctx.Event.FormData[0].Value;

        embed.AddRow();

        if (color == "Pick a Color")
        {
            embed.AddText(text:"You must select a color to bet on!")
                .WithStyles(new TextColor(new Color(255, 0, 0)));
            ctx.UpdateEmbedForUser(embed, ctx.Member.UserId);
            return;
        }
        
        int bet = (int)Math.Floor(double.Parse(ctx.Event.FormData[1].Value));

        if (user.Coins < bet) {
            embed.AddText(text:"Bet must not be above your coins!")
                .WithStyles(new TextColor(new Color(255, 0, 0)));
            ctx.UpdateEmbedForUser(embed, ctx.Member.UserId);
            return;
        }
        if (bet <= 0) {
            embed.AddText(text:"Bet must be above 0!")
                .WithStyles(new TextColor(new Color(255, 0, 0)));
            ctx.UpdateEmbedForUser(embed, ctx.Member.UserId);
            return;
        }

        AlreadyDoing.TryAdd(ctx.Member.Id, 0x0);
        
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
        double amount = bet*muit;
        user.Coins -= bet;

        string final_text = "";
        if (Winner == choice)
        {
            user.Coins += (int)Math.Ceiling(amount);
            final_text = $"You won {Math.Round(amount)} coins! 🎉🎉";
            await StatManager.AddStat(CurrentStatType.Coins, (int)amount - bet, ctx.Planet.Id);
        }
        else
        {
            final_text = "You did not win.";
            await StatManager.AddStat(CurrentStatType.Coins, 0-bet, ctx.Planet.Id);
        }

        Task.Run(async () => {
            embed.AddRow().AddText(text:$"You picked {color}");
            ctx.UpdateEmbedForUser(embed, ctx.Member.UserId);
            await Task.Delay(1750);
            embed.AddRow().AddText(text:$"The color drawn is {colorwon}");
            ctx.UpdateEmbedForUser(embed, ctx.Member.UserId);
            await Task.Delay(1750);
            embed.AddRow().AddText(text:final_text);
            var item = (EmbedTextItem)embed.embed.Pages[0].Children[0].Children[0];
            item.Text = $"Your Coins: {user.Coins}";
            ctx.UpdateEmbedForUser(embed, ctx.Member.UserId);
            AlreadyDoing.TryRemove(ctx.Member.Id, out _);
        });
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
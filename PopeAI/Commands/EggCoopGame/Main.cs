using Database.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valour.Net.EmbedMenu;
using System.Threading;
using Valour.Net.CommandHandling;
using Valour.Sdk.Models.Messages.Embeds.Styles.Bootstrap;
using IdGen;
using System.IO;
using Valour.Sdk.Models.Messages.Embeds;

namespace Valour_Bot.Commands.EggCoopGame;

public class EggCoopGame : CommandModuleBase
{
    public static ConcurrentDictionary<long, InteractionContext> InteractionContexts = new();
    public static ConcurrentDictionary<long, DateTime> UserIdsCurrentlyConnected = new();
    public static int ItemsPerResearchPage = 5;
    public static IdManager FoxIdManager = new();

    // Timer for executing timed tasks
    private static Timer _timer;

    public static Task StartAsync()
    {
        Console.WriteLine("Starting Message Worker");

        _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMilliseconds(300));

        return Task.CompletedTask;
    }

    public static async void DoWork(object? _state)
    {
        foreach (var pair in UserIdsCurrentlyConnected)
        {
            // process game updates

            var ctx = InteractionContexts[pair.Key];

            var player = (await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id)).Data.EggCoopGameData;
            await Game.ProcessTick(player);

            // handle inactivity somehow
            if (DateTime.UtcNow.Subtract(pair.Value).TotalMinutes >= 5)
            {
                player.MsPerTick = 1000;
            }
            else if (DateTime.UtcNow.Subtract(pair.Value).TotalMinutes >= 2) {

            }
            else
            {
                player.MsPerTick = 300;
                if (player.IsInOfflineProgressPage == 0)
                    await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
            }
        }
    }

    [Event(EventType.OnChannelWatching)]
    public void OnChannelWatching(ChannelWatchingContext ctx)
    {
        var oldctxs = InteractionContexts.Values.Where(x => x.Channel is not null && x.Channel.Id == ctx.Channel.Id).ToList();
        foreach (var oldctx in oldctxs) {
            if (!ctx.channelWatchingUpdate.UserIds.Contains(oldctx.Member.UserId)) {
                // means user is no long watching the channel
                if (UserIdsCurrentlyConnected.ContainsKey(oldctx.Member.Id))
                {
                    if (DateTime.UtcNow.Subtract(UserIdsCurrentlyConnected[oldctx.Member.Id]).TotalSeconds > 30)
                    {
                        InteractionContexts.TryRemove(oldctx.Member.Id, out var _);
                        UserIdsCurrentlyConnected.TryRemove(oldctx.Member.Id, out var _);
                    }
                }
            }
        }
    }

    [Command("coop")]
    public async Task UpdateGameScreen(CommandContext ctx)
    {
        if (ctx.Member.UserId != 12201879245422592)
        {
            return;
        }
        EmbedBuilder embed = new EmbedBuilder().AddPage("Egg Coop Game").AddRow().AddButton("Load The Game").OnClickSendInteractionEvent("EggCoop-Load");
        await ctx.ReplyAsync(embed);
    }

    public static async ValueTask ProcessOfflineTime(InteractionContext ctx) 
    {
        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;
        var timedifference = DateTime.UtcNow.Subtract(player.CurrentFarm.LastUpdated);
        var secondspertick = 1.0;
        var secondstouse = timedifference.TotalSeconds;
        if (timedifference.TotalDays > 3) {
            var max = 60*60*24*3.0;
            if (secondstouse > max)
                secondstouse = max;
            secondspertick = 256;
        }
        else if (timedifference.TotalDays > 1)
            secondspertick = 100;
        else if (timedifference.TotalHours > 6)
            secondspertick = 50;
        else
            secondspertick = 20;
        var farm = player.CurrentFarm;
        var starting_chickens = farm.Chickens;
        var starting_money = farm.Money;
        var starting_eggs = farm.EggsLaid;
        while (secondstouse > 0) {
            await Game.ProcessTick(player, secondspertick*1000);
            secondstouse -= secondspertick;
        }

        var text = "";
        if (timedifference.TotalMinutes < 60)
            text = $"{(long)timedifference.TotalMinutes} minutes";
        else if (timedifference.TotalHours < 24)
            text = $"{(long)timedifference.TotalHours} hours";
        else
            text = $"{(long)timedifference.TotalDays} days";

        player.IsInOfflineProgressPage = 1;

        var embed = new EmbedBuilder()
            .AddPage("Processed Time Away!")
                .AddRow()
                    .AddText($"You were away for about {text}")
                .AddRow()
                    .AddText($"+{Functions.Format(farm.Chickens-starting_chickens, Rounding: 2, NoK: true)} chickens")
                .AddRow()
                    .AddText($"+${Functions.Format(farm.Money-starting_money, Rounding: 2, NoK: true)}")
                .AddRow()
                    .AddText($"{Functions.Format(farm.EggsLaid-starting_eggs, Rounding: 2, NoK: true)} eggs laid")
                .AddRow()
                    .AddButton("Okay").OnClick(OnOkayFromOfflineTime);
        await ctx.UpdateEmbedForUser(embed);
    }

    [EmbedMenuFunc]
    public static async ValueTask OnOkayFromOfflineTime(InteractionContext ctx)
    {
        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;
        player.IsInOfflineProgressPage = 0;
        player.CurrentEmbedMenuPageNum = 0;
        await HeaderClicks[0](ctx);
    }

    public static async ValueTask<bool> CheckOfflineTime(InteractionContext ctx)
    {
        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        if (DateTime.UtcNow.Subtract(state.Data.EggCoopGameData.CurrentFarm.LastUpdated).TotalMinutes >= 1)
        {
            await ProcessOfflineTime(ctx);
            return true;
        }
        else
        {
            state.Data.EggCoopGameData.IsInOfflineProgressPage = 0;
            return false;
        }
    }

    public static void LoadStartup(InteractionContext ctx)
    {
        if (!UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            UserIdsCurrentlyConnected[ctx.Member.UserId] = DateTime.UtcNow;
        UserIdsCurrentlyConnected[ctx.Member.UserId] = DateTime.UtcNow;
        InteractionContexts[ctx.Member.UserId] = ctx;
    }

    [Interaction(EmbedIteractionEventType.ItemClicked, interactionElementId: "EggCoop-Load")]
    public async Task OnGameLoad(InteractionContext ctx)
    {
        LoadStartup(ctx);
        
        if (!await CheckOfflineTime(ctx))
            await ctx.UpdateEmbedForUser(await GetGameScreen(ctx), ctx.Member.UserId);
    }

    [EmbedMenuFunc]
    public static async ValueTask ClickedHatchChicken(InteractionContext ctx)
    {
        if (UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            UserIdsCurrentlyConnected[ctx.Member.UserId] = DateTime.UtcNow;
        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        if (state.Data.EggCoopGameData.CurrentFarm.Chickens < state.Data.EggCoopGameData.CurrentFarm.MaxChickens)
            state.Data.EggCoopGameData.CurrentFarm.Chickens += (int)state.Data.EggCoopGameData.CurrentFarm.ChickensGainedPerClick;

        await ctx.UpdateEmbedForUser(await GetGameScreen(ctx), ctx.Member.UserId);
    }

    [EmbedMenuFunc]
    public static async ValueTask GetGameScreenAsync(InteractionContext ctx)
    {
        if (UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            UserIdsCurrentlyConnected[ctx.Member.UserId] = DateTime.UtcNow;

        if (!await CheckOfflineTime(ctx))
            await ctx.UpdateEmbedForUser(await GetGameScreen(ctx), ctx.Member.UserId);
    }

    [EmbedMenuFunc]
    public static async ValueTask GetResearchScreenAsync(InteractionContext ctx)
    {
        if (UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            UserIdsCurrentlyConnected[ctx.Member.UserId] = DateTime.UtcNow;
        
        if (!await CheckOfflineTime(ctx))
            await ctx.SendTargetedEmbedUpdateForUser(await GetResearchScreen(ctx), ctx.Member.UserId);
    }

    [EmbedMenuFunc]
    public static async ValueTask GetHabitatsScreenAsync(InteractionContext ctx)
    {
        if (UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            UserIdsCurrentlyConnected[ctx.Member.UserId] = DateTime.UtcNow;

        if (!await CheckOfflineTime(ctx))
            await ctx.SendTargetedEmbedUpdateForUser(await GetHabitatsScreen(ctx), ctx.Member.UserId);
    }

    [EmbedMenuFunc]
    public static async ValueTask GetStatsScreenAsync(InteractionContext ctx)
    {
        if (UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            UserIdsCurrentlyConnected[ctx.Member.UserId] = DateTime.UtcNow;

        if (!await CheckOfflineTime(ctx))
            await ctx.SendTargetedEmbedUpdateForUser(await GetStatsScreen(ctx), ctx.Member.UserId);
    }

    [EmbedMenuFunc]
    public static async ValueTask GetBaconScreenAsync(InteractionContext ctx)
    {
        if (UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            UserIdsCurrentlyConnected[ctx.Member.UserId] = DateTime.UtcNow;

        if (!await CheckOfflineTime(ctx))
            await ctx.SendTargetedEmbedUpdateForUser(await GetBaconScreen(ctx), ctx.Member.UserId);
    }

    [EmbedMenuFunc]
    public static async ValueTask GetGoalsScreenAsync(InteractionContext ctx)
    {
        if (UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            UserIdsCurrentlyConnected[ctx.Member.UserId] = DateTime.UtcNow;

        if (!await CheckOfflineTime(ctx))
            await ctx.SendTargetedEmbedUpdateForUser(await GetGoalsScreen(ctx), ctx.Member.UserId);
    }

    [EmbedMenuFunc]
    public static async ValueTask GetPrestigeScreenAsync(InteractionContext ctx)
    {
        if (UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            UserIdsCurrentlyConnected[ctx.Member.UserId] = DateTime.UtcNow;

        if (!await CheckOfflineTime(ctx))
            await ctx.UpdateEmbedForUser(await GetPrestigeScreen(ctx), ctx.Member.UserId);
    }

    [EmbedMenuFunc]
    public static async ValueTask GetBoostsScreenAsync(InteractionContext ctx)
    {
        if (UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            UserIdsCurrentlyConnected[ctx.Member.UserId] = DateTime.UtcNow;

        if (!await CheckOfflineTime(ctx))
            await ctx.UpdateEmbedForUser(await GetBoostsScreen(ctx), ctx.Member.UserId);
    }

    public static async ValueTask<EmbedBuilder> GetBoostsScreen(InteractionContext ctx, UserEmbedState? state = null)
    {
        if (!UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            LoadStartup(ctx);
        if (state is null)
            state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);

        var player = state.Data.EggCoopGameData;
        player.CurrentEmbedMenuPageNum = 5;

        var embed = new EmbedBuilder()
            .AddPage($"Egg Coop Game - Boosts");

        AddHeader(embed, state);

        Color  colorforback = player.CurrentBoostPageNumber > 0 ? new Color(0, 0, 0) : new Color(50, 0, 0);
        var currentnum = player.CurrentBoostPageNumber;
        var totalnum = (int)(Math.Ceiling(GameData.BoostData.Count / (double)8)) - 1;
        Color colorforfront = currentnum < totalnum ? new Color(0, 0, 0) : new Color(50, 0, 0);

        embed
            .AddRow()
                .SetId("change-page-and-type-row")
                .WithStyles(new FlexJustifyContent(JustifyContent.SpaceBetween), new FlexDirection(Direction.Row))
                .WithRow()
                    .SetId("change-page-buttons")
                    .AddButtonWithNoText()
                        .SetId("back-button")
                        .OnClick(PrevBoostpage)
                        .WithStyles(new BackgroundColor(colorforback))
                        .AddText("&#60;")
                    .Close()
                    .AddButtonWithNoText()
                        .SetId("forward-button")
                        .OnClick(NextBoostpage)
                        .WithStyles(new BackgroundColor(colorforfront))
                        .AddText("&#62;")
                    .Close()
                .CloseRow()
                .WithRow()
                    .SetId("change-buying-boost-row")
                    .AddButtonWithNoText()
                        .SetId("change-buying-boost-button")
                        .OnClick(ChangeIsBuyingBoosts)
                        .AddText(player.IsBuyingBoosts ? "Use Boosts" : "Buy Boosts")
                    .Close()
                 .CloseRow()
            .AddRow()
                .WithStyles(FlexDirection.Column)
                .SetId("embed-body");

        var count = 0;
        var startingi = player.CurrentBoostPageNumber * 8;
        var max = player.IsBuyingBoosts ? GameData.BoostData.Count : player.Boosts.Count;
        for (var i = startingi; i < max; i++)
        {
            var boost = player.IsBuyingBoosts ? GameData.BoostData.First(x => x.Id == i) : GameData.BoostData.First(x => x.Id == player.Boosts.Keys.ToList()[i]);
            var owned = player.Boosts.ContainsKey(boost.Id) ? player.Boosts[boost.Id] : 0;
            embed.WithRow()
                .WithStyles(
                    FlexDirection.Row,
                    FlexJustifyContent.SpaceBetween,
                    new FlexAlignItems(AlignItem.Stretch),
                    new Margin(top: new Size(Unit.Pixels, 8))
                )
                .SetId($"embed-row-for-boost-row-{count}")
                .WithRow()
                    .WithStyles(
                        FlexDirection.Column,
                        FlexJustifyContent.SpaceBetween,
                        new FlexAlignItems(AlignItem.Stretch),
                        new FlexAlignSelf(AlignSelf.FlexStart)
                    )
                    .SetId($"embed-row-for-boost-info-{count}")
                    .AddText($"{boost.Name} ({owned}x)").WithStyles(new FontSize(new Size(Unit.Pixels, 16)))
                        .SetId($"boost-name-{count}")
                    .WithRow()
                        .AddText(boost.Description)
                        .SetId($"boost-description-{count}")
                    .CloseRow();
            
            if (!player.IsBuyingBoosts && player.CurrentFarm.ActiveBoosts.Any(x => x.Id == boost.Id))
            {
                embed
                    .AddProgress()
                        .SetId($"boost-progress-{count}")
                        .WithName($"{player.CurrentFarm.ActiveBoosts.First(x => x.Id == boost.Id).SecondsLeft:n0}s/{boost.Duration.TotalSeconds:n0}s")
                        .SetId($"boost-progress-name-{count}")
                        .WithProgressBar((int)(player.CurrentFarm.ActiveBoosts.First(x => x.Id == boost.Id).SecondsLeft / boost.Duration.TotalSeconds * 100))
                            .WithBootStrapClasses(BootstrapBackgroundColorClass.Info)
                    .Close();
            }
            
                embed
                .CloseRow()
                .WithRow()
                    .WithStyles(new FlexAlignSelf(AlignSelf.FlexEnd))
                    .AddButtonWithNoText()
                        .SetId($"boost-button-{count}");
            if (player.IsBuyingBoosts)
            {
                if (player.Bacon >= boost.Price)
                    embed.WithStyles(FlexDirection.Column, new BackgroundColor(new Color(0, 175, 0))).OnClick(BuyBoost, boost.Id.ToString());
                else
                    embed.WithStyles(FlexDirection.Column, new BackgroundColor(new Color(150, 150, 150)));
                embed.AddText($"{boost.Price} :bacon: ")
                        .WithStyles(new FontSize(new Size(Unit.Pixels, 14)))
                        .SetId($"boost-button-text-{count}");
            }
            else
            {
                string text = "";
                if (!player.CurrentFarm.ActiveBoosts.Any(x => x.Id == boost.Id) && player.Boosts[boost.Id] > 0)
                {
                    embed.WithStyles(FlexDirection.Column, new BackgroundColor(new Color(0, 175, 0))).OnClick(UseBoost, boost.Id.ToString());
                    text = "Use";
                }
                else
                {
                    embed.WithStyles(FlexDirection.Column, new BackgroundColor(new Color(150, 150, 150)));
                    text = player.CurrentFarm.ActiveBoosts.Any(x => x.Id == boost.Id) ? "Already Activated" : "Use";
                }
                embed.AddText(text)
                        .WithStyles(new FontSize(new Size(Unit.Pixels, 14)))
                        .SetId($"boost-button-text-{count}");
            }
                embed
                    .Close()
                .CloseRow();
            embed.CloseRow();
            count += 1;
            if (count >= 8)
                break;
        }

        HandleFoxes(embed, state);

        embed.CalculateHashOnItemsForSettingHasChanged();
        embed.CalculateHasChangedForAllItems(state.Data.EmbedItemsHashes);

        state.StoreItemHashes(embed);
        return embed;
    }

    [EmbedMenuFunc]
    public static async ValueTask BuyBoost(InteractionContext ctx)
    {
        ushort id = ushort.Parse(ctx.Event.ElementId.Split("::MENU$-")[0]);
        var boost = GameData.BoostData.First(x => x.Id == id);

        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;

        if (player.Bacon >= boost.Price)
        {
            player.Bacon -= boost.Price;
            if (!player.Boosts.ContainsKey(boost.Id))
                player.Boosts[boost.Id] = 0;
            player.Boosts[boost.Id] += 1;
        }

        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }

    [EmbedMenuFunc]
    public static async ValueTask UseBoost(InteractionContext ctx)
    {
        ushort id = ushort.Parse(ctx.Event.ElementId.Split("::MENU$-")[0]);
        var boost = GameData.BoostData.First(x => x.Id == id);

        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;

        if (player.Boosts.ContainsKey(boost.Id) && player.Boosts[boost.Id] > 0)
        {
            player.Boosts[boost.Id] -= 1;
            player.CurrentFarm.ActiveBoosts.Add(new() { Id = boost.Id, SecondsLeft = boost.Duration.TotalSeconds });
        }

        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }

    public static async ValueTask<EmbedBuilder> GetPrestigeScreen(InteractionContext ctx, UserEmbedState? state = null)
    {
        if (!UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            LoadStartup(ctx);
        if (state is null)
            state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);

        var player = state.Data.EggCoopGameData;
        player.CurrentEmbedMenuPageNum = 4;

        var embed = new EmbedBuilder()
            .AddPage($"Egg Coop Game - Prestige");

        AddHeader(embed, state);

        var breadgain = Game.GetBreadGain(player);
        var breadperhour = (breadgain - player.LastBreadGain) * 3600 * (1000 / player.MsPerTick);
        var bonus = player.Bread * (0.1 + Game.GetTotalEffectForType(player, ModifierType.BonusPerBread) - 1) * 100;

        embed
        .AddRow()
            .AddText("Current :bread:", Functions.Format(player.Bread, Rounding: 3, NoK: true))
        .AddRow()
            .AddText("Current bonus", $"+{Functions.Format(bonus, Rounding: 2, NoK: true)}% egg value")
        .AddRow()
            .AddText(":bread: Gain", $"{Functions.Format(breadgain, Rounding: 3, NoK: true)} ({Functions.Format(breadperhour, Rounding: 3, NoK: true)}/h)")
        .AddRow()
            .AddText("Total Prestige Earnings", Functions.Format(player.TotalPrestigeEarnings, Rounding: 3, NoK: true, ExtraSymbol: "$"))
        .AddRow()
            .AddButton("Prestige").OnClick(Prestige);

        HandleFoxes(embed, state);
        return embed;
    }

    public static async ValueTask<EmbedBuilder> GetBaconScreen(InteractionContext ctx, UserEmbedState? state = null)
    {
        if (!UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            LoadStartup(ctx);
        if (state is null)
            state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);

        var player = state.Data.EggCoopGameData;
        player.CurrentEmbedMenuPageNum = 7;

        var embed = new EmbedBuilder()
            .AddPage($"Egg Coop Game - Bacon");

        AddHeader(embed, state);

        if (player.Bread > 100_000)
        {

            var breadgain = Game.GetBreadGain(player);
            var breadperhour = (breadgain - player.LastBreadGain) * 3600 * (1000 / player.MsPerTick);
            var bonus = player.Bread * (0.1 + Game.GetTotalEffectForType(player, ModifierType.BonusPerBread) - 1) * 100;

            embed
            .AddRow()
                .SetId("bacon-count-row")
                .AddText("Current :bacon:", Functions.Format(player.Bread, Rounding: 3, NoK: true)).SetId("bacon-count");
        }

        else
        {
            embed
                .AddRow().SetId("row-to-unlock-bacon-generator")
                    .AddText("You need at least 100k :bread: before you can unlock this page!").SetId("need-at-least-100k");
        }

        HandleFoxes(embed, state);

        embed.CalculateHashOnItemsForSettingHasChanged();
        embed.CalculateHasChangedForAllItems(state.Data.EmbedItemsHashes);

        state.StoreItemHashes(embed);

        return embed;
    }

    public static async ValueTask<EmbedBuilder> GetGoalsScreen(InteractionContext ctx, UserEmbedState? state = null)
    {
        if (!UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            LoadStartup(ctx);
        if (state is null)
            state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);

        var player = state.Data.EggCoopGameData;
        player.CurrentEmbedMenuPageNum = 6;

        var embed = new EmbedBuilder()
            .AddPage($"Egg Coop Game - Goals");

        AddHeader(embed, state);

        embed.AddRow()
                .WithStyles(FlexDirection.Column)
                .SetId("embed-body");

        var count = 0;
        foreach (var item in player.CurrentGoals)
        {
            var goal = GameData.GoalData.FirstOrDefault(x => x.Id == item);
            embed.WithRow()
                .WithStyles(
                    FlexDirection.Row,
                    FlexJustifyContent.SpaceBetween,
                    new FlexAlignItems(AlignItem.Stretch),
                    new Margin(top: new Size(Unit.Pixels, 8))
                )
                .SetId($"embed-row-for-goal-row-{count}")
                .WithRow()
                    .WithStyles(
                        FlexDirection.Column,
                        FlexJustifyContent.SpaceBetween,
                        new FlexAlignItems(AlignItem.Stretch),
                        new FlexAlignSelf(AlignSelf.FlexStart)
                    )
                    .SetId($"embed-row-for-goal-info-{count}")
                    .AddText(goal.Name).WithStyles(new FontSize(new Size(Unit.Pixels, 16)))
                        .SetId($"goal-name-{count}")
                    .WithRow()
                        .AddText(goal.Description)
                        .SetId($"goal-description-{count}")
                    .CloseRow()
                    .AddProgress()
                        .SetId($"goal-progress-{count}")
                        .WithProgressBar((int)(Math.Min(goal.IsCompletedFunc(player),1.0)*100))
                            .WithBootStrapClasses(BootstrapBackgroundColorClass.Info)
                    .Close()
                .CloseRow()
                .WithRow()
                    .WithStyles(new FlexAlignSelf(AlignSelf.FlexEnd))
                    .AddButtonWithNoText()
                        .SetId($"goal-button-{count}");
            if (goal.IsCompletedFunc(player) >= 1.0)
                embed.WithStyles(FlexDirection.Column, new BackgroundColor(new Color(0, 175, 0))).OnClick(CompleteGoal, goal.Id.ToString());
            else
                embed.WithStyles(FlexDirection.Column, new BackgroundColor(new Color(150, 150, 150)));
            var text = "";
            if (goal.Reward.Type == RewardType.Bacon)
                text = $"{Functions.Format(goal.Reward.Amount, WholeNum: true, NoK: true)} :bacon:";
            else if (goal.Reward.Type == RewardType.Dollars)
                text = $"${Functions.Format(goal.Reward.Amount, Rounding: 2, NoK: true)}";
            else if (goal.Reward.Type == RewardType.Bread)
                text = $"{Functions.Format(goal.Reward.Amount, WholeNum: true, NoK: true)} :bread:";
            embed.AddText(text)
                    .WithStyles(new FontSize(new Size(Unit.Pixels, 14)))
                    .SetId($"goal-button-text-{count}")
                .Close()
            .CloseRow();
            embed.CloseRow();
            count += 1;
        }

        HandleFoxes(embed, state);

        embed.CalculateHashOnItemsForSettingHasChanged();
        embed.CalculateHasChangedForAllItems(state.Data.EmbedItemsHashes);

        state.StoreItemHashes(embed);
        return embed;
    }

    public static async ValueTask<EmbedBuilder> GetStatsScreen(InteractionContext ctx, UserEmbedState? state = null)
    {
        if (!UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            LoadStartup(ctx);
        if (state is null)
            state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);

        var player = state.Data.EggCoopGameData;
        player.CurrentEmbedMenuPageNum = 3;

        var embed = new EmbedBuilder()
            .AddPage($"Egg Coop Game - Stats");

        AddHeader(embed, state);

        var breadgain = Game.GetBreadGain(player);
        var breadperhour = (breadgain - player.LastBreadGain) * 3600 * (1000 / player.MsPerTick);

        embed.AddRow().SetId("1")
            .AddText("Farm's Value", Functions.Format(player.CurrentFarm.FarmValue, Rounding: 3, NoK: true, ExtraSymbol: "$"))
                .SetId("Farm-Value-Text")
            .AddText("Current Egg", GameData.EggData[player.CurrentFarm.EggTypeIndex].Name)
                .SetId("current-egg")
        .AddRow().SetId("2")
            .AddText("Total Prestige Earnings", Functions.Format(player.TotalPrestigeEarnings, Rounding: 3, NoK: true, ExtraSymbol: "$"))
                .SetId("Total Prestige Earnings")
        .AddRow().SetId("3")
            .AddText(":bread: Gain", $"{Functions.Format(breadgain, Rounding: 3, NoK: true)} ({Functions.Format(breadperhour, Rounding: 3, NoK: true)}/h)")
                .SetId(":bread: Gain")
        .AddRow().SetId("4")
            .AddText("Chickens Gain", Functions.Format(player.CurrentFarm.ChickenGain, Rounding: 3, NoK: true) + "/m")
                .SetId("Chickens Gain")
        .AddRow().SetId("5")
            .AddText("Laying Rate", Functions.Format(player.CurrentFarm.EggLayingRate * player.CurrentFarm.Chickens * 60, Rounding: 2, NoK: true) + "/m")
                .SetId("Laying Rate");

        HandleFoxes(embed, state);

        embed.CalculateHashOnItemsForSettingHasChanged();
        embed.CalculateHasChangedForAllItems(state.Data.EmbedItemsHashes);

        state.StoreItemHashes(embed);
        return embed;
    }

    public static async ValueTask<EmbedBuilder> GetGameScreen(InteractionContext ctx, UserEmbedState? state = null)
    {
        if (!UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            LoadStartup(ctx);
        if (state is null)
            state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);

        var player = state.Data.EggCoopGameData;
        player.CurrentEmbedMenuPageNum = 0;

        var embed = new EmbedBuilder()
            .AddPage($"Egg Coop Game - Home");

        AddHeader(embed, state);

        var nextegg = GameData.EggData[player.CurrentFarm.EggTypeIndex + 1];
        var currenteggOoMs = Math.Log10(GameData.EggData[player.CurrentFarm.EggTypeIndex].UnlockAtFarmValue);
        var farmvalueOoMs = Math.Log10(player.CurrentFarm.FarmValue);
        var diffinOoMsfarmvalue = farmvalueOoMs - currenteggOoMs;
        var progresstodiscovernext = Math.Max(0, Math.Min(diffinOoMsfarmvalue / (Math.Log10(nextegg.DiscoverAtFarmValue)-currenteggOoMs), 1.0))*100;
        var progresstounlocknext = Math.Max(0, Math.Min(diffinOoMsfarmvalue / (Math.Log10(nextegg.UnlockAtFarmValue) - currenteggOoMs), 1.0)) * 100;

        embed
            .AddRow()
                .AddText($"{Functions.Format(player.CurrentFarm.Chickens, OnlyNumbers:true)}")
            .AddRow();
        if (progresstodiscovernext <= 99.999) { 
                embed.AddProgress("Progress to next egg")
                    .WithName($"Progress to discover next egg ({Math.Round(progresstodiscovernext,2)}%)")
                        .SetId("name-for-progress")
                    .WithProgressBar((int)progresstodiscovernext)
                        .WithBootStrapClasses(BootstrapBackgroundColorClass.Info)
                    .Close();
        }
        else
        {
            embed.AddProgress("Progress to next egg")
                    .WithName($"Progress to unlock {nextegg.Name} ({Math.Round(progresstounlocknext, 2)}%)")
                        .SetId("name-for-progress")
                    .WithProgressBar((int)progresstounlocknext)
                        .WithBootStrapClasses(BootstrapBackgroundColorClass.Info)
                    .Close();
            if (progresstounlocknext >= 99.999)
            {
                embed.AddButton("Go to next egg").OnClick(NextEgg);
            }
        }
            embed.AddRow()
                .AddButton($"+{(int)player.CurrentFarm.ChickensGainedPerClick}")
                    .OnClick(ClickedHatchChicken)
                .WithStyles(new Padding(new Size(Unit.Pixels, 40), new Size(Unit.Pixels, 40)));

        HandleFoxes(embed, state);

        return embed;
    }

    [EmbedMenuFunc]
    public static async ValueTask NextEgg(InteractionContext ctx)
    {
        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;

        var nextegg = GameData.EggData[player.CurrentFarm.EggTypeIndex+1];

        if (player.CurrentFarm.FarmValue > nextegg.UnlockAtFarmValue && nextegg.Name != null)
        {
            player.CurrentFarm.Chickens = 0;
            player.CurrentFarm.Money = 0;
            player.CurrentFarm.ResearchesCompleted = new();
            player.CurrentFarm.Houses = new() 
            {
                { 0, 1 },
                { 1, 0 },
                { 2, 0 },
                { 3, 0 }
            };
            player.CurrentFarm.EggTypeIndex += 1;
        }

        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }

    [EmbedMenuFunc]
    public static async ValueTask ChangeResearchType(InteractionContext ctx)
    {
        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;
        player.CurrentFarm.InResearchType = player.CurrentFarm.InResearchType == 0 ? 1 : 0;
        state.Data.EmbedItemsHashes = null;
        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }

    [EmbedMenuFunc]
    public static async ValueTask ChangeIsBuyingBoosts(InteractionContext ctx)
    {
        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;
        player.IsBuyingBoosts = !player.IsBuyingBoosts;
        state.Data.EmbedItemsHashes = null;
        player.CurrentBoostPageNumber = 0;
        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }


    [EmbedMenuFunc]
    public static async ValueTask BuyCommonResearch(InteractionContext ctx)
    {
        ushort id = ushort.Parse(ctx.Event.ElementId.Split("::MENU$-")[0]);
        var research = GameData.ResearchData.First(x => x.Id == id);

        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;

        if (research.CostFunc(player.GetCommonResearchLevel(research.Id)) < player.CurrentFarm.Money && player.GetCommonResearchLevel(id) < research.MaxLevel)
        {
            player.CurrentFarm.Money -= research.CostFunc(player.GetCommonResearchLevel(research.Id));
            if (!player.CurrentFarm.ResearchesCompleted.ContainsKey(id))
                player.CurrentFarm.ResearchesCompleted[id] = 0;
            player.CurrentFarm.ResearchesCompleted[id] += 1;
        }

        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }

    [EmbedMenuFunc]
    public static async ValueTask CompleteGoal(InteractionContext ctx)
    {
        int id = int.Parse(ctx.Event.ElementId.Split("::MENU$-")[0]);
        var goal = GameData.GoalData.First(x => x.Id == id);

        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;

        if (goal.IsCompletedFunc(player) >= 1 && !player.GoalsDone.Contains(goal.Id))
        {
            player.CurrentGoals.Remove(goal.Id);
            player.GoalsDone.Add(goal.Id);
            goal.Reward.Execute(player);
        }

        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }

    [EmbedMenuFunc]
    public static async ValueTask BuyBaconResearch(InteractionContext ctx)
    {
        ushort id = ushort.Parse(ctx.Event.ElementId.Split("::MENU$-")[0]);
        var research = GameData.BaconResearchData.First(x => x.Id == id);

        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;

        if (research.CostFunc(player.GetBaconResearchLevel(research.Id)) < player.Bacon && player.GetBaconResearchLevel(id) < research.MaxLevel)
        {
            player.Bacon -= (long)research.CostFunc(player.GetBaconResearchLevel(research.Id));
            if (!player.BaconResearchesCompleted.ContainsKey(id))
                player.BaconResearchesCompleted[id] = 0;
            player.BaconResearchesCompleted[id] += 1;
        }

        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }


    [EmbedMenuFunc]
    public static async ValueTask ClickedFox(InteractionContext ctx)
    {
        string data = ctx.Event.ElementId.Split("::MENU$-")[0];
        var foxid = long.Parse(data);

        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;
        var fox = player.Foxes.FirstOrDefault(x => x.Id == foxid);
        if (fox is not null && !fox.IsTakenDown)
        {
            player.FoxesPetted += 1;
            fox.IsTakenDown = true;
            fox.TakenDownAt = DateTime.UtcNow;
            var reward = fox.GetReward(player);
            fox.Reward = reward;

            if (reward.Type == RewardType.Bacon)
                player.Bacon += (int)reward.Amount;
            else if (reward.Type == RewardType.Dollars)
            {
                fox.Reward.Amount = reward.Amount * player.CurrentFarm.FarmValue / 100;
                player.CurrentFarm.Money += fox.Reward.Amount;
                player.TotalPrestigeEarnings += fox.Reward.Amount;
            }
        }
        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }

    [EmbedMenuFunc]
    public static async ValueTask BuyHab(InteractionContext ctx)
    {
        string data = ctx.Event.ElementId.Split("::MENU$-")[0];
        var playerhabindex = ushort.Parse(data.Split("?")[0]);
        var habid = int.Parse(data.Split("?")[1]);

        var hab = GameData.HouseData[habid];

        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;

        if (hab.Cost < player.CurrentFarm.Money)
        {
            player.CurrentFarm.Money -= hab.Cost;
            player.CurrentFarm.Houses[playerhabindex] = habid;
        }

        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }

    [EmbedMenuFunc]
    public static async ValueTask Prestige(InteractionContext ctx)
    {
        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;

        player.Bread += Game.GetBreadGain(player);

        player.TotalPrestigeEarnings = 0;
        player.CurrentFarm.Chickens = 0;
        player.CurrentFarm.Money = 0;
        player.CurrentFarm.ResearchesCompleted = new();
        player.CurrentFarm.Houses = new()
        {
            { 0, 1 },
            { 1, 0 },
            { 2, 0 },
            { 3, 0 }
        };
        player.CurrentFarm.EggTypeIndex = 0;

        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }

    [EmbedMenuFunc]
    public static async ValueTask PrevResearchpage(InteractionContext ctx)
    {
        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;
        if (player.CurrentFarm.InResearchType == 0)
        {
            if (player.CurrentFarm.CurrentResearchPageNumber > 0)
                player.CurrentFarm.CurrentResearchPageNumber -= 1;
        }
        else
        {
            if (player.CurrentFarm.CurrentBaconResearchPageNumber > 0)
                player.CurrentFarm.CurrentBaconResearchPageNumber -= 1;
        }
        state.Data.EmbedItemsHashes = null;
        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }

    [EmbedMenuFunc]
    public static async ValueTask NextResearchpage(InteractionContext ctx)
    {
        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;
        if (player.CurrentFarm.InResearchType == 0)
        {
            if (player.CurrentFarm.CurrentResearchPageNumber < Math.Ceiling(GameData.ResearchData.Count / (double)ItemsPerResearchPage))
                player.CurrentFarm.CurrentResearchPageNumber += 1;
        }
        else
        {
            if (player.CurrentFarm.CurrentBaconResearchPageNumber < Math.Ceiling(GameData.BaconResearchData.Count / (double)ItemsPerResearchPage))
                player.CurrentFarm.CurrentBaconResearchPageNumber += 1;
        }
        state.Data.EmbedItemsHashes = null;
        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }

    [EmbedMenuFunc]
    public static async ValueTask PrevBoostpage(InteractionContext ctx)
    {
        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;

        if (player.CurrentBoostPageNumber > 0)
            player.CurrentBoostPageNumber -= 1;

        state.Data.EmbedItemsHashes = null;
        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }

    [EmbedMenuFunc]
    public static async ValueTask NextBoostpage(InteractionContext ctx)
    {
        var state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);
        var player = state.Data.EggCoopGameData;

        if (player.CurrentBoostPageNumber < Math.Ceiling(GameData.BoostData.Count / (double)8))
            player.CurrentBoostPageNumber += 1;

        state.Data.EmbedItemsHashes = null;
        await HeaderClicks[player.CurrentEmbedMenuPageNum](ctx);
    }

    public static async ValueTask<EmbedBuilder> GetResearchScreen(InteractionContext ctx, UserEmbedState? state = null)
    {
        if (!UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            LoadStartup(ctx);

        if (state is null)
            state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);

        var player = state.Data.EggCoopGameData;
        player.CurrentEmbedMenuPageNum = 1;
        var embed = new EmbedBuilder()
            .AddPage($"Egg Coop Game - Research");

        AddHeader(embed, state);

        Color colorforback = null;
        Color colorforfront = null;
        if (player.CurrentFarm.InResearchType == 0) {
            colorforback = player.CurrentFarm.CurrentResearchPageNumber > 0 ? new Color(0, 0, 0) : new Color(50, 0, 0);
            var currentnum = player.CurrentFarm.CurrentResearchPageNumber;
            var totalnum = (int)(Math.Ceiling(GameData.ResearchData.Count / (double)ItemsPerResearchPage)) - 1;
            colorforfront = currentnum < totalnum ? new Color(0, 0, 0) : new Color(50, 0, 0);
        }
        else {
            colorforback = player.CurrentFarm.CurrentBaconResearchPageNumber > 0 ? new Color(0, 0, 0) : new Color(50, 0, 0);
            var currentnum = player.CurrentFarm.CurrentBaconResearchPageNumber;
            var totalnum = (int)(Math.Ceiling(GameData.BaconResearchData.Count / (double)ItemsPerResearchPage)) - 1;
            colorforfront = currentnum < totalnum ? new Color(0, 0, 0) : new Color(50, 0, 0);
        }

        embed
        .AddRow()
            .SetId("research-change-page-and-type-row")
            .WithStyles(new FlexJustifyContent(JustifyContent.SpaceBetween), new FlexDirection(Direction.Row))
            .WithRow()
                .SetId("research-change-page-buttons")
                .AddButtonWithNoText()
                    .SetId("back-button")
                    .OnClick(PrevResearchpage)
                    .WithStyles(new BackgroundColor(colorforback))
                    .AddText("&#60;")
                .Close()
                .AddButtonWithNoText()
                    .SetId("forward-button")
                    .OnClick(NextResearchpage)
                    .WithStyles(new BackgroundColor(colorforfront))
                    .AddText("&#62;")
                .Close()
            .CloseRow()
            .WithRow()
                .SetId("change-research-type-row")
                .AddButtonWithNoText()
                    .SetId("change-research-type-button")
                    .OnClick(ChangeResearchType)
                    .WithStyles(new BackgroundColor(player.CurrentFarm.InResearchType == 1 ? new Color(0, 150, 0) : new Color("DF3F32")))
                    .AddText(player.CurrentFarm.InResearchType == 0 ? "Switch to Bacon" : "Switch to Common")
                .Close();

        embed
            .AddRow()
                .WithStyles(FlexDirection.Column)
                .SetId("embed-body");

        var count = 0;
        var startingi = player.CurrentFarm.InResearchType == 0 ? player.CurrentFarm.CurrentResearchPageNumber * ItemsPerResearchPage : player.CurrentFarm.CurrentBaconResearchPageNumber * ItemsPerResearchPage;
        var max = player.CurrentFarm.InResearchType == 0 ? GameData.ResearchData.Count : GameData.BaconResearchData.Count;
        for (var i = startingi; i < max; i++)
        {
            count += 1;
            Research research = null;
            research = player.CurrentFarm.InResearchType == 0 ? GameData.ResearchData[i] : GameData.BaconResearchData[i];
            int researchlevel = player.CurrentFarm.InResearchType == 0 ? player.GetCommonResearchLevel(research.Id) : player.GetBaconResearchLevel(research.Id);
            embed.WithRow()
                .WithStyles(
                    FlexDirection.Row,
                    FlexJustifyContent.SpaceBetween,
                    new FlexAlignItems(AlignItem.Stretch),
                    new Margin(top: new Size(Unit.Pixels, 8))
                )
                .SetId($"embed-row-for-research-row-{count}")
                .WithRow()
                    .WithStyles(
                        FlexDirection.Column,
                        FlexJustifyContent.SpaceBetween,
                        new FlexAlignItems(AlignItem.Stretch),
                        new FlexAlignSelf(AlignSelf.FlexStart)
                    )
                    .SetId($"embed-row-for-research-info-{count}")
                    .AddText(research.Name).WithStyles(new FontSize(new Size(Unit.Pixels, 16)))
                    .WithRow()
                        .AddText(research.Description)
                    .CloseRow()
                    .AddProgress()
                        .SetId($"research-progress-{count}")
                        .WithName($"{researchlevel}/{research.MaxLevel}")
                        .SetId($"research-progress-name-{count}")
                        .WithProgressBar((int)(researchlevel * 100.0 / (double)research.MaxLevel))
                            .WithBootStrapClasses(BootstrapBackgroundColorClass.Info)
                    .Close()
                .CloseRow()
                .WithRow()
                    .WithStyles(new FlexAlignSelf(AlignSelf.FlexEnd))
                    .AddButtonWithNoText()
                        .SetId($"research-button-{count}");
            if (((player.CurrentFarm.InResearchType == 0 && research.CostFunc(researchlevel) < player.CurrentFarm.Money) || (player.CurrentFarm.InResearchType == 1 && research.CostFunc(researchlevel) < player.Bacon)) && researchlevel < research.MaxLevel)
                embed.WithStyles(FlexDirection.Column, new BackgroundColor(player.CurrentFarm.InResearchType == 0 ? new Color(0, 175, 0) : new Color("9f1313"))).OnClick(player.CurrentFarm.InResearchType == 0 ? BuyCommonResearch : BuyBaconResearch, research.Id.ToString());
            else
                embed.WithStyles(FlexDirection.Column, new BackgroundColor(new Color(150, 150, 150)));
            if (researchlevel == research.MaxLevel)
            {
                embed.AddText("Max Level")
                        .WithStyles(new FontSize(new Size(Unit.Pixels, 14)))
                        .SetId($"research-button-text3-{count}")
                    .Close()
                .CloseRow();
            }
            else
            {
                var text = "";
                if (player.CurrentFarm.InResearchType == 0)
                    text = $"${Functions.Format(research.CostFunc(researchlevel), Rounding: 2, NoK: true)}";
                else if (player.CurrentFarm.InResearchType == 1)
                    text = $"{Functions.Format(research.CostFunc(researchlevel), WholeNum: true, NoK: true)} :bacon:";
                embed.AddText("Research")
                        .WithStyles(new FontSize(new Size(Unit.Pixels, 14)))
                        .SetId($"research-button-text3-{count}")
                    .AddText(text)
                        .WithStyles(new Margin(left: new Size(Unit.Auto), right: new Size(Unit.Auto)))
                        .SetId($"research-button-text2-{count}")
                    .Close()
                .CloseRow();
            }
            embed.CloseRow();

            if (count >= ItemsPerResearchPage)
                break;
        }

        //.WithStyles(new Padding(new Size(Unit.Pixels, 40), new Size(Unit.Pixels, 40), new Size(Unit.Pixels, 6), new Size(Unit.Pixels, 6)));
        HandleFoxes(embed, state);
        embed.CalculateHashOnItemsForSettingHasChanged();
        embed.CalculateHasChangedForAllItems(state.Data.EmbedItemsHashes);

        state.StoreItemHashes(embed);
        return embed;
    }

    public static async ValueTask<EmbedBuilder> GetHabitatsScreen(InteractionContext ctx, UserEmbedState? state = null)
    {
        if (!UserIdsCurrentlyConnected.ContainsKey(ctx.Member.UserId))
            LoadStartup(ctx);

        if (state is null)
            state = await UserEmbedState.GetFromMemberIdAsync(ctx.Member.Id);

        var player = state.Data.EggCoopGameData;
        player.CurrentEmbedMenuPageNum = 2;
        var embed = new EmbedBuilder()
            .AddPage($"Egg Coop Game - Habitats");

        AddHeader(embed, state);

        embed
            .AddRow()
                .AddText($"Capacity: {Functions.Format(player.CurrentFarm.MaxChickens, OnlyNumbers:true)}")
                    .WithStyles(new Margin(left: new Size(Unit.Auto), right: new Size(Unit.Auto)))
                    .SetId("max-chickens")
            .AddRow()
                .WithStyles(FlexDirection.Column)
                .SetId("embed-body");

        foreach (var pair in player.CurrentFarm.Houses)
        {
            var id = pair.Key;
            var currenthab = GameData.HouseData[pair.Value];
            var nexthab = pair.Value + 1 < GameData.HouseData.Count ? GameData.HouseData[pair.Value + 1] : null;
            embed.WithRow()
                .WithStyles(
                    FlexDirection.Row,
                    FlexJustifyContent.SpaceBetween,
                    new FlexAlignItems(AlignItem.Stretch),
                    new Margin(top: new Size(Unit.Pixels, id == 0 ? 0 : 8))
                )
                .SetId($"row-for-house-{id}")
                .WithRow()
                    .WithStyles(
                        FlexDirection.Column,
                        FlexJustifyContent.SpaceBetween,
                        new FlexAlignItems(AlignItem.Stretch),
                        new FlexAlignSelf(AlignSelf.FlexStart)
                    )
                .SetId($"left-side-row-for-house-{id}");
            if (currenthab is not null) {
                    embed.AddText(currenthab.Name)
                        .WithStyles(new FontSize(new Size(Unit.Pixels, 16)))
                        .SetId($"inner-left-row--name-for-house-{id}")
                    .WithRow()
                        .SetId($"inner-left-row-for-house-{id}")
                        .AddText(Functions.Format(currenthab.Capacity, Rounding: 2, NoK: true))
                            .SetId($"house-capacity-{id}")
                    .CloseRow();
            }
            else
            {
                embed.AddText("none").WithStyles(new FontSize(new Size(Unit.Pixels, 16))).SetId($"inner-left-side-for-house-{id}");
            }
                embed.CloseRow()
                .WithRow()
                    .WithStyles(new FlexAlignSelf(AlignSelf.FlexEnd))
                    .SetId($"right-side-row-for-house-{id}")
                    .AddButtonWithNoText()
                        .SetId($"right-side-row-for-house-button-{id}");
            if (nexthab is not null && nexthab.Cost < player.CurrentFarm.Money)
            {
                embed.WithStyles(FlexDirection.Column, new BackgroundColor(new Color(0, 175, 0))).OnClick(BuyHab, $"{pair.Key}?{nexthab.Id}")
                    .AddText($"{nexthab.Name} ({Functions.Format(nexthab.Capacity, Rounding: 2, NoK: true)})")
                        .SetId($"button-hab-name-capacity-for-house-{id}")
                        .WithStyles(new FontSize(new Size(Unit.Pixels, 14)))
                    .AddText($"${Functions.Format(nexthab.Cost, Rounding: 2, NoK: true)}")
                        .SetId($"button-hab-cost-for-house-{id}")
                        .WithStyles(new Margin(left: new Size(Unit.Auto), right: new Size(Unit.Auto)), new FontSize(new Size(Unit.Pixels, 14)));
            }
            else if (nexthab is null)
            {
                embed.WithStyles(FlexDirection.Column, new BackgroundColor(new Color(150, 150, 150)));
                embed.AddText("Max Habitat Tier")
                    .SetId($"button-hab-name-capacity-for-house-{id}")
                    .WithStyles(new FontSize(new Size(Unit.Pixels, 14)));
            }
            else
            {
                embed.WithStyles(FlexDirection.Column, new BackgroundColor(new Color(150, 150, 150)));
                embed.AddText($"{nexthab.Name} ({Functions.Format(nexthab.Capacity, Rounding: 2, NoK: true)})")
                        .SetId($"button-hab-name-capacity-for-house-{id}")
                        .WithStyles(new FontSize(new Size(Unit.Pixels, 14)))
                    .AddText($"${Functions.Format(nexthab.Cost, Rounding: 2, NoK: true)}")
                        .SetId($"button-hab-cost-for-house-{id}")
                        .WithStyles(new Margin(left: new Size(Unit.Auto), right: new Size(Unit.Auto)), new FontSize(new Size(Unit.Pixels, 14)));
            }
            embed.Close()
            .CloseRow()
            .CloseRow();
        }

        HandleFoxes(embed, state);

        embed.CalculateHashOnItemsForSettingHasChanged();
        embed.CalculateHasChangedForAllItems(state.Data.EmbedItemsHashes);

        state.StoreItemHashes(embed);

        return embed;
    }

    public static EmbedBuilder HandleFoxes(EmbedBuilder embed, UserEmbedState state)
    {
        var player = state.Data.EggCoopGameData;

        if (player.CurrentEmbedMenuPageNum != 0 && player.CurrentEmbedMenuPageNum != 3 && player.CurrentEmbedMenuPageNum != 4)
            return embed;
        // handle the fox (drone) system
        // 25s
        // 35s
        if (DateTime.UtcNow.Subtract(player.LastRegularFoxSpawn).TotalSeconds > 25)
        {
            Random rng = new Random();
            var upperbound = (int)((1000 / player.MsPerTick) * 12);
            if (rng.Next(0, upperbound) == 1)
            {
                int numtospawn = 1;
                if (rng.Next(0, 5) == 0) numtospawn = 2;
                if (rng.Next(0, 25) == 0) numtospawn = 3;

                for (int i = 0; i < numtospawn; i++)
                {
                    // spawn a common fox
                    var fox = new Fox()
                    {
                        IsCommon = true,
                        IsTakenDown = false,
                        Spawned = DateTime.UtcNow,
                        X = rng.Next(10, 40) + 35,
                        Y = 79 + rng.Next(1, 5),//rng.Next(10, 80),
                        Id = FoxIdManager.Generate()
                    };
                    player.Foxes.Add(fox);
                }
                player.LastRegularFoxSpawn = DateTime.UtcNow;
            }
        }

        embed.AddRow().SetId("foxes-row").HasChanged(true);
        // render the foxes
        foreach (var fox in player.Foxes)
        {
            if (fox.IsTakenDown)
            {
                embed.AddText("🦊")
                    .SetId($"fox-{fox.Id}")
                    .WithStyles(
                        new Position(new Size(Unit.Percent, fox.X), top: new Size(Unit.Percent, fox.Y)),
                        new FontSize(new Size(Unit.Pixels, 18))
                    );
                if (fox.Reward.Type == RewardType.Bacon) {
                    embed.AddText($"+{fox.Reward.Amount} :bacon:").SetId($"fox-{fox.Id}-reward").WithStyles(
                        new Position(new Size(Unit.Percent, fox.X-2), top: new Size(Unit.Percent, fox.Y-7)));
                }
                else
                {
                    embed.AddText($"+{Functions.Format(fox.Reward.Amount, Rounding: 2, NoK: true, ExtraSymbol: "$")}").SetId($"fox-{fox.Id}-reward").WithStyles(
                        new Position(new Size(Unit.Percent, fox.X-2), top: new Size(Unit.Percent, fox.Y-7)));
                }
            }
            else
            {
                embed.AddText("🦊").SetId($"fox-{fox.Id}")
                    .WithStyles(
                        new Position(new Size(Unit.Percent, fox.X), top: new Size(Unit.Percent, fox.Y)),
                        new FontSize(new Size(Unit.Pixels, 18))
                    )
                    .OnClick(ClickedFox, fox.Id.ToString());
            }
        }
        return embed;
    }

    private static List<string> HeaderItems = "Home,Research,Habitats,Stats,Prestige,Boosts,Goals,Bacon".Split(",").ToList();
    private static List<Func<InteractionContext, ValueTask>> HeaderClicks = new() { GetGameScreenAsync, GetResearchScreenAsync, GetHabitatsScreenAsync, GetStatsScreenAsync, GetPrestigeScreenAsync, GetBoostsScreenAsync, GetGoalsScreenAsync, GetBaconScreenAsync };

    public static EmbedBuilder AddHeader(EmbedBuilder embed, UserEmbedState state)
    {
        int i = 0;
        var player = state.Data.EggCoopGameData;
        if (state.Data.LastEmbedMenuPageNum != player.CurrentEmbedMenuPageNum)
        {
            state.Data.EmbedItemsHashes = null;
            state.Data.LastEmbedMenuPageNum = player.CurrentEmbedMenuPageNum;
        }

        embed.WithStyles(new FontSize(new Size(Unit.Pixels, 13)));
        embed.AddRow()
            .SetId("top-row-for-menu")
            .WithStyles(
                new FlexAlignItems(AlignItem.Center),
                Width.Full);
        foreach (var item in HeaderItems)
        {
            if (i == state.Data.EggCoopGameData.CurrentEmbedMenuPageNum)
                embed.AddText($"**{item}**")
                    .WithStyles(TextDecoration.UnderLine)
                    .SetId($"text-for-top-menu-{i}")
                    .OnClick(HeaderClicks[i]);
            else
                embed.AddText(item)
                    .SetId($"text-for-top-menu-{i}")
                    .OnClick(HeaderClicks[i]);
            i += 1;
        }
        embed.AddRow()
            .WithStyles(new FlexJustifyContent(JustifyContent.SpaceBetween), new FlexDirection(Direction.Row))
                .SetId("money-bacon-text-row")
                .WithRow()
                    .SetId("money-text-row")
                        .AddText(Functions.Format(state.Data.EggCoopGameData.CurrentFarm.Money, Rounding: 2, NoK: true, ExtraSymbol: "$") + $@" ({Functions.Format(state.Data.EggCoopGameData.CurrentFarm.MoneyGain, AddPlusSign: true, Rounding: 2, NoK: false, ExtraSymbol: "$")}/s)")
                            .SetId("money-text")
                .CloseRow()
                .WithRow()
                    .SetId("money-text-row")
                    .AddText(Functions.Format(state.Data.EggCoopGameData.Bacon, Rounding: 2, NoK: true, Under1KNoDecimals: true) + " 🥓")
                        .SetId("bacon-text")
                .CloseRow();
        return embed;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Valour.Api.Models;
using Valour.Api.Models.Messages.Embeds;

namespace Database.Models.Users;

public enum ModifierType
{
    EggValue,
    EggLayingRate,
    HousingCapacity,
    ChickenProductionPerHab,
    ChickensPerClick,
    ChickenProductionPerHabFactor,
    BonusPerBread,
    BreadGain
}

public enum FarmType
{
    Home,
    Contract
}

public enum EffectType
{
    Multiplicative,
    Additive
}

public class Modifier
{
    public ModifierType Type { get; set; }
    public double Value { get; set; }
    public EffectType effectType { get; set; }

    public Modifier(ModifierType type, double value) {
        Type = type;
        Value = value;
    }
}

public class House
{
    public string Name { get; set; }
    public double Capacity { get; set; }
    public double Cost { get; set; }
    public int Id { get; set; }

    public House(string name, double capacity, double cost, int id)
    {
        Name = name;
        Capacity = capacity;
        Cost = cost;
        Id = id;
    }
}

public class Egg
{
    public string Name { get; set; }
    public double Value { get; set; }
    public string Color { get; set; }

    public double DiscoverAtFarmValue { get; set; }
    public double UnlockAtFarmValue { get; set; }

    public Egg(string name, double value, string color, double discoveratfarmvalue, double unlockatfarmvalue)
    {
        Name = name;
        Value = value;
        Color = color;
        DiscoverAtFarmValue = discoveratfarmvalue;
        UnlockAtFarmValue = unlockatfarmvalue;
    }
}

public class Goal
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Func<UserEggCoopGameData, bool> IsCompletedFunc { get; set; }
    public GeneralReward Reward { get; set; }

    public Goal(int id, string name, string description, GeneralReward reward, Func<UserEggCoopGameData, bool> isCompletedFunc)
    {
        Id = id;
        Name = name;
        Description = description;
        IsCompletedFunc = isCompletedFunc;
        Reward = reward;
    }
}

public class Research
{
    public ushort Id { get; set; }
    public ushort Tier { get; set; }
    public string Name { get; set; }
    public int MaxLevel { get; set; }
    public Func<int, double> CostFunc { get; set; }

    public List<Modifier> Modifiers { get; set; }

    public string Description { get; set; }

    public EffectType LevelsCompound { get; set; }

    public Research(string name, ushort tier, int maxlevel, Func<int, double> costFunc, List<Modifier> modifiers, ushort id, string description, EffectType levelscompound = EffectType.Additive)
    {
        Name = name;
        CostFunc = costFunc;
        Modifiers = modifiers;
        Id = id;
        MaxLevel = maxlevel;
        Description = description;
        Tier = tier;
        LevelsCompound = levelscompound;
    }
}

public enum RewardType
{
    Bacon,
    Dollars,
    Bread
}

public class GeneralReward
{
    public RewardType Type { get; set; }
    public double Amount { get; set; }

    public GeneralReward(RewardType type, double amount)
    {
        Type = type;
        Amount = amount;
    }
}

public class Fox
{
    public bool IsCommon { get; set; }
    public bool IsTakenDown { get; set; }
    public DateTime TakenDownAt { get; set; } = DateTime.MinValue;
    public DateTime Spawned { get; set; } = DateTime.MinValue;
    public int X { get; set; }
    public int Y { get; set; }
    public long Id { get; set; }
    public GeneralReward? Reward { get; set; }

    public void Tick(UserEggCoopGameData player)
    {
        var rng = new Random();
        if (IsTakenDown && DateTime.UtcNow.Subtract(TakenDownAt).TotalMilliseconds > 5000)
        {
            player.Foxes.Remove(this);
        }
        else if (DateTime.UtcNow.Subtract(Spawned).TotalSeconds > 10 && !IsTakenDown)
        {
            if (rng.Next((int)(1000 / player.MsPerTick * 10)) == 0)
            {
                // disappear
                player.Foxes.Remove(this);
            }
        }
    }

    public GeneralReward GetReward(UserEggCoopGameData player)
    {
        var rnd = new Random();
        var baconormoney = rnd.Next(0, 11);
        GeneralReward reward = null;
        if (baconormoney <= 4)
        {
            var value = rnd.Next(1, 1001);
            var amount = 0.0;
            if (value <= 250) amount = 9;
            else if (value <= 850) amount = 20;
            else if (value <= 949) amount = 36;
            else if (value <= 990) amount = 72;
            else amount = 152;

            reward = new(RewardType.Bacon, amount);
        }
        else
        {
            var value = rnd.Next(1, 1001);
            var amount = 0.0;
            if (value <= 250) amount = 0.01;
            else if (value <= 850) amount = 0.035;
            else if (value <= 949) amount = 0.1;
            else if (value <= 990) amount = 0.15;
            else amount = 0.5;

            reward = new(RewardType.Dollars, amount);
        }
        return reward;
    }
}

public class ContractGoal
{
    public GeneralReward GeneralReward { get; set; }
    public double NumberOfEggsRequired { get; set; }
}

public class ContractModifier
{

}

public class Contract
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<GeneralReward> Rewards { get; set; }
}

public class Farm
{
    [Column("money", TypeName = "numeric(64, 4)")]
    public double Money { get; set; } = 0;

    [Column("chickens", TypeName = "numeric(64, 4)")]
    public double Chickens { get; set; } = 0;

    [Column("maxchickens", TypeName = "numeric(64, 4)")]
    public double MaxChickens { get; set; } = 0;

    [Column("chicken", TypeName = "numeric(64, 4)")]
    public double EggLayingRate { get; set; } = 0.05;
    public double FarmValue { get; set; } = 0;
    public ushort EggTypeIndex { get; set; } = 0;
    public double MoneyGain { get; set; } = 0;
    public double ChickenGain { get; set; } = 0;
    public double EggsLaid { get; set; } = 0;
    public int CurrentResearchPageNumber { get; set; } = 0;
    public int CurrentBaconResearchPageNumber { get; set; } = 0;
    public int InResearchType { get; set; } = 0;
    public double ChickensGainedPerClick { get; set; } = 2;
    public Dictionary<ushort, int> ResearchesCompleted { get; set; } = new();
    public Dictionary<ushort, int> Houses { get; set; } = new()
    {
        { 0, 1 },
        { 1, 0 },
        { 2, 0 },
        { 3, 0 }
    };

    [JsonIgnore]
    [NotMapped]
    public double NextChicken { get; set; } = 0;

    public FarmType FarmType { get; set; } = FarmType.Home;
}

public class UserEggCoopGameData
{
    public double TotalPrestigeEarnings { get; set; } = 0;
    public int CurrentEmbedMenuPageNum { get; set; } = 0;
    public double LastBreadGain { get; set; } = 0;
    public double Bread { get; set; } = 0;
    public long Bacon { get; set; } = 0;
    public long FoxesPetted { get; set; } = 0;
    public Dictionary<ushort, int> BaconResearchesCompleted { get; set; } = new();
    public List<int> GoalsDone { get; set; } = new();
    public List<int> CurrentGoals { get; set;} = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public DateTime LastRegularFoxSpawn { get; set; } = DateTime.UtcNow;
    public DateTime LastEliteFoxSpawn { get; set; } = DateTime.UtcNow;
    public List<Fox> Foxes { get; set; } = new();
    public long CurrentFarmId { get; set; } = 0;

    [NotMapped]
    [JsonIgnore]
    public Farm CurrentFarm => Farms[CurrentFarmId];
    public Dictionary<long, Farm> Farms { get; set; } = new();

    public int GetCommonResearchLevel(ushort id)
    {
        if (CurrentFarm.ResearchesCompleted.ContainsKey(id))
            return CurrentFarm.ResearchesCompleted[id];
        return 0;
    }

    public int GetBaconResearchLevel(ushort id)
    {
        if (BaconResearchesCompleted.ContainsKey(id))
            return BaconResearchesCompleted[id];
        return 0;
    }

    [JsonIgnore, NotMapped]
    public double MsPerTick { get; set; } = 300;
}

public class UserEmbedStateData
{
    public UserEggCoopGameData EggCoopGameData { get; set; }

    [JsonIgnore, NotMapped]
    public ConcurrentDictionary<string, ulong> EmbedItemsHashes { get; set; }

    [JsonIgnore, NotMapped]
    public int LastEmbedMenuPageNum { get; set; }
}

public class UserEmbedState
{
    [Key]
    [Column("memberid")]
    public long MemberId { get; set; }

    [Column("data", TypeName = "jsonb")]
    public UserEmbedStateData Data { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="embed">Test</param>
    public void StoreItemHashes(EmbedBuilder embed)
    {
        var oldhashs = Data.EmbedItemsHashes;
        Data.EmbedItemsHashes = new();
        foreach (var page in embed.embed.Pages)
        {
            foreach (var item in page.GetAllItems())
            {
                if (item.ExtraData is not null && item.Id != null)
                {
                    if (item.Id == "money-text" && oldhashs is not null)
                    {
                        var old = oldhashs.ContainsKey(item.Id) ? oldhashs[item.Id] : 0;
                        //Console.WriteLine(@$"{item.Id} old:{old} new:{(ulong)item.ExtraData["hash"]}");
                    }
                    Data.EmbedItemsHashes[item.Id] = (ulong)item.ExtraData["hash"];
                }
            }
        }
        Data.LastEmbedMenuPageNum = Data.EggCoopGameData.CurrentEmbedMenuPageNum;
    }

    public static async Task<UserEmbedState> GetFromMemberIdAsync(long memberId)
    {
        var state = DBCache.Get<UserEmbedState>(memberId);
        if (state is null)
        {
            state = new UserEmbedState()
            {
                MemberId = memberId,
                Data = new()
            };
            state.Data.EggCoopGameData = new();
            DBCache.AddNew(state.MemberId, state);
        }
        if (state.Data.LastEmbedMenuPageNum != state.Data.EggCoopGameData.CurrentEmbedMenuPageNum)
            state.Data.EmbedItemsHashes = new();
        if (state.Data.EggCoopGameData.Farms.Count == 0)
        {
            state.Data.EggCoopGameData.Farms.Add(0, new());
        }
        return state;
    }
}

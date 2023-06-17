using Database.Models.Users;
using PopeAI.Commands.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Valour_Bot.Commands.EggCoopGame;

public static class GameData
{
    public static List<Egg> EggData = new()
    {
        new Egg("Regular", 0.25, "444444", 1, 1),
        new Egg("Superfood", 1.25, "00ff00", 10_000_000, 70_000_000),
        new Egg("Medical", 6.25, "ff0000", 1_000_000_000, 5_300_000_000),
        new Egg("Rocket Fuel", 31.25, "", 40_000_000_000, 240_000_000_000),
        new Egg("Super Material", 150, "", 27_000_000_000_000, 170_000_000_000_000),
        new Egg("Fusion", 700, "", 8_000_000_000_000_000, 50_000_000_000_000_000),
        new Egg("Quantum", 3000, "", 3_000_000_000_000_000_000, 23_000_000_000_000_000_000.0),
        new Egg("Immortality", 12_500, "", 15_000_000_000_000_000_000_000.0, 100_000_000_000_000_000_000_000.0),
        new Egg("Tachyon", 50_000, "", 27_000_000_000_000_000_000_000_000.0, 170_000_000_000_000_000_000_000_000.0),
        new Egg("Graviton", 175_000, "", 150_000_000_000_000_000_000_000_000_000.0, 980_000_000_000_000_000_000_000_000_000.0),
        new Egg("Dilithium", 525_000, "", 760_000_000_000_000_000_000_000_000_000_000.0, 4_800_000_000_000_000_000_000_000_000_000_000.0),
        new Egg("Prodigy", 1_500_000, "", 2_500_000_000_000_000_000_000_000_000_000_000_000.0, 16_000_000_000_000_000_000_000_000_000_000_000_000.0),
        new Egg("Terraform", 10_000_000, "", 3_200_000_000_000_000_000_000_000_000_000_000_000_000.0, 30_000_000_000_000_000_000_000_000_000_000_000_000_000.0),
        new Egg(null, 0, "", 16_000_000_000_000_000_000_000_000_000_000_000_000.0, 16_000_000_000_000_000_000_000_000_000_000_000_000.0)
    };

    public static List<Research> ResearchData = new() {
        new Research("Comfortable Nests", 1, 50, (x => Math.Pow(1.35, x) * 0.5), new() { new(ModifierType.EggLayingRate, 0.1) }, 0, "Increase egg laying rate by 10%"),
        new Research("Nutritional Supplements", 1, 40, (x => Math.Pow(1.425, x) * 12.5), new() { new(ModifierType.EggValue, 0.25) }, 1, "Increase egg value by 25%"),
        new Research("Padded Packaging", 2, 30, (x => Math.Pow(1.425, x) * 410_000), new() { new(ModifierType.EggValue, 0.25) }, 2, "Increases earnings per egg by 25%"),
        new Research("Hen House Remodel", 2, 25, (x => Math.Pow(1.225, x) * 80_000), new() { new(ModifierType.HousingCapacity, 0.05) }, 3, "Increases the capacity of all habs by 5%"),
        new Research("Internal Hatcheries", 2, 35, (x => Math.Pow(1.1, x) * 1500), new() { new(ModifierType.ChickenProductionPerHab, 2) }, 4, "Each hab produces 2 chickens per minute"),
        new Research("Bigger Eggs", 2, 1, (x => 1_750_000.0), new() { new(ModifierType.EggValue, 1) }, 5, "Doubles egg value"),
        new Research("Internal Hatchery Upgrades", 3, 35, (x => Math.Pow(2.15, x) * 8_000_000), new() { new(ModifierType.ChickenProductionPerHab, 5) }, 6, "Each hab produces 5 more chickens per minute"),
        new Research("USDE Prime Certification", 4, 1, (x => 35_000_000_000_000), new() { new(ModifierType.EggValue, 2) }, 14, "Triples Egg Value"),
        new Research("Hen House A/C", 4, 50, (x => Math.Pow(1.325, x) * 3_000_000), new() { new(ModifierType.EggLayingRate, 0.05) }, 7, "Increase egg laying rate by 5%"),
        new Research("Super-Feed Diet", 4, 35, (x => Math.Pow(1.375, x) * 500_000_000), new() { new(ModifierType.EggValue, 0.25) }, 8, "Increase egg value by 25%"),
        new Research("Better Clicking", 4, 18, (x => Math.Pow(3.5, x) * 100_000_000), new() { new(ModifierType.ChickensPerClick, 1) }, 9, "Increases chickens gained per click by 1"),
        new Research("Microlux™ Chicken Suite", 4, 25, (x => Math.Pow(2, x) * 700_000_000), new() { new(ModifierType.HousingCapacity, 0.05) }, 10, "Increases the capacity of all habs by 5%"),
        new Research("Internal Hatchery Expansion", 5, 25, (x => Math.Pow(2.5, x) * 3_000_000_000_000), new() { new(ModifierType.ChickenProductionPerHab, 10) }, 11, "Each hab produces 10 more chickens per minute"),
        new Research("Improved Genetics", 5, 100, (x => Math.Pow(1.375, Math.Min(50, x)) * Math.Pow(1.45, Math.Max(0, x-50)) * 3_500_000_000_000), new() { new(ModifierType.EggValue, 0.15), new(ModifierType.EggLayingRate, 0.15) }, 12, "Increase egg value AND egg laying rate by 15%"),
        new Research("Shell Fortification", 5, 75, (x => Math.Pow(1.23, x) * 12_000_000_000_000_000.0), new() { new(ModifierType.EggValue, 0.15) }, 13, "Increase egg value by 15%"),
        new Research("Even Bigger Eggs", 5, 6, (x => Math.Pow(400, x) * 26_000_000_000_000_000_000.0), new() { new(ModifierType.EggValue, 1) }, 15, "Doubles egg value", EffectType.Multiplicative),
        new Research("Internal Hatchery Expansion", 6, 40, (x => Math.Pow(1.85, x) * 375_000_000_000_000_000.0), new() { new(ModifierType.ChickenProductionPerHab, 25) }, 16, "Each hab produces 25 more chickens per minute"),
        new Research("Genetic Purification", 6, 200, (x => Math.Pow(1.2875, x) * 100_000_000_000_000_000_000.0), new() { new(ModifierType.EggValue, 0.10) }, 17, "Increase egg value by 10%"),
        new Research("Time Compression", 6, 40, (x => Math.Pow(2.6, x) * 500_000_000_000_000_000_000_000.0), new() { new(ModifierType.EggValue, 0.10) }, 18, "Increase egg laying rate by 10%"),
        new Research("Graviton Coating", 7, 9, (x => Math.Pow(250, x) * 70_000_000_000_000_000_000_000_000_000.0), new() { new(ModifierType.EggValue, 1) }, 19, "Doubles egg value", EffectType.Multiplicative),
        new Research("Machine Learning Incubators", 7, 400, (x => Math.Pow(1.087, x) * 1.25e23), new() { new(ModifierType.ChickenProductionPerHab, 5) }, 20, "Each hab produces 5 more chickens per minute"),
        new Research("Crystalline Shelling", 7, 100, (x => Math.Pow(1.28, x) * 400_000_000_000_000_000_000_000_000_000.0), new() { new(ModifierType.EggValue, 0.25) }, 21, "Increase egg value by 25%"),
        new Research("Telepathic Will", 8, 75, (x => Math.Pow(1.53, x) * 1.75e36), new() { new(ModifierType.EggValue, 0.25) }, 22, "Increase egg value by 25%"),
        new Research("Neural Linking", 8, 50, (x => Math.Pow(10, x) * 5e26), new() { new(ModifierType.ChickenProductionPerHab, 50) }, 23, "Each hab produces 50 more chickens per minute"),
    };

    public static List<Research> BaconResearchData = new()
    {
        new Research("Bacon Int. Hatcheries", 0, 50, (x => (x * 3 + 1) * 20), new() { new(ModifierType.ChickenProductionPerHabFactor, 0.075) }, 0, "Increases internal hatchery rate by 7.5%"),
        new Research("Bread Food", 0, 140, (x => (x * 0.06 + 1) * 400), new() { new(ModifierType.BonusPerBread, 0.01) }, 1, "Increases bonus per bread by +1%"),
        new Research("Bread Bread Bread", 0, 50, (x => (x * 75) + 400), new() { new(ModifierType.BreadGain, 0.1) }, 2, "Increases bread gain by +10%"),
        new Research("Better Eggs", 0, 150, (x => (x * 5) + 20), new() { new(ModifierType.EggValue, 0.02) }, 3, "Increases egg value by 2%")
    };

    public static List<House> HouseData = new()
    {
        null,
        new House("Hen House", 500, 50, 1),
        new House("Shack", 1250, 5000, 2),
        new House("Super Shack", 2500, 175_000, 3),
        new House("Short House", 7500, 4_000_000, 4),
        new House("The Standard", 17_500, 300_000_000, 5),
        new House("Long House", 30_000, 20_000_000_000, 6),
        new House("Double Decker", 75_000, 1_000_000_000_000, 7),
        new House("Warehouse", 150_000, 100_000_000_000_000, 8),
        new House("Center", 350_000, 20_000_000_000_000_000, 9),
        new House("Bunker", 800_000, 2_000_000_000_000_000_000, 10),
        new House("Eggkea", 1_500_000, 300_000_000_000_000_000_000.0, 11),
        new House("HAB 1000", 3_000_000, 60_000_000_000_000_000_000_000.0, 12),
        new House("HAB 5000", 15_000_000, 60_000_000_000_000_000_000_000_000_000.0, 13)
    };

    public static List<Goal> GoalData = new()
    {
        new Goal(0, "Egg Up", "Start a new farm with an upgraded egg.", new(RewardType.Bacon, 192), (player => player.CurrentFarm.EggTypeIndex == 0 ? 0 : 1)),
        new Goal(1, "Cute Foxes", "Pet 5 foxes", new(RewardType.Bacon, 72), (player => player.FoxesPetted/5.0)),
        new Goal(2, "Research Expert", "Research all Tier 1 Research", new(RewardType.Bacon, 24), (player => (player.GetCommonResearchLevel(0)+player.GetCommonResearchLevel(1))/90.0)),
        new Goal(3, "Nice Eggs", "Reach Rocket Fuel", new(RewardType.Bacon, 96), (player => player.CurrentFarm.EggTypeIndex/3.0)),
        new Goal(4, "Chickens I", "Have at least 10,000 chickens", new(RewardType.Bacon, 128), (player => player.CurrentFarm.Chickens/10_000.0)),
        new Goal(5, "Chickens II", "Have at least 25,000 chickens", new(RewardType.Bacon, 164), (player => player.CurrentFarm.Chickens/25_000.0)),
        new Goal(6, "Nice Eggs II", "Reach Super Material", new(RewardType.Bacon, 162), (player => player.CurrentFarm.EggTypeIndex/4.0)),
        new Goal(7, "Cute Foxes II", "Pet 25 foxes", new(RewardType.Bacon, 144), (player => player.FoxesPetted/25.0)),
        new Goal(8, "Money", "Have at least $1 trillion", new(RewardType.Bacon, 52), (player => player.CurrentFarm.Money/1_000_000_000_000)),
        new Goal(9, "Bread", "Have at least 25 bread", new(RewardType.Bread, 10), (player => player.Bread/25.0)),
        new Goal(10, "Cute Foxes III", "Pet 100 foxes", new(RewardType.Bacon, 384), (player => player.FoxesPetted/100.0)),
        new Goal(11, "Chickens III", "Have at least 100,000 chickens", new(RewardType.Bacon, 258), (player => player.CurrentFarm.Chickens/100_000.0)),
        new Goal(12, "Money II", "Have at least $1S", new(RewardType.Bacon, 154), (player => player.CurrentFarm.Money/1_000_000_000_000_000_000)),
        new Goal(13, "Chickens IV", "Have at least 250,000 chickens", new(RewardType.Bacon, 384), (player => player.CurrentFarm.Chickens/250_000.0)),
        new Goal(14, "Bread II", "Have at least 2,500 bread", new(RewardType.Bread, 250), (player => player.Bread/2500.0)),
        new Goal(15, "Money III", "Have at least $1o", new(RewardType.Bacon, 850), (player => player.CurrentFarm.Money/1_000_000_000_000_000_000_000_000.0)),
        new Goal(16, "Money IV", "Have at least $1n", new(RewardType.Bacon, 1250), (player => player.CurrentFarm.Money/1_000_000_000_000_000_000_000_000.0)),
    };

    public static List<Boost> BoostData = new()
    {
        new Boost(0, "", "Jimbo's Excellent Bird Feed", "3x earnings for 20min", new() { new(ModifierType.EggValue, 2) }, TimeSpan.FromMinutes(20), 50),
        new Boost(1, "", "Jimbo's Premium Bird Feed", "10x earnings for 15min", new() { new(ModifierType.EggValue, 9) }, TimeSpan.FromMinutes(15), 150),
        new Boost(2, "", "Jimbo's Premium Bird Feed", "10x earnings for 3hr", new() { new(ModifierType.EggValue, 9) }, TimeSpan.FromHours(3), 750),
        new Boost(3, "", "Jimbo's Best  Bird Feed", "50x earnings for 10min", new() { new(ModifierType.EggValue, 49) }, TimeSpan.FromMinutes(10), 2500),
        new Boost(4, "", "Jimbo's Best  Bird Feed", "50x earnings for 2hr", new() { new(ModifierType.EggValue, 49) }, TimeSpan.FromHours(2), 7500),
        new Boost(5, "", "Tachyon Prism", "10x internal hatchery for 15min", new() { new(ModifierType.ChickenProductionPerHabFactor, 9) }, TimeSpan.FromMinutes(15), 50),
        new Boost(6, "", "Large Tachyon Prism", "15x internal hatchery for 3hr", new() { new(ModifierType.ChickenProductionPerHabFactor, 14) }, TimeSpan.FromHours(3), 500),
        new Boost(7, "", "Powerful Prism", "100x internal hatchery for 15min", new() { new(ModifierType.ChickenProductionPerHabFactor, 99) }, TimeSpan.FromMinutes(15), 1000),
        new Boost(8, "", "Epic Tachyon Prism", "100x internal hatchery for 3hr", new() { new(ModifierType.ChickenProductionPerHabFactor, 99) }, TimeSpan.FromHours(3), 5000),
        new Boost(9, "", "Legendary Prism", "1000x internal hatchery for 15min", new() { new(ModifierType.ChickenProductionPerHabFactor, 999) }, TimeSpan.FromMinutes(15), 12000),
        new Boost(10, "", "Supreme Tachyon Prism", "1000x internal hatchery for 1.5hr", new() { new(ModifierType.ChickenProductionPerHabFactor, 999) }, TimeSpan.FromHours(1.5), 25000),
        new Boost(11, "", "Bread Beacon", "5x bread gain for 40min", new() { new(ModifierType.TotalEarningsGainForBreadFactor, 5) }, TimeSpan.FromMinutes(40), 200),
        new Boost(12, "", "Epic Bread Beacon", "50x bread gain for 25min", new() { new(ModifierType.TotalEarningsGainForBreadFactor, 50) }, TimeSpan.FromMinutes(25), 1000),
        new Boost(13, "", "Legendary Bread Beacon", "500x bread gain for 15min", new() { new(ModifierType.TotalEarningsGainForBreadFactor, 500) }, TimeSpan.FromMinutes(15), 10000),
    };
}

public static class Game
{
    public static double GetBreadGain(UserEggCoopGameData player)
    {
        var ten6 = Math.Pow(10, -6);
        // 0.15
        // 0.16
        // 0.17
        // 0.18
        // 0.19
        var breadtogain = Math.Max(0, Math.Pow(ten6 * Math.Min(player.TotalPrestigeEarnings, Math.Pow(10, 12)), 0.16) - Math.Pow(ten6, 0.16));
        breadtogain += Math.Max(0, Math.Pow(ten6 * Math.Min(player.TotalPrestigeEarnings, Math.Pow(10, 21)), 0.175) - Math.Pow(Math.Pow(10, 6), 0.175));
        breadtogain += Math.Max(0, Math.Pow(ten6 * Math.Min(player.TotalPrestigeEarnings, Math.Pow(10, 30)), 0.195) - Math.Pow(Math.Pow(10, 15), 0.1915));
        breadtogain += Math.Max(0, Math.Pow(ten6 * Math.Min(player.TotalPrestigeEarnings, Math.Pow(10, 39)), 0.2125) - Math.Pow(Math.Pow(10, 24), 0.205));
        breadtogain += Math.Max(0, Math.Pow(ten6 * Math.Min(player.TotalPrestigeEarnings, Math.Pow(10, 48)), 0.225) - Math.Pow(Math.Pow(10, 33), 0.215));
        breadtogain += Math.Max(0, Math.Pow(ten6 * Math.Min(player.TotalPrestigeEarnings, Math.Pow(10, 60)), 0.2425) - Math.Pow(Math.Pow(10, 42), 0.23));
        breadtogain *= GetTotalEffectForType(player, ModifierType.BreadGain);
        return breadtogain;
    }
    public static double GetTotalEffectForType(UserEggCoopGameData player, ModifierType modifierType, bool isadditive = false, bool includeboosts = true)
    {
        double total = 1;
        foreach (var pair in player.CurrentFarm.ResearchesCompleted.ToList())
        {
            var research = GameData.ResearchData.First(x => x.Id == pair.Key);
            if (research.Modifiers.Any(x => x.Type == modifierType))
            {
                var value = 0.00;
                if (research.LevelsCompound == EffectType.Additive)
                    value = research.Modifiers.First(x => x.Type == modifierType).GetValue(player) * player.CurrentFarm.ResearchesCompleted[pair.Key];
                else
                    value = Math.Pow(research.Modifiers.First(x => x.Type == modifierType).GetValue(player) + 1, player.CurrentFarm.ResearchesCompleted[pair.Key]) - 1;
                if (!isadditive)
                    total *= value + 1;
                else
                    total += value;
            }
        }
        foreach (var pair in player.BaconResearchesCompleted)
        {
            var research = GameData.BaconResearchData.First(x => x.Id == pair.Key);
            if (research.Modifiers.Any(x => x.Type == modifierType))
            {
                if (!isadditive)
                    total *= (research.Modifiers.First(x => x.Type == modifierType).GetValue(player) * player.BaconResearchesCompleted[pair.Key]) + 1;
                else
                    total += (research.Modifiers.First(x => x.Type == modifierType).GetValue(player) * player.BaconResearchesCompleted[pair.Key]);
            }
        }

        if (includeboosts)
        {
            var boosts = player.CurrentFarm.ActiveBoosts.ToList().Where(x => GameData.BoostData[x.Id].Modifiers.Any(m => m.Type == modifierType)).ToList();
            if (boosts.Count > 0)
            {
                var firstboost = GameData.BoostData[boosts.First().Id];
                if (!isadditive)
                    total *= (firstboost.Modifiers.First(x => x.Type == modifierType).GetValue(player) * boosts.Count) + 1;
                else
                    total += (firstboost.Modifiers.First(x => x.Type == modifierType).GetValue(player) * boosts.Count);
            }
        }

        return total;
    }

    public static async ValueTask ProcessTick(UserEggCoopGameData player, double mspertick = 0)
    {
        // delta time
        var pertick = mspertick > 1 ? mspertick : player.MsPerTick;
        var dt = 1.0 / (1000 / pertick);

        var currentegg = GameData.EggData[player.CurrentFarm.EggTypeIndex];

        var farm = player.CurrentFarm;

        farm.ChickensGainedPerClick = 2 + GetTotalEffectForType(player, ModifierType.ChickensPerClick, true) - 1;

        farm.EggLayingRate = 0.08;
        farm.EggLayingRate *= GetTotalEffectForType(player, ModifierType.EggLayingRate);

        farm.MaxChickens = 0;
        foreach (var pair in farm.Houses)
        {
            if (pair.Value == 0)
                continue;
            var habdata = GameData.HouseData[pair.Value];
            if (habdata is not null)
                farm.MaxChickens += habdata.Capacity;
        }

        farm.MaxChickens *= GetTotalEffectForType(player, ModifierType.HousingCapacity);

        if (farm.Chickens >= farm.MaxChickens)
            farm.Chickens = farm.MaxChickens;
        else
        {
            double chickengain = (GetTotalEffectForType(player, ModifierType.ChickenProductionPerHab, true) - 1) * (double)farm.Houses.Count(x => x.Value != 0);
            chickengain *= GetTotalEffectForType(player, ModifierType.ChickenProductionPerHabFactor);
            farm.ChickenGain = chickengain;
            chickengain *= dt * (1.0 / 60);
            farm.NextChicken += chickengain;
            if (farm.NextChicken > 2)
            {
                var take = Math.Floor(farm.NextChicken);
                farm.NextChicken -= take;
                farm.Chickens += (int)take;
            }
            else if (farm.NextChicken > 1)
            {
                farm.Chickens += 1;
                farm.NextChicken -= 1;
            }
        }

        if (farm.FarmType == FarmType.Home)
            player.LastBreadGain = GetBreadGain(player);

        var breadmuit = player.Bread * (0.1 + GetTotalEffectForType(player, ModifierType.BonusPerBread) - 1) + 1;

        // calcuate moneygain
        var moneygain = ((double)farm.Chickens) * currentegg.Value * farm.EggLayingRate * dt;
        moneygain *= GetTotalEffectForType(player, ModifierType.EggValue);
        moneygain *= breadmuit;
        farm.MoneyGain = moneygain / dt;
        farm.Money += moneygain;
        farm.EggsLaid += ((double)farm.Chickens) * farm.EggLayingRate * dt;

        if (farm.FarmType == FarmType.Home)
        {
            player.TotalPrestigeEarnings += moneygain*GetTotalEffectForType(player, ModifierType.TotalEarningsGainForBreadFactor);
        }

        var p = (double)farm.Chickens;
        p += Math.Pow(farm.MaxChickens, 0.7);
        farm.FarmValue = currentegg.Value * farm.EggLayingRate * 30000 * (farm.EggTypeIndex + 2) * p * 1.85;
        farm.FarmValue *= GetTotalEffectForType(player, ModifierType.EggValue, includeboosts: false);
        farm.FarmValue *= breadmuit;

        foreach (var fox in player.Foxes.ToList())
        {
            fox.Tick(player);
        }

        while (player.CurrentGoals.Count < 3)
        {
            var goalnotdone = GameData.GoalData.FirstOrDefault(x => !player.GoalsDone.Contains(x.Id) && !player.CurrentGoals.Contains(x.Id));
            if (goalnotdone is not null)
                player.CurrentGoals.Add(goalnotdone.Id);
            else
                break;
        }

        foreach (var pair in farm.ActiveBoosts.ToList())
        {
            var boost = GameData.BoostData[pair.Id];
            pair.SecondsLeft -= pertick/1000;
            if (pair.SecondsLeft < 0)
            {
                farm.ActiveBoosts.Remove(pair);
            }
        }

        farm.LastUpdated = DateTime.UtcNow;
    }
}
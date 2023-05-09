using Database.Models.Users;
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
        new Egg(null, 0, "", 125_000_000_000_000_000_000.0, 700_000_000_000_000_000_000.0)
    };

    public static List<Research> ResearchData= new () {
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
        new Research("Better Clicking", 4, 8, (x => Math.Pow(3.5, x) * 300_000_000), new() { new(ModifierType.ChickensPerClick, 1) }, 9, "Increases chickens gained per click by 1"),
        new Research("Microlux™ Chicken Suite", 4, 15, (x => Math.Pow(2, x) * 700_000_000), new() { new(ModifierType.HousingCapacity, 0.05) }, 10, "Increases the capacity of all habs by 5%"),
        new Research("Internal Hatchery Expansion", 5, 25, (x => Math.Pow(2.5, x) * 3_000_000_000_000), new() { new(ModifierType.ChickenProductionPerHab, 10) }, 11, "Each hab produces 10 more chickens per minute"),
        new Research("Improved Genetics", 5, 85, (x => Math.Pow(1.375, Math.Min(50, x)) * Math.Pow(1.45, Math.Max(0, x-50)) * 4_500_000_000_000), new() { new(ModifierType.EggValue, 0.15), new(ModifierType.EggLayingRate, 0.15) }, 12, "Increase egg value AND egg laying rate by 15%"),
        new Research("Shell Fortification", 5, 75, (x => Math.Pow(1.23, x) * 12_000_000_000_000_000.0), new() { new(ModifierType.EggValue, 0.15) }, 13, "Increase egg value by 15%"),
        new Research("Even Bigger Eggs", 5, 6, (x => Math.Pow(450, x) * 26_000_000_000_000_000_000.0), new() { new(ModifierType.EggValue, 1) }, 15, "Doubles egg value", EffectType.Multiplicative),
        new Research("Internal Hatchery Expansion", 6, 40, (x => Math.Pow(1.9, x) * 375_000_000_000_000_000.0), new() { new(ModifierType.ChickenProductionPerHab, 25) }, 16, "Each hab produces 25 more chickens per minute"),
        new Research("Shell Fortification", 6, 125, (x => Math.Pow(1.325, x) * 100_000_000_000_000_000_000.0), new() { new(ModifierType.EggValue, 0.10) }, 17, "Increase egg value by 10%"),
    };

    public static List<Research> BaconResearchData = new()
    {
        new Research("Bacon Int. Hatcheries", 0, 50, (x => (x * 3.75 + 1) * 20), new() { new(ModifierType.ChickenProductionPerHabFactor, 0.05) }, 0, "Increases internal hatchery rate by 5%"),
        new Research("Bread Food", 0, 140, (x => (x * 0.075 + 1) * 400), new() { new(ModifierType.BonusPerBread, 0.01) }, 1, "Increases bonus per bread by +1%")
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
        new House("Center", 350_000, 20_000_000_000_000_000, 9)
    };

    public static List<Goal> GoalData = new()
    {
        new Goal(0, "Egg Up", "Start a new farm with an upgraded egg.", new(RewardType.Bacon, 192), (player => player.EggTypeIndex != 0)),
        new Goal(1, "Cute Foxes", "Pet 5 foxes", new(RewardType.Bacon, 72), (player => player.FoxesPetted >= 5)),
        new Goal(2, "Research Expert", "Research all Tier 1 Research", new(RewardType.Bacon, 24), (player => player.GetCommonResearchLevel(0) == 50 && player.GetCommonResearchLevel(1) == 40)),
        new Goal(3, "Nice Eggs", "Research Rocket Fuel", new(RewardType.Bacon, 96), (player => player.EggTypeIndex == 3)),
        new Goal(4, "Chickens I", "Have at least 10,000 chickens", new(RewardType.Bacon, 128), (player => player.Chickens > 10_000.0)),
        new Goal(5, "Chickens II", "Have at least 25,000 chickens", new(RewardType.Bacon, 164), (player => player.Chickens > 25_000.0))
    };
}

public static class Game
{
    public static double GetBreadGain(UserEggCoopGameData player)
    {
        var ten6 = Math.Pow(10, -6);
        // 0.15
        // 0.16
        var breadtogain = Math.Max(0, Math.Pow(ten6 * Math.Min(player.TotalPrestigeEarnings, Math.Pow(10, 12)), 0.16) - Math.Pow(ten6, 0.16));
        breadtogain += Math.Max(0, Math.Pow(ten6 * Math.Min(player.TotalPrestigeEarnings, Math.Pow(10, 21)), 0.175) - Math.Pow(Math.Pow(10, 6), 0.175));
        breadtogain += Math.Max(0, Math.Pow(ten6 * Math.Min(player.TotalPrestigeEarnings, Math.Pow(10, 30)), 0.19) - Math.Pow(Math.Pow(10, 15), 0.19));
        return breadtogain;
    }
    public static double GetTotalResearchEffectForType(UserEggCoopGameData player, ModifierType modifierType, bool isadditive = false)
    {
        double total = 1;
        foreach (var pair in player.ResearchesCompleted)
        {
            var research = GameData.ResearchData.First(x => x.Id == pair.Key);
            if (research.Modifiers.Any(x => x.Type == modifierType))
            {
                var value = 0.00;
                if (research.LevelsCompound == EffectType.Additive)
                    value = research.Modifiers.First(x => x.Type == modifierType).Value * player.ResearchesCompleted[pair.Key];
                else
                    value = Math.Pow(research.Modifiers.First(x => x.Type == modifierType).Value + 1, player.ResearchesCompleted[pair.Key]) - 1;
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
                    total *= (research.Modifiers.First(x => x.Type == modifierType).Value * player.BaconResearchesCompleted[pair.Key]) + 1;
                else
                    total += (research.Modifiers.First(x => x.Type == modifierType).Value * player.BaconResearchesCompleted[pair.Key]);
            }
        }
        return total;
    }
    public static async ValueTask ProcessTick(UserEggCoopGameData player)
    {
        // delta time
        var dt = 1.0 / (1000 / player.MsPerTick);

        var currentegg = GameData.EggData[player.EggTypeIndex];

        player.ChickensGainedPerClick = 2 + GetTotalResearchEffectForType(player, ModifierType.ChickensPerClick, true) - 1;

        player.EggLayingRate = 0.08;
        player.EggLayingRate *= GetTotalResearchEffectForType(player, ModifierType.EggLayingRate);

        player.MaxChickens = 0;
        foreach (var pair in player.Houses)
        {
            if (pair.Value == 0)
                continue;
            var habdata = GameData.HouseData[pair.Value];
            if (habdata is not null)
                player.MaxChickens += habdata.Capacity;
        }

        player.MaxChickens *= GetTotalResearchEffectForType(player, ModifierType.HousingCapacity);

        if (player.Chickens >= player.MaxChickens)
            player.Chickens = player.MaxChickens;
        else
        {
            double chickengain = (GetTotalResearchEffectForType(player, ModifierType.ChickenProductionPerHab, true) - 1) * (double)player.Houses.Count(x => x.Value != 0);
            chickengain *= GetTotalResearchEffectForType(player, ModifierType.ChickenProductionPerHabFactor);
            player.ChickenGain = chickengain;
            chickengain *= dt * (1.0 / 60);
            player.NextChicken += chickengain;
            if (player.NextChicken > 2)
            {
                var take = Math.Floor(player.NextChicken);
                player.NextChicken -= take;
                player.Chickens += (int)take;
            }
            else if (player.NextChicken > 1)
            {
                player.Chickens += 1;
                player.NextChicken -= 1;
            }
        }

        player.LastBreadGain = GetBreadGain(player);

        var breadmuit = player.Bread * (0.1 + GetTotalResearchEffectForType(player, ModifierType.BonusPerBread) - 1) + 1;

        // calcuate moneygain
        var moneygain = ((double)player.Chickens) * currentegg.Value * player.EggLayingRate * dt;
        moneygain *= GetTotalResearchEffectForType(player, ModifierType.EggValue);
        moneygain *= breadmuit;
        player.MoneyGain = moneygain / dt;
        player.Money += moneygain;
        player.TotalPrestigeEarnings += moneygain;

        // TEST THESE
        var p = (double)player.Chickens;
        p += Math.Pow(player.MaxChickens, 0.7);
        player.FarmValue = currentegg.Value * player.EggLayingRate * 67500 * (player.EggTypeIndex+2) * p;
        player.FarmValue *= GetTotalResearchEffectForType(player, ModifierType.EggValue);
        player.FarmValue *= breadmuit;

        foreach (var fox in player.Foxes.ToList())
        {
            fox.Tick(player);
        }

        player.LastUpdated = DateTime.UtcNow;
    }
}

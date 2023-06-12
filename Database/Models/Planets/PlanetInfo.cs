using System;
using System.ComponentModel.DataAnnotations;

namespace PopeAI.Database.Models.Planets;

public enum PlanetStoragePlan
{
    Free, // up to 1 million messages
    Basic, // up to 20 million messages
    Pro // up to 100 million messages
}

public enum ModuleType
{
    //IdleGame = 1,
    EggCoopGame = 0,
    Xp = 1,
    Coins = 2
}

public class PlanetInfo : DBItem<PlanetInfo>
{
    [Key]
    public long PlanetId { get; set; }
    public int MessagesStored { get; set; }
    public List<ModuleType> Modules { get; set; }

    public bool HasEnabled(ModuleType moduleType)
    {
        if (Modules.Contains(moduleType))
            return true;
        return false;
    }

    public PlanetInfo()
    {
        //StoragePlanId = (int)PlanetStoragePlan.Basic;
        MessagesStored = 0;

    }
}
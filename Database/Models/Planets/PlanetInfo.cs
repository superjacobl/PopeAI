using System;
using System.ComponentModel.DataAnnotations;

namespace PopeAI.Database.Models.Planets;

public enum PlanetStoragePlan
{
    Free, // up to 1 million messages
    Basic, // up to 20 million messages
    Pro // up to 100 million messages
}

public class PlanetInfo : DBItem<PlanetInfo>
{
    [Key]
    public ulong PlanetId { get; set; }
    public ulong MessagesStored { get; set; }

    public PlanetInfo()
    {
        //StoragePlanId = (int)PlanetStoragePlan.Basic;
        MessagesStored = 0;

    }
}
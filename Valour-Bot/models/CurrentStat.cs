using System.ComponentModel.DataAnnotations;

namespace PopeAI.Models
{
    public class CurrentStat
    {
        [Key]
        public ulong PlanetId {get; set;}
        public double NewCoins {get; set;}
        public ulong MessagesSent {get; set;}
        public ulong MessagesUsersSent {get; set;}
<<<<<<< HEAD:Valour Bot/models/currentstat.cs
        public DateTime LastStatUpdate {get; set;}

=======
>>>>>>> 21c70f288fe6f6c852deb9d95db2d905c5250cd6:Valour-Bot/models/CurrentStat.cs
    }
}
using System.ComponentModel.DataAnnotations;
using System;
namespace PopeAI.Models
{
    public class CurrentStat
    {
        [Key]
        public ulong PlanetId {get; set;}
        public double NewCoins {get; set;}
        public ulong MessagesSent {get; set;}
        public ulong MessagesUsersSent {get; set;}
        public DateTime LastStatUpdate {get; set;}

    }
}
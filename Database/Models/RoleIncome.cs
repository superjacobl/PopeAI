using System;
using System.ComponentModel.DataAnnotations;

namespace PopeAI.Models
{
    public class RoleIncomes
    {
        [Key]
        public ulong RoleId {get; set;}
        public int Income {get; set;}
        public string RoleName {get; set;}
        public ulong PlanetId {get; set;}
        public DateTime LastPaidOut {get; set;}
    }
}
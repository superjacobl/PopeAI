using System;
using System.ComponentModel.DataAnnotations;
using Valour.Net.Models;
using System.Threading.Tasks;
using PopeAI.Database;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PopeAI.Models
{
    public class DailyTask
    {
        [Key]
        public ulong Id {get; set;}
        public ulong MemberId {get; set;}
        public double Reward {get; set;}
        public string TaskType {get; set;}
        public double Goal {get; set;}
        public double Done {get; set;}
        public DateTime LastDayUpdated {get; set;}
    }
}
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PopeAI.Models;

namespace PopeAI.Database
{
    public class PopeAIDB : DbContext
    {
        static bool wasconfigcreatedorfound = Client.Check();

        public static string ConnectionString = $"server={Client.Config.Host};port=3306;database={Client.Config.Database};uid={Client.Config.Username};pwd={Client.Config.Password};SslMode=Required;";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseMySql(ConnectionString, ServerVersion.FromString("8.0.20-mysql"), options => options.EnableRetryOnFailure().CharSet(CharSet.Utf8Mb4));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        // These are the database sets we can access
        //public DbSet<ClientPlanetMessage> Messages { get; set; }

        /// <summary>
        /// This is only here to fulfill the need of the constructor.
        /// It does literally nothing at all.
        /// </summary>
        public static DbContextOptions DBOptions;

        public DbSet<User> Users { get; set; }

        public DbSet<ShopReward> ShopRewards { get; set; }

        public DbSet<CurrentStat> CurrentStats { get; set; }

        public DbSet<Stat> Stats { get; set; }
        public DbSet<RoleIncome> RoleIncomes { get; set; }
        public DbSet<Help> Helps { get; set; }

        public PopeAIDB(DbContextOptions options)
        {
            
        }
        public async Task AddStat(string name, double value, ulong PlanetId, PopeAIDB Context) {
            CurrentStat current = await Context.CurrentStats.FirstOrDefaultAsync(x => x.PlanetId == PlanetId);
            if (current == null) {
                CurrentStat newstat = new CurrentStat
                {
                    PlanetId = PlanetId,
                    NewCoins = 0,
                    MessagesSent = 0,
                    MessagesUsersSent = 0
                };
                await Context.CurrentStats.AddAsync(newstat);
                await Context.SaveChangesAsync();
                current = await Context.CurrentStats.FirstAsync(x => x.PlanetId == PlanetId);
            }
            switch (name)
            {
                case "Coins":
                    current.NewCoins += value;
                    break;
                case "UserMessage":
                    current.MessagesUsersSent += (ulong)value;
                    break;
                case "Message":
                    current.MessagesSent += (ulong)value;
                    break;
            }
            await Context.SaveChangesAsync();
        }
        public async Task UpdateRoleIncomes(List<Planet> planets, bool force, PopeAIDB Context) {
            RoleIncome first = await Context.RoleIncomes.FirstOrDefaultAsync();
            if (first == null) {
                first = new RoleIncome();
                first.LastPaidOut = DateTime.UtcNow;
                first.RoleId = 0;
            }
            if (DateTime.UtcNow > first.LastPaidOut.AddHours(1) || first.RoleId == 0 || force) {
                List<PlanetMemberInfo> memberinfo = new List<PlanetMemberInfo>();
                Dictionary<ulong, RoleIncome> RoleIncomeRoleIds = new Dictionary<ulong, RoleIncome>();
                List<ulong> PlanetIds = new List<ulong>();
                foreach (RoleIncome roleincome in Context.RoleIncomes) {
                    RoleIncomeRoleIds.Add(roleincome.RoleId, roleincome);
                    roleincome.LastPaidOut = DateTime.UtcNow;
                    if (PlanetIds.Contains(roleincome.PlanetId) == false) {
                        PlanetIds.Add(roleincome.PlanetId);
                    }
                }
                foreach (ulong planetid in PlanetIds) {
                    memberinfo = await planets.FirstOrDefault(x => x.Id == planetid).GetMembers();
                    CurrentStat current = await Context.CurrentStats.FirstOrDefaultAsync(x => x.PlanetId == planetid);

                    foreach (PlanetMemberInfo member in memberinfo) {
                        foreach (ulong roleid in member.RoleIds) {
                            if (RoleIncomeRoleIds.ContainsKey(roleid)) {
                                RoleIncome roleincome = RoleIncomeRoleIds[roleid];
                                (await Context.Users.FirstOrDefaultAsync(x => x.UserId == member.Member.UserId && x.PlanetId == member.Member.PlanetId)).Coins += roleincome.Income;
                                current.NewCoins += roleincome.Income;
                            }
                        }
                    }
                }
            }
            await Context.SaveChangesAsync();
        }

        public async Task UpdateStats(PopeAIDB Context) {
            Stat first = await Context.Stats.LastOrDefaultAsync();
            if (first == null) {
                first = new Stat();
                first.Time = DateTime.UtcNow;
                first.Id = 0;
            }
            if (Context.CurrentStats.Count() == 0) {
                return;
            }
            Random rnd = new Random();
            if (DateTime.UtcNow > first.Time.AddHours(1) || first.Id == 0) {
                foreach (CurrentStat currentstat in Context.CurrentStats) {
                    Stat newstat = new Stat();
                    newstat.Time = DateTime.UtcNow;
                    newstat.NewCoins = currentstat.NewCoins;
                    newstat.MessagesSent = currentstat.MessagesSent;
                    newstat.PlanetId = currentstat.PlanetId;
                    newstat.MessagesUsersSent = currentstat.MessagesUsersSent;
                    ulong num = (ulong)rnd.Next(1,int.MaxValue);
                    while (await Context.Stats.FirstOrDefaultAsync(x => x.Id == num) != null) {
                        num = (ulong)rnd.Next(1,int.MaxValue);
                    }
                    newstat.Id = num;
                    await Context.Stats.AddAsync(newstat);
                    currentstat.NewCoins = 0;
                    currentstat.MessagesSent = 0;
                    currentstat.MessagesUsersSent = 0;
                }
            }
            await Context.SaveChangesAsync();
        }
    }
}
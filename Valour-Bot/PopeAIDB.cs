using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PopeAI.Models;
using Valour.Net.Models;
using Valour.Net;

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
        public DbSet<RoleIncomes> RoleIncomes { get; set; }
        public DbSet<Help> Helps { get; set; }
        public DbSet<Lottery> Lotteries {get; set;}
        public DbSet<LotteryTicket> LotteryTickets {get; set;}
        public DbSet<DailyTask> DailyTasks {get; set;}
        public Random rnd = new Random();

        public PopeAIDB(DbContextOptions options)
        {
            
        }
        public async Task GenerateNewDailyTasks(PopeAIDB Context, ulong Memberid) {
            string[] types = new string[] {"Messages", "Hourly Claims", "Gamble Games Played", "Dice Games Played"};

            List<DailyTask> toadd = new List<DailyTask>();

            for (int i = 0; i < 3; i++)
            {
                DailyTask task = new DailyTask();
                string tasktype = types[rnd.Next(0,types.Count())];
                while (toadd.Any( x => x.TaskType == tasktype)) {
                    tasktype = types[rnd.Next(0,types.Count())];
                }
                task.TaskType = tasktype;
                int id = rnd.Next(0, int.MaxValue);
                while (await Context.DailyTasks.AnyAsync(x => x.Id == (ulong)id)) {
                    id = rnd.Next(0, int.MaxValue);
                }
                task.Id = (ulong)id;
                task.LastDayUpdated = DateTime.UtcNow;
                task.MemberId = Memberid;
                switch (tasktype)
                {
                    case "Messages":
                        double[] goals = new double[] {10,15,20,25,30,35,40,45,50};
                        task.Goal = goals[rnd.Next(0, goals.Count())];
                        task.Done = 0;
                        double[] rewards = new double[] {50,75,100,125,150,175,200,225,250,275,300};
                        task.Reward = rewards[rnd.Next(0, rewards.Count())];
                        break;
                    case "Hourly Claims":
                        goals = new double[] {3,4,5};
                        task.Goal = goals[rnd.Next(0, goals.Count())];
                        task.Done = 0;
                        rewards = new double[] {50,75,100,125,150,175,200,225,250};
                        task.Reward = rewards[rnd.Next(0, rewards.Count())];
                        break;
                    case "Gamble Games Played":
                        goals = new double[] {5,6,7,8,9,10};
                        task.Goal = goals[rnd.Next(0, goals.Count())];
                        task.Done = 0;
                        rewards = new double[] {50,75,100,125,150,175,200};
                        task.Reward = rewards[rnd.Next(0, rewards.Count())];
                        break;
                    case "Dice Games Played":
                        goals = new double[] {5,6,7,8,9,10};
                        task.Goal = goals[rnd.Next(0, goals.Count())];
                        task.Done = 0;
                        rewards = new double[] {50,75,100,125,150,175,200,225,250,275,300};
                        task.Reward = rewards[rnd.Next(0, rewards.Count())];
                        break;
                }
                toadd.Add(task);
                await Context.AddAsync(task);
            }
        }

        public async Task UpdateDailyTasks(PopeAIDB Context)
        {          
            foreach (DailyTask task in Context.DailyTasks) {
                if (DateTime.UtcNow.Day != task.LastDayUpdated.Day) {
                    Context.DailyTasks.Remove(task);
                }
            }
            await Context.SaveChangesAsync();
            foreach (User user in Context.Users) {
                PlanetMember member = await Cache.GetPlanetMember(user.UserId, user.PlanetId);
                if (await Context.DailyTasks.FirstOrDefaultAsync(x => x.MemberId == member.Id) == null) {
                    // create 3 new tasks
                    await GenerateNewDailyTasks(Context, member.Id);
                }
            }
            await Context.SaveChangesAsync();
        }
        public async Task AddStat(string name, double value, ulong PlanetId, PopeAIDB Context) {
            CurrentStat current = await Context.CurrentStats.FirstOrDefaultAsync(x => x.PlanetId == PlanetId);
            if (current == null) {
                CurrentStat newstat = new CurrentStat();
                newstat.PlanetId = PlanetId;
                newstat.NewCoins = 0;
                newstat.MessagesSent = 0;
                newstat.MessagesUsersSent = 0;
                newstat.LastStatUpdate = DateTime.UtcNow;
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
            RoleIncomes first = await Context.RoleIncomes.FirstOrDefaultAsync();
            if (first == null) {
                first = new RoleIncomes();
                first.LastPaidOut = DateTime.UtcNow;
                first.RoleId = 0;
            }
            if (DateTime.UtcNow > first.LastPaidOut.AddHours(1) || first.RoleId == 0 || force) {
                List<PlanetMemberInfo> memberinfo = new List<PlanetMemberInfo>();
                Dictionary<ulong, RoleIncomes> RoleIncomeRoleIds = new Dictionary<ulong, RoleIncomes>();
                List<ulong> PlanetIds = new List<ulong>();
                foreach (RoleIncomes roleincome in Context.RoleIncomes) {
                    RoleIncomeRoleIds.Add(roleincome.RoleId, roleincome);
                    roleincome.LastPaidOut = DateTime.UtcNow;
                    if (PlanetIds.Contains(roleincome.PlanetId) == false) {
                        PlanetIds.Add(roleincome.PlanetId);
                    }
                }
                foreach (ulong planetid in PlanetIds) {
                    CurrentStat current = await Context.CurrentStats.FirstOrDefaultAsync(x => x.PlanetId == planetid);

                    foreach (PlanetMember member in await (await Cache.GetPlanet(planetid)).GetMembers()) {
                        foreach (ulong roleid in member.RoleIds) {
                            if (RoleIncomeRoleIds.ContainsKey(roleid)) {
                                RoleIncomes roleincome = RoleIncomeRoleIds[roleid];
                                (await Context.Users.FirstOrDefaultAsync(x => x.UserId == member.User_Id && x.PlanetId == member.Planet_Id)).Coins += roleincome.Income;
                                current.NewCoins += roleincome.Income;
                            }
                        }
                    }
                }
            }
            await Context.SaveChangesAsync();
        }

        public async Task UpdateLotteries(Dictionary<ulong, Lottery> lotterycache, PopeAIDB Context) {
            foreach (Lottery lottery in Context.Lotteries) {
                if (DateTime.UtcNow > lottery.EndDate) {
                    lotterycache.Remove(lottery.PlanetId);
                    int total = (int)await Context.LotteryTickets.SumAsync(x => (double)x.Tickets);
                    Random rnd = new Random();
                    ulong WinningTicketNum = (ulong)rnd.Next(1, total+1);
                    ulong currentnum = 1;
                    foreach (LotteryTicket ticket in Context.LotteryTickets.Where(x => x.PlanetId == lottery.PlanetId)) {
                        if (currentnum+ticket.Tickets >= WinningTicketNum) {
                            if (lottery.Type == "message") {
                                await Context.AddStat("Coins", lottery.Jackpot, lottery.PlanetId, Context);
                            }
                            User winninguser = await Context.Users.FirstOrDefaultAsync(x => x.PlanetId == lottery.PlanetId && x.UserId == ticket.UserId);
                            winninguser.Coins += lottery.Jackpot;
                            PlanetMember planetuser = await winninguser.GetAuthor();
                            //await Program.PostMessage(lottery.ChannelId, lottery.PlanetId, $"{planetuser.Nickname} has won the lottery with a jackpot of over {(ulong)lottery.Jackpot} coins!");
                            Context.LotteryTickets.Remove(ticket);
                        }
                        else {
                            currentnum += ticket.Tickets;
                        }
                    Context.Lotteries.Remove(lottery);
                    }
                }
            }
            await Context.SaveChangesAsync();
        }

        public async Task UpdateStats(PopeAIDB Context) {
            CurrentStat first = await Context.CurrentStats.FirstOrDefaultAsync();
            if (first == null) {
                first = new CurrentStat();
                first.LastStatUpdate = DateTime.UtcNow;
            }
            if (Context.CurrentStats.Count() == 0) {
                return;
            }
            Random rnd = new Random();
            if (DateTime.UtcNow > first.LastStatUpdate.AddHours(1)) {
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
                    currentstat.LastStatUpdate = DateTime.UtcNow;
                }
            }
            await Context.SaveChangesAsync();
        }
    }
}
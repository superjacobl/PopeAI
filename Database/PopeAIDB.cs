global using PopeAI.Database.Models.Users;
global using System.ComponentModel.DataAnnotations;
global using Valour.Api.Items.Planets.Members;
global using System.ComponentModel.DataAnnotations.Schema;
global using PopeAI.Database;
global using PopeAI.Database.Caching;
global using PopeAI.Database.Models.Planets;
global using System.Threading.Tasks;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.Linq.Expressions;
global using Valour.Api.Items.Messages;
global using PopeAI.Database.Models.Elements;
global using PopeAI.Database.Managers;
global using PopeAI.Database.Models.Messaging;
global using Microsoft.EntityFrameworkCore;
global using PopeAI.Database.Annotations;
global using PopeAI.Database.Models.Bot;
global using Npgsql.EntityFrameworkCore;
global using NpgsqlTypes;
global using PopeAI.Database.Models.Moderating;
global using PopeAI.Database.Models;

using PopeAI.Models;
using System.Data.Common;
using System.Data;
using PopeAI.Database.Config;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Console;

namespace PopeAI.Database;

/// <summary>A replacement for <see cref="NpgsqlSqlGenerationHelper"/>
/// to convert PascalCaseCsharpyIdentifiers to alllowercasenames.
/// So table and column names with no embedded punctuation
/// get generated with no quotes or delimiters.</summary>
public class NpgsqlSqlGenerationLowercasingHelper : NpgsqlSqlGenerationHelper
{
    //Don't lowercase ef's migration table
    const string dontAlter = "__EFMigrationsHistory";
    static string Customize(string input) => input == dontAlter ? input : input.ToLower();
    public NpgsqlSqlGenerationLowercasingHelper(RelationalSqlGenerationHelperDependencies dependencies)
        : base(dependencies) { }
    public override string DelimitIdentifier(string identifier)
        => base.DelimitIdentifier(Customize(identifier));
    public override void DelimitIdentifier(StringBuilder builder, string identifier)
        => base.DelimitIdentifier(builder, Customize(identifier));
}
public class PopeAIDB : DbContext
{

    public static PooledDbContextFactory<PopeAIDB> DbFactory;

    public static PooledDbContextFactory<PopeAIDB> GetDbFactory()
    {
        string ConnectionString = $"Host={ConfigManger.Config.Host};Database={ConfigManger.Config.Database};Username={ConfigManger.Config.Username};Pwd={ConfigManger.Config.Password}";
        var options = new DbContextOptionsBuilder<PopeAIDB>()
            .UseNpgsql(ConnectionString, options => {
                options.EnableRetryOnFailure();
            })
            .ReplaceService<ISqlGenerationHelper, NpgsqlSqlGenerationLowercasingHelper>()
            .Options;
        return new PooledDbContextFactory<PopeAIDB>(options);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        string ConnectionString = $"Host={ConfigManger.Config.Host};Database={ConfigManger.Config.Database};Username={ConfigManger.Config.Username};Pwd={ConfigManger.Config.Password}";
        options.UseNpgsql(ConnectionString, options => {
            options.EnableRetryOnFailure();
        });
        //options.UseLoggerFactory(loggerFactory);  //tie-up DbContext with LoggerFactory object
        options.ReplaceService<ISqlGenerationHelper, NpgsqlSqlGenerationLowercasingHelper>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PopeAI.Database.Models.Messaging.Message>()
            .HasGeneratedTsVectorColumn(
                p => p.SearchVector,
                "english",
                p => new { p.Content })  // Included properties
            .HasIndex(p => p.SearchVector)
            .HasMethod("GIN"); // Index method
    }

    public static string GenerateSQL()
    {
        using var dbctx = DbFactory.CreateDbContext();
        string sql = dbctx.Database.GenerateCreateScript();
        sql = sql.Replace("numeric(20,0) ", "BIGINT ");
        sql = sql.Replace("CREATE TABLE", "CREATE TABLE IF NOT EXISTS");
        sql = sql.Replace("CREATE INDEX", "CREATE INDEX IF NOT EXISTS");
        sql = sql.Replace("CREATE INDEX IF NOT EXISTS ix_messages_hash ON messages (hash);", "CREATE UNIQUE INDEX IF NOT EXISTS ix_messages_hash ON messages (hash);");
        return sql;
    }

    public static List<T> RawSqlQuery<T>(string query, Func<DbDataReader, T>? map, bool noresult = false)
    {
        using var dbctx = DbFactory.CreateDbContext();
        using DbCommand command = dbctx.Database.GetDbConnection().CreateCommand();
        command.CommandText = query;
        command.CommandType = CommandType.Text;

        //Console.WriteLine(ConfigManger.Config);

        dbctx.Database.OpenConnection();

        using var result = command.ExecuteReader();
        if (!noresult)
        {
            var entities = new List<T>();

            while (result.Read())
            {
                entities.Add(map(result));
            }

            return entities;
        }
        return new List<T>();
    }

    /// <summary>
    /// This is only here to fulfill the need of the constructor.
    /// It does literally nothing at all.
    /// </summary>
    public static DbContextOptions DBOptions;


    public static BotTime botTime;

    public DbSet<DBUser> Users { get; set; }

    public DbSet<CurrentStat> CurrentStats { get; set; }

    public DbSet<Stat> Stats { get; set; }
    public DbSet<Help> Helps { get; set; }
    public DbSet<DailyTask> DailyTasks { get; set; }
    public DbSet<PopeAI.Database.Models.Messaging.Message> Messages { get; set; }
    public DbSet<PlanetInfo> PlanetInfos { get; set; }
    public DbSet<Element> Elements { get; set; }
    public DbSet<Combination> Combinations { get; set; }
    public DbSet<UserInvItem> UserInvItems { get; set; }
    public DbSet<Suggestion> Suggestions { get; set; }
    public DbSet<BotStat> BotStats { get; set; }
    public DbSet<SuggestionVote> SuggestionVotes { get; set; }

    public DbSet<BotTime> BotTimes { get; set; }

    public PopeAIDB(DbContextOptions options)
    {

    }

    /*
   public async Task UpdateRoleIncomes(List<Planet> planets, bool force, PopeAIDB Context)
   {
       return;
       RoleIncomes first = await Context.RoleIncomes.FirstOrDefaultAsync();
       if (first == null)
       {
           first = new RoleIncomes();
           first.LastPaidOut = DateTime.UtcNow;
           first.RoleId = 0;
       }
       if (DateTime.UtcNow > first.LastPaidOut.AddHours(1) || first.RoleId == 0 || force)
       {
           List<PlanetMemberInfo> memberinfo = new List<PlanetMemberInfo>();
           Dictionary<long, RoleIncomes> RoleIncomeRoleIds = new Dictionary<long, RoleIncomes>();
           List<long> PlanetIds = new List<long>();
           foreach (RoleIncomes roleincome in Context.RoleIncomes)
           {
               RoleIncomeRoleIds.Add(roleincome.RoleId, roleincome);
               roleincome.LastPaidOut = DateTime.UtcNow;
               if (PlanetIds.Contains(roleincome.PlanetId) == false)
               {
                   PlanetIds.Add(roleincome.PlanetId);
               }
           }
           foreach (long planetid in PlanetIds)
           {
               int NewCoins = 0;
               CurrentStat current = await Context.CurrentStats.FirstOrDefaultAsync(x => x.PlanetId == planetid);

               // could make this faster
               Planet planet = await Planet.FindAsync(planetid);
               foreach (PlanetMember member in await planet.GetMembersAsync())
               {
                   if (member.User_Id == 735182348615742)
                   {
                       continue;
                   }
                   foreach (long roleid in (await member.GetRolesAsync()).Select(x => x.Id))
                   {
                       if (RoleIncomeRoleIds.ContainsKey(roleid))
                       {
                           RoleIncomes roleincome = RoleIncomeRoleIds[roleid];
                           DBUser? user = DBCache.Get<DBUser>(member.Id);
                           if (user != null)
                           {
                               user.Coins += roleincome.Income;
                               NewCoins += roleincome.Income;
                           }
                       }
                   }
               }
           }
       }
       await Context.SaveChangesAsync();
   }
   public async Task UpdateLotteries(Dictionary<long, Lottery> lotterycache, PopeAIDB Context)
   {
       return;
       foreach (Lottery lottery in Context.Lotteries)
       {
           if (DateTime.UtcNow > lottery.EndDate)
           {
               lotterycache.Remove(lottery.PlanetId);
               int total = (int)await Context.LotteryTickets.SumAsync(x => (double)x.Tickets);
               Random rnd = new Random();
               long WinningTicketNum = (long)rnd.Next(1, total + 1);
               long currentnum = 1;
               foreach (LotteryTicket ticket in Context.LotteryTickets.Where(x => x.PlanetId == lottery.PlanetId))
               {
                   if (currentnum + ticket.Tickets >= WinningTicketNum)
                   {
                       if (lottery.Type == "message")
                       {
                           await StatManager.AddStat(CurrentStatType.Coins, (int)lottery.Jackpot, lottery.PlanetId);
                       }
                       DBUser winninguser = await Context.Users.FirstOrDefaultAsync(x => x.PlanetId == lottery.PlanetId && x.UserId == ticket.UserId);
                       winninguser.Coins += lottery.Jackpot;
                       PlanetMember planetuser = await winninguser.GetMember();
                       //await Program.PostMessage(lottery.ChannelId, lottery.PlanetId, $"{planetuser.Nickname} has won the lottery with a jackpot of over {(long)lottery.Jackpot} coins!");
                       Context.LotteryTickets.Remove(ticket);
                   }
                   else
                   {
                       currentnum += ticket.Tickets;
                   }
                   Context.Lotteries.Remove(lottery);
               }
           }
       }
       await Context.SaveChangesAsync();
   }
   */
}
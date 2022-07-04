using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valour.Api.Items.Users;

namespace PopeAI.Database.Models.Users;

public class DBUser : DBItem<DBUser>
{
    [Key]
    public long Id { get; set; }
    public long UserId { get; set; }
    public long PlanetId { get; set; }
    public double Coins { get; set; }
    public ushort CharsThisMinute { get; set; }
    public ushort PointsThisMinute { get; set; }
    public int TotalPoints { get; set; }
    public int TotalChars { get; set; }
    public double MessageXp { get; set; }
    public double ElementalXp { get; set; }
    public int Messages { get; set; }
    public int ActiveMinutes { get; set; }
    public DateTime LastHourly { get; set; }
    public DateTime LastSentMessage { get; set; }

    [InverseProperty("User")]
    public List<DailyTask> DailyTasks { get; set; }

    [NotMapped]
    public double Xp
    {
        get
        {
            return MessageXp + ElementalXp;
        }
    }

    [NotMapped]
    public PlanetMember? _Member { get; set; }

    [NotMapped]
    public PlanetMember Member
    {
        get
        {
            if (_Member == null)
            {
                _Member = PlanetMember.FindAsync(Id).GetAwaiter().GetResult();
            }
            return _Member;
        }
    }

    public DBUser()
    {

    }

    public DBUser(PlanetMember planetMember)
    {
        MessageXp = 0;
        ElementalXp = 0;
        Coins = 0;
        CharsThisMinute = 0;
        PointsThisMinute = 0;
        TotalPoints = 0;
        TotalChars = 0;
        LastSentMessage = DateTime.UtcNow;
        LastHourly = DateTime.UtcNow.AddHours(-10);
        Id = planetMember.Id;
        UserId = planetMember.UserId;
        PlanetId = planetMember.PlanetId;
    }

    public static string RemoveWhitespace(string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !char.IsWhiteSpace(c))
            .ToArray());
    }

    /// <summary>
    /// Gets the object that matches the id and the type.
    /// Unless you set _readonly to true, make sure you call UpdateDB() on the object after you are done using it!
    /// </summary>
    /// <param name="id">The Primary key of the object</param>
    /// <param name="_readonly">True if the item being returned will not be changed.</param>
    public static new async ValueTask<DBUser?> GetAsync(long id, bool _readonly = false)
    {
        var item = DBCache.Get<DBUser>(id);
        if (item is null)
        {
            if (_readonly)
            {
                using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
                item = await dbctx.Users.Include(x => x.DailyTasks).FirstOrDefaultAsync(x => x.Id == id);
                return item;
            }
            else
            {
                var dbctx = PopeAIDB.DbFactory.CreateDbContext();
                item = await dbctx.Users.Include(x => x.DailyTasks).FirstOrDefaultAsync(x => x.Id == id);
                item!.FromDB = true;
                item.dbctx = dbctx;
                return item;
            }
        }
        return item;
    }

    public void NewMessage(PlanetMessage msg)
    {
        if (LastSentMessage.AddSeconds(60) < DateTime.UtcNow)
        {
            double xpgain = (Math.Log10(PointsThisMinute) - 1) * 3;
            xpgain = Math.Max(0.2, xpgain);
            MessageXp += xpgain;
            ActiveMinutes += 1;
            PointsThisMinute = 0;
            LastSentMessage = DateTime.UtcNow;
        }

        string Content = RemoveWhitespace(msg.Content);

        Content = Content.Replace("*", "");

        ushort Points = 0;

        // each char grants 1 point
        Points += (ushort)Content.Length;

        // if there is media then add 100 points
        if (Content.Contains("https://vmps.valour.gg"))
        {
            Points += 100;
        }

        PointsThisMinute += Points;
        TotalChars += Content.Length;
        TotalPoints += Points;

        Messages += 1;
    }
}

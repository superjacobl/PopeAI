﻿using Database.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopeAI.Database.Models.Users;

public class DBUser : DBItem<DBUser>
{
    [Key]
    public long Id { get; set; }
    public long UserId { get; set; }
    public long PlanetId { get; set; }
    public int Coins { get; set; }
    public short PointsThisMinute { get; set; }
    public int TotalPoints { get; set; }
    public int TotalChars { get; set; }

    [DecimalType]
    public decimal MessageXp { get; set; }

    [DecimalType]
    public decimal ElementalXp { get; set; }

    [DecimalType]
    public decimal GameXp { get; set; }
    public int Messages { get; set; }
    public int ActiveMinutes { get; set; }
    public DateTime LastHourly { get; set; }
    public DateTime LastSentMessage { get; set; }

    public DateOnly LastUpdatedDailyTasks { get; set; }

    [InverseProperty("User")]
    public List<DailyTask> DailyTasks { get; set; }

	[DecimalType]
	public decimal Xp
    {
        get
        {
            return MessageXp + ElementalXp + GameXp;
        }
        set { }
    }

    [NotMapped]
    public int AvgMessageLength
    {
        get
        {
            return (int)Math.Round(TotalChars / ((decimal)Messages));
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
                _Member = PlanetMember.FindAsync(Id, PlanetId).GetAwaiter().GetResult();
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
        GameXp = 0;
        Coins = 0;
        PointsThisMinute = 0;
        TotalPoints = 0;
        TotalChars = 0;
        LastSentMessage = DateTime.UtcNow;
        LastHourly = DateTime.UtcNow.AddHours(-10);
        Id = planetMember.Id;
        UserId = planetMember.UserId;
        PlanetId = planetMember.PlanetId;
        LastUpdatedDailyTasks = DateOnly.FromDateTime(DateTime.UtcNow);
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
                if (item is null) {
                    return null;
                }
                item!.FromDB = true;
                item.dbctx = dbctx;
                return item;
            }
        }
        return item;
    }

    public void NewMessage(Message msg, PlanetInfo? info)
    {
        if (LastSentMessage.AddSeconds(60) < DateTime.UtcNow)
        {
            var bonus = MessageQueueForChannelConversationsManager.GetBonus(MessageQueueForChannelConversationsManager.GetChannelConversationType(msg.ChannelId));
            if (PointsThisMinute <= 5)
                PointsThisMinute += 5;
            if (PointsThisMinute <= 0)
                PointsThisMinute = 5;

            decimal xpgain = (decimal)((Math.Log10(PointsThisMinute) - 1) * 3 * bonus);
            xpgain = Math.Max(0.2m, xpgain);
            
            if (info.HasEnabled(ModuleType.Xp))
                MessageXp += xpgain;

            if (info.HasEnabled(ModuleType.Coins))
            {
                int CoinGain = (int)Math.Max(Math.Round(xpgain * 2), 0);
                Coins += CoinGain;
                StatManager.AddStat(CurrentStatType.Coins, CoinGain, (long)msg.PlanetId);
            }

            if (info.HasEnabled(ModuleType.Xp) || info.HasEnabled(ModuleType.Coins))
                ActiveMinutes += 1;
                PointsThisMinute = 0;

            LastSentMessage = DateTime.UtcNow;
        }

        string Content = RemoveWhitespace(msg.Content);

        Content = Content.Replace("*", "");

        short Points = 0;

        // each char grants 1 point
        Points += (short)Content.Length;

        // if there is media then add 150 points
        if (msg.AttachmentsData is not null && msg.AttachmentsData.Contains("https://cdn.valour.gg/content/"))
        {
            Points += 150;
        }

        if (PointsThisMinute < 10000)
            PointsThisMinute += Points;
        TotalChars += Content.Length;
        TotalPoints += Points;

        Messages += 1;
    }
}

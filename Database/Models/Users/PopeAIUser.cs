using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valour.Api.Items.Users;

namespace PopeAI.Database.Models.Users;

public class DBUser
{
    [Key]
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public ulong PlanetId { get; set; }
    public double Coins { get; set; }
    public int CharsThisMinute { get; set; }
    public int PointsThisMinute { get; set; }
    public int TotalPoints { get; set; }
    public int TotalChars { get; set; }
    public double MessageXp { get; set; }
    public double ElementalXp { get; set; }
    public int Messages { get; set; }
    public int ActiveMinutes { get; set; }

    public DateTime LastHourly { get; set; }

    [NotMapped]
    public double Xp
    {
        get
        {
            return MessageXp + ElementalXp;
        }
    }

    public DateTime LastSentMessage { get; set; }

    // TODO: on startup load all _member in all DBUsers to increase runtime speed

    [NotMapped]
    public PlanetMember? _member { get; set; }

    [NotMapped]
    public PlanetMember member
    {
        get
        {
            if (_member == null)
            {
                _member = GetMember().Result;
            }
            return _member;
        }
    }

    public async Task<PlanetMember> GetMember() {
        PlanetMember planetUser = await PlanetMember.FindAsync(Id);
        return planetUser;
    }

    public static string RemoveWhitespace(string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !Char.IsWhiteSpace(c))
            .ToArray());
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
        UserId = planetMember.User_Id;
        PlanetId = planetMember.Planet_Id;
    }

    public void NewMessage(PlanetMessage msg)
    {
        if (LastSentMessage.AddSeconds(60) < DateTime.UtcNow)
        {
            double xpgain = (Math.Log10(PointsThisMinute) - 1) * 3;
            xpgain = Math.Max(0.2, xpgain);
            MessageXp += (float)xpgain;
            ActiveMinutes += 1;
            PointsThisMinute = 0;
            LastSentMessage = DateTime.UtcNow;
        }

        string Content = RemoveWhitespace(msg.Content);

        Content = Content.Replace("*", "");

        int Points = 0;

        // do char points
        // each char grants 1 point
        Points += Content.Length;

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

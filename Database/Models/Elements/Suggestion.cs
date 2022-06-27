using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PopeAI.Database.Models.Elements;

public class Suggestion
{
    [Key]
    public ulong Id { get; set; }
    public string Element1 { get; set; }
    public string Element2 { get; set; }
    public string Element3 { get; set; }
    public string Result { get; set; }
    public ulong User_Id { get; set; }
    public DateTime Time_Suggested { get; set; }
    public int Ayes { get; set; }
    public int Nays { get; set; }

    public Suggestion()
    {
        Ayes = 0;
        Nays = 0;
        Time_Suggested = DateTime.UtcNow;
    }

    public Suggestion(ulong id, string element1, string element2, string element3, string result, ulong user_Id, DateTime time_Suggested, int ayes, int nays)
    {
        Id = id;
        Element1 = element1;
        Element2 = element2;
        Element3 = element3;
        Result = result;
        User_Id = user_Id;
        Time_Suggested = time_Suggested;
        Ayes = ayes;
        Nays = nays;
    }
}
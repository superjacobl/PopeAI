using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PopeAI.Database.Models.Elements;

public class UserInvItem
{
    [Key]
    [DataType("serial")]
    public int Id { get; set; }
    public ulong UserId { get; set; }

    [VarChar(16)]
    public string Element { get; set; }
    public DateTime TimeFound { get; set; }

    public UserInvItem(int id, ulong userId, string element, DateTime timeFound)
    {
        Id = id;
        UserId = userId;
        Element = element;
        TimeFound = timeFound;
    }

    public UserInvItem(ulong userId, string element)
    {
        UserId = userId;
        Element = element;
        TimeFound = DateTime.UtcNow;
    }
}
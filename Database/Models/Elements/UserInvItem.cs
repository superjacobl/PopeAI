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
    public ulong Id { get; set; }
    public ulong User_Id { get; set; }
    public string Element { get; set; }
    public DateTime TimeFound { get; set; }
}
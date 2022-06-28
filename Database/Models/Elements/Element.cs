using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PopeAI.Database.Models.Elements;

public class Element
{
    [Key]
    public ulong Id { get; set; }

    [VarChar(16)]
    public string Name { get; set; }
    public ulong Found { get; set; }
    public ulong Finder_Id { get; set; }
    public DateTime Time_Created { get; set; }
}
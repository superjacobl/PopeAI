using System;
using System.ComponentModel.DataAnnotations;

using System.Threading.Tasks;
using PopeAI.Database;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PopeAI.Database.Models.Planets;

public class Union2<A, B>
{
    public Union2(A obja)
    {
        A item = obja;
        Console.WriteLine(item);
    }
}

public enum FliterWordType
{
    Delete,
    Warn,
    Mute
}

public class FliterWord
{
    [Key]
    public ulong Id { get; set; }
    public FliterWordType fliterWordType { get; set; }
    public int? SecondsMutedFor { get; set; }
    public string Word { get; set; }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace PopeAI.Database.Models.Elements;

public class SuggestionVote
{
    [Key]
    public ulong Id { get; set; }
    public ulong User_Id { get; set; }
    public ulong Suggestion_Id { get; set; }
}
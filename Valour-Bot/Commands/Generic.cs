using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using PopeAI.Database;
using Microsoft.EntityFrameworkCore;
using PopeAI.Models;
using Valour.Net;
using Valour.Net.Models;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;

/*
stats
roll
*/

namespace PopeAI.Commands.Generic
{
    public class Generic : CommandModuleBase
    {
        PopeAIDB DBContext = new PopeAIDB(PopeAIDB.DBOptions);
        public static Dictionary<ulong, string> ScrambledWords = new Dictionary<ulong, string>();
        static Random rnd = new Random();

        [Command("help")]
        [Summary("Returns all commands")]
        public async Task HelpPage(CommandContext ctx, int page)
        {
            int skip = page*10;
            string content = "| command |\n| :-: |\n";
            foreach (Help help in DBContext.Helps.Skip(skip).Take(10))
            {
                content += $"| {help.Message} |\n";
            }
            await ctx.ReplyAsync(content);
        }

        [Command("help")]
        [Summary("Returns all commands")]
        public async Task Help(CommandContext ctx)
        {
            string content = "";
            foreach (Help help in DBContext.Helps.Take(10))
            {
                content += $"| {help.Message} |\n";
            }
            await ctx.ReplyAsync(content);
        }

        [Command("isdiscordgood")]
        [Summary("Determines if discord is good or bad.")]
        public async Task IsDiscordGood(CommandContext ctx)
        {
            await ctx.ReplyAsync("no, dickcord is bad!");
        }

        [Command("unscramble")]
        [Summary("Unscramble a given word!")]
        public async Task Unscramble(CommandContext ctx)
        {
            List<string> words = new List<string>();
            words.AddRange("people,history,way,art,world,information,map,two,family,government,health,system,computer,meat,year,thanks,music,person,reading,method,data,food,understanding,theory,law,bird,problem,software,control,power,love,internet,phone,television,science,library,nature,fact,product,idea,temperature,investment,area,society,story,activity,industry".Split(","));
            string pickedword = words[rnd.Next(0, words.Count())];
            string scrambed = ScrambleWord(pickedword);
            ScrambledWords.Add(ctx.Member.Id, pickedword);
            await ctx.ReplyAsync($"Unscramble {scrambed} for a reward! (reply with the unscrambed word)");
        }

        [Event("Message")]
        public async Task OnMessage(CommandContext ctx)
        {
            if (ScrambledWords.ContainsKey(ctx.Member.Id))
            {
                if (ScrambledWords[ctx.Member.Id] ==  ctx.Message.Content.ToLower())
                {
                    User user = await DBContext.Users.FirstOrDefaultAsync(x => x.UserId ==  ctx.Message.Author_Id && x.PlanetId ==  ctx.Message.Planet_Id);
                    double reward = (double)rnd.Next(1, 20);
                    await DBContext.AddStat("Coins", reward,  ctx.Message.Planet_Id, DBContext);
                    user.Coins += reward;
                    await DBContext.SaveChangesAsync();
                    await ctx.ReplyAsync($"Correct! Your reward is {reward} coins.");
                }
                else
                {
                    await ctx.ReplyAsync($"Incorrect. The correct word was {ScrambledWords[ctx.Member.Id]}");
                }
                ScrambledWords.Remove(ctx.Member.Id);
            }
        }

        static string ScrambleWord(string word)
        {
            char[] chars = new char[word.Length];
            Random rand = new Random();
            int index = 0;
            while (word.Length > 0)
            { // Get a random number between 0 and the length of the word. 
                int next = rand.Next(0, word.Length - 1); // Take the character from the random position 
                                                          //and add to our char array. 
                chars[index] = word[next];                // Remove the character from the word. 
                word = word.Substring(0, next) + word.Substring(next + 1);
                ++index;
            }
            return new String(chars);
        }
    }
}
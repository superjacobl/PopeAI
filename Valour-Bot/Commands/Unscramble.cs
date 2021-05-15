using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PopeAI.Database;
using Microsoft.EntityFrameworkCore;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;

namespace PopeAI.Commands.Unscramble
{
    public class Unscramble : CommandModuleBase
    {
        public static Dictionary<ulong, string> ScrambledWords = new Dictionary<ulong, string>();
        static Random rnd = new Random();

        [Command("unscramble")]
        [Summary("Unscramble a given word!")]
        public async Task UnscrambleStart(CommandContext ctx)
        {
            List<string> words = new List<string>();
            words.AddRange("random,channel,planet,valour,discord,youtube,google,firefox,github,bots,discordbot,valourbot,people,history,way,art,world,information,map,two,family,government,health,system,computer,meat,year,thanks,music,person,reading,method,data,food,understanding,theory,law,bird,problem,software,control,power,love,internet,phone,television,science,library,nature,fact,product,idea,temperature,investment,area,society,story,activity,industry".Split(","));
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
                if (ScrambledWords[ctx.Member.Id] == ctx.Message.Content.ToLower())
                {
                    User user = await Client.DBContext.Users.FirstOrDefaultAsync(x => x.UserId == ctx.Message.Author_Id && x.PlanetId == ctx.Message.Planet_Id);
                    double reward = (double)rnd.Next(1, 20);
                    await Client.DBContext.AddStat("Coins", reward, ctx.Message.Planet_Id, Client.DBContext);
                    user.Coins += reward;
                    await Client.DBContext.SaveChangesAsync();
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
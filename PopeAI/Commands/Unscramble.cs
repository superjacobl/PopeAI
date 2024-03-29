namespace PopeAI.Commands.Unscramble
{
    public class Unscramble : CommandModuleBase
    {
        public static Dictionary<long, string> ScrambledWords = new Dictionary<long, string>();
        static Random rnd = new Random();

        static List<string> words = "random,channel,planet,valour,discord,youtube,google,firefox,github,bots,discordbot,valourbot,people,history,way,art,world,information,map,two,family,government,health,system,computer,meat,year,thanks,music,person,reading,method,data,food,understanding,theory,law,bird,problem,software,control,power,love,internet,phone,television,science,library,nature,fact,product,idea,temperature,investment,area,society,story,activity,industry,element,planet".Split(",").ToList();

        [Command("unscramble")]
        [Alias("un")]
        [Summary("Unscramble a given word!")]
        public async Task GetUnscrambleAsync(CommandContext ctx)
        {
            ctx.ReplyAsync("The Unscramble game has been disabled, but will be reenabled soon.");
            EmbedBuilder embed = new EmbedBuilder()
                .AddPage("Unscramble Game")
                    .AddRow()
                        .AddButton("Load Embed")
                            .OnClickSendInteractionEvent("Unscramble-Load");
            ctx.ReplyAsync(embed);
        }

        [Interaction(EmbedIteractionEventType.ItemClicked, interactionElementId: "Unscramble-Load")]
        public async Task OnUnscrambleLoad(InteractionContext ctx)
        {
            await using var user = await DBUser.GetAsync(ctx.Member.Id);
            ctx.UpdateEmbedForUser(await GetUnscrambleEmbedAsync(ctx, user), ctx.Member.UserId);
        }

        public async Task<EmbedBuilder> GetUnscrambleEmbedAsync(IContext ctx, DBUser user)
        {
            string pickedword = words[rnd.Next(0, words.Count())];
            string scrambed = ScrambleWord(pickedword);
            ScrambledWords[ctx.Member.Id] = pickedword;
            EmbedBuilder embed = new EmbedBuilder().AddPage("Unscramble Game").AddRow();
            embed.AddText("The Unscramble game has been disabled, but will be reenabled soon.");
            return embed;
            if (user is null)
                return embed;

            embed
                    .AddText(text: $"Your Coins: {user.Coins}")
                .AddRow()
                    .AddForm("Unscramble")
                        .AddRow()
                            .AddInputBox("input", $"Unscramble {scrambed} for a reward!", "Your Answer", keepvalueonupdate: false)
                        .AddRow()
                            .AddButton(text: "Submit")
                                .OnClickSubmitForm()
                    .EndForm();
            return embed;
        }

        [Interaction(EmbedIteractionEventType.FormSubmitted, "Unscramble")]
        public async Task UnscrambleFormSubmitted(InteractionContext ctx)
        {
            await using var user = await DBUser.GetAsync(ctx.Member.Id);
            if (user is null || true)
                return;

            if (!ScrambledWords.ContainsKey(ctx.Member.Id))
            {
                var embed = await GetUnscrambleEmbedAsync(ctx, user);
                ctx.UpdateEmbedForUser(embed, ctx.Member.UserId);
                return;
            }
            if (ctx.Event.FormData.Count == 0 || ctx.Event.FormData[0].Value is null || ScrambledWords[ctx.Member.Id] != ctx.Event.FormData[0].Value.ToLower())
            {
                string before = ScrambledWords[ctx.Member.Id];
                var embed = await GetUnscrambleEmbedAsync(ctx, user);
                embed.AddRow().AddText(text: $"Incorrect. The correct word was {before}");
                ctx.UpdateEmbedForUser(embed, ctx.Member.UserId);
            }
            else
            {
                var embed = await GetUnscrambleEmbedAsync(ctx, user);
                int reward = rnd.Next(1, 3);
                await StatManager.AddStat(CurrentStatType.Coins, (int)reward, ctx.Planet.Id);
                user.Coins += reward;
                user.GameXp += 0.35m;
                embed.AddRow().AddText(text: $"Correct! Your reward is {reward} coins & 0.35xp.");
                ctx.UpdateEmbedForUser(embed, ctx.Member.UserId);
            }
        }

        static string ScrambleWord(string word2)
        {
            char[] chars = new char[word2.Length];
            Random rand = new Random();
            int index = 0;
            string word = word2;
            while (word.Length > 0)
            { // Get a random number between 0 and the length of the word. 
                int next = rand.Next(0, word.Length - 1); // Take the character from the random position 
                                                          //and add to our char array. 
                chars[index] = word[next];                // Remove the character from the word. 
                word = word.Substring(0, next) + word.Substring(next + 1);
                ++index;
            }
            if (word2 == new String(chars))
            {
                return ScrambleWord(word2);
            }
            return new String(chars);
        }
    }
}
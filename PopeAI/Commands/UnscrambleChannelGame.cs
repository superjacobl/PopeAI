namespace PopeAI.Commands.Unscramble
{
    public class UnscrambleGame 
    {
        public string CorrectWord { get; set; }
        public string ScrambledWord { get; set; }
        public long MessageId { get; set; }
        public long ChannelId { get; set; }
        public long PlanetId { get; set; }
        public long Count { get; set; }
        public DateTime LastTimeCorrect { get; set; }
        public DateTime LastTimeSomeoneInputed { get; set; }
        public Dictionary<long, long> PlayersIdsToMemberIds { get; set; }

        public async ValueTask<EmbedBuilder> GetEmbed(InteractionContext ctx, bool ifwon = false, string correctword = "", string extratext = "")
        {
            var embed = new EmbedBuilder()
                .AddPage("Unscramble Game (channel version)");

            if (ifwon) 
            {
                int reward = UnscrambleChannelGame.rnd.Next(1*PlayersIdsToMemberIds.Count, 3*PlayersIdsToMemberIds.Count);
                decimal xpreward = 0.35m*((decimal)PlayersIdsToMemberIds.Count);
                await using var user = await DBUser.GetAsync(ctx.Member.Id);

                user.Coins += reward;
                user.GameXp += xpreward;

                embed.AddRow().AddText(text:$"{ctx.Member.Nickname} guessed the correct word which was {correctword}! They receive {reward} coins & {Functions.Format(xpreward)}xp.");
                await StatManager.AddStat(CurrentStatType.Coins, (int)reward, ctx.Planet.Id);
            }
            if (extratext != "")
                embed.AddRow().AddText(text:extratext);
            embed.AddRow()
                .AddForm(EmbedItemPlacementType.RowBased, "Unscramble-Channel")
                    .AddRow()
                        .AddInputBox($"input-{Count}", $"Unscramble {ScrambledWord} first for a reward!", "Your Answer", keepvalueonupdate: false)
                    .AddRow()
                        .AddButton(text: "Submit", isSubmitButton: true)
                .EndForm();
            return embed;
        }
        
        public async ValueTask NewGame(InteractionContext ctx, bool ifwon = false, string correctword = "", string extratext = "")
        {
            CorrectWord = UnscrambleChannelGame.words[UnscrambleChannelGame.rnd.Next(0, UnscrambleChannelGame.words.Count())];
            ScrambledWord = UnscrambleChannelGame.ScrambleWord(CorrectWord);
            
            Count += 1;
            ctx.UpdateEmbedForChannel(await GetEmbed(ctx, ifwon, correctword, extratext));
        }

        public async ValueTask ProcessInteraction(InteractionContext ctx)
        {
            if (ctx.Event.FormData.First().Value is null || ctx.Event.FormData.First().Value == "") 
            {
                ctx.UpdateEmbedForUser(await GetEmbed(ctx, extratext:"Incorrect, try again!"));
                return;
            }

            if (long.Parse(ctx.Event.FormData.First().ElementId.Split("-")[1]) < Count)
            {
                return;
            }
            if (ctx.Event.FormData.First().Value.ToLower() == CorrectWord)
            {
                string correctword = CorrectWord;
                LastTimeCorrect = DateTime.UtcNow;
                await NewGame(ctx, true, correctword);
            }
            else if (Count == 0) {
                await NewGame(ctx);
            }
            else {
                ctx.UpdateEmbedForUser(await GetEmbed(ctx, extratext:"Incorrect, try again!"));
            }
        }
    }

    public class UnscrambleChannelGame : CommandModuleBase
    {
        public static Dictionary<long, UnscrambleGame> Games = new();
        public static Random rnd = new Random();
        public static List<string> words = "random,channel,planet,valour,discord,youtube,valour.net,vtech,facebook,google,television,temperature,investment,stock,market,economy,emperor,president,seantor,community,development,equipment,analysis,firefox,github,bots,interaction,message,invite,referral,friend,discordbot,valourbot,people,history,way,art,world,information,map,two,family,government,health,system,computer,meat,year,thanks,music,person,reading,method,data,food,understanding,theory,law,bird,problem,software,control,power,love,internet,phone,television,science,library,nature,fact,product,idea,temperature,investment,area,society,story,activity,industry,element,planet".Split(",").ToList();

        [Event(EventType.OnChannelWatching)]
        public async Task OnChannelWatching(ChannelWatchingContext ctx)
        {
            foreach(var game in Games.Where(x => x.Value.ChannelId == ctx.channelWatchingUpdate.ChannelId).Select(x => x.Value)) {
                Dictionary<long, long> newdict = new();
                foreach(var pair in game.PlayersIdsToMemberIds)
                {
                    if (ctx.channelWatchingUpdate.UserIds.Contains(pair.Key))
                    {
                        newdict[pair.Key] = pair.Value;
                    }
                }
                game.PlayersIdsToMemberIds = newdict;
                if (DateTime.UtcNow.Subtract(game.LastTimeCorrect).TotalSeconds > 15) {
                    var c = new InteractionContext() {
                        Channel = ctx.Channel,
                        Event = new() {
                            MessageId = game.MessageId
                        }
                    };
                    await game.NewGame(c, extratext:"No one guessed the correct word within 15 seconds!");
                    game.LastTimeCorrect = DateTime.UtcNow;
                }
                if (DateTime.UtcNow.Subtract(game.LastTimeSomeoneInputed).TotalSeconds > 60) {
                    Games.Remove(game.MessageId);
                    var message = await PlanetMessage.FindAsync(game.MessageId, game.ChannelId, game.PlanetId);
                    message.DeleteAsync();
                }
            }
        }

        [Command("unscramblechannel")]
        [Alias("unc")]
        [Summary("Unscramble a given word!")]
        public async Task GetUnscrambleAsync(CommandContext ctx)
        {
            EmbedBuilder embed = new EmbedBuilder().AddPage("Unscramble Game (channel version)").AddRow().AddButton("Unscramble-channel-Load", text: "Play!");
            ctx.ReplyAsync(embed);
        }

        [Interaction(EmbedIteractionEventType.ButtonClick, interactionElementId: "Unscramble-channel-Load")]
        public async Task OnUnscrambleLoad(InteractionContext ctx)
        {
            UnscrambleGame game = null;
            if (!Games.ContainsKey(ctx.Event.MessageId))
            {
                game = new() {
                    MessageId = ctx.Event.MessageId,
                    PlayersIdsToMemberIds = new(),
                    Count = 0,
                    LastTimeCorrect = DateTime.UtcNow,
                    ChannelId = ctx.Event.ChannelId,
                    PlanetId = ctx.Planet.Id
                };
                Games[ctx.Event.MessageId] = game;
                game.NewGame(ctx);
            }
            else
                game = Games[ctx.Event.MessageId];
            game.LastTimeSomeoneInputed = DateTime.UtcNow;
            game.PlayersIdsToMemberIds[ctx.Member.UserId] = ctx.Member.Id;
        }

        [Interaction(EmbedIteractionEventType.FormSubmitted, "Unscramble-Channel")]
        public async Task UnscrambleFormSubmitted(InteractionContext ctx)
        {
            UnscrambleGame game = null;
            if (!Games.ContainsKey(ctx.Event.MessageId))
            {
                game = new() {
                    MessageId = ctx.Event.MessageId,
                    PlayersIdsToMemberIds = new(),
                    Count = 0,
                    LastTimeCorrect = DateTime.UtcNow,
                    ChannelId = ctx.Event.ChannelId,
                    PlanetId = ctx.Planet.Id
                };
                Games[ctx.Event.MessageId] = game;
                game.NewGame(ctx);
            }
            else
                game = Games[ctx.Event.MessageId];
            game.PlayersIdsToMemberIds[ctx.Member.UserId] = ctx.Member.Id;
            game.LastTimeSomeoneInputed = DateTime.UtcNow;
            await game.ProcessInteraction(ctx);
        }

        public static string ScrambleWord(string word2)
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
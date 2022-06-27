namespace PopeAI.Commands.Elemental;

public class Elemental : CommandModuleBase
{
    public static IdManager idManager = new();
    public static PopeAIDB dbctx = new PopeAIDB(PopeAIDB.DBOptions);
    public static ConcurrentDictionary<ulong, Combination> FailedCombinations = new();
    static Random rnd = new Random();

    [Command("suggest")]
    public static async Task SuggestAynsc(CommandContext ctx, string result) 
    {
        if (!FailedCombinations.Keys.Contains(ctx.Member.User_Id)) {
            await ctx.ReplyAsync("You have not came across a new combination yet!");
            return;
        }

        FailedCombinations.Remove(ctx.Member.User_Id, out Combination _Combination);

        Suggestion suggestion = new() {
            Id = idManager.Generate(),
            Element1 = _Combination.Element1,
            Element2 = _Combination.Element2,
            Result = result,
            User_Id = ctx.Member.User_Id
        };

        if (_Combination.Element3 != null) {
            suggestion.Element3 = _Combination.Element3;
        }

        await dbctx.AddAsync(suggestion);
        await dbctx.SaveChangesAsync();

        ctx.ReplyAsync("Successfully added the suggestion!");
    }

    [Command("vote")]
    public static async Task VoteAynsc(CommandContext ctx)
    {
        EmbedBuilder b = await _VoteAynsc(ctx.Member);
        if (b != null) {
            ctx.ReplyAsync(b);
        }
    }
    public static async Task<EmbedBuilder> _VoteAynsc(PlanetMember member) 
    {
        EmbedBuilder embed = new EmbedBuilder();
        EmbedPageBuilder page = new EmbedPageBuilder();

        List<Suggestion> suggestions = await dbctx.Suggestions.OrderBy(x => x.Time_Suggested).Take(20).ToListAsync();

        if (suggestions.Count == 0) {
            return null;
        }
        
        int i = 0;
        foreach(Suggestion suggestion in suggestions) {
            bool canvote = true;
            if (await dbctx.SuggestionVotes.AnyAsync(x => x.Suggestion_Id == suggestion.Id && x.User_Id == member.User_Id)) {
                canvote = false;
            }
            string text = "";
            if (suggestion.Element3 == null) {
                text = $"{suggestion.Element1} + {suggestion.Element2} = {suggestion.Result} ({suggestion.Ayes}-{suggestion.Nays})";
            }
            else {
                text = $"{suggestion.Element1} + {suggestion.Element2} + {suggestion.Element3} = {suggestion.Result} ({suggestion.Ayes}-{suggestion.Nays})";
            }
            //page.AddText(null, "", false);
            if (i != 0) {
                page.AddText(text:"&nbsp;");
            }
            page.AddText(text:text);
            if (canvote) {
                page.AddButton($"YesVoteFromSuggestion:{suggestion.Id}", "Yes", inline:true, color:"007F0E");
                page.AddButton($"NoVoteFromSuggestion:{suggestion.Id}", "No", inline:true, color:"7F0000");
            }
            else {
                page.AddButton($"YesVoteFromSuggestion:{suggestion.Id}", "Yes", inline:true);
                page.AddButton($"NoVoteFromSuggestion:{suggestion.Id}", "No", inline:true);
            }
            i += 1;
            if (i >= 5) {
                break;
            }
        }
        embed.AddPage(page);
        return embed;
    }

    [Interaction("")]
    public static async Task InteractionAynsc(InteractionContext ctx) 
    {
        PlanetMember member = await PlanetMember.FindAsync(ctx.Event.Author_Member_Id);
        if (member.User_Id != 735182348615742) {
            return;
        }
        string elementname = ctx.Event.Element_Id;
        if (elementname.Contains("VoteFromSuggestion")) {
            ulong suggestid = ulong.Parse(elementname.Split("Suggestion:")[1]);
            Suggestion suggestion = await dbctx.Suggestions.FirstOrDefaultAsync(x => x.Id == suggestid);
            member = await PlanetMember.FindAsync(ctx.Event.Member_Id);
            if (await dbctx.SuggestionVotes.AnyAsync(x => x.Suggestion_Id == suggestion.Id && x.User_Id == member.User_Id)) {
                return;
            }
            if (elementname.Contains("YesVoteFromSuggestion")) {
                suggestion.Ayes += 1;
            }
            if (elementname.Contains("NoVoteFromSuggestion")) {
                suggestion.Nays += 1;
            }
            SuggestionVote suggestionVote = new() {
                Id = idManager.Generate(),
                User_Id = ctx.Member.User_Id,
                Suggestion_Id = suggestid
            };
            await dbctx.AddAsync(suggestionVote);

            int votes = suggestion.Ayes+suggestion.Nays;
            if (votes >= 2) {
                bool approved = false;
                if (suggestion.Nays != 0) {
                    // need 2/3 vote to approve
                    if (suggestion.Ayes/suggestion.Nays > 0.66) {
                        approved = true;
                    }
                }
                else {
                    approved = true;
                }

                if (approved) {
                    Combination combination = new(){
                        Element1 = suggestion.Element1,
                        Element2 = suggestion.Element2,
                        Result = suggestion.Result,
                        Time_Created = DateTime.UtcNow
                    };

                    if (suggestion.Element3 != "") {
                        combination.Element3 = suggestion.Element3;
                    }
                    
                    await dbctx.Combinations.AddAsync(combination);

                    Element element = await dbctx.Elements.FirstOrDefaultAsync(x => x.Name == combination.Result);
                    if (element == null) {
                        Element _element = new() {
                            Name = suggestion.Result,
                            Found = 0,
                            Finder_Id = suggestion.User_Id,
                            Time_Created = DateTime.UtcNow
                        };
                        await dbctx.Elements.AddAsync(_element);
                    }

                    List<SuggestionVote> _votes = await dbctx.SuggestionVotes.Where(x => x.Suggestion_Id == suggestion.Id).ToListAsync();

                    dbctx.SuggestionVotes.RemoveRange(_votes);

                    dbctx.Suggestions.Remove(suggestion);

                    ctx.ReplyAsync($"Enought votes were reached ({suggestion.Ayes}-{suggestion.Nays}) for this suggestion to be accepted!");

                }
            }

            await dbctx.SaveChangesAsync();
            
            PlanetMessage message = new() {
                Channel_Id = ctx.Event.Channel_Id,
                Id = ctx.Event.Message_Id
            };
            message.DeleteAsync();
            EmbedBuilder b = await _VoteAynsc(ctx.Member);
            if (b != null) {
                ctx.ReplyAsync(b);
            }
            
        }
    }

    [Command("test")]
    [Alias("tes")]
    public static async Task TestAynsc(CommandContext ctx) 
    {
        if (ctx.Member.User_Id != 735182334984193) {
            return;
        }
        EmbedBuilder embed = new EmbedBuilder();
        EmbedPageBuilder page = new EmbedPageBuilder();
        page.AddText("Currently Combining", "Water");
        page.AddInputBox("", "Element To Combine");
        page.AddButton("Submit", "Combine");
        embed.AddPage(page);
        await ctx.ReplyAsync(embed);
    }

    [Command("createlement")]
    [Alias("ce")]
    public static async Task createlementAynsc(CommandContext ctx, string name) 
    {
        if (ctx.Member.User_Id != 735182334984193) {
            return;
        }

        Element element = new() {
            Name = name,
            Found = 0,
            Finder_Id = ctx.Member.User_Id,
            Time_Created = DateTime.UtcNow
        };
        
        await dbctx.Elements.AddAsync(element);
        await dbctx.SaveChangesAsync();

        await ctx.ReplyAsync("Successfully created the element");
    }

    [Command("creatcombination")]
    [Alias("cc")]
    public static async Task creatcombinationAynsc(CommandContext ctx, string element1, string element2, string element3, string result) {
        await _creatcombinationAynsc(ctx, element1, element2, element3, result);
    }
    [Command("creatcombination")]
    [Alias("cc")]
    public static async Task creatcombinationAynsc(CommandContext ctx, string element1, string element2, string result) {
        await _creatcombinationAynsc(ctx, element1, element2, "", result);
    }
    public static async Task _creatcombinationAynsc(CommandContext ctx, string element1, string element2, string element3, string result) 
    {
        if (ctx.Member.User_Id != 735182334984193) {
            return;
        }

        Combination combination = new(){
            Element1 = element1,
            Element2 = element2,
            Result = result,
            Time_Created = DateTime.UtcNow
        };

        if (element3 != "") {
            combination.Element3 = element3;
        }
        
        await dbctx.Combinations.AddAsync(combination);

        Element element = await dbctx.Elements.FirstOrDefaultAsync(x => x.Name == combination.Result);
        if (element == null) {
            Element _element = new() {
                Name = result,
                Found = 0,
                Finder_Id = ctx.Member.User_Id,
                Time_Created = DateTime.UtcNow
            };
            await dbctx.Elements.AddAsync(_element);
        }

        await dbctx.SaveChangesAsync();

        await ctx.ReplyAsync("Successfully created the combiation");
    }

    [Command("combination")]
    [Alias("c")]
    public static async Task combinationAsync(CommandContext ctx, string element1, string element2)
    {
        await _combinationAsync(ctx, element1, element2, "");
    }
    [Command("combination")]
    [Alias("c")]
    public static async Task combinationAsync(CommandContext ctx, string element1, string element2, string element3)
    {
        await _combinationAsync(ctx, element1, element2, element3);
    }

    public static async Task _combinationAsync(CommandContext ctx, string element1, string element2, string element3)
    {
        Combination combination = null;
        UserInvItem test = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.User_Id == ctx.Member.User_Id);
        if (test == null) {
            List<string> els = new() {"water", "air", "fire", "earth"};
            foreach(string el in els) {
                UserInvItem _item = new() {
                    User_Id = ctx.Member.User_Id,
                    Element = el,
                    TimeFound = DateTime.UtcNow
                };
                await dbctx.AddAsync(_item);
            }
            await dbctx.SaveChangesAsync();
        }
        element1 = element1.ToLower();
        element2 = element2.ToLower();
        if (element3 == "") {
            UserInvItem e1 = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.User_Id == ctx.Member.User_Id && x.Element == element1);
            UserInvItem e2 = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.User_Id == ctx.Member.User_Id && x.Element == element2);
            if (e1 == null) {
                ctx.ReplyAsync($"You have not found {element1} yet!");
                return;
            }
            if (e2 == null) {
                ctx.ReplyAsync($"You have not found {element2} yet!");
                return;
            }
            combination = await dbctx.Combinations.FirstOrDefaultAsync(x => x.Element1 == element1 && x.Element2 == element2 && x.Element3 == null);
            if (combination == null) {
                combination = await dbctx.Combinations.FirstOrDefaultAsync(x => x.Element1 == element2 && x.Element2 == element1 && x.Element3 == null);
            }
            //combination = await dbctx.Combinations.FirstOrDefaultAsync(x => (x.Element1 == element1 || x.Element1 == element2) && (x.Element2 == element1 || x.Element2 == element2));
        }
        else {
            element3 = element3.ToLower();
            UserInvItem e1 = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.User_Id == ctx.Member.User_Id && x.Element == element1);
            UserInvItem e2 = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.User_Id == ctx.Member.User_Id && x.Element == element2);
            UserInvItem e3 = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.User_Id == ctx.Member.User_Id && x.Element == element3);
            if (e1 == null) {
                ctx.ReplyAsync($"You have not found {element1} yet!");
                return;
            }
            if (e2 == null) {
                ctx.ReplyAsync($"You have not found {element2} yet!");
                return;
            }
            if (e3 == null) {
                ctx.ReplyAsync($"You have not found {element3} yet!");
                return;
            }
            combination = await dbctx.Combinations.FirstOrDefaultAsync(x => x.Element1 == element3 && x.Element2 == element2 && x.Element3 == element1);
            if (combination == null) {combination = await dbctx.Combinations.FirstOrDefaultAsync(x => x.Element1 == element3 && x.Element1 == element1 && x.Element3 == element2);}
            if (combination == null) {combination = await dbctx.Combinations.FirstOrDefaultAsync(x => x.Element1 == element1 && x.Element2 == element3 && x.Element3 == element2);}
            if (combination == null) {combination = await dbctx.Combinations.FirstOrDefaultAsync(x => x.Element1 == element2 && x.Element2 == element3 && x.Element3 == element1);}
            if (combination == null) {combination = await dbctx.Combinations.FirstOrDefaultAsync(x => x.Element1 == element1 && x.Element2 == element2 && x.Element3 == element3);}
            if (combination == null) {combination = await dbctx.Combinations.FirstOrDefaultAsync(x => x.Element1 == element2 && x.Element2 == element1 && x.Element3 == element3);}
        }

        if (combination == null) {
            ctx.ReplyAsync("Not a vaild combination! Suggest it by typing /suggest <result>");
            Combination _combination = new() {
                Element1 = element1,
                Element2 = element2,
                Time_Created = DateTime.UtcNow
            };
            if (element3 != "") {
                _combination.Element3 = element3;
            }
            FailedCombinations.AddOrUpdate(ctx.Member.User_Id, _combination, (key, oldValue) => _combination);
            return;
        }

        UserInvItem item = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.User_Id == ctx.Member.User_Id && x.Element == combination.Result);
    
        if (item != null) {
            ctx.ReplyAsync($"You found {combination.Result}, but you already found this element!");
            return;
        }
        else {
            UserInvItem _item = new() {
                User_Id = ctx.Member.User_Id,
                Element = combination.Result,
                TimeFound = DateTime.UtcNow
            };
            await dbctx.AddAsync(_item);

            Element element = await dbctx.Elements.FirstOrDefaultAsync(x => x.Name == combination.Result);
            element.Found += 1;

            // reward xp

            if (combination.Difficulty == 0) {
                List<string> baseelements = new() {"fire","earth","air","water"};
                Combination _a = null;
                while (_a == null || _a.Difficulty == 0) {
                    if (baseelements.Contains(combination.Element1)) {
                        _a = new() {
                            Difficulty = 1
                        };
                        break;
                    }
                    else {
                        _a = await dbctx.Combinations.FirstOrDefaultAsync(x => x.Result == combination.Element1);
                    }
                }
                Combination _b = null;
                while (_b == null || _b.Difficulty == 0) {
                    if (baseelements.Contains(combination.Element2)) {
                        _b = new() {
                            Difficulty = 1
                        };
                        break;
                    }
                    else {
                        _b = await dbctx.Combinations.FirstOrDefaultAsync(x => x.Result == combination.Element2);
                    }
                }
                Combination _c = null;
                if (element3 != "") {
                    while (_c == null || _c.Difficulty == 0) {
                        if (baseelements.Contains(combination.Element3)) {
                            _c = new() {
                                Difficulty = 1
                            };
                            break;
                        }
                        else {
                            _c = await dbctx.Combinations.FirstOrDefaultAsync(x => x.Result == combination.Element3);
                        }
                    }
                }

                if (element3 == "") {
                    if (_a.Difficulty > _b.Difficulty) {
                        combination.Difficulty = _a.Difficulty+1;
                    }
                    else {
                        combination.Difficulty = _b.Difficulty+1;
                    }
                }

                else {
                    if (_a.Difficulty > _b.Difficulty) {
                        if (_a.Difficulty > _c.Difficulty) {
                            combination.Difficulty = _a.Difficulty+1;
                        }
                        else {
                            combination.Difficulty = _c.Difficulty+1;
                        }
                    }
                    else {
                        if (_b.Difficulty > _c.Difficulty) {
                            combination.Difficulty = _b.Difficulty+1;
                        }
                        else {
                            combination.Difficulty = _c.Difficulty+1;
                        }
                    }
                }
            }

            DBUser user = await dbctx.Users.FirstOrDefaultAsync(x => x.Id == ctx.Member.Id);
            double amount = (double)(2+(combination.Difficulty/4));
            if (element3 != "") {
                amount *= 1.3;
            }
            user.Xp += amount;

            ctx.ReplyAsync($"You found {combination.Result}! You earn {Math.Round(amount,0)}xp!");

            await dbctx.DidTask(dbctx, DailyTaskType.Combined_Elements, ctx.Member.Id, ctx);

            await dbctx.SaveChangesAsync();
        }

    }

    [Command("inv")]
    [Alias("i")]
    public static async Task InvAsync(CommandContext ctx)
    {
        EmbedBuilder embed = new EmbedBuilder();
        EmbedPageBuilder page = new EmbedPageBuilder();
        
        List<UserInvItem> items = await dbctx.UserInvItems.Where(x => x.User_Id == ctx.Member.User_Id).ToListAsync();

        int i = 0;
        foreach(UserInvItem item in items) {
            i += 1;
            if (i <= 8) {
                page.AddText(name: null, item.Element, true);
            }
            else {
                page.AddText(null, "", false);
                page.AddText(name: null, item.Element, true);
                i = 0;
            }
        }

        embed.AddPage(page);
        await ctx.ReplyAsync(embed);
    }

    [Group("element")]
    public class RoleIncomeGroup : CommandModuleBase
    {
        [Command("count")]
        public static async Task ElementCountAsync(CommandContext ctx)
        {
            await ctx.ReplyAsync($"there are {await dbctx.Elements.CountAsync()} elements");
        }

        [Command("mycount")]
        public static async Task MyElementCountAsync(CommandContext ctx)
        {
            await ctx.ReplyAsync($"You have {await dbctx.UserInvItems.Where(x => x.User_Id == ctx.Member.User_Id).CountAsync()} elements");
        }

        [Command("mypercent")]
        public static async Task MyElementPercentAsync(CommandContext ctx)
        {
            int elements = await dbctx.Elements.CountAsync();
            int found = await dbctx.UserInvItems.Where(x => x.User_Id == ctx.Member.User_Id).CountAsync();
            double percent = (double)found/elements*100;
            await ctx.ReplyAsync($"You have found {Math.Round(percent, 2)}% of all elements");
        }
    }
}
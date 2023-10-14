using PopeAI.Database.Managers;
using Valour.Net.EmbedMenu;
using Valour.Api.Models.Messages.Embeds.Styles.Bootstrap;

namespace PopeAI.Commands.Elemental;

public class Node
{
    public string Name { get; set; }
    public List<Node> Children { get; set; }
    public Node() {
        Children = new();
    }

    public Node(string name) {
        Name = name;
        Children = new();
    }

    public Node Find(string name)
    {
        if (Name == name)
            return this;
        foreach(var child in Children) {
            var node = child.Find(name);
            if (node is not null)
                return node;
        }
        return null;
    }
}

public class Elemental : CommandModuleBase
{
    public static IdManager idManager = new();
    public static ConcurrentDictionary<long, Combination> FailedCombinations = new();
    static Random rnd = new();

    public static string PrintTree(Node tree, String indent, bool last)
    {
        string s = indent + "+- " + tree.Name + "\n";
        indent += last ? "   " : "|  ";

        for (int i = 0; i < tree.Children.Count; i++)
        {
            s += PrintTree(tree.Children[i], indent, i == tree.Children.Count - 1);
        }
        return s;
    }

    //[Command("tree")]
    public static async Task ViewTreeAsync(CommandContext ctx) 
    {
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
        var inv = await dbctx.UserInvItems.Where(x => x.UserId == ctx.Member.UserId).ToListAsync();
        var elements = await dbctx.Elements.Where(x => inv.Select(x => x.Element).Contains(x.Name)).ToListAsync();
        var combinations = await dbctx.Combinations.Where(x => inv.Select(x => x.Element).Contains(x.Result)).OrderBy(x => x.Difficulty).ToListAsync();
        
        Node root = new();

        root.Children = new() {
            new("water"),
            new("air"),
            new("fire"),
            new("earth"),
        };

        int lastdiff = 1;

        foreach(var combination in combinations)
        {
            var node = root.Find(combination.Result);
            if (combination.Difficulty > lastdiff) {

            }
        }

        ctx.ReplyAsync("");
    }

    [Command("suggest")]
    public static async Task SuggestAynsc(CommandContext ctx, string result) 
    {
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
        if (!FailedCombinations.ContainsKey(ctx.Member.UserId)) {
            ctx.ReplyAsync("You have not came across a new combination yet!");
            return;
        }

        Combination _Combination = FailedCombinations[ctx.Member.UserId];

        Suggestion suggestion = null;
        string element1 = _Combination.Element1;
        string element2 = _Combination.Element2;
        string element3 = _Combination.Element3;
        string text = "";

        if (_Combination.Element3 is null) {
            suggestion = await dbctx.Suggestions.FirstOrDefaultAsync(x => x.Element1 == element1 && x.Element2 == element2 && x.Element3 == null);
            if (suggestion == null) {
                suggestion = await dbctx.Suggestions.FirstOrDefaultAsync(x => x.Element1 == element2 && x.Element2 == element1 && x.Element3 == null);
            }
            text = $"{element1}, {element2}";
        }

        else {
            suggestion = await dbctx.Suggestions.FirstOrDefaultAsync(x => x.Element1 == element3 && x.Element2 == element2 && x.Element3 == element1);
            if (suggestion == null) {suggestion = await dbctx.Suggestions.FirstOrDefaultAsync(x => x.Element1 == element3 && x.Element1 == element1 && x.Element3 == element2);}
            if (suggestion == null) {suggestion = await dbctx.Suggestions.FirstOrDefaultAsync(x => x.Element1 == element1 && x.Element2 == element3 && x.Element3 == element2);}
            if (suggestion == null) {suggestion = await dbctx.Suggestions.FirstOrDefaultAsync(x => x.Element1 == element2 && x.Element2 == element3 && x.Element3 == element1);}
            if (suggestion == null) {suggestion = await dbctx.Suggestions.FirstOrDefaultAsync(x => x.Element1 == element1 && x.Element2 == element2 && x.Element3 == element3);}
            if (suggestion == null) {suggestion = await dbctx.Suggestions.FirstOrDefaultAsync(x => x.Element1 == element2 && x.Element2 == element1 && x.Element3 == element3);}
            text = $"{element1}, {element2}, {element3}";
        }

        if (suggestion is not null) {
            ctx.ReplyAsync($"There's already a recipe that uses the elements: {text}! Use /vote to decide if that recipe should be added!");
        }

        FailedCombinations.Remove(ctx.Member.UserId, out _);

        suggestion = new() {
            Id = idManager.Generate(),
            Element1 = _Combination.Element1,
            Element2 = _Combination.Element2,
            Result = result,
            UserId = ctx.Member.UserId
        };

        if (_Combination.Element3 != null) {
            suggestion.Element3 = _Combination.Element3;
        }

        await dbctx.AddAsync(suggestion);
        await dbctx.SaveChangesAsync();

        ctx.ReplyAsync("Successfully added the suggestion!");
    }

    [Command("vote")]
    public static async Task LoadVoteScreenAsync(CommandContext ctx)
    {
		EmbedBuilder embed = new EmbedBuilder().AddPage("Elemental Recipe Voting").AddRow().AddButton("Click to Load the Voting Screen").OnClick(SendVoteScreenAsync);
		await ctx.ReplyAsync(embed);
	}

    [EmbedMenuFunc]
    public static async ValueTask SendVoteScreenAsync(InteractionContext ctx)
    {
        var embed = await _VoteAynsc(ctx.Member);
        ctx.UpdateEmbedForUser(embed);
    }

    public static async Task<EmbedBuilder> _VoteAynsc(PlanetMember member) 
    {
        var embed = new EmbedBuilder().AddPage("Suggestion Voting").AddRow().AddText("By the way, you can change your vote.");

        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();

        List<Suggestion> suggestions = await dbctx.Suggestions.OrderBy(x => x.TimeSuggested).Take(20).ToListAsync();
        var ids = suggestions.Select(x => x.Id).ToList();
        //if (suggestions.Count == 0) {
        //    return null;
        //}
        
        int i = 0;
        var votes = await dbctx.SuggestionVotes.Where(x => ids.Contains(x.SuggestionId) && x.UserId == member.UserId).ToListAsync();
        foreach(Suggestion suggestion in suggestions) {
            var vote = votes.FirstOrDefault(x => x.SuggestionId == suggestion.Id);

			string text = "";
            if (suggestion.Element3 == null) {
                text = $"{suggestion.Element1} + {suggestion.Element2} = {suggestion.Result} ({suggestion.Ayes}-{suggestion.Nays})";
            }
            else {
                text = $"{suggestion.Element1} + {suggestion.Element2} + {suggestion.Element3} = {suggestion.Result} ({suggestion.Ayes}-{suggestion.Nays})";
            }
            //page.AddText(null, "", false);
            embed.AddRow();
            embed.AddText(text:text);

            embed.AddRow();
            if (vote is null || vote.VotedFor)
            {
                embed.AddButton("Yes")
                    .SetId($"YesVoteFromSuggestion:{suggestion.Id}")
                    .OnClickSendInteractionEvent($"YesVoteFromSuggestion:{suggestion.Id}")
                    .WithStyles(new BackgroundColor(new Color("007F0E")));
            }
            else if (!vote.VotedFor)
				embed.AddButton("Yes").SetId($"YesVoteFromSuggestion:{suggestion.Id}").OnClickSendInteractionEvent($"YesVoteFromSuggestion:{suggestion.Id}");

			if (vote is null || !vote.VotedFor)
                embed.AddButton("No").SetId($"NoVoteFromSuggestion:{suggestion.Id}").OnClickSendInteractionEvent($"NoVoteFromSuggestion:{suggestion.Id}").WithStyles(new BackgroundColor(new Color("7F0000")));
            
            else
				embed.AddButton("No").SetId($"NoVoteFromSuggestion:{suggestion.Id}").OnClickSendInteractionEvent($"NoVoteFromSuggestion:{suggestion.Id}");
            i += 1;
            if (i >= 5) {
                break;
            }
        }
        //embed.CurrentPage.Title = "Vote";
        return embed;
    }

    [Interaction(EmbedIteractionEventType.ItemClicked)]
    public static async Task InteractionAynsc(InteractionContext ctx) 
    {
        PlanetMember member = await PlanetMember.FindAsync(ctx.Event.Author_MemberId, ctx.Event.PlanetId);

        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();

        string elementname = ctx.Event.ElementId;
        if (elementname.Contains("VoteFromSuggestion")) {
            long suggestid = long.Parse(elementname.Split("Suggestion:")[1]);
            Suggestion suggestion = await dbctx.Suggestions.FirstOrDefaultAsync(x => x.Id == suggestid);
            member = await PlanetMember.FindAsync(ctx.Event.MemberId, ctx.Event.PlanetId);
            var suggestionVote = await dbctx.SuggestionVotes.FirstOrDefaultAsync(x => x.SuggestionId == suggestion.Id && x.UserId == member.UserId);

            if (suggestionVote is not null)
            {
                if (suggestionVote.VotedFor)
                    suggestion.Ayes -= 1;
                else
                    suggestion.Nays -= 1;
			}

            if (elementname.Contains("YesVoteFromSuggestion"))
            {
                suggestion.Ayes += 1;
            }
            if (elementname.Contains("NoVoteFromSuggestion"))
            {
                suggestion.Nays += 1;
            }

            if (suggestionVote is null)
            {
                suggestionVote = new()
                {
                    Id = idManager.Generate(),
                    UserId = ctx.Member.UserId,
                    SuggestionId = suggestid,
                    VotedFor = elementname.Contains("YesVoteFromSuggestion")
                };
                await dbctx.AddAsync(suggestionVote);
            }
            else
            {
                suggestionVote.VotedFor = elementname.Contains("YesVoteFromSuggestion");
			}

            int votes = suggestion.Ayes+suggestion.Nays;

            // min of 3 votes
            if (votes >= 3) {
                bool approved = false;
                var total = suggestion.Nays + suggestion.Ayes;
                var ratio = 0.00;
                if (total > 0)
                    ratio = (double)suggestion.Ayes / (double)total;

                if (suggestion.Nays != 0) {
                    // need 2/3 vote to approve
                    if (ratio > 0.66) {
                        approved = true;
                    }
                }
                else {
                    approved = true;
                }

                if (approved) {
                    Combination combination = new(){
                        Id = idManager.Generate(),
                        Element1 = suggestion.Element1,
                        Element2 = suggestion.Element2,
                        Result = suggestion.Result,
                        TimeCreated = DateTime.UtcNow
                    };
                    combination.Difficulty = await combination.CalcDifficulty();

                    if (suggestion.Element3 != "") {
                        combination.Element3 = suggestion.Element3;
                    }
                    
                    dbctx.Combinations.Add(combination);

                    Element element = await dbctx.Elements.FirstOrDefaultAsync(x => x.Name == combination.Result);
                    if (element == null) {
                        Element _element = new() {
                            Id = idManager.Generate(),
                            Name = suggestion.Result,
                            Found = 0,
                            Finder_Id = suggestion.UserId,
                            Time_Created = DateTime.UtcNow
                        };
                        dbctx.Elements.Add(_element);
                        element = _element;
                    }

                    List<SuggestionVote> _votes = await dbctx.SuggestionVotes.Where(x => x.SuggestionId == suggestion.Id).ToListAsync();

                    dbctx.SuggestionVotes.RemoveRange(_votes);

                    dbctx.Suggestions.Remove(suggestion);

                    await dbctx.SaveChangesAsync();

                    var invitem_ = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.UserId == ctx.Member.UserId && x.Element == combination.Result);
                    if (invitem_ is null)
                    {
                        UserInvItem _item = new(idManager.Generate(), suggestion.UserId, combination.Result);
                        dbctx.Add(_item);
                    }

                    element.Found += 1;

                    string text = "";
                    if (suggestion.Element3 == null) {
                        text = $"{suggestion.Element1} + {suggestion.Element2} = {suggestion.Result}";
                    }
                    else {
                        text = $"{suggestion.Element1} + {suggestion.Element2} + {suggestion.Element3} = {suggestion.Result}";
                    }

                    ctx.ReplyAsync($"Enought votes were reached ({suggestion.Ayes}-{suggestion.Nays}) for [{text}] to be accepted!");
                }
                else if (total >= 5 && ratio <= 0.33)
                {
                    List<SuggestionVote> _votes = await dbctx.SuggestionVotes.Where(x => x.SuggestionId == suggestion.Id).ToListAsync();

                    dbctx.SuggestionVotes.RemoveRange(_votes);

                    dbctx.Suggestions.Remove(suggestion);

                    await dbctx.SaveChangesAsync();

                    string text = "";
                    if (suggestion.Element3 == null)
                    {
                        text = $"{suggestion.Element1} + {suggestion.Element2} = {suggestion.Result}";
                    }
                    else
                    {
                        text = $"{suggestion.Element1} + {suggestion.Element2} + {suggestion.Element3} = {suggestion.Result}";
                    }

                    ctx.ReplyAsync($"Due to receiving {total} votes and 2/3 (or greater) having voted no ({suggestion.Ayes}-{suggestion.Nays}), [{text}] has been rejected!");
                }
            }

            await dbctx.SaveChangesAsync();
            
            //PlanetMessage message = await PlanetMessage.FindAsync(ctx.Event.MessageId, ctx.Event.ChannelId, ctx.Event.PlanetId);
            //await message.DeleteAsync();

            ctx.UpdateEmbedForUser(await _VoteAynsc(ctx.Member));
        }
    }

    //[Command("test")]
    //[Alias("tes")]
    public async Task TestAynsc(CommandContext ctx) 
    {
        if (ctx.Member.UserId != 735182334984193) {
            return;
        }
        var embed = new EmbedBuilder().AddPage().AddRow();
        //embed.AddText("Currently Combining", "Water");
        //embed.AddInputBox("", "Element To Combine");
        //embed.AddButton("Submit", "Combine");
        //embed.AddPage(page);
        ctx.ReplyAsync(embed);
    }

    [Command("createlement")]
    [Alias("ce")]
    public static async Task createlementAynsc(CommandContext ctx, string name) 
    {
        if (ctx.Member.UserId != 735182334984193) {
            return;
        }

        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();

        Element element = new() {
            Id = idManager.Generate(),
            Name = name,
            Found = 0,
            Finder_Id = ctx.Member.UserId,
            Time_Created = DateTime.UtcNow,
        };
        
        await dbctx.Elements.AddAsync(element);
        await dbctx.SaveChangesAsync();

        ctx.ReplyAsync("Successfully created the element");
    }

    [Command("creatcombination")]
    [Alias("cc")]
    public static async Task CreatCombinationAynsc3Elements(CommandContext ctx, string element1, string element2, string element3, string result)
    {
        await CreateCombinationAsync(ctx, element1, element2, element3, result);
    }

    [Command("creatcombination")]
    [Alias("cc")]
    public static async Task CreatCombinationAsync2Elements(CommandContext ctx, string element1, string element2, string result)
    {
        await CreateCombinationAsync(ctx, element1, element2, "", result);
    }

    public static async Task CreateCombinationAsync(CommandContext ctx, string element1, string element2, string element3, string result) 
    {
        if (ctx.Member.UserId != 735182334984193) {
            return;
        }

        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();

        Combination combination = new(){
            Element1 = element1,
            Element2 = element2,
            Result = result,
            TimeCreated = DateTime.UtcNow
        };

        if (element3 != "") {
            combination.Element3 = element3;
        }
        
        dbctx.Combinations.Add(combination);

        Element element = await dbctx.Elements.FirstOrDefaultAsync(x => x.Name == combination.Result);
        if (element == null) {
            Element _element = new() {
                Id = idManager.Generate(),
                Name = result,
                Found = 0,
                Finder_Id = ctx.Member.UserId,
                Time_Created = DateTime.UtcNow
            };
            await dbctx.Elements.AddAsync(_element);
        }

        await dbctx.SaveChangesAsync();

        ctx.ReplyAsync("Successfully created the combiation");
    }

    [Command("combination")]
    [Alias("c", "combine")]
    public static async Task CombinationAsync2Elements(CommandContext ctx, string element1, string element2)
    {
        await CombinationAsync(ctx, element1, element2);
    }

    [Command("combination")]
    [Alias("c", "combine")]
    public static async Task CombinationAsync3Elements(CommandContext ctx, string element1, string element2, string element3)
    {
        await CombinationAsync(ctx, element1, element2, element3);
    }

    public static async Task CombinationAsync(CommandContext ctx, string element1, string element2, string element3 = "")
    {
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();

        UserInvItem test = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.UserId == ctx.Member.UserId);
        if (test == null) {
            List<string> els = new() {"water", "air", "fire", "earth"};
            foreach(string el in els) {
                await dbctx.UserInvItems.AddAsync(new(idManager.Generate(), ctx.Member.UserId, el));
            }
            await dbctx.SaveChangesAsync();
        }
        Combination combination = null;
        element1 = element1.ToLower();
        element2 = element2.ToLower();
        if (element3 == "") {
            UserInvItem e1 = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.UserId == ctx.Member.UserId && x.Element == element1);
            UserInvItem e2 = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.UserId == ctx.Member.UserId && x.Element == element2);
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
            UserInvItem e1 = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.UserId == ctx.Member.UserId && x.Element == element1);
            UserInvItem e2 = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.UserId == ctx.Member.UserId && x.Element == element2);
            UserInvItem e3 = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.UserId == ctx.Member.UserId && x.Element == element3);
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
                Id = idManager.Generate(),
                Element1 = element1,
                Element2 = element2,
                TimeCreated = DateTime.UtcNow
            };
            if (element3 != "") {
                _combination.Element3 = element3;
            }
            FailedCombinations.AddOrUpdate(ctx.Member.UserId, _combination, (key, oldValue) => _combination);
            return;
        }

        UserInvItem item = await dbctx.UserInvItems.FirstOrDefaultAsync(x => x.UserId == ctx.Member.UserId && x.Element == combination.Result);
    
        if (item != null) {
            ctx.ReplyAsync($"You found {combination.Result}, but you already found this element!");
            return;
        }
        else {
            UserInvItem _item = new(idManager.Generate(), ctx.Member.UserId, combination.Result);
            await dbctx.AddAsync(_item);

            Element element = await dbctx.Elements.FirstOrDefaultAsync(x => x.Name == combination.Result);
            element.Found += 1;

            // reward xp

            if (combination.Difficulty == 0) {
                combination.Difficulty = await combination.CalcDifficulty();
            }

            await using var user = await DBUser.GetAsync(ctx.Member.Id);
            decimal amount = 1+(((decimal)combination.Difficulty)/6.0m);
            if (element3 != null) {
                amount *= 1.5m;
            }
            user.ElementalXp += amount;

            ctx.ReplyAsync($"You found {combination.Result}! You earn {Math.Round(amount,1)}xp!");

            await DailyTaskManager.DidTask(DailyTaskType.Combined_Elements, ctx.Member.Id, ctx);

            await dbctx.SaveChangesAsync();
        }
    }

    [Command("inv")]
    [Alias("i")]
    public static async Task InvAsync(CommandContext ctx)
    {
        var embed = new EmbedBuilder().AddPage($"Inventory of {ctx.Member.Nickname}").AddRow();
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();

        List<UserInvItem> items = await dbctx.UserInvItems.Where(x => x.UserId == ctx.Member.UserId).ToListAsync();

        int i = 0;
        foreach(UserInvItem item in items) {
            i += 1;
            if (i <= 8) {
                embed.AddText(item.Element);
            }
            else {
                embed.AddText(item.Element);
                embed.AddRow();
                i = 0;
            }
        }
        ctx.ReplyAsync(embed);
    }

    [Group("element")]
    public class RoleIncomeGroup : CommandModuleBase
    {
        [Command("count")]
        public static async Task ElementCountAsync(CommandContext ctx)
        {
            using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
            ctx.ReplyAsync($"there are {(await dbctx.Elements.CountAsync())+4} elements");
        }

        [Command("mycount")]
        public static async Task MyElementCountAsync(CommandContext ctx)
        {
            using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
            int elements = (await dbctx.Elements.CountAsync())+4;
            int found = await dbctx.UserInvItems.Where(x => x.UserId == ctx.Member.UserId).CountAsync();
            double percent = (double)found/elements*100;
            ctx.ReplyAsync($"You have found {found} ({Math.Round(percent, 2)}%) of all elements");
        }
    }
}
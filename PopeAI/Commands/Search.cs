using PopeAI.Database.Models.Messaging;

/*
search
view
*/

namespace PopeAI.Commands.Search;

public class Search : CommandModuleBase
{
    Random rnd = new Random();

    public string Truncate(string value, int maxChars)
    {
        value = value.Replace("\n"," ");
        return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
    }

    public async Task OutputToListOld(List<Message> msgs, CommandContext ctx, int planetIndex = 0) {
        string content = "";
        foreach(Message msg in msgs) {
            PlanetMember member = await PlanetMember.FindAsync(msg.MemberId);
            if (msg.PlanetIndex == planetIndex) {
                content += $"=>({msg.PlanetIndex}) {member.Nickname}: {Truncate(msg.Content,60)}\n";
            }
            else {
                content += $"({msg.PlanetIndex}) {member.Nickname}: {Truncate(msg.Content,60)}\n";
            }
        }
        await ctx.ReplyAsync(content);
    }

    public async Task OutputToList(List<Message> msgs, CommandContext ctx, int planetIndex = 0) {
        EmbedBuilder embed = new();
        EmbedPageBuilder page = new();
        foreach(Message msg in msgs) {
            PlanetMember member = await PlanetMember.FindAsync(msg.MemberId);
            if (msg.PlanetIndex == planetIndex) {
                page.AddText(text:$"=>({msg.PlanetIndex}) {member.Nickname}: {Truncate(msg.Content,60)}\n");
            }
            else {
                page.AddText(text: $"({msg.PlanetIndex}) {member.Nickname}: {Truncate(msg.Content,60)}\n");
            }
            if (page.Items.Count() > 13) {
                embed.AddPage(page);
                page = new EmbedPageBuilder();
            }
        }
        if (page.Items.Count() != 0) {
            embed.AddPage(page);
        }
        await ctx.ReplyAsync(embed:embed);
    }

    [Command("view")]
    public async Task ViewAsync(CommandContext ctx, int planetIndex)
    {
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
        List<Message> msgs = await dbctx.Messages
            .Where(x => x.PlanetId == ctx.Planet.Id && x.PlanetIndex > planetIndex - 6 && x.PlanetIndex < planetIndex + 6)
            .Take(20)
            .ToListAsync();  
        await OutputToList(msgs, ctx, planetIndex);
    }

    [Command("search")]
    [Alias("find")]
    public async Task SearchAsync(CommandContext ctx, [Remainder] string content)
    {
        List<Message> messages = await SearchFuncAsync(ctx, content);
        await OutputToList(messages, ctx);
    }
    public static async Task<List<Message>> SearchFuncAsync(CommandContext ctx, string content)
    {
        
        // do the tags

        content = content.Replace(": ", ":");
        // in case
        content = content.Replace(":  ", ":");

        string[] splits = new string[] {" ", "&nbsp;", " "};
        string[] words = content.Split(splits, StringSplitOptions.None);

        List<string> remove = new List<string>();

        foreach (string t in words) {
            string[] word = t.Split(":");
            if (word.Count() != 2) {
                continue;
            }
            remove.Add($"{word[0]}:{word[1]}");
        }
        string need = content;
        foreach (string replace in remove) {
            need = need.Replace(replace, "");
        }

        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();

        IQueryable<Message> search = dbctx.Messages.AsQueryable();

        foreach (string t in words) {
            string[] word = t.Split(":");
            if (word.Count() != 2) {
                continue;
            }
            switch (word[0].ToLower())
            {
                case "from": 
                    // check if ping
                    string value = word[1];
                    value = value.Replace(" ", "");

                    if (value.Substring(0,4) == "«@m-") {
                        string v = value.Replace("«@m-", "");
                        v = v.Replace("»","");
                        if (ulong.TryParse(v, out ulong MemberID)) {
                            search = search.Where(x => x.MemberId == MemberID);
                        }
                    }
                    break;
            }
        }

        if (need != "")
        {
            search = search.Where(x => x.SearchVector.Matches(need));
        }

        List<Message> messages = await search.OrderByDescending(x => x.PlanetId).Take(100).ToListAsync();
        
        return messages;
    }

}
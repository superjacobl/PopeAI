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

    public async Task OutputToListOld(List<PopeAI.Database.Models.Messaging.Message> msgs, CommandContext ctx, int planetIndex = 0) {
        string content = "";
        foreach(var msg in msgs) {
            PlanetMember member = await PlanetMember.FindAsync(msg.MemberId, msg.PlanetId);
            if (msg.PlanetIndex == planetIndex) {
                content += $"=>({msg.PlanetIndex}) {member.Nickname}: {Truncate(msg.Content,60)}\n";
            }
            else {
                content += $"({msg.PlanetIndex}) {member.Nickname}: {Truncate(msg.Content,60)}\n";
            }
        }
        ctx.ReplyAsync(content);
    }

    public async Task OutputToList(List<PopeAI.Database.Models.Messaging.Message> msgs, CommandContext ctx, int planetIndex = 0) {
        var embed = new EmbedBuilder().AddPage().AddRow();
        if (msgs.Count == 0) {
            ctx.ReplyAsync("No messages were found.");
            return;
        }
        foreach(var msg in msgs) {
            PlanetMember member = await PlanetMember.FindAsync(msg.MemberId, msg.PlanetId);
            string content = "";
            if (msg.EmbedData is not null) {
                content = "Embed";
            }
            else {
                content = msg.Content;
            }
            if (msg.PlanetIndex == planetIndex) {
                embed.AddText(text:$"=>({msg.PlanetIndex}) {member.Nickname}: {Truncate(content,60)}\n");
            }
            else {
                embed.AddText(text: $"({msg.PlanetIndex}) {member.Nickname}: {Truncate(content,60)}\n");
            }
            embed.AddRow();
            if (embed.CurrentPage.Children.Count > 13) {
                embed.AddPage().AddRow();
            }
        }
        ctx.ReplyAsync(embed);
    }

    [Command("view")]
    public async Task ViewAsync(CommandContext ctx, int planetIndex)
    {
        using var dbctx = PopeAIDB.DbFactory.CreateDbContext();
        var msgs = await dbctx.Messages
            .Where(x => x.PlanetId == ctx.Planet.Id && x.PlanetIndex > planetIndex - 6 && x.PlanetIndex < planetIndex + 6)
            .Take(20)
            .ToListAsync();  
        await OutputToList(msgs, ctx, planetIndex);
    }

    [Command("search")]
    [Alias("find")]
    public async Task SearchAsync(CommandContext ctx, [Remainder] string content)
    {
        var messages = await SearchFuncAsync(ctx, content);
        await OutputToList(messages, ctx);
    }
    public static async Task<List<PopeAI.Database.Models.Messaging.Message>> SearchFuncAsync(CommandContext ctx, string content)
    {
        
        // do the tags

        content = content.Replace(": ", ":");
        // in case
        content = content.Replace(":  ", ":");

        string[] splits = new string[] {" ", "&nbsp;", " "};
        string[] words = content.Split(splits, StringSplitOptions.None);

        List<string> remove = new List<string>();

        bool HasReply = false;

        foreach (string t in words) {
            if (t.Contains("--reply")) {
                HasReply = true;
                continue;
            }
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

        var search = dbctx.Messages.AsQueryable();

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
                        if (long.TryParse(v, out long MemberID)) {
                            search = search.Where(x => x.MemberId == MemberID);
                        }
                    }
                    break;

                case "in": 
                    value = word[1];
                    value = value.Replace(" ", "");

                    if (value.Substring(0,4) == "«#c-") {
                        string v = value.Replace("«#c-", "");
                        v = v.Replace("»","");
                        if (long.TryParse(v, out long ChannelId)) {
                            search = search.Where(x => x.ChannelId == ChannelId);
                        }
                    }
                    break;
            }
        }

        if (HasReply) {
            search = search.Where(x => x.ReplyToId != null);
        }

        if (need != "")
        {
            search = search.Where(x => x.SearchVector.Matches(need));
        }

        var messages = await search.Where(x => x.PlanetId == ctx.Planet.Id).OrderByDescending(x => x.PlanetIndex).Take(100).ToListAsync();
        
        return messages;
    }

}
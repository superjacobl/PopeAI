using System.Text.Json.Serialization;
using System.Text.Json;

namespace PopeAI.Commands.MyValourStats
{
    public class MyValourStats : CommandModuleBase
    {
        [Interaction("GetValourData")]
        public async Task FuncMyValourStats(InteractionContext ctx)
        {
            if (ctx.Event.MemberId != 2551219538886656 || ctx.Event.Event != "GetValourData") {
                return;
            }

            using var dbctx = PopeAIDB.DbFactory.CreateDbContext();

            EmbedInteractionEvent EventReturn = new EmbedInteractionEvent()
            {
                Event = "ReturnValourData",
                Element_Id = $"{ctx.Event.Element_Id},{ctx.Event.Author_MemberId}",
                PlanetId = 2548110249689088,
                Message_Id = 0,
                Author_MemberId = ctx.Event.MemberId,
                MemberId = 2551219538886656,
                ChannelId = 2548110249689090,
                Time_Interacted = DateTime.UtcNow
            };

            switch (ctx.Event.Element_Id)
            {
                case "MessagesAroundId":
                    ulong Id = ulong.Parse(ctx.Event.Form_Data.First().Value);
                    Message m = await dbctx.Messages.FindAsync(Id);
                    Id = m.PlanetIndex;
                    List<Message> msgs = await Task.Run(() = dbctx.Messages.Where(x => x.PlanetId == ctx.Planet.Id && x.PlanetIndex > Id-15 && x.PlanetIndex < Id+15).Take(32).ToList());  
                    EventReturn.Form_Data = new();
                    foreach(Message msg in msgs) {
                        EmbedFormData Item = new()
                        {
                            Element_Id = " ",
                            Type = EmbedItemType.Text,
                            Value = JsonSerializer.Serialize(msg, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })
                        };
                        EventReturn.Form_Data.Add(Item);
                    }
                    break;

                case "Search Messages":
                    string query = ctx.Event.Form_Data.First().Value;
                    CommandContext _ctx = new();
                    _ctx.Planet = new();
                    _ctx.Planet.Id = ulong.Parse(ctx.Event.Form_Data[1].Value);
                    msgs = await Search.Search.SearchFuncAsync(_ctx, query);
                    EventReturn.Form_Data = new();
                    foreach(Message msg in msgs.Take(25)) {
                        EmbedFormData Item = new()
                        {
                            Element_Id = " ",
                            Type = EmbedItemType.Text,
                            Value = JsonSerializer.Serialize(msg, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })
                        };
                        EventReturn.Form_Data.Add(Item);
                    }
                    break;
            }

            var response = await ValourClient.Http.PostAsJsonAsync($"api/embed/interact", EventReturn);

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
    }
}
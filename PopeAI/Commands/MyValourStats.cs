using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Text.Json;
using PopeAI.Database;
using Microsoft.EntityFrameworkCore;
using PopeAI.Models;
using Valour.Api.Client;
using Valour.Net.ModuleHandling;
using Valour.Net.CommandHandling;
using Valour.Net.CommandHandling.Attributes;
using PopeAI;
/*

 */

namespace PopeAI.Commands.MyValourStats
{
    public class MyValourStats : CommandModuleBase
    {
        [Interaction("GetValourData")]
        public async Task FuncMyValourStats(InteractionContext ctx)
        {
            // check if came from self

            if (ctx.Event.Member_Id != 2551219538886656 || ctx.Event.Event != "GetValourData") {
                return;
            }

            EmbedInteractionEvent EventReturn = new EmbedInteractionEvent()
            {
                Event = "ReturnValourData",
                Element_Id = $"{ctx.Event.Element_Id},{ctx.Event.Author_Member_Id}",
                Planet_Id = 2548110249689088,
                Message_Id = 0,
                Author_Member_Id = ctx.Event.Member_Id,
                Member_Id = 2551219538886656,
                Channel_Id = 2548110249689090,
                Time_Interacted = DateTime.UtcNow
            };

            switch (ctx.Event.Element_Id)
            {
                case "MessagesAroundId":
                    ulong Id = ulong.Parse(ctx.Event.Form_Data.First().Value);
                    Message m = await Client.DBContext.Messages.FindAsync(Id);
                    Id = (ulong)m.Planet_Index;
                    List<Message> msgs = await Task.Run(() => Client.DBContext.Messages.Where(x => x.Planet_Id == ctx.Planet.Id && x.Planet_Index > Id-15 && x.Planet_Index < Id+15).Take(32).ToList());  
                    EventReturn.Form_Data = new();
                    foreach(Message msg in msgs) {
                        EmbedFormData Item = new();
                        Item.Element_Id = " ";
                        Item.Type = EmbedItemType.Text;
                        Item.Value = JsonSerializer.Serialize<Message>(msg, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull});
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
                        EmbedFormData Item = new();
                        Item.Element_Id = " ";
                        Item.Type = EmbedItemType.Text;
                        Item.Value = JsonSerializer.Serialize<Message>(msg, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull});
                        EventReturn.Form_Data.Add(Item);
                    }
                    break;
            }

            var response = await ValourClient.Http.PostAsJsonAsync($"api/embed/interact", EventReturn);

            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
    }
}
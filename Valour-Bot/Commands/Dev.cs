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

namespace PopeAI.Commands.Dev
{
    public class Dev : CommandModuleBase
    {
        [Command("userid")]
        [Summary("The User Id of the user who entered the command.")]
        public async Task UserId(CommandContext ctx)
        {
            await ctx.ReplyAsync($"Your UserId is {ctx.Member.User_Id}");
        }

        [Command("memberid")]
        [Summary("The Member Id of the user who entered the command.")]
        public async Task MemberId(CommandContext ctx)
        {
            await ctx.ReplyAsync($"Your MemberId is {ctx.Member.Id}");
        }

        [Command("planetid")]
        [Summary("The Planet Id of the channel where the command was entered.")]
        public async Task PlanetId(CommandContext ctx)
        {
            await ctx.ReplyAsync($"This planet's id is {ctx.Planet.Id}");
        }

        [Command("channelid")]
        [Summary("The Channel Id of the channel where the command was entered.")]
        public async Task ChannelId(CommandContext ctx)
        {
            await ctx.ReplyAsync($"This channel's id is {ctx.Channel.Id}");
        }
    }
}
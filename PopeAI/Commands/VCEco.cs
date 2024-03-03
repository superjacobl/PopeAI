using Valour.Sdk.Models.Economy;
using Valour.Sdk.Models;
using Valour.Shared.Models;

namespace PopeAI.Commands;
public class VCEco : CommandModuleBase
{
	[Command("vcbal")]
	public async Task VCBalAsync(CommandContext ctx)
	{
		var result = await EcoAccount.FindGlobalIdByNameAsync((await ctx.Member.GetUserAsync()).NameAndTag);
		var ecoaccount = await EcoAccount.FindAsync(result.AccountId, ISharedPlanet.ValourCentralId);
		await ctx.ReplyAsync($"{ctx.Member.Nickname}'s Valour Credits: ¢{ecoaccount.BalanceValue}");
	}

	[Command("vcbal")]
	public async Task VCBalAsync(CommandContext ctx, PlanetMember member)
	{
		var result = await EcoAccount.FindGlobalIdByNameAsync((await member.GetUserAsync()).NameAndTag);
		var ecoaccount = await EcoAccount.FindAsync(result.AccountId, ISharedPlanet.ValourCentralId);
		await ctx.ReplyAsync($"{member.Nickname}'s Valour Credits: ¢{ecoaccount.BalanceValue}");
	}
}

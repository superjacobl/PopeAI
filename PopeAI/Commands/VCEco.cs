using Valour.Api.Models.Economy;
using Valour.Api.Models;
using Valour.Shared.Models;

namespace PopeAI.Commands;
public class VCEco : CommandModuleBase
{
	[Command("vcbal")]
	public async Task VCBalAsync(CommandContext ctx)
	{
		var result = await EcoAccount.FindGlobalIdByNameAsync((await ctx.Member.GetUserAsync()).NameAndTag);
		var ecoaccount = await EcoAccount.FindAsync(result.AccountId, ISharedPlanet.ValourCentralId);
		ctx.ReplyAsync($"{ctx.Member.Nickname}'s Valour Credits: ¢{ecoaccount.BalanceValue}");
	}
}

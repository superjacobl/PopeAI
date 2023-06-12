using PopeAI.Commands.Banking;
using Valour.Net.EmbedMenu;

namespace PopeAI.Commands.Dev;

public class Settings : CommandModuleBase
{
	[Command("settings")]
	public async Task GetSettingsAsync(CommandContext ctx)
	{
		var info = await PlanetInfo.GetAsync(ctx.Planet.Id, _readonly: true);
		if (info is null)
			return;

		if (!(await ctx.Member.HasPermissionAsync(PlanetPermissions.Manage)))
		{
			ctx.ReplyAsync("You can only modify PopeAI planet settings if you have the PlanetPermissions.Manage permission!");
			return;
		}

		EmbedBuilder embed = new EmbedBuilder().AddPage("Settings")
			.AddRow()
				.AddButton("Click to load Planet Settings")
					.OnClickSendInteractionEvent("Settings-Load");
		ctx.ReplyAsync(embed);
	}

	[Interaction(EmbedIteractionEventType.ItemClicked, interactionElementId: "Settings-Load")]
	public async Task OnSettingLoad(InteractionContext ctx)
	{
		if (!(await ctx.Member.HasPermissionAsync(PlanetPermissions.Manage)))
		{
			ctx.ReplyAsync("You can only modify PopeAI planet settings if you have the PlanetPermissions.Manage permission!");
			return;
		}

		ctx.UpdateEmbedForUser(await GetSettingsEmbedAsync(ctx), ctx.Member.UserId);
	}

	public async Task<EmbedBuilder> GetSettingsEmbedAsync(IContext ctx)
	{
		var info = await PlanetInfo.GetAsync(ctx.Planet.Id, _readonly: true);
		var embed = new EmbedBuilder().AddPage($"{ctx.Planet.Name}'s Settings")
			.AddRow()
				.AddText("Click on a button to toggle whether that bot feature is enabled or disabled for your planet.")
					.WithStyles(new Width(new Size(Unit.Pixels, 225)));

		foreach (var moduletype in Enum.GetValues<ModuleType>())
		{
			if (info.HasEnabled(moduletype))
			{
				embed.AddRow()
					.AddButton(moduletype.ToString())
						.WithStyles(new BackgroundColor("007F0E"))
							.OnClick(ClickOnSetting, moduletype.ToString());
			}
			else
			{
				embed.AddRow()
					.AddButton(moduletype.ToString())
						.WithStyles(new BackgroundColor("808080"))
							.OnClick(ClickOnSetting, moduletype.ToString());
			}
		}
		return embed;
	}

    [EmbedMenuFunc]
    public async ValueTask ClickOnSetting(InteractionContext ctx)
    {
		var info = await PlanetInfo.GetAsync(ctx.Planet.Id);
		var settingid = ctx.Event.ElementId.Split("::").First();
        var module = Enum.Parse<ModuleType>(settingid);
        if (info.HasEnabled(module))
            info.Modules.Remove(module);
        else
            info.Modules.Add(module);

        await info.UpdateDB();
		await ctx.UpdateEmbedForUser(await GetSettingsEmbedAsync(ctx), ctx.Member.UserId);
	}
}
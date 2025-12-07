using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Misc;

namespace CS2_CommandsBlocker_SwiftlyS2;

[PluginMetadata(Id = "CS2_CommandsBlocker_SwiftlyS2", Version = "1.0.0", Name = "CS2-CommandsBlocker-SwiftlyS2",
    Author = "daffyy", Description = "Block specific console commands from player usage")]
public partial class CS2_CommandsBlocker_SwiftlyS2 : BasePlugin
{
    private readonly PluginConfig _config;
    private Guid _commandGuid;
    
    public CS2_CommandsBlocker_SwiftlyS2(ISwiftlyCore core) : base(core)
    {
        const string configFileName = "config.jsonc";
        const string configSection = "CS2-CommandsBlocker";

        Core.Configuration
            .InitializeJsonWithModel<PluginConfig>(configFileName, configSection)
            .Configure(builder => {
                builder.AddJsonFile(Core.Configuration.GetConfigPath(configFileName), optional: false, reloadOnChange: true);
            });

        ServiceCollection services = new();
        services.AddSwiftly(Core).AddOptionsWithValidateOnStart<PluginConfig>().BindConfiguration(configSection);

        var provider = services.BuildServiceProvider();
        _config = provider.GetRequiredService<IOptions<PluginConfig>>().Value;
    }

    public override void ConfigureSharedInterface(IInterfaceManager interfaceManager)
    {
    }

    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
    }

    public override void Load(bool hotReload)
    {
        _commandGuid = Core.Command.HookClientCommand(CommandHandler);
    }

    private HookResult CommandHandler(int playerId, string commandLine)
    {
        var isCommandBlocked = _config.ContainsBlockType
            ? _config.BlockedCommands.Any(cmd => commandLine.Contains(cmd, StringComparison.CurrentCultureIgnoreCase))
            : _config.BlockedCommands.Any(cmd => commandLine.ToLower().Equals(cmd.ToLower()));

        if (!isCommandBlocked)
            return HookResult.Continue;

        var player = Core.PlayerManager.GetPlayer(playerId);
        if (player != null && Core.Permission.PlayerHasPermission(player.SteamID, _config.AllowedPermission))
            return HookResult.Continue;

        if (_config.SendMessage && player is { IsFakeClient: false })
        {
            var localizer = Core.Translation.GetPlayerLocalizer(player);
            player.SendChat(localizer["print_info"].Colored());
            player.SendConsole(localizer["print_info"].Colored());
        }

        return HookResult.Stop;
    }

    public override void Unload()
    {
        Core.Command.UnhookClientCommand(_commandGuid);
    }
}
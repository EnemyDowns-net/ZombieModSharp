using PlayerManager_Shared.Abstractions;
using Sharp.Shared;
using Sharp.Shared.Definition;
using Sharp.Shared.Enums;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using ZombieModSharp.Core.Infection;
using ZombieModSharp.Core.Player;
using ZombieModSharp.Entities;
using ZombieModSharp.Interface.Command;
using ZombieModSharp.Interface.Infection;
using ZombieModSharp.Interface.Player;
using ZombieModSharp.Interface.ZTele;

public class Command : ICommand
{
    private readonly IClientManager _clientManager;
    private readonly IModSharp _modsharp;
    private readonly IPlayer _player;
    private readonly IZTele _ztele;
    private readonly IInfect _infect;
    private readonly IPlayerManager _playerManager;
    private readonly ISharedSystem _sharedSystem;

    private static List<CommandDefinition> commandList = [];

    public Command(IClientManager clientManager, IModSharp modSharp, IPlayer player, IZTele ztele, IInfect infect, IPlayerManager playerManager, ISharedSystem sharedSystem)
    {
        _clientManager = clientManager;
        _modsharp = modSharp;
        _player = player;
        _ztele = ztele;
        _playerManager = playerManager;
        _infect = infect;
        _sharedSystem = sharedSystem;
    }

    public void PostInit()
    {
        // _clientManager.InstallCommandCallback("ms_ztele", ZTeleCommand);
        _clientManager.InstallCommandCallback("ms_infect", InfectCommand);

        CreateCommand("ms_ztele", ZTeleCommand, "Teleport back to spawn.");
    }

    public void OnAllModulesLoaded(ISharedSystem sharedSystem)
    {
        foreach (var command in commandList)
        {
            _sharedSystem.GetClientManager().InstallCommandCallback(command.ConsoleName, (client, cmd) => OnClientUseCommand(client, cmd, command.Action));
        }
    }

    private ECommandAction OnClientUseCommand(IGameClient client, StringCommand command, Action<ISharedSystem, IGamePlayer, IGameClient?, StringCommand> action)
    {
        var result = HandlePlayerTargets(client, command, (target) =>
        {
            action(_sharedSystem, target, client, command);
        });

        return result;
    }

    public static CommandDefinition CreateCommand(string consoleName, Action<ISharedSystem, IGamePlayer, IGameClient?, StringCommand> action, string description = "")
    {
        string name = consoleName.Replace("ms_", "");
        var newCommand = new CommandDefinition(name, consoleName, description, action);
        commandList.Add(newCommand);
        return newCommand;
    }

    /*
    public ECommandAction ZTeleCommand(IGameClient client, StringCommand command)
    {
        var playerInfo = _player.GetPlayer(client);

        var receiver = new RecipientFilter(client.Slot);

        if (client == null || playerInfo == null)
        {
            _modsharp.PrintChannelFilter(HudPrintChannel.Chat, "Invalid Client.", receiver);
            return ECommandAction.Handled;
        }

        _modsharp.PrintChannelFilter(HudPrintChannel.Chat, "Teleport back to spawn.", receiver);
        _ztele.TeleportToSpawn(client);
        return ECommandAction.Skipped;
    }
    */

    public ECommandAction InfectCommand(IGameClient client, StringCommand command)
    {
        var playerInfo = _player.GetPlayer(client);

        var receiver = new RecipientFilter(client.Slot);

        if (client == null || playerInfo == null)
        {
            _modsharp.PrintChannelFilter(HudPrintChannel.Chat, "Invalid Client.", receiver);
            return ECommandAction.Handled;
        }

        _modsharp.PrintChannelFilter(HudPrintChannel.Chat, "You have been infected!", receiver);
        _infect.InfectPlayer(client);
        return ECommandAction.Skipped;
    }

    private ECommandAction HandlePlayerTargets(
            IGameClient? client,
            StringCommand command,
            Action<IGamePlayer> action)
    {
        string selector = "@me";
        if (command.ArgCount > 0 && !string.IsNullOrWhiteSpace(command.ArgString))
            selector = command.ArgString.Trim();

        var allPlayers = _playerManager!.GetPlayers();

        var targets = selector switch
        {
            "@me" when client != null => allPlayers.Where(p => p.Client.Equals(client)).ToArray(),
            "@ct" => allPlayers.Where(p =>
            {
                var controller = _sharedSystem.GetEntityManager().FindPlayerControllerBySlot(p.Client.Slot);
                return controller?.Team == CStrikeTeam.CT;
            }).ToArray(),
            "@t" => allPlayers.Where(p =>
            {
                var controller = _sharedSystem.GetEntityManager().FindPlayerControllerBySlot(p.Client.Slot);
                return controller?.Team == CStrikeTeam.TE;
            }).ToArray(),
            _ => allPlayers.Where(p =>
                !string.IsNullOrWhiteSpace(p.Name) &&
                (p.Name.Equals(selector, StringComparison.OrdinalIgnoreCase) ||
                 p.Name.StartsWith(selector, StringComparison.OrdinalIgnoreCase))
            ).ToArray()
        };

        if (targets.Length == 0)
        {
            if (client != null)
                client.SayChatMessage(false, $"{ChatColor.Red}[ADMCommands]{ChatColor.White} 找不到符合條件的玩家：{selector}");
            else
                Console.WriteLine($"找不到符合條件的玩家：{selector}");
            return ECommandAction.Handled;
        }

        foreach (var target in targets)
        {
            action(target);
        }

        return ECommandAction.Handled;
    }
}
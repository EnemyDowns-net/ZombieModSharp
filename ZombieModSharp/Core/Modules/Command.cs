using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.Modules;

public class Command : ICommand
{
    private readonly IPlayerManager _player;
    private readonly IZTele _ztele;
    private readonly IInfect _infect;
    private readonly ISharedSystem _sharedSystem;
    private readonly IClientManager _clientManager;
    private readonly IModSharp _modsharp;

    public Command(IPlayerManager player, IZTele ztele, IInfect infect, ISharedSystem sharedSystem)
    {
        _player = player;
        _ztele = ztele;
        _infect = infect;
        _sharedSystem = sharedSystem;
        _clientManager = _sharedSystem.GetClientManager();
        _modsharp = _sharedSystem.GetModSharp();
    }

    public void PostInit()
    {
        _clientManager.InstallCommandCallback("ztele", ZTeleCommand);
        _clientManager.InstallCommandCallback("infect", InfectCommand);
        _clientManager.InstallCommandCallback("human", HumanizeCommand);
    }

    private ECommandAction ZTeleCommand(IGameClient client, StringCommand command)
    {
        var playerInfo = _player.GetPlayer(client);

        if (client == null || playerInfo == null)
            return ECommandAction.Handled;

        ReplyToCommand(client, "Teleport back to spawn.");
        _ztele.TeleportToSpawn(client);
        return ECommandAction.Skipped;
    }

    private ECommandAction InfectCommand(IGameClient client, StringCommand command)
    {
        if (command.ArgCount < 1)
        {
            ReplyToCommand(client, "Usage: ms_infect <target>");
            return ECommandAction.Stopped;
        }

        var arg = command.GetArg(1);
        var target = GetTargets(client, arg);

        if (target == null || target.Count == 0)
        {
            ReplyToCommand(client, "No target is found");
            return ECommandAction.Stopped;
        }

        foreach (var player in target)
        {
            _infect.InfectPlayer(player, null, false, true);
            _modsharp.PrintChannelAll(HudPrintChannel.Chat, $"Admin {client.Name} has infected {player.Name} via command");
        }

        return ECommandAction.Skipped;
    }

    private ECommandAction HumanizeCommand(IGameClient client, StringCommand command)
    {
        if (command.ArgCount < 1)
        {
            ReplyToCommand(client, "Usage: ms_infect <target>");
            return ECommandAction.Stopped;
        }

        var arg = command.GetArg(1);
        var target = GetTargets(client, arg);

        if (target == null || target.Count == 0)
        {
            ReplyToCommand(client, "No target is found");
            return ECommandAction.Stopped;
        }

        foreach (var player in target)
        {
            _infect.HumanizeClient(player, true);
            _modsharp.PrintChannelAll(HudPrintChannel.Chat, $"Admin {client.Name} has revived {player.Name} via command");
        }

        return ECommandAction.Skipped;
    }

    private void ReplyToCommand(IGameClient client, string text)
    {
        if (client == null)
        {
            Console.WriteLine(text);
            return;
        }

        else
        {
            var receiver = new RecipientFilter(client.Slot);
            _modsharp.PrintChannelFilter(HudPrintChannel.Chat, "Teleport back to spawn.", receiver);
        }
    }

    private List<IGameClient> GetTargets(IGameClient? sender, string target)
    {
        var targets = new List<IGameClient>();

        if (string.Equals(target, "@all", StringComparison.OrdinalIgnoreCase))
        {
            targets.AddRange(_player.GetAllPlayers().Select(p => p.Key));
        }
        else if (string.Equals(target, "@me", StringComparison.OrdinalIgnoreCase))
        {
            if (sender != null)
                targets.Add(sender);
        }
        else if (string.Equals(target, "@zombies", StringComparison.OrdinalIgnoreCase))
        {
            targets.AddRange(_player.GetAllPlayers().Where(p => p.Value.IsInfected()).Select(p => p.Key));
        }
        else if (string.Equals(target, "@humans", StringComparison.OrdinalIgnoreCase))
        {
            targets.AddRange(_player.GetAllPlayers().Where(p => !p.Value.IsInfected()).Select(p => p.Key));
        }
        else if (string.Equals(target, "@ct", StringComparison.OrdinalIgnoreCase))
        {
            targets.AddRange(_player.GetAllPlayers().Where(p =>
                p.Key.GetPlayerController()?.Team == CStrikeTeam.CT).Select(p => p.Key));
        }
        else if (string.Equals(target, "@t", StringComparison.OrdinalIgnoreCase))
        {
            targets.AddRange(_player.GetAllPlayers().Where(p =>
                p.Key.GetPlayerController()?.Team == CStrikeTeam.TE).Select(p => p.Key));
        }
        else if (string.Equals(target, "@bot", StringComparison.OrdinalIgnoreCase))
        {
            targets.AddRange(_player.GetAllPlayers().Where(p => p.Key.IsFakeClient).Select(p => p.Key));
        }

        // find the name of 
        else
        {
            targets.AddRange(_player.GetAllPlayers().Where(p => p.Key.Name.Contains(target, StringComparison.OrdinalIgnoreCase)).Select(p => p.Key));
        }

        return targets;
    }
}
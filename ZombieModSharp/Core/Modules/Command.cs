using Sharp.Extensions.CommandManager;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using ZombieModSharp.Abstractions;
using ZombieModSharp.Abstractions.Storage;

namespace ZombieModSharp.Core.Modules;

public class Command : ICommand
{
    private readonly IPlayerManager _player;
    private readonly IZTele _ztele;
    private readonly IInfect _infect;
    private readonly ISharedSystem _sharedSystem;
    private readonly IModSharp _modsharp;
    private readonly ICommandManager _command;
    private readonly ISqliteDatabase _sqlite;
    private readonly ICvarServices _cvarServices;

    public Command(IPlayerManager player, IZTele ztele, IInfect infect, ISharedSystem sharedSystem, ICommandManager command, ISqliteDatabase sqlite, ICvarServices cvarServices)
    {
        _player = player;
        _ztele = ztele;
        _infect = infect;
        _sharedSystem = sharedSystem;
        _modsharp = _sharedSystem.GetModSharp();
        _command = command;
        _sqlite = sqlite;
        _cvarServices = cvarServices;
    }

    public void PostInit()
    {
        _command.RegisterClientCommand("ztele", ZTeleCommand);
        _command.RegisterAdminCommand("infect", InfectCommand, "slay");
        _command.RegisterAdminCommand("human", HumanizeCommand, "slay");
        _command.RegisterClientCommand("zsound", ZSoundCommand);
    }

    private void ZTeleCommand(IGameClient client, StringCommand command)
    {
        var playerInfo = _player.GetOrCreatePlayer(client);

        if (client == null || playerInfo == null)
            return;
        
        var allow = _cvarServices.CvarList["Cvar_ZTeleAllow"]?.GetBool();

        if(allow.HasValue && !allow.Value)
        {
            ReplyToCommand(client, "This feature is not available.");
            return;
        }

        var delay = _cvarServices.CvarList["Cvar_ZTeleDelay"]?.GetFloat();

        if(delay > 0)
        {
            ReplyToCommand(client, $"Teleport back to spawn in {delay} seconds.");
            _modsharp.PushTimer(new Func<TimerAction>(() => 
            {
                _ztele.TeleportToSpawn(client);
                return TimerAction.Continue;
            }), delay.Value, GameTimerFlags.StopOnRoundEnd|GameTimerFlags.StopOnMapEnd);
        }
        else
        {
            ReplyToCommand(client, $"Teleport back to spawn in.");
        }

        return;
    }

    private void InfectCommand(IGameClient client, StringCommand command)
    {
        if (command.ArgCount < 1)
        {
            ReplyToCommand(client, "Usage: ms_infect <target>");
            return;
        }

        var arg = command.GetArg(1);
        var target = GetTargets(client, arg);

        if (target == null || target.Count == 0)
        {
            ReplyToCommand(client, "No target is found");
            return;
        }

        var motherzombie = !_infect.IsInfectStarted();

        foreach (var player in target)
        {
            _infect.InfectPlayer(player, null, motherzombie, true);
            _modsharp.PrintChannelAll(HudPrintChannel.Chat, $"{ZombieModSharp.Prefix} Admin {client.Name} has infected {player.Name} via command");
        }

        return;
    }

    private void HumanizeCommand(IGameClient client, StringCommand command)
    {
        if (command.ArgCount < 1)
        {
            ReplyToCommand(client, "Usage: ms_human <target>");
            return;
        }

        var arg = command.GetArg(1);
        var target = GetTargets(client, arg);

        if (target == null || target.Count == 0)
        {
            ReplyToCommand(client, "No target is found");
            return;
        }

        foreach (var player in target)
        {
            _infect.HumanizeClient(player, true);
            _modsharp.PrintChannelAll(HudPrintChannel.Chat, $"{ZombieModSharp.Prefix} Admin {client.Name} has revived {player.Name} via command");
        }

        return;
    }

    private void ZSoundCommand(IGameClient client, StringCommand command)
    {
        var player = _player.GetOrCreatePlayer(client);
        var arg = command.GetArg(1);

        // we need to check if arg is number or not.
        if(!float.TryParse(arg, out var volume))
        {
            // we just keep the same value.
            volume = player.SoundVolume;
        }

        player.SoundEnabled = !player.SoundEnabled;

        // whatever happened here is we will need to insert it.
        _modsharp.InvokeFrameActionAsync(async () => {
            var success = await _sqlite.InsertPlayerSoundAsync(client.SteamId.ToString(), player.SoundEnabled, volume);
            ReplyToCommand(client, $"You have{(player.SoundEnabled ? "\x04 Enabled" : "\x04 Disabled")}\x01 zombie sound. {(player.SoundEnabled ? $"And set volume to {(int)player.SoundVolume}" : string.Empty)}");
        });
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
            _modsharp.PrintChannelFilter(HudPrintChannel.Chat, $"{ZombieModSharp.Prefix} {text}", receiver);
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
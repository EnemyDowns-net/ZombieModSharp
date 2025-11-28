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
    private readonly IGrenadeEffect _grenadeEffect;

    public Command(IPlayerManager player, IZTele ztele, IInfect infect, ISharedSystem sharedSystem, ICommandManager command, ISqliteDatabase sqlite, ICvarServices cvarServices, IGrenadeEffect grenadeEffect)
    {
        _player = player;
        _ztele = ztele;
        _infect = infect;
        _sharedSystem = sharedSystem;
        _modsharp = _sharedSystem.GetModSharp();
        _command = command;
        _sqlite = sqlite;
        _cvarServices = cvarServices;
        _grenadeEffect = grenadeEffect;
    }

    public void PostInit()
    {
        _command.RegisterClientCommand("ztele", ZTeleCommand);
        _command.RegisterAdminCommand("infect", InfectCommand, "slay");
        _command.RegisterAdminCommand("human", HumanizeCommand, "slay");
        _command.RegisterClientCommand("zsound", ZSoundCommand);
        _command.RegisterAdminCommand("togglerespawn", ToggleRespawnCommand, "slay");
        _command.RegisterAdminCommand("burnme", BurnTestCommand, "slay");
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

        if(_modsharp.GetGameRules().IsWarmupPeriod && (!_cvarServices.CvarList["Cvar_InfectWarmupEnabled"]?.GetBool() ?? false))
        {
            ReplyToCommand(client, "The infection during the warmup is not available.");
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
        float volume = 100.0f;

        if(command.ArgCount < 1)
        {
            player.SoundEnabled = !player.SoundEnabled;
            _modsharp.PrintToChatAll("This shit is not even one");
        }

        else
        {
            var arg = command.GetArg(1);

            // we need to check if arg is number or not.
            if(!float.TryParse(arg, out volume))
            {
                // we just keep the same value.
                volume = player.SoundVolume;
            }

            if(volume < 0 || volume > 100)
            {
                ReplyToCommand(client, $"Usage: ms_zsound <0-100>");
                return;
            }

            player.SoundVolume = volume;
        }

        // whatever happened here is we will need to insert it.
        _modsharp.InvokeFrameActionAsync(async () => {
            var success = await _sqlite.InsertPlayerSoundAsync(client.SteamId.ToString(), player.SoundEnabled, volume);
            ReplyToCommand(client, $"You have{(player.SoundEnabled ? "\x05 Enabled" : "\x07 Disabled")}\x01 zombie sound. {(player.SoundEnabled ? $"And set volume to\x06 {(int)player.SoundVolume}" : string.Empty)}");
        });
    }

    private void ToggleRespawnCommand(IGameClient client, StringCommand command)
    {
        if(command.ArgCount < 1)
        {
            var enabled = RespawnServices.IsRespawnEnabled();
            RespawnServices.SetRespawnEnable(!enabled);

            _modsharp.PrintToChatAll($"{ZombieModSharp.Prefix} Respawn has been{(!enabled ? "\x07 Disabled" : "\x05 Enabled")}");
            return;
        }

        if(!int.TryParse(command.GetArg(1), out var arg))
        {
            ReplyToCommand(client, "Usage ms_togglerespawn <0-1>");
            return;
        }

        if(arg > 1 || arg < 0)
        {
            ReplyToCommand(client, "Usage ms_togglerespawn <0-1>");
            return;
        }

        RespawnServices.SetRespawnEnable(Convert.ToBoolean(arg));
        _modsharp.PrintToChatAll($"{ZombieModSharp.Prefix} Respawn has been{(Convert.ToBoolean(arg) ? "\x05 Enabled" : "\x07 Disabled")}");
    }

    private void BurnTestCommand(IGameClient client, StringCommand command)
    {
        var player = client.GetPlayerController()?.GetPlayerPawn();

        ReplyToCommand(client, "Test burn");
        _grenadeEffect.IgnitePawn(player, 1, 5);
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
            targets.AddRange(_player.GetAllPlayers().Where(p => p.Key.IsFakeClient && !p.Key.IsHltv).Select(p => p.Key));
        }

        // find the name of 
        else
        {
            targets.AddRange(_player.GetAllPlayers().Where(p => p.Key.Name.Contains(target, StringComparison.OrdinalIgnoreCase)).Select(p => p.Key));
        }

        return targets;
    }
}
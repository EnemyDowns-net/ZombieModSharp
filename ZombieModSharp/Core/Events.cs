using Microsoft.Extensions.Logging;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Units;
using ZombieModSharp.Interface.Events;
using ZombieModSharp.Interface.Player;
using Sharp.Shared.Enums;
using ZombieModSharp.Interface.Infection;
using Sharp.Shared;
using ZombieModSharp.Interface.ZTele;
using TnmsPluginFoundation;
using Microsoft.Extensions.DependencyInjection;

namespace ZombieModSharp.Core;

public class Events : IEventListener
{
    private readonly TnmsPlugin _plugin;
    public int ListenerVersion => IEventListener.ApiVersion;
    public int ListenerPriority => 0;

    private readonly ILogger<Events> _logger;
    private readonly IEventManager _eventManager;
    private readonly IClientManager _clientManager;
    private readonly IEntityManager _entityManager;
    private readonly IPlayer _player;
    private readonly IInfect _infect;
    private readonly IModSharp _modSharp;
    private readonly IZTele _ztele;

    public bool RoundEnded { get; private set; } = false;

    public Events(IServiceProvider serviceProvider, ILogger<Events> logger, IPlayer player, IInfect infect, IZTele ztele)
    {
        _plugin = serviceProvider.GetRequiredService<TnmsPlugin>();
        _logger = logger;
        _eventManager = _plugin.SharedSystem.GetEventManager();
        _clientManager = _plugin.SharedSystem.GetClientManager();
        _modSharp = _plugin.SharedSystem.GetModSharp();
        _entityManager = _plugin.SharedSystem.GetEntityManager();
        _infect = infect;
        _player = player;
        _ztele = ztele;
    }

    public void RegisterEvents()
    {
        _eventManager.HookEvent("player_hurt");
        _eventManager.HookEvent("player_death");
        _eventManager.HookEvent("round_end");
        _eventManager.HookEvent("cs_pre_restart");
        _eventManager.HookEvent("round_start");
    }

    public void FireGameEvent(IGameEvent e)
    {
        var eventName = e.Name?.ToLowerInvariant();

        switch (eventName)
        {
            case "player_hurt":
                OnPlayerHurt(e);
                break;
            case "player_death":
                OnPlayerDeath(e);
                break;
            case "round_end":
                OnRoundEnd(e);
                break;
            case "round_start":
                OnRoundStart(e);
                break;
            case "player_spawn":
                OnPlayerSpawn(e);
                break;
            case "cs_pre_restart":
                OnPreRestart(e);
                break;
            default:
                break;
        }
    }

    private void OnPlayerHurt(IGameEvent e)
    {
        var userId = new UserID((ushort)e.GetInt("userid"));
        var client = _clientManager.GetGameClient(userId);

        var attackerId = new UserID((ushort)e.GetInt("attacker"));
        var attackerClient = _clientManager.GetGameClient(attackerId);

        if (client == null || attackerClient == null)
        {
            return;
        }

        if (_infect.IsInfectStarted() == false)
        {
            return;
        }

        var zmClient = _player.GetPlayer(client);
        var zmAttacker = _player.GetPlayer(attackerClient);

        if (zmClient.IsHuman() && zmAttacker.IsInfected())
        {
            // Infect the player.
            _infect.InfectPlayer(client, attackerClient);
        }
        else if (zmClient.IsInfected() && zmAttacker.IsHuman())
        {
            // Get weapon and calculate damage and knockback.
            var weapon = e.GetString("weapon");
            var damage = e.GetInt("dmg_health");
        }
    }

    private void OnPlayerDeath(IGameEvent e)
    {
        //_infect.CheckGameStatus();

        var userId = new UserID((ushort)e.GetInt("userid"));
        var client = _clientManager.GetGameClient(userId);

        var attackerId = new UserID((ushort)e.GetInt("attacker"));
        var attackerClient = _clientManager.GetGameClient(attackerId);

        _modSharp.PrintChannelAll(HudPrintChannel.Chat, $"Client {client?.Name ?? "Unknown Player"} killed by {attackerClient?.Name ?? "Unknown Player"}");
    }

    private void OnPreRestart(IGameEvent e)
    {
        _infect.OnRoundPreStart();
    }

    private void OnRoundStart(IGameEvent e)
    {
        RoundEnded = false;
        //_modSharp.PrintChannelAll(HudPrintChannel.Chat, $"The round just started");
        _infect.OnRoundStart();
    }

    private void OnRoundEnd(IGameEvent e)
    {
        RoundEnded = true;
        //_modSharp.PrintChannelAll(HudPrintChannel.Chat, $"The round just ended");
        // _infect.OnRoundEnd();
        _infect.OnRoundEnd();
    }

    private void OnPlayerSpawn(IGameEvent e)
    {
        var userId = new UserID((ushort)e.GetInt("userid"));
        var client = _clientManager.GetGameClient(userId);

        _modSharp.PrintChannelAll(HudPrintChannel.Chat, $"Client {client?.Name ?? "Unknown Player"} Spawned");
        _logger.LogInformation("PlayerSpawn: {Name}", client?.Name ?? "Unknown Player");

        // go apply spawn stuff.
        // ignore Spec and none team
        if (client == null)
            return;

        var clientEnt = _entityManager.FindPlayerControllerBySlot(client.Slot);

        var team = clientEnt?.Team ?? CStrikeTeam.UnAssigned;

        if (team == CStrikeTeam.UnAssigned || team == CStrikeTeam.Spectator)
            return;

        if (_infect.IsInfectStarted())
        {
            // infect or
            var timer = _modSharp.PushTimer(() => { _infect.InfectPlayer(client); }, 0.05, GameTimerFlags.None);
        }

        else
        {
            var timer = _modSharp.PushTimer(() => { _infect.HumanizeClient(client); }, 0.05, GameTimerFlags.None);
        }

        _ztele.OnPlayerSpawn(client);
    }
}
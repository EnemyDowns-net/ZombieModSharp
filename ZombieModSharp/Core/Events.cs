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

namespace ZombieModSharp.Core;

public class Events : IEvents, IEventListener
{
    private readonly IEventManager _eventManager;
    private readonly ILogger<Events> _logger;
    private readonly IClientManager _clientManager;
    private readonly IPlayer _player;
    private readonly IEntityManager _entityManager;
    private readonly IInfect _infect;
    private readonly IModSharp _modSharp;

    public int ListenerVersion => IEventListener.ApiVersion;
    public int ListenerPriority => 0;

    public bool RoundEnded { get; private set; } = false;

    public Events(IEventManager eventManager, ILogger<Events> logger, IClientManager clientManager, IPlayer player, IEntityManager entityManager, IInfect infect, IModSharp modSharp)
    {
        _eventManager = eventManager;
        _logger = logger;
        _clientManager = clientManager;
        _player = player;
        _modSharp = modSharp;
        _infect = infect;
        _entityManager = entityManager;
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

        /*
        if (client == null || attackerClient == null)
        {
            return;
        }

        var clientController = _entityManager.FindPlayerControllerBySlot(client.Slot);
        var attackerController = _entityManager.FindPlayerControllerBySlot(attackerClient.Slot);

        if (clientController == null || attackerController == null)
        {
            return;
        }

        if (clientController.Team == CStrikeTeam.CT && attackerController.Team == CStrikeTeam.TE)
        {
            var zmPlayer = _player.GetPlayer(attackerClient);

            if (zmPlayer.IsInfected)
            {
                // Infect the player.
                _infect.InfectPlayer(client, attackerClient);
            }
        }
        */
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

    private void OnRoundStart(IGameEvent e)
    {
        RoundEnded = false;
        _modSharp.PrintChannelAll(HudPrintChannel.Chat, $"The round just started");
    }

    private void OnRoundEnd(IGameEvent e)
    {
        RoundEnded = true;
        _modSharp.PrintChannelAll(HudPrintChannel.Chat, $"The round just ended");
        // _infect.OnRoundEnd();
    }

    private void OnPlayerSpawn(IGameEvent e)
    {
        var userId = new UserID((ushort)e.GetInt("userid"));
        var client = _clientManager.GetGameClient(userId);

        _modSharp.PrintChannelAll(HudPrintChannel.Chat, $"Client {client?.Name ?? "Unknown Player"} Spawned");
        _logger.LogInformation("PlayerSpawn: {Name}", client?.Name ?? "Unknown Player");
        /*
        var userId = new UserID((ushort)e.GetInt("userid"));
        var client = _clientManager.GetGameClient(userId);

        if (client == null)
        {
            return;
        }

        var zmPlayer = _player.GetPlayer(client);
        */
    }
}
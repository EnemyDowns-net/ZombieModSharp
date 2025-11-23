using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEvents;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Units;
using ZombieModSharp.Abstractions;
using ZombieModSharp.Core.Modules;

namespace ZombieModSharp.Core.HookManager;

public class Events : IEvents, IEventListener
{
    private readonly ISharedSystem _sharedSystem;
    private readonly IEventManager _eventManager;
    private readonly ILogger<Events> _logger;
    private readonly IPlayerManager _playerManager;
    private readonly IInfect _infect;
    private readonly IModSharp _modSharp;
    private readonly IZTele _ztele;
    private readonly IKnockback _knockback;
    private readonly ICvarServices _cvarServices;
    private readonly ISoundServices _soundServices;
    private readonly IRespawnServices _respawnServices;

    public int ListenerVersion => IEventListener.ApiVersion;
    public int ListenerPriority => 0;

    public bool RoundEnded { get; private set; } = false;

    public Events(ISharedSystem sharedSystem, ILogger<Events> logger, IPlayerManager playerManager, IInfect infect, IZTele ztele, IKnockback knockback, ICvarServices cvarServices, ISoundServices soundServices, IRespawnServices respawnServices)
    {
        _sharedSystem = sharedSystem;
        _eventManager = _sharedSystem.GetEventManager();
        _logger = logger;
        _playerManager = playerManager;
        _modSharp = _sharedSystem.GetModSharp();
        _infect = infect;
        _ztele = ztele;
        _knockback = knockback;
        _cvarServices = cvarServices;
        _soundServices = soundServices;
        _respawnServices = respawnServices;
    }

    public void Init()
    {
        _eventManager.InstallEventListener(this);
        RegisterEvents();
    }

    public void Shutdown()
    {
        _eventManager.RemoveEventListener(this);
    }

    public void RegisterEvents()
    {
        _eventManager.HookEvent("player_hurt");
        _eventManager.HookEvent("player_death");
        _eventManager.HookEvent("player_spawn");
        _eventManager.HookEvent("round_end");
        _eventManager.HookEvent("cs_pre_restart");
        _eventManager.HookEvent("round_start");
        _eventManager.HookEvent("round_freeze_end");
        _eventManager.HookEvent("warmup_end");
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
            case "round_freeze_end":
                OnRoundFreezeEnd(e);
                break;
            case "warmup_end":
                OnWarmupEnd(e);
                break;
            default:
                break;
        }
    }

    private void OnPlayerHurt(IGameEvent e)
    {
        var client = e.GetPlayerController("userid")?.GetGameClient();
        var attackerClient = e.GetPlayerController("attacker")?.GetGameClient();
        var weapon = e.GetString("weapon");

        if (client == null || attackerClient == null)
        {
            return;
        }

        if (_infect.IsInfectStarted() == false)
        {
            return;
        }

        var zmClient = _playerManager.GetOrCreatePlayer(client);
        var zmAttacker = _playerManager.GetOrCreatePlayer(attackerClient);

        if (zmClient.IsHuman() && zmAttacker.IsInfected())
        {
            if (weapon.Contains("knife"))
            {
                // Infect the player.
                _infect.InfectPlayer(client, attackerClient);
            }
        }
        else if (zmClient.IsInfected() && zmAttacker.IsHuman())
        {
            // Get weapon and calculate damage and knockback.
            var damage = e.GetInt("dmg_health");
            var hitGroup = e.GetInt("hitgroup");
            var pawn = client.GetPlayerController()?.GetPlayerPawn();

            if(pawn != null)
            {
                if(Infect.CashMultiply > 0)
                {
                    var accountService = attackerClient.GetPlayerController()?.GetInGameMoneyService();

                    if(accountService != null)
                    {
                        accountService.Account += (int)Math.Ceiling(damage * Infect.CashMultiply);
                    }
                }

                _soundServices.ZombieHurtSound(pawn);
                _knockback.KnockbackClient(client, attackerClient, weapon, damage, hitGroup);
            }
        }
    }

    private void OnPlayerDeath(IGameEvent e)
    {
        //_infect.CheckGameStatus();
        var client = e.GetPlayerController("userid")?.GetGameClient();
        var controller = e.GetPlayerController("userid");

        if(client == null)
            return;

        if(_playerManager.GetOrCreatePlayer(client).IsInfected())
        {
            var pawn = e.GetPlayerController("userid")?.GetPlayerPawn();

            if(pawn != null)
                _soundServices.EmitZombieSound(pawn, "zr.amb.zombie_die");
        }

        _respawnServices.InitRespawn(controller);
        _infect.CheckGameStatus();
        //_modSharp.PrintChannelAll(HudPrintChannel.Chat, $"Client {client?.Name ?? "Unknown Player"} killed by {attackerClient?.Name ?? "Unknown Player"}");
    }

    private void OnPreRestart(IGameEvent e)
    {
        _infect.OnRoundPreStart();
    }

    private void OnRoundFreezeEnd(IGameEvent e)
    {
        // start infection.
        // _modSharp.PrintChannelAll(HudPrintChannel.Chat, "Infect round freeze is called");
        _infect.OnRoundFreezeEnd();
    }

    private void OnRoundStart(IGameEvent e)
    {
        RoundEnded = false;
        //_modSharp.PrintChannelAll(HudPrintChannel.Chat, $"The round just started");
        _infect.OnRoundStart();

        if(_cvarServices.CvarList["Cvar_RespawnEnabled"]?.GetBool() ?? false)
            RespawnServices.SetRespawnEnable(true);

        if(_cvarServices.CvarList["Cvar_RespawnTogglerEnable"]?.GetBool() ?? false)
            _respawnServices.SetupRespawnToggler();
    }

    private void OnRoundEnd(IGameEvent e)
    {
        RoundEnded = true;
        //_modSharp.PrintChannelAll(HudPrintChannel.Chat, $"The round just ended");
        _infect.OnRoundEnd();
    }

    private void OnWarmupEnd(IGameEvent e)
    {
        _infect.SetInfectStarted(false);
    }

    private void OnPlayerSpawn(IGameEvent e)
    {
        var pawn = e.GetPlayerController("userid");
        var client = pawn?.GetGameClient();

        // _modSharp.PrintChannelAll(HudPrintChannel.Chat, $"Client {client?.Name ?? "Unknown Player"} Spawned");
        // _logger.LogInformation("PlayerSpawn: {Name}", client?.Name ?? "Unknown Player");

        // go apply spawn stuff.
        // ignore Spec and none team
        if (client == null)
            return;

        var team = pawn?.Team ?? CStrikeTeam.UnAssigned;

        if (team == CStrikeTeam.UnAssigned || team == CStrikeTeam.Spectator)
            return;

            // infect or
        _modSharp.PushTimer(() =>
        {
            var teamRespawn = _cvarServices.CvarList["Cvar_RespawnTeam"]?.GetInt32() ?? 0;

            if (_infect.IsInfectStarted())
            {
                if(teamRespawn == 0)
                    _infect.InfectPlayer(client);

                else if(teamRespawn == 1)
                    _infect.HumanizeClient(client);

                else
                {
                    var zombie = _playerManager.GetOrCreatePlayer(client).IsInfected();

                    if(zombie)
                        _infect.InfectPlayer(client);

                    else
                        _infect.HumanizeClient(client);
                }
            }
            // regardless of wtf happenned here, before infection start all player should be spawn as human.
            else
                _infect.HumanizeClient(client);

            _ztele.OnPlayerSpawn(client);
            // this is for noblock
            var noblock = _cvarServices.CvarList["Cvar_InfectNoblockEnable"]?.GetBool() ?? false;

            // _modSharp.PrintToChatAll($"Noblock = {noblock}");

            if(noblock)
            {
                pawn?.GetPlayerPawn()?.SetCollisionGroup(CollisionGroupType.Debris);
            }
        }, 0.05, GameTimerFlags.None | GameTimerFlags.StopOnMapEnd | GameTimerFlags.StopOnRoundEnd);
    }
}
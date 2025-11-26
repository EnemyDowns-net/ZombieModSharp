using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using ZombieModSharp.Abstractions;
using ZombieModSharp.Abstractions.Entities;
using ZombieModSharp.Abstractions.Storage;
using ZombieModSharp.Core.Modules;

namespace ZombieModSharp.Core.HookManager;

public class Listeners : IListeners, IClientListener, IGameListener, IEntityListener
{
    int IClientListener.ListenerVersion  => IClientListener.ApiVersion;
    int IClientListener.ListenerPriority => 0;
    int IEntityListener.ListenerVersion  => IEntityListener.ApiVersion;
    int IEntityListener.ListenerPriority => 0;
    int IGameListener.ListenerVersion  => IGameListener.ApiVersion;
    int IGameListener.ListenerPriority => 0;

    private readonly IPlayerManager _playerManager;
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<Listeners> _logger;
    private readonly IModSharp _modsharp;
    private readonly ISqliteDatabase _sqlite;
    private readonly ICvarServices _cvarServices;
    private readonly IPlayerClasses _playerClasses;
    private readonly IPrecacheManager _precacheManager;
    private readonly IRespawnServices _respawnServices;
    private readonly IEntityManager _entityManager;

    public Listeners(IPlayerManager playerManager, ISharedSystem sharedSystem, ISqliteDatabase sqlite, ICvarServices cvarServices, IPlayerClasses playerClasses, IPrecacheManager precacheManager, IRespawnServices respawnServices)
    {
        _playerManager = playerManager;
        _sharedSystem = sharedSystem;
        _logger = _sharedSystem.GetLoggerFactory().CreateLogger<Listeners>();
        _modsharp = _sharedSystem.GetModSharp();
        _sqlite = sqlite;
        _cvarServices = cvarServices;
        _playerClasses = playerClasses;
        _precacheManager = precacheManager;
        _respawnServices = respawnServices;
        _entityManager = _sharedSystem.GetEntityManager();
    }

    public void Init()
    {
        var clientManager = _sharedSystem.GetClientManager();
        clientManager.InstallClientListener(this);
        clientManager.InstallCommandListener("jointeam", OnJoinTeamCommand);

        _entityManager.InstallEntityListener(this);
        _entityManager.HookEntityInput("logic_relay", "Trigger");
        _entityManager.HookEntityInput("logic_relay", "Enable");
        _entityManager.HookEntityInput("logic_relay", "Disable");
        _modsharp.InstallGameListener(this);
    }

    public void Shutdown()
    {
        var clientManager = _sharedSystem.GetClientManager();
        clientManager.RemoveClientListener(this);
        clientManager.RemoveCommandListener("jointeam", OnJoinTeamCommand);

        _entityManager.RemoveEntityListener(this);
        _modsharp.RemoveGameListener(this);
    }

    public void OnClientPutInServer(IGameClient client)
    {
        //_logger.LogInformation("ClientPutInServer: {Name}", client.Name);
        if (client.IsHltv)
            return;

        var id = client.SteamId.ToString();

        string humanClass = string.Empty;
        string zombieClass = string.Empty;

        _modsharp.InvokeFrameActionAsync(async () => 
        {
            // this is the part of Player classes
            var classes = await _sqlite.GetPlayerClassesAsync(id);

            if (classes == null)
            {
                // _logger.LogInformation("Found nothing.");

                humanClass = _cvarServices.CvarList["Cvar_HumanDefault"]!.GetString();
                zombieClass = _cvarServices.CvarList["Cvar_ZombieDefault"]!.GetString();

                // _logger.LogInformation("Try insert {human} | {zombie}", humanClass, zombieClass);

                var insertResult = await _sqlite.InsertPlayerClassesAsync(id, humanClass, zombieClass);
                //_logger.LogInformation("Insert result for {SteamId}: {Result}", id, insertResult);
            }
            else
            {
                humanClass = classes.HumanClass;
                zombieClass = classes.ZombieClass;

                // _logger.LogInformation("Found {human} | {zombie}", classes.HumanClass, classes.ZombieClass);
            }

            var player = _playerManager.GetOrCreatePlayer(client);

            player.HumanClass = _playerClasses.GetClassByName(humanClass);
            player.ZombieClass = _playerClasses.GetClassByName(zombieClass);

            // this is sound part
            var sound = await _sqlite.GetPlayerSoundAsync(id);

            if(sound == null)
            {
                sound = new SavedSound();
                await _sqlite.InsertPlayerSoundAsync(id, true);
            }

            player.SoundEnabled = sound.Enabled;
            player.SoundVolume = sound.Volume;
        });
    }

    public void OnClientDisconnecting(IGameClient client, NetworkDisconnectionReason reason)
    {
        //_logger.LogInformation("ClientDisconnect: {Name}", client.Name);
        if (client.IsHltv)
            return;

        _playerManager.RemovePlayer(client);
    }
    
    public void OnResourcePrecache()
    {
        // _logger.LogInformation("Precache GoldShip Here");
        //_modsharp.PrecacheResource("characters/models/oylsister/uma_musume/gold_ship/goldship2.vmdl");
        //_modsharp.PrecacheResource("characters/models/s2ze/zombie_frozen/zombie_frozen.vmdl");
        _precacheManager.PrecacheAllResource();
    }

    public void OnGameInit()
    {
        // get map name
        var mapname = _modsharp.GetMapName();

        if(!string.IsNullOrEmpty(mapname))
        {
            _modsharp.ServerCommand($"exec zombiemodsharp/{mapname}.cfg");
        }
    }

    private ECommandAction OnJoinTeamCommand(IGameClient client, StringCommand command)
    {
        var team = (CStrikeTeam)int.Parse(command.GetArg(1));

        var allowJoinLate = _cvarServices.CvarList["Cvar_RespawnLateJoin"]?.GetBool() ?? false;

        if(team == CStrikeTeam.TE || team == CStrikeTeam.CT)
        {
            if(allowJoinLate)
                _respawnServices.InitRespawn(client.GetPlayerController());
        }

        return ECommandAction.Skipped;
    }

    public EHookAction OnEntityAcceptInput(IBaseEntity entity, string input, in EntityVariant value, IBaseEntity? activator, IBaseEntity? caller)
    {
        //_modsharp.PrintToChatAll($"Founded {entity.Name} {input}");

        if(!_cvarServices.CvarList["Cvar_RespawnTogglerEnable"]?.GetBool() ?? false)
            return EHookAction.Ignored;

        var respawner = _respawnServices.GetRespawnToggler();
        if(respawner == null || !respawner.IsValid())
            return EHookAction.Ignored;

        if(entity != respawner)
            return EHookAction.Ignored;

        if (input.Equals("Trigger", StringComparison.OrdinalIgnoreCase))
        {
            _modsharp.PrintToChatAll($"{ZombieModSharp.Prefix} Respawn has been disabled!");
            RespawnServices.SetRespawnEnable(false);
        }

        else if (input.Equals("Enable", StringComparison.OrdinalIgnoreCase) && !RespawnServices.IsRespawnEnabled())
        {
            _modsharp.PrintToChatAll($"{ZombieModSharp.Prefix} Respawn has been enabled!");
            RespawnServices.SetRespawnEnable();
        }

        else if (input.Equals("Disable", StringComparison.OrdinalIgnoreCase) && RespawnServices.IsRespawnEnabled())
        {
            _modsharp.PrintToChatAll($"{ZombieModSharp.Prefix} Respawn has been disabled!");
            RespawnServices.SetRespawnEnable(false);
        }

        return EHookAction.Ignored;
    }
}
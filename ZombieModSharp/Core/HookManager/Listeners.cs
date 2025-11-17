using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;
using ZombieModSharp.Abstractions;
using ZombieModSharp.Abstractions.Entities;
using ZombieModSharp.Abstractions.Storage;
using ZombieModSharp.Core.Modules;

namespace ZombieModSharp.Core.HookManager;

public class Listeners : IListeners, IClientListener, IGameListener, IEntityListener
{
    public int ListenerVersion => IClientListener.ApiVersion;
    public int ListenerPriority => 0;

    private readonly IPlayerManager _playerManager;
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<Listeners> _logger;
    private readonly IModSharp _modsharp;
    private readonly ISqliteDatabase _sqlite;
    private readonly ICvarServices _cvar;
    private readonly IPlayerClasses _playerClasses;
    private readonly IPrecacheManager _precacheManager;

    public Listeners(IPlayerManager playerManager, ISharedSystem sharedSystem, ISqliteDatabase sqlite, ICvarServices cvar, IPlayerClasses playerClasses, IPrecacheManager precacheManager)
    {
        _playerManager = playerManager;
        _sharedSystem = sharedSystem;
        _logger = _sharedSystem.GetLoggerFactory().CreateLogger<Listeners>();
        _modsharp = _sharedSystem.GetModSharp();
        _sqlite = sqlite;
        _cvar = cvar;
        _playerClasses = playerClasses;
        _precacheManager = precacheManager;
    }

    public void Init()
    {
        _sharedSystem.GetClientManager().InstallClientListener(this);
        _sharedSystem.GetEntityManager().InstallEntityListener(this);
        _modsharp.InstallGameListener(this);
    }

    public void Shutdown()
    {
        _sharedSystem.GetClientManager().RemoveClientListener(this);
        _sharedSystem.GetEntityManager().RemoveEntityListener(this);
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

                humanClass = _cvar.CvarList["Cvar_HumanDefault"]!.GetString();
                zombieClass = _cvar.CvarList["Cvar_ZombieDefault"]!.GetString();

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

    public void OnEntityCreated(IBaseEntity entity)
    {
        if(entity.Classname.Contains("weapon_"))
        {
            // _modsharp.PrintToChatAll($"Found {entity.Classname}");
            try {
                _modsharp.PushTimer(() => {
                    var weapon = entity.As<IBaseWeapon>();
                    weapon.GetWeaponData().PrimaryReserveAmmoMax = 1200;
                    weapon.ReserveAmmo = 1200;
                }, 0.07, GameTimerFlags.None|GameTimerFlags.StopOnRoundEnd|GameTimerFlags.StopOnMapEnd);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {ex}", ex.Message);
            }
        }
    }

    public void OnResourcePrecache()
    {
        // _logger.LogInformation("Precache GoldShip Here");
        //_modsharp.PrecacheResource("characters/models/oylsister/uma_musume/gold_ship/goldship2.vmdl");
        //_modsharp.PrecacheResource("characters/models/s2ze/zombie_frozen/zombie_frozen.vmdl");
        _precacheManager.PrecacheAllResource();
    }
}
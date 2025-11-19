using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Objects;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.Modules;

public class RespawnServices : IRespawnServices
{
    private readonly ISharedSystem _sharedSystem;
    private readonly IModSharp _modsharp;
    private readonly ICvarServices _cvarServices;

    private bool RespawnEnabled { get; set; } = true;

    public RespawnServices(ISharedSystem sharedSystem, ICvarServices cvarServices)
    {
        _sharedSystem = sharedSystem;
        _modsharp = _sharedSystem.GetModSharp();
        _cvarServices = cvarServices;
    }

    public void OnPlayerDeath(IGameClient client)
    {
        if(!RespawnEnabled)
            return;

        var delay = _cvarServices.CvarList["Cvar_RespawnDelay"]?.GetFloat() ?? 5.0f;

        _modsharp.PushTimer(() =>
        {
            var playerpawn = client.GetPlayerController()?.GetPlayerPawn();
            RespawnClient(playerpawn);

        }, delay, GameTimerFlags.StopOnRoundEnd|GameTimerFlags.StopOnMapEnd);
    }

    public void RespawnClient(IPlayerPawn? playerPawn)
    {
        if(!RespawnEnabled)
            return;

        if(playerPawn == null)
                return;

        if(playerPawn.IsAlive)
            return;

        playerPawn.GetController()?.Respawn();
    }

    public bool IsRespawnEnabled()
    {
        return RespawnEnabled;
    }

    public void SetRespawnEnable(bool set = true)
    {
        RespawnEnabled = set;
    }
}
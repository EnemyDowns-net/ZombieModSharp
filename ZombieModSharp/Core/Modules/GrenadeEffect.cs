using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.GameEntities;
using Sharp.Shared.GameObjects;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.Modules;

public class GrenadeEffect
{
    private readonly IPlayerManager _playerManager;
    private readonly ISharedSystem _sharedSystem;
    private readonly IEntityManager _entityManager;
    private readonly IModSharp _modsharp;
    private readonly ILogger<GrenadeEffect> _logger;

    public GrenadeEffect(IPlayerManager playerManager, ISharedSystem sharedSystem)
    {
        _playerManager = playerManager;
        _sharedSystem = sharedSystem;
        _entityManager = _sharedSystem.GetEntityManager();
        _modsharp = _sharedSystem.GetModSharp();
        _logger = _sharedSystem.GetLoggerFactory().CreateLogger<GrenadeEffect>();
    }

    public bool IgnitePawn(IPlayerPawn victimPawn,  int damage = 1, float duration = 1)
    {
        if(victimPawn == null)
            return false;

        var particle = _entityManager.FindEntityByHandle(victimPawn.EffectEntityHandle)?.As<IBaseParticle>();

        if(particle != null)
        {
            particle.DissolveStartTime = _modsharp.GetGlobals().CurTime + duration;
            return true;
        }

        particle = _entityManager.CreateEntityByName<IBaseParticle>("info_particle_system");
        
        try 
        {
            particle!.StartActive = true;
            particle.GetControlPointEntities()[0] = victimPawn.Handle;
            particle.DissolveStartTime = _modsharp.GetGlobals().CurTime + duration;
            particle.Teleport(victimPawn.GetAbsOrigin());

            particle.DispatchSpawn();
            particle.AcceptInput("SetParent", victimPawn, null, "!activator");

            victimPawn.EffectEntityHandle = particle.Handle;

            DamagePrompt(victimPawn, damage);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: {ex}", ex.Message);
            return false;
        }

        return true;
    }

    private void DamagePrompt(IPlayerPawn playerPawn, int damage)
    {
        // TakeDamageInfo.CreateFromFire()
    }
}
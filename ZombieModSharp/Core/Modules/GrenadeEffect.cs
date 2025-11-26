using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Managers;
using Sharp.Shared.Types;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.Modules;

public class GrenadeEffect : IGrenadeEffect
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

    public bool IgnitePawn(IPlayerPawn? playerPawn, int damage = 1, float duration = 1)
    {
        if(playerPawn == null)
            return false;

        var particle = _entityManager.FindEntityByHandle(playerPawn.EffectEntityHandle)?.As<IBaseParticle>();

        if(particle != null)
        {
            particle.DissolveStartTime = _modsharp.GetGlobals().CurTime + duration;
            return true;
        }

        var kv = new Dictionary<string, KeyValuesVariantValueItem>
        {
            { "effect_name", "particles/cs2fixes/napalm_fire.vpcf" },
            { "start_active", 1 },
        };

        particle = _entityManager.SpawnEntitySync<IBaseParticle>("info_particle_system", kv);
        
        try 
        {
            if(particle == null)
            {
                _modsharp.PrintToChatAll("Is fucking null");
                return false;
            }

            particle.GetControlPointEntities()[0] = playerPawn.Handle;
            particle.DissolveStartTime = _modsharp.GetGlobals().CurTime + duration;
            particle.Teleport(playerPawn.GetAbsOrigin());
            particle.AcceptInput("SetParent", playerPawn, null, "!activator");

            _modsharp.PrintToChatAll("It does work for some reason");

            playerPawn.EffectEntityHandle = particle.Handle;

            _modsharp.PushTimer(new Func<TimerAction>(() => 
            {
                if(playerPawn == null || !playerPawn.IsValid())
                    return TimerAction.Stop;

                if(particle == null || !particle.IsValidEntity)
                    return TimerAction.Stop;

                if(!particle.Classname.StartsWith("info_part"))
                {
                    _logger.LogError("Unexpceted entity is found in particle.");
                    return TimerAction.Stop;
                }

                if(particle.DissolveStartTime <= _modsharp.GetGlobals().CurTime || !playerPawn.IsAlive)
                {
                    particle.AcceptInput("Stop");
                    particle.AddIOEvent(0.1f, "Kill");
                    return TimerAction.Stop;
                }

                ApplyDamage(playerPawn, damage);
                playerPawn.VelocityModifier = 40.0f / 100.0f;
                return TimerAction.Continue;
            }), 0.5, GameTimerFlags.Repeatable|GameTimerFlags.StopOnRoundEnd|GameTimerFlags.StopOnMapEnd);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: {ex}", ex.Message);
            return false;
        }

        return true;
    }

    private void ApplyDamage(IPlayerPawn playerPawn, int damage)
    {
        playerPawn.DispatchTraceAttack(new TakeDamageInfo
        {
            Inflictor = playerPawn.Handle,
            Ability = playerPawn.Handle,
            Damage = damage,
            DamageType = DamageFlagBits.Fall,
            TakeDamageFlags = TakeDamageFlags.IgnoreArmor
        }, true);
    }
}
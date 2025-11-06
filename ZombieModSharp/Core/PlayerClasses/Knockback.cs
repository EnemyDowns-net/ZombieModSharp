using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using ZombieModSharp.Interface.Player;
using ZombieModSharp.Interface.PlayerClasses;

namespace ZombieModSharp.Core.PlayerClasses;

public class Knockback : IKnockback
{
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<Knockback> _logger;
    private readonly IPlayer _player;
    private readonly IEntityManager _entityManager;

    public Knockback(ISharedSystem sharedSystem, ILogger<Knockback> logger, IPlayer player)
    {
        _sharedSystem = sharedSystem;
        _logger = logger;
        _player = player;
        _entityManager = _sharedSystem.GetEntityManager();
    }

    public void KnockbackClient(IGameClient client, IGameClient attacker, string weapon, float damage, int hitGroup)
    {
        if (weapon.Contains("hegrenade"))
            return;

        if (client == null || attacker == null)
            return;

        // knockback is for zombie only.
        if (!_player.IsClientHuman(attacker) || !_player.IsClientInfected(client))
            return;

        var attackerPawn = _entityManager.FindPlayerControllerBySlot(attacker.Slot)?.GetPlayerPawn();

        if (attackerPawn == null)
        {
            _logger?.LogError("attacker pawn is null!");
            return;
        }

        var attackerEye = attackerPawn.GetEyeAngles();
        var foward = attackerEye.AnglesToVectorForward();

        var classKnockback = 4.0f;
        var weaponknockback = 1.0f;
        var hitgroupsKnockback = 1.0f;

        var pushVelocity = foward * damage * classKnockback * weaponknockback * hitgroupsKnockback;

        var playerPawn = _entityManager.FindPlayerControllerBySlot(client.Slot)?.GetPlayerPawn();

        if (playerPawn == null)
            return;

        var veloCity = playerPawn.GetAbsVelocity();
        playerPawn.Teleport(null, null, veloCity + pushVelocity);
    }
}
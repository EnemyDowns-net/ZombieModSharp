using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using ZombieModSharp.Interface.Configs;
using ZombieModSharp.Interface.Player;
using ZombieModSharp.Interface.PlayerClasses;

namespace ZombieModSharp.Core.PlayerClasses;

public class Knockback : IKnockback
{
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<Knockback> _logger;
    private readonly IPlayer _player;
    private readonly IWeapons _weapons;

    public Knockback(ISharedSystem sharedSystem, ILogger<Knockback> logger, IPlayer player, IWeapons weapons)
    {
        _sharedSystem = sharedSystem;
        _logger = logger;
        _player = player;
        _weapons = weapons;
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

        var attackerPawn = attacker.GetPlayerController()?.GetPlayerPawn();

        if (attackerPawn == null)
        {
            _logger?.LogError("attacker pawn is null!");
            return;
        }

        var attackerEye = attackerPawn.GetEyeAngles();
        var foward = attackerEye.AnglesToVectorForward();

        var classKnockback = 4.0f;
        var weaponknockback = _weapons.GetWeaponKnockback(weapon);
        var hitgroupsKnockback = 1.0f;

        var pushVelocity = foward * damage * classKnockback * weaponknockback * hitgroupsKnockback;

        var playerPawn = client.GetPlayerController()?.GetPlayerPawn();

        if (playerPawn == null)
            return;

        var veloCity = playerPawn.GetAbsVelocity();
        playerPawn.Teleport(null, null, veloCity + pushVelocity);
    }
}
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Objects;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.Modules;

public class Knockback : IKnockback
{
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<Knockback> _logger;
    private readonly IPlayerManager _playerManager;
    private readonly IModSharp _modsharp;
    private readonly IConfigManager _configManager;

    public Knockback(ISharedSystem sharedSystem, ILogger<Knockback> logger, IPlayerManager playerManager, IConfigManager configManager)
    {
        _sharedSystem = sharedSystem;
        _logger = logger;
        _playerManager = playerManager;
        _modsharp = _sharedSystem.GetModSharp();
        _configManager = configManager;
    }

    public void KnockbackClient(IGameClient client, IGameClient attacker, string weapon, float damage, int hitGroup)
    {
        if (weapon.Contains("hegrenade"))
            return;

        if (client == null || attacker == null)
            return;

        // knockback is for zombie only.
        if (!_playerManager.IsClientHuman(attacker) || !_playerManager.IsClientInfected(client))
            return;

        var attackerPawn = attacker.GetPlayerController()?.GetPlayerPawn();

        if (attackerPawn == null)
        {
            _logger?.LogError("attacker pawn is null!");
            return;
        }

        var attackerEye = attackerPawn.GetEyeAngles();
        var foward = attackerEye.AnglesToVectorForward();

        var classKnockback = _playerManager.GetOrCreatePlayer(client).ActiveClass?.Knockback ?? 3.0f;
        var weaponknockback = GetWeaponKnockback(weapon);
        var hitgroupsKnockback = GetHitgroupKnockback(hitGroup);

        // _modsharp.PrintToChatAll($"KB data: {weaponknockback:F2} | {hitgroupsKnockback:F2}");

        var pushVelocity = foward * damage * classKnockback * weaponknockback * hitgroupsKnockback;

        var playerPawn = client.GetPlayerController()?.GetPlayerPawn();

        if (playerPawn == null)
            return;

        var veloCity = playerPawn.GetAbsVelocity();
        playerPawn.Teleport(null, null, veloCity + pushVelocity);
    }

    
    public float GetHitgroupKnockback(int hitgroup)
    {
        if (hitgroup >= _configManager.HitgroupData.Length || hitgroup < 0)
            return 1.0f;

        return _configManager.HitgroupData[hitgroup];
    }

    public float GetWeaponKnockback(string weaponentity)
    {
        if (!_configManager.WeaponConfig.TryGetValue(weaponentity, out var weaponData))
        {
            _modsharp.PrintToChatAll($"No weapons name {weaponentity}");
            return 1.0f;
        }

        _modsharp.PrintToChatAll($"Found {weaponData.EntityName} and KB: {weaponData.Knockback}");
        return weaponData.Knockback;
    }
}
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Sharp.Shared.Types;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.HookManager;

public class Hooks : IHooks
{
    private readonly ISharedSystem _sharedSystem;
    private readonly IPlayerManager _player;
    private readonly IHookManager _hookManager;
    private readonly IModSharp _modsharp;
    private readonly IEntityManager _entityManager;

    public Hooks(ISharedSystem sharedSystem, IPlayerManager player)
    {
        _sharedSystem = sharedSystem;
        _player = player;
        _hookManager = _sharedSystem.GetHookManager();
        _modsharp = _sharedSystem.GetModSharp();
        _entityManager = _sharedSystem.GetEntityManager();
    }

    public void PostInit()
    {
        _hookManager.PlayerWeaponCanEquip.InstallHookPre(OnCanEquip);
        // _hookManager.PlayerDispatchTraceAttack.InstallHookPost(OnTakeDamaged);
    }

    private HookReturnValue<bool> OnCanEquip(IPlayerWeaponCanEquipHookParams param, HookReturnValue<bool> result)
    {
        var player = param.Client;
        var weapon = param.Weapon;

        //var isInfected = _player.IsClientInfected(player);
        //_modsharp.PrintChannelAll(HudPrintChannel.Chat, $"Zombie Status: {isInfected}");
        // just in case.
        _player.GetOrCreatePlayer(player);

        // if player is infect and weapon is knife then ignore all of it.
        if (_player.IsClientInfected(player) && !weapon.IsKnife)
        {
            //_modsharp.PrintChannelAll(HudPrintChannel.Chat, $"This is {EHookAction.SkipCallReturnOverride} and {result.ReturnValue}");
            return new HookReturnValue<bool>(EHookAction.SkipCallReturnOverride, false);
        }

        //_modsharp.PrintChannelAll(HudPrintChannel.Chat, $"This is {result.Action} and {result.ReturnValue}");
        return result;
    }

    private void OnTakeDamaged(IPlayerDispatchTraceAttackHookParams param, HookReturnValue<long> result)
    {
        var attacker = _entityManager.FindEntityByHandle(param.AttackerHandle)?.As<IPlayerController>();

        if (attacker == null)
        {
            _modsharp.PrintToChatAll("Attacker is null!");
            return;
        }

        var client = param.Controller;

        if (client == null)
        {
            _modsharp.PrintToChatAll("Client is null!");
            return;
        }

        var hitGroup = param.HitGroup;
        var damage = param.Damage;
        var weaponEnt = _entityManager.FindEntityByHandle(param.InflictorHandle);

        _modsharp.PrintToChatAll($"Classname: {weaponEnt?.Classname}");

        if (weaponEnt?.Classname.Contains("weapon", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            _modsharp.PrintToChatAll("It's Weapon!");
        }
    }
}
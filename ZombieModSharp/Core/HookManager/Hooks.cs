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
    private readonly IPlayerManager _playerManager;
    private readonly IHookManager _hookManager;
    private readonly IModSharp _modsharp;
    private readonly IEntityManager _entityManager;

    public Hooks(ISharedSystem sharedSystem, IPlayerManager playerManager)
    {
        _sharedSystem = sharedSystem;
        _playerManager = playerManager;
        _hookManager = _sharedSystem.GetHookManager();
        _modsharp = _sharedSystem.GetModSharp();
        _entityManager = _sharedSystem.GetEntityManager();
    }

    public void Init()
    {
        _hookManager.PlayerWeaponCanEquip.InstallHookPre(OnCanEquip);
        _hookManager.PlayerGetMaxSpeed.InstallHookPre(OnGetMaxSpeed);
        // _hookManager.PlayerDispatchTraceAttack.InstallHookPost(OnTakeDamaged);
    }

    public void Shutdown()
    {
        _hookManager.PlayerWeaponCanEquip.RemoveHookPre(OnCanEquip);
        _hookManager.PlayerGetMaxSpeed.RemoveHookPre(OnGetMaxSpeed);
    }

    private HookReturnValue<float> OnGetMaxSpeed(IPlayerGetMaxSpeedHookParams param, HookReturnValue<float> result)
    {
        var client = param.Controller.GetGameClient();

        if(client == null)
            return result;

        var player = _playerManager.GetOrCreatePlayer(client);

        if(player.ActiveClass != null)
        {
            return new HookReturnValue<float>(EHookAction.SkipCallReturnOverride, player.ActiveClass.Speed);
        }

        return result;
    }

    private HookReturnValue<bool> OnCanEquip(IPlayerWeaponCanEquipHookParams param, HookReturnValue<bool> result)
    {
        var player = param.Client;
        var weapon = param.Weapon;

        //var isInfected = _player.IsClientInfected(player);
        //_modsharp.PrintChannelAll(HudPrintChannel.Chat, $"Zombie Status: {isInfected}");
        // just in case.
        var client = _playerManager.GetOrCreatePlayer(player);

        // if player is infect and weapon is knife then ignore all of it.
        if (client.IsInfected() && !weapon.IsKnife)
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
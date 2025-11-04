using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.HookParams;
using Sharp.Shared.Managers;
using Sharp.Shared.Types;
using ZombieModSharp.Interface.Hooks;
using ZombieModSharp.Interface.Player;

namespace ZombieModSharp.Core;

public class Hooks : IHooks
{
    private readonly ISharedSystem _sharedSystem;
    private readonly IPlayer _player;
    private readonly IHookManager _hookManager;

    public Hooks(ISharedSystem sharedSystem, IPlayer player)
    {
        _sharedSystem = sharedSystem;
        _player = player;
        _hookManager = _sharedSystem.GetHookManager();
    }

    public void PostInit()
    {
        _hookManager.PlayerWeaponCanUse.InstallHookPre(OnCanUse);
    }

    private HookReturnValue<bool> OnCanUse(IPlayerWeaponCanUseHookParams param, HookReturnValue<bool> result)
    {
        var player = param.Client;
        var weapon = param.Weapon;
        
        // if player is infect and weapon is knife then ignore all of it.
        if(_player.IsClientInfected(player) && !weapon.Name.Contains("knife", StringComparison.OrdinalIgnoreCase))
        {
            return new HookReturnValue<bool>(EHookAction.SkipCallReturnOverride);
        }

        return result;
    }
}
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
    private readonly IModSharp _modsharp;

    public Hooks(ISharedSystem sharedSystem, IPlayer player)
    {
        _sharedSystem = sharedSystem;
        _player = player;
        _hookManager = _sharedSystem.GetHookManager();
        _modsharp = _sharedSystem.GetModSharp();
    }

    public void PostInit()
    {
        _hookManager.PlayerWeaponCanEquip.InstallHookPre(OnCanEquip);
    }

    private HookReturnValue<bool> OnCanEquip(IPlayerWeaponCanEquipHookParams param, HookReturnValue<bool> result)
    {
        var player = param.Client;
        var weapon = param.Weapon;

        //var isInfected = _player.IsClientInfected(player);
        //_modsharp.PrintChannelAll(HudPrintChannel.Chat, $"Zombie Status: {isInfected}");

        // if player is infect and weapon is knife then ignore all of it.
        if (_player.IsClientInfected(player) && !weapon.IsKnife)
        {
            //_modsharp.PrintChannelAll(HudPrintChannel.Chat, $"This is {EHookAction.SkipCallReturnOverride} and {result.ReturnValue}");
            return new HookReturnValue<bool>(EHookAction.SkipCallReturnOverride, false);
        }

        //_modsharp.PrintChannelAll(HudPrintChannel.Chat, $"This is {result.Action} and {result.ReturnValue}");
        return result;
    }
}
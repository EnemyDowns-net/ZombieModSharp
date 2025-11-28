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
    private readonly IInfect _infect;
    private readonly IGrenadeEffect _grenadeEffect;

    public Hooks(ISharedSystem sharedSystem, IPlayerManager playerManager, IInfect infect, IGrenadeEffect grenadeEffect)
    {
        _sharedSystem = sharedSystem;
        _playerManager = playerManager;
        _hookManager = _sharedSystem.GetHookManager();
        _modsharp = _sharedSystem.GetModSharp();
        _entityManager = _sharedSystem.GetEntityManager();
        _infect = infect;
        _grenadeEffect = grenadeEffect;
    }

    public void Init()
    {
        _hookManager.PlayerWeaponCanEquip.InstallHookPre(OnCanEquip);
        _hookManager.PlayerGetMaxSpeed.InstallHookPre(OnGetMaxSpeed);
        _hookManager.PlayerDispatchTraceAttack.InstallHookPre(OnTakeDamage);
        _hookManager.GiveNamedItem.InstallHookPost(OnGiveNamedItemPost);
    }

    public void Shutdown()
    {
        _hookManager.PlayerWeaponCanEquip.RemoveHookPre(OnCanEquip);
        _hookManager.PlayerGetMaxSpeed.RemoveHookPre(OnGetMaxSpeed);
        _hookManager.PlayerDispatchTraceAttack.RemoveHookPre(OnTakeDamage);
        _hookManager.GiveNamedItem.RemoveHookPost(OnGiveNamedItemPost);
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

    private HookReturnValue<long> OnTakeDamage(IPlayerDispatchTraceAttackHookParams param, HookReturnValue<long> result)
    {
        var attacker = _entityManager.FindEntityByHandle(param.AttackerPawnHandle)?.GetOriginalController()?.GetGameClient();

        var client = param.Controller.GetGameClient();
        if (attacker == null || client == null)
        {
            return result;
        }
        var attackerPlayer = _playerManager.GetOrCreatePlayer(attacker);
        var victimPlayer = _playerManager.GetOrCreatePlayer(client);

        // prevent infected stab to death for humans.
        if(attackerPlayer.IsInfected() && victimPlayer.IsHuman())
        {
            param.Damage = 1;
            return result;
        }

        if(attackerPlayer.IsHuman() && victimPlayer.IsInfected())
        {
            var inflictor = _entityManager.FindEntityByHandle(param.InflictorHandle);
            if(inflictor?.Classname.Contains("hegrenade") ?? false)
            {
                var duration = victimPlayer.ActiveClass?.NapalmDuration ?? 0.0f;

                if(duration > 0.0f)
                {
                    _grenadeEffect.IgnitePawn(param.Pawn, (int)param.Damage, duration);
                }
            }
        }

        return result;
    }

    private void OnGiveNamedItemPost(IGiveNamedItemHookParams param, HookReturnValue<IBaseWeapon> result)
    {
        /*
        if(result.ReturnValue != null || (result.ReturnValue?.IsValid() ?? false))
        {
            _modsharp.PushTimer(() => {
                result.ReturnValue.GetWeaponData().PrimaryReserveAmmoMax = 1200;
                result.ReturnValue.ReserveAmmo = 1200;
            }, 1.0f);
        }
        */
    }
}
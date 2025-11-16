using Sharp.Shared;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Types;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.Modules;

public class SoundServices : ISoundServices
{
    private readonly ISharedSystem _sharedSystem;
    private readonly IModSharp _modsharp;
    private readonly IPlayerManager _playerManager;
    
    public SoundServices(ISharedSystem sharedSystem, IModSharp modSharp, IPlayerManager playerManager)
    {
        _sharedSystem = sharedSystem;
        _modsharp = _sharedSystem.GetModSharp();
        _sharedSystem.GetSoundManager();
        _playerManager = playerManager;
    }

    public void ZombieMoan(IPlayerPawn player)
    {
        
    }

    public void EmitZombieSound(IBaseEntity source, string sound)
    {
        var allPlayer = _playerManager.GetAllPlayers().Where(p => p.Value.SoundEnabled && p.Value.SoundVolume > 0);

        foreach(var player in allPlayer)
        {
            var volume = player.Value.SoundVolume / 100;
            source.EmitSound(sound, volume, new RecipientFilter(player.Key));
        }
    }
}
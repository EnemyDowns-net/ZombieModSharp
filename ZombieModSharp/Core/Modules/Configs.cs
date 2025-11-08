using Microsoft.Extensions.Logging;
using Sharp.Shared;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.Modules;

public class Configs : IConfigs
{
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<Configs> _logger;
    private readonly IWeapons _weapons;

    public Configs(ISharedSystem sharedSystem, ILogger<Configs> logger, IWeapons weapons)
    {
        _sharedSystem = sharedSystem;
        _logger = logger;
        _weapons = weapons;
    }

    public void PostInit()
    {
        var gamePath = _sharedSystem.GetModSharp().GetGamePath();
        var configPath = Path.Combine(gamePath, "../sharp", "configs", "zombiemodsharp");

        if (!Directory.Exists(configPath))
        {
            _logger.LogWarning("Path {config} is not existed, create new one.", configPath);
            Directory.CreateDirectory(configPath);
        }

        _weapons.LoadConfig(configPath);
    }
}
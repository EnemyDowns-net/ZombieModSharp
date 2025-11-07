using System.Text.Json;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using ZombieModSharp.Entities;
using ZombieModSharp.Interface.Weapons;

namespace ZombieModSharp.Core.Weapons;

public class Weapons : IWeapons
{
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<Weapons> _logger;

    private Dictionary<string, WeaponData> weaponDatas = [];

    public Weapons(ISharedSystem sharedSystem, ILogger<Weapons> logger)
    {
        _sharedSystem = sharedSystem;
        _logger = logger;
    }

    public void LoadConfig(string path)
    {
        var configPath = Path.Combine(path, "weapons.jsonc");
        var weaponDatas = JsonSerializer.Deserialize<Dictionary<string, WeaponData>>(File.ReadAllText(configPath));


        if (weaponDatas == null)
        {
            _logger.LogCritical("The weapon datas is null!");
            return;
        }
        
        foreach (var kvp in weaponDatas)
        {
            _logger.LogInformation("Key: {Key}, Data: {Data}", kvp.Key, kvp.Value);
        }
    }
}
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using ZombieModSharp.Entities;
using ZombieModSharp.Interface.Configs;

namespace ZombieModSharp.Core.Weapons;

public class Weapons : IWeapons
{
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<Weapons> _logger;
    private readonly IModSharp _modsharp;

    private Dictionary<string, WeaponData> weaponDatas = [];

    public Weapons(ISharedSystem sharedSystem, ILogger<Weapons> logger)
    {
        _sharedSystem = sharedSystem;
        _logger = _sharedSystem.GetLoggerFactory().CreateLogger<Weapons>();
        _modsharp = _sharedSystem.GetModSharp();
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
    }

    public float GetWeaponKnockback(string weaponentity)
    {
        if (!weaponDatas.TryGetValue(weaponentity, out var weaponData))
        {
            _modsharp.PrintToChatAll($"No weapons name {weaponentity}");
            return 1.0f;
        }

        _modsharp.PrintToChatAll($"Found {weaponData.EntityName} and KB: {weaponData.Knockback}");
        return weaponData.Knockback;
    }
}
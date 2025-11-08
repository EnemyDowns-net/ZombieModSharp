using System.Text.Json;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.Modules;

public class WeaponData
{
    public required string EntityName { get; set; }
    public float Knockback { get; set; } = 1.0f;
}

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

        if (!File.Exists(configPath))
        {
            _logger.LogCritical("File is not found!");
            return;
        }

        try
        {
            var jsonContent = File.ReadAllText(configPath);
            
            // Simple comment removal (basic implementation)
            var lines = jsonContent.Split('\n');
            var cleanedLines = lines.Select(line => 
            {
                var commentIndex = line.IndexOf("//");
                return commentIndex >= 0 ? line.Substring(0, commentIndex) : line;
            });
            var cleanedJson = string.Join('\n', cleanedLines);

            weaponDatas = JsonSerializer.Deserialize<Dictionary<string, WeaponData>>(cleanedJson) ?? [];

            _logger.LogInformation("Successfully loaded {count} weapon configurations", weaponDatas.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse weapons configuration");
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
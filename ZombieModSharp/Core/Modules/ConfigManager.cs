using System.Text.Json;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using ZombieModSharp.Abstractions;
using ZombieModSharp.Abstractions.Entities;

namespace ZombieModSharp.Core.Modules;

public class ConfigManager : IConfigManager
{
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<ConfigManager> _logger;
    private readonly IModSharp _modsharp;
    private readonly IPlayerClasses _playerClasses;

    private static string configPath = string.Empty;

    // Precaching
    private List<string> _precacheList = [];

    // Config Data
    public Dictionary<string, WeaponData> WeaponConfig { get; set; } = [];
    public float[] HitgroupData { get; set; } = { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };

    public ConfigManager(ISharedSystem sharedSystem, ILogger<ConfigManager> logger, IPlayerClasses playerClasses)
    {
        _sharedSystem = sharedSystem;
        _logger = logger;
        _modsharp = _sharedSystem.GetModSharp();
        _playerClasses = playerClasses;
    }

    public void Init()
    {
        _logger.LogInformation("Configs.PostInit() called");

        var gamePath = _sharedSystem.GetModSharp().GetGamePath();
        configPath = Path.Combine(gamePath, "../sharp", "configs", "zombiemodsharp");

        if (!Directory.Exists(configPath))
        {
            _logger.LogWarning("Path {config} is not existed, create new one.", configPath);
            Directory.CreateDirectory(configPath);
        }

        _logger.LogInformation("Loading weapons config...");
        PreacheLoadConfig(configPath);
        _playerClasses.LoadConfig(configPath);
        WeaponLoadConfig(configPath);
        HigroupLoadConfig(configPath);
    }

    private void PreacheLoadConfig(string path)
    {
        var configPath = Path.Combine(path, "resources.txt");

        if (!File.Exists(configPath))
        {
            _logger.LogCritical("File is not found!");
            return;
        }

        // read all line in .txt file and ignore the line that start with // or empty space.
        _modsharp.InvokeFrameActionAsync(async () =>
        {
            var lines = await File.ReadAllLinesAsync(configPath);

            _precacheList = lines
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("//"))
                .Select(line => line.Trim())
                .ToList();

            _logger.LogInformation("Loaded {Count} resources for precaching", _precacheList.Count);
        });
    }

    private void WeaponLoadConfig(string path)
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

            WeaponConfig = JsonSerializer.Deserialize<Dictionary<string, WeaponData>>(cleanedJson) ?? [];

            _logger.LogInformation("Successfully loaded {count} weapon configurations", WeaponConfig.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse weapons configuration");
        }
    }

    private void HigroupLoadConfig(string path)
    {
        var configPath = Path.Combine(path, "hitgroups.jsonc");

        Dictionary<string, float>? hitgroupConfig = [];

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

            hitgroupConfig = JsonSerializer.Deserialize<Dictionary<string, float>>(cleanedJson) ?? [];

            _logger.LogInformation("Successfully loaded {count} weapon configurations", hitgroupConfig.Count);
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Error: {ex}", ex.Message);
            return;
        }

        if (hitgroupConfig == null)
        {
            _logger.LogCritical("The hitgroups datas is null!");
            return;
        }
    }
    
    public void PrecacheAllResource()
    {
        if (_precacheList.Count <= 0)
        {
            _logger.LogWarning("No resource found for precaching");
            return;
        }
        
        foreach(var resource in _precacheList)
        {
            _modsharp.PrecacheResource(resource);
        }
    }
}
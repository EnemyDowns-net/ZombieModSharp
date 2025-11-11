using System.Text.Json;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.GameEntities;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.Modules;

public class ClassAttribute
{
    public string Name { get; set; } = string.Empty;
    public int Team { get; set; } = 0;
    public bool MotherZombie { get; set; } = false;
    public string Model { get; set; } = "default";
    public int Health { get; set; } = 100;
    public float Speed { get; set; } = 300f;
}

public class PlayerClasses : IPlayerClasses
{
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<PlayerClasses> _logger;
    private readonly IPlayerManager _playerManager;

    public PlayerClasses(ISharedSystem sharedSystem, IPlayerManager playerManager)
    {
        _sharedSystem = sharedSystem;
        _logger = _sharedSystem.GetLoggerFactory().CreateLogger<PlayerClasses>();
        _playerManager = playerManager;
    }

    public Dictionary<string, ClassAttribute> classesData = [];

    public void LoadConfig(string path)
    {
        var configPath = Path.Combine(path, "playerclasses.jsonc");

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

            classesData = JsonSerializer.Deserialize<Dictionary<string, ClassAttribute>>(cleanedJson) ?? [];

            _logger.LogInformation("Successfully loaded {count} classes configurations", classesData.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse classes configuration");
        }
    }

    public void ApplyPlayerClassAttribute(IPlayerPawn playerPawn, ClassAttribute classAttribute)
    {
        if (!playerPawn.IsAlive)
        {
            return;
        }

        playerPawn.Health = classAttribute.Health;
        var team = classAttribute.Team;

        if (classAttribute.Model != "default" || !string.IsNullOrEmpty(classAttribute.Model))
            playerPawn.SetModel(classAttribute.Model);

        else
        {
            if (team == 0)
                playerPawn.SetModel("characters/models/tm_phoenix/tm_phoenix.vmdl");

            if (team == 1)
                playerPawn.SetModel("characters/models/ctm_sas/ctm_sas.vmdl");
        }

        SetClassArmor(playerPawn, team);

        var gameClient = playerPawn.GetController()?.GetGameClient();

        if (gameClient == null)
        {
            _logger.LogError("IGameClient is null!");
            return;
        }

        var client = _playerManager.GetOrCreatePlayer(gameClient);
        client.ActiveClass = classAttribute;
    }

    private void SetClassArmor(IPlayerPawn playerPawn, int team)
    {
        if (team == 0)
        {
            playerPawn.ArmorValue = 0;
            try
            {
                playerPawn.GetItemService()!.HasHelmet = false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error: {ex}", ex);
            }
        }
        else
        {
            playerPawn.ArmorValue = 100;
        }
    }
    
    public ClassAttribute? GetClassByName(string classname)
    {
        if (string.IsNullOrEmpty(classname))
            return null;

        if (!classesData.TryGetValue(classname, out var targetClass))
            return null;

        return targetClass;
    }
}
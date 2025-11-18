using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using ZombieModSharp.Abstractions;
using ZombieModSharp.Core.Modules;

namespace ZombieModSharp.Core.HookManager;

public class CvarServices : ICvarServices
{
    private readonly ISharedSystem _sharedSystem;
    private readonly IKnockback _knockback;
    private readonly IModSharp _modsharp;
    private readonly ILogger<CvarServices> _logger;

    // Declare here I guess
    public Dictionary<string, IConVar?> CvarList { get; set; } = [];

    public CvarServices(ISharedSystem sharedSystem, IKnockback knockback)
    {
        _sharedSystem = sharedSystem;
        _logger = _sharedSystem.GetLoggerFactory().CreateLogger<CvarServices>();
        _modsharp = _sharedSystem.GetModSharp();
        _knockback = knockback;
    }
    
    public void Init()
    {
        // we create convar 
        var conVar = _sharedSystem.GetConVarManager();
        CvarList["Cvar_HumanDefault"] = conVar.CreateConVar("zms_human_class_default", "human_default", "Default human class when player join", ConVarFlags.Release);
        CvarList["Cvar_ZombieDefault"] = conVar.CreateConVar("zms_zombie_class_default", "zombie_default", "Default zombie class when player join", ConVarFlags.Release);

        CvarList["Cvar_InfectCountdown"] = conVar.CreateConVar("zms_infect_countdown", 10.0f, 5.0f, 60.0f, "Infection Countdown", ConVarFlags.Release);
        CvarList["Cvar_InfectMotherZombieRatio"] = conVar.CreateConVar("zms_infect_motherzombie_ratio", 7.0f, 1.0f, 63.0f, "Motherzombie ratio for fist infection", ConVarFlags.Release);
        CvarList["Cvar_InfectMinimumZombie"] = conVar.CreateConVar("zms_infect_minimum_zombie", 1, 1, 63, "Minimum zombie to spawn on first infection.", ConVarFlags.Release); 
        CvarList["Cvar_InfectNoblockEnable"] =  conVar.CreateConVar("zms_infect_noblock_enable", true, "Enable noblock between player or not.", ConVarFlags.Release);
        CvarList["Cvar_InfectMotherZombieSpawn"] = conVar.CreateConVar("zms_infect_motherzombie_spawn", true, "Teleport motherzombie back to spawn.", ConVarFlags.Release);
        CvarList["Cvar_InfectKnockbackScale"] = conVar.CreateConVar("zms_infect_knockback_scale", 1.0f, 0.01f, 100.0f, "Knockback scale for modifying", ConVarFlags.Release);
        CvarList["Cvar_InfectWarmupEnabled"] = conVar.CreateConVar("zms_infect_warmup_enabled", false, "Enable infection game during warmup or not", ConVarFlags.Release);

        CvarList["Cvar_RespawnEnabled"] = conVar.CreateConVar("zms_respawn_enabled", true, "Enable respawn during the round.", ConVarFlags.Release);
        CvarList["Cvar_RespawnDelay"] = conVar.CreateConVar("zms_respawn_delay", 5.0f, "Respawn Delay timer after death.", ConVarFlags.Release);
        CvarList["Cvar_RespawnLateJoin"] = conVar.CreateConVar("zms_respawn_late_join", true, "Allow player to join during the round.", ConVarFlags.Release);
        CvarList["Cvar_RespawnTeam"] = conVar.CreateConVar("zms_respawn_team", 0, "Respawn Team [0 = Zombie|1 = Human]", ConVarFlags.Release);
        
        CvarList["Cvar_ZTeleAllow"] = conVar.CreateConVar("zms_ztele_allow", true, "Allow Ztele command or not", ConVarFlags.Release);
        CvarList["Cvar_ZTeleDelay"] = conVar.CreateConVar("zms_ztele_delay", 5.0f, "Delay timer before player can get teleported with ztele command", ConVarFlags.Release);
        // we check if covar existed or not.

        conVar.InstallChangeHook(CvarList["Cvar_InfectKnockbackScale"]!, (convar) =>
        {
            if(convar.Name == "zms_infect_knockback_scale")
            {
                var scale = convar.GetFloat();
                _knockback.SetKnockbackScale(scale);
                _modsharp.PrintToChatAll($"ConVar: zms_infect_knockback_scale set to {scale}");
                _logger.LogInformation("Scale is set to {scale}", scale);
            }
        });

        AutoExecConfigFile();
    }

    // we create convar file here.
    private void AutoExecConfigFile()
    {
        // search for path first.
        var gamePath = _sharedSystem.GetModSharp().GetGamePath();
        var configPath = Path.Combine(gamePath, "cfg", "zombiemodsharp");

        if (!Directory.Exists(configPath))
        {
            _logger.LogWarning("Path {config} is not existed, create new one.", configPath);
            Directory.CreateDirectory(configPath);
        }

        var configFile = Path.Combine(configPath, "zombiemodsharp.cfg");

        if(!File.Exists(configFile))
        {
            // create new one
            _modsharp.InvokeFrameActionAsync(async () => { 
                await CreateNewConfigFileAsync(configFile);
                _modsharp.ServerCommand($"exec zombiemodsharp/zombiemodsharp.cfg");
            });
        }

        else
            _modsharp.ServerCommand($"exec zombiemodsharp/zombiemodsharp.cfg");
    }

    private async Task CreateNewConfigFileAsync(string path)
    {
        if (File.Exists(path))
        {
            _logger.LogWarning("Config file is already existed!");
            return;
        }

        using (var configFile = new StreamWriter(path, false, System.Text.Encoding.UTF8))
        {
            await configFile.WriteLineAsync($"// This file is generated by ZombieModSharp.dll at {DateTime.Today}");
            await configFile.WriteLineAsync();

            foreach (var convar in CvarList)
            {
                if (convar.Value == null)
                    continue;

                try
                {
                    _logger.LogInformation("Writing convar: {Name}", convar.Value.Name);
                    await CreateConVarLineAsync(configFile, convar.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing convar: {Name}", convar.Value.Name);
                    throw;
                }
            }
        }
    }

    private async Task CreateConVarLineAsync(StreamWriter configFile, IConVar conVar)
    {
        if (configFile == null)
        {
            _logger.LogCritical("[CreateConVarLine] Config File is null");
            return;
        }

        var command = conVar.Name;
        var defaultValue = conVar.GetString();
        var description = conVar.HelpString;

        await configFile.WriteLineAsync($"// {description}");
        await configFile.WriteLineAsync($"// -");
        await configFile.WriteLineAsync($"// Default: {defaultValue}");
        await configFile.WriteLineAsync($"{command} {defaultValue}");
        await configFile.WriteLineAsync();
    }
}
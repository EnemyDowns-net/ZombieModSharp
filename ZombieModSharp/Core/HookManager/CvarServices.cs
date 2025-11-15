using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.HookManager;

public class CvarServices : ICvarServices
{
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<CvarServices> _logger;

    // Declare here I guess
    public Dictionary<string, IConVar?> CvarList { get; set; } = [];

    public CvarServices(ISharedSystem sharedSystem)
    {
        _sharedSystem = sharedSystem;
        _logger = _sharedSystem.GetLoggerFactory().CreateLogger<CvarServices>();
    }
    
    public void Init()
    {
        // we create convar 
        var conVar = _sharedSystem.GetConVarManager();
        CvarList["Cvar_HumanDefault"] = conVar.CreateConVar("zms_human_class_default", "human_default", "Default human class when player join", ConVarFlags.Release);
        CvarList["Cvar_ZombieDefault"] = conVar.CreateConVar("zms_zombie_class_default", "zombie_default", "Default zombie class when player join", ConVarFlags.Release);

        CvarList["Cvar_InfectCountdown"] = conVar.CreateConVar("zms_infect_countdown", 10.0f, "Infection Countdown", ConVarFlags.Release);
        CvarList["Cvar_InfectMotherZombieRatio"] = conVar.CreateConVar("zms_infect_motherzombie_ratio", 7.0f, "Motherzombie ratio for fist infection", ConVarFlags.Release);
        CvarList["Cvar_InfectMinimumZombie"] = conVar.CreateConVar("zms_infect_minimum_zombie", 1, "Minimum zombie to spawn on first infection.", ConVarFlags.Release); 
        CvarList["Cvar_InfectNoblockEnable"] =  conVar.CreateConVar("zms_infect_noblock_enable", true, "Enable noblock between player or not.", ConVarFlags.Release);
        CvarList["Cvar_InfectMotherZombieSpawn"] = conVar.CreateConVar("zms_infect_motherzombie_spawn", true, "Teleport motherzombie back to spawn.", ConVarFlags.Release);
        
        CvarList["Cvar_ZTeleAllow"] = conVar.CreateConVar("zms_ztele_allow", true, "Allow Ztele command or not", ConVarFlags.Release);
        CvarList["Cvar_ZTeleDelay"] = conVar.CreateConVar("zms_ztele_delay", 5.0f, "Delay timer before player can get teleported with ztele command", ConVarFlags.Release);
        // we check if covar existed or not.
        
    }
}
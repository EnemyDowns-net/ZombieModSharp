using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using ZombieModSharp.Abstractions;

namespace ZombieModSharp.Core.HookManager;

public class CvarManager : ICvarManager
{
    private readonly ISharedSystem _sharedSystem;
    private readonly ILogger<CvarManager> _logger;

    // Declare here I guess
    public Dictionary<string, IConVar?> CvarList { get; set; } = [];

    public CvarManager(ISharedSystem sharedSystem)
    {
        _sharedSystem = sharedSystem;
        _logger = _sharedSystem.GetLoggerFactory().CreateLogger<CvarManager>();
    }
    
    public void Init()
    {
        // we create convar 
        var conVar = _sharedSystem.GetConVarManager();
        CvarList["Cvar_HumanDefault"] = conVar.CreateConVar("zms_human_class_default", "human_default", "Default human class when player join", ConVarFlags.Release);
        CvarList["Cvar_ZombieDefault"] = conVar.CreateConVar("zms_zombie_class_default", "zombie_default", "Default zombie class when player join", ConVarFlags.Release);

        // we check if covar existed or not.
        
    }
}
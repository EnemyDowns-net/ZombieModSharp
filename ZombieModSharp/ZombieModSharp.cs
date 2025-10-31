using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Listeners;
using TnmsPluginFoundation;
using ZombieModSharp.Core;
using ZombieModSharp.Core.Infection;
using ZombieModSharp.Core.Player;
using ZombieModSharp.Interface.Command;
using ZombieModSharp.Interface.Events;
using ZombieModSharp.Interface.Infection;
using ZombieModSharp.Interface.Listeners;
using ZombieModSharp.Interface.Player;
using ZombieModSharp.Interface.ZTele;

namespace ZombieModSharp;

public sealed class ZombieModSharp(ISharedSystem sharedSystem, string dllPath, string sharpPath, Version? version, IConfiguration coreConfiguration, bool hotReload) : TnmsPlugin(sharedSystem, dllPath, sharpPath, version, coreConfiguration, hotReload)
{
    // outside modules
    public override string DisplayName => "ZombieModSharp";
    public override string DisplayAuthor => "Oylsister";
    public override string BaseCfgDirectoryPath => "unused";
    public override string ConVarConfigPath => "cfg/ZombieModSharp/ZombieModSharp.cfg";
    public override string PluginPrefix => "ZMS";
    public override bool UseTranslationKeyInPluginPrefix => false;

    protected override void TnmsAllPluginsLoaded(bool hotReload)
    {
        RegisterModule<ZombieModSharpCoreModule>();
    }
}
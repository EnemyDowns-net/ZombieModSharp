using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TnmsPluginFoundation.Models.Plugin;
using ZombieModSharp.Core;
using ZombieModSharp.Core.Infection;
using ZombieModSharp.Core.Player;
using ZombieModSharp.Interface.Events;
using ZombieModSharp.Interface.Infection;
using ZombieModSharp.Interface.Player;
using ZombieModSharp.Interface.ZTele;

public class ZombieModSharpCoreModule : PluginModuleBase
{
    public override string PluginModuleName => "ZombieModSharpCoreModule";
    public override string ModuleChatPrefix => "ZMS";
    protected override bool UseTranslationKeyInModuleChatPrefix => false;

    private readonly IServiceProvider _serviceProvider;
    private readonly IPlayer _player;
    private readonly Listeners _clientListener;
    private readonly Events _events;
    private readonly IInfect _infect;
    private readonly IZTele _ztele;

    public ZombieModSharpCoreModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _player = new Player();
        _infect = new Infect(_serviceProvider, _serviceProvider.GetRequiredService<ILogger<Infect>>(), _player);
        _clientListener = new Listeners(_serviceProvider, _player, _serviceProvider.GetRequiredService<ILogger<Listeners>>());
        _ztele = new ZTele(_serviceProvider, _player, _serviceProvider.GetRequiredService<ILogger<ZTele>>());
        _events = new Events(_serviceProvider, _serviceProvider.GetRequiredService<ILogger<Events>>(), _player, _infect,  _ztele);
    }

    protected override void OnInitialize()
    {
        SharedSystem.GetClientManager().InstallClientListener(_clientListener);
        SharedSystem.GetEventManager().InstallEventListener(_events);

        _events.RegisterEvents();
    }

    protected override void OnUnloadModule()
    {
        SharedSystem.GetClientManager().RemoveClientListener(_clientListener);
        SharedSystem.GetEventManager().RemoveEventListener(_events);
    }
}


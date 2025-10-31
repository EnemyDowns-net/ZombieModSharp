using Sharp.Shared.GameEntities;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEvents;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using ZombieModSharp.Interface.Player;
using ZombieModSharp.Interface.Infection;
using Sharp.Shared;
using ZombieModSharp.Enums;
using Microsoft.Extensions.DependencyInjection;
using TnmsPluginFoundation;
using TnmsPluginFoundation.Models.Plugin;

namespace ZombieModSharp.Core.Infection;

public class (IServiceProvider provider) : PluginModuleBase(provider),IInfect
{
    private readonly TnmsPlugin _plugin;
    private readonly IEntityManager _entityManager;
    private readonly IEventManager _eventManager;
    private readonly ILogger<Infect> _logger;
    private readonly IPlayer _player;
    private readonly IModSharp _modSharp;

    private bool InfectStarted = false;

    public Infect(IServiceProvider serviceProvider, ILogger<Infect> logger, IPlayer player)
    {
        _plugin = serviceProvider.GetRequiredService<TnmsPlugin>();
        _entityManager = _plugin.SharedSystem.GetEntityManager();
        _eventManager = _plugin.SharedSystem.GetEventManager();
        _logger = logger;
        _player = player;
        _modSharp = _plugin.SharedSystem.GetModSharp();
    }

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IInfect, Infect>();
    }

    public void InfectPlayer(IGameClient client, IGameClient? attacker = null, bool motherzombie = false, bool force = false)
    {
        if (client == null)
        {
            return;
        }

        if (!InfectStarted)
        {
            InfectStarted = true;
        }

        var clientController = _entityManager.FindPlayerControllerBySlot(client.Slot);
        clientController?.SwitchTeam(CStrikeTeam.TE);

        var zmPlayer = _player.GetPlayer(client);
        zmPlayer.IsZombie = true;

        if (attacker == null)
            return;

        // Fire fake event here.
        var fakeEvent = _eventManager.CreateEvent("player_death", true);

        if (fakeEvent == null)
        {
            return;
        }

        try
        {
            fakeEvent.SetPlayer("userid", client.Slot);
            fakeEvent.SetPlayer("attacker", attacker.Slot);
            fakeEvent.SetString("weapon", "weapon_knife");

            fakeEvent.FireToClients();
        }
        catch (Exception ex)
        {
            // Ignore
            _logger.LogCritical("Failed to fire fake player_death event: {Exception}", ex);
        }
        finally
        {
            fakeEvent.Dispose();
        }
    }

    public void HumanizeClient(IGameClient client, bool force = false)
    {
        if (client == null)
        {
            return;
        }

        var clientController = _entityManager.FindPlayerControllerBySlot(client.Slot);
        clientController?.SwitchTeam(CStrikeTeam.CT);

        var zmPlayer = _player.GetPlayer(client);
        zmPlayer.IsZombie = false;
    }

    public void OnRoundPreStart()
    {
        var allPlayers = _player.GetAllPlayers();

        foreach (var kvp in allPlayers)
        {
            var client = kvp.Key;
            var zmPlayer = kvp.Value;

            zmPlayer.IsZombie = false;

            var controller = _entityManager.FindPlayerControllerBySlot(client.Slot);
            if (controller == null)
            {
                continue;
            }

            controller.SwitchTeam(CStrikeTeam.CT);
        }
    }

    public void OnRoundStart()
    {
        _modSharp.PrintChannelAll(HudPrintChannel.Chat, " \x04[Z:MS]\x01 Current game mode is \x05Humans vs. Zombies\x01, the goal for zombies is to infect all humans by knifing them.");
    }

    public void OnRoundEnd()
    {
        InfectStarted = false;

        var allPlayers = _player.GetAllPlayers();

        foreach (var kvp in allPlayers)
        {
            var client = kvp.Key;
            var zmPlayer = kvp.Value;

            zmPlayer.IsZombie = false;
            if (zmPlayer.MotherZombieStatus == MotherZombieStatus.Chosen)
                zmPlayer.MotherZombieStatus = MotherZombieStatus.Last;
        }
    }

    public void CheckGameStatus()
    {
        // if CT count is 0, end the round.
        if (!InfectStarted)
        {
            return;
        }
        var allPlayers = _player.GetAllPlayers();

        int ctCount = 0;
        int tCount = 0;

        foreach (var kvp in allPlayers)
        {
            var client = kvp.Key;
            var zmPlayer = kvp.Value;

            var controller = _entityManager.FindPlayerControllerBySlot(client.Slot);
            if (controller == null)
            {
                continue;
            }

            if (controller.Team == CStrikeTeam.CT)
            {
                ctCount++;
            }
            else if (controller.Team == CStrikeTeam.TE)
            {
                tCount++;
            }
        }

        if (ctCount <= 0 && tCount > 0)
        {
            InfectStarted = false;
            _modSharp.GetGameRules().TerminateRound(5.0f, RoundEndReason.TerroristsWin);
        }

        else if (tCount <= 0 && ctCount > 0)
        {
            InfectStarted = false;
            _modSharp.GetGameRules().TerminateRound(5.0f, RoundEndReason.CTsWin);
        }
    }
    
    public bool IsInfectStarted()
    {
        return InfectStarted;
    }
}
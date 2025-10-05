using Sharp.Shared.GameEntities;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEvents;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using ZombieModSharp.Interface.Player;
using ZombieModSharp.Interface.Infection;

namespace ZombieModSharp.Core.Infection;

public class Infect : IInfect
{
    private readonly IEntityManager _entityManager;
    private readonly IEventManager _eventManager;
    private readonly ILogger<Infect> _logger;
    private readonly IPlayer _player;
    private readonly IGameRules _gameRules;

    private bool InfectStarted = false;

    public Infect(IEntityManager entityManager, IEventManager eventManager, ILogger<Infect> logger, IPlayer player, IGameRules gameRules)
    {
        _entityManager = entityManager;
        _eventManager = eventManager;
        _logger = logger;
        _player = player;
        _gameRules = gameRules;
    }

    public void InfectPlayer(IGameClient client, IGameClient? attacker = null, bool motherzombie = false, bool force = false)
    {
        if (client == null)
        {
            return;
        }

        var clientController = _entityManager.FindPlayerControllerBySlot(client.Slot);
        clientController?.SwitchTeam(CStrikeTeam.TE);

        var zmPlayer = _player.GetPlayer(client);
        zmPlayer.IsInfected = true;

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

    public void OnRoundEnd()
    {
        InfectStarted = false;

        var allPlayers = _player.GetAllPlayers();

        foreach (var kvp in allPlayers)
        {
            var client = kvp.Key;
            var zmPlayer = kvp.Value;

            zmPlayer.IsInfected = false;

            var controller = _entityManager.FindPlayerControllerBySlot(client.Slot);
            if (controller == null)
            {
                continue;
            }
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
            _gameRules.TerminateRound(5.0f, RoundEndReason.TerroristsWin);
        }

        else if (tCount <= 0 && ctCount > 0)
        {
            InfectStarted = false;
            _gameRules.TerminateRound(5.0f, RoundEndReason.CTsWin);
        }
    }
}
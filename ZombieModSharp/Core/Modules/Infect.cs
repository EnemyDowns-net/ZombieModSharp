using Sharp.Shared.GameEntities;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Enums;
using Microsoft.Extensions.Logging;
using ZombieModSharp.Abstractions;
using Sharp.Shared;
using ZombieModSharp.Abstractions.Entities;

namespace ZombieModSharp.Core.Modules;

public class Infect : IInfect
{
    private readonly ISharedSystem _sharedSystem;
    private readonly IEntityManager _entityManager;
    private readonly IEventManager _eventManager;
    private readonly ILogger<Infect> _logger;
    private readonly IPlayerManager _player;
    private readonly IModSharp _modSharp;

    private bool InfectStarted = false;

    public Infect(ISharedSystem sharedSystem, ILogger<Infect> logger, IPlayerManager player, IModSharp modSharp)
    {
        _sharedSystem = sharedSystem;
        _entityManager = _sharedSystem.GetEntityManager();
        _eventManager = _sharedSystem.GetEventManager();
        _logger = logger;
        _player = player;
        _modSharp = modSharp;
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

        var zmPlayer = _player.GetOrCreatePlayer(client);
        zmPlayer.IsZombie = true;

        var clientController = client.GetPlayerController();

        if(clientController == null)
        {
            _logger.LogError("The client controller is null!");
            return;
        }

        clientController.SwitchTeam(CStrikeTeam.TE);

        // implement model changed and health.
        var pawn = clientController.GetPlayerPawn();

        if(pawn == null)
        {
            _logger.LogError("The client controller is null!");
            return;
        }

        pawn.Health = 8000;
        pawn.ArmorValue = 0;
        try
        {
            pawn.GetItemService()!.HasHelmet = false;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: {ex}", ex);
        }

        pawn.SetModel("characters/models/s2ze/zombie_frozen/zombie_frozen.vmdl");

        // forcing drop all weapon.
        var weapons = pawn.GetWeaponService()?.GetMyWeapons();

        if (weapons == null)
        {
            _logger.LogError("Client weapon is null");
            return;
        }

        // drop all weapon that has hammerid.
        foreach (var weapon in weapons)
        {
            var entity = _entityManager.FindEntityByHandle(weapon)?.As<IBaseWeapon>();

            if (entity == null)
                continue;

            if (!string.IsNullOrEmpty(entity.HammerId))
            {
                pawn.DropWeapon(entity);
            }
        }

        pawn.RemoveAllItems();
        pawn.GiveNamedItem(EconItemId.KnifeTe);

        if (attacker == null)
            return;

        CheckGameStatus();

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

        var zmPlayer = _player.GetOrCreatePlayer(client);
        zmPlayer.IsZombie = false;

        var clientController = client.GetPlayerController();

        if(clientController == null)
        {
            _logger.LogError("The client controller is null!");
            return;
        }

        clientController.SwitchTeam(CStrikeTeam.CT);

        // implement model changed and health.
        var pawn = clientController.GetPawn();

        if(pawn == null)
        {
            _logger.LogError("The client controller is null!");
            return;
        }

        pawn.Health = 100;
        pawn.ArmorValue = 100;
        pawn.SetModel("characters/models/oylsister/uma_musume/gold_ship/goldship2.vmdl");
    }

    public void OnRoundPreStart()
    {
        var allPlayers = _player.GetAllPlayers();

        foreach (var kvp in allPlayers)
        {
            var client = kvp.Key;
            var zmPlayer = kvp.Value;

            zmPlayer.IsZombie = false;

            var controller = client.GetPlayerController();
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

    public void OnRoundFreezeEnd()
    {
        // start countdown.
        InitialCountDown();
        //_modSharp.PrintChannelAll(HudPrintChannel.Chat, "Infect round freeze is called");
    }

    public void OnRoundEnd()
    {
        InfectStarted = false;

        var allPlayers = _player.GetAllPlayers();

        foreach (var kvp in allPlayers)
        {
            var client = kvp.Key;
            var zmPlayer = kvp.Value;

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

            var controller = client.GetPlayerController();
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

    private void InitialCountDown()
    {
        int timerCount = 15;

        var timer = _modSharp.PushTimer(new Func<TimerAction>(() =>
        {
            try
            {
                if (!IsInfectStarted())
                    InfectMotherZombie();
                    
                return TimerAction.Continue;
            }
            catch (Exception e)
            {
                _logger.LogError("Error: {e}", e);
                return TimerAction.Stop;
            }
        }), 15.0f, GameTimerFlags.StopOnRoundEnd | GameTimerFlags.StopOnMapEnd);

        _modSharp.PrintChannelAll(HudPrintChannel.Hint, $"First infection start in {timerCount} seconds");

        var countdown = _modSharp.PushTimer(new Func<TimerAction>(() =>
        {
            try
            {
                timerCount--;
                _modSharp.PrintChannelAll(HudPrintChannel.Hint, $"First infection start in {timerCount} seconds");

                if (timerCount <= 0)
                    return TimerAction.Stop;

                if (IsInfectStarted())
                    return TimerAction.Stop;

                return TimerAction.Continue;
            }
            catch (Exception e)
            {
                _logger.LogError("Error: {e}", e);
                return TimerAction.Stop;
            }
        }), 1.0f, GameTimerFlags.Repeatable | GameTimerFlags.StopOnRoundEnd | GameTimerFlags.StopOnMapEnd);
    }

    private void InfectMotherZombie()
    {
        // Get All Player with motherzombie status, and alive.
        var candidate = _player.GetAllPlayers().Where(p => p.Value.MotherZombieStatus == MotherZombieStatus.None
            && (p.Key.GetPlayerController()?.IsAlive ?? false));

        var totalPlayer = _player.GetAllPlayers().Where(p => p.Key.GetPlayerController()?.IsAlive ?? false).Count();

        // Calculate
        var requireZm = totalPlayer / 7;

        // if zombie is less than 0 then make one.
        if (requireZm <= 0)
            requireZm = 1;

        int made = 0;

        // this part is mother zombie reset
        if (requireZm > candidate.Count())
        {
            // if any candidate left here.
            if (candidate.Any())
            {
                // we just confirm their infection right the way.
                foreach (var player in candidate)
                {
                    made++;
                    InfectPlayer(player.Key, null, true, false);
                    player.Value.MotherZombieStatus = MotherZombieStatus.Chosen;
                }
            }

            // reset status
            foreach (var player in _player.GetAllPlayers().Where(p => p.Value.MotherZombieStatus == MotherZombieStatus.Last))
            {
                player.Value.MotherZombieStatus = MotherZombieStatus.None;
            }

            // getting candidate again.
            candidate = _player.GetAllPlayers().Where(p => p.Value.MotherZombieStatus == MotherZombieStatus.None
                && (p.Key.GetPlayerController()?.IsAlive ?? false));

            _modSharp.PrintChannelAll(HudPrintChannel.Chat, "Mother Zombie has been reset.");
        }

        if (requireZm - made <= 0)
            return;

        var random = new Random();
        var shuffledCandidates = candidate.OrderBy(x => random.Next()).ToList();
        var selectedMotherZombies = shuffledCandidates.Take(requireZm - made);

        foreach (var player in selectedMotherZombies)
        {
            InfectPlayer(player.Key, null, true, false);
            player.Value.MotherZombieStatus = MotherZombieStatus.Chosen;
        }
    }

    public bool IsInfectStarted()
    {
        return InfectStarted;
    }

    public void SetInfectStarted(bool result)
    {
        InfectStarted = result;
    }
}
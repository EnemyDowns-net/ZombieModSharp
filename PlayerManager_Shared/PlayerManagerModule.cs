using Microsoft.Extensions.Logging;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using Sharp.Shared.Units;
using PlayerManager_Shared.Abstractions;
using System;
using System.Linq;
using static CSource2Metrics_MatchPerfSummary_Notification.Types;

namespace PlayerManager_Shared
{
    public class PlayerManagerModule : IPlayerManager, IEventListener
    {
        public int ListenerVersion => IEventListener.ApiVersion;
        public int ListenerPriority => 0;

        private readonly GamePlayer?[] _players;
        private readonly ILogger<PlayerManagerModule> _logger;
        private readonly IEntityManager _entityManager;
        private readonly IClientManager _clientManager;   // ✅ 新增欄位

        public event Action<GamePlayer>? ClientPutInServer;
        public event Action<GamePlayer>? ClientDisconnected;

        public PlayerManagerModule(
            IEntityManager entityManager,
            IClientManager clientManager,
            ILogger<PlayerManagerModule> logger)
        {
            _entityManager = entityManager;
            _clientManager = clientManager;   // ✅ 指派
            _logger = logger;
            _players = new GamePlayer?[PlayerSlot.MaxPlayerSlot];
        }

        // ✅ 實作 IEventListener.FireGameEvent
        public void FireGameEvent(IGameEvent e)
        {
            

            switch (e.Name?.ToLowerInvariant())
            {
                case "player_connect":
                {
                    var userId = new UserID((ushort)e.GetInt("userid"));
                    var client = _clientManager.GetGameClient(userId);
                    if (client != null)
                    {
                        OnClientConnected(client);
                        
                    }
                    break;
                }

                case "player_connect_full":
                case "player_activate":
                case "player_spawn":
                {
                    var userId = new UserID((ushort)e.GetInt("userid"));
                    var client = _clientManager.GetGameClient(userId);
                    if (client != null)
                    {
                        OnClientPutInServer(client);
                        
                    }
                    break;
                }

                case "player_disconnect":
                {
                    var userId = new UserID((ushort)e.GetInt("userid"));
                    var client = _clientManager.GetGameClient(userId);
                    if (client != null)
                    {
                        OnClientDisconnected(client, NetworkDisconnectionReason.Disconnected);
                       
                    }
                    break;
                }
            }

            // 額外：每次事件後都可以 log 快取人數
            var currentPlayers = GetPlayers(ignoreFakeClient: false).Length;
            
        }

        public void OnClientConnected(IGameClient client)
        {
            var slot = client.Slot;

            if (_players[slot] is { } old &&
                old.Client.Equals(client) &&
                old.SteamId == client.SteamId)
            {
                _logger.LogWarning("Double connection with same slot. old: {old}, new: {new}", old.Client, client);
                return;
            }

            _players[slot] = new GamePlayer(client);
        }

        public void OnClientPutInServer(IGameClient client)
        {
            var slot = client.Slot;

            if (_players[slot] is not { } player || player.SteamId != client.SteamId)
                return;

            player.UpdateClient(client);

            var controller = _entityManager.FindEntityByIndex(client.ControllerIndex);
            player.SetController(controller);

            ClientPutInServer?.Invoke(player);
        }

        public void OnClientDisconnected(IGameClient client, NetworkDisconnectionReason reason)
        {
            var slot = client.Slot;

            if (_players[slot] is not { } player || !player.Client.Equals(client))
                return;

            ClientDisconnected?.Invoke(player);

            player.Invalidate();
            _players[slot] = null;
        }

        public IGamePlayer? GetPlayer(PlayerSlot slot)
            => _players[slot];

        public IGamePlayer? GetPlayer(SteamID steamId)
        {
            return _players.FirstOrDefault(p =>
                p is not null &&
                p.IsValid() &&
                p.SteamId == steamId);
        }

        public IGamePlayer? GetPlayer(IGameClient client)
        {
            return _players.FirstOrDefault(p =>
                p is not null &&
                p.IsValid() &&
                p.Client.Equals(client));
        }

        public IGamePlayer[] GetPlayers(bool ignoreFakeClient = true)
        {
            return _players
                .Where(Filter)
                .ToArray()!;

            bool Filter(GamePlayer? player)
            {
                if (player is null || !player.IsValid())
                    return false;

                if (ignoreFakeClient && player.IsFakeClient)
                    return false;

                return player.Client.SignOnState >= SignOnState.Connected;
            }
        }
    }
}

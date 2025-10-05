
using Sharp.Shared.Enums;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using Sharp.Shared.Units;
using PlayerManager_Shared.Abstractions;

namespace PlayerManager_Shared
{
    public class GamePlayer : IGamePlayer
    {
        public IGameClient Client { get; private set; }
        public SteamID SteamId => Client.SteamId;
        public bool IsFakeClient => Client.IsFakeClient;

        private object? _controller;

        public GamePlayer(IGameClient client)
        {
            Client = client;
        }

        public void UpdateClient(IGameClient client)
        {
            Client = client;
        }

        public void SetController(object? controller)
        {
            _controller = controller;
        }

        public void Invalidate()
        {
            Client = null!;
            _controller = null;
        }

        public bool IsValid()
        {
            // 改用 Client.IsValid 來判斷快取是否有效
            return Client != null && Client.IsValid;
        }

        // ✅ 額外封裝（可選）
        public string Name => Client.Name;
        public bool IsConnected => Client.SignOnState >= SignOnState.Connected;
        public void SendMessage(string message) => Client.SayChatMessage(false, message);
        public void ExecuteCommand(string command) => Client.Command(command);
    }
}

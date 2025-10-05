using Sharp.Shared.Enums;
using Sharp.Shared.Objects;
using Sharp.Shared.Units;

namespace PlayerManager_Shared.Abstractions;

public interface IPlayerManager
{
    static string Identity => typeof(IPlayerManager).FullName ?? nameof(IPlayerManager);

    public IGamePlayer[] GetPlayers(bool ignoreFakeClient = true);
    public IGamePlayer? GetPlayer(IGameClient client);
    public IGamePlayer? GetPlayer(SteamID steamId);
    public IGamePlayer? GetPlayer(PlayerSlot slot);
    public void OnClientDisconnected(IGameClient client, NetworkDisconnectionReason reason);
    public void OnClientPutInServer(IGameClient client);
    public void OnClientConnected(IGameClient client);
}
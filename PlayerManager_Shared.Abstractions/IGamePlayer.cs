using Sharp.Shared.Objects;
using Sharp.Shared.Units;

namespace PlayerManager_Shared.Abstractions;

public interface IGamePlayer
{
    IGameClient Client { get; }
    SteamID SteamId { get; }
    bool IsFakeClient { get; }

    void UpdateClient(IGameClient client);
    void SetController(object? controller);
    void Invalidate();
    bool IsValid();

    // ✅ 新增
    string Name { get; }
    bool IsConnected { get; }
    void SendMessage(string message);
    void ExecuteCommand(string command);
}
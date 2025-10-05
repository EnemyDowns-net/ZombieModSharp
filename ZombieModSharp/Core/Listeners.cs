using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using ZombieModSharp.Interface.Listeners;
using ZombieModSharp.Interface.Player;

namespace ZombieModSharp.Core;

public class Listeners : IListeners, IClientListener
{
    public int ListenerVersion => IClientListener.ApiVersion;
    public int ListenerPriority => 0;

    private readonly IPlayer _player;

    public Listeners(IPlayer player)
    {
        _player = player;
    }

    public void RegisterListners()
    {

    }

    void OnClientPutInServer(IGameClient client)
    {
        _player.GetPlayer(client);
    }

    void OnClientDisconnect(IGameClient client)
    {
        _player.RemovePlayer(client);
    }
}
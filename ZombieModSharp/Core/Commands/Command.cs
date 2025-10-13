using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using ZombieModSharp.Core.Player;
using ZombieModSharp.Interface.Command;
using ZombieModSharp.Interface.Player;
using ZombieModSharp.Interface.ZTele;

public class Command : ICommand
{
    private readonly IClientManager _clientManager;
    private readonly IModSharp _modsharp;
    private readonly IPlayer _player;
    private readonly IZTele _ztele;

    public Command(IClientManager clientManager, IModSharp modSharp, IPlayer player, IZTele ztele)
    {
        _clientManager = clientManager;
        _modsharp = modSharp;
        _player = player;
        _ztele = ztele;
    }

    public void PostInit()
    {
        _clientManager.InstallCommandCallback("ms_ztele", ZTeleCommand);
    }

    public ECommandAction ZTeleCommand(IGameClient client, StringCommand command)
    {
        var playerInfo = _player.GetPlayer(client);

        var receiver = new RecipientFilter(client.Slot);

        if(client == null || playerInfo == null)
        {
            _modsharp.PrintChannelFilter(HudPrintChannel.Chat, "Invalid Client.", receiver);
            return ECommandAction.Handled;
        }

        _modsharp.PrintChannelFilter(HudPrintChannel.Chat, "Teleport back to spawn.", receiver);
        _ztele.TeleportToSpawn(client);
        return ECommandAction.Skipped;
    }
}
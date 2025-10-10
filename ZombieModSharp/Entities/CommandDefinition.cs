using PlayerManager_Shared.Abstractions;
using Sharp.Shared;
using Sharp.Shared.Objects;

public class CommandDefinition
{
    public string Name { get; }
    public string ConsoleName { get; }
    public string Description { get; }
    public Action<ISharedSystem, IGamePlayer, IGameClient?> Action { get; }

    public CommandDefinition(string name, string consoleName, string description,
        Action<ISharedSystem, IGamePlayer, IGameClient?> action)
    {
        Name = name;
        ConsoleName = consoleName;
        Description = description;
        Action = action;
    }
}
    
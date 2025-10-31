using Microsoft.Extensions.Logging;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using TnmsPluginFoundation;
using TnmsPluginFoundation.Models.Command;

public class InfectCommand(IServiceProvider serviceProvider) : TnmsAbstractCommandBase(serviceProvider)
{
    private readonly TnmsPlugin _plugin;
    private readonly IInfect _infect;
    public override string CommandName => "infect";
    public override string CommandDescription => "This command infects a player.";
    public override TnmsCommandRegistrationType CommandRegistrationType { get; } = TnmsCommandRegistrationType.Client | TnmsCommandRegistrationType.Server;

    protected override void ExecuteCommand(IGameClient? player, StringCommand commandInfo, ValidatedArguments? validatedArguments)
    {
        
    }
}
namespace ZombieModSharp.Abstractions;

public interface ICommand
{
    public void Init();
    public void Shutdown();
}
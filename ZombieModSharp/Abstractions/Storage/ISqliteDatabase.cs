using ZombieModSharp.Abstractions.Entities;

namespace ZombieModSharp.Abstractions.Storage;

public interface ISqliteDatabase
{
    Task Init();
    void Shutdown();
    Task<SavedClasses?> GetPlayerClassesAsync(string playerAuth);
    Task<bool> InsertPlayerClassesAsync(string playerAuth, string humanClass, string zombieClass);
    Task<bool> InsertPlayerSoundAsync(string playerAuth, bool enabled, float volume = 100.0f);
    Task<SavedSound> GetPlayerSoundAsync(string playerAuth);
}
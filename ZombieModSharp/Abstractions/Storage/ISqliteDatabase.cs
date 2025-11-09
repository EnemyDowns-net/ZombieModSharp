using ZombieModSharp.Abstractions.Entities;

namespace ZombieModSharp.Abstractions.Storage;

public interface ISqliteDatabase
{
    Task Init();
    Task<SavedClasses?> GetPlayerClassesAsync(string playerAuth);
    Task<bool> InsertPlayerClassesAsync(string playerAuth, string humanClass, string zombieClass);
}
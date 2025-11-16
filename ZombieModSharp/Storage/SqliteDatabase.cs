
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using ZombieModSharp.Abstractions.Entities;
using ZombieModSharp.Abstractions.Storage;
using ZombieModSharp.Core.Modules;

namespace ZombieModSharp.Storage;

public class SqliteDatabase : ISqliteDatabase
{
    // Class implementation goes here
    private readonly SqliteConnection _connection;
    private readonly ILogger<SqliteDatabase> _logger;

    public SqliteDatabase(string connectionString, ILogger<SqliteDatabase> logger)
    {
        _logger = logger;
        _connection = new SqliteConnection(connectionString);
        _connection.Open();
    }

    public async Task Init()
    {
        _logger.LogInformation("Initialized database.");
        
        // table for player classes
        await _connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS zs_playerclasses (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                player_auth VARCHAR(64) NOT NULL UNIQUE,
                human_class TEXT NOT NULL,
                zombie_class TEXT NOT NULL
            );");

        await _connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS zs_playersound (
                Id INTEGER PRIMARY KEY,
                sound_enable INTEGER NOT NULL,
                volume FLOAT NOT NULL DEFAULT 100.0,
                FOREIGN KEY(Id) REFERENCES zs_playerclasses(Id) ON DELETE CASCADE
            );");
    }

    public void Shutdown()
    {
        _connection.Close();
    }

    public async Task<bool> InsertPlayerClassesAsync(string playerAuth, string humanClass, string zombieClass)
    {
        // _logger.LogInformation("Insert classes for {playerAuth} : {human} | {zombie}", playerAuth, humanClass, zombieClass);
        
        var result = await _connection.ExecuteAsync(@"
            INSERT INTO zs_playerclasses (player_auth, human_class, zombie_class)
            VALUES (@PlayerAuth, @HumanClass, @ZombieClass)
            ON CONFLICT(player_auth) DO UPDATE SET
                human_class = @HumanClass,
                zombie_class = @ZombieClass;",
            new { PlayerAuth = playerAuth, HumanClass = humanClass, ZombieClass = zombieClass });

        return result > 0;
    }

    public async Task<SavedClasses?> GetPlayerClassesAsync(string playerAuth)
    {
        // _logger.LogInformation("Try get data for {steamid}", playerAuth);

        var classes = await _connection.QueryFirstOrDefaultAsync<SavedClasses>(@"
            SELECT zombie_class AS ZombieClass, human_class AS HumanClass
            FROM zs_playerclasses
            WHERE player_auth = @PlayerAuth;",
            new { PlayerAuth = playerAuth });

        return classes;
    }

    public async Task<bool> InsertPlayerSoundAsync(string playerAuth, bool enabled, float volume = 100.0f)
    {
        // 1. Get the Id from zs_playerclasses
        var id = await _connection.QueryFirstOrDefaultAsync<int?>(@"
            SELECT Id FROM zs_playerclasses WHERE player_auth = @PlayerAuth;",
            new { PlayerAuth = playerAuth });

        if (id == null)
        {
            _logger.LogInformation("ID is null!");
            return false; // Player must exist in zs_playerclasses
        }

        // 2. Insert or update zs_playersound
        var result = await _connection.ExecuteAsync(@"
            INSERT INTO zs_playersound (Id, sound_enable, volume)
            VALUES (@Id, @SoundEnable, @Volume)
            ON CONFLICT(Id) DO UPDATE SET sound_enable = @SoundEnable, volume = @Volume;",
            new { Id = id.Value, SoundEnable = enabled ? 1 : 0, Volume = volume });

        // _logger.LogInformation("Try to insert {id} as {bool}", id.Value, enabled);

        return result > 0;
    }

    public async Task<SavedSound> GetPlayerSoundAsync(string playerAuth)
    {
        var result = await _connection.QueryFirstOrDefaultAsync<SavedSound>(@"
            SELECT s.sound_enable AS Enabled, s.volume AS Volume 
            FROM zs_playersound s
            JOIN zs_playerclasses c ON s.Id = c.Id
            WHERE c.player_auth = @PlayerAuth;",
            new { PlayerAuth = playerAuth }
        );

        if (result == null)
        {
            return new SavedSound();
        }

        return result;
    }
}
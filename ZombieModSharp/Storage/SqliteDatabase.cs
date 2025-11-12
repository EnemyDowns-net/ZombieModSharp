
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
}
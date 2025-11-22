# ZombieModSharp

This is the modsharp module for popular zombie infection gameplay that known as Zombie Escape, yes you can try run it as Zombie Mod, since most of the based-code is about simply zombie infection gameplay.

And No I will not make modifying that support various mode such as zombie swarm, zombie riot, zombie plauge, etc. because **I'm too lazy** and if you want, the based-code is here and do it yourself. 

The project is continuous from **[ZombieSharp](https://github.com/oylsister/ZombieSharp)** which is CSSharp project, however, zombiemodsharp doesn't contain all feature from this previous project, since first focusing is to make gamemode playable first.

### Build

#### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Git (for cloning the repository)

#### Building from Source

1. **Clone the repository**
   ```bash
   git clone https://github.com/EnemyDowns-net/ZombieModSharp.git
   cd ZombieModSharp
   ```

2. **Build the project**
   ```bash
   dotnet build
   ```

3. **Output location**
   - The compiled DLL will be located at:
     ```
     ZombieModSharp/bin/Release/net10.0/ZombieModSharp.dll
     ```

#### Installation

1. Copy the compiled `ZombieModSharp.dll` to your ModSharp modules directory:
   ```
   game/sharp/modules/
   ```

2. Copy the required configuration files (in build folder):
   ```
   game/sharp/configs/zombiemodsharp/
   ```

3. Copy the gamedata file (in build folder):
   ```
   game/sharp/gamedata/ZombieModSharp.jsonc
   ```

4. Start your server.

### Configuration

All configuration files are located in `game/csgo/sharp/configs/zombiemodsharp/`

#### Configuration Files

##### 1. `weapons.jsonc` - Weapon Knockback Settings

Configure knockback multipliers for each weapon. Higher values = more knockback on zombies.

```jsonc
{
    "ak47": {
        "EntityName": "weapon_ak47",
        "Knockback": 1.1
    },
    "awp": {
        "EntityName": "weapon_awp",
        "Knockback": 0.8
    }
    // ... more weapons
}
```

- **EntityName**: The weapon's entity name in CS2
- **Knockback**: Multiplier for knockback force (default: 1.0)
  - Values < 1.0 = less knockback
  - Values > 1.0 = more knockback

##### 2. `hitgroups.jsonc` - Hitbox Knockback Multipliers

Configure knockback multipliers based on where the zombie is hit.

```jsonc
{
    "0": 1.2,  // Generic
    "1": 1.5,  // Head
    "2": 1.2,  // Chest
    "3": 1.1,  // Stomach
    "4": 1.0,  // Left Arm
    "5": 1.0,  // Right Arm
    "6": 0.9,  // Left Leg
    "7": 0.9,  // Right Leg
    "8": 1.0,  // Neck
    "9": 1.0   // Generic (unused)
}
```

**Hitgroup IDs:**
- 0 = Generic
- 1 = Head
- 2 = Chest
- 3 = Stomach
- 4 = Left Arm
- 5 = Right Arm
- 6 = Left Leg
- 7 = Right Leg
- 8 = Neck

##### 3. `playerclasses.jsonc` - Player Class Settings

Configure zombie and human classes with their properties.

```jsonc
{
    "zombie_default": {
        "Name": "Zombie Default",
        "Team": 0,                    // 0 = Zombie, 1 = Human
        "MotherZombie": false,        // Is this a mother zombie class?
        "Model": "path/to/model.vmdl",
        "Health": 8000,
        "Knockback": 0.0,             // Additional knockback resistance
        "Speed": 250.0                // Movement speed
    },
    "human_default": {
        "Name": "Human Default",
        "Team": 1,
        "MotherZombie": false,
        "Model": "path/to/model.vmdl",
        "Health": 100,
        "Knockback": 3.0,             // Knockback multiplier when shooting
        "Speed": 250.0
    }
}
```

**Properties:**
- **Name**: Display name for the class
- **Team**: 0 for Zombie, 1 for Human
- **MotherZombie**: Whether this class is for mother zombies
- **Model**: Path to the player model (must be precached)
- **Health**: Starting health points
- **Knockback**: Knockback modifier for this class
- **Speed**: Movement speed (250 = default CS2 speed)

##### 4. `resources.txt` - Resource Precaching

List all custom models and sounds that need to be precached on map load.

```
// Player models
characters/models/path/to/model.vmdl

// Sound events
soundevents/soundevents_custom.vsndevts
```

Add one resource per line. Lines starting with `//` are comments.

#### Knockback Formula

The final knockback force is calculated as:
```
Knockback = Damage × ClassKnockback × WeaponKnockback × HitgroupKnockback × BaseMultiplier
```

**Example:**
- Damage: 30
- Class Knockback: 4.0
- Weapon Knockback: 1.1 (AK-47)
- Hitgroup Knockback: 1.5 (Head)
- Base Multiplier: 1.0

Result: `30 × 4.0 × 1.1 × 1.5 = 198` Velocity knockback


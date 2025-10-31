# Dependency Injection Implementation Guide

## Current State
The project currently instantiates dependencies manually in the `ZombieModSharp` constructor, which creates tight coupling and makes testing difficult.

## Recommended Implementation

### 1. Register Services in ServiceCollection

Replace the manual instantiation in `ZombieModSharp.cs` constructor with proper service registration:

```csharp
private void RegisterServices(ServiceCollection services)
{
    // Register core interfaces
    services.AddSingleton<IPlayer, Player>();
    services.AddSingleton<IInfect, Infect>();
    services.AddSingleton<IZTele, ZTele>();
    services.AddSingleton<IEvents, Events>();
    services.AddSingleton<IListeners, Listeners>();
    services.AddSingleton<ICommand, Command>();
    
    // Register external dependencies that are already available
    services.AddSingleton(_sharedSystem);
    services.AddSingleton(_sharedSystem.GetEntityManager());
    services.AddSingleton(_sharedSystem.GetEventManager());
    services.AddSingleton(_sharedSystem.GetModSharp());
    services.AddSingleton(_sharedSystem.GetClientManager());
}

public ZombieModSharp(ISharedSystem sharedSystem,
                      string dllPath,
                      string sharpPath,
                      Version? version,
                      IConfiguration? coreConfiguration,
                      bool hotReload)
{
    // ... existing null checks ...
    
    _sharedSystem = sharedSystem ?? throw new ArgumentNullException(nameof(sharedSystem));
    
    var services = new ServiceCollection();
    
    // Add logging
    services.AddSingleton(sharedSystem.GetLoggerFactory());
    services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
    
    // Register all our services
    RegisterServices(services);
    
    _serviceProvider = services.BuildServiceProvider();
    _logger = sharedSystem.GetLoggerFactory().CreateLogger<ZombieModSharp>();
    
    // Get services from container
    _player = _serviceProvider.GetRequiredService<IPlayer>();
    _infect = _serviceProvider.GetRequiredService<IInfect>();
    _ztele = _serviceProvider.GetRequiredService<IZTele>();
    _eventListener = _serviceProvider.GetRequiredService<IEvents>();
    _listeners = _serviceProvider.GetRequiredService<IListeners>();
    _command = _serviceProvider.GetRequiredService<ICommand>();
}
```

### 2. Update Constructor Dependencies

Update all implementation classes to receive their dependencies through constructors:

#### Player.cs (already correct)
```csharp
public class Player : IPlayer
{
    // No dependencies needed - already correct
}
```

#### Infect.cs (already correctly using constructor injection)
```csharp
public class Infect : IInfect
{
    public Infect(IEntityManager entityManager, IEventManager eventManager, ILogger<Infect> logger, IPlayer player, IModSharp modSharp)
    {
        // Already correctly implemented
    }
}
```

#### Events.cs (already correctly using constructor injection)
```csharp
public class Events : IEvents, IEventListener
{
    public Events(ISharedSystem sharedSystem, ILogger<Events> logger, IPlayer player, IInfect infect, IZTele ztele)
    {
        // Already correctly implemented
    }
}
```

### 3. Service Lifetimes

Choose appropriate service lifetimes:

- **Singleton**: For stateful services that should be shared across the application
  - `IPlayer` - Maintains player state
  - `IInfect` - Manages infection state
  - `IEvents` - Event handling
  
- **Transient**: For stateless services or when you need a new instance each time
  - Use sparingly in this context
  
- **Scoped**: Not typically used in game plugins

### 4. Benefits of This Approach

1. **Testability**: Easy to mock dependencies for unit testing
2. **Loose Coupling**: Classes depend on interfaces, not concrete implementations
3. **Configuration**: Service registration is centralized
4. **Extensibility**: Easy to swap implementations or add decorators
5. **Dependency Management**: Container handles complex dependency graphs

### 5. Future Implementation Example

When adding new services, follow this pattern:

```csharp
// 1. Define interface
public interface IWeaponManager
{
    void GiveWeapon(IGameClient client, string weaponName);
    void RemoveWeapons(IGameClient client);
}

// 2. Implement interface
public class WeaponManager : IWeaponManager
{
    private readonly IEntityManager _entityManager;
    private readonly ILogger<WeaponManager> _logger;
    private readonly IPlayer _player;
    
    public WeaponManager(IEntityManager entityManager, ILogger<WeaponManager> logger, IPlayer player)
    {
        _entityManager = entityManager;
        _logger = logger;
        _player = player;
    }
    
    // Implementation...
}

// 3. Register in ServiceCollection
services.AddSingleton<IWeaponManager, WeaponManager>();

// 4. Use in other classes via constructor injection
public class SomeOtherClass
{
    private readonly IWeaponManager _weaponManager;
    
    public SomeOtherClass(IWeaponManager weaponManager)
    {
        _weaponManager = weaponManager;
    }
}
```

### 6. Testing Benefits

With proper DI, testing becomes much easier:

```csharp
[Test]
public void TestInfectPlayer()
{
    // Arrange
    var mockEntityManager = new Mock<IEntityManager>();
    var mockEventManager = new Mock<IEventManager>();
    var mockLogger = new Mock<ILogger<Infect>>();
    var mockPlayer = new Mock<IPlayer>();
    var mockModSharp = new Mock<IModSharp>();
    
    var infect = new Infect(
        mockEntityManager.Object,
        mockEventManager.Object,
        mockLogger.Object,
        mockPlayer.Object,
        mockModSharp.Object
    );
    
    // Act & Assert
    // Test your logic with mocked dependencies
}
```

## Summary

The project has the foundation for dependency injection (using `ServiceCollection` and getting loggers from the container) but needs to be fully implemented for the custom interfaces. The constructors are already correctly designed for DI, so the main work is registering the services properly and resolving them from the container.
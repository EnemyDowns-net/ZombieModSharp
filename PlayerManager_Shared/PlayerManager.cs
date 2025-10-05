using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Definition;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using PlayerManager_Shared.Abstractions;

using static Sharp.Shared.Definition.ChatColor;

namespace PlayerManager_Shared
{
    public sealed class PlayerManager : IModSharpModule
    {
        public string DisplayName => "PlayerManager_Shared";
        public string DisplayAuthor => "Widez";

        private readonly ILogger<PlayerManager> _logger;
        private readonly ISharedSystem _sharedSystem;
        private readonly IPlayerManager _playerManager;

        public PlayerManager(
            ISharedSystem sharedSystem,
            string? dllPath,
            string? sharpPath,
            Version? version,
            IConfiguration? coreConfiguration,
            bool hotReload)
        {
            _sharedSystem = sharedSystem ?? throw new ArgumentNullException(nameof(sharedSystem));
            _logger = _sharedSystem.GetLoggerFactory().CreateLogger<PlayerManager>();
            _playerManager = new PlayerManagerModule(
                _sharedSystem.GetEntityManager(),
                _sharedSystem.GetClientManager(),
                _sharedSystem.GetLoggerFactory().CreateLogger<PlayerManagerModule>()
            );
        }

        public bool Init()
        {
            
            return true;
        }

        public void PostInit()
        {
            
            // 安裝事件監聽器
            _sharedSystem.GetEventManager().InstallEventListener((IEventListener)_playerManager);

            var eventMgr = _sharedSystem.GetEventManager();
            eventMgr.HookEvent("player_connect");
            eventMgr.HookEvent("player_connect_full");
            eventMgr.HookEvent("player_activate");
            eventMgr.HookEvent("player_spawn");
            eventMgr.HookEvent("player_disconnect");

            // ✅ 將 IPlayerManager 註冊給 ModuleManager，供其他模組引用
            _sharedSystem.GetSharpModuleManager()
                .RegisterSharpModuleInterface<IPlayerManager>(
                    this,
                    IPlayerManager.Identity,   // identity，其他模組用這個名稱來取得
                    _playerManager
                );

            _logger.LogInformation("PlayerManager 啟動成功");

        }

        public void Shutdown()
        {
            _logger.LogInformation("PlayerManager 關閉");
        }

        
    }
}

# Changelog

## [2.0.1] - 2026-02-04

### Fixed
- **Active Profile Detection**: Player level now correctly uses the active profile instead of picking the highest level across all profiles
- **Profile-Aware Caching**: Level cache is now profile-aware and automatically updates when switching profiles without server restart
- **Debug Logging**: `debugLogging` config flag now properly controls debug output verbosity

### Improved
- **Startup Logging**: Cleaner console output - config details only shown when `debugLogging=true`
- **Player Level Logging**: Always shows resolved player level with source information (forced/activeProfile/firstProfile/default)
- **Profile Change Detection**: Logs profile changes and re-detects level automatically

### Technical
- Added `_cachedProfileId` to track which profile the cached level belongs to
- Cache invalidation on profile switch
- Improved logging structure: `LogInformation` for key status, `LogDebug` for detailed diagnostics

---

## [2.0.0] - 2026-02-03

### Added
- **Automatic Player Level Detection**: No manual configuration needed - reads directly from profile files
- **229 Keys Auto-Discovery**: Automatically discovers all keys and keycards from game database
- **Progressive Loot System**: Spawn rates scale with player level (Rookie/Survivor/Veteran/Elite tiers)
- **Map-Based Key Routing**: Keys spawn more frequently on their home maps (optional)
- **Price Adjustment System**: Balances economy by reducing key prices on flea market and traders
- **Smart Rarity System**: Keys categorized as Common, Rare, Super Rare, or Keycard

### Configuration
- `enabled`: Enable/disable the mod
- `debugLogging`: Enable detailed debug output
- `forcedPlayerLevel`: Override auto-detection (0 = auto)
- `baseKeySpawnChance`: Base spawn chance (default: 500)
- `enableProgressiveSystem`: Level-based spawn scaling
- `enableKeyRouting`: Map-based key routing
- `enablePriceAdjustment`: Price balancing
- `targetContainers`: Containers to modify

### Compatibility
- Works with LotsOfLoot Redux and similar loot mods
- Compatible with FIKA multiplayer
- SPT 4.0.11+ required

### Credits
- Original concept by MusicManiac
- v2.0 rewrite by miXXed

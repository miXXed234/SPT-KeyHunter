# KeyHunter v2.0.0 - SPT 4.x

A complete rewrite of the KeysInLoot concept by MusicManiac, designed for SPT 4.0.11+.

## Features

- 🤖 **Automatic Player Level Detection** - No manual configuration needed
- 🔑 **Automatic Key Discovery** - 229 keys automatically detected from game database
- 📊 **Progressive Loot System** - Keys spawn based on your level (Rookie → Elite)
- 🗺️ **Map-Based Key Routing** - Optional: Keys spawn more on their home maps
- 💰 **Price Adjustment System** - Balances economy when keys are more available
- 🎯 **Smart Rarity System** - Common, Rare, Super Rare, and Keycard tiers

## Installation

1. Download `KeyHunter-v2.0.0.zip` from Releases
2. Extract to your SPT folder (maintains `SPT/user/mods/KeyHunter` structure)
3. Start server - config generates automatically

## Configuration

Edit `config.json` in `SPT/user/mods/KeyHunter/`:

### Key Settings
- `forcedPlayerLevel`: Set to `0` for auto-detection, or specify manual level
- `baseKeySpawnChance`: Spawn probability (default: 500)
  - With LotsOfLoot: 500
  - Vanilla SPT: 500-1000
- `enableProgressiveSystem`: Level-based spawn rates (default: true)
- `enableKeyRouting`: Map-specific key spawns (default: false)
- `enablePriceAdjustment`: Reduce key prices (default: true)

### Level Tiers
- **Rookie (1-10)**: Common keys frequent, rare keys very limited
- **Survivor (11-25)**: Balanced spawn rates
- **Veteran (26-40)**: Most keys available
- **Elite (41+)**: All keys spawn freely (keycards slightly reduced)

## Compatibility

- ✅ SPT 4.0.11+
- ✅ LotsOfLoot Redux and similar loot mods
- ✅ FIKA multiplayer

## Credits

- Original concept: **MusicManiac**
- v2.0 rewrite: **miXXed**

## License

MIT License

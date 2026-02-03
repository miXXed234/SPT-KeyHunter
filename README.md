# KeyHunter for SPT 4.x

This mod was inspired by the "KeysInLoot" concept created by MusicManiac, designed for SPT 4.0.11+.
It increases the spawn rate of keys and keycards in specific containers (Jackets, Duffles, Dead Scavs) without flooding the loot tables.

## What's New in v2.0.0

- Automatic player level detection - no manual configuration needed
- 229 keys automatically discovered from the game database
- Progressive loot system that scales with your level
- Optional map-based key routing (keys spawn more on their home maps)
- Price adjustment system to balance the economy
- Smart rarity system (Common, Rare, Super Rare, Keycard)

## Configuration

Edit `config.json` to customize the mod:

- **enabled**: Enable or disable the mod
- **forcedPlayerLevel**: Set to `0` for automatic level detection, or specify a manual level
- **baseKeySpawnChance**: Spawn chance for keys (default: 500)
  - Recommended with LotsOfLoot: 500
  - Recommended vanilla SPT: 500-1000
- **enableProgressiveSystem**: Keys spawn based on your player level (default: true)
- **enableKeyRouting**: Keys spawn more frequently on their home maps (default: false)
- **enablePriceAdjustment**: Reduces key prices on flea market and traders (default: true)
- **targetContainers**: List of container IDs to modify
- **debugLogging**: Enable debug output in console

## How it Works

The mod runs on server start and patches the loot tables in the database.
It automatically detects all keys and keycards from the game database.
Keys are added to specified containers with configurable spawn chances.
If progressive system is enabled, spawn rates scale with your player level:
- Rookie (1-10): Common keys frequent, rare keys very limited
- Survivor (11-25): Balanced spawn rates
- Veteran (26-40): Most keys available
- Elite (41+): All keys spawn freely (keycards slightly reduced)

This increases the probability of finding keys in jackets, duffle bags, and dead scavs while maintaining balance.

## Compatibility

Works with LotsOfLoot Redux and similar loot mods.
Compatible with FIKA multiplayer.

## Credits

Original concept by MusicManiac.
v2.0 rewrite by miXXed.

# KeyHunter (C#) for SPT 4.x

This mod was inspired by the "KeysInLoot" concept created by MusicManiac, designed for SPT 4.0.11+.
It increases the spawn rate of keys and keycards in specific containers (Jackets, Duffles, Dead Scavs) without flooding the loot tables.

## Configuration

Edit `config.json` to customize the mod:
- **enabled**: Enable or disable the mod
- **keySpawnChance**: Spawn chance multiplier for keys (higher = more frequent)
- **keysToAdd**: List of key IDs to add to containers
- **targetContainers**: List of container IDs to modify
- **debugLogging**: Enable debug output in console

## How it Works

- The mod runs on server start and patches the loot tables in the database.
- It identifies all items that are Keys or Keycards from the `keysToAdd` list.
- It iterates through all locations and the specified target containers.
- For each container, it adds keys with the specified spawn chance.
- This increases the probability of finding keys in jackets, duffle bags, and dead scavs.

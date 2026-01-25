using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KeyHunter.Config
{
    public class ModConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("keysToAdd")]
        public List<string> KeysToAdd { get; set; } = new List<string>();

        [JsonPropertyName("keySpawnChance")]
        public float KeySpawnChance { get; set; } = 5000.0f;

        [JsonPropertyName("targetContainers")]
        public List<string> TargetContainers { get; set; } = new List<string>
        {
            "578f8778245977358849a9b5", // Jacket
            "578f87a3245977356274f2cb", // Duffel bag
            "5909e4b686f7747f5b744fa4"  // Dead Scav
        };

        [JsonPropertyName("debugLogging")]
        public bool DebugLogging { get; set; } = true;
    }
}

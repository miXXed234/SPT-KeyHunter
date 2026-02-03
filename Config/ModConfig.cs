using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KeyHunter.Config
{
    public class ModConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        [JsonPropertyName("debugLogging")]
        public bool DebugLogging { get; set; } = false;

        [JsonPropertyName("forcedPlayerLevel")]
        public int? ForcedPlayerLevel { get; set; } = 0;

        [JsonPropertyName("enableKeyRouting")]
        public bool EnableKeyRouting { get; set; } = false;

        [JsonPropertyName("homeMapMultiplier")]
        public float HomeMapMultiplier { get; set; } = 1.0f;

        [JsonPropertyName("otherMapMultiplier")]
        public float OtherMapMultiplier { get; set; } = 1.0f;

        [JsonPropertyName("enableProgressiveSystem")]
        public bool EnableProgressiveSystem { get; set; } = true;

        [JsonPropertyName("levelThresholds")]
        public LevelThresholds LevelThresholds { get; set; } = new();

        [JsonPropertyName("enablePriceAdjustment")]
        public bool EnablePriceAdjustment { get; set; } = true;

        [JsonPropertyName("fleaPriceMultiplier")]
        public float FleaPriceMultiplier { get; set; } = 0.75f;

        [JsonPropertyName("traderPriceMultiplier")]
        public float TraderPriceMultiplier { get; set; } = 0.75f;

        [JsonPropertyName("baseKeySpawnChance")]
        public float BaseKeySpawnChance { get; set; } = 500.0f;

        [JsonPropertyName("targetContainers")]
        public List<string> TargetContainers { get; set; } = new()
        {
            "578f8778245977358849a9b5",
            "578f87a3245977356274f2cb",
            "5909e4b686f7747f5b744fa4"
        };
    }

    public class LevelThresholds
    {
        [JsonPropertyName("rookieMaxLevel")]
        public int RookieMaxLevel { get; set; } = 10;

        [JsonPropertyName("survivorMaxLevel")]
        public int SurvivorMaxLevel { get; set; } = 25;

        [JsonPropertyName("veteranMaxLevel")]
        public int VeteranMaxLevel { get; set; } = 40;

        [JsonPropertyName("rookieMultipliers")]
        public RarityMultipliers Rookie { get; set; } = new()
        {
            Common = 1.0f,
            Rare = 0.6f,
            SuperRare = 0.3f,
            Keycard = 0.15f
        };

        [JsonPropertyName("survivorMultipliers")]
        public RarityMultipliers Survivor { get; set; } = new()
        {
            Common = 1.0f,
            Rare = 0.9f,
            SuperRare = 0.6f,
            Keycard = 0.35f
        };

        [JsonPropertyName("veteranMultipliers")]
        public RarityMultipliers Veteran { get; set; } = new()
        {
            Common = 1.0f,
            Rare = 1.0f,
            SuperRare = 0.8f,
            Keycard = 0.6f
        };

        [JsonPropertyName("eliteMultipliers")]
        public RarityMultipliers Elite { get; set; } = new()
        {
            Common = 1.0f,
            Rare = 1.0f,
            SuperRare = 1.0f,
            Keycard = 0.85f
        };
    }

    public class RarityMultipliers
    {
        [JsonPropertyName("common")]
        public float Common { get; set; } = 1.0f;

        [JsonPropertyName("rare")]
        public float Rare { get; set; } = 1.0f;

        [JsonPropertyName("superRare")]
        public float SuperRare { get; set; } = 1.0f;

        [JsonPropertyName("keycard")]
        public float Keycard { get; set; } = 1.0f;
    }
}

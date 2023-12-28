using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace CS2_HideAndSeek;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("Tag")] public string Tag { get; set; } = "[HNS]";
    [JsonPropertyName("TagColor")] public string TagColor { get; set; } = "Gold";
    [JsonPropertyName("RespawnTime")] public int RespawnTime { get; set; } = 50;
    [JsonPropertyName("SeekerHealth")] public int SeekerHealth { get; set; } = 777;
    [JsonPropertyName("TwoSeekers")] public int TwoSeekers { get; set; } = 7;
    [JsonPropertyName("ThreeSeekers")] public int ThreeSeekers { get; set; } = 10;
}
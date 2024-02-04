using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace CS2_HideAndSeek;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("RowEditFlag")] public string RowEditFlag { get; set; } = "@css/generic";
    [JsonPropertyName("Maniacs")]
    public Maniac[] Maniacs { get; set; } = new Maniac[]
    {
        // 1 по дефолту, то есть наичнаем с 2
        new Maniac(2, 6),
        new Maniac(3, 10)
    };
}
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace CS2_HideAndSeek;

public class Maniac
{
    // if ManiacCount = 2 And PlayersCount = 3 Then 1CT 2T's
    public int ManiacCount { get; set; } = 0;
    public int PlayersCount { get; set; } = 0;

    public Maniac(int maniacCount, int playersCount)
    {
        ManiacCount = maniacCount;
        PlayersCount = playersCount;
    }
}
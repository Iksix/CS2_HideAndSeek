using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Config;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using FormatWith;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace CS2_HideAndSeek;



public class CS2_HideAndSeek : BasePlugin, IPluginConfig<PluginConfig>
{
    // ================================ COLORS
    public static char Default = '\x01';
    public static char White = '\x01';
    public static char Darkred = '\x02';
    public static char Green = '\x04';
    public static char LightYellow = '\x03';
    public static char LightBlue = '\x03';
    public static char Olive = '\x05';
    public static char Lime = '\x06';
    public static char Red = '\x07';
    public static char Purple = '\x03';
    public static char Grey = '\x08';
    public static char Yellow = '\x09';
    public static char Gold = '\x10';
    public static char Silver = '\x0A';
    public static char Blue = '\x0B';
    public static char DarkBlue = '\x0C';
    public static char BlueGrey = '\x0D';
    public static char Magenta = '\x0E';
    public static char LightRed = '\x0F';
    // ================================
    
    // ================================ SETTINGS
    public string PluginTag = "[HNS]";
    public static char PluginTagColor = '\x10';
    public int RespawnPlayerTime = 50; // players will respawn first 50 seconds after round starts
    public int SeekerHealth = 777; // <1000 pls
    public bool HnsMode = true; // 0 or 1
    public int FlashbangForCT = 1; // 0 or 1
    public int SmokeForCT = 1; // 0 or 1
    // ================================ SETTINGS
    
    public override string ModuleName => "HideAndSeek";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "iks";

    private List<CCSPlayerController> _tPlayers = new();
    private List<CCSPlayerController> _playersInRow = new();
    
    private Timer? _respawnTimer;

    public int TwoSeekers = 6; // CT players count for 2 seekers
    public int ThreeSeekers = 10; // CT players count for 2 seekers
    
    public PluginConfig Config { get; set; }
    
    public void OnConfigParsed(PluginConfig config)
    {
        config = ConfigManager.Load<PluginConfig>("CS2_HideAndSeek");
        
        PluginTag = config.Tag;
        switch (config.TagColor)
        {
            case "Default":
                PluginTagColor = Default;
                break;
            case "White":
                PluginTagColor = White;
                break;
            case "Darkred":
                PluginTagColor = Darkred;
                break;
            case "Green":
                PluginTagColor = Green;
                break;
            case "LightYellow":
                PluginTagColor = LightYellow;
                break;
            case "LightBlue":
                PluginTagColor = LightBlue;
                break;
            case "Olive":
                PluginTagColor = Olive;
                break;
            case "Lime":
                PluginTagColor = Lime;
                break;
            case "Red":
                PluginTagColor = Red;
                break;
            case "Purple":
                PluginTagColor = Purple;
                break;
            case "Grey":
                PluginTagColor = Grey;
                break;
            case "Yellow":
                PluginTagColor = Yellow;
                break;
            case "Gold":
                PluginTagColor = Gold;
                break;
            case "Silver":
                PluginTagColor = Silver;
                break;
            case "Blue":
                PluginTagColor = Blue;
                break;
            case "DarkBlue":
                PluginTagColor = DarkBlue;
                break;
            case "BlueGrey":
                PluginTagColor = BlueGrey;
                break;
            case "Magenta":
                PluginTagColor = Magenta;
                break;
            case "LightRed":
                PluginTagColor = LightRed;
                break;
            default:
                // Handle the case where the color code is not recognized.
                // You might want to set a default color in this case.
                PluginTagColor = Default;
                break;
        }
        RespawnPlayerTime = config.RespawnTime;
        SeekerHealth = config.SeekerHealth;
        TwoSeekers = config.TwoSeekers;
        ThreeSeekers = config.ThreeSeekers;
        HnsMode = config.HnsMode;
        FlashbangForCT = config.FlashbangForCT;
        SmokeForCT = config.SmokeForCT;
        Console.WriteLine("[HNS] Config parsed");
        Config = config;
    }
    
    public override void Load(bool hotReload)
    {
            AddCommandListener("jointeam", (controller, info) => OnJointeam(controller, info), HookMode.Pre);
    }
    
    //================================== Commands 
    
    //========== hnsConsole - @css/root
    
    [ConsoleCommand("css_hns_reload")]
    public void OnReloadCommand(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller != null)
        {
            if (!AdminManager.PlayerHasPermissions(controller, "@css/root"))
            {
                return;
            }
        }
        if (controller != null)
        {
            controller.PrintToChat($" {PluginTagColor}{PluginTag} " + $"{Green}Config loaded!");
        }
        
        OnConfigParsed(Config);
    }
    
    [ConsoleCommand("css_hns")]
    public void OnHnsCommand(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller != null)
        {
            if (!AdminManager.PlayerHasPermissions(controller, "@css/root"))
            {
                return;
            }
        }

        if (HnsMode)
        {
            Console.WriteLine($" {PluginTag} " + "HNS mode is already enabled");
            if (controller != null)
            {
                controller.PrintToChat($" {PluginTagColor}{PluginTag} " + $"{Green}HNS mode is already enabled");
            }
            return;
        }

        HnsMode = true;
        Console.WriteLine($" {PluginTag} " + Localizer["main.onHns"]);
        Server.PrintToChatAll($" {PluginTagColor}{PluginTag} " + Localizer["main.onHns"]);
        _tPlayers.Clear();
        _playersInRow.Clear();
        if (_respawnTimer != null)
        {
            _respawnTimer.Kill();
            _respawnTimer = null; 
        }
        NativeAPI.IssueServerCommand("sv_cheats 1");
        NativeAPI.IssueServerCommand("endround");
        NativeAPI.IssueServerCommand("sv_cheats 0");
    }
    [ConsoleCommand("css_exithns")]
    public void OnExitHnsCommand(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller != null)
        {
            if (!AdminManager.PlayerHasPermissions(controller, "@css/root"))
            {
                return;
            }
        };

        if (!HnsMode)
        {
            Console.WriteLine($" {PluginTag} " + "HNS mode is already disabled");
            if (controller != null)
            {
                controller.PrintToChat($" {PluginTagColor}{PluginTag} " + $"{Red}HNS mode is already disabled");
            }
            return;
        }

        HnsMode = false;
        Console.WriteLine($" {PluginTag} " + Localizer["main.offHns"]);
        Server.PrintToChatAll($" {PluginTagColor}{PluginTag} " + Localizer["main.offHns"]);
        
        _tPlayers.Clear();
        _playersInRow.Clear();
        if (_respawnTimer != null)
        {
            _respawnTimer.Kill();
            
        }
        _respawnTimer = null;
        NativeAPI.IssueServerCommand("sv_cheats 1");
        NativeAPI.IssueServerCommand("endround");
        NativeAPI.IssueServerCommand("sv_cheats 0");
    }
    
    //========== /hnsConsole - @css/root
    
    [ConsoleCommand("css_row")]
    public void OnRowCommand(CCSPlayerController? controller, CommandInfo info)
    {
        
        if (controller == null) return;
        if (!HnsMode)
        {
            controller.PrintToChat($" {PluginTagColor}{PluginTag} {Red}HNS mode is disabled!");
            return;
        }
        for (int i = 0; i < _playersInRow.Count; i++)
        {
            if (_playersInRow[i] == controller)
            {
                _playersInRow.RemoveAt(i);
                info.ReplyToCommand($" {PluginTagColor}{PluginTag} " + Localizer["main.onOutQueue"]);
                return;
            }
        }
        _playersInRow.Add(controller);
        
        info.ReplyToCommand($" {PluginTagColor}{PluginTag} " + Localizer["main.onEnterQueue"]);
    }
    
    [ConsoleCommand("css_rowlist")]
    public void OnRowListCommand(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller == null) return;
        
        if (!HnsMode)
        {
            controller.PrintToChat($" {PluginTagColor}{PluginTag} {Red}HNS mode is disabled!");
            return;
        }
        controller.PrintToChat($" {PluginTagColor}{PluginTag} " + Localizer["main.onRowList"]);
        foreach (var player in _playersInRow)
        {
            controller.PrintToChat($" {Olive}{player.PlayerName}");
        }
    }
    //================================== Commands 
    
    //================================== Hooks 

    public HookResult OnJointeam(CCSPlayerController controller, CommandInfo info)
    {
        if (!HnsMode) return HookResult.Continue;
        
        if (controller.TeamNum == 2)
        {
            foreach (var player in _tPlayers)
            {
                if (controller == player)
                {
                    return HookResult.Continue;
                }
            }
            return HookResult.Handled;
        }

        if (controller.TeamNum == 3)
        {
            foreach (var player in _tPlayers)
            {
                if (controller == player)
                {
                    return HookResult.Handled;
                }
            }
        }
        return HookResult.Continue;
    }
    
    [GameEventHandler(HookMode.Pre)]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (!HnsMode) return HookResult.Continue;
        if (_respawnTimer != null)
        {
            _respawnTimer.Kill();
            _respawnTimer = null;
        }
        _tPlayers.Clear();
        List<CCSPlayerController> ctPlayers = new();
        List<CCSPlayerController> outRow = new();
        foreach (var player in Utilities.GetPlayers())
        {
            if (player.TeamNum > 1 && !player.IsBot)
            {
                player.SwitchTeam(CsTeam.CounterTerrorist);
                ctPlayers.Add(player);
            }

            foreach (var rowMember in _playersInRow)
            {
                if (player != rowMember && !player.IsBot)
                {
                    outRow.Add(player);
                }
            }
        }
        
        
        Random rnd = new Random((int)DateTime.Now.Ticks);
        
        // Players From Row
        if (ctPlayers.Count >= ThreeSeekers)
        {
            for (int i = 0; i < _playersInRow.Count && i < 3; i++)
            {
                int playerIndex = rnd.Next(0, _playersInRow.Count);
                
                _tPlayers.Add(_playersInRow[playerIndex]);
            }
            Console.WriteLine(_tPlayers.Count);
            for (int i = 0; i < 3 - _tPlayers.Count; i++)
            {
                _tPlayers.Add(outRow[rnd.Next(0, outRow.Count)]);
            }

            foreach (var player in (_tPlayers))
            {
                player.SwitchTeam(CsTeam.Terrorist);
            }
            _playersInRow.Clear();
            return HookResult.Continue;
        }
        if (ctPlayers.Count >= TwoSeekers)
        {
            for (int i = 0; i < _playersInRow.Count && i < 2; i++)
            {
                int playerIndex = rnd.Next(0, _playersInRow.Count);
                
                _tPlayers.Add(_playersInRow[playerIndex]);
            }
            for (int i = 0; i < 2 - _tPlayers.Count; i++)
            {
                _tPlayers.Add(outRow[rnd.Next(0, outRow.Count)]);
            }

            foreach (var player in (_tPlayers))
            {
                player.SwitchTeam(CsTeam.Terrorist);
            }
            _playersInRow.Clear();

            return HookResult.Continue;
        }
        int rnum = rnd.Next(0, Utilities.GetPlayers().Count);
        _tPlayers.Add(Utilities.GetPlayers()[rnum]);
        foreach (var player in _tPlayers)
        {
            player.SwitchTeam(CsTeam.Terrorist);
        }
        _playersInRow.Clear();

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        if (!HnsMode) return HookResult.Continue;
        if (_respawnTimer != null)
        {
            _respawnTimer.Kill();
            _respawnTimer = null;
        }
        var players = Utilities.GetPlayers();
        foreach (var player in _tPlayers)
        {
            if (player.PlayerPawn.Value != null)
                player.PlayerPawn.Value.Health = SeekerHealth;
        }

        foreach (var player in players)
        {
            if (player.TeamNum == 3)
            {
                for (int i = 0; i < FlashbangForCT; i++)
                {
                    player.GiveNamedItem(CsItem.Flashbang);
                }
                for (int i = 0; i < SmokeForCT; i++)
                {
                    player.GiveNamedItem(CsItem.SmokeGrenade);
                }
            }
        }

        int counter = RespawnPlayerTime;
        
        bool hasAlivePlayers = false;

        
        
        _respawnTimer = AddTimer(1, () =>
        {

            foreach (var player in players)
            {
                try
                {
                    if (player.PawnIsAlive)
                    {
                        hasAlivePlayers = true;
                    }
                }
                catch
                {
                    return; 
                }
            }
            
            if (!hasAlivePlayers)
            {
                _respawnTimer.Kill();
                _respawnTimer = null;
                return;
            }
            counter--;
            foreach (var player in players)
            {
                if (player.TeamNum > 1 && player.PlayerPawn.Value != null && player.PlayerPawn.Value.LifeState == 1)
                {
                    player.Respawn();
                    player.PrintToChat($" {PluginTagColor}{PluginTag} " + Localizer["main.WhenRespawn"]);
                }

                if (player.TeamNum != 2)
                {
                    player.PrintToCenter($" {PluginTagColor}{PluginTag} " + Localizer["main.RespawnTime"].ToString().FormatWith(new {seconds = counter}));
                }
            }
            if (counter <= 0)
            {
                if (_respawnTimer != null)
                {
                    _respawnTimer.Kill();
                    _respawnTimer = null;
                }
            }
        }, TimerFlags.REPEAT);
        
        return HookResult.Continue;
    }
    [GameEventHandler]
    public HookResult OnMapShutdown(EventMapShutdown @event, GameEventInfo info)
    {
        if (!HnsMode) return HookResult.Continue;
        if (_respawnTimer != null)
        {
            _respawnTimer.Kill();
        }
        _respawnTimer = null; 
        _tPlayers.Clear();
        _playersInRow.Clear();
        Console.WriteLine($" {PluginTag} Map Shutdown");
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        if (!HnsMode) return HookResult.Continue;
        CCSPlayerController controller = @event.Userid;
        if (controller.TeamNum == 2)
        {
            controller.PlayerPawn.Value.Health = SeekerHealth;
        }
        return HookResult.Continue;
    }
    
    //================================== Hooks 

    
}
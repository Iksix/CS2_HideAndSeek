using System.Diagnostics.Tracing;
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

namespace CS2_HideAndSeek;



public class CS2_HideAndSeek : BasePlugin, IPluginConfig<PluginConfig>
{

    public override string ModuleName => "CS2_HideAndSeek";
    public override string ModuleVersion => "2.0.0";
    public override string ModuleAuthor => "iks";

    public PluginConfig Config { get; set; }

    private List<CCSPlayerController> playersInRow = new List<CCSPlayerController>();
    private List<CCSPlayerController> seekers = new List<CCSPlayerController>();

    public override void Load(bool hotReload)
    {
        AddCommandListener("jointeam", (p, commandInfo) =>
        {
            if (p == null) return HookResult.Continue;
            var arg = XHelper.GetArgsFromCommandLine(commandInfo.GetCommandString);
            string NewTeam = arg[0];
            string OldTeam = p.TeamNum.ToString();

            if ((OldTeam == "3" && NewTeam == "1") || (OldTeam == "1" && NewTeam == "3") || (OldTeam == "0" && NewTeam != "2"))
            {
                return HookResult.Continue;
            }

            return HookResult.Stop;

        }, HookMode.Pre);
    }
    [GameEventHandler]
    public HookResult OnMapShutDown(EventMapShutdown @event, GameEventInfo info)
    {
        ClearAll();
        return HookResult.Continue;
    }
    [GameEventHandler]
    public HookResult OnGameEnd(EventGameEnd @event, GameEventInfo info)
    {
        ClearAll();
        return HookResult.Continue;
    }
    [GameEventHandler]
    public HookResult OnGameStart(EventGameStart @event, GameEventInfo info)
    {
        ClearAll();
        return HookResult.Continue;
    }

    public void OnConfigParsed(PluginConfig config)
    {
        config = ConfigManager.Load<PluginConfig>(ModuleName);

        Config = config;
    }

    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    [ConsoleCommand("css_row")]
    public void OnRowCommand(CCSPlayerController controller, CommandInfo info)
    {
        var target = playersInRow.FirstOrDefault(p => p == controller);

        if (target == null)
        {
            playersInRow.Add(controller);
            controller.PrintToChat($" {Localizer["PluginTag"]} {Localizer["RowEnter"]}");
            Server.PrintToChatAll($" {Localizer["PluginTag"]} {Localizer["Server.PlayerEnterRow"].ToString().Replace("{name}", controller.PlayerName)}");
            return;
        }

        playersInRow.Remove(controller);
        controller.PrintToChat($" {Localizer["PluginTag"]} {Localizer["RowLeave"]}");
        Server.PrintToChatAll($" {Localizer["PluginTag"]} {Localizer["Server.PlayerLeaveRow"].ToString().Replace("{name}", controller.PlayerName)}");
    }

    [ConsoleCommand("css_rowlist")]
    public void OnRowListCommand(CCSPlayerController controller, CommandInfo info)
    {
        info.ReplyToCommand($" {Localizer["PluginTag"]} {Localizer["RowList"]}");
        foreach (var p in playersInRow)
        {
            info.ReplyToCommand($" {ChatColors.Green}{p.PlayerName}");
        }
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        playersInRow.Remove(player);
        seekers.Remove(player);
        return HookResult.Continue;
    }


    [GameEventHandler(HookMode.Pre)]
    public HookResult OnRoundStart(EventRoundPrestart @event, GameEventInfo info)
    {

        // Получаем нужное кол-во маньяков, и активных игроков(Те которые либо за КТ либо за Т)
        var ManiacsCfg = Config.Maniacs.OrderByDescending(x => x.ManiacCount);
        int needManiacs = 1;

        List<CCSPlayerController> activePlayers = XHelper.GetOnlinePlayers().Where(p => p.TeamNum > 1).ToList();

        foreach (var m in ManiacsCfg)
        {
            if (needManiacs != 1) continue;

            if (activePlayers.Count() >= m.PlayersCount)
            {
                needManiacs = m.ManiacCount;
            }
        }

        Random rnd = new Random();

        // Пытаемся сначало заполнить сикеров игроками из очереди
        for (int i = 0; i < playersInRow.Count; i++)
        {
            if (seekers.Count() == needManiacs) continue; // Если их уже нужное кол-во просто скип
            var addPlayer = playersInRow[rnd.Next(0, playersInRow.Count())];
            seekers.Add(addPlayer);
            activePlayers.Remove(addPlayer); // Удаляем человека в сикерах из активных игроков
            playersInRow.Remove(addPlayer); // Удаляем человека в сикерах из очереди
        }

        // Теперь пытаемся заполнить оставшимися активными игроками
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (seekers.Count() == needManiacs) continue; // Если их уже нужное кол-во просто скип
            var addPlayer = activePlayers[rnd.Next(0, playersInRow.Count())];
            seekers.Add(addPlayer);
            activePlayers.Remove(addPlayer); // Удаляем человека в сикерах из активных игроков
        }


        // Переносим игроков
        foreach (var p in activePlayers) // В КТ
        {
            p.ChangeTeam(CsTeam.CounterTerrorist);
        }
        // В Т
        foreach (var p in seekers) // В КТ
        {
            p.ChangeTeam(CsTeam.Terrorist);
        }

        return HookResult.Continue;
    }

    public void ClearAll()
    {
        playersInRow.Clear();
        seekers.Clear();
    }


}
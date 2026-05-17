using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Data;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs;
using KodakkuAssist.Module.Script.Type;

namespace UsamisScript.EndWalker.p12s;

[ScriptType(name: "P12S [Anabaseios The Savage 4]", territorys: [1154], guid: "563bd710-59b8-46de-bbac-f1527d7c0803", version: "0.0.0.11", author: "Usami", note: noteStr, updateInfo: UpdateInfo)]

public class p12s
{
    const string noteStr =
    """
    Please check and configure the "User Settings" section as needed.
    Door boss up to Superchain, main boss up to Pangenesis.
    Duckism.
    """;
    
    private const string UpdateInfo =
        """
        1. Adapted to Kodakku 0.5.x.x
        """;

    [UserSetting("Debug Mode, turn off for non-development use")]
    public static bool DebugMode { get; set; } = false;

    public enum PD1StrategyEnum
    {
        Regular,
        Invuln,
        InvulnEx,
    }

    [UserSetting("Paradeigma 1 Strategy")]
    public PD1StrategyEnum PD1Strategy { get; set; } = PD1StrategyEnum.Regular;

    [UserSetting("Position hint circle drawing - Normal color")]
    public static ScriptColor posColorNormal { get; set; } = new ScriptColor { V4 = new Vector4(1.0f, 1.0f, 1.0f, 1.0f) };

    [UserSetting("Position hint circle drawing - Player position color")]
    public static ScriptColor posColorPlayer { get; set; } = new ScriptColor { V4 = new Vector4(0.0f, 1.0f, 1.0f, 1.0f) };

    [UserSetting("Exaflare (Cosmoflare) Explosion Zone Color")]
    public ScriptColor exflareColor { get; set; } = new ScriptColor { V4 = new Vector4(0f, 1.0f, 1.0f, 1.0f) };

    [UserSetting("Exaflare (Cosmoflare) Warning Zone Color")]
    public ScriptColor exflareWarnColor { get; set; } = new ScriptColor { V4 = new Vector4(0f, 0.3f, 0.6f, 1.0f) };
    public enum P12S_Phase
    {
        Init,           // Initial
        Paradeigma_I,   // Paradigm 1
        Paradeigma_II,  // Paradigm 2
        Paradeigma_III, // Paradigm 3
        SuperChain_I,   // Superchain 1
        Gaia_I,         // Gaia 1
        Classic_I,      // Classic 1
        Caloric_I,      // Caloric 1
        Exflare,        // Exaflare
        Pangenesis,     // Pangenesis
        Classic_II,     // Classic 2
        Caloric_II,     // Caloric 2
        Gaia_II,        // Gaia 2
    }

    // Race condition locks
    readonly object db_PD1_lockObject = new object();
    readonly object db_PD2_lockObject = new object();
    readonly object db_SC1_lockObject = new object();
    P12S_Phase phase = P12S_Phase.Init;
    List<bool> db_isLeftCleave = [false, false, false];     // Door Boss left/right cleave record
    bool db_PD1_isChecked = false;      // Whether Paradeigma 1 angel positions have been recorded
    bool db_PD1_isNorthFirst = false;   // Whether the Paradeigma 1 angel spawns first in the north
    bool db_PD1_drawn = false;          // Whether Paradeigma 1 safe zones have been drawn
    bool db_PD2_fromRightBottom = false;    // Paradeigma 2, determine start from bottom-left or bottom-right based on north angel offset
    List<bool> db_PD2_shouldWhiteTower = [false, false, false, false];  // Paradeigma 2, determine whether white or black tower is needed based on tether color
    int db_PD2_towerRecordNum = 0;      // Paradeigma 2 recorded tower buff count
    List<bool> db_PD2_isChosenTower = [false, false, false, false, false, false, false, false]; // Paradeigma 2, whether the player is marked to place a tower
    List<bool> db_PD2_isWhiteTower = [false, false, false, false, false, false, false, false];  // Paradeigma 2, whether the player is marked to place a white tower
    bool db_PD2_drawn = false;  // Paradeigma 2, whether tower placement drawing is done
    List<uint> db_SC1_theories = [];    // Superchain 1 element collection
    bool db_SC1_round1_drawn = false;   // Whether Superchain 1 first round drawing is done
    bool db_SC1_isOut = false;          // Whether Superchain 1 first round is chariot (outer)
    bool db_SC1_isSpread = false;       // Whether Superchain 1 first round is spread
    int db_SC1_myBuff = -1;             // My buff for Superchain 1
    static List<int> db_SC1_BWTBidx = [-1, -1, -1, -1];    // Black White Tower Beam index, four elements: black tower, white tower, black stack, white stack
    bool db_SC1_round2_drawn = false;   // Whether Superchain 1 second round drawing is done
    bool db_SC1_round3_drawn = false;   // Whether Superchain 1 third round drawing is done
    List<bool> mb_Gaia1_dangerPlace = [false, false, false, false, false, false, false, false]; // Gaia 1 safe corners
    bool mb_Gaia1_dangerPlace_hasDrawn = false; // Whether Gaia 1 safe corners have been drawn
    List<int> mb_Classic1_playerGroup = [0, 0, 0, 0, 0, 0, 0, 0];   // Classic 1 player groups
    List<ClassicElement> mb_Classic1_elements = new List<ClassicElement>(); // Classic 1 elements
    bool mb_Classic1_etDrawn = false;  // Whether Classic 1 element targeting drawing is done
    bool mb_Classic1_implodeDrawn = false;   // Whether Classic 1 implosion drawing is done
    bool mb_Classic1_RayDirDrawn = false;   // Whether Classic 1 ray guidance drawing is done
    int mb_Caloric_phase = 0; // Caloric phase
    List<bool> mb_Caloric_isFirstTarget = [false, false, false, false, false, false, false, false]; // Caloric initial stack targets
    List<bool> mb_Caloric_isWind = [false, false, false, false, false, false, false, false]; // Caloric buffs (wind)
    List<int> mb_Caloric_WindPriority = [0, 0, 0, 0, 0, 0, 0, 0]; // Caloric wind priority
    List<int> mb_Caloric_FirePriority = [0, 0, 0, 0, 0, 0, 0, 0]; // Caloric fire priority
    bool mb_Caloric_ParnterStackDirDrawn = false;   // Whether Caloric 4-group stack guidance is drawn
    bool mb_Caloric_ParnterStackDrawn = false;      // Whether Caloric 4-group stack range is drawn
    bool mb_Caloric_SecondParnterStackDirDrawn = false; // Whether Caloric secondary stack guidance is drawn
    bool mb_Caloric_SecondParnterStackDrawn = false; // Whether Caloric secondary stack range is drawn
    bool mb_Caloric_SecondWindDonutDrawn = false;    // Whether Caloric ring wind drawing is done
    List<bool> mb_Exflare_FlarePos = [false, false, false, false]; // Exaflare flare zones
    bool mb_Exflare_DirDrawn = false;   // Whether Exaflare guidance is drawn
    int mb_Pangenesis_towerNum = 0; // Pangenesis tower count
    List<bool> mb_Pangenesis_phase = [false, false, false, false, false, false, false, false];  // Pangenesis phase stage processing completion
    List<bool> mb_Pangenesis_isLong = [false, false, false, false, false, false, false, false];    // Pangenesis, long buff
    List<bool> mb_Pangenesis_isWhite = [false, false, false, false, false, false, false, false];   // Pangenesis, white buff
    List<bool> mb_Pangenesis_hasFactor = [false, false, false, false, false, false, false, false]; // Pangenesis, has combination factor
    List<bool> mb_Pangenesis_isTwo = [false, false, false, false, false, false, false, false];     // Pangenesis, has two layers of factor
    List<bool> mb_Pangenesis_shouldWhiteTower = [false, false, false, false, false, false, false, false];   // Pangenesis, should stand in white tower
    List<int> mb_Pangenesis_TowerIdxOrder = [-1, -1, -1, -1, -1, -1, -1, -1];   // Pangenesis, initial arrangement: left tower, left tower, top tower, bottom tower, right tower, right tower, top tower, bottom tower
    List<int> mb_Pangenesis_TowerIdxSecondOrder = [-1, -1, -1, -1, -1, -1, -1, -1];   // Pangenesis, second arrangement: top-left, top-left, bottom-left, bottom-left, top-right, top-right, bottom-right, bottom-right
    public void Init(ScriptAccessory accessory)
    {
        phase = P12S_Phase.Init;

        db_isLeftCleave = [false, false, false];     // Door Boss left/right cleave record
        db_PD1_isChecked = false;      // Whether Paradeigma 1 angel positions have been recorded
        db_PD1_isNorthFirst = false;   // Whether the Paradeigma 1 angel spawns first in the north
        db_PD1_drawn = false;          // Whether Paradeigma 1 safe zones have been drawn

        db_PD2_fromRightBottom = false;    // Paradeigma 2, determine start from bottom-left or bottom-right based on north angel offset
        db_PD2_shouldWhiteTower = [false, false, false, false];  // Paradeigma 2, determine whether white or black tower is needed based on tether color
        db_PD2_towerRecordNum = 0;      // Paradeigma 2 recorded tower buff count
        db_PD2_isChosenTower = [false, false, false, false, false, false, false, false]; // Paradeigma 2, whether the player is marked to place a tower
        db_PD2_isWhiteTower = [false, false, false, false, false, false, false, false];  // Paradeigma 2, whether the player is marked to place a white tower
        db_PD2_drawn = false;           // Paradeigma 2, whether tower placement drawing is done

        db_SC1_theories = [];           // Superchain 1 element collection
        db_SC1_round1_drawn = false;     // Whether Superchain 1 first round drawing is done
        db_SC1_isOut = false;           // Whether Superchain 1 first round is chariot (outer)
        db_SC1_isSpread = false;        // Whether Superchain 1 first round is spread

        db_SC1_myBuff = -1;             // My buff for Superchain 1
        db_SC1_BWTBidx = [-1, -1, -1, -1];  // Black White Tower Beam index, four elements: black tower, white tower, black stack, white stack

        db_SC1_round2_drawn = false;    // Whether Superchain 1 second round drawing is done
        db_SC1_round3_drawn = false;    // Whether Superchain 1 third round drawing is done

        mb_Gaia1_dangerPlace = [false, false, false, false, false, false, false, false]; // Gaia 1 safe corners
        mb_Gaia1_dangerPlace_hasDrawn = false;  // Whether Gaia 1 safe corners have been drawn

        mb_Classic1_playerGroup = [0, 0, 0, 0, 0, 0, 0, 0];   // Classic 1 player groups
        mb_Classic1_elements = new List<ClassicElement>(); // Classic 1 elements
        mb_Classic1_etDrawn = false;  // Whether Classic 1 element targeting drawing is done
        mb_Classic1_implodeDrawn = false;   // Whether Classic 1 implosion drawing is done
        mb_Classic1_RayDirDrawn = false;   // Whether Classic 1 ray guidance drawing is done

        mb_Caloric_phase = 0; // Caloric phase
        mb_Caloric_isFirstTarget = [false, false, false, false, false, false, false, false]; // Caloric initial stack targets
        mb_Caloric_isWind = [false, false, false, false, false, false, false, false]; // Caloric buffs (wind)
        mb_Caloric_WindPriority = [0, 0, 0, 0, 0, 0, 0, 0]; // Caloric wind priority
        mb_Caloric_FirePriority = [0, 0, 0, 0, 0, 0, 0, 0]; // Caloric fire priority
        mb_Caloric_ParnterStackDirDrawn = false;   // Whether Caloric 4-group stack guidance is drawn
        mb_Caloric_ParnterStackDrawn = false;      // Whether Caloric 4-group stack range is drawn
        mb_Caloric_SecondParnterStackDirDrawn = false; // Whether Caloric secondary stack guidance is drawn
        mb_Caloric_SecondParnterStackDrawn = false; // Whether Caloric secondary stack range is drawn
        mb_Caloric_SecondWindDonutDrawn = false;    // Whether Caloric ring wind drawing is done

        mb_Exflare_FlarePos = [false, false, false, false]; // Exaflare flare zones
        mb_Exflare_DirDrawn = false;   // Whether Exaflare guidance is drawn

        mb_Pangenesis_towerNum = 0; // Pangenesis tower count
        mb_Pangenesis_phase = [false, false, false, false, false, false, false, false];  // Pangenesis phase stage processing completion
        mb_Pangenesis_isLong = [false, false, false, false, false, false, false, false];    // Pangenesis, long buff
        mb_Pangenesis_isWhite = [false, false, false, false, false, false, false, false];   // Pangenesis, white buff
        mb_Pangenesis_hasFactor = [false, false, false, false, false, false, false, false]; // Pangenesis, has combination factor
        mb_Pangenesis_isTwo = [false, false, false, false, false, false, false, false];     // Pangenesis, has two layers of factor
        mb_Pangenesis_shouldWhiteTower = [false, false, false, false, false, false, false, false];   // Pangenesis, should stand in white tower
        mb_Pangenesis_TowerIdxOrder = [-1, -1, -1, -1, -1, -1, -1, -1];   // Pangenesis, initial arrangement: left tower, left tower, top tower, bottom tower, right tower, right tower, top tower, bottom tower
        mb_Pangenesis_TowerIdxSecondOrder = [-1, -1, -1, -1, -1, -1, -1, -1];   // Pangenesis, second arrangement: top-left, top-left, bottom-left, bottom-left, top-right, top-right, bottom-right, bottom-right


        // DebugMsg($"/e Init Success.", accessory);
        accessory.Method.MarkClear();
        accessory.Method.RemoveDraw(".*");
    }

    public static void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!DebugMode) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    [ScriptMethod(name: "General Debug Use", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:=TST"], userControl: false)]
    public void EchoDebug(Event @event, ScriptAccessory accessory)
    {
        if (!DebugMode) return;
        var msg = @event["Message"].ToString();
        DebugMsg($"Received player message: {msg}", accessory);

        DebugMsg($"hello", accessory);

        string hasFactorStr = string.Join(", ", mb_Pangenesis_hasFactor);
        DebugMsg($"hasFactorStr: {hasFactorStr}", accessory);

        string isLongStr = string.Join(", ", mb_Pangenesis_isLong);
        DebugMsg($"isLongStr: {isLongStr}", accessory);

        string isWhiteStr = string.Join(", ", mb_Pangenesis_isWhite);
        DebugMsg($"isWhiteStr: {isWhiteStr}", accessory);

        string isTwoStr = string.Join(", ", mb_Pangenesis_isTwo);
        DebugMsg($"isTwoStr: {isTwoStr}", accessory);

        string shouldWhiteStr = string.Join(", ", mb_Pangenesis_shouldWhiteTower);
        DebugMsg($"shouldWhiteStr: {shouldWhiteStr}", accessory);

        string TowerIdxOrderStr = string.Join(", ", mb_Pangenesis_TowerIdxOrder);
        DebugMsg($"TowerIdxOrderStr: {TowerIdxOrderStr}", accessory);

        string TowerIdxSecondOrderStr = string.Join(", ", mb_Pangenesis_TowerIdxSecondOrder);
        DebugMsg($"TowerIdxSecondOrderStr: {TowerIdxSecondOrderStr}", accessory);
    }

    [ScriptMethod(name: "Remove Drawing", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:=RMV"], userControl: false)]
    public void RemoveDraw(Event @event, ScriptAccessory accessory)
    {
        if (!DebugMode) return;
        accessory.Method.RemoveDraw(".*");
    }

    #region Door Boss: Left/Right Cleave

    [ScriptMethod(name: "Door Boss: Left/Right Cleave Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["Param:regex:^(19|20|21|22|23|24)$"], userControl: false)]
    public void DB_SideCleaveRecord(Event @event, ScriptAccessory accessory)
    {
        var param = @event.Param();
        var paramMapping = new Dictionary<uint, (int index, bool value, string wing)>
        {
            // { key, (index, value, wing) }
            { 19, (0, true, "Top-Left Wing") },
            { 20, (0, false, "Top-Right Wing") },
            { 21, (1, true, "Middle-Left Wing") },
            { 22, (1, false, "Middle-Right Wing") },
            { 23, (2, true, "Bottom-Left Wing") },
            { 24, (2, false, "Bottom-Right Wing") }
        };
        if (paramMapping.ContainsKey(param))
        {
            var (index, value, wing) = paramMapping[param];
            db_isLeftCleave[index] = value;
            DebugMsg($"【Door Boss: Left/Right Cleave Record】Detected {wing}", accessory);
        }
    }

    // Top-Left Wing 19  82E2 Cast 33506
    // Top-Right Wing 20  82E1 Cast 33505
    // Middle-Left Wing 21  
    // Middle-Right Wing 22
    // Bottom-Left Wing 23  82E8 Cast 33512
    // Bottom-Right Wing 24  82E7 Cast 33511
    // Trinity of Souls
    [ScriptMethod(name: "Door Boss: Left/Right Cleave Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(33506|33505|33512|33511)$"])]
    public void DB_SideCleaveDraw(Event @event, ScriptAccessory accessory)
    {
        var spos = @event.SourcePosition();
        var srot = @event.SourceRotation();
        var isTopWingFirst = @event.ActionId() == 33506 || @event.ActionId() == 33505;
        List<bool> sideCleaveLeft = new List<bool>(db_isLeftCleave);

        // If top wings activate first, order unchanged; otherwise, middle wing left cleave becomes right cleave
        sideCleaveLeft[1] = isTopWingFirst ? sideCleaveLeft[1] : !sideCleaveLeft[1];
        string action1_str = sideCleaveLeft[0] == sideCleaveLeft[1] ? "Stop" : "Cross";
        string action2_str = sideCleaveLeft[1] == sideCleaveLeft[2] ? "Stop" : "Cross";
        // If top wings activate first, do [0] vs [1] first, then [1] vs [2] later
        string action_str = isTopWingFirst ? $"First 【{action1_str}】 then 【{action2_str}】" : $"First 【{action2_str}】 then 【{action1_str}】";

        DebugMsg($"【Door Boss: Left/Right Cleave Drawing】Avoidance solution: {action_str}", accessory);

        if (isTopWingFirst)
        {
            drawSideCleave(sideCleaveLeft[0], 0, 10000, spos, srot, accessory);
            drawSideCleave(sideCleaveLeft[1], 10000, 2600, spos, srot, accessory);
            drawSideCleave(sideCleaveLeft[2], 12600, 2600, spos, srot, accessory);
        }
        else
        {
            drawSideCleave(sideCleaveLeft[0], 12600, 2600, spos, srot, accessory);
            drawSideCleave(sideCleaveLeft[1], 10000, 2600, spos, srot, accessory);
            drawSideCleave(sideCleaveLeft[2], 0, 10000, spos, srot, accessory);
        }

        accessory.Method.TextInfo(action_str, 17000, true);
    }

    public static void drawSideCleave(bool isLeft, int delay, int destoryAt, Vector3 spos, float srot, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Left/Right Cleave";
        dp.Scale = new(50);
        dp.Position = spos;
        // dp.Rotation is counter-clockwise, here it rotates 90 degrees counter-clockwise
        dp.Rotation = isLeft ? srot + float.Pi / 2 : srot + float.Pi / -2;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    #endregion

    #region Door Boss: Tank Buster & Dialogue

    [ScriptMethod(name: "Door Boss: Tank Buster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33532"])]
    public void DB_TankBuster(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Top Enmity Line Tank Buster-1";
        dp.Scale = new(5, 40);
        dp.Owner = @event.SourceId();
        // Target is determined when the cast bar starts
        dp.TargetObject = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = 0;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Top Enmity Line Tank Buster-2";
        dp.Scale = new(5, 40);
        dp.Owner = @event.SourceId();
        // Top enmity can change during the cast
        dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Door Boss: Dialogue Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3353[45])$"])]
    public void DB_Dialogos(Event @event, ScriptAccessory accessory)
    {

        var sid = @event.SourceId();
        int MyIndex = IndexHelper.getMyIndex(accessory);

        string action_str;
        switch (MyIndex)
        {
            case 0:
                action_str = "【Outside target ring】 Bait far";
                break;
            case 1:
                action_str = "【Middle】 Bait near";
                break;
            default:
                action_str = "【Between target rings】 Avoid near/far";
                break;
        }
        accessory.Method.TextInfo(action_str, 6200, true);

        var isMT = MyIndex == 0;
        var isST = MyIndex == 1;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Dialogue - Near";
        dp.Owner = sid;
        dp.Color = isST ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
        dp.CentreOrderIndex = 1u;
        dp.Delay = 0;
        dp.DestoryAt = 5200;
        dp.Scale = new(6);
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Dialogue - Far";
        dp.Owner = sid;
        dp.Color = isMT ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
        dp.CentreOrderIndex = 1u;
        dp.Delay = 0;
        dp.DestoryAt = 6200;
        dp.Scale = new(6);
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    #endregion

    #region Door Boss: Paradeigma 1

    [ScriptMethod(name: "Door Boss: Paradigm Phase Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33517"], userControl: false)]
    public void DB_PhaseRecord(Event @event, ScriptAccessory accessory)
    {
        switch (phase)
        {
            case P12S_Phase.Init:
                phase = P12S_Phase.Paradeigma_I;
                break;
            case P12S_Phase.Paradeigma_I:
                phase = P12S_Phase.Paradeigma_II;
                break;
            case P12S_Phase.Paradeigma_II:
                phase = P12S_Phase.Paradeigma_III;
                break;
            default:
                phase = P12S_Phase.Init;
                break;
        }
    }

    [ScriptMethod(name: "Door Boss: Paradeigma 1 Angel Position Record", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:16172"], userControl: false)]
    public void DB_Paradeigma_I_PositionRecord(Event @event, ScriptAccessory accessory)
    {
        lock (db_PD1_lockObject)
        {
            // Check only once, if Z < 100 then it's north
            if (phase != P12S_Phase.Paradeigma_I || db_PD1_isChecked) return;
            var spos = @event.SourcePosition();
            db_PD1_isNorthFirst = spos.Z < 100;
            db_PD1_isChecked = true;
            DebugMsg($"【Door Boss: Paradeigma 1 Angel Position Record】Paradeigma 1 angel is {(db_PD1_isNorthFirst ? "North" : "South")}", accessory);
        }
    }

    [ScriptMethod(name: "Door Boss: Paradeigma 1 Angel Position Drawing", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:16172"])]
    public void DB_Paradeigma_I_Waymark(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(100).ContinueWith(t =>
        {
            lock (db_PD1_lockObject)
            {
                if (db_PD1_drawn) return;
                if (!db_PD1_isChecked) return;

                if (PD1Strategy == PD1StrategyEnum.Regular)
                {
                    int MyIndex = IndexHelper.getMyIndex(accessory);
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    // Even index for first round (MT H1 D1 D3)
                    bool isFirstRound = MyIndex % 2 == 0;
                    int MyPos = MyIndex / 2;

                    if (isFirstRound)
                    {
                        drawPD1Spread(db_PD1_isNorthFirst, MyPos, 1000, 10000, accessory);
                        drawPD1Safe(db_PD1_isNorthFirst, 11000, 5000, accessory);
                    }
                    else
                    {
                        drawPD1Safe(db_PD1_isNorthFirst, 1000, 10000, accessory);
                        drawPD1Spread(db_PD1_isNorthFirst, MyPos, 11000, 5000, accessory);
                    }
                }

                else if (PD1Strategy == PD1StrategyEnum.Invuln)
                {
                    int MyIndex = IndexHelper.getMyIndex(accessory);
                    var dp = accessory.Data.GetDefaultDrawProperties();

                    switch (MyIndex)
                    {
                        case 0:
                        case 1:
                            // Move close and use invuln
                            drawPD1Spread(db_PD1_isNorthFirst, 0, 1000, 15000, accessory);
                            break;
                        default:
                            // Stay with the group
                            drawPD1Safe(db_PD1_isNorthFirst, 1000, 15000, accessory);
                            break;
                    }
                }

                else if (PD1Strategy == PD1StrategyEnum.InvulnEx)
                {
                    int MyIndex = IndexHelper.getMyIndex(accessory);
                    var dp = accessory.Data.GetDefaultDrawProperties();

                    switch (MyIndex)
                    {
                        case 0:
                            // MT moves far first, then close and uses invuln
                            drawPD1Spread(db_PD1_isNorthFirst, 2, 1000, 10000, accessory);
                            drawPD1Spread(db_PD1_isNorthFirst, 0, 11000, 5000, accessory);
                            break;
                        case 1:
                            // ST moves close and uses invuln
                            drawPD1Spread(db_PD1_isNorthFirst, 0, 1000, 15000, accessory);
                            break;
                        case 6:
                            // D3 moves close first, then returns to group
                            drawPD1Spread(db_PD1_isNorthFirst, 1, 1000, 10000, accessory);
                            drawPD1Safe(db_PD1_isNorthFirst, 11000, 5000, accessory);
                            break;
                        default:
                            drawPD1Safe(db_PD1_isNorthFirst, 1000, 15000, accessory);
                            break;
                    }
                }

                db_PD1_drawn = true;
            }
        });
    }

    public static void drawPD1Spread(bool isNorthSpread, int MyPos, int delay, int destoryAt, ScriptAccessory accessory)
    {
        Vector3[,] pos = new Vector3[2, 5];
        pos[0, 0] = new(100, 0, 95);    // Paradeigma 1 bait position: North 2 / MT ST
        pos[0, 1] = new(100, 0, 100);   // Paradeigma 1 bait position: North 1 / H1 H2
        pos[0, 2] = new(100, 0, 90);    // Paradeigma 1 bait position: North 3 / D1 D2
        pos[0, 3] = new(100, 0, 85);    // Paradeigma 1 bait position: North 4 / D3 D4
        pos[0, 4] = new(100, 0, 109);   // Paradeigma 1 gather position: South

        pos[1, 0] = new(100, 0, 105);   // Paradeigma 1 bait position: South 2 / MT ST
        pos[1, 1] = new(100, 0, 100);   // Paradeigma 1 bait position: South 1 / H1 H2
        pos[1, 2] = new(100, 0, 110);   // Paradeigma 1 bait position: South 3 / D1 D2
        pos[1, 3] = new(100, 0, 115);   // Paradeigma 1 bait position: South 4 / D3 D4
        pos[1, 4] = new(100, 0, 91);    // Paradeigma 1 gather position: North

        var dp = accessory.Data.GetDefaultDrawProperties();
        for (int i = 0; i < 4; i++)
        {
            dp.Name = $"Paradeigma 1 Bait Position-{i}";
            dp.Scale = new(1.5f);
            // If north bait, draw 4 north positions
            dp.Position = pos[isNorthSpread ? 0 : 1, i];
            dp.Color = (i == MyPos) ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
            dp.Delay = delay;
            dp.DestoryAt = destoryAt;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Paradeigma 1 Spread Guidance";
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = pos[isNorthSpread ? 0 : 1, MyPos];
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Scale = new(1f);
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    public static void drawPD1Safe(bool isNorthSpread, int delay, int destoryAt, ScriptAccessory accessory)
    {
        Vector3[,] pos = new Vector3[2, 5];
        pos[0, 0] = new(100, 0, 95);    // Paradeigma 1 bait position: North 2 / MT ST
        pos[0, 1] = new(100, 0, 100);   // Paradeigma 1 bait position: North 1 / H1 H2
        pos[0, 2] = new(100, 0, 90);    // Paradeigma 1 bait position: North 3 / D1 D2
        pos[0, 3] = new(100, 0, 85);    // Paradeigma 1 bait position: North 4 / D3 D4
        pos[0, 4] = new(100, 0, 109);   // Paradeigma 1 gather position: South

        pos[1, 0] = new(100, 0, 105);   // Paradeigma 1 bait position: South 2 / MT ST
        pos[1, 1] = new(100, 0, 100);   // Paradeigma 1 bait position: South 1 / H1 H2
        pos[1, 2] = new(100, 0, 110);   // Paradeigma 1 bait position: South 3 / D1 D2
        pos[1, 3] = new(100, 0, 115);   // Paradeigma 1 bait position: South 4 / D3 D4
        pos[1, 4] = new(100, 0, 91);    // Paradeigma 1 gather position: North

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Paradeigma 1 Safe Position";
        dp.Scale = new(1.5f);
        // If north bait, safe at south
        dp.Position = pos[isNorthSpread ? 0 : 1, 4];
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Paradeigma 1 Safe Position Guidance";
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = pos[isNorthSpread ? 0 : 1, 4];
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Scale = new(1f);
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    #endregion
    #region Door Boss: Paradeigma 2

    [ScriptMethod(name: "Door Boss: Paradeigma 2 Add Tether Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3352[12])$"], userControl: false)]
    public void DB_Paradeigma_II_LineRecord(Event @event, ScriptAccessory accessory)
    {
        // If black tether (33522), white tower is needed
        bool shouldWhiteTower = @event.ActionId() == 33522;
        Vector3 spos = @event.SourcePosition();
        string log = "";

        // Get north angel first
        // North 0, East 1, South 2, West 3
        if (spos.Z < 80)
        {
            db_PD2_shouldWhiteTower[0] = shouldWhiteTower;
            log += $"North angel {(shouldWhiteTower ? "Black" : "White")} tether";

            if (spos.X > 100)
                db_PD2_fromRightBottom = true;  // If north angel is offset to the right, tower placement starts from bottom-right

            log += $"{(db_PD2_fromRightBottom ? "Offset Right" : "Offset Left")}.";
        }
        else
        {
            int index = (spos.X > 120) ? 1 : (spos.Z > 120) ? 2 : 3;
            db_PD2_shouldWhiteTower[index] = shouldWhiteTower;
            log += $"{(index == 1 ? "East" : index == 2 ? "South" : "West")} angel {(shouldWhiteTower ? "Black" : "White")} tether.";
        }

        DebugMsg($"【Door Boss: Paradeigma 2 Add Tether Record】{log}", accessory);
    }

    [ScriptMethod(name: "Door Boss: Paradeigma 2 Black/White Tower Marker Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3579|3580)$"], userControl: false)]
    public void DB_Paradeigma_II_TowerRecord(Event @event, ScriptAccessory accessory)
    {
        lock (db_PD2_lockObject)
        {
            var tid = @event.TargetId();
            var targetIndex = IndexHelper.getPlayerIdIndex(tid, accessory);
            if (targetIndex == -1) return;

            // Being selected means needing to place a tower, 3579 is white tower
            db_PD2_isChosenTower[targetIndex] = true;
            db_PD2_isWhiteTower[targetIndex] = @event.StatusID() == 3579;
            db_PD2_towerRecordNum++;
        }
    }

    [ScriptMethod(name: "Door Boss: Add Tether Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3352[12])$"])]
    public void DB_Paradeigma_II_LineDraw(Event @event, ScriptAccessory accessory)
    {
        var tid = @event.TargetId();
        var sid = @event.SourceId();
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Add Tether";
        dp.Scale = new(5);
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Owner = sid;
        dp.TargetObject = tid;
        dp.Color = tid == accessory.Data.Me ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        dp.Delay = 0;
        dp.DestoryAt = 8700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Door Boss: Add Shockwave Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33518"])]
    public void DB_Paradeigma_II_ShootDraw(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Add Tether";
        dp.Scale = new(10, 60);
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = 0;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Door Boss: Paradeigma 2 Black/White Tower Drawing", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3579|3580)$"])]
    public void DB_Paradeigma_II_TowerDraw(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(100).ContinueWith(t =>
        {
            lock (db_PD2_lockObject)
            {
                if (db_PD2_drawn) return;

                if (db_PD2_towerRecordNum != 4) return;
                db_PD2_drawn = true;

                int MyIndex = IndexHelper.getMyIndex(accessory);
                if (MyIndex == -1 || !db_PD2_isChosenTower[MyIndex]) return;

                var tposIndex = MyIndex < 4
                    ? db_PD2_shouldWhiteTower.IndexOf(db_PD2_isWhiteTower[MyIndex])
                    : db_PD2_shouldWhiteTower.LastIndexOf(db_PD2_isWhiteTower[MyIndex]);

                DebugMsg($"【Door Boss: Paradeigma 2 Black/White Tower Drawing】Need to guide the tower to angel {tposIndex}", accessory);

                if (!db_PD2_fromRightBottom) tposIndex++;

                Vector3[] tpos = new Vector3[5];
                tpos[0] = new(110, 0, 110);
                tpos[1] = new(90, 0, 110);
                tpos[2] = new(90, 0, 90);
                tpos[3] = new(110, 0, 90);
                tpos[4] = new(110, 0, 110);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Black/White Tower Placement";
                dp.Scale = new(1);
                dp.Color = accessory.Data.DefaultSafeColor.WithW(3f);
                dp.Delay = 0;
                dp.DestoryAt = 11000;
                dp.Position = tpos[tposIndex];
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Scale = new(1f);
                dp.Name = $"Black/White Tower Placement Guidance";
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = tpos[tposIndex];
                dp.ScaleMode = ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 0;
                dp.DestoryAt = 11000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        });
    }
    #endregion




    #region Door Boss: Superchain I (Step 1, Chariot/Donut Stack/Spread)

    [ScriptMethod(name: "Door Boss: Enter Superchain I Phase", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33498"], userControl: false)]
    public void DB_SuperChain_I_PhaseRecord(Event @event, ScriptAccessory accessory)
    {
        phase = P12S_Phase.SuperChain_I;
    }

    [ScriptMethod(name: "Door Boss: Superchain I Element Collection", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(16176|16177|16178|16179|16180)$"], userControl: false)]
    public void DB_SuperChain_I_TheoryCollect(Event @event, ScriptAccessory accessory)
    {
        lock (db_SC1_lockObject)
        {
            if (phase != P12S_Phase.SuperChain_I) return;
            db_SC1_theories.Add(@event.SourceId());
            DebugMsg($"Detected new Superchain element, current list contains {db_SC1_theories.Count()}", accessory);
        }
    }

    [ScriptMethod(name: "Door Boss: Superchain I First Group Drawing", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(16176|16177|16178|16179|16180)$"])]
    public void DB_SuperChain_I_FirstRound(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(100).ContinueWith(t =>
        {
            lock (db_SC1_lockObject)
            {
                if (db_SC1_round1_drawn) return;
                if (phase != P12S_Phase.SuperChain_I) return;
                if (db_SC1_theories.Count() != 3) return;
                db_SC1_round1_drawn = true;
                DebugMsg($"Entering Superchain I First Group Drawing.", accessory);

                IGameObject? destTheory = null;    // Target point, Superchain element

                for (int i = 0; i < 3; i++)
                {
                    var theoryObject = accessory.GetById(db_SC1_theories[i]);
                    if (theoryObject == null) return;
                    switch (theoryObject.DataId)
                    {
                        case 16176:
                            destTheory = theoryObject;
                            break;
                        case 16177:
                            db_SC1_isOut = true;
                            break;
                        case 16178:
                            db_SC1_isOut = false;
                            break;
                        case 16179:
                            db_SC1_isSpread = true;
                            break;
                        case 16180:
                            db_SC1_isSpread = false;
                            break;
                    }
                }
                if (destTheory == null) return;

                // Draw Chariot/Donut range
                drawCircleDonutAtPos(destTheory.Position, 0, 11000, db_SC1_isOut, accessory);
                // Draw Spread/Stack fan range
                drawSpreadStackAtPos(destTheory.Position, 7000, 4000, db_SC1_isSpread, accessory);
                // Draw Spread/Stack positions
                drawSpreadStackStdPos(destTheory.Position, 0, 11500, db_SC1_isOut, db_SC1_isSpread, accessory);
            }
        });
    }

    private static void drawCircleDonutAtPos(Vector3 pos, int delay, int destoryAt, bool isCircle, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Superchain Element";
        dp.Position = pos;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        dp.Color = accessory.Data.DefaultDangerColor;
        if (isCircle)
        {
            dp.Scale = new(7);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        else
        {
            dp.Scale = new(30);
            dp.InnerScale = new(6);
            dp.Radian = float.Pi * 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
    }

    private static void drawSpreadStackAtPos(Vector3 pos, int delay, int destoryAt, bool isSpread, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var MyIndex = IndexHelper.getMyIndex(accessory);
        for (int i = 0; i < (isSpread ? 8 : 4); i++)
        {
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Four/Eight Parts{i}";
            dp.Position = pos;
            dp.TargetObject = accessory.Data.PartyList[i];
            dp.Color = (i == MyIndex || i == MyIndex - 4) ? accessory.Data.DefaultSafeColor.WithW(0.5f) : accessory.Data.DefaultDangerColor.WithW(0.5f);
            dp.Delay = delay;
            dp.DestoryAt = destoryAt;
            dp.Scale = new Vector2(40);
            dp.Radian = isSpread ? float.Pi / 180 * 30 : float.Pi / 180 * 35;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
    }

    private static void drawSpreadStackStdPos(Vector3 pos, int delay, int destoryAt, bool isCircle, bool isSpread, ScriptAccessory accessory)
    {
        int MyIndex = IndexHelper.getMyIndex(accessory);
        float deg_init = DirectionCalc.FindRadian(pos, new Vector3(100, 0, 100));
        float deg;
        List<int> safePoint = isSpread ? [6, 1, 5, 2, 7, 0, 4, 3] : [3, 0, 2, 1, 3, 0, 2, 1];
        var dp = accessory.Data.GetDefaultDrawProperties();
        for (int i = 0; i < (isSpread ? 8 : 4); i++)
        {
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Position{i}";
            dp.Scale = new(1f);
            deg = isSpread ? deg_init + float.Pi / 4 * i + float.Pi / 8 : deg_init + float.Pi / 2 * i + float.Pi / 4;
            dp.Position = DirectionCalc.ExtendPoint(pos, deg, isCircle ? 8 : 5);
            dp.Color = safePoint[MyIndex] == i ? posColorPlayer.V4.WithW(3f) : posColorNormal.V4;
            dp.Delay = delay;
            dp.DestoryAt = destoryAt;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    #endregion

    #region Door Boss: Superchain I (Step 2, Black/White Left/Right Stack)

    [ScriptMethod(name: "Door Boss: Superchain I Buff Collection", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3576|3577|3579|3580|3581|3582)$"], userControl: false)]
    public void DB_SuperChain_I_BuffRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.SuperChain_I) return;
        if (db_SC1_myBuff != -1 && !db_SC1_BWTBidx.Contains(-1)) return;
        var tid = @event.TargetId();
        var sid = @event.StatusID();
        var sidMapping = new Dictionary<uint, (int value, string mention, int lidx)>
        {
            { 3581, (0, "White Stack", 3) },
            { 3582, (1, "Black Stack", 2) },
            { 3579, (2, "White Tower", 1) },
            { 3580, (3, "Black Tower", 0) },
            // { 3578, (4, "Spread", -1) },
            { 3576, (5, "Initial White", -1) },
            { 3577, (6, "Initial Black", -1) }
        };
        if (sidMapping.ContainsKey(sid))
        {
            var (value, mention, lidx) = sidMapping[sid];
            if (lidx != -1)
                db_SC1_BWTBidx[lidx] = IndexHelper.getPlayerIdIndex(tid, accessory);
            if (tid == accessory.Data.Me)
            {
                db_SC1_myBuff = value;
                DebugMsg($"Detected self buff as {mention}", accessory);
            }
        }
    }

    [ScriptMethod(name: "Door Boss: Superchain I Second Group Drawing", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(16176|16177|16178|16179|16180)$"])]
    public void DB_SuperChain_I_SecondRound(Event @event, ScriptAccessory accessory)
    {
        IGameObject? destDonutChar = null;
        Task.Delay(100).ContinueWith(t =>
        {
            lock (db_SC1_lockObject)
            {
                if (db_SC1_theories.Count() != 7) return;
                if (phase != P12S_Phase.SuperChain_I) return;
                if (db_SC1_round2_drawn) return;
                db_SC1_round2_drawn = true;
                DebugMsg($"Entering Superchain I Second Group Drawing.", accessory);

                // Take the last 4 elements, two endpoints, one chariot, one donut
                List<uint> SC1_SubList = db_SC1_theories.GetRange(3, 4);

                IGameObject? dest1Theory = null;
                IGameObject? dest2Theory = null;
                IGameObject? inTheory = null;
                IGameObject? outTheory = null;

                // Determine endpoint positions
                for (int i = 0; i < 4; i++)
                {
                    var theoryObject = accessory.GetById(SC1_SubList[i]);
                    if (theoryObject == null) return;

                    switch (theoryObject.DataId)
                    {
                        case 16176:
                            if (dest1Theory == null)
                                dest1Theory = theoryObject;
                            else
                                dest2Theory = theoryObject;
                            break;
                        case 16177:
                            outTheory = theoryObject;
                            break;
                        case 16178:
                            inTheory = theoryObject;
                            break;
                    }
                }
                destDonutChar = GetDonutChar(dest1Theory, dest2Theory, inTheory, outTheory);
                if (destDonutChar == null) return;
                drawCircleDonutAtPos(destDonutChar.Position, 6500, 7000, false, accessory);
            }
        });

        Task.Delay(3500).ContinueWith(t =>
        {
            bool atLeft = db_SC1_myBuff == 2 || db_SC1_myBuff == 5 || db_SC1_myBuff == 1;
            if (destDonutChar == null) return;
            drawStackStdPos(destDonutChar.Position, 3000, 7000, atLeft, accessory);
        });
    }

    private static IGameObject? GetDonutChar(IGameObject? dest1Theory, IGameObject? dest2Theory, IGameObject? inTheory, IGameObject? outTheory)
    {
        if (dest1Theory == null || dest2Theory == null || inTheory == null || outTheory == null) return null;
        float dest1ToDonut = new Vector2(dest1Theory.Position.X - inTheory.Position.X,
                                        dest1Theory.Position.Z - inTheory.Position.Z).Length();
        float dest2ToDonut = new Vector2(dest2Theory.Position.X - inTheory.Position.X,
                                        dest2Theory.Position.Z - inTheory.Position.Z).Length();
        float dest1ToCircle = new Vector2(dest1Theory.Position.X - outTheory.Position.X,
                                        dest1Theory.Position.Z - outTheory.Position.Z).Length();
        float dest2ToCircle = new Vector2(dest2Theory.Position.X - outTheory.Position.X,
                                        dest2Theory.Position.Z - outTheory.Position.Z).Length();

        if (dest1ToDonut < dest1ToCircle && dest2ToDonut > dest2ToCircle)
            return dest1Theory;
        if (dest1ToDonut > dest1ToCircle && dest2ToDonut < dest2ToCircle)
            return dest2Theory;

        return null;
    }

    private static void drawStackStdPos(Vector3 pos, int delay, int destoryAt, bool atLeft, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Left/Right Stack{atLeft}";
        dp.TargetPosition = DirectionCalc.RotatePoint(pos, new Vector3(100, 0, 100), atLeft ? float.Pi / 180 * 17 : float.Pi / 180 * -17);
        dp.Position = new(100, 0, 100);
        dp.Color = posColorPlayer.V4;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        dp.Scale = new(5, 20);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    #endregion

    #region Door Boss: Superchain I (Step 3, Chariot/Donut, Tower Placement, Spread)

    [ScriptMethod(name: "Door Boss: Superchain I Third Group Drawing", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(16176|16177|16178|16179|16180)$"])]
    public void DB_SuperChain_I_ThirdRound(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(100).ContinueWith(t =>
        {
            lock (db_SC1_lockObject)
            {
                if (db_SC1_theories.Count() != 10) return;
                if (phase != P12S_Phase.SuperChain_I) return;
                // Spawn an endpoint, Chariot/Donut
                if (db_SC1_round3_drawn) return;
                db_SC1_round3_drawn = true;

                if (DebugMode)
                    accessory.Method.SendChat($"/e [DEBUG]: Entering Superchain I Third Group Drawing...");

                List<uint> SC1_SubList = db_SC1_theories.GetRange(7, 3);

                // Determine endpoint position
                IGameObject? destTheory = null;
                IGameObject? inTheory = null;
                IGameObject? outTheory = null;

                for (int i = 0; i < 3; i++)
                {
                    var theoryObject = accessory.GetById(SC1_SubList[i]);
                    if (theoryObject == null) return;

                    switch (theoryObject.DataId)
                    {
                        case 16176:
                            destTheory = theoryObject;
                            break;
                        case 16177:
                            outTheory = theoryObject;
                            break;
                        case 16178:
                            inTheory = theoryObject;
                            break;
                    }
                }

                if (destTheory == null || inTheory == null || outTheory == null) return;

                float destToDonut = new Vector2(destTheory.Position.X - inTheory.Position.X,
                    destTheory.Position.Z - inTheory.Position.Z).Length();
                float destToCircle = new Vector2(destTheory.Position.X - outTheory.Position.X,
                    destTheory.Position.Z - outTheory.Position.Z).Length();

                bool isDonutFirst = destToDonut < destToCircle;

                // Draw Chariot/Donut range
                drawCircleDonutAtPos(destTheory.Position, 10000, 4800, !isDonutFirst, accessory);
                drawCircleDonutAtPos(destTheory.Position, 14800, 2000, isDonutFirst, accessory);

                switch (db_SC1_myBuff)
                {
                    case 0:
                        // White Stack, stand in black right tower
                        // Drawing logic: find the player placing the tower, first danger, then safe
                        drawTowerCircleOnPlayer(db_SC1_BWTBidx[0], false, accessory);
                        break;
                    case 1:
                        // Black Stack, stand in white left tower
                        drawTowerCircleOnPlayer(db_SC1_BWTBidx[1], false, accessory);
                        break;
                    case 2:
                        // White Tower, place tower on the left
                        drawTowerCircleOnPlayer(db_SC1_BWTBidx[1], true, accessory);
                        drawSC1TowerDir(destTheory.Position, 2, accessory);
                        break;
                    case 3:
                        // Black Tower, place tower on the right
                        drawTowerCircleOnPlayer(db_SC1_BWTBidx[0], true, accessory);
                        drawSC1TowerDir(destTheory.Position, 3, accessory);
                        break;
                    case 5:
                    case 6:
                        // Initial White/Black, Spread
                        drawSC1SpreadDir(destTheory.Position, accessory);
                        drawSC1SpreadCircle(accessory);
                        break;
                    default:
                        break;
                }
            }
        });
    }
    private static void drawTowerCircleOnPlayer(int playerIdx, bool isSafe, ScriptAccessory accessory)
    {
        var dp = AssignDp.drawCircle(accessory.Data.PartyList[playerIdx], 14300, 3000, $"Tower Placement Follow{playerIdx}", accessory);
        dp.Scale = new(3);
        dp.Color = isSafe ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    private static void drawSC1SpreadDir(Vector3 dest, ScriptAccessory accessory)
    {
        var angle_toBoss = DirectionCalc.rad2Angle(DirectionCalc.FindRadian(dest, new(100, 0, 100)));
        List<float> angle_spread = new List<float> {
            DirectionCalc.angle2Rad(angle_toBoss - 20),
            DirectionCalc.angle2Rad(angle_toBoss + 20),
            DirectionCalc.angle2Rad(angle_toBoss - 160),
            DirectionCalc.angle2Rad(angle_toBoss + 160),
        };
        var myIndex = IndexHelper.getMyIndex(accessory);

        for (int i = 0; i < 4; i++)
        {
            DebugMsg($"{myIndex} vs {i}", accessory);
            var tpos = DirectionCalc.ExtendPoint(dest, angle_spread[i], 15);
            var dp = AssignDp.dirPos2Pos(dest, tpos, 14800, 5000, $"Spread{i}", accessory);
            dp.Color = ((myIndex == i) || (myIndex == i + 4)) ? posColorPlayer.V4.WithW(3f) : posColorNormal.V4;
            dp.Scale = new(2f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }
    }

    private static void drawSC1TowerDir(Vector3 dest, int myBuff, ScriptAccessory accessory)
    {
        // myBuff 2 White Tower Left
        // myBuff 3 Black Tower Right
        var angle_toBoss = DirectionCalc.rad2Angle(DirectionCalc.FindRadian(dest, new(100, 0, 100)));

        List<float> angle_tower = new List<float> {
            DirectionCalc.angle2Rad(angle_toBoss - 52),
            DirectionCalc.angle2Rad(angle_toBoss + 52),
        };

        var tpos = DirectionCalc.ExtendPoint(dest, angle_tower[myBuff - 2], 12);
        var dp = AssignDp.dirPos2Pos(dest, tpos, 14300, 5000, $"Tower Placement{myBuff}Guidance", accessory);
        dp.Scale = new(2f);
        dp.Color = posColorPlayer.V4.WithW(3f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }

    private static void drawSC1SpreadCircle(ScriptAccessory accessory)
    {
        // Players not in db_SC1_BWTBidx are the ones spreading
        for (int i = 0; i < accessory.Data.PartyList.Count(); i++)
        {
            if (db_SC1_BWTBidx.Contains(i)) continue;
            var dp = AssignDp.drawCircle(accessory.Data.PartyList[i], 15800, 4000, $"Spread{i}", accessory);
            dp.Scale = new(6);
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    [ScriptMethod(name: "Door Boss: Superchain I Tower Placement Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(33549|33548)$"])]
    public void DB_SuperChain_I_TowerDir(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.SuperChain_I) return;
        // 33548 White Tower
        // 33549 Black Tower
        var tpos = @event.TargetPosition();
        var aid = @event.ActionId();

        // I am White Stack, detected Black Tower || I am Black Stack, detected White Tower
        var match = (db_SC1_myBuff == 0 && aid == 33549) || (db_SC1_myBuff == 1 && aid == 33548);

        var dp = AssignDp.drawStatic(tpos, 0, 0, 3000, $"Tower Placement{aid}", accessory);
        dp.Color = match ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        dp.Scale = new(3f);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

        if (match)
        {
            var dp0 = AssignDp.dirPos(tpos, 0, 3000, $"Tower Placement{aid}Guidance", accessory);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);
        }
    }
    #endregion

    #region Main Boss: Gaia 1

    [ScriptMethod(name: "Main Boss: Phase Transition Gaia 1 (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33574"], userControl: false)]
    public void MB_PhaseChange_Gaia(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            P12S_Phase.Classic_I => P12S_Phase.Gaia_II,
            P12S_Phase.Caloric_I => P12S_Phase.Gaia_II,
            P12S_Phase.Exflare => P12S_Phase.Gaia_II,
            P12S_Phase.Pangenesis => P12S_Phase.Gaia_II,
            P12S_Phase.Classic_II => P12S_Phase.Gaia_II,
            P12S_Phase.Caloric_II => P12S_Phase.Gaia_II,
            _ => P12S_Phase.Gaia_I,
        };
        DebugMsg($"Current phase: {phase}", accessory);
    }

    [ScriptMethod(name: "Main Boss: Gaia, Add Laser", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33584"])]
    public void MB_Gaia_BeamRay(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Gaia_I && phase != P12S_Phase.Gaia_II) return;
        var sid = @event.SourceId();
        var sname = @event.SourceName();

        var dp = assignDp_Line(sid, $"Add Laser{sid}", 6, 20, 0, 7000, accessory);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        DebugMsg($"Detected [{sid}|{sname}] casting laser and drawing.", accessory);
    }

    private static DrawPropertiesEdit assignDp_Line(uint sid, string name, int width, int length, int delay, int destory, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(width, length);      // Width 6, Length 20
        dp.Owner = sid;             // Draw from the front of this unit
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destory;
        return dp;
    }

    [ScriptMethod(name: "Main Boss: Gaia, Safe Zone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3357[789])$"])]
    public void MB_Gaia1_SafetyField(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Gaia_I && phase != P12S_Phase.Gaia_II) return;

        // Three types of Geocentric safe forms
        const uint VERTICAL = 33577;
        const uint DONUT = 33578;
        const uint HORIZON = 33579;

        var aid = @event.ActionId();
        var myIndex = IndexHelper.getMyIndex(accessory);

        switch (aid)
        {
            case VERTICAL:
                DebugMsg($"Detected Geocentric vertical safe [{aid}].", accessory);
                drawVerticalSafetyField(accessory);
                drawVerticalSpreadPos(myIndex, accessory);
                break;
            case DONUT:
                DebugMsg($"Detected Geocentric donut safe [{aid}].", accessory);
                drawDonutSafetyField(accessory);
                drawDonutSpreadPos(myIndex, accessory);
                break;
            case HORIZON:
                DebugMsg($"Detected Geocentric horizontal safe [{aid}].", accessory);
                drawHorizonSafetyField(accessory);
                drawHorizonSpreadPos(myIndex, accessory);
                break;
            default:
                break;
        }
    }

    private static DrawPropertiesEdit assignDp_DonutSafetyField(int scale, int inner_scale, int delay, int destory, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Position = new(100, 0, 100);
        dp.Radian = float.Pi * 2;
        dp.Scale = new(scale);
        dp.InnerScale = new(inner_scale);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(4f);
        dp.Delay = delay;
        dp.DestoryAt = destory;
        return dp;
    }

    private static void drawDonutSafetyField(ScriptAccessory accessory)
    {
        var dp = assignDp_DonutSafetyField(7, 3, 0, 7000, "Geocentric Donut Outer", accessory);
        dp.Position = new(100, 0, 90);
        dp.Color = ColorHelper.colorDark.V4.WithW(4f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

        var dp2 = AssignDp.drawStatic(new(100, 0, 90), 0, 0, 7000, "Geocentric Donut Inner", accessory);
        dp2.Color = ColorHelper.colorDark.V4.WithW(4f);
        dp2.Scale = new(2);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);
    }

    private static void drawVerticalSafetyField(ScriptAccessory accessory)
    {
        var dp = AssignDp.drawStatic(new(100, 0, 82), float.Pi, 0, 7000, "Geocentric Vertical Center Line", accessory);
        dp.Scale = new(4, 20);
        dp.Color = ColorHelper.colorDark.V4.WithW(4f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        var dp2 = AssignDp.drawStatic(new(95, 0, 82), float.Pi, 0, 7000, "Geocentric Vertical Left Line", accessory);
        dp2.Scale = new(4, 20);
        dp2.Color = ColorHelper.colorDark.V4.WithW(4f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);

        var dp3 = AssignDp.drawStatic(new(105, 0, 82), float.Pi, 0, 7000, "Geocentric Vertical Right Line", accessory);
        dp3.Scale = new(4, 20);
        dp3.Color = ColorHelper.colorDark.V4.WithW(4f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp3);
    }

    private static void drawHorizonSafetyField(ScriptAccessory accessory)
    {
        var dp = AssignDp.drawStatic(new(92, 0, 90), float.Pi / 2, 0, 7000, "Geocentric Horizontal Center Line", accessory);
        dp.Scale = new(4, 20);
        dp.Color = ColorHelper.colorDark.V4.WithW(4f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        var dp2 = AssignDp.drawStatic(new(95, 0, 82), float.Pi / 2, 0, 7000, "Geocentric Horizontal Top Line", accessory);
        dp2.Scale = new(4, 20);
        dp2.Color = ColorHelper.colorDark.V4.WithW(4f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);

        var dp3 = AssignDp.drawStatic(new(105, 0, 82), float.Pi / 2, 0, 7000, "Geocentric Horizontal Right Line", accessory);
        dp3.Scale = new(4, 20);
        dp3.Color = ColorHelper.colorDark.V4.WithW(4f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp3);
    }

    [ScriptMethod(name: "Main Boss: Gaia, Spread", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0016"])]
    public void MB_SpreadIcon(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Gaia_I && phase != P12S_Phase.Gaia_II) return;
        var tid = @event.TargetId();
        var uid = accessory.Data.Me;

        var dp = AssignDp.drawCircle(tid, 0, 3000, $"Divine Judgment{tid}", accessory);
        dp.Color = tid == uid ? accessory.Data.DefaultSafeColor.WithW(3f) : accessory.Data.DefaultDangerColor.WithW(1.5f);
        dp.Scale = new(1);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    private static void drawDonutSpreadPos(int myIndex, ScriptAccessory accessory)
    {
        List<float> spreadDir = [-0.5f, 0.5f, -1.5f, 1.5f, -3.5f, 3.5f, -2.5f, 2.5f];
        Vector3[] safePos = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            safePos[i] = DirectionCalc.ExtendPoint(new(100, 0, 90), DirectionCalc.angle2Rad(spreadDir[i] * 45), 2.5f);
        }

        for (int i = 0; i < 8; i++)
        {
            var dp = AssignDp.drawStatic(safePos[i], 0, 0, 7000, $"Donut Spread Position{i}", accessory);
            dp.Scale = new(0.5f);
            dp.Color = myIndex == i ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            if (myIndex != i) continue;
            var dp0 = AssignDp.dirPos(safePos[i], 0, 7000, $"Donut Spread Position Guidance{i}", accessory);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);
        }
    }

    private static void drawVerticalSpreadPos(int myIndex, ScriptAccessory accessory)
    {
        List<int> spreadDirLR = [-1, 1, -1, 1, -1, 1, -1, 1];
        List<int> spreadDirUD = [-3, -3, -1, -1, 3, 3, 1, 1];

        Vector3[] safePos = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            safePos[i] = new Vector3(100, 0, 90) + new Vector3(spreadDirLR[i] * 2.5f, 0, 0) + new Vector3(0, 0, spreadDirUD[i] * 2);
        }

        for (int i = 0; i < 8; i++)
        {
            var dp = AssignDp.drawStatic(safePos[i], 0, 0, 7000, $"Vertical Spread Position{i}", accessory);
            dp.Scale = new(0.5f);
            dp.Color = myIndex == i ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            if (myIndex != i) continue;
            var dp0 = AssignDp.dirPos(safePos[i], 0, 7000, $"Vertical Spread Position Guidance{i}", accessory);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);
        }
    }

    private static void drawHorizonSpreadPos(int myIndex, ScriptAccessory accessory)
    {
        List<int> spreadDirUD = [-1, -1, -1, -1, 1, 1, 1, 1];
        List<int> spreadDirLR = [-1, 1, -3, 3, -1, 1, -3, 3];

        // 84 88 92 96

        Vector3[] safePos = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            safePos[i] = new Vector3(100, 0, 90) + new Vector3(spreadDirLR[i] * 2, 0, 0) + new Vector3(0, 0, spreadDirUD[i] * 2.5f);
        }

        for (int i = 0; i < 8; i++)
        {
            var dp = AssignDp.drawStatic(safePos[i], 0, 0, 7000, $"Horizontal Spread Position{i}", accessory);
            dp.Scale = new(0.5f);
            dp.Color = myIndex == i ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            if (myIndex != i) continue;
            var dp0 = AssignDp.dirPos(safePos[i], 0, 7000, $"Horizontal Spread Position Guidance{i}", accessory);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);
        }
    }

    [ScriptMethod(name: "Main Boss: Gaia, Spread Remove (Uncontrollable)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:33582", "TargetIndex:1"], userControl: false)]
    public void MB_Gaia1_SpreadIconRemove(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Gaia_I && phase != P12S_Phase.Gaia_II) return;
        var tid = @event.TargetId();
        accessory.Method.RemoveDraw($"Divine Judgment{tid}");
    }

    [ScriptMethod(name: "Main Boss: Gaia 1, Corner Safe Zone", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4562", "SourceDataId:16182"])]
    public async void MB_Gaia1_PartySafeCorner(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Gaia_I) return;
        lock (mb_Gaia1_dangerPlace)
        {
            var spos = @event.SourcePosition();
            var dangerIdx = DirectionCalc.PositionRoundToDirs(spos, new(100, 0, 90), 8);
            mb_Gaia1_dangerPlace[dangerIdx] = true;
            mb_Gaia1_dangerPlace[dangerIdx >= 4 ? dangerIdx - 4 : dangerIdx + 4] = true;
            DebugMsg($"Directions {dangerIdx} and {(dangerIdx >= 4 ? dangerIdx - 4 : dangerIdx + 4)} marked as dangerous", accessory);
        }

        await Task.Delay(100);

        lock (mb_Gaia1_dangerPlace)
        {
            if (mb_Gaia1_dangerPlace.Count(x => x) != 6) return;    // If not 6 dangerous directions, do not proceed
            if (mb_Gaia1_dangerPlace_hasDrawn) return;

            var myIndex = IndexHelper.getMyIndex(accessory);
            for (int i = 0; i < mb_Gaia1_dangerPlace.Count(); i++)
            {
                if (mb_Gaia1_dangerPlace[i]) continue;
                var isMySafeCorner = drawSafeCorner_Gaia1(i, myIndex, accessory);
                DebugMsg($"Drew safe corner {i}, {(isMySafeCorner ? "and it is the destination" : "not the destination")}", accessory);
            }
            mb_Gaia1_dangerPlace_hasDrawn = true;
        }
    }
    private static bool drawSafeCorner_Gaia1(int posIdx, int myIndex, ScriptAccessory accessory)
    {
        var dp = assignDp_SafeCornerCircle(posIdx, $"Safe Corner{posIdx}", accessory);
        // DPS safe zone is the bottom-right half, corresponding to directions 2, 3, 4, 5.
        var isMySafeCorner = ((myIndex >= 4) && (posIdx >= 2) && (posIdx <= 5)) || ((myIndex < 4) && ((posIdx <= 1) || (posIdx >= 6)));

        dp.Color = isMySafeCorner ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        if (!isMySafeCorner) return false;
        var pos = DirectionCalc.ExtendPoint(new(100, 0, 90), DirectionCalc.angle2Rad(45 * posIdx), 6.5f);
        var dp0 = AssignDp.dirPos(pos, 0, 6500, $"Guidance{posIdx}Prepare", accessory);
        dp0.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);

        var dp1 = AssignDp.dirPos(pos, 6500, 3500, $"Guidance{posIdx}", accessory);
        dp1.Color = accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);

        return true;
    }

    private static DrawPropertiesEdit assignDp_SafeCornerCircle(int idx, string name, ScriptAccessory accessory)
    {
        var pos = DirectionCalc.ExtendPoint(new(100, 0, 90), DirectionCalc.angle2Rad(45 * idx), 6.5f);
        var dp = AssignDp.drawStatic(pos, 0, 0, 10000, name, accessory);
        dp.Scale = new(1);
        return dp;
    }

    #endregion


    #region Main Boss: Classic 1

    [ScriptMethod(name: "Main Boss: Phase Transition Classic (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33585"], userControl: false)]
    public void MB_PhaseChange_Classic(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            P12S_Phase.Pangenesis => P12S_Phase.Classic_II,
            _ => P12S_Phase.Classic_I,
        };
        DebugMsg($"Current phase: {phase}", accessory);
    }

    [ScriptMethod(name: "Main Boss: Classic 1, Player Classic Grouping (Uncontrollable)", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(016F|017[012])$"], userControl: false)]
    public void MB_Classic1_PlayerGroupRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Classic_I) return;
        var id = @event.Id();
        var tid = @event.TargetId();
        var tidx = IndexHelper.getPlayerIdIndex(tid, accessory);
        var tname = @event.TargetName();

        // Four types of classic markers
        const uint CIRCLE = 367;
        const uint CROSS = 370;
        const uint TRIANGLE = 368;
        const uint SQUARE = 369;

        lock (mb_Classic1_playerGroup)
        {
            switch (id)
            {
                case CIRCLE:
                    mb_Classic1_playerGroup[tidx] = mb_Classic1_playerGroup[tidx] + 1;
                    break;
                case CROSS:
                    mb_Classic1_playerGroup[tidx] = mb_Classic1_playerGroup[tidx] + 2;
                    break;
                case TRIANGLE:
                    mb_Classic1_playerGroup[tidx] = mb_Classic1_playerGroup[tidx] + 3;
                    break;
                case SQUARE:
                    mb_Classic1_playerGroup[tidx] = mb_Classic1_playerGroup[tidx] + 4;
                    break;
                default:
                    break;
            }
        }
        // DebugMsg($"Player {tidx}({tname}) received Icon {id}", accessory);
    }

    [ScriptMethod(name: "Main Boss: Classic 1, Player A/B Grouping (Uncontrollable)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(356[01])$"], userControl: false)]
    public void MB_Classic1_PlayerBuffRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Classic_I) return;
        var sid = @event.StatusID();
        var tid = @event.TargetId();
        var tidx = IndexHelper.getPlayerIdIndex(tid, accessory);
        var tname = @event.TargetName();

        // Two types of buffs
        const uint ALPHA = 3560;
        const uint BETA = 3561;

        lock (mb_Classic1_playerGroup)
        {
            switch (sid)
            {
                case ALPHA:
                    break;
                case BETA:
                    mb_Classic1_playerGroup[tidx] = mb_Classic1_playerGroup[tidx] + 10;
                    break;
                default:
                    break;
            }
        }
        // DebugMsg($"Player {tidx}({tname}) received buff {(sid == ALPHA ? "ALPHA" : "BETA")}", accessory);
    }

    [ScriptMethod(name: "Main Boss: Classic 1, Element Grouping (Uncontrollable)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(1618[345])$"], userControl: false)]
    public void MB_Classic1_ElementRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Classic_I) return;
        var did = @event.DataId();
        var sid = @event.SourceId();
        var sname = @event.SourceName();
        var spos = @event.SourcePosition();

        int[] position = getElementPos(spos);
        lock (mb_Classic1_elements)
        {
            mb_Classic1_elements.Add(new ClassicElement(sid, did, position[0], position[1]));
            if (mb_Classic1_elements.Count() != 12) return;
            sortClassicElements(mb_Classic1_elements, accessory);
        }
    }
    private void sortClassicElements(List<ClassicElement> elements, ScriptAccessory accessory)
    {
        for (int i = 0; i < 12; i++)
        {
            ClassicElement element = elements[i];
            if (!element.isWater())
                continue;

            // This element is water, need to find adjacent elements
            List<int> e_idxs = getNearbyElementIdxs(element, elements);

            DebugMsg($"Checking water element ({element.Row}, {element.Col}), has {e_idxs.Count()} adjacent elements", accessory);

            var FiresNearby = 0;
            var EarthesNearby = 0;

            foreach (var idx in e_idxs)
            {
                ClassicElement t_element = elements[idx];
                if (t_element.isFire())
                    FiresNearby++;
                if (t_element.isEarth())
                    EarthesNearby++;
            }

            foreach (var idx in e_idxs)
            {
                // Set adjacent element as target for checking
                ClassicElement t_element = elements[idx];
                DebugMsg($"Checking {(t_element.isFire() ? "Fire" : "Earth")} element ({t_element.Row}, {t_element.Col})", accessory);

                // If already chosen, ignore
                if (t_element.HasChosen) continue;

                // If only one fire/earth element is adjacent, it must be the target
                if ((FiresNearby == 1 && t_element.isFire()) || (EarthesNearby == 1 && t_element.isEarth()))
                {
                    AssignTarget(t_element, element);
                    DebugMsg($"Water element ({element.Row},{element.Col}) found its sole target {(t_element.isFire() ? "Fire" : "Earth")} element ({t_element.Row},{t_element.Col})", accessory);
                    continue;
                }

                // Further check adjacent elements of the target element
                List<int> te_idxs = getNearbyElementIdxs(t_element, elements);

                // If the number of adjacent water elements is not 1, it's ambiguous, ignore
                var te_water_nearby = 0;
                foreach (var te_idx in te_idxs)
                {
                    if (elements[te_idx].isWater())
                        te_water_nearby++;
                }
                if (te_water_nearby != 1) continue;

                // Target element is the connection object for this water element
                AssignTarget(t_element, element);
                DebugMsg($"Water element ({element.Row},{element.Col}) found target after secondary check {(t_element.isFire() ? "Fire" : "Earth")} element ({t_element.Row},{t_element.Col})", accessory);
            }
        }
    }

    private void AssignTarget(ClassicElement t_element, ClassicElement element)
    {
        t_element.TargetRow = element.Row;
        t_element.TargetCol = element.Col;
        t_element.TargetSid = element.Sid;
        t_element.HasChosen = true;
    }

    private List<int> getNearbyElementIdxs(ClassicElement element, List<ClassicElement> elements)
    {
        List<int> idxs = new List<int>();
        for (int i = 0; i < 12; i++)
        {
            if (element.isNear(elements[i]))
                idxs.Add(i);
        }
        return idxs;
    }
    private int[] getElementPos(Vector3 spos)
    {
        // 88 96 104 112
        // 80 1 2 3 4

        // 84 92 100
        // 76 1 2 3
        int col = (int)Math.Round((spos.X - 80) / 8);
        int row = (int)Math.Round((spos.Z - 76) / 8);

        return [row, col];
    }

    public class ClassicElement
    {
        const uint FIRE = 16183;
        const uint WATER = 16184;
        const uint EARTH = 16185;
        public uint Sid { get; set; }
        public uint Type { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public int TargetRow { get; set; } = -1;
        public int TargetCol { get; set; } = -1;
        public uint TargetSid { get; set; } = 0;
        public bool HasChosen { get; set; } = false;
        public ClassicElement(uint sid, uint type, int row, int col)
        {
            Sid = sid;
            Type = type;
            Row = row;
            Col = col;
        }
        public bool isNear(ClassicElement element)
        {
            int sum = Math.Abs(Row - element.Row) + Math.Abs(Col - element.Col);
            return sum == 1;
        }
        public bool isWater()
        {
            return Type == WATER;
        }
        public bool isFire()
        {
            return Type == FIRE;
        }
        public bool isEarth()
        {
            return Type == EARTH;
        }
    }

    [ScriptMethod(name: "Main Boss: Classic 1, Element Guidance", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:16183"])]
    public async void MB_Classic1_ElementTargetDraw(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Classic_I) return;
        await Task.Delay(300);

        if (mb_Classic1_etDrawn) return;
        mb_Classic1_etDrawn = true;

        var myIndex = IndexHelper.getMyIndex(accessory);

        foreach (var element in mb_Classic1_elements)
        {
            if (element.isWater()) continue;
            drawElementRoute(element, mb_Classic1_playerGroup[myIndex], accessory);
        }
    }

    private void drawElementRoute(ClassicElement e, int myClassicBuff, ScriptAccessory accessory)
    {
        int myCol = myClassicBuff % 10;
        bool isAlpha = myClassicBuff < 10;

        // Match condition: ((element is Fire and I am Alpha) or (element is Earth and I am Beta)) and my column equals target (water) column
        bool isMatched = ((e.isFire() && isAlpha) || (e.isEarth() && !isAlpha)) && myCol == e.TargetCol;

        var dp = assignDp_Element2Element(e, accessory);
        dp.Color = isMatched ? posColorPlayer.V4.WithW(3f) : posColorNormal.V4.WithW(1.5f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        if (!isMatched) return;

        IGameObject? water = accessory.GetById(e.TargetSid);
        IGameObject? fe = accessory.GetById(e.Sid);
        if (water == null || fe == null) return;

        Vector3 tpos = (fe.Position - water.Position) / 8 * 3f + water.Position;
        var dp0 = AssignDp.dirPos(tpos, 0, 6000, $"Classic Element Guidance", accessory);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);

    }
    private DrawPropertiesEdit assignDp_Element2Element(ClassicElement e, ScriptAccessory accessory)
    {
        var dp = AssignDp.drawOwner2Target(e.TargetSid, e.Sid, 0, 12000, $"Classic Element{e.Row}{e.Col}", accessory);
        dp.Scale = new(2f, 4f);
        dp.Color = posColorNormal.V4.WithW(1.5f);
        return dp;
    }

    // Purple tether appears (0001), element implosion
    [ScriptMethod(name: "Main Boss: Classic 1, Implosion and Ray Guidance Preparation", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0001"])]
    public void MB_Classic1_RayDirPrepared(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Classic_I) return;

        if (mb_Classic1_implodeDrawn) return;
        mb_Classic1_implodeDrawn = true;

        foreach (var element in mb_Classic1_elements)
        {
            var dp = AssignDp.drawCircle(element.Sid, 0, 12000, $"Implosion{element.Sid}", accessory);
            dp.Scale = new(4f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        var myIndex = IndexHelper.getMyIndex(accessory);
        var myClassicIndex = getMyClassicIndex(myIndex);
        drawRayDirPrepared(myClassicIndex, true, accessory);
    }

    private int getMyClassicIndex(int myIndex)
    {
        var myClassicBuff = mb_Classic1_playerGroup[myIndex];
        bool isAlpha = myClassicBuff < 10;
        var myClassicIndex = ((myClassicBuff % 10) - 1) * 2 + (isAlpha ? 0 : 1);
        return myClassicIndex;
    }

    private void drawRayDirPrepared(int myClassicIndex, bool isDanger, ScriptAccessory accessory)
    {
        Vector3[] sonyPos = new Vector3[8];
        sonyPos[0] = new(84, 0, 88);
        sonyPos[1] = new(84, 0, 96);
        sonyPos[2] = new(92, 0, 88);
        sonyPos[3] = new(92, 0, 96);
        sonyPos[4] = new(108, 0, 88);
        sonyPos[5] = new(108, 0, 96);
        sonyPos[6] = new(116, 0, 88);
        sonyPos[7] = new(116, 0, 96);

        for (int i = 0; i < sonyPos.Count(); i++)
        {
            if (isDanger)
            {
                var dp = AssignDp.drawStatic(sonyPos[i], 0, 0, 12000, $"Classic Ray Standby Position{i}", accessory);
                dp.Scale = new(1f);
                dp.Color = i == myClassicIndex ? posColorPlayer.V4.WithW(3f) : posColorNormal.V4;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }

            if (i != myClassicIndex) continue;
            var dp0 = AssignDp.dirPos(sonyPos[i], 0, 12000, $"{(isDanger ? $"Classic Ray Standby Position Guidance{i}" : $"Classic Ray Position Guidance{i}")}", accessory);
            dp0.Color = isDanger ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);
        }
    }

    [ScriptMethod(name: "Main Boss: Classic 1, Implosion Drawing Remove (Uncontrollable)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:33587"], userControl: false)]
    public void MB_Classic1_ImplodeRemove(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Classic_I) return;
        accessory.Method.RemoveDraw($"^(Implosion.*)$");
    }

    // Green line disappears, Classic ray guidance
    [ScriptMethod(name: "Main Boss: Classic 1, Ray Range and Guidance", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3588"])]
    public void MB_Classic1_RayDir(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Classic_I) return;
        if (mb_Classic1_RayDirDrawn) return;
        mb_Classic1_RayDirDrawn = true;

        accessory.Method.RemoveDraw($"^(Classic Ray Standby Position Guidance.*)$");

        var myIndex = IndexHelper.getMyIndex(accessory);
        var myClassicIndex = getMyClassicIndex(myIndex);
        drawRayDirPrepared(myClassicIndex, false, accessory);
        drawPalladianRay(new(92, 0, 92), accessory);
        drawPalladianRay(new(108, 0, 92), accessory);
    }

    private void drawPalladianRay(Vector3 pos, ScriptAccessory accessory)
    {
        for (uint i = 0; i < 4; i++)
        {
            var dp = AssignDp.drawTargetOrder(0, i + 1, 0, 12000, $"Palladian Ray{i}", accessory);
            dp.Position = pos;
            dp.Scale = new(20f);
            dp.Radian = float.Pi / 6;
            // dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
            dp.Color = ColorHelper.colorPink.V4;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
    }

    [ScriptMethod(name: "Main Boss: Classic 1, Ray and Guidance Remove (Uncontrollable)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:33572"], userControl: false)]
    public void MB_Classic1_RayDirRemove(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Classic_I) return;
        accessory.Method.RemoveDraw($"^(Palladian Ray.*)$");
        accessory.Method.RemoveDraw($"^(Classic Ray Standby Position.*)$");
        accessory.Method.RemoveDraw($"^(Classic Ray Position Guidance.*)$");
    }

    #endregion

    #region Main Boss: Caloric 1

    [ScriptMethod(name: "Main Boss: Phase Transition Caloric (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33592"], userControl: false)]
    public void MB_PhaseChange_Caloric(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            P12S_Phase.Classic_II => P12S_Phase.Caloric_II,
            _ => P12S_Phase.Caloric_I,
        };
        mb_Caloric_phase = 0;
        DebugMsg($"Current phase: {phase}", accessory);
    }

    [ScriptMethod(name: "Main Boss: Caloric 1, Initial Marker Record (Uncontrollable)", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:012F"], userControl: false)]
    public async void MB_Caloric_FirstDir(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(200);

        if (phase != P12S_Phase.Caloric_I) return;
        if (mb_Caloric_phase > 2) return;

        var tid = @event.TargetId();
        var tidx = IndexHelper.getPlayerIdIndex(tid, accessory);
        mb_Caloric_isFirstTarget[tidx] = true;
        mb_Caloric_WindPriority[tidx]++;
        if (tidx >= 4)
            mb_Caloric_WindPriority[tidx]++;
        // 1 top-left, 2 top-right, 3 bottom-right, 4 bottom-left

        mb_Caloric_phase++;
    }

    [ScriptMethod(name: "Main Boss: Caloric 1, Initial Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33597"])]
    public async void MB_Caloric_FirstStack(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(300);

        if (phase != P12S_Phase.Caloric_I) return;
        if (mb_Caloric_phase > 2) return;

        var myIndex = IndexHelper.getMyIndex(accessory);
        var tid = @event.TargetId();
        drawCaloricStack(tid, mb_Caloric_isFirstTarget[myIndex], accessory);
        drawCaloricFirstDir(accessory);
    }

    private void drawCaloricStack(uint owner_id, bool isDanger, ScriptAccessory accessory)
    {
        var dp = AssignDp.drawCircle(owner_id, 0, 8000, $"Caloric Stack{owner_id}", accessory);
        dp.Scale = new(4);
        dp.Color = isDanger ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    private void drawCaloricFirstDir(ScriptAccessory accessory)
    {
        var myIndex = IndexHelper.getMyIndex(accessory);
        Vector3 SAFE = new Vector3(100, 0, 97.5f);
        Vector3 UPLEFT = new Vector3(99, 0, 89);
        Vector3 UPRIGHT = new Vector3(101, 0, 89);

        var isSafe = !mb_Caloric_isFirstTarget[myIndex];
        var pos = isSafe ? SAFE : (myIndex < 4 ? UPLEFT : UPRIGHT);
        var dp0 = AssignDp.dirPos(pos, 0, 8000, $"Caloric Initial Guidance{myIndex}", accessory);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);
    }

    [ScriptMethod(name: "Main Boss: Caloric Phase Transition, 4-Group Stack (Uncontrollable)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:33592"], userControl: false)]
    public void MB_PhaseChange_Caloric_PartnerStack(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Caloric_I) return;
        if (mb_Caloric_phase > 2) return;
        mb_Caloric_phase = 10;
        accessory.Method.RemoveDraw($"^(Caloric Stack.*)$");
        accessory.Method.RemoveDraw($"^(Caloric Initial Guidance.*)$");
    }

    [ScriptMethod(name: "Main Boss: Caloric Buff Record (Uncontrollable)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(359[01])$"], userControl: false)]
    public void MB_Caloric_BuffRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Caloric_I) return;
        if (mb_Caloric_phase < 10) return;

        var tid = @event.TargetId();
        var tidx = IndexHelper.getPlayerIdIndex(tid, accessory);
        var sid = @event.StatusID();

        // const uint FIRE = 3590;
        const uint WIND = 3591;
        mb_Caloric_isWind[tidx] = sid == WIND;
    }

    [ScriptMethod(name: "Main Boss: Caloric, 4-Group Stack Range", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3590"])]
    public async void MB_Caloric_PartnerStack(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Caloric_I) return;
        if (mb_Caloric_phase < 10) return;
        mb_Caloric_phase++;
        if (mb_Caloric_ParnterStackDrawn) return;
        mb_Caloric_ParnterStackDrawn = true;

        await Task.Delay(200);

        DebugMsg($"Starting to draw fire stack range {mb_Caloric_phase}", accessory);

        var myStackPos = getMyStackPos(accessory);
        for (int i = 0; i < 8; i++)
        {
            if (mb_Caloric_isWind[i]) continue;
            bool isMyPartner = myStackPos == mb_Caloric_FirePriority[i];
            drawCaloricPartnerStack(accessory.Data.PartyList[i], isMyPartner, accessory);
        }
    }

    private int getMyStackPos(ScriptAccessory accessory)
    {
        var myIndex = IndexHelper.getMyIndex(accessory);
        var myStackPos = (mb_Caloric_WindPriority[myIndex] + mb_Caloric_FirePriority[myIndex]) % 10;
        return myStackPos;
    }

    private void drawCaloricPartnerStack(uint owner_id, bool isMyPartner, ScriptAccessory accessory)
    {
        var dp = AssignDp.drawCircle(owner_id, 0, 12000, $"Caloric 4-Group Stack{owner_id}", accessory);
        dp.Scale = new(4);
        dp.Color = isMyPartner ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Main Boss: Caloric, 4-Group Stack Wind/Fire Guidance", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3591"])]
    public async void MB_Caloric_PartnerStackDir(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Caloric_I) return;
        if (mb_Caloric_phase < 10) return;
        mb_Caloric_phase++;
        if (mb_Caloric_ParnterStackDirDrawn) return;
        mb_Caloric_ParnterStackDirDrawn = true;

        await Task.Delay(100);

        DebugMsg($"Starting Caloric priority calculation {mb_Caloric_phase}", accessory);
        calcCaloricPriority();
        string WindPriorityStr = string.Join(", ", mb_Caloric_WindPriority);
        DebugMsg($"Wind priority: {WindPriorityStr}", accessory);
        string FirePriorityStr = string.Join(", ", mb_Caloric_FirePriority);
        DebugMsg($"Fire priority: {FirePriorityStr}", accessory);

        var myIndex = IndexHelper.getMyIndex(accessory);
        var myStackPos = getMyStackPos(accessory);
        Vector3 stackPos = myStackPos switch
        {
            1 => new(97.5f, 0, 92.5f),
            2 => new(102.5f, 0, 92.5f),
            3 => new(102.5f, 0, 97.5f),
            4 => new(97.5f, 0, 97.5f),
            _ => new(100, 0, 100)
        };

        if (mb_Caloric_isWind[myIndex] && mb_Caloric_WindPriority[myIndex] <= 2)
            stackPos = stackPos - new Vector3(0, 0, 3.5f);

        var dp = AssignDp.dirPos(stackPos, 0, 12000, $"Caloric Position{myStackPos}{mb_Caloric_isWind[myIndex]}", accessory);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    private void calcCaloricPriority()
    {
        for (var i = 0; i < 8; i++)
        {
            if (mb_Caloric_isWind[i])
            {
                if (mb_Caloric_WindPriority[i] != 0) continue;
                mb_Caloric_WindPriority[i] = mb_Caloric_WindPriority.Max() + 1;
            }
            else
            {
                if (mb_Caloric_FirePriority[i] != 0) continue;
                mb_Caloric_FirePriority[i] = mb_Caloric_FirePriority.Max() + 1;
            }
        }
    }

    [ScriptMethod(name: "Main Boss: Caloric Phase Transition, Wind Knockback (Uncontrollable)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:33594"], userControl: false)]
    public void MB_PhaseChange_Caloric_WindKnockBack(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Caloric_I) return;
        if (mb_Caloric_phase < 10) return;
        mb_Caloric_phase = 20;
        accessory.Method.RemoveDraw($"^(Caloric Position.*)$");
        accessory.Method.RemoveDraw($"^(Caloric 4-Group Stack.*)$");
    }

    [ScriptMethod(name: "Main Boss: Caloric, Secondary Fire Stack Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3590"], userControl: false)]
    public void MB_Caloric_SecondBuffRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Caloric_I) return;
        if (mb_Caloric_phase < 20) return;

        var tid = @event.TargetId();
        var tidx = IndexHelper.getPlayerIdIndex(tid, accessory);
        // Secondary fire stack is always chosen from the initial fire group
        lock (mb_Caloric_FirePriority)
        {
            mb_Caloric_FirePriority[tidx] += 10;
        }
    }

    [ScriptMethod(name: "Main Boss: Caloric, Secondary Fire Stack Guidance", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3590"])]
    public async void MB_Caloric_SecondPartnerStackDir(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Caloric_I) return;
        if (mb_Caloric_phase < 20) return;
        mb_Caloric_phase++;

        if (mb_Caloric_SecondParnterStackDirDrawn) return;
        mb_Caloric_SecondParnterStackDirDrawn = true;

        await Task.Delay(100);

        DebugMsg($"Starting secondary Caloric priority calculation {mb_Caloric_phase}", accessory);
        calcCaloricPrioritySecond();

        string WindPriorityStr = string.Join(", ", mb_Caloric_WindPriority);
        DebugMsg($"Wind priority: {WindPriorityStr}", accessory);
        string FirePriorityStr = string.Join(", ", mb_Caloric_FirePriority);
        DebugMsg($"Fire priority: {FirePriorityStr}", accessory);

        var myIndex = IndexHelper.getMyIndex(accessory);
        var myStackPos = getMyStackPos(accessory);

        Vector3 stackPos = myStackPos switch
        {
            1 => mb_Caloric_isWind[myIndex] ? new(93.5f, 0, 85.5f) : new(97.5f, 0, 92.5f),
            2 => mb_Caloric_isWind[myIndex] ? new(106.5f, 0, 85.5f) : new(102.5f, 0, 92.5f),
            3 => new(106.5f, 0, 100.5f),
            4 => new(93.5f, 0, 100.5f),
            _ => new(100, 0, 100)
        };

        var dp = AssignDp.dirPos(stackPos, 0, 12000, $"Caloric Secondary Position{myStackPos}{mb_Caloric_isWind[myIndex]}", accessory);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    private void calcCaloricPrioritySecond()
    {
        bool needSwap = false;
        var fireTargets = mb_Caloric_FirePriority.Where(x => x > 10).Take(2).ToList();

        // Same left/right
        if (fireTargets.Sum() % 10 == 5)
            needSwap = true;

        for (int i = 0; i < 8; i++)
        {
            mb_Caloric_FirePriority[i] = (mb_Caloric_FirePriority[i] % 10) switch
            {
                1 => mb_Caloric_FirePriority[i],
                2 => mb_Caloric_FirePriority[i],
                3 => (mb_Caloric_FirePriority[i] > 10 ? 10 : 0) + (needSwap ? 1 : 2),
                4 => (mb_Caloric_FirePriority[i] > 10 ? 10 : 0) + (needSwap ? 2 : 1),
                _ => mb_Caloric_FirePriority[i],
            };
        }
    }

    [ScriptMethod(name: "Main Boss: Caloric, Secondary Stack Range", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3590"])]
    public async void MB_Caloric_SecondPartnerStack(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Caloric_I) return;
        if (mb_Caloric_phase < 20) return;
        mb_Caloric_phase++;
        if (mb_Caloric_SecondParnterStackDrawn) return;
        mb_Caloric_SecondParnterStackDrawn = true;

        await Task.Delay(200);

        DebugMsg($"Starting to draw fire stack range {mb_Caloric_phase}", accessory);

        var myStackPos = getMyStackPos(accessory);
        for (int i = 0; i < 8; i++)
        {
            if (mb_Caloric_isWind[i]) continue;
            if (mb_Caloric_FirePriority[i] < 10) continue;
            bool isMyPartner = myStackPos == mb_Caloric_FirePriority[i] % 10;
            drawCaloricPartnerStack(accessory.Data.PartyList[i], isMyPartner, accessory);
        }
    }

    [ScriptMethod(name: "Main Boss: Caloric, Secondary Ring Wind Range", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3590"])]
    public async void MB_Caloric_SecondWindDonut(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Caloric_I) return;
        if (mb_Caloric_phase < 20) return;
        mb_Caloric_phase++;
        if (mb_Caloric_SecondWindDonutDrawn) return;
        mb_Caloric_SecondWindDonutDrawn = true;

        await Task.Delay(200);

        DebugMsg($"Starting to draw ring wind range {mb_Caloric_phase}", accessory);

        for (int i = 0; i < accessory.Data.PartyList.Count(); i++)
        {
            if (!mb_Caloric_isWind[i]) continue;
            drawWindCircle(accessory.Data.PartyList[i], accessory);
        }
    }

    private void drawWindCircle(uint owner_id, ScriptAccessory accessory)
    {
        var dp = AssignDp.drawCircle(owner_id, 0, 12000, $"Caloric Ring Wind{owner_id}", accessory);
        dp.Scale = new(7);
        // dp.Color = accessory.Data.DefaultDangerColor;
        dp.Color = ColorHelper.colorLightBlue.V4;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Main Boss: Caloric End (Uncontrollable)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:33595"], userControl: false)]
    public void MB_PhaseChange_Caloric_End(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Caloric_I) return;
        if (mb_Caloric_phase < 20) return;
        mb_Caloric_phase = 30;
        accessory.Method.RemoveDraw($"^(Caloric 4-Group Stack.*)$");
        accessory.Method.RemoveDraw($"^(Caloric Ring Wind.*)$");
        accessory.Method.RemoveDraw($"^(Caloric Secondary Position.*)$");
    }

    #endregion

    #region Main Boss: Exaflare 1

    [ScriptMethod(name: "Main Boss: Phase Transition Exaflare (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33566"], userControl: false)]
    public void MB_PhaseChange_Exflare(Event @event, ScriptAccessory accessory)
    {
        phase = P12S_Phase.Exflare;
        DebugMsg($"Current phase: {phase}", accessory);
    }

    [ScriptMethod(name: "Main Boss: Exaflare Warning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33567"])]
    public void MB_Exflare(Event @event, ScriptAccessory accessory)
    {
        var spos = @event.SourcePosition();
        var srot = @event.SourceRotation();

        Vector3[] exflarePos = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            exflarePos[i] = DirectionCalc.ExtendPoint(spos, DirectionCalc.BaseInnGame2DirRad(srot), 8 * i);
        }

        const int CAST_TIME = 6000;
        const int INTERVAL_TIME = 2000;

        for (int i = 0; i < 6; i++)
        {
            var destoryAt = i == 0 ? CAST_TIME : INTERVAL_TIME;
            var delay = i == 0 ? 0 : CAST_TIME + (i - 1) * INTERVAL_TIME;
            // Primary Exaflare
            drawExflare(exflarePos[i], delay, destoryAt, accessory);
            // Warning Exaflare
            if (i < 5)
                drawExflareWarn(exflarePos[i + 1], 1, delay, destoryAt, accessory);
            if (i < 4)
                drawExflareWarn(exflarePos[i + 2], 2, delay, destoryAt, accessory);
        }
    }

    private void drawExflare(Vector3 spos, int delay, int destoryAt, ScriptAccessory accessory)
    {
        var dp = AssignDp.drawStatic(spos, 0, delay, destoryAt, $"Exaflare{spos}", accessory);
        dp.Scale = new(6f);
        dp.Color = exflareColor.V4.WithW(1.5f);
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    private void drawExflareWarn(Vector3 spos, int adv, int delay, int destoryAt, ScriptAccessory accessory)
    {
        const int INTERVAL_TIME = 2000;

        var destroy_add = INTERVAL_TIME * (adv - 1);
        var dp = AssignDp.drawStatic(spos, 0, delay, destoryAt + destroy_add, $"Exaflare{spos}", accessory);
        dp.Scale = new(6f);
        dp.Color = exflareWarnColor.V4.WithW(0.8f / adv);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Main Boss: Exaflare Flare Safe Zone Record (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34435"], userControl: false)]
    public void MB_ExflarePosRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Exflare) return;
        var spos = @event.SourcePosition();
        var posidx = DirectionCalc.PositionRoundToDirs(spos, new(100, 0, 95), 4);
        mb_Exflare_FlarePos[posidx] = true;
    }

    [ScriptMethod(name: "Main Boss: Exaflare Safe Zone Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34435"])]
    public async void MB_ExflareDir(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Exflare) return;
        var spos = @event.SourcePosition();
        if (mb_Exflare_DirDrawn) return;
        mb_Exflare_DirDrawn = true;

        await Task.Delay(100);
        if (mb_Exflare_FlarePos.Count(x => x) != 2) return;
        DebugMsg($"{(mb_Exflare_FlarePos[0] ? "Exaflare at top, left/right safe" : "Exaflare at left, top/bottom safe")}", accessory);

        Vector3[] ExflareDir = new Vector3[2];
        var myIndex = IndexHelper.getMyIndex(accessory);

        // Exaflare at top, left/right safe
        ExflareDir[0] = getExflareFirstSafePos(mb_Exflare_FlarePos[0], myIndex);
        ExflareDir[1] = getExflareSecondSafePos(mb_Exflare_FlarePos[0], myIndex);
        drawExflareDir(ExflareDir[0], ExflareDir[1], accessory);
    }

    private Vector3 getExflareFirstSafePos(bool isLeftSafe, int myIndex)
    {
        Vector3[] ExflareSafePos = new Vector3[4];
        ExflareSafePos[0] = new(98, 0, 81);     // Top
        ExflareSafePos[1] = new(119, 0, 92);    // Right
        ExflareSafePos[2] = new(102, 0, 109);   // Bottom
        ExflareSafePos[3] = new(81, 0, 98);     // Left

        int safePosIdx;
        if (isLeftSafe)
            safePosIdx = (myIndex % 2 == 0) ? 3 : 1;    // MT group has even index, remainder 0
        else
            safePosIdx = (myIndex % 4 < 2) ? 0 : 2;     // Melee group index divided by 4, remainder 0, 1
        return ExflareSafePos[safePosIdx];
    }

    private Vector3 getExflareSecondSafePos(bool isLeftSafe, int myIndex)
    {
        const int CENTER_Z = 95;
        const int CENTER_X = 100;

        Vector3[] ExflareSpreadPos = new Vector3[16];
        ExflareSpreadPos[0] = new(81, 0, 81);
        ExflareSpreadPos[1] = DirectionCalc.FoldPointLR(ExflareSpreadPos[0], CENTER_X);
        ExflareSpreadPos[4] = new(93, 0, 81);
        ExflareSpreadPos[5] = DirectionCalc.FoldPointLR(ExflareSpreadPos[4], CENTER_X);
        ExflareSpreadPos[2] = DirectionCalc.FoldPointUD(ExflareSpreadPos[4], CENTER_Z);
        ExflareSpreadPos[3] = DirectionCalc.FoldPointUD(ExflareSpreadPos[5], CENTER_Z);
        ExflareSpreadPos[6] = DirectionCalc.FoldPointUD(ExflareSpreadPos[0], CENTER_Z);
        ExflareSpreadPos[7] = DirectionCalc.FoldPointUD(ExflareSpreadPos[1], CENTER_Z);

        ExflareSpreadPos[8] = new(81, 0, 81);
        ExflareSpreadPos[9] = DirectionCalc.FoldPointLR(ExflareSpreadPos[8], CENTER_X);
        ExflareSpreadPos[12] = new(81, 0, 90);
        ExflareSpreadPos[13] = DirectionCalc.FoldPointLR(ExflareSpreadPos[12], CENTER_X);
        ExflareSpreadPos[10] = DirectionCalc.FoldPointUD(ExflareSpreadPos[12], CENTER_Z);
        ExflareSpreadPos[11] = DirectionCalc.FoldPointUD(ExflareSpreadPos[13], CENTER_Z);
        ExflareSpreadPos[14] = DirectionCalc.FoldPointUD(ExflareSpreadPos[8], CENTER_Z);
        ExflareSpreadPos[15] = DirectionCalc.FoldPointUD(ExflareSpreadPos[9], CENTER_Z);

        int safePosIdx;
        if (isLeftSafe)
            safePosIdx = myIndex + 8;    // 8-15 for left/right spread
        else
            safePosIdx = myIndex;     // 0-7 for top/bottom spread
        return ExflareSpreadPos[safePosIdx];
    }

    private void drawExflareDir(Vector3 safePos, Vector3 spreadPos, ScriptAccessory accessory)
    {
        var dp = AssignDp.dirPos(safePos, 0, 7000, $"Exaflare Flare Safe", accessory);
        var dp0 = AssignDp.dirPos2Pos(safePos, spreadPos, 0, 7000, $"Prepare to Spread", accessory);
        dp0.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);

        var dp1 = AssignDp.dirPos(spreadPos, 7000, 3000, $"Spread Position", accessory);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
    }

    #endregion

    #region Main Boss: Pangenesis

    [ScriptMethod(name: "Main Boss: Phase Transition Pangenesis (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33599"], userControl: false)]
    public void MB_PhaseChange_Pangenesis(Event @event, ScriptAccessory accessory)
    {
        phase = P12S_Phase.Pangenesis;
        DebugMsg($"Current phase: {phase}", accessory);
    }

    [ScriptMethod(name: "Main Boss: Pangenesis Queue Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33599"])]
    public async void MB_Pangenesis_InitPosition(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(100);
        if (phase != P12S_Phase.Pangenesis) return;
        accessory.Method.TextInfo($"Form a horizontal line", 5000);
    }

    [ScriptMethod(name: "Main Boss: Pangenesis Buff Record (Uncontrollable)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3593|357[67])$"], userControl: false)]
    public async void MB_Pangenesis_BuffRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Pangenesis) return;
        if (mb_Pangenesis_phase[0]) return;
        var tid = @event.TargetId();
        var tidx = IndexHelper.getPlayerIdIndex(tid, accessory);
        var sid = @event.StatusID();
        var stc = @event.StackCount();
        var dur = @event.DurationMilliseconds();

        const uint DNA = 3593;
        const uint WHITE = 3576;
        const uint BLACK = 3577;
        const uint LONG = 20000;
        const uint SHORT = 16000;

        // DebugMsg($"Detected TIDX {tidx}, SID {sid}, STC {stc}, DUR {dur}", accessory);

        switch (sid)
        {
            case DNA:
                mb_Pangenesis_hasFactor[tidx] = true;
                mb_Pangenesis_isTwo[tidx] = stc == 2;
                // DebugMsg($"Player {tidx}: {(sid == DNA ? "Has Factor" : "No Factor")}, {(stc == 2 ? "2 Layers": "1 Layer")}", accessory);
                break;
            case WHITE:
            case BLACK:
                mb_Pangenesis_isWhite[tidx] = sid == WHITE;
                mb_Pangenesis_isLong[tidx] = dur == LONG;
                mb_Pangenesis_shouldWhiteTower[tidx] = sid == BLACK;
                // DebugMsg($"Player {tidx}: {(sid == WHITE ? "White Buff" : "Black Buff")}, {(dur == LONG ? "Long": "Short")}", accessory);
                break;
            default:
                break;
        }


        await Task.Delay(100);
        mb_Pangenesis_phase[0] = true;
        // DebugMsg($"Pangenesis initial buff record locked", accessory);
    }

    [ScriptMethod(name: "Main Boss: Pangenesis Nonpolarity Group Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33602"])]
    public void MB_Pangenesis_NonpolarityPos(Event @event, ScriptAccessory accessory)
    {
        var FirstLeftPlayerIdx = getSpecificTowerPlayer(true, false, false, false, true);
        var FirstRightPlayerIdx = getSpecificTowerPlayer(true, false, false, false, false);
        var SecondUpLeftPlayerIdx = getSpecificTowerPlayer(false, false, false, false, true);
        var SecondUpRightPlayerIdx = getSpecificTowerPlayer(false, false, false, false, false);

        mb_Pangenesis_TowerIdxOrder[0] = FirstLeftPlayerIdx;
        mb_Pangenesis_TowerIdxOrder[4] = FirstRightPlayerIdx;
        mb_Pangenesis_TowerIdxOrder[2] = SecondUpLeftPlayerIdx;
        mb_Pangenesis_TowerIdxOrder[6] = SecondUpRightPlayerIdx;
        mb_Pangenesis_TowerIdxSecondOrder[0] = SecondUpLeftPlayerIdx;
        mb_Pangenesis_TowerIdxSecondOrder[4] = SecondUpRightPlayerIdx;

        DebugMsg($"Left Tower 1 {IndexHelper.getPlayerJobByIndex(FirstLeftPlayerIdx)} position.", accessory);
        DebugMsg($"Right Tower 1 {IndexHelper.getPlayerJobByIndex(FirstRightPlayerIdx)} position.", accessory);
        DebugMsg($"Left Tower 2 Top {IndexHelper.getPlayerJobByIndex(SecondUpLeftPlayerIdx)} position.", accessory);
        DebugMsg($"Right Tower 2 Top {IndexHelper.getPlayerJobByIndex(SecondUpRightPlayerIdx)} position.", accessory);

        const int CENTER_X = 100;
        const int CENTER_Z = 91;

        Vector3 FIRST_LEFT = new Vector3(85, 0, 91);
        Vector3 FIRST_RIGHT = DirectionCalc.FoldPointLR(FIRST_LEFT, CENTER_X);
        Vector3 SECOND_UP_LEFT = new Vector3(90, 0, 88);
        Vector3 SECOND_UP_RIGHT = DirectionCalc.FoldPointLR(SECOND_UP_LEFT, CENTER_X);

        var myIndex = IndexHelper.getMyIndex(accessory);
        if (myIndex == FirstLeftPlayerIdx)
            drawPangenesisDir(FIRST_LEFT, FirstLeftPlayerIdx, 0, 9000, accessory);
            // var dp_FirstLeft = assignDp_TowerDir(FIRST_LEFT, FirstLeftPlayerIdx, 0, 9000, accessory);
            // accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp_FirstLeft);
        if (myIndex == FirstRightPlayerIdx)
            drawPangenesisDir(FIRST_RIGHT, FirstRightPlayerIdx, 0, 9000, accessory);
            // var dp_FirstRight = assignDp_TowerDir(FIRST_RIGHT, FirstRightPlayerIdx, 0, 9000, accessory);
            // accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp_FirstRight);
        if (myIndex == SecondUpLeftPlayerIdx)
            drawPangenesisDir(SECOND_UP_LEFT, SecondUpLeftPlayerIdx, 0, 14000, accessory);
            // var dp_SecondUpLeft = assignDp_TowerDir(SECOND_UP_LEFT, SecondUpLeftPlayerIdx, 0, 14000, accessory);
            // accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp_SecondUpLeft);
        if (myIndex == SecondUpRightPlayerIdx)
            drawPangenesisDir(SECOND_UP_RIGHT, SecondUpRightPlayerIdx, 0, 14000, accessory);
            // var dp_SecondUpRight = assignDp_TowerDir(SECOND_UP_RIGHT, SecondUpRightPlayerIdx, 0, 14000, accessory);
            // accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp_SecondUpRight);
    }

    [ScriptMethod(name: "Main Boss: Pangenesis Tower Count Record (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3360[34])$"], userControl: false)]
    public void MB_Pangenesis_TowerNumRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Pangenesis) return;
        mb_Pangenesis_towerNum++;
    }

    [ScriptMethod(name: "Main Boss: Pangenesis Step 1", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3360[34])$"])]
    public async void MB_Pangenesis_FirstPlace(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Pangenesis) return;
        if (mb_Pangenesis_phase[1]) return;

        // Wait for tower count to be recorded
        await Task.Delay(100);
        if (mb_Pangenesis_towerNum > 2) return;

        var myIndex = IndexHelper.getMyIndex(accessory);

        bool isLeftTower = false;
        bool isWhiteTower = false;
        bool needWhiteBuff;

        const int CENTER_X = 100;
        const uint WHITE_TOWER = 33603;
        const uint BLACK_TOWER = 33604;

        var aid = @event.ActionId();
        var tpos = @event.TargetPosition();

        if (tpos.X < CENTER_X)
            isLeftTower = true;
        if (aid == WHITE_TOWER)
            isWhiteTower = true;
        needWhiteBuff = !isWhiteTower;

        // Only when the tower color appears can the remaining 4 players be determined
        mb_Pangenesis_shouldWhiteTower[mb_Pangenesis_TowerIdxOrder[isLeftTower ? 0 : 4]] = isWhiteTower;
        mb_Pangenesis_shouldWhiteTower[mb_Pangenesis_TowerIdxOrder[isLeftTower ? 2 : 6]] = !isWhiteTower;

        // Side Tower 1: 2-layer factor + short black/white buff
        var FirstPlayerIdx = getSpecificTowerPlayer(true, true, false, needWhiteBuff, true);
        mb_Pangenesis_TowerIdxOrder[isLeftTower ? 1 : 5] = FirstPlayerIdx;

        DebugMsg($"{(isLeftTower ? "Left Tower 1" : "Right Tower 1")} is {(isWhiteTower ? "White" : "Black")}, {IndexHelper.getPlayerJobByIndex(FirstPlayerIdx)} soaks.", accessory);

        if (myIndex == FirstPlayerIdx)
            drawPangenesisDir(tpos, FirstPlayerIdx, 0, 5000, accessory);
            // var dp = assignDp_TowerDir(tpos, FirstPlayerIdx, 0, 5000, accessory);
            // accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        const int CENTER_Z = 91;
        Vector3 SECOND_UP_LEFT = new Vector3(90, 0, 88);
        Vector3 SECOND_UP_RIGHT = DirectionCalc.FoldPointLR(SECOND_UP_LEFT, CENTER_X);
        Vector3 SECOND_DOWN_LEFT = DirectionCalc.FoldPointUD(SECOND_UP_LEFT, CENTER_Z);
        Vector3 SECOND_DOWN_RIGHT = DirectionCalc.FoldPointUD(SECOND_UP_RIGHT, CENTER_Z);

        // Tower 2 Down: 2-layer factor + long black/white buff
        var SecondDownPlayerIdx = getSpecificTowerPlayer(true, true, true, needWhiteBuff, true);
        mb_Pangenesis_TowerIdxOrder[isLeftTower ? 3 : 7] = SecondDownPlayerIdx;
        mb_Pangenesis_TowerIdxSecondOrder[isLeftTower ? 2 : 6] = SecondDownPlayerIdx;

        DebugMsg($"{(isLeftTower ? "Left Tower 2 Down" : "Right Tower 2 Down")} is {(isWhiteTower ? "White" : "Black")}, {IndexHelper.getPlayerJobByIndex(SecondDownPlayerIdx)} soaks.", accessory);

        if (myIndex == SecondDownPlayerIdx)
            drawPangenesisDir(isLeftTower ? SECOND_DOWN_LEFT : SECOND_DOWN_RIGHT, SecondDownPlayerIdx, 0, 10000, accessory);
            // var dp = assignDp_TowerDir(isLeftTower ? SECOND_DOWN_LEFT : SECOND_DOWN_RIGHT, SecondDownPlayerIdx, 0, 10000, accessory);
            // accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        await Task.Delay(100);
        mb_Pangenesis_phase[1] = true;
        DebugMsg($"=== Pangenesis Step 1 End ===", accessory);
    }

    [ScriptMethod(name: "Main Boss: Pangenesis Tower Color Change (Uncontrollable)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(357[67])$"], userControl: false)]
    public void MB_Pangenesis_ChangePolar(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Pangenesis) return;
        if (mb_Pangenesis_towerNum < 2) return;
        // During the process, whenever a black/white buff is added, the tower color needs to be swapped
        var tid = @event.TargetId();
        var tidx = IndexHelper.getPlayerIdIndex(tid, accessory);
        mb_Pangenesis_shouldWhiteTower[tidx] = !mb_Pangenesis_shouldWhiteTower[tidx];
    }

    [ScriptMethod(name: "Main Boss: Pangenesis Step 2", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3360[34])$"])]
    public async void MB_Pangenesis_SecondPlace(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Pangenesis) return;
        if (mb_Pangenesis_phase[2]) return;

        // Wait for tower count to be recorded
        await Task.Delay(100);
        if (mb_Pangenesis_towerNum <= 2) return;
        if (mb_Pangenesis_towerNum > 6) return;

        // Wait for polarity buffs to be recorded
        await Task.Delay(1000);

        var myIndex = IndexHelper.getMyIndex(accessory);

        bool isLeftTower = false;
        bool isWhiteTower = false;
        bool isUpTower = false;

        const int CENTER_X = 100;
        const int CENTER_Z = 91;
        const uint WHITE_TOWER = 33603;
        const uint BLACK_TOWER = 33604;

        var aid = @event.ActionId();
        var tpos = @event.TargetPosition();

        if (tpos.X < CENTER_X)
            isLeftTower = true;
        if (tpos.Z < CENTER_Z)
            isUpTower = true;
        if (aid == WHITE_TOWER)
            isWhiteTower = true;

        // A black/white tower spawned, check isLeft
        // If isLeft, the players soaking the black/white tower should be the ones from Tower Order indices 0 and 1
        // If not isLeft, check indices 4 and 5
        int TowerOrderPlayerIdx;

        if (isLeftTower)
        {
            if ((mb_Pangenesis_shouldWhiteTower[mb_Pangenesis_TowerIdxOrder[0]] && isWhiteTower) || (!mb_Pangenesis_shouldWhiteTower[mb_Pangenesis_TowerIdxOrder[0]] && !isWhiteTower))
                TowerOrderPlayerIdx = 0;
            else
                TowerOrderPlayerIdx = 1;
        }
        else
        {
            if ((mb_Pangenesis_shouldWhiteTower[mb_Pangenesis_TowerIdxOrder[4]] && isWhiteTower) || (!mb_Pangenesis_shouldWhiteTower[mb_Pangenesis_TowerIdxOrder[4]] && !isWhiteTower))
                TowerOrderPlayerIdx = 4;
            else
                TowerOrderPlayerIdx = 5;
        }

        int playerIdx = mb_Pangenesis_TowerIdxOrder[TowerOrderPlayerIdx];
        int SecondTowerIdx = 1 + (isLeftTower ? 0 : 4) + (isUpTower ? 0 : 2);
        mb_Pangenesis_TowerIdxSecondOrder[SecondTowerIdx] = playerIdx;

        DebugMsg($"{(isLeftTower ? "Left Tower 2" : "Right Tower 2")} {(isUpTower ? "Top" : "Bottom")} {(isWhiteTower ? "White" : "Black")} Tower, {IndexHelper.getPlayerJobByIndex(playerIdx)} soaks.", accessory);

        if (myIndex == playerIdx)
            drawPangenesisDir(tpos, playerIdx, 0, 3900, accessory);
            // var dp = assignDp_TowerDir(tpos, playerIdx, 0, 3900, accessory);
            // accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        await Task.Delay(100);
        mb_Pangenesis_phase[2] = true;
        DebugMsg($"=== Pangenesis Step 2 End ===", accessory);
    }

    [ScriptMethod(name: "Main Boss: Pangenesis Step 3", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3360[34])$"])]
    public async void MB_Pangenesis_ThirdPlace(Event @event, ScriptAccessory accessory)
    {
        if (phase != P12S_Phase.Pangenesis) return;
        if (mb_Pangenesis_phase[3]) return;

        // Wait for tower count to be recorded
        await Task.Delay(100);
        if (mb_Pangenesis_towerNum <= 6) return;
        if (mb_Pangenesis_towerNum > 10) return;

        // Wait for polarity buffs to be recorded
        await Task.Delay(1000);

        var myIndex = IndexHelper.getMyIndex(accessory);

        bool isLeftTower = false;
        bool isWhiteTower = false;
        bool isUpTower = false;

        const int CENTER_X = 100;
        const int CENTER_Z = 91;
        const uint WHITE_TOWER = 33603;
        const uint BLACK_TOWER = 33604;

        var aid = @event.ActionId();
        var tpos = @event.TargetPosition();

        if (tpos.X < CENTER_X)
            isLeftTower = true;
        if (tpos.Z < CENTER_Z)
            isUpTower = true;
        if (aid == WHITE_TOWER)
            isWhiteTower = true;

        for (int i = 0; i < 4; i++)
        {
            var idx = isLeftTower ? i : i + 4;
            var playerIdx = mb_Pangenesis_TowerIdxSecondOrder[idx];

            if ((mb_Pangenesis_shouldWhiteTower[playerIdx] && isWhiteTower) || (!mb_Pangenesis_shouldWhiteTower[playerIdx] && !isWhiteTower))
            {
                DebugMsg($"{(isLeftTower ? "Left Tower 3" : "Right Tower 3")} {(isUpTower ? "Top" : "Bottom")} {(isWhiteTower ? "White" : "Black")} Tower, {IndexHelper.getPlayerJobByIndex(playerIdx)} soaks.", accessory);

                if (myIndex != playerIdx) continue;
                drawPangenesisDir(tpos, playerIdx, 0, 3900, accessory);
                // var dp = assignDp_TowerDir(tpos, playerIdx, 0, 3900, accessory);
                // accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                // var dp0 = AssignDp.drawStatic(tpos, 0, 0, 3900, $"Pangenesis{playerIdx}", accessory);
                // dp0.Scale = new(3f);
                // dp0.Color = accessory.Data.DefaultSafeColor;
                // accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
            }
        }

        await Task.Delay(100);
        mb_Pangenesis_phase[3] = true;
        DebugMsg($"=== Pangenesis Step 3 End ===", accessory);
    }

    private int getSpecificTowerPlayer(bool hasFactor, bool isTwo, bool isLong, bool isWhiteBuff, bool isFirst)
    {
        for (int i = 0; i < 8; i++)
        {
            int idx = isFirst ? i : 7 - i;
            bool condition_1 = hasFactor ? mb_Pangenesis_hasFactor[idx] : !mb_Pangenesis_hasFactor[idx];
            bool condition_2 = isTwo ? mb_Pangenesis_isTwo[idx] : !mb_Pangenesis_isTwo[idx];
            bool condition_3 = isLong ? mb_Pangenesis_isLong[idx] : !mb_Pangenesis_isLong[idx];
            bool condition_4 = isWhiteBuff ? mb_Pangenesis_isWhite[idx] : !mb_Pangenesis_isWhite[idx];

            if (condition_1 && condition_2 && condition_3 && condition_4)
                return idx;
        }
        return -1;
    }

    private DrawPropertiesEdit assignDp_TowerDir(Vector3 tower_pos, int player_idx, int delay, int destoryAt, ScriptAccessory accessory)
    {
        var dp = AssignDp.dirPos(tower_pos, delay, destoryAt, $"Pangenesis{player_idx}-{tower_pos}", accessory);
        return dp;
    }

    private void drawPangenesisDir(Vector3 tower_pos, int player_idx, int delay, int destoryAt, ScriptAccessory accessory)
    {
        var dp = assignDp_TowerDir(tower_pos, player_idx, delay, destoryAt, accessory);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        var dp0 = AssignDp.drawStatic(tower_pos, 0, delay, destoryAt, $"Pangenesis{player_idx}", accessory);
        dp0.Scale = new(3f);
        dp0.Color = accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp0);
    }

    #endregion
}


#region Function Collections
public static class EventExtensions
{
    private static bool ParseHexId(string? idStr, out uint id)
    {
        id = 0;
        if (string.IsNullOrEmpty(idStr)) return false;
        try
        {
            var idStr2 = idStr.Replace("0x", "");
            id = uint.Parse(idStr2, System.Globalization.NumberStyles.HexNumber);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static uint ActionId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
    }

    public static uint SourceId(this Event @event)
    {
        return ParseHexId(@event["SourceId"], out var id) ? id : 0;
    }

    public static uint DataId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["DataId"]);
    }

    public static uint SourceDataId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["SourceDataId"]);
    }

    public static uint TargetId(this Event @event)
    {
        return ParseHexId(@event["TargetId"], out var id) ? id : 0;
    }

    public static uint TargetIndex(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["TargetIndex"]);
    }

    public static Vector3 SourcePosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
    }

    public static Vector3 TargetPosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
    }

    public static Vector3 EffectPosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
    }

    public static float SourceRotation(this Event @event)
    {
        return JsonConvert.DeserializeObject<float>(@event["SourceRotation"]);
    }

    public static float TargetRotation(this Event @event)
    {
        return JsonConvert.DeserializeObject<float>(@event["TargetRotation"]);
    }

    public static string SourceName(this Event @event)
    {
        return @event["SourceName"];
    }

    public static string TargetName(this Event @event)
    {
        return @event["TargetName"];
    }

    public static uint DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["DurationMilliseconds"]);
    }

    public static uint Index(this Event @event)
    {
        return ParseHexId(@event["Index"], out var id) ? id : 0;
    }

    public static uint State(this Event @event)
    {
        return ParseHexId(@event["State"], out var id) ? id : 0;
    }

    public static uint DirectorId(this Event @event)
    {
        return ParseHexId(@event["DirectorId"], out var id) ? id : 0;
    }

    public static uint Id(this Event @event)
    {
        return ParseHexId(@event["Id"], out var id) ? id : 0;
    }

    public static uint StatusID(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StatusID"]);
    }

    public static uint StackCount(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StackCount"]);
    }

    public static uint Param(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["Param"]);
    }
}

public static class IbcHelper
{
    public static IGameObject? GetById(this ScriptAccessory sa, uint id)
    {
        return sa.Data.Objects.SearchByEntityId(id);
    }
}

public static class DirectionCalc
{
    // North as 0 for list
    // InnGame      List    Dir
    // 0            - 4     pi
    // 0.25 pi      - 3     0.75pi
    // 0.5 pi       - 2     0.5pi
    // 0.75 pi      - 1     0.25pi
    // pi           - 0     0
    // 1.25 pi      - 7     1.75pi
    // 1.5 pi       - 6     1.5pi
    // 1.75 pi      - 5     1.25pi
    // Dir = Pi - InnGame (+ 2pi)

    /// <summary>
    /// Convert in-game base angle (South as 0, increasing counter-clockwise) to logic base angle (North as 0, increasing clockwise)
    /// </summary>
    /// <param name="radian">In-game base angle</param>
    /// <returns>Logic base angle</returns>
    public static float BaseInnGame2DirRad(float radian)
    {
        float r = (float)Math.PI - radian;
        if (r < 0) r = (float)(r + 2 * Math.PI);
        if (r > 2 * Math.PI) r = (float)(r - 2 * Math.PI);
        return r;
    }

    /// <summary>
    /// Convert logic base angle (North as 0, increasing clockwise) to in-game base angle (South as 0, increasing counter-clockwise)
    /// </summary>
    /// <param name="radian">Logic base angle</param>
    /// <returns>In-game base angle</returns>
    public static float BaseDirRad2InnGame(float radian)
    {
        float r = (float)Math.PI - radian;
        if (r < Math.PI) r = (float)(r + 2 * Math.PI);
        if (r > Math.PI) r = (float)(r - 2 * Math.PI);
        return r;
    }

    /// <summary>
    /// Input logic base angle, get logic direction
    /// </summary>
    /// <param name="radian">Logic base angle</param>
    /// <param name="dirs">Total number of directions</param>
    /// <returns>Logic direction corresponding to the logic base angle</returns>
    public static int DirRadRoundToDirs(float radian, int dirs)
    {
        var r = Math.Round(radian / (2f / dirs * Math.PI));
        if (r == dirs) r = r - dirs;
        return (int)r;
    }

    /// <summary>
    /// Input coordinates, get normal division logic direction (with top-right as 0)
    /// </summary>
    /// <param name="point">Coordinate point</param>
    /// <param name="center">Center point</param>
    /// <param name="dirs">Total number of directions</param>
    /// <returns>Logic direction corresponding to the coordinate point</returns>
    public static int PositionFloorToDirs(Vector3 point, Vector3 center, int dirs)
    {
        // Normal division, 0° is the dividing line, dividing 360° into dirs parts
        var r = Math.Floor(dirs / 2 - dirs / 2 * Math.Atan2(point.X - center.X, point.Z - center.Z) / Math.PI) % dirs;
        return (int)r;
    }

    /// <summary>
    /// Input coordinates, get diagonal division logic direction (with straight up as 0)
    /// </summary>
    /// <param name="point">Coordinate point</param>
    /// <param name="center">Center point</param>
    /// <param name="dirs">Total number of directions</param>
    /// <returns>Logic direction corresponding to the coordinate point</returns>
    public static int PositionRoundToDirs(Vector3 point, Vector3 center, int dirs)
    {
        // Diagonal division, 0° returns 0, dividing 360° into dirs parts
        var r = Math.Round(dirs / 2 - dirs / 2 * Math.Atan2(point.X - center.X, point.Z - center.Z) / Math.PI) % dirs;
        return (int)r;
    }

    /// <summary>
    /// Convert angle to radian
    /// </summary>
    /// <param name="angle">Angle in degrees</param>
    /// <returns>Corresponding radian value</returns>
    public static float angle2Rad(float angle)
    {
        // Convert input angle to radian
        float radian = (float)(angle * Math.PI / 180);
        return radian;
    }

    /// <summary>
    /// Convert radian to angle
    /// </summary>
    /// <param name="radian">Radian value</param>
    /// <returns>Corresponding angle in degrees</returns>
    public static float rad2Angle(float radian)
    {
        // Convert input radian to angle
        float angle = (float)(radian / Math.PI * 180);
        return angle;
    }

    /// <summary>
    /// Rotate a point around a center by a logic base radian
    /// </summary>
    /// <param name="point">Point to rotate</param>
    /// <param name="center">Center</param>
    /// <param name="radian">Rotation radian</param>
    /// <returns>Rotated coordinate point</returns>
    public static Vector3 RotatePoint(Vector3 point, Vector3 center, float radian)
    {
        // Rotate a point clockwise by a certain radian around a center
        Vector2 v2 = new(point.X - center.X, point.Z - center.Z);
        var rot = MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian;
        var length = v2.Length();
        return new(center.X + MathF.Sin(rot) * length, center.Y, center.Z - MathF.Cos(rot) * length);
    }

    /// <summary>
    /// Extend a point from a center point by a logic base angle
    /// </summary>
    /// <param name="center">Center point to extend from</param>
    /// <param name="radian">Rotation radian</param>
    /// <param name="length">Extension length</param>
    /// <returns>Extended coordinate point</returns>
    public static Vector3 ExtendPoint(Vector3 center, float radian, float length)
    {
        // Extend a point a certain length at a certain radian
        return new(center.X + MathF.Sin(radian) * length, center.Y, center.Z - MathF.Cos(radian) * length);
    }

    /// <summary>
    /// Find the logic base radian from an outer point to the center
    /// </summary>
    /// <param name="center">Center</param>
    /// <param name="new_point">Outer point</param>
    /// <returns>Logic base radian from the outer point to the center</returns>
    public static float FindRadian(Vector3 center, Vector3 new_point)
    {
        // Find the radian from the point to the center
        float radian = MathF.PI - MathF.Atan2(new_point.X - center.X, new_point.Z - center.Z);
        if (radian < 0)
            radian += 2 * MathF.PI;
        return radian;
    }

    /// <summary>
    /// Fold the input point horizontally
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerx">Center axis X coordinate</param>
    /// <returns></returns>
    public static Vector3 FoldPointLR(Vector3 point, int centerx)
    {
        Vector3 v3 = new(2 * centerx - point.X, point.Y, point.Z);
        return v3;
    }

    /// <summary>
    /// Fold the input point vertically
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerx">Center axis Z coordinate</param>
    /// <returns></returns>
    public static Vector3 FoldPointUD(Vector3 point, int centerz)
    {
        Vector3 v3 = new(point.X, point.Y, 2 * centerz - point.Z);
        return v3;
    }
}

public static class IndexHelper
{
    /// <summary>
    /// Input player dataId, get the corresponding position index
    /// </summary>
    /// <param name="pid">Player SourceId</param>
    /// <param name="accessory"></param>
    /// <returns>The position index corresponding to the player</returns>
    public static int getPlayerIdIndex(uint pid, ScriptAccessory accessory)
    {
        // Get player IDX
        return accessory.Data.PartyList.IndexOf(pid);
    }

    /// <summary>
    /// Get the position index of the main perspective player
    /// </summary>
    /// <param name="accessory"></param>
    /// <returns>Position index of the main perspective player</returns>
    public static int getMyIndex(ScriptAccessory accessory)
    {
        return accessory.Data.PartyList.IndexOf(accessory.Data.Me);
    }

    /// <summary>
    /// Input player dataId, get the corresponding position name, output string only for text output
    /// </summary>
    /// <param name="pid">Player SourceId</param>
    /// <param name="accessory"></param>
    /// <returns>The position name corresponding to the player</returns>
    public static string getPlayerJobByID(uint pid, ScriptAccessory accessory)
    {
        // Get player role abbreviation, only for DEBUG output
        var a = accessory.Data.PartyList.IndexOf(pid);
        switch (a)
        {
            case 0: return "MT";
            case 1: return "ST";
            case 2: return "H1";
            case 3: return "H2";
            case 4: return "D1";
            case 5: return "D2";
            case 6: return "D3";
            case 7: return "D4";
            default: return "unknown";
        }
    }

    /// <summary>
    /// Input position index, get the corresponding position name, output string only for text output
    /// </summary>
    /// <param name="idx">Position index</param>
    /// <returns></returns>
    public static string getPlayerJobByIndex(int idx)
    {
        switch (idx)
        {
            case 0: return "MT";
            case 1: return "ST";
            case 2: return "H1";
            case 3: return "H2";
            case 4: return "D1";
            case 5: return "D2";
            case 6: return "D3";
            case 7: return "D4";
            default: return "unknown";
        }
    }
}

public static class ColorHelper
{
    public static ScriptColor colorRed = new ScriptColor { V4 = new Vector4(1.0f, 0f, 0f, 1.0f) };
    public static ScriptColor colorPink = new ScriptColor { V4 = new Vector4(1f, 0f, 1f, 1.0f) };
    public static ScriptColor colorCyan = new ScriptColor { V4 = new Vector4(0f, 1f, 0.8f, 1.0f) };
    public static ScriptColor colorDark = new ScriptColor { V4 = new Vector4(0f, 0f, 0f, 1.0f) };
    public static ScriptColor colorLightBlue = new ScriptColor { V4 = new Vector4(0.48f, 0.40f, 0.93f, 1.0f) };
    public static ScriptColor colorWhite = new ScriptColor { V4 = new Vector4(1f, 1f, 1f, 2f) };
}

public static class AssignDp
{
    /// <summary>
    /// Return dp from self to a target location, can modify dp.TargetPosition, dp.Scale
    /// </summary>
    /// <param name="target_pos">Target location</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit dirPos(Vector3 target_pos, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(1f);
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = target_pos;
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    /// <summary>
    /// Return dp from a start location to a target location, can modify dp.Position, dp.TargetPosition, dp.Scale
    /// </summary>
    /// <param name="start_pos">Start location</param>
    /// <param name="target_pos">Target location</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit dirPos2Pos(Vector3 start_pos, Vector3 target_pos, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(1f);
        dp.Position = start_pos;
        dp.TargetPosition = target_pos;
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    /// <summary>
    /// Return dp from self to a target object, can modify dp.TargetObject, dp.Scale
    /// </summary>
    /// <param name="target_id">Target object</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit dirTarget(uint target_id, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(1f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = target_id;
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    /// <summary>
    /// Return dp related to an object's enmity or a fixed point, can modify dp.TargetResolvePattern, dp.TargetOrderIndex, dp.Owner
    /// </summary>
    /// <param name="owner_id">Start target id, usually the boss</param>
    /// <param name="order_idx">Order, starting from 1</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit drawTargetOrder(uint owner_id, uint order_idx, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(5, 40);
        dp.Owner = owner_id;
        dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
        dp.TargetOrderIndex = order_idx;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    /// <summary>
    /// Return dp related to an object's distance, can modify dp.CentreResolvePattern, dp.CentreOrderIndex, dp.Owner
    /// </summary>
    /// <param name="owner_id">Start target id, usually the boss</param>
    /// <param name="order_idx">Order, starting from 1</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit drawCenterOrder(uint owner_id, uint order_idx, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(5);
        dp.Owner = owner_id;
        dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
        dp.CentreOrderIndex = order_idx;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }
    /// <summary>
    /// Return owner to target dp, can modify dp.Owner, dp.TargetObject, dp.Scale
    /// </summary>
    /// <param name="owner_id">Start target id, usually self</param>
    /// <param name="target_id">Target unit id</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit drawOwner2Target(uint owner_id, uint target_id, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(5, 40);
        dp.Owner = owner_id;
        dp.TargetObject = target_id;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    /// <summary>
    /// Return circle drawing, follows owner, can modify dp.Owner, dp.Scale
    /// </summary>
    /// <param name="owner_id">Start target id, usually self or boss</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit drawCircle(uint owner_id, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(5);
        dp.Owner = owner_id;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    /// <summary>
    /// Return donut drawing, follows owner, can modify dp.Owner, dp.Scale
    /// </summary>
    /// <param name="owner_id">Start target id, usually self or boss</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit drawDonut(uint owner_id, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(22);
        dp.InnerScale = new(6);
        dp.Radian = float.Pi * 2;
        dp.Owner = owner_id;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    /// <summary>
    /// Return static dp, usually for guiding fixed positions. Can modify dp.Position, dp.Rotation, dp.Scale
    /// </summary>
    /// <param name="center">Start position, usually arena center</param>
    /// <param name="rotate_rad">Rotation angle, North as 0 degrees clockwise</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit drawStatic(Vector3 center, float rotate_rad, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(5, 20);
        dp.Position = center;
        dp.Rotation = DirectionCalc.BaseDirRad2InnGame(rotate_rad);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }
}


#endregion
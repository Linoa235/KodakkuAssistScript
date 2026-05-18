using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Data;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Module.GameEvent.Types;
using KodakkuAssist.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;
using Dalamud.Utility.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Lumina.Excel.Sheets;

namespace UsamisKodakku.Scripts._07_DawnTrail.FRU;

[ScriptType(name: Name, territorys: [1238], guid: "76aed92d-26ce-40a3-a94c-1a3091e79315",
    version: Version, Author: "Linoa235", note: NoteStr, updateInfo: UpdateInfo)]

// ^(?!.*((Monk|Machinist|Dragoon|Samurai|Ninja|Viper|Reaper|Dancer|Bard|Astrologian|Sage|Scholar|(Eos|Selene)|Seraph|White Mage|Warrior|Paladin|Dark Knight|Gunbreaker|Pictomancer|Black Mage|Blue Mage|Summoner|Carbuncle|Demigod Bahamut|Demigod Phoenix|Garuda-Egi|Titan-Egi|Ifrit-Egi|Puppet)\] (Used|Cast))).*35501.*$
// ^\[\w+\|[^|]+\|E\]\s\w+

public class FruPatch
{
    private const string NoteStr =
        """
        v0.0.0.12
        Command mode is effective for P3 mechanic 2.
        If command mode is enabled, melee-optimized waymarks will be applied. (Fixed MT left and D1 right, not MT left and ST right!)
        Both tanks will receive guidance on the direction they need to face, regardless of which tank is baiting.
        If issues occur, please provide feedback via ARR.
        """;

    private const string Name = "FRU_Patch [Futures Rewritten Ultimate Patch]";
    private const string Version = "0.0.0.12";
    private const string DebugVersion = "a";
    private const string UpdateInfo =
        """
        1. Adapted to Kodakku 0.5.x.x
        """;
    private const bool Debugging = false;
    private static readonly bool LocalTest = false;
    private static readonly bool LocalStrTest = false;      // Use string representation only for local waymarks.

    [UserSetting("Debug Mode, turn off for non-developers")]
    public static bool DebugMode { get; set; } = false;

    [UserSetting("Position hint circle drawing - Normal color")]
    public static ScriptColor PosColorNormal { get; set; } = new ScriptColor { V4 = new Vector4(1.0f, 1.0f, 1.0f, 1.0f) };
    [UserSetting("Position hint circle drawing - Player position color")]
    public static ScriptColor PosColorPlayer { get; set; } = new ScriptColor { V4 = new Vector4(0.0f, 1.0f, 1.0f, 1.0f) };

    [UserSetting("P1 - Fall of Faith Priority")]
    public static FallOfFaithPriorityEnum FallOfFaithPriority { get; set; } = FallOfFaithPriorityEnum.H_T_D_H;
    public enum FallOfFaithPriorityEnum
    {
        H_T_D_H,
        T_H_D,
        H_T_D,
    }
    [UserSetting("P2 - Light Rampage Strategy")]
    public static LightRampageStgEnum LightRampageStg { get; set; } = LightRampageStgEnum.Grey9;
    public enum LightRampageStgEnum
    {
        Hexagram,
        Grey9,
    }

    [UserSetting("P3 - Apocalypse MT/ST priority swap")]
    public static bool ApoTankPriorSwap { get; set; } = false;

    [UserSetting("P3 - Apocalypse Strategy")]
    public static ApoStgEnum ApoStg { get; set; } = ApoStgEnum.CrowdFirst;
    public enum ApoStgEnum
    {
        CrowdFirst,
        CrownFirst,
    }

    [UserSetting("Command Mode (Master Switch)")]
    public static bool CaptainMode { get; set; } = false;

    [UserSetting("Command Mode - Enable [P1 Utopian Sky] command")]
    public static bool UosCaptainMode { get; set; } = false;

    [UserSetting("Command Mode - Enable [P1 Fall of Faith (DB Idle-Fixed)] command")]
    public static bool FofCaptainMode { get; set; } = false;

    [UserSetting("Command Mode - Enable [P3 Apocalypse Grouping] command")]
    public static bool ApoCaptainMode { get; set; } = false;

    private static readonly Random Random = new();          // For random testing
    private int rdTarget = -1;                              // For random testing
    private volatile List<bool> _recorded = new bool[20].ToList();      // Recorded flags
    private static List<ManualResetEvent> _events = Enumerable
        .Range(0, 20)
        .Select(_ => new ManualResetEvent(false))
        .ToList();

    private enum FruPhase
    {
        Init,
        P1A_UtopianSky,             // P1A Utopian Sky
        P1B_FallOfFaith,            // P1B Fall of Faith
        P1C_BurntStrike,            // P1C Burnt Strike
        P2A_DiamondDust,            // P2A Diamond Dust
        P2B_Mirror,                 // P2B Mirror, Mirror
        P2C_LightRampant,           // P2B Light Rampant
        P2D_AbsoluteZero,           // P2C Absolute Zero
        P3A_UltimateRelativity,     // P3A Ultimate Relativity
        P3B_Apocalypse,             // P3B Apocalypse
        P4A_DarklitDragonsong,      // P4A Darklit Dragonsong
        P4B_CrystallizeTime,        // P4B Crystallize Time
        P5A_FulgentBlade,           // P5A Fulgent Blade
        P5B_ParadiseRegained,       // P5B Paradise Regained
        P5C_PolarizingStrike,       // P5C Polarizing Strike
    }

    private static readonly Vector3 Center = new Vector3(100, 0, 100);
    private FruPhase _fruPhase = FruPhase.Init;
    private static PriorityDict _pd = new PriorityDict();
    private static Counter _ct = new Counter();
    private static Apocalypse _apo = new Apocalypse();
    private static UlRelativity _ulr = new UlRelativity();
    
    private static List<ulong> _fragTargets = [];

    private const int Mt = 0;
    private const int St = 1;
    private const int H1 = 2;
    private const int H2 = 3;
    private const int D1 = 4;
    private const int D2 = 5;
    private const int D3 = 6;
    private const int D4 = 7;

    private const ulong FragmentDataId = 17841;

    public void Init(ScriptAccessory accessory)
    {
        _fruPhase = FruPhase.Init;

        rdTarget = -1;
        _recorded = new bool[20].ToList();
        _events = Enumerable
            .Range(0, 20)
            .Select(_ => new ManualResetEvent(false))
            .ToList();

        accessory.DebugMsg($"Init {Name} v{Version}{DebugVersion} Success.\n{UpdateInfo}", DebugMode);
        accessory.Method.MarkClear();
        LocalMarkClear(accessory);
        accessory.Method.RemoveDraw(".*");
    }

    [ScriptMethod(name: "General Debug Use", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo"], userControl: false)]
    public void EchoDebugActive(Event @event, ScriptAccessory accessory)
    {
        if (!DebugMode) return;
        var msg = @event.Message();
        switch (msg)
        {
            case "=TST":
                accessory.DebugMsg($"Debug action.", DebugMode);
                break;

            case "=CLEAR":
                accessory.DebugMsg($"Deleting drawings and local waymarks.", DebugMode);
                LocalMarkClear(accessory);
                accessory.Method.RemoveDraw(".*");
                break;
        }
    }

    #region P1 Fatebreaker

    [ScriptMethod(name: "---- ã€ŠP1: Fatebreakerã€‹ ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["ActionId:Hello1aya2World"],
        userControl: true)]
    public void SplitLine_FateBreaker(Event @event, ScriptAccessory accessory)
    {
        if (!DebugMode) return;
        accessory.DebugMsg($"debug how did someone click this", DebugMode);
        LocalMarkClear(accessory);
        accessory.Method.RemoveDraw(".*");
    }

    #region P1.1 Utopian Sky

    [ScriptMethod(name: "Utopian Sky Phase Change", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4015[45])$"],
        userControl: Debugging)]
    public void UtopianSkyPhaseChange(Event @event, ScriptAccessory accessory)
    {
        _fruPhase = FruPhase.P1A_UtopianSky;
        accessory.DebugMsg($"Current phase: {_fruPhase}", DebugMode);
        // Initialize priority class
        _pd.Init(accessory, "Utopian Sky");
        // TN on top, DPS on bottom, MT/D1 responsible for swapping
        _pd.AddPriorities([3, 0, 1, 2, 4, 5, 6, 7]);
    }

    [ScriptMethod(name: "Utopian Sky Stack Marker", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(00F9)$"],
        userControl: true)]
    public void UtopianSkyStackMark(Event ev, ScriptAccessory sa)
    {
        /*
         * Attack 1: Tethered, upper half
         * Attack 2: Tethered, lower half
         * Stop 1: Only appears on D1, if marked, means go to upper half
         * Stop 2: Only appears on MT, if marked, means go to lower half
         * Initial priority setup: _pd.AddPriorities([3, 0, 1, 2, 4, 5, 6, 7]);
         * When a target is tethered, their priority +10.
         */
        if (_fruPhase != FruPhase.P1A_UtopianSky) return;
        var tid = ev.TargetId;
        var tidx = sa.GetPlayerIdIndex(tid);
        sa.Log.Debug($"Utopian Sky {ev.Id}, {ev.Id0()}");
        lock (_pd)
        {
            _pd.Priorities[tidx] += 10;
            _pd.AddActionCount();
        }
        if (!_pd.IsActionCountEqualTo(2)) return;
        _pd.ShowPriorities();

        // Sorted ascending, indices 0-2 idle on top, 3-5 idle on bottom, 6 tethered on top, 7 tethered on bottom.
        {
            var upTetherTarget = _pd.SelectSpecificPriorityIndex(6);
            var downTetherTarget = _pd.SelectSpecificPriorityIndex(7);
            // Waymark
            MarkPlayerByIdx(sa, upTetherTarget.Key, MarkType.Attack1, UosCaptainMode);
            MarkPlayerByIdx(sa, downTetherTarget.Key, MarkType.Attack2, UosCaptainMode);
            // Send Debug info
            var str = "\n";
            str += $"Up: {upTetherTarget.Key} ({sa.GetPlayerJobByIndex(upTetherTarget.Key)})\n";
            str += $"Down: {downTetherTarget.Key} ({sa.GetPlayerJobByIndex(downTetherTarget.Key)})\n";
            sa.DebugMsg(str, DebugMode);
        }

        var remainPlayers = _pd.SelectSmallPriorityIndices(6);
        // Swap check, only need to check indices 2 or 3
        if (remainPlayers[2].Key == D1)
        {
            // Condition met, meaning D1 is on top.
            var str = $"Player {remainPlayers[2].Key} ({sa.GetPlayerJobByIndex(remainPlayers[2].Key)} needs to swap.)";
            sa.DebugMsg(str, DebugMode);
            MarkPlayerByIdx(sa, remainPlayers[2].Key, MarkType.Stop1, UosCaptainMode);
        }

        if (remainPlayers[3].Key == Mt)
        {
            // Condition met, meaning MT is on bottom.
            var str = $"Player {remainPlayers[3].Key} ({sa.GetPlayerJobByIndex(remainPlayers[3].Key)} needs to swap.)";
            sa.DebugMsg(str, DebugMode);
            MarkPlayerByIdx(sa, remainPlayers[3].Key, MarkType.Stop2, UosCaptainMode);
        }

        // If subsequent guidance is needed, remainPlayers can be called directly, Key represents role index, Value is priority.
    }

    #endregion P1.1 Utopian Sky

    #region P1.2 Fall of Faith

    [ScriptMethod(name: "Burnished Glory Phase Change (DEBUG ONLY)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40170)$"],
        userControl: Debugging)]
    public void BurnishedGloryPhaseChange(Event @event, ScriptAccessory accessory)
    {
        _fruPhase = _fruPhase switch
        {
            FruPhase.P1B_FallOfFaith => FruPhase.P1C_BurntStrike,
            _ => FruPhase.P1B_FallOfFaith
        };
        accessory.DebugMsg($"Current phase: {_fruPhase}", DebugMode);

        // Delete waymarks from Utopian Sky and Fall of Faith
        MarkClear(accessory);

        // Set up Fall of Faith settings
        if (_fruPhase != FruPhase.P1B_FallOfFaith) return;
        // Initialize priority class
        _pd.Init(accessory, "Fall of Faith");

        // Waymark priority setup
        List<int> priority = FallOfFaithPriority switch
        {
            FallOfFaithPriorityEnum.T_H_D => [0, 1, 2, 3, 4, 5, 6, 7],
            FallOfFaithPriorityEnum.H_T_D => [2, 3, 0, 1, 4, 5, 6, 7],
            FallOfFaithPriorityEnum.H_T_D_H => [1, 2, 0, 7, 3, 4, 5, 6],
            _ => [2, 3, 0, 1, 4, 5, 6, 7],
        };

        _pd.AddPriorities(priority);
    }

    [ScriptMethod(name: "Fall of Faith Marker & Guidance (DB Idle-Fixed)", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(00F9|011F)$"],
        userControl: true)]
    public void FallOfFaithMarkAndGuidance(Event ev, ScriptAccessory sa)
    {
        /*
         * Personal use, DB Idle-Fixed
         * Divided into Lightning group, Fire group, Idle group
         * Lightning: +10, +20, +30, +40 (n)
         * Fire: +80, +70, +60, +50 (8-n)
         * Odd: +0
         * Even: +100
         * Sorted ascending, waymarks Bind1, Stop1, Bind2, Stop2 (Left, Top-Left, Right, Top-Right respectively)
         */
        // const uint fire = 0x00F9;
        const uint lightning = 0x011F;

        if (_fruPhase != FruPhase.P1B_FallOfFaith) return;
        var tid = ev.TargetId;
        var tidx = sa.GetPlayerIdIndex(tid);
        var tetherType = ev.Id0();
        sa.Log.Debug($"Fall of Faith {ev.Id}, {ev.Id0()}");

        // Guidance based on myIndex can be done later
        var myIndex = sa.GetMyIndex();

        lock (_pd)
        {
            // +1 when tether appears
            _pd.AddActionCount();
            sa.Log.Debug($"Fall of Faith Tether count {_pd.ActionCount} tethered to {sa.GetPlayerJobByIndex(tidx)}({tidx}) is {(tetherType == lightning ? "Lightning" : "Fire")} tether");

            // Add corresponding value based on comments
            var addNum = _pd.ActionCount % 2 == 0 ? 100 : 0;
            addNum += 10 * (tetherType == lightning ? _pd.ActionCount : 9 - _pd.ActionCount);
            _pd.Priorities[tidx] += addNum;

            switch (_pd.ActionCount)
            {
                case 1:
                    MarkPlayerByIdx(sa, tidx, tetherType == lightning ? MarkType.Bind1 : MarkType.Stop1, FofCaptainMode);
                    if (tidx == myIndex) FallOfFaithGuidance(sa, tetherType == lightning ? 4 : 5);
                    break;
                case 2:
                    MarkPlayerByIdx(sa, tidx, tetherType == lightning ? MarkType.Bind2 : MarkType.Stop2, FofCaptainMode);
                    if (tidx == myIndex) FallOfFaithGuidance(sa, tetherType == lightning ? 6 : 7);
                    break;
                case 3:
                    // 5 idle players, subtract 1 to remove offset
                    MarkPlayerByIdx(sa, tidx, (_pd.FindPriorityIndexOfKey(tidx) - 1) % 2 == 0 ? MarkType.Bind1 : MarkType.Stop1, FofCaptainMode);
                    if (tidx == myIndex) FallOfFaithGuidance(sa, (_pd.FindPriorityIndexOfKey(tidx) - 1) % 2 == 0 ? 4 : 5);
                    break;
                case 4:
                    // 4 idle players
                    MarkPlayerByIdx(sa, tidx, _pd.FindPriorityIndexOfKey(tidx) % 2 == 0 ? MarkType.Bind2 : MarkType.Stop2, FofCaptainMode);
                    if (tidx == myIndex) FallOfFaithGuidance(sa, _pd.FindPriorityIndexOfKey(tidx) % 2 == 0 ? 6 : 7);
                    break;
            }
        }

        if (!_pd.IsActionCountEqualTo(4)) return;
        _pd.ShowPriorities();

        Thread.MemoryBarrier();

        // indices 0~3 idle, indices 4~7, can waymark Bind1, Stop1, Bind2, Stop2 (Left, Top-Left, Right, Top-Right respectively)
        MarkPlayerByIdx(sa, _pd.SelectSpecificPriorityIndex(0).Key, MarkType.Attack1, FofCaptainMode);
        MarkPlayerByIdx(sa, _pd.SelectSpecificPriorityIndex(1).Key, MarkType.Attack2, FofCaptainMode);
        MarkPlayerByIdx(sa, _pd.SelectSpecificPriorityIndex(2).Key, MarkType.Attack3, FofCaptainMode);
        MarkPlayerByIdx(sa, _pd.SelectSpecificPriorityIndex(3).Key, MarkType.Attack4, FofCaptainMode);

        var myPriority = _pd.FindPriorityIndexOfKey(myIndex);

        // Lightning/Fire tether swap hint. Take the tens digit of the priority value, if difference is 1, no swap; if difference is 2, swap.
        if (myPriority >= 4)
        {
            var myPriVal = _pd.SelectSpecificPriorityIndex(myPriority).Value;
            var ptPriority = myPriority % 2 == 0 ? myPriority + 1 : myPriority - 1;
            var ptPriVal = _pd.SelectSpecificPriorityIndex(ptPriority).Value;
            var subtract = Math.Abs(myPriVal / 10 % 10 - ptPriVal / 10 % 10);
            sa.DebugMsg($"Priority (({myPriority}){myPriVal} - ({ptPriority}){ptPriVal}) / 10 % 10 = {subtract}", DebugMode);
            if (subtract != 2) return;
            FallOfFaithGuidance(sa, myPriority, true);
            sa.Method.TextInfo($"Tethered players, prepare to swap", 3000, true);
        }
        else
        {
            // Idle player guidance
            FallOfFaithGuidance(sa, myPriority);
        }
    }

    private void FallOfFaithGuidance(ScriptAccessory accessory, int priority, bool swapHint = false)
    {
        const int lightLeft = 4;
        const int fireLeft = 5;
        const int lightRight = 6;
        const int fireRight = 7;

        const int freeLeftMiddle = 0;
        const int freeLeftBottom = 1;
        const int freeRightBottom = 2;
        const int freeRightMiddle = 3;

        // baseMiddlePos is center-left coordinate, bias is offset for up, down, left.
        var baseMiddlePos = new Vector3(95, 0, 100);
        const float bias = 2f;

        var tpos = priority switch
        {
            lightLeft => baseMiddlePos,
            lightRight => baseMiddlePos.FoldPointHorizon(Center.X),
            freeLeftMiddle => baseMiddlePos - new Vector3(bias / 2, 0, 0),
            freeRightMiddle => (baseMiddlePos - new Vector3(bias / 2, 0, 0)).FoldPointHorizon(Center.X),

            fireLeft => baseMiddlePos - new Vector3(0, 0, bias),
            fireRight => (baseMiddlePos - new Vector3(0, 0, bias)).FoldPointHorizon(Center.X),
            freeLeftBottom => (baseMiddlePos - new Vector3(0, 0, bias)).FoldPointVertical(Center.Z),
            freeRightBottom => (baseMiddlePos - new Vector3(0, 0, bias)).FoldPointHorizon(Center.X).FoldPointVertical(Center.Z),

            _ => baseMiddlePos,
        };

        if (!swapHint)
        {
            var dp = accessory.DrawGuidance(tpos, 0, 20000, $"Usami-FallOfFaithInitPriority{priority}Guidance");
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        else
        {
            var swapPos = priority switch
            {
                fireLeft => baseMiddlePos,
                fireRight => baseMiddlePos.FoldPointHorizon(Center.X),
                lightLeft => baseMiddlePos - new Vector3(0, 0, bias),
                lightRight => (baseMiddlePos - new Vector3(0, 0, bias)).FoldPointHorizon(Center.X),
                _ => baseMiddlePos,
            };

            var dp = accessory.DrawGuidance(tpos, swapPos, 0, 20000, $"Usami-FallOfFaithSwapPriority{priority}Guidance", isSafe: false);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
    }

    [ScriptMethod(name: "Fall of Faith Marker/Guidance Removal (DEBUG ONLY)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40141"],
        userControl: Debugging, suppress: 10000)]
    public void FallOfFaithRemove(Event @event, ScriptAccessory accessory)
    {
        if (_fruPhase != FruPhase.P1B_FallOfFaith) return;
        MarkClear(accessory);
        accessory.Method.RemoveDraw($"Usami-FallOfFaith.*");
    }

    #endregion Fall of Faith

    #endregion P1 Fatebreaker

    #region P2 Shiva

    [ScriptMethod(name: "---- ã€ŠP2: ShivaÂ·Mitronã€‹ ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["ActionId:Hello1aya2World"],
        userControl: true)]
    public void SplitLine_Shiva(Event @event, ScriptAccessory accessory)
    {
    }

    #region P2.1 Diamond Dust

    [ScriptMethod(name: "Diamond Dust Phase Change (DEBUG ONLY)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40197)$"],
        userControl: Debugging)]
    public void DiamondDustPhaseChange(Event @event, ScriptAccessory accessory)
    {
        _fruPhase = FruPhase.P2A_DiamondDust;
        accessory.DebugMsg($"Current phase: {_fruPhase}", DebugMode);
    }

    #endregion P2.1 Diamond Dust

    #region P2.2 Mirror

    [ScriptMethod(name: "Mirror, Mirror Phase Change (DEBUG ONLY)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40179)$"],
        userControl: Debugging)]
    public void MirrorPhaseChange(Event @event, ScriptAccessory accessory)
    {
        _fruPhase = FruPhase.P2B_Mirror;
        accessory.DebugMsg($"Current phase: {_fruPhase}", DebugMode);
    }

    #endregion P2.2 Mirror

    #region P2.3 Light Rampant

    [ScriptMethod(name: "Light Rampart Phase Change (DEBUG ONLY)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40212)$"],
        userControl: Debugging)]
    public void LightRampartPhaseChange(Event @event, ScriptAccessory accessory)
    {
        _fruPhase = FruPhase.P2C_LightRampant;
        _pd.Init(accessory, "Light Rampant");

        List<int> priorities = LightRampageStg switch
        {
            LightRampageStgEnum.Grey9 => [20, 42, 2, 24, 13, 33, 11, 31],
            LightRampageStgEnum.Hexagram => [0, 1, 2, 3, 10, 11, 12, 13],
            _ => [20, 42, 2, 24, 13, 33, 11, 31]
        };

        // Grey9 puddle placement
        /* Lower number on left, higher number on right
        *    00 10 20 30 40
        * 0        MT
        * 1     D3    D4
        * 2  H1          ST 
        * 3     D1    D2
        * 4        H2
        */

        // Hexagram puddle placement
        /* Lower number on bottom-left, higher number on top-right
        *       00  01  02  03
        * 00        ST  H1
        * 00    MT          H2
        * 10    D1          D4
        * 10        D2  D3
        */

        _pd.AddPriorities(priorities);
        accessory.DebugMsg($"Current phase: {_fruPhase}", DebugMode);
    }

    [ScriptMethod(name: "Light Rampant Puddle Placement", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(0177)$"],
        userControl: true)]
    public void LuminousHammerGuidance(Event @event, ScriptAccessory accessory)
    {
        if (_fruPhase != FruPhase.P2C_LightRampant) return;
        lock (_pd)
        {
            var tid = @event.TargetId;
            var tidx = accessory.GetPlayerIdIndex(tid);
            _pd.AddActionCount();
            _pd.AddPriority(tidx, 100);
        }

        if (_pd.ActionCount != 2) return;
        _pd.ShowPriorities();

        // +100 pushes priority values to the end, smaller on left/bottom, larger on right/top
        var tLeft = _pd.SelectSpecificPriorityIndex(6).Key;
        var tRight = _pd.SelectSpecificPriorityIndex(7).Key;
        var myIndex = accessory.GetMyIndex();
        if ((myIndex != tLeft) && (myIndex != tRight)) return;

        // Here leftRoute refers to left/up path
        List<Vector3> leftRoute = LightRampageStg switch
        {
            LightRampageStgEnum.Grey9 =>
                [
                    new Vector3(92, 0, 100),
                    new Vector3(94.6f, 0, 94),
                    new Vector3(100, 0, 92),
                    new Vector3(105.6f, 0, 92),
                    new Vector3(111.3f, 0, 88.7f)
                ],
            LightRampageStgEnum.Hexagram =>
                [
                    new Vector3(100, 0, 92).RotatePoint(Center, 22.5f.DegToRad()),
                    new Vector3(100, 0, 92).RotatePoint(Center, 67.5f.DegToRad()),
                    new Vector3(100, 0, 92).RotatePoint(Center, 112.5f.DegToRad()),
                    new Vector3(100, 0, 92).RotatePoint(Center, 157.5f.DegToRad()),
                    new Vector3(100, 0, 92).RotatePoint(Center, 157.5f.DegToRad()) + new Vector3(0, 0, 8.5f),
                ],
            _ =>
                [
                    new Vector3(92, 0, 100),
                    new Vector3(94.6f, 0, 94),
                    new Vector3(100, 0, 92),
                    new Vector3(105.6f, 0, 92),
                    new Vector3(111.3f, 0, 88.7f)
                ],
        };

        List<Vector3> rightRoute =
        [
            leftRoute[0].PointCenterSymmetry(Center),
            leftRoute[1].PointCenterSymmetry(Center),
            leftRoute[2].PointCenterSymmetry(Center),
            leftRoute[3].PointCenterSymmetry(Center),
            leftRoute[4].PointCenterSymmetry(Center),
        ];

        for (var i = 0; i < 4; i++)
        {
            var dp1 = accessory.DrawGuidance(leftRoute[i], leftRoute[i + 1], 0, 14000, $"Usami-LightRampantLeft{i}{i + 1}", isSafe: myIndex == tLeft);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp1);
            var dp2 = accessory.DrawGuidance(rightRoute[i], rightRoute[i + 1], 0, 14000, $"Usami-LightRampantRight{i}{i + 1}", isSafe: myIndex == tRight);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp2);
        }

        var dp = accessory.DrawGuidance(myIndex == tLeft ? leftRoute[0] : rightRoute[0], 0, 8000, $"Usami-LightRampantInit");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

    }

    [ScriptMethod(name: "Light Rampant Orb Explosion Time", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40219)$"],
        userControl: true)]
    public void LightBalloonExplode(Event ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P2C_LightRampant) return;
        var sid = ev.SourceId;
        var dp = sa.DrawCircle(sid, 11, 2500, 2500, $"LightOrb{sid}", true);
        dp.Color = ColorHelper.ColorRed.V4.WithW(4f);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    #endregion P2.3 Light Rampant

    #region P2.4 Absolute Zero

    [ScriptMethod(name: "Absolute Zero Phase Change (DEBUG ONLY)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40224)$"],
        userControl: Debugging)]
    public void AbsoluteZeroPhaseChange(Event @event, ScriptAccessory accessory)
    {
        _fruPhase = FruPhase.P2D_AbsoluteZero;
        accessory.DebugMsg($"Current phase: {_fruPhase}", DebugMode);
    }

    [ScriptMethod(name: "Dark Crystal Untargetable", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17828"],
        userControl: true)]
    public async void DarkCrystalUntargetable(Event ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P2D_AbsoluteZero) return;
        var sid = ev.SourceId;
        await Task.Delay(500);
        sa.SetTargetable(sa.GetById(sid), false);
    }

    #endregion P2.4 Absolute Zero

    #endregion P2 Shiva

    #region P3 Gaia

    [ScriptMethod(name: "---- ã€ŠP3: Oracle of Darknessã€‹ ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["ActionId:Hello1aya2World"],
        userControl: true)]
    public void SplitLine_Gaia(Event ev, ScriptAccessory accessory)
    {
    }

    #region P3.1 Ultimate Relativity

    [ScriptMethod(name: "Ultimate Relativity Phase Change (DEBUG ONLY)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40266)$"],
        userControl: Debugging)]
    public void UlrPhaseChange(Event ev, ScriptAccessory sa)
    {
        _fruPhase = FruPhase.P3A_UltimateRelativity;
        sa.DebugMsg($"Current phase: {_fruPhase}", DebugMode);
        _ct.Init(sa, "Ultimate Relativity");
        _pd.Init(sa, "Ultimate Relativity");
        _ulr.Init(sa, _pd, _ct);
        _pd.AddPriorities([0, 1, 2, 3, 104, 105, 106, 107]);
        _events = Enumerable
            .Range(0, 20)
            .Select(_ => new ManualResetEvent(false))
            .ToList();
    }

    [ScriptMethod(name: "Print Ultimate Relativity Info (DEBUG ONLY)", eventType: EventTypeEnum.NpcYell, eventCondition: ["ActionId:Hello1aya2World"],
        userControl: Debugging)]
    public void UlrMessagePrint(Event ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P3B_Apocalypse) return;
        _ulr.ShowMessage();
    }

    [ScriptMethod(name: "Dark Fire Type Record (DEBUG ONLY)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(2455|2462)$"],
        userControl: Debugging)]
    public void UlrDarkFireTypeRecord(Event ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P3A_UltimateRelativity) return;

        // 11s, 21s, 31s
        const uint fire = 2455;
        const uint ice = 2462;
        const uint fireShort = 11000 - 2000;
        const uint fireMid = 21000 - 2000;
        const uint fireLong = 31000 - 2000;

        var dur = ev.DurationMilliseconds();
        var tidx = sa.GetPlayerIdIndex(ev.TargetId);
        var stid = ev.StatusId;
        sa.Log.Debug($"Detected {sa.GetPlayerJobByIndex(tidx)}'s {(stid == fire ? "Fire" : "Ice")} {dur}");

        lock (_ulr)
        {
            if (stid == fire)
            {
                if (dur > fireShort)
                    _pd.AddPriority(tidx, 10);
                if (dur > fireMid)
                    _pd.AddPriority(tidx, 10);
                if (dur > fireLong)
                    _pd.AddPriority(tidx, 10);
            }
            else
            {
                // TN considered short fire, DPS considered long fire
                _pd.AddPriority(tidx, tidx <= 3 ? 10 : 30);
            }
        }
    }

    [ScriptMethod(name: "Hourglass Tether Record (DEBUG ONLY)", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(0085|0086)$"],
        userControl: Debugging)]
    public void UlrHourglassTetherRecord(Event ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P3A_UltimateRelativity) return;
        const uint fast = 0x86;
        const uint slow = 0x85;
        lock (_ulr)
        {
            var spos = ev.SourcePosition;
            var sdir = spos.Position2Dirs(Center, 8);
            _ulr.TetherDirection[sdir] = ev.Id0() == fast ? 1 : 2;
            sa.Log.Debug($"Detected hourglass at direction {sdir}, ev.Id = {ev.Id} ev.Id0 = {ev.Id0()}, {(ev.Id0() == fast ? "Fast" : "Slow")}");
            _ct.AddCounter();

            if (_ct.Number != 5) return;
            sa.Log.Debug($"5 tethers recorded, starting to find Relative North.");
            _ulr.BuildTrueDirection();
        }
    }

    [ScriptMethod(name: "Auto Face During Stun", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4163"],
        userControl: true, suppress: 10000)]
    public void UlrAutoFace(Event ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P3A_UltimateRelativity) return;
        var myDir = _ulr.GetDirection(sa.GetMyIndex());
        sa.Log.Debug($"Stunned, triggering auto-face.");
        sa.SetRotation(sa.Data.MyObject, (myDir * 45f).DegToRad().Game2Logic());
    }

    public class UlRelativity
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public ScriptAccessory sa { get; set; } = null!;
        public int RelativeNorth { get; set; } = -1;
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public PriorityDict pd { get; set; } = null!;
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public Counter ct { get; set; } = null!;
        public List<int> TetherDirection { get; set; } = Enumerable.Repeat(0, 8).ToList();  // 0 none, 1 fast, 2 slow
        public List<int> RelativeDirection { get; set; } = [4, 6, 5, 3, 7, 1, 2, 0];
        public List<int> TrueDirection { get; set; } = Enumerable.Repeat(0, 8).ToList();
        public void Init(ScriptAccessory accessory, PriorityDict priorityDict, Counter counter)
        {
            sa = accessory;
            pd = priorityDict;
            ct = counter;
            TetherDirection = Enumerable.Repeat(0, 8).ToList();
            TrueDirection = Enumerable.Repeat(0, 8).ToList();
            RelativeNorth = -1;
        }

        public void BuildTrueDirection()
        {
            // Find the slow hourglass
            var slowTetherIdx = TetherDirection.IndexOf(2, 0);
            var checkIdx1 = (slowTetherIdx - 2 + 8) % 8;
            var checkIdx2 = (slowTetherIdx + 2 + 8) % 8;
            RelativeNorth = TetherDirection[checkIdx1] == 1 ? checkIdx1 : checkIdx2;
            sa.Log.Debug($"Calculated Relative North from slow tether {slowTetherIdx}, checking {checkIdx1} and {checkIdx2}, got {RelativeNorth} as Relative North");

            for (int i = 0; i < 8; i++)
            {
                var jobIdx = pd.SelectSpecificPriorityIndex(i).Key;
                var dir = (RelativeDirection[i] + RelativeNorth) % 8;
                TrueDirection[jobIdx] = dir;
                sa.Log.Debug($"Sequence {i}, {sa.GetPlayerJobByIndex(jobIdx)}({jobIdx}), needs to go to direction {dir}.");
            }
        }

        public void ShowMessage()
        {
            var str = "\n ---- [Ultimate Relativity] ----\n";
            str += $"Relative North: {RelativeNorth}.\n";
            str += $"Positions per role: {sa.BuildListStr(TrueDirection)}";

            sa.Log.Debug(str);
        }

        public int GetDirection(int jobIdx)
        {
            return TrueDirection[jobIdx];
        }

    }

    #endregion P3.1 Ultimate Relativity

    #region P3.2 Apocalypse

    [ScriptMethod(name: "Print Apocalypse Info (DEBUG ONLY)", eventType: EventTypeEnum.NpcYell, eventCondition: ["ActionId:Hello1aya2World"],
            userControl: Debugging)]
    public void ApoMessagePrint(Event @ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P3B_Apocalypse) return;
        _apo.ShowMessage();
    }

    [ScriptMethod(name: "Apocalypse Phase Change (DEBUG ONLY)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40269)$"],
        userControl: Debugging)]
    public void ApoPhaseChange(Event @ev, ScriptAccessory sa)
    {
        _fruPhase = FruPhase.P3B_Apocalypse;
        _pd.Init(sa, "Apocalypse");
        _ct.Init(sa, "Apocalypse External Waymarks");
        MarkClear(sa);
        _apo.Init(sa, _pd);
        // Recommend enabling if ST is baiting the super jump
        _pd.AddPriorities(ApoTankPriorSwap ? [1, 0, 2, 3, 7, 6, 5, 4] : [0, 1, 2, 3, 7, 6, 5, 4]);    // Initial THD priority
        _events = Enumerable
            .Range(0, 20)
            .Select(_ => new ManualResetEvent(false))
            .ToList();
        sa.DebugMsg($"Current phase: {_fruPhase}", DebugMode);
    }

    [ScriptMethod(name: "Dark Water Type Record (DEBUG ONLY)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2461"], userControl: Debugging)]
    public void DarkWaterTypeRecord(Event ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P3B_Apocalypse) return;
        var dur = ev.DurationMilliseconds();
        var tidx = sa.GetPlayerIdIndex(ev.TargetId);

        // 10s, 29s, 38s
        const uint waterShort = 10000 - 2000;
        const uint waterMid = 29000 - 2000;
        const uint waterLong = 38000 - 2000;

        lock (_pd)
        {
            if (dur > waterShort)
                _pd.AddPriority(tidx, 10);
            if (dur > waterMid)
                _pd.AddPriority(tidx, 10);
            if (dur > waterLong)
                _pd.AddPriority(tidx, 10);
            _pd.AddActionCount();

            if (_pd.ActionCount == 6)
            {
                _apo.Grouping();
            }
        }
    }

    [ScriptMethod(name: "Apocalypse External Waymark Detection", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:regex:^(0[1234678]|11)$"],
        userControl: Debugging)]
    public void DarkWaterMarkerFromOut(Event ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P3B_Apocalypse) return;
        if (CaptainMode && ApoCaptainMode) return;
        _events[(int)EventIdx.ApoGrouping].WaitOne();

        lock (_apo)
        {
            _ct.AddCounter();
            var mark = ev.Id0();
            var tid = ev.TargetId;
            var tidx = sa.GetPlayerIdIndex(tid);

            var groupIdx = mark switch
            {
                0x1 => 0,     // Atk1
                0x2 => 2,     // Atk2
                0x3 => 4,     // Atk3
                0x4 => 6,     // Atk4
                0x6 => 1,     // Bind1
                0x7 => 3,     // Bind2
                0x8 => 5,     // Bind3
                0x11 => 7,    // Square
                _ => 0,
            };

            sa.DebugMsg($"Detected external waymark {mark} on player {sa.GetPlayerJobByIndex(tidx)}", DebugMode);

            // Directly modify definition, priority values become meaningless now
            _apo.TempGroup[groupIdx] = new KeyValuePair<int, int>(tidx, 0);

            // If not 8 waymarks, invalidate.
            if (_ct.Number != 8) return;
            sa.DebugMsg($"Detected 8 external waymarks, overriding _apo.Group grouping logic.", DebugMode);
            _apo.Group = [.. _apo.TempGroup];
            _events[(int)EventIdx.ApoPreciseGrouping].Set();
            _apo.GroupingFixed = true;
        }
    }

    [ScriptMethod(name: "Dark Water Range Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2461"], userControl: true)]
    public void DarkWaterRange(Event ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P3B_Apocalypse) return;
        // Determine grouping internally -> check if precise grouping is complete -> range hint
        _events[(int)EventIdx.ApoGrouping].WaitOne();
        _events[(int)EventIdx.ApoPreciseGrouping].WaitOne(1000);

        var tid = ev.TargetId;
        var tidx = sa.GetPlayerIdIndex(tid);
        var dur = ev.DurationMilliseconds();

        var isSameGroup = _apo.GetMyGroup() == _apo.GetPlayerGroup(tidx);
        var delayed = !_apo.GroupingFixed;

        var dp = sa.DrawCircle(tid, 6, (int)dur - 3000 - (delayed ? 1000 : 0), 3000, $"Usami-RagingWater{tidx}", true);
        dp.Color = isSameGroup ? sa.Data.DefaultSafeColor : sa.Data.DefaultDangerColor;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Group Swap Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40270"],
     userControl: true)]
    public void DarkWaterSwapHint(Event @ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P3B_Apocalypse) return;
        // Determine grouping internally -> check if precise grouping is complete -> swap hint
        _events[(int)EventIdx.ApoGrouping].WaitOne();
        _events[(int)EventIdx.ApoPreciseGrouping].WaitOne(1000);

        var myIndex = sa.GetMyIndex();
        var myPreviousGroup = myIndex < 4 ? 0 : 1;     // 0 Left, 1 Right
        var myCurrentGroup = _apo.GetMyGroup();

        var dp = sa.DrawGuidance(_apo.IsInLeftGroup(myIndex) ? new Vector3(93, 0, 100) : new Vector3(107, 0, 100), 0, 3000, $"Usami-RagingWaterPosition");
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        var str = _apo.GroupingFixed ? "According to waymarks" : ApoTankPriorSwap ? "According to priority (ST>MT) low swaps" : "According to priority low swaps";
        sa.Method.TextInfo($"{(myPreviousGroup == myCurrentGroup ? "No Swap" : "Swap")}({str})", 3000, true);
        sa.DebugMsg($"{(myPreviousGroup == myCurrentGroup ? "No Swap" : "Swap")}({str})", DebugMode);
    }

    [ScriptMethod(name: "Spirit Taker Range & Spread Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40288"], userControl: true)]
    public void SpiritTakerHint(Event @ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P3B_Apocalypse) return;
        var myIndex = sa.GetMyIndex();
        for (int i = 0; i < 8; i++)
        {
            var dp = sa.DrawCircle(sa.Data.PartyList[i], 5, 1000, 2000, $"Usami-SpiritTakerRange");
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        // Left group gathering point
        Vector3 leftCenter = new(92, 0, 100);
        Vector3 rightCenter = leftCenter.FoldPointHorizon(Center.X);
        List<Vector3> spreadTargetPos = Enumerable.Repeat(new Vector3(0, 0, 0), 20).ToList();

        List<float> rot = [0f, 0f, 180f, 180f, -60f, -60f, -120f, -120f];

        for (int i = 0; i < 8; i += 2)
        {
            spreadTargetPos[i] = leftCenter.ExtendPoint(rot[i].DegToRad(), 8f);
            spreadTargetPos[i + 1] = spreadTargetPos[i].PointCenterSymmetry(Center);
        }

        sa.DebugMsg($"Am I in left group? {_apo.IsInLeftGroup(myIndex)}, Am I in right group? {_apo.IsInRightGroup(myIndex)}", DebugMode);

        for (int i = 0; i < 8; i++)
        {
            // Considering potential pre-positioning issues, discard specific path spread.
            var isSafe = (_apo.IsInLeftGroup(myIndex) && i % 2 == 0) || (_apo.IsInRightGroup(myIndex) && i % 2 == 1);
            var startPos = spreadTargetPos[i].PointInOutside(i % 2 == 0 ? leftCenter : rightCenter, 6f);
            var dp = sa.DrawGuidance(startPos, spreadTargetPos[i], 0, 3000, $"SpiritTakerSpread", isSafe: isSafe);
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);
        }
    }

    [ScriptMethod(name: "Apocalypse Spread Position Guidance", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40288"], userControl: true, suppress: 10000)]
    public void ApoSpreadGuidance(Event @ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P3B_Apocalypse) return;

        var myIndex = sa.GetMyIndex();
        var myGroupSafePosIdx = _apo.IsInCrowd(myIndex) ? -1 : 0;
        int mySafeDir;
        // Choose crowd safe zone based on player's group.
        if (ApoStg == ApoStgEnum.CrownFirst)
        {
            // If player is in left group, find crown safe zone in the north; else find crown safe zone in the south
            int safeDir = (_apo.SafePoints[1] + 3) % 8;
            int mySafeDirIdx = _apo.IsInLeftGroup(myIndex) ? (safeDir < 4 ? 1 : 3) : (safeDir >= 4 ? 1 : 3);
            if (_apo.IsInCrowd(myIndex))
                mySafeDirIdx -= 1;
            mySafeDir = _apo.SafePoints[mySafeDirIdx];
        }
        else
        {
            // (ApoStg == ApoStgEnum.CrowdFirst)
            // If player is in left group, find crowd safe zone in the north; else find crowd safe zone in the south
            int safeDir = (_apo.SafePoints[0] + 3) % 8;
            int mySafeDirIdx = _apo.IsInLeftGroup(myIndex) ? (safeDir < 4 ? 0 : 2) : (safeDir >= 4 ? 0 : 2);
            if (!_apo.IsInCrowd(myIndex))
                mySafeDirIdx += 1;
            mySafeDir = _apo.SafePoints[mySafeDirIdx];
        }

        if (_apo.GroupingFixed && _apo.IsInCrowd(myIndex))
            myGroupSafePosIdx = _apo.GetMyGroupIdx() / 2 - 1;

        sa.DebugMsg($"Player's safe direction is {mySafeDir}, specific spread position sequence is {myGroupSafePosIdx}.", DebugMode);

        for (int i = 0; i < 4; i++)
        {
            List<Vector3> safePos = _apo.GetSafePos(_apo.SafePoints[i]);
            for (int j = 0; j < (i % 2 == 0 ? 3 : 1); j++)
            {
                bool isSafe = mySafeDir == _apo.SafePoints[i];

                if (_apo.GroupingFixed && _apo.IsInCrowd(myIndex))
                    isSafe &= myGroupSafePosIdx == j;

                var dp = sa.DrawStaticCircle(safePos[j], isSafe ? sa.Data.DefaultSafeColor.WithW(3f) : sa.Data.DefaultDangerColor.WithW(3f), 0, 10000, $"Usami-SpreadPosDir{i}-Idx{j}", 0.5f);
                sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

                if (_apo.GroupingFixed && isSafe)
                {
                    var dp0 = sa.DrawGuidance(safePos[j], 0, 10000, $"Usami-SpreadPosDir{i}-Idx{j}Guidance");
                    sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);
                }
            }
        }
    }

    [ScriptMethod(name: "Apocalypse Type Record (DEBUG ONLY)", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:4", "Id2:regex:^(16|64)$"], userControl: Debugging)]
    public void ExaflareTypeRecord(Event ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P3B_Apocalypse) return;

        lock (_apo)
        {
            if (_apo.ActionCount >= 2) return;
            var northApoDir = ev.SourcePosition.Position2Dirs(Center, 8);
            var rot = ev.Id2();
            _apo.AddActionCount();

            // Record north start point and rotation direction
            if ((northApoDir + 3) % 8 < 4)
            {
                _apo.NorthStartPoint = northApoDir;
                _apo.RotationDir = rot == 0x16 ? 1 : -1;
            }

            if (_apo.ActionCount != 2) return;
            _apo.GetSafeDirs();
        }
    }

    [ScriptMethod(name: "Dark Dance Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40181"], userControl: true)]
    public void ApoDarkDanceGuidance(Event ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P3B_Apocalypse) return;
        // Baiting is done from the crown positions, directly take the crown positions for MT and D1 from apo.

        // _apo.SafePoints[1] and [3] are the crown positions, just point the arrow there.
        var tpos1 = new Vector3(100, 0, 80).RotatePoint(Center, (_apo.SafePoints[1] * 45f).DegToRad());
        var tpos2 = new Vector3(100, 0, 80).RotatePoint(Center, (_apo.SafePoints[3] * 45f).DegToRad());

        var isTank = sa.Data.MyObject is { } o && o.IsTank();

        sa.Method.TextInfo(isTank ? $"Prepare to bait" : $"Avoid bait area", 3000, true);

        var dp1 = sa.DrawGuidance(tpos1.PointInOutside(Center, 13f), tpos1, 0, 3000, $"Usami-DarkDanceBaitPos1", scale: 3f, isSafe: false);
        var dp2 = sa.DrawGuidance(tpos2.PointInOutside(Center, 13f), tpos2, 0, 3000, $"Usami-DarkDanceBaitPos2", scale: 3f, isSafe: false);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
        var dp11 = sa.DrawGuidance(tpos1.PointInOutside(Center, 13f), tpos1, 3000, 2000, $"Usami-DarkDanceBaitPos1", scale: 3f, isSafe: isTank);
        var dp22 = sa.DrawGuidance(tpos2.PointInOutside(Center, 13f), tpos2, 3000, 2000, $"Usami-DarkDanceBaitPos2", scale: 3f, isSafe: isTank);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp11);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp22);

        var dp = sa.DrawTargetNearFarOrder(ev.SourceId, 1, false, 8, 8, 3000, 2000, $"Usami-DarkDanceTarget");
        dp.Color = isTank ? sa.Data.DefaultSafeColor : sa.Data.DefaultDangerColor;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Dark Dance Knockback Direction Guidance", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40181", "TargetIndex:1"], userControl: true)]
    public void ApoDarkDanceKnockBackGuidance(Event ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P3B_Apocalypse) return;
        var sid = ev.SourceId;
        // Divide Gaia into left and right
        var isLeft = _apo.IsInLeftGroup(sa.GetMyIndex());
        var dp0 = sa.DrawGuidance(sid, 0, 1500, 3500, $"Usami-KnockbackDirLeft", -155f.DegToRad().Ccw2Cw(), 3f, isSafe: isLeft);
        var dp1 = sa.DrawGuidance(sid, 0, 1500, 3500, $"Usami-KnockbackDirRight", 155f.DegToRad().Ccw2Cw(), 3f, isSafe: !isLeft);
        dp0.Scale = new Vector2(3f, 14f);
        dp1.Scale = new Vector2(3f, 14f);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
    }

    [ScriptMethod(name: "Apocalypse Waymark Removal", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40282"],
        userControl: Debugging)]
    public void ApoMarkerRemove(Event @event, ScriptAccessory accessory)
    {
        if (_fruPhase != FruPhase.P3B_Apocalypse) return;
        MarkClear(accessory);
    }

    public class Apocalypse
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public ScriptAccessory accessory { get; set; } = null!;
        public int ActionCount { get; set; } = 0;
        public int NorthStartPoint { get; set; } = -1;
        public List<int> SafePoints { get; set; } = [-1, -1, -1, -1];
        public int RotationDir { get; set; } = 0; // 1 clockwise, -1 counter-clockwise
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public List<KeyValuePair<int, int>> Group { get; set; } = null!;
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public List<KeyValuePair<int, int>> TempGroup { get; set; } = null!;
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public PriorityDict Priorities { get; set; } = null!;
        public bool GroupingFixed { get; set; } = false;    // Whether spread positions are precise for each player
        public void Init(ScriptAccessory _accessory, PriorityDict _priorities)
        {
            accessory = _accessory;
            Priorities = _priorities;
            ActionCount = 0;
            NorthStartPoint = -1;
            SafePoints = [-1, -1, -1, -1];
            RotationDir = 0;
            GroupingFixed = false;
        }

        public void AddActionCount()
        {
            ActionCount++;
        }

        public void Grouping()
        {
            TempGroup = Priorities.SelectSmallPriorityIndices(8);

            var str = "";
            str += $"Initial Group Left: {TempGroup[0].Key}, {TempGroup[2].Key}, {TempGroup[4].Key}, {TempGroup[6].Key}\n";
            str += $"Initial Group Right: {TempGroup[1].Key}, {TempGroup[3].Key}, {TempGroup[5].Key}, {TempGroup[7].Key}\n";
            accessory.DebugMsg(str, DebugMode);

            if (CaptainMode && ApoCaptainMode)
            {
                // Melee adjustment: if ST goes right, D2 goes left without thought; if D2 goes left, ST goes right without thought.
                // Due to priority swap, "St" in comments refers to the "idle tank" (non-baiting tank)
                // const int groupLeft = 0;
                const int groupRight = 1;
                var freeTank = ApoTankPriorSwap ? Mt : St;
                var freeTankGroup = Priorities.FindPriorityIndexOfKey(freeTank) % 2;
                var d2Group = Priorities.FindPriorityIndexOfKey(D2) % 2;

                if (freeTankGroup == d2Group)
                {
                    // When St and D2 are in the same group (3 melees), a swap is needed.
                    // If St is in the right group, means squeezed out by Mt, D2 needs to swap again with its same-buff partner. D2's index is later, partner's index is earlier.
                    // If St is in the left group, means D2 is squeezed out by Mt, St needs to swap again with its same-buff partner. St's index is earlier, partner's index is later.
                    var targetIdx = freeTankGroup == groupRight ? Priorities.FindPriorityIndexOfKey(D2) : Priorities.FindPriorityIndexOfKey(freeTank);
                    var offset = freeTankGroup == groupRight ? -1 : 1;
                    (TempGroup[targetIdx + offset], TempGroup[targetIdx]) = (TempGroup[targetIdx], TempGroup[targetIdx + offset]);
                }

                // After swap, reprocess priorities to create melee-ranged priority.
                List<bool> inRightGroup = new bool[8].ToList();
                for (int i = 0; i < 8; i++)
                {
                    inRightGroup[TempGroup[i].Key] = i % 2 == groupRight;
                }

                // Remove the tens digit representing Water buff, re-add tens digit for "Left Group" and "Right Group",
                // Change units digit to MT-ST-D1-D2, H1-H2-D3-D4 in ascending order.
                for (int i = 0; i < 8; i++)
                    Priorities.Priorities[i] = inRightGroup[i] ? 10 : 0;

                Priorities.AddPriorities(ApoTankPriorSwap ? [1, 0, 4, 5, 2, 3, 6, 7] : [0, 1, 4, 5, 2, 3, 6, 7]);
                // After command mode arrangement
                Priorities.ShowPriorities();

                // After resetting priorities, place elements into tempGroup in a specific order
                // Left1, Right1, Left2, Right2, Left3, Right3, Left4, Right4
                TempGroup = [Priorities.SelectSpecificPriorityIndex(0), Priorities.SelectSpecificPriorityIndex(4),
                            Priorities.SelectSpecificPriorityIndex(1), Priorities.SelectSpecificPriorityIndex(5),
                            Priorities.SelectSpecificPriorityIndex(2), Priorities.SelectSpecificPriorityIndex(6),
                            Priorities.SelectSpecificPriorityIndex(3), Priorities.SelectSpecificPriorityIndex(7)];

                // Since the position is precise for each player, set fixed to true
                GroupingFixed = true;

                // Waymark
                MarkPlayerByIdx(accessory, TempGroup[0].Key, MarkType.Attack1, ApoCaptainMode);
                MarkPlayerByIdx(accessory, TempGroup[2].Key, MarkType.Attack2, ApoCaptainMode);
                MarkPlayerByIdx(accessory, TempGroup[4].Key, MarkType.Attack3, ApoCaptainMode);
                MarkPlayerByIdx(accessory, TempGroup[6].Key, MarkType.Attack4, ApoCaptainMode);
                MarkPlayerByIdx(accessory, TempGroup[1].Key, MarkType.Bind1, ApoCaptainMode);
                MarkPlayerByIdx(accessory, TempGroup[3].Key, MarkType.Bind2, ApoCaptainMode);
                MarkPlayerByIdx(accessory, TempGroup[5].Key, MarkType.Bind3, ApoCaptainMode);
                MarkPlayerByIdx(accessory, TempGroup[7].Key, MarkType.Square, ApoCaptainMode);

                var str0 = "";
                str0 += $"Final Group Left: {TempGroup[0].Key}, {TempGroup[2].Key}, {TempGroup[4].Key}, {TempGroup[6].Key}\n";
                str0 += $"Final Group Right: {TempGroup[1].Key}, {TempGroup[3].Key}, {TempGroup[5].Key}, {TempGroup[7].Key}\n";
                accessory.DebugMsg(str0, DebugMode);

                _events[(int)EventIdx.ApoPreciseGrouping].Set();
            }

            // If fixed is false (command mode off, or no MoMo-style waymarking), tempGroup ordering is: Left none, Right none, Left short water, Right short water, Left mid water, Right mid water, Left long water, Right long water.
            Group = [.. TempGroup];

            var strLeft = $"{accessory.GetPlayerJobByIndex(Group[0].Key)},{accessory.GetPlayerJobByIndex(Group[2].Key)},{accessory.GetPlayerJobByIndex(Group[4].Key)},{accessory.GetPlayerJobByIndex(Group[6].Key)}";
            var strRight = $"{accessory.GetPlayerJobByIndex(Group[1].Key)},{accessory.GetPlayerJobByIndex(Group[3].Key)},{accessory.GetPlayerJobByIndex(Group[5].Key)},{accessory.GetPlayerJobByIndex(Group[7].Key)}";
            accessory.DebugMsg($"\nApocalypse Stack Group{(GroupingFixed ? "(Fixed)" : "")}:\nLeft Group: {strLeft}\nRight Group: {strRight}", DebugMode);

            _events[(int)EventIdx.ApoGrouping].Set();
        }

        public void GetSafeDirs()
        {
            /*      
            * Calculated safe zones (dir+3)%8, 0123 are north side, 4567 are south side.
            *           3
            *       2       4
            *   1               5
            *       0       6
            *           7
            */

            // One step counter-rotation for north Apocalypse
            var dir = (NorthStartPoint + 8 - RotationDir) % 8;
            bool isNorthSafePoint = (dir + 3) % 8 < 4;

            // Define crowd safe points
            SafePoints[isNorthSafePoint ? 0 : 2] = dir;
            SafePoints[isNorthSafePoint ? 2 : 0] = GetSymmetricPoint(dir);

            // Rotate further counter-clockwise to get MT/D1 safe points
            SafePoints[1] = (SafePoints[0] + 8 - RotationDir) % 8;
            SafePoints[3] = (SafePoints[2] + 8 - RotationDir) % 8;

            accessory.DebugMsg($"\nCrowd safe points: North {SafePoints[0]}, South {SafePoints[2]}.\nCrown safe points: North {SafePoints[1]}, South {SafePoints[3]}.", DebugMode);
        }

        public int GetSymmetricPoint(int dir)
        {
            // Apocalypse rotates clockwise, safe zone is one step counter-clockwise.
            return (dir + 8 + 4) % 8;
        }

        public int GetPlayerGroupIdx(int idx)
        {
            // In Fixed mode, this returns the left-front, right-front, left-back, right-back grouping.
            return Group.FindIndex(i => i.Key == idx);
        }
        public int GetMyGroupIdx() => GetPlayerGroupIdx(accessory.GetMyIndex());
        public int GetPlayerGroup(int idx) => GetPlayerGroupIdx(idx) % 2;
        public int GetMyGroup() => GetPlayerGroup(accessory.GetMyIndex());
        public bool IsInLeftGroup(int idx) => GetPlayerGroupIdx(idx) % 2 == 0;
        public bool IsInRightGroup(int idx) => GetPlayerGroupIdx(idx) % 2 == 1;
        public bool IsInCrowd(int idx)
        {
            return (idx != D1) && (idx != (ApoTankPriorSwap ? St : Mt));
        }

        public List<Vector3> GetSafePos(int dir)
        {
            List<Vector3> safePosList = [new(100, 0, 90.2f), new(104.5f, 0, 80.84f), new(95.5f, 0, 80.84f)];
            for (int i = 0; i < 3; i++)
                safePosList[i] = safePosList[i].RotatePoint(Center, 45f.DegToRad() * dir);
            return safePosList;
        }

        public void ShowMessage()
        {
            var str = "\n ---- [Apocalypse] ----\n";
            str += $"\nCrowd safe points: North {SafePoints[0]}, South {SafePoints[2]}.\nCrown safe points: North {SafePoints[1]}, South {SafePoints[3]}.";

            var strLeft = $"{accessory.GetPlayerJobByIndex(Group[0].Key)},{accessory.GetPlayerJobByIndex(Group[2].Key)},{accessory.GetPlayerJobByIndex(Group[4].Key)},{accessory.GetPlayerJobByIndex(Group[6].Key)}";
            var strRight = $"{accessory.GetPlayerJobByIndex(Group[1].Key)},{accessory.GetPlayerJobByIndex(Group[3].Key)},{accessory.GetPlayerJobByIndex(Group[5].Key)},{accessory.GetPlayerJobByIndex(Group[7].Key)}";

            str += $"\nApocalypse Stack Group{(GroupingFixed ? "(Fixed)" : "")}:\nLeft Group: {strLeft}\nRight Group: {strRight}";
            accessory.DebugMsg(str, DebugMode);
        }
    }

    #endregion P3.2 Apocalypse

    #endregion P3 Gaia

    #region P4 Light & Dark Maidens

    [ScriptMethod(name: "---- ã€ŠP4: Light & Dark Maidensã€‹ ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["ActionId:Hello1aya2World"],
        userControl: true)]
    public void SplitLine_Girls(Event @event, ScriptAccessory accessory)
    {
    }

    [ScriptMethod(name: "Futures Fragment Hit Police", eventType: EventTypeEnum.ActionEffect,
        eventCondition: ["ActionId:regex:^(40(190|271|274|241|279|280|277|284|285|289|248|303|250))$"],
        userControl: true)]
    public void FragmentMonitor(Event ev, ScriptAccessory sa)
    {
        if (_fruPhase is not FruPhase.P4A_DarklitDragonsong and not FruPhase.P4B_CrystallizeTime) return;
        
        IGameObject? tObj = sa.GetById(ev.TargetId);
        if (tObj == null) return;

        lock (_fragTargets)
        {
            // Initialize hit list and add
            var tidx = ev.TargetIndex();
            if (tidx == 1 && _fragTargets.Count != 0)
                _fragTargets.Clear();
            _fragTargets.Add(ev.TargetId);
            
            // Target is a Futures Fragment
            var tDataId = tObj.DataId;
            if (tDataId != FragmentDataId) return;

            // Get the skill that hit the fragment
            var aid = ev.ActionId;
            // Skill list
            var skillParam = new Dictionary<uint, string>
            {
                { 40190, "Light Wave Bait" },
                { 40271, "Water Stack" },
                { 40274, "Dark Spread" },
                { 40241, "Head Bump" },
                { 40279, "Ice Donut" },
                { 40280, "Wind Knockback" },
                { 40277, "Dark Stack" },
                { 40284, "Midnight Dance (Far)" },
                { 40285, "Midnight Dance (Near)" },
                { 40289, "Spirit Taker" },
                { 40248, "Death Cycle (Shiva)" },
                { 40303, "Death Cycle (Gaia)" },
                { 40250, "Endless Epiphany" },
            };

            var str = "";
            if (_fragTargets.Count != 1)
            {
                foreach (var target in _fragTargets)
                {
                    IGameObject? obj = sa.GetById(target);
                    if (obj == null) continue;
                    str += obj.Name + " ";
                }
            }
            
            if (skillParam.TryGetValue(aid, out var skillName))
            {
                sa.Method.SendChat($"/e Futures Fragment seems to have been hit by [{skillName}] due to [{str}]!!!<se.11>");
            }
        }
    }

    #region P4.1 Darklit Dragonsong

    [ScriptMethod(name: "Darklit Dragonsong Phase Change (DEBUG ONLY)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40239)$"],
        userControl: Debugging)]
    public void DarklitDragonsongPhaseChange(Event ev, ScriptAccessory sa)
    {
        _fruPhase = FruPhase.P4A_DarklitDragonsong;
        _events = Enumerable
            .Range(0, 20)
            .Select(_ => new ManualResetEvent(false))
            .ToList();
        _recorded = new bool[20].ToList();
        sa.DebugMsg($"Current phase: {_fruPhase}", DebugMode);
    }

    [ScriptMethod(name: "Light Chain Highlight", eventType: EventTypeEnum.Tether, eventCondition: ["Id:006E"],
        userControl: true)]
    public void DldLightChainVisual(Event ev, ScriptAccessory sa)
    {
        if (_fruPhase != FruPhase.P4A_DarklitDragonsong) return;
        var dp1 = sa.DrawGuidance(ev.SourceId, ev.TargetId, 0, 9000, $"Usami-LightChainOuter");
        dp1.Color = ColorHelper.ColorDark.V4.WithW(3f);
        var dp2 = sa.DrawGuidance(ev.SourceId, ev.TargetId, 0, 9000, $"Usami-LightChainInner");
        dp2.Color = ColorHelper.ColorYellow.V4;
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp1);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp2);
    }

    #endregion P4.1 Darklit Dragonsong

    #region P4.2 Crystallize Time

    [ScriptMethod(name: "Crystallize Time Phase Change (DEBUG ONLY)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40240)$"],
        userControl: Debugging)]
    public void CrystallizeTimePhaseChange(Event @event, ScriptAccessory accessory)
    {
        _fruPhase = FruPhase.P4B_CrystallizeTime;
        accessory.DebugMsg($"Current phase: {_fruPhase}", DebugMode);
    }

    #endregion P4.2 Crystallize Time

    #endregion P4 Light & Dark Maidens

    #region P5 PandoraÂ·Mitron

    [ScriptMethod(name: "---- ã€ŠP5: PandoraÂ·Mitronã€‹ ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["ActionId:Hello1aya2World"],
        userControl: true)]
    public void SplitLine_Pandora(Event @event, ScriptAccessory accessory)
    {
    }

    #region P5.1 Fulgent Blade

    [ScriptMethod(name: "Fulgent Blade Phase Change (DEBUG ONLY)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40306)$"],
        userControl: Debugging)]
    public void FulgentBladePhaseChange(Event @event, ScriptAccessory accessory)
    {
        _fruPhase = FruPhase.P5A_FulgentBlade;
        accessory.DebugMsg($"Current phase: {_fruPhase}", DebugMode);
    }

    #endregion P5.1 Fulgent Blade

    #region P5.2 Paradise Regained

    [ScriptMethod(name: "Paradise Regained Phase Change (DEBUG ONLY)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40319)$"],
        userControl: Debugging)]
    public void ParadiseRegainedPhaseChange(Event @event, ScriptAccessory accessory)
    {
        _fruPhase = FruPhase.P5B_ParadiseRegained;
        accessory.DebugMsg($"Current phase: {_fruPhase}", DebugMode);
    }

    #endregion P5.2 Paradise Regained

    #region P5.3 Polarizing Strike

    [ScriptMethod(name: "Polarizing Strike Phase Change (DEBUG ONLY)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40316)$"],
        userControl: Debugging)]
    public void PolStrikePhaseChange(Event @event, ScriptAccessory accessory)
    {
        _fruPhase = FruPhase.P5C_PolarizingStrike;
        accessory.DebugMsg($"Current phase: {_fruPhase}", DebugMode);
    }

    #endregion P5.3 Polarizing Strike

    #endregion P5 PandoraÂ·Mitron

    #region Event Enum
    public enum EventIdx : int
    {
        // _events
        ApoGrouping = 0,
        ApoPreciseGrouping = 1,

        // _recorded
    }

    #endregion
    #region Class Functions
    public class PriorityDict
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public ScriptAccessory accessory { get; set; } = null!;
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public Dictionary<int, int> Priorities { get; set; } = null!;
        public string Annotation { get; set; } = "";
        public int ActionCount { get; set; } = 0;

        public void Init(ScriptAccessory _accessory, string annotation, int partyNum = 8)
        {
            accessory = _accessory;
            Priorities = new Dictionary<int, int>();
            for (var i = 0; i < partyNum; i++)
            {
                Priorities.Add(i, 0);
            }
            Annotation = annotation;
            ActionCount = 0;
        }

        /// <summary>
        /// Increase priority for a specific Key
        /// </summary>
        /// <param name="idx">key</param>
        /// <param name="priority">priority value</param>
        public void AddPriority(int idx, int priority)
        {
            Priorities[idx] += priority;
        }

        /// <summary>
        /// Find the smallest `num` values from Priorities and return a new Dict
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public List<KeyValuePair<int, int>> SelectSmallPriorityIndices(int num)
        {
            return SelectMiddlePriorityIndices(0, num);
        }

        /// <summary>
        /// Find the largest `num` values from Priorities and return a new Dict
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public List<KeyValuePair<int, int>> SelectLargePriorityIndices(int num)
        {
            return SelectMiddlePriorityIndices(0, num, true);
        }

        /// <summary>
        /// Find the intermediate values in ascending order from Priorities and return a new Dict
        /// </summary>
        /// <param name="skip">Skip `skip` elements. If starting from the second element, skip=1</param>
        /// <param name="num"></param>
        /// <param name="descending">Descending order, default false</param>
        /// <returns></returns>
        public List<KeyValuePair<int, int>> SelectMiddlePriorityIndices(int skip, int num, bool descending = false)
        {
            if (Priorities.Count < skip + num)
                return new List<KeyValuePair<int, int>>();

            IEnumerable<KeyValuePair<int, int>> sortedPriorities;
            if (descending)
            {
                // Sort by value descending, then by key, skip `skip` elements, take `num` key-value pairs
                sortedPriorities = Priorities
                    .OrderByDescending(pair => pair.Value) // Sort by value first
                    .ThenBy(pair => pair.Key) // Then by key
                    .Skip(skip) // Skip first `skip` elements
                    .Take(num); // Take `num` key-value pairs
            }
            else
            {
                // Sort by value ascending, then by key, skip `skip` elements, take `num` key-value pairs
                sortedPriorities = Priorities
                    .OrderBy(pair => pair.Value) // Sort by value first
                    .ThenBy(pair => pair.Key) // Then by key
                    .Skip(skip) // Skip first `skip` elements
                    .Take(num); // Take `num` key-value pairs
            }

            return sortedPriorities.ToList();
        }

        /// <summary>
        /// Find the data at the `idx`-th position in ascending order from Priorities and return a new Dict
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="descending">Descending order, default false</param>
        /// <returns></returns>
        public KeyValuePair<int, int> SelectSpecificPriorityIndex(int idx, bool descending = false)
        {
            var sortedPriorities = SelectMiddlePriorityIndices(0, 8, descending);
            return sortedPriorities[idx];
        }

        /// <summary>
        /// Find the rank (position after sorting) of the value corresponding to a given key from Priorities
        /// </summary>
        /// <param name="key"></param>
        /// <param name="descending">Descending order, default false</param>
        /// <returns></returns>
        public int FindPriorityIndexOfKey(int key, bool descending = false)
        {
            var sortedPriorities = SelectMiddlePriorityIndices(0, 8, descending);
            var i = 0;
            foreach (var dict in sortedPriorities)
            {
                if (dict.Key == key) return i;
                i++;
            }

            return i;
        }

        /// <summary>
        /// Add priority values in bulk
        /// Usually used for special priorities (e.g., H-T-D-H)
        /// </summary>
        /// <param name="priorities"></param>
        public void AddPriorities(List<int> priorities)
        {
            if (Priorities.Count != priorities.Count)
                throw new ArgumentException("The input list length differs from the internal setting length");

            for (var i = 0; i < Priorities.Count; i++)
                AddPriority(i, priorities[i]);
        }

        /// <summary>
        /// Output the Priority dictionary's Keys and priorities
        /// </summary>
        /// <returns></returns>
        public string ShowPriorities()
        {
            var str = $"{Annotation} Priority Dictionary:\n";
            foreach (var pair in Priorities)
            {
                str += $"Key {pair.Key} ({accessory.GetPlayerJobByIndex(pair.Key)}), Value {pair.Value}\n";
            }
            return str;
        }

        public string PrintAnnotation()
        {
            return Annotation;
        }

        public PriorityDict DeepCopy()
        {
            return JsonConvert.DeserializeObject<PriorityDict>(JsonConvert.SerializeObject(this)) ?? new PriorityDict();
        }

        public void AddActionCount(int count = 1)
        {
            ActionCount += count;
        }

        public bool IsActionCountEqualTo(int times)
        {
            return ActionCount == times;
        }
    }

    public class Counter
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public ScriptAccessory accessory { get; set; } = null!;
        public int Number { get; set; } = 0;
        public bool Enable { get; set; } = true;
        public string Annotation = "";

        public void Init(ScriptAccessory _accessory, string annotation, bool enable = true)
        {
            accessory = _accessory;
            Number = 0;
            Enable = enable;
            Annotation = annotation;
        }

        public string ShowCounter()
        {
            var str = $"{Annotation} Counter ã€{(Enable ? "Enabled" : "Disabled")}ã€‘: {Number}\n";
            accessory.DebugMsg(str, DebugMode);
            return str;
        }

        public void DisableCounter()
        {
            Enable = false;
            var str = $"Disabling counter for {Annotation}.\n";
            accessory.DebugMsg(str, DebugMode);
        }

        public void EnableCounter()
        {
            Enable = true;
            var str = $"Enabling counter for {Annotation}.\n";
            accessory.DebugMsg(str, DebugMode);
        }

        public void AddCounter(int num = 1)
        {
            if (!Enable) return;
            Number += num;
        }

        public void TimesCounter(int num = 1)
        {
            if (!Enable) return;
            Number *= num;
        }
    }

    #endregion Class Functions

    #region Waymark Clear Functions

    private static void LocalMarkClear(ScriptAccessory accessory)
    {
        accessory.Method.Mark(0xE000000, MarkType.Attack1, true);
        accessory.Method.Mark(0xE000000, MarkType.Attack2, true);
        accessory.Method.Mark(0xE000000, MarkType.Attack3, true);
        accessory.Method.Mark(0xE000000, MarkType.Attack4, true);
        accessory.Method.Mark(0xE000000, MarkType.Attack5, true);
        accessory.Method.Mark(0xE000000, MarkType.Attack6, true);
        accessory.Method.Mark(0xE000000, MarkType.Attack7, true);
        accessory.Method.Mark(0xE000000, MarkType.Attack8, true);
        accessory.Method.Mark(0xE000000, MarkType.Bind1, true);
        accessory.Method.Mark(0xE000000, MarkType.Bind2, true);
        accessory.Method.Mark(0xE000000, MarkType.Bind3, true);
        accessory.Method.Mark(0xE000000, MarkType.Stop1, true);
        accessory.Method.Mark(0xE000000, MarkType.Stop2, true);
        accessory.Method.Mark(0xE000000, MarkType.Square, true);
        accessory.Method.Mark(0xE000000, MarkType.Circle, true);
        accessory.Method.Mark(0xE000000, MarkType.Cross, true);
        accessory.Method.Mark(0xE000000, MarkType.Triangle, true);
    }

    private static void MarkClear(ScriptAccessory accessory)
    {
        if (!CaptainMode) return;
        if (LocalTest)
        {
            accessory.DebugMsg($"Deleting waymarks for local test.");
            if (LocalStrTest) return;
            LocalMarkClear(accessory);
        }
        else
            accessory.Method.MarkClear();
    }

    private static void MarkPlayerByIdx(ScriptAccessory accessory, int idx, MarkType marker, bool enable = true)
    {
        if (!CaptainMode) return;
        if (!enable) return;
        accessory.DebugMsg($"Waymarking {idx}({accessory.GetPlayerJobByIndex(idx)}) with {marker}.", DebugMode && LocalStrTest);
        if (LocalStrTest) return;
        accessory.Method.Mark(accessory.Data.PartyList[idx], marker, LocalTest);
    }

    private static void MarkPlayerById(ScriptAccessory accessory, uint id, MarkType marker, bool enable = true)
    {
        if (!CaptainMode) return;
        if (!enable) return;
        accessory.DebugMsg($"Waymarking {accessory.GetPlayerIdIndex(id)}({accessory.GetPlayerJobById(id)}) with {marker}.",
            DebugMode && LocalStrTest);
        if (LocalStrTest) return;
        accessory.Method.Mark(id, marker, LocalTest);
    }

    private static int GetMarkedPlayerIndex(ScriptAccessory accessory, List<MarkType> markerList, MarkType marker)
    {
        return markerList.IndexOf(marker);
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
    public static uint DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["DurationMilliseconds"]);
    }

    public static uint Id2(this Event @event)
    {
        return ParseHexId(@event["Id2"], out var id) ? id : 0;
    }

    public static uint Id0(this Event @event)
    {
        return ParseHexId(@event["Id"], out var id) ? id : 0;
    }

    public static uint TargetIndex(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["TargetIndex"]);
    }

    public static string Message(this Event ev)
    {
        return ev["Message"];
    }
}

public static class IbcHelper
{
    public static IGameObject? GetById(this ScriptAccessory sa, ulong id)
    {
        return sa.Data.Objects.SearchById(id);
    }

    public static IGameObject? GetMe(this ScriptAccessory sa)
    {
        return sa.Data.Objects.LocalPlayer;
    }

    public static IEnumerable<IGameObject?> GetByDataId(this ScriptAccessory sa, uint dataId)
    {
        return sa.Data.Objects.Where(x => x.DataId == dataId);
    }

    public static string GetCharJob(this ScriptAccessory sa, uint id, bool fullName = false)
    {
        IPlayerCharacter? chara = (IPlayerCharacter?)sa.GetById(id);
        if (chara == null) return "None";
        return fullName ? chara.ClassJob.Value.Name.ToString() : chara.ClassJob.Value.Abbreviation.ToString();
    }

    public static bool AtNorth(this ScriptAccessory sa, uint id, float centerZ)
    {
        var chara = sa.GetById(id);
        if (chara == null) return false;
        return chara.Position.Z <= centerZ;
    }
    public static bool AtSouth(this ScriptAccessory sa, uint id, float centerZ)
    {
        var chara = sa.GetById(id);
        if (chara == null) return false;
        return chara.Position.Z > centerZ;
    }
    public static bool AtWest(this ScriptAccessory sa, uint id, float centerX)
    {
        var chara = sa.GetById(id);
        if (chara == null) return false;
        return chara.Position.X <= centerX;
    }
    public static bool AtEast(this ScriptAccessory sa, uint id, float centerX)
    {
        var chara = sa.GetById(id);
        if (chara == null) return false;
        return chara.Position.X > centerX;
    }

    public static bool HasStatus(this ScriptAccessory sa, uint id, uint statusId)
    {
        IGameObject? chara = sa.GetById(id);
        if (chara == null || !chara.IsValid()) return false;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)chara.Address;
            return charaStruct->GetStatusManager()->HasStatus(statusId, id);
        }
    }
}

public static class DirectionCalc
{
    public static float DegToRad(this float deg) => (deg + 360f) % 360f / 180f * float.Pi;
    public static float RadToDeg(this float rad) => (rad + 2 * float.Pi) % (2 * float.Pi) / float.Pi * 180f;
    // North as 0 for list
    // Game         List    Logic
    // 0            - 4     pi
    // 0.25 pi      - 3     0.75pi
    // 0.5 pi       - 2     0.5pi
    // 0.75 pi      - 1     0.25pi
    // pi           - 0     0
    // 1.25 pi      - 7     1.75pi
    // 1.5 pi       - 6     1.5pi
    // 1.75 pi      - 5     1.25pi
    // Logic = Pi - Game (+ 2pi)

    /// <summary>
    /// Convert in-game base angle (South as 0, increasing counter-clockwise) to logic base angle (North as 0, increasing clockwise)
    /// Algorithm is identical to Logic2Game, kept separate for code readability.
    /// </summary>
    /// <param name="radian">In-game base angle</param>
    /// <returns>Logic base angle</returns>
    public static float Game2Logic(this float radian)
    {
        var r = float.Pi - radian;
        r = (r + float.Pi * 2) % (float.Pi * 2);
        return r;
    }

    /// <summary>
    /// Convert logic base angle (North as 0, increasing clockwise) to in-game base angle (South as 0, increasing counter-clockwise)
    /// Algorithm is identical to Game2Logic, kept separate for code readability.
    /// </summary>
    /// <param name="radian">Logic base angle</param>
    /// <returns>In-game base angle</returns>
    public static float Logic2Game(this float radian)
    {
        return radian.Game2Logic();
    }

    /// <summary>
    /// For rotations, in FFXIV game base clockwise rotation is negative.
    /// </summary>
    /// <param name="radian"></param>
    /// <returns></returns>
    public static float Cw2Ccw(this float radian)
    {
        return -radian;
    }

    /// <summary>
    /// For rotations, in FFXIV game base clockwise rotation is negative.
    /// Identical to Cw2Ccw, kept separate for code readability.
    /// </summary>
    /// <param name="radian"></param>
    /// <returns></returns>
    public static float Ccw2Cw(this float radian)
    {
        return -radian;
    }

    /// <summary>
    /// Input logic base angle, get logic direction (diagonal division: straight up as 0, normal division: top-right as 0, increasing clockwise)
    /// </summary>
    /// <param name="radian">Logic base angle</param>
    /// <param name="dirs">Total number of directions</param>
    /// <param name="diagDivision">Diagonal division, default true</param>
    /// <returns>Logic direction corresponding to the logic base angle</returns>
    public static int Rad2Dirs(this float radian, int dirs, bool diagDivision = true)
    {
        var r = diagDivision
            ? Math.Round(radian / (2f * float.Pi / dirs))
            : Math.Floor(radian / (2f * float.Pi / dirs));
        r = (r + dirs) % dirs;
        return (int)r;
    }

    /// <summary>
    /// Input coordinates, get logic direction (diagonal division: straight up as 0, normal division: top-right as 0, increasing clockwise)
    /// </summary>
    /// <param name="point">Coordinate point</param>
    /// <param name="center">Center point</param>
    /// <param name="dirs">Total number of directions</param>
    /// <param name="diagDivision">Diagonal division, default true</param>
    /// <returns>Logic direction corresponding to the coordinate point</returns>
    public static int Position2Dirs(this Vector3 point, Vector3 center, int dirs, bool diagDivision = true)
    {
        double dirsDouble = dirs;
        var r = diagDivision
            ? Math.Round(dirsDouble / 2 - dirsDouble / 2 * Math.Atan2(point.X - center.X, point.Z - center.Z) / Math.PI) % dirsDouble
            : Math.Floor(dirsDouble / 2 - dirsDouble / 2 * Math.Atan2(point.X - center.X, point.Z - center.Z) / Math.PI) % dirsDouble;
        return (int)r;
    }

    /// <summary>
    /// Rotate a point around a center by a logic base radian
    /// </summary>
    /// <param name="point">Point to rotate</param>
    /// <param name="center">Center</param>
    /// <param name="radian">Rotation radian</param>
    /// <returns>Rotated coordinate point</returns>
    public static Vector3 RotatePoint(this Vector3 point, Vector3 center, float radian)
    {
        // Rotate a point clockwise by a certain radian around a center
        Vector2 v2 = new(point.X - center.X, point.Z - center.Z);
        var rot = MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian;
        var length = v2.Length();
        return new Vector3(center.X + MathF.Sin(rot) * length, center.Y, center.Z - MathF.Cos(rot) * length);
    }

    /// <summary>
    /// Extend a point from a center point by a logic base angle
    /// </summary>
    /// <param name="center">Center point to extend from</param>
    /// <param name="radian">Rotation radian</param>
    /// <param name="length">Extension length</param>
    /// <returns>Extended coordinate point</returns>
    public static Vector3 ExtendPoint(this Vector3 center, float radian, float length)
    {
        // Extend a point a certain length at a certain radian
        return new Vector3(center.X + MathF.Sin(radian) * length, center.Y, center.Z - MathF.Cos(radian) * length);
    }

    /// <summary>
    /// Find the logic base radian from an outer point to the center
    /// </summary>
    /// <param name="center">Center</param>
    /// <param name="newPoint">Outer point</param>
    /// <returns>Logic base radian from the outer point to the center</returns>
    public static float FindRadian(this Vector3 newPoint, Vector3 center)
    {
        var radian = MathF.PI - MathF.Atan2(newPoint.X - center.X, newPoint.Z - center.Z);
        if (radian < 0)
            radian += 2 * MathF.PI;
        return radian;
    }

    /// <summary>
    /// Fold the input point horizontally
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerX">Center axis X coordinate</param>
    /// <returns></returns>
    public static Vector3 FoldPointHorizon(this Vector3 point, float centerX)
    {
        return point with { X = 2 * centerX - point.X };
    }

    /// <summary>
    /// Fold the input point vertically
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerZ">Center axis Z coordinate</param>
    /// <returns></returns>
    public static Vector3 FoldPointVertical(this Vector3 point, float centerZ)
    {
        return point with { Z = 2 * centerZ - point.Z };
    }

    /// <summary>
    /// Center symmetry of the input point
    /// </summary>
    /// <param name="point">Input point</param>
    /// <param name="center">Center point</param>
    /// <returns></returns>
    public static Vector3 PointCenterSymmetry(this Vector3 point, Vector3 center)
    {
        return point.RotatePoint(center, float.Pi);
    }

    /// <summary>
    /// Extend the input point towards or away from a center point along the same angle, default inward
    /// </summary>
    /// <param name="point">Point to extend</param>
    /// <param name="center">Center point</param>
    /// <param name="length">Extension length</param>
    /// <param name="isOutside">Whether to extend outward</param>>
    /// <returns></returns>
    public static Vector3 PointInOutside(this Vector3 point, Vector3 center, float length, bool isOutside = false)
    {
        Vector2 v2 = new(point.X - center.X, point.Z - center.Z);
        var targetPos = (point - center) / v2.Length() * length * (isOutside ? 1 : -1) + point;
        return targetPos;
    }

    /// <summary>
    /// Calculate distance between two points
    /// </summary>
    /// <param name="point"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static float DistanceTo(this Vector3 point, Vector3 target)
    {
        Vector2 v2 = new(point.X - target.X, point.Z - target.Z);
        return v2.Length();
    }

    /// <summary>
    /// Find the angular difference between two points, range 0~360 deg
    /// </summary>
    /// <param name="basePoint">Base position</param>
    /// <param name="targetPos">Target position to compare</param>
    /// <param name="center">Arena center</param>
    /// <returns></returns>
    public static float FindRadianDifference(this Vector3 targetPos, Vector3 basePoint, Vector3 center)
    {
        var baseRad = basePoint.FindRadian(center);
        var targetRad = targetPos.FindRadian(center);
        var deltaRad = targetRad - baseRad;
        if (deltaRad < 0)
            deltaRad += float.Pi * 2;
        return deltaRad;
    }

    /// <summary>
    /// From a third-person perspective, check if a target is to the right of another target.
    /// </summary>
    /// <param name="basePoint">Base position</param>
    /// <param name="targetPos">Target position to compare</param>
    /// <param name="center">Arena center</param>
    /// <returns></returns>
    public static bool IsAtRight(this Vector3 targetPos, Vector3 basePoint, Vector3 center)
    {
        // From center looking outwards, is it on the right?
        return targetPos.FindRadianDifference(basePoint, center) < float.Pi;
    }

    /// <summary>
    /// Get the specified digit of a number
    /// </summary>
    /// <param name="val">Given integer</param>
    /// <param name="x">Corresponding digit, units digit is 1</param>
    /// <returns></returns>
    public static int GetDecimalDigit(this int val, int x)
    {
        string valStr = val.ToString();
        int length = valStr.Length;

        if (x < 1 || x > length)
        {
            return -1;
        }

        char digitChar = valStr[length - x]; // Take the x-th digit from the right
        return int.Parse(digitChar.ToString());
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
    public static int GetPlayerIdIndex(this ScriptAccessory accessory, ulong pid)
    {
        return accessory.Data.PartyList.IndexOf((uint)pid);
    }

    /// <summary>
    /// Get the position index of the main perspective player
    /// </summary>
    /// <param name="accessory"></param>
    /// <returns>Position index of the main perspective player</returns>
    public static int GetMyIndex(this ScriptAccessory accessory)
    {
        return accessory.Data.PartyList.IndexOf(accessory.Data.Me);
    }

    /// <summary>
    /// Input player dataId, get the corresponding position name, output string only for text output
    /// </summary>
    /// <param name="pid">Player SourceId</param>
    /// <param name="accessory"></param>
    /// <returns>The position name corresponding to the player</returns>
    public static string GetPlayerJobById(this ScriptAccessory accessory, uint pid)
    {
        var idx = accessory.Data.PartyList.IndexOf(pid);
        var str = accessory.GetPlayerJobByIndex(idx);
        return str;
    }

    /// <summary>
    /// Input position index, get the corresponding position name, output string only for text output
    /// </summary>
    /// <param name="idx">Position index</param>
    /// <param name="fourPeople">Is it a 4-man dungeon</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static string GetPlayerJobByIndex(this ScriptAccessory accessory, int idx, bool fourPeople = false)
    {
        var str = idx switch
        {
            0 => "MT",
            1 => fourPeople ? "H1" : "ST",
            2 => fourPeople ? "D1" : "H1",
            3 => fourPeople ? "D2" : "H2",
            4 => "D1",
            5 => "D2",
            6 => "D3",
            7 => "D4",
            _ => "unknown"
        };
        return str;
    }
}

public static class ColorHelper
{
    public static ScriptColor ColorRed = new ScriptColor { V4 = new Vector4(1.0f, 0f, 0f, 1.0f) };
    public static ScriptColor ColorPink = new ScriptColor { V4 = new Vector4(1f, 0f, 1f, 1.0f) };
    public static ScriptColor ColorCyan = new ScriptColor { V4 = new Vector4(0f, 1f, 0.8f, 1.0f) };
    public static ScriptColor ColorDark = new ScriptColor { V4 = new Vector4(0f, 0f, 0f, 1.0f) };
    public static ScriptColor ColorLightBlue = new ScriptColor { V4 = new Vector4(0.48f, 0.40f, 0.93f, 1.0f) };
    public static ScriptColor ColorWhite = new ScriptColor { V4 = new Vector4(1f, 1f, 1f, 2f) };
    public static ScriptColor ColorYellow = new ScriptColor { V4 = new Vector4(1.0f, 1.0f, 0f, 1.0f) };

}

#region Drawing Functions
public static class AssignDp
{
    /// <summary>
    /// Return arrow guidance dp
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="ownerObj">Arrow start, can be uint or Vector3</param>
    /// <param name="targetObj">Arrow target, can be uint or Vector3, 0 means no target</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="rotation">Arrow rotation angle</param>
    /// <param name="scale">Arrow width</param>
    /// <param name="isSafe">Use safe color</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static DrawPropertiesEdit DrawGuidance(this ScriptAccessory accessory,
        object ownerObj, object targetObj, int delay, int destroy, string name, float rotation = 0, float scale = 1f, bool isSafe = true)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(scale);
        dp.Rotation = rotation;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Color = isSafe ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;

        switch (ownerObj)
        {
            case uint sid:
                dp.Owner = sid;
                break;
            case ulong sid:
                dp.Owner = sid;
                break;
            case Vector3 spos:
                dp.Position = spos;
                break;
            default:
                throw new ArgumentException("Invalid target type for ownerObj");
        }

        switch (targetObj)
        {
            case uint tid:
                if (tid != 0) dp.TargetObject = tid;
                break;
            case ulong tid:
                if (tid != 0) dp.TargetObject = tid;
                break;
            case Vector3 tpos:
                dp.TargetPosition = tpos;
                break;
        }

        return dp;
    }

    public static DrawPropertiesEdit DrawGuidance(this ScriptAccessory accessory,
        object targetObj, int delay, int destroy, string name, float rotation = 0, float scale = 1f, bool isSafe = true)
    => accessory.DrawGuidance(accessory.Data.Me, targetObj, delay, destroy, name, rotation, scale, isSafe);

    /// <summary>
    /// Return left/right cleave fan drawing
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="ownerId">Start target id, usually the boss</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="isLeftCleave">Is left cleave</param>
    /// <param name="radian">Fan angle</param>
    /// <param name="scale">Fan size</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawLeftRightCleave(this ScriptAccessory accessory, uint ownerId, bool isLeftCleave, int delay, int destroy, string name, float radian = float.Pi, float scale = 60f, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(scale);
        dp.Radian = radian;
        dp.Rotation = isLeftCleave ? float.Pi / 2 : -float.Pi / 2;
        dp.Owner = ownerId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return front/back cleave fan drawing
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="ownerId">Start target id, usually the boss</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="isFrontCleave">Is front cleave</param>
    /// <param name="radian">Fan angle</param>
    /// <param name="scale">Fan size</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawFrontBackCleave(this ScriptAccessory accessory, uint ownerId, bool isFrontCleave, int delay, int destroy, string name, float radian = float.Pi, float scale = 60f, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(scale);
        dp.Radian = radian;
        dp.Rotation = isFrontCleave ? 0 : -float.Pi;
        dp.Owner = ownerId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return dp for nearest/farthest target relative to an object
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="ownerId">Start target id, usually the boss</param>
    /// <param name="orderIdx">Order, starting from 1</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="width">Drawing width</param>
    /// <param name="length">Drawing length</param>
    /// <param name="isNear">True for nearest, false for farthest</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <param name="lengthByDistance">Whether length scales with distance</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawTargetNearFarOrder(this ScriptAccessory accessory, ulong ownerId, uint orderIdx,
        bool isNear, float width, float length, int delay, int destroy, string name, bool lengthByDistance = false, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Owner = ownerId;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.CentreResolvePattern =
            isNear ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
        dp.CentreOrderIndex = orderIdx;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode |= lengthByDistance ? ScaleMode.YByDistance : ScaleMode.None;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return dp for nearest/farthest player relative to a position
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="position">Specific coordinate point</param>
    /// <param name="orderIdx">Order, starting from 1</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="width">Drawing width</param>
    /// <param name="length">Drawing length</param>
    /// <param name="isNear">True for nearest, false for farthest</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <param name="lengthByDistance">Whether length scales with distance</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawPositionNearFarOrder(this ScriptAccessory accessory, Vector3 position, uint orderIdx,
        bool isNear, float width, float length, int delay, int destroy, string name, bool lengthByDistance = false, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Position = position;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.TargetResolvePattern =
            isNear ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
        dp.TargetOrderIndex = orderIdx;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode |= lengthByDistance ? ScaleMode.YByDistance : ScaleMode.None;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return dp for the owner's spell target
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="ownerId">Start target id, usually the boss</param>
    /// <param name="width">Drawing width</param>
    /// <param name="length">Drawing length</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <param name="lengthByDistance">Whether length scales with distance</param>
    /// <param name="name">Drawing name</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawOwnersTarget(this ScriptAccessory accessory, uint ownerId, float width, float length, int delay,
        int destroy, string name, bool lengthByDistance = false, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Owner = ownerId;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode |= lengthByDistance ? ScaleMode.YByDistance : ScaleMode.None;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return dp related to owner's enmity
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="ownerId">Start target id, usually the boss</param>
    /// <param name="orderIdx">Enmity order, starting from 1</param>
    /// <param name="width">Drawing width</param>
    /// <param name="length">Drawing length</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <param name="lengthByDistance">Whether length scales with distance</param>
    /// <param name="name">Drawing name</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawOwnersEnmityOrder(this ScriptAccessory accessory, uint ownerId, uint orderIdx, float width, float length, int delay, int destroy, string name, bool lengthByDistance = false, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Owner = ownerId;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.CentreResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.CentreOrderIndex = orderIdx;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode |= lengthByDistance ? ScaleMode.YByDistance : ScaleMode.None;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return owner to target dp, can modify dp.Owner, dp.TargetObject, dp.Scale
    /// </summary>
    /// <param name="ownerId">Start target id, usually self</param>
    /// <param name="targetId">Target unit id</param>
    /// <param name="rotation">Drawing rotation angle</param>
    /// <param name="width">Drawing width</param>
    /// <param name="length">Drawing length</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="lengthByDistance">Whether length scales with distance</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawTarget2Target(this ScriptAccessory accessory, uint ownerId, uint targetId, float width, float length, int delay, int destroy, string name, float rotation = 0, bool lengthByDistance = false, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Rotation = rotation;
        dp.Owner = ownerId;
        dp.TargetObject = targetId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= lengthByDistance ? ScaleMode.YByDistance : ScaleMode.None;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return a fan drawing directed at a target
    /// </summary>
    /// <param name="sourceId">Start target id, usually self</param>
    /// <param name="targetId">Target unit id</param>
    /// <param name="radian">Fan angle</param>
    /// <param name="scale">Fan size</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="color">Drawing color</param>
    /// <param name="rotation">Rotation angle</param>
    /// <param name="lengthByDistance">Whether length scales with distance</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawFanToTarget(this ScriptAccessory accessory, uint sourceId, uint targetId, float radian, float scale, int delay, int destroy, string name, Vector4 color, float rotation = 0, bool lengthByDistance = false, bool byTime = false)
    {
        var dp = accessory.DrawTarget2Target(sourceId, targetId, scale, scale, delay, destroy, name, rotation, lengthByDistance, byTime);
        dp.Radian = radian;
        dp.Color = color;
        return dp;
    }

    /// <summary>
    /// Return line drawing between owner and target, using Line drawing type
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="ownerId">Start target id, usually self</param>
    /// <param name="targetId">Target unit id</param>
    /// <param name="scale">Line width</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawConnectionBetweenTargets(this ScriptAccessory accessory, uint ownerId,
        uint targetId, int delay, int destroy, string name, float scale = 1f)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(scale);
        dp.Owner = ownerId;
        dp.TargetObject = targetId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= ScaleMode.YByDistance;
        return dp;
    }

    /// <summary>
    /// Return circle drawing, follows owner, can modify dp.Owner, dp.Scale
    /// </summary>
    /// <param name="ownerId">Start target id, usually self or boss</param>
    /// <param name="scale">Circle size</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawCircle(this ScriptAccessory accessory, ulong ownerId, float scale, int delay, int destroy, string name, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(scale);
        dp.Owner = ownerId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return donut drawing, follows owner, can modify dp.Owner, dp.Scale
    /// </summary>
    /// <param name="ownerId">Start target id, usually self or boss</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="scale">Outer solid size</param>
    /// <param name="innerScale">Inner hollow size</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawDonut(this ScriptAccessory accessory, uint ownerId, float scale, float innerScale, int delay, int destroy, string name, bool byTime = false)
    {
        var dp = accessory.DrawFan(ownerId, float.Pi * 2, 0, scale, innerScale, delay, destroy, name, byTime);
        return dp;
    }

    /// <summary>
    /// Return static dp, usually for guiding fixed positions. Can modify dp.Position, dp.Rotation, dp.Scale
    /// </summary>
    /// <param name="ownerObj">Drawing start, can be uint or Vector3</param>
    /// <param name="targetObj">Drawing target, can be uint or Vector3, 0 means no target</param>
    /// <param name="radian">Shape angle</param>
    /// <param name="rotation">Rotation angle, North as 0 degrees clockwise</param>
    /// <param name="width">Drawing width</param>
    /// <param name="length">Drawing length</param>
    /// <param name="color">If Vector4, use this color</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawStatic(this ScriptAccessory accessory, object ownerObj, object targetObj,
        float radian, float rotation, float width, float length, object color, int delay, int destroy, string name)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        switch (ownerObj)
        {
            case uint sid:
                dp.Owner = sid;
                break;
            case ulong sid:
                dp.Owner = sid;
                break;
            case Vector3 spos:
                dp.Position = spos;
                break;
        }
        switch (targetObj)
        {
            case uint tid:
                if (tid != 0) dp.TargetObject = tid;
                break;
            case ulong tid:
                if (tid != 0) dp.TargetObject = tid;
                break;
            case Vector3 tpos:
                dp.TargetPosition = tpos;
                break;
        }
        dp.Radian = radian;
        dp.Rotation = rotation.Logic2Game();
        switch (color)
        {
            case Vector4 clr:
                dp.Color = clr;
                break;
            default:
                dp.Color = accessory.Data.DefaultDangerColor;
                break;
        }
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        return dp;
    }

    /// <summary>
    /// Return static circle dp, usually for guiding fixed positions.
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="center">Circle center position</param>
    /// <param name="color">Circle color</param>
    /// <param name="scale">Circle size, default 1.5f</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawStaticCircle(this ScriptAccessory accessory, Vector3 center, Vector4 color,
        int delay, int destroy, string name, float scale = 1.5f)
        => accessory.DrawStatic(center, (uint)0, 0, 0, scale, scale, color, delay, destroy, name);

    /// <summary>
    /// Return static donut dp, usually for guiding fixed positions.
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="center">Donut center position</param>
    /// <param name="color">Donut color</param>
    /// <param name="scale">Donut outer radius, default 1.5f</param>
    /// <param name="innerscale">Donut inner radius, default scale-0.05f</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawStaticDonut(this ScriptAccessory accessory, Vector3 center, Vector4 color,
        int delay, int destroy, string name, float scale, float innerscale = 0)
        => accessory.DrawStatic(center, (uint)0,
        float.Pi * 2, 0, scale, scale, color, delay, destroy, name);

    /// <summary>
    /// Return rectangle
    /// </summary>
    /// <param name="ownerId">Start target id, usually self or boss</param>
    /// <param name="width">Rectangle width</param>
    /// <param name="length">Rectangle length</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawRect(this ScriptAccessory accessory, uint ownerId, float width, float length, int delay, int destroy, string name, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Owner = ownerId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return fan
    /// </summary>
    /// <param name="ownerId">Start target id, usually self or boss</param>
    /// <param name="radian">Fan radian</param>
    /// <param name="rotation">Shape rotation angle</param>
    /// <param name="scale">Fan size</param>
    /// <param name="innerScale">Fan inner hollow size</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawFan(this ScriptAccessory accessory, uint ownerId, float radian, float rotation, float scale, float innerScale, int delay, int destroy, string name, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(scale);
        dp.InnerScale = new Vector2(innerScale);
        dp.Radian = radian;
        dp.Rotation = rotation;
        dp.Owner = ownerId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return knockback direction drawing
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="target">Knockback source, can be uint or Vector3</param>
    /// <param name="width">Knockback drawing width</param>
    /// <param name="length">Knockback drawing length/distance</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="ownerId">Start target ID, usually self or another player</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static DrawPropertiesEdit DrawKnockBack(this ScriptAccessory accessory, uint ownerId, object target, float length, int delay, int destroy, string name, float width = 1.5f, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Owner = ownerId;
        switch (target)
        {
            case uint tid:
                dp.TargetObject = tid;
                break;
            case ulong tid:
                dp.TargetObject = tid;
                break;
            case Vector3 tpos:
                dp.TargetPosition = tpos;
                break;
            default:
                throw new ArgumentException("Invalid target type for DrawKnockBack");
        }
        dp.Rotation = float.Pi;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    public static DrawPropertiesEdit DrawKnockBack(this ScriptAccessory accessory, object target, float length,
        int delay, int destroy, string name, float width = 1.5f, bool byTime = false)
        => accessory.DrawKnockBack(accessory.Data.Me, target, length, delay, destroy, name, width, byTime);

    /// <summary>
    /// Return look away (sight) drawing
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="target">Look away source, can be uint or Vector3</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="ownerId">Start target ID, usually self or another player</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static DrawPropertiesEdit DrawSightAvoid(this ScriptAccessory accessory, uint ownerId, object target, int delay, int destroy, string name)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = ownerId;
        switch (target)
        {
            case uint tid:
                dp.TargetObject = tid;
                break;
            case ulong tid:
                dp.TargetObject = tid;
                break;
            case Vector3 tpos:
                dp.TargetPosition = tpos;
                break;
            default:
                throw new ArgumentException("Invalid target type for DrawSightAvoid");
        }
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        return dp;
    }

    public static DrawPropertiesEdit DrawSightAvoid(this ScriptAccessory accessory, object target, int delay,
        int destroy, string name)
        => accessory.DrawSightAvoid(accessory.Data.Me, target, delay, destroy, name);

    /// <summary>
    /// Return multi-directional extension guidance drawings
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="owner">Spread source</param>
    /// <param name="extendDirs">Spread angles</param>
    /// <param name="myDirIdx">Player's corresponding angle index</param>
    /// <param name="width">Guidance arrow width</param>
    /// <param name="length">Guidance arrow length</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="colorPlayer">Arrow guidance color for the player</param>
    /// <param name="colorNormal">Arrow guidance color for others</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static List<DrawPropertiesEdit> DrawExtendDirection(this ScriptAccessory accessory, object owner,
        List<float> extendDirs, int myDirIdx, float width, float length, int delay, int destroy, string name,
        Vector4 colorPlayer, Vector4 colorNormal)
    {
        List<DrawPropertiesEdit> dpList = [];
        switch (owner)
        {
            case uint sid:
                for (var i = 0; i < extendDirs.Count; i++)
                {
                    var dp = accessory.DrawGuidance(owner, sid, delay, destroy, $"{name}{i}", extendDirs[i], width);
                    dp.Color = i == myDirIdx ? colorPlayer : colorNormal;
                    dpList.Add(dp);
                }
                break;
            case Vector3 spos:
                for (var i = 0; i < extendDirs.Count; i++)
                {
                    var dp = accessory.DrawGuidance(spos, spos.ExtendPoint(extendDirs[i], length), delay, destroy,
                        $"{name}{i}", 0, width);
                    dp.Color = i == myDirIdx ? colorPlayer : colorNormal;
                    dpList.Add(dp);
                }
                break;
            default:
                throw new ArgumentException("Invalid target type for DrawExtendDirection");
        }

        return dpList;
    }

    /// <summary>
    /// Return multi-location guidance drawing list
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="positions">Location positions</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="colorPosPlayer">Color for the player's position marker</param>
    /// <param name="colorPosNormal">Color for position markers</param>
    /// <param name="colorGo">Color for the starting guidance arrow</param>
    /// <param name="colorPrepare">Color for the preparation guidance arrow</param>
    /// <returns>Three Lists in dpList: position markers, player guidance arrows, location-to-next-location guidance arrows</returns>
    public static List<List<DrawPropertiesEdit>> DrawMultiGuidance(this ScriptAccessory accessory,
        List<Vector3> positions, List<int> delay, List<int> destroy, string name,
        Vector4 colorGo, Vector4 colorPrepare, Vector4 colorPosNormal, Vector4 colorPosPlayer)
    {
        List<List<DrawPropertiesEdit>> dpList = [[], [], []];
        for (var i = 0; i < positions.Count; i++)
        {
            var dpPos = accessory.DrawStaticCircle(positions[i], colorPosPlayer, delay[i], destroy[i], $"{name}pos{i}");
            dpList[0].Add(dpPos);
            var dpGuide = accessory.DrawGuidance(positions[i], colorGo, delay[i], destroy[i], $"{name}guide{i}");
            dpList[1].Add(dpGuide);
            if (i == positions.Count - 1) break;
            var dpPrep = accessory.DrawGuidance(positions[i], positions[i + 1], delay[i], destroy[i], $"{name}prep{i}");
            dpList[2].Add(dpPrep);
        }
        return dpList;
    }

    public static void DebugMsg(this ScriptAccessory accessory, string str, bool debugMode = false, bool debugChat = false)
    {
        if (!debugMode)
            return;
        accessory.Log.Debug($"/e [DEBUG] {str}");

        if (!debugChat)
            return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    /// <summary>
    /// Convert List content to string.
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="myList"></param>
    /// <param name="isJob">If true, convert to role name before string conversion</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string BuildListStr<T>(this ScriptAccessory accessory, List<T> myList, bool isJob = false)
    {
        return string.Join(", ", myList.Select(item =>
        {
            if (isJob && item != null && item is int i)
                return accessory.GetPlayerJobByIndex(i);
            return item?.ToString() ?? "";
        }));
    }
}

#endregion Drawing Functions

#region Special Functions
public static class SpecialFunction
{
    public static void SetTargetable(this ScriptAccessory sa, IGameObject? obj, bool targetable)
    {
        if (obj == null || !obj.IsValid())
        {
            sa.Log.Error($"Provided IGameObject is invalid.");
            return;
        }
        unsafe
        {
            GameObject* charaStruct = (GameObject*)obj.Address;
            if (targetable)
            {
                if (obj.IsDead || obj.IsTargetable) return;
                charaStruct->TargetableStatus |= ObjectTargetableFlags.IsTargetable;
            }
            else
            {
                if (!obj.IsTargetable) return;
                charaStruct->TargetableStatus &= ~ObjectTargetableFlags.IsTargetable;
            }
        }
        sa.Log.Debug($"SetTargetable {targetable} => {obj.Name} {obj}");
    }

    public static void SetRotation(this ScriptAccessory sa, IGameObject? obj, float rotation)
    {
        if (obj == null || !obj.IsValid())
        {
            sa.Log.Error($"Provided IGameObject is invalid.");
            return;
        }
        unsafe
        {
            GameObject* charaStruct = (GameObject*)obj.Address;
            charaStruct->SetRotation(rotation);
        }
        sa.Log.Debug($"SetRotation => {obj.Name.TextValue} | {obj} => {rotation}");
    }
}

#endregion Special Functions

#endregion Function Collections
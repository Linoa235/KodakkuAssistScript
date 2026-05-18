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
using KodakkuAssist.Data;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Extensions;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs;
using KodakkuAssist.Module.Script.Type;

namespace UsamisKodakku.Scripts._06_EndWalker.DSR;

[ScriptType(name: Name, territorys: [968, 1112], guid: "d5b96147-b74b-4bcb-8f1e-1f96f0d998bd", 
    version: Version, Author: "Linoa235", note: NoteStr, updateInfo: UpdateInfo)]

// ^(?!.*((Monk|Machinist|Dragoon|Samurai|Ninja|Viper|Reaper|Dancer|Bard|Astrologian|Sage|Scholar|(Eos|Selene)|Seraph|White Mage|Warrior|Paladin|Dark Knight|Gunbreaker|Pictomancer|Black Mage|Blue Mage|Summoner|Carbuncle|Demigod Bahamut|Demigod Phoenix|Garuda-Egi|Titan-Egi|Ifrit-Egi|Puppet)\] (Used|Cast))).*$

public class DsrPatch
{
    private const string NoteStr =
        """
        Personal additions based on K's DSR drawings.
        Please check and configure the "User Settings" section as needed.
        Type "/e =Exaflare" in the Reminiscence Court to test the special Exaflare movement strategy.
        Duckism.
        """;
    
    private const string Name = "DSR_Patch [Dragonsong's Reprise Ultimate Patch]";
    private const string Version = "0.0.0.16";
    private const string DebugVersion = "a";
    
    private const string UpdateInfo =
        """
        1. Adapted to Kodakku 0.5.x.x
        """;
    
    private const bool Debugging = false;
    
    [UserSetting("Position hint circle drawing - Normal color")]
    public static ScriptColor PosColorNormal { get; set; } = new ScriptColor { V4 = new Vector4(1.0f, 1.0f, 1.0f, 1.0f) };
    [UserSetting("Position hint circle drawing - Player position color")]
    public static ScriptColor PosColorPlayer { get; set; } = new ScriptColor { V4 = new Vector4(0.0f, 1.0f, 1.0f, 1.0f) };
    
    public enum ExaflareSpecStrategyEnum
    {
        NeverFront,
        NeverUniverse,
        LeastMovement,
        AlwaysFront,
        PleaseDontDoThat,
    }
    [UserSetting("Exaflare Guidance Special Strategy")]
    public static ExaflareSpecStrategyEnum ExaflareStrategy { get; set; } = ExaflareSpecStrategyEnum.NeverUniverse;
    
    [UserSetting("Use built-in program colors for Exaflare (Beijing Flare)")]
    public static bool ExaflareBuiltInColor { get; set; } = true;
    [UserSetting("Exaflare (Beijing Flare) Explosion Zone Color")]
    public ScriptColor ExaflareColor { get; set; } = new ScriptColor { V4 = new Vector4(1.0f, 1.0f, 0f, 1.0f) };
    [UserSetting("Draw warning zone for the next Exaflare")]
    public static bool ExaflareWarnDrawn { get; set; } = true;
    [UserSetting("Exaflare Warning Zone Color")]
    public ScriptColor ExaflareWarnColor { get; set; } = new ScriptColor { V4 = new Vector4(0.6f, 0.6f, 1.0f, 1.0f) };
    
    private enum DsrPhase
    {
        Init,                   // Initial
        Phase2Strength,         // P2 Mechanic 1
        Phase2Sancity,          // P2 Mechanic 2
        Phase3Nidhogg,          // P3 Nidhogg
        Phase4Eyes,             // P4 Dragon's Eyes
        Phase5HeavensWrath,     // P5 Mechanic 1
        Phase5HeavensDeath,     // P5 Mechanic 2
        Phase6IceAndFire1,      // P6 First Ice and Fire
        Phase6NearOrFar1,       // P6 First Near/Far
        Phase6Flame,            // P6 Cross Fire
        Phase6NearOrFar2,       // P6 Second Near/Far
        Phase6IceAndFire2,      // P6 Second Ice and Fire
        Phase6Cauterize,        // P6 Divebomb
        Phase7Exaflare1,        // P7 First Exaflare
        Phase7Stack1,           // P7 First Stack
        Phase7Nuclear1,         // P7 First Flare
        Phase7Exaflare2,        // P7 Second Exaflare
        Phase7Stack2,           // P7 Second Stack
        Phase7Nuclear2,         // P7 Second Flare
        Phase7Exaflare3,        // P7 Third Exaflare
        Phase7Stack3,           // P7 Third Stack
        Phase7Enrage,           // P7 Enrage
    }
    
    private static List<string> _role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
    private static Vector3 _center = new Vector3(100, 0, 100);
    private DsrPhase _dsrPhase = DsrPhase.Init;
    private List<bool> _drawn = new bool[20].ToList();                  // Drawing record
    private volatile List<bool> _recorded = new bool[20].ToList();      // Recorded flags
    private int _pureOfHeartBaitCount = 0;                              // P1/P4.5 Pure of Heart bait count
    private List<bool> _p2SafeDirection = new bool[8].ToList();         // P2 Mechanic 1 Thrust safe positions
    private Vector3 _p2ThordanPos = new Vector3(0, 0, 0);               // P2 Thordan's position
    private List<uint> _p2TetherKnightId = [0, 0];                      // P2 Mechanic 1 tethered knight IDs, left and right
    private bool _p3DfgEnable = false;                                  // P3 Guidance enable
    private static PriorityDict _dfg = new PriorityDict();              // P3 mechanic record
    private List<Vector3> _p3TowerAppearPos = [];                       // P3 tower spawn positions
    private int _p4MirageDiveNum = 0;                                   // P4 Mirage Dive count
    private bool _p4PrepareToCenter = false;                            // P4 Mirage Dive prepare to go back to center
    private List<bool> _p4MirageDiveNumFirstRoundTarget = new bool[8].ToList();         // P4 Mirage Dive first round targets
    private List<int> _p4MirageDivePos = [];                            // P4 Mirage Dive target directions (top-left as 0, increasing clockwise)
    private Vector3 _p5VedrfolnirPos = new Vector3(0, 0, 0);            // P5 Vedrfolnir's position
    private List<bool> _p6DragonsGlowAction = [false, false];           // P6 Dragon's Glow record
    private List<bool> _p6DragonsWingAction = [false, false, false];    // P6 Dragon's Wing record [Far T/ Near F, Left safe T/ Right safe F, Front safe T/ Back safe F / Inner safe T/ Outer safe F]
    private List<bool> _p7FirstEnmityOrder = [false, false];            // P7 Auto-attack enmity record
    private readonly List<int> _p7TrinityOrderIdx = [4, 5, 6, 7, 2, 3]; // P7 Trinity order
    private bool _p7TrinityDisordered = false;                          // P7 Trinity order error flag
    private bool _p7TrinityTankDisordered = false;                      // P7 Tank Trinity enmity error flag
    private int _p7TrinityNum = 0;                                      // P7 Trinity count
    private DsrExaflare? _p7Exaflare = null;                            // P7 Exaflare class
    private uint _p7BossId = 0;                                         // P7 boss Id
    
    private ManualResetEvent _thrustEvent = new(false);
    private ManualResetEvent _thordanCastAtEdgeEvent = new(false);
    private ManualResetEvent _mirageDiveRound = new(false);
    private ManualResetEvent _p5VedrfolnirPosRecordEvent = new(false);
    private ManualResetEvent _iceAndFireEvent = new(false);
    private ManualResetEvent _nearOrFarWingsEvent = new(false);
    private ManualResetEvent _nearOrFarCauterizeEvent = new(false);
    private ManualResetEvent _nearOrFarInOutEvent = new(false);
    private ManualResetEvent _bladeEvent = new(false);
    private ManualResetEvent _trinityEvent = new(false);
    
    private const uint ChariotBlade = 298;
    
    public void Init(ScriptAccessory sa)
    {
        sa.Log.Debug($"Init {Name} v{Version}{DebugVersion} Success.\n{UpdateInfo}");
        sa.Method.MarkClear();
        sa.Method.RemoveDraw(".*");
        
        _dsrPhase = DsrPhase.Init;
        _drawn = new bool[20].ToList();
        _recorded = new bool[20].ToList();
        _p7BossId = 0;
        _pureOfHeartBaitShown = false;
        
        _thordanCastAtEdgeEvent = new ManualResetEvent(false);
        _thrustEvent = new ManualResetEvent(false);
        _mirageDiveRound = new ManualResetEvent(false);
        _p5VedrfolnirPosRecordEvent = new ManualResetEvent(false);
        _iceAndFireEvent = new ManualResetEvent(false);
        _nearOrFarWingsEvent = new ManualResetEvent(false);
        _nearOrFarCauterizeEvent = new ManualResetEvent(false);
        _nearOrFarInOutEvent = new ManualResetEvent(false);
        _bladeEvent = new ManualResetEvent(false);
        _trinityEvent = new ManualResetEvent(false);
    }
    
    #region P1
    
    [ScriptMethod(name: "---- ã€ŠP1&P4.5: Door Bossã€‹ ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_PhaseDoorBoss(Event @event, ScriptAccessory accessory)
    {
    }
    
    private bool _pureOfHeartBaitShown = false;
    [ScriptMethod(name: "Pure of Heart Bait", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25316"], 
        userControl: true)]
    public void PureOfHeartBait(Event @event, ScriptAccessory accessory)
    {
        _pureOfHeartBaitCount = 0;
        _pureOfHeartBaitShown = true;
        // Pure of Heart bait order: H1 H2, D3 D4, D1 D2, MT ST
        var myIndex = accessory.GetMyIndex();
        // This is the first Pure of Heart, if not H1/H2, do not participate
        if (myIndex is not (2 or 3)) return;
        // todo modify delay and destroy
        DrawPureOfHeartBait(accessory, 0, 15000);
    }

    private void DrawPureOfHeartBait(ScriptAccessory sa, int delay, int destroy)
    {
        var myIndex = sa.GetMyIndex();
        Vector3[] baitPos = [new(86.5f, 0.0f, 107.0f), new(86.5f, 0.0f, 103.0f), new(91.5f, 0.0f, 107.0f), new(91.5f, 0.0f, 103.0f)];   //91.5
        var baitPosIdx = myIndex % 2;   // High above, low below
        if (myIndex is 0 or 1 or 6 or 7)
            baitPosIdx += 2;
        for (var posIdx = 0; posIdx < 4; posIdx++)
        {
            var color = baitPosIdx == posIdx ? PosColorPlayer.V4 : PosColorNormal.V4;
            var dp = sa.DrawStaticCircle(baitPos[posIdx], color, delay, destroy, $"PureOfHeart", 0.5f);
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            if (baitPosIdx != posIdx) continue;
            var dpGuide = sa.DrawGuidance(baitPos[posIdx], delay, destroy, $"PureOfHeartGuidance");
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dpGuide);
        }
    }
    
    [ScriptMethod(name: "Pure of Heart Subsequent Bait", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25369"], 
        userControl: false)]
    public void PureOfHeartBaitRest(Event @event, ScriptAccessory sa)
    {
        if (!_pureOfHeartBaitShown) return;
        if (@event.TargetIndex() != 1) return;
        var myIndex = sa.GetMyIndex();
        lock (this)
        {
            _pureOfHeartBaitCount++;
            sa.Log.Debug($"Pure of Heart bait count: {_pureOfHeartBaitCount}");
            if (_pureOfHeartBaitCount > 6) return;
            var baitDict = new Dictionary<int, int> { { 1, 6 }, { 2, 7 }, { 3, 4 }, { 4, 5 }, { 5, 0 }, { 6, 1 } };
            if (baitDict[_pureOfHeartBaitCount] != myIndex) return;
            sa.Log.Debug($"Start drawing player's Pure of Heart bait");
            DrawPureOfHeartBait(sa, 0, 5000);
        }
    }
    
    #endregion P1
    
    #region P2

    [ScriptMethod(name: "---- ã€ŠP2: King Thordanã€‹ ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_PhaseKingThordan(Event @event, ScriptAccessory accessory)
    {
    }
    
    [ScriptMethod(name: "Invisible Cleave Bait Range", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25545"])]
    public void P2_AscalonConcealed(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        var dp = accessory.DrawFan(sid, float.Pi / 6, 0, 30, 0, 0, 1500, $"InvisibleCleave");
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.5f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "Mechanic 1 Phase Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25555"], userControl: false)]
    public void P2_StrengthPhaseRecord(Event @event, ScriptAccessory sa)
    {
        _dsrPhase = DsrPhase.Phase2Strength;
        _p2SafeDirection = new bool[8].ToList();
        _p2ThordanPos = new Vector3(0, 0, 0);
        _p2TetherKnightId = [0, 0];
        sa.Log.Debug($"Current phase: {_dsrPhase}");
    }
    
    [ScriptMethod(name: "Mechanic 1 Thrust Direction Record", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:regex:^(378[123])$"], userControl: false)]
    public void ThurstDirectionRecord(Event @event, ScriptAccessory sa)
    {
        if (_dsrPhase != DsrPhase.Phase2Strength) return;

        var spos = @event.SourcePosition();
        var dir = spos.Position2Dirs(_center, 8);
        lock (_p2SafeDirection)
        {
            _p2SafeDirection[dir % 4] = true;
            sa.Log.Debug($"Number of true inside List: {_p2SafeDirection.Count(x => x)}");
            if (_p2SafeDirection.Count(x => x) != 3) return;
            _thrustEvent.Set();
        }
    }
        
    [ScriptMethod(name: "Mechanic 1 Spread Safe Position Guidance", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:3781"], userControl: true)]
    public void ThrustSafePosDraw(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase2Strength) return;
        _thrustEvent.WaitOne();
        
        // Since _p2SafeDirection has its direction modulo 4, it must be 0, 1, 2, 3, the unique false can be found among the first 4 indices.
        var safeDir = _p2SafeDirection.IndexOf(false);
        var northPos = new Vector3(100, 0, 80);
        var myIndex = accessory.GetMyIndex();
        var isStGroup = myIndex % 2 == 1;
        // ST group in 0, 1, 2, 3
        var tposCenter =
            northPos.RotatePoint(_center, isStGroup ? safeDir * float.Pi / 4 : (safeDir + 4) * float.Pi / 4);
        var tposIn = tposCenter.PointInOutside(_center, 7.5f);
        var tposLeft = tposCenter.RotatePoint(_center, 20f.DegToRad());
        var tposRight = tposCenter.RotatePoint(_center, -20f.DegToRad());
        List<Vector3> tposList = [tposCenter, tposIn, tposLeft, tposRight];

        var dp = accessory.DrawGuidance(tposList[myIndex / 2], 0, 7000, $"P2Mechanic1SafePos{myIndex}");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        
        _thrustEvent.Reset();
    }
    
    [ScriptMethod(name: "Mechanic 1 Spread Safe Position Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25548"], userControl: false)]
    public void ThrustSafePosRemove(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase2Strength) return;
        var myIndex = accessory.GetMyIndex();

        accessory.Method.RemoveDraw($"P2Mechanic1SafePos{myIndex}");
    }
    
    [ScriptMethod(name: "Mechanic 1 Thordan Edge Position Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25550"], userControl: false)]
    public void ThordanPosRecord(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase2Strength) return;
        var spos = @event.SourcePosition();
        _p2ThordanPos = spos;
        _thordanCastAtEdgeEvent.Set();
    }
    
    [ScriptMethod(name: "Mechanic 1 Tank Tether Hint", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:regex:^(255[01])$"], userControl: true)]
    public void TankTetherRouteGuidance(Event @event, ScriptAccessory sa)
    {
        if (_dsrPhase != DsrPhase.Phase2Strength) return;
        var myIndex = sa.GetMyIndex();
        if (myIndex > 1) return;
        _thordanCastAtEdgeEvent.WaitOne();
        lock (_p2TetherKnightId)
        {
            var sid = @event.SourceId();
            var sname = @event.SourceName();
            var spos = @event.SourcePosition();
            // var rad = spos.FindRadian(_p2ThordanPos);
            
            var atRight = spos.IsAtRight(_p2ThordanPos, _center);
            _p2TetherKnightId[atRight ? 1 : 0] = sid;
            
            sa.Log.Debug($"Recorded {sname} (dialog {@event.Id()}) on the {(atRight ? "right" : "left")}");

            if (_p2TetherKnightId.Contains(0)) return;
            var targetKnightIdx = myIndex == 0 ? 0 : 1;
            var chara = sa.GetById(_p2TetherKnightId[targetKnightIdx]);
            if (chara == null) return;
            
            var knightPos = chara.Position;
            var tetherEdgePos = _p2ThordanPos.RotatePoint(_center, (myIndex == 0 ? 1 : -1) * 18f.DegToRad());
            tetherEdgePos = tetherEdgePos.PointInOutside(_center, 3f);
            var dp = sa.DrawGuidance(knightPos, tetherEdgePos, 0, 10000, $"TetherPath");
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
    }
    
    [ScriptMethod(name: "Mechanic 1 Tether Hint Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25550"], userControl: false)]
    public void TankTetherRouteGuidanceRemove(Event @event, ScriptAccessory sa)
    {
        if (_dsrPhase != DsrPhase.Phase2Strength) return;
        sa.Method.RemoveDraw($"TetherPath");
        _thordanCastAtEdgeEvent.Reset();
    }
    
    [ScriptMethod(name: "Mechanic 2 Phase Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25569"], userControl: false)]
    public void P2_SancityPhaseRecord(Event @event, ScriptAccessory sa)
    {
        _dsrPhase = DsrPhase.Phase2Sancity;
        sa.Log.Debug($"Current phase: {_dsrPhase}");
    }
    
    #endregion P2

    #region P3
    
    [ScriptMethod(name: "---- ã€ŠP3: Nidhoggã€‹ ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_PhaseNidhogg(Event @event, ScriptAccessory accessory)
    {
    }
    
    [ScriptMethod(name: "P3: Phase Record", eventType: EventTypeEnum.ActionEffect,
        eventCondition: ["ActionId:26376"], userControl: Debugging)]
    public void P3_PhaseRecord(Event ev, ScriptAccessory sa)
    {
        _dsrPhase = DsrPhase.Phase3Nidhogg;
        _p3DfgEnable = false;
        // Hundreds: First stack +0, Second +100, Third +100
        // Tens: Down arrow +0, Middle +10, Up arrow +20
        // Units: Left/Middle/Right positions +0, +1, +2
        // Arranged so units can change anytime, after tens change units cannot interfere
        _dfg.Init(sa, "Fell Dragon's Wrath");
        _p3TowerAppearPos = [];
        sa.Log.Debug($"Current phase: {_dsrPhase}");
    }

    [ScriptMethod(name: "Fell Dragon's Wrath Process Guidance", eventType: EventTypeEnum.StatusAdd,
        eventCondition:["StatusID:regex:^(300[456])$"], userControl: true)]
    public void P3_LimitCutRecord(Event ev, ScriptAccessory sa)
    {
        if (_dsrPhase != DsrPhase.Phase3Nidhogg) return;
        _p3DfgEnable = true;
        var stid = ev.StatusId;
        var tid = ev.TargetId;
        var tidx = sa.GetPlayerIdIndex(tid);
        
        var lmVal = stid switch
        {
            3004 => 0,      // First stack
            3005 => 100,    // Second stack
            3006 => 200,    // Third stack
            _ => 0
        };
        lock (_dfg)
        {
            // First three first stack, middle two second stack, last three third stack
            _dfg.AddPriority(tidx, lmVal);
            sa.Log.Debug($"Player {sa.GetPlayerJobByIndex(tidx)} is stack {lmVal/100+1}.");
        }
    }
    
    [ScriptMethod(name: "Arrow Record", eventType: EventTypeEnum.StatusAdd,
        eventCondition:["StatusID:regex:^(275[567])$"], userControl: Debugging)]
    public void P3_LimitCutPosRecord(Event ev, ScriptAccessory sa)
    {
        if (_dsrPhase != DsrPhase.Phase3Nidhogg) return;
        if (!_p3DfgEnable) return;
        lock (_dfg)
        {
            var stid = ev.StatusId;
            var tid = ev.TargetId;
            var tidx = sa.GetPlayerIdIndex(tid);
        
            var dirVal = stid switch
            {
                2756 => 20, // Up arrow, Up B
                2757 => 0, // Down arrow, Down D
                2755 => 10, // Middle
                _ => 10
            };
            
            _dfg.AddPriority(tidx, dirVal);
            _dfg.AddActionCount();
            sa.Log.Debug($"Player {sa.GetPlayerJobByIndex(tidx)} arrow: {dirVal switch
            {
                0 => "Down Arrow",
                10 => "Middle",
                _ => "Up Arrow"
            }}.");
            
            if (_dfg.ActionCount != 8) return;
            
            // Get own value and update based on position
            var myPriority = _dfg.Priorities[sa.GetMyIndex()];
            RefreshGroupPosPriority(sa, myPriority);
            sa.Log.Debug($"Player's value in {_dfg.Annotation} mechanic: {myPriority}");
        }
    }
    
    private void RefreshGroupPosPriority(ScriptAccessory sa, int myPriority)
    {
        // Get same group player Ids
        var myGroupVal = (myPriority / 100) switch
        {
            // Value meaning here:
            // Tens: start index
            // Units: how many players to take
            0 => 3,
            1 => 32,
            2 => 53,
            _ => 0
        };
        
        if (myGroupVal == 0)
        {
            sa.Log.Error($"GetDfgGroupPlayers: myGroupVal == 0");
            return;
        }
        
        var myGroupDict = _dfg.SelectMiddlePriorityIndices(myGroupVal / 10, myGroupVal % 10);
        List<KeyValuePair<int, ulong>> myGroupPlayerIds = [];
        for (int i = 0; i < myGroupVal % 10; i++)
        {
            var pidx = myGroupDict[i].Key;
            var eid = sa.Data.PartyList[pidx];
            var prior = myGroupDict[i].Value;
            myGroupPlayerIds.Add(new KeyValuePair<int, ulong>(pidx, eid));
            sa.Log.Debug($"Player in same group: {sa.GetPlayerJobByIndex(pidx)}, their priority value is {prior}, EntityId is {eid}");
        }
        
        // Sort based on left/right position within the group
        var sortedGroupPlayerIds = myGroupPlayerIds
            .OrderBy(v => sa.GetById(v.Value).Position.X)
            .ToList();

        // Add values to priority dictionary based on sorting
        for (int i = 0; i < sortedGroupPlayerIds.Count; i++)
        {
            var pidx = sortedGroupPlayerIds[i].Key;
            // Remove the units digit
            _dfg.Priorities[pidx] = _dfg.Priorities[pidx] / 10 * 10;
            _dfg.AddPriority(pidx, i);
            
            sa.Log.Debug($"Detected {sa.GetPlayerJobByIndex(pidx)} at {GetDfgPosStr(i, sortedGroupPlayerIds.Count == 2)}, updated priority value to {_dfg.Priorities[pidx]}");
        }
    }
    
    private string GetDfgPosStr(int myDfgIdx, bool isSecondRound = false)
    {
        var str = myDfgIdx switch
        {
            0 => "Left",
            1 => "Middle",
            2 => "Right",
            3 => "Left",
            4 => "Right",
            5 => "Left",
            6 => "Middle",
            7 => "Right",
            _ => "Unknown"
        };

        if (isSecondRound && myDfgIdx is 0 or 1)
            str = myDfgIdx == 1 ? "Right" : "Left";
        return str;
    }
    
    private Vector3 GetDfgTowerPosV3(int myDfgIdx)
    {
        var towerPos = myDfgIdx switch
        {
            0 => new Vector3(_center.X - 7.5f, 0, _center.Z),
            1 => new Vector3(_center.X, 0, _center.Z + 7.5f),
            2 => new Vector3(_center.X + 7.5f, 0, _center.Z),
            3 => new Vector3(91.75f, 0, 90.8f),
            4 => new Vector3(108.25f, 0, 90.8f),
            5 => new Vector3(_center.X - 7.5f, 0, _center.Z),
            6 => new Vector3(_center.X, 0, _center.Z + 7.5f),
            7 => new Vector3(_center.X + 7.5f, 0, _center.Z),
            _ => new Vector3(0, 0, 0)
        };
        return towerPos;
    }
    
    [ScriptMethod(name: "Mahjong Process, Tower and Stack", eventType: EventTypeEnum.StartCasting,
        eventCondition:["ActionId:regex:^(2638[67])$"], userControl: Debugging)]
    public void P3_LimitCutAction(Event @event, ScriptAccessory sa)
    {
        if (_dsrPhase != DsrPhase.Phase3Nidhogg) return;
        if (!_p3DfgEnable) return;
        _dfg.AddActionCount(10);
        // Only need the sorting to know the mahjong process
        var myPriority = _dfg.Priorities[sa.GetMyIndex()];
        var myDfgIdx = _dfg.FindPriorityIndexOfKey(sa.GetMyIndex());
        var hasArrow = myPriority / 10 % 10 != 1;
        var posStr = GetDfgPosStr(myDfgIdx, myDfgIdx is 3 or 4);
        var towerPos = GetDfgTowerPosV3(myDfgIdx);
        
        const int lashGnashCastTime = 7600;
        const int inOutCastFirst = 3700;
        const int inOutCastSecond = 3100;
        const int towerExistTime = 6800;
        
        if (_dfg.ActionCount == 18) // Under normal circumstances, this value is 18 during the first Chariot/Donut cast. Five tower placements happen during this time. It becomes 33 during the second Chariot/Donut cast.
        {
            switch (myDfgIdx)
            {
                case 0:
                case 1:
                case 2:
                    sa.Log.Debug($"First stack {posStr} Round 1, go {posStr}{towerPos} to place tower, then return to group");
                    DrawTowerDir(towerPos, 0, lashGnashCastTime, $"PlaceTower1", sa);
                    // Tens digit represents arrow, if 1 then middle, no need to draw facing
                    DrawTowerPosDir(towerPos, 0, lashGnashCastTime, $"PlaceTower1Facing", sa, hasArrow);
                    DrawBackToGroup(lashGnashCastTime, towerExistTime, $"Group", sa);
                    break;
                case 3:
                case 4:
                    sa.Log.Debug($"Second stack {posStr} Round 1, first return to group, then go {posStr}{towerPos} to place tower");
                    DrawBackToGroup(0, lashGnashCastTime, $"Group", sa);
                    const int jump2DelayTime = lashGnashCastTime + inOutCastFirst + inOutCastSecond;
                    const int jump2Destroy = 17700 - jump2DelayTime;  // 17700 taken from the time node below
                    DrawTowerDir(towerPos, jump2DelayTime, jump2Destroy, $"PlaceTower2", sa);
                    DrawTowerPosDir(towerPos, jump2DelayTime, jump2Destroy, $"PlaceTower2Facing", sa, hasArrow);
                    break;
                case 5:
                case 6:
                case 7:
                    sa.Log.Debug($"Third stack {posStr} Round 1, return to group");
                    DrawBackToGroup(0, lashGnashCastTime, $"Group", sa);
                    break;
            }
        }
        else if (_dfg.ActionCount == 33)
        {
            switch (myDfgIdx)
            {
                case 0:
                case 2:
                    sa.Log.Debug($"First stack {posStr} Round 2, bait then return to group");
                    DrawBackToGroup(26900 - 21500, 28900 - 26900, $"Stack", sa);
                    break;
                case 1:
                    sa.Log.Debug($"First stack {posStr} Round 2, return to group");
                    DrawBackToGroup(0, lashGnashCastTime, $"Stack", sa);
                    break;
                case 3:
                case 4:
                    sa.Log.Debug($"Second stack {posStr} Round 2, return to group");
                    DrawBackToGroup(0, lashGnashCastTime, $"Stack", sa);
                    break;
                case 5:
                case 6:
                case 7:
                    sa.Log.Debug($"Third stack {posStr} Round 2, first go {posStr}{towerPos} to place tower, then return to group");
                    DrawTowerDir(towerPos, 0, lashGnashCastTime, $"PlaceTower", sa);
                    DrawTowerPosDir(towerPos, 0, lashGnashCastTime, $"PlaceTower3Facing", sa, hasArrow);
                    DrawBackToGroup(lashGnashCastTime, towerExistTime, $"Group", sa);
                    break;
            }
        }
        else
        {
            sa.Log.Error($"P3_LimitCutAction error, _dfg.ActionCount = {_dfg.ActionCount}");
        }
    }
    
    [ScriptMethod(name: "Mahjong Process, Tower Step Guidance", eventType: EventTypeEnum.ActionEffect, 
        eventCondition:["ActionId:regex:^(2638[234])$", "TargetIndex:1"], userControl: Debugging)]
    public void P3_TowerAfterPlaced(Event ev, ScriptAccessory sa)
    {
        if (_dsrPhase != DsrPhase.Phase3Nidhogg) return;
        // This is the tower placement. If the player group doesn't follow the pre-positioning, there's a chance to adjust the script here.
        if (!_p3DfgEnable) return;
        lock (_dfg)
        {
            _dfg.AddActionCount();
            var tid = ev.TargetId;
            var aid = ev.ActionId;
            var sid = ev.SourceId;
            var myDfgIdx = _dfg.FindPriorityIndexOfKey(sa.GetMyIndex());
            // The sid of the subsequently generated tower position is no longer the original sid, need to find its position after offset here.
            var tpos = GetTowerAppearPos(sa, sid, aid);
            _p3TowerAppearPos.Add(tpos);
            
            var towerRound = _dfg.ActionCount switch
            {
                21 => 0,
                23 => 1,
                36 => 2,
                _ => -1
            };
            if (towerRound == -1)
            {
                sa.Log.Debug($"_dfg.ActionCount == {_dfg.ActionCount}, value not reached, exiting");
                return;
            }
            
            var myPriority = _dfg.Priorities[sa.GetMyIndex()];
            // First/Second/Third stack player places tower, refresh relative positions within the group to change subsequent logic
            if (towerRound == myPriority / 100)
                RefreshGroupPosPriority(sa, myPriority);
            
            // Sort the three tower coordinates left to right
            _p3TowerAppearPos.Sort((pos1, pos2) => pos1.X.CompareTo(pos2.X));
            
            // Input the current round, my priority rank, draw towers
            DrawTowerRange(sa, towerRound, myDfgIdx, myPriority);
            
            // Clear towers
            _p3TowerAppearPos = [];
        }
    }
    
    private DrawPropertiesEdit DrawTowerDir(Vector3 towerPos, int delay, int destroy, string name, ScriptAccessory accessory, bool draw = true)
    {
        var dp = accessory.DrawDirPos(towerPos, delay, destroy, name);
        if (draw)
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        return dp;
    }
    private DrawPropertiesEdit DrawTowerPosDir(Vector3 towerPos, int delay, int destroy, string name, ScriptAccessory accessory, bool draw = true)
    {
        const int left = 0;
        const int middle = 1;
        const int right = 2;

        var targetPos = towerPos.ExtendPoint(-90f.DegToRad(), 3.1f);
        var dp = accessory.DrawDirPos2Pos(towerPos, targetPos, delay, destroy, name);
        dp.Scale = new Vector2(3f);
        dp.Color = ColorHelper.ColorYellow.V4;
        if (draw)
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);
        return dp;
    }
    
    private DrawPropertiesEdit DrawBackToGroup(int delay, int destroy, string name, ScriptAccessory accessory, bool draw = true)
    {
        var stackPos = new Vector3(100, 0, 92);
        var dp = accessory.DrawDirPos(stackPos, delay, destroy, name);
        if (draw)
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        return dp;
    }

    private void DrawTowerRange(ScriptAccessory sa, int towerRound, int myDfgIdx, int myPriority)
    {
        // Calculate duration
        // towerExistTime - towerCastingTime
        //     0, 6800 - 3000  => 3800
        //     6800 - 3000 + 300, 3000     => 3300
        //         => 7100
        
        const int towerExistTime = 7100;

        var myRound = myDfgIdx switch
        {
            // Which round of tower the player needs to stand on
            0 => 1,
            2 => 1,
            1 => 2,
            3 => 2,
            4 => 2,
            5 => 0,
            6 => 0,
            7 => 0,
            _ => -1
        };
        if (myRound == -1)
        {
            sa.Log.Error($"myDfgIdx = {myDfgIdx} resulted in myRound = {myRound}");
            return;
        }
        var isMyRound = myRound == towerRound;
        var myTowerPos = GetDfgPosStr(myDfgIdx);
        
        for (int i = 0; i < _p3TowerAppearPos.Count; i++)
        {
            // This is the player's tower placement round, and this tower matches the player's direction
            var thisTowerPos = GetDfgPosStr(i, towerRound == 1);
            var isMyTower = isMyRound && (thisTowerPos == myTowerPos);

            var color = isMyTower ? sa.Data.DefaultSafeColor.WithW(1.5f) : sa.Data.DefaultDangerColor;
            var dp1 = sa.DrawStaticCircle(_p3TowerAppearPos[i], color, 0, towerExistTime, $"Tower{towerRound}{thisTowerPos}", 5f);
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
            
            if (!isMyTower) continue;
            sa.Log.Debug($"Detected player needs to stand on tower {myRound} round {myTowerPos} tower");
            var dp01 = sa.DrawDirPos(_p3TowerAppearPos[i], 0, towerExistTime, $"Tower{towerRound}{thisTowerPos}Guidance");
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp01);
        }
    }
    
    private Vector3 GetTowerAppearPos(ScriptAccessory sa, ulong sid, uint type)
    {
        // const uint inPlace = 26382;
        // const uint front = 26383;
        // const uint behind = 26384;
        
        var chara = sa.GetById(sid);
        var srot = chara.Rotation;
        var spos = chara.Position;
        
        if (type == 26382) return spos;
        var newPos = spos.ExtendPoint(srot.Game2Logic(), 14);
        return newPos;
    }
    
    // 0        Casting LashGnash           0
    // +7600    Stack #1 + Jump #1          7600
    // +3700    Chariot/Donut #1            11300
    // +3100    Donut/Chariot #1            14400
    // +0       Towers #1                   14400
    // +2500    StartCast Geirskogul #1     16900
    // +800     Jump #2                     17700
    // +3800    Casting LashGnash           21500
    // +2800    Towers #2                   24300
    // +2600    StartCast Geirskogul #2     26900
    // +2200    Stack #2 + Jump #3          28900
    // +3700    Chariot/Donut #2            32600
    // +3100    Donut/Chariot #2            35700
    // +0       Towers #3                   35700
    // +2000    StartCast Geirskogul #3     37700
    // +4500    Geirskogul #3               42200
    
    // TowerExistTime       6800, 6600, 6800
    // PlaceTowerTimeNode   7600, 17700, 28900
    
    #endregion P3

    #region P4

    [ScriptMethod(name: "---- ã€ŠP4: Dragon's Eyeã€‹ ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_PhaseEyes(Event @event, ScriptAccessory accessory)
    {
    }

    [ScriptMethod(name: "P4 Phase Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2748"],
        userControl: false)]
    public void P4_EyesPhaseRecord(Event @event, ScriptAccessory sa)
    {
        if (_dsrPhase == DsrPhase.Phase4Eyes) return;
        _dsrPhase = DsrPhase.Phase4Eyes;
        _p4MirageDiveNum = 0;
        _p4MirageDiveNumFirstRoundTarget = new bool[8].ToList();
        _p4MirageDivePos = [];
        _p4PrepareToCenter = false;
        sa.Log.Debug($"Current phase: {_dsrPhase}");
    }
    
    [ScriptMethod(name: "Opening Position Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2748"],
        userControl: true)]
    public void EyesTargetMention(Event @event, ScriptAccessory accessory)
    {
        var tid = @event.TargetId();
        if (tid != accessory.Data.Me) return;
        var myIndex = accessory.GetMyIndex();
        // MT D1 D2 H1
        var isBlueEye = myIndex is 0 or 2 or 4 or 5;
        var isTank = myIndex is 0 or 1;
        accessory.Method.TextInfo($"{(isTank ? "Turn on tank stance, " : "")}{(isBlueEye ? "Position for left blue orb" : "Position for right red orb")}", 3000, isTank);
    }
    
    [ScriptMethod(name: "Red/Blue Buff Swap Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(277[56])$"],
        userControl: true)]
    public void EyesBuffExchange(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase4Eyes) return;
        var tid = @event.TargetId();
        if (tid != accessory.Data.Me) return;
        const uint redBuff = 2775;
        const uint blueBuff = 2776;
        var stid = @event.StatusId();
        var myIndex = accessory.GetMyIndex();
        if (_drawn[0]) return;
        _drawn[0] = true;
        
        var needChange = (myIndex < 4 && stid != blueBuff) || (myIndex >= 4 && stid != redBuff);
        if (!needChange) return;
        var dp = accessory.DrawGuidance(_center, 0, 5000, $"RedBlueBuffSwap");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        accessory.Method.TextInfo($"Swap buffs in center", 3000);
    }
    
    [ScriptMethod(name: "Red/Blue Buff Swap Remove", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(277[56])$"],
        userControl: false)]
    public void EyesBuffExchangeRemove(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase4Eyes) return;
        var tid = @event.TargetId();
        if (tid != accessory.Data.Me) return;
        const uint redBuff = 2775;
        const uint blueBuff = 2776;
        var stid = @event.StatusId();
        var myIndex = accessory.GetMyIndex();
        
        var changeComplete = (myIndex < 4 && stid == blueBuff) || (myIndex >= 4 && stid == redBuff);
        if (!changeComplete) return;
        accessory.Method.RemoveDraw($"RedBlueBuffSwap");
    }
    
    [ScriptMethod(name: "DPS Orb Bait Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(1260[78])$"],
        userControl: true)]
    public void PobYellowOrbsGuidance(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase4Eyes) return;
        if (_drawn[1]) return;
        _drawn[1] = true;
        // Timer starts when orb appears
        var myIndex = accessory.GetMyIndex();
        if (myIndex < 4) return;

        var orbPos = new Vector3(83, 0, 100);
        if (myIndex is 6 or 7)
            orbPos = orbPos.FoldPointHorizon(_center.X);
        
        // Needs refinement to find the exact time the orb grows
        var dp0 = accessory.DrawGuidance(orbPos, 4000, 2000, $"DPSOrbBaitPrepare");
        dp0.Color = accessory.Data.DefaultDangerColor;
        var dp1 = accessory.DrawGuidance(orbPos, 6000, 5000, $"DPSOrbBait");
        dp1.Color = accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
    }
    
    [ScriptMethod(name: "DPS Orb Bait Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26817"],
        userControl: false)]
    public void PobYellowOrbsGuidanceRemove(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase4Eyes) return;
        var tid = @event.TargetId();
        if (tid != accessory.Data.Me) return;
        var myIndex = accessory.GetMyIndex();
        if (myIndex < 4) return;
        accessory.Method.RemoveDraw($"DPSOrbBait.*");
    }
    
    [ScriptMethod(name: "TN Orb Bait Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(1260[78])$"],
        userControl: true)]
    public void PobBlueOrbsGuidance(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase4Eyes) return;
        if (_drawn[2]) return;
        _drawn[2] = true;
        // Timer starts when orb appears
        var myIndex = accessory.GetMyIndex();
        if (myIndex >= 4) return;

        var orbPos = new Vector3(90, 0, 93);
        if (myIndex >= 2)
            orbPos = orbPos.FoldPointVertical(_center.Z);
        if (myIndex % 2 == 1)
            orbPos = orbPos.FoldPointHorizon(_center.X);
        
        // accessory.Method.TextInfo($"Swap buff with DPS", 2500);
        var dp0 = accessory.DrawGuidance(orbPos, 10000, 2000, $"TNOrbBaitPrepare");
        dp0.Color = accessory.Data.DefaultDangerColor;
        var dp1 = accessory.DrawGuidance(orbPos, 12000, 5000, $"TNOrbBait");
        dp1.Color = accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
    }
    
    [ScriptMethod(name: "Buff Swap Hint Before TN Orb Bait", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26817"],
        userControl: true)]
    public void BuffExchangeHintBeforePobBlueOrbs(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase4Eyes) return;
        if (_drawn[5]) return;
        _drawn[5] = true;
        // Timer starts when orb appears
        var myIndex = accessory.GetMyIndex();
        if (myIndex >= 4) return;
        
        accessory.Method.TextInfo($"Swap buff with DPS", 2500);
    }
    
    [ScriptMethod(name: "TN Orb Bait Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26815"],
        userControl: false)]
    public void PobBlueOrbsGuidanceRemove(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase4Eyes) return;
        var tid = @event.TargetId();
        if (tid != accessory.Data.Me) return;
        var myIndex = accessory.GetMyIndex();
        if (myIndex >= 4) return;
        accessory.Method.RemoveDraw($"TNOrbBait.*");
    }
    
    [ScriptMethod(name: "Mirage Dive Initial Position Hint", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:12607"],
        userControl: true)]
    public void MirageDiveStandPosMention(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase4Eyes) return;
        if (_drawn[3]) return;
        _drawn[3] = true;

        Vector3 targetPos;
        var myIndex = accessory.GetMyIndex();
        if (myIndex >= 4)
            targetPos = new(90, 0, 100);
        else
        {
            targetPos = new(84.5f, 0, 94.5f);
            targetPos = targetPos.RotatePoint(new(90, 0, 100), myIndex * 90f.DegToRad());
        }
        var dp = accessory.DrawGuidance(targetPos, 0, 5000, $"MirageDivePositionHint");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Mirage Dive Count and Target Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26820", "TargetIndex:1"],
        userControl: false)]
    public void MirageDiveNumRecord(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase4Eyes) return;
        var tid = @event.TargetId();
        var tidx = accessory.GetPlayerIdIndex(tid);
        lock (_p4MirageDiveNumFirstRoundTarget)
        {
            _p4MirageDiveNum++;
            if (_p4MirageDiveNum <= 2)
                _p4MirageDiveNumFirstRoundTarget[tidx] = true;
        }

        lock (_p4MirageDivePos)
        {
            var tpos = @event.TargetPosition();
            var tdir = tpos.Position2Dirs(new Vector3(90, 0, 100), 4, false);
            _p4MirageDivePos.Add((tdir + 1) % 4);
            if (_p4MirageDivePos.Count != 2) return;
            _p4MirageDivePos.Sort();
            _mirageDiveRound.Set();
        }
    }
    
    [ScriptMethod(name: "Mirage Dive Await Return to Center Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26820", "TargetIndex:1"],
        userControl: true)]
    public void MirageDiveBackToCenterMentionAwait(Event @event, ScriptAccessory sa)
    {
        if (_dsrPhase != DsrPhase.Phase4Eyes) return;
        if (_p4PrepareToCenter) return;
        var tid = @event.TargetId();
        if (tid != sa.Data.Me) return;
        if (_p4MirageDiveNum > 6) return;
        _p4PrepareToCenter = true;
        
        var dp = sa.DrawGuidance(new Vector3(90, 0, 100), 0, 5000, $"MirageDiveAwaitReturnToCenterHint");
        dp.Color = sa.Data.DefaultDangerColor;
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        sa.Log.Debug($"Player took damage, prepare to return to center");
    }
    
    [ScriptMethod(name: "Mirage Dive Return to Center Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2776"],
        userControl: true)]
    public void MirageDiveBackToCenterMention(Event @event, ScriptAccessory sa)
    {
        if (_dsrPhase != DsrPhase.Phase4Eyes) return;
        if (!_p4PrepareToCenter) return;
        var tid = @event.TargetId();
        if (tid != sa.Data.Me) return;
        if (_p4MirageDiveNum > 6) return;
        _p4PrepareToCenter = false;
        
        sa.Method.RemoveDraw($"MirageDiveAwaitReturnToCenterHint");
        var dp = sa.DrawGuidance(new Vector3(90, 0, 100), 0, 2500, $"MirageDiveReturnToCenterHint");
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        sa.Log.Debug($"Player buff swapped, return to center");
    }
    
    [ScriptMethod(name: "Mirage Dive Swap Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26820", "TargetIndex:1"],
        userControl: true)]
    public void MirageDiveSwapMention(Event @event, ScriptAccessory sa)
    {
        if (_dsrPhase != DsrPhase.Phase4Eyes) return;
        if (_drawn[4]) return;
        _drawn[4] = true;
        _mirageDiveRound.WaitOne();
        
        _drawn[4] = false;
        _mirageDiveRound.Reset();
        
        if (_p4MirageDiveNum > 6) return;
        var highPriorityPlayer = _p4MirageDiveNum switch
        {
            2 => 4,
            4 => 6,
            6 => _p4MirageDiveNumFirstRoundTarget.IndexOf(true),
            _ => 0,
        };
        var lowPriorityPlayer = _p4MirageDiveNum switch
        {
            2 => 5,
            4 => 7,
            6 => _p4MirageDiveNumFirstRoundTarget.LastIndexOf(true),
            _ => 0,
        };
        
        var basePos = new Vector3(84.5f, 0, 94.5f);
        var highPriorityPos = basePos.RotatePoint(new(90, 0, 100), _p4MirageDivePos[0] * 90f.DegToRad());
        var lowPriorityPos = basePos.RotatePoint(new(90, 0, 100), _p4MirageDivePos[1] * 90f.DegToRad());

        var highPriorityPlayerJob = sa.GetPlayerJobByIndex(highPriorityPlayer);
        var lowPriorityPlayerJob = sa.GetPlayerJobByIndex(lowPriorityPlayer);
        var myIndex = sa.GetMyIndex();

        if (myIndex == highPriorityPlayer)
        {
            var dp = sa.DrawGuidance(highPriorityPos, 0, 5000, $"HighPriorityPosition{highPriorityPlayer}");
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        if (myIndex == lowPriorityPlayer)
        {
            var dp = sa.DrawGuidance(lowPriorityPos, 0, 5000, $"LowPriorityPosition{lowPriorityPlayer}");
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        var str = "";
        str += $"Round {_p4MirageDiveNum / 2}, high priority {highPriorityPlayerJob} go to position {_p4MirageDivePos[0]}\n";
        str += $"Round {_p4MirageDiveNum / 2}, low priority {lowPriorityPlayerJob} go to position {_p4MirageDivePos[1]}";
        sa.Log.Debug(str);
        _p4MirageDivePos.Clear();
    }
    
    #endregion P4
    
    #region P5

    [ScriptMethod(name: "---- ã€ŠP5: Alternate Thordanã€‹ ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_PhaseAlternateThordan(Event @event, ScriptAccessory accessory)
    {
    }
    
    [ScriptMethod(name: "P5: Mechanic 1, Phase Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27529"], userControl: false)]
    public void P5_HeavensWrath_PhaseRecord(Event @event, ScriptAccessory sa)
    {
        _dsrPhase = DsrPhase.Phase5HeavensWrath;
        sa.Log.Debug($"Current phase: {_dsrPhase}");
    }

    [ScriptMethod(name: "Twisting Dive Warning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27531"])]
    public async void P5_TwistingDive(Event @event, ScriptAccessory accessory)
    {
        DrawTwister(3000, 3000, accessory);
        await Task.Delay(3000);
        accessory.Method.TextInfo("Twister", 3000, true);
    }

    [ScriptMethod(name: "Twister Danger Positions", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2001168", "Operate:Add"])]
    public void TwisterField(Event @event, ScriptAccessory accessory)
    {
        var spos = @event.SourcePosition();
        var dp = accessory.DrawStaticCircle(spos, ColorHelper.ColorRed.V4.WithW(3), 0, 4000, $"Twister{spos}");
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    private void DrawTwister(int delay, int destroy, ScriptAccessory accessory)
    {
        for (var i = 0; i < accessory.Data.PartyList.Count; i++)
        {
            var dp = accessory.DrawCircle(accessory.Data.PartyList[i], 1.5f, delay, destroy, $"Twister{i}", true);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    [ScriptMethod(name: "Big Circle Flare Warning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25573"])]
    public void P5_AlterFlare(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase5HeavensWrath) return;
        var spos = @event.SourcePosition();
        var dp = accessory.DrawStaticCircle(spos, ColorHelper.ColorRed.V4.WithW(1.5f), 0, 4000, $"BigCircleFlareDanger", 8f);
        dp.ScaleMode |= ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Mechanic 1 Vedrfolnir Position Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27531"],
        userControl: false)]
    public void VedrfolnirPosRecord(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase5HeavensWrath) return;
        var spos = @event.SourcePosition();
        _p5VedrfolnirPos = spos;
        _p5VedrfolnirPosRecordEvent.Set();
    }
    
    [ScriptMethod(name: "Mechanic 1 Tether Guidance", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0005"],
        userControl: true)]
    public void SpiralPierceTetherGuidance(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase5HeavensWrath) return;
        _p5VedrfolnirPosRecordEvent.WaitOne();
        var tid = @event.TargetId();
        if (tid != accessory.Data.Me) return;
        var spos = @event.SourcePosition();
        var atRight = spos.IsAtRight(_p5VedrfolnirPos, _center);
        var targetPos = spos.RotatePoint(_center, (atRight ? 1 : -1) * 172.5f.DegToRad());
        
        targetPos = targetPos.PointInOutside(_center, 2f);
        var dp = accessory.DrawGuidance(targetPos, 0, 8000, $"Mechanic1TetherGuidance");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Mechanic 1 Tether Guidance Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:27530"],
        userControl: false)]
    public void SpiralPierceTetherGuidanceRemove(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase5HeavensWrath) return;
        accessory.Method.RemoveDraw($"Mechanic1TetherGuidance");
    }
    
    [ScriptMethod(name: "Mechanic 1 Skyward Leap Guidance", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:000E"],
        userControl: true)]
    public void SkywardLeapGuidance(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase5HeavensWrath) return;
        _p5VedrfolnirPosRecordEvent.WaitOne();
        var tid = @event.TargetId();
        if (tid != accessory.Data.Me) return;
        
        var targetPos = _p5VedrfolnirPos.RotatePoint(_center, -67.5f.DegToRad());
        targetPos = targetPos.PointInOutside(_center, 2f);
        var dp = accessory.DrawGuidance(targetPos, 0, 8000, $"Mechanic1SkywardLeapGuidance");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "Mechanic 1 Skyward Leap Guidance Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29346"],
        userControl: false)]
    public void SkywardLeapGuidanceRemove(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase5HeavensWrath) return;
        _p5VedrfolnirPosRecordEvent.Reset();
        accessory.Method.RemoveDraw($"Mechanic1SkywardLeapGuidance");
    }
    
    [ScriptMethod(name: "P5: Mechanic 2, Phase Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27538"], userControl: false)]
    public void P5_HeavensDeath_PhaseRecord(Event @event, ScriptAccessory sa)
    {
        _dsrPhase = DsrPhase.Phase5HeavensDeath;
        sa.Log.Debug($"Current phase: {_dsrPhase}");
    }

    [ScriptMethod(name: "Mechanic 2 Axe Knight Direction Guidance", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7747", "SourceDataId:12637"])]
    public void P5_FindSerGuerrique(Event @event, ScriptAccessory sa)
    {
        if (_dsrPhase != DsrPhase.Phase5HeavensDeath) return;
        var spos = @event.SourcePosition();
        sa.Log.Debug($"Found Axe Knight position {spos}");
        var dp = sa.DrawDirPos2Pos(_center, spos, 0, 4000, $"CenterToAxeKnight", 2f);
        dp.Color = ColorHelper.ColorWhite.V4;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }

    #endregion P5
    
    #region P6 Ice & Fire

    [ScriptMethod(name: "---- ã€ŠP6: Twin Dragonsã€‹ ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_PhaseDragons(Event @event, ScriptAccessory accessory)
    {
    }
    
    [ScriptMethod(name: "P6: First Ice & Fire, Phase Record", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:12613"], userControl: false)]
    public void P6_IceAndFire1_PhaseRecord(Event @event, ScriptAccessory sa)
    {
        // Hraesvelgr appearing means entering first Ice & Fire
        if (_dsrPhase != DsrPhase.Phase5HeavensDeath) return;
        _dsrPhase = DsrPhase.Phase6IceAndFire1;
        _p6DragonsGlowAction = [false, false];
        _recorded = new bool[20].ToList();
        sa.Log.Debug($"Current phase: {_dsrPhase}");
    }

    [ScriptMethod(name: "P6: Second Ice & Fire, Phase Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(2794[79])$"], userControl: false)]
    public void P6_IceAndFire2_PhaseRecord(Event @event, ScriptAccessory sa)
    {
        // Hot Wing / Hot Tail as the start of second Ice & Fire
        if (_dsrPhase != DsrPhase.Phase6NearOrFar2) return;
        _dsrPhase = DsrPhase.Phase6IceAndFire2;
        _p6DragonsGlowAction = [false, false];
        _recorded = new bool[20].ToList();
        sa.Log.Debug($"Current phase: {_dsrPhase}");
    }

    
    [ScriptMethod(name: "P6: Ice & Fire Glow Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2795[4567])$"], userControl: false)]
    public void P6_IceAndFireGlowRecord(Event @event, ScriptAccessory accessory)
    {
        const uint blackBuster = 27954;
        const uint whiteBuster = 27956;
        const uint blackGlow = 27955;
        const uint whiteGlow = 27957;
        
        if (_dsrPhase != DsrPhase.Phase6IceAndFire1 && _dsrPhase != DsrPhase.Phase6IceAndFire2) return;
        var aid = @event.ActionId();
        switch (aid)
        {
            case blackBuster:
            case blackGlow:
                _p6DragonsGlowAction[0] = aid == blackGlow;
                break;
            case whiteBuster:
            case whiteGlow:
                _p6DragonsGlowAction[1] = aid == whiteGlow;
                break;
        }

        lock (_recorded)
        {
            _recorded[1] = _recorded[0];
            _recorded[0] = true;
            if (_recorded[0] && _recorded[1])
                _iceAndFireEvent.Set();
        }
    }

    [ScriptMethod(name: "Ice & Fire Tank Buster Solution", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27960"])]
    public void P6_IceAndFireTankSolution(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase is not (DsrPhase.Phase6IceAndFire1 or DsrPhase.Phase6IceAndFire2))
            return;
        _iceAndFireEvent.WaitOne();
        // await Task.Delay(100);
        var myIndex = accessory.GetMyIndex();
        var tankBusterPosition = new Vector3[4];
        tankBusterPosition[0] = new Vector3(84.5f, 0, 88f);
        tankBusterPosition[1] = tankBusterPosition[0].FoldPointHorizon(_center.X);
        tankBusterPosition[2] = tankBusterPosition[0];
        tankBusterPosition[3] = tankBusterPosition[1].FoldPointVertical(_center.Z);

        if (_p6DragonsGlowAction[0] && _p6DragonsGlowAction[1])
        {
            // Center stack tank buster, no guidance if not tank
            if (myIndex > 1) return;
            // Delete the small markers from K's script for the two tanks
            accessory.Method.RemoveDraw("P6 Second IceFire Line ND Position.*");
            var dp = accessory.DrawDirPos(_center, 0, 6000, $"IceFireCenterStackGuidance");
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        else
        {
            // Edge tank buster, don't draw the circle for the player's own buster to avoid blinding
            var busterIdx = _p6DragonsGlowAction.FindIndex(x => x == false);
            
            var str = "";
            str += $"Nidhogg Glow:{_p6DragonsGlowAction[0]}, Hraesvelgr Glow:{_p6DragonsGlowAction[1]}\n";
            str += $"It's the {(busterIdx == 0 ? "Nidhogg's" : "Hraesvelgr's")} tank buster.";
            accessory.Log.Debug($"{str}");

            var isMyBuster = myIndex == busterIdx;
            var dp = accessory.DrawCircle(accessory.Data.PartyList[busterIdx], isMyBuster ? 2f : 15f, 0, 6000, $"IceFireTankBuster");
            dp.Color = isMyBuster ? ColorHelper.ColorRed.V4 : ColorHelper.ColorYellow.V4;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            // Edge spread, no guidance if not tank
            if (myIndex > 1) return;
            // Delete the small markers from K's script for the two tanks
            accessory.Method.RemoveDraw("P6 Second IceFire Line ND Position.*");
            var isIceAndFire2 = _dsrPhase == DsrPhase.Phase6IceAndFire2;

            var dp0 = accessory.DrawDirPos(tankBusterPosition[isIceAndFire2 ? myIndex + 2 : myIndex], 0, 6000,
                $"IceFireTankBusterPositionGuidance");
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);

            var dp1 = accessory.DrawStaticCircle(tankBusterPosition[isIceAndFire2 ? myIndex + 2 : myIndex],
                PosColorPlayer.V4.WithW(1.5f), 0, 6000, $"IceFireTankBusterPointArea", 1f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
        }
        _iceAndFireEvent.Reset();
    }

    #endregion P6 Ice & Fire

    #region P6 Near/Far

    [ScriptMethod(name: "P6: Near/Far, Phase Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:27970"], userControl: false)]
    public void P6_NearOrFar_PhaseRecord(Event @event, ScriptAccessory accessory)
    {
        // Use the ActionEffect of Endless Cycle as phase node because Nidhogg flies first, then Hraesvelgr casts
        if (_dsrPhase is DsrPhase.Phase6NearOrFar1 or DsrPhase.Phase6NearOrFar2)
            return;
        _dsrPhase = _dsrPhase switch
        {
            DsrPhase.Phase6IceAndFire1 => DsrPhase.Phase6NearOrFar1,
            DsrPhase.Phase6Flame => DsrPhase.Phase6NearOrFar2,
            _ => DsrPhase.Phase6NearOrFar1,
        };
        _p6DragonsWingAction = [false, false, false];   // P6 Dragon's Wing record
        accessory.Log.Debug($"Current phase: {_dsrPhase}");
    }
    
    [ScriptMethod(name: "P6: Near/Far, Wings Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(279(39|4[023]))$"], userControl: false)]
    public void P6_NearOrFar_WingsRecord(Event @event, ScriptAccessory accessory)
    {
        // LEFT left wing glows, safe on the player's left side.
        const uint leftFar = 27940;
        const uint leftNear = 27939;
        const uint rightFar = 27943;
        // const uint rightNear = 27942;
        
        if (_dsrPhase is not (DsrPhase.Phase6NearOrFar1 or DsrPhase.Phase6NearOrFar2))
            return;
        
        var aid = @event.ActionId();
        // [Far T/Near F, Left safe T/Right safe F, Front safe T/Back safe F / Inner safe T/Outer safe F]
        _p6DragonsWingAction[0] = aid is leftFar or rightFar;
        _p6DragonsWingAction[1] = aid is leftFar or leftNear;
        accessory.Log.Debug($"Detected {(_p6DragonsWingAction[0] ? "Tanks Away" : "Tanks Close")}, {(_p6DragonsWingAction[1] ? "Left" : "Right")} safe");
        _nearOrFarWingsEvent.Set();
    }

    
    [ScriptMethod(name: "P6: Near/Far, Cauterize Record", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7747", "SourceDataId:12612"], userControl: false)]
    public void P6_NearOrFar_CauterizeRecord(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase6NearOrFar1) return;
        var spos = @event.SourcePosition();
        // [Far T/Near F, Left safe T/Right safe F, Front safe T/Back safe F / Inner safe T/Outer safe F]
        _p6DragonsWingAction[2] = spos.X < _center.X;
        accessory.Log.Debug($"Detected {(_p6DragonsWingAction[2] ? "Front" : "Back")} safe");
        _nearOrFarCauterizeEvent.Set();
    }

    [ScriptMethod(name: "P6: Near/Far, Inner/Outer Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2794[79])$"], userControl: false)]
    public void P6_NearOrFar_BlackWingsRecord(Event @event, ScriptAccessory accessory)
    {
        const uint insideSafe = 27947;
        // const uint outsideSafe = 27949;
        if (_dsrPhase != DsrPhase.Phase6NearOrFar2) return;
        var aid = @event.ActionId();
        // [Far T/Near F, Left safe T/Right safe F, Front safe T/Back safe F / Inner safe T/Outer safe F]
        _p6DragonsWingAction[2] = aid == insideSafe;
        accessory.Log.Debug($"Detected {(_p6DragonsWingAction[2] ? "Inside" : "Outside")} safe");
        _nearOrFarInOutEvent.Set();
    }

    [ScriptMethod(name: "First Near/Far Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(279(39|4[023]))$"])]
    public void P6_NearOrFar1_Dir(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase6NearOrFar1) return;
        _nearOrFarCauterizeEvent.WaitOne();
        _nearOrFarWingsEvent.WaitOne();
        Vector3[] nearOrFarSafePos = GetQuarterSafePos(_p6DragonsWingAction);
        var nearOrFarDirPosIdx = GetQuarterSafePosIdx(_p6DragonsWingAction);
        accessory.Log.Debug($"MT go {nearOrFarDirPosIdx[0]}, ST go {nearOrFarDirPosIdx[1]}, Group go {nearOrFarDirPosIdx[2]}");

        var myIndex = accessory.GetMyIndex();
        var myPartIdx = myIndex >= 2 ? 2 : myIndex;
        var targetPos = nearOrFarSafePos[nearOrFarDirPosIdx[myPartIdx]];

        for (var i = 0; i < 3; i++)
        {
            var tempPos = nearOrFarSafePos[nearOrFarDirPosIdx[i]];
            var color = i == myPartIdx ? PosColorPlayer.V4.WithW(1.5f) : PosColorNormal.V4;
            var dp0 = accessory.DrawStaticCircle(tempPos, color, 0, 7500, $"FirstNearFarPos{i}", 1f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp0);
        }

        var dp = accessory.DrawDirPos(targetPos, 0, 7500, $"FirstNearFarGuidance");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        _nearOrFarCauterizeEvent.Reset();
        _nearOrFarWingsEvent.Reset();
    }

    private Vector3[] GetQuarterSafePos(List<bool> wings)
    {
        // Four endpoints within the first quadrant
        // The order of points within the quadrant is, based on the first quadrant reference (facing Hraesvelgr, top-left), starting from top-left clockwise
        // Translate vertically, fold horizontally
        Vector3[] quarterSafePos = new Vector3[4];
        quarterSafePos[0] = new Vector3(120f, 0, 80f);
        quarterSafePos[1] = new Vector3(120f, 0, 98f);
        quarterSafePos[2] = new Vector3(102f, 0, 98f);
        quarterSafePos[3] = new Vector3(102f, 0, 80f);
        for (var i = 0; i < 4; i++)
        {
            // Back safe, translate backward
            if (!wings[2])
                quarterSafePos[i] -= new Vector3(22f, 0, 0);
            // Right safe, fold horizontally
            if (!wings[1])
                quarterSafePos[i] = quarterSafePos[i].FoldPointVertical(_center.Z);
        }
        return quarterSafePos;
    }

    private static int[] GetQuarterSafePosIdx(List<bool> wings)
    {
        // Return array, representing the safe position Index for MT, ST, and Group

        // Far, both tanks away, group close
        // Near, both tanks close, group away
        return wings[0] ? [2, 3, 1] : [1, 0, 3];
    }

    [ScriptMethod(name: "Second Near/Far Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2794[79])$"])]
    public void P6_NearOrFar2_Dir(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase6NearOrFar2) return;
        _nearOrFarInOutEvent.WaitOne();
        _nearOrFarWingsEvent.WaitOne();

        Vector3[] nearOrFarSafePos = GetLineSafePos(_p6DragonsWingAction);
        int[] nearOrFarDirPosIdx = GetLineSafePosIdx(_p6DragonsWingAction);

        var myIndex = accessory.GetMyIndex();
        var myPartIdx = myIndex >= 2 ? 2 : myIndex;
        var targetPos = nearOrFarSafePos[nearOrFarDirPosIdx[myPartIdx]];

        for (var i = 0; i < 3; i++)
        {
            var color = i == myPartIdx ? PosColorPlayer.V4.WithW(1.5f) : PosColorNormal.V4;
            var tempPos = nearOrFarSafePos[nearOrFarDirPosIdx[i]];
            var dp0 = accessory.DrawStaticCircle(tempPos, color, 0, 7500, $"SecondNearFarPos{i}", 1f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp0);
        }

        var dp = accessory.DrawDirPos(targetPos, 0, 7500, $"SecondNearFarGuidance");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        _nearOrFarInOutEvent.Reset();
        _nearOrFarWingsEvent.Reset();
    }

    private static Vector3[] GetLineSafePos(List<bool> wings)
    {
        // Three points on a line: near, middle, far
        Vector3[] lineSafePos = new Vector3[3];
        lineSafePos[0] = new Vector3(120f, 0, 100f);
        lineSafePos[1] = new Vector3(100f, 0, 100f);
        lineSafePos[2] = new Vector3(80f, 0, 100f);

        Vector3 dv3 = new(0f, 0f, 0f);

        // Left safe subtract, right safe add
        dv3 += new Vector3(0f, 0f, 2f) * (wings[1] ? -1 : 1);
        // Inside safe unchanged, outside safe multiply
        dv3 *= wings[2] ? 1 : 5;

        for (var i = 0; i < 3; i++)
            lineSafePos[i] += dv3;
        
        return lineSafePos;
    }

    private static int[] GetLineSafePosIdx(List<bool> wings)
    {
        // Return array, representing the safe position Index for MT, ST, and Group

        // Far, both tanks away, group close
        // Near, both tanks close, group away
        return wings[0] ? [1, 2, 0] : [1, 0, 2];
    }

    #endregion P6 Near/Far
    
    #region P6 Cross Fire

    [ScriptMethod(name: "P6: Cross Fire, Phase Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27973"], userControl: false)]
    public void P6_Flame_PhaseRecord(Event @event, ScriptAccessory accessory)
    {
        _dsrPhase = DsrPhase.Phase6Flame;
        accessory.Log.Debug($"Current phase: {_dsrPhase}");
    }

    [ScriptMethod(name: "Cross Fire Stack Target", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27974"])]
    public void P6_FlameStackTarget(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase6Flame) return;
        var tid = @event.TargetId();
        var dp = accessory.DrawCircle(tid, 6, 0, 12500, $"DeathCycleTarget");
        dp.Color = accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    #endregion P6 Cross Fire

    #region P6 Divebomb

    [ScriptMethod(name: "Divebomb Both Tanks Guidance", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7737", "SourceDataId:12613"])]
    public void P6_CauterizeDir(Event @event, ScriptAccessory accessory)
    {
        if (_dsrPhase != DsrPhase.Phase6IceAndFire2) return;
        _dsrPhase = DsrPhase.Phase6Cauterize;
        accessory.Log.Debug($"Current phase: {_dsrPhase}");

        Vector3[] cauterizePos = new Vector3[2];
        cauterizePos[0] = new Vector3(95f, 0, 79f);
        cauterizePos[1] = new Vector3(105f, 0, 79f);

        var myIndex = accessory.GetMyIndex();
        if (myIndex > 1) return;

        var dp = accessory.DrawDirPos(cauterizePos[myIndex], 0, 5000, $"DivebombTankInterceptPos{myIndex}");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    #endregion P6 Divebomb

    #region P7 Exaflare

    [ScriptMethod(name: "---- ã€ŠP7: Dragon-King Thordanã€‹ ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_PhaseDragonKingThordan(Event @event, ScriptAccessory accessory)
    {
    }
    
    [ScriptMethod(name: "P7: BossId Record & Exaflare Class Init", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["SourceDataId:12616"], userControl: false)]
    public void P7_BossIdRecord(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        _p7BossId = sid;
        List<int> scoreList = ExaflareStrategy switch
        {
            // moveStep,isFront,isUniverse
            ExaflareSpecStrategyEnum.NeverFront => [2, 100, 50],
            ExaflareSpecStrategyEnum.NeverUniverse => [2, 10, 100],
            ExaflareSpecStrategyEnum.LeastMovement => [20, 10, 50],
            ExaflareSpecStrategyEnum.AlwaysFront => [2, -10, 50],
            _ => [-10, 100, 0],
        };
        _p7Exaflare = new DsrExaflare(scoreList);
    }
    

    [ScriptMethod(name: "P7: Chariot/Donut Sword Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2056", "Param:regex:^(29[89])$"], userControl: false)]
    public void P7_BossBladeRecord(Event @event, ScriptAccessory accessory)
    {
        var param = @event.Param();
        accessory.Log.Debug($"Chariot/Donut Sword: {param} (298 Chariot, 299 Donut)");
        _p7Exaflare?.SetBladeType(param);
        if (!IsExaflarePhase()) return;
        _bladeEvent.Set();
    }
    
    [ScriptMethod(name: "Exaflare Range Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28060"])]
    public void P7_ExaflareDrawn(Event @event, ScriptAccessory accessory)
    {
        // Spreads forward, left, right
        var spos = @event.SourcePosition();
        var srot = @event.SourceRotation();
        var bossChara = accessory.GetById(_p7BossId);
        var bossRot = bossChara?.Rotation ?? float.Pi;
        var bossPos = bossChara?.Position ?? _center;
        const int intervalTime = 1900;
        const int castTime = 6900;
        const int extendDistance = 7;
        const int dirNum = 3;
        const int extNum = 6;
        const int advWarnNum = 1;   // How many steps to extend the warning
        float[] flareRot = [0, -float.Pi / 2, float.Pi / 2];
        
        Vector3[,] exaflarePos = BuildExaflareVector(spos, dirNum, extNum, srot, flareRot, extendDistance);
        DrawExaflareScene(exaflarePos, ExaflareWarnDrawn, advWarnNum, castTime, intervalTime, accessory);
        
        if (_p7Exaflare == null) return;
        lock (_p7Exaflare)
        {
            _p7Exaflare.SetBossPos(bossPos, accessory);
            _p7Exaflare.AddExaflare(spos, bossRot, srot, accessory);
        }
    }
    
    [ScriptMethod(name: "Exaflare Special Solution Guidance", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2056", "Param:regex:^(29[89])$"])]
    public void P7_ExaflareGuidance(Event @event, ScriptAccessory accessory)
    {
        // Can calculate after recording Chariot/Donut
        if (_p7Exaflare == null) return;
        if (!IsExaflarePhase()) return;
        if (ExaflareStrategy == ExaflareSpecStrategyEnum.PleaseDontDoThat) return;
        if (!_p7Exaflare.ExaflareRecordComplete()) return;
        _bladeEvent.WaitOne();
        var guidePosList = _p7Exaflare.ExportExaflareSolution(accessory);
        accessory.Log.Debug($"The strategy you chose is {ExaflareStrategy}");
        DrawExaflareGuidePos(guidePosList, accessory);
        _bladeEvent.Reset();
    }
    
    private void DrawExaflareGuidePos(List<Vector3> guidePosList, ScriptAccessory accessory)
    {
        const int intervalTime = 1900;
        const int castTime = 6900;
        const int baseTime = castTime - 900;    // 900ms is the time for the Ice/Fire sword to attach to Thordan

        for (var i = 0; i < guidePosList.Count; i++)
        {
            var delay = i == 0 ? 0 : baseTime + (i - 1) * intervalTime;
            var destroy = i == 0 ? baseTime : intervalTime;

            var dp01 = accessory.DrawDirPos(guidePosList[i], delay, destroy, $"ExaflareStep{i}-Player-Position");
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp01);
            if (i >= guidePosList.Count - 1) continue;
            var dp12 = accessory.DrawDirPos2Pos(guidePosList[i], guidePosList[i + 1], delay, destroy, $"ExaflareStep{i}-Position-Position");
            dp12.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp12);
        }
    }
    
    /// <summary>
    /// Draw the Exaflare scene
    /// </summary>
    /// <param name="exaflarePos">Exaflare matrix</param>
    /// <param name="warnDrawn">Whether to draw warning Exaflare</param>
    /// <param name="advWarnNum">How many steps to draw warning Exaflare</param>
    /// <param name="castTime">Initial Exaflare ability cast time</param>
    /// <param name="intervalTime">Exaflare interval time</param>
    /// <param name="accessory"></param>
    private void DrawExaflareScene(Vector3[,] exaflarePos, bool warnDrawn, int advWarnNum, int castTime, int intervalTime, ScriptAccessory accessory)
    {
        var dirNum = exaflarePos.GetLength(0);
        var extNum = exaflarePos.GetLength(1);
        
        for (var ext = 0; ext < extNum; ext++)
        {
            // Calculate the appearance time and delay for each position. The first Exaflare often needs special handling, subsequent ones use the same interval.
            var destroy = ext == 0 ? castTime : intervalTime;
            var delay= ext == 0 ? 0 : castTime + (ext - 1) * intervalTime;
            
            if (ext == 0)
            {
                // Primary Exaflare, for the initial one (ext=0), only draw dir=0, no extension at any angle
                DrawExaflare(exaflarePos[0, ext], delay, destroy, accessory);
                DrawExaflareEdge(exaflarePos[0, ext], delay, destroy, accessory);
            }
            else
            {
                // For subsequent Exaflares (ext>0), extend at the corresponding angles
                for (var dir = 0; dir < dirNum; dir++)
                {
                    DrawExaflare(exaflarePos[dir, ext], delay, destroy, accessory);
                    DrawExaflareEdge(exaflarePos[dir, ext], delay, destroy, accessory);
                }
            }
            
            if (!warnDrawn) continue;
            for (var adv = 1; adv <= advWarnNum; adv++)
            {
                if (ext >= extNum - adv) continue;
                for (var dir = 0; dir < dirNum; dir++)
                    DrawExaflareWarn(exaflarePos[dir, ext + adv], adv, delay, destroy, intervalTime, accessory);
            }
        }
    }
    
    /// <summary>
    /// Build the Exaflare coordinate matrix
    /// </summary>
    /// <param name="sourcePos">Primary Exaflare position</param>
    /// <param name="dirNum">How many directions a single Exaflare involves</param>
    /// <param name="extNum">How many times a single Exaflare extends</param>
    /// <param name="sourceRot">Exaflare illusion rotation angle</param>
    /// <param name="flareRot">Rotation angles for each direction</param>
    /// <param name="extDistance">Exaflare step extension distance</param>
    private Vector3[,] BuildExaflareVector(Vector3 sourcePos, int dirNum, int extNum, float sourceRot, float[] flareRot, float extDistance)
    {
        Vector3[,] exaflarePos = new Vector3[dirNum, extNum];
        if (flareRot.Length != dirNum) return exaflarePos;
        for (var ext = 0; ext < extNum; ext++)
            for (var dir = 0; dir < dirNum; dir++)
                exaflarePos[dir, ext] = sourcePos.ExtendPoint(sourceRot.Game2Logic() + flareRot[dir], ext * extDistance);
        return exaflarePos;
    }
    
    private void DrawExaflare(Vector3 spos, int delay, int destroy, ScriptAccessory accessory)
    {
        const int scale = 6;
        var color = ExaflareBuiltInColor ? ColorHelper.ColorExaflare.V4 : ExaflareColor.V4.WithW(1f);
        var dp = accessory.DrawStaticCircle(spos, color, delay, destroy, $"Exaflare{spos}", scale);
        dp.ScaleMode |= ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    private void DrawExaflareEdge(Vector3 spos, int delay, int destroy, ScriptAccessory accessory)
    {
        const float scale = 6;
        // const float innerScale = scale - 0.05f;
        var color = ExaflareBuiltInColor ? ColorHelper.ColorExaflare.V4 : ExaflareColor.V4.WithW(1.5f);
        var dp = accessory.DrawStaticDonut(spos, color, delay, destroy, $"ExaflareEdge{spos}", scale);
        // dp.Color = ColorHelper.colorDark.V4;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp);
    }

    private void DrawExaflareWarn(Vector3 spos, int adv, int delay, int destroy, int interval, ScriptAccessory accessory)
    {
        const int scale = 6;
        var destroyItv = interval * (adv - 1);
        var color = ExaflareBuiltInColor ? ColorHelper.ColorExaflareWarn.V4.WithW(1f / adv) : ExaflareWarnColor.V4.WithW(1f / adv);
        var dp = accessory.DrawStaticCircle(spos, color, delay, destroy + destroyItv, $"ExaflareWarn{spos}", scale);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    private void DebugExaflare(float[] srot, float bossRotRad, uint bladeType, ScriptAccessory accessory)
    {
        accessory.Log.Debug($"The strategy you chose is {ExaflareStrategy}");
        
        List<int> scoreList = ExaflareStrategy switch
        {
            // moveStep,isFront,isUniverse
            ExaflareSpecStrategyEnum.NeverFront => [2, 100, 50],
            ExaflareSpecStrategyEnum.NeverUniverse => [2, 10, 100],
            ExaflareSpecStrategyEnum.LeastMovement => [20, 10, 50],
            ExaflareSpecStrategyEnum.AlwaysFront => [2, -10, 50],
            _ => [-10, 100, 0],
        };
        _p7Exaflare = new DsrExaflare(scoreList);
        
        // Spreads forward, left, right
        // var spos = @event.SourcePosition();
        // var srot = @event.SourceRotation();
        Vector3[] spos =
        [
            _center.ExtendPoint(bossRotRad.Game2Logic() - float.Pi, 8),
            _center.ExtendPoint(bossRotRad.Game2Logic() + 60f.DegToRad(), 8),
            _center.ExtendPoint(bossRotRad.Game2Logic() - 60f.DegToRad(), 8)
        ];
        var bossChara = accessory.GetById(_p7BossId);
        var bossRot = bossChara?.Rotation ?? bossRotRad;
        var bossPos = bossChara?.Position ?? _center;
        const int intervalTime = 1900;
        const int castTime = 6900;
        const int extendDistance = 7;
        const int dirNum = 3;
        const int extNum = 6;
        const int advWarnNum = 1;   // How many steps to extend the warning
        float[] flareRot = [0, -float.Pi / 2, float.Pi / 2];

        for (int i = 0; i < 3; i++)
        {
            Vector3[,] exaflarePos = BuildExaflareVector(spos[i], dirNum, extNum, srot[i], flareRot, extendDistance);
            // Draw Exaflare direction arrows
            var dp1 = accessory.DrawDirPos2Pos(spos[i], spos[i].ExtendPoint(srot[i].Game2Logic() + flareRot[0], 6), 0, castTime, $"Arrow1", 5.9f);
            var dp2 = accessory.DrawDirPos2Pos(spos[i], spos[i].ExtendPoint(srot[i].Game2Logic() + flareRot[1], 6), 0, castTime, $"Arrow2", 5.9f);
            var dp3 = accessory.DrawDirPos2Pos(spos[i], spos[i].ExtendPoint(srot[i].Game2Logic() + flareRot[2], 6), 0, castTime, $"Arrow3", 5.9f);
            dp1.Color = ColorHelper.ColorRed.V4;
            dp2.Color = ColorHelper.ColorRed.V4;
            dp3.Color = ColorHelper.ColorRed.V4;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);
            
            DrawExaflareScene(exaflarePos, ExaflareWarnDrawn, advWarnNum, castTime, intervalTime, accessory);
            if (_p7Exaflare == null) return;
            lock (_p7Exaflare)
            {
                _p7Exaflare.SetBossPos(bossPos, accessory);
                _p7Exaflare.AddExaflare(spos[i], bossRot, srot[i], accessory);
            }
        }
        _p7Exaflare.SetBladeType(bladeType);
        switch (bladeType)
        {
            case ChariotBlade:
                var dp1 = accessory.DrawStaticCircle(_center, accessory.Data.DefaultDangerColor.WithW(2f), 0, castTime, $"Chariot", 8f);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
                break;
            case ChariotBlade + 1:
                var dp2 = accessory.DrawStaticDonut(_center, accessory.Data.DefaultDangerColor.WithW(2f), 0, castTime, $"Donut", 50f, 8f);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
                break;
        }
        
        // Can calculate after recording Chariot/Donut
        if (_p7Exaflare == null) return;
        // if (!IsExaflarePhase()) return;
        if (ExaflareStrategy == ExaflareSpecStrategyEnum.PleaseDontDoThat) return;
        if (!_p7Exaflare.ExaflareRecordComplete()) return;
        var guidePosList = _p7Exaflare.ExportExaflareSolution(accessory);
        DrawExaflareGuidePos(guidePosList, accessory);
    }

    [ScriptMethod(name: "Reminiscence Court Exaflare Simulator", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:=Exaflare"], userControl: false)]
    public void ExaflareEchoDebug(Event @event, ScriptAccessory accessory)
    {
        // ---- DEBUG CODE ----
        
        _center = new Vector3(400, -54.97f, -400);
        Random random = new Random();
        float bossRotLogicDeg = random.Next(0, 360);
        var bossRotLogicRad = bossRotLogicDeg.DegToRad();
        accessory.Log.Debug($"Randomized Boss facing {bossRotLogicRad.RadToDeg()}");
        float[] srot =
        [
            (random.Next(0, 8) * float.Pi / 4 + bossRotLogicRad).Logic2Game(),
            (random.Next(0, 8) * float.Pi / 4 + bossRotLogicRad).Logic2Game(),
            (random.Next(0, 8) * float.Pi / 4 + bossRotLogicRad).Logic2Game()
        ];
        Vector3 bossFace = _center.ExtendPoint(bossRotLogicRad, 8f);
        var dp = accessory.DrawDirPos2Pos(_center, bossFace, 0, 7000, $"Facing", 7.9f);
        dp.Color = ColorHelper.ColorDark.V4;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        DebugExaflare(srot, bossRotLogicRad.Logic2Game(), (uint)random.Next(0, 2) + ChariotBlade, accessory);
        // -- DEBUG CODE END --
    }
    
    #endregion P7 Exaflare
    
    #region P7 Trinity

    [ScriptMethod(name: "P7: Phase Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2805[179]|28206)$"], userControl: false)]
    public void P7_PhaseRecord(Event @event, ScriptAccessory accessory)
    {
        _dsrPhase = _dsrPhase switch
        {
            DsrPhase.Phase6Cauterize => DsrPhase.Phase7Exaflare1,
            DsrPhase.Phase7Exaflare1 => DsrPhase.Phase7Stack1,
            DsrPhase.Phase7Stack1 => DsrPhase.Phase7Nuclear1,
            DsrPhase.Phase7Nuclear1 => DsrPhase.Phase7Exaflare2,
            DsrPhase.Phase7Exaflare2 => DsrPhase.Phase7Stack2,
            DsrPhase.Phase7Stack2 => DsrPhase.Phase7Nuclear2,
            DsrPhase.Phase7Nuclear2 => DsrPhase.Phase7Exaflare3,
            DsrPhase.Phase7Exaflare3 => DsrPhase.Phase7Stack3,
            DsrPhase.Phase7Stack3 => DsrPhase.Phase7Enrage,
            _ => DsrPhase.Phase7Exaflare1,
        };
        accessory.Log.Debug($"Current phase: {_dsrPhase}");

        if (!_p7FirstEnmityOrder.Contains(true))
        {
            // Initialize
            _p7FirstEnmityOrder = [true, false];
            _p7TrinityDisordered = false;
            _p7TrinityTankDisordered = false;
            _p7TrinityNum = 0;
        }
        else
        {
            _p7FirstEnmityOrder[0] = !_p7FirstEnmityOrder[0];
            _p7FirstEnmityOrder[1] = !_p7FirstEnmityOrder[1];
            accessory.Log.Debug($"MT is {(_p7FirstEnmityOrder[0] ? "Top Enmity" : "Second Enmity")}, ST is {(_p7FirstEnmityOrder[1] ? "Top Enmity" : "Second Enmity")}");
        }
        _trinityEvent.Set();
        
        if (!IsStackPhase()) return;
        List<int> scoreList = ExaflareStrategy switch
        {
            // moveStep,isFront,isUniverse
            ExaflareSpecStrategyEnum.NeverFront => [2, 100, 50],
            ExaflareSpecStrategyEnum.NeverUniverse => [2, 10, 100],
            ExaflareSpecStrategyEnum.LeastMovement => [20, 10, 50],
            ExaflareSpecStrategyEnum.AlwaysFront => [2, -10, 50],
            _ => [-10, 100, 0],
        };
        _p7Exaflare = new DsrExaflare(scoreList);
        
    }
    
    private bool IsExaflarePhase()
    {
        return _dsrPhase is DsrPhase.Phase7Exaflare1 or DsrPhase.Phase7Exaflare2 or DsrPhase.Phase7Exaflare3;
    }
    
    private bool IsStackPhase()
    {
        return _dsrPhase is DsrPhase.Phase7Stack1 or DsrPhase.Phase7Stack2 or DsrPhase.Phase7Stack3;
    }

    [ScriptMethod(name: "Trinity Attack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2805[179])$"])]
    public void P7_TrinityAttack(Event @event, ScriptAccessory accessory)
    {
        _trinityEvent.WaitOne();
        var aid = @event.ActionId();
        var sid = @event.SourceId();
        const uint exaflare = 28059;
        const uint stack = 28051;
        const uint nuclear = 28057;

        var delay = aid switch
        {
            exaflare => 15200,
            stack => 18500,
            nuclear => 27200,
            _ => 0
        };
        
        delay = _dsrPhase switch
        {
            DsrPhase.Phase7Stack1 => delay,
            DsrPhase.Phase7Stack2 => delay + 1100,
            DsrPhase.Phase7Stack3 => delay + 2200,
            _ => delay
        };

        DrawTrinityAggro(sid, delay - 4000, 4000, 1, accessory);
        DrawTrinityAggro(sid, delay - 4000, 4000, 2, accessory);
        DrawTrinityAggro(sid, delay, 4000, 1, accessory);
        DrawTrinityAggro(sid, delay, 4000, 2, accessory);
        DrawTrinityNear(sid, delay - 4000, 4000, accessory);
        DrawTrinityNear(sid, delay, 4000, accessory);
        _trinityEvent.Reset();
    }

    private void DrawTrinityAggro(uint sid, int delay, int destroy, uint aggroIdx, ScriptAccessory accessory)
    {
        var myIndex = accessory.GetMyIndex();
        Vector4 color;

        if (myIndex > 1 || _p7TrinityTankDisordered)
            color = accessory.Data.DefaultDangerColor;
        else
        {
            switch (_p7FirstEnmityOrder[myIndex])
            {
                case true when aggroIdx == 1:
                case false when aggroIdx == 2:
                    color = accessory.Data.DefaultSafeColor;
                    break;
                default:
                    color = accessory.Data.DefaultDangerColor;
                    break;
            }
        }
        
        var dp = accessory.DrawOwnersEnmityOrder(sid, aggroIdx, 3f, 3f, delay, destroy, $"TrinityAggro{aggroIdx}", byTime: true);
        dp.Color = color.WithW(2f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    private void DrawTrinityNear(uint sid, int delay, int destroy, ScriptAccessory accessory)
    {
        var myIndex = accessory.GetMyIndex();

        var dp = accessory.DrawTargetNearFarOrder(sid, 1, true, 3f, 3f, delay, destroy, $"TrinityNear", byTime: true);
        if (_p7TrinityDisordered)
            dp.Color = accessory.Data.DefaultDangerColor;
        else
            dp.Color = myIndex == _p7TrinityOrderIdx[_p7TrinityNum] ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Trinity Order Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:28065"], userControl: false)]
    public void P7_TrinityOrderRecord(Event @event, ScriptAccessory accessory)
    {
        // If main perspective is tank, ignore their own Trinity order
        var myIndex = accessory.GetMyIndex();
        if (myIndex < 2) return;

        var targetIdx = @event.TargetIndex();
        if (targetIdx != 1)
        {
            if (_p7TrinityDisordered) return;
            accessory.Log.Debug($"Someone took an extra sword, invalidating");
            accessory.Method.TextInfo($"Someone took an extra sword, safe colors will no longer be displayed", 3000, true);
            _p7TrinityDisordered = true;
            return;
        }

        var tid = @event.TargetId();
        var tidx = accessory.GetPlayerIdIndex(tid);
        if (_p7TrinityOrderIdx[_p7TrinityNum] != tidx && !_p7TrinityDisordered)
        {
            accessory.Log.Debug($"Wrong player took the sword, invalidating");
            accessory.Method.TextInfo($"Wrong player took the sword, safe colors will no longer be displayed", 3000, true);
            _p7TrinityDisordered = true;
        }

        _p7TrinityNum++;
        if (_p7TrinityNum >= 6)
            _p7TrinityNum = 0;

        var targetRecent = accessory.GetPlayerJobByIndex(tidx);
        var targetNext = accessory.GetPlayerJobByIndex(_p7TrinityOrderIdx[_p7TrinityNum]);
        accessory.Log.Debug($"The player who just took the sword is {targetRecent}, the next player to take the sword is {targetNext}");
    }

    [ScriptMethod(name: "Trinity Tank Sword Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(2806[34])$"], userControl: false)]
    public void P7_TrinityTankRecord(Event @event, ScriptAccessory accessory)
    {
        var aid = @event.ActionId();
        var tid = @event.TargetId();
        
        // Non-tank player took the sword
        var tidx = accessory.GetPlayerIdIndex(tid);
        if (tidx > 1) return;

        // Main perspective is not a tank
        var myIndex = accessory.GetMyIndex();
        if (myIndex > 1) return;

        // Already invalid
        if (_p7TrinityTankDisordered) return;

        const uint aggro1 = 28063;
        const uint aggro2 = 28064;

        // Aggro 1 effect but target is aggro 2 || Aggro 2 effect but target is aggro 1
        if ((_p7FirstEnmityOrder[tidx] || aid != aggro1) && (!_p7FirstEnmityOrder[tidx] || aid != aggro2)) return;
        accessory.Log.Debug($"Sword enmity error, invalidating");
        accessory.Method.TextInfo($"Sword enmity error, safe colors will no longer be displayed", 3000, true);
        _p7TrinityTankDisordered = true;
    }

    #endregion P7 Trinity
    public class PriorityDict
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public ScriptAccessory sa {get; set;} = null!;
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public Dictionary<int, int> Priorities {get; set;} = null!;
        public string Annotation { get; set; } = "";
        public int ActionCount { get; set; } = 0;
        
        public void Init(ScriptAccessory accessory, string annotation, int partyNum = 8)
        {
            sa = accessory;
            Priorities = new Dictionary<int, int>();
            ActionCount = 0;
            for (var i = 0; i < partyNum; i++)
            {
                Priorities.Add(i, 0);
            }
            Annotation = annotation;
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
        public string ShowPriorities(bool showJob = true)
        {
            var str = $"{Annotation} ({ActionCount}-th) Priority Dictionary:\n";
            if (Priorities.Count == 0)
            {
                str += $"PriorityDict Empty.\n";
                return str;
            }
            foreach (var pair in Priorities)
            {
                str += $"Key {pair.Key} {(showJob ? $"({_role[pair.Key]})" : "")}, Value {pair.Value}\n";
            }

            return str;
        }

        public PriorityDict DeepCopy()
        {
            return JsonConvert.DeserializeObject<PriorityDict>(JsonConvert.SerializeObject(this)) ?? new PriorityDict();
        }

        public void AddActionCount(int count = 1)
        {
            ActionCount += count;
        }
    }
}

#region Exaflare Class

public class DsrExaflare(List<int> scoreList)
{
    // Top-right 0, Down 1, Left 2
    private List<Vector3> ExaflarePosList { get; set; } = Enumerable.Repeat(new Vector3(0, 0, 0), 3).ToList();
    private Vector3 BossPos { get; set; } = new Vector3(0, 0, 0);
    private List<int> ExaflareDirList { get; set; } = [0, 0, 0];
    private uint BladeType { get; set; } = 0;
    private List<ExaflareSolution> ExaflareSolutionList { get; set; } = [];
    public int RecordedExaflareNum = 0;

    private ExaflareSolution BuildOneStepSolutionNew(ScriptAccessory accessory)
    {
        // One-step Exaflare
        const bool isUniverse = false;
        var moveStep = 0;
        Vector3 pos2;
        Vector3 pos3;
        int targetExaflareIdx;
        var debugText = $"[a][One-step Exaflare]: \n";
        
        if (!IsFrontPointedByExaflare(0))
            targetExaflareIdx = 0;
        else if (!IsFrontPointedByExaflare(2))
            targetExaflareIdx = 2;
        else
        {
            targetExaflareIdx = 0;
            moveStep++;
        }

        pos2 = ExaflarePosList[targetExaflareIdx];
        
        if (moveStep == 0)
        {
            debugText += $"[a]Detected {GetExaflareIdxStr(targetExaflareIdx)} Exaflare not pointed, can be used as safe point\n";
            pos3 = pos2;
        }
        else
        {
            debugText += $"[a]Detected front Exaflares are both pointed, using front two-step Exaflare, arbitrarily take top-left as safe point\n";
            pos3 = ExaflarePosList[1].PointInOutside(BossPos, 12f);
        }
        
        // pos1 defines starting point based on role
        var myIndex = accessory.GetMyIndex();
        var pos1 = FindFirstSafePosAtFront(targetExaflareIdx, myIndex < 1);
        debugText += $"[a]Player index {myIndex}, {(myIndex < 1?"tank":"group")} perspective,\nposition {(myIndex < 1?"front":"back")}\n";
        moveStep++;
        
        accessory.Log.Debug(debugText);
        
        return new ExaflareSolution([pos1, pos2, pos3], moveStep, true, isUniverse, "One-step Exaflare", scoreList,
            accessory);
    }
    
    private ExaflareSolution BuildTwoStepSolution(ScriptAccessory accessory)
    {
        // Two-step Exaflare
        var backExaflarePos = ExaflarePosList[1];
        var isUniverse = false;
        var moveStep = 0;
        // pos1 during cast, find safe zone from back Exaflare's Chariot/Donut
        var pos1 = FindFirstSafePos(1, true);
        moveStep++;
        // pos2 after first explosion, find back Exaflare position
        var pos2 = backExaflarePos;
        // pos3 after second explosion, observe the front two
        Vector3 pos3;
        var debugText = $"[b][Two-step Exaflare]: \n";
        
        // Whether the two front Exaflares point to the back
        var idx0Point = IsBackPointedByExaflare(0);
        var idx2Point = IsBackPointedByExaflare(2);

        if (!idx0Point && !idx2Point)
        {
            // Neither points to the back, stay
            pos3 = backExaflarePos;
            debugText += $"[b]Detected front Exaflares do not point to the back, treat as back one-step Exaflare\n";
        }
        else if (!idx0Point && idx2Point)
        {
            // Top-right does not point to the back, go left
            pos3 = backExaflarePos.RotatePoint(BossPos, 45f.DegToRad());
            moveStep++;
            debugText += $"[b]Detected top-right Exaflare does not point to the back, go left-back\n";
        }
        else if (idx0Point && !idx2Point)
        {
            // Top-left does not point to the back, go right
            pos3 = backExaflarePos.RotatePoint(BossPos, -45f.DegToRad());
            moveStep++;
            debugText += $"[b]Detected top-left Exaflare does not point to the back, go right\n";
        }
        else
        {
            // All point to the back, use universe strategy
            pos3 = FindUniversalSafePos();
            isUniverse = true;
            moveStep++;
            debugText += $"[b]Detected all Exaflares point to the back, using Universe strategy\n";
        }
        accessory.Log.Debug(debugText);
        return new ExaflareSolution([pos1, pos2, pos3], moveStep, false, isUniverse, "Two-step Exaflare", scoreList,
            accessory);
    }

    /// <summary>
    /// Position clockwise or counter-clockwise from a given Exaflare
    /// </summary>
    /// <param name="exaflareIdx">A given Exaflare index</param>
    /// <param name="isCw">Find clockwise</param>
    /// <returns></returns>
    private Vector3 FindFirstSafePos(int exaflareIdx, bool isCw)
    {
        var exaflarePos = ExaflarePosList[exaflareIdx];
        var rad = exaflarePos.FindRadian(BossPos) + (isCw ? 50f.DegToRad() : -50f.DegToRad());
        var firstSafePos = BossPos.ExtendPoint(rad, IsChariot() ? 8.5f : 7.5f);
        return firstSafePos;
    }

    private Vector3 FindFirstSafePosAtFront(int exaflareIdx, bool isTank)
    {
        // var exaflarePos = ExaflarePosList[exaflareIdx];
        if (isTank) // Tank starts from the front
        {
            if (exaflareIdx == 0)
                return FindFirstSafePos(exaflareIdx, false);
            if (exaflareIdx == 2)
                return FindFirstSafePos(exaflareIdx, true);
        }
        else
        {
            if (exaflareIdx == 0)
                return FindFirstSafePos(exaflareIdx, true);
            if (exaflareIdx == 2)
                return FindFirstSafePos(exaflareIdx, false);
        }
        return new Vector3(0, 0, 0);
    }

    private Vector3 FindUniversalSafePos()
    {
        return ExaflarePosList[1].PointInOutside(BossPos, 13.2f - 8f, true);
    }
    
    public void SetBossPos(Vector3 bossPosV3, ScriptAccessory accessory)
    {
        BossPos = bossPosV3;
        // accessory.DebugMsg($"Set Boss position {BossPos}", debugMode);
    }
    
    /// <summary>
    /// Add Exaflare property
    /// </summary>
    /// <param name="exaflarePosV3">Exaflare position</param>
    /// <param name="bossRotation">Boss rotation angle</param>
    /// <param name="exaflareRot">Exaflare rotation angle</param>
    /// <param name="accessory"></param>
    public void AddExaflare(Vector3 exaflarePosV3, float bossRotation, float exaflareRot, ScriptAccessory accessory)
    {
        var idx = FindExaflareIdx(exaflarePosV3, bossRotation);
        // No need to convert difference
        var exaflareRelativeDir = exaflareRot.Game2Logic() - bossRotation.Game2Logic();
        var dir = exaflareRelativeDir.Rad2Dirs(8);
        ExaflareDirList[idx] = dir;
        ExaflarePosList[idx] = exaflarePosV3;
        accessory.Log.Debug($"Added {GetExaflareIdxStr(idx)} Exaflare, coordinate {exaflarePosV3}, facing {GetDirStr(dir)}");
        RecordedExaflareNum++;
    }
    
    /// <summary>
    /// Find the index of the corresponding Exaflare based on its center position
    /// Because the Exaflare position changes with the Boss's facing, subtract the Boss rotation offset
    /// </summary>
    /// <param name="exaflarePosV3">Exaflare center position</param>
    /// <param name="bossRotation">Boss facing</param>
    /// <returns></returns>
    private int FindExaflareIdx(Vector3 exaflarePosV3, float bossRotation)
    {
        var exaflareBaseDir = exaflarePosV3.FindRadian(BossPos);
        var exaflareRelativeDir = exaflareBaseDir - bossRotation.Game2Logic();
        var idx = exaflareRelativeDir.Rad2Dirs(3, false);
        return idx;
    }

    /// <summary>
    /// Return whether this Exaflare is a cardinal direction (even number in 8-direction system)
    /// </summary>
    /// <param name="idx">Exaflare index</param>
    /// <returns></returns>
    private bool IsExaflareRightDir(int idx)
    {
        return ExaflareDirList[idx] % 2 == 0;
    }

    /// <summary>
    /// Check if the back is pointed to by the Exaflare with index idx
    /// </summary>
    /// <param name="idx">Exaflare index</param>
    /// <returns></returns>
    private bool IsBackPointedByExaflare(int idx)
    {
        // Condition for top-right Exaflare pointing to back: top-right Exaflare is not cardinal and direction is not 1
        // Condition for top-left Exaflare pointing to back: top-left Exaflare is not cardinal and direction is not 7
        var result = idx switch
        {
            0 => !IsExaflareRightDir(idx) && ExaflareDirList[idx] != 1,
            2 => !IsExaflareRightDir(idx) && ExaflareDirList[idx] != 7,
            _ => false
        };
        return result;
    }

    /// <summary>
    /// Check if the front Exaflare with index idx is pointed to
    /// </summary>
    /// <param name="idx">Exaflare index</param>
    /// <returns></returns>
    private bool IsFrontPointedByExaflare(int idx)
    {
        // Top-right Exaflare is pointed to if: top-left Exaflare is cardinal and direction is not 6 (facing left) OR back Exaflare is diagonal and direction is not 5 (facing bottom-left)
        // Top-left Exaflare is pointed to if: top-right Exaflare is cardinal and direction is not 2 (facing right) OR back Exaflare is diagonal and direction is not 3 (facing bottom-right)
        var result = idx switch
        {
            0 => (IsExaflareRightDir(2) && ExaflareDirList[2] != 6) ||
                 (!IsExaflareRightDir(1) && ExaflareDirList[1] != 5),
            2 => (IsExaflareRightDir(0) && ExaflareDirList[0] != 2) ||
                 (!IsExaflareRightDir(1) && ExaflareDirList[1] != 3),
            _ => false
        };
        return result;
    }
    
    public void SetBladeType(uint type)
    {
        BladeType = type;
    }

    private bool IsChariot()
    {
        const uint chariotFireBlade = 298;
        return BladeType == chariotFireBlade;
    }

    private void AddExaflareSolution(ExaflareSolution solution)
    {
        ExaflareSolutionList.Add(solution);
    }

    public List<Vector3> ExportExaflareSolution(ScriptAccessory accessory)
    {
        AddExaflareSolution(BuildOneStepSolutionNew(accessory));
        AddExaflareSolution(BuildTwoStepSolution(accessory));
        
        ExaflareSolutionList = ExaflareSolutionList.OrderBy(solution => solution.Score).ToList();
        accessory.Log.Debug($"Comparison of two solutions, higher priority is {ExaflareSolutionList[0].Description}, Score {ExaflareSolutionList[0].Score}");
        return ExaflareSolutionList[0].ExaflareSolutionPosList;
    }
    
    /*
     * Methods below are for building Exaflare paths, can be made into a separate class later.
     */
    
    // /// <summary>
    // /// Build Exaflare coordinates
    // /// </summary>
    // /// <param name="center">Center</param>
    // /// <param name="rotation">Rotation angle</param>
    // /// <param name="extendDistance">Extension distance</param>
    // /// <returns></returns>
    // private Vector3 GetExaflarePos(Vector3 center, float rotation, float extendDistance)
    // {
    //     return center.ExtendPoint(rotation, extendDistance);
    // }

    // private Vector3[] BuildExaflareVector(Vector3 center, float rotation, int extendNum, float extendDistance)
    // {
    //     var exaflarePos = new Vector3[extendNum];
    //     for (var i = 0; i < extendNum; i++)
    //         exaflarePos[i] = GetExaflarePos(center, rotation, (i + 1) * extendDistance);
    //     return exaflarePos;
    // }

    public bool ExaflareRecordComplete()
    {
        return RecordedExaflareNum == 3;
    }

    private string GetExaflareIdxStr(int idx)
    {
        return idx switch
        {
            0 => "Top-Right",
            1 => "Back",
            2 => "Top-Left",
            _ => "Unknown"
        };
    }
    
    private string GetDirStr(int idx)
    {
        return idx switch
        {
            0 => "Straight Up",
            1 => "Top-Right",
            2 => "Straight Right",
            3 => "Bottom-Right",
            4 => "Straight Down",
            5 => "Bottom-Left",
            6 => "Straight Left",
            7 => "Top-Left",
            _ => "Unknown"
        };
    }

    public class ExaflareSolution
    {
        /*
         * Exaflare Optimization Strategy
         * There are four solution options for Exaflare:
         * 1. NeverFront
         *      Back two-step > Universe >>> Front one-step > Front two-step
         * 2. NeverUniverse
         *      Back two-step > Front one-step > Front two-step >>> Universe
         * 3. LeastMovement
         *      Front one-step > Back two-step > Front two-step >>> Universe
         * 4. AlwaysFront
         *      Front one-step > Front two-step > Back two-step > Universe
         * After solving, the lower score wins.
         *
         * Corresponding relationship between solution types and score contributions
         *                  basic   moveStep    isFront     isUniverse
         * NeverFront        100        2         100           50
         * NeverUniverse     100        2          10          100
         * LeastMovement     100       20          10           50
         * AlwaysFront       100        2         -10           50
         */
        public List<Vector3> ExaflareSolutionPosList { get; set; }
        public int MoveStep { get; set; }
        public bool IsFront { get; set; }
        public bool IsUniverse { get; set; }
        public int Score { get; set; }
        public string Description { get; set; }

        public ExaflareSolution(List<Vector3> exaflareSolutionPosList, int moveStep, bool isFront, bool isUniverse,
            string description, List<int> scoreList, ScriptAccessory accessory)
        {
            ExaflareSolutionPosList = exaflareSolutionPosList;
            MoveStep = moveStep;
            IsFront = isFront;
            IsUniverse = isUniverse;
            Score = CalcScore(scoreList, accessory, description);
            Description = description;
        }
        private int CalcScore(List<int> scoreList, ScriptAccessory accessory, string description)
        {
            const int moveStepIdx = 0;
            const int isFrontIdx = 1;
            const int isUniverseIdx = 2;
            const int baseScore = 100;
            var moveStepScore = scoreList[moveStepIdx] * MoveStep;
            var isFrontScore = IsFront ? scoreList[isFrontIdx] : 0;
            var isUniverseScore = IsUniverse ? scoreList[isUniverseIdx] : 0;
            var totalScore = baseScore + moveStepScore + isFrontScore + isUniverseScore;
            accessory.Log.Debug(
                $"{description} score: Base {baseScore} + Steps {moveStepScore} + Front {isFrontScore} + Universe {isUniverseScore} = {totalScore}");
            return totalScore;
        }
    }
}

#endregion


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

    public static float SourceRotation(this Event @event)
    {
        return JsonConvert.DeserializeObject<float>(@event["SourceRotation"]);
    }

    public static string SourceName(this Event @event)
    {
        return @event["SourceName"];
    }

    public static uint Id(this Event @event)
    {
        return ParseHexId(@event["Id"], out var id) ? id : 0;
    }

    public static uint StatusId(this Event @event)
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
    public static IGameObject? GetById(this ScriptAccessory sa, ulong id)
    {
        return sa.Data.Objects.SearchById(id);
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
        // if (r < 0) r = (float)(r + 2 * Math.PI);
        // if (r > 2 * Math.PI) r = (float)(r - 2 * Math.PI);

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
        // var r = (float)Math.PI - radian;
        // if (r < Math.PI) r = (float)(r + 2 * Math.PI);
        // if (r > Math.PI) r = (float)(r - 2 * Math.PI);

        return radian.Game2Logic();
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

        // Another solution to be verified
        // var nextPos = Vector3.Transform((point - center), Matrix4x4.CreateRotationY(radian)) + center;
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
        // Find the radian from the point to the center
        float radian = MathF.PI - MathF.Atan2(newPoint.X - center.X, newPoint.Z - center.Z);
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
        // Vector3 v3 = new(2 * centerX - point.X, point.Y, point.Z);
        // return v3;
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
        // Vector3 v3 = new(point.X, point.Y, 2 * centerZ - point.Z);
        // return v3;
        return point with { Z = 2 * centerZ - point.Z };
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
    /// Find the angular difference between two points, range 0~360deg
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
    /// From the center looking outwards, check if a target is to the right of another target.
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
        // Get player IDX
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
        // Get player role abbreviation, only for DEBUG output
        var idx = accessory.Data.PartyList.IndexOf(pid);
        var str = accessory.GetPlayerJobByIndex(idx);
        return str;
    }

    /// <summary>
    /// Input position index, get the corresponding position name, output string only for text output
    /// </summary>
    /// <param name="idx">Position index</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static string GetPlayerJobByIndex(this ScriptAccessory accessory, int idx)
    {
        var str = idx switch
        {
            0 => "MT",
            1 => "ST",
            2 => "H1",
            3 => "H2",
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
    public static ScriptColor ColorExaflare = new ScriptColor { V4 = new Vector4(1.0f, 1.0f, 0.0f, 1.5f) };
    public static ScriptColor ColorExaflareWarn = new ScriptColor { V4 = new Vector4(0.6f, 0.6f, 1f, 1.0f) };
}

public static class ListHelper
{
    /// <summary>
    /// Convert List to String for output
    /// </summary>
    /// <param name="list"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string StringList<T>(this List<T> list)
    {
        return string.Join(", ", list);
    }
}

public static class AssignDp
{
    /// <summary>
    /// Return arrow guidance related dp
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
            case Vector3 tpos:
                dp.TargetPosition = tpos;
                break;
        }

        return dp;
    }
    
    public static DrawPropertiesEdit DrawGuidance(this ScriptAccessory accessory, 
        object targetObj, int delay, int destroy, string name, float rotation = 0, float scale = 1f, bool isSafe = true)
    {
        return targetObj switch
        {
            uint uintTarget => accessory.DrawGuidance(accessory.Data.Me, uintTarget, delay, destroy, name, rotation, scale, isSafe),
            Vector3 vectorTarget => accessory.DrawGuidance(accessory.Data.Me, vectorTarget, delay, destroy, name, rotation, scale, isSafe),
            _ => throw new ArgumentException("targetObj must be of type uint or Vector3")
        };
    }
    
    
    /// <summary>
    /// Return dp from self to a target location, can modify dp.TargetPosition, dp.Scale
    /// </summary>
    /// <param name="targetPos">Target location</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="scale">Guidance line width</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawDirPos(this ScriptAccessory accessory, Vector3 targetPos, int delay, int destroy, string name, float scale = 1f)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(scale);
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = targetPos;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        return dp;
    }

    /// <summary>
    /// Return dp from a start location to a target location, can modify dp.Position, dp.TargetPosition, dp.Scale
    /// </summary>
    /// <param name="startPos">Start location</param>
    /// <param name="targetPos">Target location</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="scale">Guidance line width</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawDirPos2Pos(this ScriptAccessory accessory, Vector3 startPos, Vector3 targetPos, int delay, int destroy, string name, float scale = 1f)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(scale);
        dp.Position = startPos;
        dp.TargetPosition = targetPos;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
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
    public static DrawPropertiesEdit DrawOwnersEnmityOrder(this ScriptAccessory accessory, ulong ownerId, uint orderIdx, float width, float length, int delay, int destroy, string name, bool lengthByDistance = false, bool byTime = false)
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
    /// Return static dp, usually for guiding fixed positions. Can modify dp.Position, dp.Rotation, dp.Scale
    /// </summary>
    /// <param name="center">Drawing center position</param>
    /// <param name="radian">Shape angle</param>
    /// <param name="rotation">Rotation angle, North as 0 degrees clockwise</param>
    /// <param name="width">Drawing width</param>
    /// <param name="length">Drawing length</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destroy">Drawing disappears after `destroy` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawStatic(this ScriptAccessory accessory, Vector3 center, float radian, float rotation, float width, float length, int delay, int destroy, string name)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Position = center;
        dp.Radian = radian;
        dp.Rotation = rotation.Logic2Game();
        dp.Color = accessory.Data.DefaultDangerColor;
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
    public static DrawPropertiesEdit DrawStaticCircle(this ScriptAccessory accessory, Vector3 center, Vector4 color, int delay, int destroy, string name, float scale = 1.5f)
    {
        var dp = accessory.DrawStatic(center, 0, 0, scale, scale, delay, destroy, name);
        dp.Color = color;
        return dp;
    }

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
    public static DrawPropertiesEdit DrawStaticDonut(this ScriptAccessory accessory, Vector3 center, Vector4 color, int delay, int destroy, string name, float scale, float innerscale = 0)
    {
        var dp = accessory.DrawStatic(center, float.Pi * 2, 0, scale, scale, delay, destroy, name);
        dp.Color = color;
        dp.InnerScale = innerscale != 0f ? new Vector2(innerscale) : new Vector2(scale - 0.05f);
        return dp;
    }

    /// <summary>
    /// Return fan drawing
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
    public static DrawPropertiesEdit DrawFan(this ScriptAccessory accessory, ulong ownerId, float radian, float rotation, float scale, float innerScale, int delay, int destroy, string name, bool byTime = false)
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
}

#endregion
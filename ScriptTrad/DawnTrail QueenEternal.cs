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
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;

namespace UsamisKodakku.Script._07_DawnTrail.QueenEternal;

[ScriptType(name: Name, territorys: [1243], guid: "45fff289-e23d-41ab-9039-71cd310668e4", 
    version: Version, author: "Usami", note: NoteStr)]

// ^(?!.*((Monk|Machinist|Dragoon|Samurai|Ninja|Viper|Reaper|Dancer|Bard|Astrologian|Sage|Scholar|(Eos|Selene)|Seraph|White Mage|Warrior|Paladin|Dark Knight|Gunbreaker|Pictomancer|Black Mage|Blue Mage|Summoner|Carbuncle|Demigod Bahamut|Demigod Phoenix|Garuda-Egi|Titan-Egi|Ifrit-Egi|Puppet)\] (Used|Cast))).*35501.*$

public class QueenEternalEx
{
    const string NoteStr =
    """
    Absolute Authority uses the standard strategy. If skipping mechanics, please ignore the nuclear explosion (Flare) direction arrow.
    For Earth phase's intelligent priority tower assignment, you can enable "Earth Phase 4-Player Tower Intelligent Priority Determination" in the user settings.
    If not using this feature, ensure the party list order is correct (pay special attention to D1/D2) to avoid incorrect tower guidance.
    Duckism.
    """;

    private const string Name = "QueenEternalEx [Eternal Queen's Reminiscence Extreme]";
    private const string Version = "0.0.0.9";
    private const string DebugVersion = "a";
    private const string Note = "Initial version completed";
    
    [UserSetting("Debug Mode, turn off for non-development use")]
    public static bool DebugMode { get; set; } = false;
    [UserSetting("Earth Phase 4-Player Tower Intelligent Priority Determination")]
    public static bool IntelligentPriorTowerMode { get; set; } = true;
    [UserSetting("Position Hint Circle Drawing - Normal Color")]
    public static ScriptColor PosColorNormal { get; set; } = new ScriptColor { V4 = new Vector4(1.0f, 1.0f, 1.0f, 1.0f) };
    [UserSetting("Position Hint Circle Drawing - Player Position Color")]
    public static ScriptColor PosColorPlayer { get; set; } = new ScriptColor { V4 = new Vector4(0.0f, 1.0f, 1.0f, 1.0f) };
    [UserSetting("Exaflare Explosion Zone Color")]
    public ScriptColor exflareColor { get; set; } = new ScriptColor { V4 = new Vector4(0f, 1.0f, 1.0f, 1.0f) };
    [UserSetting("Exaflare Warning Zone Color")]
    public ScriptColor exflareWarnColor { get; set; } = new ScriptColor { V4 = new Vector4(0f, 0.5f, 1.0f, 1.0f) };
    
    private List<bool> _drawn = new bool[20].ToList();                   // Drawing record
    private QueenEternalPhase _phase = QueenEternalPhase.Init;
    private static uint _bossId = 0;
    
    private static readonly Vector3 Center = new Vector3(100, 0, 100);
    private static readonly Vector3 CenterWind = new Vector3(100, 0, 92.5f);
    private static readonly Vector3 CenterEarth = new Vector3(100, 0, 94f);
    private static readonly Vector3 CenterIce = new Vector3(100, 0, 100);
    private static readonly Vector3 GravityFieldLeft = new Vector3(92f, 0, 94f);
    private List<ManualResetEvent> _manualResetEvents = Enumerable.Repeat(new ManualResetEvent(false), 20).ToList();
    private List<AutoResetEvent> _autoResetEvents = Enumerable.Repeat(new AutoResetEvent(false), 20).ToList();
    private List<bool> _earthPhaseTarget = new bool[8].ToList(); // Drawing record
    private List<int> _intelligentPriority = Enumerable.Repeat(0, 8).ToList();
    private bool _intelligentPriorityValid = true;
    
    private bool _isFlareTarget = false;    // Absolute Authority Flare target
    private Vector3 _sourceIceDartPos = new Vector3(0, 0, 0);     // Position of the ice pillar tethered to the player
    private int _myIceBridgeIdx = 0;       // Ice bridge taken during the ice phase
    private List<bool> _iceRushRangeDrawn = new bool[8].ToList();   // Whether the Ice Rush range has been drawn
    private int _raisedTributeNum = 0;      // Tether stack, stack count

    
    public void Init(ScriptAccessory accessory)
    {
        // DebugMsg($"Init {Name} v{Version}{DebugVersion} Success.\n{Note}", accessory);
        _phase = QueenEternalPhase.Init;
        _drawn = new bool[20].ToList();
        _bossId = 0;
        _manualResetEvents = Enumerable.Repeat(new ManualResetEvent(false), 20).ToList();
        _autoResetEvents = Enumerable.Repeat(new AutoResetEvent(false), 20).ToList();
        
        _earthPhaseTarget = new bool[8].ToList(); // Drawing record
        _isFlareTarget = false;     // Absolute Authority Flare target
        _sourceIceDartPos = new Vector3(0, 0, 0);     // Position of the ice pillar tethered to the player
        _myIceBridgeIdx = 0;       // Ice bridge taken during the ice phase
        _iceRushRangeDrawn = new bool[8].ToList();   // Whether the Ice Rush range has been drawn
        _raisedTributeNum = 0;      // Tether stack, stack count
        _exaflareShown = false;     // Whether exaflare is displayed
        
        _intelligentPriority = Enumerable.Repeat(0, 8).ToList();    // Weight for 4-player tower intelligent priority mode
        _intelligentPriorityValid = true;   // Whether the 4-player tower intelligent priority mode is valid
        
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
        // ---- DEBUG CODE ----
        
        // -- DEBUG CODE END --
    }
    
    #region Aethertithe
    
    [ScriptMethod(name: "Record Boss ID", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40972"], userControl: false)]
    public void BossIdRecord(Event @event, ScriptAccessory accessory)
    {
        if (_bossId != 0) return;
        _bossId = @event.SourceId();
    }
    
    [ScriptMethod(name: "---- 《Aethertithe》 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_Aethertithe(Event @event, ScriptAccessory accessory)
    {
    }
    
    [ScriptMethod(name: "Aethertithe Stack Area Hint", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:regex:^((04|08|10)000100)$", "Index:00000000"], userControl: true)]
    public void AethertitheStackPositionHint(Event @event, ScriptAccessory accessory)
    {
        var id = @event.Id();
        List<bool> safePlace = [true, true, true];
        const uint left = 0x04000100;
        const uint middle = 0x08000100;
        const uint right = 0x10000100;
        var idx = id switch
        {
            left => 0,
            middle => 1,
            _ => 2
        };
        
        safePlace[idx] = false;
        var myIndex = accessory.GetMyIndex();
        var playerPos = myIndex % 2 == 0 ? safePlace.IndexOf(true) : safePlace.LastIndexOf(true);
        
        List<Vector3> dirPos = [new Vector3(80, 0, 100), new Vector3(100, 0, 120), new Vector3(120, 0, 100)];
        List<float> rot = [-60f.DegToRad(), 0, 60f.DegToRad()];
        for (var i = 0; i < 3; i++)
        {
            var startPos = new Vector3(100, 0, 80);
            var dp = accessory.DrawGuidance(startPos, dirPos[i], 0, 6000, $"StackGuidance{i}", 0, 2f);
            dp.Color = i == playerPos ? PosColorPlayer.V4.WithW(3f) : PosColorNormal.V4;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }

        var dp0 = accessory.DrawFan(_bossId, 70f.DegToRad(), rot[idx], 100, 0, 0, 20000, $"Aethertithe{idx}");
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp0);
        
        // var stackPosStr = playerPos switch
        // {
        //     0 => "Left",
        //     1 => "Middle",
        //     _ => "Right"
        // };
        // accessory.Method.TextInfo($"{stackPosStr} Stack", 6000);
    }
    
    [ScriptMethod(name: "Aethertithe Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4097[456])"], userControl: false)]
    public void AethertitheStackRemove(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }
    
    #endregion Aethertithe

    #region Left/Right Cleave

    [ScriptMethod(name: "---- 《Legitimate Force》 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_LegitimateForce(Event @event, ScriptAccessory accessory)
    {
    }
    
    private bool _legitimateForce;
    [ScriptMethod(name: "Legitimate Force Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4099[02])"], userControl: true)]
    public void LegitimateForce(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        var aid = @event.ActionId();
        _legitimateForce = true;
        const uint attackLeftHand = 40992;
        
        var dp = accessory.DrawLeftRightCleave(sid, aid == attackLeftHand, 3000, 5000, $"LegitimateForce1");
        dp.Scale = new Vector2(150f);
        if (_phase is QueenEternalPhase.Earth)
        {
            dp.Offset = new(0, -3.5f, 0);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
        }
        else
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Legitimate Force Second", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4099[02])"], userControl: false)]
    public void LegitimateForceSecond(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        var aid = @event.ActionId();
        if (!_legitimateForce) return;
        _legitimateForce = false;
        accessory.Method.RemoveDraw($"LegitimateForce1");
        const uint attackLeftHand = 40992;
        var dp = accessory.DrawLeftRightCleave(sid, aid != attackLeftHand, 0, 8000, $"LegitimateForce2");
        dp.Scale = new Vector2(150f);
        if (_phase == QueenEternalPhase.Earth)
        {
            dp.Offset = new(0, -3.5f, 0);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
        }
        else
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "Legitimate Force Second Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4099[34])"], userControl: false)]
    public void LegitimateForceRemove(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"LegitimateForce2");
    }
    
    #endregion
    
    #region Wind Phase
    
    [ScriptMethod(name: "---- 《Laws of Wind》 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_LawsofWind(Event @event, ScriptAccessory accessory)
    {
    }
    
    [ScriptMethod(name: "Wind Phase Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40995"], userControl: false)]
    public void LawsofWindPhaseChange(Event @event, ScriptAccessory accessory)
    {
        _phase = QueenEternalPhase.Wind;
        accessory.DebugMsg($"Current phase: {_phase}", DebugMode);
        _autoResetEvents[0].Set();
    }
    
    [ScriptMethod(name: "Wind Stack Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40995"], userControl: true)]
    public void AeroquellStackPositionHint(Event @event, ScriptAccessory accessory)
    {
        _autoResetEvents[0].WaitOne();
        if (_phase != QueenEternalPhase.Wind) return;
        var myIndex = accessory.GetMyIndex();
        
        var stackPos = new Vector3(94f, 0, 89.5f);
        if (myIndex % 2 == 1)
            stackPos = stackPos.FoldPointHorizon(CenterWind.X).FoldPointVertical(CenterWind.Z);
        var dp = accessory.DrawGuidance(stackPos, 0, 9000, $"WindStack");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        
        // var stackPosStr = myIndex % 2 == 0 ? "Top-left center" : "Bottom-right center";
        // accessory.Method.TextInfo($"{stackPosStr} Stack", 4000);
        // accessory.Method.TTS($"{stackPosStr} Stack");
    }
    
    [ScriptMethod(name: "Missing Link Stack First Hint", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0146"], userControl: true)]
    public void MissingLinkStackFirst(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Wind) return;
        var tid = @event.TargetId();
        if (tid != accessory.Data.Me) return;
        
        var dp = accessory.DrawGuidance(CenterWind, 0, 4000, $"MissingLinkStack");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        
        accessory.Method.TextInfo($"Gather center", 4000);
        // accessory.Method.TTS($"Gather center";
    }
    
    [ScriptMethod(name: "Missing Link Stack Remove", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3587)$"], userControl: false)]
    public void MissingLinkStackRemove(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"MissingLinkStack");
    }
    
    [ScriptMethod(name: "Missing Line Tether Direction Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3587)$"], userControl: true)]
    public void MissingLineTether(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Wind) return;
        var tid = @event.TargetId();
        if (tid != accessory.Data.Me) return;
        var myIndex = accessory.GetMyIndex();
        
        var tetherDes = new Vector3(88f, 0, 104f);
        if (myIndex >= 4)
            tetherDes = tetherDes.FoldPointHorizon(CenterWind.X).FoldPointVertical(CenterWind.Z);
        var dp = accessory.DrawGuidance(tetherDes, 0, 6000, $"TetherPosition");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        
        // var tetherDesStr = myIndex <= 3 ? $"Bottom-left" : "Top-right";
        // accessory.Method.TextInfo($"{tetherDesStr} Tether", 3000);
        // accessory.Method.TTS($"{tetherDesStr} Tether");
    }
    
    [ScriptMethod(name: "Missing Line Tether Direction Remove", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:regex:^(3587)$"], userControl: false)]
    public void MissingLineTetherRemove(Event @event, ScriptAccessory accessory)
    {
        var tid = @event.TargetId();
        if (tid != accessory.Data.Me) return;
        accessory.Method.RemoveDraw($"TetherPosition");
        _autoResetEvents[0].Set();
    }

    private bool _windChargeToLeft;
    [ScriptMethod(name: "Wind Knockback Direction Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(4189|4190)$"], userControl: false)]
    public void WindOfChangeRecord(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Wind) return;
        var tid = @event.TargetId();
        var stid = @event.StatusId();
        if (tid != accessory.Data.Me) return;
        const uint eastWindOfCharge = 4189;
        _windChargeToLeft = stid == eastWindOfCharge;   // Knock back left
        accessory.DebugMsg($"Recorded {stid}, {(_windChargeToLeft ? "Knock back left" : "Knock back right")}", DebugMode);
    }

    private List<Vector3> _windChargeGuidancePosition = [new(0, 0, 0), new(0, 0, 0), new(0, 0, 0)];
    private bool _windGuidance;
    [ScriptMethod(name: "Wind Knockback Guidance Part 1", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4099[02])$"], userControl: true)]
    public void WindOfChangeGuidancePart1(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Wind) return;
        _autoResetEvents[0].WaitOne();
        var aid = @event.ActionId();
        var myIndex = accessory.GetMyIndex();
        _windGuidance = true;
        
        const float deltaZ = 10.5f;   // Z difference between top and bottom half
        const float centerDeltaX = 1f;  // X difference to dodge left/right cleave
        const float edgeX = 12f;    // X difference between center and wind arena edge
        
        // TN are in the bottom half, so add dz. This is based on the top half.
        var dz = myIndex <= 3 ? new Vector3(0, 0, deltaZ) : new Vector3(0, 0, -deltaZ);

        // Determine dx offset based on left/right cleave order
        const uint attackRightFirst = 40992;
        var dx = aid == attackRightFirst ? new Vector3(-centerDeltaX, 0, 0) : new Vector3(centerDeltaX, 0, 0);

        // Determine dxWind offset based on knockback direction
        var dxWind = _windChargeToLeft ? new Vector3(edgeX, 0, 0) : new Vector3(-edgeX, 0, 0);

        var targetPos1 = CenterWind + dz + dx;
        var targetPos2 = CenterWind + dz - dx;
        var targetPos3 = CenterWind + dz + dxWind;
        _windChargeGuidancePosition = [targetPos1, targetPos2, targetPos3];
        
        var dp1 = accessory.DrawGuidance(_windChargeGuidancePosition[0], 0, 8000, $"WindKnockback1");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
        var dp12 = accessory.DrawGuidance(_windChargeGuidancePosition[0], _windChargeGuidancePosition[1], 0, 8000, $"WindKnockback12");
        dp12.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp12);
    }
    
    [ScriptMethod(name: "Wind Knockback Guidance Part 2", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4099[02])$"], userControl: false)]
    public void WindOfChangeGuidancePart2(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Wind) return;
        accessory.Method.RemoveDraw($"WindKnockback1");
        accessory.Method.RemoveDraw($"WindKnockback12");
        
        if (!_windGuidance) return;
        var dp2 = accessory.DrawGuidance(_windChargeGuidancePosition[1], 0, 6000, $"WindKnockback2");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
        var dp23 = accessory.DrawGuidance(_windChargeGuidancePosition[1], _windChargeGuidancePosition[2], 0, 8000, $"WindKnockback23");
        dp23.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp23);
    }
    
    [ScriptMethod(name: "Wind Knockback Guidance Part 3", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4099[34])$"], userControl: false)]
    public void WindOfChangeGuidancePart3(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Wind) return;
        accessory.Method.RemoveDraw($"WindKnockback2");
        accessory.Method.RemoveDraw($"WindKnockback23");
        
        if (!_windGuidance) return;
        var dp3 = accessory.DrawGuidance(_windChargeGuidancePosition[2], 0, 6000, $"WindKnockback3");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);
    }
    
    [ScriptMethod(name: "Wind Knockback Guidance Remove", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:regex:^(4189|4190)$"], userControl: false)]
    public void WindOfChangeGuidanceRemove(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Wind) return;
        if (@event.TargetId() != accessory.Data.Me) return;
        accessory.Method.RemoveDraw($".*");
        _windGuidance = false;
    }
    
    #endregion Wind Phase

    #region Divide and Conquer
    [ScriptMethod(name: "---- 《Divide and Conquer》 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_DivideAndConquer(Event @event, ScriptAccessory accessory)
    {
    }
    
    [ScriptMethod(name: "Position Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40983)$"], userControl: true)]
    public void DivideAndConquerPosition(Event @event, ScriptAccessory accessory)
    {
        var myIndex = accessory.GetMyIndex();
        var sid = @event.SourceId();
        var spos = @event.SourcePosition();

        List<float> rot = [-85f.DegToRad(), 85f.DegToRad(), -10f.DegToRad(), 10f.DegToRad(), -60f.DegToRad(), 60f.DegToRad(), -35f.DegToRad(), 35f.DegToRad()];
        for (var i = 0; i < 8; i++)
        {
            var dp = accessory.DrawRect(sid, 2f, 60f, 0, 10000, $"DivideAndConquer{i}");
            dp.Rotation = rot[i];
            dp.Color = i == myIndex ? PosColorPlayer.V4.WithW(3f) : PosColorNormal.V4;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }
        
        // var standPosStr = myIndex switch
        // {
        //     0 => "Top-left",
        //     1 => "Top-right",
        //     2 => "Center-left",
        //     3 => "Center-right",
        //     4 => "Left 2",
        //     5 => "Right 2",
        //     6 => "Left 3",
        //     7 => "Right 3",
        //     _ => "???"
        // };
        // accessory.Method.TextInfo($"{standPosStr} bait then dodge", 4000);
        // accessory.Method.TTS($"{standPosStr} bait then dodge");
    }
    
    [ScriptMethod(name: "Divide and Conquer Second", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(30505)$"], userControl: true)]
    public void DivideAndConquerDelayCast(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();

        var dp = accessory.DrawRect(sid, 5, 60, 0, 3000, $"DivideAndConquerSecond{sid}");
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    #endregion Divide and Conquer

    #region Earth Phase

    [ScriptMethod(name: "---- 《Laws of Earth》 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_LawsofEarth(Event @event, ScriptAccessory accessory)
    {
    }
    
    [ScriptMethod(name: "Earth Phase Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41000"], userControl: false)]
    public void LawsofEarthPhaseChange(Event @event, ScriptAccessory accessory)
    {
        _phase = QueenEternalPhase.Earth;
        accessory.DebugMsg($"Current phase: {_phase}", DebugMode);
    }
    
    [ScriptMethod(name: "First Round Tower Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4099[02])$"], userControl: true)]
    public void LawsofEarthTower8(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Earth) return;

        // Left/Right Gravity Field
        var targetPos1 = Enumerable.Repeat(new Vector3(0, 0, 0), 2).ToList();
        targetPos1[0] = GravityFieldLeft;
        targetPos1[1] = GravityFieldLeft.FoldPointHorizon(CenterEarth.X);
        
        // 8-player tower positions
        List<Vector3> targetPos2 = Enumerable.Repeat(new Vector3(0, 0, 0), 8).ToList();
        targetPos2[0] = new Vector3(94f, 0, 88f);
        targetPos2[2] = targetPos2[0].FoldPointVertical(CenterEarth.Z);
        targetPos2[4] = targetPos2[0].FoldPointHorizon(GravityFieldLeft.X);
        targetPos2[6] = targetPos2[4].FoldPointVertical(CenterEarth.Z);
        targetPos2[1] = targetPos2[0].FoldPointHorizon(CenterEarth.X);
        targetPos2[3] = targetPos2[2].FoldPointHorizon(CenterEarth.X);
        targetPos2[5] = targetPos2[4].FoldPointHorizon(CenterEarth.X);
        targetPos2[7] = targetPos2[6].FoldPointHorizon(CenterEarth.X);

        var myIndex = accessory.GetMyIndex();
        var idx1 = myIndex % 2 == 0 ? 0 : 1;

        var dp1 = accessory.DrawGuidance(CenterEarth, targetPos1[idx1], 0, 11000, $"StandOnGravityField{idx1}");
        var dp2 = accessory.DrawGuidance(targetPos1[idx1], targetPos2[myIndex], 0, 11000, $"Tower{myIndex}");
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp2.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
        
        dp1 = accessory.DrawGuidance(CenterEarth, targetPos1[idx1], 11000, 5000, $"StandOnGravityField{idx1}");
        dp2 = accessory.DrawGuidance(targetPos1[idx1], targetPos2[myIndex], 11000, 5000, $"Tower{myIndex}");
        dp1.Color = accessory.Data.DefaultSafeColor;
        dp2.Color = accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
        
        // var idxStr = myIndex switch
        // {
        //     0 => "Top-left inner",
        //     1 => "Bottom-left inner",
        //     2 => "Top-right inner",
        //     3 => "Bottom-right inner",
        //     4 => "Top-left outer",
        //     5 => "Bottom-left outer",
        //     6 => "Top-right outer",
        //     7 => "Bottom-right outer",
        //     _ => "???"
        // };
        //
        // accessory.Method.TextInfo($"Prepare to stand on {idxStr} tower", 4000);
        // accessory.Method.TTS($"Prepare to stand on {idxStr} tower");
    }

    [ScriptMethod(name: "Tower Hint Delete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4100[12])$"], userControl: false)]
    public void LawsofEarthTower8Remove(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Earth) return;
        accessory.Method.RemoveDraw($".*");
    }
    
    [ScriptMethod(name: "Tower Priority Determination", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4100[12])$"], userControl: false)]
    public void Tower4PriorityJudge(Event @event, ScriptAccessory accessory)
    {
        // Determine based on the relative positions of the 8-player tower
        if (_phase != QueenEternalPhase.Earth) return;
        if (_drawn[3]) return;
        _drawn[3] = true;
        if (!IntelligentPriorTowerMode) return;

        for (var i = 0; i < accessory.Data.PartyList.Count; i++)
        {
            var str = $"Info for player {accessory.GetPlayerJobByIndex(i)}: ";
            var id = accessory.Data.PartyList[i];
            var chara = accessory.GetById(id);

            if (chara == null || chara.IsDead)
            {
                _intelligentPriorityValid = false;
                accessory.DebugMsg($"A character is dead or non-existent, intelligent determination disabled.", DebugMode);
                return;
            }

            if (accessory.Data.MyObject is null) return;
            if (!accessory.Data.MyObject.IsDps())
            {
                _intelligentPriority[i] += 1;
                str += $"is TN (+1)";
            }
            else
            {
                str += $"is DPS (+0)";
            }

            if (chara.Position.X > CenterEarth.X)
            {
                _intelligentPriority[i] += 100;
                str += $", in right half (+100)";
            }
            else
            {
                str += $", in left half (+0)";
            }
            
            if (chara.Position.Z > CenterEarth.Z)
            {
                _intelligentPriority[i] += 10;
                str += $", in bottom half (+10)";
            }
            else
            {
                str += $", in top half (+0)";
            }
            accessory.DebugMsg($"{str} = {_intelligentPriority[i]} points.", DebugMode);
        }
        
        // Check validity
        // 1. 4 players in right half (>= 100)
        // 2. 4 players in bottom half (% 100 >= 10)
        if (_intelligentPriority.Count(x => x >= 100) != 4)
        {
            _intelligentPriorityValid = false;
            accessory.DebugMsg($"Number of players in right half is not 4, intelligent determination disabled", DebugMode);
            return;
        }

        if (_intelligentPriority.Count(x => x % 100 >= 10) != 4)
        {
            _intelligentPriorityValid = false;
            accessory.DebugMsg($"Number of players in bottom half is not 4, intelligent determination disabled", DebugMode);
            return;
        }
    }
    
    [ScriptMethod(name: "Big Circle (Defamation) Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(41004)$"], userControl: true)]
    public void LawsofEarthDefamation(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Earth) return;
        var tid = @event.TargetId();
        var tidx = accessory.GetPlayerIdIndex(tid);
        HandleEarthPhaseTarget(tidx, accessory, $"Big Circle Target");
        
        if (tid != accessory.Data.Me) return;
        // Assuming groups are correct, one left and one right
        
        // Left/Right Gravity Field
        var targetPos1 = Enumerable.Repeat(new Vector3(0, 0, 0), 2).ToList();
        targetPos1[0] = GravityFieldLeft;
        targetPos1[1] = GravityFieldLeft.FoldPointHorizon(CenterEarth.X);
        
        var targetPos2 = Enumerable.Repeat(new Vector3(0, 0, 0), 2).ToList();
        targetPos2[0] = new Vector3(82f, 0, 95f);
        targetPos2[1] = targetPos2[0].FoldPointHorizon(CenterEarth.X);
    
        var myIndex = accessory.GetMyIndex();
        var idx = myIndex % 2 == 0 ? 0 : 1;
        var bias = myIndex % 4 <= 1 ? new Vector3(0, 0, -5) : new Vector3(0, 0, 5);
        
        var dp1 = accessory.DrawGuidance(targetPos1[idx] + bias, targetPos1[idx], 0, 8000,
            $"StandOnGravityField{idx}");
        var dp2 = accessory.DrawGuidance(targetPos1[idx], targetPos2[idx], 0, 8000, $"PlaceCircle{idx}");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
    }
    
    [ScriptMethod(name: "Fan (Cone) Bait Hint", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0011"], userControl: true)]
    public void LawsofEarthFan(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Earth) return;
        var sid = @event.SourceId();
        var sidx = accessory.GetPlayerIdIndex(sid);
        HandleEarthPhaseTarget(sidx, accessory, $"Fan Target");

        var dp = accessory.DrawFan(_bossId, 60f.DegToRad(), 0, 60f, 0, 4000, 4000, $"FanBait");
        dp.TargetObject = sid;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
        
        if (sid != accessory.Data.Me) return;
        // Assuming groups are correct, one left and one right
        
        // Left/Right Gravity Field
        var targetPos1 = Enumerable.Repeat(new Vector3(0, 0, 0), 2).ToList();
        targetPos1[0] = GravityFieldLeft;
        targetPos1[1] = GravityFieldLeft.FoldPointHorizon(CenterEarth.X);
        
        var targetPos2 = Enumerable.Repeat(new Vector3(0, 0, 0), 2).ToList();
        targetPos2[0] = new Vector3(90f, 0, 80.5f);
        targetPos2[1] = targetPos2[0].FoldPointHorizon(CenterEarth.X);

        var myIndex = accessory.GetMyIndex();
        var idx = myIndex % 2 == 0 ? 0 : 1;
        var bias = myIndex % 4 <= 1 ? new Vector3(0, 0, -5) : new Vector3(0, 0, 5);
        
        var dp1 = accessory.DrawGuidance(targetPos1[idx] + bias, targetPos1[idx], 0, 8000,
            $"StandOnGravityField{idx}");
        var dp2 = accessory.DrawGuidance(targetPos1[idx], targetPos2[idx], 0, 8000, $"PlaceCircle{idx}");
        dp2.Offset = new Vector3(0, -3.5f, 0);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
    }
    
    private void HandleEarthPhaseTarget(int tidx, ScriptAccessory accessory, string targetType)
    {
        lock (_earthPhaseTarget)
        {
            _earthPhaseTarget[tidx] = true;
            var count = _earthPhaseTarget.Count(x => x == true);
            if (count == 4) _manualResetEvents[0].Set();

            if (DebugMode)
            {
                accessory.DebugMsg($"Detected {targetType}, now have {count} targets", DebugMode);
            }
        }
    }
    
    [ScriptMethod(name: "Second Round Tower Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(41003)$"], userControl: true)]
    public void LawsofEarthGravityEmpireTower(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Earth) return;
        
        // var count = _earthPhaseTarget.Count(x => x == true);
        // accessory.DebugMsg($"Before WaitOne, {count} targets", DebugMode);
        _manualResetEvents[0].WaitOne();
        // var count1 = _earthPhaseTarget.Count(x => x == true);
        // accessory.DebugMsg($"After WaitOne, {count1} targets", DebugMode);
        
        var myIndex = accessory.GetMyIndex();
        List<int> sortedTowerTargetIdxs;

        if (_intelligentPriorityValid)
        {
            var str = "";
            // Find the four indices where _earthPhaseTarget is false
            var towerTargetIdxs = _earthPhaseTarget
                .Select((value, index) => new { value, index })
                .Where(x => x.value == false)
                .Select(x => x.index)
                .ToList();
            str = $"Players assigned to towers: {accessory.BuildListStr(towerTargetIdxs)}\n";
            
            // Find the corresponding _intelligentPriority values
            var priorityValues = towerTargetIdxs
                .Select(index => _intelligentPriority[index])
                .ToList();
            str = $"Corresponding priority weights: {accessory.BuildListStr(priorityValues)}\n";
            
            // Sort them ascending, record original indices
            sortedTowerTargetIdxs = priorityValues
                .Select((value, index) => new { OriginalIndex = towerTargetIdxs[index], Value = value })
                .OrderBy(x => x.Value)
                .Select(x => x.OriginalIndex)
                .ToList();
            str = $"Tower intelligent priority order: {accessory.BuildListStr(sortedTowerTargetIdxs)}\n";
            
            accessory.DebugMsg($"{str}", DebugMode);
        }
        else
        {
            // Priority order based on D1-MT-D3-H1, D2-ST-D4-H2
            List<int> priorityIdx = [4, 0, 6, 2, 5, 1, 7, 3];

            var towerTargetIdxs = _earthPhaseTarget
                .Select((value, index) => new { value, index })
                .Where(x => x.value == false)
                .Select(x => x.index)
                .ToList();

            var priorityMap = priorityIdx
                .Select((value, index) => new { value, index })
                .ToDictionary(x => x.value, x => x.index);

            sortedTowerTargetIdxs = towerTargetIdxs
                .OrderBy(index => priorityMap[index])
                .ToList();

            var str = accessory.BuildListStr(sortedTowerTargetIdxs);
            accessory.DebugMsg($"Tower priority order: {str}", DebugMode);
        }
        
        if (_earthPhaseTarget[myIndex]) return;

        var myTowerIdx = sortedTowerTargetIdxs.IndexOf(myIndex);
        List<Vector3> targetTowerPositions = Enumerable.Repeat(new Vector3(0, 0, 0), 4).ToList();
        targetTowerPositions[0] = new Vector3(GravityFieldLeft.X, 0, 89f);
        targetTowerPositions[1] = targetTowerPositions[0].FoldPointVertical(GravityFieldLeft.Z);
        targetTowerPositions[2] = targetTowerPositions[0].FoldPointHorizon(CenterEarth.X);
        targetTowerPositions[3] = targetTowerPositions[1].FoldPointHorizon(CenterEarth.X);

        var dp = accessory.DrawGuidance(targetTowerPositions[myTowerIdx], 0, 8000, $"Tower{myTowerIdx}");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Earth Phase Gravity Empire Mechanic Delete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(41003)$"], userControl: false)]
    public void LawsofEarthGravityEmpireRemove(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Earth) return;
        accessory.Method.RemoveDraw($".*");
        _manualResetEvents[0].Reset();
    }
    
    #endregion Earth Phase

    #region Meteor Placement

    [ScriptMethod(name: "Meteor Placement", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4100[678])$"], userControl: true)]
    public void MeteorImpactPos(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Earth) return;
        var aid = @event.ActionId();
        var myIndex = accessory.GetMyIndex();
        var tid = @event.TargetId();

        const uint meteorStart = 41006;
        if (aid != meteorStart && tid != accessory.Data.Me) return;
        
        List<Vector3[]> targetPos = Enumerable.Range(0, 8)
            .Select(_ => new Vector3[] { new(0, 0, 0), new(0, 0, 0) })
            .ToList();
        targetPos[0] = [new Vector3(88.5f, 0, GravityFieldLeft.Z), new Vector3(95.5f, 0, GravityFieldLeft.Z)];
        targetPos[1] = [new Vector3(104.5f, 0, GravityFieldLeft.Z), new Vector3(111.5f, 0, GravityFieldLeft.Z)];
        targetPos[4] = [new Vector3(88.5f, 0, 86.5f), new Vector3(95.5f, 0, 86.5f)];
        targetPos[5] = [new Vector3(104.5f, 0, 86.5f), new Vector3(111.5f, 0, 86.5f)];
        targetPos[2] = [CenterEarth, CenterEarth];
        targetPos[3] = [CenterEarth, CenterEarth];
        targetPos[6] = [CenterEarth, CenterEarth];
        targetPos[7] = [CenterEarth, CenterEarth];
        
        if (aid == 41006)
        {
            var dp = accessory.DrawGuidance(targetPos[myIndex][0], 0, 8000, $"Meteor1");
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        else if (!_drawn[0])
        {
            _drawn[0] = true;
            accessory.Method.RemoveDraw($"Meteor1");
            var dp = accessory.DrawGuidance(targetPos[myIndex][1], 0, 8000, $"Meteor2");
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        else
        {
            accessory.Method.RemoveDraw(".*");
        }
    }

    #endregion Meteor Placement

    #region Absolute Authority
    
    [ScriptMethod(name: "---- 《Absolute Authority》 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_AbsoluteAuthority(Event @event, ScriptAccessory accessory)
    {
    }

    [ScriptMethod(name: "Bait Cannon", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(010[EF])$"], userControl: true)]
    public void CoronationGuidance(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        if (sid != accessory.Data.Me) return;
        var tpos = @event.TargetPosition();
        var dir = tpos.Position2Dirs(Center, 4);
        var id = @event.Id();
        const uint relativeRightCorner = 0x010F;
        
        List<Vector3> targetPos = Enumerable.Repeat(new Vector3(0, 0, 0), 8).ToList();
        for (var i = 0; i < 4; i++)
        {
            targetPos[i] = Center.ExtendPoint((45f + 90f * i).DegToRad(), 27.5f);
            targetPos[i + 4] = Center.ExtendPoint((90f * i).DegToRad(), 19.5f);
        }
        var myTargetPos = id == relativeRightCorner ? targetPos[dir] : targetPos[dir + 4];
        var dp = accessory.DrawGuidance(myTargetPos, 0, 10000, $"BaitCannon");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Bait Cannon Disappear", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(40982)$"], userControl: false)]
    public void CoronationGuidanceRemove(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }

    [ScriptMethod(name: "Corner Standby Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(41025)$"], userControl: true)]
    public void AbsoluteAuthorityHint(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.DrawGuidance(new Vector3(119, 0, 81), 0, 10000, $"AbsoluteAuthorityTopRightStandby");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Movement to Center", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(41025)$"], userControl: true)]
    public void AbsoluteAuthorityGuide(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"AbsoluteAuthorityTopRightStandby");
        var dp1 = accessory.DrawGuidance(new Vector3(119, 0, 81), new Vector3(100, 0, 81), 0, 10000, $"AbsoluteAuthorityMovement1");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
        var dp2 = accessory.DrawGuidance(new Vector3(100, 0, 81), Center, 0, 10000, $"AbsoluteAuthorityMovement2");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
    }

    [ScriptMethod(name: "Flare Movement Guidance", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(4186)$"],
        userControl: true)]
    public void AbsoluteAuthorityFlareGuide(Event @event, ScriptAccessory accessory)
    {
        var tid = @event.TargetId();
        if (tid != accessory.Data.Me) return;
        _isFlareTarget = true;
        var myIndex = accessory.GetMyIndex();
        
        List<Vector3> flarePos = Enumerable.Repeat(new Vector3(0, 0, 0), 4).ToList();
        flarePos[0] = new(81, 0, 81);
        flarePos[1] = flarePos[0].FoldPointHorizon(Center.X);
        flarePos[2] = flarePos[0].FoldPointVertical(90);
        flarePos[3] = flarePos[2].FoldPointHorizon(Center.X);
        var myPosIdx = myIndex % 4;
        
        var dp = accessory.DrawGuidance(Center, flarePos[myPosIdx], 0, 10000, $"AbsoluteAuthorityFlare");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Absolute Authority Movement Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4103[01])$"],
        userControl: false)]
    public void AbsoluteAuthorityGuideRemove(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"AbsoluteAuthority.*");
    }
    
    [ScriptMethod(name: "Alone (Heel) Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4103[01])$"],
        userControl: true)]
    public void AbsoluteAuthorityHeelGuidance(Event @event, ScriptAccessory accessory)
    {
        if (_drawn[1]) return;
        _drawn[1] = true;
        
        if (!_isFlareTarget) return;
        var myIndex = accessory.GetMyIndex();
        List<int> partnerIdx = [2, 3, 0, 1, 6, 7, 4, 5];
        var myPartnerIdx = partnerIdx[myIndex];
        var dp = accessory.DrawConnectionBetweenTargets(accessory.Data.Me, accessory.Data.PartyList[myPartnerIdx], 0,
            10000, $"Alone", 2f);
        dp.Color = accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        accessory.Method.TextInfo($"Stack with partner", 3000, true);
    }
    
    [ScriptMethod(name: "Alone (Heel) Hint Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4103[23])$"],
        userControl: false)]
    public void AbsoluteAuthorityHeelGuideRemove(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*");
    }

    #endregion Absolute Authority

    #region Ice Phase

    [ScriptMethod(name: "---- 《Laws of Ice》 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_LawsofIce(Event @event, ScriptAccessory accessory)
    {
    }
    
    [ScriptMethod(name: "Ice Phase Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41013"], userControl: false)]
    public void LawsofIcePhaseChange(Event @event, ScriptAccessory accessory)
    {
        if (_phase == QueenEternalPhase.Ice) return;
        _phase = QueenEternalPhase.Ice;
        accessory.DebugMsg($"Current phase: {_phase}", DebugMode);
        accessory.Method.TextInfo($"Keep moving!", 4000, true);
    }
    
    [ScriptMethod(name: "Movement Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41014"], userControl: true)]
    public void LawsofIcePhaseMoveHint(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo($"Keep moving!", 4000, true);
    }
    
    [ScriptMethod(name: "Ice Pillar Range", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(0039|0001)$"], userControl: true)]
    public void IceRushRange(Event @event, ScriptAccessory accessory)
    {
        var tid = @event.TargetId();
        var tidx = accessory.GetPlayerIdIndex(tid);
        if (_iceRushRangeDrawn[tidx]) return;
        
        var sid = @event.SourceId();
        var myIndex = accessory.GetMyIndex();

        lock (_earthPhaseTarget)
        {
            _iceRushRangeDrawn[tidx] = true;
            var count = _earthPhaseTarget.Count(x => x == true);
            var delay = count <= 4 ? 8000 : 4000;
            var destroy = 4000;
            var dp = accessory.DrawTarget2Target(sid, tid, 4, 80, delay, destroy, $"IceRushRange{tidx}");
            dp.Color = tidx == myIndex ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

    }
    
    private List<Vector3> _upIceBridgeRoute = [new(100, 0, 96), new(90, 0, 96), new(110, 0, 96)];
    private List<Vector3> _downIceBridgeRoute = [new(100, 0, 104), new(90, 0, 104), new(110, 0, 104)];
    [ScriptMethod(name: "Ice Pillar Tether Path", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(0039|0001)$"], userControl: true)]
    public void IceRushRoute(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Ice) return;
        var tid = @event.TargetId();
        var sid = @event.SourceId();
        if (tid != accessory.Data.Me) return;
        if (_sourceIceDartPos != new Vector3(0, 0, 0)) return;
        var spos = @event.SourcePosition();
        _sourceIceDartPos = spos;
        
        List<Vector3> targetPosList = Enumerable.Repeat(new Vector3(0, 0, 0), 4).ToList();
        
        // Determine if it's the first round
        var isFirstRound = _sourceIceDartPos.Z > 108;
        // 91 97 103 109 ==-90==> 1 7 13 19 == /6 ==> 0 1 2 3
        if (isFirstRound)
        {
            var iceIdx = Math.Floor((_sourceIceDartPos.X - 90) / 6);
            accessory.DebugMsg($"Tethered by the {iceIdx+1}st ice pillar from the bottom in the first round", DebugMode);
            List<Vector3> iceBaitPos = Enumerable.Repeat(new Vector3(0, 0, 0), 4).ToList();
            iceBaitPos[0] = new(108.5f, 0, 80.5f);
            iceBaitPos[1] = new(115.5f, 0, 80.5f);
            iceBaitPos[2] = iceBaitPos[1].FoldPointHorizon(CenterIce.X);
            iceBaitPos[3] = iceBaitPos[0].FoldPointHorizon(CenterIce.X);

            targetPosList = iceIdx switch
            {
                0 => [CenterIce, _upIceBridgeRoute[0], _upIceBridgeRoute[2], iceBaitPos[0]],
                1 => [CenterIce, _downIceBridgeRoute[0], _downIceBridgeRoute[2], iceBaitPos[1]],
                2 => [CenterIce, _downIceBridgeRoute[0], _downIceBridgeRoute[1], iceBaitPos[2]],
                _ => [CenterIce, _upIceBridgeRoute[0], _upIceBridgeRoute[1], iceBaitPos[3]],
            };
            
            _myIceBridgeIdx = (int)iceIdx;
        }
        else
        {
            var iceIdx = _sourceIceDartPos.Position2Dirs(CenterIce, 4, false);
            accessory.DebugMsg($"Tethered by the {iceIdx+1}st left/right ice pillar (starting North, clockwise) in the second round", DebugMode);
            List<Vector3> iceBaitPos = Enumerable.Repeat(new Vector3(0, 0, 0), 4).ToList();
            iceBaitPos[0] = new(84.5f, 0, 109.5f);
            iceBaitPos[1] = iceBaitPos[0].FoldPointVertical(CenterIce.Z);
            iceBaitPos[2] = iceBaitPos[1].FoldPointHorizon(CenterIce.X);
            iceBaitPos[3] = iceBaitPos[0].FoldPointHorizon(CenterIce.X);

            targetPosList = iceIdx switch
            {
                0 => [CenterIce, _downIceBridgeRoute[0], _downIceBridgeRoute[1], iceBaitPos[0]],
                1 => [CenterIce, _upIceBridgeRoute[0], _upIceBridgeRoute[1], iceBaitPos[1]],
                2 => [CenterIce, _upIceBridgeRoute[0], _upIceBridgeRoute[2], iceBaitPos[2]],
                _ => [CenterIce, _downIceBridgeRoute[0], _downIceBridgeRoute[2], iceBaitPos[3]],
            };

            var bridgeStr = iceIdx switch
            {
                0 => "Bottom-left",
                1 => "Top-left",
                2 => "Top-right",
                _ => "Bottom-right",
            };
            
            _myIceBridgeIdx = 10 + iceIdx;
            accessory.Method.TextInfo($"Observe the [{bridgeStr}] bridge, wait for the first round to cross", 4000, true);
        }
        
        var dp01 = accessory.DrawGuidance(targetPosList[0], targetPosList[1], 0, 15000, $"IcePath01");
        var dp12 = accessory.DrawGuidance(targetPosList[1], targetPosList[2], 0, 15000, $"IcePath12");
        var dp23 = accessory.DrawGuidance(targetPosList[2], targetPosList[3], 0, 15000, $"IcePath23");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp01);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp12);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp23);
    }

    [ScriptMethod(name: "Path Back to Center After Dodge", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4101[56])"], userControl: true)]
    public void AfterIceRushRoute(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Ice) return;
        if (_drawn[2]) return;
        _drawn[2] = true;
        accessory.Method.RemoveDraw($"IcePath.*");
        accessory.Method.RemoveDraw($"IceRushRange.*");
        
        List<Vector3> targetPosList = _myIceBridgeIdx switch
        {
            0 => [CenterIce, _upIceBridgeRoute[0], _upIceBridgeRoute[2]],
            1 => [CenterIce, _downIceBridgeRoute[0], _downIceBridgeRoute[2]],
            2 => [CenterIce, _downIceBridgeRoute[0], _downIceBridgeRoute[1]],
            3 => [CenterIce, _upIceBridgeRoute[0], _upIceBridgeRoute[1]],
            
            10 => [CenterIce, _downIceBridgeRoute[0], _downIceBridgeRoute[1]],
            11 => [CenterIce, _upIceBridgeRoute[0], _upIceBridgeRoute[1]],
            12 => [CenterIce, _upIceBridgeRoute[0], _upIceBridgeRoute[2]],
            13 => [CenterIce, _downIceBridgeRoute[0], _downIceBridgeRoute[2]],
        };
        
        var dp21 = accessory.DrawGuidance(targetPosList[2], targetPosList[1], 0, 15000, $"BackToCenterPath01");
        var dp10 = accessory.DrawGuidance(targetPosList[1], targetPosList[0], 0, 15000, $"BackToCenterPath12");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp21);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp10);

        if (_myIceBridgeIdx >= 10)
            accessory.Method.TextInfo($"Cross [First] then back to center", 4000, false);
        else
            accessory.Method.TextInfo($"Cross [Last] then back to center", 4000, true);
    }
    
    [ScriptMethod(name: "Back to Center Path Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4099[02])"], userControl: false)]
    public void AfterIceRushRouteRemove(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Ice) return;
        accessory.Method.RemoveDraw($"BackToCenterPath.*");
    }
    
    private bool _raisedTribute;
    [ScriptMethod(name: "Tether Stack", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(020C)"], userControl: true)]
    public void RaisedTributeRoute(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Ice) return;
        if (_raisedTributeNum != 0) return;
        
        _raisedTribute = true;
        
        var myIndex = accessory.GetMyIndex();
        if (myIndex != 4 && myIndex != 5) return;
        var topRightCorner = new Vector3(115.5f, 0, 80.5f);
        var topLeftCorner = topRightCorner.FoldPointHorizon(CenterIce.X);
        List<Vector3> targetPosList = myIndex switch
        {
            4 => [CenterIce, _upIceBridgeRoute[0], _upIceBridgeRoute[1], topLeftCorner],
            5 => [CenterIce, _upIceBridgeRoute[0], _upIceBridgeRoute[2], topRightCorner],
        };
        
        var dp01 = accessory.DrawGuidance(targetPosList[0], targetPosList[1], 0, 15000, $"D_TetherPath01");
        var dp12 = accessory.DrawGuidance(targetPosList[1], targetPosList[2], 0, 15000, $"D_TetherPath12");
        var dp23 = accessory.DrawGuidance(targetPosList[2], targetPosList[3], 0, 15000, $"D_TetherPath23");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp01);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp12);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp23);
    }
    
    [ScriptMethod(name: "Tether Stack Count Increase", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(41024)", "TargetIndex:1"], userControl: false)]
    public void RaisedTributeNumPlus(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Ice) return;
        _raisedTributeNum++;
    }
    
    [ScriptMethod(name: "Tether Stack Second Round", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(020C)"], userControl: false)]
    public void RaisedTributeRouteSecond(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Ice) return;
        if (_raisedTributeNum != 1) return;
        var myIndex = accessory.GetMyIndex();
        
        _raisedTribute = false;
        accessory.Method.RemoveDraw($"D_Tether.*");
        
        var topRightCorner = new Vector3(115.5f, 0, 80.5f);
        var topLeftCorner = topRightCorner.FoldPointHorizon(CenterIce.X);
        
        List<int> idxList = [0, 1, 4, 5];
        if (!idxList.Contains(myIndex)) return;
        
        List<Vector3> targetPosList = myIndex switch
        {
            0 => [CenterIce, _upIceBridgeRoute[0], _upIceBridgeRoute[1], topLeftCorner],
            1 => [CenterIce, _upIceBridgeRoute[0], _upIceBridgeRoute[2], topRightCorner],
            
            4 => [CenterIce, _downIceBridgeRoute[0], _downIceBridgeRoute[1], topLeftCorner],
            5 => [CenterIce, _downIceBridgeRoute[0], _downIceBridgeRoute[2], topRightCorner],
        };

        if (myIndex <= 1)
        {
            var dp01 = accessory.DrawGuidance(targetPosList[0], targetPosList[1], 0, 15000, $"T_TetherPath01");
            var dp12 = accessory.DrawGuidance(targetPosList[1], targetPosList[2], 0, 15000, $"T_TetherPath12");
            var dp23 = accessory.DrawGuidance(targetPosList[2], targetPosList[3], 0, 15000, $"T_TetherPath23");
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp01);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp12);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp23);
        }
        else
        {
            var dp32 = accessory.DrawGuidance(targetPosList[3], targetPosList[2], 0, 15000, $"D_ReturnPath32");
            var dp21 = accessory.DrawGuidance(targetPosList[2], targetPosList[1], 0, 15000, $"D_ReturnPath21");
            var dp10 = accessory.DrawGuidance(targetPosList[1], targetPosList[0], 0, 15000, $"D_ReturnPath10");
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp32);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp21);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp10);
        }
    }
    
    [ScriptMethod(name: "Tether Stack Remove", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(020C)"], userControl: false)]
    public void RaisedTributeRouteRemove(Event @event, ScriptAccessory accessory)
    {
        if (_phase != QueenEternalPhase.Ice) return;
        if (_raisedTributeNum <= 2) return;
        accessory.Method.RemoveDraw($".*");
    }
    
    #endregion Ice Phase

    #region Radical Shift (Enrage)

    [ScriptMethod(name: "---- 《Soft Enrage》 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
        userControl: true)]
    public void SplitLine_Enrage(Event @event, ScriptAccessory accessory)
    {
    }
    
    [ScriptMethod(name: "Fixed Safe Spot", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(41039)"], userControl: true)]
    public void RadicalShift(Event @event, ScriptAccessory accessory)
    {
        if (_phase == QueenEternalPhase.Ice)
            _phase = QueenEternalPhase.Enrage;
        var myIndex = accessory.GetMyIndex();
        List<Vector3> safePos = Enumerable.Repeat(new Vector3(0, 0, 0), 8).ToList();
        safePos[0] = new Vector3(91.3f, 0f, 86.6f);
        safePos[1] = safePos[0].FoldPointHorizon(Center.X);
        safePos[2] = new Vector3(95.5f, 0f, 94.6f);
        safePos[3] = safePos[2].FoldPointHorizon(Center.X);
        safePos[4] = new Vector3(91.45f, 0f, 98f);
        safePos[5] = safePos[4].FoldPointHorizon(Center.X);
        safePos[6] = new Vector3(88.35f, 0f, 101.65f);
        safePos[7] = safePos[6].FoldPointHorizon(Center.X);
        
        var dp = accessory.DrawGuidance(safePos[myIndex], 0, 15000, $"FixedSafeSpot{myIndex}");
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Fixed Safe Spot Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(41039)"], userControl: false)]
    public void RadicalShiftRemove(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }
    
    private bool _exaflareShown = false;
    [ScriptMethod(name: "Exaflare Range", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(41043)"], userControl: true)]
    public void Exaflare(Event @event, ScriptAccessory accessory)
    {
        _exaflareShown = true;
        var sid = @event.SourceId();
        var dp = accessory.DrawCircle(sid, 6, 0, 5000, $"Exaflare{sid}", true);
        dp.Color = exflareColor.V4.WithW(2f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        var dpWarn = accessory.DrawCircle(sid, 6, 0, 6100, $"ExaflareWarning{sid}");
        dpWarn.Offset = new Vector3(0, 0, -8.5f);
        dpWarn.Color = exflareWarnColor.V4;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dpWarn);
    }
    
    
    [ScriptMethod(name: "Exaflare Subsequent", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4104[34])"], userControl: false)]
    public void ExaflareRest(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        var spos = @event.SourcePosition();
        if (!_exaflareShown) return;
        
        var dp = accessory.DrawCircle(sid, 6, 0, 1100, $"ExaflareWarning{sid}");
        dp.Offset = new Vector3(0, 0, -8.5f);
        dp.Color = exflareColor.V4.WithW(2f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        var dpWarn = accessory.DrawCircle(sid, 6, 0, 2200, $"ExaflareWarning{sid}");
        dpWarn.Offset = new Vector3(0, 0, -17f);
        dpWarn.Color = exflareWarnColor.V4;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dpWarn);
    }

    #endregion Radical Shift
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
    
    public static uint DataId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["DataId"]);
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
    public static IGameObject? GetById(this ScriptAccessory sa, ulong gameObjectId)
    {
        return sa.Data.Objects.SearchById(gameObjectId);
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
    /// <param name="newPoint">Outer point</param>
    /// <param name="center">Center</param>
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
}

public static class IndexHelper
{
    /// <summary>
    /// Input player dataId, get the corresponding position index
    /// </summary>
    /// <param name="pid">Player SourceId</param>
    /// <param name="accessory"></param>
    /// <returns>The position index corresponding to the player</returns>
    public static int GetPlayerIdIndex(this ScriptAccessory accessory, uint pid)
    {
        // Get player IDX
        return accessory.Data.PartyList.IndexOf(pid);
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
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static DrawPropertiesEdit DrawGuidance(this ScriptAccessory accessory, 
        object ownerObj, object targetObj, int delay, int destroy, string name, float rotation = 0, float scale = 1f)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(scale);
        dp.Rotation = rotation;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultSafeColor;
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
        object targetObj, int delay, int destroy, string name, float rotation = 0, float scale = 1f)
    {
        return targetObj switch
        {
            uint uintTarget => accessory.DrawGuidance(accessory.Data.Me, uintTarget, delay, destroy, name, rotation, scale),
            Vector3 vectorTarget => accessory.DrawGuidance(accessory.Data.Me, vectorTarget, delay, destroy, name, rotation, scale),
            _ => throw new ArgumentException("targetObj must be of type uint or Vector3")
        };
    }
    
    /// <summary>
    /// Return left/right cleave fan
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
    /// Return front/back cleave fan
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
    public static DrawPropertiesEdit DrawTargetNearFarOrder(this ScriptAccessory accessory, uint ownerId, uint orderIdx,
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
    public static DrawPropertiesEdit DrawOwnersEntityOrder(this ScriptAccessory accessory, uint ownerId, uint orderIdx, float width, float length, int delay, int destroy, string name, bool lengthByDistance = false, bool byTime = false)
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
    
    public static DrawPropertiesEdit DrawFanToTarget(this ScriptAccessory accessory, uint sourceId, uint targetId, float radian, float scale, int delay, int destroy, string name, Vector4 color, float rotation = 0, bool lengthByDistance = false, bool byTime = false)
    {
        var dp = accessory.DrawTarget2Target(sourceId, targetId, scale, scale, delay, destroy, name, rotation, lengthByDistance, byTime);
        dp.Radian = radian;
        dp.Color = color;
        return dp;
    }

    /// <summary>
    /// Return connection line dp between owner and target, using Line drawing type
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
    public static DrawPropertiesEdit DrawCircle(this ScriptAccessory accessory, uint ownerId, float scale, int delay, int destroy, string name, bool byTime = false)
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
            case Vector3 spos:
                dp.Position = spos;
                break;
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
    public static DrawPropertiesEdit DrawStaticCircle(this ScriptAccessory accessory, Vector3 center, Vector4 color, int delay, int destroy, string name, float scale = 1.5f)
    {
        var dp = accessory.DrawStatic(center, (uint)0, 0, 0, scale, scale, color, delay, destroy, name);
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
        var dp = accessory.DrawStatic(center, (uint)0, float.Pi * 2, 0, scale, scale, color, delay, destroy, name);
        dp.InnerScale = innerscale != 0f ? new Vector2(innerscale) : new Vector2(scale - 0.05f);
        return dp;
    }

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
    /// Return knockback drawing
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
            // Decide whether to use TargetObject or TargetPosition based on the type of tid
            case uint tid:
                dp.TargetObject = tid; // If tid is uint
                break;
            case Vector3 tpos:
                dp.TargetPosition = tpos; // If tid is Vector3
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
    
    public static DrawPropertiesEdit DrawKnockBack(this ScriptAccessory accessory, object target, float length, int delay, int destroy, string name, float width = 1.5f, bool byTime = false)
    {
        return target switch
        {
            uint uintTarget => accessory.DrawKnockBack(accessory.Data.Me, uintTarget, length, delay, destroy, name, width, byTime),
            Vector3 vectorTarget => accessory.DrawKnockBack(accessory.Data.Me, vectorTarget, length, delay, destroy, name, width, byTime),
            _ => throw new ArgumentException("target must be of type uint or Vector3")
        };
    }
    
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
            // Decide whether to use TargetObject or TargetPosition based on the type of tid
            case uint tid:
                dp.TargetObject = tid; // If tid is uint
                break;
            case Vector3 tpos:
                dp.TargetPosition = tpos; // If tid is Vector3
                break;
            default:
                throw new ArgumentException("Invalid target type for DrawSightAvoid");
        }
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        return dp;
    }
    
    public static DrawPropertiesEdit DrawSightAvoid(this ScriptAccessory accessory, object target, int delay, int destroy, string name)
    {
        return target switch
        {
            uint uintTarget => accessory.DrawSightAvoid(accessory.Data.Me, uintTarget, delay, destroy, name),
            Vector3 vectorTarget => accessory.DrawSightAvoid(accessory.Data.Me, vectorTarget, delay, destroy, name),
            _ => throw new ArgumentException("target must be of type uint or Vector3")
        };
    }

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
                    var dp = accessory.DrawRect(sid, width, length, delay, destroy, $"{name}{i}");
                    dp.Rotation = extendDirs[i];
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
    
    /// <summary>
    /// External debug mode
    /// </summary>
    /// <param name="str"></param>
    /// <param name="debugMode"></param>
    /// <param name="accessory"></param>
    public static void DebugMsg(this ScriptAccessory accessory, string str, bool debugMode = true)
    {
        if (!debugMode)
            return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }
    
    public static string BuildListStr<T>(this ScriptAccessory accessory, List<T> myList)
    {
        return string.Join(", ", myList.Select(item => item?.ToString() ?? ""));
    }
}

public enum QueenEternalPhase : uint
{
    Init,
    Wind,
    Earth,
    Ice,
    Enrage
}

#endregion
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

namespace UsamisKodakku.Scripts._06_EndWalker.TopReborn;

[ScriptType(name: Name, territorys: [1122], guid: "7c5f04b6-69ba-4a77-9ac7-e3b186ece8e1",
    version: Version, author: "Linoa235", note: NoteStr, updateInfo: UpdateInfo)]

// ^(?!.*((Monk|Machinist|Dragoon|Samurai|Ninja|Viper|Reaper|Dancer|Bard|Astrologian|Sage|Scholar|(Eos|Selene)|Seraph|White Mage|Warrior|Paladin|Dark Knight|Gunbreaker|Pictomancer|Black Mage|Blue Mage|Summoner|Carbuncle|Demigod Bahamut|Demigod Phoenix|Garuda-Egi|Titan-Egi|Ifrit-Egi|Puppet)) (Used|Cast))).*35501.*$
// ^\[\w+\|[^|]+\|E\]\s\w+

public class TopReborn
{
    const string NoteStr =
        $"""
        v{Version}
        Modified from Karlin/Meva's script,
        Use special features with caution.
        """;
    
    const string UpdateInfo =
        $"""
         {Version}
         1. Fixed an issue where the automatic facing assistance for the small screen (TV) and P5 mechanic 1 continued to work even when the script-wide option "Enable *-marked special features in method settings" was disabled.
         """;

    private const string Name = "TOP Deluxe Reborn";
    private const string Version = "0.0.0.17";
    private const string DebugVersion = "a";

    private const bool Debugging = false;

    private static readonly List<string> Role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
    private static readonly Vector3 Center = new Vector3(100, 0, 100);
    
    private static PriorityDict _pd = new PriorityDict();       // Versatile dictionary
    private double _parse = 0;

    private static P1StateParams _p1 = new();
    private static P2StateParams _p2 = new();
    private static P3StateParams _p3 = new();
    private static P4StateParams _p4 = new();
    private static P5AStateParams _p5A = new();
    private static P5BStateParams _p5B = new();
    private static P5CStateParams _p5C = new();
    private static P6StateParams _p6 = new();
    
    [UserSetting("Enable *-marked special features in method settings")]
    public bool SpecialMode { get; set; } = false;

    public void Init(ScriptAccessory sa)
    {
        RefreshParams(sa);
        sa.DebugMsg($"Script {Name} v{Version}{DebugVersion} initialized", Debugging);
        sa.Method.RemoveDraw(".*");
        sa.Method.ClearFrameworkUpdateAction(this);
    }
    
    private void RefreshParams(ScriptAccessory sa)
    {
        _pd = new PriorityDict();
        _parse = 0;
        ResetSupportUnitVisibility(sa);

        _p1.Reset(sa);
        _p1.Dispose();
        _p2.Reset(sa);
        _p2.Dispose();
        _p3.Reset(sa);
        _p3.Dispose();
        _p4.Reset(sa);
        _p4.Dispose();
        _p5A.Reset(sa);
        _p5A.Dispose();
        _p5B.Reset(sa);
        _p5B.Dispose();
        _p5C.Reset(sa);
        _p5C.Dispose();
        _p6.Reset(sa);
        _p6.Dispose();
    }

    private void ResetSupportUnitVisibility(ScriptAccessory sa)
    {
        const uint SUPPORTER_DATAID = 9020;
        var objEnums = sa.GetByDataId(SUPPORTER_DATAID);
        unsafe
        {
            foreach (var obj in objEnums)
            {
                sa.WriteVisible(obj, true);
            }
        }
    }
    
    [ScriptMethod(name: "Test: Parameter Reset", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void ParameterReset(Event ev, ScriptAccessory sa)
    {
        RefreshParams(sa);
    }
    
    [ScriptMethod(name: "Test: Show Priority Table", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void ShowPriorityTable(Event ev, ScriptAccessory sa)
    {
        sa.DebugMsg(_pd.ShowPriorities(), Debugging);
    }
    
    [ScriptMethod(name: "Test: Temporary Test", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void TemporaryTest(Event ev, ScriptAccessory sa)
    {
        var rot = 119.74815f.DegToRad();
        var startRegion = rot.RadianToRegion(12, 8, isDiagDiv: true);
        var safeRegion = (startRegion + 3) % 12;     // a certain safe zone angle
        sa.DebugMsg($"Cannon region: {startRegion}, a certain safe zone region: {safeRegion}", Debugging);
    }
    
    #region P1A Loop Program

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP1A Loop Programã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P1A_LoopProgram_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P1A_LoopProgram_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31491"], userControl: Debugging)]
    public void P1A_LoopProgram_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        _parse = 1.1;
        sa.DebugMsg($"Current phase: {_parse}", Debugging);
        _p1.BossId = ev.TargetId;
        _p1.Register();
        _pd.Init(sa, "P1 Line Towers");
        _pd.AddPriorities([2, 3, 1, 8, 4, 5, 6, 7]);    // Lower value = higher priority
    }
    
    [ScriptMethod(name: "P1A_LoopProgram_BuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3004|3005|3006|3451)$"], userControl: Debugging)]
    public void P1A_LoopProgram_BuffRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.1) return;
        if (ev.SourceId == 0x00000000) return;
        var idx = sa.GetPlayerIdIndex((uint)ev.TargetId);
        var priVal = ev.StatusId switch
        {
            3004 => 10,
            3005 => 20,
            3006 => 30,
            3451 => 40,
            _ => 0
        };
        _pd.AddPriority(idx, priVal);
    }
    
    [ScriptMethod(name: "P1A_LoopProgram_TowerCollection", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Add", "DataId:2013245"], userControl: Debugging)]
    public void P1A_LoopProgram_TowerCollection(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.1) return;
        lock (_p1.TowerDictionary)
        {
            // sa.DebugMsg($"Ready for tower collection R{_p1.LineTowerRound} {_p1.PreviousDrawClearCompleted.WaitOne(0)}", Debugging);
            
            if (_p1.LineTowerRound != 0)
                _p1.PreviousDrawClearCompleted.WaitOne();
            
            var towerPos = ev.SourcePosition;
            var towerPriority = towerPos.GetRadian(Center).RadianToRegion(4, baseRegionIdx: 2, isDiagDiv: true, isCw: true);   // North as 0, increasing clockwise
            _p1.TowerDictionary[towerPriority] = towerPos;
            // sa.DebugMsg($"Collected tower at direction {towerPriority} in round {_p1.LineTowerRound}", Debugging);
            if (_p1.TowerDictionary.Count != 2) return;
            _p1.LineTowerRound++;
            sa.DebugMsg($"Line tower round increased to {_p1.LineTowerRound}", Debugging);
            _p1.EachRoundTowerCompleted.Set();
            _p1.PreviousDrawClearCompleted.Reset();
        }
    }
    
    [ScriptMethod(name: "P1A_LoopProgram_GatherReminder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31491"], userControl: true)]
    public void P1A_LoopProgram_GatherReminder(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.1) return;
        sa.Method.TextInfo("Gather behind the boss", 2000);
        sa.Method.TTS("Gather behind the boss");
    }
    
    [ScriptMethod(name: "P1A_LoopProgram_StartPositionReminder", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3004|3005|3006|3451)$"])]
    public void P1A_LoopProgram_StartPositionReminder(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.1) return;
        if (ev.TargetId != sa.Data.Me) return;

        var isFirstTether = ev.StatusId == 3006;
        sa.Method.TextInfo(isFirstTether ? "Front for tether" : "Back", 3000);
        sa.Method.TTS(isFirstTether ? "Front for tether" : "Back");
    }

    [ScriptMethod(name: "P1A_LoopProgram_ClearDrawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3149[67])$"], suppress: 100, userControl: Debugging)]
    public void P1A_LoopProgram_ClearDrawing(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.1) return;
        
        sa.DebugMsg($"Clear loop program drawing Round {_p1.LineTowerRound}", Debugging);
        
        sa.Method.RemoveDraw($"P1_LoopProgram_R{_p1.LineTowerRound}.*");
        _p1.TowerDictionary = new Dictionary<int, Vector3>();
        _p1.LastProximityStatus = 0;
        sa.Method.UnregistFrameworkUpdateAction(_p1.IdleGuidanceFramework);
        _p1.PreviousDrawClearCompleted.Set();
        
        if (_p1.LineTowerRound < 4) return;
        sa.Method.UnregistFrameworkUpdateAction(_p1.TetherScanFramework);
    }
    
    [ScriptMethod(name: "P1A_LoopProgram_LineTowerPosition", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Add", "DataId:2013245"], suppress: 500)]
    public void P1A_LoopProgram_LineTowerPosition(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.1) return;
        _p1.EachRoundTowerCompleted.WaitOne();
        int  myIndex      = sa.GetMyIndex();
        int  myPriVal     = _pd.Priorities[myIndex];
        int  myTowerRound = myPriVal / 10;
        int  myLineRound  = (myTowerRound - 1 + 2) % 4 + 1;
        int  myPriority   = _pd.FindPriorityIndexOfKey(myIndex);   // Ascending order, starting from 0, even = high priority, odd = low priority
        bool isHighPrior  = myPriority % 2 == 0;

        try
        {
            if (_p1.LineTowerRound == myTowerRound)
            {
                var myTower = isHighPrior ? _p1.TowerDictionary.MinBy(kvp => kvp.Key) : _p1.TowerDictionary.MaxBy(kvp => kvp.Key);
                // sa.DebugMsg($"Current round {_p1.LineTowerRound} is player's tower round, player needs to stand at tower direction {myTower.Key} (North as 0, increasing clockwise)", Debugging);
                sa.DrawGuidance(myTower.Value, 0, 9000, $"P1_LoopProgram_R{_p1.LineTowerRound}_TowerPosition");
                sa.DrawCircle(myTower.Value, 0, 9000, $"P1_LoopProgram_R{_p1.LineTowerRound}_TowerRange", 3f, isSafe: true);
            }
            else if (_p1.LineTowerRound == myLineRound)
            {
                var myLineRegion = isHighPrior ? 0 : 3;
                while (_p1.TowerDictionary.ContainsKey(myLineRegion))
                {
                    myLineRegion += isHighPrior ? 1 : -1;
                    if (myLineRegion is < 4 and > -1) continue;
                    sa.Log.Error($"Could not find safe zone for player's tether");
                    return;
                }
                var myLinePos = new Vector3(100, 0, 85).RotateAndExtend(Center, myLineRegion * -90f.DegToRad());
                // sa.DebugMsg($"Current round {_p1.LineTowerRound} is player's line round, player needs to tether to direction {myLineRegion} (North as 0, increasing clockwise)", Debugging);
                sa.DrawGuidance(myLinePos, 0, 9000, $"P1_LoopProgram_R{_p1.LineTowerRound}_LinePosition");
            }
            else
            {
                _p1.IdleGuidanceFramework = sa.Method.RegistFrameworkUpdateAction(Action);
                // sa.DebugMsg($"Current round {_p1.LineTowerRound}, player is idle, stay near the towers (North as 0, increasing clockwise)", Debugging);

                if (_p1.LineTowerRound < myTowerRound)
                {
                    sa.DrawCircle(_p1.TowerDictionary.First().Value, 0, 9000, $"P1_LoopProgram_R{_p1.LineTowerRound}_Tower1DangerRange", 3f, isSafe: false);
                    sa.DrawCircle(_p1.TowerDictionary.Last().Value, 0, 9000, $"P1_LoopProgram_R{_p1.LineTowerRound}_Tower2DangerRange", 3f, isSafe: false);
                }
                
                void Action()
                {
                    Vector3  myPos           = sa.Data.MyObject.Position;
                    Vector3  tower1Safe      = new Vector3(100, 0, 86).RotateAndExtend(Center, _p1.TowerDictionary.First().Key * -90f.DegToRad());
                    Vector3  tower2Safe      = new Vector3(100, 0, 86).RotateAndExtend(Center, _p1.TowerDictionary.Last().Key * -90f.DegToRad());
                    float    distanceTower1  = Vector3.Distance(myPos, tower1Safe);
                    float    distanceTower2  = Vector3.Distance(myPos, tower2Safe);
                    int      currentStatus   = distanceTower1 < distanceTower2 ? 1 : 2;
    
                    // Update drawing if status changes
                    if (currentStatus == _p1.LastProximityStatus) return;
            
                    // Remove old drawing
                    sa.Method.RemoveDraw($"P1_LoopProgram_R{_p1.LineTowerRound}_IdlePosition{_p1.LastProximityStatus}");
                    sa.DrawGuidance(currentStatus == 1 ? tower1Safe : tower2Safe, 0, Int32.MaxValue, $"P1_LoopProgram_R{_p1.LineTowerRound}_IdlePosition{currentStatus}");
                    _p1.LastProximityStatus = currentStatus;
                }
            }
        }
        finally
        {
            _p1.EachRoundTowerCompleted.Reset();
        }
    }

    [ScriptMethod(name: "P1A_LoopProgram_TetherMark", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(31496)$", "TargetIndex:1"], suppress: 500)]
    public void P1A_LoopProgram_TetherMark(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.1) return;
        _p1.EachRoundTowerCompleted.WaitOne(1000);
        
        int  myIndex      = sa.GetMyIndex();
        int  myPriVal     = _pd.Priorities[myIndex];
        int  myTowerRound = myPriVal / 10;
        int  myLineRound  = (myTowerRound - 1 + 2) % 4 + 1;

        if (_p1.LineTowerRound != myLineRound) return;
        
        int  myPriority     = _pd.FindPriorityIndexOfKey(myIndex);  // Ascending order, starting from 0, even = high priority, odd = low priority
        int  targetPriority = (myPriority - 2 + 8) % 8;             // Priority index of the player this player needs to tether to
        var  targetPartyIndex = _pd.SelectSpecificPriorityIndex(targetPriority).Key;

        var dp = sa.DrawLine(ev.TargetId, sa.Data.PartyList[targetPartyIndex], 0, 9000,
            $"P1_LoopProgram_R{_p1.LineTowerRound}_TetherMark", 0, 5, 10, byY: true, isSafe: true, draw: false);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
        
        // sa.DebugMsg($"Player priority ascending order index is {myPriority}, needs to tether from sequence {targetPriority} {sa.GetPlayerJobByIndex(targetPartyIndex)}", Debugging);
    }
    
    [ScriptMethod(name: "P1A_LoopProgram_TetherMarkRemoval", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0059"], userControl: Debugging)]
    public void P1A_LoopProgram_TetherMarkRemoval(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.1) return;
        if (ev.SourceId != sa.Data.Me) return;
        sa.Method.RemoveDraw($"P1_LoopProgram_R{_p1.LineTowerRound}_TetherMark");
    }
    
    [ScriptMethod(name: "P1A_LoopProgram_TetherPlayerLargeCircleDrawing", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0059"], userControl: true)]
    public void P1A_LoopProgram_TetherPlayerLargeCircleDrawing(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.1) return;

        if (_p1.TetherScanActive) return;
        _p1.TetherScanActive = true;
        
        // Draw large circle only when the correct player in the correct round tethers and is at a certain distance from the boss
        _p1.TetherScanFramework = sa.Method.RegistFrameworkUpdateAction(Action);
        
        const uint TETHER_ID = 0x59;
        return;
        
        void Action()
        {
            var bossObj = sa.GetById(_p1.BossId);
            if (bossObj is null) return;
            
            foreach (var member in sa.Data.PartyList)
            {
                // Find party member
                IGameObject? memberObj = sa.GetById(member);
                if (memberObj is null) continue;
                int memberIdx = sa.GetPlayerIdIndex(member);
                
                void CleanUp()
                {
                    _p1.TetherDrawingDictionary.Remove(memberIdx, out _);
                    sa.Method.RemoveDraw($"P1_LoopProgram_R{_p1.LineTowerRound}_TetherSource{memberIdx}");
                }

                if (!_pd.Priorities.TryGetValue(memberIdx, out int memberPrival) ||
                    memberPrival < 10) { CleanUp(); continue; }    // Priority not ready
                
                // Calculate line tower round
                int memberTowerRound = memberPrival / 10;
                int memberLineRound  = (memberTowerRound - 1 + 2) % 4 + 1;
                if (memberLineRound != _p1.LineTowerRound) { CleanUp(); continue; }

                // Distance check
                float distance = Vector3.Distance(memberObj.Position, bossObj.Position);
                if (distance < 5f) { CleanUp(); continue; }
                
                // Tether source check
                var tetherSource = sa.GetTetherSource((IBattleChara?)memberObj, TETHER_ID);
                bool isCorrectTether = tetherSource.Count == 1 && tetherSource[0] == _p1.BossId;
                if (!isCorrectTether) { CleanUp(); continue; }

                // Avoid redrawing
                if (_p1.TetherDrawingDictionary.TryAdd(memberIdx, true))
                    sa.DrawCircle(member, 0, Int32.MaxValue, $"P1_LoopProgram_R{_p1.LineTowerRound}_TetherSource{memberIdx}", 15);

            }
        }
    }
    
    #endregion P1A Loop Program

    #region P1B Omniscient

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP1B Omniscientã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P1B_Omniscient_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P1B_Omniscient_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31499"], userControl: Debugging)]
    public void P1B_Omniscient_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        _parse = 1.2;
        _pd.Init(sa, "P1 Omniscient");
        _pd.AddPriorities([2, 3, 1, 8, 4, 5, 6, 7]);    // Lower value = higher priority
        _p1.OmniscientRound = 1;
    }
    
    [ScriptMethod(name: "P1B_Omniscient_BuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3004|3005|3006|3451)$"], userControl: Debugging)]
    public void P1B_Omniscient_BuffRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.2) return;
        if (ev.SourceId == 0x00000000) return;
        var idx = sa.GetPlayerIdIndex((uint)ev.TargetId);
        var priVal = ev.StatusId switch
        {
            3004 => 10,
            3005 => 20,
            3006 => 30,
            3451 => 40,
            _ => 0
        };
        _pd.AddPriority(idx, priVal); 
    }
    
    [ScriptMethod(name: "P1B_Omniscient_CWCCWRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(31501|32368)$"], userControl: Debugging)]
    public void P1B_Omniscient_CWCCWRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.2) return;
        if (_p1.OmniscientDirectionDetermined) return;

        const uint FIRST_FLAME = 31501;
        const uint REST_FLAME = 32368;
        
        lock (_p1)
        {
            if (ev.ActionId == FIRST_FLAME && _p1.OmniscientFirstAngleStorage < -8)
            {
                _p1.OmniscientFirstAngleStorage = ev.SourceRotation;
                sa.DebugMsg($"Stored Omniscient first angle {_p1.OmniscientFirstAngleStorage.RadToDeg()}", Debugging);
                _p1.OmniscientFirstAngleStoredEvent.Set();
            }
                
            if (ev.ActionId == REST_FLAME)
            {
                float diff = ev.SourceRotation.GetDiffRad(_p1.OmniscientFirstAngleStorage);
                sa.DebugMsg($"Difference between current angle {ev.SourceRotation.RadToDeg()} and previous is {diff.RadToDeg()}", Debugging);
                if (MathF.Abs(diff) > float.Pi / 2) return;
                _p1.OmniscientIsClockwise = diff < 0;
                _p1.OmniscientDirectionDetermined = true;
                sa.DebugMsg($"Omniscient direction determined: {(diff < 0 ? "Clockwise" : "Counter-clockwise")}", Debugging);
                _p1.OmniscientDirectionDeterminedEvent.Set();
            }
        }
    }
    
    [ScriptMethod(name: "P1B_Omniscient_StartingLine", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(31501)$"], userControl: true)]
    public void P1B_Omniscient_StartingLine(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.2) return;
        if (_p1.OmniscientStartingLineDrawn) return;
        _p1.OmniscientStartingLineDrawn = true;
        
        int  myIndex      = sa.GetMyIndex();
        int  myPriority   = _pd.FindPriorityIndexOfKey(myIndex);   // Ascending order, starting from 0, even = high priority, odd = low priority
        bool isHighPrior  = myPriority % 2 == 0;
        
        _p1.OmniscientFirstAngleStoredEvent.WaitOne();
        var startRegion = _p1.OmniscientFirstAngleStorage.RadianToRegion(12, 8, isDiagDiv: true);
        startRegion = (startRegion + 3) % 12;     // a certain safe zone angle
        sa.DebugMsg($"Certain safe zone angle is: {startRegion}, I am high priority: {isHighPrior}, I am rank {myPriority}", Debugging);
        var isNotMyRegion = startRegion < 6 ^ isHighPrior;
        if (isNotMyRegion)
            startRegion = (startRegion + 6) % 12;
        sa.DebugMsg($"Final determined safe zone angle: {startRegion}", Debugging);
        var bossObj = sa.GetById(_p1.BossId);
        if (bossObj is null) return;
        var bossPos = bossObj.Position;
        
        var startRad = ((startRegion + 12 - 8) % 12 * 30f).DegToRad();
        // Draw a line from the boss itself to tempRad
        var dp = sa.DrawLine(bossPos, 0, 0, 6000, $"P1_Omniscient_StartingLine", startRad, 1f, 20f, true, draw: false);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);
        
        _p1.OmniscientDirectionDeterminedEvent.WaitOne();
        var routeRad = startRad + (_p1.OmniscientIsClockwise ? -float.Pi / 2 : float.Pi / 2);
        // Draw CW/CCW indicators
        for (int i = 0; i < 4; i++)
        {
            var basePoint = bossPos + new Vector3(0, 0, 1);
            var startPoint = basePoint.RotateAndExtend(bossPos, startRad, (i + 1) * 4.5f);
            var dp0 = sa.DrawLine(startPoint, 0, 0, 6000, $"P1_Omniscient_StartPointer", routeRad, 1f, 2f, true, draw: false);
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp0);
        }
        
        _p1.OmniscientFirstAngleStoredEvent.Reset();
        _p1.OmniscientDirectionDeterminedEvent.Reset();
    }
    
    [ScriptMethod(name: "P1B_Omniscient_RoundIncrement", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31502"], userControl: Debugging, suppress: 500)]
    public void P1B_Omniscient_RoundIncrement(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.2) return;
        sa.Method.RemoveDraw($"P1_Omniscient_R{_p1.OmniscientRound}.*");
        _p1.OmniscientRound++;
        if (_p1.OmniscientRound > 4) return;
        sa.DebugMsg($"Now Omniscient Round {_p1.OmniscientRound}", Debugging);
    }
    
    [ScriptMethod(name: "P1B_Omniscient_GoOutReminder", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(349[567]|3424)$"])]
    public void P1B_Omniscient_GoOutReminder(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.2) return;
        
        if (ev.TargetId != sa.Data.Me) return;
        _p1.OmniscientGoOutTimerFramework = sa.Method.RegistFrameworkUpdateAction(Action);
        return;
        
        void Action()
        {
            var statusId = ev.StatusId;
            var dur = sa.GetStatusRemainingTime(sa.Data.MyObject, statusId);
            if (dur > 5f) return;
            sa.Method.TextInfo("Go out, go out", 2000);
            sa.Method.TTS("Go out, go out");
            sa.Method.UnregistFrameworkUpdateAction(_p1.OmniscientGoOutTimerFramework);
        }
    }
    
    [ScriptMethod(name: "P1B_Omniscient_TurnBackReminder", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31502", "TargetIndex:1"])]
    public void P1B_Omniscient_TurnBackReminder(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.2) return;
        if (ev.TargetId != sa.Data.Me) return;
        sa.Method.TextInfo("Turn back", 2000);
        sa.Method.TTS("Turn back");
    }
    
    [ScriptMethod(name: "P1B_Omniscient_MarkedLine", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"])]
    public void P1B_Omniscient_MarkedLine(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.2) return;
        if (sa.Data.MyObject.IsTank()) return;  // Tanks can ignore
        bool isMe = ev.TargetId == sa.Data.Me;
        sa.DrawRect(_p1.BossId, ev.TargetId, 0, 5000, $"P1_Omniscient_MarkedLine", 0, 6, 50, isMe);
    }
    
    [ScriptMethod(name: "P1B_Omniscient_LatterHalfGuidance", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"], suppress: 15000)]
    public void P1B_Omniscient_LatterHalfGuidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.2) return;
        var bossObj = sa.GetById(_p1.BossId);
        if (bossObj is null) return;
        var bossPos = bossObj.Position;

        List<float> rotDeg = [180, 180, -54, 54, -18, 18, -90, 90];
        int myIndex = sa.GetMyIndex();
        var pos = (bossPos + new Vector3(0, 0, 10)).RotateAndExtend(bossPos, rotDeg[myIndex].DegToRad(),
            sa.Data.MyObject.IsTank() ? 2.5f : 0);
        sa.DrawGuidance(pos, 0, 6000, $"P1_Omniscient_LatterHalfGuidance");

        for (int i = 0; i < 8; i++)
        {
            var dp = sa.DrawLine(bossPos, 0, 0, 6000, $"P1_Omniscient_GuideLine", rotDeg[i].DegToRad(), 20f, 20f, draw: false);
            dp.Color = i switch
            {
                0 or 1 => new Vector4(0.1f, 0.1f, 1, 1),
                2 or 3 => new Vector4(0.1f, 1f, 0.1f, 1),
                _ => new Vector4(1, 0.1f, 0.1f, 1),
            };
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Line, dp);
        }
    }
    
    [ScriptMethod(name: "P1B_Omniscient_FarthestCleaveRange", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"], suppress: 15000)]
    public void P1B_Omniscient_FarthestCleaveRange(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.2) return;

        var bossObj = sa.GetById(_p1.BossId);
        if (bossObj is null) return;
        _p1.OmniscientFarthestDistanceFramework = sa.Method.RegistFrameworkUpdateAction(Action);

        void Action()
        {
            // 1. Build distance dictionary
            var bossPos = bossObj.Position;
            var distanceDict = new Dictionary<int, float>(sa.Data.PartyList.Count);
            foreach (var memberId in sa.Data.PartyList)
            {
                var member = sa.GetById(memberId);
                if (member == null || !member.IsValid()) continue;
                distanceDict[sa.GetPlayerIdIndex(memberId)] = Vector3.Distance(member.Position, bossPos);
            }
            
            // 2. Take only the top two
            var topTwo = distanceDict.OrderByDescending(kvp => kvp.Value)
                .Take(2)
                .Select(kvp => kvp.Key)
                .ToArray();
            
            // 3. Mode determination
            int myIndex = sa.GetMyIndex();
            bool isInTopTwo = topTwo.Contains(myIndex);
            bool isTank = sa.Data.MyObject.IsTank();

            const int IS_TANK_NOT_TOP2_DANGER = 1;
            const int NOT_TANK_IS_TOP2_DANGER = 2;
            const int NOT_TANK_NOT_TOP2_SAFE = 3;
            const int IS_TANK_IS_TOP2_SAFE = -1;
            
            int currentState = (isTank, isInTopTwo) switch
            {
                (true, false) => IS_TANK_NOT_TOP2_DANGER,
                (false, true) => NOT_TANK_IS_TOP2_DANGER,
                (false, false) => NOT_TANK_NOT_TOP2_SAFE,
                _ => IS_TANK_IS_TOP2_SAFE
            };
            
            // 4. Return if state unchanged
            if (currentState == _p1.LastFarDistanceStatus) return;

            // 5. If tank and farthest, don't draw, return and clear drawings
            if (currentState == IS_TANK_IS_TOP2_SAFE)
            {
                sa.Method.RemoveDraw("P1_Omniscient_LatterHalf_CleaveState.*");
                return;
            }
            
            var isDangerous = currentState != NOT_TANK_NOT_TOP2_SAFE;
            var radian = (isDangerous ? 30f : 120f).DegToRad();
            var color = isDangerous ? new Vector4(1f, 0.1f, 0.1f, 2f) : sa.Data.DefaultSafeColor;
            
            // Drawing logic
            if (currentState == NOT_TANK_IS_TOP2_DANGER)
            {
                DrawFanCleave(myIndex, radian, color, !isDangerous, currentState);
            }
            else if (currentState == IS_TANK_NOT_TOP2_DANGER)
            {
                if (topTwo[0] > 1) DrawFanCleave(topTwo[0], radian, color, !isDangerous, currentState);
                if (topTwo[1] > 1) DrawFanCleave(topTwo[1], radian, color, !isDangerous, currentState);
            }
            else
            {
                // Who are the farthest? Not my concern, just draw for tanks
                DrawFanCleave(0, radian, color, !isDangerous, currentState);
                DrawFanCleave(1, radian, color, !isDangerous, currentState);
            }
            
            _p1.LastFarDistanceStatus = currentState;
            return;
            
            void DrawFanCleave(int playerIdx, float radian, Vector4 color, bool isSafe, int state)
            {
                var dp = sa.DrawFan(_p1.BossId, sa.Data.PartyList[playerIdx], 0, int.MaxValue,
                    $"P1_Omniscient_LatterHalf_CleaveState{state}", radian, 0, 20, 0, isSafe: isSafe, draw: false);
                dp.Color = color;
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                switch (state)
                {
                    case IS_TANK_NOT_TOP2_DANGER:
                        sa.Method.RemoveDraw($"P1_Omniscient_LatterHalf_CleaveState{NOT_TANK_IS_TOP2_DANGER}");
                        sa.Method.RemoveDraw($"P1_Omniscient_LatterHalf_CleaveState{NOT_TANK_NOT_TOP2_SAFE}");
                        break;
                    case NOT_TANK_IS_TOP2_DANGER:
                        sa.Method.RemoveDraw($"P1_Omniscient_LatterHalf_CleaveState{IS_TANK_NOT_TOP2_DANGER}");
                        sa.Method.RemoveDraw($"P1_Omniscient_LatterHalf_CleaveState{NOT_TANK_NOT_TOP2_SAFE}");
                        break;
                    case NOT_TANK_NOT_TOP2_SAFE:
                        sa.Method.RemoveDraw($"P1_Omniscient_LatterHalf_CleaveState{IS_TANK_NOT_TOP2_DANGER}");
                        sa.Method.RemoveDraw($"P1_Omniscient_LatterHalf_CleaveState{NOT_TANK_IS_TOP2_DANGER}");
                        break;
                }
            }
        }
    }

    [ScriptMethod(name: "P1B_Omniscient_DiffuseWaveCannonCount", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31504"], suppress: 200, userControl: Debugging)]
    public void P1B_Omniscient_DiffuseWaveCannonCount(Event ev, ScriptAccessory sa)
    {
        if (_parse != 1.2) return;
        _p1.DiffuseWaveCannonCount++;

        if (_p1.DiffuseWaveCannonCount == 5)
        {
            sa.Method.UnregistFrameworkUpdateAction(_p1.OmniscientFarthestDistanceFramework);
            sa.Method.RemoveDraw($"P1.*");
        }
    }

    #endregion P1B Omniscient

    #region P2 Omega Firewall Settings

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP2 Omega Firewall Settingsã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P2_OmegaFirewallSettings_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P2A_Firewall_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31552"], userControl: Debugging)]
    public void P2A_Firewall_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        _parse = 2;
        sa.Method.RemoveDraw($"P1.*");
        _p1.Reset(sa);
        _p1.Dispose();
        _p2.Register();
    }
    
    [ScriptMethod(name: "P2A_Firewall_BossIdRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3155[23])$"], userControl: Debugging)]
    public void P2A_Firewall_BossIdRecord(Event ev, ScriptAccessory sa)
    {
        const uint FIREWALL_MALE = 31552;
        const uint FIREWALL_FEMALE = 31553;

        switch (ev.ActionId)
        {
            case FIREWALL_FEMALE:
                _p2.BossIdFemale = ev.SourceId;
                break;
            case FIREWALL_MALE:
                _p2.BossIdMale = ev.SourceId;
                break;
            default:
                return;
        }
    }
    
    [ScriptMethod(name: "*P2A_Firewall_BlockInvalidTargets", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31552"], userControl: true)]
    public void P2A_Firewall_BlockInvalidTargets(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2) return;
        
        if (!SpecialMode) return;
        _p2.EnableFirewall = true;
        _p2.FirewallTargetCheckFramework = sa.Method.RegistFrameworkUpdateAction(Action);
        const uint MALE_DISABLE = 3499;
        const uint FEMALE_DISABLE = 3500;
        return;
    
        void Action()
        {
            var myObject = sa.Data.MyObject;
            if (myObject is null) return;

            if (!_p2.EnableFirewall)
            {
                if (_p2.LastFirewallStatus == 0) return;
                // During P2 mechanic 1
                sa.SetTargetable(sa.GetById(_p2.BossIdMale), false);
                sa.SetTargetable(sa.GetById(_p2.BossIdFemale), false);
                _p2.LastFirewallStatus = 0;
                return;
            }
            
            const int MALE_UNTARGETABLE = 1;
            const int FEMALE_UNTARGETABLE = 2;
            const int FREELY_TARGETABLE = 3;
            const int UNREACHABLE = 4;
            
            int currentState = (myObject.HasStatus(MALE_DISABLE), myObject.HasStatus(FEMALE_DISABLE)) switch
            {
                (true, false) => MALE_UNTARGETABLE,
                (false, true) => FEMALE_UNTARGETABLE,
                (false, false) => FREELY_TARGETABLE,
                _ => UNREACHABLE
            };
            
            if (currentState == _p2.LastFirewallStatus && _p2.LastFirewallStatus != 0) return;
            
            _p2.LastFirewallStatus = currentState;
            sa.SetTargetable(sa.GetById(_p2.BossIdMale), !myObject.HasStatus(MALE_DISABLE));
            sa.SetTargetable(sa.GetById(_p2.BossIdFemale), !myObject.HasStatus(FEMALE_DISABLE));
        }
    }

    [ScriptMethod(name: "P2A_Firewall_TemporarilyDisableDuringMechanic1", eventType: EventTypeEnum.Targetable,
        eventCondition: ["Targetable:False", "DataId:regex:^(1571[23])$"], userControl: Debugging, suppress: 1000)]
    public void P2A_Firewall_TemporarilyDisableDuringMechanic1(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.1) return;
        sa.DebugMsg($"P2A_Firewall_TemporarilyDisableDuringMechanic1", Debugging);
        _p2.EnableFirewall = false;
    }
    
    [ScriptMethod(name: "P2A_Firewall_MaleFemaleGenderSwap", eventType: EventTypeEnum.ActionEffect,
        eventCondition: ["ActionId:regex:^(3151[78])$"], userControl: Debugging, suppress: 1000)]
    public void P2A_Firewall_MaleFemaleGenderSwap(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.1) return;
        sa.DebugMsg($"P2A_Firewall_MaleFemaleGenderSwap", Debugging);
        (_p2.BossIdFemale, _p2.BossIdMale) = (_p2.BossIdMale, _p2.BossIdFemale);
    }
    
    [ScriptMethod(name: "*P2A_Firewall_ReEnableAfterMechanic1", eventType: EventTypeEnum.Targetable,
        eventCondition: ["Targetable:True", "DataId:regex:^(1571[23])$"], userControl: Debugging)]
    public void P2A_Firewall_ReEnableAfterMechanic1(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.15) return;
        if (!SpecialMode) return;
        lock (_p2)
        {
            _p2.SelectableTargetCount++;
            sa.DebugMsg($"P2A_Firewall_ReEnableAfterMechanic1: {_p2.SelectableTargetCount}", Debugging);
            if (_p2.SelectableTargetCount < 2) return;
            sa.DebugMsg($"P2A_Firewall_ReEnableAfterMechanic1", Debugging);
            _p2.EnableFirewall = true;
        }
    }

    [ScriptMethod(name: "P2A_Firewall_DisableFirewallCheckBeforeMechanic2", eventType: EventTypeEnum.StatusRemove,
        eventCondition: ["StatusID:regex:^(3500|3499)$"], userControl: Debugging, suppress: 1000)]
    public void P2A_Firewall_DisableFirewallCheckBeforeMechanic2(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.2) return;
        sa.Method.UnregistFrameworkUpdateAction(_p2.FirewallTargetCheckFramework);
        _p2.EnableFirewall = false;
        sa.SetTargetable(sa.GetById(_p2.BossIdMale), true);
        sa.SetTargetable(sa.GetById(_p2.BossIdFemale), true);
    }
    
    [ScriptMethod(name: "*P2A_Firewall_SetInvincibleUntargetable", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(671)$"], userControl: true)]
    public void P2_Firewall_SetInvincibleUntargetable(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.2) return;
        if (!SpecialMode) return;
        sa.SetTargetable(sa.GetById(ev.TargetId), false);
    }
    
    [ScriptMethod(name: "P2A_Firewall_InvincibleRemoval", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:regex:^(671)$"], userControl: Debugging)]
    public void P2_Firewall_InvincibleRemoval(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.2) return;
        sa.SetTargetable(sa.GetById(ev.TargetId), true);
    }
    
    #endregion P2 Omega Firewall Settings

    #region P2A Synergy Program

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP2A Synergy Programã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P2A_SynergyProgram_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P2A_SynergyProgram_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31550"], userControl: Debugging)]
    public void P2A_SynergyProgram_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        _parse = 2.1;
        _pd.Init(sa, "P2 SONY");
        _pd.AddPriorities([2, 3, 1, 8, 4, 5, 6, 7]);    // Lower value = higher priority
    }
    
    [ScriptMethod(name: "P2A_SynergyProgram_FarNearRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3427|3428)$"], userControl: Debugging, suppress: 10000)]
    public void P2_SynergyProgram_FarNearRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.1) return;
        const uint MID_GLITCH = 3427, REMOTE_GLITCH = 3428;
        _p2.SynergyProgramIsFarTether = ev.StatusId == REMOTE_GLITCH;
        sa.DebugMsg($"Recorded Synergy Program tether as {(_p2.SynergyProgramIsFarTether ? "Far" : "Near")}", Debugging);
    }
    
    [ScriptMethod(name: "P2A_SynergyProgram_SONYRecord", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(01A[0123])$"], userControl: Debugging)]
    public void P2_SynergyProgram_SONYRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.1) return;
        const uint CIRCLE_1 = 416, CROSS_2 = 419, TRIANGLE_3 = 417, SQUARE_4 = 418;
        lock (_pd)
        {
            var idx = sa.GetPlayerIdIndex((uint)ev.TargetId);
            var priVal = ev.Id0() switch
            {
                CIRCLE_1 => 10,
                CROSS_2 => 20,
                TRIANGLE_3 => 30,
                SQUARE_4 => 40,
                _ => 0
            };
            _pd.AddPriority(idx, priVal);
            _pd.AddActionCount();
            if (_pd.ActionCount != 8) return;

            // Add 100 priority to the four players on the right
            var (key1, key2, key3, key4) = (_pd.SelectSpecificPriorityIndex(1).Key,
                _pd.SelectSpecificPriorityIndex(3).Key,
                _pd.SelectSpecificPriorityIndex(5).Key, _pd.SelectSpecificPriorityIndex(7).Key);
            
            _pd.AddPriority(key1, 100);
            _pd.AddPriority(key2, 100);
            _pd.AddPriority(key3, 100);
            _pd.AddPriority(key4, 100);
        }

    }
    
    [ScriptMethod(name: "P2A_SynergyProgram_MaleFemaleAttackRange", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7747", "SourceDataId:regex:^(1571[45])$"])]
    public void P2_SynergyProgram_MaleFemaleAttackRange(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.1) return;
        Vector3 pos = ev.SourcePosition;
        if (Vector3.Distance(pos, Center) > 12) return;
        
        var obj = sa.GetById(ev.SourceId);
        if (obj == null) return;
        
        var dataId = obj.DataId;
        var transId = sa.GetTransformationId(obj);
        if (transId == null) return;
        
        const uint MAN = 15714, WOMAN = 15715;
        // const byte MAN_CHARIOT = 0, MAN_DONUT = 4, WOMAN_CROSS = 0, WOMAN_HOTWING = 4;

        switch (dataId == MAN, transId == 0)
        {
            case (true, true):
                sa.DrawCircle(ev.SourceId, 0, 5500, $"P2_SynergyProgram_MaleFemaleAttackRange_MaleChariot", 10);
                break;
            case (true, false):
                sa.DrawDonut(ev.SourceId, 0, 5500, $"P2_SynergyProgram_MaleFemaleAttackRange_MaleDonut", 40, 10);
                break;
            case (false, true):
                var dp1 = sa.DrawRect(ev.SourceId, 0, 5500, $"P2_SynergyProgram_MaleFemaleAttackRange_FemaleCross1", 0, 10, 60, draw: false);
                var dp2 = sa.DrawRect(ev.SourceId, 0, 5500, $"P2_SynergyProgram_MaleFemaleAttackRange_FemaleCross2", float.Pi / 2, 10, 60, draw: false);
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1);
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp2);
                break;
            case (false, false):
                var dp3 = sa.DrawDonut(ev.SourceId, 0, 5500, $"P2_SynergyProgram_MaleFemaleAttackRange_FemaleHotWing", 60, 8, draw: false);
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.HotWing, dp3);
                break;
        }
    }

    [ScriptMethod(name: "P2A_SynergyProgram_MaleFemaleAttackRangeDelete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3152[56])$"], userControl: Debugging, suppress: 10000)]
    public void P2_SynergyProgram_MaleFemaleAttackRangeDelete(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.1) return;
        // Delete using male's chariot/donut cast finish marker
        sa.Method.RemoveDraw($"P2_SynergyProgram_MaleFemaleAttackRange.*");
        _p2.EyeLaserReadyForDraw.Set();
    }

    [ScriptMethod(name: "P2A_SynergyProgram_EyeLaserRange", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:2"])]
    public void P2_SynergyProgram_EyeLaserRange(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.1) return;
        _p2.EyeLaserReadyForDraw.WaitOne();
        var basePos = new Vector3(100, 0, 80);
        var eyePos = basePos.RotateAndExtend(Center, -45f.DegToRad() * _p2.LargeEyeDirection);
        sa.DrawRect(eyePos, Center, 0, 10000, "P2_SynergyProgram_EyeLaser", 0, 16, 40);
        _p2.EyeLaserReadyForDraw.Reset();
    }
    
    [ScriptMethod(name: "P2A_SynergyProgram_EyeLaserAndSONYGuidanceDelete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31521"], userControl: Debugging)]
    public void P2_SynergyProgram_EyeLaserAndSONYGuidanceDelete(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.1) return;
        sa.Method.RemoveDraw($"P2_SynergyProgram_EyeLaser");
        sa.Method.RemoveDraw($"P2_SynergyProgram_SONYPOS");
    }

    [ScriptMethod(name: "P2A_SynergyProgram_EyeLaserDirectionRecord", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:2"], userControl: Debugging)]
    public void P2_SynergyProgram_EyeLaserDirectionRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.1) return;
        var region = ev.Index() - 1;
        sa.DebugMsg($"Recorded Flag2, region: {region}", Debugging);
        // Index starts from 1, starting at A, increasing clockwise
        _p2.LargeEyeDirection = (int)region;
        _p2.EyeLaserDirectionRecord.Set();
    }
    
    [ScriptMethod(name: "P2A_SynergyProgram_SONYPOS", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3152[56])$"],
        userControl: true, suppress: 10000)]
    public void P2_SynergyProgram_SONYPOS(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.1) return;
        _p2.EyeLaserDirectionRecord.WaitOne();
        
        // BasePos definitions are in the order of Circle High - Cross High - Triangle High - Square High - Circle Low - ...
        List<Vector3> middleBasePos =
        [
            new(88.5f, 0, 85.5f), new(88.5f, 0, 95.0f), new(88.5f, 0, 105.0f), new(88.5f, 0, 114.5f),
            new(111.5f, 0, 85.5f), new(111.5f, 0, 95.5f), new(111.5f, 0, 105.0f), new(111.5f, 0, 114.5f)
        ];
        List<Vector3> farBasePos =
        [
            new(91.5f, 0, 83.0f), new(82.0f, 0, 93.0f), new(82.0f, 0, 107.0f), new(91.5f, 0, 117.0f),
            new(108.5f, 0, 117.0f), new(118.0f, 0, 107.0f), new(118.0f, 0, 93.0f), new(108.5f, 0, 83.0f)
        ];

        sa.DebugMsg(_pd.ShowPriorities(), Debugging);
        var rank = _pd.FindPriorityIndexOfKey(sa.GetMyIndex());
        var pos = (_p2.SynergyProgramIsFarTether ? farBasePos : middleBasePos)[rank].RotateAndExtend(Center, _p2.LargeEyeDirection * -45f.DegToRad());
        sa.DrawGuidance(pos, 0, 10000, $"P2_SynergyProgram_SONYPOS");
        
        _p2.EyeLaserDirectionRecord.Reset();
    }

    [ScriptMethod(name: "P2A_SynergyProgram_MaleChariotPositionRecord", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31516"], userControl: Debugging)]
    public void P2_SynergyProgram_MaleChariotPositionRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.1) return;
        _p2.MaleChariotDirection = ev.SourcePosition.GetRadian(Center).RadianToRegion(8, isDiagDiv: true);
        sa.DebugMsg($"Male Chariot direction {_p2.MaleChariotDirection}", Debugging);
        _p2.MaleChariotDirectionRecord.Set();
    }
    
    [ScriptMethod(name: "P2A_SynergyProgram_StackRecord", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0064"], userControl: Debugging)]
    public void P2_SynergyProgram_StackPosition(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.1) return;
        lock (_pd)
        {
            var index = sa.GetPlayerIdIndex((uint)ev.TargetId);
            _pd.AddPriority(index, 1000);
            
            // Find tether partner, increase priority value
            var icon = _pd.Priorities[index].GetDecimalDigit(2);
            foreach (var kvp in _pd.Priorities)
            {
                if (kvp.Key == index) continue;
                if (kvp.Value.GetDecimalDigit(2) != icon) continue;
                _pd.AddPriority(kvp.Key, 500);
            }
            
            _pd.AddActionCount();
            if (_pd.ActionCount < 10) return;
            _p2.StackRecord.Set();
            sa.DebugMsg(_pd.ShowPriorities(), Debugging);
            _parse = 2.15;
            sa.DebugMsg($"Phase changed to {_parse}", Debugging);
        }
    }
    
    [ScriptMethod(name: "P2A_SynergyProgram_StackGuidance", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31521"], userControl: true)]
    public void P2_SynergyProgram_StackGuidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.1) return;
        
        _p2.StackRecord.WaitOne();
        _p2.MaleChariotDirectionRecord.WaitOne();
        
        try
        {
            int leftRegion = (_p2.MaleChariotDirection + 2 + 8) % 8;
            int rightRegion = (_p2.MaleChariotDirection - (_p2.SynergyProgramIsFarTether ? 2 : 4) + 8) % 8;
            sa.DebugMsg($"Stack left direction {leftRegion}, right direction {rightRegion}");

            var dp1 = sa.DrawLine(Center, 0, 0, 6000, $"P2_SynergyProgram_GuideLineLeft", leftRegion * 45f.DegToRad(), 20f, 20f,
                draw: false);
            dp1.Color = new Vector4(0.1f, 1f, 0.1f, 1);
            var dp2 = sa.DrawLine(Center, 0, 0, 6000, $"P2_SynergyProgram_GuideLineRight", rightRegion * 45f.DegToRad(), 20f, 20f,
                draw: false);
            dp2.Color = new Vector4(0.1f, 1f, 0.1f, 1);
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Line, dp1);
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Line, dp2);
        
            // Left: Male Chariot direction +2; Right: -2 (far) or -4 (middle)
            const int IDLE = 1;
            const int STACK_PARTNER = 2;
            const int STACK_SOURCE = 3;

            int myPriVal = _pd.Priorities[sa.GetMyIndex()];
            int myRank = _pd.FindPriorityIndexOfKey(sa.GetMyIndex());

            (int myState, string myStateStr) = myPriVal switch
            {
                > 1000 => (STACK_SOURCE, "Stack Source"),
                > 500 => (STACK_PARTNER, "Stack Partner"),
                _ => (IDLE, "Idle")
            };

            int safePosRegion = 0;
            bool bothStackSourceAtRight = _pd.SelectSpecificPriorityIndex(1, true).Value.GetDecimalDigit(3) == 1;
            bool bothStackSourceAtLeft = _pd.SelectSpecificPriorityIndex(0, true).Value.GetDecimalDigit(3) == 0;
            
            switch (myState)
            {
                case IDLE:
                {
                    safePosRegion = myPriVal.GetDecimalDigit(3) == 0 ? leftRegion : rightRegion;
                    // safePosRegion = myRank <= 1 ? leftRegion : rightRegion;
                    break;
                }
                case STACK_PARTNER:
                {
                    var needReverse = bothStackSourceAtLeft || (bothStackSourceAtRight && _p2.SynergyProgramIsFarTether);
                    safePosRegion = myRank == (needReverse ? 5 : 4) ? leftRegion : rightRegion;
                    break;
                }
                case STACK_SOURCE:
                {
                    var needReverse = bothStackSourceAtRight && !_p2.SynergyProgramIsFarTether;
                    safePosRegion = myRank == (needReverse ? 7 : 6) ? leftRegion : rightRegion;
                    break;
                }
            }
            sa.DebugMsg($"Player type is {myStateStr}, safe zone is {(safePosRegion == leftRegion ? "Left" : "Right")}", Debugging);
            var pos = new Vector3(100, 0, 105).RotateAndExtend(Center, safePosRegion * 45f.DegToRad());
            sa.DrawGuidance(pos, 0, 10000, $"P2_SynergyProgram_StackKnockbackPosition");
        
            _p2.FemaleKnockbackRecord.WaitOne();
        
            sa.Method.RemoveDraw($"P2_SynergyProgram_StackKnockbackPosition");
            var pos2 = new Vector3(100, 0, _p2.SynergyProgramIsFarTether ? 119.5f : 115f).RotateAndExtend(Center, safePosRegion * 45f.DegToRad());
            sa.DrawGuidance(pos2, 0, 10000, $"P2_SynergyProgram_StackKnockbackNext");
            
            _p2.MaleStackChariotRecord.WaitOne();
            sa.Method.RemoveDraw($"P2_SynergyProgram.*");
        }
        finally
        {
            _p2.StackRecord.Reset();
            _p2.MaleChariotDirectionRecord.Reset();
            _p2.FemaleKnockbackRecord.Reset();
            _p2.MaleStackChariotRecord.Reset();
        }
    }

    [ScriptMethod(name: "P2A_SynergyProgram_FemaleCenterKnockbackRecord", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31534"],
        userControl: Debugging, suppress: 10000)]
    public void P2_SynergyProgram_FemaleKnockbackRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.15) return;
        _p2.FemaleKnockbackRecord.Set();
    }
    
    [ScriptMethod(name: "P2A_SynergyProgram_MaleStackChariotRecord", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31526"],
        userControl: Debugging, suppress: 10000)]
    public void P2_SynergyProgram_MaleStackChariotRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.15) return;
        _p2.MaleStackChariotRecord.Set();
    }

    #endregion P2A Synergy Program

    #region P2B Blades

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP2B Bladesã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P2B_Blades_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P2B_Blades_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31544"], userControl: Debugging)]
    public void P2_Blades_PhaseSplit(Event @event, ScriptAccessory accessory)
    {
        _parse = 2.2;
    }
    
    [ScriptMethod(name: "P2B_Blades_ArcherArrow", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31539"], userControl: true)]
    public void P2_Blades_ArcherArrow(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.2) return;
        sa.DrawRect(ev.SourceId, 1000, 7000, $"P2_Blades_ArcherArrow", 0, 10, 42);
    }
    
    [ScriptMethod(name: "P2B_Blades_ArcherArrowDelete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31539"], userControl: Debugging)]
    public void P2_Blades_ArcherArrowDelete(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.2) return;
        sa.Method.RemoveDraw($"P2_Blades_ArcherArrow");
    }
    
    [ScriptMethod(name: "P2B_Blades_TetherCleave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3154[01])$"], userControl: true)]
    public void P2_Blades_TetherCleave(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.2) return;
        var dp = sa.DrawFan(ev.SourceId, 0, 5500, 2000, $"P2_Blades_TetherCleave",
            float.Pi / 2, 0, 40, 0, draw: false);
        dp.TargetResolvePattern = PositionResolvePatternEnum.TetherTarget;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "P2B_Blades_TetherCleaveDelete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3154[01])$"], userControl: Debugging, suppress: 10000)]
    public void P2_Blades_TetherCleaveDelete(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.2) return;
        sa.Method.RemoveDraw($"P2_Blades_TetherCleave");
    }
    
    [ScriptMethod(name: "P2B_Blades_ShieldComboGuidanceDirectionHint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31527"], userControl: true)]
    public void P2_Blades_ShieldComboGuidanceDirectionHint(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.2) return;
        if (sa.Data.MyObject.IsTank())
        {
            var dp = sa.DrawLine(sa.Data.Me, ev.SourceId, 0, 10000, $"P2_Blades_ShieldComboMaleTether", 0, 3f, 10f, isSafe: true, byY: true, draw: false);
            dp.Color = new Vector4(0f, 1f, 1f, 1f);
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);

            var spos = ev.SourcePosition;
            var myPos = new Vector3(100, 0, 108).RotateAndExtend(Center,
                spos.GetRadian(Center) + (sa.GetMyIndex() == 0 ? 20f : -20f).DegToRad());
            sa.DrawGuidance(myPos, 0, 10000, $"P2_Blades_ShieldComboGuidanceDirectionHint");
        }
        else
            sa.DrawGuidance(Center, 0, 10000, $"P2_Blades_ShieldComboGuidanceDirectionHint");
    }
    
    [ScriptMethod(name: "P2B_Blades_ShieldComboGuidanceDirectionDelete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31527"], userControl: Debugging)]
    public void P2_Blades_ShieldComboGuidanceDirectionDelete(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.2) return;
        sa.Method.RemoveDraw($"P2_Blades_ShieldCombo.*");
    }
    
    [ScriptMethod(name: "P2B_Blades_StackAfterShieldCombo", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31528", "TargetIndex:1"], userControl: true)]
    public void P2_Blades_StackAfterShieldCombo(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.2) return;

        var targetIsMe = ev.TargetId == sa.Data.Me;
        var dp = sa.DrawCircle(ev.SourceId, 0, 10000, $"P2_Blades_Stack", 6f, isSafe: !targetIsMe, draw: false);
        dp.SetOwnersDistanceOrder(true, 1);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        sa.Method.TextInfo(targetIsMe ? "Go out, go out" : "Stack up", 3000);
        sa.Method.TTS(targetIsMe ? "Go out, go out" : "Stack up");
    }
    
    [ScriptMethod(name: "P2B_Blades_StackAfterShieldComboDelete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31529", "TargetIndex:1"], userControl: Debugging)]
    public void P2_Blades_StackAfterShieldComboDelete(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.2) return;
        sa.Method.RemoveDraw($"P2_Blades_Stack");
    }
    
    #endregion P2B Blades

    #region P2C Transition

    [ScriptMethod(name: "P2C_Transition_PhaseSplit", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31507"], userControl: Debugging)]
    public void P2C_Transition_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        _parse = 2.5;
        _pd.Init(sa, "P2.5 Transition");
    }
    
    [ScriptMethod(name: "P2C_Transition_RecordMarkers", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:regex:^(0[1234679]|10)$"],
        userControl: Debugging)]
    public void P2C_Transition_RecordMarkers(Event ev, ScriptAccessory sa)
    {
        // Record Attack 1234, Bind 12, Stop 12
        if (_parse != 2.5) return;
        
        lock (_pd)
        {
            var mark = ev.Id0();
            var tidx = sa.GetPlayerIdIndex((uint)ev.TargetId);
            var targetJob = sa.GetPlayerJobByIndex(tidx);
            sa.DebugMsg($"Detected {targetJob} marked with ev.Id {mark}", Debugging);
            _pd.AddActionCount();
            var pdVal = mark switch
            {
                0x01 => 10,    // Attack 1
                0x02 => 20,    // Attack 2
                0x03 => 30,    // Attack 3
                0x04 => 40,    // Attack 4
                0x06 => 100,   // Bind 1
                0x07 => 110,   // Bind 2
                0x09 => 200,   // Stop 1
                0x10 => 210,   // Stop 2
                _ => 0
            };
            _pd.AddPriority(tidx, pdVal);
            if (_pd.ActionCount != 8) return;
            sa.DebugMsg($"P2C_Transition_RecordMarkers: Markers recorded", Debugging);
            sa.DebugMsg($"{_pd.ShowPriorities()}", Debugging);
            _p2.TransitionMarkerRecord.Set();   // Marker record
        }
    }
    
    [ScriptMethod(name: "P2C_Transition_InitialPositionGuidance", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3426)$"],
        userControl: true, suppress: 10000)]
    public void P2C_Transition_InitialPosition(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.5) return;
        
        // Atk1-4, Bind1-2, Stop1-2
        List<float> deg = [-90f, -30f, 30f, 90f, -150f, 150f, -150f, 150f];
        int myIndex = sa.GetMyIndex();
        
        var hasMarker = _p2.TransitionMarkerRecord.WaitOne(3000);
        
        if (!hasMarker)
        {
            sa.DebugMsg($"Markers not detected within time, enable THD priority order", Debugging);
            _pd.AddPriorities([1, 2, 3, 4, 5, 6, 7, 8]);
            for (int i = 0; i < sa.Data.PartyList.Count; i++)
            {
                var obj = sa.GetById(sa.Data.PartyList[i]);
                if (obj is null) return;
                (bool hasStack, bool hasSpread) = (((IBattleChara)obj).HasStatus(3426), ((IBattleChara)obj).HasStatus(3425));
                var priVal = (hasStack, hasSpread) switch
                {
                    (true, false) => 100,
                    (false, true) => 10,
                    (false, false) => 200,
                    _ => 0
                };
                if (priVal == 0) return;
                _pd.AddPriority(i, priVal);
            }
        }
        
        float myPosDeg = deg[_pd.FindPriorityIndexOfKey(myIndex)];
        var pos = new Vector3(100, 0, 119.5f).RotateAndExtend(Center, myPosDeg.DegToRad());
        sa.DrawGuidance(pos, 0, 5000, $"P2C_Transition_InitialPositionGuidance");
        _p2.TransitionMarkerRecord.Reset();
    }
    
    [ScriptMethod(name: "P2C_Transition_Quake", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3156[789]|31570)$", "TargetIndex:1"],
        userControl: true)]
    public void P2C_Transition_Quake(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.5) return;
        const uint CHARIOT = 31567, DONUT_1 = 31568, DONUT_2 = 31569, DONUT_3 = 31570;

        (int outScale, int innerScale, int donutCount) = ev.ActionId switch
        {
            CHARIOT => (12, 6, 1),
            DONUT_1 => (18, 12, 2),
            DONUT_2 => (24, 18, 3),
            _ => (0, 0, 4)
        };

        string prefix = "P2C_Transition_QuakeDonut";
        sa.Method.RemoveDraw($"{prefix}{donutCount-1}");
        if (donutCount == 4) return;
        
        var dp = sa.DrawDonut(Center, 0, 10000, $"{prefix}{donutCount}", outScale, innerScale, draw: false);
        dp.Color = sa.Data.DefaultDangerColor.WithW(2f);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
        if (donutCount != 3) return;
        sa.Method.RemoveDraw($"P2C_Transition_InitialPositionGuidance");
    }

    [ScriptMethod(name: "P2C_Transition_ArmPositionRecord", eventType: EventTypeEnum.PlayActionTimeline,
        eventCondition: ["Id:regex:^(774[78])$", "SourceDataId:regex:^(1571[89])$"], userControl: Debugging, suppress: 10000)]
    public void P2C_Transition_ArmPositionRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.5) return;
        var region = ev.SourcePosition.GetRadian(Center).RadianToRegion(6, isDiagDiv: true);
        _p2.TransitionArmFirstUpwardTriangle = region % 2 == 1;
    }
    
    [ScriptMethod(name: "P2C_Transition_StackSpreadGuidance", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:regex:^(31566)$"], userControl: true, suppress: 10000)]
    public void P2C_Transition_StackSpreadGuidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.5) return;
        
        // Atk1-4, Bind1-2, Stop1-2
        List<float> deg = [-90f, -30f, 30f, 90f, -150f, 150f, -150f, 150f];
        int myPriIndex = _pd.FindPriorityIndexOfKey(sa.GetMyIndex());
        
        float myPosDeg = deg[myPriIndex];
        Vector3 regionPos = new Vector3(100, 0, 119.5f).RotateAndExtend(Center, myPosDeg.DegToRad());

        List<bool> ccwPrep     = [false, true, false, true, true, false, true, false];
        List<bool> ccwMovement = [true, false, true, false, false, true, false, true];

        const float PREP_ROTATE_DEG_BASE = 7.5f;
        const float MOVEMENT_ROTATE_DEG_BASE = 25f;
        const float MOVEMENT_EXTEND_DISTANCE_BASE = -3f;

        float prepRotateDeg = PREP_ROTATE_DEG_BASE.DegToRad() * (ccwPrep[myPriIndex] ? 1 : -1) * (_p2.TransitionArmFirstUpwardTriangle ? 1 : -1);
        float movementRotateDeg = MOVEMENT_ROTATE_DEG_BASE.DegToRad() * (ccwMovement[myPriIndex] ? 1 : -1) * (_p2.TransitionArmFirstUpwardTriangle ? 1 : -1);

        Vector3 prepPos = regionPos.RotateAndExtend(Center, prepRotateDeg);
        Vector3 movementPos = prepPos.RotateAndExtend(Center, movementRotateDeg, MOVEMENT_EXTEND_DISTANCE_BASE);

        sa.DrawGuidance(prepPos, 0, 2000, $"P2C_Transition_StackSpreadGuidance1");
        sa.DrawGuidance(prepPos, movementPos, 0, 2000, $"P2C_Transition_StackSpreadGuidance2Prepare", isSafe: false);
        sa.DrawGuidance(movementPos, 2000, 2000, $"P2C_Transition_StackSpreadGuidance2");
    }
    
    [ScriptMethod(name: "*P2C_Transition_HideFieldDonutAndArmChariotEffects", eventType: EventTypeEnum.ActionEffect,
        eventCondition: ["ActionId:regex:^(3156[689]|31570)$", "TargetIndex:1"], userControl: true)]
    public void P2C_Transition_HideEffects(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.5) return;
        if (!SpecialMode) return;
        sa.WriteVisible(sa.GetById((uint)ev.SourceId), false);
    }

    #endregion P2C Transition

    #region P3A Hello World

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP3A Hello Worldã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P3A_HelloWorld_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P3A_HelloWorld_InitialSpread", eventType: EventTypeEnum.Targetable, eventCondition: ["DataId:15717"], userControl: true)]
    public void P3A_HelloWorld_InitialSpread(Event ev, ScriptAccessory sa)
    {
        if (_parse != 2.5) return;
        List<int> region = [4, 2, 6, 0, 7, 1, 5, 3];
        sa.DrawGuidance(new Vector3(100, 0, 105f).RotateAndExtend(Center, region[sa.GetMyIndex()] * 45f.DegToRad()), 0,
            5000, $"P3A_HelloWorld_InitialSpread");
    }
    
    [ScriptMethod(name: "P3A_HelloWorld_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31573"], userControl: Debugging)]
    public void P3A_HelloWorld_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        _parse = 3;
        _p2.Reset(sa);
        _p2.Dispose();
        _p3.Register();
        _pd.Init(sa, "P3HW");
        _p3.BossId = ev.SourceId;
        
        ResetSupportUnitVisibility(sa);
    }
    
    [ScriptMethod(name: "P3A_HelloWorld_InitialSpreadDelete", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31573"], userControl: Debugging)]
    public void P3A_HelloWorld_InitialSpreadDelete(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3) return;
        sa.Method.RemoveDraw($"P3A_HelloWorld_InitialSpread");
    }
    
    [ScriptMethod(name: "P3A_HelloWorld_BuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(343[6789]|3527)$"],
        userControl: Debugging)]
    public void P3A_HelloWorld_BuffRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3) return;
        if (ev.SourceId == 0x00000000) return;
        const uint STACK_PREP = 3436;
        const uint DEFAMATION_PREP = 3437;
        const uint RED_ROT_PREP = 3438;
        const uint BLUE_ROT_PREP = 3439;
        const uint NEED_DEFAMATION = 3527;
        
        lock (_pd)
        {
            if (_pd.ActionCount >= 10) return;
            var score = ev.StatusId switch
            {
                BLUE_ROT_PREP   => 2,
                RED_ROT_PREP    => 4,
                STACK_PREP      => 1,
                DEFAMATION_PREP => 16,
                NEED_DEFAMATION => 8,
                _ => 0
            };
            _pd.AddPriority(sa.GetPlayerIdIndex((uint)ev.TargetId), score);
            _pd.AddActionCount();
            if (_pd.SelectSpecificPriorityIndex(0, true).Value != 20) return;
            _p3.BigCircleIsRedTower = true;
            
        }
    }
    
    [ScriptMethod(name: "P3A_HelloWorld_RoundIncrement", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(31599)$"],
        userControl: Debugging)]
    public void P3A_HelloWorld_RoundIncrement(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3) return;
        _p3.HelloWorldRound++;
        _p3.RedTowerPosition = 0;
        _p3.BlueTowerPosition = 0;
        sa.DebugMsg($"Now Hello World Round {_p3.HelloWorldRound}", Debugging);
        sa.DebugMsg(_pd.ShowPriorities(), Debugging);
        _p3.HelloWorldRoundRecord.Set();
    }
    
    [ScriptMethod(name: "P3A_HelloWorld_RedBlueTowerDirectionRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3158[34])$"],
        userControl: Debugging)]
    public void P3A_HelloWorld_RedBlueTowerDirectionRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3) return;
        
        _p3.HelloWorldRoundRecord.WaitOne();
        
        const uint RED_TOWER = 31583;
        const uint BLUE_TOWER = 31584;
        
        lock (_pd)
        {
            var region = ev.SourcePosition.GetRadian(Center).RadianToRegion(8, isDiagDiv: true);
            if (ev.ActionId == RED_TOWER)
                AddTowerParam(ref _p3.RedTowerPosition, region);
            else
                AddTowerParam(ref _p3.BlueTowerPosition, region);
            if (_p3.RedTowerPosition / 100 + _p3.BlueTowerPosition / 100 < 4) return;
            _p3.RedBlueTowerDirectionRecord.Set();
        }
        void AddTowerParam(ref int towerParam, int region)
        {
            towerParam += region * (towerParam > 0 ? 1 : 10);
            towerParam += 100;
        }
    }
    
    [ScriptMethod(name: "P3A_HelloWorld_InitialDestinationMarking", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(31599)$"],
        userControl: true)]
    public void P3A_HelloWorld_InitialDestinationMarking(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3) return;
        
        //  è¿œç¦» after stack -> stack -> é è¿‘ after big circle -> big circle ->
        const int STATE_REMOTE_BREAK = 0;
        const int STATE_STACK = 1;
        const int STATE_LOCAL_BREAK = 2;
        const int STATE_DEFAMATION = 3;

        _p3.HelloWorldRoundRecord.WaitOne();
        _p3.RedBlueTowerDirectionRecord.WaitOne();
        
        const float REMOTE_OUT = 11.5f;
        const float STACK_OUT = 12f;
        const float DEFAMATION_OUT = 15f;
        const float LOCAL_OUT = 14f;
        
        const float REMOTE_CLOSE_DEG = 30f;
        const float STACK_CLOSE_DEG = 20f;
        const float DEFAMATION_CLOSE_DEG = 0f;
        const float LOCAL_CLOSE_DEG = -30f;
        
        try
        {
            // Calculate player's current state (0:Far Tether, 1:Stack, 2:Near Tether, 3:Big Circle)
            var myState = (_pd.FindPriorityIndexOfKey(sa.GetMyIndex()) / 2 + _p3.HelloWorldRound - 1 + 4) % 4;
            sa.DebugMsg($"Player's current state: {myState}, 0 Far Tether, 1 Stack Tower, 2 Near Tether Big Circle, 3 Big Circle Tower", Debugging);
            sa.DebugMsg($"Red Tower Pos: {_p3.RedTowerPosition}, Blue Tower Pos: {_p3.BlueTowerPosition}, Big Circle is Red Tower {_p3.BigCircleIsRedTower}", Debugging);
            
            int defamationTower = _p3.BigCircleIsRedTower ? _p3.RedTowerPosition : _p3.BlueTowerPosition;
            int stackTower = _p3.BigCircleIsRedTower ? _p3.BlueTowerPosition : _p3.RedTowerPosition;
            var (towerPos, baseAngle, extend, prefix) = GetStateConfig(myState, defamationTower, stackTower);
            DrawDestinationMarks(sa, towerPos, baseAngle, extend, prefix, _p3.HelloWorldRound);
            sa.Method.TextInfo(prefix, 5000);
        }
        finally
        {
            _p3.RedBlueTowerDirectionRecord.Reset();
        }

        return;

        (int towerPos, float baseAngle, float extend, string prefix) GetStateConfig(
            int state, int defamationTower, int stackTower)
        {
            return state switch
            {
                STATE_DEFAMATION   => (defamationTower, DEFAMATION_CLOSE_DEG, DEFAMATION_OUT, $"Inside Big Circleã€{(_p3.BigCircleIsRedTower ? "Red" : "Blue")}ã€‘Tower"),
                STATE_REMOTE_BREAK => (stackTower,      REMOTE_CLOSE_DEG,     REMOTE_OUT,     $"Far Tetherã€{(_p3.BigCircleIsRedTower ? "Blue" : "Red")}ã€‘Between Towers Stack"),
                STATE_STACK        => (stackTower,      STACK_CLOSE_DEG,      STACK_OUT,      $"Stackã€{(_p3.BigCircleIsRedTower ? "Blue" : "Red")}ã€‘Inside Tower"),
                STATE_LOCAL_BREAK  => _p3.HelloWorldRound == 4 
                                    ? (stackTower,      REMOTE_CLOSE_DEG,     REMOTE_OUT,     $"Near Tether Round 4ã€{(_p3.BigCircleIsRedTower ? "Blue" : "Red")}ã€‘Between Towers Stack") // Round 4 Near uses Far
                                    : (defamationTower, LOCAL_CLOSE_DEG,      LOCAL_OUT,      $"Near Tetherã€{(_p3.BigCircleIsRedTower ? "Red" : "Blue")}ã€‘Outside Tower Spread"), // Other rounds use Big Circle tower
                _ => throw new ArgumentException($"Unknown state: {state}")
            };
        }
        
        void DrawDestinationMarks(ScriptAccessory sa, int towerPos, float baseAngle, 
            float extend, string prefix, int round)
        {
            // Determine if the first tower is recorded in the units digit (true: units, false: tens)
            // bool isFirstAtDigitOne = towerPos.GetDecimalDigit(1) < towerPos.GetDecimalDigit(2);
            // The difference is Â±2 or Â±6, when the subtraction result is -2 or 6, the direction represented by the units digit is the first tower, rotating counter-clockwise by 2 gives the second tower
            bool isFirstAtDigitOne = Math.Abs(2 - (towerPos.GetDecimalDigit(1) - towerPos.GetDecimalDigit(2))) == 4;
            float direction = isFirstAtDigitOne ? 1f : -1f;
    
            for (int i = 1; i <= 2; i++)
            {
                float offsetAngle = baseAngle * direction * (i == 1 ? 1f : -1f);
                int region = towerPos.GetDecimalDigit(i);

                Vector3 basePos = new Vector3(100, 0, 100 + extend);
                float rotateRad = (region * 45f + offsetAngle).DegToRad();
                Vector3 pos = basePos.RotateAndExtend(Center, rotateRad);
                
                sa.DebugMsg($"Drawing Round {round} {prefix}", Debugging);
                var dp = sa.DrawCircle(pos, 0, 10000, $"P3A_HelloWorld_InitialDestinationMarking_R{round}_{prefix}{i}", 0.5f, isSafe: true, draw: false);
                dp.Color = sa.Data.DefaultSafeColor.WithW(2f);
                sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
            }
        }
    }

    [ScriptMethod(name: "P3A_HelloWorld_BigCircleAndNearTetherDebuffMarking", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3158[34])$", "TargetIndex:1"],
        userControl: true, suppress: 500)]
    public void P3A_HelloWorld_BigCircleAndNearTetherDebuffMarking(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3) return;
        
        //  è¿œç¦» after stack -> stack -> é è¿‘ after big circle -> big circle ->
        const int STATE_REMOTE_BREAK = 0;
        const int STATE_STACK = 1;
        const int STATE_LOCAL_BREAK = 2;
        const int STATE_DEFAMATION = 3;
        
        // Calculate player's current state (0:Far Tether, 1:Stack, 2:Near Tether, 3:Big Circle)
        var myState = (_pd.FindPriorityIndexOfKey(sa.GetMyIndex()) / 2 + _p3.HelloWorldRound - 1 + 4) % 4;
        var myPos = sa.Data.MyObject.Position;
        
        // Big circle marks spread points
        if (myState == STATE_DEFAMATION)
        {
            var targetPos = GetDefamationPosition();
            sa.DrawGuidance(targetPos, 0, 5000, $"P3A_HelloWorld_BigCircleMark_R{_p3.HelloWorldRound}_{myState}");
            return;
        }
        
        // Near tether marks big circle debuff transfer
        if (myState == STATE_LOCAL_BREAK)
        {
            if (_p3.HelloWorldRound == 4) return;
            var target = GetDefamationPlayer(sa);
            sa.DrawGuidance(target, 0, 5000, $"P3A_HelloWorld_NearTetherMark_R{_p3.HelloWorldRound}_{myState}");
        }
        
        // Local method: Get the tower position for the big circle
        Vector3 GetDefamationPosition()
        {
            var towerRegion = _p3.BigCircleIsRedTower ? _p3.RedTowerPosition : _p3.BlueTowerPosition;
            var towerRegion1 = towerRegion.GetDecimalDigit(1);
            var towerRegion2 = towerRegion.GetDecimalDigit(2);
        
            var pos1 = new Vector3(100, 0, 114).RotateAndExtend(Center, towerRegion1 * 45f.DegToRad());
            var pos2 = new Vector3(100, 0, 114).RotateAndExtend(Center, towerRegion2 * 45f.DegToRad());

            return Vector3.Distance(myPos, pos1) < Vector3.Distance(myPos, pos2) ? pos1 : pos2;
        }

        uint GetDefamationPlayer(ScriptAccessory sa)
        {
            var defamationPlayers = 0;
            for (int i = 0; i < 8; i++)
            {
                var playerState = (_pd.FindPriorityIndexOfKey(i) / 2 + _p3.HelloWorldRound - 1 + 4) % 4;
                if (playerState != STATE_DEFAMATION) continue;
                defamationPlayers += 100 + (defamationPlayers > 0 ? i * 10 : i);
                if (defamationPlayers > 200) break;
            }

            var player1 = sa.Data.PartyList[defamationPlayers.GetDecimalDigit(1)];
            var player2 = sa.Data.PartyList[defamationPlayers.GetDecimalDigit(2)];
            
            var pos1 = sa.GetById(player1).Position;
            var pos2 = sa.GetById(player2).Position;
            
            return Vector3.Distance(myPos, pos1) < Vector3.Distance(myPos, pos2) ? player1 : player2;
        }
    }
    
    [ScriptMethod(name: "P3A_HelloWorld_RoundRecordStateReset", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3158[34])$", "TargetIndex:1"],
        userControl: Debugging, suppress: 500)]
    public void P3A_HelloWorld_RoundRecordStateReset(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3) return;
        sa.Method.RemoveDraw($"P3A_HelloWorld_InitialDestinationMarking_R{_p3.HelloWorldRound}.*");
        _p3.HelloWorldRoundRecord.Reset();
    }
    
    [ScriptMethod(name: "P3A_HelloWorld_BigCircleAndNearTetherDebuffMarkingDelete", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3526|3429)$"], userControl: Debugging)]
    public void P3A_HelloWorld_BigCircleAndNearTetherDebuffMarkingDelete(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3) return;
        
        // Delete only when a debuff transfer event occurs around the player
        var myPos = sa.Data.MyObject.Position;
        var targetPos = sa.GetById(ev.TargetId).Position;
        if (Vector3.Distance(myPos, targetPos) > 5f) return;
        
        sa.Method.RemoveDraw($"P3A_HelloWorld_NearTetherMark_R{_p3.HelloWorldRound}.*");
        sa.Method.RemoveDraw($"P3A_HelloWorld_BigCircleMark_R{_p3.HelloWorldRound}.*");
    }
    
    [ScriptMethod(name: "P3A_HelloWorld_SmallChariot", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3435|3528)$"], userControl: true)]
    public void P3A_HelloWorld_SmallChariot(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3) return;
        sa.DrawCircle(ev.TargetId, 2500, 10000, $"P3A_HelloWorld_SmallChariot_{ev.TargetId}", 5f);
    }
    
    [ScriptMethod(name: "P3A_HelloWorld_SmallChariotDelete", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:regex:^(3435|3528)$"], userControl: Debugging)]
    public void P3A_HelloWorld_SmallChariotDelete(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3) return;
        sa.Method.RemoveDraw($"P3A_HelloWorld_SmallChariot_{ev.TargetId}");
    }
    
    #endregion P3A Hello World

    #region P3B Small Screen

    [ScriptMethod(name: "P3B_SmallScreen_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31588"], userControl: Debugging)]
    public void P3B_Transition_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        // Use critical error cast bar to enter small screen early
        _parse = 3.1;
        _pd.Init(sa, "P3B Small Screen");
        _pd.AddPriorities([2, 3, 1, 7, 4, 5, 6, 7]);
    }
    
    [ScriptMethod(name: "P3B_SmallScreen_RecordMarkers", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:regex:^(0[12345678])$"],
        userControl: Debugging)]
    public void P3B_SmallScreen_RecordMarkers(Event ev, ScriptAccessory sa)
    {
        // Record Attack 12345, Bind 123
        if (_parse != 3.1) return;
        
        lock (_pd)
        {
            var mark = ev.Id0();
            var tidx = sa.GetPlayerIdIndex((uint)ev.TargetId);
            var targetJob = sa.GetPlayerJobByIndex(tidx);
            sa.DebugMsg($"Detected {targetJob} marked with ev.Id {mark}", Debugging);
            _pd.AddActionCount();
            var pdVal = mark switch
            {
                0x01 => 10,    // Attack 1
                0x02 => 20,    // Attack 2
                0x03 => 30,    // Attack 3
                0x04 => 40,    // Attack 4
                0x05 => 50,    // Attack 5
                0x06 => 110,   // Bind 1
                0x07 => 120,   // Bind 2
                0x08 => 130,   // Bind 3
                _ => 0
            };
            _pd.AddPriority(tidx, pdVal);
            if (_pd.ActionCount != 8) return;
            sa.DebugMsg($"P3B_SmallScreen_RecordMarkers: Markers recorded", Debugging);
            sa.DebugMsg($"{_pd.ShowPriorities()}", Debugging);
            _p3.SmallScreenMarkerRecord.Set();   // Marker record
        }
    }

    [ScriptMethod(name: "P3B_SmallScreen_BaldScanDirectionRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3159[56])$"],
        userControl: Debugging)]
    public void P3B_SmallScreen_BaldScanDirectionRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3.1) return;
        const uint CANNON_RIGHT = 31595;
        _p3.BaldSmallScreenDirectionRight = ev.ActionId == CANNON_RIGHT;
        _p3.BaldScanDirectionRecord.Set();
    }
    
    [ScriptMethod(name: "P3B_SmallScreen_ChariotRange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3159[56])$"],
        userControl: true)]
    public void P3B_SmallScreen_ChariotRange(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3.1) return;
        foreach (var member in sa.Data.PartyList)
            sa.DrawCircle(member, 4000, 6000, $"P3B_SmallScreen_ChariotRange", 7f);
    }
    
    [ScriptMethod(name: "P3B_SmallScreen_Guidance", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(345[23])$"],
        userControl: true, suppress: 1000)]
    public void P3B_SmallScreen_Guidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3.1) return;
        var hasMarker = _p3.SmallScreenMarkerRecord.WaitOne(2500);
        if (!hasMarker)
        {
            sa.DebugMsg($"Markers not detected within time, enable HTDH priority order", Debugging);
            for (int i = 0; i < sa.Data.PartyList.Count; i++)
            {
                var obj = sa.GetById(sa.Data.PartyList[i]);
                if (obj is null) continue;
                if (!((IBattleChara)obj).HasStatusAny([3452, 3453])) continue;
                _pd.AddPriority(i, 100);
            }
        }
        _p3.BaldScanDirectionRecord.WaitOne();
        
        // Shoot right/left safe, Attack 1-5, Bind 1-3
        List<Vector3> staticPos =
        [
            new(99.0f, 0, 91.0f), new(110.0f, 0, 100.0f), new(118.0f, 0, 100.0f), new(99.0f, 0, 109.0f), new(99.0f, 0, 119.0f),
            new(93.0f, 0, 82.0f), new(86.0f, 0, 92.5f), new(86.0f, 0, 107.5f)
        ];

        var myRank = _pd.FindPriorityIndexOfKey(sa.GetMyIndex());
        var myPos = staticPos[myRank];
        if (!_p3.BaldSmallScreenDirectionRight && myRank is 0 or 3 or 4 or 5)
            myPos = myPos.FoldPointHorizon(Center.X);
        sa.DrawGuidance(myPos, 0, 10000, $"P3B_SmallScreen_Guidance");
    }

    [ScriptMethod(name: "P3B_SmallScreen_FacingCalculation", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(345[23])$"], userControl: Debugging)]
    public void P3B_SmallScreen_FacingCalculation(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3.1) return;
        if (ev.TargetId != sa.Data.Me) return;
        const uint PLAYER_CANNON_RIGHT = 3452;
        
        _p3.BaldScanDirectionRecord.WaitOne();
        _p3.SmallScreenMarkerRecord.WaitOne(3000);

        int myBindRank = _pd.FindPriorityIndexOfKey(sa.GetMyIndex()) - 5;
        if (myBindRank < 0) return;

        // Based on bald shooting right, player shoots right
        var faceRegion = myBindRank switch
        {
            0 => 0,     // Bind 1 face down
            1 => 3,     // Bind 2 face left
            2 => 1,     // Bind 3 face right
            _ => -1
        };
        if (faceRegion < 0) return;
        faceRegion += (ev.StatusId != PLAYER_CANNON_RIGHT ? 2 : 0) + (myBindRank == 0 && !_p3.BaldSmallScreenDirectionRight ? 2 : 0);

        _p3.SmallScreenPlayerFacing = (faceRegion + 4) % 4;
        _p3.SmallScreenPlayerFacingRecord.Set();
    }
    
    [ScriptMethod(name: "P3B_SmallScreen_FacingArrowAuxiliaryDrawing", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(345[23])$"], userControl: true)]
    public void P3B_SmallScreen_FacingArrowAuxiliary(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3.1) return;
        if (ev.TargetId != sa.Data.Me) return;
        
        _p3.SmallScreenPlayerFacingRecord.WaitOne();
        DrawFacingArrow(sa, _p3.SmallScreenPlayerFacing, false, "P3B_SmallScreen_FacingArrowAuxiliary_Self");
        DrawFacingArrow(sa, _p3.SmallScreenPlayerFacing, true,  "P3B_SmallScreen_FacingArrowAuxiliary_Correct");
        return;

        void DrawFacingArrow(ScriptAccessory sa, int dir, bool isSupport, string name)
        {
            var dp = sa.DrawLine(sa.Data.Me, 0, 0, 10000, name,
                isSupport ? dir * 90f.DegToRad() : 0, 1f, 4.5f,
                isSafe: isSupport, draw: false);
            dp.FixRotation = isSupport;
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);
        }
    }
    
    [ScriptMethod(name: "*P3B_SmallScreen_AutomaticFacingAssist", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3159[56])$"], userControl: true)]
    public void P3B_SmallScreen_AutomaticFacingAssist(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3.1) return;
        if (!SpecialMode) return;
        
        // 1. Determine entry condition
        _p3.SmallScreenPlayerFacingRecord.WaitOne();
        int myBindRank = _pd.FindPriorityIndexOfKey(sa.GetMyIndex()) - 5;
        if (myBindRank < 0) return;
        
        var myObject = sa.Data.MyObject;
        if (myObject == null) return;
        
        // 2. Set correct facing
        int correctFaceDir = _p3.SmallScreenPlayerFacing;
        sa.DebugMsg($"Small screen player facing should be {correctFaceDir} ", Debugging);
        float correctFaceRotation = correctFaceDir * 90f.DegToRad();
        
        // 3. Activate trigger
        _p3.SmallScreenFacingAssistFramework = sa.Method.RegistFrameworkUpdateAction(Action);
        return;
        
        void Action()
        {
            var myRotation = myObject.Rotation;
            var rotationDiff = MathF.Abs(myRotation.GetDiffRad(correctFaceRotation));
            if (!sa.IsMoving() && rotationDiff > 0.1f && (DateTime.Now - _p3.SmallScreenFacingAssistTriggerTime).TotalMilliseconds > 250)
            {
                _p3.SmallScreenFacingAssistTriggerTime = DateTime.Now;
                sa.SetRotation(myObject, correctFaceRotation);
            }
        }
    }
    
    [ScriptMethod(name: "P3B_SmallScreen_ProcessingEnd", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3159[56])$"],
        userControl: Debugging)]
    public void P3B_SmallScreen_ProcessingEnd(Event ev, ScriptAccessory sa)
    {
        if (_parse != 3.1) return;
        _p3.SmallScreenMarkerRecord.Reset();
        _p3.BaldScanDirectionRecord.Reset();
        _p3.SmallScreenPlayerFacingRecord.Reset();
        sa.Method.RemoveDraw($"P3B_SmallScreen.*");
        sa.Method.UnregistFrameworkUpdateAction(_p3.SmallScreenFacingAssistFramework);
    }

    #endregion P3B Small Screen

    #region P4 Blue Screen

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP4 Blue Screenã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P4_BlueScreen_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P4_BlueScreen_PhaseSplit", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(31559)$"],
        userControl: Debugging)]
    public void P4_BlueScreen_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        _parse = 4;
        _p3.Reset(sa);
        _p3.Dispose();
        _p4.Register();
        _p4.BossId = ev.SourceId;
    }
    
    [ScriptMethod(name: "P4_BlueScreen_EachRoundWaveCannonInit", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3161[05])$"], userControl: Debugging, suppress: 1000)]
    public void P4_BlueScreen_EachRoundWaveCannonInit(Event ev, ScriptAccessory sa)
    {
        if (_parse != 4) return;
        sa.Method.RemoveDraw($"P4_BlueScreen_R{_p4.BlueScreenWaveCannonRound}.*");
        
        _p4.BlueScreenWaveCannonRound++;
        _pd.Init(sa, $"P4BlueScreenR{_p4.BlueScreenWaveCannonRound}");
        _pd.AddPriorities([1, 8, 3, 6, 4, 5, 2, 7]);
        sa.DebugMsg($"Now Blue Screen Wave Cannon Round {_p4.BlueScreenWaveCannonRound}", Debugging);
        _p4.WaveCannonInitRecord.Set();
    }
    
    [ScriptMethod(name: "P4_BlueScreen_WaveCannonSpreadGuidance", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3161[05])$"],
        userControl: true, suppress: 1000)]
    public void P4_BlueScreen_WaveCannonSpreadGuidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 4) return;
        _p4.WaveCannonInitRecord.WaitOne();
        
        lock (_p4)
        {
            if (_p4.BlueScreenWaveCannonRound > 3) return;
            List<int> partySpreadRegion = [11, 5, 13, 3, 14, 2, 12, 4];
            var pos = new Vector3(100, 0, 114f).RotateAndExtend(Center, partySpreadRegion[sa.GetMyIndex()] * 22.5f.DegToRad());
            sa.DrawGuidance(pos, 0, 5000, $"P4_BlueScreen_WaveCannonSpreadGuidance");
            for (int i = 0; i < 8; i++)
            {
                var dp = sa.DrawLine(Center, 0, 0, 10000, $"P4_BlueScreen_WaveCannonGuideLine{i}", partySpreadRegion[i] * 22.5f.DegToRad(), 20f, 20f, draw: false);
                dp.Color = i switch
                {
                    0 or 1 => new Vector4(0.1f, 0.1f, 1, 1),
                    2 or 3 => new Vector4(0.1f, 1f, 0.1f, 1),
                    _ => new Vector4(1, 0.1f, 0.1f, 1),
                };
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Line, dp);
            }
        }
    }
    
    [ScriptMethod(name: "*P4_BlueScreen_WaveCannonSpreadGuidanceRemoval", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31614"], userControl: Debugging)]
    public void P4_BlueScreen_WaveCannonSpreadGuidanceRemoval(Event ev, ScriptAccessory sa)
    {
        if (_parse != 4) return;
        sa.Method.RemoveDraw($"P4_BlueScreen_WaveCannon.*");
        _p4.WaveCannonInitRecord.Reset();
    }
    
    [ScriptMethod(name: "P4_BlueScreen_Quake", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3156[789]|31570)$", "TargetIndex:1"],
        userControl: true)]
    public void P4_BlueScreen_Quake(Event ev, ScriptAccessory sa)
    {
        if (_parse != 4) return;
        const uint CHARIOT = 31567, DONUT_1 = 31568, DONUT_2 = 31569, DONUT_3 = 31570;

        (int outScale, int innerScale, int donutCount) = ev.ActionId switch
        {
            CHARIOT => (12, 6, 1),
            DONUT_1 => (18, 12, 2),
            DONUT_2 => (24, 18, 3),
            _ => (0, 0, 4)
        };

        string prefix = "P4_BlueScreen_QuakeDonut";
        sa.Method.RemoveDraw($"{prefix}{donutCount-1}");
        if (donutCount == 4) return;
        
        var dp = sa.DrawDonut(Center, 0, 10000, $"{prefix}{donutCount}", outScale, innerScale, draw: false);
        dp.Color = sa.Data.DefaultDangerColor.WithW(2.5f);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "P4_BlueScreen_WaveCannonStackTargetRecord", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(22393)$"], userControl: Debugging)]
    public void P4_BlueScreen_WaveCannonStackTargetRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 4) return;
        lock (_pd)
            _pd.AddPriority(sa.GetPlayerIdIndex((uint)ev.TargetId), 10);
    }
    
    [ScriptMethod(name: "P4_BlueScreen_StackDrawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31614"], userControl: true, suppress: 1000)]
    public void P4_BlueScreen_StackDrawing(Event ev, ScriptAccessory sa)
    {
        if (_parse != 4) return;
        
        var myPriRank = _pd.FindPriorityIndexOfKey(sa.GetMyIndex());
        var mySafeBool = myPriRank is 0 or 1 or 2 or 6 ? 0 : 1;
        for (int i = 0; i < 2; i++)
        {
            var targetIdx = _pd.SelectSpecificPriorityIndex(i+6).Key;
            sa.DrawRect(_p4.BossId, sa.Data.PartyList[targetIdx], 0, 10000,
                $"P4_BlueScreen_R{_p4.BlueScreenWaveCannonRound}_StackDrawing", 0, 6, 50, isSafe: i == mySafeBool);
        }
    }
    
    [ScriptMethod(name: "P4_BlueScreen_StackGuidance", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31614"], userControl: true, suppress: 1000)]
    public void P4_BlueScreen_StackGuidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 4) return;
        sa.DrawLine(Center, 0, 0, 6000, $"P4_BlueScreen_R{_p4.BlueScreenWaveCannonRound}_StackGuideLine1", 20f.DegToRad(), 20f, 20f, isSafe: true, draw: true);
        sa.DrawLine(Center, 0, 0, 6000, $"P4_BlueScreen_R{_p4.BlueScreenWaveCannonRound}_StackGuideLine2", -20f.DegToRad(), 20f, 20f, isSafe: true, draw: true);

        sa.DebugMsg(_pd.ShowPriorities(), Debugging);
        
        var myPriRank = _pd.FindPriorityIndexOfKey(sa.GetMyIndex());
        var myRegion = myPriRank is 0 or 1 or 2 or 6 ? -1 : 1;
        var extend = 0;     // TODO, stack positions vary by round
        var myPos = new Vector3(100, 0, 114).RotateAndExtend(Center, myRegion * 20f.DegToRad(), extend); 
        sa.DrawGuidance(myPos, 0, 10000, $"P4_BlueScreen_R{_p4.BlueScreenWaveCannonRound}_StackGuidance");
    }

    [ScriptMethod(name: "P4_BlueScreen_SecondWaveCannon", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31616"], userControl: true)]
    public void P4_BlueScreen_SecondWaveCannon(Event ev, ScriptAccessory sa)
    {
        if (_parse != 4) return;
        sa.DrawRect(ev.SourceId, 0, 10000, $"P4_BlueScreen_SecondWaveCannon", 0, 6, 50);
    }
    
    [ScriptMethod(name: "P4_BlueScreen_SecondWaveCannonRemoval", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31616"], userControl: Debugging, suppress: 1000)]
    public void P4_BlueScreen_SecondWaveCannonRemoval(Event ev, ScriptAccessory sa)
    {
        if (_parse != 4) return;
        sa.Method.RemoveDraw($"P4_BlueScreen_SecondWaveCannon");
    }
    
    [ScriptMethod(name: "*P4_BlueScreen_HideFieldDonutEffects", eventType: EventTypeEnum.ActionEffect,
        eventCondition: ["ActionId:regex:^(3156[689]|31570)$", "TargetIndex:1"], userControl: true)]
    public void P4_BlueScreen_HideEffects(Event ev, ScriptAccessory sa)
    {
        if (_parse != 4) return;
        if (!SpecialMode) return;
        sa.WriteVisible(sa.GetById((uint)ev.SourceId), false);
    }
    
    [ScriptMethod(name: "*P4_BlueScreen_HideFirstWaveCannonEffects", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31614"], userControl: true)]
    public void P4_BlueScreen_HideFirstWaveCannonEffects(Event ev, ScriptAccessory sa)
    {
        if (_parse != 4) return;
        if (!SpecialMode) return;
        sa.WriteVisible(sa.GetById(ev.SourceId), false);
    }
    
    [ScriptMethod(name: "*P4_BlueScreen_HideSecondWaveCannonEffects", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31616"], userControl: true)]
    public void P4_BlueScreen_HideSecondWaveCannonEffects(Event ev, ScriptAccessory sa)
    {
        if (_parse != 4) return;
        if (!SpecialMode) return;
        sa.WriteVisible(sa.GetById(ev.SourceId), false);
    }
    
    [ScriptMethod(name: "*P4_BlueScreen_HideStackWaveCannonEffects", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31615"], userControl: true)]
    public void P4_BlueScreen_HideStackWaveCannonEffects(Event ev, ScriptAccessory sa)
    {
        if (_parse != 4) return;
        if (!SpecialMode) return;
        sa.WriteVisible(sa.GetById(ev.SourceId), false);
    }
    
    #endregion P4 Blue Screen

    #region P5A Mechanic 1 / First Debuff Transfer
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP5A1 Mechanic 1ã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P5_Mechanic1_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P5_Opening_PhaseSplit", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31621"],
        userControl: Debugging)]
    public void P5_Opening_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        _parse = 5.0;
    }

    [ScriptMethod(name: "P5A1_Mechanic1_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31624"],
        userControl: Debugging)]
    public void P5A1_Mechanic1_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        _parse = 5.1;
        
        _p4.Reset(sa);
        _p4.Dispose();
        _p5A.Register();
        
        ResetSupportUnitVisibility(sa);
        _pd.Init(sa, "P5 Mechanic 1");
        _pd.AddPriorities([0, 1, 2, 3, 4, 5, 6, 7]);    // Add priority based on role order
        sa.DebugMsg($"Current phase: {_parse}", Debugging);
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_EyeLaser", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375AC", "Id:00020001"])]
    public void P5A1_Mechanic1_EyeLaser(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        var rot = ev.Index() - 1;
        var basePos = new Vector3(100, 0, 80);
        var eyePos = basePos.RotateAndExtend(Center, -45f.DegToRad() * rot);
        sa.DrawRect(eyePos, Center, 7500, 12500, "P5A1_Mechanic1_EyeLaser", 0, 16, 40);
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_EyeLaserDelete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31521"], userControl: Debugging)]
    public void P5A1_Mechanic1_EyeLaserDelete(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        sa.Method.RemoveDraw($"P5A1_Mechanic1_EyeLaser");
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_FarTetherRecord", eventType: EventTypeEnum.Tether, eventCondition: ["Id:00C9"], userControl: Debugging)]
    public void P5A1_Mechanic1_FarTetherRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;

        lock (_pd)
        {
            var targetId = ev.TargetId;
            var targetIdx = sa.GetPlayerIdIndex((uint)targetId);
            var sourceId = ev.SourceId;
            var sourceIdx = sa.GetPlayerIdIndex((uint)sourceId);
            var pdValMax = _pd.SelectSpecificPriorityIndex(0, true).Value;
            
            // Before adding priority value, get the max value in the priority dictionary (i.e., check if already added), add +1000/+2000 to the two far tether partners
            _pd.AddPriority(targetIdx, pdValMax >= 1000 ? 2000 : 1000);
            _pd.AddPriority(sourceIdx, pdValMax >= 1000 ? 2000 : 1000);
            
            _p5A.FarTetherPartnerRecord.Set();
        }
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_RecordMarkers", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:regex:^(0[123467])$"],
        userControl: Debugging)]
    public void P5A1_Mechanic1_RecordMarkers(Event ev, ScriptAccessory sa)
    {
        // Record only Attack 1234 and Bind 12
        if (_parse != 5.1) return;
        
        lock (_pd)
        {
            var mark = ev.Id0();
            var tidx = sa.GetPlayerIdIndex((uint)ev.TargetId);
            var targetJob = sa.GetPlayerJobByIndex(tidx);
            sa.DebugMsg($"Detected {targetJob} marked with ev.Id {mark}", Debugging);
            _pd.AddActionCount();
            var pdVal = mark switch
            {
                1 => 10,    // Attack 1
                2 => 20,    // Attack 2
                3 => 30,    // Attack 3
                4 => 40,    // Attack 4
                6 => 100,   // Bind 1
                7 => 200,   // Bind 2
                _ => 0
            };
            _pd.AddPriority(tidx, pdVal);
            if (_pd.ActionCount != 6) return;
            sa.DebugMsg($"P5_Mechanic1_RecordMarkers: Markers recorded", Debugging);
            sa.DebugMsg($"{_pd.ShowPriorities()}", Debugging);
            _p5A.MarkerRecord.Set();   // Marker record
        }
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_LocateBald", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["SourceDataId:14669", "Id:7747"],
        userControl: Debugging)]
    public void P5A1_Mechanic1_LocateBald(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        var spos = ev.SourcePosition;
        var dir = spos.GetRadian(Center).RadianToRegion(4, isDiagDiv: true);
        _p5A.BaldPosition = dir;
        _p5A.BeetlePosition = (dir + 2) % 4;
        sa.DebugMsg($"Mechanic 1 Bald Position: {_p5A.BaldPosition}, Mechanic 1 Beetle Position: {_p5A.BeetlePosition}", Debugging);
        _p5A.BaldBeetleLocate.Set();
    } 
    
    [ScriptMethod(name: "P5A1_Mechanic1_InitialPositionGuidance", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["SourceDataId:14669", "Id:7747"],
        userControl: true)]
    public void P5A1_Mechanic1_InitialPositionGuidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        
        _p5A.FarTetherPartnerRecord.WaitOne(5000);
        _p5A.MarkerRecord.WaitOne(2000);
        _p5A.BaldBeetleLocate.WaitOne(2000);
        
        var myIndex = sa.GetMyIndex();
        var myPdVal = _pd.Priorities[myIndex];
        
        // Far tether unmarked player automatically matches partner and corrects priority
        if (IsFarLineWithoutMarker(myPdVal))
        {
            int partnerIndex = FindFarLinePartnerIndex(myPdVal);
            int partnerChain = _pd.Priorities[partnerIndex].GetDecimalDigit(3); // 1 or 2
            _pd.AddPriority(myIndex, partnerChain * 100 + 10);  // 11 or 21
            sa.DebugMsg($"Far tether unmarked player found partner: {sa.GetPlayerJobByIndex(partnerIndex)}", Debugging);
        }
        
        // 2. Unify priority markerVal
        for (int i = 0; i < 8; i++)
            _pd.Priorities[i] = (_pd.Priorities[i] % 1000) / 10;

        int markerVal = _pd.Priorities[myIndex];
        sa.DebugMsg($"After correction, markerVal = {markerVal}", Debugging);
        
        // 3. Calculate standby points
        Vector3 wait1 = CalcWaitPoint(markerVal, isLeft: true );
        Vector3 wait2 = CalcWaitPoint(markerVal, isLeft: false);

        // 4. Draw guidance
        sa.DrawGuidance(wait1, 0, 5000, "P5A1_Mechanic1_StandbyLocation1");
        sa.DrawGuidance(wait2, 0, 5000, "P5A1_Mechanic1_StandbyLocation2");

        // 5. Reset semaphores
        _p5A.FarTetherPartnerRecord.Reset();
        _p5A.MarkerRecord.Reset();
        _p5A.BaldBeetleLocate.Reset();
        
        return;

        bool IsFarLineWithoutMarker(int pd) => pd >= 1000 && pd.GetDecimalDigit(3) == 0;
        
        int FindFarLinePartnerIndex(int myPd)
        {
            // Thousands digit 1 -> find descending 3rd (index 2); thousands digit 2 -> find descending 1st (index 0)
            int thousand = myPd / 1000;
            return _pd.SelectSpecificPriorityIndex(thousand == 2 ? 0 : 2, true).Key;
        }
        
        Vector3 CalcWaitPoint(int marker, bool isLeft)
        {
            Vector3 raw = isLeft ? new Vector3(90f, 0, 106f) : new Vector3(110f, 0, 106f);

            bool needBack  = marker is 3 or 4 or 20 or 21;      // Outer group
            bool useOmega  = marker is 10 or 11 or 20 or 21;    // Far tether group
            int  dir       = useOmega ? _p5A.BaldPosition : _p5A.BeetlePosition;

            if (needBack) raw += new Vector3(0, 0, 8);
            return raw.RotateAndExtend(Center, 90f.DegToRad() * dir);
        }
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_RecordFists", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(157(09|10))$"], userControl: Debugging)]
    public void P5A1_Mechanic1_RecordFists(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        
        const uint BLUE_ROCKET  = 15709;
        // const uint YELLOW_ROCKET = 15710;

        var myPos = sa.Data.MyObject.Position;
        
        // 1. Calculate player's quarter half
        if (_p5A.PlayerQuarterHalf < 0)
            _p5A.PlayerQuarterHalf = myPos.GetRadian(Center).RadianToRegion(4, isDiagDiv: false);
        
        // 2. Determine fist quarter half
        int mobRegion = ev.SourcePosition.GetRadian(Center).RadianToRegion(4, isDiagDiv: false);
        if (mobRegion != _p5A.PlayerQuarterHalf) return;
        
        // 3. Count
        lock (_pd)
        {
            _pd.AddActionCount();
            _p5A.FistCount++;
            _p5A.FistColor += ev.DataId() == BLUE_ROCKET ? 1 : -1;

            sa.DebugMsg($"Player half spawned {_p5A.FistCount}th fist, " + $"color {(ev.DataId() == BLUE_ROCKET ? "Blue" : "Yellow")}, " + $"cumulative color value={_p5A.FistColor}", Debugging);
        }
        
        // 4. All 14 adds (6 markers + 8 fists) spawned
        if (_pd.ActionCount != 14) return;
        _p5A.FistRecord.Set();
        sa.Method.RemoveDraw($"P5A1_Mechanic1_StandbyLocation.*");
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_FistStandbyGuidance", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(157(09|10))$"],
        userControl: true, suppress: 10000)]
    public void P5A1_Mechanic1_FistStandbyGuidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        _p5A.FistRecord.WaitOne(2000);
        try
        {
            var myIndex = sa.GetMyIndex();
            var markerVal = _pd.Priorities[myIndex];
        
            var isOutside = markerVal is 3 or 4 or 20 or 21;    // Attack 3, Attack 4, Bind 2, Stop 2
            var isRemoteTetherOutside = markerVal is 20 or 21;  // Bind 2, Stop 2
        
            // var myQuadrant = _p5A.PlayerQuarterHalf;
            var punchCountAtMyQuadrant = _p5A.FistCount;
            var punchColorAtMyQuadrant = _p5A.FistColor;
        
            // 1. Inside players, no swap, stand at quadrant point
            if (!isOutside)
            {
                // isRemoteTetherOutside determines whether the player is near the far tether group (Omega) or near tether group (Beetle)
                Vector3 pos = GetQuadrantPoint(isRemoteTetherOutside, _p5A.BaldPosition, _p5A.PlayerQuarterHalf);
                sa.DrawGuidance(pos, 0, 5000, $"P5A1_Mechanic1_Fist");
                return;
            }
        
            // 2. Incorrect fist count, don't draw
            if (punchCountAtMyQuadrant != 2)
            {
                // Missing player, don't give specific coordinates
                sa.Method.TextInfo("Observe same group fist color for swap", 4000, true);
                return;
            }
        
            // 3. Check if fists are same color, mirror swap point
            bool needSwap = punchColorAtMyQuadrant != 0;
            Vector3 final = GetQuadrantPoint(isRemoteTetherOutside, _p5A.BaldPosition, _p5A.PlayerQuarterHalf);
            if (needSwap)
                final = MirrorAcrossBoss(final, _p5A.BaldPosition);
            sa.DrawGuidance(final, 0, 5000, $"P5A1_Mechanic1_Fist");
        }
        finally
        {
            _p5A.FistRecord.Reset();
        }
        return;
        
        Vector3 GetQuadrantPoint(bool isRemoteTetherOutside, int omegaDir, int myQuadrant)
        {
            // All coordinates are based on quadrant 0 (bottom right), later mirrored by quadrant
            Vector3 raw = isRemoteTetherOutside ? new Vector3(102.7f, 0, 110f) : new Vector3(108.7f, 0, 110f);

            // Rotate overall -90Â° clockwise when Boss is in odd direction
            if (omegaDir % 2 == 1)
                raw = raw.RotateAndExtend(new Vector3(109.9f, 0, 110f), -90f.DegToRad());
            
            raw = myQuadrant switch
            {
                1 => raw.FoldPointVertical(Center.Z),
                2 => raw.PointCenterSymmetry(Center),
                3 => raw.FoldPointHorizon(Center.X),
                _ => raw,
            };

            return raw;
        }
        
        Vector3 MirrorAcrossBoss(Vector3 pos, int omegaDir)
        {
            return omegaDir % 2 == 0
                ? pos.FoldPointHorizon(Center.X)
                : pos.FoldPointVertical(Center.Z);
        }
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_LaserArmRotationGuidePosition", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(009[CD])$"], userControl: true)]
    public void P5A1_Mechanic1_LaserArmRotationGuidePosition(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        const int ICON_ROTATE_CW  = 156;   // 009C

        lock (_p5A.LaserArmDirectionDict)
        {
            bool    isCw    = ev.Id0() == ICON_ROTATE_CW;
            ulong   tid     = ev.TargetId;
            Vector3 tpos    = sa.GetById(tid)?.Position ?? Center;
            Vector3 baitPos = tpos.RotateAndExtend(Center, (isCw ? 5f : -5f).DegToRad(), -1f);
            int     region  = tpos.GetRadian(Center).RadianToRegion(12, isDiagDiv: true);
        
            sa.DebugMsg($"Added to LaserArmDirectionDict: Key: {region} / 12, Value: {baitPos}", Debugging);
            _p5A.LaserArmDirectionDict.TryAdd(region, baitPos);
        
            var dp = sa.DrawCircle(baitPos, 0, 10000, $"P5A1_Mechanic1_LaserArmRotationGuidePosition", 0.5f, isSafe: true, draw: false);
            dp.Color = sa.Data.DefaultSafeColor.WithW(3f);
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_PlayerGuideLaserArmGuidance", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31587"], userControl: true)]
    public void P5A1_Mechanic1_PlayerGuideLaserArmGuidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        if (ev.TargetId != sa.Data.Me) return;
        
        // This check might be returned due to suppress, so pay attention to suppress usage scenarios. Ensure event can be triggered. Otherwise, use bool.
        if (_p5A.FirstFarTetherBroken) return;
        _p5A.FirstFarTetherBroken = true;
        _p5A.EnablePlayerGuideLaserArmGuidance = true;
        
        // 1. Basic data
        int     myIndex = sa.GetMyIndex();
        int     marker  = _pd.Priorities[myIndex];
        Vector3 myPos   = ev.TargetPosition;
        
        bool isShieldTarget = marker is 10 or 11;               // Bind 1, Stop 1, need to be shield bashed towards center
        bool isOutside      = marker is 3 or 4 or 20 or 21;     // Attack 3, Attack 4, Bind 2, Stop 2, towards outside
        bool isBind         = marker is 10 or 20 or 11 or 21;   // Bind 1, Bind 2, Stop 1, Stop 2
        
        // 2. Calculate Bald's 12-direction and arm number
        int     omega12  = _p5A.BaldPosition * 3;          // 0~11
        int     armUnit  = CalcArmUnit(omega12, isOutside, isBind, myPos, Center);
        _p5A.PlayerGuideLaserArmDirection = armUnit;
        
        Vector3 guidePos = isShieldTarget
            ? new Vector3(100, 0, 105).RotateAndExtend(Center, armUnit * 30f.DegToRad())
            : _p5A.LaserArmDirectionDict[armUnit];

        sa.DrawGuidance(guidePos, 0, 4000, isShieldTarget ? "P5A1_Mechanic1_PlayerGuideLaserArmGuidance_Center" : "P5A1_Mechanic1_PlayerGuideLaserArmGuidance_Fist", isSafe: false);
        return;
        
        int CalcArmUnit(int omega12, bool outside, bool bind, Vector3 myPos, Vector3 center)
        {
            Vector3 omegaPos = new Vector3(100, 0, 120).RotateAndExtend(center, omega12 * 30f.DegToRad());
            bool isRight = IsAtRight(omegaPos, myPos, center);
            // 3-bit encoding
            int code = (outside ? 4 : 0) | (isRight ? 2 : 0) | (bind ? 1 : 0);

            return code switch
            {
                0b111 => (omega12 + 1)  % 12,
                0b110 => (omega12 + 5)  % 12,
                0b101 => (omega12 + 11) % 12,
                0b100 => (omega12 + 7)  % 12,
                0b010 => (omega12 + 3)  % 12,
                0b000 => (omega12 + 9)  % 12,
                
                0b011 => (omega12 + 3)  % 12,
                0b001 => (omega12 + 9)  % 12,
                _     => -1
            };
        }

        bool IsAtRight(Vector3 posReference, Vector3 posTarget, Vector3 posCenter) =>
            posTarget.GetRadian(posCenter).GetDiffRad(posReference.GetRadian(posCenter)) > 0;
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_PlayerGuideLaserArmGuidanceRefresh", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31482"],
        userControl: Debugging, suppress: 10000)]
    public void P5A1_Mechanic1_PlayerGuideLaserArmGuidanceRefresh(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        sa.Method.RemoveDraw($"P5A1_Mechanic1_PlayerGuideLaserArmGuidance.*");
            
        if (!_p5A.EnablePlayerGuideLaserArmGuidance) return;
            
        var myIndex = sa.GetMyIndex();
        var markerVal = _pd.Priorities[myIndex];
        var isShieldTarget = markerVal is 10 or 11; 
        if (isShieldTarget) return;
            
        sa.DebugMsg($"Player guide laser arm direction: {_p5A.PlayerGuideLaserArmDirection} / 12", Debugging);
        Vector3 armUnitPos = _p5A.LaserArmDirectionDict[_p5A.PlayerGuideLaserArmDirection];
        
        sa.DrawGuidance(armUnitPos, 0, 4000, $"P5A1_Mechanic1_GuideFist");
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_RotatingArmGuideGuidanceDelete", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31600"],
        userControl: Debugging, suppress: 10000)]
    public void P5A1_Mechanic1_RotatingArmGuideGuidanceDelete(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        sa.Method.RemoveDraw($"P5A1_Mechanic1_GuideFist");
    }
        
    [ScriptMethod(name: "P5A1_Mechanic1_PlayerCenterShieldGuidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31482"],
        userControl: true, suppress: 10000)]
    public void P5A1_Mechanic1_PlayerCenterShieldGuidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        var myIndex = sa.GetMyIndex();
        var markerVal = _pd.Priorities[myIndex];
            
        if (markerVal is not 10 and not 11) return;
        var myArmUnit = _p5A.PlayerGuideLaserArmDirection;
            
        var centerBiasPos = new Vector3(100, 0, 105).RotateAndExtend(Center, myArmUnit * 30f.DegToRad());
        sa.DrawGuidance(centerBiasPos, 0, 3000, $"P5A1_Mechanic1_CenterShieldComboGuidance");
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_NearTetherStandbyGuidanceAfterRotatingArm", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31600"],
        userControl: true, suppress: 10000)]
    public void P5A1_Mechanic1_NearTetherStandbyGuidanceAfterRotatingArm(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        var myIndex = sa.GetMyIndex();
        var markerVal = _pd.Priorities[myIndex];

        if (markerVal is not 1 and not 2 and not 3 and not 4) return;
        var myArmUnit = _p5A.PlayerGuideLaserArmDirection;
        
        var standByPos = new Vector3(100, 0, 114).
            RotateAndExtend(Center, MathF.Round((float)myArmUnit * 2 / 3) * 45f.DegToRad());
        sa.DrawGuidance(standByPos, 0, 6000, $"P5A1_Mechanic1_NearTetherStandbyGuidanceAfterRotatingArm");
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_BaldLeftRightScanRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3163[89])$"],
        userControl: Debugging)]
    public void P5A1_Mechanic1_BaldLeftRightScanRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        const uint OMEGA_RIGHT_CANNON = 31638;
        _p5A.BaldLeftRightScan = ev.ActionId == OMEGA_RIGHT_CANNON ? 1 : 2;
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_PlayerSmallScreenBuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(345[23])$"],
        userControl: Debugging)]
    public void P5A1_Mechanic1_PlayerSmallScreenBuffRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        var tidx = sa.GetPlayerIdIndex((uint)ev.TargetId);
        _pd.AddPriority(tidx, 100);    // Small screen marker +100
        const uint PLAYER_RIGHT_CANNON = 3452;
        _p5A.PlayerLeftRightScan = ev.StatusId == PLAYER_RIGHT_CANNON ? 1 : 2; // Right 1, Left 2
    }
        
    [ScriptMethod(name: "P5A1_Mechanic1_ShieldComboTargetRecord", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31528", "TargetIndex:1"],
        userControl: Debugging)]
    public void P5A1_Mechanic1_ShieldComboTargetRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        sa.Method.RemoveDraw($"P5A1_Mechanic1_CenterShieldComboGuidance");
        var tidx = sa.GetPlayerIdIndex((uint)ev.TargetId);
        _pd.AddPriority(tidx, 1000);    // Shield Combo target +1000
        _p5A.ShieldComboRecord.Set();
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_StackAndSmallScreenGuidance", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31528"],
        userControl: true)]
    public void P5A1_Mechanic1_StackAndSmallScreenGuidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        _p5A.ShieldComboRecord.WaitOne(1000);

        try
        {
            int myIndex   = sa.GetMyIndex();
            int myPriVal  = _pd.Priorities[myIndex];
            int markerVal = myPriVal % 100;          // Marker only
            
            // 1. Near tether group exits
            if (markerVal is 1 or 2 or 3 or 4) return;
            
            // 2. Same target? Small screen marker +100, Shield Combo +1000
            bool isSameTarget = _pd.SelectSpecificPriorityIndex(0, true).Value >= 1100;
            
            // 3. Calculate base coordinates (based on Bald at C, right sword)
            Vector3 shieldPos = new(99f + (isSameTarget ? -3.5f : 0), 0, 115f);
            Vector3 stackPos  = new(99f + (isSameTarget ? 0 : -3.5f), 0, 100f);
            sa.DebugMsg($"Stack position: {stackPos}, Shield Combo position: {shieldPos}", Debugging);
            
            // 4. Left/Right sword mirroring
            if (_p5A.BaldLeftRightScan == 2)   // 1 Right, 2 Left
            {
                shieldPos = shieldPos.FoldPointHorizon(Center.X);
                stackPos  = stackPos.FoldPointHorizon(Center.X);
            }

            // 5. Rotate to Bald's current direction
            float rotRad = _p5A.BaldPosition * 90f.DegToRad();
            shieldPos = shieldPos.RotateAndExtend(Center, rotRad);
            stackPos  = stackPos.RotateAndExtend(Center, rotRad);
            
            // 6. Draw guidance
            bool isShield = myPriVal / 1000 == 1;
            sa.DrawGuidance(isShield ? shieldPos : stackPos, 0, 5000, isShield ? "P5A1_Mechanic1_ShieldComboGuidance" : "P5A1_Mechanic1_StackGuidance");

            // 7. Draw small screen facing assist
            bool isTv = myPriVal % 1000 >= 100;
            if (isTv)
            {
                int faceDir = (_p5A.BaldPosition + (_p5A.BaldLeftRightScan != _p5A.PlayerLeftRightScan ? 2 : 0)) % 4;
                DrawFacingArrow(sa, faceDir, true,  "P5A1_Mechanic1_SmallScreenFacingAssist_Correct");
                DrawFacingArrow(sa, faceDir, false, "P5A1_Mechanic1_SmallScreenFacingAssist_Self");
                sa.Method.TextInfo("Small Screen, stand outside", 3000, true);
            }
            else
            {
                sa.Method.TextInfo("Avoid small screen, stand inside", 3000, true);
            }
            
        }
        finally
        {
            _p5A.ShieldComboRecord.Reset();
        }
        return;
        
        void DrawFacingArrow(ScriptAccessory sa, int dir, bool isSupport, string name)
        {
            var dp = sa.DrawLine(sa.Data.Me, 0, 0, 5000, name,
                isSupport ? dir * 90f.DegToRad() : 0, 1f, 4.5f,
                isSafe: isSupport, draw: false);
            dp.FixRotation = isSupport;
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);
        }
    }

    [ScriptMethod(name: "*P5A1_Mechanic1_SmallScreenAutoFacingAssist", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3163[89])$"], userControl: true)]
    public void P5A1_Mechanic1_SmallScreenAutoFacingAssist(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        if (!SpecialMode) return;
        
        // 1. Determine entry condition
        int  myIndex  = sa.GetMyIndex();
        int  myPriVal = _pd.Priorities[myIndex];
        bool isTv     = myPriVal % 1000 >= 100;
        if (!isTv) return;
        
        var myObject = sa.Data.MyObject;
        if (myObject == null) return;
        
        // 2. Set correct facing
        int correctFaceDir = (_p5A.BaldPosition + (_p5A.BaldLeftRightScan != _p5A.PlayerLeftRightScan ? 2 : 0)) % 4;
        float correctFaceRotation = correctFaceDir * 90f.DegToRad();
        
        // 3. Activate trigger
        _p5A.SmallScreenFacingAssistFramework = sa.Method.RegistFrameworkUpdateAction(Action);
        return;
        
        void Action()
        {
            var myRotation = myObject.Rotation;
            var rotationDiff = MathF.Abs(myRotation.GetDiffRad(correctFaceRotation));
            if (!sa.IsMoving() && rotationDiff > 0.1f && (DateTime.Now - _p5A.SmallScreenFacingAssistTriggerTime).TotalMilliseconds > 250)
            {
                _p5A.SmallScreenFacingAssistTriggerTime = DateTime.Now;
                sa.SetRotation(myObject, correctFaceRotation);
            }
        }
    }

    [ScriptMethod(name: "P5A1_Mechanic1_SmallScreenAutoFacingAssistOff", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3163[89])$"], userControl: Debugging)]
    public void P5A1_Mechanic1_SmallScreenAutoFacingAssistOff(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        sa.Method.UnregistFrameworkUpdateAction(_p5A.SmallScreenFacingAssistFramework);
    }
    
    [ScriptMethod(name: "P5A1_Mechanic1_ClearDrawingPrepareFirstTransfer", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(31529)$"],
        userControl: Debugging)]
    public void P5A1_Mechanic1_ClearDrawingPrepareFirstTransfer(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.1) return;
        _parse = 5.15;
        sa.Method.RemoveDraw("P5A1_Mechanic1.*");
            
        for (int i = 0; i < 8; i++)
        {
            _pd.Priorities[i] %= 100;    // Keep units and tens, delete small screen and shield combo records
        }
        sa.DebugMsg($"First Transfer: After correction, {_pd.ShowPriorities()}", Debugging);
    }
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP5A2 First Transferã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P5A2_FirstTransfer_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P5A2_FirstTransfer_BeetleLeftRightSwordRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3163[67])$"],
        userControl: Debugging)]
    public void P5A2_FirstTransfer_BeetleLeftRightSwordRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.15) return;
        const uint BEETLE_RIGHT_CLEAVE = 31636;
        _p5A.BeetleLeftRightSword = ev.ActionId == BEETLE_RIGHT_CLEAVE ? 1 : 2;
        _p5A.BeetleLeftRightSwordRecord.Set();
    }
        
    [ScriptMethod(name: "P5A2_FirstTransfer_BeetleLeftRightSword", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3163[67])$"],
        userControl: true)]
    public void P5A2_FirstTransfer_BeetleLeftRightSword(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.15) return;
        const uint BEETLE_RIGHT_CLEAVE = 31636;
        var rot = ev.ActionId == BEETLE_RIGHT_CLEAVE ? -float.Pi / 2 : float.Pi / 2;
        sa.DrawFan(ev.SourceId, 0, 10000, $"P5A2_FirstTransfer_BeetleLeftRightSword", 210f.DegToRad(), rot, 90, 0);
    }
    
    [ScriptMethod(name: "P5A2_FirstTransfer_Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3163[67])$"],
    userControl: true)]
    public void P5A25_FirstTransfer_Guidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.15) return;
        _p5A.BeetleLeftRightSwordRecord.WaitOne();
        try
        {
            int myIndex = sa.GetMyIndex();
            int marker  = _pd.Priorities[myIndex];
            
            // 1. Configuration table: marker -> coordinates (based on Beetle right sword, Beetle at C)
            // Only modify this section
            Vector3[] basePos =
            {
                new(98f,   0, 119f),    // 1   Atk1   FarTarget
                new(89.4f, 0, 83.8f),   // 2   Atk2   FarTarget
                new(91.1f, 0, 111.1f),  // 3   Atk3   NearTarget
                new(86.3f, 0, 113.7f),  // 4   Atk4   NearTarget
                new(80.5f, 0, 100f),    // 10  Bind1  FarSource
                new(93.5f, 0, 100f),    // 20  Bind2  NearSource
                new(83.8f, 0, 89f),     // 11  Stop1  Idle
                new(83.8f, 0, 89f)      // 21  Stop2  Idle
            };
            
            // 2. Automatic mapping: marker to index
            List<int> map = [1, 2, 3, 4, 10, 20, 11, 21]; // One-to-one correspondence with basePos index
            int idx = map.IndexOf(marker);
            if (idx < 0)
            {
                sa.DebugMsg($"Failed to read player marker info {marker}", Debugging);
                return;
            }
            
            Vector3 pos = basePos[idx];
            
            const int BEETLE_RIGHT_CLEAVE_RECORD = 1;
            // 3. Left/Right sword mirroring
            if (_p5A.BeetleLeftRightSword != BEETLE_RIGHT_CLEAVE_RECORD)          // 1 Right, 2 Left
                pos = pos.FoldPointHorizon(Center.X);

            // 4. Direction rotation
            pos = pos.RotateAndExtend(Center, _p5A.BeetlePosition * 90f.DegToRad());
            sa.DrawGuidance(pos, 0, 5000, "P5A2_FirstTransfer_Guidance");
        }
        finally
        {
            _p5A.BeetleLeftRightSwordRecord.Reset();
        }
    }
    
    [ScriptMethod(name: "P5A2_FirstTransfer_GuidanceRemoval", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3163[67])$"],
        userControl: Debugging)]
    public void P5A2_FirstTransfer_GuidanceRemoval(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.15) return;
        sa.Method.RemoveDraw("P5A2_FirstTransfer.*");
    }
    
    #endregion P5A Mechanic 1 / First Debuff Transfer

    #region P5B Mechanic 2 / Second Transfer

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP5B1 Mechanic 2ã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P5B1_Mechanic2_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P5B1_Mechanic2_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32788"],
        userControl: Debugging)]
    public void P5B1_Mechanic2_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        _parse = 5.2;
        
        _p5A.Reset(sa);
        _p5A.Dispose();
        _p5B.Register();
        
        ResetSupportUnitVisibility(sa);
        _pd.Init(sa, "P5 Mechanic 2");
        _pd.AddPriorities([0, 1, 2, 3, 4, 5, 6, 7]);    // Add priority based on role order
        sa.DebugMsg($"Current phase: {_parse}", Debugging);
    }
    
    [ScriptMethod(name: "P5B1_Mechanic2_GetMalePosition", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7747", "SourceDataId:15720"], userControl: Debugging)]
    public void P5B1_Mechanic2_GetMalePosition(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.2) return;
        var region = ev.SourcePosition.GetRadian(Center).RadianToRegion(16, isDiagDiv: true);
        _p5B.MaleDirection = region;
        sa.DebugMsg($"P5B1_Mechanic2_GetMalePosition: {region} / 16", Debugging);
    }
    
    [ScriptMethod(name: "P5B1_Mechanic2_GetFarNearTether", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(342[78])$"], userControl: Debugging, suppress: 1000)]
    public void P5B1_Mechanic2_GetFarNearTether(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.2) return;
        const uint MID_GLITCH = 3427, REMOTE_GLITCH = 3428;
        _p5B.SynergyProgramIsFarTether = ev.StatusId == REMOTE_GLITCH;
        sa.DebugMsg($"Recorded Synergy Program tether as {(_p5B.SynergyProgramIsFarTether ? "Far" : "Near")}", Debugging);
    }
    
    [ScriptMethod(name: "P5B1_Mechanic2_GetSpreadMarkers", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:regex:^(0[1234678]|11|12)$"],
        userControl: Debugging)]
    public void P5B1_Mechanic2_GetSpreadMarkers(Event ev, ScriptAccessory sa)
    {
        // Record Attack 1234, Bind 123, Square/Circle
        if (_parse != 5.2) return;
        
        lock (_pd)
        {
            var mark = ev.Id0();
            var tidx = sa.GetPlayerIdIndex((uint)ev.TargetId);
            var targetJob = sa.GetPlayerJobByIndex(tidx);
            sa.DebugMsg($"Detected {targetJob} marked with ev.Id {mark}", Debugging);
            _pd.AddActionCount();
            var pdVal = mark switch
            {
                0x01 => 10,    // Attack 1
                0x02 => 20,    // Attack 2
                0x03 => 30,    // Attack 3
                0x04 => 40,    // Attack 4
                0x06 => 80,   // Bind 1
                0x07 => 70,   // Bind 2
                0x08 => 60,   // Bind 3
                0x11 or 0x12 => 50,   // Square/Circle
                _ => 0
            };
            _pd.AddPriority(tidx, pdVal);
            if (_pd.ActionCount != 8) return;
            sa.DebugMsg($"P5B1_Mechanic2_GetSpreadMarkers: Markers recorded", Debugging);
            sa.DebugMsg($"{_pd.ShowPriorities()}", Debugging);
            _p5B.SpreadMarkersRecorded.Set();   // Marker record
        }
    }

    [ScriptMethod(name: "P5B1_Mechanic2_SpreadWaveCannonPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(31603)$"],
        userControl: true)]
    public void P5B1_Mechanic2_SpreadWaveCannonPosition(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.2) return;
        _p5B.SpreadMarkersRecorded.WaitOne();
        // With male direction as south, increasing counter-clockwise
        var myPriValRank = _pd.FindPriorityIndexOfKey(sa.GetMyIndex());
        var myRegion = (_p5B.MaleDirection + 1 + myPriValRank * 2) % 16;
        _p5B.PlayerSpreadDirection = myRegion;
        sa.DebugMsg($"P5B1_Mechanic2_SpreadWaveCannonPosition: Player spread direction is {myRegion} / 16", Debugging);
        
        var basePos = new Vector3(100f, 0f, _p5B.SynergyProgramIsFarTether ? 119.75f : 111.75f);
        var myExtend = myPriValRank switch
        {
            0 or 7 when !_p5B.SynergyProgramIsFarTether => 1,
            1 or 6 when _p5B.SynergyProgramIsFarTether => -1,
            _ => 0
        };
        var myPos = basePos.RotateAndExtend(Center, myRegion * 22.5f.DegToRad(), myExtend);
        sa.DrawGuidance(myPos, 0, 10000, $"P5B1_Mechanic2_SpreadWaveCannonPosition");
    }

    [ScriptMethod(name: "P5B1_Mechanic2_SpreadWaveCannonEnd", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(31603)$"],
        userControl: Debugging)]
    public void P5B1_Mechanic2_SpreadWaveCannonEnd(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.2) return;
        sa.Method.RemoveDraw("P5B1_Mechanic2_SpreadWaveCannonPosition");
        _p5B.SpreadMarkersRecorded.Reset();
        _parse = 5.21;
    }

    [ScriptMethod(name: "P5B1_Mechanic2_TowerCollection", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Add", "DataId:regex:^(201324[56])$"],
        userControl: Debugging)]
    public void P5B1_Mechanic2_TowerCollection(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.21) return;
        const uint DOUBLE_TOWER = 2013246;
        lock (_p5B.TowerDirectionTypeDict)
        {
            var isDoubleTower = ev.DataId() == DOUBLE_TOWER;
            var region = ev.SourcePosition.GetRadian(Center).RadianToRegion(16, isDiagDiv: true);
            _p5B.TowerDirectionTypeDict.TryAdd(region, isDoubleTower);
            sa.DebugMsg($"P5B1_Mechanic2_TowerCollection: {(isDoubleTower ? "Double" : "Single")} tower at direction {region}", Debugging);

            if ((_p5B.TowerDirectionTypeDict.Count == 5 && _p5B.SynergyProgramIsFarTether) || (_p5B.TowerDirectionTypeDict.Count == 6 && !_p5B.SynergyProgramIsFarTether))
                _p5B.TowerDirectionRecorded.Set();
        }
    }
    
    [ScriptMethod(name: "P5B1_Mechanic2_StandOnTowerKnockbackPoint", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:regex:^(201324[56])$"],
        userControl: true, suppress: 1000)]
    public void P5B1_Mechanic2_StandOnTowerKnockbackPoint(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.21) return;
        _p5B.TowerDirectionRecorded.WaitOne();
        _p5B.PlayerTowerToStand = _p5B.FindPlayerTower(sa);
        var basePos = new Vector3(100, 0, 102.5f);
        var rad = _p5B.PlayerTowerToStand * 22.5f.DegToRad();
        var myKnockBackPos = basePos.RotateAndExtend(Center, rad);
        sa.DrawGuidance(myKnockBackPos, 0, 10000, $"P5B1_Mechanic2_StandOnTowerKnockbackPoint");
        sa.DrawLine(Center, 0, 0, 10000, $"P5B1_Mechanic2_StandOnTowerKnockbackGuideLine", rad, 20f, 20f, isSafe: true);
    }
    
    [ScriptMethod(name: "P5B1_Mechanic2_TowerPositionHint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(31534)$"],
        userControl: true, suppress: 1000)]
    public void P5B1_Mechanic2_TowerPositionHint(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.21) return;
        var text = _p5B.SynergyProgramIsFarTether ? "Stand at the edge to lengthen the tether" : "Stand in the middle of the tower";
        sa.Method.TextInfo(text, 3000);
    }
    
    [ScriptMethod(name: "P5B1_Mechanic2_ClearDrawingAfterTowerKnockback", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(31534)$"],
        userControl: Debugging)]
    public void P5B1_Mechanic2_ClearDrawingAfterTowerKnockback(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.21) return;
        sa.Method.RemoveDraw("P5B1_Mechanic2_StandOnTower.*");
        _p5B.TowerDirectionRecorded.Reset();
    }
    
    [ScriptMethod(name: "P5B1_Mechanic2_TowersVanishPhaseTransition", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Remove", "DataId:regex:^(201324[56])$"],
        userControl: Debugging, suppress: 1000)]
    public void P5B1_Mechanic2_TowersVanishPhaseTransition(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.21) return;
        _parse = 5.25;
        _pd.Init(sa, "P5 Second Transfer");
        _pd.AddPriorities([0, 1, 2, 3, 4, 5, 6, 7]);    // Add priority based on role order
        sa.DebugMsg($"Current phase: {_parse}", Debugging);
    }
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP5B2 Second Transferã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P5B2_SecondTransfer_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P5B2_SecondTransfer_GetRotatingMarkers", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:regex:^(0[1234679]|10)$"],
        userControl: Debugging)]
    public void P5B2_SecondTransfer_GetRotatingMarkers(Event ev, ScriptAccessory sa)
    {
        // Record Attack 1234, Bind 12, Stop 12
        if (_parse != 5.25) return;
        
        lock (_pd)
        {
            var mark = ev.Id0();
            var tidx = sa.GetPlayerIdIndex((uint)ev.TargetId);
            var targetJob = sa.GetPlayerJobByIndex(tidx);
            sa.DebugMsg($"Detected {targetJob} marked with ev.Id {mark}", Debugging);
            _pd.AddActionCount();
            var pdVal = mark switch
            {
                0x01 => 10,    // Attack 1
                0x02 => 20,    // Attack 2
                0x03 => 30,    // Attack 3
                0x04 => 40,    // Attack 4
                0x06 => 100,   // Bind 1
                0x07 => 110,   // Bind 2
                0x09 => 200,   // Stop 1
                0x10 => 210,   // Stop 2
                _ => 0
            };
            _pd.AddPriority(tidx, pdVal);
            if (_pd.ActionCount != 8) return;
            sa.DebugMsg($"P5B2_SecondTransfer_GetRotatingMarkers: Markers recorded", Debugging);
            sa.DebugMsg($"{_pd.ShowPriorities()}", Debugging);
            _p5B.RotatingMarkersRecorded.Set();   // Marker record
        }
    }
    
    [ScriptMethod(name: "P5B2_SecondTransfer_GetFemalePositionAndSkill", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7747", "SourceDataId:15720"], userControl: Debugging)]
    public void P5B2_SecondTransfer_GetFemalePositionAndSkill(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.25) return;
        var region = ev.SourcePosition.GetRadian(Center).RadianToRegion(8, isDiagDiv: true);
        _p5B.FemaleDirection = region;
        _p5B.SecondTransferFemaleId = ev.SourceId;
        var obj = sa.GetById(ev.SourceId);
        if (obj == null) return;
        var transId = sa.GetTransformationId(obj);
        sa.DebugMsg($"Obtained Female TransformationId: {transId}", Debugging);
        if (transId == null) return;
        
        const byte WOMAN_CROSS = 0, WOMAN_HOTWING = 4;
        _p5B.FemaleIsCrossOuterSafe = transId != WOMAN_HOTWING;
        
        sa.DebugMsg($"P5B2_SecondTransfer_GetFemalePositionAndSkill: {region} / 8, {(_p5B.FemaleIsCrossOuterSafe ? "Cross outer safe" : "HotWing inner safe")}", Debugging);
        _p5B.FemaleSkillRecorded.Set();
    }
    
    [ScriptMethod(name: "P5B2_SecondTransfer_GetRingRotationDirection", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(009[CD])$"], userControl: Debugging)]
    public void P5B2_SecondTransfer_GetRingRotationDirection(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.25) return;
        const int ICON_ROTATE_CW  = 156;   // 009C
        _p5B.RingIsClockwise = ev.Id0() == ICON_ROTATE_CW;
        sa.DebugMsg($"P5B2_SecondTransfer_GetRingRotationDirection: {(_p5B.RingIsClockwise ? "Clockwise" : "Counter-clockwise")}", Debugging);
        _p5B.RingDirectionRecorded.Set();
    }
    
    [ScriptMethod(name: "P5B2_SecondTransfer_RingLineAndStartPosition", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(009[CD])$"], userControl: true)]
    public void P5B2_SecondTransfer_RingLineAndStartPosition(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.25) return;
        _p5B.RingDirectionRecorded.WaitOne();
        _p5B.FemaleSkillRecorded.WaitOne();
        _p5B.RotatingMarkersRecorded.WaitOne();
        
        // Female direction edge, counter-clockwise 22.5deg, start point for Stop1, Stop2, Attack1; mirror across center for others
        var startPos = new Vector3(100, 0, 119.5f).RotateAndExtend(Center,
            _p5B.FemaleDirection * 45f.DegToRad() + (_p5B.RingIsClockwise ? 22.5f : -22.5f).DegToRad());

        var myPriValRank = _pd.FindPriorityIndexOfKey(sa.GetMyIndex());
        if (myPriValRank is 1 or 2 or 3 or 4 or 5)
            startPos = startPos.PointCenterSymmetry(Center);
        sa.DrawGuidance(startPos, 0, 10000, $"P5B2_SecondTransfer_RingLineAndStartPosition_StartPos");
        
        var dp = sa.DrawRect(ev.SourceId, 0, 10000, $"P5B2_SecondTransfer_RingLineAndStartPosition_RingLine", 0, 12, 50);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "P5B2_SecondTransfer_GetRingId", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(31631)$"], userControl: Debugging)]
    public void P5B2_SecondTransfer_GetRingId(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.25) return;
        _p5B.RingId = ev.SourceId;
    }
    
    [ScriptMethod(name: "P5B2_SecondTransfer_RingFirstCast", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(31631)$", "TargetIndex:1"], userControl: Debugging)]
    public void P5B2_SecondTransfer_RingFirstCast(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.25) return;
        
        _p5B.RingDirectionRecorded.Reset();
        _p5B.FemaleSkillRecorded.Reset();
        _p5B.RotatingMarkersRecorded.Reset();
        
        sa.Method.RemoveDraw($"P5B2_SecondTransfer_RingLineAndStartPosition_RingLine");
        sa.Method.RemoveDraw($"P5B2_SecondTransfer_RingLineAndStartPosition_StartPos");
    }
    
    [ScriptMethod(name: "P5B2_SecondTransfer_RingAttackDrawing", eventType: EventTypeEnum.ActionEffect,
        eventCondition: ["ActionId:regex:^(31631)$", "TargetIndex:1"], userControl: true)]
    public void P5B2_SecondTransfer_RingAttackDrawing(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.25) return;
        
        for (int i = 0; i < 13; i++)
        {
            var rotation = _p5B.FemaleDirection * 45f.DegToRad() + (i + 1) * 9f.DegToRad() * (_p5B.RingIsClockwise ? -1 : 1);
            var dp = sa.DrawRect(Center, 0, 10000, $"P5B2_SecondTransfer_RingAttackDrawing{i}", rotation, 10, 60, draw: false);
            dp.Color = sa.Data.DefaultDangerColor.WithW(0.25f);
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    
        
    [ScriptMethod(name: "P5B2_SecondTransfer_RingAttackDrawingDelete", eventType: EventTypeEnum.ActionEffect,
        eventCondition: ["ActionId:regex:^(31632)$", "TargetIndex:1"], userControl: Debugging)]
    public void P5B2_SecondTransfer_RingAttackDrawingDelete(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.25) return;
        sa.Method.RemoveDraw($"^(P5B2_SecondTransfer_RingAttackDrawing{_p5B.RingAttackCount})$");
        _p5B.RingAttackCount++;
    }
    
    [ScriptMethod(name: "P5B2_SecondTransfer_FemaleAttackRangeDrawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(31631)$", "TargetIndex:1"], userControl: true)]
    public void P5B2_SecondTransfer_FemaleAttackRangeDrawing(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.25) return;
        if (_p5B.FemaleIsCrossOuterSafe)
        {
            var dp1 = sa.DrawRect(_p5B.SecondTransferFemaleId, 0, 5500, $"P5B2_SecondTransfer_FemaleAttackRange_FemaleCross1", 0, 10, 60, draw: false);
            var dp2 = sa.DrawRect(_p5B.SecondTransferFemaleId, 0, 5500, $"P5B2_SecondTransfer_FemaleAttackRange_FemaleCross2", float.Pi / 2, 10, 60, draw: false);
            dp1.Color = sa.Data.DefaultDangerColor.WithW(2.5f);
            dp2.Color = sa.Data.DefaultDangerColor.WithW(2.5f);
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1);
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp2);
        }
        else
        {
            var dp3 = sa.DrawDonut(_p5B.SecondTransferFemaleId, 0, 5500, $"P5B2_SecondTransfer_FemaleAttackRange_FemaleHotWing", 60, 8, draw: false);
            dp3.Color = sa.Data.DefaultDangerColor.WithW(2.5f);
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.HotWing, dp3);
        }
    }
    
    [ScriptMethod(name: "P5B2_SecondTransfer_EnterOrStayHint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(31631)$", "TargetIndex:1"], userControl: true)]
    public void P5B2_SecondTransfer_EnterOrStayHint(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.25) return;
        var text = _p5B.FemaleIsCrossOuterSafe ? "Stay, stay, stay" : "Go, go, go";
        sa.Method.TextInfo(text, 1500, true);
    }
    
    [ScriptMethod(name: "*P5B2_SecondTransfer_TemporarilyRemoveBigRingForFemaleCross", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(31631)$", "TargetIndex:1"], userControl: true)]
    public void P5B2_SecondTransfer_TemporarilyRemoveBigRingForFemaleCross(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.25) return;
        if (!SpecialMode) return;
        if (!_p5B.FemaleIsCrossOuterSafe) return;
        sa.DebugMsg($"Female is cross, temporarily removing big ring", Debugging);
        sa.WriteVisible(sa.GetById(_p5B.RingId), false);
    }
    
    [ScriptMethod(name: "P5B2_SecondTransfer_FemaleSkillCasts", eventType: EventTypeEnum.ActionEffect,
        eventCondition: ["ActionId:regex:^(3153[123])$", "TargetIndex:1"], userControl: Debugging, suppress: 1000)]
    public void P5B2_SecondTransfer_FemaleSkillCasts(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.25) return;
        _p5B.FemaleCrossHotWingCasted.Set();
        sa.Method.RemoveDraw($"P5B2_SecondTransfer_FemaleAttackRange.*");
        if (!_p5B.FemaleIsCrossOuterSafe) return;
        sa.Method.TextInfo("Go, go, go", 1500, true);
        if (!SpecialMode) return;
        sa.WriteVisible(sa.GetById(_p5B.RingId), true);
    }
    
    [ScriptMethod(name: "P5B2_SecondTransfer_Guidance", eventType: EventTypeEnum.ActionEffect,
        eventCondition: ["ActionId:regex:^(3153[123])$", "TargetIndex:1"], userControl: true, suppress: 1000)]
    public void P5B2_SecondTransfer_Guidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.25) return;
        _p5B.FemaleCrossHotWingCasted.WaitOne();

        var myPriValRank = _pd.FindPriorityIndexOfKey(sa.GetMyIndex());
        const int ATK1 = 0, ATK2 = 1, ATK3 = 2, ATK4 = 3;
        const int BIND1 = 4, BIND2 = 5, STOP1 = 6, STOP2 = 7;

        var myBasePos = myPriValRank switch
        {
            ATK1 => new Vector3(80.5f, 0, 100f),
            ATK2 => new Vector3(119.5f, 0, 100f),
            ATK3 => new Vector3(98f, 0, 89f),
            ATK4 => new Vector3(94.74f, 0, 81.74f),
            BIND1 => new Vector3(110f, 0, 100f),
            BIND2 => new Vector3(105.26f, 0, 81.74f),
            STOP1 => new Vector3(111.34f, 0, 116.22f),
            STOP2 => new Vector3(88.66f, 0, 116.22f),
        };

        if (!_p5B.RingIsClockwise && myPriValRank is ATK1 or ATK2 or BIND1)
            myBasePos = myBasePos.FoldPointHorizon(Center.X);

        var rotation = _p5B.FemaleDirection * 45f.DegToRad();
        var safePos = myBasePos.RotateAndExtend(Center, rotation);
        sa.DrawGuidance(safePos, 0, 10000, $"P5B2_SecondTransfer_Guidance");
    }
    
    [ScriptMethod(name: "P5A2_SecondTransfer_End", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(31625|33040)$"],
        userControl: Debugging)]
    public void P5A2_SecondTransfer_End(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.25) return;
        sa.Method.RemoveDraw("P5B2_SecondTransfer.*");
        _p5B.FemaleCrossHotWingCasted.Reset();
    }
    
    #endregion P5B Mechanic 2 / Second Transfer
    
    #region P5C Mechanic 3 / Third Transfer

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP5C1 Mechanic 3ã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P5C1_Mechanic3_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P5C1_Mechanic3_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32789"],
        userControl: Debugging)]
    public void P5C1_Mechanic3_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        _parse = 5.3;
        
        _p5B.Reset(sa);
        _p5B.Dispose();
        _p5C.Register();
        
        ResetSupportUnitVisibility(sa);
        _pd.Init(sa, "P5 Mechanic 3");
        _pd.AddPriorities([0, 1, 2, 3, 4, 5, 6, 7]);    // Add priority based on role order
        sa.DebugMsg($"Current phase: {_parse}", Debugging);
    }

    [ScriptMethod(name: "P5C1_Mechanic3_GetMaleFemaleComboSkills", eventType: EventTypeEnum.PlayActionTimeline,
        eventCondition: ["Id:7747", "SourceDataId:regex:^(15721|15722)$"], userControl: Debugging)]
    public void P5C1_Mechanic3_GetMaleFemaleComboSkills(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.3) return;
        const uint OMEGA_MALE = 15721, OMEGA_FEMALE = 15722, OMEGA_BALD = 14669;
        const byte MAN_CHARIOT = 0, MAN_DONUT = 4, WOMAN_CROSS = 0, WOMAN_HOTWING = 4;
        
        lock (_p5C.ComboSkillRecord)
        {
            if (_p5C.ComboSkillRecord.Count >= 6) return;
            var region = ev.SourcePosition.GetRadian(Center).RadianToRegion(4);
            IGameObject? obj = sa.GetById(ev.SourceId);
            if (obj == null) return;
            var transId = sa.GetTransformationId(obj);
            var dataId = obj.DataId;
            var isFirstRound = _p5C.ComboSkillRecord.Count < 2;
            
            var skillId = (dataId == OMEGA_FEMALE ? 8 : 0) + (transId == 4 ? 4 : 0) + region;
            _p5C.ComboSkillRecord.TryAdd(region, (skillId, isFirstRound));
            sa.DebugMsg($"At direction {region} there is {(isFirstRound ? "first" : "second")} round skill, Skill ID {skillId}", Debugging);
            if (_p5C.ComboSkillRecord.Count != 4) return;
            _p5C.MaleFemaleComboSkillsRecorded.Set();
        }
    }
    
    [ScriptMethod(name: "P5C1_Mechanic3_GetBaldComboSkillsAndSafePoint", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:regex:^(3164[34])$"], userControl: Debugging)]
    public void P5C1_Mechanic3_GetBaldComboSkillsAndSafePoint(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.3) return;
        const uint FRONT_FIRST = 31643, SIDE_FIRST = 31644;
        _p5C.MaleFemaleComboSkillsRecorded.WaitOne();
        lock (_p5C.ComboSkillRecord)
        {
            if (_p5C.ComboSkillRecord.Count >= 6) return;
            // region is arbitrary, pick two values outside 0~3
            var firstRoundSkillId = ev.ActionId == FRONT_FIRST ? 17 : 16;
            var secondRoundSkillId = ev.ActionId == FRONT_FIRST ? 16 : 17;
            _p5C.ComboSkillRecord.TryAdd(10, (firstRoundSkillId, true));
            _p5C.ComboSkillRecord.TryAdd(11, (secondRoundSkillId, false));
            sa.DebugMsg($"Bald's first round skill Id: {firstRoundSkillId}", Debugging);
            
            if (_p5C.ComboSkillRecord.Count != 6) return;
            _p5C.FindComboAttackSafePoint(sa);
            sa.DebugMsg($"Combo skill safe spots: {_p5C.ComboSkillSafeSpot[0]}, {_p5C.ComboSkillSafeSpot[1]}", Debugging);
            
            _p5C.MaleFemaleComboSkillsRecorded.Reset();
            _p5C.ComboSkillSafeSpotRecorded.Set();
        }
    }

    [ScriptMethod(name: "*P5C1_Mechanic3_ShrinkSpawnedMaleFemaleAndBald", eventType: EventTypeEnum.PlayActionTimeline,
        eventCondition: ["Id:7747", "SourceDataId:regex:^(15721|15722|14669)$"], userControl: true)]
    public void P5C1_Mechanic3_ShrinkSpawnedMaleFemaleAndBald(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.3) return;
        if (!SpecialMode) return;
        var obj = sa.GetById(ev.SourceId);
        sa.ScaleModify(obj, 0.4f);
    }

    [ScriptMethod(name: "P5C1_Mechanic3_ComboSkillAttackRangeDrawing", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:regex:^(3164[34])$"], userControl: true)]
    public void P5C1_Mechanic3_ComboSkillAttackRangeDrawing(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.3) return;
        _p5C.ComboSkillSafeSpotRecorded.WaitOne();

        foreach (var (region, (skillId, isFirstRound)) in _p5C.ComboSkillRecord)
            DrawComboSkillAttackRange(sa, region, skillId, isFirstRound, "FirstRound");

        _p5C.FirstRoundComboEnd.WaitOne();
        sa.Method.RemoveDraw($"P5C1_Mechanic3_ComboSkillAttackRangeDrawing_FirstRound.*");
        
        foreach (var (region, (skillId, isFirstRound)) in _p5C.ComboSkillRecord)
            DrawComboSkillAttackRange(sa, region, skillId, !isFirstRound, "SecondRound");
        
        return;
        
        void DrawComboSkillAttackRange(ScriptAccessory sa, int region, int skillId, bool draw, string prefix)
        {
            if (!draw) return;
            var skillType = skillId / 4;
            
            Vector3 ownerPos;
            float rotation;
            if (region < 4)
            {
                ownerPos = new Vector3(100, 0, 110).RotateAndExtend(Center, (45f + 90f * region).DegToRad());
                rotation = (225f + 90f * region).DegToRad();
            }
            else
            {
                ownerPos = new Vector3(100, 0, 100);
                rotation = skillId == 16 ? 90f.DegToRad() : 0f;
            }
            
            // skillType
            const int CHARIOT = 0, DONUT = 1, CROSS = 2, HOTWING = 3, FAN = 4;
            switch (skillType)
            {
                case DONUT:
                    sa.DrawDonut(ownerPos, 0, 12000, $"P5C1_Mechanic3_ComboSkillAttackRangeDrawing_{prefix}_MaleDonut", 40, 10);
                    break;
                case CHARIOT:
                    sa.DrawCircle(ownerPos, 0, 12000, $"P5C1_Mechanic3_ComboSkillAttackRangeDrawing_{prefix}_MaleChariot", 10);
                    break;
                case CROSS:
                    var dp1 = sa.DrawRect(ownerPos, 0, 12000, $"P5C1_Mechanic3_ComboSkillAttackRangeDrawing_{prefix}_FemaleCross1", rotation, 10, 60, draw: false);
                    var dp2 = sa.DrawRect(ownerPos, 0, 12000, $"P5C1_Mechanic3_ComboSkillAttackRangeDrawing_{prefix}_FemaleCross2", rotation + float.Pi / 2, 10, 60, draw: false);
                    sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1);
                    sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp2);
                    break;
                case HOTWING:
                    var dp3 = sa.DrawDonut(ownerPos, 0, 12000, $"P5C1_Mechanic3_ComboSkillAttackRangeDrawing_{prefix}_FemaleHotWing", 60, 8, draw: false);
                    dp3.Rotation = rotation;
                    sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.HotWing, dp3);
                    break;
                case FAN:
                    sa.DrawFan(ownerPos, 0, 12000, $"P5C1_Mechanic3_ComboSkillAttackRangeDrawing_{prefix}_BaldSword1", 120f.DegToRad(), rotation, 20f, 0f);
                    sa.DrawFan(ownerPos, 0, 12000, $"P5C1_Mechanic3_ComboSkillAttackRangeDrawing_{prefix}_BaldSword2", 120f.DegToRad(), rotation + 180f.DegToRad(), 20f, 0f);
                    break;
            }
        }
    }

    [ScriptMethod(name: "P5C1_Mechanic3_SafeSpotGuidance", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:regex:^(3164[34])$"], userControl: true)]
    public void P5C1_Mechanic3_SafeSpotGuidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.3) return;
        _p5C.ComboSkillSafeSpotRecorded.WaitOne();
        var (safePos1, hintString1) = GetSafePosOfComboSkill(_p5C.ComboSkillSafeSpot[0]);
        var (safePos2, hintString2) = GetSafePosOfComboSkill(_p5C.ComboSkillSafeSpot[1]);
        sa.Method.TextInfo($"{hintString1} -> {hintString2}", 4500, true);
        
        sa.DrawGuidance(safePos1, 0, 12000, $"P5C1_Mechanic3_SafeSpotGuidance1");
        sa.DrawGuidance(safePos1, safePos2, 0, 12000, $"P5C1_Mechanic3_SafeSpotGuidance1Prepare", isSafe: false);

        _p5C.FirstRoundComboEnd.WaitOne();
        sa.Method.RemoveDraw($"P5C1_Mechanic3_SafeSpotGuidance1.*");
        sa.DrawGuidance(safePos2, 0, 12000, $"P5C1_Mechanic3_SafeSpotGuidance2");

        (Vector3, string) GetSafePosOfComboSkill(int safePosIdx)
        {
            var rotation = safePosIdx % 4 * 90f.DegToRad();
            var distance = (safePosIdx / 4) switch
            {
                0 => 4.5f,
                1 => 12f,
                2 => 19f,
                _ => 0,
            };

            var markHint = (safePosIdx % 4) switch
            {
                0 => "C",
                1 => "B",
                2 => "A",
                _ => "D",
            };
            
            var distanceHint = (safePosIdx / 4) switch
            {
                0 => "Near",
                1 => "Middle",
                _ => "Far",
            };
            
            var hintString = $"{markHint}{distanceHint}";
            return (new Vector3(100, 0, 100 + distance).RotateAndExtend(Center, rotation), hintString);
        }
    }

    [ScriptMethod(name: "P5C1_Mechanic3_FirstRoundComboEnd", eventType: EventTypeEnum.ActionEffect,
        eventCondition: ["ActionId:regex:^(3152[56])$"], userControl: Debugging)]
    public void P5C1_Mechanic3_FirstRoundComboEnd(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.3) return;
        _p5C.FirstRoundComboEnd.Set();
    }
    
    [ScriptMethod(name: "P5C1_Mechanic3_FirstHalfComboEnd", eventType: EventTypeEnum.ActionEffect,
        eventCondition: ["ActionId:regex:^(3160[78])$"], userControl: Debugging)]
    public void P5C1_Mechanic3_FirstHalfComboEnd(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.3) return;
        sa.Method.RemoveDraw($"P5C1_Mechanic3.*");
        _p5C.FirstRoundComboEnd.Reset();
        _p5C.ComboSkillSafeSpotRecorded.Reset();
        _parse = 5.35;
    }

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP5C2 Third Transferã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P5C2_ThirdTransfer_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P5C2_ThirdTransfer_PhaseSplit", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void P5B2_ThirdTransfer_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        _parse = 5.35;
        sa.DebugMsg($"Current phase: {_parse}", Debugging);
    }
    
    [ScriptMethod(name: "P5C2_ThirdTransfer_GetThirdTransferMarkers", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:regex:^(0[1234679]|10)$"],
        userControl: Debugging)]
    public void P5C2_ThirdTransfer_GetThirdTransferMarkers(Event ev, ScriptAccessory sa)
    {
        // Record Attack 1234, Bind 12, Stop 12
        if (_parse != 5.35) return;
        
        lock (_pd)
        {
            var mark = ev.Id0();
            var tidx = sa.GetPlayerIdIndex((uint)ev.TargetId);
            var targetJob = sa.GetPlayerJobByIndex(tidx);
            sa.DebugMsg($"Detected {targetJob} marked with ev.Id {mark}", Debugging);
            _pd.AddActionCount();
            var pdVal = mark switch
            {
                0x01 => 10,    // Attack 1
                0x02 => 20,    // Attack 2
                0x03 => 30,    // Attack 3
                0x04 => 40,    // Attack 4
                0x06 => 100,   // Bind 1
                0x07 => 110,   // Bind 2
                0x09 => 200,   // Stop 1
                0x10 => 210,   // Stop 2
                _ => 0
            };
            _pd.AddPriority(tidx, pdVal);
            if (_pd.ActionCount != 8) return;
            sa.DebugMsg($"P5C2_ThirdTransfer_GetThirdTransferMarkers: Markers recorded", Debugging);
            sa.DebugMsg($"{_pd.ShowPriorities()}", Debugging);
            _p5C.ThirdTransferMarkersRecorded.Set();   // Marker record
        }
    }

    [ScriptMethod(name: "P5C2_ThirdTransfer_BaldScanRange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3163[89])$"],
        userControl: true)]
    public void P5C2_ThirdTransfer_BaldScanRange(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.35) return;
        _p5C.ThirdTransferMarkersRecorded.WaitOne();
        var myPriValRank = _pd.FindPriorityIndexOfKey(sa.GetMyIndex());
        var isSafe = myPriValRank >= 6;
        const uint CANNON_LEFT = 31639, CANNON_RIGHT = 31638;
        var rotation = (ev.ActionId == CANNON_RIGHT ? 90f : -90f).DegToRad();
        sa.DrawFan(Center, 0, 10000, $"P5C2_ThirdTransfer_BaldScanRange", float.Pi, rotation, 40, 0, isSafe);
    }

    [ScriptMethod(name: "P5C2_ThirdTransfer_Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3163[89])$"],
        userControl: true)]
    public void P5C2_ThirdTransfer_Guidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.35) return;
        _p5C.ThirdTransferMarkersRecorded.WaitOne();

        var myPriValRank = _pd.FindPriorityIndexOfKey(sa.GetMyIndex());
        const int ATK1 = 0, ATK2 = 1, ATK3 = 2, ATK4 = 3;
        const int BIND1 = 4, BIND2 = 5, STOP1 = 6, STOP2 = 7;

        // Based on Omega attacking top side
        var safePos = myPriValRank switch
        {
            ATK1 => new Vector3(80.5f, 0, 100.5f),
            ATK2 => new Vector3(119.5f, 0, 100.5f),
            ATK3 => new Vector3(102f, 0, 111f),
            ATK4 => new Vector3(105.26f, 0f, 118.26f),
            BIND1 => new Vector3(90f, 0f, 100.5f),
            BIND2 => new Vector3(94.74f, 0f, 118.26f),
            STOP1 => new Vector3(90.8f, 0f, 90.8f),
            STOP2 => new Vector3(109.2f, 0f, 90.8f),
        };

        const uint CANNON_LEFT = 31639, CANNON_RIGHT = 31638;
        var rotation = (ev.ActionId == CANNON_LEFT ? 90f : -90f).DegToRad();
        safePos = safePos.RotateAndExtend(Center, rotation);
        sa.DrawGuidance(safePos, 0, 10000, $"P5C2_ThirdTransfer_Guidance");
    }
    
    [ScriptMethod(name: "P5C2_ThirdTransfer_OmegaScanEndThirdTransferEnd", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3163[89])$"],
        userControl: Debugging)]
    public void P5C3_ThirdTransfer_OmegaScanEndThirdTransferEnd(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.35) return;
        _p5C.ThirdTransferMarkersRecorded.Reset();
        sa.Method.RemoveDraw($"P5C2_ThirdTransfer.*");
        _pd.Init(sa, "P5 Fourth Transfer");
        _parse = 5.38;
        sa.DebugMsg($"Current phase: {_parse}", Debugging);
    }
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP5C3 Fourth Transferã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P5C3_FourthTransfer_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P5C3_FourthTransfer_GetFourthTransferMarkers", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:regex:^(0[1234679]|10)$"],
        userControl: Debugging)]
    public void P5C3_FourthTransfer_GetFourthTransferMarkers(Event ev, ScriptAccessory sa)
    {
        // Record Attack 1234, Bind 12, Stop 12
        if (_parse != 5.38) return;
        
        lock (_pd)
        {
            var mark = ev.Id0();
            var tidx = sa.GetPlayerIdIndex((uint)ev.TargetId);
            var targetJob = sa.GetPlayerJobByIndex(tidx);
            sa.DebugMsg($"Detected {targetJob} marked with ev.Id {mark}", Debugging);
            _pd.AddActionCount();
            var pdVal = mark switch
            {
                0x01 => 10,    // Attack 1
                0x02 => 20,    // Attack 2
                0x03 => 30,    // Attack 3
                0x04 => 40,    // Attack 4
                0x06 => 100,   // Bind 1
                0x07 => 110,   // Bind 2
                0x09 => 200,   // Stop 1
                0x10 => 210,   // Stop 2
                _ => 0
            };
            _pd.AddPriority(tidx, pdVal);
            if (_pd.ActionCount != 8) return;
            sa.DebugMsg($"P5C3_FourthTransfer_GetFourthTransferMarkers: Markers recorded", Debugging);
            sa.DebugMsg($"{_pd.ShowPriorities()}", Debugging);
            _p5C.FourthTransferMarkersRecorded.Set();   // Marker record
        }
    }
    
    [ScriptMethod(name: "P5C3_FourthTransfer_BeetleDirectionRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32374"], userControl: Debugging)]
    public void P5C3_FourthTransfer_BeetleDirectionRecord(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.38) return;

        _p5C.BeetleId = ev.SourceId;
        _p5C.BeetleDirection = ev.SourcePosition.GetRadian(Center).RadianToRegion(4, isDiagDiv: true);
        sa.DebugMsg($"P5C3_FourthTransfer_BeetleDirectionRecord: {_p5C.BeetleDirection} / 4", Debugging);
        _p5C.BeetleDirectionRecorded.Set();
    }

    [ScriptMethod(name: "P5C3_FourthTransfer_Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32374"], userControl: true)]
    public void P5C3_FourthTransfer_Guidance(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.38) return;
        
        _p5C.BeetleDirectionRecorded.WaitOne();
        _p5C.FourthTransferMarkersRecorded.WaitOne();
        
        var myPriValRank = _pd.FindPriorityIndexOfKey(sa.GetMyIndex());
        const int ATK1 = 0, ATK2 = 1, ATK3 = 2, ATK4 = 3;
        const int BIND1 = 4, BIND2 = 5, STOP1 = 6, STOP2 = 7;

        // Based on Beetle direction South
        var myBasePos = myPriValRank switch
        {
            ATK1 => new Vector3(119.5f, 0, 100f),
            ATK2 => new Vector3(80.5f, 0, 100f),
            ATK3 => new Vector3(98f, 0, 89f),
            ATK4 => new Vector3(94.74f, 0f, 81.74f),
            BIND1 => new Vector3(110f, 0f, 100f),
            BIND2 => new Vector3(105.26f, 0f, 81.74f),
            STOP1 => new Vector3(110.3f, 0f, 116.5f),
            STOP2 => new Vector3(89.7f, 0f, 116.5f),
        };

        var safePos = myBasePos.RotateAndExtend(Center, _p5C.BeetleDirection * 90f.DegToRad());
        sa.DrawGuidance(safePos, 0, 10000, $"P5C3_FourthTransfer_Guidance");
    }

    [ScriptMethod(name: "P5C3_FourthTransfer_End", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:32374"],
        userControl: Debugging)]
    public void P5C3_FourthTransfer_End(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.38) return;
        _parse = 5.39;
        _p5C.BeetleDirectionRecorded.Reset();
        _p5C.FourthTransferMarkersRecorded.Reset();
        _p5C.Reset(sa);
        _p5C.Dispose();
        sa.Method.RemoveDraw($"P5.*");
    }

    [ScriptMethod(name: "P5C3_FourthTransfer_TetherRange", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0059"],
        userControl: true)]
    public void P5C3_FourthTransfer_TetherRange(Event ev, ScriptAccessory sa)
    {
        if (_parse != 5.38) return;

        _p5C.BeetleDirectionRecorded.WaitOne();
        _p5C.FourthTransferMarkersRecorded.WaitOne();
        
        if (_p5C.TetherScanActive) return;
        _p5C.TetherScanActive = true;
        
        // Draw large circle only when the correct player in the correct round tethers
        _p5C.TetherScanFramework = sa.Method.RegistFrameworkUpdateAction(Action);
        
        const uint TETHER_ID = 0x59;
        return;
        
        void Action()
        {
            var bossObj = sa.GetById(_p5C.BeetleId);
            if (bossObj is null) return;
            
            foreach (var member in sa.Data.PartyList)
            {
                // Find party member
                IGameObject? memberObj = sa.GetById(member);
                if (memberObj is null) continue;
                int memberIdx = sa.GetPlayerIdIndex(member);
                
                void CleanUp()
                {
                    _p5C.TetherDrawingDictionary.Remove(memberIdx, out _);
                    sa.Method.RemoveDraw($"P5C_FourthTransfer_TetherRange_TetherSource{memberIdx}");
                }

                // Get priority, Stop markers have priority >=200
                if (!_pd.Priorities.TryGetValue(memberIdx, out int memberPrival) || memberPrival < 200) { CleanUp(); continue; }
                
                // Tether source check
                var tetherSource = sa.GetTetherSource((IBattleChara?)memberObj, TETHER_ID);
                bool isCorrectTether = tetherSource.Count == 1 && tetherSource[0] == _p5C.BeetleId;
                if (!isCorrectTether) { CleanUp(); continue; }

                // Avoid redrawing
                if (_p5C.TetherDrawingDictionary.TryAdd(memberIdx, true))
                    sa.DrawCircle(member, 0, Int32.MaxValue, $"P5C3_FourthTransfer_TetherRange_TetherSource{memberIdx}", 15);
            }
        }
    }

    #endregion P5C Mechanic 3 / Third Transfer

    #region P6 Cosmic Memory
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP6 Cosmic Memoryã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P6_AutoAttack_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P6_CosmicMemory_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31649"],
        userControl: Debugging)]
    public void P6_CosmicMemory_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        _parse = 6;
        _p6.Reset(sa);
        _p6.Register();
        _p6.BossId = ev.SourceId;
        ResetSupportUnitVisibility(sa);
        sa.DebugMsg($"Current phase: {_parse}", Debugging);
    }
    
    [ScriptMethod(name: "P6_AutoAttackDrawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31649", "TargetIndex:1"],
        userControl: true)]
    public void P6_AutoAttackDrawing(Event ev, ScriptAccessory sa)
    {
        if (_parse != 6) return;
        _p6.EnableAutoAttackDrawing = true;
        _p6.AutoAttackDrawingFramework = sa.Method.RegistFrameworkUpdateAction(Action);
        return;
        
        void Action()
        {
            if (!_p6.EnableAutoAttackDrawing)
                return;
            
            var myIndex = sa.GetMyIndex();
            UpdateFarthestPlayer();
            UpdateMainTank();
            return;
            
            void UpdateFarthestPlayer()
            {
                int farthestIdx = FindFarthestPlayerIndex();
                if (farthestIdx == _p6.FarthestPlayerIdx) return;
                
                const string hint = "Farthest";
                sa.Method.RemoveDraw($"P6_AutoAttackDrawing_{hint}_{_p6.FarthestPlayerIdx}");
                _p6.FarthestPlayerIdx = farthestIdx;
                
                DrawPlayerCircle(farthestIdx, hint, farthestIdx == 1 && myIndex == 1);
                
                if (farthestIdx != 1 && myIndex == 1 && !_p6.HasDrawnAwayIndicatorLine)
                {
                    _p6.HasDrawnAwayIndicatorLine = true;
                    var dp = sa.DrawGuidance(Center, 0, Int32.MaxValue, $"P6_AutoAttackDrawing_AwayIndicatorLine", 
                        180f.DegToRad(), draw: false);
                    sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);
                    sa.Method.TextInfo("Move to the farthest!", 1500, true);
                }
                else if (farthestIdx == 1 && myIndex == 1 && _p6.HasDrawnAwayIndicatorLine)
                {
                    _p6.HasDrawnAwayIndicatorLine = false;
                    sa.Method.RemoveDraw($"P6_AutoAttackDrawing_AwayIndicatorLine");
                }
            }
            
            int FindFarthestPlayerIndex()
            {
                float maxDistance = 0f;
                int farthestIdx = 0;

                foreach (uint memberId in sa.Data.PartyList)
                {
                    var memberObj = sa.GetById(memberId);
                    if (memberObj is null) continue;

                    float distance = Vector3.Distance(memberObj.Position, Center);
                    if (distance < maxDistance) continue;
                    maxDistance = distance;
                    farthestIdx = sa.GetPlayerIdIndex(memberId);
                }
                return farthestIdx;
            }
            
            void UpdateMainTank()
            {
                int mainTankIdx = sa.GetPlayerIdIndex((uint)sa.Data.EnmityList[_p6.BossId][0]);
                if (mainTankIdx == _p6.MainTankPlayerIdx) return;
                const string hint = "MainTank";
                sa.Method.RemoveDraw($"P6_AutoAttackDrawing_{hint}_{_p6.MainTankPlayerIdx}");
                _p6.MainTankPlayerIdx = mainTankIdx;

                DrawPlayerCircle(mainTankIdx, hint, mainTankIdx == 0 && myIndex == 0);

                if (mainTankIdx != 0 && myIndex == 0 && !_p6.HasGivenEstablishAggroHint)
                {
                    _p6.HasGivenEstablishAggroHint = true;
                    sa.Method.TextInfo("Establish aggro!", 1500, true);
                }
                else if (mainTankIdx == 0 && myIndex == 0 && _p6.HasGivenEstablishAggroHint)
                {
                    _p6.HasGivenEstablishAggroHint = false;
                }
            }
            
            void DrawPlayerCircle(int playerIdx, string hint, bool isCorrectChara)
            {
                uint playerId = sa.Data.PartyList[playerIdx];
            
                var dp = sa.DrawCircle(playerId, 0, Int32.MaxValue, $"P6_AutoAttackDrawing_{hint}_{playerIdx}", 
                    5f, isCorrectChara, draw: false);
            
                dp.Color = dp.Color.WithW(2f);
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }
    }
    
    [ScriptMethod(name: "P6_CosmicFlare_Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31654"],
        userControl: true)]
    public void P6_CosmicFlare_Drawing(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        _p6.CosmicFlareDrawingStart.WaitOne();
        
        var sid = ev.SourceId;
        var myIndex = sa.GetMyIndex();
        var isTank = myIndex <= 1;
        var dp1 = sa.DrawDonut(sid, 0, 10000, $"P6_CosmicFlare_Drawing1", 8f, 7.7f, isTank, draw: false);
        dp1.Color = isTank ? sa.Data.DefaultSafeColor.WithW(5f) : sa.Data.DefaultDangerColor.WithW(5f);
        dp1.SetOwnersDistanceOrder(true, 1);
        var dp2 = sa.DrawDonut(sid, 0, 10000, $"P6_CosmicFlare_Drawing2", 8f, 7.7f, isTank, draw: false);
        dp2.Color = isTank ? sa.Data.DefaultSafeColor.WithW(5f) : sa.Data.DefaultDangerColor.WithW(5f);
        dp2.SetOwnersDistanceOrder(true, 2);
        var dp3 = sa.DrawDonut(sid, 0, 10000, $"P6_CosmicFlare_Drawing3", 6f, 5.7f, !isTank, draw: false);
        dp3.Color = !isTank ? sa.Data.DefaultSafeColor.WithW(5f) : sa.Data.DefaultDangerColor.WithW(5f);
        dp3.SetOwnersDistanceOrder(true, 3);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp3);
    }
    
    [ScriptMethod(name: "P6_CosmicFlare_DrawingDelete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31655"],
        userControl: Debugging, suppress: 500)]
    public void P6A_CosmicFlare_DrawingDelete(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        sa.Method.RemoveDraw($"P6.*");
        
        switch (_parse)
        {
            case 6.1:
                _p6.ResetCosmoArrow(sa);
                break;
            case 6.2:
                _p6.ResetUnlimitedWaveCannon();
                _p6.ResetSpreadWaveCannon();
                break;
        }

        _p6.EnableAutoAttackDrawing = true;
        _p6.CosmicFlareDrawingStart.Reset();
    }

    [ScriptMethod(name: "P6_WaveCannon_StackPositionDrawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31657"],
        userControl: true)]
    public void P6_WaveCannon_StackPositionDrawing(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        _p6.WaveCannonStackDrawingStart.WaitOne();
        var isTank = sa.GetMyIndex() <= 1;
        var stackPos = new Vector3(100, 0, isTank ? 103 : 109);
        sa.DrawGuidance(stackPos, 0, 10000, $"P6_WaveCannon_StackPositionDrawing");
    }
    
    [ScriptMethod(name: "P6_WaveCannon_SpreadAttackCount", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31659"],
        userControl: Debugging, suppress: 500)]
    public void P6_WaveCannon_SpreadAttackCount(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        _p6.WaveCannonJudgeCount++;
        if (_p6.WaveCannonJudgeCount < 2) return;
        sa.Method.RemoveDraw($"P6_WaveCannon_SpreadGuidance.*");
        sa.Method.RemoveDraw($"P6_WaveCannon_GuideLine.*");
        _p6.WaveCannonStackDrawingStart.Set();
    }
    
    [ScriptMethod(name: "P6_WaveCannon_StackDrawingDelete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31658"],
        userControl: Debugging, suppress: 500)]
    public void P6_WaveCannon_StackDrawingDelete(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        sa.Method.RemoveDraw($".*");
        switch (_parse)
        {
            case 6.1:
                _p6.ResetUnlimitedWaveCannon();
                _p6.ResetSpreadWaveCannon();
                break;
            case 6.2:
                _p6.ResetCosmoArrow(sa);
                break;
        }
        _p6.EnableAutoAttackDrawing = true;
        _p6.WaveCannonSpreadDrawingStart.Reset();
        _p6.WaveCannonStackDrawingStart.Reset();
    }

    #endregion P6 Cosmic Memory
    
    #region P6A Cosmo Arrow

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP6A Cosmo Arrowã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P6A_CosmoArrow_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P6A_CosmoArrow_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31650"],
        userControl: Debugging)]
    public void P6A_CosmoArrow_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        _parse = _parse == 6.1 ? 6.2 : 6.1;
        sa.DebugMsg($"Current phase: {_parse} Cosmo Arrow", Debugging);
        _p6.ResetAutoAttack(sa, false);
        _p6.CosmoArrowCastStart.Set();
    }
    
    [ScriptMethod(name: "P6A_CosmoArrow_TypeDetermination", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31651"],
        userControl: Debugging, suppress: 1000)]
    public void P6A_CosmoArrow_TypeDetermination(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        
        _p6.CosmoArrowCastStart.WaitOne();
        if (_p6.CosmoArrowTypeDetermined) return;

        var region = ev.SourcePosition.GetRadian(Center).RadianToRegion(8, isDiagDiv: true);
        _p6.CosmoArrowIsInner = region % 2 == 0;
        _p6.CosmoArrowTypeDetermined = true;
        sa.DebugMsg($"P6A_CosmoArrow_TypeDetermination: Phase {_parse} Cosmo Arrow, type is {(_p6.CosmoArrowIsInner ? "Inner" : "Outer")}", Debugging);
    }
    
    [ScriptMethod(name: "P6A_CosmoArrow_CastFinish", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31650"],
        userControl: Debugging)]
    public void P6A_CosmoArrow_CastFinish(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        _p6.CosmoArrowCastStart.Reset();
    }
    
    [ScriptMethod(name: "*P6A_CosmoArrow_HideEffects", eventType: EventTypeEnum.ActionEffect,
        eventCondition: ["ActionId:regex:^(3165[12])$", "TargetIndex:1"], userControl: true)]
    public void P6A_CosmoArrow_HideEffects(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        if (!SpecialMode) return;
        if (_p6.CosmoArrowJudgeCount > 4) return;
        sa.WriteVisible(sa.GetById(ev.SourceId), false);
    }
    
    [ScriptMethod(name: "P6A_CosmoArrow_JudgeCountIncrement", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3165[12])$"],
        userControl: Debugging, suppress: 500)]
    public void P6A_CosmoArrow_JudgeCountIncrement(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        _p6.CosmoArrowJudgeCount++;
        switch (_parse)
        {
            case 6.1 when _p6.CosmoArrowJudgeCount == 7:
                _p6.CosmicFlareDrawingStart.Set();
                if (!SpecialMode) return;
                ResetSupportUnitVisibility(sa);
                break;
            case 6.2 when _p6.CosmoArrowJudgeCount == 5:
                _p6.WaveCannonSpreadDrawingStart.Set();
                if (!SpecialMode) return;
                ResetSupportUnitVisibility(sa);
                break;
        }
    }
    
    [ScriptMethod(name: "P6A_CosmoArrow_Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31650"],
        userControl: true)]
    public void P6A_CosmoArrow_Drawing(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        sa.DebugMsg($"P6A_CosmoArrow_Drawing: Starting Cosmo Arrow drawing Framework", Debugging);
        _p6.CosmoArrowDrawingFramework = sa.Method.RegistFrameworkUpdateAction(Action);
        return;

        void Action()
        {
            int[] innerArrowPattern = [0, 12, 60, 195, 3, 12, 48, 192];
            int[] outerArrowPattern = [0, 12, 15, 51, 204, 48, 192];
            if (!_p6.CosmoArrowTypeDetermined) return;
            var cosmoArrowPattern = _p6.CosmoArrowIsInner ? innerArrowPattern : outerArrowPattern;
            if (_p6.CosmoArrowDrawingPatternIndex == _p6.CosmoArrowJudgeCount) return;
            _p6.CosmoArrowDrawingPatternIndex = _p6.CosmoArrowJudgeCount;
            
            var pattern = _p6.CosmoArrowDrawingPatternIndex >= cosmoArrowPattern.Length ? 0 : cosmoArrowPattern[_p6.CosmoArrowDrawingPatternIndex];
            DrawCosmoArrowPattern(pattern);
            
            sa.DebugMsg($"P6A_CosmoArrow_Drawing: Drawing Cosmo Arrow pattern index {_p6.CosmoArrowDrawingPatternIndex} ({pattern})", Debugging);
            return;
            
            void DrawCosmoArrowPattern(int pt)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (((pt >> i) & 1) == 1)
                    {
                        // Calculate center position
                        float biasZ = (2.5f + (i / 2) * 5) * (i % 2 == 0 ? -1 : 1);
                        sa.DrawRect(new Vector3(80f, 0, 100f + biasZ), 0, 20000,
                            $"P6A_CosmoArrow_Drawing_{i}", 90f.DegToRad(), 5f, 60f);
                        sa.DrawRect(new Vector3(100f + biasZ, 0, 80f), 0, 20000,
                            $"P6A_CosmoArrow_Drawing_{i}", 0, 5f, 60f);
                    }
                    else
                        sa.Method.RemoveDraw($"P6A_CosmoArrow_Drawing_{i}");
                }
            }
        }
    }
    
    [ScriptMethod(name: "P6A_CosmoArrow_Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31650"],
        userControl: true)]
    public void P6A_CosmoArrow_Guidance(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        sa.DebugMsg($"P6A_CosmoArrow_Guidance: Starting Cosmo Arrow guidance Framework", Debugging);
        _p6.CosmoArrowGuidanceFramework = sa.Method.RegistFrameworkUpdateAction(Action);
        return;

        void Action()
        {
            // Based on bottom right as reference
            int[][] cosmoArrowGuidancePattern =
            [
                [33, 11, 11, 55, 55, 55, 33],   // Inner Cosmo Arrow guidance pattern DPS
                [33, 55, 55, 33, 55, 33, 33],   // Outer Cosmo Arrow guidance pattern DPS
                [33, 11, 11, 33, 53, 88, 38],   // Inner Cosmo Arrow guidance pattern TN
                [33, 55, 55, 33, 88, 38, 38]    // Outer Cosmo Arrow guidance pattern TN
            ];
            
            if (!_p6.CosmoArrowTypeDetermined) return;
            int cosmoArrowGuidanceIdx, myRotation;
            int myIndex = sa.GetMyIndex();
            bool isFirstCosmoArrow = _parse == 6.1;
            if (isFirstCosmoArrow)
            {
                (cosmoArrowGuidanceIdx, myRotation) = myIndex switch
                {
                    0 => (2, 2),
                    1 => (0, 0),
                    _ => (0, 3)
                };
            }
            else
            {
                (cosmoArrowGuidanceIdx, myRotation) = myIndex switch
                {
                    0 => (2, 2),     // MT top left, up
                    1 => (2, 1),     // ST top right, right
                    2 => (2, 3),     // H1 bottom left, left
                    3 => (2, 0),     // H2 bottom right, down
                    4 => (0, 3),     // D1 bottom left
                    5 => (0, 0),     // D2 bottom right
                    6 => (0, 2),     // D3 top left
                    7 => (0, 1)      // D4 top right
                };
            }
            cosmoArrowGuidanceIdx += _p6.CosmoArrowIsInner ? 0 : 1;
            var cosmoArrowGuidance = cosmoArrowGuidancePattern[cosmoArrowGuidanceIdx];
            if (_p6.CosmoArrowGuidancePatternIndex == _p6.CosmoArrowJudgeCount) return;
            _p6.CosmoArrowGuidancePatternIndex = _p6.CosmoArrowJudgeCount;
            
            var guidance = _p6.CosmoArrowGuidancePatternIndex >= cosmoArrowGuidance.Length ? 0 : cosmoArrowGuidance[_p6.CosmoArrowGuidancePatternIndex];
            var nextGuidance = _p6.CosmoArrowGuidancePatternIndex + 1 >= cosmoArrowGuidance.Length ? 0 : cosmoArrowGuidance[_p6.CosmoArrowGuidancePatternIndex + 1];
            DrawCosmoArrowGuidance(guidance, nextGuidance, myRotation);
            sa.Method.RemoveDraw($"P6A_CosmoArrow_Guidance{_p6.CosmoArrowGuidancePatternIndex - 1}");
            sa.DebugMsg($"P6A_CosmoArrow_Guidance: Drawing Cosmo Arrow guidance pattern index {_p6.CosmoArrowGuidancePatternIndex} {guidance} {myRotation} {nextGuidance}", Debugging);
            return;
            
            void DrawCosmoArrowGuidance(int gd, int nextGd, int rot)
            {
                if (gd == 0) return;
                var guidancePos = GetGuidancePos(gd, rot);
                if (nextGd != 0)
                {
                    var nextGuidancePos = GetGuidancePos(nextGd, rot);
                    sa.DrawGuidance(guidancePos, nextGuidancePos, 0, 20000, $"P6A_CosmoArrow_Guidance{_p6.CosmoArrowGuidancePatternIndex}", isSafe: false);
                }
                sa.DrawGuidance(guidancePos, 0, 20000, $"P6A_CosmoArrow_Guidance{_p6.CosmoArrowGuidancePatternIndex}", isSafe: true);
            }

            Vector3 GetGuidancePos(int gd, int rot)
            {
                var (digitX, digitZ) = (gd.GetDecimalDigit(1), gd.GetDecimalDigit(2));
                var biasX = digitX == 8 ? 0f : (2.5f + (digitX / 2) * 5) * (digitX % 2 == 0 ? -1 : 1);
                var biasZ = digitZ == 8 ? 13f : (2.5f + (digitZ / 2) * 5) * (digitZ % 2 == 0 ? -1 : 1);
                Vector3 guidancePos = new Vector3(100 + biasX, 0, 100 + biasZ).RotateAndExtend(Center, rot * 90f.DegToRad());
                return guidancePos;
            }
        }
    }

    #endregion P6A Cosmo Arrow

    #region P6B Unlimited Wave Cannon

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP6B Unlimited Wave Cannonã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P6B_UnlimitedWaveCannon_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P6B_UnlimitedWaveCannon_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31660"],
        userControl: Debugging)]
    public void P6B_UnlimitedWaveCannon_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        sa.DebugMsg($"Current phase: {_parse} Unlimited Wave Cannon", Debugging);
        _p6.ResetAutoAttack(sa, false);
    }
    
    [ScriptMethod(name: "P6B_UnlimitedWaveCannon_GuidanceCenter", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31660"],
        userControl: true)]
    public void P6B_UnlimitedWaveCannon_GuidanceCenter(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        sa.DrawGuidance(Center, 0, 10000, $"P6A_UnlimitedWaveCannon_GuidanceCenter");
    }
    
    [ScriptMethod(name: "P6B_UnlimitedWaveCannon_CalculateMoveDirection", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31661"],
        userControl: Debugging)]
    public void P6B_UnlimitedWaveCannon_CalculateMoveDirection(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        var region = ev.SourcePosition.GetRadian(Center).RadianToRegion(8, isDiagDiv: true);
        if (_p6.UnlimitedWaveCannonFirstCannonDirection == -1)
        {
            _p6.UnlimitedWaveCannonFirstCannonDirection = region;
        }
        else if (!_p6.UnlimitedWaveCannonDirectionDetermined)
        {
            var diff = _p6.UnlimitedWaveCannonFirstCannonDirection - region;
            _p6.UnlimitedWaveCannonIsClockwise = diff is 1 or -7;
            _p6.UnlimitedWaveCannonDirectionDetermined = true;
            _p6.UnlimitedWaveCannonMovementDirectionDrawing.Set();
        }
    }
    
    [ScriptMethod(name: "P6B_UnlimitedWaveCannon_MovementDirectionDrawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31660"],
        userControl: true)]
    public void P6B_UnlimitedWaveCannon_MovementDirectionDrawing(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        _p6.UnlimitedWaveCannonMovementDirectionDrawing.WaitOne();
        DrawCannonRoute(false);
        _p6.YellowCircleUnderfoot.Set();
        DrawCannonRoute(true);
        if (_parse == 6.1)  // Along with spread wave cannon
        {
            _p6.WaveCannonSpreadDrawingStart.WaitOne();
            sa.Method.RemoveDraw($"P6B_UnlimitedWaveCannon_MovementDirectionDrawing.*");
            
            List<int> partySpreadRegion = [4, 2, 6, 0, 7, 1, 5, 3];
            var pos = new Vector3(100, 0, 114f).RotateAndExtend(Center, partySpreadRegion[sa.GetMyIndex()] * 45f.DegToRad());
            sa.DrawGuidance(pos, 0, 20000, $"P6_WaveCannon_SpreadGuidance");
            for (int i = 0; i < 8; i++)
            {
                var dp = sa.DrawLine(Center, 0, 0, 20000, $"P6_WaveCannon_GuideLine{i}", partySpreadRegion[i] * 45f.DegToRad(), 40f, 20f, draw: false);
                dp.Color = i switch
                {
                    0 or 1 => new Vector4(0.1f, 0.1f, 1, 1),
                    2 or 3 => new Vector4(0.1f, 1f, 0.1f, 1),
                    _ => new Vector4(1, 0.1f, 0.1f, 1),
                };
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Line, dp);
            }
        }
        
        void DrawCannonRoute(bool isSafe)
        {
            var isCw = _p6.UnlimitedWaveCannonIsClockwise;
            var startRegion = (_p6.UnlimitedWaveCannonFirstCannonDirection + (isCw ? 1 : -1) + 8) % 8;
            var startDonutRad = (_p6.UnlimitedWaveCannonFirstCannonDirection * 45f).DegToRad();
            var startLineRad = (startRegion * 45f).DegToRad() + (isCw ? 22.5f : -22.5f).DegToRad();
            
            sa.DebugMsg($"Start drawing wave cannon route, isCW {isCw}, startRegion {startRegion}, startDonutRad {startDonutRad.RadToDeg()}, startLineRad {startLineRad.RadToDeg()}", Debugging);
            
            var dp1 = sa.DrawRect(Center, 0, 0, 20000, $"P6B_UnlimitedWaveCannon_MovementDirectionDrawing_Line_{isSafe}",
                startLineRad, 2f, 15f, isSafe, draw: false);
            var dp2 = sa.DrawFan(Center, 0, 0, 20000, $"P6B_UnlimitedWaveCannon_MovementDirectionDrawing_Route_{isSafe}",
                135f.DegToRad(), startDonutRad, 15f, 13f, isSafe, draw: false);
            dp1.Color = dp1.Color.WithW(3f);
            dp2.Color = dp2.Color.WithW(3f);
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1);
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
            
            sa.Method.RemoveDraw($"P6B_UnlimitedWaveCannon_MovementDirectionDrawing_Route_{!isSafe}");
            sa.Method.RemoveDraw($"P6B_UnlimitedWaveCannon_MovementDirectionDrawing_Line_{!isSafe}");
        }
    }

    [ScriptMethod(name: "P6B_UnlimitedWaveCannon_YellowCircleUnderfoot", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31663"],
        userControl: Debugging, suppress: 500)]
    public void P6B_UnlimitedWaveCannon_YellowCircleUnderfoot(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        _p6.PlayerYellowCircleUnderfootRound++;
        switch (_p6.PlayerYellowCircleUnderfootRound)
        {
            case 1:
                _p6.YellowCircleUnderfoot.Set();
                sa.Method.RemoveDraw($"P6A_UnlimitedWaveCannon_GuidanceCenter");
                break;
            case 4:
                _p6.YellowCircleUnderfoot.Reset();
                _p6.UnlimitedWaveCannonMovementDirectionDrawing.Reset();
                break;
        }
        
        switch (_parse)
        {
            case 6.1 when _p6.PlayerYellowCircleUnderfootRound == 5:
                _p6.WaveCannonSpreadDrawingStart.Set();
                break;
            case 6.2 when _p6.PlayerYellowCircleUnderfootRound == 6:
                _p6.CosmicFlareDrawingStart.Set();
                break;
        }
    }

    #endregion P6B Unlimited Wave Cannon

    #region P6C Cosmic Meteor

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€” ã€ŠP6C Cosmic Meteorã€‹ â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void P6C_CosmicMeteor_Separator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "P6C_CosmicMeteor_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31664"],
        userControl: Debugging)]
    public void P6C_CosmicMeteor_PhaseSplit(Event ev, ScriptAccessory sa)
    {
        _parse = 6.3;
        _p6.ResetAutoAttack(sa, true);
        sa.DebugMsg($"Current phase: {_parse}", Debugging);
    }
    
    [ScriptMethod(name: "P6C_CosmicMeteor_GuidanceCenterAndSubsequentSpread", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31664"],
        userControl: true)]
    public void P6C_CosmicMeteor_GuidanceCenterAndSubsequentSpread(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        sa.DrawGuidance(Center, 0, 10000, $"P6C_CosmicMeteor_GuidanceCenterAndSubsequentSpread");
        List<int> partySpreadRegion = [5, 2, 6, 0, 7, 1, 4, 3];
        var pos = new Vector3(100, 0, 114f).RotateAndExtend(Center, partySpreadRegion[sa.GetMyIndex()] * 45f.DegToRad());
        sa.DrawGuidance(Center, pos, 0, 20000, $"P6C_CosmicMeteor_GuidanceCenterAndSubsequentSpread", isSafe: false);
    }

    [ScriptMethod(name: "P6C_CosmicMeteor_RemoveCenterGuidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31666"],
        userControl: Debugging, suppress: 500)]
    public void P6C_CosmicMeteor_RemoveCenterGuidance(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        sa.Method.RemoveDraw($"P6C_CosmicMeteor_GuidanceCenterAndSubsequentSpread");
    }

    [ScriptMethod(name: "P6C_CosmicMeteor_SpreadGuidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31666"],
        userControl: true, suppress: 500)]
    public void P6C_CosmicMeteor_SpreadGuidance(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        List<int> partySpreadRegion = [5, 2, 6, 0, 7, 1, 4, 3];
        var pos = new Vector3(100, 0, 114f).RotateAndExtend(Center, partySpreadRegion[sa.GetMyIndex()] * 45f.DegToRad());
        sa.DrawGuidance(pos, 0, 20000, $"P6C_CosmicMeteor_SpreadGuidance", isSafe: true);
        for (int i = 0; i < 8; i++)
        {
            var dp = sa.DrawLine(Center, 0, 0, 20000, $"P6C_CosmicMeteor_GuideLine{i}", partySpreadRegion[i] * 45f.DegToRad(), 40f, 20f, draw: false);
            dp.Color = i switch
            {
                0 or 1 => new Vector4(0.1f, 0.1f, 1, 1),
                2 or 3 => new Vector4(0.1f, 1f, 0.1f, 1),
                _ => new Vector4(1, 0.1f, 0.1f, 1),
            };
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Line, dp);
        }
    }
    
    [ScriptMethod(name: "P6C_CosmicMeteor_RemoveSpreadGuidanceAndLines", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:32699"],
        userControl: Debugging, suppress: 500)]
    public void P6C_CosmicMeteor_RemoveSpreadGuidanceAndLines(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        sa.Method.RemoveDraw($"P6C_CosmicMeteor_SpreadGuidance");
        sa.Method.RemoveDraw($"P6C_CosmicMeteor_GuideLine.*");
    }
    
    [ScriptMethod(name: "P6C_CosmicMeteor_MeteorTargetCollection", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:015A"],
        userControl: Debugging)]
    public void P6C_CosmicMeteor_MeteorTargetCollection(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        lock (_p6.MeteorTargets)
        {
            _p6.MeteorTargets.Add(sa.GetPlayerIdIndex((uint)ev.TargetId));
            if (_p6.MeteorTargets.Count < 3) return;
            _p6.MeteorTargetsCollected.Set();
        }
    }

    [ScriptMethod(name: "P6C_CosmicMeteor_MeteorTargetLines", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:015A"],
        userControl: Debugging, suppress: 500)]
    public void P6C_CosmicMeteor_MeteorTargetLines(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        _p6.MeteorTargetsCollected.WaitOne();
        
        var dp1 = sa.DrawGuidance(sa.Data.PartyList[_p6.MeteorTargets[0]], sa.Data.PartyList[_p6.MeteorTargets[1]],
            0, 10000, $"P6C_CosmicMeteor_MeteorTargetLinesAndGuidance01", draw: false);
        var dp2 = sa.DrawGuidance(sa.Data.PartyList[_p6.MeteorTargets[1]], sa.Data.PartyList[_p6.MeteorTargets[2]],
            0, 10000, $"P6C_CosmicMeteor_MeteorTargetLinesAndGuidance12", draw: false);
        var dp3 = sa.DrawGuidance(sa.Data.PartyList[_p6.MeteorTargets[2]], sa.Data.PartyList[_p6.MeteorTargets[0]],
            0, 10000, $"P6C_CosmicMeteor_MeteorTargetLinesAndGuidance20", draw: false);
        dp1.Color = new Vector4(0f, 0f, 0f, 3f);
        dp2.Color = new Vector4(0f, 0f, 0f, 3f);
        dp3.Color = new Vector4(0f, 0f, 0f, 3f);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp1);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp2);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp3);
        dp1.Color = new Vector4(1f, 1f, 0f, 1f);
        dp2.Color = new Vector4(1f, 1f, 0f, 1f);
        dp3.Color = new Vector4(1f, 1f, 0f, 1f);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp1);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp2);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp3);
    }

    [ScriptMethod(name: "P6C_CosmicMeteor_MeteorGuidance", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:015A"],
        userControl: Debugging, suppress: 500)]
    public void P6C_CosmicMeteor_MeteorGuidance(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        _p6.MeteorTargetsCollected.WaitOne();

        // Crowd safe zone
        var crowdSafeRegion = _p6.MeteorTargets.Contains(6) ? 0 : 2;
        
        if (_p6.MeteorTargets.Contains(sa.GetMyIndex()))
        {
            // Meteor target includes self, draw danger zone and guide lines

            sa.DrawFan(Center, 0, 20000, $"P6C_CosmicMeteor_CrowdSafeZoneIndicator",
                150f.DegToRad(), crowdSafeRegion * 90f.DegToRad(), 30f, 0f);
            
            for (int i = 0; i < 3; i++)
            {
                sa.DrawLine(Center, 0, 0, 20000, $"P6C_CosmicMeteor_MeteorGuideLine{i}",
                    (i - 1) * 90f.DegToRad(), 40f, 20f, isSafe: true);
            }
        }
        else
        {
            sa.DrawGuidance(new Vector3(100, 0, 114.5f).RotateAndExtend(Center, crowdSafeRegion * 90f.DegToRad()), 0,
                20000, $"P6C_CosmicMeteor_MeteorGuidance");
        }
    }

    [ScriptMethod(name: "P6C_CosmicMeteor_RemoveCosmicMeteorDrawings", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31668"],
        userControl: Debugging, suppress: 500)]
    public void P6C_CosmicMeteor_RemoveCosmicMeteorDrawings(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        _p6.MeteorTargetsCollected.Reset();
        _p6.Reset(sa);
        _p6.Dispose();
        sa.Method.RemoveDraw(".*");
    }
    
    [ScriptMethod(name: "P6C_CosmicMeteor_AfterMechanicEnd", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31668"],
        userControl: true, suppress: 500)]
    public void P6C_CosmicMeteor_AfterMechanicEnd(Event ev, ScriptAccessory sa)
    {
        if (_parse < 6) return;
        // If all players are alive
        foreach (var member in sa.Data.PartyList)
        {
            var obj = sa.GetById(member);
            if (obj == null) return;
            if (obj.IsDead) return;
        }
        sa.Method.TextInfo($"Beyond the limit, embrace victory!", 2000);
    }

    #endregion P6C Cosmic Meteor
    
    #region Priority Dictionary Class
    public class PriorityDict
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public ScriptAccessory sa {get; set;} = null!;
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public Dictionary<int, int> Priorities {get; set;} = null!;
        public string Annotation { get; set; } = "";
        public int ActionCount { get; set; } = 0;
        
        public void Init(ScriptAccessory accessory, string annotation, int partyNum = 8, bool refreshActionCount = true)
        {
            sa = accessory;
            Priorities = new Dictionary<int, int>();
            for (var i = 0; i < partyNum; i++)
            {
                Priorities.Add(i, 0);
            }
            Annotation = annotation;
            if (refreshActionCount)
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
            var sortedPriorities = SelectMiddlePriorityIndices(0, Priorities.Count, descending);
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
            var sortedPriorities = SelectMiddlePriorityIndices(0, Priorities.Count, descending);
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
                str += $"Key {pair.Key} {(showJob ? $"({Role[pair.Key]})" : "")}, Value {pair.Value}\n";
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

    #endregion Priority Dictionary Class

    #region Parameter Container Classes
    
    private class P1StateParams
    {
        public ulong BossId = 0;

        public Dictionary<int, Vector3> TowerDictionary = new();
        public Dictionary<int, bool> TetherDrawingDictionary = new();
        public int LineTowerRound = 0;
        public DateTime TowerTriggerTime = DateTime.MinValue;
        public ManualResetEvent EachRoundTowerCompleted = new ManualResetEvent(false);
        public ManualResetEvent PreviousDrawClearCompleted = new ManualResetEvent(false);
        public string TetherScanFramework = "";
        public string IdleGuidanceFramework = "";
        public int LastProximityStatus = 0;
        public bool TetherScanActive = false;
        
        public int OmniscientRound = 0;
        public bool OmniscientIsClockwise = false;
        public bool OmniscientStartingLineDrawn = false;
        public bool OmniscientDirectionDetermined = false;
        public float OmniscientFirstAngleStorage = -10;
        public ManualResetEvent OmniscientDirectionDeterminedEvent = new ManualResetEvent(false);
        public ManualResetEvent OmniscientFirstAngleStoredEvent = new ManualResetEvent(false);
        public int LastFarDistanceStatus = 0;
        public string OmniscientGoOutTimerFramework = "";
        public string OmniscientFarthestDistanceFramework = "";
        public int DiffuseWaveCannonCount = 0;
        
        public void Reset(ScriptAccessory sa)
        {
            BossId = 0;
            TowerDictionary = new Dictionary<int, Vector3>();
            TetherDrawingDictionary = new Dictionary<int, bool>();
            LineTowerRound = 0;
            TowerTriggerTime = DateTime.MinValue;
            
            LastProximityStatus = 0;
            TetherScanActive = false;
            OmniscientRound = 0;
            OmniscientIsClockwise = false;
            OmniscientStartingLineDrawn = false;
            OmniscientDirectionDetermined = false;
            OmniscientFirstAngleStorage = -10;
            
            LastFarDistanceStatus = 0;
            sa.Method.UnregistFrameworkUpdateAction(IdleGuidanceFramework);
            sa.Method.UnregistFrameworkUpdateAction(TetherScanFramework);
            sa.Method.UnregistFrameworkUpdateAction(OmniscientGoOutTimerFramework);
            sa.Method.UnregistFrameworkUpdateAction(OmniscientFarthestDistanceFramework);
            
            IdleGuidanceFramework = "";
            TetherScanFramework = "";
            OmniscientGoOutTimerFramework = "";
            OmniscientFarthestDistanceFramework = "";
            
            sa.DebugMsg($"P1 parameters reset", Debugging);
        }

        public void Dispose()
        {
            EachRoundTowerCompleted.Dispose();
            PreviousDrawClearCompleted.Dispose();
            OmniscientDirectionDeterminedEvent.Dispose();
            OmniscientFirstAngleStoredEvent.Dispose();
        }

        public void Register()
        {
            EachRoundTowerCompleted = new ManualResetEvent(false);
            PreviousDrawClearCompleted = new ManualResetEvent(false);
            OmniscientDirectionDeterminedEvent = new ManualResetEvent(false);
            OmniscientFirstAngleStoredEvent = new ManualResetEvent(false);
            EachRoundTowerCompleted.Reset();
            PreviousDrawClearCompleted.Reset();
            OmniscientDirectionDeterminedEvent.Reset();
            OmniscientFirstAngleStoredEvent.Reset();
        }
    }
    
    private class P2StateParams
    {
        public ulong BossIdMale = 0;
        public ulong BossIdFemale = 0;
        
        public int LastFirewallStatus = 0;
        public bool SynergyProgramIsFarTether = false;
        public int SelectableTargetCount = 0;
        public bool EnableFirewall = false;
        public bool TransitionArmFirstUpwardTriangle = false;
        public int LargeEyeDirection = 0;
        public int MaleChariotDirection = 0;

        public ManualResetEvent EyeLaserDirectionRecord = new(false);
        public ManualResetEvent StackRecord = new(false);
        public ManualResetEvent MaleChariotDirectionRecord = new(false);
        public ManualResetEvent FemaleKnockbackRecord = new(false);
        public ManualResetEvent MaleStackChariotRecord = new(false);
        public ManualResetEvent EyeLaserReadyForDraw = new(false);
        public ManualResetEvent TransitionMarkerRecord = new(false);
        
        public string FirewallTargetCheckFramework = "";
        
        public void Reset(ScriptAccessory sa)
        {
            BossIdMale = 0;
            BossIdFemale = 0;
            
            LastFirewallStatus = 0;
            SynergyProgramIsFarTether = false;
            SelectableTargetCount = 0;
            EnableFirewall = false;
            TransitionArmFirstUpwardTriangle = false;
            LargeEyeDirection = 0;
            MaleChariotDirection = 0;
            
            sa.Method.UnregistFrameworkUpdateAction(FirewallTargetCheckFramework);
            FirewallTargetCheckFramework = "";
        }

        public void Dispose()
        {
            EyeLaserDirectionRecord.Dispose();
            StackRecord.Dispose();
            MaleChariotDirectionRecord.Dispose();
            FemaleKnockbackRecord.Dispose();
            MaleStackChariotRecord.Dispose();
            EyeLaserReadyForDraw.Dispose();
            TransitionMarkerRecord.Dispose();
        }
        
        public void Register()
        {
            EyeLaserDirectionRecord = new ManualResetEvent(false);
            StackRecord = new ManualResetEvent(false);
            MaleChariotDirectionRecord = new ManualResetEvent(false);
            FemaleKnockbackRecord = new ManualResetEvent(false);
            MaleStackChariotRecord = new ManualResetEvent(false);
            EyeLaserReadyForDraw = new ManualResetEvent(false);
            TransitionMarkerRecord = new ManualResetEvent(false);
            EyeLaserDirectionRecord.Reset();
            StackRecord.Reset();
            MaleChariotDirectionRecord.Reset();
            FemaleKnockbackRecord.Reset();
            MaleStackChariotRecord.Reset();
            EyeLaserReadyForDraw.Reset();
            TransitionMarkerRecord.Reset();
        }
    }
    
    private class P3StateParams
    {
        public ulong BossId = 0;
        public int HelloWorldRound = 0;

        public int RedTowerPosition = 0;
        public int BlueTowerPosition = 0;
        public bool BigCircleIsRedTower = false;
        public bool BaldSmallScreenDirectionRight = false;
        public int SmallScreenPlayerFacing = 0;
        
        public ManualResetEvent RedBlueTowerDirectionRecord = new(false);
        public ManualResetEvent HelloWorldRoundRecord = new(false);
        public ManualResetEvent SmallScreenMarkerRecord = new(false);
        public ManualResetEvent BaldScanDirectionRecord = new(false);
        public ManualResetEvent SmallScreenPlayerFacingRecord = new(false);
        
        public string SmallScreenFacingAssistFramework = "";
        public DateTime SmallScreenFacingAssistTriggerTime = DateTime.MinValue;
        
        public void Reset(ScriptAccessory sa)
        {
            BossId = 0;
            HelloWorldRound = 0;
            RedTowerPosition = 0;
            BlueTowerPosition = 0;
            BigCircleIsRedTower = false;
            BaldSmallScreenDirectionRight = false;

            SmallScreenPlayerFacing = 0;
            SmallScreenFacingAssistTriggerTime = DateTime.MinValue;
            sa.Method.UnregistFrameworkUpdateAction(SmallScreenFacingAssistFramework);
            SmallScreenFacingAssistFramework = "";
        }

        public void Dispose()
        {
            RedBlueTowerDirectionRecord.Dispose();
            HelloWorldRoundRecord.Dispose();
            SmallScreenMarkerRecord.Dispose();
            BaldScanDirectionRecord.Dispose();
            SmallScreenPlayerFacingRecord.Dispose();
        }
        
        public void Register()
        {
            RedBlueTowerDirectionRecord = new ManualResetEvent(false);
            HelloWorldRoundRecord = new ManualResetEvent(false);
            SmallScreenMarkerRecord = new ManualResetEvent(false);
            BaldScanDirectionRecord = new ManualResetEvent(false);
            SmallScreenPlayerFacingRecord = new ManualResetEvent(false);
            RedBlueTowerDirectionRecord.Reset();
            HelloWorldRoundRecord.Reset();
            SmallScreenMarkerRecord.Reset();
            BaldScanDirectionRecord.Reset();
            SmallScreenPlayerFacingRecord.Reset();
        }
    }
    
    private class P4StateParams
    {
        public ulong BossId = 0;
        public int BlueScreenWaveCannonRound = 0;
        public ManualResetEvent WaveCannonInitRecord = new(false);
        public void Reset(ScriptAccessory sa)
        {
            BossId = 0;
            BlueScreenWaveCannonRound = 0;
        }

        public void Dispose()
        {
            WaveCannonInitRecord.Dispose();
        }
        
        public void Register()
        {
            WaveCannonInitRecord = new ManualResetEvent(false);
        }
    }
    
    private class P5AStateParams
    {
        public int BeetlePosition = 0;
        public int BaldPosition = 0;
        public int PlayerQuarterHalf = -1;
        public int FistCount = 0;
        public int FistColor = 0;
        public int PlayerGuideLaserArmDirection = 0;
        public int BaldLeftRightScan = 0;
        public int PlayerLeftRightScan = 0;
        public int BeetleLeftRightSword = 0;

        public Dictionary<int, Vector3> LaserArmDirectionDict = new();
    
        public bool EnablePlayerGuideLaserArmGuidance = false;
        public bool FirstFarTetherBroken = false;

        public ManualResetEvent FarTetherPartnerRecord = new(false);
        public ManualResetEvent MarkerRecord = new(false);
        public ManualResetEvent BaldBeetleLocate = new(false);
        public ManualResetEvent FistRecord = new(false);
        public ManualResetEvent ShieldComboRecord = new(false);
        public ManualResetEvent BeetleLeftRightSwordRecord = new(false);

        public string SmallScreenFacingAssistFramework = "";
        public DateTime SmallScreenFacingAssistTriggerTime = DateTime.MinValue;

        // One-click reset method
        public void Reset(ScriptAccessory sa)
        {
            BeetlePosition = 0;
            BaldPosition = 0;
            PlayerQuarterHalf = -1;
            FistCount = 0;
            FistColor = 0;
            PlayerGuideLaserArmDirection = 0;
            BaldLeftRightScan = 0;
            PlayerLeftRightScan = 0;
            BeetleLeftRightSword = 0;
            
            EnablePlayerGuideLaserArmGuidance = false;
            FirstFarTetherBroken = false;

            LaserArmDirectionDict = new Dictionary<int, Vector3>();
            SmallScreenFacingAssistTriggerTime = DateTime.MinValue;
            
            sa.Method.UnregistFrameworkUpdateAction(SmallScreenFacingAssistFramework);
            SmallScreenFacingAssistFramework = "";
        }
        
        public void Dispose()
        {
            FarTetherPartnerRecord.Dispose();
            MarkerRecord.Dispose();
            BaldBeetleLocate.Dispose();
            FistRecord.Dispose();
            ShieldComboRecord.Dispose();
            BeetleLeftRightSwordRecord.Dispose();
        }
        
        public void Register()
        {
            FarTetherPartnerRecord = new ManualResetEvent(false);
            MarkerRecord = new ManualResetEvent(false);
            BaldBeetleLocate = new ManualResetEvent(false);
            FistRecord = new ManualResetEvent(false);
            ShieldComboRecord = new ManualResetEvent(false);
            BeetleLeftRightSwordRecord = new ManualResetEvent(false);
            FarTetherPartnerRecord.Reset();
            MarkerRecord.Reset();
            BaldBeetleLocate.Reset();
            FistRecord.Reset();
            ShieldComboRecord.Reset();
            BeetleLeftRightSwordRecord.Reset();
        }
    }

    private class P5BStateParams
    {
        public bool SynergyProgramIsFarTether = false;
        public int MaleDirection = 0;
        public int PlayerSpreadDirection = 0;
        public int PlayerTowerToStand = 0;
        public Dictionary<int, bool> TowerDirectionTypeDict = new();    // Direction, whether it's a double tower
        public ManualResetEvent SpreadMarkersRecorded = new(false);
        public ManualResetEvent TowerDirectionRecorded = new(false);
        public ManualResetEvent TowerHandlingCompleted = new(false);
        
        public int FemaleDirection = 0;
        public bool FemaleIsCrossOuterSafe = false;
        public bool RingIsClockwise = false;
        public ulong RingId = 0;
        public ulong SecondTransferFemaleId = 0;
        public int RingAttackCount = 0;
        public ManualResetEvent RotatingMarkersRecorded = new(false);
        public ManualResetEvent RingDirectionRecorded = new(false);
        public ManualResetEvent FemaleSkillRecorded = new(false);
        public ManualResetEvent FemaleCrossHotWingCasted = new(false);
        
        public void Reset(ScriptAccessory sa)
        {
            SynergyProgramIsFarTether = false;
            MaleDirection = 0;
            PlayerSpreadDirection = 0;
            PlayerTowerToStand = 0;
            TowerDirectionTypeDict = new Dictionary<int, bool>();    // Direction, whether it's a double tower
            
            FemaleDirection = 0;
            FemaleIsCrossOuterSafe = false;
            RingIsClockwise = false;
            RingId = 0;
            SecondTransferFemaleId = 0;
            RingAttackCount = 0;
        }

        public void Dispose()
        {
            SpreadMarkersRecorded.Dispose();
            TowerDirectionRecorded.Dispose();
            TowerHandlingCompleted.Dispose();
            
            RotatingMarkersRecorded.Dispose();
            FemaleCrossHotWingCasted.Dispose();
            RingDirectionRecorded.Dispose();
            FemaleSkillRecorded.Dispose();
        }
        
        public void Register()
        {
            SpreadMarkersRecorded = new ManualResetEvent(false);
            TowerDirectionRecorded = new ManualResetEvent(false);
            TowerHandlingCompleted = new ManualResetEvent(false);
            
            RotatingMarkersRecorded = new ManualResetEvent(false);
            FemaleCrossHotWingCasted = new ManualResetEvent(false);
            RingDirectionRecorded = new ManualResetEvent(false);
            FemaleSkillRecorded = new ManualResetEvent(false);
            SpreadMarkersRecorded.Reset();
            TowerDirectionRecorded.Reset();
            TowerHandlingCompleted.Reset();
            RotatingMarkersRecorded.Reset();
            FemaleCrossHotWingCasted.Reset();
            RingDirectionRecorded.Reset();
            FemaleSkillRecorded.Reset();
        }

        public int FindPlayerTower(ScriptAccessory sa)
        {
            int[] towerRegionOffsets = [2, 1, -2, -1];
            
            int bestTower = -1;
            int highestScore = -1;
            
            foreach (int offset in towerRegionOffsets)
            {
                int region = (PlayerSpreadDirection + offset + 16) % 16;
                if (!TowerDirectionTypeDict.TryGetValue(region, out bool isDoubleTower))
                    continue;
                sa.DebugMsg($"Detected {(isDoubleTower ? "Double" : "Single")} tower at direction {region}", Debugging);
                int score = 1 + (isDoubleTower ? 10 : 0);
                if (score <= highestScore) continue;
                highestScore = score;
                bestTower = region;
                if (isDoubleTower) break;
            }
            
            sa.DebugMsg($"Player's assigned tower to stand: {bestTower}", Debugging);
            return bestTower;
        }
    }
    
    private class P5CStateParams
    {
        public Dictionary<int, (int, bool)> ComboSkillRecord = new();   // Direction, (skill type, is first round)
        public ManualResetEvent MaleFemaleComboSkillsRecorded = new(false);
        public ManualResetEvent ComboSkillSafeSpotRecorded = new(false);
        public ManualResetEvent FirstRoundComboEnd = new(false);
        public int[] ComboSkillSafeSpot = [];
        
        public int BeetleDirection = 0;
        public ulong BeetleId = 0;
        public bool TetherScanActive = false;
        public ManualResetEvent ThirdTransferMarkersRecorded = new(false);
        public ManualResetEvent FourthTransferMarkersRecorded = new(false);
        public ManualResetEvent BeetleDirectionRecorded = new(false);
        public Dictionary<int, bool> TetherDrawingDictionary = new();
        
        public string TetherScanFramework = "";
        
        public Dictionary<int, List<bool>> SkillTypeSafeZoneDict = new();    // Skill Type, Safe Zone List (12 points)
        public void Reset(ScriptAccessory sa)
        {
            BeetleDirection = 0;
            BeetleId = 0;
            TetherScanActive = false;
            ComboSkillRecord = new Dictionary<int, (int, bool)>();
            ComboSkillSafeSpot = [];
            TetherDrawingDictionary = new Dictionary<int, bool>();
            
            sa.Method.UnregistFrameworkUpdateAction(TetherScanFramework);
            TetherScanFramework = "";
            
            // 0~3 Chariot, 4~7 Donut, 8~11 Cross, 12~15 HotWing, 16 Bald horizontal swing, 17 Bald vertical swing
            SkillTypeSafeZoneDict = new Dictionary<int, List<bool>>
            {
                { 4, [true, true, false, false, true, true, false, false, false, false, false, false] },
                { 5, [false, true, true, false, false, true, true, false, false, false, false, false] },
                { 6, [false, false, true, true, false, false, true, true, false, false, false, false] },
                { 7, [true, false, false, true, true, false, false, true, false, false, false, false] },
                { 0, [false, false, true, true, false, false, true, true, true, true, true, true] },
                { 1, [true, false, false, true, true, false, false, true, true, true, true, true] },
                { 2, [true, true, false, false, true, true, false, false, true, true, true, true] },
                { 3, [false, true, true, false, false, true, true, false, true, true, true, true] },
                { 8, [false, false, false, false, false, false, true, true, false, false, true, true] },
                { 9, [false, false, false, false, true, false, false, true, true, false, false, true] },
                { 10, [false, false, false, false, true, true, false, false, true, true, false, false] },
                { 11, [false, false, false, false, false, true, true, false, false, true, true, false] },
                { 12, [true, true, true, true, false, false, false, false, false, false, false, false] },
                { 13, [true, true, true, true, false, false, false, false, false, false, false, false] },
                { 14, [true, true, true, true, false, false, false, false, false, false, false, false] },
                { 15, [true, true, true, true, false, false, false, false, false, false, false, false] },
                { 16, [true, false, true, false, true, false, true, false, true, false, true, false] },
                { 17, [false, true, false, true, false, true, false, true, false, true, false, true] }
            };
        }

        public void Dispose()
        {
            MaleFemaleComboSkillsRecorded.Dispose();
            ComboSkillSafeSpotRecorded.Dispose();
            FirstRoundComboEnd.Dispose();
            ThirdTransferMarkersRecorded.Dispose();
            FourthTransferMarkersRecorded.Dispose();
            BeetleDirectionRecorded.Dispose();
        }
        
        public void Register()
        {
            MaleFemaleComboSkillsRecorded = new ManualResetEvent(false);
            ComboSkillSafeSpotRecorded = new ManualResetEvent(false);
            FirstRoundComboEnd = new ManualResetEvent(false);
            ThirdTransferMarkersRecorded = new ManualResetEvent(false);
            FourthTransferMarkersRecorded = new ManualResetEvent(false);
            BeetleDirectionRecorded = new ManualResetEvent(false);
            MaleFemaleComboSkillsRecorded.Reset();
            ComboSkillSafeSpotRecorded.Reset();
            FirstRoundComboEnd.Reset();
            ThirdTransferMarkersRecorded.Reset();
            FourthTransferMarkersRecorded.Reset();
            BeetleDirectionRecorded.Reset();
        }

        public void FindComboAttackSafePoint(ScriptAccessory sa)
        {
            // Use local functions to unify logic for both rounds, eliminating duplicate code
            // return (FindSafePointForRound(isFirstRound: true, sa), FindSafePointForRound(isFirstRound: false, sa));
            ComboSkillSafeSpot = [FindSafePointForRound(isFirstRound: true, sa), FindSafePointForRound(isFirstRound: false, sa)];
        }

        private int FindSafePointForRound(bool isFirstRound, ScriptAccessory sa)
        {
            var roundSkills = ComboSkillRecord.Where(kvp => kvp.Value.Item2 == isFirstRound).ToList();
            bool[] safePoints = new bool[12]; Array.Fill(safePoints, true);
            foreach (var (region, (skill, _)) in roundSkills)
            {
                sa.DebugMsg($"Found {(isFirstRound ? "first" : "second")} round skill {skill} at direction {region}", Debugging);
                var skillSafeZones = SkillTypeSafeZoneDict[skill];
                for (int i = 0; i < 12; i++)
                    safePoints[i] &= skillSafeZones[i];
            }
            int safeIndex = Array.IndexOf(safePoints, true);
            sa.DebugMsg(safeIndex == -1
                    ? $"No safe point found for {(isFirstRound ? "first" : "second")} round"
                    : $"Safe point index for {(isFirstRound ? "first" : "second")} round: {safeIndex}", Debugging);

            return safeIndex;
        }
    }
    
    private class P6StateParams
    {
        public ulong BossId = 0;
        public bool EnableAutoAttackDrawing = false;
        public string AutoAttackDrawingFramework = "";
        public bool HasDrawnAwayIndicatorLine = false;
        public bool HasGivenEstablishAggroHint = false;
        public int FarthestPlayerIdx = -1;
        public int MainTankPlayerIdx = -1;
        
        public bool CosmoArrowIsInner = false;
        public bool CosmoArrowTypeDetermined = false;
        public int CosmoArrowJudgeCount = 0;
        public int CosmoArrowDrawingPatternIndex = -1;
        public int CosmoArrowGuidancePatternIndex = -1;
        public string CosmoArrowDrawingFramework = "";
        public string CosmoArrowGuidanceFramework = "";

        public int UnlimitedWaveCannonFirstCannonDirection = -1;
        public bool UnlimitedWaveCannonIsClockwise = false;
        public int PlayerYellowCircleUnderfootRound = 0;
        public bool UnlimitedWaveCannonDirectionDetermined = false;

        public int WaveCannonJudgeCount = 0;
        
        public List<int> MeteorTargets = [];
        
        public ManualResetEvent CosmoArrowCastStart = new(false);
        public ManualResetEvent CosmicFlareDrawingStart = new(false);
        public ManualResetEvent WaveCannonSpreadDrawingStart = new(false);
        public ManualResetEvent UnlimitedWaveCannonMovementDirectionDrawing = new(false);
        public ManualResetEvent YellowCircleUnderfoot = new(false);
        public ManualResetEvent WaveCannonStackDrawingStart = new(false);
        public ManualResetEvent MeteorTargetsCollected = new(false);
        
        public void Reset(ScriptAccessory sa)
        {
            BossId = 0;
            MeteorTargets = [];
            ResetAutoAttack(sa, true);
            ResetCosmoArrow(sa);
            ResetUnlimitedWaveCannon();
            ResetSpreadWaveCannon();
        }

        public void ResetAutoAttack(ScriptAccessory sa, bool unRegist)
        {
            EnableAutoAttackDrawing = false;
            HasDrawnAwayIndicatorLine = false;
            HasGivenEstablishAggroHint = false;
            FarthestPlayerIdx = -1;
            MainTankPlayerIdx = -1;
            sa.Method.RemoveDraw($"P6_AutoAttackDrawing.*");
            if (!unRegist) return;
            sa.Method.UnregistFrameworkUpdateAction(AutoAttackDrawingFramework);
        }
        
        public void ResetCosmoArrow(ScriptAccessory sa)
        {
            CosmoArrowIsInner = false;
            CosmoArrowTypeDetermined = false;
            CosmoArrowJudgeCount = 0;
            CosmoArrowDrawingPatternIndex = -1;
            CosmoArrowGuidancePatternIndex = -1;
            sa.Method.UnregistFrameworkUpdateAction(CosmoArrowDrawingFramework);
            sa.Method.UnregistFrameworkUpdateAction(CosmoArrowGuidanceFramework);
        }

        public void ResetUnlimitedWaveCannon()
        {
            UnlimitedWaveCannonFirstCannonDirection = -1;
            UnlimitedWaveCannonIsClockwise = false;
            PlayerYellowCircleUnderfootRound = 0;
            UnlimitedWaveCannonDirectionDetermined = false;
        }

        public void ResetSpreadWaveCannon()
        {
            WaveCannonJudgeCount = 0;
        }

        public void Dispose()
        {
            CosmoArrowCastStart.Dispose();
            CosmicFlareDrawingStart.Dispose();
            WaveCannonSpreadDrawingStart.Dispose();
            UnlimitedWaveCannonMovementDirectionDrawing.Dispose();
            YellowCircleUnderfoot.Dispose();
            WaveCannonStackDrawingStart.Dispose();
            MeteorTargetsCollected.Dispose();
        }
        
        public void Register()
        {
            CosmoArrowCastStart = new ManualResetEvent(false);
            CosmicFlareDrawingStart = new ManualResetEvent(false);
            WaveCannonSpreadDrawingStart = new ManualResetEvent(false);
            UnlimitedWaveCannonMovementDirectionDrawing = new ManualResetEvent(false);
            YellowCircleUnderfoot = new ManualResetEvent(false);
            WaveCannonStackDrawingStart = new ManualResetEvent(false);
            MeteorTargetsCollected = new ManualResetEvent(false);
            CosmoArrowCastStart.Reset();
            CosmicFlareDrawingStart.Reset();
            WaveCannonSpreadDrawingStart.Reset();
            UnlimitedWaveCannonMovementDirectionDrawing.Reset();
            YellowCircleUnderfoot.Reset();
            WaveCannonStackDrawingStart.Reset();
            MeteorTargetsCollected.Reset();
        }
    }
    #endregion Parameter Container Classes
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

    public static uint Id0(this Event @event)
    {
        return ParseHexId(@event["Id"], out var id) ? id : 0;
    }
    
    public static uint Index(this Event ev)
    {
        return JsonConvert.DeserializeObject<uint>(ev["Index"]);
    }
    
    public static uint DataId(this Event ev)
    {
        return JsonConvert.DeserializeObject<uint>(ev["DataId"]);
    }
}

public static class IbcHelper
{
    public static IGameObject? GetById(this ScriptAccessory sa, ulong gameObjectId)
    {
        return sa.Data.Objects.SearchById(gameObjectId);
    }
    
    public static IEnumerable<IGameObject?> GetByDataId(this ScriptAccessory sa, uint dataId)
    {
        return sa.Data.Objects.Where(x => x.DataId == dataId);
    }

    public static float GetStatusRemainingTime(this ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return 0;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return charaStruct->GetStatusManager()->GetRemainingTime(statusIdx);
        }
    }
    
    public static List<ulong> GetTetherSource(this ScriptAccessory sa, IBattleChara? battleChara, uint tetherId)
    {
        List<ulong> tetherSourceId = [];
        if (battleChara == null || !battleChara.IsValid()) return [];
        unsafe
        {
            BattleChara* chara = (BattleChara*)battleChara.Address;
            var tetherList = chara->Vfx.Tethers;

            foreach (var tether in tetherList)
            {
                if (tether.Id != tetherId) continue;
                tetherSourceId.Add(tether.TargetId.ObjectId);
            }
        }
        return tetherSourceId;
    }
    
    public static unsafe byte? GetTransformationId(this ScriptAccessory sa, IGameObject? obj)
    {
        if (obj == null) return null;
        Character* objStruct = (Character*)obj.Address;
        return objStruct->Timeline.ModelState;
    }
    
}
#region Calculation Functions

public static class MathTools
{
    public static float DegToRad(this float deg) => (deg + 360f) % 360f / 180f * float.Pi;
    public static float RadToDeg(this float rad) => (rad + 2 * float.Pi) % (2 * float.Pi) / float.Pi * 180f;
    
    /// <summary>
    /// Get the radian value of an arbitrary point relative to a center point, with direction (0, 0, 1) as 0 and (1, 0, 0) as pi/2.
    /// I.e., increases counter-clockwise.
    /// </summary>
    /// <param name="point">Arbitrary point</param>
    /// <param name="center">Center point</param>
    /// <returns></returns>
    public static float GetRadian(this Vector3 point, Vector3 center)
        => MathF.Atan2(point.X - center.X, point.Z - center.Z);

    /// <summary>
    /// Get the length between an arbitrary point and a center point.
    /// </summary>
    /// <param name="point">Arbitrary point</param>
    /// <param name="center">Center point</param>
    /// <returns></returns>
    public static float GetLength(this Vector3 point, Vector3 center)
        => new Vector2(point.X - center.X, point.Z - center.Z).Length();
    
    /// <summary>
    /// Rotate an arbitrary point counter-clockwise around a center point and extend it.
    /// </summary>
    /// <param name="point">Arbitrary point</param>
    /// <param name="center">Center point</param>
    /// <param name="radian">Rotation radian</param>
    /// <param name="length">Extension length based on the point</param>
    /// <returns></returns>
    public static Vector3 RotateAndExtend(this Vector3 point, Vector3 center, float radian, float length = 0)
    {
        var baseRad = point.GetRadian(center);
        var baseLength = point.GetLength(center);
        var rotRad = baseRad + radian;
        return new Vector3(
            center.X + MathF.Sin(rotRad) * (length + baseLength),
            center.Y,
            center.Z + MathF.Cos(rotRad) * (length + baseLength)
        );
    }
    
    /// <summary>
    /// Get the divided region number of a given angle.
    /// </summary>
    /// <param name="radian">Input radian</param>
    /// <param name="regionNum">Number of region divisions</param>
    /// <param name="baseRegionIdx">Initial index of the 0-degree region</param>>
    /// <param name="isDiagDiv">Is diagonal division, default false</param>
    /// <param name="isCw">Is increasing clockwise, default false</param>
    /// <returns></returns>
    public static int RadianToRegion(this float radian, int regionNum, int baseRegionIdx = 0, bool isDiagDiv = false, bool isCw = false)
    {
        var sepRad = float.Pi * 2 / regionNum;
        var inputAngle = radian * (isCw ? -1 : 1) + (isDiagDiv ? sepRad / 2 : 0);
        var rad = (inputAngle + 4 * float.Pi) % (2 * float.Pi);
        return ((int)Math.Floor(rad / sepRad) + baseRegionIdx + regionNum) % regionNum;
    }
    
    /// <summary>
    /// Fold the input point horizontally.
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerX">Center axis X coordinate</param>
    /// <returns></returns>
    public static Vector3 FoldPointHorizon(this Vector3 point, float centerX)
        => point with { X = 2 * centerX - point.X };

    /// <summary>
    /// Fold the input point vertically.
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerZ">Center axis Z coordinate</param>
    /// <returns></returns>
    public static Vector3 FoldPointVertical(this Vector3 point, float centerZ)
        => point with { Z = 2 * centerZ - point.Z };

    /// <summary>
    /// Center symmetry of the input point.
    /// </summary>
    /// <param name="point">Input point</param>
    /// <param name="center">Center point</param>
    /// <returns></returns>
    public static Vector3 PointCenterSymmetry(this Vector3 point, Vector3 center) 
        => point.RotateAndExtend(center, float.Pi, 0);
    
    /// <summary>
    /// Get the specified digit of a number.
    /// </summary>
    /// <param name="val">Given integer</param>
    /// <param name="x">Corresponding digit, units digit is 1</param>
    /// <returns></returns>
    public static int GetDecimalDigit(this int val, int x)
    {
        var valStr = val.ToString();
        var length = valStr.Length;
        if (x < 1 || x > length) return 0;
        var digitChar = valStr[length - x]; // Take the x-th digit from the right
        return int.Parse(digitChar.ToString());
    }

    /// <summary>
    /// Get the difference between two radians (rad to radReference). Difference > 0 indicates counter-clockwise.
    /// </summary>
    /// <param name="rad">Measured angle</param>
    /// <param name="radReference">Reference angle</param>
    /// <returns></returns>
    public static float GetDiffRad(this float rad, float radReference)
    {
        var diff = (rad - radReference + 4 * float.Pi) % (2 * float.Pi);
        if (diff > float.Pi) diff -= 2 * float.Pi;
        return diff;
    }
}

#endregion Calculation Functions

#region Index Helper Functions
public static class IndexHelper
{
    /// <summary>
    /// Input player dataId, get the corresponding position index
    /// </summary>
    /// <param name="pid">Player SourceId</param>
    /// <param name="sa"></param>
    /// <returns>The position index corresponding to the player</returns>
    public static int GetPlayerIdIndex(this ScriptAccessory sa, uint pid)
    {
        // Get player IDX
        return sa.Data.PartyList.IndexOf(pid);
    }

    /// <summary>
    /// Get the position index of the main perspective player
    /// </summary>
    /// <param name="sa"></param>
    /// <returns>Position index of the main perspective player</returns>
    public static int GetMyIndex(this ScriptAccessory sa)
    {
        return sa.Data.PartyList.IndexOf(sa.Data.Me);
    }

    /// <summary>
    /// Input position index, get the corresponding position name, output string only for text output
    /// </summary>
    /// <param name="idx">Position index</param>
    /// <param name="fourPeople">Is it a 4-man dungeon</param>
    /// <param name="sa"></param>
    /// <returns></returns>
    public static string GetPlayerJobByIndex(this ScriptAccessory sa, int idx, bool fourPeople = false)
    {
        List<string> role8 = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
        List<string> role4 = ["T", "H", "D1", "D2"];
        if (idx < 0 || idx >= 8 || (fourPeople && idx >= 4))
            return "Unknown";
        return fourPeople ? role4[idx] : role8[idx];
    }
    
}
#endregion Index Helper Functions

#region Drawing Functions

public static class DrawTools
{
    /// <summary>
    /// Return drawing properties
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Drawing base, can be UID or position</param>
    /// <param name="targetObj">Drawing target, can be UID or position</param>
    /// <param name="delay">Delay in ms before appearance</param>
    /// <param name="destroy">Disappears after `destroy` ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="radian">Radian range of the drawing shape</param>
    /// <param name="rotation">Rotation radian of the drawing shape, relative to owner's forward direction, increasing counter-clockwise</param>
    /// <param name="width">Width of the drawing shape</param>
    /// <param name="length">Length of the drawing shape</param>
    /// <param name="innerWidth">Inner width of the drawing shape</param>
    /// <param name="innerLength">Inner length of the drawing shape</param>
    /// <param name="drawModeEnum">Drawing mode</param>
    /// <param name="drawTypeEnum">Drawing type</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <param name="byY">Animation based on distance change</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawOwnerBase(this ScriptAccessory sa, 
        object ownerObj, object targetObj, int delay, int destroy, string name, 
        float radian, float rotation, float width, float length, float innerWidth, float innerLength,
        DrawModeEnum drawModeEnum, DrawTypeEnum drawTypeEnum, bool isSafe = false,
        bool byTime = false, bool byY = false, bool draw = true)
    {
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.InnerScale = new Vector2(innerWidth, innerLength);
        dp.Radian = radian;
        dp.Rotation = rotation;
        dp.Color = isSafe ? sa.Data.DefaultSafeColor: sa.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        dp.ScaleMode |= byY ? ScaleMode.YByDistance : ScaleMode.None;
        
        switch (ownerObj)
        {
            case uint u:
                dp.Owner = u;
                break;
            case ulong ul:
                dp.Owner = ul;
                break;
            case Vector3 spos:
                dp.Position = spos;
                break;
            default:
                throw new ArgumentException($"ownerObj type {ownerObj.GetType()} input error");
        }

        switch (targetObj)
        {
            case 0:
            case 0u:
                break;
            case uint u:
                dp.TargetObject = u;
                break;
            case ulong ul:
                dp.TargetObject = ul;
                break;
            case Vector3 tpos:
                dp.TargetPosition = tpos;
                break;
            default:
                throw new ArgumentException($"targetObj type {targetObj.GetType()} input error");
        }
        
        if (draw)
            sa.Method.SendDraw(drawModeEnum, drawTypeEnum, dp);
        return dp;
    }

    /// <summary>
    /// Return guidance drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Start point</param>
    /// <param name="targetObj">End point</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="rotation">Arrow rotation angle</param>
    /// <param name="width">Arrow width</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawGuidance(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name,
        float rotation = 0, float width = 1f, bool isSafe = true, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, 0, rotation, width,
            width, 0, 0, DrawModeEnum.Imgui, DrawTypeEnum.Displacement, isSafe, false, true, draw);
    
    public static DrawPropertiesEdit DrawGuidance(this ScriptAccessory sa,
        object targetObj, int delay, int destroy, string name, float rotation = 0, float width = 1f, bool isSafe = true,
        bool draw = true)
        => sa.DrawGuidance((ulong)sa.Data.Me, targetObj, delay, destroy, name, rotation, width, isSafe, draw);

    /// <summary>
    /// Return circle drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Center point</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="scale">Circle radius/diameter</param>
    /// <param name="byTime">Expand over time</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawCircle(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name,
        float scale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, 0, delay, destroy, name, 2 * float.Pi, 0, scale, scale,
            0, 0, DrawModeEnum.Default, DrawTypeEnum.Circle, isSafe, byTime,false, draw);

    /// <summary>
    /// Return donut drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Center point</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="outScale">Outer radius</param>
    /// <param name="innerScale">Inner radius</param>
    /// <param name="byTime">Expand over time</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawDonut(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name,
        float outScale, float innerScale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, 0, delay, destroy, name, 2 * float.Pi, 0, outScale, outScale, innerScale,
            innerScale, DrawModeEnum.Default, DrawTypeEnum.Donut, isSafe, byTime, false, draw);

    /// <summary>
    /// Return fan drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Center point</param>
    /// <param name="targetObj">Target</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="radian">Radian</param>
    /// <param name="rotation">Rotation angle</param>
    /// <param name="outScale">Outer radius</param>
    /// <param name="innerScale">Inner radius</param>
    /// <param name="byTime">Expand over time</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawFan(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name, float radian, float rotation,
        float outScale, float innerScale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, radian, rotation, outScale, outScale, innerScale,
            innerScale, DrawModeEnum.Default, innerScale == 0 ? DrawTypeEnum.Fan : DrawTypeEnum.Donut, isSafe, byTime, false, draw);

    public static DrawPropertiesEdit DrawFan(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name, float radian, float rotation,
        float outScale, float innerScale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawFan(ownerObj, 0, delay, destroy, name, radian, rotation, outScale, innerScale, isSafe, byTime, draw);

    /// <summary>
    /// Return rectangle drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Rectangle start point</param>
    /// <param name="targetObj">Target</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="rotation">Rotation angle</param>
    /// <param name="width">Rectangle width</param>
    /// <param name="length">Rectangle length</param>
    /// <param name="byTime">Expand over time</param>
    /// <param name="byY">Expand based on distance</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawRect(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name, float rotation,
        float width, float length, bool isSafe = false, bool byTime = false, bool byY = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, 0, rotation, width, length, 0, 0,
            DrawModeEnum.Default, DrawTypeEnum.Rect, isSafe, byTime, byY, draw);
    
    public static DrawPropertiesEdit DrawRect(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name, float rotation,
        float width, float length, bool isSafe = false, bool byTime = false, bool byY = false, bool draw = true)
        => sa.DrawRect(ownerObj, 0, delay, destroy, name, rotation, width, length, isSafe, byTime, byY, draw);

    /// <summary>
    /// Return line drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Line start point</param>
    /// <param name="targetObj">Line target point</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="rotation">Rotation angle</param>
    /// <param name="width">Line width</param>
    /// <param name="length">Line length</param>
    /// <param name="byTime">Expand over time</param>
    /// <param name="byY">Expand based on distance</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawLine(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name, float rotation,
        float width, float length, bool isSafe = false, bool byTime = false, bool byY = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, 1, rotation, width, length, 0, 0,
            DrawModeEnum.Default, DrawTypeEnum.Line, isSafe, byTime, byY, draw);

    /// <summary>
    /// Assign distance-based targeting to the given dp, using the owner as reference
    /// </summary>
    /// <param name="self"></param>
    /// <param name="isNearOrder">Order by near or far from owner</param>
    /// <param name="orderIdx">Starting from 1</param>
    /// <returns></returns>
    public static DrawPropertiesEdit SetOwnersDistanceOrder(this DrawPropertiesEdit self, bool isNearOrder,
        uint orderIdx)
    {
        self.CentreResolvePattern = isNearOrder
            ? PositionResolvePatternEnum.PlayerNearestOrder
            : PositionResolvePatternEnum.PlayerFarestOrder;
        self.CentreOrderIndex = orderIdx;
        return self;
    }
}

#endregion Drawing Functions

#region Debug Functions

public static class DebugFunction
{
    public static void DebugMsg(this ScriptAccessory sa, string msg, bool enable = true, bool showInChatBox = false)
    {
        if (!enable) return;
        sa.Log.Debug(msg);
        if (!showInChatBox) return;
        sa.Method.SendChat($"/e {msg}");
    }
}

#endregion Debug Functions

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

    public static unsafe void ScaleModify(this ScriptAccessory sa, IGameObject? obj, float scale, bool vfxScaled = true)
    {
        sa.Method.RunOnMainThreadAsync(Action);
        void Action()
        {
            if (obj == null) return;
            GameObject* charaStruct = (GameObject*)obj.Address;
            if (!obj.IsValid() || !charaStruct->IsReadyToDraw())
            {
                sa.Log.Error($"Provided IGameObject is invalid.");
                return;
            }
            charaStruct->Scale = scale;
            if (vfxScaled)
                charaStruct->VfxScale = scale;

            if (charaStruct->IsCharacter())
                ((BattleChara*)charaStruct)->Character.CharacterData.ModelScale = scale;
        
            charaStruct->DisableDraw();
            charaStruct->EnableDraw();
        
            sa.Log.Debug($"ScaleModify => {obj.Name.TextValue} | {obj} => {scale}");
        }
    }

    public static void SetRotation(this ScriptAccessory sa, IGameObject? obj, float radian, bool show = false)
    {
        if (obj == null || !obj.IsValid())
        {
            sa.Log.Error($"Provided IGameObject is invalid.");
            return;
        }
        unsafe
        {
            GameObject* charaStruct = (GameObject*)obj.Address;
            charaStruct->SetRotation(radian);
        }
        sa.Log.Debug($"Changed facing of {obj.Name.TextValue} | {obj.EntityId} => {radian.RadToDeg()}");
        
        if (!show) return;
        var ownerObj = sa.GetById(obj.EntityId);
        if (ownerObj == null) return;
        var dp = sa.DrawGuidance(ownerObj, 0, 0, 2000, $"Changed facing {obj.Name.TextValue}", radian, draw: false);
        dp.FixRotation = true;
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);
    }
    
    public static unsafe bool IsMoving(this ScriptAccessory sa)
    {
        FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentMap* ptr = FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentMap.Instance();
        return ptr is not null && ptr->IsPlayerMoving;
    }
    
    public static unsafe void WriteVisible(this ScriptAccessory sa, IGameObject? actor, bool visible)
    {
        const VisibilityFlags VISIBLE_FLAG = VisibilityFlags.None;
        const VisibilityFlags INVISIBILITY_FLAG = VisibilityFlags.Model;
        try
        {
            var flagsPtr = &((GameObject*)actor?.Address)->RenderFlags;
            *flagsPtr = visible ? VISIBLE_FLAG : INVISIBILITY_FLAG;
        }
        catch (Exception e)
        {
            sa.Log.Error(e.ToString());
            throw;
        }
    }

}

#endregion Special Functions

#endregion Function Collections
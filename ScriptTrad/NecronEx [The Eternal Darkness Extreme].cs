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
using Lumina.Excel.Sheets;

namespace UsamisKodakku.Scripts._07_DawnTrail.NecronEx;

[ScriptType(name: Name, territorys: [1296], guid: "1829f7d6-9e64-4cf7-9be4-e5d8a2e03d21",
    version: Version, author: "Usami", note: NoteStr, updateInfo: UpdateInfo)]

// ^(?!.*((Monk|Machinist|Dragoon|Samurai|Ninja|Viper|Reaper|Dancer|Bard|Astrologian|Sage|Scholar|(Eos|Selene)|Seraph|White Mage|Warrior|Paladin|Dark Knight|Gunbreaker|Pictomancer|Black Mage|Blue Mage|Summoner|Carbuncle|Demigod Bahamut|Demigod Phoenix|Garuda-Egi|Titan-Egi|Ifrit-Egi|Puppet)|F\] (Used|Cast))).*35501.*$
// ^\[\w+\|[^|]+\|E\]\s\w+

public class NecronEx
{
    private const string
        Name = "NecronEx [The Eternal Darkness Extreme]",
        Version = "0.0.0.7",
        DebugVersion = "a";
    
    const string NoteStr =
        $"""
        {Version}
        Initial release, Duckism.
        """;
    
    const string UpdateInfo =
        $"""
        {Version}
        1. Fixed the issue where the MT/D1, ST/D2 group guidance for the Second and Fourth Azure Wave was reversed.
        """;

    private const bool
        Debugging = false;

    private static readonly
        Vector3 Center = new Vector3(100, 0, 100);
    
    private volatile List<bool> _bools = new bool[20].ToList();      // Recorded flags
    private List<int> _numbers = Enumerable.Repeat(0, 8).ToList();
    private static List<ManualResetEvent> _events = Enumerable
        .Range(0, 20)
        .Select(_ => new ManualResetEvent(false))
        .ToList();
    private static bool _initHint = false;

    private static List<Vector3> _poses = Enumerable.Repeat(new Vector3(0, 0, 0), 8).ToList();
    private static List<uint> _aetherBlightRec = [];
    private static List<ulong> _aetherBlightSourceRec = [];
    private static List<Vector3> _markerPos = 
        [new Vector3(86.75f, 0, 94.28f), new Vector3(113.30f, 0, 94.35f),
         new Vector3(90.31f, 0, 97.01f), new Vector3(109.58f, 0, 97.37f),
         new Vector3(96.13f, 0, 85.74f), new Vector3(103.36f, 0, 85.92f),
         new Vector3(97.66f, 0, 89.95f), new Vector3(101.94f, 0, 89.98f)];

    private static int
        // _castTime_FoD = 0,      // Fear of Death
        // _castTime_CG = 0,       // Cold Grip
        _castTime_MmM = 0;      // Memento Mori

    private static bool
        _judging_FoD = false,
        _judging_CG = false,
        _judging_MmM = false,
        _judging_CoL = false;

    public void Init(ScriptAccessory sa)
    {
        RefreshParams();
        RefreshCastTimeParams();
        // sa.Log.Debug($"Script {Name} v{Version}{DebugVersion} initialized.");
        sa.Method.RemoveDraw(".*");
        _initHint = false;
    }
    
    private void RefreshParams()
    {
        _bools = new bool[20].ToList();
        _numbers = Enumerable.Repeat(0, 20).ToList();
        _events = Enumerable
            .Range(0, 20)
            .Select(_ => new ManualResetEvent(false))
            .ToList();
        
        _poses = Enumerable.Repeat(new Vector3(0, 0, 0), 8).ToList();
        _aetherBlightRec = [];
        _aetherBlightSourceRec = [];
    }
    
    private void RefreshCastTimeParams()
    {
        // Reset ability cast counts
        // _castTime_FoD = 0;
        // _castTime_CG = 0;
        _castTime_MmM = 0;
        
        // Reset in-progress flags
        _judging_FoD = false;
        _judging_CG = false;
        _judging_MmM = false;
        _judging_CoL = false;
    }

    [ScriptMethod(name: "---- Test Items ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void SplitLine_Test(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "Parameter Reset", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void ParameterReset(Event ev, ScriptAccessory sa)
    {
        RefreshParams();
    }
    
    [ScriptMethod(name: "Disappear", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void Disappear(Event ev, ScriptAccessory sa)
    {
        sa.WriteInvisible(sa.Data.MyObject);
    }
    
    [ScriptMethod(name: "Come Back", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void ComeBack(Event ev, ScriptAccessory sa)
    {
        sa.WriteVisible(sa.Data.MyObject);
    }
    
    [ScriptMethod(name: "Initial Hint", eventType: EventTypeEnum.Chat, eventCondition: ["Message:regex:^(The flesh cannot escape death's embrace.\nThis is your limitation.)$"],
        userControl: true)]
    public void InitialHint(Event ev, ScriptAccessory sa)
    {
        if (_initHint) return;
        _initHint = true;
        var myIndex = sa.GetMyIndex(); 
        List<string> role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
        sa.Method.TextInfo(
            $"You are 【{role[myIndex]}】, " +
            $"Please adjust if incorrect.", 5000);
    }
    
    [ScriptMethod(name: "---- Azure Impact ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void AzureImpactSeparator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "Tank Buster Azure Impact", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44592)$"],
        userControl: true)]
    public void AzureImpactDrawing(Event ev, ScriptAccessory sa)
    {
        var dp = sa.DrawFan(ev.SourceId, 5000, 5000, $"AzureImpact", 100f.DegToRad(), 0, 100, 0, draw: false);
        dp.SetOwnersEnmityOrder(1);
        dp.Color = new Vector4(1, 0, 0, 1);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        // sa.Log.Debug($"Drawing AzureImpact");
    }
    
    [ScriptMethod(name: "Azure Impact Determination", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44593)$", "TargetIndex:1"],
        userControl: Debugging)]
    public void AzureImpactDetermination(Event ev, ScriptAccessory sa)
    {
        _numbers[0]++;
        // sa.Log.Debug($"Azure Impact count {_numbers[0]}");
        if (_numbers[0] % 2 != 0) return;
        sa.Method.RemoveDraw($"AzureImpact");
        // sa.Log.Debug($"Deleted Azure Impact drawing, resetting count");
        _numbers[0] = 0;
    }
    
    #region Fear of Death
    
    [ScriptMethod(name: "---- Fear of Death ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void FearOfDeathSeparator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "Fear of Death", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44550)$"],
        userControl: Debugging)]
    public void FearOfDeath(Event ev, ScriptAccessory sa)
    {
        // _castTime_FoD++;
        // sa.Log.Debug($"Casting Fear of Death #{_castTime_FoD}");
        _judging_FoD = true;
        
        _numbers[1] = 0;
        _bools[0] = true;
        // sa.Log.Debug($"Fear of Death #{_castTime_FoD} added event _events[0], until hands and guidance are recorded");
        _events[0].Set();
        
        // switch (_castTime_FoD)
        // {
        //     case 1:
        //     case 2:
        //     case 3:
        //         _numbers[1] = 0;
        //         _bools[0] = true;
        //         // sa.Log.Debug($"Fear of Death #{_castTime_FoD} added event _events[0], until hands and guidance are recorded");
        //         _events[0].Set();
        //         break;
        //     default:
        //         break;
        // }
    }
    
    [ScriptMethod(name: "Fear of Death Hand Positions", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(18700)$"],
        userControl: true)]
    public void FearOfDeathHandPositions(Event ev, ScriptAccessory sa)
    {
        if (!_judging_FoD) return;
        _events[0].WaitOne(10000);
        if (!_bools[0]) return;

        lock (_numbers)
        {
            // Record positions
            _poses[_numbers[1]] = ev.SourcePosition;
            _numbers[1]++;
            // sa.Log.Debug($"Recorded hand #{_numbers[1]} pit position {ev.SourcePosition}");
        }
        
        // After full record, sort
        if (_numbers[1] != 8) return;
        _poses.Sort((a, b) => {
            // Sort left to right ascending, then top to bottom ascending.
            int z = a.X.CompareTo(b.X);
            return z != 0 ? z : a.Z.CompareTo(b.Z);
        });

        // MT H1 D1 D3 D2 D4 ST H2
        List<int> playerIdx = [0, 2, 4, 6, 5, 7, 1, 3];
        
        // Drawings and guidance
        for (int i = 0; i < 8; i++)
        {
            sa.DrawCircle(_poses[i], 0, 20000, $"Pit{i}", 3f);
            if (sa.GetMyIndex() != playerIdx[i]) continue;
            var downSide = i % 2 == 1;
            sa.DrawGuidance(_poses[i] + new Vector3(0, 0, downSide ? 4 : -4), 0, 20000, $"PitGuidance{i}");
            // sa.Log.Debug($"Guidance to pit #{i} (left to right, top to bottom), pit is {(downSide ? "down" : "up")}");
        }

    }
    
    [ScriptMethod(name: "Fear of Death Pit Drawing Delete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44551)$"],
        userControl: Debugging, suppress: 10000)]
    public void FearOfDeathPitDrawingDelete(Event ev, ScriptAccessory sa)
    {
        if (!_judging_FoD) return;
        _events[0].Reset();
        // sa.Log.Debug($"Fear of Death pit determined, drawing deleted, releasing lock");
        sa.Method.RemoveDraw($"Pit.*");
    }
    
    [ScriptMethod(name: "Fear of Death Crush Drawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44551)$"],
        userControl: true, suppress: 10000)]
    public void FearOfDeathCrushDrawing(Event ev, ScriptAccessory sa)
    {
        if (!_judging_FoD) return;
        if (!_bools[0]) return;
        // sa.Log.Debug($"Drawing crush bait");
        List<int> playerIdx = [0, 2, 4, 6, 5, 7, 1, 3];
        for (int i = 0; i < 8; i++)
        {
            var isMyIdx = sa.GetMyIndex() == playerIdx[i];
            var dp = sa.DrawRect(_poses[i], 0, 20000, $"CrushBait{i}",
                0, 6, 24, isMyIdx, draw: false);
            dp.SetPositionDistanceOrder(true, 1);
            dp.Color = isMyIdx ? sa.Data.DefaultSafeColor : new Vector4(0, 0, 0, 2f);
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
    }
    
    [ScriptMethod(name: "Fear of Death Crush Drawing Delete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44552)$"],
        userControl: Debugging, suppress: 10000)]
    public void FearOfDeathCrushDrawingDelete(Event ev, ScriptAccessory sa)
    {
        if (!_judging_FoD) return;
        // sa.Log.Debug($"Fear of Death crush determined, drawing deleted");
        sa.Method.RemoveDraw($"CrushBait.*");
        _judging_FoD = false;
    }
    
    #endregion Fear of Death
    
    #region Cold Grip
    
    [ScriptMethod(name: "---- Cold Grip ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void ColdGripSeparator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "Cold Grip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4455[34])$"],
        userControl: Debugging)]
    public void ColdGrip(Event ev, ScriptAccessory sa)
    {
        // _castTime_CG++;
        // sa.Log.Debug($"Casting Cold Grip #{_castTime_CG}");
        _judging_CG = true;
        
        _bools[1] = true;
        _bools[2] = ev.ActionId == 44553;   // Left safe
        // sa.Log.Debug($"Cold Grip #{_castTime_CG} added event _events[1], until first part drawing is complete");
        _events[1].Set();
        
        // switch (_castTime_CG)
        // {
        //     case 1:
        //     case 2:
        //     case 3:
        //         _bools[1] = true;
        //         _bools[2] = ev.ActionId == 44553;   // Left safe
        //         // sa.Log.Debug($"Cold Grip #{_castTime_CG} added event _events[1], until first part drawing is complete");
        //         _events[1].Set();
        //         break;
        //     default:
        //         break;
        // }
    }
    
    [ScriptMethod(name: "Cold Grip First Part Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4455[34])$"],
        userControl: true)]
    public void ColdGripFirstPartDrawing(Event ev, ScriptAccessory sa)
    {
        _events[1].WaitOne(10000);
        if (!_bools[1]) return;
        if (!_judging_CG) return;

        var isLeftSafe = _bools[2];
        // sa.Log.Debug($"Cold Grip {(isLeftSafe ? "Left" : "Right")} safe, drawing first part");

        sa.DrawRect(new Vector3(88, 0, 85), 0, 6000, $"ColdGripFirstPart", 0, 12, 100);
        sa.DrawRect(new Vector3(112, 0, 85), 0, 6000, $"ColdGripFirstPart", 0, 12, 100);

        // Guide lines
        for (int i = 0; i < 5; i++)
        {
            var dp = sa.DrawLine(new Vector3(isLeftSafe ? 95 : 105, 0, 90 + i * 5),
                new Vector3(isLeftSafe ? 93 : 107, 0, 90 + i * 5),
                0, 20000, $"ColdGripGuideLine", 0, 1f, 2f, true, draw: false);
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);
        }
        
    }
    
    [ScriptMethod(name: "Cold Grip Second Part Drawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44612)$"],
        userControl: true, suppress: 1000)]
    public void ColdGripSecondPartDrawing(Event ev, ScriptAccessory sa)
    {
        if (!_judging_CG) return;
        if (!_bools[1]) return;
        var isLeftSafe = _bools[2];
        // sa.Log.Debug($"Cold Grip {(isLeftSafe ? "Left" : "Right")} safe, first part determined, drawing second part");
        sa.Method.RemoveDraw($"ColdGripFirstPart.*");
        sa.DrawRect(new Vector3(isLeftSafe ? 106 : 94, 0, 85), 0, 6000, $"ColdGripSecondPart", 0, 24, 100);
    }
    
    [ScriptMethod(name: "Cold Grip Drawing Delete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44555)$"],
        userControl: Debugging)]
    public void ColdGripDrawingDelete(Event ev, ScriptAccessory sa)
    {
        if (!_judging_CG) return;
        _events[1].Reset();
        // sa.Log.Debug($"Cold Grip second part determined, drawing deleted, releasing lock _events[1]");
        sa.Method.RemoveDraw($"ColdGrip.*");
        _judging_CG = false;
    }
    
    #endregion Cold Grip
    
    #region Memento Mori
    
    [ScriptMethod(name: "---- Memento Mori ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void MementoMoriSeparator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "Memento Mori", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4456[56])$"],
        userControl: Debugging)]
    public void MementoMori(Event ev, ScriptAccessory sa)
    {
        _castTime_MmM++;
        // sa.Log.Debug($"Casting Memento Mori #{_castTime_MmM}");
        _judging_MmM = true;
        
        _bools[2] = true;
        _numbers[2] = 0;    // Safe row
        _numbers[6] = 0;    // Hand count
        // sa.Log.Debug($"Memento Mori #{_castTime_MmM} added event _events[2], until safe row is determined");
        _events[2].Set();
    }

    [ScriptMethod(name: "Memento Mori Line Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4456[56])$"],
        userControl: true)]
    public void MementoMoriLineDrawing(Event ev, ScriptAccessory sa)
    {
        _events[2].WaitOne(10000);
        if (!_bools[2]) return;
        if (!_judging_MmM) return;
        sa.DrawRect(ev.SourceId, 0, 20000, $"MementoMoriCenterLine", 0, 12, 100);
        _bools[3] = ev.ActionId == 44565;   // 44565 left has fewer
        var isLeftLess = _bools[3];
        // sa.Log.Debug($"Memento Mori {(isLeftLess ? "Left" : "Right")} has fewer, drawing.");
    }
    
    [ScriptMethod(name: "Memento Mori Hand Drawing", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(18700)$"],
        userControl: true)]
    public void MementoMoriHandDrawing(Event ev, ScriptAccessory sa)
    {
        if (!_judging_MmM) return;
        _events[2].WaitOne(10000);
        if (!_bools[2]) return;

        lock (_numbers)
        {
            _numbers[6]++;
            sa.DrawRect(ev.SourceId, 0, 20000, $"MementoMoriHand", 0, 6, 24);
            var isLeftLess = _bools[3];
            // Left has fewer, find facing left, rotation 270 (-90); right has fewer, find facing right, rotation 90
            var rotation = ev.SourceRotation.RadToDeg();
            // sa.Log.Debug($"Found rotation {rotation} for hand #{_numbers[6]} {ev.SourcePosition} ");

            if ((isLeftLess && rotation > 180f) || (!isLeftLess && rotation < 180f))
            {
                // Z axis values 88 94 100 106 112, safe row can be obtained via (pos.z - 87) / 6
                _numbers[2] = (int)Math.Floor((ev.SourcePosition.Z - 87) / 6);
                // sa.Log.Debug($"Memento Mori {_castTime_MmM} obtained safe row {_numbers[2]}");
            }

            if (_numbers[6] != 5) return;
            // sa.Log.Debug($"Memento Mori {_castTime_MmM} finished recording hands, releasing lock _events[2]");
            _numbers[6] = 0;
            _events[2].Reset();
            
            // sa.Log.Debug($"Memento Mori {_castTime_MmM} added event _events[3], until drawing and guidance are complete");
            _events[3].Set();
        }
    }

    [ScriptMethod(name: "Memento Mori Guidance (Please ensure previous item is enabled)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4456[56])$"],
        userControl: true)]
    public void MementoMoriGuidance(Event ev, ScriptAccessory sa)
    {
        _events[3].WaitOne(10000);
        if (!_bools[2])
        {
            _events[3].Reset();
            return;
        }
        if (!_judging_MmM) return;
        
        var safeRow = _numbers[2];
        var isLeftLess = _bools[3];
        var reverseBias = isLeftLess ? 0 : 1;
        
        // If second Memento Mori, ignore
        if (_castTime_MmM == 2)
        {
            // sa.Log.Debug($"Memento Mori {_castTime_MmM} ignoring, releasing lock _events[3]");
            _events[3].Reset();
            return;
        }
        
        // The safe row for Memento Mori cannot be the first or last row, so the following guidance rules apply:
        // D1, D2 always in the first row; D3, D4 always in the last row; MT, ST always in the safe row offset up; H1, H2 always in the safe row offset down
        // Calculated based on left having fewer, shift bias if opposite
        var safePos = sa.GetMyIndex() switch
        {
            4 => new Vector3(82.5f, 0, 85.5f) + new Vector3(24f * reverseBias, 0, 0),
            5 => new Vector3(93.5f, 0, 85.5f) + new Vector3(24f * reverseBias, 0, 0),
            6 => new Vector3(82.5f, 0, 114.5f) + new Vector3(24f * reverseBias, 0, 0),
            7 => new Vector3(93.5f, 0, 114.5f) + new Vector3(24f * reverseBias, 0, 0),
            0 => new Vector3(106.5f, 0, 85.5f + safeRow * 6) + new Vector3(-24f * reverseBias, 0, 0),
            1 => new Vector3(117.5f, 0, 85.5f + safeRow * 6) + new Vector3(-24f * reverseBias, 0, 0),
            2 => new Vector3(82.5f, 0, 85.5f + (safeRow + 1) * 6) + new Vector3(24f * reverseBias, 0, 0) +
                 new Vector3(0, 0, safeRow >= 3 ? -7 : 0),
            3 => new Vector3(93.5f, 0, 85.5f + (safeRow + 1) * 6) + new Vector3(24f * reverseBias, 0, 0) +
                 new Vector3(0, 0, safeRow >= 3 ? -7 : 0),
            _ => new Vector3(100f, 0, 100f),
        };
        sa.DrawGuidance(safePos, 0, 20000, $"MementoMoriGuidance");
        // sa.Log.Debug($"Memento Mori {_castTime_MmM} guidance complete, releasing lock _events[3]");
        
        _events[3].Reset();
    }
    
    [ScriptMethod(name: "Memento Mori Drawing Delete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44567)$"],
        userControl: Debugging, suppress: 1000)]
    public void MementoMoriDrawingDelete(Event ev, ScriptAccessory sa)
    {
        if (!_judging_MmM) return;
        // sa.Log.Debug($"Memento Mori {_castTime_MmM} hand crush determined, drawing deleted");
        sa.Method.RemoveDraw($"MementoMori.*");
        _judging_MmM = false;
    }
    
    #endregion Memento Mori

    #region Aether Blight

    [ScriptMethod(name: "---- Aether Blight ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void AetherBlightSeparator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "Aether Blight Marker Record", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(025[CDEF])$"],
        userControl: Debugging)]
    public void AetherBlightMarkerRecord(Event ev, ScriptAccessory sa)
    {
        var id = ev.Id0();
        _aetherBlightRec.Add(id);
        // sa.Log.Debug($"Aether Blight {id} recorded (#{_aetherBlightRec.Count}), 604 Chariot, 605 Donut, 606 Hit sides, 607 Hit center");
    }
    
    [ScriptMethod(name: "Azure Wave Tether Record", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(015[BCDE])$"],
        userControl: Debugging)]
    public void AzureWaveTetherRecord(Event ev, ScriptAccessory sa)
    {
        _aetherBlightSourceRec.Add(ev.SourceId);
        // sa.Log.Debug($"Detected Azure Wave tether #{_aetherBlightSourceRec.Count}, tether {ev.SourceId}, at {ev.SourcePosition}");
    }
    
    [ScriptMethod(name: "Azure Multiple Wave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4455[78]|4516[78])$"],
        userControl: Debugging)]
    public void AzureMultipleWave(Event ev, ScriptAccessory sa)
    {
        // 44557, 45167 Double Wave, 44 split stack
        // 44558, 45168 Quad Wave, 22 split stack
        // Initial sequence, default 0, used for Season transpose
        
        _numbers[4] = ev.ActionId is 44557 or 44558 ? 1 : 4;    // Total drawing count
        _bools[8] = ev.ActionId is 45167 or 45168;  // Is Azure Wave
        _bools[5] = ev.ActionId is 44558 or 45168;  // Is 22 split stack

        _bools[4] = true;
        // sa.Log.Debug($"Azure Multiple{(_bools[8] ? "Wave" : "Wave")} prepare drawing, total drawings {_numbers[4]}");
        _events[4].Set();
        // sa.Log.Debug($"Azure Multiple{(_bools[8] ? "Wave" : "Wave")} added event _events[4]");
    }
    
    [ScriptMethod(name: "Azure Multiple Wave Stack Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4455[78])$"],
        userControl: true)]
    public void AzureMultipleWaveStackDrawing(Event ev, ScriptAccessory sa)
    {
        _events[4].WaitOne(10000);
        if (!_bools[4]) return;
        var isPartnerStack = _bools[5];
        List<int> partnerJudge = [0, 11, 2, 13, 0, 11, 2, 13];
        
        // Azure Multiple Wave, draw stack for targets (choose TH here)
        var myPartIdx = partnerJudge[sa.GetMyIndex()];
        sa.DrawFan(ev.SourceId, sa.Data.PartyList[2], 0, 20000, $"AzureMultipleWaveStack H1",
            (isPartnerStack ? 20f : 25f).DegToRad(), 0, 100, 0, isPartnerStack ? myPartIdx == partnerJudge[2] : myPartIdx < 10);
        sa.DrawFan(ev.SourceId, sa.Data.PartyList[3], 0, 20000, $"AzureMultipleWaveStack H2",
            (isPartnerStack ? 20f : 25f).DegToRad(), 0, 100, 0, isPartnerStack ? myPartIdx == partnerJudge[3] : myPartIdx > 10);

        if (isPartnerStack)
        {
            sa.DrawFan(ev.SourceId, sa.Data.PartyList[0], 0, 20000, $"AzureMultipleWaveStack MT",
                (isPartnerStack ? 20f : 25f).DegToRad(), 0, 100, 0, myPartIdx == partnerJudge[0]);
            sa.DrawFan(ev.SourceId, sa.Data.PartyList[1], 0, 20000, $"AzureMultipleWaveStack ST",
                (isPartnerStack ? 20f : 25f).DegToRad(), 0, 100, 0, myPartIdx == partnerJudge[1]);
        }
        // sa.Log.Debug($"Azure {(isPartnerStack ? "Quad" : "Double")} Wave stack range drawing complete, releasing lock");
    }
    
    [ScriptMethod(name: "Azure Multiple Wave Arena Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4455[78])$"],
        userControl: true)]
    public void AzureMultipleWaveArenaDrawing(Event ev, ScriptAccessory sa)
    {
        _events[4].WaitOne(10000);
        if (!_bools[4]) return;

        var drawTotalCount = _numbers[4];
        if (drawTotalCount != _aetherBlightRec.Count)
        {
            sa.Log.Error($"Total drawing count {drawTotalCount} does not match recorded list Count {_aetherBlightRec.Count}, stopping drawing.");
            return;
        }
        
        // 604 Chariot, 605 Donut, 606 Hit sides, 607 Hit center
        switch (_aetherBlightRec[0])
        {
            case 606:
                sa.DrawRect(new Vector3(88, 0, 85), 0, 5500, $"AzureMultipleWaveHitSides", 0, 12, 100);
                sa.DrawRect(new Vector3(112, 0, 85), 0, 5500, $"AzureMultipleWaveHitSides", 0, 12, 100);
                // sa.Log.Debug($"AzureMultipleWave Hit Sides drawing complete");
                break;
            
            case 607:
                sa.DrawRect(new Vector3(100, 0, 78), 0, 5500, $"AzureMultipleWaveHitCenter", 0, 12, 100);
                // sa.Log.Debug($"AzureMultipleWave Hit Center drawing complete");
                break;
            
            case 604:
                sa.DrawCircle(new Vector3(100, 0, 78), 0, 5500, $"AzureMultipleWaveChariot", 20);
                // sa.Log.Debug($"AzureMultipleWave Chariot drawing complete");
                break;
            
            case 605:
                sa.DrawDonut(new Vector3(100, 0, 78), 0, 5500, $"AzureMultipleWaveDonut", 60, 16);
                // sa.Log.Debug($"AzureMultipleWave Donut drawing complete");
                break;
        }
        
        // sa.Log.Debug($"AzureMultipleWave drawing complete");
    }
    
    [ScriptMethod(name: "Azure Multiple Wave Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4455[78])$"],
        userControl: true)]
    public void AzureMultipleWaveGuidance(Event ev, ScriptAccessory sa)
    {
        _events[4].WaitOne(10000);
        if (!_bools[4]) return;
        
        var isPartnerStack = _bools[5];
        
        // Azure Multiple Wave, guidance
        var myIndex = sa.GetMyIndex();
        var isInsideSafe = _aetherBlightRec[0] is 605 or 606;

        var myPosIdx = myIndex % 4 + (isInsideSafe ? 4 : 0);
        if (!isPartnerStack && myPosIdx % 4 <= 2) myPosIdx += 2;
        sa.Log.Debug($"Is group stack: {isPartnerStack}, is inside safe: {isInsideSafe}, my index: {myPosIdx}");
        sa.DrawGuidance(_markerPos[myPosIdx], 0, 20000, $"AzureMultipleWaveGuidance");
        
        // sa.Log.Debug($"Azure {(isPartnerStack ? "Quad" : "Double")} guidance {_markerPos[myPosIdx]} drawing complete, releasing lock");
    }
    
    [ScriptMethod(name: "Azure Multiple Wave Drawing Delete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4455[78])$"],
        userControl: Debugging)]
    public void AzureMultipleWaveDrawingDelete(Event ev, ScriptAccessory sa)
    {
        _aetherBlightRec = [];
        _aetherBlightSourceRec = [];
        _events[4].Reset();
        // sa.Log.Debug($"AzureMultipleWave determined, clearing matrices, drawing deleted, releasing lock _events[4]");
        sa.Method.RemoveDraw($"AzureMultipleWave.*");
    }
    
    [ScriptMethod(name: "Azure Multiple Wave Determination", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4516[78])$"],
        userControl: Debugging)]
    public void AzureMultipleWaveDetermination(Event ev, ScriptAccessory sa)
    {
        _events[4].WaitOne(10000);
        if (!_bools[4]) return;
        int startIdx = 0;
        float lowestY = 30f;
        // Get startIdx
        for (int i = 0; i < _aetherBlightSourceRec.Count; i++)
        {
            // Find the unit with the lowest Y
            var obj = sa.GetById(_aetherBlightSourceRec[i]);
            if (obj is null) continue;
            if (obj.Position.Y >= lowestY) continue;
            lowestY = obj.Position.Y;
            startIdx = i;
        }
        _numbers[3] = startIdx;
        _numbers[5] = 0;
        // sa.Log.Debug($"After traversal, lowest unit index is {startIdx}, id is {_aetherBlightSourceRec[startIdx]}, height is {lowestY}");
        _events[5].Set();
    }
    
    [ScriptMethod(name: "Azure Multiple Wave Arena Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4516[78])$"],
        userControl: true)]
    public void AzureMultipleWaveArenaDrawing2(Event ev, ScriptAccessory sa)
    {
        _events[5].WaitOne(10000);
        if (!_bools[4]) return;
        _bools[6] = true;
        
        var drawTotalCount = _numbers[4];
        if (drawTotalCount != _aetherBlightRec.Count)
        {
            sa.Log.Error($"Total drawing count {drawTotalCount} does not match recorded list Count {_aetherBlightRec.Count}, stopping drawing.");
            _bools[6] = false;
            return;
        }
        
        var startIdx = _numbers[3];
        // 604 Chariot, 605 Donut, 606 Hit sides, 607 Hit center
        switch (_aetherBlightRec[startIdx])
        {
            case 606:
                sa.DrawRect(new Vector3(88, 0, 85), 0, 15000, $"AzureWave0 HitSides", 0, 12, 100);
                sa.DrawRect(new Vector3(112, 0, 85), 0, 15000, $"AzureWave0 HitSides", 0, 12, 100);
                // sa.Log.Debug($"AzureWave0 Hit Sides drawing complete");
                break;
            
            case 607:
                sa.DrawRect(new Vector3(100, 0, 78), 0, 15000, $"AzureWave0 HitCenter", 0, 12, 100);
                // sa.Log.Debug($"AzureWave0 Hit Center drawing complete");
                break;
            
            case 604:
                sa.DrawCircle(new Vector3(100, 0, 78), 0, 15000, $"AzureWave0 Chariot", 20);
                // sa.Log.Debug($"AzureWave0 Chariot drawing complete");
                break;
            
            case 605:
                sa.DrawDonut(new Vector3(100, 0, 78), 0, 15000, $"AzureWave0 Donut", 60, 16);
                // sa.Log.Debug($"AzureWave0 Donut drawing complete");
                break;
        }
    }

    [ScriptMethod(name: "Azure Multiple Wave Counter", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4518[345]|44608)$", "TargetIndex:1"],
        userControl: Debugging, suppress: 1000)]
    public void AzureMultipleWaveCounter(Event ev, ScriptAccessory sa)
    {
        _events[4].WaitOne(10000);
        if (!_bools[8]) return;
        _numbers[5]++;
        // sa.Log.Debug($"Azure Wave counter increased to #{_numbers[5]}, adding lock _events[6]");
        _events[6].Set();
    }
    
    [ScriptMethod(name: "Azure Multiple Wave Counter Unlock", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4518[345]|44608)$", "TargetIndex:1"],
        userControl: Debugging, suppress: 1000)]
    public async void AzureMultipleWaveCounterUnlock(Event ev, ScriptAccessory sa)
    {
        _events[4].WaitOne(10000);
        if (!_bools[8]) return;
        _events[6].WaitOne(500);
        
        await Task.Delay(500);
        // sa.Log.Debug($"Azure Multiple Wave counter _events[6] unlocked");
        _events[6].Reset();
    }

    [ScriptMethod(name: "Azure Multiple Wave Subsequent Arena Drawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4518[345]|44608)$", "TargetIndex:1"],
        userControl: Debugging, suppress: 1000)]
    public void AzureMultipleWaveSubsequentArenaDrawing(Event ev, ScriptAccessory sa)
    {
        if (!_bools[6]) return;
        _events[6].WaitOne(10000);
        var drawCount = _numbers[5];
        var drawTotalCount = _numbers[4];
        if (drawTotalCount != _aetherBlightRec.Count)
        {
            sa.Log.Error($"Total drawing count {drawTotalCount} does not match recorded list Count {_aetherBlightRec.Count}, stopping drawing.");
            return;
        }
        // sa.Log.Debug($"Deleting AzureWave{drawCount-1} drawing");
        sa.Method.RemoveDraw($"AzureWave{drawCount-1}.*");
        if (drawCount >= drawTotalCount) return;
        
        var startIdx = _numbers[3];
        var drawIdx = (startIdx + drawCount) % drawTotalCount;
        
        // 604 Chariot, 605 Donut, 606 Hit sides, 607 Hit center
        switch (_aetherBlightRec[drawIdx])
        {
            case 606:
                sa.DrawRect(new Vector3(88, 0, 85), 0, 15000, $"AzureWave{drawCount} HitSides", 0, 12, 100);
                sa.DrawRect(new Vector3(112, 0, 85), 0, 15000, $"AzureWave{drawCount} HitSides", 0, 12, 100);
                // sa.Log.Debug($"AzureWave{drawCount} Hit Sides drawing complete");
                break;
            
            case 607:
                sa.DrawRect(new Vector3(100, 0, 78), 0, 15000, $"AzureWave{drawCount} HitCenter", 0, 12, 100);
                // sa.Log.Debug($"AzureWave{drawCount} Hit Center drawing complete");
                break;
            
            case 604:
                sa.DrawCircle(new Vector3(100, 0, 78), 0, 15000, $"AzureWave{drawCount} Chariot", 20);
                // sa.Log.Debug($"AzureWave{drawCount} Chariot drawing complete");
                break;
            
            case 605:
                sa.DrawDonut(new Vector3(100, 0, 78), 0, 15000, $"AzureWave{drawCount} Donut", 60, 16);
                // sa.Log.Debug($"AzureWave{drawCount} Donut drawing complete");
                break;
        }
    }
    
    [ScriptMethod(name: "Azure Multiple Wave Stack Drawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4518[345]|44608)$", "TargetIndex:1"],
        userControl: true, suppress: 1000)]
    public void AzureMultipleWaveStackDrawing2(Event ev, ScriptAccessory sa)
    {
        _events[6].WaitOne(10000);
        var drawCount = _numbers[5];
        if (drawCount != 3) return;
        var isPartnerStack = _bools[5];
        List<int> partnerJudge = [0, 11, 2, 13, 0, 11, 2, 13];
        
        // Azure Multiple Wave, draw stack for targets (choose TH here)
        var myPartIdx = partnerJudge[sa.GetMyIndex()];
        sa.DrawFan(new Vector3(100, 0, 78), sa.Data.PartyList[2], 0, 20000, $"AzureWaveStack H1",
            (isPartnerStack ? 20f : 30f).DegToRad(), 0, 100, 0, isPartnerStack ? myPartIdx == partnerJudge[2] : myPartIdx < 10);
        sa.DrawFan(new Vector3(100, 0, 78), sa.Data.PartyList[3], 0, 20000, $"AzureWaveStack H2",
            (isPartnerStack ? 20f : 30f).DegToRad(), 0, 100, 0, isPartnerStack ? myPartIdx == partnerJudge[3] : myPartIdx > 10);

        if (isPartnerStack)
        {
            sa.DrawFan(new Vector3(100, 0, 78), sa.Data.PartyList[0], 0, 20000, $"AzureWaveStack MT",
                (isPartnerStack ? 20f : 30f).DegToRad(), 0, 100, 0, myPartIdx == partnerJudge[0]);
            sa.DrawFan(new Vector3(100, 0, 78), sa.Data.PartyList[1], 0, 20000, $"AzureWaveStack ST",
                (isPartnerStack ? 20f : 30f).DegToRad(), 0, 100, 0, myPartIdx == partnerJudge[1]);
        }
        // sa.Log.Debug($"Azure {(isPartnerStack ? "Quad" : "Double")} Wave stack range drawing complete");
    }
    
    [ScriptMethod(name: "Azure Multiple Wave Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4516[78])$"],
        userControl: true)]
    public void AzureMultipleWaveGuidance2(Event ev, ScriptAccessory sa)
    {
        _bools[7] = true;
        _events[5].WaitOne(10000);
        var isPartnerStack = _bools[5];
        
        // Azure Multiple Wave, guidance
        var myIndex = sa.GetMyIndex();
        var startIdx = _numbers[3];
        
        var drawCount = _numbers[5];
        var drawTotalCount = _numbers[4];
        var drawIdx = (startIdx + drawCount) % drawTotalCount;
       
        var isInsideSafe = _aetherBlightRec[drawIdx] is 605 or 606;

        // var myPosIdx = myIndex % 4 + (isPartnerStack ? 0 : 2) + (isInsideSafe ? 4 : 0);
        var myPosIdx = myIndex % 4 + (isInsideSafe ? 4 : 0);
        if (!isPartnerStack && myPosIdx % 4 <= 2) myPosIdx += 2;
        sa.DrawGuidance(_markerPos[myPosIdx], 0, 20000, $"AzureWaveGuidance #{drawCount}");

        var str = _aetherBlightRec[drawIdx] switch
        {
            604 => "Chariot",
            605 => "Donut",
            606 => "Hit sides",
            607 => "Hit center",
            _ => "Unknown",
        };
        
        // sa.Log.Debug($"Azure {(isPartnerStack ? "Quad" : "Double")} Wave {str}({_aetherBlightRec[drawIdx]}) guidance #{drawCount} {_markerPos[myPosIdx]} drawing complete");
    }
    
    [ScriptMethod(name: "Azure Multiple Wave Subsequent Guidance", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4518[345]|44608)$", "TargetIndex:1"],
        userControl: Debugging, suppress: 1000)]
    public void AzureMultipleWaveSubsequentGuidance(Event ev, ScriptAccessory sa)
    {
        if (!_bools[7]) return;
        _events[6].WaitOne(10000);
        
        var isPartnerStack = _bools[5];
        
        // Azure Multiple Wave, guidance
        var myIndex = sa.GetMyIndex();
        var startIdx = _numbers[3];
        
        var drawCount = _numbers[5];
        var drawTotalCount = _numbers[4];
        var drawIdx = (startIdx + drawCount) % drawTotalCount;
        
        if (drawCount >= drawTotalCount) return;
        var isInsideSafe = _aetherBlightRec[drawIdx] is 605 or 606;
        
        // sa.Log.Debug($"Deleting AzureWave{drawCount-1} guidance");
        sa.Method.RemoveDraw($"AzureWaveGuidance #{drawCount-1}.*");

        var myPosIdx = myIndex % 4 + (isPartnerStack ? 0 : 2) + (isInsideSafe ? 4 : 0);
        sa.DrawGuidance(_markerPos[myPosIdx], 0, 20000, $"AzureWaveGuidance #{drawCount}");
        
        var str = _aetherBlightRec[drawIdx] switch
        {
            604 => "Chariot",
            605 => "Donut",
            606 => "Hit sides",
            607 => "Hit center"
        };
        
        // sa.Log.Debug($"Azure {(isPartnerStack ? "Quad" : "Double")} Wave {str}({_aetherBlightRec[drawIdx]}) guidance #{drawCount} {_markerPos[myPosIdx]} drawing complete");
    }
    
    [ScriptMethod(name: "Azure Multiple Wave Drawing Delete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4518[345]|44608)$", "TargetIndex:1"],
        userControl: Debugging, suppress: 1000)]
    public async void AzureMultipleWaveDrawingDelete2(Event ev, ScriptAccessory sa)
    {
        _events[6].WaitOne(10000);
        if (!_bools[8]) return;
        await Task.Delay(800);
        
        var drawCount = _numbers[5];
        if (drawCount != 4) return;
        // _aetherBlightRec = [];
        // _aetherBlightSourceRec = [];
        // _events[4].Reset();
        // // sa.Log.Debug($"Azure Multiple Wave determined, clearing matrices, drawing deleted, releasing lock _events[4]");
        RefreshParams();
        // sa.Log.Debug($"Azure Multiple Wave determined, refreshing parameters");
        sa.Method.RemoveDraw($"AzureWave.*");
    }
    
    #endregion Aether Blight

    #region Grand Cross

    [ScriptMethod(name: "---- Grand Cross ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void GrandCrossSeparator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "Grand Cross Refresh Parameters", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44568)$"],
        userControl: Debugging)]
    public void GrandCrossRefreshParams(Event ev, ScriptAccessory sa)
    {
        // sa.Log.Debug($"Casting Grand Cross, refreshing parameters");
        RefreshParams();
    }
    
    [ScriptMethod(name: "Grand Cross Gather Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44568)$"],
        userControl: true)]
    public void GrandCrossGatherHint(Event ev, ScriptAccessory sa)
    {
        sa.Method.TextInfo($"Gather center, two rounds of bait", 5000, true);
    }
    
    [ScriptMethod(name: "Grand Cross Attenuation Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44570)$"],
        userControl: true)]
    public void GrandCrossAttenuationHint(Event ev, ScriptAccessory sa)
    {
        sa.Method.TextInfo($"Gather center, two rounds of bait, avoid corners", 5000, true);
    }
    
    [ScriptMethod(name: "Grand Cross Laser", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(015[78])$"],
        userControl: true)]
    public void GrandCrossLaser(Event ev, ScriptAccessory sa)
    {
        var obj1 = sa.GetById(ev.SourceId);
        var obj2 = sa.GetById(ev.TargetId);
        
        sa.WriteInvisible(obj1);
        sa.WriteInvisible(obj2);
        
        if (obj1 is null || obj2 is null) return;

        var isFast = obj1.DataId == 18761;
        var startPos = obj1.Position.RotateAndExtend(Center, isFast ? 41f.DegToRad() : -153f.DegToRad(), 0);
        var targetPos = obj2.Position.RotateAndExtend(Center, isFast ? 41f.DegToRad() : -153f.DegToRad(), 0);
        
        var dp = sa.DrawRect(startPos, targetPos, 0, 15000,
            $"GrandCrossLaser {ev.SourceId}", 0, 5, 40, draw: false);
        dp.Color = new Vector4(1, 0, 0, 1);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        // sa.Log.Debug($"Predicted Grand Cross Laser {(isFast ? "Fast" : "Slow")} {ev.SourceId}, {startPos} to {targetPos}, drawing");
    }
    
    [ScriptMethod(name: "Grand Cross Laser Disappear", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(4541)$"],
        userControl: Debugging)]
    public void GrandCrossLaserDisappear(Event ev, ScriptAccessory sa)
    {
        // sa.Log.Debug($"Soul piece {ev.TargetId} starts rotating, Grand Cross Laser disappearing");
        sa.Method.RemoveDraw($"GrandCrossLaser {ev.TargetId}");
    }
    
    [ScriptMethod(name: "Grand Cross Mark Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44572)$"],
        userControl: Debugging)]
    public void GrandCrossMarkRecord(Event ev, ScriptAccessory sa)
    {
        lock (_numbers)
        {
            _numbers[8]++;
            _bools[9] |= ev.TargetId == sa.Data.Me;
            if (_numbers[8] < 4) return;
            _events[7].Set();
            // sa.Log.Debug($"Recorded marks, player {(_bools[9] ? "will not stand in tower" : "will stand in tower")}");
        }
    }

    [ScriptMethod(name: "Grand Cross Tower Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44573)$"],
        userControl: true, suppress: 5000)]
    public void GrandCrossTowerGuidance(Event ev, ScriptAccessory sa)
    {
        _events[7].WaitOne(10000);

        var pos = ev.SourcePosition.GetRadian(Center).RadianToRegion(8, 0, true, false);
        var isDiagTower = pos % 2 == 1;
        // sa.Log.Debug($"Detected tower direction {pos}, {(isDiagTower ? "is" : "is not")} a diagonal tower");

        List<int> rot = [2, 1, 3, 0];
        var myIndex = sa.GetMyIndex() % 4;
        
        var tower = !_bools[9];
        if (tower)
        {
            sa.DrawGuidance(new Vector3(100, 0, 105.5f).RotateAndExtend(Center, (rot[myIndex] * 90f + (isDiagTower ? 45f : 0f)).DegToRad(), 0),
                0, 5000, $"GrandCrossTowerSpread");
            // sa.Log.Debug($"Tower guidance complete");
        }
        else
        {
            sa.DrawGuidance(new Vector3(100, 0, 105.5f).RotateAndExtend(Center, (rot[myIndex] * 90f + (isDiagTower ? 0f : 45f)).DegToRad(), 0),
                0, 5000, $"GrandCrossTowerSpread");
            // sa.Log.Debug($"Spread guidance complete");
        }

    }
    
    [ScriptMethod(name: "Grand Cross Tower Guidance Delete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44573)$"],
        userControl: Debugging, suppress: 5000)]
    public void GrandCrossTowerGuidanceDelete(Event ev, ScriptAccessory sa)
    {
        _bools[9] = false;
        _numbers[8] = 0;
        _events[7].Reset();
        // sa.Log.Debug($"Tower determined, deleting Grand Cross tower guidance");
        sa.Method.RemoveDraw($"GrandCrossTowerSpread");
    }
    
    #endregion Grand Cross

    #region Mobs

    [ScriptMethod(name: "---- Mobs ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void MobsSeparator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "Mobs Crush", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4457[89]|44584)$"],
        userControl: true)]
    public void MobsCrush(Event ev, ScriptAccessory sa)
    {
        var dp = sa.DrawRect(ev.SourceId, ev.ActionId is 44579 or 44584 ? 0 : ev.TargetId, 0, 3000,
            $"Mobs{ev.SourceId}Crush", 0, 6, 24, draw: false);
        dp.Color = ev.ActionId is 44579 or 44584 ? sa.Data.DefaultDangerColor : new Vector4(1, 0, 0, 1);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "Mobs Crush Drawing Delete 1", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4457[89]|44584)$"],
        userControl: Debugging)]
    public void MobsCrushDrawingDelete1(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Mobs{ev.SourceId}Crush");
    }
    
    [ScriptMethod(name: "Mobs Crush Drawing Delete 2", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:regex:^(4457[89]|44584)$"],
        userControl: Debugging)]
    public void MobsCrushDrawingDelete2(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Mobs{ev.SourceId}Crush");
    }
   
    [ScriptMethod(name: "Transition Refresh Parameters", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44580)$"],
        userControl: Debugging)]
    public void TransitionRefreshParams(Event ev, ScriptAccessory sa)
    {
        // sa.Log.Debug($"Casting transition The Eternal Darkness, refreshing parameters");
        RefreshParams();
    }

    #endregion Mobs

    #region The End's Embrace

    [ScriptMethod(name: "---- The End's Embrace (Ring) ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void TheEndsEmbraceSeparator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "Edge Dark Arm Drag In", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44591)$"],
        userControl: true)]
    public void EdgeDarkArmDragIn(Event ev, ScriptAccessory sa)
    {
        sa.DrawRect(ev.SourceId, 0, 5000, $"EdgeDarkArm{ev.SourceId}DragIn", 0, 10, 36);
    }
    
    [ScriptMethod(name: "Edge Dark Arm Danger Zone Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44591)$"],
        userControl: Debugging)]
    public void EdgeDarkArmDangerZoneRecord(Event ev, ScriptAccessory sa)
    {
        lock (_numbers)
        {
            var rowIdx = (int)Math.Floor((ev.SourcePosition.Z - 79) / 10);
            _numbers[10] += (int)Math.Pow(10, rowIdx - 1);
            // sa.Log.Debug($"Edge Dark Arm row {rowIdx} dangerous (1-based), count value {_numbers[10]}");
        }
    }
    
    [ScriptMethod(name: "Edge Dark Arm Drag In Delete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44591)$"],
        userControl: Debugging)]
    public void EdgeDarkArmDragInDelete(Event ev, ScriptAccessory sa)
    {
        lock (_numbers)
        {
            var rowIdx = (int)Math.Floor((ev.SourcePosition.Z - 79) / 10);
            _numbers[10] -= (int)Math.Pow(10, rowIdx - 1);
            // sa.Log.Debug($"Edge Dark Arm row {rowIdx} determined, drawing deleted, count value {_numbers[10]}");
            sa.Method.RemoveDraw($"EdgeDarkArm{ev.SourceId}DragIn");
        }
    }
    
    [ScriptMethod(name: "The End's Embrace Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44597)$"],
        userControl: true)]
    public void TheEndsEmbraceGuidance(Event ev, ScriptAccessory sa)
    {
        // Find safe zone
        if (_numbers[10] == 0) return;  // No arms, anywhere safe
        int safeRow = 0;
        for (int i = 0; i < 3; i++)
        {
            if (_numbers[10].GetDecimalDigit(i + 1) != 0) continue;
            safeRow = i + 1;
            // sa.Log.Debug($"Found safe row for Dark Arm {safeRow}");
        }
        
        var baitPos = new Vector3(88f, 0, 87f);
        var myIndex = sa.GetMyIndex();
        
        if (myIndex is 2 or 3 or 6 or 7)
            baitPos += new Vector3(0, 0, 6);
        if (myIndex is 4 or 5 or 6 or 7)
            baitPos += new Vector3(6, 0, 0);
        if (myIndex is 1 or 3 or 5 or 7)
            baitPos = baitPos.FoldPointHorizon(Center.X);

        baitPos += new Vector3(0, 0, (safeRow - 1) * 10);
        sa.DrawGuidance(baitPos, 0, 4000, $"TheEndsEmbraceGuidance");
        sa.DrawGuidance(baitPos, new Vector3(100, 0, 80 + safeRow * 10),
            0, 4000, $"TheEndsEmbraceSubsequentGuidancePrepare", isSafe: false);
        sa.DrawGuidance(new Vector3(100, 0, 80 + safeRow * 10),
            4000, 4000, $"TheEndsEmbraceSubsequentGuidance", isSafe: true);
    }

    [ScriptMethod(name: "The End's Embrace Guidance Delete", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44567)$"],
        userControl: Debugging, suppress: 2000)]
    public void TheEndsEmbraceGuidanceDelete(Event ev, ScriptAccessory sa)
    {
        // sa.Log.Debug($"Dark Hand casting crush, deleting The End's Embrace guidance");
        sa.Method.RemoveDraw($"TheEndsEmbrace.*");
    }
    
    #endregion The End's Embrace

    #region Circle of Lives

    [ScriptMethod(name: "---- Circle of Lives ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void CircleOfLivesSeparator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "Circle of Lives Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44600)$"],
        userControl: true)]
    public void CircleOfLivesDrawing(Event ev, ScriptAccessory sa)
    {
        sa.DrawDonut(ev.SourceId, _judging_CoL ? 2500 : 0, _judging_CoL ? 4500 : 7000,
            $"CircleOfLives{ev.SourceId}", 50, 3);
        _judging_CoL = true;
    }
    
    [ScriptMethod(name: "Circle of Lives Drawing Delete", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44600)$"],
        userControl: Debugging)]
    public void CircleOfLivesDrawingDelete(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"CircleOfLives{ev.SourceId}");
    }

    #endregion Circle of Lives

    #region Mass Macabre

    [ScriptMethod(name: "---- Mass Macabre ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void MassMacabreSeparator(Event ev, ScriptAccessory sa)
    {
    }
    
    [ScriptMethod(name: "Mass Macabre Refresh Parameters", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44595)$"],
        userControl: Debugging)]
    public void MassMacabreRefreshParams(Event ev, ScriptAccessory sa)
    {
        // sa.Log.Debug($"Casting Mass Macabre, refreshing parameters");
        RefreshParams();
    }
    
    [ScriptMethod(name: "Mass Macabre Tower 1", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44595)$"],
        userControl: true)]
    public void MassMacabreTower1(Event ev, ScriptAccessory sa)
    {
        var myIndex = sa.GetMyIndex();
        var isMelee = myIndex is 0 or 1 or 4 or 5;
        // sa.Log.Debug($"Mass Macabre Tower 1 guidance");
        sa.DrawGuidance(new Vector3(100, 0, isMelee ? 94 : 106), 0, 10000, $"MassMacabreTower1");
    }
    
    [ScriptMethod(name: "Mass Macabre Tower Subsequent", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44819)$"],
        userControl: Debugging)]
    public void MassMacabreTowerCount(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId != sa.Data.Me) return;
        _numbers[7]++;
        // sa.Log.Debug($"Completed Mass Macabre tower, count {_numbers[7]}");
        sa.Method.RemoveDraw($"MassMacabreTower{_numbers[7]}.*");

        var myIndex = sa.GetMyIndex();
        switch (_numbers[7])
        {
            case 1:
                var mySecondTower = new Vector3(85f, 0, 103f);
                if (myIndex is 4 or 5 or 6 or 7) mySecondTower = mySecondTower.FoldPointVertical(Center.Z);
                if (myIndex is 1 or 3 or 4 or 5) mySecondTower = mySecondTower.FoldPointHorizon(Center.X);
                // sa.Log.Debug($"Guidance to Tower #2 {mySecondTower}");
                sa.DrawGuidance(mySecondTower, 0, 4000, $"MassMacabreTower2Prepare", isSafe: false);
                sa.DrawGuidance(mySecondTower, 4000, 6000, $"MassMacabreTower2", isSafe: true);
                break;
            case 2:
                var myThirdTower = myIndex switch
                {
                    2 or 6 or 7 => new Vector3(93.5f, 0, 112f),
                    3 or 4 or 5 => new Vector3(93.5f, 0, 112f).PointCenterSymmetry(Center),
                    0 => new Vector3(82.5f, 0, 85.5f),
                    1 => new Vector3(82.5f, 0, 85.5f).FoldPointHorizon(Center.X),
                };
                // sa.Log.Debug($"Guidance to Tower #3 {myThirdTower} or tank buster point");
                sa.DrawGuidance(myThirdTower, 0, 4000, $"MassMacabreTower3Prepare", isSafe: false);
                sa.DrawGuidance(myThirdTower, 4000, 6000, $"MassMacabreTower3", isSafe: true);
                break;
            case 3:
                // Both tanks usually don't reach this step
                var myFourthTower = myIndex switch
                {
                    2 or 6 or 7 => new Vector3(93.5f, 0, 112f).FoldPointHorizon(Center.X),
                    3 or 4 or 5 => new Vector3(93.5f, 0, 112f).FoldPointVertical(Center.Z),
                    0 => new Vector3(82.5f, 0, 85.5f),
                    1 => new Vector3(82.5f, 0, 85.5f).FoldPointHorizon(Center.X),
                };
                // sa.Log.Debug($"Guidance to Tower #4 {myFourthTower} or tank buster point");
                sa.DrawGuidance(myFourthTower, 0, 4000, $"MassMacabreTower4Prepare", isSafe: false);
                sa.DrawGuidance(myFourthTower, 4000, 6000, $"MassMacabreTower4", isSafe: true);
                break;
            default:
                break;
        }
    }
    
    #endregion Mass Macabre
    
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
    
}

public static class IbcHelper
{
    public static IGameObject? GetById(this ScriptAccessory sa, ulong gameObjectId)
    {
        return sa.Data.Objects.SearchById(gameObjectId);
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
    public static Vector3 RotateAndExtend(this Vector3 point, Vector3 center, float radian, float length)
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
        if (x < 1 || x > length) return -1;
        var digitChar = valStr[length - x]; // Take the x-th digit from the right
        return int.Parse(digitChar.ToString());
    }
}

#endregion Calculation Functions

#region Index Helper Functions
public static class IndexHelper
{
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
                throw new ArgumentException($"ownerObj {ownerObj} 的目标类型 {ownerObj.GetType()} 输入错误");
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
                throw new ArgumentException($"targetObj {targetObj} 的目标类型 {targetObj.GetType()} 输入错误");
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
            innerScale, DrawModeEnum.Default, DrawTypeEnum.Fan, isSafe, byTime, false, draw);

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
    /// Assign enmity order drawing based on ownerId to the given dp.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="orderIdx">Enmity order, starting from 1</param>
    /// <returns></returns>
    public static DrawPropertiesEdit SetOwnersEnmityOrder(this DrawPropertiesEdit self, uint orderIdx)
    {
        self.TargetResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        self.TargetOrderIndex = orderIdx;
        return self;
    }
    
    /// <summary>
    /// Assign distance-based targeting to the given dp, using the position as reference
    /// </summary>
    /// <param name="self"></param>
    /// <param name="isNearOrder">Order by near or far from the position</param>
    /// <param name="orderIdx">Starting from 1</param>
    /// <returns></returns>
    public static DrawPropertiesEdit SetPositionDistanceOrder(this DrawPropertiesEdit self, bool isNearOrder,
        uint orderIdx)
    {
        self.TargetResolvePattern = isNearOrder
            ? PositionResolvePatternEnum.PlayerNearestOrder
            : PositionResolvePatternEnum.PlayerFarestOrder;
        self.TargetOrderIndex = orderIdx;
        return self;
    }
}

#endregion Drawing Functions

#region Special Functions

public static class SpecialFunc
{
    [Flags]
    public enum DrawState : uint
    {
        Invisibility      = 0x00_00_00_02,
        IsLoading         = 0x00_00_08_00,
        SomeNpcFlag       = 0x00_00_01_00,
        MaybeCulled       = 0x00_00_04_00,
        MaybeHiddenMinion = 0x00_00_80_00,
        MaybeHiddenSummon = 0x00_80_00_00,
    }
    
    public static unsafe DrawState* ActorDrawState(IGameObject actor)
        => (DrawState*)(&((GameObject*)actor.Address)->RenderFlags);
    
    public static unsafe void WriteInvisible(this ScriptAccessory sa, IGameObject? actor)
    {
        try
        {
            // Invisibility      = 0x00_00_00_02,
            *ActorDrawState(actor!) |= DrawState.Invisibility;
        }
        catch (Exception e)
        {
            sa.Log.Error(e.ToString());
            throw;
        }
    }
    
    public static unsafe void WriteVisible(this ScriptAccessory sa, IGameObject? actor)
    {
        try
        {
            *ActorDrawState(actor!) &= ~DrawState.Invisibility;
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
using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Data;
using KodakkuAssist.Module.Draw.Manager;
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
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;
using Lumina.Excel.Sheets;

namespace KodakkuScripts.UsamisKodakku._07_DawnTrail.UnrealShinryu;

[ScriptType(name: Name, territorys: [730, 1372], guid: "fcf45bf5-bb72-42f8-b918-4c4b779fb70c",
    version: Version, author: "Usami", note: NoteStr, updateInfo: UpdateInfo)]

public class UnrealShinryu
{
    const string NoteStr =
        $"""
        {Version}
        Initial version
        """;
    
    const string UpdateInfo =
        $"""
         {Version}
         1. Added P1 Supernova safe zone puddle indicator
         2. Added P1 Spiral Charge danger zone indicator
         """;

    private const string Name = "Shinryu [Unreal]";
    private const string Version = "0.0.0.3";
    private const string DebugVersion = "a";
    private const bool Debugging = false;
    private static readonly List<string> Role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
    private static readonly Vector3 Center1 = new Vector3(0, -380, 0);
    
    private long lastTriggerTime;
    
    private ShinryuParams _shinryuParam = new ShinryuParams();

    public void Init(ScriptAccessory sa)
    {
        sa.Log.Debug($"[DEBUG] Script {Name} v{Version}{DebugVersion} initialized.");
        sa.Method.RemoveDraw(".*");
        _shinryuParam.Reset(sa);
    }
    
    [ScriptMethod(name: "Duty Detection", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(9720|50260)$"],
        userControl: Debugging)]
    public void DetectDuty(Event ev, ScriptAccessory sa)
    {
        if (_shinryuParam.DutyDetected) return;
        _shinryuParam.IsUnreal = ev.ActionId != 9720;
        _shinryuParam.DutyDetected = true;
    }
    
    [ScriptMethod(name: "Detect Earth's Fury", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:4", "Id2:8"],
        userControl: Debugging)]
    public void DetectEarthsFury(Event ev, ScriptAccessory sa)
    {
        var distance = (ev.SourcePosition - Center1).Length();
        if (distance > 2f) return;
        _shinryuParam.GreenFloorCracks = true;
        sa.DebugMsg($"[INFO]【Earth's Fury】Green floor cracks True");
    }
    
    [ScriptMethod(name: "Flame Chain", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(0061)$"],
        userControl: true)]
    public void FlameChain(Event ev, ScriptAccessory sa)
    {
        lock (_shinryuParam)
        {
            if (MathTools.Debounce(ref lastTriggerTime, 500))
            {
                _shinryuParam.FlameChainCount++;
                sa.DebugMsg($"[INFO]【Flame Chain】Current count: {_shinryuParam.FlameChainCount}");
            };
        }
        
        if (ev.TargetId != sa.Data.Me) return;
        if (_shinryuParam.FlameChainCount is 3 or 4) return;
        
        var myIndex = sa.GetMyIndex();
        var dir = (myIndex % 4) switch
        {
            0 => "top-left",
            1 => "top-right",
            2 => _shinryuParam.FlameChainCount == 2 ? "top-left" : "bottom-left",
            3 => _shinryuParam.FlameChainCount == 2 ? "top-right" : "bottom-right",
            _ => ""
        };
        sa.Method.TextInfo($"Break chain at {dir}", 2500);
        sa.Method.TTS($"Break chain at {dir}");
    }
    
    [ScriptMethod(name: "Tidal Wave Knockback", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(9690|50230)$"],
        userControl: true)]
    public void TidalWaveKnockback(Event ev, ScriptAccessory sa)
    {
        var region = ev.SourcePosition.GetRadian(Center1).RadianToRegion(4, 0, true);
        DrawRegionKnockback(sa, region , "Tidal Wave Knockback");
        sa.DebugMsg($"[INFO]【Tidal Wave Knockback】Direction {region}");
        
        void DrawRegionKnockback(ScriptAccessory sa, int dir, string name)
        {
            var dp = sa.DrawLine(sa.Data.Me, 0, 4000, 6000, name, dir * 90f.DegToRad() + 180f.DegToRad(), 3f, 35f,
                sa.Data.DefaultDangerColor.WithW(2f), draw: false);
            dp.FixRotation = true;
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }
    }
    
    [ScriptMethod(name: "Icicle Impale Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(9712|50252)$"],
        userControl: true)]
    public void IcicleImpaleLine(Event ev, ScriptAccessory sa)
    {
        sa.DrawRect(ev.SourceId, 0, 2500, $"Icicle Impale", 0, 10, 60, sa.Data.DefaultDangerColor.WithW(2f));
    }
    
    [ScriptMethod(name: "Tail Placement", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(007E)$"],
        userControl: true)]
    public void TailPlacement(Event ev, ScriptAccessory sa)
    {
        _shinryuParam.TailPlacementCount++;
        sa.DebugMsg($"[INFO]【Tail Placement】Current count: {_shinryuParam.TailPlacementCount}");
        
        if (ev.TargetId != sa.Data.Me) return;
        var pos = _shinryuParam.TailPlacementCount switch
        {
            1 => "bottom-right",
            2 => "bottom-left",
            3 => _shinryuParam.GreenFloorCracks ? "top-left" : "center",
            _ => ""
        };
        sa.Method.TextInfo($"Place tail at {pos}", 2500);
        sa.Method.TTS($"Place tail at {pos}");
    }
    
    [ScriptMethod(name: "Tail Smash", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(9698|50238)$"],
        userControl: true)]
    public void TailSmash(Event ev, ScriptAccessory sa)
    {
        sa.DrawRect(ev.SourcePosition, 0, 3000, $"Tail Smash", ev.SourceRotation, 20, 20, sa.Data.DefaultDangerColor.WithW(2f));
        sa.DrawRect(ev.SourcePosition, 0, 3000, $"Tail Smash", ev.SourceRotation + 180f.DegToRad(), 20, 20, sa.Data.DefaultDangerColor.WithW(2f));
    }
    
    [ScriptMethod(name: "Lightning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(9706|50246)$"],
        userControl: true)]
    public void Lightning(Event ev, ScriptAccessory sa)
    {
        var sinks = sa.GetByDataId(2004237);
        foreach (var sink in sinks)
            sa.DrawCircle(sink!.GameObjectId, 0, 8000, $"Puddle", 5.25f, new Vector4(1, 0, 0, 5));
        
        foreach (var p in sa.Data.PartyList.Where(p => p != sa.Data.Me))
            sa.DrawCircle(p, 0, 8000, $"Lightning Spread", 5, sa.Data.DefaultDangerColor.WithW(2f), byTime: true);
    }
    
    [ScriptMethod(name: "Snowstorm AOE Prompt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(9713|50253)$"],
        userControl: true)]
    public void Snowstorm(Event ev, ScriptAccessory sa)
    {
        sa.Method.TextInfo($"AOE", 2500, true);
        sa.Method.TTS($"AOE");
    }
    
    [ScriptMethod(name: "Death Sentence", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(9715|50255)$"],
        userControl: true)]
    public void DeathSentence(Event ev, ScriptAccessory sa)
    {
        _shinryuParam.DeathSentenceCount++;
        sa.DebugMsg($"[INFO]【Death Sentence】Count: {_shinryuParam.DeathSentenceCount}");

        var myIndex = sa.GetMyIndex();
        var hintText = myIndex switch
        {
            0 or 1 => "Stack tankbuster",
            _ => "Avoid Light of Judgment"
        };
        sa.Method.TextInfo(hintText, 2500, true);
        sa.Method.TTS(hintText);

        var isTank = sa.Data.MyObject!.IsTank();
        sa.DrawCircle(ev.TargetId, 0, 4000, $"Death Sentence Target", 4f,
            (isTank ? sa.Data.DefaultSafeColor : sa.Data.DefaultDangerColor).WithW(2f), byTime: true);
    }
    
    [ScriptMethod(name: "Judgment Bolt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(9723|50263)$"],
        userControl: true)]
    public void JudgmentBolt(Event ev, ScriptAccessory sa)
    {
        var sinks = sa.GetByDataId(2004237);
        foreach (var sink in sinks)
            sa.DrawCircle(sink!.GameObjectId, 0, 8000, $"Puddle", 5.25f, new Vector4(1, 0, 0, 5));
    }
    
    [ScriptMethod(name: "Hellfire and Supernova", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(9722|50262)$"],
        userControl: true)]
    public void HellfireAndSupernova(Event ev, ScriptAccessory sa)
    {
        var sinks = sa.GetByDataId(2004237);
        foreach (var sink in sinks)
            sa.DrawCircle(sink!.GameObjectId, 0, 8000, $"Puddle", 5.25f, sa.Data.DefaultSafeColor.WithW(5f));
    }
    
    [ScriptMethod(name: "Earth Breath", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(0028)$"],
        userControl: true)]
    public void EarthBreath(Event ev, ScriptAccessory sa)
    {
        var bossObj = sa.GetByDataId(_shinryuParam.IsUnreal ? 19934u : 8026u).First();
        sa.DrawFan(bossObj!.GameObjectId, ev.TargetId, 0, 6000, $"Earth Breath Cone", 60f.DegToRad(), 0f, 80f, 0f, sa.Data.DefaultDangerColor.WithW(2f));
        if (ev.TargetId != sa.Data.Me) return;
        var myIndex = sa.GetMyIndex();
        
        var pos = myIndex switch
        {
            <= 3 => "top-left",
            _ => "top-right"
        };
        sa.Method.TextInfo($"Guide cone at {pos}", 2500);
        sa.Method.TTS($"Guide cone at {pos}");
    }
    
    [ScriptMethod(name: "Diamond Dust", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(9724|50264)$"],
        userControl: true)]
    public void DiamondDust(Event ev, ScriptAccessory sa)
    {
        var myIndex = sa.GetMyIndex();
        sa.Method.TextInfo($"AOE, gather center{(myIndex == 3 ? ", skating soon" : "")}", 2500, true);
        sa.Method.TTS($"AOE, gather center{(myIndex == 3 ? ", skating soon" : "")}");
    }
    
    [ScriptMethod(name: "Atmospheric Burst", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(9726|50266)$"],
        userControl: true)]
    public void AtmosphericBurst(Event ev, ScriptAccessory sa)
    {
        sa.DrawKnockBack(Center1, 0, 10000, $"Atmospheric Burst", 3f, 20f, sa.Data.DefaultDangerColor.WithW(2f));
    }
    
    [ScriptMethod(name: "Spiral Charge", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(9731|50271)$"],
        userControl: true)]
    public void SpiralCharge(Event ev, ScriptAccessory sa)
    {
        sa.DrawRect(ev.SourceId, 0, 6000, $"Spiral Charge", 0, 60, 100, sa.Data.DefaultDangerColor.WithW(1.5f));
    }
    
    [ScriptMethod(name: "Aetheric Ray Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(9752|50292)$"],
        userControl: true)]
    public void AethericRayLine(Event ev, ScriptAccessory sa)
    {
        var dp = sa.DrawRect(ev.SourceId, 0, 2500, $"Aetheric Ray", 0, 3, 100, sa.Data.DefaultDangerColor.WithW(2f), draw: false);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "Trillion Slash", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(9803|50343)$"],
        userControl: true)]
    public void TrillionSlash(Event ev, ScriptAccessory sa)
    {
        if (!sa.Data.MyObject!.IsTank()) return;
        sa.Method.TextInfo($"Tank swap", 2500, true);
        sa.Method.TTS($"Tank swap");
    }
    
    [ScriptMethod(name: "Supernova (P3)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(10015|50305)$"],
        userControl: true)]
    public void SupernovaP3(Event ev, ScriptAccessory sa)
    {
        sa.Method.TextInfo($"Gather, stop moving", 2500, true);
        sa.Method.TTS($"Gather, stop moving");
    }
    
    [ScriptMethod(name: "Lightning (P3)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(10016|50306)$"],
        userControl: true)]
    public void LightningP3(Event ev, ScriptAccessory sa)
    {
        sa.Method.TextInfo($"Gather, keep moving", 2500, true);
        sa.Method.TTS($"Gather, keep moving");
    }
    
    [ScriptMethod(name: "Lightning Cyclone (P3)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(10021|50309)$"],
        userControl: true)]
    public void LightningCycloneP3(Event ev, ScriptAccessory sa)
    {
        if (MathTools.Debounce(ref lastTriggerTime, 500))
        {
            sa.Method.TextInfo($"Move out!", 2500, true);
            sa.Method.TTS($"Move out!");
        };
        sa.DrawCircle(ev.EffectPosition, 0, 3000, $"Lightning Cyclone", 5f, sa.Data.DefaultDangerColor.WithW(2f));
    }
    
    [ScriptMethod(name: "Shinryu's Howl Donut", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(9800|50293)$"],
        userControl: true)]
    public void ShinryusHowlDonut(Event ev, ScriptAccessory sa)
    {
        sa.DrawDonut(ev.SourceId, 0, 5000, $"Shinryu's Howl", 50, 10, sa.Data.DefaultDangerColor.WithW(2f));
    }

    #region Parameter Container Class
    private class ShinryuParams
    {
        public int TailPlacementCount = 0;
        public int FlameChainCount = 0;
        public bool GreenFloorCracks = false;
        public int DeathSentenceCount = 0;
        public bool IsUnreal = false;
        public bool DutyDetected = false;
        public void Reset(ScriptAccessory sa)
        {
            TailPlacementCount = 0;
            FlameChainCount = 0;
            GreenFloorCracks = false;
            DeathSentenceCount = 0;
            IsUnreal = false;
            DutyDetected = false;
            
            sa.DebugMsg($"Shinryu parameters Reset", Debugging);
        }
    }
    #endregion
}
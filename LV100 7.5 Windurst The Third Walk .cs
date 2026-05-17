using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Windurst_The_Third_Walk;

[ScriptType(guid: "fa374b84-5ce5-405c-a22d-3e7ea1c9591b", name: "LV100 7.5 Windurst: The Third Walk", territorys: [1368],
    version: "0.0.0.4", Author: "Linoa235", note: noteStr)]

public class Windurst_The_Third_Walk
{
    const string noteStr =
        """
        LV100 7.5 Windurst: The Third Walk v0.0.0.4:
        Initial drawing, incomplete, makeshift for now!
        """;
    
    #region User Controls

    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [UserSetting("Developer Mode")]
    public bool isDeveloper { get; set; } = false;

    #endregion
    
    #region Trash Mobs
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€”â€”â€”â€”  Trash Mobs  â€”â€”â€”â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void TrashMobs(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "1.5 Abaddon_Cleave (Center Knockback)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50481$"])]
    public void Abaddon_Cleave(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Cleave_Center Knockback Prediction";
        dp.Scale = new(0.5f, 22f);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "1.5 Nemean Lion_Wind Vortex (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50091$"])]
    public void NemeanLion_WindVortex(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Nemean Lion_Wind Vortex";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (50f, 40f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "1.5 Nemean Lion_Explosion AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50092$"])]
    public void Explosion(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "1.5 Nemean Lion_Golden Explosion AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50093$"])]
    public void GoldenExplosion(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        
        if (isTTS && !isHealer) accessory.Method.TTS($"Interrupt <Nemean Lion>");
        if (isTTS && !isHealer) accessory.Method.EdgeTTS($"Interrupt <Nemean Lion>");
        
        if (isText && isTank) accessory.Method.TextInfo($"Interrupt <Nemean Lion>", duration: 4000, true);
    }
    
    [ScriptMethod(name: "1.5 Medusa_Left/Right Shadow Slash (Cone)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5009[89]$"])]
    public void ShadowSlash(Event @event, ScriptAccessory accessory)
    {
        var isR = @event.ActionId == 50098;

        if (isR)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Right Shadow Slash";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(60);
            dp.Radian = 180f.DegToRad(); 
            dp.Rotation = 270f.DegToRad();
            dp.DestoryAt = 4700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        else
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"Left Shadow Slash";
            dp1.Color = accessory.Data.DefaultDangerColor;
            dp1.Owner = @event.SourceId();
            dp1.Scale = new Vector2(60f);
            dp1.Radian = 180f.DegToRad(); 
            dp1.Rotation = 90f.DegToRad();
            dp1.DestoryAt = 4700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
        }
    }
    
    [ScriptMethod(name: "1.5 Medusa_Howl AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50103$"])]
    public void Howl(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "1.5 Medusa_Contempt AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50100$"])]
    public void Contempt(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "1.5 Medusa_Petrification Look Away", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50102$"])]
    public void Medusa_Petrification(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Look away from Medusa");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Look away from Medusa");
    }
    
    [ScriptMethod(name: "2.5 Ava_Ain_Shockwave AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50486$"])]
    public void Shockwave(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"AOE");
    }
    
    #endregion
    
    #region BOSS1_Demon Shantotto

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€”  BOSS1_Demon Shantotto  â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Boss1_DemonShantotto(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "Incensed Flare AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50215$"])]
    public void IncensedFlare(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "Vidofnir Stack Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50214$"])]
    public void Vidofnir(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Stack tankbuster");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Stack tankbuster");
        
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Vidofnir";
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 6000;
        if (!isTank) 
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        else
        {
            dp.Color = accessory.Data.DefaultSafeColor;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Pulverizing Earthquake (Sandwich Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50196$"])]
    public void PulverizingEarthquake(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Pulverizing Earthquake";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (12f, 30f);
        dp.DestoryAt = 8700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "Empirical Study (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50208$"])]
    public void EmpiricalStudy(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Empirical Study";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (12f, 80f);
        dp.DestoryAt = 3500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "Ring Jump Fire (Initial Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50201$"])]
    public void RingJumpFire(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Ring Jump Fire";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.0f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(70f);
        dp.InnerScale = new Vector2(6f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Deploy Footwork_Ring Jump Fire (Continuous Donut Positions)", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^01(80|7F)$"])]
    public void DeployFootwork_RingJumpFire(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Ring Jump Fire{@event.TargetId}";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(10f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.InnerScale = new Vector2(5.9f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = @event.Id == 0180 ? 20200 : 23900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Approach Cooling Frost (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50203$"])]
    public void ApproachCoolingFrost(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Ring Jump Fire.*");
        
        if (isTTS) accessory.Method.TTS($"Move away plus spread");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Move away plus spread");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Approach Cooling Frost";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 1900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Meteor (Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50185$"])]
    public void Meteor(Event @event, ScriptAccessory accessory)
    {
        string targetName = @event["TargetName"]?.ToString();
        if (!string.IsNullOrEmpty(targetName))
        {
            if (isText) accessory.Method.TextInfo($"Stack on {targetName}", duration: 5300, true);
            if (isTTS) accessory.Method.TTS($"Stack on {targetName}");
            if (isEdgeTTS) accessory.Method.EdgeTTS($"Stack on {targetName}");
        }
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Meteor";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Shockwave Large AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50187$"])]
    public void ShockwaveLargeAOE(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Large AOE damage", duration: 9500, true);
        if (isTTS) accessory.Method.TTS($"Large AOE damage");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Large AOE damage");
    }
    
    [ScriptMethod(name: "Blowing Gale Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^539[89]$"])]
    public void BlowingGale(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo($"Knockback to wall", duration: 9300, true);
        if (isTTS) accessory.Method.TTS($"Knockback to wall");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Knockback to wall");
    }
    
    [ScriptMethod(name: "Final Exam (Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50211$"])]
    public void FinalExam(Event @event, ScriptAccessory accessory)
    {
        string targetName = @event["TargetName"]?.ToString();
        if (!string.IsNullOrEmpty(targetName))
        {
            if (isText) accessory.Method.TextInfo($"Stack on {targetName}", duration: 7100, true);
            if (isTTS) accessory.Method.TTS($"Stack on {targetName}");
            if (isEdgeTTS) accessory.Method.EdgeTTS($"Stack on {targetName}");
        }
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Final Exam";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 7800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region BOSS2_Giant Reappearance Alexander
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€”  BOSS2_Giant Reappearance Alexander  â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Boss2_Alexander(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Strong Banish IV AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50161$"])]
    public void StrongBanishIV(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "Judgment Light (Half-room)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5014[67]$"])]
    public void JudgmentLight(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Judgment Light";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.Radian = 180f.DegToRad(); 
        dp.Delay = @event.ActionId == 50146 ? 0 : 6800;
        dp.DestoryAt = @event.ActionId == 50146 ? 6700 : 2900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "Holy Flame (Triangle but makeshift for now)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50143$"])]
    public void HolyFlame(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Holy Flame";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "Million Holy (Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50158$"])]
    public void MillionHoly(Event @event, ScriptAccessory accessory)
    {
        string targetName = @event["TargetName"]?.ToString();
        if (!string.IsNullOrEmpty(targetName))
        {
            if (isText) accessory.Method.TextInfo($"Stack on {targetName}", duration: 5300, true);
            if (isTTS) accessory.Method.TTS($"Stack on {targetName}");
            if (isEdgeTTS) accessory.Method.EdgeTTS($"Stack on {targetName}");
        }
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Million Holy{@event.TargetId}";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 10200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Gordias System_Reflect Wall", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50686$"])]
    public void ReflectWall(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Stop attacking <Reflect Wall>", duration: 3300, true);
        if (isTTS) accessory.Method.TTS($"Stop attacking Reflect Wall");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Stop attacking Reflect Wall");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Reflect Wall{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 20000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Divine Judgment AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50153$"])]
    public void DivineJudgment(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Large AOE damage", duration: 9000, true);
        if (isTTS) accessory.Method.TTS($"Large AOE damage");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Large AOE damage");
    }
    
    private static int _ElectrifyCount = 0;

    [ScriptMethod(name: "Gordias System_Support Order_Large Discharge (Tether Chariot)", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^01AC$"])]
    public void SupportOrder_LargeDischarge(Event @event, ScriptAccessory accessory)
    {
        _ElectrifyCount++;
    
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Support Order{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(18f);
    
        int groupIndex = (_ElectrifyCount - 1) / 2;
        if (groupIndex % 2 == 1)
        {
            dp.Delay = 5000;
            dp.DestoryAt = 3000;
        }
        else
        {
            dp.DestoryAt = 9000;
        }
    
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Holy Lance (Line Tankbuster)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50160$"])]
    public void HolyLance(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Holy Lance{@event.TargetId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId;
        dp.Scale = new (6f, 60f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }

    #endregion
    
    #region BOSS3_Promathia
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€”  BOSS3_Promathia  â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Boss3_Promathia(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Divine Song AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50317$"])]
    public void DivineSong(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "Instant_Explosion (Moving Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50320$"])]
    public void Instant_Explosion(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Instant_Explosion{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(16f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Illusory Ring (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50321$"])]
    public void IllusoryRing(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Illusory Ring";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 12400;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Netherworld Hearth_Twilight Paradise (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50331$"])]
    public void TwilightParadise(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Go behind plus get close");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Go behind plus get close");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Twilight Paradise";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Vacuous Sigh_Aurora Veil (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50355$"])]
    public void AuroraVeil(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Aurora Veil";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (7f, 7f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "Vacuous Thought_Wind of Nothingness (Rotating Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50354$"])]
    public void WindOfNothingness(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Wind of Nothingness";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (6f, 16f);
        dp.DestoryAt = 300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "Void Seed Knockback", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50349$"], suppress: 1000)]
    public void VoidSeed(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Knockback to wall", duration: 4300, true);
        if (isTTS) accessory.Method.TTS($"Knockback to wall");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Knockback to wall");
    }
    
    [ScriptMethod(name: "Soulbirth AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50694$"])]
    public void Soulbirth(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Large AOE + knockdown", duration: 9000, true);
        if (isTTS) accessory.Method.TTS($"Large AOE + knockdown");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Large AOE + knockdown");
    }
    
    [ScriptMethod(name: "Heavenly Salvation (Half-room Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50329$"])]
    public void HeavenlySalvation(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Heavenly Salvation";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (50f, 50f);
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "Netherworld Hearth (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50332$"])]
    public void NetherworldHearth(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Netherworld Hearth";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (5f, 50f);
        dp.DestoryAt = 7200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "Lure (Tower Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50565$"])]
    public void Lure(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Move away");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Move away");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Lure{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region BOSS4_Shinryu
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€”  BOSS4_Shinryu  â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Boss4_Shinryu(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Cosmic Breath (Upper Half)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49107$"])]
    public void CosmicBreath(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"â†“ Go to the lower platform â†“", duration: 6300, true);
        if (isTTS) accessory.Method.TTS($"Go to lower platform");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Go to lower platform");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Cosmic Breath";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (70f, 50f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "Cosmic Tail (Lower Half)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49110$"])]
    public void CosmicTail(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"â†‘ Go to the upper platform â†‘", duration: 6300, true);
        if (isTTS) accessory.Method.TTS($"Go to upper platform");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Go to upper platform");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Cosmic Tail";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (70f, 50f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "Twilight Drape_Twilight Radiance Debuff Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^5352$"])]
    public void TwilightDrape_TwilightRadiance(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo($"Stay on the upper platform during Twilight Nebula (Light)", duration: 4300, true);
        if (isTTS) accessory.Method.TTS($"Stay on upper platform (Light)");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Stay on upper platform (Light)");
    }
    
    [ScriptMethod(name: "Twilight Drape_Twilight Shadow Debuff Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^5353$"])]
    public void TwilightDrape_TwilightShadow(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo($"Stay on the lower platform during Twilight Nebula (Dark)", duration: 4300, true);
        if (isTTS) accessory.Method.TTS($"Stay on lower platform (Dark)");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Stay on lower platform (Dark)");
    }
    
    [ScriptMethod(name: "Atomic Tail (Lower Half)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49130$"])]
    public void AtomicTail(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"â†‘ Go to upper platform, platform destruction â†‘", duration: 6300, true);
        if (isTTS) accessory.Method.TTS($"Go to upper platform");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Go to upper platform");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Atomic Tail";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (70f, 50f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    #endregion
    
    #region BOSS4_Void King
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”â€”â€”  BOSS4_Void King  â€”â€”â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Boss4_VoidKing(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Void Declaration AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49179$"])]
    public void VoidDeclaration(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Large AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Large AOE");
    }
    
    [ScriptMethod(name: "Left/Right Cross Sword", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4915[3-6]$"])]
    public void CrossSword(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp.Scale = new (30f, 60f);
        dp1.Scale = new (36f, 70f);
        dp1.Color = dp.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = dp.Owner = @event.SourceId();
        dp1.DestoryAt = dp.DestoryAt = 8700;
        
        switch (@event.ActionId())
        {
            case 49153:
                dp.Name = "Right Cross Sword_Diagonal";
                dp.Offset = new Vector3(15, 0, 15);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
                break;
            case 49154:
                dp.Name = "Left Cross Sword_Diagonal";
                dp.Offset = new Vector3(-15, 0, 15);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
                break;
            case 49155:
                dp1.Name = "Left Cross Sword_Straight";
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1); 
                break;
            case 49156:
                dp1.Name = "Right Cross Sword_Straight";
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1); 
                break;
        }
    }
    
    [ScriptMethod(name: "Dual Breath (Chariot Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^491(59|60)$"])]
    public void DualBreath(Event @event, ScriptAccessory accessory)
    {
        switch (@event.ActionId())
        {
            case 49159:
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Dual Breath_Donut";
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = @event.SourceId();
                dp.Scale = new Vector2(60f);
                dp.InnerScale = new Vector2(20f);
                dp.Radian = 90f.DegToRad(); 
                dp.DestoryAt = 5700;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                break;
            case 49160:
                var dp1 = accessory.Data.GetDefaultDrawProperties();
                dp1.Name = $"Dual Breath_Chariot";
                dp1.Color = accessory.Data.DefaultDangerColor;
                dp1.Owner = @event.SourceId();
                dp1.Scale = new Vector2(35f);
                dp1.Radian = 90f.DegToRad(); 
                dp1.DestoryAt = 5700;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
                break;
        }
    }
    
    [ScriptMethod(name: "Cataclysm Blade (Cone)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49162$"])]
    public void CataclysmBlade(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Cataclysm Blade";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 45f.DegToRad(); 
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "Atomic Ray (Moving Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49165$"])]
    public void AtomicRay(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Avoid line");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Avoid line");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Atomic Ray";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (15f, 60f);
        dp.DestoryAt = 1200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    #endregion

    #region Drawing Cleanup

    [ScriptMethod(name: "Cast Interrupt Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: [], userControl: false)]
    public void CastInterruptCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Death Cleanup", eventType: EventTypeEnum.Death, eventCondition: [], userControl: false)]
    public void DeathCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "Anti-Knockback Cleanup", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(160|1209|2663)$"], userControl: false)]
    public void AntiKnockbackCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw($".*(Knockback|Pull).*");
    }
    
    [ScriptMethod(name: "Reflect Wall Cleanup", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:regex:^5377$"], userControl: false)]
    public void ReflectWallCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Reflect Wall.*");
    }

    #endregion
}

#region EventExtensions

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

    public static uint Command(this Event @event)
    {
        return ParseHexId(@event["Command"], out var cid) ? cid : 0;
    }
    
    public static uint DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["DurationMilliseconds"]);
    }

    public static float SourceRotation(this Event @event)
    {
        return JsonConvert.DeserializeObject<float>(@event["SourceRotation"]);
    }

    public static float TargetRotation(this Event @event)
    {
        return JsonConvert.DeserializeObject<float>(@event["TargetRotation"]);
    }

    public static byte Index(this Event @event)
    {
        return (byte)(ParseHexId(@event["Index"], out var index) ? index : 0);
    }

    public static uint State(this Event @event)
    {
        return ParseHexId(@event["State"], out var state) ? state : 0;
    }

    public static string SourceName(this Event @event)
    {
        return @event["SourceName"];
    }

    public static string TargetName(this Event @event)
    {
        return @event["TargetName"];
    }

    public static uint TargetId(this Event @event)
    {
        return ParseHexId(@event["TargetId"], out var id) ? id : 0;
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

    public static uint DirectorId(this Event @event)
    {
        return ParseHexId(@event["DirectorId"], out var id) ? id : 0;
    }

    public static uint StatusID(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StatusId"]);
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

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;
        return attribute?.Description ?? value.ToString();
    }
}

public static class MathTools
{
    public static float DegToRad(this float deg) => (deg + 360f) % 360f / 180f * float.Pi;
    public static float RadToDeg(this float rad) => (rad + 2 * float.Pi) % (2 * float.Pi) / float.Pi * 180f;

    public static float GetRadian(this Vector3 point, Vector3 center)
        => MathF.Atan2(point.X - center.X, point.Z - center.Z);

    public static float GetLength(this Vector3 point, Vector3 center)
        => new Vector2(point.X - center.X, point.Z - center.Z).Length();

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

    public static int RadianToRegion(this float radian, int regionNum, int baseRegionIdx = 0, bool isDiagDiv = false, bool isCw = false)
    {
        var sepRad = float.Pi * 2 / regionNum;
        var inputAngle = radian * (isCw ? -1 : 1) + (isDiagDiv ? sepRad / 2 : 0);
        var rad = (inputAngle + 4 * float.Pi) % (2 * float.Pi);
        return ((int)Math.Floor(rad / sepRad) + baseRegionIdx + regionNum) % regionNum;
    }

    public static Vector3 FoldPointHorizon(this Vector3 point, float centerX)
        => point with { X = 2 * centerX - point.X };

    public static Vector3 FoldPointVertical(this Vector3 point, float centerZ)
        => point with { Z = 2 * centerZ - point.Z };

    public static Vector3 PointCenterSymmetry(this Vector3 point, Vector3 center)
        => point.RotateAndExtend(center, float.Pi, 0);

    public static int GetDecimalDigit(this int val, int x)
    {
        var valStr = val.ToString();
        var length = valStr.Length;
        if (x < 1 || x > length) return -1;
        var digitChar = valStr[length - x];
        return int.Parse(digitChar.ToString());
    }
}

public enum MarkType
{
    None = -1,
    Attack1 = 0,
    Attack2 = 1,
    Attack3 = 2,
    Attack4 = 3,
    Attack5 = 4,
    Bind1 = 5,
    Bind2 = 6,
    Bind3 = 7,
    Ignore1 = 8,
    Ignore2 = 9,
    Square = 10,
    Circle = 11,
    Cross = 12,
    Triangle = 13,
    Attack6 = 14,
    Attack7 = 15,
    Attack8 = 16,
    Count = 17
}

public static class IbcHelper
{
    public static IGameObject? GetById(this ScriptAccessory sa, ulong gameObjectId)
    {
        return sa.Data.Objects.SearchById(gameObjectId);
    }

    public static IGameObject? GetMe(this ScriptAccessory sa)
    {
        return sa.Data.Objects.LocalPlayer;
    }

    public static IEnumerable<IGameObject?> GetByDataId(this ScriptAccessory sa, uint dataId)
    {
        return sa.Data.Objects.Where(x => x.DataId == dataId);
    }

    public static string GetPlayerJob(this ScriptAccessory sa, IPlayerCharacter? playerObject, bool fullName = false)
    {
        if (playerObject == null) return "None";
        return fullName ? playerObject.ClassJob.Value.Name.ToString() : playerObject.ClassJob.Value.Abbreviation.ToString();
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

    public static bool HasStatus(this ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return false;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return statusIdx != -1;
        }
    }

    public static unsafe ulong GetMarkerEntityId(uint markerIndex)
    {
        var markingController = MarkingController.Instance();
        if (markingController == null) return 0;
        if (markerIndex >= 17) return 0;

        return markingController->Markers[(int)markerIndex];
    }

    public static MarkType GetObjectMarker(IGameObject? obj)
    {
        if (obj == null || !obj.IsValid()) return MarkType.None;

        ulong targetEntityId = obj.EntityId;
            
        for (uint i = 0; i < 17; i++)
        {
            var markerEntityId = GetMarkerEntityId(i);
            if (markerEntityId == targetEntityId)
            {
                return (MarkType)i;
            }
        }

        return MarkType.None;
    }

    public static bool HasMarker(IGameObject? obj, MarkType markType)
    {
        return GetObjectMarker(obj) == markType;
    }

    public static bool HasAnyMarker(IGameObject? obj)
    {
        return GetObjectMarker(obj) != MarkType.None;
    }

    private static ulong GetMarkerForObject(IGameObject? obj)
    {
        if (obj == null) return 0;
        unsafe
        {
            for (uint i = 0; i < 17; i++)
            {
                var markerEntityId = GetMarkerEntityId(i);
                if (markerEntityId == obj.EntityId)
                {
                    return markerEntityId;
                }
            }
        }
        return 0;
    }

    private static MarkType GetMarkerTypeForObject(IGameObject? obj)
    {
        if (obj == null) return MarkType.None;
        unsafe
        {
            for (uint i = 0; i < 17; i++)
            {
                var markerEntityId = GetMarkerEntityId(i);
                if (markerEntityId == obj.EntityId)
                {
                    return (MarkType)i;
                }
            }
        }
        return MarkType.None;
    }

    public static string GetMarkerName(MarkType markType)
    {
        return markType switch
        {
            MarkType.Attack1 => "Attack 1",
            MarkType.Attack2 => "Attack 2",
            MarkType.Attack3 => "Attack 3",
            MarkType.Attack4 => "Attack 4",
            MarkType.Attack5 => "Attack 5",
            MarkType.Bind1 => "Bind 1",
            MarkType.Bind2 => "Bind 2",
            MarkType.Bind3 => "Bind 3",
            MarkType.Ignore1 => "Ignore 1",
            MarkType.Ignore2 => "Ignore 2",
            MarkType.Square => "Square",
            MarkType.Circle => "Circle",
            MarkType.Cross => "Cross",
            MarkType.Triangle => "Triangle",
            MarkType.Attack6 => "Attack 6",
            MarkType.Attack7 => "Attack 7",
            MarkType.Attack8 => "Attack 8",
            _ => "No Marker"
        };
    }
    
    public static float GetHitboxRadius(IGameObject obj)
    {
        if (obj == null || !obj.IsValid()) return -1;
        return obj.HitboxRadius;
    }

}

public static class HelperExtensions
{
    public static unsafe uint GetCurrentTerritoryId()
    {
        return AgentMap.Instance()->CurrentTerritoryId;
    }
}

public unsafe static class ExtensionVisibleMethod
{
    public static bool IsCharacterVisible(this ICharacter chr)
    {
        var v = (IntPtr)(((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)chr.Address)->GameObject.DrawObject);
        if (v == IntPtr.Zero) return false;
        return Bitmask.IsBitSet(*(byte*)(v + 136), 0);
    }

    public static class Bitmask
    {
        public static bool IsBitSet(ulong b, int pos)
        {
            return (b & (1UL << pos)) != 0;
        }

        public static void SetBit(ref ulong b, int pos)
        {
            b |= 1UL << pos;
        }

        public static void ResetBit(ref ulong b, int pos)
        {
            b &= ~(1UL << pos);
        }

        public static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static bool IsBitSet(short b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }
}
#endregion
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

[ScriptType(guid: "5b6949a6-6139-4b52-a2d8-c087f2e94eeb", name: "LV100 7.5 Windurst: The Third Walk", territorys: [1368],
    version: "0.0.0.4", author: "Linoa235", note: noteStr)]

public class Windurst_The_Third_Walk
{
    const string noteStr =
        """
        LV100 7.5 Windurst: The Third Walk v0.0.0.4:
        Initial drawing, incomplete, use as is for now!
        """;
    
    #region User Settings

    [UserSetting("TTS toggle (choose one of the two TTS options)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS toggle (choose one of the two TTS options)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup text hint toggle")]
    public bool isText { get; set; } = true;
    
    [UserSetting("Developer mode")]
    public bool isDeveloper { get; set; } = false;

    #endregion
    
    #region Mobs
    
    [ScriptMethod(name: "———————————  Mobs  ———————————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void MobsPart(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "1.5 Abaddon - One-Handed Cut Center Knockback", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50481$"])]
    public void Abaddon_OneHandedCut(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "One-Handed Cut_Center Knockback Prediction";
        dp.Scale = new(0.5f, 22f);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "1.5 Nemean Lion - Wind Vortex (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50091$"])]
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
    
    [ScriptMethod(name: "1.5 Nemean Lion - Detonation AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50092$"])]
    public void Detonation(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "1.5 Nemean Lion - Golden Detonation AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50093$"])]
    public void GoldenDetonation(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        
        if (isTTS && !isHealer)accessory.Method.TTS($"Interrupt <Nemean Lion>");
        if (isTTS && !isHealer)accessory.Method.EdgeTTS($"Interrupt <Nemean Lion>");
        
        if (isText && isTank)accessory.Method.TextInfo($"Interrupt <Nemean Lion>", duration: 4000, true);
        // Interrupt guide line to be added later
    }
    
    [ScriptMethod(name: "1.5 Medusa - Left/Right Shadow Slash (Cone)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5009[89]$"])]
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
    
    [ScriptMethod(name: "1.5 Medusa - Howl AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50103$"])]
    public void Howl(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "1.5 Medusa - Scorn AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50100$"])]
    public void Scorn(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "1.5 Medusa - Petrify Look Away", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50102$"])]
    public void Medusa_Petrify(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"Look away from Medusa");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Look away from Medusa");
    }
    
    [ScriptMethod(name: "2.5 Ava Ein - Shockwave AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50486$"])]
    public void Shockwave(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    #endregion
    
    #region Boss1_Demon Shantotto

    [ScriptMethod(name: "————————  Boss1_Demon Shantotto  ————————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Boss1_DemonShantotto(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "Wrathful Flare AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50215$"])]
    public void WrathfulFlare(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "Vidofnir Stack Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50214$"])]
    public void Vidofnir(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"Stack tankbuster");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Stack tankbuster");
        
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Vidofnir";
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 6000;
        if(!isTank) 
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        else
        {
            dp.Color = accessory.Data.DefaultSafeColor;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Shattering Quake (Sandwich Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50196$"])]
    public void ShatteringQuake(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Shattering Quake";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (12f, 30f);
        dp.DestoryAt = 8700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "Empirical Research (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50208$"])]
    public void EmpiricalResearch(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Empirical Research";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (12f, 80f);
        dp.DestoryAt = 3500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "Ring Jump Fire (Initial Dynamo)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50201$"])]
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
    
    [ScriptMethod(name: "Expand Footwork Array - Ring Jump Fire (Continuous Dynamo Positions)", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^01(80|7F)$"])]
    public void ExpandFootworkArray_RingJumpFire(Event @event, ScriptAccessory accessory)
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
    
    [ScriptMethod(name: "Get Close to Cool Down (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50203$"])]
    public void GetCloseToCoolDown(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Ring Jump Fire.*");
        
        if (isTTS)accessory.Method.TTS($"Move away and spread");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Move away and spread");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Get Close to Cool Down";
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
            if (isText)accessory.Method.TextInfo($"Group up near {targetName}", duration: 5300, true);
            if (isTTS) accessory.Method.TTS($"Group up near {targetName}");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"Group up near {targetName}");
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
    public void ShockwaveLarge(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"Large AOE damage", duration: 9500, true);
        if (isTTS)accessory.Method.TTS($"Large AOE damage");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Large AOE damage");
    }
    
    [ScriptMethod(name: "Blowing Wind Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^539[89]$"])]
    public void BlowingWind(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo($"Knockback to wall", duration: 9300, true);
        if (isTTS)accessory.Method.TTS($"Knockback to wall");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Knockback to wall");
        
        // Draw after knockback
    }
    
    [ScriptMethod(name: "Final Exam (Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50211$"])]
    public void FinalExam(Event @event, ScriptAccessory accessory)
    {
        string targetName = @event["TargetName"]?.ToString();
        if (!string.IsNullOrEmpty(targetName))
        {
            if (isText)accessory.Method.TextInfo($"Group up near {targetName}", duration: 7100, true);
            if (isTTS) accessory.Method.TTS($"Group up near {targetName}");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"Group up near {targetName}");
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
    
    #region Boss2_Reincarnated Alexander

    [ScriptMethod(name: "————————  Boss2_Reincarnated Alexander  ————————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Boss2_Alexander(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Banish IV AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50161$"])]
    public void BanishIV(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "Sacred Arrow (Cone)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5012[45]$"])]
    public void SacredArrow(Event @event, ScriptAccessory accessory)
    {
        // Not left-right nor clockwise-counterclockwise, what is this??
        // Can't draw for now, leave placeholder
    }
    
    [ScriptMethod(name: "Judgment (Half Room)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^5014[67]$"])]
    public void Judgment(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Judgment";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.Radian = 180f.DegToRad(); 
        dp.Delay = @event.ActionId == 50146 ? 0 : 6800;
        dp.DestoryAt = @event.ActionId == 50146 ? 6700 : 2900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "Sacred Fire (Triangle but using placeholder)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50143$"])]
    public void SacredFire(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Sacred Fire";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "Mega Holy (Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50158$"])]
    public void MegaHoly(Event @event, ScriptAccessory accessory)
    {
        string targetName = @event["TargetName"]?.ToString();
        if (!string.IsNullOrEmpty(targetName))
        {
            if (isText)accessory.Method.TextInfo($"Group up near {targetName}", duration: 5300, true);
            if (isTTS) accessory.Method.TTS($"Group up near {targetName}");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"Group up near {targetName}");
        }
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"MegaHoly{@event.TargetId}";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 10200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Gordius System - Reflect Barrier", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50686$"])]
    public void ReflectBarrier(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"Stop attacking <Reflect Barrier>", duration: 3300, true);
        if (isTTS)accessory.Method.TTS($"Stop attacking Reflect Barrier");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Stop attacking Reflect Barrier");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Reflect Barrier{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 20000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Divine Judgment AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50153$"])]
    public void DivineJudgment(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"Large AOE damage", duration: 9000, true);
        if (isTTS)accessory.Method.TTS($"Large AOE damage");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Large AOE damage");
    }
    
    private static int _ElectrifyCount = 0;

    [ScriptMethod(name: "Gordius System - Support Command_Mega Discharge (Tether Chariot)", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^01AC$"])]
    public void SupportCommand_MegaDischarge(Event @event, ScriptAccessory accessory)
    {
        _ElectrifyCount++;
    
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Support Command{@event.SourceId}";
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
    
    #region Boss3_Promathia
    
    [ScriptMethod(name: "————————  Boss3_Promathia  ————————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Boss3_Promathia(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Divine Song AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50317$"])]
    public void DivineSong(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "Instant_Explosion (Moving Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50320$"])]
    public void InstantExplosion(Event @event, ScriptAccessory accessory)
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
    
    [ScriptMethod(name: "Illusion Ring (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50321$"])]
    public void IllusionRing(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Illusion Ring";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 12400;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Hades Feast_Elysian Haven (Dynamo)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50331$"])]
    public void ElysianHaven(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"Go behind and get close");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Go behind and get close");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Elysian Haven";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Void Sigh_Aurora Veil (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50355$"])]
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
    
    [ScriptMethod(name: "Void Thought_Nihil Wind (Rotating Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50354$"])]
    public void NihilWind(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Nihil Wind";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (6f, 16f);
        dp.DestoryAt = 300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "Void Seed Knockback", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50349$"],suppress:1000)]
    public void VoidSeed(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"Knockback to wall", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"Knockback to wall");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Knockback to wall");
    }
    
    [ScriptMethod(name: "Spirit AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50694$"])]
    public void Spirit(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"Large AOE + knockdown", duration: 9000, true);
        if (isTTS)accessory.Method.TTS($"Large AOE + knockdown");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Large AOE + knockdown");
    }
    
    [ScriptMethod(name: "Sun Revival (Half Room)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50329$"])]
    public void SunRevival(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Sun Revival";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (50f, 50f);
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "Hades Feast (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50332$"])]
    public void HadesFeast(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Hades Feast";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (5f, 50f);
        dp.DestoryAt = 7200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "Lure (Tower Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^50565$"])]
    public void Lure(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"Leave the spot");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Leave the spot");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Lure{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region Boss4_Shinryu
    
    [ScriptMethod(name: "————————  Boss4_Shinryu  ————————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Boss4_Shinryu(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Cosmic Breath (Upper Half)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49107$"])]
    public void CosmicBreath(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"↓ Go to lower platform ↓", duration: 6300, true);
        if (isTTS)accessory.Method.TTS($"Go to lower platform");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Go to lower platform");
        
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
        if (isText)accessory.Method.TextInfo($"↑ Go to upper platform ↑", duration: 6300, true);
        if (isTTS)accessory.Method.TTS($"Go to upper platform");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Go to upper platform");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Cosmic Tail";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (70f, 50f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "Dusk Shine_Dusk Radiance Debuff Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^5352$"])]
    public void DuskShine_DuskRadiance(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo($"Stay on the upper platform during Dusk Nova (Light)", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"Stay on the upper platform (Light)");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Stay on the upper platform (Light)");
    }
    
    [ScriptMethod(name: "Dusk Shade_Dusk Shadow Debuff Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^5353$"])]
    public void DuskShade_DuskShadow(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo($"Stay on the lower platform during Dusk Nova (Dark)", duration: 4300, true);
        if (isTTS)accessory.Method.TTS($"Stay on the lower platform (Dark)");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Stay on the lower platform (Dark)");
    }
    
    [ScriptMethod(name: "Atomic Tail (Lower Half)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49130$"])]
    public void AtomicTail(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo($"↑ Go to upper platform, platform destruction ↑", duration: 6300, true);
        if (isTTS)accessory.Method.TTS($"Go to upper platform");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Go to upper platform");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Atomic Tail";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (70f, 50f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    #endregion
    
    #region Boss4_Nihil King
    
    [ScriptMethod(name: "————————  Boss4_Nihil King  ————————", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Boss4_NihilKing(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Void Declaration AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^49179$"])]
    public void VoidDeclaration(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS($"Large AOE");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Large AOE");
    }
    
    [ScriptMethod(name: "Left/Right Alternating Sword", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4915[3-6]$"])]
    public void AlternatingSword(Event @event, ScriptAccessory accessory)
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
                dp.Name = "Right Alternating Sword_Diagonal";
                dp.Offset = new Vector3(15, 0, 15);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
                break;
            case 49154:
                dp.Name = "Left Alternating Sword_Diagonal";
                dp.Offset = new Vector3(-15, 0, 15);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
                break;
            case 49155:
                dp1.Name = "Left Alternating Sword_Straight";
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1); 
                break;
            case 49156:
                dp1.Name = "Right Alternating Sword_Straight";
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1); 
                break;
        }
        
    }
    
    [ScriptMethod(name: "Double Breath (Chariot Dynamo)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^491(59|60)$"])]
    public void DoubleBreath(Event @event, ScriptAccessory accessory)
    {
        switch (@event.ActionId())
        {
            case 49159:
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Double Breath_Dynamo";
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
                dp1.Name = $"Double Breath_Chariot";
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
        if (isTTS)accessory.Method.TTS($"Avoid the line");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Avoid the line");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Atomic Ray";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new (15f, 60f);
        dp.DestoryAt = 1200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    #endregion

    #region Drawing Destruction

    [ScriptMethod(name: "Cast Interrupt Destruction", eventType: EventTypeEnum.CancelAction, eventCondition: [], userControl: false)]
    public void CastInterruptDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Death Destruction", eventType: EventTypeEnum.Death, eventCondition: [], userControl: false)]
    public void DeathDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "Anti-Knockback Destruction", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(160|1209|2663)$"],userControl: false)]
    public void AntiKnockbackDestruction(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw($".*(knockback|pull).*");
    }
    
    [ScriptMethod(name: "Reflect Barrier Destruction", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:regex:^5377$"], userControl: false)]
    public void ReflectBarrierDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Reflect Barrier.*");
    }

    #endregion
}

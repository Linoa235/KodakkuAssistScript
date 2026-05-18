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
using System.Threading.Tasks;
using KodakkuAssist.Extensions;

namespace Thornmarch_Extreme;

[ScriptType(guid: "a25a701a-0a9a-4bbf-b00c-87e53ce797e6", name: "Thornmarch Extreme", territorys: [364],
    version: "0.0.0.4", Author: "Linoa235", note: noteStr)]

public class ThornmarchExtreme
{
    const string noteStr =
        """
        v0.0.0.3:
        LV50 Thornmarch Extreme Initial Drawing
        You can basically clear without reading guides. No role restrictions. Disable unnecessary hints yourself.
        """;
    
    // Missing mechanic: Moogle Cheer
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [UserSetting("[Dev] Debug Mode")]
    public bool isDebug { get; set; } = false;

    public static bool isTank;
    public static bool isDps;
    public static bool isHealer;
   
    public void Init(ScriptAccessory accessory)
    {
        var player = accessory.Data.MyObject;
        isTank = player?.IsTank() ?? false;
        isDps = player?.IsDps() ?? false;
        isHealer = player?.IsHealer() ?? false;
    }
    
    private volatile int timeMooglesseOblige = 0;
    public void MooglesseOblige(ScriptAccessory accessory) {
        timeMooglesseOblige = 0;
    }
    
    #region Instance Hints
    
    [ScriptMethod(name: "Opening Hint", eventType: EventTypeEnum.Director, eventCondition: ["Command:40000001"])]
    public async void OpeningHint(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        var isDps = accessory.Data.MyObject?.IsDps() ?? false;
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;

        if (isTank && isText) accessory.Method.TextInfo("Difficulty: â˜†, Key: Weaken adds and kill together\nT: Good King drains MP, DRK not recommended, watch cleave direction", duration: 5000, true);
        if (isDps && isText) accessory.Method.TextInfo("Difficulty: â˜†, Key: Weaken adds and kill together\nDPS: Manage add HP, first two need simultaneous kills", duration: 5000, true);
        if (isHealer && isText) accessory.Method.TextInfo("Difficulty: â˜†, Key: Weaken adds and kill together\nH: Esuna [Moogle-Mania] and [Wroth Flames] during combo moves, watch AOE", duration: 5000, true);
        
        if (isTTS) accessory.Method.TTS("Dodge what you see, manage add HP");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Dodge what you see, manage add HP");
        accessory.Method.SendChat("/e â€”â€”â€”â€”Cheat Sheetâ€”â€”â€”â€”\nT: MT pulls Axe & Good King, ST pulls Wall, watch cleave (Good King drains MP, DRK not recommended)\nDPS: Manage add HP, first two need simultaneous kills\nH: Esuna [Moogle-Mania] and [Wroth Flames] during combo moves, watch AOE");
    }
    
    [ScriptMethod(name: "Add Spawn Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2070"])]
    public void AddSpawnHint(Event @event, ScriptAccessory accessory)
    {        
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (!isTank) return; 
        if (isText) accessory.Method.TextInfo("MT pulls <Axe> & <Good King>, ST pulls <Wall>, all have cleaves\nAll adds need to be killed simultaneously twice", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Watch cleave direction, manage HP");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Watch cleave direction, manage HP");
        accessory.Method.SendChat("/e Pull Hint: MT pulls Axe & Good King, ST pulls Wall, all have cleaves\nAll adds need to be killed simultaneously twice, then AOE and enrage");
    }
    
    [ScriptMethod(name: "Mooglesse Oblige Count Reset", userControl: false, eventType: EventTypeEnum.Chat,
        eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^å®¶è‡£ä»¬ï¼Œ\nåˆ°ä½ ä»¬å‡ºé©¬çš„æ—¶å€™äº†åº“å•µï¼", "Sender:Good King Moggle Mog XII"])]
    public void Reset_MooglesseOblige(Event @event, ScriptAccessory accessory) {
        timeMooglesseOblige = 0;
    }
    
    [ScriptMethod(name: "Mooglesse Oblige Resurrection Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2069"])]
    public void MooglesseOblige(Event @event, ScriptAccessory accessory)
    {
        ++timeMooglesseOblige;
        switch (timeMooglesseOblige)
        {
            case 1:
            {
                if (isText) accessory.Method.TextInfo($"Resurrection #{timeMooglesseOblige}", duration: 2700, false);
                if (isTTS) accessory.Method.TTS($"Resurrection #{timeMooglesseOblige}");
                if (isEdgeTTS) accessory.Method.EdgeTTS($"Resurrection #{timeMooglesseOblige}");
                accessory.Method.SendChat($"/e Resurrection count: {timeMooglesseOblige}");
            }
                break;
            case 2:
            {
                if (isText) accessory.Method.TextInfo($"Resurrection #{timeMooglesseOblige}, massive AOE incoming\nThen after killing one add, enrage cast", duration: 2700, false);
                if (isTTS) accessory.Method.TTS($"Resurrection #{timeMooglesseOblige}, massive AOE incoming");
                if (isEdgeTTS) accessory.Method.EdgeTTS($"Resurrection #{timeMooglesseOblige}, massive AOE incoming");
                accessory.Method.SendChat($"/e Resurrection count: {timeMooglesseOblige}");
            }
                break;
            default:
            {
                accessory.Method.SendChat($"/e Count error! Current count: {timeMooglesseOblige}");
            }
                break;
        }
    }
    #endregion
    
    #region Add Basic Mechanics
    [ScriptMethod(name: "Fluffy Axe_Moogle Darkness (Cleave)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2052"])]
    public void MoogleDarkness(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Moogle Darkness";
        dp.Color = new Vector4(1f, 0f, 1f, 0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6.9f);
        dp.Radian = 120f.DegToRad();
        dp.Delay = 8000;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Fluffy Wall_Moogle Light (Cleave)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2054"])]
    public void MoogleLight(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Moogle Light";
        dp.Color = new Vector4(1f, 1f, 1f, 0.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6.9f);
        dp.Radian = 120f.DegToRad();
        dp.Delay = 8000;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Soft Sound_Moogle March (Buff Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1623"])]
    public void MoogleMarch(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Pull other moogles out of the buff circle", duration: 5200, false);
        if (isTTS) accessory.Method.TTS("Pull other moogles out of the buff circle");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Pull other moogles out of the buff circle");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Moogle March";
        dp.Color = new Vector4(0f, 1f, 1f, 0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Fluffy Flare (Large Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2055"])]
    public void FluffyFlare(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fluffy Flare";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(20.9f);
        dp.DestoryAt = 7700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Fluffy Flare Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:2055"], userControl: false)]
    public void FluffyFlareCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Fluffy Flare");
    }
    
    [ScriptMethod(name: "Moogle Trueflight Marker Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:1635"])]
    public void MoogleTrueflight(Event @event, ScriptAccessory accessory)
    {
        string tname = @event["TargetName"]?.ToString() ?? "Unknown Target";

        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isTTS) accessory.Method.TTS("Quadruple thrust marker");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Quadruple thrust marker");
            accessory.Method.SendChat("/e Quadruple thrust marker");
        } else
        {
            if (isTTS) accessory.Method.TTS($"Quadruple attack on {tname}");
            if (isEdgeTTS) accessory.Method.EdgeTTS($"Quadruple attack on {tname}");
            accessory.Method.SendChat($"/e Quadruple attack on <{@event.TargetName()}>");
        }
    }
    #endregion
    
    #region Combo Phase
    [ScriptMethod(name: "Fluffy Meteor (Circle AOE)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2056"])]
    public void FluffyMeteor(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fluffy Meteor";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Moogle Triangle Attack", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:2114"])]
    public void MoogleTriangleAttack(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Move away from center, stay near arena edge to avoid large triangle", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Move away from center");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Move away from center");
    }
    
    [ScriptMethod(name: "Moogle-Mania Esuna Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:473"])]
    public void MoogleMania(Event @event, ScriptAccessory accessory)
    {
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (!isHealer) return; 
        if (isText) accessory.Method.TextInfo("Esuna <Moogle-Mania>", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Esuna <Moogle-Mania>");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Esuna <Moogle-Mania>");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Moogle-Mania{@event.TargetId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new(1f);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Moogle-Mania Esuna Cleanup", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:473"], userControl: false)]
    public void MoogleManiaCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Moogle-Mania{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "Fluffy Heal_Fluffy Holy Interrupt Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2059"])]
    public void FluffyHoly(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Attack <Fluffy Heal> to interrupt <Fluffy Holy>", duration: 3000, true);
        if (isTTS) accessory.Method.TTS("Attack <Fluffy Heal> to interrupt");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Attack <Fluffy Heal> to interrupt");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fluffy Holy";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Fluffy Holy Interrupt Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:2059"], userControl: false)]
    public void FluffyHolyCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Fluffy Holy");
        accessory.Method.SendChat("/e Fluffy Holy interrupted");
    }
    
    [ScriptMethod(name: "Bring It On Kupo Move Away Hint", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:2108"])]
    public void BringItOnKupo(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Move away from <Fluffy Barrier> and <Fluffy Wall>", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Move away from tethered targets");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Move away from tethered targets");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fluffy Wall Tether";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.TargetObject = accessory.Data.Me;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Scale = new(1);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Wroth Flames Esuna Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:402"])]
    public void WrothFlames(Event @event, ScriptAccessory accessory)
    {
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (!isHealer) return; 
        if (isText) accessory.Method.TextInfo("Esuna <Wroth Flames>", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Esuna <Wroth Flames>");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Esuna <Wroth Flames>");
            
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Wroth Flames{@event.TargetId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new(1f);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        accessory.Method.RemoveDraw("(Fluffy Wall|Fluffy Barrier) Tether");
    }
    
    [ScriptMethod(name: "Wroth Flames Esuna Cleanup", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:402"], userControl: false)]
    public void WrothFlamesCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Wroth Flames{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "Moogle Death Rain (Marker Circle)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:1636"])]
    public void MoogleDeathRain(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Moogle Death Rain";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    #endregion
    
    #region Enrage Phase
    [ScriptMethod(name: "Moogle Meteor (AOE)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2072", "SourceDataId:236"])]
    public void MoogleMeteor(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Large AOE damage", duration: 4700, true);
        if (isTTS) accessory.Method.TTS("Large AOE damage");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Large AOE damage");
    }
    
    [ScriptMethod(name: "Death Moogle Warning Enrage Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2121"])]
    public void DeathMoogleWarning(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Attack the Good King, enrage", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Focus attack on Good King");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Focus attack on Good King");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Death Moogle Warning";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 1500;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    #endregion
}

// EventExtensions, EnumExtensions, MathTools, DirectionCalc, etc. are identical to previous files
// and are omitted here for brevity. They follow the same pattern as the other translated files.
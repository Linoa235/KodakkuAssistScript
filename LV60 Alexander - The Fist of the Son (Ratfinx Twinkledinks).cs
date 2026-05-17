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

namespace A5N;

[ScriptType(guid: "1462516c-2bc2-4560-8244-387845cc098d", name: "A5N", territorys: [520],
    version: "0.0.0.2", Author: "Linoa235", note: noteStr)]

public class A5N
{
    const string noteStr =
        """
        v0.0.0.1:
        LV60 Alexander - The Fist of the Son (Ratfinx Twinkledinks) Initial Drawing
        """;
    
    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "Full-power Punch Tank Swap Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:995", "StackCount:3"])]
    public void FullPowerPunchTank(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (@event.TargetId() != accessory.Data.Me)
        {
            if (isTank && isText) accessory.Method.TextInfo("Provoke, line tankbuster incoming", duration: 10500, true);
            if (isTank && isTTS) accessory.Method.TTS("Provoke boss"); 
            if (isTank && isEdgeTTS) accessory.Method.EdgeTTS("Provoke boss");
        }
        else
        {
            if (isTank && isText) accessory.Method.TextInfo("Use heavy mitigation or swap", duration: 10500, true); 
            if (isTank && isTTS) accessory.Method.TTS("Continuous tankbuster"); 
            if (isTank && isEdgeTTS) accessory.Method.EdgeTTS("Continuous tankbuster");
        }
    }
    
    [ScriptMethod(name: "Full-power Punch Mitigation Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:996"])]
    public void FullPowerPunchMitigation(Event @event, ScriptAccessory accessory)
    {
        var boss = accessory.Data.Objects.GetByDataId(5353).FirstOrDefault();
        if (boss == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Full-power Punch";
        dp.Scale = new (4, 45.2f);
        dp.Owner = boss.GameObjectId;
        dp.TargetObject = @event.TargetId();
        dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.TargetOrderIndex = 1;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
        
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (!isTank && isTTS) accessory.Method.TTS("Tank line tankbuster, use mitigation"); 
        if (!isTank && isEdgeTTS) accessory.Method.EdgeTTS("Tank line tankbuster, use mitigation");
    }
    
    [ScriptMethod(name: "Full-power Punch Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:5526"], userControl: false)]
    public void FullPowerPunchCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Full-power Punch");
    }
    
    [ScriptMethod(name: "Strong Acid Poison Poison Pool Marker", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"])]
    public void StrongAcidPoison(Event @event, ScriptAccessory accessory)
    {
        if (isTTS && @event.TargetId() == accessory.Data.Me) accessory.Method.TTS("Move away and place poison pool"); 
        if (isEdgeTTS && @event.TargetId() == accessory.Data.Me) accessory.Method.EdgeTTS("Move away and place poison pool"); 
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Strong Acid Poison";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Bomb Deployment (Red Poison Hint)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5520"])]
    public void BombDeployment(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank)
        {
            if (isText) accessory.Method.TextInfo("Pull boss to the corner, avoid bombs", duration: 19000, true);
            if (isTTS) accessory.Method.TTS("Pull boss to the corner");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Pull boss to the corner");
        }
        else
        {
            if (isText) accessory.Method.TextInfo("Activate device, get red poison, become a gorilla and push bombs to opposite corner", duration: 19000, true);
            if (isTTS) accessory.Method.TTS("Activate device, get red poison, become gorilla, push bombs");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Activate device, get red poison, become gorilla, push bombs");
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Red Poison Tether";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Position = new Vector3(0f, -10f, -190f);
            dp.TargetPosition = new Vector3(16f, -10f, -190f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Scale = new(1);
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
    }
    
    [ScriptMethod(name: "Super Bomb_Grand Explosion", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:5354"])]
    public void GrandExplosion(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Grand Explosion";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new(35);
        dp.DestoryAt = 16000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = "Grand Explosion Outline";
        dp2.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp2.Owner = @event.SourceId();
        dp2.Scale = new Vector2(35.06f);
        dp2.InnerScale = new Vector2(35f);
        dp2.Radian = float.Pi * 2;
        dp2.DestoryAt = 16000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
    }

    [ScriptMethod(name: "Grand Explosion Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:5521"], userControl: false)]
    public void GrandExplosionCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Grand Explosion.*");
    }

    [ScriptMethod(name: "Charge (Purple Poison Hint)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5522"])]
    public void Charge(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Activate device, get purple poison to dodge 4 AOEs", duration: 15400, true);
        if (isTTS) accessory.Method.TTS("Get purple poison to dodge 4 AOEs");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Get purple poison to dodge 4 AOEs");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Purple Poison Tether";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = new Vector3(-16f, -10f, -190f);
        dp.Scale = new(1);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    uint vulture = 0;
    
    public void Init(ScriptAccessory accessory) {
        vulture = 0; 
    }
    
    [ScriptMethod(name: "Purple Poison Tether Cleanup", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:999"], userControl: false)]
    public void PurplePoisonCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me) accessory.Method.RemoveDraw("Purple Poison Tether");
        if (@event.TargetId() == accessory.Data.Me) vulture = 1;
    }
    
    [ScriptMethod(name: "Discharge Agent Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:5531"])]
    public void DischargeAgent(Event @event, ScriptAccessory accessory)
    {
        if (vulture == 1 && isTTS) accessory.Method.TTS("Discharge agent");
        if (vulture == 1 && isEdgeTTS) accessory.Method.EdgeTTS("Discharge agent");
    }
    
    [ScriptMethod(name: "Agent Discharge Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:5476"], userControl: false)]
    public void AgentDischargeCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me) vulture = 0; 
    }
}

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

    public static uint StatusId(this Event @event)
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
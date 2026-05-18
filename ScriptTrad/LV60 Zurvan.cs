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
using KodakkuAssist.Extensions;
using KodakkuAssist.Data;
using System.Threading.Tasks;

namespace Zurvan;

[ScriptType(guid: "591b862d-7978-4b25-9843-8dfef00177d4", name: "Zurvan", territorys: [637],
    version: "0.0.0.5", author: "Linoa235", note: noteStr)]

public class Zurvan
{
    const string noteStr =
        """
        v0.0.0.4:
        LV60 Zurvan Initial Drawing
        """;
        
    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "Wave Cannon", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7741"])]
    public void WaveCannon(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Wave Cannon";
        dp.Scale = new (10, 55.3f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Flight_Multiple Aspects", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7714"])]
    public void MultipleAspects(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Multiple Aspects";
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0.4f, 0f, 0.4f);
        dp.Scale = new(10f, 88f); 
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "Demon Claw (Stack)", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:003E"])]
    public void DemonClaw(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Demon Claw";
        dp.Owner = @event.TargetId();
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Scale = new Vector2(7);
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp); 
    }
    
    [ScriptMethod(name: "Fire and Ice Ice Circle", eventType: EventTypeEnum.Chat, 
        eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^(æˆ‘çš„å¹æ¯å°†åŒ–ä¸ºå¯’å†°.*|By sorrow's chill doth all become ice.*|æˆ‘ãŒå˜†ãã¯ã€æ°·ã¨ãªã£ã¦å‡ã¦ä»˜ã.*)$"])]
    public void FireAndIce(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Don't stand in the middle", duration: 4000, true);
        if (isTTS) accessory.Method.TTS("Don't stand in the middle");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Don't stand in the middle");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fire and Ice";
        dp.Color = new Vector4(1f, 1f, 0f, 1.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5.3f);
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Add Spawn Position", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:7726"])]
    public void AddSpawnPosition(Event @event, ScriptAccessory accessory)
    {
        var player = accessory.Data.MyObject;
        if (isText) accessory.Method.TextInfo("Adds will spawn at true north", duration: 3700, false);
        if (isTTS) accessory.Method.TTS("Adds will spawn at true north");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Adds will spawn at true north");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Add Spawn Position";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Position = new Vector3(0f, 0f, -11f);
        dp.Scale = new Vector2(2.1f);
        dp.DestoryAt = 4400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = "Pull Guide Line";
        dp2.Owner = accessory.Data.Me;
        dp2.Color = accessory.Data.DefaultSafeColor;
        dp2.ScaleMode |= ScaleMode.YByDistance;
        dp2.TargetPosition = new Vector3(0f, 0f, -11f);
        dp2.Scale = new(1);
        dp2.DestoryAt = 4400;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
    }
    
    [ScriptMethod(name: "Wise Servant Attack Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6554"])]
    public void WiseServant(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank) return; 
        if (isText) accessory.Method.TextInfo("Attack the Wise Servant", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Attack the Wise Servant");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Attack the Wise Servant");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Wise Servant Tether";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 20500;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Wise Servant Tether Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:7731"], userControl: false)]
    public void WiseServantTetherCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Wise Servant Tether");
    }
    
    uint MyFire = 0;
    uint MyIce = 0;
    
    public void Init(ScriptAccessory accessory) {
        MyFire = 0;
        MyIce = 0; 
    }
    
    [ScriptMethod(name: "Ice-Fire Brand Buff Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^114[34]$"], userControl: false)]
    public void IceFireBrandRecord(Event @event, ScriptAccessory accessory) 
    {
        if (@event.TargetId() != accessory.Data.Me) return;  
        switch (@event.StatusID())
        {
            case 1143:
                MyFire = 1;
                break;
            case 1144:
                MyIce = 1;
                break;
        }
    }
    
    [ScriptMethod(name: "Ice-Fire Brand Tower Soak Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7776"])]
    public void IceFireBrand(Event @event, ScriptAccessory accessory)
    {
        if (MyFire == 1)
        {
            if (isText) accessory.Method.TextInfo("Soak fire tower", duration: 14000, true);
            if (isTTS) accessory.Method.TTS("Soak fire tower");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Soak fire tower");
        }

        if (MyIce == 1)
        {
            if (isText) accessory.Method.TextInfo("Soak ice tower", duration: 14000, false);
            if (isTTS) accessory.Method.TTS("Soak ice tower");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Soak ice tower");
        }
    }
    
    [ScriptMethod(name: "Ice-Fire Brand Buff Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^777[235]$"], userControl: false)]
    public void IceFireBrandCleanup(Event @event, ScriptAccessory accessory)
    {
        MyFire = 0;
        MyIce = 0;
    }
    
    [ScriptMethod(name: "Ice-Fire Brand Buff Reset", userControl: false, eventType: EventTypeEnum.Chat, 
        eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^(åˆ»å°å‘åŠ¨.*|Graven in flesh, the brand is awoken.*|è‚‰ä½“ã«åˆ»ã¾ã‚Œã—ã€åˆ»å°ã‚’ç™ºå‹•ã™ã‚‹.*)$"])]
    public void IceFireBrandReset(Event @event, ScriptAccessory accessory)
    {
        MyFire = 0;
        MyIce = 0;
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

    public static uint StatusID(this Event @event)
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
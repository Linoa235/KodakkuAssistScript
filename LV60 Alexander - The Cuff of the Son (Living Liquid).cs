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

namespace A3N;

[ScriptType(guid: "69712d1f-26c4-441f-97f0-e63f8baf98a6", name: "A3N", territorys: [444],
    version: "0.0.0.3", Author: "Linoa235", note: noteStr)]

public class A3N
{
    const string noteStr =
        """
        v0.0.0.2:
        LV60 Alexander - The Cuff of the Son (Living Liquid) Initial Drawing
        """;
    
    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "Rinse Circle Marker", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:001A"])]
    public void Rinse(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Rinse";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.4f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Flush Center Knockback", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4863"])]
    public void Flush(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Flush Knockback Tether";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = new Vector3(58f, -9f, -63f);
        dp.Scale = new(1);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Anti-Knockback Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"], userControl: false)]
    public void AntiKnockbackCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("Flush.*");
    }

    uint MyMagnetism = 0;
    uint PartnerMagnetism = 0;
    
    public void Init(ScriptAccessory accessory) {
        MyMagnetism = 0;
        PartnerMagnetism = 0;
    }
    
    [ScriptMethod(name: "Magnetism Buff Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^69[89]$"], userControl: false)]
    public void MagnetismRecord(Event @event, ScriptAccessory accessory)
    {         
        if (@event.TargetId() == accessory.Data.Me)
            switch (@event.StatusID())
            {
                case 698:
                    MyMagnetism = 1;
                    break;
                case 699:
                    MyMagnetism = 2;
                    break;
            }
        
        if (@event.TargetId() != accessory.Data.Me)
            switch (@event.StatusID())
            {
                case 698:
                    PartnerMagnetism = 1;
                    break;
                case 699:
                    PartnerMagnetism = 2;
                    break;
            }
    }
    
    [ScriptMethod(name: "Magnetism Tether Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4858"])]
    public void MagnetismHint(Event @event, ScriptAccessory accessory)
    {
        if (MyMagnetism == 0) return;  
        if (MyMagnetism == PartnerMagnetism)
        {
            if (isText) accessory.Method.TextInfo("Approach tethered partner, knockback to safe zone", duration: 5000, true);
            if (isTTS) accessory.Method.TTS("Approach tethered partner, knockback to safe zone");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Approach tethered partner, knockback to safe zone");
        }
        else
        {
            if (isText) accessory.Method.TextInfo("Move away from tethered partner, pull to safe zone", duration: 5000, true);
            if (isTTS) accessory.Method.TTS("Move away from tethered partner, pull to safe zone");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Move away from tethered partner, pull to safe zone");
        }
    }
    
    [ScriptMethod(name: "Magnetism Buff Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:4871"], userControl: false)]
    public void MagnetismCleanup(Event @event, ScriptAccessory accessory)
    {
        MyMagnetism = 0;
        PartnerMagnetism = 0;
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
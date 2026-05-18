using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.Draw;
using Dalamud.Utility.Numerics;
using System.Numerics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Meva.Heavensward.KodakkuAssist.Alexander;

[ScriptType(name: "LV.60 Alexander - The Burden of the Father", territorys: [583], guid: "11e579e2-b47f-4995-b5ea-2987fc9102a8", version: "0.0.0.4", author: "Linoa235", note:noteStr)]
public class A12N
{
    const string noteStr =
        """
        v0.0.0.4:
        1. Now supports text banner/TTS toggle/DR TTS toggle (in user settings) (Make sure DailyRoutines plugin is properly installed before using DR TTS toggle) (Do not enable both TTS toggles at the same time)
        """;
    
    [UserSetting("Text banner toggle")]
    public bool isText { get; set; } = true;
    [UserSetting("TTS toggle")]
    public bool isTTS { get; set; } = false;
    [UserSetting("DR TTS toggle")]
    public bool isDRTTS { get; set; } = true;

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }

    [ScriptMethod(name: "Mega Holy", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6887"])]
    public void MegaHoly(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 2000, true);
        accessory.TTS("AOE", isTTS, isDRTTS);
    }
    
    // Holy Cross 1
    [ScriptMethod(name: "Holy Cross 1", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6885"])]
    public void HolyCross1(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Avoid cross lasers", duration: 2000, true);
        accessory.TTS("Avoid cross lasers", isTTS, isDRTTS);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Holy Cross 1";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(16.0f, 60.0f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }

    // Holy Cross 2 (rotated 90 degrees)
    [ScriptMethod(name: "Holy Cross 2", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6885"])]
    public void HolyCross2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Holy Cross 2";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new(16.0f, 60.0f);
        dp.Rotation = float.Pi / 2;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }

    [ScriptMethod(name: "Gravity Anomaly", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6891"])]
    public void GravityAnomaly(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Move away from expanding dark circle", duration: 2000, true);
        accessory.TTS("Move away from expanding dark circle", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "White Light Whip", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:562"])]
    public void WhiteLightWhip(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Spread", duration: 2000, true);
        accessory.TTS("Spread", isTTS, isDRTTS);
        
        var circleDp = accessory.Data.GetDefaultDrawProperties();
        circleDp.Name = "White Light Whip Area";
        circleDp.Owner = @event.TargetId();
        circleDp.Color = accessory.Data.DefaultDangerColor;
        circleDp.Scale = new(4);
        circleDp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, circleDp);
    }

    [ScriptMethod(name: "Collective Sin", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1122"])]
    public void CollectiveSin(Event @event, ScriptAccessory accessory)
    {
        string tname = @event["TargetName"]?.ToString() ?? "Unknown target";
        
        accessory.Method.TextInfo($"Stack with {tname}", 2000);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Stack Area";
        dp.Owner = @event.TargetId();
        dp.Color = accessory.Data.DefaultSafeColor; 
        dp.Scale = new(4);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Aggravated Sin", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1121"])]
    public void AggravatedSin(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Spread", 2000);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Aggravated Sin Area";
        dp.Owner = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new(2.5f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Holy Communion", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:6908"])]
    public void HolyCommunion(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Place circle, tether mark", duration: 4000, true);
        accessory.TTS("Place circle, tether mark", isTTS, isDRTTS);
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

    public static string DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<string>(@event["DurationMilliseconds"]) ?? string.Empty;
    }

    public static float SourceRotation(this Event @event)
    {
        return float.TryParse(@event["SourceRotation"], out var rot) ? rot : 0;
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
        return JsonConvert.DeserializeObject<string>(@event["SourceName"]) ?? string.Empty;
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
}

public static class Extensions
{
    public static void TTS(this ScriptAccessory accessory, string text, bool isTTS, bool isDRTTS)
    {
        if (isDRTTS)
        {
            accessory.Method.SendChat($"/pdr tts {text}");
        }
        else if (isTTS)
        {
            accessory.Method.TTS(text);
        }
    }
}
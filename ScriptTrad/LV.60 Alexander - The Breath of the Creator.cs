using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.Draw;
using Dalamud.Utility.Numerics;
using System.Numerics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;

namespace Meva.Heavensward.KodakkuAssist.Alexander;

[ScriptType(name: "LV.60 Alexander - The Breath of the Creator", territorys: [582], guid: "0cc640e0-bc4c-4d99-8da5-79f76f9ec600", version: "0.0.0.4", Author: "Linoa235", note:noteStr)]
public class A11N
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

    #region Adds
    [ScriptMethod(name: "Napalm Heavy", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6850"])]
    public void NapalmHeavy(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Heavy", duration: 2000, true);
        accessory.TTS("Heavy", isTTS, isDRTTS);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Napalm Heavy Area";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(8.5f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Napalm Donut", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6851"])]
    public void NapalmDonut(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Donut", duration: 2000, true);
        accessory.TTS("Donut", isTTS, isDRTTS);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Napalm Donut";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new Vector2(12);
        dp.InnerScale = new Vector2(3);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    #endregion

    #region Boss
    [ScriptMethod(name: "Hundredfold Blast", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6745"])]
    public void HundredfoldBlast(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Nuclear explosion mark", duration: 4000, true);
        accessory.TTS("Nuclear explosion mark", isTTS, isDRTTS);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Hundredfold Blast";
        dp.Owner = @event.TargetId();
    }
    
    [ScriptMethod(name: "Dark Fate", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6681"])]
    public void DarkFate(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Bleeding AOE", duration: 4000, true);
        accessory.TTS("Bleeding AOE", isTTS, isDRTTS);
    }
    
    [ScriptMethod(name: "Devastator Impact", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(6751|6787)$"])]
    public void DevastatorImpact(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Move away from line mark", duration: 2000, true);
        accessory.TTS("Move away from line mark", isTTS, isDRTTS);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Devastator Impact";
        dp.Scale = new(6,50);
        dp.TargetObject = @event.TargetId();
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Propeller Wind", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(6744|6773)$"])]
    public void PropellerWind(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Hide behind device", duration: 3000);
        accessory.TTS("Hide behind device", isTTS, isDRTTS);
    }
    
    [ScriptMethod(name: "Shield Prompt", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataID:6101"])]
    public void ShieldPrompt(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Destroy shield from the front", duration: 4000);
        accessory.TTS("Destroy shield from the front", isTTS, isDRTTS);
    }

    #endregion
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
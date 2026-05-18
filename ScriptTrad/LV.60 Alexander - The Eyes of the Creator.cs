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

[ScriptType(name: "LV.60 Alexander - The Eyes of the Creator", territorys: [580], guid: "edec2426-e729-4673-a929-e5ef6832b490", version: "0.0.0.3", author: "Linoa235", note: noteStr)]
public class A9N
{
    const string noteStr =
        """
        v0.0.0.3
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

    [ScriptMethod(name: "Scrap Burst", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6926"])]
    public void ScrapBurst(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Hide behind the rock!", duration: 2000);
        accessory.TTS("Hide behind the rock", isTTS, isDRTTS);
    }
    
    [ScriptMethod(name: "Kill Add Prompt", eventType: EventTypeEnum.ActionEffect, eventCondition: ["DataId:6922"])]
    public void KillAddPrompt(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Pull into glowing floor to kill", duration: 2000);
    }

    [ScriptMethod(name: "Kill Large Add Prompt", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6354"])]
    public void KillLargeAddPrompt(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Kill on opposite corner of glowing floor", duration: 4000);
        accessory.TTS("Kill on opposite corner of glowing floor", isTTS, isDRTTS);
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
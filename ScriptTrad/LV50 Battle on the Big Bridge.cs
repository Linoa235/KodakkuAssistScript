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

namespace BattleOnTheBigBridgen;

[ScriptType(guid: "c4cc856a-3a29-4d58-87e6-684ad478fd33", name: "Battle on the Big Bridge", territorys: [366],
    version: "0.0.0.4", author: "Linoa235", note: noteStr)]

public class BattleOnTheBigBridge
{
    const string noteStr =
        """
        v0.0.0.3:
        LV50 Battle on the Big Bridge Initial Drawing
        Please choose one TTS option in User Settings, do not enable both.
        """;
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    
    [ScriptMethod(name: "Frog Song", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:439"])]
    public void FrogSong(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;

        if (isText) accessory.Method.TextInfo("Avoid the green chicken", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Avoid the green chicken");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Avoid the green chicken");

        foreach (var item in accessory.Data.Objects.GetByDataId(2824))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Enkidu";
            dp.Owner = item.EntityId;
            dp.Color = new Vector4(1f, 0f, 0f, 2f);
            dp.Scale = new Vector2(1.5f);
            dp.DestoryAt = 20000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "Frog Cleanup", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:439"], userControl: false)]
    public void FrogCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Enkidu");
    }
    
    [ScriptMethod(name: "Confusion Heal to Full Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:11"], suppress: (5000))]
    public void Confusion(Event @event, ScriptAccessory accessory)
    {
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        
        if (isHealer && isText) accessory.Method.TextInfo("Heal confused teammate to full", duration: 5000, false);
        if (isHealer && isTTS) accessory.Method.TTS("Heal confused teammate to full");
        if (isHealer && isEdgeTTS) accessory.Method.EdgeTTS("Heal confused teammate to full");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Confusion{@event.SourceId()}";
        dp.Owner = @event.TargetId();
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Scale = new Vector2(1.5f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp); 
    }
    
    [ScriptMethod(name: "Confusion Cleanup", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:11"], userControl: false)]
    public void ConfusionCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Confusion{@event.SourceId()}");
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
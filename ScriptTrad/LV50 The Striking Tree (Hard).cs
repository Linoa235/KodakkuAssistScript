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

namespace theStrikingTree_Hard;

[ScriptType(guid: "2166623a-b7d4-4bdc-b11e-71cbb235364f", name: "The Striking Tree (Hard)", territorys: [374],
    version: "0.0.0.3", author: "Linoa235", note: noteStr)]

public class theStrikingTree_Hard_
{
    const string noteStr =
        """
        v0.0.0.2:
        LV50 The Striking Tree (Hard) Initial Drawing
        """;
    
    [ScriptMethod(name: "Chaos Strike Marker Hint", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0001"])]
    public void ChaosStrike(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.TextInfo("Terror marker, stand behind the boss", duration: 2700, true);
        accessory.Method.TTS("Stand behind the boss");
    }
    
    [ScriptMethod(name: "Chaos Strike Terror Debuff Highlight", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:66"])]
    public void Terror(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Terror{@event.TargetId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 2f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(1.5f);
        dp.DestoryAt = 39000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Chaos Strike Terror Debuff Cleanup", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:66"], userControl: false)]
    public void TerrorCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Terror{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "Thunderstorm Marker Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2278"])]
    public void Thunderstorm(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.TextInfo("Go blast the feared player", duration: 3700, false);
        accessory.Method.TTS("Go blast the feared player");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Thunderstorm Outline";
        dp.Color = new Vector4(0f, 1f, 1f, 4f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5f);
        dp.InnerScale = new Vector2(4.95f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Arbiter's Shadow_Thunderbolt (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2282"])]
    public void Thunderbolt(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Thunderbolt{@event.SourceId()}";
        dp.Scale = new (4, 50f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Arbiter's Shadow_Thunderbolt Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:2282"], userControl: false)]
    public void ThunderboltCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Thunderbolt{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Karma Marker Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:514"])]
    public void Karma(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.TextInfo("Stop attacking, eat 3 orbs", duration: 5000, true);
        accessory.Method.TTS("Stop attacking, eat 3 orbs");
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

    public static string DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<string>(@event["DurationMilliseconds"]) ?? string.Empty;
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
using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using System.Reflection.Metadata;
using System.Net;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using System.Runtime.Intrinsics.Arm;
using System.Collections.Generic;
using System.ComponentModel;

namespace Veever.A_Realm_Reborn.MorethanaFeeler;

[ScriptType(name: "LV.30 More than a Feeler", territorys: [], $17f3cd8fe-65bc-40cf-8067-3dec4be570e6", version: "0.0.0.1", Author: "Linoa235", guid: "5b72893a-a34c-4d75-9c8e-5e9d22526769")]

public class More_than_a_Feeler
{
    const string noteStr =
    """
    v0.0.0.3:
    1. Now supports text banner/TTS toggle/DR TTS toggle (make sure you have correctly installed the `DailyRoutines` plugin before using DR TTS toggle) (do not enable both TTS toggles at the same time)
    2. Marker toggle and local toggle are in user settings, you can choose to turn them off or on (local enabled by default)
    Duckmen.
    """;
    [UserSetting("Text Banner Toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = false;

    [UserSetting("DR TTS Toggle")]
    public bool isDRTTS { get; set; } = true;

    [UserSetting("Marker Toggle")]
    public bool isMark { get; set; } = true;

    [UserSetting("Local Marker Toggle (ON = local only, OFF = party)")]
    public bool LocalMark { get; set; } = true;

    [UserSetting("Debug Toggle")]
    public bool isDebug { get; set; } = false;

    public int attackCount;

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        attackCount = 0;
    }

    [ScriptMethod(name: "Waypoint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:1126"])]
    public async void Navi(Event @event, ScriptAccessory accessory)
    {
        if (isDebug) accessory.Method.SendChat("/e im in Navi");
        await Task.Delay(1500);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Waypoint";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = long.MaxValue;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "Delete Waypoint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:872"])]
    public void delNavi(Event @event, ScriptAccessory accessory)
    {
        if (isDebug) accessory.Method.SendChat("/e im in delNavi");
        if (attackCount == 1)
        {
            accessory.Method.RemoveDraw("Waypoint");
            if (isText) accessory.Method.TextInfo("Focus attack on Tatatastro Tailor", duration: 6000, true);
            accessory.TTS("Focus attack on Tatatastro Tailor", isTTS, isDRTTS);
            if (isMark) accessory.Method.Mark(@event.SourceId(), KodakkuAssist.Module.GameOperate.MarkType.Attack1, LocalMark);
        }
        attackCount++;
    }

    [ScriptMethod(name: "Bad Breath", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:604"])]
    public void BadBreath(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "BadBreath";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15);
        dp.Radian = float.Pi / 180 * 120;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Stale Bubble", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:1027"])]
    public void stalebubble(Event @event, ScriptAccessory accessory)
    {
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "stalebubble";
        dp1.Color = new Vector4(255 / 255.0f, 0 / 255.0f, 0 / 225.0f, 1f);
        dp1.Owner = @event.SourceId();
        dp1.ScaleMode = ScaleMode.ByTime;
        dp1.Scale = new Vector2(0.5f);
        dp1.DestoryAt = long.MaxValue;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp1);
    }

    [ScriptMethod(name: "Delete Stale Bubble", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:1027"])]
    public void Delstalebubble(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"{@event.SourceId()}");
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

    public static uint DataId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["DataId"]);
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
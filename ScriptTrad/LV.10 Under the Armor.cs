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

namespace Veever.A_Realm_Reborn.UndertheArmor;

[ScriptType(name: "LV.10 Under the Armor", territorys: [190], guid: "25fb9aae-bc9a-484c-afd9-0f8639f9e644",
    version: "0.0.0.5", Author: "Linoa235", note: noteStr)]

public class Under_the_Armor
{
    const string noteStr =
    """
    v0.0.0.5:
    1. Now supports banner text/TTS/DR TTS (ensure DailyRoutines plugin is properly installed before using DR TTS). Do not enable both TTS options simultaneously.
    2. Added a switch. There's no TTS in this dungeon anyway, don't look.jpg
    Duckmen.
    """;
    
    [UserSetting("Banner Text Toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = false;

    [UserSetting("DR TTS Toggle")]
    public bool isDRTTS { get; set; } = true;

    public int attackCount;

    public void Init(ScriptAccessory accessory)
    {
        attackCount = 0;
    }

    [ScriptMethod(name: "Guidance", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:515"])]
    public void Navi(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Guidance";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = @event.SourcePosition();
        dp.Scale = new(1);
        dp.DestoryAt = long.MaxValue;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "Delete Guidance", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:870"])]
    public void delNavi(Event @event, ScriptAccessory accessory)
    {
        if (attackCount == 1)
        {
            accessory.Method.RemoveDraw(".*");
        }
        attackCount++;
    }

    [ScriptMethod(name: "Iron Justice", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:356"])]
    public void IronJustice(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.SendChat("/ac \"Low Blow\"");
        accessory.Method.SendChat("/ac ä¸‹è¸¢");
        accessory.Method.SendChat("/ac \"Low Blow\"");
        accessory.Method.SendChat("/ac ä¸‹è¸¢");
        
        accessory.Method.SendChat("/ac \"Leg Sweep\"");
        accessory.Method.SendChat("/ac æ‰«è…¿");
        accessory.Method.SendChat("/ac \"Leg Sweep\"");
        accessory.Method.SendChat("/ac æ‰«è…¿");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Iron Justice";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10);
        dp.Radian = float.Pi / 180 * 120;
        dp.DestoryAt = 2500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
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

    public static uint SourceRotation(this Event @event)
    {
        return ParseHexId(@event["SourceRotation"], out var sourceRotation) ? sourceRotation : 0;
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
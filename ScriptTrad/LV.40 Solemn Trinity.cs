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
using System.Windows;

namespace Veever.A_Realm_Reborn.SolemnTrinity;

[ScriptType(name: "LV.40 Solemn Trinity", territorys: [300], guid: "32e7ed24-f55c-48b0-8a40-3fa7442dd6df",
    version: "0.0.0.3", Author: "Linoa235", note: noteStr)]

public class Solemn_Trinity
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
    public int SleepCount;

    private readonly object SleepLock = new object();

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        attackCount = 0;
        SleepCount = 0;
    }

    [ScriptMethod(name: "Overpower", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:720"])]
    public void Overpower(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Overpower";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.9f);
        dp.Radian = float.Pi / 180 * 90;
        dp.DestoryAt = 2500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Boss Indication 0", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:1816"])]
    public async void BossNotify(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss Indication 0";
        //dp.Color = accessory.Data.DefaultDangerColor;
        dp.Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        dp.Owner = @event.SourceId();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(2f);
        dp.DestoryAt = long.MaxValue;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

        if (isText) accessory.Method.TextInfo("Attack Long Clios", duration: 5000, true);
        accessory.TTS("Attack Long Clios", isTTS, isDRTTS);
        if (isMark) accessory.Method.Mark(@event.SourceId(), KodakkuAssist.Module.GameOperate.MarkType.Attack1, LocalMark);
    }

    [ScriptMethod(name: "Delete Boss Indication 0", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:1816"])]
    public async void delBossNotify(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Boss Indication 0");
        accessory.Method.MarkClear();
    }

    [ScriptMethod(name: "Boss Indication 1", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:1824"])]
    public async void BossNotify1(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss Indication 1";
        //dp.Color = accessory.Data.DefaultDangerColor;
        dp.Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        dp.Owner = @event.SourceId();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(2f);
        dp.DestoryAt = long.MaxValue;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

        if (isText) accessory.Method.TextInfo("Prioritize attacking the marked boss", duration: 5000, true);
        accessory.TTS("Prioritize attacking the marked boss", isTTS, isDRTTS);
        if (isMark) accessory.Method.Mark(@event.SourceId(), KodakkuAssist.Module.GameOperate.MarkType.Attack1, LocalMark);
    }

    [ScriptMethod(name: "Boss Indication 2", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:1819"])]
    public async void BossNotify2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss Indication 2";
        //dp.Color = accessory.Data.DefaultDangerColor;
        dp.Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        dp.Owner = @event.SourceId();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(2f);
        dp.DestoryAt = long.MaxValue;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
        if (isMark) accessory.Method.Mark(@event.SourceId(), KodakkuAssist.Module.GameOperate.MarkType.Attack2, LocalMark);
    }

    [ScriptMethod(name: "Barbarous Scream", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:596"])]
    public async void BarbarousScream(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Barbarous Scream";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(5.3f);
        dp.DestoryAt = 2500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Colossal Slam", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:692"])]
    public void ColossalSlam(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Colossal Slam";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9f);
        dp.Radian = float.Pi / 180 * 120;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Bone Powder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:600"])]
    public async void BonePowder(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Bone Powder";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = 2500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Delete Draw", eventType: EventTypeEnum.Director, eventCondition: ["Command:40000002", "Instance:8003271E"])]
    public async void delDraw(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
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
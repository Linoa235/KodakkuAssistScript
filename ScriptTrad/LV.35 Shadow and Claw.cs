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

namespace Veever.A_Realm_Reborn.ShadowandClaw;

[ScriptType(name: "LV.35 Shadow and Claw", territorys: [223], guid: "8c9d477d-c344-4c57-9314-d543115a1c19",
    version: "0.0.0.4", author: "Linoa235", note: noteStr)]

public class Shadow_and_Claw
{
    const string noteStr =
    """
    v0.0.0.4:
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
    public int drawEyeDelay;

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        attackCount = 0;
        drawEyeDelay = 0;
    }

    [ScriptMethod(name: "Condemnation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:912"])]
    public void Condemnation(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Condemnation";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.3f);
        dp.Radian = float.Pi / 180 * 90;
        dp.DestoryAt = 2500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "notify", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:870", "SourceId:400111A2"])]
    public void notify(Event @event, ScriptAccessory accessory)
    {
        if (isDebug) accessory.Method.SendChat($"/e notifycount: {attackCount}");
        if (attackCount == 0)
        {
            if (isText) accessory.Method.TextInfo("Focus attack on Shadow Claw", duration: 2000, true);
            accessory.TTS("Focus attack on Shadow Claw", isTTS, isDRTTS);
            if (isMark) accessory.Method.Mark(@event.SourceId(), KodakkuAssist.Module.GameOperate.MarkType.Attack1, LocalMark);
        }
        attackCount++;
    }

    [ScriptMethod(name: "Shadow Eye Range", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:1802"])]
    public async void drawEye(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Do not attack Shadow Eye, be careful to move away from chariot", duration: 5000, true);
        accessory.TTS("Do not attack Shadow Eye, be careful to move away from chariot", isTTS, isDRTTS);

        for (int i = 0; i < 5; i++)
        {
            switch (i)
            {
                case 0:
                    drawEyeDelay = 7900;
                    break;
                case 1:
                    drawEyeDelay = 18100;
                    break;
                case 2:
                    drawEyeDelay = 28400;
                    break;
                case 3:
                    drawEyeDelay = 38700;
                    break;
                case 4:
                    drawEyeDelay = 49000;
                    break;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Shadow Eye Range {i}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId();
            dp.ScaleMode = ScaleMode.ByTime;
            dp.Scale = new Vector2(14f);
            dp.Delay = drawEyeDelay;
            dp.DestoryAt = 2500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"Shadow Eye Range {i} Outline";
            dp1.Scale = new(14f);
            dp1.InnerScale = new(13.9f);
            dp1.Radian = float.Pi * 2;
            dp1.Color = new Vector4(178 / 255.0f, 34 / 255.0f, 34 / 255.0f, 10.0f);
            dp1.Owner = @event.SourceId();
            dp1.Delay = drawEyeDelay;
            dp1.DestoryAt = 2500;
            dp1.Radian = 2 * float.Pi;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
        }
        if (!LocalMark) await Task.Delay(1000);
        if (isMark) accessory.Method.Mark(@event.SourceId(), KodakkuAssist.Module.GameOperate.MarkType.Stop1, LocalMark);
    }

    [ScriptMethod(name: "Triclip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:932"])]
    public void Triclip(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Triclip";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(4f, 5f);
        dp.DestoryAt = 2500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Curse", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(585|586)$"])]
    public async void Curse(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(5);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Curse Range";
        //dp.Color = accessory.Data.DefaultDangerColor;
        dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
        dp.Owner = @event.SourceId();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(3.8f);
        dp.DestoryAt = 2400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Delete Draw", eventType: EventTypeEnum.Director, eventCondition: ["Command:40000002", "Instance:8003271B"])]
    public async void delDraw(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }

    [ScriptMethod(name: "Frightful Roar", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:933"])]
    public async void FrightfulRoar(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Frightful Roar";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Delete Frightful Roar", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:933"])]
    public async void delFrightfulRoar(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Frightful Roar");
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
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

namespace Veever.EndWalker.theMinstrelsBalladEndsingersAria;

[ScriptType(name: "LV.90 The Minstrel's Ballad: Endsinger's Aria (Unrestricted, guid: "e5705363-5aea-4b18-8418-41eaa06f04eb")", territorys: [998], $1537b5e04-fd07-4e69-a5ef-08f3e76529dc",
    version: "0.0.0.6", Author: "Linoa235", note: noteStr)]

public class the_Minstrels_Ballad_Endsingers_Aria
{
    const string noteStr =
    """
    v0.0.0.6:
    1. Now supports banner text/TTS/DR TTS (ensure DailyRoutines plugin is properly installed before using DR TTS). Do not enable both TTS options simultaneously.
    2. The underlying extensions of these old scripts are too lazy to refactor (just adding whatever I can).
    Duckmen. 
    """;
    
    [UserSetting("Banner Text Toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = false;

    [UserSetting("DR TTS Toggle")]
    public bool isDRTTS { get; set; } = true;

    public int connectNotify = 0;

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");   
    }

    [ScriptMethod(name: "Grip of Despair", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28701"])]
    public void GripofDespair(Event @event, ScriptAccessory accessory)
    {
        List<Vector3> vectorList = new List<Vector3>
        {
            new Vector3(92.89f, 0.00f, 87.07f),
            new Vector3(92.96f, 0.00f, 95.95f),
            new Vector3(92.97f, 0.00f, 104.98f),
            new Vector3(93.06f, 0.00f, 113.93f),
            new Vector3(107.00f, 0.00f, 86.94f),
            new Vector3(107.13f, 0.00f, 96.03f),
            new Vector3(107.28f, 0.00f, 105.11f),
            new Vector3(107.03f, 0.00f, 114.14f),
        };

        var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

        if (index == -1) return;

        bool isLeftSide = index >= 0 && index <= 3;
        bool isRightSide = index >= 4 && index <= 7;

        if (isLeftSide || isRightSide)
        {
            string sideText = isLeftSide ? "Left" : "Right";
            if (isText) accessory.Method.TextInfo($"Stack in center, prepare to pull line to {sideText}", duration: 4700, true);
            if (isTTS) accessory.Method.TTS($"Stack in center, prepare to pull line to {sideText}");
            if (isDRTTS) accessory.Method.SendChat($"/pdr tts Stack in center, prepare to pull line to {sideText}");

            Vector3 pos = isLeftSide ? new Vector3(81.38f, 0.00f, 102.54f) : new Vector3(118.63f, 0.00f, 99.46f);
            Vector3 pos1 = vectorList[index];

            DrawDisplacement(accessory, "Line Tether Guidance 1", pos, 4700, 5000);
            DrawDisplacement(accessory, "Line Tether Guidance 2", pos1, 9700, 5000);
        }
    }

    private void DrawDisplacement(ScriptAccessory accessory, string name, Vector3 targetPosition, int delay, int destroyAt)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = targetPosition;
        dp.Scale = new Vector2(2);
        dp.Delay = delay;
        dp.DestoryAt = destroyAt;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "Elenchos Inside Danger", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28706"])]
    public void ElenchosInside(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Elenchos";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.TargetPosition = new Vector3(100f, 0f, 100f);
        dp.Scale = new Vector2(14, 40);
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Elenchos Outside Danger", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28705"])]
    public void ElenchosOutside(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Elenchos";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Position = new Vector3(100f, 0f, 80f);
        dp.TargetPosition = new Vector3(100f, 0f, 100f);
        dp.Scale = new Vector2(14, 40);
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Hubris", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28717"])]
    public void Hubris(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Double tankbuster", duration: 4700, true);
        if (isTTS) accessory.Method.TTS("Double tankbuster");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Double tankbuster");
    }

    [ScriptMethod(name: "AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(28718|28662)$"])]
    public void AOE(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 4700, true);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts AOE");
    }

    [ScriptMethod(name: "Eironeia", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28720"])]
    public void Eironeia(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Group stack", duration: 4700, true);
        if (isTTS) accessory.Method.TTS($"Group stack");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Group stack");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Eironeia (Stack)";
        dp.Color = new Vector4(0 / 255.0f, 255 / 255.0f, 255 / 255.0f, 1.0f);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Diairesis", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28668"])]
    public void Diairesis(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Sweeping Gouge";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20);
        dp.Radian = float.Pi / 180 * 180;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Blue Star Impact", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(286[67]7)$"])]
    public void BlueStar(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Knockback to safe position", duration: 4700, true);
        if (isTTS) accessory.Method.TTS("Knockback to safe position");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Knockback to safe position");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Blue Star Impact";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(5);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"Blue Star Knockback";
        dp1.Scale = new(1.5f, 23);
        dp1.Color = accessory.Data.DefaultSafeColor;
        dp1.Owner = accessory.Data.Me;
        dp1.TargetPosition = @event.TargetPosition();
        dp1.Rotation = float.Pi;
        dp1.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
    }

    [ScriptMethod(name: "Red Star Impact", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28666"])]
    public void RedStar(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Red Star Impact";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(30);
        dp.DestoryAt = 7000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
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
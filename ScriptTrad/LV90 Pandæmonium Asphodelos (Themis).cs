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

namespace P11n;

[ScriptType(name: "LV90 Pandæmonium Asphodelos (Themis, guid: "6127899c-cbc8-4e84-9683-9ddce8e0e4d9")", territorys: [], $102b2bde4-c6d7-499f-9420-c39b10180428", version: "0.0.0.1", Author: "Linoa235")]

public class P11n
{
    const string noteStr =
        """
        v0.0.0.1:
        LV90 PandÃ¦monium: Asphodelos (Themis) Initial Drawing
        """;
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "Diffraction-Dark Shockwave Two-Stage Expanding Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3320[78]$"])]
    public void DarkShockwave(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Dark Shockwave";
        dp.Color = new Vector4(1f, 0f, 1f, 0.6f);
        dp.Owner = @event.SourceId;
        dp.Scale = new (16, 46f);
        if (@event.ActionId == 33207)
        {
            dp.Offset = new Vector3(-16, 0, 0);
        }
        else
        {
            dp.Offset = new Vector3(16, 0, 0);
        }
        dp.Delay = 5900;
        dp.DestoryAt = 2600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "Diffraction-Light Burst Two-Stage Preview", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33206"])]
    public void LightBurstPreview(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Light Burst";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.25f);
        dp.Scale = new (26, 46f);
        dp.Owner = @event.SourceId;
        dp.DestoryAt = 8500;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "Diffraction-Light Burst Two-Stage Expanding Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33206"])]
    public void LightBurst(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Light Burst";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.3f);
        dp.Scale = new (26, 46f);
        dp.Owner = @event.SourceId;
        dp.Delay = 5900;
        dp.DestoryAt = 2600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "Light and Dark Sigil: Imbalance", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^355[56]$"])]
    public void LightAndDarkSigil(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (@event.StatusId == 3555 && isText) accessory.Method.TextInfo("Stand in dark area", duration: 5000, true);
        if (@event.StatusId == 3556 && isText) accessory.Method.TextInfo("Stand in light area", duration: 5000, false);
    }
    
    [ScriptMethod(name: "Overruled Knockback Prediction", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3327[45]$"])]
    public void Overruled(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Overruled";
        dp.Scale = new(1.2f, 11);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(8f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);
    }
    
    [ScriptMethod(name: "Anti-Knockback Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"], userControl: false)]
    public void AntiKnockbackCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("Overruled");
    }
    
    [ScriptMethod(name: "Overruled - Outer Dark (Knockback Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34548"])]
    public void OuterDark(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Outer Dark";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 9200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Overruled - Inner Light (Knockback Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34547"])]
    public void InnerLight(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Inner Light";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 9200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Sustainment - Dark Loop (Jumping Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34767"])]
    public void DarkLoop(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Dark Loop";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.Delay = 6500;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Sustainment - Stack", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:013E"])]
    public void Sustainment(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Sustainment";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 6300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Sustainment - Light Blast (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:34766"])]
    public void LightBlast(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Light Blast";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.Delay = 6500;
        dp.DestoryAt = 2500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Grudge (Continuous Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33243"])]
    public void Grudge(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Grudge";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 9500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Clone_Diffraction-Dark Shockwave Two-Stage Expanding Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3324[12]$"])]
    public void Clone_DarkShockwave(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Dark Shockwave";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.SourceId;
        dp.Scale = new (16, 46f);
        if (@event.ActionId == 33241)
        {
            dp.Offset = new Vector3(-16, 0, 0);
        }
        else
        {
            dp.Offset = new Vector3(16, 0, 0);
        }
        dp.Delay = 7900;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "Clone_Diffraction-Light Burst Two-Stage Preview", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33240"])]
    public void Clone_LightBurstPreview(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Light Burst";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.4f);
        dp.Scale = new (26, 46f);
        dp.Owner = @event.SourceId;
        dp.DestoryAt = 11100;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "Clone_Diffraction-Light Burst Two-Stage Expanding Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33240"])]
    public void Clone_LightBurst(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Light Burst";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (26, 46f);
        dp.Owner = @event.SourceId;
        dp.Delay = 7900;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
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
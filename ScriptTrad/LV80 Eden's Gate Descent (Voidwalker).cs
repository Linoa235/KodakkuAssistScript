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

namespace E2n;

[ScriptType(guid: "0bccb85f-fc25-40ec-8941-4b7a1aee30d5", name: "E2N", territorys: [850],
    version: "0.0.0.2", Author: "Linoa235", note: noteStr)]

public class E2n
{
    const string noteStr =
        """
        v0.0.0.1:
        LV80 Eden's Gate: Descent (Voidwalker) Initial Drawing
        """;
    
    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "Doomvoid Sever (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15931"])]
    public void DoomvoidSever(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Doomvoid Sever";
        dp.Scale = new (10, 50f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);  
    }
    
    [ScriptMethod(name: "Doomvoid Slicer (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15932"])]
    public void DoomvoidSlicer(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("Approach");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Doomvoid Slicer Tether";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Nyx Add Highlight", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:10509"])]
    public void Nyx(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Nyx";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor; 
        dp.Scale = new(6f, 6f); 
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = "Nyx Direction";
        dp2.Color = accessory.Data.DefaultDangerColor;
        dp2.Owner = @event.SourceId();
        dp2.Scale = new Vector2(1.5f, 3);
        dp2.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp2);
    }
    
    [ScriptMethod(name: "Nyx Cleanup", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:10509"], userControl: false)]
    public void NyxCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Nyx.*");
    }
    
    [ScriptMethod(name: "Delayed Spread - Dark Flare", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1810"])]
    public async void DarkFlare(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Dark Flare";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(8f);
        dp.Delay = @event.DurationMilliseconds() - 3000;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        await Task.Delay((int)@event.DurationMilliseconds() - 3000); 
        if (@event.TargetId() == accessory.Data.Me && isText) accessory.Method.TextInfo("Spread", duration: 2500, true);
    }
    
    [ScriptMethod(name: "Delayed Stack - Dark Holy", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1809"])]
    public void DarkHoly(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Dark Holy";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.Delay = @event.DurationMilliseconds() - 3000;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Delayed Look Away - Shadow Eye", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1812"])]
    public void ShadowEye(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Shadow Eye";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(1f);
        dp.Delay = @event.DurationMilliseconds() - 3000;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Empty Void Knockback Prediction", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15942"])]
    public void EmptyVoid(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Empty Void";
        dp.Scale = new(1.5f, 25);
        dp.Color = new Vector4(0f, 1f, 1f, 3f);
        dp.Owner = accessory.Data.Me;
        dp.Rotation = @event.SourceRotation();
        dp.FixRotation = true;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Anti-Knockback Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"], userControl: false)]
    public void AntiKnockbackCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("Empty Void");
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
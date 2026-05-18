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

namespace Nabriales;

[ScriptType(guid: "f9c383ef-da60-4e43-8b0a-7b662b286489", name: "Nabriales", territorys: [426],
    version: "0.0.0.4", author: "Linoa235", note: noteStr)]

public class Nabriales
{
    const string noteStr =
        """
        v0.0.0.3:
        LV50 Nabriales Initial Drawing
        """;
    
    [ScriptMethod(name: "Double", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3443"])]
    public void Double(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS("Double tankbuster");
    }
    
    [ScriptMethod(name: "Triple", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3444"])]
    public void Triple(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS("Triple tankbuster");
    }
    
    [ScriptMethod(name: "Detonation (AOE)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3437"])]
    public void Detonation(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "Touch Red Orb Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:3421"])]
    public void TouchRedOrbHint(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.EdgeTTS("Touch red orb");
    }
    
    [ScriptMethod(name: "Dark Aether I", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3651"])]
    public void DarkAetherI(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Dark Aether I{@event.SourceId()}";
        dp.Color = new Vector4(0f, 1f, 1f, 2);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.5f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Dark Aether Burst I", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3651"])]
    public void DarkAetherBurstI(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Dark Aether Burst I{@event.SourceId()}";
        dp.Color = new Vector4(0f, 1f, 1f, 0.3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Dark Aether I Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:3421"], userControl: false)]
    public void DarkAetherICleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Dark Aether I{@event.SourceId()}");
        accessory.Method.RemoveDraw($"Dark Aether Burst I{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Dark Aether II", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3652"])]
    public void DarkAetherII(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Dark Aether II{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 2);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.5f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Dark Aether Burst II", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3652"])]
    public void DarkAetherBurstII(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Dark Aether Burst II{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 0.3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(11f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Dark Aether II Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:3422"], userControl: false)]
    public void DarkAetherIICleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Dark Aether II{@event.SourceId()}");
        accessory.Method.RemoveDraw($"Dark Aether Burst II{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Touch Blue Orb Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:3423"])]
    public void TouchBlueOrbHint(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.EdgeTTS("Touch blue orb");
    }
    
    [ScriptMethod(name: "Dark Aether III", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3653"])]
    public void DarkAetherIII(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Dark Aether III{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.5f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Dark Aether Burst III", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3653"])]
    public void DarkAetherBurstIII(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Dark Aether Burst III{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 0.3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Dark Aether III Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:3423"], userControl: false)]
    public void DarkAetherIIICleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Dark Aether III{@event.SourceId()}");
        accessory.Method.RemoveDraw($"Dark Aether Burst III{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Shadow Sprite Kill Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3654"])]
    public void ShadowSprite(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Kill Shadow Sprite", duration: 2000, true);
    }
    
    uint Extend = 0;
    public void Init(ScriptAccessory accessory) {
        Extend = 0;
    }
    
    [ScriptMethod(name: "Delay (Pull)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:3425"])]
    public void Delay(Event @event, ScriptAccessory accessory)
    {
        Extend = 1;
        accessory.Method.TextInfo("Pull", duration: 6500, true);
    }

    [ScriptMethod(name: "Delay Gate", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3655"])]
    public void DelayGate(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Delay Gate";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = 15800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Delay Gate Pull Prediction", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3655"])]
    public void Pull(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Pull";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.2f);
        dp.Scale = new(1, 6);
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = @event.SourcePosition();
        dp.DestoryAt = 15800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Delay Gate Cleanup", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:3655"], userControl: false)]
    public void DelayGateCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Delay Gate");
        accessory.Method.RemoveDraw("Pull");
    }
    
    [ScriptMethod(name: "Comet Tower Tether", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2005159", "Operate:Add", "Kind:EventObj"])]
    public void Comet(Event @event, ScriptAccessory accessory)
    {
        if (Extend != 1) return;  
        accessory.Method.TextInfo("Tank soak tower, prepare melee LB", duration: 12000, true);
        accessory.Method.EdgeTTS("Tank soak tower, prepare melee LB");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Tower Outline";
        dp.Color = new Vector4(1f, 1f, 0f, 8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3.1f);
        dp.InnerScale = new Vector2(3f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 12000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
        Extend = 0;
    }
    
    [ScriptMethod(name: "Dimensional Rift Kill Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3656"])]
    public void DimensionalRift(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS("Kill Dimensional Rift");
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
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

namespace O4n;

[ScriptType(guid: "b1b035ad-554d-4e49-84e3-9901e8ba941e", name: "O4N", territorys: [694],
    version: "0.0.0.2", author: "Linoa235", note: noteStr)]

public class O4n
{
    const string noteStr =
        """
        v0.0.0.1:
        LV70 Omega: Deltascape (Exdeath) Drawing
        Modified and supplemented based on JiaXX's drawing
        """;
    
    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "Doom", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:910"])]
    public void Doom(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Healer esuna doom", 2500);
    }

    [ScriptMethod(name: "Thunder (Area Tankbuster)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9405"])]
    public void Thunder(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Thunder";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId;
        dp.CentreResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.CentreOrderIndex = 1;
        dp.Scale = new Vector2(5);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Thunder (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9409"])]
    public void ThunderChariot(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Thunder Chariot";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(14.8f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Freeze (Frozen)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9408"])]
    public void Freeze(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Keep moving", duration: 4000, false);
    }
    
    [ScriptMethod(name: "Flare (Fire Spread)", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0019"])]
    public void FlareSpread(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = @event.TargetId();
        dp.Name = "Flare Spread";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Scale = new Vector2(4);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Flare (Fever)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9407"])]
    public void Fever(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Stay still", duration: 7400, true);
    }
    
    [ScriptMethod(name: "Death Breath", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9419"])]
    public void DeathBreath(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(20);
        dp.Radian = float.Pi * 2 / 3;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Holy (Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9413"])]
    public void Holy(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = @event.TargetId();
        dp.Name = "Holy";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Scale = new Vector2(6);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Black Hole", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:7802"])]
    public void BlackHole(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Name = "Black Hole";
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(2);
        dp.DestoryAt = 16500;
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
        
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = "Black Hole Outline";
        dp2.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp2.Owner = @event.SourceId();
        dp2.Scale = new Vector2(2.08f);
        dp2.InnerScale = new Vector2(2f);
        dp2.Radian = float.Pi * 2;
        dp2.DestoryAt = 16500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
    }

    [ScriptMethod(name: "Black Hole Cleanup", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:7802"], userControl: false)]
    public void BlackHoleCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Black Hole.*");
    }
    
    [ScriptMethod(name: "Vacuum Wave (Knockback)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9400"])]
    public void VacuumWave(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Vacuum Wave";
        dp.Scale = new(1.5f, 11f);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Anti-Knockback Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"], userControl: false)]
    public void AntiKnockbackCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Vacuum Wave");
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
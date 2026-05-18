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
using KodakkuAssist.Extensions;
using System.Threading.Tasks;
using KodakkuAssist.Extensions;

namespace Battle_in_the_Big_Keep;

[ScriptType(guid: "94e68f7e-2f2e-4a70-8ff3-14e9e2048e6b", name: "Battle in the Big Keep", territorys: [396],
    version: "0.0.0.3", author: "Linoa235", note: noteStr)]

public class Battle_in_the_Big_Keep
{
    const string noteStr =
        """
        v0.0.0.2:
        LV50 Battle in the Big Keep Initial Drawing
        Please choose one TTS option in User Settings, do not enable both.
        """;
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    
    [ScriptMethod(name: "Opening Hint", eventType: EventTypeEnum.Chat, eventCondition: ["Type:NPCDialogueAnnouncements", "Message:regex:^(ä¸€èµ·ä¸Šå§.*|To my side.*|è¡Œããž.*)$"])]
    public void Opening(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Only attack <Enkidu>", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Attack Enkidu");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Attack Enkidu");
    }
    
    [ScriptMethod(name: "Wind (Target Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3316"])]
    public void Wind(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Wind";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(4f);
        dp.DestoryAt = 700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Shrinking Melody Tether Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:438"])]
    public void ShrinkingMelody(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo("Stay close to tethered teammate", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Stay close to tethered teammate");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Stay close to tethered teammate");
    }
    
    [ScriptMethod(name: "Heavy & Slow Esuna Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:9"])]
    public void EsunaDebuffHint(Event @event, ScriptAccessory accessory)
    {
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (isHealer && isText) accessory.Method.TextInfo("Esuna debuff from teammate", duration: 5000, false);
        if (isHealer && isTTS) accessory.Method.TTS("Esuna debuff from teammate");
        if (isHealer && isEdgeTTS) accessory.Method.EdgeTTS("Esuna debuff from teammate");
    }
    
    uint Chicken = 0;
    
    public void Init(ScriptAccessory accessory) {
        Chicken = 0; 
    }
    
    [ScriptMethod(name: "Chicken Song_Chicken Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:644"], userControl: false)]
    public void ChickenSong(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        Chicken = 1; 
    }
    
    [ScriptMethod(name: "Chicken Cleanup", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:644"], userControl: false)]
    public void ChickenCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        Chicken = 0; 
    }
    
    [ScriptMethod(name: "Wind Circle Touch Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3623"])]
    public void WindCircleHint(Event @event, ScriptAccessory accessory)
    {
        if (Chicken != 1) return; 
        if (isText) accessory.Method.TextInfo("Touch wind circle", duration: 2500, false);
        if (isTTS) accessory.Method.TTS("Touch wind circle");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Touch wind circle");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Wind Gust";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Wind Gust Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:3318"], userControl: false)]
    public void WindGustCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Wind Gust");
    }
    
    [ScriptMethod(name: "Missile (Target Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3320"])]
    public void Missile(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Missile";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Capture Tether Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3335"])]
    public void Capture(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo("Move away to break tether", duration: 3000, true);
        if (isTTS) accessory.Method.TTS("Move away to break tether");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Move away to break tether");
    }
    
    [ScriptMethod(name: "Sword Dance (Continuous Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3333"])]
    public void SwordDance(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Sword Dance";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "Masamune (Line Charge)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3325"])]
    public void Masamune(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Masamune";
        dp.Scale = new (8f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Attack Dragon Head Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3626"])]
    public void AttackDragonHeadHint(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Attack the dragon head", duration: 2500, false);
        if (isTTS) accessory.Method.TTS("Attack the dragon head");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Attack the dragon head");
    }
    
    [ScriptMethod(name: "Shock_Discharge (Chariot)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3650"])]
    public void Discharge(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Discharge";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 4600;
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

#region Math Functions

public static class MathTools
{
    public static float DegToRad(this float deg) => (deg + 360f) % 360f / 180f * float.Pi;
    public static float RadToDeg(this float rad) => (rad + 2 * float.Pi) % (2 * float.Pi) / float.Pi * 180f;

    public static float GetRadian(this Vector3 point, Vector3 center)
        => MathF.Atan2(point.X - center.X, point.Z - center.Z);

    public static float GetLength(this Vector3 point, Vector3 center)
        => new Vector2(point.X - center.X, point.Z - center.Z).Length();

    public static Vector3 RotateAndExtend(this Vector3 point, Vector3 center, float radian, float length)
    {
        var baseRad = point.GetRadian(center);
        var baseLength = point.GetLength(center);
        var rotRad = baseRad + radian;
        return new Vector3(
            center.X + MathF.Sin(rotRad) * (length + baseLength),
            center.Y,
            center.Z + MathF.Cos(rotRad) * (length + baseLength)
        );
    }

    public static int RadianToRegion(this float radian, int regionNum, int baseRegionIdx = 0, bool isDiagDiv = false, bool isCw = false)
    {
        var sepRad = float.Pi * 2 / regionNum;
        var inputAngle = radian * (isCw ? -1 : 1) + (isDiagDiv ? sepRad / 2 : 0);
        var rad = (inputAngle + 4 * float.Pi) % (2 * float.Pi);
        return ((int)Math.Floor(rad / sepRad) + baseRegionIdx + regionNum) % regionNum;
    }

    public static Vector3 FoldPointHorizon(this Vector3 point, float centerX)
        => point with { X = 2 * centerX - point.X };

    public static Vector3 FoldPointVertical(this Vector3 point, float centerZ)
        => point with { Z = 2 * centerZ - point.Z };

    public static Vector3 PointCenterSymmetry(this Vector3 point, Vector3 center)
        => point.RotateAndExtend(center, float.Pi, 0);

    public static int GetDecimalDigit(this int val, int x)
    {
        var valStr = val.ToString();
        var length = valStr.Length;
        if (x < 1 || x > length) return -1;
        var digitChar = valStr[length - x];
        return int.Parse(digitChar.ToString());
    }
}

#endregion
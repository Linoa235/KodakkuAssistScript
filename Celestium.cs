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

namespace Celestium;

[ScriptType(guid: "7703f1a9-5698-4896-8908-bb8e415c1321", name: "Celestium", territorys: [796],
    version: "0.0.0.6", Author: "Linoa235", note: noteStr)]

public class Celestium
{
    const string noteStr =
        """
        v0.0.0.5:
        Celestium drawing and mechanic alerts, updated sporadically
        If unable to update, delete and refresh to re-download
        Currently supported floors: 18 [Blasting Duel]
        """;
    
    #region Basic Controls
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    #endregion

    #region 03: First Stone Wall - Xipacna
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 03: First Stone Wall - Xipacna â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor3(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Exorcism Shock (Interrupt)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14365"])]
    public void ExorcismShock(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Interrupt the boss", duration: 3000, true);
        if (isTTS) accessory.Method.TTS("Interrupt the boss");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Interrupt the boss");
    }
    
    #endregion
    
    #region 08: Blue Fang, Red Fang
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 08: Blue Fang, Red Fang â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor8(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Grand Explosion (Interrupt)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14680"])]
    public void GrandExplosion(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Interrupt the boss", duration: 3000, true);
        if (isTTS) accessory.Method.TTS("Interrupt the boss");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Interrupt the boss");
    }
    
    #endregion
    
    #region 18: Blasting Duel

    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 18: Blasting Duel â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor18(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Wild Charge (Line Knockback)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15055"])]
    public void WildCharge(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS("Charge knockback");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Charge knockback");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Wild Charge{@event.SourceId()}";
        dp.Scale = new (8f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }

    [ScriptMethod(name: "Wild Charge Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15055"], userControl: false)]
    public void WildChargeCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Wild Charge{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Ripping Claw (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15050"])]
    public void RippingClaw(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Ripping Claw{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Ripping Claw Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15050"], userControl: false)]
    public void RippingClawCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Ripping Claw{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Tail Smash (Tail Swipe)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15052"])]
    public void TailSmash(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Tail Smash{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.Radian = 90f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Tail Smash Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15052"], userControl: false)]
    public void TailSmashCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Tail Smash{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Fireball (Front Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15051"])]
    public void Fireball(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Fireball{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Fireball Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15051"], userControl: false)]
    public void FireballCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Fireball{@event.SourceId()}");
    }
    
    #endregion
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
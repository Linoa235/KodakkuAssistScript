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

namespace Omicron_Recall_Killing_Order;

[ScriptType(guid: "b73d07ef-aa90-45a9-ab4b-fc3ccce8791b", name: "Omicron Recall: Destroy the Omicron Weapon Chi", territorys: [960],
    version: "0.0.0.3", author: "Tetora", note: noteStr)]

public class Chi
{
    const string noteStr =
        """
        v0.0.0.2:
        LV90 Special FATE Drawing
        Omicron Recall: Destroy the Omicron Weapon Chi
        """;
    
    [ScriptMethod(name: "Lost Tether", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^758[67]$"])]
    public void LostTether(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Lost appeared", duration: 5000, true);
        accessory.Method.TTS("Lost appeared");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Lost Tether";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Lost Tether Cleanup", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:regex:^758[67]$"], userControl: false)]
    public void LostTetherCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Lost Tether");
    }
    
    [ScriptMethod(name: "Terminal Attack (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25172|2595[356])$"])]
    public void DonutAttack(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Donut Attack";
        dp.Color = new Vector4(1f, 1f, 0f, 0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(16f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = @event.ActionId() == 25953 ? 4700 : 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Terminal Attack (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25173|2595[478])$"])]
    public void LineAttack(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Line Attack";
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 1f, 0f, 0.4f);
        dp.Scale = new(32f, 120f); 
        dp.DestoryAt = @event.ActionId() == 25954 ? 4700 : 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "Front Sweep & Rear Sweep", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25959|2596[023]|2652[34])$"])]
    public void HalfRoomCone(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        dp.Color = new Vector4(1f, 1f, 0f, 0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(45);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 5700;
        
        switch (@event.ActionId())
        {
            case 26523:
            case 25959:
            case 25960:
                dp.Name = "Front Sweep";
                dp.Rotation = 0f.DegToRad(); 
                break;
            case 26524:
            case 25962:
            case 25963:
                dp.Name = "Rear Sweep";
                dp.Rotation = 180f.DegToRad(); 
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Zero-Type Front Sweep & Rear Sweep", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2595[5-8]|2596[03])$"])]
    public void ZeroTypeHalfRoomCone(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        dp.Color = new Vector4(1f, 1f, 0f, 0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(45);
        dp.Radian = 180f.DegToRad();
        
        switch (@event.ActionId())
        {
            case 25955:
            case 25957:
                dp.Name = "Front Sweep";
                dp.Rotation = 0f.DegToRad(); 
                dp.DestoryAt = 12200;
                break;
            case 25956:
            case 25958:
                dp.Name = "Rear Sweep";
                dp.Rotation = 180f.DegToRad(); 
                dp.DestoryAt = 12200;
                break;
            case 25960:
                dp.Name = "Rear Sweep";
                dp.Rotation = 180f.DegToRad(); 
                dp.Delay = 5800;
                dp.DestoryAt = 4400;
                break;
            case 25963:
                dp.Name = "Front Sweep";
                dp.Rotation = 0f.DegToRad(); 
                dp.Delay = 5800;
                dp.DestoryAt = 4400;
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Underground Penetration Bomb (Three-Through-One)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25101"])]
    public void UndergroundPenetrationBomb1(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Underground Penetration Bomb 1";
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0f, 0f, 0.6f);
        dp.Scale = new(20f, 20f);
        dp.Delay = 7700;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "Underground Penetration Bomb (Random)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25976"])]
    public void UndergroundPenetrationBomb2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Underground Penetration Bomb 2";
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0f, 0f, 0.6f);
        dp.Scale = new(20f, 20f);
        dp.Delay = 9800;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "Underground Penetration Bomb (Random) Pre-Explosion Display", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25976"])]
    public void UndergroundPenetrationBomb3(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Underground Penetration Bomb 3";
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 1f, 0f, 0.4f);
        dp.Scale = new(20f, 20f);
        dp.Delay = 8800;
        dp.DestoryAt = 1000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "TV Death Cleanup", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:13515"], userControl: false)]
    public void TVDeathCleanup(Event @event, ScriptAccessory accessory)
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
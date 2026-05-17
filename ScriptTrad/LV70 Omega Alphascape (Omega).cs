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

namespace O11n;

[ScriptType(name: "LV70 Omega Alphascape (Omega, guid: "5f040632-9d09-4bf5-a36a-3246e732e105")", territorys: [], $17db71ec0-546e-46c9-bca3-d710d6e5bacc", version: "0.0.0.1", Author: "Linoa235")]

public class O11n
{
    const string noteStr =
        """
        v0.0.0.3:
        LV70 Omega: Alphascape (Omega) Initial Drawing
        Supports DR auto-use of Task Command on Lightning Discharge Points.
        (Default is ON. Ensure you have DailyRoutines plugin v1.4.9.0 or higher properly installed.)
        """;
    
    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [UserSetting("DR Auto Charge on Lightning Discharge Points")]
    public bool isDRHelper { get; set; } = true;
    
    [ScriptMethod(name: "Lightning Discharge Point Auto-Use Task Command", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:9638"])]
    public void LightningDischargePointStart(Event @event, ScriptAccessory accessory)
    {
        if (isDRHelper) accessory.Method.SendChat("/pdr load Alphascape3Helper");
    }
    
    [ScriptMethod(name: "Mustard Bomb (Tankbuster)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12935"])]
    public void MustardBombTankbuster(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Mustard Bomb Tankbuster";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Port/Starboard SalvoÂ·Wave Cannon", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(12929|1293[012])$"])]
    public void SalvoWaveCannon(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Salvo Wave Cannon";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60);
        dp.Radian = 210f.DegToRad();
        
        switch (@event.ActionId())
        {
            case 12929:
                dp.Rotation = 270f.DegToRad();
                dp.DestoryAt = 4700;
                break;
            case 12930:
                dp.Rotation = 270f.DegToRad();
                dp.DestoryAt = 2700;
                break;
            case 12931:
                dp.Rotation = 90f.DegToRad();
                dp.DestoryAt = 4700;
                break;
            case 12932:
                dp.Rotation = 90f.DegToRad();
                dp.DestoryAt = 2700;
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Generate Peripheral - Rocket Punch (Line)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:9622"])]
    public void RocketPunch(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Rocket Punch";
        dp.Scale = new (15f, 49f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 12500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Rocket Punch Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:13724"], userControl: false)]
    public void RocketPunchCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Rocket Punch");
    }

    [ScriptMethod(name: "Mustard Bomb (Tether)", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0054"])]
    public async void Shockwave(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Shockwave");
            
        await Task.Delay(200);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Shockwave";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 8400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Shockwave Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:12928"], userControl: false)]
    public void ShockwaveCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Shockwave");
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
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

namespace The_Serpentlord_Seethes;

[ScriptType(name: "LV100 The Serpentlord Seethes Duel in the Wilds", territorys: [], $15af6a71b-a7c4-4e9e-bac1-8fcf5b152d9e", version: "0.0.0.1", Author: "Linoa235", guid: "93f834e9-d3d0-412d-8e7b-e1a5ed5404e7")]

public class Ttokrrone
{
    const string noteStr =
        """
        v0.0.0.3:
        LV100 Special FATE Drawing
        The Serpentlord Seethes: Duel in the Wilds
        """;
    
    #region Lost Related
    
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
    #endregion
    
    [ScriptMethod(name: "Earth-Swallowing Serpent (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3757[89]|3758[0-3])$"])]
    public void EarthSwallowingSerpent(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Earth-Swallowing Serpent";
        dp.Scale = new (27, 68f);
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(0f, 0f, 1f, 1f);
        dp.DestoryAt = @event.DurationMilliseconds() + 1400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    #region Sandstorm Mechanics
    
    [ScriptMethod(name: "One-Sided Sandstorm", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3731[3-6]$"])]
    public void OneSidedSandstorm(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Leave hitbox", duration: 4900, true);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "One-Sided Sandstorm";
        dp.Color = new Vector4(1f, 0f, 0f, 1.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 5700;
        
        switch (@event.ActionId())
        {
            case 37313:
                dp.Rotation = 0f.DegToRad();
                break;
            case 37314:
                dp.Rotation = 180f.DegToRad();
                break;
            case 37315:
                dp.Rotation = 270f.DegToRad();
                break;
            case 37316:
                dp.Rotation = 90f.DegToRad();
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "Gravel Chariot";
        dp1.Color = new Vector4(1f, 0f, 0f, 1.4f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(13f);
        dp1.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
    }

    [ScriptMethod(name: "Spinning Sandstorm", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3731[78]|3732[12])$"])]
    public void SpinningSandstorm(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60);
        dp.Radian = 90f.DegToRad();
        dp.Color = new Vector4(1f, 0f, 0f, 1.2f);
        
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(60);
        dp1.Radian = 90f.DegToRad();
        dp1.Color = new Vector4(1f, 0f, 0f, 1.2f);

        switch (@event.ActionId())
        {
            case 37317:
            case 37321:
                dp.Name = "Forward Spinning Sandstorm";
                dp.Rotation = 0f.DegToRad(); 
                dp.DestoryAt = 24700;
                break;
            case 37318:
            case 37322:
                dp.Name = "Backward Spinning Sandstorm";
                dp.Rotation = 180f.DegToRad();
                dp.DestoryAt = 7600;
                
                dp1.Name = "Backward Spinning Sandstorm Adjust";
                dp1.Delay = 7800;
                dp1.DestoryAt = 17100;
                break;
        }

        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
    }
    
    [ScriptMethod(name: "Spinning Sandstorm Next Turn Prediction", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3731[78]|3732[12])$"])]
    public void SpinningSandstormPrediction(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60);
        dp.Radian = 90f.DegToRad();
        dp.Color = new Vector4(1f, 1f, 0f, 0.8f);

        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(60);
        dp1.Radian = 90f.DegToRad();
        dp1.Color = new Vector4(1f, 1f, 0f, 0.6f);
        
        switch (@event.ActionId())
        {
            case 37317:
                dp.Name = "Forward Spinning Sandstorm R";
                dp.Rotation = 270f.DegToRad();
                dp.DestoryAt = 22700;
                accessory.Method.RemoveDraw($"Backward Spinning Sandstorm\\w*");
                accessory.Method.RemoveDraw($"Forward Spinning Sandstorm L");
                break;
            case 37321:
                dp.Name = "Forward Spinning Sandstorm L";
                dp.Rotation = 90f.DegToRad();
                dp.DestoryAt = 22700;
                accessory.Method.RemoveDraw($"Backward Spinning Sandstorm\\w*");
                accessory.Method.RemoveDraw($"Forward Spinning Sandstorm R");
                break;
            case 37318:
                dp.Name = "Backward Spinning Sandstorm R";
                dp.Rotation = 90f.DegToRad();
                dp.DestoryAt = 7600;
                
                dp1.Name = "Backward Spinning Sandstorm R Adjust";
                dp1.Rotation = 270f.DegToRad();
                dp1.Delay = 7600;
                dp1.DestoryAt = 15100;
                accessory.Method.RemoveDraw($"Forward Spinning Sandstorm\\w?");
                accessory.Method.RemoveDraw($"Backward Spinning Sandstorm\\L?Adjust");
                break;
            case 37322:
                dp.Name = "Backward Spinning Sandstorm L";
                dp.Rotation = 270f.DegToRad();
                dp.DestoryAt = 7600;
                
                dp1.Name = "Backward Spinning Sandstorm L Adjust";
                dp1.Rotation = 90f.DegToRad();
                dp1.Delay = 7600;
                dp1.DestoryAt = 15100;
                accessory.Method.RemoveDraw($"Forward Spinning Sandstorm\\w?");
                accessory.Method.RemoveDraw($"Backward Spinning Sandstorm\\R?Adjust");
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
    }

    [ScriptMethod(name: "Spinning Sandstorm Cleanup", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37327"], userControl: false)]
    public void SpinningSandstormCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }
    
    [ScriptMethod(name: "Spinning Sandstorm_Gravel (Continuous Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3731[78]|3732[12])$"])]
    public void Gravel(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Leave hitbox", duration: 16000, true);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Gravel Omen";
        dp.Color = new Vector4(1f, 1f, 0f, 1.4f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 6900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "Gravel Continuous";
        dp1.Color = new Vector4(1f, 0f, 0f, 1.6f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(13f);
        dp1.Delay = 6900;
        dp1.DestoryAt = 9100;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
    }
    #endregion
    
    #region Flying Sand Mechanics
    [ScriptMethod(name: "Flying Sand (Chariot Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3733[1-4]$"])]
    public void FlyingSand(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(19f);
        dp.DestoryAt = 7800;
        
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(60);
        dp1.InnerScale = new Vector2(14);
        dp1.Radian = 180f.DegToRad();
        dp1.DestoryAt = 7800;

        switch (@event.ActionId())
        {
            case 37331:
                dp.Name = "Flying Sand Chariot";
                dp.Color = new Vector4(1f, 0f, 0f, 1.4f);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                break;
            case 37332:
                dp1.Name = "Flying Sand Donut";
                dp1.Color = new Vector4(1f, 0f, 1f, 1.4f);
                dp1.Scale = new Vector2(60f);
                dp1.InnerScale = new Vector2(14f);
                dp1.Radian = float.Pi * 2;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
                break;
            case 37333:
                dp.Name = "Flying Sand Left Chariot";
                dp.Color = new Vector4(1f, 0f, 0f, 1.6f);
                dp.Radian = 180f.DegToRad();
                dp.Rotation = 90f.DegToRad();
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                dp1.Name = "Flying Sand Right Donut";
                dp1.Color = new Vector4(1f, 0f, 1f, 1.4f);
                dp1.Rotation = 270f.DegToRad();
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
                break;
            case 37334:
                dp.Name = "Flying Sand Right Chariot";
                dp.Color = new Vector4(1f, 0f, 0f, 1.6f);
                dp.Radian = 180f.DegToRad();
                dp.Rotation = 270f.DegToRad();
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                dp1.Name = "Flying Sand Left Donut";
                dp1.Color = new Vector4(1f, 0f, 1f, 1.4f);
                dp1.Rotation = 90f.DegToRad();
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
                break;
        }
    }   
    #endregion
    
    [ScriptMethod(name: "Sand Orb_Appearance Highlight", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38647"])]
    public void SandOrb_Appearance(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Sand Orb_Appearance";
        dp.Color = new Vector4(1f, 1f, 0f, 1.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Sand Orb_Grand Explosion (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3924[56]$"])]
    public void GrandExplosion(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Grand Explosion";
        dp.Color = new Vector4(1f, 1f, 0f, 1.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = @event.ActionId() == 39245 ? 7700 : 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Serpentlord Death Cleanup", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:16863"], userControl: false)]
    public void SerpentlordDeathCleanup(Event @event, ScriptAccessory accessory)
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
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

namespace DevoutPilgrimsVSDaivadipa;

[ScriptType(guid: "da82aeb0-9635-4f13-a1c1-39a0c859f596", name: "Beast Path Deity Worship: False God Descends", territorys: [957],
    version: "0.0.0.5", author: "Tetora", note: noteStr)]

public class Daivadipa
{
    const string noteStr =
        """
        v0.0.0.4:
        LV90 Special FATE Drawing
        Beast Path Deity Worship: False God Descends
        """;
    
    #region Basic Controls
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    #endregion
    
    [ScriptMethod(name: "Lost Tether", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^758[67]$"])]
    public void LostTether(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Lost appeared", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Lost appeared");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Lost appeared");

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
    
    [ScriptMethod(name: "Flame Manipulator Popup Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2649[89]$"])]
    public void FlameManipulatorHint(Event @event, ScriptAccessory accessory)
    {
        switch (@event.ActionId())
        {
            case 26498:
                if (isText) accessory.Method.TextInfo("Blue safe first", duration: 5000, false);
                if (isTTS) accessory.Method.TTS("Blue safe first");
                if (isEdgeTTS) accessory.Method.EdgeTTS("Blue safe first");
                break;
            case 26499:
                if (isText) accessory.Method.TextInfo("Red safe first", duration: 5000, true);
                if (isTTS) accessory.Method.TTS("Red safe first");
                if (isEdgeTTS) accessory.Method.EdgeTTS("Red safe first");
                break;
        }
    }
    
    [ScriptMethod(name: "Left Trident & Right Holy Axe", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2650[89]$"])]
    public void HalfRoomCone(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(65);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 6700;
        
        switch (@event.ActionId())
        {
            case 26508:
                dp.Name = "Left Trident";
                dp.Rotation = 90f.DegToRad(); 
                break;
            case 26509:
                dp.Name = "Right Holy Axe";
                dp.Rotation = 270f.DegToRad(); 
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Holy Flame Smite (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2649[89]$"])]
    public void HolyFlameSmite(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new(10f, 50f);
        dp.DestoryAt = 6700;
        
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Scale = new(10f, 50f);
        dp1.DestoryAt = 2200;

        switch (@event.ActionId())
        {
            case 26498:
                foreach (var item in accessory.Data.Objects.GetByDataId(13679))
                {
                    dp.Name = "Holy Flame Smite Red";
                    dp.Owner = item.EntityId;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
                
                foreach (var item in accessory.Data.Objects.GetByDataId(13680))
                {
                    dp1.Name = "Holy Flame Smite Blue";
                    dp1.Owner = item.EntityId;
                    dp1.Delay = 6900;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1);
                }
                break;
            case 26499:
                foreach (var item in accessory.Data.Objects.GetByDataId(13680))
                {
                    dp.Name = "Holy Flame Smite Blue";
                    dp.Owner = item.EntityId;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
                
                foreach (var item in accessory.Data.Objects.GetByDataId(13679))
                {
                    dp1.Name = "Holy Flame Smite Red";
                    dp1.Owner = item.EntityId;
                    dp1.Delay = 6900;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1);
                }
                break;
        }
    }
    
    [ScriptMethod(name: "Burning (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2649[89]$"])]
    public void Burning(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();

        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 6700;
        
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Scale = new Vector2(10f);
        dp1.DestoryAt = 3700;
        
        switch (@event.ActionId())
        {
            case 26498:
                foreach (var item in accessory.Data.Objects.GetByDataId(13681))
                {
                    dp.Name = "Burning Red";
                    dp.Owner = item.EntityId;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                
                foreach (var item in accessory.Data.Objects.GetByDataId(13682))
                {
                    dp1.Name = "Burning Blue";
                    dp1.Owner = item.EntityId;
                    dp1.Delay = 7200;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
                }
                break;
            case 26499:
                foreach (var item in accessory.Data.Objects.GetByDataId(13682))
                {
                    dp.Name = "Burning Blue";
                    dp.Owner = item.EntityId;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                
                foreach (var item in accessory.Data.Objects.GetByDataId(13681))
                {
                    dp1.Name = "Burning Red";
                    dp1.Owner = item.EntityId;
                    dp1.Delay = 7200;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
                }
                break;
        }
    }
    
    [ScriptMethod(name: "Move Command Position Prediction", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^19(5[89]|6[01])$"])]
    public async void MoveCommand(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        await Task.Delay(8000);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Move Command";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(1f, 10f);
        dp.DestoryAt = 3000;
        
        switch (@event["StatusID"])
        {
            case "1958":
                dp.Rotation = 0f.DegToRad();
                if (isText) accessory.Method.TextInfo("Forced move: Forward", duration: 3000, true);
                if (isTTS) accessory.Method.TTS("Move forward to safe zone");
                if (isEdgeTTS) accessory.Method.EdgeTTS("Move forward to safe zone");
                break;
            case "1959":
                dp.Rotation = 180f.DegToRad();
                if (isText) accessory.Method.TextInfo("Forced move: Back", duration: 3000, true);
                if (isTTS) accessory.Method.TTS("Move backward to safe zone");
                if (isEdgeTTS) accessory.Method.EdgeTTS("Move backward to safe zone");
                break;
            case "1960":
                dp.Rotation = 90f.DegToRad();
                if (isText) accessory.Method.TextInfo("Forced move: Left", duration: 3000, true);
                if (isTTS) accessory.Method.TTS("Move left to safe zone");
                if (isEdgeTTS) accessory.Method.EdgeTTS("Move left to safe zone");
                break;
            case "1961":
                dp.Rotation = 270f.DegToRad();
                if (isText) accessory.Method.TextInfo("Forced move: Right", duration: 3000, true);
                if (isTTS) accessory.Method.TTS("Move right to safe zone");
                if (isEdgeTTS) accessory.Method.EdgeTTS("Move right to safe zone");
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);
    }
    
    [ScriptMethod(name: "Move Command Cleanup Backup", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1257"], userControl: false)]
    public void MoveCommandCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Move Command");
    }
    
    [ScriptMethod(name: "Elephant Death Cleanup", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:13677"], userControl: false)]
    public void ElephantDeathCleanup(Event @event, ScriptAccessory accessory)
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
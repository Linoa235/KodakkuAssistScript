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

namespace Ala_Mhigo;

[ScriptType(guid: "33b94458-6ed0-4063-b7c9-64a11b10650b", name: "Ala Mhigo", territorys: [1146],
    version: "0.0.0.2", Author: "Linoa235", note: noteStr)]

public class Ala_Mhigo
{
    const string noteStr =
        """
        v0.0.0.1:
        LV70 Ala Mhigo Initial Drawing
        Please choose one TTS option in User Settings, do not enable both.
        """;
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    
    [ScriptMethod(name: "BOSS1_Guardian Scorpion Lock On", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:8263"])]
    public void LockOn(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS("Get away");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Get away");
    }
    
    [ScriptMethod(name: "BOSS3_Zenos Fudo San Dan (Cone Tankbuster)", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:5372"])]
    public void FudoSanDanTankbuster(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("Cone tankbuster", duration: 2500, true);
        if(isTTS) accessory.Method.TTS("Cone tankbuster");
        if(isEdgeTTS) accessory.Method.EdgeTTS("Cone tankbuster");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fudo San Dan Tankbuster";
        dp.Scale = new Vector2(10f);
        dp.Radian = 120f.DegToRad();
        dp.Owner = @event.SourceId();
        dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.TargetOrderIndex = 1;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);  
    }
    
    [ScriptMethod(name: "BOSS3_Zenos Fudo San Dan (Cone)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8290"])]
    public void FudoSanDanCone(Event @event, ScriptAccessory accessory)
    {
        if(isTTS) accessory.Method.TTS("Get away");
        if(isEdgeTTS) accessory.Method.EdgeTTS("Get away");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fudo San Dan Line";
        dp.Scale = new Vector2(10f);
        dp.Radian = 120f.DegToRad();
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0f, 0f, 1.2f);
        dp.DestoryAt = 1800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);  
    }
    
    [ScriptMethod(name: "BOSS3_Zenos Raikiri Issen (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(8294|9607)$"])]
    public void RaikiriIssen(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Raikiri Issen";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = @event.ActionId() == 8294 ? 5700 : 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_Zenos Fudan Issen (Knockback)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(8293|9606)$"])]
    public void FudanIssen(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("Knockback towards safe direction", duration: 5000, true);
        if(isTTS) accessory.Method.TTS("Knockback");
        if(isEdgeTTS) accessory.Method.EdgeTTS("Knockback");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fudan Issen";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Anti-Knockback Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"], userControl: false)]
    public void AntiKnockbackCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("Fudan Issen");
    }
    
    [ScriptMethod(name: "BOSS3_Zenos Yoto Issen (Line Spread)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(8296|9608)$"])]
    public void YotoIssen(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("Spread", duration: 4500, true);
        if(isTTS) accessory.Method.TTS("Spread");
        if(isEdgeTTS) accessory.Method.EdgeTTS("Spread");
        
        for (var i = 0; i <4;i++)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Yoto Issen{i}";
            dp.Scale = new(6, 41);
            dp.Owner = @event.SourceId();
            dp.TargetObject = accessory.Data.PartyList[i];
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
    }
    
    [ScriptMethod(name: "BOSS3_Zenos Mumyo Sen (Tether Induced Cone)", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0029"])]
    public void MumyoSen(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isText) accessory.Method.TextInfo("Guide cone outside the arena", duration: 7400, true);
            if (isTTS) accessory.Method.TTS("Guide cone outside the arena");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Guide cone outside the arena");
        }

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Mumyo Sen";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(41);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 8100;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
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
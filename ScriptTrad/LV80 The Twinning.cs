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

namespace theTwinning;

[ScriptType(guid: "8294dff8-0878-4cab-af9e-25748a646710", name: "The Twinning", territorys: [840],
    version: "0.0.0.3", Author: "Linoa235", note: noteStr)]

public class theTwinning
{
    const string noteStr =
        """
        v0.0.0.2:
        LV80 The Twinning Initial Drawing
        """;
    
    #region Basic Controls
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "Head Strike & Interject Cancel Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^75(38|51)$"], userControl: false)]
    public void InterruptCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Trash.*128-tonze Swing{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Low Kick & Leg Sweep & Holy Stun Cleanup", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2"], userControl: false)]
    public void StunCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Trash.*128-tonze Swing{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Cast Cancel Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15802"], userControl: false)]
    public void CastCancelCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Trash.*{@event.SourceId()}");
    }

    #endregion
    
    #region Trash Mobs
    
    [ScriptMethod(name: "Trash_Automated Minotaur 128-tonze Swing (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15802"])]
    public void HundredTwentyEightTonzeSwing(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Interrupt or stun <Automated Minotaur>", duration: 4000, false);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Trash_128-tonze Swing{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Trash_Mass-Produced Karyas Head (Tankbuster)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15804"])]
    public void MainHead(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("<Main Head> Tankbuster", duration: 2000, false);
        if (isTTS) accessory.Method.TTS("Tankbuster");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Tankbuster");
    }
    
    [ScriptMethod(name: "Trash_Vitalized Centaur Berserk Interrupt Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15808"], suppress: 30000)]
    public void Berserk(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Interrupt or stun <Vitalized Centaur>", duration: 4300, false);
    }
    
    #endregion
    
    #region Boss Section
    
    [ScriptMethod(name: "BOSS1_Zaghnal Type-1 Omen (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15717"])]
    public void Omen(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank && isText) accessory.Method.TextInfo("Avoid cleave", duration: 3300, true);
        if (isTank && isTTS) accessory.Method.TTS("Avoid cleave");
        if (isTank && isEdgeTTS) accessory.Method.EdgeTTS("Avoid cleave");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Omen";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "BOSS1_Zaghnal Type-1 Despair Impact (Line Spread)", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^003[2-5]$"])]
    public void DespairImpact(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Spread, avoid cages", duration: 7100, true);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        var boss = accessory.Data.Objects.GetByDataId(10193).FirstOrDefault();
        if (boss == null) return;
        dp.Owner = boss.GameObjectId;
        dp.Name = "Despair Impact";
        dp.Scale = new (6, 50f);
        dp.TargetObject = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        
        switch (@event["Id"])
        {
            case "0032":
                dp.DestoryAt = 7200;
                break;
            case "0033":
                dp.DestoryAt = 7400;
                break;
            case "0034":
                dp.DestoryAt = 7600;
                break;
            case "0035":
                dp.DestoryAt = 7800;
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS1_Zaghnal Type-1 Extermination Shot (Fire Circle Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15715"])]
    public void ExterminationShot(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Extermination Shot";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.4f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo("Stack and place fire circle", duration: 4300, true);
        if (isTTS) accessory.Method.TTS("Stack and place fire circle");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Stack and place fire circle");
    }
    
    [ScriptMethod(name: "BOSS2_Mithridates Thunder Bomb (Chariot)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:10244"])]
    public void ThunderBomb(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Thunder Bomb";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Thunder Bomb Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:15857"], userControl: false)]
    public void ThunderBombCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Thunder Bomb");
    }
    
    [ScriptMethod(name: "BOSS3_Tycoon Magitek Cross Laser (Line)", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2010169", "Operate:Add"])]
    public void MagitekLaser(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Magitek Laser";
        dp.Scale = new (8, 50f);
        dp.Offset = new Vector3 (0, 0, 10);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Magitek Laser Cleanup", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:4", "Id2:512"], userControl: false)]
    public void MagitekLaserCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Magitek Laser");
    }
    
    [ScriptMethod(name: "Magitek Laser Cleanup 2", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:15859"], userControl: false)]
    public void MagitekLaserCleanup2(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Magitek Laser");
    }
    
    [ScriptMethod(name: "BOSS3_Tycoon Artificial Gravity (Expanding Black Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15866"])]
    public void ArtificialGravity(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Artificial Gravity";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
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
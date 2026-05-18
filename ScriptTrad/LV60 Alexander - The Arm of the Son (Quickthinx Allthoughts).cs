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

namespace A7N;

[ScriptType(guid: "8b9e040a-ed20-4ef7-816c-64931359be0b", name: "A7N", territorys: [522],
    version: "0.0.0.1", Author: "Linoa235", note: noteStr)]

public class A7N
{
    const string noteStr =
        """
        v0.0.0.1:
        LV60 Alexander - The Arm of the Son (Quickthinx Allthoughts) Initial Drawing
        """;
    
    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [ScriptMethod(name: "Goblin Wave Cannon (Marker Line)", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0018"])]
    public void GoblinWaveCannon(Event @event, ScriptAccessory accessory)
    {        
        if (@event.TargetId() != accessory.Data.Me) {
            if (isText) accessory.Method.TextInfo("Stay away from marked player", duration: 5300, true);
            if (isTTS) accessory.Method.TTS("Stay away from marked player");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Stay away from marked player");
        } 
        else {
            if (isText) accessory.Method.TextInfo("Line marker", duration: 5300, true);
            if (isTTS) accessory.Method.TTS("Line marker");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Line marker");
        }
        
        var boss = accessory.Data.Objects.GetByDataId(5381).FirstOrDefault();
        if (boss == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Goblin Wave Cannon";
        dp.Scale = new (6f, 64f);
        dp.Owner = boss.GameObjectId;
        dp.TargetObject = @event.TargetId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Stun Bomb (Grab Marker)", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0029"])]
    public void StunBomb(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) {
            if (isText) accessory.Method.TextInfo("Stay away from marked player", duration: 8500, true);
            if (isTTS) accessory.Method.TTS("Stay away from marked player");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Stay away from marked player");
        } 
        else {
            if (isText) accessory.Method.TextInfo("Grab marker", duration: 8500, true);
            if (isTTS) accessory.Method.TTS("Grab marker");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Grab marker");
        }
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Stun Bomb";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 9200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Salvo Continuous Vulnerability Tankbuster Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3821"])]
    public void Salvo(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Continuous vulnerability tankbuster", duration: 7800, true);
        if (isTTS) accessory.Method.TTS("Continuous vulnerability tankbuster");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Continuous vulnerability tankbuster");
    }
    
    [ScriptMethod(name: "Shanoa Cheer Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5612"])]
    public void Cheer(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Prepare to focus fire on Sincere", duration: 2000, true);
        if (isTTS) accessory.Method.TTS("Prepare to focus fire on Sincere");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Prepare to focus fire on Sincere");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Shanoa";
        dp.Owner = accessory.Data.Me;
        dp.Color = new Vector4(1f, 1f, 0f, 1f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Sincere Kill Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:5384"])]
    public void SincereKillHint(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Kill Sincere", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Kill Sincere");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Kill Sincere");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Sincere";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Sincere Cleanup", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:5384"], userControl: false)]
    public void SincereCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Sincere");
    }
    
    [ScriptMethod(name: "Iron Ball Highlight", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:5388"])]
    public void IronBall(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Iron Ball";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(4.8f);
        dp.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "Iron Ball";
        dp1.Scale = new (1f, 9.6f); 
        dp1.Owner = @event.SourceId();
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp1.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp1);  
    }
    
    [ScriptMethod(name: "Iron Ball Cleanup", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:5388"], userControl: false)]
    public void IronBallCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Iron Ball");
    }
    
    [ScriptMethod(name: "Goblin Spark AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5615"])]
    public void GoblinSpark(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 3000, false);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "Prison Lock Kill Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:5389"])]
    public void PrisonLockKillHint(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Kill the prison lock", duration: 2500, true);
        if (isTTS) accessory.Method.TTS("Kill the prison lock");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Kill the prison lock");
    }
    
    uint MyPrey = 0;
    
    public void Init(ScriptAccessory accessory) {
        MyPrey = 0;
    }
    
    [ScriptMethod(name: "Marker Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1051"], userControl: false)]
    public void MarkerRecord(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        MyPrey = 1;
    }
    
    [ScriptMethod(name: "Fever Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1049"])]
    public void FeverHint(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo("Fever: stop moving", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Stop moving");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Stop moving");
        MyPrey = 0;
    }
    
    [ScriptMethod(name: "Suffocation Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:938"])]
    public void SuffocationHint(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo("Kill <Alarm System>", duration: 5000, false);
        if (isTTS) accessory.Method.TTS("Kill the add");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Kill the add");
        MyPrey = 0;
    }
    
    [ScriptMethod(name: "Frostbite Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:285"])]
    public void FrostbiteHint(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo("Kill <Type-3 Goblin Armor L>", duration: 5000, false);
        if (isTTS) accessory.Method.TTS("Kill the add");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Kill the add");
        MyPrey = 0;
    }
    
    [ScriptMethod(name: "Poison Hint", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:1010"])]
    public void PoisonHint(Event @event, ScriptAccessory accessory)
    {
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (MyPrey != 1 || @event.TargetId() != accessory.Data.Me) return; 
        if (isHealer && isText) accessory.Method.TextInfo("Stand on the poison gas vent", duration: 6000, true);
        if (isHealer && isTTS) accessory.Method.TTS("Stand on the poison gas vent");
        if (isHealer && isEdgeTTS) accessory.Method.EdgeTTS("Stand on the poison gas vent");
        MyPrey = 0;
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

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;
        return attribute?.Description ?? value.ToString();
    }
}

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
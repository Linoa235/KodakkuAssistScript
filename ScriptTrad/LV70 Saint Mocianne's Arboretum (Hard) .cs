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
using KodakkuAssist.Extensions;

namespace SaintMociannesArboretum_Hard;

[ScriptType(guid: "0c2f52e6-86b2-4e42-97dd-96750c0842bc", name: "Saint Mocianne's Arboretum (Hard)", territorys: [788],
    version: "0.0.0.4", author: "Linoa235", note: noteStr)]

public class SaintMociannesArboretum_Hard
{
    const string noteStr =
        """
        v0.0.0.3:
        LV70 Saint Mocianne's Arboretum (Hard) Initial Drawing
        """;
    
    #region Basic Controls
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [UserSetting("Developer Mode")]
    public bool isDeveloper { get; set; } = false;
    
    #endregion
    
    #region BOSS1_Mudmouth
    
    [ScriptMethod(name: "BOSS1_Mudmouth Sludge Bomb (Poison Circle Marker Prediction)", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0001"])]
    public void SludgeBomb(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me && isTTS) accessory.Method.TTS("Poison circle placement marker");
        if (@event.TargetId() == accessory.Data.Me && isEdgeTTS) accessory.Method.EdgeTTS("Poison circle placement marker");
            
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Sludge Bomb";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 3600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS1_Mudmouth Sludge Bomb (Poison Circle Early Display)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:11854"])]
    public void SludgeBombPoisonCircle(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Sludge Bomb Poison Circle";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.Delay = 2700;
        dp.DestoryAt = 2800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "BOSS1_Mudmouth Strata Puncture (Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:11850"])]
    public void StrataPuncture(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Strata Puncture";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS1_Mudmouth Predation_Malicious Venom Zone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:11855"])]
    public void MaliciousVenomZone(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("Hide behind small flower", duration: 8000, true);
        if(isTTS) accessory.Method.TTS("Hide behind small flower");
        if(isEdgeTTS) accessory.Method.EdgeTTS("Hide behind small flower");
        
        foreach (var item in accessory.Data.Objects.GetByDataId(9264))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Malicious Venom Zone";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = item.EntityId;
            dp.TargetPosition = new Vector3(0, 3, -82);
            dp.Scale = new Vector2(40);
            dp.Radian = 180f.DegToRad();
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
        }
    }
    #endregion
    
    [ScriptMethod(name: "Trash_Blooming Bilok Gurgling Spirit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12507"])]
    public void GurglingSpirit(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Gurgling Spirit{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 3700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Gurgling Spirit Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:12507"], userControl: false)]
    public void GurglingSpiritCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Gurgling Spirit{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Gurgling Spirit Cleanup 2", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(2|9|1608)$"], userControl: false)]
    public void GurglingSpiritCleanup2(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Gurgling Spirit{@event.SourceId()}");
    }
    
    #region BOSS2_Lahamu
    
    [ScriptMethod(name: "BOSS2_Mud Golem Rock Crumbling (Line)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:9258"])]
    public void RockCrumbling(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Rock Crumbling";
        dp.Scale = new (10, 45f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.6f);
        dp.DestoryAt = 12000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS2_Lahamu Earthquake", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0028"])]
    public void Earthquake(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        var boss = accessory.Data.Objects.GetByDataId(9257).FirstOrDefault();
        if (boss == null) return;
        dp.Owner = boss.GameObjectId;
        dp.TargetObject = @event.TargetId();
        
        dp.Name = "Earthquake";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Scale = new Vector2(77);
        dp.Radian = 30f.DegToRad();
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    #endregion
    

    [ScriptMethod(name: "Open Water Conduit Hint", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2009586", "Operate:Add", 
        "SourcePosition:{\"X\":287.36,\"Y\":-353.81,\"Z\":-230.91}"])]
    public void WaterConduit(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Water Conduit";
        dp.Color = new Vector4(0f, 1f, 0f, 3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(2.04f);
        dp.InnerScale = new Vector2(2f);
        dp.Radian = 2 * float.Pi;
        dp.Offset = new Vector3 (0,1,0);
        dp.DestoryAt = 180000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Water Conduit Cleanup", userControl: false, eventType: EventTypeEnum.ObjectEffect, 
        eventCondition: ["SourceName:regex:^(æ”¾æ°´æ “|water conduit)$", "Id2:2", "Id1:1","SourcePosition:{\"X\":287.36,\"Y\":-353.81,\"Z\":-230.91}"])]
    public void WaterConduitCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Water Conduit");
    }

    
    #region BOSS3_Rotten Muddy Monster

    uint Tokkapchi = 0;
    public void Init(ScriptAccessory accessory) {
        Tokkapchi = 0;
    }
    
    [ScriptMethod(name: "BOSS3_Rotten Muddy Monster Opening Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12597"], userControl: false)]
    public void MuddyLash(Event @event, ScriptAccessory accessory)
    {
        Tokkapchi = 1;
    }
    
    [ScriptMethod(name: "BOSS3_Rotten Muddy Monster Sludge Discharge", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id2:2", "Id1:1"])]
    public void SludgeDischarge(Event @event, ScriptAccessory accessory)
    {
        if (Tokkapchi == 1)
        {
            if(isText) accessory.Method.TextInfo("Stand on platform", duration: 7500, false);
            if(isTTS) accessory.Method.TTS("Stand on platform");
            if(isEdgeTTS) accessory.Method.EdgeTTS("Stand on platform");
        }
    }
    
    [ScriptMethod(name: "BOSS3_Rotten Muddy Monster Sludge", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12600"])]
    public void Sludge(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("Spread, leave platform", duration: 4500, true);
        if(isTTS) accessory.Method.TTS("Spread, leave platform");
        if(isEdgeTTS) accessory.Method.EdgeTTS("Spread, leave platform");
    }
    
    [ScriptMethod(name: "BOSS3_Sludge Slime Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:9262"])]
    public void SludgeSlime(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("Push slime onto platform, stay away from all damage", duration: 5000, true);
        if(isTTS) accessory.Method.TTS("Push add onto platform, stay away from all damage");
        if(isEdgeTTS) accessory.Method.EdgeTTS("Push add onto platform, stay away from all damage");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Sludge Slime";
        dp.Color = new Vector4(1f, 1f, 0f, 6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.1f);
        dp.InnerScale = new Vector2(1.0f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "BOSS3_Sludge Slime Burst & Corruption Explosion", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(1319[67]|13216)$"])]
    public void CorruptionExplosion(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Sludge Slime");
        
        switch (@event.ActionId())
        {
            case 13196:
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "Burst";
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = @event.SourceId();
                dp.Scale = new Vector2(6f);
                dp.DestoryAt = 2700;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                break;
            
            case 13197:
                if(isText) accessory.Method.TextInfo("Large AOE damage, use mitigation & shields", duration: 2500, false);
                if(isTTS) accessory.Method.TTS("Large AOE damage, use mitigation & shields");
                if(isEdgeTTS) accessory.Method.EdgeTTS("Large AOE damage, use mitigation & shields");
                break;
            
            case 13216:
                if(isText) accessory.Method.TextInfo("Very large AOE damage, use full raid mitigation & shields", duration: 2500, true);
                if(isTTS) accessory.Method.TTS("Very large AOE damage, use full mitigation & shields");
                if(isEdgeTTS) accessory.Method.EdgeTTS("Very large AOE damage, use full mitigation & shields");
                
                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = "Corruption Explosion";
                dp2.Color = new Vector4(1f, 0f, 0f, 1f);
                dp2.Owner = @event.SourceId();
                dp2.Scale = new Vector2(60f);
                dp2.DestoryAt = 2700;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);
                break;
        }
    }
    
    [ScriptMethod(name: "BOSS3_Rotten Muddy Monster Sludge Splash", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12604"])]
    public void SludgeSplash(Event @event, ScriptAccessory accessory)
    {
        if(isText) accessory.Method.TextInfo("Push slime to safe zone", duration: 7000, true);
        if(isTTS) accessory.Method.TTS("Push add to safe zone");
        if(isEdgeTTS) accessory.Method.EdgeTTS("Push add to safe zone");
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
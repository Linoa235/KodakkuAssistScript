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

namespace E8n;

[ScriptType(guid: "888aafb6-3df3-4a86-87e1-6990070f7b2e", name: "E8N", territorys: [905],
    version: "0.0.0.4", author: "Linoa235", note: noteStr)]

public class E8n
{
    const string noteStr =
        """
        v0.0.0.3:
        LV80 Eden's Verse: Refulgence (Conceived Shiva) Initial Drawing
        """;
    
    #region P1
    
    [ScriptMethod(name: "Transform_Shining Armor (Look Away)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:20303"])]
    public async void ShiningArmor(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(6200); 
        accessory.Method.TextInfo("Look away from boss", duration: 2500, true);
        accessory.Method.EdgeTTS("Look away from boss");
    }
    
    [ScriptMethod(name: "Transform_Ice Armor (Ice Floor)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:20302"])]
    public void IceArmor(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Stop moving", duration: 4700, true);
        accessory.Method.EdgeTTS("Stop moving");
    }
    
    [ScriptMethod(name: "Axe Kick (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19938"])]
    public void AxeKick(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Axe Kick";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(16f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Sickle Kick (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19939"])]
    public void SickleKick(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Sickle Kick";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.InnerScale = new Vector2(4f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Diamond Dust_Icicle Impact", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19978"])]
    public void IcicleImpact(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Icicle Impact";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 7700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Heavenly Strike (Knockback)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19928"])]
    public void HeavenlyStrike(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Knockback", duration: 4700, true);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Heavenly Strike";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Anti-Knockback Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"], userControl: false)]
    public void AntiKnockbackCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("Heavenly Strike");
    }
    
    [ScriptMethod(name: "Frost Sting (Tail Swipe)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19932"])]
    public void FrostSting(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Frost Sting";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = 90f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Chain Reflection: Frost Sting", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19967"])]
    public void ChainReflection_FrostSting(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Chain Reflection_Frost Sting";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Frost Slash (270Â° Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19931"])]
    public void FrostSlash(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Frost Slash";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = 270f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Chain Reflection: Frost Slash", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19966"])]
    public void ChainReflection_FrostSlash(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Chain Reflection_Frost Slash";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30);
        dp.Radian = 270f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    #endregion
    
    #region P2 World Split
    
    [ScriptMethod(name: "Earth Sprite_Stoneskin Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19951"])]
    public void Stoneskin(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Interrupt <Earth Sprite>", duration: 2500, true);
        accessory.Method.EdgeTTS("Interrupt Earth Sprite");
    }
    
    #endregion
    
    #region P3

    [ScriptMethod(name: "Split Holy Approach Tether", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19949"])]
    public void SplitHoly(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Fake flare: approach the boss", duration: 7700, true);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Split Holy";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    uint TargetMe = 0;
    public void Init(ScriptAccessory accessory) {
        TargetMe = 0; 
    }
    
    [ScriptMethod(name: "Wave of Light Marker Record", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"], userControl: false)]
    public void WaveOfLightRecord(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        TargetMe = 1; 
    }
    
    [ScriptMethod(name: "Wave of Light Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:19929"])]
    public void WaveOfLight(Event @event, ScriptAccessory accessory)
    {
        if (TargetMe == 1)
        {
            accessory.Method.TextInfo("Guide the cone, avoid towers", duration: 5200, true);
        }
        else
        {
            accessory.Method.TextInfo("Soak tower", duration: 6700, true);
        }
    }
    
    [ScriptMethod(name: "Wave of Light Spread Cone", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"])]
    public void WaveOfLightCone(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var boss = accessory.Data.Objects.GetByDataId(11635).FirstOrDefault();
        if (boss == null) return;
        dp.Owner = boss.GameObjectId;
        
        dp.Name = "Wave of Light";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new Vector2(40);
        dp.Radian = 60f.DegToRad();
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
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
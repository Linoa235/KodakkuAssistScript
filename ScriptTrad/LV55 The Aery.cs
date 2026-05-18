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

namespace the_Aery;

[ScriptType(guid: "04549e8d-7039-4436-9790-9bbe9abf940a", name: "LV55 The Aery", territorys: [1065],
    version: "0.0.0.3", author: "Linoa235", note: noteStr)]

public class the_Aery
{
    const string noteStr =
        """
        v0.0.0.3:
        LV55 The Aery Initial Drawing
        Please choose one TTS option in User Settings, do not enable both.
        """;
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    
    [ScriptMethod(name: "BOSS1_Rangda Wyvern Thundercloud Radiation (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3889"])]
    public void ThundercloudRadiation(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Thundercloud Radiation";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(64.9f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }

    [ScriptMethod(name: "BOSS1_Rangda Wyvern Lightning Rod Hint", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0006"])]
    public void LightningRod(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo("Pass the tether to the pillar on the edge", duration: 9600, true);
        if (isTTS) accessory.Method.TTS("Pass the tether to the pillar on the edge");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Pass the tether to the pillar on the edge");
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Color = accessory.Data.DefaultSafeColor;
        dp1.Scale = new Vector2(2.4f);
        dp1.DestoryAt = 9600;
        foreach (var item in accessory.Data.Objects.GetByDataId(3752))
        {
            dp1.Name = $"Ancient Statue{item.EntityId}";
            dp1.Owner = item.EntityId;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
        }
    }
    
    [ScriptMethod(name: "Lightning Rod Cleanup", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:2574"], userControl: false)]
    public void LightningRodCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Lightning Rod");
        accessory.Method.RemoveDraw($"Ancient Statue.*");
    }
    
    [ScriptMethod(name: "BOSS1_Rangda Wyvern Electrocution Knockback Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3890"])]
    public void ElectrocutionKnockback(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Knockback", duration: 4000, false);
        if (isTTS) accessory.Method.TTS("Knockback");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Knockback");
    }
    
    [ScriptMethod(name: "BOSS1_Rangda Wyvern Electrocution Line AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3890"])]
    public void Electrocution(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Electrocution";
        dp.Scale = new (5f, 64.9f);
        dp.Owner = @event.SourceId();
        dp.TargetObject = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.DestoryAt = 4300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Anti-Knockback Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"], userControl: false)]
    public void AntiKnockbackCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("Electrocution Knockback");
    }
    
    [ScriptMethod(name: "BOSS2_Giascutus Flammable Gas (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30181"], suppress: 600000)]
    public void FlammableGas(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Flammable Gas";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(26f);
        dp.InnerScale = new Vector2(20f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "BOSS2_Giascutus Explosion (Flammable Gas)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30184"])]
    public void Explosion(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Explosion";
        dp.Color = new Vector4(1f, 1f, 1f, 2.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 9700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS2_Giascutus Dragon's Roar (AOE)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31233"])]
    public void DragonsRoar2(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 3000, false);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "BOSS2_Giascutus Circle Flame (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30191"])]
    public void CircleFlame(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Circle Flame";
        dp.Color = new Vector4(1f, 1f, 0f, 1.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS2_Giascutus Ring Fire (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30190"])]
    public void RingFire(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Ring Fire";
        dp.Color = new Vector4(1f, 1f, 0f, 1.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.InnerScale = new Vector2(11f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Trash_Aery Mutant Dragon Ripping Claw (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5137"])]
    public void RippingClaw(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Ripping Claw";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "Ripping Claw Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:5137"], userControl: false)]
    public void RippingClawCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Ripping Claw");
    }
    
    [ScriptMethod(name: "BOSS3_Nidhogg Dragon's Roar (AOE)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30206"])]
    public void DragonsRoar3(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 4000, false);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "BOSS3_Nidhogg Fear Roar (Underfoot Circle-Imgui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30200"])]
    public void FearRoar(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fear Roar";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "BOSS3_Nidhogg Evil Flame Dive Early Display", eventType: EventTypeEnum.SetObjPos, eventCondition: ["Id:0197", "SourceDataId:14794", 
        "SourcePosition:regex:^({\"X\":35.00,\"Y\":149.24,\"Z\":-304.00}|{\"X\":13.00,\"Y\":147.97,\"Z\":-304.00}|{\"X\":57.00,\"Y\":147.97,\"Z\":-304.00})$"])]
    public void EvilFlameDive(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Evil Flame Dive";
        dp.Scale = new (22f, 80f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }

    
    [ScriptMethod(name: "BOSS3_Jet-black Orb Jet-black Flame Focus Fire Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:14795"])]
    public void JetBlackOrb(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Focus fire <Jet-black Orb>", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Focus fire Jet-black Orb");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Focus fire Jet-black Orb");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Jet-black Orb Tether";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Jet-black Flame Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:30204"], userControl: false)]
    public void JetBlackFlameCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Jet-black Orb Tether");
    }
    
    [ScriptMethod(name: "BOSS3_Nidhogg Crimson Orb (Tankbuster)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30205"])]
    public void CrimsonOrb(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Tankbuster", duration: 4000, false);
        if (isTTS) accessory.Method.TTS("Tankbuster");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Tankbuster");
    }
    
    [ScriptMethod(name: "BOSS3_Nidhogg Slaughter (AOE)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30207"])]
    public void Slaughter(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 5000, false);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS("AOE");
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
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


namespace the_Fractal_Continuum_Hard;

[ScriptType(guid: "d7858816-8eda-421e-b60d-59bdc00c3a55", name: "The Fractal Continuum (Hard)", territorys: [743],
    version: "0.0.0.3", author: "Linoa235", note: noteStr)]

public class the_Fractal_Continuum_Hard
{
    const string noteStr =
        """
        v0.0.0.2:
        The Fractal Continuum (Hard) Drawing
        Note: Boss 2 Triad and Final Boss Light Pillar mechanics not battle-tested. Please provide feedback on ARR if issues.
        """;
    
    #region Trash Mobs
    [ScriptMethod(name: "Trash-Minotaur XX-tonze Sweep (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^10(981|658)$"])]
    public void XXTonzeSweep(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"XX-tonze Sweep{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9);
        dp.Radian = 180f.DegToRad();
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
 
    [ScriptMethod(name: "Trash-Chimera Ice Howl (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2144"])]
    public void IceHowl(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Ice Howl{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.4f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Trash-Chimera Thunder Howl (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2145"])]
    public void ThunderHowl(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Thunder Howl{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(7.4f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    #endregion
    
    #region BOSS1_Floating Turret Mainboard
    [ScriptMethod(name: "BOSS1_Floating Turret Mainboard Siege Cannon (Front Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10149"])]
    public void SiegeCannon(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Siege Cannon";
        dp.Scale = new (8, 20f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS1_Proto-Turret Diffusion Ray (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10153"])]
    public void DiffusionRay(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Diffusion Ray";
        dp.Scale = new (4, 45f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS1_Floating Turret Mainboard High Voltage Thunder Current (Horizontal Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10149"])]
    public void HighVoltageThunderCurrent(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "High Voltage Thunder Current";
        dp.Scale = new (40, 4f);
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);  
    }
    #endregion
    
    #region BOSS2_Ultima Warrior
    [ScriptMethod(name: "BOSS2_Ultima Warrior Siege Cannon (Front Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10130"])]
    public void SiegeCannon2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Siege Cannon 2";
        dp.Scale = new (6, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS2_Ultima Warrior Infinity (Expanding Circle)", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2009284", "Operate:Add", "Kind:EventObj"])]
    public void Infinity(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Infinity";
        dp.Color = new Vector4(1f, 0f, 0f, 0.5f);
        dp.Position = @event.SourcePosition();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 22300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS2_Ultima Warrior Ice-Fire Brand Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1143"])]
    public void IceFireBrandHint(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        switch (@event.StatusId())
        {
            case 1143:
                accessory.Method.TextInfo("Stand on fire tower", duration: 3000, true);
                break;
            case 1144:
                accessory.Method.TextInfo("Stand on ice tower", duration: 3000, true);
                break;
        }
    }
    #endregion
    
    #region BOSS3_Ultima Beast
    [ScriptMethod(name: "BOSS3_Ultima Beast Death Spiral (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^1015[78]$"])]
    public void DeathSpiral(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Death Spiral";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 5700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_Ultima Beast Aether Bend (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^101(59|60)$"])]
    public void AetherBend(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Aether Bend";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "BOSS3_Ultima Beast Light Pillar (Ice Flower Floor Fire)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:10171"])]
    public void LightPillar(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Light Pillar";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3f);
        dp.Delay = 1900;
        dp.DestoryAt = 3500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    #endregion
    
    #region Death Cleanup
    [ScriptMethod(name: "Minotaur Death Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:regex:^10(981|658)$"], userControl: false)]
    public void MinotaurDeathCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"XX-tonze Sweep{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Chimera Death Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:regex:^214(4|5)$"], userControl: false)]
    public void ChimeraDeathCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Ice Howl{@event.SourceId()}");
        accessory.Method.RemoveDraw($"Thunder Howl{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Boss Death Cleanup", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:regex:^(8390|2|3)$"], userControl: false)]
    public void BossDeathCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
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
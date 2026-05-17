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

namespace MainScenario_Roulette;

[ScriptType(guid: "84c15eea-2a19-4477-ad21-cd43d1263cfa", name: "Main Scenario Roulette", territorys: [1043, 1044, 1048],
    version: "0.0.0.3", author: "Tetora", note: noteStr)]

public class MainScenario_Roulette
{
    const string noteStr =
        """
        v0.0.0.2:
        LV50 Main Scenario Roulette Initial Drawing
        DR Helper requires Daily Routines plugin to be properly installed.
        """;
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [UserSetting("DR Praetorium Auto-use Magitek Cannon on mech")]
    public bool isDRHelper { get; set; } = true;
    
    [ScriptMethod(name: "DR Praetorium Auto-use Magitek Cannon on mech", eventType: EventTypeEnum.ChangeMap, eventCondition: ["MapId:100"])]
    public void AutoUse(Event @event, ScriptAccessory accessory)
    {
        if (isDRHelper) accessory.Method.SendChat("/pdr load ThePraetoriumHelper");
    }
    
    #region The Porta Decumana
    
    [ScriptMethod(name: "————The Porta Decumana————", eventType: EventTypeEnum.Tether, eventCondition: ["ActionId:"])]
    public void ThePortaDecumana(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "BOSS1_Magitek Colossus Rubricatus High-power Magitek Laser (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28773"])]
    public void HighPowerMagitekLaser(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "High-power Magitek Laser";
        dp.Scale = new (4, 60f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS1_Magitek Colossus Rubricatus Request Bombing (AOE)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29268"])]
    public void RequestBombing(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 2000, false);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "BOSS2_Magitek Vanguard Hilda Vaporization Bomb (Meteor)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28779"])]
    public void VaporizationBomb(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Vaporization Bomb";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.DestoryAt = 6700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_Livia Magitek Ion (AOE)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28033"])]
    public void MagitekIon(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 4000, false);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS("AOE");
    }
    
    #endregion
    
    #region The Praetorium
    
    [ScriptMethod(name: "————The Praetorium————", eventType: EventTypeEnum.Tether, eventCondition: ["ActionId:"])]
    public void ThePraetorium(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "En route_Bombardment & Magitek Cannon (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^29(180|049)$"])]
    public void MagitekCannon(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Magitek Cannon{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Magitek Cannon Interrupt Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:29180"], userControl: false)]
    public void MagitekCannonCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Magitek Cannon{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "BOSS1_Magitek Colossus Ceruleum Radiation (AOE)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28474"])]
    public void CeruleumRadiation(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 4000, false);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "BOSS2_Nero Overload Shatter (Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28477"])]
    public void OverloadShatter(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Overload Shatter";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId;
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_Gaius Dread War (AOE)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28495"])]
    public void DreadWar(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 4000, false);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "BOSS3_Gaius Grace End (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2848[78]$"])]
    public void GraceEnd(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Grace End";
        dp.Scale = new (4, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "BOSS3_Gaius Composure (Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28493"])]
    public void Composure(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Composure";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId;
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_Gaius Guidance (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29052"])]
    public void Guidance(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Guidance";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Delay = 2700;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region The Ultimate Weapon
    
    [ScriptMethod(name: "————The Ultimate Weapon————", eventType: EventTypeEnum.Tether, eventCondition: ["ActionId:"])]
    public void TheUltimateWeapon(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Magitek Nuclear Explosion (AOE)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29022"])]
    public void MagitekNuclearExplosion(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 4000, false);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "Siege Cannon (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29020"])]
    public void SiegeCannon(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Siege Cannon";
        dp.Scale = new (12, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Magitek Laser (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2900[89]|29010)$"])]
    public void MagitekLaser(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Magitek Laser";
        dp.Scale = new (6, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        switch (@event.ActionId())
        {
            case 29010:  // Left
                dp.Rotation = MathHelpers.DegToRad(45f);
                break;
            case 29008:  // Center
                dp.Rotation = MathHelpers.DegToRad(0f);
                break;
            case 29009:  // Right
                dp.Rotation = MathHelpers.DegToRad(315f);
                break;
        }
        dp.DestoryAt = 1900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Aether Wave (Center Knockback)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29015"])]
    public void AetherWave(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Center knockback + touch orbs", duration: 3000, false);
        if (isTTS) accessory.Method.TTS("Center knockback");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Center knockback");
    }
    
    [ScriptMethod(name: "Focused Laser (Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29014"])]
    public void FocusedLaser(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Focused Laser";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId;
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
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

public static class MathHelpers
{
    public static float DegToRad(float degrees)
    {
        return degrees * (float)(Math.PI / 180.0);
    }
    
    public static double DegToRad(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    
    public static float RadToDeg(float radians)
    {
        return radians * (float)(180.0 / Math.PI);
    }
    
    public static double RadToDeg(double radians)
    {
        return radians * 180.0 / Math.PI;
    }
}
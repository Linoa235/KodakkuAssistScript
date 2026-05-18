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

namespace TheHead_theTail_theWholeDamnedThing;

[ScriptType(guid: "bc8c9569-5a56-4ae8-88dd-92b31862a43f", name: "Archaeotania: The Head, the Tail, the Whole Damned Thing", territorys: [818],
    version: "0.0.0.5", author: "Linoa235", note: noteStr)]

public class Archaeotania
{
    const string noteStr =
        """
        v0.0.0.4:
        LV80 Special FATE Drawing
        Archaeotania: The Head, the Tail, the Whole Damned Thing
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
    
    [ScriptMethod(name: "Civilization's Ruin (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(16441|17089)$"])]
    public void CivilizationsRuin(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Civilization's Ruin";
        dp.Scale = new (15, 62f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Tidal Wave Knockback Tether", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:16452"])]
    public void TidalWaveTether(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Approach water spout for knockback (percentage true damage)", duration: 8200, true);
        if (isTTS) accessory.Method.TTS("Approach water spout knockback");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Approach water spout knockback");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Tidal Wave Tether";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 7600;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Tornado_Storm Circle", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:10162"])]
    public void Tornado(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Tornado";
        dp.Color = new Vector4(1f, 0f, 0f, 1.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 42000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Tornado_Direction Line", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:10162"])]
    public void TornadoDirectionLine(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Tornado Direction Line";
        dp.Color = new Vector4(1f, 1f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1f, 5f);
        dp.DestoryAt = 42000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);
    }
    
    [ScriptMethod(name: "Tornado Cleanup", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:10162"], userControl: false)]
    public void TornadoCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Tornado");
        accessory.Method.RemoveDraw("Tornado Direction Line");
    }
    
    [ScriptMethod(name: "Tornado Cleanup Backup", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:16442"], userControl: false)]
    public void TornadoCleanupBackup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Tornado");
        accessory.Method.RemoveDraw("Tornado Direction Line");
    }
    
    [ScriptMethod(name: "Seagull Death Cleanup", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:10157"], userControl: false)]
    public void SeagullDeathCleanup(Event @event, ScriptAccessory accessory)
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
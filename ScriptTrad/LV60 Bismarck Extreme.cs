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

namespace Bismarck_Extreme;

[ScriptType(guid: "6740a08d-3e43-427f-b458-a6aab1f305d2", name: "Bismarck Extreme", territorys: [447],
    version: "0.0.0.2", author: "Linoa235", note: noteStr)]

public class Bismarck_Extreme
{
    const string noteStr =
        """
        v0.0.0.1:
        LV60 Bismarck Extreme Initial Drawing
        """;
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    uint MagitekFieldGenerator = 1;
    
    public void Init(ScriptAccessory accessory) {
        MagitekFieldGenerator = 1; 
    }
    
    [ScriptMethod(name: "Magitek Field Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:4778"], userControl: false)]
    public void MagitekField(Event @event, ScriptAccessory accessory)
    {
        MagitekFieldGenerator = 0; 
    }
    
    [ScriptMethod(name: "Opening Hint", eventType: EventTypeEnum.Director, eventCondition: ["Command:40000001"])]
    public async void OpeningHint(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Difficulty: â˜†\nKill differently colored snakes, watch for weather during transition", duration: 5000, true);
        accessory.Method.SendChat("/e â€”â€”â€”â€”Cheat Sheetâ€”â€”â€”â€”\nAfter snakes spawn: Blue buff kills green snake, green buff kills blue snake\nThunder: Spread, don't attack water bubble\nLight rain: Center chariot, attack water bubble\nHeavy rain: Center knockback into donut, attack water bubble");
    }
    
    [ScriptMethod(name: "Pull Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3827"])]
    public void PullHint(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (!isTank) return; 
        if (isText) accessory.Method.TextInfo("Pull other adds to the ranged add <Vundu Urmahi>", duration: 5000, false);
        if (isTTS) accessory.Method.TTS("Pull other adds to the ranged add");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Pull other adds to the ranged add");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Vundu Urmahi";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Scale = new Vector2(1f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Leviathan's Fury First Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4010"])]
    public void LeviathansFury(Event @event, ScriptAccessory accessory)
    {
        if (MagitekFieldGenerator == 0) return;
        if (isText) accessory.Method.TextInfo("Use <Magitek Field Generator>", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Use Magitek Field Generator");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Use Magitek Field Generator");
    }
    
    [ScriptMethod(name: "Get on Back Hint", eventType: EventTypeEnum.Targetable, eventCondition: ["Targetable:True", "SourceName:regex:^(è§’è´¨ç”²å£³|chitin carapace|å¼·ç¡¬å¤–æ®»)$"])]
    public void GetOnBackHint(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS("Get on back");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Get on back");
    }
    
    [ScriptMethod(name: "Whale Bone Bomb (Induction Water Circle Hint)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4021"])]
    public void WhaleBoneBomb(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Guide triple water circles", duration: 2000, false);
        if (isTTS) accessory.Method.TTS("Guide triple water circles");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Guide triple water circles");
    }
    
    [ScriptMethod(name: "Funnel Cloud (Tornado) Highlight", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:3830"])]
    public void FunnelCloud(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Funnel Cloud";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(4.5f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = "Funnel Cloud Outline";
        dp2.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp2.Owner = @event.SourceId();
        dp2.Scale = new Vector2(4.6f);
        dp2.InnerScale = new Vector2(4.5f);
        dp2.Radian = float.Pi * 2;
        dp2.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
    }
    
    [ScriptMethod(name: "Funnel Cloud Cleanup", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:3830"], userControl: false)]
    public void FunnelCloudCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Funnel Cloud.*");
    }
    
    [ScriptMethod(name: "Dead Water/Dead Wind Cast Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4032"])]
    public void DeadWindWater(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank)
        {
            if (isText) accessory.Method.TextInfo("Gain buff, then provoke opposite-colored target", duration: 2000, false);
            if (isTTS) accessory.Method.TTS("Gain buff, then provoke opposite-colored target");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Gain buff, then provoke opposite-colored target");
        }
        else
        {
            if (isText) accessory.Method.TextInfo("Gain buff, then attack opposite-colored target", duration: 2000, false);
            if (isTTS) accessory.Method.TTS("Gain buff, then attack opposite-colored target");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Gain buff, then attack opposite-colored target");
        }
    }
    
    [ScriptMethod(name: "Dominance over Wind/Water Buff Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^71[78]$"])]
    public void DominanceOverWindWater(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        var elemental = @event.StatusId == 717 ? "blue water snake" : "green wind snake";
        if (isText) accessory.Method.TextInfo($"Attack {elemental}", duration: 4000, true);
        if (isTTS) accessory.Method.TTS($"Attack {elemental}");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Attack {elemental}");
    }
    
    #region P3 Weather Mechanics

    [ScriptMethod(name: "Call Wind and Rain Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:4021"])]
    public void CallWindAndRain(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Thunder: Spread + don't attack water bubble\nHeavy rain: Attack water bubble, center knockback into donut\nLight rain: Attack water bubble, move away from center", duration: 10000, true);
        if (isTTS) accessory.Method.TTS("Watch weather change");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Watch weather change");
        accessory.Method.SendChat("/e â€”â€”â€”â€”Call Wind and Rainâ€”â€”â€”â€”\nThunder: Spread, don't attack water bubble\nLight rain: Center chariot, attack water bubble\nHeavy rain: Center knockback into donut, attack water bubble");
    }
    
    [ScriptMethod(name: "Thunder_Spread Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4016"])]
    public void Thunder(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Spread, don't attack water bubble", duration: 2000, true);
        if (isTTS) accessory.Method.TTS("Spread, don't attack water bubble");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Spread, don't attack water bubble");
    }
    
    [ScriptMethod(name: "Thunder_Thunderbolt & Thunderhead Spread", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^407[01]$"])]
    public void Thunderbolt(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Thunderbolt";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(@event.ActionId == 4071 ? 4 : 5);
        dp.DestoryAt = @event.ActionId == 4071 ? 3700 : 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Light Rain_Barbaric Tear (Center Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4014"])]
    public void BarbaricTear(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Barbaric Tear";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Offset = new Vector3(-15, 0, 0);
        dp.Scale = new Vector2(8);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Heavy Rain_Sharp Wind (Center Knockback)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4015"])]
    public void SharpWind(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Center knockback", duration: 5400, true);
        if (isTTS) accessory.Method.TTS("Center knockback");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Center knockback");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Knockback Tether";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = @event.TargetPosition + new Vector3(-15, 0, 0);
        dp.Scale = new(1);
        dp.DestoryAt = 6100;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Anti-Knockback Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"], userControl: false)]
    public void AntiKnockbackCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("Knockback Tether");
    }
    
    [ScriptMethod(name: "Heavy Rain_Storm (Center Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4012"])]
    public void Storm(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Storm";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.InnerScale = new Vector2(5f);
        dp.Offset = new Vector3(-15, 0, 0);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6100;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
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
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

namespace A_Finale_Most_Formidable;

[ScriptType(guid: "1f37bcfd-933c-43bd-a822-eb8e321b5175", name: "A Finale Most Formidable: The Secret Weapon", territorys: [814],
    version: "0.0.0.4", author: "Linoa235", note: noteStr)]

public class Formidable
{
    const string noteStr =
        """
        v0.0.0.3:
        LV80 Special FATE Drawing
        A Finale Most Formidable: The Secret Weapon
        """;

    [ScriptMethod(name: "Lost Tether", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^758[67]$"])]
    public void LostTether(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Lost appeared", duration: 5000, true);
        accessory.Method.TTS("Lost appeared");

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
    
    [ScriptMethod(name: "Guardian Automaton Kill Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:10868"])]
    public void GuardianAutomaton(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Kill <Guardian Automaton>", duration: 8000, true);
        accessory.Method.TTS("Kill the add");
    }
    
    [ScriptMethod(name: "Incendiary Bomb (Ground Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17397"])]
    public void IncendiaryBomb(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Incendiary Bomb";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Gulug Fire Suction Range", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17395"])]
    public void GulugFireInhale(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Approach the boss's feet, don't leave the circle", duration: 5000, true);
        accessory.Method.TTS("Approach boss feet");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Gulug Fire Inhale";
        dp.Color = new Vector4(1f, 0f, 1f, 0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.InnerScale = new Vector2(10f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 22800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "Gulug Fire Inhale Rim";
        dp1.Color = new Vector4(1f, 0f, 1f, 2f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(10.08f);
        dp1.InnerScale = new Vector2(10f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 22800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "Large Bomb Explosion (Expanding Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17411"])]
    public void Explosion(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Explosion";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 11700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Dwarven Missile Highlight", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:11221"])]
    public void DwarvenMissile(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Dwarven Missile{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Color = new Vector4(1f, 0f, 0f, 1.6f);
        dp.Scale = new(2f, 5f); 
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    [ScriptMethod(name: "Dwarven Missile Explosion Range Prediction", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:11221"])]
    public void DwarvenMissile_Explosion(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Dwarven Missile Explosion{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0.4f, 0f, 0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Dwarven Missile Explosion Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:18003"], userControl: false)]
    public void DwarvenMissileExplosionCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Dwarven Missile{@event.SourceId()}");
        accessory.Method.RemoveDraw($"Dwarven Missile Explosion{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Dwarven Missile Removal Cleanup", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:11221"], userControl: false)]
    public void DwarvenMissileRemovalCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Dwarven Missile{@event.SourceId()}");
        accessory.Method.RemoveDraw($"Dwarven Missile Explosion{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Dwarven Lightning Bomb (Chariot)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:11228"])]
    public void DwarvenLightningBombChariot(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Dwarven Lightning Bomb Chariot";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 8700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Dwarven Lightning Bomb (Donut)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:10908"])]
    public void DwarvenLightningBombDonut(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Approach purple orb donut", duration: 5000, true);
        accessory.Method.TTS("Approach purple orb donut");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Dwarven Lightning Bomb Donut";
        dp.Color = new Vector4(1f, 0f, 1f, 0.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(8.5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "Dwarven Lightning Bomb Donut Outline";
        dp1.Color = new Vector4(1f, 0f, 1f, 2f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(8.58f);
        dp1.InnerScale = new Vector2(8.5f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = "Donut Tether";
        dp2.Owner = accessory.Data.Me;
        dp2.Color = accessory.Data.DefaultSafeColor;
        dp2.ScaleMode |= ScaleMode.YByDistance;
        dp2.TargetObject = @event.SourceId();
        dp2.Scale = new(1);
        dp2.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
    }
    
    [ScriptMethod(name: "Steam Burst Knockback", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17394"])]
    public void SteamBurst(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Knockback", duration: 3000, true);
        accessory.Method.TTS("Knockback");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Steam Burst";
        dp.Scale = new(2f, 15);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Anti-Knockback Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"], userControl: false)]
    public void AntiKnockbackCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw("Steam Burst");
    }

    [ScriptMethod(name: "Motion Detection Disruptor Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1269"])]
    public async void MotionDetectionDisruptor(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        await Task.Delay(3500);

        accessory.Method.TextInfo("Stop moving", duration: 1200, true);
        accessory.Method.TTS("Stop moving");
    }

    [ScriptMethod(name: "Sweet Potato Death Cleanup", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:10573"], userControl: false)]
    public void SweetPotatoDeathCleanup(Event @event, ScriptAccessory accessory)
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
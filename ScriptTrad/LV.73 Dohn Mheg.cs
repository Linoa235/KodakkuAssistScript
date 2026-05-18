using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using System.Reflection.Metadata;

namespace Veever.Shadowbringers.DohnMheg;

[ScriptType(name: "LV.73 Dohn Mheg", territorys: [821], guid: "cb2a78d9-7a72-4d02-a3cd-18580a246976",
    version: "0.0.0.7", author: "Linoa235", note: noteStr)]

public class DohnMheg
{
    const string noteStr =
    """
    v0.0.0.7:
    1. Now supports text banner/TTS toggle/DR TTS toggle (make sure you have correctly installed the `DailyRoutines` plugin before using DR TTS toggle) (do not enable both TTS toggles at the same time)
    2. The underlying extensions of these previous scripts are currently too lazy to refactor (just add whatever)
    Duckmen.
    """;
    [UserSetting("Text Banner Toggle")]
    public bool isText { get; set; } = true;
    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = false;
    [UserSetting("DR TTS Toggle")]
    public bool isDRTTS { get; set; } = true;

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }

    #region Trash Mobs
    [ScriptMethod(name: "Watering Wheel", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15786"])]
    public void WateringWheel(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Interrupt the Fuath", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Interrupt the Fuath");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Interrupt the Fuath");
    }


    [ScriptMethod(name: "Unfinal Sting", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15794"])]
    public void UnfinalSting(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Unfinal Sting";
        dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3,8);
        dp.DestoryAt = 2500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Straight Punch", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15787"])]
    public void StraightPunch(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Mini tankbuster", duration: 2700, true);
        if (isTTS) accessory.Method.TTS("Mini tankbuster");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Mini tankbuster");
    }

    #endregion


    #region Boss1

    [ScriptMethod(name: "Boss1 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8857"])]
    public void Boss1Tankbuster(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Tankbuster incoming", duration: 4000, true);
        if (isTTS) accessory.Method.TTS("Tankbuster incoming");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Tankbuster incoming");
    }

    [ScriptMethod(name: "Boss1 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15813"])]
    public void Boss1AOE(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 4000, true);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts AOE");
    }



    [ScriptMethod(name: "Boss1 Landsblood (Too hard, gave up)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:8800"])]
    public void Boss1Landsblood(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();

        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "Landsblood";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.SourcePosition();
        dp.Scale = new Vector2(6);
        dp.DestoryAt = 1000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }


    [ScriptMethod(name: "Boss1 Stack", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:003E"])]
    public void Boss1Stack(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        string tname = @event["TargetName"]?.ToString() ?? "Unknown Target";

        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "Boss1 Stack";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6);
        dp.DestoryAt = 5500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        if (isText) accessory.Method.TextInfo($"Stack with {tname}", duration: 4000, true);
        if (isTTS) accessory.Method.TTS($"Stack with {tname}");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Stack with {tname}");
    }
    #endregion

    #region Boss2
    [ScriptMethod(name: "Boss2 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:8915"])]
    public void Boss2AOE(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 4000, true);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts AOE");
    }

    [ScriptMethod(name: "Boss2 Summon Fodder", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:8897"])]
    public void Boss2Fodder(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Pick up a tether connected to the boss", duration: 8000, true);
        if (isTTS) accessory.Method.TTS("Pick up a tether connected to the boss");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Pick up a tether connected to the boss");
    }

    #endregion

    #region Boss3
    [ScriptMethod(name: "Boss3 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:13732"])]
    public void Boss3Tankbuster(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Tankbuster incoming", duration: 4000, true);
        if (isTTS) accessory.Method.TTS("Tankbuster incoming");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Tankbuster incoming");
    }

    [ScriptMethod(name: "Boss3 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:13708"])]
    public void Boss3AOE(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 4700, true);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts AOE");
    }

    [ScriptMethod(name: "Boss3 Imp Choir", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:13552"])]
    public void Boss3ImpChoir(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Turn away from the boss", duration: 3700, true);
        if (isTTS) accessory.Method.TTS("Turn away from the boss");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Turn away from the boss");
    }

    [ScriptMethod(name: "Boss3 Toad Choir", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:13551"])]
    public void Boss3ToadChoir(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Go behind the boss", duration: 3700, true);
        if (isTTS) accessory.Method.TTS("Go behind the boss");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Go behind the boss");

        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "Boss3 Toad Choir";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(21);
        dp.Radian = float.Pi / 180 * 150;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Boss3 Finale", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15723"])]
    public void Boss3Finale(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("After crossing the bridge, enter the circle and attack the Dreaming Stringed Instrument", duration: 15000, true);
        if (isTTS) accessory.Method.TTS("After crossing the bridge, enter the circle and attack the instrument");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts After crossing the bridge, enter the circle and attack the instrument");
    }

    [ScriptMethod(name: "Boss3 Corrosive Bile", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:13547"])]
    public void Boss3CorrosiveBile(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Stay away from the boss's front", duration: 3700, true);
        if (isTTS) accessory.Method.TTS("Stay away from the boss's front");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Stay away from the boss's front");

        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "Boss3 Corrosive Bile";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(20);
        dp.Radian = float.Pi / 180 * 90;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Boss3 Flailing Tentacles", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:13952"])]
    public void Boss3FlailingTentacles(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Stay away from the boss's corners", duration: 4700, true);
        if (isTTS) accessory.Method.TTS("Stay away from the boss's corners");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Stay away from the boss's corners");

        float iAng = 45;
        float aIncrement = 90;

        for (var i = 0; i < 4; i++)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Boss3 Corrosive Bite:{i}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.TargetId();
            dp.Scale = new Vector2(20);
            dp.Radian = float.Pi / 180 * 50;
            dp.Rotation = float.Pi / 180 * (iAng + i * aIncrement);
            dp.DestoryAt = 4700;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
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
}
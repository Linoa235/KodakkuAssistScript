using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using System.Reflection.Metadata;
using System.Net;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using System.Runtime.Intrinsics.Arm;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Veever.EndWalker.theTowerofZot;

[ScriptType(name: "LV.81 The Tower of Zot", territorys: [952], guid: "98a97134-f87b-4386-aad9-2a99e81794ab",
    version: "0.0.0.4", author: "Veever", note: noteStr)]

public class the_Tower_of_Zot
{
    const string noteStr =
    """
    v0.0.0.4:
    1. Now supports text banner/TTS toggle/DR TTS toggle (make sure you have correctly installed the `DailyRoutines` plugin before using DR TTS toggle) (do not enable both TTS toggles at the same time)
    2. Marker toggle and local toggle are in user settings, you can choose to turn them off or on (local enabled by default)
    3. After killing Boss1, a door waypoint is generated. Green circles show the door that opens after killing adds (prevents disorientation)
    4. Boss2 marker deletion currently has a bug, but with normal DPS the boss will be almost dead by then (one Attack1 marker doesn't affect much), so maybe it will be fixed eventually
    5. Strongly recommend using local markers by default because they are instant (inhuman)
    6. Recommended to turn off Cactbot announcement features to prevent duplicate announcements
    Duckmen.
    """;
    [UserSetting("Text Banner Toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = false;

    [UserSetting("DR TTS Toggle")]
    public bool isDRTTS { get; set; } = true;

    [UserSetting("Marker Toggle")]
    public bool isMark { get; set; } = true;

    [UserSetting("Local Marker Toggle (ON = local only, OFF = party)")]
    public bool LocalMark { get; set; } = true;

    [UserSetting("Debug toggle, please turn off for non-development use")]
    public bool isDebug { get; set; } = false;

    public int berserkerSpheresCount;
    public int PrakamyaSiddhiCount;
    public int DeltaFireIIICount;

    private readonly object berserkerSpheresLock = new object();
    private readonly object DeltaFireIIILock = new object();

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        accessory.Method.MarkClear();
        berserkerSpheresCount = 0;
        PrakamyaSiddhiCount = 0;
        DeltaFireIIICount = 0;
    }

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    #region Trash Mobs
    [ScriptMethod(name: "Soporific Gas", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27874"])]
    public async void SoporificGas(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Soporific Gas";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(9f);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"Soporific Gas Outline";
        dp1.Scale = new(9f);
        dp1.InnerScale = new(8.98f);
        dp1.Radian = float.Pi * 2;
        dp1.Color = new Vector4(178 / 255.0f, 34 / 255.0f, 34 / 255.0f, 10.0f);
        dp1.Owner = @event.SourceId();
        dp1.DestoryAt = 4000;
        dp1.Radian = 2 * float.Pi;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp1);

        if (isText) accessory.Method.TextInfo("Chariot, can be interrupted", duration: 4000, true);
        accessory.TTS("Chariot, can be interrupted", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "Garlean Fire", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:24138"])]
    public void GarleanFire(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Garlean Fire";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = float.Pi / 180 * 90;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Left Arm Slash", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:24140"])]
    public void LeftArmSlash(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Left Arm Slash";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Radian = float.Pi / 180 * 90;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Boss1 - Boss2 Waypoint", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:13294"])]
    public async void Navi(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(50);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Navi1";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Position = new Vector3(-300.14f, -185.02f, 159.79f);
        dp.Scale = new Vector2(1.55f);
        dp.DestoryAt = long.MaxValue;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = "Navi2";
        dp1.Color = accessory.Data.DefaultSafeColor;
        dp1.Position = new Vector3(-330.10f, -181.00f, 82.42f);
        dp1.Scale = new Vector2(1.55f);
        dp1.DestoryAt = long.MaxValue;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);

        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = "Navi3";
        dp2.Color = accessory.Data.DefaultSafeColor;
        dp2.Position = new Vector3(-238.35f, -172.03f, 60.46f);
        dp2.Scale = new Vector2(1.55f);
        dp2.DestoryAt = long.MaxValue;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);
    }

    [ScriptMethod(name: "Delete Boss1 - Boss2 Waypoint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:13295"])]
    public void DelNavi(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }

    [ScriptMethod(name: "Diffusion Ray", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:24147"])]
    public void DiffusionRay(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Diffusion Ray";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition(); 
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(5);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    #endregion

    #region Boss1
    [ScriptMethod(name: "Boss1 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25248"])]
    public void Boss1Tankbuster(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Poison tankbuster incoming", duration: 4000, true);
        accessory.TTS("Poison tankbuster incoming", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "Manusya Blizzard III", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25238"])]
    public void ManusyaBlizzardIII(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Manusya Blizzard III";
        //dp.Color = accessory.Data.DefaultDangerColor;
        dp.Color = new Vector4(0 / 255.0f, 255 / 255.0f, 255 / 255.0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = float.Pi / 180 * 20;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Manusya Fire III", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25237"])]
    public void ManusyaFireIII(Event @event, ScriptAccessory accessory)
    {
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = $"Manusya Fire III (Single Dynamo)";
        dp2.Color = accessory.Data.DefaultSafeColor;
        dp2.Owner = @event.SourceId();
        dp2.Scale = new Vector2(5);
        dp2.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);

        var dp3 = accessory.Data.GetDefaultDrawProperties();
        dp3.Name = $"Manusya Fire III Waypoint";
        dp3.Owner = accessory.Data.Me;
        dp3.Color = accessory.Data.DefaultSafeColor;
        dp3.ScaleMode |= ScaleMode.YByDistance;
        dp3.TargetPosition = @event.SourcePosition();
        dp3.Scale = new(2);
        dp3.DestoryAt = 2500;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);
    }

    [ScriptMethod(name: "Manusya Thunder III", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25239"])]
    public void ManusyaThunderIII(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Manusya Thunder III";
        dp.Color = new Vector4(123 / 255.0f, 104 / 255.0f, 238 / 255.0f, 2f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(3);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Manusya Bio III", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25240"])]
    public void ManusyaBioIII(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Manusya Bio III";
        dp.Color = new Vector4(50 / 255.0f, 205 / 255.0f, 50 / 255.0f, 1.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = float.Pi / 180 * 180;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        if (isText) accessory.Method.TextInfo("Go behind the boss", duration: 4000, true);
        accessory.TTS("Go behind the boss", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "Boss1 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25242"])]
    public void TransmuteFireIII(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Prepare for targeted dynamo, approach the boss and watch for the next mechanic", duration: 3000, true);
        accessory.TTS("Prepare for targeted dynamo, approach the boss and watch for the next mechanic", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "Dhrupad", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25244"])]
    public void Dhrupad(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE-like, healers watch party HP", duration: 4000, true);
        accessory.TTS("AOE-like, healers watch party HP", isTTS, isDRTTS);
    }
    #endregion

    #region Boss2
    [ScriptMethod(name: "Boss2 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25257"])]
    public void Boss2Tankbuster(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Tankbuster incoming", duration: 3700, true);
        accessory.TTS("Tankbuster incoming", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "Prapti Siddhi", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25256"])]
    public void PraptiSiddhi(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Prapti Siddhi";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(4, 40);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Delay = 400;
        dp.DestoryAt = 1600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Berserker Spheres Chariot", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:13296"])]
    public void berserkerSpheres(Event @event, ScriptAccessory accessory)
    {
        lock (berserkerSpheresLock)
        {
            if (berserkerSpheresCount == 0)
            {
                if (isText) accessory.Method.TextInfo("Go to the safe zone", duration: 6000, true);
                accessory.TTS("Go to the safe zone", isTTS, isDRTTS);
            }
            if (berserkerSpheresCount == 5)
            {
                if (isText) accessory.Method.TextInfo("Go to the safe zone, prepare to attack the boss", duration: 6000, true);
                accessory.TTS("Go to the safe zone, prepare to attack the boss", isTTS, isDRTTS);
            }
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Berserker Spheres Chariot";
            dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
            dp.Position = @event.SourcePosition();
            dp.ScaleMode = ScaleMode.ByTime;
            dp.Scale = new Vector2(15f);
            dp.DestoryAt = (berserkerSpheresCount <= 4) ? 11501 : 20300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            DebugMsg($"berserkerSpheresCount: {berserkerSpheresCount}", accessory);
            berserkerSpheresCount++;
        }
    }

    [ScriptMethod(name: "Mark Boss", eventType: EventTypeEnum.MorelogCompat, eventCondition: ["Id:0197", "SourceDataId:13295"])]
    public async void markBoss(Event @event, ScriptAccessory accessory)
    {
        DebugMsg("markBoss", accessory);
        if (isMark) accessory.Method.Mark(@event.SourceId(), KodakkuAssist.Module.GameOperate.MarkType.Attack1, LocalMark);
    }

    [ScriptMethod(name: "Prakamya Siddhi", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25251"])]
    public void PrakamyaSiddhi(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Chariot, move away from boss", duration: 3700, true);
        accessory.TTS("Chariot, move away from boss", isTTS, isDRTTS);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Prakamya Siddhi";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        if (PrakamyaSiddhiCount > 0)
        {
            DebugMsg("clear markBoss", accessory);
            accessory.Method.MarkClear();
            DebugMsg("Finish clear markBoss", accessory);
        }
        PrakamyaSiddhiCount++;
    }

    [ScriptMethod(name: "Manusya Stop", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25255"])]
    public async void ManusyaStop(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(3500);
        if (isText) accessory.Method.TextInfo("Spread, do not stand in the same line", duration: 3700, true);
        accessory.TTS("Spread, do not stand in the same line", isTTS, isDRTTS);
    }
    #endregion

    #region Boss3
    [ScriptMethod(name: "Delta Fire III Spread", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25265"])]
    public void DeltaFireIII(Event @event, ScriptAccessory accessory)
    {
        lock (DeltaFireIIILock)
        {
            if (DeltaFireIIICount == 0)
            {
                if (isText) accessory.Method.TextInfo("First dynamo, then spread", duration: 4000, true);
                accessory.TTS("First dynamo, then spread", isTTS, isDRTTS);
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Delta Fire III Spread";
            dp.Color = new Vector4(255 / 255.0f, 0 / 255.0f, 251 / 255.0f, 1.0f);
            dp.Owner = @event.TargetId();
            dp.ScaleMode = ScaleMode.ByTime;
            dp.Scale = new Vector2(6);
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    [ScriptMethod(name: "Boss3 Prapti Siddhi", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25275"])]
    public void Boss3PraptiSiddhi(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Boss3 Prapti Siddhi";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(4, 40);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Delay = 400;
        dp.DestoryAt = 1600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Boss3 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25274|25280)$"])]
    public void Boss3Tankbuster(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Tankbuster + AOE-like, healers watch party HP", duration: 4000, true);
        accessory.TTS("Tankbuster + AOE-like, healers watch party HP", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "Delta Fire III Dynamo", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25263"])]
    public void DeltaFire1III(Event @event, ScriptAccessory accessory)
    {
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = $"Delta Fire III (Single Dynamo)";
        dp2.Color = accessory.Data.DefaultSafeColor;
        dp2.Position = @event.EffectPosition();
        dp2.Scale = new Vector2(5);
        dp2.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);

        var dp3 = accessory.Data.GetDefaultDrawProperties();
        dp3.Name = $"Delta Fire III Waypoint";
        dp3.Owner = accessory.Data.Me;
        dp3.Color = accessory.Data.DefaultSafeColor;
        dp3.ScaleMode |= ScaleMode.YByDistance;
        dp3.TargetPosition = @event.EffectPosition();
        dp3.Scale = new(2);
        dp3.DestoryAt = 2500;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);
    }

    [ScriptMethod(name: "Boss3 Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25272"])]
    public void Boss3Stack(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        string tname = @event["TargetName"]?.ToString() ?? "Unknown Target";

        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "Boss3 Stack";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5);
        dp.DestoryAt = 5000;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        if (isText) accessory.Method.TextInfo($"Stack with {tname}", duration: 5000, true);
        accessory.TTS($"Stack with {tname}", isTTS, isDRTTS);
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

    public static uint DataId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["DataId"]);
    }

    public static uint Command(this Event @event)
    {
        return ParseHexId(@event["Command"], out var cid) ? cid : 0;
    }

    public static string DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<string>(@event["DurationMilliseconds"]) ?? string.Empty;
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
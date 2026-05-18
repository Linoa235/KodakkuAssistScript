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

namespace Veever.Heavensward.AetherochemicalResearchFacility;

[ScriptType(name: "LV.60 Aetherochemical Research Facility", territorys: [1110], guid: "b3532f58-c1ca-4e1d-90cd-52f2abde17e4",
    version: "0.0.0.7", Author: "Linoa235", note: noteStr)]

public class AetherochemicalResearchFacility
{
    public int fireCount;
    public int iceCount;
    public int Away0061Count;
    public int TetherCount;

    private readonly object fireLock = new object();
    private readonly object iceLock = new object();
    private readonly object tetherLock = new object();
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
        fireCount = 0;
        iceCount = 0;
        Away0061Count = 0;
        TetherCount = 0;
    }

    #region Boss1
    [ScriptMethod(name: "Boss1 Magitek Turret", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(373[67])$"])]
    public void Boss1MagitekTurret(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Focus fire on Magitek Turret", duration: 10000, true);
        if (isTTS) accessory.Method.TTS("Focus fire on Magitek Turret");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Focus fire on Magitek Turret");
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "MagitekTurretPos";
        dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
        dp.Position = @event.SourcePosition();
        dp.Scale = new Vector2(2);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Boss1 Magitek Ray", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4321"])]
    public void Boss1MagitekRay(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Dodge the laser", duration: 2700, true);
        if (isTTS) accessory.Method.TTS("Dodge the laser");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Dodge the laser");
    }

    [ScriptMethod(name: "Boss1 Magitek Spread", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4316"])]
    public void Boss1MagitekSpread(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Go behind the boss", duration: 4200, true);
        if (isTTS) accessory.Method.TTS("Go behind the boss");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Go behind the boss");
    }
    #endregion




    #region Boss2
    [ScriptMethod(name: "Boss2 Cleave Announcement", eventType: EventTypeEnum.Chat, eventCondition: ["Message:å¯åŠ¨åˆæˆç”Ÿç‰©æ€§èƒ½è¯„æµ‹ç³»ç»Ÿâ€”â€”èµ«é²çŽ›å¥‡æ–¯ã€‚"])]
    public void Boss2Notification(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Most of this boss's skills are cleaves, non-tank jobs should not stand in front of the boss", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Most of this boss's skills are cleaves, non-tank jobs should not stand in front of the boss");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Most of this boss's skills are cleaves, non-tank jobs should not stand in front of the boss");
    }

    [ScriptMethod(name: "Boss2 Petrification", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4331"])]
    public void Boss2Petrifaction(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Turn away from the boss", duration: 2700, true);
        if (isTTS) accessory.Method.TTS("Turn away from the boss");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Turn away from the boss");
    }


    [ScriptMethod(name: "Boss2 Ballistic Missile", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4771"])]
    public void Boss2BallisticMissile(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Two-person stack, do not exceed two people", duration: 4000, true);
        if (isTTS) accessory.Method.TTS("Two-person stack, do not exceed two people");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Two-person stack, do not exceed two people");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Ballistic Missile";
        dp.Color = new Vector4(1.0f, 0.4f, 1.0f, 1.0f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(4);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }


    [ScriptMethod(name: "Boss2 Stack", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:005D"])]
    public void Boss2Stack(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        string tname = @event["TargetName"]?.ToString() ?? "Unknown Target";

        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "Boss2 Stack";
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


    #region Boss3-P1
    [ScriptMethod(name: "Boss3 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31885"])]
    public void Boss3AOE(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 4700, true);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts AOE");
    }

    [ScriptMethod(name: "Boss3 Fire Magic Orb", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:15782"])]
    public void Boss3Fireball(Event @event, ScriptAccessory accessory)
    {
        lock (fireLock)
        {
            if (fireCount <= 4)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"{fireCount}Fire Magic Orb";
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Position = @event.SourcePosition();
                dp.Scale = new Vector2(8);
                dp.DestoryAt = 6200;
                dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                //accessory.Method.SendChat($"iceCount:{iceCount}, fireCount: {fireCount}, TetherCount: {TetherCount}");
            }
            fireCount++;
        }
    }


    [ScriptMethod(name: "Boss3 Grip of Night", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32790"])]
    public void Boss3GripofNight(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Go behind the boss", duration: 5700, true);
        if (isTTS) accessory.Method.TTS("Go behind the boss");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Go behind the boss");
    }

    [ScriptMethod(name: "Boss3 Ice Magic Orb", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:15781"])]
    public void Boss3Iceball(Event @event, ScriptAccessory accessory)
    {
        lock (iceLock)
        {
            if (iceCount <= 3)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"{iceCount}Ice Magic Orb";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Position = @event.SourcePosition();
                dp.Scale = new Vector2(5);
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                //accessory.Method.SendChat($"iceCount:{iceCount}, fireCount: {fireCount}, TetherCount: {TetherCount}");
                if (iceCount == 1)
                {
                    if (isText) accessory.Method.TextInfo("Enter the green safe zone", duration: 5700, true);
                    if (isTTS) accessory.Method.TTS("Enter the green safe zone");
                    if (isDRTTS) accessory.Method.SendChat($"/pdr tts Enter the green safe zone");
                }
            }
            iceCount++;
        }
    }


    [ScriptMethod(name: "Boss3 Stack", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31892"])]
    public void Boss3EndofDays(Event @event, ScriptAccessory accessory)
    {
        string tname = @event["TargetName"]?.ToString() ?? "Unknown Target";
        if (isText) accessory.Method.TextInfo($"Stack with {tname}", duration: 4700, true);

        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "EndofDays";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Scale = new Vector2(6, 25);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        if (isTTS) accessory.Method.TTS($"Stack with {tname}");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Stack with {tname}");
    }

    #endregion


    #region Boss3-P2
    [ScriptMethod(name: "Boss3-P2 Reset Variables", eventType: EventTypeEnum.Chat, eventCondition: ["Message:regex:^æš—ä¹‹åŠ›åœ¨æ¶ŒåŠ¨â€¦â€¦\\r?\\nå¦‚ç«ç‚Žèˆ¬çƒ­çƒˆï¼Œå¦‚å†°éœœèˆ¬å¯‚é™ï¼$"])]
    public void Boss3P2Clean(Event @event, ScriptAccessory accessory)
    {
        iceCount = 10;
        fireCount = 10;
        TetherCount = 10;
        //accessory.Method.SendChat($"iceCount:{iceCount}, fireCount: {fireCount}, TetherCount: {TetherCount}");
    }


    [ScriptMethod(name: "Boss3 AOE2", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(319[10]0)$"])]
    public void Boss3AOE2(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 4700, true);
        if (isTTS) accessory.Method.TTS("AOE");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts AOE");
    }

    [ScriptMethod(name: "Boss3 Big AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:33024"])]
    public void Boss3Annihilation(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Big AOE", duration: 6000, true);
        if (isTTS) accessory.Method.TTS("Big AOE");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Big AOE");
    }


    [ScriptMethod(name: "Boss3 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31911"])]
    public void Boss3Tankbuster(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Tankbuster incoming", duration: 4700, true);
        if (isTTS) accessory.Method.TTS("Tankbuster incoming");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Tankbuster incoming");
    }

    [ScriptMethod(name: "Boss3 Stack2", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:00A1"])]
    public void Boss3Stack(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        string tname = @event["TargetName"]?.ToString() ?? "Unknown Target";

        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "Boss3 Stack";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6);
        dp.DestoryAt = 5500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        if (isText) accessory.Method.TextInfo($"Stack with {tname}", duration: 5000, true);
        if (isTTS) accessory.Method.TTS($"Stack with {tname}");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Stack with {tname}");
    }

    [ScriptMethod(name: "Boss3 Tether", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3505"])]
    public async void Boss3Away3505(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)
        {
            await Task.Delay(36000 - 2000);
            if (isText) accessory.Method.TextInfo("Group up, wait for tether to resolve then break tether", duration: 1500, true);
            if (isTTS) accessory.Method.TTS("Group up, wait for tether to resolve then break tether");
            if (isDRTTS) accessory.Method.SendChat($"/pdr tts Group up, wait for tether to resolve then break tether");

            await Task.Delay(2500);
            if (isText) accessory.Method.TextInfo("Break the tether", duration: 4000, true);
            if (isTTS) accessory.Method.TTS("Break the tether");
            if (isDRTTS) accessory.Method.SendChat($"/pdr tts Break the tether");
        }
    }


    [ScriptMethod(name: "Boss3 Stack2-2", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31907"])]
    public void Boss3EntropicFlame(Event @event, ScriptAccessory accessory)
    {
        string tname = @event["TargetName"]?.ToString() ?? "Unknown Target";
        if (isText) accessory.Method.TextInfo($"Stack with {tname}", duration: 4700, true);

        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "EntropicFlame";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Scale = new Vector2(6, 20);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        if (isTTS) accessory.Method.TTS($"Stack with {tname}");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Stack with {tname}");
    }

    [ScriptMethod(name: "Boss3 Focus Fire on Arcane Sphere Reminder", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:15788"])]
    public void Boss3ArcaneSphere(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Focus attack on the Arcane Sphere", duration: 4700, true);
        if (isTTS) accessory.Method.TTS("Focus attack on the Arcane Sphere");
        if (isDRTTS) accessory.Method.SendChat($"/pdr tts Focus attack on the Arcane Sphere");
    }


    #region Tether Basic Logic (Commented out - kept as is)
    // ... (commented code remains unchanged)
    #endregion


    [ScriptMethod(name: "Boss3 Tether Handler", eventType: EventTypeEnum.Tether, eventCondition: ["Id:006E"])]
    public void Boss3Tether(Event @event, ScriptAccessory accessory)
    {
        bool case1 = false, case2 = false;

        lock (tetherLock)
        {
            var pos1 = @event.SourcePosition();
            var pos2 = @event.TargetPosition();
            var midPoint = CalculateMidPoint(pos1, pos2);

            if (TetherCount <= 1)
            {
                DrawCircle(accessory, midPoint, 16, 10800, $"1Fire Magic Orb Tether1(Double Big Chariot){TetherCount}", accessory.Data.DefaultDangerColor, false);
            }
            else if (TetherCount == 2)
            {
                DrawCircle(accessory, midPoint, 5, 10000, $"2Ice Magic Orb Tether1(Single Dynamo){TetherCount}", accessory.Data.DefaultSafeColor, false);
                DrawDisplacement(accessory, midPoint, 2, 6000, $"{iceCount}Ice Magic Orb Tether1 Waypoint", delay: 0);
            }
            else if (TetherCount > 2 && TetherCount <= 4)
            {
                DrawCircle(accessory, midPoint, 16, 10000, $"3Fire Magic Orb Tether2(Double Big Chariot){TetherCount}", accessory.Data.DefaultDangerColor, false);
            }
            else if (TetherCount == 10 || TetherCount == 13)
            {
                float spostotal = pos1.X + pos1.Y + pos1.Z;
                float tpostotal = pos2.X + pos2.Y + pos2.Z;
                float difference = Math.Abs(spostotal - tpostotal);
                int roundedDifference = (int)Math.Round(difference);

                switch (roundedDifference)
                {
                    case 14:
                        case1 = true;
                        break;
                    case 26:
                        case2 = true;
                        break;
                }
            }

            TetherCount++;
        }

        if (case1)
        {
            DrawCase1(accessory);
        }

        if (case2)
        {
            DrawCase2(accessory);
        }
    }

    private Vector3 CalculateMidPoint(Vector3 pos1, Vector3 pos2)
    {
        if (pos1.X == pos2.X)
        {
            return pos1.Y == pos2.Y
                ? new Vector3(pos1.X, pos1.Y, (pos1.Z + pos2.Z) / 2.0f)
                : new Vector3(pos1.X, (pos1.Y + pos2.Y) / 2.0f, pos1.Z);
        }
        return new Vector3((pos1.X + pos2.X) / 2.0f, pos1.Y, pos1.Z);
    }

    private void DrawCircle(ScriptAccessory accessory, Vector3 position, float scale, int duration, string name, Vector4 color, bool isDebug = false, int delay = 0)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color;
        dp.Position = position;
        dp.Scale = new Vector2(scale);
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        isDebug = false;
        if (isDebug)
        {
            accessory.Method.SendChat($"iceCount:{iceCount}, fireCount: {fireCount}, TetherCount: {TetherCount}");
        }
    }

    private void DrawDisplacement(ScriptAccessory accessory, Vector3 target, float scale, int duration, string name, int delay = 0)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = target;
        dp.Scale = new Vector2(scale);
        dp.Delay = delay;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    private void DrawCase1(ScriptAccessory accessory)
    {
        var pos1 = new Vector3(219.00f, -456.46f, 79.00f);
        var pos2 = new Vector3(241.00f, -456.46f, 79.00f);
        var pos3 = new Vector3(230f, -456.46f, 79.00f);
        Vector3[] positions = { pos1, pos2 };

        foreach (var position in positions)
        {
            DrawCircle(accessory, position, 16, 10000, $"4Fire Magic Orb Tether P2-1(Double Big Chariot){TetherCount}", accessory.Data.DefaultDangerColor, false);
        }

        DrawCircle(accessory, pos3, 5, 5000, $"5Ice Magic Orb Tether P2-1(Single Dynamo){TetherCount}", accessory.Data.DefaultSafeColor, delay: 10000);
        DrawDisplacement(accessory, pos3, 2, 5000, $"{iceCount}Ice Magic Orb Tether P2-1 Waypoint", delay: 10000);
    }

    private void DrawCase2(ScriptAccessory accessory)
    {
        var pos1 = new Vector3(219.00f, -456.46f, 79.00f);
        var pos2 = new Vector3(241.00f, -456.46f, 79.00f);
        var pos3 = new Vector3(230f, -456.46f, 79.00f);
        Vector3[] positions = { pos1, pos2 };

        DrawCircle(accessory, pos3, 5, 9500, $"6Ice Magic Orb Tether P2-2(Single Dynamo){TetherCount}", accessory.Data.DefaultSafeColor);
        DrawDisplacement(accessory, pos3, 2, 9000, $"{iceCount}Ice Magic Orb Tether P2-2 Waypoint");

        foreach (var position in positions)
        {
            DrawCircle(accessory, position, 16, 2500, $"7Fire Magic Orb Tether P2-2(Double Big Chariot){TetherCount}", accessory.Data.DefaultDangerColor, false, delay: 10000);
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

    public static string SourceName(this Event @event)
    {
        return JsonConvert.DeserializeObject<string>(@event["SourceName"]) ?? string.Empty;
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
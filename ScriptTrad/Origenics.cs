using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading;
using Dalamud.Game.ClientState.Objects.Types;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using ECommons;
using ECommons.DalamudServices;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace KodakkuScript.Script._07_DawnTrail;

[ScriptType(guid: "9F1D4DC0-1891-7AC0-9571-D5266198751D", name: "Origenics", territorys: [1208], version: "0.0.0.1",
    author: "Poetry")]
public class Origenics
{
    private int _aoeCount = 0;
    private readonly object _lockObject = new object();
    private uint _thunderId = 0;

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        InitData();
    }

    private void InitData()
    {
        _aoeCount = 0;
        _thunderId = 0;
    }

    private static bool ParseObjectId(string? idStr, out uint id)
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

    private Vector3 DeserializeVector3(string propertyName)
    {
        return JsonConvert.DeserializeObject<Vector3>(propertyName);
    }

    private int DeserializeInt(string propertyName)
    {
        return JsonConvert.DeserializeObject<int>(propertyName);
    }

    // Boss 1
    // Boss1 - Piercing Scream
    // Boss2 - Thunder Conversion Wave
    // Boss3 - Psychokinesis Wave

    [ScriptMethod(name: "AOE", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:regex:^(36519|36371|36436)$"])]
    public void Aoe(Event @event, ScriptAccessory accessory)
    {
        var duration = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.TextInfo("AOE", duration);
    }

    // Poison Spray
    [ScriptMethod(name: "Boss1-Poison Spray: Circle AOE", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:regex:^3851[89]$"])]
    public void PoisonCircle(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Poison Spray: Circle AOE Indicator";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(6);
        dp.Position = DeserializeVector3(@event["EffectPosition"]);
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    // Arrogant
    // 36463
    // 36464
    // 36465 Right
    // 36466 Left
    // 36467 Back
    [ScriptMethod(name: "Boss1-Arrogant: Random Sequence Cone Attack", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:regex:^3646[5-7]$"])]
    public void Arrogant(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Arrogant: Random Sequence Cone Attack";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(25);
        if (DeserializeInt(@event["ActionId"]) == 36465)
        {
            dp.Radian = 7 * float.Pi / 6;
            dp.Rotation = -float.Pi / 2;
        }
        else if (DeserializeInt(@event["ActionId"]) == 36466)
        {
            dp.Radian = 7 * float.Pi / 6;
            dp.Rotation = float.Pi / 2;
        }
        else
        {
            dp.Radian = float.Pi / 2;
            dp.Rotation = float.Pi;
        }
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]) + 7000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    // Boss 2
    // Swivel Arm
    [ScriptMethod(name: "Boss2-Swivel Arm: Front Left/Rear Right Cone AOE", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:36370"])]
    public void Boss2Fan(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = $"Boss2-Swivel Arm";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
        dp.Scale = new Vector2(30);
        dp.Radian = float.Pi / 2;
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    // Salvo
    // 36372 Dangerous
    // 36373 Safe
    [ScriptMethod(name: "Boss2-Salvo: Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36372"])]
    public void Boss2Straight(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss2-Salvo: Line";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
        dp.Scale = new Vector2(4, 40);
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
    }

    // Laser Cannon
    // 36366 Dangerous
    // 38807 Safe
    [ScriptMethod(name: "Boss2-Laser Cannon: Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36366"])]
    public void Boss2Laser(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss2-Laser Cannon: Line";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
        dp.Scale = new Vector2(10, 40);
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
    }

    // Rush Surge
    [ScriptMethod(name: "Boss2-Rush Surge: Knockback Indicator", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36367"])]
    public void Boss2Charge(Event @event, ScriptAccessory accessory)
    {
        var duration = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.TextInfo("Knockback towards physical box (Anti-knockback works)!", duration);
    }

    // Boss 3
    // Suppression Assault
    [ScriptMethod(name: "Boss3-Suppression Assault: Half-room Cone", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:39233"])]
    public void Boss3NorthSouth(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss3-Suppression Assault";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(26);
        dp.Radian = float.Pi;
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    // Psychokinesis Reaction
    [ScriptMethod(name: "Boss3-Psychokinesis Reaction: Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36428"])]
    public void Boss3Straight(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss3-Psychokinesis Reaction: Line";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(13, 70);
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    // Psychokinesis Repulsion
    // 36433 Vertical Knockback
    // 36434 Horizontal Knockback
    // Horizontal Knockback Grid Drawing
    [ScriptMethod(name: "Boss3-Psychokinesis Repulsion: Knockback", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36434"])]
    public void Boss3Horizontal(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss3-Psychokinesis Repulsion: Horizontal Knockback";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
        dp.Scale = new Vector2(20, 17);
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
    }

    // Psychokinesis Repulsion
    // 39055
    [ScriptMethod(name: "Boss3-Psychokinesis Repulsion: Knockback Indicator", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39055"])]
    public void Boss3Charge(Event @event, ScriptAccessory accessory)
    {
        var duration = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.TextInfo("Knockback to safe zone", duration);
    }

    // Psychokinesis Suppression 39055
    [ScriptMethod(name: "Boss3-Psychokinesis Suppression: Cleave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39055"])]
    public void Boss3Charge2(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss3-Psychokinesis Suppression: Cleave";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(26);
        dp.Radian = float.Pi;
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    // Thunder Spear Throw - Psychokinesis Reaction 38953
    // Thunder Spear Recall 36431
    [ScriptMethod(name: "Boss3-Thunder Spear Throw: Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38953"])]
    public void Boss3ThunderStraight(Event @event, ScriptAccessory accessory)
    {
        lock (_lockObject)
        {
            _aoeCount++;
        }
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss3-Thunder Spear Throw: Psychokinesis Reaction";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(10, 45);
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]) + 5000;
        if (_aoeCount == 7)
        {
            dp.TargetObject = _thunderId;
            lock (_lockObject)
            {
                InitData();
            }
        }
        else if (_aoeCount == 6)
        {
            _thunderId = sid;
            dp.Rotation = float.Pi;
        }
        else
        {
            dp.Rotation = float.Pi;
        }
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Boss3-Thunder Spear Recall", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36431"])]
    public void Boss3ThunderStraight2(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss3-Thunder Spear Recall";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(10, 33);
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
    }
}
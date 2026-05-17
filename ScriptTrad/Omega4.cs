using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.Draw;
using Dalamud.Utility.Numerics;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using Dalamud.Memory.Exceptions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using KodakkuAssist.Module.GameOperate;

namespace KodakkuAssist.Omega;

[ScriptType(name: "Omega4", territorys: [], $133f35655-e9a8-4eb5-9581-cdc4ed08697d", version: "0.0.0.1", Author: "Linoa235", guid: "4cc70fcd-3912-4e7e-91b0-8d730e2b7e40")]
public class Omega4
{
    [ScriptMethod(name: "Doom", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:910"])]
    public void Doom(Event @event, ScriptAccessory accessory)
    {

        accessory.Method.TextInfo("Healer, please save me!", 2000);
    }

    [ScriptMethod(name: "Fever", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9407"])]
    public void Fever(Event @event, ScriptAccessory accessory)
    {

        accessory.Method.TextInfo("Stop moving", 2000);
    }

    [ScriptMethod(name: "Icebound", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9408"])]
    public void Icebound(Event @event, ScriptAccessory accessory)
    {

        accessory.Method.TextInfo("Keep moving", 2000);
    }

    [ScriptMethod(name: "Black Hole", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9416"])]
    public void BlackHole(Event @event, ScriptAccessory accessory)
    {

        accessory.Method.TextInfo("Move away from black hole", 2000);
    }

    [ScriptMethod(name: "Thunder Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9405"])]
    public void ThunderTankbuster(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Thunder Tankbuster";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = accessory.Data.PartyList[0];
        dp.Scale = new Vector2(5);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Thunder AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9409"])]
    public void ThunderAOE(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Thunder AOE";
        if (ParseObjectId(@event["SourceId"], out var id))
        {
            dp.Owner = id;
        }
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(15);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Explosion", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9402"])]
    public void Explosion(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        if (ParseObjectId(@event["TargetId"], out var id))
        {
            dp.Owner = id;
        }
        dp.Name = "Explosion";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(4);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Death Breath", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9419"])]
    public void DeathBreath(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        if (ParseObjectId(@event["SourceId"], out var id))
        {
            dp.Owner = id;
        }
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(20);
        dp.Radian = float.Pi * 2 / 3;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Black Hole Spawn", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:7802"])]
    public void BlackHoleSpawn(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        if (ParseObjectId(@event["SourceId"], out var id))
        {
            dp.Owner = id;
        }

        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Name = $"Black Hole {@event["SourceId"]}";
        dp.Scale = new Vector2(2);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Black Hole Clear", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:7802"])]
    public void BlackHoleClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Black Hole {@event["SourceId"]}");
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
}
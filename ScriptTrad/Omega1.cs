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

[ScriptType(name: "Omega1", territorys: [691], guid: "e0053855-1afd-49a7-85c8-537a6c7c1273", version: "0.0.0.1",
    author: "JiaXX")]
public class Omega1
{
    [ScriptMethod(name: "Fireball", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6769"])]
    public void Fireball(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        if (ParseObjectId(@event["SourceId"], out var id))
        {
            dp.Owner = id;
        }

        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Name = $"Fireball {@event["SourceId"]}";
        dp.Scale = new Vector2(8);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Fireball Clear", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:9173"])]
    public void FireballClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Fireball {@event["SourceId"]}");
    }

    [ScriptMethod(name: "Wind Wing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9182"])]
    public void WindWing(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Position";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Scale = new Vector2(1.5f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "Double Thunder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9175"])]
    public void DoubleThunder(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "MT Tankbuster";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = accessory.Data.PartyList[0];
        dp.Scale = new Vector2(3);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        dp.Name = "ST Tankbuster";
        dp.Owner = accessory.Data.PartyList[1];
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
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
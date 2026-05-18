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

[ScriptType(name: "Omega2", territorys: [692], guid: "41a74daf-9b2b-4061-a27f-2b773dd3f4e7", version: "0.0.0.1",
    author: "Linoa235")]
public class Omega2
{
    private uint parse = 0;

    public void Init(ScriptAccessory accessory)
    {
        parse = 0;
        accessory.Method.MarkClear();
    }

    [ScriptMethod(name: "Earthquake", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9490"])]
    public void Earthquake(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = new Vector3(0, 0, 0);
        dp.Scale = new Vector2(20);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4700;
        accessory.Method.TextInfo("Jump", 2000);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Sink", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:567"])]
    public void Sink(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = new Vector3(0, 0, 0);
        dp.Scale = new Vector2(20);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5000;
        accessory.Method.TextInfo("Jump", 2000);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Gravity Manipulation", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:9477"])]
    public void GravityManipulation(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Color = accessory.Data.DefaultDangerColor;
        if (ParseObjectId(@event["TargetId"], out var id))
        {
            dp.Owner = id;
        }

        dp.Scale = new Vector2(6);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5000;
        accessory.Method.TextInfo("Jump", 2000);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Intrusion", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9616"])]
    public void Intrusion(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Color = accessory.Data.DefaultDangerColor;
        if (ParseObjectId(@event["SourceId"], out var id))
        {
            dp.Owner = id;
        }

        dp.Scale = new Vector2(4);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Main Shock", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:9381"])]
    public void MainShock(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Color = accessory.Data.DefaultDangerColor;
        if (ParseObjectId(@event["SourceId"], out var id))
        {
            dp.Owner = id;
        }

        dp.Scale = new Vector2(15);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 1000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Dark Light", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9474"])]
    public void DarkLight(Event @event, ScriptAccessory accessory)
    {
        switch (parse)
        {
            case 0:
                accessory.Method.TextInfo("Jump", 1000);
                break;
            case 1:
                accessory.Method.TextInfo("Don't jump", 1000);
                break;
        }

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Safe Position";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Scale = new Vector2(1.5f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = new Vector3(0, 0, 0);
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        parse++;
    }

    [ScriptMethod(name: "Demon Eye", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9485"])]
    public void DemonEye(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Look away from boss", 1000);
    }

    [ScriptMethod(name: "Roar", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9471"])]
    public void Roar(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Position";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Scale = new Vector2(1.5f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = new Vector3(0, 0, -18);
        dp.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
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
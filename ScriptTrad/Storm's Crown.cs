using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace Cyf5119Script.Endwalker.StormsCrown;

[ScriptType(guid: "776A7DFB-F8C3-4ECC-BFB7-3631D083A117", name: "Storm's Crown", territorys: [1071], version: "0.0.0.2", author: "Cyf5119")]
public class StormsCrown
{
    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }

    [ScriptMethod(name: "Void Aero IV", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30134"])]
    public void VoidAeroIV(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("AOE", 5000);
    }

    [ScriptMethod(name: "Void Aero III", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30135"])]
    public void VoidAeroIII(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Void Aero III";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5000;
        dp.Owner = @event.TargetId();
        dp.Scale = new(5);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
        accessory.Method.TextInfo("Stack Tankbuster", 5000);
    }

    [ScriptMethod(name: "Savage Barbery 1", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(30138|30144)$"])]
    public void SavageBarbery1(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Savage Barbery";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 8000;
        dp.Owner = @event.SourceId();
        if (@event.ActionId() == 30138)
        {
            dp.Scale = new Vector2(20);
            dp.InnerScale = new Vector2(6);
            dp.Radian = float.Pi * 2;
            accessory.Method.SendDraw(0, DrawTypeEnum.Donut, dp);
        }
        else
        {
            dp.Scale = new(12, 40);
            accessory.Method.SendDraw(0, DrawTypeEnum.Straight, dp);
        }
    }

    [ScriptMethod(name: "Savage Barbery 2", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(30139|30145)$"])]
    public void SavageBarbery2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Savage Barbery";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 10100;
        dp.Position = @event.EffectPosition();
        dp.Scale = new(20);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Hair Raid", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30147"])]
    public void HairRaid(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Hair Raid";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 8000;
        dp.Owner = @event.SourceId();
        dp.Scale = new(40);
        dp.Radian = float.Pi / 180 * 120;
        accessory.Method.SendDraw(0, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Deadly Twist", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30172"])]
    public void DeadlyTwist(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Deadly Twist";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.DestoryAt = 5000;
        dp.Owner = @event.TargetId();
        dp.Scale = new(6);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Curling Iron", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30130"])]
    public void CurlingIron(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Leave target circle, cutscene incoming.", 13200);
    }

    [ScriptMethod(name: "Catabasis", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30140"])]
    public void Catabasis(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Large AOE", 8000);
    }

    [ScriptMethod(name: "Knuckle Drum", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:30103"])]
    public void KnuckleDrum(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Consecutive AOE", 10000);
    }
}
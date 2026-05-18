using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace Cyf5119Script.Endwalker.TheFinalDay;

[ScriptType(guid: "c996343b-7614-49fd-822b-877a35a1fd52", name: "The Final Day", territorys: [997], version: "0.0.0.2", Author: "Linoa235")]
public class TheFinalDay
{
    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }

    [ScriptMethod(name: "Elegeia", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(26156|26242)$"])]
    public void Elegeia(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("AOE", 8300);
    }

    [ScriptMethod(name: "Stellar Collision", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(26158|26171)$"])]
    public void StellarCollision(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Stellar Collision";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.ActionId() == 26158 ? 7000 : 2000;
        dp.Position = @event.EffectPosition();
        dp.Scale = new(30);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Galaxias", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27754"])]
    public void Galaxias(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Galaxias";
        dp.Color = new(0.2f, 1, 1, 2);
        dp.DestoryAt = 5000;
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = @event.EffectPosition();
        dp.Rotation = float.Pi;
        dp.Scale = new(1, 13);
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Elenchos", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(26179|26180)$"])]
    public void Elenchos(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Elenchos";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6000;
        dp.Owner = @event.SourceId();
        var middle = @event.ActionId() == 26180;
        dp.Scale = new(middle ? 14 : 13, 40);
        accessory.Method.SendDraw(0, middle ? DrawTypeEnum.Rect : DrawTypeEnum.Straight, dp);
    }

    [ScriptMethod(name: "Pharmakon", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26187"])]
    public void Pharmakon(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Pharmakon";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 7700;
        dp.Position = @event.TargetPosition();
        dp.Scale = new(6);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Aporrhoia", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26174"])]
    public void Aporrhoia(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Aporrhoia";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5000;
        dp.Owner = @event.SourceId();
        dp.Scale = new(5, 40);
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Hubris", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26195"])]
    public void Hubris(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Hubris";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5000;
        dp.Owner = @event.TargetId();
        dp.Scale = new(6);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Misery", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26182"])]
    public void Misery(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Misery";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = 3000;
        dp.DestoryAt = 22000;
        dp.Owner = @event.SourceId();
        dp.Scale = new(12);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Crash", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26199"])]
    public void Crash(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Crash";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 9000;
        dp.Position = @event.EffectPosition();
        dp.Scale = new(15);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }
}
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace Cyf5119Script.Dawntrail.M2n;

[ScriptType(guid: "4FD21978-B76C-4BF7-A3F5-D0490BB51915", name: "M2n", territorys: [1227], version: "0.0.0.3", author: "Cyf5119")]
public class M2n
{
    public void Init(ScriptAccessory accessory)
    {
    }

    [ScriptMethod(name: "AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37220|37234|37243)$"])]
    public void AOE(Event @event, ScriptAccessory accessory)
    {
        var aid = @event.ActionId();
        accessory.Method.TextInfo("AOE", aid == 37234 ? 7000 : 5000);
    }

    [ScriptMethod(name: "Tankbuster", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:00E6"])]
    public void Tankbuster(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Double Tank Cone Tankbuster", duration: 5000);
        var dp = accessory.Data.GetDefaultDrawProperties();
        var boss = accessory.Data.Objects.FirstOrDefault(x => x.DataId == 0x422A);
        dp.Name = "Tankbuster";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = boss?.EntityId ?? 0;
        dp.TargetObject = @event.TargetId();
        dp.DestoryAt = 5000;
        dp.Radian = float.Pi / 180 * 30;
        dp.Scale = new Vector2(40);
        accessory.Method.SendDraw(0, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37232|39821)$"])]
    public void Stack(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Stack";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.DestoryAt = 7000;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Donut", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(397(38|40))$"])]
    public void TemptingTwist(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Donut";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6200;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30);
        dp.InnerScale = new Vector2(7);
        dp.Radian = float.Pi * 2;
        accessory.Method.SendDraw(0, DrawTypeEnum.Donut, dp);
    }

    [ScriptMethod(name: "Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3973[79])$"])]
    public void HoneyBeeline(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Line";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6200;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(14, 60);
        accessory.Method.SendDraw(0, DrawTypeEnum.Straight, dp);
    }

    [ScriptMethod(name: "Cone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37235"])]
    public void BlowKiss(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Cone";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6000;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = float.Pi / 180 * 120;
        accessory.Method.SendDraw(0, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Poison", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37230"])]
    public void Splinter(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Poison";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4000;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Charge", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3952[56])$"])]
    public void BlindingLove(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Charge";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 7000;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8, 50);
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }
}
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace Cyf5119Script.Dawntrail.M3n;

[ScriptType(guid: "47418897-6ab6-4be4-a611-fb49f9068de8", name: "M3n", territorys: [1229], version: "0.0.0.3", author: "Linoa235")]
public class M3n
{
    public void Init(ScriptAccessory accessory)
    {
    }

    [ScriptMethod(name: "AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37846"])]
    public void BrutalImpact(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Multi-hit AOE", 5000);
    }

    [ScriptMethod(name: "Stack Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37845"])]
    public void KnuckleSandwich(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Stack Tankbuster", duration: 5000);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Stack Tankbuster";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.DestoryAt = 5000;
        dp.Scale = new Vector2(6);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37929"])]
    public void Stack(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Stack";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.DestoryAt = 5000;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Brutal Lariat", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(396(3[89]|5[2345]))$"])]
    public void BrutalLariat(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var aid = @event.ActionId();
        dp.Name = "Brutal Lariat";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = aid > 39653 ? 3100 : 6100;
        dp.Position = @event.EffectPosition();
        dp.Rotation = accessory.Data.Objects.SearchById(@event.SourceId())?.Rotation ?? 0;
        dp.Scale = new Vector2(34, 70);
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Lariat Combo Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3964[4567])$"])]
    public void LariatComboTip(Event @event, ScriptAccessory accessory)
    {
        var aid = @event.ActionId();
        accessory.Method.TextInfo(aid % 2 == 0 ? "Cross through later" : "Stay still later", 6100);
    }

    [ScriptMethod(name: "Murderous Mist", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37813"])]
    public void MurderousMist(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Murderous Mist";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5000;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.Radian = float.Pi / 180 * 270;
        accessory.Method.SendDraw(0, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Self-Destruct", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3781[67])$"])]
    public void SelfDestruct(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var aid = @event.ActionId();
        dp.Name = "Self-Destruct";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = aid == 37816 ? 0 : 5000;
        dp.DestoryAt = aid == 37816 ? 5000 : 3000;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }
}
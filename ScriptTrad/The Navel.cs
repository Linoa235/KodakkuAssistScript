using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace Cyf5119Script.ARealmReborn.TheNavel;

[ScriptType(guid: "BC3B91DA-224A-4356-B7B3-75A8366A2C1C", name: "The Navel", territorys: [1046, 293], version: "0.0.0.3", author: "Cyf5119")]
public class TheNavel
{
    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }

    [ScriptMethod(name: "Landslide", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(650|1364)$"])]
    public void Landslide(Event @event, ScriptAccessory accessory)
    {
        var r = accessory.Data.Objects.SearchById(@event.SourceId)?.HitboxRadius ?? 0;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Landslide";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.ActionId == 650 ? 3000 : 2200;
        dp.Owner = @event.SourceId;
        dp.Scale = new(6, 35 + r);
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Weight of the Land", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(973|1363)$"])]
    public void WeightOfTheLand(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Weight of the Land";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(4);
        dp.DestoryAt = @event.ActionId == 650 ? 3500 : 2500;
        dp.Position = @event.EffectPosition;
        dp.Scale = new(6);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Burst", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1052"])]
    public void Burst(Event @event, ScriptAccessory accessory)
    {
        var r = accessory.Data.Objects.SearchById(@event.SourceId)?.HitboxRadius ?? 0;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Burst";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5000;
        dp.Owner = @event.SourceId;
        dp.Scale = new(5 + r);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }
}
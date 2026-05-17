using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace Cyf5119Script.ARealmReborn.TheNavel;

[ScriptType(name: "The Navel", territorys: [], $107808eda-4c52-41e1-9a91-241a1b3dea52", version: "0.0.0.1", Author: "Linoa235", guid: "2cad1216-992d-48d3-aec0-4d1a0be6cee7")]
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
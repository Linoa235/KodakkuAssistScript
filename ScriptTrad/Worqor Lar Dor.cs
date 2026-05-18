using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using Newtonsoft.Json;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace Cyf5119Script.Dawntrail.WorqorLarDor;

[ScriptType(guid: "739f3c7d-c99b-4301-a368-0bbd754e36e3", name: "Worqor Lar Dor", territorys: [1195], version: "0.0.0.3", author: "Linoa235")]
public class WorqorLarDor
{
    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }

    [ScriptMethod(name: "Susurrant Breath", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36156"])]
    public void SusurrantBreath(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Susurrant Breath";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 7300;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(50);
        dp.Radian = float.Pi / 180 * 80;
        accessory.Method.SendDraw(0, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Slithering Strike", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36158"])]
    public void SlitheringStrike(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Slithering Strike";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 7300;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(24);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Strangling Coil", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36160"])]
    public void StranglingCoil(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Strangling Coil";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 7300;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(30);
        dp.InnerScale = new Vector2(8);
        dp.Radian = float.Pi * 2;
        accessory.Method.SendDraw(0, DrawTypeEnum.Donut, dp);
    }

    [ScriptMethod(name: "Ruinfall", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36189"])]
    public void Ruinfall(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Ruinfall";
        dp.Color = new Vector4(0.2f, 1, 1, 2);
        dp.DestoryAt = 8000;
        dp.Owner = accessory.Data.Me;
        dp.Scale = new Vector2(1, 21);
        dp.FixRotation = true;
        dp.Rotation = 0;
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Blighted Bolt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36174"])]
    public void BlightedBolt(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Blighted Bolt";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 7800;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(7);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Calamitous Cry", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(34722|26708)$"])]
    public void CalamitousCry(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Calamitous Cry";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.DestoryAt = @event.ActionId() == 34722 ? 6100 : 5000;
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Scale = new Vector2(6, 40);
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Arcane Lightning", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:16769"])]
    public void ArcaneLightning(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Arcane Lightning";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 8900;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5, 50);
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Chilling Cataclysm", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17555"])]
    public void ChillingCataclysm(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Chilling Cataclysm";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 7700;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5, 80);
        dp.FixRotation = true;
        for (int i = 0; i < 4; i++)
        {
            dp.Rotation = float.Pi / 4 * i;
            accessory.Method.SendDraw(0, DrawTypeEnum.Straight, dp);
        }
    }

    [ScriptMethod(name: "Northern Cross", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:00000002"])]
    public void NorthernCross(Event @event, ScriptAccessory accessory)
    {
        if (!(@event["Id"] == "00020001" || @event["Id"] == "00200010")) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Chilling Cataclysm";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 9100;
        dp.Scale = new Vector2(25, 60);
        dp.Rotation = -2.21f;
        if (@event["Id"] == "00020001")
            dp.Position = new Vector3(116.47f, -0.02f, 127.98f);
        else
            dp.Position = new Vector3(131.49f, -0.02f, 107.99f);
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }
}
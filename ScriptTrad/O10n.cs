using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace Cyf5119Script.Stormblood.O10n;

[ScriptType(guid: "95a112de-90fa-4edd-b1f1-e2cabbac09e6", name: "O10n", territorys: [799], version: "0.0.0.2", author: "Linoa235")]
public class O10n
{
    private bool IsCross = false;
    private uint BossId = 0;
    private bool TimeLock = false;

    public void Init(ScriptAccessory accessory)
    {
        IsCross = false;
        BossId = 0;
        TimeLock = false;
        accessory.Method.RemoveDraw(".*");
    }

    [ScriptMethod(name: "Cross Judge", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:12744"],
        userControl: false)]
    public void CrossJudge(Event @event, ScriptAccessory accessory)
    {
        IsCross = true;
        BossId = @event.SourceId();
    }

    [ScriptMethod(name: "Cross", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:12744"])]
    public void Cross(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Cross";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 9600;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(18, 60);
        accessory.Method.SendDraw(0, DrawTypeEnum.Straight, dp);
        dp.Rotation = float.Pi / 2;
        accessory.Method.SendDraw(0, DrawTypeEnum.Straight, dp);
    }

    [ScriptMethod(name: "Circle or Donut", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:12743"], userControl: false)]
    public void CircleOrDonut(Event @event, ScriptAccessory accessory)
    {
        IsCross = false;
        BossId = @event.SourceId();
    }

    [ScriptMethod(name: "Circle", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:12745"])]
    public void Circle(Event @event, ScriptAccessory accessory)
    {
        if (IsCross) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Circle";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3500;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(14);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Donut", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:12747"])]
    public void Donut(Event @event, ScriptAccessory accessory)
    {
        if (IsCross) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Donut";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3500;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40);
        dp.InnerScale = new Vector2(10);
        dp.Radian = float.Pi * 2;
        accessory.Method.SendDraw(0, DrawTypeEnum.Donut, dp);
    }

    [ScriptMethod(name: "Earth Shaker", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0028"])]
    public void EarthShaker(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Earth Shaker";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4100;
        dp.Owner = BossId;
        dp.TargetObject = @event.TargetId();
        dp.Scale = new Vector2(60);
        dp.Radian = float.Pi / 180 * 30;
        accessory.Method.SendDraw(0, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Akh Morn", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12742"])]
    public void AkhMorn(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Akh Morn";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.DestoryAt = 9300;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Cauterize", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:12865"])]
    public void Cauterize(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Cauterize";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4500;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20, 60);
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Scarlet Thread", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:9290"])]
    public async void ScarletThread(Event @event, ScriptAccessory accessory)
    {
        if (TimeLock) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Scarlet Thread";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6900;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(4, 80);
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
        await Task.Delay(1000);
        TimeLock = true;
        await Task.Delay(2000);
        TimeLock = false;
    }

    [ScriptMethod(name: "Exaflare", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:13871"])]
    public void Exaflare(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var spos = @event.SourcePosition();
        var srot = @event.SourceRotation();
        dp.Name = "Exaflare";
        dp.Scale = new Vector2(6);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = 2600;
        dp.DestoryAt = 3000;
        for (int i = 0; i < 4; i++)
        {
            spos = new Vector3(spos.X + (float)Math.Sin(srot) * 8, spos.Y, spos.Z + (float)Math.Cos(srot) * 8);
            dp.Position = spos;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Delay += 1500;
        }
    }
}
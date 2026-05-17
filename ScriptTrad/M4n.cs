using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace Cyf5119Script.Dawntrail.M4n;

[ScriptType(guid: "F4A95B34-AE13-40E4-9106-78D607BCFD57", name: "M4n", territorys: [1231], version: "0.0.0.4", author: "Cyf5119")]
public class M4n
{
    private List<bool> IsFront = [];
    private int MaxCannonTimes = 0;
    private int SparkCount = 0;

    public void Init(ScriptAccessory accessory)
    {
        IsFront = [];
        MaxCannonTimes = 0;
        SparkCount = 0;
    }

    [ScriptMethod(name: "AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37575"])]
    public void WrathOfZeus(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("AOE", 5000);
    }

    [ScriptMethod(name: "Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37576"])]
    public void WickedJolt(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Tankbuster", duration: 5000);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Tankbuster";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.DestoryAt = 5000;
        dp.Scale = new Vector2(5, 60);
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Stack", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:013C"])]
    public void Stack(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Stack";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.DestoryAt = 9000;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Stack Two", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:00A1"])]
    public void Stack2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Stack Two";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.DestoryAt = 5400;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Sidewise Spark", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:regex:^(3756[4567])$"])]
    public void SidewiseSpark(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Sidewise Spark";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = SparkCount > 1 ? 4000 : 7000;
        dp.Delay = SparkCount > 1 ? 3000 : 0;
        dp.Owner = @event.SourceId();
        dp.Rotation = @event.ActionId() % 2 == 0 ? float.Pi / -2 : float.Pi / 2;
        dp.Scale = new Vector2(60);
        dp.Radian = float.Pi;
        accessory.Method.SendDraw(0, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Clone Sidewise Spark", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:regex:^(456[68])$", "SourceDataId:16996"])]
    public async void SidewiseSparkAdds(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Sidewise Spark";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Rotation = @event.Id() == 4566 ? float.Pi / -2 : float.Pi / 2;
        dp.Scale = new Vector2(60);
        dp.Radian = float.Pi;
        dp.DestoryAt = SparkCount > 1 ? 4000 : 8000;
        dp.Delay = SparkCount > 1 ? 4000 : 0;
        accessory.Method.SendDraw(0, DrawTypeEnum.Fan, dp);

        SparkCount += 1;
        await Task.Delay(8000);
        if (SparkCount > 0)
            SparkCount -= 1;
    }

    [ScriptMethod(name: "Stampeding Thunder", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3754[78])$"])]
    public void StampedingThunder(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Stampeding Thunder";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 9300;
        dp.Position = @event.ActionId() == 37547 ? new Vector3(95, 0, 80) : new Vector3(105, 0, 80);
        dp.Scale = new Vector2(30, 40);
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Cannon Cast Bar", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(375(49|52)|39759|3976[567])$"])]
    public void CannonStartCasting(Event @event, ScriptAccessory accessory)
    {
        var aid = @event.ActionId();
        IsFront = [];
        switch (aid)
        {
            case 37549 or 37552:
                MaxCannonTimes = 3;
                break;
            case 39759 or 39765:
                MaxCannonTimes = 4;
                break;
            case 39766 or 39767:
                MaxCannonTimes = 5;
                break;
        }
    }

    [ScriptMethod(name: "Cannon", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970"])]
    public void CannonRecord(Event @event, ScriptAccessory accessory)
    {
        IsFront.Add(@event.Param() == 723);
        // 723 = North, 724 = South

        var count = IsFront.Count;
        var max = MaxCannonTimes;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Cannon {count}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Rotation = @event.Param() == 723 ? 0 : float.Pi;
        dp.Scale = new Vector2(10, 40);

        var destime = -670 * count + 2540 * max + 2100;
        if (count < 2)
        {
            dp.DestoryAt = destime + 570;
        }
        else
        {
            dp.Delay = destime - 1870;
            dp.DestoryAt = 1870;
        }

        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }


    [ScriptMethod(name: "Bewitching Flight", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37560"])]
    public void BewitchingFlight(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Bewitching Flight";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 7000;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5, 40);
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Burst", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37561"])]
    public void Burst(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Burst";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 7000;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(16, 40);
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Witch Hunt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37557"])]
    public void WitchHunt(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Witch Hunt";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6200;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }
}
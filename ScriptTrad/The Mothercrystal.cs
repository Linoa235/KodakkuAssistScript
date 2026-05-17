using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Utility.Numerics;
using Newtonsoft.Json;
using KodakkuAssist.Data;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;

namespace Cyf5119Scripts.Endwalker.TheMothercrystal;

[ScriptType(guid: "EB83FE40-4DDC-42C8-B248-CBFDF9D0E2C1", name: "The Mothercrystal", territorys: [995], version: "0.0.0.1", author: "Cyf5119")]
public class TheMothercrystal
{
    private static List<Vector3> _beaconList = [];
    private static readonly Vector3 Center = new(100f, 0f, 100f);

    public void Init(ScriptAccessory sa)
    {
        _beaconList = [];
        sa.Method.RemoveDraw(".*");
    }

    [ScriptMethod(name: "AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2607[12]|26064)$"])]
    public void Aoe(Event evt, ScriptAccessory sa)
    {
        sa.Method.TextInfo("AOE", 5000);
    }

    [ScriptMethod(name: "Exodus", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26043"])]
    public void Exodus(Event evt, ScriptAccessory sa)
    {
        sa.Method.TextInfo("Exodus", 14582, true);
    }

    [ScriptMethod(name: "Stack Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26070"])]
    public void MousasScorn(Event evt, ScriptAccessory sa)
    {
        var dp = sa.FastDp("Stack Tankbuster", evt.TargetId, 5000, 4);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        sa.Method.TextInfo("Stack Tankbuster", 5000);
    }

    [ScriptMethod(name: "Cone Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26069"])]
    public void HerossSundering(Event evt, ScriptAccessory sa)
    {
        var dp = sa.FastDp("Cone Tankbuster", evt.SourceId, 5000, 40);
        dp.TargetObject = evt.TargetId;
        dp.Radian = float.Pi / 2;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        sa.Method.TextInfo("Cone Tankbuster", 5000);
    }

    [ScriptMethod(name: "Heavy", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2273", "Param:436"])]
    public void HighestHoly(Event evt, ScriptAccessory sa)
    {
        var dp = sa.FastDp("Heavy", evt.SourceId, 6000, 10);
        dp.Color = dp.Color.WithW(0.5f);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Color = dp.Color.WithW(3);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Donut", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2273", "Param:437"])]
    public void Anthelion(Event evt, ScriptAccessory sa)
    {
        var dp = sa.FastDp("Donut", evt.SourceId, 6000, 40);
        dp.InnerScale = new Vector2(5);
        dp.Radian = float.Pi * 2;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }

    [ScriptMethod(name: "Cross", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:2273"])]
    public void Equinox(Event evt, ScriptAccessory sa)
    {
        var dp = sa.FastDp("Cross", evt.SourceId, 6000, new Vector2(10, 80));
        dp.Color = dp.Color.WithW(0.5f);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        dp.Rotation = float.Pi / 2;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

        dp.ScaleMode = ScaleMode.XByTime;
        dp.Color = dp.Color.WithW(3);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        dp.Rotation = 0;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }

    [ScriptMethod(name: "Crystalline Stone III", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27737"])]
    public void CrystallineStoneIII(Event evt, ScriptAccessory sa)
    {
        var dp = sa.FastDp("Crystalline Stone III", evt.TargetId, 5000, 6, true);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Beacon", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26062"])]
    public void Beacon(Event evt, ScriptAccessory sa)
    {
        lock (_beaconList)
        {
            _beaconList.Add(evt.EffectPosition);
            if (_beaconList.Count < 15) return;
            Vector3 wpos1 = new Vector3(0), wpos2 = new Vector3(0);
            for (var i = 0; i < 5; i++)
            {
                wpos1 += _beaconList[i + 10] - Center;
                wpos2 += _beaconList[i] - Center;
            }

            wpos1 = Vector3.Normalize(wpos1) * -8 + Center;
            wpos2 = Vector3.Normalize(wpos2) * 8 + Center;

            var dp = sa.FastDp("Beacon", sa.Data.Me, 1, 2, true);
            dp.ScaleMode = ScaleMode.YByDistance;

            dp.TargetPosition = wpos1;
            dp.Delay = 5200;
            dp.DestoryAt = 12700;
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp.TargetPosition = wpos2;
            dp.Delay = 17900;
            dp.DestoryAt = 3200;
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
    }

    [ScriptMethod(name: "Parhelic Circle", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:regex:^(201172[45])$", "Operate:Add"])]
    public void ParhelicCircle(Event evt, ScriptAccessory sa)
    {
        var dp = sa.FastDp("Parhelic Circle", Center, 9300, 6);
        if (evt["DataId"] == "2011724")
        {
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            for (var i = 0; i < 6; i++)
            {
                dp.Position = Vector3.Transform(new Vector3(0, 0, 17), Matrix4x4.CreateRotationY(float.Pi / 3 * i + evt.SourceRotation)) + Center;
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }
        else
        {
            for (var i = 0; i < 3; i++)
            {
                dp.Position = Vector3.Transform(new Vector3(0, 0, 8), Matrix4x4.CreateRotationY(float.Pi / 3 * (2 * i + 1) + evt.SourceRotation)) + Center;
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }
    }

    [ScriptMethod(name: "Light Wave", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2011723", "Operate:Add"], suppress: 5000)]
    public void LightWave(Event evt, ScriptAccessory sa)
    {
        foreach (var actor in sa.Data.Objects.Where(x => x.DataId == 9020 && Vector3.Distance(x.Position, new Vector3(100, 0, 100)) > 20))
        {
            var dp = sa.FastDp("Light Wave", actor.EntityId, 15000, new Vector2(16, 60));
            dp.Offset = new Vector3(0, 0, 7.5f);
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
    }

    [ScriptMethod(name: "Echoes", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0131"])]
    public void Echoes(Event evt, ScriptAccessory sa)
    {
        var dp = sa.FastDp("Echoes", evt.TargetId, 10000, 6, true);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
}
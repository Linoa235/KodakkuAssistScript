using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using KodakkuAssist.Data;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;

namespace Cyf5119Script.Endwalker.TheGildedAraya;

[ScriptType(guid: "c1afaf1a-dd0b-472e-94d3-c845839c422a", name: "The Gilded Araya", territorys: [1136], version: "0.0.0.1", author: "Linoa235")]
public class TheGildedAraya
{
    public void Init(ScriptAccessory sa)
    {
        sa.Method.RemoveDraw(".*");
    }
    
    [ScriptMethod(name: "Lower Realm", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36001"])]
    public void LowerRealm(Event evt, ScriptAccessory sa)
    {
        sa.Method.TextInfo("AOE", 5000);
    }
    
    [ScriptMethod(name: "Ephemerality", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35990"])]
    public void Ephemerality(Event evt, ScriptAccessory sa)
    {
        sa.Method.TextInfo("AOE", 5000);
    }
    
    [ScriptMethod(name: "Laceration", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:35990", "TargetIndex:1"])]
    public void Laceration(Event evt, ScriptAccessory sa)
    {
        var targets = sa.Data.Objects.Where(x => x.DataId == 0x40F8);
        foreach (var target in targets)
        {
            var dp = sa.FastDp("Laceration", target.GameObjectId, 7300, 9);
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "Cutting Jewel", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36000"])]
    public void CuttingJewel(Event evt, ScriptAccessory sa)
    {
        sa.Method.TextInfo("Tankbuster", 5000);
    }
    
    [ScriptMethod(name: "Pedestal Purge", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35970"])]
    public void PedestalPurge(Event evt, ScriptAccessory sa)
    {
        var dp = sa.FastDp("Pedestal Purge", evt.EffectPosition, 4000, 60);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Wheel of Deincarnation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35972"])]
    public void WheelOfDeincarnation(Event evt, ScriptAccessory sa)
    {
        var dp = sa.FastDp("Wheel of Deincarnation", evt.EffectPosition, 4000, 96);
        dp.InnerScale = new Vector2(48);
        dp.Radian = float.Pi * 2;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Bladewise", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35974"])]
    public void Bladewise(Event evt, ScriptAccessory sa)
    {
        var dp = sa.FastDp("Bladewise", evt.SourceId, 4000, new Vector2(28, 100));
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "Khadga", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3601[123])$"])]
    public void Khadga(Event evt, ScriptAccessory sa)
    {
        var dp = sa.FastDp("Khadga", evt.SourceId, 2000, 20);
        dp.Delay = 11000;
        dp.Radian = float.Pi;
        if (evt.ActionId > 36011)
            dp.Rotation = float.Pi / (evt.ActionId > 36012 ? 2 : -2);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "The Face", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3602[23])$"])]
    public void TheFace(Event evt, ScriptAccessory sa)
    {
        var taid = evt.ActionId < 36023 ? 36015 : 36016;
        var target = sa.Data.Objects.FirstOrDefault(x => x is IBattleChara y && y.CastActionId == taid);
        if (target == null) return;
        
        var dp = sa.FastDp("The Face", target.GameObjectId, 8000, 20);
        dp.Radian = float.Pi;
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
    }
}

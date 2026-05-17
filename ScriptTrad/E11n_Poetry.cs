using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading;
using Dalamud.Game.ClientState.Objects.Types;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using ECommons;
using ECommons.DalamudServices;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace KodakkuScript.Script._05_Shadowbringers;

[ScriptType(guid:"e12185b8-2930-44a8-8e1b-9f58c3fd6e0a",name:"E11n", territorys: [944], version: "0.0.0.1", 
    author: "Poetry")]
public class E11n
{
    private static bool ParseObjectId(string? idStr, out uint id)
    {
        id = 0;
        if (string.IsNullOrEmpty(idStr)) return false;
        try
        {
            var idStr2 = idStr.Replace("0x", "");
            id = uint.Parse(idStr2, System.Globalization.NumberStyles.HexNumber);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }
    
    // 前后分开绘制
    [ScriptMethod(name:"燃烧击：直线",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:regex:^(2206[024])$"])]
    public void 燃烧击_直线 (Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"燃烧击：直线-1";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = sid;
        dp1.DestoryAt = 8000;
        dp1.ScaleMode = ScaleMode.ByTime;
        dp1.Scale = new(10,80); 
        var dp2 = accessory.Data.GetDefaultDrawProperties();;
        dp2.Name = $"燃烧击：直线-2";
        dp2.Color = accessory.Data.DefaultDangerColor;
        dp2.Owner = sid;
        dp2.ScaleMode = ScaleMode.ByTime;
        dp2.DestoryAt = 8000;
        dp2.Scale = new(10,80);
        dp2.Rotation = float.Pi;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp1);
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp2);
    }
    
    // 22061
    // 22084 分身击退
    [ScriptMethod(name:"火燃爆：击退",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:regex:^(220(61|84))$"])]
    public void 火燃爆_击退 (Event @event, ScriptAccessory accessory)
    {
        Thread.Sleep(6000);
        accessory.Method.TextInfo($"打完穿进击退!", 5000);
    }
    
    // 22063 扩散直线
    // 22086 分身扩散直线
    [ScriptMethod(name:"雷燃爆：扩散直线",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:regex:^(220(63|86))$"])]
    public void 雷燃爆_扩散直线 (Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"雷燃爆：扩散直线-1";
        dp1.Color = new Vector4(1f,0.886f,0f,1f);
        dp1.Owner = sid;
        dp1.ScaleMode = ScaleMode.ByTime;
        dp1.DestoryAt = 9700;
        dp1.Scale = new(20,80); 
        var dp2 = accessory.Data.GetDefaultDrawProperties();;
        dp2.Name = $"雷燃爆：扩散直线-2";
        dp2.Color = new Vector4(1f,0.886f,0f,1f);
        dp2.Owner = sid;
        dp2.ScaleMode = ScaleMode.ByTime;
        dp2.DestoryAt = 9700;
        dp2.Scale = new(20,80);
        dp2.Rotation = float.Pi;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp1);
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp2);
    }

    // 22075 小光轮
    [ScriptMethod(name: "光炎：小光轮", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22075"])]
    public void 光炎_小光轮 (Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"光炎：小光轮";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.Owner = sid;
        dp.Scale = new(5);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
    }

    // 22076 大光轮
    [ScriptMethod(name: "光炎：大光轮", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22076"])]
    public void 光炎_大光轮 (Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"光炎：大光轮";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.Owner = sid;
        dp.Scale = new(10);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
    }
    
    // 22079
    
    // 22083
    // 22085 分身燃烧击
    [ScriptMethod(name: "分身燃烧击：直线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2208[35])$"])]
    public void 分身燃烧击 (Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"分身燃烧击：直线";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = sid;
        dp.DestoryAt = 8000;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new(10,80);
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp);
    }
    
    // StatusID：1678 获取幻影的位置
    // 22097 绝命战士的幻影：爆破领域
    [ScriptMethod(name:"幻影：爆破领域",eventType:EventTypeEnum.StatusAdd,eventCondition:["StatusID:1678"])]
    public void 幻影_爆破领域(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"幻影_爆破领域";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = tid;
        dp.DestoryAt = 14000;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new(16, 80);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

}
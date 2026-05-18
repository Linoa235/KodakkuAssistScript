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

[ScriptType(guid:"c01cd7a2-4f1c-4c10-9a9d-2559db1c6623",name:"E11n", territorys: [944], version: "0.0.0.1", 
    author: "Linoa235")]
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
    
    // å‰åŽåˆ†å¼€ç»˜åˆ¶
    [ScriptMethod(name:"ç‡ƒçƒ§å‡»ï¼šç›´çº¿",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:regex:^(2206[024])$"])]
    public void ç‡ƒçƒ§å‡»_ç›´çº¿ (Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"ç‡ƒçƒ§å‡»ï¼šç›´çº¿-1";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = sid;
        dp1.DestoryAt = 8000;
        dp1.ScaleMode = ScaleMode.ByTime;
        dp1.Scale = new(10,80); 
        var dp2 = accessory.Data.GetDefaultDrawProperties();;
        dp2.Name = $"ç‡ƒçƒ§å‡»ï¼šç›´çº¿-2";
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
    // 22084 åˆ†èº«å‡»é€€
    [ScriptMethod(name:"ç«ç‡ƒçˆ†ï¼šå‡»é€€",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:regex:^(220(61|84))$"])]
    public void ç«ç‡ƒçˆ†_å‡»é€€ (Event @event, ScriptAccessory accessory)
    {
        Thread.Sleep(6000);
        accessory.Method.TextInfo($"æ‰“å®Œç©¿è¿›å‡»é€€!", 5000);
    }
    
    // 22063 æ‰©æ•£ç›´çº¿
    // 22086 åˆ†èº«æ‰©æ•£ç›´çº¿
    [ScriptMethod(name:"é›·ç‡ƒçˆ†ï¼šæ‰©æ•£ç›´çº¿",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:regex:^(220(63|86))$"])]
    public void é›·ç‡ƒçˆ†_æ‰©æ•£ç›´çº¿ (Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"é›·ç‡ƒçˆ†ï¼šæ‰©æ•£ç›´çº¿-1";
        dp1.Color = new Vector4(1f,0.886f,0f,1f);
        dp1.Owner = sid;
        dp1.ScaleMode = ScaleMode.ByTime;
        dp1.DestoryAt = 9700;
        dp1.Scale = new(20,80); 
        var dp2 = accessory.Data.GetDefaultDrawProperties();;
        dp2.Name = $"é›·ç‡ƒçˆ†ï¼šæ‰©æ•£ç›´çº¿-2";
        dp2.Color = new Vector4(1f,0.886f,0f,1f);
        dp2.Owner = sid;
        dp2.ScaleMode = ScaleMode.ByTime;
        dp2.DestoryAt = 9700;
        dp2.Scale = new(20,80);
        dp2.Rotation = float.Pi;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp1);
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp2);
    }

    // 22075 å°å…‰è½®
    [ScriptMethod(name: "å…‰ç‚Žï¼šå°å…‰è½®", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22075"])]
    public void å…‰ç‚Ž_å°å…‰è½® (Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"å…‰ç‚Žï¼šå°å…‰è½®";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.Owner = sid;
        dp.Scale = new(5);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
    }

    // 22076 å¤§å…‰è½®
    [ScriptMethod(name: "å…‰ç‚Žï¼šå¤§å…‰è½®", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22076"])]
    public void å…‰ç‚Ž_å¤§å…‰è½® (Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"å…‰ç‚Žï¼šå¤§å…‰è½®";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.Owner = sid;
        dp.Scale = new(10);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
    }
    
    // 22079
    
    // 22083
    // 22085 åˆ†èº«ç‡ƒçƒ§å‡»
    [ScriptMethod(name: "åˆ†èº«ç‡ƒçƒ§å‡»ï¼šç›´çº¿", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2208[35])$"])]
    public void åˆ†èº«ç‡ƒçƒ§å‡» (Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"åˆ†èº«ç‡ƒçƒ§å‡»ï¼šç›´çº¿";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = sid;
        dp.DestoryAt = 8000;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new(10,80);
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp);
    }
    
    // StatusIDï¼š1678 èŽ·å–å¹»å½±çš„ä½ç½®
    // 22097 ç»å‘½æˆ˜å£«çš„å¹»å½±ï¼šçˆ†ç ´é¢†åŸŸ
    [ScriptMethod(name:"å¹»å½±ï¼šçˆ†ç ´é¢†åŸŸ",eventType:EventTypeEnum.StatusAdd,eventCondition:["StatusID:1678"])]
    public void å¹»å½±_çˆ†ç ´é¢†åŸŸ(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"å¹»å½±_çˆ†ç ´é¢†åŸŸ";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = tid;
        dp.DestoryAt = 14000;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new(16, 80);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

}
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

[ScriptType(guid: "0a76aa07-cac9-4442-91f9-2d964d63bd28",name:"E11n", territorys: [944], version: "0.0.0.1", 
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
    
    // Draw front and back separately
    [ScriptMethod(name:"Burning Strike: Line", eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:regex:^(2206[024])$"])]
    public void BurningStrike_Line(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"Burning Strike: Line-1";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = sid;
        dp1.DestoryAt = 8000;
        dp1.ScaleMode = ScaleMode.ByTime;
        dp1.Scale = new(10,80); 
        var dp2 = accessory.Data.GetDefaultDrawProperties();;
        dp2.Name = $"Burning Strike: Line-2";
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
    // 22084 Clone Knockback
    [ScriptMethod(name:"Fire Explosion: Knockback", eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:regex:^(220(61|84))$"])]
    public void FireExplosion_Knockback(Event @event, ScriptAccessory accessory)
    {
        Thread.Sleep(6000);
        accessory.Method.TextInfo($"Move through after knockback!", 5000);
    }
    
    // 22063 Spread Line
    // 22086 Clone Spread Line
    [ScriptMethod(name:"Lightning Explosion: Spread Line", eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:regex:^(220(63|86))$"])]
    public void LightningExplosion_SpreadLine(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"Lightning Explosion: Spread Line-1";
        dp1.Color = new Vector4(1f,0.886f,0f,1f);
        dp1.Owner = sid;
        dp1.ScaleMode = ScaleMode.ByTime;
        dp1.DestoryAt = 9700;
        dp1.Scale = new(20,80); 
        var dp2 = accessory.Data.GetDefaultDrawProperties();;
        dp2.Name = $"Lightning Explosion: Spread Line-2";
        dp2.Color = new Vector4(1f,0.886f,0f,1f);
        dp2.Owner = sid;
        dp2.ScaleMode = ScaleMode.ByTime;
        dp2.DestoryAt = 9700;
        dp2.Scale = new(20,80);
        dp2.Rotation = float.Pi;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp1);
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp2);
    }

    // 22075 Small Light Ring
    [ScriptMethod(name: "Light Blaze: Small Light Ring", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22075"])]
    public void LightBlaze_SmallRing(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Light Blaze: Small Light Ring";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.Owner = sid;
        dp.Scale = new(5);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
    }

    // 22076 Large Light Ring
    [ScriptMethod(name: "Light Blaze: Large Light Ring", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22076"])]
    public void LightBlaze_LargeRing(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Light Blaze: Large Light Ring";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.Owner = sid;
        dp.Scale = new(10);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
    }
    
    // 22079
    
    // 22083
    // 22085 Clone Burning Strike
    [ScriptMethod(name: "Clone Burning Strike: Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2208[35])$"])]
    public void CloneBurningStrike(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Clone Burning Strike: Line";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = sid;
        dp.DestoryAt = 8000;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new(10,80);
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp);
    }
    
    // StatusID: 1678 Get clone position
    // 22097 Phantom of the Doom Warrior: Blast Zone
    [ScriptMethod(name:"Phantom: Blast Zone", eventType:EventTypeEnum.StatusAdd, eventCondition:["StatusID:1678"])]
    public void Phantom_BlastZone(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Phantom_BlastZone";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = tid;
        dp.DestoryAt = 14000;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new(16, 80);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

}
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

[ScriptType(guid:"c00d50b8-a549-470a-96d8-9891f2c6a133",name:"E11n", territorys: [944], version: "0.0.0.1", 
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
    
    [ScriptMethod(name:"Burning Strike: Line",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:regex:^(2206[024])$"])]
    public void BurningStrikeLine(Event @event, ScriptAccessory accessory)
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
    
    [ScriptMethod(name:"Fire Explosion: Knockback",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:regex:^(220(61|84))$"])]
    public void FireExplosionKnockback(Event @event, ScriptAccessory accessory)
    {
        Thread.Sleep(6000);
        accessory.Method.TextInfo($"Go through after hit then knockback!", 5000);
    }
    
    [ScriptMethod(name:"Lightning Explosion: Spread Line",eventType:EventTypeEnum.StartCasting, eventCondition:["ActionId:regex:^(220(63|86))$"])]
    public void LightningExplosionSpreadLine(Event @event, ScriptAccessory accessory)
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

    [ScriptMethod(name: "Light Flame: Small Wheel", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22075"])]
    public void LightFlameSmallWheel(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Light Flame: Small Wheel";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.Owner = sid;
        dp.Scale = new(5);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
    }

    [ScriptMethod(name: "Light Flame: Large Wheel", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:22076"])]
    public void LightFlameLargeWheel(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Light Flame: Large Wheel";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.Owner = sid;
        dp.Scale = new(10);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
    }
    
    [ScriptMethod(name: "Copy Burning Strike: Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2208[35])$"])]
    public void CopyBurningStrike(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Copy Burning Strike: Line";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = sid;
        dp.DestoryAt = 8000;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new(10,80);
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp);
    }
    
    [ScriptMethod(name:"Phantom: Burst Field",eventType:EventTypeEnum.StatusAdd,eventCondition:["StatusID:1678"])]
    public void PhantomBurstField(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Phantom_BurstField";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = tid;
        dp.DestoryAt = 14000;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new(16, 80);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
}

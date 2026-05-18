using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.GameEvent.Types;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Script;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace BakaWater77.M11N;

[ScriptType(
       name: "M11N",
       territorys: new uint[] { 1324 },
       guid: "bf37625f-d41e-4c8f-8664-104015cdd199",
       version: "0.0.0.1",
       author: "Linoa235",
       note: null
    )]
public class ArcadiaCruiserweightM3
{
    public bool isText { get; set; } = true;
    
    private static bool ParseObjectId(string? idStr, out uint id)
    {
        id = 0;
        if (string.IsNullOrEmpty(idStr)) return false;

        try
        {
            id = uint.Parse(idStr.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
            return true;
        }
        catch
        {
            return false;
        }
    }

    [ScriptMethod(
        name: "Zenith's Reign",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(46006)$" },
        userControl: true
    )]
    public void ZenithsReign(Event @event, ScriptAccessory accessory)
    {
        if (isText)
            accessory.Method.TextInfo("AOE", duration: 4700);
    }

    [ScriptMethod(
        name: "Weapon Call: Assault",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(46008|46007|46009)$" },
        userControl: true
    )]
    public void WeaponCallAssault(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        
        if (@event.ActionId == 46008)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Donut";
            dp.Owner = sid;
            dp.Scale = new Vector2(5);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        else if (@event.ActionId == 46007)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Chariot";
            dp.Owner = sid;
            dp.Scale = new Vector2(8);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(
        name: "Weapon Call: Cross",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(46009)$" },
        userControl: true
    )]
    public void Cross(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        float[] rotations = { @event.SourceRotation, @event.SourceRotation + MathF.PI, @event.SourceRotation + MathF.PI / 2, @event.SourceRotation - MathF.PI / 2 };
        
        foreach (var rot in rotations)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cross";
            dp.Position = @event.EffectPosition;
            dp.Scale = new Vector2(10, 60);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Rotation = rot;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
    }
    
    [ScriptMethod(
        name: "Sequential Chariot/Donut/Cross by Tether",
        eventType: EventTypeEnum.SetObjPos,
        eventCondition: new[] { "Id:regex:^(0197)$" },
        userControl: true
    )]
    public void SequentialChariotDonutCross(Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceDataId() == 19185)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Donut";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(5);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 17000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        else if (@event.SourceDataId() == 19184)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Chariot";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(8);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 17000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        else if (@event.SourceDataId() == 19186)
        {
            float[] rotations = { @event.SourceRotation, @event.SourceRotation + MathF.PI, @event.SourceRotation + MathF.PI / 2, @event.SourceRotation - MathF.PI / 2 };
            foreach (var rot in rotations)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "Cross";
                dp.Position = @event.SourcePosition;
                dp.Scale = new Vector2(10, 60);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Rotation = rot;
                dp.DestoryAt = 17000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }
    }
    
    [ScriptMethod(
        name: "Weapon Strike Chariot/Donut/Cross Judgment",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(46031|46030|46032)$" },
        userControl: true
    )]
    public async void WeaponStrikeJudgment(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(500);

        if (@event.ActionId == 46031)
        {
            accessory.Method.RemoveDraw("Donut.*");
        }
        else if (@event.ActionId == 46030)
        {
            accessory.Method.RemoveDraw("Chariot.*");
        }
        else if (@event.ActionId == 46032)
        {
            accessory.Method.RemoveDraw("Cross.*");
        }
    }
    
    [ScriptMethod(
        name: "Clear Drawings",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(46028)$" },
        userControl: true
    )]
    public void ClearDrawings(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }
}

public static class EventExtensions
{
    public static uint SourceDataId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["SourceDataId"]);
    }
}
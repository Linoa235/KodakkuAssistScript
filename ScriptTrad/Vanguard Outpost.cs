using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Threading;
using Dalamud.Game.ClientState.Objects.Types;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using ECommons;
using ECommons.DalamudServices;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace KodakkuScript.Script._07_DawnTrail;

[ScriptType(guid: "e3b0c442-98fc-1c14-9ddf-4b9b8a8f1a1f", name: "Vanguard Outpost", territorys: [1198], version: "0.0.0.1",
    Author: "Linoa235")]
public class Vanguard
{
    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }
    
    // Boss1
    [ScriptMethod(name: "Enhanced Mobility: Heavy", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(36559|39141)$"])]
    public void EnhancedMobility1(Event @event,ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Enhanced Mobility: Heavy";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = sid;
        dp.Scale = new(17);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(5);
        dp.DestoryAt = 9000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
    }

    [ScriptMethod(name:"Enhanced Mobility: Donut",eventType:EventTypeEnum.StartCasting,eventCondition:["ActionId:regex:^(36560|39140)$"])]
    public void EnhancedMobility2(Event @event,ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Enhanced Mobility: Donut";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = sid;
        dp.InnerScale = new(14);
        dp.Scale = new(60);
        dp.Radian = float.Pi;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3500;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Donut,dp);
    }

    [ScriptMethod(name: "Dispatch: Rush", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36569"])]
    public void DispatchRush(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Dispatch: Rush";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new(5,40);
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp);
    }

   [ScriptMethod(name: "Dispatch: Airstrike", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36570"])]
    public void DispatchAirstrike(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dms = JsonConvert.DeserializeObject<int>(@event["DurationMilliseconds"]);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Dispatch: Airstrike";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
        dp.Scale = new(14);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.DestoryAt = dms+100;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
    }

    // Boss2
    [ScriptMethod(name:"Dynamic Induction Bomb",eventType:EventTypeEnum.StatusAdd,eventCondition:["StatusID:3802"])]
    public void DynamicInductionBomb(Event @event,ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        var dms = JsonConvert.DeserializeObject<int>(@event["DurationMilliseconds"]);
        if (tid == accessory.Data.Me)
        {
            accessory.Method.TTS("Stop moving soon");
            Thread.Sleep(dms-3000);
            accessory.Method.TextInfo("Stop moving", 3000);
            accessory.Method.TTS("Stop moving");
        }
    }
    
    //Boss3
    [ScriptMethod(name:"Soulbane Saber",eventType:EventTypeEnum.StartCasting,eventCondition:["ActionId:regex:^(36574|36589|36590)$"])]
    public void SoulbaneSaber(Event @event,ScriptAccessory accessory)
    {
    }

    [ScriptMethod(name:"Soulbane Saber Burst: Half Room",eventType:EventTypeEnum.StartCasting,eventCondition:["ActionId:regex:^365(75|91)$"])]
    public void SoulbaneSaberBurst(Event @event,ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dms = JsonConvert.DeserializeObject<int>(@event["DurationMilliseconds"]);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Soulbane Saber Burst";
        dp.Owner = sid;
        dp.Scale = new(19);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.DestoryAt = dms;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Radian = float.Pi;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Fan,dp);
    }

    [ScriptMethod(name:"Syntheslither",eventType:EventTypeEnum.StartCasting,eventCondition:["ActionId:regex:^3658[0-8]$"])]
    public void Syntheslither(Event @event,ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        var dms = JsonConvert.DeserializeObject<int>(@event["DurationMilliseconds"]);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Syntheslither";
        dp.Owner = tid;
        dp.Scale = new(19);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = dms;
        dp.Radian = float.Pi/2;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Fan,dp);
        
    }
    
    [ScriptMethod(name:"Slitherbane",eventType:EventTypeEnum.StartCasting,eventCondition:["ActionId:regex:^3659[23]$"])]
    public void Slitherbane(Event @event,ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
        var dms = JsonConvert.DeserializeObject<int>(@event["DurationMilliseconds"]);
        bool isFront = aid % 2 == 0;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"{(isFront ? "Front" : "Rear")} Slitherbane";
        dp.Color = new Vector4(1f,0.886f,0f,1f);
        dp.Owner = sid;
        dp.Scale = new(19);
        dp.DestoryAt = dms;
        dp.Radian =float.Pi;
        dp.Rotation = isFront ? 0 : float.Pi;
        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Fan,dp);
    }


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
}
using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Module.GameEvent.Types;
using KodakkuAssist.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;
using Dalamud.Utility.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;

namespace BakaWater77.M10N;

[ScriptType(
       name: "M10N",
       territorys: new uint[] { 1322 },
       guid: "DC98AE77-83FB-4B76-ACA7-45BBCF05DEFE",
       version: "0.0.0.2",
       author: "Baka-Water77",
       note: null
    )]
public class M10N
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

    private static readonly Vector3 Center = new Vector3(100, 0, 100);
    private static List<uint> _elementSnake = [];
    private static int _elementSnakeTargetCount = 0;
    private ManualResetEvent _elementSnakeManualEvent = new ManualResetEvent(false);

    public void Init(ScriptAccessory sa)
    {
        RefreshParams();
        sa.Method.RemoveDraw(".*");
        sa.Method.ClearFrameworkUpdateAction(this);
    }

    private void RefreshParams()
    {
        _elementSnake = [];
        _elementSnakeTargetCount = 0;
        _elementSnakeManualEvent = new ManualResetEvent(false);
    }

    [ScriptMethod(name: "———————— Test Items ————————", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void TestItemsSeparator(Event ev, ScriptAccessory sa) { }

    [ScriptMethod(name: "Test Snake Range", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void TestSnakeRange(Event ev, ScriptAccessory sa)
    {
        sa.DrawFan(new Vector3(87, 0, 113), sa.Data.Me, 0, 2000, $"Snake1211", 30f.DegToRad(), 0f, 40, 0);
    }

    [ScriptMethod(name: "Test TargetIcon", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(029[56]|027[BC])$"],
        userControl: Debugging)]
    public void TestTargetIcon(Event ev, ScriptAccessory sa)
    {
        var a = uint.Parse(ev["Id"], System.Globalization.NumberStyles.HexNumber);
    }

    [ScriptMethod(name: "———————— Snake ————————", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: true)]
    public void SnakeSeparator(Event ev, ScriptAccessory sa) { }

    [ScriptMethod(name: "Locate Snake", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:regex:^(2|512)$", "Index:regex:^(1[456789]|2[012])$"],
        userControl: Debugging)]
    public void LocateSnake(Event ev, ScriptAccessory sa)
    {
        const uint WATER_ELEMENT = 2;
        const uint FIRE_ELEMENT = 512;

        lock (_elementSnake)
        {
            var elementTypeVal = JsonConvert.DeserializeObject<uint>(ev["Flag"]) == FIRE_ELEMENT ? 10 : 0;
            var region = JsonConvert.DeserializeObject<uint>(ev["Index"]) - 14;
            var val = (uint)(region + elementTypeVal);
            _elementSnake.Add(val);

            if (_elementSnake.Count != 2) return;
            _elementSnakeManualEvent.Set();
        }
    }

    [ScriptMethod(name: "Spinning Snake Range Drawing", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(029[56]|027[BC])$"],
        userControl: true)]
    public void GetMarker(Event ev, ScriptAccessory sa)
    {
        _elementSnakeManualEvent.WaitOne();

        lock (_elementSnake)
        {
            var isWaterIcon = uint.Parse(ev["Id"], System.Globalization.NumberStyles.HexNumber) is 0x0295 or 0x027B;
            var targetIndex = sa.Data.PartyList.IndexOf((uint)ev.TargetId);

            foreach (var snakeVal in _elementSnake)
            {
                if ((isWaterIcon && snakeVal >= 10) || (!isWaterIcon && snakeVal < 10)) continue;
                var region = snakeVal % 10;
                var regionCenter = new Vector3(87 + 13 * (region % 3), 0, 87 + 13 * (int)(region / 3));
                sa.DrawFan(regionCenter, ev.TargetId, 0, 10000, $"Snake Marker{targetIndex}", 30f.DegToRad(), 0f, 60, 0);
            }
            _elementSnakeTargetCount++;
            if (_elementSnakeTargetCount < 4) return;
            _elementSnakeTargetCount = 0;
            _elementSnake.Clear();
            _elementSnakeManualEvent.Reset();
        }
    }

    [ScriptMethod(name: "Snake Drawing Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["TargetIndex:1", "ActionId:regex:^(4650[56])$"],
        userControl: Debugging)]
    public void SnakeDrawingRemove(Event ev, ScriptAccessory sa)
    {
        var targetIndex = sa.Data.PartyList.IndexOf((uint)ev.TargetId);
        sa.Method.RemoveDraw($"Snake Marker{targetIndex}");
    }

    [ScriptMethod(
        name: "Spirited",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(46466|46467)$" },
        userControl: true
    )]
    public void Spirited(Event @event, ScriptAccessory accessory)
    {
        if (isText)
            accessory.Method.TextInfo("AOE", duration: 4700);
    }

    [ScriptMethod(
        name: "Wave Spin",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(46488)$" },
        userControl: true
    )]
    public void WaveSpin(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var sourceObj = accessory.Data.Objects.FirstOrDefault(x => x.GameObjectId == sid);
        if (sourceObj == null) return;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Wave Spin";
        dp.Owner = sid;
        dp.Scale = new Vector2(60);
        dp.Rotation = sourceObj.Rotation;
        dp.Position = sourceObj.Position;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6500;
        dp.Radian = MathF.PI * 2f / 3f;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(
        name: "Riding the Wave",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(46483)$" },
        userControl: true
    )]
    public void RidingTheWave(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var sourceObj = accessory.Data.Objects.FirstOrDefault(x => x.GameObjectId == sid);
        if (sourceObj == null) return;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Riding the Wave";
        dp.Owner = sid;
        dp.Scale = new Vector2(15f, 50f);
        dp.Position = @event.EffectPosition;
        dp.Rotation = sourceObj.Rotation;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(
        name: "Mixed Explosion",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(46507)$" },
        userControl: true
    )]
    public void MixedExplosion(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Mixed Explosion";
        dp.Owner = sid;
        dp.Scale = new Vector2(9);
        dp.Position = @event.EffectPosition;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
}

#region Function Collection

#region Math Functions

public static class MathTools
{
    public static float DegToRad(this float deg) => (deg + 360f) % 360f / 180f * float.Pi;
    public static float RadToDeg(this float rad) => (rad + 2 * float.Pi) % (2 * float.Pi) / float.Pi * 180f;
}

#endregion

#region Drawing Functions

public static class DrawTools
{
    public static DrawPropertiesEdit DrawOwnerBase(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name,
        float radian, float rotation, float width, float length, float innerWidth, float innerLength,
        DrawModeEnum drawModeEnum, DrawTypeEnum drawTypeEnum, bool isSafe = false,
        bool byTime = false, bool byY = false, bool draw = true)
    {
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.InnerScale = new Vector2(innerWidth, innerLength);
        dp.Radian = radian;
        dp.Rotation = rotation;
        dp.Color = isSafe ? sa.Data.DefaultSafeColor : sa.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        dp.ScaleMode |= byY ? ScaleMode.YByDistance : ScaleMode.None;

        switch (ownerObj)
        {
            case uint u:
                dp.Owner = u;
                break;
            case ulong ul:
                dp.Owner = ul;
                break;
            case Vector3 spos:
                dp.Position = spos;
                break;
            default:
                throw new ArgumentException($"ownerObj {ownerObj} target type error");
        }

        switch (targetObj)
        {
            case 0:
            case 0u:
                break;
            case uint u:
                dp.TargetObject = u;
                break;
            case ulong ul:
                dp.TargetObject = ul;
                break;
            case Vector3 tpos:
                dp.TargetPosition = tpos;
                break;
            default:
                throw new ArgumentException($"targetObj {targetObj} target type error");
        }

        if (draw)
            sa.Method.SendDraw(drawModeEnum, drawTypeEnum, dp);
        return dp;
    }

    public static DrawPropertiesEdit DrawFan(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name, float radian, float rotation,
        float outScale, float innerScale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, radian, rotation, outScale, outScale, innerScale,
            innerScale, DrawModeEnum.Default, innerScale == 0 ? DrawTypeEnum.Fan : DrawTypeEnum.Donut, isSafe, byTime, false, draw);

    public static DrawPropertiesEdit DrawFan(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name, float radian, float rotation,
        float outScale, float innerScale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawFan(ownerObj, 0, delay, destroy, name, radian, rotation, outScale, innerScale, isSafe, byTime, draw);
}

#endregion

#endregion
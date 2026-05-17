using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Data;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace Cyf5119Script.Dawntrail.M1n;

[ScriptType(name: "M1n", territorys: [], $19005ff39-a283-4f9f-8b5f-13fa44297bbe", version: "0.0.0.1", Author: "Linoa235", guid: "b1c9ba15-8f93-4ae5-ad12-8cd4aa25637f")]
public class M1n
{
    private uint tethered = 0;

    public void Init(ScriptAccessory accessory)
    {
        tethered = 0;
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

    [ScriptMethod(name: "AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37696"])]
    public void BloodyScratch(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("AOE", duration: 5000);
    }

    [ScriptMethod(name: "Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37706"])]
    public void BiscuitMaker(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Tankbuster", duration: 5000);
    }

    [ScriptMethod(name: "Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37693"])]
    public void Clawful(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        dp.Name = "Stack";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.DestoryAt = 5000;
        dp.Owner = tid;
        dp.Scale = new Vector2(5);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "One Two Paw", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3764[2356])$"])]
    public void OneTwoPaw(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
        bool isFirst = new List<uint> { 37642, 37646 }.Contains(aid);
        dp.Name = "One Two Paw";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = isFirst ? 0 : 6800;
        dp.DestoryAt = isFirst ? 6800 : 3000;
        dp.Owner = sid;
        dp.Scale = new Vector2(60);
        dp.Radian = float.Pi;
        dp.Rotation = new List<uint> { 37643, 37646 }.Contains(aid) ? float.Pi / 2 : float.Pi / -2;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Black Cat Crossing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(376(49|50))$"])]
    public void BlackCatCrossing(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
        bool isFirst = aid == 37649;
        dp.Name = "Black Cat Crossing";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = isFirst ? 0 : 4000;
        dp.DestoryAt = isFirst ? 7000 : 3000;
        dp.Owner = sid;
        dp.Scale = new Vector2(60);
        dp.Radian = float.Pi / 4;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Mouser", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3(7653|9275))$"])]
    public void Mouser(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
        var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        dp.Name = $"Mouser {spos}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(aid == 37653 ? 1 : 3);
        dp.Delay = 10400 - 4000;
        dp.DestoryAt = 4000;
        dp.Position = spos;
        dp.Rotation = 0;
        dp.FixRotation = true;
        dp.Scale = new Vector2(10);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }

    [ScriptMethod(name: "Knockback Prediction", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37661"])]
    public void Shockwave(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        dp.Name = "Knockback Prediction";
        dp.Color = new(0.2f, 1f, 1f, 1.6f);
        dp.DestoryAt = 7000;
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = sid;
        dp.Rotation = float.Pi;
        dp.Scale = new(1.5f, 18);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }

    private void DrawRect(ScriptAccessory accessory, Vector3 spos, Vector3 tpos, uint delay)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Predaceous Pounce Rectangle";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = 4000;
        dp.Position = spos;
        dp.TargetPosition = tpos;
        dp.Scale = new Vector2(6);
        dp.ScaleMode = ScaleMode.YByDistance;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    private void DrawCircle(ScriptAccessory accessory, Vector3 spos, uint delay)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Predaceous Pounce Circle";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = 4000;
        dp.Position = spos;
        dp.Scale = new Vector2(11);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Predaceous Pounce 1", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:regex:^(376(8[2-9]|9[01]))$"])]
    public void PredaceousPounce1(Event @event, ScriptAccessory accessory)
    {
        var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
        var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        var epos = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
        if (aid % 2 > 0)
            DrawCircle(accessory, spos, (aid - 37681) * 600 + 9000);
        else
            DrawRect(accessory, spos, epos, (aid - 37681) * 600 + 9000);
    }

    [ScriptMethod(name: "Predaceous Pounce 2", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3963[01])$"])]
    public void PredaceousPounce2(Event @event, ScriptAccessory accessory)
    {
        var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
        var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        var epos = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
        if (aid % 2 > 0)
            DrawCircle(accessory, spos, 19200 - 4000);
        else
            DrawRect(accessory, spos, epos, 19700 - 4000);
    }

    [ScriptMethod(name: "Tether Record", eventType: EventTypeEnum.Tether, eventCondition: ["Id:000C"], userControl: false)]
    public void TetherRecord(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        tethered = tid;
    }

    [ScriptMethod(name: "Leaping One Two Paw", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3766[3456])$"])]
    public async void LeapingOneTwoPaw(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
        await Task.Delay(500);
        var tid = tethered;

        bool isLeftFirst = aid % 2 == 0;
        dp.Name = "Leaping One Two Paw";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 8200;
        dp.Owner = tid;
        dp.Scale = new Vector2(60);
        dp.Radian = float.Pi;
        dp.Rotation = isLeftFirst ? float.Pi / 2 : float.Pi / -2;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        dp.Delay = 8200;
        dp.DestoryAt = 2000;
        dp.Rotation = isLeftFirst ? float.Pi / -2 : float.Pi / 2;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Leaping Black Cat Crossing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37673|38928)$"])]
    public async void LeapingBlackCatCrossing(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        await Task.Delay(500);
        var tid = tethered;
        var obj = accessory.Data.Objects.SearchByEntityId(sid);
        if (obj is null) return;
        var isCardinalsFirst = ((IBattleChara)obj).StatusList.Where(status => status.StatusId == 2193).Count() > 0;

        dp.Name = "Leaping Black Cat Crossing";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 8400;
        dp.Owner = tid;
        dp.Scale = new Vector2(60);
        dp.Radian = float.Pi / 4;
        dp.Rotation = isCardinalsFirst ? float.Pi / 4 : 0;
        for (int i = 0; i < 4; i++)
        {
            dp.Rotation += float.Pi / 2 * i;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        dp.Delay = 8400;
        dp.DestoryAt = 2000;
        dp.Rotation += float.Pi / 4;
        for (int i = 0; i < 4; i++)
        {
            dp.Rotation += float.Pi / 2 * i;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
    }

    [ScriptMethod(name: "Overshadow", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37657"])]
    public void Overshadow(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        dp.Name = "Phase 3 Stack";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.DestoryAt = 5200;
        dp.Owner = sid;
        dp.TargetObject = tid;
        dp.Scale = new Vector2(5, 60);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
}
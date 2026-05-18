using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace Cyf5119Script.Dawntrail.TheStrayboroughDeadwalk;

[ScriptType(guid: "3e045bf2-7a6b-49f4-8c6f-f8b47448c0b7", name: "The Strayborough Deadwalk", territorys: [1204], version: "0.0.0.8", author: "Linoa235")]
public class TheStrayboroughDeadwalk
{
    [UserSetting(note: "Good Head friend prompt time (milliseconds)")] public int Prop1 { get; set; } = 60000;

    private List<Vector3> tethered = [];
    private uint stackRecord = 0;

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        tethered = [];
        stackRecord = 0;
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

    #region BOSS1

    [ScriptMethod(name: "Boss1 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36529"])]
    public void Boss1Aoe(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("AOE", duration: 5000);
    }

    [ScriptMethod(name: "Boss1 Exaflare", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39683"])]
    public void Boss1Exaflare(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        var srot = JsonConvert.DeserializeObject<float>(@event["SourceRotation"]);
        dp.Name = "Boss1Exaflare";
        dp.Scale = new Vector2(4);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = 4400;
        dp.DestoryAt = 3200;
        for (int i = 0; i < 4; i++)
        {
            spos = new Vector3(spos.X + (float)Math.Sin(srot) * 4, spos.Y, spos.Z + (float)Math.Cos(srot) * 4);
            dp.Position = spos;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Delay += 1600;
        }
    }

    [ScriptMethod(name: "Boss1 Chasing AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39686"])]
    public async void Boss1ChasingAoe(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        dp.Name = "Boss1ChasingAoe";
        dp.Scale = new Vector2(4);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 1600;

        var target = accessory.Data.Objects.Where(y => accessory.Data.PartyList.Contains(y.EntityId)).MinBy(x => Vector3.Distance(pos, x.Position));
        if (target is null) return;
        await Task.Delay(2000);
        for (int i = 0; i < 4; i++)
        {
            var tpos = target.Position;
            pos += Vector3.Normalize(tpos - pos) * 3;
            dp.Position = pos;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            await Task.Delay(1600);
        }
    }

    [ScriptMethod(name: "Good Head Friends", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:36533"])]
    public void Boss1Friends(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        ParseObjectId(@event["SourceId"], out var sid);

        dp.DestoryAt = Prop1;
        dp.Owner = sid;

        dp.Name = $"Good Head Friend {sid} 1";
        dp.Scale = new Vector2(1f, 2f);
        dp.Color = new Vector4(1f, .2f, .2f, 1.5f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        dp.Name = $"Good Head Friend {sid} 2";
        dp.Scale = new Vector2(1f, 40f);
        dp.Color = new Vector4(1f, 1f, .2f, .5f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Good Head Friends Clear 1", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7740"], userControl: false)]
    public void Boss1FriendsClear1(Event @event, ScriptAccessory accessory)
    {
        if (JsonConvert.DeserializeObject<uint>(@event["SourceDataId"]) != 16827) return;
        ParseObjectId(@event["SourceId"], out var sid);
        accessory.Method.RemoveDraw($"Good Head Friend {sid} 1");
        accessory.Method.RemoveDraw($"Good Head Friend {sid} 2");
    }

    [ScriptMethod(name: "Good Head Friends Clear 2", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:36535"], userControl: false)]
    public void Boss1FriendsClear2(Event @event, ScriptAccessory accessory)
    {
        ParseObjectId(@event["SourceId"], out var sid);
        accessory.Method.RemoveDraw($"Good Head Friend {sid} 1");
        accessory.Method.RemoveDraw($"Good Head Friend {sid} 2");
    }

    [ScriptMethod(name: "Good Head", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4561"])]
    public void Boss1Heads(Event @event, ScriptAccessory accessory)
    {
        if (JsonConvert.DeserializeObject<uint>(@event["SourceDataId"]) != 16901) return;
        ParseObjectId(@event["SourceId"], out var sid);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = sid;
        dp.Name = $"Good Head {sid}";
        dp.Scale = new Vector2(2);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Good Head Clear", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3653[26])$"], userControl: false)]
    public void Boss1HeadsClear(Event @event, ScriptAccessory accessory)
    {
        ParseObjectId(@event["SourceId"], out var sid);
        accessory.Method.RemoveDraw($"Good Head {sid}");
    }

    #endregion

    
    #region BOSS2

    [ScriptMethod(name: "Boss2 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36725"])]
    public void Boss2Aoe(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("AOE", duration: 5000);
    }

    [ScriptMethod(name: "Boss2 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36726"])]
    public void Boss2Tankbuster(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Tankbuster", duration: 5000);
    }

    [ScriptMethod(name: "Tether Record", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0114"], userControl: false)]
    public void Boss2TetheredRecord(Event @event, ScriptAccessory accessory)
    {
        var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        lock (tethered)
        {
            if (tethered.Count > 1)
                tethered.Clear();
            tethered.Add(spos);
        }
    }

    [ScriptMethod(name: "Tether Clear", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:36720"], userControl: false)]
    public void Boss2TetheredClear(Event @event, ScriptAccessory accessory)
    {
        tethered.Clear();
    }

    [ScriptMethod(name: "Boss2 Teacups", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:regex:^(00000023|00000001)$"])]
    public void Boss2Teacups(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var id = @event["Id"];
        if (!TeacupsHelper(id, out uint dura, out var pos)) return;
        dp.Name = "Boss2 Teacups";
        dp.Scale = new(19);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = dura;
        foreach (var p in pos)
        {
            dp.Position = p;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    private bool TeacupsHelper(string Id, out uint dura, out List<Vector3> pos)
    {
        pos = [];
        dura = 0;
        if (tethered.Count > 2 || tethered.Count < 1) return false;
        List<(Vector3, Vector3?, List<Vector3>)> _pos;
        switch (Id)
        {
            case "02000100":
                dura = 11500;
                _pos = new List<(Vector3, Vector3?, List<Vector3>)>
                {
                    (new(17, -38, -163), new(17, -38, -177), [new(3.5f, -38, -161.5f), new(30.5f, -38, -178.5f)]),
                    (new(17, -38, -153), new(10, -38, -170), [new(25.5f, -38, -156.5f), new(20.5f, -38, -178.5f)]),
                    (new(17, -38, -153), new(17, -38, -177), [new(20.5f, -38, -178.5f), new(3.5f, -38, -161.5f)]),
                    (new(34, -38, -170), null, [new(8.5f, -38, -173.5f)]),
                    (new(0, -38, -170), null, [new(25.5f, -38, -166.5f)])
                };
                break;
            case "10000800":
                dura = 14500;
                _pos = new List<(Vector3, Vector3?, List<Vector3>)>
                {
                    (new(0, -38, -170), new(34, -38, -170), [new(8.5f, -38, -156.5f), new(25.5f, -38, -183.5f)]),
                    (new(0, -38, -170), new(17, -38, -187), [new(3.5f, -38, -178.5f), new(8.5f, -38, -156.5f)]),
                    (new(17, -38, -187), new(17, -38, -153), [new(30.5f, -38, -161.5f), new(3.5f, -38, -178.5f)])
                };
                break;
            case "00100001":
                dura = 16000;
                pos = tethered;
                return true;
            case "00400020":
                dura = 19000;
                _pos = new List<(Vector3, Vector3?, List<Vector3>)>
                {
                    (new(0, -38, -170), new(17, -38, -163), [new(5, -38, -165), new(22, -38, -182)]),
                    (new(17, -38, -177), new(17, -38, -153), [new(5, -38, -175), new(29, -38, -175)])
                };
                break;
            default:
                return false;
        }

        foreach (var (pos1, pos2, positions) in _pos)
        {
            if (CheckPositions(pos1, pos2))
            {
                pos = positions;
                return true;
            }
        }

        return false;
    }

    private bool CheckPositions(Vector3 pos1, Vector3? pos2)
    {
        if (tethered.Count == 1)
            return Vector3.Distance(pos1, tethered[0]) < 1;
        return (Vector3.Distance(pos1, tethered[0]) < 1 && Vector3.Distance((Vector3)pos2, tethered[1]) < 1) ||
               (Vector3.Distance(pos1, tethered[1]) < 1 && Vector3.Distance((Vector3)pos2, tethered[0]) < 1);
    }

    #endregion

    
    #region BOSS3

    [ScriptMethod(name: "Boss3 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37168"])]
    public void Boss3Aoe(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Bleeding AOE", duration: 5000);
    }

    [ScriptMethod(name: "Boss3 Stack Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:37144"], userControl: false)]
    public void Boss3ShareRecord(Event @event, ScriptAccessory accessory)
    {
        ParseObjectId(@event["TargetId"], out var tid);
        stackRecord = tid;
    }

    [ScriptMethod(name: "Boss3 Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37142"])]
    public async void Boss3Share(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        ParseObjectId(@event["SourceId"], out var sid);
        dp.Name = "Boss3 Stack";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Scale = new Vector2(8, 80);
        dp.DestoryAt = 5000;
        dp.Owner = sid;

        await Task.Delay(100);
        dp.TargetObject = stackRecord;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Boss3 Spicy Tail", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37139"])]
    public void Boss3Rect1(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        ParseObjectId(@event["SourceId"], out var sid);
        dp.Name = "Boss3 Spicy Tail";
        dp.Color = new Vector4(.2f, 1f, 1f, .8f);
        dp.Scale = new Vector2(16, 80);
        dp.DestoryAt = 6700;
        dp.Owner = sid;

        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Boss3 Spicy Wings", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37147"])]
    public void Boss3Rect2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        ParseObjectId(@event["SourceId"], out var sid);
        dp.Name = "Boss3 Spicy Wings";
        dp.Color = new Vector4(.2f, 1f, 1f, .8f);
        dp.Scale = new Vector2(12, 50);
        dp.DestoryAt = 6700;
        dp.Owner = sid;

        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Boss3 Adds", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37340"])]
    public void Boss3Rect3(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        ParseObjectId(@event["SourceId"], out var sid);
        dp.Name = "Boss3 Adds";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(4, 40);
        dp.DestoryAt = 6000;
        dp.Owner = sid;

        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    #endregion
}
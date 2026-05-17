using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Data;
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
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;
using Lumina.Excel.Sheets;

namespace BakaWater77.M9N;

[ScriptType(name: "M9N", territorys: [], guid: "bd21de42-5fb2-43ab-8a4a-871319002818", version: "0.0.0.1", Author: "Linoa235")]
public class M9N
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
        name: "Moon Half-Phase Left",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(48823)$" },
        userControl: true
    )]
    public void MoonHalfPhaseLeft(Event @event, ScriptAccessory accessory)
    {
        if (isText)
            accessory.Method.TextInfo("Go to Right first, then cross later", duration: 4700, true);

        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var sourceObj = accessory.Data.Objects.FirstOrDefault(x => x.GameObjectId == sid);
        if (sourceObj == null) return;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Moon Half-Phase Left";
        dp.Owner = sid;
        dp.Scale = new Vector2(60, 60);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Rotation = sourceObj.Rotation - MathF.PI / 2;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(
        name: "Moon Half-Phase Right",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(48825)$" },
        userControl: true
    )]
    public void MoonHalfPhaseRight(Event @event, ScriptAccessory accessory)
    {
        if (isText)
            accessory.Method.TextInfo("Go to Left first, then cross later", duration: 4700, true);

        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var sourceObj = accessory.Data.Objects.FirstOrDefault(x => x.GameObjectId == sid);
        if (sourceObj == null) return;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Moon Half-Phase Right";
        dp.Owner = sid;
        dp.Scale = new Vector2(60, 60);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Rotation = sourceObj.Rotation + MathF.PI / 2;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    #region Moon Half-Phase (Large Left Half-Room Cleave)
    [ScriptMethod(
        name: "Large Left Half-Room Cleave",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(48824)$" },
        userControl: true
    )]
    public void LargeLeftHalfRoomCleave(Event @event, ScriptAccessory accessory)
    {
        if (isText)
            accessory.Method.TextInfo("Go to Right outside hitbox first, then cross later", duration: 4700, true);

        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var sourceObj = accessory.Data.Objects.FirstOrDefault(x => x.GameObjectId == sid);
        if (sourceObj == null) return;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Large Left Half-Room Cleave";
        dp.Owner = sid;
        dp.Scale = new Vector2(60, 60);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Rotation = sourceObj.Rotation - MathF.PI / 2;
        dp.Offset = new Vector3(4, 0, 0);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(
        name: "Large Right Half-Room Cleave",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(48826)$" },
        userControl: true
    )]
    public void LargeRightHalfRoomCleave(Event @event, ScriptAccessory accessory)
    {
        if (isText)
            accessory.Method.TextInfo("Go to Left outside hitbox first, then cross later", duration: 4700, true);

        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var sourceObj = accessory.Data.Objects.FirstOrDefault(x => x.GameObjectId == sid);
        if (sourceObj == null) return;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Large Right Half-Room Cleave";
        dp.Owner = sid;
        dp.Scale = new Vector2(60, 60);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Rotation = sourceObj.Rotation + MathF.PI / 2;
        dp.Offset = new Vector3(-4, 0, 0);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    #endregion

    [ScriptMethod(
        name: "Ether Loss",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(45897)$" }
    )]
    public void EtherLoss(Event @event, ScriptAccessory accessory)
    {
        float rotation = @event.SourceRotation();
        if (!ParseObjectId(@event["TargetId"], out uint targetId))
            return;

        Task.Run(() =>
        {
            var targetObj = accessory.Data.Objects.FirstOrDefault(x => x.GameObjectId == targetId);
            if (targetObj == null) return;
            Vector3 targetPos = targetObj.Position;
            DrawCrossAOE(accessory, targetPos, rotation, 7000);
        });
    }

    private void DrawCrossAOE(ScriptAccessory accessory, Vector3 position, float rotation, int duration = 0)
    {
        Vector2 scale = new Vector2(6, 40);
        var color = accessory.Data.DefaultDangerColor;
        float[] rotations = { rotation, rotation + MathF.PI, rotation + MathF.PI / 2, rotation - MathF.PI / 2 };

        foreach (var rot in rotations)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Ether Loss";
            dp.Position = position;
            dp.Scale = scale;
            dp.Rotation = rot;
            dp.Color = color;
            dp.DestoryAt = duration;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
    }

    [ScriptMethod(
        name: "Sadistic Scream",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(45875)$" }
    )]
    public void SadisticScream(Event @event, ScriptAccessory accessory)
    {
        if (isText)
            accessory.Method.TextInfo("AOE", duration: 4700, true);
    }

    [ScriptMethod(
        name: "Fatal Voice",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(45921)$" },
        userControl: true
    )]
    public void FatalVoice(Event @event, ScriptAccessory accessory)
    {
        if (isText)
            accessory.Method.TextInfo("AOE", duration: 4700, true);
    }

    [ScriptMethod(
        name: "Full-Room Damage",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(45886)$" },
        userControl: true
    )]
    public void FullRoomDamage(Event @event, ScriptAccessory accessory)
    {
        if (isText)
            accessory.Method.TextInfo("AOE", duration: 4700, true);
    }

    [ScriptMethod(
        name: "Deadly Finale",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(45888|45890)$" },
        userControl: true
    )]
    public void DeadlyFinale(Event @event, ScriptAccessory accessory)
    {
        if (isText)
            accessory.Method.TextInfo("AOE", duration: 4700, true);
    }

    [ScriptMethod(
        name: "Insatiable Greed",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(45892)$" },
        userControl: true
    )]
    public void InsatiableGreed(Event @event, ScriptAccessory accessory)
    {
        if (isText)
            accessory.Method.TextInfo("AOE", duration: 4700, true);
    }
}

public static class EventExtensions
{
    public static Vector3 SourcePosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
    }
    public static float SourceRotation(this Event @event)
    {
        return float.Parse(@event["SourceRotation"]);
    }
    public static uint SourceDataId(this Event @event)
    {
        return uint.Parse(@event["SourceDataId"]);
    }
    public static Vector3 EffectPosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
    }
}
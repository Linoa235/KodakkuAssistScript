using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Data;

namespace Cyf5119Scripts.Dawntrail.M12SP2S4;

[ScriptType(guid: "cf2f9b53-aef2-4ddb-847d-1aeea93edc71", name: "M12S Phase 4 Rough Drawing", territorys: [1327], version: "0.0.0.1", Author: "Linoa235", note: Note, updateInfo: Info)]
public class M12SP2S4
{
    private const string Note = "M12S Phase 4 rough drawing, please wait for detailed drawing, good luck on week 1.";
    private const string Info = "Empty";
    
    private static readonly Vector3 Center = new(100, 0, 100);
    private static uint num = 0;
    private static bool isAxisFirst = false;
    private static Dictionary<uint, Vector3> playerShadowPos = new();
    private static Dictionary<uint, byte> playerShapes = new();
    private static Dictionary<uint, byte> playerIndex = new();
    private static Dictionary<uint, byte> addShapes = new();

    public void Init(ScriptAccessory sa)
    {
        num = 0;
    }

    private void Reset()
    {
        isAxisFirst = false;
        playerShadowPos.Clear();
        playerShapes.Clear();
        playerIndex.Clear();
        addShapes.Clear();
    }

    [ScriptMethod(name: "Mirror Dream Count", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46345"], userControl: false)]
    public void MirrorDreamCount(Event evt, ScriptAccessory sa)
    {
        num++;
        Reset();
    }

    private bool PhaseCheck() => num != 1;

    
    #region Shadow Tether

    [ScriptMethod(name: "Player Shadow Spawn", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4562", "SourceDataId:19210"], suppress: 5000, userControl: false)]
    public void PlayerShadowSpawn(Event evt, ScriptAccessory sa)
    {
        if (PhaseCheck()) return;
        isAxisFirst = (evt.SourcePosition().V3YAngle(Center) + 22.5) % 90 < 45;
    }

    [ScriptMethod(name: "Player Shadow Tether", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0175"], userControl: false)]
    public void PlayerShadowTether(Event evt, ScriptAccessory sa)
    {
        if (PhaseCheck()) return;
        var obj = sa.Data.Objects.SearchById(evt.SourceId());
        if (obj == null || obj.DataId != 19210) return;
        lock (playerShadowPos)
        {
            playerShadowPos[evt.TargetId()] = evt.SourcePosition();
        }
    }
    
    [ScriptMethod(name: "Player Tether Record", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(017[01])$"], userControl: false)]
    public void PlayerTetherRecord(Event evt, ScriptAccessory sa)
    {
        if (PhaseCheck()) return;
        byte shape = evt["Id"] switch
        {
            "0170" => 0, // 20 large circle
            "0171" => 1, // 5 stack
            _      => 2
        };
        lock (playerShapes)
        {
            playerShapes[evt.TargetId()] = shape;
            playerIndex[evt.TargetId()] = (byte)((360 - evt.SourcePosition().V3YAngle(Center) + 22.5) % 180 / 45);
        }
    }

    [ScriptMethod(name: "Clone -> Player Stack/Large Circle", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46377"])]
    public void ClonePlayerStackLargeCircle(Event evt, ScriptAccessory sa)
    {
        if (PhaseCheck()) return;
        foreach (var pid in sa.Data.PartyList)
        {
            var shape = playerShapes[pid];
            var index = playerIndex[pid];
            var delay = index switch
            {
                0 => 11100,
                1 => 17500,
                2 => 21200,
                3 => 27400,
                _ => 0
            };
            var dp = sa.FastDp("Clone Player Stack Large Circle", pid, 6300, shape > 0 ? 5 : 20, shape > 0);
            dp.Delay = delay;
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    [ScriptMethod(name: "Clone -> Shadow Stack/Large Circle", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:46365", "TargetIndex:1"])]
    public void CloneShadowStackLargeCircle(Event evt, ScriptAccessory sa)
    {
        if (PhaseCheck()) return;
        foreach (var pid in sa.Data.PartyList)
        {
            var shape = playerShapes[pid];
            var shadowPos = playerShadowPos[pid];
            var isFirst = isAxisFirst == ((shadowPos.V3YAngle(Center) + 22.5) % 90 < 45);
            var dp = sa.FastDp("bigshadow", shadowPos, 10700, shape > 0 ? 5 : 20, shape > 0);
            dp.Delay = isFirst ? 8400 : 29400;
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    #endregion


    #region Add Combo
    
    [ScriptMethod(name: "Add Shape Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4635[123])$"], userControl: false)]
    public void AddShapeRecord(Event evt, ScriptAccessory sa)
    {
        if (PhaseCheck()) return;
        byte shape = evt.ActionId() switch
        {
            46351 => 1, // side cones
            46352 => 2, // front/back cones
            46353 => 3, // heavy
            _     => 4
        };
        lock (addShapes)
        {
            addShapes[evt.SourceId()] = shape;
        }
    }

    private static void DrawAdds(ScriptAccessory sa, uint id, uint duration, uint delay = 0)
    {
        var shape = addShapes[id];
        var dp = sa.FastDp("Cone Heavy Combo", id, duration, shape > 2 ? 10 : 60);
        dp.Delay = delay;
        switch (shape)
        {
            case 1:
                dp.Rotation = float.Pi / 2;
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                dp.Rotation = -float.Pi / 2;
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                break;
            case 2:
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                dp.Rotation = float.Pi;
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                break;
            case 3:
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                break;
            default:
                return;
        }
    }

    [ScriptMethod(name: "First Cone Heavy Combo", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(017[01])$"], suppress: 10000)]
    public void FirstConeHeavyCombo(Event evt, ScriptAccessory sa)
    {
        if (PhaseCheck()) return;
        foreach (var (sid, shape) in addShapes)
            DrawAdds(sa, sid, 12740);
    }

    [ScriptMethod(name: "Second Cone Heavy Combo", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:46297", "TargetIndex:1"])]
    public void SecondConeHeavyCombo(Event evt, ScriptAccessory sa)
    {
        if (PhaseCheck()) return;
        DrawAdds(sa, evt.SourceId(), 8600, 17600);
    }

    [ScriptMethod(name: "Third Cone Heavy Combo", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:46366", "TargetIndex:1"])]
    public void ThirdConeHeavyCombo(Event evt, ScriptAccessory sa)
    {
        if (PhaseCheck()) return;
        DrawAdds(sa, evt.SourceId(), 4800, 1500);
    }

    #endregion


    #region Tower

    [ScriptMethod(name: "Tower Prompt", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:regex:^(201501[3456])$", "Operate:Add"])]
    public void TowerPrompt(Event evt, ScriptAccessory sa)
    {
        if (PhaseCheck()) return;
        switch (evt.DataId())
        {
            case 2015013: // Wind
                var dpWind = sa.FastDp("wind", evt.SourcePosition(), 5000, new Vector2(2, 50), true);
                dpWind.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                dpWind.TargetOrderIndex = 1;
                dpWind.Delay = 47400;
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dpWind);
                break;
            case 2015014: // Dark
                var dpDark = sa.FastDp("dark", evt.SourcePosition(), 5000, new Vector2(10, 50));
                dpDark.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                dpDark.TargetOrderIndex = 1;
                dpDark.Delay = 47400;
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dpDark);
                break;
            case 2015015: // Earth
            case 2015016: // Fire
                break;
        }
    }

    [ScriptMethod(name: "Earth Tower Rock Pile", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46327"])]
    public void EarthTowerRockPile(Event evt, ScriptAccessory sa)
    {
        if (PhaseCheck()) return;
        var dp = sa.FastDp("Earth Tower Rock Pile", evt.SourceId(), 5000, 4);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Near/Far Cone", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(476[67])$"])]
    public void NearFarCone(Event evt, ScriptAccessory sa)
    {
        if (PhaseCheck()) return;
        var dp = sa.FastDp("Near/Far Cone", evt.TargetId(), 5000, 60);
        dp.Radian = float.Pi / 6;
        dp.Delay = 5000;
        dp.TargetResolvePattern = evt.StatusId() > 4766 ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
        dp.TargetOrderIndex = 1;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    #endregion
}
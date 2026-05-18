using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Cyf5119Script.Endwalker.P1To4N;

[ScriptType(guid: "2faeb35f-ccfe-4267-b206-767cb44b5216", name: "P1N-P4N Bundle", territorys: [1002, 1004, 1006, 1008], version: "0.0.0.2", author: "Linoa235", note: "Includes P1N, P2N, P3N, P4N")]
public class P1To4N
{
    [PluginService] public static IClientState ClientState { get; private set; }

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }

    #region P1N

    [ScriptMethod(EventTypeEnum.StartCasting, "P1N-Tankbuster", ["ActionId:26099"])]
    public void HeavyHand(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Single-target Tankbuster", 5000);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P1N-AOE", ["ActionId:regex:^(26100|26089|26090)$"])]
    public void P1NAOE(Event @event, ScriptAccessory accessory)
    {
        var dura = @event.ActionId() == 26100 ? 5000 : 7000;
        accessory.Method.TextInfo("AOE", dura);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P1N-Left/Right Cleave", ["ActionId:regex:^(2806[67])$"])]
    public void GaolersFlail(Event @event, ScriptAccessory accessory)
    {
        var isLeft = @event.ActionId() > 28066;
        var dp = accessory.FastDp("Left/Right Cleave", @event.SourceId(), 8700, 60);
        dp.Radian = float.Pi;
        dp.Rotation = isLeft ? float.Pi / 2 : -float.Pi / 2;
        accessory.Method.SendDraw(0, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P1N-Line Knockback", ["ActionId:26085"])]
    public void PitilessFlail(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        var dp = accessory.FastDp("Line Knockback", accessory.Data.Me, 5000, new Vector2(2, 11), true);
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        accessory.Method.SendDraw(0, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(EventTypeEnum.TargetIcon, "P1N-Stack", ["Id:003E"])]
    public void TrueHoly(Event @event, ScriptAccessory accessory)
    {
        unsafe
        {
            var x = AgentMap.Instance()->CurrentTerritoryId;
            if (ClientState.TerritoryType != 1002) return;
        }
        var dp = accessory.FastDp("Stack", @event.TargetId(), 5000, 6, true);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    #endregion

    #region P2N

    [ScriptMethod(EventTypeEnum.StartCasting, "P2N-Stack Tankbuster", ["ActionId:26638"])]
    public void DoubledImpact(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Stack Tankbuster", @event.TargetId(), 5000, 6);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P2N-AOE", ["ActionId:regex:^(26639|26614)$"])]
    public void P2NAOE(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("AOE", 5000);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P2N-Spoken Cataract 1", ["ActionId:regex:^(2661[567])$"])]
    public void SpokenCataract1(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Spoken Cataract", @event.SourceId(), 7000, 60);
        dp.Radian = float.Pi;
        accessory.Method.SendDraw(0, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P2N-Spoken Cataract 2", ["ActionId:regex:^(2662[12])$"])]
    public void SpokenCataract2(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Spoken Cataract", @event.SourceId(), 7000, new Vector2(15, 100));
        accessory.Method.SendDraw(0, DrawTypeEnum.Straight, dp);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P2N-Shockwave", ["ActionId:regex:26631"])]
    public void Shockwave(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Shockwave", accessory.Data.Me, 6000, new Vector2(2, 13), true);
        dp.TargetPosition = @event.EffectPosition();
        dp.Rotation = float.Pi;
        accessory.Method.SendDraw(0, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(EventTypeEnum.ActionEffect, "P2N-Coherence", ["ActionId:regex:27924", "TargetIndex:1"])]
    public void Coherence(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Coherence", @event.SourceId(), 8400, new Vector2(6, 60), true);
        dp.TargetObject = @event.TargetId();
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P2N-Dissociation", ["ActionId:regex:26630"])]
    public void Dissociation(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Dissociation", @event.SourceId(), 8000, new Vector2(20, 50));
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    #endregion

    #region P3N

    [ScriptMethod(EventTypeEnum.StartCasting, "P3N-Heat of Condemnation", ["ActionId:26291"])]
    public void HeatOfCondemnation(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Heat of Condemnation", @event.TargetId(), 6000, 6);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P3N-AOE", ["ActionId:regex:^(26296)$"])]
    public void P3NAOE(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("AOE", 5000);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P3N-Dead Rebirth", ["ActionId:26281"])]
    public void DeadRebirth(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Large AOE", 10000);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P3N-Center Large Circle", ["ActionId:26263"])]
    public void Fireplume(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Center Large Circle", @event.EffectPosition(), 6000, 15);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P3N-Half-room Cleave", ["ActionId:regex:^(2629[23])$"])]
    public void Cinderwing(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Half-room Cleave", @event.SourceId(), 5000, 60);
        dp.Radian = float.Pi;
        dp.Rotation = @event.ActionId() > 26292 ? float.Pi / 2 : -float.Pi / 2;
        accessory.Method.SendDraw(0, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P3N-Trail of Condemnation", ["ActionId:26287"])]
    public void TrailOfCondemnation(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Trail of Condemnation", @event.SourceId(), 4500, new Vector2(15, 40));
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    #endregion

    #region P4N

    [ScriptMethod(EventTypeEnum.StartCasting, "P4N-Elegant Evisceration", ["ActionId:27216"])]
    public void ElegantEvisceration(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Elegant Evisceration", @event.TargetId(), 5000, 5);
        accessory.Method.SendDraw(0, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P4N-AOE", ["ActionId:regex:^(27217|27200)$"])]
    public void P4NAOE(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("AOE", 5000);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P4N-Hell Skewer", ["ActionId:27215"])]
    public void HellSkewer(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Hell Skewer", @event.SourceId(), 5000, new Vector2(6, 60));
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P4N-Well Pinax", ["ActionId:27198"])]
    public void WellPinax(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Well Pinax", accessory.Data.Me, 9000, new Vector2(2, 15), true);
        dp.TargetPosition = new Vector3(100, 0, 100);
        dp.Rotation = float.Pi;
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P4N-Shifting Strike Fan", ["ActionId:27214"])]
    public void ShiftingStrikeFan(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Shifting Strike Fan", @event.SourceId(), 8500, 60);
        dp.Radian = float.Pi / 180 * 120;
        accessory.Method.SendDraw(0, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(EventTypeEnum.StartCasting, "P4N-Shifting Strike Knockback", ["ActionId:28082"])]
    public void ShiftingStrikeKnockback(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.FastDp("Shifting Strike Knockback", accessory.Data.Me, 8700, new Vector2(2, 25), true);
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        accessory.Method.SendDraw(0, DrawTypeEnum.Rect, dp);
    }

    #endregion
}
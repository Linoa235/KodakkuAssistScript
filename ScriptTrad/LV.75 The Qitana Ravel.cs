using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility.Numerics;
using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.UI;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using Lumina.Data.Parsing.Layer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using static Lumina.Data.Parsing.Layer.LayerCommon;

namespace Veever.Shadowbringers.the_Qitana_Ravel;

[ScriptType(name: "LV.75 The Qitana Ravel", territorys: [823], guid: "50c922c1-1ecd-4750-8b55-24f19793408f",
    version: "0.0.0.2", Author: "Linoa235", note: noteStr)]

public class the_Qitana_Ravel
{
    const string noteStr =
    """
    v0.0.0.2:
    1. If you need a draw for a mechanic or notice any issues, @ me on DC or DM me.
    2. Only tested using a single replay, drawings may not be 100% accurate.
    Duckism.
    """;

    [UserSetting("Banner text toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS toggle")]
    public bool isTTS { get; set; } = true;

    //[UserSetting("Guide arrow toggle")]
    //public bool isLead { get; set; } = true;

    //[UserSetting("Target Marker toggle")]
    //public bool isMark { get; set; } = true;

    //[UserSetting("Local target marker toggle (ON = local only, OFF = party shared)")]
    //public bool LocalMark { get; set; } = true;

    //[UserSetting("Waymark guide toggle")]
    //public bool PostNamazuPrint { get; set; } = true;

    //[UserSetting("PostNamazu Port Setting")]
    //public int PostNamazuPort { get; set; } = 2019;

    //[UserSetting("Waymarks: local toggle (off = party shared, OOC only)")]
    //public bool PostNamazuisLocal { get; set; } = true;

    [UserSetting("Debug on/off (don't touch unless you know what you're doing)")]
    public bool isDebug { get; set; } = false;

    //private readonly object OminousWindMarkerTTSLock = new object();
    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    [ScriptMethod(name: "debug", eventType: EventTypeEnum.Chat, eventCondition: ["Message:debug"])]
    public void debug(Event @event, ScriptAccessory accessory)
    {

    }

    #region Statue Guards
    //Guard 1: Top-Right Bottom-Left Rock
    //Guard 2: Top-Left Bottom-Right Rock

    // guard1
    // right0: 620.22
    // right1: 627.75
    // right2: 635.25
    // right3: 642.75
    // right4: 650.38
    // right5: 657.87


    // guard2
    


    [ScriptMethod(name: "Statue Activated", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0025"])]
    public void StatueActivate(Event @event, ScriptAccessory accessory)
    {
        var sourcePos = @event.SourcePosition();
        //DrawHelper.DrawCircle(accessory, sourcePos, new Vector2(3f), 6000, $"StatueActivate-{@event.SourceId()}", accessory.Data.DefaultSafeColor);
        
        var aoeType = GetAOETypeByPosition(sourcePos);

        DebugMsg($"Statue Activated - {aoeType}- {sourcePos}", accessory);
        
        DrawWrathAOE(accessory, sourcePos, aoeType, @event.SourceId(), @event);
    }

    private string GetAOETypeByPosition(Vector3 position)
    {
        var x = position.X;
        var z = position.Z;
        
        if ((Math.Abs(x - (-17f)) < 1f && Math.Abs(z - 627f) < 1f) ||
            (Math.Abs(x - 17f) < 1f && Math.Abs(z - 642f) < 1f) ||
            (Math.Abs(x - (-17f)) < 1f && Math.Abs(z - 436f) < 1f) ||
            (Math.Abs(x - 17f) < 1f && Math.Abs(z - 421f) < 1f))
        {
            return "mid";
        }

        if ((Math.Abs(x - (-17f)) < 1f && Math.Abs(z - 642f) < 1f) ||
            (Math.Abs(x - 17f) < 1f && Math.Abs(z - 627f) < 1f) ||
            (Math.Abs(x - 17f) < 1f && Math.Abs(z - 436f) < 1f) ||
            (Math.Abs(x - (-17f)) < 1f && Math.Abs(z - 421f) < 1f))
        {
            return "short";
        }
        
        return "long";
    }

    private void DrawWrathAOE(ScriptAccessory accessory, Vector3 position, string aoeType, ulong sourceId, Event @event)
    {
        switch (aoeType)
        {
            case "short":
                WrathOfTheRonkaShort(@event, accessory);
                break;
            case "mid":
                WrathOfTheRonkaMedium(@event, accessory);
                break;
            default:
                WrathOfTheRonkaLong(@event, accessory);
                break;
        }
        //position.Z += 4.4f;
        //position.X = 17.40f;
        //DrawHelper.DrawRect(accessory, position, position + new Vector3(scale.X, 0, 0), scale, 6000, name, accessory.Data.DefaultDangerColor, rotation: float.Pi / 2);
    }

    //The three conditions below do not trigger, just keeping the names here (x), no one should be looking at this spaghetti code anyway
    [ScriptMethod(name: "Wrath of the Ronka Long", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15918"], userControl: false)]
    public void WrathOfTheRonkaLong(Event @event, ScriptAccessory accessory)
    {
        var pos = @event.SourcePosition();
        var x = pos.X;
        var z = pos.Z;
        var rot = float.Pi;
        if (((Math.Abs(z) < 660f && Math.Abs(z) > 612f) && (x > -23f && x < -15f)) || ((Math.Abs(z) < 460f && Math.Abs(z) > 400f) && (x > -23f && x < -15f)))
        {
            DebugMsg($"rot changed", accessory);
            rot = 0;
        }
        DebugMsg($"rot = {rot}", accessory);
        DrawHelper.DrawRect(accessory, pos, pos + new Vector3(35f, 0, 0), new Vector2(8f, 35f), 6000, $"WrathLong-{@event.SourceId()}", accessory.Data.DefaultDangerColor, rotation: rot);
    }

    [ScriptMethod(name: "Wrath of the Ronka Medium", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15917"], userControl: false)]
    public void WrathOfTheRonkaMedium(Event @event, ScriptAccessory accessory)
    { 
        var pos = @event.SourcePosition();
        var x = pos.X;
        var z = pos.Z;
        var rot = float.Pi;
        if (((Math.Abs(z) < 660f && Math.Abs(z) > 612f) && (x > -23f && x < -15f)) || ((Math.Abs(z) < 460f && Math.Abs(z) > 400f) && (x > -23f && x < -15f)))
        {
            DebugMsg($"rot changed", accessory);
            rot = 0;
        }
        DebugMsg($"rot = {rot}", accessory);
        DrawHelper.DrawRect(accessory, pos, pos + new Vector3(22f, 0, 0), new Vector2(8f, 22f), 6000, $"WrathMedium-{@event.SourceId()}", accessory.Data.DefaultDangerColor, rotation: rot);
    }

    [ScriptMethod(name: "Wrath of the Ronka Short", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15916"], userControl: false)]
    public void WrathOfTheRonkaShort(Event @event, ScriptAccessory accessory)
    {
        var pos = @event.SourcePosition();
        var x = pos.X;
        var z = pos.Z;
        var rot = float.Pi;
        if (((Math.Abs(z) < 660f && Math.Abs(z) > 612f) && (x > -23f && x < -15f)) || ((Math.Abs(z) < 460f && Math.Abs(z) > 400f) && (x > -23f && x < -15f)))
        {
            DebugMsg($"rot changed", accessory);
            rot = 0;
        }
        DebugMsg($"rot = {rot}", accessory);
        DrawHelper.DrawRect(accessory, pos, pos + new Vector3(12f, 0, 0), new Vector2(8f, 12f), 6000, $"WrathShort-{@event.SourceId()}", accessory.Data.DefaultDangerColor, rotation: rot);
    }

    [ScriptMethod(name: "Wrath of the Ronka Clear", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:10816"], userControl: false)]
    public void WrathOfTheRonkaClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("WrathLong-.*|WrathMedium-.*|WrathShort-.*");
    }

    #endregion


    #region Mobs
    [ScriptMethod(name: "Ronkan Abyss", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17387"])]
    public void RonkanAbyss(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Move away", duration: 2700, true);
        if (isTTS) accessory.Method.EdgeTTS("Move away");
        DrawHelper.DrawCircle(accessory, @event.EffectPosition(), new Vector2(6f), 2700, $"RonkanAbyss-{@event.SourceId()}", accessory.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Ronkan Abyss Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:17387"], userControl: false)]
    public void RonkanAbyssClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"RonkanAbyss-{@event.SourceId()}");
    }


    [ScriptMethod(name: "Burning Beam", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15923"])]
    public void BurningBeam(Event @event, ScriptAccessory accessory)
    {
        DrawHelper.DrawRectOwner(accessory, @event.SourceId(), new Vector2(4, 15f), 2700, $"BurningBeam-{@event.SourceId()}", accessory.Data.DefaultDangerColor, shape: ScaleMode.ByTime);
    }

    [ScriptMethod(name: "Burning Beam Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15923"], userControl: false)]
    public void BurningBeamClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"BurningBeam-{@event.SourceId()}");
    }

    [ScriptMethod(name: "Hood Swing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15924"])]
    public void HoodSwing(Event @event, ScriptAccessory accessory)
    {
        DrawHelper.DrawFanOwner(accessory, @event.SourceId(), null, new Vector2(9f), 120f, 2700, $"HoodSwing-{@event.SourceId()}", accessory.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Hood Swing Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15924"], userControl: false)]
    public void HoodSwingClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"BurningBeam-{@event.SourceId()}");
    }

    [ScriptMethod(name: "Sinspitter", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15926"])]
    public void Sinspitter(Event @event, ScriptAccessory accessory)
    {
        DrawHelper.DrawCircle(accessory, @event.EffectPosition(), new Vector2(3f), 2700, $"Sinspitter-{@event.SourceId()}", accessory.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Sinspitter Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:15926"], userControl: false)]
    public void SinspitterClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Sinspitter-{@event.SourceId()}");
    }

    [ScriptMethod(name: "Self-destruct", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:16260"])]
    public void Selfdestruct(Event @event, ScriptAccessory accessory)
    {
        DrawHelper.DrawCircle(accessory, @event.EffectPosition(), new Vector2(6.9f), 2700, $"Selfdestruct-{@event.SourceId()}", accessory.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Self-destruct Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:16260"], userControl: false)]
    public void SelfdestructClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Selfdestruct-{@event.SourceId()}");
    }

    #endregion


    #region Boss1
    [ScriptMethod(name: "Stonefist", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15497"])]
    public void Stonefist(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Tank buster", duration: 3700, true);
        if (isTTS) accessory.Method.EdgeTTS("Tank buster");
    }

    [ScriptMethod(name: "Sun Toss", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15498"])]
    public void SunToss(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Move away", duration: 3700, true);
        if (isTTS) accessory.Method.EdgeTTS("Move away");
        DrawHelper.DrawCircle(accessory, @event.EffectPosition(), new Vector2(5f), 2700, $"SunToss-{@event.SourceId()}", accessory.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "AOE Warning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15499"])]
    public void AOENotify(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 2700, true);
        if (isTTS) accessory.Method.EdgeTTS("AOE");
    }


    [ScriptMethod(name: "Draw Wall", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id2:8", "Id1:4"])]
    public void DrawWall(Event @event, ScriptAccessory accessory)
    {
        DebugMsg("Calling Draw Wall", accessory);
        var pos = @event.SourcePosition();
        var left = new Vector3(-10f, 5.15f, 286.91f);
        var right = new Vector3(10f, 5.15f, 286.91f);
        var originpos1 = new Vector3(-7.22f, 5.35f, 328.78f);
        var originpos2 = new Vector3(8.01f, 5.33f, 328.78f);
        if (@event.SourcePosition() != originpos1 && @event.SourcePosition() != originpos2)
        {
            DebugMsg($"Wall position not match, pos = {pos}", accessory);
            return;
        }
        var source = @event.SourcePosition();
        if (pos.X == -7.22f)
        {
            source = left;
            pos.X = left.X;
        } else
        {
            source = right;
            pos.X = right.X;
        }

        DrawHelper.DrawRect(accessory, source, pos, new Vector2(20f, 60f), 10000, $"DrawWall");
    }

    [ScriptMethod(name: "Lozatl's Fury - Right", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15503"])]
    public void LozatlsFuryRight(Event @event, ScriptAccessory accessory)
    {
        DrawHelper.DrawFanOwner(accessory, @event.SourceId(), (-float.Pi / 2), new Vector2(20f), 180f, 3700, $"LozatlsFuryRight");
    }

    [ScriptMethod(name: "Lozatl's Fury - Left", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15504"])]
    public void LozatlsFuryLeft(Event @event, ScriptAccessory accessory)
    {
        DrawHelper.DrawFanOwner(accessory, @event.SourceId(), (float.Pi / 2), new Vector2(20f), 180f, 3700, $"LozatlsFuryLeft");
    }






    #endregion



    #region Boss2
    [ScriptMethod(name: "Ripper Fang", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15505"])]
    public void RipperFang(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Tank buster", duration: 3700, true);
        if (isTTS) accessory.Method.EdgeTTS("Tank buster");
    }


    [ScriptMethod(name: "AOE Warning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(15506|15507)$"])]
    public async void AOENotifyBoss2(Event @event, ScriptAccessory accessory)
    {
        if (@event.ActionId() == 15507)
        {
            if (isText) accessory.Method.TextInfo("Multi-stage AOE", duration: 2700, true);
            if (isTTS) accessory.Method.EdgeTTS("Multi-stage AOE");
        } else
        {
            if (isText) accessory.Method.TextInfo("AOE", duration: 2700, true);
            if (isTTS) accessory.Method.EdgeTTS("AOE");
        }
        await Task.Delay(3000);
        accessory.Method.RemoveDraw($"BurningBeam-.*");

    }

    [ScriptMethod(name: "Stonefall", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(15509|15510|15511)$"])]
    public async void StoneFall(Event @event, ScriptAccessory accessory)
    {
        DebugMsg("rock", accessory);
        if (@event.ActionId() == 15509)
        {
            DebugMsg("small rock", accessory);
            DrawHelper.DrawCircle(accessory, @event.EffectPosition(), new Vector2(2f), 1700, $"StonefallPos-{@event.EffectPosition()}");
        }
        else if(@event.ActionId() == 15511)
        {
            DebugMsg("large rock", accessory);
            DrawHelper.DrawCircle(accessory, @event.EffectPosition(), new Vector2(4f), 1700, $"StonefallPos-{@event.EffectPosition()}");

            await Task.Delay(2000);
            var stoneList = IbcHelper.GetByDataId(accessory, 2009805);
            foreach (var stone in stoneList)
            {
                if (stone == null || !stone.IsValid()) continue;
                DrawHelper.DrawFanOwner(accessory, stone.GameObjectId, null, new Vector2(15f), 30f, 15000, $"StonefallDrop-{stone.GameObjectId}", color: new Vector4(1, 1, 0, 1), scaleByTime: false);
            }
        } else
        {
            DebugMsg("mid rock", accessory);
            DrawHelper.DrawCircle(accessory, @event.EffectPosition(), new Vector2(3f), 1700, $"StonefallPos-{@event.EffectPosition()}");
        }
    }


    #endregion


    #region Boss3
    [ScriptMethod(name: "Rend", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15513"])]
    public void Rend(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Tank buster", duration: 3700, true);
        if (isTTS) accessory.Method.EdgeTTS("Tank buster");
    }

    [ScriptMethod(name: "Hound out of Heaven Charge Hint", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0039"])]
    public void HoundoutofHeaven(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Move away from the boss, lengthen tether until it turns purple", duration: 4700, true);
        if (isTTS) accessory.Method.EdgeTTS("Move away from the boss, lengthen tether until it turns purple");
    }

    [ScriptMethod(name: "AOE Warning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15515"])]
    public void AOENotifyBoss3(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 2700, true);
        if (isTTS) accessory.Method.EdgeTTS("AOE");
    }

    [ScriptMethod(name: "Inhale", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17168"])]
    public void Inhale(Event @event, ScriptAccessory accessory)
    {

        DrawHelper.DrawDisplacement(accessory, @event.EffectPosition(), new Vector2(2f), 4000, $"Inhale");
    }

    [ScriptMethod(name: "Heaving Breath", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15520"])]
    public void HeavingBreath(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Heaving Breath";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = accessory.Data.Me;
        dp.Scale = new Vector2(2, 35);
        dp.Rotation = 0;
        dp.DestoryAt = 3200;
        dp.FixRotation = true;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        if (isText) accessory.Method.TextInfo("Dodge poison circle knockback", duration: 4700, true);
        if (isTTS) accessory.Method.EdgeTTS("Dodge poison circle knockback");
    }

    [ScriptMethod(name: "Confession of Faith - Center", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15522"])]
    public void ConfessionofFaith(Event @event, ScriptAccessory accessory)
    {
        var pos = @event.SourcePosition();
        pos.Z -= 4f;
        DrawHelper.DrawFan(accessory, pos, null, new Vector2(60f), 60f, 5200, $"Confession of Faith", accessory.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Confession of Faith Marked Spread", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15523"])]
    public void ConfessionofFaithMark(Event @event, ScriptAccessory accessory)
    {
        DrawHelper.DrawCircleObject(accessory, @event.TargetId(), new Vector2(5f), 5500, $"Confession of Faith Marked", new Vector4(1, 1, 0, 1));
    }

    [ScriptMethod(name: "Confession of Faith - Two Sides", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15524"])]
    public void ConfessionofFaithTwoSides(Event @event, ScriptAccessory accessory)
    {
        var pos = @event.SourcePosition();
        pos.Z -= 7f;
        DrawHelper.DrawFan(accessory, pos, 40 * (float.Pi / 180), new Vector2(60f), 60f, 5500, $"Confession of Faith", accessory.Data.DefaultDangerColor);
        DrawHelper.DrawFan(accessory, pos, -40 * (float.Pi / 180), new Vector2(60f), 60f, 5500, $"Confession of Faith", accessory.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Confession of Faith Marked Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15525"])]
    public void ConfessionofFaithMarkStack(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Stack in the center", duration: 4700, true);
        if (isTTS) accessory.Method.EdgeTTS("Stack in the center");
        DrawHelper.DrawCircleObject(accessory, @event.TargetId(), new Vector2(5f), 5500, $"Confession of Faith Stack", accessory.Data.DefaultSafeColor);
    }

    #endregion




    #region Helpers

    public unsafe static float GetStatusRemainingTime(ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return 0;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return charaStruct->GetStatusManager()->GetRemainingTime(statusIdx);
        }
    }


    private unsafe uint[] ScanTether(Event evt, ScriptAccessory sa, uint id)
    {
        if (sa?.Data?.Objects == null) return Array.Empty<uint>();
        List<uint> dataId = [id];
        List<uint> players = [];
        foreach (var fire in sa.Data.Objects.Where(x => dataId.Contains(x.DataId)))
        {
            if (fire?.Address == null) continue;
            var targetId = ((BattleChara*)fire.Address)->Vfx.Tethers[0].TargetId.ObjectId;
            players.Add(targetId);
        }
        DebugMsg($"players: {string.Join(", ", players)}", sa);
        return players.ToArray();
    }

    public static class DrawHelper
    {
        public static void DrawBeam(ScriptAccessory accessory, Vector3 sourcePosition, Vector3 targetPosition, string name = "Light's Course", int duration = 6700, Vector4? color = null, int delay = 0)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Color = color ?? accessory.Data.DefaultDangerColor;
            dp.Position = sourcePosition;
            dp.TargetPosition = targetPosition;
            dp.Scale = new Vector2(10, 50);
            dp.Delay = delay;
            dp.DestoryAt = duration;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        public static void DrawCircle(ScriptAccessory accessory, Vector3 position, Vector2 scale, int duration, string name, Vector4? color = null, bool scaleByTime = true, int delay = 0)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Color = color ?? accessory.Data.DefaultDangerColor;
            dp.Position = position;
            dp.Scale = scale;
            dp.Delay = delay;
            dp.DestoryAt = duration;
            if (scaleByTime) dp.ScaleMode = ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        public static void DrawDisplacement(ScriptAccessory accessory, Vector3 target, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0, bool fix = false)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Owner = accessory.Data.Me;
            dp.Color = color ?? accessory.Data.DefaultSafeColor;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.TargetPosition = target;
            dp.Scale = scale;
            dp.Delay = delay;
            dp.DestoryAt = duration;
            dp.FixRotation = fix;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        public static void DrawDisplacementby2points(ScriptAccessory accessory, Vector3 origin, Vector3 target, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Position = origin;
            dp.Color = color ?? accessory.Data.DefaultSafeColor;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.TargetPosition = target;
            dp.Scale = scale;
            dp.Delay = delay;
            dp.DestoryAt = duration;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        public static void DrawRect(ScriptAccessory accessory, Vector3 position, Vector3 targetPos, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0, float rotation = 0)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Color = color ?? accessory.Data.DefaultDangerColor;
            dp.Position = position;
            dp.TargetPosition = targetPos;
            dp.Scale = scale;
            dp.Rotation = rotation;
            dp.Delay = delay;
            dp.DestoryAt = duration;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        public static void DrawRectOwner(ScriptAccessory accessory, ulong owner, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0, float rotation = 0, ScaleMode shape = ScaleMode.None)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Color = color ?? accessory.Data.DefaultDangerColor;
            dp.Owner = owner;
            dp.Scale = scale;
            dp.Rotation = rotation;
            dp.Delay = delay;
            dp.DestoryAt = duration;
            dp.ScaleMode = shape;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        public static void DrawFan(ScriptAccessory accessory, Vector3 position, float ?rotation, Vector2 scale, float angle, int duration, string name, Vector4? color = null, int delay = 0, bool fix = false)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Color = color ?? accessory.Data.DefaultDangerColor;
            dp.Position = position;
            if (rotation.HasValue) dp.Rotation = rotation.Value;
            dp.Scale = scale;
            dp.Radian = angle * (float.Pi / 180);
            dp.Delay = delay;
            dp.DestoryAt = duration;
            dp.FixRotation = fix;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        public static void DrawFanOwner(ScriptAccessory accessory, ulong owner, float ?rotation, Vector2 scale, float angle, int duration, string name, Vector4? color = null, int delay = 0, bool scaleByTime = true, bool fix = false)
        { 
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Color = color ?? accessory.Data.DefaultDangerColor;
            dp.Owner = owner;
            if (rotation.HasValue) dp.Rotation = rotation.Value;
            dp.Scale = scale;
            dp.Radian = angle * (float.Pi / 180);
            dp.Delay = delay;
            dp.DestoryAt = duration;
            dp.FixRotation = fix;
            if (scaleByTime) dp.ScaleMode = ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        public static void DrawLine(ScriptAccessory accessory, Vector3 startPosition, Vector3 endPosition, float width, int duration, string name, Vector4? color = null, int delay = 0)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Color = color ?? accessory.Data.DefaultDangerColor;
            dp.Position = startPosition;
            dp.TargetPosition = endPosition;
            dp.Scale = new Vector2(width, 1);
            dp.Delay = delay;
            dp.DestoryAt = duration;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Line, dp);
        }

        public static void DrawArrow(ScriptAccessory accessory, Vector3 startPosition, Vector3 endPosition, float x, float y, int duration, string name, Vector4? color = null, int delay = 0)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Color = color ?? accessory.Data.DefaultDangerColor;
            dp.Position = startPosition;
            dp.TargetPosition = endPosition;
            dp.Scale = new Vector2(x, y);
            dp.Delay = delay;
            dp.DestoryAt = duration;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Arrow, dp);
        }

        public static void DrawArrowOwner(ScriptAccessory accessory, ulong ob, Vector3 endPosition, float x, float y, int duration, string name, Vector4? color = null, int delay = 0)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Color = color ?? accessory.Data.DefaultDangerColor;
            dp.Owner = ob;
            dp.TargetPosition = endPosition;
            dp.Scale = new Vector2(x, y);
            dp.Delay = delay;
            dp.DestoryAt = duration;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Arrow, dp);
        }

        public static void DrawCircleObject(ScriptAccessory accessory, ulong? ob, Vector2 scale, int duration, string name, Vector4? color = null, bool scaleByTime = true, int delay = 0)
        {
            if (ob == null) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Color = color ?? accessory.Data.DefaultDangerColor;
            dp.Owner = ob.Value;
            dp.Scale = scale;
            dp.Delay = delay;
            dp.DestoryAt = duration;
            if (scaleByTime) dp.ScaleMode = ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

}


public static class EventExtensions
{
    private static bool ParseHexId(string? idStr, out uint id)
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

    public static uint Id(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["Id"]);
    }

    public static uint ActionId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
    }

    public static uint SourceId(this Event @event)
    {
        return ParseHexId(@event["SourceId"], out var id) ? id : 0;
    }

    public static uint SourceDataId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["SourceDataId"]);
    }

    public static uint DataId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["DataId"]);
    }

    public static uint Command(this Event @event)
    {
        return ParseHexId(@event["Command"], out var cid) ? cid : 0;
    }

    public static string DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<string>(@event["DurationMilliseconds"]) ?? string.Empty;
    }

    public static float SourceRotation(this Event @event)
    {
        return JsonConvert.DeserializeObject<float>(@event["SourceRotation"]);
    }

    public static float TargetRotation(this Event @event)
    {
        return JsonConvert.DeserializeObject<float>(@event["TargetRotation"]);
    }

    public static byte Index(this Event @event)
    {
        return (byte)(ParseHexId(@event["Index"], out var index) ? index : 0);
    }

    public static uint State(this Event @event)
    {
        return ParseHexId(@event["State"], out var state) ? state : 0;
    }

    public static string SourceName(this Event @event)
    {
        return @event["SourceName"];
    }

    public static string TargetName(this Event @event)
    {
        return @event["TargetName"];
    }

    public static uint TargetId(this Event @event)
    {
        return ParseHexId(@event["TargetId"], out var id) ? id : 0;
    }

    public static Vector3 SourcePosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
    }

    public static Vector3 TargetPosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
    }

    public static Vector3 EffectPosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
    }

    public static uint DirectorId(this Event @event)
    {
        return ParseHexId(@event["DirectorId"], out var id) ? id : 0;
    }

    public static uint StatusId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StatusId"]);
    }

    public static uint StackCount(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StackCount"]);
    }

    public static uint Param(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["Param"]);
    }

    public static string Operate(this Event @event)
    {
        return @event["Operate"];
    }
}

public static class IbcHelper
{
    public static KodakkuAssist.Data.IGameObject? GetById(ScriptAccessory accessory, uint id)
    {
        return accessory.Data.Objects.SearchByEntityId(id);
    }

    public static KodakkuAssist.Data.IGameObject? GetMe(ScriptAccessory accessory)
    {
        return accessory.Data.Objects.SearchByEntityId(accessory.Data.Me);
    }

    public static KodakkuAssist.Data.IGameObject? GetFirstByDataId(ScriptAccessory accessory, uint dataId)
    {
        return accessory.Data.Objects.Where(x => x.DataId == dataId).FirstOrDefault();
    }

    public static IEnumerable<KodakkuAssist.Data.IGameObject> GetByDataId(ScriptAccessory accessory, uint dataId)
    {
        return accessory.Data.Objects.Where(x => x.DataId == dataId);
    }

    public static IEnumerable<KodakkuAssist.Data.IGameObject> GetParty(ScriptAccessory accessory)
    {
        foreach (var pid in accessory.Data.PartyList)
        {
            var obj = accessory.Data.Objects.SearchByEntityId(pid);
            if (obj != null) yield return obj;
        }
    }

    public static IEnumerable<KodakkuAssist.Data.IGameObject> GetPartyEntities(ScriptAccessory accessory)
    {
        return accessory.Data.Objects.Where(obj => accessory.Data.PartyList.Contains(obj.EntityId));
    }

    public static bool HasStatus(this IBattleChara ibc, uint statusId)
    {
        return ibc.StatusList.Any(x => x.StatusId == statusId);
    }

    public static bool HasStatusAny(this IBattleChara ibc, uint[] statusIds)
    {
        return ibc.StatusList.Any(x => statusIds.Contains(x.StatusId));
    }

    public static unsafe uint Tethering(this IBattleChara ibc, int index = 0)
    {
        return ((BattleChara*)ibc.Address)->Vfx.Tethers[index].TargetId.ObjectId;
    }
}

public static class NamazuHelper
{
    public class NamazuCommand(ScriptAccessory accessory, string url, string command, string param)
    {
        private ScriptAccessory accessory { get; set; } = accessory;
        private string _url = url;

        public void PostCommand()
        {
            var url = $"{_url}/{command}";
            //accessory.Method.SendChat($"/e Sending {param} to {url}");
            accessory.Method.HttpPost(url, param);
        }
    }

    public class Waymark
    {
        public ScriptAccessory accessory { get; set; }
        private Dictionary<string, object> _jsonObj = new();
        private string? _jsonPayload;

        public Waymark(ScriptAccessory _accessory)
        {
            accessory = _accessory;
        }

        public void AddWaymarkType(string type, Vector3 pos, bool active = true)
        {
            string[] validTypes = ["A", "B", "C", "D", "One", "Two", "Three", "Four"];
            var waymarkType = type;
            if (!validTypes.Contains(type)) return;
            _jsonObj[waymarkType] = new Dictionary<string, object>
            {
                { "X", pos.X },
                { "Y", pos.Y },
                { "Z", pos.Z },
                { "Active", active }
            };
        }

        public void SetJsonPayload(bool local = true, bool log = true)
        {
            _jsonObj["LocalOnly"] = local;
            _jsonObj["Log"] = log;
            _jsonPayload = JsonConvert.SerializeObject(_jsonObj);
        }

        public string? GetJsonPayload()
        {
            if (_jsonPayload == null)
                SetJsonPayload();
            return _jsonPayload;
        }

        public void PostWaymarkCommand(int port)
        {
            var param = GetJsonPayload();
            if (param == null) return;
            var post = new NamazuCommand(accessory, $"http://127.0.0.1:{port}", "place", param);
            post.PostCommand();
        }

        public void ClearWaymarks(int port)
        {
            var post = new NamazuCommand(accessory, $"http://127.0.0.1:{port}", "place", "clear");
            post.PostCommand();
        }
    }
    #endregion
}
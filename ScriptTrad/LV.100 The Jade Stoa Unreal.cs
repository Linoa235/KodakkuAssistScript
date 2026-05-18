using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using System.Reflection.Metadata;
using System.Net;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using FFXIVClientStructs;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Veever.DawnTrail.The_Jade_Stoa_Unreal;

[ScriptType(name: "LV.100 The Jade Stoa Unreal", territorys: [1239], guid: "c03383f7-19b8-4ae9-b5d6-519cf7b598bf",
    version: "0.0.0.4", author: "Veever", note: noteStr)]

public class The_Jade_Stoa_Unreal
{
    const string noteStr =
    """
    v0.0.0.4:
    1. This script uses the Zi Yan guide. Please adjust your! Kwek! party order before playing! (Very important, affects waypoint and mechanic announcements)
    2. If you're too lazy to adjust or don't want waypoints that require party position detection, you can turn off the waypoint toggle in user settings
    Duckmen.
    """;

    [UserSetting("Text Banner Toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = false;

    [UserSetting("DR TTS Toggle")]
    public bool isDRTTS { get; set; } = true;

    [UserSetting("Waypoint Toggle")]
    public bool isLead { get; set; } = true;

    [UserSetting("Marker Toggle")]
    public bool isMark { get; set; } = true;

    [UserSetting("Local Marker Toggle (ON = local only, OFF = party)")]
    public bool LocalMark { get; set; } = true;

    [UserSetting("Debug toggle, please turn off for non-development use")]
    public bool isDebug { get; set; } = false;

    public KodakkuAssist.Data.IGameObject? Boss { get; set; }

    public int HighestStakesCount;
    public int OminousWindMarkerTTSCount;
    public int HakuteiNotifyCount;
    public int ShockStrikeCount;

    public bool isMTGroup;

    private readonly object OminousWindMarkerTTSLock = new object();

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        HighestStakesCount = 0;
        OminousWindMarkerTTSCount = 0;
        HakuteiNotifyCount = 0;
        ShockStrikeCount = 0;
    }

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    [ScriptMethod(name: "debug", eventType: EventTypeEnum.Chat, eventCondition: ["Message:debug"])]
    public async void debug(Event @event, ScriptAccessory accessory)
    {
        // DebugMsg($"Me:{IbcHelper.GetMe().Name}", accessory);
        // DebugMsg($"job:{IbcHelper.GetMe().ClassJob.Value.Name}", accessory);
    }

    #region Phase 1
    [ScriptMethod(name: "Storm Pulse", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39933"])]
    public void StormPulse(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("AOE", duration: 3000, true);
        accessory.TTS("AOE", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39931"])]
    public void Tankbuster(Event @event, ScriptAccessory accessory)
    {
        var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
        if (index == 0)
        {
            if (isText) accessory.Method.TextInfo("AoE tankbuster, use mitigation", duration: 2500, true);
            accessory.TTS("AoE tankbuster, use mitigation", isTTS, isDRTTS);
        }
        else
        {
            if (isText) accessory.Method.TextInfo("AoE tankbuster, move away from MT", duration: 2500, true);
            accessory.TTS("AoE tankbuster, move away from MT", isTTS, isDRTTS);
        }
    }

    [ScriptMethod(name: "Highest Stakes Position Record", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:003E"])]
    public void HighestStakesPosRecord(Event @event, ScriptAccessory accessory)
    {
        uint id = @event.TargetId();
        var boss = IbcHelper.GetById(accessory, id);
        if (boss == null)
        {
            DebugMsg("Object not found", accessory);
            return;
        }
        Boss = boss;
    }

    [ScriptMethod(name: "Highest Stakes", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39939"])]
    public void HighestStakes(Event @event, ScriptAccessory accessory)
    {
        var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

        isMTGroup = (HighestStakesCount % 2 == 0) ? true : false;
        if (Boss == null)
        {
            return;
        }

        if (isMTGroup == true)
        {
            if (index == 1)
            {
                if (isText) accessory.Method.TextInfo("Provoke the boss", duration: 3700, true);
                accessory.TTS("Provoke the boss", isTTS, isDRTTS);
            }
            else
            {
                if (isText) accessory.Method.TextInfo("MT group go to stack", duration: 3700, true);
                accessory.TTS("MT group go to stack", isTTS, isDRTTS);

                var dp = accessory.Data.GetDefaultDrawProperties();

                dp.Name = "Highest Stakes";
                dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
                dp.Position = Boss.Position;
                dp.Scale = new Vector2(6);
                dp.DestoryAt = 5000;
                dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                DebugMsg($"Me:{index}", accessory);
                if (index == 2 || index == 4 || index == 5)
                {
                    DebugMsg($"Me:MT group", accessory);
                    var dp1 = accessory.Data.GetDefaultDrawProperties();
                    dp1.Name = $"Highest Stakes Waypoint MT Group";
                    dp1.Owner = accessory.Data.Me;
                    dp1.Color = accessory.Data.DefaultSafeColor;
                    dp1.ScaleMode |= ScaleMode.YByDistance;
                    dp1.TargetPosition = dp.Position;
                    dp1.Scale = new(2);
                    dp1.DestoryAt = 4500;
                    if (isLead) accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
                }
            }
        }

        if (isMTGroup == false)
        {
            if (isText) accessory.Method.TextInfo("ST group go to stack", duration: 3700, true);
            accessory.TTS("ST group go to stack", isTTS, isDRTTS);

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "Highest Stakes";
            dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
            dp.Position = Boss.Position;
            dp.Scale = new Vector2(6);
            dp.DestoryAt = 5000;
            dp.ScaleMode = ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            DebugMsg($"Me:{index}", accessory);
            if (index == 3 || index == 6 || index == 7)
            {
                DebugMsg($"Me:ST group", accessory);
                var dp1 = accessory.Data.GetDefaultDrawProperties();
                dp1.Name = $"Highest Stakes Waypoint ST Group";
                dp1.Owner = accessory.Data.Me;
                dp1.Color = accessory.Data.DefaultSafeColor;
                dp1.ScaleMode |= ScaleMode.YByDistance;
                dp1.TargetPosition = dp.Position;
                dp1.Scale = new(2);
                dp1.DestoryAt = 4500;
                if (isLead) accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
            }
        }
        HighestStakesCount++;
    }

    [ScriptMethod(name: "Aratama Marker", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17801"])]
    public void AratamaMarker(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Aratama Marker";
        dp.Color = new Vector4(1.0f, 0.0f, 0.0f, 0.784f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.5f);
        dp.InnerScale = new Vector2(1.48f);
        dp.DestoryAt = 10000;
        dp.Radian = 2 * float.Pi;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp);
    }

    [ScriptMethod(name: "Eastern Ball Dodge", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:39952"])]
    public void EasternBall(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Eastern Ball Dodge Marker";
        dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1f);
        var pos = @event.EffectPosition();
        pos.Y = 0;
        dp.Position = pos;
        dp.Scale = new Vector2(2f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Ominous Wind Marker", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1481"])]
    public void OminousWindMarker(Event @event, ScriptAccessory accessory)
    {
        lock (OminousWindMarkerTTSLock)
        {
            if (OminousWindMarkerTTSCount == 0)
            {
                if (isText) accessory.Method.TextInfo("Marked players move away from each other", duration: 2500, true);
                accessory.TTS("Marked players move away from each other", isTTS, isDRTTS);
            }
            OminousWindMarkerTTSCount++;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Ominous Wind Marker";
            dp.Color = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);
            dp.Owner = @event.TargetId();
            dp.Scale = new Vector2(5f);
            dp.InnerScale = new Vector2(4.95f);
            dp.DestoryAt = 10000;
            dp.Radian = 2 * float.Pi;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp);
        }
    }

    [ScriptMethod(name: "Fire and Lightning (39930)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39930"])]
    public void FireandLightning_39930(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Stay away from boss front", duration: 2500, true);
        accessory.TTS("Stay away from boss front", isTTS, isDRTTS);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fire and Lightning 39930";
        dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f, 50f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "ST Hakutei Pull Reminder", eventType: EventTypeEnum.Targetable, eventCondition: ["SourceName:regex:^(白帝|Hakutei)$"])]
    public void HakuteiNotify(Event @event, ScriptAccessory accessory)
    {
        var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
        if (HakuteiNotifyCount == 0)
        {
            if (index == 0)
            {
                if (isText) accessory.Method.TextInfo("MT bring boss to the north side of the arena", duration: 3000, true);
                accessory.TTS("MT bring boss to the north side of the arena", isTTS, isDRTTS);
            }

            if (index == 1)
            {
                if (isText) accessory.Method.TextInfo("Hakutei appears, ST grab aggro and go to the south side", duration: 3000, true);
                accessory.TTS("Hakutei appears, ST grab aggro and go to the south side", isTTS, isDRTTS);
            }
        }
        HakuteiNotifyCount++;
    }

    [ScriptMethod(name: "Aratama", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0004"])]
    public void Aratama(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)
        {
            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            if (index == 3 || index == 2)
            {
                if (isText) accessory.Method.TextInfo("Healers go to the east side to bait three Aratama", duration: 2000, true);
                accessory.TTS("Healers go to the east side to bait three Aratama", isTTS, isDRTTS);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Aratama Healer Waypoint";
                dp.Owner = accessory.Data.Me;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.TargetPosition = new Vector3(18, 0, 0);
                dp.Scale = new(2);
                dp.DestoryAt = 4500;
                if (isLead) accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            if (index > 3)
            {
                if (isText) accessory.Method.TextInfo("DPS go to the west side to bait three Aratama", duration: 2000, true);
                accessory.TTS("DPS go to the west side to bait three Aratama", isTTS, isDRTTS);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Aratama DPS Waypoint";
                dp.Owner = accessory.Data.Me;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.TargetPosition = new Vector3(-18, 0, 0);
                dp.Scale = new(2);
                dp.DestoryAt = 4500;
                if (isLead) accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
    }

    [ScriptMethod(name: "ST White Herald Reminder", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0057"])]
    public void WhiteHeraldNotify(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isText) accessory.Method.TextInfo("ST go to the south side to bait White Herald charge", duration: 3700, true);
            accessory.TTS("ST go to the south side to bait White Herald charge", isTTS, isDRTTS);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"White Herald Waypoint";
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.TargetPosition = new Vector3(2.13f, -0.00f, 19.39f);
            dp.Scale = new(2);
            dp.DestoryAt = 5000;
            if (isLead) accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
    }

    [ScriptMethod(name: "Distant Clap (except ST)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39934"])]
    public void DistantClap(Event @event, ScriptAccessory accessory)
    {
        var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
        if (index != 1)
        {
            if (isText) accessory.Method.TextInfo("Dynamo, go under the boss", duration: 2000, true);
            accessory.TTS("Dynamo, go under the boss", isTTS, isDRTTS);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Distant Clap";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(4f);
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"Distant Clap Waypoint";
            dp1.Owner = accessory.Data.Me;
            dp1.Color = accessory.Data.DefaultSafeColor;
            dp1.ScaleMode |= ScaleMode.YByDistance;
            dp1.TargetPosition = @event.SourcePosition();
            dp1.Scale = new(2);
            dp1.DestoryAt = 5000;
            if (isLead) accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
        }
    }

    [ScriptMethod(name: "Fire and Lightning (Hakutei - Full - 39935)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39935"])]
    public void FireandLightning_39935(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Stay away from line AOE", duration: 2000, true);
        accessory.TTS("Stay away from line AOE", isTTS, isDRTTS);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fire and Lightning 39935";
        dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f, 50f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Shock Strike", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39961"])]
    public async void ShockStrike(Event @event, ScriptAccessory accessory)
    {
        if (ShockStrikeCount == 0)
        {
            if (isText) accessory.Method.TextInfo("Focus fire on Hakutei, be careful with orb collision", duration: 2500, true);
            accessory.TTS("Focus fire on Hakutei, be careful with orb collision", isTTS, isDRTTS);
        }
        else
        {
            if (isText) accessory.Method.TextInfo("Focus fire on Byakko", duration: 2500, true);
            accessory.TTS("Focus fire on Byakko", isTTS, isDRTTS);
            await Task.Delay(14700);
            if (isText) accessory.Method.TextInfo("Tank LB", duration: 2500, true);
            accessory.TTS("Tank LB", isTTS, isDRTTS);
        }

        ShockStrikeCount++;
    }
    #endregion

    #region Phase 2
    [ScriptMethod(name: "P2-1 Sweep the Leg", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39957"])]
    public void SweeptheLeg(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Center dynamo", duration: 2000, true);
        accessory.TTS("Center dynamo", isTTS, isDRTTS);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "P2-1 Sweep the Leg Marker";
        dp.Color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
        dp.Position = new Vector3(0, 0, 0);
        dp.Scale = new Vector2(25f);
        dp.InnerScale = new Vector2(5f);
        dp.DestoryAt = 4800;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Radian = 2 * float.Pi;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp);
    }

    [ScriptMethod(name: "Phase 2 Aratama", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:39953"])]
    public void phase2Aratama(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Phase 2 Aratama";
        dp.Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(2f);
        dp.DestoryAt = 4800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Phase 2 Transition AOE", eventType: EventTypeEnum.Chat, eventCondition: ["Message:regex:^(化为灰烬吧！| To ashes with you! | 塵芥と消えるがよい！)$"])]
    public void phase2AOE(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Heavy AOE", duration: 2500, true);
        accessory.TTS("Heavy AOE", isTTS, isDRTTS);
    }

    [ScriptMethod(name: "P2-2 Sweep the Leg", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39932"])]
    public void P2SweeptheLeg(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Go behind the boss", duration: 3700, true);
        accessory.TTS("Go behind the boss", isTTS, isDRTTS);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "P2-2 Sweep the Leg";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(24f);
        dp.Radian = float.Pi / 180 * 270;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Hundredfold Havoc", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39942"])]
    public void HundredfoldHavoc(Event @event, ScriptAccessory accessory)
    {
        var basePos = @event.EffectPosition();

        List<Vector3> HundredfoldHavoclistNSEW = new List<Vector3>
        {
            new Vector3(-0.02f, -0.02f, -5.02f),
            new Vector3(-5.02f, -0.02f, -0.02f),
            new Vector3(4.99f, -0.02f, -0.02f),
            new Vector3(-0.02f, -0.02f, 4.99f),
            new Vector3(-0.02f, 0f, -10.02f),
            new Vector3(-10.02f, 0f, -0.02f),
            new Vector3(9.99f, 0f, -0.02f),
            new Vector3(-0.02f, 0f, 9.99f),
        };

        List<Vector3> HundredfoldHavoclistCorner = new List<Vector3>
        {
            new Vector3(-3.56f, -0.02f, -3.56f),
            new Vector3(-3.56f, -0.02f, 3.52f),
            new Vector3(3.52f, -0.02f, 3.52f),
            new Vector3(3.52f, -0.02f, -3.56f),
            new Vector3(-7.12f, 0f, -7.12f),
            new Vector3(-7.12f, 0f, 7.04f),
            new Vector3(7.04f, 0f, 7.04f),
            new Vector3(7.04f, 0f, -7.12f),
        };

        var dp0 = accessory.Data.GetDefaultDrawProperties();
        dp0.Name = "Hundredfold Havoc Base";
        dp0.Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        dp0.Position = basePos;
        dp0.Scale = new Vector2(5f);
        dp0.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp0);

        DebugMsg($"{@event.EffectPosition()}", accessory);

        if (basePos == HundredfoldHavoclistNSEW[0])
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = "Hundredfold Havoc Base";
            dp1.Color = new Vector4(1.0f, 1f, 0.0f, 1.0f);
            dp1.Position = HundredfoldHavoclistNSEW[4];
            dp1.Scale = new Vector2(5f);
            dp1.InnerScale = new Vector2(4.95f);
            dp1.Delay = 4700;
            dp1.DestoryAt = 1000;
            dp1.Radian = 2 * float.Pi;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp1);
        }
        if (basePos == HundredfoldHavoclistNSEW[1])
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = "Hundredfold Havoc Base";
            dp1.Color = new Vector4(1.0f, 1f, 0.0f, 1.0f);
            dp1.Position = HundredfoldHavoclistNSEW[5];
            dp1.Scale = new Vector2(5f);
            dp1.InnerScale = new Vector2(4.95f);
            dp1.Delay = 4700;
            dp1.DestoryAt = 1000;
            dp1.Radian = 2 * float.Pi;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp1);
        }
        if (basePos == HundredfoldHavoclistNSEW[2])
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = "Hundredfold Havoc Base";
            dp1.Color = new Vector4(1.0f, 1f, 0.0f, 1.0f);
            dp1.Position = HundredfoldHavoclistNSEW[6];
            dp1.Scale = new Vector2(5f);
            dp1.InnerScale = new Vector2(4.95f);
            dp1.Delay = 4700;
            dp1.DestoryAt = 1000;
            dp1.Radian = 2 * float.Pi;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp1);
        }
        if (basePos == HundredfoldHavoclistNSEW[3])
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = "Hundredfold Havoc Base";
            dp1.Color = new Vector4(1.0f, 1f, 0.0f, 1.0f);
            dp1.Position = HundredfoldHavoclistNSEW[7];
            dp1.Scale = new Vector2(5f);
            dp1.InnerScale = new Vector2(4.95f);
            dp1.Delay = 4700;
            dp1.DestoryAt = 1000;
            dp1.Radian = 2 * float.Pi;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp1);
        }

        if (basePos == HundredfoldHavoclistCorner[0])
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = "Hundredfold Havoc";
            dp1.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
            dp1.Position = HundredfoldHavoclistCorner[4];
            dp1.Scale = new Vector2(5f);
            dp1.InnerScale = new Vector2(4.95f);
            dp1.Delay = 4700;
            dp1.DestoryAt = 1000;
            dp1.Radian = 2 * float.Pi;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp1);
        }
        if (basePos == HundredfoldHavoclistCorner[1])
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = "Hundredfold Havoc";
            dp1.Color = new Vector4(1.0f, 1f, 0.0f, 1.0f);
            dp1.Position = HundredfoldHavoclistCorner[5];
            dp1.Scale = new Vector2(5f);
            dp1.InnerScale = new Vector2(4.95f);
            dp1.Delay = 4700;
            dp1.DestoryAt = 1000;
            dp1.Radian = 2 * float.Pi;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp1);
        }
        if (basePos == HundredfoldHavoclistCorner[2])
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = "Hundredfold Havoc";
            dp1.Color = new Vector4(1.0f, 1f, 0.0f, 1.0f);
            dp1.Position = HundredfoldHavoclistCorner[6];
            dp1.Scale = new Vector2(5f);
            dp1.InnerScale = new Vector2(4.95f);
            dp1.Delay = 4700;
            dp1.DestoryAt = 1000;
            dp1.Radian = 2 * float.Pi;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp1);
        }
        if (basePos == HundredfoldHavoclistCorner[3])
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = "Hundredfold Havoc";
            dp1.Color = new Vector4(1.0f, 1f, 0.0f, 1.0f);
            dp1.Position = HundredfoldHavoclistCorner[7];
            dp1.Scale = new Vector2(5f);
            dp1.InnerScale = new Vector2(4.95f);
            dp1.Delay = 4700;
            dp1.DestoryAt = 1000;
            dp1.Radian = 2 * float.Pi;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp1);
        }
    }

    [ScriptMethod(name: "Bombogenesis", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0065"])]
    public void Bombogenesis(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)
        {
            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            if (index == 0 || index == 1)
            {
                if (isText) accessory.Method.TextInfo("Tanks go to point A to bait Bombogenesis", duration: 2500, true);
                accessory.TTS("Tanks go to point A to bait Bombogenesis", isTTS, isDRTTS);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Bombogenesis Tank Waypoint";
                dp.Owner = accessory.Data.Me;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.TargetPosition = new Vector3(-0.72f, -0.00f, -18.46f);
                dp.Scale = new(2);
                dp.DestoryAt = 4500;
                if (isLead) accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            if (index == 2 || index == 3)
            {
                if (isText) accessory.Method.TextInfo("Healers go to point B to bait Bombogenesis", duration: 2500, true);
                accessory.TTS("Healers go to point B to bait Bombogenesis", isTTS, isDRTTS);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Bombogenesis Healer Waypoint";
                dp.Owner = accessory.Data.Me;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.TargetPosition = new Vector3(16.48f, -0.00f, 10.22f);
                dp.Scale = new(2);
                dp.DestoryAt = 4500;
                if (isLead) accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            if (index > 3)
            {
                if (isText) accessory.Method.TextInfo("DPS go to point C to bait Bombogenesis", duration: 2500, true);
                accessory.TTS("DPS go to point C to bait Bombogenesis", isTTS, isDRTTS);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Bombogenesis DPS Waypoint";
                dp.Owner = accessory.Data.Me;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.TargetPosition = new Vector3(-17.41f, 0.00f, 7.78f);
                dp.Scale = new(2);
                dp.DestoryAt = 4500;
                if (isLead) accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
    }

    [ScriptMethod(name: "Bombogenesis Expand", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2009431"])]
    public void BombogenesisExpend(Event @event, ScriptAccessory accessory)
    {
        DebugMsg($"Operate: {@event.Operate()}", accessory);
        if (@event.Operate() == "Add")
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Bombogenesis Expand";
            dp.Color = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);
            dp.Position = @event.SourcePosition();
            dp.Scale = new Vector2(12f);
            dp.DestoryAt = 20000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            DebugMsg($"{@event.SourcePosition()}", accessory);
        }
        else
        {
            accessory.Method.RemoveDraw("Bombogenesis Expand");
        }
    }
    #endregion
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

public static class Extensions
{
    public static void TTS(this ScriptAccessory accessory, string text, bool isTTS, bool isDRTTS)
    {
        if (isDRTTS)
        {
            accessory.Method.SendChat($"/pdr tts {text}");
        }
        else if (isTTS)
        {
            accessory.Method.TTS(text);
        }
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

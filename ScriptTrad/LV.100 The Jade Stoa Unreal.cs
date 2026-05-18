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

[ScriptType(name: "LV.100 The Jade Stoa Unreal", territorys: [1239], guid: "d5f987d5-1170-4d06-b01b-9e18d681ad54",
    version: "0.0.0.4", author: "Linoa235", note: noteStr)]

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
        } else
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
        if (Boss == null) return;

        if (isMTGroup == true)
        {
            // MT Group logic
        }
        else
        {
            // ST Group logic
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

    // Additional Phase 1 mechanics: Ominous Wind Marker, Fire and Lightning, Hakutei, etc.
    #endregion

    #region Phase 2
    [ScriptMethod(name: "P2-1 Sweep the Leg", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39957"])]
    public void SweeptheLeg(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Center Dynamo", duration: 2000, true);
        accessory.TTS("Center Dynamo", isTTS, isDRTTS);

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

    [ScriptMethod(name: "Phase 2 Transition AOE", eventType: EventTypeEnum.Chat, eventCondition: ["Message:regex:^(åŒ–ä¸ºç°çƒ¬å§ï¼| To ashes with you! | å¡µèŠ¥ã¨æ¶ˆãˆã‚‹ãŒã‚ˆã„ï¼)$"])]
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

        // Position-based drawing logic (same as original)
    }

    [ScriptMethod(name: "Bombogenesis", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0065"])]
    public void Bombogenesis(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() == accessory.Data.Me)
        {
            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            // Role-based waypoint logic (same as original)
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
        } else
        {
            accessory.Method.RemoveDraw("Bombogenesis Expand");
        }
    }
    #endregion
}

// EventExtensions, Extensions, IbcHelper classes (same as previous files)
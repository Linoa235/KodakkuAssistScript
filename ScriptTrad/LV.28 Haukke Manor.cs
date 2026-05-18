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
using FFXIVClientStructs.FFXIV.Client.UI;
using System.Runtime.Intrinsics.Arm;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace Veever.A_Realm_Reborn.Haukke_Manor;

[ScriptType(name: "LV.28 Haukke Manor", territorys: [1040], guid: "cdcb203d-798a-4a7e-a883-497b3337be20",
    version: "0.0.0.4", author: "Linoa235", note: noteStr)]

public class Haukke_Manor
{
    const string noteStr =
    """
    v0.0.0.4:
    1. Green markers indicate keys to pick up, red indicates do not pick up
    2. If you need a draw for a mechanic or notice any issues, @ me on DC or DM me
    3. If you want Namazu markers, make sure ACT is open and Namazu plugin is installed
    Duckmen.
    """;

    [UserSetting("Text Banner Toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = true;

    [UserSetting("Waypoint Toggle")]
    public bool isLead { get; set; } = true;

    [UserSetting("Target Marker Toggle")]
    public bool isMark { get; set; } = true;

    [UserSetting("Local Target Marker Toggle (ON = local only, OFF = party)")]
    public bool LocalMark { get; set; } = true;

    [UserSetting("Waymark Guide Toggle")]
    public bool PostNamazuPrint { get; set; } = true;

    [UserSetting("PostNamazu Port Setting")]
    public int PostNamazuPort { get; set; } = 2019;

    [UserSetting("Waymarks Local Toggle (if non-local selected, script only places markers OOC)")]
    public bool PostNamazuisLocal { get; set; } = true;

    [UserSetting("Debug toggle, please turn off for non-development use")]
    public bool isDebug { get; set; } = false;

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        PostWaymark(accessory);
    }

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    #region Waymark
    private static readonly Vector3 posA = new Vector3(48.76f, 0.00f, 36.00f);
    private static readonly Vector3 posB = new Vector3(16.94f, 0.00f, 70.82f);
    private static readonly Vector3 posC = new Vector3(-36.62f, 0.00f, 16.15f);

    public void PostWaymark(ScriptAccessory accessory)
    {
        var waymark = new NamazuHelper.Waymark(accessory);
        waymark.AddWaymarkType("A", posA); 
        waymark.AddWaymarkType("B", posB);
        waymark.AddWaymarkType("C", posC);
        waymark.SetJsonPayload(LocalMark, PostNamazuisLocal);
        waymark.PostWaymarkCommand(PostNamazuPort);
    }
    #endregion

    #region Trash Mobs
    [ScriptMethod(name: "Dark Mist", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29776"])]
    public void DarkMist(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Move away or interrupt", duration: 3700, true);
        if (isTTS) accessory.Method.EdgeTTS("Move away or interrupt");
        if (isMark) accessory.Method.Mark(@event.SourceId(), KodakkuAssist.Module.GameOperate.MarkType.Attack1, LocalMark);
        DrawHelper.DrawCircleObject(accessory, @event.SourceId(), new Vector2(8f), 3700, $"DarkMist-{@event.SourceId()}", accessory.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "DarkMist Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:29776"], userControl: false)]
    public void DarkMistClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"DarkMist-{@event.SourceId()}");
    }

    [ScriptMethod(name: "Dread Gaze", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:513"])]
    public void DreadGaze(Event @event, ScriptAccessory accessory)
    {
        DrawHelper.DrawFanOwner(accessory, @event.SourceId(), 0, new Vector2(7.35f), 90, 2700, $"DreadGaze-{@event.SourceId()}", accessory.Data.DefaultDangerColor);
    }
    #endregion

    #region Boss1
    [ScriptMethod(name: "Void Fire II", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:855"])]
    public void VoidFireII(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Move away or interrupt", duration: 3700, true);
        if (isTTS) accessory.Method.EdgeTTS("Move away or interrupt");
        if (isMark) accessory.Method.Mark(@event.SourceId(), KodakkuAssist.Module.GameOperate.MarkType.Attack1, LocalMark);
        DrawHelper.DrawCircle(accessory, @event.EffectPosition(), new Vector2(5f), 2700, $"VoidFireII-{@event.SourceId()}", accessory.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Dark Mist", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:705"])]
    public void DarkMist1(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Move away", duration: 3700, true);
        if (isTTS) accessory.Method.EdgeTTS("Move away");
        if (isMark) accessory.Method.Mark(@event.SourceId(), KodakkuAssist.Module.GameOperate.MarkType.Attack1, LocalMark);
        DrawHelper.DrawCircle(accessory, @event.EffectPosition(), new Vector2(9.4f), 3700, $"DarkMist1-{@event.SourceId()}", accessory.Data.DefaultDangerColor);
    }
    #endregion

    #region Boss2
    [ScriptMethod(name: "Ice Spikes", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:859"])]
    public async void IceSpikes(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Interrupt the Manor Jester", duration: 3000, true);
        if (isTTS) accessory.Method.EdgeTTS("Interrupt the Manor Jester");
        if (isMark) accessory.Method.Mark(@event.SourceId(), KodakkuAssist.Module.GameOperate.MarkType.Bind1, LocalMark);
        await Task.Delay(4000);
        accessory.Method.MarkClear();
    }

    [ScriptMethod(name: "Soul Drain", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:860"])]
    public async void SoulDrain(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Stun the Manor Steward", duration: 3700, true);
        if (isTTS) accessory.Method.EdgeTTS("Stun the Manor Steward");
        if (isMark) accessory.Method.Mark(@event.SourceId(), KodakkuAssist.Module.GameOperate.MarkType.Bind1, LocalMark);
        await Task.Delay(4000);
        accessory.Method.MarkClear();
    }
    #endregion

    #region Boss3
    [ScriptMethod(name: "Petrifying Eye", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28648"])]
    public void PetrifyingEye(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        if (isText) accessory.Method.TextInfo("Turn away from the eye", duration: 4700, true);
        if (isTTS) accessory.Method.EdgeTTS("Turn away from the eye");
    }

    [ScriptMethod(name: "Maid Target Mark", eventType: EventTypeEnum.Targetable, eventCondition: ["DataId:14506", "Targetable:True"])]
    public void TargetMark(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Prioritize attacking the Attendant", duration: 4700, true);
        if (isTTS) accessory.Method.EdgeTTS("Prioritize attacking the Attendant");
        if (isMark) accessory.Method.Mark(@event.SourceId(), KodakkuAssist.Module.GameOperate.MarkType.Attack1, LocalMark);
    }

    [ScriptMethod(name: "Boss3 Dark Mist", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28646"])]
    public void Boss3DarkMist(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Move away or Leg Sweep to interrupt", duration: 3700, true);
        if (isTTS) accessory.Method.EdgeTTS("Move away or Leg Sweep to interrupt");
        DrawHelper.DrawCircle(accessory, @event.EffectPosition(), new Vector2(9f), 3700, $"DarkMist-{@event.SourceId()}", accessory.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Draw Clear", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:14504"], userControl: false)]
    public void DrawClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
    }
    #endregion
}

// Helper classes (EventExtensions, IbcHelper, NamazuHelper, DrawHelper - same as previous files)
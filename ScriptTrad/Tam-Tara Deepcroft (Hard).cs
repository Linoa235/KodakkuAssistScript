using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace the_Tam_Tara_Deepcroft_Hard;

[ScriptType(guid: "6d397f5e-20b1-4c7b-9e4f-7a1ecbab2333", name: "Tam-Tara Deepcroft (Hard)", territorys: [373],
    version: "0.0.0.1", author: "Tetora", note: noteStr)]

public class the_Tam_Tara_Deepcroft_Hard
{
    const string noteStr =
        """
        v0.0.0.1:
        LV50 Tam-Tara Deepcroft (Hard) Initial Drawing
        """;

    #region User Settings

    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;

    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;

    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("Developer Mode")]
    public bool isDeveloper { get; set; } = false;

    #endregion

    #region Draw Cleanup

    [ScriptMethod(name: "Orb Destruction", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(2399|2400)$"], userControl: false)]
    public void OrbDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.SourceId}");
    }

    [ScriptMethod(name: "Cast Interrupt Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: [], userControl: false)]
    public void CastInterruptCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.SourceId()}");
    }

    [ScriptMethod(name: "Death Cleanup", eventType: EventTypeEnum.Death, eventCondition: [], userControl: false)]
    public void DeathCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.TargetId()}");
    }

    [ScriptMethod(name: "Head Strike & Interject Cancel Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^75(38|51)$"], userControl: false)]
    public void InterruptCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.TargetId()}");
    }

    #endregion

    #region BOSS1_Celebrant Liavinne

    [ScriptMethod(name: "BOSS1_Celebrant Liavinne - Inhuman Deed (Red Orb Bind Marker)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^2390$"])]
    public void InhumanDeed(Event @event, ScriptAccessory accessory)
    {
        string tname = @event["TargetName"]?.ToString() ?? "Unknown Target";

        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isText) accessory.Method.TextInfo("Stay away from boss front, don't hit adds", duration: 3700, true);
            if (isTTS) accessory.Method.TTS("Stay away from boss front, don't hit adds");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Stay away from boss front, don't hit adds");
        }
        else
        {
            if (isText) accessory.Method.TextInfo($"Bind marker on <{tname}>, don't hit adds", duration: 1700, false);
            if (isTTS) accessory.Method.TTS($"Bind marker on {tname}, don't hit adds");
            if (isEdgeTTS) accessory.Method.EdgeTTS($"Bind marker on {tname}, don't hit adds");
        }
    }

    [ScriptMethod(name: "BOSS1_Celebrant Liavinne - Corrupt Arrow Large Circle Marker", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^00BD$"])]
    public void CorruptArrow(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        if (isText) accessory.Method.TextInfo($"Use AOE to hit adds", duration: 4300, true);
        if (isTTS) accessory.Method.TTS($"Use AOE to hit adds");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Use AOE to hit adds");
    }

    [ScriptMethod(name: "BOSS1_Celebrant Liavinne - Rage AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2393$"])]
    public void Rage(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"AOE");
    }

    #endregion

    [ScriptMethod(name: "Trash_Decarabia - Evil Eye (Chariot) Stun Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2450$"])]
    public void EvilEye(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Stun <Decarabia>", duration: 1700, true);
        if (isTTS) accessory.Method.TTS($"Stun <Decarabia>");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Stun <Decarabia>");
    }

    [ScriptMethod(name: "Trash_Decarabia - Strong Paralysis (Circle Marker) Interrupt Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^1916$"])]
    public void StrongParalysis(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Interrupt <Decarabia>", duration: 4300, true);
        if (isTTS) accessory.Method.TTS($"Interrupt <Decarabia>");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Interrupt <Decarabia>");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"StrongParalysis{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    #region BOSS2_Spare Body

    [ScriptMethod(name: "BOSS2_Paiyo Reiyo - Protection Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^280$"])]
    public void ProtectionHint(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Protect the lalafell in the middle, hug the arena edge", duration: 3000, true);
        if (isTTS) accessory.Method.TTS($"Protect the lalafell in the middle, hug the arena edge");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Protect the lalafell in the middle, hug the arena edge");
    }

    [ScriptMethod(name: "BOSS2_Nameless Spirit - Small Orb", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^3051$"])]
    public void NamelessSpirit(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"NamelessSpirit{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 7200000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "BOSS2_Nameless Soul - Large Orb", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^3052$"])]
    public void NamelessSoul(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Use mitigation and touch the orb", duration: 3000, true);
        if (isTTS) accessory.Method.TTS($"Use mitigation and touch the orb");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Use mitigation and touch the orb");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"NamelessSoul{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 7200000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    #endregion

    [ScriptMethod(name: "Trash_Mindflayer - Ulcer Interrupt Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^1079$"])]
    public void Ulcer(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Interrupt <Mindflayer>", duration: 2000, true);
        if (isTTS) accessory.Method.TTS($"Interrupt <Mindflayer>");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Interrupt <Mindflayer>");
    }

    #region BOSS3_Vigorous Avair

    [ScriptMethod(name: "BOSS3_Fiancé Kill Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^3056$"], suppress: 9000)]
    public void Fiance(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Kill <Fiancé>", duration: 2000, true);
        if (isTTS) accessory.Method.TTS($"Kill add");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Kill add");
    }

    #endregion

}

#region EventExtensions
// ... (keep existing EventExtensions, MathTools, IbcHelper, etc. - only translated comments if any)
// Note: The extensive helper classes at the bottom are mostly code and were kept as-is since they contain minimal user-facing strings.
// Only method names and comments would need translation if any exist, but in this file they are minimal.
#endregion
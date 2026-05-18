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
using System.Threading.Tasks;
using KodakkuAssist.Extensions;

namespace Thornmarch_Hard;

[ScriptType(guid: "a04a23b0-20a5-4381-93fe-3cef7ceccc9c", name: "Thornmarch (Hard) (Fun Edition)", territorys: [1067],
    version: "0.0.0.3", Author: "Linoa235", note: noteStr)]

public class Thornmarch_Hard
{
    const string noteStr =
        """
        v0.0.0.2:
        LV50 Thornmarch (Hard)
        Pure fun, no real meaning. Feel free to skip if you don't like it.
        Voice lines currently work for CN version, not adapted for other languages yet.
        """;
    
    [UserSetting("Voice Line TTS Toggle")]
    public bool isTTS { get; set; } = true;
    
    [UserSetting("Voice Line Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [UserSetting("Mechanic Popup Text Toggle")]
    public bool isText2 { get; set; } = true;
    
    [UserSetting("Print Bubble Lines to Chat")]
    public bool isSendMessage { get; set; } = true;
    
    [UserSetting("Mechanic Fill Animation Toggle")]
    public bool isFill { get; set; } = true;
    
    #region Voice Lines
    
    [ScriptMethod(name: "Those who rebel against the Moogle retinue will be utterly crushed, kupo!", userControl: false, eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements", "Message:åæŠ—èŽ«å¤åŠ›å®¶è‡£å›¢çš„äºº\nä¼šè¢«èŽ«å¤ä»¬å½»åº•å‡»æºƒåº“å•µï¼"])]
    public void WillBeUtterlyCrushedKupo(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Those who rebel against the Moogle retinue will be utterly crushed, kupo!", duration: 4000, true);
        if (isTTS) accessory.Method.EdgeTTS("Those who rebel against the Moogle retinue will be utterly crushed, kupo!");
    }
    
    [ScriptMethod(name: "Taste the moogle's axe, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1950"])]
    public void TasteTheMooglesAxeKupo(Event @event, ScriptAccessory accessory)
    {
        if (isSendMessage) accessory.Method.SendChat("/e Fluffy Axe KuputaÂ·Kapa: Taste the moogle's axe, kupo!");
    }
    
    [ScriptMethod(name: "Watch me shoot you in the butt, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1960"])]
    public void WatchMeShootYouInTheButtKupo(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Watch me shoot you in the butt, kupo!", duration: 4000, true);
        if (isTTS) accessory.Method.EdgeTTS("Watch me shoot you in the butt, kupo!");
        if (isSendMessage) accessory.Method.SendChat("/e Soft Bow KupokoÂ·Koji: Watch me shoot you in the butt, kupo!");
    }
    
    [ScriptMethod(name: "I can't believe we lost, kupo...", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1947"])]
    public void ICanBelieveWeLostKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("I can't believe we lost, kupo...");
        if (isSendMessage) accessory.Method.SendChat("/e Fluffy Wall KupudiÂ·Kupu: I can't believe we lost, kupo...");
    }
    
    [ScriptMethod(name: "I've been defeated, kupo...", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1952"])]
    public void IveBeenDefeatedKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("I've been defeated, kupo...");
        if (isSendMessage) accessory.Method.SendChat("/e Fluffy Axe KuputaÂ·Kapa: I've been defeated, kupo...");
    }
    
    [ScriptMethod(name: "I got taken down, kupo...", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1962"])]
    public void IGotTakenDownKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("I got taken down, kupo...");
        if (isSendMessage) accessory.Method.SendChat("/e Soft Bow KupokoÂ·Koji: I got taken down, kupo...");
    }
    
    [ScriptMethod(name: "Want to be skewered, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1955"])]
    public void WantToBeSkeweredKupo(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Want to be skewered, kupo!", duration: 4000, true);
        if (isTTS) accessory.Method.EdgeTTS("Want to be skewered, kupo!");
        if (isSendMessage) accessory.Method.SendChat("/e Fluffy Heal KuproÂ·Chip: Want to be skewered, kupo!");
    }
    
    [ScriptMethod(name: "Come dance with the moogles, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1573"])]
    public void ComeDanceWithTheMooglesKupo(Event @event, ScriptAccessory accessory)
    {
        if (isSendMessage) accessory.Method.SendChat("/e Soft Sound PukuhiÂ·Piko: Come dance with the moogles, kupo!");
    }
    
    [ScriptMethod(name: "Moogle lost, kupo...", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1957"])]
    public void MoogleLostKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("Moogle lost, kupo...");
        if (isSendMessage) accessory.Method.SendChat("/e Fluffy Heal KuproÂ·Chip: Moogle lost, kupo...");
    }
    
    [ScriptMethod(name: "I can't sing anymore, kupo...", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1975"])]
    public void ICanSingAnymoreKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("I can't sing anymore, kupo...");
        if (isSendMessage) accessory.Method.SendChat("/e Soft Sound PukuhiÂ·Piko: I can't sing anymore, kupo...");
    }
    
    [ScriptMethod(name: "Playing with fire is so fun, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1965"])]
    public void PlayingWithFireIsSoFunKupo(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Playing with fire is so fun, kupo!", duration: 4000, true);
        if (isTTS) accessory.Method.EdgeTTS("Playing with fire is so fun, kupo!");
        if (isSendMessage) accessory.Method.SendChat("/e Soft Strength PukuraÂ·Puchi: Playing with fire is so fun, kupo!");
    }
    
    [ScriptMethod(name: "D-Do we really have to fight, kupo?", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1978"])]
    public void DoWeReallyHaveToFightKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("D-Do we really have to fight, kupo?");
        if (isSendMessage) accessory.Method.SendChat("/e Fluffy Barrier PukunaÂ·Pako: D-Do we really have to fight, kupo?");
    }
    
    [ScriptMethod(name: "Focus power into the Fluffy Meteor and smash you to pieces, kupo!", userControl: false, eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements", "Message:å°†åŠ›é‡é›†ä¸­åˆ°ç»’ç»’é™¨çŸ³ä¸Šï¼Œ\næŠŠä½ ä»¬ç ¸çƒ‚åº“å•µï¼"])]
    public void SmashYouToPiecesKupo(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Focus power into the Fluffy Meteor and smash you to pieces, kupo!", duration: 4000, true);
        if (isTTS) accessory.Method.EdgeTTS("Focus power into the Fluffy Meteor and smash you to pieces, kupo!");
    }
    
    [ScriptMethod(name: "The moogle has burned out, kupo...", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1967"])]
    public void MoogleHasBurnedOutKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("The moogle has burned out, kupo...");
        if (isSendMessage) accessory.Method.SendChat("/e Soft Strength PukuraÂ·Puchi: The moogle has burned out, kupo...");
    }
    
    [ScriptMethod(name: "So scary, kupo...", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1980"])]
    public void SoScaryKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("So scary, kupo...");
        if (isSendMessage) accessory.Method.SendChat("/e Fluffy Barrier PukunaÂ·Pako: So scary, kupo...");
    }
    
    [ScriptMethod(name: "Y-You're pretty good, kupo... But the Good King will take care of you, kupo!", userControl: false, eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements", "Message:æŒºã€æŒºåŽ‰å®³çš„å˜›åº“å•µâ€¦â€¦\nä½†è´¤çŽ‹å¤§äººä¼šæ”¶æ‹¾ä½ ä»¬çš„åº“å•µï¼"])]
    public void GoodKingWillTakeCareOfYouKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("Y-You're pretty good, kupo... But the Good King will take care of you, kupo!");
    }
    
    [ScriptMethod(name: "Oh Good King, please deliver your judgment, kupo!", userControl: false, eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements", "Message:è´¤çŽ‹å¤§äººå•Šï¼Œ\nè¯·é™ä¸‹æ‚¨çš„åˆ¶è£åº“å•µï¼"])]
    public void PleaseDeliverYourJudgmentKupo(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Oh Good King, please deliver your judgment, kupo!", duration: 4000, true);
        if (isTTS) accessory.Method.EdgeTTS("Oh Good King, please deliver your judgment, kupo!");
    }
    
    [ScriptMethod(name: "Oh Good King... what was the second line again, kupo?", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1982"])]
    public void WhatWasTheSecondLineAgainKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("Oh Good King... what was the second line again, kupo?");
        if (isSendMessage) accessory.Method.SendChat("/e Fluffy Barrier PukunaÂ·Pako: Oh Good King... what was the second line again, kupo?");
    }
    
    [ScriptMethod(name: "Death Moogle Warning! You will pay for your defiance, kupo!", userControl: false, eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements", "Message:æ­»äº¡èŽ«å¤è­¦å‘Šï¼\nä½ ä»¬ä¼šä¸ºå¿¤é€†è¡Œä¸ºä»˜å‡ºä»£ä»·åº“å•µï¼"])]
    public void YouWillPayForYourDefianceKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("Death Moogle Warning! You will pay for your defiance, kupo!");
    }
    
    [ScriptMethod(name: "The moogles' battle has only just begun, kupo!", userControl: false, eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements", "Message:èŽ«å¤ä»¬çš„æˆ˜æ–—æ‰åˆšåˆšå¼€å§‹åº“å•µï¼"])]
    public void BattleHasOnlyJustBegunKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("The moogles' battle has only just begun, kupo!");
    }
    
    [ScriptMethod(name: "Retainers, it's your turn to step up, kupo!", userControl: false, eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements", "Message:å®¶è‡£ä»¬ï¼Œ\nåˆ°ä½ ä»¬å‡ºé©¬çš„æ—¶å€™äº†åº“å•µï¼"])]
    public void YourTurnToStepUpKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("Retainers, it's your turn to step up, kupo!");
    }
    
    [ScriptMethod(name: "L-Let me show you the moogle's true power, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13788"])]
    public void ShowYouTruePowerKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("L-Let me show you the moogle's true power, kupo!");
        if (isSendMessage) accessory.Method.SendChat("/e Fluffy Barrier PukunaÂ·Pako: L-Let me show you the moogle's true power, kupo!");
    }
    
    [ScriptMethod(name: "Taste the Scattershot Arrow, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1963"])]
    public void TasteScattershotArrowKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("Taste the Scattershot Arrow, kupo!");
        if (isSendMessage) accessory.Method.SendChat("/e Soft Bow KupokoÂ·Koji: Taste the Scattershot Arrow, kupo!");
    }
    
    [ScriptMethod(name: "Sink into the poisonous bog, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:1981"])]
    public void SinkIntoPoisonousBogKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("Sink into the poisonous bog, kupo!");
        if (isSendMessage) accessory.Method.SendChat("/e Fluffy Barrier PukunaÂ·Pako: Sink into the poisonous bog, kupo!");
    }
    
    [ScriptMethod(name: "Retainers, deliver judgment upon the wicked, kupo!", userControl: false, eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements", "Message:å®¶è‡£ä»¬ï¼Œ\nç»™åäººä»¥åˆ¶è£åº“å•µï¼"])]
    public void DeliverJudgmentUponTheWickedKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("Retainers, deliver judgment upon the wicked, kupo!");
    }
    
    [ScriptMethod(name: "Know your place, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13781"])]
    public void KnowYourPlaceKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("Know your place, kupo!");
        if (isSendMessage) accessory.Method.SendChat("/e Good King Moggle Mog XII: Know your place, kupo!");
    }
    
    [ScriptMethod(name: "I'll break you apart, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13783"])]
    public void IllBreakYouApartKupo(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("I'll break you apart, kupo!\nI'll beat you senseless, kupo!", duration: 8700, true);
        if (isTTS) accessory.Method.EdgeTTS("I'll break you apart, kupo!");
        if (isSendMessage) accessory.Method.SendChat("/e Fluffy Axe KuputaÂ·Kapa: I'll break you apart, kupo!");
    }
    
    [ScriptMethod(name: "I'll beat you senseless, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13782"])]
    public void BeatYouSenselessKupo(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("I'll break you apart, kupo!\nI'll beat you senseless, kupo!", duration: 8700, true);
        if (isTTS) accessory.Method.EdgeTTS("I'll beat you senseless, kupo!");
        if (isSendMessage) accessory.Method.SendChat("/e Fluffy Wall KupudiÂ·Kupu: I'll beat you senseless, kupo!");
    }
    
    [ScriptMethod(name: "Listen to a moogle's song, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13787"])]
    public void ListenToMooglesSongKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("Listen to a moogle's song, kupo!");
        if (isSendMessage) accessory.Method.SendChat("/e Soft Sound PukuhiÂ·Piko: Listen to a moogle's song, kupo!");
    }
    
    [ScriptMethod(name: "Retainers, join hands and fight the enemy, kupo!", userControl: false, eventType: EventTypeEnum.Chat, eventCondition: 
        ["Type:NPCDialogueAnnouncements", "Message:å®¶è‡£ä»¬ï¼Œ\nè”èµ·æ‰‹æ¥ï¼Œå¯¹æŠ—æ•Œäººåº“å•µï¼"])]
    public void JoinHandsAndFightKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("Retainers, join hands and fight the enemy, kupo!");
    }
    
    [ScriptMethod(name: "The moogle's magic is burning bright, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13789"])]
    public void MagicIsBurningBrightKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("The moogle's magic is burning bright, kupo!");
        if (isSendMessage) accessory.Method.SendChat("/e Soft Strength PukuraÂ·Puchi: The moogle's magic is burning bright, kupo!");
    }
    
    [ScriptMethod(name: "I'll crush you, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13784"])]
    public void IllCrushYouKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("I'll crush you, kupo!");
        if (isSendMessage) accessory.Method.SendChat("/e Good King Moggle Mog XII: I'll crush you, kupo!");
    }
    
    [ScriptMethod(name: "Take a moogle's strike, kupo!", userControl: false, eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:13785"])]
    public void TakeMooglesStrikeKupo(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.EdgeTTS("Take a moogle's strike, kupo!");
        if (isSendMessage) accessory.Method.SendChat("/e Fluffy Heal KuproÂ·Chip: Take a moogle's strike, kupo!");
    }
    #endregion
    
    #region Mechanics
    
    [ScriptMethod(name: "Spinning Moogle Shield (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29216"])]
    public void SpinningMoogleShield(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Spinning Moogle Shield";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        if (isFill) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Spinning Moogle Shield Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:29216"], userControl: false)]
    public void SpinningMoogleShieldCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Spinning Moogle Shield");
    }
    
    [ScriptMethod(name: "Moogle Death Rain (Spread)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29191"])]
    public void MoogleDeathRain(Event @event, ScriptAccessory accessory)
    {
        if (isText2) accessory.Method.TextInfo("Spread out", duration: 2500, true);
        accessory.Method.EdgeTTS("Spread out");
    }
    
    [ScriptMethod(name: "Hundred Kupo Sweep (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29215"])]
    public void HundredKupoSweep(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Hundred Kupo Sweep";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 4700;
        if (isFill) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Hundred Kupo Sweep Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:29215"], userControl: false)]
    public void HundredKupoSweepCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Hundred Kupo Sweep");
    }

    [ScriptMethod(name: "Fluffy Holy (AOE)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2921[01]$"])]
    public void FluffyHoly(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "Moogle Nocturne (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29213"])]
    public void MoogleNocturne(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Moogle Nocturne";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 4700;
        if (isFill) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Revelry", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3080"], userControl: false)]
    public void Revelry(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.TextInfo("Why didn't you dodge the orange circle?", duration: 11000, true);
        accessory.Method.EdgeTTS("Why didn't you dodge the orange circle?");
    }
    
    [ScriptMethod(name: "Moogle Nocturne Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:29213"], userControl: false)]
    public void MoogleNocturneCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Moogle Nocturne");
    }
    
    [ScriptMethod(name: "Fluffy Meteor (Tower)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29193"])]
    public void FluffyMeteor(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS("Soak tower");
    }
    
    [ScriptMethod(name: "Moogle Thrust (Tankbuster)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29214"])]
    public void MoogleThrust(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS($"Tankbuster on {@event.TargetName()}");
    }
    
    [ScriptMethod(name: "Death Moogle Warning (AOE)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29217"])]
    public void DeathMoogleWarning(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS("AOE");
    }
    
    [ScriptMethod(name: "Good King's Decree", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2918[89]|29190)$"])]
    public void GoodKingsDecree(Event @event, ScriptAccessory accessory)
    {
        switch (@event.ActionId())
        {
            case 29188:
            {
                if (isText2) accessory.Method.TextInfo("Phase 1: Stay away from poison, stack > spread, then AOE", duration: 3700, true);
            }
                break;
            case 29189:
            {
                if (isText2) accessory.Method.TextInfo("Phase 2: Three-through-one big circle, avoid cleave, stack tankbuster then AOE", duration: 3700, true);
            }
                break;
            case 29190:
            {
                if (isText2) accessory.Method.TextInfo("Phase 3: Dodge triple orange circles, soak towers, stack, dodge chariot/donut, then AOE + tankbuster", duration: 3700, true);
            }
                break;
        }
    }
    
    [ScriptMethod(name: "Fluffy Bog (Poison Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29207"])]
    public void FluffyBog(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS("Avoid poison pool AOE");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fluffy Bog";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 5000;
        if (isFill) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Deadly Poison", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3082"], userControl: false)]
    public void DeadlyPoison(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.TextInfo("Why did you eat that?", duration: 3000, true);
        accessory.Method.EdgeTTS("Why did you eat that?");
    }
    
    [ScriptMethod(name: "Moogle Stone (Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29204"])]
    public void MoogleStone(Event @event, ScriptAccessory accessory)
    {
        if (isText2) accessory.Method.TextInfo($"Stack with {@event.TargetName()}", duration: 5300, true);
        accessory.Method.EdgeTTS($"Stack with {@event.TargetName()}");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Moogle Stone";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 6000;
        if (isFill) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Moogle Airborne (Large Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^2919[67]$"])]
    public void MoogleAirborne(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Moogle Airborne";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.DestoryAt = 8700;
        if (isFill) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Fluffy Double Meteor (Stack Tankbuster)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29206"])]
    public void FluffyDoubleMeteor(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS($"Stack tankbuster on {@event.TargetName()}");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fluffy Double Meteor";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 5000;
        if (isFill) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Moogle Comet (Guide Orange Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29198"])]
    public void MoogleCometHint(Event @event, ScriptAccessory accessory)
    {
        if (isText2) accessory.Method.TextInfo("Guide triple orange circles", duration: 7700, true);
        accessory.Method.EdgeTTS("Guide triple orange circles");
    }
    
    [ScriptMethod(name: "Moogle Comet (Triple Orange Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29199"])]
    public void MoogleComet(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Moogle Comet";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        if (isFill) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Fluffy Boulder (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29619"])]
    public void FluffyBoulderDonut(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fluffy Boulder Donut";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(20f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Fluffy Boulder (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29201"])]
    public void FluffyBoulderChariot(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fluffy Boulder Chariot";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 4700;
        if (isFill) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Good King's Forest (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29208"])]
    public void GoodKingsForest(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Good King's Forest";
        dp.Scale = new (10, 50f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Thousand Kupo Charge (Tankbuster)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29209"])]
    public void ThousandKupoCharge(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.EdgeTTS($"Tankbuster on {@event.TargetName()}");
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

    public static uint Command(this Event @event)
    {
        return ParseHexId(@event["Command"], out var cid) ? cid : 0;
    }
    
    public static uint DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["DurationMilliseconds"]);
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
}

#region Math Functions

public static class MathTools
{
    public static float DegToRad(this float deg) => (deg + 360f) % 360f / 180f * float.Pi;
    public static float RadToDeg(this float rad) => (rad + 2 * float.Pi) % (2 * float.Pi) / float.Pi * 180f;

    public static float GetRadian(this Vector3 point, Vector3 center)
        => MathF.Atan2(point.X - center.X, point.Z - center.Z);

    public static float GetLength(this Vector3 point, Vector3 center)
        => new Vector2(point.X - center.X, point.Z - center.Z).Length();

    public static Vector3 RotateAndExtend(this Vector3 point, Vector3 center, float radian, float length)
    {
        var baseRad = point.GetRadian(center);
        var baseLength = point.GetLength(center);
        var rotRad = baseRad + radian;
        return new Vector3(
            center.X + MathF.Sin(rotRad) * (length + baseLength),
            center.Y,
            center.Z + MathF.Cos(rotRad) * (length + baseLength)
        );
    }

    public static int RadianToRegion(this float radian, int regionNum, int baseRegionIdx = 0, bool isDiagDiv = false, bool isCw = false)
    {
        var sepRad = float.Pi * 2 / regionNum;
        var inputAngle = radian * (isCw ? -1 : 1) + (isDiagDiv ? sepRad / 2 : 0);
        var rad = (inputAngle + 4 * float.Pi) % (2 * float.Pi);
        return ((int)Math.Floor(rad / sepRad) + baseRegionIdx + regionNum) % regionNum;
    }

    public static Vector3 FoldPointHorizon(this Vector3 point, float centerX)
        => point with { X = 2 * centerX - point.X };

    public static Vector3 FoldPointVertical(this Vector3 point, float centerZ)
        => point with { Z = 2 * centerZ - point.Z };

    public static Vector3 PointCenterSymmetry(this Vector3 point, Vector3 center)
        => point.RotateAndExtend(center, float.Pi, 0);

    public static int GetDecimalDigit(this int val, int x)
    {
        var valStr = val.ToString();
        var length = valStr.Length;
        if (x < 1 || x > length) return -1;
        var digitChar = valStr[length - x];
        return int.Parse(digitChar.ToString());
    }
}

#endregion
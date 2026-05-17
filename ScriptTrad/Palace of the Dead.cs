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
using System.ComponentModel;

namespace the_Palace_of_the_Dead;

[ScriptType(name: "Palace of the Dead", territorys: [], guid: "cc8c4022-8b31-43d2-a865-18188907cbc7", version: "0.0.0.1", Author: "Linoa235")]

public class the_Palace_of_the_Dead
{
    const string noteStr =
        """
        v0.0.1.1:
        Palace of the Dead Drawing
        Note: Floor numbers in method settings are only for separation, not batch toggles.
        Please bring ARR feedback if issues occur!
        """;
    
    // 70 BOSS and 170 BOSS share conditions, need to check sid. 170 BOSS damage buff also conflicts with self-buff, need sid check.
    // [Completed] 1~99 floors and 101~199 floors have many duplicate monster skill IDs, need sid check.
    
    #region Records
    
    /* StatusID
     * Transformation 565 [StackCount:42 Manticore] [StackCount:43 Succubus]
     * Curse 1087 (Mimic - Resentment)
     * Blind 1088
     * Max HP Reduction 1089
     * Damage Reduction 1090
     * Haste 1091
     * Ability Seal 1092
     * Max HP/MP Increase 1093
     * Item Ban 1094
     * Sprint Ban 1095
     * Knockback Immunity 1096
     * Natural HP Regen Ban 1097
     * Skill Ban 1113 (Morph)
     */
    
    #endregion
    
    #region Basic Controls
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [UserSetting("Enable Cheats (Requires corresponding plugin & permissions)")]
    public bool isHack { get; set; } = false;
    
    [UserSetting(note: "Select default Y-adjust depth")]
    public DepthsEnum Depths { get; set; } = DepthsEnum.Default;
    
    public enum DepthsEnum
    {
        [Description("0")]
        Default = 0,
        [Description("2")]
        Depths2 = 1,
        [Description("3")]
        Depths3 = 2,
        [Description("5")]
        Depths5 = 3,
        [Description("7")]
        Depths7 = 4,
        [Description("20")]
        Depths20 = 5,
        [Description("50")]
        Depths50 = 6,
    }
    
    [UserSetting("Developer Mode")]
    public bool isDeveloper { get; set; } = false;
    
    #endregion
    
    #region Global Variables
    
    [ScriptMethod(name: "Head Strike & Interject Cancel Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^75(38|51)$"], userControl: false)]
    public void InterruptCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "Special Status Cleanup", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(2|3|1511|1113)$"], userControl: false)]
    public void SpecialStatusCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.TargetId()}");
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
    
    [ScriptMethod(name: "Anti-Knockback Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"], userControl: false)]
    public void AntiKnockbackCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw(".*(Knockback|Suck).*");
    }
    
    public bool KnockPenalty = false;
    
    [ScriptMethod(name: "Weather: Knockback Immunity Added", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1096"], userControl: false)]
    public void KnockbackBuffAdded(Event @event, ScriptAccessory accessory)
    {
        KnockPenalty = true;
    }
    
    [ScriptMethod(name: "Weather: Knockback Immunity Removed", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:1096"], userControl: false)]
    public void KnockbackBuffRemoved(Event @event, ScriptAccessory accessory)
    {
        KnockPenalty = false;
    }
    
    private int timesRemedyBomb = 0;
    private int Sap = 0;
    
    public void Init(ScriptAccessory accessory) {
        timesRemedyBomb = 0;
        Sap = 0;
    }
    
    [ScriptMethod(name: "Death Reset", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:0"], userControl: false)]
    public void DeathReset(Event @event, ScriptAccessory accessory)
    {
        timesRemedyBomb = 0;
        Sap = 0;
        if (isDeveloper) accessory.Method.SendChat($"/e Debug Info: Death detected, variables reset");
    }
    
    #endregion
    
    // General Content
    [ScriptMethod(name: "Mimic_Resentment Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6397"])]
    public void Mimic_Resentment(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Interrupt the mimic", duration: 2300, true);
        if (isTTS) accessory.Method.TTS("Interrupt the mimic");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Interrupt the mimic");
    }
    
    #region 1~10 Floors - Nothing to draw
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 1 ~ 10 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor1(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region 11~20 Floors - Nothing to draw
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 11 ~ 20 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor11(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region 21~30 Floors
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 21 ~ 30 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor21(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Palace Minotaur_110-tonze Swing (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6364", "SourceDataId:5802"])]
    public void PalaceMinotaur_110TonzeSwing(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Minotaur_110-tonze Swing{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.8f);
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Palace Scannit_Chittering (Sleep Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6365", "SourceDataId:5803"])]
    public void Scannit_Chittering(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Scannit_Chittering{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 0.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(21.6f);
        dp.DestoryAt = 2200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "30 Ningishzida_Fear Mist (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6429"])]
    public void Ningishzida_FearMist(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Get close to the boss in the middle", duration: 5300, true);
        if (isTTS) accessory.Method.TTS("Get close to the boss in the middle");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Get close to the boss in the middle");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Ningishzida_Fear Mist";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(58.8f);
        dp.InnerScale = new Vector2(4.8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    #endregion
    
    #region 31~40 Floors
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 31 ~ 40 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor31(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Nightmare Gourami_Suction (Pull)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6372", "SourceDataId:5811"])]
    public void NightmareGourami_Suction(Event @event, ScriptAccessory accessory)
    {
        if (!KnockPenalty) {
            if (isTTS) accessory.Method.TTS("Pull");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Pull");
        }
    }
    #endregion
    
    #region 41~50 Floors
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 41 ~ 50 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor41(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Nightmare Manticore_Ripping Claw (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6394", "SourceDataId:5827"])]
    public void NightmareManticore_RippingClaw(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties(); 
        dp.Name = $"Nightmare Manticore_Ripping Claw{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.2f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 1900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    #endregion
    
    #region 51~60 Floors
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 51 ~ 60 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor51(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "60 Dark Knight_Carnage (Knockback)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7089"])]
    public void DarkKnight_Carnage(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Knockback to safe zone", duration: 3300, true);
        if (isTTS) accessory.Method.TTS("Knockback to safe zone");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Knockback to safe zone");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Dark Knight_Carnage Knockback";
        dp.Scale = new(1.6f, 25f);
        dp.Color = new Vector4(0f, 1f, 1f, 3f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 3400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "60 Shadow Demon_Resentment Chariot Early Display", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6374"])]
    public void ShadowDemon_Resentment(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Shadow Demon_Resentment";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 5500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    #endregion
    
    #region 61~70 Floors
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 61 ~ 70 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor61(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region 71~80 Floors
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 71 ~ 80 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor71(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Palace Cyclops_Hundred-tonze Swing (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6971", "SourceDataId:6203"])]
    public void PalaceCyclops_HundredTonzeSwing(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Cyclops_Hundred-tonze Swing{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.8f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Palace Cyclops_Evil Eye (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6972", "SourceDataId:6203"])]
    public void PalaceCyclops_EvilEye(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Cyclops_Evil Eye{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(4f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "80 Gudanna_Pull (No Real Meaning) [Shared with Floor 180]", eventType: EventTypeEnum.StartCasting, eventCondition: ["DataId:01"])]
    public void Gudanna_Pull(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region 81~90 Floors
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 81 ~ 90 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor81(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Palace Chimera_Ice Roar (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7078", "SourceDataId:6220"])]
    public void PalaceChimera_IceRoar(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Chimera_Ice Roar{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9.7f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Palace Chimera_Thunder Roar (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7079", "SourceDataId:6220"])]
    public void PalaceChimera_ThunderRoar(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Chimera_Thunder Roar{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(6.7f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "90 Lava Bomb_Self-Destruct (Chariot)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6377"])]
    public void LavaBomb_SelfDestruct(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Lava Bomb_Self-Destruct{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6.6f);
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "90 Gray Bomb_Kill Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6376"])]
    public void GrayBomb_KillHint(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Kill the Gray Bomb", duration: 3000, true);
        if (isTTS) accessory.Method.TTS("Kill the Gray Bomb");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Kill the Gray Bomb");
    }
    
    [ScriptMethod(name: "90 Stun Bomb_Frost Shot Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6378"])]
    public void StunBomb_FrostShot(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Push the Stun Bomb to the boss's feet", duration: 3000, true);
        if (isTTS) accessory.Method.TTS("Push the Stun Bomb to the boss's feet");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Push the Stun Bomb to the boss's feet");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Stun Bomb_Frost Shot";
        dp.Color = new Vector4(1f, 0f, 0f, 1.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.2f);
        dp.DestoryAt = 24700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Stun Bomb Frost Shot Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:7104"], userControl: false)]
    public void StunBomb_FrostShotCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Stun Bomb_Frost Shot");
    }
    #endregion
    
    #region 91~100 Floors - Nothing to draw
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 91 ~ 100 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor91(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region 101~110 Floors - Nothing to draw
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 101 ~ 110 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor101(Event @event, ScriptAccessory accessory) { }
    #endregion
    
    #region 111~120 Floors
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 111 ~ 120 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor111(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Palace Salamander_Mucous Membrane Interrupt Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7014", "SourceDataId:6249"])]
    public void PalaceSalamander_MucousMembrane(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Interrupt <Palace Salamander>", duration: 2300, true);
        if (isTTS) accessory.Method.TTS("Interrupt Palace Salamander");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Interrupt Palace Salamander");
    }
    #endregion
    
    #region 121~130 Floors
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 121 ~ 130 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor121(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Palace Scannit_Chittering (Sleep Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6365", "SourceDataId:6267"])]
    public void PalaceScannit_Chittering(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Scannit_Chittering{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 0.1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(21.6f);
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Palace Minotaur_110-tonze Swing (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6364", "SourceDataId:6266"])]
    public void PalaceMinotaur2_110TonzeSwing(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Minotaur_110-tonze Swing{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.8f);
        dp.DestoryAt = 4200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "130 Alphard_Fear Mist (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7141"])]
    public void Alphard_FearMist(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Get close to the boss in the middle", duration: 1300, true);
        if (isTTS) accessory.Method.TTS("Get close to the boss in the middle");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Get close to the boss in the middle");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Alphard_Fear Mist";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(58.8f);
        dp.InnerScale = new Vector2(4.8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    #endregion
    
    #region 131~140 Floors
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 131 ~ 140 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor131(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Palace Ghostfish_Suction (Pull)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6372", "SourceDataId:6274"])]
    public void PalaceGhostfish_Suction(Event @event, ScriptAccessory accessory)
    {
        if (!KnockPenalty) {
            if (isTTS) accessory.Method.TTS("Pull then chariot");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Pull then chariot");
        }
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Ghostfish_Suction{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Palace Ghostfish_Flood (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6373", "SourceDataId:6274"])]
    public void PalaceGhostfish_Flood(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Ghostfish_Flood{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Palace Ahriman_Level 5 Petrify (Cone)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7031", "SourceDataId:6272"])]
    public void PalaceAhriman_Level5Petrify(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Ahriman_Level 5 Petrify{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.1f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    #endregion
    
    #region 141~150 Floors
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 141 ~ 150 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor141(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Palace Wraith_Strong Paralysis Interrupt Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6386", "SourceDataId:6284"])]
    public void PalaceWraith_StrongParalysis(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Interrupt <Palace Wraith>", duration: 4300, true);
        if (isTTS) accessory.Method.TTS("Interrupt <Palace Wraith>");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Interrupt <Palace Wraith>");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Wraith_Strong Paralysis{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 0.6f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Palace Mask_Strong Paralysis Interrupt Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6388", "SourceDataId:6286"])]
    public void PalaceMask_StrongParalysis(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Interrupt <Palace Mask>", duration: 4300, true);
        if (isTTS) accessory.Method.TTS("Interrupt <Palace Mask>");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Interrupt <Palace Mask>");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Mask_Strong Paralysis{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 0.6f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Palace Manticore_Ripping Claw (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6394", "SourceDataId:6289"])]
    public void PalaceManticore_RippingClaw(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties(); 
        dp.Name = $"Palace Manticore_Ripping Claw{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.2f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 1900;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    #endregion
    
    #region 151~160 Floors
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 151 ~ 160 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor151(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Palace Imp_Ice Thorn Barrier Interrupt Alert", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6943", "SourceDataId:6295"])]
    public void PalaceImp_IceThornBarrier(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Interrupt <Palace Imp>", duration: 800, true);
        if (isTTS) accessory.Method.TTS("Interrupt <Palace Imp>");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Interrupt <Palace Imp>");
    }
    
    [ScriptMethod(name: "160 Non-living Knight_Carnage (Knockback)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7156"])]
    public void NonLivingKnight_Carnage(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Knockback to safe zone", duration: 3300, true);
        if (isTTS) accessory.Method.TTS("Knockback to safe zone");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Knockback to safe zone");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Non-living Knight_Carnage Knockback";
        dp.Scale = new(1.6f, 25f);
        dp.Color = new Vector4(0f, 1f, 1f, 3f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 3400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "160 Greater Shadow_Resentment Chariot Early Display", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6383"])]
    public void GreaterShadow_Resentment(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Greater Shadow_Resentment";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    #endregion
    
    #region 161~170 Floors
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 161 ~ 170 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor161(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Palace Diplocaulus_Mucous Membrane Interrupt Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7014", "SourceDataId:6305"])]
    public void PalaceDiplocaulus_MucousMembrane(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Interrupt <Palace Diplocaulus>", duration: 2300, true);
        if (isTTS) accessory.Method.TTS("Interrupt <Palace Diplocaulus>");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Interrupt <Palace Diplocaulus>");
    }
    #endregion
    
    #region 171~180 Floors Trash
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 171 ~ 180 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor171(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Palace Yeti_Glare (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7061", "SourceDataId:6317"])]
    public void PalaceYeti_Glare(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Yeti_Glare{@event.SourceId()}";
        dp.Scale = new (7, 21f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Palace Yeti_Hundred-tonze Swing (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6971", "SourceDataId:6317"])]
    public void PalaceYeti_HundredTonzeSwing(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Yeti_Hundred-tonze Swing{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Palace Gigantopithecus_Chest Thump (Out-of-Combat Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6973", "SourceDataId:6318"])]
    public void PalaceGigantopithecus_ChestThump(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Gigantopithecus_Chest Thump{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0.8f, 0f, 0.3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(53.6f);
        dp.Delay = 1700;
        dp.DestoryAt = 15300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Palace Gigantopithecus Chest Thump Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:6499", "SourceDataId:6318"], userControl: false)]
    public void PalaceGigantopithecus_ChestThumpCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Palace Gigantopithecus_Chest Thump{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Palace Gigantopithecus Chest Thump Cleanup Backup", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:53", "SourceDataId:6318"], userControl: false)]
    public void PalaceGigantopithecus_ChestThumpCleanupBackup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Palace Gigantopithecus_Chest Thump{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Palace Harpy_Blissful Wind Interrupt Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7060", "SourceDataId:6316"])]
    public void PalaceHarpy_BlissfulWind(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Interrupt <Palace Harpy>", duration: 2000, true);
        if (isTTS) accessory.Method.TTS("Interrupt <Palace Harpy>");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Interrupt <Palace Harpy>");
    }
    #endregion

    #region Floor 180 Boss Dandaine Sognet
    [ScriptMethod(name: "180 Gudanna & Dandaine Sognet_Pull (Chariot)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6384"])]
    public void DandaineSognet_Pull(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Dandaine Sognet_Pull{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 44000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Gudanna & Dandaine Sognet Pull Cleanup", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:6384"], userControl: false)]
    public void PalaceGigantopithecus_PullCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Dandaine Sognet_Pull{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "180 Dandaine Sognet_Ecliptic Meteor", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7166"])]
    public void DandaineSognet_EclipticMeteor(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("80% true damage", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("80% true damage");
        if (isEdgeTTS) accessory.Method.EdgeTTS("80% true damage");
    }
    #endregion

    #region 181~190 Floors Trash
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 181 ~ 190 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor181(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "Palace Garm_Ice Roar (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7078", "SourceDataId:6335"])]
    public void PalaceGarm_IceRoar(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Garm_Ice Roar{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.4f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Palace Garm_Thunder Roar (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7078", "SourceDataId:6335"])]
    public void PalaceGarm_ThunderRoar(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Garm_Thunder Roar{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(7.4f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    #endregion

    #region Floor 190 Boss Bomb Father
    [ScriptMethod(name: "190 Stun Bomb_Ice Shard (Chariot)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6387"])]
    public void StunBomb_IceShard(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Stun Bomb_Ice Shard";
        dp.Color = new Vector4(1f, 0.5f, 0f, 1.5f);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6.6f);
        dp.DestoryAt = 8400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "190 Remedy Bomb Kill Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6385"])]
    public void RemedyBomb_KillHint(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Kill the Remedy Bomb", duration: 5000, false);
        if (isTTS) accessory.Method.TTS("Kill the Remedy Bomb");   
        if (isEdgeTTS) accessory.Method.EdgeTTS("Kill the Remedy Bomb");  
        
        ++timesRemedyBomb;
    }
    
    [ScriptMethod(name: "190 Ground Sap Cast Count", userControl: false, eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7169"])]
    public void GroundSapCastCount(Event @event, ScriptAccessory accessory)
    {
        ++Sap;
        if (isDeveloper) accessory.Method.SendChat($"/e Debug Info: Ground Sap cast count: {Sap}");
    }
    
    [ScriptMethod(name: "190 Lava Bomb Spawn Position Prediction", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:7169"])]
    public void LavaBomb_StunShotPrediction_ByGroundSapCount(Event @event, ScriptAccessory accessory)
    {
        if (Sap % 4 == 0 && Sap > 0)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Lava Bomb_Stun Shot Prediction";
            dp.Color = new Vector4(0f, 0f, 1f, 1.5f);
            dp.Scale = new Vector2(7.2f);
            dp.DestoryAt = 5500;
            
            switch (Sap / 4)
            {
                case 1:
                {
                    dp.Position = new Vector3(-288.63f, 0.14f, -300.26f);
                    if (isDeveloper) accessory.Method.SendChat($"/e Debug Info: Ground Sap #{Sap}, drawing prediction location 1");
                    break;
                }
                case 2:
                {
                    dp.Position = new Vector3(-297.46f, 0.12f, -297.52f);
                    if (isDeveloper) accessory.Method.SendChat($"/e Debug Info: Ground Sap #{Sap}, drawing prediction location 2");
                    break;
                }
                case 3:
                {
                    dp.Position = new Vector3(-288.84f, 0.12f, -305.54f);
                    if (isDeveloper) accessory.Method.SendChat($"/e Debug Info: Ground Sap #{Sap}, drawing prediction location 3");
                    break;
                }
                case 4:
                {
                    dp.Position = new Vector3(-309.13f, 0.05f, -303.74f);
                    if (isDeveloper) accessory.Method.SendChat($"/e Debug Info: Ground Sap #{Sap}, drawing prediction location 4");
                    break;
                }
                case 5:
                {
                    dp.Position = new Vector3(-298.36f, 0.05f, -293.63f);
                    if (isDeveloper) accessory.Method.SendChat($"/e Debug Info: Ground Sap #{Sap}, drawing prediction location 5");
                    break;
                }
                case 6:
                {
                    dp.Position = new Vector3(-301.96f, 0.05f, -314.29f);
                    if (isDeveloper) accessory.Method.SendChat($"/e Debug Info: Ground Sap #{Sap}, drawing prediction location 6");
                    break;
                }
                case 7:
                {
                    dp.Position = new Vector3(-299.12f, 0.05f, -297.56f);
                    if (isDeveloper) accessory.Method.SendChat($"/e Debug Info: Ground Sap #{Sap}, drawing prediction location 7");
                    break;
                }
            }
            
            if (isText) accessory.Method.TextInfo("Predicted <Lava Bomb> spawn location", duration: 3500, false);
            if (isTTS) accessory.Method.TTS("Predicted Lava Bomb spawn location");   
            if (isEdgeTTS) accessory.Method.EdgeTTS("Predicted Lava Bomb spawn location");  
            
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "Lava Bomb Stun Shot Prediction Cleanup", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6386"], userControl: false)]
    public void LavaBomb_StunShotPredictionCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Lava Bomb_Stun Shot Prediction");
    }
    
    [ScriptMethod(name: "190 Lava Bomb Stun Shot Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:6386"])]
    public void LavaBomb_StunShot(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Push the Lava Bomb to the boss's feet", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Push the Lava Bomb to the boss's feet");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Push the Lava Bomb to the boss's feet");  
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Lava Bomb_Stun Shot";
        dp.Color = new Vector4(1f, 0f, 0f, 3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.2f);
        dp.DestoryAt = 24700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Lava Bomb Stun Shot Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:7170"], userControl: false)]
    public void LavaBomb_StunShotCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Lava Bomb_Stun Shot");
    }
    
    private Guid _currentExplosionOperationId = Guid.Empty;

    [ScriptMethod(name: "190 Bomb Father_Grand Explosion Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7103"])]
    public async void BombFather_GrandExplosionHint(Event @event, ScriptAccessory accessory)
    { 
        var operationId = Guid.NewGuid();
        _currentExplosionOperationId = operationId;
    
        await Task.Delay(15800);
    
        if (_currentExplosionOperationId != operationId) return;
    
        if (isText) accessory.Method.TextInfo("99.9% true damage, use instant heals", duration: 8500, true);
        if (isTTS) accessory.Method.TTS("99.9% true damage, use instant heals");
        if (isEdgeTTS) accessory.Method.EdgeTTS("99.9% true damage, use instant heals");  
    }

    [ScriptMethod(name: "Grand Explosion Interrupt Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:7103"], userControl: false)]
    public void GrandExplosionInterruptCleanup(Event @event, ScriptAccessory accessory)
    {
        _currentExplosionOperationId = Guid.NewGuid();
    }
    #endregion
    
    #region 191~200 Floors Trash
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” 191 ~ 200 Floors â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor191(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Onyx Dragon_Evil Eye (Look Away)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7043", "SourceDataId:6338"])]
    public void OnyxDragon_EvilEye(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Onyx Dragon_Evil Eye{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Palace Ghost_Level 5 Doom (Cone)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:7084", "SourceDataId:6341"])]
    public void PalaceGhost_Level5Doom(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Palace Ghost_Level 5 Doom{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.1f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    #endregion
    
    #region Cheat Section
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” Cheat Section (Requires corresponding plugin & permissions) â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void CheatSection(Event @event, ScriptAccessory accessory) { }

    [ScriptMethod(name: "[DR] When Transformed into Manticore, Set Speed to 1.5x", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:565", "StackCount:42", "Duration:60.00"])]
    public void AddManticoreSpeed(Event @event, ScriptAccessory accessory)
    {
        if (!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1.5");
        accessory.Method.SendChat($"/e Quack: [DR] Speed changed: 1.5x");
        if (isTTS) accessory.Method.TTS("Speed changed to 1.5x");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Speed changed to 1.5x");
    }
    
    [ScriptMethod(name: "[DR] When Manticore Ends, Reset Speed to Default", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:565", "StackCount:42", "Duration:0.00"])]
    public void RemoveManticoreSpeed(Event @event, ScriptAccessory accessory)
    {
        if (!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed -1");
        accessory.Method.SendChat($"/e Quack: [DR] Speed changed: Default");
        if (isTTS) accessory.Method.TTS("Speed reset to default");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Speed reset to default");
    }
    
    [ScriptMethod(name: "[DR] When Transformed into Succubus, Set Speed to 1.2x", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:565", "StackCount:43", "Duration:60.00"])]
    public void AddSuccubusSpeed(Event @event, ScriptAccessory accessory)
    {
        if (!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1.2");
        accessory.Method.SendChat($"/e Quack: [DR] Speed changed: 1.2x");
        if (isTTS) accessory.Method.TTS("Speed changed to 1.2x");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Speed changed to 1.2x");
    }
    
    [ScriptMethod(name: "[DR] When Succubus Ends, Reset Speed to Default", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:565", "StackCount:43", "Duration:0.00"])]
    public void RemoveSuccubusSpeed(Event @event, ScriptAccessory accessory)
    {
        if (!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed -1");
        accessory.Method.SendChat($"/e Quack: [DR] Speed changed: Default");
        if (isTTS) accessory.Method.TTS("Speed reset to default");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Speed reset to default");
    }
    
    [ScriptMethod(name: "[IC] When Transformed into Manticore, Cancel Y-adjust", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:565", "StackCount:42", "Duration:60.00"])]
    public void AddManticoreDepths(Event @event, ScriptAccessory accessory)
    {
        if (!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/i-ching-commander y_adjust 0");
        accessory.Method.SendChat($"/e Quack: [IC] Y-adjust cancelled");
        if (isText) accessory.Method.TextInfo("Y-adjust cancelled", duration: 1300, true);
    }
    
    [ScriptMethod(name: "[IC] When Manticore Ends, Auto Y-adjust", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:565", "StackCount:42", "Duration:0.00"])]
    public void RemoveManticoreDepths(Event @event, ScriptAccessory accessory)
    {
        if (!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
    
        string depthValue = Depths.GetDescription();
        
        accessory.Method.SendChat($"/i-ching-commander y_adjust -{depthValue}");
        accessory.Method.SendChat($"/e Quack: [IC] Auto Y-adjust -{depthValue}m");
        if (isText) accessory.Method.TextInfo($"Auto Y-adjust -{depthValue}m", duration: 1300, true);
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

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;
        return attribute?.Description ?? value.ToString();
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
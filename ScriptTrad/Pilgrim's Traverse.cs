// File: Pilgrims_Traverse_Tetora.cs
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
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Pilgrims_Traverse;

[ScriptType(guid: "3619b5a7-d4ed-4675-a60e-c873ebbacbde", name: "Pilgrim's Traverse", Author: "Linoa235", 
    territorys: [1281, 1282, 1283, 1284, 1285, 1286, 1287, 1288, 1289, 1290, 1311, 1333],
    version: "0.0.1.91",note: noteStr)]

public class Pilgrims_Traverse
{
    const string noteStr =
        """
        v0.0.1.91:
        Pilgrim's Traverse Basic Drawing
        Changelog on Discord, please provide ARR recording if issues occur
        Note: Layer numbers in method settings are foråˆ†å‰²çº¿ effects only, not batch switches
        """;

    
    #region Basic Controls
    
    [UserSetting("TTS Switch (Please enable only one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Switch (Please enable only one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Text Popup Switch")]
    public bool isText { get; set; } = true;
    
    [UserSetting("Enable Underground Special Drawing")]
    public bool isUnderGround { get; set; } = true;
    
    [UserSetting("Color of AOEs that can be hit while underground")]
    public ScriptColor UnderGround_AOEs { get; set; } = new() { V4 = new(1f, 0f, 0f, 1.2f) };
    
    [UserSetting("Color for Suction-type Skills")]
    public ScriptColor InhaleColor { get; set; } = new() { V4 = new(0f, 1f, 1f, 0.4f) };
    
    [UserSetting("Enable Mini Tools (Already confirmed settings)")]
    public bool isMiniTools { get; set; } = false;
    
    [UserSetting("Set Teleportation Construct Search Color")]
    public ScriptColor teleporter { get; set; } = new() { V4 = new(0f, 1f, 0f, 2f) };
    
    [UserSetting("Set Votive Candelabra Search Color")]
    public ScriptColor votiveCandelabra { get; set; } = new() { V4 = new(1f, 0f, 1f, 1.4f) };
    
    [UserSetting(note: "Select Auto Rekindle Target")]
    public RekindleEnum Rekindle { get; set; } = RekindleEnum.TargetsTarget;
    
    [UserSetting("Potion Alert Master Switch for Deep Thinking Battles")]
    public bool isPotions { get; set; } = true;
    
    [UserSetting("Auto Anti-Knockback for Flaming Domain (Except Tanks, Bottom Layer requires bottom option enabled)")]
    public AutoAntiKnockbackEnum AutoAntiKnockback { get; set; } = AutoAntiKnockbackEnum.None;
    
    private static List<string> _AutoAntiKnockback = ["Arm's Length", "Surecast"];
    
    public enum AutoAntiKnockbackEnum
    {
        None = -1,
        ArmsLength = 0,
        Surecast = 1,
        DR = 2,
        IChing = 3,
    }
    
    public enum RekindleEnum
    {
        [Description("<tt>")]
        TargetsTarget = 0,
        [Description("<2>")]
        PartyList2 = 1,
        [Description("<me>")]
        Me = 2,
    }
    
    [UserSetting("Enable Bottom Layer (Requires corresponding plugin and permissions)")]
    public bool isHack { get; set; } = false;
    
    [UserSetting(note: "Select Default Underground Depth")]
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
    
    public static class MapIds
    {
        public const uint PilgrimsTraverse0 = 1281; // Floors 1~10 Pilgrim's Trail
        public const uint PilgrimsTraverse1 = 1282; // Floors 11~20 Pilgrim's Trail
        public const uint PilgrimsTraverse2 = 1283;
        public const uint PilgrimsTraverse3 = 1284;
        public const uint PilgrimsTraverse4 = 1285;
        public const uint PilgrimsTraverse5 = 1286;
        public const uint PilgrimsTraverse6 = 1287;
        public const uint PilgrimsTraverse7 = 1288;
        public const uint PilgrimsTraverse8 = 1289;
        public const uint PilgrimsTraverse9 = 1290;
        public const uint TheFinalVerse = 1333; // The Final Verse Extermination Battle
        public const uint TheFinalVerseQuantum = 1311; // The Final Verse Quantum Deep Thinking Battle
    }
    
    private ScriptAccessory _sa = null;

    public void Init(ScriptAccessory accessory) {
        PerilousLair = 0; // 80 Boss Painful Circle Circle
        RoaringRing = 0; // 80 Boss Purple Thunder Ring Donut
        _blackandwhite = 0; // The Final Verse Quantum Battle Black and White Mark Record
        _spinelash = 0; // The Final Verse Quantum Battle Spinelash Line Mark Count Record
        _sa = accessory;
        accessory.Method.ClearFrameworkUpdateAction(this);
        ResetMechanic(); // The Final Verse Floor Fire Crystal Reset
    }
    
    [ScriptMethod(name: "Head Smash & Interject Interrupt Destruction", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^75(38|51)$"], userControl: false)]
    public void InterruptDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "Special Status Destruction", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(2|3|1511|1113)$"], userControl: false)]
    public void SpecialStatusDestruction(Event @event, ScriptAccessory accessory)
    {
        // When the corresponding monster gains Stun[2], Sleep[3], Petrification[1511], (Morph) Unable to Act[1113] states, drawing needs to be destroyed
        accessory.Method.RemoveDraw($".*{@event.TargetId()}");
    }
        
    [ScriptMethod(name: "Cast Interruption Destruction", eventType: EventTypeEnum.CancelAction, eventCondition: [], userControl: false)]
    public void CastInterruptionDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Death Destruction", eventType: EventTypeEnum.Death, eventCondition: [], userControl: false)]
    public void DeathDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($".*{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "Anti-Knockback Destruction", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"],userControl: false)]
    public void AntiKnockbackDestruction(Event @event, ScriptAccessory accessory)
    {
        if ( @event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw(".*(Knockback|Suction).*");
    }
    
    public bool KnockPenalty = false;
    
    [ScriptMethod(name: "Weather: Knockback Immunity Added", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1096"],userControl: false)]
    public void AddKnockbackBuff(Event @event, ScriptAccessory accessory)
    {
        KnockPenalty = true;
    }
    
    [ScriptMethod(name: "Weather: Knockback Immunity Removed", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:1096"],userControl: false)]
    public void RemoveKnockbackBuff(Event @event, ScriptAccessory accessory)
    {
        KnockPenalty = false;
    }
    
    #endregion
    
    // General Content
    [ScriptMethod(name: "Mimic_Malice Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44852"])]
    public void MimicMalice(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Interrupt Mimic", duration: 2000, true);
        if (isTTS) accessory.Method.TTS("Interrupt Mimic");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Interrupt Mimic");
    }
    
    #region Tools Section
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” Tools Section (Please disable unnecessary functions first) â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void ToolsSection(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Self Bomb-Mother Big Burst Range Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44629"])]
    public void BombMotherBigBurstSelf (Event @event, ScriptAccessory accessory)
    {
        if(!isMiniTools) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"BombMother_BigBurst{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 1500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Teleportation Construct Finder", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2014756", "Operate:Add"])]
    public void TeleportationConstructFinder(Event @event, ScriptAccessory accessory)
    {
        if(!isMiniTools) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"TeleportationConstructFinder";
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new (1f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Color = teleporter.V4;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp); 
    }
    
    [ScriptMethod(name: "Votive Candelabra Finder", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2014759", "Operate:Add"])]
    public void VotiveCandelabraFinder(Event @event, ScriptAccessory accessory)
    {
        if(!isMiniTools) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"VotiveCandelabra";
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new (1f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Color = votiveCandelabra.V4;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp); 
    }

    [ScriptMethod(name: "Votive Candelabra Destruction", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:16", "Id2:32"], userControl: false)]
    public void VotiveCandelabraDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"VotiveCandelabra");
    }
    
    [ScriptMethod(name: "Auto Cancel Two-Stage Crimson Strike (Prevents auto-loop from getting stuck when underground can't hit)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4403"])]
    public void AutoRemoveCrimsonStrike(Event @event, ScriptAccessory accessory)
    {
        if(!isMiniTools || @event.TargetId() != accessory.Data.Me) return;
        accessory.Method.SendChat($"/statusoff Crimson Strike Ready");
        if (isDeveloper) accessory.Method.SendChat($"/e Duck: Canceled \"Crimson Strike Ready\"");
    }
    
    [ScriptMethod(name: "Auto Attempt to Use Rekindle (Very team-oriented even when underground)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1868"])]
    public void AutoUseRekindle(Event @event, ScriptAccessory accessory)
    {
        string rekindleValue = Rekindle.GetDescription();
        
        if(!isMiniTools ||  @event.SourceId() != accessory.Data.Me || @event.TargetId() != accessory.Data.Me) return;
        accessory.Method.SendChat($"/ac Starry Hyperflow {rekindleValue}");
        accessory.Method.SendChat($"/e Duck: Attempted to apply Rekindle to \"{rekindleValue}\"");
    }
    
    #endregion
    
    #region  Floors 1~10 
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” Floors 1 ~ 10 â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor1(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "1~2 Pilgrim's Echeveria_Leafcutter (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44631"])]
    public void PilgrimsEcheveria_Leafcutter(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsEcheveria_Leafcutter{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 15f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "1~3 Pilgrim's Mandragora_Rustling Wind (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44641"])]
    public void PilgrimsMandragora_RustlingWind(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsMandragora_RustlingWind{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 15f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "6~7 Pilgrim's Hornet_Unfinal Sting (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44637"])]
    public void PilgrimsHornet_UnfinalSting(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsHornet_UnfinalSting{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (3f, 9f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "6~9 Pilgrim's Clematis_Spinning Attack (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44638"])]
    public void PilgrimsClematis_SpinningAttack(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsClematis_SpinningAttack{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 10f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
        
    [ScriptMethod(name: "10 Mandragora_Hedge Mazing (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44855"])]
    public void Mandragora_HedgeMazing (Event @event, ScriptAccessory accessory)
    {
        // 44054 is meaningless cast (should be yellow circle generation process), damage source is 44855, both have different cast times
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Mandragora_HedgeMazing";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(14f);
        dp.DestoryAt = 13200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"Mandragora_HedgeMazingOutline";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(14f);
        dp1.InnerScale = new Vector2(13.9f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 13200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "10 Mandragora_Leafmash (Jump Circle)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:44058"])]
    public void Mandragora_Leafmash (Event @event, ScriptAccessory accessory)
    {
        // Mandragora cast [ActionId: 44055 ; Cast 9.7s], four consecutive jump marks are [ActionId: 44058] deals damage about 8.9s after release, each interval about 1.8s, damage source [ActionId: 44057 ; Cast 1.6s]
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Mandragora_Leafmash";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(15f);
        dp.Delay = 2700;
        dp.DestoryAt = 6200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"Mandragora_LeafmashOutline";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp1.Position = @event.EffectPosition();
        dp1.Scale = new Vector2(15f);
        dp1.InnerScale = new Vector2(14.94f);
        dp1.Radian = float.Pi * 2;
        dp1.Delay = 2700;
        dp1.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
        
    }
    
    #endregion
    
    #region  Floors 11~20 
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” Floors 11 ~ 20 â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor11(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "11~12 Pilgrim's Puck_Ovation (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44657"])]
    public void PilgrimsPuck_Ovation(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsPuck_Ovation{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 14f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "11~12 Forgiven Ignorance_Silkscreen (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44646"])]
    public void ForgivenIgnorance_Silkscreen(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenIgnorance_Silkscreen{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 16f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "16~17 Forgiven Disobedience_Head Butt (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44651"])]
    public void ForgivenDisobedience_HeadButt(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenDisobedience_HeadButt{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (3f, 10f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "20 Forgiven Emulation_Burst (Four Consecutive Circles)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4345[6-9]$"])]
    public void ForgivenEmulation_Burst (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenEmulation_Burst";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(11f);
        
        switch (@event.ActionId())
        {
            case 43456:
                dp.Delay = 0;
                dp.DestoryAt = 1700;
                break;
            case 43457:
                dp.Delay = 1700;
                dp.DestoryAt = 1300;
                break;
            case 43458:
                dp.Delay = 3000;
                dp.DestoryAt = 1300;
                break;
            case 43459:
                dp.Delay = 4300;
                dp.DestoryAt = 1300;
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "20 Tree Root_Wood's Embrace (Ice Flower)", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2014795", "Operate:Add"])]
    public void TreeRoot_WoodsEmbrace(Event @event, ScriptAccessory accessory)
    {
        // Placed head mark TargetIconId: 0017 , Embrace ActionId: 43462 
        float[] degrees = { 0f, 45f, 90f, 135f };
    
        foreach (float degree in degrees)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"TreeRoot_WoodsEmbrace_{degree}Â°";
            dp.Scale = new (6f, 60f);
            dp.Owner = @event.SourceId();
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
            dp.DestoryAt = 19200;
            dp.Rotation = degree * (float)Math.PI / 180f;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    
    #endregion
    
    #region Floors 21~30
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” Floors 21 ~ 30 â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor21(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "21~23 Forgiven Bribe_Aetherial Spark (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44680"])]
    public void ForgivenBribe_AetherialSpark(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenBribe_AetherialSpark{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 12f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "24~25 Pilgrim's Sea Angel_Parasitism TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44681"])]
    public void PilgrimsSeaAngel_Parasitism(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("Stun Sea Angel Tankbuster");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Stun Sea Angel Tankbuster");
    }
    
    [ScriptMethod(name: "26~29 Forgiven Cruelty_Lumen Infinitum (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44668"])]
    public void ForgivenCruelty_LumenInfinitum(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenCruelty_LumenInfinitum{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (5f, 40f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "26~27 Forgiven Avarice_Ripper Claw (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44669"])]
    public void ForgivenAvarice_RipperClaw(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenAvarice_RipperClaw{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "28~29 Forgiven Narrow-mindedness_Words of Woe (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44683"])]
    public void ForgivenNarrowMindedness_WordsOfWoe(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenNarrowMindedness_WordsOfWoe{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (6f, 47f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "28~29 Forgiven Narrow-mindedness_Swinge (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44684"])]
    public void ForgivenNarrowMindedness_Swinge (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenNarrowMindedness_Swinge{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "28~29 Pilgrim's Golem_Stonelight (Wall-Piercing Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44675"])]
    public void PilgrimsGolem_Stonelight(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsGolem_Stonelight{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (6f, 60f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "30 Forgiven Betrayal_Brutal Halo (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^39(642|643|674|743)$"])]
    public void ForgivenBetrayal_BrutalHalo (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenBetrayal_BrutalHalo";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.2f);
        dp.Owner = @event.SourceId();
        
        switch (@event.ActionId())
        {
            case 39642:
                dp.Scale = new Vector2(14f);
                dp.InnerScale = new Vector2(9f);
                break;
            case 39643:
                dp.Scale = new Vector2(19f);
                dp.InnerScale = new Vector2(14f);
                break;
            case 39674:
                dp.Scale = new Vector2(24f);
                dp.InnerScale = new Vector2(19f);
                break;
            case 39743:
                dp.Scale = new Vector2(29f);
                dp.InnerScale = new Vector2(24f);
                break;
        }
        
        dp.Radian = float.Pi * 2;
        dp.Delay = 7000;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "30 Forgiven Betrayal_Grip of Salvation (Left/Right Slices)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40401|40549)$"])]
    public void ForgivenBetrayal_GripOfSalvation(Event @event, ScriptAccessory accessory)
    {
        // Right slice meaningless 40401 damage source 40551 then left slice Salvation Arm 40552 ; Left slice 40549 then right slice Salvation Arm 40553
        var isR = @event.ActionId == 40401;

        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        
        dp.Name = $"ForgivenBetrayal_GripOfSalvation";
        dp.Color = new Vector4(1f, 0f, 0f, 0.75f);
        dp1.Owner = dp.Owner = @event.SourceId();
        dp.Scale = new (30f, 60f);
        dp.Offset = new Vector3(isR ? 15f : -15f, 0f, 0f);
        dp1.Delay = dp.DestoryAt = 7400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp); 
        
        dp1.Name = $"ForgivenBetrayal_GripOfSalvation";
        dp1.Color = new Vector4(1f, 0f, 0f, 1.5f);
        dp1.Scale = new (30f);
        dp1.Radian = 220f.DegToRad();
        dp1.Rotation = isR ? 70f.DegToRad() : 290f.DegToRad(); 
        dp1.DestoryAt = 7800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1); 
    }
    
    [ScriptMethod(name: "30 Bounds of Indulgence (Rotating Circle)", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7750", "SourceDataId:17930"])]
    public void ForgivenBetrayal_BoundsOfIndulgence (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenBetrayal_BoundsOfIndulgence";
        dp.Color = new Vector4(0f, 0f, 1f, 3f);
        dp1.Owner = dp.Owner = @event.SourceId();
        dp1.Scale = dp.Scale = new Vector2(4f);
        dp1.DestoryAt = dp.DestoryAt = 60000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        dp1.Name = "ForgivenBetrayal_BoundsOfIndulgenceOutline";
        dp1.Color = new Vector4(0f, 0f, 1f, 10f);
        dp1.InnerScale = new Vector2(3.94f);
        dp1.Radian = float.Pi * 2;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }

    [ScriptMethod(name: "Bounds of Indulgence Destruction", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7740", "SourceDataId:17930"] ,userControl:false)]
    public void BoundsOfIndulgenceDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("ForgivenBetrayal_BoundsOfIndulgence.*");
    }

    
    #endregion
    
    #region  Floors 31~40
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” Floors 31 ~ 40 â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor31(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "31~33 Pilgrim's Peagasus_Nicker (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44703"])]
    public void PilgrimsPeagasus_Nicker (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsPeagasus_Nicker{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "32~35 Pilgrim's Vouivre_Alpine Draft (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44700"])]
    public void PilgrimsVouivre_AlpineDraft(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsVouivre_AlpineDraft{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (5f, 45f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "36~38 Pilgrim's Judge_Death's Door (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44694"])]
    public void PilgrimsJudge_DeathsDoor(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsJudge_DeathsDoor{@event.SourceId()}";
        dp.Scale = new (2f, 21f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "36~39 Forgiven Pestilence_Poison Pollen Pair (Two-Stage Tail Sweep)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41344"])]
    public void ForgivenPestilence_PoisonPollenPair(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenPestilence_PoisonPollenPair{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Radian = 120f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.Delay = 2700;
        dp.DestoryAt = 5600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "36~39 Pilgrim's Golem_Line of Fire (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40891"])]
    public void PilgrimsGolem_LineOfFire(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsGolem_LineOfFire{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (8f, 60f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "36~39 Pilgrim's Golem_Buster Knuckles (Two-Stage Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40558"])]
    public void PilgrimsGolem_BusterKnuckles (Event @event, ScriptAccessory accessory)
    {
        // After Heavy Punch [ActionId:40558 / 3.7s] 4s later shows second stage Circle two judgments interval about 2s
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsGolem_BusterKnuckles{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(15f);
        dp.Delay = 4000;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "37~39 Forgiven Irascibility_Left & Right Tentacle (Left/Right Slices)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4469[01]$"])]
    public void ForgivenIrascibility_LeftRightTentacle(Event @event, ScriptAccessory accessory)
    {
        // Right slice 44691 ; Left slice 44690
        var isR = @event.ActionId == 44691;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenIrascibility_LeftRightTentacle{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "40 Forgiven Innocence_Blown Blessing TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4212[34]$"])]
    public void ForgivenInnocence_BlownBlessing (Event @event, ScriptAccessory accessory)
    {
        if (@event.ActionId == 42123)
        {
            if (isText)accessory.Method.TextInfo("Triple Knockback", duration: 2000, true);
            if (isTTS)accessory.Method.TTS("Triple Knockback");
            if (isEdgeTTS)accessory.Method.EdgeTTS("Triple Knockback");
        }
        else
        {
            if (isText)accessory.Method.TextInfo("Triple Circle", duration: 2000, true);
            if (isTTS)accessory.Method.TTS("Triple Circle");
            if (isEdgeTTS)accessory.Method.EdgeTTS("Triple Circle");
        }
    }
    
    [ScriptMethod(name: "40 Forgiven Innocence_Shining Shot (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42129"])]
    public void ForgivenInnocence_ShiningShot (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenInnocence_ShiningShot";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.Delay = @event.SourceDataId() == 9020 ? 0 : 4800;
        dp.DestoryAt = @event.SourceDataId() == 9020 ? 9700 : 4900;
        dp.ScaleMode = @event.SourceDataId() == 9020 ? ScaleMode.ByTime : ScaleMode.None;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "40 Forgiven Innocence_Saltwater Shot (Knockback)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42128"])]
    public void ForgivenInnocence_SaltwaterShotKnockback(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "ForgivenInnocence_SaltwaterShotKnockback";
        dp.Scale = new Vector2(1f, 21f);
        dp.Color = new Vector4(0f, 1f, 1f, 3f);
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = @event.TargetPosition();
        dp.Rotation = float.Pi;
        dp.Delay = @event.SourceDataId() == 9020 ? 0 : 7500;
        dp.DestoryAt = @event.SourceDataId() == 9020 ? 9700 : 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "40 Forgiven Innocence_Saltwater Shot (First Knockback Position)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42128", "SourceDataId:18467"], suppress:9700)]
    public void ForgivenInnocence_SaltwaterShotKnockbackPosition(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "ForgivenInnocence_SaltwaterShotKnockbackPosition";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.InnerScale = new Vector2(4.92f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 9700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "40 Forgiven Innocence_Near Tide (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45169"])]
    public void ForgivenInnocence_NearTide (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenInnocence_NearTide";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "40 Forgiven Innocence_Far Tide (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45170"])]
    public void ForgivenInnocence_FarTide (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenInnocence_FarTide";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.InnerScale = new Vector2(8f);
        dp1.Radian = dp.Radian = float.Pi * 2;
        dp1.DestoryAt = dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
        dp1.Name = $"ForgivenInnocence_FarTideSafeZone";
        dp1.Color = accessory.Data.DefaultSafeColor.WithW(10f);
        dp1.Scale = new Vector2(7.9f);
        dp1.InnerScale = new Vector2(7.75f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    #endregion
    
    #region  Floors 41~50
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” Floors 41 ~ 50 â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor41(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "42~45 Pilgrim's Arsenal_Smite of Rage (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44714"])]
    public void PilgrimsArsenal_SmiteOfRage(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsArsenal_SmiteOfRage{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (4f, 6f);
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "42~45 Pilgrim's Arsenal_Whirl of Rage (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44715"])]
    public void PilgrimsArsenal_WhirlOfRage (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsArsenal_WhirlOfRage{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "43~46 Pilgrim's Tortoise_Tortoise Stomp (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41724"])]
    public void PilgrimsTortoise_TortoiseStomp (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsTortoise_TortoiseStomp{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 11700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "46~49 Pilgrim's Statue_Magnetic Shock (Suction)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41427"])]
    public void PilgrimsStatue_MagneticShock (Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("Suction");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Suction");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsStatue_MagneticShock{@event.SourceId()}";
        dp.Color = InhaleColor.V4;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"PilgrimsStatue_MagneticShock{@event.SourceId()}";
        dp1.Color = InhaleColor.V4.WithW(5f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(15f);
        dp1.InnerScale = new Vector2(14.94f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "46~49 Pilgrim's Statue_Plaincracker (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41512"])]
    public void PilgrimsStatue_Plaincracker (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsStatue_Plaincracker{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "46~49 Pilgrim's Cactus_Creeping Combination (Tail Sweep)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41854"])]
    public void PilgrimsCactus_CreepingCombination(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsCactus_CreepingCombination{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Radian = 90f.DegToRad(); 
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 6000; // Cast 2.7s, about 3s later turns and hits front again
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "47~49 Pilgrim's Antlion_One-two March (Round-trip Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4470[89]$"])]
    public void PilgrimsAntlion_OneTwoMarch(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsAntlion_OneTwoMarch{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (8f, 15f);
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = @event.ActionId() == 44708 ? 2700: 700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        if ( @event.ActionId() == 44708)
        {
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"PilgrimsAntlion_OneTwoMarchReturnPrediction{@event.SourceId()}";
            dp1.Owner = @event.SourceId();
            dp1.Scale = new (8f, 15f);
            if (isUnderGround) {dp1.Color = UnderGround_AOEs.V4; }
            else {dp1.Color = accessory.Data.DefaultDangerColor; }
            dp1.Rotation = 180f.DegToRad();
            dp1.Delay = 2700;
            dp1.DestoryAt = 2500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1); 
        }
    }
    
    // 50 Boss Augmented Ogre
    
    [ScriptMethod(name: "50 Augmented Ogre_Liquefaction Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43531"])]
    public void AugmentedOgre_Liquefaction(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("Stand on the rock", duration: 2000, true);
        if (isTTS)accessory.Method.TTS("Stand on the rock");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Stand on the rock");
    }
    
    [ScriptMethod(name: "50 Augmented Ogre_Sandpit Target Alert", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0280"])]
    public void AugmentedOgre_SandpitTarget(Event @event, ScriptAccessory accessory)
    {
        // Enter Sandpit ActionId: 43533
        if (HelperExtensions.GetCurrentTerritoryId() != MapIds.PilgrimsTraverse4) return;
        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isText)accessory.Method.TextInfo("Tracking AOE target (avoid teammates)", duration: 13800, true);
            if (isTTS)accessory.Method.TTS("Tracking AOE target");
            if (isEdgeTTS)accessory.Method.EdgeTTS("Tracking AOE target");
        }
        else
        {
            if (isText)accessory.Method.TextInfo("Avoid tracking AOE (run along, don't block)", duration: 13800, true);
            if (isTTS)accessory.Method.TTS("Avoid tracking AOE");
            if (isEdgeTTS)accessory.Method.EdgeTTS("Avoid tracking AOE");
        }
    }
    
    [ScriptMethod(name: "50 Augmented Ogre_Pit Ambush First (First Tracking)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43534"])]
    public void AugmentedOgre_PitAmbush (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"AugmentedOgre_PitAmbush";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "50 Augmented Ogre_Windraiser Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43537"])]
    public void AugmentedOgre_Windraiser(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("Enter quicksand later to avoid knockback\n(Knockback immunity ineffective)", duration: 3000, true);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Windraiser";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = new Vector3(-300f, 0f, -300f);
        dp.Scale = new Vector2(20f);
        dp.InnerScale = new Vector2(15f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "50 Augmented Ogre_Biting Wind Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43538"])]
    public async void AugmentedOgre_BitingWind(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("Knockback countdown", duration: 7000, false);
        await Task.Delay(4700);
        if (isTTS)accessory.Method.TTS("Enter quicksand");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Enter quicksand");
    }
    
    #endregion
    
    #region  Floors 51~59
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” Floors 51 ~ 60 â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor51(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "53~56 Pilgrim's Basilisk_Smoldering Scales Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42212"])]
    public void PilgrimsBasilisk_SmolderingScales(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("Basilisk counter damage");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Basilisk counter damage");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsBasilisk_SmolderingScales{@event.SourceId()}";
        dp.Color = UnderGround_AOEs.V4.WithW(10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3.2f);
        dp.InnerScale = new Vector2(3f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "53~56 Pilgrim's Basilisk_Blaze Spikes Counter Tip", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4579"])]
    public void PilgrimsBasilisk_BlazeSpikes(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsBasilisk_BlazeSpikes{@event.SourceId()}";
        dp.Color = UnderGround_AOEs.V4.WithW(10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3.2f);
        dp.InnerScale = new Vector2(3f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "PilgrimsBasilisk_BlazeSpikesDestruction", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4579"],userControl: false)]
    public void PilgrimsBasilisk_BlazeSpikesDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"PilgrimsBasilisk_BlazeSpikes{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "56~59 Pilgrim's Sand Boa_Earthen Auger (270Â° Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42091"])]
    public void PilgrimsSandBoa_EarthenAuger (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsSandBoa_EarthenAuger{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 270f.DegToRad(); 
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "56~59 Pilgrim's Giant_Heavy Scrapline (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44737"])]
    public void PilgrimsGiant_HeavyScrapline (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsGiant_HeavyScrapline{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "57~59 Pilgrim's Howl_Barreling Smash (Line Charge)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44730"])]
    public void PilgrimsHowl_BarrelingSmash(Event @event, ScriptAccessory accessory)
    {
        if (!isUnderGround) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsHowl_BarrelingSmash{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.Scale = new (7f, 12f);
        dp.Color = UnderGround_AOEs.V4;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "57~59 Pilgrim's Howl_Scythe Tail (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44731"])]
    public void PilgrimsHowl_ScytheTail (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsHowl_ScytheTail{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "57~59 Pilgrim's Howl_Master of Levin (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44732"])]
    public void PilgrimsHowl_MasterOfLevin(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsHowl_MasterOfLevin{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "57~59 Pilgrim's Manasneeze_Bafflement Bulb (Confusion)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42144"])]
    public void PilgrimsManasneeze_BafflementBulb (Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("Interrupt Manasneeze visual cue (no resistance)", duration: 4300, true);
        if (isTTS)accessory.Method.TTS("Interrupt or stun Manasneeze");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Interrupt or stun Manasneeze");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsManasneeze_BafflementBulb{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"PilgrimsManasneeze_BafflementBulbOutline{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(8f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(40f);
        dp1.InnerScale = new Vector2(39.94f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "57~59 Pilgrim's Manasneeze_Trounce (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42147"])]
    public void PilgrimsManasneeze_Trounce(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsManasneeze_Trounce{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "57~59 Pilgrim's Manasneeze_Mighty Spin (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42148"])]
    public void PilgrimsManasneeze_MightySpin (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsManasneeze_MightySpin{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(14f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region 60 Boss ArcheoMalik
    
    [ScriptMethod(name: "60 ArcheoMalik_Spineshot (Front/Back Cone)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44866"])]
    public void ArcheoMalik_Spineshot (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ArcheoMalik_Spineshot";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = @event.DurationMilliseconds();
    
        float[] rotations = { 0f, 180f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
    }
    
    [ScriptMethod(name: "60 ArcheoMalik_Spinning Needles (Rotating Cone)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44868"])]
    public void ArcheoMalik_SpinningNeedles (Event @event, ScriptAccessory accessory)
    {
        // Spinning Needles [ActionId: 44868 - Cast: 4.7s] ; total 10 judgments, each rotation 60Â°, interval about 1s, damage source ActionId: 44909 ;
        // Clockwise rotation [TargetIcon: 00A7] ; Counterclockwise rotation [TargetIcon: 00A7] 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ArcheoMalik_SpinningNeedles";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = 15200; 
    
        float[] rotations = { 0f, 180f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
    }
    
    [ScriptMethod(name: "60 ArcheoMalik_Branch Out (Cactus Square)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4485[89]$"])]
    public void ArcheoMalik_BranchOut(Event @event, ScriptAccessory accessory)
    {
        // Main cast: 44857
        var isBig = @event.ActionId == 44859;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ArcheoMalik_BranchOut";
        dp.Scale = isBig? new (30f, 30f): new (10f, 10f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
    
    #endregion
    
    #region  Floors 61~70
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” Floors 61 ~ 70 â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor61(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "61~64 Forgiven Doubt_Concealed", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:18832"])]
    public void ForgivenDoubt_Concealed (Event @event, ScriptAccessory accessory)
    {
        var obj = IbcHelper.GetById(accessory, @event.SourceId);
        if (obj == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenDoubt_Concealed{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(IbcHelper.GetHitboxRadius(obj)); // Target circle is 2.4m, only visible within +5m, state disappears after aggro
        dp.DestoryAt = 600000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "61~64 Forgiven Doubt_Concealed Facing", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:18832"])]
    public void ForgivenDoubt_ConcealedFacing (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenDoubt_ConcealedFacing{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f); // Reference visual provoke range
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 600000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "ForgivenDoubt_ConcealedDestruction", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:676"],userControl:false)]
    public void ForgivenDoubt_ConcealedDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"ForgivenDoubt_Concealed.*{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "ForgivenDoubt_ConcealedBackupDestruction", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44749|45130)"],userControl:false)]
    public void ForgivenDoubt_ConcealedBackupDestruction(Event @event, ScriptAccessory accessory)
    {
        // 44749 Leap ; 45130 Auto-attack
        accessory.Method.RemoveDraw($"ForgivenDoubt_Concealed.*{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "61~69 Pilgrim's Colossus_Accelerate (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42516"])]
    public void PilgrimsColossus_Accelerate (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsColossus_Accelerate{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "61~69 Pilgrim's Colossus_Subduction (Two-Stage Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42516"])]
    public void PilgrimsColossus_Subduction (Event @event, ScriptAccessory accessory)
    {
        // After Accelerate [ActionId:42516 / 3.7s] 4s later shows second stage Donut two judgments interval about 3.1s
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsColossus_Subduction{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(11f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.Delay = 3700;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "61~64 Forgiven Doubt_Body Press (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44748"])]
    public void ForgivenDoubt_BodyPress (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenDoubt_BodyPress{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "61~63 Pilgrim's Dwarf_Plain Pound (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44753"])]
    public void PilgrimsDwarf_PlainPound (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsDwarf_PlainPound{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "61~63 Pilgrim's Cliffsides_Head Butt (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44740"])]
    public void PilgrimsCliffsides_HeadButt(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsCliffsides_HeadButt{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "62~65 Forgiven Riot_Shockwave (Two-Stage Left/Right Slices)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4221[46]$"])]
    public void ForgivenRiot_Shockwave(Event @event, ScriptAccessory accessory)
    {
        // First right slice 42214 ; First left slice 42216
        var isR = @event.ActionId == 42214;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenRiot_Shockwave1{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"ForgivenRiot_Shockwave2{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(30f);
        dp1.Radian = 180f.DegToRad(); 
        dp1.Rotation = isR ? 90f.DegToRad() : 270f.DegToRad();
        dp1.Delay = 4700;
        dp1.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
    }
    
    [ScriptMethod(name: "63~66 Forgiven Resentment_Hailfire (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42515"])]
    public void ForgivenResentment_Hailfire(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenResentment_Hailfire{@event.SourceId()}";
        dp.Scale = new (4f, 45f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "64~66 Forgiven Injustice_Rockslide (Cross)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44743"])]
    public void ForgivenInjustice_Rockslide(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenInjustice_Rockslide{@event.SourceId()}";
        dp.Scale = new (10f, 80f); // Range to be confirmed, may be 10,40
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3700;
        
        float[] rotations = { 0f, 90f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    
    [ScriptMethod(name: "65~67 Forgiven Attachment_Sewer Water (Front/Back Slice)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4475[01]$"])]
    public void ForgivenAttachment_SewerWater(Event @event, ScriptAccessory accessory)
    {
        // Front slice 44750 ; Back slice 44751
        var isF = @event.ActionId == 44750;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenAttachment_SewerWater";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isF ? 0f.DegToRad() : 180f.DegToRad();
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "65~68 Pilgrim's Queen_Unfinal Sting (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42510"])]
    public void PilgrimsQueen_UnfinalSting(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsQueen_UnfinalSting{@event.SourceId()}";
        dp.Scale = new (3f, 8f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "66~69 Forgiven Opinion_Several Thousand Needles (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42220"])]
    public void ForgivenOpinion_SeveralThousandNeedles(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenOpinion_SeveralThousandNeedles{@event.SourceId()}";
        dp.Scale = new (8f, 20f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "67~69 Pilgrim's Ngozi_Landslip (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44755"])]
    public void PilgrimsNgozi_Landslip(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsNgozi_Landslip{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(13f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "67~69 Forgiven Gluttony_Stone Gaze (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44746"])]
    public void ForgivenGluttony_StoneGaze (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenGluttony_StoneGaze{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "67~69 Forgiven Gluttony_Body Slam (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44747"])]
    public void ForgivenGluttony_BodySlam (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenGluttony_BodySlam{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    // 70 Boss Forgiven Zeal
    
    [ScriptMethod(name: "70 Forgiven Zeal_Zealous Glower (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^434(06|11)$"])]
    public void ForgivenZeal_ZealousGlower(Event @event, ScriptAccessory accessory)
    {
        // Two ActionIds correspond to near to far (43406) / far to near (43411) are meaningless casts, then 4 consecutive ids for subsequent segmented damage
        var zealousGlower = @event.ActionId == 43411 ? "far to near" : "near to far";
        if (isText)accessory.Method.TextInfo($"Light Orb Donut: {zealousGlower}", duration: 8600, true);
        if (isTTS)accessory.Method.TTS($"Donut {zealousGlower}");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Donut {zealousGlower}");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenZeal_ZealousGlower";
        dp.Scale = new (10f, 25f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "70 Forgiven Zeal_Donut Safe Prediction", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^434(07|12|18|23)$"])]
    public void ForgivenZeal_DonutSafePrediction (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        
        dp1.InnerScale = dp.InnerScale = new Vector2(2.92f);
        dp1.Scale = dp.Scale = new Vector2(3f);
        dp1.Radian = dp.Radian = float.Pi * 2;
        dp.Color = accessory.Data.DefaultSafeColor.WithW(10f);
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        
        switch (@event.ActionId())
        {
            case 43407: // Zealous Glower near to far
                dp1.Name = dp.Name = $"ForgivenZeal_ZealousGlower_NearToFarPrediction";
                dp1.Position = dp.Position = @event.EffectPosition();
                dp1.DestoryAt = dp.Delay = 4700;
                dp.DestoryAt = 6000;
                break;
            case 43412: // Zealous Glower far to near
                dp1.Name = dp.Name = $"ForgivenZeal_ZealousGlower_FarToNearPrediction";
                dp1.Position = dp.Position = @event.EffectPosition();
                dp1.DestoryAt = dp.Delay = 4700;
                dp.DestoryAt = 6000;
                break;
            case 43418: // Zealous Eye left clockwise
                dp1.Owner = dp.Owner = @event.SourceId();
                dp1.Offset = dp.Offset = new Vector3(-7.5f, 0f, 0f);
                dp1.Name = dp.Name = $"ForgivenZeal_ZealousEye_LeftClockwisePrediction";
                dp1.DestoryAt = dp.Delay = 8400;
                dp.DestoryAt = 1600;
                break;
            case 43423: // Zealous Eye right counterclockwise
                dp1.Owner = dp.Owner = @event.SourceId();
                dp1.Offset = dp.Offset = new Vector3(7.5f, 0f, 0f);
                dp1.Name = dp.Name = $"ForgivenZeal_ZealousEye_RightCounterclockwisePrediction";
                dp1.DestoryAt = dp.Delay = 8400;
                dp.DestoryAt = 1600;
                break;            
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);

    }
    
    [ScriptMethod(name: "70 Forgiven Zeal_Light Orb Brutal Halo Safe Extent", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43417"])]
    public void ForgivenZeal_LightOrbBrutalHaloSafeExtent (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenZeal_LightOrbBrutalHaloSafeExtent";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = 1700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"ForgivenZeal_LightOrbBrutalHaloOutline";
        dp.Color = accessory.Data.DefaultSafeColor.WithW(20f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(3f);
        dp1.InnerScale = new Vector2(2.94f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 1700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "70 Forgiven Zeal_Ardorous Eye (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^434(18|23)$"])]
    public void ForgivenZeal_ArdorousEye (Event @event, ScriptAccessory accessory)
    {
        // Two skill ids correspond to left clockwise (43418) / right counterclockwise (43423)
        var ardorousEye = @event.ActionId == 43418 ? "starting from the left side of the boss, clockwise" : "starting from the right side of the boss, counterclockwise";
        if (isText)accessory.Method.TextInfo($"Light Orb Donut: {ardorousEye}", duration: 10000, true);
        if (isTTS)accessory.Method.TTS($"Donut {ardorousEye}");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Donut {ardorousEye}");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenZeal_ArdorousEye";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.Delay = 5000;
        dp.DestoryAt = 3400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "70 Forgiven Zeal_2000-mina swing (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43429"])]
    public void ForgivenZeal_2000MinaSwing (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenZeal_2000MinaSwing";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "70 Forgiven Zeal_Disorienting Groan Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43431"])]
    public void ForgivenZeal_DisorientingGroan(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("Center knockback (anti-knockback effective)", duration: 6000, true);
        if (isTTS)accessory.Method.TTS("Knockback");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Knockback");
    }
    
    [ScriptMethod(name: "70 Forgiven Zeal_Octuple Swipe (Cone)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43432"])]
    public void ForgivenZeal_OctupleSwipe(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenZeal_OctupleSwipe";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 24600;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    // Octuple Swipe ActionId: 43432 from start of cast to 8th judgment about 26.2s, omen cast 43437, damage source 4343[3456] probably corresponding to four directions
    
    #endregion
    
    #region  Floors 71~80
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” Floors 71 ~ 80 â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor71(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "71~74 Pilgrim's Detonator_Fracture (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42770"])]
    public void PilgrimsDetonator_Fracture (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsDetonator_Fracture{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "71~74 Pilgrim's Detonator_Self-destruct (Distance Attenuation)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42771"])]
    public void PilgrimsDetonator_SelfDestruct (Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("Move away from self-destruct (distance attenuation)", duration: 4000, true);
        if (isTTS)accessory.Method.TTS("Move away from self-destruct");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Move away from self-destruct");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsDetonator_SelfDestructDangerZone{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(11f);
        dp.DestoryAt = 4700;
        // dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"PilgrimsDetonator_SelfDestruct{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(20f);
        dp1.DestoryAt = 4700;
        dp1.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
    }
    
    [ScriptMethod(name: "71~74 Forgiven Doubt_Gravel Shower (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44763"])]
    public void ForgivenDoubt_GravelShower(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenDoubt_GravelShower{@event.SourceId()}";
        dp.Scale = new (4f, 10f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "71~73 Pilgrim's Hinged_Sandblast (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44769"])]
    public void PilgrimsHinged_Sandblast(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsHinged_Sandblast{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "72~75 Forgiven Malice_Ablution (Circle Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42578|42748)$"])]
    public void ForgivenMalice_Ablution (Event @event, ScriptAccessory accessory)
    {
        // First Circle: 42578 ; First Donut: 42748
        const float innerScale = 10f;  // Circle range / Donut inner radius
        const float outerScale = 40f;  // Donut outer radius
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Color = dp.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = dp.Owner = @event.SourceId();
        dp1.Delay = dp.DestoryAt = 4700;
        dp1.DestoryAt = 3000;
        
        if (@event.ActionId == 42578)
        {
            dp.Name = $"ForgivenMalice_Ablution_Circle{@event.SourceId()}";
            dp.Scale = new Vector2(innerScale);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp1.Name = $"ForgivenMalice_Ablution_Donut{@event.SourceId()}";
            dp1.Scale = new Vector2(outerScale);
            dp1.InnerScale = new Vector2(innerScale);
            dp1.Radian = float.Pi * 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
        }
        else
        {
            dp.Name = $"ForgivenMalice_Ablution_Donut{@event.SourceId()}";
            dp.Scale = new Vector2(outerScale);
            dp.InnerScale = new Vector2(innerScale);
            dp.Radian = float.Pi * 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
            dp1.Name = $"ForgivenMalice_Ablution_Circle{@event.SourceId()}";
            dp1.Scale = new Vector2(innerScale);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
        }
    }
    
    [ScriptMethod(name: "73~76 Forgiven Pride_Hail of Heels (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44759"])]
    public void ForgivenPride_HailOfHeels(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenPride_HailOfHeels{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = 9700; // Cast time 2.7s, will activate 4 times consecutively
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "74~76 Pilgrim's Giant Worm_Earthquake (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44767"])]
    public void PilgrimsGiantWorm_Earthquake (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsGiantWorm_Earthquake{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "75~77 Pilgrim's Ammit_Topple (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44765"])]
    public void PilgrimsAmmit_Topple (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsAmmit_Topple{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "75~78 Forgiven Strife_Trounce (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42522"])]
    public void ForgivenStrife_Trounce(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenStrife_Trounce{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 60f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "75~78 Forgiven Strife_Mighty Spin (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42520"])]
    public void ForgivenStrife_MightySpin (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenStrife_MightySpin{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(14f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "76~79 Forgiven Corruption_Rolling Barrage (Large Circle after disengage)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42523"])]
    public void ForgivenCorruption_RollingBarrage (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenCorruption_RollingBarrage{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(45f);
        dp.DestoryAt = 15700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"ForgivenCorruption_RollingBarrageOutline{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(45f);
        dp1.InnerScale = new Vector2(44.94f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 15700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "76~79 Forgiven Corruption_Forward Barrage (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42577"])]
    public void ForgivenCorruption_ForwardBarrage(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenCorruption_ForwardBarrage{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "77~79 Forgiven Slander_Orogenic Storm (Target Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44762"])]
    public void ForgivenSlander_OrogenicStorm(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "ForgivenSlander_OrogenicStorm";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "77~79 Forgiven Slander_Metamorphic Blast (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44761"])]
    public void ForgivenSlander_MetamorphicBlast(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenSlander_MetamorphicBlast{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "77~79 Forgiven Boasting_Peripheral Lasers (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44758"])]
    public void ForgivenBoasting_PeripheralLasers (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenBoasting_PeripheralLasers{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"ForgivenBoasting_PeripheralLasers{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(60f);
        dp1.InnerScale = new Vector2(59.95f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "77~79 Forgiven Boasting_Cross Lasers (Cross)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44757"])]
    public void ForgivenBoasting_CrossLasers(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenBoasting_CrossLasers{@event.SourceId()}";
        dp.Scale = new (10f, 120f); // To be corrected
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        
        float[] rotations = { 0f, 90f };
    
        foreach (float rotation in rotations)
        {
            dp.Rotation = rotation.DegToRad();
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    
    // 80 Boss
    
    uint PerilousLair=0; // Painful Circle Circle
    uint RoaringRing=0; // Purple Thunder Ring Donut
    
    [ScriptMethod(name: "80 Forgiven Disrespect_Perilous Lair (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43472"])]
    public void ForgivenDisrespect_PerilousLair (Event @event, ScriptAccessory accessory)
    {
        PerilousLair = 1;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenDisrespect_PerilousLair";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "80 Forgiven Disrespect_Roaring Ring (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43468"])]
    public void ForgivenDisrespect_RoaringRing (Event @event, ScriptAccessory accessory)
    {
        RoaringRing = 1;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenDisrespect_RoaringRing";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(48f);
        dp.InnerScale = new Vector2(8f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Painful Circle & Roaring Ring Variable Destruction", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^434(68|72)$"],userControl:false)]
    public void PainfulCircleRoaringRingDestruction(Event @event, ScriptAccessory accessory)
    {
        PerilousLair = 0;
        RoaringRing = 0;
    }
    
    [ScriptMethod(name: "80 Forgiven Disrespect_Profane Waul (Light Half-Room Slice)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43473"])]
    public void ForgivenDisrespect_ProfaneWaul(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ForgivenDisrespect_ProfaneWaul";
        dp.Color = new Vector4(1f, 1f, 1f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 180f.DegToRad(); 
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "80 Forgiven Disrespect_Shadow of Death Tip", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4518"])]
    public void ForgivenDisrespect_ShadowOfDeath(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("Eat white half-room slice", duration: 10000, true);
        if (isTTS)accessory.Method.TTS("Eat white half-room slice");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Eat white half-room slice");
    }
    
    [ScriptMethod(name: "80 Forgiven Disrespect_Nowhere to Run Tip", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4519", "Param:1"])]
    public void ForgivenDisrespect_NowhereToRun(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText)accessory.Method.TextInfo("Reduce movement, don't reach 8 stacks", duration: 16300, true);
        if (isTTS)accessory.Method.TTS("Reduce movement");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Reduce movement");
    }
    
    #endregion
    
    #region  Floors 81~90
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” Floors 81 ~ 90 â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor81(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "81~83 Summoning Gremlin_Claw (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44770"])]
    public void SummoningGremlin_Claw(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningGremlin_Claw{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "81~83 Pilgrim's Hyaena_Nox Blast (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44774"])]
    public void PilgrimsHyaena_NoxBlast(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsHyaena_NoxBlast{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "81~83 Pilgrim's Hyaena_Maul TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44773"])]
    public void PilgrimsHyaena_Maul(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("Interrupt Hyaena tankbuster (no resistance)", duration: 11300, true);
        if (isTTS)accessory.Method.TTS("Interrupt Hyaena tankbuster");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Interrupt Hyaena tankbuster");
    }
    
    [ScriptMethod(name: "81~84 Pilgrim's Kabuso_Dark II (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44775"])]
    public void PilgrimsKabuso_DarkII(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsKabuso_DarkII{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
        
    [ScriptMethod(name: "81~84 Pilgrim's Cavalry_Storm Slash (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43321"])]
    public void PilgrimsCavalry_StormSlash(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsCavalry_StormSlash{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "81~84 Pilgrim's Cavalry_Valfodr (Charge Knockback)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43319"])]
    public void PilgrimsCavalry_Valfodr(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("Charge knockback + triple knockback");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Charge knockback + triple knockback");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsCavalry_Valfodr{@event.SourceId()}";
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Scale = new (6f);
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
    }
    
    [ScriptMethod(name: "83~85 Summoning Imp_Blizzard Trap (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44779"])]
    public void SummoningImp_BlizzardTrap (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningImp_BlizzardTrap{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "84~86 Summoning High Demon_Abyssal Swing (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44780"])]
    public void SummoningHighDemon_AbyssalSwing(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningHighDemon_AbyssalSwing{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "84~88 Pilgrim's Karma_Claw and Tail Front", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43132"])]
    public void PilgrimsKarma_ClawAndTailFront1(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsKarma_ClawAndTailFront1{@event.SourceId()}";
        dp.Scale = new (3f, 8f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "84~88 Pilgrim's Karma_Claw and Tail Back", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43132"])]
    public void PilgrimsKarma_ClawAndTailBack2(Event @event, ScriptAccessory accessory)
    {
        // Second stage tail sweep ActionId: 43131
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsKarma_ClawAndTailBack2{@event.SourceId()}";
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.Radian = 120f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.Delay = 2700;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "86~89 Summoning Humbaba_Triple & Quadruple Blow Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4332[34]$"])]
    public void SummoningHumbaba_TripleQuadrupleBlow(Event @event, ScriptAccessory accessory)
    {
        string skullDasherCount = @event.ActionId == 43323 ? "triple" : "quadruple";
    
        if (isText) accessory.Method.TextInfo($"{skullDasherCount} consecutive auto-attacks + large cleave", duration: 4000, true);
        if (isTTS) accessory.Method.TTS($"{skullDasherCount} auto-attacks then cleave");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"{skullDasherCount} auto-attacks then cleave");
    }
    
    [ScriptMethod(name: "86~89 Summoning Humbaba_Bellows (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44293"])]
    public void SummoningHumbaba_Bellows (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningHumbaba_Bellows{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 1200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "84~86 Summoning Nightmare_Passions' Heat (Fever Target)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43322"])]
    public void SummoningNightmare_PassionsHeat (Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Stun <Summoning Nightmare> range fever", duration: 4300, true);
        if (isTTS) accessory.Method.TTS($"Stun Nightmare fever");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Stun Nightmare fever");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningNightmare_PassionsHeat{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "86~89 Summoning Minstrel_Dark II (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44787"])]
    public void SummoningMinstrel_DarkII(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningMinstrel_DarkII{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(20f);
        dp.Radian = 60f.DegToRad();
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "86~89 Summoning Minstrel_Inner Demons (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44788"])]
    public void SummoningMinstrel_InnerDemons (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningMinstrel_InnerDemons{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "86~89 Summoning Cerberus_Blitzen (Front Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44784"])]
    public void SummoningCerberus_Blitzen (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningCerberus_Blitzen{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "86~89 Summoning Cerberus_Hellpounce (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44785"])]
    public void SummoningCerberus_Hellpounce(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningCerberus_Hellpounce{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "86-89 Summoning Cerberus_Tail Blow (Back Cone)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44786"])]
    public void SummoningCerberus_TailBlow(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningCerberus_TailBlow{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(19f);
        dp.Radian = 90f.DegToRad(); 
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "86-89 Summoning Baal_Incinerating Lahar (Large Circle after disengage)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43133"])]
    public void SummoningBaal_IncineratingLahar (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningBaal_IncineratingLahar{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(46f);
        dp.DestoryAt = 15700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"SummoningBaal_IncineratingLaharOutline{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(46f);
        dp1.InnerScale = new Vector2(45.96f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 15700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "86~89 Summoning Baal_Abyssal Ray (Wall-Piercing Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43134"])]
    public void SummoningBaal_AbyssalRay(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningBaal_AbyssalRay{@event.SourceId()}";
        dp.Scale = new (20f, 40f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    // 90 Boss
    
    [ScriptMethod(name: "90 Manakel_Backhand (Cone)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4425[01]$"])]
    public void Manakel_Backhand(Event @event, ScriptAccessory accessory)
    {
        // Lower left safe: 44250 ; Lower right safe: 44251
        var isR = @event.ActionId == 44250;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Manakel_Backhand";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 270f.DegToRad();
        dp.Rotation = isR ? 315f.DegToRad() : 45f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "90 Manakel_Fore-hind Folly & Twin-winged Treachery (Front/Back Left/Right Cones)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44262"])]
    public void Manakel_MagicImpact(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Manakel_MagicImpact";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(35f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 7300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "90 Directional Magic Circle_Arcane Beacon (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43796|44257)$"])]
    public void DirectionalMagicCircle_ArcaneBeacon(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"DirectionalMagicCircle_ArcaneBeacon";
        dp.Scale = new (10f, 50f);
        dp.Rotation = 180f.DegToRad(); // Entity fires outward, need to rotate back inward
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.ActionId == 43796 ? 5300 : 7300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "90 Manakel_Meteorite (Ground Yellow Circle Judgment Time)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44268"])]
    public void Manakel_Meteorite (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Manakel_Meteorite";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "90 Manakel_Skinflayer Safe", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44266"])]
    public void Manakel_SkinflayerSafe(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Manakel_SkinflayerSafe";
        dp.Scale = new (40f, 10f); // Arena 40m (10m x 4 cells), knockback distance 30m, one row in front of boss is knockback safe zone
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.8f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "90 Manakel_Skinflayer Distance Prediction", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44266"])]
    public void Manakel_SkinflayerDistancePrediction(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("Get close for knockback", duration: 4300, true);
        if (isTTS)accessory.Method.TTS("Get close for knockback");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Get close for knockback");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Manakel_SkinflayerDistancePrediction";
        dp.Scale = new(1.5f, 30);
        dp.Color = new Vector4(0f, 1f, 1f, 3f);
        dp.Owner = accessory.Data.Me;
        dp.Rotation = @event.SourceRotation();
        dp.FixRotation = true;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    #endregion
    
    #region Floors 91~98 
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” Floors 91 ~ 100 â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void Floor91(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "91~94 Summoning Doll_Whinge (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44512"])]
    public void SummoningDoll_Whinge (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningDoll_Whinge{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "91~98 Summoning Nightmare_Dark Vision (Wall-Piercing Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44349"])]
    public void SummoningNightmare_DarkVision(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("Wall-piercing line");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Wall-piercing line");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningNightmare_DarkVision{@event.SourceId()}";
        dp.Scale = new (5f, 41f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
                      else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "91~98 Summoning Nightmare_Endless Nightmare (Self-Destruct Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44350"])]
    public void SummoningNightmare_EndlessNightmare (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningNightmare_EndlessNightmare{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(18f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"SummoningNightmare_EndlessNightmareOutline{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp1.Owner = @event.SourceId();
        dp1.Scale = new Vector2(18f);
        dp1.InnerScale = new Vector2(17.96f);
        dp1.Radian = float.Pi * 2;
        dp1.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
    }
    
    [ScriptMethod(name: "91~93 Pilgrim's Violet_Violet Creeper_Creeping Ivy (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44499"])]
    public void PilgrimsVioletVioletCreeper_CreepingIvy(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsVioletVioletCreeper_CreepingIvy{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(9f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "91~93 Pilgrim's Rafflesia_Rotten Stench (Wall-Piercing Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44500"])]
    public void PilgrimsRafflesia_RottenStench(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("Wall-piercing line");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Wall-piercing line");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsRafflesia_RottenStench{@event.SourceId()}";
        dp.Scale = new (12f, 45f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "93~95 Pilgrim's Warg_Triple & Quadruple Skull Dasher Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^443(39|40)$"])]
    public void PilgrimsWarg_TripleQuadrupleSkullDasher(Event @event, ScriptAccessory accessory)
    {
        string skullDasherCount = @event.ActionId == 44339 ? "triple" : "quadruple";
    
        if (isText) accessory.Method.TextInfo($"{skullDasherCount} consecutive auto-attacks + circle", duration: 4000, true);
        if (isTTS) accessory.Method.TTS($"{skullDasherCount} auto-attacks then circle");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"{skullDasherCount} auto-attacks then circle");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsWarg_HeavySmashPrediction{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.InnerScale = new Vector2(5.98f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "93~95 Pilgrim's Warg_Heavy Smash (Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44342"])]
    public void PilgrimsWarg_HeavySmash (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsWarg_HeavySmash{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 1200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "93~95 Summoning Zahak_Petribreath (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44508"])]
    public void SummoningZahak_Petribreath(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningZahak_Petribreath{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(28f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "93~95 Summoning Zahak_Tail Drive (Instant Tail Sweep)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:18854"])]
    public void SummoningZahak_TailDrive(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningZahak_TailDrive{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.5f); // Reference value, target circle 3.5m
        dp.Radian = 90f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 600000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "SummoningZahak_TailDriveDestruction", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:18854"],userControl:false)]
    public void SummoningZahak_TailDriveDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"SummoningZahak_TailDrive{@event.SourceId()}");
    }

    [ScriptMethod(name: "SummoningZahak_TailDriveBackupDestruction", eventType: EventTypeEnum.Death, eventCondition: ["TargetDataId:18854"],userControl:false)]
    public void SummoningZahak_TailDriveBackupDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"SummoningZahak_TailDrive{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "93~98 Summoning Sawtooth_Honeyed Spit (Front/Left/Right)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4435[6-8]$"])]
    public void SummoningSawtooth_HoneyedSpit(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningSawtooth_HoneyedSpit{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.DestoryAt = 6700;
    
        switch (@event.ActionId())
        {
            case 44356: // Front cone
                dp.Name = $"SummoningSawtooth_FrontHoneyedSpit{@event.SourceId()}";
                dp.Radian = 120f.DegToRad();
                dp.Rotation = 0f;
                break;
            case 44357: // Left half-room
                dp.Name = $"SummoningSawtooth_LeftHoneyedSpit{@event.SourceId()}";
                dp.Radian = 180f.DegToRad();
                dp.Rotation = 135f.DegToRad();
                break;
            case 44358: // Right half-room
                dp.Name = $"SummoningSawtooth_RightHoneyedSpit{@event.SourceId()}";
                dp.Radian = 180f.DegToRad();
                dp.Rotation = 225f.DegToRad();
                break;
        }
    
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "95~97 Pilgrim's Malice_Smashing Blow (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44513"])]
    public void PilgrimsMalice_SmashingBlow(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"PilgrimsMalice_SmashingBlow{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(14f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "95~98 Summoning Old Demon_Unholy Darkness (Target Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44514"])]
    public void SummoningOldDemon_UnholyDarkness (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningOldDemon_UnholyDarkness{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "95_98 Summoning Old Demon_Karma (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44515"])]
    public void SummoningOldDemon_Karma(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningOldDemon_Karma{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(35f);
        dp.Radian = 90f.DegToRad(); 
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "96~98 Summoning Acheron_Authority's Edge (Left/Right Slices)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^445(09|10)$"])]
    public void SummoningAcheron_AuthoritysEdge(Event @event, ScriptAccessory accessory)
    {
        // Right slice 44509 ; Left slice 44510
        var isR = @event.ActionId == 44509;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningAcheron_AuthoritysEdge{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 180f.DegToRad(); 
        dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "96~98 Summoning Gusion_Left & Right Smite (Two-Stage Left/Right Slices)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4434[57]$"])]
    public void SummoningGusion_LeftRightSmite(Event @event, ScriptAccessory accessory)
    {
        // First right slice 44345 ; First left slice 44347
        // This enemy's prototype is from Snowcloak's 1st boss_White Beast, its skill name and (XAxis, EffectRange) are also consistent. The arena in Snowcloak is rectangular, making it easier to see the actual range
        var isR = @event.ActionId == 44345;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningGusion_LeftRightSmite{@event.SourceId()}";
        dp.Scale = new (40f, 80f);
        dp.Offset = new Vector3(isR ? 20f : -20f, 0f, 0f);
        // dp.Rotation = isR ? 270f.DegToRad() : 90f.DegToRad();
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp); 
        
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"SummoningGusion_RightLeftSmite{@event.SourceId()}";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Owner = @event.SourceId();
        dp1.Scale = new (40f, 80f);
        dp1.Offset = new Vector3(isR ? -20f : 20f, 0f, 0f);
        // dp1.Rotation = isR ? 90f.DegToRad() : 270f.DegToRad();
        dp1.Delay = 5000;
        dp1.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1); 
    }
    
    [ScriptMethod(name: "96~98 Summoning Destruction_Stare (Wall-Piercing Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44354"])]
    public void SummoningDestruction_Stare(Event @event, ScriptAccessory accessory)
    {
        if (isTTS)accessory.Method.TTS("Wall-piercing line");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Wall-piercing line");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningDestruction_Stare{@event.SourceId()}";
        dp.Scale = new (8f, 60f);
        dp.Owner = @event.SourceId();
        if (isUnderGround) {dp.Color = UnderGround_AOEs.V4; }
        else {dp.Color = accessory.Data.DefaultDangerColor; }
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "96~98 Summoning Destruction_Mortal Gaze Line (Lookaway Tether)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44355"])]
    public void SummoningDestruction_MortalGazeLine(Event @event, ScriptAccessory accessory)
    { 
        if (isText)accessory.Method.TextInfo("Look away from <Summoning Destruction>", duration: 2300, true);
        if (isTTS)accessory.Method.TTS("Look away from big eye");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Look away from big eye");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SummoningDestruction_MortalGazeLine{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 1f, 1f);
        dp.Owner = @event.SourceId;
        dp.TargetObject = accessory.Data.Me;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Scale = new(1);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "96~98 Summoning Destruction_Mortal Gaze Extent", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44355"])]
    public void SummoningDestruction_MortalGazeExtent(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "SummoningDestruction_MortalGazeExtent";
        dp.Color = new Vector4(1f, 0f, 1f, 10f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(59.95f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    #endregion

    #region The Final Verse Floor Fire Related
    
    private readonly Dictionary<uint, string> crystalDirections = new Dictionary<uint, string>();
    private readonly HashSet<uint> processedCrystals = new HashSet<uint>();
    private readonly Dictionary<Vector3, string> crystalPositionDirections = new Dictionary<Vector3, string>();

    private int firstGroupCount = 0;
    private int secondGroupCount = 0;
    private int firstGroupAddCount = 0;
    private int secondGroupAddCount = 0;
    private bool isFirstGroupComplete = false;
    private string firstGroupDirection = "vertical";
    private string secondGroupDirection = "horizontal"; 
    private bool resetScheduled = false;
    
    // Crystal generation: 44115 (6 each time) / Crystal DataId: 2014832 // Moves 4m each time, explosion interval 0.8~0.9s

    [ScriptMethod(name: "Abyssal Blaze Crystal Generation Skill", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44078|44115)"], userControl:false)]
    public void Q40_AbyssalBlazeCrystalGenerationSkill(Event @event, ScriptAccessory accessory)
    {
        Vector3 spawnPosition = @event.EffectPosition;
        string direction;
        
        if (HelperExtensions.GetCurrentTerritoryId() == MapIds.TheFinalVerseQuantum) // Deep thinking battle is 6+6 crystals
        {
            if (firstGroupAddCount < 6)
            {
                direction = firstGroupDirection;
                firstGroupAddCount++;
                // if(isDeveloper) accessory.Method.SendChat($"/e [FloorFire Generation] First group {firstGroupAddCount}/6: Position {spawnPosition}, Direction {direction}");
            }
            else if (secondGroupAddCount < 6)
            {
                direction = secondGroupDirection;
                secondGroupAddCount++;
                // if(isDeveloper) accessory.Method.SendChat($"/e [FloorFire Generation] Second group {secondGroupAddCount}/6: Position {spawnPosition}, Direction {direction}");
            }
            else
            {
                direction = "error";
                if (isDeveloper) accessory.Method.SendChat($"/e [FloorFire Warning] Action generated crystals exceeding 12 limit");
            }

            // Directly record position and direction
            crystalPositionDirections[spawnPosition] = direction;

            if (firstGroupAddCount + secondGroupAddCount == 12)
            {
                if (isDeveloper) accessory.Method.SendChat($"/e [FloorFire] Completed 12 crystal recordings via ActionId");
            }
        }
        else // Normal is 4+4 crystals
        {
            if (firstGroupAddCount < 4)
            {
                direction = firstGroupDirection;
                firstGroupAddCount++;
                // if(isDeveloper) accessory.Method.SendChat($"/e [FloorFire Generation] First group {firstGroupAddCount}/4: Position {spawnPosition}, Direction {direction}");
            }
            else if (secondGroupAddCount < 4)
            {
                direction = secondGroupDirection;
                secondGroupAddCount++;
                // if(isDeveloper) accessory.Method.SendChat($"/e [FloorFire Generation] Second group {secondGroupAddCount}/4: Position {spawnPosition}, Direction {direction}");
            }
            else
            {
                direction = "error";
                if (isDeveloper) accessory.Method.SendChat($"/e [FloorFire Warning] Action generated crystals exceeding 8 limit");
            }

            // Directly record position and direction
            crystalPositionDirections[spawnPosition] = direction;

            if (firstGroupAddCount + secondGroupAddCount == 8)
            {
                if (isDeveloper) accessory.Method.SendChat($"/e [FloorFire] Completed 8 crystal recordings via ActionId");
            }
        }
    }
    
    [ScriptMethod(name: "Abyssal Blaze Crystal Generation Debug", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2014832"], userControl:false)]
    public void Q40_AbyssalBlazeCrystalGenerationDebug(Event @event, ScriptAccessory accessory)
    {
        uint crystalSourceId = @event.SourceId();
        Vector3 crystalPosition = @event.SourcePosition();
        
        // if(isDeveloper) accessory.Method.SendChat($"/e [FloorFire Debug] ObjectChanged: {crystalSourceId} Position {crystalPosition}");
    }
    
    [ScriptMethod(name: "Abyssal Blaze Boss Cast Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4407[45]|4479[78])$"] ,userControl:false)]
    public void Q40_AbyssalBlazeBossCastRecord(Event @event, ScriptAccessory accessory)
    {
        ResetMechanic();
        
        switch (@event.ActionId)
        {
            case 44075:
            case 44798:
                firstGroupDirection = "vertical";   // First up-down
                secondGroupDirection = "horizontal"; // Then left-right
                break;
            case 44074:
            case 44797:
                firstGroupDirection = "horizontal"; // First left-right
                secondGroupDirection = "vertical";   // Then up-down
                break;
            default:
                firstGroupDirection = "???";
                secondGroupDirection = "???";
                break;
        }
        
        if(isDeveloper) accessory.Method.SendChat($"/e [FloorFire] Reset: First {firstGroupDirection} then {secondGroupDirection}, ActionId[{@event.ActionId}] counters cleared");
    }
    
    private string FindDirectionByPosition(Vector3 position, float tolerance = 0.5f)
    {
        foreach (var kvp in crystalPositionDirections)
        {
            if (IsPositionMatch(kvp.Key, position, tolerance))
            {
                return kvp.Value;
            }
        }
        return null;
    }
    
    private bool IsPositionMatch(Vector3 pos1, Vector3 pos2, float tolerance = 0.5f)
    {
        return Math.Abs(pos1.X - pos2.X) < tolerance &&
               Math.Abs(pos1.Y - pos2.Y) < tolerance &&
               Math.Abs(pos1.Z - pos2.Z) < tolerance;
    }
    
    private List<Vector3> CalculateStepPositions(Vector3 startPos, string direction, int step)
    {
        float offset = 4f * step;
        var positions = new List<Vector3>();

        switch (direction)
        {
            case "vertical":
                positions.Add(new Vector3(startPos.X, startPos.Y, startPos.Z + offset));
                positions.Add(new Vector3(startPos.X, startPos.Y, startPos.Z - offset));
                break;
            case "horizontal":
                positions.Add(new Vector3(startPos.X + offset, startPos.Y, startPos.Z));
                positions.Add(new Vector3(startPos.X - offset, startPos.Y, startPos.Z));
                break;
            default:
                positions.Add(startPos);
                break;
        }
        return positions;
    }
    
    private async void ScheduleDelayedReset(ScriptAccessory accessory)
    {
        int maxWaitTime = 30000;
        
        await System.Threading.Tasks.Task.Delay(maxWaitTime);
        
        if (processedCrystals.Count > 0)
        {
            ResetMechanic();
            if(isDeveloper) accessory.Method.SendChat($"/e [Debug] Reset floor fire crystal counter");
        }
    }
    
    private void ResetMechanic()
    {
        firstGroupAddCount = 0;
        secondGroupAddCount = 0;
        crystalPositionDirections.Clear();
        firstGroupDirection = "vertical";
        secondGroupDirection = "horizontal";
        resetScheduled = false;
        processedCrystals.Clear();
        crystalDirections.Clear();
    }
    
    #endregion
    
    #region Floor 99 Boss The Final Verse
    
    // The Final Verse      NPCID: 14037 Target circle 28.5m
    // Devoured Eater  NPCID: 14038 Target circle 15.0m
    // Abyssal Blaze (Step Floor Fire) Normal difficulty first up-down ActionId: 44075 / then left-right ActionId: 44076
    // Summon Crystal: 44078 / Crystal Explosion: 44079   first left-right ActionId: 44074 / then up-down ActionId: 44077
    
    uint _myLightVengeance=0;
    uint _myDarkVengeance=0; 
    
    [ScriptMethod(name: "99 Chains of Condemnation (Fever)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4406[39]$"])]
    public void Normal_ChainsOfCondemnation(Event @event, ScriptAccessory accessory)
    {
        // Main meaningless cast Fast: 44063 Source: 44064 / Slow: 44069 Source:44070, where Source has 0.7s longer cast than meaningless
        // Chains of Condemnation (Fever) StatusID: 4562, application time about 2.6s
        
        bool isFastCast = @event.ActionId == 44063;
        bool isSlowCast = @event.ActionId == 44069;
    
        int duration = isFastCast ? 4000 : 7000;
        string timingType = isFastCast ? "early" : "late";

        if (isText) accessory.Method.TextInfo($"Stop moving {timingType} for line", duration: duration, true);
        if (isTTS) accessory.Method.TTS($"Stop moving {timingType} for line");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Stop moving {timingType} for line");
    }
    
    [ScriptMethod(name: "99 Fireball (Cyclone) Portent Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4406[18]$"])]
    public void Normal_FireballPortent(Event @event, ScriptAccessory accessory)
    {
        bool isFastCast = @event.ActionId == 44061;
        bool isSlowCast = @event.ActionId == 44068;

        int duration = isFastCast ? 5000 : 8000;
        string timingType = isFastCast ? "early" : "late";

        if (isText) accessory.Method.TextInfo($"Line {timingType} cyclone", duration: duration, true);
        if (isTTS) accessory.Method.TTS($"Line {timingType} cyclone");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Line {timingType} cyclone");
    }
    
    [ScriptMethod(name: "99 Fireball (Cyclone)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44062"])]
    public void Normal_Fireball (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Fireball";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 1800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "99 The Final Verse_Spinelash Portent (Piercing Target Line Omen)", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:00EA"])]
    public void Normal_SpinelashPortent(Event @event, ScriptAccessory accessory)
    {
        if (HelperExtensions.GetCurrentTerritoryId() != MapIds.TheFinalVerse) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        var boss = accessory.Data.Objects.GetByDataId(18666).FirstOrDefault(); // Extermination battle - The Final Verse DataId: 18666
        if (boss == null) return;
        dp.Owner = boss.GameObjectId;

        dp.Name = $"SpinelashPortent";
        dp.Scale = new (4f, 60f);
        dp.Color = accessory.Data.DefaultSafeColor.WithW(0.6f);
        dp.FixRotation = true;
        dp.DestoryAt = 6400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "99 The Final Verse_Spinelash (Piercing Target Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45118"])]
    public void Normal_Spinelash(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("Dodge", duration: 800, true);
        if (isTTS)accessory.Method.TTS("Dodge");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Dodge");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Spinelash";
        dp.Scale = new (4f, 60f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 1500;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "99 Blade of First Light (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^440(67|73)$"])]
    public void Normal_BladeOfFirstLight(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"BladeOfFirstLight";
        dp.Scale = new (15f, 30f);
        dp.Owner = @event.SourceId();
        dp.Offset = new Vector3 (0, 0 ,15); // The entity is originally in the middle of the line, using Straight, but considering omen effect, using Rect offset is better
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.DurationMilliseconds();
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "99 Bounds of Sin TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44082"])]
    public void Normal_BoundsOfSinTTS(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("Trapping prison", duration: 3000, true);
        if (isTTS)accessory.Method.TTS("Trapping prison");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Trapping prison");
    }
    
    [ScriptMethod(name: "99 Bounds of Sin (Trapping Prison Judgment Animation)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44083"])]
    public void Normal_BoundsOfSin (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"BoundsOfSin";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "99 Abyssal Blaze (Floor Fire) Initial Explosion Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44079"])]
    public void Normal_AbyssalBlazeInitial(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "AbyssalBlazeInitial";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 6700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
   [ScriptMethod(name: "99 Abyssal Blaze (Floor Fire) Step Explosion", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:44079"])]
    public void Normal_AbyssalBlazeStep(Event @event, ScriptAccessory accessory)
    {
        uint fireSourceId = @event.SourceId();
        Vector3 firePosition = @event.SourcePosition();
        
        string direction = FindDirectionByPosition(firePosition);
        
        if (string.IsNullOrEmpty(direction))
        {
            direction = "vertical";
            if(isDeveloper) accessory.Method.SendChat($"/e [FloorFire Warning] No direction record found for position {firePosition}");
            return;
        }
        
        // if(isDeveloper) accessory.Method.SendChat($"/e [FloorFire] Floor fire source {fireSourceId} using direction: {direction}");
        
        int maxSteps = (direction == "vertical") ? 7 : 10;
        
        for (int predictStep = 1; predictStep <= 2; predictStep++)
        {
            var predictPositions = CalculateStepPositions(firePosition, direction, predictStep);
            foreach (var predictPos in predictPositions)
            {
                var predictDp = accessory.Data.GetDefaultDrawProperties();
                predictDp.Name = $"AbyssalBlazeStep{predictStep}Prediction";
                predictDp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
                predictDp.Owner = fireSourceId;
                predictDp.Position = predictPos;
                predictDp.Scale = new Vector2(5f);
                predictDp.DestoryAt = 800 * predictStep;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, predictDp);
            }
        }
        
        for (int step = 1; step <= maxSteps; step++) 
        {
            var stepPositions = CalculateStepPositions(firePosition, direction, step);
            
            foreach (var stepPos in stepPositions)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"AbyssalBlazeStep{step}";
                dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
                dp.Owner = fireSourceId;
                dp.Position = stepPos;
                dp.Scale = new Vector2(5f);
                dp.DestoryAt = 1000;
                dp.Delay = 800 * step;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                
                for (int predictStep = step + 1; predictStep <= step + 2; predictStep++)
                {
                    if (predictStep <= maxSteps)
                    {
                        var nextStepPositions = CalculateStepPositions(firePosition, direction, predictStep);
                        foreach (var nextStepPos in nextStepPositions)
                        {
                            var predictDp = accessory.Data.GetDefaultDrawProperties();
                            predictDp.Name = $"AbyssalBlazeStep{predictStep}Prediction";
                            
                            float alpha = predictStep == step + 1 ? 0.8f : 0.4f;
                            predictDp.Color = accessory.Data.DefaultDangerColor.WithW(alpha);
                            
                            predictDp.Owner = fireSourceId;
                            predictDp.Position = nextStepPos;
                            predictDp.Scale = new Vector2(5f);
                            predictDp.DestoryAt = 1000;
                            predictDp.Delay = 800 * step; 
                            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, predictDp);
                        }
                    }
                }
            }
        }
    }
    
    [ScriptMethod(name: "99 Drain Aether (Buff Detection)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4409[02]$"])]
    public void Normal_DrainAether(Event @event, ScriptAccessory accessory)
    {
        // debuff: 4559 Dark / 4560 Light
        // Drain Aether Normal difficulty 44088 Short Dark / 44089 Long Dark / 44090 Short Light / 44092 Long Light
        (string firstDrainAether, string secondDrainAether) = @event.ActionId switch
        {
            // 44088 => ("Eat Light", "Eat Dark"), // Short Dark
            // 44089 => ("Eat Dark", "Eat Light"), // Long Dark
            44090 => ("Eat Dark", "Eat Light"), // Short Light
            44092 => ("Eat Light", "Eat Dark"), // Long Light
            _ => ("Unknown", "Unknown")
        };
    
        if (isText)accessory.Method.TextInfo($"First {firstDrainAether}, then {secondDrainAether}", duration: 10000, true);
        if (isTTS)accessory.Method.TTS($"First {firstDrainAether}, then {secondDrainAether}");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"First {firstDrainAether}, then {secondDrainAether}");
    }
    
    
    #endregion
    
    #region The Final Verse Quantum Deep Thinking Battle Full Tribute Difficulty Q40
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” The Final Verse Quantum Deep Thinking Battle â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void TheFinalVerseQuantum(Event @event, ScriptAccessory accessory) { }
    
    // The Final Verse      NPCID: 14037 Target circle 28.5m
    // Devoured Eater  NPCID: 14038 Target circle 15.0m
    
    // P1 Abyssal Blaze (Black-White Match + Tower Stepping + Floor Fire) â†’ Blade of First Light + Chains of Condemnation / Fireball + Tether & Cross Fire â†’ Spinelash (Block Stack) â†’ Focus adds then role position ready for P2
    
    [ScriptMethod(name: "Countdown Buff Renewal Tip", eventType: EventTypeEnum.Countdown, eventCondition: ["Type:Start"])]
    public void CountdownBuffRenewalTip(Event @event, ScriptAccessory accessory)
    {
        if (IbcHelper.HasStatus(accessory, accessory.Data.MyObject, 0x11CF) || 
            IbcHelper.HasStatus(accessory, accessory.Data.MyObject, 0x11D0)) return;
        if (isText)accessory.Method.TextInfo("Get opening buff", duration: 5000, true);
        if (isTTS)accessory.Method.TTS("Get buff");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Get buff");
    }
    
    [ScriptMethod(name: "HP Gap Tip", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2550"])]
    public void Q40_HPGapTip(Event @event, ScriptAccessory accessory)
    {
        string targetName = @event.TargetName();

        var validBosses = new HashSet<string>
        {
            "The Final Verse", "Eminent Grief", "ã‚¨ãƒŸãƒãƒ³ãƒˆã‚°ãƒªãƒ¼ãƒ•",
            "Devoured Eater", "devoured eater", "ä¾µè•ã•ã‚ŒãŸç½ªå–°ã„"
        };

        if (!validBosses.Contains(targetName))
        {
            if(isDeveloper) accessory.Method.SendChat($"/e [DEBUG] Ignoring unit: {targetName}");
            return;
        }

        var bossNameMapping = new Dictionary<string, (string displayName, string oppositeColor)>
        {
            { "The Final Verse", ("Dark", "Light") },
            { "Eminent Grief", ("Dark", "Light") },
            { "ã‚¨ãƒŸãƒãƒ³ãƒˆã‚°ãƒªãƒ¼ãƒ•", ("Dark", "Light") },
    
            { "Devoured Eater", ("Light", "Dark") },
            { "devoured eater", ("Light", "Dark") },
            { "ä¾µè•ã•ã‚ŒãŸç½ªå–°ã„", ("Light", "Dark") }
        };

        if (bossNameMapping.TryGetValue(targetName, out var bossInfo))
        {
            string displayName = bossInfo.displayName;
            string trueColor = bossInfo.oppositeColor; // Eat the opposite color
            
            uint stackCount = @event.StatusParam;
        
            string stackInfo = stackCount >= 1 ? $"{stackCount}" : "";

            if (isText) accessory.Method.TextInfo($"HP gap {stackInfo} layers, eat {trueColor} hit {displayName}", duration: 2000, true);
            if (isTTS) accessory.Method.TTS($"HP gap {stackInfo} layers, eat {trueColor} hit {displayName}");
            if (isEdgeTTS) accessory.Method.EdgeTTS($"HP gap {stackInfo} layers, eat {trueColor} hit {displayName}");
            
            accessory.Method.SendChat($"/e [HP Gap Tip]: Eat {trueColor} hit {displayName} ({stackCount} layers)");
        }
    }

    [ScriptMethod(name: "Abyssal Blaze (Floor Fire) Cast Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4407[45]|4479[78])$"])]
    public void Q40_AbyssalBlazeTip(Event @event, ScriptAccessory accessory)
    {
        string isFirst, isSecond;
 
        switch (@event.ActionId)
        {
            case 44075:
            case 44798:
                isFirst = "up-down";
                isSecond = "left-right";
                break;
            case 44074:
            case 44797:
                isFirst = "left-right";
                isSecond = "up-down";
                break;
            default:
                isFirst = "unknown";
                isSecond = "unknown";
                break;
        }
        
        if (isText) accessory.Method.TextInfo($"Floor fire: First {isFirst}, then {isSecond}", duration: 16700, true);
        if (isTTS) accessory.Method.TTS($"First {isFirst}, then {isSecond}");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"First {isFirst}, then {isSecond}");
        accessory.Method.SendChat($"/e [Kodakku] Floor fire record: First {isFirst}, then {isSecond}");
    }
    
    [ScriptMethod(name: "Abyssal Blaze (Floor Fire) Initial Explosion Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44118"])]
    public void Q40_AbyssalBlazeInitial(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "AbyssalBlazeInitial";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 6700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Abyssal Blaze (Floor Fire) Step Explosion", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:44118"])]
    public void Q40_AbyssalBlazeStep(Event @event, ScriptAccessory accessory)
    {
        uint fireSourceId = @event.SourceId();
        Vector3 firePosition = @event.SourcePosition();
        
        string direction = FindDirectionByPosition(firePosition);
        
        if (string.IsNullOrEmpty(direction))
        {
            direction = "vertical";
            if(isDeveloper) accessory.Method.SendChat($"/e [FloorFire Warning] No direction record found for position {firePosition}");
            return;
        }
        
        // if(isDeveloper) accessory.Method.SendChat($"/e [FloorFire] Floor fire source {fireSourceId} using direction: {direction}");
        
        int maxSteps = (direction == "vertical") ? 7 : 10;
        
        for (int predictStep = 1; predictStep <= 2; predictStep++)
        {
            var predictPositions = CalculateStepPositions(firePosition, direction, predictStep);
            foreach (var predictPos in predictPositions)
            {
                var predictDp = accessory.Data.GetDefaultDrawProperties();
                predictDp.Name = $"AbyssalBlazeStep{predictStep}Prediction";
                predictDp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
                predictDp.Owner = fireSourceId;
                predictDp.Position = predictPos;
                predictDp.Scale = new Vector2(5f);
                predictDp.DestoryAt = 800 * predictStep;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, predictDp);
            }
        }
        
        for (int step = 1; step <= maxSteps; step++) 
        {
            var stepPositions = CalculateStepPositions(firePosition, direction, step);
            
            foreach (var stepPos in stepPositions)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"AbyssalBlazeStep{step}";
                dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
                dp.Owner = fireSourceId;
                dp.Position = stepPos;
                dp.Scale = new Vector2(5f);
                dp.DestoryAt = 1000;
                dp.Delay = 800 * step;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                
                for (int predictStep = step + 1; predictStep <= step + 2; predictStep++)
                {
                    if (predictStep <= maxSteps)
                    {
                        var nextStepPositions = CalculateStepPositions(firePosition, direction, predictStep);
                        foreach (var nextStepPos in nextStepPositions)
                        {
                            var predictDp = accessory.Data.GetDefaultDrawProperties();
                            predictDp.Name = $"AbyssalBlazeStep{predictStep}Prediction";
                            
                            float alpha = predictStep == step + 1 ? 0.8f : 0.4f;
                            predictDp.Color = accessory.Data.DefaultDangerColor.WithW(alpha);
                            
                            predictDp.Owner = fireSourceId;
                            predictDp.Position = nextStepPos;
                            predictDp.Scale = new Vector2(5f);
                            predictDp.DestoryAt = 1000;
                            predictDp.Delay = 800 * step; 
                            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, predictDp);
                        }
                    }
                }
            }
        }
    }
    
    [ScriptMethod(name: "Abyssal Sun Tower Stepping Tip", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:2", "Index:27"])]
    public void Q40_AbyssalSunTip (Event @event, ScriptAccessory accessory)
    {
        // Actually Index is 27~30, corresponding to 4 towers on the field
        if (isText)accessory.Method.TextInfo($"Eat white, prepare to step on tower", duration: 2000, false);
        if (isTTS)accessory.Method.TTS($"Eat white, prepare to step on tower");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"Eat white, prepare to step on tower");
    }
    
    [ScriptMethod(name: "Abyssal Sun Tower Stepping Potion Tip", eventType: EventTypeEnum.EnvControl, eventCondition: ["Flag:32", "Index:regex:^(2[789]|30)$"],suppress:5000)]
    public void Q40_AbyssalSunTowerSteppingPotionTip(Event @event, ScriptAccessory accessory)
    {
        // When stepping on the tower yourself, there will be StatusID 2922 bleeding, but you may not be the first to step on the tower, so use the first person who steps on the tower as a prompt
        if(!isPotions) return;
        // if (isText)accessory.Method.TextInfo("Drink potion", duration: 2000, true);
        if (isTTS)accessory.Method.TTS("Drink potion");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Drink potion");
    }
    
    [ScriptMethod(name: "Bounds of Sin (Trapping Prison) Cast TTS Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4479[78]$"])]
    public void Q40_BoundsOfSinTip(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Trapping prison");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Trapping prison");
    }
    
    [ScriptMethod(name: "Bounds of Sin (Trapping Prison Judgment Animation)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44122"])]
    public void Q40_BoundsOfSin (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Q40_BoundsOfSin";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3f);
        dp.DestoryAt = 2700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    uint _blackandwhite = 0; // Black-White Match mark record
    
    [ScriptMethod(name: "Black-White Match Mark Record", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^004(D|E)$"] ,userControl:false)]
    public void Q40_BlackWhiteMatchMarkRecord (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        _blackandwhite = 1;
        if(isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: Successfully recorded black-white match mark");
    }
    
    [ScriptMethod(name: "Black-White Match TTS Tip", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^004(D|E)$"])]
    public void Q40_BlackWhiteMatchTTSTip (Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isTTS)accessory.Method.TTS("Black-white match");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Black-white match");
    }

    [ScriptMethod(name: "Black-White Match Judgment Tip", eventType: EventTypeEnum.Director, eventCondition: ["Command:80000026", "Instance:8003EA93"],suppress: 1000)]
    public async void Q40_BlackWhiteMatchJudgmentTip (Event @event, ScriptAccessory accessory)
    {
        if(!isTTS || isEdgeTTS) return;
        // Subsequent parameters are [31~34|9|1|0], but Duck can't use them.jpg
        // The aether of light and dark neutralized...
        if (_blackandwhite == 0)return;
        await Task.Delay(1000);
        Console.Beep(2000, 200); 
        // if (isTTS)accessory.Method.TIPS($"Heavy!");
        // if (isEdgeTTS)accessory.Method.EdgeTTS($"Heavy!");
        // accessory.Method.SendChat($"/e [Black-White Match] Hit! <se.4> <se.4> <se.4> ");

        _blackandwhite = 0;
    }
    
    [ScriptMethod(name: "Blade of First Light (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^441(04|10)$"])]
    public void Q40_BladeOfFirstLight(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"BladeOfFirstLight";
        dp.Scale = new (15f, 30f);
        dp.Owner = @event.SourceId();
        dp.Offset = new Vector3 (0, 0 ,15); // The entity is originally in the middle of the line, using Straight, but considering omen effect, using Rect offset is better
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = @event.ActionId() == 44104 ? 4700 : 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Fireball (Cyclone) Preparatory Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^44(097|105)$"])]
    public void Q40_FireballPreparatory(Event @event, ScriptAccessory accessory)
    {
        bool isFastCast = @event.ActionId == 44097;
        bool isSlowCast = @event.ActionId == 44105;

        int duration = isFastCast ? 4000 : 7000;
        string timingType = isFastCast ? "early" : "late";

        if (isText) accessory.Method.TextInfo($"Line {timingType} cyclone", duration: duration, true);
        if (isTTS) accessory.Method.TTS($"Line {timingType} cyclone");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Line {timingType} cyclone");
    }
    
    [ScriptMethod(name: "Fireball (Cyclone) Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44098"])]
    public void Q40_Fireball (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Q40_Fireball";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 1800;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Chains of Condemnation (Fever) Preparatory Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^44(099|106)$"])]
    public void Q40_ChainsOfCondemnationPreparatory(Event @event, ScriptAccessory accessory)
    {
        // Main meaningless cast Fast: 44099 Source: 44100 / Slow: 44106 Source:44107, where Source has 0.7s longer cast than meaningless
        // Chains of Condemnation (Fever) StatusID: 4562, application time about 2.6s
        
        bool isFastCast = @event.ActionId == 44099;
        bool isSlowCast = @event.ActionId == 44106;
    
        int duration = isFastCast ? 4000 : 7000;
        string timingType = isFastCast ? "early" : "late";

        if (isText) accessory.Method.TextInfo($"Stop moving {timingType} for line", duration: duration, true);
        if (isTTS) accessory.Method.TTS($"Stop moving {timingType} for line");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Stop moving {timingType} for line");
    }
    
    [ScriptMethod(name: "Chains of Condemnation (Fever) Tip", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4562"])]
    public void Q40_ChainsOfCondemnation(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isTTS) accessory.Method.TTS($"Stop moving");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Stop moving");
    }
    
    [ScriptMethod(name: "Searing Chains Preparatory Tip", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0061"])]
    public void Q40_SearingChains_PreparatoryTip(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 

        if (isPotions)
        {
            var isTank = accessory.Data.MyObject?.IsTank() ?? false;
            if (isTank) return; // Do tanks need me to remind them to drink potions?
            if (isText)accessory.Method.TextInfo("Go to center for tether, prepare fire damage potion", duration: 3000, true);
            if (isTTS)accessory.Method.TTS("Go to center for tether, prepare potion");
            if (isEdgeTTS)accessory.Method.EdgeTTS("Go to center for tether, prepare potion");
        }
        else
        {
            if (isText)accessory.Method.TextInfo("Go to center for tether", duration: 2000, true);
            if (isTTS)accessory.Method.TTS("Go to center for tether");
            if (isEdgeTTS)accessory.Method.EdgeTTS("Go to center for tether");
        }
    }
    
    [ScriptMethod(name: "Searing Chains Tether Tip", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4563"])]
    public void Q40_SearingChains_TetherTip(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        // if (isText)accessory.Method.TextInfo("Break the tether", duration: 2000, true);
        if (isTTS)accessory.Method.TTS("Break the tether");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Break the tether");
    }
    
    uint _spinelash = 0; // Spinelash cast target line record
    
    [ScriptMethod(name: "Spinelash (Target Stack Tip)", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"])]
    public void Q40_SpinelashTip(Event @event, ScriptAccessory accessory)
    {
        if (HelperExtensions.GetCurrentTerritoryId() != MapIds.TheFinalVerseQuantum) return; // Deep thinking battle - The Final Verse DataId: 18670
        
        _spinelash++;
        
        if (@event.TargetId() == accessory.Data.Me)
        {
            if (isTTS)accessory.Method.TTS("Stack target");
            if (isEdgeTTS)accessory.Method.EdgeTTS("Stack target");
        }
        else
        {
            string tname = @event["TargetName"]?.ToString() ?? "unknown target";
            if (isTTS) accessory.Method.TTS($"Block gun stack point {tname}");
            if (isEdgeTTS) accessory.Method.EdgeTTS($"Block gun stack point {tname}");
        }
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"SpinelashPortent";
        dp.Scale = new (1f, 30f);
        dp.Color = accessory.Data.DefaultSafeColor.WithW(1.4f);
        
        switch (_spinelash)
        {
            case 1 :
                dp.Position = new Vector3(-613.4f, 0f, -315f); // Left
                break;
            case 2 :
                dp.Position = new Vector3(-586.6f, 0f, -315f); // Right
                break;
            case 3 :
                dp.Position = new Vector3(-600f, 0f, -315f); // Middle
                break;
        }
        
        dp.DestoryAt = 6400;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
        
        if (isDeveloper)  accessory.Method.SendChat($"/e [DEBUG] Current target count: {_spinelash}");
    }
    
    [ScriptMethod(name: "Spinelash (Line Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45119"])]
    public void Q40_Spinelash(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Spinelash";
        dp.Scale = new (8f, 60f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Vodoriga Minion Focus Reminder", eventType: EventTypeEnum.Targetable, eventCondition: ["DataId:18672", "Targetable:True"])]
    public void Q40_VodorigaMinionReminder(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo("Kill Vodoriga minion", duration: 2000, true);
        if (isTTS)accessory.Method.TTS("Kill minion");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Kill minion");
    }
    
    // P2 Shackles of Sanctity (Role Debuff) â†’ Flaming Domain (Guide Cage Tether + Suction) â†’ Guide Triple Yellow Circle â†’ Tail Combo (Tankbuster Tower + Diagonal AOE) â†’ Unholy Darkness (AOE+DOT) â†’ Tail Combo (Tankbuster Tower + Diagonal AOE)
    // â†’ Abyssal Blaze (Store Floor Fire) + Guide Triple Yellow Circle â†’ Bounds of Sin (Trapping Prison) + Black-White Match â†’ Floor Fire Judgment
    
    [ScriptMethod(name: "Shackles of Sanctity (Role Debuff) Cast Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44801"])]
    public void Q40_ShacklesOfSanctityTip(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("Role debuff positioning, prepare to refresh light/dark", duration: 4000, true);
        if (isTTS) accessory.Method.TTS($"Role debuff positioning");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Role debuff positioning");
    }
    
    [ScriptMethod(name: "Shackles of Sanctity: Healing [Healer Heat Wind]", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4564"])]
    public void Q40_ShacklesOfSanctity_Healing (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Q40_ShacklesOfSanctity_Healing";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(10f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(21f);
        dp.InnerScale = new Vector2(20.95f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 65000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Shackles of Sanctity: Abilities [DPS]", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4565"])]
    public void Q40_ShacklesOfSanctity_Abilities (Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Q40_ShacklesOfSanctity_Abilities{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(6f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(8f);
        dp.InnerScale = new Vector2(7.96f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 65000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Shackles of Sanctity Backup Destruction", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:regex:^456[45]$"],userControl: false)]
    public void ShacklesOfSanctityBackupDestruction(Event @event, ScriptAccessory accessory)
    {
        if (@event.StatusId == 4564) accessory.Method.RemoveDraw("Q40_ShacklesOfSanctity_Healing");
        if (@event.StatusId == 4565) accessory.Method.RemoveDraw($"Q40_ShacklesOfSanctity_Abilities{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Flaming Domain (Suction) Cast Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44153"])]
    public void Q40_FlamingDomainTip(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("Suction, refresh buff, tank furthest guide tether", duration: 5000, true);
        if (isTTS) accessory.Method.TTS($"Suction, refresh buff");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Suction, refresh buff");
    }
    
    [ScriptMethod(name: "Flaming Domain (Suction) Auto Anti-Knockback [Except Tank]", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44153"])]
    public void Q40_FlamingDomainAutoAntiKnockback(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank) return;

        if (AutoAntiKnockback == AutoAntiKnockbackEnum.ArmsLength) 
        { 
            accessory.Method.SendChat($"/ac Arm's Length"); 
            accessory.Method.SendChat($"/e [Kodakku]: Attempted to auto-use anti-knockback - Arm's Length");
        }
        else if (AutoAntiKnockback == AutoAntiKnockbackEnum.Surecast) 
        { 
            accessory.Method.SendChat($"/ac Surecast"); 
            accessory.Method.SendChat($"/e [Kodakku]: Attempted to auto-use anti-knockback - Surecast");
        }
        else if (AutoAntiKnockback == AutoAntiKnockbackEnum.DR && isHack) 
        { 
            accessory.Method.SendChat($"/pdr load AutoAntiKnockback"); 
            accessory.Method.SendChat($"/e [Kodakku]: Attempted to auto-enable anti-knockback - DR");
        }
        else if (AutoAntiKnockback == AutoAntiKnockbackEnum.IChing && isHack) 
        {
            accessory.Method.SendChat($"/i-ching-commander anti_knock 0 0"); 
            accessory.Method.SendChat($"/e [Kodakku]: Attempted to auto-enable anti-knockback - I-Ching");
        }
        
    }
    
    [ScriptMethod(name: "Tail Combo (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44161"])]
    public void Q40_TailCombo(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"TailCombo";
        dp.Scale = new (9f, 42f);
        dp.Offset = new Vector3(0f, 0f, 10f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 2000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Tail Combo Safe Guide", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^4415[78]$"])]
    public void Q40_TailComboSafeGuide(Event @event, ScriptAccessory accessory)
    {
        // 44157 hits top-right bottom-left top-left safe ; 44158 hits top-left bottom-right top-right safe
        
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (!isTank) return; 
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "TailComboSafeGuide";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = @event.ActionId() ==44157 ? new Vector3(-603.5f, 0f, -312f) : new Vector3(-596.5f, 0f, -312.4f);
        dp.Scale = new(0.3f);
        dp.Delay = 1500;
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Eruption (Triple Guide)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44156"])]
    public void Q40_Eruption (Event @event, ScriptAccessory accessory)
    {
        var myObject = accessory.Data.MyObject;
        if (myObject == null) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Q40_Eruption";
        if (IbcHelper.HasStatus(accessory, accessory.Data.MyObject, 0x11D4) ||
            IbcHelper.HasStatus(accessory, accessory.Data.MyObject, 0x11D5))
        {
            dp.Color = accessory.Data.DefaultDangerColor.WithW(1.6f);
        }
        else
        {
            dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        }
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Arcane Sphere Focus Reminder", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:18676"],suppress:1000)]
    public void Q40_ArcaneSphereReminder(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo("Attack Arcane Sphere", duration: 2000, true);
        if (isTTS)accessory.Method.TTS("Attack magic circle");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Attack magic circle");
    }
    
    [ScriptMethod(name: "Unholy Darkness (AOE) Cast Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^441(64|76)$"])]
    public void Q40_UnholyDarknessTip(Event @event, ScriptAccessory accessory)
    {
        // if (isText)accessory.Method.TextInfo("Bleeding AOE", duration: 6000, true);
        if (isPotions)
        {
            var isTank = accessory.Data.MyObject?.IsTank() ?? false;
            if (isTank) return; // Do tanks need me to remind them to drink potions?
            if (isTTS)accessory.Method.TTS("Bleeding AOE, drink potion");
            if (isEdgeTTS)accessory.Method.EdgeTTS("Bleeding AOE, drink potion");
        }
        else
        {
            if (isTTS)accessory.Method.TTS("Bleeding AOE");
            if (isEdgeTTS)accessory.Method.EdgeTTS("Bleeding AOE");
        }
    }
    
    // P3 Crime and Punishment (Poison Transfer) [First] Blade of First Light (Line) + Chains of Condemnation / Fireball â†’ [Second] Spinelash (Block Stack + Minion) â†’ Bounds of Sin (Trapping Prison) + Tether + Cross Fire
    // â†’ [Third] Abyssal Blaze Store Floor Fire + Black-White Match â†’ Drain Aether (Buff Detection) + Floor Fire Judgment â†’ Unholy Darkness (AOE+DOT)
    // First transfer same group â†’ Second transfer same role â†’ Third transfer same group [ T & D1 as one group; H & D2 as one group ]
    // First transfer: [after spicy wings spicy tail] poison in [center top-right] light floor, receive poison next to it ; Second transfer: same role transfer in place after line stack, others dodge ; Third transfer: direct transfer at black-white match position
    
    [ScriptMethod(name: "Crime and Punishment (Poison Transfer) Cast Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44165"])]
    public void Q40_CrimeAndPunishmentTip(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("Poison transfer phase: First eat dark", duration: 5000, true);
        if (isTTS) accessory.Method.TTS($"Eat dark, prepare to transfer poison");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Eat dark, prepare to transfer poison");
    }
    
    [ScriptMethod(name: "Sin Bearer (Poison) Target Broadcast", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4567", "Param:1"])]
    public void SinBearerTargetBroadcast(Event @event, ScriptAccessory accessory)
    {
            string tname = @event["TargetName"]?.ToString() ?? "unknown target";
            if (isTTS) accessory.Method.TTS($"Poison on {tname}");
            if (isEdgeTTS) accessory.Method.EdgeTTS($"Poison on {tname}");
    }
    
    [ScriptMethod(name: "Sin Bearer (Poison) Drawing", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4567"])]
    public async void SinBearerDrawing(Event @event, ScriptAccessory accessory)
    {
            uint layerCount = @event.StatusParam;

            var dp = accessory.Data.GetDefaultDrawProperties();

            if (layerCount == 1)
            {
                accessory.Method.RemoveDraw($"SinBearer.*"); // Remove the previous person's explosion range, change to show target small circle
                await Task.Delay(50);
                dp.Color = new Vector4(1f, 1f, 1f, 2f); // Poison target fill color
                
                var dp1 = accessory.Data.GetDefaultDrawProperties(); // Poison target outline drawing
                dp1.Name = dp.Name = $"SinBearer{layerCount}";
                dp1.Color = new Vector4(1f, 1f, 1f, 10f);
                dp1.Scale = dp.Scale = new Vector2(0.7f);
                dp1.Owner = dp.Owner = @event.TargetId();
                dp1.InnerScale = new Vector2(0.65f);
                dp1.Radian = float.Pi * 2;
                dp1.DestoryAt = dp.DestoryAt = 30000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp1);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else if (layerCount == 12)
            {
                accessory.Method.RemoveDraw($"SinBearer.*"); // Remove the target drawing small circle, change to show explosion range
                await Task.Delay(50);
                dp.Name = $"SinBearer{layerCount}";
                dp.Color = new Vector4(1f, 1f, 1f, 1f);
                dp.Owner = @event.TargetId();
                dp.Scale = new Vector2(4f);
                dp.DestoryAt = 12000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else if (layerCount >= 12 && layerCount <= 15) // TTS broadcast poison layers when about to explode
            {
                if (isTTS) accessory.Method.TTS($"{layerCount}");
                if (isEdgeTTS) accessory.Method.EdgeTTS($"{layerCount}");
            }
            else
            {
                return;
            }
    }
    
    [ScriptMethod(name: "SinBearerDestruction", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4567"],userControl: false)]
    public void SinBearerDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"SinBearer.*");
    }
    
    [ScriptMethod(name: "Crime and Punishment (Poison Transfer) Doom Dispel Reminder", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4594"])]
    public void Q40_CrimeAndPunishmentDoomDispelReminder(Event @event, ScriptAccessory accessory)
    {
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (isHealer && isText)accessory.Method.TextInfo("Dispel Doom", duration: 3300, true);
        if (isHealer && isTTS) accessory.Method.TTS($"Dispel Doom");
        if (isHealer && isEdgeTTS) accessory.Method.EdgeTTS($"Dispel Doom");
    }
    
    private const uint DarkVengeance = 0x11CF;    // 4559 = 0x11CF Dark Echo
    private const uint LightVengeance = 0x11D0;   // 4560 = 0x11D0 Light Echo
    private const uint SinBearer = 0x11D7;        // 4567 = 0x11D7 Sin Bearer [Poison]
    
    [ScriptMethod(name: "Drain Aether (Buff Detection)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4413[13]$"])]
    public void Q40_DrainAether(Event @event, ScriptAccessory accessory)
    {
        (string firstDrainAether, string secondDrainAether) = @event.ActionId switch
        {
            // 44129 => ("Eat Light", "Eat Dark"), // Short Dark
            // 44130 => ("Eat Dark", "Eat Light"), // Long Dark
            44131 => ("Eat Dark", "Eat Light"), // Short Light
            44133 => ("Eat Light", "Eat Dark"), // Long Light
            _ => ("Unknown", "Unknown")
        };
        
        var myObject = accessory.Data.MyObject;
        if (myObject == null) return;
    
        if (!IbcHelper.HasStatus(accessory, accessory.Data.MyObject, 0x11D7))
        {
            if (isText)accessory.Method.TextInfo($"First {firstDrainAether}, then {secondDrainAether}", duration: 10000, true);
            if (isTTS)accessory.Method.TTS($"First {firstDrainAether}, then {secondDrainAether}");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"First {firstDrainAether}, then {secondDrainAether}");
        }
        else
        {
            if (isText)accessory.Method.TextInfo($"Poison, always stay dark", duration: 10000, true);
            if (isTTS)accessory.Method.TTS($"Stay dark");
            if (isEdgeTTS)accessory.Method.EdgeTTS($"Stay dark");
        }
    }

    
    // P4 Fevered Flame â†’ Abyssal Blaze (Store Floor Fire)
    // 1 legitimate (Idyllshire) advantage: melee doesn't lose DPS ; 2-combined double X method advantage: melee doesn't lose DPS ; 2-combined single X method disadvantage: melee loses DPS and higher DPS requirement ; 3-combined method advantage: lower total damage
    
    [ScriptMethod(name: "Fevered Flame (Fire Phase) Cast TTS Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^441(69|70)$"])]
    public void Q40_FeveredFlameTip(Event @event, ScriptAccessory accessory)
    {
        if (isText)accessory.Method.TextInfo("Fire phase in position, refresh buff, keep drinking potions", duration: 3000, true);
        if (isTTS) accessory.Method.TTS($"Fire phase in position, refresh buff");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Fire phase in position, refresh buff");
    }
    
    private int checkPoint = 0;
    [ScriptMethod(name: "Fire Clone_Danger Zone Highlight", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3913","Param:regex:^92[123]$"])]
    public void Q40_FireCloneDangerZoneHighlight(Event @event, ScriptAccessory accessory)
    {
        if (checkPoint == 0)
        {
            _ClearFireGuid = accessory.Method.RegistFrameworkUpdateAction(ClearFireClonesFramework);
        }
        checkPoint++;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = @event.TargetId();
        dp.DestoryAt = 60000;
        switch (@event.StatusParam)
        {
            case 921:
                //accessory.Method.RemoveDraw($@".*FireCloneHighlight\[2\]{@event.TargetId}");
                //accessory.Method.RemoveDraw($@".*FireCloneHighlight\[3\]{@event.TargetId}");
                accessory.Method.RemoveDraw($@"FireCloneHighlight\[[23]\]{@event.TargetId}");
                dp.Name = $"FireCloneHighlight[1]{@event.TargetId}";
                dp.Color = accessory.Data.DefaultDangerColor.WithW(1.5f);
                dp.Scale = new Vector2(3f);
                break;
            case 922:
                //accessory.Method.RemoveDraw($@".*FireCloneHighlight\[1\]{@event.TargetId}");
                //accessory.Method.RemoveDraw($@".*FireCloneHighlight\[3\]{@event.TargetId}");
                accessory.Method.RemoveDraw($@"FireCloneHighlight\[[13]\]{@event.TargetId}");
                dp.Name = $"FireCloneHighlight[2]{@event.TargetId}";
                dp.Color = accessory.Data.DefaultDangerColor.WithW(1.2f);
                dp.Scale = new Vector2(6f);
                break;
            case 923:
                //accessory.Method.RemoveDraw($@".*FireCloneHighlight\[1\]{@event.TargetId}");
                //accessory.Method.RemoveDraw($@".*FireCloneHighlight\[2\]{@event.TargetId}");
                accessory.Method.RemoveDraw($@"FireCloneHighlight\[[12]\]{@event.TargetId}");
                dp.Name = $"FireCloneHighlight[3]{@event.TargetId}";
                dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
                dp.Scale = new Vector2(9f);
                break;
            default:
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    private string _ClearFireGuid = "";
    private void ClearFireClonesFramework()
    {
        ScriptAccessory sa = _sa;

        foreach (var obj in IbcHelper.GetByDataId(sa, 18675))
        {
            if (obj == null) continue;

            if (!ExtensionVisibleMethod.IsCharacterVisible((ICharacter)obj))
            {
                sa.Method.RemoveDraw($".*{obj.EntityId}$");
            }
        }
    }

    [ScriptMethod(name: "Framework Clear", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44100"], userControl: false)]
    public void FrameworkClear(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.ClearFrameworkUpdateAction(this);
        checkPoint = 0;
    }

    [ScriptMethod(name: "Fire Clone Danger Zone Cast Destruction", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44171"],userControl: false)]
    public void FireCloneDangerZoneCastDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"FireCloneHighlight.*{@event.SourceId}");
    }
    
    [ScriptMethod(name: "Fire Clone Danger Zone Backup Destruction", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:18675"],userControl: false)]
    public void FireCloneDangerZoneBackupDestruction(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"FireCloneHighlight.*{@event.SourceId}");
    }
    
    private int _explosionCount = 0;
    private DateTime lastExplosionTime = DateTime.MinValue;

    [ScriptMethod(name: "Self-destruct (Fire Clone Explosion) TTS Tip", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44171"])]
    public void Q40_FireClone_SelfDestruct(Event @event, ScriptAccessory accessory)
    {
        if ((DateTime.Now - lastExplosionTime).TotalSeconds > 30)
        {
            _explosionCount = 0;
            if(isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: Explosion counter reset");
        }
    
        _explosionCount++;
        lastExplosionTime = DateTime.Now;
        
        if (isText)accessory.Method.TextInfo($"{_explosionCount} explosion", duration: 6300, true);
        if (isTTS)accessory.Method.TTS($"{_explosionCount} explosion");
        if (isEdgeTTS)accessory.Method.EdgeTTS($"{_explosionCount} explosion");
        accessory.Method.SendChat($"/e [Self-destruct count]: [{_explosionCount}]");
    }
    
    #endregion
    
    #region Bottom Layer Section
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€” Bottom Layer Section (Requires corresponding plugin and permissions) â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:"])]
    public void BottomLayerSection(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "[DR] During weather sprint, fix movement speed to 1.4x", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1548"])]
    public void SprintSpeed(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1.4");
        accessory.Method.SendChat($"/e Duck: [DR] Movement speed changed: 1.4x");
        if (isTTS)accessory.Method.TTS("Movement speed changed to 1.4x");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Movement speed changed to 1.4x");
    }
    
    [ScriptMethod(name: "[DR] During Swift Sprint, restore movement speed to default", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4709"])]
    public void SwiftSprintSpeedDefault(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed -1");
        accessory.Method.SendChat($"/e Duck: [DR] Movement speed changed: default");
        if (isTTS)accessory.Method.TTS("Movement speed restored to default");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Movement speed restored to default");
    }

    [ScriptMethod(name: "[DR] When transforming into mud ball, change movement speed to 1.2x", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4708", "StackCount:54"])]
    public void AddMudPieSpeed(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1.2");
        accessory.Method.SendChat($"/e Duck: [DR] Movement speed changed: 1.2x");
        if (isTTS)accessory.Method.TTS("Movement speed changed to 1.2x");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Movement speed changed to 1.2x");
    }
    
    [ScriptMethod(name: "[DR] When mud ball cancels, restore movement speed to default", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4708", "StackCount:54"])]
    public void RemoveMudPieSpeed(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed -1");
        accessory.Method.SendChat($"/e Duck: [DR] Movement speed changed: default");
        if (isTTS)accessory.Method.TTS("Movement speed restored to default");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Movement speed restored to default");
    }
    
    [ScriptMethod(name: "[DR] When transforming into Bomb-Mother, change movement speed to 1.5x", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4708", "StackCount:55"])]
    public void AddProgenitrixSpeed(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed 1.5");
        accessory.Method.SendChat($"/e Duck: [DR] Movement speed changed: 1.5x");
        if (isTTS)accessory.Method.TTS("Movement speed changed to 1.5x");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Movement speed changed to 1.5x");
    }
    
    [ScriptMethod(name: "[DR] When Bomb-Mother cancels, restore movement speed to default", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4708", "StackCount:55"])]
    public void RemoveProgenitrixSpeed(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/pdrspeed -1");
        accessory.Method.SendChat($"/e Duck: [DR] Movement speed changed: default");
        if (isTTS)accessory.Method.TTS("Movement speed restored to default");
        if (isEdgeTTS)accessory.Method.EdgeTTS("Movement speed restored to default");
    }
    
    [ScriptMethod(name: "[IC] When transforming into mud ball, cancel underground", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4708", "StackCount:54"])]
    public void AddMudPieDepths(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/i-ching-commander y_adjust 0");
        accessory.Method.SendChat($"/e Duck: [IC] Underground canceled");
        if (isText) accessory.Method.TextInfo("Underground canceled", duration: 1300, true);
        // if (isTTS)accessory.Method.TTS("Underground canceled");
        // if (isEdgeTTS)accessory.Method.EdgeTTS("Underground canceled");
    }
    
    [ScriptMethod(name: "[IC] When mud ball cancels, auto underground", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4708", "StackCount:54"])]
    public void RemoveMudPieDepths(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
    
        // Get the description value of depth
        string depthValue = Depths.GetDescription();
        
        accessory.Method.SendChat($"/i-ching-commander y_adjust -{depthValue}");
        accessory.Method.SendChat($"/e Duck: [IC] Auto underground -{depthValue}m");
        if (isText) accessory.Method.TextInfo($"Auto underground -{depthValue}m", duration: 1300, true);
        // if (isTTS)accessory.Method.TTS("Auto underground");
        // if (isEdgeTTS)accessory.Method.EdgeTTS("Auto underground");
    }
    
    [ScriptMethod(name: "[IC] When transforming into Bomb-Mother, cancel underground", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4708", "StackCount:55"])]
    public void AddProgenitrixDepths(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.SendChat($"/i-ching-commander y_adjust 0");
        accessory.Method.SendChat($"/e Duck: [IC] Underground canceled");
        if (isText) accessory.Method.TextInfo("Underground canceled", duration: 1300, true);
        // if (isTTS)accessory.Method.TTS("Underground canceled");
        // if (isEdgeTTS)accessory.Method.EdgeTTS("Underground canceled");
    }
    
    [ScriptMethod(name: "[IC] When Bomb-Mother cancels, auto underground", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4708", "StackCount:55"])]
    public void RemoveProgenitrixDepths(Event @event, ScriptAccessory accessory)
    {
        if(!isHack) return;
        if (@event.TargetId() != accessory.Data.Me) return; 
    
        // Get the description value of depth
        string depthValue = Depths.GetDescription();
        
        accessory.Method.SendChat($"/i-ching-commander y_adjust -{depthValue}");
        accessory.Method.SendChat($"/e Duck: [IC] Auto underground -{depthValue}m");
        if (isText) accessory.Method.TextInfo($"Auto underground -{depthValue}m", duration: 1300, true);
        // if (isTTS)accessory.Method.TTS("Auto underground");
        // if (isEdgeTTS)accessory.Method.EdgeTTS("Auto underground");
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

public static class MathTools
{
    public static float DegToRad(this float deg) => (deg + 360f) % 360f / 180f * float.Pi;
    public static float RadToDeg(this float rad) => (rad + 2 * float.Pi) % (2 * float.Pi) / float.Pi * 180f;

    /// <summary>
    /// Gets the radian value of any point with respect to the center point, with direction (0, 0, 1) as 0 and (1, 0, 0) as pi/2.
    /// That is, increasing counterclockwise.
    /// </summary>
    /// <param name="point">Any point</param>
    /// <param name="center">Center point</param>
    /// <returns></returns>
    public static float GetRadian(this Vector3 point, Vector3 center)
        => MathF.Atan2(point.X - center.X, point.Z - center.Z);

    /// <summary>
    /// Gets the distance of any point with respect to the center point.
    /// </summary>
    /// <param name="point">Any point</param>
    /// <param name="center">Center point</param>
    /// <returns></returns>
    public static float GetLength(this Vector3 point, Vector3 center)
        => new Vector2(point.X - center.X, point.Z - center.Z).Length();

    /// <summary>
    /// Rotates any point counterclockwise around the center point and extends it.
    /// </summary>
    /// <param name="point">Any point</param>
    /// <param name="center">Center point</param>
    /// <param name="radian">Rotation radians</param>
    /// <param name="length">Extension length based on this point</param>
    /// <returns></returns>
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

    /// <summary>
    /// Gets which region a given angle falls into
    /// </summary>
    /// <param name="radian">Input radians</param>
    /// <param name="regionNum">Number of region divisions</param>
    /// <param name="baseRegionIdx">Initial index of the 0-degree region</param>>
    /// <param name="isDiagDiv">Whether to divide diagonally, default false</param>
    /// <param name="isCw">Whether to increase clockwise, default false</param>
    /// <returns></returns>
    public static int RadianToRegion(this float radian, int regionNum, int baseRegionIdx = 0, bool isDiagDiv = false, bool isCw = false)
    {
        var sepRad = float.Pi * 2 / regionNum;
        var inputAngle = radian * (isCw ? -1 : 1) + (isDiagDiv ? sepRad / 2 : 0);
        var rad = (inputAngle + 4 * float.Pi) % (2 * float.Pi);
        return ((int)Math.Floor(rad / sepRad) + baseRegionIdx + regionNum) % regionNum;
    }

    /// <summary>
    /// Folds the input point horizontally
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerX">Center line coordinate point</param>
    /// <returns></returns>
    public static Vector3 FoldPointHorizon(this Vector3 point, float centerX)
        => point with { X = 2 * centerX - point.X };

    /// <summary>
    /// Folds the input point vertically
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerZ">Center line coordinate point</param>
    /// <returns></returns>
    public static Vector3 FoldPointVertical(this Vector3 point, float centerZ)
        => point with { Z = 2 * centerZ - point.Z };

    /// <summary>
    /// Symmetrizes the input point about the center
    /// </summary>
    /// <param name="point">Input point</param>
    /// <param name="center">Center point</param>
    /// <returns></returns>
    public static Vector3 PointCenterSymmetry(this Vector3 point, Vector3 center)
        => point.RotateAndExtend(center, float.Pi, 0);

    /// <summary>
    /// Gets the specified digit of a number
    /// </summary>
    /// <param name="val">Given value</param>
    /// <param name="x">Corresponding digit, 1 for units</param>
    /// <returns></returns>
    public static int GetDecimalDigit(this int val, int x)
    {
        var valStr = val.ToString();
        var length = valStr.Length;
        if (x < 1 || x > length) return -1;
        var digitChar = valStr[length - x]; // Take the x-th digit from the right
        return int.Parse(digitChar.ToString());
    }
}

public enum MarkType
{
    None = -1,
    Attack1 = 0,
    Attack2 = 1,
    Attack3 = 2,
    Attack4 = 3,
    Attack5 = 4,
    Bind1 = 5,
    Bind2 = 6,
    Bind3 = 7,
    Ignore1 = 8,
    Ignore2 = 9,
    Square = 10,
    Circle = 11,
    Cross = 12,
    Triangle = 13,
    Attack6 = 14,
    Attack7 = 15,
    Attack8 = 16,
    Count = 17
}

public static class IbcHelper
{
    public static IGameObject? GetById(this ScriptAccessory sa, ulong gameObjectId)
    {
        return sa.Data.Objects.SearchById(gameObjectId);
    }

    public static IGameObject? GetMe(this ScriptAccessory sa)
    {
        return sa.Data.Objects.LocalPlayer;
    }

    public static IEnumerable<IGameObject?> GetByDataId(this ScriptAccessory sa, uint dataId)
    {
        return sa.Data.Objects.Where(x => x.DataId == dataId);
    }

    public static string GetPlayerJob(this ScriptAccessory sa, IPlayerCharacter? playerObject, bool fullName = false)
    {
        if (playerObject == null) return "None";
        return fullName ? playerObject.ClassJob.Value.Name.ToString() : playerObject.ClassJob.Value.Abbreviation.ToString();
    }

    public static float GetStatusRemainingTime(this ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return 0;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return charaStruct->GetStatusManager()->GetRemainingTime(statusIdx);
        }
    }

    public static bool HasStatus(this ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return false;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return statusIdx != -1;
        }
    }

    /// <summary>
    /// Gets the object EntityId for a specified mark index
    /// </summary>
    public static unsafe ulong GetMarkerEntityId(uint markerIndex)
    {
        var markingController = MarkingController.Instance();
        if (markingController == null) return 0;
        if (markerIndex >= 17) return 0;

        return markingController->Markers[(int)markerIndex];
    }

    /// <summary>
    /// Gets the mark on the object
    /// </summary>
    /// <returns>MarkType</returns>
    public static MarkType GetObjectMarker(IGameObject? obj)
    {
        if (obj == null || !obj.IsValid()) return MarkType.None;

        ulong targetEntityId = obj.EntityId;
            
        for (uint i = 0; i < 17; i++)
        {
            var markerEntityId = GetMarkerEntityId(i);
            if (markerEntityId == targetEntityId)
            {
                return (MarkType)i;
            }
        }

        return MarkType.None;
    }

    /// <summary>
    /// Checks if the object has a specified mark
    /// </summary>
    public static bool HasMarker(IGameObject? obj, MarkType markType)
    {
        return GetObjectMarker(obj) == markType;
    }

    /// <summary>
    /// Checks if the object has any mark
    /// </summary>
    public static bool HasAnyMarker(IGameObject? obj)
    {
        return GetObjectMarker(obj) != MarkType.None;
    }

    private static ulong GetMarkerForObject(IGameObject? obj)
    {
        if (obj == null) return 0;
        unsafe
        {
            for (uint i = 0; i < 17; i++)
            {
                var markerEntityId = GetMarkerEntityId(i);
                if (markerEntityId == obj.EntityId)
                {
                    return markerEntityId;
                }
            }
        }
        return 0;
    }

    private static MarkType GetMarkerTypeForObject(IGameObject? obj)
    {
        if (obj == null) return MarkType.None;
        unsafe
        {
            for (uint i = 0; i < 17; i++)
            {
                var markerEntityId = GetMarkerEntityId(i);
                if (markerEntityId == obj.EntityId)
                {
                    return (MarkType)i;
                }
            }
        }
        return MarkType.None;
    }

    /// <summary>
    /// Gets the name of the mark
    /// </summary>
    public static string GetMarkerName(MarkType markType)
    {
        return markType switch
        {
            MarkType.Attack1 => "Attack1",
            MarkType.Attack2 => "Attack2",
            MarkType.Attack3 => "Attack3",
            MarkType.Attack4 => "Attack4",
            MarkType.Attack5 => "Attack5",
            MarkType.Bind1 => "Stop1",
            MarkType.Bind2 => "Stop2",
            MarkType.Bind3 => "Bind3",
            MarkType.Ignore1 => "Ignore1",
            MarkType.Ignore2 => "Ignore2",
            MarkType.Square => "Square",
            MarkType.Circle => "Circle",
            MarkType.Cross => "Cross",
            MarkType.Triangle => "Triangle",
            MarkType.Attack6 => "Attack6",
            MarkType.Attack7 => "Attack7",
            MarkType.Attack8 => "Attack8",
            _ => "No Mark"
        };
    }
    
    public static float GetHitboxRadius(IGameObject obj)
    {
        if (obj == null || !obj.IsValid()) return -1;
        return obj.HitboxRadius;
    }

}

public static class HelperExtensions
{
    public static unsafe uint GetCurrentTerritoryId()
    {
        return AgentMap.Instance()->CurrentTerritoryId; // Additional map ID judgment
    }
}

#region Special Functions
public unsafe static class ExtensionVisibleMethod
{
    public static bool IsCharacterVisible(this ICharacter chr)
    {
        var v = (IntPtr)(((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)chr.Address)->GameObject.DrawObject);
        if (v == IntPtr.Zero) return false;
        return Bitmask.IsBitSet(*(byte*)(v + 136), 0);
    }

    public static class Bitmask
    {
        public static bool IsBitSet(ulong b, int pos)
        {
            return (b & (1UL << pos)) != 0;
        }

        public static void SetBit(ref ulong b, int pos)
        {
            b |= 1UL << pos;
        }

        public static void ResetBit(ref ulong b, int pos)
        {
            b &= ~(1UL << pos);
        }

        public static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static bool IsBitSet(short b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }
}
#endregion Special Functions
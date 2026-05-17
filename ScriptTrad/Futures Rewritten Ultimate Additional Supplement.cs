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

namespace FRU_DLC;

[ScriptType(name: "Futures Rewritten Ultimate Additional Supplement", territorys: [], guid: "e7e2b815-ba3f-4df9-a7e8-931aaa48e31a", version: "0.0.0.1", Author: "Linoa235")]

public class FRU_DLC
{
    const string noteStr =
        """
        v0.0.0.3:
        Futures Rewritten Ultimate Additional Supplement
        Can be used simultaneously with Lyse's drawings and Lian's patch, no major conflicts.
        Suggest turning everything off in method settings first, then enable as needed.
        Some banner alerts may conflict - banners only show the first one that appears. You can disable unnecessary ones based on timeline.
        Examples: [P4 Gaia Appearance Banner Countdown] conflicts with [P4 Edge of Oblivion AOE Alert] and Lyse's [P4 Akh Rhai Avoid Alert]. Please enable only one of the three.
        """;
    
    #region Basic Controls
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    private static List<string> _BottleGemdraught = ["Grade 3 Strength Tincture", "Grade 3 Dexterity Tincture", "Grade 3 Intelligence Tincture", "Grade 3 Mind Tincture", "Grade 2 Strength Tincture", "Grade 2 Dexterity Tincture", "Grade 2 Intelligence Tincture", "Grade 2 Mind Tincture"];
    
    public enum BottleGemdraughtEnum
    {
        None = -1,
        Grade3StrengthTincture = 0,
        Grade3DexterityTincture = 1,
        Grade3IntelligenceTincture = 2,
        Grade3MindTincture = 3,
        Grade2StrengthTincture = 4,
        Grade2DexterityTincture = 5,
        Grade2IntelligenceTincture = 6,
        Grade2MindTincture = 7,
    }
    
    private static List<string> _AkhMorn = ["Gaia", "Ryne"];
    
    public enum AkhMornEnum
    {
        None = -1,
        Gaia = 0,
        Ryne = 1,
    }
    
    [UserSetting(note: "P4 Akh Morn Stack Position")]
    public AkhMornEnum AkhMorn { get; set; } = AkhMornEnum.None;
    
    [UserSetting("P4 Second Phase Auto Anti-Knockback [Y-Shape Knockback Strategy]")]
    public bool isAutoAntiKnockback { get; set; } = false;
    
    [UserSetting("Dragoon Progression Tips")]
    public bool isDRG { get; set; } = false;
    
    [UserSetting("[Dragoon] Light Rampant Auto Lance Charge Pre-cast (Before Tower Soak)")]
    public bool isAutoLanceCharge { get; set; } = false;
    
    [UserSetting("Developer Mode")]
    public bool isDeveloper { get; set; } = false;
    
    #endregion
    
    #region Records & Transitions
    
    public enum FRU_Phase
    {
        Init,
        Fatebreaker,
        FallOfFaith,
        UsurperOfFrost,
        LightRampant,
        OracleOfDarkness,
        Les,
        DarklitDragonsong,
        CrystallizeTime,
        Pandora,
    }
    
    FRU_Phase phase = FRU_Phase.Init;
    
    public void Init(ScriptAccessory accessory) {
        phase = FRU_Phase.Init;
    }
    
    [ScriptMethod(name: "P1 Opening Transition (Record Blasting Strike)", userControl: false, eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4014[48]$"])]
    public void P1OpeningTransition(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.Init => FRU_Phase.Fatebreaker,
            _ => phase
        };
        if (isDeveloper) accessory.Method.SendChat($"/e [Tora Debug]: Phase successfully transitioned to Opening");
    }
    
    [ScriptMethod(name: "P1 Phase Transition - Fall of Faith", userControl: false, eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40140"])]
    public void P1FallOfFaithTransition(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.Fatebreaker => FRU_Phase.FallOfFaith,
            _ => phase
        };
        if (isDeveloper) accessory.Method.SendChat($"/e [Tora Debug]: Phase successfully transitioned to Lightning/Fire Line");
    }
    
    [ScriptMethod(name: "P2 Opening Transition", userControl: false, eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17823"])]
    public void P2OpeningTransition(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.FallOfFaith => FRU_Phase.UsurperOfFrost,
            _ => phase
        };
        if (isDeveloper) accessory.Method.SendChat($"/e [Tora Debug]: Phase successfully transitioned to P2");
    }
    
    [ScriptMethod(name: "P2 Phase Transition - Light Rampant", userControl: false, eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40212"])]
    public void P2LightRampantTransition(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.UsurperOfFrost => FRU_Phase.LightRampant,
            _ => phase
        };
        if (isDeveloper) accessory.Method.SendChat($"/e [Tora Debug]: Phase successfully transitioned to Light Rampant");
    }
    
    [ScriptMethod(name: "P3 Opening Transition", userControl: false, eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40226"])]
    public void P3OpeningTransition(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.LightRampant => FRU_Phase.OracleOfDarkness,
            _ => phase
        };
        if (isDeveloper) accessory.Method.SendChat($"/e [Tora Debug]: Phase successfully transitioned to P3");
    }
    
    [ScriptMethod(name: "P4 Opening Transition", userControl: false, eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40246"])]
    public void P4OpeningTransition(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.OracleOfDarkness => FRU_Phase.Les,
            _ => phase
        };
        if (isDeveloper) accessory.Method.SendChat($"/e [Tora Debug]: Phase successfully transitioned to P4");
    }
    
    [ScriptMethod(name: "P4 Phase Transition - First Phase [Darklit Dragonsong]", userControl: false, eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40301"])]
    public void P4DarklitDragonsongTransition(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.Les => FRU_Phase.DarklitDragonsong,
            _ => phase
        };
        if (isDeveloper) accessory.Method.SendChat($"/e [Tora Debug]: Phase successfully transitioned to P4 First Phase [Darklit Dragonsong]");
    }
    
    [ScriptMethod(name: "P4 Phase Transition - Second Phase [Crystallize Time]", userControl: false, eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40298"])]
    public void P4CrystallizeTimeTransition(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.DarklitDragonsong => FRU_Phase.CrystallizeTime,
            _ => phase
        };
        if (isDeveloper) accessory.Method.SendChat($"/e [Tora Debug]: Phase successfully transitioned to P4 Second Phase [Crystallize Time]");
    }
    
    [ScriptMethod(name: "P5 Phase Transition", userControl: false, eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17839"])]
    public void P5OpeningTransition(Event @event, ScriptAccessory accessory)
    {
        phase = phase switch
        {
            FRU_Phase.CrystallizeTime => FRU_Phase.Pandora,
            _ => phase
        };
        if (isDeveloper) accessory.Method.SendChat($"/e [Tora Debug]: Phase successfully transitioned to P5");
    }
    
    #endregion
    
    #region General Main Part
    
    [ScriptMethod(name: "P1 Lightning/Fire Line Queue Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40167"], suppress: 1000)]
    public void FallOfFaith(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Lightning/Fire line queue", duration: 6000, true);
        if (isTTS) accessory.Method.TTS("Lightning/Fire line queue");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Lightning/Fire line queue");
    }
    
    [ScriptMethod(name: "P2.5 Winter Storm (Large Circle) Judgment Time Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40256"])]
    public void WinterStorm(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Winter Storm{@event.SourceId()}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(7f);
        dp.DestoryAt = 3000;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Winter Storm Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:40256"], userControl: false)]
    public void WinterStormCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Winter Storm{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "P4 Gaia Appearance Banner Countdown", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40186"])]
    public async void P4AddGaia(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Gaia appearing soon, watch party buff timing", duration: 5300, true);
        await Task.Delay(3800);
        if (isTTS) accessory.Method.TTS("Gaia appearing soon");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Gaia appearing soon");
    }
    
    [ScriptMethod(name: "P4 Fragment of Fate Target Circle Drawing", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17841"])]
    public async void FragmentOfFate(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Fragment of Fate";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3.5f);
        dp.DestoryAt = 180000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "P4 Fragment of Fate Drawing Cleanup", userControl: false, eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:17841"])]
    public async void RemoveFragmentOfFate(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Fragment of Fate");
    }
    
    [ScriptMethod(name: "P4 Edge of Oblivion AOE Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40174"])]
    public void EdgeOfOblivion(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Crystal AOE", duration: 4300, false);
        if (isTTS) accessory.Method.TTS("Crystal AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Crystal AOE");
    }
    
    [ScriptMethod(name: "P4 Akh Morn Gaia Tether", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40302"])]
    public void P4TetherGaia(Event @event, ScriptAccessory accessory)
    {
        if (AkhMorn == AkhMornEnum.Gaia)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Tether Gaia";
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.TargetObject = @event.SourceId();
            dp.Scale = new(1);
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
    }
    
    [ScriptMethod(name: "P4 Akh Morn Ryne Tether", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40247"])]
    public void P4TetherRyne(Event @event, ScriptAccessory accessory)
    {
        if (AkhMorn == AkhMornEnum.Ryne)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Tether Ryne";
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.TargetObject = @event.SourceId();
            dp.Scale = new(1);
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
    }
    
    [ScriptMethod(name: "P4 Second Phase Y-Shape Knockback Alert (with Auto Anti-Knockback)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40229"])]
    public async void P4YAutoAntiKnockback(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(2500);
        
        if (isAutoAntiKnockback)
        {
            accessory.Method.SendChat($"/ac Arm's Length");
            accessory.Method.SendChat($"/ac Surecast");
            accessory.Method.SendChat($"/e [Tora Debug]: Attempted auto anti-knockback");
        }
        
        if (isText) accessory.Method.TextInfo("Anti-knockback", duration: 1500, true);
        if (isTTS) accessory.Method.TTS("Anti-knockback");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Anti-knockback");
    }
    
    [ScriptMethod(name: "P4 Final Delay 120 Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40247"])]
    public void P4Finale(Event @event, ScriptAccessory accessory)
    {
        if (phase != FRU_Phase.CrystallizeTime) return;
        if (isText) accessory.Method.TextInfo("Delay 120 burst to P5", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Delay burst");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Delay burst");
    }
    
    #endregion
    
    #region Dragoon Tools
    
    [ScriptMethod(name: "â€”â€”â€”â€” Dragoon Tools (No Real Practical Meaning) â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["DataId:1"])]
    public void DragoonTools(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "P1 Opening Hint", eventType: EventTypeEnum.Chat, eventCondition: ["Message:è·ç¦»æˆ˜æ–—å¼€å§‹è¿˜æœ‰5ç§’ï¼"])]
    public async void FatebreakerStart(Event @event, ScriptAccessory accessory)
    {
        if (!isDRG) return; 
        await Task.Delay(3000);
        if (isText) accessory.Method.TextInfo("1GCD High Jump, 2GCD Battle Litany Lance Charge", duration: 7000, false);
        if (isTTS) accessory.Method.TTS("1GCD High Jump, 2GCD Battle Litany Lance Charge");
        if (isEdgeTTS) accessory.Method.EdgeTTS("1GCD High Jump, 2GCD Battle Litany Lance Charge");
    }
    
    [ScriptMethod(name: "P1 Before Fog Dragon Ascension Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40168"])]
    public void UtopianSkyEarly(Event @event, ScriptAccessory accessory)
    {
        if (!isDRG) return; 
        if (phase != FRU_Phase.Fatebreaker) return;
        if (isText) accessory.Method.TextInfo("Pre-backflip, use enhanced Spear as final instead of Nastrond 4", duration: 10000, false);
        if (isTTS) accessory.Method.TTS("Pre-backflip, Spear finisher");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Pre-backflip, Spear finisher");
    }
    
    [ScriptMethod(name: "P1 Fog Dragon Landing Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40167"], suppress: 1000)]
    public void FatebreakerTargetable(Event @event, ScriptAccessory accessory)
    {
        if (!isDRG) return; 
        if (isText) accessory.Method.TextInfo("1GCD Feint, 2GCD High Jump, delay Lance Charge for Blue Cannon, after positionals queue", duration: 6000, true);
        if (isTTS) accessory.Method.TTS("1GCD Feint, delay burst");
        if (isEdgeTTS) accessory.Method.EdgeTTS("1GCD Feint, delay burst");
    }
    
    [ScriptMethod(name: "P2 Light Rampant Pre-Tower Lance Charge Hint (with Auto Lance Charge)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4022[01]$"])]
    public void P2LanceCharge(Event @event, ScriptAccessory accessory)
    {
        if (!isDRG) return; 
        if (phase != FRU_Phase.LightRampant) return;
        if (isAutoLanceCharge) accessory.Method.SendChat($"/ac Lance Charge");
        if (isAutoLanceCharge) accessory.Method.SendChat($"/e [Tora Debug] Dragoon Tool: Attempted auto Lance Charge");
        if (isText) accessory.Method.TextInfo("Pre-cast Lance Charge", duration: 5000, true);
        if (isTTS) accessory.Method.TTS("Pre-cast Lance Charge");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Pre-cast Lance Charge");
    }
    
    [ScriptMethod(name: "P2.5 Dragoon Timeline Cheat Sheet", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17829"])]
    public async void P25DRGTimeLine(Event @event, ScriptAccessory accessory)
    {
        if (!isDRG) return; 
        if (isText) accessory.Method.TextInfo("[1GCD] Battle Litany Potion [2GCD] High Jump [3GCD] Sprint Lance Charge\n[4GCD] Geirskogul Nastrond [5GCD] Backflip for large circle [6GCD] Blue Cannon ready weave\n[7GCD] Mirage Dive + Stardiver", duration: 17500, false);
        
        await Task.Delay(8200);
        if (isTTS) accessory.Method.TTS("Backflip to guide large circle");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Backflip to guide large circle");
    }
    
    [ScriptMethod(name: "P4 Opening Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17841"])]
    public async void P4AddFragmentOfFate(Event @event, ScriptAccessory accessory)
    {
        if (!isDRG) return; 
        if (isText) accessory.Method.TextInfo("After dodging Akh Rhai, 1GCD Potion + Lance Charge\nUse raid buffs when Gaia appears", duration: 6000, false);
        if (isTTS) accessory.Method.TTS("Watch burst timing");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Watch burst timing");
    }
    
    [ScriptMethod(name: "P4 Akh Rhai Lance Charge Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40237"])]
    public async void P4AkhRhai(Event @event, ScriptAccessory accessory)
    {
        if (!isDRG) return; 
        if (isTTS) accessory.Method.TTS("Potion plus Lance Charge");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Potion plus Lance Charge");
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
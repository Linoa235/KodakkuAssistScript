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

namespace the_Whorleater_Extreme;

[ScriptType(guid: "d78d10f8-038a-4e6a-8eaa-5d4e10593087", name: "Leviathan Extreme", territorys: [359],
    version: "0.0.0.4", author: "Linoa235", note: noteStr)]

public class the_Whorleater_Extreme
{
    const string noteStr =
        """
        v0.0.0.3:
        LV50 Leviathan Extreme Initial Drawing
        """;
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [UserSetting("[Dev] Debug Mode")]
    public bool isDebug { get; set; } = false;
    
    #region Records & Phase Transitions
    uint Dive = 0;
    uint Phase = 0;
    
    public void Init(ScriptAccessory accessory) {
        Dive = 0;
        Phase = 0;
    }
    
    [ScriptMethod(name: "Phase Transition: P1", eventType: EventTypeEnum.Chat, userControl: false, eventCondition: ["Type:NPCDialogueAnnouncements",
        "Message:regex:^(è¢«å…‰æ˜Žæ‰€æ±¡æŸ“çš„äººä»¬.*|You trespass upon my domain.*|æˆ‘ãŒæµ·åŸŸã‚’ä¾µã™ã‹.*)$"])]
    public void PhaseTransition1(Event @event, ScriptAccessory accessory)
    {
        Phase = 1;
    }
    
    [ScriptMethod(name: "Phase Transition: P2", eventType: EventTypeEnum.Chat, userControl: false, eventCondition: ["Type:NPCDialogueAnnouncements",
        "Message:regex:^(è“è‰²æ¸…å‡€çš„å¤§æµ·ä¹‹æ°´å°†ä¼šæ‰«åŽ»ä¸€åˆ‡æ±¡ç§½.*|Drink deep of the sea's bitter draught.*|æ¸…æµ„ãªã‚‹é’ãæ°´ã‚’ä»¥ã¦ã€ç©¢ã‚Œæ¸…ã‚ã‚“.*)$"])]
    public void PhaseTransition2(Event @event, ScriptAccessory accessory)
    {
        Phase = 2;
    }
    
    [ScriptMethod(name: "Phase Transition: P3", eventType: EventTypeEnum.Chat, userControl: false, eventCondition: ["Type:NPCDialogueAnnouncements",
        "Message:regex:^(ç«Ÿç„¶å¯¹æˆ‘å–å¼„å°ä¼Žä¿©.*|You challenge me with trickery.*|ã¬ã…ã€å°ç´°å·¥ã‚’å¼„ã—ãŸã‹.*)$"])]
    public void PhaseTransition3(Event @event, ScriptAccessory accessory)
    {
        Phase = 3;
    }
    
    [ScriptMethod(name: "Dive Record", eventType: EventTypeEnum.Targetable, eventCondition: ["SourceName:regex:^(åˆ©ç»´äºšæ¡‘|Leviathan|ãƒªãƒ´ã‚¡ã‚¤ã‚¢ã‚µãƒ³)$", "Targetable:False"], userControl: false)]
    public void DiveRecord(Event @event, ScriptAccessory accessory)
    {
        Dive = 1;
    }
    
    [ScriptMethod(name: "Dive Cancel", eventType: EventTypeEnum.Targetable, eventCondition: ["SourceName:regex:^(åˆ©ç»´äºšæ¡‘|Leviathan|ãƒªãƒ´ã‚¡ã‚¤ã‚¢ã‚µãƒ³)$", "Targetable:True"], userControl: false)]
    public void DiveCancel(Event @event, ScriptAccessory accessory)
    {
        Dive = 0;
    }
    #endregion
    
    [ScriptMethod(name: "Opening Hint", eventType: EventTypeEnum.Director, eventCondition: ["Command:40000001"])]
    public async void OpeningHint(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        var isDps = accessory.Data.MyObject?.IsDps() ?? false;
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;

        if (isTank && isText) accessory.Method.TextInfo("Difficulty: â˜…â˜†, leave if not confident\nT: MT pulls head, ST pulls tail. ST also pulls adds, stun Wave-tongue Sahagin (immune below 60%)", duration: 10000, true);
        if (isDps && isText) accessory.Method.TextInfo("Difficulty: â˜†\nDPS: Attack yellow orbs, ignore blue orbs. Prioritize Wave-tongue Sahagin adds, avoid tank's auto-attacks", duration: 10000, true);
        if (isHealer && isText) accessory.Method.TextInfo("Difficulty: â˜…â˜…, leave if not confident\nH: Let fairy and Regen heal ST. Heal party at staggered times, avoid Water Mirror debuff if possible", duration: 10000, true);
        accessory.Method.SendChat("/e â€”â€”â€”â€”Cheat Sheetâ€”â€”â€”â€”\nT: MT pulls head, ST pulls tail. ST also pulls adds, stun Wave-tongue Sahagin (immune below 60%)\nST kites blue orb, after one slam, move away from party and use heavy mitigation to explode\nDPS: Attack yellow orbs, prioritize Wave-tongue Sahagin adds, avoid tank's auto-attacks\nH: Let fairy and Regen heal ST. Heal party at staggered times, avoid Water Mirror debuff if possible");
    }
    
    [ScriptMethod(name: "Veil of the Whorl Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2165"])]
    public void VeilOfTheWhorl(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Casters hit head, physical hit tail\nRDM, BLU, DRK pay extra attention to your skills!\nTail has positionals!", duration: 5000, true);
    }
    
    [ScriptMethod(name: "Wave-tongue Sahagin Kill Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:2807"])]
    public void WaveTongueSahaginHint(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Priority kill <Wave-tongue Sahagin>", duration: 2500, true);
    }
    
    [ScriptMethod(name: "Wave-tongue Sahagin_Panic Storm", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1865"])]
    public void PanicStorm(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS("Don't step into panic circle");
        if (isEdgeTTS) accessory.Method.EdgeTTS("Don't step into panic circle");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Panic Storm";
        dp.Color = new Vector4(1f, 0f, 1f, 1.5f);
        dp.Position = @event.EffectPosition();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Wave-tongue Sahagin_Panic Baptism", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:1866"])]
    public void PanicBaptism(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Stun <Wave-tongue Sahagin>", duration: 3200, true);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Panic Baptism";
        dp.Color = new Vector4(1f, 0f, 1f, 2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.5f);
        dp.DestoryAt = 3200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Dive Hint", eventType: EventTypeEnum.Targetable, eventCondition: ["SourceName:regex:^(åˆ©ç»´äºšæ¡‘|Leviathan|ãƒªãƒ´ã‚¡ã‚¤ã‚¢ã‚µãƒ³)$", "Targetable:False"])]
    public void DiveHint(Event @event, ScriptAccessory accessory)
    {
        if (isText && Phase == 1) accessory.Method.TextInfo("Go to opposite side of water spout for knockback", duration: 5000, true);
        if (isText && Phase != 1) accessory.Method.TextInfo("Stack in middle, dodge north/south dives, stay away from side water spout knockback", duration: 5000, true);
    }
    
    [ScriptMethod(name: "Spinning Dive Divebomb", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:2804"])]
    public void SpinningDive(Event @event, ScriptAccessory accessory)
    {
        if (Dive != 1) return;  
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Spinning Dive";
        dp.Scale = new (16, 40f);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
    
    [ScriptMethod(name: "Tidal Wave (Knockback)", eventType: EventTypeEnum.Chat, eventCondition: ["Type:NPCDialogueAnnouncements",
        "Message:regex:^(ç«Ÿç„¶å¯¹æˆ‘å–å¼„å°ä¼Žä¿©.*|You challenge me with trickery.*|ã¬ã…ã€å°ç´°å·¥ã‚’å¼„ã—ãŸã‹.*)$"])]
    public void TidalWave(Event @event, ScriptAccessory accessory)
    {
        foreach (var item in accessory.Data.Objects.GetByDataId(2802))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Tidal Wave";
            dp.Scale = new (40f, 30f);
            dp.Owner = item.EntityId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
        }
    }
    
    [ScriptMethod(name: "Tidal Wave Foam Hint", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:2810"])]
    public void TidalWaveFoam(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank && isText) accessory.Method.TextInfo("ST pick up blue foam, avoid crowd, kite for about half a minute, then use mitigation and explode away from party", duration: 34500, false);
        if (isTTS) accessory.Method.TTS("ST pull blue orb, it explodes after about 30 seconds, party stay away");
        if (isEdgeTTS) accessory.Method.EdgeTTS("ST pull blue orb, it explodes after about 30 seconds, party stay away");
    }
    
    #region Drawing Cleanup
    [ScriptMethod(name: "Slam Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2090"], userControl: false)]
    public void SlamCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Tidal Wave");
    }
    
    [ScriptMethod(name: "Panic Baptism Cleanup", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:1866"], userControl: false)]
    public void PanicBaptismCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Panic Baptism");
    }
    
    [ScriptMethod(name: "Spinning Dive Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:2186"], userControl: false)]
    public void SpinningDiveCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw("Spinning Dive");
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
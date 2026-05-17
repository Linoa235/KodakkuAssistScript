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

namespace The_Baldesion_Arsenal_Eureka;

[ScriptType(name: "The Forbidden Land, Eureka - The Baldesion Arsenal", territorys: [], guid: "aaab3962-b4c2-40db-b250-f5fc52238eaa", version: "0.0.0.1", Author: "Linoa235")]

public class The_Baldesion_Arsenal
{
    const string noteStr =
        """
        v0.0.0.3:
        The Forbidden Land, Eureka - The Baldesion Arsenal Initial Drawing
        [WIP] Not finished, not tested, WILL be electric. If you're a chad, record a video for me.
        """;
    
    #region Basic Controls
    
    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Disable TTS conflicting with ACT")]
    public bool isACT { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [UserSetting("Developer Mode")]
    public bool isDeveloper { get; set; } = false;
    
    #endregion
    
    #region Global Settings
    
    uint AstralEssence = 0;
    uint UmbralEssence = 0;
    uint ball = 0;
    uint Cube = 0;
    uint Pyramid = 0;
    uint Stellation = 0;
    
    public void Init(ScriptAccessory accessory) {
        AstralEssence = 0;
        UmbralEssence = 0;
        ball = 0;
        Cube = 0;
        Pyramid = 0;
        Stellation = 0;
    }
    
    [ScriptMethod(name: "Head Strike & Interject Interrupt Cleanup", eventType: EventTypeEnum.ActionEffect, userControl: false, 
     eventCondition: ["ActionId:regex:^(7538|7551)$"])]
    public void HeadStrikeInterjectInterruptCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Baldesion Sprite_Banish{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Stun Interrupt Cleanup", eventType: EventTypeEnum.ActionEffect, userControl: false, 
        eventCondition: ["ActionId:regex:^(139|7540|7863)$"])]
    public void StunInterruptCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Baldesion Sprite_Banish{@event.SourceId()}");
    }
    
    [ScriptMethod(name: "Sleep Interrupt Cleanup", eventType: EventTypeEnum.ActionEffect, userControl: false, 
        eventCondition: ["ActionId:regex:^(25880|16560)$"])]
    public void SleepInterruptCleanup(Event @event, ScriptAccessory accessory)
    {
        // accessory.Method.RemoveDraw($"Not yet{@event.TargetId()}");
    }
    
    [ScriptMethod(name: "Anti-Knockback Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7548|7559)$"], userControl: false)]
    public void AntiKnockbackCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        accessory.Method.RemoveDraw(".*Knockback.*");
    }
    
    #endregion
    
    #region Trash Hints
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”  Trash Basic Hints â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:0"])]
    public void TrashBasicHints(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Baldesion Wise Toad_Toy Hammer Stillness Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15363"])]
    public void BaldesionWiseToad_ToyHammer(Event @event, ScriptAccessory accessory)
    { 
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (isHealer || isDeveloper)
        {
            if (isText) accessory.Method.TextInfo("Stillness <Baldesion Wise Toad>", duration: 3000, true);
            if (isACT) return; 
            if (isTTS) accessory.Method.TTS("Stillness <Baldesion Wise Toad>");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Stillness <Baldesion Wise Toad>");
        }
    }
    
    [ScriptMethod(name: "Baldesion Sprite_Banish Interrupt Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15383"])]
    public void BaldesionSprite_BanishHint(Event @event, ScriptAccessory accessory)
    { 
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank || isDeveloper)
        {
            if (isText) accessory.Method.TextInfo("Interrupt <Baldesion Sprite>", duration: 5300, true);
            if (isACT) return; 
            if (isTTS) accessory.Method.TTS("Interrupt <Baldesion Sprite>");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Interrupt <Baldesion Sprite>");
        }
    }
    
    [ScriptMethod(name: "Baldesion Centaur_Berserk Stillness Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15358"])]
    public void BaldesionCentaur_Berserk(Event @event, ScriptAccessory accessory)
    { 
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (isHealer || isDeveloper)
        {
            if (isText) accessory.Method.TextInfo("Stillness <Baldesion Centaur>", duration: 3000, true);
            if (isACT) return; 
            if (isTTS) accessory.Method.TTS("Stillness <Centaur>");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Stillness <Centaur>");
        }
    }
    
    [ScriptMethod(name: "Baldesion Kalkulibrena_Farce Stillness Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15368"])]
    public void BaldesionKalkulibrena_Farce(Event @event, ScriptAccessory accessory)
    { 
        var isHealer = accessory.Data.MyObject?.IsHealer() ?? false;
        if (isHealer || isDeveloper)
        {
            if (isText) accessory.Method.TextInfo("Stillness <Baldesion Kalkulibrena>", duration: 3000, true);
            if (isACT) return; 
            if (isTTS) accessory.Method.TTS("Stillness <Kalkulibrena>");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Stillness <Kalkulibrena>");
        }
    }
    
    [ScriptMethod(name: "Baldesion Grimoire_Silence & Paralysis Interrupt Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^153(21|76)$"])]
    public void BaldesionGrimoire_SilenceParalysis(Event @event, ScriptAccessory accessory)
    { 
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank || isDeveloper)
        {
            if (isText) accessory.Method.TextInfo("Interrupt <Baldesion Grimoire>", duration: 4300, true);
            if (isACT) return; 
            if (isTTS) accessory.Method.TTS("Interrupt <Baldesion Grimoire>");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Interrupt <Baldesion Grimoire>");
        }
    }
    
    [ScriptMethod(name: "Baldesion Scholar Strix_Dim Chapter Interrupt Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15373"])]
    public void BaldesionScholarStrix_DimChapter(Event @event, ScriptAccessory accessory)
    { 
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank || isDeveloper)
        {
            if (isText) accessory.Method.TextInfo("Interrupt <Baldesion Scholar Strix>", duration: 2300, true);
            if (isACT) return; 
            if (isTTS) accessory.Method.TTS("Interrupt <Scholar Strix>");
            if (isEdgeTTS) accessory.Method.EdgeTTS("Interrupt <Scholar Strix>");
        }
    }
    
    #endregion
    
    #region Trash Drawing
    
    [ScriptMethod(name: "â€”â€”â€”â€”â€”â€”  Skill Drawing â€”â€”â€”â€”â€”â€”", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:0"])]
    public void TrashSkillDrawing(Event @event, ScriptAccessory accessory) { }
    
    [ScriptMethod(name: "Baldesion Byblos_Tail Smash (Tail Swipe)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15356"])]
    public void BaldesionByblos_TailSmash(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "Baldesion Byblos_Tail Smash";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(12);
        dp.Radian = 90f.DegToRad();
        dp.Rotation = 180f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Baldesion Sprite_Banish (Marker Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15383"])]
    public void BaldesionSprite_BanishDrawing(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Baldesion Sprite_Banish{@event.SourceId()}";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Baldesion Kalkulibrena_Eye of Fear (Petrify Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15366"])]
    public void BaldesionKalkulibrena_EyeOfFear(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "Baldesion Kalkulibrena_Eye of Fear";
        dp.Color = new Vector4(1f, 0f, 1f, 0.6f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(42.2f);
        dp.Radian = 120f.DegToRad();
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
    }
    
    [ScriptMethod(name: "Baldesion Fan_Ancient Storm (Knockback Tether)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15372"])]
    public void BaldesionFan_AncientStorm(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Approach <Baldesion Fan> knockback", duration: 4300, true);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Ancient Storm Knockback";
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = @event.SourceId();
        dp.Scale = new(1);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    #endregion
    
    #region BOSS1 At / Oven
    
    [ScriptMethod(name: "BOSS1_At_Oven_Magic of Fire (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^1465[01]$"])]
    public void MagicOfFire(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Magic of Fire";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10f);
        dp.DestoryAt = 4700;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS1_At_Oven_Consolidated Magick_Blood Magic (Loneliness Hint)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1753"])]
    public void ConsolidatedMagick_BloodMagic(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return; 
        if (isText) accessory.Method.TextInfo("Loneliness stack", duration: 2300, true);
        if (!isACT && isTTS) accessory.Method.TTS("Loneliness stack");
        if (!isACT && isEdgeTTS) accessory.Method.TTS("Loneliness stack");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Consolidated Magick_Blood Magic";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(0.5f);
        dp.DestoryAt = 2300;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion

    #region BOSS2 Raiden
    [ScriptMethod(name: "BOSS2_Raiden_Ame-no-Nuboko (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14441"])]
    public void AmeNoNuboko(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Ame-no-Nuboko";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(25f);
        dp.DestoryAt = 7200;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS2_Raiden_Round Zantetsuken (Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14442"])]
    public void RoundZantetsuken(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Round Zantetsuken";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(60f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "BOSS2_Raiden_Flat Zantetsuken (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^1444[34]$"])]
    public void FlatZantetsuken(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Flat Zantetsuken";
        dp.Scale = new(70, 39);
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 6200;
        
        switch (@event.ActionId())
        {
            case 14444:
                dp.Rotation = 90f.DegToRad();
                dp.Offset = new Vector3(3.5f, 0, 0);
                break;
            case 14443:
                dp.Rotation = -90f.DegToRad();
                dp.Offset = new Vector3(-3.5f, 0, 0);
                break;
        }
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "BOSS2_Raiden_Skull Cracker (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14460"])]
    public void SkullCracker(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Skull Cracker";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(10.4f);
        dp.DestoryAt = 4200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region BOSS3 Absolute Virtue
    
    [ScriptMethod(name: "Astral/Umbral Essence Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^171[01]$"], userControl: false)]
    public void AstralUmbralEssenceRecord(Event @event, ScriptAccessory accessory) 
    {
        switch (@event.StatusID())
        {
            case 1710:
                AstralEssence = 1;
                if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: Recorded <Astral Essence>");
                break;
            case 1711:
                UmbralEssence = 1;
                if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: Recorded <Umbral Essence>");
                break;
        }
    }
    
    [ScriptMethod(name: "BOSS3_Absolute Virtue_Polarity Fluctuation (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^1422[0-3]$"])]
    public void PolarityFluctuation(Event @event, ScriptAccessory accessory)
    { 
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Polarity Fluctuation";
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 4200;
        switch (@event.ActionId())
        {
            case 14220:
                dp.Color = new Vector4(1f, 1f, 1f, 1f);
                break;
            case 14221:
                dp.Color = new Vector4(1f, 1f, 1f, 1f);
                break;
            case 14222:
                dp.Color = new Vector4(0f, 0f, 0f, 1f);
                break;
            case 14223:
                dp.Color = new Vector4(0f, 0f, 0f, 1f);
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_Absolute Virtue_Light/Dark Aurora (Enhanced Half-Room)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^142(17|18|30|31)$"])]
    public void HalfRoomAurora(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Half-Room Aurora";
        dp.Scale = new(50, 30);
        dp.Owner = @event.SourceId();
        switch (@event.ActionId())
        {
            case 14217:
                dp.Color = new Vector4(1f, 1f, 1f, 1f);
                dp.DestoryAt = 2700;
                break;
            case 14218:
                dp.Color = new Vector4(0f, 0f, 0f, 1f);
                dp.DestoryAt = 2700;
                break;
            case 14230:
                dp.Color = new Vector4(1f, 1f, 1f, 1f);
                dp.DestoryAt = 4700;
                break;
            case 14231:
                dp.Color = new Vector4(0f, 0f, 0f, 1f);
                dp.DestoryAt = 4700;
                break;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    [ScriptMethod(name: "BOSS3_Absolute Virtue_Aurora Wind (Circle Tankbuster)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14234"])]
    public void AuroraWind(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Aurora Wind";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId;
        dp.CentreResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.CentreOrderIndex = 1;
        dp.Scale = new Vector2(5);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS3_Relative Virtue_Shockwave (12 o'clock search)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:14229"])]
    public void RelativeShockwave(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Relative Shockwave";
        dp.Color = new Vector4(1f, 0f, 0f, 1f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(3);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #endregion
    
    #region BOSS4 Proto-Ozma
    
    [ScriptMethod(name: "BOSS4_Proto-Ozma_Form Change Reset", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:"])]
    public async void FormChangeReset(Event @event, ScriptAccessory accessory)
    {
        ball = 0;
        Cube = 0;
        Pyramid = 0;
        Stellation = 0;
        if (isDeveloper) accessory.Method.SendChat($"/e [DEBUG]: Form change reset");
    }
    
    [ScriptMethod(name: "BOSS4_Proto-Ozma_Sphere Form (Black Hole)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:"])]
    public void ProtoOzma_SphereForm(Event @event, ScriptAccessory accessory)
    {
    }
    
    [ScriptMethod(name: "BOSS4_Proto-Ozma_Cube Form (Donut)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1070"])]
    public void ProtoOzma_CubeForm(Event @event, ScriptAccessory accessory)
    {
        Cube = 1;
        if (isText) accessory.Method.TextInfo("<Cube> Auto: Line to main tank", duration: 10000, true);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Bright Star";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(38f);
        dp.InnerScale = new Vector2(32f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "BOSS4_Proto-Ozma_Pyramid Form (Line)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1071"])]
    public void ProtoOzma_PyramidForm(Event @event, ScriptAccessory accessory)
    {
        Pyramid = 0;
        if (isText) accessory.Method.TextInfo("<Pyramid> Auto: Farthest circular AOE (with bleed)", duration: 10000, true);
        
        for (int axisRotation = 0; axisRotation < 360; axisRotation += 120)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Shrinking Ray";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(5.5f, 40f);
            dp.Owner = @event.SourceId();
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp); 
        }
    }
    
    [ScriptMethod(name: "BOSS4_Proto-Ozma_Stellation Form (Chariot)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1744"])]
    public void ProtoOzma_StellationForm(Event @event, ScriptAccessory accessory)
    {
        Stellation = 1;
        if (isText) accessory.Method.TextInfo("<Stellation> Auto: Circular stack (random marker)", duration: 10000, true);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Morning Star";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(27f);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "BOSS4_Proto-Ozma_Holy (Knockback Prediction)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17394"])]
    public void Holy(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Approach knockback", duration: 3300, true);
        if (!isACT && isTTS) accessory.Method.TTS("Approach knockback");
        if (!isACT && isEdgeTTS) accessory.Method.TTS("Approach knockback");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Holy Knockback Prediction";
        dp.Scale = new(1f, 3f);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "BOSS4_Proto-Ozma_Meteor (Knockback Prediction)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17394"])]
    public void Meteor(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo("Approach knockback", duration: 4300, true);
        if (!isACT && isTTS) accessory.Method.TTS("Approach knockback");
        if (!isACT && isEdgeTTS) accessory.Method.TTS("Approach knockback");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Meteor Knockback Prediction";
        dp.Scale = new(1f, 8f);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = @event.SourceId();
        dp.Rotation = float.Pi;
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "BOSS4_Proto-Ozma_Meteor (Knockback Source)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:17394"])]
    public void MeteorKnockbackSource(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Meteor Knockback Source";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(0.1f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "BOSS4_Proto-Ozma_Acceleration Bomb Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1072"])]
    public async void AccelerationBomb(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        await Task.Delay(13500);
        accessory.Method.TextInfo("Stop moving", duration: 800, true);
        accessory.Method.TTS("Stop moving");
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

    public static uint StatusID(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StatusID"]);
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

public static class IbcHelper
{
    public static bool HasStatus(this IBattleChara chara, uint statusId)
    {
        return chara.StatusList.Any(x => x.StatusId == statusId);
    }

    public static bool HasStatus(this IBattleChara chara, uint[] statusIds)
    {
        return chara.StatusList.Any(x => statusIds.Contains(x.StatusId));
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
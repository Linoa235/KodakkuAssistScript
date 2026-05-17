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

namespace TetoraKodakkuScript._00_Other;

[ScriptType(name: "Action Area", territorys: [], $120c9181d-055c-4fdd-832b-69bf749fc5b1", version: "0.0.0.1", Author: "Linoa235", guid: "bbc248d1-51a0-460b-aada-a9fc25c41cad")]

public class ActionArea
{
    const string noteStr =
        """
        v0.0.0.1:
        [Test Version] Won't be updated for a while due to laziness, use as is for now.
        Action area drawing, works in all scenarios.
        Set appropriate brightness first and test with any skill.
        Displacement skill prediction triggered by macros, e.g., [/e 15m] [/e 10mF] [/e 15mB]
        """;
    
    #region User Controls
    
    [UserSetting("Show Displacement Skill Prediction Distance Circle")]
    public bool IsMoveActionsCircle { get; set; } = true;
    
    [UserSetting("Displacement Skill Prediction Display Time (ms)")]
    public int MoveActionsTime { get; set; } = 10000;
    
    [UserSetting("Displacement Skill Prediction Color")]
    public ScriptColor MoveActionsColor { get; set; } = new() { V4 = new(0f, 1f, 1f, 1f) };
    
    [UserSetting("Persistent AOE Drawing Master Toggle")]
    public bool IsPersistentAoEs { get; set; } = true;
    
    [UserSetting("Persistent AOE Color")]
    public ScriptColor PersistentAoEsColor { get; set; } = new() { V4 = new(0f, 1f, 1f, 2f) };
    
    [UserSetting("Persistent AOE Outline Brightness (recommend 10+)")]
    public float PersistentOutlineBrightness { get; set; } = 15;
    
    [UserSetting("Persistent AOE Fill Brightness (recommend less than 1)")]
    public float PersistentFillBrightness { get; set; } = 0.2f;
    
    [UserSetting("Other AOE Color")]
    public ScriptColor ActionAoEsColor { get; set; } = new() { V4 = new(0f, 1f, 0f, 1f) };
    
    [UserSetting("Other AOE Outline Brightness (recommend 10+)")]
    public float ActionOutlineBrightness { get; set; } = 15;
    
    [UserSetting("Other AOE Fill Brightness (recommend less than 1)")]
    public float ActionFillBrightness { get; set; } = 0.2f;
    
    [UserSetting("Select Displacement Prediction or Area Outline Drawing Type")]
    public BlendModeEnum BlendMode { get; set; } = BlendModeEnum.Default;
    
    private static List<string> _blendMode = ["Imgui", "VFX"];
    
    public enum BlendModeEnum
    {
        Default = 0,
        Imgui = 1,
        VFX = 2,
    }
    
    #endregion

    [ScriptMethod(name: "Displacement Skill Prediction Circle", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^1[05]m[FB]?$"])]
    public void DisplacementSkillPredictionCircle(Event @event, ScriptAccessory accessory)
    {
        if (!IsMoveActionsCircle) return;
        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = "15m Circle";
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(14.95f);
        dp.Owner = accessory.Data.Me;
        dp.Color = MoveActionsColor.V4.WithW(10f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = MoveActionsTime;
        accessory.Method.SendDraw((DrawModeEnum)BlendMode, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Displacement Skill Prediction Line - Forward", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^1[05]mF?$"])]
    public void DisplacementSkillPredictionLineForward(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "15m Forward";
        dp.Scale = new(0.5f, 15f);
        dp.Owner = accessory.Data.Me;
        dp.Color = MoveActionsColor.V4.WithW(2f);
        dp.DestoryAt = MoveActionsTime;
        dp.Rotation = float.Pi * 2;
        accessory.Method.SendDraw((DrawModeEnum)BlendMode, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Displacement Skill Prediction Line - Backward", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^1[05]mB?$"])]
    public void DisplacementSkillPredictionLineBackward(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "15m Backward";
        dp.Scale = new(0.5f, 15f);
        dp.Owner = accessory.Data.Me;
        dp.Color = MoveActionsColor.V4.WithW(2f);
        dp.DestoryAt = MoveActionsTime;
        dp.Rotation = float.Pi;
        accessory.Method.SendDraw((DrawModeEnum)BlendMode, DrawTypeEnum.Displacement, dp);
    }
    
    [ScriptMethod(name: "Displacement Skill Prediction Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(94|2440[12])$"], userControl: false)]
    public void DisplacementSkillPredictionCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return;
        switch (@event.ActionId())
        {
            case 94:
                accessory.Method.RemoveDraw($"15m.*");
                break;
            case 16010:
                accessory.Method.RemoveDraw($"10m.*");
                break;
            case 24401:
                accessory.Method.RemoveDraw($"15m.*");
                break;
            case 24402:
                accessory.Method.RemoveDraw($"15m.*");
                break;
            case 34684:
                accessory.Method.RemoveDraw($"15m.*");
                break;
            case 37008:
                accessory.Method.RemoveDraw($"15m.*");
                break;
        }
    }
    
    [ScriptMethod(name: "[Outline] Earthly Star", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:17215"])]
    public void EarthlyStarOutline(Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Earthly Star Outline";
        dp.Color = PersistentAoEsColor.V4.WithW(PersistentOutlineBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.InnerScale = new Vector2(4.98f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 720000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[Fill] Earthly Star", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:17215"])]
    public void EarthlyStarFill(Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Earthly Star Fill";
        dp.Color = PersistentAoEsColor.V4.WithW(PersistentFillBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 720000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Faerie Disappear Auto Cleanup Earthly Star", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:1008"])]
    public void FaerieDisappearAutoCleanupEarthlyStar(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Earthly Star.*");
    }
    
    [ScriptMethod(name: "Persistent Skill Cleanup", eventType: EventTypeEnum.CombatChanged, eventCondition: ["InCombat:False"], userControl: false)]
    public void PersistentSkillCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Earthly Star.*");
    }
    
    [ScriptMethod(name: "[Outline] Standard Step", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1818"])]
    public void StandardStepOutline(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Standard Step Outline";
        dp.Color = ActionAoEsColor.V4.WithW(ActionOutlineBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(14.96f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 15000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[Fill] Standard Step", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1818"])]
    public void StandardStepFill(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Standard Step Fill";
        dp.Color = ActionAoEsColor.V4.WithW(ActionFillBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 15000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[Outline] Finishing Move", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3868"])]
    public void FinishingMoveOutline(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Finishing Move Outline";
        dp.Color = ActionAoEsColor.V4.WithW(ActionOutlineBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(14.96f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[Fill] Finishing Move", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3868"])]
    public void FinishingMoveFill(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Finishing Move Fill";
        dp.Color = ActionAoEsColor.V4.WithW(ActionFillBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[Outline] Technical Step", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1819"])]
    public void TechnicalStepOutline(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Technical Step Outline";
        dp.Color = ActionAoEsColor.V4.WithW(ActionOutlineBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(14.96f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 15000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[Fill] Technical Step", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1819"])]
    public void TechnicalStepFill(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Technical Step Fill";
        dp.Color = ActionAoEsColor.V4.WithW(ActionFillBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 15000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "[Outline] Tillana", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2698"])]
    public void TillanaOutline(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Tillana Outline";
        dp.Color = ActionAoEsColor.V4.WithW(ActionOutlineBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.InnerScale = new Vector2(14.96f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "[Fill] Tillana", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2698"])]
    public void TillanaFill(Event @event, ScriptAccessory accessory)
    {
        if (@event.TargetId() != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Tillana Fill";
        dp.Color = ActionAoEsColor.V4.WithW(ActionFillBrightness);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(15f);
        dp.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Dancer Skill Cleanup", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:regex:^(181[89]|2698|3868)$"], userControl: false)]
    public void DancerSkillCleanup(Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return;
        switch (@event.StatusID())
        {
            case 1818:
                accessory.Method.RemoveDraw($"Standard Step.*");
                break;
            case 1819:
                accessory.Method.RemoveDraw($"Technical Step.*");
                break;
            case 2698:
                accessory.Method.RemoveDraw($"Tillana.*");
                break;
            case 3868:
                accessory.Method.RemoveDraw($"Finishing Move.*");
                break;
        }
    }
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

public static class MathHelpers
{
    public static float DegToRad(float degrees)
    {
        return degrees * (float)(Math.PI / 180.0);
    }
    
    public static double DegToRad(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    
    public static float RadToDeg(float radians)
    {
        return radians * (float)(180.0 / Math.PI);
    }
    
    public static double RadToDeg(double radians)
    {
        return radians * 180.0 / Math.PI;
    }
}
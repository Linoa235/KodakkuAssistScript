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

namespace SaintMociannesArboretum;

[ScriptType(guid: "809d4428-a108-412a-b819-e3a1ede1f383", name: "Saint Mocianne's Arboretum", territorys: [511],
    version: "0.0.0.1", Author: "Linoa235", note: noteStr)]

public class SaintMociannesArboretum
{
    const string noteStr =
        """
        v0.0.0.1:
        LV60 Saint Mocianne's Arboretum Initial Drawing
        """;

    #region Basic Settings

    [UserSetting("TTS Toggle (Choose one TTS option)")] public bool isTTS { get; set; } = false;
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")] public bool isEdgeTTS { get; set; } = true;
    [UserSetting("Popup Text Toggle")] public bool isText { get; set; } = true;

    #endregion

    #region Drawing Cleanup

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

    #endregion
    
    [ScriptMethod(name: "Trash_Leafy Sapling Spirit Water (Cleave Highlight)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5355"])]
    public void LeafySapling_SpiritWater(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Leafy Sapling_Spirit Water{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Radian = MathHelpers.DegToRad(60f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "Trash_Hawk Wasp Needle (Line Highlight)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:4526"])]
    public void Needle(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Needle{@event.SourceId}";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Scale = new (3f, 9.1f);
        dp.DestoryAt = 2700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Trash_Orn Wasp Final Sting Tankbuster Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2482"])]
    public void FinalSting(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Stun Final Sting", duration: 3300, true);
        if (isTTS) accessory.Method.TTS($"Stun Final Sting");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Stun Final Sting");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Final Sting{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(1.1f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    #region BOSS1_The Rosehouse
    
    [ScriptMethod(name: "BOSS1_The Rosehouse Fetid Air (Center Poison Pool)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5230"])]
    public void FetidAirCenter(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Fetid Air Center{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.2f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 21000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS1_The Rosehouse Fetid Air (Continuous Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5224"])]
    public void FetidAirCone(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Fetid Air Cone{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(29.8f);
        dp.Radian = 90f.DegToRad();
        dp.DestoryAt = 21000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "BOSS1_Rosebud Stench (Continuous Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5316"])]
    public void Stench(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Stench{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(7.6f);
        dp.Radian = 32f.DegToRad();
        dp.DestoryAt = 21000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "BOSS1_The Rosehouse Sowing Kill Add Hint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:5226"])]
    public void Sowing(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Kill the add");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Kill the add");
    }
    
    #endregion
    
    #region BOSS2_Queen Hawk

    [ScriptMethod(name: "BOSS2_Queen Hawk Defense Order Kill Add Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:152"])]
    public void DefenseOrder(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Kill the add");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Kill the add");
    }

    [ScriptMethod(name: "BOSS2_Queen Hawk Cooperation Order - Cross Fire (Line)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5243"])]
    public void CrossFire(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Cross Fire";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new (14f, 50f);
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    
    #endregion
    
    #region BOSS3_Belladonna

    [ScriptMethod(name: "BOSS3_Belladonna Atropine Spores Approach Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5215"])]
    public void AtropineSpores(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Approach the boss");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Approach the boss");
    }

    [ScriptMethod(name: "BOSS3_Belladonna Vacuum Soul AOE Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5221"])]
    public void VacuumSoul(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "BOSS3_Belladonna Swollen Bulb_Mildew", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:5173"])]
    public void Mildew(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Mildew{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(6f);
        dp.Delay = 5000;
        dp.DestoryAt = 90000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Swollen Bulb Cleanup", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:5173"], userControl: false)]
    public void SwollenBulbCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Mildew{@event.SourceId}");
    }
    
    [ScriptMethod(name: "BOSS3_Belladonna Charming Gaze Look Away Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5216"])]
    public void CharmingGaze(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Look away from boss");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Look away from boss");
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

    public static unsafe ulong GetMarkerEntityId(uint markerIndex)
    {
        var markingController = MarkingController.Instance();
        if (markingController == null) return 0;
        if (markerIndex >= 17) return 0;

        return markingController->Markers[(int)markerIndex];
    }

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

    public static bool HasMarker(IGameObject? obj, MarkType markType)
    {
        return GetObjectMarker(obj) == markType;
    }

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

    public static string GetMarkerName(MarkType markType)
    {
        return markType switch
        {
            MarkType.Attack1 => "Attack 1",
            MarkType.Attack2 => "Attack 2",
            MarkType.Attack3 => "Attack 3",
            MarkType.Attack4 => "Attack 4",
            MarkType.Attack5 => "Attack 5",
            MarkType.Bind1 => "Bind 1",
            MarkType.Bind2 => "Bind 2",
            MarkType.Bind3 => "Bind 3",
            MarkType.Ignore1 => "Ignore 1",
            MarkType.Ignore2 => "Ignore 2",
            MarkType.Square => "Square",
            MarkType.Circle => "Circle",
            MarkType.Cross => "Cross",
            MarkType.Triangle => "Triangle",
            MarkType.Attack6 => "Attack 6",
            MarkType.Attack7 => "Attack 7",
            MarkType.Attack8 => "Attack 8",
            _ => "No Marker"
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
        return AgentMap.Instance()->CurrentTerritoryId;
    }
}
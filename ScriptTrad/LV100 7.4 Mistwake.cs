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

namespace Mistwake;

[ScriptType(name: "LV100 7.4 Mistwake", territorys: [], guid: "f669d803-1154-4440-8e5a-c74173b54ee1", version: "0.0.0.1", Author: "Linoa235")]

public class Mistwake
{
    const string noteStr =
        """
        v0.0.0.2:
        Mistwake Initial Drawing
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
    
    #region BOSS1_Treno Catoblepas
    
    [ScriptMethod(name: "BOSS1_Treno Catoblepas Earthquake AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43327"])]
    public void Earthquake(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "BOSS1_Treno Catoblepas Thunder II Spread Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43331"])]
    public void ThunderIIHint(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Spread, avoid rocks", duration: 2800, true);
        if (isTTS) accessory.Method.TTS($"Spread, avoid rocks");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Spread, avoid rocks");
    }

    [ScriptMethod(name: "BOSS1_Treno Catoblepas Thunder II Ground AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43332"])]
    public void ThunderIIGround(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Thunder II Ground";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = @event.EffectPosition;
        dp.Scale = new Vector2(5f);
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS1_Treno Catoblepas Thunder II Player Marker", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43333"])]
    public void ThunderIIPlayer(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Owner = @event.TargetId();
        dp.DestoryAt = 4700;
        
        if (@event.TargetId() == accessory.Data.Me)
        {
            dp.Name = $"Thunder II Outline";
            dp.Color = new Vector4(1f, 0f, 0f, 10f);
            dp.InnerScale = new Vector2(4.95f);
            dp.Radian = float.Pi * 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        else
        {
            dp.Name = $"Thunder II Player";
            dp.Color = new Vector4(1f, 0f, 0f, 1f);
            dp.Scale = new Vector2(5f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "BOSS1_Treno Catoblepas Bedeviling Light Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43330"])]
    public void BedevilingLight(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Hide behind rocks");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Hide behind rocks");
    }
    
    [ScriptMethod(name: "BOSS1_Treno Catoblepas Thunder III Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43329"])]
    public void TrenoCatoblepas_ThunderIII(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Tankbuster");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Tankbuster");
    }
    
    [ScriptMethod(name: "BOSS1_Treno Catoblepas Ray of Lightning (Line Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44825"])]
    public void RayOfLightning(Event @event, ScriptAccessory accessory)
    {
        if (isText) accessory.Method.TextInfo($"Line stack, avoid rocks", duration: 5300, true);
        if (isTTS) accessory.Method.TTS($"Line stack, avoid rocks");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Line stack, avoid rocks");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Ray of Lightning";
        dp.Scale = new (5f, 50f);
        dp.Owner = @event.SourceId();
        dp.TargetObject = @event.TargetId();
        dp.Color = accessory.Data.DefaultSafeColor; 
        dp.DestoryAt = 6200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }
        
    [ScriptMethod(name: "BOSS1_Treno Catoblepas Petribreath (Cleave)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43335"])]
    public void Petribreath(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Petribreath";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(30f);
        dp.Radian = 120f.DegToRad(); 
        dp.DestoryAt = 4700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    #endregion
    
    #region BOSS2_Amdusias
    
    [ScriptMethod(name: "BOSS2_Amdusias Thunderclap Concerto (Large Cone)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45337|45342)$"])]
    public void ThunderclapConcerto(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Thunderclap Concerto";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = 300f.DegToRad(); 
        dp.Rotation = @event.ActionId() == 45337 ? 0f.DegToRad() : 180f.DegToRad();
        dp.DestoryAt = 5200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "BOSS2_Amdusias Bio II AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45345"])]
    public void BioII(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"AOE");
    }
    
    private readonly Queue<string> _thunderChargeDraws = new();
    private int _drawCounter = 0;

    [ScriptMethod(name: "BOSS2_Amdusias Galloping Thunder (Line Charge)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45348"])]
    public void GallopingThunder(Event @event, ScriptAccessory accessory)
    {
        var drawName = $"Galloping Thunder{@event.SourceId()}_{++_drawCounter}";
        _thunderChargeDraws.Enqueue(drawName);
    
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = drawName;
        dp.Owner = @event.SourceId();
        dp.TargetPosition = @event.EffectPosition();
        dp.Scale = new (5f);
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp.DestoryAt = 15000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);  
    }

    [ScriptMethod(name: "Galloping Thunder Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:45347"], userControl: false)]
    public void GallopingThunderCleanup(Event @event, ScriptAccessory accessory)
    {
        if (_thunderChargeDraws.Count > 0)
        {
            var drawName = _thunderChargeDraws.Dequeue();
            accessory.Method.RemoveDraw(drawName);
        }
    }

    [ScriptMethod(name: "BOSS2_Toxin Cloud Burst (Circle)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2536"])]
    public void Burst(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Burst{@event.TargetId}";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(9f);
        dp.DestoryAt = 4500;
        dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Toxin Cloud Burst Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:45349"], userControl: false)]
    public void BurstCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Burst{@event.SourceId}");
    }
    
    [ScriptMethod(name: "BOSS2_Amdusias Thunder IV AOE + Detonation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45351"])]
    public void ThunderIV(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"AOE, stay away from remaining toxin clouds");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"AOE, stay away from remaining toxin clouds");
        
        foreach (var item in accessory.Data.Objects.GetByDataId(19064))
        {
            if (item is KodakkuAssist.Data.IBattleChara chara)
            {
                if (!IbcHelper.HasStatus(accessory, chara, 0x9E8))
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "Thunder IV";
                    dp.Owner = item.EntityId;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Scale = new Vector2(9f);
                    dp.DestoryAt = 5700;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
        }
    }
    
    [ScriptMethod(name: "BOSS2_Amdusias Thunder III (Continuous Stack)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45353"])]
    public void Amdusias_ThunderIII(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Thunder III";
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = @event.TargetId();
        dp.Scale = new Vector2(6f);
        dp.DestoryAt = 7200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "BOSS2_Amdusias Shockbolt Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45356"])]
    public void Shockbolt(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Tankbuster");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Tankbuster");
    }
    
    #endregion
    
    #region BOSS3_Thundergust Griffin
    
    [ScriptMethod(name: "BOSS3_Thundergust Griffin Thunderspark AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45291"])]
    public void Thunderspark(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"AOE");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"AOE");
    }
    
    [ScriptMethod(name: "BOSS3_Thundergust Griffin Golden Talons Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45305"])]
    public void GoldenTalons(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Tankbuster");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Tankbuster");
    }
    
    [ScriptMethod(name: "BOSS3_Thundergust Griffin Thunderbolt Orb Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4529[678]|4694[34])$"])]
    public void Thunderbolt(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Thunderbolt";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
        dp.Scale = new (6f, 92f);
        dp.DestoryAt = 5200;

        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }

    [ScriptMethod(name: "BOSS3_Thundergust Griffin Fulgurous Fall Line Knockback", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45301"])]
    public void FulgurousFall(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Center knockback then avoid line");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Center knockback then avoid line");
    }
    
    [ScriptMethod(name: "BOSS3_Thundergust Griffin Electrogenetic Force Two-Stage Line Alert", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:45302"])]
    public void ElectrogeneticForce(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Get away");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Get away");
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
#endregion
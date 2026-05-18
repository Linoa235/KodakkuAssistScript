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

namespace A10N;

[ScriptType(guid: "7a2bd5ba-ebd5-4f8d-9016-fe07f0b650d0", name: "A10N", territorys: [581],
    version: "0.0.0.1", author: "Linoa235", note: noteStr)]

public class NewDuty
{
    const string noteStr =
        """
        v0.0.0.1:
        Alexander - The Heart of the Creator (Lamebrix Strikebocks) Initial Drawing
        """;
    
    #region User Controls

    [UserSetting("TTS Toggle (Choose one TTS option)")]
    public bool isTTS { get; set; } = false;
    
    [UserSetting("EdgeTTS Toggle (Choose one TTS option)")]
    public bool isEdgeTTS { get; set; } = true;
    
    [UserSetting("Popup Text Toggle")]
    public bool isText { get; set; } = true;
    
    [UserSetting("Developer Mode")]
    public bool isDeveloper { get; set; } = false;

    #endregion
    
    #region Drawing Cleanup
    
    [ScriptMethod(name: "Mechanic Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(685[459]|6860)$"], userControl: false)]
    public void MechanicCleanup(Event @event, ScriptAccessory accessory)
    {
        switch (@event.ActionId())
        {
            case 6854:
                accessory.Method.RemoveDraw($"Hammer Strike");
                break;
            case 6855:
                accessory.Method.RemoveDraw($"Ice Arrow");
                break;
            case 6859:
                accessory.Method.RemoveDraw($"Goblin Slash"); // Single-fire fill donut
                break;
            case 6860:
                accessory.Method.RemoveDraw($"Goblin Spin"); // Single-fire fill chariot
                break;
        }
    }
    
    [ScriptMethod(name: "Spike Trap Cleanup", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:regex:^80[89]$"], userControl: false)]
    public void SpikeTrapCleanup(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw($"Spike Trap");
    }
    
    #endregion
    
    [ScriptMethod(name: "Spike Trap Highlight", eventType: EventTypeEnum.ChangeMap, eventCondition: ["MapId:regex:^329$"])]
    public void SpikeTrapHighlight(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Spike Trap";
        dp.Color = new Vector4(1f, 0f, 0f, 1.2f);
        dp.Scale = new Vector2(1);
        dp.Position = new Vector3(0f, -115f, 12f);
        dp.DestoryAt = 5400000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Ice Trap_Ice Arrow (Left/Right)", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:65", "Id2:32"])]
    public void IceArrow(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Ice Trap");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Ice Trap");

        foreach (var item in accessory.Data.Objects.GetByDataId(6148))
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Ice Arrow";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = item.EntityId;
            dp.Scale = new(5f, 50f);
            dp.DestoryAt = 4500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
    }
    
    [ScriptMethod(name: "Iron Ball Trap_Hammer Strike (Circle)", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:1", "Id2:32"])]
    public void HammerStrike(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Iron Ball Trap");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Iron Ball Trap");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Hammer Strike";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(4f);
        dp.DestoryAt = 4500;
        
        Vector3 basePosition = new Vector3(0f, -115f, 0f);
        float[] zOffsets = { 0f, -12f, 12f };
    
        foreach (float zOffset in zOffsets)
        {
            dp.Position = new Vector3(basePosition.X, basePosition.Y, basePosition.Z + zOffset);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    
    [ScriptMethod(name: "Surehit (Red Marker Spread)", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^0019$"])]
    public void Surehit(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Surehit{@event.SourceId}";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(4f);
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Goblin Rush Tankbuster Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^6863$"])]
    public void GoblinRush(Event @event, ScriptAccessory accessory)
    {
        var isTank = accessory.Data.MyObject?.IsTank() ?? false;
        if (isTank && isText) accessory.Method.TextInfo($"Tankbuster", duration: 4000, true);
        if (isTTS) accessory.Method.TTS($"Tankbuster");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Tankbuster");
    }
    
    [ScriptMethod(name: "Single-fire Fill - Goblin Spin (Chariot)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^6841$"])]
    public void GoblinSpin(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Move away");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Move away");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Goblin Spin";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(8f);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    [ScriptMethod(name: "Single-fire Fill - Goblin Slash (Donut)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^6840$"])]
    public void GoblinSlash(Event @event, ScriptAccessory accessory)
    {
        if (isTTS) accessory.Method.TTS($"Approach");
        if (isEdgeTTS) accessory.Method.EdgeTTS($"Approach");
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Goblin Slash";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(50f);
        dp.InnerScale = new Vector2(5f);
        dp.Radian = float.Pi * 2;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    [ScriptMethod(name: "Rotating Chainsaw_Slash", eventType: EventTypeEnum.Chat, eventCondition: ["Type:NPCDialogueAnnouncements", 
        "Message:regex:^(å‘¼å“§â€¦â€¦å‘¼å“§â€¦â€¦\næ¥è¯•è¯•é’è“ä¹‹æ‰‹åˆ¶ä½œçš„é™·é˜±å§å“¥å¸ƒï¼|Pssshkoh... Illuminati give Lamebrix toys. Lamebrix likes funplaying with toys!|ã‚·ãƒ¥ã‚³ã‚©â€¦â€¦ã‚·ãƒ¥ã‚³ã‚©â€¦â€¦\nã€Œé’ã®æ‰‹ã€ãŒä½œã£ãŸæ‚ªè¶£å‘³ãªç½ â€¦â€¦ä½¿ã£ã¦ã¿ã‚‹ã‚´ãƒ–ï¼)$"])]
    public void Slash(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Slash";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.Scale = new(4f, 20f);
        dp.Rotation = 90f.DegToRad();
        dp.DestoryAt = 12200;

        Vector3 basePosition = new Vector3(0f, -115f, 0f);
        float[] zOffsets = { 0f, -12f, 12f };
    
        foreach (float zOffset in zOffsets)
        {
            dp.Position = new Vector3(basePosition.X, basePosition.Y, basePosition.Z + zOffset);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
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
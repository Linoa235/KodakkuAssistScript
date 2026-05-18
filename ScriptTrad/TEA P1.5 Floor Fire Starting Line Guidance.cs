using System;
using System.Numerics;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using Newtonsoft.Json;

namespace KodakkuScriptTea
{
    [ScriptType(name: "TEA P1.5 Floor Fire Starting Line Guidance", territorys: [887], guid: "3efbea6e-ec81-4d98-957a-601d9d14aae3", version: "0.0.0.2", author: "Linoa235")]
    public class TeaScript
    {
        private bool is1256 = false;
        private bool isDetermined = false;

        [ScriptMethod(name: "P1.5 Floor Fire Guidance", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:18480"])]
        public void P15FloorFire(Event @event, ScriptAccessory accessory)
        {
            if (@event.ActionId() != 18480) return;
            var pos = @event.EffectPosition();

            if (!isDetermined)
            {
                var side = DetermineSide(pos.X, pos.Z);

                if (side == "Left" && is1256)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Floor Fire Starting Point";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 2800;
                    dp.TargetPosition = pos;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    isDetermined = true;
                }
                else if (side == "Right" && !is1256)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Floor Fire Starting Point";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 2800;
                    dp.TargetPosition = pos;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    isDetermined = true;
                }
            }
        }

        [ScriptMethod(name: "P1.5 Mahjong Record", eventType: EventTypeEnum.TargetIcon)]
        public void P15MahjongRecord(Event @event, ScriptAccessory accessory)
        {
            EventExtensions.ParseHexId(@event["Id"], out var id);
            isDetermined = false;
            var target = @event.TargetId();
            if (target != accessory.Data.Me) return;
            switch (id - 78)
            {
                case 1: is1256 = true; break;
                case 2: is1256 = true; break;
                case 3: is1256 = false; break;
                case 4: is1256 = false; break;
                case 5: is1256 = true; break;
                case 6: is1256 = true; break;
                case 7: is1256 = false; break;
                case 8: is1256 = false; break;
            }
        }

        private static string DetermineSide(float x, float z)
        {
            const float centerX = 100f;
            const float centerZ = 100f;
            const float northX = 100f;
            const float northZ = 0f;
            const double angleOffset = 22.5;
            double radiansOffset = angleOffset * Math.PI / 180;

            double rotatedX = centerX + (northX - centerX) * Math.Cos(radiansOffset) - (northZ - centerZ) * Math.Sin(radiansOffset);
            double rotatedZ = centerZ + (northX - centerX) * Math.Sin(radiansOffset) + (northZ - centerZ) * Math.Cos(radiansOffset);

            float dx = x - centerX;
            float dz = z - centerZ;
            double rotatedDx = rotatedX - centerX;
            double rotatedDz = rotatedZ - centerZ;
            double crossProduct = dx * rotatedDz - dz * rotatedDx;

            if (crossProduct > 0) return "Left";
            else if (crossProduct < 0) return "Right";
            else return "On the line";
        }
    }
}

public static class EventExtensions
{
    public static bool ParseHexId(string? idStr, out uint id)
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

    public static string DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<string>(@event["DurationMilliseconds"]) ?? string.Empty;
    }

    public static uint SourceRotation(this Event @event)
    {
        return ParseHexId(@event["SourceRotation"], out var sourceRotation) ? sourceRotation : 0;
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
        return JsonConvert.DeserializeObject<string>(@event["SourceName"]) ?? string.Empty;
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
}
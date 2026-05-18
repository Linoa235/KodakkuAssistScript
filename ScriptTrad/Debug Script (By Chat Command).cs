using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;
using System.Numerics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.ManagedFontAtlas;
using KodakkuAssist.Data;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Extensions;
using Newtonsoft.Json;
using System.Runtime.Intrinsics.Arm;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Module.Draw.Manager;

namespace KodakkuDebugScript
{
    [ScriptType(
        name: "Debug Script (By Chat Command)",
        guid: "e447e994-66e9-4240-9400-243ba7c1e78f",
        territorys: [],
        version: "0.0.2",
        Author: "Linoa235",
        note: "Enter commands in echo channel for dynamic drawing.\nUsage:\n[target] [shape] [parameters...]\n\nTarget (optional, defaults to self):\n- test ... (draw on self)\n- test eid=[entity ID] ... (draw on specified entity)\n\nShapes and parameters:\n- circle [radius]\n- fan [radius] [angle]\n- rect [width] [length]\n- donut [outer radius] [inner radius]\n- straight [width] [length]\n\nExamples:\n- test circle 5\n- test eid=4000123A fan 10 90"
    )]
    public class GeneralDebugScript
    {
        private enum DrawOriginType { Object, Position }
        private class DrawOrigin
        {
            public DrawOriginType Type { get; set; }
            public ulong OwnerId { get; set; }
            public Vector3 Position { get; set; }
            public string SourceName { get; set; }
        }

        public void Init(ScriptAccessory accessory)
        {
            accessory.Log.Debug("General Debug Script loaded.");
        }

        [ScriptMethod(
            name: "Test Self",
            eventType: EventTypeEnum.Chat,
            eventCondition: new string[] { "Type:Echo" }
        )]
        public async void TestSelf(Event @event, ScriptAccessory accessory)
        {
            string message = @event["Message"];
            if (message.Trim().Equals("Test Self", StringComparison.OrdinalIgnoreCase))
            {
                accessory.Method.SendChat("/e --- Surrounding Player List ---");
                foreach (var obj in accessory.Data.Objects)
                {
                    if (obj is IPlayerCharacter player)
                    {
                        string playerName = player.Name.ToString();
                        var playerJob = player.ClassJob.Value.Name;
                        accessory.Method.SendChat($"/e Player: {playerName}, Job: {playerJob}");
                        await Task.Delay(10);
                    }
                }
                accessory.Method.SendChat("/e --- End of List ---");
            }
        }

        [ScriptMethod(
            name: "Check Job Properties",
            eventType: EventTypeEnum.Chat,
            eventCondition: ["Type:Echo"]
        )]
        public void DebugClassJobProperties(Event @event, ScriptAccessory accessory)
        {
            if (@event["Message"] != "Check Job") return;

            var player = accessory.Data.MyObject;
            if (player?.ClassJob.Value == null)
            {
                accessory.Method.SendChat("/e Unable to retrieve job information.");
                return;
            }

            var classJob = player.ClassJob.Value;
            string report = $"--- Job Information ---\n" +
                            $"Name: {classJob.Name}\n" +
                            $"Abbreviation: {classJob.Abbreviation}\n" +
                            $"ID: {classJob.JobIndex}\n" +
                            $"Role: {classJob.Role}\n" +
                            $"IsTank: {IsTank(player)}\n" +
                            $"IsHealer: {IsHealer(player)}\n" +
                            $"IsDps: {IsDps(player)}";

            accessory.Method.SendChat($"/e {report}");
        }

        [ScriptMethod(
            name: "Chat Command Debug Drawing",
            eventType: EventTypeEnum.Chat,
            eventCondition: new string[] { "Type:Echo" }
        )]
        public void OnEchoChat(Event @event, ScriptAccessory accessory)
        {
            string message = @event["Message"].ToLower();
            string[] parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2 || parts[0] != "test") return;

            DrawOrigin origin = null;
            int commandStartIndex = 1;

            if (parts[1].StartsWith("eid="))
            {
                string eidString = parts[1].Substring(4);
                if (!ulong.TryParse(eidString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong eid))
                {
                    accessory.Method.SendChat($"/e [Debug Script] Error: Invalid entity ID format: {eidString}");
                    return;
                }

                var target = accessory.Data.Objects.SearchById(eid);
                if (target == null)
                {
                    accessory.Method.SendChat($"/e [Debug Script] Error: Object with ID {eidString} not found.");
                    return;
                }
                origin = new DrawOrigin { Type = DrawOriginType.Object, OwnerId = target.EntityId, SourceName = $"Entity({eidString.ToUpper()})" };
                commandStartIndex = 2;
            }
            else
            {
                origin = new DrawOrigin { Type = DrawOriginType.Object, OwnerId = accessory.Data.Me, SourceName = "You" };
            }

            if (parts.Length <= commandStartIndex) return;

            string shapeCommand = parts[commandStartIndex];
            string[] shapeParts = parts.Skip(commandStartIndex).ToArray();

            try
            {
                switch (shapeCommand)
                {
                    case "circle": HandleCircleCommand(shapeParts, origin, accessory); break;
                    case "fan": HandleFanCommand(shapeParts, origin, accessory); break;
                    case "rect": HandleRectCommand(shapeParts, origin, accessory); break;
                    case "donut": HandleDonutCommand(shapeParts, origin, accessory); break;
                    case "straight": HandleStraightCommand(shapeParts, origin, accessory); break;
                }
            }
            catch (Exception ex)
            {
                accessory.Log.Error($"Error processing debug command: {ex.Message}");
                accessory.Method.SendChat($"/e [Debug Script] Command error: {ex.Message}");
            }
        }

        private void ApplyOrigin(DrawPropertiesEdit dp, DrawOrigin origin)
        {
            dp.Owner = origin.OwnerId;
        }

        private void HandleCircleCommand(string[] parts, DrawOrigin origin, ScriptAccessory accessory)
        {
            if (parts.Length != 2) throw new ArgumentException("Invalid Circle command format. Correct format: circle [radius]");
            if (!float.TryParse(parts[1], out float radius)) throw new ArgumentException($"Invalid radius value: {parts[1]}");

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"debug_circle_{Guid.NewGuid()}";
            ApplyOrigin(dp, origin);
            dp.Scale = new Vector2(radius);
            dp.Color = new Vector4(0.1f, 0.8f, 0.8f, 1.0f);
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
            accessory.Method.SendChat($"/e [Debug Script] Drawn circle with radius {radius} on {origin.SourceName}.");
        }

        private void HandleFanCommand(string[] parts, DrawOrigin origin, ScriptAccessory accessory)
        {
            if (parts.Length != 3) throw new ArgumentException("Invalid Fan command format. Correct format: fan [radius] [angle]");
            if (!float.TryParse(parts[1], out float radius)) throw new ArgumentException($"Invalid radius value: {parts[1]}");
            if (!float.TryParse(parts[2], out float angleDegrees)) throw new ArgumentException($"Invalid angle value: {parts[2]}");

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"debug_fan_{Guid.NewGuid()}";
            ApplyOrigin(dp, origin);
            dp.Scale = new Vector2(radius);
            dp.Radian = angleDegrees * MathF.PI / 180.0f;
            dp.Color = new Vector4(0.8f, 0.1f, 0.8f, 1.0f);
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
            accessory.Method.SendChat($"/e [Debug Script] Drawn fan with radius {radius}, angle {angleDegrees}Â° on {origin.SourceName}.");
        }

        private void HandleRectCommand(string[] parts, DrawOrigin origin, ScriptAccessory accessory)
        {
            if (parts.Length != 3) throw new ArgumentException("Invalid Rect command format. Correct format: rect [width] [length]");
            if (!float.TryParse(parts[1], out float width) || !float.TryParse(parts[2], out float length))
                throw new ArgumentException("Invalid width or length value.");

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"debug_rect_{Guid.NewGuid()}";
            ApplyOrigin(dp, origin);
            dp.Scale = new Vector2(width, length);
            dp.Color = new Vector4(0.8f, 0.4f, 0.1f, 1.0f);
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
            accessory.Method.SendChat($"/e [Debug Script] Drawn rectangle with width {width}, length {length} on {origin.SourceName}.");
        }

        private void HandleDonutCommand(string[] parts, DrawOrigin origin, ScriptAccessory accessory)
        {
            if (parts.Length != 3) throw new ArgumentException("Invalid Donut command format. Correct format: donut [outer radius] [inner radius]");
            if (!float.TryParse(parts[1], out float outerRadius) || !float.TryParse(parts[2], out float innerRadius))
                throw new ArgumentException("Invalid outer or inner radius value.");

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"debug_donut_{Guid.NewGuid()}";
            ApplyOrigin(dp, origin);
            dp.Scale = new Vector2(outerRadius);
            dp.InnerScale = new Vector2(innerRadius);
            dp.Radian = MathF.PI * 2;
            dp.Color = new Vector4(0.1f, 0.8f, 0.4f, 1.0f);
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp);
            accessory.Method.SendChat($"/e [Debug Script] Drawn donut with outer radius {outerRadius}, inner radius {innerRadius} on {origin.SourceName}.");
        }

        private void HandleStraightCommand(string[] parts, DrawOrigin origin, ScriptAccessory accessory)
        {
            if (parts.Length != 3) throw new ArgumentException("Invalid Straight command format. Correct format: straight [width] [length]");
            if (!float.TryParse(parts[1], out float width) || !float.TryParse(parts[2], out float length))
                throw new ArgumentException("Invalid width or length value.");

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"debug_straight_{Guid.NewGuid()}";
            ApplyOrigin(dp, origin);
            dp.Scale = new Vector2(width, length);
            dp.Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
            accessory.Method.SendChat($"/e [Debug Script] Drawn straight line with width {width}, length {length} on {origin.SourceName}.");
        }

        #region Helper_Functions
        private bool IsTank(IPlayerCharacter player)
        {
            if (player?.ClassJob.Value == null) return false;
            return player.ClassJob.Value.Role == 1;
        }

        private bool IsHealer(IPlayerCharacter player)
        {
            if (player?.ClassJob.Value == null) return false;
            return player.ClassJob.Value.Role == 4;
        }

        private bool IsDps(IPlayerCharacter player)
        {
            if (player?.ClassJob.Value == null) return false;
            return !IsTank(player) && !IsHealer(player);
        }

        private Vector3 RotatePoint(Vector3 point, Vector3 center, float angleRad)
        {
            float s = MathF.Sin(angleRad);
            float c = MathF.Cos(angleRad);
            point.X -= center.X;
            point.Z -= center.Z;
            float xnew = point.X * c - point.Z * s;
            float znew = point.X * s + point.Z * c;
            point.X = xnew + center.X;
            point.Z = znew + center.Z;
            return point;
        }
        #endregion
    }
}
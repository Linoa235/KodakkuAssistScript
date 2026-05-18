using FFXIVClientStructs.FFXIV.Common.Math;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using Newtonsoft.Json;
using System;

namespace KDrawScript.Dev
{
    [ScriptType(name: "Abyssos The Eighth Circle (P8)", territorys: [1087], guid: "05e7f8c2-2f5d-4b34-8c56-5fa6f24bf211", version: "0.0.0.1", author: "Linoa235")]
    public class Abyssos_The_Eighth_Circle
    {
        [UserSetting(note: "Enable text reminders")]
        public bool EnableTextInfo { get; set; } = true;

        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(".*");
        }

        [ScriptMethod(name: "Sunforge", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3095[89])$"])]
        public void Sunforge(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!float.TryParse(@event["SourceRotation"], out var rot)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Sunforge - {sid}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 8000;

            switch (@event["ActionId"])
            {
                case "30958":
                    dp.Position = ParsePosition(@event, "EffectPosition");
                    dp.Scale = new(14, 84);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                    break;
                case "30959":
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = ParsePosition(@event, "EffectPosition");
                    dp.Scale = new(84, 14);
                    dp.Rotation = rot;
                    dp.FixRotation = true;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                    break;
            }
        }

        [ScriptMethod(name: "Flameviper Tankbuster Range", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30984"])]
        public void Flameviper(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Flameviper";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.TargetObject = tid;
            dp.DestoryAt = 5000;
            dp.Scale = new(5, 60);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Reforged Reflection Text Reminder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3105[12])$"])]
        public void ReforgedReflection(Event @event, ScriptAccessory accessory)
        {
            switch (@event["ActionId"])
            {
                case "31051": SendText("Chariot", accessory); break;
                case "31052": SendText("Donut", accessory); break;
            }
        }

        [ScriptMethod(name: "Quadrupedal Impact Knockback Prediction", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:30988"])]
        public void QuadrupedalImpact(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            var pos = ParsePosition(@event, "SourcePosition");
            dp.Name = $"Quadrupedal Impact - {pos.X},{pos.Y}";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = ParsePosition(@event, "SourcePosition");
            dp.Rotation = float.Pi;
            dp.Scale = new(1.5f, 15);
            dp.Delay = 4000;
            dp.DestoryAt = 3000;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "Cthonic Vent First Four-Corner Large Chariot", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0005"])]
        public void CthonicVent(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Cthonic Vent - {sid}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 5000;
            dp.Scale = new(23);

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Petrifaction Look Away", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30972"])]
        public void Petrification(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Petrifaction - {sid}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 5000;
            dp.Scale = new(3);

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Torch Flame Arena Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30968"])]
        public void TorchFlame(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Torch Flame - {sid}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 7000;
            dp.Scale = new(10, 10);

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }

        #region Utility
        private static bool ParseObjectId(string? idStr, out uint id)
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

        private static Vector3 ParsePosition(Event @event, string type)
        {
            return JsonConvert.DeserializeObject<Vector3>(@event[type]);
        }

        private void SendText(string text, ScriptAccessory accessory, int duration = 2000, bool isImportant = true)
        {
            if (!EnableTextInfo) return;
            accessory.Method.TextInfo(text, duration, isImportant);
        }
        #endregion
    }
}
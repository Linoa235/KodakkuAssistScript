using FFXIVClientStructs.FFXIV.Common.Math;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using Newtonsoft.Json;
using System;

namespace KDrawScript.Dev
{
    [ScriptType(name: "Alexander - The Fist of the Son (A5)", territorys: [520], guid: "bf9e655f-c0f1-425e-8b7c-e7b803ad024d", version: "0.0.0.1", Author: "Linoa235")]
    public class TheFistoftheSon
    {
        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(".*");
        }

        [ScriptMethod(name: "Gobjab", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:996"])]
        public void Gobjab(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("Swap tanks or heal current tank to full", duration: 2000, true);
        }

        [ScriptMethod(name: "Glupgloop", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"])]
        public void Glupgloop(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            accessory.Method.TextInfo("Place poison in corner", duration: 2000, true);
        }

        [ScriptMethod(name: "Bomb's Away", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5520"])]
        public void BombsAway(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("Push the bomb", duration: 2000, true);
        }

        [ScriptMethod(name: "Big Burst", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:5354"])]
        public void BigBurst(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Big Burst - {sid}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 15000;
            dp.Scale = new(35);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Burst Cancel", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:5521"])]
        public void BurstCancel(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            accessory.Method.RemoveDraw($"Big Burst.*");
        }

        [ScriptMethod(name: "Boost", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5522"])]
        public void Boost(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("Step on left poison to become bird!", duration: 2000, true);
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
        #endregion
    }
}
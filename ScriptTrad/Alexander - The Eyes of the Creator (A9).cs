using FFXIVClientStructs.FFXIV.Common.Math;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using Newtonsoft.Json;
using System;

namespace KDrawScript.Dev
{
    [ScriptType(name: "Alexander - The Eyes of the Creator (A9)", territorys: [580], guid: "bc5ed8fd-5a8b-47d8-a921-45032b05788d", version: "0.0.0.1", author: "Linoa235")]
    public class TheEyesoftheCreator
    {
        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(".*");
        }

        [ScriptMethod(name: "Doll Scarp", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(635[012])$"])]
        public void DollScarp(Event @event, ScriptAccessory accessory)
        {
            var position = ParsePosition(@event, "SourcePosition");
            var location = PositionToLocation(position);

            accessory.Method.TextInfo($"ST place bomb at {location}, pull to glowing spot", duration: 4000, true);
        }

        [ScriptMethod(name: "Scrap Burst", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6926"])]
        public void ScrapBurst(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("Hide behind rock", duration: 2000, true);
        }

        [ScriptMethod(name: "Scrapline", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6928"])]
        public void Scrapline(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("Hide behind", duration: 2000, true);
        }

        [ScriptMethod(name: "Faust", eventType: EventTypeEnum.Tether, eventCondition: ["Id:000C"])]
        public void Faust(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("Attack the add", duration: 2000, true);
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

        private static string PositionToLocation(Vector3 position)
        {
            if (position.X < 0)
            {
                if (position.Z > -250) return "Bottom Left";
                else return "Top Left";
            }
            else
            {
                if (position.Z > -250) return "Bottom Right";
                else return "Top Right";
            }
        }
        #endregion
    }
}
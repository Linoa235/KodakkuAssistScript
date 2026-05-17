using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using System.Windows.Forms;
using Dalamud.Game.ClientState.Objects.Types;

namespace MyScriptNamespace
{
    [ScriptType(name: "Variant Dungeon Auto", territorys: [], $11cb5331f-98d9-48c9-bb53-86cb11dae6a0", version: "0.0.0.1", Author: "Linoa235", guid: "6391d10b-f377-4ed8-a821-76efdc7e5977")]
    public class VariantDungeonAutomation
    {
        [UserSetting(note: "Use heal on self when HP is below this value")]
        public int healthThreshold { get; set; } = 20000;

        [UserSetting(note: "Variant Heal Slot, Left=2, Right=1")]
        public int healthSlot { get; set; } = 1;

        [UserSetting(note: "DoT Slot, Left=2, Right=1")]
        public int DotSlot { get; set; } = 1;

        DateTime lasthealth = DateTime.Now;
        DateTime lastdot = DateTime.Now;

        public void Init(ScriptAccessory accessory)
        {
        }

        [ScriptMethod(name: "Auto DoT", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3359"])]
        public void AutoDot(Event @event, ScriptAccessory accessory)
        {
            lock (this)
            {
                if ((DateTime.Now - lastdot).TotalSeconds < 2.5) return;
                if (!ParseObjectId(@event["SourceId"], out var sid)) return;
                if (!ParseObjectId(@event["TargetId"], out var tid)) return;
                if (sid != accessory.Data.Me) return;
                var obj = accessory.Data.Objects.SearchByEntityId(tid);
                if (obj == null) return;
                if (((IBattleChara)obj).IsDead) return;
                accessory.Method.SelectTarget(tid);
                accessory.Method.SendChat($"/generalaction ä»»åŠ¡æŒ‡ä»¤{DotSlot}");
                lastdot = DateTime.Now;
            }
        }

        [ScriptMethod(name: "Auto Heal", eventType: EventTypeEnum.UpdateHpMp)]
        public void AutoHeal(Event @event, ScriptAccessory accessory)
        {
            if ((DateTime.Now - lasthealth).TotalSeconds < 2.5) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (sid != accessory.Data.Me) return;
            if (!int.TryParse(@event["Hp"].Split('/')[0], out var chp)) return;
            accessory.Log.Debug($"{chp}");
            if (chp < healthThreshold)
            {
                accessory.Log.Debug("a6");
                accessory.Method.SendChat($"/generalaction ä»»åŠ¡æŒ‡ä»¤{healthSlot}");
                lasthealth = DateTime.Now;
            }
        }

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
    }
}
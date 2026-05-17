using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using System.Windows.Forms;
using Dalamud.Game.ClientState.Objects.Types;

namespace MyScriptNamespace
{
    [ScriptType(name: "Variant Dungeon Automation", territorys: [], $11bdde73d-e1b6-4673-8c8a-39d90a9a93f9", version: "0.0.0.1", Author: "Linoa235", guid: "dc162268-0763-4de0-b3b2-296b29dcc04a")]
    public class VariantDungeonAutomation
    {
        
        [UserSetting(note:"Use healing on self when HP is below this value")]
        public int healthThreshold { get; set; } = 20000;

        [UserSetting(note: "Variant heal slot, left side is 2, right side is 1")]
        public int healthSlot { get; set; } = 1;
        [UserSetting(note: "Dot slot, left side is 2, right side is 1")]
        public int DotSlot { get; set; } = 1;


        DateTime lasthealth=DateTime.Now;
        DateTime lastdot= DateTime.Now;

        public void Init(ScriptAccessory accessory)
        {
        }


        [ScriptMethod(name: "Auto Dot", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3359"])]
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
                accessory.Method.SendChat($"/CommonSkill DutyAction{DotSlot}");
                lastdot = DateTime.Now;
            }
            

        }

        [ScriptMethod(name: "Auto Heal", eventType: EventTypeEnum.UpdateHpMp)]
        public void AutoHeal(Event @event, ScriptAccessory accessory)
        {
            if ((DateTime.Now-lasthealth).TotalSeconds<2.5) return;
            if (!ParseObjectId(@event["SourceId"],out var sid)) return;
            if (sid!=accessory.Data.Me) return;
            if (!int.TryParse(@event["Hp"].Split('/')[0], out var chp)) return;
            accessory.Log.Debug($"{chp}");
            if (chp<healthThreshold)
            {
                accessory.Log.Debug("Healing triggered");
                accessory.Method.SendChat($"/CommonSkill DutyAction{healthSlot}");
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
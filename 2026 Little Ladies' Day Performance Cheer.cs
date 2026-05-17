using Newtonsoft.Json;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using KodakkuAssist.Script;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Module.GameEvent;

namespace TsingNamespace.Normal.LittleLadiesDay2026
{
    [ScriptType(name: "2026 Little Ladies' Day Performance Cheer", territorys: [130], guid: "22cfc380-edc2-f441-f9f9-f9f212f2632f", version: "0.0.0.1", Author: "Linoa235")]
    public class LittleLadiesDay
    {
        [UserSetting("Performance Audience Status Refresh Reminder")]
        public bool FanStatusReminder { get; set; } = true;

        [UserSetting("Use Voice for Performance Audience Status Refresh Reminder")]
        public bool FanStatusReminderVoice { get; set; } = false;

        [ScriptMethod(name: "Little Ladies Switch to Targetable State",
            eventType: EventTypeEnum.Targetable,
            suppress: 1000,
            eventCondition: ["DataId:regex:^(18859|1886[0-2])$", "Targetable:True"])]
        public void TargetableTrue_LittleLadies(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"[TargetableTrue_LittleLadies] SourceId: {@event.SourceId}");
            ulong sourceId = @event.SourceId;
            Thread.Sleep(1000);

            IPlayerCharacter myChara = accessory.Data.MyObject;
            if (myChara is null) return;
            Status statusInfo = null;

            uint fanStatusId = 1494;
            foreach (Status status in myChara.StatusList)
            {
                if (status.StatusId == fanStatusId)
                {
                    statusInfo = status;
                    break;
                }
            }
            if (statusInfo is null) return;

            uint npcDataId = statusInfo.Param switch
            {
                561 => 18859,
                562 => 18860,
                563 => 18861,
                564 => 18862,
                _ => 18859,
            };
            uint myActionId = npcDataId switch
            {
                18859 => 44501,
                18860 => 44502,
                18861 => 44503,
                18862 => 44504,
                _ => 44501,
            };

            IGameObject? targetNpc = accessory.Data.Objects.GetByDataId(npcDataId).FirstOrDefault(obj => obj.IsTargetable);
            if (targetNpc is not null)
            {
                accessory.Method.SelectTarget((uint)targetNpc.GameObjectId);
                accessory.Log.Debug($"[TargetableTrue_LittleLadies] TargetNpc found! DataId: {targetNpc.DataId}, ObjectId: {targetNpc.GameObjectId}, Name: {targetNpc.Name}");
                accessory.Method.UseAction((uint)targetNpc.GameObjectId, myActionId);
            }
            else
            {
                accessory.Log.Debug($"[TargetableTrue_LittleLadies] TargetNpc NOT found! DataId: {npcDataId}");
                IGameObject? sourceNpc = accessory.Data.Objects.SearchById(sourceId);
                if (sourceNpc is not null)
                {
                    myActionId = sourceNpc.DataId switch
                    {
                        18859 => 44501,
                        18860 => 44502,
                        18861 => 44503,
                        18862 => 44504,
                        _ => myActionId,
                    };
                    accessory.Method.SelectTarget((uint)sourceNpc.GameObjectId);
                    accessory.Log.Debug($"[TargetableTrue_LittleLadies] SourceNpc found! DataId: {sourceNpc.DataId}, ObjectId: {sourceNpc.GameObjectId}, Name: {sourceNpc.Name}");
                    accessory.Method.UseAction((uint)sourceNpc.GameObjectId, myActionId);
                }
            }
        }

        DateTime lastStatusAddTime = DateTime.MinValue;

        [ScriptMethod(name: "Little Ladies Performance Audience Status Added",
            eventType: EventTypeEnum.StatusAdd,
            suppress: 1000,
            eventCondition: ["StatusID:regex:^(1494)$", "SourceId:E0000000"])]
        public void StatusAdd_Fans(Event @event, ScriptAccessory accessory)
        {
            if (@event.TargetId != accessory.Data.Me) return;
            accessory.Log.Debug($"[StatusAdd_Fans] My Fan status Added!");
            lastStatusAddTime = DateTime.Now;
        }

        [ScriptMethod(name: "Little Ladies Performance Audience Status Removed",
            eventType: EventTypeEnum.StatusRemove,
            suppress: 1000,
            eventCondition: ["StatusID:regex:^(1494)$", "SourceId:E0000000"])]
        public void StatusRemove_Fans(Event @event, ScriptAccessory accessory)
        {
            if (@event.TargetId != accessory.Data.Me) return;
            accessory.Log.Debug($"[StatusRemove_Fans] My Fan status removed!");
            if (DateTime.Now - lastStatusAddTime > TimeSpan.FromSeconds(1700) && FanStatusReminder)
            {
                accessory.Log.Debug($"[StatusRemove_Fans] Status removed by timeout, giving reminder.");
                if (FanStatusReminderVoice)
                {
                    accessory.Method.TTS("Please refresh your performance audience status!");
                }
                accessory.Method.SendChat("/e <se.1>");
            }
        }
    }
}
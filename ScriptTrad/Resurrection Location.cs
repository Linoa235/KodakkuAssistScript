using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
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
using System.Collections.Concurrent;
using KodakkuAssist.Module.Draw.Manager;

namespace KodakkuAssistXSZYYS
{
    [ScriptType(
        name: "Resurrection Location",
        $1400b8024-e2e2-4aad-b1ba-b561fb4c18cd",
        territorys: [],
        version: "0.0.2",
        Author: "Linoa235",
        note: "See where you will resurrect"
    , guid: "68eb3787-666b-476e-92ac-10fafe78401b")]
    public class ResurrectionLocation
    {
        [ScriptMethod(name: "Resurrection", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:148"])]
        public async void Resurrection(Event @event, ScriptAccessory accessory)
        {
            try
            {
                if (@event.TargetId != accessory.Data.Me) return;
                accessory.Method.RemoveDraw("RebornPosition");
                await Task.Delay(50);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "RebornPosition";
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = @event.SourcePosition;
                dp.Scale = new Vector2(2);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.ScaleMode = ScaleMode.YByDistance;
                dp.DestoryAt = 60000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            catch (Exception e)
            {
                accessory.Log.Debug("Resurrection not found");
            }
        }

        [ScriptMethod(name: "Remove Resurrection", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:148"], userControl: false)]
        public void RemoveReborn(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw("RebornPosition");
        }
    }
}
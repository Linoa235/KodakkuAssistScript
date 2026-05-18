using System;
using System.Linq;
using System.Threading.Tasks;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;

namespace Cyf5119Script.General.InvulnAlert;

[ScriptType(guid: "7f02c10c-98be-4c62-bd6f-f5a9d337272e", name: "Invuln Alert Macro", territorys: [], version: "0.0.0.4", Author: "Linoa235", note: "Invincibility countdown prompt for self.")]
public class InvulnAlert
{
    [UserSetting("Channel")] public string channel { get; set; } = "e";
    [UserSetting("Sound Effect")] public string se { get; set; } = "<se.1><se.1>";

    [ScriptMethod(name: "Hallowed Ground", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:82"])]
    public void HallowedGround(Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return;
        Alert(accessory, "Hallowed Ground");
    }

    [ScriptMethod(name: "Holmgang", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:409"])]
    public void Holmgang(Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return;
        Alert(accessory, "Holmgang");
    }

    [ScriptMethod(name: "Living Dead", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:810"])]
    public void LivingDead(Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return;
        accessory.Method.SendChat($"/e Living Dead activated, please let me die!{se}");
    }

    [ScriptMethod(name: "Walking Dead", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:811"])]
    public void WalkingDead(Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return;
        Alert(accessory, "Walking Dead");
    }

    [ScriptMethod(name: "Superbolide", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1836"])]
    public void Superbolide(Event @event, ScriptAccessory accessory)
    {
        if (@event.SourceId() != accessory.Data.Me) return;
        Alert(accessory, "Superbolide");
    }

    private async void Alert(ScriptAccessory accessory, string str)
    {
        accessory.Method.SendChat($"/{channel} {str} activated!{se}");
        await Task.Delay(7000);
        accessory.Method.SendChat($"/{channel} {str} 3 seconds remaining!");
        await Task.Delay(1000);
        accessory.Method.SendChat($"/{channel} {str} 2 seconds remaining!");
        await Task.Delay(1000);
        accessory.Method.SendChat($"/{channel} {str} 1 second remaining!");
        await Task.Delay(1000);
        accessory.Method.SendChat($"/{channel} {str} ended!{se}");
    }
}
using System;
using System.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;

namespace HelloWorld;

[ScriptType(name: "HelloWorld", territorys: [], $174f5227e-a4d1-4095-b640-040c39897cb4", version: "0.0.0.1", Author: "Linoa235", guid: "c45aac15-dda5-4a84-95b8-e727ef9eae44")]
public class HelloWorld
{
    [ScriptMethod(name: "SampleMethod", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:24286"])]
    public void SampleMethod(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Hello World!", 5000);
    }
}
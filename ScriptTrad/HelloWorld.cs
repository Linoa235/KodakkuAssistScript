using System;
using System.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;

namespace HelloWorld;

[ScriptType(
    name: "HelloWorld",
    territorys: new uint[] { },
    guid: "58d98bc0-23f6-4133-9afa-87060a2905b8",
    version: "0.0.0.1",
    Author: "Linoa235",
    note: null
)]
public class HelloWorld
{
    [ScriptMethod(name: "SampleMethod", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:24286"])]
    public void SampleMethod(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Hello World!", 5000);
    }
}
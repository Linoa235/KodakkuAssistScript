using System;
using System.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;

namespace HelloWorld;

[ScriptType(
    name: "HelloWorld",
    territorys: new uint[] { },
    guid: "92503fe9-a50c-40c7-8eb0-9614dd8586b4",
    version: "0.0.0.1",
    author: "Linoa235",
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
using System;
using System.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;

namespace HelloWorld;

[ScriptType(
    name: "HelloWorld",
    territorys: new uint[] { },
    guid: "d568107b-5463-4ad4-b0dc-2316b68c6dda",
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
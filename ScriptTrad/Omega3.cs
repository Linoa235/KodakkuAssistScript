using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.Draw;
using Dalamud.Utility.Numerics;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using Dalamud.Memory.Exceptions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using KodakkuAssist.Module.GameOperate;

namespace KodakkuAssist.Omega;

[ScriptType(name: "Omega3", territorys: [693], guid: "790af03f-7995-47ad-a850-96c6b8a3d2ab", version: "0.0.0.1",
    author: "Linoa235")]
public class Omega3
{
    [ScriptMethod(name: "Croak", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9318"])]
    public void Croak(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        if (ParseObjectId(@event["SourceId"], out var id))
        {
            dp.Owner = id;
        }
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(20);
        dp.Radian = float.Pi * 5 / 6;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Game Start", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9325"])]
    public void GameStart(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Go to your role's corresponding tile", 2000);
    }

    [ScriptMethod(name: "Queen's Dance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9329"])]
    public void QueensDance(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Go to the blue tile", 2000);
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
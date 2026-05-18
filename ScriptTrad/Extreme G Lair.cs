using Dalamud.Game;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.GameEvent.Types;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Script;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace BakaWater77.æžæ ¼èŽ±æ¨æ‹‰;

[ScriptType(
   name: "Extreme G Lair",
   territorys: new uint[] { 1308 },
   guid: "192298bf-0e3f-4acf-824e-da5b70ad8249",
   version: "0.0.0.3",
   Author: "Linoa235",
   note: null
)]
public class ExtremeGLair
{
    public bool isText { get; set; } = true;
    
    private static bool ParseObjectId(string? idStr, out uint id)
    {
        id = 0;
        if (string.IsNullOrEmpty(idStr)) return false;
        try
        {
            id = uint.Parse(idStr.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private Dictionary<uint, Event> startCastingCache = new();
    private uint lastActionIdForScatter = 0;
    private uint lastTargetIdForShare = 0;

    [ScriptMethod(
        name: "Overboost",
        eventType: EventTypeEnum.StartCasting,
        eventCondition: new[] { "ActionId:regex:^(45663|45664|45670|45677|45696)$" },
        userControl: true
    )]
    public async void Overboost(Event @event, ScriptAccessory accessory)
    {
        if (!int.TryParse(@event["ActionId"], out var actionId))
            return;

        if (actionId == 45663)
        {
            if (isText)
                accessory.Method.TextInfo("Spread soon", duration: 4700);
            lastTargetIdForShare = 0;
            lastActionIdForScatter = (uint)actionId;
            return;
        }

        if (actionId == 45664)
        {
            if (isText)
                accessory.Method.TextInfo("Stack soon", duration: 4700);
            lastActionIdForScatter = 0;
            if (ParseObjectId(@event["TargetId"], out uint TargetId))
            {
                lastTargetIdForShare = TargetId;
            }
            return;
        }

        if (actionId == 45670 || actionId == 45677 || actionId == 45696)
        {
            await Task.Delay(8500);
            
            if (lastTargetIdForShare == 0x400024A8) // 4TN
            {
                DrawMembers(accessory, new int[] { 0, 1, 2, 3 }, accessory.Data.DefaultSafeColor, "Overboost");
            }
            else if (lastTargetIdForShare == 0x40001594 || lastTargetIdForShare == 0x40002AF7 || lastTargetIdForShare == 0x40000D71) // 4DPS
            {
                DrawMembers(accessory, new int[] { 4, 5, 6, 7 }, accessory.Data.DefaultSafeColor, "Overboost");
            }
            else if (lastActionIdForScatter == 45663) // Spread
            {
                DrawMembers(accessory, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, accessory.Data.DefaultDangerColor, "Overboost");
            }
        }
    }

    private void DrawMembers(ScriptAccessory accessory, int[] indices, Vector4 color, string name)
    {
        foreach (var index in indices)
        {
            var memberObj = accessory.Data.PartyList.ElementAtOrDefault(index);
            if (memberObj == null) continue;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Owner = memberObj;
            dp.Scale = new Vector2(5);
            dp.Color = color;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    [ScriptMethod(
        name: "Ether Cannon",
        eventType: EventTypeEnum.TargetIcon,
        eventCondition: ["Id:027E"],
        userControl: true
    )]
    public void EtherCannon(Event @event, ScriptAccessory accessory)
    {
        if (isText)
            accessory.Method.TextInfo("Spread", duration: 4700);

        for (int i = 0; i < accessory.Data.PartyList.Count; i++)
        {
            if (i == 0 || i == 1) continue;
            var p = accessory.Data.PartyList[i];
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Ether Cannon";
            dp.Owner = p;
            dp.Scale = new Vector2(6);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    [ScriptMethod(
        name: "Ether Shockwave",
        eventType: EventTypeEnum.TargetIcon,
        eventCondition: ["Id:027D"],
        userControl: true
    )]
    public void DrawH1H2Circle(Event ev, ScriptAccessory sa)
    {
        if (sa.Data.PartyList.Count < 2) return;
        if (isText)
            sa.Method.TextInfo("Stack", duration: 4700);

        var H1H2 = new[]
        {
            (Index: 2, Name: "H1"),
            (Index: 3, Name: "H2")
        };

        foreach (var (index, name) in H1H2)
        {
            var memberObj = sa.Data.PartyList[index];
            if (memberObj == 0) continue;
            var dp = sa.Data.GetDefaultDrawProperties();
            dp.Name = "Ether Shockwave";
            dp.Owner = memberObj;
            dp.Scale = new Vector2(6);
            dp.Color = sa.Data.DefaultSafeColor;
            dp.DestoryAt = 6000;
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
}
using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using System.Reflection.Metadata;
using System.Net;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using System.Runtime.Intrinsics.Arm;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using FFXIVClientStructs;
using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace Veever.DawnTrail.Cenote_Ja_Ja_Gural;

[ScriptType(name: "Cenote Ja Ja Gural", territorys: [1209], guid: "94f21f83-f6c9-479f-9bff-3ba2b462fa41",
    version: "0.0.0.4", author: "Linoa235", note: noteStr)]

public class Cenote_Ja_Ja_Gural
{
    const string noteStr =
    """
    v0.0.0.4:
    1. Cenote Ja Ja Gural drawing is based only on my own runs. No drawings for later Sky Arrow mechanics.
    2. If you have footage of this, please DM me on Discord and I will add it to the drawings.
    Duckmen.
    """;

    [UserSetting("Debug toggle, please turn off for non-development use")]
    public bool isDebug { get; set; } = false;

    public int AetherialLightCount;

    private readonly object AetherialLightLock = new object();

    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        AetherialLightCount = 0;
    }

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    [ScriptMethod(name: "Crypsis", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3997"])]
    public void Crypsis(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Crypsis";
        dp.Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(5f);
        dp.InnerScale = new Vector2(4.9f);
        dp.DestoryAt = 10000;
        dp.Radian = 2 * float.Pi;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, dp);
    }

    [ScriptMethod(name: "Golden Gall", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38267"])]
    public void GoldenGall(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Golden Gall";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId();
        dp.Scale = new Vector2(40f);
        dp.Radian = float.Pi;
        dp.DestoryAt = 6700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Aetherial Light", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38271"])]
    public async void AetherialLight(Event @event, ScriptAccessory accessory)
    {
        lock (AetherialLightLock)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Aetherial Light";
            dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
            dp.Owner = @event.SourceId();
            dp.Scale = new Vector2(40f);
            dp.Radian = float.Pi / 180 * 60;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"Aetherial Light Safe";
            dp1.Color = accessory.Data.DefaultSafeColor;
            dp1.Owner = @event.SourceId();
            dp1.Scale = new Vector2(40f);
            dp1.Radian = float.Pi / 180 * 60;
            dp1.Delay = 5000;
            dp1.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);
        }
        if (AetherialLightCount >= 5)
        {
            await Task.Delay(5000);
            accessory.Method.RemoveDraw(".*");
        }
        DebugMsg($"{AetherialLightCount}", accessory);
        AetherialLightCount++;
    }

    [ScriptMethod(name: "Flame Blade", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3825[15]$"])]
    public void FlameBlade(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Flame Blade";
        dp.Owner = @event.SourceId();
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new(5f, 40f);
        dp.DestoryAt = 2200;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
    }
}

// EventExtensions, Extensions, IbcHelper classes (same as previous files)
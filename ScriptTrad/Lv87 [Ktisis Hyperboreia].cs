using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Data;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Module.GameEvent.Types;
using KodakkuAssist.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;
using Dalamud.Utility.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;
using Lumina.Excel.Sheets;

namespace UsamisScript;

[ScriptType(name: "Lv87 [Ktisis Hyperboreia]", territorys: [974],
    guid: "8d4e4a9c-b144-4ec2-82cd-46b38867e4e6", version: "0.0.0.2", author: "Usami", note: "Boss2 and Boss3 only, stop eating DoTs!", updateInfo: UpdateInfo)]
public class KtisisHyperboreia
{
    private const string UpdateInfo =
        """
        1. Adapted to Kodakku 0.5.x.x
        """;
    
    [UserSetting("Debug mode (players don't need to enable)")]
    public bool DebugMode { get; set; } = false;

    List<bool> Boss2_SafePosition = [false, false, false];

    public void Init(ScriptAccessory accessory)
    {
        Boss2_SafePosition = [false, false, false];
        accessory.Method.RemoveDraw(".*");
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

    public static int PositionMatchesTo8Dir(Vector3 point, Vector3 center)
    {
        float x = point.X - center.X;
        float z = point.Z - center.Z;

        int direction = (int)Math.Round(4 - 4 * Math.Atan2(x, z) / Math.PI) % 8;
        return (direction + 8) % 8;
    }

    #region Boss2: King of the Largon

    [ScriptMethod(name: "BOSS2: Breath Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(2812|2813|2814)$"], userControl: false)]
    public void Boss2_BreathCollect(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var stid = @event["StatusID"];
        switch (stid)
        {
            case "2812":
                Boss2_SafePosition[0] = true;
                if (DebugMode)
                    accessory.Method.SendChat($"/e [DEBUG]: Detected middle head breath...");
                break;
            case "2813":
                Boss2_SafePosition[1] = true;
                if (DebugMode)
                    accessory.Method.SendChat($"/e [DEBUG]: Detected left head breath...");
                break;
            case "2814":
                Boss2_SafePosition[2] = true;
                if (DebugMode)
                    accessory.Method.SendChat($"/e [DEBUG]: Detected right head breath...");
                break;
        }
    }

    [ScriptMethod(name: "BOSS2: Breath Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25734|25735|25736|25737|25738|25739)$"])]
    public void Boss2_BreathCall(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;

        if (DebugMode)
            accessory.Method.SendChat($"/e [DEBUG]: Middle {Boss2_SafePosition[0]}, Left {Boss2_SafePosition[1]}, Right {Boss2_SafePosition[2]}");

        if (Boss2_SafePosition[2])
        {
            Boss2_SafePosition[2] = false;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Cone detection - Right Rear";
            dp.Scale = new(30);
            dp.Radian = float.Pi * 2 / 3;
            dp.Rotation = -float.Pi * 2 / 3;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 0;
            dp.DestoryAt = 6700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        if (Boss2_SafePosition[1])
        {
            Boss2_SafePosition[1] = false;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Cone detection - Left Rear";
            dp.Scale = new(30);
            dp.Radian = float.Pi * 2 / 3;
            dp.Rotation = float.Pi * 2 / 3;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 0;
            dp.DestoryAt = 6700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        if (Boss2_SafePosition[0])
        {
            Boss2_SafePosition[0] = false;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Cone detection - Front";
            dp.Scale = new(30);
            dp.Radian = float.Pi * 2 / 3;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 0;
            dp.DestoryAt = 6700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
    }

    #endregion

    #region Boss3: Hermes

    [ScriptMethod(name: "BOSS3: True Aero IV", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25889|27836)$"])]
    public void Boss3_TrueAeroIV(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"True Aero IV Line";
        dp.Scale = new(10, 50);
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Delay = 0;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "BOSS3: Quad True Aero IV", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^27837$"])]
    public void Boss3_QuadTrueAeroIV(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Quad True Aero IV";
        dp.Scale = new(10, 50);
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Delay = 6000;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    #endregion
}
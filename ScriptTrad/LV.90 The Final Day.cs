using Dalamud.Utility.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.GameEvent.Types;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Script;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Veever.EndWalker.the_Final_Day;

[ScriptType(name: Name, territorys: [997], guid: "2d5109ba-3e76-453f-a681-454d782aed9a",
    version: Version, author: "Linoa235", note: NoteStr, updateInfo: UpdateInfo)]

public class the_Final_Day
{
    const string NoteStr =
    """
    v0.0.0.1
    1. If you need a draw for a mechanic or notice any issues, @ me on DC or DM me.
    Duckmen
    """;

    const string UpdateInfo =
    """
        v0.0.0.1
    """;

    private const string Name = "LV.90 The Final Day";
    private const string Version = "0.0.0.1";
    private const string DebugVersion = "a";

    private const bool Debugging = true;

    private static readonly List<string> Role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
    private static readonly Vector3 Center = new Vector3(100, 0, 100);

    [UserSetting("Language")]
    public Language language { get; set; } = Language.Chinese;

    [UserSetting("Draw opacity â€” higher value = more visible")]
    public static float ColorAlpha { get; set; } = 1f;

    [UserSetting("Text Banner Toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = true;

    [UserSetting("Guide Arrow Toggle")]
    public bool isLead { get; set; } = true;

    [UserSetting("Debug toggle, please turn off for non-development use")]
    public bool isDebug { get; set; } = false;

    public enum Language
    {
        Chinese,
        English
    }

    public int HubrisTTSCount = 0;
    private readonly object HubrisLock = new object();

    private List<ulong> tetherRecordList = new List<ulong>();

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Log.Debug($"[DEBUG] {str}");
    }

    public void Init(ScriptAccessory sa)
    {
        sa.Log.Debug($"Script {Name} v{Version}{DebugVersion} initialized.");
        sa.Method.RemoveDraw(".*");
        HubrisTTSCount = 0;
        tetherRecordList.Clear();
    }

    [ScriptMethod(name: "Elegeia", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(26156|26242|26206)$"])]
    public void Elegeia(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Telomania", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(26207)$"])]
    public void Telomania(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Multi AOEs" : "Multi AOEs";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Stellar Collision", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(26158)$"])]
    public void StellarCollision(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(30), 6700, $"StellarCollision-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Galaxie", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(27754)$"])]
    public void Galaxie(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Knockback from Center" : "Knockback from Center";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 6700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = "Galaxie";
        dp.Color = sa.Data.DefaultSafeColor;
        dp.Owner = sa.Data.Me;
        dp.TargetPosition = ev.SourcePosition;
        dp.Rotation = float.Pi;
        dp.Scale = new Vector2(2, 13);
        dp.DestoryAt = 4700;
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "Elenchos Inside", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(26180)$"])]
    public void ElenchosInside(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(14, 40), 5700, $"Elenchos Inside-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Elenchos Outside", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(26179)$"])]
    public void ElenchosOutside(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(13, 40), 5700, $"Elenchos Outside-{ev.SourceId}",
            sa.Data.DefaultDangerColor, offset: new Vector3(0, 0, 20));
    }

    [ScriptMethod(name: "Elenchos Mobs", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(26174)$"])]
    public void ElenchosMobs(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(5, 40), 4700, $"Elenchos Mobs-{ev.SourceId}",
            sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Pharmakon", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(26188)$"])]
    public void Pharmakon(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(6), 1700, $"Pharmakon-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Dead Star", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(24142)$"])]
    public void DeadStar(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(6), 2700, $"Dead Star-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Hubris", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(26195)$"])]
    public async void Hubris(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(6), 4700, $"Hubris-{ev.SourceId}", 
            new Vector4(1, 0, 0, ColorAlpha), scaleByTime: false);

        lock (HubrisLock)
        {
            if (ev.TargetId == sa.Data.Me)
            {
                string msg = language == Language.Chinese ? "AOE tankbuster â€” Stay away from party" : "AOE tankbuster â€” Stay away from party";
                if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
                if (isTTS) sa.Method.EdgeTTS($"{msg}");
            }
            else if (HubrisTTSCount == 0)
            {
                string msg = language == Language.Chinese ? "Avoid AOE tankbuster" : "Avoid AOE tankbuster";
                if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
                if (isTTS) sa.Method.EdgeTTS($"{msg}");
            }

            HubrisTTSCount++;
        }

        await Task.Delay(4700);
        HubrisTTSCount = 0;
    }

    [ScriptMethod(name: "Tether Record", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(00A6)$"], userControl: false)]
    public void TetherRecord(Event ev, ScriptAccessory sa)
    {
        if (tetherRecordList.Contains(ev.TargetId)) return;
        tetherRecordList.Add(ev.TargetId);
        if (isDebug) sa.Log.Debug($"Add {ev.TargetId} to List");
    }

    [ScriptMethod(name: "Fatalism", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(26162)$"])]
    public void Fatalism(Event ev, ScriptAccessory sa)
    {
        if (isDebug) sa.Log.Debug($"List: {tetherRecordList[0]}, List1: {tetherRecordList}");
        if (tetherRecordList.Count == 1)
        {
            DrawHelper.DrawCircleObject(sa, tetherRecordList[0], new Vector2(30), 8700,
                $"Fatalism-{ev.SourceId}", sa.Data.DefaultDangerColor, delay: 11000);
        }

        if (tetherRecordList.Count == 2)
        {
            DrawHelper.DrawCircleObject(sa, tetherRecordList[1], new Vector2(30), 10500,
                $"Fatalism-{ev.SourceId}", sa.Data.DefaultDangerColor, delay: 9000);
        }
    }

    private List<Vector3> EpigonoiVectors = new List<Vector3>
    {
        new Vector3(86.64f, 0.00f, 86.55f),
        new Vector3(113.76f, -0.00f, 86.33f),
        new Vector3(113.41f, 0.00f, 113.47f),
        new Vector3(86.73f, 0.00f, 113.21f)
    };

    [ScriptMethod(name: "Epigonoi", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(26182)$"], suppress: 3000)]
    public void Epigonoi(Event ev, ScriptAccessory sa)
    {
        for (int i = 0; i < EpigonoiVectors.Count; i++)
        {
            DrawHelper.DrawCircle(sa, EpigonoiVectors[i], new Vector2(1f), 20000, $"Epigonoi: {EpigonoiVectors[i]}", color: sa.Data.DefaultSafeColor,
                scaleByTime: false);
        }
    }

    [ScriptMethod(name: "Crash", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(26199)$"])]
    public void Crash(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(15f), 8700, $"Crash: {ev.EffectPosition}", color: sa.Data.DefaultDangerColor,
            scaleByTime: true);
    }

    [ScriptMethod(name: "Ultimate Fate", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(27481)$"])]
    public void UltimateFate(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Tank LB" : "Tank LB";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }
}

#region Function Libraries
// All helper classes (EventExtensions, IbcHelper, MathTools, IndexHelper, DrawTools, MarkerHelper, SpecialFunction, NamazuHelper, DrawHelper, ExtensionMethods, ExtensionVisibleMethod) are identical to previous files
#endregion

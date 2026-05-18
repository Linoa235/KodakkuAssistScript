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
using static FFXIVClientStructs.FFXIV.Client.Graphics.Render.Skeleton;

namespace Veever.EndWalker.the_Dead_Ends;

[ScriptType(name: Name, territorys: [973], guid: "d66d8b1a-44a2-4752-a613-4c52abf81c7b",
    version: Version, author: "Linoa235", note: NoteStr, updateInfo: UpdateInfo)]

public class the_Dead_Ends
{
    const string NoteStr =
    """
    v0.0.0.1
    1. If you need a draw for a mechanic or notice any issues, @ me on DC or DM me.
    2. Boss 2 knockback distance is uncertain due to lack of examples.  
       If you have ARR without anti-knockback used, please send it to me.
    Duckmen
    """;

    const string UpdateInfo =
    """
        v0.0.0.1
    """;

    private const string Name = "LV.90 The Dead Ends";
    private const string Version = "0.0.0.1";
    private const string DebugVersion = "a";

    private const bool Debugging = true;

    private static readonly List<string> Role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
    private static readonly Vector3 Center = new Vector3(100, 0, 100);

    private static uint BossDataId = 9708;

    [UserSetting("Language")]
    public Language language { get; set; } = Language.Chinese;

    [UserSetting("Draw opacity â€” higher value = more visible")]
    public static float ColorAlpha { get; set; } = 1f;

    [UserSetting("Text Banner Toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = true;

    [UserSetting("Auto anti-knockback")]
    public bool useaction { get; set; } = true;

    [UserSetting("Guide Arrow Toggle")]
    public bool isLead { get; set; } = true;

    [UserSetting("Debug toggle, please turn off for non-development use")]
    public bool isDebug { get; set; } = false;

    public enum Language
    {
        Chinese,
        English
    }

    private Dictionary<ulong, ulong> _tethersDict = new();
    private static bool _initHint = false;

    private readonly object CountLock = new object();
    
    // Boss1 related variables
    public bool isinRing = false;
    public bool isNorthWind = true;
    List<Vector3> vector3s = new();

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    public void Init(ScriptAccessory sa)
    {
        sa.Log.Debug($"Script {Name} v{Version}{DebugVersion} initialized.");
        sa.Method.RemoveDraw(".*");

        isinRing = false;
        isNorthWind = true;
        vector3s.Clear();
    }

    private static IGameObject? GetBossObject(ScriptAccessory sa)
    {
        return sa.GetByDataId(BossDataId).FirstOrDefault();
    }

    #region Trash Mobs
    [ScriptMethod(name: "---- Mobs ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void mobs(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "Terminal Bloom", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26223"])]
    public void TerminalBloom(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2700, $"TerminalBloom-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "TerminalBloom Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:26223"], userControl: false)]
    public void TerminalBloomClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"TerminalBloom-{ev.SourceId}");
    }

    [ScriptMethod(name: "Plague Fang", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28306"])]
    public void PlagueFang(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(6f), 90, 2700, $"PlagueFang-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Plague Fang Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:28306"], userControl: false)]
    public void PlagueFangClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"PlagueFang-{ev.SourceId}");
    }

    [ScriptMethod(name: "Photon Burst", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28273"])]
    public void PhotonBurst(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(5f), 2700, $"PhotonBurst-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Photon Burst Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:28273"], userControl: false)]
    public void PhotonBurstClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"PhotonBurst-{ev.SourceId}");
    }
    #endregion

    #region Boss1
    [ScriptMethod(name: "---- Boss1 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void Boss1(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "Medicine Field", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43798"])]
    public void MedicineField(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Heavy AOE" : "Heavy AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Cough Up", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25918"])]
    public void CoughUp(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 3700, $"CoughUp-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Certain Solitude", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:28349"])]
    public void CertainSolitude(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "At least 2 players Stack" : "At least 2 players Stack";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
    }

    [ScriptMethod(name: "Wave of Nausea", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28347"])]
    public async void WaveofNausea(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Dynamo (In)" : "Dynamo (In)";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
        
        isinRing = true;
        DebugMsg($"isinRing {isinRing}", sa);
        await Task.Delay(10000);
        isinRing = false;
        DebugMsg($"isinRing {isinRing}", sa);
    }

    [ScriptMethod(name: "WindCheck", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:regex:^(00020080|01000010)$", "Index:regex:^(0000002B)$"], userControl: false)]
    public void WindCheck(Event ev, ScriptAccessory sa)
    {
        DebugMsg($" WindCheck {ev.Id} {ev.Index().ToString("X")}", sa);
        if (ev.Id == 00020080)
        {
            isNorthWind = true;
        }
        else if (ev.Id == 01000010)
        {
            isNorthWind = false;
        }
        DebugMsg($" isNorthWind: {isNorthWind}", sa);
    }

    [ScriptMethod(name: "Necrotic Fluid", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25919)$"])]
    public async void NecroticFluid(Event ev, ScriptAccessory sa)
    {
        DebugMsg($"isNorthWind {isNorthWind}", sa);
        Vector3 pos = ev.EffectPosition;
        DrawHelper.DrawCircle(sa, pos, new Vector2(6f), 6200, $"NecroticFluid-{ev.SourceId}", sa.Data.DefaultDangerColor);
        var delayTime = 6200;
        for (int i = 0; i < 6; i++)
        {
            DebugMsg($"pos {pos}", sa);
            if (pos.Z >= -157.5f || pos.Z <= -198.47f) break;
            if (isNorthWind)
            {
                pos.Z += 6;
            }
            else
            {
                pos.Z -= 6;
            }
            DrawHelper.DrawCircle(sa, pos, new Vector2(6f), 2000, $"NecroticFluid-{ev.SourceId}", sa.Data.DefaultDangerColor, delay: delayTime, scaleByTime: false);
            delayTime += 1400;
        }

        List<Vector3> safePosList = new List<Vector3>
        {
            new Vector3(272.42f, 501.01f, -185.33f),
            new Vector3(260.78f, 501.01f, -170.60f),
        };

        List<Vector3> safeRingPosList = new List<Vector3>
        {
            new Vector3(266.47f, 501.01f, -183.05f),
            new Vector3(266.47f, 501.01f, -172.84f)
        };

        await Task.Delay(2000);
        if (isinRing)
        {
            DebugMsg("In ring", sa);
            if (isNorthWind)
            {
                DebugMsg("north wind safe ring draw", sa);
                if (isLead) DrawHelper.DrawDisplacement(sa, safeRingPosList[0], new Vector2(1f), 4700, "RingNorthWind", color: sa.Data.DefaultSafeColor);
                if (isLead) DrawHelper.DrawDisplacement(sa, safePosList[0], new Vector2(1f), 4700, "NorthWindSafePos", color: sa.Data.DefaultSafeColor, delay: 4300);
            }
            else
            {
                DebugMsg("South wind safe ring draw", sa);
                if (isLead) DrawHelper.DrawDisplacement(sa, safeRingPosList[1], new Vector2(1f), 4700, "RingSouthWind", color: sa.Data.DefaultSafeColor);
                if (isLead) DrawHelper.DrawDisplacement(sa, safePosList[1], new Vector2(1f), 4700, "SouthWindSafePos", color: sa.Data.DefaultSafeColor, delay: 4300);
            }
        }
        else
        {
            DebugMsg("not in ring", sa);
            if (isNorthWind)
            {
                DebugMsg("north wind safe draw", sa);
                if (isLead) DrawHelper.DrawDisplacement(sa, safePosList[0], new Vector2(1f), 4700, "NorthWindSafePos", color: sa.Data.DefaultSafeColor);
            }
            else
            {
                DebugMsg("South wind safe draw", sa);
                if (isLead) DrawHelper.DrawDisplacement(sa, safePosList[1], new Vector2(1f), 4700, "SouthWindSafePos", color: sa.Data.DefaultSafeColor);
            }
        }
    }

    [ScriptMethod(name: "Pox Flail", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25920"])]
    public void PoxFlail(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Tankbuster" : "Tankbuster";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Blighted Water", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25922)$"])]
    public void BlightedWater(Event ev, ScriptAccessory sa)
    {
        string tname = ev["TargetName"]?.ToString() ?? "Unknown Target";
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Stack" : "Stack";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
        else
        {
            string msg = language == Language.Chinese ? $"Stack with {tname}" : $"Stack with {tname}";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(6), 4900, "BlightedWater", color: sa.Data.DefaultSafeColor, scaleByTime: false);
    }
    #endregion

    #region Boss2
    [ScriptMethod(name: "---- Boss2 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void Boss2(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25936"])]
    public void boss2AOE(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Small-bore Laser", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28352"])]
    public void SmallboreLaser(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(4, 20), 4700, $"SmallboreLaser-{ev.SourceId}");
    }

    [ScriptMethod(name: "Spread Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(28359|25928)$"])]
    public void SpreadNotify(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Spread" : "Spread";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(6), 4700, "Spread Notify", color: new Vector4(1,0,1,ColorAlpha), scaleByTime: true); 
    }

    [ScriptMethod(name: "Peacefire", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25934)$"])]
    public void Peacefire(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(10), 7000, $"Peacefire-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Peacefire Cancel", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:regex:^(25934)$"])]
    public void PeacefireCancel(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Peacefire-{ev.SourceId}");
    }

    [ScriptMethod(name: "Eclipsing Exhaust", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25931)$"])]
    public void EclipsingExhaust(Event ev, ScriptAccessory sa)
    {
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = "EclipsingExhaust-knock";
        dp.Color = sa.Data.DefaultSafeColor;
        dp.Owner = sa.Data.Me;
        dp.TargetPosition = ev.EffectPosition;
        dp.Rotation = float.Pi;
        dp.Scale = new Vector2(2, 11f);
        dp.DestoryAt = 4700;
        if (isLead) sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        if (useaction) sa.Method.UseAction(sa.Data.Me, 7559);
        if (useaction) sa.Method.UseAction(sa.Data.Me, 7548);
    }

    [ScriptMethod(name: "Tank Buster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25935)$"])]
    public void Boss2TankBuster(Event ev, ScriptAccessory sa)
    {
        string tname = ev["TargetName"]?.ToString() ?? "Unknown Target";
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Laser buster" : "Laser buster";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
        else
        {
            string msg = language == Language.Chinese ? "Avoid buster Laser" : "Avoid buster Laser";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }

        DrawHelper.DrawRectObjectTarget(sa, ev.SourceId, ev.TargetId, new Vector2(10, 46), 5000, "Boss2TankBuster", color: sa.Data.DefaultDangerColor);
    }
    #endregion

    #region Boss3
    [ScriptMethod(name: "---- Boss3 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void Boss3(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25950"])]
    public void boss3AOE(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Pity", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25949"])]
    public void Pity(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Tankbuster" : "Tankbuster";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Lamellar Light", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25939)$"])]
    public void LamellarLight(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(15), 6000, $"LamellarLight-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha), scaleByTime: true);
    }

    [ScriptMethod(name: "Butterfly Lamellar Light", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25942)$"])]
    public void ButterflyLamellarLight(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(2, 30), 3000, $"ButterflyLamellarLight-{ev.SourceId}", color: new Vector4(1, 0, 0, ColorAlpha));
    }

    [ScriptMethod(name: "Benevolence", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25946)$"])]
    public void Benevolence(Event ev, ScriptAccessory sa)
    {
        string tname = ev["TargetName"]?.ToString() ?? "Unknown Target";
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Stack" : "Stack";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 5100, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
        else
        {
            string msg = language == Language.Chinese ? $"Stack with {tname}" : $"Stack with {tname}";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 5100, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(6), 5100, "Benevolence", color: sa.Data.DefaultSafeColor, scaleByTime: false);
    }

    [ScriptMethod(name: "Loving Embrace", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25943|25944)$"])]
    public void LovingEmbrace(Event ev, ScriptAccessory sa)
    {
        var rot = float.Pi / 2;
        if (ev.ActionId == 25943) rot = -float.Pi / 2;
        DrawHelper.DrawFanObject(sa, ev.SourceId, rot, new Vector2(45f), 180, 6700, $"LovingEmbrace-{ev.SourceId}", scaleByTime: false, color: new Vector4(0, 1, 1, 1));
    }

    [ScriptMethod(name: "Still Embrace", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(25948)$"])]
    public void StillEmbrace(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Spread" : "Spread";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 5100, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(6), 5100, "StillEmbrace Notify", color: new Vector4(1, 0, 1, 1), scaleByTime: true);
    }
    #endregion
}

#region Function Libraries
// All helper classes (EventExtensions, IbcHelper, MathTools, IndexHelper, DrawTools, MarkerHelper, SpecialFunction, NamazuHelper, DrawHelper, ExtensionMethods, ExtensionVisibleMethod) are identical to previous files
#endregion
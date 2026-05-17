using Dalamud.Utility.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
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

namespace Veever.DawnTrail.San_d_Oria_The_Second_Walk;

[ScriptType(name: "LV100 San d'Oria The Second Walk", territorys: [], guid: "f15ebb97-7874-4c3b-8c97-4b1d2beec0c4", version: "0.0.0.1", Author: "Linoa235")]

public class San_d_Oria_The_Second_Walk
{
    const string NoteStr =
    """
    v0.0.0.4
    1. If you need a draw for a mechanic or notice any issues, @ me on DC or DM me.
    2. Boss 1 may have an issue where the double-arm Rect AOE lasts a bit too long.  
       (Not a big deal so I'm too lazy to fix it.)
    3. Boss 3 does not auto-use anti-knockback for tankbuster knockbacks. 
    4. In the initial version, Boss 2's Energy Ray (reflect laser) and Boss 4's Duplicate may have drawing issues (wrong/missing).  
       If you encounter this, please provide an ARR for feedback on DC.
    5. Boss 2's floor drawing has a small issue â€” just ignore it.
    6. If the game crashes, please report it in the dc channel.  
       If no ARR is available, let me know roughly which mechanic and the time it happened, and provide your [dalamud.log].
    Duckmen
    """;

    const string UpdateInfo =
    """
        v0.0.0.4
        Added drawing for mobs before Boss 2
    """;

    private const string Name = "LV.100 San d'Oria: The Second Walk";
    private const string Version = "0.0.0.4";
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

    [UserSetting("Auto anti-knockback")]
    public bool useAntiKnockback { get; set; } = true;

    [UserSetting("Guide Arrow Toggle")]
    public bool isLead { get; set; } = true;

    [UserSetting("Target Marker Toggle")]
    public bool isMark { get; set; } = true;

    [UserSetting("Local Target Marker Toggle (ON = local only, OFF = party shared)")]
    public bool LocalMark { get; set; } = true;

    [UserSetting("Debug toggle, please turn off for non-development use")]
    public bool isDebug { get; set; } = false;

    public enum Language
    {
        Chinese,
        English
    }

    private static bool _initHint = false;

    private int MoontideFontCount = 0;
    public int SynchronizedStrikeCount = 0;

    private readonly object MoontideFontLock = new object();
    private readonly object CountLock = new object();
    private readonly object SynchronizedStrikeLock = new object();
    private readonly object Boss1QuadraRecordDataLock = new object();
    private readonly object TetherDataLock = new object();

    private Dictionary<uint, (ulong, Vector3)> tetherData = new Dictionary<uint, (ulong, Vector3)>();
    private Dictionary<uint, (ulong, Vector3, int)> Boss1QuadraRecordData = new Dictionary<uint, (ulong, Vector3, int)>();

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }
     
    public void Init(ScriptAccessory sa)
    {
        sa.Log.Debug($"Script {Name} v{Version}{DebugVersion} initialized.");
        sa.Method.RemoveDraw(".*");

        MoontideFontCount = 0;
        SynchronizedStrikeCount = 0;

        isSecondTankBuster = false;
        duplicatePhase2 = false;
        hasKnockback = false;
        frameworkRegistered = false;

        sa.Method.ClearFrameworkUpdateAction(this);
        Boss1QuadraRecordGuid = "";

        tetherData.Clear();
        Boss1QuadraRecordData.Clear();
        surfaceMissiles.Clear();
        boss3TankBusterList.Clear();
        boss4TankBusterList.Clear();
    }

    #region Trash Mobs
    [ScriptMethod(name: "---- Mobs ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void mobs(Event ev, ScriptAccessory sa)
    {
    }

    #region Before Boss1
    [ScriptMethod(name: "Dust Cloud", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43554"])]
    public void DustCloud(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(10f), 120, 2700, $"DustCloud-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Dust Cloud Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43554"], userControl: false)]
    public void DustCloudClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"DustCloud-{ev.SourceId}");
    }
    #endregion

    #region Before Boss2
    [ScriptMethod(name: "Electroswipe", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43559"])]
    public void Electroswipe(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(50f), 120, 5700, $"Electroswipe-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Electroswipe Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43559"], userControl: false)]
    public void ElectroswipeClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Electroswipe-{ev.SourceId}");
    }

    [ScriptMethod(name: "Interrupt Cleanup", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^75(38|51)$"], userControl: false)]
    public void destoryCancelAction(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($".*{ev.TargetId}");
    }
    #endregion

    #region Before Boss3
    [ScriptMethod(name: "Paralyze III", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43570"])]
    public void ParalyzeIII(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 5700, $"ParalyzeIII-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Paralyze III Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43570"], userControl: false)]
    public void ParalyzeIIIClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"ParalyzeIII-{ev.SourceId}");
    }

    [ScriptMethod(name: "Sucker", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43791"])]
    public void Sucker(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(8f), 6000, $"Sucker-{ev.SourceId}", scaleByTime: false, color: new Vector4(1, 0, 0, ColorAlpha));
        if (useAntiKnockback)
        {
            sa.Method.UseAction(sa.Data.Me, 7559);
            sa.Method.UseAction(sa.Data.Me, 7548);
        }
    }

    [ScriptMethod(name: "Impact Roar", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43566"])]
    public void ImpactRoar(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Banish III & Catapult", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43562|43568|43567)$"])]
    public void BanishIIICatapult(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2700, $"BanishIIIorCatapult-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Banish III & Catapult Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:regex:^(43562|43568|43567)$"], userControl: false)]
    public void BanishIIICatapultClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"BanishIIIorCatapult-{ev.SourceId}");
    }

    [ScriptMethod(name: "Mighty Shatter", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43575"])]
    public void MightyShatter(Event ev, ScriptAccessory sa)
    {
        string msg = "";
        if (isMark)
        {
            msg = language == Language.Chinese ? "Interrupt marked target" : "Interrupt marked target";
        }
        else
        {
            msg = language == Language.Chinese ? "Interrupt Alkyoneus" : "Interrupt Alkyoneus";
        }

        if (isText) sa.Method.TextInfo($"{msg}", duration: 3000, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
        if (isMark) sa.Method.Mark((uint)ev.TargetId, KodakkuAssist.Module.GameOperate.MarkType.Bind1, LocalMark);
    }

    [ScriptMethod(name: "Power Attack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43569"])]
    public void PowerAttack(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(20f), 120, 2700, $"PowerAttack-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Power Attack Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43569"], userControl: false)]
    public void PowerAttackClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"PowerAttack-{ev.SourceId}");
    }
    #endregion
    #endregion

    #region Boss1
    private static IGameObject? GetBossObject(ScriptAccessory sa, uint BossDataId)
    {
        return sa.GetByDataId(BossDataId).FirstOrDefault();
    }
    private uint westHandDataId = 18753;
    private uint eastHandDataId = 18754;

    [ScriptMethod(name: "---- Boss1 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void Boss1(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "Stonega IV", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44490"])]
    public void StonegaIV(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Synchronized Smite", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44444|44443)$"])]
    public void SynchronizedSmite(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(32, 60), 4700, $"SynchronizedSmite-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(0, 0, 30));
    }

    [ScriptMethod(name: "Crimson Riddle", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45045|45044)$"])]
    public void CrimsonRiddle(Event ev, ScriptAccessory sa)
    {
        float rot = 0;
        if (ev.ActionId == 45045) rot = float.Pi;
        DrawHelper.DrawFanObject(sa, ev.SourceId, rot, new Vector2(30f), 180, 4700, $"CrimsonRiddle-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    #region Four Gods
    [ScriptMethod(name: "Boss1_4God Clear", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44414)$"], userControl: false)]
    public void Boss1_4godClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.UnregistFrameworkUpdateAction(Boss1QuadraRecordGuid);
        frameworkRegistered = false;

        lock (Boss1QuadraRecordDataLock)
        {
            Boss1QuadraRecordData.Clear();
        }
        lock (TetherDataLock)
        {
            tetherData.Clear();
        }
    }

    #region Azure Dragon (Seiryu)
    [ScriptMethod(name: "Eastwind Wheel", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44416|44417)$"])]
    public void EastwindWheel(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(18, 60), 7700, $"EastwindWheelRect-{ev.SourceId}", new Vector4(0, 1, 1, ColorAlpha));

        if (ev.ActionId == 44417)
        {
            DrawHelper.DrawFanObject(sa, ev.SourceId, float.Pi / 4, new Vector2(65f), 90, 7700, $"EastwindWheelFan-{ev.SourceId}", new Vector4(0, 1, 1, ColorAlpha), offset: new Vector3(9, 0, 0));
        }
        else if (ev.ActionId == 44416)
        {
            DrawHelper.DrawFanObject(sa, ev.SourceId, -float.Pi / 4, new Vector2(65f), 90, 7700, $"EastwindWheelFan-{ev.SourceId}", new Vector4(0, 1, 1, ColorAlpha), offset: new Vector3(-9, 0, 0));
        }
    }
    #endregion

    #region White Tiger (Byakko)
    [ScriptMethod(name: "Gloaming Gleam", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44431)$"])]
    public void GloamingGleam(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(12, 50), 3000, $"GloamingGleam-{ev.SourceId}", new Vector4(1, 1, 0, ColorAlpha));
    }

    [ScriptMethod(name: "Razor Fang", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44432)$"])]
    public void RazorFang(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(20f), 4500, $"RazorFang-{ev.SourceId}", new Vector4(1, 1, 0, ColorAlpha), scaleByTime: false);
    }
    #endregion

    #region Black Tortoise (Genbu)
    [ScriptMethod(name: "Moontide Font", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44421|44422)$"])]
    public void MoontideFont(Event ev, ScriptAccessory sa)
    {
        lock (MoontideFontLock)
        {
            MoontideFontCount++;
            DebugMsg($"MoontideFontCount: {MoontideFontCount}", sa);
        }

        int duration = int.TryParse(ev["DurationMilliseconds"]?.ToString(), out var d) ? d : 8000;
        var delay = 0;

        DebugMsg($"time: {duration}", sa);

        if (MoontideFontCount >= 10)
        {
            delay = 3000;
            duration -= 3000;
        }

        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(9f), duration, $"MoontideFont-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: delay);
    }

    [ScriptMethod(name: "Midwinter March", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44337)$"])]
    public async void MidwinterMarch(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Stay near danger zone, then move in" : "Stay near danger zone, then move in";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(12f), 6700, $"MidwinterMarch-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false);
        await Task.Delay(7000);

        string msg1 = language == Language.Chinese ? "Move in (Dynamo)" : "Move in (Dynamo)";
        if (isText) sa.Method.TextInfo($"{msg1}", duration: 3500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg1}");

        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(12f), 3500, $"MidwinterMarchDountSafe-{ev.SourceId}", sa.Data.DefaultSafeColor, scaleByTime: false);
        DrawHelper.DrawDount(sa, ev.EffectPosition, new Vector2(60f), new Vector2(12f), 3500, $"MidwinterMarchDount-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false);
    }

    [ScriptMethod(name: "Dead Wringer", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44439)$"])]
    public void DeadWringer(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Move in (Dynamo)" : "Move in (Dynamo)";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 3500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(14f), 4700, $"DeadWringerDountSafe-{ev.SourceId}", sa.Data.DefaultSafeColor, scaleByTime: false);
        DrawHelper.DrawDount(sa, ev.EffectPosition, new Vector2(60f), new Vector2(14f), 4700, $"DeadWringerDount-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false);
    }
    #endregion

    #region Vermilion Bird (Suzaku)
    [ScriptMethod(name: "Arm of Purgatory", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44796|44794)$"])]
    public void ArmofPurgatory(Event ev, ScriptAccessory sa)
    {
        if (ev.ActionId == 44796)
        {
            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(15f), 5200, $"ArmofPurgatory-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha), scaleByTime: false);
        }
        else
        {
            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(15f), 10700, $"ArmofPurgatory-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha), scaleByTime: false);
        }
    }

    [ScriptMethod(name: "Vermilion Flight", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44795)$"])]
    public void VermilionFlight(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20, 60), 7700, $"VermilionFlight-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }
    #endregion
    #endregion

    [ScriptMethod(name: "Tether Record 0156", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(0156)$"], userControl: false)]
    public async void tetherRecorder0156(Event ev, ScriptAccessory sa)
    {
        DebugMsg($"0156 record {ev.SourceId}", sa);
        lock (TetherDataLock)
        {
            tetherData[ev.Id] = (ev.SourceId, ev.SourcePosition);
        }

        await Task.Delay(4700);
        lock (TetherDataLock)
        {
            tetherData.Clear();
        }
    }

    [ScriptMethod(name: "Striking Right", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44435|44462)$"])]
    public void StrikingRight(Event ev, ScriptAccessory sa)
    {
        Vector3 tetherPosition;
        lock (TetherDataLock)
        {
            if (!tetherData.ContainsKey(0156))
            {
                DebugMsg($"Tether data for 0156 not found", sa);
                return;
            }
            tetherPosition = tetherData[0156].Item2;
        }

        if (ev.ActionId == 44435)
        {
            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(10f), 4700, $"StrikingRight-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false);
            DrawHelper.DrawCircle(sa, tetherPosition, new Vector2(30f), 4700, $"StrikingRightBig-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false);
        }
        else
        {
            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(10f), 4700, $"StrikingRight-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: 3000);
            DrawHelper.DrawCircle(sa, tetherPosition, new Vector2(30f), 4700, $"StrikingRightBig-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: 3000);
        }
    }

    [ScriptMethod(name: "Striking Left", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44436|44463)$"])]
    public void StrikingLeft(Event ev, ScriptAccessory sa)
    {
        Vector3 tetherPosition;
        lock (TetherDataLock)
        {
            if (!tetherData.ContainsKey(0156))
            {
                DebugMsg($"Tether data for 0156 not found", sa);
                return;
            }
            tetherPosition = tetherData[0156].Item2;
        }

        if (ev.ActionId == 44436)
        {
            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(10f), 4700, $"StrikingLeft-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false);
            DrawHelper.DrawCircle(sa, tetherPosition, new Vector2(30f), 4700, $"StrikingLeftBig-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false);
        }
        else
        {
            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(10f), 4700, $"StrikingLeft-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: 3000);
            DrawHelper.DrawCircle(sa, tetherPosition, new Vector2(30f), 4700, $"StrikingLeftBig-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: 3000);
        }
    }

    [ScriptMethod(name: "Smiting Right Large", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44441)$"])]
    public void SmitingRightLarge(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(30f), 4700, $"SmitingRightLarge-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false);
    }

    [ScriptMethod(name: "Double Wringer", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44445)$"])]
    public void DoubleWringer(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(14f), 9700, $"DoubleWringer-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false);
    }

    [ScriptMethod(name: "Synchronized Strike", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44464)$"])]
    public async void SynchronizedStrike(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(10, 60), 8200, $"SynchronizedStrike-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha), offset: new Vector3(0, 0, 30));
        lock (SynchronizedStrikeLock)
        {
            SynchronizedStrikeCount++;
            DebugMsg($"SynchronizedStrikeCount: {SynchronizedStrikeCount}", sa);
        }

        if (SynchronizedStrikeCount == 2)
        {
            DebugMsg($"SynchronizedStrikeCount == 2", sa);
            var westHand = GetBossObject(sa, westHandDataId);
            var eastHand = GetBossObject(sa, eastHandDataId);
            if (westHand == null || eastHand == null) return;

            DrawHelper.DrawRectObjectNoTarget(sa, westHand.EntityId, new Vector2(32, 60), 4700, $"SynchronizedStrike2-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(0, 0, 30), delay: 12000);
            DrawHelper.DrawRectObjectNoTarget(sa, eastHand.EntityId, new Vector2(32, 60), 4700, $"SynchronizedStrike2-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(0, 0, 30), delay: 12000);
            await Task.Delay(5000);
            SynchronizedStrikeCount = 0;
        }
    }

    private string Boss1QuadraRecordGuid = "";
    public bool frameworkRegistered = false;

    [ScriptMethod(name: "Boss1 Quadra Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44433)$"])]
    public void Boss1QuRecord(Event ev, ScriptAccessory sa)
    {
        lock (Boss1QuadraRecordDataLock)
        {
            Boss1QuadraRecordData.Clear();
        }

        if (!string.IsNullOrEmpty(Boss1QuadraRecordGuid))
        {
            sa.Method.UnregistFrameworkUpdateAction(Boss1QuadraRecordGuid);
        }

        DebugMsg($"Register new framework for Boss1QuRecord", sa);
        sa.Log.Debug($"Register new framework for Boss1QuRecord");
        Boss1QuadraRecordGuid = sa.Method.RegistFrameworkUpdateAction(() =>
        {
            try
            {
                Dictionary<uint, (ulong, Vector3, int)> localData;
                lock (Boss1QuadraRecordDataLock)
                {
                    if (Boss1QuadraRecordData.Count != 2) return;
                    localData = new Dictionary<uint, (ulong, Vector3, int)>(Boss1QuadraRecordData);
                    Boss1QuadraRecordData.Clear();
                }

                var westHand = GetBossObject(sa, westHandDataId);
                var eastHand = GetBossObject(sa, eastHandDataId);
                if (westHand == null || eastHand == null) return;

                foreach (var kv in localData)
                {
                    switch (kv.Value.Item3)
                    {
                        case 0:
                            HandleBoss1Quadra(sa, kv.Key, kv.Value.Item1, kv.Value.Item2, westHand, eastHand, 0);
                            break;
                        case 1:
                            HandleBoss1Quadra(sa, kv.Key, kv.Value.Item1, kv.Value.Item2, westHand, eastHand, 6500);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                sa.Log.Error($"Framework callback error: {ex.Message}");
                lock (Boss1QuadraRecordDataLock)
                {
                    Boss1QuadraRecordData.Clear();
                }
            }
        });
        frameworkRegistered = true;
    }

    public void HandleBoss1Quadra(ScriptAccessory sa, uint actionId, ulong sid, Vector3 efpos, IGameObject? westHand, IGameObject? eastHand, int delay)
    {
        DebugMsg($"HandleBoss1Quadra In, ActionId: {actionId}", sa);
        sa.Log.Debug($"HandleBoss1Quadra In, ActionId: {actionId}");
        switch (actionId)
        {
            case 44462:
                DebugMsg($"44462 case", sa);
                sa.Log.Debug($"HandleBoss1Quadra In, ActionId: {actionId}");
                DrawHelper.DrawCircle(sa, efpos, new Vector2(10f), 4700, $"44462s-{efpos}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: delay);

                if (westHand != null)
                {
                    DrawHelper.DrawCircle(sa, westHand.Position, new Vector2(30f), 6500, $"44462L-{westHand.EntityId}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: delay);
                }
                break;
            case 44463:
                DebugMsg($"44463 case", sa);
                sa.Log.Debug($"HandleBoss1Quadra In, ActionId: {actionId}");
                DrawHelper.DrawCircle(sa, efpos, new Vector2(10f), 4700, $"44463s-{efpos}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: delay);

                if (eastHand != null)
                {
                    DrawHelper.DrawCircle(sa, eastHand.Position, new Vector2(30f), 6500, $"44463L-{eastHand.EntityId}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: delay);
                }
                break;
            case 44461:
                DebugMsg($"44461 case", sa);
                sa.Log.Debug($"HandleBoss1Quadra In, ActionId: {actionId}");
                DrawHelper.DrawCircle(sa, efpos, new Vector2(14f), 5000, $"44461Danger-{sid}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: delay);

                DrawHelper.DrawCircle(sa, efpos, new Vector2(14f), 4700, $"44461safe-{sid}", sa.Data.DefaultSafeColor, scaleByTime: false, delay: 5000 + delay);
                DrawHelper.DrawDount(sa, efpos, new Vector2(60f), new Vector2(14f), 4700, $"44461dount-{sid}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: 5000 + delay);
                break;
        }
    }

    public async void RemoveSynchronizedStrike2(ScriptAccessory sa)
    {
        await Task.Delay(1000);
        sa.Method.RemoveDraw($"SynchronizedStrike2.*");
    }

    [ScriptMethod(name: "Boss1 Action Monitor", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44462|44463|44464|44461)$"], userControl: false, suppress: 2000)]
    public void Boss1ActionMonitor(Event ev, ScriptAccessory sa)
    {
        DebugMsg($"RecordAction {ev.ActionId}", sa);
        sa.Log.Debug($"RecordAction {ev.ActionId}");

        lock (Boss1QuadraRecordDataLock)
        {
            var item3 = Boss1QuadraRecordData.Count;

            if (!Boss1QuadraRecordData.ContainsKey(ev.ActionId))
            {
                Boss1QuadraRecordData.Add(ev.ActionId, (ev.SourceId, ev.EffectPosition, item3));
                DebugMsg($"Boss1QuadraRecordData.Add post: ActionId:{ev.ActionId}; SourceId: {ev.SourceId}; EffectPosition: {ev.EffectPosition}, item3: {item3}", sa);
                sa.Log.Debug($"Boss1QuadraRecordData.Add post: ActionId:{ev.ActionId}; SourceId: {ev.SourceId}; EffectPosition: {ev.EffectPosition}, item3: {item3}");
            }
            else
            {
                DebugMsg($"ActionId {ev.ActionId} already exists in Boss1QuadraRecordData, skipping", sa);
                sa.Log.Debug($"ActionId {ev.ActionId} already exists in Boss1QuadraRecordData, skipping");
            }
        }
    }

    #region Sand Area
    [ScriptMethod(name: "Deadly Hold", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44466)$"])]
    public void DeadlyHold(Event ev, ScriptAccessory sa)
    {
        sa.Method.UnregistFrameworkUpdateAction(Boss1QuadraRecordGuid);
        frameworkRegistered = false;

        lock (Boss1QuadraRecordDataLock)
        {
            Boss1QuadraRecordData.Clear();
        }
        lock (TetherDataLock)
        {
            tetherData.Clear();
        }

        bool isTank = ExtensionMethods.IsTank(sa.Data.MyObject);
        if (isTank)
        {
            DebugMsg($"isTank: {isTank}", sa);
            string msg = language == Language.Chinese ? "Tanks take tower" : "Tanks take tower";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 8000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
        else
        {
            DebugMsg($"isTank: {isTank}", sa);
            string msg = language == Language.Chinese ? "Attack Arms" : "Attack Arms";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 8000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
    }
    #endregion

    [ScriptMethod(name: "Boss1 Clear", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:regex:^(18754)$"], userControl: false)]
    public void Boss1Clear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw(".*");
        MoontideFontCount = 0;
        SynchronizedStrikeCount = 0;

        sa.Method.ClearFrameworkUpdateAction(this);
        Boss1QuadraRecordGuid = "";
        frameworkRegistered = false;

        lock (Boss1QuadraRecordDataLock)
        {
            Boss1QuadraRecordData.Clear();
        }
        lock (TetherDataLock)
        {
            tetherData.Clear();
        }
        lock (SurfaceMissileLock)
        {
            surfaceMissiles.Clear();
        }
    }
    #endregion

    #region Boss2
    [ScriptMethod(name: "---- Boss2 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void Boss2(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44331"])]
    public void boss2AOE(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Tankbuster Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44305"])]
    public void boss2Tankbuster(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Targeted Buster" : "Targeted Buster";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
    }

    [ScriptMethod(name: "Energy Ray", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44338"])]
    public void EnergyRay(Event ev, ScriptAccessory sa)
    {
        var casterPos = ev.SourcePosition;
        var rotation = ev.SourceRotation;

        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(16f, 40f), 4700, $"EnergyRay-Initial-{ev.SourceId}", sa.Data.DefaultDangerColor);

        var initialPosMap = new Dictionary<Vector3, int[]>
        {
            { new Vector3(755f, 320, 816f), new int[] { 0, 1 } },
            { new Vector3(755f, 320, 800f), new int[] { 2, 3 } },
            { new Vector3(820f, 380, 784f), new int[] { 4 } },
            { new Vector3(820f, 380, 816f), new int[] { 5 } },
            { new Vector3(755f, 320, 784f), new int[] { 6, 7 } }
        };

        var aoeMap = new (Vector3 firstReflect, int angleFirst, Vector3 secondReflect, int angleSecond)[]
        {
            (new Vector3(725f, 320, 816f), 2, new Vector3(725f, 320, 784f), 1),
            (new Vector3(745f, 320, 816f), 2, new Vector3(745f, 320, 800f), 3),
            (new Vector3(745f, 320, 800f), 0, new Vector3(745f, 320, 816f), 3),
            (new Vector3(725f, 320, 800f), 2, new Vector3(725f, 320, 784f), 1),
            (new Vector3(810f, 380f, 784f), 0, Vector3.Zero, 0),
            (new Vector3(810f, 380f, 816f), 2, Vector3.Zero, 0),
            (new Vector3(745f, 320, 784f), 0, new Vector3(745f, 320, 816f), 3),
            (new Vector3(725f, 320, 784f), 0, new Vector3(725f, 320, 800f), 1)
        };

        var cardinalAngles = new float[] { 0f, float.Pi / 2, float.Pi, -float.Pi / 2 };

        var manaScreens = sa.Data.Objects.Where(obj => obj.DataId == 0x1EBE8B || obj.DataId == 0x1EBE8C).ToList();

        if (manaScreens.Count == 0) return;

        DebugMsg($"Found Screen, count: {manaScreens.Count}", sa);

        foreach (var posMapping in initialPosMap)
        {
            if (Vector3.Distance(casterPos, posMapping.Key) < 2f)
            {
                DebugMsg($"Caster position matched: {posMapping.Key}", sa);
                var indices = posMapping.Value;
                DebugMsg($"Possible indices: {string.Join(", ", indices)}", sa);
                foreach (var index in indices)
                {
                    var aoe = aoeMap[index];

                    var matchingScreen = manaScreens.FirstOrDefault(screen =>
                        Vector3.Distance(screen.Position, aoe.firstReflect) < 2f);

                    if (matchingScreen != null)
                    {
                        var firstAngle = cardinalAngles[aoe.angleFirst];
                        var dp1 = sa.Data.GetDefaultDrawProperties();
                        dp1.Name = $"EnergyRay-Reflect1-{index}";
                        dp1.Color = sa.Data.DefaultDangerColor;
                        dp1.Position = aoe.firstReflect;
                        dp1.Scale = new Vector2(20f, 48f);
                        dp1.Rotation = firstAngle;
                        dp1.Delay = 700;
                        dp1.DestoryAt = 4700;
                        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1);
                        DebugMsg($"First reflection drawn at {aoe.firstReflect} with angle {firstAngle} rad ({firstAngle * 180 / float.Pi} deg)", sa);

                        if (aoe.secondReflect != Vector3.Zero)
                        {
                            var secondAngle = cardinalAngles[aoe.angleSecond];
                            var dp2 = sa.Data.GetDefaultDrawProperties();
                            dp2.Name = $"EnergyRay-Reflect2-{index}";
                            dp2.Color = sa.Data.DefaultDangerColor;
                            dp2.Position = aoe.secondReflect;
                            dp2.Scale = new Vector2(20f, 40f);
                            dp2.Rotation = secondAngle;
                            dp2.Delay = 1200;
                            dp2.DestoryAt = 4700;
                            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);
                            DebugMsg($"Second reflection drawn at {aoe.secondReflect} with angle {secondAngle} rad ({secondAngle * 180 / float.Pi} deg)", sa);
                        }
                        return;
                    }
                }
            }
        }
    }

    [ScriptMethod(name: "Energy Ray Clean", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(44338|44339|44340|44341|44342)$"], userControl: false)]
    public void EnergyRayClean(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"EnergyRay-.*-{ev.SourceId}");
    }

    [ScriptMethod(name: "Fire Recorder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44327|44325)$"])]
    public void FireRecorder(Event ev, ScriptAccessory sa)
    {
        string msg = "";
        if (ev.ActionId == 44327)
        {
            msg = language == Language.Chinese ? "Front then back!" : "Front then back!";
        }
        else
        {
            msg = language == Language.Chinese ? "Back then Front!" : "Back then Front!";
        }

        if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Omega Blaster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44329|44330)$"])]
    public void OmegaBlaster(Event ev, ScriptAccessory sa)
    {
        int delay = 0;
        int destory = 6500;
        if (ev.ActionId == 44330)
        {
            delay = 6500;
            destory = 2500;
        }
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = $"OmegaBlaster-{ev.SourceId}";
        dp.Color = new Vector4(0, 1, 1, ColorAlpha);
        dp.Owner = ev.SourceId;
        dp.Scale = new Vector2(50f);
        dp.Radian = 180 * (float.Pi / 180);
        dp.Delay = delay;
        dp.DestoryAt = destory;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Crash", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44295"])]
    public void Crash(Event ev, ScriptAccessory sa)
    {
        var pos = ev.SourcePosition;
        bool isWest = false;

        if (pos.X < 781)
        {
            isWest = true;
        }

        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(24, 40), 10200, $"Crash-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha));

        if (isWest)
        {
            var dp = sa.Data.GetDefaultDrawProperties();
            dp.Name = "CrashDisplacementWest";
            dp.Color = sa.Data.DefaultSafeColor;
            dp.Owner = sa.Data.Me;
            dp.Rotation = float.Pi / 2;
            dp.Scale = new Vector2(2, 25f);
            dp.FixRotation = true;
            dp.DestoryAt = 10200;
            if (isLead) sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        else if (!isWest)
        {
            var dp = sa.Data.GetDefaultDrawProperties();
            dp.Name = "CrashDisplacementEast";
            dp.Color = sa.Data.DefaultSafeColor;
            dp.Owner = sa.Data.Me;
            dp.Rotation = -float.Pi / 2;
            dp.Scale = new Vector2(2, 25f);
            dp.FixRotation = false;
            dp.DestoryAt = 10200;
            if (isLead) sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        surfaceMissiles.Clear();
    }

    private List<(Vector3 position, ulong tid)> surfaceMissiles = new();
    private readonly object SurfaceMissileLock = new object();

    [ScriptMethod(name: "Surface Missile Record", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0268"])]
    public void surfaceMissileRecord(Event ev, ScriptAccessory sa)
    {
        var targetObj = IbcHelper.GetById(sa, ev.TargetId);
        if (targetObj == null) return;
        var pos = targetObj.Position;
        if (pos == Vector3.Zero) return;

        DebugMsg($"Surface Missile Record {ev.TargetId}, pos: {pos}", sa);
        sa.Log.Debug($"Surface Missile Record {ev.TargetId}, pos: {pos}");
        lock (SurfaceMissileLock)
        {
            surfaceMissiles.Add((pos.Quantized(), ev.TargetId));
            DebugMsg($"surfaceMissiles Count: {surfaceMissiles.Count}", sa);
            sa.Log.Debug($"surfaceMissiles Count: {surfaceMissiles.Count}");
        }

        if (surfaceMissiles.Count == 12)
        {
            DebugMsg($"----------surfaceMissiles == 12--------------", sa);
            sa.Log.Debug($"----------surfaceMissiles == 12--------------");

            int index = 1;

            foreach (var t in surfaceMissiles)
            {
                Vector3 position = t.position;
                ulong targetId = t.tid;

                DebugMsg($"index: {index}; pos: {position}; tid: {targetId}", sa);
                sa.Log.Debug($"index: {index}; pos: {position}; tid: {targetId}");
                index++;
            }

            var delay = 0;
            var destory = 0;

            for (int i = 0; i < surfaceMissiles.Count; i++)
            {
                var (position, tid) = surfaceMissiles[i];

                if (i < 4)
                {
                    destory = 2800;
                }
                if (i >= 4 && i < 8)
                {
                    delay = 2800;
                    destory = 2800;
                }
                if (i >= 8)
                {
                    delay = 5600;
                    destory = 3000;
                }

                DrawHelper.DrawRectObjectNoTarget(sa, tid, new Vector2(20, 12), destory, $"{i}: surfaceMissiles - delay:{delay} - destory:{destory}", offset: new Vector3(0, 0, 6f), delay: delay);
            }
        }
    }

    [ScriptMethod(name: "Guided Missile", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44324"])]
    public void GuidedMissile(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 1000, $"GuidedMissile-{ev.EffectPosition}", scaleByTime: false, color: new Vector4(1, 0, 0, ColorAlpha));
    }

    [ScriptMethod(name: "Multi-missile", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45037|45036)$"])]
    public void Multimissile(Event ev, ScriptAccessory sa)
    {
        var destory = 3700;
        var size = 6f;
        if (ev.ActionId == 45036)
        {
            destory = 3800;
            size = 10f;
        }
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(size), destory, $"Multimissile-{ev.EffectPosition}");
    }

    [ScriptMethod(name: "Citadel Siege", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44312"])]
    public void CitadelSiege(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(10f, 48f), 4700, $"Citadel Siege-{ev.EffectPosition}", color: new Vector4(1, 0, 0, ColorAlpha), scalemode: ScaleMode.ByTime);
    }

    [ScriptMethod(name: "Chemical Bomb", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44303"])]
    public void ChemicalBomb(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(20f), 4700, $"Chemical Bomb-{ev.EffectPosition}", color: new Vector4(1, 0, 0, ColorAlpha), delay: 2700, scaleByTime: false);
    }

    [ScriptMethod(name: "Boss2 Clear & Reset", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:18759"], userControl: false, suppress: 3000)]
    public void Boss2Clear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw(".*");
        lock (SurfaceMissileLock)
        {
            surfaceMissiles.Clear();
        }
    }
    #endregion

    #region Boss3
    [ScriptMethod(name: "---- Boss3 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void Boss3(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:(44221|44212)"])]
    public void boss3AOE(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (ev.ActionId == 44212)
        {
            msg = language == Language.Chinese ? "Heavy AOE" : "Heavy AOE";
        }

        if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Proving Ground", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45065"])]
    public void ProvingGround(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Don't stand under boss" : "Don't stand under boss";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 2700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
        DrawHelper.DrawCircle(sa, ev.SourcePosition, new Vector2(5f), 2700, $"ProvingGround-{ev.SourceId}", color: new Vector4(1, 0, 0, ColorAlpha), scaleByTime: false);
    }

    [ScriptMethod(name: "Elemental Blade", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44(19[1-9]|20[0-2]|179|18[0-9]|190))$"])]
    public void ElementalBlade(Event ev, ScriptAccessory sa)
    {
        DebugMsg($"Action Id: {ev.ActionId}", sa);
        var width = 5f;

        if (ev.ActionId >= 44197)
        {
            width = 20f;
        }

        if (ev.ActionId >= 44191 && ev.ActionId <= 44202)
        {
            DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(width, 80f), 8700, $"ElementalBlade-{ev.SourceId}", color: new Vector4(1, 0, 0, ColorAlpha));
        }

        var angle = 20f;

        if (ev.ActionId >= 44185)
        {
            angle = 100f;
        }

        if (ev.ActionId >= 44179 && ev.ActionId <= 44190)
        {
            var dp = sa.Data.GetDefaultDrawProperties();
            dp.Name = $"ElementalBladeFan-{ev.SourceId}";
            dp.Color = new Vector4(1, 0, 0, ColorAlpha);
            dp.Owner = ev.SourceId;
            dp.Scale = new Vector2(45f);
            dp.Radian = angle * (float.Pi / 180);
            dp.DestoryAt = 8700;
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
    }

    public bool isSecondTankBuster = false;
    List<ulong> boss3TankBusterList = new List<ulong>();
    private readonly object boss3TankBusterLock = new object();

    [ScriptMethod(name: "Tank knock-back buster Notify", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0265"])]
    public void boss3TankBuster(Event ev, ScriptAccessory sa)
    {
        lock (boss3TankBusterLock)
        {
            boss3TankBusterList.Add(ev.TargetId);
            DebugMsg($"boss3TankBusterList Count: {boss3TankBusterList.Count}, isSecondTankBuster: {isSecondTankBuster}", sa);
            if (boss3TankBusterList.Count == 3)
            {
                DebugMsg($"Contains me :{boss3TankBusterList.Contains((ulong)sa.Data.Me)}", sa);
                sa.Log.Debug($"me: {sa.Data.Me}, list1: {boss3TankBusterList[0]}; List2: {boss3TankBusterList[1]}, List3: {boss3TankBusterList[2]}");

                string msg = "";
                if (boss3TankBusterList.Contains((ulong)sa.Data.Me))
                {
                    msg = language == Language.Chinese ? "Tankbuster with knockback, Use mits" : "Tankbuster with knockback, Use mits";
                }
                else
                {
                    msg = language == Language.Chinese ? "AOE Tankbuster â€” Stay Away" : "AOE Tankbuster â€” Stay Away";
                }
                if (isText) sa.Method.TextInfo($"{msg}", duration: 6900, true);
                if (isTTS) sa.Method.EdgeTTS($"{msg}");
                boss3TankBusterList.Clear();
            }
        }

        DrawHelper.DrawRectObjectTarget(sa, ev.SourceId, ev.TargetId, new Vector2(10f, 40f), 6900, $"TankBusterKnockback-{ev.TargetId}", color: new Vector4(1, 0, 0, ColorAlpha));

        if (ev.TargetId == sa.Data.Me && isLead)
        {
            var knockbackDistance = 20f;
            if (isSecondTankBuster) knockbackDistance = 30f;
            var dp = sa.Data.GetDefaultDrawProperties();
            dp.Name = "boss3TankBuster Knockback";
            dp.Color = sa.Data.DefaultSafeColor;
            dp.Owner = sa.Data.Me;
            dp.TargetObject = ev.SourceId;
            dp.Rotation = float.Pi;
            dp.Scale = new Vector2(2, knockbackDistance);
            dp.DestoryAt = 6900;
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
    }

    [ScriptMethod(name: "Sublime Estoc", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:9352"])]
    public void SublimeEstoc(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(5f, 40f), 4500, $"SublimeEstoc-{ev.SourceId}", color: new Vector4(1, 1, 0, ColorAlpha));
    }

    [ScriptMethod(name: "Great Wheel", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44207|44205|44206)$"])]
    public void GreatWheel(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Chariot (Out)" : "Chariot (Out)";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 1000, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(10f), 2700, $"GreatWheel-{ev.SourceId}", color: new Vector4(1, 0, 0, ColorAlpha));
    }

    [ScriptMethod(name: "Great Wheel Front", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44209"])]
    public void GreatWheelFan(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(40f), 180, 5500, $"GreatWheelFan-{ev.SourceId}", color: new Vector4(1, 0, 0, ColorAlpha), scaleByTime: false);
    }

    [ScriptMethod(name: "Elemental Resonance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44216"])]
    public void ElementalResonance(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(18f), 6700, $"ElementalResonance-{ev.SourceId}", color: new Vector4(1, 0, 0, ColorAlpha), scaleByTime: false);
    }

    [ScriptMethod(name: "Illumed Estoc", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44218"])]
    public void IllumedEstoc(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(13f, 120f), 7700, $"IllumedEstoc-{ev.SourceId}", color: new Vector4(1, 0, 0, ColorAlpha), offset: new Vector3(0, 0, 30));
    }

    [ScriptMethod(name: "Shield Bash", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44222"])]
    public void ShieldBash(Event ev, ScriptAccessory sa)
    {
        isSecondTankBuster = true;
        string msg = language == Language.Chinese ? "Knockback to safe lane" : "Knockback to safe lane";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 6700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        List<Vector3> safePos = new List<Vector3>
        {
            new Vector3(-199.95f, -900.00f, 144.75f),
            new Vector3(-195.50f, -900.00f, 152.50f),
            new Vector3(-205.20f, -900.00f, 152.88f)
        };

        foreach (var pos in safePos)
        {
            DrawHelper.DrawCircle(sa, pos, new Vector2(2f), 6700, $"ShieldBashSafe-{pos}", color: sa.Data.DefaultSafeColor, scaleByTime: false);
        }

        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = "ShieldBash Knockback";
        dp.Color = sa.Data.DefaultSafeColor;
        dp.Owner = sa.Data.Me;
        dp.TargetObject = ev.SourceId;
        dp.Rotation = float.Pi;
        dp.Scale = new Vector2(2, 30f);
        dp.DestoryAt = 6700;
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "Empyreal Banish IV", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44224"])]
    public void EmpyrealBanishIV(Event ev, ScriptAccessory sa)
    {
        string tname = ev["TargetName"]?.ToString() ?? "Unknown Target";

        string msg = language == Language.Chinese ? $"Stack with {tname}" : $"Stack with {tname}";

        if (ev.TargetId == sa.Data.Me)
        {
            msg = language == Language.Chinese ? "Stack" : "Stack";
        }

        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(5f), 4700, $"EmpyrealBanishIV-{ev.SourceId}", color: sa.Data.DefaultSafeColor, scaleByTime: false);
    }
    #endregion

    #region Boss4
    [ScriptMethod(name: "---- Boss4 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: true)]
    public void Boss4(Event ev, ScriptAccessory sa)
    {
    }

    List<ulong> boss4TankBusterList = new List<ulong>();
    private readonly object boss4TankBusterLock = new object();

    [ScriptMethod(name: "Tank knock-back buster Notify", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0158"])]
    public void boss4TankBuster(Event ev, ScriptAccessory sa)
    {
        lock (boss4TankBusterLock)
        {
            boss4TankBusterList.Add(ev.TargetId);
            DebugMsg($"boss4TankBusterList Count: {boss4TankBusterList.Count}", sa);
            if (boss4TankBusterList.Count == 3)
            {
                DebugMsg($"Contains me :{boss4TankBusterList.Contains((ulong)sa.Data.Me)}", sa);
                if (isDebug) sa.Log.Debug($"me: {sa.Data.Me}, list1: {boss4TankBusterList[0]}; List2: {boss4TankBusterList[1]}, List3: {boss4TankBusterList[2]}");

                string msg = "";
                if (boss4TankBusterList.Contains((ulong)sa.Data.Me))
                {
                    msg = language == Language.Chinese ? "AOE Tankbuster, Use mits" : "AOE Tankbuster, Use mits";
                }
                else
                {
                    msg = language == Language.Chinese ? "AOE Tankbuster â€” Stay Away" : "AOE Tankbuster â€” Stay Away";
                }
                if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
                if (isTTS) sa.Method.EdgeTTS($"{msg}");
                boss4TankBusterList.Clear();
            }
        }

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(6f), 4700, $"TankBusterKnockback-{ev.TargetId}", color: new Vector4(1, 0, 0, ColorAlpha));
    }

    [ScriptMethod(name: "Cronos Sling (Dynamo)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44366"])]
    public void CronosSlingDynamo(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? $"Dynamo (In)" : $"Dynamo (In)";

        if (isText) sa.Method.TextInfo($"{msg}", duration: 7200, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(6f), 7200, $"CronosSlingDynamoSafe-{ev.SourceId}", color: sa.Data.DefaultSafeColor, scaleByTime: false);
        DrawHelper.DrawDount(sa, ev.EffectPosition, new Vector2(70f), new Vector2(6f), 7200, $"CronosSlingDynamoSafe-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: false);
    }

    [ScriptMethod(name: "Cronos Sling (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44365"])]
    public void CronosSlingChariot(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? $"Chariot (Out)" : $"Chariot (Out)";

        if (isText) sa.Method.TextInfo($"{msg}", duration: 7200, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(9f), 7200, $"CronosSlingChariot-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: false);
    }

    [ScriptMethod(name: "Cronos Sling (Haircut left/right)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44367|44368)$"])]
    public void CronosSlingHaircut(Event ev, ScriptAccessory sa)
    {
        var rot = float.Pi / 2;
        if (ev.ActionId == 44367)
        {
            rot = -float.Pi / 2;
        }

        DrawHelper.DrawFanObject(sa, ev.SourceId, rot, new Vector2(70f), 180, 13000, $"CronosSlingHaircut-{ev.SourceId}", color: new Vector4(1, 0, 0, ColorAlpha), scaleByTime: false);
    }

    [ScriptMethod(name: "Empyreal Vortex", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44400"])]
    public void EmpyrealVortex(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? $"Spread" : $"Spread";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(5f), 4700, $"EmpyrealVortex-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Warp", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44375"])]
    public void Warp(Event ev, ScriptAccessory sa)
    {
        Vector3 Centerpos = new Vector3(800.00f, -900.00f, -800.00f);
        var tarobj1 = IbcHelper.GetById(sa, ev.TargetId);
        if (tarobj1 == null) return;

        DebugMsg($"distance: {Vector3.Distance(tarobj1.Position, Centerpos)})", sa);
        if (Vector3.Distance(tarobj1.Position, Centerpos) < 15f) return;

        string msg = language == Language.Chinese ? $"Move to the Portal" : $"Move to the Portal";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 3700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        DrawHelper.DrawDisplacement(sa, tarobj1.Position, new Vector2(2, 4), 5500, $"Warp-{ev.TargetId}", color: sa.Data.DefaultSafeColor);
    }

    [ScriptMethod(name: "Sleepga", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44376"])]
    public void Sleepga(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(70), 180, 2700, $"Sleepga-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: false);
    }

    [ScriptMethod(name: "Gaea Stream", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44373"])]
    public void GaeaStream(Event ev, ScriptAccessory sa)
    {
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = $"GaeaStream-{ev.SourceId}";
        dp.Color = new Vector4(1, 1, 0, ColorAlpha);
        dp.Owner = ev.SourceId;
        dp.Scale = new Vector2(4, 24);
        dp.DestoryAt = 1700;
        dp.Rotation = float.Pi / 2;
        dp.Offset = new Vector3(12, 0, 0);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Omega Javelin", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44382"], suppress: 5000)]
    public void OmegaJavelin(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? $"Away from Javelin" : $"Away from Javelin";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4200, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    private bool duplicatePhase2 = false;

    [ScriptMethod(name: "Duplicate", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:regex:^(0000002[C-F]|000000(3[0-9])|0000003[A-D]|00000024)$"])]
    public void Duplicate(Event ev, ScriptAccessory sa)
    {
        var index = ev.Index();
        var state = uint.Parse(ev["State"]?.ToString() ?? "0", System.Globalization.NumberStyles.HexNumber);

        DebugMsg($"EnvControl - Index: 0x{index:X} ({index}), State: 0x{state:X} ({state})", sa);
        sa.Log.Debug($"EnvControl - Index: 0x{index:X} ({index}), State: 0x{state:X} ({state})");

        switch (index)
        {
            case >= 0x2Cu and <= 0x34u when !duplicatePhase2 && state == 0x00020001u || duplicatePhase2 && state == 0x00080010u:
                DebugMsg($"1. Duplicate AOE at tile index: 0x{index:X}, duplicatePhase2: {duplicatePhase2}", sa);
                sa.Log.Debug($"1. Duplicate AOE at tile index: 0x{index:X}, duplicatePhase2: {duplicatePhase2}");

                var tile = index - 0x2Cu;
                AddDuplicateAOEs((int)(tile / 3), (int)(tile % 3), sa);
                break;

            case >= 0x35u and <= 0x3Du:
                DebugMsg($"2. Duplicate AOE at tile index: 0x{index:X}", sa);
                sa.Log.Debug($"2. Duplicate AOE at tile index: 0x{index:X}");
                switch (state)
                {
                    case 0x00080010u:
                        DebugMsg($"case 0x00080010u activated", sa);
                        sa.Log.Debug($"case 0x00080010u activated");
                        var tile2 = index - 0x35u;
                        AddDuplicateAOEs((int)(tile2 / 3), (int)(tile2 % 3), sa);
                        break;
                    case 0x00020001u:
                        DebugMsg($"case 0x00020001u activated", sa);
                        sa.Log.Debug($"case 0x00020001u activated");
                        var tile3 = index - 0x35u;
                        var tilePos = new Vector3(784f + tile3 % 3 * 16f, -900, -816f + tile3 / 3 * 16f) + new Vector3(0, 0, -8f);
                        DebugMsg($"Draw Duplicate at tile index: 0x{index:X}, pos: {tilePos}", sa);
                        sa.Log.Debug($"Draw Duplicate at tile index: 0x{index:X}, pos: {tilePos}");
                        DrawHelper.DrawRectPosNoTarget(sa, tilePos, new Vector2(16f, 16f), 7800, $"Duplicate-{index}", new Vector4(1, 0, 0, ColorAlpha), drawMode: DrawModeEnum.Default);
                        break;
                }
                break;
            case 0x24u when state == 0x00200040u:
                duplicatePhase2 = true;
                DebugMsg("Duplicate Phase 2 activated", sa);
                sa.Log.Debug("Duplicate Phase 2 activated");
                break;
        }
    }

    private void AddDuplicateAOEs(int row, int col, ScriptAccessory sa)
    {
        DebugMsg($"AddDuplicateAOEs at row: {row}, col: {col}", sa);
        (int dr, int dc)[] offsets =
        [
            (0, 0),
            (-1, 0),
            (1, 0),
            (0, -1),
            (0, 1),
        ];

        for (var i = 0; i < 5; ++i)
        {
            var nRow = row + offsets[i].dr;
            var nCol = col + offsets[i].dc;

            sa.Log.Debug($"Checking offset {i}: nRow: {nRow}, nCol: {nCol}");
            if (nRow is >= 0 and < 3 && nCol is >= 0 and < 3)
            {
                var aoePos = new Vector3(784f + nCol * 16f, -900, -816f + nRow * 16f) + new Vector3(0, 0, -8f);
                var duration = duplicatePhase2 ? 11100 : 10400;

                var dp = sa.Data.GetDefaultDrawProperties();
                dp.Name = $"offset {i} - DuplicateAOE-{nRow}-{nCol}-pos: {aoePos}";
                dp.Color = new Vector4(1, 0, 0, ColorAlpha);
                dp.Position = aoePos;
                dp.Scale = new Vector2(16f, 16f);
                dp.DestoryAt = duration;
                dp.Offset = new Vector3(0, 0, 0);
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }
    }

    [ScriptMethod(name: "Duplicate Clear", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44369|44370)$"], userControl: false)]
    public void DuplicateClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw("Duplicate.*");
        sa.Method.RemoveDraw("DuplicateAOE.*");
        DebugMsg("Duplicate AOEs cleared", sa);
    }

    [ScriptMethod(name: "Stellar Burst", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44403)$"])]
    public void StellarBurst(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? $"Stack Mid" : $"Stack Mid";
        if (ev.TargetId == sa.Data.Me)
        {
            msg = language == Language.Chinese ? $"Stack" : $"Stack";
        }

        if (isText) sa.Method.TextInfo($"{msg}", duration: 6000, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(6f), 12000, $"StellarBurst-{ev.SourceId}", color: sa.Data.DefaultSafeColor, scaleByTime: false);
    }

    string Tornadoguid = "";
    private bool hasKnockback = false;

    [ScriptMethod(name: "Tornado", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44395)$"])]
    public async void Tornado(Event ev, ScriptAccessory sa)
    {
        Tornadoguid = sa.Method.RegistFrameworkUpdateAction(() =>
        {
            List<uint> knockBackStatus = new List<uint> { 1209, 160, 2663 };

            for (int i = 0; i < knockBackStatus.Count; i++)
            {
                var hasStatus = IbcHelper.HasStatus(sa, sa.Data.MyObject, knockBackStatus[i]);
                if (hasStatus)
                {
                    hasKnockback = true;
                    sa.Method.RemoveDraw($"Tornado-Danger:{ev.SourceId}");
                    break;
                }
            }
        });

        if (!hasKnockback)
        {
            if (useAntiKnockback) sa.Method.UseAction(sa.Data.Me, 7559);
            if (useAntiKnockback) sa.Method.UseAction(sa.Data.Me, 7548);
            DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(21f), 6000, $"Tornado-Danger:{ev.SourceId}", scaleByTime: false, color: new Vector4(1, 0, 0, ColorAlpha));
        }

        await Task.Delay(5700);
        if (Tornadoguid != "")
        {
            sa.Method.UnregistFrameworkUpdateAction(Tornadoguid);
            Tornadoguid = "";
            hasKnockback = false;
        }
    }

    [ScriptMethod(name: "Orbital Flame & Orbital Levin", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(18747|18746)$"])]
    public void orbitalFlameLevin(Event ev, ScriptAccessory sa)
    {
        var scale = 3f;
        var color = new Vector4(1, 1, 0, ColorAlpha);
        var arrowScale = new Vector2(1, 3);
        if (uint.Parse(ev["DataId"]?.ToString() ?? "0") == 18746)
        {
            scale = 1.5f;
            color = new Vector4(0, 1, 1, ColorAlpha);
            arrowScale = new Vector2(1, 2);
        }

        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = $"orbitalflame-{ev.SourceId}";
        dp.Color = color;
        dp.Owner = ev.SourceId;
        dp.Scale = new Vector2(scale);
        dp.DestoryAt = int.MaxValue;
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

        var dp1 = sa.Data.GetDefaultDrawProperties();
        dp1.Name = $"orbitalflame arrow-{ev.SourceId}";
        dp1.Color = sa.Data.DefaultSafeColor;
        dp1.Owner = ev.SourceId;
        dp1.Scale = arrowScale;
        dp1.DestoryAt = int.MaxValue;
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp1);
    }

    [ScriptMethod(name: "Orbital Flame Clear", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:regex:^(18747|18746)$"], userControl: false)]
    public void orbitalflameClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"orbitalflame-{ev.SourceId}");
        sa.Method.RemoveDraw($"orbitalflame arrow-{ev.SourceId}");
    }

    [ScriptMethod(name: "Flood", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44390)$"])]
    public void Flood(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(20f), 6000, $"Flood-{ev.SourceId}", scaleByTime: false, color: new Vector4(1, 0, 0, ColorAlpha));
    }
    #endregion

    #region Priority Dictionary Class
    public class PriorityDict
    {
        public ScriptAccessory sa { get; set; } = null!;
        public Dictionary<int, int> Priorities { get; set; } = null!;
        public string Annotation { get; set; } = "";
        public int ActionCount { get; set; } = 0;

        public void Init(ScriptAccessory accessory, string annotation, int partyNum = 8, bool refreshActionCount = true)
        {
            sa = accessory;
            Priorities = new Dictionary<int, int>();
            for (var i = 0; i < partyNum; i++)
            {
                Priorities.Add(i, 0);
            }
            Annotation = annotation;
            if (refreshActionCount)
                ActionCount = 0;
        }

        public void AddPriority(int idx, int priority)
        {
            Priorities[idx] += priority;
        }

        public List<KeyValuePair<int, int>> SelectSmallPriorityIndices(int num)
        {
            return SelectMiddlePriorityIndices(0, num);
        }

        public List<KeyValuePair<int, int>> SelectLargePriorityIndices(int num)
        {
            return SelectMiddlePriorityIndices(0, num, true);
        }

        public List<KeyValuePair<int, int>> SelectMiddlePriorityIndices(int skip, int num, bool descending = false)
        {
            if (Priorities.Count < skip + num)
                return new List<KeyValuePair<int, int>>();

            IEnumerable<KeyValuePair<int, int>> sortedPriorities;
            if (descending)
            {
                sortedPriorities = Priorities
                    .OrderByDescending(pair => pair.Value)
                    .ThenBy(pair => pair.Key)
                    .Skip(skip)
                    .Take(num);
            }
            else
            {
                sortedPriorities = Priorities
                    .OrderBy(pair => pair.Value)
                    .ThenBy(pair => pair.Key)
                    .Skip(skip)
                    .Take(num);
            }

            return sortedPriorities.ToList();
        }

        public KeyValuePair<int, int> SelectSpecificPriorityIndex(int idx, bool descending = false)
        {
            var sortedPriorities = SelectMiddlePriorityIndices(0, Priorities.Count, descending);
            return sortedPriorities[idx];
        }

        public int FindPriorityIndexOfKey(int key, bool descending = false)
        {
            var sortedPriorities = SelectMiddlePriorityIndices(0, Priorities.Count, descending);
            var i = 0;
            foreach (var dict in sortedPriorities)
            {
                if (dict.Key == key) return i;
                i++;
            }
            return i;
        }

        public void AddPriorities(List<int> priorities)
        {
            if (Priorities.Count != priorities.Count)
                throw new ArgumentException("Input list length differs from internal length");

            for (var i = 0; i < Priorities.Count; i++)
                AddPriority(i, priorities[i]);
        }

        public string ShowPriorities(bool showJob = true)
        {
            var str = $"{Annotation} ({ActionCount}-th) Priority Dictionary:\n";
            if (Priorities.Count == 0)
            {
                str += $"PriorityDict Empty.\n";
                return str;
            }
            foreach (var pair in Priorities)
            {
                str += $"Key {pair.Key} {(showJob ? $"({Role[pair.Key]})" : "")}, Value {pair.Value}\n";
            }
            return str;
        }

        public PriorityDict DeepCopy()
        {
            return JsonConvert.DeserializeObject<PriorityDict>(JsonConvert.SerializeObject(this)) ?? new PriorityDict();
        }

        public void AddActionCount(int count = 1)
        {
            ActionCount += count;
        }
    }
    #endregion
}

#region Function Libraries
// All helper classes (EventExtensions, IbcHelper, MathTools, IndexHelper, DrawTools, MarkerHelper, SpecialFunction, NamazuHelper, DrawHelper, ExtensionMethods, ExtensionVisibleMethod) are identical to previous files
#endregion
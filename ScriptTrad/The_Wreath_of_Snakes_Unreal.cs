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

namespace Veever.DawnTrail.the_Wreath_of_Snakes_Unreal;

[ScriptType(name: Name, territorys: [825, 1302], guid: "3a915832-971d-4c27-b802-407a1e30ae53",
    version: Version, author: "Veever & Usami", note: NoteStr, updateInfo: UpdateInfo)]

public class UnrealSeiryu
{
    const string NoteStr =
    """
    v0.0.0.1
    1. If you need a draw or notice any issues, @ me on DC or DM me.
    2. For the Forbidden Arts Stack, both healers are hard-locked draw
       (if both healers die, it’s GG / pray to Hydaelyn xd).
    3. Tower positions are as follows:
          MT / D1    ST / D2
          H1 / D3    H2 / D4
    4. EX trial draw are not fully implemented yet (missing skill IDs — honestly just lazy, 
       will add them one day).
    5. PostNamazu markers default to standard N/E/S/W (ABCD).
    Duckmen.
    """;

    private const string Name = "LV.100 Wreath of Snakes [Unreal]";
    private const string Version = "0.0.0.1";
    private const string DebugVersion = "a";
    private const string UpdateInfo = "";

    private const bool Debugging = false;

    private static readonly List<string> Role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
    private static readonly Vector3 Center = new Vector3(100, 0, 100);
    
    private static uint BossDataId = 9708;

    [UserSetting("Language")]
    public Language language { get; set; } = Language.Chinese;

    [UserSetting("Draw opacity — higher value = more visible")]
    public static float ColorAlpha { get; set; } = 1f;

    [UserSetting("Banner text toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS toggle")]
    public bool isTTS { get; set; } = true;

    [UserSetting("Auto anti-knockback")]
    public bool useaction { get; set; } = true;

    [UserSetting("Guide arrow toggle")]
    public bool isLead { get; set; } = true;

    [UserSetting("Target Marker toggle")]
    public bool isMark { get; set; } = true;

    [UserSetting("Local target marker toggle (ON = local only, OFF = party shared)")]
    public bool LocalMark { get; set; } = true;

    [UserSetting("Waymark guide toggle")]
    public bool PostNamazuPrint { get; set; } = true;

    [UserSetting("PostNamazu Port Setting")]
    public int PostNamazuPort { get; set; } = 2019;

    [UserSetting("Waymarks: local toggle (off = party shared, OOC only)")]
    public bool PostNamazuisLocal { get; set; } = true;

    [UserSetting("Debug switch, turn off unless developing")]
    public bool isDebug { get; set; } = false;

    public enum Language
    {
        Chinese,
        English
    }

    private volatile List<bool> _bools = new bool[20].ToList();
    private List<int> _numbers = Enumerable.Repeat(0, 8).ToList();
    private static List<ManualResetEvent> _events = Enumerable
        .Range(0, 20)
        .Select(_ => new ManualResetEvent(false))
        .ToList();
    
    private Dictionary<ulong, ulong> _tethersDict = new();
    private static bool _initHint = false;
    public int KanaboTTSCount;

    private enum SeiryuPhase
    {
        Init,
        P2A_Mobs,
        P3A_RainStorm,
        P3B_BrazenSoul,
    }
    
    private static SeiryuPhase _seiryuPhase = SeiryuPhase.Init;

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    public void Init(ScriptAccessory sa)
    {
        _seiryuPhase = SeiryuPhase.Init;
        RefreshParams();
        sa.Log.Debug($"Script {Name} v{Version}{DebugVersion} initialized.");
        sa.Method.RemoveDraw(".*");
        if (PostNamazuPrint) PostWaymark(sa);
        KanaboTTSCount = 0;
        _initHint = false;
    }
    
    private void RefreshParams()
    {
        _bools = new bool[20].ToList();
        _numbers = Enumerable.Repeat(0, 20).ToList();
        _events = Enumerable
            .Range(0, 20)
            .Select(_ => new ManualResetEvent(false))
            .ToList();
        _tethersDict = new Dictionary<ulong, ulong>();
    }

    #region Waymark
    private static readonly Vector3 posA = new Vector3(100.00f, -0.00f, 80.86f);
    private static readonly Vector3 posB = new Vector3(119.28f, -0.00f, 100.00f);
    private static readonly Vector3 posC = new Vector3(100.00f, -0.00f, 119.03f); 
    private static readonly Vector3 posD = new Vector3(80.51f, -0.00f, 100.00f);

    public void PostWaymark(ScriptAccessory accessory)
    {
        var waymark = new NamazuHelper.Waymark(accessory);
        waymark.AddWaymarkType("A", posA);
        waymark.AddWaymarkType("B", posB);
        waymark.AddWaymarkType("C", posC);
        waymark.AddWaymarkType("D", posD);

        waymark.SetJsonPayload(LocalMark, PostNamazuisLocal);
        waymark.PostWaymarkCommand(PostNamazuPort);
    }
    #endregion

    [ScriptMethod(name: "Place waymarks manually", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdk"],
    userControl: true)]
    public void userNamazuPost(Event ev, ScriptAccessory sa)
    {
        PostWaymark(sa);
    }

    [ScriptMethod(name: "---- Timeline Logging ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void SplitLine_Timeline(Event ev, ScriptAccessory sa)
    {
        // DataId
        // 9708 Seiryu
        // 9710 Red Onmyoji
        // 9711 Blue Onmyoji
        // 9712 Rock Onmyoji
        // 10103 Heaven Onmyoji
        // 9714 Mud Onmyoji (small)
        // 9713 Swamp Onmyoji (large)
        // 9715 Mountain Onmyoji
    }

    [ScriptMethod(name: "---- Test Items ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void TestItems(Event ev, ScriptAccessory sa)
    {
        var bossId = GetBossObject(sa).GameObjectId;
        var dp = sa.DrawCircle(bossId, 0, 3000, $"Heavy", 12f, byTime: true, draw: false);
        dp.Color = sa.Data.DefaultDangerColor.WithW(ColorAlpha);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        sa.ScaleModify(GetBossObject(sa), 2f);
    }
    
    [ScriptMethod(name: "Strategy and Role Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(14275|43962)$"], 
        userControl: true)]
    public void MethodRoleTips(Event ev, ScriptAccessory sa)
    {
        KanaboTTSCount = 0;
        if (_initHint) return;
        _initHint = true;
        
        var aid = ev.ActionId;
        BossDataId = aid == 43962u ? 18643u : 9708u;
        
        var myIndex = sa.Data.PartyList.IndexOf(sa.Data.Me); 
        List<string> role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
        sa.Method.TextInfo(
            $"You are [{role[myIndex]}], please adjust if incorrect.", 5000);
    }

    [ScriptMethod(name: "AOE warnings", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43962)$"],
        userControl: true)]
    public void AOENotify(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 3700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Kuji-kiri Draw", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4399[3])$"],
        userControl: true)]
    public void bladeSigil(Event ev, ScriptAccessory sa)
    {
        Vector3 TargetPos0 = ev.TargetPosition;
        Vector3 TargetPos1 = ev.TargetPosition;
        DebugMsg($"TargetPos0: {TargetPos0}, TargetPos1: {TargetPos1}, rotation: {ev.SourceRotation}", sa);
        if (ev.SourceRotation == 1.57f)
        {
            DebugMsg($"X change", sa);
            TargetPos0.X += 30f;
            TargetPos1.X -= 30f;
        }
        else if (ev.SourceRotation == 0f)
        {
            DebugMsg($"Z change", sa);
            TargetPos0.Z += 30f; 
            TargetPos1.Z -= 30f;
        }

        DrawHelper.DrawRect(sa, ev.SourcePosition, TargetPos0, new Vector2(4f, 50f), 3000, $"bladeSigil0-{ev.SourceId}", color: new Vector4(0, 1, 1, ColorAlpha));
        DrawHelper.DrawRect(sa, ev.SourcePosition, TargetPos1, new Vector2(4f, 50f), 3000, $"bladeSigil1-{ev.SourceId}", color: new Vector4(0, 1, 1, ColorAlpha));
    }

    [ScriptMethod(name: "Main Body: Heavy/Donut", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(148(49|51|53)|4402[468])$"], 
        userControl: true)]
    public void HeavyDonut(Event ev, ScriptAccessory sa)
    {
        var aid = ev.ActionId;
        var sid = ev.SourceId;
        sa.Log.Debug($"Detected Heavy/Donut {aid}, executing draw");

        DrawPropertiesEdit? dp, dp2;
        
        switch (aid)
        {
            case 14849 or 44024:
                dp = sa.DrawCircle(sid, 0, 3000, $"Heavy", 12f, draw: false);
                dp.Color = sa.Data.DefaultDangerColor.WithW(ColorAlpha);
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                string msg = language == Language.Chinese ? "Heavy - Stay away from boss" : "Heavy - Stay away from boss";
                if (isText) sa.Method.TextInfo($"{msg}", duration: 2700, true);
                if (isTTS) sa.Method.EdgeTTS($"{msg}");
                break;
            
            case 14853 or 44028:
                string msg1 = language == Language.Chinese ? "Donut first, then Heavy" : "Donut → Heavy";
                if (isText) sa.Method.TextInfo($"{msg1}", duration: 2700, true);
                if (isTTS) sa.Method.EdgeTTS($"{msg1}");
                
                dp = sa.DrawDonut(sid, 0, 3000, $"Donut", 30f, 7f, draw: false);
                dp.Color = sa.Data.DefaultDangerColor.WithW(ColorAlpha);
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                
                dp2 = sa.DrawCircle(sid, 3000, 3000, $"Heavy", 12f, draw: false);
                dp2.Color = sa.Data.DefaultDangerColor.WithW(ColorAlpha);
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);
                break;
            
            case 14851 or 44026:
                string msg2 = language == Language.Chinese ? "Heavy first, then Donut" : "Heavy → Donut";
                if (isText) sa.Method.TextInfo($"{msg2}", duration: 2700, true);
                if (isTTS) sa.Method.EdgeTTS($"{msg2}");

                dp = sa.DrawCircle(sid, 0, 3000, $"Heavy", 12f, draw: false);
                dp.Color = sa.Data.DefaultDangerColor.WithW(ColorAlpha);
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                
                dp2 = sa.DrawDonut(sid, 3000, 3000, $"Donut", 30f, 7f, draw: false);
                dp2.Color = sa.Data.DefaultDangerColor.WithW(ColorAlpha);
                sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
                break;
                
            default:
                break;
        }
    }
    
    [ScriptMethod(name: "Main Body: Infirm Soul + Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(14290|43977)$"],
        userControl: true)]
    public async void InfirmSoulAndTankbuster(Event ev, ScriptAccessory sa)
    {
        var myIndex = sa.Data.PartyList.IndexOf(sa.Data.Me);
        var aid = ev.ActionId;
        sa.Log.Debug($"Detected Infirm Soul {aid}, executing draw");
        var sid = ev.SourceId;
        var tid = ev.TargetId;

        var dp = sa.DrawCircle(sid, 0, 10000, $"Tankbuster Target", 4f, byTime: false, draw: false);
        dp.SetOwnersEnmityOrder(1);
        dp.Color = sa.Data.DefaultDangerColor.WithW(ColorAlpha);
        
        var dp0 = sa.DrawCircle(tid, 0, 10000, $"Debuff Target", 4f, byTime: false, draw: false);
        dp0.Color = new Vector4(0f, 0f, 1f, ColorAlpha);
        
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp0);

        if (myIndex == 0 || myIndex == 1)
        {
            string msg = language == Language.Chinese ? "Tank swap" : "Tank swap";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 3000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");

            await Task.Delay(13000);
            string msg1 = language == Language.Chinese ? "Tank swap" : "Tank swap";
            if (isText) sa.Method.TextInfo($"{msg1}", duration: 3000);
            if (isTTS) sa.Method.EdgeTTS($"{msg1}");
        }
    }
    
    [ScriptMethod(name: "Infirm Soul + Tankbuster (Draw Delete)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(14292|43979)$"],
        userControl: Debugging)]
    public void InfirmSoulAndTankbuster_DrawDelete(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Tankbuster Target");
        sa.Method.RemoveDraw($"Debuff Target");
    }
    
    [ScriptMethod(name: "--- Onmyoji Summon 1 Phase Transition ---", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(14286|43973)$"],
        userControl: Debugging)]
    public void OnmyojiSummon1_PhaseTransition(Event ev, ScriptAccessory sa)
    {
        sa.Log.Debug($"Detected Onmyoji Summon 1 {ev.ActionId}.");
        _seiryuPhase = SeiryuPhase.P2A_Mobs;
        RefreshParams();
        sa.Log.Debug($"Phase transition to: {_seiryuPhase}");
    }

    [ScriptMethod(name: "Red Wheel: Red Rush", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(0011)$"],
        userControl: true)]
    public void RedRush(Event ev, ScriptAccessory sa)
    {
        sa.Log.Debug($"Detected Red Rush tether {ev.Id0()}, tether targets {ev.SourceId}, {ev.TargetId}.");

        var playerId = sa.Data.PartyList.Contains((uint)ev.SourceId) ? ev.SourceId : ev.TargetId;
        var bossId = playerId == ev.SourceId ? ev.TargetId : ev.SourceId;
        
        var dp = sa.DrawRect(bossId, playerId, 0, 10000, $"Red Rush",
            0, 5f, 10, false, false, true, false);
        dp.Color = sa.Data.DefaultDangerColor.WithW(ColorAlpha);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        lock (_numbers)
        {
            if (ev.TargetId == sa.Data.Me)
            {
                _bools[0] = true;
                string msg = language == Language.Chinese ? "Go to flanks, avoid cleaving" : "Go to flanks, avoid cleaving";
                if (isText) sa.Method.TextInfo($"{msg}", duration: 3000, true);
                if (isTTS) sa.Method.EdgeTTS($"{msg}");

                if (useaction) sa.Method.UseAction(sa.Data.Me, 7559);
                if (useaction) sa.Method.UseAction(sa.Data.Me, 7548);
            }
                
        
            _numbers[0]++;
            sa.Log.Debug($"Detected Red Rush #{_numbers[0]}");

            if (_numbers[0] == 2)
            {
                _events[0].Set();
                sa.Log.Debug($"Red Rush record successful, releasing lock");
            }
        }
        
        if (ev.TargetId != sa.Data.Me) return;
        var dp0 = sa.DrawKnockBack(bossId, 0, 10000, $"Red Rush Knockback",
            3f, 15, false, false);
        dp0.Color = new Vector4(0f, 1f, 1f, ColorAlpha);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp0);
    }
    
    [ScriptMethod(name: "Blue Wheel: Blue Bolt", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(0039)$"],
        userControl: true)]
    public void BlueBolt(Event ev, ScriptAccessory sa)
    {
        sa.Log.Debug($"Detected Blue Bolt tether {ev.Id0()}, tether targets {ev.SourceId}, {ev.TargetId}.");
        _events[0].WaitOne();
        sa.Log.Debug($"Blue Bolt unlocked successfully, player {(_bools[0] ? "avoiding stack" : "participating in stack")}");
        
        var playerId = sa.Data.PartyList.Contains((uint)ev.SourceId) ? ev.SourceId : ev.TargetId;
        var bossId = playerId == ev.SourceId ? ev.TargetId : ev.SourceId;
        
        var dp = sa.DrawRect(bossId, playerId, 0, 6000, $"Blue Bolt",
            0, 5f, 40, false, false, false, false);
        dp.Color = _bools[0]
            ? sa.Data.DefaultDangerColor.WithW(ColorAlpha)
            : sa.Data.DefaultSafeColor.WithW(ColorAlpha);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        _events[0].Reset();
    }

    [ScriptMethod(name: "Onmyoji - 100-tonze Swing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44004)$"],
    userControl: true)]
    public void tonzeSwing(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(16f), 3700, "100-tonze Swing");
    }

    [ScriptMethod(name: "Red Rush, Blue Bolt (Draw Delete)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(1432[01]|4400[78])$"],
        userControl: Debugging)]
    public void BlueBoltDrawDelete(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Red Rush.*");
        sa.Method.RemoveDraw($"Blue Bolt.*");
    }
    
    [ScriptMethod(name: "Rock Onmyoji: Kanabo", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(0054)$"],
        userControl: true)]
    public void Kanabo(Event ev, ScriptAccessory sa)
    {
        sa.Log.Debug($"Detected Kanabo tether {ev.Id0()}, tether targets {ev.SourceId}, {ev.TargetId}.");
        var bossId = ev.SourceId;
        var playerId = ev.TargetId;
        var myIndex = sa.Data.PartyList.IndexOf(sa.Data.Me);

        if (myIndex == 0 && KanaboTTSCount == 0)
        {
            string msg = language == Language.Chinese ? "Take WEST line, drag out" : "Take WEST line, drag out";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 3000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
            KanaboTTSCount++;
        }

        if (myIndex == 1 && KanaboTTSCount == 0)
        {
            string msg = language == Language.Chinese ? "Take EAST line, drag out" : "Take EAST line, drag out";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 3000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
            KanaboTTSCount++;
        }

        var oldTargetId = _tethersDict.GetValueOrDefault(bossId, 0u);
        sa.Log.Debug($"{bossId}'s previous target was {oldTargetId}.");
        if (oldTargetId == playerId) return;
        
        sa.Method.RemoveDraw($"Kanabo{bossId}_{oldTargetId}");
        _tethersDict[bossId] = playerId;
        
        var dp = sa.DrawFan(bossId, playerId, 0, 10000, $"Kanabo{bossId}_{playerId}",
            60f.DegToRad(), 0, 40f, 0f, draw: false);
        dp.Color = sa.Data.DefaultDangerColor.WithW(ColorAlpha);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "Rock Onmyoji: Kanabo (Draw Delete)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(14318|44005)$"],
        userControl: Debugging)]
    public void KanaboDrawDelete(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Kanabo.*");
    }
    
    [ScriptMethod(name: "Mud Onmyoji: Stoneskin Prompt and Local Marker", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(14324|44011)$"],
        userControl: true)]
    public void Stoneskin(Event ev, ScriptAccessory sa)
    {
        sa.Log.Debug($"Detected {ev.SourceId} casting Stoneskin {ev.ActionId}.");
        var sid = ev.SourceId;
        var myIndex = sa.Data.PartyList.IndexOf(sa.Data.Me);
        var dp = sa.DrawCircle(sid, 0, 5000, $"Stoneskin{sid}", 3f, draw: false);
        dp.Color = new Vector4(1f, 0f, 0f, ColorAlpha);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        if (myIndex == 0)
        {
            string msg = language == Language.Chinese ? "Silence the marked mob!" : "Silence the marked mob!";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 3000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }

        if (isMark) sa.Method.Mark((uint)sid, MarkType.Attack1, LocalMark);
    }
    
    [ScriptMethod(name: "Mud Onmyoji: Stoneskin, Restore after interrupt or cast", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(14324|44011)$"],
        userControl: Debugging)]
    public void StoneskinRestore1(Event ev, ScriptAccessory sa)
    {
        sa.Log.Debug($"Detected {ev.SourceId} finished casting Stoneskin {ev.ActionId}.");
        var sid = ev.SourceId;
        sa.MarkClear(local: true);
        sa.Method.RemoveDraw($"Stoneskin{sid}");
    }
    
    [ScriptMethod(name: "Mud Onmyoji: Stoneskin, Restore after interrupt or cast", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:regex:^(14324|44011)$"],
        userControl: Debugging)]
    public void StoneskinRestore2(Event ev, ScriptAccessory sa)
    {
        sa.Log.Debug($"Detected {ev.SourceId} finished casting Stoneskin {ev.ActionId}.");
        var sid = ev.SourceId;
        sa.MarkClear(local: true);
        sa.Method.RemoveDraw($"Stoneskin{sid}");
    }
    
    [ScriptMethod(name: "--- Aether Phase Transition ---", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(14281|43968)$"],
        userControl: Debugging)]
    public void Aether_PhaseTransition(Event ev, ScriptAccessory sa)
    {
        sa.Log.Debug($"Detected Aether {ev.ActionId}.");
        _seiryuPhase = SeiryuPhase.P3A_RainStorm;
        RefreshParams();
        sa.Log.Debug($"Phase transition to: {_seiryuPhase}");
    }
    
    [ScriptMethod(name: "Second Half: Coursing River Knockback", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(14327|44014)$"],
        userControl: true)]
    public void CoursingRiverKnockback(Event ev, ScriptAccessory sa)
    {
        sa.Log.Debug($"Detected Coursing River {ev.ActionId}, position {ev.EffectPosition}");

        var dir = ev.EffectPosition.GetRadian(Center).RadianToRegion(4, isDiagDiv: true);
        sa.Log.Debug($"Coursing River direction {dir} (1 right, 3 left)");
        
        var dp = sa.DrawRect(sa.Data.Me, 0, 10000, $"Coursing River", 
            -dir * float.Pi/2, 5f, 25f, draw: false);
        dp.FixRotation = true;
        dp.Color = new Vector4(0f, 1f, 1f, ColorAlpha);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

        var knockBackPos = new Vector3(100, 0, 107).RotateAndExtend(Center, dir * float.Pi / 2, 0);
        sa.Log.Debug($"Coursing River knockback position {knockBackPos}");
        var dp0 = sa.DrawGuidance(knockBackPos, 0, 10000, $"Coursing River Guide", draw: false);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);
    }
    
    [ScriptMethod(name: "Second Half: Coursing River Knockback (Draw Delete)", eventType: EventTypeEnum.KnockBack, eventCondition: ["Distance:regex:^(25.00)$"],
        userControl: Debugging)]
    public void CoursingRiverKnockbackDelete(Event ev, ScriptAccessory sa)
    {
        sa.Log.Debug($"Detected knockback distance 25.00.");
        sa.Method.RemoveDraw($"Coursing River.*");
    }
    
    [ScriptMethod(name: "Mountain Onmyoji: Crushing Palm", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(143(09|10)|43999)$"],
        userControl: true)]
    public void CrushingPalm(Event ev, ScriptAccessory sa)
    {
        sa.Log.Debug($"Detected Crushing Palm {ev.ActionId}.");
        var aid = ev.ActionId;

        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = $"CrushingPalm{aid}";
        dp.Color = sa.Data.DefaultDangerColor;
        dp.Owner = ev.SourceId;
        dp.Scale = new Vector2(20f);
        dp.Radian = 180 * (float.Pi / 180);
        dp.DestoryAt = 4200;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    [ScriptMethod(name: "Mountain Onmyoji: Crushing Palm (Draw Delete)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(14312)$"],
        userControl: Debugging)]
    public void CrushingPalmDrawDelete(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"CrushingPalm.*");
    }
    
    [ScriptMethod(name: "--- Brazen Soul Phase Transition ---", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(14308|43995)$"],
        userControl: Debugging)]
    public void BrazenSoul_PhaseTransition(Event ev, ScriptAccessory sa)
    {
        sa.Log.Debug($"Detected Brazen Soul {ev.ActionId}.");
        _seiryuPhase = SeiryuPhase.P3B_BrazenSoul;
        RefreshParams();
        sa.Log.Debug($"Phase transition to: {_seiryuPhase}");
    }
    
    [ScriptMethod(name: "Brazen Soul: Rising Dragon Gather Prompt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(15397|44034)$"],
        userControl: true)]
    public void RisingDragonGatherPrompt(Event ev, ScriptAccessory sa)
    {
        sa.Log.Debug($"Detected Rising Dragon {ev.ActionId}.");
        Vector3 midpos = new Vector3(100, 0, 100);
        var dp = sa.DrawGuidance(midpos, 0, 3000, $"Gather Point");

        string msg = language == Language.Chinese ? "Gather to bait circle" : "Gather to bait circle";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4000, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    public bool towered = true;

    [ScriptMethod(name: "Brazen Soul: Tower Update", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(00A9)$"],
    userControl: true)]
    public void towerUpdate(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            towered = false;
        }
        else
        {
            towered = true;
        }
    }

    [ScriptMethod(name: "Brazen Soul: Tower Navigation", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:regex:^(2009660)$", "Operate:Add"],
        userControl: true, suppress: 10000)]
    public void TowerNavigation(Event ev, ScriptAccessory sa)
    {
        var myIndex = sa.GetMyIndex();
        List<Vector3> towerPos = [new(88.7f, 0, 88.7f), new(111.3f, 0, 88.7f), new(88.7f, 0, 111.3f), new (111.3f, 0, 111.3f)];
        List<Vector3> notowerPos = [new(100.28f, 0.01f, 86.13f), new(114.16f, 0.01f, 99.95f), new(86.70f, 0.01f, 100.50f), new(99.98f, 0.01f, 115.13f)];
        DebugMsg($"towered: {towered}", sa);
        if (towered)
        {
            var dp = sa.DrawGuidance(towerPos[myIndex % 4], 0, 10000, $"Tower Navigation");
        } else
        {
            var dp = sa.DrawGuidance(notowerPos[myIndex % 4], 0, 10000, $"Tower Navigation");
        }
        
    }
    
    [ScriptMethod(name: "Brazen Soul: Tower Navigation (Draw Delete 1)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(14301|43988)$"],
        userControl: Debugging)]
    public void TowerNavigationDrawDelete1(Event ev, ScriptAccessory sa)
    {
        var tid = ev.TargetId;
        var aid = ev.ActionId;
        sa.Log.Debug($"Detected Descending Snake {aid}, target {tid} ({sa.GetPlayerIdIndex((uint)tid)}).");
        
        if (tid != sa.Data.Me) return;
        sa.Method.RemoveDraw($"Tower Navigation.*");
    }
    
    [ScriptMethod(name: "Brazen Soul: Tower Navigation (Draw Delete 2)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(14301|43988)$"],
        userControl: Debugging)]
    public void TowerNavigationDrawDelete2(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Tower Navigation.*");
    }
    
    [ScriptMethod(name: "Brazen Soul: Forbidden Arts Stack", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(14277|43964)$"],
        userControl: true)]
    public void ForbiddenArtsStack(Event ev, ScriptAccessory sa)
    {
        var aid = ev.ActionId;
        var sid = ev.SourceId;
        var tid = ev.TargetId;
        sa.Log.Debug($"Detected Forbidden Arts {aid}, target {tid} ({sa.GetPlayerIdIndex((uint)tid)}).");
        
        var dp = sa.DrawRect(sid, sa.Data.PartyList[2], 0, 6000, $"Forbidden Arts Stack",
            0, 5f, 40, false, false, false, false);
        dp.Color = sa.Data.DefaultSafeColor.WithW(ColorAlpha);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        var dp1 = sa.DrawRect(sid, sa.Data.PartyList[3], 0, 6000, $"Forbidden Arts Stack",
            0, 5f, 40, false, false, false, false);
        dp1.Color = sa.Data.DefaultSafeColor.WithW(ColorAlpha);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1);
    }
    
    private static IGameObject? GetBossObject(ScriptAccessory sa)
    {
        return sa.GetByDataId(BossDataId).FirstOrDefault();
    }
    
    #region Priority Dictionary Class
    public class PriorityDict
    {
        public ScriptAccessory sa {get; set;} = null!;
        public Dictionary<int, int> Priorities {get; set;} = null!;
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
                throw new ArgumentException("Input list length does not match internal length");

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
using Dalamud.Utility.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Lua;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.GameEvent.Types;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Script;
using Lumina.Data;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using KodaMarkType = KodakkuAssist.Module.GameOperate.MarkType;

namespace Veever.DawnTrail.TheWindwardWildsExtreme;

[ScriptType(name: "LV.100 Guardian Arkveld (Extreme) The Windward", territorys: [], guid: "d5135e4c-de45-4fd7-ab9d-3b31d84208b3", version: "0.0.0.1", Author: "Linoa235")]

// ^(?!.*((Monk|Machinist|Dragoon|Samurai|Ninja|Viper|Reaper|Dancer|Bard|Astrologian|Sage|Scholar|(Eos|Selene)|Seraph|White Mage|Warrior|Paladin|Dark Knight|Gunbreaker|Pictomancer|Black Mage|Blue Mage|Summoner|Carbuncle|Demigod Bahamut|Demigod Phoenix|Garuda-Egi|Titan-Egi|Ifrit-Egi|Puppet)\] (Used|Cast))).*35501.*$
// ^\[\w+\|[^|]+\|E\]\s\w+ 

public class TheWindwardWildsExtreme
{
    const string NoteStr =
    """
    v0.0.0.8
    ----- Thanks to @Usami for the distance-based Cracked Crystal drawing method.-----
    ----- Please read the notes before use and adjust user settings as needed. -----
    1. If you need a draw or notice any issues, @ me on DC or DM me.
    2. Left/Right notifies are **boss-relative** (based on you facing the boss), 
       not the boss model’s own left/right. Please keep this in mind.
    3. This script follows the Game8 & 子言 guide.
    4. If you want WayMarkers, make sure ACT is running and the PostNamazu plugin is installed.
    5. Most drawing depends on **party list order** — please make sure the party order is correct.
    6. Cleaned up a few TTS after the drawings. If you need any of them, let me know and I’ll restore them.
    7. Stack drawings may behave oddly if a healer goes down.
    8. There may be some odd bugs with network lag.
    9. Wyvern’s Weal now has both recommend and stack guide arrow.
       The recommended guide arrow is shown in green, and the stack guide arrow is shown in purple.
    10. Boss Model Scale and VFX Scale can be adjusted in User Settings, 1 is the Game default value.
    11. You can now manually set your Role in the User Settings → Role Settings.
        `默认defalutRole` follows KodakkuAssist’s Role order.
    Duckmen.
    """;

    const string UpdateStr =
    """
    v0.0.0.8
    Adapted for the new patch
    Duckmen.
    ----------------------------------
    1. Adapted for the new patch
    Duckmen.
    """;

    private const string Name = "LV.100 Guardian Arkveld (Extreme) [The Windward Wilds (Extreme)]";
    private const string Version = "0.0.0.8";
    private const string DebugVersion = "a";

    private const bool Debugging = false;

    private static readonly List<string> Role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
    private static readonly Vector3 Center = new Vector3(100, 0, 100);


    [UserSetting("Announcement language")]
    public Language language { get; set; } = Language.Chinese;

    [UserSetting("Drawing opacity — higher value = more visible")]
    public static float ColorAlpha { get; set; } = 1f;

    [UserSetting("Boss Model Scale")]
    public static float BossModelScale { get; set; } = 1f;

    [UserSetting("Boss VFX Scale")]
    public static float BossVFXScale { get; set; } = 1f;

    [UserSetting("Banner text toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS toggle")]
    public bool isTTS { get; set; } = true;

    //[UserSetting("Auto anti-knockback")]
    //public bool useaction { get; set; } = true;

    [UserSetting("Guide arrow toggle")]
    public bool isLead { get; set; } = true;

    [UserSetting("Role Settings")]
    public IndexMode RoleSetting { get; set; } = IndexMode.DefaultRole;

    //[UserSetting("Target Marker toggle")]
    //public bool isMark { get; set; } = true;

    //[UserSetting("Local target marker toggle (ON = local only, OFF = party shared)")]
    //public bool LocalMark { get; set; } = true;

    [UserSetting("Waymark guide toggle")]
    public bool PostNamazuPrint { get; set; } = true;

    [UserSetting("PostNamazu Port Setting")]
    public int PostNamazuPort { get; set; } = 2019;

    [UserSetting("Waymarks: local toggle (off = party shared, OOC only)")]
    public bool PostNamazuisLocal { get; set; } = true;

    [UserSetting("Debug on/off (don't touch unless you know what you're doing)")]
    public bool isDebug { get; set; } = false;

    public enum Language
    {
        Chinese,
        English
    }

    private static bool _initHint = false;

    public uint GuardianArkveldDataId = 18661;
    public int ChainbladeBlowTripleCount = 0;
    public bool StackInProgress = false;
    public bool IsWestEastTower = true;

    private readonly object CountLock = new object();
    private readonly object _crystalsLock = new object();
    private readonly object GroupStackLock = new object();

    List<ulong> StackPlayerId = new List<ulong>();
    Dictionary<int, ulong> ClamorousChaseDict = new Dictionary<int, ulong>();
    List<Vector3> ChainbladeBlowTriplePos = new List<Vector3>();

    private enum GuardianArkveldPhase
    {
        Phase1,
        Phase2,
    }

    private static GuardianArkveldPhase guardianArkveldPhase = GuardianArkveldPhase.Phase1;


    public void DebugMsg(string str, ScriptAccessory sa)
    {
        if (!isDebug) return;
        sa.Log.Debug($"[DEBUG] {str}");
    }

    private ScriptAccessory _sa = null;

    // Class Defined
    public class Crystal(IGameObject obj, bool drawn)
    {
        public IGameObject Obj { get; set; } = obj;
        public bool Drawn { get; set; } = drawn;
    }

    // Params
    private List<Crystal> _crystals = [];

    // Trigger
    private bool _crossExaflareCrystalTriggered = false;    // Cross Exaflare
    private bool _dragonBeamCrystalTriggered = false;       // Wyvern's Weal
    private bool _edgeExaflareCrystalTriggered = false;     // Edge Exaflare

    // Framework Guid
    private string _crystalCloseGuid = "";

    // TTS Cooldown
    private long _lastStackTTSTime = 0;
    private const long TTS_COOLDOWN = 3000;

    // My Index
    //private int myIndex = -1;
    private static readonly List<string> role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
    public enum IndexMode
    {

        DefaultRole = -1,
        MT = 0,
        ST = 1,
        H1 = 2,
        H2 = 3,
        D1 = 4,
        D2 = 5,
        D3 = 6,
        D4 = 7
    }


    public void Init(ScriptAccessory sa)
    {
        guardianArkveldPhase = GuardianArkveldPhase.Phase1;
        sa.Log.Debug($"Script {Name} v{Version}{DebugVersion} initialized successfully.");
        sa.Method.RemoveDraw(".*");

        if (PostNamazuPrint) PostWaymark(sa);

        RefreshParams();
        _sa = sa;

        sa.Method.ClearFrameworkUpdateAction(this);

        _ = ScriptVersionChecker.CheckVersionAsync(
            sa,
            "b4a3871d-2499-4152-aaa2-a911ee1bbce2",
            Version,
            showNotification: true
        );

        SpecialFunction.SetModelScale(sa, GuardianArkveldDataId, BossModelScale, BossVFXScale);
    }
     
    private void RefreshParams()
    {
        _crystals = [];

        _crossExaflareCrystalTriggered = false;
        _dragonBeamCrystalTriggered = false;
        _edgeExaflareCrystalTriggered = false;
        _initHint = false;
        IsWestEastTower = true;
        StackInProgress = false;

        _crystalCloseGuid = "";
        ChainbladeBlowTripleCount = 0;

        ClamorousChaseDict.Clear();
        ChainbladeBlowTriplePos.Clear();
    }

    #region Waymark
    private static readonly Vector3 posA = new Vector3(100.00f, -0.00f, 82.7f);
    private static readonly Vector3 posB = new Vector3(118.00f, -0.00f, 100.00f);
    private static readonly Vector3 posC = new Vector3(100.00f, -0.00f, 118.00f);
    private static readonly Vector3 posD = new Vector3(81.5f, -0.00f, 100.00f);
    private static readonly Vector3 posOne = Center;


    public void PostWaymark(ScriptAccessory accessory)
    {
        var waymark = new NamazuHelper.Waymark(accessory);
        waymark.AddWaymarkType("A", posA);
        waymark.AddWaymarkType("B", posB);
        waymark.AddWaymarkType("C", posC);
        waymark.AddWaymarkType("D", posD);
        waymark.AddWaymarkType("One", posOne);

        waymark.SetJsonPayload(PostNamazuisLocal, PostNamazuisLocal);
        waymark.PostWaymarkCommand(PostNamazuPort);
    }
    #endregion

    [ScriptMethod(name: "Place waymarks manually", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdk"],
    userControl: Debugging)]
    public void userNamazuPost(Event ev, ScriptAccessory sa)
    {
        PostWaymark(sa);
    }

    [ScriptMethod(name: "Change Scale", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdk"],
    userControl: Debugging)]
    public void setModelscale(Event ev, ScriptAccessory sa)
    {
        SpecialFunction.SetModelScale(sa, GuardianArkveldDataId, BossModelScale, BossVFXScale);
    }

    [ScriptMethod(name: "---- Timeline Logging ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void SplitLine_Timeline(Event ev, ScriptAccessory sa)
    {
        
        // DataId


        // ActionId


        // StatusId


        // TetherId

        // Too many, not displayed

    }

    //[ScriptMethod(name: "set JobIndex", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^(?i)(mt|st|h1|h2|d1|d2|d3|d4)$"], userControl: Debugging)]
    //public void SetMyIndex(Event ev, ScriptAccessory sa)
    //{
    //    var input = ev["Message"]?.ToString()?.ToUpper();
    //    if (string.IsNullOrEmpty(input)) return;

    //    var index = role.FindIndex(r => r.Equals(input, StringComparison.OrdinalIgnoreCase));
    //    if (index >= 0)
    //    {
    //        myIndex = index;
    //        if (isDebug) sa.Log.Debug($"MyIndex manually set to: {myIndex} ({role[myIndex]})");

    //        string msg = language == Language.Chinese ? $"/e Role has been set to {role[myIndex]}" : $"/e Role has been set -> {role[myIndex]}";
    //        sa.Method.SendChat($"{msg}");
    //    }
    //}

    [ScriptMethod(name: "JobIndexTest", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:regex:^(?i)(index)$"], userControl: Debugging)]
    public void MyIndex(Event ev, ScriptAccessory sa)
    {
        if (isDebug) sa.Method.SendChat($"/e MyIndex is: {GetMyIndex(sa)} ({role[GetMyIndex(sa)]}), index: {sa.Data.PartyList.IndexOf(sa.Data.Me)}");
    }
    
    public int GetMyIndex(ScriptAccessory sa)
    {
        var myIndex = -1;
        if (RoleSetting != IndexMode.DefaultRole)
        {
            myIndex = (int)RoleSetting;
        }
        else
        {
            myIndex = sa.Data.PartyList.IndexOf(sa.Data.Me);
        }
        if (isDebug) sa.Log.Debug($"MyIndex set to: {myIndex} ({role[myIndex]})");
        return myIndex;
    }

    [ScriptMethod(name: "Role Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43950)$"],
        userControl: true)]
    public void RoleTips(Event ev, ScriptAccessory sa)
    {
        ChainbladeBlowTripleCount = 0;

        if (_initHint) return;
        _initHint = true;

        List<string> role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];

        string msg = language == Language.Chinese ?
            $"/e You are 【{role[GetMyIndex(sa)]}】，" +
            $"Please adjust if incorrect.<se.2><se.2>" :
            $"/e You are【{role[GetMyIndex(sa)]}】，" +
            $"Please adjust if incorrect.<se.2><se.2>";

        sa.Method.SendChat($"{msg}");
    }

    private IGameObject? GetBossObject(ScriptAccessory sa)
    {
        return sa.GetByDataId(GuardianArkveldDataId).FirstOrDefault();
    }

    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43950|43951|45202)$"],
        userControl: true)]
    public void AOENotify(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}"); 
        ChainbladeBlowTripleCount = 0;
        ClamorousChaseDict.Clear();
        ChainbladeBlowTriplePos.Clear();
        ChainbladeBlowTripleCount = 0;
    }

    #region Phase 1
    [ScriptMethod(name: "P1 Chainblade Blow", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43892|43891)$"])]
    public void P1ChainbladeBlow(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            // 43892 boss right first
            // 43891 boss left first
            case 43892:
                {
                    string msg = language == Language.Chinese ? "Left ←← Right" : "Left ←← Right";
                    string msg1 = language == Language.Chinese ? "Right then Left" : "Right then Left";
                    if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
                    if (isTTS) sa.Method.EdgeTTS($"{msg1}");

                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(28f, 80f), 6700, $"Chainblade Blow rightP1-{ev.SourceId}",
                        sa.Data.DefaultDangerColor, offset: new Vector3(7f, 0, 20f));

                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(28f, 80f), 3500, $"Chainblade Blow leftP1-{ev.SourceId}",
                        sa.Data.DefaultDangerColor, offset: new Vector3(-7f, 0, 20f), delay: 6700);
                    break;
                }
            case 43891:
                {
                    string msg = language == Language.Chinese ? "Left →→ Right" : "Left →→ Right";
                    string msg1 = language == Language.Chinese ? "Left then Right" : "Left then Right";
                    if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
                    if (isTTS) sa.Method.EdgeTTS($"{msg1}");

                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(28f, 80f), 6700, $"Chainblade Blow leftP1-{ev.SourceId}",
                        sa.Data.DefaultDangerColor, offset: new Vector3(-7f, 0, 20f));

                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(28f, 80f), 3500, $"Chainblade Blow rightP1-{ev.SourceId}",
                        sa.Data.DefaultDangerColor, offset: new Vector3(7f, 0, 20f), delay: 6700);
                    break;
                }
        }
    }

    [ScriptMethod(name: "Wyvern's Siegeflight", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45111|43905|45104)$"])]
    public void WyvernsSiegeflight(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            // 45111 small
            // 43905 split
            // 
            case 45111 or 45104:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(8f, 40f), 6200, $"Wyvern's Siegeflight mid-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha));
                    break;
                }
            case 43905:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20f, 40f), 3000, $"Wyvern's Siegeflight Left-{ev.SourceId}",
                        sa.Data.DefaultDangerColor, offset: new Vector3(12f, 0, 0f), delay: 6000);
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20f, 40f), 3000, $"Wyvern's Siegeflight Right-{ev.SourceId}",
                        sa.Data.DefaultDangerColor, offset: new Vector3(-12f, 0, 0f), delay: 6000);
                    break;
                }
        }
    }

    [ScriptMethod(name: "P1 Guardian Siegeflight", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43900|43906|45099)$"], suppress: 1000)]
    public void P1GuardianSiegeflight(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            // 43835 small
            // 43836 large
            case 43900 or 45099:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(8f, 40f), 6200, $"Guardian Siegeflight mid-{ev.SourceId}", new Vector4(0, 1, 1, ColorAlpha));
                    break;
                }
            case 43906:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(16f, 40f), 3000, $"Guardian Siegeflight mid-{ev.SourceId}",
                        new Vector4(1, 0, 0, ColorAlpha), delay: 6200);
                    break;
                }
        }
    }


    [ScriptMethod(name: "Line AoEs [Wyvern's Siegeflight]", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43940)$"])]
    public void LineAoEs(Event ev, ScriptAccessory sa)
    {
        var spos = ev.SourcePosition;
        var srot = ev.SourceRotation;

        float[] distances = { 0f, 8f, 16f, 24f };
        int[] durations = { 2200, 2700, 2700, 2700 };
        int[] delays = { 0, 2200, 5200, 7600 };

        for (int i = 0; i < distances.Length; i++)
        {
            float worldX = spos.X + distances[i] * MathF.Sin(srot);
            float worldZ = spos.Z + distances[i] * MathF.Cos(srot);
            Vector3 aoePosition = new Vector3(worldX, spos.Y, worldZ);

            DrawHelper.DrawRectPosNoTarget(sa, aoePosition, new Vector2(40f, 8f), srot, durations[i],
                $"Line AoEs [Wyvern's Siegeflight]-{ev.SourceId}-{i}",
                new Vector4(1, 0, 0, ColorAlpha),
                delay: delays[i]);
        }
    }


    [ScriptMethod(name: "Chainblade Blow 3-hit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43911|43912|43913|43914)$"])]
    public void ChainbladeBlowTriple(Event ev, ScriptAccessory sa)
    {
        lock (CountLock)
        {
            switch (ev.ActionId)
            {
                case 43911:
                    {
                        ChainbladeBlowTriplePos.Add(ev.EffectPosition);
                        if (ChainbladeBlowTripleCount == 0)
                        {
                            string msg = language == Language.Chinese ? "Go to the edge of the third Chariot, then move in" : "To Third Chariot edge, then in";
                            if (isText) sa.Method.TextInfo($"{msg}", duration: 5700, true);
                            if (isTTS) sa.Method.EdgeTTS($"{msg}");
                        }

                        if (ChainbladeBlowTripleCount == 1)
                        {
                            var bossObj = GetBossObject(sa);
                            if (bossObj == null) return;
                            DrawHelper.DrawRectPosTarget(sa, bossObj.Position, ChainbladeBlowTriplePos[0], new Vector2(12f, 33f), 5700,
                                "Connect", color: new Vector4(1, 0, 1, ColorAlpha));

                            DrawHelper.DrawRectPosTarget(sa, ChainbladeBlowTriplePos[0], ChainbladeBlowTriplePos[1], new Vector2(12f, 24f), 6500,
                                "Connect", color: new Vector4(1, 0, 1, ColorAlpha));
                        }

                        if (ChainbladeBlowTripleCount == 2)
                        {
                            DrawHelper.DrawRectPosTarget(sa, ChainbladeBlowTriplePos[1], ChainbladeBlowTriplePos[2], new Vector2(12f, 24f), 7500,
                                "Connect", color: new Vector4(1, 0, 1, ColorAlpha));
                        }

                        if (ChainbladeBlowTripleCount != 2)
                        {
                            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(8f), 7200, $"Chainblade Blow Circle-{ev.SourceId}", sa.Data.DefaultDangerColor);
                        }
                        else
                        {
                            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(8f), 7200, $"Chainblade Blow Circle-{ev.SourceId}", new Vector4(1, 1, 0, ColorAlpha));                      
                        }

                        ChainbladeBlowTripleCount++;
                        break;
                    }
                case 43912:
                    {
                        DrawHelper.DrawDount(sa, ev.EffectPosition, new Vector2(14f), new Vector2(8f), 2000,
                            $"Chainblade Blow Donut-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: 7200);
                        break;
                    }
                case 43913:
                    {
                        DrawHelper.DrawDount(sa, ev.EffectPosition, new Vector2(20f), new Vector2(14f), 2000,
                            $"Chainblade Blow Donut-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: 9200);
                        break;
                    }
                case 43914:
                    {
                        DrawHelper.DrawDount(sa, ev.EffectPosition, new Vector2(26f), new Vector2(20f), 2000,
                            $"Chainblade Blow Donut-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: 11200);
                        break;
                    }
            }
        }
    }

    [ScriptMethod(name: "Group Stack", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(0064)$"])]
    public async void GroupStack(Event ev, ScriptAccessory sa)
    {
        lock (CountLock)
        {
            DebugMsg("In Group Stack Method Check", sa);
            StackPlayerId.Add(ev.TargetId);
            if (StackPlayerId.Count < 2) return;
        }
        lock (GroupStackLock)
        {
            StackInProgress = true;
            DebugMsg("In Group Stack Method", sa);
            var targetindex0 = sa.Data.PartyList.IndexOf((uint)StackPlayerId[0]);
            var targetindex1 = sa.Data.PartyList.IndexOf((uint)StackPlayerId[1]);

            var object0 = IbcHelper.GetById(sa, StackPlayerId[0]);
            var object1 = IbcHelper.GetById(sa, StackPlayerId[1]);
            if (object0 == null || object1 == null) return;

            // Check H1 Group
            bool IsH1Group(int index) => index is 0 or 2 or 4 or 5;

            if (isDebug) DebugMsg($"targetindex0: {targetindex0}, targetindex1: {targetindex1}, myIndex: {GetMyIndex(sa)}", sa);

            // myobj got targeted
            if (StackPlayerId.Contains(sa.Data.Me))
            {
                bool nearBoss = IsH1Group(GetMyIndex(sa));
                string msg = language == Language.Chinese
                    ? (nearBoss ? "Stack near the boss" : "Stack away from the boss")
                    : (nearBoss ? "Stack Near Boss" : "Stack Away Boss");

                if (isDebug) DebugMsg(nearBoss ? "A" : "B", sa);
                if (isText) sa.Method.TextInfo($"{msg}", duration: 5700, true);
                if (isTTS) sa.Method.EdgeTTS($"{msg}");
                DrawHelper.DrawCircleObject(sa, sa.Data.Me, new Vector2(4.5f), 7700,
                    $"Stack {(nearBoss ? "Near" : "Away")}", color: sa.Data.DefaultSafeColor);
            }
            else
            {
                // myobj no targeted
                int targetIndex = -1;
                ulong targetPlayerId = 0;
                IGameObject? targetObject = null;

                if (IsH1Group(GetMyIndex(sa)))
                {
                    // H1 Group
                    if (IsH1Group(targetindex0))
                    {
                        targetIndex = 0;
                        targetPlayerId = StackPlayerId[0];
                        targetObject = object0;
                    }
                    else if (IsH1Group(targetindex1))
                    {
                        targetIndex = 1;
                        targetPlayerId = StackPlayerId[1];
                        targetObject = object1;
                    }
                }
                else
                {
                    // H2 Group
                    if (!IsH1Group(targetindex0))
                    {
                        targetIndex = 0;
                        targetPlayerId = StackPlayerId[0];
                        targetObject = object0;
                    }
                    else if (!IsH1Group(targetindex1))
                    {
                        targetIndex = 1;
                        targetPlayerId = StackPlayerId[1];
                        targetObject = object1;
                    }
                }

                // Find Same Group Target
                if (targetIndex >= 0 && targetObject != null)
                {
                    if (isDebug) DebugMsg($"Going to stack with index {targetIndex}", sa);

                    string msg = language == Language.Chinese
                        ? $"Stack with {IbcHelper.GetObjectName(targetObject)}"
                        : $"Stack with {IbcHelper.GetObjectName(targetObject)}";

                    if (isText) sa.Method.TextInfo($"{msg}", duration: 5700, true);

                    long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    if (isTTS && (currentTime - _lastStackTTSTime > TTS_COOLDOWN))
                    {
                        sa.Method.EdgeTTS($"{msg}");
                        _lastStackTTSTime = currentTime;
                    }

                    DrawHelper.DrawCircleObject(sa, targetPlayerId, new Vector2(4.5f), 7700,
                        $"Stack with {IbcHelper.GetObjectName(targetObject)}", color: sa.Data.DefaultSafeColor);
                    if (isLead) DrawHelper.DrawDisplacementObject(sa, targetPlayerId, new Vector2(2f), 7700,
                        $"Stack Displacement with {IbcHelper.GetObjectName(targetObject)}", color: new Vector4(1, 0, 1, 2));
                }
            }
        }
        await Task.Delay(7000);
        StackPlayerId.Clear();
        StackInProgress = false;
    }

    [ScriptMethod(name: "Stack", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(0064)$"])]
    public async void Stack(Event ev, ScriptAccessory sa)
    {
        await Task.Delay(1100);
        if (StackInProgress) return;

        DebugMsg("In Stack Method", sa);
        string tname = ev["TargetName"]?.ToString() ?? "Unknown Target";
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Stack on you" : "Stack";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 5700, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
        else
        {
            string msg = language == Language.Chinese ? $"Stack with {tname}" : $"Stack with {tname}";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 5700, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(4.5f), 6000, "Stack", color: sa.Data.DefaultSafeColor);
    }

    [ScriptMethod(name: "P1 Wyvern's Ouroblade", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43916|43918)$"])]
    public void P1WyvernsOuroblade(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            // 43916 left
            // 43918 right
            case 43916:
                {
                    DrawHelper.DrawFanObject(sa, ev.SourceId, float.Pi / 2, new Vector2(40f), 180, 6700, $"Wyvern's Ouroblade left-{ev.SourceId}", sa.Data.DefaultDangerColor,
                        scaleByTime: false);
                    break;
                }
            case 43918:
                {
                    DrawHelper.DrawFanObject(sa, ev.SourceId, -float.Pi / 2, new Vector2(40f), 180, 6700, $"Wyvern's Ouroblade right-{ev.SourceId}", sa.Data.DefaultDangerColor,
                        scaleByTime: false);
                    break;
                }
        }
    }

    // Tower soaking
    List<Vector3> WestEastTowersPos = new List<Vector3>
    {
        new Vector3(88.00f, 0.00f, 96.00f),         // Main Tank West
        new Vector3(110.00f, 0.00f, 102.00f),       // Side Tank East

        new Vector3(92.00f, 0.00f, 106.00f),        // D1 SW
        new Vector3(102.00f, 0.00f, 110.00f),       // D2 SE

        new Vector3(96.00f, 0.00f, 88.00f),         // D3 NW
        new Vector3(108.00f, 0.00f, 90.00f),        // D4 NE
    };

    List<Vector3> NorthSouthTowersPos = new List<Vector3>
    {
        new Vector3(102.00f, 0.00f, 90.00f),       // Main Tank North
        new Vector3(96.00f, 0.00f, 112.00f),       // Side Tank South

        new Vector3(88.00f, 0.00f, 104.00f),        // D1 SW1
        new Vector3(108.00f, 0.00f, 108.00f),       // D2 SE1

        new Vector3(90.00f, 0.00f, 92.00f),         // D3 NW
        new Vector3(110.00f, 0.00f, 98.00f),        // D4 NE1

    };

    [ScriptMethod(name: "Check Tower", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43921)$"], userControl: false)]
    public void CheckTower(Event ev, ScriptAccessory sa)
    {
        lock (CountLock)
        {
            if (Vector3.Distance(ev.SourcePosition, WestEastTowersPos[0]) < 3f ||
                Vector3.Distance(ev.SourcePosition, WestEastTowersPos[1]) < 3f)
            {
                IsWestEastTower = true;
            }
            else
            {
                IsWestEastTower = false;
            }
        }
    }

    [ScriptMethod(name: "Guardian Resonance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43919)$"])]
    public async void GuardianResonance(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? $"Stack at point 1 → drop 3 circles → soak towers (No Puddles on Towers)" : $"Stack at point 1 → drop 3 circles → soak towers (No Puddles on Towers)";
        string msg1 = language == Language.Chinese ? $"Stack at point one and drop circles then soak towers" : $"Stack at point one and drop circles then soak towers";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 5000, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg1}");

        List<string> role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
        await Task.Delay(1000);
        if (IsWestEastTower)
        {
            switch (GetMyIndex(sa))
            {
                case 0: // MT
                    DrawHelper.DrawCircle(sa, WestEastTowersPos[0], new Vector2(4f), 11000, $"Guardian Resonance-{Role[GetMyIndex(sa)]}",
                        sa.Data.DefaultSafeColor, scaleByTime: false, drawmode: DrawModeEnum.Imgui);
                    if (isLead) DrawHelper.DrawDisplacement(sa, WestEastTowersPos[0], new Vector2(2f), 11000, $"Guardian Resonance Navi-{Role[GetMyIndex(sa)]}");
                    break;
                case 1: // ST
                    DrawHelper.DrawCircle(sa, WestEastTowersPos[1], new Vector2(4f), 11000, $"Guardian Resonance-{Role[GetMyIndex(sa)]}",
                        sa.Data.DefaultSafeColor, scaleByTime: false, drawmode: DrawModeEnum.Imgui);
                    if (isLead) DrawHelper.DrawDisplacement(sa, WestEastTowersPos[1], new Vector2(2f), 11000, $"Guardian Resonance Navi-{Role[GetMyIndex(sa)]}");
                    break;
                case 4: // D1
                    DrawHelper.DrawCircle(sa, WestEastTowersPos[2], new Vector2(2f), 11000, $"Guardian Resonance-{Role[GetMyIndex(sa)]}",
                        sa.Data.DefaultSafeColor, scaleByTime: false, drawmode: DrawModeEnum.Imgui);
                    if (isLead) DrawHelper.DrawDisplacement(sa, WestEastTowersPos[2], new Vector2(2f), 11000, $"Guardian Resonance Navi-{Role[GetMyIndex(sa)]}");
                    break;
                case 5: // D2
                    DrawHelper.DrawCircle(sa, WestEastTowersPos[3], new Vector2(2f), 11000, $"Guardian Resonance-{Role[GetMyIndex(sa)]}",
                        sa.Data.DefaultSafeColor, scaleByTime: false, drawmode: DrawModeEnum.Imgui);
                    if (isLead) DrawHelper.DrawDisplacement(sa, WestEastTowersPos[3], new Vector2(2f), 11000, $"Guardian Resonance Navi-{Role[GetMyIndex(sa)]}");
                    break;
                case 6: // D3
                    DrawHelper.DrawCircle(sa, WestEastTowersPos[4], new Vector2(2f), 11000, $"Guardian Resonance-{Role[GetMyIndex(sa)]}",
                        sa.Data.DefaultSafeColor, scaleByTime: false, drawmode: DrawModeEnum.Imgui);
                    if (isLead) DrawHelper.DrawDisplacement(sa, WestEastTowersPos[4], new Vector2(2f), 11000, $"Guardian Resonance Navi-{Role[GetMyIndex(sa)]}");
                    break;
                case 7: // D4
                    DrawHelper.DrawCircle(sa, WestEastTowersPos[5], new Vector2(2f), 11000, $"Guardian Resonance-{Role[GetMyIndex(sa)]}",
                        sa.Data.DefaultSafeColor, scaleByTime: false, drawmode: DrawModeEnum.Imgui);
                    if (isLead) DrawHelper.DrawDisplacement(sa, WestEastTowersPos[5], new Vector2(2f), 11000, $"Guardian Resonance Navi-{Role[GetMyIndex(sa)]}");
                    break;
                default:
                    await Task.Delay(4500);
                    string msg2 = language == Language.Chinese ? $"Healers cover the tower" : $"Healer Cover Tower";
                    if (isText) sa.Method.TextInfo($"{msg2}", duration: 5000, true);
                    if (isTTS) sa.Method.EdgeTTS($"{msg2}");
                    break;
            }
        } else
        {
            switch (GetMyIndex(sa))
            {
                case 0: // MT
                    DrawHelper.DrawCircle(sa, NorthSouthTowersPos[0], new Vector2(4f), 11000, $"Guardian Resonance-{Role[GetMyIndex(sa)]}",
                        sa.Data.DefaultSafeColor, scaleByTime: false, drawmode: DrawModeEnum.Imgui);
                    if (isLead) DrawHelper.DrawDisplacement(sa, NorthSouthTowersPos[0], new Vector2(2f), 11000, $"Guardian Resonance Navi-{Role[GetMyIndex(sa)]}");
                    break;
                case 1: // ST
                    DrawHelper.DrawCircle(sa, NorthSouthTowersPos[1], new Vector2(4f), 11000, $"Guardian Resonance-{Role[GetMyIndex(sa)]}",
                        sa.Data.DefaultSafeColor, scaleByTime: false, drawmode: DrawModeEnum.Imgui);
                    if (isLead) DrawHelper.DrawDisplacement(sa, NorthSouthTowersPos[1], new Vector2(2f), 11000, $"Guardian Resonance Navi-{Role[GetMyIndex(sa)]}");
                    break;
                case 4: // D1
                    DrawHelper.DrawCircle(sa, NorthSouthTowersPos[2], new Vector2(2f), 11000, $"Guardian Resonance-{Role[GetMyIndex(sa)]}",
                        sa.Data.DefaultSafeColor, scaleByTime: false, drawmode: DrawModeEnum.Imgui);
                    if (isLead) DrawHelper.DrawDisplacement(sa, NorthSouthTowersPos[2], new Vector2(2f), 11000, $"Guardian Resonance Navi-{Role[GetMyIndex(sa)]}");
                    break;
                case 5: // D2
                    DrawHelper.DrawCircle(sa, NorthSouthTowersPos[3], new Vector2(2f), 11000, $"Guardian Resonance-{Role[GetMyIndex(sa)]}",
                        sa.Data.DefaultSafeColor, scaleByTime: false, drawmode: DrawModeEnum.Imgui);
                    if (isLead) DrawHelper.DrawDisplacement(sa, NorthSouthTowersPos[3], new Vector2(2f), 11000, $"Guardian Resonance Navi-{Role[GetMyIndex(sa)]}");
                    break;
                case 6: // D3
                    DrawHelper.DrawCircle(sa, NorthSouthTowersPos[4], new Vector2(2f), 11000, $"Guardian Resonance-{Role[GetMyIndex(sa)]}",
                        sa.Data.DefaultSafeColor, scaleByTime: false, drawmode: DrawModeEnum.Imgui);
                    if (isLead) DrawHelper.DrawDisplacement(sa, NorthSouthTowersPos[4], new Vector2(2f), 11000, $"Guardian Resonance Navi-{Role[GetMyIndex(sa)]}");
                    break;
                case 7: // D4
                    DrawHelper.DrawCircle(sa, NorthSouthTowersPos[5], new Vector2(2f), 11000, $"Guardian Resonance-{Role[GetMyIndex(sa)]}",
                        sa.Data.DefaultSafeColor, scaleByTime: false, drawmode: DrawModeEnum.Imgui);
                    if (isLead) DrawHelper.DrawDisplacement(sa, NorthSouthTowersPos[5], new Vector2(2f), 11000, $"Guardian Resonance Navi-{Role[GetMyIndex(sa)]}");
                    break;
                default:
                    await Task.Delay(4500);
                    string msg2 = language == Language.Chinese ? $"Healers cover the tower" : $"Healer Cover Tower";
                    if (isText) sa.Method.TextInfo($"{msg2}", duration: 5000, true);
                    if (isTTS) sa.Method.EdgeTTS($"{msg2}");
                    break;
            }
        }
    }


    [ScriptMethod(name: "Wyvern's Vengeance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43926)$"])]
    public void WyvernsVengeance(Event ev, ScriptAccessory sa)
    {
        var effectPos = ev.EffectPosition;
        var srot = ev.SourceRotation;

        float[] distances = { 0f, 8f, 16f };
        int[] delays = { 0, 0, 0 };
        int[] durations = { 4700, 6500, 8300 };

        for (int i = 0; i < distances.Length; i++)
        {
            float worldX = effectPos.X + distances[i] * MathF.Sin(srot);
            float worldZ = effectPos.Z + distances[i] * MathF.Cos(srot);
            Vector3 circlePos = new Vector3(worldX, effectPos.Y, worldZ);

            DrawHelper.DrawCircle(sa, circlePos, new Vector2(6f), durations[i],
                $"Wyvern's Vengeance-{ev.SourceId}-{i}",
                new Vector4(1, 0, 0, ColorAlpha), scaleByTime: false, delay: delays[i]);
        }
    }

    [ScriptMethod(name: "Wyvern's Vengeance Cracked Crystal Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43926)$"],
    userControl: true, suppress: 2000)]
    public void CrossExaflareCrystalFw(Event ev, ScriptAccessory sa)
    {
        // Framework Start
        if (_crossExaflareCrystalTriggered) return;

        _crossExaflareCrystalTriggered = true;
        sa.Log.Debug($"Crystal real-time distance calculation, range display enabled");
        _crystalCloseGuid = sa.Method.RegistFrameworkUpdateAction(CrystalCloseFrameworkAction);
    }

    [ScriptMethod(name: "Wyvern's Vengeance - Init Other Frameworks", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43926)$"],
        userControl: Debugging, suppress: 2000)]
    public void CrossExaflareCrystalFwRefresh(Event ev, ScriptAccessory sa)
    {
        // _crossExaflareCrystalTriggered = false;
        _dragonBeamCrystalTriggered = false;
        _edgeExaflareCrystalTriggered = false;
    }

    [ScriptMethod(name: "Crystals Recorder", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(1866[23])$"],
    userControl: Debugging)]
    public void CrystalRecord(Event ev, ScriptAccessory sa)
    {
        // 18662 small
        // 18663 large
        lock (_crystals)
        {
            var obj = sa.GetById(ev.SourceId);
            if (obj is null) return;

            _crystals.Add(new Crystal(obj, false));

            var dataId = JsonConvert.DeserializeObject<uint>(ev["DataId"]);
            if (isDebug) sa.Log.Debug($"Recorded the {_crystals.Count}nd {(dataId == 18662 ? "Small" : "Large")} crystal");
        }
    }
    [ScriptMethod(name: "Remove Crystals", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4392[45])$", "TargetIndex:1"],
    userControl: Debugging)]
    public void CrystalSplashRemove(Event ev, ScriptAccessory sa)
    {
        lock (_crystalsLock)
        {
            var obj = sa.GetById(ev.SourceId);
            if (obj is null) return;

            sa.WriteVisible(obj, false);

            sa.Method.RemoveDraw($"Crystal-{ev.SourceId}");
            _crystals.RemoveAll(x => x.Obj.GameObjectId == ev.SourceId);

            if (isDebug) sa.Log.Debug($"Crystal {ev.SourceId} exploded, removed. Remaining in list: {_crystals.Count}");
        }
    }

    [ScriptMethod(name: "Wyvern's Radiance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44808)$"])]
    public void WyvernsRadiance(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(12f), 1700, $"Wyvern's Radiance-{ev.SourceId}",
            new Vector4(1, 0, 0, ColorAlpha), scaleByTime: true);
    }
    #endregion

    [ScriptMethod(name: "Phase Change AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43935)$"],
    userControl: true)]
    public void PCAOENotify(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Triple AOE" : "Triple AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
        guardianArkveldPhase = GuardianArkveldPhase.Phase2;
        ClamorousChaseDict.Clear();
        ChainbladeBlowTriplePos.Clear();
        ChainbladeBlowTripleCount = 0;
    }

    #region Phase 2
    [ScriptMethod(name: "Clamorous Chase Check", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(0194|0195|0196|0197|0198|0199|019A|019B)$"],
    userControl: false)]
    public void ClamorousChaseCheck(Event ev, ScriptAccessory sa)
    {
        // TargetIcon
        // 0194 ONE     1
        // 0195 TWO     2
        // 0196 THREE   3
        // 0197 FOUR    4
        // 0198 FIVE    5
        // 0199 SIX     6
        // 019A SEVEN   7
        // 019B EIGHT   8
        lock (CountLock)
        {
            var index = 0;
            DebugMsg($"ClamorousChaseCheck Detected, id: {ev["Id"]}", sa);

            switch (ev["Id"])
            {
                case "0194":
                    index = 1;
                    ClamorousChaseDict.Add(index, ev.TargetId);
                    DebugMsg($"ClamorousChaseDict 1 added", sa);
                    break;
                case "0195":
                    index = 2;
                    ClamorousChaseDict.Add(index, ev.TargetId);
                    DebugMsg($"ClamorousChaseDict 2 added", sa);
                    break;
                case "0196":
                    index = 3;
                    ClamorousChaseDict.Add(index, ev.TargetId);
                    DebugMsg($"ClamorousChaseDict 3 added", sa);
                    break;
                case "0197":
                    index = 4;
                    ClamorousChaseDict.Add(index, ev.TargetId);
                    DebugMsg($"ClamorousChaseDict 4 added", sa);
                    break;
                case "0198":
                    index = 5;
                    ClamorousChaseDict.Add(index, ev.TargetId);
                    DebugMsg($"ClamorousChaseDict 5 added", sa);
                    break;
                case "0199":
                    index = 6;
                    ClamorousChaseDict.Add(index, ev.TargetId);
                    DebugMsg($"ClamorousChaseDict 6 added", sa);
                    break;
                case "019A":
                    index = 7;
                    ClamorousChaseDict.Add(index, ev.TargetId);
                    DebugMsg($"ClamorousChaseDict 7 added", sa);
                    break;
                case "019B":
                    index = 8;
                    ClamorousChaseDict.Add(index, ev.TargetId);
                    DebugMsg($"ClamorousChaseDict 8 added", sa);
                    break;
            }
        }
    }

    [ScriptMethod(name: "Clamorous Chase", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43955|43958)$"])]
    public void ClamorousChase(Event ev, ScriptAccessory sa)
    {
        // 43955 clockwise  
        // 43958 anti-clockwise
        lock (CountLock)
        {
            Vector3 posASafe = new Vector3(100.00f, 0f, 92f);
            if (ev.ActionId == 43955)
            {
                DebugMsg("Clamorous Chase 43955 clockwise Detected", sa);
                if (ClamorousChaseDict.ContainsKey(1) && ClamorousChaseDict[1] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 1 Detected", sa);
                    DrawHelper.DrawCircle(sa, posB, new Vector2(2f), 8900,
                        $"Clamorous Chase 1-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posB, new Vector2(2f), 8900,
                        $"Clamorous Chase Navi 1-{ev.SourceId}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 1-{ev.SourceId}", delay: 8900);
                }

                if (ClamorousChaseDict.ContainsKey(2) && ClamorousChaseDict[2] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 2 Detected", sa);
                    DrawHelper.DrawCircle(sa, posC, new Vector2(2f), 12000,
                        $"Clamorous Chase 2-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posC, new Vector2(2f), 12000,
                        $"Clamorous Chase Navi 2-{ev.SourceId}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 2-{ev.SourceId}", delay: 12000);
                }

                if (ClamorousChaseDict.ContainsKey(3) && ClamorousChaseDict[3] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 3 Detected", sa);
                    DrawHelper.DrawCircle(sa, posD, new Vector2(2f), 15100,
                        $"Clamorous Chase 3-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posD, new Vector2(2f), 15100,
                        $"Clamorous Chase Navi 3-{ev.SourceId}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 3-{ev.SourceId}", delay: 15100);
                }

                if (ClamorousChaseDict.ContainsKey(4) && ClamorousChaseDict[4] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 4 Detected", sa);
                    DrawHelper.DrawCircle(sa, posD, new Vector2(2f), 8900,
                        $"Clamorous Chase 4-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posASafe, new Vector2(2f), 8900,
                        $"Clamorous Chase Navi 4-{ev.SourceId}");

                    string msg = language == Language.Chinese ? "Wait in the safe zone, then bait." : "Wait in the safe zone, then bait.";
                    if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
                    if (isTTS) sa.Method.EdgeTTS($"{msg}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posA, new Vector2(2f), 9300,
                        $"Clamorous Chase Navi 4-{ev.SourceId}", delay: 8900);

                    if (isLead) DrawHelper.DrawCircle(sa, posA, new Vector2(2f), 9300,
                        $"Clamorous Chase 4-{ev.SourceId}",
                        sa.Data.DefaultSafeColor, delay: 8900);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 4-{ev.SourceId}", delay: 18200);
                }

                if (ClamorousChaseDict.ContainsKey(5) && ClamorousChaseDict[5] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 5 Detected", sa);
                    DrawHelper.DrawCircle(sa, posOne, new Vector2(2f), 13300,
                        $"Clamorous Chase 5-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 13300,
                        $"Clamorous Chase Navi 5-{ev.SourceId}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posB, new Vector2(2f), 8000,
                        $"Clamorous Chase Navi 5-{ev.SourceId}", delay: 13300);

                    if (isLead) DrawHelper.DrawCircle(sa, posB, new Vector2(2f), 8000,
                        $"Clamorous Chase 5-{ev.SourceId}",
                        sa.Data.DefaultSafeColor, delay: 13300);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 5-{ev.SourceId}", delay: 21300);
                }

                if (ClamorousChaseDict.ContainsKey(6) && ClamorousChaseDict[6] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 6 Detected", sa);
                    DrawHelper.DrawCircle(sa, posOne, new Vector2(2f), 16400,
                        $"Clamorous Chase 6-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 16400,
                        $"Clamorous Chase Navi 6-{ev.SourceId}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posC, new Vector2(2f), 8000,
                        $"Clamorous Chase Navi 6-{ev.SourceId}", delay: 16400);

                    DrawHelper.DrawCircle(sa, posC, new Vector2(2f), 8000,
                        $"Clamorous Chase 6-{ev.SourceId}",
                        sa.Data.DefaultSafeColor, delay: 16400);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 6-{ev.SourceId}", delay: 24400);
                }


                if (ClamorousChaseDict.ContainsKey(7) && ClamorousChaseDict[7] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 7 Detected", sa);
                    DrawHelper.DrawCircle(sa, posOne, new Vector2(2f), 19500,
                        $"Clamorous Chase 7-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 19500,
                        $"Clamorous Chase Navi 7-{ev.SourceId}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posD, new Vector2(2f), 8000,
                        $"Clamorous Chase Navi 7-{ev.SourceId}", delay: 19500);

                    DrawHelper.DrawCircle(sa, posD, new Vector2(2f), 8000,
                        $"Clamorous Chase 7-{ev.SourceId}",
                        sa.Data.DefaultSafeColor, delay: 19500);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 7-{ev.SourceId}", delay: 27500);
                }

                if (ClamorousChaseDict.ContainsKey(8) && ClamorousChaseDict[8] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 8 Detected", sa);
                    DrawHelper.DrawCircle(sa, posOne, new Vector2(2f), 22600,
                        $"Clamorous Chase 8-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 22600,
                        $"Clamorous Chase Navi 8-{ev.SourceId}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posA, new Vector2(2f), 8000,
                        $"Clamorous Chase Navi 8-{ev.SourceId}", delay: 22600);

                    DrawHelper.DrawCircle(sa, posA, new Vector2(2f), 8000,
                        $"Clamorous Chase 8-{ev.SourceId}",
                        sa.Data.DefaultSafeColor, delay: 22600);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 8-{ev.SourceId}", delay: 30600);
                }
            }
            else
            {
                DebugMsg("Clamorous Chase 43958 anti-clockwise Detected", sa);
                if (ClamorousChaseDict.ContainsKey(1) && ClamorousChaseDict[1] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 1 Detected", sa);
                    DrawHelper.DrawCircle(sa, posD, new Vector2(2f), 8900,
                        $"Clamorous Chase 1-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posD, new Vector2(2f), 8900,
                        $"Clamorous Chase Navi 1-{ev.SourceId}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 1-{ev.SourceId}", delay: 8900);
                }

                if (ClamorousChaseDict.ContainsKey(2) && ClamorousChaseDict[2] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 2 Detected", sa);
                    DrawHelper.DrawCircle(sa, posC, new Vector2(2f), 12000,
                        $"Clamorous Chase 2-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posC, new Vector2(2f), 12000,
                        $"Clamorous Chase Navi 2-{ev.SourceId}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 2-{ev.SourceId}", delay: 12000);
                }

                if (ClamorousChaseDict.ContainsKey(3) && ClamorousChaseDict[3] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 3 Detected", sa);
                    DrawHelper.DrawCircle(sa, posB, new Vector2(2f), 15100,
                        $"Clamorous Chase 3-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posB, new Vector2(2f), 15100,
                        $"Clamorous Chase Navi 3-{ev.SourceId}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 3-{ev.SourceId}", delay: 15100);
                }

                if (ClamorousChaseDict.ContainsKey(4) && ClamorousChaseDict[4] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 4 Detected", sa);
                    DrawHelper.DrawCircle(sa, posASafe, new Vector2(2f), 8900,
                        $"Clamorous Chase 4-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posASafe, new Vector2(2f), 8900,
                        $"Clamorous Chase Navi 4-{ev.SourceId}");

                    string msg = language == Language.Chinese ? "Wait in the safe zone, then bait." : "Wait in the safe zone, then bait.";
                    if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
                    if (isTTS) sa.Method.EdgeTTS($"{msg}");


                    if (isLead) DrawHelper.DrawDisplacement(sa, posA, new Vector2(2f), 9300,
                        $"Clamorous Chase Navi 4-{ev.SourceId}", delay: 8900);

                    DrawHelper.DrawCircle(sa, posA, new Vector2(2f), 9300,
                        $"Clamorous Chase 4-{ev.SourceId}",
                        sa.Data.DefaultSafeColor, delay: 8900);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 4-{ev.SourceId}", delay: 18200);
                }

                if (ClamorousChaseDict.ContainsKey(5) && ClamorousChaseDict[5] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 5 Detected", sa);
                    DrawHelper.DrawCircle(sa, posOne, new Vector2(2f), 13300,
                        $"Clamorous Chase 5-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 13300,
                        $"Clamorous Chase Navi 5-{ev.SourceId}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posD, new Vector2(2f), 8000,
                        $"Clamorous Chase Navi 5-{ev.SourceId}", delay: 13300);

                    DrawHelper.DrawCircle(sa, posD, new Vector2(2f), 8000,
                        $"Clamorous Chase 5-{ev.SourceId}",
                        sa.Data.DefaultSafeColor, delay: 13300);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 5-{ev.SourceId}", delay: 21300);
                }

                if (ClamorousChaseDict.ContainsKey(6) && ClamorousChaseDict[6] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 6 Detected", sa);
                    DrawHelper.DrawCircle(sa, posOne, new Vector2(2f), 16400,
                        $"Clamorous Chase 6-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 16400,
                        $"Clamorous Chase Navi 6-{ev.SourceId}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posC, new Vector2(2f), 8000,
                        $"Clamorous Chase Navi 6-{ev.SourceId}", delay: 16400);

                    DrawHelper.DrawCircle(sa, posC, new Vector2(2f), 8000,
                        $"Clamorous Chase 6-{ev.SourceId}",
                        sa.Data.DefaultSafeColor, delay: 16400);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 6-{ev.SourceId}", delay: 24400);
                }


                if (ClamorousChaseDict.ContainsKey(7) && ClamorousChaseDict[7] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 7 Detected", sa);
                    DrawHelper.DrawCircle(sa, posOne, new Vector2(2f), 19500,
                        $"Clamorous Chase 7-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 19500,
                        $"Clamorous Chase Navi 7-{ev.SourceId}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posB, new Vector2(2f), 8000,
                        $"Clamorous Chase Navi 7-{ev.SourceId}", delay: 19500);

                    DrawHelper.DrawCircle(sa, posB, new Vector2(2f), 8000,
                        $"Clamorous Chase 7-{ev.SourceId}",
                        sa.Data.DefaultSafeColor, delay: 19500);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 7-{ev.SourceId}", delay: 27500);
                }

                if (ClamorousChaseDict.ContainsKey(8) && ClamorousChaseDict[8] == sa.Data.Me)
                {
                    DebugMsg("Clamorous Chase 8 Detected", sa);
                    DrawHelper.DrawCircle(sa, posOne, new Vector2(2f), 22600,
                        $"Clamorous Chase 8-{ev.SourceId}",
                        sa.Data.DefaultSafeColor);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 22600,
                        $"Clamorous Chase Navi 8-{ev.SourceId}");

                    if (isLead) DrawHelper.DrawDisplacement(sa, posA, new Vector2(2f), 8000,
                        $"Clamorous Chase Navi 8-{ev.SourceId}", delay: 22600);

                    DrawHelper.DrawCircle(sa, posA, new Vector2(2f), 8000,
                        $"Clamorous Chase 8-{ev.SourceId}",
                        sa.Data.DefaultSafeColor, delay: 22600);

                    if (isLead) DrawHelper.DrawDisplacement(sa, posOne, new Vector2(2f), 4000,
                        $"Clamorous Chase Navi 8-{ev.SourceId}", delay: 30600);
                }
            }
        }
    }

    [ScriptMethod(name: "Wyvern's Weal Target", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(01D6)$"], suppress: 30000)]
    public void WyvernsWealTarget(Event ev, ScriptAccessory sa)
    { 
        var RightTopPos = new Vector3(110.74f, 0f, 84.63f);
        var LeftTopPos = new Vector3(89.87f, 0f, 83.45f);
        var RightMidPos = new Vector3(118.30f, 0.00f, 96.75f);
        var LeftMidPos = new Vector3(82.35f, 0.00f, 96.75f);

        if (ev.TargetId == sa.Data.Me)
        {
            if (isLead) DrawHelper.DrawDisplacement(sa, RightTopPos, new Vector2(2f), 7700, "Wyvern's Weal Target Lead Navi");
            string msg = language == Language.Chinese ? "Bait at the top-right corner" : "Bait at top-right corner";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 5000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");

            if (isLead) DrawHelper.DrawDisplacement(sa, LeftTopPos, new Vector2(2f), 9000, "Wyvern's Weal Target Lead Navi", delay: 7700);
            // Check H1 Group
            bool IsH1Group(int index) => index is 0 or 2 or 4 or 5;

            if (IsH1Group(GetMyIndex(sa)))
            {
                if (isLead) DrawHelper.DrawDisplacement(sa, RightTopPos, new Vector2(2f), 9000, "Wyvern's Weal Target Lead Navi", delay: 16700);
            } else
            {
                if (isLead) DrawHelper.DrawDisplacement(sa, RightMidPos, new Vector2(2f), 9000, "Wyvern's Weal Target Lead Navi", delay: 16700);
            }
        } else
        {
            if (isLead) DrawHelper.DrawDisplacement(sa, LeftTopPos, new Vector2(2f), 7700, "Wyvern's Weal Target Lead Navi");
            string msg = language == Language.Chinese ? "Wait at the top-left corner" : "Wait at top-left corner";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 5000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");

            bool IsH1Group(int index) => index is 0 or 2 or 4 or 5;

            if (IsH1Group(GetMyIndex(sa)))
            {
                if (isLead) DrawHelper.DrawDisplacement(sa, RightTopPos, new Vector2(2f), 9000, "Wyvern's Weal Target Lead Navi", delay: 16700);
            }
            else
            {
                if (isLead) DrawHelper.DrawDisplacement(sa, RightMidPos, new Vector2(2f), 9000, "Wyvern's Weal Target Lead Navi", delay: 16700);
            }

            if (isLead) DrawHelper.DrawDisplacement(sa, LeftTopPos, new Vector2(2f), 9000, "Wyvern's Weal Target Lead Navi", delay: 25700);
        }
    }

    [ScriptMethod(name: "Wyvern's Weal Cracked Crystal Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43937)$"],
    userControl: true, suppress: 2000)]
    public void DragonBeamCrystalFw(Event ev, ScriptAccessory sa)
    {
        if (_dragonBeamCrystalTriggered) return;
        _dragonBeamCrystalTriggered = true;
        if (isDebug) sa.Log.Debug($"Crystal real-time distance calculation, range display enabled");
        _crystalCloseGuid = sa.Method.RegistFrameworkUpdateAction(CrystalCloseFrameworkAction);
    }

    [ScriptMethod(name: "Wyvern's Weal - Init Other Frameworks", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43937)$"],
    userControl: Debugging, suppress: 2000)]
    public void DragonBeamCrystalFwRefresh(Event ev, ScriptAccessory sa)
    {
        _crossExaflareCrystalTriggered = false;
        _edgeExaflareCrystalTriggered = false;
    }

    [ScriptMethod(name: "P2 Chainblade Blow", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45083|45086)$"])]
    public void P2ChainbladeBlow(Event ev, ScriptAccessory sa)
    {
        var spos = ev.SourcePosition;
        var srot = ev.SourceRotation;

        switch (ev.ActionId)
        {
            // 45086 boss right first
            case 45083:
                {
                    string msg = language == Language.Chinese ? "Left →→ Right" : "Left →→ Right";
                    string msg1 = language == Language.Chinese ? "Left then Right" : "Left then Right";
                    if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
                    if (isTTS) sa.Method.EdgeTTS($"{msg1}");

                    float rightX = spos.X - 20 * MathF.Sin(srot) + 7 * MathF.Cos(srot);
                    float rightZ = spos.Z - 20 * MathF.Cos(srot) - 7 * MathF.Sin(srot);
                    Vector3 rightPos = new Vector3(rightX, spos.Y, rightZ);

                    float leftX = spos.X - 20 * MathF.Sin(srot) - 7 * MathF.Cos(srot);
                    float leftZ = spos.Z - 20 * MathF.Cos(srot) + 7 * MathF.Sin(srot);
                    Vector3 leftPos = new Vector3(leftX, spos.Y, leftZ);

                    DrawHelper.DrawRectPosNoTarget(sa, rightPos, new Vector2(28f, 80f), srot, 6700,
                        $"Chainblade Blow right-{ev.SourceId}", sa.Data.DefaultDangerColor);

                    DrawHelper.DrawRectPosNoTarget(sa, leftPos, new Vector2(28f, 80f), srot, 3500,
                        $"Chainblade Blow left-{ev.SourceId}", sa.Data.DefaultDangerColor, delay: 6700);
                    break;
                }
            // 45083 boss left first
            case 45086:
                {
                    string msg = language == Language.Chinese ? "Left ←← Right" : "Left ←← Right";
                    string msg1 = language == Language.Chinese ? "Right then Left" : "Right then Left";
                    if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
                    if (isTTS) sa.Method.EdgeTTS($"{msg1}");

                    float rightX = spos.X - 20 * MathF.Sin(srot) + 7 * MathF.Cos(srot);
                    float rightZ = spos.Z - 20 * MathF.Cos(srot) - 7 * MathF.Sin(srot);
                    Vector3 rightPos = new Vector3(rightX, spos.Y, rightZ);

                    float leftX = spos.X - 20 * MathF.Sin(srot) - 7 * MathF.Cos(srot);
                    float leftZ = spos.Z - 20 * MathF.Cos(srot) + 7 * MathF.Sin(srot);
                    Vector3 leftPos = new Vector3(leftX, spos.Y, leftZ);

                    DrawHelper.DrawRectPosNoTarget(sa, leftPos, new Vector2(28f, 80f), srot, 6700,
                        $"Chainblade Blow left-{ev.SourceId}", sa.Data.DefaultDangerColor);

                    DrawHelper.DrawRectPosNoTarget(sa, rightPos, new Vector2(28f, 80f), srot, 3500,
                        $"Chainblade Blow right-{ev.SourceId}", sa.Data.DefaultDangerColor, delay: 6700);
                    break;
                }
        }
    }




    [ScriptMethod(name: "P2 Wyvern's Siegeflight", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45071|45072)$"])]
    public void P2WyvernsSiegeflight(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            // 45071 small
            // 45072 split
            case 45071:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(8f, 40f), 6200, $"Wyvern's Siegeflight mid-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha));
                    break;
                }
            case 45072:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20f, 40f), 3400, $"Wyvern's Siegeflight Left-{ev.SourceId}",
                        sa.Data.DefaultDangerColor, offset: new Vector3(12f, 0, 0f), delay: 6600);
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20f, 40f), 3400, $"Wyvern's Siegeflight Right-{ev.SourceId}",
                        sa.Data.DefaultDangerColor, offset: new Vector3(-12f, 0, 0f), delay: 6600);
                    break;
                }
        }
    }

    [ScriptMethod(name: "P2 Wyvern's Ouroblade", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45107|45108)$"])]
    public void P2WyvernsOuroblade(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            // 45107 left
            // 45108 right
            case 45107:
                {
                    DrawHelper.DrawFanObject(sa, ev.SourceId, float.Pi / 2, new Vector2(40f), 180, 6700, $"Wyvern's Ouroblade left-{ev.SourceId}", sa.Data.DefaultDangerColor,
                        scaleByTime: false);
                    break;
                }
            case 45108:
                {
                    DrawHelper.DrawFanObject(sa, ev.SourceId, -float.Pi / 2, new Vector2(40f), 180, 6700, $"Wyvern's Ouroblade right-{ev.SourceId}", sa.Data.DefaultDangerColor,
                        scaleByTime: false);
                    break;
                }
        }
    }

    [ScriptMethod(name: "Steeltail Thrust", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43949|44806)$"])]
    public void SteeltailThrust(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTargetWithRot(sa, ev.SourceId, new Vector2(6f, 30f), float.Pi, 3300, $"Steeltail Thrust-{ev.SourceId}", sa.Data.DefaultDangerColor);
        string msg = language == Language.Chinese ? "Avoid behind the boss" : "Avoid behind the boss";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 3000, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    public Vector3 WrathfulRattlePos = new Vector3(0, 0, 0);
    public float WrathfulRattleRot = 0f;
    [ScriptMethod(name: "Wrathful Rattle", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43944)$"])]
    public void WrathfulRattle(Event ev, ScriptAccessory sa)
    {
        DebugMsg($"In WrathfulRattle", sa);
        lock (CountLock)
        {
            var spos = ev.SourcePosition;
            WrathfulRattlePos = spos;
            var srot = ev.SourceRotation;
            WrathfulRattleRot = srot;

            int[] delays = { 3200, 6200, 9200, 12200 };
            int[] durations = { 3000, 3000, 3000, 3000 };


            DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(8f, 40f), 3200, $"Wrathful Rattle main-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(0, 0, 20));

            for (int i = 0; i < delays.Length; i++)
            {
                float sideDist = 6f + i * 4f;

                float rightX = WrathfulRattlePos.X - 20 * MathF.Sin(WrathfulRattleRot) + sideDist * MathF.Cos(WrathfulRattleRot);
                float rightZ = WrathfulRattlePos.Z - 20 * MathF.Cos(WrathfulRattleRot) - sideDist * MathF.Sin(WrathfulRattleRot);
                Vector3 rightPos = new Vector3(rightX, WrathfulRattlePos.Y, rightZ);


                float leftX = WrathfulRattlePos.X - 20 * MathF.Sin(WrathfulRattleRot) - sideDist * MathF.Cos(WrathfulRattleRot);
                float leftZ = WrathfulRattlePos.Z - 20 * MathF.Cos(WrathfulRattleRot) + sideDist * MathF.Sin(WrathfulRattleRot);
                Vector3 leftPos = new Vector3(leftX, WrathfulRattlePos.Y, leftZ);

                DrawHelper.DrawRectPosNoTarget(sa, rightPos, new Vector2(4f, 40f), WrathfulRattleRot, durations[i],
                    $"Wrathful Rattle right-{ev.SourceId}-{i}",
                    color: new Vector4(1, 0, 0, ColorAlpha), delay: delays[i]);

                DrawHelper.DrawRectPosNoTarget(sa, leftPos, new Vector2(4f, 40f), WrathfulRattleRot, durations[i],
                    $"Wrathful Rattle left-{ev.SourceId}-{i}",
                    color: new Vector4(1, 0, 0, ColorAlpha), delay: delays[i]);
            }
        }
    }

    [ScriptMethod(name: "Wrathful Rattle Return", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43946)$"])]
    public void WrathfulRattleReturn(Event ev, ScriptAccessory sa)
    {
        lock (CountLock)
        {
            var Epos = new Vector3(118.00f, 0.00f, 100.00f);
            var Wpos = new Vector3(82.00f, 0.00f, 100.00f);
            var Npos = new Vector3(100.00f, 0.00f, 82.00f);
            var Spos = new Vector3(100.00f, 0.00f, 118.00f);

            var spos = ev.SourcePosition;
            var srot = ev.SourceRotation;

            int[] delays = { 2800, 4500, 7000, 9500, 12000, 15000, 17500, 20000, 22500 };
            int[] durations = { 1700, 1700, 2500, 2500, 3000, 2500, 2500, 2500, 2500 };


            DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(8f, 40f), 2800, $"Wrathful Rattle main-{ev.SourceId}",
                sa.Data.DefaultDangerColor, offset: new Vector3(0, 0, 20));
            // E to W
            if (Vector3.Distance(spos, Epos) < 10)
            {
                for (int i = 0; i < delays.Length; i++)
                {
                    float sideDist = 4f + i * 4f;

                    float X = spos.X + sideDist * MathF.Cos(srot);
                    float Z = spos.Z - 20 * MathF.Cos(srot) + sideDist * MathF.Sin(srot);
                    Vector3 Pos = new Vector3(X, spos.Y, Z);

                    DrawHelper.DrawRectPosNoTarget(sa, Pos, new Vector2(4f, 40f), srot, durations[i],
                        $"Wrathful Rattle right-{ev.SourceId}-{i}",
                        color: new Vector4(1, 0, 0, ColorAlpha), delay: delays[i]);
                    DebugMsg($"Pos: {Pos}, srot: {srot}， MathF.Cos(srot)  {MathF.Cos(srot)}", sa);
                }
            }   // W to E
            else if (Vector3.Distance(spos, Wpos) < 10)
            {
                for (int i = 0; i < delays.Length; i++)
                {
                    float sideDist = 4f + i * 4f;

                    float X = spos.X - sideDist * MathF.Cos(srot);
                    float Z = spos.Z - 20 * MathF.Cos(srot) + sideDist * MathF.Sin(srot);
                    Vector3 Pos = new Vector3(X, spos.Y, Z);

                    DrawHelper.DrawRectPosNoTarget(sa, Pos, new Vector2(4f, 40f), srot, durations[i],
                        $"Wrathful Rattle left-{ev.SourceId}-{i}",
                        color: new Vector4(1, 0, 0, ColorAlpha), delay: delays[i]);
                }
            }   // S to N
            else if (Vector3.Distance(spos, Spos) < 10)
            {
                for (int i = 0; i < delays.Length; i++)
                {
                    float sideDist = 4f + i * 4f;

                    float X = spos.X - 20 * MathF.Sin(srot) + sideDist * MathF.Cos(srot);
                    float Z = spos.Z - 20 * MathF.Cos(srot) + sideDist * MathF.Sin(srot);
                    Vector3 Pos = new Vector3(X, spos.Y, Z);

                    DrawHelper.DrawRectPosNoTarget(sa, Pos, new Vector2(4f, 40f), srot, durations[i],
                        $"Wrathful Rattle S-N-{ev.SourceId}-{i}",
                        color: new Vector4(1, 0, 0, ColorAlpha), delay: delays[i]);
                }
            }   // N to S
            else if (Vector3.Distance(spos, Npos) < 10)
            {
                for (int i = 0; i < delays.Length; i++)
                {
                    float sideDist = 4f + i * 4f;

                    float X = spos.X - 20 * MathF.Sin(srot) - sideDist * MathF.Cos(srot);
                    float Z = spos.Z - 20 * MathF.Cos(srot) - sideDist * MathF.Sin(srot);
                    Vector3 Pos = new Vector3(X, spos.Y, Z);

                    DrawHelper.DrawRectPosNoTarget(sa, Pos, new Vector2(4f, 40f), srot, durations[i],
                        $"Wrathful Rattle N-S-{ev.SourceId}-{i}",
                        color: new Vector4(1, 0, 0, ColorAlpha), delay: delays[i]);
                }
            }
        }
    }

    [ScriptMethod(name: "P2 Arena Edge Wyvern's Vengeance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43952)$"])]
    public void P2WyvernsVengeance(Event ev, ScriptAccessory sa)
    {
        var startPos = ev.EffectPosition;
        var srot = ev.SourceRotation;

        float moveDistance = 8f;
        //int[] durations = { 7700, 1300, 1300, 1300, 1300, 1300 };
        //int[] delays = { 0, 7700, 9000, 10300, 11600, 12900 };
        int[] durations = { 7700, 2700, 2700, 2700, 2700, 2700 };
        int[] delays = { 0, 6400, 7700, 9000, 10300, 11600 };

        for (int i = 0; i < 6; i++)
        {
            float X = startPos.X + moveDistance * i * MathF.Sin(srot);
            float Z = startPos.Z + moveDistance * i * MathF.Cos(srot);
            Vector3 drawPos = new Vector3(X, startPos.Y, Z);

            DrawHelper.DrawCircle(sa, drawPos, new Vector2(6), durations[i],
                $"P2 Wyvern's Vengeance-{i}-{ev.SourceId}",
                color: new Vector4(1, 0, 0, ColorAlpha),
                scaleByTime: false,
                delay: delays[i]);
        }
    }

    [ScriptMethod(name: "P2 Arena Edge Wyvern's Vengeance Cracked Crystal Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43952)$"],
    userControl: true, suppress: 2000)]
    public void EdgeExaflareCrystalFw(Event ev, ScriptAccessory sa)
    {
        if (_edgeExaflareCrystalTriggered) return;

        _edgeExaflareCrystalTriggered = true;
        sa.Log.Debug($"Crystal real-time distance calculation, range display enabled");
        _crystalCloseGuid = sa.Method.RegistFrameworkUpdateAction(CrystalCloseFrameworkAction);
    }

    [ScriptMethod(name: "P2 Arena Edge Wyvern's Vengeance - Init Other Frameworks", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43952)$"],
        userControl: Debugging, suppress: 2000)]
    public void EdgeExaflareCrystalFwRefresh(Event ev, ScriptAccessory sa)
    {
        _crossExaflareCrystalTriggered = false;
        _dragonBeamCrystalTriggered = false;
    }

    [ScriptMethod(name: "P2 Chariot", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43933)$"])]
    public void P2Chariot(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(12f), 4700,
            $"P2 Chariot-{ev.SourceId}",
            sa.Data.DefaultDangerColor);

    }

    #endregion
    #region Framework
    private void CrystalCloseFrameworkAction()
    {
        ScriptAccessory sa = _sa;
        var closeDistance = 2f;

        var myobj = sa.Data.MyObject;
        if (myobj is null) return;
        var myPos = myobj.Position;

        lock (_crystals)
        {
            if (_crystals.Count == 0)
            {
                if (isDebug) sa.Log.Debug($"Crystal count is zero, clearing");
                sa.Method.UnregistFrameworkUpdateAction(_crystalCloseGuid);
            }

            foreach (var crystal in _crystals)
            {
                var distance = Vector3.Distance(crystal.Obj.Position, myPos);
                var isSmallCrystal = crystal.Obj.DataId == 18662;
                var crystalRange = isSmallCrystal ? 6 : 12;
                bool isClose = distance < crystalRange + closeDistance;

                if (isClose && !crystal.Drawn)
                {
                    if (isDebug) sa.Log.Debug($"Player is close to crystal {crystal.Obj.GameObjectId}, displaying drawing, range {crystalRange}");
                    DrawHelper.DrawCircleObject(sa, crystal.Obj.GameObjectId, new Vector2(crystalRange), 60000, $"Crystal-{crystal.Obj.GameObjectId}",
                        new Vector4(0, 1, 1, ColorAlpha), scaleByTime: false);
                    crystal.Drawn = true;
                }
                else if (!isClose && crystal.Drawn)
                {
                    if (isDebug) sa.Log.Debug($"Player is far from crystal {crystal.Obj.GameObjectId}, deleting drawing");
                    sa.Method.RemoveDraw($"Crystal-{crystal.Obj.GameObjectId}");
                    crystal.Drawn = false;
                }
            }
        }
    }
    #endregion


    [ScriptMethod(name: "Unit - Remove & Refresh ALL", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:73"],
    userControl: Debugging)]
    public void Uninit(Event ev, ScriptAccessory sa)
    {
        if (isDebug) sa.Log.Debug($"Uninit Triggered");
        sa.Method.RemoveDraw(".*");
        _crystals.RemoveAll(x => x.Obj.GameObjectId == ev.SourceId);
        RefreshParams();

        sa.Method.ClearFrameworkUpdateAction(this);
    }


    #region Priority Dictionary
    public class PriorityDict
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public ScriptAccessory sa { get; set; } = null!;
        // ReSharper disable once NullableWarningSuppressionIsUsed
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

        /// <summary>
        /// Increase priority for a specific Key
        /// </summary>
        /// <param name="idx">key</param>
        /// <param name="priority">priority value</param>
        public void AddPriority(int idx, int priority)
        {
            Priorities[idx] += priority;
        }

        /// <summary>
        /// Find the smallest `num` values from Priorities and return a new Dict
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public List<KeyValuePair<int, int>> SelectSmallPriorityIndices(int num)
        {
            return SelectMiddlePriorityIndices(0, num);
        }

        /// <summary>
        /// Find the largest `num` values from Priorities and return a new Dict
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public List<KeyValuePair<int, int>> SelectLargePriorityIndices(int num)
        {
            return SelectMiddlePriorityIndices(0, num, true);
        }

        /// <summary>
        /// Find the intermediate values in ascending order from Priorities and return a new Dict
        /// </summary>
        /// <param name="skip">Skip `skip` elements. If starting from the second element, skip=1</param>
        /// <param name="num"></param>
        /// <param name="descending">Descending order, default false</param>
        /// <returns></returns>
        public List<KeyValuePair<int, int>> SelectMiddlePriorityIndices(int skip, int num, bool descending = false)
        {
            if (Priorities.Count < skip + num)
                return new List<KeyValuePair<int, int>>();

            IEnumerable<KeyValuePair<int, int>> sortedPriorities;
            if (descending)
            {
                // Sort by value descending, then by key, skip `skip` elements, take `num` key-value pairs
                sortedPriorities = Priorities
                    .OrderByDescending(pair => pair.Value) // Sort by value first
                    .ThenBy(pair => pair.Key) // Then by key
                    .Skip(skip) // Skip first `skip` elements
                    .Take(num); // Take `num` key-value pairs
            }
            else
            {
                // Sort by value ascending, then by key, skip `skip` elements, take `num` key-value pairs
                sortedPriorities = Priorities
                    .OrderBy(pair => pair.Value) // Sort by value first
                    .ThenBy(pair => pair.Key) // Then by key
                    .Skip(skip) // Skip first `skip` elements
                    .Take(num); // Take `num` key-value pairs
            }

            return sortedPriorities.ToList();
        }

        /// <summary>
        /// Find the data at the `idx`-th position in ascending order from Priorities and return a new Dict
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="descending">Descending order, default false</param>
        /// <returns></returns>
        public KeyValuePair<int, int> SelectSpecificPriorityIndex(int idx, bool descending = false)
        {
            var sortedPriorities = SelectMiddlePriorityIndices(0, Priorities.Count, descending);
            return sortedPriorities[idx];
        }

        /// <summary>
        /// Find the rank (position after sorting) of the value corresponding to a given key from Priorities
        /// </summary>
        /// <param name="key"></param>
        /// <param name="descending">Descending order, default false</param>
        /// <returns></returns>
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

        /// <summary>
        /// Add priority values in bulk
        /// Usually used for special priorities (e.g., H-T-D-H)
        /// </summary>
        /// <param name="priorities"></param>
        public void AddPriorities(List<int> priorities)
        {
            if (Priorities.Count != priorities.Count)
                throw new ArgumentException("Input list length differs from internal setting length");

            for (var i = 0; i < Priorities.Count; i++)
                AddPriority(i, priorities[i]);
        }

        /// <summary>
        /// Output the Priority dictionary's Keys and priorities
        /// </summary>
        /// <returns></returns>
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

    #endregion Priority Dictionary

}


#region Function Collections
public static class EventExtensions
{
    private static bool ParseHexId(string? idStr, out uint id)
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

    public static uint Id0(this Event @event)
    {
        return ParseHexId(@event["Id"], out var id) ? id : 0;
    }

    public static uint Index(this Event ev)
    {
        return ParseHexId(ev["Index"], out var index) ? index : 0;
    }

    public static uint DataId(this Event ev)
    {
        return JsonConvert.DeserializeObject<uint>(ev["DataId"]);
    }
}


public static class IbcHelper
{
    public static IGameObject? GetById(this ScriptAccessory sa, ulong gameObjectId)
    {
        return sa.Data.Objects.SearchById(gameObjectId);
    }

    public static IGameObject? GetMe(this ScriptAccessory sa)
    {
        return sa.Data.Objects.LocalPlayer;
    }

    public static IEnumerable<IGameObject?> GetByDataId(this ScriptAccessory sa, uint dataId)
    {
        return sa.Data.Objects.Where(x => x.DataId == dataId);
    }

    public static string GetPlayerJob(this ScriptAccessory sa, IPlayerCharacter? playerObject, bool fullName = false)
    {
        if (playerObject == null) return "None";
        return fullName ? playerObject.ClassJob.Value.Name.ToString() : playerObject.ClassJob.Value.Abbreviation.ToString();
    }

    /// <summary>
    /// Get the player's role
    /// Return: "Tank" / "Healer" / "Melee DPS" / "Ranged DPS" / "Unknown" / "None"
    /// </summary>
    public static string GetPlayerRole(this ScriptAccessory sa, IPlayerCharacter? playerObject)
    {
        if (playerObject == null) return "None";
        return playerObject.ClassJob.Value.Role switch
        {
            1 => "Tank",
            4 => "Healer",
            2 => "Melee DPS",
            3 => "Ranged DPS",
            _ => "Unknown"
        };
    }

    public static float GetStatusRemainingTime(this ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return 0;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return charaStruct->GetStatusManager()->GetRemainingTime(statusIdx);
        }
    }

    public static bool HasStatus(this ScriptAccessory sa, IBattleChara? battleChara, uint statusId)
    {
        if (battleChara == null || !battleChara.IsValid()) return false;
        unsafe
        {
            BattleChara* charaStruct = (BattleChara*)battleChara.Address;
            var statusIdx = charaStruct->GetStatusManager()->GetStatusIndex(statusId);
            return statusIdx != -1;
        }
    }

    public static float GetHitboxRadius(IGameObject obj)
    {
        if (obj == null || !obj.IsValid()) return -1;
        return obj.HitboxRadius;
    }

    public static string GetObjectName(IGameObject obj)
    {
        if (obj == null || !obj.IsValid()) return "None";
        return obj.Name.ToString();
    }

    /// <summary>
    /// Get the EntityId of the object with the specified marker index
    /// </summary>
    public static unsafe ulong GetMarkerEntityId(uint markerIndex)
    {
        var markingController = MarkingController.Instance();
        if (markingController == null) return 0;
        if (markerIndex >= 17) return 0;

        return markingController->Markers[(int)markerIndex];
    }

    /// <summary>
    /// Get the marker on the object
    /// </summary>
    /// <returns>MarkType</returns>
    public static MarkType GetObjectMarker(IGameObject? obj)
    {
        if (obj == null || !obj.IsValid()) return MarkType.None;

        ulong targetEntityId = obj.EntityId;

        for (uint i = 0; i < 17; i++)
        {
            var markerEntityId = GetMarkerEntityId(i);
            if (markerEntityId == targetEntityId)
            {
                return (MarkType)i;
            }
        }

        return MarkType.None;
    }

    /// <summary>
    /// Check if the object has a specific marker
    /// </summary>
    public static bool HasMarker(IGameObject? obj, MarkType markType)
    {
        return GetObjectMarker(obj) == markType;
    }

    /// <summary>
    /// Check if the object has any marker
    /// </summary>
    public static bool HasAnyMarker(IGameObject? obj)
    {
        return GetObjectMarker(obj) != MarkType.None;
    }

    private static ulong GetMarkerForObject(IGameObject? obj)
    {
        if (obj == null) return 0;
        unsafe
        {
            for (uint i = 0; i < 17; i++)
            {
                var markerEntityId = GetMarkerEntityId(i);
                if (markerEntityId == obj.EntityId)
                {
                    return markerEntityId;
                }
            }
        }
        return 0;
    }

    private static MarkType GetMarkerTypeForObject(IGameObject? obj)
    {
        if (obj == null) return MarkType.None;
        unsafe
        {
            for (uint i = 0; i < 17; i++)
            {
                var markerEntityId = GetMarkerEntityId(i);
                if (markerEntityId == obj.EntityId)
                {
                    return (MarkType)i;
                }
            }
        }
        return MarkType.None;
    }

    /// <summary>
    /// Get the name of the marker
    /// </summary>
    public static string GetMarkerName(MarkType markType)
    {
        return markType switch
        {
            MarkType.Attack1 => "Attack 1",
            MarkType.Attack2 => "Attack 2",
            MarkType.Attack3 => "Attack 3",
            MarkType.Attack4 => "Attack 4",
            MarkType.Attack5 => "Attack 5",
            MarkType.Bind1 => "Bind 1",
            MarkType.Bind2 => "Bind 2",
            MarkType.Bind3 => "Bind 3",
            MarkType.Ignore1 => "Ignore 1",
            MarkType.Ignore2 => "Ignore 2",
            MarkType.Square => "Square",
            MarkType.Circle => "Circle",
            MarkType.Cross => "Cross",
            MarkType.Triangle => "Triangle",
            MarkType.Attack6 => "Attack 6",
            MarkType.Attack7 => "Attack 7",
            MarkType.Attack8 => "Attack 8",
            _ => "None"
        };
    }
}

public enum MarkType
{
    None = -1,
    Attack1 = 0,
    Attack2 = 1,
    Attack3 = 2,
    Attack4 = 3,
    Attack5 = 4,
    Bind1 = 5,
    Bind2 = 6,
    Bind3 = 7,
    Ignore1 = 8,
    Ignore2 = 9,
    Square = 10,
    Circle = 11,
    Cross = 12,
    Triangle = 13,
    Attack6 = 14,
    Attack7 = 15,
    Attack8 = 16,
    Count = 17
}

public static class ActionExt
{
    public static unsafe bool IsReadyWithCanCast(uint actionId, ActionType actionType)
    {
        var am = ActionManager.Instance();
        if (am == null) return false;

        var adjustedId = am->GetAdjustedActionId(actionId);

        // 0 = Ready）
        if (am->GetActionStatus(actionType, adjustedId) != 0)
            return false;

        ulong targetId = 0;
        var ts = TargetSystem.Instance();
        if (ts != null && ts->GetTargetObject() != null)
            targetId = ts->GetTargetObject()->GetGameObjectId();

        return am->GetActionStatus(actionType, adjustedId, targetId) == 0;
    }

    public static bool IsSpellReady(this uint spellId) => IsReadyWithCanCast(spellId, ActionType.Action);
    public static bool IsAbilityReady(this uint abilityId) => IsReadyWithCanCast(abilityId, ActionType.EventAction);
}

#region Calculation Functions

public static class MathTools
{
    public static float DegToRad(this float deg) => (deg + 360f) % 360f / 180f * float.Pi;
    public static float RadToDeg(this float rad) => (rad + 2 * float.Pi) % (2 * float.Pi) / float.Pi * 180f;

    /// <summary>
    /// Normalize radian to the range -π to π
    /// </summary>
    public static float NormalizeRadian(this float rad)
    {
        rad = (rad + 2 * float.Pi) % (2 * float.Pi); // First convert to 0-2π
        if (rad > float.Pi) rad -= 2 * float.Pi; // If greater than π, convert to negative range
        return rad;
    }

    /// <summary>
    /// Get the radian value of an arbitrary point relative to a center point, with direction (0, 0, 1) as 0 and (1, 0, 0) as pi/2.
    /// I.e., increases counter-clockwise.
    /// </summary>
    /// <param name="point">Arbitrary point</param>
    /// <param name="center">Center point</param>
    /// <returns></returns>
    public static float GetRadian(this Vector3 point, Vector3 center)
        => MathF.Atan2(point.X - center.X, point.Z - center.Z);

    /// <summary>
    /// Get the length between an arbitrary point and a center point.
    /// </summary>
    /// <param name="point">Arbitrary point</param>
    /// <param name="center">Center point</param>
    /// <returns></returns>
    public static float GetLength(this Vector3 point, Vector3 center)
        => new Vector2(point.X - center.X, point.Z - center.Z).Length();

    /// <summary>
    /// Rotate an arbitrary point counter-clockwise around a center point and extend it.
    /// </summary>
    /// <param name="point">Arbitrary point</param>
    /// <param name="center">Center point</param>
    /// <param name="radian">Rotation radian</param>
    /// <param name="length">Extension length based on the point</param>
    /// <returns></returns>
    public static Vector3 RotateAndExtend(this Vector3 point, Vector3 center, float radian, float length)
    {
        var baseRad = point.GetRadian(center);
        var baseLength = point.GetLength(center);
        var rotRad = baseRad + radian;
        return new Vector3(
            center.X + MathF.Sin(rotRad) * (length + baseLength),
            center.Y,
            center.Z + MathF.Cos(rotRad) * (length + baseLength)
        );
    }

    /// <summary>
    /// Get the divided region number of a given angle.
    /// </summary>
    /// <param name="radian">Input radian</param>
    /// <param name="regionNum">Number of region divisions</param>
    /// <param name="baseRegionIdx">Initial index of the 0-degree region</param>>
    /// <param name="isDiagDiv">Is diagonal division, default false</param>
    /// <param name="isCw">Is increasing clockwise, default false</param>
    /// <returns></returns>
    public static int RadianToRegion(this float radian, int regionNum, int baseRegionIdx = 0, bool isDiagDiv = false, bool isCw = false)
    {
        var sepRad = float.Pi * 2 / regionNum;
        var inputAngle = radian * (isCw ? -1 : 1) + (isDiagDiv ? sepRad / 2 : 0);
        var rad = (inputAngle + 4 * float.Pi) % (2 * float.Pi);
        return ((int)Math.Floor(rad / sepRad) + baseRegionIdx + regionNum) % regionNum;
    }

    /// <summary>
    /// Fold the input point horizontally.
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerX">Center axis X coordinate</param>
    /// <returns></returns>
    public static Vector3 FoldPointHorizon(this Vector3 point, float centerX)
        => point with { X = 2 * centerX - point.X };

    /// <summary>
    /// Fold the input point vertically.
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerZ">Center axis Z coordinate</param>
    /// <returns></returns>
    public static Vector3 FoldPointVertical(this Vector3 point, float centerZ)
        => point with { Z = 2 * centerZ - point.Z };

    /// <summary>
    /// Center symmetry of the input point.
    /// </summary>
    /// <param name="point">Input point</param>
    /// <param name="center">Center point</param>
    /// <returns></returns>
    public static Vector3 PointCenterSymmetry(this Vector3 point, Vector3 center)
        => point.RotateAndExtend(center, float.Pi, 0);

    /// <summary>
    /// Get the specified digit of a number.
    /// </summary>
    /// <param name="val">Given integer</param>
    /// <param name="x">Corresponding digit, units digit is 1</param>
    /// <returns></returns>
    public static int GetDecimalDigit(this int val, int x)
    {
        var valStr = val.ToString();
        var length = valStr.Length;
        if (x < 1 || x > length) return -1;
        var digitChar = valStr[length - x]; // Take the x-th digit from the right
        return int.Parse(digitChar.ToString());
    }

    /// <summary>
    /// Calculate target position based on angle and distance
    /// </summary>
    public static Vector3 GetPositionByAngle(Vector3 origin, float angleInDegrees, float distance)
    {
        float radian = angleInDegrees * MathF.PI / 180f;

        return new Vector3(
            origin.X + distance * MathF.Cos(radian),
            origin.Y,
            origin.Z + distance * MathF.Sin(radian)
        );
    }
}

#endregion Calculation Functions

#region Index Helper Functions
public static class IndexHelper
{
    /// <summary>
    /// Input player dataId, get the corresponding position index
    /// </summary>
    /// <param name="pid">Player SourceId</param>
    /// <param name="sa"></param>
    /// <returns>The position index corresponding to the player</returns>
    public static int GetPlayerIdIndex(this ScriptAccessory sa, uint pid)
    {
        // Get player IDX
        return sa.Data.PartyList.IndexOf(pid);
    }

    /// <summary>
    /// Get the position index of the main perspective player
    /// </summary>
    /// <param name="sa"></param>
    /// <returns>Position index of the main perspective player</returns>
    public static int GetMyIndex(this ScriptAccessory sa)
    {
        return sa.Data.PartyList.IndexOf(sa.Data.Me);
    }

    /// <summary>
    /// Input player dataId, get the corresponding position name, output string only for text output
    /// </summary>
    /// <param name="pid">Player SourceId</param>
    /// <param name="sa"></param>
    /// <returns>The position name corresponding to the player</returns>
    public static string GetPlayerJobById(this ScriptAccessory sa, uint pid)
    {
        // Get player role abbreviation, only for DEBUG output
        var idx = sa.Data.PartyList.IndexOf(pid);
        var str = sa.GetPlayerJobByIndex(idx);
        return str;
    }

    /// <summary>
    /// Input position index, get the corresponding position name, output string only for text output
    /// </summary>
    /// <param name="idx">Position index</param>
    /// <param name="fourPeople">Is it a 4-man dungeon</param>
    /// <param name="sa"></param>
    /// <returns></returns>
    public static string GetPlayerJobByIndex(this ScriptAccessory sa, int idx, bool fourPeople = false)
    {
        List<string> role8 = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
        List<string> role4 = ["T", "H", "D1", "D2"];
        if (idx < 0 || idx >= 8 || (fourPeople && idx >= 4))
            return "Unknown";
        return fourPeople ? role4[idx] : role8[idx];
    }

    /// <summary>
    /// Convert List content to string.
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="myList"></param>
    /// <param name="isJob">If true, convert to role name before string conversion</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string BuildListStr<T>(this ScriptAccessory sa, List<T> myList, bool isJob = false)
    {
        return string.Join(", ", myList.Select(item =>
        {
            if (isJob && item != null && item is int i)
                return sa.GetPlayerJobByIndex(i);
            return item?.ToString() ?? "";
        }));
    }
}
#endregion Index Helper Functions

#region Drawing Functions

public static class DrawTools
{
    /// <summary>
    /// Return drawing properties
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Drawing base, can be UID or position</param>
    /// <param name="targetObj">Drawing target, can be UID or position</param>
    /// <param name="delay">Delay in ms before appearance</param>
    /// <param name="destroy">Disappears after `destroy` ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="radian">Radian range of the drawing shape</param>
    /// <param name="rotation">Rotation radian of the drawing shape, relative to owner's forward direction, increasing counter-clockwise</param>
    /// <param name="width">Width of the drawing shape</param>
    /// <param name="length">Length of the drawing shape</param>
    /// <param name="innerWidth">Inner width of the drawing shape</param>
    /// <param name="innerLength">Inner length of the drawing shape</param>
    /// <param name="drawModeEnum">Drawing mode</param>
    /// <param name="drawTypeEnum">Drawing type</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="byTime">Fill animation over time</param>
    /// <param name="byY">Animation based on distance change</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawOwnerBase(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name,
        float radian, float rotation, float width, float length, float innerWidth, float innerLength,
        DrawModeEnum drawModeEnum, DrawTypeEnum drawTypeEnum, bool isSafe = false,
        bool byTime = false, bool byY = false, bool draw = true)
    {
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.InnerScale = new Vector2(innerWidth, innerLength);
        dp.Radian = radian;
        dp.Rotation = rotation;
        dp.Color = isSafe ? sa.Data.DefaultSafeColor : sa.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        dp.ScaleMode |= byY ? ScaleMode.YByDistance : ScaleMode.None;

        switch (ownerObj)
        {
            case uint u:
                dp.Owner = u;
                break;
            case ulong ul:
                dp.Owner = ul;
                break;
            case Vector3 spos:
                dp.Position = spos;
                break;
            default:
                throw new ArgumentException($"ownerObj {ownerObj} 的目标类型 {ownerObj.GetType()} 输入错误");
        }

        switch (targetObj)
        {
            case 0:
            case 0u:
                break;
            case uint u:
                dp.TargetObject = u;
                break;
            case ulong ul:
                dp.TargetObject = ul;
                break;
            case Vector3 tpos:
                dp.TargetPosition = tpos;
                break;
            default:
                throw new ArgumentException($"targetObj {targetObj} 的目标类型 {targetObj.GetType()} 输入错误");
        }

        if (draw)
            sa.Method.SendDraw(drawModeEnum, drawTypeEnum, dp);
        return dp;
    }

    /// <summary>
    /// Return guidance drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Start point</param>
    /// <param name="targetObj">End point</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="rotation">Arrow rotation angle</param>
    /// <param name="width">Arrow width</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawGuidance(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name,
        float rotation = 0, float width = 1f, bool isSafe = true, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, 0, rotation, width,
            width, 0, 0, DrawModeEnum.Imgui, DrawTypeEnum.Displacement, isSafe, false, true, draw);

    public static DrawPropertiesEdit DrawGuidance(this ScriptAccessory sa,
        object targetObj, int delay, int destroy, string name, float rotation = 0, float width = 1f, bool isSafe = true,
        bool draw = true)
        => sa.DrawGuidance((ulong)sa.Data.Me, targetObj, delay, destroy, name, rotation, width, isSafe, draw);

    /// <summary>
    /// Return circle drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Center point</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="scale">Circle radius/diameter</param>
    /// <param name="byTime">Expand over time</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawCircle(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name,
        float scale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, 0, delay, destroy, name, 2 * float.Pi, 0, scale, scale,
            0, 0, DrawModeEnum.Default, DrawTypeEnum.Circle, isSafe, byTime, false, draw);

    /// <summary>
    /// Return donut drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Center point</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="outScale">Outer radius</param>
    /// <param name="innerScale">Inner radius</param>
    /// <param name="byTime">Expand over time</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawDonut(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name,
        float outScale, float innerScale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, 0, delay, destroy, name, 2 * float.Pi, 0, outScale, outScale, innerScale,
            innerScale, DrawModeEnum.Default, DrawTypeEnum.Donut, isSafe, byTime, false, draw);

    /// <summary>
    /// Return fan drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Center point</param>
    /// <param name="targetObj">Target</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="radian">Radian</param>
    /// <param name="rotation">Rotation angle</param>
    /// <param name="outScale">Outer radius</param>
    /// <param name="innerScale">Inner radius</param>
    /// <param name="byTime">Expand over time</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawFan(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name, float radian, float rotation,
        float outScale, float innerScale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, radian, rotation, outScale, outScale, innerScale,
            innerScale, DrawModeEnum.Default, DrawTypeEnum.Fan, isSafe, byTime, false, draw);

    public static DrawPropertiesEdit DrawFan(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name, float radian, float rotation,
        float outScale, float innerScale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawFan(ownerObj, 0, delay, destroy, name, radian, rotation, outScale, innerScale, isSafe, byTime, draw);

    /// <summary>
    /// Return rectangle drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Rectangle start point</param>
    /// <param name="targetObj">Target</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="rotation">Rotation angle</param>
    /// <param name="width">Rectangle width</param>
    /// <param name="length">Rectangle length</param>
    /// <param name="byTime">Expand over time</param>
    /// <param name="byY">Expand based on distance</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawRect(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name, float rotation,
        float width, float length, bool isSafe = false, bool byTime = false, bool byY = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, 0, rotation, width, length, 0, 0,
            DrawModeEnum.Default, DrawTypeEnum.Rect, isSafe, byTime, byY, draw);

    public static DrawPropertiesEdit DrawRect(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name, float rotation,
        float width, float length, bool isSafe = false, bool byTime = false, bool byY = false, bool draw = true)
        => sa.DrawRect(ownerObj, 0, delay, destroy, name, rotation, width, length, isSafe, byTime, byY, draw);

    /// <summary>
    /// Return look away (sight) drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="targetObj">Target</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawSightAvoid(this ScriptAccessory sa,
        object targetObj, int delay, int destroy, string name, bool isSafe = true, bool draw = true)
        => sa.DrawOwnerBase(sa.Data.Me, targetObj, delay, destroy, name, 0, 0, 0, 0, 0, 0,
            DrawModeEnum.Default, DrawTypeEnum.SightAvoid, isSafe, false, false, draw);

    /// <summary>
    /// Return knockback drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="targetObj">Knockback source</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="width">Arrow width</param>
    /// <param name="length">Arrow length</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawKnockBack(this ScriptAccessory sa,
        object targetObj, int delay, int destroy, string name, float width, float length,
        bool isSafe = false, bool draw = true)
        => sa.DrawOwnerBase(sa.Data.Me, targetObj, delay, destroy, name, 0, float.Pi, width, length, 0, 0,
            DrawModeEnum.Default, DrawTypeEnum.Displacement, isSafe, false, false, draw);

    /// <summary>
    /// Return line drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Line start point</param>
    /// <param name="targetObj">Line target point</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="rotation">Rotation angle</param>
    /// <param name="width">Line width</param>
    /// <param name="length">Line length</param>
    /// <param name="byTime">Expand over time</param>
    /// <param name="byY">Expand based on distance</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawLine(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name, float rotation,
        float width, float length, bool isSafe = false, bool byTime = false, bool byY = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, 1, rotation, width, length, 0, 0,
            DrawModeEnum.Default, DrawTypeEnum.Line, isSafe, byTime, byY, draw);

    /// <summary>
    /// Return connection line drawing between two objects
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Start source</param>
    /// <param name="targetObj">Target source</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="width">Line width</param>
    /// <param name="isSafe">Use safe color</param>
    /// <param name="draw">Draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawConnection(this ScriptAccessory sa, object ownerObj, object targetObj,
        int delay, int destroy, string name, float width = 1f, bool isSafe = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, 0, 0, width, width,
            0, 0, DrawModeEnum.Imgui, DrawTypeEnum.Line, isSafe, false, true, draw);

    /// <summary>
    /// Assign nearest/farthest target drawing relative to the owner to the given dp.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="isNearOrder">Order by near or far from owner</param>
    /// <param name="orderIdx">Starting from 1</param>
    /// <returns></returns>
    public static DrawPropertiesEdit SetOwnersDistanceOrder(this DrawPropertiesEdit self, bool isNearOrder,
        uint orderIdx)
    {
        self.CentreResolvePattern = isNearOrder
            ? PositionResolvePatternEnum.PlayerNearestOrder
            : PositionResolvePatternEnum.PlayerFarestOrder;
        self.CentreOrderIndex = orderIdx;
        return self;
    }

    /// <summary>
    /// Assign enmity order drawing relative to the owner to the given dp.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="orderIdx">Enmity order, starting from 1</param>
    /// <returns></returns>
    public static DrawPropertiesEdit SetOwnersEnmityOrder(this DrawPropertiesEdit self, uint orderIdx)
    {
        self.CentreResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        self.CentreOrderIndex = orderIdx;
        return self;
    }

    /// <summary>
    /// Assign nearest/farthest target drawing relative to a position to the given dp.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="isNearOrder">Order by near or far from owner</param>
    /// <param name="orderIdx">Starting from 1</param>
    /// <returns></returns>
    public static DrawPropertiesEdit SetPositionDistanceOrder(this DrawPropertiesEdit self, bool isNearOrder,
        uint orderIdx)
    {
        self.TargetResolvePattern = isNearOrder
            ? PositionResolvePatternEnum.PlayerNearestOrder
            : PositionResolvePatternEnum.PlayerFarestOrder;
        self.TargetOrderIndex = orderIdx;
        return self;
    }

    /// <summary>
    /// Assign owner's spell target as source for the drawing.
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit SetOwnersTarget(this DrawPropertiesEdit self)
    {
        self.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
        return self;
    }
}

#endregion Drawing Functions

#region Waymark Functions

public static class MarkerHelper
{
    public static void LocalMarkClear(this ScriptAccessory sa)
    {
        sa.Log.Debug($"Deleting local waymarks.");
        sa.Method.Mark(0xE000000, KodaMarkType.Attack1, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Attack2, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Attack3, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Attack4, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Attack5, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Attack6, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Attack7, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Attack8, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Bind1, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Bind2, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Bind3, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Stop1, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Stop2, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Square, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Circle, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Cross, true);
        sa.Method.Mark(0xE000000, KodaMarkType.Triangle, true);
    }

    public static void MarkClear(this ScriptAccessory sa,
        bool enable = true, bool local = false, bool localString = false)
    {
        if (!enable) return;
        sa.Log.Debug($"Command received: Deleting waymarks");

        if (local)
        {
            if (localString)
                sa.Log.Debug($"[Character Simulation] Deleting local waymarks.");
            else
                sa.LocalMarkClear();
        }
        else
            sa.Method.MarkClear();
    }

    public static void MarkPlayerByIdx(this ScriptAccessory sa, int idx, KodaMarkType marker,
        bool enable = true, bool local = false, bool localString = false)
    {
        if (!enable) return;
        if (localString)
            sa.Log.Debug($"[Local Character Simulation] Waymarking {idx}({sa.GetPlayerJobByIndex(idx)}) with {marker}.");
        else
            sa.Method.Mark(sa.Data.PartyList[idx], marker, local);
    }

    public static void MarkPlayerById(ScriptAccessory sa, uint id, KodaMarkType marker,
        bool enable = true, bool local = false, bool localString = false)
    {
        if (!enable) return;
        if (localString)
            sa.Log.Debug($"[Local Character Simulation] Waymarking {sa.GetPlayerIdIndex(id)}({sa.GetPlayerJobById(id)}) with {marker}.");
        else
            sa.Method.Mark(id, marker, local);
    }

    public static int GetMarkedPlayerIndex(this ScriptAccessory sa, List<KodaMarkType> markerList, KodaMarkType marker)
    {
        return markerList.IndexOf(marker);
    }
}

#endregion

#region Special Functions

public static class SpecialFunction
{
    public static void SetTargetable(this ScriptAccessory sa, IGameObject? obj, bool targetable)
    {
        if (obj == null || !obj.IsValid())
        {
            sa.Log.Error($"Provided IGameObject is invalid.");
            return;
        }
        unsafe
        {
            GameObject* charaStruct = (GameObject*)obj.Address;
            if (targetable)
            {
                if (obj.IsDead || obj.IsTargetable) return;
                charaStruct->TargetableStatus |= ObjectTargetableFlags.IsTargetable;
            }
            else
            {
                if (!obj.IsTargetable) return;
                charaStruct->TargetableStatus &= ~ObjectTargetableFlags.IsTargetable;
            }
        }
        sa.Log.Debug($"SetTargetable {targetable} => {obj.Name} {obj}");
    }

    public static unsafe void SetModelScale(ScriptAccessory sa, uint dataId, float scale, float VfxScale)
    {
        sa.Method.RunOnMainThreadAsync(() =>
        {
            var obj = sa.Data.Objects.FirstOrDefault(o => o.DataId == dataId);
            if (obj == null) return;

            var gameObj = (GameObject*)obj.Address;
            if (gameObj == null || !gameObj->IsReadyToDraw()) return;

            gameObj->Scale = scale;
            gameObj->VfxScale = VfxScale;

            if (gameObj->IsCharacter())
            {
                var chara = (BattleChara*)gameObj;
                chara->Character.CharacterData.ModelScale = scale;
            }

            gameObj->DisableDraw();
            gameObj->EnableDraw();
        });
    }

    public static void SetRotation(this ScriptAccessory sa, IGameObject? obj, float radian, bool show = false)
    {
        if (obj == null || !obj.IsValid())
        {
            sa.Log.Error($"Provided IGameObject is invalid.");
            return;
        }
        unsafe
        {
            GameObject* charaStruct = (GameObject*)obj.Address;
            charaStruct->SetRotation(radian);
        }
        sa.Log.Debug($"Changed facing of {obj.Name.TextValue} | {obj.EntityId} => {radian.RadToDeg()}");

        if (!show) return;
        var ownerObj = sa.GetById(obj.EntityId);
        if (ownerObj == null) return;
        var dp = sa.DrawGuidance(ownerObj, 0, 0, 2000, $"Changed facing of {obj.Name.TextValue}", radian, draw: false);
        dp.FixRotation = true;
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);

    }

    public static void SetPosition(this ScriptAccessory sa, IGameObject? obj, Vector3 position, bool show = false)
    {
        if (obj == null || !obj.IsValid())
        {
            sa.Log.Error($"Provided IGameObject is invalid.");
            return;
        }
        unsafe
        {
            GameObject* charaStruct = (GameObject*)obj.Address;
            charaStruct->SetPosition(position.X, position.Y, position.Z);
        }
        sa.Log.Debug($"Changed position => {obj.Name.TextValue} | {obj.EntityId} => {position}");

        if (!show) return;
        var dp = sa.DrawCircle(position, 0, 2000, $"Teleport point {obj.Name.TextValue}", 0.5f, true, draw: false);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

    }

    [Flags]
    public enum DrawState : uint
    {
        Invisibility = 0x00_00_00_02,
        IsLoading = 0x00_00_08_00,
        SomeNpcFlag = 0x00_00_01_00,
        MaybeCulled = 0x00_00_04_00,
        MaybeHiddenMinion = 0x00_00_80_00,
        MaybeHiddenSummon = 0x00_80_00_00,
    }

    public static unsafe DrawState* ActorDrawState(IGameObject actor)
        => (DrawState*)(&((GameObject*)actor.Address)->RenderFlags);

    /// <summary>
    /// Check object visibility (Read)
    /// </summary>
    /// <param name="sa">ScriptAccessory</param>
    /// <param name="obj">Obj need check</param>
    /// <param name="checkVisible">true=check visible, false=check invisible</param>
    /// <returns>Returns True if the check condition is met</returns>
    public static unsafe bool IsActorVisible(this ScriptAccessory sa, IGameObject? obj, bool checkVisible = true)
    {
        if (obj == null) return false;

        try
        {
            var state = *ActorDrawState(obj);
            bool isVisible = (state & DrawState.Invisibility) == 0;

            return checkVisible ? isVisible : !isVisible;
        }
        catch (Exception e)
        {
            sa.Log.Error($" {e} ");
            return false;
        }
    }

    /// <summary>
    /// Set object visibility (Write)
    /// </summary>
    public static unsafe void WriteVisible(this ScriptAccessory sa, IGameObject? actor, bool visible)
    {
        if (actor == null) return;

        try
        {
            var statePtr = ActorDrawState(actor);
            if (visible)
                *statePtr &= ~DrawState.Invisibility;
            else
                *statePtr |= DrawState.Invisibility;
        }
        catch (Exception e)
        {
            sa.Log.Error($" {e} ");
            throw;
        }
    }

    /// <summary>
    /// Check DrawState
    /// </summary>
    public static unsafe bool HasDrawState(this ScriptAccessory sa, IGameObject? actor, DrawState state)
    {
        if (actor == null) return false;

        try
        {
            return (*ActorDrawState(actor) & state) != 0;
        }
        catch (Exception e)
        {
            sa.Log.Error($" {e} ");
            return false;
        }
    }
}

#endregion Special Functions


public static class NamazuHelper
{
    public class NamazuCommand(ScriptAccessory accessory, string url, string command, string param)
    {
        private ScriptAccessory accessory { get; set; } = accessory;
        private string _url = url;

        public void PostCommand()
        {
            var url = $"{_url}/{command}";
            accessory.Log.Debug($"Sending {param} to {url}");
            accessory.Method.HttpPost(url, param);
        }
    }

    public class Waymark
    {
        public ScriptAccessory accessory { get; set; }
        private Dictionary<string, object> _jsonObj = new();
        private string? _jsonPayload;

        public Waymark(ScriptAccessory _accessory)
        {
            accessory = _accessory;
        }

        public void AddWaymarkType(string type, Vector3 pos, bool active = true)
        {
            string[] validTypes = ["A", "B", "C", "D", "One", "Two", "Three", "Four"];
            var waymarkType = type;
            if (!validTypes.Contains(type)) return;
            _jsonObj[waymarkType] = new Dictionary<string, object>
            {
                { "X", pos.X },
                { "Y", pos.Y },
                { "Z", pos.Z },
                { "Active", active }
            };
        }

        public void SetJsonPayload(bool local = true, bool log = true)
        {
            _jsonObj["LocalOnly"] = local;
            _jsonObj["Log"] = log;
            _jsonPayload = JsonConvert.SerializeObject(_jsonObj);
        }

        public string? GetJsonPayload()
        {
            if (_jsonPayload == null)
                SetJsonPayload();
            return _jsonPayload;
        }

        public void PostWaymarkCommand(int port)
        {
            var param = GetJsonPayload();
            if (param == null) return;
            var post = new NamazuCommand(accessory, $"http://127.0.0.1:{port}", "place", param);
            post.PostCommand();
        }

        public void ClearWaymarks(int port)
        {
            var post = new NamazuCommand(accessory, $"http://127.0.0.1:{port}", "place", "clear");
            post.PostCommand();
        }
    }
}

public static class DrawHelper
{
    public static void DrawBeam(ScriptAccessory accessory, Vector3 sourcePosition, Vector3 targetPosition, string name = "Light's Course", int duration = 6700, Vector4? color = null, int delay = 0)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = sourcePosition;
        dp.TargetPosition = targetPosition;
        dp.Scale = new Vector2(10, 50);
        dp.Delay = delay;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    public static void DrawCircle(ScriptAccessory accessory, Vector3 position, Vector2 scale, int duration, string name, Vector4? color = null, bool scaleByTime = true, int delay = 0, DrawModeEnum drawmode = DrawModeEnum.Default, Vector3? offset = null)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = position;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.Offset = offset ?? new Vector3(0, 0, 0);
        if (scaleByTime) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(drawmode, DrawTypeEnum.Circle, dp);
    }

    public static void DrawDisplacement(ScriptAccessory accessory, Vector3 targetPos, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0, DrawModeEnum drawmode = DrawModeEnum.Imgui)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Owner = accessory.Data.Me;
        dp.Color = color ?? accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = targetPos;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(drawmode, DrawTypeEnum.Displacement, dp);
    }

    public static void DrawDisplacementby2points(ScriptAccessory accessory, Vector3 origin, Vector3 target, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Position = origin;
        dp.Color = color ?? accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetPosition = target;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    public static void DrawDisplacementObject(ScriptAccessory accessory, ulong target, Vector2 scale, int duration, string name, float? rotation = null, Vector4? color = null, int delay = 0, bool fix = false, DrawModeEnum drawmode = DrawModeEnum.Imgui)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Owner = accessory.Data.Me;
        dp.Color = color ?? accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = target;
        dp.Scale = scale;
        if (rotation.HasValue) dp.Rotation = rotation.Value;
        dp.Delay = delay;
        dp.FixRotation = fix;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(drawmode, DrawTypeEnum.Displacement, dp);
    }

    public static void DrawRect(ScriptAccessory accessory, Vector3 position, Vector3 targetPos, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = position;
        dp.TargetPosition = targetPos;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    public static void DrawRectObjectNoTarget(ScriptAccessory accessory, ulong owner, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0, ScaleMode scalemode = ScaleMode.None, Vector3? offset = null)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Owner = owner;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.ScaleMode = scalemode;
        dp.Offset = offset ?? new Vector3(0, 0, 0);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    public static void DrawRectObjectNoTargetWithRot(ScriptAccessory accessory, ulong owner, Vector2 scale, float rotation, int duration, string name, Vector4? color = null, int delay = 0, ScaleMode scalemode = ScaleMode.None, Vector3? offset = null)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Owner = owner;
        dp.Scale = scale;
        dp.Rotation = rotation;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.ScaleMode = scalemode;
        dp.Offset = offset ?? new Vector3(0, 0, 0);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    public static void DrawRectPosNoTarget(ScriptAccessory accessory, Vector3 pos, Vector2 scale, float rotation, int duration, string name, Vector4? color = null, int delay = 0, ScaleMode scalemode = ScaleMode.None, Vector3? offset = null, DrawModeEnum drawMode = DrawModeEnum.Default)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = pos;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.Rotation = rotation;
        dp.DestoryAt = duration;
        dp.ScaleMode = scalemode;
        dp.Offset = offset ?? new Vector3(0, 0, 0);
        accessory.Method.SendDraw(drawMode, DrawTypeEnum.Rect, dp);
    }

    public static void DrawRectPosTarget(ScriptAccessory accessory, Vector3 pos, Vector3 targetpos, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0, ScaleMode scalemode = ScaleMode.None, Vector3? offset = null, DrawModeEnum drawMode = DrawModeEnum.Default)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = pos;
        dp.TargetPosition = targetpos;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.ScaleMode = scalemode;
        dp.Offset = offset ?? new Vector3(0, 0, 0);
        accessory.Method.SendDraw(drawMode, DrawTypeEnum.Rect, dp);
    }

    public static void DrawRectObjectTarget(ScriptAccessory accessory, ulong owner, ulong target, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0, ScaleMode scalemode = ScaleMode.None)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Owner = owner;
        dp.TargetObject = target;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.ScaleMode = scalemode;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    public static void DrawFan(ScriptAccessory accessory, Vector3 position, float rotation, Vector2 scale, float angle,
                                int duration, string name, Vector4? color = null, int delay = 0,
                                bool fix = false, Vector3? offset = null, DrawModeEnum drawmode = DrawModeEnum.Default, bool scaleByTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = position;
        dp.Rotation = rotation;
        dp.Scale = scale;
        dp.Radian = angle * (float.Pi / 180);
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.FixRotation = fix;
        dp.Offset = offset ?? new Vector3(0, 0, 0);
        if (scaleByTime) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(drawmode, DrawTypeEnum.Fan, dp);
    }

    public static void DrawFanNoRot(ScriptAccessory accessory, Vector3 position, Vector2 scale, float angle, int duration, string name, Vector4? color = null, int delay = 0, bool fix = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = position;
        dp.Scale = scale;
        dp.Radian = angle * (float.Pi / 180);
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.FixRotation = fix;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    public static void DrawFanObjectNoRot(ScriptAccessory accessory, ulong owner, Vector2 scale, float angle, int duration, string name, Vector4? color = null, int delay = 0, bool fix = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Owner = owner;
        dp.Scale = scale;
        dp.Radian = angle * (float.Pi / 180);
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.FixRotation = fix;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    public static void DrawFanObject(ScriptAccessory accessory, ulong owner, float rotation,
        Vector2 scale, float angle, int duration, string name, Vector4? color = null,
        int delay = 0, bool scaleByTime = true, bool fix = false, Vector3? offset = null, DrawModeEnum drawmode = DrawModeEnum.Default)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Owner = owner;
        dp.Rotation = rotation;
        dp.Scale = scale;
        dp.Radian = angle * (float.Pi / 180);
        dp.Delay = delay;
        dp.DestoryAt = duration;
        dp.FixRotation = fix;
        dp.Offset = offset ?? new Vector3(0, 0, 0);
        if (scaleByTime) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(drawmode, DrawTypeEnum.Fan, dp);
    }

    public static void DrawLine(ScriptAccessory accessory, Vector3 startPosition, Vector3 endPosition, float width, int duration, string name, Vector4? color = null, int delay = 0)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = startPosition;
        dp.TargetPosition = endPosition;
        dp.Scale = new Vector2(width, 1);
        dp.Delay = delay;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Line, dp);
    }

    public static void DrawArrow(ScriptAccessory accessory, Vector3 startPosition, Vector3 endPosition, float x, float y, int duration, string name, Vector4? color = null, int delay = 0)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = startPosition;
        dp.TargetPosition = endPosition;
        dp.Scale = new Vector2(x, y);
        dp.Delay = delay;
        dp.DestoryAt = duration;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Arrow, dp);
    }

    public static void DrawCircleObject(ScriptAccessory accessory, ulong? ob, Vector2 scale, int duration, string name, Vector4? color = null, bool scaleByTime = true, int delay = 0)
    {
        if (ob == null) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Owner = ob.Value;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        if (scaleByTime) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    public static void DrawDonut(ScriptAccessory accessory, Vector3 position, Vector2 scale, Vector2 innerscale, int duration, string name, Vector4? color = null, bool scaleByTime = true, int delay = 0, DrawModeEnum drawmode = DrawModeEnum.Default)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = position;
        dp.Radian = 2 * float.Pi;
        dp.Scale = scale;
        dp.InnerScale = innerscale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        if (scaleByTime) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(drawmode, DrawTypeEnum.Donut, dp);
    }

    public static void DrawDonutObject(ScriptAccessory accessory, ulong? ob, Vector2 scale, Vector2 innerscale, int duration, string name, Vector4? color = null, bool scaleByTime = true, int delay = 0, DrawModeEnum drawmode = DrawModeEnum.Default)
    {
        if (ob == null) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Owner = ob.Value;
        dp.Radian = 2 * float.Pi;
        dp.Scale = scale;
        dp.InnerScale = innerscale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        if (scaleByTime) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(drawmode, DrawTypeEnum.Donut, dp);
    }
}


#endregion Function Collections

#region Extension Methods
public static class ExtensionMethods
{
    public static float Round(this float value, float precision) => MathF.Round(value / precision) * precision;

    public static Vector3 ToDirection(this float rotation)
    {
        return new Vector3(
            MathF.Sin(rotation),
            0f,
            MathF.Cos(rotation)
        );
    }

    public static Vector3 Quantized(this Vector3 position, float gridSize = 1f)
    {
        return new Vector3(
            MathF.Round(position.X / gridSize) * gridSize,
            MathF.Round(position.Y / gridSize) * gridSize,
            MathF.Round(position.Z / gridSize) * gridSize
        );
    }

    /// <summary>
    /// Get the player's job name or abbreviation
    /// </summary>
    public static string GetPlayerJob(
        this ScriptAccessory sa,
        IPlayerCharacter? playerObject,
        bool fullName = false
    )
    {
        if (playerObject == null) return "None";
        return fullName
            ? playerObject.ClassJob.Value.Name.ToString()
            : playerObject.ClassJob.Value.Abbreviation.ToString();
    }

    /// <summary>
    /// Check if the player is a tank
    /// </summary>
    public static bool IsTank(IPlayerCharacter? playerObject)
    {
        if (playerObject == null) return false;
        return playerObject.ClassJob.Value.Role == 1;
    }
}

public unsafe static class ExtensionVisibleMethod
{
    public static bool IsCharacterVisible(this ICharacter chr)
    {
        var v = (IntPtr)(((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)chr.Address)->GameObject.DrawObject);
        if (v == IntPtr.Zero) return false;
        return Bitmask.IsBitSet(*(byte*)(v + 136), 0);
    }

    public static class Bitmask
    {
        public static bool IsBitSet(ulong b, int pos)
        {
            return (b & (1UL << pos)) != 0;
        }

        public static void SetBit(ref ulong b, int pos)
        {
            b |= 1UL << pos;
        }

        public static void ResetBit(ref ulong b, int pos)
        {
            b &= ~(1UL << pos);
        }

        public static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static bool IsBitSet(short b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }
}
#endregion

#region Script Version Checker
public static class ScriptVersionChecker
{
    private const string OnlineRepoUrl = "https://raw.githubusercontent.com/VeeverSW/Kodakku-Script/refs/heads/main/OnlineRepo.json";
    private static readonly HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// Online repository script information
    /// </summary>
    public class OnlineScriptInfo
    {
        public string Name { get; set; } = "";
        public string Guid { get; set; } = "";
        public string Version { get; set; } = "";
        public string Author { get; set; } = "";
        public string Repo { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string Note { get; set; } = "";
        public string UpdateInfo { get; set; } = "";
        public int[] TerritoryIds { get; set; } = Array.Empty<int>();
    }

    /// <summary>
    /// Version comparison result
    /// </summary>
    public enum VersionCompareResult
    {
        /// <summary>Current version is up-to-date or newer</summary>
        UpToDate,
        /// <summary>A new version is available</summary>
        UpdateAvailable,
        /// <summary>No matching script found</summary>
        NotFound,
        /// <summary>Check failed</summary>
        Error
    }

    /// <summary>
    /// Check script version
    /// </summary>
    /// <param name="sa">ScriptAccessory</param>
    /// <param name="guid">Script GUID</param>
    /// <param name="currentVersion">Current version number</param>
    /// <param name="showNotification">Whether to show notification</param>
    /// <returns>Version comparison result</returns>
    public static async Task<(VersionCompareResult result, OnlineScriptInfo? onlineInfo)> CheckVersionAsync(
        ScriptAccessory sa,
        string guid,
        string currentVersion,
        bool showNotification = true)
    {
        try
        {
            sa.Log.Debug($"Checking script version (GUID: {guid}, Current version: {currentVersion})");

            var response = await _httpClient.GetStringAsync(OnlineRepoUrl);
            var onlineScripts = JsonConvert.DeserializeObject<List<OnlineScriptInfo>>(response);

            if (onlineScripts == null || onlineScripts.Count == 0)
            {
                sa.Log.Error("Unable to parse online repository data");
                return (VersionCompareResult.Error, null);
            }

            var onlineScript = onlineScripts.FirstOrDefault(s =>
                s.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase));

            if (onlineScript == null)
            {
                sa.Log.Debug($"No script found in online repository with GUID {guid}");
                if (showNotification)
                {
                    sa.Method.TextInfo("This script is not registered in the online repository", 3000);
                }
                return (VersionCompareResult.NotFound, null);
            }

            sa.Log.Debug($"Found online script: {onlineScript.Name}, Online version: {onlineScript.Version}");

            var compareResult = CompareVersions(currentVersion, onlineScript.Version);

            if (compareResult < 0)
            {
                sa.Log.Debug($"New version available: {onlineScript.Version} (Current: {currentVersion})");
                if (showNotification)
                {
                    sa.Method.TextInfo(
                        $"New version {onlineScript.Version} available\nCurrent version: {currentVersion}",
                        5000,
                        true);
                }
                return (VersionCompareResult.UpdateAvailable, onlineScript);
            }
            else
            {
                sa.Log.Debug($"Current version is up-to-date (Current: {currentVersion}, Online: {onlineScript.Version})");

                return (VersionCompareResult.UpToDate, onlineScript);
            }
        }
        catch (HttpRequestException ex)
        {
            sa.Log.Error($"Network request failed: {ex.Message}");
            if (showNotification)
            {
                sa.Method.TextInfo("Version check failed: Network error", 3000, true);
            }
            return (VersionCompareResult.Error, null);
        }
        catch (Exception ex)
        {
            sa.Log.Error($"Version check failed: {ex.Message}");
            if (showNotification)
            {
                sa.Method.TextInfo("Version check failed", 3000, true);
            }
            return (VersionCompareResult.Error, null);
        }
    }

    /// <summary>
    /// Compare version numbers
    /// </summary>
    /// <param name="version1">Version 1 (e.g., "0.0.0.3")</param>
    /// <param name="version2">Version 2 (e.g., "0.0.0.5")</param>
    /// <returns>Negative: version1 < version2, 0: equal, Positive: version1 > version2</returns>
    private static int CompareVersions(string version1, string version2)
    {
        var v1Parts = version1.Split('.').Select(p => int.TryParse(p, out var num) ? num : 0).ToArray();
        var v2Parts = version2.Split('.').Select(p => int.TryParse(p, out var num) ? num : 0).ToArray();

        int maxLength = Math.Max(v1Parts.Length, v2Parts.Length);

        for (int i = 0; i < maxLength; i++)
        {
            int v1Part = i < v1Parts.Length ? v1Parts[i] : 0;
            int v2Part = i < v2Parts.Length ? v2Parts[i] : 0;

            if (v1Part < v2Part) return -1;
            if (v1Part > v2Part) return 1;
        }

        return 0;
    }
}
#endregion
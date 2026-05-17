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
using System.Windows.Forms;
using System.Xml.Linq;

namespace Veever.Shadowbringers.Malikah_s_Well;

[ScriptType(name: Name, territorys: [836], guid: "41547ef2-b54c-4296-9ca4-8e54992ea6fb",
    version: Version, author: "Veever", note: NoteStr, updateInfo: UpdateInfo)]

// ^(?!.*((Monk|Machinist|Dragoon|Samurai|Ninja|Viper|Reaper|Dancer|Bard|Astrologian|Sage|Scholar|(Eos|Selene)|Seraph|White Mage|Warrior|Paladin|Dark Knight|Gunbreaker|Pictomancer|Black Mage|Blue Mage|Summoner|Carbuncle|Demigod Bahamut|Demigod Phoenix|Garuda-Egi|Titan-Egi|Ifrit-Egi|Automaton Queen)\] (Used|Cast))).*35501.*$
// ^\[\w+\|[^|]+\|E\]\s\w+

public class Malikah_s_Well
{
    const string NoteStr =
    """
    v0.0.0.1
    1. If you need a draw for a mechanic or notice any issues, @ me on DC or DM me.
    Duckmen
    ------------------------------
    1. Si necesita un dibujo para una mecánica o nota algún problema, @ me en DC o envíame un MD.
    Duckmen
    """;

    const string UpdateInfo =
    """
        v0.0.0.1
    """;

    private const string Name = "LV.77 Malikah's Well";
    private const string Version = "0.0.0.1";
    private const string DebugVersion = "a";

    private const bool Debugging = true;

    private static readonly List<string> Role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];

    [UserSetting("Announce Language")]
    public Language language { get; set; } = Language.Chinese;

    [UserSetting("Draw Opacity — higher value = more visible")]
    public static float ColorAlpha { get; set; } = 1f;

    [UserSetting("Banner Text Toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = true;

    //[UserSetting("Auto Anti-knockback")]
    //public bool useAntiKnockback { get; set; } = true;

    [UserSetting("Guide Arrow Toggle")]
    public bool isLead { get; set; } = true;

    //[UserSetting("Target Marker Toggle")]
    //public bool isMark { get; set; } = true;

    //[UserSetting("Local target marker toggle (ON = local only, OFF = party shared)")]
    //public bool LocalMark { get; set; } = true;

    //[UserSetting("Waymark guide toggle")]
    //public bool PostNamazuPrint { get; set; } = true;

    //[UserSetting("PostNamazu Port Setting")]
    //public int PostNamazuPort { get; set; } = 2019;

    //[UserSetting("Waymarks: local toggle (OFF = party shared, OOC only)")]
    //public bool PostNamazuisLocal { get; set; } = true;

    [UserSetting("Debug on/off (don't touch unless you know what you're doing)")]
    public bool isDebug { get; set; } = false;

    public enum Language
    {
        Chinese,
        English
    }

    public int HereticsForkcount = 0;
    public int BreakingWheelCount = 0;

    private readonly object CountLock = new object();

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    public void Init(ScriptAccessory sa)
    {
        if (isDebug) sa.Log.Debug($"Script {Name} v{Version}{DebugVersion} initialized.");
        sa.Method.RemoveDraw(".*");
        HereticsForkcount = 0;
        BreakingWheelCount = 0;
    }


    #region Trash Mobs
    [ScriptMethod(name: "---- Trash Mobs ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void mobs(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "Head Strike & Interject Cancel Destroy", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^75(38|51)$"], userControl: false)]
    public void destoryCancelAction(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($".*{ev.TargetId}");
    }

    #region Before Boss 1
    [ScriptMethod(name: "Earthshatter", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:16261"])]
    public void Earthshatter(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(8f, 12f), 2700, $"Earthshatter-{ev.SourceId}", sa.Data.DefaultDangerColor, scalemode: ScaleMode.ByTime);
    }

    [ScriptMethod(name: "Earthshatter Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:16261"], userControl: false)]
    public void EarthshatterClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Earthshatter-{ev.SourceId}");
    }
    #endregion

    #region Before Boss 2
    [ScriptMethod(name: "Self-destruct", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:16263"])]
    public void SelfDestruct(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(6.9f), 2700, $"SelfDestruct-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Self-destruct Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:16263"], userControl: false)]
    public void SelfDestructClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"SelfDestruct-{ev.SourceId}");
    }

    [ScriptMethod(name: "Pebble", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:16262"])]
    public void Pebble(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(3f), 2200, $"Pebble-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Pebble Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:16262"], userControl: false)]
    public void PebbleClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Pebble-{ev.SourceId}");
    }

    #endregion

    #region Before Boss 3
    [ScriptMethod(name: "Realm Shaker", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:16266"])]
    public void RealmShaker(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2700, $"Realm Shaker-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Realm Shaker Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:16266"], userControl: false)]
    public void RealmShakerClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Realm Shaker-{ev.SourceId}");
    }

    [ScriptMethod(name: "Acclaim", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:16265"])]
    public void Acclaim(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(12f), 120, 2700, $"Acclaim-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Acclaim Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:16265"], userControl: false)]
    public void AcclaimClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Acclaim-{ev.SourceId}");
    }




    #endregion

    #endregion


    #region Boss 1
    private static IGameObject? GetBossObject(ScriptAccessory sa, uint BossDataId)
    {
        return sa.GetByDataId(BossDataId).FirstOrDefault();
    }

    [ScriptMethod(name: "---- Boss 1 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void Boss1(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "Tankbuster Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15589"])]
    public void boss1Tankbuster(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "死刑点名, 注意减伤" : "Tankbuster on YOU, use mitigation";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4200, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
    }

    [ScriptMethod(name: "Falling Rock", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15594"])]
    public void FallingRock(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(4f), 2700, $"FallingRock-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Head Toss", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15590"])]
    public void HeadToss(Event ev, ScriptAccessory sa)
    {
        string tname = ev["TargetName"]?.ToString() ?? "Unknown Target";

        string msg = language == Language.Chinese ? $"与{tname}分摊" : $"Stack with {tname}";

        if (ev.TargetId == sa.Data.Me)
        {
            msg = language == Language.Chinese ? "分摊点名" : "Stack marker";
        }

        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(5f), 4700, $"HeadToss-{ev.SourceId}", color: sa.Data.DefaultSafeColor, scaleByTime: false);
    }


    [ScriptMethod(name: "Right Round", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15591"])]
    public void RightRound(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? $"钢铁远离" : $"Chariot (Out)";

        if (isText) sa.Method.TextInfo($"{msg}", duration: 7200, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(8.5f), 2200, $"RightRound-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha), scaleByTime: true);
    }

    [ScriptMethod(name: "Flail Smash", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15593"])]
    public void FlailSmash(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(11f), 2200, $"RightRound-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha), scaleByTime: true);
    }
    #endregion

    #region Boss 2
    [ScriptMethod(name: "---- Boss 2 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void Boss2(Event ev, ScriptAccessory sa)
    {
    }


    //[ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44331"])]
    //public void boss2AOE(Event ev, ScriptAccessory sa)
    //{
    //    string msg = language == Language.Chinese ? "AOE" : "AOE";
    //    if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
    //    if (isTTS) sa.Method.EdgeTTS($"{msg}");
    //}

    [ScriptMethod(name: "Tankbuster Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15595"])]
    public void boss2Tankbuster(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "死刑点名, 注意减伤" : "Tankbuster on YOU, use mitigation";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4200, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
    }

    [ScriptMethod(name: "Wellbore", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15597"])]
    public void Wellbore(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(15f), 6700, $"Wellbore-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha), scaleByTime: true);
    }

    [ScriptMethod(name: "Geyser Eruption", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:15598"])]
    public void GeyserEruption(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(8f), 3200, $"GeyserEruption-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha), scaleByTime: true);
    }

    [ScriptMethod(name: "Geysers Draw", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2009801", "Operate:Add"])]
    public void GeysersDraw(Event ev, ScriptAccessory sa)
    {
        if (isDebug) sa.Log.Debug("In GeysersDraw");
        DrawHelper.DrawCircle(sa, ev.SourcePosition, new Vector2(4f), 120000, $"GeysersDraw-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha), scaleByTime: false, drawmode: DrawModeEnum.Imgui);
    }
     
    [ScriptMethod(name: "Geysers Clear", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2009801", "Operate:Remove"])]
    public void GeysersClear(Event ev, ScriptAccessory sa)
    {
        if (isDebug) sa.Log.Debug("In GeysersClear");
        sa.Method.RemoveDraw($"GeysersDraw-{ev.SourceId}");
    }

    [ScriptMethod(name: "High Pressure", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(15596)$"])]
    public async void HighPressure(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "远离钻井击退" : "Move away from Danger Zone for knockback";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 2000, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
         
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = "HighPressure";
        dp.Color = sa.Data.DefaultSafeColor;
        dp.Owner = sa.Data.Me;
        dp.TargetPosition = ev.SourcePosition;
        dp.Rotation = float.Pi;
        dp.Scale = new Vector2(2, 20);
        dp.DestoryAt = 3700;
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        await Task.Delay(4200);
        string msg1 = language == Language.Chinese ? "前往Boss身后" : "Move behind boss";
        if (isText) sa.Method.TextInfo($"{msg1}", duration: 3700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg1}");
    }

    [ScriptMethod(name: "Swift Spill", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(009C|009D)$"])]
    public void SwiftSpill(Event ev, ScriptAccessory sa)
    {
        if (isDebug) sa.Log.Debug("In SwiftSpill");

        var degree = 60f;
        if (isDebug) sa.Log.Debug($"{ev["Id"]}");
        if (ev["Id"] == "009C")
        {
            if (isDebug) sa.Log.Debug("Is 0x009C");
            degree = -60f;
        }

        var index = 0f;
        var boss = IbcHelper.GetById(sa, ev.SourceId);
        if (boss == null) return;
        if (isDebug) sa.Log.Debug($"bossRot:{boss.Rotation}");

        for (var i = 0; i < 6; i++)
        {
            if (i == 0)
            {
                DrawHelper.DrawFan(sa, boss.Position, boss.Rotation, new Vector2(22f), 60, 6700, $"SwiftSpill-{ev.SourceId}-i:{i}-deg:{degree}", sa.Data.DefaultDangerColor);
            }
            else
            {
                DrawHelper.DrawFan(sa, boss.Position, boss.Rotation + MathTools.DegToRad(index + i * degree), new Vector2(22f), 60, 3800, $"SwiftSpill-{ev.SourceId}-i:{i}-deg:{index + i * degree}", sa.Data.DefaultDangerColor, delay: 3700 + i * 1000);
            }
             
        }
    }


    #endregion

    #region Boss 3
    [ScriptMethod(name: "---- Boss 3 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void Boss3(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:(15601)"])]
    public void boss3AOE(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";

        if (isText) sa.Method.TextInfo($"{msg}", duration: 3500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
        HereticsForkcount = 0;
        BreakingWheelCount = 0;
    }

    [ScriptMethod(name: "Heretic's Fork", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:(15602|15609|15886)"])]
    public void HereticsFork(Event ev, ScriptAccessory sa)
    {
        lock (CountLock)
        {
            switch (ev.ActionId)
            {
                case 15602:
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(10, 40f), 4700, $"HereticsFork-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(0, 0, 20));
                    DrawHelper.DrawRectObjectNoTargetWithRot(sa, ev.SourceId, new Vector2(10, 40f), float.Pi / 2, 4700, $"HereticsFork-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(20, 0, 0));
                    break;
                case 15609:
                    if (HereticsForkcount == 0)
                    {
                        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(10f, 60f), 7700, $"HereticsFork-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(0, 0, 30));
                        DrawHelper.DrawRectObjectNoTargetWithRot(sa, ev.SourceId, new Vector2(10f, 60f), float.Pi / 2, 7700, $"HereticsFork-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(30, 0, 0));
                        HereticsForkcount++;
                        break;
                    } else
                    {
                        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(10f, 60f), 3700, $"HereticsFork-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(0, 0, 30), delay: 4000);
                        DrawHelper.DrawRectObjectNoTargetWithRot(sa, ev.SourceId, new Vector2(10f, 60f), float.Pi / 2, 3700, $"HereticsFork-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(30, 0, 0), delay: 4000);
                        HereticsForkcount++;
                        break;
                    }
                case 15886:
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(10, 40f), 4700, $"HereticsFork-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(0, 0, 20), delay: 20000);
                    DrawHelper.DrawRectObjectNoTargetWithRot(sa, ev.SourceId, new Vector2(10, 40f), float.Pi / 2, 4700, $"HereticsFork-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(20, 0, 0), delay: 20000);
                    break;
            }
        }
    }

    [ScriptMethod(name: "Breaking Wheel", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:(15605|15610|15887)"])]
    public void BreakingWheel(Event ev, ScriptAccessory sa)
    {
        lock (CountLock)
        {
            switch (ev.ActionId)
            {
                case 15605:
                    DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(5f), 4700, $"BreakingWheel-{ev.SourceId}", sa.Data.DefaultSafeColor, scaleByTime: false);
                    if (isLead) DrawHelper.DrawDisplacement(sa, ev.SourcePosition, new Vector2(1f), 4700, $"BreakingWheelNavi-{ev.SourceId}", sa.Data.DefaultSafeColor);
                    break;
                case 15610:
                    if (BreakingWheelCount == 0)
                    {
                        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(5f), 8700, $"BreakingWheel-{ev.SourceId}", sa.Data.DefaultSafeColor, scaleByTime: false);
                        if (isLead) DrawHelper.DrawDisplacement(sa, ev.SourcePosition, new Vector2(1f), 8700, $"BreakingWheelNavi-{ev.SourceId}", sa.Data.DefaultSafeColor);
                        BreakingWheelCount++;
                        break;
                    } else
                    {
                        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(5f), 4000, $"BreakingWheel-{ev.SourceId}", sa.Data.DefaultSafeColor, scaleByTime: false, delay: 4700);
                        if (isLead) DrawHelper.DrawDisplacement(sa, ev.SourcePosition, new Vector2(1f), 4000, $"BreakingWheelNavi-{ev.SourceId}", sa.Data.DefaultSafeColor, delay: 4700);
                        BreakingWheelCount++;
                        break;
                    }

                case 15887:
                    DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(5f), 4000, $"BreakingWheel-{ev.SourceId}", sa.Data.DefaultSafeColor, scaleByTime: false, delay: 24700);
                    if (isLead) DrawHelper.DrawDisplacement(sa, ev.SourcePosition, new Vector2(1f), 4000, $"BreakingWheelNavi-{ev.SourceId}", sa.Data.DefaultSafeColor, delay: 24700);
                    break;
            }
        }
    }
    #endregion

    #region Priority Dictionary Class
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

#region Function Collection
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
    
    public static uint GetObjectIconId(IGameObject? obj)
    {
        if (obj == null || !obj.IsValid()) return 0;

        unsafe
        {
            var gameObj = (GameObject*)obj.Address;
            if (gameObj == null) return 0;

            return gameObj->NamePlateIconId;
        }
    }
    
    public static bool HasIconId(IGameObject? obj, int iconId)
    {
        return GetObjectIconId(obj) == iconId;
    }

}
#region Math Functions

public static class MathTools
{
    public static float DegToRad(this float deg) => (deg + 360f) % 360f / 180f * float.Pi;
    public static float RadToDeg(this float rad) => (rad + 2 * float.Pi) % (2 * float.Pi) / float.Pi * 180f;

    public static float NormalizeRadian(this float rad)
    {
        rad = (rad + 2 * float.Pi) % (2 * float.Pi);
        if (rad > float.Pi) rad -= 2 * float.Pi;
        return rad;
    }

    public static float GetRadian(this Vector3 point, Vector3 center)
        => MathF.Atan2(point.X - center.X, point.Z - center.Z);

    public static float GetLength(this Vector3 point, Vector3 center)
        => new Vector2(point.X - center.X, point.Z - center.Z).Length();

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

    public static int RadianToRegion(this float radian, int regionNum, int baseRegionIdx = 0, bool isDiagDiv = false, bool isCw = false)
    {
        var sepRad = float.Pi * 2 / regionNum;
        var inputAngle = radian * (isCw ? -1 : 1) + (isDiagDiv ? sepRad / 2 : 0);
        var rad = (inputAngle + 4 * float.Pi) % (2 * float.Pi);
        return ((int)Math.Floor(rad / sepRad) + baseRegionIdx + regionNum) % regionNum;
    }

    public static Vector3 FoldPointHorizon(this Vector3 point, float centerX)
        => point with { X = 2 * centerX - point.X };

    public static Vector3 FoldPointVertical(this Vector3 point, float centerZ)
        => point with { Z = 2 * centerZ - point.Z };

    public static Vector3 PointCenterSymmetry(this Vector3 point, Vector3 center)
        => point.RotateAndExtend(center, float.Pi, 0);

    public static int GetDecimalDigit(this int val, int x)
    {
        var valStr = val.ToString();
        var length = valStr.Length;
        if (x < 1 || x > length) return -1;
        var digitChar = valStr[length - x];
        return int.Parse(digitChar.ToString());
    }
}

#endregion

#region Index Functions
public static class IndexHelper
{
    public static int GetPlayerIdIndex(this ScriptAccessory sa, uint pid)
    {
        return sa.Data.PartyList.IndexOf(pid);
    }

    public static int GetMyIndex(this ScriptAccessory sa)
    {
        return sa.Data.PartyList.IndexOf(sa.Data.Me);
    }

    public static string GetPlayerJobById(this ScriptAccessory sa, uint pid)
    {
        var idx = sa.Data.PartyList.IndexOf(pid);
        var str = sa.GetPlayerJobByIndex(idx);
        return str;
    }

    public static string GetPlayerJobByIndex(this ScriptAccessory sa, int idx, bool fourPeople = false)
    {
        List<string> role8 = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
        List<string> role4 = ["T", "H", "D1", "D2"];
        if (idx < 0 || idx >= 8 || (fourPeople && idx >= 4))
            return "Unknown";
        return fourPeople ? role4[idx] : role8[idx];
    }

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
#endregion

#region Drawing Functions

public static class DrawTools
{
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
                throw new ArgumentException($"ownerObj {ownerObj} target type {ownerObj.GetType()} error");
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
                throw new ArgumentException($"targetObj {targetObj} target type {targetObj.GetType()} error");
        }

        if (draw)
            sa.Method.SendDraw(drawModeEnum, drawTypeEnum, dp);
        return dp;
    }

    public static DrawPropertiesEdit DrawGuidance(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name,
        float rotation = 0, float width = 1f, bool isSafe = true, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, 0, rotation, width,
            width, 0, 0, DrawModeEnum.Imgui, DrawTypeEnum.Displacement, isSafe, false, true, draw);

    public static DrawPropertiesEdit DrawGuidance(this ScriptAccessory sa,
        object targetObj, int delay, int destroy, string name, float rotation = 0, float width = 1f, bool isSafe = true,
        bool draw = true)
        => sa.DrawGuidance((ulong)sa.Data.Me, targetObj, delay, destroy, name, rotation, width, isSafe, draw);

    public static DrawPropertiesEdit DrawCircle(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name,
        float scale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, 0, delay, destroy, name, 2 * float.Pi, 0, scale, scale,
            0, 0, DrawModeEnum.Default, DrawTypeEnum.Circle, isSafe, byTime, false, draw);

    public static DrawPropertiesEdit DrawDonut(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name,
        float outScale, float innerScale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, 0, delay, destroy, name, 2 * float.Pi, 0, outScale, outScale, innerScale,
            innerScale, DrawModeEnum.Default, DrawTypeEnum.Donut, isSafe, byTime, false, draw);

    public static DrawPropertiesEdit DrawFan(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name, float radian, float rotation,
        float outScale, float innerScale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, radian, rotation, outScale, outScale, innerScale,
            innerScale, DrawModeEnum.Default, DrawTypeEnum.Fan, isSafe, byTime, false, draw);

    public static DrawPropertiesEdit DrawFan(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name, float radian, float rotation,
        float outScale, float innerScale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawFan(ownerObj, 0, delay, destroy, name, radian, rotation, outScale, innerScale, isSafe, byTime, draw);

    public static DrawPropertiesEdit DrawRect(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name, float rotation,
        float width, float length, bool isSafe = false, bool byTime = false, bool byY = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, 0, rotation, width, length, 0, 0,
            DrawModeEnum.Default, DrawTypeEnum.Rect, isSafe, byTime, byY, draw);

    public static DrawPropertiesEdit DrawRect(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name, float rotation,
        float width, float length, bool isSafe = false, bool byTime = false, bool byY = false, bool draw = true)
        => sa.DrawRect(ownerObj, 0, delay, destroy, name, rotation, width, length, isSafe, byTime, byY, draw);

    public static DrawPropertiesEdit DrawSightAvoid(this ScriptAccessory sa,
        object targetObj, int delay, int destroy, string name, bool isSafe = true, bool draw = true)
        => sa.DrawOwnerBase(sa.Data.Me, targetObj, delay, destroy, name, 0, 0, 0, 0, 0, 0,
            DrawModeEnum.Default, DrawTypeEnum.SightAvoid, isSafe, false, false, draw);

    public static DrawPropertiesEdit DrawKnockBack(this ScriptAccessory sa,
        object targetObj, int delay, int destroy, string name, float width, float length,
        bool isSafe = false, bool draw = true)
        => sa.DrawOwnerBase(sa.Data.Me, targetObj, delay, destroy, name, 0, float.Pi, width, length, 0, 0,
            DrawModeEnum.Default, DrawTypeEnum.Displacement, isSafe, false, false, draw);

    public static DrawPropertiesEdit DrawLine(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name, float rotation,
        float width, float length, bool isSafe = false, bool byTime = false, bool byY = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, 1, rotation, width, length, 0, 0,
            DrawModeEnum.Default, DrawTypeEnum.Line, isSafe, byTime, byY, draw);

    public static DrawPropertiesEdit DrawConnection(this ScriptAccessory sa, object ownerObj, object targetObj,
        int delay, int destroy, string name, float width = 1f, bool isSafe = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, 0, 0, width, width,
            0, 0, DrawModeEnum.Imgui, DrawTypeEnum.Line, isSafe, false, true, draw);

    public static DrawPropertiesEdit SetOwnersDistanceOrder(this DrawPropertiesEdit self, bool isNearOrder,
        uint orderIdx)
    {
        self.CentreResolvePattern = isNearOrder
            ? PositionResolvePatternEnum.PlayerNearestOrder
            : PositionResolvePatternEnum.PlayerFarestOrder;
        self.CentreOrderIndex = orderIdx;
        return self;
    }

    public static DrawPropertiesEdit SetOwnersEnmityOrder(this DrawPropertiesEdit self, uint orderIdx)
    {
        self.CentreResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        self.CentreOrderIndex = orderIdx;
        return self;
    }

    public static DrawPropertiesEdit SetPositionDistanceOrder(this DrawPropertiesEdit self, bool isNearOrder,
        uint orderIdx)
    {
        self.TargetResolvePattern = isNearOrder
            ? PositionResolvePatternEnum.PlayerNearestOrder
            : PositionResolvePatternEnum.PlayerFarestOrder;
        self.TargetOrderIndex = orderIdx;
        return self;
    }

    public static DrawPropertiesEdit SetOwnersTarget(this DrawPropertiesEdit self)
    {
        self.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
        return self;
    }
}

#endregion

#region Marker Functions

public static class MarkerHelper
{
    public static void LocalMarkClear(this ScriptAccessory sa)
    {
        sa.Log.Debug($"Deleting local markers.");
        sa.Method.Mark(0xE000000, MarkType.Attack1, true);
        sa.Method.Mark(0xE000000, MarkType.Attack2, true);
        sa.Method.Mark(0xE000000, MarkType.Attack3, true);
        sa.Method.Mark(0xE000000, MarkType.Attack4, true);
        sa.Method.Mark(0xE000000, MarkType.Attack5, true);
        sa.Method.Mark(0xE000000, MarkType.Attack6, true);
        sa.Method.Mark(0xE000000, MarkType.Attack7, true);
        sa.Method.Mark(0xE000000, MarkType.Attack8, true);
        sa.Method.Mark(0xE000000, MarkType.Bind1, true);
        sa.Method.Mark(0xE000000, MarkType.Bind2, true);
        sa.Method.Mark(0xE000000, MarkType.Bind3, true);
        sa.Method.Mark(0xE000000, MarkType.Stop1, true);
        sa.Method.Mark(0xE000000, MarkType.Stop2, true);
        sa.Method.Mark(0xE000000, MarkType.Square, true);
        sa.Method.Mark(0xE000000, MarkType.Circle, true);
        sa.Method.Mark(0xE000000, MarkType.Cross, true);
        sa.Method.Mark(0xE000000, MarkType.Triangle, true);
    }

    public static void MarkClear(this ScriptAccessory sa,
        bool enable = true, bool local = false, bool localString = false)
    {
        if (!enable) return;
        sa.Log.Debug($"Command received: clear markers");

        if (local)
        {
            if (localString)
                sa.Log.Debug($"[Character Simulation] Deleting local markers.");
            else
                sa.LocalMarkClear();
        }
        else
            sa.Method.MarkClear();
    }

    public static void MarkPlayerByIdx(this ScriptAccessory sa, int idx, MarkType marker,
        bool enable = true, bool local = false, bool localString = false)
    {
        if (!enable) return;
        if (localString)
            sa.Log.Debug($"[Local Character Simulation] Marking {idx}({sa.GetPlayerJobByIndex(idx)}) with {marker}.");
        else
            sa.Method.Mark(sa.Data.PartyList[idx], marker, local);
    }

    public static void MarkPlayerById(ScriptAccessory sa, uint id, MarkType marker,
        bool enable = true, bool local = false, bool localString = false)
    {
        if (!enable) return;
        if (localString)
            sa.Log.Debug($"[Local Character Simulation] Marking {sa.GetPlayerIdIndex(id)}({sa.GetPlayerJobById(id)}) with {marker}.");
        else
            sa.Method.Mark(id, marker, local);
    }

    public static int GetMarkedPlayerIndex(this ScriptAccessory sa, List<MarkType> markerList, MarkType marker)
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
            sa.Log.Error($"Invalid IGameObject passed.");
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

    public static void ScaleModify(this ScriptAccessory sa, IGameObject? obj, float scale)
    {
        if (obj == null || !obj.IsValid())
        {
            sa.Log.Error($"Invalid IGameObject passed.");
            return;
        }
        unsafe
        {
            GameObject* charaStruct = (GameObject*)obj.Address;
            charaStruct->Scale = scale;
            charaStruct->DisableDraw();
            charaStruct->EnableDraw();
        }
        sa.Log.Debug($"ScaleModify => {obj.Name.TextValue} | {obj} => {scale}");
    }

    public static void SetRotation(this ScriptAccessory sa, IGameObject? obj, float radian, bool show = false)
    {
        if (obj == null || !obj.IsValid())
        {
            sa.Log.Error($"Invalid IGameObject passed.");
            return;
        }
        unsafe
        {
            GameObject* charaStruct = (GameObject*)obj.Address;
            charaStruct->SetRotation(radian);
        }
        sa.Log.Debug($"Changing facing {obj.Name.TextValue} | {obj.EntityId} => {radian.RadToDeg()}");

        if (!show) return;
        var ownerObj = sa.GetById(obj.EntityId);
        if (ownerObj == null) return;
        var dp = sa.DrawGuidance(ownerObj, 0, 0, 2000, $"Changing facing {obj.Name.TextValue}", radian, draw: false);
        dp.FixRotation = true;
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);

    }

    public static void SetPosition(this ScriptAccessory sa, IGameObject? obj, Vector3 position, bool show = false)
    {
        if (obj == null || !obj.IsValid())
        {
            sa.Log.Error($"Invalid IGameObject passed.");
            return;
        }
        unsafe
        {
            GameObject* charaStruct = (GameObject*)obj.Address;
            charaStruct->SetPosition(position.X, position.Y, position.Z);
        }
        sa.Log.Debug($"Changing position => {obj.Name.TextValue} | {obj.EntityId} => {position}");

        if (!show) return;
        var dp = sa.DrawCircle(position, 0, 2000, $"Teleport point {obj.Name.TextValue}", 0.5f, true, draw: false);
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

    }
}

#endregion


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

    public static void DrawCircle(ScriptAccessory accessory, Vector3 position, Vector2 scale, int duration, string name, Vector4? color = null, bool scaleByTime = true, int delay = 0, DrawModeEnum drawmode = DrawModeEnum.Default)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = position;
        dp.Scale = scale;
        dp.Delay = delay;
        dp.DestoryAt = duration;
        if (scaleByTime) dp.ScaleMode = ScaleMode.ByTime;
        accessory.Method.SendDraw(drawmode, DrawTypeEnum.Circle, dp);
    }

    public static void DrawDisplacement(ScriptAccessory accessory, Vector3 targetPos, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0)
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
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
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

    public static void DrawDisplacementObject(ScriptAccessory accessory, ulong target, Vector2 scale, int duration, string name, float rotation, Vector4? color = null, int delay = 0, bool fix = false, DrawModeEnum drawmode = DrawModeEnum.Imgui)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Owner = accessory.Data.Me;
        dp.Color = color ?? accessory.Data.DefaultSafeColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.TargetObject = target;
        dp.Scale = scale;
        dp.Rotation = rotation;
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

    public static void DrawRectPosNoTarget(ScriptAccessory accessory, Vector3 pos, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0, ScaleMode scalemode = ScaleMode.None, Vector3? offset = null, DrawModeEnum drawMode = DrawModeEnum.Default)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = pos;
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

    public static void DrawDount(ScriptAccessory accessory, Vector3 position, Vector2 scale, Vector2 innerscale, int duration, string name, Vector4? color = null, bool scaleByTime = true, int delay = 0, DrawModeEnum drawmode = DrawModeEnum.Default)
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

    public static void DrawDountObject(ScriptAccessory accessory, ulong? ob, Vector2 scale, Vector2 innerscale, int duration, string name, Vector4? color = null, bool scaleByTime = true, int delay = 0, DrawModeEnum drawmode = DrawModeEnum.Default)
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



#endregion

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
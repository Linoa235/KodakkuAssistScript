using Dalamud.Utility.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
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

namespace Veever.Heavensward.the_Great_Gubal_Library_Hard;

[ScriptType(name: Name, territorys: [578], guid: "b49e44c7-278c-4c87-8afb-dcf4daecf65e",
    version: Version, author: "Veever", note: NoteStr, updateInfo: UpdateInfo)]

// ^(?!.*((Monk|Machinist|Dragoon|Samurai|Ninja|Viper|Reaper|Dancer|Bard|Astrologian|Sage|Scholar|(Eos|Selene)|Seraph|White Mage|Warrior|Paladin|Dark Knight|Gunbreaker|Pictomancer|Black Mage|Blue Mage|Summoner|Carbuncle|Bahamut|Phoenix|Garuda|Titan|Ifrit|Queen Automaton)\] (Used|Cast))).*35501.*$
// ^\[\w+\|[^|]+\|E\]\s\w+


public class the_Great_Gubal_Library_Hard
{
    const string NoteStr =
    """
    v0.0.0.1
    1. If you need a mechanic drawing or encounter any issues, please @ me on DC or DM me.
    2. Boss 1 knockback felt unnecessary to draw, so I just skipped it.
    Duckmen
    ------------------------------
    1. 如果需要某个机制的绘画或者哪里出了问题请在dc@我或者私信我
    2. Boss1的击退感觉没有必要画所以没有画
    鸭门
    """;

    const string UpdateInfo =
    """
        v0.0.0.1
    """;

    private const string Name = "LV.60 The Great Gubal Library (Hard)";
    private const string Version = "0.0.0.1";
    private const string DebugVersion = "a";

    private const bool Debugging = true;

    private static readonly List<string> Role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];

    [UserSetting("Announce Language")]
    public Language language { get; set; } = Language.Chinese;

    //[UserSetting("Draw opacity — higher value = more visible")]
    //public static float ColorAlpha { get; set; } = 1f;

    [UserSetting("Banner text toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS toggle")]
    public bool isTTS { get; set; } = true;

    //[UserSetting("Auto anti-knockback")]
    //public bool useAntiKnockback { get; set; } = true;

    [UserSetting("Guide arrow toggle")]
    public bool isLead { get; set; } = true;

    //[UserSetting("Target Marker toggle")]
    //public bool isMark { get; set; } = true;

    //[UserSetting("Local target marker toggle (ON = local only, OFF = party shared)")]
    //public bool LocalMark { get; set; } = true;

    //[UserSetting("Waymark guide toggle")]
    //public bool PostNamazuPrint { get; set; } = true;

    //[UserSetting("PostNamazuPort Setting")]
    //public int PostNamazuPort { get; set; } = 2019;

    //[UserSetting("Waymarks: local toggle(off = party shared, OOC only)")]
    //public bool PostNamazuisLocal { get; set; } = true;

    [UserSetting("Debug on/off (don't touch unless you know what you're doing)")]
    public bool isDebug { get; set; } = false;

    public enum Language
    {
        Chinese,
        English
    }

    private Dictionary<Tiles, (TilesColor, Vector3)> tileData = new Dictionary<Tiles, (TilesColor, Vector3)>
    {
        { Tiles.leftUpper, (TilesColor.white, new Vector3(375.30f, 0.02f,-165.33f))},
        { Tiles.leftLower, (TilesColor.white, new Vector3(352.68f, 0.02f, -165.33f))},
        { Tiles.rightUpper, (TilesColor.white, new Vector3(375.30f, 0.02f, -142.69f))},
        { Tiles.rightLower, (TilesColor.white, new Vector3(352.68f, 0.02f, -142.69f))}
    };

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        //accessory.Method.SendChat($"/e [DEBUG] {str}");
        accessory.Log.Debug($"[DEBUG] {str}");
    }

    public void Init(ScriptAccessory sa)
    {
        sa.Log.Debug($"Script {Name} v{Version}{DebugVersion} initialized.");
        sa.Method.RemoveDraw(".*");
    }


    #region Mobs
    [ScriptMethod(name: "---- Mobs ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void mobs(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "Terror Eye", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:2352"])]
    public void TerrorEye(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2200, $"TerrorEye-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Terror Eye Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:2352"], userControl: false)]
    public void TerrorEyeClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"TerrorEye-{ev.SourceId}");
    }

    [ScriptMethod(name: "Condemnation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:912"])]
    public void Condemnation(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(7f), 90, 2200, $"Condemnation-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Condemnation Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:912"], userControl: false)]
    public void CondemnationClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Condemnation-{ev.SourceId}");
    }

    [ScriptMethod(name: "Batter", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6538"])]
    public void Batter(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(7.5f), 2200, $"Batter-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Batter Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:6538"], userControl: false)]
    public void BatterClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Batter-{ev.SourceId}");
    }

    [ScriptMethod(name: "Dark", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:928"])]
    public void Dark(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(8f), 120, 2200, $"Dark-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Dark Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:928"], userControl: false)]
    public void DarkClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Dark-{ev.SourceId}");
    }

    [ScriptMethod(name: "Double Smash", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:618"])]
    public void DoubleSmash(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(8f), 120, 2200, $"DoubleSmash-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Double Smash Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:618"], userControl: false)]
    public void DoubleSmashClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"DoubleSmash-{ev.SourceId}");
    }

    [ScriptMethod(name: "Water III", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:5831"])]
    public void WaterIII(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(8f), 2700, $"WaterIII-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Water III Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:5831"], userControl: false)]
    public void WaterIIIClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"WaterIII-{ev.SourceId}");
    }

    [ScriptMethod(name: "Magic Hammer", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6544"])]
    public void MagicHammer(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(8f), 2700, $"MagicHammer-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Magic Hammer Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:6544"], userControl: false)]
    public void MagicHammerClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"MagicHammer-{ev.SourceId}");
    }
    #endregion


    #region Boss1
    private static IGameObject? GetBossObject(ScriptAccessory sa, uint BossDataId)
    {
        return sa.GetByDataId(BossDataId).FirstOrDefault();
    }

    [ScriptMethod(name: "---- Boss1 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
        userControl: Debugging)]
    public void Boss1(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "Discontinue", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6464"])]
    public void Discontinue(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20, 16.5f), 3700, $"Discontinue-{ev.SourceId}", sa.Data.DefaultDangerColor, scalemode: ScaleMode.ByTime);
    }


    [ScriptMethod(name: "Frightful Roar", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6459"])]
    public void FrightfulRoar(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(7.5f), 2700, $"Frightful Roar-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Discontinue2", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6466"])]
    public void Discontinue2(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(10, 20f), 3700, $"Discontinue2-{ev.SourceId}", sa.Data.DefaultDangerColor, scalemode: ScaleMode.ByTime);
    }

    [ScriptMethod(name: "Boss1 Clear", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:410"], userControl: false, suppress: 4000)]
    public async void Boss1Clear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw(".*");
        if (isLead)
        {
            await Task.Delay(3000);
            var startpos = new Vector3(100.68f, -8.00f, -0.01f);
            var endpos = new Vector3(100.68f, -8.00f, 5f);

            var startpos1 = new Vector3(100.74f, -8.00f, 36.02f);
            var endpos1 = new Vector3(95.74f, -8.00f, 36.02f);

            var startpos2 = new Vector3(76.95f, -8.00f, 72.00f);
            var endpos2 = new Vector3(80.95f, -8.00f, 72.00f);

            var startpos3 = new Vector3(126.68f, -8.00f, 71.96f);
            var endpos3 = new Vector3(126.68f, -8.00f, 65.00f);

            DrawHelper.DrawArrow(sa, startpos, endpos, new Vector2(1, 5f), int.MaxValue, $"Navi", color: sa.Data.DefaultSafeColor);
            DrawHelper.DrawArrow(sa, startpos1, endpos1, new Vector2(1, 4f), int.MaxValue, $"Navi1", color: sa.Data.DefaultSafeColor);
            DrawHelper.DrawArrow(sa, startpos2, endpos2, new Vector2(1, 4f), int.MaxValue, $"Navi2", color: sa.Data.DefaultSafeColor);
            DrawHelper.DrawArrow(sa, startpos3, endpos3, new Vector2(1, 4f), int.MaxValue, $"Navi3", color: sa.Data.DefaultSafeColor);
        }
    }

    #endregion


    #region Boss2
    [ScriptMethod(name: "---- Boss2 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void Boss2(Event ev, ScriptAccessory sa)
    {
    }


    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6469"])]
    public void boss2AOE(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Navi.*");
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 2700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Sea of Flames", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6504"])]
    public void SeaofFlames(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6), 2700, $"SeaofFlames-{ev.EffectPosition}");
    }

    [ScriptMethod(name: "Rush", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6471"])]
    public void Rush(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Tether Away" : "Tether Away";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 3000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");

        }
    }

    [ScriptMethod(name: "Seal of Night and Day", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(0059|0058)$"])]
    public void SealofNightandDay(Event ev, ScriptAccessory sa)
    {
        // 0059 Blue
        // 0058 red
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = "";
            if (ev.Id == 0059)
            {
                msg = language == Language.Chinese ? "Stand on Blue tile" : "Stand on Blue tile";
            } else
            {
                msg = language == Language.Chinese ? "Stand on Red tile" : "Stand on Red tile";
            }

            if (isText) sa.Method.TextInfo($"{msg}", duration: 5700, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");

        }
    }

    [ScriptMethod(name: "Boss2 Clear", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:434"], userControl: false, suppress: 4000)]
    public async void Boss2Clear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw(".*");
        if (isLead)
        {
            await Task.Delay(3000);

            var startpos = new Vector3(77.87f, -16.00f, -96.00f);
            var endpos = new Vector3(77.87f, -16.00f, -100.00f);

            DrawHelper.DrawArrow(sa, startpos, endpos, new Vector2(1, 5f), int.MaxValue, $"Navi", color: sa.Data.DefaultSafeColor);
        }
    }
    #endregion

    #region Boss3
    [ScriptMethod(name: "---- Boss3 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void Boss3(Event ev, ScriptAccessory sa)
    {
    }


    [ScriptMethod(name: "tileStore", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(6479|6480)$"])]
    public void tileStore(Event ev, ScriptAccessory sa)
    {
        // 6479 white
        // 6480 black
        sa.Method.RemoveDraw($"Navi.*");
        if (ev.ActionId == 6479)
        {
            var keysToUpdate = tileData
                .Where(tile => Vector3.Distance(tile.Value.Item2, ev.EffectPosition) < 2f)
                .Select(tile => tile.Key)
                .ToList();

            foreach (var key in keysToUpdate)
            {
                tileData[key] = (TilesColor.white, tileData[key].Item2);
            }
        }

        if (ev.ActionId == 6480)
        {
            var keysToUpdate = tileData
                .Where(tile => Vector3.Distance(tile.Value.Item2, ev.EffectPosition) < 2f)
                .Select(tile => tile.Key)
                .ToList();

            foreach (var key in keysToUpdate)
            {
                tileData[key] = (TilesColor.black, tileData[key].Item2);
            }
        }
    }

    [ScriptMethod(name: "tileStore1", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:regex:^(2007508)$"])]
    public void tileStore1(Event ev, ScriptAccessory sa)
    {

        // 2007508 green
        var keysToUpdate = tileData
            .Where(tile => Vector3.Distance(tile.Value.Item2, ev.SourcePosition) < 2f)
            .Select(tile => tile.Key)
            .ToList();

        foreach (var key in keysToUpdate)
        {
            tileData[key] = (TilesColor.green, tileData[key].Item2);
        }

    }

    [ScriptMethod(name: "On the Properties of Quakes", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:(6486)"])]
    public void Quakes(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Stand on White tile" : "Stand on White tile";

        if (isText) sa.Method.TextInfo($"{msg}", duration: 6700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        foreach (var tile in tileData)
        {
            if (tile.Value.Item1 == TilesColor.white)
            {
                DrawHelper.DrawCircle(sa, tile.Value.Item2, new Vector2(6f), 6700, $"Quakes", color: sa.Data.DefaultSafeColor, scaleByTime: false);
            }
        }
    }

    [ScriptMethod(name: "On the Properties of Tornados", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:(6487)"])]
    public void Tornados(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Stand on Black tile" : "Stand on Black tile";

        if (isText) sa.Method.TextInfo($"{msg}", duration: 6700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        foreach (var tile in tileData)
        {
            if (tile.Value.Item1 == TilesColor.black)
            {
                DrawHelper.DrawCircle(sa, tile.Value.Item2, new Vector2(4f), 6700, $"Tornados", color: sa.Data.DefaultSafeColor, scaleByTime: false);
            }
        }
    }

    [ScriptMethod(name: "On the Properties of Imps", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:(6489)"])]
    public void Imps(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Stand on Green tile" : "Stand on Green tile";

        if (isText) sa.Method.TextInfo($"{msg}", duration: 6700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        foreach (var tile in tileData)
        {
            if (tile.Value.Item1 == TilesColor.green)
            {
                DrawHelper.DrawCircle(sa, tile.Value.Item2, new Vector2(4f), 6700, $"Imps", color: sa.Data.DefaultSafeColor, scaleByTime: false);
            }
        }
    }
    private enum TilesColor
    {
       white = 0,
       green = 1,
       black = 2,
    }

    private enum Tiles
    {
        leftUpper = 0,
        leftLower = 1,
        rightUpper = 2,
        rightLower = 3,
    }


    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6485"])]
    public void boss3AOE(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 2500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
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

        /// <summary>
        /// Adds priority to a specific Key
        /// </summary>
        /// <param name="idx">key</param>
        /// <param name="priority">priority value</param>
        public void AddPriority(int idx, int priority)
        {
            Priorities[idx] += priority;
        }

        /// <summary>
        /// Finds the first 'num' smallest values from Priorities and returns a new Dict
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public List<KeyValuePair<int, int>> SelectSmallPriorityIndices(int num)
        {
            return SelectMiddlePriorityIndices(0, num);
        }

        /// <summary>
        /// Finds the first 'num' largest values from Priorities and returns a new Dict
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public List<KeyValuePair<int, int>> SelectLargePriorityIndices(int num)
        {
            return SelectMiddlePriorityIndices(0, num, true);
        }

        /// <summary>
        /// Finds the middle values from Priorities in ascending order and returns a new Dict
        /// </summary>
        /// <param name="skip">Skip 'skip' elements. If starting from the second element, skip=1</param>
        /// <param name="num"></param>
        /// <param name="descending">Sort descending, default is false</param>
        /// <returns></returns>
        public List<KeyValuePair<int, int>> SelectMiddlePriorityIndices(int skip, int num, bool descending = false)
        {
            if (Priorities.Count < skip + num)
                return new List<KeyValuePair<int, int>>();

            IEnumerable<KeyValuePair<int, int>> sortedPriorities;
            if (descending)
            {
                // Sort by value descending, then by key
                sortedPriorities = Priorities
                    .OrderByDescending(pair => pair.Value) // Sort by value first
                    .ThenBy(pair => pair.Key) // Then sort by key
                    .Skip(skip) // Skip the first 'skip' elements
                    .Take(num); // Take the first 'num' key-value pairs
            }
            else
            {
                // Sort by value ascending, then by key
                sortedPriorities = Priorities
                    .OrderBy(pair => pair.Value) // Sort by value first
                    .ThenBy(pair => pair.Key) // Then sort by key
                    .Skip(skip) // Skip the first 'skip' elements
                    .Take(num); // Take the first 'num' key-value pairs
            }

            return sortedPriorities.ToList();
        }

        /// <summary>
        /// Finds the data at the 'idx'th position from Priorities in ascending order and returns it
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="descending">Sort descending, default is false</param>
        /// <returns></returns>
        public KeyValuePair<int, int> SelectSpecificPriorityIndex(int idx, bool descending = false)
        {
            var sortedPriorities = SelectMiddlePriorityIndices(0, Priorities.Count, descending);
            return sortedPriorities[idx];
        }

        /// <summary>
        /// Finds the corresponding value from Priorities for the given key and returns its sorted position
        /// </summary>
        /// <param name="key"></param>
        /// <param name="descending">Sort descending, default is false</param>
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
        /// Adds priority values all at once
        /// Usually used for special priorities (e.g., H-T-D-H)
        /// </summary>
        /// <param name="priorities"></param>
        public void AddPriorities(List<int> priorities)
        {
            if (Priorities.Count != priorities.Count)
                throw new ArgumentException("Input list length does not match internal length");

            for (var i = 0; i < Priorities.Count; i++)
                AddPriority(i, priorities[i]);
        }

        /// <summary>
        /// Outputs the priority dictionary's Keys and priorities
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

    #endregion Priority Dictionary Class

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
}
#region Calculation Functions

public static class MathTools
{
    public static float DegToRad(this float deg) => (deg + 360f) % 360f / 180f * float.Pi;
    public static float RadToDeg(this float rad) => (rad + 2 * float.Pi) % (2 * float.Pi) / float.Pi * 180f;

    /// <summary>
    /// Gets the radian value of any point relative to the center point, with (0, 0, 1) direction as 0 and (1, 0, 0) direction as pi/2.
    /// I.e., increasing counterclockwise.
    /// </summary>
    /// <param name="point">Any point</param>
    /// <param name="center">Center point</param>
    /// <returns></returns>
    public static float GetRadian(this Vector3 point, Vector3 center)
        => MathF.Atan2(point.X - center.X, point.Z - center.Z);

    /// <summary>
    /// Gets the distance between any point and the center point.
    /// </summary>
    /// <param name="point">Any point</param>
    /// <param name="center">Center point</param>
    /// <returns></returns>
    public static float GetLength(this Vector3 point, Vector3 center)
        => new Vector2(point.X - center.X, point.Z - center.Z).Length();

    /// <summary>
    /// Rotates any point counterclockwise around the center point and extends it.
    /// </summary>
    /// <param name="point">Any point</param>
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
    /// Gets the division region for a given angle
    /// </summary>
    /// <param name="radian">Input radian</param>
    /// <param name="regionNum">Number of region divisions</param>
    /// <param name="baseRegionIdx">Initial Idx of the region where 0 degrees is located</param>>
    /// <param name="isDiagDiv">Whether to divide diagonally, default is false</param>
    /// <param name="isCw">Whether to increase clockwise, default is false</param>
    /// <returns></returns>
    public static int RadianToRegion(this float radian, int regionNum, int baseRegionIdx = 0, bool isDiagDiv = false, bool isCw = false)
    {
        var sepRad = float.Pi * 2 / regionNum;
        var inputAngle = radian * (isCw ? -1 : 1) + (isDiagDiv ? sepRad / 2 : 0);
        var rad = (inputAngle + 4 * float.Pi) % (2 * float.Pi);
        return ((int)Math.Floor(rad / sepRad) + baseRegionIdx + regionNum) % regionNum;
    }

    /// <summary>
    /// Folds the input point horizontally
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerX">Center line X coordinate</param>
    /// <returns></returns>
    public static Vector3 FoldPointHorizon(this Vector3 point, float centerX)
        => point with { X = 2 * centerX - point.X };

    /// <summary>
    /// Folds the input point vertically
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerZ">Center line Z coordinate</param>
    /// <returns></returns>
    public static Vector3 FoldPointVertical(this Vector3 point, float centerZ)
        => point with { Z = 2 * centerZ - point.Z };

    /// <summary>
    /// Center-symmetrically transforms the input point
    /// </summary>
    /// <param name="point">Input point</param>
    /// <param name="center">Center point</param>
    /// <returns></returns>
    public static Vector3 PointCenterSymmetry(this Vector3 point, Vector3 center)
        => point.RotateAndExtend(center, float.Pi, 0);

    /// <summary>
    /// Gets the specified digit of a given number
    /// </summary>
    /// <param name="val">Given number</param>
    /// <param name="x">Corresponding digit, ones place = 1</param>
    /// <returns></returns>
    public static int GetDecimalDigit(this int val, int x)
    {
        var valStr = val.ToString();
        var length = valStr.Length;
        if (x < 1 || x > length) return -1;
        var digitChar = valStr[length - x]; // Take the x-th digit from the right
        return int.Parse(digitChar.ToString());
    }
}

#endregion Calculation Functions

#region Position Sequence Functions
public static class IndexHelper
{
    /// <summary>
    /// Input player dataId, get the corresponding position index
    /// </summary>
    /// <param name="pid">Player SourceId</param>
    /// <param name="sa"></param>
    /// <returns>Position index corresponding to the player</returns>
    public static int GetPlayerIdIndex(this ScriptAccessory sa, uint pid)
    {
        // Get player IDX
        return sa.Data.PartyList.IndexOf(pid);
    }

    /// <summary>
    /// Gets the position index corresponding to the main perspective player
    /// </summary>
    /// <param name="sa"></param>
    /// <returns>Position index corresponding to the main perspective player</returns>
    public static int GetMyIndex(this ScriptAccessory sa)
    {
        return sa.Data.PartyList.IndexOf(sa.Data.Me);
    }

    /// <summary>
    /// Input player dataId, get the corresponding position title, output characters only for text output
    /// </summary>
    /// <param name="pid">Player SourceId</param>
    /// <param name="sa"></param>
    /// <returns>Position title corresponding to the player</returns>
    public static string GetPlayerJobById(this ScriptAccessory sa, uint pid)
    {
        // Get player role abbreviation, for DEBUG output only
        var idx = sa.Data.PartyList.IndexOf(pid);
        var str = sa.GetPlayerJobByIndex(idx);
        return str;
    }

    /// <summary>
    /// Input position index, get the corresponding position title, output characters only for text output
    /// </summary>
    /// <param name="idx">Position index</param>
    /// <param name="fourPeople">Whether it's a four-player dungeon</param>
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
    /// Converts the information in the List to a string.
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="myList"></param>
    /// <param name="isJob">Whether it's a job, call the job conversion function before converting to string</param>
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
#endregion Position Sequence Functions

#region Drawing Functions

public static class DrawTools
{
    /// <summary>
    /// Returns a drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Drawing reference, can be UID or position</param>
    /// <param name="targetObj">Drawing target, can be UID or position</param>
    /// <param name="delay">Appears after delay ms</param>
    /// <param name="destroy">Disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="radian">Radian range of the drawn graphic</param>
    /// <param name="rotation">Rotation radian of the drawn graphic, increases counterclockwise relative to owner's front</param>
    /// <param name="width">Width of the drawn graphic, can remain consistent with length for some graphics</param>
    /// <param name="length">Length of the drawn graphic, can remain consistent with width for some graphics</param>
    /// <param name="innerWidth">Inner width of the drawn graphic, can remain consistent with length for some graphics</param>
    /// <param name="innerLength">Inner length of the drawn graphic, can remain consistent with width for some graphics</param>
    /// <param name="drawModeEnum">Drawing mode</param>
    /// <param name="drawTypeEnum">Drawing type</param>
    /// <param name="isSafe">Whether to use safe color</param>
    /// <param name="byTime">Animation effect fills over time</param>
    /// <param name="byY">Animation effect changes with distance</param>
    /// <param name="draw">Whether to draw directly</param>
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
                throw new ArgumentException($"ownerObj {ownerObj} target type {ownerObj.GetType()} input error");
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
                throw new ArgumentException($"targetObj {targetObj} target type {targetObj.GetType()} input error");
        }

        if (draw)
            sa.Method.SendDraw(drawModeEnum, drawTypeEnum, dp);
        return dp;
    }

    /// <summary>
    /// Returns a guidance drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Starting point</param>
    /// <param name="targetObj">End point</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="rotation">Arrow rotation angle</param>
    /// <param name="width">Arrow width</param>
    /// <param name="isSafe">Whether to use safe color</param>
    /// <param name="draw">Whether to draw directly</param>
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
    /// Returns a circle drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Circle center</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="scale">Circle radius</param>
    /// <param name="byTime">Whether to expand over time</param>
    /// <param name="isSafe">Whether to use safe color</param>
    /// <param name="draw">Whether to draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawCircle(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name,
        float scale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, 0, delay, destroy, name, 2 * float.Pi, 0, scale, scale,
            0, 0, DrawModeEnum.Default, DrawTypeEnum.Circle, isSafe, byTime, false, draw);

    /// <summary>
    /// Returns a donut drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Circle center</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="outScale">Outer radius</param>
    /// <param name="innerScale">Inner radius</param>
    /// <param name="byTime">Whether to expand over time</param>
    /// <param name="isSafe">Whether to use safe color</param>
    /// <param name="draw">Whether to draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawDonut(this ScriptAccessory sa,
        object ownerObj, int delay, int destroy, string name,
        float outScale, float innerScale, bool isSafe = false, bool byTime = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, 0, delay, destroy, name, 2 * float.Pi, 0, outScale, outScale, innerScale,
            innerScale, DrawModeEnum.Default, DrawTypeEnum.Donut, isSafe, byTime, false, draw);

    /// <summary>
    /// Returns a fan drawing
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
    /// <param name="byTime">Whether to expand over time</param>
    /// <param name="isSafe">Whether to use safe color</param>
    /// <param name="draw">Whether to draw directly</param>
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
    /// Returns a rectangle drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Rectangle start</param>
    /// <param name="targetObj">Target</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="rotation">Rotation angle</param>
    /// <param name="width">Rectangle width</param>
    /// <param name="length">Rectangle length</param>
    /// <param name="byTime">Whether to expand over time</param>
    /// <param name="byY">Whether to expand with distance</param>
    /// <param name="isSafe">Whether to use safe color</param>
    /// <param name="draw">Whether to draw directly</param>
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
    /// Returns a sight avoidance drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="targetObj">Target</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="isSafe">Whether to use safe color</param>
    /// <param name="draw">Whether to draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawSightAvoid(this ScriptAccessory sa,
        object targetObj, int delay, int destroy, string name, bool isSafe = true, bool draw = true)
        => sa.DrawOwnerBase(sa.Data.Me, targetObj, delay, destroy, name, 0, 0, 0, 0, 0, 0,
            DrawModeEnum.Default, DrawTypeEnum.SightAvoid, isSafe, false, false, draw);

    /// <summary>
    /// Returns a knockback drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="targetObj">Knockback source</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="width">Arrow width</param>
    /// <param name="length">Arrow length</param>
    /// <param name="isSafe">Whether to use safe color</param>
    /// <param name="draw">Whether to draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawKnockBack(this ScriptAccessory sa,
        object targetObj, int delay, int destroy, string name, float width, float length,
        bool isSafe = false, bool draw = true)
        => sa.DrawOwnerBase(sa.Data.Me, targetObj, delay, destroy, name, 0, float.Pi, width, length, 0, 0,
            DrawModeEnum.Default, DrawTypeEnum.Displacement, isSafe, false, false, draw);

    /// <summary>
    /// Returns a line drawing
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Line start</param>
    /// <param name="targetObj">Line target</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="rotation">Rotation angle</param>
    /// <param name="width">Line width</param>
    /// <param name="length">Line length</param>
    /// <param name="byTime">Whether to expand over time</param>
    /// <param name="byY">Whether to expand with distance</param>
    /// <param name="isSafe">Whether to use safe color</param>
    /// <param name="draw">Whether to draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawLine(this ScriptAccessory sa,
        object ownerObj, object targetObj, int delay, int destroy, string name, float rotation,
        float width, float length, bool isSafe = false, bool byTime = false, bool byY = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, 1, rotation, width, length, 0, 0,
            DrawModeEnum.Default, DrawTypeEnum.Line, isSafe, byTime, byY, draw);

    /// <summary>
    /// Returns a connection line drawing between two objects
    /// </summary>
    /// <param name="sa"></param>
    /// <param name="ownerObj">Starting source</param>
    /// <param name="targetObj">Target source</param>
    /// <param name="delay">Delay</param>
    /// <param name="destroy">Disappearance time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="width">Line width</param>
    /// <param name="isSafe">Whether to use safe color</param>
    /// <param name="draw">Whether to draw directly</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawConnection(this ScriptAccessory sa, object ownerObj, object targetObj,
        int delay, int destroy, string name, float width = 1f, bool isSafe = false, bool draw = true)
        => sa.DrawOwnerBase(ownerObj, targetObj, delay, destroy, name, 0, 0, width, width,
            0, 0, DrawModeEnum.Imgui, DrawTypeEnum.Line, isSafe, false, true, draw);

    /// <summary>
    /// Assigns the input dp with a near/far target drawing based on ownerId as the source
    /// </summary>
    /// <param name="self"></param>
    /// <param name="isNearOrder">Near order or far order calculated from owner</param>
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
    /// Assigns the input dp with an enmity order drawing based on ownerId as the source
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
    /// Assigns the input dp with a near/far target drawing based on position as the source
    /// </summary>
    /// <param name="self"></param>
    /// <param name="isNearOrder">Near order or far order calculated from owner</param>
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
    /// Assigns the input dp with a drawing based on ownerId's cast target as the source
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
        sa.Log.Debug($"Received command: delete markers");

        if (local)
        {
            if (localString)
                sa.Log.Debug($"[Character simulation] Deleting local markers.");
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
            sa.Log.Debug($"[Local character simulation] Marking {idx}({sa.GetPlayerJobByIndex(idx)}) with {marker}.");
        else
            sa.Method.Mark(sa.Data.PartyList[idx], marker, local);
    }

    public static void MarkPlayerById(ScriptAccessory sa, uint id, MarkType marker,
        bool enable = true, bool local = false, bool localString = false)
    {
        if (!enable) return;
        if (localString)
            sa.Log.Debug($"[Local character simulation] Marking {sa.GetPlayerIdIndex(id)}({sa.GetPlayerJobById(id)}) with {marker}.");
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
            sa.Log.Error($"Input IGameObject is invalid.");
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
            sa.Log.Error($"Input IGameObject is invalid.");
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
            sa.Log.Error($"Input IGameObject is invalid.");
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
            sa.Log.Error($"Input IGameObject is invalid.");
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

    public static void DrawFan(ScriptAccessory accessory, Vector3 position, float rotation, Vector2 scale, float angle, int duration, string name, Vector4? color = null, int delay = 0, bool fix = false)
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
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    public static void DrawFanObject(ScriptAccessory accessory, ulong owner, float rotation, Vector2 scale, float angle, int duration, string name, Vector4? color = null, int delay = 0, bool scaleByTime = true, bool fix = false, Vector3? offset = null)
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
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
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

    public static void DrawArrow(ScriptAccessory accessory, Vector3 startPosition, Vector3 endPosition, Vector2 scale, int duration, string name, Vector4? color = null, int delay = 0)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = color ?? accessory.Data.DefaultDangerColor;
        dp.Position = startPosition;
        dp.TargetPosition = endPosition;
        dp.Scale = scale;
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
}


#endregion Function Collection

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
    /// Gets the player's job name or abbreviation
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
    /// Determines whether the player is a tank job
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

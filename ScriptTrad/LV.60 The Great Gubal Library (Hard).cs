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

[ScriptType(name: Name, territorys: [578], guid: "594b2ffa-852a-4875-b65d-d19a4a663b30",
    version: Version, author: "Linoa235", note: NoteStr, updateInfo: UpdateInfo)]

public class the_Great_Gubal_Library_Hard
{
    const string NoteStr =
    """
    v0.0.0.1
    1. If you need a draw for a mechanic or notice any issues, @ me on DC or DM me.
    2. Boss 1 knockback felt unnecessary to draw, so I just skipped it.
    Duckmen
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

    [UserSetting("Language")]
    public Language language { get; set; } = Language.Chinese;

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
        accessory.Log.Debug($"[DEBUG] {str}");
    }

    public void Init(ScriptAccessory sa)
    {
        sa.Log.Debug($"Script {Name} v{Version}{DebugVersion} initialized.");
        sa.Method.RemoveDraw(".*");
    }

    #region Trash Mobs
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

    [ScriptMethod(name: "Discontinue 2", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:6466"])]
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
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = "";
            if (ev.Id == 0059)
            {
                msg = language == Language.Chinese ? "Stand on Blue tile" : "Stand on Blue tile";
            }
            else
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

    [ScriptMethod(name: "Tile Store", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(6479|6480)$"])]
    public void tileStore(Event ev, ScriptAccessory sa)
    {
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

    [ScriptMethod(name: "Tile Store 1", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:regex:^(2007508)$"])]
    public void tileStore1(Event ev, ScriptAccessory sa)
    {
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
}

#region Function Libraries
// All helper classes (EventExtensions, IbcHelper, MathTools, IndexHelper, DrawTools, MarkerHelper, SpecialFunction, NamazuHelper, DrawHelper, ExtensionMethods, ExtensionVisibleMethod) are identical to previous files
#endregion
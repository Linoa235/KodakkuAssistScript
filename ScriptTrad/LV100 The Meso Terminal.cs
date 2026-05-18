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

namespace Veever.DawnTrail.TheMesoTerminal;

[ScriptType(name: Name, territorys: [1292], guid: "d0df8a70-b611-4433-9531-24be07b984f9",
    version: Version, Author: "Linoa235", note: NoteStr, updateInfo: UpdateInfo)]

public class TheMesoTerminal
{
    const string NoteStr =
    """
    v0.0.0.4
    1. If you need a draw for a mechanic or notice any issues, @ me on DC or DM me.
    2. Boss 3: The circle draw for Bombardment may fail to draw if you disconnect
    Duckmen
    """;

    const string UpdateInfo =
    """
        v0.0.0.4
        Fixed some logic issues with Boss 3 NPC mechanic
    """;

    private const string Name = "LV.100 The Meso Terminal";
    private const string Version = "0.0.0.4";
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

    [UserSetting("Guide Arrow Toggle")]
    public bool isLead { get; set; } = true;

    [UserSetting("Target Marker Toggle")]
    public bool isMark { get; set; } = true;

    [UserSetting("Local Target Marker Toggle (ON = local only, OFF = party)")]
    public bool LocalMark { get; set; } = true;

    [UserSetting("Waymark Guide Toggle")]
    public bool PostNamazuPrint { get; set; } = true;

    [UserSetting("PostNamazu Port Setting")]
    public int PostNamazuPort { get; set; } = 2019;

    [UserSetting("Waymarks: local toggle (OFF = party shared, OOC only)")]
    public bool PostNamazuisLocal { get; set; } = true;

    [UserSetting("Debug toggle, please turn off for non-development use")]
    public bool isDebug { get; set; } = false;

    public enum Language
    {
        Chinese,
        English
    }

    private Dictionary<ulong, ulong> _tethersDict = new();
    private static bool _initHint = false;

    private List<ulong> _terrorList = new();
    private readonly object CountLock = new object();

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    public void Init(ScriptAccessory sa)
    {
        sa.Log.Debug($"Script {Name} v{Version}{DebugVersion} initialized.");
        sa.Method.RemoveDraw(".*");

        _terrorList.Clear();
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

    [ScriptMethod(name: "Electric Shock", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44040"])]
    public void ElectricShock(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(5f), 4000, $"ElectricShock-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Electric Shock Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:44040"], userControl: false)]
    public void ElectricShockClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"ElectricShock-{ev.SourceId}");
    }

    [ScriptMethod(name: "Alexandrian Gravity", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44041"])]
    public void AlexandrianGravity(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2700, $"Alexandrian Gravity-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Alexandrian Gravity Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:44041"], userControl: false)]
    public void AlexandrianGravityClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Alexandrian Gravity-{ev.SourceId}");
    }

    [ScriptMethod(name: "Pressure Wave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44044"])]
    public void PressureWave(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanOwner(sa, ev.SourceId, 0, new Vector2(6f), 120, 2700, $"PressureWave-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Pressure Wave Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:44044"], userControl: false)]
    public void PressureWaveClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"PressureWave-{ev.SourceId}");
    }

    [ScriptMethod(name: "Rusted Knives & Overpower", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4404[56])$"])]
    public void RustedKnives_Overpower(Event ev, ScriptAccessory sa)
    {
        DebugMsg($"In RustedKnives_Overpower, ActionId: {ev.ActionId}", sa);
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(5f), 2700, $"Rusted Knives & Overpower-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "Rusted Knives & Overpower Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:regex:^(4404[56])$"], userControl: false)]
    public void RustedKnives_OverpowerClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Rusted Knives & Overpower-{ev.SourceId}");
    }

    [ScriptMethod(name: "Electray", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44048)$"])]
    public void Electray(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(4, 14), 3700, $"Electray-{ev.SourceId}", scalemode: ScaleMode.ByTime);
    }

    [ScriptMethod(name: "Electray Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:regex:^(44048)$"], userControl: false)]
    public void ElectrayClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Electray-{ev.SourceId}");
    }

    [ScriptMethod(name: "Steelforged Belief", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45042)$"])]
    public void SteelforgedBelief(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 3700, $"Steelforged Belief-{ev.SourceId}", sa.Data.DefaultDangerColor);
    }

    [ScriptMethod(name: "SteelforgedBelief Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:regex:^(45042)$"], userControl: false)]
    public void SteelforgedBeliefClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Steelforged Belief-{ev.SourceId}");
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

    [ScriptMethod(name: "Pungent Aerosol", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43807"])]
    public void PungentAerosol(Event ev, ScriptAccessory sa)
    {
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = "Pungent Aerosol";
        dp.Color = sa.Data.DefaultSafeColor;
        dp.Owner = sa.Data.Me;
        dp.TargetPosition = ev.EffectPosition;
        dp.Rotation = float.Pi;
        dp.Scale = new Vector2(2, 24f);
        dp.DestoryAt = 4000;
        if (isLead) sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "Biochemical Front", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43802"])]
    public void BiochemicalFront(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Avoid front" : "Avoid front";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        DrawHelper.DrawFanOwner(sa, ev.SourceId, 0, new Vector2(40), 180, 4700, $"Biochemical Front-{ev.SourceId}");
    }

    [ScriptMethod(name: "Concentrated Dose", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43799"])]
    public void ConcentratedDose(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Dot Tankbuster" : "Dot Tankbuster";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Sterile Sphere", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4380[56])$"])]
    public void SterileSphere(Event ev, ScriptAccessory sa)
    {
        DebugMsg($"In SterileSphere, ActionId: {ev.ActionId}", sa);
        if (ev.ActionId == 43805)
        {
            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(15f), 5200, $"Sterile Sphere-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha), drawmode: DrawModeEnum.Imgui);
        }

        if (ev.ActionId == 43806)
        {
            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(8f), 5200, $"Sterile Sphere-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha), drawmode: DrawModeEnum.Imgui);
        }
    }
    #endregion

    #region Boss2
    [ScriptMethod(name: "---- Boss2 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void Boss2(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43578"], suppress: 5000)]
    public void boss2AOE(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 2700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Boss Tether Check & Mark", eventType: EventTypeEnum.Tether, eventCondition: ["Id:00F9"])]
    public void bossTetherCheck(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Attack tethered Boss" : "Attack tethered Boss";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 2700, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
            if (isMark) sa.Method.MarkClear();
            if (isMark) sa.Method.Mark((uint)ev.SourceId, MarkType.Attack1, LocalMark);
        }
    }

    [ScriptMethod(name: "Dismemberment", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43587"])]
    public void Dismemberment(Event ev, ScriptAccessory sa)
    {
        DebugMsg($"In Dismemberment, SourceId: {ev.SourceId}, TargetId: {ev.TargetId}", sa);
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = $"Dismemberment: {ev.SourceId}";
        dp.Color = new Vector4(1, 1, 0, 1);
        dp.Owner = ev.SourceId;
        dp.Scale = new Vector2(4, 16);
        dp.DestoryAt = 5700;
        dp.ScaleMode = ScaleMode.ByTime;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Peal of Judgment Draw", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:43594"])]
    public void PealofJudgment(Event ev, ScriptAccessory sa)
    {
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = $"Peal of Judgment: {ev.SourceId}";
        dp.Color = new Vector4(1, 0, 1, 1);
        dp.Owner = ev.SourceId;
        dp.Scale = new Vector2(4, 2);
        dp.DestoryAt = 500;
        dp.Offset = new Vector3(0, 0, 1);
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        var dp1 = sa.Data.GetDefaultDrawProperties();
        dp1.Name = $"Peal of Judgment arrow: {ev.SourceId}";
        dp1.Color = sa.Data.DefaultSafeColor;
        dp1.Owner = ev.SourceId;
        dp1.Scale = new Vector2(1, 2);
        dp1.DestoryAt = 500;
        sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp1);
    }

    [ScriptMethod(name: "Execution Wheel (Dynamo)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43596"])]
    public void ExecutionWheel(Event ev, ScriptAccessory sa)
    {
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = $"Execution Wheel: {ev.SourceId}";
        dp.Color = sa.Data.DefaultDangerColor;
        dp.Owner = ev.SourceId;
        dp.Scale = new Vector2(9);
        dp.InnerScale = new Vector2(4);
        dp.Radian = 2 * float.Pi;
        dp.DestoryAt = 3200;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }

    [ScriptMethod(name: "Chopping Block (Chariot)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43595"])]
    public void ChoppingBlock(Event ev, ScriptAccessory sa)
    {
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = $"Chopping Block: {ev.SourceId}";
        dp.Color = sa.Data.DefaultDangerColor;
        dp.Owner = ev.SourceId;
        dp.Scale = new Vector2(6);
        dp.DestoryAt = 3200;
        dp.ScaleMode = ScaleMode.ByTime;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Flaying Flail", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43592"])]
    public void FlayingFlail(Event ev, ScriptAccessory sa)
    {
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = $"Flaying Flail: {ev.SourceId}";
        dp.Color = sa.Data.DefaultDangerColor;
        dp.Position = ev.EffectPosition;
        dp.Scale = new Vector2(5);
        dp.DestoryAt = 4700;
        sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Will Breaker", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:44856"])]
    public void WillBreaker(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Interrupt!" : "Interrupt!";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
    }

    [ScriptMethod(name: "Death Penalty", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43588"])]
    public async void DeathPenalty(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            await Task.Delay(4500);
            string msg = language == Language.Chinese ? "Esuna Doom" : "Esuna Doom";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
    }

    [ScriptMethod(name: "Hellmaker Mark", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:18642"])]
    public async void Hellmakermark(Event ev, ScriptAccessory sa)
    {
        Vector3 pos = ev.SourcePosition;
        Vector3 mypos = sa.Data.MyObject?.Position ?? Vector3.Zero;
        var distance = Vector3.Distance(pos, mypos);

        if (distance <= 18)
        {
            string msg = language == Language.Chinese ? "Attack Hellmaker" : "Attack Hellmaker";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
            if (isMark) sa.Method.MarkClear();
            if (isMark) sa.Method.Mark((uint)ev.SourceId, MarkType.Attack1, LocalMark);
        }
    }
    #endregion

    #region Boss3
    [ScriptMethod(name: "---- Boss3 ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void Boss3(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43825"], suppress: 5000)]
    public void boss3AOE(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Electray", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43810"])]
    public void Boss3Electray(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(8, 45), 4700, $"Electray-{ev.SourceId}", scalemode: ScaleMode.ByTime);
        sa.Method.RemoveDraw($"Bombardment");
    }

    [ScriptMethod(name: "Terror Spawn", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:18624"])]
    public void TerrorSpawn(Event ev, ScriptAccessory sa)
    {
        lock (CountLock)
        {
            if (!_terrorList.Contains(ev.SourceId))
            {
                _terrorList.Add(ev.SourceId);
                DebugMsg($"Terror spawned: {ev.SourceId}, total: {_terrorList.Count}", sa);
            }
        }
    }

    [ScriptMethod(name: "Terror Despawn", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:18624"])]
    public void TerrorDespawn(Event ev, ScriptAccessory sa)
    {
        _terrorList.Remove(ev.SourceId);
        DebugMsg($"Terror despawned: {ev.SourceId}, total: {_terrorList.Count}", sa);
    }

    public static bool AlmostEqual(float a, float b, float eps) => Math.Abs(a - b) <= eps;
    public static bool AlmostEqual(Vector3 a, Vector3 b, float eps) => Vector3.Distance(a, b) <= eps;

    [ScriptMethod(name: "Bombardment", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:regex:^(1870[5-8])$"])]
    public void Bombardment(Event ev, ScriptAccessory sa)
    {
        DebugMsg($"In Bombardment, terrorList count: {_terrorList.Count}", sa);
        if (ExtensionVisibleMethod.IsCharacterVisible((ICharacter)IbcHelper.GetByDataId(sa, 18623).FirstOrDefault())) return;
        DebugMsg($"No return", sa);
        if (_terrorList.Count > 6)
        {
            var count = _terrorList.Count;
            var pos = ev.SourcePosition;
            var big = false;

            for (int i = 0; i < count; i++)
            {
                var t = IbcHelper.GetById(sa, _terrorList[i]);
                if (t?.EntityId == ev.SourceId)
                {
                    continue;
                }
                if (t != null && AlmostEqual(t.Position, pos, 6f))
                {
                    sa.Log.Debug($"tid: {t.GameObjectId}, tpos: {t.Position}, pos: {pos}, visible : {ExtensionVisibleMethod.IsCharacterVisible((ICharacter)t)}");
                    if (!ExtensionVisibleMethod.IsCharacterVisible((ICharacter)t)) continue;
                    big = true;
                    sa.Log.Debug($"isbig: tid: {t.GameObjectId}, tpos: {t.Position}, pos: {pos}, visible : {ExtensionVisibleMethod.IsCharacterVisible((ICharacter)t)}");
                    break;
                }
            }

            DebugMsg($"In Bombardment, count: {count}, pos: {pos}, big: {big}", sa);

            Vector2 size = big ? new Vector2(14f) : new Vector2(3f);

            Vector3 position = big ? pos + 3.5f * ExtensionMethods.ToDirection(ExtensionMethods.Round(ev.SourceRotation, 1f)) : pos;

            var dp = sa.Data.GetDefaultDrawProperties();
            dp.Name = "Bombardment";
            dp.Color = sa.Data.DefaultDangerColor;
            dp.Position = ExtensionMethods.Quantized(position);
            dp.Rotation = float.Pi;
            dp.Scale = size;
            dp.DestoryAt = 9700;
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
        }
    }

    [ScriptMethod(name: "Impression", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43818"])]
    public void Impression(Event ev, ScriptAccessory sa)
    {
        var dp = sa.Data.GetDefaultDrawProperties();
        dp.Name = "Impression-knock";
        dp.Color = sa.Data.DefaultSafeColor;
        dp.Owner = sa.Data.Me;
        dp.TargetPosition = ev.EffectPosition;
        dp.Rotation = float.Pi;
        dp.Scale = new Vector2(2, 11f);
        dp.DestoryAt = 4700;
        if (isLead) sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(10f), 4700, "Impression-circle", color: new Vector4(1, 0, 0, ColorAlpha));
    }

    [ScriptMethod(name: "Memory of the Pyre", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43824"])]
    public void MemoryofthePyre(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Tankbuster" : "Tankbuster";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Keraunography", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43813"])]
    public void Keraunography(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20, 60), 3700, $"Keraunography-{ev.SourceId}");
    }

    [ScriptMethod(name: "Turmoil", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4381[45])$"])]
    public void Turmoil(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            // left
            case 43814:
                {
                    DrawHelper.DrawFanOwner(sa, ev.SourceId, (-float.Pi / 2), new Vector2(50f), 180, 4700, $"Turmoil-{ev.SourceId}", scaleByTime: false);
                    string msg = language == Language.Chinese ? "Go Right" : "Go Right";
                    if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
                    if (isTTS) sa.Method.EdgeTTS($"{msg}");
                    break;
                }
            // right
            case 43815:
                {
                    DrawHelper.DrawFanOwner(sa, ev.SourceId, (float.Pi / 2), new Vector2(50f), 180, 4700, $"Turmoil-{ev.SourceId}", scaleByTime: false);
                    string msg = language == Language.Chinese ? "Go Left" : "Go Left";
                    if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
                    if (isTTS) sa.Method.EdgeTTS($"{msg}");
                    break;
                }
        }
    }

    [ScriptMethod(name: "Memory of the Storm (Stack)", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(020D)$"])]
    public void MemoryoftheStorm(Event ev, ScriptAccessory sa)
    {
        string tname = ev["TargetName"]?.ToString() ?? "Unknown Target";
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Stack" : "Stack";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        } else
        {
            string msg = language == Language.Chinese ? $"Stack with {tname}" : $"Stack with {tname}";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }

        DrawHelper.DrawRectObjectTarget(sa, ev.SourceId, ev.TargetId, new Vector2(8, 40), 5000, "MemoryoftheStorm", color: sa.Data.DefaultSafeColor);
    }
    #endregion
}

#region Function Libraries
// (Helper classes remain the same as previous files)
#endregion
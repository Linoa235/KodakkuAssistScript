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
using KodaMarkType = KodakkuAssist.Module.GameOperate.MarkType;

namespace Veever.DawnTrail.Vault_Oneiron;

[ScriptType(name: Name, territorys: [1279], guid: "027ed2ae-4162-4d1e-a77d-a74d9441b065",
    version: Version, author: "Veever", note: NoteStr, updateInfo: UpdateInfo)]

// ^(?!.*((Monk|Machinist|Dragoon|Samurai|Ninja|Viper|Reaper|Dancer|Bard|Astrologian|Sage|Scholar|(Eos|Selene)|Seraph|White Mage|Warrior|Paladin|Dark Knight|Gunbreaker|Pictomancer|Black Mage|Blue Mage|Summoner|Carbuncle|Demigod Bahamut|Demigod Phoenix|Garuda-Egi|Titan-Egi|Ifrit-Egi|Puppet)\] (Used|Cast))).*35501.*$
// ^\[\w+\|[^|]+\|E\]\s\w+

public class Vault_Oneiron  
{
    const string NoteStr =
    """
    v0.0.0.2
    1. If you need a drawing for a mechanic or notice any issues, please contact me on Discord or send me a private message.
    Duckism
    ------------------------------
    1. If you need a draw for a mechanic or notice any issues, @ me on DC or DM me.
    Duckmen
    """;

    const string UpdateInfo =
    """
        v0.0.0.2
    """;

    private const string Name = "Vault Oneiron";
    private const string Version = "0.0.0.2";
    private const string DebugVersion = "a";

    private const bool Debugging = true;

    [UserSetting("Announcement language")]
    public Language language { get; set; } = Language.Chinese;

    [UserSetting("Drawing opacity — higher value = more visible")]
    public static float ColorAlpha { get; set; } = 1f;

    [UserSetting("Banner text toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS toggle")]
    public bool isTTS { get; set; } = true;

    [UserSetting("Auto anti-knockback")]
    public bool useAntiKnockback { get; set; } = true;

    //[UserSetting("Guide arrow toggle")]
    //public bool isLead { get; set; } = true;

    //[UserSetting("Target Marker toggle")]
    //public bool isMark { get; set; } = true;

    //[UserSetting("Local target marker toggle (ON = local only, OFF = party shared)")]
    //public bool LocalMark { get; set; } = true;

    //[UserSetting("Waymark guide toggle")]
    //public bool PostNamazuPrint { get; set; } = true;

    //[UserSetting("PostNamazu Port Setting")]
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

    private readonly object CountLock = new object();


    private Dictionary<uint, (ulong, Vector3)> tetherData = new Dictionary<uint, (ulong, Vector3)>();
    private Dictionary<uint, (ulong, Vector3, int)> Boss1QuadraRecordData = new Dictionary<uint, (ulong, Vector3, int)>();

    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    public void Init(ScriptAccessory sa)
    {
        sa.Log.Debug($"Script {Name} v{Version}{DebugVersion} initialized successfully.");
        sa.Method.RemoveDraw(".*");
        VioletBoltCount = 0;
    }


    #region Mobs
    [ScriptMethod(name: "-------- Mobs --------", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void Mobs(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "Head Trauma & Interject - Interrupt Destroy", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^75(38|51)$"], userControl: false)]
    public void destoryCancelAction(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($".*{ev.TargetId}");
    }

    [ScriptMethod(name: "Batter", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43602)$"])]
    public void Batter(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2700, $"Batter-{ev.SourceId}", color: new Vector4(1, 0, 0, ColorAlpha), scaleByTime: true);
    }

    [ScriptMethod(name: "Batter Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43602"], userControl: false)]
    public void ElectroswipeClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Batter-{ev.SourceId}");
    }

    [ScriptMethod(name: "Flood", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43605)$"])]
    public void Flood(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(8f), 2700, $"Flood-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Flood Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43605"], userControl: false)]
    public void FloodClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Flood-{ev.SourceId}");
    }

    [ScriptMethod(name: "Water III", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43606)$"])]
    public void WaterIII(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(8f), 3700, $"WaterIII-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Water III Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43606"], userControl: false)]
    public void WaterIIIClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"WaterIII-{ev.SourceId}");
    }

    //[ScriptMethod(name: "Protean Wave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43609)$"])]
    //public void ProteanWave(Event ev, ScriptAccessory sa)
    //{
    //    DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(39f), 30, 3700, $"ProteanWave-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    //}

    [ScriptMethod(name: "AOES", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43608|43633|43626|43616)$"])]
    public void MobsAOES(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 3000, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Tank Buster Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43607|43638|43615)$"])]
    public void MobsTankbuster(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Tank buster on you" : "Targeted Buster";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 3000, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
    }

    [ScriptMethod(name: "Brine Bomb", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43610)$"])]
    public void BrineBomb(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(5f), 2700, $"BrineBomb-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "BrineBomb Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43610"], userControl: false)]
    public void BrineBombClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"BrineBomb-{ev.SourceId}");
    }

    [ScriptMethod(name: "Bitter Chill", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43631)$"])]
    public void BitterChill(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2700, $"BitterChill-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Bitter Chill Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43631"], userControl: false)]
    public void BitterChillClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"BitterChill-{ev.SourceId}");
    }

    [ScriptMethod(name: "Tank AOE Buster Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43630|43623)$"])]
    public void TankBusterMobs(Event ev, ScriptAccessory sa)
    {
        string msg = "";
        if (ev.TargetId == sa.Data.Me)
        {
            msg = language == Language.Chinese ? "Line AOE tank buster, use mitigation" : "Tankbuster, Use mits";
        }
        else
        {
            msg = language == Language.Chinese ? "Move away from the AOE tank buster" : "AOE Tankbuster — Stay Away";
        }
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");


        DrawHelper.DrawRectObjectTarget(sa, ev.SourceId, ev.TargetId, new Vector2(8f, 60f), 4700, $"TankBuster-{ev.TargetId}", color: new Vector4(1, 0, 0, ColorAlpha));
    }

    [ScriptMethod(name: "Homing Shot", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43635)$"])]
    public void HomingShot(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2700, $"Homing Shot-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Homing Shot Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43635"], userControl: false)]
    public void HomingShotClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Homing Shot-{ev.SourceId}");
    }

    [ScriptMethod(name: "Thrown Flames", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43636)$"])]
    public void ThrownFlames(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(8f), 2700, $"ThrownFlames-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Thrown Flames Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43636"], userControl: false)]
    public void ThrownFlamesClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"ThrownFlames-{ev.SourceId}");
    }

    [ScriptMethod(name: "Entangle", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43621)$"])]
    public void Entangle(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(4f), 2700, $"Entangle-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Entangle Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43621"], userControl: false)]
    public void EntangleClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Entangle-{ev.SourceId}");
    }

    [ScriptMethod(name: "Gust", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43624)$"])]
    public void Gust(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2700, $"Gust-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Gust Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43624"], userControl: false)]
    public void GustClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Gust-{ev.SourceId}");
    }

    [ScriptMethod(name: "Whipwind", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43625)$"])]
    public void Whipwind(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(40f, 60f), 5700,
            $"Whipwind-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scalemode: ScaleMode.ByTime);
    }

    [ScriptMethod(name: "Whipwind Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43625"], userControl: false)]
    public void WhipwindClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Whipwind-{ev.SourceId}");
    }

    [ScriptMethod(name: "Molten Silk", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43613)$"])]
    public void MoltenSilk(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, float.Pi, new Vector2(9f), 270, 2700, $"Molten Silk-{ev.SourceId}",
            color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Molten Silk Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43613"], userControl: false)]
    public void MoltenSilkClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Molten Silk-{ev.SourceId}");
    }

    [ScriptMethod(name: "Flying Press", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43614)$"])]
    public void FlyingPress(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2700, $"Flying Press-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Flying Press Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43614"], userControl: false)]
    public void FlyingPressClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Flying Press-{ev.SourceId}");
    }

    [ScriptMethod(name: "Scoop", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43618)$"])]
    public void Scoop(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(13f), 120, 3700, $"Scoop-{ev.SourceId}",
            color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Scoop Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43618"], userControl: false)]
    public void ScoopClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Scoop-{ev.SourceId}");
    }

    [ScriptMethod(name: "Spin", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43619)$"])]
    public void Spin(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(11f), 3200, $"Spin-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Spin Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43619"], userControl: false)]
    public void SpinClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Spin-{ev.SourceId}");
    }

    [ScriptMethod(name: "Lance Swing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43726)$"])]
    public void LanceSwing(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(8f), 3700, $"Lance Swing-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Lance Swing Clear", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:43726"], userControl: false)]
    public void LanceSwingClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw($"Lance Swing-{ev.SourceId}");
    }

    #endregion

    #region The Vault Vegetators
    [ScriptMethod(name: "-------- The Vault Vegetators --------", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void vaultVegetators(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "AOES", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(32305|32303|32302|32304|32301)$"])]
    public void AOES(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(7f), 3200, $"UpliftSequence-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }
    #endregion

    #region Fastitocalon
    [ScriptMethod(name: "-------- Fastitocalon --------", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void Fastitocalon(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "Tremblor", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43664)$"])]
    public void Tremblor(Event ev, ScriptAccessory sa)
    {
        //string msg = language == Language.Chinese ? "Dodge between two rings" : "Dodge between two rings";
        //if (isText) sa.Method.TextInfo($"{msg}", duration: 4200, true);
        //if (isTTS) sa.Method.EdgeTTS($"{msg}");

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(10f), 3700, $"Tremblor-{ev.SourceId}", color: new Vector4(1, 0, 0, ColorAlpha), scaleByTime: true);
        DrawHelper.DrawDonutObject(sa, ev.TargetId, new Vector2(20f), new Vector2(10f), 3700, $"Tremblor1-{ev.SourceId}", delay: 3000, color: new Vector4(1, 0, 0, ColorAlpha), scaleByTime: false);
        DrawHelper.DrawDonutObject(sa, ev.TargetId, new Vector2(30f), new Vector2(20f), 3700, $"Tremblor2-{ev.SourceId}", delay: 6000, color: new Vector4(1, 0, 0, ColorAlpha), scaleByTime: false);

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(10f), 7000, $"TremblorSafe-{ev.SourceId}", delay: 3700, color: new Vector4(0, 1, 0, 0.5f), scaleByTime: false);
        DrawHelper.DrawDonutObject(sa, ev.TargetId, new Vector2(20f), new Vector2(10f), 3700, $"TremblorSafe1-{ev.SourceId}", delay: 6700, color: new Vector4(0, 1, 0, 0.5f), scaleByTime: false);
        DrawHelper.DrawDonutObject(sa, ev.TargetId, new Vector2(30f), new Vector2(20f), 1000, $"TremblorSafe2-{ev.SourceId}", delay: 9700, color: new Vector4(0, 1, 0, 0.5f), scaleByTime: false);
    }

    [ScriptMethod(name: "Uplift Sequence", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43659|43660|43661|43662)$"])]
    public void UpliftSequence(Event ev, ScriptAccessory sa)
    {
        // 43659    4700
        // 43660    6700
        // 43661    8700
        // 43662    10700
        var time = 4700;
        if (ev.ActionId == 43660) time = 6700;
        else if (ev.ActionId == 43661) time = 8700;
        else if (ev.ActionId == 43662) time = 10700;

        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), time, $"UpliftSequence-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }


    [ScriptMethod(name: "Tank Buster Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43668"])]
    public void FastitocalonTankbuster(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Tank buster on you" : "Targeted Buster";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        } else
        {
            string msg = language == Language.Chinese ? "Move away from the AOE tank buster" : "Avoid AOE tankbuster";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
    }

    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43672"])]
    public void FastitocalonAOE(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Earthshake", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43670)$"])]
    public void Earthshake(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2700, $"Earthshake-{ev.SourceId}", color: new Vector4(1, 1, 0, ColorAlpha), scaleByTime: true);
    }
    #endregion

    #region Great Gimme Cat
    [ScriptMethod(name: "-------- Great Gimme Cat --------", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void greatgimmecat(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "Preening Prism", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43687|43688)$"])]
    public void PreeningPrism(Event ev, ScriptAccessory sa)
    {
        // 43687 5700
        // 43688 7700
        var delay = 0;
        var time = 5700;
        if (ev.ActionId == 43688)
        {
            delay = 5700;
            time = 2000;
        }
        DrawHelper.DrawFanObject(sa, ev.SourceId, MathTools.DegToRad(0f), new Vector2(14f), 60, time,
            $"PreeningPrismOrigin-{ev.SourceId}", color: sa.Data.DefaultDangerColor, drawmode: DrawModeEnum.Imgui, scaleByTime: false, delay: delay);
        DrawHelper.DrawFanObject(sa, ev.SourceId, MathTools.DegToRad(112.5f), new Vector2(20f), 45, time, 
            $"PreeningPrismZoffset-{ev.SourceId}", color: sa.Data.DefaultDangerColor, offset: new Vector3(14.1f, 0, -14.1f), drawmode: DrawModeEnum.Imgui, scaleByTime: false, delay: delay);
        DrawHelper.DrawFanObject(sa, ev.SourceId, MathTools.DegToRad(-112.5f), new Vector2(20f), 45, time,
            $"PreeningPrismXoffset-{ev.SourceId}", color: sa.Data.DefaultDangerColor, offset: new Vector3(-14.1f, 0, -14.1f), drawmode: DrawModeEnum.Imgui, scaleByTime: false, delay: delay);
    }

    [ScriptMethod(name: "Basking Beam", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43690)$"])]
    public void BaskingBeam(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(5f, 40f), 2700,
            $"BaskingBeam-{ev.SourceId}", color: new Vector4(1, 1, 0, ColorAlpha), offset: new Vector3(0, 0, 20));
    }

    [ScriptMethod(name: "Tank Buster Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43698"])]
    public void GimmeTankbuster(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Tank buster on you" : "Targeted Buster";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
        else
        {
            string msg = language == Language.Chinese ? "Move away from the AOE tank buster" : "Avoid AOE tankbuster";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
    }

    [ScriptMethod(name: "Glitterbox", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43692)$"])]
    public void Glitterbox(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(5f), 2700, $"Glitterbox-{ev.SourceId}", color: new Vector4(1, 0, 0, ColorAlpha), scaleByTime: true);
    }

    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43672"])]
    public void GimmeAOE(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    #endregion

    #region Old Bitter-eye

    [ScriptMethod(name: "-------- Old Bitter-eye --------", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void OldBitterEye(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "10-stone Swing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43675)$"])]
    public void TenStoneSwing(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(11f), 3700, $"10-stone Swing-{ev.SourceId}", 
            color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "100-stone Swipe", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43678)$"])]
    public void OoStoneSwipe(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawFanObject(sa, ev.SourceId, 0, new Vector2(40f), 60, 3700, $"100-stone Swipe-{ev.SourceId}",
            color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Laughing Leap", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43683)$"])]
    public void LaughingLeap(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(10f), 3700, $"Laughing Leap-{ev.SourceId}",
            color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Eye of the Thunderstorm", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43680)$"])]
    public void EyeoftheThunderstorm(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Move in (Dynamo)" : "Move in (Dynamo)";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 3500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(5f), 3700, $"Eye of the Thunderstorm Safe-{ev.SourceId}",
            color: sa.Data.DefaultSafeColor, scaleByTime: false);

        DrawHelper.DrawDonutObject(sa, ev.SourceId, new Vector2(40f), new Vector2(5f), 3700, $"Eye of the Thunderstorm Danger-{ev.SourceId}",
            color: sa.Data.DefaultDangerColor, scaleByTime: false);
    }

    [ScriptMethod(name: "1000-stone Swing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43677)$"])]
    public void OoostoneSwing(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Interrupt the boss" : "Interrupt!";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 7700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "100-stone Swing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43676)$"])]
    public void OostoneSwing(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(13f), 1200, $"100-stone Swing-{ev.SourceId}",
            color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Predatorial Instinct Anti-KnockBack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43684)$"])]
    public void PredatorialInstinct(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "Move away from the boss" : "Away from Boss";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");

        DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(13f), 7700, $"100-stone Swing-{ev.SourceId}",
            color: sa.Data.DefaultDangerColor, scaleByTime: true);

        if (useAntiKnockback)
        {
            sa.Method.UseAction(sa.Data.Me, 7559);
            sa.Method.UseAction(sa.Data.Me, 7548);
        } 
    }

    [ScriptMethod(name: "Tank AOE Buster Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43682)$"])]
    public void OldBitterEyeTankBusterMobs(Event ev, ScriptAccessory sa)
    {
        string msg = "";
        if (ev.TargetId == sa.Data.Me)
        {
            msg = language == Language.Chinese ? "Line AOE tank buster, use mitigation" : "Tankbuster, Use mits";
        }
        else
        {
            msg = language == Language.Chinese ? "Move away from the AOE tank buster" : "AOE Tankbuster — Stay Away";
        }
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");


        DrawHelper.DrawRectObjectTarget(sa, ev.SourceId, ev.TargetId, new Vector2(8f, 65f), 4700, $"TankBuster-{ev.TargetId}", color: new Vector4(1, 0, 0, ColorAlpha));
    }
    #endregion

    #region Paeonia
    [ScriptMethod(name: "-------- Paeonia --------", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void paeonia(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "Lightning Bolt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43704)$"])]
    public void LightningBolt(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2700, $"Lightning Bolt-{ev.SourceId}",
            color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    [ScriptMethod(name: "Hypnotize", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43702)$"])]
    public async void Hypnotize(Event ev, ScriptAccessory sa)
    {
        await Task.Delay(4000);
        string msg = language == Language.Chinese ? "Face away from the boss" : "Face Away from Boss";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 3000, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Levinroot Ring", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43713|43710)$"])]
    public void LevinrootRing(Event ev, ScriptAccessory sa)
    {
        if (ev.ActionId == 43713)
        {
            string msg = language == Language.Chinese ? "First move in, then move out" : "Dynamo then Chariot";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");

            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(10f), 4700, $"Levinroot Ring Safe-{ev.SourceId}",
                color: sa.Data.DefaultSafeColor, scaleByTime: false);
            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(10f), 3000, $"Levinroot Ring Danger-{ev.SourceId}",
                color: sa.Data.DefaultDangerColor, scaleByTime: false, delay: 4700);
        } else
        {
            string msg = language == Language.Chinese ? "First move out, then move in" : "Chariot then Dynamo";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4700, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");

            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(10f), 4700, $"Levinroot Ring Danger-{ev.SourceId}",
                color: sa.Data.DefaultDangerColor, scaleByTime: false);

            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(10f), 3000, $"Levinroot Ring Safe-{ev.SourceId}",
                color: sa.Data.DefaultSafeColor, scaleByTime: false, delay: 4700);

        }
    }

    [ScriptMethod(name: "Lightning Crossing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43706|43707)$"])]
    public void LightningCrossing(Event ev, ScriptAccessory sa)
    {
        // 43707 6700
        // 43706 4700
        if (ev.ActionId == 43706)
        {
            DrawHelper.DrawFanObjectNoRot(sa, ev.SourceId, new Vector2(40f), 45, 4700,
                $"Lightning Crossing Danger-{ev.SourceId}", color: sa.Data.DefaultDangerColor);

            DrawHelper.DrawFanObjectNoRot(sa, ev.SourceId, new Vector2(40f), 45, 2000,
                $"Lightning Crossing Safe-{ev.SourceId}", color: sa.Data.DefaultSafeColor, delay: 4700);
        }
        else
        {
            DrawHelper.DrawFanObjectNoRot(sa, ev.SourceId, new Vector2(40f), 45, 4700,
                $"Lightning Crossing Safe-{ev.SourceId}", color: sa.Data.DefaultSafeColor);

            DrawHelper.DrawFanObjectNoRot(sa, ev.SourceId, new Vector2(40f), 45, 2000,
                $"Lightning Crossing Danger-{ev.SourceId}", color: sa.Data.DefaultDangerColor, delay: 4700);
        }
    }

    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43715"])]
    public void paeoniaAOE(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Tank Buster Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43716"])]
    public void paeoniaTankbuster(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Tank buster on you" : "Targeted Buster";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
    }
    #endregion

    #region Gwyddneu

    [ScriptMethod(name: "-------- Gwyddneu --------", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void Gwyddneu(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43648|43654)$"])]
    public void GwyddneuAOE(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Lightning Bolt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43652)$"])]
    public void GwyddneuLightningBolt(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2700, $"Lightning Bolt-{ev.SourceId}",
            color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }

    public int VioletBoltCount = 0;
    [ScriptMethod(name: "Violet Bolt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43645)$"])]
    public async void VioletBolt(Event ev, ScriptAccessory sa)
    {
        var delay = 0;
        var duration = 4700;
        lock (CountLock)
        {
            if (VioletBoltCount > 3)
            {
                delay = 2700;
                duration = 2000;
            }
            DrawHelper.DrawFanObjectNoRot(sa, ev.SourceId, new Vector2(70f), 45, duration, $"VioletBolt-{ev.SourceId}",
                color: sa.Data.DefaultDangerColor, delay: delay);
            sa.Log.Debug($"VioletBolt Count: {VioletBoltCount}");
            VioletBoltCount++;
        }

        await Task.Delay(10000);
        VioletBoltCount = 0;

    }

    [ScriptMethod(name: "Shock", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(18599|18598)$"])]
    public void Shock(Event ev, ScriptAccessory sa)
    {
        // 18598 small
        // 18599 big
        sa.Log.Debug($"Shock DataId: {ev.DataId()}");
        switch (ev.DataId())
        {
            case 18598:
                DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(8f), 120000, $"Shock-{ev.SourceId}",
                    color: sa.Data.DefaultDangerColor, scaleByTime: false);
                break;
            case 18599:
                DrawHelper.DrawCircleObject(sa, ev.SourceId, new Vector2(16f), 120000, $"Shock-{ev.SourceId}",
                    color: sa.Data.DefaultDangerColor, scaleByTime: false);
                break;
        }

    }

    [ScriptMethod(name: "Shock Remove", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43655|43656)$"])]
    public async void ShockRemove(Event ev, ScriptAccessory sa)
    {
        await Task.Delay(2700);
        sa.Method.RemoveDraw($"Shock-{ev.SourceId}");
    }

    [ScriptMethod(name: "Tank Buster Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43650"])]
    public void GwyddneuTankbuster(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Tank buster on you" : "Targeted Buster";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
        else
        {
            string msg = language == Language.Chinese ? "Move away from the AOE tank buster" : "Avoid AOE tankbuster";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
    }

    [ScriptMethod(name: "Gwyddneu Clear", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:regex:^(9020)$"], userControl: false)]
    public void GwyddneuClear(Event ev, ScriptAccessory sa)
    {
        sa.Method.RemoveDraw(".*");
    }

    #endregion

    #region Gilded Sentry
    [ScriptMethod(name: "-------- Gilded Sentry --------", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void gildedSentry(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "Cross Lightning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43719)$"])]
    public void CrossLightning(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(10f, 100f), 4700,
            $"Cross Lightning-{ev.SourceId}", color: new Vector4(1, 0, 1, ColorAlpha), offset: new Vector3(0, 0, 50));
            
        DrawHelper.DrawRectObjectNoTargetWithRot(sa, ev.SourceId, new Vector2(10f, 100f), float.Pi / 2, 4700,
            $"Cross Lightning-{ev.SourceId}", color: new Vector4(1, 0, 1, ColorAlpha), offset: new Vector3(50, 0, 0));
    }

    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43723)$"])]
    public void gildedSentryAOE(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "Alexandrian Thunder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43721)$"])]
    public void AlexandrianThunder(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(6f), 2700, $"Alexandrian Thunder-{ev.SourceId}", color: sa.Data.DefaultDangerColor, scaleByTime: true);
    }


    #endregion

    #region Spirit of Thunder
    [ScriptMethod(name: "-------- Spirit of Thunder --------", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld:asdf"],
    userControl: Debugging)]
    public void SpiritofThunder(Event ev, ScriptAccessory sa)
    {
    }

    [ScriptMethod(name: "Electray", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43773|43774)$"])]
    public void Electray(Event ev, ScriptAccessory sa)
    {
        // 43774 7700
        // 43773 4700
        // V2(10, 20)
        var delay = 0;
        var duration = 4700;
        if (ev.ActionId == 43774)
        {
            delay = 5000;
            duration = 2700;
        }
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(10f, 20f), duration,
            $"Cross Lightning-{ev.SourceId}", color: new Vector4(1, 0, 1, ColorAlpha), delay: delay);

        if (ev.ActionId == 43773)
        {
            DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(10f, 20f), 2700,
                $"Cross Lightning-{ev.SourceId}", color: sa.Data.DefaultSafeColor, delay: 4700);
        }
    }

    [ScriptMethod(name: "Tank Buster Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:43784"])]
    public void SpiritofThunderTankbuster(Event ev, ScriptAccessory sa)
    {
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "Tank buster on you" : "Targeted Buster";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
        else
        {
            string msg = language == Language.Chinese ? "Move away from the AOE tank buster" : "Avoid AOE tankbuster";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
    }

    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43785)$"])]
    public void SpiritofThunderAOE(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4200, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    [ScriptMethod(name: "High-voltage Electray", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43778|43779|43780|43781)$"])]
    public void HighVoltageElectray(Event ev, ScriptAccessory sa)
    {
        // 43778 4700
        // 43779 5700
        // 43780 6700
        // 43781 7700
        switch (ev.ActionId)
        {
            case 43778:
                DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20f, 20f), 4700,
                    $"High-voltage Electray 0-{ev.SourceId}", color: new Vector4(1, 0, 1, ColorAlpha), delay: 0, offset: new Vector3(0, 0, 10));

                DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20f, 20f), 3000,
                    $"High-voltage Electray Safe 0-{ev.SourceId}", color: sa.Data.DefaultSafeColor, delay: 4700, offset: new Vector3(0, 0, 10));
                break;
            case 43779:
                DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20f, 20f), 1000,
                    $"High-voltage Electray 1-{ev.SourceId}", color: new Vector4(1, 0, 1, ColorAlpha), delay: 4700, offset: new Vector3(0, 0, 10));

                DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20f, 20f), 2000,
                    $"High-voltage Electray Safe 1-{ev.SourceId}", color: sa.Data.DefaultSafeColor, delay: 5700, offset: new Vector3(0, 0, 10));
                break;
            case 43780:
                DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20f, 20f), 1000,
                    $"High-voltage Electray 2-{ev.SourceId}", color: new Vector4(1, 0, 1, ColorAlpha), delay: 5700, offset: new Vector3(0, 0, 10));

                DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20f, 20f), 1000,
                    $"High-voltage Electray Safe 2-{ev.SourceId}", color: sa.Data.DefaultSafeColor, delay: 6700, offset: new Vector3(0, 0, 10));
                break;
            case 43781:
                DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20f, 20f), 1000,
                    $"High-voltage Electray 3-{ev.SourceId}", color: new Vector4(1, 0, 1, ColorAlpha), delay: 6700, offset: new Vector3(0, 0, 10));
                break;
        }
    }

    [ScriptMethod(name: "Power Line", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(18614)$"])]
    public void PowerLine(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(10f, 40f), 120000,
            $"Power Line-{ev.SourceId}", color: new Vector4(1, 0, 1, ColorAlpha));
    }

    [ScriptMethod(name: "PowerLine Remove", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43788)$"])]
    public async void PowerLineRemove(Event ev, ScriptAccessory sa)
    {
        await Task.Delay(1000);
        sa.Method.RemoveDraw($"Power Line-{ev.SourceId}");
    }

    #endregion
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

    /// <summary>
    /// Get the object's Icon ID above its head
    /// </summary>
    public static uint GetObjectIconId(IGameObject? obj)
    {
        if (obj == null || !obj.IsValid()) return 0;

        unsafe
        {
            var gameObj = (GameObject*)obj.Address;
            if (gameObj == null) return 0;

            // Directly read the marker/Icon ID above the head
            return gameObj->NamePlateIconId;
        }
    }
    
    /// <summary>
    /// Check if the object has a specific Icon ID
    /// </summary>
    public static bool HasIconId(IGameObject? obj, int iconId)
    {
        return GetObjectIconId(obj) == iconId;
    }

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

    public static void ScaleModify(this ScriptAccessory sa, IGameObject? obj, float scale)
    {
        if (obj == null || !obj.IsValid())
        {
            sa.Log.Error($"Provided IGameObject is invalid.");
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
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
using Lumina.Data.Parsing.Layer;
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
using System.Net.Http;
using static FFXIVClientStructs.FFXIV.Client.Game.UI.Telepo.Delegates;
using KodaMarkType = KodakkuAssist.Module.GameOperate.MarkType;

namespace Veever.DawnTrail.TheWindwardWilds;

[ScriptType(name: Name, territorys: [1300], guid: "89c903db-488d-442d-9538-620460ef3966",
    version: Version, author: "Veever", note: NoteStr, updateInfo: UpdateInfo)]

// ^(?!.*((Monk|Machinist|Dragoon|Samurai|Ninja|Viper|Reaper|Dancer|Bard|Astrologian|Sage|Scholar|(Eos|Selene)|Seraph|White Mage|Warrior|Paladin|Dark Knight|Gunbreaker|Pictomancer|Black Mage|Blue Mage|Summoner|Carbuncle|Demigod Bahamut|Demigod Phoenix|Garuda-Egi|Titan-Egi|Ifrit-Egi|Automaton Queen)\] (Used|Cast))).*35501.*$
// ^\[\w+\|[^|]+\|E\]\s\w+

public class TheWindwardWilds
{
    const string NoteStr =
    """
    v0.0.0.4
    1. If you need a draw for a mechanic or notice any issues, @ me on DC or DM me.
    2. Cleaned up a few TTS after the drawings. If you need any of them, let me know and I’ll restore them.
    3. Boss Model Scale and VFX Scale can be adjusted in User Settings, 1 is the Game default value.
    Duckmen.
    ----------------------------------
    1. Si necesita un dibujo para una mecánica o nota algún problema, @ me en DC o envíame un MD.
    2. Limpié un par de TTS después de los dibujos. Si necesita alguno, hágamelo saber y lo restauraré.
    3. La escala del modelo del jefe y la escala de VFX se pueden ajustar en la Configuración de Usuario. 1 es el valor predeterminado del juego.
    Duckmen.
    """;

    const string UpdateStr =
    """
    v0.0.0.4
    Adapted for the new patch
    Duckmen.
    ----------------------------------
    Adaptado para el nuevo parche
    Duckmen.
    """;

    private const string Name = "LV.100 Guardian Arkveld Hunting [The Windward Wilds]";
    private const string Version = "0.0.0.4";
    private const string DebugVersion = "a";
    private const string UpdateInfo = UpdateStr;

    private const bool Debugging = false;

    private static readonly List<string> Role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
    private static readonly Vector3 Center = new Vector3(100, 0, 100);

    [UserSetting("Announce Language")]
    public Language language { get; set; } = Language.Chinese;

    [UserSetting("Draw Opacity — higher value = more visible")]
    public static float ColorAlpha { get; set; } = 1f;

    [UserSetting("Boss Model Scale")]
    public static float BossModelScale { get; set; } = 1f;

    [UserSetting("Boss VFX Scale")]
    public static float BossVFXScale { get; set; } = 1f;

    [UserSetting("Banner Text Toggle")]
    public bool isText { get; set; } = true;

    [UserSetting("TTS Toggle")]
    public bool isTTS { get; set; } = true;

    //[UserSetting("Auto Anti-knockback")]
    //public bool useaction { get; set; } = true;

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


    private enum GuardianArkveldPhase
    {
        Phase1,
        Phase2,
    }

    private static GuardianArkveldPhase guardianArkveldPhase = GuardianArkveldPhase.Phase1;
    public uint BossDataId = 18658;
    public int ChainbladeBlowTripleCount = 0;


    public void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!isDebug) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    public void Init(ScriptAccessory sa)
    {
        guardianArkveldPhase = GuardianArkveldPhase.Phase1;
        sa.Log.Debug($"Script {Name} v{Version}{DebugVersion} initialized.");
        sa.Method.RemoveDraw(".*");
        ChainbladeBlowTripleCount = 0;

        SpecialFunction.SetModelScale(sa, BossDataId, BossModelScale, BossVFXScale);

        _ = ScriptVersionChecker.CheckVersionAsync(
            sa,
            "89c903db-488d-442d-9538-620460ef3966",
            Version,
            showNotification: true
        );
    }


    [ScriptMethod(name: "AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43886|45201)$"],
        userControl: true)]
    public void AOENotify(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "AOE" : "AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
        ChainbladeBlowTripleCount = 0;
    }

    #region Phase 1
    [ScriptMethod(name: "P1 Chainblade Blow", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43833|43832)$"])]
    public void P1ChainbladeBlow(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            // 43833 boss right
            // 43832 boss left
            case 43833:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(28f, 80f), 6899, $"Chainblade Blow right-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(7f, 0, 20f));
                    break;
                }
            case 43832:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(28f, 80f), 6899, $"Chainblade Blow left-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(-7f, 0, 20f));
                    break;
                }
        }
    }

    [ScriptMethod(name: "P1 Wyvern's Siegeflight", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43838|43839)$"])]
    public void P1WyvernsSiegeflight(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            // 43838 small
            // 43839 split
            case 43838:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(8f, 40f), 6200, $"Wyvern's Siegeflight mid-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha));
                    break;
                }
            case 43839:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20f, 40f), 3400, $"Wyvern's Siegeflight Left-{ev.SourceId}",
                        sa.Data.DefaultDangerColor, offset: new Vector3(12f, 0, 0f), delay: 6600);
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(20f, 40f), 3400, $"Wyvern's Siegeflight Right-{ev.SourceId}",
                        sa.Data.DefaultDangerColor, offset: new Vector3(-12f, 0, 0f), delay: 6600);
                    break;
                }
        }
    }

    [ScriptMethod(name: "P1 Guardian Siegeflight", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43835|43836)$"])]
    public void P1GuardianSiegeflight(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            // 43835 small
            // 43836 large
            case 43835:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(8f, 40f), 6200, $"Guardian Siegeflight mid-{ev.SourceId}", new Vector4(0, 1, 1, ColorAlpha));
                    break;
                }
            case 43836:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(16f, 40f), 3500, $"Guardian Siegeflight mid-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha), delay: 6500);
                    break;
                }
        }
    }


    [ScriptMethod(name: "Line AoEs [Wyvern's Siegeflight]", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43877)$"])]
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

    [ScriptMethod(name: "Chainblade Blow 3-hit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43844|43845|43846|43847)$"])]
    public async void ChainbladeBlowTriple(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            case 43844:
                {
                    if (ChainbladeBlowTripleCount == 0)
                    {
                        string msg = language == Language.Chinese ? "前往第三个钢铁边缘，随后进入躲避" : "To Third Chariot edge, then in";
                        if (isText) sa.Method.TextInfo($"{msg}", duration: 5700, true);
                        if (isTTS) sa.Method.EdgeTTS($"{msg}");
                    }

                    if (ChainbladeBlowTripleCount != 2)
                    {
                        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(8f), 7200, $"Chainblade Blow Circle-{ev.SourceId}", sa.Data.DefaultDangerColor);
                    } else
                    {
                        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(8f), 7200, $"Chainblade Blow Circle-{ev.SourceId}", new Vector4(1, 1, 0, ColorAlpha));

                        await Task.Delay(10000);
                        ChainbladeBlowTripleCount = 0;
                    }

                    ChainbladeBlowTripleCount++;
                    break;
                }
            case 43845:
                {
                    DrawHelper.DrawDount(sa, ev.EffectPosition, new Vector2(14f), new Vector2(8f), 2000,
                        $"Chainblade Blow Dount-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: 7200);
                    break;
                }
            case 43846:
                {
                    DrawHelper.DrawDount(sa, ev.EffectPosition, new Vector2(20f), new Vector2(14f), 2000,
                        $"Chainblade Blow Dount-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: 9200);
                    break;
                }
            case 43847:
            {
                DrawHelper.DrawDount(sa, ev.EffectPosition, new Vector2(26f), new Vector2(20f), 2000,
                    $"Chainblade Blow Dount-{ev.SourceId}", sa.Data.DefaultDangerColor, scaleByTime: false, delay: 11200);
                break;
            }
        }
    }


    [ScriptMethod(name: "Stack", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(0064)$"])]
    public void Stack(Event ev, ScriptAccessory sa)
    {
        string tname = ev["TargetName"]?.ToString() ?? "Unknown Target";
        if (ev.TargetId == sa.Data.Me)
        {
            string msg = language == Language.Chinese ? "分摊点名" : "Stack marker";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 5700, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }
        else
        {
            string msg = language == Language.Chinese ? $"与{tname}分摊" : $"Stack with {tname}";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 5700, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        }

        DrawHelper.DrawCircleObject(sa, ev.TargetId, new Vector2(4.5f), 6000, "Stack", color: sa.Data.DefaultSafeColor);
    }


    [ScriptMethod(name: "P1 Wyvern's Ouroblade", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43849|43851)$"])]
    public void P1WyvernsOuroblade(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            // 43849 left
            // 43851 right
            case 43849:
                {
                    DrawHelper.DrawFanObject(sa, ev.SourceId, float.Pi / 2, new Vector2(40f), 180, 6700, $"Wyvern's Ouroblade left-{ev.SourceId}", sa.Data.DefaultDangerColor,
                        scaleByTime: false);
                    break;
                }
            case 43851:
                {
                    DrawHelper.DrawFanObject(sa, ev.SourceId, -float.Pi / 2, new Vector2(40f), 180, 6700, $"Wyvern's Ouroblade right-{ev.SourceId}", sa.Data.DefaultDangerColor,
                        scaleByTime: false);
                    break;
                }
        }
    }

    [ScriptMethod(name: "Guardian Resonance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43853)$"], suppress: 5000)]
    public void GuardianResonance(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? $"集合 → 放圈 → 踩塔" : $"Stack → drop circles → soak towers";
        string msg1 = language == Language.Chinese ? $"集合放圈后踩塔" : "Stack and drop circles then soak towers";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 5000, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg1}");
    }

    [ScriptMethod(name: "Guardian Resonance Tank Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43854)$"])]
    public void GuardianResonanceTank(Event ev, ScriptAccessory sa)
    {
        var myobj = sa.Data.MyObject;
        if (myobj == null) return;

        if (IbcHelper.GetPlayerRole(sa, myobj) == "Tank")
        {
            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(4f), 13000, $"Guardian Resonance Tank-{ev.SourceId}",
                sa.Data.DefaultSafeColor, scaleByTime: false);
        }
        else
        {
            DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(4f), 13000, $"Guardian Resonance Tank-{ev.SourceId}",
                new Vector4(1, 0, 0, ColorAlpha), scaleByTime: false);
        }
    }

    [ScriptMethod(name: "Wyvern's Vengeance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43860)$"])]
    public void WyvernsVengeance(Event ev, ScriptAccessory sa)
    {
        var effectPos = ev.EffectPosition;
        var srot = ev.SourceRotation;

        float[] distances = { 0f, 8f, 16f };
        int[] delays = { 0, 4700, 6500 };
        int[] durations = { 4700, 1800, 1800 };

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

    [ScriptMethod(name: "Wyvern's Radiance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44808)$"])]
    public void WyvernsRadiance(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawCircle(sa, ev.EffectPosition, new Vector2(12f), 1700, $"Wyvern's Radiance-{ev.SourceId}",
            new Vector4(1, 0, 0, ColorAlpha), scaleByTime: true);
    }
    #endregion

    [ScriptMethod(name: "Phase Change AOE Notify", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43870)$"],
    userControl: true)]
    public void PCAOENotify(Event ev, ScriptAccessory sa)
    {
        string msg = language == Language.Chinese ? "三连AOE" : "Triple AOE";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
        guardianArkveldPhase = GuardianArkveldPhase.Phase2;
    }

    #region Phase 2
    [ScriptMethod(name: "Wyvern's Weal", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45048|45049)$"])]
    public void WyvernsWeal(Event ev, ScriptAccessory sa)
    {
        var spos = ev.SourcePosition;
        var srot = ev.SourceRotation;
        
        switch (ev.ActionId)
        {
            case 45048:
                DrawHelper.DrawFanObject(sa, ev.SourceId, MathTools.DegToRad(-60), new Vector2(50f), 110, 7700, $"Wyvern's Weal clockwise-{ev.SourceId}", sa.Data.DefaultDangerColor,
                    scaleByTime: false);
                break;
            case 45049:
                DrawHelper.DrawFanObject(sa, ev.SourceId, MathTools.DegToRad(60), new Vector2(50f), 110, 7700, $"Wyvern's Weal clockwise-{ev.SourceId}", sa.Data.DefaultDangerColor,
                    scaleByTime: false);
                break;
        }

        float forwardDist = 9f;
        float Dist = 4.5f;
        float worldX;
        float worldZ;    
        worldX = spos.X + forwardDist * MathF.Sin(srot) - Dist * MathF.Cos(srot);
        worldZ = spos.Z + forwardDist * MathF.Cos(srot) + Dist * MathF.Sin(srot);

        Vector3 antiClockwisePos = new Vector3(worldX, spos.Y, worldZ);
        worldX = spos.X + forwardDist * MathF.Sin(srot) + Dist * MathF.Cos(srot);
        worldZ = spos.Z + forwardDist * MathF.Cos(srot) - Dist * MathF.Sin(srot);
        Vector3 clockwisePos = new Vector3(worldX, spos.Y, worldZ);
         
        if (isLead)
        {
            string msg = language == Language.Chinese ? "前往安全区域" : "Move to Safe Place";
            if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
            if (isTTS) sa.Method.EdgeTTS($"{msg}");
        } else
        {
            if (ev.ActionId == 45049)
            {
                string msg = language == Language.Chinese ? "前往左上角躲避" : "Move to TL corner";
                string msg1 = language == Language.Chinese ? "前往左上角躲避" : "Move to Top Left corner";
                if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
                if (isTTS) sa.Method.EdgeTTS($"{msg1}");
            } else
            {
                string msg = language == Language.Chinese ? "前往右上角躲避" : "Move to TR corner";
                string msg1 = language == Language.Chinese ? "前往左上角躲避" : "Move to Top right corner";
                if (isText) sa.Method.TextInfo($"{msg}", duration: 4500, true);
                if (isTTS) sa.Method.EdgeTTS($"{msg1}");
            }

        }

        switch (ev.ActionId)
        {
            case 45049:
                if (isLead) DrawHelper.DrawDisplacement(sa, antiClockwisePos, new Vector2(2f), 6000, "Wyvern's Weal Displacement");
                break;
            case 45048:
                if (isLead) DrawHelper.DrawDisplacement(sa, clockwisePos, new Vector2(2f), 6000, "Wyvern's Weal Displacement");
                break;
        }
    }

    [ScriptMethod(name: "P2 Guardian Siegeflight", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45069|45070)$"],
        userControl: true)]
    public void P2GuardianSiegeflight(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            // 45069 small
            // 45070 large
            case 45069:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(8f, 40f), 6200, $"Guardian Siegeflight mid-{ev.SourceId}", new Vector4(0, 1, 1, ColorAlpha));
                    break;
                }
            case 45070:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(16f, 40f), 3500, $"Guardian Siegeflight mid-{ev.SourceId}", new Vector4(1, 0, 0, ColorAlpha), delay: 6500);
                    break;
                }
        }
    }

    [ScriptMethod(name: "P2 Chainblade Blow", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45059|45056)$"])]
    public void P2ChainbladeBlow(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            // 45059 boss right
            // 45056 boss left
            case 45059:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(28f, 80f), 6899, $"Chainblade Blow right-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(7f, 0, 20f));
                    break;
                }
            case 45056:
                {
                    DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(28f, 80f), 6899, $"Chainblade Blow left-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(-7f, 0, 20f));
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

    [ScriptMethod(name: "P2 Wyvern's Ouroblade", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45061|45063)$"])]
    public void P2WyvernsOuroblade(Event ev, ScriptAccessory sa)
    {
        switch (ev.ActionId)
        {
            // 45061 left
            // 45063 right
            case 45061:
                {
                    DrawHelper.DrawFanObject(sa, ev.SourceId, float.Pi / 2, new Vector2(40f), 180, 5700, $"Wyvern's Ouroblade left-{ev.SourceId}", sa.Data.DefaultDangerColor,
                        scaleByTime: false);
                    break;
                }
            case 45063:
                {
                    DrawHelper.DrawFanObject(sa, ev.SourceId, -float.Pi / 2, new Vector2(40f), 180, 5700, $"Wyvern's Ouroblade right-{ev.SourceId}", sa.Data.DefaultDangerColor,
                        scaleByTime: false);
                    break;
                }
        }
    }

    [ScriptMethod(name: "Steeltail Thrust", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(44804)$"])]
    public void SteeltailThrust(Event ev, ScriptAccessory sa)
    {
        DrawHelper.DrawRectObjectNoTargetWithRot(sa, ev.SourceId, new Vector2(6f, 60f), float.Pi, 3300, $"Steeltail Thrust-{ev.SourceId}", sa.Data.DefaultDangerColor);
        string msg = language == Language.Chinese ? "远离背后" : "Avoid behind the boss";
        if (isText) sa.Method.TextInfo($"{msg}", duration: 3000, true);
        if (isTTS) sa.Method.EdgeTTS($"{msg}");
    }

    public Vector3 WrathfulRattlePos = new Vector3(0, 0, 0);
    public float WrathfulRattleRot = 0f;
    [ScriptMethod(name: "Wrathful Rattle", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43880)$"])]
    public void WrathfulRattle(Event ev, ScriptAccessory sa)
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

    [ScriptMethod(name: "Wrathful Rattle Return", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(43882)$"])]
    public void WrathfulRattleReturn(Event ev, ScriptAccessory sa)
    {
        var Epos = new Vector3(118.00f, 0.00f, 100.00f);
        var Wpos = new Vector3(82.00f, 0.00f, 100.00f);
        var spos = ev.SourcePosition;
        var srot = ev.SourceRotation;

        int[] delays =    { 2800, 4500, 7000, 9500, 12000, 15000, 17500, 20000, 22500 };
        int[] durations = { 1700, 1700, 2500, 2500, 3000, 2500, 2500, 2500, 2500 };


        DrawHelper.DrawRectObjectNoTarget(sa, ev.SourceId, new Vector2(8f, 40f), 2800, $"Wrathful Rattle main-{ev.SourceId}", sa.Data.DefaultDangerColor, offset: new Vector3(0, 0, 20));

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
                DebugMsg($"Pos: {Pos}, srot: {srot}, MathF.Cos(srot) {MathF.Cos(srot)}", sa);
            }
        }
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
        }
    }

    [ScriptMethod(name: "Unit - Remove & Refresh ALL", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:73"],
    userControl: Debugging)]
    public void Uninit(Event ev, ScriptAccessory sa)
    {
        if (isDebug) sa.Log.Debug($"Uninit Triggered");
        sa.Method.RemoveDraw(".*");

        sa.Method.ClearFrameworkUpdateAction(this);
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


    public static unsafe ulong GetMarkerEntityId(uint markerIndex)
    {
        var markingController = MarkingController.Instance();
        if (markingController == null) return 0;
        if (markerIndex >= 17) return 0;

        return markingController->Markers[(int)markerIndex];
    }

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

    public static bool HasMarker(IGameObject? obj, MarkType markType)
    {
        return GetObjectMarker(obj) == markType;
    }

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
            _ => "No Marker"
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

    public static void MarkPlayerByIdx(this ScriptAccessory sa, int idx, KodaMarkType marker,
        bool enable = true, bool local = false, bool localString = false)
    {
        if (!enable) return;
        if (localString)
            sa.Log.Debug($"[Local Character Simulation] Marking {idx}({sa.GetPlayerJobByIndex(idx)}) with {marker}.");
        else
            sa.Method.Mark(sa.Data.PartyList[idx], marker, local);
    }

    public static void MarkPlayerById(ScriptAccessory sa, uint id, KodaMarkType marker,
        bool enable = true, bool local = false, bool localString = false)
    {
        if (!enable) return;
        if (localString)
            sa.Log.Debug($"[Local Character Simulation] Marking {sa.GetPlayerIdIndex(id)}({sa.GetPlayerJobById(id)}) with {marker}.");
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


#region Script Version Check
public static class ScriptVersionChecker
{
    private const string OnlineRepoUrl = "https://raw.githubusercontent.com/VeeverSW/Kodakku-Script/refs/heads/main/OnlineRepo.json";
    private static readonly HttpClient _httpClient = new HttpClient();

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

    public enum VersionCompareResult
    {
        UpToDate,
        UpdateAvailable,
        NotFound,
        Error
    }

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
                sa.Log.Debug($"Script with GUID {guid} not found in online repository");
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
                sa.Log.Debug($"New version found: {onlineScript.Version} Please update (Current: {currentVersion})");
                if (showNotification)
                {
                    sa.Method.TextInfo(
                        $"New version {onlineScript.Version} available, please update\nCurrent version: {currentVersion}",
                        5000,
                        true);
                }
                return (VersionCompareResult.UpdateAvailable, onlineScript);
            }
            else
            {
                sa.Log.Debug($"Current version is up to date (Current: {currentVersion}, Online: {onlineScript.Version})");

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
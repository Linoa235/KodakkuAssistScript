using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Extensions;
using Dalamud.Utility.Numerics;
using System.Numerics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using KodakkuAssist.Module.GameOperate;
using System.ComponentModel;
using System.Collections;
using KodakkuAssist.Module.Draw.Manager;

namespace UsamisScript.StormBlood.Ucob;

[ScriptType(name: "UCOB [The Unending Coil of Bahamut]", territorys: [733], guid: "91c35ae6-1fa4-4d8c-9985-07dbfa9f7dd3",
    version: "0.0.2.2", author: "Linoa235", note: noteStr, updateInfo: UpdateInfo)]
public class Ucob
{
    // TODO
    // Nothing for now, maybe add dual tower vertical drop range

    const string noteStr =
    """
    Based on @Joshua's initial Bahamut script with modifications and expansions.
    Thanks to collaborators @KnightRider, @Meva.
    Please check and configure the "User Settings" as needed.
    Quack.
    """;
    
    private const string UpdateInfo =
        """
        1. Adapted for DuckDuck 0.5.x.x
        """;

    [UserSetting("Debug mode, turn off if not developing")]
    public bool DebugMode { get; set; } = false;

    [UserSetting("Black orb trajectory length")]
    public float blackOrbTrackLength { get; set; } = 4;

    [UserSetting("Black orb direction drawing color")]
    public ScriptColor blackOrbTrackColor { get; set; } = new ScriptColor { V4 = new Vector4(1.0f, 1.0f, 0f, 1.0f) };

    [UserSetting("Draw black orb explosion area")]
    public bool showBlackOrbField { get; set; } = true;

    [UserSetting("Black orb explosion area drawing color")]
    public ScriptColor blackOrbFieldColor { get; set; } = new ScriptColor { V4 = new Vector4(1.0f, 1.0f, 0f, 1.0f) };

    [UserSetting("P2: Show other players' Cauterize dive guidance paths")]
    public bool showOtherCauterizeRoute { get; set; } = false;

    [UserSetting("P3: [Tenstrike] Show global orb collision/interception info")]
    public bool showGlobalTenStrikeBlackOrbMsg { get; set; } = true;

    public enum BahamutFavorNorthTypeEnum
    {
        Draw12OClock_ShowTempNorth,
        Draw8SpreadDirections_ShowRecomDir,
        DontDraw,
    }

    [UserSetting("P4: Mid stack/spread + tornado: drawing method based on timing")]
    public BahamutFavorNorthTypeEnum BahamutFavorNorth { get; set; } = BahamutFavorNorthTypeEnum.Draw8SpreadDirections_ShowRecomDir;

    [UserSetting("P5: Exaflare explosion zone color")]
    public ScriptColor exflareColor { get; set; } = new ScriptColor { V4 = new Vector4(0f, 1.0f, 1.0f, 1.0f) };

    [UserSetting("P5: Exaflare warning zone color")]
    public ScriptColor exflareWarnColor { get; set; } = new ScriptColor { V4 = new Vector4(0f, 0.5f, 1.0f, 1.0f) };

    [UserSetting("P5: Show stack/buster count")]
    public bool showStackBusterNum { get; set; } = false;

    [UserSetting("Position indicator circle - Normal color")]
    public ScriptColor posColorNormal { get; set; } = new ScriptColor { V4 = new Vector4(1.0f, 1.0f, 1.0f, 1.0f) };

    [UserSetting("Position indicator circle - Player position color")]
    public ScriptColor posColorPlayer { get; set; } = new ScriptColor { V4 = new Vector4(0.2f, 1.0f, 0.2f, 1.0f) };

    public ScriptColor colorRed = new ScriptColor { V4 = new Vector4(1.0f, 0f, 0f, 1.0f) };
    public ScriptColor colorPink = new ScriptColor { V4 = new Vector4(1f, 0f, 1f, 1.0f) };
    public ScriptColor colorCyan = new ScriptColor { V4 = new Vector4(0f, 1f, 1f, 1.0f) };

    public enum UCOB_Phase
    {
        Twintania,  // P1
        Nael,   // P2
        Quickmarch_1st,
        Blackfire_2nd,
        Fellruin_3rd,
        Heavensfall_4th,
        Tenstrike_5th,
        GrandOctet_6th,
        BahamutFavor, // P4
        FlamesRebirth,  // P5
    }
    UCOB_Phase phase = UCOB_Phase.Twintania;
    int restrictorNum = 0;                          // P1 restrictor drop count
    Vector3[] RestrictorPos = new Vector3[3];       // P1 restrictor position records
    List<bool> GenerateTarget = [false, false, false, false, false, false, false, false];   // Players targeted by Generate (black orb)
    List<uint> DeathSentenceTarget = [0, 0, 0];     // P2 Death Sentence targets
    List<int> FireBallStatus = [0, 0, 0, 0, 0, 0, 0, 0];    // P2 fire/ice buff status
    int FireBallTimes = 0;  // P2 fireball stack count
    Dictionary<uint, int> CauterizeDragons = new();   // P2 Cauterize dragon dictionary (id, position)
    int CauterizeTimes = 0;     // P2 Cauterize guide count
    Vector3 QuickMarchPos = new(0, 0, 0);           // P3 Quickmarch position
    bool QuickMarchStackDrawn = false;              // P3 Quickmarch stack drawing completion flag
    bool QuickMarchEarthShakerDrawn = false;        // P3 Quickmarch Earth Shaker drawing completion flag
    Vector3 NaelPosition = new(0, 0, 0);            // P3 Nael position record
    Vector3 TwintaniaPosition = new(0, 0, 0);       // P3 Twintania position record
    Vector3 BahamutPosition = new(0, 0, 0);         // P3 Bahamut position record
    bool blackFireDrawn = false;                    // P3 Blackfire waypoint drawing completion flag
    List<bool> HeavensFallDangerPos = [false, false, false, false, false, false, false, false]; // P3 Heavensfall safe positions (left/right only)
    List<bool> HeavensFallBossPos = [false, false, false, false, false, false, false, false];   // P3 Heavensfall boss positions (left/right only)
    List<bool> HeavensFallTowerPos = [false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false];  // P4 Heavensfall tower positions
    bool HeavensFallTowerDrawn = false;         // P3 Heavensfall tower target drawing completion flag
    bool isTenStrikeTarget = false;              // P3 Tenstrike player is black orb target
    bool TenStrikeBlackOrbDrawn = false;        // P3 Tenstrike black orb waypoint drawing completion flag
    int TenStrikeEarthShakerNum = 0;            // P3 Tenstrike Earth Shaker target count
    bool isEarthShakerFirstRound = false;       // P3 Tenstrike Earth Shaker is first round
    bool grandOctDrawn = false;                 // P3 Grand Octet start position drawing completion flag
    int grandOctIconNum = 0;                    // P3 Grand Octet target icon count
    List<bool> grandOctTargetChosen = [false, false, false, false, false, false, false, false]; // P3 Grand Octet target selection
    Vector3 BahamutFavorPos = new(0, 0, 0);     // P4 initial pull position as 12 o'clock (bottom-right)
    int ArkMornNum = 0;                         // P5 Akh Morn (buster) count
    int MornAfahNum = 0;                        // P5 Morn Afah (stack) count
    private static readonly Dictionary<(int, int, int, int, int), (int, int, int)> CauterizeSafePos = 
    new Dictionary<(int, int, int, int, int), (int, int, int)>
    {
        {(1, 2, 3, 4, 5), (11, 5, 7)},
        {(1, 2, 3, 4, 6), (11, 5, 7)},
        {(1, 2, 3, 4, 7), (11, 5, 7)},
        {(1, 2, 3, 4, 8), (11, 5, 8)},
        {(1, 2, 3, 5, 6), (2, 5, 8)},
        {(1, 2, 3, 5, 7), (11, 5, 8)},
        {(1, 2, 3, 5, 8), (2, 5, 8)},
        {(1, 2, 3, 6, 7), (2, 5, 10)},
        {(1, 2, 3, 6, 8), (11, 5, 8)},
        {(1, 2, 3, 7, 8), (2, 5, 8)},
        {(1, 2, 4, 5, 6), (2, 6, 9)},
        {(1, 2, 4, 5, 7), (11, 3, 8)},
        {(1, 2, 4, 5, 8), (2, 6, 8)},
        {(1, 2, 4, 6, 7), (2, 6, 10)},
        {(1, 2, 4, 6, 8), (2, 6, 9)},
        {(1, 2, 4, 7, 8), (11, 3, 8)},
        {(1, 2, 5, 6, 7), (2, 5, 10)},
        {(1, 2, 5, 6, 8), (2, 5, 10)},
        {(1, 2, 5, 7, 8), (11, 5, 8)},
        {(1, 2, 6, 7, 8), (2, 6, 11)},
        {(1, 3, 4, 5, 6), (2, 6, 8)},
        {(1, 3, 4, 5, 7), (2, 6, 8)},
        {(1, 3, 4, 5, 8), (2, 6, 8)},
        {(1, 3, 4, 6, 7), (2, 6, 10)},
        {(1, 3, 4, 6, 8), (2, 6, 9)},
        {(1, 3, 4, 7, 8), (2, 6, 8)},
        {(1, 3, 5, 6, 7), (2, 5, 10)},
        {(1, 3, 5, 6, 8), (2, 5, 9)},
        {(1, 3, 5, 7, 8), (2, 5, 8)},
        {(1, 3, 6, 7, 8), (2, 6, 11)},
        {(1, 4, 5, 6, 7), (2, 8, 10)},
        {(1, 4, 5, 6, 8), (2, 8, 10)},
        {(1, 4, 5, 7, 8), (2, 8, 11)},
        {(1, 4, 6, 7, 8), (2, 6, 11)},
        {(1, 5, 6, 7, 8), (3, 9, 11)},
        {(2, 3, 4, 5, 6), (1, 6, 8)},
        {(2, 3, 4, 5, 7), (1, 6, 8)},
        {(2, 3, 4, 5, 8), (1, 6, 9)},
        {(2, 3, 4, 6, 7), (4, 7, 10)},
        {(2, 3, 4, 6, 8), (1, 6, 9)},
        {(2, 3, 4, 7, 8), (4, 7, 11)},
        {(2, 3, 5, 6, 7), (4, 8, 10)},
        {(2, 3, 5, 6, 8), (1, 5, 9)},
        {(2, 3, 5, 7, 8), (4, 8, 11)},
        {(2, 3, 6, 7, 8), (11, 6, 11)},
        {(2, 4, 5, 6, 7), (3, 8, 10)},
        {(2, 4, 5, 6, 8), (3, 8, 10)},
        {(2, 4, 5, 7, 8), (3, 8, 11)},
        {(2, 4, 6, 7, 8), (3, 9, 11)},
        {(2, 5, 6, 7, 8), (4, 9, 11)},
        {(3, 4, 5, 6, 7), (2, 5, 10)},
        {(3, 4, 5, 6, 8), (2, 7, 10)},
        {(3, 4, 5, 7, 8), (2, 8, 11)},
        {(3, 4, 6, 7, 8), (5, 9, 11)},
        {(3, 5, 6, 7, 8), (5, 9, 11)},
        {(4, 5, 6, 7, 8), (4, 9, 11)}
    };

    public void Init(ScriptAccessory accessory)
    {
        phase = UCOB_Phase.Twintania;
        restrictorNum = 0;                      // P1 restrictor drop count

        RestrictorPos[0] = new(0, 0, 0);        // P1 restrictor position records
        RestrictorPos[1] = new(0, 0, 0);
        RestrictorPos[2] = new(0, 0, 0);

        GenerateTarget = [false, false, false, false, false, false, false, false];   // Players targeted by Generate (black orb)

        DeathSentenceTarget = [0, 0, 0];        // P2 Death Sentence targets
        FireBallStatus = [0, 0, 0, 0, 0, 0, 0, 0];    // P2 fire/ice buff status
        FireBallTimes = 0;      // P2 fireball stack count
        CauterizeDragons = new();            // P2 Cauterize dragon dictionary (id, position)
        CauterizeTimes = 0;                  // P2 Cauterize guide count

        QuickMarchPos = new(0, 0, 0);           // P3 Quickmarch position
        QuickMarchStackDrawn = false;           // P3 Quickmarch stack drawing completion flag
        QuickMarchEarthShakerDrawn = false;     // P3 Quickmarch Earth Shaker drawing completion flag

        blackFireDrawn = false;                 // P3 Blackfire waypoint drawing completion flag

        NaelPosition = new(0, 0, 0);            // P3 Nael position record
        TwintaniaPosition = new(0, 0, 0);       // P3 Twintania position record
        BahamutPosition = new(0, 0, 0);         // P3 Bahamut position record

        HeavensFallDangerPos = [false, false, false, false, false, false, false, false]; // P3 Heavensfall safe positions (left/right only)
        HeavensFallBossPos = [false, false, false, false, false, false, false, false];   // P3 Heavensfall boss positions (left/right only)
        HeavensFallTowerPos = [false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false];  // P3 Heavensfall towers    
        HeavensFallTowerDrawn = false;          // P3 Heavensfall tower target drawing completion flag

        isTenStrikeTarget = false;                // P3 Tenstrike player is black orb target
        TenStrikeBlackOrbDrawn = false;        // P3 Tenstrike black orb waypoint drawing completion flag
        TenStrikeEarthShakerNum = 0;            // P3 Tenstrike Earth Shaker target count
        isEarthShakerFirstRound = false;        // P3 Tenstrike Earth Shaker is first round

        grandOctDrawn = false;                  // P3 Grand Octet start position drawing completion flag
        grandOctIconNum = 0;                    // P3 Grand Octet target icon count
        grandOctTargetChosen = [false, false, false, false, false, false, false, false];    // P3 Grand Octet target selection

        BahamutFavorPos = new(0, 0, 0);         // P4 initial pull position as 12 o'clock (bottom-right)

        ArkMornNum = 0;
        MornAfahNum = 0;

        accessory.Method.MarkClear();
        accessory.Method.RemoveDraw(".*");

    }

    private int PositionTo8Dir(Vector3 point, Vector3 centre)
    {
        var r = Math.Round(4 - 4 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 8;
        return (int)r;
    }

    private int PositionTo16Dir(Vector3 point, Vector3 centre)
    {
        var r = Math.Round(8 - 8 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 16;
        return (int)r;
    }

    private Vector3 RotatePoint(Vector3 point, Vector3 centre, float radian)
    {

        Vector2 v2 = new(point.X - centre.X, point.Z - centre.Z);

        var rot = MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian;
        var length = v2.Length();
        return new(centre.X + MathF.Sin(rot) * length, centre.Y, centre.Z - MathF.Cos(rot) * length);
    }

    private Vector3 ExtendPoint(Vector3 centre, float radian, float length)
    {
        return new(centre.X + MathF.Sin(radian) * length, centre.Y, centre.Z - MathF.Cos(radian) * length);
    }

    private float FindRadian(Vector3 centre, Vector3 new_point)
    {
        float radian = MathF.PI - MathF.Atan2(new_point.X - centre.X, new_point.Z - centre.Z);
        if (radian < 0)
            radian += 2 * MathF.PI;
        return radian;
    }
    private float angle2Rad(float angle)
    {
        float radian = (float)(angle * Math.PI / 180);
        return radian;
    }
    private int getPlayerIdIndex(ScriptAccessory accessory, uint pid)
    {
        return accessory.Data.PartyList.IndexOf(pid);
    }

    private int getMyIndex(ScriptAccessory accessory)
    {
        return accessory.Data.PartyList.IndexOf(accessory.Data.Me);
    }

    private string getPlayerJobIndex(ScriptAccessory accessory, uint pid)
    {
        var a = accessory.Data.PartyList.IndexOf(pid);
        switch (a)
        {
            case 0: return "MT";
            case 1: return "ST";
            case 2: return "H1";
            case 3: return "H2";
            case 4: return "D1";
            case 5: return "D2";
            case 6: return "D3";
            case 7: return "D4";
            default: return "unknown";
        }
    }

    private static bool ParseObjectId(string? idStr, out uint id)
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

    [ScriptMethod(name: "Test", eventType: EventTypeEnum.Chat, eventCondition: ["abcdefgh"], userControl: false)]
    public void EchoDebug(Event ev, ScriptAccessory sa)
    {
        var chara = sa.Data.Objects.SearchById(0x40006505);
        if (chara == null) return;
        var achara = sa.Data.Objects.SearchById(0x104FCCD4);
        if (achara == null) return;
        sa.Log.Debug($"{chara.Name}, {chara.EntityId}");
        sa.Log.Debug($"{achara.Name}, {achara.EntityId}");
        unsafe
        {
            var tetherAdd = (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)chara.Address;
            var a = tetherAdd->NameString;
            var tethers = tetherAdd->Vfx.Tethers;
            var tetherNum = tethers.Length;
            sa.Log.Debug($"tetherNum = {tetherNum}");
            var tetherTarget0 = tethers[0].TargetId.ObjectId;
            var tetherId0 = tethers[0].Id;
            sa.Log.Debug($"tetherTarget0 {tetherId0} = {tetherTarget0:x8}");   // First tether target
            var tetherTarget1 = tethers[1].TargetId.ObjectId;
            sa.Log.Debug($"tetherTarget1 = {tetherTarget1:x8}");   // Second tether target
            
            var atetherAdd = (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)achara.Address;
            var atethers = tetherAdd->Vfx.Tethers;
            var atetherNum = tethers.Length;
            sa.Log.Debug($"tetherNum = {tetherNum}");
            var atetherTarget0 = tethers[0].TargetId.ObjectId;
            var atetherId0 = tethers[0].Id;
            sa.Log.Debug($"tetherTarget0 {tetherId0} = {tetherTarget0:x8}");   // First tether target
            var atetherTarget1 = tethers[1].TargetId.ObjectId;
            sa.Log.Debug($"tetherTarget1 = {tetherTarget1:x8}");   // Second tether target
        }
    }

    #region Global

    [ScriptMethod(name: "[Global] Black Orb Path", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:8160"])]
    public void BlackOrbTrack(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"BlackOrbPath-{sid}";
        dp.Scale = new(2f, blackOrbTrackLength);
        dp.Color = blackOrbTrackColor.V4.WithW(3);
        dp.Owner = sid;
        dp.Delay = 3500;
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "[Global] Hypernova Danger Zone", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2003393", "Operate:Add"])]
    public void HypernovaField(Event @event, ScriptAccessory accessory)
    {
        var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Hypernova Danger Zone";
        dp.Scale = new(5f);
        dp.Position = spos;
        dp.DestoryAt = 15000;
        dp.Color = colorRed.V4.WithW(3);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "[Global] Black Orb Path Removal (Uncontrollable)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:9903"], userControl: false)]
    public void BlackOrbTrackRemove(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        accessory.Method.RemoveDraw($"BlackOrbPath-{sid}");
    }

    [ScriptMethod(name: "[Global] Generate Target Record (Uncontrollable)", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0076"], userControl: false)]
    public void GenerateTargetRecord(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(50).ContinueWith(t =>
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var tidx = getPlayerIdIndex(accessory, tid);
            GenerateTarget[tidx] = true;
            if (DebugMode)
            {
                var tidjob = getPlayerJobIndex(accessory, tid);
                accessory.Method.SendChat($"/e Detected {tidjob} targeted by Generate.");
            }

            if (phase != UCOB_Phase.Tenstrike_5th) return;
            if (tidx == getMyIndex(accessory))
                isTenStrikeTarget = true;
        });
    }

    [ScriptMethod(name: "[Global] Restrictor Position Record (Uncontrollable)", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2001151", "Operate:Add"], userControl: false)]
    public void RestrictorPosRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.Twintania) return;
        var tpos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);

        if (DebugMode)
        {
            accessory.Method.SendChat($"/e Recorded restrictor at position ({tpos.X}, {tpos.Z}).");
        }
        RestrictorPos[restrictorNum] = tpos;
        restrictorNum++;

        if (restrictorNum != 3) return;
        float restrictorRad1 = FindRadian(new(0, 0, 0), RestrictorPos[1]);
        float restrictorRad2 = FindRadian(new(0, 0, 0), RestrictorPos[2]);
        float bahamutFavorRad = (restrictorRad1 + restrictorRad2) / 2;
        BahamutFavorPos = ExtendPoint(new(0, 0, 0), bahamutFavorRad, 24);

        if (restrictorNum == 3 && DebugMode)
        {
            accessory.Method.SendChat($"/e Restrictor count reached 3, positions recorded.");
        }
    }
    
    [ScriptMethod(name: "[Global] Refresh Generate Targets (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9902"], userControl: false)]
    public void RefreshGenerateTarget(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(500).ContinueWith(t =>
        {
            GenerateTarget = [false, false, false, false, false, false, false, false];
        });
    }

    [ScriptMethod(name: "[Global] Black Orb Explosion Area", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9902"])]
    public void BlackOrbField(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        if (!showBlackOrbField) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        for (int i = 0; i < restrictorNum; i++)
        {
            dp.Name = $"BlackOrbExplosionArea-{i}";
            dp.Scale = new(7.5f);
            dp.Color = blackOrbFieldColor.V4.WithW(0.4f);
            dp.Position = RestrictorPos[i];
            dp.Delay = 3500;
            dp.DestoryAt = phase == UCOB_Phase.Tenstrike_5th ? 3500 : 10000;     // Special handling for Tenstrike's two rounds
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    [ScriptMethod(name: "[Global] Black Orb Explosion Area Removal", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:9903"], userControl: false)]
    public void BlackOrbFieldRemove(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        // Do not remove during Tenstrike's two rounds
        if (phase == UCOB_Phase.Tenstrike_5th) return;
        var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        int minIdx = getNearestOrbField(spos);
        var tidx = getPlayerIdIndex(accessory, tid);
        accessory.Method.RemoveDraw($"BlackOrbExplosionArea-{minIdx}");
        accessory.Method.RemoveDraw($"RestrictorPositionMarker{minIdx}");
        accessory.Method.RemoveDraw($"RestrictorOrbPlayer{tidx}Waypoint");
    }

    [ScriptMethod(name: "[Global] Twister Danger Zone", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2001168", "Operate:Add"])]
    public void Twister_Field(Event @event, ScriptAccessory accessory)
    {
        var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Twister Danger Zone";
        dp.Scale = new(1.5f);
        dp.Position = spos;
        dp.DestoryAt = 7000;
        dp.Color = colorRed.V4.WithW(3);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    private int getNearestOrbField(Vector3 spos)
    {
        int minIdx = 0;
        float minLength = 999f;
        for (int i = 0; i < restrictorNum; i++)
        {
            float length = new Vector2(spos.X - RestrictorPos[i].X, spos.Z - RestrictorPos[i].Z).Length();
            if (length < minLength)
            {
                minLength = length;
                minIdx = i;
            }
        }
        return minIdx;
    }

    #endregion

    #region P1: Twintania

    private void drawTwister(int delay, int destoryAt, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("Twister", delay, true);
        for (var i = 0; i < 8; i++)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Twister{i}";
            dp.Scale = new(1.5f);
            dp.Owner = accessory.Data.PartyList[i];
            dp.Delay = delay;
            dp.DestoryAt = destoryAt;
            dp.ScaleMode = ScaleMode.ByTime;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    [ScriptMethod(name: "P1&P4 Twintania: Twister Self-Position Warning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9898"])]
    public void Twister_PlayerPosition(Event @event, ScriptAccessory accessory)
    {
        drawTwister(0, 2000, accessory);
    }

    [ScriptMethod(name: "P1&P3 Twintania: Fireball Stack Range Warning", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0075"])]
    public void FireBallStackTarget(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"FireballStackRange";
        dp.Scale = new(4f);
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = tid;
        dp.DestoryAt = 10000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "P1&P3 Twintania: Fireball Stack Range Warning Removal (Uncontrollable)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:9900", "TargetIndex:1"], userControl: false)]
    public void FireBallStackTargetRemove(Event @event, ScriptAccessory accessory)
    {
        // Darn fireball timings are inconsistent
        accessory.Method.RemoveDraw($"FireballStackRange");
    }

    #endregion

    #region P2: Nael

    [ScriptMethod(name: "P2 Nael: Phase Change Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9922"], userControl: false)]
    public void P2_PhaseChange(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.Twintania) return;
        phase = UCOB_Phase.Nael;
    }

    [ScriptMethod(name: "P2 Nael: Fire Dragon Tether Stack Range Warning", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0005"])]
    public void FireBallDragonStackTarget(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        FireBallTimes++;
        // accessory.Log.Debug($"{accessory.Data.PartyList.Count}");
        var MyIndex = getMyIndex(accessory);
        // accessory.Log.Debug($"MyIndex: {MyIndex} ");
        // accessory.Log.Debug($"me: {accessory.Data.Me}");
        // accessory.Log.Debug($"party: {accessory.Data.PartyList[0]}, {accessory.Data.PartyList[1]}, {accessory.Data.PartyList[2]}, {accessory.Data.PartyList[3]}, " +
        //                     $"{accessory.Data.PartyList[4]}, {accessory.Data.PartyList[5]}, {accessory.Data.PartyList[6]}, {accessory.Data.PartyList[7]}");
        // accessory.Log.Debug($"FireBallStatus: {BuildListStr(accessory, FireBallStatus)}");
        // accessory.Log.Debug($"FireBallStatus[MyIndex]: {FireBallStatus[MyIndex]}");
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"FireDragonFireballStackRange";
        dp.Scale = new(4f);
        dp.Owner = tid;
        dp.DestoryAt = 10000;
        switch (FireBallTimes)
        {
            case 1:
                // Go take 1st fire
                dp.Color = FireBallStatus[MyIndex] != 1 ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                break;
            case 2:
                // If target is me OR I have ice buff, go take 2nd fire
                dp.Color = (tid == accessory.Data.Me) | (FireBallStatus[MyIndex] == -1) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                break;
            case 3:
            case 4:
                // If target is me OR I don't have fire buff, go take 3rd/4th fire
                dp.Color = (tid == accessory.Data.Me) | (FireBallStatus[MyIndex] != 1) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                break;
            default:
                return;
        }
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    /// <summary>
    /// Convert list information to string.
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="myList"></param>
    /// <param name="isJob">If true, convert to job string before conversion</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string BuildListStr<T>(ScriptAccessory accessory, List<T> myList, bool isJob = false)
    {
        return string.Join(", ", myList.Select(item => item?.ToString() ?? ""));
    }

    [ScriptMethod(name: "P2 Nael: Fire Dragon Tether Stack Range Warning Removal (Uncontrollable)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:9925", "TargetIndex:1"], userControl: false)]
    public void FireBallDragonStackTargetRemove(Event @event, ScriptAccessory accessory)
    {
        // Darn fire tether timings are inconsistent
        accessory.Method.RemoveDraw($"FireDragonFireballStackRange");
    }
    [ScriptMethod(name: "P2 Nael: Fire/Ice Status Add Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(465|464)$"], userControl: false)]
    public void FireBallStatusAddRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.Nael) return;
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        var stid = JsonConvert.DeserializeObject<uint>(@event["StatusID"]);
        var idx = getPlayerIdIndex(accessory, tid);
        if (stid == 465)
            // -1 for ice, +1 for fire
            FireBallStatus[idx] = -1;
        else
            FireBallStatus[idx] = 1;
    }
    [ScriptMethod(name: "P2 Nael: Fire/Ice Status Remove Record", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:regex:^(465|464)$"], userControl: false)]
    public void FireBallStatusRemoveRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.Nael) return;
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        var idx = getPlayerIdIndex(accessory, tid);
        FireBallStatus[idx] = 0;
    }

    [ScriptMethod(name: "P2 Nael: Death Sentence Record (Uncontrollable)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:210"], userControl: false)]
    public void DeathSentence_Record(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
        // DeathSentenceNum++;
        switch (dur)
        {
            case 6000:
                DeathSentenceTarget[0] = tid;
                if (DebugMode)
                    accessory.Method.SendChat($"/e Detected Doom1 player: {getPlayerJobIndex(accessory, tid)}");
                break;
            case 10000:
                DeathSentenceTarget[1] = tid;
                if (DebugMode)
                    accessory.Method.SendChat($"/e Detected Doom2 player: {getPlayerJobIndex(accessory, tid)}");
                break;
            case 16000:
                DeathSentenceTarget[2] = tid;
                if (DebugMode)
                    accessory.Method.SendChat($"/e Detected Doom3 player: {getPlayerJobIndex(accessory, tid)}");
                break;
        }
    }

    [ScriptMethod(name: "P2 Nael: Wings of Salvation Warning and Waypoint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9930"])]
    public void WingsOfSalvation_Position(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(2000).ContinueWith(t =>
        {
            var epos = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
            uint DoomPlayerID = 0; // First non-zero value in DeathSentenceTarget

            for (int i = 0; i < DeathSentenceTarget.Count; i++)
            {
                if (DeathSentenceTarget[i] != 0)
                {
                    DoomPlayerID = DeathSentenceTarget[i];  // Assign to DoomPlayerID
                    DeathSentenceTarget[i] = 0;  // Set that value to 0
                    if (DebugMode)
                        accessory.Method.SendChat($"/e Preparing to draw Doom player: {getPlayerJobIndex(accessory, DoomPlayerID)}");
                    break;  // Exit loop after finding first non-zero value
                }
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"WhiteCircleRangeWarning";
            dp.Scale = new(1.5f);
            dp.Position = epos;
            dp.Delay = 1000;
            dp.DestoryAt = 4000;
            dp.Color = DoomPlayerID == accessory.Data.Me ? accessory.Data.DefaultSafeColor.WithW(3) : colorRed.V4.WithW(3);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            if (DoomPlayerID == accessory.Data.Me)
            {
                accessory.Method.TextInfo("About to step on white circle", 2000, true);
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Scale = new(0.5f);
                dp.Name = $"WhiteCircleWaypoint";
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = epos;
                dp.ScaleMode = ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 1000;
                dp.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        });
    }

    [ScriptMethod(name: "P2 Nael: Chain Lightning Target Warning", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:9927"])]
    public void ChainLightening(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ChainLightning{tid}";
        dp.Scale = new(5);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = tid;
        dp.Delay = 4000;
        dp.DestoryAt = 2000;
        dp.Color = colorPink.V4.WithW(3);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    #endregion

    #region P2: Nael Quotes

    private void drawNaelQuote_Circle(uint sid, int delay, int destoryAt, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Iron Chariot";
        dp.Scale = new(8.55f);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = sid;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        dp.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    private void drawNaelQuote_Donut(uint sid, int delay, int destoryAt, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Lunar Dynamo";
        dp.Scale = new(22);
        dp.InnerScale = new(6);
        dp.Radian = float.Pi * 2;
        dp.Owner = sid;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        dp.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }

    private void drawNaelQuote_Stack(uint sid, int delay, int destoryAt, ScriptAccessory accessory)
    {
        for (var i = 0; i < 8; i++)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"ThermionicBeam{i}";
            dp.Scale = new(4);
            dp.Owner = accessory.Data.PartyList[i];
            dp.Delay = delay;
            dp.DestoryAt = destoryAt;
            dp.Color = accessory.Data.DefaultSafeColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    private void drawNaelQuote_Spread(uint sid, int delay, int destoryAt, ScriptAccessory accessory)
    {
        for (var i = 0; i < 8; i++)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"RavenBlast{i}";
            dp.Scale = new(3);
            dp.ScaleMode = ScaleMode.ByTime;
            dp.Owner = accessory.Data.PartyList[i];
            dp.Delay = delay;
            dp.DestoryAt = destoryAt;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    private void drawNaelQuote_Tank(uint sid, int delay, int destoryAt, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"LunaryBuster";
        dp.Scale = new(5);
        dp.Owner = sid;
        dp.CentreResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.CentreOrderIndex = 1;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        dp.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    private void drawMeteorStream(uint sid, int delay, int destoryAt, ScriptAccessory accessory)
    {
        for (var i = 0; i < 8; i++)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"MeteorStream{i}";
            dp.Scale = new(4);
            dp.ScaleMode = ScaleMode.ByTime;
            dp.Owner = accessory.Data.PartyList[i];
            dp.Delay = delay;
            dp.DestoryAt = destoryAt;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    [ScriptMethod(name: "P2 Nael: Quotes", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:regex:^(649[234567]|650[01])$"])]
    public void NaelQuotesP2(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var quoteId = @event["Id"];

        switch (quoteId)
        {
            case "6492":
                {
                    // O moon, light the way to iron authority!
                    drawNaelQuote_Donut(sid, 0, 5000, accessory);
                    drawNaelQuote_Circle(sid, 5000, 3000, accessory);
                    break;
                }
            case "6493":
                {
                    // O moon, your heat sears the enemy!
                    drawNaelQuote_Donut(sid, 0, 5000, accessory);
                    drawNaelQuote_Stack(sid, 5000, 3000, accessory);
                    break;
                }
            case "6494":
                {
                    // The searing path that burns is iron authority!
                    drawNaelQuote_Stack(sid, 0, 5000, accessory);
                    drawNaelQuote_Circle(sid, 5000, 3000, accessory);
                    break;
                }
            case "6495":
                {
                    // Burning, bestow upon me the moon's blessing!
                    drawNaelQuote_Stack(sid, 0, 5000, accessory);
                    drawNaelQuote_Donut(sid, 5000, 3000, accessory);

                    break;
                }
            case "6496":
                {
                    // I come to tread upon iron authority!
                    drawNaelQuote_Spread(sid, 0, 5000, accessory);
                    drawNaelQuote_Circle(sid, 5000, 3000, accessory);
                    break;
                }
            case "6497":
                {
                    // I come to howl at the moon!
                    drawNaelQuote_Spread(sid, 0, 5000, accessory);
                    drawNaelQuote_Donut(sid, 5000, 3000, accessory);
                    break;
                }
            case "6500":
                {
                    // Supernova, shine brighter! Praise the red moon on starfall night!
                    drawMeteorStream(sid, 12000, 3000, accessory);
                    drawNaelQuote_Tank(sid, 15000, 2000, accessory);
                    break;
                }
            case "6501":
                {
                    // Supernova, shine brighter! Illuminate the burning lands under the red moon!
                    drawNaelQuote_Tank(sid, 13000, 2000, accessory);
                    drawNaelQuote_Stack(sid, 15000, 2000, accessory);
                    break;
                }
        }
    }
    #endregion

    #region P2: Cauterize (Dragons)

    [ScriptMethod(name: "P2 Nael: Cauterize Dragon Position Record (Uncontrollable)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(816[34567])$"], userControl: false)]
    public void CauterizePosRecord(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        lock (CauterizeDragons)
        {
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var dir = PositionTo8Dir(pos, new(0, 0, 0));
            CauterizeDragons.Add(sid, dir);
        }
    }

    [ScriptMethod(name: "P2 Nael: Cauterize Dive Guide Range Warning", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0014"])]
    public void CauterizeTarget(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.Nael) return;

        CauterizeTimes++;
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;

        // Sort CauterizeDragons by direction (dir) ascending
        var sortedDragons = CauterizeDragons
            .OrderBy(d => d.Value)  // Sort by dir ascending
            .ToList();
        if (!showOtherCauterizeRoute && tid != accessory.Data.Me) return;

        switch (CauterizeTimes)
        {
            case 1:
                CauterizeSafePosDraw(accessory);
                CauterizeRouteDraw(sortedDragons[0].Key, tid, accessory);
                CauterizeRouteDraw(sortedDragons[1].Key, tid, accessory);
                break;
            case 2:
                CauterizeSafePosDraw(accessory);
                CauterizeRouteDraw(sortedDragons[2].Key, tid, accessory);
                break;
            case 3:
                CauterizeSafePosDraw(accessory);
                CauterizeRouteDraw(sortedDragons[3].Key, tid, accessory);
                CauterizeRouteDraw(sortedDragons[4].Key, tid, accessory);
                break;
        }
    }
    private void CauterizeRouteDraw(uint sid, uint tid, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"CauterizeGuide{sid}";
        dp.Scale = new(20, 45);
        dp.Owner = sid;
        dp.DestoryAt = 6000;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.TargetObject = tid;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        
        var dp0 = accessory.Data.GetDefaultDrawProperties();
        dp0.Name = $"CauterizeDirection{sid}";
        dp0.Scale = new(2.5f, 12.5f);
        dp0.Owner = sid;
        dp0.DestoryAt = 6000;
        dp0.Color = accessory.Data.DefaultDangerColor.WithW(1f);
        dp0.TargetObject = tid;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp0);
    }

    private void CauterizeSafePosDraw(ScriptAccessory accessory)
    {
        var sortedDragonsDir = CauterizeDragons.OrderBy(d => d.Value).Select(d => d.Value + 1).ToArray();
        var sortedDragonsDirTuple = (sortedDragonsDir[0], sortedDragonsDir[1], sortedDragonsDir[2], sortedDragonsDir[3], sortedDragonsDir[4]);
        var safePosClock = CauterizeSafePos.TryGetValue(sortedDragonsDirTuple, out var safePos) ? safePos : (0,0,0);
        if (safePosClock.Equals((0,0,0)))
        {
            return;
        }
        var basePos = new Vector3(0, 0, -21);
        int clock = CauterizeTimes switch
        {
            1 => safePosClock.Item1,
            2 => safePosClock.Item2,
            3 => safePosClock.Item3,
            _ => throw new IndexOutOfRangeException()
        };
        var SafePos = RotatePoint(basePos, new(0, 0, 0), angle2Rad(clock * 30 % 360));
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"CauterizeSafeSpot";
        dp.Scale = new(2);
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = SafePos;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "P2 Nael: Cauterize Actual Range Warning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(993[12345])$"])]
    public void CauterizeField(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Cauterize";
        dp.Scale = new(20, 45);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = sid;
        dp.DestoryAt = 3700;
        dp.Color = colorCyan.V4.WithW(0.6f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    #endregion

    #region P3: Bahamut (Quickmarch ~ Fellruin)

    [ScriptMethod(name: "P3 Bahamut: [Quickmarch] Phase Change Record (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9954"], userControl: false)]
    public void P3_PhaseChange_1st(Event @event, ScriptAccessory accessory)
    {
        // if (phase != UCOB_Phase.Nael) return;
        phase = UCOB_Phase.Quickmarch_1st;
    }

    // Twisting Dive
    [ScriptMethod(name: "P3 Bahamut: Twisting Dive", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9906"])]
    public void TwistingDive(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "TwistingDiveRange";
        dp.Scale = new(8, 45);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.Owner = sid;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        drawTwister(0, 5200, accessory);
    }

    [ScriptMethod(name: "P3 Bahamut: Lunar Dive", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9923"])]
    public void LunarDive(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "LunarDive";
        dp.Scale = new(8, 45);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.Owner = sid;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "P3 Bahamut: Megaflare Dive and Spread", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9953"])]
    public void MegaFlareDive(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "MegaflareDive";
        dp.Scale = new(12, 45);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.Owner = sid;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        if (phase != UCOB_Phase.Quickmarch_1st) return;
        for (var i = 0; i < 8; i++)
        {
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "MegaflareDiveSpread";
            dp.Scale = new(5);
            dp.ScaleMode = ScaleMode.ByTime;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = accessory.Data.PartyList[i];
            dp.Delay = 4000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    [ScriptMethod(name: "P3 Bahamut: Earth Shaker Range Warning", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0028"])]
    public void EarthShaker(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"EarthShaker{tid}";
        dp.Position = new(0, 0, 0);
        dp.Scale = new(50);
        dp.Radian = float.Pi / 2;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.TargetObject = tid;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "P3 Bahamut: [Quickmarch] Megaflare Dive Position Record (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9953"], userControl: false)]
    public void MegaFlareDivePosRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.Quickmarch_1st) return;
        QuickMarchPos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
    }

    [ScriptMethod(name: "P3 Bahamut: [Quickmarch] Temporary North (Dive Start) Direction Indicator", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9953"])]
    public void MegaFlareDiveNorth(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.Quickmarch_1st) return;
        Task.Delay(100).ContinueWith(t =>
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Quickmarch12OClockMarker";
            dp.Scale = new(1.5f);
            dp.ScaleMode = ScaleMode.YByDistance;
            dp.Color = posColorNormal.V4.WithW(2);
            dp.Position = new(0, 0, 0);
            dp.TargetPosition = QuickMarchPos;
            dp.Delay = 4000;
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        });
    }

    [ScriptMethod(name: "P3 Bahamut: [Quickmarch] Stack Waypoint", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0027"])]
    public void QuickmarchStack(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        if (phase != UCOB_Phase.Quickmarch_1st) return;
        if (QuickMarchStackDrawn) return;

        QuickMarchStackDrawn = true;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"StackIndicator{tid}";
        dp.Owner = tid;
        dp.Scale = new(3);
        dp.Radian = float.Pi / 2;
        dp.Color = tid == accessory.Data.Me ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        // Stack position indicator
        var rad = FindRadian(new(0, 0, 0), QuickMarchPos);
        var stackPos = ExtendPoint(new(0, 0, 0), rad, -6);

        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"StackPosition{tid}";
        dp.Scale = new(3f);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Color = tid == accessory.Data.Me ? posColorPlayer.V4.WithW(3) : posColorNormal.V4.WithW(1);
        dp.Position = stackPos;
        dp.Delay = 0;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        if (tid != accessory.Data.Me) return;
        accessory.Method.TextInfo("About to stack", 5000, true);
        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Scale = new(0.5f);
        dp.Name = $"StackWaypoint{tid}";
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = stackPos;
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Delay = 0;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "P3 Bahamut: [Quickmarch] Earth Shaker Position Indicator", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0028"])]
    public void EarthShakerDir(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        if (phase != UCOB_Phase.Quickmarch_1st) return;
        if (tid != accessory.Data.Me) return;
        if (QuickMarchEarthShakerDrawn) return;

        QuickMarchEarthShakerDrawn = true;
        accessory.Method.RemoveDraw($"Quickmarch12OClockMarker");
        var esPos_right = RotatePoint(QuickMarchPos, new(0, 0, 0), float.Pi / 2);
        var esPos_left = RotatePoint(QuickMarchPos, new(0, 0, 0), -float.Pi / 2);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "EarthShakerWaypoint-Mid";
        dp.Scale = new(1.5f);
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = getPlayerIdIndex(accessory, accessory.Data.Me) > 3 ? posColorPlayer.V4.WithW(3) : posColorNormal.V4.WithW(1);
        dp.Position = new(0, 0, 0);
        dp.TargetPosition = QuickMarchPos;
        dp.Delay = 0;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "EarthShakerWaypoint-Right";
        dp.Scale = new(1.5f);
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = getPlayerIdIndex(accessory, accessory.Data.Me) == 3 ? posColorPlayer.V4.WithW(3) : posColorNormal.V4.WithW(1);
        dp.Position = new(0, 0, 0);
        dp.TargetPosition = esPos_right;
        dp.Delay = 0;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "EarthShakerWaypoint-Left";
        dp.Scale = new(1.5f);
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = getPlayerIdIndex(accessory, accessory.Data.Me) == 2 ? posColorPlayer.V4.WithW(3) : posColorNormal.V4.WithW(1);
        dp.Position = new(0, 0, 0);
        dp.TargetPosition = esPos_left;
        dp.Delay = 0;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

    }

    [ScriptMethod(name: "P3 Bahamut: [Blackfire] Phase Change Record (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9955"], userControl: false)]
    public void P3_PhaseChange_2nd(Event @event, ScriptAccessory accessory)
    {
        // if (phase != UCOB_Phase.Quickmarch_1st) return;
        phase = UCOB_Phase.Blackfire_2nd;
    }

    [ScriptMethod(name: "P3 Bahamut: Nael Position Record (Uncontrollable)", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:8161"], userControl: false)]
    public void NaelPosRecord(Event @event, ScriptAccessory accessory)
    {
        var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        NaelPosition = spos;
    }

    [ScriptMethod(name: "P3 Bahamut: [Blackfire] Nael Position Waypoint", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:8161"])]
    public void BlackFireNaelDir(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.Blackfire_2nd) return;
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;

        Task.Delay(100).ContinueWith(t =>
        {
            if (new Vector2(NaelPosition.X, NaelPosition.Z).Length() < 23) return;
            if (blackFireDrawn) return;
            if (DebugMode)
            {
                accessory.Method.SendChat($"/e Found Nael position, drawing at feet, {new Vector2(NaelPosition.X, NaelPosition.Z).Length()}");
                var dp0 = accessory.Data.GetDefaultDrawProperties();
                dp0.Name = $"NaelPosition";
                dp0.Scale = new(3f);
                dp0.Color = posColorPlayer.V4.WithW(3);
                dp0.Position = NaelPosition;
                dp0.Delay = 0;
                dp0.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp0);
            }
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "NaelPositionWaypoint";
            dp.Scale = new(0.5f, 24);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = sid;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            blackFireDrawn = true;
        });
    }

    [ScriptMethod(name: "P3 Bahamut: [Fellruin] Phase Change Record (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9956"], userControl: false)]
    public void P3_PhaseChange_3rd(Event @event, ScriptAccessory accessory)
    {
        // if (phase != UCOB_Phase.Blackfire_2nd) return;
        phase = UCOB_Phase.Fellruin_3rd;
    }

    // I come to howl at the moon! Summon starfall night!
    [ScriptMethod(name: "P3 Bahamut: [Fellruin] Spread and Donut", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:6502"])]
    public void FR_Spread_and_In(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        drawNaelQuote_Spread(sid, 0, 5000, accessory);
        drawNaelQuote_Donut(sid, 5000, 3000, accessory);
    }

    // I come from the moon, summon starfall night!
    [ScriptMethod(name: "P3 Bahamut: [Fellruin] Donut and Spread", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:6503"])]
    public void FR_In_and_Spread(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        drawNaelQuote_Donut(sid, 0, 5000, accessory);
        drawNaelQuote_Spread(sid, 5000, 3000, accessory);
    }

    [ScriptMethod(name: "P3 Bahamut: [Fellruin] Meteor Stream after Aetheric Profusion", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:9905"])]
    public void AethericProfusion(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        drawMeteorStream(sid, 0, 4000, accessory);
    }

    #endregion

    #region P3: Bahamut (Heavensfall)

    [ScriptMethod(name: "P3 Bahamut: [Heavensfall] Phase Change Record (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9957"], userControl: false)]
    public void P3_PhaseChange_4th(Event @event, ScriptAccessory accessory)
    {
        // if (phase != UCOB_Phase.Fellruin_3rd) return;
        phase = UCOB_Phase.Heavensfall_4th;
    }

    [ScriptMethod(name: "P3 Bahamut: [Heavensfall] Start Position Record (Uncontrollable)", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:regex:^(8161|8159|8168)$"], userControl: false)]
    public void HeavensFallPosRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.Heavensfall_4th) return;
        var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        float distance = new Vector2(spos.X, spos.Z).Length();
        if (distance < 23) return;
        var idx = PositionTo8Dir(spos, new(0, 0, 0));

        // Update boss position and danger positions
        HeavensFallBossPos[idx] = true;

        // Set position opposite to idx as dangerous
        HeavensFallDangerPos[idx] = true;
        HeavensFallDangerPos[idx >= 4 ? idx - 4 : idx + 4] = true;
    }

    [ScriptMethod(name: "P3 Bahamut: [Heavensfall] Start Position Waypoint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9906"])]
    public void HeavensFallSafeDir(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.Heavensfall_4th) return;
        Task.Delay(100).ContinueWith(t =>
        {
            // At this point, only two false values remain in HeavensFallDangerPos, which are the safe points.
            // Check the previous variable of these two false indices in HeavensFallBossPos, if true, it's right side; if false, it's left side.
            // Find the two remaining false values in HeavensFallDangerPos
            List<int> safeIndices = new List<int>();
            for (int i = 0; i < HeavensFallDangerPos.Count(); i++)
            {
                if (!HeavensFallDangerPos[i])  // Only care about indices with value false
                    safeIndices.Add(i);
            }

            if (safeIndices.Count() == 2)
            {
                var MyIndex = getMyIndex(accessory);
                // Check the position before the safe position in HeavensFallBossPos
                foreach (var idx in safeIndices)
                {
                    int prevIdx = idx == 0 ? 7 : idx - 1;  // If idx is 0, previous is 7, otherwise idx - 1
                    // Determine if boss exists at the previous position
                    bool isPrevTrue = HeavensFallBossPos[prevIdx];
                    if (isPrevTrue)
                    {
                        // If previous position is true, this idx is the safe point to the right of the three brothers, for D2 and D4
                        Vector3 safePosition = RotatePoint(new Vector3(0, 0, -23), new Vector3(0, 0, 0), idx * float.Pi / 4);
                        HeavensFallSafeDirDraw(safePosition, MyIndex == 5 || MyIndex == 7, accessory);
                        if (DebugMode)
                            accessory.Method.SendChat($"/e {idx} is the safe point on the right");
                    }
                    else
                    {
                        // If previous position is false, this idx is the safe point to the left of the three brothers, for MT and H1
                        Vector3 safePosition = RotatePoint(new Vector3(0, 0, -23), new Vector3(0, 0, 0), idx * float.Pi / 4);
                        HeavensFallSafeDirDraw(safePosition, MyIndex == 0 || MyIndex == 2, accessory);
                        if (DebugMode)
                            accessory.Method.SendChat($"/e {idx} is the safe point on the left");
                    }
                }

                // Safe point in front of Nael, for ST and H2
                var idx_nael = PositionTo8Dir(NaelPosition, new(0, 0, 0));
                Vector3 safePositionNael = RotatePoint(new Vector3(0, 0, -23), new Vector3(0, 0, 0), idx_nael * float.Pi / 4);
                HeavensFallSafeDirDraw(safePositionNael, MyIndex == 1 || MyIndex == 3, accessory);
                if (DebugMode)
                    accessory.Method.SendChat($"/e {idx_nael} is the safe point in front of Nael");

                // Safe point behind Nael, for D1 and D3
                idx_nael = PositionTo8Dir(NaelPosition, new(0, 0, 0));
                idx_nael = idx_nael >= 4 ? idx_nael - 4 : idx_nael + 4;
                safePositionNael = RotatePoint(new Vector3(0, 0, -23), new Vector3(0, 0, 0), idx_nael * float.Pi / 4);
                HeavensFallSafeDirDraw(safePositionNael, MyIndex == 4 || MyIndex == 6, accessory);
                if (DebugMode)
                    accessory.Method.SendChat($"/e {idx_nael} is the safe point behind Nael");
            }
        });
    }

    private void HeavensFallSafeDirDraw(Vector3 safepos, bool isPlayerPos, ScriptAccessory accessory)
    {
        if (isPlayerPos)
        {
            var dp0 = accessory.Data.GetDefaultDrawProperties();
            dp0.Name = "HeavensfallSafeSpotWaypoint";
            dp0.Scale = new(0.5f, 22);
            dp0.ScaleMode |= ScaleMode.YByDistance;
            dp0.Color = accessory.Data.DefaultSafeColor;
            dp0.Owner = accessory.Data.Me;
            dp0.TargetPosition = safepos;
            dp0.Delay = 0;
            dp0.DestoryAt = 3700;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);
        }

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "HeavensfallSafeSpot";
        dp.Scale = new(4);
        dp.Color = isPlayerPos ? posColorPlayer.V4.WithW(3) : posColorNormal.V4;
        dp.Position = safepos;
        dp.Delay = 0;
        dp.DestoryAt = 3700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "P3 Bahamut: [Heavensfall] Tower Position Record (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9951"], userControl: false)]
    public void HeavensFallTower(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.Heavensfall_4th) return;
        var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        var idx_tower = PositionTo16Dir(spos, new(0, 0, 0));
        HeavensFallTowerPos[idx_tower] = true;
    }

    [ScriptMethod(name: "P3 Bahamut: [Heavensfall] Tower Position Waypoint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9951"])]
    public void HeavensFallTowerDir(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.Heavensfall_4th) return;

        Task.Delay(100).ContinueWith(t =>
        {
            if (HeavensFallTowerDrawn) return;
            HeavensFallTowerDrawn = true;
            var naelIdx = PositionTo16Dir(NaelPosition, new(0, 0, 0));
            var towerJudgeIdx = naelIdx;
            var towerPlayerIdxCount = 0;
            var MyIndex = getMyIndex(accessory);
            List<int> towerPlayerTarget = [7, 0, 6, 1, 5, 2, 4, 3];

            for (int i = 0; i < HeavensFallTowerPos.Count(); i++)
            {
                if (DebugMode)
                {
                    accessory.Method.SendChat($"/e Searching for tower at position {towerJudgeIdx}, this is tower #{towerPlayerIdxCount}.");
                }
                if (HeavensFallTowerPos[towerJudgeIdx])  // Found tower position
                {
                    if (towerPlayerIdxCount == towerPlayerTarget[MyIndex])
                    {
                        // Found it!
                        var dp0 = accessory.Data.GetDefaultDrawProperties();
                        dp0.Name = $"TargetTower{towerJudgeIdx}";
                        dp0.Scale = new(3f);
                        dp0.Color = posColorPlayer.V4.WithW(3);
                        dp0.Position = RotatePoint(new Vector3(0, 0, -22), new Vector3(0, 0, 0), towerJudgeIdx * float.Pi / 8);
                        dp0.Delay = 0;
                        dp0.DestoryAt = 6500;
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp0);

                        dp0 = accessory.Data.GetDefaultDrawProperties();
                        dp0.Name = $"KnockbackPosition{towerJudgeIdx}";
                        dp0.Scale = new(1f);
                        dp0.Color = posColorPlayer.V4;
                        dp0.Position = RotatePoint(new Vector3(0, 0, -10), new Vector3(0, 0, 0), towerJudgeIdx * float.Pi / 8);
                        dp0.Delay = 0;
                        dp0.DestoryAt = 6500;
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp0);

                        dp0 = accessory.Data.GetDefaultDrawProperties();
                        dp0.Name = "HeavensfallKnockbackTowerWaypoint";
                        dp0.Scale = new(0.5f);
                        dp0.ScaleMode |= ScaleMode.YByDistance;
                        dp0.Color = accessory.Data.DefaultSafeColor;
                        dp0.Owner = accessory.Data.Me;
                        dp0.TargetPosition = RotatePoint(new Vector3(0, 0, -10), new Vector3(0, 0, 0), towerJudgeIdx * float.Pi / 8);
                        dp0.Delay = 0;
                        dp0.DestoryAt = 6500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);
                    }
                    towerPlayerIdxCount++;
                }
                towerJudgeIdx++;
                if (towerJudgeIdx == HeavensFallTowerPos.Count())
                    towerJudgeIdx = 0;
            }
        });
    }

    [ScriptMethod(name: "P3 Bahamut: [Heavensfall] Center Tower and Knockback Indicator", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9911"])]
    public void HeavensFallMiddleTower(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = "Heavensfall";
        dp.Scale = new(4);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Position = new(0, 0, 0);
        dp.DestoryAt = 5000;
        dp.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp2.Name = "HeavensfallKnockback";
        dp2.Scale = new(1.5f, 12);
        dp2.Owner = accessory.Data.Me;
        dp2.TargetPosition = new(0, 0, 0);
        dp2.Rotation = float.Pi;
        dp2.Color = accessory.Data.DefaultSafeColor;
        dp2.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp2);
    }

    #endregion
    #region P3: Bahamut (Tenstrike)

    [ScriptMethod(name: "P3 Bahamut: [Tenstrike] Phase Change Record (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9958"], userControl: false)]
    public void P3_PhaseChange_5th(Event @event, ScriptAccessory accessory)
    {
        // if (phase != UCOB_Phase.Heavensfall_4th) return;
        phase = UCOB_Phase.Tenstrike_5th;
    }

    [ScriptMethod(name: "P3 Bahamut: [Tenstrike] Sequential Meteor Stream Warning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9958"])]
    public void TenStrikeMeteorStream(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(7500).ContinueWith(t =>
        {
            if (phase != UCOB_Phase.Tenstrike_5th) return;
            for (var i = 0; i < 8; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"MeteorStream{i}";
                dp.Scale = new(4);
                dp.Owner = accessory.Data.PartyList[i];
                dp.DestoryAt = 15000;
                dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        });
    }

    [ScriptMethod(name: "P3 Bahamut: [Tenstrike] Center-Restrictor-Edge Three-Point Line Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9958"])]
    public void TenStrikeOrbTargetRoute(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(8500).ContinueWith(t =>
        {
            if (phase != UCOB_Phase.Tenstrike_5th) return;
            var MyIndex = getMyIndex(accessory);
            for (var i = 0; i < 3; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"ThreePointLine{i}";
                dp.Scale = new(2f);
                dp.ScaleMode = ScaleMode.YByDistance;
                dp.Position = new(0, 0, 0);
                dp.TargetPosition = new(RestrictorPos[i].X / RestrictorPos[i].Length() * 24, 0, RestrictorPos[i].Z / RestrictorPos[i].Length() * 24);
                dp.DestoryAt = 10000;
                dp.Color = isTenStrikeTarget ? posColorPlayer.V4.WithW(2f) : colorRed.V4.WithW(2f);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
            }
        });
    }

    [ScriptMethod(name: "P3 Bahamut: [Tenstrike] Sequential Meteor Stream Warning Removal (Uncontrollable)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:9920", "TargetIndex:1"], userControl: false)]
    public void MB_TyrantsFire(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.Tenstrike_5th) return;
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        var tidx = getPlayerIdIndex(accessory, tid);
        accessory.Method.RemoveDraw($"MeteorStream{tidx}");
    }

    [ScriptMethod(name: "P3 Bahamut: [Tenstrike] Orb Collision/Interception Message and Waypoint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9902"])]
    public void TenStrikeBlackOrbDir(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        if (phase != UCOB_Phase.Tenstrike_5th) return;
        if (TenStrikeBlackOrbDrawn) return;

        int trueCount = 0;
        for (int i = 0; i < GenerateTarget.Count(); i++)
        {
            if (GenerateTarget[i])
                trueCount++;
        }
        if (trueCount != 3) return;
        TenStrikeBlackOrbDrawn = true;

        // Calculate the number of players per black orb group and show message
        List<int> TenStrikeBlackOrbGroupTargetNum = calcTenStrikeGroupTargetNum(GenerateTarget);
        if (DebugMode)
        {
            string tenStrikeTargetNum = string.Join(", ", TenStrikeBlackOrbGroupTargetNum);
            accessory.Method.SendChat($"/e Number of black orb players per group: {tenStrikeTargetNum}");
        }

        if (showGlobalTenStrikeBlackOrbMsg)
            showTenStrikeBlackOrbMsg(TenStrikeBlackOrbGroupTargetNum, accessory);

        // Calculate collision and interception priorities, output mission list
        // Note: Interception priority has no unique rule, needs flexible handling based on situation.
        List<int> missionList = judgeTenStrikeBlackOrbRoute(GenerateTarget);
        if (showGlobalTenStrikeBlackOrbMsg)
        {
            string mission_record = "";
            mission_record += $"H1 Group: Collision {getPlayerJobIndexByIdx(missionList[0])}, Interception {getPlayerJobIndexByIdx(missionList[1])}\n";
            mission_record += $"H2 Group: Collision {getPlayerJobIndexByIdx(missionList[2])}, Interception {getPlayerJobIndexByIdx(missionList[3])}\n";
            mission_record += $"D3D4 Group: Collision {getPlayerJobIndexByIdx(missionList[4])}, Interception {getPlayerJobIndexByIdx(missionList[5])}\n";
            accessory.Method.SendChat($"/e {mission_record}");
        }

        int routeDestoryTime1 = 5000;   // First waypoint duration & second waypoint appearance time
        int routeDestoryTime2 = 5000;   // Second waypoint (interceptor) duration
        // Restrictor index: 0 for H1 group D, 1 for D3D4 group C, 2 for H2 group B
        drawBlackOrbRoute(missionList[0], 0, 0, routeDestoryTime1, true, accessory);
        drawBlackOrbRoute(missionList[1], 0, 0, routeDestoryTime1, false, accessory);
        drawBlackOrbRoute(missionList[1], 0, routeDestoryTime1, routeDestoryTime2, true, accessory);
        drawBlackOrbRoute(missionList[2], 2, 0, routeDestoryTime1, true, accessory);
        drawBlackOrbRoute(missionList[3], 2, 0, routeDestoryTime1, false, accessory);
        drawBlackOrbRoute(missionList[3], 2, routeDestoryTime1, routeDestoryTime2, true, accessory);
        drawBlackOrbRoute(missionList[4], 1, 0, routeDestoryTime1, true, accessory);
        drawBlackOrbRoute(missionList[5], 1, 0, routeDestoryTime1, false, accessory);
        drawBlackOrbRoute(missionList[5], 1, routeDestoryTime1, routeDestoryTime2, true, accessory);
    }

    private List<int> calcTenStrikeGroupTargetNum(List<bool> targets)
    {
        List<int> TenStrikeBlackOrbGroupTargetNum = [0, 0, 0];
        for (int i = 0; i < 8; i++)
        {
            if (targets[i])
            {
                if (i == 0 | i == 2 | i == 4)
                    TenStrikeBlackOrbGroupTargetNum[0]++;
                else if (i == 1 | i == 3 | i == 5)
                    TenStrikeBlackOrbGroupTargetNum[1]++;
                else if (i == 6 | i == 7)
                    TenStrikeBlackOrbGroupTargetNum[2]++;
            }
        }
        return TenStrikeBlackOrbGroupTargetNum;
    }

    private void showTenStrikeBlackOrbMsg(List<int> targetNum, ScriptAccessory accessory)
    {
        // TODO Confirm message duration
        int msgDuration = 8000;
        switch (targetNum)
        {
            case [1, 1, 1]:
                accessory.Method.TextInfo($"1 black orb per group, no swap needed", msgDuration, false);
                break;
            case [2, 1, 0]:
                accessory.Method.TextInfo($"ã€H1 Groupã€‘: 2 black orbs, swap to ã€D3D4 Groupã€‘", msgDuration, true);
                break;
            case [2, 0, 1]:
                accessory.Method.TextInfo($"ã€H1 Groupã€‘: 2 black orbs, swap to ã€H2 Groupã€‘", msgDuration, true);
                break;
            case [1, 2, 0]:
                accessory.Method.TextInfo($"ã€H2 Groupã€‘: 2 black orbs, swap to ã€D3D4 Groupã€‘", msgDuration, true);
                break;
            case [0, 2, 1]:
                accessory.Method.TextInfo($"ã€H2 Groupã€‘: 2 black orbs, swap to ã€H1 Groupã€‘", msgDuration, true);
                break;
            case [1, 0, 2]:
                accessory.Method.TextInfo($"ã€D3D4 Groupã€‘: 2 black orbs, swap to ã€H2 Groupã€‘\nã€H1/H2 Groupã€‘: Assist interception for ã€D3D4 Groupã€‘", msgDuration, true);
                break;
            case [0, 1, 2]:
                accessory.Method.TextInfo($"ã€D3D4 Groupã€‘: 2 black orbs, swap to ã€H1 Groupã€‘\nã€H1/H2 Groupã€‘: Assist interception for ã€D3D4 Groupã€‘", msgDuration, true);
                break;
            case [3, 0, 0]:
                accessory.Method.TextInfo($"ã€H1 Groupã€‘: 3 black orbs, swap to other groups\nã€H2/D3D4 Groupã€‘: Assist interception for ã€H1 Groupã€‘", msgDuration, true);
                break;
            case [0, 3, 0]:
                accessory.Method.TextInfo($"ã€H2 Groupã€‘: 3 black orbs, swap to other groups\nã€H1/D3D4 Groupã€‘: Assist interception for ã€H2 Groupã€‘", msgDuration, true);
                break;
        }
    }

    private List<int> judgeTenStrikeBlackOrbRoute(List<bool> targets)
    {
        // Calculate mission assignment
        // [H1 group restrictor collision, interception, H2 group restrictor collision, interception, D3D4 group restrictor collision, interception]
        List<int> missionList = [-1, -1, -1, -1, -1, -1];

        // Priorities for each group's collision and interception
        // H1 MT D1 D3 ST D4 D2 H2
        List<int> priority_group1 = [2, 0, 4, 6, 1, 7, 5, 3];
        // H2 ST D2 D4 MT D3 D1 H1
        List<int> priority_group2 = [3, 1, 5, 7, 0, 6, 4, 2];
        // D3 D4 D1 D2 MT ST H1 H2
        List<int> priority_group3 = [6, 7, 4, 5, 0, 1, 2, 3];

        // First round: determine collision players
        for (int i = 0; i < 8; i++)
        {
            // If target is marked by black orb AND target not already in mission list AND corresponding black orb position target not yet determined
            if (targets[priority_group1[i]] && !missionList.Contains(priority_group1[i]) && missionList[0] == -1)
                missionList[0] = priority_group1[i];
            if (targets[priority_group2[i]] && !missionList.Contains(priority_group2[i]) && missionList[2] == -1)
                missionList[2] = priority_group2[i];
            if (targets[priority_group3[i]] && !missionList.Contains(priority_group3[i]) && missionList[4] == -1)
                missionList[4] = priority_group3[i];
        }

        // Second round: determine interception players
        for (int i = 0; i < 8; i++)
        {
            // If target not marked by black orb AND target not already in mission list AND corresponding interception position target not yet determined
            if (!targets[priority_group1[i]] && !missionList.Contains(priority_group1[i]) && missionList[1] == -1)
                missionList[1] = priority_group1[i];
            if (!targets[priority_group2[i]] && !missionList.Contains(priority_group2[i]) && missionList[3] == -1)
                missionList[3] = priority_group2[i];
            if (!targets[priority_group3[i]] && !missionList.Contains(priority_group3[i]) && missionList[5] == -1)
                missionList[5] = priority_group3[i];
        }

        return missionList;
    }

    public static string getPlayerJobIndexByIdx(int idx)
    {
        switch (idx)
        {
            case 0: return "MT";
            case 1: return "ST";
            case 2: return "H1";
            case 3: return "H2";
            case 4: return "D1";
            case 5: return "D2";
            case 6: return "D3";
            case 7: return "D4";
            default: return "unknown";
        }
    }

    [ScriptMethod(name: "P3 Bahamut: [Tenstrike] Earth Shaker Waypoint", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0028"])]
    public void TenstrikeEarthShakerDir(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.Tenstrike_5th) return;
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        TenStrikeEarthShakerNum++;

        lock (this)
        {
            Task.Delay(100).ContinueWith(t =>
            {
                if (DebugMode)
                    accessory.Method.SendChat($"/e Detected TenStrikeEarthShakerNum: {TenStrikeEarthShakerNum}");

                if (TenStrikeEarthShakerNum != 4) return;

                if (tid == accessory.Data.Me)
                    isEarthShakerFirstRound = true;

                Vector3 safePosition = new(0, 0, -24);
                Vector3 safePosition1 = RotatePoint(safePosition, new(0, 0, 0), float.Pi / 180 * 80);
                Vector3 safePosition2 = RotatePoint(safePosition, new(0, 0, 0), -float.Pi / 180 * 80);
                Vector3 safePosition3 = RotatePoint(safePosition, new(0, 0, 0), float.Pi / 180 * 140);
                Vector3 safePosition4 = RotatePoint(safePosition, new(0, 0, 0), -float.Pi / 180 * 140);

                Vector3 safePosition5 = RotatePoint(safePosition, new(0, 0, 0), float.Pi / 180 * 40);
                Vector3 safePosition6 = RotatePoint(safePosition, new(0, 0, 0), -float.Pi / 180 * 40);
                Vector3 safePosition7 = RotatePoint(safePosition, new(0, 0, 0), float.Pi / 180 * 100);
                Vector3 safePosition8 = RotatePoint(safePosition, new(0, 0, 0), -float.Pi / 180 * 100);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "TenstrikeSafeSpotMarker";
                dp.Scale = new(1.5f);
                dp.ScaleMode = ScaleMode.YByDistance;
                dp.Color = posColorNormal.V4.WithW(3);
                dp.Position = new(0, 0, 0);
                dp.DestoryAt = 5000;

                if (isEarthShakerFirstRound)
                {
                    dp.Delay = 0;
                    dp.TargetPosition = safePosition1;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
                    dp.TargetPosition = safePosition2;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
                    dp.TargetPosition = safePosition3;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
                    dp.TargetPosition = safePosition4;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
                }

                else
                {
                    dp.Delay = 5000;
                    dp.TargetPosition = safePosition5;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
                    dp.TargetPosition = safePosition6;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
                    dp.TargetPosition = safePosition7;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
                    dp.TargetPosition = safePosition8;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
                }
            });
        }
    }

    #endregion
    #region P3: Bahamut (Grand Octet)

    [ScriptMethod(name: "P3 Bahamut: [Grand Octet] Phase Change Record (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9959"], userControl: false)]
    public void P3_PhaseChange_6th(Event @event, ScriptAccessory accessory)
    {
        // if (phase != UCOB_Phase.Heavensfall_5th) return;
        phase = UCOB_Phase.GrandOctet_6th;
    }

    [ScriptMethod(name: "P3 Bahamut: Twintania Position Record (Uncontrollable)", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:8159"], userControl: false)]
    public void TwintaniaPosRecord(Event @event, ScriptAccessory accessory)
    {
        var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        TwintaniaPosition = spos;
    }

    [ScriptMethod(name: "P3 Bahamut: Bahamut Position Record (Uncontrollable)", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:8168"], userControl: false)]
    public void BahamutPosRecord(Event @event, ScriptAccessory accessory)
    {
        var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        BahamutPosition = spos;
    }

    [ScriptMethod(name: "P3 Bahamut: [Grand Octet] Start Position Drawing", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:8168"])]
    public void GrandOctStartPosDir(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.GrandOctet_6th) return;
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;

        Task.Delay(100).ContinueWith(t =>
        {
            float CalculateDistance(Vector3 position) => new Vector2(position.X, position.Z).Length();
            if (CalculateDistance(BahamutPosition) < 23 || CalculateDistance(TwintaniaPosition) < 23 || CalculateDistance(NaelPosition) < 23) return;

            if (grandOctDrawn) return;

            if (DebugMode)
            {
                accessory.Method.SendChat($"/e Found Bahamut position, drawing at feet, {CalculateDistance(BahamutPosition)}");
                var dp0 = accessory.Data.GetDefaultDrawProperties();
                dp0.Name = $"BahamutPosition";
                dp0.Scale = new(3f);
                dp0.Color = posColorNormal.V4;
                dp0.Position = BahamutPosition;
                dp0.Delay = 0;
                dp0.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp0);
            }

            var BahamutDir = PositionTo8Dir(BahamutPosition, new(0, 0, 0));
            var NaelDir = PositionTo8Dir(NaelPosition, new(0, 0, 0));

            bool isAdvanced = Math.Abs(BahamutDir - NaelDir) == 4;  // Nael is opposite Bahamut, move one step ahead
            bool isTurnLeft = BahamutDir % 2 == 0;                  // Bahamut facing even (cardinal), run left towards outer edge
            var startDir = BahamutDir > 3 ? BahamutDir - 4 : BahamutDir + 4;
            if (isAdvanced)
            {
                startDir = isTurnLeft ? startDir - 1 : startDir + 1;
            }

            startDir = (startDir == -1) ? 7 : (startDir == 8) ? 0 : startDir;

            if (DebugMode)
            {
                accessory.Method.SendChat($"/e Starting position: {startDir}");
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"GrandOctetStartPosition";
            dp.Scale = new(5f);
            dp.Color = posColorPlayer.V4.WithW(3);
            dp.Position = RotatePoint(new Vector3(0, 0, -23), new Vector3(0, 0, 0), startDir * float.Pi / 4);
            dp.Delay = 0;
            dp.DestoryAt = 6500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"GrandOctetStartWaypoint-Danger";
            dp.Scale = new(0.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = RotatePoint(new Vector3(0, 0, -23), new Vector3(0, 0, 0), startDir * float.Pi / 4);
            dp.Delay = 0;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            accessory.Method.TextInfo("Wait for markers to appear, then go to start point", 3000);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"GrandOctetStartWaypoint-Safe";
            dp.Scale = new(0.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = RotatePoint(new Vector3(0, 0, -23), new Vector3(0, 0, 0), startDir * float.Pi / 4);
            dp.Delay = 5000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            grandOctDrawn = true;
        });
    }

    [ScriptMethod(name: "P3 Bahamut: [Grand Octet] Target Record (Uncontrollable)", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(0077|0029|0014)$"], userControl: false)]
    public void GrandOctTargetRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.GrandOctet_6th) return;
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        grandOctIconNum++;
        grandOctTargetChosen[getPlayerIdIndex(accessory, tid)] = true;
    }

    [ScriptMethod(name: "P3 Bahamut: [Grand Octet] Start Mention", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9923"])]
    public void GrandOctStartMention(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.GrandOctet_6th) return;

        var BahamutDir = PositionTo8Dir(BahamutPosition, new(0, 0, 0));
        bool isTurnLeft = BahamutDir % 2 == 0;
        if (isTurnLeft)
            accessory.Method.TextInfo("After Nael dives, run to the ã€LEFTã€‘ facing outward", 3000);
        else
            accessory.Method.TextInfo("After Nael dives, run to the ã€RIGHTã€‘ facing outward", 3000);
    }

    [ScriptMethod(name: "P3 Bahamut: [Grand Octet] Return to Center, Twintania Position and Guidance", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0029"])]
    public void GrandOctTarget(Event @event, ScriptAccessory accessory)
    {
        if (phase != UCOB_Phase.GrandOctet_6th) return;
        Task.Delay(100).ContinueWith(t =>
        {
            if (grandOctIconNum != 7) return;
            var MyIndex = getMyIndex(accessory);
            var TwintaniaTargetIdx = 0;
            for (int i = 0; i < grandOctTargetChosen.Count(); i++)
            {
                if (!grandOctTargetChosen[i])  // Only care about indices with value false
                    TwintaniaTargetIdx = i;
            }
            if (MyIndex == TwintaniaTargetIdx)
            {
                accessory.Method.TextInfo("Return to center, prepare to guide Twintania", 3000, true);
            }
            else
            {
                accessory.Method.TextInfo("Return to center, look for Twintania, watch for marker", 3000, true);
            }

            if (showOtherCauterizeRoute || MyIndex == TwintaniaTargetIdx)
            {
                var dp0 = accessory.Data.GetDefaultDrawProperties();
                dp0.Name = $"GrandOctetTwintaniaDiveGuidance";
                dp0.Scale = new(8, 45);
                dp0.Position = TwintaniaPosition;
                dp0.TargetObject = accessory.Data.PartyList[TwintaniaTargetIdx];
                dp0.Delay = 3500;
                dp0.DestoryAt = 5500;
                dp0.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp0);
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"GrandOctetTwintaniaPositionMarker";
            dp.Scale = new(1.5f);
            dp.ScaleMode = ScaleMode.YByDistance;
            dp.Color = posColorNormal.V4.WithW(2);
            dp.Position = new(0, 0, 0);
            dp.TargetPosition = TwintaniaPosition;
            dp.Delay = 2000;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

        });
    }

    #endregion

    #region P4: Bahamut Favor (Megaflare)

    [ScriptMethod(name: "P4 Bahamut Favor: Phase Change Record (Uncontrollable)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:9960", "TargetIndex:2"], userControl: false)]
    public void P4_PhaseChange(Event @event, ScriptAccessory accessory)
    {
        phase = UCOB_Phase.BahamutFavor;
        if (DebugMode)
        {
            accessory.Method.SendChat($"/e Detected entering P4.");
        }
    }
    private void drawBlackOrbRoute(int pidx, int residx, int delay, int destoryAt, bool isSafe, ScriptAccessory accessory)
    {
        // pidx: player index
        // residx: restrictor index
        if (pidx != getMyIndex(accessory)) return;

        if (phase == UCOB_Phase.BahamutFavor)
        {
            if (residx == 0)
                accessory.Method.TextInfo("First dodge twister, then hit orb", 3000, false);
            else
                accessory.Method.TextInfo("First hit orb, then dodge twister", 3000, false);
        }

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"RestrictorPositionMarker{residx}";
        dp.Scale = new(1.5f);
        dp.Position = RestrictorPos[residx];
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        dp.Color = posColorPlayer.V4.WithW(3);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Scale = new(0.5f);
        dp.Name = $"RestrictorOrbPlayer{pidx}Waypoint";
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = RestrictorPos[residx];
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = isSafe ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "P4 Bahamut Favor: Restrictor Waypoint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9902"])]
    public void BlackOrbDir(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        if (phase != UCOB_Phase.BahamutFavor) return;
        int trueCount = 0;
        for (int i = 0; i < GenerateTarget.Count(); i++)
        {
            if (GenerateTarget[i])
                trueCount++;
        }
        if (trueCount != 3) return;

        // Check if 4/5/6 are true, if so DoSomething(4/5/6)
        // P4 black orbs: D1 (B), D2 (C), D3 (D), D4 (assist)
        if (GenerateTarget[4])
            drawBlackOrbRoute(4, 2, 0, 5500, true, accessory);
        else
            drawBlackOrbRoute(7, 2, 0, 5500, true, accessory);

        if (GenerateTarget[5])
            drawBlackOrbRoute(5, 1, 0, 5500, true, accessory);
        else
            drawBlackOrbRoute(7, 1, 0, 5500, true, accessory);

        if (GenerateTarget[6])
            drawBlackOrbRoute(6, 0, 0, 5500, true, accessory);
        else
            drawBlackOrbRoute(7, 0, 0, 5500, true, accessory);
    }

    private void drawBahamutFavorNorth(int delay, int destoryAt, ScriptAccessory accessory)
    {
        switch (BahamutFavorNorth)
        {
            case BahamutFavorNorthTypeEnum.Draw12OClock_ShowTempNorth:
                drawBahamutFavorTempNorth(delay, destoryAt, accessory);
                break;
            case BahamutFavorNorthTypeEnum.Draw8SpreadDirections_ShowRecomDir:
                drawBahamutFavorSpreadDir(delay, destoryAt, accessory);
                break;
            case BahamutFavorNorthTypeEnum.DontDraw:
            default:
                return;
        }
    }

    private void drawBahamutFavorTempNorth(int delay, int destoryAt, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"P4_12OClockMarker";
        dp.Scale = new(1.5f);
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = posColorNormal.V4.WithW(2);
        dp.Position = new(0, 0, 0);
        dp.TargetPosition = BahamutFavorPos;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }

    private void drawBahamutFavorSpreadDir(int delay, int destoryAt, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        var initRad = FindRadian(new(0, 0, 0), BahamutFavorPos);
        List<float> angles = [-20, 20, -105, 105, -60, 60, -150, 150];

        for (int i = 0; i < 8; i++)
        {
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P4_SpreadDirectionMarker{i}";
            dp.Scale = new(1.25f);
            dp.ScaleMode = ScaleMode.YByDistance;
            dp.Color = i == getMyIndex(accessory) ? posColorPlayer.V4.WithW(5f) : posColorNormal.V4.WithW(5f);
            dp.Position = new(0, 0, 0);
            dp.TargetPosition = ExtendPoint(new Vector3(0, 0, 0), initRad + angle2Rad(angles[i]), 20);
            dp.Delay = delay;
            dp.DestoryAt = destoryAt;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }
    }

    [ScriptMethod(name: "P4 Nael: Quotes and Temporary North Direction Guide", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:regex:^(650[4567])$"])]
    public void NaelQuotesP4(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var quoteId = @event["Id"];

        switch (quoteId)
        {
            case "6504":
                {
                    // Iron, burn! Become the sword by which I tread!
                    // Iron, stack, spread
                    drawNaelQuote_Circle(sid, 0, 5000, accessory);
                    drawNaelQuote_Stack(sid, 5000, 3000, accessory);
                    // Indicate direction after stack
                    drawBahamutFavorNorth(8000, 8000, accessory);
                    drawNaelQuote_Spread(sid, 8000, 3000, accessory);
                    break;
                }
            case "6505":
                {
                    // Iron, become the burning sword by which I tread!
                    // Iron, spread, stack
                    drawNaelQuote_Circle(sid, 0, 5000, accessory);
                    drawNaelQuote_Spread(sid, 5000, 3000, accessory);
                    drawNaelQuote_Stack(sid, 8000, 3000, accessory);
                    // Indicate direction after stack
                    drawBahamutFavorNorth(11000, 5000, accessory);
                    break;
                }
            case "6506":
                {
                    // I come from the moon to tread upon the burning lands!
                    // Donut, spread, stack
                    drawNaelQuote_Donut(sid, 0, 5000, accessory);
                    drawNaelQuote_Spread(sid, 5000, 3000, accessory);
                    drawNaelQuote_Stack(sid, 8000, 3000, accessory);
                    // Indicate direction after stack
                    drawBahamutFavorNorth(11000, 5000, accessory);
                    break;
                }
            case "6507":
                {
                    // I come from the moon with iron!
                    // Donut, iron, spread
                    drawNaelQuote_Donut(sid, 0, 5000, accessory);
                    drawNaelQuote_Circle(sid, 5000, 3000, accessory);
                    // Indicate direction after stack
                    drawBahamutFavorNorth(8000, 8000, accessory);
                    drawNaelQuote_Spread(sid, 8000, 3000, accessory);
                    break;
                }
        }
    }

    #endregion

    #region P5: Flames of Rebirth

    [ScriptMethod(name: "P5 Golden: Phase Change Record (Uncontrollable)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9964"], userControl: false)]
    public void P5_PhaseChange(Event @event, ScriptAccessory accessory)
    {
        if (phase == UCOB_Phase.FlamesRebirth) return;
        phase = UCOB_Phase.FlamesRebirth;
    }

    [ScriptMethod(name: "P5 Golden: Morn Afah Stack Target", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9964"])]
    public void MornAfahStack(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        MornAfahNum++;

        if (showStackBusterNum)
            accessory.Method.TextInfo($"Morn Afah (Stack) #{MornAfahNum}", 4000, true);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"MornAfahStack";
        dp.Scale = new(4);
        dp.Owner = tid;
        dp.DestoryAt = 4000;
        dp.Color = accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "P5 Golden: Akh Morn Buster Target", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9962"])]
    public void AkhMornTankBuster(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        ArkMornNum++;

        if (showStackBusterNum)
            accessory.Method.TextInfo($"Akh Morn (Buster) #{ArkMornNum}", 4000, true);

        var MyIndex = getMyIndex(accessory);
        var isTank = MyIndex == 0 || MyIndex == 1;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"AkhMornBuster";
        dp.Scale = new(4);
        dp.Owner = tid;
        dp.DestoryAt = 4000;
        dp.Color = isTank ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "P5 Golden: Exaflare Warning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:9968"])]
    public void Exaflare(Event @event, ScriptAccessory accessory)
    {
        var srot = JsonConvert.DeserializeObject<float>(@event["SourceRotation"]);
        var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);

        for (int i = 0; i < 6; i++)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Exaflare{i}";
            dp.Scale = new(6);
            // dp.Rotation = srot;
            dp.ScaleMode = ScaleMode.ByTime;
            dp.Color = exflareColor.V4.WithW(3);
            dp.Position = ExtendPoint(spos, float.Pi - srot, 8 * i);
            dp.Delay = i == 0 ? 0 : 4000 + 1500 * (i - 1);
            dp.DestoryAt = i == 0 ? 4000 : 1500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            drawExflareWarn(1, i, spos, srot, accessory);
            drawExflareWarn(2, i, spos, srot, accessory);
        }
    }

    private void drawExflareWarn(uint idx, int iter_i, Vector3 spos, float srot, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"ExaflareWarning{idx}-{iter_i}";
        dp.Scale = new(6);
        dp.Color = exflareWarnColor.V4.WithW(0.8f / idx);
        dp.Position = ExtendPoint(spos, float.Pi - srot, 8 * (iter_i + idx));
        dp.Delay = iter_i == 0 ? 0 : 4000 + 1500 * (iter_i - 1);
        dp.DestoryAt = 1500 * (idx - 1) + (iter_i == 0 ? 4000 : 1500);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    #endregion
}
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Network.Structures.InfoProxy;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs;
using KodakkuAssist.Module.Script.Type;

namespace UsamisScript.EndWalker.p8s;

[ScriptType(name: "P8S [Anabaseios The Savage 2]", territorys: [1088],
    guid: "97df6974-c726-4a00-9016-293c184adf5c", version: "0.0.0.7", author: "Usami", note: noteStr, updateInfo: UpdateInfo)]
public class p8s
{
    const string noteStr =
    """
    v0.0.0.7
    Chariot 1 set H2 in center, ST directly right.
    Duckism.
    """;

    private const string UpdateInfo =
    """
    1. Adapted to Kodakku 0.5.x.x
    """;

    [UserSetting("Debug Mode, turn off for non-development use")]
    public static bool DebugMode { get; set; } = false;

    [UserSetting("Enable [Party] chat macro for main boss Mechanic 1 tower")]
    public static bool HC1_ChatGuidance { get; set; } = false;

    [UserSetting("Position hint circle drawing - Normal color")]
    public static ScriptColor posColorNormal { get; set; } = new ScriptColor { V4 = new Vector4(1.0f, 1.0f, 1.0f, 1.0f) };

    [UserSetting("Position hint circle drawing - Player position color")]
    public static ScriptColor posColorPlayer { get; set; } = new ScriptColor { V4 = new Vector4(0.0f, 1.0f, 1.0f, 1.0f) };

    // Door Boss record stack/spread
    bool db_isStack;
    // Door Boss blue tether Torch Flame count
    int db_torchFlameNum;
    int db_illusorySunforgeTimes;
    int db_gorgonIdx;
    int db_gorgonPartnerIdx;

    uint db_gorgonTarget;
    int db_gorgonTargetPos;

    List<uint> db_upliftOrder = [];
    List<uint> db_flareTarget = [];
    bool[] db_isFirstRound = [false, false, false, false, false, false, false, false];
    bool[] db_isGorgonEye = [false, false, false, false, false, false, false, false];
    List<int> db_GorgonPosition = [];
    List<uint> db_GorgonSid = [];

    public enum MB_Phase
    {
        Opening,
        NA1,
        HC1,
        LD,
        NA2,
        HC2,
    }
    MB_Phase mb_phase = MB_Phase.Opening;
    static bool mb_isLeftCleave = false;   // Main boss, is left half cleave
    // Main boss, is purple circle target
    bool[] mb_isNATarget = [false, false, false, false, false, false, false, false];

    // Main boss Mechanic 1 corresponding buff targets
    // 2-player stack, 3-player stack, short alpha, long alpha, short beta, long beta, short gamma, long gamma
    List<uint> mb_hc1_sid = [0, 0, 0, 0, 0, 0, 0, 0];
    uint mb_sideCleaveNum;
    uint mb_conceptFinNum;
    // string? mb_towerColor;
    string? mb_mentionTxt;
    Vector3 mb_TwoStackDestination = default;
    Vector3 mb_ThreeStackDestination = default;
    Vector3 mb_UnmergeDestination = default;
    bool[] mb_joinMerge = [false, false, false];

    uint mb_alphaLongFollower;
    uint mb_betaLongFollower;
    uint mb_gammaLongFollower;
    bool mb_NA1_isTNFixed = false;
    bool mb_NA1_isLine1Safe = false;
    List<int> mb_LD_playerOrder = [];       // Main boss Limitless Desolation phase, player vulnerability order
    List<LD_Tower> mb_LD_towerOrder = [];   // Main boss Limitless Desolation phase, tower order

    public void Init(ScriptAccessory accessory)
    {
        db_torchFlameNum = 0;
        db_illusorySunforgeTimes = 0;
        db_gorgonIdx = 0;
        db_gorgonPartnerIdx = -1;
        db_isStack = false;
        db_gorgonTarget = 0;
        db_gorgonTargetPos = 0;
        bool[] db_isFirstRound = [false, false, false, false, false, false, false, false];
        bool[] db_isGorgonEye = [false, false, false, false, false, false, false, false];

        db_upliftOrder = [];
        db_flareTarget = [];
        db_GorgonPosition = [];
        db_GorgonSid = [];

        mb_phase = MB_Phase.Opening;

        mb_isLeftCleave = false;   // Main boss, is left half cleave
        mb_isNATarget = [false, false, false, false, false, false, false, false];

        mb_hc1_sid = [0, 0, 0, 0, 0, 0, 0, 0];
        mb_sideCleaveNum = 0;
        mb_conceptFinNum = 0;
        mb_mentionTxt = "Hello Koda!";

        mb_TwoStackDestination = default;
        mb_ThreeStackDestination = default;
        mb_UnmergeDestination = default;
        mb_joinMerge = [false, false, false];

        mb_alphaLongFollower = 0;
        mb_betaLongFollower = 0;
        mb_gammaLongFollower = 0;
        mb_NA1_isTNFixed = false;
        mb_NA1_isLine1Safe = false;

        mb_LD_playerOrder = [];         // Main boss Limitless Desolation phase, player vulnerability order
        mb_LD_towerOrder = [];          // Main boss Limitless Desolation phase, tower order

        accessory.Method.RemoveDraw(".*");
    }
    public static void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!DebugMode) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    [ScriptMethod(name: "General Debug Use", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:=TST"], userControl: false)]
    public void EchoDebug(Event @event, ScriptAccessory accessory)
    {
        if (!DebugMode) return;
        var msg = @event["Message"].ToString();
        accessory.Method.SendChat($"/e Received player message: {msg}");

        mb_mentionTxt = "Line1";
        accessory.Method.SendChat($"/e {mb_mentionTxt}");
        mb_mentionTxt = "Line2";
        accessory.Method.SendChat($"/e {mb_mentionTxt}");
        mb_mentionTxt = "Line3";
        accessory.Method.SendChat($"/e {mb_mentionTxt}");

    }

    [ScriptMethod(name: "Anti-Knockback Remove Drawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(7559|7548|7389)$"], userControl: false)]
    // Surecast | Arm's Length | Thrill of Battle (?) 
    public void RemoveLine(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        if (sid == accessory.Data.Me)
        {
            accessory.Method.RemoveDraw("^(Anti-Knockback-.*)$");
            // DebugMsg($"/e Detected anti-knockback, removing knockback marker", accessory);
        }
    }

    #region Door Boss: Basics

    [ScriptMethod(name: "Door Boss: Record Spread/Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3099[67])$"], userControl: false)]
    public void DB_RecordSpreadAndStack(Event @event, ScriptAccessory accessory)
    {
        // 30996 Spread (Octuple Flare)
        // 30997 Stack (Quadruple Flare)
        db_isStack = @event.ActionId() == 30996;
        DebugMsg($"Recorded 【{(db_isStack ? "Spread" : "Stack")}】.", accessory);
    }

    [ScriptMethod(name: "Door Boss: Blue Tether Range", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31015"])]
    public void DB_TorchFlame(Event @event, ScriptAccessory accessory)
    {
        // if (db_torchFlameNum >= 12) return;
        // accessory.Method.SendChat($"/e db_torchFlameNum {db_torchFlameNum}...");

        db_torchFlameNum++;
        var spos = @event.SourcePosition();
        var dp = assignDp_TorchFlame(spos, 0, 10000, $"Torch Flame{db_torchFlameNum}", accessory);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    private static DrawPropertiesEdit assignDp_TorchFlame(Vector3 pos, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(10, 10);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = pos - new Vector3(0, 0, 5);
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    [ScriptMethod(name: "Door Boss: Dragon/Phoenix", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3099[45])$"])]
    public void DB_Sunforge(Event @event, ScriptAccessory accessory)
    {
        var epos = @event.EffectPosition();
        var srot = @event.SourceRotation();
        // var epos = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
        // var tpos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
        // var srot = JsonConvert.DeserializeObject<float>(@event["SourceRotation"]);
        // var isHotTail = @event["ActionId"] == "30994";

        var isDragon = @event.ActionId() == 30994;
        var isOpening = db_torchFlameNum == 12;

        // Opening will have blue fire first, then Dragon/Phoenix, so use two colors to distinguish.
        if (isDragon)
        {
            var dp = assignDp_DragonLine(epos, srot, 0, 7700, $"Dragon{db_torchFlameNum}", accessory);
            dp.Delay = isOpening ? 3000 : 0;
            dp.DestoryAt = isOpening ? 4700 : 7700;
            dp.Color = isOpening ? ColorHelper.DelayDangerColor.V4 : accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        else
        {
            var dp = assignDp_PhoenixWing(epos, srot, 0, 7700, $"Phoenix{db_torchFlameNum}", accessory);
            dp.Color = isOpening ? ColorHelper.DelayDangerColor.V4 : accessory.Data.DefaultDangerColor;
            dp.Delay = isOpening ? 3000 : 0;
            dp.DestoryAt = isOpening ? 4700 : 7700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        accessory.Method.TextInfo($"About to 【{(db_isStack ? "Spread" : "Stack")}】...", 8000, true);
    }

    private static DrawPropertiesEdit assignDp_DragonLine(Vector3 pos, float rot, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(14, 45);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = pos;
        dp.Rotation = rot;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }
    private static DrawPropertiesEdit assignDp_PhoenixWing(Vector3 pos, float rot, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(45, 20);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = pos;
        dp.Rotation = rot;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    [ScriptMethod(name: "Door Boss: Tank Buster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31045"])]
    public void DB_TankBuster(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        var tid = @event.TargetId();

        var dp1 = assignDp_TankBusterLine(sid, 0, 6000, $"Line Tank Buster - Target", accessory);
        dp1.TargetObject = tid;

        var dp2 = assignDp_TankBusterLine(sid, 0, 9000, $"Line Tank Buster - Top Enmity", accessory);
        dp2.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;

        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);
    }

    private static DrawPropertiesEdit assignDp_TankBusterLine(uint sid, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Owner = sid;
        dp.Scale = new(5, 40);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    [ScriptMethod(name: "Door Boss: Transformation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3105[12])$"])]
    public void DB_Reforge(Event @event, ScriptAccessory accessory)
    {
        var isSnake = @event.ActionId() == 31052;
        var sid = @event.SourceId();

        if (isSnake)
        {
            var dp = assignDp_ReforgeSnakeCircle(sid, 0, 11500, $"Snake Chariot", accessory);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        else
        {
            var dp = assignDp_ReforgeBeastKB(sid, 4000, 6000, $"Beast Knockback", accessory);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }
    }
    private static DrawPropertiesEdit assignDp_ReforgeSnakeCircle(uint sid, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(10);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = sid;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    private static DrawPropertiesEdit assignDp_ReforgeBeastKB(uint sid, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(2, 20);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = accessory.Data.Me;
        dp.Rotation = float.Pi;
        dp.TargetObject = sid;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    #endregion

    #region Door Boss: Chariot 1

    [ScriptMethod(name: "Door Boss: Chariot 1 Spread Hint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31027"])]
    public void DB_BeastSpread(Event @event, ScriptAccessory accessory)
    {
        for (int i = 0; i < accessory.Data.PartyList.Count(); i++)
        {
            var dp = AssignDp.drawCircle(accessory.Data.PartyList[i], 0, 13000, $"Chariot 1 Spread Warning{i}", accessory);
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        drawSpreadPos(accessory);
    }

    private static void drawSpreadPos(ScriptAccessory accessory)
    {
        Vector3[] safePos = new Vector3[8];
        safePos[0] = new Vector3(100, 0, 90);
        safePos[1] = new Vector3(110, 0, 100);
        safePos[2] = new Vector3(90, 0, 100);
        safePos[3] = new Vector3(100, 0, 100);
        safePos[4] = new Vector3(90, 0, 110);
        safePos[5] = new Vector3(110, 0, 110);
        safePos[6] = new Vector3(90, 0, 90);
        safePos[7] = new Vector3(110, 0, 90);

        var myIndex = IndexHelper.getMyIndex(accessory);

        for (int i = 0; i < 8; i++)
        {
            var dp = AssignDp.drawStatic(safePos[i], 0, 0, 6000, $"Chariot 1 Start Position{i}", accessory);
            dp.Scale = new(1.5f);
            dp.Color = myIndex == i ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        var dp0 = AssignDp.dirPos(safePos[myIndex], 0, 6000, $"Chariot 1 Start Guidance{myIndex}", accessory);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);
    }

    // Determine hit order
    [ScriptMethod(name: "Door Boss: Chariot 1 Hit Order Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31029"], userControl: false)]
    public void DB_UpliftRecord(Event @event, ScriptAccessory accessory)
    {
        var tid = @event.TargetId();
        if (@event.TargetIndex() != 1) return;
        db_upliftOrder.Add(tid);
    }

    // Determine bait order
    [ScriptMethod(name: "Door Boss: Chariot 1 Bait", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31030"])]
    public void DB_Stomp(Event @event, ScriptAccessory accessory)
    {
        if (db_upliftOrder.Count(x => x == accessory.Data.Me) > 1)
        {
            accessory.Method.TextInfo("Good luck...", 8000, true);
            return;
        }
        var myTurn = db_upliftOrder.IndexOf(accessory.Data.Me) / 2 + 1;
        drawBeastStompRouteDir(myTurn, accessory);
        switch (myTurn)
        {
            case 1: accessory.Method.TextInfo("Round 1, first 【Top-left 1 point bait】, then 【A/D dodge】", 8000, true); return;
            case 2: accessory.Method.TextInfo("Round 2, first 【Center bait】, then 【A/D dodge】", 8000, true); return;
            case 3: accessory.Method.TextInfo("Round 3, first 【A/D dodge】, then 【Top-left 1 point bait】", 8000, true); return;
            case 4: accessory.Method.TextInfo("Round 4, first 【A/D dodge】, then 【Center bait】", 8000, true); return;
            default: accessory.Method.TextInfo("It seems Hephaistos didn't notice you...", 8000, true); return;
        }
    }
    private static void drawBeastStompRouteDir(int myTurn, ScriptAccessory accessory)
    {
        // Top-left, Top-center, Left-center, Center
        Vector3[] beastStompPos = [new(90, 0, 90), new(100, 0, 90), new(90, 0, 100), new(100, 0, 100)];
        for (int i = 0; i < 4; i++)
        {
            var dp = AssignDp.drawStatic(beastStompPos[i], 0, 0, 8000, $"Chariot 1 Position{i}", accessory);
            dp.Scale = new(1.5f);
            dp.Color = posColorNormal.V4.WithW(0.6f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        List<int> stompIdx = myTurn switch
        {
            1 => new List<int> { 0, 1, 1, 1 },
            2 => new List<int> { 3, 3, 2, 2 },
            3 => new List<int> { 1, 0, 0, 1 },
            4 => new List<int> { 1, 1, 3, 3 },
            _ => new List<int> { -1, -1, -1, -1 }
        };

        List<int> delayTime = [0, 5250, 7750, 10250];
        List<int> destoryTime = [5250, 2500, 2500, 2500];

        for (int i = 0; i < 4; i++)
        {
            var dp_a = AssignDp.dirPos(beastStompPos[stompIdx[i]], delayTime[i], destoryTime[i], $"Guidance{i}", accessory);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp_a);

            if (i + 1 == 4) break;
            var dp_b = AssignDp.dirPos2Pos(beastStompPos[stompIdx[i]], beastStompPos[stompIdx[i + 1]], delayTime[i], destoryTime[i], $"Prepare Position{i}", accessory);
            dp_b.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp_b);
        }
    }

    #endregion

    #region Door Boss: Snake 1

    // 177.3 "Gorgomanteia" Ability { id: "791A", source: "Hephaistos" }
    // Record player buffs
    // 3004 BBC Mahjong 1
    // 3005 BBD Mahjong 2
    // 3351 D17 Petrification
    // 3326 CFE Poison
    [ScriptMethod(name: "Door Boss: Snake 1 Record Buffs", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3004|3005|3351|3326)$"], userControl: false)]
    public void DB_BRC_Gorgomanteia(Event @event, ScriptAccessory accessory)
    {
        var tid = @event.TargetId();
        var index = IndexHelper.getPlayerIdIndex(tid, accessory);
        if (index == -1) return;

        if (@event.StatusID() == 3004 || @event.StatusID() == 3005)
            db_isFirstRound[index] = @event.StatusID() == 3004;
        else
            db_isGorgonEye[index] = @event.StatusID() == 3351;

    }

    // May lose players during Snake 1 buff... dead players have no buff...
    [ScriptMethod(name: "Door Boss: Snake 1 Find Partner", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31018"], userControl: false)]
    public void DB_GorgonPartnerRecord(Event @event, ScriptAccessory accessory)
    {
        var MyIndex = IndexHelper.getMyIndex(accessory);
        for (int i = 0; i < 8; i++)
        {
            // Among the 8 players, find: same round as me, same buff type as me, and not myself
            bool isSameRound = db_isFirstRound[i] == db_isFirstRound[MyIndex];
            bool isSameGorgonEye = db_isGorgonEye[i] == db_isGorgonEye[MyIndex];
            bool isNotMyself = i != MyIndex;

            if (isSameRound && isSameGorgonEye && isNotMyself)
            {
                db_gorgonPartnerIdx = i;
                DebugMsg($"Found your partner: {IndexHelper.getPlayerJobByIndex(i)}", accessory);
                break;  // Found a qualifying teammate, exit loop
            }
        }
    }

    [ScriptMethod(name: "Door Boss: Snake 1 Record Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31019"], userControl: false)]
    public void DB_GorgonPositionRecord(Event @event, ScriptAccessory accessory)
    {
        var spos = @event.SourcePosition();
        var sid = @event.SourceId();
        // db_GorgonPosition and db_GorgonSid, same index records the same snake's SourceID and 8-direction logical direction
        db_GorgonPosition.Add(DirectionCalc.PositionRoundToDirs(spos, new(100, 0, 100), 8));
        db_GorgonSid.Add(sid);
    }

    [ScriptMethod(name: "Door Boss: Snake 1 Gaze Range", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31019"])]
    public void DB_Petrifaction(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        var spos = @event.SourcePosition();

        // Draw snake spawn point
        var dp1 = assignDp_SnakeAppearPos(spos, 0, 10000, $"Snake{sid} Spawn Point", accessory);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);

        // Draw look away from snake
        var dp2 = assignDp_SnakeSightAvoid(spos, 0, 10000, $"Snake{sid} Look Away", accessory);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.SightAvoid, dp2);
    }

    private static DrawPropertiesEdit assignDp_SnakeAppearPos(Vector3 pos, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(2);
        dp.Color = ColorHelper.GorgonColor.V4;
        dp.Position = pos;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    private static DrawPropertiesEdit assignDp_SnakeSightAvoid(Vector3 tpos, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = tpos;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    [ScriptMethod(name: "Door Boss: Snake 1 Priority Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31019"], userControl: false)]
    public void DB_GorgonPriority(Event @event, ScriptAccessory accessory)
    {
        lock (this)
        {
            // Count after snake casts petrification
            db_gorgonIdx++;
            var myIndex = IndexHelper.getMyIndex(accessory);
            if (db_gorgonIdx != 2 && db_gorgonIdx != 4) return;
            if (db_gorgonIdx == 2 && !db_isFirstRound[myIndex]) return;
            if (db_gorgonIdx == 4 && db_isFirstRound[myIndex]) return;

            // Find target snake
            // Determine priority with partner
            bool isHighPriority = myIndex < db_gorgonPartnerIdx ? true : false;
            DebugMsg($"My priority is 【{(isHighPriority ? "High" : "Low")}】", accessory);

            // accessory.Method.SendChat($"/e My priority is {(isHighPriority ? "High" : "Low")}...");
            uint gorgon_HighPriority;
            uint gorgon_LowPriority;
            int gorgon_HighPriorityPosition;
            int gorgon_LowPriorityPosition;

            // Set snake position priority
            if (db_isFirstRound[myIndex])
            {
                gorgon_HighPriorityPosition = db_GorgonPosition[0] < db_GorgonPosition[1] ? db_GorgonPosition[0] : db_GorgonPosition[1];
                gorgon_LowPriorityPosition = db_GorgonPosition[0] == gorgon_HighPriorityPosition ? db_GorgonPosition[1] : db_GorgonPosition[0];
                gorgon_HighPriority = db_GorgonPosition[0] < db_GorgonPosition[1] ? db_GorgonSid[0] : db_GorgonSid[1];
                gorgon_LowPriority = db_GorgonSid[0] == gorgon_HighPriority ? db_GorgonSid[1] : db_GorgonSid[0];
            }
            else
            {
                gorgon_HighPriorityPosition = db_GorgonPosition[2] < db_GorgonPosition[3] ? db_GorgonPosition[2] : db_GorgonPosition[3];
                gorgon_LowPriorityPosition = db_GorgonPosition[2] == gorgon_HighPriorityPosition ? db_GorgonPosition[3] : db_GorgonPosition[2];
                gorgon_HighPriority = db_GorgonPosition[2] < db_GorgonPosition[3] ? db_GorgonSid[2] : db_GorgonSid[3];
                gorgon_LowPriority = db_GorgonSid[2] == gorgon_HighPriority ? db_GorgonSid[3] : db_GorgonSid[2];
            }

            DebugMsg($"High priority snake at {gorgon_HighPriorityPosition}, low priority snake at {gorgon_LowPriorityPosition}", accessory);

            db_gorgonTarget = isHighPriority ? gorgon_HighPriority : gorgon_LowPriority;
            db_gorgonTargetPos = isHighPriority ? gorgon_HighPriorityPosition : gorgon_LowPriorityPosition;
        }
    }

    // 201.1 "Eye of the Gorgon 1" Ability { id: "792D", source: "Hephaistos" }
    // Player snake placement range hint
    [ScriptMethod(name: "Door Boss: Snake 1 Petrification Eye Direction", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31019"])]
    public void DB_EyeGorgon(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(50).ContinueWith(t =>
        {
            var myIndex = IndexHelper.getMyIndex(accessory);

            if (!db_isGorgonEye[myIndex]) return;
            if (db_gorgonIdx != 2 && db_gorgonIdx != 4) return;
            if (db_gorgonIdx == 2 && !db_isFirstRound[myIndex]) return;
            if (db_gorgonIdx == 4 && db_isFirstRound[myIndex]) return;

            // Draw petrification eye fan
            var dp1 = assignDp_GorgonEyeFan(0, 5000, $"Petrification Eye{db_gorgonIdx}", accessory);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp1);

            // Draw petrification eye target snake
            var dp2 = assignDp_SnakeTarget(db_gorgonTarget, 0, 10000, $"Snake Target{db_gorgonIdx}", accessory);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);

            var dp3 = AssignDp.dirTarget(db_gorgonTarget, 0, 5000, $"Direction to Target Snake{db_gorgonIdx}", accessory);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);

            // Send information
            string gorgon_target_position_txt = "Unknown Position";
            switch (db_gorgonTargetPos)
            {
                case 0: gorgon_target_position_txt = "Straight Up A"; break;
                case 1: gorgon_target_position_txt = "Top-Right 2"; break;
                case 2: gorgon_target_position_txt = "Straight Right B"; break;
                case 3: gorgon_target_position_txt = "Bottom-Right 3"; break;
                case 4: gorgon_target_position_txt = "Straight Down C"; break;
                case 5: gorgon_target_position_txt = "Bottom-Left 4"; break;
                case 6: gorgon_target_position_txt = "Straight Left D"; break;
                case 7: gorgon_target_position_txt = "Top-Left 1"; break;
            }
            accessory.Method.TextInfo($"Control the snake at 【{gorgon_target_position_txt}】...", 8000, true);
        });
    }
    private static DrawPropertiesEdit assignDp_GorgonEyeFan(int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(50);
        dp.Radian = float.Pi / 4;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = accessory.Data.Me;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    private static DrawPropertiesEdit assignDp_SnakeTarget(uint tid, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(2);
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Owner = tid;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    // 204.2 "Blood of the Gorgon 1" Ability { id: "792F", source: "Hephaistos" }
    // Player poison placement range hint
    [ScriptMethod(name: "Door Boss: Snake 1 Poison Direction", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31019"])]
    public void DB_BloodGorgon(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(50).ContinueWith(t =>
        {
            var myIndex = IndexHelper.getMyIndex(accessory);

            if (db_isGorgonEye[myIndex]) return;
            if (db_gorgonIdx != 2 && db_gorgonIdx != 4) return;
            if (db_gorgonIdx == 2 && !db_isFirstRound[myIndex]) return;
            if (db_gorgonIdx == 4 && db_isFirstRound[myIndex]) return;

            // Draw poison circle
            var dp1 = assignDp_PoisonCircle(accessory.Data.Me, 0, 5000, $"Poison Circle{db_gorgonIdx}", accessory);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);

            // Draw poison target snake
            var dp2 = assignDp_SnakeTarget(db_gorgonTarget, 0, 10000, $"Snake Target{db_gorgonIdx}", accessory);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);

            var dp3 = AssignDp.dirTarget(db_gorgonTarget, 0, 10000, $"Direction to Target Snake{db_gorgonIdx}", accessory);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);

            // Send information
            string gorgon_target_position_txt = "Unknown Position";
            switch (db_gorgonTargetPos)
            {
                case 0: gorgon_target_position_txt = "Straight Up A"; break;
                case 1: gorgon_target_position_txt = "Top-Right 2"; break;
                case 2: gorgon_target_position_txt = "Straight Right B"; break;
                case 3: gorgon_target_position_txt = "Bottom-Right 3"; break;
                case 4: gorgon_target_position_txt = "Straight Down C"; break;
                case 5: gorgon_target_position_txt = "Bottom-Left 4"; break;
                case 6: gorgon_target_position_txt = "Straight Left D"; break;
                case 7: gorgon_target_position_txt = "Top-Left 1"; break;
            }
            accessory.Method.TextInfo($"Use poison to stop the snake at 【{gorgon_target_position_txt}】...", 8000, true);
        });
    }

    private static DrawPropertiesEdit assignDp_PoisonCircle(uint owner_id, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(4);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = owner_id;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        return dp;
    }

    #endregion

    #region Door Boss: Illusion 1

    [ScriptMethod(name: "Door Boss: Illusory Dragon/Phoenix", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3105[89])$"])]
    public void DB_IllusorySunforge(Event @event, ScriptAccessory accessory)
    {
        db_illusorySunforgeTimes++;
        var epos = @event.EffectPosition();
        var srot = @event.SourceRotation();

        var isDragon = @event.ActionId() == 31058;

        if (isDragon)
        {
            var dp = assignDp_DragonLine(epos, srot, 0, 7700, $"Illusory Dragon", accessory);
            dp.Color = ColorHelper.DelayDangerColor.V4;
            dp.Delay = db_illusorySunforgeTimes < 3 ? 0 : 1000;
            dp.DestoryAt = 7500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        else
        {
            var dp = assignDp_PhoenixWing(epos, srot, 0, 7700, $"Illusory Phoenix", accessory);
            dp.Scale = new(90, 20);
            dp.Color = ColorHelper.DelayDangerColor.V4;
            dp.Delay = db_illusorySunforgeTimes < 3 ? 0 : 1000;
            dp.DestoryAt = 7500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
    }

    // Dragon/Phoenix and Spread
    [ScriptMethod(name: "Door Boss: Illusion 1 Spread", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31009"])]
    public void DB_ManifoldFlames(Event @event, ScriptAccessory accessory)
    {
        for (int i = 0; i < accessory.Data.PartyList.Count(); i++)
        {
            var dp = assignDp_IllusionSpread(accessory.Data.PartyList[i], 0, 6500, $"Illusion 1 Spread{i}", accessory);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    private static DrawPropertiesEdit assignDp_IllusionSpread(uint sid, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(5);
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    [ScriptMethod(name: "Door Boss: Illusion 1 Vulnerability Collection", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29390"], userControl: false)]
    public void DB_HemitheosFlare(Event @event, ScriptAccessory accessory)
    {
        var tid = @event.TargetId();
        db_flareTarget.Add(tid);
    }

    [ScriptMethod(name: "Door Boss: Illusion 1 Bait", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31009"])]
    public void DB_NestFlamevipers(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(6000).ContinueWith(t =>
        {
            var hasDebuff = db_flareTarget.Contains(accessory.Data.Me);
            var sid = @event.SourceId();

            for (uint i = 1; i < 5; i++)
            {
                var dp = AssignDp.drawTargetOrder(sid, i, 0, 4000, $"Illusion 1 Bait-{i}", accessory);
                dp.Scale = new(5, 40);
                dp.TargetOrderIndex = i;
                dp.Color = hasDebuff ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }

            if (hasDebuff)
                accessory.Method.TextInfo("Move far away to avoid bait", 4000, true);
            else
                accessory.Method.TextInfo("Move close to bait", 4000, true);
        });
    }

    [ScriptMethod(name: "Door Boss: Illusion 1 Stack/Spread", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3100[67])$"])]
    public void DB_EmergentFlare(Event @event, ScriptAccessory accessory)
    {
        var isSpread = @event.ActionId() == 31007;
        var sid = @event.SourceId();
        if (isSpread)
        {
            for (int i = 0; i < accessory.Data.PartyList.Count(); i++)
            {
                var dp = AssignDp.drawOwner2Target(sid, accessory.Data.PartyList[i], 0, 6000, $"Illusion 1 Spread Line{i}", accessory);
                dp.Scale = new(5, 40);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }
        else
        {
            int[] partner = [6, 7, 4, 5, 2, 3, 0, 1];
            var myIndex = IndexHelper.getMyIndex(accessory);
            for (int i = 0; i < 4; i++)
            {
                var ii = myIndex > 3 ? i + 4 : i;
                var dp = AssignDp.drawCircle(accessory.Data.PartyList[ii], 0, 6000, $"Illusion 1 Stack{ii}", accessory);
                dp.Scale = new(5);
                dp.Color = myIndex == ii || myIndex == partner[ii] ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }
    }
    #endregion


    #region Main Boss: Basics

    [ScriptMethod(name: "Main Boss: Half-Room Cleave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3119[12])$"])]
    public void MB_SideCleave(Event @event, ScriptAccessory accessory)
    {
        var isleft = @event.ActionId() == 31191;
        mb_isLeftCleave = isleft;
        var dp = AssignDp.drawStatic(new Vector3(100, 0, 100), float.Pi, 0, 5700, $"Main Boss Half-Room Cleave{isleft}", accessory);
        dp.Scale = new(20, 40);
        dp.Position = isleft ? new(90, 0, 80) : new(110, 0, 80);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Main Boss: Half-Room Cleave Increase Phase", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3119[12])$"], userControl: false)]
    public void MB_PhaseAdd(Event @event, ScriptAccessory accessory)
    {
        mb_sideCleaveNum++;
    }

    [ScriptMethod(name: "Main Boss: Phase (Natural Alignment Record)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31163"], userControl: false)]
    public void MB_PhaseChange_NA(Event @event, ScriptAccessory accessory)
    {
        mb_phase = mb_phase switch
        {
            MB_Phase.Opening => MB_Phase.NA1,
            MB_Phase.NA1 => MB_Phase.NA2,
            _ => MB_Phase.NA1
        };
    }

    #endregion

    #region Main Boss: Natural Alignment 1

    [ScriptMethod(name: "Main Boss: Natural Alignment 1 Purple Circle Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2552"], userControl: false)]
    public void MB_NaturalAlignment(Event @event, ScriptAccessory accessory)
    {
        if (mb_phase != MB_Phase.NA1) return;
        var tid = @event.TargetId();
        var tidx = accessory.Data.PartyList.IndexOf(tid);
        if (tidx == -1) return;
        mb_isNATarget[tidx] = true;

        if (mb_isNATarget.Count(x => x == true) != 2) return;
        // If purple circle targets are DPS, TN positions are fixed
        mb_NA1_isTNFixed = tidx > 3;

    }

    [ScriptMethod(name: "Main Boss: Yellow Circle Bait", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31369"])]
    public void MB_TyrantFlare(Event @event, ScriptAccessory accessory)
    {
        for (int i = 0; i < accessory.Data.PartyList.Count(); i++)
        {
            var dp = AssignDp.drawCircle(accessory.Data.PartyList[i], 0, 3000, $"Yellow Circle Bait{i}", accessory);
            dp.Scale = new(6);
            dp.ScaleMode = ScaleMode.ByTime;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
    // Force Cast, shows Stack/Spread or Ice/Fire sequence pattern
    [ScriptMethod(name: "Main Boss: Natural Alignment 1 Stack/Spread", eventType: EventTypeEnum.StatusAdd, eventCondition: ["Param:regex:^(48[02])$"])]
    public void MB_ForceStackSpread(Event @event, ScriptAccessory accessory)
    {
        if (mb_phase != MB_Phase.NA1) return;
        var isStackFirst = @event.Param() == 480;
        var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
        if (mb_isNATarget[myIndex])
            accessory.Method.TextInfo($"First 【{(isStackFirst ? "Avoid Stack" : "Spread")}】, then 【{(isStackFirst ? "Spread" : "Avoid Stack")}】", 5000, true);
        else
            accessory.Method.TextInfo($"First 【{(isStackFirst ? "Stack" : "Spread")}】, then 【{(isStackFirst ? "Spread" : "Stack")}】", 5000, true);

        // Draw spread
        drawForceSpread(isStackFirst, mb_isNATarget, accessory);
        drawSpreadDir(isStackFirst, myIndex, accessory);
        // Draw stack
        drawForceStack(isStackFirst, myIndex, mb_isNATarget[myIndex], accessory);
    }
    private static void drawForceSpread(bool isStackFirst, bool[] naTargetList, ScriptAccessory accessory)
    {
        var spreadTime = isStackFirst ? 6100 : 3000;

        for (int i = 0; i < accessory.Data.PartyList.Count(); i++)
        {
            if (naTargetList[i]) continue;
            var dp = AssignDp.drawCircle(accessory.Data.PartyList[i], spreadTime, spreadTime, $"Purple Circle Spread{i}", accessory);
            dp.Scale = new(6);
            dp.ScaleMode = ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    private static void drawForceStack(bool isStackFirst, int myIndex, bool isTarget, ScriptAccessory accessory)
    {
        var stackTime = isStackFirst ? 3000 : 6100;
        var stackOwner = myIndex < 4 ? accessory.Data.PartyList[myIndex + 4] : accessory.Data.PartyList[myIndex - 4];
        var owner_id = isTarget ? stackOwner : accessory.Data.Me;
        var dp = AssignDp.drawCircle(owner_id, stackTime, stackTime, $"Purple Circle Stack{myIndex}", accessory);
        dp.Scale = new(6);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Color = isTarget ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    private async static void drawSpreadDir(bool isStackFirst, int myIndex, ScriptAccessory accessory)
    {
        var spreadTime = isStackFirst ? 6100 : 3000;

        if (isStackFirst)
        {
            await Task.Delay(6500);
            DebugMsg($"Starting to draw spread positions", accessory);
            drawSpreadPos(mb_isLeftCleave, spreadTime - 400, myIndex, accessory);
        }
        else
            drawSpreadLine(spreadTime, myIndex, accessory);
        return;
    }

    private static void drawSpreadLine(int spreadTime, int myIndex, ScriptAccessory accessory)
    {
        Vector3[] safePos = new Vector3[8];
        safePos[0] = new Vector3(100, 0, 90);
        safePos[1] = new Vector3(110, 0, 100);
        safePos[2] = new Vector3(90, 0, 100);
        safePos[3] = new Vector3(100, 0, 100);
        safePos[4] = new Vector3(90, 0, 90);
        safePos[5] = new Vector3(110, 0, 90);
        safePos[6] = new Vector3(90, 0, 110);
        safePos[7] = new Vector3(110, 0, 110);

        for (int i = 0; i < 8; i++)
        {
            var dp = AssignDp.dirPos2Pos(new Vector3(100, 0, 100), safePos[i], spreadTime - 500, spreadTime + 500, $"Spread Direction{i}", accessory);
            dp.Scale = new(1.5f);
            dp.Color = myIndex == i ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }
    }

    private static void drawSpreadPos(bool isLeftCleave, int spreadTime, int myIndex, ScriptAccessory accessory)
    {
        Vector3[] safePos = new Vector3[8];
        // Right
        safePos[0] = new Vector3(100.5f, 0, 80);
        safePos[1] = new Vector3(110, 0, 80);
        safePos[2] = new Vector3(100.5f, 0, 90);
        safePos[3] = new Vector3(100.5f, 0, 100);
        safePos[4] = new Vector3(110, 0, 90);
        safePos[5] = new Vector3(110, 0, 100);
        safePos[6] = new Vector3(110, 0, 110);
        safePos[7] = new Vector3(100.5f, 0, 110);

        if (!isLeftCleave)
        {
            for (int i = 0; i < 8; i++)
            {
                safePos[i] = DirectionCalc.FoldPointLR(safePos[i], 100);
            }
        }

        for (int i = 0; i < 8; i++)
        {
            var dp0 = AssignDp.drawStatic(safePos[i], 0, 0, spreadTime, $"Spread Position{i}", accessory);
            dp0.Scale = new(1f);
            dp0.Color = myIndex == i ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp0);
        }

        var dp = AssignDp.dirPos(safePos[myIndex], 0, spreadTime, $"Spread Position Guidance{myIndex}", accessory);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    // 5076.4 "Forcible Trifire/Forcible Difreeze" Ability { id: ["79BD", "79BE"], source: "Hephaistos" }
    [ScriptMethod(name: "Main Boss: Natural Alignment 1 Ice/Fire Bait Range", eventType: EventTypeEnum.StatusAdd, eventCondition: ["Param:regex:^(47[68])$"])]
    public void MB_ForceFireFreeze(Event @event, ScriptAccessory accessory)
    {
        if (mb_phase != MB_Phase.NA1) return;
        var tid = @event.TargetId();
        var isFireFirst = @event.Param() == 476;
        var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

        var dp = accessory.Data.GetDefaultDrawProperties();
        // Two-player fire, three groups
        for (uint i = 1; i < 4; i++)
        {
            dp.Name = $"Purple Circle Fire-{i}";
            dp.Scale = new(5);
            dp.Owner = tid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            dp.CentreOrderIndex = i;
            dp.Color = mb_isNATarget[myIndex] ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
            dp.Delay = isFireFirst ? 0 : 6100;
            dp.DestoryAt = isFireFirst ? 6000 : 6100;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        // Three-player ice, two groups
        for (uint i = 1; i < 3; i++)
        {
            dp.Name = $"Purple Circle Ice-{i}";
            dp.Scale = new(5);
            dp.Owner = tid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = i + 1;
            dp.Color = mb_isNATarget[myIndex] ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
            dp.Delay = isFireFirst ? 6100 : 0;
            dp.DestoryAt = isFireFirst ? 6100 : 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    [ScriptMethod(name: "Main Boss: Illusion Cannon Determine Row", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:15079"], userControl: false)]
    public void MB_IllusionBeamLineIdx(Event @event, ScriptAccessory accessory)
    {
        var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        if (pos.Z > 100) return;
        // Only need to determine once, subsequent judgments are unaffected
        mb_NA1_isLine1Safe = pos.Z < 90 ? false : true;
    }

    [ScriptMethod(name: "Main Boss: Natural Alignment 1 Ice/Fire Guidance Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["Param:regex:^(47[68])$"])]
    public void MB_ForceFireFreezeGuide(Event @event, ScriptAccessory accessory)
    {
        if (mb_phase != MB_Phase.NA1) return;
        var isFireFirst = @event.Param() == 476;
        var myIndex = IndexHelper.getMyIndex(accessory);
        DebugMsg($"{(mb_NA1_isLine1Safe ? "First row safe first" : "Second row safe first")}", accessory);

        // I am TN and TN fixed, or I am DPS and DPS fixed
        var isFixed = (mb_NA1_isTNFixed && myIndex < 4) || (!mb_NA1_isTNFixed && myIndex >= 4);

        Vector3[] dxFixed = [new(-8.5f, 0, 0), new(-6.5f, 0, 0), new(6.5f, 0, 0), new(8.5f, 0, 0)];
        Vector3 destinationPoint1;
        Vector3 destinationPoint2;
        Vector3 dzFreeze = mb_NA1_isLine1Safe ? new(0, 0, -0.5f) : new(0, 0, 0.5f);
        Vector3 dzFire = mb_NA1_isLine1Safe ? new(0, 0, -9.5f) : new(0, 0, 9.5f);

        if (isFixed)
        {
            // Brainless fixed group
            var biasIdx = myIndex >= 4 ? myIndex - 4 : myIndex;
            if (myIndex < 4)
            {
                if (myIndex == 0) biasIdx = 0;
                else if (myIndex == 1) biasIdx = 3;
                else if (myIndex == 2) biasIdx = 1;
                else if (myIndex == 3) biasIdx = 2;
            }
            destinationPoint1 = (Vector3)new(100, 0, 90) + dxFixed[biasIdx] + dzFreeze;
            destinationPoint2 = (Vector3)new(100, 0, 90) + dxFixed[biasIdx] - dzFreeze;
            accessory.Method.TextInfo($"Brainless group, fixed position", 10000);
        }
        else if (mb_isNATarget[myIndex])
        {
            // Purple circle group
            destinationPoint1 = (Vector3)new(100, 0, 90) + dzFreeze;
            destinationPoint2 = (Vector3)new(100, 0, 90) - dzFreeze;
            accessory.Method.TextInfo($"Purple circle group, fixed center position", 10000);
        }
        else
        {
            // Brain-using group
            int biasIdx;
            if (myIndex < 4)
            {
                List<uint> TN_priority = new List<uint> { 1, 2, 0, 3 };
                List<bool> TN_NATarget = new List<bool> { mb_isNATarget[0], mb_isNATarget[1], mb_isNATarget[2], mb_isNATarget[3] };

                int firstFalseIdx = TN_NATarget.IndexOf(false);
                int lastFalseIdx = TN_NATarget.LastIndexOf(false);

                if (firstFalseIdx != -1 && lastFalseIdx != -1)
                {
                    bool firstFalseHigh = TN_priority[firstFalseIdx] < TN_priority[lastFalseIdx];
                    bool isFirstFalse = myIndex == firstFalseIdx;
                    biasIdx = ((isFirstFalse && firstFalseHigh) || (!isFirstFalse && !firstFalseHigh)) ? 0 : 3;
                }
                else
                {
                    // No false found, good luck
                    biasIdx = 3;
                }
            }
            else
            {
                // Find the last false from the end, if it's me, then I have low priority
                int lastFalseIdx = Array.LastIndexOf(mb_isNATarget, false);
                biasIdx = myIndex == lastFalseIdx ? 3 : 0;
            }

            destinationPoint1 = isFireFirst ? (Vector3)new(100, 0, 90) + dzFire : (Vector3)new(100, 0, 90) + dxFixed[biasIdx] + dzFreeze;
            destinationPoint2 = isFireFirst ? (Vector3)new(100, 0, 90) + dxFixed[biasIdx] - dzFreeze : (Vector3)new(100, 0, 90) - dzFire;
            accessory.Method.TextInfo($"Brain-using group, priority position", 10000);
        }

        var dp = accessory.Data.GetDefaultDrawProperties();

        dp.Name = $"Ice/Fire Guidance-1";
        dp.Scale = new(0.5f);
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = destinationPoint1;
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Delay = 0;
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Ice/Fire Guidance-2";
        dp.Scale = new(0.5f);
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = destinationPoint2;
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Delay = 6100;
        dp.DestoryAt = 6100;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

    }

    [ScriptMethod(name: "Main Boss: Illusion Cannon", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31371"])]
    public void MB_IllusionBeam(Event @event, ScriptAccessory accessory)
    {
        var sid = @event.SourceId();
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Illusion Cannon";
        dp.Scale = new(10, 50);
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 5700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    #endregion

    #region Main Boss: Mechanic 1

    [ScriptMethod(name: "Main Boss: High Concept", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31148"], userControl: false)]
    public void MB_HighConceptReady(Event @event, ScriptAccessory accessory)
    {
        mb_conceptFinNum = 0;
        mb_phase = mb_phase switch
        {
            MB_Phase.NA1 => MB_Phase.HC1,
            MB_Phase.NA2 => MB_Phase.HC2,
            _ => MB_Phase.HC1
        };
    }

    // 5118.6 "High Concept 1" Ability { id: "710A", source: "Hephaistos" } window 20,20
    // 3330 D02 = Imperfection: Alpha                                   BUFF
    // 3331 D03 = Imperfection: Beta
    // 3332 D04 = Imperfection: Gamma
    // 3333 D05 = Perfection: Alpha                                     Pattern
    // 3334 D06 = Perfection: Beta
    // 3335 D07 = Perfection: Gamma
    // 3336 D08 = Inconceivable (temporary after merging)               Forbidden to merge
    // 3337 D09 = Winged Conception (alpha + beta)                      Green Wind
    // 3338 D0A = Aquatic Conception (alpha + gamma)                    Blue Water
    // 3339 D0B = Shocking Conception (beta + gamma)                    Purple Thunder Horse
    // 3340 D0C = Fiery Conception (ifrits, alpha + alpha)              Ifrit
    // 3341 D0D = Toxic Conception (snake, beta + beta)                 Twin Snakes
    // 3342 D0E = Growing Conception (tree together, gamma + gamma)     Great Tree
    // 3343 D0F = Immortal Spark (feather)          Phoenix pre-feather
    // 3344 D10 = Immortal Conception (phoenix)     Phoenix
    // 3345 D11 = Solosplice     Single-player stack
    // 3346 D12 = Multisplice    Two-player stack
    // 3347 D13 = Supersplice    Three-player stack
    [ScriptMethod(name: "Main Boss: Mechanic 1 Record Buffs", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3346|3347|3331|3330|3332)$"], userControl: false)]
    public void MB_BRC_HighConcept1(Event @event, ScriptAccessory accessory)
    {
        if (mb_phase != MB_Phase.HC1) return;
        var tid = @event.TargetId();
        var dur = @event.DurationMilliseconds();
        // if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
        var isLong = dur > 9000;
        switch (@event["StatusID"])
        {
            case "3346":
                mb_hc1_sid[0] = tid;
                break;
            case "3347":
                mb_hc1_sid[1] = tid;
                break;
            case "3330":
                if (isLong)
                {
                    mb_hc1_sid[3] = tid;
                }
                else
                {
                    mb_hc1_sid[2] = tid;
                }
                break;
            case "3331":
                if (isLong)
                {
                    mb_hc1_sid[5] = tid;
                }
                else
                {
                    mb_hc1_sid[4] = tid;
                }
                break;
            case "3332":
                if (isLong)
                {
                    mb_hc1_sid[7] = tid;
                }
                else
                {
                    mb_hc1_sid[6] = tid;
                }
                break;
            default:
                break;
        }
    }

    [ScriptMethod(name: "Main Boss: Mechanic 1 Initial Position Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31148"])]
    public void MB_HC1_GuidePhase0(Event @event, ScriptAccessory accessory)
    {
        if (mb_phase != MB_Phase.HC1) return;
        Task.Delay(10900).ContinueWith(t =>
        {
            var myHCIndex = mb_hc1_sid.IndexOf(accessory.Data.Me);
            var dp = accessory.Data.GetDefaultDrawProperties();

            Vector3 destinationPoint;
            switch (myHCIndex)
            {
                case 0: destinationPoint = new(108, 0, 90); break;
                case 1: destinationPoint = new(100, 0, 100); break;
                case 2: destinationPoint = new(80, 0, 80); break;
                case 3: destinationPoint = new(108, 0, 90); break;
                case 4: destinationPoint = new(80, 0, 120); break;
                case 5: destinationPoint = new(100, 0, 100); break;
                case 6: destinationPoint = new(120, 0, 120); break;
                case 7: destinationPoint = new(100, 0, 100); break;
                default: destinationPoint = new(100, 0, 100); break;
            }

            dp.Name = $"Mechanic 1 Initial Guidance";
            dp.Scale = new(0.5f);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = destinationPoint;
            dp.ScaleMode = ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 0;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        });

    }

    [ScriptMethod(name: "Main Boss: Record Tower Color", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00020001"], userControl: false)]
    public void MB_TowerColorRecord(Event @event, ScriptAccessory accessory)
    {
        if (mb_phase != MB_Phase.HC1) return;
        lock (this)
        {
            // if (!int.TryParse(@event["Index"], System.Globalization.NumberStyles.HexNumber, null, out var tower_color)) return;
            var tower_color = @event.Index();
            mb_conceptFinNum++;
            // accessory.Method.SendChat($"/e mb_conceptFinNum = {mb_conceptFinNum}");

            if (mb_conceptFinNum == 2)
            {
                if (tower_color >= 26 && tower_color <= 35)
                {
                    // mb_towerColor = "purple";
                    mb_TwoStackDestination = new(90, 0, 110);
                    mb_ThreeStackDestination = new(110, 0, 110);
                    mb_UnmergeDestination = new(90, 0, 90);
                    mb_mentionTxt = "2-person → B, 3-person → C, Puzzle → A";
                    mb_joinMerge = [false, true, true];

                    mb_alphaLongFollower = mb_hc1_sid[2];
                    mb_betaLongFollower = mb_hc1_sid[0];
                    mb_gammaLongFollower = mb_hc1_sid[1];
                }
                else if (tower_color >= 36 && tower_color <= 45)
                {
                    // mb_towerColor = "blue";
                    mb_TwoStackDestination = new(90, 0, 90);
                    mb_ThreeStackDestination = new(110, 0, 110);
                    mb_UnmergeDestination = new(90, 0, 110);
                    mb_mentionTxt = "2-person → A, 3-person → C, Puzzle → B";
                    mb_joinMerge = [true, false, true];

                    mb_alphaLongFollower = mb_hc1_sid[0];
                    mb_betaLongFollower = mb_hc1_sid[4];
                    mb_gammaLongFollower = mb_hc1_sid[1];
                }
                else if (tower_color >= 46 && tower_color <= 55)
                {
                    // mb_towerColor = "green";
                    mb_TwoStackDestination = new(90, 0, 90);
                    mb_ThreeStackDestination = new(90, 0, 110);
                    mb_UnmergeDestination = new(110, 0, 110);
                    mb_mentionTxt = "2-person → A, 3-person → B, Puzzle → C";
                    mb_joinMerge = [true, true, false];

                    mb_alphaLongFollower = mb_hc1_sid[0];
                    mb_betaLongFollower = mb_hc1_sid[1];
                    mb_gammaLongFollower = mb_hc1_sid[6];
                }
                else return;

                if (HC1_ChatGuidance)
                {
                    accessory.Method.SendChat($"/p {mb_mentionTxt} <se.1>");
                }
                else
                {
                    accessory.Method.SendChat($"/e {mb_mentionTxt}");
                }

            }
            else if (mb_conceptFinNum == 6)
            {
                if (tower_color >= 26 && tower_color <= 35)
                {
                    // mb_towerColor = "purple";
                    mb_joinMerge = [false, true, true];
                }
                else if (tower_color >= 36 && tower_color <= 45)
                {
                    // mb_towerColor = "blue";
                    mb_joinMerge = [true, false, true];
                }
                else if (tower_color >= 46 && tower_color <= 55)
                {
                    // mb_towerColor = "green";
                    mb_joinMerge = [true, true, false];
                }
                else return;
            }
        }
    }

    [ScriptMethod(name: "Main Boss: Mechanic 1 First Merge Guidance", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00020001"])]
    public void MB_HC1_GuidePhase1(Event @event, ScriptAccessory accessory)
    {
        if (mb_phase != MB_Phase.HC1) return;
        Task.Delay(50).ContinueWith(t =>
        {
            if (mb_sideCleaveNum == 1 && mb_conceptFinNum == 2)
            {
                var myHCIndex = mb_hc1_sid.IndexOf(accessory.Data.Me);

                bool shouldJoinMerge = (myHCIndex == 2 && mb_joinMerge[0]) || (myHCIndex == 4 && mb_joinMerge[1]) || (myHCIndex == 6 && mb_joinMerge[2]);
                bool shouldAvoidMerge = (myHCIndex == 2 && !mb_joinMerge[0]) || (myHCIndex == 4 && !mb_joinMerge[1]) || (myHCIndex == 6 && !mb_joinMerge[2]);

                var dp0 = accessory.Data.GetDefaultDrawProperties();
                if (shouldJoinMerge)
                {
                    dp0.Name = "Participate in Merge Guidance";
                    dp0.Scale = new(0.5f);
                    dp0.Owner = accessory.Data.Me;
                    dp0.TargetPosition = new(100, 0, 100);
                    dp0.ScaleMode = ScaleMode.YByDistance;
                    dp0.Color = accessory.Data.DefaultSafeColor;
                    dp0.Delay = 0;
                    dp0.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp0);

                    dp0 = accessory.Data.GetDefaultDrawProperties();
                    dp0.Name = "Participate in Merge Area";
                    dp0.Scale = new(5);
                    dp0.Position = new(100, 0, 100);
                    dp0.Color = accessory.Data.DefaultSafeColor;
                    dp0.Delay = 0;
                    dp0.DestoryAt = 7000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp0);
                    accessory.Method.TextInfo($"Participate in merge", 5000);

                }
                else if (shouldAvoidMerge)
                {
                    dp0 = accessory.Data.GetDefaultDrawProperties();
                    dp0.Name = "Avoid Merge Area";
                    dp0.Scale = new(5);
                    dp0.Position = new(100, 0, 100);
                    dp0.Color = accessory.Data.DefaultDangerColor;
                    dp0.Delay = 0;
                    dp0.DestoryAt = 7000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp0);
                    accessory.Method.TextInfo($"Avoid merge", 5000, true);
                }
            }
        });
    }

    [ScriptMethod(name: "Main Boss: Mechanic 1 Post-Merge Position Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3119[12])$"])]
    public void MB_HC1_GuidePhase2(Event @event, ScriptAccessory accessory)
    {
        if (mb_phase != MB_Phase.HC1) return;
        if (mb_conceptFinNum != 2) return;
        var myHCIndex = mb_hc1_sid.IndexOf(accessory.Data.Me);
        bool joinedMerge = false;
        Vector3 safeDestination = new(110, 0, 90);

        var dp = accessory.Data.GetDefaultDrawProperties();
        switch (myHCIndex)
        {
            case 0:
                dp.TargetPosition = mb_TwoStackDestination;
                break;
            case 1:
                dp.TargetPosition = mb_ThreeStackDestination;
                break;
            case 2:
                joinedMerge = mb_joinMerge[0];
                dp.TargetPosition = joinedMerge ? safeDestination : mb_UnmergeDestination;
                break;
            case 3:
                dp.TargetPosition = new(80, 0, 80);
                break;
            case 4:
                joinedMerge = mb_joinMerge[1];
                dp.TargetPosition = joinedMerge ? safeDestination : mb_UnmergeDestination;
                break;
            case 5:
                dp.TargetPosition = new(80, 0, 120);
                break;
            case 6:
                joinedMerge = mb_joinMerge[2];
                dp.TargetPosition = joinedMerge ? safeDestination : mb_UnmergeDestination;
                break;
            case 7:
                dp.TargetPosition = new(120, 0, 120);
                break;
        }
        dp.Name = $"Mechanic 1 Phase 2 Guidance";
        dp.Scale = new(0.5f);
        dp.Owner = accessory.Data.Me;
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Delay = 3500;
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        if (joinedMerge)
        {
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Mechanic 1 Phase 2 Safe Zone";
            dp.Scale = new(8);
            dp.Position = safeDestination;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 3500;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    [ScriptMethod(name: "Main Boss: Mechanic 1 Second Merge Guidance", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00020001"])]
    public void MB_HC1_GuidePhase3(Event @event, ScriptAccessory accessory)
    {
        if (mb_phase != MB_Phase.HC1) return;
        Task.Delay(50).ContinueWith(t =>
        {
            if (mb_sideCleaveNum == 2 && mb_conceptFinNum == 6)
            {
                var myHCIndex = mb_hc1_sid.IndexOf(accessory.Data.Me);

                bool shouldJoinMergeUp = (myHCIndex == 3 && mb_joinMerge[0]) ||
                                         (accessory.Data.Me == mb_betaLongFollower && mb_joinMerge[1]) ||
                                         (accessory.Data.Me == mb_gammaLongFollower && mb_joinMerge[2]);

                bool shouldJoinMergeDown = (accessory.Data.Me == mb_alphaLongFollower && mb_joinMerge[0]) ||
                                           (myHCIndex == 5 && mb_joinMerge[1]) ||
                                           (myHCIndex == 7 && mb_joinMerge[2]);

                var dp = accessory.Data.GetDefaultDrawProperties();
                if (shouldJoinMergeUp)
                {
                    dp.Name = $"Participate in Top Half Merge Guidance";
                    dp.Scale = new(0.5f);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = new(100, 0, 90);
                    dp.ScaleMode = ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 0;
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp.Name = $"Participate in Top Half Merge Area";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.Me;
                    dp.Position = new(100, 0, 90);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 0;
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    accessory.Method.TextInfo($"Participate in top half merge", 6000);

                }
                else if (shouldJoinMergeDown)
                {
                    dp.Name = $"Participate in Bottom Half Merge Guidance";
                    dp.Scale = new(0.5f);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = new(100, 0, 110);
                    dp.ScaleMode = ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 0;
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp.Name = $"Participate in Bottom Half Merge Area";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.Me;
                    dp.Position = new(100, 0, 110);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 0;
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    accessory.Method.TextInfo($"Participate in bottom half merge", 6000);
                }
                else
                {
                    dp.Name = $"Avoid Top Half Merge Area";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.Me;
                    dp.Position = new(100, 0, 90);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 0;
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                    dp.Name = $"Avoid Bottom Half Merge Area";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.Me;
                    dp.Position = new(100, 0, 110);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 0;
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                    accessory.Method.TextInfo($"Avoid merge", 6000, true);
                }
            }
        });
    }

    #endregion

    #region Main Boss: Limitless Desolation

    [ScriptMethod(name: "Main Boss: Limitless Desolation Guidance and Spread Warning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:30189"])]
    public void MB_LimitlessDesolation(Event @event, ScriptAccessory accessory)
    {
        mb_phase = MB_Phase.LD;
        var dp = accessory.Data.GetDefaultDrawProperties();
        for (int i = 0; i < accessory.Data.PartyList.Count(); i++)
        {
            dp.Name = $"Limitless Desolation Spread-{i}";
            dp.Scale = new(6);
            dp.Owner = accessory.Data.PartyList[i];
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.6f);
            dp.Delay = 0;
            dp.DestoryAt = 20000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        var myIndex = IndexHelper.getMyIndex(accessory);
        drawLDSpreadDir(5000, myIndex, accessory);
    }

    private static void drawLDSpreadDir(int castTime, int myIndex, ScriptAccessory accessory)
    {
        Vector3[] safePos = new Vector3[8];
        safePos[0] = new Vector3(90, 0, 80);
        safePos[1] = new Vector3(80, 0, 90);
        safePos[2] = new Vector3(80, 0, 100);
        safePos[3] = new Vector3(90, 0, 110);
        safePos[4] = DirectionCalc.FoldPointLR(safePos[0], 100);
        safePos[5] = DirectionCalc.FoldPointLR(safePos[1], 100);
        safePos[6] = DirectionCalc.FoldPointLR(safePos[2], 100);
        safePos[7] = DirectionCalc.FoldPointLR(safePos[3], 100);

        for (int i = 0; i < 8; i++)
        {
            var dp0 = AssignDp.drawStatic(safePos[i], 0, 0, castTime, $"Spread Position{i}", accessory);
            dp0.Scale = new(1f);
            dp0.Color = myIndex == i ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp0);
        }

        var dp = AssignDp.dirPos(safePos[myIndex], 0, castTime, $"Spread Position Guidance{myIndex}", accessory);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "Main Boss: Limitless Desolation Yellow Circle Warning", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:30192"])]
    public void MB_TyrantsFire(Event @event, ScriptAccessory accessory)
    {
        // After detecting vulnerability, draw yellow circle warning
        var tid = @event.TargetId();
        var tidx = accessory.Data.PartyList.IndexOf(tid);
        accessory.Method.RemoveDraw($"Limitless Desolation Spread-{tidx}");

        if (tid != accessory.Data.Me) return;

        // 31368
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Limitless Desolation Yellow Circle Warning";
        dp.Scale = new(8);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = tid;
        dp.Color = ColorHelper.DelayDangerColor.V4;
        dp.Delay = 0;
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    /// <summary>
    /// Limitless Desolation tower data, containing "which tower", "relative coordinates", "which player soaks it"
    /// </summary>
    public class LD_Tower
    {
        public int TowerIdx { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public int PlayerIdx { get; set; }
        public LD_Tower(int tidx, int row, int col, int playerIdx)
        {
            TowerIdx = tidx;
            Row = row;
            Col = col;
            PlayerIdx = playerIdx;
        }
    }

    [ScriptMethod(name: "Main Boss: Limitless Desolation Tower Record", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375AB", "Id:00020001", "Index:regex:^(000000(0[9ABC]|4[CDEF]|5[0145]))$"], userControl: false)]
    public void MB_LDTowerRecord(Event @event, ScriptAccessory accessory)
    {
        lock (mb_LD_towerOrder)
        {
            var idx = @event.Index();
            int Row;
            int Col;
            (Row, Col) = idx switch
            {
                0x9 => (2, 2),
                0xA => (2, 3),
                0xB => (3, 2),
                0xC => (3, 3),
                0x4C => (1, 1),
                0x4D => (1, 2),
                0x4E => (1, 3),
                0x4F => (1, 4),
                0x50 => (2, 1),
                0x51 => (2, 4),
                0x54 => (3, 1),
                0x55 => (3, 4),
                _ => (0, 0)
            };
            mb_LD_towerOrder.Add(new LD_Tower(mb_LD_towerOrder.Count(), Row, Col, -1));
            DebugMsg($"Detected generation of tower number {mb_LD_towerOrder.Count()} (Row {Row}, Col {Col}).", accessory);
        }
    }

    [ScriptMethod(name: "Main Boss: Limitless Desolation Tower Drawing", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:30192"])]
    public async void MB_LDPlayerRecord(Event @event, ScriptAccessory accessory)
    {
        var tid = @event.TargetId();
        if (@event.TargetIndex() != 1) return;
        var pidx = IndexHelper.getPlayerIdIndex(tid, accessory);
        mb_LD_playerOrder.Add(pidx);
        DebugMsg($"Detected vulnerability: {IndexHelper.getPlayerJobByIndex(pidx)}", accessory);
        // {mb_LD_playerOrder.Count()}th

        await Task.Delay(1000);

        lock (mb_LD_towerOrder)
        {
            var myIndex = IndexHelper.getMyIndex(accessory);
            for (int i = 0; i < mb_LD_towerOrder.Count(); i++)
            {
                if (mb_LD_towerOrder[i].PlayerIdx != -1) continue;

                var isTN = pidx <= 3;
                var isLeftTower = mb_LD_towerOrder[i].Col <= 2;

                if ((isTN && isLeftTower) || (!isTN && !isLeftTower))
                {
                    mb_LD_towerOrder[i].PlayerIdx = pidx;
                    if (myIndex == pidx)
                        drawLDDir(mb_LD_towerOrder[i], accessory);
                }
                else
                    continue;
            }
        }

    }

    private static void drawLDDir(LD_Tower tower, ScriptAccessory accessory)
    {
        // Before the yellow circle appears underfoot, mark the tower range as a danger zone
        var dp_tdanger = assignDp_Tower(tower.Row, tower.Col, accessory);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp_tdanger);

        // After the danger zone disappears, mark the tower range as safe
        var dp_tsafe = assignDp_Tower(tower.Row, tower.Col, accessory);
        dp_tsafe.Color = accessory.Data.DefaultSafeColor;
        dp_tsafe.Delay = 7000;
        // Set long disappearance time intentionally, remove with envcontrol
        dp_tsafe.DestoryAt = 30000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp_tsafe);

        // After the danger zone disappears, guide the player to the tower
        var dp_tdir = assignDp_TowerDir(tower.Row, tower.Col, accessory);
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp_tdir);
        return;
    }

    private static DrawPropertiesEdit assignDp_Tower(int row, int col, ScriptAccessory accessory)
    {
        Vector3 tower_center = new Vector3(75 + col * 10, 0, 75 + row * 10);
        var delay = 0;
        var destoryAt = 7000;   // 8000 - task delay 100
        var dp = AssignDp.drawStatic(tower_center, 0, delay, destoryAt, $"Tower{row}{col}", accessory);
        dp.Scale = new(4);
        dp.Color = ColorHelper.colorRed.V4;
        return dp;
    }

    private static DrawPropertiesEdit assignDp_TowerDir(int row, int col, ScriptAccessory accessory)
    {
        Vector3 tower_center = new Vector3(75 + col * 10, 0, 75 + row * 10);
        var delay = 7000;
        var destoryAt = 30000;   // 8000 - task delay 100
        var dp = AssignDp.dirPos(tower_center, delay, destoryAt, $"Tower Guidance{row}{col}", accessory);
        return dp;
    }

    [ScriptMethod(name: "Main Boss: Limitless Desolation Tower Disappear", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375AB", "Id:00080004", "Index:regex:^(000000(0[9ABC]|4[CDEF]|5[0145]))$"], userControl: false)]
    public void MB_LDTowerRemove(Event @event, ScriptAccessory accessory)
    {
        var idx = @event.Index();
        int Row;
        int Col;
        (Row, Col) = idx switch
        {
            0x9 => (2, 2),
            0xA => (2, 3),
            0xB => (3, 2),
            0xC => (3, 3),
            0x4C => (1, 1),
            0x4D => (1, 2),
            0x4E => (1, 3),
            0x4F => (1, 4),
            0x50 => (2, 1),
            0x51 => (2, 4),
            0x54 => (3, 1),
            0x55 => (3, 4),
            _ => (0, 0)
        };
        accessory.Method.RemoveDraw($"Tower Guidance{Row}{Col}");
        accessory.Method.RemoveDraw($"Tower{Row}{Col}");
    }

    // EnvControl Logging
    // 800375AB 00020001
    // Index
    // 00000005 (2,2) Rock      // 00000009 (2,2) Tower
    // 00000006 (2,3) Rock      // 0000000A (2,3) Tower
    // 00000007 (3,2) Rock      // 0000000B (3,2) Tower
    // 00000008 (3,3) Rock      // 0000000C (3,3) Tower

    // 00000046 (1,1) Rock      // 0000004C (1,1) Tower
    // 00000047 (1,2) Rock      // 0000004D (1,2) Tower
    // 00000048 (1,3) Rock      // 0000004E (1,3) Tower
    // 00000049 (1,4) Rock      // 0000004F (1,4) Tower
    // 0000004A (2,1) Rock      // 00000050 (2,1) Tower
    // 0000004B (2,4) Rock      // 00000051 (2,4) Tower

    // 00000052 (3,1) Rock      // 00000054 (3,1) Tower
    // 00000053 (3,4) Rock      // 00000055 (3,4) Tower

    // State
    // 00020001 Spawn
    // 00200010 Touched
    // 00400001 Exit
    // 00080004 Disappear

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

    public static uint ActionId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
    }

    public static uint SourceId(this Event @event)
    {
        return ParseHexId(@event["SourceId"], out var id) ? id : 0;
    }

    public static uint SourceDataId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["SourceDataId"]);
    }

    public static uint TargetId(this Event @event)
    {
        return ParseHexId(@event["TargetId"], out var id) ? id : 0;
    }

    public static uint TargetIndex(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["TargetIndex"]);
    }

    public static Vector3 SourcePosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
    }

    public static Vector3 TargetPosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
    }

    public static Vector3 EffectPosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
    }

    public static float SourceRotation(this Event @event)
    {
        return JsonConvert.DeserializeObject<float>(@event["SourceRotation"]);
    }

    public static float TargetRotation(this Event @event)
    {
        return JsonConvert.DeserializeObject<float>(@event["TargetRotation"]);
    }

    public static string SourceName(this Event @event)
    {
        return @event["SourceName"];
    }

    public static string TargetName(this Event @event)
    {
        return @event["TargetName"];
    }

    public static uint DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["DurationMilliseconds"]);
    }

    public static uint Index(this Event @event)
    {
        return ParseHexId(@event["Index"], out var id) ? id : 0;
    }

    public static uint State(this Event @event)
    {
        return ParseHexId(@event["State"], out var id) ? id : 0;
    }

    public static uint DirectorId(this Event @event)
    {
        return ParseHexId(@event["DirectorId"], out var id) ? id : 0;
    }

    public static uint StatusID(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StatusID"]);
    }

    public static uint StackCount(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["StackCount"]);
    }

    public static uint Param(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["Param"]);
    }
}

public static class DirectionCalc
{
    // North as 0 for list
    // InnGame      List    Dir
    // 0            - 4     pi
    // 0.25 pi      - 3     0.75pi
    // 0.5 pi       - 2     0.5pi
    // 0.75 pi      - 1     0.25pi
    // pi           - 0     0
    // 1.25 pi      - 7     1.75pi
    // 1.5 pi       - 6     1.5pi
    // 1.75 pi      - 5     1.25pi
    // Dir = Pi - InnGame (+ 2pi)

    /// <summary>
    /// Convert in-game base angle (South as 0, increasing counter-clockwise) to logic base angle (North as 0, increasing clockwise)
    /// </summary>
    /// <param name="radian">In-game base angle</param>
    /// <returns>Logic base angle</returns>
    public static float BaseInnGame2DirRad(float radian)
    {
        float r = (float)Math.PI - radian;
        if (r < 0) r = (float)(r + 2 * Math.PI);
        if (r > 2 * Math.PI) r = (float)(r - 2 * Math.PI);
        return r;
    }

    /// <summary>
    /// Convert logic base angle (North as 0, increasing clockwise) to in-game base angle (South as 0, increasing counter-clockwise)
    /// </summary>
    /// <param name="radian">Logic base angle</param>
    /// <returns>In-game base angle</returns>
    public static float BaseDirRad2InnGame(float radian)
    {
        float r = (float)Math.PI - radian;
        if (r < Math.PI) r = (float)(r + 2 * Math.PI);
        if (r > Math.PI) r = (float)(r - 2 * Math.PI);
        return r;
    }

    /// <summary>
    /// Input logic base angle, get logic direction
    /// </summary>
    /// <param name="radian">Logic base angle</param>
    /// <param name="dirs">Total number of directions</param>
    /// <returns>Logic direction corresponding to the logic base angle</returns>
    public static int DirRadRoundToDirs(float radian, int dirs)
    {
        var r = Math.Round(radian / (2f / dirs * Math.PI));
        if (r == dirs) r = r - dirs;
        return (int)r;
    }

    /// <summary>
    /// Input coordinates, get normal division logic direction (with top-right as 0)
    /// </summary>
    /// <param name="point">Coordinate point</param>
    /// <param name="center">Center point</param>
    /// <param name="dirs">Total number of directions</param>
    /// <returns>Logic direction corresponding to the coordinate point</returns>
    public static int PositionFloorToDirs(Vector3 point, Vector3 center, int dirs)
    {
        // Normal division, 0° is the dividing line, dividing 360° into dirs parts
        var r = Math.Floor(dirs / 2 - dirs / 2 * Math.Atan2(point.X - center.X, point.Z - center.Z) / Math.PI) % dirs;
        return (int)r;
    }

    /// <summary>
    /// Input coordinates, get diagonal division logic direction (with straight up as 0)
    /// </summary>
    /// <param name="point">Coordinate point</param>
    /// <param name="center">Center point</param>
    /// <param name="dirs">Total number of directions</param>
    /// <returns>Logic direction corresponding to the coordinate point</returns>
    public static int PositionRoundToDirs(Vector3 point, Vector3 center, int dirs)
    {
        // Diagonal division, 0° returns 0, dividing 360° into dirs parts
        var r = Math.Round(dirs / 2 - dirs / 2 * Math.Atan2(point.X - center.X, point.Z - center.Z) / Math.PI) % dirs;
        return (int)r;
    }

    /// <summary>
    /// Convert angle to radian
    /// </summary>
    /// <param name="angle">Angle in degrees</param>
    /// <returns>Corresponding radian value</returns>
    public static float angle2Rad(float angle)
    {
        // Convert input angle to radian
        float radian = (float)(angle * Math.PI / 180);
        return radian;
    }

    /// <summary>
    /// Rotate a point around a center by a logic base radian
    /// </summary>
    /// <param name="point">Point to rotate</param>
    /// <param name="center">Center</param>
    /// <param name="radian">Rotation radian</param>
    /// <returns>Rotated coordinate point</returns>
    public static Vector3 RotatePoint(Vector3 point, Vector3 center, float radian)
    {
        // Rotate a point clockwise by a certain radian around a center
        Vector2 v2 = new(point.X - center.X, point.Z - center.Z);
        var rot = MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian;
        var length = v2.Length();
        return new(center.X + MathF.Sin(rot) * length, center.Y, center.Z - MathF.Cos(rot) * length);
    }

    /// <summary>
    /// Extend a point from a center point by a logic base angle
    /// </summary>
    /// <param name="center">Center point to extend from</param>
    /// <param name="radian">Rotation radian</param>
    /// <param name="length">Extension length</param>
    /// <returns>Extended coordinate point</returns>
    public static Vector3 ExtendPoint(Vector3 center, float radian, float length)
    {
        // Extend a point a certain length at a certain radian
        return new(center.X + MathF.Sin(radian) * length, center.Y, center.Z - MathF.Cos(radian) * length);
    }

    /// <summary>
    /// Find the logic base radian from an outer point to the center
    /// </summary>
    /// <param name="center">Center</param>
    /// <param name="new_point">Outer point</param>
    /// <returns>Logic base radian from the outer point to the center</returns>
    public static float FindRadian(Vector3 center, Vector3 new_point)
    {
        // Find the radian from the point to the center
        float radian = MathF.PI - MathF.Atan2(new_point.X - center.X, new_point.Z - center.Z);
        if (radian < 0)
            radian += 2 * MathF.PI;
        return radian;
    }

    /// <summary>
    /// Fold the input point horizontally
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerx">Center axis X coordinate</param>
    /// <returns></returns>
    public static Vector3 FoldPointLR(Vector3 point, int centerx)
    {
        Vector3 v3 = new(2 * centerx - point.X, point.Y, point.Z);
        return v3;
    }
}

public static class IndexHelper
{
    /// <summary>
    /// Input player dataId, get the corresponding position index
    /// </summary>
    /// <param name="pid">Player SourceId</param>
    /// <param name="accessory"></param>
    /// <returns>The position index corresponding to the player</returns>
    public static int getPlayerIdIndex(uint pid, ScriptAccessory accessory)
    {
        // Get player IDX
        return accessory.Data.PartyList.IndexOf(pid);
    }

    /// <summary>
    /// Get the position index of the main perspective player
    /// </summary>
    /// <param name="accessory"></param>
    /// <returns>Position index of the main perspective player</returns>
    public static int getMyIndex(ScriptAccessory accessory)
    {
        return accessory.Data.PartyList.IndexOf(accessory.Data.Me);
    }

    /// <summary>
    /// Input player dataId, get the corresponding position name, output string only for text output
    /// </summary>
    /// <param name="pid">Player SourceId</param>
    /// <param name="accessory"></param>
    /// <returns>The position name corresponding to the player</returns>
    public static string getPlayerJobByID(uint pid, ScriptAccessory accessory)
    {
        // Get player role abbreviation, only for DEBUG output
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

    /// <summary>
    /// Input position index, get the corresponding position name, output string only for text output
    /// </summary>
    /// <param name="idx">Position index</param>
    /// <returns></returns>
    public static string getPlayerJobByIndex(int idx)
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
}

public static class ColorHelper
{
    public static ScriptColor colorRed = new ScriptColor { V4 = new Vector4(1.0f, 0f, 0f, 1.0f) };
    public static ScriptColor colorPink = new ScriptColor { V4 = new Vector4(1f, 0f, 1f, 1.0f) };
    public static ScriptColor colorCyan = new ScriptColor { V4 = new Vector4(0f, 1f, 0.8f, 1.0f) };

    // Door Boss Dragon/Phoenix delayed danger zone color, purple
    public static ScriptColor DelayDangerColor = new ScriptColor { V4 = new Vector4(1f, 0.2f, 1f, 1.5f) };
    // Door Boss Snake position color
    public static ScriptColor GorgonColor = new ScriptColor { V4 = new Vector4(1f, 1f, 1f, 2f) };
}

public static class AssignDp
{
    /// <summary>
    /// Return dp from self to a target location, can modify dp.TargetPosition, dp.Scale
    /// </summary>
    /// <param name="target_pos">Target location</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit dirPos(Vector3 target_pos, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(0.5f);
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = target_pos;
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    /// <summary>
    /// Return dp from a start location to a target location, can modify dp.Position, dp.TargetPosition, dp.Scale
    /// </summary>
    /// <param name="start_pos">Start location</param>
    /// <param name="target_pos">Target location</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit dirPos2Pos(Vector3 start_pos, Vector3 target_pos, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(0.5f);
        dp.Position = start_pos;
        dp.TargetPosition = target_pos;
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    /// <summary>
    /// Return dp from self to a target object, can modify dp.TargetObject, dp.Scale
    /// </summary>
    /// <param name="target_id">Target object</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit dirTarget(uint target_id, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(0.5f);
        dp.Owner = accessory.Data.Me;
        dp.TargetObject = target_id;
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Color = accessory.Data.DefaultSafeColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    /// <summary>
    /// Return dp related to an object's enmity, can modify dp.TargetResolvePattern, dp.TargetOrderIndex, dp.Owner
    /// </summary>
    /// <param name="owner_id">Start target id, usually the boss</param>
    /// <param name="order_idx">Order, starting from 1</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit drawTargetOrder(uint owner_id, uint order_idx, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(5, 40);
        dp.Owner = owner_id;
        dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
        dp.TargetOrderIndex = order_idx;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    /// <summary>
    /// Return dp related to an object's distance, can modify dp.CentreResolvePattern, dp.CentreOrderIndex, dp.Owner
    /// </summary>
    /// <param name="owner_id">Start target id, usually the boss</param>
    /// <param name="order_idx">Order, starting from 1</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit drawCenterOrder(uint owner_id, uint order_idx, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(5);
        dp.Owner = owner_id;
        dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
        dp.CentreOrderIndex = order_idx;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }
    /// <summary>
    /// Return owner to target dp, can modify dp.Owner, dp.TargetObject, dp.Scale
    /// </summary>
    /// <param name="owner_id">Start target id, usually self</param>
    /// <param name="target_id">Target unit id</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit drawOwner2Target(uint owner_id, uint target_id, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(5, 40);
        dp.Owner = owner_id;
        dp.TargetObject = target_id;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    /// <summary>
    /// Return circle drawing, follows owner, can modify dp.Owner, dp.Scale
    /// </summary>
    /// <param name="owner_id">Start target id, usually self or boss</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit drawCircle(uint owner_id, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(5);
        dp.Owner = owner_id;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }

    /// <summary>
    /// Return static dp, usually for guiding fixed positions. Can modify dp.Position, dp.Rotation, dp.Scale
    /// </summary>
    /// <param name="center">Start position, usually arena center</param>
    /// <param name="radian">Rotation angle, North as 0 degrees clockwise</param>
    /// <param name="delay">Delay in ms before drawing appears</param>
    /// <param name="destoryAt">Drawing disappears after `destoryAt` ms</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit drawStatic(Vector3 center, float radian, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new(5, 20);
        dp.Position = center;
        dp.Rotation = DirectionCalc.BaseDirRad2InnGame(radian);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        return dp;
    }
}


#endregion
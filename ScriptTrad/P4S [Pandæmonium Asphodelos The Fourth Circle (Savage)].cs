using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Data;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Module.GameEvent.Types;
using KodakkuAssist.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;
using Dalamud.Utility.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Vfx;
using Lumina.Excel.Sheets;

namespace UsamisScript.EndWalker.p4s;

[ScriptType(name: "P4S [Pandæmonium: Asphodelos The Fourth Circle (Savage)]", territorys: [1009], guid: "de9e31e6-d040-48e3-bf0b-aa4e2643f79d", version: "0.0.0.5", author: "Usami", note: noteStr, updateInfo: UpdateInfo)]

public class p4s
{
    const string noteStr =
    """
    Please check and configure the "User Settings" as needed.
    "Cheese" refers to the Black Sugar Lychee strategy.
    Gates only provide rot/poison prompts, body up to Phase 2.
    """;
    
    private const string UpdateInfo =
        """
        1. Adapted to Kodakku 0.5.x.x
        """;

    [UserSetting("Debug mode, turn off unless developing")]
    public static bool DebugMode { get; set; } = false;
    public enum Act2StrategyEnum
    {
        Regular,
        Cheese_Spread
    }

    [UserSetting("Phase 2 Strategy")]
    public Act2StrategyEnum Act2Strategy { get; set; } = Act2StrategyEnum.Cheese_Spread;

    [UserSetting("Position Circle Drawing - Normal Color")]
    public static ScriptColor posColorNormal { get; set; } = new ScriptColor { V4 = new Vector4(1.0f, 1.0f, 1.0f, 1.0f) };

    [UserSetting("Position Circle Drawing - Player Position Color")]
    public static ScriptColor posColorPlayer { get; set; } = new ScriptColor { V4 = new Vector4(0.0f, 1.0f, 1.0f, 1.0f) };

    public enum P4S_Phase
    {
        Init,           // Initial
        Act1,           // Act 1
        Act2,           // Act 2
        Act3            // Act 3
    }
    public static Vector3 CENTER = new Vector3(100, 0, 100);
    P4S_Phase phase = P4S_Phase.Init;
    List<bool> Drawn = new bool[20].ToList();   // Drawing record
    int BloodrakeCastTime = 0;  // Bloodrake cast count
    List<int> BloodrakeNum = [0, 0, 0, 0, 0, 0, 0, 0];  // Bloodrake count record
    int Act1CirclePosition = -1;    // Act 1 large heavy position
    int Act2CirclePosition = -1;    // Act 2 large heavy position
    Act2Solution Act2Sol = new Act2Solution();  // Act 2 solution
    public void Init(ScriptAccessory accessory)
    {
        phase = P4S_Phase.Init;
        BloodrakeCastTime = 0;
        Drawn = new bool[20].ToList();
        accessory.Method.RemoveDraw(".*");
    }

    public static void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!DebugMode) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    [ScriptMethod(name: "Anytime DEBUG", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:=TST"], userControl: false)]
    public void EchoDebug(Event @event, ScriptAccessory accessory)
    {
        if (!DebugMode) return;

        var str = Act2Sol.Print();
        DebugMsg($"{str}", accessory);
    }

    [ScriptMethod(name: "Gates: Bloodrake Initialization (uncontrolled)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27096"], userControl: false)]
    public void BloodrakeInit(Event @event, ScriptAccessory accessory)
    {
        if (BloodrakeCastTime == 0)
            BloodrakeNum = [0, 0, 0, 0, 0, 0, 0, 0];
        BloodrakeCastTime++;
    }

    [ScriptMethod(name: "Gates: Bloodrake Record (uncontrolled)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:27096"], userControl: false)]
    public void BloodrakeRecord(Event @event, ScriptAccessory accessory)
    {
        var tid = @event.TargetId();
        var tidx = accessory.getPlayerIdIndex(tid);

        if (BloodrakeCastTime == 1)
            BloodrakeNum[tidx] = BloodrakeNum[tidx] + 1;
        else if (BloodrakeCastTime == 2)
            BloodrakeNum[tidx] = BloodrakeNum[tidx] + 10;
        else
            return;
    }

    [ScriptMethod(name: "Gates: Rot/Line Poison Action Prompt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27110"])]
    public void DirectorAction(Event @event, ScriptAccessory accessory)
    {
        var myIndex = accessory.getMyIndex();
        switch (BloodrakeNum[myIndex])
        {
            case 0:
                accessory.Method.TextInfo($"Take rot, take line", 15000, false);
                break;
            case 1:
                accessory.Method.TextInfo($"Take rot, don't take line", 15000, false);
                break;
            case 10:
                accessory.Method.TextInfo($"Don't take rot, take line", 15000, false);
                break;
            case 11:
                accessory.Method.TextInfo($"Don't take rot, don't take line", 15000, false);
                break;
            default:
                return;
        }
    }

    [ScriptMethod(name: "Body: Phase 1, Phase Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27148"], userControl: false)]
    public void Act1_PhaseRecord(Event @event, ScriptAccessory accessory)
    {
        phase = P4S_Phase.Act1;
        Act1CirclePosition = -1;
        Drawn[0] = false;
        DebugMsg($"Current phase: {phase}", accessory);
    }

    [ScriptMethod(name: "Body: Phase 1 Field Prompt", eventType: EventTypeEnum.Tether, eventCondition: ["Id:00AD"])]
    public void Act1_Field(Event @event, ScriptAccessory accessory)
    {
        if (phase != P4S_Phase.Act1) return;
        if (Act1CirclePosition != -1) return;
        if (Drawn[0]) return;
        Drawn[0] = true;

        var spos = @event.SourcePosition();
        Act1CirclePosition = spos.PositionRoundToDirs(CENTER, 4);

        DebugMsg($"Drawing Phase 1 {Act1CirclePosition}", accessory);

        var _pos1 = spos;
        var _pos2 = spos.RotatePoint(CENTER, float.Pi);
        drawBigCircle(_pos1, 0, 11000, $"Heavy1", accessory);
        drawBigCircle(_pos2, 0, 11000, $"Heavy2", accessory);

        var _pos3 = spos.RotatePoint(CENTER, float.Pi / 2);
        var _pos4 = spos.RotatePoint(CENTER, -float.Pi / 2);
        drawBigCircle(_pos3, 14000, 3000, $"Heavy3", accessory);
        drawBigCircle(_pos4, 14000, 3000, $"Heavy4", accessory);
    }

    private static void drawBigCircle(Vector3 spos, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.drawStatic(spos, 0, delay, destoryAt, name);
        dp.Scale = new(20);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    private static void drawTowerRegion(Vector3 spos, int delay, int destoryAt, string name, ScriptAccessory accessory)
    {
        var dp = accessory.drawStatic(spos, 0, delay, destoryAt, name);
        dp.Scale = new(4);
        dp.Color = ColorHelper.colorCyan.V4.WithW(1.5f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Body: Near/Far Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(2717[45])$"])]
    public void MB_TankBusterFarNear(Event @event, ScriptAccessory accessory)
    {
        var aid = @event.ActionId();
        var sid = @event.SourceId();
        const uint NEAR = 27174;
        var _isNear = aid == NEAR;

        IPlayerCharacter? me = (IPlayerCharacter?)accessory.GetMe();
        if (me == null) return;
        var isTank = me.IsTank();

        var dp1 = accessory.drawCenterOrder(sid, 1, 0, 5000, $"Tankbuster1");
        dp1.CentreResolvePattern = _isNear ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
        dp1.Scale = new(5f);
        dp1.Color = isTank ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);

        var dp2 = accessory.drawCenterOrder(sid, 2, 0, 5000, $"Tankbuster2");
        dp2.CentreResolvePattern = _isNear ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
        dp2.Scale = new(5f);
        dp2.Color = isTank ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);
    }

    [ScriptMethod(name: "Body: Stack Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28280"])]
    public void MB_TankBuster(Event @event, ScriptAccessory accessory)
    {
        var tid = @event.TargetId();
        var dp = accessory.drawCircle(tid, 0, 5000, $"Tankbuster{tid}");
        dp.Scale = new(6f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Body: Phase 2, Phase Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28340"], userControl: false)]
    public void Act2_PhaseRecord(Event @event, ScriptAccessory accessory)
    {
        phase = P4S_Phase.Act2;
        Act2CirclePosition = -1;
        for (int i = 1; i <= 5; i++)
            Drawn[i] = false;
        Act2Sol.Init();
        DebugMsg($"Current phase: {phase}", accessory);
    }

    [ScriptMethod(name: "Body: Phase 2 Field Prompt", eventType: EventTypeEnum.Tether, eventCondition: ["Id:00AD"])]
    public void Act2_Field(Event @event, ScriptAccessory accessory)
    {
        if (phase != P4S_Phase.Act2) return;
        if (Act2CirclePosition != -1) return;
        if (Drawn[1]) return;
        Drawn[1] = true;

        var spos = @event.SourcePosition();
        var _tempPos = spos.PositionRoundToDirs(CENTER, 4);
        var _atNorthLeftTower = _tempPos == 0 && spos.X < CENTER.X;
        var _atEastUpTower = _tempPos == 1 && spos.Z < CENTER.Z;
        var _atSouthRightTower = _tempPos == 2 && spos.X > CENTER.X;
        var _atWestDownTower = _tempPos == 3 && spos.Z > CENTER.Z;

        if (_atNorthLeftTower || _atEastUpTower || _atSouthRightTower || _atWestDownTower)
            return;

        Act2CirclePosition = _tempPos;
        DebugMsg($"Drawing Phase 2 {Act2CirclePosition}", accessory);

        var _pos1 = spos;
        var _pos2 = spos.RotatePoint(CENTER, float.Pi);
        drawBigCircle(_pos1, 0, 19000, $"Heavy1", accessory);
        drawBigCircle(_pos2, 0, 19000, $"Heavy2", accessory);
        var _pos3 = spos.RotatePoint(CENTER, float.Pi / 2);
        var _pos4 = spos.RotatePoint(CENTER, -float.Pi / 2);
        drawBigCircle(_pos3, 19000, 7000, $"Heavy3", accessory);
        drawBigCircle(_pos4, 19000, 7000, $"Heavy4", accessory);

        Vector3 _towerPos1;
        Vector3 _towerPos2;
        Vector3 _towerPos3;
        Vector3 _towerPos4;
        if (Act2CirclePosition % 2 == 0)
        {
            Act2Sol.isLRSafeFirst = true;
            _towerPos1 = _pos1 + (_pos1.Z < CENTER.Z ? new Vector3(-8, 0, 0) : new Vector3(8, 0, 0));
            _towerPos2 = _pos2 + (_pos2.Z < CENTER.Z ? new Vector3(-8, 0, 0) : new Vector3(8, 0, 0));
            _towerPos3 = _pos3 + (_pos3.X < CENTER.X ? new Vector3(0, 0, 8) : new Vector3(0, 0, -8));
            _towerPos4 = _pos4 + (_pos4.X < CENTER.X ? new Vector3(0, 0, 8) : new Vector3(0, 0, -8));
        }
        else
        {
            Act2Sol.isLRSafeFirst = false;
            _towerPos1 = _pos1 + (_pos1.X < CENTER.X ? new Vector3(0, 0, 8) : new Vector3(0, 0, -8));
            _towerPos2 = _pos2 + (_pos2.X < CENTER.X ? new Vector3(0, 0, 8) : new Vector3(0, 0, -8));
            _towerPos3 = _pos3 + (_pos3.Z < CENTER.Z ? new Vector3(-8, 0, 0) : new Vector3(8, 0, 0));
            _towerPos4 = _pos4 + (_pos4.Z < CENTER.Z ? new Vector3(-8, 0, 0) : new Vector3(8, 0, 0));
        }
        drawTowerRegion(_towerPos3, 0, 19000, $"Tower3", accessory);
        drawTowerRegion(_towerPos4, 0, 19000, $"Tower4", accessory);
        drawTowerRegion(_towerPos1, 19000, 7000, $"Tower1", accessory);
        drawTowerRegion(_towerPos2, 19000, 7000, $"Tower2", accessory);
    }

    [ScriptMethod(name: "Body: Phase 2 Yellow Circle Bait Prompt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27177"])]
    public void Act2_DarkDesign(Event @event, ScriptAccessory accessory)
    {
        for (int i = 0; i < accessory.Data.PartyList.Count(); i++)
        {
            var dp = accessory.drawCircle(accessory.Data.PartyList[i], 0, 5000, $"Bait Yellow Circle{i}");
            dp.ScaleMode = ScaleMode.ByTime;
            dp.Color = ColorHelper.colorPink.V4;
            dp.Scale = new(6f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }

    public class Act2Solution
    {
        public Vector3[] TowerPos = new Vector3[4];
        public Vector3[] CirclePos = new Vector3[4];
        public List<int> FireTargets { get; set; }
        public List<int> WindTargets { get; set; }
        public List<int> DarkTargets { get; set; }
        public bool isLRSafeFirst { get; set; }
        public int CircleCastTimes { get; set; }
        public Act2Solution()
        {
            FireTargets = new List<int> { };
            WindTargets = new List<int> { };
            DarkTargets = new List<int> { };
            Init();
        }
        public void Init()
        {
            isLRSafeFirst = false;
            FireTargets = new List<int> { };
            WindTargets = new List<int> { };
            DarkTargets = new List<int> { };
            CircleCastTimes = 0;

            TowerPos[0] = new(96, 0, 82);
            TowerPos[1] = TowerPos[0].RotatePoint(CENTER, float.Pi / 2);
            TowerPos[2] = TowerPos[0].RotatePoint(CENTER, float.Pi);
            TowerPos[3] = TowerPos[0].RotatePoint(CENTER, float.Pi / -2);

            CirclePos[0] = new(104, 0, 82);
            CirclePos[1] = CirclePos[0].RotatePoint(CENTER, float.Pi / 2);
            CirclePos[2] = CirclePos[0].RotatePoint(CENTER, float.Pi);
            CirclePos[3] = CirclePos[0].RotatePoint(CENTER, float.Pi / -2);
        }
        public string Print()
        {
            string safePosStr = $"{(isLRSafeFirst ? "Left-Right safe first" : "Top-Bottom safe first")}";

            string FireTargetsStr = string.Join(", ", FireTargets);
            string str1 = $"FireTargets: {FireTargetsStr}";

            string WindTargetsStr = string.Join(", ", WindTargets);
            string str2 = $"WindTargets: {WindTargetsStr}";

            string DarkTargetsStr = string.Join(", ", DarkTargets);
            string str3 = $"DarkTargets: {DarkTargetsStr}";

            string str = safePosStr + '\n' + str1 + '\n' + str2 + '\n' + str3;
            return str;
        }
    }

    [ScriptMethod(name: "Body: Phase 2, Head Marker Record", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(012[DEF])$"], userControl: false)]
    public void Act2_IconRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != P4S_Phase.Act2) return;
        var id = @event.Id();
        var tid = @event.TargetId();
        var tidx = accessory.getPlayerIdIndex(tid);

        const uint DARK = 0x012D;
        const uint FIRE = 0x012F;
        const uint WIND = 0x012E;

        if (id == DARK)
            Act2Sol.DarkTargets.Add(tidx);
        if (id == FIRE)
            Act2Sol.FireTargets.Add(tidx);
        if (id == WIND)
            Act2Sol.WindTargets.Add(tidx);
    }

    [ScriptMethod(name: "Body: Phase 2, Tether Record", eventType: EventTypeEnum.Tether, eventCondition: ["Id:00AC"], userControl: false)]
    public async void Act2_TetherRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != P4S_Phase.Act2) return;
        var sid = @event.SourceId();
        var tid = @event.TargetId();
        var sidx = accessory.getPlayerIdIndex(sid);
        var tidx = accessory.getPlayerIdIndex(tid);

        await Task.Delay(100);

        if (Act2Sol.DarkTargets.Contains(sidx) && !Act2Sol.DarkTargets.Contains(tidx))
            Act2Sol.DarkTargets.Add(tidx);
        if (Act2Sol.DarkTargets.Contains(tidx) && !Act2Sol.DarkTargets.Contains(sidx))
            Act2Sol.DarkTargets.Add(sidx);

        if (Act2Sol.FireTargets.Contains(sidx) && !Act2Sol.FireTargets.Contains(tidx))
            Act2Sol.FireTargets.Add(tidx);
        if (Act2Sol.FireTargets.Contains(tidx) && !Act2Sol.FireTargets.Contains(sidx))
            Act2Sol.FireTargets.Add(sidx);
    }

    [ScriptMethod(name: "Body: Phase 2 Large Circle Cast Count Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27150"], userControl: false)]
    public void Act2_CircleCastTimeRecord(Event @event, ScriptAccessory accessory)
    {
        if (phase != P4S_Phase.Act2) return;
        Act2Sol.CircleCastTimes++;
    }

    [ScriptMethod(name: "Body: Phase 2 Position Solution (Cheese)", eventType: EventTypeEnum.Tether, eventCondition: ["Id:00AC"])]
    public async void Act2_SpreadSolution(Event @event, ScriptAccessory accessory)
    {
        if (phase != P4S_Phase.Act2) return;
        if (Drawn[2]) return;
        Drawn[2] = true;
        if (Act2Strategy != Act2StrategyEnum.Cheese_Spread) return;

        await Task.Delay(500);
        drawSpreadDir(Act2Sol, accessory);
    }

    private static void drawSpreadDir(Act2Solution act2sol, ScriptAccessory accessory)
    {
        bool isNorth = false;
        bool isEastDown = false;
        bool isEastUp = false;
        bool isSouth = false;
        bool isWestDown = false;
        bool isWestUp = false;
        int myIndex = accessory.getMyIndex();

        if (act2sol.DarkTargets.Contains(myIndex))
            isNorth = true;
        if (act2sol.FireTargets.Contains(myIndex))
        {
            if (accessory.Data.MyObject is { } o && o.IsDps())
            {
                isWestDown = true;
                isEastDown = true;
            }
            else
            {
                isWestUp = true;
                isEastUp = true;
            }
        }
        if (act2sol.WindTargets.Contains(myIndex))
            isSouth = true;

        var dp = accessory.dirPos2Pos(CENTER, CENTER.ExtendPoint(0, 20), 0, 10000, $"North Navigation");
        dp.Color = isNorth ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
        dp.Scale = new(2f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

        dp = accessory.dirPos2Pos(CENTER, CENTER.ExtendPoint(float.Pi, 20), 0, 10000, $"South Navigation");
        dp.Color = isSouth ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
        dp.Scale = new(2f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

        dp = accessory.dirPos2Pos(CENTER, CENTER.ExtendPoint(float.Pi * 0.42f, 20), 0, 10000, $"East-Top Navigation");
        dp.Color = isEastUp ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
        dp.Scale = new(2f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

        dp = accessory.dirPos2Pos(CENTER, CENTER.ExtendPoint(float.Pi * -0.42f, 20), 0, 10000, $"West-Top Navigation");
        dp.Color = isWestUp ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
        dp.Scale = new(2f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

        dp = accessory.dirPos2Pos(CENTER, CENTER.ExtendPoint(float.Pi * 0.58f, 20), 0, 10000, $"East-Bottom Navigation");
        dp.Color = isEastDown ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
        dp.Scale = new(2f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

        dp = accessory.dirPos2Pos(CENTER, CENTER.ExtendPoint(float.Pi * -0.58f, 20), 0, 10000, $"West-Bottom Navigation");
        dp.Color = isWestDown ? posColorPlayer.V4.WithW(2f) : posColorNormal.V4;
        dp.Scale = new(2f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "Body: Phase 2 Position Solution (Regular) Step 1", eventType: EventTypeEnum.Tether, eventCondition: ["Id:00AC"])]
    public async void Act2_RegularSolutionFirst(Event @event, ScriptAccessory accessory)
    {
        if (phase != P4S_Phase.Act2) return;
        if (Drawn[3]) return;
        Drawn[3] = true;
        if (Act2Strategy != Act2StrategyEnum.Regular) return;

        await Task.Delay(500);

        var myIndex = accessory.getMyIndex();
        if (Act2Sol.DarkTargets.Contains(myIndex))
        {
            drawDarkTargetRouteFirst(Act2Sol, myIndex, false, accessory);
            drawDarkTargetTowerFirst(Act2Sol, myIndex, true, accessory);
        }
        else
        {
            var dp = accessory.dirPos(CENTER, 0, 10000, $"Phase 2 Other1{myIndex}");
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            if (Act2Sol.FireTargets.Contains(myIndex))
                drawFireTargetStackFirst(Act2Sol, myIndex, true, accessory);
            if (Act2Sol.WindTargets.Contains(myIndex))
                drawWindTargetStackFirst(Act2Sol, myIndex, true, accessory);
        }
    }

    private static void drawDarkTargetRouteFirst(Act2Solution act2sol, int myIndex, bool isPreparing, ScriptAccessory accessory)
    {
        Vector3 UPLEFT = CENTER.ExtendPoint((act2sol.isLRSafeFirst ? 45f : -45f).angle2Rad(), 15);
        float _rot_radian;
        if (accessory.Data.MyObject is { } o && o.IsTank())
            _rot_radian = 0;
        else
            _rot_radian = float.Pi;
        var spos = UPLEFT.RotatePoint(CENTER, _rot_radian);
        var dp = accessory.dirPos(spos, 0, 10000, $"Phase 2 Dark1{isPreparing}{myIndex}");
        dp.Color = isPreparing ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    private static void drawDarkTargetTowerFirst(Act2Solution act2sol, int myIndex, bool isPreparing, ScriptAccessory accessory)
    {
        int _tower_idx;
        if (accessory.Data.MyObject is { } o && o.IsTank())
            _tower_idx = 0;
        else
            _tower_idx = 2;
        if (act2sol.isLRSafeFirst)
            _tower_idx++;

        var dp = accessory.dirPos(act2sol.TowerPos[_tower_idx], 0, 10000, $"Phase 2 Dark Tower1{isPreparing}{myIndex}");
        dp.Color = isPreparing ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    private static void drawFireTargetStackFirst(Act2Solution act2sol, int myIndex, bool isPreparing, ScriptAccessory accessory)
    {
        int _circle_idx;

        if (accessory.Data.MyObject is { } o && !o.IsDps())
        {
            if (accessory.Data.MyObject.IsTank())
                _circle_idx = 0;
            else
                _circle_idx = 2;
        }
        else
            _circle_idx = 0;

        if (act2sol.isLRSafeFirst)
            _circle_idx++;

        var dp = accessory.dirPos(act2sol.CirclePos[_circle_idx], 0, 10000, $"Phase 2 Fire Stack1{isPreparing}{myIndex}");
        dp.Color = isPreparing ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    private static void drawWindTargetStackFirst(Act2Solution act2sol, int myIndex, bool isPreparing, ScriptAccessory accessory)
    {
        int _circle_idx = 2;

        if (act2sol.isLRSafeFirst)
            _circle_idx++;

        var dp = accessory.dirPos(act2sol.CirclePos[_circle_idx], 0, 10000, $"Phase 2 Wind Stack1{isPreparing}{myIndex}");
        dp.Color = isPreparing ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "Body: Phase 2 Position Solution (Regular) Step 2", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27178"])]
    public void Act2_RegularSolutionSecond(Event @event, ScriptAccessory accessory)
    {
        if (phase != P4S_Phase.Act2) return;
        if (Drawn[4]) return;
        Drawn[4] = true;
        if (Act2Strategy != Act2StrategyEnum.Regular) return;

        accessory.Method.RemoveDraw($"^(Phase 2 Fire Stack1{true}.*)$");
        accessory.Method.RemoveDraw($"^(Phase 2 Wind Stack1{true}.*)$");
        accessory.Method.RemoveDraw($"^(Phase 2 Dark1{false}.*)$");
        accessory.Method.RemoveDraw($"^(Phase 2 Dark Tower1{true}.*)$");
        accessory.Method.RemoveDraw($"^(Phase 2 Other1.*)$");

        var myIndex = accessory.getMyIndex();
        if (Act2Sol.DarkTargets.Contains(myIndex))
        {
            drawDarkTargetTowerFirst(Act2Sol, myIndex, false, accessory);
            drawDarkTargetRouteSecond(Act2Sol, myIndex, true, accessory);
        }
        else
        {
            if (Act2Sol.FireTargets.Contains(myIndex))
            {
                drawFireTargetStackFirst(Act2Sol, myIndex, false, accessory);
                drawFireTargetRouteSecond(Act2Sol, myIndex, true, accessory);
            }
            if (Act2Sol.WindTargets.Contains(myIndex))
            {
                drawWindTargetStackFirst(Act2Sol, myIndex, false, accessory);
                drawWindTargetStackSecond(Act2Sol, myIndex, true, accessory);
            }
        }
    }
    private static void drawDarkTargetRouteSecond(Act2Solution act2sol, int myIndex, bool isPreparing, ScriptAccessory accessory)
    {
        int _pos_idx = 1;
        if (act2sol.isLRSafeFirst)
            _pos_idx++;

        var isTank = accessory.Data.MyObject is { } o && o.IsTank();            
        var _target_pos = isTank ? act2sol.CirclePos[_pos_idx] : act2sol.TowerPos[_pos_idx];
        var dp = accessory.dirPos(_target_pos, 0, 10000, $"Phase 2 Dark2{isPreparing}{myIndex}");
        dp.Color = isPreparing ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    private static void drawWindTargetStackSecond(Act2Solution act2sol, int myIndex, bool isPreparing, ScriptAccessory accessory)
    {
        int _circle_idx = 3;

        if (act2sol.isLRSafeFirst)
            _circle_idx = 0;

        var dp = accessory.dirPos(act2sol.CirclePos[_circle_idx], 0, 10000, $"Phase 2 Wind2{isPreparing}{myIndex}");
        dp.Color = isPreparing ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    private static void drawFireTargetRouteSecond(Act2Solution act2sol, int myIndex, bool isPreparing, ScriptAccessory accessory)
    {
        int _pos_idx;

        if (accessory.Data.MyObject is { } o && !o.IsDps())
        {
            if (accessory.Data.MyObject.IsTank())
                _pos_idx = 1;
            else
                _pos_idx = 3;
        }
        else
        {
            if (act2sol.FireTargets.Max() == myIndex)
                _pos_idx = 1;
            else
                _pos_idx = 3;
        }

        if (act2sol.isLRSafeFirst)
            _pos_idx = _pos_idx == 3 ? 0 : _pos_idx + 1;

        var isHealer = accessory.Data.MyObject is { } o2 && o2.IsHealer();
        var _target_pos = isHealer ? act2sol.TowerPos[_pos_idx] : act2sol.CirclePos[_pos_idx];
        var dp = accessory.dirPos(_target_pos, 0, 10000, $"Phase 2 Fire2{isPreparing}{myIndex}");
        dp.Color = isPreparing ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "Body: Phase 2 Position Solution (Regular) Step 3", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:27150"])]
    public void Act2_RegularSolutionThird(Event @event, ScriptAccessory accessory)
    {
        if (phase != P4S_Phase.Act2) return;
        if (Drawn[5]) return;
        Drawn[5] = true;
        if (Act2Strategy != Act2StrategyEnum.Regular) return;

        accessory.Method.RemoveDraw($"^(Phase 2 Fire2{true}.*)$");
        accessory.Method.RemoveDraw($"^(Phase 2 Wind2{true}.*)$");
        accessory.Method.RemoveDraw($"^(Phase 2 Dark2{true}.*)$");
        accessory.Method.RemoveDraw($"^(Phase 2 Fire Stack1{false}.*)$");
        accessory.Method.RemoveDraw($"^(Phase 2 Wind Stack1{false}.*)$");
        accessory.Method.RemoveDraw($"^(Phase 2 Dark Tower1{false}.*)$");

        var myIndex = accessory.getMyIndex();
        if (Act2Sol.DarkTargets.Contains(myIndex))
        {
            drawDarkTargetRouteSecond(Act2Sol, myIndex, false, accessory);
        }
        else
        {
            if (Act2Sol.FireTargets.Contains(myIndex))
            {
                drawFireTargetRouteSecond(Act2Sol, myIndex, false, accessory);
            }
            if (Act2Sol.WindTargets.Contains(myIndex))
            {
                drawWindTargetStackSecond(Act2Sol, myIndex, false, accessory);
            }
        }
    }

    [ScriptMethod(name: "Body: Phase 2 Position Solution Removal", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:27150"], userControl: false)]
    public void Act2_RegularSolutionRemove(Event @event, ScriptAccessory accessory)
    {
        if (phase != P4S_Phase.Act2) return;
        if (Act2Sol.CircleCastTimes < 4) return;
        accessory.Method.RemoveDraw($".*");
    }
}
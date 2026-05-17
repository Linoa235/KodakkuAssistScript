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

namespace UsamisScript.EndWalker.Rubicante;

[ScriptType(name: "Rubicante", territorys: [], $15067fdd9-effe-4eec-8f10-78dfb50b5568", version: "0.0.0.1", Author: "Linoa235", guid: "8d733cd2-daf9-4fef-b88d-4270faf0c4f0")]

public class Rubicante
{
    const string noteStr =
    """
    v0.0.0.2:
    Duckmen.
    """;
    
    private const string UpdateInfo =
        """
        1. Adapted to Kodakku 0.5.x.x
        """;

    [UserSetting("Debug mode, turn off unless developing")]
    public static bool DebugMode { get; set; } = false;

    const uint CLOCKWISE = 0x00020001;    // EnvControl State represents clockwise rotation
    const uint COUNTER = 0x00200010;      // EnvControl State represents counterclockwise rotation
    const uint INNER = 0x00000001;        // EnvControl Index represents inner magic circle
    const uint MIDDLE = 0x00000002;       // EnvControl Index represents middle magic circle
    const uint OUTER = 0x00000003;        // EnvControl Index represents outer magic circle
    const uint FAN = 15750;               // Outer magic circle blue fan SourceDataId
    const uint INN = 15765;               // Inner magic circle SourceDataId
    const uint MID = 15766;               // Middle magic circle SourceDataId
    const uint OUT = 15767;               // Outer magic circle SourceDataId
    const uint SINGLE_LINE = 542;         // EnvControl Param inner magic circle is single line
    const uint V_SHAPE = 543;             // EnvControl Param inner magic circle is V-shape
    const uint DOUBLE_LINE = 544;         // EnvControl Param inner magic circle is double line
    const uint NEKO = 545;                // EnvControl Param middle magic circle is cat
    const uint EIGHTPOS = 546;            // EnvControl Param outer magic circle is eight-direction

    List<IBattleChara?> InnerCircleId = [null, null, null];
    List<bool> isCaptured = [false, false, false];
    List<int> InnCircleFaceDir = [0, 0, 0];      // Inner magic circle facing
    List<uint> InnCircleType = [0, 0, 0];        // Inner magic circle type
    List<int> RotateCircleDir = [0, 0, 0];       // Large wheel rotation direction: -1 counterclockwise, +1 clockwise, 0 no rotation/unknown
    List<bool> OuterFanMagic = [false, false, false, false, false, false, false, false];   // Outer magic circle distribution: true for fan, false for half-room cleave
    static List<int> NekoOutputAtDir0 = [0, 1, -1, 2, 4, 6, -1, 7];    // When cat direction is 0, input to output mapping
    List<int> OuterTargetMagicDir = [-1, -1];    // Large wheel target
    List<uint> IntercardFlags = [0x02000200, 0x00200020, 0x00020002, 0x00800080];   // Diagonal safe State markers
    public void Init(ScriptAccessory accessory)
    {
        InnerCircleId = [null, null, null];
        isCaptured = [false, false, false];

        InnCircleFaceDir = [0, 0, 0];       // Inner magic circle facing
        InnCircleType = [0, 0, 0];          // Inner magic circle type

        RotateCircleDir = [0, 0, 0];        // Large wheel rotation direction: -1 counterclockwise, +1 clockwise, 0 no rotation
        OuterFanMagic = [false, false, false, false, false, false, false, false];   // Outer magic circle distribution: true for fan, false for half-room cleave
        NekoOutputAtDir0 = [0, 1, -1, 2, 4, 6, -1, 7];    // When cat direction is 0, input to output mapping
        OuterTargetMagicDir = [-1, -1];      // Large wheel target

        IntercardFlags = [0x02000200, 0x00200020, 0x00020002, 0x00800080];   // Diagonal safe State markers

        accessory.Method.MarkClear();
        accessory.Method.RemoveDraw(".*");
    }

    #region Large Wheel
    [ScriptMethod(name: "Purgatorial Magic Circle Outer Type Record (uncontrolled)", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:15750"], userControl: false)]
    public void MagicCircleOuterTypeRecord(Event @event, ScriptAccessory accessory)
    {
        var srot = @event.SourceRotation();
        var sdid = @event.SourceDataId();
        var sdir = DirectionCalc.DirRadRoundToDirs(DirectionCalc.BaseInnGame2DirRad(srot), 8);
        OuterFanMagic[sdir] = sdid == FAN ? true : false;
    }

    [ScriptMethod(name: "Purgatorial Magic Circle Inner Rotation Direction Record (uncontrolled)", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:regex:^(0000000[123])$"], userControl: false)]
    public void MagicCircleRotationRecord(Event @event, ScriptAccessory accessory)
    {
        var idx = @event.Index();
        var rotate_dir = @event.State();
        int mcidx;
        int mcdir;
        string log = "";

        var idxMapping = new Dictionary<uint, (int _mcidx, string _log)>
        {
            { INNER, (0, "Inner ring rotating, ") },
            { MIDDLE, (1, "Middle ring rotating, ") },
            { OUTER, (2, "Outer ring rotating, ") }
        };

        var dirMapping = new Dictionary<uint, (int _mcdir, string _log)>
        {
            { COUNTER, (-1, "counterclockwise") },
            { CLOCKWISE, (1, "clockwise") }
        };

        if (idxMapping.ContainsKey(idx))
        {
            var (_mcidx, _log) = idxMapping[idx];
            log = log + _log;
            mcidx = _mcidx;
        }
        else
        {
            log = log + "Unknown ring rotating, ";
            mcidx = 0;
        }

        if (dirMapping.ContainsKey(rotate_dir))
        {
            var (_mcdir, _log) = dirMapping[rotate_dir];
            log = log + _log;
            mcdir = _mcdir;
        }
        else
        {
            log = log + "unknown direction";
            mcdir = 0;
        }
        RotateCircleDir[mcidx] = mcdir;
        DebugMsg(log, accessory);
    }

    [ScriptMethod(name: "Purgatorial Magic Circle Inner Type and ID Record (uncontrolled)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["Param:regex:^(54[23456])$"], userControl: false)]
    public void MagicCircleTypeRecord(Event @event, ScriptAccessory accessory)
    {
        var param = @event.Param();
        var tid = @event.TargetId();

        var paramMapping = new Dictionary<uint, (int _idx, string _pos, string _type)>
        {
            { SINGLE_LINE, (0, "inner ring", "single line") },
            { V_SHAPE, (0, "inner ring", "V-shape") },
            { DOUBLE_LINE, (0, "inner ring", "double line") },
            { NEKO, (1, "middle ring", "cat")},
            { EIGHTPOS, (2, "outer ring", "eight-direction")}
        };
        if (!paramMapping.ContainsKey(param)) return;
        var (_idx, _pos, _type) = paramMapping[param];

        if (!isCaptured[_idx])
        {
            isCaptured[_idx] = true;
            InnerCircleId[_idx] = (IBattleChara?)accessory.GetById(tid);
            DebugMsg($"Confirmed {_pos} ID: {tid}", accessory);
        }

        var rot = InnerCircleId[_idx]?.Rotation ?? 0f;
        var logic_dir = DirectionCalc.DirRadRoundToDirs(DirectionCalc.BaseInnGame2DirRad((float)rot), 8);
        InnCircleType[_idx] = param;
        InnCircleFaceDir[_idx] = logic_dir;
        DebugMsg($"Detected {_pos} magic circle type: {_type}, logic direction {logic_dir}", accessory);
    }

    [ScriptMethod(name: "Large Wheel Algorithm", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(31940|33000)$"], userControl: false)]
    public void PurgationAlgorithm(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(100).ContinueWith(t =>
        {
            var startPos = getFireStartPos(InnCircleType[0], InnCircleFaceDir[0] + RotateCircleDir[0]);
            string startPosStr = string.Join(", ", startPos);
            DebugMsg($"- Detected start point: {startPosStr}", accessory);

            List<int> NekoResponse = rotateNekoResponse(InnCircleFaceDir[1] + RotateCircleDir[1]);
            string NekoTarget = string.Join(", ", NekoResponse);
            DebugMsg($"- Detected cat head response: {NekoTarget}", accessory);

            for (int i = 0; i < 2; i++)
            {
                if (startPos[i] != -1)
                {
                    OuterTargetMagicDir[i] = NekoResponse[startPos[i]];
                }
            }
            string outerTarget = string.Join(", ", OuterTargetMagicDir);
            DebugMsg($"- Detected large wheel target: {outerTarget}", accessory);

            int count = OuterFanMagic.Count();
            string RotateCircleDirStr = string.Join(", ", RotateCircleDir);
            DebugMsg($"- Detected large wheel rotation data: {RotateCircleDirStr}", accessory);

            var RotateParam = RotateCircleDir[2] == -1 ? 7 : RotateCircleDir[2];
            List<bool> OuterFanMagicBias = new List<bool>(OuterFanMagic.Skip(count - RotateParam).Concat(OuterFanMagic.Take(count - RotateParam)));
            for (int i = 0; i < 2; i++)
            {
                if (OuterTargetMagicDir[i] != -1)
                {
                    drawOuterMagic(OuterTargetMagicDir[i], OuterFanMagicBias[OuterTargetMagicDir[i]], accessory);
                }
            }
        });
    }

    public static void drawOuterMagic(int outDir, bool magicTypeIsFan, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        float radian = (float)Math.PI / 4 * outDir;

        dp.Name = $"Outer magic circle {outDir}";
        dp.Position = DirectionCalc.RotatePoint(new(100, 0, 80), new(100, 0, 100), radian);
        dp.TargetPosition = new(100, 0, 100);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = 0;
        dp.DestoryAt = 20000;

        dp.Scale = magicTypeIsFan ? new(45f) : new(45, 20);
        dp.Radian = magicTypeIsFan ? (float)Math.PI / 3 : (float)Math.PI * 2;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, magicTypeIsFan ? DrawTypeEnum.Fan : DrawTypeEnum.Rect, dp);
    }

    public static int[] getFireStartPos(uint innType, int innDir)
    {
        int startPos1;
        int startPos2;
        switch (innType)
        {
            case SINGLE_LINE:
                startPos1 = innDir;
                startPos2 = -1;
                break;
            case V_SHAPE:
                startPos1 = innDir;
                startPos2 = (innDir + 2) % 8;
                break;
            case DOUBLE_LINE:
                startPos1 = innDir;
                startPos2 = (innDir + 4) % 8;
                break;
            default:
                startPos2 = -1;
                startPos1 = -1;
                break;
        }
        return [startPos1, startPos2];
    }

    public static List<int> rotateNekoResponse(int nekoDir)
    {
        var nekoDirVar = nekoDir;
        if (nekoDir > 7) nekoDirVar = nekoDirVar - 8;
        if (nekoDir < 0) nekoDirVar = nekoDirVar + 8;

        List<int> nekoResponse = new List<int>(NekoOutputAtDir0);

        for (int i = 0; i < nekoResponse.Count(); i++)
        {
            if (nekoResponse[i] == -1) continue;
            nekoResponse[i] += nekoDirVar;
            nekoResponse[i] = nekoResponse[i] % 8;
        }

        int count = nekoResponse.Count();
        nekoResponse = new List<int>(nekoResponse.Skip(count - nekoDirVar).Concat(nekoResponse.Take(count - nekoDirVar)));

        return nekoResponse;
    }

    [ScriptMethod(name: "Large Wheel Parameter Initialization (uncontrolled)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(31940|33000)$"], userControl: false)]
    public void PurgationInit(Event @event, ScriptAccessory accessory)
    {
        InnCircleFaceDir = [0, 0, 0];
        InnCircleType = [0, 0, 0];
        RotateCircleDir = [0, 0, 0];
        OuterFanMagic = [false, false, false, false, false, false, false, false];
        OuterTargetMagicDir = [-1, -1];
        accessory.Method.RemoveDraw(".*");

        DebugMsg($"- Large wheel parameters initialized.", accessory);
    }

    #endregion

    #region Flamespire
    [ScriptMethod(name: "Flamespire Initial Danger Zone", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:00000004"])]
    public async void Flamespire(Event @event, ScriptAccessory accessory)
    {
        var state = @event.State();
        bool isInterCard = false;
        if (state == 0x00080004)
        {
            await Task.Delay(2500);
            accessory.Method.RemoveDraw($"Flamespire Initial Danger Zone");
            return;
        }
        if (IntercardFlags.Contains(state))
        {
            DebugMsg($"Detected diagonal safety first", accessory);
            isInterCard = true;
        }

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Flamespire Initial Danger Zone";

        if (!isInterCard)
        {
            dp.Position = new(100, 0, 100);
            dp.Rotation = (float)Math.PI / 4;
            dp.Scale = new(12, 40);
            dp.Delay = 0;
            dp.DestoryAt = 20000;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(1.5f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

            dp.Rotation = (float)Math.PI / -4;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
        else
        {
            dp.Position = new(100, 0, 100);
            dp.Rotation = 0;
            dp.Scale = new(12, 40);
            dp.Delay = 0;
            dp.DestoryAt = 20000;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(1.5f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

            dp.Rotation = (float)Math.PI / 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
    }
    #endregion

    public static void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!DebugMode) return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    [ScriptMethod(name: "Anytime DEBUG", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:=TST"], userControl: false)]
    public void EchoDebug(Event @event, ScriptAccessory accessory)
    {
        if (!DebugMode) return;
        var msg = @event["Message"].ToString();
        accessory.Method.SendChat($"/e Received player message: {msg}");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Cone tankbuster";
        dp.Scale = new(60);
        dp.Position = new(100, 0, 100);
        dp.TargetObject = accessory.Data.PartyList[0];
        dp.Radian = (float)Math.PI / 1.5f;
        dp.Delay = 0;
        dp.DestoryAt = 5000;
        dp.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
}
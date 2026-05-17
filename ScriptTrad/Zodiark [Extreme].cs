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

namespace UsamisScript.EndWalker.ZodiarkEx;

[ScriptType(name: "Zodiark [Extreme]", territorys: [993],
    guid: "e24a0c8b-5c41-4e58-87c3-355f1f925986", version: "0.0.0.6", author: "Usami", note: noteStr)]
public class ZodiarkEx
{
    const string noteStr =
    """
    v0.0.0.6:
    Duckmen.
    """;

    [UserSetting("Debug mode, turn off unless developing")]
    public bool DebugMode { get; set; } = false;

    int ParadeigmaNum = 0;      // Paradigm count
    Vector3[] BirdOrBeastPos = new Vector3[4];    // Quetzalcoatl & Behemoth positions
    Vector3[] SnakePos = new Vector3[16];         // Python positions
    Vector3[] SnakePosTarget = new Vector3[16];   // Python target positions
    List<uint> EsoterikosSourceIds = [];          // Recorded esoterikos IDs
    public ScriptColor colorPink = new ScriptColor { V4 = new Vector4(1f, 0f, 1f, 1.0f) };
    public ScriptColor colorRed = new ScriptColor { V4 = new Vector4(1f, 0f, 0f, 1.0f) };
    bool isTurnLeft = false;                      // Rotation direction
    bool isStarFall = false;                      // Transition phase, for triple esoterikos drawing
    Dictionary<string, (int, int)>? AstralMapping = null;
    int AstralNum = 0;          // Astral count
    int AstralType = 0;         // Astral safe zone type
    Vector3[] AstralSafePos = new Vector3[6];   // 6 astral safe points
    public void Init(ScriptAccessory accessory)
    {
        ParadeigmaNum = 0;

        BirdOrBeastPos[0] = new(89.50f, 0, 89.50f);    // Top-left
        BirdOrBeastPos[1] = new(110.50f, 0, 89.50f);   // Top-right
        BirdOrBeastPos[2] = new(89.50f, 0, 110.50f);   // Bottom-left
        BirdOrBeastPos[3] = new(110.50f, 0, 110.50f);  // Bottom-right

        SnakePos[0] = new(85.00f, 0, 75.00f);
        SnakePos[1] = new(95.00f, 0, 75.00f);
        SnakePos[2] = new(105.00f, 0, 75.00f);
        SnakePos[3] = new(115.00f, 0, 75.00f);

        SnakePos[4] = new(125.00f, 0, 85.00f);
        SnakePos[5] = new(125.00f, 0, 95.00f);
        SnakePos[6] = new(125.00f, 0, 105.00f);
        SnakePos[7] = new(125.00f, 0, 115.00f);

        SnakePos[8] = new(115.00f, 0, 125.00f);
        SnakePos[9] = new(105.00f, 0, 125.00f);
        SnakePos[10] = new(95.00f, 0, 125.00f);
        SnakePos[11] = new(85.00f, 0, 125.00f);

        SnakePos[12] = new(75.00f, 0, 115.00f);
        SnakePos[13] = new(75.00f, 0, 105.00f);
        SnakePos[14] = new(75.00f, 0, 95.00f);
        SnakePos[15] = new(75.00f, 0, 85.00f);

        SnakePosTarget[0] = SnakePos[11];
        SnakePosTarget[1] = SnakePos[10];
        SnakePosTarget[2] = SnakePos[9];
        SnakePosTarget[3] = SnakePos[8];
        SnakePosTarget[4] = SnakePos[15];
        SnakePosTarget[5] = SnakePos[14];
        SnakePosTarget[6] = SnakePos[13];
        SnakePosTarget[7] = SnakePos[12];
        SnakePosTarget[8] = SnakePos[3];
        SnakePosTarget[9] = SnakePos[2];
        SnakePosTarget[10] = SnakePos[1];
        SnakePosTarget[11] = SnakePos[0];
        SnakePosTarget[12] = SnakePos[7];
        SnakePosTarget[13] = SnakePos[6];
        SnakePosTarget[14] = SnakePos[5];
        SnakePosTarget[15] = SnakePos[4];

        AstralSafePos[0] = new(86, 0, 86);
        AstralSafePos[1] = new(100, 0, 86);
        AstralSafePos[2] = new(114, 0, 86);
        AstralSafePos[3] = new(86, 0, 100);
        AstralSafePos[4] = new(100, 0, 100);
        AstralSafePos[5] = new(114, 0, 100);

        EsoterikosSourceIds = [];

        isTurnLeft = false;
        isStarFall = false;

        AstralMapping = new Dictionary<string, (int val1, int val2)>
        {
            {"00020001", (1, 1)},
            {"00800040", (10, 10)},
            {"10000800", (2, 20)},
            {"00200010", (3, 30)}
        };

        AstralNum = 0;
        AstralType = 0;

        accessory.Method.RemoveDraw(".*");
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

    private void DebugMsg(string str, ScriptAccessory accessory)
    {
        if (!DebugMode) return;
        accessory.Method.SendChat(str);
    }

    [ScriptMethod(name: "Anytime DEBUG", eventType: EventTypeEnum.Chat, eventCondition: ["Type:Echo", "Message:=TST"], userControl: false)]
    public void EchoDebug(Event @event, ScriptAccessory accessory)
    {
        if (!DebugMode) return;
        var msg = @event["Message"].ToString();
        accessory.Method.SendChat($"/e Received player message: {msg}");

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Heavy";
        dp.Scale = new(3f);
        dp.Position = new(86f, 0, 86f);
        dp.Delay = 0;
        dp.DestoryAt = 2000;
        dp.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "[Global] Paradigm Count Record (uncontrolled)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26559"], userControl: false)]
    public void ParadeigmaNumRecord(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        ParadeigmaNum++;
        isStarFall = false;
        DebugMsg($"/e [DEBUG] Paradigm count increased: {ParadeigmaNum}.", accessory);
    }

    private void drawBirdDonut(int birdIdx, int delay, int destoryAt, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Donut {birdIdx}";
        dp.Scale = new(15);
        dp.InnerScale = new(5);
        dp.Radian = float.Pi * 2;
        dp.Position = BirdOrBeastPos[birdIdx];
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        dp.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }

    private void drawBeastCircle(int beastIdx, int delay, int destoryAt, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Heavy {beastIdx}";
        dp.Scale = new(15);
        dp.Position = BirdOrBeastPos[beastIdx];
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        dp.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    private void drawSnakeLine(int[] SnakeIdx, int delay, int destoryAt, ScriptAccessory accessory)
    {
        DebugMsg($"/e [DEBUG] Found snake {SnakeIdx[0]} -> {SnakePosTarget[SnakeIdx[0]]} and {SnakeIdx[1]} -> {SnakePosTarget[SnakeIdx[1]]}", accessory);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Line {SnakeIdx[0]}";
        dp.Scale = new(11);
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Position = SnakePos[SnakeIdx[0]];
        dp.TargetPosition = SnakePosTarget[SnakeIdx[0]];
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        dp.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        dp.Name = $"Line {SnakeIdx[1]}";
        dp.Position = SnakePos[SnakeIdx[1]];
        dp.TargetPosition = SnakePosTarget[SnakeIdx[1]];
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    private void drawFan(uint sid, int delay, int destoryAt, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Triangle Esoterikos {sid}";
        dp.Scale = new(60);
        dp.Radian = float.Pi / 3;
        dp.Owner = sid;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        dp.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    private void drawHalfCleave(uint sid, int delay, int destoryAt, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Half-Room Esoterikos {sid}";
        dp.Scale = new(42, 21);
        dp.Owner = sid;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        dp.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    private void drawLine(uint sid, int delay, int destoryAt, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Line Esoterikos {sid}";
        dp.Scale = new(16, 42);
        dp.Owner = sid;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        dp.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Stack Marker", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:013C"])]
    public void GenerateTargetRecord(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        DebugMsg($"/e Detected {tid} marked for stack.", accessory);

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Stack {tid}";
        dp.Scale = new(5);
        dp.Owner = tid;
        dp.Delay = 0;
        dp.DestoryAt = 12000;
        dp.Color = accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Esoterikos (Triangle, Half-Room, Line)", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:regex:^(1371[123])$"])]
    public void Exoterikos(Event @event, ScriptAccessory accessory)
    {
        lock (this)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            if (EsoterikosSourceIds.Contains(sid))
            {
                DebugMsg($"/e [DEBUG] Removing esoterikos: {sid}.", accessory);
                EsoterikosSourceIds.Remove(sid);
                accessory.Method.RemoveDraw(@$"(Line|Triangle|Half-Room)Esoterikos{sid}");
                return;
            }
            else
                EsoterikosSourceIds.Add(sid);

            var sdid = JsonConvert.DeserializeObject<uint>(@event["SourceDataId"]);

            DebugMsg($"/e [DEBUG] Found esoterikos {sid} casting: {sdid}.", accessory);

            switch (sdid)
            {
                case 13711:
                    if (isStarFall)
                        drawLine(sid, 5000, 25000, accessory);
                    else
                        drawLine(sid, 0, 25000, accessory);
                    break;
                case 13712:
                    drawHalfCleave(sid, 0, 25000, accessory);
                    break;
                case 13713:
                    drawFan(sid, 0, 25000, accessory);
                    break;
                default:
                    return;
            }
        }
    }

    [ScriptMethod(name: "Mourning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26601"], userControl: false)]
    public void OrbsDownSync(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(50).ContinueWith(t =>
        {
            EsoterikosSourceIds = [];
            isStarFall = true;
            accessory.Method.RemoveDraw(".*");
        });
    }

    [ScriptMethod(name: "Algedon (Diagonal Charge)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26606"])]
    public void Algedon(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Algedon {sid}";
        dp.Scale = new(30, 60);
        dp.Owner = sid;
        dp.Delay = 0;
        dp.DestoryAt = 8000;
        dp.Color = colorPink.V4.WithW(1.5f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }

    [ScriptMethod(name: "Adikia (Small Fist)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26609"])]
    public void Adikia(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Adikia {sid}";
        dp.Scale = new(21);
        dp.Position = new Vector3(121.0f, 0, 100.0f);
        dp.Delay = 0;
        dp.DestoryAt = 7500;
        dp.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        dp.Position = new Vector3(79.0f, 0, 100.0f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Astral (Transition Starfall)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26599"])]
    public void Astral(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Astral {sid}";
        dp.Scale = new(10);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = sid;
        dp.Delay = 0;
        dp.DestoryAt = 3000;
        dp.Color = accessory.Data.DefaultDangerColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(name: "Bird Donut", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:80034E71", "Id:00200010", "Index:regex:^(0000001[5678])$"])]
    public async void BirdDonut(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["Index"], out var idx)) return;

        DebugMsg($"/e [DEBUG] Found bird donut {idx}.", accessory);

        switch (ParadeigmaNum)
        {
            case 1:
            case 2:
                drawBirdDonut(getBirdIndex(idx), 0, 20000, accessory);
                break;
            case 3:
            case 4:
            case 5:
            case 6:
                await Task.Delay(5000);
                drawBirdDonut(isTurnLeft ? getBeastBirdTurnLeftIndex(getBirdIndex(idx)) : getBeastBirdTurnRightIndex(getBirdIndex(idx)), 0, 20000, accessory);
                break;
            case 7:
            case 8:
            case 9:
                await Task.Delay(9500);
                drawBirdDonut(isTurnLeft ? getBeastBirdTurnLeftIndex(getBirdIndex(idx)) : getBeastBirdTurnRightIndex(getBirdIndex(idx)), 0, 20000, accessory);
                break;
            default:
                return;
        }
    }

    [ScriptMethod(name: "Behemoth Heavy", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:80034E71", "Id:00200010", "Index:regex:^(0000000[9ABC])$"])]
    public async void BeastCircle(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["Index"], out var idx)) return;

        DebugMsg($"/e [DEBUG] Found behemoth heavy {idx}.", accessory);

        switch (ParadeigmaNum)
        {
            case 1:
            case 2:
                drawBeastCircle(getBeastIndex(idx), 0, 20000, accessory);
                break;
            case 3:
            case 4:
            case 5:
            case 6:
                await Task.Delay(5000);
                drawBeastCircle(isTurnLeft ? getBeastBirdTurnLeftIndex(getBeastIndex(idx)) : getBeastBirdTurnRightIndex(getBeastIndex(idx)), 0, 20000, accessory);
                break;
            case 7:
            case 8:
            case 9:
                await Task.Delay(9500);
                drawBeastCircle(isTurnLeft ? getBeastBirdTurnLeftIndex(getBeastIndex(idx)) : getBeastBirdTurnRightIndex(getBeastIndex(idx)), 0, 20000, accessory);
                break;
            default:
                return;
        }
    }

    [ScriptMethod(name: "Snake Line", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:80034E71", "Id:00200010", "Index:regex:^(000000(0[DEF]|(1[01234])))$"])]
    public async void SnakeLine(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["Index"], out var idx)) return;
        DebugMsg($"/e [DEBUG] Found snake line {idx}.", accessory);

        switch (ParadeigmaNum)
        {
            case 1:
            case 2:
                drawSnakeLine(getSnakeIndex(idx), 0, 20000, accessory);
                break;
            case 3:
                await Task.Delay(9500);
                drawSnakeLine(isTurnLeft ? getSnakeTurnLeftIndex(getSnakeIndex(idx)) : getSnakeTurnRightIndex(getSnakeIndex(idx)), 0, 20000, accessory);
                break;
            case 4:
                drawSnakeLine(getSnakeIndex(idx), 0, 20000, accessory);
                break;
            case 5:
            case 6:
                await Task.Delay(5000);
                drawSnakeLine(isTurnLeft ? getSnakeTurnLeftIndex(getSnakeIndex(idx)) : getSnakeTurnRightIndex(getSnakeIndex(idx)), 0, 20000, accessory);
                break;
            case 7:
                await Task.Delay(9500);
                drawSnakeLine(isTurnLeft ? getSnakeTurnLeftIndex(getSnakeIndex(idx)) : getSnakeTurnRightIndex(getSnakeIndex(idx)), 0, 20000, accessory);
                break;
            case 8:
            case 9:
                await Task.Delay(9500);
                drawSnakeLine(isTurnLeft ? getSnakeTurnLeftIndex(getSnakeIndex(idx)) : getSnakeTurnRightIndex(getSnakeIndex(idx)), 0, 20000, accessory);
                break;
            default:
                return;
        }
    }

    [ScriptMethod(name: "Rotation Direction Record (uncontrolled)", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:80034E71", "Id:regex:^(00200010|00020001)$", "Index:00000002"], userControl: false)]
    public void RotateRecord(Event @event, ScriptAccessory accessory)
    {
        var id = @event["Id"].ToString();
        switch (id)
        {
            case "00200010":
                DebugMsg($"/e [DEBUG] Turning left.", accessory);
                isTurnLeft = true;
                break;

            case "00020001":
                DebugMsg($"/e [DEBUG] Turning right.", accessory);
                isTurnLeft = false;
                break;

            default:
                return;
        }
    }

    [ScriptMethod(name: "Astral Navigation", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:80034E71", "Index:regex:^(0000000[678])$"])]
    public void AstralDir(Event @event, ScriptAccessory accessory)
    {
        if (AstralNum == 2) return;
        AstralNum++;
        var id = @event["Id"].ToString();
        if (AstralMapping.ContainsKey(id))
        {
            var (val1, val2) = AstralMapping[id];
            AstralType = AstralType + (AstralNum == 1 ? val1 : val2);
        }

        if (AstralNum != 2) return;
        DebugMsg($"/e Got AstralType: {AstralType}", accessory);
        switch (AstralType)
        {
            case 31:
                drawAstralDir(3, 4, 2, accessory);
                break;
            case 12:
                drawAstralDir(0, 3, 1, accessory);
                break;
            case 21:
                drawAstralDir(3, 1, 4, accessory);
                break;
            case 32:
                drawAstralDir(0, 4, 2, accessory);
                break;
            case 23:
                drawAstralDir(0, 1, 2, accessory);
                break;
            case 13:
                drawAstralDir(0, 3, 1, accessory);
                break;
            default:
                break;
        }
    }

    private void drawAstralDir(int safeIdx1, int safeIdx2, int safeIdx3, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Astral Safe Zone 1";
        dp.Scale = new(3f);
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Position = AstralSafePos[safeIdx1];
        dp.Delay = 0;
        dp.DestoryAt = 11000;
        dp.Color = accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        dp.Name = $"Astral Safe Zone 2";
        dp.Position = AstralSafePos[safeIdx2];
        dp.Delay = 0;
        dp.DestoryAt = 15000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        dp.Name = $"Astral Safe Zone 3";
        dp.Position = AstralSafePos[safeIdx3];
        dp.Delay = 0;
        dp.DestoryAt = 19000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        
        dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Astral Navigation Marker 1";
        dp.Scale = new(0.5f);
        dp.ScaleMode = ScaleMode.YByDistance;
        dp.Owner = accessory.Data.Me;
        dp.TargetPosition = AstralSafePos[safeIdx1];
        dp.Delay = 0;
        dp.DestoryAt = 11000;
        dp.Color = accessory.Data.DefaultSafeColor;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        dp.Name = $"Astral Navigation Marker 2";
        dp.TargetPosition = AstralSafePos[safeIdx2];
        dp.Delay = 11000;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        dp.Name = $"Astral Navigation Marker 2";
        dp.TargetPosition = AstralSafePos[safeIdx3];
        dp.Delay = 15000;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(name: "Fire Line Safe Zone", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:80034E71", "Id:regex:^(00020001|00400020)$", "Index:00000005"])]
    public async void Firebar(Event @event, ScriptAccessory accessory)
    {
        await Task.Delay(500);
        var id = @event["Id"].ToString();
        switch (id)
        {
            case "00400020":
                DebugMsg($"/e [DEBUG] Detected fire line from top-left to bottom-right.", accessory);
                if (isTurnLeft)
                {
                    drawQuadrant(1, 0, 20000, accessory);
                    drawQuadrant(3, 0, 20000, accessory);
                }
                else
                {
                    drawQuadrant(0, 0, 20000, accessory);
                    drawQuadrant(2, 0, 20000, accessory);
                }
                break;
            case "00020001":
                DebugMsg($"/e [DEBUG] Detected fire line from top-right to bottom-left.", accessory);
                if (isTurnLeft)
                {
                    drawQuadrant(0, 0, 20000, accessory);
                    drawQuadrant(2, 0, 20000, accessory);
                }
                else
                {
                    drawQuadrant(1, 0, 20000, accessory);
                    drawQuadrant(3, 0, 20000, accessory);
                }
                break;
            default:
                return;
        }
    }

    private void drawQuadrant(int posIdx, int delay, int destoryAt, ScriptAccessory accessory)
    {
        Vector3 bias;
        switch (posIdx)
        {
            case 0:
                bias = new Vector3(0, 0, -1.5f);
                break;
            case 1:
                bias = new Vector3(-1.5f, 0, 0);
                break;
            case 2:
                bias = new Vector3(0, 0, 1.5f);
                break;
            case 3:
                bias = new Vector3(1.5f, 0, 0);
                break;
            default:
                return;
        }
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Quadrant {posIdx}";
        dp.Scale = new(30);
        dp.Radian = float.Pi / 2;
        dp.Position = new Vector3(100, 0, 100) + bias;
        dp.Rotation = float.Pi / 2 * posIdx;
        dp.Delay = delay;
        dp.DestoryAt = destoryAt;
        dp.Color = colorRed.V4.WithW(1.5f);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "Bird Donut Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26593"], userControl: false)]
    public void BirdDonutRemove(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(@$"Donut\d+$");
        accessory.Method.RemoveDraw(@$"Quadrant\d+$");
    }

    [ScriptMethod(name: "Behemoth Heavy Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26594"], userControl: false)]
    public void BeastCircleRemove(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(@$"Heavy\d+$");
        accessory.Method.RemoveDraw(@$"Quadrant\d+$");
    }

    [ScriptMethod(name: "Snake Line Remove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26595"], userControl: false)]
    public void SnakeLineRemove(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(@$"Line\d+$");
        accessory.Method.RemoveDraw(@$"Quadrant\d+$");
    }

    private int getBirdIndex(uint index)
    {
        switch (index)
        {
            case 21:
                return 0;
            case 22:
                return 1;
            case 23:
                return 2;
            case 24:
                return 3;
            default:
                return -1;
        }
    }

    private int getBeastIndex(uint index)
    {
        switch (index)
        {
            case 9:
                return 0;
            case 10:
                return 1;
            case 11:
                return 2;
            case 12:
                return 3;
            default:
                return -1;
        }
    }

    private int getBeastBirdTurnRightIndex(int posIndex)
    {
        switch (posIndex)
        {
            case 0:
                return 1;
            case 1:
                return 3;
            case 2:
                return 0;
            case 3:
                return 2;
            default:
                return -1;
        }
    }

    private int getBeastBirdTurnLeftIndex(int posIndex)
    {
        switch (posIndex)
        {
            case 0:
                return 2;
            case 1:
                return 0;
            case 2:
                return 3;
            case 3:
                return 1;
            default:
                return -1;
        }
    }

    private int[] getSnakeIndex(uint index)
    {
        switch (index)
        {
            case 13:
                return [0, 2];
            case 14:
                return [1, 3];
            case 15:
                return [11, 9];
            case 16:
                return [10, 8];
            case 17:
                return [15, 13];
            case 18:
                return [14, 12];
            case 19:
                return [4, 6];
            case 20:
                return [5, 7];
            default:
                return [-1, -1];
        }
    }

    private int[] getSnakeTurnRightIndex(int[] posIndex)
    {
        return [posIndex[0] + 4 > 15 ? posIndex[0] + 4 - 16 : posIndex[0] + 4,
                posIndex[1] + 4 > 15 ? posIndex[1] + 4 - 16 : posIndex[1] + 4];
    }

    private int[] getSnakeTurnLeftIndex(int[] posIndex)
    {
        return [posIndex[0] - 4 < 0 ? posIndex[0] - 4 + 16 : posIndex[0] - 4,
                posIndex[1] - 4 < 0 ? posIndex[1] - 4 + 16 : posIndex[1] - 4];
    }
}
// File: OmegaProtocolUltimate_Unknown.cs
using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
//using ECommons;
using System.Numerics;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;
using System.Xml.Linq;
using Dalamud.Utility.Numerics;
using Dalamud.Game.ClientState.Objects.Types;

namespace MyScriptNamespace
{
    
    [ScriptType(name: "OmegaProtocolUltimate", territorys: [1122],guid: "200e600c-99bb-41cc-a876-c48c6440774a", version:"0.0.0.5",note: noteStr)]
    public class OmegaProtocolUltimate
    {
        const string noteStr =
        """
        Omega Protocol Ultimate Verification Battle
        """;

        [UserSetting("P3_Initial Queue Order")]
        public P3SortEnum P3_StackSort { get; set; }
        [UserSetting("P3_TV Strategy")]
        public P3TVEnum P3_TV_Strategy { get; set; }

        List<int> HtdhParty = [2, 0, 1, 4, 5, 6, 7, 3];
        double parse = 0;

        uint P1_BossId = 0;
        List<int> P1_Buff = [0, 0, 0, 0, 0, 0, 0, 0];
        List<Vector3> P1_TowerPos = [];
        DateTime P1_TowerTime = DateTime.MinValue;
        DateTime P1_FanTime = DateTime.MinValue;
        int P1_LineRound = 0;
        int P1_FireCount = 0;

        bool P2_PTBuffIsFar = false;
        List<int> P2_Sony = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P2_Stack = [0, 0, 0, 0, 0, 0, 0, 0];
        Dictionary<uint,uint> P2_LineTether = [];

        int P3_ArmCount = 0;
        List <int> P3_StartBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        bool P3_StartPreDone = false;
        bool P3_StartDone = false;
        List<int> P3_TVBuff = [0, 0, 0, 0, 0, 0, 0, 0];

        List<int> P4Stack = [];

        int P51_Eye = 0;
        List<int> P51_Buff = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P51_Fist = [0, 0, 0, 0];
        bool P51_FistDone = false;

        public enum P3SortEnum
        {
            HTDH,
            THD
        }
        public enum P3TVEnum
        {
            Normal,
            Static
        }

        public void Init(ScriptAccessory accessory)
        {
            parse = 0;
        }
        #region P1
        [ScriptMethod(name: "P1_LoopProgram_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31491"], userControl:false)]
        public void P1_LoopProgram_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            P1_BossId = tid;
            parse = 1.1;
            P1_TowerPos = [];
            P1_LineRound = 0;
        }
        [ScriptMethod(name: "P1_LoopProgram_BuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3004|3005|3006|3451)$"], userControl:false)]
        public void P1_LoopProgram_BuffRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            P1_Buff[index] = @event["StatusID"] switch
            {
                "3004" => 1,
                "3005" => 2,
                "3006" => 3,
                "3451" => 4,
                _=>0
            };
        }
        [ScriptMethod(name: "P1_LoopProgram_TowerCollection", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Add", "DataId:2013245"], userControl: false)]
        public void P1_LoopProgram_TowerCollection(Event @event, ScriptAccessory accessory)
        {
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            lock (P1_TowerPos)
            {
                P1_TowerPos.Add(pos);
            }
        }
        [ScriptMethod(name: "P1_LoopProgram_GatherAlert", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31491"])]
        public void P1_LoopProgram_GatherAlert(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            accessory.Method.TextInfo("Gather behind the boss", 2000);
            accessory.Method.TTS("Gather behind the boss");
        }
        [ScriptMethod(name: "P1_LoopProgram_StartPositionAlert", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3004|3005|3006|3451)$"])]
        public void P1_LoopProgram_StartPositionAlert(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            if (@event["StatusID"] == "3006")
            {
                accessory.Method.TextInfo("Go forward for tether", 3000);
                accessory.Method.TTS("Forward for tether");
            }
            else
            {
                accessory.Method.TextInfo("Stay back", 3000);
                accessory.Method.TTS("Stay back");
            }
        }
        [ScriptMethod(name: "P1_LoopProgram_TetherTowerPosition", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Add", "DataId:2013245"])]
        public async void P1_LoopProgram_TetherTowerPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            lock (this)
            {
                if ((DateTime.Now - P1_TowerTime).TotalSeconds < 2) return;
                P1_TowerTime = DateTime.Now;
            }
            await Task.Delay(50);
            Vector3 centre = new(100, 0, 100);
            var towerCount = P1_TowerPos.Count;
            List<int> HtdhParty = [2, 0, 1, 4, 5, 6, 7, 3];
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var myBuff = P1_Buff[myindex];
            var index1 = P1_Buff.IndexOf(myBuff);
            var index2 = P1_Buff.LastIndexOf(myBuff);
            var hIndex = HtdhParty.IndexOf(index1) < HtdhParty.IndexOf(index2) ? index1 : index2;
            var meIsHigh = hIndex == myindex;
            var idle = false;
            // Tower
            if (towerCount == myBuff * 2)
            {
                idle = true;
                var hPos = default(Vector3);
                var lPos = default(Vector3);
                if (RoundPositionTo4Dir(P1_TowerPos[towerCount - 2], centre) < RoundPositionTo4Dir(P1_TowerPos[towerCount - 1], centre))
                {
                    hPos = P1_TowerPos[towerCount - 2];
                    lPos = P1_TowerPos[towerCount - 1];
                }
                else
                {
                    hPos = P1_TowerPos[towerCount - 1];
                    lPos = P1_TowerPos[towerCount - 2];
                }
                var dealpos = meIsHigh ? hPos : lPos;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_LoopProgram_TowerPosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_LoopProgram_TowerRange";
                dp.Scale = new(3);
                dp.Position = dealpos;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            // Line
            if (towerCount % 8 == (myBuff + 2) * 2 % 8)
            {
                idle = true;
                List<int> isTower = [0, 0, 0, 0];
                isTower[RoundPositionTo4Dir(P1_TowerPos[towerCount - 2], centre)] = 1;
                isTower[RoundPositionTo4Dir(P1_TowerPos[towerCount - 1], centre)] = 1;
                var my4Dir = meIsHigh ? isTower.IndexOf(0) : isTower.LastIndexOf(0);
                var dealpos = RotatePoint(new(100, 0, 85), centre, float.Pi / 2 * my4Dir);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_LoopProgram_LinePosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            // Idle
            if (!idle)
            {
                var myPos = accessory.Data.Objects.SearchByEntityId(accessory.Data.Me)?.Position ?? default;
                var drot = (myPos - P1_TowerPos[towerCount - 2]).Length() < (myPos - P1_TowerPos[towerCount - 1]).Length() ? RoundPositionTo4Dir(P1_TowerPos[towerCount - 2], centre) : RoundPositionTo4Dir(P1_TowerPos[towerCount - 1], centre);
                var dealpos = RotatePoint(new(100, 0, 86), centre, float.Pi / 2 * drot);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_LoopProgram_IdlePosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        [ScriptMethod(name: "P1_LoopProgram_TetherMark", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31496", "TargetIndex:1"])]
        public void P1_LoopProgram_TetherMark(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            P1_LineRound++;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var waitBuff = (P1_LineRound + 1) % 4 + 1;
            var catchBuff = (P1_LineRound + 2) % 4 + 1;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (catchBuff != P1_Buff[myindex]) return;

            var myBuff = P1_Buff[myindex];
            var index1 = P1_Buff.IndexOf(myBuff);
            var index2 = P1_Buff.LastIndexOf(myBuff);
            var hIndex = HtdhParty.IndexOf(index1) < HtdhParty.IndexOf(index2) ? index1 : index2;
            var meIsHigh = hIndex == myindex;

            var index3 = P1_Buff.IndexOf(waitBuff);
            var index4 = P1_Buff.LastIndexOf(waitBuff);
            var hWaitIndex = HtdhParty.IndexOf(index3) < HtdhParty.IndexOf(index4) ? index3 : index4;
            var lWaitIndex = HtdhParty.IndexOf(index3) < HtdhParty.IndexOf(index4) ? index4 : index3;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_LoopProgram_TetherMark";
            dp.Scale = new(10);
            dp.Owner = tid;
            dp.TargetObject = meIsHigh ? accessory.Data.PartyList[hWaitIndex] : accessory.Data.PartyList[lWaitIndex];
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = new(1,1,0,1);
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
        }
        [ScriptMethod(name: "P1_LoopProgram_TetherMarkRemove", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0059"])]
        public void P1_LoopProgram_TetherMarkRemove(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (sid != accessory.Data.Me) return;
            accessory.Method.RemoveDraw("P1_LoopProgram_TetherMark");
        }

        [ScriptMethod(name: "P1_AlmightyLord_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31499"], userControl: false)]
        public void P1_AlmightyLord_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 1.2;
            P1_FireCount = 0;
        }
        [ScriptMethod(name: "P1_AlmightyLord_BuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3004|3005|3006|3451)$"], userControl: false)]
        public void P1_AlmightyLord_BuffRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            P1_Buff[index] = @event["StatusID"] switch
            {
                "3004" => 1,
                "3005" => 2,
                "3006" => 3,
                "3451" => 4,
                _ => 0
            };
        }
        [ScriptMethod(name: "P1_AlmightyLord_HighLowOrderBroadcast", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31499"])]
        public async void P1_AlmightyLord_HighLowOrderBroadcast(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            await Task.Delay(100);
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var mybuff = P1_Buff[myindex];
            var i1 = P1_Buff.IndexOf(mybuff);
            var i2 = P1_Buff.LastIndexOf(mybuff);
            var hIndex = HtdhParty.IndexOf(i1) < HtdhParty.IndexOf(i2) ? i1 : i2;
            if (hIndex == myindex)
            {
                accessory.Method.TextInfo("High order (top/right)", 10000);
                accessory.Method.TTS("High order (top/right)");
            }
            else
            {
                accessory.Method.TextInfo("Low order (bottom/left)", 10000);
                accessory.Method.TTS("Low order (bottom/left)");
            }
        }
        [ScriptMethod(name: "P1_AlmightyLord_SingleTargetHitBroadcast", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31502"])]
        public async void P1_AlmightyLord_SingleTargetHitBroadcast(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid == accessory.Data.Me)
            {
                accessory.Method.TextInfo("Turn around", 2000);
                accessory.Method.TTS("Turn around");
            }
        }
        [ScriptMethod(name: "P1_AlmightyLord_StackRange", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(350[789]|3510)$"])]
        public void P1_AlmightyLord_StackRange(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;

            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_AlmightyLord_StackRange";
            dp.Scale = new(6,30);
            dp.Owner = P1_BossId;
            dp.TargetObject = tid;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = dur - 3000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P1_AlmightyLord_FarthestDistanceCleave", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"])]
        public void P1_AlmightyLord_FarthestDistanceCleave(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            lock (this)
            {
                if ((DateTime.Now - P1_FanTime).TotalSeconds < 20) return;
                P1_FanTime = DateTime.Now;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_AlmightyLord_FarthestDistanceCleave1";
            dp.Scale = new(20);
            dp.Radian = float.Pi / 3 * 2;
            dp.Owner = P1_BossId;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            dp.TargetOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 13000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_AlmightyLord_FarthestDistanceCleave2";
            dp.Scale = new(20);
            dp.Radian = float.Pi / 3 * 2;
            dp.Owner = P1_BossId;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            dp.TargetOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 13000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        [ScriptMethod(name: "P1_AlmightyLord_TargetLine", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"])]
        public void P1_AlmightyLord_TargetLine(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_AlmightyLord_TargetLine";
            dp.Scale = new(6,50);
            dp.TargetObject = tid;
            dp.Owner = P1_BossId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P1_AlmightyLord_TCleavePosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32368"])]
        public void P1_AlmightyLord_TCleavePosition(Event @event, ScriptAccessory accessory)
        {
            lock (this)
            {
                P1_FireCount++;
                if (P1_FireCount != 26) return;
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                if (myindex != 0 && myindex != 1) return;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_AlmightyLord_TCleavePosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(100,0,86);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 11000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        #endregion

        #region P2
        [ScriptMethod(name: "P2_CoordinationProgramPT_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31550"], userControl: false)]
        public void P2_CoordinationProgramPT_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 2.1;
            P2_Stack = [];
        }
        [ScriptMethod(name: "P2_CoordinationProgramPT_BuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3427|3428)$"], userControl: false)]
        public void P2_CoordinationProgramPT_BuffRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            P2_PTBuffIsFar = @event["StatusID"] == "3428";
        }
        [ScriptMethod(name: "P2_CoordinationProgramPT_SonyRecord", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(01A[0123])$"], userControl: false)]
        public void P2_CoordinationProgramPT_SonyRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            lock (P2_Sony)
            {
                P2_Sony[accessory.Data.PartyList.IndexOf(tid)] = @event["Id"] switch
                {
                    "01A0" => 1,
                    "01A1" => 3,
                    "01A2" => 4,
                    "01A3" => 2,
                    _ => 0
                };
            }
        }
        [ScriptMethod(name: "P2_CoordinationProgramPT_MaleFemaleAOE", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7747", "SourceDataId:regex:^(15714|15715)$"])]
        public void P2_CoordinationProgramPT_MaleFemaleAOE(Event @event, ScriptAccessory accessory)
        {
            // 15714 Male
            // 15715 Female
            // Male sword 0 shield 4
            // Female beacon 0 leg blade 4
            if (parse != 2.1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            Vector3 centre = new(100, 0, 100);
            if ((pos - centre).Length() > 12) return;
            var c = accessory.Data.Objects.SearchById(sid) as KodakkuAssist.Data.ICharacter;
            if (c == null) return;
            var transformationID = c!.GetTransformationID();
            if (@event["SourceDataId"] == "15714")
            {
                // Male
                if (transformationID == 0)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_CoordinationProgramPT_MaleCircle";
                    dp.Scale = new(10);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (transformationID == 4)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_CoordinationProgramPT_MaleDonut";
                    dp.Scale = new(40);
                    dp.InnerScale = new(10);
                    dp.Radian = float.Pi * 2;
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                }
            }
            if (@event["SourceDataId"] == "15715")
            {
                if (transformationID == 0)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_CoordinationProgramPT_FemaleCross1";
                    dp.Scale = new(10, 60);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_CoordinationProgramPT_FemaleCross2";
                    dp.Scale = new(10, 60);
                    dp.Rotation = float.Pi / 2;
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                }
                if (transformationID == 4)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_CoordinationProgramPT_FemaleWings1";
                    dp.Scale = new(60, 20);
                    dp.Owner = sid;
                    dp.Rotation = float.Pi / 2;
                    dp.Offset = new(-5, 0, 0);
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_CoordinationProgramPT_FemaleWings2";
                    dp.Scale = new(60, 20);
                    dp.Owner = sid;
                    dp.Rotation = float.Pi / -2;
                    dp.Offset = new(5, 0, 0);
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
            }
        }
        [ScriptMethod(name: "P2_CoordinationProgramPT_EyeLaser", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375AC", "Id:00020001"])]
        public void P2_CoordinationProgramPT_EyeLaser(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var rot = @event["Index"] switch
            {
                "00000001" => 0,
                "00000002" => 1,
                "00000003" => 2,
                "00000004" => 3,
                "00000005" => 4,
                "00000006" => 5,
                "00000007" => 6,
                "00000008" => 7,
                _ => -1
            };
            if (rot == -1) return;
            var pos = RotatePoint(new(100, 0, 80), new(100, 0, 100), float.Pi / 4 * rot);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_CoordinationProgramPT_EyeLaser";
            dp.Scale = new(16,40);
            dp.Position = pos;
            dp.TargetPosition = new(100, 0, 100);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7500;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2_CoordinationProgramPT_FiveCircles", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31521", "TargetIndex:1"])]
        public void P2_CoordinationProgramPT_FiveCircles(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            foreach (var c in accessory.Data.Objects)
            {
                if(c.DataId== 15714 || c.DataId == 15713)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_CoordinationProgramPT_FiveCircles";
                    dp.Scale = new(10);
                    dp.Owner = c.GameObjectId;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 11000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
        }
        [ScriptMethod(name: "P2_CoordinationProgramPT_EyeLaserSonyPosition", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375AC", "Id:00020001"])]
        public async void P2_CoordinationProgramPT_EyeLaserSonyPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dir = @event["Index"] switch
            {
                "00000001" => 0,
                "00000002" => 1,
                "00000003" => 2,
                "00000004" => 3,
                "00000005" => 4,
                "00000006" => 5,
                "00000007" => 6,
                "00000008" => 7,
                _ => -1
            };
            if (dir == -1) return;
            await Task.Delay(3000);
            Vector3 centre = new(100, 0, 100);

            Vector3 middleLeft1Pos = RotatePoint(new(088.5f, 0, 085.5f), centre, float.Pi / 4 * dir);
            Vector3 middleRight1Pos = RotatePoint(new(111.5f, 0, 085.5f), centre, float.Pi / 4 * dir);
            Vector3 middleLeft2Pos = RotatePoint(new(088.5f, 0, 095.0f), centre, float.Pi / 4 * dir);
            Vector3 middleRight2Pos = RotatePoint(new(111.5f, 0, 095.0f), centre, float.Pi / 4 * dir);
            Vector3 middleLeft3Pos = RotatePoint(new(088.5f, 0, 105.0f), centre, float.Pi / 4 * dir);
            Vector3 middleRight3Pos = RotatePoint(new(111.5f, 0, 105.0f), centre, float.Pi / 4 * dir);
            Vector3 middleLeft4Pos = RotatePoint(new(088.5f, 0, 114.5f), centre, float.Pi / 4 * dir);
            Vector3 middleRight4Pos = RotatePoint(new(111.5f, 0, 114.5f), centre, float.Pi / 4 * dir);

            Vector3 farLeft1Pos = RotatePoint(new(091.5f, 0, 083.0f), centre, float.Pi / 4 * dir);
            Vector3 farRight1Pos = RotatePoint(new(108.5f, 0, 117.0f), centre, float.Pi / 4 * dir);
            Vector3 farLeft2Pos = RotatePoint(new(082.0f, 0, 093.0f), centre, float.Pi / 4 * dir);
            Vector3 farRight2Pos = RotatePoint(new(118.0f, 0, 107.0f), centre, float.Pi / 4 * dir);
            Vector3 farLeft3Pos = RotatePoint(new(082.0f, 0, 107.0f), centre, float.Pi / 4 * dir);
            Vector3 farRight3Pos = RotatePoint(new(118.0f, 0, 093.0f), centre, float.Pi / 4 * dir);
            Vector3 farLeft4Pos = RotatePoint(new(091.5f, 0, 117.0f), centre, float.Pi / 4 * dir);
            Vector3 farRight4Pos = RotatePoint(new(108.5f, 0, 083.0f), centre, float.Pi / 4 * dir);

            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var mySony = P2_Sony[myindex];
            var myPartnerIndex = P2_Sony.IndexOf(mySony) == myindex ? P2_Sony.LastIndexOf(mySony) : P2_Sony.IndexOf(mySony);
            var meIsHigh = HtdhParty.IndexOf(myindex) < HtdhParty.IndexOf(myPartnerIndex);
            Vector3 dealpos = mySony switch
            {
                1 => P2_PTBuffIsFar ? (meIsHigh ? farLeft1Pos : farRight1Pos) : (meIsHigh ? middleLeft1Pos : middleRight1Pos),
                2 => P2_PTBuffIsFar ? (meIsHigh ? farLeft2Pos : farRight2Pos) : (meIsHigh ? middleLeft2Pos : middleRight2Pos),
                3 => P2_PTBuffIsFar ? (meIsHigh ? farLeft3Pos : farRight3Pos) : (meIsHigh ? middleLeft3Pos : middleRight3Pos),
                4 => P2_PTBuffIsFar ? (meIsHigh ? farLeft4Pos : farRight4Pos) : (meIsHigh ? middleLeft4Pos : middleRight4Pos),
                _ => default
            };
            if (dealpos == default) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_CoordinationProgramPT_EyeLaserSonyPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P2_CoordinationProgramPT_StackPosition", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0064"])]
        public void P2_CoordinationProgramPT_StackPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            lock (P2_Stack)
            {
                P2_Stack.Add(accessory.Data.PartyList.IndexOf(tid));
                if (P2_Stack.Count != 2) return;
            }

            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            List<int> leftGroup = [];
            leftGroup.Add(HtdhParty.IndexOf(P2_Sony.IndexOf(1)) < HtdhParty.IndexOf(P2_Sony.LastIndexOf(1)) ? P2_Sony.IndexOf(1) : P2_Sony.LastIndexOf(1));
            leftGroup.Add(HtdhParty.IndexOf(P2_Sony.IndexOf(2)) < HtdhParty.IndexOf(P2_Sony.LastIndexOf(2)) ? P2_Sony.IndexOf(2) : P2_Sony.LastIndexOf(2));
            leftGroup.Add(HtdhParty.IndexOf(P2_Sony.IndexOf(3)) < HtdhParty.IndexOf(P2_Sony.LastIndexOf(3)) ? P2_Sony.IndexOf(3) : P2_Sony.LastIndexOf(3));
            leftGroup.Add(HtdhParty.IndexOf(P2_Sony.IndexOf(4)) < HtdhParty.IndexOf(P2_Sony.LastIndexOf(4)) ? P2_Sony.IndexOf(4) : P2_Sony.LastIndexOf(4));

            // Two stacks on the left
            if (leftGroup.Contains(P2_Stack[0]) && leftGroup.Contains(P2_Stack[1]))
            {
                var lowStackIndex = P2_Sony[P2_Stack[0]] < P2_Sony[P2_Stack[1]] ? P2_Stack[1] : P2_Stack[0];
                var lowStackSony = P2_Sony[lowStackIndex];
                var lowStackPartnerIndex = P2_Sony.IndexOf(lowStackSony) == lowStackIndex ? P2_Sony.LastIndexOf(lowStackSony) : P2_Sony.IndexOf(lowStackSony);
                leftGroup.Remove(lowStackIndex);
                leftGroup.Add(lowStackPartnerIndex);
            }
            // Two stacks on the right
            if (!leftGroup.Contains(P2_Stack[0]) && !leftGroup.Contains(P2_Stack[1]))
            {
                if (P2_PTBuffIsFar)
                {
                    var lowStackIndex = P2_Sony[P2_Stack[0]] < P2_Sony[P2_Stack[1]] ? P2_Stack[0] : P2_Stack[1];
                    var lowStackSony = P2_Sony[lowStackIndex];
                    var lowStackPartnerIndex = P2_Sony.IndexOf(lowStackSony) == lowStackIndex ? P2_Sony.LastIndexOf(lowStackSony) : P2_Sony.IndexOf(lowStackSony);
                    leftGroup.Remove(lowStackPartnerIndex);
                    leftGroup.Add(lowStackIndex);
                }
                else
                {
                    var lowStackIndex = P2_Sony[P2_Stack[0]] < P2_Sony[P2_Stack[1]] ? P2_Stack[1] : P2_Stack[0];
                    var lowStackSony = P2_Sony[lowStackIndex];
                    var lowStackPartnerIndex = P2_Sony.IndexOf(lowStackSony) == lowStackIndex ? P2_Sony.LastIndexOf(lowStackSony) : P2_Sony.IndexOf(lowStackSony);
                    leftGroup.Remove(lowStackPartnerIndex);
                    leftGroup.Add(lowStackIndex);
                }
            }

            Vector3 dealpos = default;
            if (P2_PTBuffIsFar)
            {
                dealpos = leftGroup.Contains(myindex) ? new(94, 0, 100) : new(106, 0, 100);
            }
            else
            {
                dealpos = leftGroup.Contains(myindex) ? new(97, 0, 100) : new(100, 0, 103);
            }
            var c = accessory.Data.Objects.Where(o => o.DataId == 15713).FirstOrDefault();
            if (c == null) return;
            var dir8 = RoundPositionTo8Dir(c!.Position, new(100, 0, 100));
            //accessory.Log.Debug($"P2_CoordinationProgramPT {dir8} {leftGroup.Contains(myindex)}");
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_CoordinationProgramPT_StackPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = RotatePoint(dealpos, new(100, 0, 100), float.Pi / 4 * dir8);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "P2_CoordinationProgramLB_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31544"], userControl: false)]
        public void P2_CoordinationProgramLB_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 2.2;
            P2_LineTether = [];
        }
        [ScriptMethod(name: "P2_CoordinationProgramLB_ArcherSacredSword", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31539"])]
        public void P2_CoordinationProgramLB_ArcherSacredSword(Event @event, ScriptAccessory accessory)
        {
            if(parse != 2.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_CoordinationProgramLB_ArcherSacredSword";
            dp.Scale = new(10,42);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 500;
            dp.DestoryAt = 7500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2_CoordinationProgramLB_LineDance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3154[01])$"])]
        public void P2_CoordinationProgramLB_LineDance(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P2_CoordinationProgramLB_LineDance-{sid}";
            dp.Scale = new(40);
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.TetherTarget;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 8200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        //[ScriptMethod(name: "P2_CoordinationProgramLB_LineDanceRemove", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31539"])]
        //public void P2_CoordinationProgramLB_LineDanceRemove(Event @event, ScriptAccessory accessory)
        //{
        //    if (parse != 2.2) return;
        //    foreach (var item in P2_LineTether)
        //    {
        //        accessory.Method.RemoveDraw($"P2_CoordinationProgramLB_LineDance-{item.Key}-{item.Value}");
        //    }
        //}
        [ScriptMethod(name: "P2_CoordinationProgramLB_ShieldComboS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31527"])]
        public void P2_CoordinationProgramLB_ShieldComboS(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P2_CoordinationProgramLB_ShieldComboS-1-1";
            dp.Scale = new(5);
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P2_CoordinationProgramLB_ShieldComboS-1-2";
            dp.Scale = new(5);
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P2_CoordinationProgramLB_ShieldComboS-2";
            dp.Scale = new(6);
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 1;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 5200;
            dp.DestoryAt = 2800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P2_CoordinationProgramLB_ShieldComboSHitAlert", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31528"])]
        public void P2_CoordinationProgramLB_ShieldComboSHitAlert(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (accessory.Data.Me != tid) return;
            accessory.Method.TextInfo("Move out",3000);
            accessory.Method.TTS("Move out");
        }

        [ScriptMethod(name: "P2_CoordinationProgramLB_ArcherSacredSwordGuidePosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31544"])]
        public void P2_CoordinationProgramLB_ArcherSacredSwordGuidePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_CoordinationProgramLB_ArcherSacredSwordGuidePosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = new(100,0,94.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P2_CoordinationProgramLB_ShieldComboS_MalePositionTether", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32369"])]
        public void P2_CoordinationProgramLB_ShieldComboS_MalePositionTether(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myindex != 0 && myindex != 1) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_CoordinationProgramLB_ShieldComboS_MalePositionTether";
            dp.Scale = new(5);
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = sid;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 8000;
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
        }
        [ScriptMethod(name: "P2_CoordinationProgramLB_ShieldComboSSecondPosition", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31528"])]
        public void P2_CoordinationProgramLB_ShieldComboSSecondPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            Vector3 dealpos = new(100, 0, 100);
            if (accessory.Data.Me == tid)
            {
                var pos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
                dealpos = RotatePoint(pos, new(100, 0, 100), float.Pi/2);
            }
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_CoordinationProgramLB_ShieldComboSSecondPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 2800;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        #endregion

        #region P3

        [ScriptMethod(name: "P3_Intro_PhaseSplit", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31507"], userControl: false)]
        public void P3_Intro_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 3.0;
            P3_ArmCount = 0;
            P3_StartBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            P3_StartDone = false;
            P3_StartPreDone = false;
            P3_TVBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        }
        [ScriptMethod(name: "P3_Intro_BuffCollection", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3425|3426)$"], userControl: false)]
        public void P3_Intro_BuffCollection(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            if (index == -1) return;
            lock (P3_StartBuff)
            {
                //1 Spread 2 Stack
                P3_StartBuff[index] = @event["StatusID"] == "3425" ? 1 : 2;
            }
        }
        [ScriptMethod(name: "P3_TV_BuffCollection", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3452|3453)$"], userControl: false)]
        public void P3_TV_BuffCollection(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            if (index == -1) return;
            lock (P3_TVBuff)
            {
                P3_TVBuff[index] = @event["StatusID"] == "3452" ? 1 : 2;
            }
        }
        [ScriptMethod(name: "P3_Intro_ArmAOE", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:regex:^(774[78])$", "SourceDataId:regex:^(1571[89])$"], userControl: false)]
        public void P3_Intro_ArmAOE(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            lock (this)
            {
                P3_ArmCount++;
                if (!ParseObjectId(@event["SourceId"], out var sid)) return;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Intro_ArmAOE";
                dp.Scale = new(11);
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = P3_ArmCount > 3 ? 11000 : 0;
                dp.DestoryAt = P3_ArmCount > 3 ? 2500 : 14000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }
        [ScriptMethod(name: "P3_Intro_Earthquake", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31567"])]
        public void P3_Intro_Earthquake(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Intro_Earthquake_1";
            dp.Scale = new(6);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 0;
            dp.DestoryAt = 4800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Intro_Earthquake_2";
            dp.InnerScale = new(6);
            dp.Scale = new(12);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 4800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Intro_Earthquake_3";
            dp.InnerScale = new(12);
            dp.Scale = new(18);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Intro_Earthquake_4";
            dp.InnerScale = new(18);
            dp.Scale = new(24);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 8800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        [ScriptMethod(name: "P3_TV_SelfAOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3159[56])$"])]
        public void P3_TV_SelfAOE(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_TV_SelfAOE";
            dp.Scale = new(7);
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P3_Intro_BuffPrePosition", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:3426"])]
        public async void P3_Intro_BuffPrePosition(Event @event, ScriptAccessory accessory)
        {
            lock (this)
            {
                if(P3_StartPreDone) return;
                P3_StartPreDone = true;
            }
            await Task.Delay(100);
            List<int> sortOrder = P3_StackSort switch
            {
                P3SortEnum.HTDH => HtdhParty,
                P3SortEnum.THD => [0, 1, 2, 3, 4, 5, 6, 7],
                _ => [0, 1, 2, 3, 4, 5, 6, 7],
            };
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            //1 Spread 2 Stack
            var myP3StartBuff = P3_StartBuff[myindex];
            var myP3Index = 0;
            for (int i = 0; i < sortOrder.Count; i++)
            {

                var index = sortOrder[i];
                if (myP3StartBuff == P3_StartBuff[index]) myP3Index++;
                //accessory.Log.Debug($"{myindex} {index} {myP3StartBuff} {P3_StartBuff[index]} {myP3Index}");
                if (index == myindex) break;
            }
            Vector3 dealpos = default;
            if (myP3StartBuff == 2 || myP3StartBuff == 0)
            {
                dealpos = myP3Index switch
                {
                    1 => new(092.00f, 0, 086.14f),
                    2 => new(108.00f, 0, 086.14f),
                    _ => default,
                };
            }
            if (myP3StartBuff == 1)
            {
                dealpos = myP3Index switch
                {
                    1 => new(084.00f, 0, 100.00f),
                    2 => new(092.00f, 0, 113.86f),
                    3 => new(108.00f, 0, 113.86f),
                    4 => new(116.00f, 0, 100.00f),
                    _ => default,
                };
            }
            if (dealpos == default) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Intro_BuffPrePosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 3100;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P3_Intro_Position", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:regex:^(774[78])$", "SourceDataId:regex:^(1571[89])$"])]
        public void P3_Intro_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if (MathF.Abs(pos.X - 100) > 1) return;
            if (P3_StartDone) return;
            P3_StartDone = true;

            var northCirle = pos.Z < 100;

            List<int> sortOrder = P3_StackSort switch
            {
                P3SortEnum.HTDH => HtdhParty,
                P3SortEnum.THD => [0, 1, 2, 3, 4, 5, 6, 7],
                _ => [0, 1, 2, 3, 4, 5, 6, 7],
            };
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            //1 Spread 2 Stack
            var myP3StartBuff = P3_StartBuff[myindex];
            var myP3Index = 0;
            for (int i = 0; i < sortOrder.Count; i++)
            {
                var index = sortOrder[i];
                if (myP3StartBuff == P3_StartBuff[index]) myP3Index++;
                //accessory.Log.Debug($"{myindex} {index} {myP3StartBuff} {P3_StartBuff[index]} {myP3Index}");
                if (index == myindex) break;
            }

            Vector3 dealpos1 = default;
            Vector3 dealpos2 = default;
            Vector3 dealpos3 = default;
            Vector3 dealpos4 = default;
            if (myP3StartBuff == 2 || myP3StartBuff == 0)
            {
                dealpos1 = myP3Index switch
                {
                    1 => northCirle ? new(086.7f, 0, 086.7f) : new(094.8f, 0, 082.0f),
                    2 => northCirle ? new(113.3f, 0, 086.7f) : new(105.2f, 0, 082.0f),
                    _ => default,
                };
                dealpos2 = myP3Index switch
                {
                    1 => northCirle ? new(087.8f, 0, 087.8f) : new(095.0f, 0, 083.5f),
                    2 => northCirle ? new(112.2f, 0, 087.8f) : new(105.0f, 0, 083.5f),
                    _ => default,
                };
                dealpos3 = myP3Index switch
                {
                    1 => northCirle ? new(088.4f, 0, 085.5f) : new(093.1f, 0, 082.8f),
                    2 => northCirle ? new(111.6f, 0, 085.5f) : new(106.9f, 0, 082.8f),
                    _ => default,
                };
                dealpos4 = myP3Index switch
                {
                    1 => northCirle ? new(094.7f, 0, 083.7f) : new(088.5f, 0, 087.0f),
                    2 => northCirle ? new(105.3f, 0, 083.7f) : new(111.5f, 0, 087.0f),
                    _ => default,
                };
            }
            if (myP3StartBuff == 1)
            {
                dealpos1 = myP3Index switch
                {
                    1 => northCirle ? new(082.0f, 0, 095.0f) : new(082.0f, 0, 104.7f),
                    2 => northCirle ? new(095.0f, 0, 118.0f) : new(086.5f, 0, 113.0f),
                    3 => northCirle ? new(105.0f, 0, 118.0f) : new(113.5f, 0, 113.0f),
                    4 => northCirle ? new(118.0f, 0, 095.0f) : new(118.0f, 0, 104.7f),
                    _ => default,
                };
                dealpos2 = myP3Index switch
                {
                    1 => northCirle ? new(083.5f, 0, 095.5f) : new(083.5f, 0, 104.5f),
                    2 => northCirle ? new(095.0f, 0, 116.5f) : new(088.0f, 0, 112.0f),
                    3 => northCirle ? new(105.0f, 0, 116.5f) : new(112.0f, 0, 112.0f),
                    4 => northCirle ? new(116.5f, 0, 095.5f) : new(116.5f, 0, 104.5f),
                    _ => default,
                };
                dealpos3 = myP3Index switch
                {
                    1 => northCirle ? new(081.7f, 0, 097.2f) : new(081.6f, 0, 102.8f),
                    2 => northCirle ? new(093.2f, 0, 117.2f) : new(088.5f, 0, 114.5f),
                    3 => northCirle ? new(106.8f, 0, 117.2f) : new(111.5f, 0, 114.5f),
                    4 => northCirle ? new(118.3f, 0, 097.2f) : new(118.4f, 0, 102.8f),
                    _ => default,
                };
                dealpos4 = myP3Index switch
                {
                    1 => northCirle ? new(083.5f, 0, 104.0f) : new(084.0f, 0, 095.0f),
                    2 => northCirle ? new(088.5f, 0, 112.5f) : new(095.0f, 0, 116.3f),
                    3 => northCirle ? new(111.5f, 0, 112.5f) : new(105.0f, 0, 116.3f),
                    4 => northCirle ? new(116.5f, 0, 104.0f) : new(116.0f, 0, 095.0f),
                    _ => default,
                };
            }

            if (dealpos1 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Intro_Position_Pre";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos1;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (dealpos1 != default && dealpos2 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Intro_Position_1-2";
                dp.Scale = new(2);
                dp.Position = dealpos1;
                dp.TargetPosition = dealpos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Intro_Position_2";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 6000;
                dp.DestoryAt = 2000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (dealpos2 != default && dealpos3 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Intro_Position_2-3";
                dp.Scale = new(2);
                dp.Position = dealpos2;
                dp.TargetPosition = dealpos3;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Intro_Position_3";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos3;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 8000;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            if (dealpos3 != default && dealpos4 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Intro_Position_3-4";
                dp.Scale = new(2);
                dp.Position = dealpos3;
                dp.TargetPosition = dealpos4;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 14000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Intro_Position_4";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos4;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 14000;
                dp.DestoryAt = 2000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        [ScriptMethod(name: "P3_TV_Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3159[56])$"])]
        public void P3_TV_Position(Event @event, ScriptAccessory accessory)
        {
            //31595 East
            //31595 West
            if (parse != 3.0) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var meIsIdle = P3_TVBuff[myindex]==0;
            var myBuffIndex = 0;
            var isEast = @event["ActionId"] == "31595";
            Vector3 dealpos = default;

            for (int i = 0; i < HtdhParty.Count; i++)
            {
                var index = HtdhParty[i];
                var isIdle = P3_TVBuff[index] == 0;
                if (meIsIdle == isIdle) myBuffIndex++;
                if (index == myindex) break;
            }
            if (P3_TV_Strategy==P3TVEnum.Normal)
            {
                if (meIsIdle)
                {
                    dealpos = myBuffIndex switch
                    {
                        1 => isEast ? new(099.0f, 0, 091.0f) : new(101.0f, 0, 091.0f),
                        2 => isEast ? new(104.0f, 0, 100.0f) : new(096.0f, 0, 100.0f),
                        3 => isEast ? new(115.5f, 0, 100.0f) : new(084.5f, 0, 100.0f),
                        4 => isEast ? new(099.0f, 0, 109.0f) : new(101.0f, 0, 109.0f),
                        5 => isEast ? new(099.0f, 0, 119.0f) : new(101.0f, 0, 119.0f),
                        _ => default
                    };
                }
                else
                {
                    dealpos = myBuffIndex switch
                    {
                        1 => isEast ? new(093.0f, 0, 082.0f) : new(107.0f, 0, 082.0f),
                        2 => isEast ? new(086.0f, 0, 092.5f) : new(114.0f, 0, 092.5f),
                        3 => isEast ? new(086.0f, 0, 107.5f) : new(114.0f, 0, 107.5f),
                        _ => default
                    } ;
                }
            }
            if (P3_TV_Strategy == P3TVEnum.Static)
            {
                if (meIsIdle)
                {
                    dealpos = myBuffIndex switch
                    {
                        1 => isEast ? new(099.0f, 0, 091.0f) : new(101.0f, 0, 091.0f),
                        2 => new(109.0f, 0, 100.0f),
                        3 => new(119.0f, 0, 100.0f),
                        4 => isEast ? new(099.0f, 0, 109.0f) : new(101.0f, 0, 109.0f),
                        5 => isEast ? new(099.0f, 0, 119.0f) : new(101.0f, 0, 119.0f),
                        _ => default
                    };
                }
                else
                {
                    dealpos = myBuffIndex switch
                    {
                        1 => isEast ? new(095.0f, 0, 082.0f) : new(105.0f, 0, 082.0f),
                        2 => new(086.0f, 0, 092.0f),
                        3 => new(086.0f, 0, 108.0f),
                        _ => default
                    };
                }
            }

            if (dealpos == default) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_TV_Position";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P3_TV_FacingAssist", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3159[56])$"])]
        public void P3_TV_FacingAssist(Event @event, ScriptAccessory accessory)
        {
            //31595 East
            //31595 West
            if (parse != 3.0) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var meIsIdle = P3_TVBuff[myindex] == 0;
            if (meIsIdle) return;
            var meLeft = P3_TVBuff[myindex] == 2;
            var myBuffIndex = 0;
            var isEast = @event["ActionId"] == "31595";
            float? seeRot = null;

            for (int i = 0; i < HtdhParty.Count; i++)
            {
                var index = HtdhParty[i];
                var isIdle = P3_TVBuff[index] == 0;
                if (meIsIdle == isIdle) myBuffIndex++;
                if (index == myindex) break;
            }
            //b pi/2

            seeRot = myBuffIndex switch
            {
                1 => isEast ? (meLeft ? float.Pi : 0) : (meLeft ? 0 : float.Pi),
                2 => meLeft ? float.Pi / 2 : float.Pi / -2,
                3 => meLeft ? float.Pi / -2 : float.Pi / 2,
                _ => null
            };
            if (seeRot == null) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_TV_FacingAssist_Self1";
            dp.Scale = new(5, 5);
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_TV_FacingAssist_Self2";
            dp.Scale = new(5, 1.5f);
            dp.Offset = new(0, 0, -5);
            dp.Rotation = float.Pi / 6 * 5;
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_TV_FacingAssist_Self3";
            dp.Scale = new(5, 1.5f);
            dp.Offset = new(0, 0, -5);
            dp.Rotation = float.Pi / 6 * -5;
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_TV_FacingAssist_Direction1";
            dp.Scale = new(10,4);
            dp.FixRotation = true;
            dp.Rotation = seeRot.Value;
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
        }
        #endregion

        #region P4
        [ScriptMethod(name: "P4_PhaseSplit", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31559"], userControl: false)]
        public void P4_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 4.0;
            P4Stack = [];
        }
        [ScriptMethod(name: "P4_StackMarkRecord", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:22393"], userControl:false)]
        public void P4_StackMarkRecord(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            if (index == -1) return;
            lock (P4Stack)
            {
                P4Stack.Add(index);
            }
        }
        [ScriptMethod(name: "P4_Earthquake", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31567"])]
        public void P4_Earthquake(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Earthquake_1";
            dp.Scale = new(6);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 0;
            dp.DestoryAt = 4800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Earthquake_2";
            dp.InnerScale = new(6);
            dp.Scale = new(12);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 4800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Earthquake_3";
            dp.InnerScale = new(12);
            dp.Scale = new(18);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Earthquake_4";
            dp.InnerScale = new(18);
            dp.Scale = new(24);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 8800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        [ScriptMethod(name: "P4_FirstWaveBlastHitAlert", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31614", "TargetIndex:1"])]
        public void P4_FirstWaveBlastHitAlert(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            accessory.Method.TextInfo("Move", 2000, true);
            accessory.Method.TTS("Move");
        }
        [ScriptMethod(name: "P4_SecondWaveBlast", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31616"])]
        public void P4_SecondWaveBlast(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_SecondOctaWaveBlast";
            dp.Scale = new(6,50);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 0;
            dp.DestoryAt = 4800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "P4_FirstWaveBlastGuidePosition", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3161[07])$"])]
        public void P4_FirstWaveBlastGuidePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 dealpos = myindex switch
            {
                0 => new(087.5f, 0, 094.5f),
                6 => new(086.5f, 0, 100.0f),
                2 => new(087.5f, 0, 105.0f),
                4 => new(090.5f, 0, 109.5f),
                1 => new(112.5f, 0, 094.5f),
                7 => new(113.5f, 0, 100.0f),
                3 => new(112.5f, 0, 105.0f),
                5 => new(109.5f, 0, 109.5f),
                _ => default
            };
            if (dealpos == default) return;

            if (@event["ActionId"] == "31610")
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_FirstWaveBlastGuidePosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 14000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_FirstWaveBlastGuidePosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 5500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_FirstWaveBlastGuidePosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 15500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        [ScriptMethod(name: "P4_SecondWaveBlastStackPosition", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31614", "TargetIndex:1"])]
        public void P4_SecondWaveBlastStackPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;

            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            var stack1 = P4Stack[^1];
            var stack2 = P4Stack[^2];

            List<int> leftGroup = [0, 6, 2, 4];
            List<int> rightGroup = [1, 7, 3, 5];
            if (leftGroup.Contains(stack1) && leftGroup.Contains(stack2))
            {
                var change = leftGroup.IndexOf(stack1) < leftGroup.IndexOf(stack2) ? stack2 : stack1;
                leftGroup.Remove(change);
                leftGroup.Add(5);
                rightGroup.Remove(5);
                rightGroup.Add(change);
            }
            if (rightGroup.Contains(stack1) && rightGroup.Contains(stack2))
            {
                var change = rightGroup.IndexOf(stack1) < rightGroup.IndexOf(stack2) ? stack2 : stack1;
                rightGroup.Remove(change);
                rightGroup.Add(4);
                leftGroup.Remove(4);
                leftGroup.Add(change);
            }

            Vector3 dealpos = leftGroup.Contains(myindex) ? new(96.5f, 0, 113) : new(103.5f, 0, 113);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_SecondWaveBlastStackPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        #endregion

        #region P5
        [ScriptMethod(name: "P5_Intro_PhaseSplit", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31621"], userControl: false)]
        public void P5_Intro_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 5.0;
        }
        [ScriptMethod(name: "P5_FirstMechanic_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31624"], userControl: false)]
        public void P5_FirstMechanic_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 5.1;
            P51_Eye = 0;
            P51_Buff = [0, 0, 0, 0, 0, 0, 0, 0];
            P51_Fist = [0, 0, 0, 0];
        }
        [ScriptMethod(name: "P5_FirstMechanic_EyeRecord", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00020001"], userControl: false)]
        public void P5_FirstMechanic_EyeRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            P51_Eye = @event["Index"] switch
            {
                "00000001" => 0,
                "00000002" => 1,
                "00000003" => 2,
                "00000004" => 3,
                "00000005" => 4,
                "00000006" => 5,
                "00000007" => 6,
                "00000008" => 7,
                _ => 0

            };
        }
        [ScriptMethod(name: "P5_FirstMechanic_BuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3442|3443|3440|3504)$"], userControl: false)]
        public void P5_FirstMechanic_BuffRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            lock (P51_Buff)
            {
                P51_Buff[index] += @event["StatusID"] switch
                {
                    //3440 10 Green Tether
                    //3504 20 Blue Tether
                    //3442 100 Close World
                    //3443 200 Far World
                    "3440" => 10,
                    "3504" => 20,
                    "3442" => 100,
                    "3443" => 200,
                    _ => 0
                };
            }
        }
        [ScriptMethod(name: "P5_FirstMechanic_TetherRecord", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(00C9|00C8)$"], userControl: false)]
        public void P5_FirstMechanic_TetherRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var tindex = accessory.Data.PartyList.IndexOf(tid);
            var sindex = accessory.Data.PartyList.IndexOf(sid);
            lock (P51_Buff)
            {
                P51_Buff[tindex] += sindex;
                P51_Buff[sindex] += tindex;
            }
        }
        [ScriptMethod(name: "P5_FirstMechanic_FistRecord", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(15709|15710)$"], userControl: false)]
        public void P5_FirstMechanic_FistRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var rot = FloorPositionTo4Dir(pos, new(100, 0, 100));
            lock (P51_Fist)
            {
                P51_Fist[rot] += @event["DataId"] == "15709" ? 1 : 10;
            }
        }
        [ScriptMethod(name: "P5_FirstMechanic_TetherPrePosition", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00020001"])]
        public async void P5_FirstMechanic_TetherPrePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            await Task.Delay(100);

            var StartList = GetP51StartList();

            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 dealpos = StartList.IndexOf(myindex) switch
            {
                0 => new Vector3(088.0f, 0, 088.0f),
                1 => new Vector3(088.0f, 0, 112.0f),
                2 => new Vector3(094.0f, 0, 088.0f),
                3 => new Vector3(094.0f, 0, 112.0f),
                4 => new Vector3(113.0f, 0, 098.5f),
                5 => new Vector3(113.0f, 0, 101.5f),
                6 => new Vector3(109.0f, 0, 092.0f),
                7 => new Vector3(109.0f, 0, 108.0f),
                _ => default
            };

            if (dealpos == default) return;

            dealpos = RotatePoint(dealpos, new(100, 0, 100), float.Pi / 4 * P51_Eye);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_FirstMechanic_TetherPrePosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 8500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P5_FirstMechanic_FistPosition", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(15709)$"])]
        public async void P5_FirstMechanic_FistPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            lock (this)
            {
                if (P51_FistDone) return;
                P51_FistDone = true;
            }
            await Task.Delay(50);

            var eye4dir = P51_Eye / 2;

            var StartList = GetP51StartList();
        }
        private List<int> GetP51StartList()
        {
            List<int> rst = [];
            List<int> d3htdhList = [6, 2, 0, 1, 4, 5, 7, 3];

            //3440 10 Green Tether
            //3504 20 Blue Tether
            //3442 100 Close World
            //3443 200 Far World

            // Green Tether Group
            List<int> greenTetherGroup = [];
            for (int i = 0; i < 8; i++)
            {
                var index = d3htdhList[i];
                var b = P51_Buff[index] % 100;
                if (b / 10 == 1 && !greenTetherGroup.Contains(index))
                {
                    greenTetherGroup.Add(index);
                    greenTetherGroup.Add(b % 10);
                }
            }

            // Blue Tether Group
            List<int> blueTetherGroup = [];
            //for (int i = 0; i < 8; i++)
            //{
            //    if (P51_Buff[i] /100==2)
            //    {
            //        blueTetherGroup.Add(i);
            //        blueTetherGroup.Add(P51_Buff[i] % 10);
            //    }
            //}
            for (int i = 0; i < 8; i++)
            {
                var index = d3htdhList[i];
                var b = P51_Buff[index] % 100;
                if (b / 10 == 2 && !blueTetherGroup.Contains(index))
                {
                    blueTetherGroup.Add(index);
                    blueTetherGroup.Add(b % 10);
                }
            }

            rst.AddRange(greenTetherGroup);
            rst.AddRange(blueTetherGroup);
            return rst;
        }
        #endregion

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
        private int RoundPositionTo4Dir(Vector3 point, Vector3 centre)
        {
            var r = Math.Round(2 - 2 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 4;
            return (int)r;
        }
        private int RoundPositionTo8Dir(Vector3 point, Vector3 centre)
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r = Math.Round(4 - 4 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 8;
            return (int)r;

        }
        private int FloorPositionTo4Dir(Vector3 point, Vector3 centre)
        {
            var r = Math.Floor(2 - 2 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 4;
            return (int)r;
        }
        private int FloorPositionTo8Dir(Vector3 point, Vector3 centre)
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r = Math.Floor(4 - 4 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 8;
            return (int)r;

        }
        private Vector3 RotatePoint(Vector3 point, Vector3 centre, float radian)
        {

            Vector2 v2 = new(point.X - centre.X, point.Z - centre.Z);

            var rot = (MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian);
            var lenth = v2.Length();
            return new(centre.X + MathF.Sin(rot) * lenth, centre.Y, centre.Z - MathF.Cos(rot) * lenth);
        }
    }
}
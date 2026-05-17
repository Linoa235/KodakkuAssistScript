// File: EdenUltimate_Unknown.cs
using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;
using System.Xml.Linq;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Extensions;

namespace MyScriptNamespace
{
    
    [ScriptType(name: "EdenUltimate", territorys: [1238],guid: "a4e14eff-0aea-a4b6-d8c3-47644a3e9e9a", version:"0.0.0.11",note: noteStr)]
    public class EdenUltimate
    {
        const string noteStr =
        """
        Eden Ultimate Preview Version P4 Complete
        P3 floor fire positioning and water stack missing
        P4 white circle indication missing
        """;

        [UserSetting("P1_Spinning Wheel Grouping Basis")]
        public P1BrightFireEnum P1BrightFireGroup { get; set; }
        [UserSetting("P1_Four-Tether Head Mark")]
        public P1TetherEnum p1Thther4Type { get; set; }
        [UserSetting("P1_Four-Tether Head Mark")]
        public bool p1Thther4Marker { get; set; } = false;

        [UserSetting("P2_Light Rampant Tether Method")]
        public P2LightRampantTetherEmum P2LightRampantTetherDeal { get; set; }
        [UserSetting("P2_Light Rampant Octa Position Method")]
        public P2LightRampant8DirEmum P2LightRampant8DirSet { get; set; }

        [UserSetting("P3_Lamp Method")]
        public P3LampEmum P3LampDeal { get; set; }
        [UserSetting("P3_T Jump Guide Position")]
        public bool P3JumpPosition { get; set; } = false;

        [UserSetting("P4_Second Mechanic Normal/Slow Lamp AOE Display Time (ms)")]
        public uint P4LampDisplayDur { get; set; } =3000;
        [UserSetting("P4_White Circle Indicator Line Color")]
        public ScriptColor P4WhiteCircleLineColor { get; set; } = new();

        [UserSetting("P5_Floor Fire Color")]
        public ScriptColor P5PathColor { get; set; } = new() { V4=new(0,1,1,1)};

        int? firstTargetIcon = null;
        double parse = 0;

        int P1FogDragonCount =0;
        List<int> P1FogDragonRecord = [0, 0, 0, 0];
        bool P1FogDragonThunder = false;

        bool P1SpinningWheelThunder = false;
        List<int> P1SpinningWheelGrab = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P1FourTether = [];
        bool P1FourTetherStart = false;
        List<int> P1Tower = [0, 0, 0, 0];

        bool P2DDCircle = false;
        List<int> P2DDIceDir = [];
        List<int> P2RedMirror = [];
        uint P2BossId = 0;
        List<int> P2LightRampantCircle = [];
        List<int> P2LightRampantBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        bool P2LightRampantTetherDone = new();

        List<int> P3FireBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P3WaterBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P3ReturnBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P3Lamp = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P3LampWise = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P3Stack = [0, 0, 0, 0, 0, 0, 0, 0];
        bool P3FloorFireDone = false;
        int P3FloorFire = 0;

        uint P4FragmentId;
        List<int> P4Tether = [-1, -1, -1, -1, -1, -1, -1, -1];
        List<int> P4Stack = [0, 0, 0, 0, 0, 0, 0, 0];
        bool P4TetherDone = false;
        List<int> P4ClawBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P4OtherBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        int P4BlueTether = 0;
        List<Vector3> P4WhiteCirclePos = [];
        List<Vector3> P4WaterPos = [];

        bool P5MtDone = false;
        string P5Tower = "";

        public enum P1TetherEnum
        {
            OneLine,
            Mgl_TwoLine
        }
        public enum P1BrightFireEnum
        {
            TH_Up,
            MtGroup_Up,
            MtStD3D4_Up
        }
        public enum P2LightRampant8DirEmum
        {
            Normal,
            TN_Up
        }
        public enum P2LightRampantTetherEmum
        {
            CircleNum,
            LTeam,
            AC_Cross,
            NewGrey9
        }

        public enum P3LampEmum
        {
            MGL
        }

        public enum P4WhiteCirleEmum
        {
            IceB,
            Ice3
        }

        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(".*");
            if (p1Thther4Marker)
                accessory.Method.MarkClear();
            parse = 1d;
            P1FogDragonRecord = [0, 0, 0, 0];
            P1FogDragonCount = 0;
            P1SpinningWheelGrab = [0, 0, 0, 0, 0, 0, 0, 0];
            P1FourTether = [];
            P1FourTetherStart = false;
            P1Tower = [0, 0, 0, 0];

            P2DDIceDir.Clear();

            P3FloorFireDone = false;
            P3Stack = [0, 0, 0, 0, 0, 0, 0, 0];
        }

        #region P1

        [ScriptMethod(name: "P1_OctaThunderFire_GuideCone",eventType: EventTypeEnum.StartCasting,eventCondition: ["ActionId:regex:^((4014[48])|40329|40330)$"])]
        public void P1_OctaThunderFire_GuideCone(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            foreach (var pm in accessory.Data.PartyList)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_OctaThunderFire_GuideCone";
                dp.Scale = new(60);
                dp.Radian = float.Pi / 8;
                dp.Owner = sid;
                dp.TargetObject=pm;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }

        }
        [ScriptMethod(name: "P1_OctaThunderFire_SubsequentCone", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(40145)$", "TargetIndex:1"])]
        public void P1_OctaThunderFire_SubsequentCone(Event @event, ScriptAccessory accessory)
        {
            var dur = 2000;
            if (parse != 1d) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!float.TryParse(@event["SourceRotation"], out var rot)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_OctaThunderFire_SubsequentCone1";
            dp.Scale = new(60);
            dp.FixRotation = true;
            dp.Rotation = rot;
            dp.Radian = float.Pi / 8;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_OctaThunderFire_SubsequentCone2";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 8;
            dp.FixRotation = true;
            dp.Rotation = rot + float.Pi / -8;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 2000;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_OctaThunderFire_SubsequentCone3";
            dp.Scale = new(60);
            dp.FixRotation = true;
            dp.Rotation = rot + float.Pi / -4;
            dp.Radian = float.Pi / 8;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 4000;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }
        [ScriptMethod(name: "P1_OctaThunderFire_SpreadStack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4014[48])|40329|40330)$"])]
        public void P1_OctaThunderFire_SpreadStack(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            if (@event["ActionId"]== "40148" || @event["ActionId"] == "40330")
            {
                foreach (var pm in accessory.Data.PartyList)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_OctaThunderFire_Spread";
                    dp.Scale = new(6);
                    dp.Owner = pm;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 5000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
            else
            {
                int[] group = [6, 7, 4, 5, 2, 3, 0, 1];
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                for (int i = 0; i < 4; i++)
                {
                    var ismygroup = myindex == i || group[i] == myindex;

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_OctaThunderFire_Stack";
                    dp.Scale = new(6);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = ismygroup ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.Delay = 5000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
            

        }
        [ScriptMethod(name: "P1_OctaThunderFire_GuidePosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4014[48])|40329|40330)$"])]
        public void P1_OctaThunderFire_GuidePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var spread = @event["ActionId"] == "40148"|| @event["ActionId"] == "40330";
            var rot8 = myindex switch
            {
                0 => 0,
                1 => 2,
                2 => 6,
                3 => 4,
                4 => 5,
                5 => 3,
                6 => 7,
                7 => 1,
                _ => 0,
            };
            var outPoint = spread && (myindex == 2 || myindex == 3 || myindex == 6 || myindex == 7);
            var mPosEnd = RotatePoint(outPoint? new(100, 0, 90) : new(100, 0, 95), new(100, 0, 100), float.Pi / 4 * rot8);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_OctaThunderFire_GuidePosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = mPosEnd;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        }
        [ScriptMethod(name: "P1_TBusterBuffExplosion", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4166"])]
        public void P1_TBusterBuffExplosion(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if(!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            var displayTime = 4000;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_TBusterBuffExplosion1";
            dp.Scale = new(10);
            dp.Owner = tid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = dur- displayTime;
            dp.DestoryAt = displayTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_TBusterBuffExplosion2";
            dp.Scale = new(10);
            dp.Owner = tid;
            dp.CentreResolvePattern=PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = dur - displayTime;
            dp.DestoryAt = displayTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "P1_FogDragon_PositionRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40158)$"], userControl: false)]
        public void P1_FogDragon_PositionRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var obj= accessory.Data.Objects.SearchByEntityId(sid+1);
            if(obj == null) return;
            var dir8= PositionTo8Dir(obj.Position, new(100, 0, 100));
            P1FogDragonRecord[dir8 % 4] = 1;
        }
        [ScriptMethod(name: "P1_FogDragon_ThunderFireRecord", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4015[45])$"], userControl: false)]
        public void P1_FogDragon_ThunderFireRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            P1FogDragonThunder = (@event["ActionId"] == "40155");
        }
        [ScriptMethod(name: "P1_FogDragon_Range", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40158)$"])]
        public void P1_FogDragon_Range(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_FogDragonRange";
            dp.Scale = new(16,50);
            dp.Owner = sid+1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P1_FogDragon_SpreadStack", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4015[45])$"])]
        public void P1_FogDragon_SpreadStack(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;

            if (@event["ActionId"] == "40155")
            {
                foreach (var pm in accessory.Data.PartyList)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FogDragon_Spread";
                    dp.Scale = new(5);
                    dp.Owner = pm;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 10000;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
            else
            {
                List<int> h1group = [0, 2, 4, 6];
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

                var isH1group = h1group.Contains(myindex);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_FogDragon_Stack1";
                dp.Scale = new(6);
                dp.Owner = accessory.Data.PartyList[2];
                dp.Color = isH1group ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                dp.Delay = 10000;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_FogDragon_Stack2";
                dp.Scale = new(6);
                dp.Owner = accessory.Data.PartyList[3];
                dp.Color = !isH1group ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                dp.Delay = 10000;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }

        }
        [ScriptMethod(name: "P1_FogDragon_PrePosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4015[45])$"])]
        public void P1_FogDragon_PrePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var rot8 = myindex switch
            {
                0 => 0,
                1 => 1,
                2 => 6,
                3 => 4,
                4 => 5,
                5 => 3,
                6 => 7,
                7 => 2,
                _ => 0,
            };
            var mPosEnd = RotatePoint(new(100, 0, 82), new(100, 0, 100), float.Pi / 4 * rot8);
            if (myindex==0)
            {
                mPosEnd = RotatePoint(mPosEnd, new(100, 0, 100), float.Pi / 36);
            }
            if (myindex == 6)
            {
                mPosEnd = RotatePoint(mPosEnd, new(100, 0, 100), float.Pi / -36);
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_FogDragon_PrePosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = mPosEnd;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        }
        [ScriptMethod(name: "P1_FogDragon_Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40158)$"])]
        public void P1_FogDragon_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;

            lock (this)
            {
                P1FogDragonCount ++; 
                if(P1FogDragonCount != 3) return;
                Task.Delay(100).ContinueWith(t =>
                {
                    if (!P1FogDragonThunder)
                    {
                        var safeDir = P1FogDragonRecord.IndexOf(0);
                        List<int> h1group = [0, 2, 4, 6];
                        var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                        var isH1group = h1group.Contains(myindex);
                        var rot8 = safeDir switch
                        {
                            0 => isH1group ? 0 : 4,
                            1 => isH1group ? 5 : 1,
                            2 => isH1group ? 6 : 2,
                            3 => isH1group ? 7 : 3,
                            _ => 0
                        };
                        var mPosEnd = RotatePoint(new(100,0,84), new(100, 0, 100), float.Pi / 4 * rot8);

                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_FogDragon_StackPosition";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = mPosEnd;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 9000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                    else
                    {
                        var safeDir = P1FogDragonRecord.IndexOf(0);
                        List<int> h1group = [0, 2, 4, 6];
                        var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                        var isH1group = h1group.Contains(myindex);
                        Vector3 p1 = new(100.0f, 0, 88.0f);
                        Vector3 p2 = new(100.0f, 0, 80.5f);
                        Vector3 p3 = new(106.5f, 0, 81.5f);
                        Vector3 p4 = new(093.5f, 0, 81.5f);
                        var rot8 = safeDir switch
                        {
                            0 => isH1group ? 0 : 4,
                            1 => isH1group ? 5 : 1,
                            2 => isH1group ? 6 : 2,
                            3 => isH1group ? 7 : 3,
                            _ => 0
                        };
                        var myPosA = myindex switch
                        {
                            0 => p2,
                            1 => p2,
                            2 => p1,
                            3 => p1,
                            4 => p3,
                            5 => p3,
                            6 => p4,
                            7 => p4,
                            _ => p1,
                        };
                        var mPosEnd = RotatePoint(myPosA, new(100, 0, 100), float.Pi / 4 * rot8);

                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_FogDragon_SpreadPosition";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = mPosEnd;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 9000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                });
                
            }

        }


        [ScriptMethod(name: "P1_SpinningWheel_ThunderFireRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4015[01])$"], userControl: false)]
        public void P1_SpinningWheel_ThunderFireRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            P1SpinningWheelThunder = (@event["ActionId"] == "40151");
        }
        [ScriptMethod(name: "P1_SpinningWheel_ThunderLine", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40164)$"])]
        public void P1_SpinningWheel_ThunderLine(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var delay = 4000;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_SpinningWheel_ThunderLine";
            dp.Scale = new(20, 40);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay=delay;
            dp.DestoryAt = 9700-delay;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P1_SpinningWheel_FireLine", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40161)$"])]
        public void P1_SpinningWheel_FireLine(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var delay = 4000;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_SpinningWheel_FireLine";
            dp.Scale = new(10, 40);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = delay;
            dp.DestoryAt = 7700 - delay;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }

        [ScriptMethod(name: "P1_SpinningWheel_GrabRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4165"],userControl:false)]
        public void P1_SpinningWheel_GrabRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            lock (this)
            {
                P1SpinningWheelGrab[accessory.Data.PartyList.IndexOf(tid)] = 1;
            }
        }
        [ScriptMethod(name: "P1_SpinningWheel_KnockbackPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40152)$"])]
        public void P1_SpinningWheel_KnockbackPosition(Event @event, ScriptAccessory accessory)
        {
            //dy 7
            if (parse != 1d) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var pos= JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if (MathF.Abs(pos.Z - 100) > 1) return;
            
            var atEast = pos.X - 100 > 1;
            var o1= P1SpinningWheelGrab.IndexOf(1);
            var o2 = P1SpinningWheelGrab.LastIndexOf(1);
            List<int> upGroup = [];
            if (P1BrightFireGroup==P1BrightFireEnum.TH_Up)
            {
                upGroup.Add(o1);
                if (o1 != 1 && o2 != 1) upGroup.Add(1);
                if (o1 != 2 && o2 != 2) upGroup.Add(2);
                if (o1 != 3 && o2 != 3) upGroup.Add(3);
                if (upGroup.Count < 4 && o1 != 0 && o2 != 0) upGroup.Add(0);
                if (upGroup.Count < 4 && o1 != 4 && o2 != 4) upGroup.Add(4);
            }
            if (P1BrightFireGroup == P1BrightFireEnum.MtGroup_Up)
            {
                upGroup.Add(o1);
                if (o1 != 2 && o2 != 2) upGroup.Add(2);
                if (o1 != 4 && o2 != 4) upGroup.Add(4);
                if (o1 != 6 && o2 != 6) upGroup.Add(6);
                if (upGroup.Count < 4 && o1 != 0 && o2 != 0) upGroup.Add(0);
                if (upGroup.Count < 4 && o1 != 1 && o2 != 1) upGroup.Add(1);
            }
            if (P1BrightFireGroup == P1BrightFireEnum.MtStD3D4_Up)
            {
                List<int> upIndex = [0, 1, 6, 7];
                if (upIndex.Contains(o1) && !upIndex.Contains(o2)) upGroup.Add(o1);
                if (upIndex.Contains(o2) && !upIndex.Contains(o1)) upGroup.Add(o2);
                if (upIndex.Contains(o1) && !upIndex.Contains(o2))
                {
                    if (upIndex.IndexOf(o1)<upIndex.IndexOf(o2))
                    {
                        upGroup.Add(o1);
                    }
                    else
                    {
                        upGroup.Add(o2);
                    }
                }
                var up0 = upGroup[0];
                var down0 = up0 == o1 ? o2 : o1;
                if (up0 != 1 && down0 != 1) upGroup.Add(1);
                if (up0 != 6 && down0 != 6) upGroup.Add(6);
                if (up0 != 7 && down0 != 7) upGroup.Add(7);
                if (upGroup.Count < 4 && up0 != 0 && down0 != 0) upGroup.Add(0);
                if (upGroup.Count < 4 && up0 != 4 && down0 != 4) upGroup.Add(4);
            }

            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var dealpos1 = new Vector3(atEast ? 105.5f : 94.5f, 0, upGroup.Contains(myindex) ? 93 : 107);
            var dealpos2 = new Vector3(atEast ? 102 : 98, 0, upGroup.Contains(myindex) ? 93 : 107);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_SpinningWheel_KnockbackPosition1";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos1;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_SpinningWheel_KnockbackPosition2";
            dp.Scale = new(2);
            dp.Position = dealpos1;
            dp.TargetPosition = dealpos2;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_SpinningWheel_KnockbackPosition3";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos2;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 4000;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


        }

        [ScriptMethod(name: "P1_FourTether_ClearRecorder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40170)$"])]
        public void P1_FourTether_ClearRecorder(Event @event, ScriptAccessory accessory)
        {
            
            if (parse != 1d) return;
            P1FourTether.Clear();
            P1FourTetherStart =true;
            if (p1Thther4Marker)
                accessory.Method.MarkClear();
        }
        [ScriptMethod(name: "P1_FourTether_Recorder", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(00F9|011F)$"],userControl:false)]
        public void P1_FourTether_Recorder(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index= accessory.Data.PartyList.IndexOf(tid);
            var id = @event["Id"] == "00F9" ? 10 : 20;
            P1FourTether.Add(id + index);
        }
        [ScriptMethod(name: "P1_FourTether_HeadMark", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(00F9|011F)$"],userControl:false)]
        public void P1_FourTether_HeadMark(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            if (!p1Thther4Marker) return;
            if (!P1FourTetherStart) return;
            Task.Delay(50).ContinueWith(t =>
            {
                var index = P1FourTether.Last() % 10;
                accessory.Method.Mark(accessory.Data.PartyList[index], (KodakkuAssist.Module.GameOperate.MarkType)P1FourTether.Count);
                //accessory.Log.Debug($"{index} {(KodakkuAssist.Module.GameOperate.MarkType)P1FourTether.Count}");
            });
        }
        [ScriptMethod(name: "P1_FourTether_Position", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(00F9|011F)$"])]
        public void P1_FourTether_Position(Event @event, ScriptAccessory accessory)
        {
            if (!P1FourTetherStart) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var dis = 2.5f;//Distance from target
            var far = 5f;//Distance from boss
            Task.Delay(50).ContinueWith(t =>
            {
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                Vector3 t1p1 = new(100, 0, 100 - far);
                Vector3 t1p2 = new(100, 0, 100-far-dis);
                Vector3 t2p1 = new(100, 0, 100 + far);
                Vector3 t2p2 = new(100, 0, 100 + far + dis);
                Vector3 t3p1 = new(100, 0, 100 - far - dis);
                Vector3 t3p2 = new(100, 0, 100 - far);
                Vector3 t4p1 = new(100, 0, 100 + far + dis);
                Vector3 t4p2 = new(100, 0, 100 + far);
                
                if (P1FourTether.Count ==1 && tid==accessory.Data.Me)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FourTether_Line1Position1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t1p1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 13000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FourTether_Line1Position2";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t1p2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay =13000;
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                if (P1FourTether.Count == 2 && tid == accessory.Data.Me)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FourTether_Line2Position1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t2p1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 13500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FourTether_Line2Position2";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t2p2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 13500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                if (P1FourTether.Count == 3 && tid == accessory.Data.Me)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FourTether_Line3Position1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t3p1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FourTether_Line3Position2";
                    dp.Scale = new(3);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t3p2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 7500;
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                if (P1FourTether.Count == 4 && tid == accessory.Data.Me)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FourTether_Line4Position1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t4p1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 8500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FourTether_Line4Position2";
                    dp.Scale = new(3);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t4p2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 8500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                if (P1FourTether.Count == 4)
                {
                    var tehterObjIndex = P1FourTether.Select(o => o % 10).ToList();
                    var tehterIsFire = P1FourTether.Select(o => o < 20).ToList();
                    List<int> idleObjIndex = [];
                    if (p1Thther4Type==P1TetherEnum.OneLine)
                    {
                        for (int i = 0; i < accessory.Data.PartyList.Count; i++)
                        {
                            if (!tehterObjIndex.Contains(i))
                            { idleObjIndex.Add(i); }
                        }
                    }
                    if(p1Thther4Type==P1TetherEnum.Mgl_TwoLine)
                    {
                        List<int> group1 = [0, 1, 2, 3];
                        List<int> group2 = [4, 5, 6, 7];
                        group1.RemoveAll(x => tehterObjIndex.Contains(x));
                        while (group1.Count>2)
                        {
                            var m = group1.First();
                            group1.Remove(m);
                            group2.Add(m);
                        }
                        idleObjIndex.AddRange(group1);
                        idleObjIndex.AddRange(group2);
                    }
                    
                    if (!idleObjIndex.Contains(myindex)) return;

                    Vector3 i1p1 = tehterIsFire[0] ? new(100, 0, 100 - far - dis) : new(100 - dis, 0, 100 - far);
                    Vector3 i1p2 = tehterIsFire[2] ? new(100, 0, 100 - far - dis) : new(100 - dis, 0, 100 - far);
                    Vector3 i2p1 = tehterIsFire[0] ? new(100, 0, 100 - far - dis) : new(100 + dis, 0, 100 - far);
                    Vector3 i2p2 = tehterIsFire[2] ? new(100, 0, 100 - far - dis) : new(100 + dis, 0, 100 - far);
                    Vector3 i3p1 = tehterIsFire[1] ? new(100, 0, 100 + far + dis) : new(100 - dis, 0, 100 + far);
                    Vector3 i3p2 = tehterIsFire[3] ? new(100, 0, 100 + far + dis) : new(100 - dis, 0, 100 + far);
                    Vector3 i4p1 = tehterIsFire[1] ? new(100, 0, 100 + far + dis) : new(100 + dis, 0, 100 + far);
                    Vector3 i4p2 = tehterIsFire[3] ? new(100, 0, 100 + far + dis) : new(100 + dis, 0, 100 + far);
                    Vector3 dealpos1 = default;
                    Vector3 dealpos2 = default;

                    dealpos1 = idleObjIndex.IndexOf(myindex) switch
                    {
                        0 => i1p1,
                        1 => i2p1,
                        2 => i3p1,
                        3 => i4p1,
                    };
                    dealpos2 = idleObjIndex.IndexOf(myindex) switch
                    {
                        0 => i1p2,
                        1 => i2p2,
                        2 => i3p2,
                        3 => i4p2,
                    };
                    var upgroup = (idleObjIndex.IndexOf(myindex) == 0 || idleObjIndex.IndexOf(myindex) == 1);

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FourTether_Position1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = upgroup ? 5000 : 8500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FourTether_Position2";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = upgroup ? 5000 : 8500;
                    dp.DestoryAt = upgroup ? 6000 : 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            });
        }

        [ScriptMethod(name: "P1_Tower_Recorder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4012[234567]|4013[15])$"], userControl: false)]
        public void P1_Tower_Recorder(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            lock (this)
            {
                var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                var count = @event["ActionId"] switch
                {
                    "40135" => 1,
                    "40131" => 1,
                    "40122" => 2,
                    "40123" => 3,
                    "40124" => 4,
                    "40125" => 2,
                    "40126" => 3,
                    "40127" => 4,
                };
                if (MathF.Abs(pos.Z - 100) < 1)
                {
                    P1Tower[1] = count;
                }
                else
                {
                    if (pos.Z - 100 > 1) P1Tower[2] = count;
                    else P1Tower[0] = count;
                }
                if (pos.X - 100 > 1)
                {
                    P1Tower[3] = 1;
                }
            }
        }
        [ScriptMethod(name: "P1_Tower_ThunderFireLine", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40134|40129)$"])]
        public void P1_Tower_ThunderFireLine(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (@event["ActionId"] == "40134")
            {
                //Thunder
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_Tower_ThunderLine";
                dp.Scale = new(20, 40);
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 8200;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_Tower_ThunderLineInner";
                dp.Scale = new(10, 40);
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 6500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_Tower_FireLine";
                dp.Scale = new(10, 40);
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 6500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
            }


        }
        [ScriptMethod(name: "P1_Tower_Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40134|40129)$"])]
        public void P1_Tower_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1d) return;
            Task.Delay(100).ContinueWith(t =>
            {
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                if (@event["ActionId"] == "40134")
                {
                    var eastTower = P1Tower[3] == 1;
                    //Thunder
                    if (myindex==0|| myindex==1)
                    {
                        var dx = eastTower ? -10.5f : 10.5f;
                        var dy = myindex == 1 ? -5.5f : 5.5f;
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_ThunderTower_Position_T";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = new(100+dx,0,100+dy);
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 10500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                    else
                    {
                        var myIndex2 = myindex - 1;
                        Vector3 dealpos = default;
                        if (myIndex2 > 0 && myIndex2 <= P1Tower[0]) dealpos = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                        if (myIndex2 > P1Tower[0] && myIndex2 <= P1Tower[0]+ P1Tower[1]) dealpos = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                        if (myIndex2 > P1Tower[0] + P1Tower[1] && myIndex2 <= P1Tower[0] + P1Tower[1]+ P1Tower[2]) dealpos = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                        Vector3 towerpos = default;
                        if (myIndex2 > 0 && myIndex2 <= P1Tower[0]) towerpos = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                        if (myIndex2 > P1Tower[0] && myIndex2 <= P1Tower[0] + P1Tower[1]) towerpos = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                        if (myIndex2 > P1Tower[0] + P1Tower[1] && myIndex2 <= P1Tower[0] + P1Tower[1] + P1Tower[2]) towerpos = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_ThunderTower_Position_ND";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = dealpos;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 10500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_ThunderTower_ND";
                        dp.Scale = new(4);
                        dp.Position = towerpos;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 10500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

                    }
                }
                else
                {
                    var eastTower = P1Tower[3] == 1;
                    //Fire
                    if (myindex == 0 || myindex == 1)
                    {
                        var dx2 = eastTower ? -2f : 2f;
                        var dx1 = eastTower ? -5.5f : 5.5f;
                        var dy = myindex == 1 ? -5.5f : 5.5f;

                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_ThunderTower_Position_T1";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = new(100 + dx1, 0, 100 + dy);
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 6500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_ThunderTower_Position_T2";
                        dp.Scale = new(2);
                        dp.Position = new(100 + dx1, 0, 100 + dy);
                        dp.TargetPosition = new(100 + dx2, 0, 100 + dy);
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 6500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_ThunderTower_Position_T3";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = new(100 + dx2, 0, 100 + dy);
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.Delay = 6500;
                        dp.DestoryAt = 1700;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                    else
                    {
                        var myIndex2 = myindex - 1;
                        Vector3 dealpos = default;
                        if (myIndex2 > 0 && myIndex2 <= P1Tower[0]) dealpos = new(eastTower ? 102f : 98f, 0, 90.81f);
                        if (myIndex2 > P1Tower[0] && myIndex2 <= P1Tower[0] + P1Tower[1]) dealpos = new(eastTower ? 102f : 98f, 0, 100f);
                        if (myIndex2 > P1Tower[0] + P1Tower[1] && myIndex2 <= P1Tower[0] + P1Tower[1] + P1Tower[2]) dealpos = new(eastTower ? 102f : 98f, 0, 109.18f);

                        Vector3 towerpos = default;
                        if (myIndex2 > 0 && myIndex2 <= P1Tower[0]) towerpos = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                        if (myIndex2 > P1Tower[0] && myIndex2 <= P1Tower[0] + P1Tower[1]) towerpos = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                        if (myIndex2 > P1Tower[0] + P1Tower[1] && myIndex2 <= P1Tower[0] + P1Tower[1] + P1Tower[2]) towerpos = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_ThunderTower_Position_ND";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = dealpos;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 9000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_ThunderTower_ND";
                        dp.Scale = new(4);
                        dp.Position = towerpos;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 10500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

                    }
                }
            });
            
        }

        #endregion

        #region P2
        [ScriptMethod(name: "P2_PhaseSwitch", eventType: EventTypeEnum.Director, eventCondition: ["Instance:800375BF", "Command:8000001E"],userControl:false)]
        public void P2_PhaseSwitch(Event @event, ScriptAccessory accessory)
        {
            parse = 2d;
        }

        [ScriptMethod(name: "P2_DiamondDust_BossIdRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40180)$"], userControl: false)]
        public void P2_DiamondDust_BossIdRecord(Event @event, ScriptAccessory accessory)
        {
            parse = 2.1d;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            P2BossId = sid;
            P2DDIceDir.Clear();
        }
        [ScriptMethod(name: "P2_DiamondDust_CircleDonutRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4020[23]))$"],userControl: false)]
        public void P2_DiamondDust_CircleDonutRecord(Event @event, ScriptAccessory accessory)
        {
            P2DDCircle = (@event["ActionId"] == "40202");//Circle
        }
        [ScriptMethod(name: "P2_DiamondDust_CircleDonut", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4020[23]))$"])]
        public void P2_DiamondDust_CircleDonut(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (@event["ActionId"]=="40202")//Circle
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_DiamondDust_Circle";
                dp.Scale = new(16);
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_DiamondDust_Donut";
                dp.Scale = new(20);
                dp.InnerScale = new(4);
                dp.Radian = float.Pi * 2;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
            }
        }
        [ScriptMethod(name: "P2_DiamondDust_ConeGuide", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4020[23]))$"])]
        public void P2_DiamondDust_ConeGuide(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dur = 3000;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_ConeGuide1";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Owner = sid;
            dp.TargetResolvePattern=PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_ConeGuide2";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_ConeGuide3";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_ConeGuide4";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


        }
        [ScriptMethod(name: "P2_DiamondDust_IceFlowerPosition", eventType: EventTypeEnum.TargetIcon)]
        public void P2_DiamondDust_IceFlowerPosition(Event @event, ScriptAccessory accessory)
        {
            //accessory.Log.Debug($"{ParsTargetIcon(@event["Id"])}");
            if (ParsTargetIcon(@event["Id"]) != 127) return;
            if (parse != 2.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var rot = myIndex switch
            {
                0 => 6,
                1 => 0,
                2 => 4,
                3 => 2,
                4 => 4,
                5 => 2,
                6 => 6,
                7 => 0,
                _ => 0,
            };
            Vector3 epos1 = P2DDCircle ? new(119.5f, 0, 100.0f) : new(103.5f, 0, 100.0f);
            Vector3 epos2 = P2DDCircle ? new(119.5f, 0, 100.0f) : new(108.0f, 0, 100.0f);
            var dir8 = P2DDIceDir.FirstOrDefault() % 4;
            var dr = dir8 == 0 || dir8 == 2 ? -1 : 0;
            var dealpos1 = RotatePoint(epos1, new(100, 0, 100), float.Pi / 4 * (rot + dr));
            var dealpos2 = RotatePoint(epos2, new(100, 0, 100), float.Pi / 4 * (rot + dr));
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_IceFlowerPosition1";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition= dealpos1;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_IceFlowerPosition3";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Position = dealpos1;
            dp.TargetPosition = dealpos2;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_IceFlowerPosition3";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos2;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 5500;
            dp.DestoryAt = 2500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P2_DiamondDust_ConeGuidePosition", eventType: EventTypeEnum.TargetIcon)]
        public void P2_DiamondDust_ConeGuidePosition(Event @event, ScriptAccessory accessory)
        {
            //accessory.Log.Debug($"{ParsTargetIcon(@event["Id"])}");
            if (ParsTargetIcon(@event["Id"]) != 127) return;
            if (parse != 2.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            int[] group = [6, 7, 4, 5, 2, 3, 0, 1];
            if (accessory.Data.PartyList.IndexOf(tid) != group[myIndex]) return;
            var rot = myIndex switch
            {
                0 => 6,
                1 => 0,
                2 => 4,
                3 => 2,
                4 => 4,
                5 => 2,
                6 => 6,
                7 => 0,
                _ => 0,
            };
            var dir8 = P2DDIceDir.FirstOrDefault() % 4;
            var dr = dir8 == 0 || dir8 == 2 ? 0 : -1;
            Vector3 epos = P2DDCircle ? new(116.5f, 0, 100f): new(101f, 0, 100f);
            var dealpos = RotatePoint(epos, new(100, 0, 100), float.Pi / 4 * (rot+dr));
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_ConeGuidePosition";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 6500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P2_DiamondDust_ChainRingRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40198)$"], userControl: false)]
        public void P2_DiamondDust_ChainRingRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var pos= JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            lock (P2DDIceDir)
            {
                P2DDIceDir.Add(PositionTo8Dir(pos, new(100, 0, 100)));
            }
        }
        [ScriptMethod(name: "P2_DiamondDust_KnockbackPosition", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^((4020[23]))$", "TargetIndex:1"])]
        public void P2_DiamondDust_KnockbackPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            Task.Delay(2500).ContinueWith(t =>
            {
                var nPos = new Vector3(100, 0, 96);
                var dir8 = P2DDIceDir.FirstOrDefault() % 4;
                int[] h1Group = [0, 2, 4, 6];
                var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var isH1Group = h1Group.Contains(myIndex);
                
                var rot = dir8 switch
                {
                    0 => 4,
                    1 => 1,
                    2 => 2,
                    3 => 3,
                };
                
                rot += isH1Group ? 4 : 0;
                var dealpos = RotatePoint(nPos, new(100, 0, 100), float.Pi / 4 * rot);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_DiamondDust_KnockbackPosition";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            });
            
        }
        [ScriptMethod(name: "P2_DiamondDust_SequentialBladeClonePosition", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^40208$", "TargetIndex:1"])]
        public void P2_DiamondDust_SequentialBladeClonePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);

            Vector3 dealpos = new(100 + (pos.X - 100) * 1.4f, 0, 100 + (pos.Z - 100) * 1.4f);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_SequentialBladeClonePosition";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


        }
        [ScriptMethod(name: "P2_DiamondDust_SequentialBladeRange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4019[34]$"])]
        public void P2_DiamondDust_SequentialBladeRange(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var time = 300;
            //93 First front
            if (@event["ActionId"]=="40193")
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_DiamondDust_SequentialBladeRangeFront1";
                dp.Scale = new(30);
                dp.Radian = float.Pi / 2 * 3;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 3500-time;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_DiamondDust_SequentialBladeRangeBack2";
                dp.Scale = new(30);
                dp.Radian = float.Pi / 2;
                dp.Rotation = float.Pi;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 3500-time;
                dp.DestoryAt = 2000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_DiamondDust_SequentialBladeRangeBack1";
                dp.Scale = new(30);
                dp.Radian = float.Pi / 2;
                dp.Rotation = float.Pi;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 3500-time;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_DiamondDust_SequentialBladeRangeFront2";
                dp.Scale = new(30);
                dp.Radian = float.Pi / 2 * 3;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 3500-time;
                dp.DestoryAt = 2000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
        }
        [ScriptMethod(name: "P2_DiamondDust_BossLookaway", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^40208$", "TargetIndex:1"])]
        public void P2_DiamondDust_BossLookaway(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_BossLookaway";
            dp.Scale = new(5);
            dp.Owner = accessory.Data.Me;
            dp.TargetObject=P2BossId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.SightAvoid, dp);


        }

        [ScriptMethod(name: "P2_DoubleMirror_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40179)$"], userControl: false)]
        public void P2_DoubleMirror_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 2.2d;
            P2RedMirror.Clear();
        }
        [ScriptMethod(name: "P2_DoubleMirror_SpreadStack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[01])$"])]
        public void P2_DoubleMirror_SpreadStack(Event @event, ScriptAccessory accessory)
        {
            if(parse != 2.2) return;
            if (@event["ActionId"]=="40221")
            {
                foreach (var pm in accessory.Data.PartyList)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_DoubleMirror_Spread";
                    dp.Scale = new(5);
                    dp.Owner = pm;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
            else
            {
                int[] group = [6, 7, 4, 5, 2, 3, 0, 1];
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                for (int i = 0; i < 4; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_DoubleMirror_Stack";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = group[myindex]==i||i==myindex?accessory.Data.DefaultSafeColor: accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
            
        }
        [ScriptMethod(name: "P2_DoubleMirror_BlueMirrorDonutAndGuide", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375BF", "State:00020001"])]
        public void P2_DoubleMirror_BlueMirrorDonutAndGuide(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!int.TryParse(@event["Index"], out var dir8)) return;
            Vector3 npos = new(100, 0, 80);
            dir8--;
            Vector3 dealpos = RotatePoint(npos, new(100, 0, 100), float.Pi / 4 * dir8);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DoubleMirror_BlueMirrorDonut";
            dp.Scale = new(20);
            dp.InnerScale = new(4);
            dp.Radian = float.Pi * 2;
            dp.Position = dealpos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

             dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DoubleMirror_BlueMirrorConeGuide1";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DoubleMirror_BlueMirrorConeGuide2";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DoubleMirror_BlueMirrorConeGuide3";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DoubleMirror_BlueMirrorConeGuide4";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }
        [ScriptMethod(name: "P2_DoubleMirror_RedMirrorDonutAndGuide", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375BF", "State:02000100"])]
        public void P2_DoubleMirror_RedMirrorDonutAndGuide(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!int.TryParse(@event["Index"], out var dir8)) return;
            Vector3 npos = new(100, 0, 80);
            dir8--;
            Vector3 dealpos = RotatePoint(npos, new(100, 0, 100), float.Pi / 4 * dir8);
            var dur = 3000;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DoubleMirror_RedMirrorDonut";
            dp.Scale = new(20);
            dp.InnerScale = new(4);
            dp.Radian = float.Pi * 2;
            dp.Position = dealpos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 17500;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DoubleMirror_RedMirrorConeGuide1";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 23000-dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DoubleMirror_RedMirrorConeGuide2";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 23000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DoubleMirror_RedMirrorConeGuide3";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 23000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DoubleMirror_RedMirrorConeGuide4";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 23000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }
        [ScriptMethod(name: "P2_DoubleMirror_BlueMirrorDonutAndGuidePosition", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375BF", "State:00020001"])]
        public void P2_DoubleMirror_BlueMirrorDonutAndGuidePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!int.TryParse(@event["Index"], out var dir8)) return;
            Vector3 npos = new(100, 0, 80);
            dir8--;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myindex == 0 || myindex == 1 || myindex == 4 || myindex == 5)
            {
                dir8 += 4;
                npos = new(100, 0, 85);
            }
           
            Vector3 dealpos = RotatePoint(npos, new(100, 0, 100), float.Pi / 4 * dir8);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DoubleMirror_BlueMirrorDonutAndGuidePosition";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 12000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P2_DoubleMirror_RedMirrorGuidePosition", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375BF", "State:02000100"])]
        public void P2_DoubleMirror_RedMirrorGuidePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!int.TryParse(@event["Index"], out var dir8)) return;
            dir8--;
            lock (P2RedMirror)
            {
                P2RedMirror.Add(dir8);
                if (P2RedMirror.Count != 2) return;
            }
            var leftRot8 = (P2RedMirror[0] - P2RedMirror[1] == -2 || P2RedMirror[0] - P2RedMirror[1] - 8 == -2) ? P2RedMirror[0] : P2RedMirror[1];
            var rightRot8 = (P2RedMirror[0] - P2RedMirror[1] == 2 || P2RedMirror[0] + 8 - P2RedMirror[1] == 2) ? P2RedMirror[0] : P2RedMirror[1];

            var myrot = leftRot8;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myindex == 0 || myindex == 1 || myindex == 4 || myindex == 5)
            {
                myrot = rightRot8;
            }
            Vector3 npos = myindex switch
            {
                0 => new(102f, 0, 80.5f),
                1 => new(98f, 0, 80.5f),
                2 => new(102f, 0, 80.5f),
                3 => new(98f, 0, 80.5f),
                4 => new(101.3f, 0, 83f),
                5 => new(98.7f, 0, 83f),
                6 => new(101.3f, 0, 83f),
                7 => new(98.7f, 0, 83f),
                _ => new(100, 0, 80)
            };
            var dealpos = RotatePoint(npos, new(100, 0, 100), myrot * float.Pi / 4);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DoubleMirror_RedMirrorGuidePosition";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 13500;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "P2_LightRampant_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40212)$"], userControl: false)]
        public void P2_LightRampant_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 2.3d;
            P2LightRampantCircle.Clear();
            P2LightRampantTetherDone = false;
            P2LightRampantBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        }
        [ScriptMethod(name: "P2_LightRampant_BigCircleCollection", eventType: EventTypeEnum.TargetIcon,userControl:false)]
        public void P2_LightRampant_BigCircleCollection(Event @event, ScriptAccessory accessory)
        {
            if (ParsTargetIcon(@event["Id"]) != 157) return;
            if (parse != 2.3) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index=accessory.Data.PartyList.IndexOf(tid);
            lock (P2LightRampantCircle)
            {
                P2LightRampantCircle.Add(index);
            }
        }
        [ScriptMethod(name: "P2_LightRampant_BuffCollection", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2257"], userControl: false)]
        public void P2_LightRampant_BuffCollection(Event @event, ScriptAccessory accessory)
        {
            
            if (parse != 2.3) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (!int.TryParse(@event["StackCount"], out var count)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            lock (P2LightRampantBuff)
            {
                P2LightRampantBuff[index] = count;
            }
        }
        [ScriptMethod(name: "P2_LightRampant_SpreadStack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[01])$"])]
        public void P2_LightRampant_SpreadStack(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.3) return;
            if (@event["ActionId"] == "40221")
            {
                foreach (var pm in accessory.Data.PartyList)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_LightRampant_Spread";
                    dp.Scale = new(5);
                    dp.Owner = pm;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
            else
            {
                int[] group = [6, 7, 4, 5, 2, 3, 0, 1];
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                for (int i = 0; i < 4; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_LightRampant_Stack";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = group[myindex] == i || i == myindex ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }

        }
        [ScriptMethod(name: "P2_LightRampant_StackBuff", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4159"])]
        public void P2_LightRampant_StackBuff(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.3) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_LightRampant_StackBuff";
            dp.Scale = new(5);
            dp.Owner = tid;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 12000;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P2_LightRampant_TowerPosition", eventType: EventTypeEnum.TargetIcon)]
        public void P2_LightRampant_TowerPosition(Event @event, ScriptAccessory accessory)
        {

            if (ParsTargetIcon(@event["Id"]) != 157) return;
            if (parse != 2.3) return;
            lock (this)
            {
                if (P2LightRampantTetherDone) return;
                P2LightRampantTetherDone = true;
            }
            Task.Delay(50).ContinueWith(t =>
            {
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                if (P2LightRampantCircle.Contains(myindex)) return;
                
                List<int> tetherGroup = [];
                if (P2LightRampant8DirSet == P2LightRampant8DirEmum.Normal)
                {
                    if (!P2LightRampantCircle.Contains(2)) tetherGroup.Add(2);
                    if (!P2LightRampantCircle.Contains(6)) tetherGroup.Add(6);
                    if (!P2LightRampantCircle.Contains(0)) tetherGroup.Add(0);
                    if (!P2LightRampantCircle.Contains(7)) tetherGroup.Add(7);
                    if (!P2LightRampantCircle.Contains(3)) tetherGroup.Add(3);
                    if (!P2LightRampantCircle.Contains(5)) tetherGroup.Add(5);
                    if (!P2LightRampantCircle.Contains(1)) tetherGroup.Add(1);
                    if (!P2LightRampantCircle.Contains(4)) tetherGroup.Add(4);
                }
                if (P2LightRampant8DirSet == P2LightRampant8DirEmum.TN_Up)
                {
                    if (!P2LightRampantCircle.Contains(0)) tetherGroup.Add(0);
                    if (!P2LightRampantCircle.Contains(1)) tetherGroup.Add(1);
                    if (!P2LightRampantCircle.Contains(2)) tetherGroup.Add(2);
                    if (!P2LightRampantCircle.Contains(3)) tetherGroup.Add(3);
                    if (!P2LightRampantCircle.Contains(7)) tetherGroup.Add(7);
                    if (!P2LightRampantCircle.Contains(6)) tetherGroup.Add(6);
                    if (!P2LightRampantCircle.Contains(5)) tetherGroup.Add(5);
                    if (!P2LightRampantCircle.Contains(4)) tetherGroup.Add(4);
                }


                var myGroupIndex = tetherGroup.IndexOf(myindex);
                Vector3 t1 = new(100.00f, 0, 084.00f);
                Vector3 t2 = new(113.85f, 0, 092.00f);
                Vector3 t3 = new(113.85f, 0, 108.00f);
                Vector3 t4 = new(100.00f, 0, 116.00f);
                Vector3 t5 = new(086.14f, 0, 108.00f);
                Vector3 t6 = new(086.14f, 0, 092.00f);

                Vector3 pa = new(100.00f, 0, 82.00f);
                Vector3 pb = new(118.00f, 0, 100.00f);
                Vector3 pc = new(100.00f, 0, 118.00f);
                Vector3 pd = new(82.00f, 0, 100.00f);


                Vector3 dealpos = default;
                Vector3 dealpos2 = default;
                if (P2LightRampantTetherDeal == P2LightRampantTetherEmum.CircleNum)
                {
                    var count = 0;
                    if (myindex == 0)
                    {
                        dealpos = t4;
                    }
                    count += P2LightRampantCircle.Contains(0) ? 1 : 0;
                    if (myindex == 7)
                    {
                        dealpos = P2LightRampantCircle.Contains(0) ? t4 : t2;
                    }
                    count += P2LightRampantCircle.Contains(7) ? 1 : 0;
                    if (myindex == 1)
                    {
                        if (count == 0) dealpos = t6;
                        if (count == 1) dealpos = t2;
                        if (count == 2) dealpos = t4;
                    }
                    count += P2LightRampantCircle.Contains(1) ? 1 : 0;
                    if (myindex == 5)
                    {
                        if (count == 0) dealpos = t3;
                        if (count == 1) dealpos = t6;
                        if (count == 2) dealpos = t2;
                    }
                    count += P2LightRampantCircle.Contains(5) ? 1 : 0;
                    if (myindex == 3)
                    {
                        if (count == 0) dealpos = t5;
                        if (count == 1) dealpos = t3;
                        if (count == 2) dealpos = t6;
                    }
                    count += P2LightRampantCircle.Contains(3) ? 1 : 0;
                    if (myindex == 4)
                    {
                        if (count == 0) dealpos = t1;
                        if (count == 1) dealpos = t5;
                        if (count == 2) dealpos = t3;
                    }
                    count += P2LightRampantCircle.Contains(4) ? 1 : 0;
                    if (myindex == 2)
                    {
                        dealpos = P2LightRampantCircle.Contains(6) ? t1 : t5;
                    }
                    if (myindex == 6)
                    {
                        dealpos = t1;
                    }


                    if ((dealpos - t1).Length() < 1 || (dealpos - t2).Length() < 1 || (dealpos - t3).Length() < 1)
                    {
                        dealpos2 = pb;
                    }
                    else
                    {
                        dealpos2 = pd;
                    }
                }
                if (P2LightRampantTetherDeal == P2LightRampantTetherEmum.NewGrey9)
                {
                    var count = 0;
                    if (myindex == 0)
                    {
                        dealpos = t4;
                    }
                    count += P2LightRampantCircle.Contains(0) ? 1 : 0;
                    if (myindex == 7)
                    {
                        dealpos = P2LightRampantCircle.Contains(0) ? t4 : t6;
                    }
                    count += P2LightRampantCircle.Contains(7) ? 1 : 0;
                    if (myindex == 1)
                    {
                        if (count == 0) dealpos = t2;
                        if (count == 1) dealpos = t6;
                        if (count == 2) dealpos = t4;
                    }
                    count += P2LightRampantCircle.Contains(1) ? 1 : 0;
                    if (myindex == 5)
                    {
                        if (count == 0) dealpos = t5;
                        if (count == 1) dealpos = t2;
                        if (count == 2) dealpos = t6;
                    }
                    count += P2LightRampantCircle.Contains(5) ? 1 : 0;
                    if (myindex == 3)
                    {
                        if (count == 0) dealpos = t3;
                        if (count == 1) dealpos = t5;
                        if (count == 2) dealpos = t2;
                    }
                    count += P2LightRampantCircle.Contains(3) ? 1 : 0;
                    if (myindex == 4)
                    {
                        if (count == 0) dealpos = t1;
                        if (count == 1) dealpos = t3;
                        if (count == 2) dealpos = t5;
                    }
                    count += P2LightRampantCircle.Contains(4) ? 1 : 0;
                    if (myindex == 2)
                    {
                        dealpos = P2LightRampantCircle.Contains(6) ? t1 : t3;
                    }
                    if (myindex == 6)
                    {
                        dealpos = t1;
                    }

                    if ((dealpos - t2).Length() < 1 || (dealpos - t3).Length() < 1 || (dealpos - t4).Length() < 1)
                    {
                        dealpos2 = pb;
                    }
                    else
                    {
                        dealpos2 = pd;
                    }
                }
                if (P2LightRampantTetherDeal == P2LightRampantTetherEmum.LTeam)
                {
                    dealpos = myGroupIndex switch
                    {
                        1 => t1,
                        4 => t4,
                        0 => t5,
                        2 => t3,
                        3 => t6,
                        5 => t2,
                    };
                    if ((dealpos - t1).Length() < 1 || (dealpos - t2).Length() < 1 || (dealpos - t6).Length() < 1)
                    {
                        dealpos2 = pa;
                    }
                    else
                    {
                        dealpos2 = pc;
                    }
                }
                if (P2LightRampantTetherDeal == P2LightRampantTetherEmum.AC_Cross)
                {
                    dealpos = myGroupIndex switch
                    {
                        1 => t4,
                        4 => t1,
                        0 => t6,
                        2 => t2,
                        3 => t5,
                        5 => t3,
                    };
                    if ((dealpos - t1).Length() < 1 || (dealpos - t2).Length() < 1 || (dealpos - t6).Length() < 1)
                    {
                        dealpos2 = pa;
                    }
                    else
                    {
                        dealpos2 = pc;
                    }
                }

                

                var dur = 10000;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_LightRampant_TowerPosition";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = dur;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_LightRampant_TowerPosition";
                dp.Scale = new(4);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = dur;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

                

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_LightRampant_GatherPosition";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = dur;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_LightRampant_GatherPosition";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = dur;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            });

        }
        [ScriptMethod(name: "P2_DoubleMirror_CentralTowerPosition", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375BF", "State:00020001", "Index:00000015"])]
        public void P2_LightRampant_CentralTowerPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.3) return;
            var myindex= accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (P2LightRampantBuff[myindex]<=2)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_LightRampant_CentralTowerPosition";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(100,0,100);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_LightRampant_TowerPosition";
                dp.Scale = new(4);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = new(100, 0, 100);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
            }
        }
        
        [ScriptMethod(name: "P2_LightRampant_OctaSpreadPosition", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4022[01])$"])]
        public void P2_LightRampant_OctaSpreadPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.3) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var rot8 = myindex switch
            {
                0 => 0,
                1 => 2,
                2 => 6,
                3 => 4,
                4 => 5,
                5 => 3,
                6 => 7,
                7 => 1,
                _ => 0,
            };
            var mPosEnd = RotatePoint(new(100, 0, 95), new(100, 0, 100), float.Pi / 4 * rot8);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_LightRampant_OctaSpreadPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = mPosEnd;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        }

        [ScriptMethod(name: "P2.5_DarkCrystalAOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40262"])]
        public void P2_DarkCrystalAOE(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2.5_DarkCrystalAOE";
            dp.Scale = new(50);
            dp.Radian = float.Pi / 9;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
        }
        #endregion

        #region P3
        [ScriptMethod(name: "P3_TimeCompression_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40266)$"], userControl: false)]
        public void P3_TimeCompression_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 3.1d;
            P3FireBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            P3WaterBuff= [0, 0, 0, 0, 0, 0, 0, 0];
            P3ReturnBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            P3Lamp = [0, 0, 0, 0, 0, 0, 0, 0];
            P3LampWise = [0, 0, 0, 0, 0, 0, 0, 0];
        }
        [ScriptMethod(name: "P3_TimeCompression_BuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(2455|2456|2464|2462|2461|2460)$"], userControl: false)]
        public void P3_TimeCompression_BuffRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if(!float.TryParse(@event["Duration"], out var dur)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            if (index == -1) return;
            //Ice
            if (@event["StatusID"] == "2462")
            {
                lock (P3FireBuff)
                {
                    P3FireBuff[index] = 4;
                }
            }
            //Fire
            if (@event["StatusID"] == "2455")
            {
                
                var count = 1;
                if (dur > 20) count = 2;
                if (dur > 30) count = 3;
                lock (P3FireBuff)
                {
                    P3FireBuff[index] = count;
                }
            }
            //Return
            if (@event["StatusID"] == "2464")
            {
                var count = 1;
                if (dur > 20) count = 3;
                lock (P3ReturnBuff)
                {
                    P3ReturnBuff[index] = count;
                }
            }
            //Water
            if (@event["StatusID"] == "2461")
            {
                lock (P3WaterBuff)
                {
                    P3WaterBuff[index] = 1;
                }
            }
            //Circle
            if (@event["StatusID"] == "2460")
            {
                lock (P3WaterBuff)
                {
                    P3WaterBuff[index] = 2;
                }
            }
            //Lookaway
            if (@event["StatusID"] == "2456")
            {
                lock (P3WaterBuff)
                {
                    P3WaterBuff[index] = 3;
                }
            }



        }
        [ScriptMethod(name: "P3_TimeCompression_LampRecord", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(0085|0086)$"], userControl: false)]
        public void P3_TimeCompression_LampRecord(Event @event, ScriptAccessory accessory)
        {
            //0085 Purple
            //0086 Yellow
            if (parse != 3.1) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var dir8= PositionTo8Dir(pos, new(100, 0, 100));
            lock (P3Lamp)
            {
                P3Lamp[dir8] = @event["Id"] == "0086" ? 1 : 2;
            }
        }
        [ScriptMethod(name: "P3_TimeCompression_LampWiseRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970"],userControl:false)]
        public void P3_TimeCompression_LampWiseRecord(Event @event, ScriptAccessory accessory)
        {
            //buff2970, 13 269 clockwise 92 348 counterclockwise
            if (parse != 3.1) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
            Vector3 centre = new(100, 0, 100);
            var dir8 = PositionTo8Dir(pos, centre);
            P3LampWise[dir8] = @event["StackCount"] == "92" ? 1 : 0;
        }
        [ScriptMethod(name: "P3_TimeCompression_LampAOE", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40235", "TargetIndex:1"])]
        public void P3_TimeCompression_LampAOE(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.1) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var rot= JsonConvert.DeserializeObject<float>(@event["SourceRotation"]);
            Vector3 centre = new(100, 0, 100);
            var dir8 = PositionTo8Dir(pos, centre);
            var isWise = P3LampWise[dir8] == 1;
            for (int i = 0; i < 9; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_LampAOE";
                dp.Scale = new(5,50);
                dp.Position = pos;
                dp.Rotation = rot + (i + 1) * float.Pi / 12 * (isWise ? -1 : 1);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 2000+(i*1000);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }
        [ScriptMethod(name: "P3_TimeCompression_BuffPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40293"])]
        public void P3_TimeCompression_BuffPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.1) return;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myIndex == -1) return;
            var myDir8 = MyLampIndex(myIndex);
            //accessory.Log.Debug($"myDir8 {myDir8}");
            if (myDir8 == -1) return;
            var myRot = myDir8 * float.Pi / 4;

            Vector3 centre = new(100, 0, 100);
            Vector3 fireN = new(100, 0, 84.5f);
            Vector3 returnPosN = P3WaterBuff[myIndex] == 2 ? new(100, 0, 91.5f) : new(100, 0, 98);
            Vector3 stopPos = new(100, 0, 101);
            //Fire
            var myFire = P3FireBuff[myIndex];
            //Short Fire
            if (myFire == 1)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_ShortFire_PlaceFire";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(fireN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_ShortFire_PlaceReturn";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 7500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_ShortFire_CenterStack";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 12500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_ShortFire_OutputPosition";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 22500;
                dp.DestoryAt = 15000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            //Medium Fire
            if (myFire == 2)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_MediumFire_CenterStack";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_MediumFire_PlaceReturn";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 7500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_MediumFire_PlaceFire";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(fireN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 12500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_MediumFire_Center";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition =centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 17500;
                dp.DestoryAt = 10000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_MediumFire_OutputPosition";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 32500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            }

            //Long Fire
            if (myFire == 3)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_LongFire_CenterStack";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_LongFire_CenterStack";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 12500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_LongFire_Return";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 17500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_LongFire_PlaceFire";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(fireN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 22500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_LongFire_Output";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 27500;
                dp.DestoryAt = 10000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            if (myFire == 4)
            {
                if (myIndex <4)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_TimeCompression_IceTH_PlaceIce";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_TimeCompression_IceTH_PlaceReturn";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 7500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_TimeCompression_IceTH_CenterStack";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 12500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_TimeCompression_IceTH_OutputPosition";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 22500;
                    dp.DestoryAt = 15000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                else
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_TimeCompression_IceD_CenterStack";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_TimeCompression_IceD_CenterStack";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 12500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_TimeCompression_IceD_Return";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 17500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_TimeCompression_IceD_PlaceIce";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 22500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_TimeCompression_LongFire_Output";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 27500;
                    dp.DestoryAt = 10000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }
        }
        [ScriptMethod(name: "P3_TimeCompression_LampPosition", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970"])]
        public void P3_TimeCompression_LampPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.1) return;
            //buff2970, 13 269 clockwise 92 348 counterclockwise
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
            Vector3 centre = new(100, 0, 100);
            var myIndex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var dir8 = PositionTo8Dir(pos, centre);
            Vector3 nPos = @event["StackCount"] == "92" ? new(98, 0, 90) : new(102, 0, 90);
            if (dir8 == MyLampIndex(myIndex))
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_TimeCompression_LampPosition";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(nPos,centre,dir8*float.Pi/4);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }

        [ScriptMethod(name: "P3_TimeCompression_BreakShieldGatherAlert", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40286"])]
        public void P3_TimeCompression_BreakShieldGatherAlert(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.1) return;
            accessory.Method.TextInfo("Gather center",3000);
            accessory.Method.TTS("Gather center");
        }
        [ScriptMethod(name: "P3_TimeCompression_DarkHalo", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40290"])]
        public void P3_TimeCompression_DarkHalo(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_TimeCompression_DarkHalo";
            dp.Scale = new(20);
            dp.Owner = sid;
            dp.TargetObject = tid;
            dp.Color = myindex == 0 || myindex == 1 ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }


        [ScriptMethod(name: "P3_DelayedChantEcho_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40269)$"], userControl: false)]
        public void P3_DelayedChantEcho_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 3.2d;
            P3FloorFire = -1;
        }
        [ScriptMethod(name: "P3_DelayedChantEcho_FloorFire", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:4", "Id2:regex:^(16|64)$"])]
        public void P3_DelayedChantEcho_FloorFire(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.2) return;
            lock (this)
            {
                if (P3FloorFireDone) return;
                P3FloorFireDone = true;
            }
            Vector3 centre = new(100, 0, 100);
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var clockwise = @event["Id2"] == "64" ? -1 : 1;
            var preTime = 100;
            //Interval 11 2 2 2 2 2

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_Center";
            dp.Scale = new(9);
            dp.Position = centre;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 9700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_StartPoint_11";
            dp.Scale = new(9);
            dp.Position = pos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 12000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_StartPoint_12";
            dp.Scale = new(9);
            dp.Position = pos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 17000 - preTime;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_StartPoint_21";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 12000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_StartPoint_22";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 17000 - preTime;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_SecondPoint_11";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * clockwise);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 14000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_SecondPoint_12";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * clockwise);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 19000 - preTime;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_SecondPoint_21";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * clockwise + float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 14000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_SecondPoint_22";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * clockwise + float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 19000 - preTime;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_ThirdPoint_11";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 2 * clockwise);
            dp.Color = (accessory.Data.DefaultDangerColor + accessory.Data.DefaultSafeColor) / 2;
            dp.Delay = 3000;
            dp.DestoryAt = 8000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_ThirdPoint_12";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 2 * clockwise);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 11000 - preTime;
            dp.DestoryAt = 8000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_ThirdPoint_21";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 2 * clockwise + float.Pi);
            dp.Color = (accessory.Data.DefaultDangerColor + accessory.Data.DefaultSafeColor) / 2;
            dp.Delay = 3000;
            dp.DestoryAt = 8000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_ThirdPoint_22";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 2 * clockwise + float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 11000 - preTime;
            dp.DestoryAt = 8000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_FourthPoint_11";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * 3 * clockwise);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 15000 - preTime;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SecondMechanic_FloorFire_FourthPoint_21";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * 3 * clockwise + float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 15000 - preTime;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "P3_DelayedChantEcho_ShatterSoul", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40288"])]
        public void P3_DelayedChantEcho_ShatterSoul(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.2) return;
            for (int i = 0; i < 8; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_DelayedChantEcho_ShatterSoul";
                dp.Scale = new(5);
                dp.Owner = accessory.Data.PartyList[i];
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 1000;
                dp.DestoryAt = 2500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }
        [ScriptMethod(name: "P3_DelayedChantEcho_KnockbackAlert", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40182", "TargetIndex:1"])]
        public void P3_DelayedChantEcho_KnockbackAlert(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedChantEcho_KnockbackAlert1";
            dp.Scale = new(2,21);
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = sid;
            dp.Rotation = float.Pi;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedChantEcho_KnockbackAlert2";
            dp.Scale = new(2);
            dp.Owner = sid;
            dp.TargetObject = accessory.Data.Me;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P3_DelayedChantEcho_FloorFireRecord", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:4", "Id2:regex:^(16|64)$"],userControl:false)]
        public void P3_DelayedChantEcho_FloorFireRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.2) return;
            lock (this)
            {
                if (P3FloorFire != -1) return;
                Vector3 centre = new(100, 0, 100);
                var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                P3FloorFire = PositionTo8Dir(pos,new(100,0,100));
                P3FloorFire+= @event["Id2"] == "64" ? 10 : 20;
            }
            
        }
        [ScriptMethod(name: "P3_DelayedChantEcho_TGuidePosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40181"])]
        public void P3_DelayedChantEcho_TGuidePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.2) return;
            if (!P3JumpPosition) return;
            var me= accessory.Data.Objects.SearchById(accessory.Data.Me);
            if(me == null) return;
            accessory.Method.TextInfo("Guide at edge", 3000);
            accessory.Method.TTS("Guide at edge");
            var dir8 = P3FloorFire % 10 % 4;
            Vector3 posN = new(100, 0, 86);
            var rot = dir8 switch
            {
                0 => 6,
                1 => 7,
                2 => 0,
                3 => 5
            };
            var pos1 = RotatePoint(posN, new(100, 0, 100), float.Pi / 4 * rot);
            var pos2 = RotatePoint(posN, new(100, 0, 100), float.Pi / 4 * rot+float.Pi);
            var dealpos = (pos1 - me.Position).Length() < (pos2 - me.Position).Length() ? pos1 : pos2;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedChantEcho_TGuidePosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.TargetPosition = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        }


        private int MyLampIndex(int myPartyIndex)
        {
            var nLampIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                if (P3Lamp[i] == 1 && P3Lamp[(i+3)%8]==1 && P3Lamp[(i + 5) % 8] == 1)
                {
                    nLampIndex=i;
                    break;
                }
            }
            if (P3LampDeal==P3LampEmum.MGL)
            {
                //Short Fire
                if (P3FireBuff[myPartyIndex] == 1)
                {
                    if (myPartyIndex<4)
                    {
                        return (nLampIndex + 4) % 8;
                    }
                    else
                    {
                        var lowIndex = P3FireBuff.LastIndexOf(1);
                        if (lowIndex != myPartyIndex)
                        {
                            return (nLampIndex + 7) % 8;
                        }
                        else
                        {
                            return (nLampIndex + 1) % 8;
                        }
                    }
                    
                }
                //Medium Fire
                if (P3FireBuff[myPartyIndex] == 2)
                {
                    if (myPartyIndex < 4) return (nLampIndex + 6) % 8;
                    else return (nLampIndex + 2) % 8;
                }
                //Long Fire
                if (P3FireBuff[myPartyIndex] == 3)
                {
                    if (myPartyIndex<4)
                    {
                        var highIndex = P3FireBuff.IndexOf(3);
                        if (highIndex == myPartyIndex)
                        {
                            return (nLampIndex + 5) % 8;
                        }
                        else
                        {
                            return (nLampIndex + 3) % 8;
                        }
                    }
                    else
                    {
                        return (nLampIndex + 0) % 8;
                    }
                    
                }
                //Ice
                if (P3FireBuff[myPartyIndex] == 4)
                {
                    if (myPartyIndex < 4) return (nLampIndex + 4) % 8;
                    else return (nLampIndex + 0) % 8;
                }
            }

            return -1;
        }
        #endregion

        #region P4

       
        [ScriptMethod(name: "P4_Manifestation_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40246"], userControl: false)]
        public void P4_Manifestation_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 4.1d;
        }
        [ScriptMethod(name: "P4_TimeCrystal_MemoryCrystalCollection", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40174"], userControl: false)]
        public void P4_TimeCrystal_MemoryCrystalCollection(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            P4FragmentId = sid;
        }
        [ScriptMethod(name: "P4_Manifestation_HeavenlyCycle", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40237"])]
        public void P4_Manifestation_HeavenlyCycle(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Manifestation_HeavenlyCycle";
            dp.Scale = new(4);
            dp.Owner = sid;
            dp.TargetObject = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P4_Manifestation_HeavenlyCycleGatherAlert", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40246"])]
        public void P4_Manifestation_HeavenlyCycleGatherAlert(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("Gather", 9500);
            accessory.Method.TTS("Gather");
        }
        [ScriptMethod(name: "P4_Manifestation_HeavenlyCycleAvoidAlert", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40186"])]
        public void P4_Manifestation_HeavenlyCycleAvoidAlert(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("Run", 3000);
            accessory.Method.TTS("Run");
        }

        [ScriptMethod(name: "P4_DarkLightDragonSong_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40239"], userControl: false)]
        public void P4_DarkLightDragonSong_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 4.2d;
            P4Tether = [-1, -1, -1, -1, -1, -1, -1, -1];
            P4Stack = [0, 0, 0, 0, 0, 0, 0, 0];
            P4TetherDone = false;
        }
        [ScriptMethod(name: "P4_DarkLightDragonSong_BuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2461"], userControl: false)]
        public void P4_DarkLightDragonSong_BuffRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var tIndex = accessory.Data.PartyList.IndexOf(tid);
            P4Stack[tIndex] = 1;
        }
        [ScriptMethod(name: "P4_DarkLightDragonSong_TetherCollection", eventType: EventTypeEnum.Tether, eventCondition: ["Id:006E"], userControl: false)]
        public void P4_DarkLightDragonSong_TetherCollection(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var sIndex = accessory.Data.PartyList.IndexOf(sid);
            var tIndex = accessory.Data.PartyList.IndexOf(tid);
            P4Tether[sIndex] = tIndex;
        }
        [ScriptMethod(name: "P4_DarkLightDragonSong_GuideCone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40187"])]
        public void P4_DarkLightDragonSong_GuideCone(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            for (uint i = 1; i < 5; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_DarkLightDragonSong_GuideCone";
                dp.Scale = new(20);
                dp.Radian = float.Pi / 3;
                dp.Owner = sid;
                dp.TargetResolvePattern=PositionResolvePatternEnum.PlayerNearestOrder;
                dp.TargetOrderIndex = i;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 4000;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
        }
        [ScriptMethod(name: "P4_DarkLightDragonSong_ShatterSoul", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40187"])]
        public void P4_DarkLightDragonSong_ShatterSoul(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.2) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarkLightDragonSong_ShatterSoul_Crystal";
            dp.Scale = new(8.5f);
            dp.Owner = P4FragmentId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            for (int i = 0; i < 8; i++)
            {
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_DarkLightDragonSong_ShatterSoul";
                dp.Scale = new(5);
                dp.Owner = accessory.Data.PartyList[i];
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 3500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }
        [ScriptMethod(name: "P4_DarkLightDragonSong_HolyWings", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[78])$"])]
        public void P4_DarkLightDragonSong_HolyWings(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarkLightDragonSong_HolyWings";
            dp.Scale = new(40,20);
            dp.Owner = sid;
            dp.Rotation = @event["ActionId"] == "40227" ? float.Pi / 2 : float.Pi / -2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P4_DarkLightDragonSong_WaterStack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[78])$"])]
        public void P4_DarkLightDragonSong_WaterStack(Event @event, ScriptAccessory accessory)
        {
            var tIndex = P4Tether[0] == -1 ? 1 : 0;
            var nIndex = P4Tether[2] == -1 ? 3 : 2;
            var d1Index = -1;
            var d2Index = -1;
            List<int> upGroup = [];
            List<int> downGroup = [];
            for (int i = 4; i < 7; i++)
            {
                for (int j = i + 1; j < 8; j++)
                {
                    if (P4Tether[i] != -1 && P4Tether[j] != -1)
                    {
                        d1Index = i;
                        d2Index = j;
                    }
                }
            }
            // t tether high d low d butterfly
            if ((P4Tether[tIndex] == d1Index && P4Tether[d2Index] == tIndex) || (P4Tether[tIndex] == d2Index && P4Tether[d1Index] == tIndex))
            {
                upGroup.Add(tIndex);
                upGroup.Add(nIndex);
                downGroup.Add(d1Index);
                downGroup.Add(d2Index);
            }
            // t tether high d n square
            if ((P4Tether[tIndex] == d1Index && P4Tether[nIndex] == tIndex) || (P4Tether[d1Index] == tIndex && P4Tether[tIndex] == nIndex))
            {
                upGroup.Add(d1Index);
                upGroup.Add(nIndex);
                downGroup.Add(tIndex);
                downGroup.Add(d2Index);
            }
            // t tether low d n hourglass
            if ((P4Tether[tIndex] == d2Index && P4Tether[nIndex] == tIndex) || (P4Tether[d2Index] == tIndex && P4Tether[tIndex] == nIndex))
            {
                upGroup.Add(tIndex);
                upGroup.Add(d1Index);
                downGroup.Add(nIndex);
                downGroup.Add(d2Index);
            }

            var stack1 = P4Stack.IndexOf(1);
            var stack2 = P4Stack.LastIndexOf(1);
            var tetherStack = P4Tether[stack1] == -1 ? stack2 : stack1;
            var idleStack= P4Tether[stack1] == -1 ? stack1 : stack2;

            List<int> idles = [];
            for (int i = 0; i < 8; i++)
            {
                if (P4Tether[i] == -1)
                {
                    idles.Add(i);
                }
            }
            var ii = idles.IndexOf(idleStack);

            if (upGroup.Contains(tetherStack))
            {
                //Tether stack on top
                if (ii==0||ii==2)
                {
                    downGroup.Add(idles[0]);
                    downGroup.Add(idles[2]);
                    upGroup.Add(idles[1]);
                    upGroup.Add(idles[3]);
                }
                if (ii == 1 || ii == 3)
                {
                    downGroup.Add(idles[1]);
                    downGroup.Add(idles[3]);
                    upGroup.Add(idles[0]);
                    upGroup.Add(idles[2]);
                }
            }
            if (downGroup.Contains(tetherStack))
            {
                //Tether stack on bottom
                if (ii == 0 || ii == 2)
                {
                    upGroup.Add(idles[0]);
                    upGroup.Add(idles[2]);
                    downGroup.Add(idles[1]);
                    downGroup.Add(idles[3]);
                }
                if (ii == 1 || ii == 3)
                {
                    upGroup.Add(idles[1]);
                    upGroup.Add(idles[3]);
                    downGroup.Add(idles[0]);
                    downGroup.Add(idles[2]);
                }
            }

            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarkLightDragonSong_Stack";
            dp.Scale = new(6);
            dp.Owner = accessory.Data.PartyList[tetherStack];
            dp.Color = upGroup.Contains(tetherStack)==upGroup.Contains(myindex)?accessory.Data.DefaultSafeColor: accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarkLightDragonSong_Stack";
            dp.Scale = new(6);
            dp.Owner = accessory.Data.PartyList[idleStack];
            dp.Color = upGroup.Contains(idleStack) == upGroup.Contains(myindex) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarkLightDragonSong_Stack_Crystal";
            dp.Scale = new(9.5f);
            dp.Owner = P4FragmentId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P4_DarkLightDragonSong_EndlessInsight", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40249"])]
        public void P4_DarkLightDragonSong_EndlessInsight(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarkLightDragonSong_EndlessInsight";
            dp.Scale = new(4);
            dp.Owner =sid;
            dp.CentreResolvePattern=PositionResolvePatternEnum.OwnerTarget;
            dp.Color =accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        #region FarNearJump
        //[ScriptMethod(name: "P4_DarkLightDragonSong_FarNearJump", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40283"])]
        //public void P4_DarkLightDragonSong_FarNearJump(Event @event, ScriptAccessory accessory)
        //{
        //    if (parse != 4.2) return;
        //    if (!ParseObjectId(@event["SourceId"], out var sid)) return;

        //    var dp = accessory.Data.GetDefaultDrawProperties();
        //    dp.Name = "P4_DarkLightDragonSong_FarJump";
        //    dp.Scale = new(8);
        //    dp.Owner = sid;
        //    dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
        //    dp.Color = accessory.Data.DefaultDangerColor;
        //    dp.DestoryAt = 5000;
        //    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        //    dp = accessory.Data.GetDefaultDrawProperties();
        //    dp.Name = "P4_DarkLightDragonSong_NearJump";
        //    dp.Scale = new(8);
        //    dp.Owner = sid;
        //    dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
        //    dp.Color = accessory.Data.DefaultDangerColor;
        //    dp.Delay = 5000;
        //    dp.Delay = 3500;
        //    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        //}
        #endregion
        [ScriptMethod(name: "P4_DarkLightDragonSong_TowerPosition", eventType: EventTypeEnum.Tether, eventCondition: ["Id:006E"])]
        public void P4_DarkLightDragonSong_TowerPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.2) return;
            
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (sid != accessory.Data.Me) return;
            //accessory.Log.Debug("Line");
            Task.Delay(50).ContinueWith(t =>
            {
                var tIndex = P4Tether[0] == -1 ? 1 : 0;
                var nIndex = P4Tether[2] == -1 ? 3 : 2;
                var d1Index = -1;
                var d2Index = -1;
                List<int> upGroup = [];
                List<int> downGroup = [];
                for (int i = 4; i < 7; i++)
                {
                    for (int j = i + 1; j < 8; j++)
                    {
                        if (P4Tether[i] != -1 && P4Tether[j] != -1)
                        {
                            d1Index = i;
                            d2Index = j;
                        }
                    }
                }
                // t tether high d low d butterfly
                if ((P4Tether[tIndex] == d1Index && P4Tether[d2Index] == tIndex) || (P4Tether[tIndex] == d2Index && P4Tether[d1Index] == tIndex))
                {
                    upGroup.Add(tIndex);
                    upGroup.Add(nIndex);
                    downGroup.Add(d1Index);
                    downGroup.Add(d2Index);
                }
                // t tether high d n square
                if ((P4Tether[tIndex] == d1Index && P4Tether[nIndex] == tIndex) || (P4Tether[d1Index] == tIndex && P4Tether[tIndex] == nIndex))
                {
                    upGroup.Add(d1Index);
                    upGroup.Add(nIndex);
                    downGroup.Add(tIndex);
                    downGroup.Add(d2Index);
                }
                // t tether low d n hourglass
                if ((P4Tether[tIndex] == d2Index && P4Tether[nIndex] == tIndex) || (P4Tether[d2Index] == tIndex && P4Tether[tIndex] == nIndex))
                {
                    upGroup.Add(tIndex);
                    upGroup.Add(d1Index);
                    downGroup.Add(nIndex);
                    downGroup.Add(d2Index);
                }

                var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                Vector3 dealpos = upGroup.Contains(myIndex) ? new(100, 0, 92) : new(100, 0, 108);

                var dur = 10000;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_DarkLightDragonSong_TowerPosition";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = dur;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_DarkLightDragonSong_TowerPosition";
                dp.Scale = new(4);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = dur;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
            });
            
        }
        [ScriptMethod(name: "P4_DarkLightDragonSong_GuidePosition", eventType: EventTypeEnum.Tether, eventCondition: ["Id:006E"])]
        public void P4_DarkLightDragonSong_GuidePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.2) return;
            lock (this)
            {
                if (P4TetherDone) return;
                P4TetherDone = true;
            }
            Task.Delay(50).ContinueWith(t =>
            {
                List<int> idles = [];
                for (int i = 0; i < 8; i++)
                {
                    if (P4Tether[i] == -1)
                    {
                        idles.Add(i);
                    }
                }
                var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                if (!idles.Contains(myIndex)) return;
                Vector3 dealpos = idles.IndexOf(myIndex) switch
                {
                    0 => new(095.8f, 0, 098.0f),
                    1 => new(104.2f, 0, 098.0f),
                    2 => new(095.8f, 0, 102.0f),
                    3 => new(104.2f, 0, 102.0f),
                };

                var dur = 10000;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_DarkLightDragonSong_GuidePosition";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = dur;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            });

        }
        [ScriptMethod(name: "P4_DarkLightDragonSong_StackPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[78])$"])]
        public void P4_DarkLightDragonSong_StackPosition(Event @event, ScriptAccessory accessory)
        {
            var tIndex = P4Tether[0] == -1 ? 1 : 0;
            var nIndex = P4Tether[2] == -1 ? 3 : 2;
            var d1Index = -1;
            var d2Index = -1;
            List<int> upGroup = [];
            List<int> downGroup = [];
            for (int i = 4; i < 7; i++)
            {
                for (int j = i + 1; j < 8; j++)
                {
                    if (P4Tether[i] != -1 && P4Tether[j] != -1)
                    {
                        d1Index = i;
                        d2Index = j;
                    }
                }
            }
            // t tether high d low d butterfly
            if ((P4Tether[tIndex] == d1Index && P4Tether[d2Index] == tIndex) || (P4Tether[tIndex] == d2Index && P4Tether[d1Index] == tIndex))
            {
                upGroup.Add(tIndex);
                upGroup.Add(nIndex);
                downGroup.Add(d1Index);
                downGroup.Add(d2Index);
            }
            // t tether high d n square
            if ((P4Tether[tIndex] == d1Index && P4Tether[nIndex] == tIndex) || (P4Tether[d1Index] == tIndex && P4Tether[tIndex] == nIndex))
            {
                upGroup.Add(d1Index);
                upGroup.Add(nIndex);
                downGroup.Add(tIndex);
                downGroup.Add(d2Index);
            }
            // t tether low d n hourglass
            if ((P4Tether[tIndex] == d2Index && P4Tether[nIndex] == tIndex) || (P4Tether[d2Index] == tIndex && P4Tether[tIndex] == nIndex))
            {
                upGroup.Add(tIndex);
                upGroup.Add(d1Index);
                downGroup.Add(nIndex);
                downGroup.Add(d2Index);
            }

            var stack1 = P4Stack.IndexOf(1);
            var stack2 = P4Stack.LastIndexOf(1);
            var tetherStack = P4Tether[stack1] == -1 ? stack2 : stack1;
            var idleStack = P4Tether[stack1] == -1 ? stack1 : stack2;

            List<int> idles = [];
            for (int i = 0; i < 8; i++)
            {
                if (P4Tether[i] == -1)
                {
                    idles.Add(i);
                }
            }
            var ii = idles.IndexOf(idleStack);

            if (upGroup.Contains(tetherStack))
            {
                //Tether stack on top
                if (ii == 0 || ii == 2)
                {
                    downGroup.Add(idles[0]);
                    downGroup.Add(idles[2]);
                    upGroup.Add(idles[1]);
                    upGroup.Add(idles[3]);
                }
                if (ii == 1 || ii == 3)
                {
                    downGroup.Add(idles[1]);
                    downGroup.Add(idles[3]);
                    upGroup.Add(idles[0]);
                    upGroup.Add(idles[2]);
                }
            }
            if (downGroup.Contains(tetherStack))
            {
                //Tether stack on bottom
                if (ii == 0 || ii == 2)
                {
                    upGroup.Add(idles[0]);
                    upGroup.Add(idles[2]);
                    downGroup.Add(idles[1]);
                    downGroup.Add(idles[3]);
                }
                if (ii == 1 || ii == 3)
                {
                    upGroup.Add(idles[1]);
                    upGroup.Add(idles[3]);
                    downGroup.Add(idles[0]);
                    downGroup.Add(idles[2]);
                }
            }

            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            Vector3 dealpos = new(@event["ActionId"] == "40227" ? 105 : 95, 0, upGroup.Contains(myindex) ? 92.5f : 107.5f);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarkLightDragonSong_StackPosition";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


        }

        [ScriptMethod(name: "P4_TimeCrystal_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40240"], userControl: false)]
        public void P4_TimeCrystal_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 4.3d;
            P4ClawBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            P4OtherBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            P4WhiteCirclePos = [];
            P4WaterPos = [];
        }
        [ScriptMethod(name: "P4_TimeCrystal_BuffCollection", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(326[34]|2454|246[0123])$"], userControl: false)]
        public void P4_TimeCrystal_BuffCollection(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.3) return;
            var id = @event["StatusID"];
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            //3623 Red Claw 1 short 2 long
            if (id == "3263")
            {
                if (!float.TryParse(@event["Duration"], out float dur)) return;
                P4ClawBuff[index] = dur > 20 ? 2 : 1;
            }

            if (id == "3264")
            {
                P4ClawBuff[index] = 3;
            }
            //Dark 4
            if (id == "2460")
            {
                P4OtherBuff[index] = 4;
            }
            //Water 3
            if (id == "2461")
            {
                P4OtherBuff[index] = 3;
            }
            //Ice 1
            if (id == "2462")
            {
                P4OtherBuff[index] = 1;
            }
            //Wind 2
            if (id == "2463")
            {
                P4OtherBuff[index] = 2;
            }
            //Earth 5
            if (id == "2454")
            {
                P4OtherBuff[index] = 5;
            }
        }
        [ScriptMethod(name: "P4_TimeCrystal_BlueTetherCollection", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0085"], userControl: false)]
        public void P4_TimeCrystal_BlueTetherCollection(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.3) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            P4BlueTether = PositionTo6Dir(pos, new(100, 0, 100)) % 3;
        }
        [ScriptMethod(name: "P4_TimeCrystal_LampAOE", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0085"])]
        public void P4_TimeCrystal_LampAOE(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.3) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            Vector3 normalPos = new(pos.X, 0, 200 - pos.Z);
            Vector3 fastPos = new(100, 0, pos.Z > 100 ? 111 : 89);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_TimeCrystal_LampAOE_Fast";
            dp.Scale = new(12);
            dp.Position = fastPos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_TimeCrystal_LampAOE_Medium";
            dp.Scale = new(12);
            dp.Position = normalPos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 13000 - P4LampDisplayDur;
            dp.DestoryAt = P4LampDisplayDur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_TimeCrystal_LampAOE_Slow";
            dp.Scale = new(12);
            dp.Position = pos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 18000 - P4LampDisplayDur;
            dp.DestoryAt = P4LampDisplayDur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P4_TimeCrystal_EarthStackRange", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2454"])]
        public void P4_TimeCrystal_EarthStackRange(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.3) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_TimeCrystal_EarthStackRange";
            dp.Scale = new(6);
            dp.Owner = tid;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 14000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_TimeCrystal_EarthStackRange_Crystal";
            dp.Scale = new(9.5f);
            dp.Owner = P4FragmentId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 14000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P4_TimeCrystal_ShatterSoul", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2452"])]
        public void P4_TimeCrystal_ShatterSoul(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.3) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_TimeCrystal_ShatterSoul_Crystal";
            dp.Scale = new(8.5f);
            dp.Owner = P4FragmentId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            for (int i = 0; i < 8; i++)
            {
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_ShatterSoul";
                dp.Scale = new(5);
                dp.Owner = accessory.Data.PartyList[i];
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 3500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            
            
        }
        [ScriptMethod(name: "P4_TimeCrystal_BuffPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40293"])]
        public void P4_TimeCrystal_BuffPosition(Event @event, ScriptAccessory accessory)
        {
            
            //buff after 3.5s
            if (parse != 4.3) return;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            //Short Red
            if (P4ClawBuff[myIndex] == 1)
            {
                var isHigh = P4ClawBuff.IndexOf(1) == myIndex;
                Vector3 dealpos= isHigh ? new(87, 0, 100) : new(113, 0, 100);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_BuffPosition_HeadCharge";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 10500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                //High stack 088 085 -> 093 082
                //High idle 081 103 -> 081 097
                //Low stack 112 085 -> 107 082
                //Low idle 119 103 -> 119 97
                Vector3 dealpos2 = isHigh ? (P4BlueTether == 1 ? new(081, 0, 103) : new(088, 0, 085)) : (P4BlueTether == 1 ? new(112, 0, 085) : new(119, 0, 103));
                Vector3 dealpos3 = isHigh ? (P4BlueTether == 1 ? new(081, 0, 097) : new(093, 0, 082)) : (P4BlueTether == 1 ? new(107, 0, 082) : new(119, 0, 097));

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_BuffPosition_pos2PreTether";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 10500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_BuffPosition_pos2Position";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 10500;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_BuffPosition_pos3PreTether";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos2;
                dp.TargetPosition = dealpos3;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 13500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_BuffPosition_pos3Position";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos3;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 13500;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            //Long Red
            if (P4ClawBuff[myIndex] == 2)
            {
                var isHigh = P4ClawBuff.IndexOf(2) == myIndex;
                Vector3 dealpos1 = isHigh ? new(088.5f, 0, 115.5f) : new(111.5f, 0, 115.5f);
                Vector3 dealpos2 = isHigh ? new(090.2f, 0, 117.0f) : new(109.8f, 0, 117.0f);
                Vector3 dealpos3 = isHigh ? new(092.5f, 0, 118.0f) : new(107.5f, 0, 118.0f);
                Vector3 dealpos4 = isHigh ? new(092.0f, 0, 110.0f) : new(108.0f, 0, 110.0f);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_BuffPosition_DodgeAC";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos1;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_BuffPosition_DodgeAC->Knockback";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos1;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_BuffPosition_Knockback";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 7500;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_BuffPosition_Knockback->DodgeDiagonal";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos2;
                dp.TargetPosition = dealpos3;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 10500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_BuffPosition_DodgeDiagonal";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos3;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 10500;
                dp.DestoryAt = 2500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_BuffPosition_DodgeDiagonal->HeadCharge";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos3;
                dp.TargetPosition = dealpos4;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 13000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_BuffPosition_HeadCharge";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 13000;
                dp.DestoryAt = 2000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            //Blue
            if (P4ClawBuff[myIndex] == 3)
            {
                if (P4OtherBuff[myIndex] == 4)
                {
                    Vector3 dealpos1 = P4BlueTether == 1 ? new(112, 0, 85) : new(88, 0, 85);
                    Vector3 dealpos2 = P4BlueTether == 1 ? new(108.7f, 0, 85) : new(091.3f, 0, 85);
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_TimeCrystal_BuffPosition_DodgeLamp1";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos1;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_TimeCrystal_BuffPosition_DodgeLamp->Stack";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Position = dealpos1;
                    dp.TargetPosition = dealpos2;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_TimeCrystal_BuffPosition_DodgeLamp2";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos2;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 7500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                else
                {
                    Vector3 dealpos1 = P4BlueTether == 1 ? new(88, 0, 115) : new(112, 0, 115);
                    Vector3 dealpos2 = P4BlueTether == 1 ? new(090.8f, 0, 116.0f) : new(109.2f, 0, 116.0f);
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_TimeCrystal_BuffPosition_DodgeLampAC";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos1;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_TimeCrystal_BuffPosition_DodgeAC->Knockback";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Position = dealpos1;
                    dp.TargetPosition = dealpos2;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_TimeCrystal_BuffPosition_Knockback";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos2;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 7500;
                    dp.DestoryAt = 3000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }
        }
        [ScriptMethod(name: "P4_TimeCrystal_WhiteCirclePositionIndicator", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40241", "TargetIndex:1"])]
        public void P4_TimeCrystal_WhiteCirclePositionIndicator(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.3) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
            lock (P4WhiteCirclePos)
            {
                P4WhiteCirclePos.Add(pos);
                if (P4WhiteCirclePos.Count == 1 || P4WhiteCirclePos.Count == 3) return;

            }
        }
        [ScriptMethod(name: "P4_TimeCrystal_ReturnPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40251"])]
        public void P4_TimeCrystal_ReturnPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.3) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            P4WaterPos.Add(pos);
            if (P4WaterPos.Count == 1) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 centre = new(100, 0, 100);
            var dir8 = PositionTo8Dir((P4WaterPos[0] + P4WaterPos[1]) / 2, centre)-1;
            Vector3 mtPos = new(107, 0, 88);
            Vector3 stPos = new(112, 0, 93);
            Vector3 mtgPos = new(106, 0, 92);
            Vector3 stgPos = new(108, 0, 94);
            if (myindex==0)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_ReturnPosition_MT";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(mtPos, centre, float.Pi / 4 * dir8);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (myindex == 1)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_ReturnPosition_ST";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(stPos, centre, float.Pi / 4 * dir8);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (myindex == 2 || myindex == 4 || myindex == 6)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_ReturnPosition_MTG";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(mtgPos, centre, float.Pi / 4 * dir8);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (myindex == 3 || myindex == 5 || myindex == 7)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_TimeCrystal_ReturnPosition_STG";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(stgPos, centre, float.Pi / 4 * dir8);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }

        #endregion

        #region P5
        [ScriptMethod(name: "P5_FloorFire", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40118|40307)$"])]
        public void P5_FloorFire(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_FloorFire";
            dp.Scale = new(80, 5);
            dp.Owner = sid;
            dp.Color = P5PathColor.V4.WithW(3);
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P5_FloorFire_Advance_{@event["SourceId"]}";
            dp.Scale = new(80, 5);
            dp.Offset = new(0,0,-5);
            dp.Owner = sid;
            dp.Color = P5PathColor.V4.WithW(3);
            dp.Delay = 7000;
            dp.DestoryAt = 20000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P5_FloorFireRemoval", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(40118|4030[789])$"],userControl:false)]
        public void P5_FloorFireRemoval(Event @event, ScriptAccessory accessory)
        {
            if (!float.TryParse(@event["SourceRotation"],out var rot)) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            Vector3 centre = new(100, 0, 100);
            Vector3 posNext = new(pos.X + 5 * MathF.Sin(rot), 0, pos.Z + 5 * MathF.Cos(rot));
            if ((posNext- centre).Length()>20)
            {
                accessory.Method.RemoveDraw($"P5_FloorFire_Advance_{@event["SourceId"]}");
            }
        }

        [ScriptMethod(name: "P5_LightAndDarkWings", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40313|40233)$"])]
        public void P5_LightAndDarkWings(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var r = 225f;
            var rot = (180 - r / 2) / 180f * float.Pi;
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_LightAndDarkWings";
            dp.Scale = new(20);
            dp.Owner = sid;
            dp.Radian = r / 180 * float.Pi;
            dp.TargetObject = accessory.Data.EnmityList[sid][0];
            dp.Rotation = @event["ActionId"] == "40313" ? rot : -rot;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_LightAndDarkWings_FarNear";
            dp.Scale = new(4);
            dp.Owner = sid;
            dp.CentreResolvePattern = @event["ActionId"] == "40313"? PositionResolvePatternEnum.PlayerFarestOrder: PositionResolvePatternEnum.PlayerNearestOrder;
            dp.Rotation = @event["ActionId"] == "40313" ? rot : -rot;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_LightAndDarkWings";
            dp.Scale = new(20);
            dp.Owner = sid;
            dp.Radian = r / 180 * float.Pi;
            dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
            dp.Rotation = @event["ActionId"] == "40313" ? -rot : rot;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7300;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_LightAndDarkWings_FarNear";
            dp.Scale = new(4);
            dp.Owner = sid;
            dp.CentreResolvePattern = @event["ActionId"] == "40313" ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
            dp.Rotation = @event["ActionId"] == "40313" ? rot : -rot;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7300;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P5_LightAndDarkWings_Reset", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40319"])]
        public void P5_LightAndDarkWings_Reset(Event @event, ScriptAccessory accessory)
        {
            P5MtDone = false;
        }
        [ScriptMethod(name: "P5_LightAndDarkWings_TowerCollection", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375BF", "State:00010004", "Index:regex:^(0000003[012])"])]
        public void P5_LightAndDarkWings_TowerCollection(Event @event, ScriptAccessory accessory)
        {
            P5Tower = @event["Index"];
        }
        [ScriptMethod(name: "P5_LightAndDarkWings_MTGuidePosition", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375BF", "State:00010004", "Index:regex:^(0000003[012])"])]
        public void P5_LightAndDarkWings_MTGuidePosition(Event @event, ScriptAccessory accessory)
        {
            //40313 first left then right first far then near
            //40233 first right then left first near then far
            if (P5MtDone) return;
            P5MtDone = true;
            if (accessory.Data.PartyList.IndexOf(accessory.Data.Me) != 0) return;
            var light = @event["ActionId"] == "40313";
            Vector3 towerPos = P5Tower switch
            {
                "00000032" => new(100, 0, 107),
                "00000031" => new(106.06f, 0, 96.50f),
                "00000030" => new(93.94f, 0, 96.50f)
            };

            Vector3 mtPos1 = RotatePoint(towerPos, new(100, 0, 100), float.Pi);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_LightAndDarkWings_MTGuidePosition1";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = mtPos1;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 2300;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P5_LightAndDarkWings_TGuidePosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40313|40233)$"])]
        public void P5_LightAndDarkWings_TGuidePosition(Event @event, ScriptAccessory accessory)
        {
            //40313 first left then right first far then near
            //40233 first right then left first near then far

            var light = @event["ActionId"] == "40313";
            Vector3 towerPos = P5Tower switch
            {
                "00000032" => new(100, 0, 107),
                "00000031" => new(106.06f, 0, 96.50f),
                "00000030" => new(93.94f, 0, 96.50f)
            };
            var mtRot = 187.5f / 180 * MathF.PI;

            Vector3 mtPos1 = RotatePoint(towerPos, new(100, 0, 100), light ? mtRot : -mtRot);
            Vector3 mtPos2 = light ? new((mtPos1.X - 100) / 7 + 100, 0, (mtPos1.Z - 100) / 7 + 100) : new((mtPos1.X - 100) / 7 * 15 + 100, 0, (mtPos1.Z - 100) / 7 * 15 + 100);
            var stRot = 105f / 180 * MathF.PI;
            Vector3 stPos2 = RotatePoint(mtPos1, new(100, 0, 100), light ? stRot : -stRot);
            Vector3 stPos1= light ? new((stPos2.X - 100) / 7*15 + 100, 0, (stPos2.Z - 100) / 7*15 + 100) : new((stPos2.X - 100) / 7  + 100, 0, (stPos2.Z - 100) / 7 + 100);
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myindex==0)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P5_LightAndDarkWings_MTGuidePosition1";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = mtPos1;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 6900;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P5_LightAndDarkWings_MTGuidePosition2";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = mtPos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 6900;
                dp.DestoryAt = 4500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P5_LightAndDarkWings_MTGuidePosition2Preview";
                dp.Scale = new(2);
                dp.Position = mtPos1;
                dp.TargetPosition = mtPos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.TargetColor= accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 6900;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (myindex==1)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P5_LightAndDarkWings_STGuidePosition1";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = stPos1;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7900;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P5_LightAndDarkWings_STGuidePosition2Preview";
                dp.Scale = new(2);
                dp.Position = stPos1;
                dp.TargetPosition = stPos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.TargetColor = accessory.Data.DefaultSafeColor;
                dp.Delay = 3000;
                dp.DestoryAt = 4900;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P5_LightAndDarkWings_STGuidePosition2";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = stPos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 7900;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }

        #endregion

        private int ParsTargetIcon(string id)
        {
            firstTargetIcon ??= int.Parse(id, System.Globalization.NumberStyles.HexNumber);
            return int.Parse(id, System.Globalization.NumberStyles.HexNumber) - (int)firstTargetIcon;
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
        /// <summary>
        /// Round to nearest
        /// </summary>
        /// <param name="point"></param>
        /// <param name="centre"></param>
        /// <returns></returns>
        private int PositionTo8Dir(Vector3 point, Vector3 centre)
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r = Math.Round(4 - 4 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 8;
            return (int)r;

        }
        private int PositionTo6Dir(Vector3 point, Vector3 centre)
        {
            var r = Math.Round(3 - 3 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 6;
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
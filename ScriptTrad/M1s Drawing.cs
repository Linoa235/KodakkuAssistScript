using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.Draw;
using Dalamud.Utility.Numerics;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using Dalamud.Memory.Exceptions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Dalamud.Bindings.ImGui;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Extensions;


namespace KarlinScriptNamespace
{
    [ScriptType(name: "M1s Drawing", territorys: [1226], guid: "b0656e91-13e3-4aa9-922f-854b2252e0bb", version: "0.0.0.9", author: "Linoa235")]
    public class M1sDraw
    {
        [UserSetting("Floor Repair Knockback, Mt Group Safe Half")]
        public KnockBackMtPosition MtSafeFloor { get; set; }

        [UserSetting("Clone Jump Prediction Color")]
        public ScriptColor jumpColor { get; set; } = new() { V4 = new(1, 1, 0, 1) };

        public enum KnockBackMtPosition
        {
            NorthHalf,
            SouthHalf,
            EastHalf,
            WestHalf
        }

        int? firstTargetIcon = null;
        List<int> FloorBrokeList = new ();
        uint copyCatTarget;
        uint parse;
        List<uint> P3TetherTarget = new();
        List<string> P3JumpSkill = new();
        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(@".*");
            //accessory.Method.MarkClear();

            firstTargetIcon = null;
            parse = 1;
            P3TetherTarget = new();
            P3JumpSkill = new();

        }
        [ScriptMethod(name: "Phase Split", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(38036|37963)$"],userControl:false)]
        public void PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse++;
        }
        [ScriptMethod(name: "Cone Guide", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37948"])]
        public void ConeGuide(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var Interval = 1000;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-1-1";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);



            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-1-2";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;

            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-1-3";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;

            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-1-4";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;

            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-2-1";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);



            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-2-2";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;

            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-2-3";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;

            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-2-4";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;

            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Cone Guide Second Stage", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37952"])]
        public void ConeGuideSecondStage(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;


            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide Second Stage";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 1500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Left/Right Cleave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3794[3467]$"])]
        public void LeftRightCleave(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var idStr = @event["ActionId"];
            var isfast = (idStr == "37943" || idStr == "37947");
            var isleft = (idStr == "37947" || idStr == "37944");

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"LeftRightCleave-{(isleft ? "Left" : "Right")}{(isfast ? "Fast" : "Slow")}";
            dp.Scale = new(40,20);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
            dp.Owner = sid;
            dp.Rotation = isleft ? float.Pi / 2 : float.Pi / -2;
            dp.Delay = isfast ? 0 : 6000;
            dp.DestoryAt = isfast ? 6000 : 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Clone Left/Right Cleave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37989|3799[023])$"])]
        public void CloneLeftRightCleave(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var idStr = @event["ActionId"];
            var isfast = (idStr == "37989" || idStr == "37993");
            var isleft = (idStr == "37993" || idStr == "37990");

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Clone LeftRightCleave-{(isleft ? "Left" : "Right")}{(isfast ? "Fast" : "Slow")}";
            dp.Scale = new(100);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
            dp.Owner = sid;
            dp.Rotation = isleft ? float.Pi / 2 : float.Pi / -2;
            dp.Delay = isfast ? 0 : 6000;
            dp.DestoryAt = isfast ? 6000 : 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Jump Left/Right Cleave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3796[5678]$"])]
        public void JumpLeftRightCleave(Event @event, ScriptAccessory accessory)
        {
            //37965 left jump right cleave
            //37966 left jump left cleave
            //37967 right jump right cleave
            //37968 right jump left cleave
            var actionId = @event["ActionId"];
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var leftJump = (actionId == "37965" || actionId == "37966");
            var leftFast= (actionId == "37966" || actionId == "37968");
            Vector3 dv;
            if (leftJump) dv = new(-10, 0, 0);
            else dv = new(10, 0, 0);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"JumpLeftRightCleave-{(leftJump?"Left":"Right")}Jump{(leftFast ? "Left" : "Right")}CleaveFast";
            dp.Position=pos+dv;
            dp.Scale = new(60,30);
            dp.Rotation = leftFast? float.Pi / -2: float.Pi / 2;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3); ;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"JumpLeftRightCleave-{(leftJump ? "Left" : "Right")}Jump{(leftFast ? "Right" : "Left")}CleaveSlow";
            dp.Position = pos + dv;
            dp.Scale = new(60,30);
            dp.Rotation = leftFast ? float.Pi / 2 : float.Pi / -2;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3); ;
            dp.Delay = 7000;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);


        }
        [ScriptMethod(name: "Jump Cone Guide", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(38959|37975)$"])]
        public void JumpConeGuide(Event @event, ScriptAccessory accessory)
        {
            //38959 right cone
            //37975 left cone
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            Vector3 dv = @event["ActionId"] == "37975" ? new(-10, 0, 0) : new(10, 0, 0);
            if (Math.Abs(pos.X-110)<1)
            {
                dv = @event["ActionId"] == "37975" ? new(0, 0, 10) : new(0, 0, -10);
            }
            if (Math.Abs(pos.X - 90) < 1)
            {
                dv = @event["ActionId"] == "37975" ? new(0, 0, -10) : new(0, 0, 10);
            }
            var Interval = 1000;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-1-1";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos+dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Delay = 0;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);



            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-1-2";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos + dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Delay = 0;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-1-3";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos + dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Delay = 0;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-1-4";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos + dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Delay = 0;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-2-1";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos + dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);



            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-2-2";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos + dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-2-3";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos + dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide-2-4";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos + dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }

        [ScriptMethod(name: "Jump Cone Guide Second Stage", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37980"])]
        public void JumpConeGuideSecondStage(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;


            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Cone Guide Second Stage";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 1500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }


        [ScriptMethod(name: "P3 Tether Collection", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0066"], userControl: false)]
        public void P3TetherCollection(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            P3TetherTarget.Add(sid);
        }
        [ScriptMethod(name: "P3 Jump Skill Collection", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(38959|37975|3796[5678])$"])]
        public void P3JumpSkillCollection(Event @event, ScriptAccessory accessory)
        {
            //38959 right cone
            //37975 left cone
            //37965 left jump right cleave
            //37966 left jump left cleave
            //37967 right jump right cleave
            //37968 right jump left cleave
            if (parse != 3) return;
            P3JumpSkill.Add(@event["ActionId"]);
        }

        [ScriptMethod(name: "P3 Clone Tether Skill", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0066"])]
        public void P3CloneTetherSkill(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            Task.Delay(100).ContinueWith((t) =>
            {
                //38959 right cone
                //37975 left cone
                //37965 left jump right cleave
                //37966 left jump left cleave
                //37967 right jump right cleave
                //37968 right jump left cleave
                if (P3TetherTarget.Count < 3) return;
                var skillId= P3JumpSkill[P3TetherTarget.IndexOf(sid)];
                
                if(skillId == "38959" || skillId == "37975")
                {
                    var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                    var leftJump = (skillId == "37975");
                    var isNorthCopy = Math.Abs(pos.Z - 95) < 1;
                    Vector3 dv=new(0,0,0);
                    if (isNorthCopy)
                    {
                        dv = leftJump ? new(10, 0, 0) : new(-10, 0, 0);
                    }
                    else
                    {
                        dv = leftJump ? new(-10, 0, 0) : new(10, 0, 0);
                    }
                    
                    var Interval = 1000;

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"CloneJumpCone-{(isNorthCopy ? "North" : "South")}Clone{(leftJump ? "Left" : "Right")}JumpGuide-1-1";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 1;
                    dp.Delay = 0;
                    dp.DestoryAt = 17000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);



                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"CloneJumpCone-{(isNorthCopy ? "North" : "South")}Clone{(leftJump ? "Left" : "Right")}JumpGuide-1-2";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 2;
                    dp.Delay = 0;
                    dp.DestoryAt = 17000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"CloneJumpCone-{(isNorthCopy ? "North" : "South")}Clone{(leftJump ? "Left" : "Right")}JumpGuide-1-3";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 3;
                    dp.Delay = 0;
                    dp.DestoryAt = 17000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"CloneJumpCone-{(isNorthCopy ? "North" : "South")}Clone{(leftJump ? "Left" : "Right")}JumpGuide-1-4";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 4;
                    dp.Delay = 0;
                    dp.DestoryAt = 17000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"CloneJumpCone-{(isNorthCopy ? "North" : "South")}Clone{(leftJump ? "Left" : "Right")}JumpGuide-2-1";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 1;
                    dp.Delay = 17000 + Interval;
                    dp.DestoryAt = 3000 - Interval;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);



                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"CloneJumpCone-{(isNorthCopy ? "North" : "South")}Clone{(leftJump ? "Left" : "Right")}JumpGuide-2-2";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 2;
                    dp.Delay = 17000 + Interval;
                    dp.DestoryAt = 3000 - Interval;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"CloneJumpCone-{(isNorthCopy ? "North" : "South")}Clone{(leftJump ? "Left" : "Right")}JumpGuide-2-3";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 3;
                    dp.Delay = 17000 + Interval;
                    dp.DestoryAt = 3000 - Interval;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"CloneJumpCone-{(isNorthCopy ? "North" : "South")}Clone{(leftJump ? "Left" : "Right")}JumpGuide-2-4";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 4;
                    dp.Delay = 17000 + Interval;
                    dp.DestoryAt = 3000 - Interval;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                }
                if(skillId == "37965" || skillId == "37966" || skillId == "37967" || skillId == "37968")
                {
                    var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                    var leftJump = (skillId == "37965" || skillId == "37966");
                    var leftFast = (skillId == "37966" || skillId == "37968");
                    var isNorthCopy = Math.Abs(pos.Z - 95) < 1;
                    Vector3 dv = new(0, 0, 0);
                    
                    if (isNorthCopy)
                    {
                        dv = leftJump ? new(10, 0, 0) : new(-10, 0, 0);
                    }else
                    {
                        dv = leftJump ? new(-10, 0, 0) : new(10, 0, 0);
                    }

                    var rotation= leftFast ? float.Pi / -2 : float.Pi / 2;
                    rotation += isNorthCopy ? float.Pi : 0;

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"JumpLeftRightCleave-{(isNorthCopy ? "North" : "South")}Clone{(leftJump ? "Left" : "Right")}Jump{(leftFast ? "Left" : "Right")}CleaveFast";
                    dp.Position = pos + dv;
                    dp.Scale = new(60,30);
                    dp.Rotation = rotation;
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(3); ;
                    dp.DestoryAt = 16000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"JumpLeftRightCleave-{(isNorthCopy ? "North" : "South")}Clone{(leftJump ? "Left" : "Right")}Jump{(leftFast ? "Left" : "Right")}CleaveSlow";
                    dp.Position = pos + dv;
                    dp.Scale = new(60, 30);
                    dp.Rotation = -rotation;
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(3); ;
                    dp.Delay = 16000;
                    dp.DestoryAt = 2000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }

            });
        }

        [ScriptMethod(name: "P3 Clone Facing", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0066"])]
        public void P3CloneFacing(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            Task.Delay(100).ContinueWith((t) =>
            {
                //38959 right cone
                //37975 left cone
                //37965 left jump right cleave
                //37966 left jump left cleave
                //37967 right jump right cleave
                //37968 right jump left cleave
                if (P3TetherTarget.Count < 3) return;
                var skillId = P3JumpSkill[P3TetherTarget.IndexOf(sid)];
                var leftJump = (skillId == "37965" || skillId == "37966" || skillId == "37975");

                var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                var isNorthCopy = Math.Abs(pos.Z - 95) < 1;
                Vector3 dv = new(0, 0, 0);
                if (isNorthCopy)
                {
                    dv = leftJump ? new(10, 0, 0) : new(-10, 0, 0);
                }
                else
                {
                    dv = leftJump ? new(-10, 0, 0) : new(10, 0, 0);
                }
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Clone Facing";
                dp.Scale = new(4.9f, 5f);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Position = pos + dv;
                dp.Rotation = pos.Z > 100 ? 0 : float.Pi;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.FixRotation = true;
                dp.Rotation = isNorthCopy ? 0 : float.Pi;
                dp.DestoryAt = 17000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            });
        }

        [ScriptMethod(name: "P3 Pull Assist", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0066"])]
        public void P3PullAssist(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            Task.Delay(100).ContinueWith((t) =>
            {
                //38959 right cone
                //37975 left cone
                //37965 left jump right cleave
                //37966 left jump left cleave
                //37967 right jump right cleave
                //37968 right jump left cleave

                if (P3TetherTarget.Count == 2)
                {
                    var skillId1 = P3JumpSkill[0];
                    var skillId2 = P3JumpSkill[1];
                    var leftJump1 = (skillId1 == "37965" || skillId1 == "37966" || skillId1 == "37975");
                    var leftJump2 = (skillId2 == "37965" || skillId2 == "37966" || skillId2 == "37975");

                    var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                    var isNorthCopy1 = Math.Abs(pos.Z - 95) < 1;
                    var epos = new Vector3(100, 0, 100);
                    if (leftJump1 != leftJump2) 
                    {
                        if (isNorthCopy1)
                        {
                            epos = leftJump2 ? new(110, 0, 100) : new(90, 0, 100);
                        }
                        else
                        {
                            epos = leftJump2 ? new(90, 0, 100) : new(110, 0, 100);
                        }
                    }

                    
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"P3 Pull Assist Start";
                    dp.Scale = new(2, 10);
                    dp.Color = jumpColor.V4;
                    dp.Owner = tid;
                    dp.TargetPosition = epos;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Delay = 0;
                    dp.DestoryAt = 13000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }


                //First jump
                if (P3TetherTarget.Count == 3)
                {
                    var jump1 = P3TetherTarget.IndexOf(sid);
                    var jump2 = jump1 == 0 ? 1 : 0;
                    var skillId1 = P3JumpSkill[jump1];
                    var skillId2 = P3JumpSkill[jump2];
                    var leftJump1 = (skillId1 == "37965" || skillId1 == "37966" || skillId1 == "37975");
                    var leftJump2 = (skillId2 == "37965" || skillId2 == "37966" || skillId2 == "37975");

                    var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                    var pos2=new Vector3(pos.X,pos.Y,100-(pos.Z-100));
                    var isNorthCopy1 = Math.Abs(pos.Z - 95) < 1;

                    //38959 right cone
                    //37975 left cone
                    var firstDur = (skillId1 == "38959" || skillId1 == "37975") ? 25000 : 17000;

                    Vector3 dv1 = new(0, 0, 0);
                    if (isNorthCopy1)
                    {
                        dv1 = leftJump1 ? new(10, 0, 0) : new(-10, 0, 0);
                    }
                    else
                    {
                        dv1 = leftJump1 ? new(-10, 0, 0) : new(10, 0, 0);
                    }
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"P3 Pull Assist 1";
                    dp.Scale = new(2, 10);
                    dp.Color = jumpColor.V4;
                    dp.Owner = tid;
                    dp.TargetPosition = pos + dv1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Delay = 0;
                    dp.DestoryAt = firstDur;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    Vector3 dv2 = new(0, 0, 0);
                    if (isNorthCopy1)
                    {
                        dv2 = leftJump2 ? new(-10, 0, 0) : new(10, 0, 0);
                    }
                    else
                    {
                        dv2 = leftJump2 ? new(10, 0, 0) : new(-10, 0, 0);
                       
                    }
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"P3 Pull Assist 2";
                    dp.Scale = new(2, 10);
                    dp.Color = jumpColor.V4;
                    dp.Owner = tid;
                    dp.TargetPosition = pos2 + dv2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Delay = firstDur;
                    dp.DestoryAt = 47000- firstDur;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                

            });
        }

        [ScriptMethod(name: "Pair Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37982|38016)$"])]
        public void PairStack(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var sid)) return;

            int[] stackGroup = [6, 5, 4, 7, 2, 1, 0, 3];
            var index= accessory.Data.PartyList.ToList().IndexOf(sid);
            var myIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            var isMyStack = (index == myIndex || myIndex == stackGroup[index]);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Pair Stack";
            dp.Scale = new(4);
            dp.Color = isMyStack?accessory.Data.DefaultSafeColor: accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "Four-Player Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37984|38018)$"])]
        public void FourPlayerStack(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var sid)) return;

            int[] h1Group = [0,2,4,6];
            var index = accessory.Data.PartyList.ToList().IndexOf(sid);
            var myIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);

            var isMyStack = (h1Group.Contains(index)== h1Group.Contains(myIndex));

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Four-Player Stack";
            dp.Scale = new(5);
            dp.Color = isMyStack ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Four-Player Line Stack", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:34722"])]
        public void FourPlayerLineStack(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;

            int[] h1Group = [0, 2, 4, 6];
            var index = accessory.Data.PartyList.ToList().IndexOf(tid);
            var myIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);

            var isMyStack = (h1Group.Contains(index) == h1Group.Contains(myIndex));

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Four-Player Line Stack";
            dp.Scale = new(6,40);
            dp.Color = isMyStack ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.TargetObject = tid;
            dp.DestoryAt = 5500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "Seven-Player Line Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38039"])]
        public void SevenPlayerLineStack(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Seven-Player Line Stack";
            dp.Scale = new(5, 40);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = sid;
            dp.TargetObject = tid;
            dp.DestoryAt = 5500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "Wind Circle Spread", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38022"])]
        public void WindCircleSpread(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Wind Circle Spread";
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            if(sid==accessory.Data.Me) accessory.Method.TextInfo("Wind Circle Spread",8000,true);
        }
        [ScriptMethod(name: "Role Spread Prompt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38041"])]
        public void RoleSpreadPrompt(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("Role Spread",5500,false);
        }

        [ScriptMethod(name: "Floor Break Safe Zone Reset", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37953"],userControl:false)]
        public void FloorBreakSafeZoneReset(Event @event, ScriptAccessory accessory)
        {
            FloorBrokeList = new ();
        }
        [ScriptMethod(name: "Floor Break Safe Zone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(39276|37955)$"])]
        public void FloorBreakSafeZone(Event @event, ScriptAccessory accessory)
        {
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var centre = new Vector3(100, 0, 100);
            var dv = pos - centre;
            if (dv.Length() > 10) return;
            lock (FloorBrokeList)
            {
                var index = FloorToIndex(pos);
                FloorBrokeList.Add(index);
                
                if (FloorBrokeList.Count == 5)
                {
                    var during = 20000;
                    var nwSafe = (index == 0 || index == 2);
                    Vector3 endPos = default;
                    if (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 0)
                    {
                        endPos = nwSafe ? new Vector3(95, 0, 95) : new Vector3(105, 0, 95);
                    }
                    else
                    {
                        endPos = nwSafe ? new Vector3(105, 0, 105) : new Vector3(95, 0, 105);
                    }
                    var safePosIndex1 = FloorToIndex(endPos);

                    var safePosBrokeIndex= FloorBrokeList.IndexOf(safePosIndex1);
                    if (safePosBrokeIndex == 0)
                    {
                        var startPos = Math.Abs(FloorBrokeList[3]- safePosIndex1)%4==1 ? IndexToFloor(FloorBrokeList[3]): IndexToFloor(FloorBrokeList[2]);
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"Floor Break Safe Zone";
                        dp.Scale = new(2);
                        dp.Position = startPos;
                        dp.TargetPosition = endPos;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = during;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                    if (safePosBrokeIndex == 1)
                    {
                        if(Math.Abs(FloorBrokeList[3] - safePosIndex1) % 4 == 1)
                        {
                            var startPos = IndexToFloor(FloorBrokeList[3]);
                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"Floor Break Safe Zone";
                            dp.Scale = new(2);
                            dp.Position = startPos;
                            dp.TargetPosition = endPos;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                        else
                        {
                            var startPos = IndexToFloor(FloorBrokeList[3]);
                            var pos2= IndexToFloor(FloorBrokeList[0]);
                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"Floor Break Safe Zone";
                            dp.Scale = new(2);
                            dp.Position = startPos;
                            dp.TargetPosition = pos2;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                            dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"Floor Break Safe Zone";
                            dp.Scale = new(2);
                            dp.Position = pos2;
                            dp.TargetPosition = endPos;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                        


                    }
                    if (safePosBrokeIndex == 2)
                    {
                        if (Math.Abs(FloorBrokeList[0] - safePosIndex1) % 4 == 1)
                        {
                            var pos2 = IndexToFloor(FloorBrokeList[0]);
                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"Floor Break Safe Zone";
                            dp.Scale = new(2);
                            dp.Position = endPos;
                            dp.TargetPosition = pos2;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                            dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"Floor Break Safe Zone";
                            dp.Scale = new(2);
                            dp.Position = pos2;
                            dp.TargetPosition = new((endPos.X - 100) * 0.6f + 100, 0, (endPos.Z - 100) * 0.6f + 100);
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                        else
                        {
                            var pos1 = IndexToFloor(FloorBrokeList[3]);
                            var pos2 = IndexToFloor(FloorBrokeList[0]);
                            var pos3 = IndexToFloor(FloorBrokeList[1]);

                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"Floor Break Safe Zone";
                            dp.Scale = new(2);
                            dp.Position = pos1;
                            dp.TargetPosition = pos2;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                            dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"Floor Break Safe Zone";
                            dp.Scale = new(2);
                            dp.Position = pos2;
                            dp.TargetPosition = pos3;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                            dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"Floor Break Safe Zone";
                            dp.Scale = new(2);
                            dp.Position = pos3;
                            dp.TargetPosition = endPos;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                    }
                    if (safePosBrokeIndex == 3)
                    {
                        if (Math.Abs(FloorBrokeList[0] - safePosIndex1) % 4 == 1)
                        {
                            var pos2 = IndexToFloor(FloorBrokeList[0]);
                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"Floor Break Safe Zone";
                            dp.Scale = new(2);
                            dp.Position = endPos;
                            dp.TargetPosition = pos2;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                            dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"Floor Break Safe Zone";
                            dp.Scale = new(2);
                            dp.Position = pos2;
                            dp.TargetPosition = new((endPos.X - 100) * 0.6f + 100, 0, (endPos.Z - 100) * 0.6f + 100);
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                        else
                        {
                            var pos2 = IndexToFloor(FloorBrokeList[1]);
                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"Floor Break Safe Zone";
                            dp.Scale = new(2);
                            dp.Position = endPos;
                            dp.TargetPosition = pos2;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                            dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"Floor Break Safe Zone";
                            dp.Scale = new(2);
                            dp.Position = pos2;
                            dp.TargetPosition = new((endPos.X - 100) * 0.6f + 100, 0, (endPos.Z - 100) * 0.6f + 100);
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                    }




                    


                    
                    
                    
                    
                }
                

                

                

            }



        }
        
        

        [ScriptMethod(name: "Clone Cat Claw Mark", eventType: EventTypeEnum.TargetIcon,userControl:false)]
        public void CloneCatClawMark(Event @event, ScriptAccessory accessory)
        {
            if (ParsTargetIcon(@event["Id"]) != 320) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            copyCatTarget = tid;
        }
        [ScriptMethod(name: "Clone Slam Knockback", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37958"])]
        public void CloneSlamKnockback(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(50).ContinueWith(t =>
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "Clone Slam Knockback";
                dp.Scale = new(1.5f, 10);
                dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
                dp.Owner = copyCatTarget;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
            });
        }

        [ScriptMethod(name: "Clone Slam Cross", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37960|37958)$"])]
        public void CloneSlamCross(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(50).ContinueWith(t =>
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "Clone Slam Cross";
                dp.Scale = new(1.5f, 80);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = copyCatTarget;
                dp.FixRotation = true;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "Clone Slam Cross";
                dp.Scale = new(1.5f, 80);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = copyCatTarget;
                dp.FixRotation = true;
                dp.Rotation = float.Pi / 2;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
            });
        }

        [ScriptMethod(name: "Floor Repair Safe Zone", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00080004", "Index:regex:^(0000000[1247])$"])]
        public void FloorRepairSafeZone(Event @event, ScriptAccessory accessory)
        {
            var myIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);

            if (MtSafeFloor == KnockBackMtPosition.SouthHalf || MtSafeFloor == KnockBackMtPosition.NorthHalf)
            {
                int[] northGroup = MtSafeFloor == KnockBackMtPosition.NorthHalf ? [0, 2, 4, 6] : [1, 3, 5, 7];
                var isNorthGroup = northGroup.Contains(myIndex);
                if (@event["Index"] == "00000001")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Floor Repair Safe Zone";
                    dp.Scale = new(20f, 10);
                    dp.Position = isNorthGroup ? new(90, 0, 85) : new(110, 0, 115);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
                if (@event["Index"] == "00000002")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Floor Repair Safe Zone";
                    dp.Scale = new(20f, 10);
                    dp.Position = isNorthGroup ? new(110, 0, 85) : new(90, 0, 115);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
                if (@event["Index"] == "00000004")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Floor Repair Safe Zone";
                    dp.Scale = new(10, 20);
                    dp.Position = isNorthGroup ? new(85, 0, 90) : new(115, 0, 110);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
                if (@event["Index"] == "00000007")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Floor Repair Safe Zone";
                    dp.Scale = new(10, 20);
                    dp.Position = isNorthGroup ? new(115, 0, 90) : new(85, 0, 110);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
            }
            else
            {
                int[] eastGroup = MtSafeFloor == KnockBackMtPosition.EastHalf ? [0, 2, 4, 6] : [1, 3, 5, 7];
                var isEastGroup = eastGroup.Contains(myIndex);
                if (@event["Index"] == "00000001")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Floor Repair Safe Zone";
                    dp.Scale = new(20f, 10);
                    dp.Position = isEastGroup ?  new(110, 0, 115): new(90, 0, 85);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
                if (@event["Index"] == "00000002")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Floor Repair Safe Zone";
                    dp.Scale = new(20f, 10);
                    dp.Position = isEastGroup ? new(110, 0, 85) : new(90, 0, 115);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
                if (@event["Index"] == "00000004")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Floor Repair Safe Zone";
                    dp.Scale = new(10, 20);
                    dp.Position = isEastGroup ? new(115, 0, 110) : new(85, 0, 90);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
                if (@event["Index"] == "00000007")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Floor Repair Safe Zone";
                    dp.Scale = new(10, 20);
                    dp.Position = isEastGroup ? new(115, 0, 90) : new(85, 0, 110);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
            }


        }
        [ScriptMethod(name: "Knockback", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37964"])]
        public void Knockback(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Knockback";
            dp.Scale = new(1.5f,21);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = sid;
            dp.Rotation = float.Pi;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "Near/Far Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3961[12])$"])]
        public void NearFarStack(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            dur += 1300;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"NearFarStack Near";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = sid;
            dp.CentreResolvePattern=PositionResolvePatternEnum.PlayerNearestOrder;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"NearFarStack Far";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
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
        private int ParsTargetIcon(string id)
        {
            firstTargetIcon??= int.Parse(id, System.Globalization.NumberStyles.HexNumber);
            return int.Parse(id, System.Globalization.NumberStyles.HexNumber) - (int)firstTargetIcon;
        }
        
        private int FloorToIndex(Vector3 pos)
        {
            var centre = new Vector3(100, 0, 100);
            var dv = pos - centre;
            var index = 0;
            if (dv.X > 0)
            {
                if (dv.Z > 0)
                {
                    index = 3;
                }
                else
                {
                    index = 0;
                }
            }
            else
            {
                if (dv.Z > 0)
                {
                    index = 2;
                }
                else
                {
                    index = 1;
                }
            }
            return index;
        }
        private Vector3 IndexToFloor(int index)
        {
            switch (index)
            {
                case 0: return new(105, 0, 95);
                case 1: return new(95, 0, 95);
                case 2: return new(95, 0, 105);
                case 3: return new(105, 0, 105);
            }
            return default;
        }
    }
}
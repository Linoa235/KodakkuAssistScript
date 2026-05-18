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
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Module.Draw.Manager;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading;
using KodakkuAssist.Extensions;


namespace KarlinScriptNamespace
{
    [ScriptType(name: "M5s Drawing", territorys: [1257], guid: "a756c231-c625-4c1e-bb73-e0bb42afdd5b", version: "0.0.0.3", Author: "Linoa235", note: noteStr, updateInfo: updateInfoStr)]
    public class M5sDraw
    {
        const string noteStr =
        """
        
        """;
        const string updateInfoStr =
        """
        1. Added Cross Reaper floor fire drawing
        2. Added Tetris safe zone drawing
        """;

        [UserSetting("Left/Right Cleave Stack Delay Display Time")]
        public uint StackDelay { get; set; } = 5000;
        [UserSetting("Clone Left/Right Cleave Interval")]
        public int flipDuring { get; set; } = 2450;

        private int parse;
        private ulong BossId;
        private bool isHealerStack;
        private int fireSafePoint = 0;
        private int fireCount = 0;
        private bool spotLightClockwise;
        private List<uint> flipDir=[];
        private List<DateTime> wavelengthBuffEndTime = [default, default, default, default, default, default, default, default];
        private ManualResetEvent SpotlightResetEvent = new(false);
        private ManualResetEvent BurnBuffResetEvent = new(false);
        private bool spotLightN;
        private bool longBurnBuff;
        private int spotLightRound;
        [Flags]
        private enum FlipDangerArea
        {
            None = 0,
            Point1 = 1,
            Point2 = 2,
            Point3 = 4,
            Point4 = 8,
        }

        public void Init(ScriptAccessory accessory)
        {
            parse = 0;

        }
        [ScriptMethod(name: "BossId Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:41767", "TargetIndex:1"],userControl:false)]
        public void BossIdRecord(Event @event, ScriptAccessory accessory)
        {
            BossId = @event.SourceId;
        }

        [ScriptMethod(name: "Bleeding Tankbuster", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:01D7"])]
        public void Tankbuster(Event @event, ScriptAccessory accessory)
        {
            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s Bleeding Tankbuster";
            dp.Scale = new(25);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = BossId;
            dp.TargetObject = @event.TargetId;
            dp.DestoryAt = 5700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        [ScriptMethod(name: "Stack Method Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42880|42881)$"], userControl: false)]
        public void StackMethodRecord(Event @event, ScriptAccessory accessory)
        {
            isHealerStack = @event.ActionId == 42881;
        }
        [ScriptMethod(name: "Left/Right Cleave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4279[234567]|4280[0123459]|4281[01234]|4220[345678])$"])]
        public void LeftRightCleave(Event @event, ScriptAccessory accessory)
        {
            var actionId = @event.ActionId;
            var during=7000;
            if(     actionId ==42792 || actionId==42793 
                || actionId == 42794 || actionId == 42795 
                || actionId == 42796 || actionId == 42797
                || actionId == 42203 || actionId == 42794
                )
            {
                during = 6500;
            }
            var rotation = float.Pi / 2;
            if (    actionId == 42792 || actionId == 42793 || actionId == 42794 
                ||  actionId == 42800 || actionId == 42801 || actionId == 42802
                ||  actionId == 42809 || actionId == 42810 || actionId == 42811 
                ||  actionId == 42203 || actionId == 42205 || actionId == 42207)
            {
                rotation = -rotation;
            }
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s LeftRightCleave_Front";
            dp.Owner = @event.SourceId;
            dp.Scale = new(40, 20);
            dp.Rotation = rotation;
            dp.DestoryAt = during;
            dp.Color= accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s LeftRightCleave_Back";
            dp.Owner = @event.SourceId;
            dp.Scale = new(40, 20);
            dp.Rotation = -rotation;
            dp.Delay = during;
            dp.DestoryAt = 8500-during;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "Left/Right Cleave Cross Prompt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4279[234567]|4280[0123459]|4281[01234]|4220[345678])$"])]
        public void LeftRightCleaveCrossPrompt(Event @event, ScriptAccessory accessory)
        {
            var actionId = @event.ActionId;
            var during = 7000;
            if (actionId == 42792 || actionId == 42793
                || actionId == 42794 || actionId == 42795
                || actionId == 42796 || actionId == 42797
                || actionId == 42203 || actionId == 42794
                )
            {
                during = 6500;
            }
            Task.Delay(during).ContinueWith(t =>
            {
                accessory.Method.TextInfo("Cross", 1000);
                accessory.Method.TTS("Cross");
            });

        }
        [ScriptMethod(name: "Left/Right Cleave Group Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4279[234567]|4280[0123459]|4281[01234]|4220[345678])$"])]
        public void LeftRightCleaveGroupStack(Event @event, ScriptAccessory accessory)
        {
            if (isHealerStack)
            {
                var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var h1Group = myindex == 0 || myindex == 2 || myindex == 4 || myindex == 6;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s LeftRightCleave_H1Stack";
                dp.Owner = @event.SourceId;
                dp.TargetObject = accessory.Data.PartyList[2];
                dp.Scale = new(8, 50);
                dp.Delay = StackDelay;
                dp.DestoryAt = 10500 - StackDelay;
                dp.Color = h1Group ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s LeftRightCleave_H2Stack";
                dp.Owner = @event.SourceId;
                dp.TargetObject = accessory.Data.PartyList[3];
                dp.Scale = new(8, 50);
                dp.Delay = StackDelay;
                dp.DestoryAt = 10500 - StackDelay;
                dp.Color = h1Group ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            else
            {
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var tGroup = myindex == 0 || myindex == 1;
                var hGroup = myindex == 2 || myindex == 3;
                var dGroup = myindex == 4 || myindex == 5 || myindex == 6 || myindex == 7;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s LeftRightCleave_TStack";
                dp.Owner = @event.SourceId;
                dp.TargetObject = myindex == 0? accessory.Data.PartyList[1]: accessory.Data.PartyList[0];
                dp.Scale = new(40);
                dp.Radian = float.Pi / 4;
                dp.Delay = StackDelay;
                dp.DestoryAt = 10500 - StackDelay;
                dp.Color = tGroup ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s LeftRightCleave_HStack";
                dp.Owner = @event.SourceId;
                dp.TargetObject = myindex == 2 ? accessory.Data.PartyList[3] : accessory.Data.PartyList[2];
                dp.Scale = new(40);
                dp.Radian = float.Pi / 4;
                dp.Delay = StackDelay;
                dp.DestoryAt = 10500 - StackDelay;
                dp.Color = hGroup ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s LeftRightCleave_DStack";
                dp.Owner = @event.SourceId;
                dp.TargetObject = myindex == 4 ? accessory.Data.PartyList[5]: accessory.Data.PartyList[4];
                dp.Scale = new(40);
                dp.Radian = float.Pi / 4;
                dp.Delay = StackDelay;
                dp.DestoryAt = 10500 - StackDelay;
                dp.Color = dGroup ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
        }

        [ScriptMethod(name: "Dance Phase Reset", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42838"], userControl: false)]
        public void DancePhaseReset(Event @event, ScriptAccessory accessory)
        {
            fireSafePoint = 0;
            fireCount = 0;
            parse ++;
            SpotlightResetEvent = new(false);
            BurnBuffResetEvent = new(false);
            spotLightRound = 0;
        }
        [ScriptMethod(name: "Inner Spotlight Clockwise Record", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4572", "SourceDataId:18363"], userControl: false)]
        public void InnerSpotlightClockwiseRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            var pos = @event.SourcePosition;
            if (MathF.Abs(pos.X - 97.5f) < 1 && MathF.Abs(pos.Z - 92.5f) < 1) spotLightClockwise = false;
            if (MathF.Abs(pos.X - 102.5f) < 1 && MathF.Abs(pos.Z - 92.5f) < 1) spotLightClockwise = true;
        }
        [ScriptMethod(name: "Floor Fire Situation Record", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:3", "Flag:regex:^(2|32)$"], userControl: false)]
        public void FloorFireSituationRecord(Event @event, ScriptAccessory accessory)
        {
            if (fireSafePoint != 0) return;
            fireSafePoint = @event["Flag"] == "2" ? 2 : 1;
        }
        [ScriptMethod(name: "Floor Fire Drawing", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:3", "Flag:regex:^(8|128)$"])]
        public void FloorFireDrawing(Event @event, ScriptAccessory accessory)
        {
            fireCount ++;
            accessory.Log.Debug($"{parse} {fireCount}");
            if (parse == 1 && fireCount == 10) return;
            if (parse == 4 && fireCount == 8) return;
            var westNorth = @event["Flag"]=="8";
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var xOffset = westNorth == (i % 2 == 0) ? 5 : 0;
                    Vector3 pos = new(85.0f + j * 10 + xOffset, 0, 82.5f + i * 5);
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"M5s FloorFireDrawing Round{fireCount+1} {j}{i}";
                    dp.Position = pos;
                    dp.Scale = new(5);
                    dp.Rotation = -float.Pi / 2;
                    dp.DestoryAt = 4000;
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
            }
        }
        [ScriptMethod(name: "First Burn Buff Processing Position", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4461"])]
        public void FirstBurnBuffProcessingPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (@event.TargetId != accessory.Data.Me) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            Task.Delay(dur - 8000).ContinueWith(t => {
                if (parse != 1) return;
                var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var first = dur < 25000;
                Vector3 dealpos = default;
                if (myindex == 2 || myindex == 6)
                {
                    dealpos = fireSafePoint ==2 ?  new(87.5f, 0, 87.5f) : dealpos = new(87.5f, 0, 112.5f);
                }
                if (myindex == 3 || myindex == 7)
                {
                    dealpos = fireSafePoint == 2 ? new(112.5f, 0, 112.5f) : dealpos = new(112.5f, 0, 87.5f);
                }
                if (myindex == 0 || myindex == 4)
                {
                    if (first)
                    {
                        if (fireSafePoint == 1)
                        {
                            dealpos= spotLightClockwise ? new(97.5f, 0, 92.5f) : new(92.5f, 0, 97.5f);
                        }
                        if(fireSafePoint == 2)
                        {
                            dealpos = spotLightClockwise ? new(92.5f, 0, 102.5f) : new(97.5f, 0, 107.5f);
                        }
                    }
                    else
                    {
                        if (fireSafePoint == 1)
                        {
                            dealpos = spotLightClockwise ? new(92.5f, 0, 97.5f) : new(97.5f, 0, 92.5f);
                        }
                        if (fireSafePoint == 2)
                        {
                            dealpos = spotLightClockwise ? new(97.5f, 0, 107.5f) : new(92.5f, 0, 102.5f);
                        }
                    }
                }
                if (myindex == 1 || myindex == 5)
                {
                    if (first)
                    {
                        if (fireSafePoint == 1)
                        {
                            dealpos = spotLightClockwise ? new(102.5f, 0, 107.5f) : new(107.5f, 0, 102.5f);
                        }
                        if (fireSafePoint == 2)
                        {
                            dealpos = spotLightClockwise ? new(107.5f, 0, 97.5f) : new(102.5f, 0, 92.5f);
                        }
                    }
                    else
                    {
                        if (fireSafePoint == 1)
                        {
                            dealpos = spotLightClockwise ? new(107.5f, 0, 102.5f) : new(102.5f, 0, 107.5f);
                        }
                        if (fireSafePoint == 2)
                        {
                            dealpos = spotLightClockwise ? new(102.5f, 0, 92.5f) : new(107.5f, 0, 97.5f);
                        }
                    }
                }


                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s Burn Buff Processing Position";
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition= dealpos;
                dp.Scale = new(3);
                dp.ScaleMode|=ScaleMode.YByDistance;
                dp.DestoryAt = 10000;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s Burn Buff Processing Position";
                dp.Position = dealpos;
                dp.Scale = new(2.5f);
                dp.DestoryAt = 10000;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

            });
        }
        [ScriptMethod(name: "Second Burn Spotlight Record", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4572", "SourceDataId:18363"], userControl: false)]
        public void SecondBurnSpotlightRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5) return;
            var pos = @event.SourcePosition;
            if (MathF.Abs(pos.X - 85f) < 1 && MathF.Abs(pos.Z - 100f) < 1) 
            { 
                spotLightN = true;
                SpotlightResetEvent.Set();
            }
            if (MathF.Abs(pos.X - 85f) < 1 && MathF.Abs(pos.Z - 85f) < 1)
            {
                spotLightN = false;
                SpotlightResetEvent.Set();
            }
        }
        [ScriptMethod(name: "Second Burn Buff Processing Position", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4461"])]
        public void SecondBurnBuffProcessingPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5) return;
            if (@event.TargetId != accessory.Data.Me) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            Task.Delay(dur - 9500).ContinueWith(t =>
            {
                if (parse != 5) return;
                SpotlightResetEvent.WaitOne();
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var first = dur < 11000;
                Vector3 dealpos = default;
                //Cardinal lamp (100,0,85) Intercardinal lamp (85,0,85)
                if (myindex == 0 || myindex == 6)
                {
                    if (spotLightN) 
                        dealpos = first ? new(85, 0, 85) : new(100, 0, 85);
                    else 
                        dealpos = first ? new(100, 0, 85) : new(85, 0, 85);
                }
                if (myindex == 1 || myindex == 5)
                {
                    if (spotLightN)
                        dealpos = first ? new(115, 0, 115) : new(100, 0, 115);
                    else
                        dealpos = first ? new(100, 0, 115) : new(115, 0, 115);
                }
                if (myindex == 2 || myindex == 4)
                {
                    if (spotLightN)
                        dealpos = first ? new(85, 0, 115) : new(85, 0, 100);
                    else
                        dealpos = first ? new(85, 0, 100) : new(85, 0, 115);
                }
                if (myindex == 3 || myindex == 7)
                {
                    if (spotLightN)
                        dealpos = first ? new(115, 0, 85) : new(115, 0, 100);
                    else
                        dealpos = first ? new(115, 0, 100) : new(115, 0, 85);
                }

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s Burn Buff Processing Position";
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.Scale = new(3);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = first ? 9500 : 10000;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s Burn Buff Processing Position";
                dp.Position = dealpos;
                dp.Scale = new(2.5f);
                dp.Delay = first ? 0 : 1000;
                dp.DestoryAt = first ? 9500 : 9000;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

            });
            



        }
        [ScriptMethod(name: "Second Burn Buff Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4461"],userControl:false)]
        public void SecondBurnBuffRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5) return;
            if (@event.TargetId != accessory.Data.Me) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            longBurnBuff = dur > 11000;
            BurnBuffResetEvent.Set();
        }
        [ScriptMethod(name: "Second Burn Guide Position", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4561", "SourceDataId:18362"], suppress:1000)]
        public void SecondBurnGuidePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5) return;
            spotLightRound++;
            var pos = @event.SourcePosition;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            SpotlightResetEvent.WaitOne();
            BurnBuffResetEvent.WaitOne();
            var danceN = (MathF.Abs(pos.X - 100f) < 1 || MathF.Abs(pos.Z - 100f) < 1);
            var first = spotLightRound == 1;
            if (!longBurnBuff && first) return;
            if (longBurnBuff && !first) return;
            var nowSpotlightN =spotLightN != first;
            Vector3 dealpos = default;
            if (myindex == 0 || myindex == 6)
            {
                if (nowSpotlightN)
                    dealpos = danceN ? new(98, 0, 93) : new(93, 0, 93);
                else
                    dealpos = danceN ? new(100, 0, 91) : new(95, 0, 93);
            }
            if (myindex == 1 || myindex == 5)
            {
                if (nowSpotlightN)
                    dealpos = danceN ? new(102, 0, 107) : new(107, 0, 107);
                else
                    dealpos = danceN ? new(100, 0, 109) : new(105, 0, 107);
            }
            if (myindex == 2 || myindex == 4)
            {
                if (nowSpotlightN)
                    dealpos = danceN ? new(93, 0, 102) : new(93, 0, 107);
                else
                    dealpos = danceN ? new(91, 0, 100) : new(93, 0, 105);
            }
            if (myindex == 3 || myindex == 7)
            {
                if (nowSpotlightN)
                    dealpos = danceN ? new(107, 0, 98) : new(107, 0, 93);
                else
                    dealpos = danceN ? new(109, 0, 100) : new(105, 0, 93);
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s Second Burn Guide Position";
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Scale = new(3);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 10700;
            dp.Color = accessory.Data.DefaultSafeColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "Heavy/Donut", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42876|42878)$"])]
        public void HeavyDonut(Event @event, ScriptAccessory accessory)
        {
            var circle = @event.ActionId == 42876;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s HeavyDonut_Heavy";
            dp.Owner = @event.SourceId;
            dp.Scale = new(7);
            dp.Delay= circle ? 0 : 5000;
            dp.DestoryAt = circle ? 5000 : 2500;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s HeavyDonut_Donut";
            dp.Owner = @event.SourceId;
            dp.Scale = new(40);
            dp.InnerScale = new(5);
            dp.Radian = float.Pi * 2;
            dp.Delay = circle ? 5000 : 0;
            dp.DestoryAt = circle ? 2500 : 5000;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

        }

        [ScriptMethod(name: "Water Wave Second Stage", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42853)$"])]
        public void WaterWaveSecondStage(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s HeavyDonut_Heavy";
            dp.Owner = @event.SourceId;
            dp.Scale = new(40);
            dp.Radian = float.Pi / 4;
            dp.DestoryAt = 2500;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }
        [ScriptMethod(name: "Clone Phase Reset", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39474"], userControl: false)]
        public void ClonePhaseReset(Event @event, ScriptAccessory accessory)
        {
            flipDir = [];
            parse ++;
        }
        [ScriptMethod(name: "Clone Left/Right Cleave Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4276[2345])$"],userControl:false)]
        public void CloneLeftRightCleaveRecord(Event @event, ScriptAccessory accessory)
        {
            flipDir.Add(@event.ActionId-42761);
            accessory.Log.Debug($"{@event.ActionId - 42761}");
        }
        [ScriptMethod(name: "Wavelength Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(4463|4462)$"], userControl: false)]
        public void WavelengthRecord(Event @event, ScriptAccessory accessory)
        {
            var index = accessory.Data.PartyList.IndexOf((uint)@event.TargetId);
            if (index == -1) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            lock (wavelengthBuffEndTime)
            {
                wavelengthBuffEndTime[index] = DateTime.Now.AddMilliseconds(dur);
            }
        }
        [ScriptMethod(name: "Wavelength Partner", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(4463|4462)$"])]
        public void WavelengthPartner(Event @event, ScriptAccessory accessory)
        {
            if (accessory.Data.Me != @event.TargetId) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            Task.Delay(dur - 4000).ContinueWith(t =>
            {
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var myEnd=wavelengthBuffEndTime[myindex];
                for (int i = 0; i < accessory.Data.PartyList.Count; i++)
                {
                    if (i != myindex && Math.Abs((wavelengthBuffEndTime[i] - myEnd).TotalSeconds) < 2)
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"M5s Wavelength Partner";
                        dp.Owner = accessory.Data.PartyList[i];
                        dp.Scale = new(2);
                        dp.DestoryAt = 4000;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    }
                }
            });
            
        }
        [ScriptMethod(name: "Wavelength Stack Reminder", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(4463|4462)$"])]
        public void WavelengthStackReminder(Event @event, ScriptAccessory accessory)
        {
            if (accessory.Data.Me != @event.TargetId) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            Task.Delay(dur - 4000).ContinueWith(t =>
            {
                accessory.Method.TextInfo("Stack", 4000);
                accessory.Method.TTS("Stack");
            });

        }
        [ScriptMethod(name: "Dance Wave Full Open Pull TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42836)$"])]
        public void DanceWaveFullOpenPullTTS(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TTS("Pull South");
            accessory.Method.TextInfo("Pull South", 5000);
        }
        [ScriptMethod(name: "Clone Left/Right Cleave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42858)$"])]
        public void CloneLeftRightCleave(Event @event, ScriptAccessory accessory)
        {
            if (parse!=2) return;
            var dur1 = 5800;
            for (int i = 1; i < flipDir.Count; i++)
            {
                if (flipDir[i] != flipDir[i-1])
                {
                    break;
                }
                dur1 += flipDuring;
            }
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s CloneLeftRightCleave1";
            dp.Owner = BossId;
            dp.Scale = new(40, 20);
            dp.Rotation = flipDir[0] == 3 ? float.Pi / 2 : -float.Pi / 2;
            dp.DestoryAt = dur1;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            for (int i = 1; i < flipDir.Count; i++)
            {
                if (flipDir[i] == flipDir[i - 1]) continue;
                var dur = flipDuring;
                for (int j = i+1; j < flipDir.Count; j++)
                {
                    if (flipDir[j] != flipDir[j - 1])
                    {
                        break;
                    }
                    dur += flipDuring;
                }
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s CloneLeftRightCleave{i+1}";
                dp.Owner = BossId;
                dp.Scale = new(40, 20);
                dp.Rotation = flipDir[i] == 3 ? float.Pi / 2 : -float.Pi / 2;
                dp.Delay = 5800 + (i - 1) * flipDuring;
                dp.DestoryAt = dur;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }
        [ScriptMethod(name: "Clone Front/Back/Left/Right Cleave Range Preview", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(41872)$"])]
        public void CloneFrontBackLeftRightCleaveRangePreview(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6) return;

            FlipDangerArea danger1Area = FlipDangerArea.None;
            FlipDangerArea danger2Area = FlipDangerArea.None;
            FlipDangerArea danger3Area = FlipDangerArea.None;
            FlipDangerArea danger4Area = FlipDangerArea.None;

            danger1Area |= flipDir[0] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            for (int i = 0; i < flipDir.Count; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s FrontBackLeftRightCleaveRangePreview{1}";
                dp.Owner = BossId;
                dp.Scale = new(40, 20);
                dp.Rotation = flipDir[i] switch
                {
                    1 => 0,
                    2 => float.Pi,
                    3 => float.Pi / 2,
                    _ => -float.Pi / 2,
                };
                dp.Delay = i  * 750;
                dp.DestoryAt = 750;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }

        }
        [ScriptMethod(name: "Clone Front/Back/Left/Right Cleave Cross Method", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(41872)$"])]
        public void CloneFrontBackLeftRightCleaveCrossMethod(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6) return;

            FlipDangerArea danger1Area = FlipDangerArea.None;
            FlipDangerArea danger2Area = FlipDangerArea.None;
            FlipDangerArea danger3Area = FlipDangerArea.None;
            FlipDangerArea danger4Area = FlipDangerArea.None;

            danger1Area |= flipDir[0] switch
            {
                1 => FlipDangerArea.Point2| FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            danger1Area |= flipDir[1] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            danger2Area |= flipDir[2] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            danger2Area |= flipDir[3] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            danger3Area |= flipDir[4] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            danger3Area |= flipDir[5] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            danger4Area |= flipDir[6] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            danger4Area |= flipDir[7] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            Vector3 dealpos1 = default;
            if (!danger1Area.HasFlag(FlipDangerArea.Point1)) dealpos1 = new(101, 0, 99);
            if (!danger1Area.HasFlag(FlipDangerArea.Point2)) dealpos1 = new(101, 0, 101);
            if (!danger1Area.HasFlag(FlipDangerArea.Point3)) dealpos1 = new(99, 0, 101);
            if (!danger1Area.HasFlag(FlipDangerArea.Point4)) dealpos1 = new(99, 0, 99);
            Vector3 dealpos2 = default;
            if (!danger2Area.HasFlag(FlipDangerArea.Point1)) dealpos2 = new(101, 0, 99);
            if (!danger2Area.HasFlag(FlipDangerArea.Point2)) dealpos2 = new(101, 0, 101);
            if (!danger2Area.HasFlag(FlipDangerArea.Point3)) dealpos2 = new(99, 0, 101);
            if (!danger2Area.HasFlag(FlipDangerArea.Point4)) dealpos2 = new(99, 0, 99);
            Vector3 dealpos3 = default;
            if (!danger3Area.HasFlag(FlipDangerArea.Point1)) dealpos3 = new(101, 0, 99);
            if (!danger3Area.HasFlag(FlipDangerArea.Point2)) dealpos3 = new(101, 0, 101);
            if (!danger3Area.HasFlag(FlipDangerArea.Point3)) dealpos3 = new(99, 0, 101);
            if (!danger3Area.HasFlag(FlipDangerArea.Point4)) dealpos3 = new(99, 0, 99);
            Vector3 dealpos4 = default;
            if (!danger4Area.HasFlag(FlipDangerArea.Point1)) dealpos4 = new(101, 0, 99);
            if (!danger4Area.HasFlag(FlipDangerArea.Point2)) dealpos4 = new(101, 0, 101);
            if (!danger4Area.HasFlag(FlipDangerArea.Point3)) dealpos4 = new(99, 0, 101);
            if (!danger4Area.HasFlag(FlipDangerArea.Point4)) dealpos4 = new(99, 0, 99);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s CloneFrontBackLeftRightCleaveProcessingPosition1";
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos1;
            dp.Scale = new(3);
            dp.ScaleMode|= ScaleMode.YByDistance;
            dp.DestoryAt = 8300;
            dp.Color = accessory.Data.DefaultSafeColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s CloneFrontBackLeftRightCleaveProcessingPosition2_1";
            dp.Position = dealpos1;
            dp.TargetPosition = dealpos2;
            dp.Scale = new(3);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 8300;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s CloneFrontBackLeftRightCleaveProcessingPosition2_2";
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos2;
            dp.Scale = new(3);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = 8300;
            dp.DestoryAt = 3000;
            dp.Color = accessory.Data.DefaultSafeColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s CloneFrontBackLeftRightCleaveProcessingPosition3_1";
            dp.Position = dealpos2;
            dp.TargetPosition = dealpos3;
            dp.Scale = new(3);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = 8300;
            dp.DestoryAt = 3000;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s CloneFrontBackLeftRightCleaveProcessingPosition3_2";
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos3;
            dp.Scale = new(3);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = 11300;
            dp.DestoryAt = 3000;
            dp.Color = accessory.Data.DefaultSafeColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s CloneFrontBackLeftRightCleaveProcessingPosition4_1";
            dp.Position = dealpos3;
            dp.TargetPosition = dealpos4;
            dp.Scale = new(3);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = 11300;
            dp.DestoryAt = 3000;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s CloneFrontBackLeftRightCleaveProcessingPosition4_2";
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos4;
            dp.Scale = new(3);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = 14300;
            dp.DestoryAt = 3000;
            dp.Color = accessory.Data.DefaultSafeColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


        }
        [ScriptMethod(name: "Clone Left/Right Cleave Cross Prompt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42858)$"])]
        public void CloneLeftRightCleaveCrossPrompt(Event @event, ScriptAccessory accessory)
        {
            for (int i = 1; i < flipDir.Count; i++)
            {
                if (flipDir[i] != flipDir[i - 1])
                {
                    Task.Delay(5800 + (i - 1) * flipDuring).ContinueWith(t =>
                    {
                        if (parse!=2) return;
                        accessory.Method.TextInfo("Cross", 1000);
                        accessory.Method.TTS("Cross");
                    });
                }
                
            }
        }
        [ScriptMethod(name: "Arrow Clone Left/Right Cleave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42869|42870|42788|42789)$"])]
        public void ArrowCloneLeftRightCleave(Event @event, ScriptAccessory accessory)
        {
            var left = @event.ActionId == 42869 || @event.ActionId == 42788;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s ArrowCloneLeftRightCleave";
            dp.Owner = @event.SourceId;
            dp.Scale = new(90, 30);
            dp.Rotation = left ? -float.Pi / 2 : float.Pi / 2;
            dp.DestoryAt = 5000;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);


        }
        
        [ScriptMethod(name: "Consecutive Heavy/Donut", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39908"])]
        public void ConsecutiveHeavyDonut(Event @event, ScriptAccessory accessory)
        {
            for (int i = 0; i < 3; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s ConsecutiveHeavyDonut_Heavy";
                dp.Owner = @event.SourceId;
                dp.Scale = new(7);
                dp.Delay = 8000 + i * 2 * flipDuring;
                dp.DestoryAt = flipDuring;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }

            for (int i = 0; i < 4; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s ConsecutiveHeavyDonut_Donut";
                dp.Owner = @event.SourceId;
                dp.Scale = new(40);
                dp.InnerScale = new(5);
                dp.Radian = float.Pi * 2;
                dp.Delay = i == 0 ? 5200 : 5500 + i * 2 * flipDuring;
                dp.DestoryAt = i == 0 ? 2800 : flipDuring;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
            }
            
        }
        [ScriptMethod(name: "Tetris Reset", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42836"], userControl: false)]
        public void TetrisReset(Event @event, ScriptAccessory accessory)
        {
            parse = 3;

        }
        [ScriptMethod(name: "Tetris Spread/Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42844|42846)$"])]
        public void TetrisSpreadStack(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            if (@event["ActionId"] =="42844")
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s Tetris_Stack";
                dp.Owner = @event.TargetId;
                dp.Scale = new(4);
                dp.DestoryAt = 5000;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s Tetris_Spread";
                dp.Owner = @event.TargetId;
                dp.Scale = new(5);
                dp.DestoryAt = 5000;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }

        }
        [ScriptMethod(name: "Tetris Floor Fire Safe Zone Drawing", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:4", "Flag:regex:^(128|512)$"])]
        public void TetrisFloorFireSafeZoneDrawing(Event @event, ScriptAccessory accessory)
        {
            var dur = 2058;
            var left = @event["Flag"] == "128";
            for (int i = 0; i < 15; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s TetrisFloorFireSafeZoneDrawing Round{i} 1";
                dp.Position = new(left ? 87.5f : 112.5f, 0, 60.0f + i * 5);
                dp.Scale = new(5,25);
                dp.Delay = i == 0 ? 0 : 3000 + (i - 1) * dur;
                dp.DestoryAt = i == 0 ? 3000 : dur;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s TetrisFloorFireSafeZoneDrawing Round{i} 2";
                dp.Position = new(92.5f, 0, 45.0f + i * 5);
                dp.Scale = new(5, 20);
                dp.Delay = i == 0 ? 0 : 3000 + (i - 1) * dur;
                dp.DestoryAt = i == 0 ? 3000 : dur;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s TetrisFloorFireSafeZoneDrawing Round{i} 3";
                dp.Position = new(left ? 102.5f : 97.5f, 0, 60.0f + i * 5);
                dp.Scale = new(5, 25);
                dp.Delay = i == 0 ? 0 : 3000 + (i - 1) * dur;
                dp.DestoryAt = i == 0 ? 3000 : dur;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s TetrisFloorFireSafeZoneDrawing Round{i} 4";
                dp.Position = new(107.5f, 0, 45.0f + i * 5);
                dp.Scale = new(5, 20);
                dp.Delay = i == 0 ? 0 : 3000 + (i - 1) * dur;
                dp.DestoryAt = i == 0 ? 3000 : dur;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
            }
        }
        [ScriptMethod(name: "Moonwalk Phase Reset", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42847"], userControl: false)]
        public void MoonwalkPhaseReset(Event @event, ScriptAccessory accessory)
        {
            parse = 4;

        }
        [ScriptMethod(name: "Moonwalk Aoe", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42867|42868)$"])]
        public void MoonwalkAoe(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s ConsecutiveHeavyDonut_Heavy";
            dp.Owner = @event.SourceId;
            dp.Scale = new(15, 40);
            dp.Offset = new(@event["ActionId"] == "42867" ? 7.5f : -7.5f, 0, 0);
            dp.DestoryAt = 10500;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "Moonwalk Spread/Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42844|42846)$"])]
        public void MoonwalkSpreadStack(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4) return;
            if (@event["ActionId"] == "42844")
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s Tetris_Stack";
                dp.Owner = @event.TargetId;
                dp.Scale = new(4);
                dp.DestoryAt = 5000;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s Tetris_Spread";
                dp.Owner = @event.TargetId;
                dp.Scale = new(5);
                dp.DestoryAt = 5000;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }

        }
        

    }
}
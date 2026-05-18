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
using System.Security.Cryptography;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Collections;
using System.Text;
using NAudio.Gui;
using KodakkuAssist.Extensions;


namespace KarlinScriptNamespace
{
    [ScriptType(name: "M6s Drawing", territorys: [1259], guid: "e715669c-071c-4c09-ae91-bd434d63f9fa", version: "0.0.0.5", Author: "Linoa235", note: noteStr, updateInfo: updateInfoStr)]
    public class M6sDraw
    {
        const string noteStr =
        """
        M6S Extended Drawing Script
        Currently includes the following:
        1. Extended drawing (not all mechanics, use with Usami script)
        2. Zoo phase tank pull route indicator
        3. Zoo round 2 fish bait auto-selection (requires setting left/right), round 4 fish auto-selection (based on role assignment)
        4. Zoo phase melee auto-target horse selection
        """;
        const string updateInfoStr =
        """
        1. Added zoo round 4 horse selection
        """;
        public enum zoo2FishEnum
        {
            None,
            Left,
            Right,
        }

        [UserSetting("Zoo Round 2 Fish Bait")]
        public zoo2FishEnum zoo2Fish { get; set; }

        int parse = 0;
        List<int> StickyMousseTarget = [];
        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(@".*");
            parse = 0;

        }
        [ScriptMethod(name: "P1 Color Riot Tankbuster Range", eventType: EventTypeEnum.StartCasting,eventCondition: ["ActionId:regex:^(4264[12])$"])]
        public void P1_ColorRiot(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Near Tankbuster Range";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Far Tankbuster Range";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "P1 Color Riot Non-Tank Reminder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4264[12])$"])]
        public void P1_ColorRiotDPSNote(Event @event, ScriptAccessory accessory)
        {
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myindex<2) return; 

            accessory.Method.TTS($"Stand behind on target circle");
            accessory.Method.TextInfo($"Stand behind on target circle",7000);
        }

        [ScriptMethod(name: "P1 Sticky Mousse Reset Counter", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42645"])]
        public void P1_StickyMousse_ResetCounter(Event @event, ScriptAccessory accessory)
        {
            StickyMousseTarget.Clear();
        }
        [ScriptMethod(name: "P1 Sticky Mousse Pre-position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42645"])]
        public void P1_StickyMousse_PrePosition(Event @event, ScriptAccessory accessory)
        {
            Vector3 h1Point = new(92, 0, 100);
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 myPoint = new();
            if (myindex==0||myindex==1)
            {
                myPoint = new(100, 0, 100);
            }
            else
            {
                if (myindex == 2) myPoint = RotatePoint(h1Point, new(100, 0, 100), 0);
                if (myindex == 3) myPoint = RotatePoint(h1Point, new(100, 0, 100), MathF.PI);
                if (myindex == 4) myPoint = RotatePoint(h1Point, new(100, 0, 100), -MathF.PI / 3);
                if (myindex == 5) myPoint = RotatePoint(h1Point, new(100, 0, 100), -MathF.PI / 3*2);
                if (myindex == 6) myPoint = RotatePoint(h1Point, new(100, 0, 100), MathF.PI / 3);
                if (myindex == 7) myPoint = RotatePoint(h1Point, new(100, 0, 100), MathF.PI / 3*2);
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Sticky Mousse Pre-position";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner =accessory.Data.Me;
            dp.TargetPosition = myPoint;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


        }
        [ScriptMethod(name: "P1 Sticky Mousse Stack Range", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:42646", "TargetIndex:1"])]
        public void P2_StickyMousse_StackRange(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P1 Sticky Mousse Stack Range";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Scale = new(4);
            dp.Owner = @event.TargetId;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Vfx, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "P2 Desert Phase Transition", eventType: EventTypeEnum.StartCasting,eventCondition: ["ActionId:42600"],userControl:false)]
        public void P2_PhaseChange(Event @event, ScriptAccessory accessory)
        {
            parse = 21;
        }
        //[ScriptMethod(name: "P2 First Round Big Circle Navigation", eventType: EventTypeEnum.StatusAdd,eventCondition: ["StatusID:4454"])]
        //public void P2_FirstRoundBigCircleNavigation(Event @event, ScriptAccessory accessory)
        //{
        //    if (parse != 21) return;
        //    if (@event.TargetId != accessory.Data.Me) return;
        //    var myindex= accessory.Data.PartyList.IndexOf(accessory.Data.Me);
        //    if (myindex == 2 || myindex ==3) return;

        //    var dp = accessory.Data.GetDefaultDrawProperties();
        //    dp.Name = $"P2 - First Round Big Circle Navigation";
        //    dp.Color = accessory.Data.DefaultSafeColor;
        //    dp.Owner = accessory.Data.Me;
        //    dp.TargetPosition = myindex < 2 ? new(112, 0, 105) : new(88, 0, 105);
        //    dp.ScaleMode |= ScaleMode.YByDistance;
        //    dp.Delay = 38000;
        //    dp.DestoryAt = 5000;
        //    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        //}
        [ScriptMethod(name: "P2 Desert Big Circle Range", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4454"])]
        public void P2_DesertBigCircleRange(Event @event, ScriptAccessory accessory)
        {
            if (parse != 21) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P2 - Desert Big Circle Range";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(15);
            dp.Owner = @event.TargetId;
            dp.Delay = dur-5000;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Vfx, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P2 Big Circle Reminder", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4454"])]
        public void P2_BigCircleReminder(Event @event, ScriptAccessory accessory)
        {
            if (parse != 21) return;
            var isme = (@event.TargetId == accessory.Data.Me);
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var tindex = accessory.Data.PartyList.IndexOf((uint)@event.TargetId);
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            if (myindex == 2 || myindex == 3) return;
            Task.Delay(6500).ContinueWith(t =>
            {
                if (parse != 21) return;
                if (tindex == 0 && (myindex == 0 || myindex == 1))
                {
                    if (isme)
                    {
                        accessory.Method.TTS($"Big circle, swap tanks");
                        accessory.Method.TextInfo($"Big circle, swap tanks", dur - 13500);
                    }
                    else
                    {
                        accessory.Method.TTS($"Swap tanks");
                        accessory.Method.TextInfo($"Swap tanks", dur - 13500);
                    }
                }
                else
                {
                    if (tindex != myindex) return;
                    accessory.Method.TTS($"Big circle");
                    accessory.Method.TextInfo($"Big circle", dur - 13500);
                }
                
                
            });
            
            Task.Delay(dur - 5000).ContinueWith(t =>
            {
                if (parse != 21) return;
                if (tindex != myindex) return;
                accessory.Method.TTS($"Go out to place circle");
                accessory.Method.TextInfo($"Go out to place circle", 5000,true);
            });

        }

        [ScriptMethod(name: "P3 Zoo Phase Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4266[12345])$"], userControl: false)]
        public void P3_PhaseChange(Event @event, ScriptAccessory accessory)
        {
            parse = (int)@event.ActionId - 42661 + 30;
        }

        [ScriptMethod(name: "P3 Zoo First Round Pull Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42661)$"])]
        public void P3_Zoo_FirstRoundPullPosition(Event @event, ScriptAccessory accessory)
        {
            //13000ms
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 pos = new(93, 0, 100);
            if (myIndex==0) pos= new(107, 0, 100);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3 Zoo First Round Pull Position";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = pos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 13000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            if (myIndex == 0)
            {
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo First Round Pull Position MT";
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Position = pos;
                dp.TargetPosition = new(100,0,110);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 13000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo First Round Pull Position MT";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(100, 0, 110); ;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Delay = 13000;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

        }
        [ScriptMethod(name: "P3 Zoo First Round Select Sheep", eventType: EventTypeEnum.Targetable, eventCondition: ["Targetable:True", "DataId:18344"])]
        public void P3_Zoo_FirstRoundSelectSheep(Event @event, ScriptAccessory accessory)
        {
            if (parse != 31) return;
            if (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 0) return;
            accessory.Method.TTS("Attack sheep");
            accessory.Method.SelectTarget((uint)@event.SourceId);
        }

        [ScriptMethod(name: "P3 Zoo Second Round Pull Position", eventType: EventTypeEnum.Targetable, eventCondition: ["Targetable:True", "DataId:18346"],suppress:1000)]
        public void P3_Zoo_SecondRoundPullPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myIndex != 0) return;
            Vector3 pos = new(110, 0, 95);
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3 Zoo Second Round Pull Position";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = pos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 13000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "P3 Zoo Second Round Select Fish and Initial Bait", eventType: EventTypeEnum.Targetable, eventCondition: ["Targetable:True", "DataId:18346"])]
        public void P3_Zoo_SecondRoundSelectFish(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            if (zoo2Fish == zoo2FishEnum.None) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if (pos == default) return;
            if (zoo2Fish== zoo2FishEnum.Right && pos.X>100)
            {
                accessory.Method.SelectTarget((uint)@event.SourceId);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo Second Round Fish Bait Position";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(105,0,80.5f);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (zoo2Fish == zoo2FishEnum.Left && pos.X < 100)
            {
                accessory.Method.SelectTarget((uint)@event.SourceId);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo Second Round Fish Bait Position";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(95, 0, 80.5f);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }

        [ScriptMethod(name: "P3 Zoo Third Round Pull Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42664)$"])]
        public void P3_Zoo_ThirdRoundPullPosition(Event @event, ScriptAccessory accessory)
        {
            //13000ms
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myIndex==1)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo Third Round Pull Position ST";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(100,0,90);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo Third Round Pull Position ST 2";
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Position = new(100, 0, 90);
                dp.TargetPosition = new(90, 0, 100);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo Third Round Pull Position ST 3";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(110, 0, 100);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Delay = 5000;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            

        }
        [ScriptMethod(name: "P3 Zoo Third Round Select Horse", eventType: EventTypeEnum.Targetable, eventCondition: ["Targetable:True", "DataId:18345"])]
        public void P3_Zoo_ThirdRoundSelectHorse(Event @event, ScriptAccessory accessory)
        {
            if (parse != 33) return;
            if (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 1) return;
            Task.Delay(50).ContinueWith(t =>
            {
                accessory.Method.SelectTarget((uint)@event.SourceId);
            });
            accessory.Method.TTS("Attack horse");
            
        }

        [ScriptMethod(name: "P3 Zoo Third Round Stun Horse Reminder", eventType: EventTypeEnum.Targetable, eventCondition: ["Targetable:True", "DataId:18345"])]
        public void P3_Zoo_ThirdRoundStunHorseReminder(Event @event, ScriptAccessory accessory)
        {
            if (parse != 33) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myindex == 0)
            {
                Task.Delay(1000).ContinueWith(t =>
                {
                    if (parse != 33) return;
                    accessory.Method.TTS($"Leg Sweep");
                    accessory.Method.TextInfo($"Leg Sweep", 2000);
                });
            }
            if (myindex == 4)
            {
                Task.Delay(4000).ContinueWith(t =>
                {
                    if (parse != 33) return;
                    accessory.Method.TTS($"Leg Sweep");
                    accessory.Method.TextInfo($"Leg Sweep", 2000);
                });
            }
            if (myindex == 5)
            {
                Task.Delay(7000).ContinueWith(t =>
                {
                    if (parse != 33) return;
                    accessory.Method.TTS($"Leg Sweep");
                    accessory.Method.TextInfo($"Leg Sweep", 2000);
                });
            }
        }

        [ScriptMethod(name: "P3 Zoo Fourth Round Pull Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42665)$"])]
        public void P3_Zoo_FourthRoundPullPosition(Event @event, ScriptAccessory accessory)
        {
            //13000ms
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myIndex == 1)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo Fourth Round Pull Position ST";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(100, 0, 90);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo Third Round Pull Position ST 2";
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Position = new(100, 0, 110);
                dp.TargetPosition = new(90, 0, 95);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo Third Round Pull Position ST 3";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(90, 0, 95);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Delay = 5000;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (myIndex == 0)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo Fourth Round Pull Position MT";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(100, 0, 110);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo Third Round Pull Position MT 2";
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Position = new(100, 0, 110);
                dp.TargetPosition = new(110, 0, 105);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo Third Round Pull Position MT 3";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(110, 0, 105);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Delay = 5000;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

        }
        [ScriptMethod(name: "P3 Zoo Fourth Round Select Fish and Initial Bait", eventType: EventTypeEnum.Targetable, eventCondition: ["Targetable:True", "DataId:18346"])]
        public void P3_Zoo_FourthRoundSelectFish(Event @event, ScriptAccessory accessory)
        {
            if (parse != 34) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myindex != 6 && myindex != 7) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if (pos == default) return;
            if (myindex==7 && pos.X > 100)
            {
                accessory.Method.SelectTarget((uint)@event.SourceId);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo Fourth Round Fish Bait Position";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(118, 0, 80.5f);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (myindex == 6 && pos.X < 100)
            {
                accessory.Method.SelectTarget((uint)@event.SourceId);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 Zoo Fourth Round Fish Bait Position";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(80.5f, 0, 95f);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        [ScriptMethod(name: "P3 Zoo Fourth Round Select Horse", eventType: EventTypeEnum.Targetable, eventCondition: ["Targetable:True", "DataId:18345"])]
        public void P3_Zoo_FourthRoundSelectHorse(Event @event, ScriptAccessory accessory)
        {
            if (parse != 34) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myindex == 1 || myindex == 4 || myindex == 5)
            {
                Task.Delay(50).ContinueWith(t =>
                {
                    accessory.Method.SelectTarget((uint)@event.SourceId);
                });
                accessory.Method.TTS("Attack horse");
            }
            

        }
        [ScriptMethod(name: "P3 Zoo Fourth Round Stun Horse Reminder", eventType: EventTypeEnum.Targetable, eventCondition: ["Targetable:True", "DataId:18345"])]
        public void P3_Zoo_FourthRoundStunHorseReminder(Event @event, ScriptAccessory accessory)
        {
            if (parse != 34) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myindex == 1)
            {
                Task.Delay(1000).ContinueWith(t =>
                {
                    if (parse != 34) return;
                    accessory.Method.TTS($"Leg Sweep");
                    accessory.Method.TextInfo($"Leg Sweep", 2000);
                });
            }
            if (myindex == 0)
            {
                Task.Delay(4000).ContinueWith(t =>
                {
                    if (parse != 34) return;
                    accessory.Method.TTS($"Leg Sweep");
                    accessory.Method.TextInfo($"Leg Sweep", 2000);
                });
            }
            if (myindex == 4)
            {
                Task.Delay(7000).ContinueWith(t =>
                {
                    if (parse != 34) return;
                    accessory.Method.TTS($"Leg Sweep");
                    accessory.Method.TextInfo($"Leg Sweep", 2000);
                });
            }
            if (myindex == 5)
            {
                Task.Delay(9000).ContinueWith(t =>
                {
                    if (parse != 34) return;
                    accessory.Method.TTS($"Leg Sweep");
                    accessory.Method.TextInfo($"Leg Sweep", 2000);
                });
            }
        }

        [ScriptMethod(name: "P3 Zoo Ending Sword Tether", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^((013F)|(0140))",])]
        public void SwordTether(Event @event, ScriptAccessory accessory)
        {
            //if(parse!=34) return;

            var sobj= accessory.Data.Objects.SearchById(@event.SourceId);
            if(sobj == null) return;
            if (sobj.DataId != 18338) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3 Zoo Ending Sword Tether";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(7,45);
            dp.Owner = @event.SourceId;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Vfx, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "P4 Mountain Phase Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42595"], userControl: false)]
        public void P4_PhaseChange(Event @event, ScriptAccessory accessory)
        {
            parse = 40;
        }
        [ScriptMethod(name: "P4 Mountain Pull Indicator", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42595"])]
        public void P4_Mountain_PullIndicator(Event @event, ScriptAccessory accessory)
        {
            //13000ms
            if (accessory.Data.PartyList.IndexOf(accessory.Data.Me) != 0) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P4 Mountain Pull Indicator";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = new(90,0,108);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 13000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            
        }

        [ScriptMethod(name: "P4 Splat Mousse Pair Stack", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:013C"])]
        public void P4_SplatMousse(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P4 Splat Mousse Pair Stack";
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = @event.TargetId;
            dp.DestoryAt = 12000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P4 Mousse Charge Near Four-Player Cone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42682)$"])]
        public void P4_MousseCharge(Event @event, ScriptAccessory accessory)
        {
            for (int i = 0; i < 4; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P4 Mousse Charge Near Four-Player Cone {i}";
                dp.Scale = new(60);
                dp.Radian = MathF.PI / 4;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = @event.SourceId;
                dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                dp.TargetOrderIndex = (uint)i+1;
                dp.DestoryAt = 4500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
        }
        [ScriptMethod(name: "P4 Mousse Charge Near Four-Player Cone Bait Reminder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42682)$"])]
        public void P4_MousseChargeBaitReminder(Event @event, ScriptAccessory accessory)
        {
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if(myindex==2 || myindex==3 || myindex== 6 || myindex == 7) return;
            accessory.Method.TTS($"Bait outside tower");
            accessory.Method.TextInfo($"Bait outside tower", 6000);
        }
        [ScriptMethod(name: "P4 Candy Flash Thunder Featherlance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42653)$"])]
        public void P4_CandyFlashThunder(Event @event, ScriptAccessory accessory)
        {
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P4 Candy Flash Thunder Featherlance";
            dp.Scale = new(3);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P4 Candy Flash Thunder Featherlance Tower Reminder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42653)$"],suppress:1000)]
        public void P4_FeatherlanceTowerReminder(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TTS($"Tower");
            accessory.Method.TextInfo($"Tower", 5000);
        }
        [ScriptMethod(name: "P4 Volcano Flight Prediction", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4450"])]
        public void P4_VolcanoFlightPrediction(Event @event, ScriptAccessory accessory)
        {
            if (parse != 40) return;
            if (@event.TargetId != accessory.Data.Me) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P4_VolcanoFlightPrediction";
            dp.Scale = new(3, 34);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P4 Volcano Flight Countdown", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4450"])]
        public void P4_VolcanoFlightCountdown(Event @event, ScriptAccessory accessory)
        {
            if (parse != 40) return;
            if (@event.TargetId != accessory.Data.Me) return;

            Task.Delay(5000).ContinueWith(t =>
            {
                if (parse != 40) return;
                accessory.Method.TextInfo($"5 seconds until flight", 5000);
                accessory.Method.TTS("5");
            });
            Task.Delay(6000).ContinueWith(t =>
            {
                if (parse != 40) return;
                accessory.Method.TTS("4");
            });
            Task.Delay(7000).ContinueWith(t =>
            {
                if (parse != 40) return;
                accessory.Method.TTS("3");
            });
            Task.Delay(8000).ContinueWith(t =>
            {
                if (parse != 40) return;
                accessory.Method.TTS("2");
            });
            Task.Delay(9000).ContinueWith(t =>
            {
                if (parse != 40) return;
                accessory.Method.TTS("1");
            });

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
        private Vector3 RotatePoint(Vector3 point, Vector3 centre, float radian)
        {

            Vector2 v2 = new(point.X - centre.X, point.Z - centre.Z);

            var rot = (MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian);
            var lenth = v2.Length();
            return new(centre.X + MathF.Sin(rot) * lenth, centre.Y, centre.Z - MathF.Cos(rot) * lenth);
        }
    }
}
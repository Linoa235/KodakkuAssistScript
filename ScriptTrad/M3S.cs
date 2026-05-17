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
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;

namespace KarlinScriptNamespace
{
    [ScriptType(name: "M3s Drawing", territorys:[1230],guid: "a7e12eeb-4f05-4b68-8d4f-f64e08b6d7a5", version:"0.0.0.9", author: "Karlin",updateInfo:updateInfoStr)]
    public class M3sDrawing
    {
        const string updateInfoStr =
        """
        Fixed P2 ground bomb processing position issues caused by plugin updates
        """;
        [UserSetting("Arrange fuse positions according to TNTN order")]
        public bool TNTN_Fuse { get; set; } =false;

        [UserSetting("P2 Ground Bomb Spread Method")]
        public P2BombDealEnum P2BombDealType { get; set; }


        int? firstTargetIcon = null;

        int parse;
        int chainObjPos4Dir;
        Vector3 chargeSafePos = default;
        bool[] isLongFuse = [false, false, false, false, false, false, false, false];
        bool[] isLongBoom = [false, false, false, false, false, false, false, false];
        (uint, uint)[] dir8obj = [(0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)];
        bool[] isLongFieldFuse = [false, false, false, false, false, false, false, false];
        int boomIndex = -1;

        public enum P2BombDealEnum
        {
            Hector,
            MMW
        }
        public void Init(ScriptAccessory accessory)
        {
            firstTargetIcon = null;
            parse = 0;
            chargeSafePos = default;
            //accessory.Method.MarkClear();
            boomIndex = -1;

        }

        [ScriptMethod(name: "Phase Split", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37895|37927)$"],userControl:false)]
        public void PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse++;
            chargeSafePos = default;
        }

        [ScriptMethod(name: "AOE Reminder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37925)$"])]
        public void AoeReminder(Event @event, ScriptAccessory accessory)
        {

            
            var count = parse switch
            {
                0=>4,
                1=>6,
                2=>8,
                _=>4
            };
            accessory.Method.TextInfo($"{count}-combo AOE", 4000 + count * 1100);
            accessory.Method.TTS($"{count}-combo AOE");


        }
        [ScriptMethod(name: "Stack Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37923)$"])]
        public void StackTankbuster(Event @event, ScriptAccessory accessory)
        {
            
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var during = 8500;
            switch (parse)
            {
                case 0:
                    during = 8500;
                    break;
                case 1:
                    during = 10500;
                    break;
                case 2:
                    during = 12500;
                    break;
                default:
                    break;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Stack Tankbuster";
            dp.Scale = new(6);
            dp.Color = (index == 0 || index == 1) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = during;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "Heavy/Donut Stack/Spread", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(378(48|49|50|51))$"])]
        public void HeavyDonutStackSpread(Event @event, ScriptAccessory accessory)
        {
            //37848 Heavy Spread
            //37849 Donut Spread
            //37850 Heavy Stack
            //37851 Donut Stack
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            int[] group= [6, 5, 4, 7, 2, 1, 0, 3];
            DrawPropertiesEdit dp;
            if (@event["ActionId"] == "37848" || @event["ActionId"] == "37850")
            {
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"HeavyDonut+StackSpread-Heavy";
                dp.Scale = new(10);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = sid;
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            if (@event["ActionId"] == "37849" || @event["ActionId"] == "37851")
            {
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"HeavyDonut+StackSpread-Donut";
                dp.Scale = new(40);
                dp.InnerScale = new(10);
                dp.Radian = float.Pi * 2;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = sid;
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
            }

            if (@event["ActionId"] == "37848" || @event["ActionId"] == "37849")
            {
                for (int i = 0; i < 8; i++)
                {
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"HeavyDonut+StackSpread-Spread";
                    dp.Scale = new(40);
                    dp.Radian = float.Pi /4;
                    dp.Owner = sid;
                    dp.TargetObject = accessory.Data.PartyList[i];
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
                    dp.DestoryAt = 7000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                }
                accessory.Method.TextInfo("8-direction Spread",7000,true);
                accessory.Method.TTS("8-direction Spread");
            }
            if (@event["ActionId"]== "37850"|| @event["ActionId"] == "37851")
            {
                for (int i = 0; i < 4; i++)
                {
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"HeavyDonut+StackSpread-Stack";
                    dp.Scale = new(40);
                    dp.Radian = float.Pi / 8;
                    dp.Owner = sid;
                    dp.TargetObject = accessory.Data.PartyList[i];
                    dp.Color = (i == index || i == group[index]) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 7000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                }
                accessory.Method.TextInfo("4-corner Stack", 7000);
                accessory.Method.TTS("4-corner Stack");
            }

            
        }
        [ScriptMethod(name: "Meteor", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(378(68|77))$"])]
        public void Meteor(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Meteor+StackSpread-Meteor";
            dp.Scale = new(22);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "Knockback", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(378(69|78))$"])]
        public void Knockback(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Knockback+StackSpread-Knockback";
            dp.Scale = new(2, 25);
            dp.Color = accessory.Data.DefaultSafeColor.WithW(3);
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = sid;
            dp.Rotation = float.Pi;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "Meteor/Knockback Stack/Spread", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(378(54|55|56|57))$"])]
        public void MeteorKnockbackStackSpread(Event @event, ScriptAccessory accessory)
        {
            //37854 Meteor Spread
            //37855 Knockback Spread
            //37856 Meteor Stack
            //37857 Knockback Stack
            
            DrawPropertiesEdit dp;
            if (@event["ActionId"] == "37854" || @event["ActionId"] == "37855")
            {
                for (int i = 0; i < 8; i++)
                {
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"MeteorStackSpread-Spread";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 8000;
                    dp.DestoryAt = 3000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                accessory.Method.TextInfo("Spread soon", 11000);
                accessory.Method.TTS("Spread soon");

            }
            if (@event["ActionId"] == "37856" || @event["ActionId"] == "37857")
            {
                var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                int[] group = [4, 5, 6, 7, 0, 1, 2, 3];
                for (int i = 0; i < 4; i++)
                {
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"MeteorStackSpread-Stack";
                    dp.Scale = new(5);
                    dp.Radian = float.Pi / 3;
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = (i == index || i == group[index]) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.Delay = 8000;
                    dp.DestoryAt = 3000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                accessory.Method.TextInfo("Stack soon", 11000);
                accessory.Method.TTS("Stack soon");
            }



        }

        [ScriptMethod(name: "Knockback Tower", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00020004", "Index:regex:^(0000000[56])$"])]
        public void KnockbackTower(Event @event, ScriptAccessory accessory)
        {
            //00000005 AC
            //00000006 BD
            var isAC = @event["Index"] == "00000005";
            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            var radian1 = 18.72f / 180f * float.Pi;
            var radian2 = 29.80f / 180f * float.Pi;
            var radian3 = 30.81f / 180f * float.Pi;

            var dur1 = 10000;
            var dur2 = 3000;
            var dur3 = 3000;
            var dur4 = 3000;

            Vector3 centre=new(100,0,100);
            Vector3 tPoint1 = default;
            Vector3 tPoint2 = default;

            if (isAC)
            {
                if (index == 0 || index == 1 || index == 4 || index == 5) 
                {
                    tPoint1 = new(100, 0, 89);
                    if (index == 0 || index == 4)
                    {
                        tPoint2 = new(111, 0, 111);
                    }
                    else
                    {
                        tPoint2 = new(89, 0, 111);
                    }
                }
                if (index == 2 || index == 3 || index == 6 || index == 7)
                {
                    tPoint1 = new(100, 0, 111);
                    if (index == 2 || index == 6)
                    {
                        tPoint2 = new(89, 0, 89);
                    }
                    else
                    {
                        tPoint2 = new(111, 0, 89);
                    }
                }
            }
            else
            {
                if (index == 0 || index == 1 || index == 4 || index == 5)
                {
                    tPoint1 = new(89, 0, 100);
                    if (index == 0 || index == 4)
                    {
                        tPoint2 = new(111, 0, 89);
                    }
                    else
                    {
                        tPoint2 = new(111, 0, 111);
                    }
                }
                if (index == 2 || index == 3 || index == 6 || index == 7)
                {
                    tPoint1 = new(111, 0, 100);
                    if (index == 2 || index == 6)
                    {
                        tPoint2 = new(89, 0, 111);
                    }
                    else
                    {
                        tPoint2 = new(89, 0, 89);
                    }
                }
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Knockback Tower-1";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Position = tPoint1;
            dp.TargetPosition = tPoint2;
            dp.Radian = radian1;
            dp.DestoryAt = dur1;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Knockback Tower-1 Position";
            dp.Scale = new(2);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = tPoint1;
            dp.DestoryAt = dur1;
            dp.ScaleMode |= ScaleMode.YByDistance;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Knockback Tower-2";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Position = tPoint2;
            dp.TargetPosition = centre;
            dp.Radian = radian2;
            dp.DestoryAt = dur1 + dur2;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);

        }
        [ScriptMethod(name: "Center Knockback Tower", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37886"])]
        public void CenterKnockbackTower(Event @event, ScriptAccessory accessory)
        {
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);

            var radian3 = 30.81f / 180f * float.Pi;

            Vector3 centre = new(100, 0, 100);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Knockback Tower-3";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Position = centre;
            dp.TargetPosition = pos;
            dp.Radian = radian3;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
        }
        [ScriptMethod(name: "Center Knockback Tower Corner AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37886"])]
        public void CenterKnockbackTowerCornerAoe(Event @event, ScriptAccessory accessory)
        {

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Center Knockback Tower Corner AOE";
            dp.Scale = new(34);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Radian = float.Pi / 2 * 3;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Chain Collection", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4019"],userControl:false)]
        public void ChainCollection(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if((pos-new Vector3(100,0,100)).Length()<1) return;
            chainObjPos4Dir = PositionRoundTo4Dir(pos , new(100, 0, 100));
        }
        [ScriptMethod(name: "P2 Clone Edge Punch", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3972[46])$"])]
        public void P2CloneEdgePunch(Event @event, ScriptAccessory accessory)
        {
            //39724 Right
            //39725
            //39726 Left
            //39727
            
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var rightHand = @event["ActionId"] == "39724";
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            
            var pos4dir = PositionRoundTo4Dir(pos, new(100, 0, 100));

            float.TryParse(@event["SourceRotation"], out var rotation);
            Vector3 backPos = default;
            if (pos4dir == 0) backPos = rightHand ? new(105, 0, 115) : new(95, 0, 115);
            if (pos4dir == 1) backPos = rightHand ? new(85, 0, 105) : new(85, 0, 95);
            if (pos4dir == 2) backPos = rightHand ? new(95, 0, 85) : new(105, 0, 85);
            if (pos4dir == 3) backPos = rightHand ? new(115, 0, 95) : new(115, 0, 105);


            var dv = rightHand ? 1 : -1;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3 Edge Punch Phase 1";
            dp.Scale = pos4dir == chainObjPos4Dir ? new(10, 30) : new(20, 30);
            dp.Offset = pos4dir == chainObjPos4Dir ? new(10 * -dv, 0, 0) : new(5 * dv, 0, 0);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6100;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Edge Punch Phase 2";
            dp.Scale = new(20, 30);
            dp.Position = backPos;
            dp.Rotation= rotation+float.Pi;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 4500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2 Edge Punch Phase 2 Safe Zone Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(397(24|26))$"])]
        public void P2EdgePunchPhase2SafeZonePosition(Event @event, ScriptAccessory accessory)
        {
            //39724 Right
            //39725
            //39726 Left
            //39727

            
            var rightHand = @event["ActionId"] == "39724";
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var pos4dir = PositionRoundTo4Dir(pos, new(100, 0, 100));
            lock (this)
            {

                float.TryParse(@event["SourceRotation"], out var rotation);

                if (MathF.Abs(pos.X - 115) < 1) chargeSafePos.Z = rightHand ? 94 : 106;
                if (MathF.Abs(pos.X - 85) < 1) chargeSafePos.Z = rightHand ? 106 : 94;

                if (MathF.Abs(pos.Z - 115) < 1) chargeSafePos.X = rightHand ? 106 : 94;
                if (MathF.Abs(pos.Z - 85) < 1) chargeSafePos.X = rightHand ? 94 : 106;

                if (chargeSafePos.X == 0 || chargeSafePos.Z == 0) return;
                


                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P2 Edge Punch Phase 2 Safe Zone Position";
                dp.Scale = new(2f, 10);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition=chargeSafePos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.TargetColor = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 6100;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P2 Edge Punch Phase 2 Safe Zone Position";
                dp.Scale = new(2f, 10);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = chargeSafePos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 6100;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }

        [ScriptMethod(name: "Fuse Collection", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(402[45])$"], userControl: false)]
        public void FuseCollection(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            isLongFuse[accessory.Data.PartyList.IndexOf(tid)] = @event["StatusID"] == "4025";
        }

        [ScriptMethod(name: "Player Fuse Prompt", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(402[45])$"])]
        public void PlayerFusePrompt(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            var shortDelay = parse == 1 ? 12000 : 20800;
            var longDelay = parse == 1 ? 17000 : 25800;
            var isLong = @event["StatusID"] == "4025";
            accessory.Method.TextInfo($"{(isLong ? "Long" : "Short")} Fuse", isLong ? shortDelay : longDelay);
            accessory.Method.TTS($"{(isLong ? "Long" : "Short")} Fuse");

        }
        [ScriptMethod(name: "Player Self-Destruct Range", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(402[45])$"])]
        public void PlayerSelfDestructRange(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var isLong = @event["StatusID"] == "4025";

            var dur = 5000;
            var shortDelay = parse == 1 ? 12000 : 20800;
            var longDelay = parse == 1 ? 17000 : 25800;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Player Self-Destruct Range";
            dp.Scale = new(6);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.Delay = isLong? longDelay-dur: shortDelay- dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "Ground Bomb Explosion Range", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:17095"])]
        public void GroundBombExplosionRange(Event @event, ScriptAccessory accessory)
        {
            
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var obj= accessory.Data.Objects.SearchByEntityId(sid);
            if(obj == null) return;
            var statusCount= ((IBattleChara)obj).StatusList.Where(status => status.StatusId == 4016).Count();

            var dur = 5000;
            var shortDelay =parse==1? 12000:19000;
            var longDelay = parse==1? 17000: 24000;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Ground Bomb Explosion Range";
            dp.Scale = new(8);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = statusCount>0 ? shortDelay : 0;
            dp.DestoryAt = statusCount > 0 ? dur : shortDelay;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "P2 Ground Bomb Processing Position", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:17095"])]
        public void P2GroundBombProcessingPosition(Event @event, ScriptAccessory accessory)
        {

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var obj = accessory.Data.Objects.SearchByEntityId(sid);
            if (obj == null) return;
            var statusCount = ((IBattleChara)obj).StatusList.Where(status => status.StatusId == 4016).Count();
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);

            var dur = 5000;
            var shortDelay = parse == 1 ? 12000 : 19000;
            var longDelay = parse == 1 ? 17000 : 24000;

            if (parse != 1) return;
            // The arena layout has only two configurations, check if bomb at point A is short
            var bomb_A = new Vector3(100f, 0, 92.05f);
            if (MathF.Abs((pos - bomb_A).Length()) >0.5) return;
            var bomb_A_isShort = statusCount == 0;
            var MyIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 destinationPos = default;
            if (P2BombDealType == P2BombDealEnum.Hector)
            {
                // accessory.Log.Debug($"Found bomb at point A, position = {pos}");
                // accessory.Log.Debug($"Bomb at point A is {(bomb_A_isShort? "Short" : "Long")}");
                // accessory.Log.Debug($"Party list, isLongFuse: {string.Join(", ", isLongFuse)}");
                // accessory.Log.Debug($"Me: {MyIndex}");

                if (MyIndex == 0 || MyIndex == 4)
                {
                    // Determine target position at point A
                    //     bomb_A_isShort      isLongFuse       goToA
                    //          1                   1             1
                    //          1                   0             0
                    //          0                   1             0
                    //          0                   0             1
                    bool goToA = !(bomb_A_isShort ^ isLongFuse[MyIndex]);
                    destinationPos = goToA ? new(100f, 0, 92.05f) : new(92.05f, 0, 100f);
                }
                else if (MyIndex == 1 || MyIndex == 5)
                {
                    bool goToA = !(bomb_A_isShort ^ isLongFuse[MyIndex]);
                    destinationPos = goToA ? new(107.95f, 0, 100f) : new(100f, 0, 107.95f);
                }

                else if (MyIndex == 2 || MyIndex == 6)
                {
                    // I have long fuse, top-left 1 must have short bomb, so go to 1 in second round
                    // I have short fuse, top-left 1 must have short bomb, so go to 2 in first round
                    destinationPos = isLongFuse[MyIndex] ? new(90.6f, 0, 90.6f) : new(109.4f, 0, 90.6f);
                }
                else if (MyIndex == 3 || MyIndex == 7)
                {
                    // I have long fuse, bottom-right 3 must have short bomb, so go to 3 in second round
                    // I have short fuse, bottom-right 3 must have short bomb, so go to 4 in first round
                    destinationPos = isLongFuse[MyIndex] ? new(109.4f, 0, 109.4f) : new(90.6f, 0, 109.4f);
                }

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "Savage Bomb Navigation";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = destinationPos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = isLongFuse[MyIndex] ? shortDelay : 0;
                dp.DestoryAt = isLongFuse[MyIndex] ? dur : shortDelay;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (P2BombDealType == P2BombDealEnum.MMW)
            {
                //Three long fuses in southwest
                var p1 = new Vector3(100f, 0, 110f);
                var p2 = new Vector3(90, 0, 100);
                var p3 = new Vector3(114.5f, 0, 94f);
                var p4 = new Vector3(107f, 0, 85.5f); 

                var p5 = new Vector3(107f, 0, 103.5f);
                var p6 = new Vector3(96.5f, 0, 93f);
                var p7 = new Vector3(87f, 0, 87f);
                var p8 = new Vector3(113f, 0, 113f);

                destinationPos = MyIndex switch
                {
                    0 => isLongFuse[MyIndex] ? p5 : p1,
                    1 => isLongFuse[MyIndex] ? p6 : p2,
                    2 => isLongFuse[MyIndex] ? p7 : p3,
                    3 => isLongFuse[MyIndex] ? p8 : p4,
                    4 => isLongFuse[MyIndex] ? p5 : p1,
                    5 => isLongFuse[MyIndex] ? p6 : p2,
                    6 => isLongFuse[MyIndex] ? p7 : p3,
                    7 => isLongFuse[MyIndex] ? p8 : p4,
                    _=>default,
                } ;
                accessory.Log.Debug($"{destinationPos.X:f2} {destinationPos.Z:f2}");
                if (destinationPos == default) return;
                destinationPos = bomb_A_isShort ? destinationPos: RotatePoint(destinationPos, new(100, 0, 100), float.Pi);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "Savage Bomb Navigation";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = destinationPos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = isLongFuse[MyIndex] ? shortDelay : 0;
                dp.DestoryAt = isLongFuse[MyIndex] ? dur : shortDelay;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }

        [ScriptMethod(name: "Player Short/Long Explosion Buff Collection", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4020"], userControl: false)]
        public void PlayerShortLongExplosionBuffCollection(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            int.TryParse(@event["DurationMilliseconds"], out var dur);
            isLongBoom[accessory.Data.PartyList.IndexOf(tid)] = dur > 30000;
        }
        [ScriptMethod(name: "Field Fuse Collection", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0111"], userControl: false)]
        public void FieldFuseCollection(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var tpos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
            var dir8 = PositionTo8Dir(spos, new(100,0,100));
            dir8obj[dir8] = (sid, tid);
            isLongFieldFuse[dir8] = (spos - tpos).Length() > 7;
        }
        [ScriptMethod(name: "Field Fuse Assignment", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37871"])]
        public void FieldFuseAssignment(Event @event, ScriptAccessory accessory)
        {
            List<int> tnGroup = TNTN_Fuse ? [0, 2, 1, 3] : [0, 1, 2, 3];
            List<int> dpsGroup = [4, 5, 6, 7];
            var myPartyIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var myFuseIndex = tnGroup.IndexOf(myPartyIndex);
            myFuseIndex = myFuseIndex == -1 ? dpsGroup.IndexOf(myPartyIndex) : myFuseIndex;
            var meIsLongBoom = isLongBoom[myPartyIndex];
            var fuseIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                if (isLongFieldFuse[i]== meIsLongBoom)
                {
                    if(fuseIndex == myFuseIndex)
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"Field Fuse Assignment";
                        dp.Scale = new(10);
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.Owner = dir8obj[i].Item1;
                        dp.TargetObject = dir8obj[i].Item2;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.DestoryAt = meIsLongBoom ? 44000 : 26000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
                    }
                    fuseIndex++;
                }
            }

        }
        [ScriptMethod(name: "Field Fuse Collision Prompt", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3787[25])$", "TargetIndex:1"])]
        public void FieldFuseCollisionPrompt(Event @event, ScriptAccessory accessory)
        {
            List<int> tnGroup = TNTN_Fuse ? [0, 2, 1, 3] : [0, 1, 2, 3];
            List<int> dpsGroup = [4, 5, 6, 7];

            var myPartyIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var myFuseIndex = tnGroup.IndexOf(myPartyIndex);
            myFuseIndex = myFuseIndex == -1 ? dpsGroup.IndexOf(myPartyIndex) : myFuseIndex;
            myFuseIndex = isLongBoom[myPartyIndex] ? myFuseIndex + 4 : myFuseIndex;
            boomIndex++;
            if (myFuseIndex == boomIndex) 
            {
                Task.Delay(2000).ContinueWith(t =>
                {
                    accessory.Method.TextInfo("Collide Fuse", 2000);
                    accessory.Method.TTS("Collide Fuse");
                });
            }

        }


        //37898 Stack
        //38738 Spread

        //37904 Heavy
        //37905 Donut
        //37908 Knockback
        [ScriptMethod(name: "P3 Combo Heavy/Donut/Knockback", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37898|38738)$"])]
        public void P3ComboHeavyDonutKnockback(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3 Combo Heavy";
            dp.Scale = new(10);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 15800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3 Combo Donut";
            dp.Scale = new(40);
            dp.InnerScale = new(6);
            dp.Radian = float.Pi * 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = 15800;
            dp.DestoryAt = 2500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = $"P3 Combo Knockback";
            dp2.Scale = new(1.5f,10);
            dp2.Color = accessory.Data.DefaultDangerColor.WithW(3);
            dp2.Owner = accessory.Data.Me;
            dp2.TargetObject = sid;
            dp2.Rotation = float.Pi;
            dp2.Delay = 18300;
            dp2.DestoryAt = 8200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp2);

        }

        [ScriptMethod(name: "P3 Combo Spread/Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37898|38738)$"])]
        public void P3ComboSpreadStack(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var actionId = @event["ActionId"];

            DrawPropertiesEdit dp;
            if (@event["ActionId"] == "38738")
            {
                for (int i = 0; i < 8; i++)
                {
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"P3 Combo-Spread";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 26500;
                    dp.DestoryAt = 3500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                accessory.Method.TextInfo("Spread soon", 26500);
                accessory.Method.TTS("Spread soon");
                Task.Delay(26500).ContinueWith(t =>
                {
                    accessory.Method.TextInfo("Spread", 3500,true);
                    accessory.Method.TTS("Spread");
                });
            }
            if (@event["ActionId"] == "37898")
            {
                var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                int[] group = [6, 5, 4, 7, 2, 1, 0, 3];
                for (int i = 0; i < 4; i++)
                {
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"P3 Combo-Stack";
                    dp.Scale = new(5);
                    dp.Radian = float.Pi / 3;
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = (i == index || i == group[index]) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.Delay = 26500;
                    dp.DestoryAt = 3500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                accessory.Method.TextInfo("Stack soon", 26500);
                accessory.Method.TTS("Stack soon");
                Task.Delay(26500).ContinueWith(t =>
                {
                    accessory.Method.TextInfo("Pair Stack", 3500, true);
                    accessory.Method.TTS("Pair Stack");
                });
            }




        }

        

        [ScriptMethod(name: "P3 Clone Edge Punch", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3989[68])$"])]
        public void P3CloneEdgePunch(Event @event, ScriptAccessory accessory)
        {
            //39896 Right
            //39897
            //39898 Left
            //39899

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var rightHand = @event["ActionId"] == "39896";
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);

            var pos4dir = PositionRoundTo4Dir(pos, new(100, 0, 100));

            float.TryParse(@event["SourceRotation"], out var rotation);
            Vector3 backPos = default;
            if (pos4dir == 0) backPos = rightHand ? new(105, 0, 115) : new(95, 0, 115);
            if (pos4dir == 1) backPos = rightHand ? new(85, 0, 105) : new(85, 0, 95);
            if (pos4dir == 2) backPos = rightHand ? new(95, 0, 85) : new(105, 0, 85);
            if (pos4dir == 3) backPos = rightHand ? new(115, 0, 95) : new(115, 0, 105);


            var dv = rightHand ? 1 : -1;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3 Edge Punch Phase 1";
            dp.Scale = pos4dir == chainObjPos4Dir? new(10, 30) : new(20, 30);
            dp.Offset = pos4dir == chainObjPos4Dir ? new(10*-dv, 0, 0) : new(5*dv, 0, 0);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 8100;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);





            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3 Edge Punch Phase 2";
            dp.Scale = new(20, 30);
            dp.Position = backPos;
            dp.Rotation = rotation + float.Pi;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 8000;
            dp.DestoryAt = 4500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "P3 Edge Punch Phase 2 Safe Zone Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3989[68])$"])]
        public void P3EdgePunchPhase2SafeZonePosition(Event @event, ScriptAccessory accessory)
        {
            //39896 Right
            //39897
            //39898 Left
            //39899


            var rightHand = @event["ActionId"] == "39896";
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var pos4dir = PositionRoundTo4Dir(pos, new(100, 0, 100));
            lock (this)
            {

                float.TryParse(@event["SourceRotation"], out var rotation);

                if (MathF.Abs(pos.X - 115) < 1) chargeSafePos.Z = rightHand ? 94 : 106;
                if (MathF.Abs(pos.X - 85) < 1) chargeSafePos.Z = rightHand ? 106 : 94;

                if (MathF.Abs(pos.Z - 115) < 1) chargeSafePos.X = rightHand ? 106 : 94;
                if (MathF.Abs(pos.Z - 85) < 1) chargeSafePos.X = rightHand ? 94 : 106;


                if (chargeSafePos.X == 0 || chargeSafePos.Z == 0) return;



                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P2 Edge Punch Phase 2 Safe Zone Position";
                dp.Scale = new(2f, 10);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = chargeSafePos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.TargetColor = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 8100;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P2 Edge Punch Phase 2 Safe Zone Position";
                dp.Scale = new(2f, 10);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = chargeSafePos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 8100;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }

        [ScriptMethod(name: "P3 Edge Punch Center Cone Danger Zone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39895"])]
        public void P3EdgePunchCenterConeDangerZone(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3 Edge Punch Center Cone Danger Zone";
            dp.Scale = new(21.2f);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp.Owner = sid;
            dp.Rotation = float.Pi;
            dp.Radian = float.Pi / 2;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "P3 Knockback Tower Punch", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3966[4567])$"])]
        public void P3KnockbackTowerPunch(Event @event, ScriptAccessory accessory)
        {
            //39664 Right
            //39665 Left
            //39666 Right
            //39667 Left
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var rightHand = @event["ActionId"] == "39664" || @event["ActionId"] == "39666";
            var dur = @event["ActionId"] == "39664" || @event["ActionId"] == "39665" ? 6100 : 3100;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3 Knockback Tower Punch Phase 1";
            dp.Scale = new(20, 30);
            dp.Offset = rightHand ? new(5, 0, 0) : new(-5, 0, 0);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "P3 Center Knockback Tower", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3966[45])$"])]
        public void P3CenterKnockbackTower(Event @event, ScriptAccessory accessory)
        {
            
            //39664 Right
            //39665 Left
            //39666 Right
            //39667 Left

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3 Center Knockback Tower";
            dp.Scale = new(1.5f, 15);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = new(100, 0, 100);
            dp.Rotation = float.Pi;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            var rightHand = @event["ActionId"] == "39664";
            float.TryParse(@event["SourceRotation"], out var rotation);
            var r = 22.74f / 180 * float.Pi;
            var rot = 45.06f / 180 * float.Pi;
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3 Center Knockback Tower Position";
            dp.Scale = new(2);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Position = new(100,0,100);
            dp.Radian = r;
            dp.Rotation = rightHand ? rotation + rot : rotation - rot;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
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
        private Vector3 RotatePoint(Vector3 point, Vector3 centre, float radian)
        {

            Vector2 v2 = new(point.X - centre.X, point.Z - centre.Z);

            var rot = (MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian);
            var lenth = v2.Length();
            return new(centre.X + MathF.Sin(rot) * lenth, centre.Y, centre.Z - MathF.Cos(rot) * lenth);
        }

        /// <summary>
        /// Round down
        /// </summary>
        /// <param name="point"></param>
        /// <param name="centre"></param>
        /// <returns></returns>
        private int PositionFloorTo4Dir(Vector3 point, Vector3 centre)
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r = Math.Floor(2 - 2 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 4;
            return (int)r;

        }

        /// <summary>
        /// Round to nearest direction
        /// </summary>
        /// <param name="point"></param>
        /// <param name="centre"></param>
        /// <returns></returns>
        private int PositionRoundTo4Dir(Vector3 point, Vector3 centre)
        {
            
            var r = Math.Round(2 - 2 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 4;
            return (int)r;
        }

        /// <summary>
        /// Round to nearest direction
        /// </summary>
        /// <param name="point"></param>
        /// <param name="centre"></param>
        /// <returns></returns>
        private int PositionTo8Dir (Vector3 point, Vector3 centre) 
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r= Math.Round(4 - 4 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 8;
            return (int)r;
            
        }
        private int PositionTo12Dir(Vector3 point, Vector3 centre)
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r = Math.Round(6 - 6 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 12;
            return (int)r;

        }
    }
}
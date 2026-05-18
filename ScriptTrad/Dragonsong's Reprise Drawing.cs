// File: DragongSingDraw_Karlin.cs
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

namespace KarlinScriptNamespace
{
    [ScriptType(name:"Dragonsong's Reprise Drawing", territorys: [968], guid: "fa48c9f9-cd68-4216-9cd4-75ab41c58b55", version:"0.0.0.6", author: "Linoa235")]
    public class DragongSingDraw
    {
        
        [UserSetting("P5 First Mechanic Tether Charge Display Delay (ms)")]
        public int p5TetherCrashDelay { get; set; } = 3000;

        [UserSetting("P6 Spread/Stack Marking")]
        public bool p6Mark {  get; set; }=false;

        [UserSetting("P7 Death Cycle 116 Stack")]
        public bool p7_116 { get; set; } = true;

        object lockObj=new object();
        
        bool p1Charge=false;
        bool p3TowerDeal = false;
        bool p5Deal = false;
        
        int? firstTargetIcon = null;
        uint p1GrenoId = 0;
        uint p1AdelId = 0;
        uint p3BossId = 0;
        uint p6FireBallCount;
        uint p6FireBallCount2;
        uint tordanId = 0;
        uint darkDragonId = 0;
        uint whiteDragonId = 0;

        double parse = 0;
        Vector3 p2AdelPos = Vector3.Zero;
        Vector3 p2ZPos = Vector3.Zero;
        Vector3 p5DivePos = Vector3.Zero;
        Vector3 p5GrenoPos = Vector3.Zero;
        Vector3 p5GreekPos = Vector3.Zero;
        Vector3 p6WhitePos = Vector3.Zero;
        Vector3 p7Stone1 = Vector3.Zero;
        Vector3 p7Stone2 = Vector3.Zero;
        Dictionary<string, HashSet<uint>> p3majong=new Dictionary<string, HashSet<uint>>();
        List<uint> p2BlueCircle = [];
        List<int> p1sony = [];
        List<bool> p2SafeDir = [];
        List<bool> p2Stone = [];
        List<bool> p2Tower = [];
        List<bool> p3Boom = [];
        List<int> p3Tower = [];
        List<int> p2StoneTeam = [];
        List<int> p5sony = [];
        List<int> p6tether = [];
        List<int> p6lightDark = [];

        (int, int) p2Jump = (-1,-1);
        (int, int) p2StoneMem = (-1, -1);

        


        public void Init(ScriptAccessory accessory)
        {
            parse = 0;

            p6FireBallCount = 0;
            p6FireBallCount2 = 0;

            firstTargetIcon =null;
            p1Charge = false;
            p3TowerDeal = false;
            p5Deal = false;

            p3majong =new Dictionary<string, HashSet<uint>>();
            p5DivePos = Vector3.Zero;
            p5GrenoPos = Vector3.Zero;
            p5GreekPos = Vector3.Zero;
            p5sony = [0, 0, 0, 0, 0, 0, 0, 0];
            p1sony = [0, 0, 0, 0, 0, 0, 0, 0];
            p3Tower = [0,0,0,0];
            p6tether = [0, 0, 0, 0, 0, 0, 0, 0];
            p6lightDark= [0, 0, 0, 0, 0, 0, 0, 0];
            p2BlueCircle = [];
            p2SafeDir = [true, true, true, true, true, true, true, true];
            p2Stone = [false, false, false, false, false, false, false, false];
            p2Tower = [false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false];
            p3Boom = [false,false,false,false];
            p2Jump = (-1, -1);

            accessory.Method.MarkClear();

        }

        #region P1
        [ScriptMethod(name: "P1 BossId", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:28532"],userControl:false)]
        public void P1_BossId(Event @event, ScriptAccessory accessory)
        {
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                p1GrenoId=sid;
            }
        }
        [ScriptMethod(name: "P1 Phase Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25300"],userControl:false)]
        public void P1_PhaseRecord(Event @event, ScriptAccessory accessory)
        {
            if(parse==0) { parse = 1; }
            parse = Math.Round(parse + 0.1, 1);
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                p1AdelId = sid;
            }
            
        }
        [ScriptMethod(name: "P1 Circle",eventType: EventTypeEnum.StartCasting,eventCondition: ["ActionId:25307"])]
        public void P1_Circle(Event @event, ScriptAccessory accessory)
        {
            var dp=accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(6);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"],out var sid))
            {
                dp.Owner= sid;
            }
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
        }
        [ScriptMethod(name: "P1 Donut", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25306"])]
        public void P1_Donut(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(70);
            dp.InnerScale = new(6);
            dp.Radian = float.Pi * 2;
            dp.Color=accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }

        [ScriptMethod(name: "P1 Firmament Fire", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25309"])]
        public void P1_FirmamentFire(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_FirmamentFire";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }

        [ScriptMethod(name: "P1 Line Multi-Dimensional Slash", eventType: EventTypeEnum.TargetIcon)]
        public void P1_LineMultiDimensionalSlash(Event @event, ScriptAccessory accessory)
        {
            if (parse <1|| parse>=2) return;
            if (ParsTargetIcon(@event["Id"]) != 0) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
           
            dp.Scale = new(8,70);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = p1GrenoId;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.TargetObject = tid;
            }
            dp.DestoryAt = 6000;
            dp.Name = $"P1_LineMultiDimensionalSlash{tid:X}";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "P1 Dimensional Rift Danger Zone", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:13071"])]
        public void P1_DimensionalRiftDangerZone(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(9);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.DestoryAt = 60000;
            dp.Name = $"P1_DimensionalRiftDangerZone{id:X}";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P1 Dimensional Rift Danger Zone Removal", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:13071"],userControl:false)]
        public void P1_DimensionalRiftDangerZoneRemoval(Event @event, ScriptAccessory accessory)
        {
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                accessory.Method.RemoveDraw($"P1_DimensionalRiftDangerZone{id:X}");
            }
        }

        [ScriptMethod(name: "P1 Lightblade Adelfell Position (ImGui)", eventType: EventTypeEnum.Targetable, eventCondition: ["Targetable:True"])]
        public void P1_LightbladeAdelfellPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse!=1.1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (sid != p1AdelId) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_LightbladeAdelfellPosition";
            dp.TargetObject = sid;
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);


        }
        [ScriptMethod(name: "P1 Lightblade (Ifrit Charge)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25294"])]
        public void P1_Lightblade(Event @event, ScriptAccessory accessory)
        {
            if (p1Charge) return;
            p1Charge = true;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(9);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7000;

            if (float.TryParse(@event["SourceRotation"],out var r))
            {
                if(MathF.Abs(r+float.Pi/4)<0.1 || MathF.Abs(r - float.Pi *0.75f) < 0.1)
                {
                    dp.Name = "P1_Lightblade(111.00,111.00)";
                    dp.Position = new(111, 0, 111);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    dp.Name = "P1_Lightblade(89.00,89.00)";
                    dp.Position = new(89, 0, 89);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (MathF.Abs(r - float.Pi / 4) < 0.1 || MathF.Abs(r + float.Pi * 0.75f) < 0.1)
                {
                    dp.Name = "P1_Lightblade(111.00,89.00)";
                    dp.Position = new(111, 0, 89);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    dp.Name = "P1_Lightblade(89.00,111.00)";
                    dp.Position = new(89, 0, 111);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }

            dp.Name = "P1_Lightblade(78.00,100.00)";
            dp.Position = new(78, 0, 100);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Name = "P1_Lightblade(92.52,100.00)";
            dp.Position = new(92.52f, 0, 100);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Name = "P1_Lightblade(107.48,100.00)";
            dp.Position = new(107.48f, 0, 100);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Name = "P1_Lightblade(122.00,100.00)";
            dp.Position = new(122, 0, 100);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Name = "P1_Lightblade(100.00,78.00)";
            dp.Position = new(100, 0, 78);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Name = "P1_Lightblade(100,92.52.00)";
            dp.Position = new(100, 0, 92.52f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Name = "P1_Lightblade(100.00,107.48)";
            dp.Position = new(100, 0, 107.48f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Name = "P1_Lightblade(100.00,122.00)";
            dp.Position = new(100, 0, 122);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "P1 Light Orb Explosion Range Removal", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25295"], userControl: false)]
        public void P1_LightOrbExplosionRangeRemoval(Event @event, ScriptAccessory accessory)
        {
            if (parse > 2) return;
            var pos= JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var name = $"P1_Lightblade\\({pos.X:f2},{pos.Z:f2}\\)";
            accessory.Method.RemoveDraw(name);
        }
        [ScriptMethod(name: "P1 Knockback Prediction", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25308"])]
        public void P1_KnockbackPrediction(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(1.5f,16);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp.Owner = accessory.Data.Me;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.TargetObject = sid;
            }
            dp.Rotation = float.Pi;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P1 Sony Record", eventType: EventTypeEnum.TargetIcon, userControl: false)]
        public void P1_SonyRecord(Event @event, ScriptAccessory accessory)
        {
            
            if (parse != 1.2) return;
            var sony = ParsTargetIcon(@event["Id"]) - 47;
            if (sony < 0 || sony > 3) return;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                var index = accessory.Data.PartyList.ToList().IndexOf(id);
                p1sony[index] = sony;
            }
        }
        [ScriptMethod(name: "P1 Sony Knockback Position (ImGui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25308"])]
        public void P1_SonyKnockbackPosition(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"parse{parse}");
            if (parse !=1.2) return;
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Scale = new(1);
            dp.Owner = accessory.Data.Me;
            dp.DestoryAt = 4000;
            dp.ScaleMode |= ScaleMode.YByDistance;

            var index = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            
                
            var cpos = new Vector3(100, 0, 100);
            var npos = new Vector3(100, 0, 96);
            
            //â—‹
            if (p1sony[index] == 0)
            {
                var p1= RotatePoint(npos, cpos, float.Pi / 2);
                var p2= RotatePoint(npos, cpos, float.Pi / -2);
                
                dp.Name= "P1_Sonyâ—‹1";
                dp.TargetPosition = p1;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                dp.Name = "P1_Sonyâ—‹2";
                dp.TargetPosition = p2;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            //â–½
            if (p1sony[index] == 1)
            {
                if(index==2||index==3)
                {
                    var p = RotatePoint(npos, cpos, float.Pi / -4); 
                    dp.Name = "P1_Sonyâ–½Healer";
                    dp.TargetPosition = p;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                else
                {
                    var p = RotatePoint(npos, cpos, float.Pi / 4 * 3);
                    dp.Name = "P1_Sonyâ–½DPS";
                    dp.TargetPosition = p;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }
            //â–¡
            if (p1sony[index] == 2)
            {
                if (index == 0 || index == 1)
                {
                    var p = RotatePoint(npos, cpos, float.Pi / 4);
                    dp.Name = "P1_Sonyâ–¡Tank";
                    dp.TargetPosition = p;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                else
                {
                    var p = RotatePoint(npos, cpos, float.Pi / -4 * 3);
                    dp.Name = "P1_Sonyâ–¡DPS";
                    dp.TargetPosition = p;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }
            //Ã—
            if (p1sony[index] == 3)
            {
                if (index == 0 || index == 1)
                {
                    var p = npos;
                    dp.Name = "P1_SonyÃ—Tank";
                    dp.TargetPosition = p;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                else
                {
                    var p = RotatePoint(npos, cpos, float.Pi);
                    dp.Name = "P1_SonyÃ—DPS";
                    dp.TargetPosition = p;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }


        }
        [ScriptMethod(name: "P1 Lightwing Flash", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25316"])]
        public void P1_LightwingFlash(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(18);
            dp.Radian = float.Pi / 6;
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.Delay = 10000;
            dp.DestoryAt = 20000;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan , dp);
            dp.TargetOrderIndex = 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
        }

        [ScriptMethod(name: "P1 Firmament Seal Player", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2661"])]
        public void P1_FirmamentSealPlayer(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(3);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            
        }
        [ScriptMethod(name: "P1 Firmament Seal Landing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25370"])]
        public void P1_FirmamentSealLanding(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_FirmamentSealLanding";
            dp.Scale = new(3);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.DestoryAt = 2500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        #endregion

        #region P2
        #region First Mechanic
        [ScriptMethod(name: "P2 First Mechanic Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25555"],userControl:false)]
        public void P2_FirstMechanicRecord(Event @event, ScriptAccessory accessory)
        {
            parse = 2.1;
            firstTargetIcon = null;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                tordanId = id;
            }
        }
        [ScriptMethod(name: "P2 First Mechanic Polå…‹å…° Charge", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:3781"])]
        public void P2_FirstMechanicPolå…‹å…°Charge(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(16,52);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2 First Mechanic ä¼Šå°¼äºšæ–¯ Charge", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:3782"])]
        public void P2_FirstMechanicä¼Šå°¼äºšæ–¯Charge(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(16, 52);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2 First Mechanic éŸ¦å°”å‰çº³ Charge", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:3783"])]
        public void P2_FirstMechanicéŸ¦å°”å‰çº³Charge(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(16, 52);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2 First Mechanic Charge Position Record", eventType: EventTypeEnum.NpcYell,userControl:false)]
        public void P2_FirstMechanicChargePositionRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var str= @event["Id"];
            if (str != "3781" && str != "3782" && str != "3783") return;
            var sourcePos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var dir= PositionTo8Dir(sourcePos, new(100, 0, 100));
            if (dir == 0 || dir == 4)
            {
                p2SafeDir[0] = false;
                p2SafeDir[4] = false;
            }
            if (dir == 1 || dir == 5)
            {
                p2SafeDir[1] = false;
                p2SafeDir[5] = false;
            }
            if (dir == 2 || dir == 6)
            {
                p2SafeDir[2] = false;
                p2SafeDir[6] = false;
            }
            if (dir == 3 || dir == 7)
            {
                p2SafeDir[3] = false;
                p2SafeDir[7] = false;
            }
        }
        [ScriptMethod(name: "P2 First Mechanic Charge Safe Zone Position (Imgui)", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:3781"])]
        public void P2_FirstMechanicChargeSafeZonePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            Task.Delay(100).ContinueWith(y =>
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_FirstMechanicChargeSafeZonePosition";
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.DestoryAt = 7000;

                var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
                var cpos = new Vector3(100, 0, 100);
                var npos = new Vector3(100, 0, 82);
                //MT
                if(idIndex==0|| idIndex == 2 || idIndex == 4 || idIndex == 6)
                {
                    dp.TargetPosition = RotatePoint(npos, cpos, p2SafeDir.LastIndexOf(true) * float.Pi / 4);
                }
                else//ST
                {
                    dp.TargetPosition = RotatePoint(npos, cpos, p2SafeDir.IndexOf(true) * float.Pi / 4);
                }

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            });
            
        }
        [ScriptMethod(name: "P2 First Mechanic Earthquake", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25558"])]
        public void P2_FirstMechanicEarthquake(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Name = "P2_FirstMechanicEarthquake";
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }

            dp.Scale = new(6);
            dp.DestoryAt = 6000;
            dp.Radian = float.Pi * 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp.Scale = new(12);
            dp.InnerScale= new(6);
            dp.Delay = 4000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp.Scale = new(18);
            dp.InnerScale = new(12);
            dp.Delay = 6000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp.Scale = new(24);
            dp.InnerScale = new(18);
            dp.Delay = 8000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp.Scale = new(30);
            dp.InnerScale = new(24);
            dp.Delay = 10000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        [ScriptMethod(name: "P2 First Mechanic Piercing Record", eventType: EventTypeEnum.TargetIcon,userControl:false)]
        public void P2_FirstMechanicPiercingRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            if (ParsTargetIcon(@event["Id"]) != 0) return;
            
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                p2BlueCircle.Add(id);
            }
        }
        [ScriptMethod(name: "P2 First Mechanic Spatial Break", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25564"])]
        public void P2_FirstMechanicSpatialBreak(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_FirstMechanicSpatialBreak";
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }

            dp.Scale = new(9);
            dp.Delay = 3000;
            dp.DestoryAt = 9000- dp.Delay;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

           
        }
        [ScriptMethod(name: "P2 First Mechanic Piercing (Big Circle)", eventType: EventTypeEnum.TargetIcon)]
        public void P2_FirstMechanicPiercing(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            if (ParsTargetIcon(@event["Id"]) != 0) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(24);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Delay = 6000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            
        }
        [ScriptMethod(name: "P2 First Mechanic Piercing Tether (ImGui)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25562"])]
        public void P2_FirstMechanicPiercingTether(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_FirstMechanicPiercingTether";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = p2BlueCircle[0];
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 9000;
            for (int i = 1; i < p2BlueCircle.Count; i++)
            {
                dp.TargetObject= p2BlueCircle[i];
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            }
        }
        [ScriptMethod(name: "P2 First Mechanic è®©å‹’åŠª Charge", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:2551"])]
        public void P2_FirstMechanicè®©å‹’åŠªCharge(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(8, 50);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
            dp.Delay = 3000;
            dp.DestoryAt = 3000;
            dp.ScaleMode |= ScaleMode.YByDistance;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2 First Mechanic é˜¿ä»£å°”è²å°” Charge", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:2550"])]
        public void P2_FirstMechanicé˜¿ä»£å°”è²å°”Charge(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(8, 50);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
            dp.Delay = 3000;
            dp.DestoryAt = 3000;
            dp.ScaleMode |= ScaleMode.YByDistance;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2 First Mechanic Knight Position (Imgui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25563"])]
        public void P2_FirstMechanicKnightPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_FirstMechanicKnightPosition";
            dp.Scale = new(8, 50);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.TargetObject = tordanId;
            dp.Owner = accessory.Data.Me;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = 500;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
        }

        #endregion

        #region Second Mechanic
        [ScriptMethod(name: "P2 Second Mechanic Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25569"], userControl: false)]
        public void P2_SecondMechanicRecord(Event @event, ScriptAccessory accessory)
        {
            parse = 2.2;
        }
        [ScriptMethod(name: "P2 Second Mechanic Dragon Eye Lookaway", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:8003759A", "Id:00020001"])]
        public void P2_SecondMechanicDragonEyeLookaway(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            var index=int.Parse(@event["Index"],System.Globalization.NumberStyles.HexNumber);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_SecondMechanicDragonEyeLookaway";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = accessory.Data.Me;
            dp.Delay = 4500;
            dp.DestoryAt = 5000;
            if (index == 0) dp.TargetPosition = new(100, 0, 65);
            if (index == 1) dp.TargetPosition = new(124.75f, 0, 75.25f);
            if (index == 2) dp.TargetPosition = new(135, 0, 100);
            if (index == 3) dp.TargetPosition = new(124.75f, 0, 124.75f);
            if (index == 4) dp.TargetPosition = new(100, 0, 135);
            if (index == 5) dp.TargetPosition = new(75.25f, 0, 124.75f);
            if (index == 6) dp.TargetPosition = new(65, 0, 100);
            if (index == 7) dp.TargetPosition = new(75.25f, 0, 75.25f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.SightAvoid, dp);
        }
        [ScriptMethod(name: "P2 Second Mechanic Knight Lookaway", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25552"])]
        public void P2_SecondMechanicKnightLookaway(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_SecondMechanicKnightLookaway";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = accessory.Data.Me;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.TargetObject = id;
            }
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.SightAvoid, dp);

        }
        [ScriptMethod(name: "P2 Second Mechanic æ³½è²å…° Position Record", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:2549"], userControl: false)]
        public void P2_SecondMechanicæ³½è²å…°PositionRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            p2ZPos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            
        }
        [ScriptMethod(name: "P2 Second Mechanic Cleave Record", eventType: EventTypeEnum.TargetIcon, userControl: false)]
        public void P2_SecondMechanicCleaveRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            var tid = ParsTargetIcon(@event["Id"]);
            if (tid != -279 && tid != -280) return;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                var index = accessory.Data.PartyList.ToList().IndexOf(id);
                if (tid == -280) p2Jump.Item1 = index;
                if (tid == -279) p2Jump.Item2 = index;
            }
        }
        [ScriptMethod(name: "P2 Second Mechanic é˜¿ä»£å°”è²å°” Position", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:12601"], userControl: false)]
        public void P2_SecondMechanicé˜¿ä»£å°”è²å°”Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            p2AdelPos=JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        }
        [ScriptMethod(name: "P2 Second Mechanic Cleave Start Position (Imgui)", eventType: EventTypeEnum.TargetIcon)]
        public void P2_SecondMechanicCleaveStartPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (ParsTargetIcon(@event["Id"]) != -279) return;
            Task.Delay(100).ContinueWith(t =>
            {
                List<int> g1Mem = [];
                List<int> g2Mem = [];

                // 2 6
                // 2 1 3 7
                // 6 0 
                g1Mem.Add(p2Jump.Item1);
                g2Mem.Add(p2Jump.Item2);
                if (p2Jump.Item1 != 0 && p2Jump.Item2 != 0) g2Mem.Add(0);
                if (p2Jump.Item1 != 2 && p2Jump.Item2 != 2) g2Mem.Add(2);
                if (p2Jump.Item1 != 6 && p2Jump.Item2 != 6) g2Mem.Add(6);
                if (p2Jump.Item1 != 1 && p2Jump.Item2 != 1) g1Mem.Add(1);
                if (p2Jump.Item1 != 3 && p2Jump.Item2 != 3) g1Mem.Add(3);
                if (p2Jump.Item1 != 7 && p2Jump.Item2 != 7) g1Mem.Add(7);
                if (p2Jump.Item1 != 4 && p2Jump.Item2 != 4)
                {
                    if (g2Mem.Count <= 3) g2Mem.Add(4);
                    else g1Mem.Add(4);
                }
                if (p2Jump.Item1 != 5 && p2Jump.Item2 != 5)
                {
                    if (g1Mem.Count <= 3) g1Mem.Add(5);
                    else g2Mem.Add(5);
                }
                
                var drot = p2AdelPos.X > 100? float.Pi / 45: float.Pi / -45;
                var meIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Scale = new(1.5f, 20);
                dp.Color = accessory.Data.DefaultSafeColor.WithW(3);
                dp.Owner = accessory.Data.Me;
                dp.DestoryAt = 5000;
                dp.ScaleMode |= ScaleMode.YByDistance;

                var cpos = new Vector3(100, 0, 100);
                var sPos = (p2ZPos - cpos) / 15 * 19.5f + cpos;
                if (g1Mem.IndexOf(meIndex) != -1)
                {
                    dp.TargetPosition = RotatePoint(sPos, cpos, float.Pi + drot * 3);
                }
                else
                {
                    dp.TargetPosition = RotatePoint(sPos, cpos, drot * 3);
                }

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);



                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Color = accessory.Data.DefaultSafeColor.WithW(3);
                dp2.Scale = new(1.5f, 20);
                dp2.ScaleMode |= ScaleMode.YByDistance;
                dp2.DestoryAt = 15000;
                dp2.Position=dp.TargetPosition;
                dp2.TargetPosition = RotatePoint(dp2.Position.Value, cpos, drot * 5);
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
            });
        }
        [ScriptMethod(name: "P2 Second Mechanic Light Orb Explosion Range", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:13070"])]
        public void P2_SecondMechanicLightOrbExplosionRange(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(9);
            dp.Color = accessory.Data.DefaultDangerColor;
            var idStr = @event["SourceId"];
            if (ParseObjectId(idStr, out var id))
            {
                dp.Owner = id;
            }
            dp.DestoryAt = 2000;
            dp.Name = $"P2_SecondMechanicLightOrbExplosionRange{idStr}";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P2 Light Orb Explosion Range Removal", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25295"],userControl:false)]
        public void P2_LightOrbExplosionRangeRemoval(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            accessory.Method.RemoveDraw($"P2_SecondMechanicLightOrbExplosionRange{@event["SourceId"]}");
        }
        [ScriptMethod(name: "P2 Second Mechanic Meteor Record", eventType: EventTypeEnum.TargetIcon,userControl:false)]
        public void P2_SecondMechanicMeteorRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (ParsTargetIcon(@event["Id"]) != -45) return;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                p2Stone[accessory.Data.PartyList.ToList().IndexOf(id)] = true;
            }
            var s1 = p2Stone.IndexOf(true);
            var s2 = p2Stone.LastIndexOf(true);
            //Record grouping
            if (s1 != s2)
            {
                p2StoneMem = (s1, s2);
                //AB mt h2
                if (s1 == 0 && s2 == 3)
                {
                    p2StoneTeam = [0, 4, 5, 1, 3, 7, 6, 2];
                }
                //AB d14
                if (s1 == 4 && s2 == 7)
                {
                    p2StoneTeam = [4, 0, 5, 1, 7, 3, 6, 2];
                }
                //AC dual tanks
                if (s1 == 0 && s2 == 1)
                {
                    p2StoneTeam = [0, 4, 7, 3, 1, 5, 6, 2];
                }
                //AC d12
                if (s1 == 4 && s2 == 5)
                {
                    p2StoneTeam = [4, 0, 7, 3, 5, 1, 6, 2];
                }
                //AD mt H1
                if (s1 == 0 && s2 == 2)
                {
                    p2StoneTeam = [0, 4, 7, 3, 2, 6, 5, 1];
                }
                //AD d13
                if (s1 == 4 && s2 == 6)
                {
                    p2StoneTeam = [4, 0, 7, 3, 6, 2, 5, 1];
                }
                //BC h2 st
                if (s1 == 1 && s2 == 3)
                {
                    p2StoneTeam = [3, 7, 4, 0, 1, 5, 6, 2];
                }
                //BC d24
                if (s1 == 5 && s2 == 7)
                {
                    p2StoneTeam = [7, 3, 4, 0, 5, 1, 6, 2];
                }
                //BD h12
                if (s1 == 2 && s2 == 3)
                {
                    p2StoneTeam = [4, 0, 3, 7, 5, 1, 2, 6];
                }
                //BD d34
                if (s1 == 6 && s2 == 7)
                {
                    p2StoneTeam = [4, 0, 7, 3, 5, 1, 6, 2];
                }
                //CD st h1
                if (s1 == 1 && s2 == 2)
                {
                    p2StoneTeam = [2, 6, 7, 3, 1, 5, 4, 0];
                }
                //CD d23
                if (s1 == 5 && s2 == 6)
                {
                    p2StoneTeam = [6, 2, 7, 3, 5, 1, 4, 0];
                }
            }
        }
        [ScriptMethod(name: "P2 Second Mechanic Meteor Tether (ImGui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25576"])]
        public void P2_SecondMechanicMeteorTether(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            Task.Delay(100).ContinueWith(t =>
            {
                
                var s1 = p2Stone.IndexOf(true);
                var s2 = p2Stone.LastIndexOf(true);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = accessory.Data.PartyList[s1];
                dp.TargetObject = accessory.Data.PartyList[s2];
                dp.DestoryAt = 12000;
                dp.Name = "P2_SecondMechanicMeteorTether(ImGui)";
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            });

        }
        [ScriptMethod(name: "P2 Second Mechanic Ice Stack Position (ImGui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25576"])]
        public void P2_SecondMechanicIceStackPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            Task.Delay(100).ContinueWith(t =>
            {
                var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
                var dir4=p2StoneTeam.IndexOf(idIndex)/2;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_SecondMechanicIceStackPosition(ImGui)";
                dp.Scale = new(3f, 10);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(new(100,0,88.5f),new(100,0,100),float.Pi/2*dir4);
                dp.DestoryAt = 7000;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            });
            

            
            //accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


        }
        [ScriptMethod(name: "P2 Second Mechanic First Round Tower Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29564"],userControl:false)]
        public void P2_SecondMechanicFirstRoundTowerRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            var sourcePos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var cpos = new Vector3(100, 0, 100);
            if ((sourcePos-cpos).Length() > 7)
            {
                var dir = (PositionTo12Dir(sourcePos, cpos) + 1) % 12;
                p2Tower[dir] = true;
            }
            else
            {
                var dir = PositionTo8Dir(sourcePos, cpos) / 2 + 12;
                p2Tower[dir] = true;
            }
        }
        [ScriptMethod(name: "P2 Second Mechanic First Round Tower Position (ImGui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29563"])]
        public void P2_SecondMechanicFirstRoundTowerPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            Task.Delay(100).ContinueWith(t =>
            {
                List<int> towerMem = [-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1];
                List<int> alternate = [];
                //High priority
                for (int i = 0; i < 4; i++)
                {
                    var MemIndex = p2StoneTeam[i * 2];
                    //Middle
                    if (p2Tower[i * 3 + 1])
                    {
                        towerMem[i * 3 + 1] = MemIndex;
                        continue;
                    }
                    //Left
                    if (p2Tower[i * 3])
                    {
                        towerMem[i * 3] = MemIndex;
                        continue;
                    }
                    //Right
                    if (p2Tower[i * 3 + 2])
                    {
                        towerMem[i * 3 + 2] = MemIndex;
                        continue;
                    }
                }

                //Low priority
                for (int i = 0; i < 4; i++)
                {
                    var MemIndex = p2StoneTeam[i * 2 + 1];
                    //Left
                    if (p2Tower[i * 3] && towerMem[i * 3] == -1)
                    {
                        towerMem[i * 3] = MemIndex;
                        continue;
                    }
                    //Right
                    if (p2Tower[i * 3 + 2] && towerMem[i * 3 + 2] == -1)
                    {
                        towerMem[i * 3 + 2] = MemIndex;
                        continue;
                    }
                    //Inner left
                    if (p2Tower[i + 12] && towerMem[i + 12] == -1)
                    {
                        towerMem[i + 12] = MemIndex;
                        continue;
                    }
                    //Fill towers
                    alternate.Add(MemIndex);
                }

                //Fill towers
                foreach (var mem in alternate)
                {
                    for (int i = 12; i < 16; i++)
                    {
                        if (p2Tower[i] && towerMem[i] == -1)
                        {
                            towerMem[i] = mem;
                            break;
                        }
                    }
                }

                var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
                var npos = new Vector3(100, 0, 82);
                var npos2 = new Vector3(100, 0, 94);
                var cpos = new Vector3(100, 0, 100);
                var dp = accessory.Data.GetDefaultDrawProperties();
                var tIndex = towerMem.IndexOf(idIndex);
                if (tIndex >= 0 && tIndex < 12)
                {
                    dp.Position = RotatePoint(npos, cpos, float.Pi / 6 * (tIndex - 1));
                }
                if (tIndex >= 12 && tIndex < 16)
                {
                    dp.Position = RotatePoint(npos2, cpos, float.Pi / 2 * (tIndex - 12) + float.Pi / 4);
                }

                dp.Name = "P2_SecondMechanicFirstRoundTowerPosition(ImGui)";
                dp.DestoryAt = 12000;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Scale = new(3);
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = "P2_SecondMechanicFirstRoundTowerPosition(ImGui)";
                dp2.Color= accessory.Data.DefaultSafeColor;
                dp2.Owner = accessory.Data.Me;
                dp2.TargetPosition = dp.Position;
                dp2.Scale = new(3f, 10);
                dp2.ScaleMode |= ScaleMode.YByDistance;
                dp2.Delay = 7500;
                dp2.DestoryAt = 4500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);

            });
        }
        [ScriptMethod(name: "P2 Second Mechanic Second Round Tower Position (ImGui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28650"])]
        public void P2_SecondMechanicSecondRoundTowerPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;

            var index = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            var posIndex =p2StoneTeam.IndexOf(index);
            if (index == p2StoneMem.Item1) posIndex = p2StoneTeam.IndexOf(p2StoneMem.Item2);
            if (index == p2StoneMem.Item2) posIndex = p2StoneTeam.IndexOf(p2StoneMem.Item1);

            var npos = new Vector3(100, 0, 82);
            var cpos = new Vector3(100, 0, 100);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.TargetPosition = RotatePoint(npos, cpos, float.Pi / 4 * posIndex);
            

            dp.Name = "P2_SecondMechanicFirstRoundTowerPosition(ImGui)";
            dp.DestoryAt = 11000;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Position = dp.TargetPosition;
            dp.Scale = new(3);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);


        }
        #endregion
        [ScriptMethod(name: "P2 Second Mechanic End Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25533"],userControl:false)]
        public void P2_SecondMechanicEndRecord(Event @event, ScriptAccessory accessory)
        {
            parse = 2.3;
        }
        [ScriptMethod(name: "P2 Knight Mighty Swing (Right)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25536"])]
        public void P2_KnightMightySwingRight(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }

            dp.Scale = new(40);
            dp.Radian = float.Pi / 180 * 130;
            dp.Rotation= float.Pi / 180 * -65;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp.Color = accessory.Data.DefaultSafeColor;
            dp.TargetColor = accessory.Data.DefaultDangerColor;
            dp.Rotation = float.Pi;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }
        [ScriptMethod(name: "P2 Knight Mighty Swing (Left)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25537"])]
        public void P2_KnightMightySwingLeft(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }

            dp.Scale = new(40);
            dp.Radian = float.Pi / 180 * 130;
            dp.Rotation = float.Pi / 180 * 65;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp.Color = accessory.Data.DefaultSafeColor;
            dp.TargetColor = accessory.Data.DefaultDangerColor;
            dp.Rotation = float.Pi;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }

        #endregion

        #region P3
        [ScriptMethod(name: "P3 Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26376"],userControl:false)]
        public void P3_Record(Event @event, ScriptAccessory accessory)
        {
            parse = 3;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                p3BossId = id;
            }
        }
        [ScriptMethod(name: "P3 Fang Tail Spiral (Circle Donut)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26386"])]
        public void P3_FangTailSpiral(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }

            dp.Scale = new(8);
            dp.Radian = float.Pi *2;
            dp.DestoryAt = 11500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp.Scale = new(40);
            dp.InnerScale = new(8);
            dp.Delay = 11500;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

        }
        [ScriptMethod(name: "P3 Tail Fang Spiral (Donut Circle)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26387"])]
        public void P3_TailFangSpiral(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }

            dp.Scale = new(40);
            dp.InnerScale= new(8);
            dp.Radian = float.Pi * 2;
            dp.DestoryAt = 11500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp.Delay = 11500;
            dp.Scale = new(8);
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "P3 Stationary Tower Prediction", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26382"])]
        public void P3_StationaryTowerPrediction(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(5);
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P3 Up Arrow Tower Prediction", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26383"])]
        public void P3_UpArrowTowerPrediction(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Offset = new(0, 0, -14);
            dp.Scale = new(5);
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P3 Down Arrow Tower Prediction", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26384"])]
        public void P3_DownArrowTowerPrediction(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Offset = new(0, 0, -14);
            dp.Scale = new(5);
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P3 Tower Position Confirmation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26385"])]
        public void P3_TowerPositionConfirmation(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(5);
            dp.DestoryAt = 2500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P3 Mahjong Geirskogul Guide", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26385"])]
        public void P3_MahjongGeirskogulGuide(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(8,62);
            dp.DestoryAt = 2500;
            dp.TargetResolvePattern=PositionResolvePatternEnum.PlayerNearestOrder;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P3 Four-Tower Geirskogul Guide", eventType: EventTypeEnum.StartCasting)]
        public void P3_FourTowerGeirskogulGuide(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            var aid = @event["ActionId"];
            if(aid!= "26391" && aid != "26392" && aid != "26393" && aid != "26394") return;
            var str = @event["SourceId"];
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Name = $"P3_FourTowerGeirskogulGuide{str}";
            if (ParseObjectId(str, out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(8, 62);
            dp.Delay = 5000;
            dp.DestoryAt = 2500;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P3 Four-Tower Geirskogul Removal", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0054"],userControl:false)]
        public void P3_FourTowerGeirskogulRemoval(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            accessory.Method.RemoveDraw($"P3_FourTowerGeirskogulGuide{@event["SourceId"]}");
        }
        [ScriptMethod(name: "P3 Geirskogul Confirmation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26378"])]
        public void P3_GeirskogulConfirmation(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(8, 62);
            dp.DestoryAt = 4500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P3 Same-Group Mahjong Tether (ImGui)", eventType: EventTypeEnum.StatusAdd)]
        public void P3_SameGroupMahjongTether(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            var stasusid = @event["StatusID"];
            if (stasusid != "3004" && stasusid != "3005" && stasusid != "3006") return;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                if (p3majong.ContainsKey(stasusid))
                {
                    p3majong[stasusid].Add(id);
                }
                else
                {
                    p3majong.Add(stasusid, []);
                    p3majong[stasusid].Add(id);
                }
            }


            if (id == accessory.Data.Me)
            {
                Task.Delay(100).ContinueWith((o) =>
                {
                    foreach (var tid in p3majong[stasusid])
                    {
                        var dp=accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P3_SameGroupMahjongTether";
                        dp.Owner = id;
                        dp.TargetObject = tid;
                        dp.Color=accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 6000;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
                    }
                });

            }
        }
        [ScriptMethod(name: "P3 Dragonsong Spear", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26380"])]
        public void P3_DragonsongSpear(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(13);
            dp.Radian = float.Pi / 2;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        [ScriptMethod(name: "P3 Four-Tower Record", eventType: EventTypeEnum.StartCasting,userControl:false)]
        public void P3_FourTowerRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            var aid = @event["ActionId"];
            if (aid != "26391" && aid != "26392" && aid != "26393" && aid != "26394") return;
            var num=int.Parse(aid)-26390;
            var sourcePos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var dir = PositionTo8Dir(sourcePos, new(100, 0, 100))/2;
            p3Tower[dir] = num;
        }
        [ScriptMethod(name: "P3 Four-Tower Position (ImGui)", eventType: EventTypeEnum.StartCasting)]
        public void P3_FourTowerPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3 || p3TowerDeal) return;
            var aid = @event["ActionId"];
            if (aid != "26391" && aid != "26392" && aid != "26393" && aid != "26394") return;
            p3TowerDeal = true;
            
            Task.Delay(100).ContinueWith(t =>
            {
                var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
                var myTower = -1;
                //D4
                if (idIndex == 7) { myTower = 0; }
                //H2
                if (idIndex == 3) { myTower = 1; }
                //H1
                if (idIndex == 2) { myTower = 2; }
                //D3
                if (idIndex == 6) { myTower = 3; }
                //St
                if (idIndex == 1) 
                {
                    
                    if (p3Tower[0] >= 2) { myTower = 0;}
                    else
                    {
                        if (p3Tower[1] > 2) { myTower = 1; }
                        else if (p3Tower[3] > 2) { myTower = 3; }
                        else if (p3Tower[2] > 2) { myTower = 2; }
                    }
                }
                //D2
                if (idIndex == 5)
                {
                    if (p3Tower[1] >= 2) { myTower = 1; }
                    else
                    {
                        if (p3Tower[2] > 2) { myTower = 2; }
                        else if (p3Tower[0] > 2) { myTower = 0; }
                        else if (p3Tower[3] > 2) { myTower = 3; }
                    }
                }
                //D1
                if (idIndex == 4)
                {
                    if (p3Tower[2] >= 2) { myTower = 2; }
                    else
                    {
                        if (p3Tower[3] > 2) { myTower = 3; }
                        else if (p3Tower[1] > 2) { myTower = 1; }
                        else if (p3Tower[0] > 2) { myTower = 0; }
                    }
                }
                //Mt
                if (idIndex == 0)
                {
                    if (p3Tower[3] >= 2) { myTower = 3; }
                    else
                    {
                        if (p3Tower[0] > 2) { myTower = 0; }
                        else if (p3Tower[2] > 2) { myTower = 2; }
                        else if (p3Tower[1] > 2) { myTower = 1; }
                    }
                }

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Position=RotatePoint(new(108,0,92),new(100,0,100),float.Pi/2*myTower);
                dp.Scale = new(5);
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

            });
        }
        [ScriptMethod(name: "P3 Soulshot Tank Assist (ImGui)", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0054"])]
        public void P3_SoulshotTankAssist(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            if (!ParseObjectId(@event["SourceId"], out var id)) return;
            var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            if ((idIndex == 0 && id == p3BossId && !p3Boom[0]) || (idIndex == 1 && id != p3BossId && !p3Boom[1]))
            {
                p3Boom[idIndex] = true;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3_Soulshot{(idIndex==0?"M":"S")}TankAssist";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = id;
                dp.Scale = new(10);
                dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            }
        }
        [ScriptMethod(name: "P3 Soulshot Range", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0054"])]
        public void P3_SoulshotRange(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            if (!ParseObjectId(@event["SourceId"], out var id)) return;
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SoulshotRange";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = id;
            dp.Scale = new(5);
            dp.CentreResolvePattern = PositionResolvePatternEnum.OwnerTarget;
            dp.DestoryAt = 7000;
            if (id == p3BossId && !p3Boom[2])
            {
                p3Boom[2] = true;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            if (id != p3BossId && !p3Boom[3])
            {
                p3Boom[3] = true;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }

        #endregion

        #region P4
        [ScriptMethod(name: "P4 Record", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:29750"], userControl: false)]
        public void P4_Record(Event @event, ScriptAccessory accessory)
        {
            parse = 4;
        }

        #endregion

        #region P5
        #region First Mechanic
        [ScriptMethod(name: "P5 First Mechanic Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27529"], userControl: false)]
        public void P5_FirstMechanicRecord(Event @event, ScriptAccessory accessory)
        {
            parse = 5.1;
        }
        [ScriptMethod(name: "P5 First Mechanic Cyclone Charge", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27531"])]
        public void P5_FirstMechanicCycloneCharge(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }

            dp.Scale = new(10,60);
            dp.DestoryAt = 6000;
            dp.Name = $"P5_FirstMechanicCycloneCharge";
            
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            
        }
        [ScriptMethod(name: "P5 First Mechanic White Dragon Position Tether (ImGui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27531"])]
        public void P5_FirstMechanicWhiteDragonPositionTether(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.TargetObject = id;
            }
            dp.Owner = accessory.Data.Me;
            dp.Scale = new(5);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 6000;
            dp.Name = $"P5_FirstMechanicWhiteDragonPositionTether";

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);

        }
        [ScriptMethod(name: "P5 First Mechanic Dual Knight Spiralspear", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0005"])]
        public void P5_FirstMechanicDualKnightTether(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.TargetObject = tid;
            }

            dp.Scale = new(16, 60);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = p5TetherCrashDelay;
            dp.DestoryAt = 6000- p5TetherCrashDelay;
            dp.Name = $"P5_FirstMechanicDualKnightTetherCharge";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P5 First Mechanic Lightning Wing", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2833"])]
        public void P5_FirstMechanicLightningWing(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(5);
            dp.DestoryAt = 5000;
            dp.Delay = int.TryParse(@event["DurationMilliseconds"], out var d) ? (d - dp.DestoryAt) :8000;
            dp.Name = $"P5_FirstMechanicLightningWing{id:X8}";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "P5 First Mechanic Piercing", eventType: EventTypeEnum.TargetIcon)]
        public void P5_FirstMechanicPiercing(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1 || ParsTargetIcon(@event["Id"]) != -316) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(24);
            dp.Delay = 2000;
            dp.DestoryAt = 4000;
            dp.Name = $"P5_FirstMechanicPiercing{id:X8}";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "P5 Ascalon's Benevolence Revelation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25546"])]
        public void P5_AscalonsBenevolenceRevelation(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }

            dp.Scale = new(50);
            dp.Radian = float.Pi / 180 * 30;
            dp.DestoryAt = 4000;
            foreach (var tid in accessory.Data.PartyList)
            {
                dp.Name = $"P5_AscalonsBenevolenceRevelation {tid:X8}";
                dp.TargetObject = tid;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }




        }
        [ScriptMethod(name: "P5 First Mechanic Dual Dragon Dive Position Record", eventType: EventTypeEnum.SetObjPos,eventCondition: ["SourceDataId:12603"],userControl:false)]
        public void P5_FirstMechanicDualDragonDivePositionRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var pos= JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            p5DivePos = new((pos.X - 100) / 9 * 19 + 100, pos.Y, (pos.Z - 100) / 9 * 19 + 100);
        }
        [ScriptMethod(name: "P5 First Mechanic Dual Dragon Dive Position", eventType: EventTypeEnum.TargetIcon)]
        public void P5_FirstMechanicDualDragonDivePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1 || ParsTargetIcon(@event["Id"]) != -310) return;
            if (!ParseObjectId(@event["TargetId"], out var id) || id!=accessory.Data.Me) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor.WithW(2f);
            dp.Owner = id;
            dp.TargetPosition=p5DivePos;
            dp.Scale = new(1,60);
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Name = $"P5_FirstMechanicDualDragonDivePosition";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

        }
        [ScriptMethod(name: "P5 First Mechanic White Dragon Dive", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27534"])]
        public void P5_FirstMechanicWhiteDragonDive(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(20, 48);
            dp.DestoryAt = 6000;
            dp.Name = $"P5_FirstMechanicWhiteDragonDive";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P5 First Mechanic Black Dragon Dive", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27533"])]
        public void P5_FirstMechanicBlackDragonDive(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(20, 48);
            dp.DestoryAt = 6000;
            dp.Name = $"P5_FirstMechanicBlackDragonDive";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P5 First Mechanic Grenold Position Record", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:12602"], userControl: false)]
        public void P5_FirstMechanicGrenoldPositionRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            p5GrenoPos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        }
        [ScriptMethod(name: "P5 First Mechanic Tether Grenold (ImGui)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25546"])]
        public void P5_FirstMechanicTetherGrenold(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_FirstMechanicTetherGrenold";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = p5GrenoPos;
            dp.Scale = new(5);
            dp.DestoryAt = 8000;
            dp.ScaleMode |= ScaleMode.YByDistance;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);

        }
        #endregion

        #region Second Mechanic
        [ScriptMethod(name: "P5 Second Mechanic Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27538"], userControl: false)]
        public void P5_SecondMechanicRecord(Event @event, ScriptAccessory accessory)
        {
            parse = 5.2;
            p5sony = [0, 0, 0, 0, 0, 0, 0, 0];
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                tordanId = id;
            }
        }
        [ScriptMethod(name: "P5 Second Mechanic Black Dragon Dive", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27533"])]
        public void P5_SecondMechanicBlackDragonDive(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(20, 48);
            dp.DestoryAt = 6000;
            dp.Name = $"P5_SecondMechanicBlackDragonDive";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P5 Second Mechanic Cyclone Charge", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27531"])]
        public void P5_SecondMechanicCycloneCharge(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }

            dp.Scale = new(10, 60);
            dp.DestoryAt = 6000;
            dp.Name = $"P5_SecondMechanicCycloneCharge";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P5 Second Mechanic Valkyrie's Lance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27539"])]
        public void P5_SecondMechanicValkyriesLance(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(10, 50);
            dp.DestoryAt = 6000;
            dp.Name = $"P5_SecondMechanicValkyriesLance";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P5 Second Mechanic Earthquake", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25558"])]
        public void P5_SecondMechanicEarthquake(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.Name = "P5_SecondMechanicEarthquake";

            dp.Scale = new(6);
            dp.DestoryAt = 6000;
            dp.Radian = float.Pi * 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp.Scale = new(12);
            dp.InnerScale = new(6);
            dp.Delay = 4000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp.Scale = new(18);
            dp.InnerScale = new(12);
            dp.Delay = 6000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp.Scale = new(24);
            dp.InnerScale = new(18);
            dp.Delay = 8000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp.Scale = new(30);
            dp.InnerScale = new(24);
            dp.Delay = 10000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        [ScriptMethod(name: "P5 Second Mechanic Dragon Eye Lookaway", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:8003759A", "Id:00020001"])]
        public void P5_SecondMechanicDragonEyeLookaway(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            var index = int.Parse(@event["Index"], System.Globalization.NumberStyles.HexNumber);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = accessory.Data.Me;
            dp.Delay = 16000;
            dp.DestoryAt = 7000;
            if (index == 0) dp.TargetPosition = new(100, 0, 65);
            if (index == 1) dp.TargetPosition = new(124.75f, 0, 75.25f);
            if (index == 2) dp.TargetPosition = new(135, 0, 100);
            if (index == 3) dp.TargetPosition = new(124.75f, 0, 124.75f);
            if (index == 4) dp.TargetPosition = new(100, 0, 135);
            if (index == 5) dp.TargetPosition = new(75.25f, 0, 124.75f);
            if (index == 6) dp.TargetPosition = new(65, 0, 100);
            if (index == 7) dp.TargetPosition = new(75.25f, 0, 75.25f);
            dp.Name = "P5_SecondMechanicDragonEyeLookaway";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.SightAvoid, dp);
        }
        [ScriptMethod(name: "P5 Second Mechanic Knight Lookaway", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:8003759A", "Id:00020001"])]
        public void P5_SecondMechanicKnightLookaway(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = tordanId;
            dp.Delay = 16000;
            dp.DestoryAt = 7000;
            
            dp.Name = "P5_SecondMechanicKnightLookaway";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.SightAvoid, dp);
        }
        [ScriptMethod(name: "P5 Second Mechanic Sony Record", eventType: EventTypeEnum.TargetIcon,userControl:false)]
        public void P5_SecondMechanicSonyRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            var sony = ParsTargetIcon(@event["Id"])+49;
            if (sony < 0 || sony > 3) return;
            if(ParseObjectId(@event["TargetId"], out var id))
            {
                var index= accessory.Data.PartyList.ToList().IndexOf(id);
                p5sony[index] += sony;
            }
        }
        [ScriptMethod(name: "P5 Second Mechanic Doom Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2976"],userControl: false)]
        public void P5_SecondMechanicDoomRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                var index = accessory.Data.PartyList.ToList().IndexOf(id);
                p5sony[index] +=10;
            }
        }
        [ScriptMethod(name: "P5 Second Mechanic Galick Position Record", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:12637"],userControl:false)]
        public void P5_SecondMechanicGalickPositionRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            p5GreekPos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        }
        [ScriptMethod(name: "P5 Second Mechanic Doom Hexa Position (ImGui)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2976"])]
        public void P5_SecondMechanicDoomHexaPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            Task.Delay(100).ContinueWith(t =>
            {
                if (p5Deal) return;
                var count = p5sony.Where(s => s > 5).Count();
                if (count != 4) return;
                p5Deal = true;
                var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
                var sony = p5sony[idIndex];
                var posid = sony > 0 ? 4 : 0;
                for (int i = 0; i < idIndex; i++)
                {
                    if(sony== p5sony[i])
                    {
                        posid++;
                    }
                }
                var cpos = new Vector3(100, 0, 100);
                var npos = 19.5f*Vector3.Normalize(new(p5GreekPos.X - 100, p5GreekPos.Y, p5GreekPos.Z - 100)) + cpos;
                if(posid==4||posid==7) { npos = 13 * Vector3.Normalize(new(p5GreekPos.X - 100, p5GreekPos.Y, p5GreekPos.Z - 100)) + cpos; }
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.Scale = new(1.5f, 60);
                dp.DestoryAt = 7000;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Name = $"P5_SecondMechanicDoomGuidePosition{sony}";

                var d = float.Pi / 180f;
                if (posid == 0) dp.TargetPosition = RotatePoint(npos, cpos, d * -90);
                if (posid == 1) dp.TargetPosition = RotatePoint(npos, cpos, d * -142.5f);
                if (posid == 2) dp.TargetPosition = RotatePoint(npos, cpos, d * 142.5f);
                if (posid == 3) dp.TargetPosition = RotatePoint(npos, cpos, d * 90);
                if (posid == 4) dp.TargetPosition = RotatePoint(npos, cpos, d * -90);
                if (posid == 5) dp.TargetPosition = RotatePoint(npos, cpos, d * -37.5f);
                if (posid == 6) dp.TargetPosition = RotatePoint(npos, cpos, d * 37.5f);
                if (posid == 7) dp.TargetPosition = RotatePoint(npos, cpos, d * 90);
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            });
        }
        [ScriptMethod(name: "P5 Second Mechanic Sony Guide Position (ImGui)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:27533"])]
        public void P5_SecondMechanicSonyGuidePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;

            var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            var sony = p5sony[idIndex];
            var posid = sony > 0 ? 4 : 0;
            for (int i = 0; i < idIndex; i++)
            {
                if (sony == p5sony[i])
                {
                    posid++;
                }
            }
            var cpos = new Vector3(100, 0, 100);
            var npos = 10 * Vector3.Normalize(new(p5GreekPos.X - 100, p5GreekPos.Y, p5GreekPos.Z - 100)) + cpos;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.Scale = new(1.5f, 60);
            dp.DestoryAt = 8000;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Name = $"P5_SecondMechanicSonyGuidePosition{sony}";

            var d = float.Pi / 180f;
            dp.TargetPosition = cpos;
            if (posid == 4) dp.TargetPosition = RotatePoint(npos, cpos, d * -90);
            if (posid == 7) dp.TargetPosition = RotatePoint(npos, cpos, d * 90);


            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P5 Second Mechanic Sony Position (Horizontal Method) (ImGui)", eventType: EventTypeEnum.TargetIcon)]
        public void P5_SecondMechanicSonyPosition_HorizontalMethod(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            if (!ParseObjectId(@event["TargetId"], out var id) || id != accessory.Data.Me) return;
            Task.Delay(100).ContinueWith(ca =>
            {
                var index = accessory.Data.PartyList.ToList().IndexOf(id);
                var sony =p5sony[index];
                var priority = p5sony.IndexOf(sony) == index;
                var cpos = new Vector3(100, 0, 100);
                var npos = 4*Vector3.Normalize(new(p5GreekPos.X-100,p5GreekPos.Y,p5GreekPos.Z-100))+ cpos;
                var npos2 = 20f * Vector3.Normalize(new(p5GreekPos.X - 100, p5GreekPos.Y, p5GreekPos.Z - 100)) + cpos;


                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = id;
                dp.Scale = new(3, 60);
                dp.DestoryAt = 5000;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Name = $"P5_SecondMechanicSony{sony}Position";

                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Color = accessory.Data.DefaultSafeColor;
                dp2.Scale = new(1f);
                dp2.DestoryAt = 5000;
                dp2.Name = $"P5_SecondMechanicSony{sony}KnockbackEnd";
                //Doom â—‹
                if (sony == 10)
                {
                    if (priority)
                    {
                        dp.TargetPosition = RotatePoint(npos, cpos, float.Pi / -2);
                        dp2.Position = RotatePoint(npos2, cpos, float.Pi / -2);
                    }
                    else 
                    { 
                        dp.TargetPosition = RotatePoint(npos, cpos, float.Pi / 2);
                        dp2.Position = RotatePoint(npos2, cpos, float.Pi / 2);
                    }
                }
                //Doom â–½
                if (sony == 11)
                {
                    dp.TargetPosition = RotatePoint(npos, cpos, float.Pi * 0.75f);
                    dp2.Position = RotatePoint(npos2, cpos, float.Pi * 0.75f);
                }
                //Doom â–¡
                if (sony == 12)
                {
                    dp.TargetPosition = RotatePoint(npos, cpos, float.Pi * -0.75f);
                    dp2.Position = RotatePoint(npos2, cpos, float.Pi * -0.75f);
                }
                //â–½
                if (sony == 1)
                {
                    dp.TargetPosition = RotatePoint(npos, cpos, float.Pi * -0.25f);
                    dp2.Position = RotatePoint(npos2, cpos, float.Pi * -0.25f);
                }
                //â–¡
                if (sony == 2)
                {
                    dp.TargetPosition = RotatePoint(npos, cpos, float.Pi * 0.25f);
                    dp2.Position = RotatePoint(npos2, cpos, float.Pi * 0.25f);
                }
                //Ã—
                if (sony == 3)
                {
                    if (priority)
                    {
                        dp.TargetPosition = npos;
                        dp2.Position = npos2;
                    }
                    else
                    {
                        dp.TargetPosition = RotatePoint(npos, cpos, float.Pi);
                        dp2.Position = RotatePoint(npos2, cpos, float.Pi);
                    }
                }


                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp2);
            });

        }



        #endregion
        #endregion

        #region P6
        [ScriptMethod(name: "P6 Intro Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26215"], userControl: false)]
        public void P6_IntroRecord(Event @event, ScriptAccessory accessory)
        {
            parse = 6.1;
            p6FireBallCount = 0;
            p6FireBallCount2 = 0;
        }
        [ScriptMethod(name: "P6 Phase Accumulation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27969"],userControl:false)]
        public void P6_PhaseAccumulation(Event @event, ScriptAccessory accessory)
        {
            parse=Math.Round(parse + 0.1, 1);
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                whiteDragonId = id;
            }
        }
        [ScriptMethod(name: "P6 Black Dragon ID", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27971"], userControl: false)]
        public void P6_BlackDragonID(Event @event, ScriptAccessory accessory)
        {
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                darkDragonId = id;
            }
        }
        [ScriptMethod(name: "P6 White Dragon Position ID Record", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:12613"], userControl: false)]
        public void P6_WhiteDragonPositionIDRecord(Event @event, ScriptAccessory accessory)
        {
            p6WhitePos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                whiteDragonId = sid;
            }

        }
        [ScriptMethod(name: "P6 Intro Ice Fire Tether Collection", eventType: EventTypeEnum.Tether, userControl: false)]
        public void P6_IntroIceFireTetherCollection(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.1) return;
            
            if (!ParseObjectId(@event["SourceId"],out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            p6tether[accessory.Data.PartyList.ToList().IndexOf(sid)] = tid==whiteDragonId ? 2 : 1;

        }
        [ScriptMethod(name: "P6 First Ice Fire Tether Position (Imgui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27960"])]
        public void P6_FirstIceFireTetherPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.1) return;
            
            List<Vector3> positions = [new(100, 0, 109.33f), new(95.7f, 0, 119), new(104.3f, 0, 119)];
            //45 26 37
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P6_FirstIceFireTetherPosition";
            dp.Owner = accessory.Data.Me;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Scale = new(1.5f);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 7000;
            var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            //D1
            if (idIndex == 4) dp.TargetPosition = positions[0];
            if (idIndex == 2) dp.TargetPosition = positions[1];
            if (idIndex == 3) dp.TargetPosition = positions[2];
            //D2
            if (idIndex == 5)
            {
                if (p6tether[4]!= p6tether[5])
                {
                    dp.TargetPosition = positions[0];
                }
                else
                {
                    if(p6tether[2] == p6tether[6])
                    {
                        dp.TargetPosition = positions[1];
                    }
                    else
                    {
                        dp.TargetPosition = positions[2];
                    }
                }
            }
            //D3
            if (idIndex == 6)
            {
                if (p6tether[2] != p6tether[6])
                {
                    dp.TargetPosition = positions[1];
                }
                else
                {
                    if (p6tether[4] == p6tether[5])
                    {
                        dp.TargetPosition = positions[0];
                    }
                    else
                    {
                        dp.TargetPosition = positions[2];
                    }
                }
            }
            //D4
            if (idIndex == 7)
            {
                if (p6tether[3] != p6tether[7])
                {
                    dp.TargetPosition = positions[2];
                }
                else
                {
                    if (p6tether[4] == p6tether[5])
                    {
                        dp.TargetPosition = positions[0];
                    }
                    else
                    {
                        dp.TargetPosition = positions[1];
                    }
                }
            }
            if (idIndex >1)
            {
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            
        }
        [ScriptMethod(name: "P6 First Ice Fire Tether Black Dragon Cone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27955"])]
        public void P6_FirstIceFireTetherBlackDragonCone(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(50);
            dp.Radian = float.Pi / 6f;
            dp.DestoryAt = 7000;
            dp.Name = "P6_FirstIceFireTetherBlackDragonCone";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }
        [ScriptMethod(name: "P6 First Ice Fire Tether White Dragon Cone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27957"])]
        public void P6_FirstIceFireTetherWhiteDragonCone(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(50);
            dp.Radian = float.Pi / 6f;
            dp.DestoryAt = 7000;
            dp.Name = "P6_FirstIceFireTetherWhiteDragonCone";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }

        [ScriptMethod(name: "P6 Endless Cycle", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27969"], userControl: false)]
        public void P6_EndlessCycle(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P6_EndlessCycle";
            dp.Scale = new(4);
            dp.DestoryAt = 8300;
            var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            if(idIndex==0|| idIndex == 2 || idIndex == 4 || idIndex == 6)
            {
                dp.Owner = accessory.Data.PartyList[2];
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);

                dp.Owner = accessory.Data.PartyList[3];
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else
            {
                dp.Owner = accessory.Data.PartyList[3];
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                dp.Owner = accessory.Data.PartyList[2];
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }

        }
        [ScriptMethod(name: "P6 Slaughter Oath Spread", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:27960"], userControl: false)]
        public void P6_SlaughterOathSpread(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.DestoryAt = 7300;
            for (int i = 4; i < accessory.Data.PartyList.Count; i++)
            {
                dp.Name = $"P6_SlaughterOathSpread D{i-3}";
                dp.Owner = accessory.Data.PartyList[i];
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }
        [ScriptMethod(name: "P6 Slaughter Oath Range", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2896"])]
        public void P6_SlaughterOathRange(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(5);
            dp.DestoryAt = 5000;
            dp.Delay = int.TryParse(@event["DurationMilliseconds"], out var d) ? d - 5000 : 0;
            
            dp.Name = $"P6_SlaughterOath";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "P6 First Ice Fire Tether Safe Point (ImGui)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26215"])]
        public void P6_FirstIceFireTetherSafePoint(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor;
            
            dp.Scale = new(0.2f);
            dp.Delay = 20000;
            dp.DestoryAt = 15000;
            dp.Name = $"P6_FirstIceFireTetherSafePoint";

            dp.Position = new(95.7f, 0, 119);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
            dp.Position = new(104.3f, 0, 119);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
            dp.Position = new(100, 0, 109.33f);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P6 Holy Wings (Left Near)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27939"])]
        public void P6_HolyWingsLeftNear(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(10);
            dp.DestoryAt = 8000;
            dp.CentreResolvePattern=PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 1;
            dp.Name = "P6_HolyWingsNear1";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.CentreOrderIndex = 2;
            dp.Name = "P6_HolyWingsNear2";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.Scale = new(22, 50);
            dp2.Owner = id;
            dp2.DestoryAt = 8000;
            dp2.Offset = new(-11, 0, 0);
            dp2.Name = "P6_HolyWingsLeft";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);

        }
        [ScriptMethod(name: "P6 Holy Wings (Left Far)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27940"])]
        public void P6_HolyWingsLeftFar(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(10);
            dp.DestoryAt = 8000;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            dp.CentreOrderIndex = 1;
            dp.Name = "P6_HolyWingsFar1";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.CentreOrderIndex = 2;
            dp.Name = "P6_HolyWingsFar2";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.Scale = new(22, 50);
            dp2.Owner = id;
            dp2.DestoryAt = 8000;
            dp2.Offset = new(-11, 0, 0);
            dp2.Name = "P6_HolyWingsLeft";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);

        }
        [ScriptMethod(name: "P6 Holy Wings (Right Near)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27942"])]
        public void P6_HolyWingsRightNear(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(10);
            dp.DestoryAt = 8000;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 1;
            dp.Name = "P6_HolyWingsNear1";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.CentreOrderIndex = 2;
            dp.Name = "P6_HolyWingsNear2";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.Scale = new(22, 50);
            dp2.Owner = id;
            dp2.DestoryAt = 8000;
            dp2.Offset = new(11, 0, 0);
            dp2.Name = "P6_HolyWingsRight";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);

        }
        [ScriptMethod(name: "P6 Holy Wings (Right Far)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27943"])]
        public void P6_HolyWingsRightFar(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(10);
            dp.DestoryAt = 8000;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            dp.CentreOrderIndex = 1;
            dp.Name = "P6_HolyWingsFar1";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.CentreOrderIndex = 2;
            dp.Name = "P6_HolyWingsFar2";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.Scale = new(22, 50);
            dp2.Owner = id;
            dp2.DestoryAt = 8000;
            dp2.Offset = new(11, 0, 0);
            dp2.Name = "P6_HolyWingsRight";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);

        }
        [ScriptMethod(name: "P6 First Black Dragon Dive", eventType: EventTypeEnum.StartCasting)]
        public void P6_FirstBlackDragonDive(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.2) return;
            if (!uint.TryParse(@event["ActionId"], out var actionId)) return;
            if (actionId != 27939 && actionId != 27940 && actionId != 27942 && actionId != 27943) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(22, 80);
            dp.Owner = darkDragonId;
            dp.DestoryAt = 7500;
            dp.Name = "P6_FirstBlackDragonDive";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P6 Burning Wings", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27948"])]
        public void P6_BurningWings(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(21, 50);
            dp.DestoryAt = 6500;
            dp.Name = "P6_BurningWings";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P6 Burning Tail", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27950"])]
        public void P6_BurningTail(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(18, 50);
            dp.DestoryAt = 6500;
            dp.Name = "P6_BurningTail";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P6 Fireball Range", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:13238"])]
        public void P6_FireballRange(Event @event, ScriptAccessory accessory)
        {
            lock (lockObj)
            {
                p6FireBallCount++;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Color=accessory.Data.DefaultDangerColor;
                //First round
                if (p6FireBallCount == 3)
                {
                    dp.Name = "P6_FireballRange1";
                    dp.Scale = new(18, 44);
                    dp.Position = new Vector3(100, 0, 100);
                    dp.DestoryAt = 12000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                    dp.Rotation = float.Pi / 2;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                }
                //Second round
                if (p6FireBallCount == 6)
                {
                    dp.Name = "P6_FireballRange2";
                    dp.Scale = new(18, 70);
                    var ipos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                    var pos = new Vector3(100, 0, 100);
                    if (ipos.X < 93.5f) pos.X = 87;
                    if (ipos.X > 106.5f) pos.X = 113;
                    if (ipos.Z < 93.5f) pos.Z = 87;
                    if (ipos.Z > 106.5f) pos.Z = 113;
                    dp.Position = pos;
                    dp.Delay = 6000;
                    dp.DestoryAt = 12000 - dp.Delay;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                    dp.Rotation = float.Pi / 2;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                }
                if (p6FireBallCount == 9)
                {
                    dp.Name = "P6_FireballRange3";
                    dp.Scale = new(18, 70);
                    var ipos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                    var pos = new Vector3(100, 0, 100);
                    if (ipos.X < 93.5f) pos.X = 87;
                    if (ipos.X > 106.5f) pos.X = 113;
                    if (ipos.Z < 93.5f) pos.Z = 87;
                    if (ipos.Z > 106.5f) pos.Z = 113;
                    dp.Position = pos;
                    dp.Delay = 8000;
                    dp.DestoryAt = 12000- dp.Delay;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                    dp.Rotation = float.Pi / 2;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                }
            }
        }
        [ScriptMethod(name: "P6 Cross Fire White Dragon Dive", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:27973"])]
        public void P6_CrossFireWhiteDragonDive(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = whiteDragonId;
            dp.Scale = new(22, 80);
            dp.Delay = 1500;
            dp.DestoryAt = 11000- dp.Delay;
            dp.Name = "P6_CrossFireWhiteDragonDive";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        
        [ScriptMethod(name: "P6 Cross Fire Start Position (ImGui)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:13238"])]
        public void P6_CrossFireStartPosition(Event @event, ScriptAccessory accessory)
        {
            lock (lockObj)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                p6FireBallCount2++;
                if (p6FireBallCount2 == 6)
                {
                    dp.Name = "P6_CrossFireStartPosition";
                    dp.Scale = new(1.5f);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color=accessory.Data.DefaultSafeColor;
                    dp.Owner = accessory.Data.Me;

                    var ipos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                    var pos = new Vector3(100, 0, 100);
                    if (ipos.Z < 93.5f) pos.Z = 109.5f;
                    if (ipos.Z > 106.5f) pos.Z = 90.5f;
                    if (p6WhitePos.X < 99) pos.X = 121.5f;
                    else pos.X = 78.5f;

                    dp.TargetPosition = pos;
                    dp.DestoryAt = 6000;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    
                }
                
            }
        }
        [ScriptMethod(name: "P6 Second Ice Fire Tether ND Position (ImGui)", eventType: EventTypeEnum.StartCasting)]
        public void P6_SecondIceFireTetherNDPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.3) return;
            var aidStr = @event["ActionId"];
            if (aidStr != "27956" && aidStr != "27957") return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P6_SecondIceFireTetherNDPosition";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.Scale = new(1.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 7000;

            var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);

            if (idIndex == 2) dp.TargetPosition = new(100, 0, 80.5f);
            if (idIndex == 3) dp.TargetPosition = new(100, 0, 119.7f);
            if (idIndex == 4) dp.TargetPosition = new(103.7f, 0, 89.2f);
            if (idIndex == 5) dp.TargetPosition = new(97, 0, 110.2f);
            if (idIndex == 6) dp.TargetPosition = new(107.2f, 0, 81.7f);
            if (idIndex == 7) dp.TargetPosition = new(92.5f, 0, 118);

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        }
        [ScriptMethod(name: "P6 Dual Dragon Ice Fire Dive", eventType: EventTypeEnum.StatusAdd)]
        public void P6_DualDragonIceFireDive(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.3) return;
            var StatusIDStr = @event["StatusID"];
            if (StatusIDStr != "2898" && StatusIDStr != "2899") return;
            if (!ParseObjectId(@event["TargetId"], out var id) || id != accessory.Data.Me) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(22, 56);
            dp.Delay= 6500;
            dp.DestoryAt = 12500-dp.Delay;
            if (StatusIDStr == "2898")
            {
                dp.Name = "P6_DualDragonIceFireDive_BlackDragon_Fire_Danger";
                dp.Owner = darkDragonId;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

                dp.Name = "P6_DualDragonIceFireDive_WhiteDragon_Fire_Safe";
                dp.Owner = whiteDragonId;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            else
            {
                dp.Name = "P6_DualDragonIceFireDive_BlackDragon_Ice_Safe";
                dp.Owner = darkDragonId;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

                dp.Name = "P6_DualDragonIceFireDive_WhiteDragon_Ice_Danger";
                dp.Owner = whiteDragonId;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            

        }

        [ScriptMethod(name: "P6 Dual Dragon Ice Fire Dive T Black Dragon", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27966"])]
        public void P6_DualDragonIceFireDive_TBlackDragon(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.3) return;
            var index= accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            if (index == 0)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Scale = new(22, 56);
                dp.DestoryAt = 5000;
                dp.Name = "P6_DualDragonIceFireDive_MTBlackDragon_Danger";
                dp.Owner = darkDragonId;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            if (index == 1)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Scale = new(22, 56);
                dp.DestoryAt = 5000;
                dp.Name = "P6_DualDragonIceFireDive_STBlackDragon_Safe";
                dp.Owner = darkDragonId;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }
        [ScriptMethod(name: "P6 Dual Dragon Ice Fire Dive T White Dragon", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27966"])]
        public void P6_DualDragonIceFireDive_TWhiteDragon(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.3) return;
            var index = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            if (index == 0)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Scale = new(22, 56);
                dp.DestoryAt = 5000;
                dp.Name = "P6_DualDragonIceFireDive_MTWhiteDragon_Safe";
                dp.Owner = whiteDragonId;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            if (index == 1)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Scale = new(22, 56);
                dp.DestoryAt = 5000;
                dp.Name = "P6_DualDragonIceFireDive_STWhiteDragon_Danger";
                dp.Owner = whiteDragonId;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }

        [ScriptMethod(name: "P6 Dark Buff Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2758"], userControl: false)]
        public void P6_DarkBuffRecord(Event @event, ScriptAccessory accessory)
        {
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                p6lightDark[accessory.Data.PartyList.ToList().IndexOf(id)] = 1;
            }
            
        }
        [ScriptMethod(name: "P6 Light Buff Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2759"], userControl: false)]
        public void P6_LightBuffRecord(Event @event, ScriptAccessory accessory)
        {
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                p6lightDark[accessory.Data.PartyList.ToList().IndexOf(id)] = 2;
            }

        }
        [ScriptMethod(name: "P6 Evil Flame/Mutual Destruction Flame", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27974"])]
        public void P6_EvilFlame(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(18000).ContinueWith(t =>
            {
                var plist = accessory.Data.PartyList.ToList();
                var idIndex = plist.IndexOf(accessory.Data.Me);
                for (int i = 0; i < p6lightDark.Count; i++)
                {
                    if (p6lightDark[i] == 0) continue;
                    if (p6lightDark[i] == 1)
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Owner = plist[i];
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.Scale = new(5);
                        dp.DestoryAt = 5000;
                        dp.Name = "P6_EvilFlame";
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    }
                    if(p6lightDark[i] == 2)
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Owner = plist[i];
                        dp.Scale = new(4);
                        dp.DestoryAt = 5000;
                        dp.Name = "P6_MutualDestructionFlame";
                        if (i==idIndex ||(p6lightDark.IndexOf(2)==i && p6lightDark.IndexOf(0)==idIndex)|| (p6lightDark.LastIndexOf(2) == i && p6lightDark.LastIndexOf(0) == idIndex))
                        {
                            dp.Color = accessory.Data.DefaultSafeColor;
                        }
                        else
                        {
                            dp.Color = accessory.Data.DefaultDangerColor;
                        }
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    }
                    
                }
                
            });
            

        }
        [ScriptMethod(name: "P6 Evil Flame/Mutual Destruction Flame Mark", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27974"], userControl: false)]
        public void P6_EvilFlameMark(Event @event, ScriptAccessory accessory)
        {
            if (!p6Mark) return;
            accessory.Method.MarkClear();
            Task.Delay(50).ContinueWith(t =>
            {
                var plist = accessory.Data.PartyList.ToList();
                int attack = 0;
                int stop = 8;
                int bind = 5;
                for (int i = 0; i < p6lightDark.Count; i++)
                {
                    if (p6lightDark[i] == 0)
                    {
                        //None
                        stop++;
                        accessory.Method.Mark(plist[i], (MarkType)stop);
                    }
                    if (p6lightDark[i] == 1)
                    {
                        //Spread
                        attack++;
                        accessory.Method.Mark(plist[i], (MarkType)attack);
                    }
                    if (p6lightDark[i] == 2)
                    {
                        //Stack
                        bind++;
                        accessory.Method.Mark(plist[i], (MarkType)bind);
                    }

                }
            });
            Task.Delay(23000).ContinueWith(t =>
            {
                accessory.Method.MarkClear();
            });
        }

        #endregion

        #region P7
        [ScriptMethod(name: "P7 Intro Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29752"], userControl: false)]
        public void P7_IntroRecord(Event @event, ScriptAccessory accessory)
        {
            parse = 7.0;
        }
        [ScriptMethod(name: "P7 Phase Accumulation Floor Fire", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28059"], userControl: false)]
        public void P7_PhaseAccumulationFloorFire(Event @event, ScriptAccessory accessory)
        {
            parse = Math.Round(parse + 0.1, 1);
        }
        [ScriptMethod(name: "P7 Phase Accumulation Death Cycle", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28051"], userControl: false)]
        public void P7_PhaseAccumulationDeathCycle(Event @event, ScriptAccessory accessory)
        {
            parse = Math.Round(parse + 0.1, 1);
        }
        [ScriptMethod(name: "P7 Phase Accumulation Meteor", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28057"], userControl: false)]
        public void P7_PhaseAccumulationMeteor(Event @event, ScriptAccessory accessory)
        {
            parse = Math.Round(parse + 0.1, 1);
        }
        [ScriptMethod(name: "P7 Circle", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2056", "StackCount:42"])]
        public void P7_Circle(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7_Circle";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(8);
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.Owner = id;
            }
            if (parse == 7.3 || parse == 7.6 || parse == 7.9)
            {
                dp.DestoryAt = 8000;
            }
            else
            {
                dp.DestoryAt = 6000;
            }
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P7 Donut", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2056", "StackCount:43"])]
        public void P7_Donut(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7_Donut";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Radian = float.Pi * 2;
            dp.Scale = new(50);
            dp.InnerScale = new(8);
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.Owner = id;
            }
            if (parse == 7.3 || parse == 7.6 || parse == 7.9)
            {
                dp.DestoryAt = 8000;
            }else
            {
                dp.DestoryAt = 6000;
            }
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        [ScriptMethod(name: "P7 Braindead Floor Fire Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28059"])]
        public void P7_BraindeadFloorFirePosition(Event @event, ScriptAccessory accessory)
        {
            var cpos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var r = float.Parse(@event["SourceRotation"]);

            var pos1 = new Vector3(cpos.X + MathF.Sin(r)*-8, cpos.Y, cpos.Z + MathF.Cos(r)*-8);
            var pos2 = new Vector3(cpos.X + MathF.Sin(r) * -14, cpos.Y, cpos.Z + MathF.Cos(r) * -14);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7_BraindeadFloorFirePosition1";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Scale = new(1.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition=pos1;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7_BraindeadFloorFirePosition2";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Scale = new(1.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = pos2;
            dp.Delay = 9000;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7_BraindeadFloorFirePosition3";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Scale = new(1.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Position = pos1;
            dp.TargetPosition = pos2;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);



        }

        [ScriptMethod(name: "P7 Death Cycle Sword Stack Position (Imgui)", eventType: EventTypeEnum.StartCasting)]
        public void P7_DeathCycleSwordStackPosition(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(50).ContinueWith(t =>
            {
                var idstr = @event["ActionId"];
                if (idstr != "29452" && idstr != "29453" && idstr != "29454") return;

                var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);

                var isme = false;
                accessory.Log.Debug($"parse:{parse}");
                if (parse == 7.2 || !p7_116)
                {
                    if (idstr == "29452" && (idIndex == 3 || idIndex == 5 || idIndex == 7)) isme = true;
                    if (idstr == "29453" && (idIndex == 2 || idIndex == 4 || idIndex == 6)) isme = true;
                    if (idstr == "29454" && (idIndex == 0 || idIndex == 1)) isme = true;
                }
                else
                {
                    if (parse == 7.5)
                    {
                        if (idstr == "29452" && (idIndex == 0)) isme = true;
                        if (idstr == "29453" && (idIndex != 0 && idIndex != 1)) isme = true;
                        if (idstr == "29454" && (idIndex == 1)) isme = true;
                    }
                    if (parse == 7.8)
                    {
                        if (idstr == "29452" && (idIndex == 1)) isme = true;
                        if (idstr == "29453" && (idIndex != 0 && idIndex != 1)) isme = true;
                        if (idstr == "29454" && (idIndex == 0)) isme = true;
                    }
                }

                if (isme)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P7_DeathCycleSwordStackPosition";
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Scale = new(1.5f);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    if (ParseObjectId(@event["SourceId"], out var sid))
                    {
                        dp.TargetObject = sid;
                    }
                    dp.DestoryAt = 6700;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P7_DeathCycleSwordStackRange";
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Scale = new(4);
                    if (ParseObjectId(@event["SourceId"], out var sid2))
                    {
                        dp.Owner = sid2;
                    }
                    dp.DestoryAt = 12000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            });
            
            
        }

        [ScriptMethod(name: "P7 First Nuclear Explosion", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28058"])]
        public void P7_FirstNuclearExplosion(Event @event, ScriptAccessory accessory)
        {
            

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7_FirstNuclearExplosion";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(21f);
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P7 Second Nuclear Explosion", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28114"])]
        public void P7_SecondNuclearExplosion(Event @event, ScriptAccessory accessory)
        {


            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7_SecondNuclearExplosion";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(21f);
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.Delay = 9000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P7 Third Nuclear Explosion", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28115"])]
        public void P7_ThirdNuclearExplosion(Event @event, ScriptAccessory accessory)
        {


            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7_ThirdNuclearExplosion";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(21f);
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.Delay = 13000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "P7 First Nuclear Explosion Position Collection", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28058"],userControl:false)]
        public void P7_FirstNuclearExplosionPositionCollection(Event @event, ScriptAccessory accessory)
        {
            p7Stone1 = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        }
        [ScriptMethod(name: "P7 Second Nuclear Explosion Position Collection", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28114"], userControl: false)]
        public void P7_SecondNuclearExplosionPositionCollection(Event @event, ScriptAccessory accessory)
        {
            p7Stone2 = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        }
        [ScriptMethod(name: "P7 Nuclear Explosion 1 to 2 (Imgui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28114"])]
        public void P7_NuclearExplosion1to2(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(50).ContinueWith(t =>
            {
                var cpos = new Vector3(100, 0, 100);
                var dot1 = Vector3.Normalize(cpos - p7Stone1);
                var pos1 = p7Stone1 + dot1 * 21f;
                var stone2pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                var dot2 = Vector3.Normalize(cpos - p7Stone2);
                var pos2 = stone2pos + dot2 * 21f;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P7_NuclearExplosionRun1";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = pos1;
                dp.Scale = new(1.5f);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = "P7_NuclearExplosion1to2";
                dp2.Color = accessory.Data.DefaultSafeColor;
                dp2.Position = pos1;
                dp2.TargetPosition = pos2;
                dp2.Scale = new(1.5f);
                dp2.ScaleMode |= ScaleMode.YByDistance;
                dp2.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);

                var dp3 = accessory.Data.GetDefaultDrawProperties();
                dp3.Name = "P7_NuclearExplosionRun2";
                dp3.Color = accessory.Data.DefaultSafeColor;
                dp3.Owner = accessory.Data.Me;
                dp3.TargetPosition = pos2;
                dp3.Scale = new(1.5f);
                dp3.ScaleMode |= ScaleMode.YByDistance;
                dp3.Delay = 9000;
                dp3.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);
            });
        }
        [ScriptMethod(name: "P7 Nuclear Explosion 2 to 3 (Imgui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28115"])]
        public void P7_NuclearExplosion2to3(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(50).ContinueWith(t =>
            {
                var cpos = new Vector3(100, 0, 100);
                var dot1 = Vector3.Normalize(cpos - p7Stone2);
                var pos1 = p7Stone2 + dot1 * 21f;
                var stone3pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                var dot2 = Vector3.Normalize(cpos - stone3pos);
                var pos2 = stone3pos + dot2 * 21f;

                

                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = "P7_NuclearExplosion2to3";
                dp2.Color = accessory.Data.DefaultSafeColor;
                dp2.Position = pos1;
                dp2.TargetPosition = pos2;
                dp2.Scale = new(1.5f);
                dp2.ScaleMode |= ScaleMode.YByDistance;
                dp2.DestoryAt = 13000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);

                var dp3 = accessory.Data.GetDefaultDrawProperties();
                dp3.Name = "P7_NuclearExplosionRun3";
                dp3.Color = accessory.Data.DefaultSafeColor;
                dp3.Owner = accessory.Data.Me;
                dp3.TargetPosition = pos2;
                dp3.Scale = new(1.5f);
                dp3.ScaleMode |= ScaleMode.YByDistance;
                dp3.Delay = 13000;
                dp3.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);
            });
        }

        #endregion

        [ScriptMethod(name: "TargetIconPrint", eventType: EventTypeEnum.TargetIcon)]
        public void TestTargetIcon(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"TargetIcon: {@event["TargetId"]} {ParsTargetIcon(@event["Id"])}"); 
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
        /// Floor to nearest
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
        /// Round to nearest
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
        private int PositionTo12Dir(Vector3 point, Vector3 centre)
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r = Math.Round(6 - 6 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 12;
            return (int)r;

        }
    }
}
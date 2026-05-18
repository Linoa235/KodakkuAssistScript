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
    [ScriptType(name: "Valigarmanda EX Draw", territorys: [1196], guid: "6faecd9c-0679-4995-b0b6-483a80ad6fc6", version: "0.0.0.3", Author: "Linoa235")]
    public class ValigarmandaExDraw
    {
        int? firstTargetIcon = null;

        /// <summary>
        /// 1=fire, 2=storm, 3=ice
        /// </summary>
        int parse = 0;
        bool iceCross = false;
        object icelock = new object();

        public void Init(ScriptAccessory accessory)
        {
            firstTargetIcon = null;
            accessory.Method.RemoveDraw(@".*");
            //accessory.Method.MarkClear();
        }

        [ScriptMethod(name: "Fire Phase", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38340"], userControl: false)]
        public void FirePhase(Event @event, ScriptAccessory accessory)
        {
            parse = 1;
        }

        [ScriptMethod(name: "Wind Phase", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38339"], userControl: false)]
        public void WindPhase(Event @event, ScriptAccessory accessory)
        {
            parse = 2;
        }

        [ScriptMethod(name: "Ice Phase", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36817"], userControl: false)]
        public void IcePhase(Event @event, ScriptAccessory accessory)
        {
            parse = 3;
        }

        #region Buffs
        [ScriptMethod(name: "Fire Stack", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3817"])]
        public void FireStack(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Fire Stack";
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Enhanced Fire Stack", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3818"])]
        public void EnhancedFireStack(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Enhanced Fire Stack";
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.Delay = 70000;
            dp.DestoryAt = 12000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Triple Fire Stack", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3819"])]
        public void TripleFireStack(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Triple Fire Stack";
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Thunder Spread", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3823"])]
        public void ThunderSpread(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Thunder Spread";
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 6000;
            dp.Delay = int.Parse(@event["DurationMilliseconds"]) - dp.DestoryAt;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Enhanced Thunder Spread", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3824"])]
        public void EnhancedThunderSpread(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Enhanced Thunder Spread";
            dp.Scale = new(8);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 6000;
            dp.Delay = int.Parse(@event["DurationMilliseconds"]) - dp.DestoryAt;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Enhanced Ice Circle", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3821"])]
        public void EnhancedIceCircle(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Enhanced Ice Circle";
            dp.Scale = new(16);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.Delay = 6000;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        #endregion

        #region Action Skills
        [ScriptMethod(name: "Gymnastics Chariot", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36812"])]
        public void GymnasticsChariot(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Action Chariot";
            dp.Scale = new(24);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 7300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Gymnastics Donut", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36816"])]
        public void GymnasticsDonut(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Gymnastics Donut";
            dp.Scale = new(30);
            dp.Radian = float.Pi * 2;
            dp.InnerScale = new(8);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = new(100, 0, 100);
            dp.DestoryAt = 7300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }

        [ScriptMethod(name: "Gymnastics Cone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36808"])]
        public void GymnasticsCone(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Gymnastics Cone";
            dp.Scale = new(50);
            dp.Radian = float.Pi / 18 * 8;
            dp.Offset = new(0, 0, 10);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 7300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Gymnastics Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(36808|36812|36816)$"])]
        public void GymnasticsStack(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            accessory.Log.Debug(@event["ActionId"]);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Gymnastics Stack";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 8000;

            dp.Owner = accessory.Data.PartyList[0];
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Owner = accessory.Data.PartyList[1];
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Owner = accessory.Data.PartyList[2];
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Owner = accessory.Data.PartyList[3];
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        #endregion

        #region Fire Phase
        [ScriptMethod(name: "T Tank Tower Cone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36889"])]
        public void TTankTowerCone(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Action Cone";
            dp.Scale = new(40);
            dp.Radian = float.Pi / 18 * 3;
            dp.Rotation = float.Pi;
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["SourceId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Volcanic Eruption Left", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00200010", "Index:0000000F"])]
        public void VolcanicEruptionLeft(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Volcanic Eruption Left";
            dp.Scale = new(22);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = new(85, 0, 100);
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Volcanic Eruption Right", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00200010", "Index:0000000E"])]
        public void VolcanicEruptionRight(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Volcanic Eruption Right";
            dp.Scale = new(22);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = new(115, 0, 100);
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        #endregion

        #region Thunder Phase
        [ScriptMethod(name: "Feather AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36833"])]
        public void FeatherAOE(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Feather AOE";
            dp.Scale = new(8);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 5800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Arena Edge Laser", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:16770"])]
        public void ArenaEdgeLaser(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Arena Edge Laser";
            dp.Scale = new(5, 50);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        #endregion

        #region Ice Phase
        [ScriptMethod(name: "Avalanche Right Front", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00200010", "Index:00000003"])]
        public void AvalancheRightFront(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Avalanche Right Front";
            dp.Scale = new(50, 24);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = new(100, 0, 100);
            dp.Rotation = 53.13f / 180 * float.Pi + float.Pi / 2;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Avalanche Left Back", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00020001", "Index:00000003"])]
        public void AvalancheLeftBack(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Avalanche Right Front";
            dp.Scale = new(50, 24);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = new(100, 0, 100);
            dp.Rotation = 36.87f / 180 * -float.Pi;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Ice Flower Cross Safe Spot", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:16667"])]
        public void IceFlowerCrossSafeSpot(Event @event, ScriptAccessory accessory)
        {
            lock (icelock)
            {
                if (iceCross) return;
                var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                Vector3 pos11 = new(97.5f, 0, 107.5f);
                Vector3 pos12 = new(92.5f, 0, 87.5f);
                Vector3 pos13 = new(117.5f, 0, 97.5f);
                Vector3 pos14 = new(87.5f, 0, 102.5f);
                Vector3 pos15 = new(82.5f, 0, 112.5f);

                Vector3 pos21 = new(82.5f, 0, 97.5f);
                Vector3 pos22 = new(102.5f, 0, 107.5f);
                Vector3 pos23 = new(107.5f, 0, 87.5f);
                Vector3 pos24 = new(112.5f, 0, 102.5f);
                Vector3 pos25 = new(117.5f, 0, 112.5f);

                if ((pos - pos11).Length() < 1f || (pos - pos12).Length() < 1f || (pos - pos13).Length() < 1f || (pos - pos14).Length() < 1f || (pos - pos15).Length() < 1f)
                {
                    iceCross = true;
                    Task.Delay(1000).ContinueWith(y => { iceCross = false; });
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Ice Flower Cross Safe Spot";
                    dp.Scale = new(2.07f, 3.54f);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Position = new(108.23f, 0, 91.77f);
                    dp.Rotation = float.Pi / -4;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
                }
                if ((pos - pos21).Length() < 1f || (pos - pos22).Length() < 1f || (pos - pos23).Length() < 1f || (pos - pos24).Length() < 1f || (pos - pos25).Length() < 1f)
                {
                    iceCross = true;
                    Task.Delay(1000).ContinueWith(y => { iceCross = false; });
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Ice Flower Cross Safe Spot";
                    dp.Scale = new(2.07f, 3.54f);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Position = new(91.77f, 0, 91.77f);
                    dp.Rotation = float.Pi / 4;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
                }
            }
        }
        #endregion

        #region Stack Hit Pillar
        [ScriptMethod(name: "Stack Hit Pillar Stack Range", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:34722"])]
        public void StackHitPillar(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Stack Hit Pillar";
            dp.Scale = new(6, 80);
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["SourceId"], out var cid))
            {
                dp.Owner = cid;
            }
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.TargetObject = tid;
            }
            dp.DestoryAt = 99000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Remove Stack Range", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:38245"], userControl: false)]
        public void RemoveStackRange(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(@".*");
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

        private int ParsTargetIcon(string id)
        {
            firstTargetIcon ??= int.Parse(id, System.Globalization.NumberStyles.HexNumber);
            return int.Parse(id, System.Globalization.NumberStyles.HexNumber) - (int)firstTargetIcon;
        }

        private Vector3 RotatePoint(Vector3 point, Vector3 centre, float radian)
        {
            Vector2 v2 = new(point.X - centre.X, point.Z - centre.Z);
            var rot = (MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian);
            var lenth = v2.Length();
            return new(centre.X + MathF.Sin(rot) * lenth, centre.Y, centre.Z - MathF.Cos(rot) * lenth);
        }

        private int PositionFloorTo4Dir(Vector3 point, Vector3 centre)
        {
            var r = Math.Floor(2 - 2 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 4;
            return (int)r;
        }

        private int PositionRoundTo4Dir(Vector3 point, Vector3 centre)
        {
            var r = Math.Round(2 - 2 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 4;
            return (int)r;
        }

        private int PositionTo8Dir(Vector3 point, Vector3 centre)
        {
            var r = Math.Round(4 - 4 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 8;
            return (int)r;
        }

        private int PositionTo12Dir(Vector3 point, Vector3 centre)
        {
            var r = Math.Round(6 - 6 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 12;
            return (int)r;
        }
    }
}
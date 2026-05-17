using System;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;

namespace Cyf5119Script.Dawntrail.TenderValley
{
    [ScriptType(guid: "C6AAF3DF-64BA-15C2-41F8-D24F7F4656DD", name: "Tender Valley", territorys: [1203], version: "0.0.0.5", author: "Cyf5119")]
    public class TenderValley
    {
        private uint
            stack1id = 0,
            stack2id = 0,
            stack3id = 0;

        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(".*");
            stack1id = 0;
            stack2id = 0;
            stack3id = 0;
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

        private static readonly (Vector3 Position, Vector2 Shape)[] AOEMap =
        [
            (new(-112.5f, -167f, -486.5f), new(7.5f, 22f)),
            (new(-147.5f, -167f, -471.5f), new(7.5f, 22f)),
            (new(-147.5f, -167f, -486.5f), new(7.5f, 12f)),
            (new(-112.5f, -167f, -471.5f), new(7.5f, 12f))
        ];

        private static Vector2 GetShape(Vector3 position)
        {
            foreach (var (pos, shape) in AOEMap)
                if (Math.Abs(position.X - pos.X) < 1 && Math.Abs(position.Z - pos.Z) < 1)
                    return shape;
            return new Vector2(7.5f, 35f);
        }

        #region BOSS1

        [ScriptMethod(name: "Boss1 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37392"])]
        public void Boss1Aoe(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("AOE", duration: 5000);
        }

        [ScriptMethod(name: "Boss1 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39242"])]
        public void Boss1Tankbuster(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("Tankbuster", duration: 5000);
        }

        [ScriptMethod(name: "Boss1 Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37391"])]
        public void Boss1Stack(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            dp.Name = "Boss1 Stack";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5000;
            dp.Owner = tid;
            dp.Scale = new(6);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Boss1 Fan", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37386"])]
        public void Boss1Fan(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            dp.Name = $"Boss1 Fan";
            dp.Color = new(0.2f, 1f, 1f, 1.6f);
            dp.DestoryAt = 6500;
            dp.Owner = sid;
            dp.Scale = new(36);
            dp.Radian = float.Pi / 180 * 50;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Boss1 Large Fan", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3915[45])$"])]
        public void Boss1Fan2(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
            dp.Name = $"Boss1 Large Fan";
            dp.Color = new(0.2f, 1f, 1f, 1f);
            dp.DestoryAt = 7000;
            dp.Owner = sid;
            dp.Rotation = aid == 39154 ? float.Pi / -2 : float.Pi / 2;
            dp.Scale = new(36);
            dp.Radian = float.Pi / 180 * 330;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Boss1 Adds", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3738[89]$)"])]
        public void Boss1Adds(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
            dp.Name = $"Boss1 Adds {sid:X}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            dp.Owner = sid;
            dp.Scale = new(aid == 37388 ? 6 : 11);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Boss1 Knockback Prediction", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37390"])]
        public void Boss1Knockback(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Boss1 Knockback Prediction";
            dp.Color = new(0.2f, 1f, 1f, 1.6f);
            dp.DestoryAt = 6000;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = new(-65f, -4f, 470f);
            dp.Rotation = float.Pi;
            dp.Scale = new(1.5f, 20);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }

        #endregion

        
        #region BOSS2

        [ScriptMethod(name: "Boss2 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36557"])]
        public void Boss2Aoe(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("AOE", duration: 5000);
        }

        [ScriptMethod(name: "Boss2 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38468"])]
        public void Boss2Tankbuster(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("Tankbuster", duration: 5000);
        }

        [ScriptMethod(name: "Boss2 Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36555"])]
        public void Boss2Stack(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            dp.Name = $"Boss2 Stack";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5000;
            dp.Owner = tid;
            dp.Scale = new(6);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Boss2 Circle Bomb", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3640[12])$"])]
        public void Boss2Circle(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
            dp.Name = $"Boss2 Circle Bomb {sid:X}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = aid == 36401 ? 11500 : 8500;
            dp.Owner = sid;
            dp.Scale = new(10);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Boss2 Line Bomb", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(365(45|51))$"])]
        public void Boss2Rect(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
            dp.Name = $"Boss2 Line Bomb {sid:X}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = aid == 36545 ? 11500 : 8500;
            dp.Owner = sid;
            dp.Scale = new(6f, 40f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        #endregion

        [ScriptMethod(name: "Midpath Statue", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0025"])]
        public void MidpathStatue(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            dp.Owner = sid;
            dp.Name = $"Midpath Statue";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 8100;
            dp.Scale = GetShape(pos);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        #region BOSS3

        [ScriptMethod(name: "Boss3 AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36748"])]
        public void Boss3Aoe(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("AOE", duration: 5000);
        }

        [ScriptMethod(name: "Boss3 Tankbuster", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36744"])]
        public void Boss3Tankbuster(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("Tankbuster", duration: 5000);
        }

        [ScriptMethod(name: "Boss3 Stack ID 1", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:003E"], userControl: false)]
        public void Boss3StackId1(Event @event, ScriptAccessory accessory)
        {
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                stack1id = tid;
            }
        }

        [ScriptMethod(name: "Boss3 Stack ID 2", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:021E"], userControl: false)]
        public void Boss3StackId2(Event @event, ScriptAccessory accessory)
        {
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                stack2id = tid;
            }
        }

        [ScriptMethod(name: "Boss3 Stack ID 3", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:021F"], userControl: false)]
        public void Boss3StackId3(Event @event, ScriptAccessory accessory)
        {
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                stack3id = tid;
            }
        }

        [ScriptMethod(name: "Boss3 Stack 1", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36753"])]
        public void Boss3Stack1(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            if (stack1id == 0) return;
            dp.Owner = stack1id;
            dp.Name = $"Boss3 Stack 1";
            dp.Scale = new(6);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Boss3 Stack 2", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36752"])]
        public void Boss3Stack2(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            if (stack2id == 0) return;
            dp.Owner = stack2id;
            dp.Name = $"Boss3 Stack 2";
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Boss3 Stack 3", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36751"])]
        public void Boss3Stack3(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            if (stack3id == 0) return;
            dp.Owner = stack3id;
            dp.Name = $"Boss3 Stack 3";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Boss3 Circle", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36749"])]
        public void Boss3Circle(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }

            dp.Name = $"Boss3 Circle {sid:X}";
            dp.Scale = new(9);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Boss3 Line", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36750"])]
        public void Boss3Line(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }

            dp.Name = $"Boss3 Line {sid:X}";
            dp.Scale = new(5f, 52f);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }

        [ScriptMethod(name: "Boss3 Knockback Prediction", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36756"])]
        public void Boss3Knockback(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.TargetObject = sid;
            }

            dp.Name = "Boss3 Knockback Prediction";
            dp.Scale = new(1.5f, 15);
            dp.Color = new(0.2f, 1f, 1f, 1.6f);
            dp.Owner = accessory.Data.Me;
            dp.Rotation = float.Pi;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "Boss3 Maze Safe Zone", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:00000001"])]
        public void Boss3MazeSafeZone(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            var id = @event["Id"];
            switch (id)
            {
                case "01000080":
                    dp.Position = new(-124f, -170f, -552f);
                    break;
                case "04000200":
                    dp.Position = new(-128f, -170f, -560f);
                    break;
                case "10000800":
                    dp.Position = new(-132f, -170f, -548f);
                    break;
                case "00020001":
                    dp.Position = new(-136f, -170f, -556f);
                    break;
                case "00100004" or "00200004" or "00400004" or "00080004":
                    accessory.Method.RemoveDraw("Boss3 Maze Safe Zone");
                    return;
            }

            dp.Name = "Boss3 Maze Safe Zone";
            dp.Scale = new(4);
            dp.Color = new(0.2f, 1f, 0.2f, 1.6f);
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }

        #endregion
    }
}
using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.Draw;
using System.Numerics;
using Newtonsoft.Json;
using KodakkuAssist.Module.Draw.Manager;

namespace A8S_Scripts
{
    [ScriptType(name: "Alexander - The Burden of the Son (A8S)", territorys: [532], guid: "2113EAA7-0C90-4802-AA48-C9CFF30BFBEA", version: "0.0.1", Author: "Linoa235")]
    public class A8S
    {
        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(".*");
        }

        [ScriptMethod(name: "Mega Beam",
                      eventType: EventTypeEnum.StartCasting,
                      eventCondition: ["ActionId:regex:^(5678|5732)$"])]
        public void MegaBeam(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["DurationMilliseconds"], out var castTime))
            {
                castTime = 3000;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "A8S_MegaBeam_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(6, 70);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = castTime;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Double Rocket Punch",
                      eventType: EventTypeEnum.StartCasting,
                      eventCondition: ["ActionId:5731"])]
        public void DoubleRocketPunch(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["DurationMilliseconds"], out var castTime))
            {
                castTime = 4000;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "A8S_DoubleRocketPunch_Danger_Zone";
            dp.Owner = @event.TargetId;
            dp.Scale = new Vector2(3, 3);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = castTime;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Super Jump",
                      eventType: EventTypeEnum.StartCasting,
                      eventCondition: ["ActionId:5733"])]
        public void SuperJump(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["DurationMilliseconds"], out var castTime))
            {
                castTime = 3000;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "A8S_SuperJump_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(10, 10);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = castTime;

            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Apocalyptic Ray",
                      eventType: EventTypeEnum.ActionEffect,
                      eventCondition: ["ActionId:5734"])]
        public void ApocalypticRay(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "A8S_ApocalypticRay_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(25, 25);
            dp.Radian = MathF.PI / 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Apocalyptic Ray (Cast)",
                      eventType: EventTypeEnum.StartCasting,
                      eventCondition: ["ActionId:5734"])]
        public void ApocalypticRayCast(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["DurationMilliseconds"], out var castTime))
            {
                castTime = 2000;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "A8S_ApocalypticRay_Cast_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(25, 25);
            dp.Radian = MathF.PI / 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = castTime;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Laser Chakram",
                      eventType: EventTypeEnum.StartCasting,
                      eventCondition: ["ActionId:5716"])]
        public void LaserChakram(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["DurationMilliseconds"], out var castTime))
            {
                castTime = 5000;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "A8S_LaserChakram_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(6, 70);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = castTime;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Phantom System",
                      eventType: EventTypeEnum.TargetIcon,
                      eventCondition: ["Id:0008"])]
        public void PhantomSystem(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "A8S_PhantomSystem_Danger_Zone";
            dp.Owner = @event.TargetId;
            dp.Scale = new Vector2(5, 5);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
}
using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.Draw;
using System.Numerics;
using Newtonsoft.Json;
using KodakkuAssist.Module.Draw.Manager;

namespace A12S_Scripts
{
    [ScriptType(name: "Alexander - The Soul of the Creator (A12S, guid: "e428df0f-3f95-4ac7-a6ab-d352d077d865")", territorys: [587], $16ac12f96-63c9-44f5-a2a8-afa55ee21671", version: "0.0.1", Author: "Linoa235")]
    public class A12S
    {
        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(".*");
        }

        [ScriptMethod(name: "Punishing Ray",
                      eventType: EventTypeEnum.StartCasting,
                      eventCondition: ["ActionId:6633"])]
        public void PunishingRay(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["DurationMilliseconds"], out var castTime))
            {
                castTime = 4000;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "A12S_PunishingRay_Danger_Zone";
            dp.Owner = @event.TargetId;
            dp.Scale = new Vector2(5, 5);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = castTime;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "White Archon Flame",
                      eventType: EventTypeEnum.TargetIcon,
                      eventCondition: ["Id:001E"])]
        public void WhiteArchonFlame(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "A12S_WhiteArchonFlame_Danger_Zone";
            dp.Owner = @event.TargetId;
            dp.Scale = new Vector2(4, 4);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Gravity Anomaly",
                      eventType: EventTypeEnum.StartCasting,
                      eventCondition: ["ActionId:6642"])]
        public void GravityAnomaly(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["DurationMilliseconds"], out var castTime))
            {
                castTime = 5000;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "A12S_GravityAnomaly_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(8, 8);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = castTime;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Holy Rite",
                      eventType: EventTypeEnum.StartCasting,
                      eventCondition: ["ActionId:6637"])]
        public void HolyRite(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "A12S_HolyRite_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(60, 60);
            dp.InnerScale = new Vector2(8, 8);
            dp.Radian = MathF.PI * 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }

        [ScriptMethod(name: "Cross Holy",
                      eventType: EventTypeEnum.StartCasting,
                      eventCondition: ["ActionId:6635"])]
        public void CrossHoly(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["DurationMilliseconds"], out var castTime))
            {
                castTime = 6000;
            }

            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = "A12S_CrossHoly_Danger_Zone_1";
            dp1.Owner = @event.SourceId;
            dp1.Scale = new Vector2(16, 120);
            dp1.Color = accessory.Data.DefaultDangerColor;
            dp1.DestoryAt = castTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = "A12S_CrossHoly_Danger_Zone_2";
            dp2.Owner = @event.SourceId;
            dp2.Scale = new Vector2(16, 120);
            dp2.Rotation = MathF.PI / 2;
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.DestoryAt = castTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp2);
        }

        [ScriptMethod(name: "Dishonor",
                      eventType: EventTypeEnum.StatusAdd,
                      eventCondition: ["StatusID:1120"])]
        public void Dishonor(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "A12S_Dishonor_Danger_Zone";
            dp.Owner = @event.TargetId;
            dp.Scale = new Vector2(30, 30);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 15000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
    }
}
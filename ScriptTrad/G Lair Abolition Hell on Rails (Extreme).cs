using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.GameEvent.Struct;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;

namespace RyougiMioScriptNamespace
{
    [ScriptType(name: "G Lair Abolition: Hell on Rails (Extreme, guid: "66cf2afa-74d9-4f44-a57f-46777ee4ed6d")", territorys: [1308], $1385f7670-e5d3-41ca-aaa5-b5a5c566427b", version: "0.0.0.2", Author: "Linoa235", note: "Bug fix")]
    public class Script1308
    {
        [UserSetting("Common Danger Color")]
        public ScriptColor DangerColor { get; set; } = new ScriptColor() { V4 = new Vector4(1.0f, 0.0f, 0.0f, 1.0f) };

        [UserSetting("Common Safe Color")]
        public ScriptColor SafeColor { get; set; } = new ScriptColor() { V4 = new Vector4(0.0f, 1.0f, 0.0f, 1.0f) };
        
        [UserSetting("Current Phase (PhaseCount) - Debug Only")]
        public int phaseCount { get; set; } = 1;

        [UserSetting("Skill Count (SkillCount) - Debug Only")]
        public int skillCount { get; set; } = 0;
        
        private int skillCount1 = 0;

        public void Init(ScriptAccessory accessory)
        {
            phaseCount = 1;
            skillCount = 0;
            skillCount1 = 0;
            accessory.Method.RemoveDraw(".*");
            accessory.Method.SendChat("/e HoR(EX) Initialized.");
        }

        [ScriptMethod(name: "Count Update_45663/45664", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45663|45664)$"], userControl: false)]
        public void UpdatePhaseBySkillCount(Event @event, ScriptAccessory accessory)
        {
            skillCount++;

            if (skillCount == 4)
            {
                this.phaseCount++;
            }
            if (skillCount >= 5)
            {
                this.phaseCount = 6;
            }
        }

        [ScriptMethod(name: "Thunder Aura (P1)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:19000"], userControl: true)]
        public void DrawLightningRect(Event @event, ScriptAccessory accessory)
        {
            if (@event.SourcePosition.Y > 4.0f || phaseCount > 2)
            {
                return;
            }

            var dpGrow = accessory.Data.GetDefaultDrawProperties();
            dpGrow.Name = $"LightningRect_Grow_{@event.SourceId}";
            dpGrow.Owner = @event.SourceId;
            dpGrow.Color = accessory.Data.DefaultDangerColor;
            dpGrow.Offset = new Vector3(0, 0, -15);
            dpGrow.DestoryAt = 7030;
            dpGrow.ScaleMode = ScaleMode.XByTime;
            dpGrow.Scale = new Vector2(5f, 30f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dpGrow);

            var dpStatic = accessory.Data.GetDefaultDrawProperties();
            dpStatic.Name = $"LightningRect_Static_{@event.SourceId}";
            dpStatic.Owner = @event.SourceId;
            dpStatic.Color = accessory.Data.DefaultDangerColor;
            dpStatic.Offset = new Vector3(0, 0, -15);
            dpStatic.ScaleMode = ScaleMode.None;
            dpStatic.Scale = new Vector2(5f, 30f);
            dpStatic.Delay = 7030;
            dpStatic.DestoryAt = 970;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dpStatic);
        }

        [ScriptMethod(name: "Thunder Aura (P5)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:19000"], userControl: true)]
        public void DrawLightningRect1(Event @event, ScriptAccessory accessory)
        {
            if ((@event.SourcePosition.X > 95.0f && @event.SourcePosition.X < 100.0f) || phaseCount < 2)
            {
                return;
            }

            var dpGrow = accessory.Data.GetDefaultDrawProperties();
            dpGrow.Name = $"LightningRect_Grow_{@event.SourceId}";
            dpGrow.Owner = @event.SourceId;
            dpGrow.Color = accessory.Data.DefaultDangerColor;
            dpGrow.Offset = new Vector3(0, 0, -15);
            dpGrow.DestoryAt = 7030;
            dpGrow.ScaleMode = ScaleMode.XByTime;
            dpGrow.Scale = new Vector2(5f, 30f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dpGrow);

            var dpStatic = accessory.Data.GetDefaultDrawProperties();
            dpStatic.Name = $"LightningRect_Static_{@event.SourceId}";
            dpStatic.Owner = @event.SourceId;
            dpStatic.Color = accessory.Data.DefaultDangerColor;
            dpStatic.Offset = new Vector3(0, 0, -15);
            dpStatic.ScaleMode = ScaleMode.None;
            dpStatic.Scale = new Vector2(5f, 30f);
            dpStatic.Delay = 7030;
            dpStatic.DestoryAt = 970;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dpStatic);
        }

        [ScriptMethod(name: "Overboost Rush", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45670"], userControl: true)]
        public void DrawBossArrow(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Boss_Arrow_{@event.SourceId}";
            dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
            dp.Position = new Vector3(100, 0, 40 + phaseCount * 50);
            dp.Rotation = 0;
            dp.DestoryAt = 7000;
            dp.ScaleMode = ScaleMode.YByTime;
            dp.Scale = new Vector2(10f, 20f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Arrow, dp);
        }

        [ScriptMethod(name: "Overboost Fog Draw", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45696"], userControl: true)]
        public void DrawBossArrow_f(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Boss_Arrow_{@event.SourceId}";
            dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
            dp.Position = new Vector3(100, 0, 60 + phaseCount * 50);
            dp.Rotation = 3.14159f;
            dp.DestoryAt = 7000;
            dp.ScaleMode = ScaleMode.YByTime;
            dp.Scale = new Vector2(10f, 20f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Arrow, dp);
        }

        [ScriptMethod(name: "Overboost", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45663|45664)$"])]
        public void DrawPhase1Circles(Event @event, ScriptAccessory accessory)
        {
            skillCount1 = skillCount1 + 1;
            long delay = 0;
            long duration = 5000;

            if (skillCount1 == 1 || skillCount1 == 2)
            {
                delay = 12000;
            }
            else
            {
                if (skillCount1 == 3)
                {
                    delay = 19500;
                }
                else if (skillCount1 == 4)
                {
                    delay = 21500;
                }
                else if (skillCount1 == 5)
                {
                    delay = 52500;
                }
                else if (skillCount1 == 6)
                {
                    delay = 49500;
                }
                else
                {
                    return;
                }
            }

            if (@event.ActionId == 45663)
            {
                foreach (var playerId in accessory.Data.PartyList)
                {
                    var player = accessory.Data.Objects.SearchById(playerId);
                    if (player == null || player.IsDead) continue;

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Circle_45663_{playerId}_{@event.SourceId}";
                    dp.Owner = playerId;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = delay;
                    dp.DestoryAt = duration;
                    dp.ScaleMode = ScaleMode.ByTime;
                    dp.Scale = new Vector2(5.0f, 5.0f);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
            else if (@event.ActionId == 45664)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Circle_45664_{accessory.Data.Me}_{@event.SourceId}";
                dp.Owner = accessory.Data.Me;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = delay;
                dp.DestoryAt = duration;
                dp.ScaleMode = ScaleMode.ByTime;
                dp.Scale = new Vector2(5.0f, 5.0f);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }

        [ScriptMethod(name: "Guardian Turret", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45681|45683|45686)$"], userControl: true)]
        public void DrawTurretLaser(Event @event, ScriptAccessory accessory)
        {
            uint actionId = @event.ActionId;
            Vector3 srcPos = @event.SourcePosition;
            float duration = 7000f;

            bool isLeftTurret = Math.Abs(srcPos.X - 85) < 2;
            bool isRightTurret = Math.Abs(srcPos.X - 115) < 2;

            if (!isLeftTurret && !isRightTurret) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Turret_{actionId}_{@event.SourceId}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = (long)duration;

            float rotation = isLeftTurret ? -float.Pi / 2 : float.Pi / 2;
            dp.Rotation = rotation;
            dp.ScaleMode = ScaleMode.XByTime;

            if (actionId == 45681)
            {
                float length = 5f;
                float width = 20f;
                if (isLeftTurret) { dp.Offset = new Vector3(0f, 0, -15); }
                if (isRightTurret) { dp.Offset = new Vector3(0, 0, -15); }
                dp.Scale = new Vector2(width, length);
                dp.Owner = @event.SourceId;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
            }
            else if (actionId == 45683)
            {
                float length = 5f;
                float width = 15f;
                if (isLeftTurret) { dp.Offset = new Vector3(0, 0, -12.5f); }
                if (isRightTurret) { dp.Offset = new Vector3(0, 0, -12.5f); }
                dp.Scale = new Vector2(width, length);
                dp.Owner = @event.SourceId;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
            }
        }

        [ScriptMethod(name: "G Lair Clone_Mark", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:19329"], userControl: true)]
        public void MarkClone(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Clone_Mark_{@event.SourceId}";
            dp.Owner = @event.SourceId;
            dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
            dp.Scale = new Vector2(2.0f);
            dp.DestoryAt = 120000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "G Lair Clone_Cone", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:19329"], userControl: true)]
        public void DrawCloneFanToTanks(Event @event, ScriptAccessory accessory)
        {
            var tankIds = accessory.Data.PartyList.Where(id =>
            {
                var obj = accessory.Data.Objects.SearchById(id);
                return obj is IBattleChara bc && bc.IsTank();
            }).ToList();

            foreach (var tankId in tankIds)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Clone_Fan_To_Tank_{tankId}_{@event.SourceId}";
                dp.Owner = @event.SourceId;
                dp.TargetObject = tankId;
                dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 0.1f);
                dp.Radian = float.Pi / 5.143f;
                dp.Scale = new Vector2(60.0f);
                dp.DestoryAt = 120000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
        }

        [ScriptMethod(name: "G Lair Clone_Spread/Stack", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(027D|027E)$"], userControl: true)]
        public void DrawSpreadStackByIcon(Event @event, ScriptAccessory accessory)
        {
            string iconId = @event["Id"];
            long duration = 6500;

            if (iconId == "027E")
            {
                foreach (var playerId in accessory.Data.PartyList)
                {
                    var player = accessory.Data.Objects.SearchById(playerId);
                    if (player == null || player.IsDead) continue;

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Icon_Spread_{playerId}_{@event.SourceId}";
                    dp.Owner = playerId;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = duration;
                    dp.ScaleMode = ScaleMode.ByTime;
                    dp.Scale = new Vector2(5.0f, 5.0f);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
            else if (iconId == "027D")
            {
                foreach (var playerId in accessory.Data.PartyList)
                {
                    var player = accessory.Data.Objects.SearchById(playerId);
                    if (player is IBattleChara bc && bc.IsHealer())
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"Icon_Stack_Healer_{playerId}_{@event.SourceId}";
                        dp.Owner = playerId;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = duration;
                        dp.ScaleMode = ScaleMode.ByTime;
                        dp.Scale = new Vector2(5.0f, 5.0f);
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    }
                }
            }
        }

        [ScriptMethod(name: "Lightning Flash", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45666"], userControl: true)]
        public void DrawLightningFlash(Event @event, ScriptAccessory accessory)
        {
            var pos = @event.EffectPosition;
            var drawPos = new Vector3(pos.X, 0, pos.Z);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"LightningFlash_{@event.SourceId}_{@event.Id}";
            dp.Position = drawPos;
            dp.Color = DangerColor.V4;
            dp.Scale = new Vector2(4.0f);
            dp.Delay = 0;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Headlight", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45690"], userControl: true)]
        public void DrawHeadlightRects(Event @event, ScriptAccessory accessory)
        {
            var srcPos = @event.SourcePosition;
            var srcRot = @event.SourceRotation;
            float width = 20f;
            float length = 30f;
            Vector3 rectOffset = new Vector3(0, 0, -10f);
            Vector4 blueColor = new Vector4(0.0f, 1.0f, 1.0f, 0.2f);

            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"Headlight_High_{@event.SourceId}";
            dp1.Position = new Vector3(srcPos.X, 5.0f, srcPos.Z);
            dp1.Rotation = srcRot;
            dp1.Color = accessory.Data.DefaultDangerColor;
            dp1.Offset = rectOffset;
            dp1.DestoryAt = 6700;
            dp1.ScaleMode = ScaleMode.YByTime;
            dp1.Scale = new Vector2(width, length);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = $"Headlight_Low_{@event.SourceId}";
            dp2.Position = new Vector3(srcPos.X, 0.0f, srcPos.Z);
            dp2.Rotation = srcRot;
            dp2.Color = blueColor;
            dp2.Offset = rectOffset;
            dp2.Delay = 6700;
            dp2.DestoryAt = 2700;
            dp2.ScaleMode = ScaleMode.YByTime;
            dp2.Scale = new Vector2(width, length);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);
        }

        [ScriptMethod(name: "Thunder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45687"], userControl: true)]
        public void DrawHeadlightRects1(Event @event, ScriptAccessory accessory)
        {
            var srcPos = @event.SourcePosition;
            var srcRot = @event.SourceRotation;
            float width = 20f;
            float length = 30f;
            Vector3 rectOffset = new Vector3(0, 0, -10f);
            Vector4 blueColor = new Vector4(0.0f, 1.0f, 1.0f, 0.2f);

            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"Headlight_High_{@event.SourceId}";
            dp1.Position = new Vector3(srcPos.X, 0.0f, srcPos.Z);
            dp1.Rotation = srcRot;
            dp1.Color = blueColor;
            dp1.Offset = rectOffset;
            dp1.DestoryAt = 6700;
            dp1.ScaleMode = ScaleMode.YByTime;
            dp1.Scale = new Vector2(width, length);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = $"Headlight_Low_{@event.SourceId}";
            dp2.Position = new Vector3(srcPos.X, 5.0f, srcPos.Z);
            dp2.Rotation = srcRot;
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.Offset = rectOffset;
            dp2.Delay = 6700;
            dp2.DestoryAt = 2700;
            dp2.ScaleMode = ScaleMode.YByTime;
            dp2.Scale = new Vector2(width, length);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);
        }
    }
}
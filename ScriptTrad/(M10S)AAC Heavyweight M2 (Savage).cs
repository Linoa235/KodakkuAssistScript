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
using System.Collections.Concurrent;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;

namespace RyougiMioScriptNamespace
{
    [ScriptType(name: "(M10S)AAC Heavyweight M2 (Savage)", territorys: [1322, 1323], guid: "adeb3ad9-6847-4965-8b0e-5ea9e3117635", version: "0.1.0.1", author: "Linoa235", note: "M10S, script works in both M10N/S.")]
    public class RyougiMio_1323
    {
        #region Settings
        [UserSetting("Enable Screen Text Alerts")]
        public bool EnableText { get; set; } = true;
        [UserSetting("Enable TTS Voice Alerts")]
        public bool EnableTTS { get; set; } = true;

        [UserSetting("Common Danger Color")]
        public ScriptColor DangerColor { get; set; } = new ScriptColor() { V4 = new Vector4(1.0f, 0.0f, 0.0f, 0.01f) };
        [UserSetting("Common Safe Color")]
        public ScriptColor SafeColor { get; set; } = new ScriptColor() { V4 = new Vector4(0.0f, 1.0f, 0.0f, 0.01f) };
        [UserSetting("Water Blue Color")]
        public ScriptColor _waterColor { get; set; } = new ScriptColor() { V4 = new Vector4(0f, 1f, 1f, 0.5f) };

        [UserSetting("Guide Color (default cyan)")]
        public ScriptColor GuideColor { get; set; } = new ScriptColor() { V4 = new Vector4(0.0f, 1.0f, 1.0f, 0.01f) };
        #endregion

        #region Variables
        private List<(uint Id, DateTime Time)> _fireWaveMarkList = new();
        private ScriptAccessory _acc;
        private static int _waterMechanicParam = 0;
        #endregion

        #region Methods
        private void DrawCircle(ScriptAccessory accessory, string name, Vector3 pos, Vector4 color, int duration)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Scale = new Vector2(6f);
            dp.Color = color;
            dp.DestoryAt = duration;
            dp.Position = pos;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 1;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        private void DrawFan(ScriptAccessory accessory, string name, Vector3 pos, Vector4 color, int duration, uint orderIndex)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Scale = new Vector2(60f);
            dp.Radian = float.Pi / 4;
            dp.Color = color;
            dp.DestoryAt = duration;
            dp.Position = pos;
            dp.TargetPosition = pos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = orderIndex;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        private void QTTS(string text, int rate = 0)
        {
            if (!EnableTTS) return;
            _acc.Method.TTS(text, rate);
        }

        private void QText(string text, int duration, bool isWarning = false)
        {
            if (!EnableText) return;
            _acc.Method.TextInfo(text, duration, isWarning);
        }
        #endregion

        #region Initialization
        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(".*");
            _acc = accessory;
            _waterMechanicParam = 0;

            lock (_fireWaveMarkList)
            {
                _fireWaveMarkList.Clear();
            }

            accessory.Method.SendChat("/e M10S Initialized.");
        }
        #endregion

        #region TTS Only
        [ScriptMethod(name: "Spirited TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46466|46520|46467|46521)$"])]
        public void Elemental_AOE_Alert(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (aid == 46466 || aid == 46520)
            {
                QTTS("Fire AOE");
                QText("Fire AOE", 5000, true);
            }
            else if (aid == 46467 || aid == 46521)
            {
                QTTS("Water AOE");
                QText("Water AOE", 5000, true);
            }
        }

        [ScriptMethod(name: "Splash/Surge TTS (guess)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46543|46544|46551|46552)$"])]
        public void SpreadStack_Alert(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (aid == 46543 || aid == 46551)
            {
                QTTS("Spread");
                QText("Spread", 5000, true);
            }
            else if (aid == 46544 || aid == 46552)
            {
                QTTS("Stack");
                QText("Stack", 5000, true);
            }
        }

        [ScriptMethod(name: "Double Swirl/Cross Swirl TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46557|46560)$"])]
        public void SpreadStack_Alert1(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (aid == 46557 || aid == 46551)
            {
                QTTS("Dodge after water wave");
                QText("Dodge after water wave", 5000, true);
            }
            else if (aid == 46560 || aid == 46552)
            {
                QTTS("Don't move after water wave");
                QText("Don't move after water wave", 5000, true);
            }
        }

        [ScriptMethod(name: "Wave Spin TTS (guess)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(47249|47250)$"])]
        public void DelayedMechanic_Alert(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (aid == 47249)
            {
                QTTS("Cone soon");
                QText("Cone soon", 5000, true);
            }
            else if (aid == 47250)
            {
                QTTS("Knockback soon");
                QText("Knockback soon", 5000, true);
            }
        }

        [ScriptMethod(name: "Flame Impact/Deep Impact TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46464|46518|46465|46519)$"])]
        public void TankBuster_Alert(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (aid == 46464 || aid == 46518)
            {
                QTTS("Stack tankbuster");
                QText("Stack tankbuster", 5000, true);
            }
            else if (aid == 46465 || aid == 46519)
            {
                QTTS("Tankbuster");
                QText("Tankbuster", 5000, true);
            }
        }

        [ScriptMethod(name: "Limit Technique TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46499|46500)$"])]
        public void LimitCut_AOE_Alert(Event @event, ScriptAccessory accessory)
        {
            QTTS("Continuous AOE");
            QText("Continuous AOE", 5000, true);
        }

        [ScriptMethod(name: "Water Surf/Fire Surf TTS (guess)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46532|46563)$"])]
        public void JumpAndTower_Alert(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (aid == 46532)
            {
                QTTS("Crimson 4 jumps");
                QText("Crimson 4 jumps", 3000, true);
            }
            else if (aid == 46563)
            {
                QTTS("Two-person tower");
                QText("Two-person tower", 3000, true);
            }
        }

        [ScriptMethod(name: "Deep Blue Spread/Stack", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2056"])]
        public void Status_2056_Add(Event @event, ScriptAccessory accessory)
        {
            if (!int.TryParse(@event["Param"], out var stack)) return;

            if (stack == 1007 || stack == 1005 || stack == 1008 || stack == 1006)
            {
                _waterMechanicParam = stack;

                if (stack == 1007)
                {
                    QTTS("Full party stack");
                    QText("Full party stack", 5000, true);
                }
                else if (stack == 1005)
                {
                    QTTS("Two-group stack");
                    QText("Two-group stack", 5000, true);
                }
                else if (stack == 1008)
                {
                    QTTS("Water group spread");
                    QText("Water group spread", 5000, true);
                }
                else if (stack == 1006)
                {
                    QTTS("Full party spread");
                    QText("Full party spread", 5000, true);
                }
            }
        }
        #endregion

        #region Fire Wave Slash
        [ScriptMethod(name: "M10S: Fire Wave Slash Marker Record", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(0298)$"], userControl: false)]
        public void FireWave_MarkRecord(Event @event, ScriptAccessory accessory)
        {
            var idStr = @event["TargetId"];
            if (string.IsNullOrEmpty(idStr)) return;
            if (!uint.TryParse(idStr.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var tid)) return;
            lock (_fireWaveMarkList)
            {
                _fireWaveMarkList.Add((tid, DateTime.Now));
            }
        }

        [ScriptMethod(name: "Fire Wave Slash", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46478|46537)$"])]
        public void FireWave_Draw(Event @event, ScriptAccessory accessory)
        {
            var srcIdStr = @event["SourceId"];
            if (string.IsNullOrEmpty(srcIdStr)) return;
            if (!uint.TryParse(srcIdStr.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var sid)) return;
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;

            var boss = accessory.Data.Objects.SearchById(sid);
            if (boss == null) return;
            var spos = boss.Position;
            var now = DateTime.Now;
            float fanAngle = (aid == 46478) ? float.Pi / 3 : float.Pi * 2 * 330 / 360;

            lock (_fireWaveMarkList)
            {
                _fireWaveMarkList.RemoveAll(x => (now - x.Time).TotalSeconds > 10);
                foreach (var mark in _fireWaveMarkList)
                {
                    var tObj = accessory.Data.Objects.SearchById(mark.Id);
                    if (tObj == null) continue;
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Fire Wave Slash-{mark.Id}-{aid}";
                    dp.Scale = new Vector2(60f);
                    dp.Radian = fanAngle;
                    dp.Owner = sid;
                    dp.TargetObject = mark.Id;
                    dp.Rotation = 0;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = dur;
                    dp.ScaleMode = ScaleMode.ByTime;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                }
            }
        }
        #endregion

        #region Guesswork
        [ScriptMethod(name: "Wave Spin (guess)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46547|46488|46550)$"])]
        public void FanAndKnockback_Draw(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Draw_{aid}";
            dp.Position = @event.SourcePosition;
            dp.Rotation = @event.SourceRotation;
            dp.DestoryAt = dur;
            dp.Color = accessory.Data.DefaultDangerColor;
            if (aid == 46547 || aid == 46488)
            {
                dp.Radian = float.Pi * 2 / 3;
                dp.Scale = new Vector2(60f);
                dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
            else if (aid == 46550)
            {
                dp.Scale = new Vector2(5f, 30f);
                dp.ScaleMode = ScaleMode.YByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Arrow, dp);
            }
        }

        [ScriptMethod(name: "Flame Impact", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46518|46464)$"])]
        public void Action_46518_ColorCheck(Event @event, ScriptAccessory accessory)
        {
            if (!int.TryParse(@event["DurationMilliseconds"], out var duration)) return;

            var tidStr = @event["TargetId"];
            if (string.IsNullOrEmpty(tidStr) ||
                !uint.TryParse(tidStr, System.Globalization.NumberStyles.HexNumber, null, out var tid))
                return;

            uint myId = accessory.Data.Me;
            var partyIds = accessory.Data.PartyList;
            int myIndex = -1;

            for (int i = 0; i < partyIds.Count; i++)
            {
                if (partyIds[i] == myId)
                {
                    myIndex = i;
                    break;
                }
            }

            bool amITank = (myIndex == 0 || myIndex == 1);
            var drawColor = amITank ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Action_46518_{tid}_{DateTime.Now.Ticks}";
            dp.Owner = tid;
            dp.Scale = new Vector2(6f);
            dp.Color = drawColor;
            dp.DestoryAt = duration;
            dp.ScaleMode = ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Cross Swirl/Double Swirl_Revised", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46560|46557)$"])]
        public void Boss_Tracking_Fan(Event @event, ScriptAccessory accessory)
        {
            if (!int.TryParse(@event["DurationMilliseconds"], out var duration)) return;
            float rad30 = float.Pi / 6;

            var partyIds = accessory.Data.PartyList;
            foreach (var tid in partyIds)
            {
                var obj = accessory.Data.Objects.FirstOrDefault(x => x.EntityId == tid);

                if (obj is IBattleChara player)
                {
                    bool hasBuff = player.StatusList.Any(s => s.StatusId == 4974);
                    if (hasBuff) continue;

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"TrackFan_{@event["ActionId"]}_{tid}";
                    dp.Owner = @event.SourceId;
                    dp.TargetObject = tid;
                    dp.Radian = rad30;
                    dp.Scale = new Vector2(60f);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = duration;
                    dp.ScaleMode = ScaleMode.ByTime;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                }
            }
        }

        [ScriptMethod(name: "Cross Swirl/Double Swirl 2", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(46561|46558)$"])]
        public void Action_FollowUp_Fans(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;

            int duration = 2600;
            float rad30 = float.Pi / 6;
            float rad24 = 22.5f * (float.Pi / 180f);
            float rad15 = 15f * (float.Pi / 180f);

            var pos = @event.SourcePosition;
            var rot = @event.SourceRotation;

            if (aid == 46558)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Fan_46558_{DateTime.Now.Ticks}";
                dp.Position = pos;
                dp.Rotation = rot;
                dp.Radian = rad30;
                dp.Scale = new Vector2(60f);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = duration;
                dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
            else if (aid == 46561)
            {
                var dpLeft = accessory.Data.GetDefaultDrawProperties();
                dpLeft.Name = $"Fan_46561_Left_{DateTime.Now.Ticks}";
                dpLeft.Position = pos;
                dpLeft.Rotation = rot + rad24;
                dpLeft.Radian = rad15;
                dpLeft.Scale = new Vector2(60f);
                dpLeft.Color = accessory.Data.DefaultDangerColor;
                dpLeft.DestoryAt = duration;
                dpLeft.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dpLeft);

                var dpRight = accessory.Data.GetDefaultDrawProperties();
                dpRight.Name = $"Fan_46561_Right_{DateTime.Now.Ticks}";
                dpRight.Position = pos;
                dpRight.Rotation = rot - rad24;
                dpRight.Radian = rad15;
                dpRight.Scale = new Vector2(60f);
                dpRight.Color = accessory.Data.DefaultDangerColor;
                dpRight.DestoryAt = duration;
                dpRight.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dpRight);
            }
        }

        [ScriptMethod(name: "EnvControl15-21", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:regex:^\\d+$"])]
        public void EnvControl_Grid_Mechanic(Event @event, ScriptAccessory accessory)
        {
            if (!int.TryParse(@event["Index"], out var index)) return;
            if (index < 14 || index > 22) return;
            if (!int.TryParse(@event["Flag"], out var flag)) return;

            Vector3 gridPos = index switch
            {
                14 => new Vector3(87f, 0f, 87f),
                15 => new Vector3(100f, 0f, 87f),
                16 => new Vector3(113f, 0f, 87f),
                17 => new Vector3(87f, 0f, 100f),
                18 => new Vector3(100f, 0f, 100f),
                19 => new Vector3(113f, 0f, 100f),
                20 => new Vector3(87f, 0f, 113f),
                21 => new Vector3(100f, 0f, 113f),
                22 => new Vector3(113f, 0f, 113f),
                _ => Vector3.Zero
            };

            string baseName = $"Grid_{index}";

            if (flag == 4 || flag == 8)
            {
                accessory.Method.RemoveDraw($"{baseName}_TB");
                accessory.Method.RemoveDraw($"{baseName}_Stack");
                for (int i = 1; i <= 4; i++) accessory.Method.RemoveDraw($"{baseName}_Spread_{i}");
                return;
            }

            bool isWater = (flag == 2 || flag == 32 || flag == 128);
            Vector4 drawColor = isWater ? _waterColor.V4 : accessory.Data.DefaultDangerColor;

            if (flag == 128 || flag == 8192)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"{baseName}_TB";
                dp.Scale = new Vector2(6f);
                dp.Color = drawColor;
                dp.DestoryAt = 9999999;
                dp.ScaleMode = ScaleMode.None;
                dp.Position = gridPos;
                dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                dp.CentreOrderIndex = 1;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else if (flag == 32 || flag == 2048)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"{baseName}_Stack";
                dp.Scale = new Vector2(60f);
                dp.Radian = float.Pi / 4;
                dp.Color = drawColor;
                dp.DestoryAt = 9999999;
                dp.ScaleMode = ScaleMode.None;
                dp.Position = gridPos;
                dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                dp.TargetOrderIndex = 1;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
            else if (flag == 2 || flag == 512)
            {
                for (uint i = 1; i <= 4; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"{baseName}_Spread_{i}";
                    dp.Scale = new Vector2(60f);
                    dp.Radian = float.Pi / 4;
                    dp.Color = drawColor;
                    dp.DestoryAt = 9999999;
                    dp.ScaleMode = ScaleMode.None;
                    dp.Position = gridPos;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = i;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                }
            }
        }

        [ScriptMethod(name: "Env_2-5", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:regex:^[2-5]$"])]
        public void Env_Buff_Draw(Event @event, ScriptAccessory accessory)
        {
            if (!int.TryParse(@event["Index"], out var index)) return;
            if (index < 2 || index > 5) return;
            if (!int.TryParse(@event["Flag"], out var flag)) return;

            uint buffId = 4975;
            int duration = 10000;

            if (flag == 128)
            {
                var myId = accessory.Data.Me;
                var myObj = accessory.Data.Objects.FirstOrDefault(x => x.EntityId == myId);

                if (myObj is IBattleChara me)
                {
                    if (me.StatusList.Any(s => s.StatusId == buffId))
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"Safe_{index}_{myId}";
                        dp.Owner = myId;
                        dp.Scale = new Vector2(6f);
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = duration;
                        dp.ScaleMode = ScaleMode.ByTime;
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    }
                }
            }
            else if (flag == 2048)
            {
                var partyIds = accessory.Data.PartyList;

                foreach (var tid in partyIds)
                {
                    var obj = accessory.Data.Objects.FirstOrDefault(x => x.EntityId == tid);

                    if (obj is IBattleChara player)
                    {
                        if (player.StatusList.Any(s => s.StatusId == buffId))
                        {
                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"Danger_{index}_{tid}";
                            dp.Owner = tid;
                            dp.Scale = new Vector2(5f);
                            dp.Color = accessory.Data.DefaultDangerColor;
                            dp.DestoryAt = duration;
                            dp.ScaleMode = ScaleMode.ByTime;
                            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                        }
                    }
                }
            }
        }

        [ScriptMethod(name: "Mixed Explosion", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46587"])]
        public void Action_46587_Circle(Event @event, ScriptAccessory accessory)
        {
            if (!int.TryParse(@event["DurationMilliseconds"], out var duration)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Circle_46587_{@event.SourceId}_{DateTime.Now.Ticks}";
            dp.Position = @event.SourcePosition;
            dp.Scale = new Vector2(9f);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.ScaleMode = ScaleMode.ByTime;
            dp.DestoryAt = duration;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Tether Rectangle_Rect Version", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(027B|027C)$"])]
        public void Draw_Link_Rect(Event @event, ScriptAccessory accessory)
        {
            string iconId = @event["Id"];
            uint targetPlayerId = (uint)@event.TargetId;

            uint searchDataId = 0;
            if (iconId == "027B") searchDataId = 19288;
            else if (iconId == "027C") searchDataId = 19287;

            if (searchDataId == 0) return;

            var sourceNpc = accessory.Data.Objects.FirstOrDefault(x => x.DataId == searchDataId);
            if (sourceNpc == null) return;

            Vector4 drawColor = (iconId == "027B") ? _waterColor.V4 : accessory.Data.DefaultDangerColor;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Link_Rect_{iconId}_{targetPlayerId}_{DateTime.Now.Ticks}";
            dp.Owner = sourceNpc.EntityId;
            dp.TargetObject = targetPlayerId;
            dp.Scale = new Vector2(8f, 1f);
            dp.Color = drawColor;
            dp.DestoryAt = 4700;
            dp.ScaleMode = ScaleMode.YByDistance;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        #endregion
    }
}
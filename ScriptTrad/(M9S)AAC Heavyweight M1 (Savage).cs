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
    [ScriptType(name: "(M9S)AAC Heavyweight M1 (Savage)", territorys: [1320, 1321], guid: "9fe75d93-db1c-4d64-aede-056815904533", version: "1.0.0.0", author: "Linoa235", note: "M9S, script works in both M9N/S. TTS-marked mechanics only have announcements.")]
    public class RyougiMio_1321
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

        [UserSetting("Guide Color (default cyan)")]
        public ScriptColor GuideColor { get; set; } = new ScriptColor() { V4 = new Vector4(0.0f, 1.0f, 1.0f, 0.01f) };
        #endregion

        #region Variables
        private ScriptAccessory _acc;
        private static long _chainsawTime = 0;
        private static int _chainsawDelay = 0;
        private int _moonCount = 0;
        private static int _fan45969Count = 0;
        private static long _fan45969LastTime = 0;
        private static Dictionary<int, long> _fanGroupEndTimes = new Dictionary<int, long>();
        private static Dictionary<uint, int> _moonPhaseDurations = new Dictionary<uint, int>();
        private HashSet<uint> _active4729Tids = new HashSet<uint>();
        #endregion

        #region Methods
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
            _moonPhaseDurations.Clear();
            _acc = accessory;
            _chainsawTime = 0;
            _chainsawDelay = 0;
            _moonCount = 0;
            _fan45969Count = 0;
            _fan45969LastTime = 0;
            _fanGroupEndTimes.Clear();
            accessory.Method.SendChat("/e M9S Initialized.");
        }
        #endregion

        #region TTS Only
        [ScriptMethod(name: "Fatal Voice TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45921|45956)$"])]
        public void AOE_Alert(Event @event, ScriptAccessory accessory)
        {
            QTTS("AOE");
            QText("AOE", 4700, true);
        }

        [ScriptMethod(name: "Sonic Scatter TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45980)$"])]
        public void AOE_Alert1(Event @event, ScriptAccessory accessory)
        {
            QTTS("Spread");
        }

        [ScriptMethod(name: "Sonic Gather TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45981)$"])]
        public void AOE_Aler2t(Event @event, ScriptAccessory accessory)
        {
            QTTS("Stack");
        }

        [ScriptMethod(name: "Hardcore Voice TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45915|45951|45916|45952)$"])]
        public void HardcoreVoice(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;

            if (aid == 45916 || aid == 45952)
            {
                QTTS("Enhanced double tankbuster");
                QText("Enhanced double tankbuster", 5000, true);
            }
            else
            {
                QTTS("Double tankbuster");
                QText("Double tankbuster", 5000, true);
            }
        }

        [ScriptMethod(name: "Erosive Low Voice/Sharp Pitch TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45957|45958)$"])]
        public void ErosionVoicePitch(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (aid == 45957)
            {
                QTTS("Spread");
                QText("Full party spread", 5000, true);
            }
            else if (aid == 45958)
            {
                QTTS("Stack");
                QText("Group stack", 5000, true);
            }
        }
        #endregion

        #region Left/Right Cleave
        [ScriptMethod(name: "Moon Half-Phase Left/Right Half-Room Cleave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45906|45907|45910|45911|45943|45944|45947|45948)$"])]
        public void HalfMoon(Event @event, ScriptAccessory accessory)
        {
            var totalDuration = int.Parse(@event["DurationMilliseconds"]);
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;

            var leftSideIds = new HashSet<uint> { 45906, 45911, 45943, 45948 };
            var secondHitIds = new HashSet<uint> { 45907, 45911, 45944, 45948 };
            var pairMap = new Dictionary<uint, uint>
            {
                { 45907, 45906 }, { 45911, 45910 },
                { 45944, 45943 }, { 45948, 45947 }
            };

            bool isLeft = leftSideIds.Contains(aid);
            bool isSecond = secondHitIds.Contains(aid);

            _moonCount++;

            Vector3 drawPos;

            if (_moonCount == 3 || _moonCount == 4)
            {
                drawPos = @event.SourcePosition;
            }
            else
            {
                drawPos = new Vector3(100f, 0f, 100f);
            }

            var srcRot = @event.SourceRotation;
            float width = 40f;
            float length = 40f;

            float rotOffset = isLeft ? (MathF.PI / 2) : -(MathF.PI / 2);
            float finalRot = srcRot + rotOffset;

            int delay = 0;
            int destory = totalDuration;

            if (!isSecond)
            {
                _moonPhaseDurations[aid] = totalDuration;
            }
            else
            {
                if (pairMap.TryGetValue(aid, out var firstId))
                {
                    var firstDuration = _moonPhaseDurations.ContainsKey(firstId) ? _moonPhaseDurations[firstId] : 5000;
                    delay = firstDuration;
                    destory = totalDuration - delay;
                }
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Moon Half-Phase_{aid}_{DateTime.Now.Ticks}";
            dp.Position = drawPos;
            dp.Rotation = finalRot;
            dp.Scale = new Vector2(width, length);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = delay;
            dp.DestoryAt = destory;
            dp.ScaleMode = ScaleMode.YByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Enhanced Moon Half-Phase Left/Right Cleave", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45908|45909|45912|45913|45945|45946|45949|45950)$"])]
        public void EnhancedHalfMoon(Event @event, ScriptAccessory accessory)
        {
            var totalDuration = int.Parse(@event["DurationMilliseconds"]);
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;

            var leftSideIds = new HashSet<uint> { 45908, 45913, 45945, 45950 };
            var secondHitIds = new HashSet<uint> { 45909, 45913, 45946, 45950 };
            var pairMap = new Dictionary<uint, uint>
            {
                { 45909, 45908 },
                { 45913, 45912 },
                { 45946, 45945 },
                { 45950, 45949 }
            };

            float px = MathF.Round(@event.EffectPosition.X);
            float pz = MathF.Round(@event.EffectPosition.Z);
            var drawPos = new Vector3(px, 0f, pz);
            var srcRot = @event.SourceRotation;
            bool isLeft = leftSideIds.Contains(aid);
            bool isSecond = secondHitIds.Contains(aid);
            float width = 40f;
            float length = 40f;

            float rotOffset = isLeft ? (MathF.PI / 2) : -(MathF.PI / 2);
            float finalRot = srcRot + rotOffset;

            int delay = 0;
            int destory = totalDuration;

            if (!isSecond)
            {
                _moonPhaseDurations[aid] = totalDuration;
            }
            else
            {
                if (pairMap.TryGetValue(aid, out var firstId))
                {
                    var firstDuration = _moonPhaseDurations.ContainsKey(firstId) ? _moonPhaseDurations[firstId] : 5000;
                    delay = firstDuration;
                    destory = totalDuration - delay;
                }
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Enhanced Moon Half-Phase_{aid}";
            dp.Position = drawPos;
            dp.Rotation = finalRot;
            dp.Scale = new Vector2(width, length);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = delay;
            dp.DestoryAt = destory;
            dp.ScaleMode = ScaleMode.YByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        #endregion

        #region Bat Mechanics
        [ScriptMethod(name: "Bat", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45940"])]
        public void Bat_Auto_Rotate_Direction(Event @event, ScriptAccessory accessory)
        {
            var bats = accessory.Data.Objects.Where(obj => obj.DataId == 19503);
            var center = new Vector3(100f, 0, 100f);

            foreach (var bat in bats)
            {
                var batPos = bat.Position;
                var batRot = bat.Rotation;

                Vector3 vecRadius = batPos - center;
                double dist = vecRadius.Length();

                Vector3 vecBatFace = new Vector3((float)Math.Sin(batRot), 0, (float)Math.Cos(batRot));

                Vector3 vecCW = new Vector3(vecRadius.Z, 0, -vecRadius.X);
                Vector3 vecCCW = new Vector3(-vecRadius.Z, 0, vecRadius.X);

                Vector3 finalVec;
                if (Vector3.Dot(vecCW, vecBatFace) > Vector3.Dot(vecCCW, vecBatFace))
                {
                    finalVec = vecCW;
                }
                else
                {
                    finalVec = vecCCW;
                }

                Vector3 drawPos = center + finalVec;

                int delay = 0;
                int duration = 0;

                if (dist > 5 && dist < 10)
                {
                    delay = 0; duration = 8200;
                }
                else if (dist >= 10 && dist < 15)
                {
                    delay = 8200; duration = 3500;
                }
                else if (dist >= 15 && dist < 30)
                {
                    delay = 11700; duration = 3500;
                }
                else continue;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Bat_SmartRot_{bat.EntityId}_{DateTime.Now.Ticks}";
                dp.Owner = 0;
                dp.Position = drawPos;
                dp.Scale = new Vector2(8f);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = delay;
                dp.DestoryAt = duration;
                dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }

        [ScriptMethod(name: "Bat Buff", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4729"])]
        public void Status_4729_Add(Event @event, ScriptAccessory accessory)
        {
            var tidStr = @event["TargetId"];
            if (!ulong.TryParse(tidStr, System.Globalization.NumberStyles.HexNumber, null, out var tid))
            {
                if (!ulong.TryParse(tidStr, out tid)) return;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Buff_4729_Ring_{tid}";
            dp.Owner = tid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 600000;
            dp.Scale = new Vector2(8.0f);
            dp.InnerScale = new Vector2(7.95f);
            dp.Radian = float.Pi * 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }

        [ScriptMethod(name: "Bat Buff Remove", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4729"])]
        public void Status_4729_Remove(Event @event, ScriptAccessory accessory)
        {
            var tidStr = @event["TargetId"];
            if (!ulong.TryParse(tidStr, System.Globalization.NumberStyles.HexNumber, null, out var tid))
            {
                if (!ulong.TryParse(tidStr, out tid)) return;
            }

            string drawName = $"Buff_4729_Ring_{tid}";
            accessory.Method.RemoveDraw(drawName);
        }
        #endregion

        #region Hammer Saw Blade
        [ScriptMethod(name: "Deadly Saw_Forward", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45877|45927)$"])]
        public void Chainsaw_Forward(Event @event, ScriptAccessory accessory)
        {
            var duration = int.Parse(@event["DurationMilliseconds"]);
            var rawPos = @event.EffectPosition;
            var drawPos = new Vector3(MathF.Round(rawPos.X), 0, MathF.Round(rawPos.Z));
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Deadly Saw_Forward";
            dp.Position = drawPos;
            dp.Rotation = @event.SourceRotation;
            dp.Scale = new Vector2(20f, 10f);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = duration;
            dp.ScaleMode = ScaleMode.YByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Deadly Saw_Rush Out", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45878|45879|45880|45928|45929|45930)$"])]
        public void Chainsaw_RushOut(Event @event, ScriptAccessory accessory)
        {
            var duration = int.Parse(@event["DurationMilliseconds"]);
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;

            float length = 32f;
            if (aid == 45879 || aid == 45929) length = 22f;
            if (aid == 45880 || aid == 45930) length = 12f;

            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long diff = now - _chainsawTime;
            int finalDelay = 0;
            int finalDuration = duration;

            if (_chainsawTime == 0 || Math.Abs(diff) > 5000)
            {
                finalDelay = 0;
                _chainsawDelay = 0;
                _chainsawTime = now;
            }
            else if (diff < 1000)
            {
                finalDelay = _chainsawDelay;
                finalDuration = duration - _chainsawDelay;
            }
            else
            {
                finalDelay = duration - (int)diff;
                finalDuration = (int)diff;
                _chainsawDelay = finalDelay;
                _chainsawTime = now;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Deadly Saw_{aid}";
            dp.Position = @event.SourcePosition;
            dp.Rotation = @event.SourceRotation;
            dp.Scale = new Vector2(5f, length);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = finalDelay;
            dp.DestoryAt = finalDuration;
            dp.ScaleMode = ScaleMode.YByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Deadly Circular Saw", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(19189|19190)$"])]
        public void Straight_Entity_Spawn(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["DataId"], out var id)) return;
            int duration = 120000;
            float width = 0f;
            float length = 0f;
            if (id == 19189)
            {
                width = 6f;
                length = 8f;
            }
            else if (id == 19190)
            {
                width = 12f;
                length = 5f;
            }
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Deadly Circular Saw_{@event.SourceId}";
            dp.Owner = @event.SourceId;
            dp.Rotation = @event.SourceRotation;
            dp.Scale = new Vector2(width, length);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = duration;
            dp.ScaleMode = ScaleMode.None;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
        #endregion

        #region Ether Loss
        [ScriptMethod(name: "Ether Loss_Cross", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45897|45971)$"])]
        public void Fatal_Cross_AOE(Event @event, ScriptAccessory accessory)
        {
            var duration = int.Parse(@event["DurationMilliseconds"]);
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            float radius = 40f;
            float width = (aid == 45971) ? 10f : 6f;

            for (int i = 0; i < 4; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Ether Loss_{aid}_{i}";
                dp.Position = @event.SourcePosition;
                dp.Rotation = @event.SourceRotation + (float)(Math.PI / 2 * i);
                dp.Scale = new Vector2(width, radius);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = duration;
                dp.ScaleMode = ScaleMode.YByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }
        #endregion

        #region Guesswork
        [ScriptMethod(name: "Mark Bat", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(19502)$"])]
        public void Mark_19502_Circle(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Mark_19502_{@event.SourceId}";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(0.5f);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 60000;
            dp.ScaleMode = ScaleMode.None;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        private long _lastTriggerTime_45989_Single = 0;

        [ScriptMethod(name: "M11S: Rotating Cone (Unidirectional 6-hit/Follow)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45989"])]
        public void RotateFan_Single_Sequence(Event @event, ScriptAccessory accessory)
        {
            Vector3 centerPos = @event.SourcePosition;
            float startAngle = @event.SourceRotation;
            float rotationStep = (float)(Math.PI / 8);
            float fanRadian = (float)(Math.PI / 6);
            float fanRadius = 60f;

            int currentDelay = 0;

            for (int i = 0; i < 6; i++)
            {
                int duration = (i == 0) ? 2700 : 2400;
                float finalRot = startAngle + (i * rotationStep);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"RotFan_Single_{@event.SourceId}_{i}";
                dp.Position = centerPos;
                dp.Rotation = finalRot;
                dp.Radian = fanRadian;
                dp.Scale = new Vector2(fanRadius);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = currentDelay;
                dp.DestoryAt = duration;
                dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                currentDelay += duration;
            }
        }

        [ScriptMethod(name: "Hardcore Voice", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45951"])]
        public void Circle_Target_6m(Event @event, ScriptAccessory accessory)
        {
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;

            var tidStr = @event["TargetId"];
            if (string.IsNullOrEmpty(tidStr) ||
                !ulong.TryParse(tidStr.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var tid))
                return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Circle_6m_{tid}_{DateTime.Now.Ticks}";
            dp.Owner = tid;
            dp.Scale = new Vector2(6f);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = dur;
            dp.ScaleMode = ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Enhanced Hardcore Voice", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45952"])]
        public void Circle_15m(Event @event, ScriptAccessory accessory)
        {
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Circle_15m_{@event.SourceId}_{DateTime.Now.Ticks}";
            dp.Position = @event.SourcePosition;
            dp.Scale = new Vector2(15f);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = dur;
            dp.ScaleMode = ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Ether Loss_Cone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:45969"])]
        public void Fan_45_AOE(Event @event, ScriptAccessory accessory)
        {
            var duration = int.Parse(@event["DurationMilliseconds"]);
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (now - _fan45969LastTime > 15000)
            {
                _fan45969Count = 0;
                _fanGroupEndTimes.Clear();
            }
            _fan45969LastTime = now;

            _fan45969Count++;

            int groupIndex = (_fan45969Count - 1) / 2;

            long myEndTime = now + duration;
            if (!_fanGroupEndTimes.ContainsKey(groupIndex) || myEndTime > _fanGroupEndTimes[groupIndex])
            {
                _fanGroupEndTimes[groupIndex] = myEndTime;
            }

            int delay = 0;
            if (groupIndex > 0)
            {
                if (_fanGroupEndTimes.TryGetValue(groupIndex - 1, out long prevEndTime))
                {
                    delay = (int)Math.Max(0, prevEndTime - now);
                }
            }

            int finalDuration = duration - delay;
            if (finalDuration <= 0) finalDuration = 100;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Fan45_{@event.SourceId}_{_fan45969Count}_{DateTime.Now.Ticks}";
            dp.Position = @event.SourcePosition;
            dp.Rotation = @event.SourceRotation;
            dp.Radian = (float)Math.PI / 4;
            dp.Scale = new Vector2(60f);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = delay;
            dp.DestoryAt = finalDuration;
            dp.ScaleMode = ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Sonic", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(45980|45981)$"])]
        public void Dual_Spell_Mechanic(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            var duration = int.Parse(@event["DurationMilliseconds"]);

            uint myId = accessory.Data.Me;
            var partyIds = accessory.Data.PartyList;

            bool iHave4730 = false;
            var myObj = accessory.Data.Objects.FirstOrDefault(x => x.EntityId == myId);
            if (myObj is IBattleChara me)
            {
                iHave4730 = me.StatusList.Any(s => s.StatusId == 4730);
            }

            int myIndex = -1;
            for (int i = 0; i < partyIds.Count; i++)
            {
                if (partyIds[i] == myId)
                {
                    myIndex = i;
                    break;
                }
            }

            float rad100 = 100f * (float.Pi / 180f);
            float rad45 = float.Pi / 4;

            for (int i = 0; i < partyIds.Count; i++)
            {
                var tid = partyIds[i];
                var obj = accessory.Data.Objects.FirstOrDefault(x => x.EntityId == tid);

                if (obj is IBattleChara player)
                {
                    bool targetHas4730 = player.StatusList.Any(s => s.StatusId == 4730);
                    if (targetHas4730) continue;

                    bool isMe = (i == myIndex);

                    if (aid == 45981)
                    {
                        if (!isMe && !iHave4730) continue;
                    }

                    float angle = 0f;
                    if (aid == 45980)
                    {
                        bool isTank = (i == 0 || i == 1);
                        angle = isTank ? rad100 : rad45;
                    }
                    else
                    {
                        angle = rad100;
                    }

                    var drawColor = accessory.Data.DefaultDangerColor;

                    if (aid == 45980)
                    {
                        if (myIndex >= 4)
                        {
                            if (i >= 4 && !isMe)
                            {
                                drawColor = accessory.Data.DefaultSafeColor;
                            }
                        }
                    }
                    else
                    {
                        if (!isMe)
                        {
                            drawColor = accessory.Data.DefaultSafeColor;
                        }
                    }

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"DualSpell_{aid}_{player.EntityId}";
                    dp.Owner = @event.SourceId;
                    dp.TargetObject = player.EntityId;
                    dp.Radian = angle;
                    dp.Scale = new Vector2(60f);
                    dp.Color = drawColor;
                    dp.DestoryAt = duration;
                    dp.ScaleMode = ScaleMode.ByTime;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                }
            }
        }

        [ScriptMethod(name: "Status2056 Drawing", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2056"])]
        public void Status_2056_Add(Event @event, ScriptAccessory accessory)
        {
            if (!int.TryParse(@event["StackCount"], out var stack)) return;

            var tidStr = @event["TargetId"];
            if (!uint.TryParse(tidStr, System.Globalization.NumberStyles.HexNumber, null, out var tid)) return;

            string drawName = $"Status_2056_{tid}";

            if (stack == 38)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = drawName;
                dp.Owner = tid;
                dp.Scale = new Vector2(7f);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 99999;
                dp.ScaleMode = ScaleMode.None;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else if (stack == 39)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = drawName;
                dp.Owner = tid;
                dp.Scale = new Vector2(15f);
                dp.InnerScale = new Vector2(4f);
                dp.Radian = float.Pi * 2;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 99999;
                dp.ScaleMode = ScaleMode.None;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
            }
        }

        [ScriptMethod(name: "Status2056 Remove", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:2056"])]
        public void Status_2056_Remove(Event @event, ScriptAccessory accessory)
        {
            var tidStr = @event["TargetId"];
            if (!uint.TryParse(tidStr, System.Globalization.NumberStyles.HexNumber, null, out var tid)) return;

            string drawName = $"Status_2056_{tid}";
            accessory.Method.RemoveDraw(drawName);
        }
        #endregion
    }
}
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
    [ScriptType(name: "(M11S)AAC Heavyweight M3 (Savage)", territorys: [1324, 1325], guid: "ac2f7b8e-f8fb-46e4-bc52-5f70fca0a061", version: "0.1.5.0", Author: "Linoa235", note: "M11S, script works in both M11N/S.")]
    public class RyougiMio_1325
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
        [UserSetting("Meteor Color")]
        public ScriptColor SafeColor1 { get; set; } = new ScriptColor() { V4 = new Vector4(0.0f, 0.0f, 0.6f, 1.0f) };

        [UserSetting("Guide Color (default cyan)")]
        public ScriptColor GuideColor { get; set; } = new ScriptColor() { V4 = new Vector4(0.0f, 1.0f, 1.0f, 0.01f) };
        #endregion

        #region Variables
        public class ObjectState
        {
            public uint DataId;
            public Vector3 Position;
            public float Rotation;
            public int GroupId;
        }
        public class ObjectStateSix
        {
            public uint DataId;
            public Vector3 Position;
            public float Rotation;
            public int GroupId;
            public int Index;
            public bool IsDrawn;
        }
        private ScriptAccessory _acc;

        [UserSetting("P4 Tower Soak Strategy")]
        public Phase4_Towers Phase4_Towers1 { get; set; } = Phase4_Towers.AllGuided;

        public enum Phase4_Towers
        {
            AllGuided,
            MeleeClockwiseRangedCounterclockwise
        }

        [UserSetting("Dominion Guidance Configuration")]
        public DominionGuidance DominionGuidance1 { get; set; } = DominionGuidance.Standard22Stack;

        public enum DominionGuidance
        {
            Standard22Stack,
            MeleeFixedStack
        }

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

        private Dictionary<uint, ObjectState> _objStorage = new Dictionary<uint, ObjectState>();
        private Dictionary<uint, ObjectStateSix> _objStorage1 = new Dictionary<uint, ObjectStateSix>();
        private int _setPosCount = 0;
        private int _orderCounter = 0;
        private long _mechanic47086StartTime = 0;
        private int _castCount_46131 = 0;
        private bool _hasCast46148 = false;
        private HashSet<uint> _markedPlayers = new HashSet<uint>();
        private List<MechanicObject> _castingObjects = new List<MechanicObject>();
        private bool _hasCast46162 = false;
        private Dictionary<uint, long> _tether0039DrawnTime = new Dictionary<uint, long>();
        private HashSet<uint> _targetIcon001EPlayers = new HashSet<uint>();
        private List<(uint SourceId, uint ActionId, int Quadrant)> _castingObjects46166_46167 = new List<(uint, uint, int)>();
        private HashSet<int> _dominion46112Regions = new HashSet<int>();
        private long _dominion46112LastEventTicks = 0;
        private long _dominion46112LastDrawTicks = 0;
        private long _starTrackLastCastTicks = 0;
        private Dictionary<uint, (long Ticks, Vector3 Position, float Rotation)> _starTrackLastCastBySource = new Dictionary<uint, (long, Vector3, float)>();
        private long _starTrackFirstCastTicks = 0;
        private HashSet<int> _starTrackFirstBlocks = new HashSet<int>();

        private class MechanicObject
        {
            public uint ActionId;
            public uint SourceId;
            public int Quadrant;
            public int Duration;
        }

        private readonly List<Vector2> _group1Coords = new List<Vector2>
        {
            new Vector2(103.11f, 111.59f),
            new Vector2(111.59f, 103.11f)
        };

        private readonly List<Vector2> _group2Coords = new List<Vector2>
        {
            new Vector2(108.49f, 91.51f),
            new Vector2(96.89f, 88.41f)
        };

        private readonly List<Vector2> _group3Coords = new List<Vector2>
        {
            new Vector2(88.41f, 96.90f),
            new Vector2(91.51f, 108.49f)
        };

        private readonly int[] _fixedSequence = new int[] { 1, 3, 2 };
        private List<Vector2> _allValidCoords;
        private readonly List<Vector2> _validCoords = new List<Vector2>
        {
            new Vector2(101.42f, 112.14f), new Vector2(106.00f, 110.39f),
            new Vector2(110.60f, 100.12f), new Vector2(108.92f, 99.15f),
            new Vector2(110.40f, 93.97f),  new Vector2(100.00f, 87.97f),
            new Vector2(89.60f, 93.97f),   new Vector2(91.08f, 99.15f),
            new Vector2(89.40f, 100.12f),  new Vector2(94.00f, 110.39f),
            new Vector2(98.58f, 112.14f)
        };

        private readonly HashSet<uint> _targetDataIds = new HashSet<uint> { 19184, 19185, 19186 };
        private Dictionary<uint, ObjectState> _tripleComboStorage = new Dictionary<uint, ObjectState>();
        private int _tripleComboSetPosCount = 0;
        private HashSet<uint> _tripleComboRecordedIds = new HashSet<uint>();
        private int _tripleComboCount = 0;

        private void DrawMechanic(ObjectState obj, uint objId, int delay, int duration, uint bossId, ScriptAccessory accessory)
        {
            string baseName = $"Triple_{obj.DataId}_{delay}_{DateTime.Now.Ticks}";

            if (obj.DataId == 19184)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = baseName + "_Iron_Obj";
                dp.Position = obj.Position;
                dp.Scale = new Vector2(8f);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = delay; dp.DestoryAt = duration; dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else if (obj.DataId == 19185)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = baseName + "_Moon_Obj";
                dp.Position = obj.Position;
                dp.Scale = new Vector2(60f); dp.InnerScale = new Vector2(5f);
                dp.Radian = float.Pi * 2;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = delay; dp.DestoryAt = duration; dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
            }
            else if (obj.DataId == 19186)
            {
                for (int k = 0; k < 4; k++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"{baseName}_Cross_Obj_{k}";
                    dp.Position = obj.Position;
                    dp.Rotation = obj.Rotation + (float)(Math.PI / 2 * k);
                    dp.Scale = new Vector2(10f, 40f);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = delay; dp.DestoryAt = duration; dp.ScaleMode = ScaleMode.YByTime;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
            }

            if (obj.DataId == 19184)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = baseName + "_Iron_Player";
                dp.Owner = accessory.Data.Me;
                dp.Scale = new Vector2(6f);
                dp.Color = GuideColor.V4;
                dp.Delay = delay; dp.DestoryAt = duration; dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else if (obj.DataId == 19185)
            {
                var party = accessory.Data.PartyList;
                foreach (var tid in party)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"{baseName}_Moon_Player_{tid}";
                    dp.Owner = objId; dp.TargetObject = tid;
                    dp.Radian = float.Pi / 6; dp.Scale = new Vector2(60f);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = delay; dp.DestoryAt = duration; dp.ScaleMode = ScaleMode.ByTime;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                }
            }
            else if (obj.DataId == 19186)
            {
                var party = accessory.Data.PartyList;
                for (int i = 2; i <= 3; i++)
                {
                    if (i >= party.Count) break;
                    var tid = party[i];
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"{baseName}_Cross_Tank_{tid}";
                    dp.Owner = objId; dp.TargetObject = tid;
                    dp.Scale = new Vector2(6f); dp.ScaleMode = ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = delay; dp.DestoryAt = duration;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
            }
        }

        private void DrawMechanic(ObjectStateSix obj, uint objId, int delay, int duration, uint bossId, ScriptAccessory accessory)
        {
            string baseName = $"SixCombo_{obj.DataId}_{delay}_{DateTime.Now.Ticks}";

            if (obj.DataId == 19184)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = baseName + "_Iron_Obj";
                dp.Position = obj.Position;
                dp.Scale = new Vector2(8f);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = delay; dp.DestoryAt = duration; dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = baseName + "_Iron_Player";
                dp2.Owner = accessory.Data.Me;
                dp2.Scale = new Vector2(6f);
                dp2.Color = accessory.Data.DefaultSafeColor;
                dp2.Delay = delay; dp2.DestoryAt = duration; dp2.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);
            }
            else if (obj.DataId == 19185)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = baseName + "_Moon_Obj";
                dp.Position = obj.Position;
                dp.Scale = new Vector2(60f); dp.InnerScale = new Vector2(5f);
                dp.Radian = float.Pi * 2;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = delay; dp.DestoryAt = duration; dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

                var party = accessory.Data.PartyList;
                foreach (var tid in party)
                {
                    var dpP = accessory.Data.GetDefaultDrawProperties();
                    dpP.Name = $"{baseName}_Moon_Player_{tid}";
                    dpP.Owner = objId; dpP.TargetObject = tid;
                    dpP.Radian = float.Pi / 6; dpP.Scale = new Vector2(60f);
                    dpP.Color = accessory.Data.DefaultDangerColor;
                    dpP.Delay = delay; dpP.DestoryAt = duration; dpP.ScaleMode = ScaleMode.ByTime;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dpP);
                }
            }
            else if (obj.DataId == 19186)
            {
                for (int k = 0; k < 4; k++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"{baseName}_Cross_Obj_{k}";
                    dp.Position = obj.Position;
                    dp.Rotation = obj.Rotation + (float)(Math.PI / 2 * k);
                    dp.Scale = new Vector2(10f, 40f);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = delay; dp.DestoryAt = duration; dp.ScaleMode = ScaleMode.YByTime;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }

                var party = accessory.Data.PartyList;
                for (int hi = 2; hi <= 3; hi++)
                {
                    if (hi >= party.Count) break;
                    var tid = party[hi];
                    var dpH = accessory.Data.GetDefaultDrawProperties();
                    dpH.Name = $"{baseName}_Cross_Healer_{tid}";
                    dpH.Owner = objId; dpH.TargetObject = tid;
                    dpH.Scale = new Vector2(6f); dpH.ScaleMode = ScaleMode.YByDistance;
                    dpH.Color = accessory.Data.DefaultDangerColor;
                    dpH.Delay = delay; dpH.DestoryAt = duration;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dpH);
                }
            }

            uint myId = accessory.Data.Me;
            var partyList = accessory.Data.PartyList;
            int myIndex = partyList.IndexOf(myId);
            if (myIndex < 0) return;

            Vector3 targetPos;
            bool canGuide = true;
            if (obj.DataId == 19184)
            {
                float guideRot = obj.Rotation + MathF.PI;
                targetPos = GetPointByRotation(obj.Position, guideRot, 11f);
            }
            else if (obj.DataId == 19185)
            {
                int step = myIndex switch
                {
                    0 => 0,
                    7 => 1,
                    3 => 2,
                    5 => 3,
                    1 => 4,
                    4 => 5,
                    2 => 6,
                    6 => 7,
                    _ => -1
                };
                if (step < 0) return;
                float guideRot = obj.Rotation - (MathF.PI / 4f * step);
                targetPos = GetPointByRotation(obj.Position, guideRot, 3.5f);
            }
            else if (obj.DataId == 19186)
            {
                bool isStGroup = myIndex == 1 || myIndex == 3 || myIndex == 5 || myIndex == 7;
                float angle = isStGroup
                    ? obj.Rotation - (MathF.PI * 3f / 4f)
                    : obj.Rotation - (MathF.PI * 5f / 4f);
                targetPos = GetPointByRotation(obj.Position, angle, 11f);
            }
            else
            {
                canGuide = false;
                targetPos = default;
            }

            if (!canGuide) return;

            var dpGuide = accessory.Data.GetDefaultDrawProperties();
            dpGuide.Name = baseName + "_Guide";
            dpGuide.Owner = myId;
            dpGuide.TargetPosition = targetPos;
            dpGuide.Scale = new Vector2(0.5f);
            dpGuide.ScaleMode = ScaleMode.YByDistance;
            dpGuide.Color = GuideColor.V4;
            dpGuide.Delay = delay;
            dpGuide.DestoryAt = duration;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dpGuide);
        }

        private void TryDrawSingleObject(ObjectStateSix obj, uint objId, uint bossId, ScriptAccessory accessory)
        {
            if (obj.IsDrawn) return;

            int i = obj.Index - 1;
            int plannedDelayFromStart = 0;
            int duration = 0;

            if (i == 0)
            {
                plannedDelayFromStart = 0;
                duration = 7050;
            }
            else
            {
                plannedDelayFromStart = 7050 + 5140 * (i - 1);
                duration = 5140;
            }

            long now = DateTime.Now.Ticks;
            long targetTick = _mechanic47086StartTime + (plannedDelayFromStart * 10000);
            long remainingDelayMs = (targetTick - now) / 10000;
            if (remainingDelayMs < 0) remainingDelayMs = 0;

            DrawMechanic(obj, objId, (int)remainingDelayMs, duration, bossId, accessory);
            obj.IsDrawn = true;
        }

        private void ProcessMechanicLogic(ScriptAccessory accessory)
        {
            var myId = accessory.Data.Me;
            var party = accessory.Data.PartyList;
            int myIndex = -1;

            for (int i = 0; i < party.Count; i++)
            {
                if (party[i] == myId) { myIndex = i; break; }
            }
            if (myIndex == -1)
            {
                _castingObjects46166_46167.Clear();
                _targetIcon001EPlayers.Clear();
                return;
            }

            var objs46166 = _castingObjects46166_46167
                .Where(x => x.ActionId == 46166)
                .OrderBy(x => x.Quadrant)
                .ToList();

            var objs46167 = _castingObjects46166_46167
                .Where(x => x.ActionId == 46167)
                .OrderBy(x => x.Quadrant)
                .ToList();

            MechanicObject targetObj = null;

            if (myIndex == 0)
            {
                if (objs46166.Count >= 1) targetObj = objs46166[0];
            }
            else if (myIndex == 1)
            {
                if (objs46166.Count >= 2) targetObj = objs46166[1];
            }
            else
            {
                if (_markedPlayers.Contains(myId)) return;

                if (myIndex == 4 || myIndex == 5)
                {
                    if (objs46167.Count >= 1) targetObj = objs46167[0];
                }
                else if (myIndex == 2 || myIndex == 3 || myIndex == 6 || myIndex == 7)
                {
                    if (objs46167.Count >= 2) targetObj = objs46167[1];
                }
            }

            if (targetObj != null)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Displace_Link_{targetObj.SourceId}_{DateTime.Now.Ticks}";
                dp.Owner = targetObj.SourceId;
                dp.TargetObject = myId;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Scale = new Vector2(0.5f);
                dp.DestoryAt = targetObj.Duration;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
            }
        }

        private static Vector3 GetPointByRotation(Vector3 origin, float rotation, float distance)
        {
            return new Vector3(
                origin.X + MathF.Sin(rotation) * distance,
                origin.Y,
                origin.Z + MathF.Cos(rotation) * distance
            );
        }

        #endregion

        #region Initialization

        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(".*");
            _acc = accessory;
            _setPosCount = 0;
            _hasCast46148 = false;
            _tripleComboSetPosCount = 0;
            _tripleComboRecordedIds.Clear();
            _tether0039DrawnTime.Clear();
            _targetIcon001EPlayers.Clear();
            _castingObjects46166_46167.Clear();
            _dominion46112Regions.Clear();
            _dominion46112LastEventTicks = 0;
            _dominion46112LastDrawTicks = 0;
            _starTrackLastCastTicks = 0;
            _starTrackLastCastBySource.Clear();
            _starTrackFirstCastTicks = 0;
            _starTrackFirstBlocks.Clear();
            _objStorage.Clear();
            _objStorage1.Clear();
            _tripleComboStorage.Clear();
            _hasCast46162 = false;
            _orderCounter = 0;
            _castCount_46131 = 0;
            _mechanic47086StartTime = 0;
            _allValidCoords = _group1Coords.Concat(_group2Coords).Concat(_group3Coords).ToList();
            _markedPlayers.Clear();
            _castingObjects.Clear();

            accessory.Method.SendChat("/e M11S Initialized.");
        }

        #endregion

        #region TTS Only

        [ScriptMethod(name: "Weapon Call", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46087|46088|46089|46010|46012|46014)$"])]
        public void WeaponCall_Alert(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (aid == 46087 || aid == 46010)
            {
                QTTS("Chariot");
                QText("Chariot", 3000, true);
            }
            else if (aid == 46088 || aid == 46012)
            {
                QTTS("Donut");
                QText("Donut", 3000, true);
            }
            else if (aid == 46089 || aid == 46014)
            {
                QTTS("Cross");
                QText("Cross", 3000, true);
            }
        }

        [ScriptMethod(name: "Veteran Weapon TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46028|46102)$"])]
        public void TripleCharge_Alert(Event @event, ScriptAccessory accessory)
        {
            QTTS("Prepare for triple charge");
            QText("Prepare for triple charge", 3000, true);
        }

        [ScriptMethod(name: "Dominion Bombardment TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46037|46114|46115)$"])]
        public void TankBuster_Combo_Alert(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;

            if (aid == 46037 || aid == 46114)
            {
                QTTS("Circle spread plus tankbuster");
                QText("Circle spread + tankbuster", 3000, true);
            }
            else if (aid == 46115)
            {
                QTTS("Cone stack plus tankbuster");
                QText("Cone stack + tankbuster", 3000, true);
            }
        }

        [ScriptMethod(name: "Grand Maelstrom TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46039|46117)$"])]
        public void HPtoOne_Alert(Event @event, ScriptAccessory accessory)
        {
            QTTS("HP to 1");
            QText("HP to 1", 3000, true);
        }

        [ScriptMethod(name: "Eternal Dominion TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46042|46120)$"])]
        public void AOE_Alert_46042(Event @event, ScriptAccessory accessory)
        {
            QTTS("AOE");
            QText("AOE", 3000, true);
        }

        [ScriptMethod(name: "Heavy Meteor TTS (guess)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46152"])]
        public void Stack_Alert_46152(Event @event, ScriptAccessory accessory)
        {
            QTTS("Stack");
            QText("Stack", 3000, true);
        }

        [ScriptMethod(name: "Shockwave TTS (guess)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46140"])]
        public void Meteor_Alert_46140(Event @event, ScriptAccessory accessory)
        {
            QTTS("Large meteor");
            QText("Large meteor", 3000, true);
        }

        [ScriptMethod(name: "Rotating Fire TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(47037|46170)$"])]
        public void RotatingFire_Alert(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;

            if (aid == 47038)
            {
                QTTS("Bidirectional 22 stack");
            }
            else if (aid == 46171)
            {
                QTTS("Four-way 4-person spread");
            }
        }

        #endregion

        #region Dominion

        [ScriptMethod(name: "Dominion", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46035|46112)$"])]
        public void DoubleRectCleave_Draw(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            float baseRotation = @event.SourceRotation;
            var tidStr = @event["TargetId"];
            if (!string.IsNullOrEmpty(tidStr) &&
                ulong.TryParse(tidStr.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var tid))
            {
                var tObj = accessory.Data.Objects.SearchById(tid);
                if (tObj != null)
                {
                    baseRotation = tObj.Rotation;
                }
            }

            for (int i = 0; i < 2; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Rect_Cleave_{aid}_{i}_{DateTime.Now.Ticks}";
                dp.Position = @event.SourcePosition;
                dp.Rotation = baseRotation + (float)(Math.PI * i);
                dp.Scale = new Vector2(10f, 60f);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = dur;
                dp.ScaleMode = ScaleMode.YByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }

            if (aid == 46112)
            {
                TrackDominionGuidance(@event.SourcePosition, dur, accessory);
            }
        }

        #endregion

        #region Guesswork

        [ScriptMethod(name: "Heavy Slash (guess)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46096"])]
        public void TrackingFan_Alert(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            var tidStr = @event["TargetId"];
            if (string.IsNullOrEmpty(tidStr) ||
                !ulong.TryParse(tidStr.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var tid))
                return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Track_Fan_{aid}_{DateTime.Now.Ticks}";
            dp.Owner = @event.SourceId;
            dp.ScaleMode = ScaleMode.ByTime;
            dp.TargetObject = tid;
            dp.Scale = new Vector2(60f);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = dur;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Bombardment (guess)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46133"])]
        public void TargetCircle_46133(Event @event, ScriptAccessory accessory)
        {
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;

            var tidStr = @event["TargetId"];
            if (string.IsNullOrEmpty(tidStr) ||
                !ulong.TryParse(tidStr.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var tid))
                return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Target_Circle_46133_{tid}_{DateTime.Now.Ticks}";
            dp.Owner = tid;
            dp.Scale = new Vector2(4f);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.ScaleMode = ScaleMode.ByTime;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Beast Flame Tail Strike", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46072|46128|46073|46129)$"])]
        public void FrontBackFan_Draw(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Fan_FB_{aid}_{DateTime.Now.Ticks}";
            dp.Position = @event.SourcePosition;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new Vector2(60f);
            dp.Radian = float.Pi / 2;
            dp.DestoryAt = dur;
            dp.ScaleMode = ScaleMode.ByTime;
            if (aid == 46072 || aid == 46128)
            {
                dp.Rotation = @event.SourceRotation;
            }
            else if (aid == 46073 || aid == 46129)
            {
                dp.Rotation = @event.SourceRotation + float.Pi;
            }
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Skyward Earthward", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46064|46066|46068|46070|46155|46157|46159|46161)$"])]
        public void Rect_Gradient_40x40(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Rect_Front_40x40_{aid}_{DateTime.Now.Ticks}";
            dp.Position = @event.SourcePosition;
            dp.Rotation = @event.SourceRotation;
            dp.Scale = new Vector2(40f, 40f);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = dur;
            dp.ScaleMode = ScaleMode.YByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            var dpBack = accessory.Data.GetDefaultDrawProperties();
            dpBack.Name = $"Rect_Back_60x60_{aid}_{DateTime.Now.Ticks}";
            dpBack.Position = @event.SourcePosition;
            dpBack.Rotation = @event.SourceRotation + (float)Math.PI;
            dpBack.Scale = new Vector2(60f, 60f);
            dpBack.Color = accessory.Data.DefaultDangerColor;
            dpBack.DestoryAt = dur;
            dpBack.ScaleMode = ScaleMode.YByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dpBack);
        }

        [ScriptMethod(name: "Dominion Bombardment Circle", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46114"])]
        public void Action_46114_Index(Event @event, ScriptAccessory accessory)
        {
            if (!int.TryParse(@event["DurationMilliseconds"], out var castDuration)) return;
            int finalDuration = castDuration + 7300;

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

            var dpAggro = accessory.Data.GetDefaultDrawProperties();
            dpAggro.Name = $"Aggro2_{@event.SourceId}_{DateTime.Now.Ticks}";
            dpAggro.Owner = @event.SourceId;
            dpAggro.CentreResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
            dpAggro.CentreOrderIndex = 2;
            dpAggro.Scale = new Vector2(6f);
            dpAggro.DestoryAt = finalDuration;
            dpAggro.ScaleMode = ScaleMode.ByTime;
            dpAggro.Color = amITank ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dpAggro);

            for (int i = 0; i < partyIds.Count; i++)
            {
                if (i > 1)
                {
                    var tid = partyIds[i];
                    var dpHD = accessory.Data.GetDefaultDrawProperties();
                    dpHD.Name = $"HD_Danger_{tid}_{DateTime.Now.Ticks}";
                    dpHD.Owner = tid;
                    dpHD.Scale = new Vector2(6f);
                    dpHD.Color = accessory.Data.DefaultDangerColor;
                    dpHD.DestoryAt = finalDuration;
                    dpHD.ScaleMode = ScaleMode.ByTime;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dpHD);
                }
            }
        }

        [ScriptMethod(name: "Dominion Bombardment Cone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46115"])]
        public void Action_46115_Logic(Event @event, ScriptAccessory accessory)
        {
            if (!int.TryParse(@event["DurationMilliseconds"], out var castDuration)) return;
            int finalDuration = castDuration + 7300;

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

            float rad90 = float.Pi / 2;
            float rad45 = float.Pi / 4;

            for (int i = 0; i <= 1; i++)
            {
                if (i < partyIds.Count)
                {
                    var targetId = partyIds[i];
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Tank_Fan_90_{targetId}_{DateTime.Now.Ticks}";
                    dp.Owner = @event.SourceId;
                    dp.TargetObject = targetId;
                    dp.Radian = rad90;
                    dp.Scale = new Vector2(60f);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = finalDuration;
                    dp.ScaleMode = ScaleMode.ByTime;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                }
            }

            if (myIndex > 1)
            {
                var dpSafe = accessory.Data.GetDefaultDrawProperties();
                dpSafe.Name = $"HD_Safe_Fan_45_{myId}_{DateTime.Now.Ticks}";
                dpSafe.Owner = @event.SourceId;
                dpSafe.TargetObject = myId;
                dpSafe.Radian = rad45;
                dpSafe.Scale = new Vector2(60f);
                dpSafe.Color = accessory.Data.DefaultSafeColor;
                dpSafe.DestoryAt = finalDuration;
                dpSafe.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dpSafe);
            }
        }

        #region Triple Combo Improved

        private bool IsCloseToAny(List<Vector2> coords, Vector2 pos, float threshold = 1.0f)
        {
            foreach (var v in coords)
            {
                if (Vector2.Distance(pos, v) < threshold) return true;
            }
            return false;
        }

        private bool IsValidTripleComboPosition(Vector2 pos)
        {
            return IsCloseToAny(_group1Coords, pos) ||
                IsCloseToAny(_group2Coords, pos) ||
                IsCloseToAny(_group3Coords, pos);
        }

        private List<IGameObject> SortWeaponsClockwiseWithTolerance(List<IGameObject> weapons, Vector3 center, float bossRotationRad)
        {
            if (weapons == null || weapons.Count == 0)
            {
                return new List<IGameObject>();
            }

            float bossRotationDeg = bossRotationRad * 180f / MathF.PI;
            if (bossRotationDeg < 0) bossRotationDeg += 360f;

            var weaponsWithAngle = weapons
                .Where(w => w != null)
                .Select(w => new
                {
                    Weapon = w,
                    Angle = GetAbsoluteAngle(w.Position, center)
                })
                .ToList();

            var firstWeapon = weaponsWithAngle
                .OrderBy(w => GetAngleDifference(w.Angle, bossRotationDeg))
                .First();

            float startAngle = firstWeapon.Angle;

            return weaponsWithAngle
                .OrderBy(w => GetRelativeAngleFrom(w.Angle, startAngle))
                .Select(w => w.Weapon)
                .ToList();
        }

        private float GetAngleDifference(float angle1, float angle2)
        {
            float diff = MathF.Abs(angle1 - angle2);
            if (diff > 180f) diff = 360f - diff;
            return diff;
        }

        private float GetRelativeAngleFrom(float angle, float startAngle)
        {
            float relative = angle - startAngle;
            if (relative < 0) relative += 360f;
            return relative;
        }

        [ScriptMethod(name: "Triple Combo", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46103"])]
        public async void Triple_Combo_Draw_Improved(Event @event, ScriptAccessory accessory)
        {
            uint bossId = (uint)@event.SourceId;
            await Task.Delay(2000);

            Vector3 center = new Vector3(100f, 0f, 100f);

            var weapons = accessory.Data.Objects
                .Where(obj => obj.DataId == 19184 || obj.DataId == 19185 || obj.DataId == 19186)
                .Where(obj => Vector2.Distance(new Vector2(obj.Position.X, obj.Position.Z), new Vector2(100f, 100f)) > 5f)
                .ToList();

            if (weapons.Count < 3)
            {
                accessory.Method.SendChat($"/e [Warning] Insufficient objects: {weapons.Count}");
                return;
            }

            float bossRotation = @event.SourceRotation;
            try
            {
                var bossObj = accessory.Data.Objects.SearchById(bossId);
                if (bossObj != null)
                {
                    bossRotation = bossObj.Rotation;
                }
            }
            catch { }

            var sortedWeapons = weapons
                .OrderBy(w => {
                    float dx = w.Position.X - center.X;
                    float dz = w.Position.Z - center.Z;
                    float angle = MathF.Atan2(dx, dz);
                    float relative = angle - bossRotation;
                    while (relative > 0) relative -= MathF.PI * 2;
                    while (relative <= -MathF.PI * 2) relative += MathF.PI * 2;
                    if (MathF.Abs(relative + MathF.PI * 2) < 0.01f) relative = 0;
                    return -relative;
                })
                .ToList();

            int[] delays = { 0, 6300, 11400 };
            int[] durations = { 6300, 5100, 5100 };

            for (int i = 0; i < 3; i++)
            {
                var weapon = sortedWeapons[i];
                var obj = new ObjectState
                {
                    DataId = weapon.DataId,
                    Position = weapon.Position,
                    Rotation = weapon.Rotation,
                    GroupId = 0
                };
                DrawTripleComboMechanic(obj, weapon.EntityId, delays[i], durations[i], bossId, accessory);
            }
        }

        private float GetAbsoluteAngle(Vector3 point, Vector3 center)
        {
            float dx = point.X - center.X;
            float dz = point.Z - center.Z;
            float angleRad = MathF.Atan2(dx, dz);
            float angleDeg = angleRad * 180f / MathF.PI;
            if (angleDeg < 0) angleDeg += 360f;
            return angleDeg;
        }

        private List<IGameObject> SortWeaponsClockwise(List<IGameObject> weapons, Vector3 center, float bossRotationRad)
        {
            if (weapons == null || weapons.Count == 0) return new List<IGameObject>();
            float bossRotationDeg = bossRotationRad * 180f / MathF.PI;
            if (bossRotationDeg < 0) bossRotationDeg += 360f;
            float startRotationDeg = bossRotationDeg;
            return weapons
                .Where(w => w != null)
                .OrderBy(w => GetRelativeAngle(w.Position, center, startRotationDeg))
                .ToList();
        }

        private float GetRelativeAngle(Vector3 point, Vector3 center, float startRotationDeg)
        {
            float dx = point.X - center.X;
            float dz = point.Z - center.Z;
            float angleRad = MathF.Atan2(dx, -dz);
            float angleDeg = angleRad * 180f / MathF.PI;
            if (angleDeg < 0) angleDeg += 360f;
            float relative = angleDeg - startRotationDeg;
            if (relative < 0) relative += 360f;
            return relative;
        }

        private static int? GetDominionRegion(Vector3 position)
        {
            float dx = position.X - 100f;
            float dz = position.Z - 100f;

            if (MathF.Abs(dx) <= 1f && MathF.Abs(dz) <= 1f) return null;

            float absDx = MathF.Abs(dx);
            float absDz = MathF.Abs(dz);

            if (absDz > absDx)
            {
                return dz >= 0 ? 2 : 0;
            }
            return dx >= 0 ? 1 : 3;
        }

        private static float NormalizeAngle(float angle)
        {
            float twoPi = MathF.PI * 2f;
            while (angle > MathF.PI) angle -= twoPi;
            while (angle < -MathF.PI) angle += twoPi;
            return angle;
        }

        private static readonly Vector3[] _starTrackCenterBlockCenters = new[]
        {
            new Vector3(95f, 0f, 95f),
            new Vector3(105f, 0f, 95f),
            new Vector3(95f, 0f, 105f),
            new Vector3(105f, 0f, 105f)
        };

        private static HashSet<int> GetStarTrackHitBlocks(Vector3 lineOrigin, float lineRotation)
        {
            var result = new HashSet<int>();
            float sin = MathF.Sin(lineRotation);
            float cos = MathF.Cos(lineRotation);

            const float halfWidth = 5f;
            const float length = 60f;
            const float halfBlock = 5f;
            const float tolerance = 0.5f;
            float xLimit = halfWidth + halfBlock + tolerance;
            float zMin = -halfBlock - tolerance;
            float zMax = length + halfBlock + tolerance;

            for (int i = 0; i < _starTrackCenterBlockCenters.Length; i++)
            {
                Vector3 blockCenter = _starTrackCenterBlockCenters[i];
                float dx = blockCenter.X - lineOrigin.X;
                float dz = blockCenter.Z - lineOrigin.Z;

                float localX = dx * cos - dz * sin;
                float localZ = dx * sin + dz * cos;

                if (MathF.Abs(localX) <= xLimit && localZ >= zMin && localZ <= zMax)
                {
                    result.Add(i);
                }
            }
            return result;
        }

        private struct DominionSafePositions
        {
            public Vector3 NearRight;
            public Vector3 NearLeft;
            public Vector3 FarRight;
            public Vector3 FarLeft;
        }

        private static DominionSafePositions GetDominionSafePositions(int region)
        {
            Vector3 center = new Vector3(100f, 0f, 100f);
            Vector3 vertex = region switch
            {
                0 => new Vector3(100f, 0f, 80f),
                1 => new Vector3(120f, 0f, 100f),
                2 => new Vector3(100f, 0f, 120f),
                3 => new Vector3(80f, 0f, 100f),
                _ => new Vector3(100f, 0f, 100f)
            };

            bool axisIsX = MathF.Abs(vertex.X - 100f) > 0.01f;
            float axisBase = axisIsX ? vertex.X : vertex.Z;
            float axisDir = axisBase > 100f ? -1f : 1f;
            float nearAxis = axisBase + axisDir * 3.5f;
            float farAxis = axisBase + axisDir * 20f;
            float perpBase = axisIsX ? vertex.Z : vertex.X;

            Vector3 nearA;
            Vector3 nearB;
            Vector3 farA;
            Vector3 farB;

            if (axisIsX)
            {
                nearA = new Vector3(nearAxis, 0f, perpBase + 7f);
                nearB = new Vector3(nearAxis, 0f, perpBase - 7f);
                farA = new Vector3(farAxis, 0f, perpBase + 8.5f);
                farB = new Vector3(farAxis, 0f, perpBase - 8.5f);
            }
            else
            {
                nearA = new Vector3(perpBase + 7f, 0f, nearAxis);
                nearB = new Vector3(perpBase - 7f, 0f, nearAxis);
                farA = new Vector3(perpBase + 8.5f, 0f, farAxis);
                farB = new Vector3(perpBase - 8.5f, 0f, farAxis);
            }

            Vector3 forward = Vector3.Normalize(new Vector3(vertex.X - center.X, 0f, vertex.Z - center.Z));
            Vector3 right = new Vector3(-forward.Z, 0f, forward.X);

            bool nearAIsRight = Vector3.Dot(new Vector3(nearA.X - center.X, 0f, nearA.Z - center.Z), right) > 0f;
            bool farAIsRight = Vector3.Dot(new Vector3(farA.X - center.X, 0f, farA.Z - center.Z), right) > 0f;

            return new DominionSafePositions
            {
                NearRight = nearAIsRight ? nearA : nearB,
                NearLeft = nearAIsRight ? nearB : nearA,
                FarRight = farAIsRight ? farA : farB,
                FarLeft = farAIsRight ? farB : farA
            };
        }

        private static float GetDominionAxisValue(Vector3 point)
        {
            return MathF.Abs(point.X - 100f) > 0.01f ? point.X : point.Z;
        }

        private static IEnumerable<Vector3> GetDominionSafePoints(int region)
        {
            DominionSafePositions safe = GetDominionSafePositions(region);
            return new[] { safe.NearRight, safe.NearLeft, safe.FarRight, safe.FarLeft };
        }

        private void TrackDominionGuidance(Vector3 position, int duration, ScriptAccessory accessory)
        {
            long now = DateTime.Now.Ticks;
            long resetTicks = 150000000L;

            if (now - _dominion46112LastEventTicks > resetTicks)
            {
                _dominion46112Regions.Clear();
                _dominion46112LastDrawTicks = 0;
            }

            _dominion46112LastEventTicks = now;

            if (_dominion46112LastDrawTicks != 0 && now - _dominion46112LastDrawTicks < resetTicks)
            {
                return;
            }

            var region = GetDominionRegion(position);
            if (!region.HasValue) return;

            _dominion46112Regions.Add(region.Value);

            if (_dominion46112Regions.Count == 3)
            {
                int missing = Enumerable.Range(0, 4).First(r => !_dominion46112Regions.Contains(r));

                uint myId = accessory.Data.Me;
                var party = accessory.Data.PartyList;
                int myIndex = party.IndexOf(myId);

                if (myIndex >= 0)
                {
                    DominionSafePositions safe = GetDominionSafePositions(missing);
                    Vector3 targetPos = default;
                    bool hasTarget = true;

                    if (myIndex == 2 || myIndex == 6)
                    {
                        targetPos = safe.NearRight;
                    }
                    else if (myIndex == 3 || myIndex == 7)
                    {
                        targetPos = safe.NearLeft;
                    }
                    else if (DominionGuidance1 == DominionGuidance.Standard22Stack)
                    {
                        if (myIndex == 0 || myIndex == 4)
                        {
                            targetPos = safe.FarRight;
                        }
                        else if (myIndex == 1 || myIndex == 5)
                        {
                            targetPos = safe.FarLeft;
                        }
                        else
                        {
                            hasTarget = false;
                        }
                    }
                    else
                    {
                        Vector3 farNegative = GetDominionAxisValue(safe.FarRight) < 100f ? safe.FarRight : safe.FarLeft;
                        Vector3 farPositive = GetDominionAxisValue(safe.FarRight) > 100f ? safe.FarRight : safe.FarLeft;

                        if (myIndex == 0 || myIndex == 4)
                        {
                            targetPos = farNegative;
                        }
                        else if (myIndex == 1 || myIndex == 5)
                        {
                            targetPos = farPositive;
                        }
                        else
                        {
                            hasTarget = false;
                        }
                    }

                    if (hasTarget)
                    {
                        var dpGuide = accessory.Data.GetDefaultDrawProperties();
                        dpGuide.Name = $"Dominion_46112_GuideLine_{myId}_{DateTime.Now.Ticks}";
                        dpGuide.Owner = myId;
                        dpGuide.TargetPosition = targetPos;
                        dpGuide.Scale = new Vector2(0.5f);
                        dpGuide.ScaleMode = ScaleMode.YByDistance;
                        dpGuide.Color = GuideColor.V4;
                        dpGuide.DestoryAt = duration;

                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dpGuide);
                    }
                }

                _dominion46112LastDrawTicks = now;
                _dominion46112Regions.Clear();
            }
            else if (_dominion46112Regions.Count > 3)
            {
                _dominion46112Regions.Clear();
            }
        }

        private void DrawTripleComboMechanic(ObjectState obj, uint objId, int delay, int duration, uint bossId, ScriptAccessory accessory)
        {
            string baseName = $"Triple_{obj.DataId}_{delay}_{DateTime.Now.Ticks}";

            if (obj.DataId == 19184)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = baseName + "_Iron_Obj";
                dp.Position = obj.Position;
                dp.Scale = new Vector2(8f);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = delay;
                dp.DestoryAt = duration;
                dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                var dpPlayer = accessory.Data.GetDefaultDrawProperties();
                dpPlayer.Name = baseName + "_Iron_Player";
                dpPlayer.Owner = accessory.Data.Me;
                dpPlayer.Scale = new Vector2(6f);
                dpPlayer.Color = accessory.Data.DefaultSafeColor;
                dpPlayer.Delay = delay;
                dpPlayer.DestoryAt = duration;
                dpPlayer.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dpPlayer);
            }
            else if (obj.DataId == 19185)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = baseName + "_Moon_Obj";
                dp.Position = obj.Position;
                dp.Scale = new Vector2(60f);
                dp.InnerScale = new Vector2(5f);
                dp.Radian = float.Pi * 2;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = delay;
                dp.DestoryAt = duration;
                dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

                var party = accessory.Data.PartyList;
                foreach (var tid in party)
                {
                    var dpFan = accessory.Data.GetDefaultDrawProperties();
                    dpFan.Name = $"{baseName}_Moon_Fan_{tid}";
                    dpFan.Owner = objId;
                    dpFan.TargetObject = tid;
                    dpFan.Radian = float.Pi / 6;
                    dpFan.Scale = new Vector2(60f);
                    dpFan.Color = accessory.Data.DefaultDangerColor;
                    dpFan.Delay = delay;
                    dpFan.DestoryAt = duration;
                    dpFan.ScaleMode = ScaleMode.ByTime;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dpFan);
                }
            }
            else if (obj.DataId == 19186)
            {
                for (int k = 0; k < 4; k++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"{baseName}_Cross_Obj_{k}";
                    dp.Position = obj.Position;
                    dp.Rotation = obj.Rotation + (float)(Math.PI / 2 * k);
                    dp.Scale = new Vector2(10f, 40f);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = delay;
                    dp.DestoryAt = duration;
                    dp.ScaleMode = ScaleMode.YByTime;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }

                var party = accessory.Data.PartyList;
                for (int hi = 2; hi <= 3; hi++)
                {
                    if (hi >= party.Count) break;
                    var tid = party[hi];
                    var dpRect = accessory.Data.GetDefaultDrawProperties();
                    dpRect.Name = $"{baseName}_Cross_Healer_Rect_{tid}";
                    dpRect.Owner = objId;
                    dpRect.TargetObject = tid;
                    dpRect.Scale = new Vector2(6f, 60f);
                    dpRect.Color = accessory.Data.DefaultDangerColor;
                    dpRect.Delay = delay + 2500;
                    dpRect.DestoryAt = Math.Max(duration - 2500, 1000);
                    dpRect.ScaleMode = ScaleMode.YByTime;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dpRect);
                }
            }

            uint myId = accessory.Data.Me;
            var partyList = accessory.Data.PartyList;
            int myIndex = partyList.IndexOf(myId);
            if (myIndex < 0) return;

            Vector3 targetPos;
            bool canGuide = true;
            if (obj.DataId == 19184)
            {
                float guideRot = obj.Rotation + MathF.PI;
                targetPos = GetPointByRotation(obj.Position, guideRot, 11f);
            }
            else if (obj.DataId == 19185)
            {
                int step = myIndex switch
                {
                    0 => 0,
                    7 => 1,
                    3 => 2,
                    5 => 3,
                    1 => 4,
                    4 => 5,
                    2 => 6,
                    6 => 7,
                    _ => -1
                };
                if (step < 0) return;
                float guideRot = obj.Rotation - (MathF.PI / 4f * step);
                targetPos = GetPointByRotation(obj.Position, guideRot, 3.5f);
            }
            else if (obj.DataId == 19186)
            {
                bool isStGroup = myIndex == 1 || myIndex == 3 || myIndex == 5 || myIndex == 7;
                float angle = isStGroup
                    ? obj.Rotation - (MathF.PI * 3f / 4f)
                    : obj.Rotation - (MathF.PI * 5f / 4f);
                targetPos = GetPointByRotation(obj.Position, angle, 11f);
            }
            else
            {
                canGuide = false;
                targetPos = default;
            }

            if (!canGuide) return;

            var dpGuide = accessory.Data.GetDefaultDrawProperties();
            dpGuide.Name = baseName + "_Guide";
            dpGuide.Owner = myId;
            dpGuide.TargetPosition = targetPos;
            dpGuide.Scale = new Vector2(0.5f);
            dpGuide.ScaleMode = ScaleMode.YByDistance;
            dpGuide.Color = GuideColor.V4;
            dpGuide.Delay = delay;
            dpGuide.DestoryAt = duration;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dpGuide);
        }

        #endregion

        [ScriptMethod(name: "Record 6-Combo", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:regex:^(19184|19185|19186)$"])]
        public void Record_Obj_Pos1(Event @event, ScriptAccessory accessory)
        {
            Vector3 rawPos = @event.SourcePosition;
            Vector2 checkPos = new Vector2(rawPos.X, rawPos.Z);

            bool isValid = false;
            foreach (var v in _validCoords)
            {
                if (Vector2.Distance(checkPos, v) < 1.0f)
                {
                    isValid = true;
                    break;
                }
            }
            if (!isValid) return;

            uint sid = (uint)@event.SourceId;

            if (_objStorage1.ContainsKey(sid))
            {
                _objStorage1.Remove(sid);
            }
            else
            {
                if (uint.TryParse(@event["SourceDataId"], out var did))
                {
                    _orderCounter++;

                    var newObj = new ObjectStateSix
                    {
                        DataId = did,
                        Position = rawPos,
                        Rotation = @event.SourceRotation,
                        Index = _orderCounter,
                        IsDrawn = false
                    };
                    _objStorage1[sid] = newObj;

                    long now = DateTime.Now.Ticks;
                    if (_mechanic47086StartTime > 0 && (now - _mechanic47086StartTime < 20 * 10000000))
                    {
                        TryDrawSingleObject(newObj, sid, (uint)@event.SourceId, accessory);
                    }
                }
            }
        }

        [ScriptMethod(name: "6-Combo", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:47086"])]
        public void Action_47086_Draw(Event @event, ScriptAccessory accessory)
        {
            _mechanic47086StartTime = DateTime.Now.Ticks;

            if (_objStorage1.Count == 0) return;

            foreach (var kvp in _objStorage1)
            {
                TryDrawSingleObject(kvp.Value, kvp.Key, (uint)@event.SourceId, accessory);
            }
        }

        [ScriptMethod(name: "Grand Maelstrom Fan", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46120"])]
        public void Action_46120_Fan(Event @event, ScriptAccessory accessory)
        {
            var towers = accessory.Data.Objects.Where(x => x.DataId == 19183).ToList();
            if (towers.Count == 0) return;

            foreach (var tower in towers)
            {
                for (uint i = 1; i <= 2; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Fan_19183_{tower.EntityId}_{i}_{DateTime.Now.Ticks}";
                    dp.Owner = tower.EntityId;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = i;
                    dp.Scale = new Vector2(60f);
                    dp.Radian = float.Pi / 2;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 2300;
                    dp.ScaleMode = ScaleMode.ByTime;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                }
            }
        }

        [ScriptMethod(name: "19183 Danger Circle", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:19183"])]
        public void OnAddCombatant_19183_Circle(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Circle_19183_{@event.SourceId}";
            dp.Position = @event.SourcePosition;
            dp.Scale = new Vector2(4f);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 120000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "19183 Danger Circle Remove", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:19183"])]
        public void OnRemoveCombatant_19183_Circle(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw($"Circle_19183_{@event.SourceId}");
        }

        [ScriptMethod(name: "Star Track", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46131"])]
        public void OnCast_46131(Event @event, ScriptAccessory accessory)
        {
            long nowTicks = DateTime.Now.Ticks;
            uint sourceId = (uint)@event.SourceId;
            Vector3 sourcePos = @event.SourcePosition;
            float sourceRot = @event.SourceRotation;
            const int duplicateWindowMs = 300;
            const float duplicatePosTolerance = 0.2f;
            const float duplicateRotTolerance = 0.05f;

            if (_starTrackLastCastBySource.TryGetValue(sourceId, out var lastCast))
            {
                bool withinTime = nowTicks - lastCast.Ticks < duplicateWindowMs * 10000L;
                bool samePos = Vector3.Distance(lastCast.Position, sourcePos) <= duplicatePosTolerance;
                bool sameRot = MathF.Abs(NormalizeAngle(sourceRot - lastCast.Rotation)) <= duplicateRotTolerance;
                if (withinTime && samePos && sameRot) return;
            }
            _starTrackLastCastBySource[sourceId] = (nowTicks, sourcePos, sourceRot);

            int castIndex = System.Threading.Interlocked.Increment(ref _castCount_46131);
            _starTrackLastCastTicks = nowTicks;

            int roundIndex = ((castIndex - 1) / 2) % 8 + 1;
            bool isFirstInRound = castIndex % 2 == 1;
            int delayDanger = 3800;
            int dangerDuration = 2200;
            bool isStartRound = roundIndex == 1 || roundIndex == 5;
            if (isStartRound)
            {
                delayDanger = 0;
                dangerDuration = 6000;
            }

            var dpDanger = accessory.Data.GetDefaultDrawProperties();
            dpDanger.Name = $"Rect_46131_Danger_{castIndex}_{DateTime.Now.Ticks}";
            dpDanger.Color = accessory.Data.DefaultDangerColor;
            dpDanger.Scale = new Vector2(10f, 60f);
            dpDanger.Position = sourcePos;
            dpDanger.Rotation = sourceRot;
            dpDanger.ScaleMode = ScaleMode.ByTime;
            dpDanger.Delay = delayDanger;
            dpDanger.DestoryAt = dangerDuration;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dpDanger);

            if (isFirstInRound)
            {
                _starTrackFirstCastTicks = nowTicks;
                _starTrackFirstBlocks = new HashSet<int>(GetStarTrackHitBlocks(sourcePos, sourceRot));
                return;
            }

            if (_starTrackFirstBlocks.Count == 0)
            {
                _starTrackFirstCastTicks = 0;
                _starTrackFirstBlocks.Clear();
                return;
            }

            _starTrackFirstCastTicks = 0;
            _starTrackFirstBlocks.Clear();
        }

        [ScriptMethod(name: "Record State", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46148"])]
        public void Record_46148(Event @event, ScriptAccessory accessory)
        {
            _hasCast46148 = true;
        }

        [ScriptMethod(name: "Comet/Fire Breath", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:00F4"])]
        public void OnTargetIcon_00F4(Event @event, ScriptAccessory accessory)
        {
            string tidStr = @event["TargetId"];
            if (string.IsNullOrEmpty(tidStr) ||
                !ulong.TryParse(tidStr.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var targetId))
            {
                return;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.ScaleMode = ScaleMode.ByTime;

            if (!_hasCast46148)
            {
                dp.Name = $"Icon_00F4_Circle_{targetId}_{DateTime.Now.Ticks}";
                dp.Owner = targetId;
                dp.Scale = new Vector2(4f);
                dp.DestoryAt = 8200;
                dp.ScaleMode = ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else
            {
                var sourceObj = accessory.Data.Objects.FirstOrDefault(x => x.DataId == 19180);
                if (sourceObj == null) return;

                dp.Name = $"Link_Delay3s_19180_{targetId}_{DateTime.Now.Ticks}";
                dp.Owner = sourceObj.EntityId;
                dp.TargetObject = targetId;
                dp.Scale = new Vector2(6f, 60f);
                dp.Delay = 3000;
                dp.DestoryAt = 6500;
                dp.ScaleMode = ScaleMode.YByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }

        [ScriptMethod(name: "King Meteor", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0039"])]
        public void OnTether_0039(Event @event, ScriptAccessory accessory)
        {
            if (!_hasCast46162) return;

            string tidStr = @event["TargetId"];
            if (string.IsNullOrEmpty(tidStr) ||
                !ulong.TryParse(tidStr.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var targetId))
            {
                return;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Tether_0039_Rect_{targetId}_{DateTime.Now.Ticks}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new Vector2(10f, 60f);
            dp.Owner = @event.SourceId;
            dp.TargetObject = targetId;
            dp.DestoryAt = 7500;
            dp.ScaleMode = ScaleMode.YByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Rotating Fire Draw", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(47037|46170)$"])]
        public void RotatingFire_Draw(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var aid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;

            int targetCount = (aid == 47037) ? 2 : 4;

            for (uint i = 1; i <= targetCount; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Fire_Rect_Link_{aid}_{i}_{DateTime.Now.Ticks}";
                dp.Color = GuideColor.V4;
                dp.Scale = new Vector2(6f, 60f);
                dp.Owner = @event.SourceId;
                dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                dp.TargetOrderIndex = i;
                dp.DestoryAt = dur;
                dp.ScaleMode = ScaleMode.YByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }

        [ScriptMethod(name: "Record Marker 001E", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:001E"])]
        public void OnTargetIcon_001E(Event @event, ScriptAccessory accessory)
        {
            string tidStr = @event["TargetId"];
            if (string.IsNullOrEmpty(tidStr) ||
                !ulong.TryParse(tidStr.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var targetId))
            {
                return;
            }
            _markedPlayers.Add((uint)targetId);
        }

        [ScriptMethod(name: "ENV22-25", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:regex:^(22|23|24|25)$"])]
        public void OnEnvControl_Rect_Draw(Event @event, ScriptAccessory accessory)
        {
            string flagStr = @event["Flag"];
            if (string.IsNullOrEmpty(flagStr) ||
                !uint.TryParse(flagStr, System.Globalization.NumberStyles.HexNumber, null, out uint flagValue))
            {
                return;
            }

            if (flagValue != 2) return;

            if (!int.TryParse(@event["Index"], out int index)) return;

            float posX = 0;
            switch (index)
            {
                case 22: posX = 79f; break;
                case 23: posX = 89f; break;
                case 24: posX = 111f; break;
                case 25: posX = 121f; break;
                default: return;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Env_Rect_{index}_{DateTime.Now.Ticks}";
            dp.Color = SafeColor1.V4;
            dp.Scale = new Vector2(10f, 40f);
            dp.Position = new Vector3(posX, 0, 80f);
            dp.Rotation = 0f;
            dp.Delay = 23000;
            dp.DestoryAt = 5600;
            dp.ScaleMode = ScaleMode.YByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Line Tether", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(0039|00F9)$"])]
        public void OnTether_0039_00F9(Event @event, ScriptAccessory accessory)
        {
            if (_hasCast46162) return;

            string tidStr = @event["TargetId"];
            if (string.IsNullOrEmpty(tidStr) ||
                !ulong.TryParse(tidStr.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var targetId))
            {
                return;
            }

            uint tid = (uint)targetId;
            long now = DateTime.Now.Ticks;
            long cooldown = 28 * 10000000L;

            if (_tether0039DrawnTime.TryGetValue(tid, out long lastTime))
            {
                if (now - lastTime < cooldown) return;
            }

            _tether0039DrawnTime[tid] = now;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"0039_{tid}";
            dp.Owner = @event.SourceId;
            dp.TargetObject = tid;
            dp.Scale = new Vector2(10f, 60f);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 23000;
            dp.DestoryAt = 5600;
            dp.ScaleMode = ScaleMode.YByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Detect 46162", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:46162"])]
        public void OnCast_46162(Event @event, ScriptAccessory accessory)
        {
            _hasCast46162 = true;
        }

        #endregion

        #region Towers

        [ScriptMethod(name: "Record Marker 001E for Towers", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:001E"])]
        public void OnTargetIcon_001E_Record(Event @event, ScriptAccessory accessory)
        {
            string tidStr = @event["TargetId"];
            if (string.IsNullOrEmpty(tidStr) ||
                !ulong.TryParse(tidStr.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var targetId))
            {
                return;
            }
            _targetIcon001EPlayers.Add((uint)targetId);
        }

        private object _lock46166_46167 = new object();

        [ScriptMethod(name: "1122 Towers", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(46166|46167)$"])]
        public void OnCast_46166_46167_Record(Event @event, ScriptAccessory accessory)
        {
            if (!uint.TryParse(@event["ActionId"], out var actionId)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var duration)) return;

            Vector3 pos = @event.SourcePosition;
            uint sourceId = (uint)@event.SourceId;

            int quadrant = 0;
            if (pos.X > 100 && pos.Z < 100) quadrant = 1;
            else if (pos.X > 100 && pos.Z > 100) quadrant = 2;
            else if (pos.X < 100 && pos.Z > 100) quadrant = 3;
            else if (pos.X < 100 && pos.Z < 100) quadrant = 4;

            if (quadrant == 0) return;

            int currentCount = 0;
            bool shouldDraw = false;

            lock (_lock46166_46167)
            {
                if (_castingObjects46166_46167.Any(x => x.SourceId == sourceId)) return;

                _castingObjects46166_46167.Add((sourceId, actionId, quadrant));
                currentCount = _castingObjects46166_46167.Count;

                if (currentCount >= 4) shouldDraw = true;
            }

            if (shouldDraw)
            {
                if (Phase4_Towers1 == Phase4_Towers.MeleeClockwiseRangedCounterclockwise)
                {
                    DrawDisplacementLogic(accessory, duration);
                }
                if (Phase4_Towers1 == Phase4_Towers.AllGuided)
                {
                    DrawDisplacementLogic1(accessory, duration);
                }
            }
        }

        private void DrawDisplacementLogic1(ScriptAccessory accessory, int duration)
        {
            uint myId = accessory.Data.Me;
            var party = accessory.Data.PartyList;
            int myIndex = -1;

            for (int i = 0; i < party.Count; i++)
            {
                if (party[i] == myId)
                {
                    myIndex = i;
                    break;
                }
            }

            if (myIndex == -1)
            {
                _castingObjects46166_46167.Clear();
                _targetIcon001EPlayers.Clear();
                return;
            }

            var objs46166 = _castingObjects46166_46167
                .Where(x => x.ActionId == 46166)
                .OrderBy(x => x.Quadrant)
                .ToList();

            var objs46167 = _castingObjects46166_46167
                .Where(x => x.ActionId == 46167)
                .OrderBy(x => x.Quadrant)
                .ToList();

            uint targetSourceId = 0;
            bool skipDraw = false;

            if (myIndex == 0)
            {
                if (objs46166.Count >= 1) targetSourceId = objs46166[0].SourceId;
            }
            else if (myIndex == 1)
            {
                if (objs46166.Count >= 2) targetSourceId = objs46166[1].SourceId;
            }
            else if (myIndex == 4)
            {
                if (objs46167.Count >= 1) targetSourceId = objs46167[0].SourceId;
            }
            else if (myIndex == 5)
            {
                if (objs46167.Count >= 2) targetSourceId = objs46167[1].SourceId;
            }
            else if (myIndex == 2 || myIndex == 3 || myIndex == 6 || myIndex == 7)
            {
                if (_targetIcon001EPlayers.Contains(myId))
                {
                    skipDraw = true;
                }
                else
                {
                    int[] priorityOrder = { 2, 6, 7, 3 };

                    var availablePlayers = priorityOrder
                        .Where(idx => idx < party.Count && !_targetIcon001EPlayers.Contains(party[idx]))
                        .ToList();

                    int myRank = -1;
                    for (int i = 0; i < availablePlayers.Count; i++)
                    {
                        if (availablePlayers[i] == myIndex)
                        {
                            myRank = i;
                            break;
                        }
                    }

                    if (myRank == -1) skipDraw = true;
                    else if (myRank == 0)
                    {
                        if (objs46167.Count >= 1) targetSourceId = objs46167[0].SourceId;
                    }
                    else if (myRank == 1)
                    {
                        if (objs46167.Count >= 2) targetSourceId = objs46167[1].SourceId;
                    }
                    else skipDraw = true;
                }
            }

            if (!skipDraw && targetSourceId != 0)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Displacement_{myId}_{targetSourceId}_{DateTime.Now.Ticks}";
                dp.Owner = myId;
                dp.TargetObject = targetSourceId;
                dp.Scale = new Vector2(0.5f);
                dp.ScaleMode = ScaleMode.YByDistance;
                dp.Color = GuideColor.V4;
                dp.DestoryAt = duration;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
            }

            _castingObjects46166_46167.Clear();
            _targetIcon001EPlayers.Clear();
        }

        private void DrawDisplacementLogic(ScriptAccessory accessory, int duration)
        {
            uint myId = accessory.Data.Me;
            var party = accessory.Data.PartyList;
            int myIndex = -1;

            for (int i = 0; i < party.Count; i++)
            {
                if (party[i] == myId)
                {
                    myIndex = i;
                    break;
                }
            }

            if (myIndex == -1)
            {
                _castingObjects46166_46167.Clear();
                _targetIcon001EPlayers.Clear();
                return;
            }

            var objs46166 = _castingObjects46166_46167
                .Where(x => x.ActionId == 46166)
                .OrderBy(x => x.Quadrant)
                .ToList();

            var objs46167 = _castingObjects46166_46167
                .Where(x => x.ActionId == 46167)
                .OrderBy(x => x.Quadrant)
                .ToList();

            uint targetSourceId = 0;
            bool skipDraw = false;

            if (myIndex == 0)
            {
                if (objs46166.Count >= 1) targetSourceId = objs46166[0].SourceId;
            }
            else if (myIndex == 1)
            {
                if (objs46166.Count >= 2) targetSourceId = objs46166[1].SourceId;
            }
            else if (myIndex >= 2 && myIndex <= 7)
            {
                if (_targetIcon001EPlayers.Contains(myId))
                {
                    skipDraw = true;
                }
                else
                {
                    if (myIndex == 4 || myIndex == 5)
                    {
                        if (objs46167.Count >= 1) targetSourceId = objs46167[0].SourceId;
                    }
                    else if (myIndex == 2 || myIndex == 3 || myIndex == 6 || myIndex == 7)
                    {
                        if (objs46167.Count >= 2) targetSourceId = objs46167[1].SourceId;
                    }
                }
            }

            if (!skipDraw && targetSourceId != 0)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Displacement_{myId}_{targetSourceId}_{DateTime.Now.Ticks}";
                dp.Owner = myId;
                dp.TargetObject = targetSourceId;
                dp.Scale = new Vector2(0.5f);
                dp.ScaleMode = ScaleMode.YByDistance;
                dp.Color = GuideColor.V4;
                dp.DestoryAt = duration;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
            }

            _castingObjects46166_46167.Clear();
            _targetIcon001EPlayers.Clear();
        }

        [ScriptMethod(name: "Player Death Remove Tether", eventType: EventTypeEnum.Death)]
        public void OnPlayerDeath(Event @event, ScriptAccessory accessory)
        {
            string tidStr = @event["TargetId"];
            if (string.IsNullOrEmpty(tidStr) ||
                !ulong.TryParse(tidStr.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var targetId))
            {
                return;
            }

            uint tid = (uint)targetId;
            accessory.Method.RemoveDraw($"0039_{tid}");
            if (_tether0039DrawnTime.ContainsKey(tid))
            {
                _tether0039DrawnTime.Remove(tid);
            }
        }

        #endregion
    }
}
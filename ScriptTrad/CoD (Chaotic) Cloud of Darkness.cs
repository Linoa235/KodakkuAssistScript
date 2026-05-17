using Dalamud.Utility.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Common.Math;
using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KDrawScript.Dev
{
    [ScriptType(name: "CoD (Chaotic, guid: "e68faebf-94f0-4392-bbd6-3214e983b7d0") Cloud of Darkness", territorys: [1241], $1067f84fc-489a-4fd2-9f64-5f7f886b37ce", version: "0.0.2.1", Author: "Linoa235", note: NoteStr, updateInfo: UpdateInfo)]
    public class Cloud_of_Darkness_Chaotic
    {
        private const string NoteStr =
        """
        Currently includes basic drawings and A/C team D2-4 waypoints (i.e., fixed swap to opposite platform group).
        Inner platform group mechanics under development.
        Please report any issues with an ARR log.
        """;

        private const string UpdateInfo =
        """
        1. Added functionality to make the boss untargetable based on buff (except for outer tanks, keeping the possibility of provoking the big cloud for rescue).
        2. Added inner platform seed placement waypoints.
        3. Added direction guidance for Thorny Vine tether and tank standby position.
        4. Added pre-positioning and return position indicators for Pivot Particle Beam for inner platform group.
        """;

        private const bool Debugging = false;
        private const bool ReplayGroup = false;

        private static List<string> _role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
        private static List<string> _alliance = ["A", "B", "C"];
        private int _partyMemberIdx = -1;
        private enum CodPhase
        {
            Init,
            Diamond,
            Tilt,
            // Exchange,    // Deprecated, can use bool HaveLoomingChaos instead
        }

        private CodPhase _codPhase = CodPhase.Init;
        private static List<ManualResetEvent> _events = Enumerable
            .Range(0, 20)
            .Select(_ => new ManualResetEvent(false))
            .ToList();
        private static PriorityDict _pd = new PriorityDict();

        private List<(ulong, string)> Embrace = [];
        private string DelayWhat = string.Empty;
        private bool HaveLoomingChaos = false;
        private bool HasShownMemberIdx = false;
        private readonly List<Vector3> FlarePoint = [new(72, 0, 76), new(100, 0, 103), new(126, 0, 76)];
        private readonly List<Vector3> SeedPoint = [new(0, 0, 0), new(70, 0, 92), new(70, 0, 107), new(130, 0, 108), new(130, 0, 92)]; // Only A, C Party Each have two points
        private readonly Object SeedLock = new();
        private readonly List<Vector3> TetherPointA = [new(67, 0, 93), new(80, 0, 95), new(80, 0, 104), new(67, 0, 106)];
        private readonly List<Vector3> TetherPointC = [new(133, 0, 106), new(119, 0, 103), new(119, 0, 95), new(132, 0, 94)];
        private readonly List<Vector3> SpreadPointC = [new(126.89f, 0, 94.59f), new(129.41f, 0, 95.50f), new(131.86f, 0, 97.56f), new(131.69f, 0, 102.13f), new(130.01f, 0, 105.10f), new(125.94f, 0, 105.83f)];
        private readonly List<Vector3> SpreadPointA = [new(73.57f, 0, 105.46f), new(70.24f, 0, 105.06f), new(68.16f, 0, 103.41f), new(68.50f, 0, 98.08f), new(70.24f, 0, 96.12f), new(73.31f, 0, 96.32f)];
        private readonly Vector3 CenterC = new(126.50f, 0, 100);
        private readonly Vector3 CenterA = new(73.50f, 0, 100);
        private readonly Vector3 Center = new(100, 0, 100);
        private readonly Tiles TileInstance = new();
        private readonly Towers TowerInstance = new();
        private readonly List<uint> SeedTarget = [];
        private int RazingRecord = 0;
        private bool EverDrawPhaser = false;

        [UserSetting(note: "Enable text notifications")]
        public bool EnableTextInfo { get; set; } = true;

        [UserSetting(note: "Enable additional guidance. Please ensure party sorting is correct.")]
        public bool EnableGuidance { get; set; } = false;

        [UserSetting(note: "Select your alliance.")]
        public PartyEnum Party { get; set; } = PartyEnum.None;
        
        [UserSetting(note: "Show role assignment notification on phase transition")]
        public bool ShowMemberIdxHint { get; set; } = true;

        [UserSetting(note: "Automatically use Arm's Length/Surecast on stored knockback/pull and interrupt Flood of Darkness")]
        public bool UseAction { get; set; } = false;

        [UserSetting(note: "Special notifications - DO NOT ENABLE unless you know what this does")]
        public bool SpecialText { get; set; } = false;

        [UserSetting(note: "Enable inner platform floor tile drawing")]
        public bool DrawTiles { get; set; } = false;

        [UserSetting(note: "Enable experimental tower waypoints")]
        public bool DrawTowers { get; set; } = false;

        [UserSetting(note: "Show displacement indicators even when using knockback immunity")]
        public bool AlwaysShowDisplacement { get; set; } = false;

        public enum PartyEnum
        {
            None = -1,
            A = 0,
            B = 1,
            C = 2
        }

        public void Init(ScriptAccessory accessory)
        {
            _codPhase = CodPhase.Init;
            List<ManualResetEvent> _events = Enumerable
                .Range(0, 20)
                .Select(_ => new ManualResetEvent(false))
                .ToList();
            _pd = new PriorityDict();
            _pd.Init(accessory, "Init", 0);
            
            Embrace.Clear();
            DelayWhat = string.Empty;
            HaveLoomingChaos = false;
            HasShownMemberIdx = false;
            SeedTarget.Clear();
            RazingRecord = 0;
            EverDrawPhaser = false;
            accessory.Method.RemoveDraw(".*");
        }

        #region TestRegion

        [ScriptMethod(name: "---- Test Items ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
            userControl: Debugging)]
        public void SplitLine_TestRegion(Event ev, ScriptAccessory sa)
        {
        }

        [ScriptMethod(name: "Test: Inner or Outer Platform", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
            userControl: Debugging)]
        public void LocateAtWhichPlatform(Event ev, ScriptAccessory sa)
        {
            var str = "";
            str += $"{(IsOnInnerPlatform(sa, sa.Data.Me) ? "On inner platform" : "Not on inner platform")}";
            str += $"{(IsOnSidePlatform(sa, sa.Data.Me) ? "On outer platform" : "Not on outer platform")}";
            sa.Log.Debug(str);
        }

        [ScriptMethod(name: "Test: Who Am I", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
            userControl: Debugging)]
        public void WhoAmI(Event ev, ScriptAccessory sa)
        {
            var myMemberIdx = GetMemberIdx(sa);
            sa.Log.Debug($"Your role: [{_alliance[myMemberIdx / 10]} Alliance {_role[myMemberIdx % 10]}]");
        }

        [ScriptMethod(name: "Test: Get Inner Platform Players", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
            userControl: Debugging)]
        public void PrintInnerPlatformPlayers(Event ev, ScriptAccessory sa)
        {
            var players = GetInnerPlatformPlayers(sa);
            List<string> jobString = ["Tank", "Healer", "Dps"];
            sa.Log.Debug($"====== Inner Platform Players: ======");
            foreach (var player in players)
            {
                sa.Log.Debug(
                    $"{player.Key}, Same Alliance {player.Value.Item1}, Role {jobString[player.Value.Item2]}," +
                    $" {player.Value.Item3}, eid {player.Value.Item4:x8}, Position {player.Value.Item5}");
            }
        }

        [ScriptMethod(name: "Test: Get Outer Platform Players", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
            userControl: Debugging)]
        public void PrintSidePlatformPlayers(Event ev, ScriptAccessory sa)
        {
            var players = GetSidePlatformPlayers(sa);
            List<string> jobString = ["Tank", "Healer", "Dps"];
            sa.Log.Debug($"====== Outer Platform Players: ======");
            foreach (var player in players)
            {
                sa.Log.Debug(
                    $"{player.Key}, Same Alliance {player.Value.Item1}, Role {jobString[player.Value.Item2]}," +
                    $" {player.Value.Item3}, eid {player.Value.Item4:x8}, Position {player.Value.Item5}");
            }
        }

        [ScriptMethod(name: "Test: Toggle Big Cloud Targetable", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
            userControl: Debugging)]
        public unsafe void ToggleCloudsTargetable(Event ev, ScriptAccessory sa)
        {
            var cloudCharaEnum = sa.Data.Objects.GetByDataId(0x461e);
            List<IGameObject> cloudCharaList = cloudCharaEnum.ToList();
            sa.Log.Debug($"Found {cloudCharaList.Count} entities with ID 0x461e (big clouds).");
            if (cloudCharaList.Count != 1) return;

            var cloudChara = cloudCharaList[0];
            SetTargetable(sa, cloudChara, !cloudChara.IsTargetable);
            sa.Log.Debug($"Toggled big cloud targetable state.");
        }

        [ScriptMethod(name: "Test: Toggle Small Clouds Targetable", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
            userControl: Debugging)]
        public unsafe void ToggleShadowsTargetable(Event ev, ScriptAccessory sa)
        {
            var shadowCharaEnum = sa.Data.Objects.GetByDataId(0x461f);
            List<IGameObject> shadowCharaList = shadowCharaEnum.ToList();
            sa.Log.Debug($"Found {shadowCharaList.Count} entities with ID 0x461f (small clouds).");
            if (shadowCharaList.Count != 2) return;

            foreach (var shadowChara in shadowCharaList)
                SetTargetable(sa, shadowChara, !shadowChara.IsTargetable);
            sa.Log.Debug($"Toggled small clouds targetable state.");
        }
        
        [ScriptMethod(name: "Test: Seed Position Enumeration", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
            userControl: Debugging)]
        public void SeedPositionEnumTest(Event ev, ScriptAccessory sa)
        {
            for (int dir = 0; dir < 4; dir++)
            {
                for (int priorIdx = 0; priorIdx < 2; priorIdx++)
                {
                    var pos = GetSeedField(dir, priorIdx);
                    sa.Log.Debug($"Direction {dir}, Priority Sequence {priorIdx} ({(priorIdx == 0 ? "Near" : "Far")}), Seed placed at {pos}");
                }
            }
        }
        
        [ScriptMethod(name: "Test: Print PriorityDict", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
            userControl: Debugging)]
        public void PrintPriorityDict(Event ev, ScriptAccessory sa)
        {
            var str = _pd.ShowPriorities(false);
            sa.Log.Debug(str);
        }
        

        #endregion TestRegion

        #region P1

        [ScriptMethod(name: "---- Phase Diamond ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
            userControl: true)]
        public void SplitLine_Phase2(Event ev, ScriptAccessory sa)
        {
        }

        [ScriptMethod(name: "Phase Transition - Diamond", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40509"],
            userControl: Debugging)]
        public void PhaseChange_P2(Event ev, ScriptAccessory sa)
        {
            _codPhase = CodPhase.Diamond;
            sa.Log.Debug($"Current Phase: {_codPhase}");
            var partyMemberIdxNew = GetMemberIdx(sa);
            if (_partyMemberIdx != partyMemberIdxNew || !HasShownMemberIdx)
            {
                _partyMemberIdx = partyMemberIdxNew;
                HasShownMemberIdx = true;
                if (ShowMemberIdxHint)
                    sa.Method.TextInfo($"Your role: [{_alliance[partyMemberIdxNew / 10]} Alliance {_role[partyMemberIdxNew % 10]}], adjust in User Settings if incorrect.",
                        5000, false);
                else
                    SendText("AOE", sa);
            }
        }

        [ScriptMethod(name: "Blade of Darkness Left/Right Small Moon Ring and Donut", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4044[468])$"])]
        public void BladeofDarkness(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();

            if (@event["ActionId"] != "40448")
            {
                dp.Name = "Blade of Darkness";
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Radian = float.Pi * 2;
                dp.InnerScale = new(12);
                dp.Scale = new(60);
                dp.Position = ParsePosition(@event, "EffectPosition");
                dp.Owner = sid;
                dp.DestoryAt = 7000;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
            }
            else if (@event["ActionId"] == "40448")
            {
                dp.Name = "Blade of Darkness";
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Radian = float.Pi;
                dp.Scale = new(30);
                dp.Position = ParsePosition(@event, "EffectPosition");
                dp.Owner = sid;
                dp.DestoryAt = 7000;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
        }

        [ScriptMethod(name: "AOE Notification", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40510|40456)$"])]
        public void DelugeofDarkness(Event @event, ScriptAccessory accessory)
        {
            SendText("AOE", accessory);
        }

        [ScriptMethod(name: "Razing-volley Particle Beam Outer Wheel Laser", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40511"])]
        public void RazingvolleyParticleBeam(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = $"Razing-volley Particle Beam - {sid}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(8, 45);
            dp.Owner = sid;
            if (RazingRecord < 2) dp.DestoryAt = 8000;
            else
            {
                dp.Delay = 4000;
                dp.DestoryAt = 4000;
            }

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Razing-volley Particle Beam Outer Wheel Laser Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40511"], suppress: 500)]
        public void RazingvolleyParticleBeamRecord(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(200).ContinueWith(t =>
            {
                if (!ParseObjectId(@event["SourceId"], out var sid)) return;
                RazingRecord++;
            });
        }

        [ScriptMethod(name: "Razing-volley Particle Beam Cancel", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40511"], UserControl = false)]
        public void RazingvolleyParticleBeamCancel(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (RazingRecord >= 4) RazingRecord = 0;
            accessory.Method.RemoveDraw($"Razing-volley Particle Beam - {sid}");
        }


        [ScriptMethod(name: "Rapid-sequence Particle Beam", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40512"])]
        public void RapidsequenceParticleBeam(Event @event, ScriptAccessory accessory)
        {
            SendText("Line Stack", accessory);
        }

        [ScriptMethod(name: "Unholy Darkness Healer Pair Stack", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0064"])]
        public void UnholyDarkness(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = $"Unholy Darkness - {tid}";
            dp.Color = accessory.Data.DefaultDangerColor;
            if (EnableGuidance)
                if (IsInSameParty(accessory, tid))
                    if (IsInSameStack(accessory, accessory.Data.Me, tid))
                        dp.Color = accessory.Data.DefaultSafeColor;
            dp.Scale = new(6);
            dp.Owner = tid;
            dp.DestoryAt = 7000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Flare Drawing", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:015A"])]
        public void Flare(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = $"Flare AOE - {tid}";
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.4f);
            dp.Scale = new(25);
            dp.Owner = tid;
            dp.Delay = 1500;
            dp.DestoryAt = 6000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            if (accessory.Data.Me != tid || !EnableGuidance) return;
            var index = Party switch
            {
                PartyEnum.A => 0,
                PartyEnum.B => 1,
                PartyEnum.C => 2,
                _ => -1
            };
            if (index == -1) return;

            dp.Name = $"Flare Guide";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Scale = new(1.5f);
            dp.Owner = accessory.Data.Me;
            dp.DestoryAt = 5500;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.TargetPosition = FlarePoint[index];

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "Grim Embrace", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(012[CD])$"], UserControl = false)]
        public void GrimEmbrace(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            switch (@event["Id"])
            {
                case "012C":
                    Embrace.Add((sid, "Forward"));
                    if (sid == accessory.Data.Me) SendText("Stored Forward", accessory);
                    break;
                case "012D":
                    Embrace.Add((sid, "Backward"));
                    if (sid == accessory.Data.Me) SendText("Stored Backward", accessory);
                    break;
            }
        }

        [ScriptMethod(name: "Embrace AOE Release Forward/Backward Drawing", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4181"])]
        public void EmbraceAOE(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (Embrace.Count == 0) return;
            var embrace = Embrace.FirstOrDefault(x => x.Item1 == tid);
            if (string.IsNullOrEmpty(embrace.Item2)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Embrace AOE - {tid}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(8, 8);
            dp.Owner = tid;
            dp.Delay = int.Parse(@event["DurationMilliseconds"]) - 7000;
            dp.DestoryAt = 7000;

            if (tid != accessory.Data.Me)
            {
                dp.Delay = int.Parse(@event["DurationMilliseconds"]) - 3000;
                dp.DestoryAt = 3000;
            }

            if (embrace.Item2 == "Backward")
                dp.Rotation = float.Pi;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "Embrace AOE Release Cancel", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4181"])]
        public void EmbraceAOECancel(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            accessory.Method.RemoveDraw($"Embrace AOE - {tid}");
            Embrace.RemoveAll(x => x.Item1 == tid);
        }

        [ScriptMethod(name: "Endeath Pull Notification", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40515|40531)$"])]
        public void Endeath(Event @event, ScriptAccessory accessory)
        {
            if (@event["ActionId"] == "40515")
            {
                SendText("Prepare for Pull", accessory);
            }
            else if (@event["ActionId"] == "40531")
            {
                DelayWhat = "Endeath";
                SendText("Stored Pull", accessory);
            }
        }

        [ScriptMethod(name: "Delay Death & Aero Delayed Notification", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:4182"])]
        public void DelayDeathAero(Event @event, ScriptAccessory accessory)
        {
            if (string.IsNullOrEmpty(DelayWhat)) return;

            if (DelayWhat == "Endeath")
            {
                SendText("Prepare for Pull", accessory);

                Task.Delay(500).ContinueWith(t =>
                {
                    if (HaveMitigation(accessory)) return;
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "Endeath - Pull";
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = new(100, 0, 76.28f);
                    dp.Scale = new(1.5f, 15);
                    dp.DestoryAt = 3000;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                });
            }
            else if (DelayWhat == "Enaero")
            {
                SendText("Prepare for Knockback", accessory);

                Task.Delay(500).ContinueWith(t =>
                {
                    if (HaveMitigation(accessory)) return;
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "Enaero - Knockback";
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Position = new(100, 0, 76.28f);
                    dp.TargetObject = accessory.Data.Me;
                    dp.Scale = new(1.5f, 21);
                    dp.DestoryAt = 2000;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                );
            }
            if (UseAction) AutoSCAL(accessory);
            DelayWhat = string.Empty;
        }

        [ScriptMethod(name: "Enaero Knockback Notification", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40524|40532)$"])]
        public void Enaero(Event @event, ScriptAccessory accessory)
        {
            if (@event["ActionId"] == "40524")
            {
                SendText("Prepare for Knockback", accessory);
            }
            else if (@event["ActionId"] == "40532")
            {
                DelayWhat = "Enaero";
                SendText("Stored Knockback", accessory);
            }
        }

        [ScriptMethod(name: "Enaero AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4052[36])$"])]
        public void EnaeroAOE(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "Enaero AOE";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;

            switch (@event["ActionId"])
            {
                case "40523":
                    dp.Scale = new(8);
                    dp.DestoryAt = 2000;

                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                    dp.Name = "Enaero - Knockback";
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Position = new(100, 0, 76.28f);
                    dp.TargetObject = accessory.Data.Me;
                    dp.Scale = new(1.5f, 21);
                    dp.DestoryAt = 2500;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    break;
                case "40526":
                    dp.Scale = new(8);
                    dp.DestoryAt = 1000;

                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    break;
            }
        }

        [ScriptMethod(name: "Endeath AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4052[01]|4051[78])$"])]
        public void EndeathAOE(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Endeath AOE";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;

            switch (@event["ActionId"])
            {
                case "40520":
                    dp.Scale = new(6);
                    dp.DestoryAt = 3000;

                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    break;
                case "40521":
                    dp.Scale = new(40);
                    dp.InnerScale = new(6);
                    dp.Delay = 3000;
                    dp.DestoryAt = 2000;
                    dp.Radian = float.Pi * 2;

                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                    return;
                case "40517":
                    dp.Scale = new(6);
                    dp.DestoryAt = 4000;

                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                    dp.Name = "Endeath - Pull";
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = new(100, 0, 76.28f);
                    dp.Scale = new(1.5f, 15);
                    dp.DestoryAt = 3000;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    break;
                case "40518":
                    dp.Scale = new(40);
                    dp.InnerScale = new(6);
                    dp.Delay = 4000;
                    dp.DestoryAt = 2000;
                    dp.Radian = float.Pi * 2;

                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                    return;
            }
        }

        [ScriptMethod(name: "Break IV Look Away Notification", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4052[79])$"])]
        public void BreakIV(Event @event, ScriptAccessory accessory)
        {
            SendText("Look Away", accessory);
            // if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            //
            // var dp = accessory.Data.GetDefaultDrawProperties();
            // dp.Name = $"Break IV - {sid}";
            // dp.Color = accessory.Data.DefaultDangerColor;
            // dp.Scale = new(1);
            // dp.Owner = sid;
            // dp.TargetObject = accessory.Data.Me;
            // dp.DestoryAt = 4000;
            //
            // accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            var sid = @event.SourceId;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "BreakEye";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = sid;
            dp.Delay = 0;
            dp.DestoryAt = 4000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.SightAvoid, dp);
        }
        #endregion
        #region P2

        [ScriptMethod(name: "---- Phase Tilt ----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloayaWorld"],
            userControl: true)]
        public void SplitLine_Phase3(Event ev, ScriptAccessory sa)
        {
        }

        [ScriptMethod(name: "Phase Transition - Tilt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40449"],
            userControl: Debugging)]
        public void PhaseChange_P3(Event ev, ScriptAccessory sa)
        {
            _codPhase = CodPhase.Tilt;
            sa.Log.Debug($"Current Phase: {_codPhase}");
            var partyMemberIdxNew = GetMemberIdx(sa);
            if (_partyMemberIdx != partyMemberIdxNew || !HasShownMemberIdx)
            {
                _partyMemberIdx = partyMemberIdxNew;
                HasShownMemberIdx = true;
                if (ShowMemberIdxHint)
                    sa.Method.TextInfo($"Your role: [{_alliance[partyMemberIdxNew / 10]} Alliance {_role[partyMemberIdxNew % 10]}], adjust in User Settings if incorrect.",
                    5000, false);
                else
                    SendText($"AOE", sa);
            }
        }
        
        [ScriptMethod(name: "Initialize Tiles", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40449"], userControl: false)]
        public void RecordOwner(Event @event, ScriptAccessory accessory) => TileInstance.InitOwner(accessory);
        
        
        [ScriptMethod(name: "Initial Position at Start", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40449"],
        userControl: true)]
        public void P3InitField(Event ev, ScriptAccessory sa)
        {
            var myMemberIdx = GetMemberIdx(sa);
            if (myMemberIdx == -1)
            {
                sa.Log.Debug($"Failed to get MemberIdx, target may not be in my party");
                return;
            }

            Vector3 safePos = myMemberIdx switch
            {
                10 => GetBlockField(7, 3),  // B-MT
                11 => GetBlockField(7, 6),  // B-ST
                12 => GetBlockField(6, 2),  // B-H1
                13 => GetBlockField(6, 7),  // B-H2
                14 => GetBlockField(3, 2),  // B-D1
                15 => GetBlockField(3, 7),  // B-D2
                16 => GetBlockField(8, 2),  // B-D3
                17 => GetBlockField(8, 7),  // B-D4

                2 => GetBlockField(1, 2),   // A-H1
                1 => GetBlockField(2, 3),   // A-ST
                22 => GetBlockField(1, 7),  // C-H1
                21 => GetBlockField(2, 6),  // C-ST
                _ => new Vector3(0, 0, 0),
            };

            if (safePos != new Vector3(0, 0, 0))
            {
                var dp0 = sa.Data.GetDefaultDrawProperties();
                dp0.Name = $"Square{myMemberIdx}";
                dp0.Scale = new Vector2(6, 6);
                dp0.Position = safePos;
                dp0.Delay = 0;
                dp0.DestoryAt = 7500;
                dp0.Color = sa.Data.DefaultSafeColor;
                sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp0);
            }

            else
            {
                safePos = (myMemberIdx / 10) switch
                {
                    0 => new Vector3(73.5f, 0, 100f),
                    2 => new Vector3(126.5f, 0, 100f),
                    _ => new Vector3(0, 0, 0),
                };
            }

            if (safePos == new Vector3(0, 0, 0)) return;

            var dp = DrawGuidance(sa, safePos, 0, 7500, "Initial Position");
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            sa.Log.Debug($"Obtained {myMemberIdx} initial position {safePos}");
        }

        private int GetMemberIdx(ScriptAccessory sa)
        {
            try
            {
                var myParty = Party switch
                {
                    PartyEnum.A => 0,
                    PartyEnum.B => 10,
                    PartyEnum.C => 20,
                    _ => throw new ArgumentOutOfRangeException()
                };
                var myIndex = sa.Data.PartyList.IndexOf(sa.Data.Me);
                if (myIndex == -1) return -1;
                return myParty + myIndex;
            }
            catch { return -1; }
        }
        
        [ScriptMethod(name: "Wait for Small Clouds to Appear and Become Targetable", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["SourceDataId:17951", "Id:7747"],
            userControl: false)]
        public async void P3ShadowTimeline(Event ev, ScriptAccessory sa)
        {
            if (_codPhase != CodPhase.Tilt) return;
            sa.Log.Debug($"Detected small cloud appearance PlayActionTimeline");
            await Task.Delay(2000);
            _events[0].Set();
        }
        
        [ScriptMethod(name: "Set Boss Targetable Based on Inner/Outer Buff - 1 (Enable together with the next item)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(417[78])$"],
            userControl: true)]
        public void P3DistargetableBoss(Event ev, ScriptAccessory sa)
        {
            if (_codPhase != CodPhase.Tilt) return;
            if (ev.TargetId != sa.Data.Me) return;
            // 4177 inner platform
            // 4178 side platform
            sa.Log.Debug($"Obtained status: {ev.StatusId}");
            _events[0].WaitOne();

            var myObj = sa.Data.MyObject;
            if (myObj == null) return;
            // If a tank goes to the outer edge, the big cloud does not become untargetable, preserving the possibility of outer tank provoking the big cloud for rescue.
            if (myObj.IsTank() && ev.StatusId == 4178u) return;
            
            SetTargetableBoss(sa, ev.StatusId != 4177u, false);
        }
        
        [ScriptMethod(name: "Set Boss Targetable Based on Inner/Outer Buff - 2", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:regex:^(417[78])$"],
            userControl: true)]
        public void P3TargetableBoss(Event ev, ScriptAccessory sa)
        {
            if (_codPhase != CodPhase.Tilt) return;
            if (ev.TargetId != sa.Data.Me) return;
            // 4177 inner platform
            // 4178 side platform
            sa.Log.Debug($"Obtained status: {ev.StatusId}");
            _events[0].WaitOne();
            SetTargetableBoss(sa, ev.StatusId != 4177u, true);
        }
        
        private void SetTargetableBoss(ScriptAccessory sa, bool isCloud, bool isTargetable)
        {
            if (isCloud)
            {
                var cloudCharaEnum = sa.Data.Objects.GetByDataId(0x461e);
                List<IGameObject> cloudCharaList = cloudCharaEnum.ToList();
                sa.Log.Debug($"Found {cloudCharaList.Count} entities with ID 0x461e (big clouds).");
                if (cloudCharaList.Count != 1) return;
                var cloudChara = cloudCharaList[0];
                SetTargetable(sa, cloudChara, isTargetable);
            }
            else
            {
                var shadowCharaEnum = sa.Data.Objects.GetByDataId(0x461f);
                List<IGameObject> shadowCharaList = shadowCharaEnum.ToList();
                sa.Log.Debug($"Found {shadowCharaList.Count} entities with ID 0x461f (small clouds).");
                if (shadowCharaList.Count != 2) return;

                foreach (var shadowChara in shadowCharaList)
                    SetTargetable(sa, shadowChara, isTargetable);
            }
        }
        
        [ScriptMethod(name: "Dark Dominion Donut", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40456"])]
        public void DarkDominionDonut(Event ev, ScriptAccessory sa)
        {
            var dp = sa.Data.GetDefaultDrawProperties();
            dp.Name = "Dark Dominion Donut";
            dp.Scale = new Vector2(40);
            dp.InnerScale = new Vector2(34);
            dp.Radian = float.Pi * 2;
            dp.Position = Center;
            dp.Color = sa.Data.DefaultDangerColor;
            dp.Delay = 0;
            dp.DestoryAt = 5000;
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
    
        [ScriptMethod(name: "Ghastly Gloom Big Cloud Donut and Cross Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40458|40460)$"])]
        public void GhastlyGloom(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Ghastly Gloom";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 8000;

            switch (@event["ActionId"])
            {
                case "40458":
                    dp.Scale = new(30, 80);

                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

                    dp.Rotation = float.Pi / 2;

                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

                    break;
                case "40460":
                    dp.Scale = new(40);
                    dp.InnerScale = new(21);
                    dp.Radian = float.Pi * 2;

                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                    break;
            }
        }

        [ScriptMethod(name: "Dark Energy Particle Beam Attachment Laser Drawing", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2387"])]
        public void DarkEnergyParticleBeam(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Dark Energy Particle Beam - {tid}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(25);
            dp.Owner = tid;
            dp.Radian = 7.5f.DegToRad();
            dp.Delay = int.Parse(@event["DurationMilliseconds"]) - 5000;
            dp.DestoryAt = 5500;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Dark Energy Particle Beam Cancel", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:2387"], UserControl = false)]
        public void DarkEnergyParticleBeamCancel(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;

            accessory.Method.RemoveDraw($"Dark Energy Particle Beam - {tid}");
        }

        /*
         * TargetIcon 00EF Left 00F0 Right 00F2 6 Spread 00F1 2 Stack
         */

        [ScriptMethod(name: "Third Art Of Darkness Small Cloud Triple Attack", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(00EF|00F[012])$"])]
        public void ThirdArtOfDarkness(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Third Art Of Darkness - {tid}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.Delay = 6000;
            dp.DestoryAt = 4000;

            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            switch (@event["Id"])
            {
                case "00EF":
                    dp.Scale = new(15);
                    dp.Radian = float.Pi;
                    dp.Rotation = float.Pi / 2;
                    dp.Name += " - Left";

                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                    break;
                case "00F0":
                    dp.Scale = new(15);
                    dp.Radian = float.Pi;
                    dp.Rotation = -float.Pi / 2;
                    dp.Name += " - Right";

                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                    break;
                case "00F1":
                    dp.Scale = new(3);
                    dp.Name += " - 2 Stack";

                    for (var i = 1; i <= 3; i++)
                    {
                        dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                        dp.CentreOrderIndex = (uint)i;
                        dp.Name += $" - {i}";

                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    }

                    if (!EnableGuidance || !IsInSameSide(accessory, tid)) return;
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"Third Art Of Darkness - {tid}";
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Owner = tid;
                    dp.Delay = 6000;
                    dp.DestoryAt = 4000;
                    if (!HaveLoomingChaos)
                    {
                        dp.Name = $"Third Art Of Darkness - {index}";
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.Scale = new(1.5f, 5);
                        if (Party == PartyEnum.A)
                        {
                            dp.Position = CenterA;
                            if (index == 0 || index == 3) dp.TargetPosition = new(CenterA.X - 5, CenterA.Y, CenterA.Z);
                            else if (index == 4 || index == 6) dp.TargetPosition = new(CenterA.X, CenterA.Y, CenterA.Z + 5);
                            else if (index == 5 || index == 7) dp.TargetPosition = new(CenterA.X, CenterA.Y, CenterA.Z - 5);
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                        else if (Party == PartyEnum.C)
                        {
                            dp.Position = CenterC;
                            if (index == 0 || index == 3) dp.TargetPosition = new(CenterC.X + 5, CenterC.Y, CenterC.Z);
                            else if (index == 4 || index == 6) dp.TargetPosition = new(CenterC.X, CenterA.Y, CenterC.Z - 5);
                            else if (index == 5 || index == 7) dp.TargetPosition = new(CenterC.X, CenterC.Y, CenterC.Z + 5);
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                    }
                    if (HaveLoomingChaos && index >= 5) // Exchange
                    {
                        dp.Name = $"Third Art Of Darkness - {index}";
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.Scale = new(1.5f, 5);
                        if (Party == PartyEnum.A)
                        {
                            dp.Position = CenterC;
                            if (index == 5) dp.TargetPosition = new(CenterC.X, CenterA.Y, CenterC.Z - 5);
                            else if (index == 6) dp.TargetPosition = new(CenterC.X + 5, CenterC.Y, CenterC.Z);
                            else if (index == 7) dp.TargetPosition = new(CenterC.X, CenterC.Y, CenterC.Z + 5);
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                        else if (Party == PartyEnum.C)
                        {
                            dp.Position = CenterA;
                            if (index == 5) dp.TargetPosition = new(CenterA.X, CenterA.Y, CenterA.Z + 5);
                            else if (index == 6) dp.TargetPosition = new(CenterA.X - 5, CenterA.Y, CenterA.Z);
                            else if (index == 7) dp.TargetPosition = new(CenterA.X, CenterA.Y, CenterA.Z - 5);
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                    }

                    break;
                case "00F2":
                    dp.Scale = new(5, 22);
                    dp.Name += " - 6 Spread";

                    for (var i = 1; i <= 6; i++)
                    {
                        dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                        dp.TargetOrderIndex = (uint)i;
                        dp.Name += $" - {i}";
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                    }

                    if (!EnableGuidance || !IsInSameSide(accessory, tid)) return;
                    var priority = new int[] { 2, -1, -1, 3, 0, 5, 1, 4 };
                    if (priority[index] != -1)
                    {
                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"Third Art Of Darkness - {tid}";
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.Owner = tid;
                        dp.Delay = 6000;
                        dp.DestoryAt = 4000;
                        dp.Name = $"Third Art Of Darkness - {priority[index]}";
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.Scale = new(1.5f, 5);
                        if (Party == PartyEnum.A)
                            dp.TargetPosition = SpreadPointA[priority[index]];
                        else if (Party == PartyEnum.C)
                            dp.TargetPosition = SpreadPointC[priority[index]];
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }

                    break;
            }
        }

        [ScriptMethod(name: "Evil Seed Prepared Position Before Placement",
            eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40490"],
            suppress: 10000, userControl: true)]
        public void EvilSeedPreparedPosition(Event ev, ScriptAccessory sa)
        {
            if (_codPhase != CodPhase.Tilt) return;

            // suppress 10000, this is the cast of two small clouds on the outer edge, avoid double execution
            var myMemberIdx = GetMemberIdx(sa);
            if (myMemberIdx == -1)
            {
                sa.Log.Debug($"Failed to get MemberIdx, target may not be in my party");
                return;
            }

            // Check if player has InnerDarkness status
            if (!IsOnInnerPlatform(sa, sa.Data.Me))
            {
                sa.Log.Debug($"Player is not on the inner platform!");
                return;
            }

            Vector3 readyPos = myMemberIdx switch
            {
                10 => GetBlockField(7, 3),  // B-MT
                11 => GetBlockField(7, 6),  // B-ST
                12 => GetBlockField(7, 1),  // B-H1
                13 => GetBlockField(7, 8),  // B-H2
                14 => GetBlockField(2, 1),  // B-D1
                15 => GetBlockField(2, 8),  // B-D2
                16 => GetBlockField(8, 2),  // B-D3
                17 => GetBlockField(8, 7),  // B-D4

                2 => GetBlockField(1, 2),   // A-H1
                1 => GetBlockField(2, 3),   // A-ST
                22 => GetBlockField(1, 7),  // C-H1
                21 => GetBlockField(2, 6),  // C-ST
                _ => new Vector3(0, 0, 0),
            };

            sa.Method.TextInfo("Position for seed placement", 4000, false);

            if (readyPos == new Vector3(0, 0, 0))
            {
                sa.Log.Debug($"Player who shouldn't be on inner platform is there!");
                return;
            }

            // Draw square
            var dp0 = sa.Data.GetDefaultDrawProperties();
            dp0.Name = $"Square{myMemberIdx}";
            dp0.Scale = new Vector2(6, 6);
            dp0.Position = readyPos;
            dp0.Delay = 0;
            dp0.DestoryAt = 7000;
            dp0.Color = sa.Data.DefaultSafeColor;
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp0);

            // Draw waypoint line
            var dp = DrawGuidance(sa, readyPos, 0, 7000, "Seed placement position");
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            sa.Log.Debug($"Obtained {myMemberIdx} seed placement position {readyPos}");
        }

        [ScriptMethod(name: "Evil Seed Drawing", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0227"])]
        public void EvilSeed(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = $"Evil Seed AOE - {tid}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(5);
            dp.Owner = tid;
            dp.DestoryAt = 8000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            if (!EnableGuidance) return;
            lock (SeedLock)
            {
                if (Party == PartyEnum.None || Party == PartyEnum.B) return; // No support for B Party
                if (IsInSameParty(accessory, tid)) SeedTarget.Add(tid); // Me included of course
                if (SeedTarget.Count != 2) return;
                if (!SeedTarget.Contains(accessory.Data.Me)) return; // Not in the list
                var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var otherIndex = accessory.Data.PartyList.IndexOf(SeedTarget.First(x => x != accessory.Data.Me));
                var offset = (int)Party + (myIndex < otherIndex ? 1 : 2); // A, C Party Index at 1, 2, 3, 4

                dp.Name = $"Evil Seed Guide";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Scale = new(1.5f);
                dp.Owner = accessory.Data.Me;
                dp.DestoryAt = 8000;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.TargetPosition = SeedPoint[offset];

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        
        [ScriptMethod(name: "Evil Seed PriorityDict Initialization",
            eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40490"],
            suppress: 10000, userControl: Debugging)]
        public void EvilSeedPriorityDictInit(Event ev, ScriptAccessory sa)
        {
            // Initialize seed priority dictionary
            _pd.Init(sa, "Evil Seed", 0);
            sa.Log.Debug($"Evil Seed priority dictionary initialized");
        }
        
        [ScriptMethod(name: "Evil Seed Place Inner Platform Guidance", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0227"])]
        public void EvilSeedPlaceInnerGuidance(Event ev, ScriptAccessory sa)
        {
            lock (this)
            {
                // --- Execution conditions
                if (!IsOnInnerPlatform(sa, sa.Data.Me)) return;     // Ignore if player is not on inner platform
                _pd.ActionCount++;                                  // Increase Evil Seed target count
                if (!IsOnInnerPlatform(sa, ev.TargetId)) return;    // Ignore if targeted player is not on inner platform
                
                // Get player's corner direction
                var myChara = sa.Data.MyObject;
                if (myChara == null) return;
                var myDir = Position2Dirs(myChara.Position, Center, 4, false);
                
                // Get target's corner direction
                IPlayerCharacter? tchara = (IPlayerCharacter?)sa.Data.Objects.SearchById(ev.TargetId);
                if (tchara == null) return;
                var targetDir = Position2Dirs(tchara.Position, Center, 4, false);
                
                // Ignore if player and target are in different directions
                if (targetDir != myDir) return;
            
                // --- Add priority
                var tJobPrior = tchara.IsTank() ? 0 :
                    tchara.IsHealer() ? 2 :
                    tchara.IsDps() ? 1 :
                    -1;                     // Get target role priority
                var tidx = ev.TargetId == sa.Data.Me ? 100 : _pd.ActionCount;    // Distinguish player from other players by key
                _pd.Priorities.Add(tidx, tJobPrior);    // Add corresponding priority
                
                // Do not proceed until all 8 seeds are collected
                if (_pd.ActionCount < 8) return;
                
                // --- Priority extraction and evaluation
                var seedTargetNum = _pd.Priorities.Count;
                if (seedTargetNum > 2)
                {
                    sa.Log.Debug($"More than 2 seed targets in the area, ignoring.");
                    return;
                }
                
                // Check if player key is present
                if (!_pd.Priorities.ContainsKey(100))
                {
                    sa.Log.Debug($"Player not targeted by seed, ignoring.");
                    return;
                }
                
                // Get player's priority index in ascending order
                var myPriorIdx = _pd.FindPriorityIndexOfKey(100);
                
                // --- Waypoint
                // Only two possibilities: 0 or 1.
                // 0: small priority value, place seed near the block; 1: large priority value, place seed away from the block.
                var tpos = GetSeedField(myDir, myPriorIdx);
                var dp = DrawGuidance(sa, tpos, 0, 5000, $"Seed placement position {myDir}_{myPriorIdx}");
                sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                sa.Log.Debug($"Player targeted by seed, direction {myDir}, priority sequence {myPriorIdx} ({(myPriorIdx == 0 ? "Near" : "Far")})");
            }
        }
        
        private Vector3 GetSeedField(int dir, int priorIdx)
        {
            // Reference point: direction 0 (top-right) low priority (near) point
            Vector3 seedPlacePos = new Vector3(113.5f, 0f, 95.5f);
            
            // Flip across center if on the other side
            if (dir >= 2)
                seedPlacePos = FoldPointHorizon(FoldPointVertical(seedPlacePos, Center.Z), Center.X);
            
            // Far path case
            if ((dir + priorIdx) % 2 == 1)
                seedPlacePos = FoldPointVertical(seedPlacePos, Center.Z);

            return seedPlacePos;
        }

        [ScriptMethod(name: "Thorny Vine PriorityDict Initialization",
            eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40492"],
            suppress: 10000, userControl: Debugging)]
        public void ThornyVinePriorityDictInit(Event ev, ScriptAccessory sa)
        {
            // Initialize Thorny Vine priority dictionary
            _pd.Init(sa, "Thorny Vine", 0);
            sa.Log.Debug($"Thorny Vine priority dictionary initialized");
        }

        [ScriptMethod(name: "Evil Seed Route Inner Guidance", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:000C"])]
        public void EvilSeedRouteInnerGuidance(Event ev, ScriptAccessory sa)
        {
            lock (this)
            {
                // --- Execution conditions
                if (!IsOnInnerPlatform(sa, sa.Data.Me)) return;     // Ignore if player is not on inner platform
                _pd.ActionCount++;                                  // Increase Thorny Vine target count
                if (!IsOnInnerPlatform(sa, ev.TargetId)) return;    // Ignore if targeted player is not on inner platform
                
                // Get player's corner direction
                var myChara = sa.Data.MyObject;
                if (myChara == null) return;
                var myDir = Position2Dirs(myChara.Position, Center, 4, false);
                
                // Get target's corner direction
                IPlayerCharacter? tchara = (IPlayerCharacter?)sa.Data.Objects.SearchById(ev.TargetId);
                if (tchara == null) return;
                var targetDir = Position2Dirs(tchara.Position, Center, 4, false);
                
                // Ignore if player and target are in different directions
                if (targetDir != myDir) return;
            
                // --- Add priority
                var tJobPrior = tchara.IsTank() ? 0 :
                    tchara.IsHealer() ? 2 :
                    tchara.IsDps() ? 1 :
                    -1;                     // Get target role priority
                var tidx = ev.TargetId == sa.Data.Me ? 100 : _pd.ActionCount;    // Distinguish player from other players by key
                _pd.Priorities.Add(tidx, tJobPrior);    // Add corresponding priority
                
                // Do not proceed until all 12 Thorny Vines are collected
                if (_pd.ActionCount < 12) return;
                sa.Log.Debug($"Player direction {myDir}");
                
                // --- Priority extraction and evaluation
                var seedTargetNum = _pd.Priorities.Count;
                if (seedTargetNum != 2 && seedTargetNum != 0)
                {
                    sa.Log.Debug($"Thorny Vine targets in area are not 0 or 2, ignoring.");
                    return;
                }
                
                var posStart = GetBlockField(2, 7);
                var posEnd = GetBlockField(2, 2);
                    
                // Need vertical flip: myDir == 1 or 2 ==> (myDir + 1 + 4) % 4 >= 2
                if ((myDir + 1 + 4) % 4 >= 2)
                {
                    sa.Log.Debug($"Vertical flip");
                    posStart = FoldPointVertical(posStart, Center.Z);
                    posEnd = FoldPointVertical(posEnd, Center.Z);
                }
                
                // Need horizontal swap:
                // 1. myDir == 0, seedTargetNum = 0
                // 2. myDir == 1, seedTargetNum = 0
                // 3. myDir == 2, seedTargetNum = 2
                // 4. myDir == 3, seedTargetNum = 2
                if (myDir + seedTargetNum > 3)
                {
                    sa.Log.Debug($"Horizontal flip");
                    posStart = FoldPointHorizon(posStart, Center.X);
                    posEnd = FoldPointHorizon(posEnd, Center.X);
                }

                var dpRoute = DrawGuidance(sa, posStart, posEnd, 0, 10000, $"Thorny Vine tether path", scale: 2f);
                sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dpRoute);
                
                // Check if player key is present, also check if player is tank
                if (!_pd.Priorities.ContainsKey(100))
                {
                    sa.Log.Debug($"Player not targeted by Thorny Vine.");
                    
                    if (!myChara.IsTank()) return;
                    var tpost = myDir switch
                    {
                        0 => GetBlockField(3, 7),
                        1 => GetBlockField(6, 7),
                        2 => GetBlockField(6, 2),
                        3 => GetBlockField(3, 2),
                        _ => throw new ArgumentException("Unknown player direction")
                    };
                    
                    var dpt = DrawGuidance(sa, tpost, 0, 10000, $"Tank Thorny Vine position");
                    sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dpt);
                    var dpb = DrawBlockField(sa, tpost, sa.Data.DefaultSafeColor, 0, 7000);
                }
                
            }
        }
        
        [ScriptMethod(name: "Evil Seed Tether Positioning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40492"])]
        public void EvilSeedTether(Event @event, ScriptAccessory accessory)
        {
            if (SpecialText) SendText("Top-right take seed", accessory);
            if (!EnableGuidance) return;
            if (Party == PartyEnum.None || Party == PartyEnum.B) return; // No support for B Party
            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (index == 1 || index == 2) return; // ST and H1 inner platform

            var priority = new int[] { -1, -1, -1, 0, -1, 1, 2, 3 };
            if (priority[index] == -1) return;

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = $"Evil Seed Tether Guide";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.DestoryAt = 8000;
            dp.ScaleMode |= ScaleMode.YByDistance;
            if (Party == PartyEnum.A)
                dp.TargetPosition = TetherPointA[priority[index]];
            else if (Party == PartyEnum.C)
                dp.TargetPosition = TetherPointC[priority[index]];

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "Particle Concentration Tower Waypoints", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40472"])]
        public void ParticleConcentration(Event @event, ScriptAccessory accessory)
        {
            if (SpecialText)
                if (!HaveLoomingChaos) SendText("Left/Up Tower", accessory);
                else SendText("Right/Down Tower", accessory);
            if (!EnableGuidance) return;
            if (Party == PartyEnum.None || Party == PartyEnum.B) return; // No support for B Party
            if (HaveLoomingChaos) return; // No support after position swap
            // H1 Team take north / west. H2 Team take south / east
            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (index == 1 || index == 2) return; // ST and H1 inner platform
        }

        [ScriptMethod(name: "Flood of Darkness Interrupt", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40503"])]
        public void FloodOfDarkness(Event @event, ScriptAccessory accessory)
        {
            if (!UseAction) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!IsInSameSide(accessory, sid)) return;
            if (Party == PartyEnum.None || Party == PartyEnum.B) return;

            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (index != 0 && index != 6) return;
            AutoInterrupt(accessory, sid);
        }

        [ScriptMethod(name: "Diffusive Force Particle Beam Spread Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40464"])]
        public void DiffusiveForceParticleBeam(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Diffusive Force Particle Beam";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(7); // Second one would be smaller but for simplicity we use same scale
            dp.Owner = accessory.Data.Me;
            dp.DestoryAt = 10000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Pivot Particle Beam Inner Platform Pre-position Notification", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4046[79])$"],
            userControl: true)]
        public void P3PivotBeamGuidance(Event ev, ScriptAccessory sa)
        {
            if (_codPhase != CodPhase.Tilt) return;
            // Check if player is on inner platform
            if (!IsOnInnerPlatform(sa, sa.Data.Me)) return;

            var myMemberIdx = GetMemberIdx(sa);
            var bottomLeftSafe = ev.ActionId == 40467;  // clockwise 40467
            List<Vector3> pos = myMemberIdx switch
            {
                // UP LEFT
                2 => bottomLeftSafe ? [GetBlockField(2, 6), GetBlockField(1, 2)] : [GetBlockField(1, 2), GetBlockField(1, 2)], // AH1
                1 => bottomLeftSafe ? [GetBlockField(2, 7), GetBlockField(2, 3)] : [GetBlockField(2, 1), GetBlockField(2, 3)], // AST
                14 => bottomLeftSafe ? [GetBlockField(2, 5), GetBlockField(3, 2)] : [GetBlockField(3, 2), GetBlockField(3, 2)], // BD1

                // BOTTOM RIGHT
                13 => bottomLeftSafe ? [GetBlockField(7, 3), GetBlockField(6, 7)] : [GetBlockField(6, 7), GetBlockField(6, 7)], // BH2
                11 => bottomLeftSafe ? [GetBlockField(7, 2), GetBlockField(7, 6)] : [GetBlockField(7, 8), GetBlockField(7, 6)], // BST
                17 => bottomLeftSafe ? [GetBlockField(7, 4), GetBlockField(8, 7)] : [GetBlockField(8, 7), GetBlockField(8, 7)], // BD4

                // UP RIGHT
                22 => bottomLeftSafe ? [GetBlockField(1, 7), GetBlockField(1, 7)] : [GetBlockField(2, 3), GetBlockField(1, 7)], // CH1
                21 => bottomLeftSafe ? [GetBlockField(2, 8), GetBlockField(2, 6)] : [GetBlockField(2, 2), GetBlockField(2, 6)], // CST
                15 => bottomLeftSafe ? [GetBlockField(3, 7), GetBlockField(3, 7)] : [GetBlockField(2, 4), GetBlockField(3, 7)], // BD2

                // BOTTOM LEFT
                12 => bottomLeftSafe ? [GetBlockField(6, 2), GetBlockField(6, 2)] : [GetBlockField(7, 6), GetBlockField(6, 2)], // BH1
                10 => bottomLeftSafe ? [GetBlockField(7, 1), GetBlockField(7, 3)] : [GetBlockField(7, 7), GetBlockField(7, 3)], // BMT
                16 => bottomLeftSafe ? [GetBlockField(8, 2), GetBlockField(8, 2)] : [GetBlockField(7, 5), GetBlockField(8, 2)], // BD3

                _ => []
            };

            if (pos.Count == 0)
            {
                sa.Log.Debug($"Player not belonging to inner platform is there, handle flexibly, no waypoint provided.");
                return;
            }

            // First drawing, pre-position
            // Draw square
            var dp0 = sa.Data.GetDefaultDrawProperties();
            dp0.Name = $"Square{myMemberIdx}";
            dp0.Scale = new Vector2(6, 6);
            dp0.Position = pos[0];
            dp0.Delay = 0;
            dp0.DestoryAt = 14500;
            dp0.Color = sa.Data.DefaultSafeColor;
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp0);

            // Draw waypoint line
            var dp = DrawGuidance(sa, pos[0], 0, 14500, "Pivot Beam pre-position");
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            sa.Log.Debug($"Obtained pre-position for beam {(bottomLeftSafe ? "Bottom-Left/Top-Right safe" : "Bottom-Right/Top-Left safe")}: {pos[0]}");

            // Second drawing, return position            // Draw square
            var dp01 = sa.Data.GetDefaultDrawProperties();
            dp01.Name = $"Square{myMemberIdx}";
            dp01.Scale = new Vector2(6, 6);
            dp01.Position = pos[1];
            dp01.Delay = 21000;
            dp01.DestoryAt = 7000;
            dp01.Color = sa.Data.DefaultSafeColor;
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp01);

            // Draw waypoint line
            var dp1 = DrawGuidance(sa, pos[1], 21000, 7000, "Pivot Beam return position");
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
            sa.Log.Debug($"Obtained return position for beam {(bottomLeftSafe ? "Bottom-Left/Top-Right safe" : "Bottom-Right/Top-Left safe")}: {pos[1]}");

        }

        [ScriptMethod(name: "Chaos Condensed Particle Beam", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40461"])]
        public void ChaosCondensedParticleBeam(Event @event, ScriptAccessory accessory)
        {
            SendText("Big cloud line stack", accessory);
        }
        /* Conflict with in-game notice
        [ScriptMethod(name: "Phaser Text", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4049[56])$"])]
        public void PhaserText(Event @event, ScriptAccessory accessory)
        {
            if (@event["ActionId"] == "40495")
            {
                accessory.Method.TextInfo("Side -> Front", 2000, true);
            }
            else if (@event["ActionId"] == "40496")
            {
                accessory.Method.TextInfo("Front -> Side", 2000, true);
            }
        }
        */
        [ScriptMethod(name: "Phaser AOE Small Cloud Fan Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40497"])]
        public void PhaserAOE(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            var rot = @event.SourceRotation;
            dp.Name = $"Phaser AOE - {sid}";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(23);
            dp.Owner = sid;
            dp.Radian = 60f.DegToRad(); // Need more testing
            if (!EverDrawPhaser)
            {
                dp.DestoryAt = 8000;
            }
            else
            {
                dp.Delay = 7000;
                dp.DestoryAt = 3000;
            }

            Task.Delay(200).ContinueWith(t =>
            {
                EverDrawPhaser = true;
            });

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Phaser AOE Cancel", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40497"], UserControl = false)]
        public void PhaserAOECancel(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (EverDrawPhaser) EverDrawPhaser = false;
            accessory.Method.RemoveDraw($"Phaser AOE - {sid}");
        }

        [ScriptMethod(name: "Active Pivot Particle Beam 90-degree Front/Back Laser", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4046[79])$"])]
        public void ActivePivotParticleBeam(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            bool isOnInnerPlatform = IsOnInnerPlatform(accessory, accessory.Data.Me);

            var dp = accessory.Data.GetDefaultDrawProperties();
            var rot = -float.Pi / 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(18, 80);
            dp.Owner = sid;
            dp.Delay = isOnInnerPlatform ? 0 : 10000;

            var change = @event["ActionId"] == "40467" ? -1 : 1;
            for (var i = 0; i < 5; i++)
            {
                dp.Name = $"Active Pivot Particle Beam - {i}";
                dp.Rotation = i * change * float.Pi * 0.125f + rot;
                dp.FixRotation = true;
                dp.DestoryAt = 4500 + i * 1500 + (isOnInnerPlatform ? 10000 : 0);

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
            }
        }

        [ScriptMethod(name: "Looming Chaos", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:41673"])]
        public void LoomingChaos(Event @event, ScriptAccessory accessory) => SendText("Prepare for swap", accessory);

        [ScriptMethod(name: "Looming Chaos Mark", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:41673"], userControl: false)]
        public void LoomingChaosMark(Event @event, ScriptAccessory accessory) => HaveLoomingChaos = true;

        [ScriptMethod(name: "EnvControl Mixed", eventType: EventTypeEnum.EnvControl)]
        public void EnvControl(Event @event, ScriptAccessory accessory)
        {
            try
            {
                var Index = int.Parse(@event["Index"]);
                var Flag = int.Parse(@event["Flag"]);

                if (DrawTiles)
                {
                    if (accessory.Data.MyObject.HasStatus(4177))
                    {

                        if (Index < 3 || Index > 30) return; // Between 0x03 - 0x1E
                                                             // Flags: Init 2048 Occupied 32 Free 512 Danger 128 Break 8
                        if (Flag == 512 || Flag == 8) TileInstance.CancelDraw(Index, accessory);
                        if (Flag == 32) TileInstance.StartDraw(Index, accessory);
                        if (Flag == 128) TileInstance.StartDraw(Index, accessory, true);
                    }
                }
                if (EnableGuidance)
                {
                    if (accessory.Data.MyObject.HasStatus(4178))
                    {
                        if (Index < 0x3F || Index > 0x46) return; // Between 63 - 70 4 Active Towers
                                                                  // Flags Appear 2 Disappear 8
                        if (Flag == 8) TowerInstance.CancelDraw(Index, accessory);
                        if (Flag == 2)
                        {
                            if (TowerInstance.IsMyTower(accessory, Index, Party, HaveLoomingChaos, DrawTowers))
                                TowerInstance.StartDraw(Index, accessory);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        [ScriptMethod(name: "Cancel Tiles", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40455"], userControl: false)]
        public void CancelTiles(Event @event, ScriptAccessory accessory) => TileInstance.CancelAll(accessory);


        #endregion

        #region Utility
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

        private static Vector3 ParsePosition(Event @event, string type) => JsonConvert.DeserializeObject<Vector3>(@event[type]);

        private void SendText(string text, ScriptAccessory accessory, int duration = 2000, bool isImportant = true)
        {
            if (!EnableTextInfo) return;
            accessory.Method.TextInfo(text, duration, isImportant);
        }

        private bool IsInSameParty(ScriptAccessory accessory, uint target) => accessory.Data.PartyList.Contains(target);
        private bool IsInSameStack(ScriptAccessory accessory, uint source, uint target)
        {
            var sourceIndex = accessory.Data.PartyList.IndexOf(source);
            var targetIndex = accessory.Data.PartyList.IndexOf(target);
            return (sourceIndex % 2) == (targetIndex % 2);
        }

        private unsafe int InWhichParty(ScriptAccessory accessory, uint target)
        {
            var group = GroupManager.Instance()->MainGroup;
            for (var index = 0; index <= 2; index++)
            {
                for (var j = 0; j < 8; j++)
                {
                    var id = group.GetAllianceMemberByGroupAndIndex(index, j)->EntityId;
                    if (id == target) return index;
                }
            }
            return -1;
        }

        private void DisableGuide()
        {
            SpecialText = false;
            EnableGuidance = false;
            UseAction = false;
        }

        private void AutoSCAL(ScriptAccessory accessory)
        {
            // Sure Cast 7559 Arm's Length 7548
            var JobId = accessory.Data.MyObject.ClassJob.Value.ClassJobCategory.RowId;
            // 30 War 31 Magic
            if (JobId == 0) return;
            if (JobId == 30) accessory.Method.UseAction(0xE000_0000, 7548);
            else if (JobId == 31) accessory.Method.UseAction(0xE000_0000, 7559);
            else return;
        }

        private bool HaveMitigation(ScriptAccessory accessory)
        {
            if (AlwaysShowDisplacement) return false;
            return accessory.Data.MyObject.HasStatusAny(new uint[] { 160, 1209 });
        }

        private void AutoInterrupt(ScriptAccessory accessory, uint target)
        {
            // Head Graze 7551 Ranged Interject 7538 Tank
            var JobId = accessory.Data.MyObject.ClassJob.RowId;
            if (JobId == 0) return;

            var RangedId = new List<uint> { 31, 23, 38 };
            var TankId = new List<uint> { 19, 21, 32, 37 };
            if (RangedId.Contains(JobId)) accessory.Method.UseAction(target, 7551);
            else if (TankId.Contains(JobId)) accessory.Method.UseAction(target, 7538);
            else return;
        }

        private bool IsInSameSide(ScriptAccessory accessory, ulong tid)
        {
            var myPosition = accessory.Data.MyObject.Position;
            var targetPosition = accessory.Data.Objects.SearchById(tid).Position;
            var threshold = 20;
            return Vector3.Distance(myPosition, targetPosition) < threshold;
        }

        /// <summary>
        /// Get the center coordinates of the cell at row and column in P3 arena
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Vector3 GetBlockField(int row, int col)
        {
            Vector3 centerTriple = new(100, 0, 100);
            return centerTriple + new Vector3(6 * (col - 4) - 3, 0, 6 * (row - 4) - 3);
        }
        
        /// <summary>
        /// Draw a cell in the P3 arena at the specified center
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="blockCenter"></param>
        /// <param name="color"></param>
        /// <param name="delay"></param>
        /// <param name="destroy"></param>
        /// <param name="draw">Whether to draw directly</param>
        /// <returns></returns>
        private DrawPropertiesEdit DrawBlockField(ScriptAccessory sa, Vector3 blockCenter, Vector4 color, int delay, int destroy, bool draw = true)
        {
            // Draw square
            var dp = sa.Data.GetDefaultDrawProperties();
            dp.Name = $"Square{blockCenter}";
            dp.Scale = new Vector2(6, 6);
            dp.Position = blockCenter;
            dp.Delay = delay;
            dp.DestoryAt = destroy;
            dp.Color = color;
            if (draw) sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
            return dp;
        }
        
        /// <summary>
        /// Fold a point horizontally
        /// </summary>
        /// <param name="point">Point to fold</param>
        /// <param name="centerX">Center axis X coordinate</param>
        /// <returns></returns>
        public static Vector3 FoldPointHorizon(Vector3 point, float centerX)
        {
            return point with { X = 2 * centerX - point.X };
        }

        /// <summary>
        /// Fold a point vertically
        /// </summary>
        /// <param name="point">Point to fold</param>
        /// <param name="centerZ">Center axis Z coordinate</param>
        /// <returns></returns>
        public static Vector3 FoldPointVertical(Vector3 point, float centerZ)
        {
            return point with { Z = 2 * centerZ - point.Z };
        }

        /// <summary>
        /// Check if entity has inner platform buff
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="entityId"></param>
        /// <param name="innerPlatform">On inner platform</param>
        /// <returns></returns>
        private bool IsOnWhichPlatform(ScriptAccessory sa, ulong entityId, bool innerPlatform = true)
        {
            IPlayerCharacter? chara = (IPlayerCharacter?)sa.Data.Objects.SearchById(entityId);
            if (chara == null)
            {
                sa.Log.Error($"Input entityId {entityId} not found");
                return false;
            }
            return chara.HasStatus(innerPlatform ? 4177u : 4178u);
        }

        private bool IsOnInnerPlatform(ScriptAccessory sa, ulong entityId) => IsOnWhichPlatform(sa, entityId, true);
        private bool IsOnSidePlatform(ScriptAccessory sa, ulong entityId) => IsOnWhichPlatform(sa, entityId, false);

        /// <summary>
        /// Get dictionary of players on specified platform
        /// Key: EntityId
        /// Value: (bool sameParty, int job(0:Tank, 1:Healer, 2:Dps, -1:Unknown), string name, ulong eid, Vector3 pos)
        /// </summary>
        /// <param name="sa"></param>
        /// <param name="innerPlatform">Whether to get inner platform players</param>
        /// <returns></returns>
        private unsafe Dictionary<int, (bool, int, string, ulong, Vector3)> GetPlatformPlayers(ScriptAccessory sa, bool innerPlatform = true)
        {
            var innerPlayersDict = new Dictionary<int, (bool sameParty, int job, string name, ulong eid, Vector3 pos)>();
            // First find own party
            for (int i = 0; i < sa.Data.PartyList.Count; i++)
            {
                // Don't use foreach to avoid IndexOutOfRange crash if /pdr leaveduty (??????)
                var entityId = sa.Data.PartyList[i];
                var chara = (IPlayerCharacter?)sa.Data.Objects.SearchById(entityId);
                if (chara == null) continue;
                if (innerPlatform ? !IsOnInnerPlatform(sa, entityId) : !IsOnSidePlatform(sa, entityId)) continue;

                var job = chara.IsTank() ? 0 :
                    chara.IsHealer() ? 1 :
                    chara.IsDps() ? 2 :
                    -1;

                // Dict Key, add 100 to distinguish from MemberIdx
                innerPlayersDict.Add(i + 100, (true, job, chara.Name.ToString(), entityId, chara.Position));
            }

            // Then find alliance
            var group = GroupManager.Instance()->GetGroup(ReplayGroup);
            for (var index = 0; index <= 1; index++)
            {
                for (var j = 0; j < 8; j++)
                {
                    var entityId = group->GetAllianceMemberByGroupAndIndex(index, j)->EntityId;
                    var chara = (IPlayerCharacter?)sa.Data.Objects.SearchById(entityId);
                    if (chara == null) continue;
                    if (innerPlatform ? !IsOnInnerPlatform(sa, entityId) : !IsOnSidePlatform(sa, entityId)) continue;

                    var job = chara.IsTank() ? 0 :
                        chara.IsHealer() ? 1 :
                        chara.IsDps() ? 2 :
                        -1;

                    innerPlayersDict.Add(j + 10 * (index + 1) + 100,
                        (false, job, chara.Name.ToString(), entityId, chara.Position));
                }
            }
            return innerPlayersDict;
        }

        private Dictionary<int, (bool, int, string, ulong, Vector3)> GetInnerPlatformPlayers(ScriptAccessory sa) => GetPlatformPlayers(sa, true);
        private Dictionary<int, (bool, int, string, ulong, Vector3)> GetSidePlatformPlayers(ScriptAccessory sa) => GetPlatformPlayers(sa, false);

        /// <summary>
        /// Returns DP for arrow guidance
        /// </summary>
        /// <param name="accessory"></param>
        /// <param name="ownerObj">Arrow start, can be uint or Vector3</param>
        /// <param name="targetObj">Arrow target, can be uint or Vector3, 0 for no target</param>
        /// <param name="delay">Drawing delay</param>
        /// <param name="destroy">Drawing duration</param>
        /// <param name="name">Drawing name</param>
        /// <param name="rotation">Arrow rotation angle</param>
        /// <param name="scale">Arrow width</param>
        /// <param name="isSafe">Use safe color</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static DrawPropertiesEdit DrawGuidance(ScriptAccessory accessory,
            object ownerObj, object targetObj, int delay, int destroy, string name, float rotation = 0, float scale = 1f, bool isSafe = true)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Scale = new Vector2(scale);
            dp.Rotation = rotation;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = isSafe ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.Delay = delay;
            dp.DestoryAt = destroy;

            if (ownerObj is uint or ulong)
            {
                dp.Owner = (ulong)ownerObj;
            }
            else if (ownerObj is Vector3 spos)
            {
                dp.Position = spos;
            }
            else
            {
                throw new ArgumentException("Invalid input type for ownerObj");
            }

            if (targetObj is uint or ulong)
            {
                if ((ulong)targetObj != 0) dp.TargetObject = (ulong)targetObj;
            }
            else if (targetObj is Vector3 tpos)
            {
                dp.TargetPosition = tpos;
            }
            else
            {
                throw new ArgumentException("Invalid input type for targetObj");
            }

            return dp;
        }

        public static DrawPropertiesEdit DrawGuidance(ScriptAccessory accessory,
            object targetObj, int delay, int destroy, string name, float rotation = 0, float scale = 1f, bool isSafe = true)
            => DrawGuidance(accessory, (ulong)accessory.Data.Me, targetObj, delay, destroy, name, rotation, scale, isSafe);

        public class PriorityDict
        {
            // ReSharper disable once NullableWarningSuppressionIsUsed
            public ScriptAccessory sa { get; set; } = null!;

            // ReSharper disable once NullableWarningSuppressionIsUsed
            public Dictionary<int, int> Priorities { get; set; } = null!;
            public string Annotation { get; set; } = "";
            public int ActionCount { get; set; } = 0;

            public void Init(ScriptAccessory accessory, string annotation, int partyNum = 8)
            {
                sa = accessory;
                Priorities = new Dictionary<int, int>();
                for (var i = 0; i < partyNum; i++)
                {
                    Priorities.Add(i, 0);
                }

                Annotation = annotation;
                ActionCount = 0;
            }

            /// <summary>
            /// Add priority value for a specific key
            /// </summary>
            /// <param name="idx">key</param>
            /// <param name="priority">priority value</param>
            public void AddPriority(int idx, int priority)
            {
                Priorities[idx] += priority;
            }

            /// <summary>
            /// Find the first 'num' indices with smallest values from Priorities, return as new Dict
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public List<KeyValuePair<int, int>> SelectSmallPriorityIndices(int num)
            {
                return SelectMiddlePriorityIndices(0, num);
            }

            /// <summary>
            /// Find the first 'num' indices with largest values from Priorities, return as new Dict
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public List<KeyValuePair<int, int>> SelectLargePriorityIndices(int num)
            {
                return SelectMiddlePriorityIndices(0, num, true);
            }

            /// <summary>
            /// Find middle values from Priorities sorted in ascending order, return as new Dict
            /// </summary>
            /// <param name="skip">Skip 'skip' elements. To start from the second element, skip=1</param>
            /// <param name="num"></param>
            /// <param name="descending">Descending order, default false</param>
            /// <returns></returns>
            public List<KeyValuePair<int, int>> SelectMiddlePriorityIndices(int skip, int num, bool descending = false)
            {
                if (Priorities.Count < skip + num)
                    return new List<KeyValuePair<int, int>>();

                IEnumerable<KeyValuePair<int, int>> sortedPriorities;
                if (descending)
                {
                    // Sort by value descending, then by key
                    sortedPriorities = Priorities
                        .OrderByDescending(pair => pair.Value)
                        .ThenBy(pair => pair.Key)
                        .Skip(skip)
                        .Take(num);
                }
                else
                {
                    // Sort by value ascending, then by key
                    sortedPriorities = Priorities
                        .OrderBy(pair => pair.Value)
                        .ThenBy(pair => pair.Key)
                        .Skip(skip)
                        .Take(num);
                }

                return sortedPriorities.ToList();
            }

            /// <summary>
            /// Find the element at index 'idx' from Priorities sorted in ascending order, return as new Dict
            /// </summary>
            /// <param name="idx"></param>
            /// <param name="descending">Descending order, default false</param>
            /// <returns></returns>
            public KeyValuePair<int, int> SelectSpecificPriorityIndex(int idx, bool descending = false)
            {
                var sortedPriorities = SelectMiddlePriorityIndices(0, 8, descending);
                return sortedPriorities[idx];
            }

            /// <summary>
            /// Find the sorted position index of the value corresponding to the given key from Priorities
            /// </summary>
            /// <param name="key"></param>
            /// <param name="descending">Descending order, default false</param>
            /// <returns></returns>
            public int FindPriorityIndexOfKey(int key, bool descending = false)
            {
                var sortedPriorities = SelectMiddlePriorityIndices(0, 8, descending);
                var i = 0;
                foreach (var dict in sortedPriorities)
                {
                    if (dict.Key == key) return i;
                    i++;
                }

                return i;
            }

            /// <summary>
            /// Add priority values all at once
            /// Usually used for special priorities (e.g., H-T-D-H)
            /// </summary>
            /// <param name="priorities"></param>
            public void AddPriorities(List<int> priorities)
            {
                if (Priorities.Count != priorities.Count)
                    throw new ArgumentException("Input list length does not match internal setting");

                for (var i = 0; i < Priorities.Count; i++)
                    AddPriority(i, priorities[i]);
            }

            /// <summary>
            /// Output the priority dictionary's keys and priorities
            /// </summary>
            /// <returns></returns>
            public string ShowPriorities(bool showJob = true)
            {
                var str = $"{Annotation} ({ActionCount}-th) Priority Dictionary:\n";
                if (Priorities.Count == 0)
                {
                    str += $"PriorityDict Empty.\n";
                    return str;
                }
                foreach (var pair in Priorities)
                {
                    str += $"Key {pair.Key} {(showJob ? $"({_role[pair.Key]})" : "")}, Value {pair.Value}\n";
                }

                return str;
            }
            public PriorityDict DeepCopy()
            {
                return JsonConvert.DeserializeObject<PriorityDict>(JsonConvert.SerializeObject(this)) ??
                       new PriorityDict();
            }

        }

        public unsafe static void SetTargetable(ScriptAccessory sa, IGameObject? obj, bool targetable)
        {
            if (obj == null || !obj.IsValid())
            {
                sa.Log.Error($"Input IGameObject is invalid.");
                return;
            }

            GameObject* charaStruct = (GameObject*)obj.Address;
            if (targetable)
            {
                if (obj.IsDead || obj.IsTargetable) return;
                charaStruct->TargetableStatus |= ObjectTargetableFlags.IsTargetable;
            }
            else
            {
                if (!obj.IsTargetable) return;
                charaStruct->TargetableStatus &= ~ObjectTargetableFlags.IsTargetable;
            }
            sa.Log.Debug($"SetTargetable {targetable} => {obj.Name} {obj}");
        }
        
        /// <summary>
        /// Get logical direction from coordinates (diagonal division with top as 0, orthogonal division with top-right as 0, increasing clockwise)
        /// </summary>
        /// <param name="point">Coordinate point</param>
        /// <param name="center">Center point</param>
        /// <param name="dirs">Total number of directions</param>
        /// <param name="diagDivision">Diagonal division, default true</param>
        /// <returns>Logical direction corresponding to the coordinate point</returns>
        public static int Position2Dirs(Vector3 point, Vector3 center, int dirs, bool diagDivision = true)
        {
            double dirsDouble = dirs;
            var r = diagDivision
                ? Math.Round(dirsDouble / 2 - dirsDouble / 2 * Math.Atan2(point.X - center.X, point.Z - center.Z) / Math.PI) % dirsDouble
                : Math.Floor(dirsDouble / 2 - dirsDouble / 2 * Math.Atan2(point.X - center.X, point.Z - center.Z) / Math.PI) % dirsDouble;
            return (int)r;
        }
        

        // Courtesy of BMR
        public class Tiles
        {
            // - index arrangement:
            //      04             0B
            //   03 05 06 07 0E 0D 0C 0A
            //      08             0F
            //      09             10
            //      17             1E
            //      16             1D
            //   11 13 14 15 1C 1B 1A 18
            //      12             19
            // From 03 - 1E, total of 28 tiles

            private readonly Dictionary<int, (int x, int y)> _cellIndexToCoordinates = GenerateCellIndexToCoordinates();
            private uint BossId = 0;

            public void InitOwner(ScriptAccessory accessory) => BossId = accessory.Data.Objects.GetByDataId(0x461e).FirstOrDefault()?.EntityId ?? 0;

            private static int CellIndex(int x, int y) => (x, y) switch
            {
                (-4, -3) => 0x03,
                (-3, -4) => 0x04,
                (-3, -3) => 0x05,
                (-2, -3) => 0x06,
                (-1, -3) => 0x07,
                (-3, -2) => 0x08,
                (-3, -1) => 0x09,
                (+3, -3) => 0x0A,
                (+2, -4) => 0x0B,
                (+2, -3) => 0x0C,
                (+1, -3) => 0x0D,
                (+0, -3) => 0x0E,
                (+2, -2) => 0x0F,
                (+2, -1) => 0x10,
                (-4, +2) => 0x11,
                (-3, +3) => 0x12,
                (-3, +2) => 0x13,
                (-2, +2) => 0x14,
                (-1, +2) => 0x15,
                (-3, +1) => 0x16,
                (-3, +0) => 0x17,
                (+3, +2) => 0x18,
                (+2, +3) => 0x19,
                (+2, +2) => 0x1A,
                (+1, +2) => 0x1B,
                (+0, +2) => 0x1C,
                (+2, +1) => 0x1D,
                (+2, +0) => 0x1E,
                _ => 0
            };

            private static Dictionary<int, (int x, int y)> GenerateCellIndexToCoordinates()
            {
                var map = new Dictionary<int, (int x, int y)>();
                for (var x = -4; x <= 3; ++x)
                {
                    for (var y = -4; y <= 3; ++y)
                    {
                        var index = CellIndex(x, y);
                        if (index >= 0)
                            map[index] = (x, y);
                    }
                }
                return map;
            }

            private Vector3 CellCenter(int breakTimeIndex)
            {
                // var cellIndex = breakTimeIndex + 3; // We use it as-is.
                var cellIndex = breakTimeIndex;
                if (_cellIndexToCoordinates.TryGetValue(cellIndex, out var coordinates))
                {
                    var worldX = (coordinates.x + 0.5f) * 6f;
                    var worldZ = (coordinates.y + 0.5f) * 6f;
                    return new Vector3(100 + worldX, 0, 100 + worldZ);
                }
                else
                    return default;
            }

            private bool NeedToDraw(ScriptAccessory accessory, Vector3 target) => Vector3.Distance(accessory.Data.MyObject.Position, target) < 20f;

            public void StartDraw(int index, ScriptAccessory accessory, bool isDanger = false)
            {
                if (!NeedToDraw(accessory, CellCenter(index))) return;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Tiles - {index}";
                dp.Color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
                if (isDanger) dp.Color = accessory.Data.DefaultDangerColor;
                dp.ScaleMode = ScaleMode.ByTime;
                dp.Scale = new(6, 6);
                dp.Owner = BossId;
                dp.DestoryAt = 38 * 1000;
                if (isDanger) dp.DestoryAt = 6000;
                dp.Position = CellCenter(index);
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
            }

            public void CancelDraw(int index, ScriptAccessory accessory) => accessory.Method.RemoveDraw($"Tiles - {index}");

            public void CancelAll(ScriptAccessory accessory) => accessory.Method.RemoveDraw($"Tiles - .*");

        }

        // Also BMR
        public class Towers
        {
            // 3-man only
            // - arrangement:
            //     3F         43
            //   42  40     44  46
            //     41         45

            private readonly List<int> LeftSideTower = [0x3F, 0x40, 0x41, 0x42];
            private readonly List<int> RightSideTower = [0x43, 0x44, 0x45, 0x46];
            private readonly List<int> LeftMTGroup = [0x41, 0x42];
            private readonly List<int> RightMTGroup = [0x43, 0x46];
            private readonly List<int> LeftSTGroup = [0x3F, 0x40];
            private readonly List<int> RightSTGroup = [0x44, 0x45];
            private readonly Vector3 Center = new(100, 0, 100);

            private Vector3 TowerCenter(int index)
            {
                var offset = index switch
                {
                    0x3F => new Vector3(-26.5f, 0, -4.5f),
                    0x40 => new Vector3(-22f, 0, 0f),
                    0x41 => new Vector3(-26.5f, 0, 4.5f),
                    0x42 => new Vector3(-31f, 0, 0f),
                    0x43 => new Vector3(26.5f, 0, -4.5f),
                    0x44 => new Vector3(22f, 0, 0f),
                    0x45 => new Vector3(26.5f, 0, 4.5f),
                    0x46 => new Vector3(31f, 0, 0f),
                    _ => Vector3.Zero,
                };

                return new Vector3(100 + offset.X, 0, 100 + offset.Z);
            }

            private bool LeftOrRight(ScriptAccessory accessory) => (accessory.Data.MyObject.Position.X - Center.X) > 0;

            public bool IsMyTower(ScriptAccessory accessory, int index, PartyEnum party, bool afterSwap, bool experiment)
            {
                if (party == PartyEnum.None) return false;
                if ((party == PartyEnum.A && !afterSwap) ||
                    (party == PartyEnum.C && afterSwap))
                {
                    if (!LeftSideTower.Contains(index)) return false;
                    var idx = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                    if (idx < 5 && afterSwap && !experiment) return false;
                    if (!afterSwap)
                    {
                        if (idx % 2 == 0 && idx != 2) return LeftMTGroup.Contains(index);
                        if (idx % 2 == 1 && idx != 3) return LeftSTGroup.Contains(index);
                    }
                    if (experiment && index < 5) return LeftMTGroup.Contains(index);
                    if (index >= 5) return LeftSTGroup.Contains(index);
                }

                if ((party == PartyEnum.A && afterSwap) ||
                    (party == PartyEnum.C && !afterSwap))
                {
                    if (!RightSideTower.Contains(index)) return false;
                    var idx = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                    if (idx < 5 && afterSwap && !experiment) return false;
                    if (!afterSwap)
                    {
                        if (idx % 2 == 0 && idx != 2) return RightMTGroup.Contains(index);
                        if (idx % 2 == 1 && idx != 3) return RightSTGroup.Contains(index);
                    }
                    if (experiment && index < 5) return RightMTGroup.Contains(index);
                    if (index >= 5) return RightSTGroup.Contains(index);
                }

                if (party == PartyEnum.B && afterSwap && experiment)
                {
                    if (!accessory.Data.MyObject.HasStatus(4178)) return false;
                    if (LeftOrRight(accessory)) return RightMTGroup.Contains(index);
                    return LeftMTGroup.Contains(index);
                }
                return false;
            }

            public void StartDraw(int index, ScriptAccessory accessory)
            {
                accessory.Log.Debug($"Start drawing tower {index}");
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Tower - {index}";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.ScaleMode = ScaleMode.ByTime;
                dp.Scale = new(3);
                dp.Owner = accessory.Data.Me;
                dp.DestoryAt = 10000;
                dp.Position = TowerCenter(index);
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
            }

            public void CancelDraw(int index, ScriptAccessory accessory) => accessory.Method.RemoveDraw($"Tower - {index}");
        }

        #endregion
    }

    public static class MathHelper
    {
        public static float DegToRad(this float val)
        {
            return (float)(MathF.PI / 180f * val);
        }
    }
}
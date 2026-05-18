// File: NewMoonIslandCE_XSZYYS.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.ManagedFontAtlas;
using KodakkuAssist.Data;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using Newtonsoft.Json;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.GameOperate;

namespace KodakkuAssistXSZYYS
{
    [ScriptType(
        name: "New Moon Island CE",
        guid: "519bc5f8-1505-4bf7-8242-55c559c06c33",
        territorys: [1252],
        version: "0.2.1",
        Author: "Linoa235",
        note: "New Moon Island CE Drawing Completed"
    )]
    public class NewMoonIslandCE
    {
        // =================================================================================
        // ============================= Field and State Variable Definitions =============================
        // =================================================================================

        // -------------------- General mechanic lock objects --------------------
        private readonly object _mechanicLock = new(); // General mechanic processing lock, prevents multi-threading conflicts
        private readonly object _surgeLock = new();    // Citadel Guardian energy orb mechanic dedicated lock
        private readonly object _trapLock = new();     // Mind Flayer trap mechanic dedicated lock

        // ==================== Evolved Garula ====================
        private ulong _bossId; // Boss entity ID
        private Vector3? _noiseComplaintArenaCenter; // Arena center point
        private bool _noiseComplaintCenterRecorded; // Whether arena center has been recorded
        private bool? _lightningIsCardinal; // Record lightning direction: true for cardinal directions (N/E/S/W), false for intercardinal directions (NE/SE/SW/NW)
        private readonly Queue<ulong> _activeBirds = new(); // Queue for storing marked bird IDs
        private int _activeMechanicId; // Current charge mechanic ID being processed

        // --- "Rushing Rumble Rampage" dedicated state variables ---
        private bool _isRampageSequenceRunning; // Whether the continuous charge sequence is in progress
        private int _rampageChargeIndex; // Current charge index in the continuous charge sequence
        private Vector3 _rampageNextChargeStartPos; // Next continuous charge start position
        // ==================== Deathclaw ====================
        private static readonly Vector3 DeathclawArenaCenter = new(681f, 74f, 534f);
        // ==================== Citadel Guardian ====================
        private readonly List<ulong> _spheres = new(12);       // List of all unclassified energy orb IDs
        private readonly List<ulong> _spheresStone = new(6);   // List of "Stone" attribute energy orb IDs
        private readonly List<ulong> _spheresWind = new(6);    // List of "Wind" attribute energy orb IDs
        private readonly Dictionary<ulong, string> _surgeAoes = new(); // Stores drawn energy orb AOEs (ActorID -> DrawName), for later removal
        private bool _isHolyCasting; // Whether the boss is casting Holy

        // ==================== Mind Flayer ====================
        private readonly List<FireIceTrapInfo> _fireIceTraps = new(); // Stores all current fire-ice trap information on the field
        private readonly Dictionary<ulong, bool> _playerElements = new(); // Stores the element debuff currently carried by players (Key: player ID, Value: true for fire, false for ice)

        /// <summary>
        /// Mind Flayer fire-ice trap information carrier
        /// </summary>
        private class FireIceTrapInfo
        {
            public ulong NpcId { get; init; }
            public Vector3 Position { get; set; }
            public bool IsFire { get; init; }
        }

        // ==================== Nimbletooth Shark ====================
        private static readonly Vector3 SharkArenaCenter = new(-117f, 1, -850f); // Shark combat arena center
        private readonly List<(string Name, int Delay)> _tidalGuillotineAoes = new(3); // Stores "Tidal Guillotine" series AOE information

        // ==================== Leap Lion ====================
        private static readonly Vector3 OnTheHuntAreaCenter = new Vector3(636f, 108f, -54f); // Leap Lion combat arena center

        // Evolved Garula constants
        private const float GARULA_ARENA_RADIUS = 23f;
        private const int GARULA_CHARGE_WIDTH = 8;
        private const int GARULA_CIRCLE_RADIUS = 30;
        private const int GARULA_FAN_RADIUS = 70;
        private const int ACTION_RUSHING_RUMBLE = 41175;
        private const int ACTION_BIRDSERK_RUSH = 41176;
        private const int ACTION_RAMPAGE = 41177;

        public void Init(ScriptAccessory accessory)
        {
            accessory.Log.Debug("New Moon Island CE script loaded.");
            const string DrawPrefix = "CrescentIslandCE_";
            accessory.Method.RemoveDraw($"{DrawPrefix}.*"); // Clear all old drawings
            // TODO: Consider using DrawPrefix for all draw names in future updates to avoid conflicts with other scripts

            // Initialize all mechanic state variables
            _lightningIsCardinal = null;
            _activeBirds.Clear();
            _tidalGuillotineAoes.Clear();

            // Reset Evolved Garula mechanic state
            _noiseComplaintCenterRecorded = false;
            _noiseComplaintArenaCenter = null;
            ResetState();

            // Reset Citadel Guardian mechanic state
            ResetWindStoneLightSurgeState();

            // Reset Mind Flayer mechanic state
            lock (_trapLock)
            {
                _fireIceTraps.Clear();
                _playerElements.Clear();
            }
        }

        // --- Black Regiment ---


        [ScriptMethod(
            name: "Chocobo Attack (Black Regiment)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41163"]
        )]
        public void ChocoBeak(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "BlackRegiment_ChocoBeak_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(4, 70);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.ByTime;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }


        [ScriptMethod(
            name: "Tail Feather (Black Regiment)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41164"]
        )]
        public void ChocoMaelfeather(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "BlackRegiment_ChocoMaelfeather_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(8);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(
            name: "Chocobo Storm (Black Regiment)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41147"]
        )]
        public void ChocoWindstorm(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "BlackRegiment_ChocoWindstorm_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(16);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7000;
            dp.ScaleMode |= ScaleMode.ByTime;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(
            name: "Chocobo Cyclone (Black Regiment)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41148"]
        )]
        public void ChocoCyclone(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "BlackRegiment_ChocoCyclone_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(30, 30);
            dp.InnerScale = new Vector2(8, 8);
            dp.Radian = MathF.PI * 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }

        [ScriptMethod(
            name: "Chocobo Slaughter (Black Regiment)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41151"]
        )]
        public void ChocoSlaughterFirst(Event @event, ScriptAccessory accessory)
        {
            if (!TryDeserializeVector3(@event["SourcePosition"], out var spos)) return;
            if (!TryDeserializeFloat(@event["SourceRotation"], out var srot)) return;

            // Cache sin/cos outside the loop for performance
            var sinRot = (float)Math.Sin(srot);
            var cosRot = (float)Math.Cos(srot);

            var positions = new List<Vector3>();
            var currentPos = spos;
            for (int i = 0; i < 5; i++)
            {
                currentPos = new Vector3(
                    currentPos.X + sinRot * 5,
                    currentPos.Y,
                    currentPos.Z + cosRot * 5
                );
                positions.Add(currentPos);
            }

            const int firstExplosionTime = 5000;
            const int subsequentInterval = 1100;
            const int warningDuration = 2000;
            const int lingerDuration = 500;

            for (int i = 0; i < positions.Count; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"BlackRegiment_ChocoSlaughter_Danger_Zone_{i}";
                dp.Position = positions[i];
                dp.Scale = new Vector2(5);
                dp.Color = accessory.Data.DefaultDangerColor;

                int explosionTime = firstExplosionTime + ((i + 1) * subsequentInterval);

                dp.Delay = explosionTime - warningDuration;
                dp.DestoryAt = warningDuration + lingerDuration;
                dp.ScaleMode |= ScaleMode.ByTime;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }

        [ScriptMethod(
            name: "Magic Ray (Mystery Clay Doll)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41137"]
        )]
        public void MysticHeat(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "FromTimesBygone_MysticHeat_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(40);
            dp.Radian = DegToRad(60);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.ByTime;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(
            name: "Big Burst (Mystery Clay Doll)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41130"]
        )]
        public void BigBurst(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "FromTimesBygone_BigBurst_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(26);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 8000;
            dp.ScaleMode |= ScaleMode.ByTime;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }


        [ScriptMethod(
            name: "Death Ray (Mystery Clay Doll)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41133"]
        )]
        public void DeathRay(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = "FromTimesBygone_DeathRay_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(60);
            dp.Radian = DegToRad(90);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 8000;
            dp.ScaleMode |= ScaleMode.ByTime;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(
            name: "Flying Steelstrike (Mystery Clay Doll)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41131"]
        )]
        public void Steelstrike(Event @event, ScriptAccessory accessory)
        {
            // Loop 4 times, rotate 45 degrees each time, draw 4 straight lines forming a cross
            for (int i = 0; i < 4; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"FromTimesBygone_Steelstrike_Danger_Zone_{i}";
                dp.Owner = @event.SourceId;
                dp.Scale = new Vector2(10, 100);
                dp.Rotation = i * (MathF.PI / 4);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 8000;
                dp.ScaleMode |= ScaleMode.ByTime;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
            }
        }

        [ScriptMethod(
            name: "Arcane Orb (Mystery Clay Doll)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41135"]
        )]
        public void ArcaneOrbTelegraph(Event @event, ScriptAccessory accessory)
        {
            if (!TryDeserializeVector3(@event["EffectPosition"], out var pos)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();

            dp.Name = $"FromTimesBygone_ArcaneOrb_Danger_Zone_{pos.X}_{pos.Z}";
            dp.Position = pos;
            dp.Scale = new Vector2(6);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 4200;
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.ByTime;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        // --- Command Pot ---


        [ScriptMethod(
            name: "Command - Circle (Target) (Command Pot)",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:012F"]
        )]
        public void RockSlideStoneSwell_CircleTarget(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"Command (Tether 012F) triggered: Drawing circle AOE, target: {@event.TargetId}");
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"ExtremePrejudice_Circle_{@event.TargetId}";
            dp.Owner = @event.TargetId;
            dp.Scale = new Vector2(16);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6100;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(
            name: "Command - Cross (Target) (Command Pot)",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:0130"]
        )]
        public void RockSlideStoneSwell_Cross(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"Command (Tether 0130) triggered: Drawing cross AOE, target: {@event.TargetId}");
            // Draw first straight line
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"ExtremePrejudice_Cross1_{@event.TargetId}";
            dp1.Owner = @event.TargetId;
            dp1.Scale = new Vector2(10, 80);
            dp1.Color = accessory.Data.DefaultDangerColor;
            dp1.DestoryAt = 6100;
            dp1.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1);

            // Draw second vertical straight line
            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = $"ExtremePrejudice_Cross2_{@event.TargetId}";
            dp2.Owner = @event.TargetId;
            dp2.Scale = new Vector2(10, 80);
            dp2.Rotation = MathF.PI / 2;
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.DestoryAt = 6100;
            dp2.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp2);
        }

        [ScriptMethod(
            name: "Command - Circle (Source) (Command Pot)",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:0132"]
        )]
        public void RockSlideStoneSwell_CircleSource(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"Command (Tether 0132) triggered: Drawing circle AOE, source: {@event.SourceId}");
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"ExtremePrejudice_Circle_{@event.SourceId}";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(16);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6100;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }


        [ScriptMethod(
            name: "Command - Drawing Removal",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:regex:^(41420|39470|41421|39471)$"],
            userControl: false
        )]
        public void RemoveRockSlideStoneSwell(Event @event, ScriptAccessory accessory)
        {
            uint actionId = @event.ActionId;
            if (actionId == 41421 || actionId == 39471)
            {
                accessory.Method.RemoveDraw($"ExtremePrejudice_Cross1_{@event.SourceId}");
                accessory.Method.RemoveDraw($"ExtremePrejudice_Cross2_{@event.SourceId}");
            }
            else // StoneSwell (Circle)
            {
                accessory.Method.RemoveDraw($"ExtremePrejudice_Circle_{@event.SourceId}");
            }
        }
        #region Evolved Garula
        // --- Evolved Garula ---


        [ScriptMethod(
            name: "Arena Center Record (Evolved Garula)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41188"],
            userControl: false
        )]
        public void AgitatedGroanVisual(Event @event, ScriptAccessory accessory)
        {
            if (!_noiseComplaintCenterRecorded)
            {
                if (TryDeserializeVector3(@event["SourcePosition"], out var center))
                {
                    _noiseComplaintArenaCenter = center;
                    _noiseComplaintCenterRecorded = true;
                    accessory.Log.Debug($"Evolved Garula Arena Center recorded at: {_noiseComplaintArenaCenter}");
                }
            }
        }

        [ScriptMethod(
            name: "Circular Lightning (Evolved Garula)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41186"]
        )]
        public void EpicenterShock(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "NoiseComplaint_EpicenterShock_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(12);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(
            name: "Mammoth Bolt (Evolved Garula)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41187"]
        )]
        public void MammothBolt(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "NoiseComplaint_MammothBolt_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(25);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(
            name: "Fan-shaped Lightning (Evolved Garula)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41184"]
        )]
        public void LightningCrossing(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "NoiseComplaint_LightningCrossing_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(70);
            dp.Radian = DegToRad(45);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 4000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(
            name: "Heave (Evolved Garula)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(43262|41180)$"]
        )]
        public void Heave(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"NoiseComplaint_Heave_Danger_Zone_{@event.ActionId}";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(60);
            dp.Radian = DegToRad(120);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = (@event.ActionId == 43262) ? 4000 : 2000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(
            name: "Charge - Direction Record (Evolved Garula)",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:2193"],
            userControl: false
        )]
        public void RushingRumbleRampage_Status(Event @event, ScriptAccessory accessory)
        {
            var boss = accessory.Data.Objects.SearchById(@event.TargetId);
            if (boss == null || boss.DataId != 18078) return;
            // Extra == "0x350" is Cardinal, "0x351" is Intercardinal
            _lightningIsCardinal = @event["Param"] == "848";
            TryDrawMechanics(accessory);
        }
        [ScriptMethod(
            name: "Charge - Bird Record (Evolved Garula)",
            eventType: EventTypeEnum.TargetIcon,
            eventCondition: ["Id:0242"],
            userControl: false
        )]
        public void RushingRumbleRampage_Icon(Event @event, ScriptAccessory accessory)
        {
            // If the continuous charge sequence is running, this icon triggers the next charge.
            if (_isRampageSequenceRunning)
            {
                accessory.Log.Debug($"Detected continuous charge bird mark. Charge index: {_rampageChargeIndex}.");
                var bird = accessory.Data.Objects.SearchById(@event.TargetId);
                if (bird == null)
                {
                    accessory.Log.Error($"Unable to find bird for continuous charge {_rampageChargeIndex} {@event.TargetId}.");
                    ResetState(); // Safe reset
                    return;
                }

                // Draw this segment of the continuous charge, starting point is the previous landing point
                DrawRampageCharge(accessory, _rampageNextChargeStartPos, bird, _rampageChargeIndex);

                // If all charges are complete (usually 3 times), reset state.
                if (_rampageChargeIndex >= 3)
                {
                    accessory.Log.Debug("Continuous charge sequence complete.");
                    ResetState();
                }
            }
            // For other mechanics, enqueue the bird and try to draw.
            else
            {
                _activeBirds.Enqueue(@event.TargetId);
                accessory.Log.Debug($"Bird ID: {@event.TargetId} enqueued for non-continuous charge mechanic. Queue size: {_activeBirds.Count}");
                TryDrawMechanics(accessory);
            }
        }

        [ScriptMethod(
            name: "Rushing Rumble Start (Evolved Garula)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41175"]
        )]
        public void OnRushingRumbleStart(Event @event, ScriptAccessory accessory)
        {
            _activeMechanicId = ACTION_RUSHING_RUMBLE;
            _bossId = @event.SourceId;
            TryDrawMechanics(accessory);
        }



        [ScriptMethod(
            name: "Birdserk Rush Start (Evolved Garula)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41176"]
        )]
        public void OnBirdserkRushStart(Event @event, ScriptAccessory accessory)
        {
            _activeMechanicId = ACTION_BIRDSERK_RUSH;
            _bossId = @event.SourceId;
            TryDrawMechanics(accessory);
        }

        [ScriptMethod(
            name: "Rampage Start (Evolved Garula)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41177"]
        )]
        public void OnRampageStart(Event @event, ScriptAccessory accessory)
        {
            _activeMechanicId = ACTION_RAMPAGE;
            _bossId = @event.SourceId;
            TryDrawMechanics(accessory);
        }

        // --- Evolved Garula Helper Methods ---

        private void TryDrawMechanics(ScriptAccessory accessory)
        {
            // If a continuous charge has already started (i.e., not the first charge), it's handled by the icon event, return here
            if (_isRampageSequenceRunning) return;
            // If there is no active mechanic ID, also return
            if (_activeMechanicId == 0) return;

            switch (_activeMechanicId)
            {
                // RushingRumble (single charge): needs direction and bird
                case ACTION_RUSHING_RUMBLE when _lightningIsCardinal != null && _activeBirds.Count > 0:
                    accessory.Log.Debug("Conditions met, drawing RushingRumble.");
                    DrawRushingRumble(accessory);
                    ResetState();
                    break;

                // BirdserkRush (Bird charge): only needs bird
                case ACTION_BIRDSERK_RUSH when _activeBirds.Count > 0:
                    accessory.Log.Debug("Conditions met, drawing BirdserkRush.");
                    DrawBirdserkRush(accessory);
                    ResetState();
                    break;

                // Rushing Rumble Rampage (continuous charge) start: needs direction and bird
                case ACTION_RAMPAGE when _lightningIsCardinal != null && _activeBirds.Count > 0:
                    accessory.Log.Debug("Conditions met, starting Rushing Rumble Rampage sequence.");
                    var boss = accessory.Data.Objects.SearchById(_bossId);
                    if (boss == null) { ResetState(); return; }

                    // Set continuous charge state
                    _isRampageSequenceRunning = true;
                    _rampageChargeIndex = 0;
                    _rampageNextChargeStartPos = boss.Position;

                    // Draw the first charge
                    if (_activeBirds.TryDequeue(out var birdId))
                    {
                        var bird = accessory.Data.Objects.SearchById(birdId);
                        if (bird != null)
                        {
                            DrawRampageCharge(accessory, _rampageNextChargeStartPos, bird, _rampageChargeIndex);
                        }
                    }
                    // Note: ResetState() is not called here because the sequence has just started
                    break;
            }
        }

        private void ResetState()
        {
            _lightningIsCardinal = null;
            _activeBirds.Clear();
            _bossId = 0;
            _activeMechanicId = 0;
            _isRampageSequenceRunning = false;
            _rampageChargeIndex = 0;
        }

        /// <summary>
        /// Draw Evolved Garula's four-directional fan-shaped AOEs
        /// </summary>
        private void DrawGarulaFourFans(ScriptAccessory accessory, Vector3 position, Vector3 directionFrom, ulong uniqueId, int chargeIndex = -1)
        {
            var initialAngle = MathF.Atan2(directionFrom.X, directionFrom.Z);
            if (_lightningIsCardinal == false) // Adjust for intercardinal direction
            {
                initialAngle += DegToRad(45);
            }

            for (int i = 0; i < 4; i++)
            {
                var dpCone = accessory.Data.GetDefaultDrawProperties();
                string nameSuffix = chargeIndex >= 0 ? $"_{chargeIndex}_{i}" : $"_{i}";
                dpCone.Name = $"NoiseComplaint_Cone_{uniqueId}{nameSuffix}";
                dpCone.Position = position;
                dpCone.Scale = new Vector2(GARULA_FAN_RADIUS);
                dpCone.Radian = DegToRad(45);
                dpCone.Rotation = initialAngle + (i * DegToRad(90));
                dpCone.Color = accessory.Data.DefaultDangerColor;
                dpCone.Delay = 0;
                dpCone.DestoryAt = 10400;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dpCone);
            }
        }

        /// <summary>
        /// Draw a combination of charge line + landing circle
        /// </summary>
        private void DrawGarulaChargeWithCircle(ScriptAccessory accessory, Vector3 startPos, Vector3 endPos, string namePrefix, ulong? ownerId = null, ulong? targetId = null)
        {
            // Charge line
            var dpCharge = accessory.Data.GetDefaultDrawProperties();
            dpCharge.Name = $"{namePrefix}_Charge";
            dpCharge.Scale = new Vector2(GARULA_CHARGE_WIDTH, 100);
            dpCharge.ScaleMode |= ScaleMode.YByDistance;
            dpCharge.Color = accessory.Data.DefaultDangerColor;
            dpCharge.DestoryAt = 6300;

            if (ownerId.HasValue)
            {
                dpCharge.Owner = ownerId.Value;
                if (targetId.HasValue)
                    dpCharge.TargetObject = targetId.Value;
                else
                    dpCharge.TargetPosition = endPos;
            }
            else
            {
                dpCharge.Position = startPos;
                dpCharge.TargetPosition = endPos;
            }
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dpCharge);

            // Landing large circle
            var dpCircle = accessory.Data.GetDefaultDrawProperties();
            dpCircle.Name = $"{namePrefix}_Circle";
            dpCircle.Position = endPos;
            dpCircle.Scale = new Vector2(GARULA_CIRCLE_RADIUS);
            dpCircle.Color = accessory.Data.DefaultDangerColor;
            dpCircle.DestoryAt = 10400;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dpCircle);
        }

        private Vector3 GetArenaEdgePosition(Vector3 destination)
        {
            if (_noiseComplaintArenaCenter == null) return destination;

            var vectorFromCenter = destination - _noiseComplaintArenaCenter.Value;
            if (vectorFromCenter.LengthSquared() <= GARULA_ARENA_RADIUS * GARULA_ARENA_RADIUS)
            {
                return destination;
            }
            else
            {
                var direction = Vector3.Normalize(vectorFromCenter);
                return _noiseComplaintArenaCenter.Value + direction * GARULA_ARENA_RADIUS;
            }
        }

        private Vector3 GetLineArenaIntersection(Vector3 start, Vector3 end, ScriptAccessory accessory)
        {
            if (_noiseComplaintArenaCenter == null) return GetArenaEdgePosition(end);

            Vector2 center = new Vector2(_noiseComplaintArenaCenter.Value.X, _noiseComplaintArenaCenter.Value.Z);
            Vector2 p1 = new Vector2(start.X, start.Z);
            Vector2 p2 = new Vector2(end.X, end.Z);

            if (Vector2.DistanceSquared(p2, center) <= GARULA_ARENA_RADIUS * GARULA_ARENA_RADIUS)
            {
                return end;
            }

            Vector2 d = p2 - p1;
            Vector2 f = p1 - center;

            float a = Vector2.Dot(d, d);
            float b = 2 * Vector2.Dot(f, d);
            float c = Vector2.Dot(f, f) - GARULA_ARENA_RADIUS * GARULA_ARENA_RADIUS;

            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                return GetArenaEdgePosition(end);
            }

            float sqrtDiscriminant = MathF.Sqrt(discriminant);
            float t1 = (-b - sqrtDiscriminant) / (2 * a);
            float t2 = (-b + sqrtDiscriminant) / (2 * a);

            var validIntersections = new List<Vector3>();

            if (t1 >= 0 && t1 <= 1)
            {
                Vector2 intersection2D = p1 + t1 * d;
                var point = new Vector3(intersection2D.X, start.Y, intersection2D.Y);
                validIntersections.Add(point);
            }
            if (t2 >= 0 && t2 <= 1)
            {
                Vector2 intersection2D = p1 + t2 * d;
                var point = new Vector3(intersection2D.X, start.Y, intersection2D.Y);
                validIntersections.Add(point);
            }

            if (validIntersections.Count == 0)
            {
                return GetArenaEdgePosition(end);
            }
            if (validIntersections.Count == 1)
            {
                return validIntersections[0];
            }

            var dist1 = Vector3.DistanceSquared(validIntersections[0], end);
            var dist2 = Vector3.DistanceSquared(validIntersections[1], end);

            return dist1 <= dist2 ? validIntersections[0] : validIntersections[1];
        }

        private void DrawRushingRumble(ScriptAccessory accessory)
        {
            if (!_activeBirds.TryDequeue(out var birdId)) return;
            var bird = accessory.Data.Objects.SearchById(birdId);
            var boss = accessory.Data.Objects.SearchById(_bossId);
            if (bird == null || boss == null) return;

            var destination = GetArenaEdgePosition(bird.Position);

            // Draw charge + landing circle
            DrawGarulaChargeWithCircle(accessory, boss.Position, destination, $"NoiseComplaint_Rumble_{bird.EntityId}", _bossId);

            // Draw four-directional fans
            var dirToBoss = boss.Position - destination;
            DrawGarulaFourFans(accessory, destination, dirToBoss, bird.EntityId);
        }

        private void DrawBirdserkRush(ScriptAccessory accessory)
        {
            if (!_activeBirds.TryDequeue(out var birdId)) return;
            var bird = accessory.Data.Objects.SearchById(birdId);
            if (bird == null) return;

            // Straight line charge from boss to bird (use helper method to draw charge part, no landing circle)
            var dpCharge = accessory.Data.GetDefaultDrawProperties();
            dpCharge.Name = $"NoiseComplaint_BirdserkRush_Charge_{_bossId}";
            dpCharge.Owner = _bossId;
            dpCharge.TargetObject = bird.EntityId;
            dpCharge.Scale = new Vector2(GARULA_CHARGE_WIDTH, 100);
            dpCharge.ScaleMode |= ScaleMode.YByDistance;
            dpCharge.Color = accessory.Data.DefaultDangerColor;
            dpCharge.DestoryAt = 6300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dpCharge);

            // Large cone cleave from boss toward bird
            var dpCone = accessory.Data.GetDefaultDrawProperties();
            dpCone.Name = $"NoiseComplaint_BirdserkRush_Cone_{_bossId}";
            dpCone.Owner = _bossId;
            dpCone.TargetObject = bird.EntityId;
            dpCone.Scale = new Vector2(60);
            dpCone.Radian = DegToRad(120);
            dpCone.Color = accessory.Data.DefaultDangerColor;
            dpCone.DestoryAt = 11000;
            dpCone.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dpCone);
        }

        private void DrawRampageCharge(ScriptAccessory accessory, Vector3 chargeStartPos, IGameObject bird, int chargeIndex)
        {
            if (_lightningIsCardinal == null)
            {
                accessory.Log.Error("Unable to draw continuous charge, lightning direction unknown.");
                return;
            }

            // Calculate target position: all charges end at the intersection of the line from bird to arena center and the arena edge
            var destination = GetArenaEdgePosition(bird.Position);

            // Draw charge + landing circle
            DrawGarulaChargeWithCircle(accessory, chargeStartPos, destination, $"NoiseComplaint_Rampage_{chargeIndex}");

            // Draw four-directional fans
            var chargeDirectionVector = destination - chargeStartPos;
            DrawGarulaFourFans(accessory, destination, chargeDirectionVector, bird.EntityId, chargeIndex);

            _rampageNextChargeStartPos = destination;
            _rampageChargeIndex++;
        }

        #endregion

        #region Deathclaw
        [ScriptMethod(
            name: "Lethal Nails (Deathclaw)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(41315|41316|41317)$"]
        )]

        public void LethalNails(Event @event, ScriptAccessory accessory)
        {
            var nailId = @event.ActionId;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Deathclaw_LethalNails_{nailId}";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(7, 60);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.ScaleMode |= ScaleMode.ByTime;
            switch (nailId)
            {
                case 41315: // First type
                    dp.DestoryAt = 2000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                    break;
                case 41316: // Second type
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                    break;
                case 41317: // Third type
                    dp.Delay = 2000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                    break;
            }
        }

        [ScriptMethod(
            name: "Vertical Crosshatch (Deathclaw)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41323"]
        )]
        public void VerticalCrosshatch(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"Vertical Crosshatch triggered. ActionId: {@event.ActionId}, SourceId: {@event.SourceId}");
            const int frontBackDelay = 0;
            const int leftRightDelay = 2500;
            const int duration = 5000;

            float[] rotations = { 0, MathF.PI, MathF.PI / 2, -MathF.PI / 2 };
            int[] delays = { frontBackDelay, frontBackDelay, leftRightDelay, leftRightDelay };
            string[] names = { "Front", "Back", "Right", "Left" };

            for (int i = 0; i < 4; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Deathclaw_Crosshatch_{names[i]}_{@event.SourceId}";
                dp.Owner = @event.SourceId;
                dp.Scale = new Vector2(50);
                dp.Radian = DegToRad(90);
                dp.Rotation = rotations[i];
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = delays[i];
                dp.DestoryAt = duration;
                dp.ScaleMode |= ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                accessory.Log.Debug($"  Drawing fan: {dp.Name}, Rotation: {dp.Rotation}, Delay: {dp.Delay}ms");
            }
        }
        [ScriptMethod(
            name: "Long Vertical Crosshatch (Deathclaw)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41330"]
        )]
        public void VerticalCrosslonghatch(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"Vertical Crosshatch triggered. ActionId: {@event.ActionId}, SourceId: {@event.SourceId}");
            const int frontBackDelay = 0;
            const int leftRightDelay = 2500;
            const int duration = 7500;

            float[] rotations = { 0, MathF.PI, MathF.PI / 2, -MathF.PI / 2 };
            int[] delays = { frontBackDelay, frontBackDelay, leftRightDelay, leftRightDelay };
            string[] names = { "Front", "Back", "Right", "Left" };

            for (int i = 0; i < 4; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Deathclaw_Crosshatch_{names[i]}_{@event.SourceId}";
                dp.Owner = @event.SourceId;
                dp.Scale = new Vector2(50);
                dp.Radian = DegToRad(90);
                dp.Rotation = rotations[i];
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = delays[i];
                dp.DestoryAt = duration;
                dp.ScaleMode |= ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                accessory.Log.Debug($"  Drawing fan: {dp.Name}, Rotation: {dp.Rotation}, Delay: {dp.Delay}ms");
            }
        }

        [ScriptMethod(
            name: "Horizontal Crosshatch (Deathclaw)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41324"]
        )]
        public void HorizontalCrosshatch(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"Horizontal Crosshatch triggered. ActionId: {@event.ActionId}, SourceId: {@event.SourceId}");
            const int frontBackDelay = 2500;
            const int leftRightDelay = 0;
            const int duration = 5000;

            float[] rotations = { 0, MathF.PI, MathF.PI / 2, -MathF.PI / 2 };
            int[] delays = { frontBackDelay, frontBackDelay, leftRightDelay, leftRightDelay };
            string[] names = { "Front", "Back", "Right", "Left" };

            for (int i = 0; i < 4; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Deathclaw_Crosshatch_{names[i]}_{@event.SourceId}";
                dp.Owner = @event.SourceId;
                dp.Scale = new Vector2(50);
                dp.Radian = DegToRad(90);
                dp.Rotation = rotations[i];
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = delays[i];
                dp.DestoryAt = duration;
                dp.ScaleMode |= ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                accessory.Log.Debug($"  Drawing fan: {dp.Name}, Rotation: {dp.Rotation}, Delay: {dp.Delay}ms");
            }
        }
        [ScriptMethod(
            name: "Long Horizontal Crosshatch (Deathclaw)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41331"]
        )]
        public void HorizontalCrosslonghatch(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"Horizontal Crosshatch triggered. ActionId: {@event.ActionId}, SourceId: {@event.SourceId}");
            const int frontBackDelay = 2500;
            const int leftRightDelay = 0;
            const int duration = 7500;

            float[] rotations = { 0, MathF.PI, MathF.PI / 2, -MathF.PI / 2 };
            int[] delays = { frontBackDelay, frontBackDelay, leftRightDelay, leftRightDelay };
            string[] names = { "Front", "Back", "Right", "Left" };

            for (int i = 0; i < 4; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Deathclaw_Crosshatch_{names[i]}_{@event.SourceId}";
                dp.Owner = @event.SourceId;
                dp.Scale = new Vector2(50);
                dp.Radian = DegToRad(90);
                dp.Rotation = rotations[i];
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = delays[i];
                dp.DestoryAt = duration;
                dp.ScaleMode |= ScaleMode.ByTime;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                accessory.Log.Debug($"  Drawing fan: {dp.Name}, Rotation: {dp.Rotation}, Delay: {dp.Delay}ms");
            }
        }
        [ScriptMethod(
            name: "Lethal Claw (Deathclaw)",
            eventType: EventTypeEnum.ObjectEffect,
            eventCondition: ["Id1:16", "Id2:32"]
        )]
        public void LethalClaw(Event @event, ScriptAccessory accessory)
        {
            // Get the unit that released the mechanic
            var source = accessory.Data.Objects.SearchById(@event.SourceId);
            // Check if the releasing unit is within 21m of the arena
            if (source == null || Vector3.Distance(source.Position, DeathclawArenaCenter) > 21f)
            {
                accessory.Log.Debug("Releasing unit not within Deathclaw arena range, ignoring Lethal Claw mechanic.");
                return;
            }
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Deathclaw_LethalClaw_{@event.SourceId}";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(50);
            dp.Radian = DegToRad(90);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            accessory.Log.Debug($"Drawing Deathclaw Lethal Claw AOE: {@event.SourceId}");
        }

        // --- Deathclaw Helper Methods ---

        private void DrawCrossAOE(ScriptAccessory accessory, string baseName, Vector3 position, float rotation, int delay, int duration)
        {
            // Horizontal part
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = $"{baseName}_1";
            dp1.Position = position;
            dp1.Rotation = rotation;
            dp1.Scale = new Vector2(15, 200); // Width 15, length 200
            dp1.Color = accessory.Data.DefaultDangerColor;
            dp1.Delay = delay;
            dp1.DestoryAt = duration;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1);

            // Vertical part
            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = $"{baseName}_2";
            dp2.Position = position;
            dp2.Rotation = rotation + MathF.PI / 2; // Rotate 90 degrees
            dp2.Scale = new Vector2(15, 100);
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.Delay = delay;
            dp2.DestoryAt = duration;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp2);
        }

        #endregion

        #region Crystal Dragon
        [ScriptMethod(
            name: "Prismatic Wing (Circle/Donut) (Crystal Dragon)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(42766|42767|42768|42769)$"]
        )]
        public void PrismaticWing(Event @event, ScriptAccessory accessory)
        {
            var AID = @event.ActionId;
            switch (AID)
            {
                case 42766: // Circle
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"CrystalDragon_PrismaticWing_{AID}";
                        dp.Owner = @event.SourceId;
                        dp.Scale = new Vector2(22, 22);
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.DestoryAt = 7000;
                        dp.ScaleMode |= ScaleMode.ByTime;
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                        break;
                    }
                case 42768:
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"CrystalDragon_PrismaticWing_{AID}";
                        dp.Owner = @event.SourceId;
                        dp.Scale = new Vector2(22, 22);
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.DestoryAt = 4500;
                        dp.ScaleMode |= ScaleMode.ByTime;
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                        break;
                    }
                case 42767: // Donut
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"CrystalDragon_PrismaticWing_{AID}";
                        dp.Owner = @event.SourceId;
                        dp.Scale = new Vector2(31, 31);
                        dp.InnerScale = new Vector2(5, 5);
                        dp.Radian = DegToRad(360);
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.DestoryAt = 7000;
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                        break;
                    }
                case 42769: // Donut
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"CrystalDragon_PrismaticWing_{AID}";
                        dp.Owner = @event.SourceId;
                        dp.Scale = new Vector2(31, 31);
                        dp.InnerScale = new Vector2(5, 5);
                        dp.Radian = DegToRad(360);
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.DestoryAt = 4500;
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                        break;
                    }
            }
        }
        [ScriptMethod(
            name: "Crystallized Energy and Chaos (Crystal Dragon)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(42728|42729|42730|42731|42732|42733|42734|42735|41758|41759|41760|41761)$"]
        )]
        public void CrystallizedEnergyAndChaos(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"CrawlingDeath_Crystallized_{@event.ActionId}";
            dp.Owner = @event.SourceId;
            dp.Color = accessory.Data.DefaultDangerColor;

            switch (@event.ActionId)
            {

                case 42728:
                    dp.Scale = new Vector2(7, 7);
                    dp.Delay = 3000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    break;
                case 42729:
                    dp.Scale = new Vector2(13, 13);
                    dp.InnerScale = new Vector2(7, 7);
                    dp.Radian = DegToRad(360);
                    dp.Delay = 3000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                    break;
                case 42730:
                    dp.Scale = new Vector2(19, 19);
                    dp.InnerScale = new Vector2(13, 13);
                    dp.Radian = DegToRad(360);
                    dp.Delay = 3000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                    break;
                case 42731:
                    dp.Scale = new Vector2(25, 25);
                    dp.InnerScale = new Vector2(19, 19);
                    dp.Radian = DegToRad(360);
                    dp.Delay = 3000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                    break;


                case 42732:
                    dp.Scale = new Vector2(7, 7);
                    dp.Delay = 6000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    break;
                case 42733:
                    dp.Scale = new Vector2(13, 13);
                    dp.InnerScale = new Vector2(7, 7);
                    dp.Radian = DegToRad(360);
                    dp.Delay = 6000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                    break;
                case 42734:
                    dp.Scale = new Vector2(19, 19);
                    dp.InnerScale = new Vector2(13, 13);
                    dp.Radian = DegToRad(360);
                    dp.Delay = 6000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                    break;
                case 42735:
                    dp.Scale = new Vector2(25, 25);
                    dp.InnerScale = new Vector2(19, 19);
                    dp.Radian = DegToRad(360);
                    dp.Delay = 6000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                    break;


                case 41758:
                    dp.Scale = new Vector2(7, 7);
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    break;
                case 41759:
                    dp.Scale = new Vector2(13, 13);
                    dp.InnerScale = new Vector2(7, 7);
                    dp.Radian = DegToRad(360);
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                    break;
                case 41760:
                    dp.Scale = new Vector2(19, 19);
                    dp.InnerScale = new Vector2(13, 13);
                    dp.Radian = DegToRad(360);
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                    break;
                case 41761:
                    dp.Scale = new Vector2(25, 25);
                    dp.InnerScale = new Vector2(19, 19);
                    dp.Radian = DegToRad(360);
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                    break;
            }
        }

        #endregion

        #region New Moon Berserker
        [ScriptMethod(
           name: "Scathing Sweep (New Moon Berserker)",
           eventType: EventTypeEnum.StartCasting,
           eventCondition: ["ActionId:42691"]
       )]

        public void ScathingSweep(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "NewMoonBerserker_Sweep_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(60, 60);
            dp.Color = new Vector4(1f, 0f, 0f, 1f);
            dp.DestoryAt = 6000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(
            name: "Rage 1 (New Moon Berserker)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:37323"]
        )]
        public void NewMoonBerserkerRage1(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "NewMoonBerserker_Rage1_Danger_Zone";
            dp.Position = @event.TargetPosition;
            dp.Scale = new Vector2(8, 8);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 8000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = "NewMoonBerserker_BedrockUplift1_Danger_Zone";
            dp2.Position = @event.TargetPosition;
            dp2.Scale = new Vector2(60, 60);
            dp2.InnerScale = new Vector2(8, 8);
            dp2.Color = new Vector4(1f, 0f, 0f, 1f);
            dp2.Radian = DegToRad(360);
            dp2.Delay = 6500;
            dp2.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
        }
        [ScriptMethod(
            name: "Rage 2 (New Moon Berserker)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:30872"]
        )]
        public void NewMoonBerserkerRage2(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "NewMoonBerserker_Rage2_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(24, 24);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 9000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = "NewMoonBerserker_BedrockUplift2_Danger_Zone";
            dp2.Owner = @event.SourceId;
            dp2.Scale = new Vector2(60, 60);
            dp2.InnerScale = new Vector2(24, 24);
            dp2.Radian = DegToRad(360);
            dp2.Color = new Vector4(1f, 0f, 0f, 1f);
            dp2.Delay = 7500;
            dp2.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
        }
        [ScriptMethod(
            name: "Rage 3 (New Moon Berserker)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:30873"]
        )]
        public void NewMoonBerserkerRage3(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "NewMoonBerserker_Rage3_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(16, 16);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 9000;
            dp.DestoryAt = 6500;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = "NewMoonBerserker_BedrockUplift3_Danger_Zone";
            dp2.Owner = @event.SourceId;
            dp2.Scale = new Vector2(60, 60);
            dp2.InnerScale = new Vector2(16, 16);
            dp2.Radian = DegToRad(360);
            dp2.Color = new Vector4(1f, 0f, 0f, 1f);
            dp2.Delay = 14000;
            dp2.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
        }
        [ScriptMethod(
            name: "Rage 4 (New Moon Berserker)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:30874"]
        )]
        public void NewMoonBerserkerRage4(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "NewMoonBerserker_Rage4_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(8, 8);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 12000;
            dp.DestoryAt = 6500;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = "NewMoonBerserker_BedrockUplift4_Danger_Zone";
            dp2.Owner = @event.SourceId;
            dp2.Scale = new Vector2(60, 60);
            dp2.InnerScale = new Vector2(16, 16);
            dp2.Color = new Vector4(1f, 0f, 0f, 1f);
            dp2.Radian = DegToRad(360);
            dp2.Delay = 21000;
            dp2.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp2);
        }
        [ScriptMethod(
            name: "Fury (New Moon Berserker)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:37804"]
        )]
        public void NewMoonBerserkerFury(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "NewMoonBerserker_Fury_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(13, 13);
            dp.Color = new Vector4(1f, 0f, 0f, 1f);
            dp.DestoryAt = 6000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        #endregion
        #region Corridor Demon
        [ScriptMethod(
            name: "Explosion (Corridor Demon)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41357"]
        )]
        public void Explosion(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "CorridorDemon_Explosion_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(22, 22);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(
            name: "Tidal Breath (Corridor Demon)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41360"]
        )]
        public void TidalBreath(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "CorridorDemon_TidalBreath_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(40);
            dp.Radian = DegToRad(180);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        #endregion
        #region Knight Order
        [ScriptMethod(
            name: "Dualfist Flurry (Knight Order)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41828"]
        )]
        public void DualfistFlurry(Event @event, ScriptAccessory accessory)
        {
            var caster = accessory.Data.Objects.SearchById(@event.SourceId);
            if (caster == null)
            {
                accessory.Log.Error($"Unable to find Dualfist Flurry caster: {@event.SourceId}");
                return;
            }

            // Get initial position and forward direction
            if (!TryDeserializeVector3(@event["EffectPosition"], out var currentPos)) return;
            var direction = new Vector3(MathF.Sin(caster.Rotation), 0, MathF.Cos(caster.Rotation));

            // Skill parameters
            const int totalExplosions = 9;      
            const float radius = 6f;            
            const float stepDistance = 7f;      // Distance to advance each time
            const int firstExplosionTime = 10000;// First explosion time (based on 6 second cast)
            const int subsequentInterval = 1000;// Subsequent explosion interval
            const int warningDuration = 2000;   // Warning display time
            const int lingerDuration = 500;     // Post-explosion lingering time

            // Create drawings for all explosions in a loop
            for (int i = 0; i < totalExplosions; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"CompanyOfStone_DualfistFlurry_Danger_Zone_{i}";
                dp.Position = currentPos;
                dp.Scale = new Vector2(radius);
                dp.Color = accessory.Data.DefaultDangerColor;

                // Calculate the precise time for each explosion
                int explosionTime = firstExplosionTime + (i * subsequentInterval);

                // Set drawing delay and destruction time
                dp.Delay = explosionTime - warningDuration;
                dp.DestoryAt = warningDuration + lingerDuration;
                dp.ScaleMode |= ScaleMode.ByTime;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                // Calculate new position for the next loop
                currentPos += direction * stepDistance;
            }
        }
        [ScriptMethod(
             name: "Spinning Siege (Knight Order)",
             eventType: EventTypeEnum.StartCasting,
             eventCondition: ["ActionId:regex:^(41823|41822)$"]
         )]
        public void SpinningSiege(Event @event, ScriptAccessory accessory)
        {
            // Get caster object
            var caster = accessory.Data.Objects.SearchById(@event.SourceId);
            if (caster == null)
            {
                accessory.Log.Error($"Spinning Siege: Cannot find caster {@event.SourceId}");
                return;
            }

            // Record caster's initial facing
            float initialRotation = caster.Rotation;

            // 1. Determine rotation direction based on skill ID
            // 41822 = clockwise (CW), 41823 = counterclockwise (CCW)
            int rotationDirection = (@event.ActionId == 41822) ? -1 : 1;

            // 2. Define drawing parameters
            const float crossLength = 120f;
            const float crossWidth = 6f;
            const int rotationInterval = 1700;  // Rotation interval 1.7s
            const float rotationAngleDegrees = 9f; // Rotate 9 degrees each time
            float rotationAngleRad = DegToRad(rotationAngleDegrees);
            const int numberOfSteps = 6;

            // 3. Create a series of delayed and rotating cross AOEs in a loop
            for (int i = 0; i < numberOfSteps; i++)
            {
                int delay = 3000 + i * rotationInterval;
                int lifeSpan = 5000;
                if (lifeSpan <= 0) continue;

                // Calculate current rotation angle based on initial facing for this step
                float currentRotation = initialRotation + (i * rotationDirection * rotationAngleRad);

                // Draw the first straight line of the cross
                var dp1 = accessory.Data.GetDefaultDrawProperties();
                dp1.Name = $"SpinningSiege_{@event.ActionId}_Cross1_{i}";
                dp1.Position = @event.SourcePosition;
                dp1.Scale = new Vector2(crossWidth, crossLength);
                dp1.Rotation = currentRotation;
                dp1.Color = accessory.Data.DefaultDangerColor;
                dp1.Delay = delay;
                dp1.DestoryAt = lifeSpan;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1);

                // Draw the second (vertical) straight line of the cross
                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = $"SpinningSiege_{@event.ActionId}_Cross2_{i}";
                dp2.Position = @event.SourcePosition;
                dp2.Scale = new Vector2(crossWidth, crossLength);
                dp2.Rotation = currentRotation + (MathF.PI / 2);
                dp2.Color = accessory.Data.DefaultDangerColor;
                dp2.Delay = delay;
                dp2.DestoryAt = lifeSpan;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp2);
            }
        }


        #endregion
        #region Will-o'-the-Wisp
        [ScriptMethod(
            name: "Clone Mechanic (Will-o'-the-Wisp)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(41397|42033|42035)$"]
        )]
        
        public void CloneMechanicDraw(Event @event, ScriptAccessory accessory)
        {
            var ActionId = @event.ActionId;
            var dp = accessory.Data.GetDefaultDrawProperties();
            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"WilloWisp_CloneMechanic_{ActionId}";
            dp2.Name = $"WilloWisp_CloneMechanic_{ActionId}_2";
            switch (ActionId)
            {
                case 41397: // Knockback
                    dp.Scale = new Vector2(1.5f, 20f); // Line width 2, end circle radius 1
                    dp.Color = new(0.3f, 1.0f, 0f, 1.5f);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = @event.SourcePosition;
                    dp.Rotation = MathF.PI;
                    dp.DestoryAt = 3000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
                    accessory.Log.Debug($"Drawing knockback: Name={dp.Name}s");
                    break;
                case 42033: // Donut
                    dp.Owner = @event.SourceId;
                    dp.Scale = new Vector2(50);
                    dp.InnerScale = new Vector2(7);
                    dp.Radian = DegToRad(360);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 3000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                    accessory.Log.Debug($"Drawing donut AOE: Name={dp.Name}s");
                    break;
                case 42035: // Cross
                    dp.Owner = @event.SourceId;
                    dp.Scale = new Vector2(15, 200);
                    dp.Rotation = DegToRad(90);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 3000;
                    dp2.Owner = @event.SourceId;
                    dp2.Scale = new Vector2(15, 200);
                    dp2.Color = accessory.Data.DefaultDangerColor;
                    dp2.DestoryAt = 3000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp2);
                    accessory.Log.Debug($"Drawing cross AOE: Name={dp.Name}s");
                    accessory.Log.Debug($"Drawing cross AOE: Name={dp2.Name}s");
                    break;

            }
        }
        [ScriptMethod(
            name: "Boss Direct AOE (Will-o'-the-Wisp)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(42034|42032)$"]
        )]
        public void OnBossAOE(Event @event, ScriptAccessory accessory)
        {
            var caster = accessory.Data.Objects.SearchById(@event.SourceId);
            if (caster == null) return;

            if (@event.ActionId == 42034)
            {
                var baseName = "WilloWisp_Boss_Cross";
                DrawCrossAOE(accessory, baseName, caster.Position, caster.Rotation, 0, 5800);
                accessory.Log.Debug($"Drawing boss direct cross AOE: {baseName}");
            }
            else // ShadesNestBoss
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "WilloWisp_Boss_Donut";
                dp.Owner = @event.SourceId;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Radian = DegToRad(360);
                dp.DestoryAt = 5800;
                dp.Scale = new Vector2(50);
                dp.InnerScale = new Vector2(7);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                accessory.Log.Debug($"Drawing boss direct donut AOE: {dp.Name}");
            }
        }

        #endregion
        #region Nimbletooth Shark
        [ScriptMethod(
            name: "Hydrocleave Draw (Nimbletooth Shark)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:43149"]
        )]
        public void HydrocleaveDraw (Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "SharkAttack_Hydrocleave_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(50);
            dp.Radian = DegToRad(60); 
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        [ScriptMethod(
            name: "Tidal Guillotine (Nimbletooth Shark)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41723"]
        )]
        public void OnTidalGuillotineFirstCast(Event @event, ScriptAccessory accessory)
        {
            // Clear old list, start new sequence
            _tidalGuillotineAoes.Clear();

            if (!TryDeserializeVector3(@event["EffectPosition"], out var pos)) return;
            var castTime = 8000;
            var name = $"TidalGuillotine_AOE_0";

            // Record first AOE
            _tidalGuillotineAoes.Add((name, castTime));

            // Draw first AOE
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Position = pos;
            dp.Scale = new Vector2(20);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = castTime + 1000; // Destroy 1 second after cast ends
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            accessory.Log.Debug($"Recorded first Tidal Guillotine AOE, position: {pos}");
        }
        [ScriptMethod(
            name: "Tidal Guillotine - Record Subsequent AOE (Nimbletooth Shark)",
            eventType: EventTypeEnum.ActionEffect, 
            eventCondition: ["ActionId:41682"]
        )]
        public void OnTidalGuillotineTeleport(Event @event, ScriptAccessory accessory)
        {
            if (_tidalGuillotineAoes.Count >= 3) return; // Maximum 3

            if (!TryDeserializeVector3(@event["EffectPosition"], out var pos)) return;
            var index = _tidalGuillotineAoes.Count;
            var name = $"TidalGuillotine_AOE_{index}";

            // Determine delay based on which AOE it is
            var delay = index == 1 ? 8700 : 9900;

            // Record subsequent AOE
            _tidalGuillotineAoes.Add((name, delay));

            // Draw subsequent AOE
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = name;
            dp.Position = pos;
            dp.Scale = new Vector2(20);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = delay - 5000;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            accessory.Log.Debug($"Recorded {index + 1}th Tidal Guillotine AOE, position: {pos}");
        }
        [ScriptMethod(
            name: "Open Water - Floor Fire (Nimbletooth Shark)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41687"]
        )]
        public void OnOpenWaterRefactored(Event @event, ScriptAccessory accessory)
        {
            var caster = accessory.Data.Objects.SearchById(@event.SourceId);
            if (caster == null) return;

            // --- 1. Determine mode and set parameters ---
            var dirToCaster = caster.Position - SharkArenaCenter;
            var isInnerMode = dirToCaster.LengthSquared() < 15f * 15f;

            int maxCasts;
            int intervalMs;
            float rotationIncrementRad;
            float aoeRadius;

            if (isInnerMode)
            {
                maxCasts = 35;
                intervalMs = 1200;
                rotationIncrementRad = DegToRad(22.5f);
                aoeRadius = 4f;
            }
            else
            {
                maxCasts = 59;
                intervalMs = 690;
                rotationIncrementRad = DegToRad(12f);
                aoeRadius = 4f;
            }

            // --- 2. Calculate rotation direction ---
            var forwardVector = new Vector3(MathF.Sin(caster.Rotation), 0, MathF.Cos(caster.Rotation));
            var rotationDirection = (Vector3.Cross(dirToCaster, forwardVector).Y < 0) ? 1.0f : -1.0f;
            var signedRotationIncrement = rotationDirection * rotationIncrementRad;

            // --- 3. Pre-calculate all AOE positions ---
            var aoePositions = new List<Vector3>();
            for (int i = 0; i < maxCasts; i++)
            {
                var rotationAngle = signedRotationIncrement * i;
                // Cache cos/sin for this iteration
                var cosAngle = MathF.Cos(rotationAngle);
                var sinAngle = MathF.Sin(rotationAngle);
                var rotatedDir = new Vector3(
                    dirToCaster.X * cosAngle - dirToCaster.Z * sinAngle,
                    dirToCaster.Y,
                    dirToCaster.X * sinAngle + dirToCaster.Z * cosAngle
                );
                aoePositions.Add(SharkArenaCenter + rotatedDir);
            }

            // --- 4. Send all drawing commands at once ---
            for (int i = 0; i < aoePositions.Count; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"OpenWater_Refactored_AOE_{i}";
                dp.Position = aoePositions[i];
                dp.Scale = new Vector2(aoeRadius);

                // First circle uses a more eye-catching color
                dp.Color = (i == 0)
                    ? new Vector4(1, 0, 0, 0.8f) // Red
                    : accessory.Data.DefaultDangerColor; // Normal warning color

                // Calculate appearance and disappearance times for each AOE
                int explosionTime = 5000 + (i * intervalMs);
                int appearanceTime = explosionTime - 5000;

                dp.Delay = (appearanceTime > 0) ? appearanceTime : 0; // Warning circle appearance time
                dp.DestoryAt = 5000; // Warning circle total duration
                dp.ScaleMode |= ScaleMode.ByTime;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }

            accessory.Log.Debug($"Rotating donut (refactored) started. Mode: {(isInnerMode ? "Inner" : "Outer")}, Count: {maxCasts}, Direction: {(rotationDirection > 0 ? "Clockwise" : "Counterclockwise")}");
        }

        #endregion
        #region Citadel Guardian
        private const uint AID_AncientAeroIII_5s = 41287;  // Ancient Aero III (5s cast)
        private const uint AID_AncientAeroIII_12s = 41292; // Ancient Aero III (12s cast)
        private const uint AID_AncientStoneIII_5s = 41289; // Ancient Stone III (5s cast)
        private const uint AID_AncientStoneIII_12s = 41293; // Ancient Stone III (12s cast)
        private const uint AID_WindSurge = 41295; // Wind Surge (AOE explosion skill)
        private const uint AID_SandSurge = 41296; // Sand Surge (AOE explosion skill)
        private const uint AID_AncientHoly1 = 41284;       // Holy
        private const uint AID_LightSurge = 41294; // Light Surge

        [ScriptMethod(
            name: "Holy Flame (Citadel Guardian)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41297"]
        )]
        public void HolyFlameDraw(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "CitadelGuardian_HolyFlame_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(5, 60);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 4000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(
            name: "Wind Stone Light - Record Energy Orb (Citadel Guardian)",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: ["DataId:18125"],
            userControl: false
        )]
        public void OnSphereCreated(Event @event, ScriptAccessory accessory)
        {
            var sphere = accessory.Data.Objects.SearchById(@event.SourceId);
            if (sphere != null)
            {
                lock (_surgeLock)
                {
                    _spheres.Add(sphere.EntityId);
                }
                accessory.Log.Debug($"Recorded a new energy orb: {sphere.Name}, ID: {sphere.EntityId}");
            }
        }
        [ScriptMethod(
            name: "Wind Stone Light - Energy Orb Attribute Classification (Citadel Guardian)",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:2536"],
            userControl: false
        )]
        public void OnSphereStatusGain(Event @event, ScriptAccessory accessory)
        {
            var sphere = accessory.Data.Objects.SearchById(@event.TargetId);
            if (sphere == null) return;

            // status.Extra (Param) is used to distinguish Wind (0x224) or Stone (0x225)
            var statusParam = @event["Param"];

            lock (_surgeLock)
            {
                if (!_spheres.Contains(sphere.EntityId)) return; // Check within lock

                if (statusParam == "548")
                {
                    _spheresWind.Add(sphere.EntityId);
                    _spheres.Remove(sphere.EntityId);
                    accessory.Log.Debug($"Energy orb {sphere.EntityId} classified as [Wind]");
                }
                else if (statusParam == "549")
                {
                    _spheresStone.Add(sphere.EntityId);
                    _spheres.Remove(sphere.EntityId);
                    accessory.Log.Debug($"Energy orb {sphere.EntityId} classified as [Stone]");
                }
            }
        }

        [ScriptMethod(
            name: "Wind Stone Light - Predict and Draw AOE (Citadel Guardian)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(41287|41292|41289|41293)$"]
        )]
        public void OnAncientSpellCast(Event @event, ScriptAccessory accessory)
        {
            var caster = accessory.Data.Objects.SearchById(@event.SourceId);
            if (caster == null) return;

            // Determine skill type (Wind/Stone) and cast time based on ActionId
            List<ulong> originalSphereList;
            int castTimeMs;

            switch (@event.ActionId)
            {
                case AID_AncientAeroIII_5s:
                    originalSphereList = _spheresWind;
                    castTimeMs = 5000;
                    break;
                case AID_AncientAeroIII_12s:
                    originalSphereList = _spheresWind;
                    castTimeMs = 12000;
                    break;
                case AID_AncientStoneIII_5s:
                    originalSphereList = _spheresStone;
                    castTimeMs = 5000;
                    break;
                case AID_AncientStoneIII_12s:
                    originalSphereList = _spheresStone;
                    castTimeMs = 12000;
                    break;
                default:
                    return; // Shouldn't happen
            }

            accessory.Log.Debug($"Detected boss cast {@event.ActionId} (cast time: {castTimeMs}ms), starting to detect energy orbs within range.");

            // Create a copy of the list within the lock for safe iteration
            List<ulong> sphereIdsToCheck;
            lock (_surgeLock)
            {
                if (originalSphereList.Count == 0) return;
                sphereIdsToCheck = new List<ulong>(originalSphereList);
            }

            // Define a forward cone detection area (40 yards long, 60-degree angle)
            var coneAngle = DegToRad(60);
            var coneLength = 40f;

            // Iterate over the copy to avoid modifying the collection during iteration
            foreach (var sphereId in sphereIdsToCheck)
            {
                var sphere = accessory.Data.Objects.SearchById(sphereId);
                if (sphere == null) continue;

                // --- Cone range detection logic ---
                var vectorToSphere = sphere.Position - caster.Position;
                var distance = vectorToSphere.Length();

                if (distance > 0 && distance < coneLength)
                {
                    var casterForward = new Vector3(MathF.Sin(caster.Rotation), 0, MathF.Cos(caster.Rotation));
                    var dotProduct = Vector3.Dot(Vector3.Normalize(vectorToSphere), Vector3.Normalize(casterForward));
                    var angleToSphere = MathF.Acos(Math.Clamp(dotProduct, -1.0f, 1.0f));

                    if (Math.Abs(angleToSphere) < coneAngle / 2)
                    {
                        // Sphere is within cone range, draw AOE
                        accessory.Log.Debug($"Energy orb {sphereId} is within attack range, preparing to draw AOE.");

                        var dp = accessory.Data.GetDefaultDrawProperties();
                        var drawName = $"Surge_AOE_{sphereId}";

                        dp.Name = drawName;
                        dp.Position = sphere.Position;
                        dp.Scale = new Vector2(15); // AOE radius 15
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.Delay = castTimeMs - 5000;
                        dp.DestoryAt = 7500;
                        dp.ScaleMode |= ScaleMode.ByTime;

                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                        // Modify the original list within the lock
                        lock (_surgeLock)
                        {
                            _surgeAoes[sphereId] = drawName;
                            originalSphereList.Remove(sphereId);
                        }
                    }
                }
            }
        }
        [ScriptMethod(
            name: "Wind Stone Light - Mark Holy Cast Start (Citadel Guardian)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41284"],
            userControl: false
        )]
        public void OnAncientHolyCast(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"Detected boss start casting Holy ({@event.ActionId}), delaying light orb judgment.");
            _isHolyCasting = true;
        }
        [ScriptMethod(
            name: "Wind Stone Light - Holy Cast Complete and Detonate Light Orbs (Citadel Guardian)",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:41284"]
        )]
        public void OnAncientHolyFinish(Event @event, ScriptAccessory accessory)
        {
            if (!_isHolyCasting) return;

            accessory.Log.Debug("Holy cast complete, starting to process remaining light orbs.");

            List<ulong> lightSphereIds;
            lock (_surgeLock)
            {
                if (_spheres.Count == 0)
                {
                    _isHolyCasting = false; // Reset marker
                    return;
                }
                lightSphereIds = new List<ulong>(_spheres);
            }

            // Explosion occurs about 2.4 seconds after cast completes
            const int explosionDelay = 2400;

            foreach (var sphereId in lightSphereIds)
            {
                var sphere = accessory.Data.Objects.SearchById(sphereId);
                if (sphere == null) continue;

                accessory.Log.Debug($"Light orb {sphereId} will explode in {explosionDelay}ms.");

                var dp = accessory.Data.GetDefaultDrawProperties();
                var drawName = $"Surge_AOE_Light_{sphereId}";

                dp.Name = drawName;
                dp.Position = sphere.Position;
                dp.Scale = new Vector2(15);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 2400;
                dp.ScaleMode |= ScaleMode.ByTime;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                lock (_surgeLock)
                {
                    _surgeAoes[sphereId] = drawName;
                }
            }

            lock (_surgeLock)
            {
                _spheres.Clear();
            }

            _isHolyCasting = false; // Reset marker
        }


        [ScriptMethod(
            name: "Wind Stone Light - Clear Exploded AOE (Citadel Guardian)",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:regex:^(41296|41295|41294)$"],
            userControl: false
        )]
        public void OnSurgeExplosion(Event @event, ScriptAccessory accessory)
        {
            var explodingSphereId = @event.SourceId;

            // Find and remove drawing matching the exploding energy orb
            if (_surgeAoes.TryGetValue(explodingSphereId, out var drawName))
            {
                accessory.Method.RemoveDraw(drawName);
                _surgeAoes.Remove(explodingSphereId);
                accessory.Log.Debug($"Cleared AOE drawing: {drawName}");
            }
        }
        public void ResetWindStoneLightSurgeState()
        {
            lock (_surgeLock)
            {
                _isHolyCasting = false;
                _surgeAoes.Clear();
                _spheres.Clear();
                _spheresStone.Clear();
                _spheresWind.Clear();
            }
        }
        #endregion
        #region Mind Flayer
        [ScriptMethod(
            name: "Dark II (Mind Flayer)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41170"]
        )]
        public void OnDarkIIDraw(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "MindFlayer_DarkII_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(65);
            dp.Radian = DegToRad(90);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        [ScriptMethod(
            name: "Tentacle (Mind Flayer)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(41257|41314|41256)$"]
        )]
        public void TentacleDraw(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "MindFlayer_Tentacle_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.ScaleMode |= ScaleMode.ByTime;
            switch (@event.ActionId)
            {
                case 41257:
                    dp.Scale = new Vector2(20, 60);
                    dp.DestoryAt = 11000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                    break;
                case 41314:
                    dp.Scale = new Vector2(10, 60);
                    dp.DestoryAt = 7000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                    break;
                case 41256:
                    dp.Scale = new Vector2(10, 60);
                    dp.DestoryAt = 11000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                    break;
            }
        }


        private const uint SID_PlayingWithFire = 4211;
        private const uint SID_PlayingWithIce = 4212;
        private const uint SID_ImpElement = 2193;
        private const uint OID_JestingJackanapes = 18102;
        private const uint AID_SurpriseAttack = 41254;


        [ScriptMethod(
            name: "Fire-Ice Traps - Record Player Element (Mind Flayer)",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:regex:^(4211|4212)$"],
            userControl: false
        )]
        public void OnPlayerElementGain(Event @event, ScriptAccessory accessory)
        {
            var isFire = @event.StatusId == SID_PlayingWithFire;
            lock (_trapLock)
            {
                _playerElements[@event.TargetId] = isFire;
                // [Added log] Confirm player status was successfully recorded
                if (@event.TargetId == accessory.Data.Me)
                {
                    accessory.Log.Debug($"[Status Record] Successfully recorded your element status as: {(isFire ? "Fire" : "Ice")}");
                }
            }

            if (@event.TargetId == accessory.Data.Me)
            {
                DrawFireIceTraps(accessory);
            }
        }

        [ScriptMethod(
            name: "Fire-Ice Traps - Remove Player Element (Mind Flayer)",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:regex:^(4211|4212)$"],
            userControl: false
        )]
        public void OnPlayerElementLose(Event @event, ScriptAccessory accessory)
        {
            bool needsRedraw = false;
            lock (_trapLock)
            {
                if (_playerElements.Remove(@event.TargetId))
                {
                    needsRedraw = @event.TargetId == accessory.Data.Me;
                }
            }

            if (needsRedraw)
            {
                accessory.Log.Debug("Your element has disappeared, updating trap prompts.");
                DrawFireIceTraps(accessory);
            }
        }
        [ScriptMethod(
            name: "Fire-Ice Traps - Record Trap (Mind Flayer)",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:2193"]
        )]
        public void OnTrapCreated(Event @event, ScriptAccessory accessory)
        {
            var npc = accessory.Data.Objects.SearchById(@event.TargetId);
            if (npc == null || npc.DataId != OID_JestingJackanapes) return;
            accessory.Log.Debug($"Trap NPC {@event.TargetId} gained element status, received Param value: '{@event["Param"]}'");
            // Determine element based on status.Extra (Param), 0x344 is fire, otherwise ice
            var isFire = @event["Param"] == "836";

            var trap = new FireIceTrapInfo
            {
                NpcId = npc.EntityId,
                Position = npc.Position,
                IsFire = isFire
            };

            lock (_trapLock)
            {
                _fireIceTraps.Add(trap);
            }
            accessory.Log.Debug($"Discovered a new {(isFire ? "Fire" : "Ice")} trap at {trap.Position}");
            DrawFireIceTraps(accessory);
        }
        [ScriptMethod(
            name: "Fire-Ice Traps - Update Trap Position (Mind Flayer)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41254"]
        )]
        public void OnTrapMove(Event @event, ScriptAccessory accessory)
        {
            // Lock before accessing the shared list to ensure thread safety
            lock (_trapLock)
            {
                // Get the caster position of the "Move" skill
                var casterPosition = @event.SourcePosition;

                // Find the trap to move by checking which trap's position is very close to the caster's position
                int trapIndex = _fireIceTraps.FindIndex(t => Vector3.Distance(t.Position, casterPosition) < 1.0f);

                if (trapIndex != -1)
                {
                    // Get the old trap data
                    var oldTrap = _fireIceTraps[trapIndex];
                    var newPosition = @event.EffectPosition; // The trap's new position is the skill's target point
                    accessory.Log.Debug($"Trap moved from {oldTrap.Position} to {newPosition} (via position matching)");

                    // Create a new trap object with the new position
                    var updatedTrap = new FireIceTrapInfo
                    {
                        NpcId = oldTrap.NpcId,
                        Position = newPosition, // Use the new position
                        IsFire = oldTrap.IsFire
                    };

                    // Replace the old object in the list with the new object, which is the safest way to update
                    _fireIceTraps[trapIndex] = updatedTrap;
                }
                else
                {
                    accessory.Log.Debug($"Failed to match any known trap near position {casterPosition}.");
                }
            }
            // Redraw after modification
            DrawFireIceTraps(accessory);
        }
        [ScriptMethod(
            name: "Fire-Ice Traps - Mechanic End Cleanup (Mind Flayer)",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:regex:^(41250|41251)$"],
            userControl: false
        )]
        public void OnTrapExplosion(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug("Trap exploded, clearing all drawings and states.");
            lock (_trapLock)
            {
                _fireIceTraps.Clear();
                _playerElements.Clear();
            }
            accessory.Method.RemoveDraw("FireIceTrap_.*");
        }

        // --- Mind Flayer Helper Methods ---

        private void DrawFireIceTraps(ScriptAccessory accessory)
        {
            // [Critical correction] Before redrawing, remove all old drawings with the same name
            accessory.Method.RemoveDraw("FireIceTrap_.*");

            // [Critical correction] Add a short delay to ensure the removal command is processed
            Thread.Sleep(50); // 50 millisecond delay
            List<FireIceTrapInfo> trapsCopy;
            Dictionary<ulong, bool> playerElementsCopy;
            lock (_trapLock)
            {
                trapsCopy = _fireIceTraps.ToList();
                playerElementsCopy = _playerElements.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            // 2. Get the current player's element status
            _playerElements.TryGetValue(accessory.Data.Me, out var playerIsFire);
            bool playerHasElement = _playerElements.ContainsKey(accessory.Data.Me);
            // [Added log] Before drawing, confirm what the script thinks your status is
            if (playerHasElement)
            {
                accessory.Log.Debug($"[Pre-drawing Check] Script thinks your status is: {(playerIsFire ? "Fire" : "Ice")}");
            }
            else
            {
                accessory.Log.Debug("[Pre-drawing Check] Script thinks you have no element status.");
            }
            // 3. Dynamically determine warning duration based on the number of traps on the field
            int trapWarningDurationMs = _fireIceTraps.Count > 2 ? 20000 : 10000; // More than 2 jesters = 20 seconds, otherwise 10 seconds
            if (trapsCopy.Any())
            {
                accessory.Log.Debug($"Current trap count: {trapsCopy.Count}, warning duration set to: {trapWarningDurationMs}ms");
            }



            // 4. Iterate through all known traps and draw them
            foreach (var trap in trapsCopy)
            {
                // If the player doesn't have an element debuff, all traps are shown as small circles (basic prompt)
                if (!playerHasElement)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"FireIceTrap_Small_{trap.NpcId}";
                    dp.Position = trap.Position;
                    dp.Scale = new Vector2(8); // Small circle radius 8
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = trapWarningDurationMs; // Use dynamically calculated duration
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    continue; // Continue to next trap
                }

                // If the player has an element debuff, perform special drawing
                // Determine if the current trap's element matches the player's
                bool isSameElement = (playerIsFire == trap.IsFire);
                accessory.Log.Debug($"[Loop Check] Trap {trap.NpcId}: Player is fire({playerIsFire}), Trap is fire({trap.IsFire}). --> Same element: {isSameElement}");
                if (isSameElement)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"FireIceTrap_Big_Danger_{trap.NpcId}";
                    dp.Position = trap.Position;
                    dp.Scale = new Vector2(38); // Large circle outer radius 38
                    dp.Color = new Vector4(1.0f, 0.2f, 0.2f, 0.6f);
                    dp.DestoryAt = trapWarningDurationMs;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                else
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"FireIceTrap_Small_Safe_{trap.NpcId}";
                    dp.Position = trap.Position;
                    dp.Scale = new Vector2(8);
                    dp.Color = new Vector4(1.0f, 0.2f, 0.2f, 0.6f);
                    dp.DestoryAt = trapWarningDurationMs;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
        }

        #endregion
        #region Leap Lion OnTheHuntAreaCenter

        [ScriptMethod(
            name: "Terror Flash (Leap Lion)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41411"]
        )]
        public void TerrorFlashDraw(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "LeapLion_TerrorFlash_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(60);
            dp.Radian = DegToRad(90);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            accessory.Log.Debug("Drawing Leap Lion's Terror Flash AOE");
        }
        [ScriptMethod(
            name: "Decompress (Leap Lion)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41407"]
        )]
        public void OnDecompressDraw(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "LeapLion_Decompress_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(12);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.ByTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            accessory.Log.Debug("Drawing Leap Lion's Decompress AOE");
        }
        [ScriptMethod(
            name: "Aetherial Ray (Leap Lion)",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:0138"]
        )]
        public void OnAetherialRayTether(Event @event, ScriptAccessory accessory)
        {

            var target = accessory.Data.Objects.SearchById(@event.TargetId);
            if (target == null)
            {
                accessory.Log.Error($"Cannot find tether target: {@event.TargetId}");
                return;
            }

            // Calculate vector from center to target
            var directionVector = target.Position - OnTheHuntAreaCenter;

            // Convert vector to angle, and add a fixed 200-degree rotation
            var angle = MathF.Atan2(directionVector.X, directionVector.Z);
            var finalRotation = angle + DegToRad(200);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"CrystalDragon_AetherialRay_{target.EntityId}";
            dp.Position = OnTheHuntAreaCenter; // AOE at arena center
            dp.Scale = new Vector2(10, 28); // Width 10
            dp.Rotation = finalRotation;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5200;
            dp.ScaleMode |= ScaleMode.ByTime;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            accessory.Log.Debug($"Drawing Aetherial Ray, target: {target.Name}, rotation angle: {finalRotation}");
        }
        [ScriptMethod(
            name: "Bright Pulse (Leap Lion)",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:2193"]
        )]
        public void OnBrightPulseStatus(Event @event, ScriptAccessory accessory)
        {
            var roundel = accessory.Data.Objects.SearchById(@event.TargetId);
            if (roundel == null || roundel.DataId != 18142) return;


            // 1. Calculate direction vector from center to light orb
            var dir = roundel.Position - OnTheHuntAreaCenter;

            // 2. Determine if light orb is in inner or outer circle to decide base rotation angle
            // Radius 15 squared is 225
            var angleDegrees = dir.LengthSquared() < 225f ? 280f : 150f;

            // 3. Determine light orb's facing to decide clockwise or counterclockwise rotation
            // Get light orb's forward vector
            var forwardVector = new Vector3(MathF.Sin(roundel.Rotation), 0, MathF.Cos(roundel.Rotation));
            // Calculate the perpendicular vector of the direction vector (OrthoR)
            var orthoR = new Vector3(dir.Z, 0, -dir.X);
            // Dot product to determine direction
            if (Vector3.Dot(orthoR, forwardVector) > 0f)
            {
                angleDegrees = -angleDegrees; // Reverse rotation
            }

            // 4. Calculate final AOE position
            var angleRadians = DegToRad(angleDegrees);
            var rotatedDir = new Vector3(
                dir.X * MathF.Cos(angleRadians) - dir.Z * MathF.Sin(angleRadians),
                dir.Y,
                dir.X * MathF.Sin(angleRadians) + dir.Z * MathF.Cos(angleRadians)
            );
            var aoePosition = OnTheHuntAreaCenter + rotatedDir;

            // 5. Draw AOE
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"CrystalDragon_BrightPulse_{roundel.EntityId}";
            dp.Position = aoePosition;
            dp.Scale = new Vector2(13); // Radius 13
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5200; // Disappears after 5.2 seconds
            dp.ScaleMode |= ScaleMode.ByTime;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            accessory.Log.Debug($"Drawing Bright Pulse, light orb ID: {roundel.EntityId}, final position: {aoePosition}");
        }

        [ScriptMethod(
            name: "Drawing Removal (Leap Lion)",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:regex:^(41402|41403)$"],
            userControl: false
        )]
        public void RemoveCrystalDragonAoes(Event @event, ScriptAccessory accessory)
        {
            if (@event.ActionId == 41402) // AetherialRay
            {
                accessory.Method.RemoveDraw("CrystalDragon_AetherialRay_.*");
                accessory.Log.Debug("Cleared Aetherial Ray drawings.");
            }
            else if (@event.ActionId == 41403) // BrightPulse
            {
                accessory.Method.RemoveDraw($"CrystalDragon_BrightPulse_{@event.SourceId}");
                accessory.Log.Debug($"Cleared Bright Pulse drawing: {@event.SourceId}");
            }
        }
        #endregion

        #region Golden Tortoise
        [ScriptMethod(
            name: "Cost of Living Knockback (Golden Tortoise)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41522"]
        )]
        public void OnCostOfLivingKB(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "CostOfLivingKBDraw";
            dp.Owner = accessory.Data.Me;
            dp.Scale = new Vector2(1.5f, 30f);
            dp.Rotation = 180f * MathF.PI / 180f;
            dp.Color = new(0.3f, 1.0f, 0f, 1.5f);
            dp.TargetPosition = @event.SourcePosition;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
            accessory.Log.Debug($"Drawing knockback: Name={dp.Name}s");
        }
        [ScriptMethod(
            name: "Forced Movement (Golden Tortoise)",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:4344"]
        )]
        public void OnForcedMovement(Event @event, ScriptAccessory accessory)
        {
            if (@event.TargetId != accessory.Data.Me)
            {
                accessory.Log.Debug("Forced movement event not targeting player, ignoring.");
                return;
            }
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "ForcedMovementDraw";
            dp.Owner = accessory.Data.Me;
            dp.Scale = new Vector2(1.5f, 35f);
            dp.Color = new(0.3f, 1.0f, 0f, 1.5f);
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
            accessory.Log.Debug($"Drawing forced movement: Name={dp.Name}");
        }
        #endregion

        #region Fate

        [ScriptMethod(
            name: "Fire Guy Cone",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(30351|30353)$"]
        )]
        public void OnFluidSwingDraw(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "FluidSwing";
            dp.Radian = 90 * MathF.PI / 180f;
            dp.Scale = new Vector2(60);
            dp.ScaleMode = ScaleMode.ByTime;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        [ScriptMethod(
            name: "Fire Guy Circle",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(30340)$"]
        )]
        public void OnHeatVortexDraw(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "HeatVortex";
            dp.Scale = new Vector2(10);
            dp.ScaleMode = ScaleMode.ByTime;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(
            name: "Fire Guy Rectangle",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(30341)$"]
        )]
        public void OnFireBlastDraw(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "FireBlast";
            dp.Scale = new Vector2(6, 25);
            dp.ScaleMode = ScaleMode.ByTime;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        #endregion

        #region Helper

        // =================================================================================
        // =============================== General Helper Methods ===================================
        // =================================================================================

        /// <summary>
        /// Converts degrees to radians
        /// </summary>
        /// <param name="degrees">Angle in degrees</param>
        /// <returns>Corresponding value in radians</returns>
        private static float DegToRad(float degrees) => degrees * MathF.PI / 180.0f;

        /// <summary>
        /// Safely deserializes Vector3 from a JSON string
        /// </summary>
        private static bool TryDeserializeVector3(string json, out Vector3 result)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    result = default;
                    return false;
                }
                var deserialized = JsonConvert.DeserializeObject<Vector3>(json);
                result = deserialized;
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Safely deserializes float from a JSON string
        /// </summary>
        private static bool TryDeserializeFloat(string json, out float result)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    result = default;
                    return false;
                }
                result = JsonConvert.DeserializeObject<float>(json);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        #endregion

    }
}
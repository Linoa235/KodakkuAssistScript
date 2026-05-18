// File: TowerOfStrength_XSZYYS.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using Dalamud.Interface.ManagedFontAtlas;
using KodakkuAssist.Data;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Extensions;
using Newtonsoft.Json;
using System.Runtime.Intrinsics.Arm;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.GameOperate;
using System.Collections.Concurrent;
using FFXIVClientStructs.Havok.Animation.Rig;
using KodakkuAssist.Module.Draw.Manager;
using Lumina.Excel;

namespace KodakkuAssistXSZYYS
{
    public enum StrategySelection
    {
        ABC_123,  // Represents YuziShao grouping strategy
        Pos_152463, // Represents 152463 grouping strategy
        LemonCookie // Represents LemonCookie grouping strategy
    }

    public enum TeamSelection
    {
        A,
        B,
        C,
        One,
        Two,
        Three
    }

    public enum PositionSelection
    {
        Pos1,
        Pos2,
        Pos3,
        Pos4,
        Pos5,
        Pos6
    }
    // Holy Lance exclusive: group override (None=unchanged; top-left=A; top-right=C; bottom=B)
    public enum LanceGuideOverride
    {
        None, 
        TopLeft, 
        TopRight, 
        Bottom
    }

    [ScriptType(
    name: "Tower of Strength",
    guid: "c4901d90-1939-42c6-9554-c88501c5812d",
    territorys: [1252],
    version: "0.0.40",
    Author: "Linoa235",
    note: "Update content\r\nDirect role-based rather than party position-based guidance for second boss fireball positions\r\nMark Chemist: type ã€/e Mark Chemistã€‘ to mark all nearby chemist players\r\nTreasure Map: line to chests in 1.5 corridors\r\n\r\n------------The following functions only support Echo output by default, can be configured to respond to party check commands and output in party channel------------\r\nCheck Blue Potion: type ã€/e Blue Potion Checkã€‘ will output chemist blue potion usage, type ã€/e Blue Potion Clearã€‘ will clear all data\r\nCheck Resurrection: type ã€/e Resurrection Check <number>ã€‘, e.g., ã€/e Resurrection Check 1ã€‘ will output all nearby players with 1 remaining resurrection\r\nCheck Throw Money: type ã€/e Throw Money Checkã€‘ will output all players who used throw money and their counts, type ã€/e Throw Money Clearã€‘ will clear all data\r\n------------------------------------------------------------\r\nPlease select your team's grouping, guidance available for ABC123/152463/LemonCookie strategies\r\nFirst boss:\r\nAOE drawing: rotation, crushing\r\nGuidance: meteor mark, first tower stepping, second tower stepping\r\nSecond boss:\r\nAOE drawing: tankbuster, cone, ice-fire explosion\r\nGuidance: snowball, fireball\r\nThird boss:\r\nAOE drawing: draconic action, ice circle, dive\r\nGuidance: draconic action pre-position, tower stepping, mobs\r\nFinal boss:\r\nAOE drawing: deadly axe/spear, assassin's dagger\r\nGuidance: rune axe, holy lance"
    )]

    public class TowerOfStrength
    {
        #region User_Settings
        [UserSetting("----- Global Settings ----- (This setting has no practical meaning)")]
        public bool _____Global_Settings_____ { get; set; } = true;
        [UserSetting("Enable TTS")]
        public bool EnableTTS { get; set; } = true;
        [UserSetting("Enable Text Banner Prompts")]
        public bool EnableTextBanner { get; set; } = true;
        [UserSetting("Strategy Grouping (YuziShao i.e. ABC123/152463/LemonCookie)")]
        public StrategySelection SelectedStrategy { get; set; } = StrategySelection.ABC_123;

        [UserSetting("ã€YuziShaoã€‘Please select your assigned group in the team")]
        public TeamSelection MyTeam { get; set; } = TeamSelection.A;

        [UserSetting("ã€152463ã€‘Please select your assigned group in the team")]
        public PositionSelection MyPosition { get; set; } = PositionSelection.Pos1;
        [UserSetting("ã€LemonCookieã€‘Please select your assigned group in the team")]
        public PositionSelection MyLemonCookiePosition { get; set; } = PositionSelection.Pos1;
        [UserSetting("Rune Axe Long Mark Small Circle Guidance (shows two arrows pointing to top-left and top-right platforms)")]
        public bool LongPointName { get; set; } = false;
        [UserSetting("Holy Lance Group Override (None=unchanged)")]
        public LanceGuideOverride HolyLanceGroupOverride { get; set; } = LanceGuideOverride.None;
        [UserSetting("Police Mode (outputs names of players marked by key mechanics in echo channel)")]
        public bool PoliceMode { get; set; } = false;
        [UserSetting("Receive throw money/resurrection/blue potion check requests from party")]
        public bool ReceivePartyCheckRequest { get; set; } = false;
        [UserSetting("Blue Potion Check Scope (party only)")]
        public bool Partycheck { get; set; } = false;
        [UserSetting("----- Developer Settings ----- (This setting has no practical meaning)")]
        public bool _____Developer_Settings_____ { get; set; } = true;

        [UserSetting("Enable Developer Mode")]
        public bool Enable_Developer_Mode { get; set; } = false;

        #endregion

        // State variables for first boss
        private int _turnLeftRightCount = 0;
        // State variables for meteor mechanic
        private bool _hasCometeorStatus = false;
        private ulong _cometeorTargetId = 0;
        private const uint PortentousCometeorDataId = 2014582;
        private const float ArenaCenterZ = 379f; // Define first boss arena center Z coordinate
        private static readonly Vector3 Boss1ArenaCenter = new(700f, -481.01f, 379f);
        private bool? _isCasterInUpperHalf = null;
        private static readonly Vector3 Pos_A = new(704.49f, -481.01f, 365.38f);
        private static readonly Vector3 Pos_B = new(699.98f, -481.01f, 355.49f);
        private static readonly Vector3 Pos_C = new(695.49f, -481.01f, 365.38f);
        private static readonly Vector3 Pos_One = new(695.49f, -481.01f, 392.60f);
        private static readonly Vector3 Pos_Two = new(699.98f, -481.01f, 402.49f);
        private static readonly Vector3 Pos_Three = new(704.49f, -481.01f, 392.60f);

        // State variables for second boss
        // Store positions of blue and red AOE warning circles respectively
        private readonly object _iceFireLock = new();
        private readonly List<Vector3> _blueCircles = new();
        private readonly List<Vector3> _redCircles = new();
        private int _pairsProcessed = 0;
        // --- Snowball Rush mechanic ---
        private int _snowballRushCastCount = 0;
        private int _letterGroupRushCount = 0;
        private int _numberGroupRushCount = 0;
        private Vector3? _letterGroupNextPos;
        private Vector3? _numberGroupNextPos;
        private readonly object _snowballLock = new();
        private readonly object _fireballLock = new();
        private static readonly Vector3 InitialPosLetterGroup = new(-800.00f, -876.00f, 349.50f);
        private static readonly Vector3 InitialPosNumberGroup = new(-809.09f, -876.00f, 365.25f);
        private static readonly Vector3 SnowballArenaCenter = new(-800.00f, -876.00f, 360.00f);
        private ulong _tetherSourceId = 0;
        // --- Fireball/Geothermal mechanic ---
        private readonly List<Vector3> _fireballPositions = new();
        // Define two sets of fireball fixed coordinates
        private static readonly List<Vector3> LetterGroupFireballCoords = new()
        {
            new Vector3(-817.32f, -876.00f, 350.00f), 
            new Vector3(-817.32f, -876.00f, 370.00f), 
            new Vector3(-800.00f, -876.00f, 380.00f)  
        };
        private static readonly List<Vector3> NumberGroupFireballCoords = new()
        {
            new Vector3(-782.68f, -876.00f, 350.00f), 
            new Vector3(-800.00f, -876.00f, 340.00f), 
            new Vector3(-782.68f, -876.00f, 370.00f)  
        };
        //Third boss
        private static readonly Vector3 Boss3ArenaCenter = new(-337.00f, -840.00f, 157.00f); // Boss3 arena center
        // Enum for puddle types
        private enum PuddleType { Circle, Cross }
        // Dictionary for storing puddles on the field (Key: entity ID, Value: type)
        private readonly ConcurrentDictionary<ulong, PuddleType> _puddles = new();
        // Ice tower grouping coordinates
        private static readonly Dictionary<TeamSelection, List<Vector3>> TowerPositions_ABC123 = new()
        {
            { TeamSelection.A, new List<Vector3> { new(-346.00f, -840.00f, 151.00f), new(-343.00f, -840.00f, 148.00f), new(-355.5f, -840.0f, 138.5f), new(-337.0f, -840.0f, 131.0f) } },
            { TeamSelection.B, new List<Vector3> { new(-337.00f, -840.00f, 151.00f), new(-343.00f, -840.00f, 157.00f), new(-337.0f, -840.0f, 131.0f), new(-318.5f, -840.0f, 138.5f) } },
            { TeamSelection.C, new List<Vector3> { new(-328.00f, -840.00f, 151.00f), new(-331.00f, -840.00f, 148.00f), new(-318.5f, -840.0f, 138.5f), new(-311.0f, -840.0f, 157.0f) } },
            { TeamSelection.One, new List<Vector3> { new(-328.00f, -840.00f, 163.00f), new(-331.00f, -840.00f, 166.00f), new(-318.5f, -840.0f, 175.5f), new(-337.0f, -840.0f, 183.0f) } },
            { TeamSelection.Two, new List<Vector3> { new(-337.00f, -840.00f, 163.00f), new(-331.00f, -840.00f, 157.00f), new(-337.0f, -840.0f, 183.0f), new(-355.5f, -840.0f, 175.5f) } },
            { TeamSelection.Three, new List<Vector3> { new(-346.00f, -840.00f, 163.00f), new(-343.00f, -840.00f, 166.00f), new(-355.5f, -840.0f, 175.5f), new(-363.0f, -840.0f, 157.0f) } }
        };
        // 152463 strategy ice tower grouping coordinates
        private static readonly Dictionary<PositionSelection, List<Vector3>> TowerPositions_123456 = new()
        {
            { PositionSelection.Pos1, new List<Vector3> { new(-346.00f, -840.00f, 151.00f), new(-343.00f, -840.00f, 166.00f), new(-355.5f, -840.0f, 138.5f), new(-337.0f, -840.0f, 131.0f) } },
            { PositionSelection.Pos2, new List<Vector3> { new(-328.00f, -840.00f, 151.00f), new(-343.00f, -840.00f, 148.00f), new(-318.5f, -840.0f, 138.5f), new(-311.0f, -840.0f, 157.0f) } },
            { PositionSelection.Pos3, new List<Vector3> { new(-328.00f, -840.00f, 163.00f), new(-331.00f, -840.00f, 148.00f), new(-318.5f, -840.0f, 175.5f), new(-337.0f, -840.0f, 183.0f) } },
            { PositionSelection.Pos4, new List<Vector3> { new(-346.00f, -840.00f, 163.00f), new(-331.00f, -840.00f, 166.00f), new(-355.5f, -840.0f, 175.5f), new(-363.0f, -840.0f, 157.0f) } },
            { PositionSelection.Pos5, new List<Vector3> { new(-337.00f, -840.00f, 151.00f), new(-343.00f, -840.00f, 157.00f), new(-337.0f, -840.0f, 131.0f), new(-318.5f, -840.0f, 138.5f) } },
            { PositionSelection.Pos6, new List<Vector3> { new(-337.00f, -840.00f, 163.00f), new(-331.00f, -840.00f, 157.00f), new(-337.0f, -840.0f, 183.0f), new(-355.5f, -840.0f, 175.5f) } }
        };
        private static readonly Dictionary<PositionSelection, List<Vector3>> TowerPosition_Lemon = new()
        {
            { PositionSelection.Pos1, new List<Vector3> { new(-346.00f, -840.00f, 151.00f), new(-343.00f, -840.00f, 166.00f), new(-355.5f, -840.0f, 138.5f), new(-337.0f, -840.0f, 131.0f) } },
            { PositionSelection.Pos2, new List<Vector3> { new(-337.00f, -840.00f, 151.00f), new(-343.00f, -840.00f, 157.00f), new(-337.0f, -840.0f, 131.0f), new(-318.5f, -840.0f, 138.5f) } },
            { PositionSelection.Pos3, new List<Vector3> { new(-328.00f, -840.00f, 151.00f), new(-343.00f, -840.00f, 148.00f), new(-318.5f, -840.0f, 138.5f), new(-311.0f, -840.0f, 157.0f) } },
            { PositionSelection.Pos4, new List<Vector3> { new(-346.00f, -840.00f, 163.00f), new(-331.00f, -840.00f, 166.00f), new(-355.5f, -840.0f, 175.5f), new(-363.0f, -840.0f, 157.0f) } },
            { PositionSelection.Pos5, new List<Vector3> { new(-337.00f, -840.00f, 163.00f), new(-331.00f, -840.00f, 157.00f), new(-337.0f, -840.0f, 183.0f), new(-355.5f, -840.0f, 175.5f) } },
            { PositionSelection.Pos6, new List<Vector3> { new(-328.00f, -840.00f, 163.00f), new(-331.00f, -840.00f, 148.00f), new(-318.5f, -840.0f, 175.5f), new(-337.0f, -840.0f, 183.0f) } }
        };
        // Mob grouping coordinates
        private static readonly Dictionary<TeamSelection, Vector3> GroupMarkerPositions = new()
        {
            { TeamSelection.A, new Vector3(-347.50f, -840.00f, 146.50f) },
            { TeamSelection.B, new Vector3(-337.00f, -840.00f, 142.00f) },
            { TeamSelection.C, new Vector3(-326.50f, -840.00f, 146.50f) },
            { TeamSelection.One, new Vector3(-326.50f, -840.00f, 167.50f) },
            { TeamSelection.Two, new Vector3(-337.00f, -840.00f, 172.00f) },
            { TeamSelection.Three, new Vector3(-347.50f, -840.00f, 167.50f) }
        };
        //Final boss
        // Square AOE fixed coordinates and angles
        private static readonly List<Vector3> SquarePositions = new()
        {
            new(700f, -476f, -659.504f),
            new(712.554f, -476f, -681.248f),
            new(687.443f, -476f, -681.25f)
        };
        private static readonly float[] SquareAngles =
        {
            -45 * MathF.PI / 180.0f,
            -15 * MathF.PI / 180.0f,
            105 * MathF.PI / 180.0f
        };
        // Great Axe Prey mechanic three edge coordinates
        private static readonly List<Vector3> GreataxePreyPositions = new()
        {
            new(699.95f, -476.00f, -705.12f),
            new(673.07f, -476.00f, -658.11f),
            new(726.77f, -476.00f, -658.39f)
        };
        // Deadly Axe mechanic three safe point coordinates
        private static readonly List<Vector3> CriticalAxeSafePositions = new()
        {
            new(723.11f, -476.00f, -687.15f),
            new(677.93f, -476.00f, -686.84f),
            new(699.77f, -476.00f, -648.27f)
        };
        // Deadly Lance mechanic three safe point coordinates
        private static readonly List<Vector3> CriticalLanceSafePositions = new()
        {
            new(693.53f, -476.00f, -670.09f),
            new(699.82f, -476.00f, -680.72f),
            new(706.28f, -476.00f, -670.38f)
        };
        // Holy Lance mechanic group position coordinates
        private static readonly Vector3 RectSideInA = new(683.71f, -476.00f, -688.60f);
        private static readonly Vector3 RectSideOutA = new(680.45f, -476.00f, -691.63f);
        private static readonly Vector3 RectSideInB = new(721.56f, -476.00f, -682.53f);
        private static readonly Vector3 RectSideOutB = new(724.71f, -476.00f, -680.50f);
        private static readonly Vector3 RectSideInC = new(695.61f, -476.00f, -653.43f);
        private static readonly Vector3 RectSideOutC = new(694.24f, -476.00f, -648.11f);
        // State variables for Holy mechanic
        private enum HolyWeaponType { None, Axe, Lance }
        private HolyWeaponType _holyWeaponType = HolyWeaponType.None;
        // For recording players already checked for prey marks
        private readonly HashSet<ulong> _checkedPreyPlayers = new();
        private readonly object _preyCheckLock = new(); 
        private readonly HashSet<ulong> _sacredBowPreyRecordedPlayers = new();
        private readonly object _sacredBowPreyLock = new();
        // Dictionary for recording lance share players and their debuff durations
        private readonly Dictionary<int, List<(ulong PlayerId, float Duration)>> _lanceShareAssignments = new();
        private readonly object _lanceShareLock = new();
        
        //Support job dictionary
        private static readonly Dictionary<uint, string> _supportJobStatus = new()
        {
            { 4242, "Support Freelancer" },
            { 4358, "Support Knight" },
            { 4359, "Support Berserker" },
            { 4360, "Support Monk" },
            { 4361, "Support Hunter" },
            { 4362, "Support Samurai" },
            { 4363, "Support Bard" },
            { 4364, "Support Geomancer" },
            { 4365, "Support Time Mage" },
            { 4366, "Support Gunner" },
            { 4367, "Support Chemist" },
            { 4368, "Support Oracle" },
            { 4369, "Support Thief" }
        };
        // Dictionary for recording throw money counts and lock
        private readonly Dictionary<string, Dictionary<string, int>> _moneyThrowCounts = new();
        private readonly object _moneyThrowLock = new();
        // Dictionary for recording blue potion counts and lock
        private readonly Dictionary<string, Dictionary<string, int>> _bluePotionCounts = new();
        private readonly object _bluePotionLock = new();
        
        
        public void Init(ScriptAccessory accessory)
        {
            accessory.Log.Debug("Tower of Strength v0.0.36 script loaded.");
            accessory.Method.RemoveDraw(".*");

            _turnLeftRightCount = 0;
            // Initialize meteor mechanic state
            _hasCometeorStatus = false;
            _cometeorTargetId = 0;
            _isCasterInUpperHalf = null;
            // Ice-fire
            _blueCircles.Clear();
            _redCircles.Clear();
            _pairsProcessed = 0;
            // Snowball rush
            _snowballRushCastCount = 0;
            _letterGroupRushCount = 0;
            _numberGroupRushCount = 0;
            _letterGroupNextPos = null;
            _numberGroupNextPos = null;
            _tetherSourceId = 0;
            // Fireball/Geothermal
            _fireballPositions.Clear();
            // Third boss puddles
            _puddles.Clear();
            // Final boss
            _holyWeaponType = HolyWeaponType.None;
            lock(_sacredBowPreyLock)
            {
                _sacredBowPreyRecordedPlayers.Clear(); // Reset holy lance record
            }
            lock(_preyCheckLock)
            {
                _checkedPreyPlayers.Clear(); // Reset checked list
            }
            lock(_lanceShareLock)
            {
                _lanceShareAssignments.Clear(); // Reset lance share record
            }
            // Police mode
            lock(_moneyThrowLock)
            {
                _moneyThrowCounts.Clear();
            }
            lock(_bluePotionLock)
            {
                _bluePotionCounts.Clear();
            }

        }
        #region First Boss
        [ScriptMethod(
            name: "Initialize First Boss",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41734"],
            userControl: false
        )]
        public void OnInitializeBoss1Draw(Event @event, ScriptAccessory accessory)
        {
            // Initialize first boss state
            _hasCometeorStatus = false;
            _cometeorTargetId = 0;
            _isCasterInUpperHalf = null;
            _turnLeftRightCount = 0;
            // Clear previous drawings
            accessory.Method.RemoveDraw(".*");
            accessory.Log.Debug("First boss initialization complete.");
        }



        [ScriptMethod(
            name: "Landing (Draw)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(41812|43293|41709)$"]
        )]
        public void OnLandingDraw(Event @event, ScriptAccessory accessory)
        {
            var ActionId = @event.ActionId;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Landing_Danger_Zone";
            dp.Owner = @event.SourceId;
            switch (ActionId)
            {
                case 41812: //Landing
                    dp.Scale = new Vector2(30, 6);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 10500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                    break;
                case 43293: //Landing
                    dp.Scale = new Vector2(30, 15);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 10500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                    break;
                case 41709: //Landing
                    dp.Scale = new Vector2(18);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 8000;
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    break;
            }
        }

        [ScriptMethod(
            name: "Landing TTS (Circle)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:43293"],
            suppress: 1000
        )]
        public void OnLandingTTS(Event @event, ScriptAccessory accessory)
        {
            if (EnableTTS)
            {
                accessory.Method.EdgeTTS("Circle");
            }
        }

        [ScriptMethod(
            name: "Landing Knockback TTS",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(43794|43294)$"],
            suppress: 1000
        )]
        public void OnLandingKnockbackTTS(Event @event, ScriptAccessory accessory)
        {
            if (EnableTTS)
            {
                accessory.Method.EdgeTTS("Knockback");
            }
        }
        [ScriptMethod(
            name: "Landing (Knockback)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(43794|43294)$"]
        )]
        public void AethericBarrierKnockback(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Aetheric_Barrier_Knockback";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(30, 5);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 10500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            if(EnableTextBanner) accessory.Method.TextInfo("Knockback", 5000);
        }

        [ScriptMethod(
            name: "Tower Stepping (Guidance)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41720"],
            suppress: 1000
        )]
        public void GroupPositionGuide(Event @event, ScriptAccessory accessory)
        {
            Vector3 targetPosition = new Vector3(); // Default value

            // Select target coordinates based on user settings
            switch (SelectedStrategy)
            {
                case StrategySelection.ABC_123:
                    switch (MyTeam)
                    {
                        case TeamSelection.A: targetPosition = Pos_A; break;
                        case TeamSelection.B: targetPosition = Pos_B; break;
                        case TeamSelection.C: targetPosition = Pos_C; break;
                        case TeamSelection.One: targetPosition = Pos_One; break;
                        case TeamSelection.Two: targetPosition = Pos_Two; break;
                        case TeamSelection.Three: targetPosition = Pos_Three; break;
                    }
                    break;
                case StrategySelection.Pos_152463:
                    switch (MyPosition)
                    {
                        case PositionSelection.Pos2: targetPosition = Pos_A; break;
                        case PositionSelection.Pos5: targetPosition = Pos_B; break;
                        case PositionSelection.Pos1: targetPosition = Pos_C; break;
                        case PositionSelection.Pos4: targetPosition = Pos_One; break;
                        case PositionSelection.Pos6: targetPosition = Pos_Two; break;
                        case PositionSelection.Pos3: targetPosition = Pos_Three; break;
                    }
                    break;
                case StrategySelection.LemonCookie:
                    // LemonCookie grouping strategy
                    // 152463 group number to LemonCookie group number mapping: 1->3, 2->1, 3->6, 4->4, 5->2, 6->5
                    switch (MyLemonCookiePosition) // Use 152463 position selector to represent LemonCookie group number
                    {
                        case PositionSelection.Pos1: // LemonCookie group 1 -> corresponds to 152463 group 2 position
                            targetPosition = Pos_A; // 152463 group 2 corresponds to Pos_A
                            break;
                        case PositionSelection.Pos2: // LemonCookie group 2 -> corresponds to 152463 group 5 position
                            targetPosition = Pos_B; // 152463 group 5 corresponds to Pos_B
                            break;
                        case PositionSelection.Pos3: // LemonCookie group 3 -> corresponds to 152463 group 1 position
                            targetPosition = Pos_C; // 152463 group 1 corresponds to Pos_C
                            break;
                        case PositionSelection.Pos4: // LemonCookie group 4 -> corresponds to 152463 group 4 position
                            targetPosition = Pos_One; // 152463 group 4 corresponds to Pos_One
                            break;
                        case PositionSelection.Pos5: // LemonCookie group 5 -> corresponds to 152463 group 6 position
                            targetPosition = Pos_Two; // 152463 group 6 corresponds to Pos_Two
                            break;
                        case PositionSelection.Pos6: // LemonCookie group 6 -> corresponds to 152463 group 3 position
                            targetPosition = Pos_Three; // 152463 group 3 corresponds to Pos_Three
                            break;
                    }

                    break;
            }

            // Draw arrow pointing to target position
            var dp = accessory.Data.GetDefaultDrawProperties();
            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"Group_Position_Guide_{MyTeam}";
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = targetPosition;
            dp.Scale = new Vector2(1.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 15000;
            dp2.Color = new Vector4(0, 1, 0, 0.6f); // Green
            dp2.Scale = new Vector2(4);
            dp2.DestoryAt = 15000;
            dp2.Position = targetPosition;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp2 );

            if (Enable_Developer_Mode)
            {
                accessory.Log.Debug($"Drawing position guidance for team {MyTeam}, pointing to coordinates {targetPosition}");
            }
        }
        [ScriptMethod(
            name: "Rotation",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41731"]
        )]
        public void OnRotationDraw(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Rotation_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(37);
            dp.Radian = 90f * MathF.PI / 180f;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 8800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        [ScriptMethod(
            name: "Left/Right Turn",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(41729|41730)$"]
        )]
        public void OnTurnLeftRightDraw(Event @event, ScriptAccessory accessory)
        {
            _turnLeftRightCount++;
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = "TurnLeftRight1_Danger_Zone";
            dp1.Position = @event.SourcePosition;
            dp1.Scale = new Vector2(66, 6);
            dp1.Color = accessory.Data.DefaultDangerColor;
            dp1.DestoryAt = 8800;
            dp2.Name = "TurnLeftRight2_Danger_Zone";
            dp2.Position = @event.SourcePosition;
            dp2.Scale = new Vector2(66, 6);
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.DestoryAt = 8800;
            dp2.Rotation = MathF.PI / 2f;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp2);
            if (_turnLeftRightCount == 2 || _turnLeftRightCount == 4)
            {
                var dpNorth = accessory.Data.GetDefaultDrawProperties();
                dpNorth.Name = $"TurnLeftRight_North_Danger_Zone_{_turnLeftRightCount}";
                dpNorth.Position = Boss1ArenaCenter;
                dpNorth.Rotation = MathF.PI; // Point north
                dpNorth.Scale = new Vector2(30, 33);
                dpNorth.Color = accessory.Data.DefaultDangerColor;
                dpNorth.DestoryAt = 8800;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dpNorth);
            }
            else if (_turnLeftRightCount == 6)
            {
                var dpSouth = accessory.Data.GetDefaultDrawProperties();
                dpSouth.Name = "TurnLeftRight_South_Danger_Zone";
                dpSouth.Position = Boss1ArenaCenter;
                dpSouth.Rotation = 0; // Point south
                dpSouth.Scale = new Vector2(30, 33);
                dpSouth.Color = accessory.Data.DefaultDangerColor;
                dpSouth.DestoryAt = 8800;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dpSouth);
            }
        }

        [ScriptMethod(
            name: "Stack Mark (South Side)",
            eventType: EventTypeEnum.TargetIcon,
            eventCondition: ["Id:023E"]
        )]
        public void OnSouthStack(Event @event, ScriptAccessory accessory)
        {
            if (PoliceMode)
            {
                var target = accessory.Data.Objects.SearchById(@event.TargetId);
                if (target != null)
                {
                    accessory.Method.SendChat($"/e Stack (south side) mark: {target.Name}");
                }
            }
            if (@event.TargetId != accessory.Data.Me) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "South_Stack";
            dp.Position = Boss1ArenaCenter;
            dp.Rotation = MathF.PI;
            dp.Scale = new Vector2(30, 33);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 12000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            if (EnableTTS)
            {
                accessory.Method.EdgeTTS("Stack on south side");
            }
        }

        [ScriptMethod(
            name: "Stack Mark (North Side)",
            eventType: EventTypeEnum.TargetIcon,
            eventCondition: ["Id:023F"]
        )]
        public void OnNorthStack(Event @event, ScriptAccessory accessory)
        {
            if (PoliceMode)
            {
                var target = accessory.Data.Objects.SearchById(@event.TargetId);
                if (target != null)
                {
                    accessory.Method.SendChat($"/e Stack (north side) mark: {target.Name}");
                }
            }
            if (@event.TargetId != accessory.Data.Me) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "North_Stack";
            dp.Position = Boss1ArenaCenter;
            dp.Rotation = 0;
            dp.Scale = new Vector2(30, 33);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 12000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            if (EnableTTS)
            {
                accessory.Method.EdgeTTS("Stack on north side");
            }
        }
        [ScriptMethod(
            name: "Meteor 1 (Guidance)",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:4354"]
        )]
        public void OnCometeorStatusAdd(Event @event, ScriptAccessory accessory)
        {
            if (PoliceMode)
            {
                var target = accessory.Data.Objects.SearchById(@event.TargetId);
                if (target != null)
                {
                    accessory.Method.SendChat($"/e Meteor mark: {target.Name}");
                }
            }
            if (@event.TargetId != accessory.Data.Me) return;
            _hasCometeorStatus = true;
            if (EnableTTS)
            {
                accessory.Method.EdgeTTS("Meteor mark");
            }
            if (Enable_Developer_Mode) accessory.Log.Debug("Meteor mechanic: Player gained status.");
            TryDrawCometeorGuide(accessory);
        }
        [ScriptMethod(
            name: "Meteor Guidance - Status Removal",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:4354"],
            userControl: false
        )]
        public void OnCometeorStatusRemove(Event @event, ScriptAccessory accessory)
        {
            if (@event.TargetId != accessory.Data.Me) return;
            _hasCometeorStatus = false;
            accessory.Method.RemoveDraw("Cometeor_Guide");
            if (Enable_Developer_Mode) accessory.Log.Debug("Meteor mechanic: Player status removed, removing guidance.");
        }

        [ScriptMethod(
            name: "Meteor 2 (Guidance)",
            eventType: EventTypeEnum.ObjectChanged,
            eventCondition: ["Operate:Add", "DataId:2014582"]
        )]
        public void OnCometeorTargetSpawn(Event @event, ScriptAccessory accessory)
        {
            _cometeorTargetId = @event.SourceId;
            if (Enable_Developer_Mode) accessory.Log.Debug($"Meteor mechanic: Target unit (PortentousCometeor) appeared, ID: {@event.SourceId}.");
            TryDrawCometeorGuide(accessory);
        }
        private void TryDrawCometeorGuide(ScriptAccessory accessory)
        {
            // Only draw when the player has the status and the target exists
            if (_hasCometeorStatus && _cometeorTargetId != 0)
            {
                if (Enable_Developer_Mode) accessory.Log.Debug("Meteor mechanic: Conditions met, starting to draw guidance.");

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "Cometeor_Guide";
                dp.Owner = accessory.Data.Me;
                dp.TargetObject = _cometeorTargetId;
                dp.Scale = new Vector2(1.5f);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 12000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        [ScriptMethod(
            name: "Meteor 3 (Initialization)",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:41702"],
            userControl: false
        )]
        public void OnCometeorActionEffect(Event @event, ScriptAccessory accessory)
        {
            // Initialize meteor mechanic state
            _hasCometeorStatus = false;
            _cometeorTargetId = 0;
        }

        [ScriptMethod(
            name: "Summon (Guidance)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41741"]
        )]
        public void SummonGuide(Event @event, ScriptAccessory accessory)
        {
            Vector3 targetPosition = new Vector3();
            var letterGroupPos = new Vector3(700.24f, -481.00f, 360.46f);
            var numberGroupPos = new Vector3(700.02f, -481.00f, 398.08f);

            switch (SelectedStrategy)
            {
                case StrategySelection.ABC_123:
                    if (MyTeam == TeamSelection.A || MyTeam == TeamSelection.B || MyTeam == TeamSelection.C)
                    {
                        targetPosition = letterGroupPos;
                    }
                    else // One, Two, Three
                    {
                        targetPosition = numberGroupPos;
                    }
                    break;
                case StrategySelection.Pos_152463:
                    switch (MyPosition)
                    {
                        case PositionSelection.Pos1:
                        case PositionSelection.Pos5:
                        case PositionSelection.Pos2:
                            targetPosition = letterGroupPos;
                            break;
                        case PositionSelection.Pos4:
                        case PositionSelection.Pos6:
                        case PositionSelection.Pos3:
                            targetPosition = numberGroupPos;
                            break;
                    }
                    break;
                case StrategySelection.LemonCookie:
                    switch (MyLemonCookiePosition)
                    {
                        case PositionSelection.Pos1:
                        case PositionSelection.Pos2:
                        case PositionSelection.Pos3:
                            targetPosition = letterGroupPos;
                            break;
                        case PositionSelection.Pos4:
                        case PositionSelection.Pos5:
                        case PositionSelection.Pos6:
                            targetPosition = numberGroupPos;
                            break;
                    }
                    break;
            }

            // Draw guidance
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Summon_Guide";
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = targetPosition;
            dp.Scale = new Vector2(1.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            if (Enable_Developer_Mode)
            {
                accessory.Log.Debug($"Drawing summon gathering point for team {MyTeam}, pointing to {targetPosition}");
            }
        }
        //position.Z = 379 is the dividing line for upper/lower half
        [ScriptMethod(
            name: "Floating Tower Guidance - Record Half",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(41713|41711)$"],
            userControl: false
        )]
        public void HalfArenaRecord(Event @event, ScriptAccessory accessory)
        {
            var caster = accessory.Data.Objects.SearchById(@event.SourceId);
            if (caster == null) return;

            _isCasterInUpperHalf = caster.Position.Z > ArenaCenterZ;

            if (Enable_Developer_Mode)
            {
                accessory.Log.Debug($"Half record: Caster is in {(_isCasterInUpperHalf.Value ? "upper half" : "lower half")}");
            }
        }
        [ScriptMethod(
            name: "Letter Team Floating Tower (Guidance)",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:41707"],
            suppress: 1000
        )]
        public void FloatingTowerGuide(Event @event, ScriptAccessory accessory)
        {
            if (_isCasterInUpperHalf == null)
            {
                if (Enable_Developer_Mode) accessory.Log.Error("Floating tower guidance: Failed to obtain previous half information.");
                return;
            }

            Vector3 targetPosition = new Vector3();
            bool shouldDraw = false;

            switch (SelectedStrategy)
            {
                case StrategySelection.ABC_123:
                    if (MyTeam == TeamSelection.A || MyTeam == TeamSelection.B || MyTeam == TeamSelection.C)
                    {
                        shouldDraw = true;
                        switch (MyTeam)
                        {
                            case TeamSelection.A: targetPosition = _isCasterInUpperHalf.Value ? Pos_One : Pos_A; break;
                            case TeamSelection.B: targetPosition = _isCasterInUpperHalf.Value ? Pos_Two : Pos_B; break;
                            case TeamSelection.C: targetPosition = _isCasterInUpperHalf.Value ? Pos_Three : Pos_C; break;
                        }
                    }
                    break;

                case StrategySelection.Pos_152463:
                    switch (MyPosition)
                    {
                        case PositionSelection.Pos2: // Corresponds to A
                            shouldDraw = true;
                            targetPosition = _isCasterInUpperHalf.Value ? Pos_One : Pos_A;
                            break;
                        case PositionSelection.Pos5: // Corresponds to B
                            shouldDraw = true;
                            targetPosition = _isCasterInUpperHalf.Value ? Pos_Two : Pos_B;
                            break;
                        case PositionSelection.Pos1: // Corresponds to C
                            shouldDraw = true;
                            targetPosition = _isCasterInUpperHalf.Value ? Pos_Three : Pos_C;
                            break;
                    }
                    break;
                case StrategySelection.LemonCookie:
                    switch (MyLemonCookiePosition)
                    {
                        case PositionSelection.Pos1: // Corresponds to A
                            shouldDraw = true;
                            targetPosition = _isCasterInUpperHalf.Value ? Pos_One : Pos_A;
                            break;
                        case PositionSelection.Pos2: // Corresponds to B
                            shouldDraw = true;
                            targetPosition = _isCasterInUpperHalf.Value ? Pos_Two : Pos_B;
                            break;
                        case PositionSelection.Pos3: // Corresponds to C
                            shouldDraw = true;
                            targetPosition = _isCasterInUpperHalf.Value ? Pos_Three : Pos_C;
                            break;
                    }
                    break;
            }

            if (shouldDraw)
            {
                var dpGuide = accessory.Data.GetDefaultDrawProperties();
                dpGuide.Name = $"Floating_Tower_Guide_Arrow";
                dpGuide.Owner = accessory.Data.Me;
                dpGuide.TargetPosition = targetPosition;
                dpGuide.Scale = new Vector2(1.5f);
                dpGuide.ScaleMode |= ScaleMode.YByDistance;
                dpGuide.Color = accessory.Data.DefaultSafeColor;
                dpGuide.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dpGuide);

                var dpCircle = accessory.Data.GetDefaultDrawProperties();
                dpCircle.Name = $"Floating_Tower_Guide_Circle";
                dpCircle.Position = targetPosition;
                dpCircle.Scale = new Vector2(4);
                dpCircle.Color = new Vector4(0, 1, 0, 0.6f); // Green
                dpCircle.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dpCircle);
            }

            _isCasterInUpperHalf = null;
        }
        [ScriptMethod(
            name: "Number Team Ground Tower (Guidance)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(41713|41711)$"],
            suppress: 1000
        )]
        public void GroundTowerGuide(Event @event, ScriptAccessory accessory)
        {
            var caster = accessory.Data.Objects.SearchById(@event.SourceId);
            if (caster == null) return;

            bool isCasterInUpperHalf = caster.Position.Z > ArenaCenterZ;
            Vector3 targetPosition = new Vector3();
            bool shouldDraw = false;

            switch (SelectedStrategy)
            {
                case StrategySelection.ABC_123:
                    if (MyTeam == TeamSelection.One || MyTeam == TeamSelection.Two || MyTeam == TeamSelection.Three)
                    {
                        shouldDraw = true;
                        switch (MyTeam)
                        {
                            case TeamSelection.One: targetPosition = isCasterInUpperHalf ? Pos_One : Pos_A; break;
                            case TeamSelection.Two: targetPosition = isCasterInUpperHalf ? Pos_Two : Pos_B; break;
                            case TeamSelection.Three: targetPosition = isCasterInUpperHalf ? Pos_Three : Pos_C; break;
                        }
                    }
                    break;
                case StrategySelection.Pos_152463:
                    if (MyPosition == PositionSelection.Pos3 || MyPosition == PositionSelection.Pos4 || MyPosition == PositionSelection.Pos6)
                    {
                        shouldDraw = true;
                        switch (MyPosition)
                        {
                            case PositionSelection.Pos4: targetPosition = isCasterInUpperHalf ? Pos_One : Pos_A; break;
                            case PositionSelection.Pos6: targetPosition = isCasterInUpperHalf ? Pos_Two : Pos_B; break;
                            case PositionSelection.Pos3: targetPosition = isCasterInUpperHalf ? Pos_Three : Pos_C; break;
                        }
                    }
                    break;
                case StrategySelection.LemonCookie:
                    if (MyLemonCookiePosition == PositionSelection.Pos4 ||
                        MyLemonCookiePosition == PositionSelection.Pos5 ||
                        MyLemonCookiePosition == PositionSelection.Pos6)
                    {
                        shouldDraw = true;
                        switch (MyLemonCookiePosition)
                        {
                            case PositionSelection.Pos4: targetPosition = isCasterInUpperHalf ? Pos_One : Pos_A; break;
                            case PositionSelection.Pos5: targetPosition = isCasterInUpperHalf ? Pos_Two : Pos_B; break;
                            case PositionSelection.Pos6: targetPosition = isCasterInUpperHalf ? Pos_Three : Pos_C; break;
                        }
                    }
                    break;
            }

            if (shouldDraw)
            {
                var dpGuide = accessory.Data.GetDefaultDrawProperties();
                dpGuide.Name = $"Ground_Tower_Guide_Arrow";
                dpGuide.Owner = accessory.Data.Me;
                dpGuide.TargetPosition = targetPosition;
                dpGuide.Scale = new Vector2(1.5f);
                dpGuide.ScaleMode |= ScaleMode.YByDistance;
                dpGuide.Color = accessory.Data.DefaultSafeColor;
                dpGuide.DestoryAt = 21000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dpGuide);

                var dpCircle = accessory.Data.GetDefaultDrawProperties();
                dpCircle.Name = $"Ground_Tower_Guide_Circle";
                dpCircle.Position = targetPosition;
                dpCircle.Scale = new Vector2(4);
                dpCircle.Color = new Vector4(0, 1, 0, 0.6f); // Green
                dpCircle.DestoryAt = 21000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dpCircle);
            }
        }
        [ScriptMethod(
            name: "Floating (Guidance)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41707"]
        )]
        public void AbcTeamSafeZone(Event @event, ScriptAccessory accessory)
        {
            bool shouldDraw = false;
            switch (SelectedStrategy)
            {
                case StrategySelection.ABC_123:
                    if (MyTeam == TeamSelection.A || MyTeam == TeamSelection.B || MyTeam == TeamSelection.C)
                    {
                        shouldDraw = true;
                    }
                    break;
                case StrategySelection.Pos_152463:
                    if (MyPosition == PositionSelection.Pos1 || MyPosition == PositionSelection.Pos2 || MyPosition == PositionSelection.Pos5)
                    {
                        shouldDraw = true;
                    }
                    break;
                case StrategySelection.LemonCookie:
                    if (MyLemonCookiePosition == PositionSelection.Pos1 || MyLemonCookiePosition == PositionSelection.Pos2 || MyLemonCookiePosition == PositionSelection.Pos3)
                    {
                        shouldDraw = true;
                    }
                    break;
            }

            if (!shouldDraw) return;

            var caster = accessory.Data.Objects.SearchById(@event.SourceId);
            if (caster == null) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Abc_Safe_Zone";
            dp.Owner = caster.EntityId;
            dp.Scale = new Vector2(4);
            dp.Color = new Vector4(0, 1, 0, 0.6f);
            dp.DestoryAt = 14000;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

            if (Enable_Developer_Mode)
            {
                accessory.Log.Debug("Drawing safe zone for floating tower group.");
            }
        }
        #endregion
        #region Second Boss
        [ScriptMethod(
            name: "Initialize Second Boss",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:42492"],
            userControl: false
        )]
        public void OnInitializeBoss2Draw(Event @event, ScriptAccessory accessory)
        {
            // Initialize second boss state
            _blueCircles.Clear();
            _redCircles.Clear();
            _pairsProcessed = 0;
            // Snowball rush
            _snowballRushCastCount = 0;
            _letterGroupRushCount = 0;
            _numberGroupRushCount = 0;
            _letterGroupNextPos = null;
            _numberGroupNextPos = null;
            _tetherSourceId = 0;
            // Fireball/Geothermal
            _fireballPositions.Clear();
            accessory.Method.RemoveDraw(".*");
            if(Enable_Developer_Mode) accessory.Log.Debug("Second boss initialization complete.");
        }

        [ScriptMethod(
            name: "Slice and Dice",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:42498"]
        )]
        public void OnSliceNDiceDraw(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Slice_N_Dice_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(70);
            dp.Radian = 90f * MathF.PI / 180f;
            dp.TargetObject = @event.TargetId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(
            name: "Vengeful Blaze/Freeze/Poison",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(42429|42430|42431)$"]
        )]
        public void OnVengeDraw(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Revenge_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(60);
            dp.Radian = 120f * MathF.PI / 180f;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(
            name: "Ice-Fire Circles - Record Telegraph",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(42464|42463)$"]
        )]
        public void PrimordialChaosTelegraph(Event @event, ScriptAccessory accessory)
        {
            // Use lock to ensure thread safety
            lock (_iceFireLock)
            {
                bool isBlue = @event.ActionId == 42464;
                var position = @event.EffectPosition;

                // Store position in corresponding list based on color
                if (isBlue)
                {
                    _blueCircles.Add(position);
                }
                else
                {
                    _redCircles.Add(position);
                }

                // Try to process and draw paired AOEs
                ProcessAoePairs(accessory);
            }
        }

        private void ProcessAoePairs(ScriptAccessory accessory)
        {
            // As long as both lists have circles, they can form a pair
            while (_blueCircles.Count > 0 && _redCircles.Count > 0)
            {
                // Take the first circle from each list
                var bluePos = _blueCircles[0];
                var redPos = _redCircles[0];

                // Calculate explosion time point
                int explosionTime = 11000 + _pairsProcessed * 5500;
                // Fixed display duration
                const int displayDuration = 7000;
                // Calculate telegraph circle trigger time point (assuming each telegraph circle triggers every 2500ms)
                int triggerTime = _pairsProcessed * 2500;
                // Calculate the delay needed for drawing
                int delay = (explosionTime - displayDuration) - triggerTime;
                if (delay < 0) delay = 0; // Ensure delay is not negative
                var dpBlue = accessory.Data.GetDefaultDrawProperties();
                dpBlue.Name = $"PrimordialChaos_Blue_{_pairsProcessed}";
                dpBlue.Position = bluePos;
                dpBlue.Scale = new Vector2(22);
                dpBlue.Color = new Vector4(0.2f, 0.5f, 1f, 2.0f);
                dpBlue.Delay = delay;
                dpBlue.DestoryAt = displayDuration;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dpBlue);

                // Draw red circle
                var dpRed = accessory.Data.GetDefaultDrawProperties();
                dpRed.Name = $"PrimordialChaos_Red_{_pairsProcessed}";
                dpRed.Position = redPos;
                dpRed.Scale = new Vector2(22);
                dpRed.Color = new Vector4(1f, 0.2f, 0.2f, 2.0f);
                dpRed.Delay = delay;
                dpRed.DestoryAt = displayDuration;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dpRed);

                if(Enable_Developer_Mode) accessory.Log.Debug($"Drawing pair {_pairsProcessed + 1} of ice-fire circles");

                // Remove processed circles from lists and increment counter
                _blueCircles.RemoveAt(0);
                _redCircles.RemoveAt(0);
                _pairsProcessed++;
            }
        }

        [ScriptMethod(
            name: "Snowball Rush (Guidance)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:42447"]
        )]
        public void SnowballRush(Event @event, ScriptAccessory accessory)
        {
            lock (_snowballLock)
            {
                var sourcePos = @event.SourcePosition;
                var nextPos = @event.EffectPosition;

                bool isLetterGroup = false;
                int currentGroupRushCount = 0;

                // First (first two) casts, for grouping
                if (_snowballRushCastCount < 2)
                {
                    if (Vector3.DistanceSquared(sourcePos, InitialPosLetterGroup) < Vector3.DistanceSquared(sourcePos, InitialPosNumberGroup))
                    {
                        isLetterGroup = true;
                        _letterGroupNextPos = nextPos;
                        currentGroupRushCount = _letterGroupRushCount++;
                        if(Enable_Developer_Mode) accessory.Log.Debug("Snowball rush: Letter group confirmed.");
                    }
                    else
                    {
                        isLetterGroup = false;
                        _numberGroupNextPos = nextPos;
                        currentGroupRushCount = _numberGroupRushCount++;
                        if(Enable_Developer_Mode) accessory.Log.Debug("Snowball rush: Number group confirmed.");
                    }
                }
                // Subsequent casts, for path tracking
                else
                {
                    if (_letterGroupNextPos.HasValue && Vector3.DistanceSquared(sourcePos, _letterGroupNextPos.Value) < 1.0f)
                    {
                        isLetterGroup = true;
                        _letterGroupNextPos = nextPos;
                        currentGroupRushCount = _letterGroupRushCount++;
                        if(Enable_Developer_Mode) accessory.Log.Debug("Snowball rush: Letter group path updated.");
                    }
                    else if (_numberGroupNextPos.HasValue && Vector3.DistanceSquared(sourcePos, _numberGroupNextPos.Value) < 1.0f)
                    {
                        isLetterGroup = false;
                        _numberGroupNextPos = nextPos;
                        currentGroupRushCount = _numberGroupRushCount++;
                        if(Enable_Developer_Mode) accessory.Log.Debug("Snowball rush: Number group path updated.");
                    }
                    else
                    {
                        // Fallback in case something goes wrong
                        if(Enable_Developer_Mode) accessory.Log.Error("Snowball rush: Unable to match path.");
                        return;
                    }
                }

                // --- Color determination logic ---
                bool isSafe = false;
                // currentGroupRushCount starts from 0 (0=1st, 1=2nd, 2=3rd)
                switch (SelectedStrategy)
                {
                    case StrategySelection.ABC_123:
                        if (isLetterGroup)
                        {
                            if ((MyTeam == TeamSelection.A && currentGroupRushCount == 0) ||
                                (MyTeam == TeamSelection.B && currentGroupRushCount == 1) ||
                                (MyTeam == TeamSelection.C && currentGroupRushCount == 2))
                            {
                                isSafe = true;
                            }
                        }
                        else // isNumberGroup
                        {
                            if ((MyTeam == TeamSelection.One && currentGroupRushCount == 0) ||
                                (MyTeam == TeamSelection.Two && currentGroupRushCount == 1) ||
                                (MyTeam == TeamSelection.Three && currentGroupRushCount == 2))
                            {
                                isSafe = true;
                            }
                        }
                        break;

                    case StrategySelection.Pos_152463:
                        if (isLetterGroup) // Group 1 (A), Group 2 (B), Group 3 (C)
                        {
                            if ((MyPosition == PositionSelection.Pos1 && currentGroupRushCount == 0) || // Group 1 corresponds to A group (1st)
                                (MyPosition == PositionSelection.Pos2 && currentGroupRushCount == 1) || // Group 2 corresponds to B group (2nd)
                                (MyPosition == PositionSelection.Pos3 && currentGroupRushCount == 2))   // Group 3 corresponds to C group (3rd)
                            {
                                isSafe = true;
                            }
                        }
                        else // isNumberGroup Group 4 (One), Group 5 (Two), Group 6 (Three)
                        {
                            if ((MyPosition == PositionSelection.Pos4 && currentGroupRushCount == 0) || // Group 4 corresponds to One group (1st)
                                (MyPosition == PositionSelection.Pos5 && currentGroupRushCount == 1) || // Group 5 corresponds to Two group (2nd)
                                (MyPosition == PositionSelection.Pos6 && currentGroupRushCount == 2))   // Group 6 corresponds to Three group (3rd)
                            {
                                isSafe = true;
                            }
                        }
                        break;
                    case StrategySelection.LemonCookie:
                        if (isLetterGroup) // Group 1 (A), Group 2 (B), Group 3 (C)
                        {
                            if ((MyLemonCookiePosition == PositionSelection.Pos1 && currentGroupRushCount == 0) || // Group 1 corresponds to A group (1st)
                                (MyLemonCookiePosition == PositionSelection.Pos2 && currentGroupRushCount == 1) || // Group 2 corresponds to B group (2nd)
                                (MyLemonCookiePosition == PositionSelection.Pos3 && currentGroupRushCount == 2))   // Group 3 corresponds to C group (3rd)
                            {
                                isSafe = true;
                            }
                        }
                        else // isNumberGroup Group 4 (One), Group 5 (Two), Group 6 (Three)
                        {
                            if ((MyLemonCookiePosition == PositionSelection.Pos4 && currentGroupRushCount == 0) || // Group 4 corresponds to One group (1st)
                                (MyLemonCookiePosition == PositionSelection.Pos5 && currentGroupRushCount == 1) || // Group 5 corresponds to Two group (2nd)
                                (MyLemonCookiePosition == PositionSelection.Pos6 && currentGroupRushCount == 2))   // Group 6 corresponds to Three group (3rd)
                            {
                                isSafe = true;
                            }
                        }
                        break;
                }

                var color = isSafe ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;

                // Draw rectangular AOE
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"SnowballRush_{_snowballRushCastCount}";
                dp.Position = sourcePos;
                dp.TargetPosition = nextPos;
                dp.Scale = new Vector2(10); // Width 10
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = color; // Use calculated color
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);

                _snowballRushCastCount++;
            }
        }
        [ScriptMethod(
            name: "Glacial Impact - Record Tether Source",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:00F6"],
            userControl: false
        )]
        public void OnGlacialImpactTether(Event @event, ScriptAccessory accessory)
        {
            if (PoliceMode)
            {
                var target = accessory.Data.Objects.SearchById(@event.TargetId);
                if (target != null)
                {
                    accessory.Method.SendChat($"/e Snowball tether mark: {target.Name}");
                }
            }
            if (@event.TargetId == accessory.Data.Me)
            {
                _tetherSourceId = @event.SourceId;
                if(Enable_Developer_Mode) accessory.Log.Debug($"Glacial Impact: Player tethered, source ID: {_tetherSourceId}");
            }
        }
        [ScriptMethod(
            name: "Glacial Impact (Guidance)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:42451"]
        )]
        public async void GlacialImpact(Event @event, ScriptAccessory accessory)
        {
            await Task.Delay(1000);
            if(Enable_Developer_Mode) accessory.Log.Debug("Glacial Impact: Starting cast, triggering guidance drawing.");
            DrawGlacialImpactGuide(accessory);
        }



        private void DrawGlacialImpactGuide(ScriptAccessory accessory)
        {
            Vector3? safePosition = null;
            Vector3? finalDropPos = null;

            // Prioritize tether situation
            if (_tetherSourceId != 0)
            {
                var tetherSource = accessory.Data.Objects.SearchById(_tetherSourceId);
                if (tetherSource != null)
                {
                    var direction = Vector3.Normalize(SnowballArenaCenter - tetherSource.Position);
                    safePosition = SnowballArenaCenter + direction * 5;
                    if(Enable_Developer_Mode) accessory.Log.Debug("Glacial Impact: Detected tether, calculating special safe point.");
                }
                else
                {
                    if(Enable_Developer_Mode) accessory.Log.Error("Glacial Impact: Cannot find tether source unit.");
                }
            }
            else // If not tethered, execute grouping logic
            {
                switch (SelectedStrategy)
                {
                    case StrategySelection.ABC_123:
                        bool isUserInLetterGroup = MyTeam == TeamSelection.A || MyTeam == TeamSelection.B || MyTeam == TeamSelection.C;
                        finalDropPos = isUserInLetterGroup ? _letterGroupNextPos : _numberGroupNextPos;
                        break;

                    case StrategySelection.Pos_152463:
                        bool isPosInLetterGroup = MyPosition == PositionSelection.Pos1 || MyPosition == PositionSelection.Pos2 || MyPosition == PositionSelection.Pos3;
                        finalDropPos = isPosInLetterGroup ? _letterGroupNextPos : _numberGroupNextPos;
                        break;
                    case StrategySelection.LemonCookie:
                        bool isLemonPosInLetterGroup = MyLemonCookiePosition == PositionSelection.Pos4 || MyLemonCookiePosition == PositionSelection.Pos5 || MyLemonCookiePosition == PositionSelection.Pos6;
                        finalDropPos = isLemonPosInLetterGroup ? _letterGroupNextPos : _numberGroupNextPos;
                        break;
                }

                if (finalDropPos != null)
                {
                    var direction = Vector3.Normalize(finalDropPos.Value - SnowballArenaCenter);
                    safePosition = SnowballArenaCenter - direction * 5;
                }
                else
                {
                    if(Enable_Developer_Mode) accessory.Log.Error("Glacial Impact: Cannot find snowball final landing point.");
                }
            }

            // If safe point successfully calculated, draw guidance
            if (safePosition.HasValue)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "GlacialImpact_Guide";
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = safePosition.Value;
                dp.Scale = new Vector2(1.5f);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }

        [ScriptMethod(
            name: "Glacial Impact - End",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:42451"],
            userControl: false
        )]
        public void GlacialImpactEnd(Event @event, ScriptAccessory accessory)
        {
            // Reset tether state after mechanic ends
            _tetherSourceId = 0;
            if(Enable_Developer_Mode) accessory.Log.Debug("Glacial Impact: Mechanic ended, resetting tether state.");
        }

        //Fireball tower DataId=2014637

        [ScriptMethod(
            name: "Fireball Pre-position (Guidance)",
            eventType: EventTypeEnum.ObjectChanged,
            eventCondition: ["Operate:Add", "DataId:2014637"]
        )]
        public void FireballPrePosition(Event @event, ScriptAccessory accessory)
        {
            // Use lock to ensure thread safety
            lock (_fireballLock)
            {
                _fireballPositions.Clear();
                var fireballs = accessory.Data.Objects.Where(o => o.DataId == 2014637).ToList();
                if (!fireballs.Any()) return;

                foreach (var fireball in fireballs)
                {
                    _fireballPositions.Add(fireball.Position);
                }

                // Draw guidance based on newly recorded positions
                ProcessFireballs(accessory);
            }
        }


        [ScriptMethod(
            name: "Fireball Safe Point Redraw (Guidance)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:42434"],
            suppress: 2000
        )]
        public void RedrawFireballGuides(Event @event, ScriptAccessory accessory)
        {
            // Use stored positions from the first event to redraw
            if (!_fireballPositions.Any())
            {
                if(Enable_Developer_Mode) accessory.Log.Error("Fireball redraw: Cannot find first round fireball coordinates.");
                return;
            }
            ProcessFireballs(accessory);
        }
        private void ProcessFireballs(ScriptAccessory accessory)
        {
            bool isUserInLetterGroup = MyTeam == TeamSelection.A || MyTeam == TeamSelection.B || MyTeam == TeamSelection.C;
            switch (SelectedStrategy)
            {
                case StrategySelection.ABC_123:
                    isUserInLetterGroup = MyTeam == TeamSelection.A || MyTeam == TeamSelection.B || MyTeam == TeamSelection.C;
                    break;
                case StrategySelection.Pos_152463:
                    isUserInLetterGroup = MyPosition == PositionSelection.Pos1 || MyPosition == PositionSelection.Pos2 || MyPosition == PositionSelection.Pos3;
                    break;
                case StrategySelection.LemonCookie:
                    isUserInLetterGroup = MyLemonCookiePosition == PositionSelection.Pos1 || MyLemonCookiePosition == PositionSelection.Pos2 || MyLemonCookiePosition == PositionSelection.Pos3;
                    break;
            }
            int fireballIndex = 0;
            foreach (var fireballPos in _fireballPositions)
            {
                bool isLetterFireball = LetterGroupFireballCoords.Any(coord => Vector3.DistanceSquared(fireballPos, coord) < 1.0f);
                bool isNumberFireball = NumberGroupFireballCoords.Any(coord => Vector3.DistanceSquared(fireballPos, coord) < 1.0f);

                if ((isUserInLetterGroup && isLetterFireball) || (!isUserInLetterGroup && isNumberFireball))
                {
                    DrawPrePositionGuides(accessory, fireballPos, fireballIndex);
                }
                fireballIndex++;
            }
        }

        private void DrawPrePositionGuides(ScriptAccessory accessory, Vector3 fireballPos, int uniqueId)
        {
            var player = accessory.Data.MyObject;
            if (player == null) return;
            var directionToFireball = Vector3.Normalize(fireballPos - SnowballArenaCenter);

            if (IsDps(player))
            {
                var safePos = fireballPos + directionToFireball * 6;
                var dp = accessory.Data.GetDefaultDrawProperties();
                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Fireball_DPS_SafeZone_{uniqueId}";
                dp.Position = safePos;
                dp.Scale = new Vector2(1);
                dp.Color = new Vector4(0, 1, 0, 0.6f); // Green
                dp.DestoryAt = 10000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
                dp2.Name = "Guide_to_safepos";
                dp2.Scale = new Vector2(1);
                dp2.Owner = accessory.Data.Me;
                dp2.Color = new Vector4(0, 1, 0, 0.6f);
                dp2.ScaleMode |= ScaleMode.YByDistance;
                dp2.DestoryAt = 10000;
                dp2.TargetPosition = safePos;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
            }
            else if (IsHealer(player))
            {
                var perpendicularDir1 = new Vector3(-directionToFireball.Z, 0, directionToFireball.X);
                var perpendicularDir2 = new Vector3(directionToFireball.Z, 0, -directionToFireball.X);
                var safePos1 = fireballPos + perpendicularDir1 * 6;
                var safePos2 = fireballPos + perpendicularDir2 * 6;

                var dp1 = accessory.Data.GetDefaultDrawProperties();
                dp1.Name = $"Fireball_Healer_SafeZone1_{uniqueId}";
                dp1.Position = safePos2;
                dp1.Scale = new Vector2(1);
                dp1.Color = new Vector4(0, 1, 0, 0.6f); // Green
                dp1.DestoryAt = 10000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp1);
                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = "Guide_to_safepos";
                dp2.Scale = new Vector2(1);
                dp2.Owner = accessory.Data.Me;
                dp2.Color = new Vector4(0, 1, 0, 0.6f);
                dp2.DestoryAt = 10000;
                dp2.ScaleMode |= ScaleMode.YByDistance;
                dp2.TargetPosition = safePos2;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
                /*
                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = $"Fireball_Healer_SafeZone2_{fireball.EntityId}";
                dp2.Position = safePos2;
                dp2.Scale = new Vector2(1);
                dp2.Color = new Vector4(0, 1, 0, 0.6f); // Green
                dp2.DestoryAt = 35000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp2);
                */

            }
            else if (IsTank(player))
            {
                var directionToCenter = Vector3.Normalize(SnowballArenaCenter - fireballPos);
                var rotation = MathF.Atan2(directionToCenter.X, directionToCenter.Z);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"Fireball_Tank_SafeZone_{uniqueId}";
                dp.Position = fireballPos;
                dp.Rotation = rotation;
                dp.Scale = new Vector2(5);
                dp.Radian = 15 * MathF.PI / 180.0f;
                dp.Color = new Vector4(0, 1, 0, 0.6f); // Green
                dp.DestoryAt = 10000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = "Guide_to_safepos";
                dp2.Scale = new Vector2(1);
                dp2.Owner = accessory.Data.Me;
                dp2.Color = new Vector4(0, 1, 0, 0.6f);
                dp2.DestoryAt = 10000;
                dp2.TargetPosition = fireballPos;
                dp2.ScaleMode |= ScaleMode.YByDistance;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
            }
        }
        [ScriptMethod(
            name: "Geothermal Rupture (Guidance)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:42441"]
        )]
        public void GeothermalRupture(Event @event, ScriptAccessory accessory)
        {
            if (_fireballPositions.Count == 0)
            {
                if(Enable_Developer_Mode) accessory.Log.Error("Geothermal Rupture: Failed to obtain fireball position information.");
                return;
            }

            int pathIndex = 0;

            // Draw path for each recorded DPS position
            foreach (var fireballPos in _fireballPositions)
            {
                var directionToFireball = Vector3.Normalize(fireballPos - SnowballArenaCenter);
                var startPos = fireballPos + directionToFireball * 6;
                // Calculate path points
                var point1 = RotatePoint(startPos, fireballPos, MathF.PI / 2); // Clockwise 90 degrees
                var point2 = RotatePoint(startPos, fireballPos, MathF.PI);   // Clockwise 180 degrees

                // Draw path from start point to 90-degree point (Yellow)
                var dp1 = accessory.Data.GetDefaultDrawProperties();
                dp1.Name = $"GeothermalRupture_Path1_{pathIndex}";
                dp1.Position = startPos;
                dp1.TargetPosition = point1;
                dp1.Scale = new Vector2(1.5f);
                dp1.ScaleMode |= ScaleMode.YByDistance;
                dp1.Color = new Vector4(1f, 1f, 0f, 1f);
                dp1.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
                // Draw path from start point to 90-degree point (Green)
                var dp4 = accessory.Data.GetDefaultDrawProperties();
                dp4.Name = $"GeothermalRupture_Path1_{pathIndex}";
                dp4.Position = startPos;
                dp4.TargetPosition = point1;
                dp4.Scale = new Vector2(1.5f);
                dp4.ScaleMode |= ScaleMode.YByDistance;
                dp4.Color = new Vector4(0f, 1f, 0f, 1f);
                dp4.Delay = 5000;
                dp4.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp4);

                // Draw path from 90-degree point to 180-degree point (Yellow)
                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = $"GeothermalRupture_Path2_{pathIndex}";
                dp2.Position = point1;
                dp2.TargetPosition = point2;
                dp2.Scale = new Vector2(1.5f);
                dp2.ScaleMode |= ScaleMode.YByDistance;
                dp2.Color = new Vector4(1f, 1f, 0f, 1f);
                dp2.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
                // Draw path from 90-degree point to 180-degree point (Green)
                var dp3 = accessory.Data.GetDefaultDrawProperties();
                dp3.Name = $"GeothermalRupture_Path2_{pathIndex}";
                dp3.Position = point1;
                dp3.TargetPosition = point2;
                dp3.Scale = new Vector2(1.5f);
                dp3.ScaleMode |= ScaleMode.YByDistance;
                dp3.Color = new Vector4(0f, 1f, 0f, 1f);
                dp3.Delay = 8000;
                dp3.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);





                pathIndex++;
            }
        }

        #endregion
        #region Third Boss
        [ScriptMethod(
            name: "Initialize Third Boss",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:30705"],
            userControl: false
        )]
        public void OnInitializeBoss3Draw(Event @event, ScriptAccessory accessory)
        {
            // Initialize third boss state

            accessory.Method.RemoveDraw(".*");
            if(Enable_Developer_Mode) accessory.Log.Debug("Third boss initialization complete.");
            _puddles.Clear();
        }
        [ScriptMethod(
            name: "Draconic Action",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:30657"]
        )]
        public void OnDraconiformMotionDraw(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "DraconiformMotion_Danger_Zone1";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(60);
            dp.Radian = 90f * MathF.PI / 180f;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.ScaleMode = ScaleMode.YByTime;
            dp.DestoryAt = 4800;
            dp2.Name = "DraconiformMotion_Danger_Zone2";
            dp2.Owner = @event.SourceId;
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.Scale = new Vector2(60);
            dp2.Radian = 90f * MathF.PI / 180f;
            dp2.Rotation = MathF.PI;
            dp2.ScaleMode = ScaleMode.YByTime;
            dp2.DestoryAt = 4800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp2);
        }
        [ScriptMethod(
            name: "Dive",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:37819"]
        )]
        public void OnFrigidDiveDraw(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "FrigidDive_Danger_Zone";
            dp.Owner = @event.SourceId;
            dp.Scale = new Vector2(20, 60);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.ScaleMode = ScaleMode.ByTime;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            if (EnableTTS)
            {
                accessory.Method.EdgeTTS("Dive");
            }
        }
        [ScriptMethod(
            name: "Puddle - Record",
            eventType: EventTypeEnum.ObjectChanged,
            eventCondition: ["Operate:Add", "DataId:regex:^(2014546|2014547)$"],
            userControl: false
        )]
        public void OnPuddleSpawn(Event @event, ScriptAccessory accessory)
        {
            var id = @event.SourceId;
            var dataId = uint.Parse(@event["DataId"]);

            if (dataId == 2014546)
            {
                _puddles[id] = PuddleType.Circle;
            }
            else if (dataId == 2014547)
            {
                _puddles[id] = PuddleType.Cross;
            }
        }
        [ScriptMethod(
            name: "Puddle - Removal",
            eventType: EventTypeEnum.ObjectChanged,
            eventCondition: ["Operate:Remove", "DataId:regex:^(2014546|2014547)$"],
            userControl: false
        )]
        public void OnPuddleDespawn(Event @event, ScriptAccessory accessory)
        {
            _puddles.TryRemove(@event.SourceId, out _);
        }
        [ScriptMethod(
            name: "Puddle (Circle/Cross)",
            eventType: EventTypeEnum.ObjectEffect,
            eventCondition: ["Id1:16", "Id2:32"]
        )]
        public void OnPuddleEffect(Event @event, ScriptAccessory accessory)
        {
            var sourceId = @event.SourceId;
            var source = accessory.Data.Objects.SearchById(sourceId);

            // Check if unit exists, is within the arena, and is one of our recorded puddles
            if (source == null ||
                Vector3.Distance(source.Position, Boss3ArenaCenter) > 30 ||
                !_puddles.TryGetValue(sourceId, out var type))
            {
                return;
            }

            switch (type)
            {
                case PuddleType.Circle:
                    var dpCircle = accessory.Data.GetDefaultDrawProperties();
                    dpCircle.Name = $"Puddle_Circle_{sourceId}";
                    dpCircle.Owner = sourceId;
                    dpCircle.Scale = new Vector2(20);
                    dpCircle.Color = accessory.Data.DefaultDangerColor;
                    dpCircle.ScaleMode = ScaleMode.ByTime;
                    dpCircle.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dpCircle);
                    break;

                case PuddleType.Cross:
                    // Draw first straight line
                    var dpCross1 = accessory.Data.GetDefaultDrawProperties();
                    dpCross1.Name = $"Puddle_Cross1_{sourceId}";
                    dpCross1.Owner = sourceId;
                    dpCross1.Scale = new Vector2(16, 120);
                    dpCross1.Color = accessory.Data.DefaultDangerColor;
                    dpCross1.ScaleMode = ScaleMode.ByTime;
                    dpCross1.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dpCross1);

                    // Draw second vertical straight line
                    var dpCross2 = accessory.Data.GetDefaultDrawProperties();
                    dpCross2.Name = $"Puddle_Cross2_{sourceId}";
                    dpCross2.Owner = sourceId;
                    dpCross2.Scale = new Vector2(16, 120);
                    dpCross2.Rotation = MathF.PI / 2; // Rotate 90 degrees
                    dpCross2.Color = accessory.Data.DefaultDangerColor;
                    dpCross2.ScaleMode = ScaleMode.ByTime;
                    dpCross2.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dpCross2);
                    break;
            }
        }
        [ScriptMethod(
            name: "Draconic Action Pre-position (Guidance)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(30063|30264)$"]
        )]
        //{-336.98, -840.00, 165.53}
        public void OnDraconiformMotionGuide(Event @event, ScriptAccessory accessory)
        {
            // Draw guidance
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "DraconiformMotion_Guide";
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = new Vector3(-336.98f, -840.00f, 165.53f); // Behind boss
            dp.Scale = new Vector2(1.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            if (EnableTTS)
            {
                accessory.Method.EdgeTTS("Pre-position");
            }
        }
        [ScriptMethod(
            name: "Ice Tower - Spawn (Guidance)",
            eventType: EventTypeEnum.ObjectChanged,
            eventCondition: ["Operate:Add", "DataId:2014548"]
        )]
        public void OnIceTowerSpawn(Event @event, ScriptAccessory accessory)
        {
            var tower = accessory.Data.Objects.SearchById(@event.SourceId);
            if (tower == null) return;

            List<Vector3> teamTowerCoords = new();
            switch (SelectedStrategy)
            {
                case StrategySelection.ABC_123:
                    TowerPositions_ABC123.TryGetValue(MyTeam, out teamTowerCoords);
                    break;
                case StrategySelection.Pos_152463:
                    TowerPositions_123456.TryGetValue(MyPosition, out teamTowerCoords);
                    break;
                case StrategySelection.LemonCookie:
                    TowerPosition_Lemon.TryGetValue(MyLemonCookiePosition, out teamTowerCoords);
                    break;
            }

            if (teamTowerCoords != null)
            {
                // Check if the spawned tower is one of your team's towers
                foreach (var coord in teamTowerCoords)
                {
                    if (Vector3.DistanceSquared(tower.Position, coord) < 1.0f)
                    {
                        // Is your tower, proceed with drawing
                        var dpCircle = accessory.Data.GetDefaultDrawProperties();
                        dpCircle.Name = $"IceTower_Circle_{tower.EntityId}";
                        dpCircle.Position = tower.Position;
                        dpCircle.Scale = new Vector2(4);
                        dpCircle.Color = new Vector4(0, 1, 0, 1); // Green
                        dpCircle.DestoryAt = 22000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dpCircle);
                        // Break after finding it
                        break;
                    }
                }
            }
        }
        [ScriptMethod(
            name: "Ice Tower - Effect Trigger (Guidance)",
            eventType: EventTypeEnum.ObjectEffect,
            eventCondition: ["Id1:16", "Id2:32"]
        )]
        public void OnIceTowerEffect(Event @event, ScriptAccessory accessory)
        {
            var tower = accessory.Data.Objects.SearchById(@event.SourceId);

            if (tower == null || tower.DataId != 2014548) return;

            List<Vector3> teamTowerCoords = null;
            switch (SelectedStrategy)
            {
                case StrategySelection.ABC_123:
                    TowerPositions_ABC123.TryGetValue(MyTeam, out teamTowerCoords);
                    break;
                case StrategySelection.Pos_152463:
                    TowerPositions_123456.TryGetValue(MyPosition, out teamTowerCoords);
                    break;
                case StrategySelection.LemonCookie:
                    TowerPosition_Lemon.TryGetValue(MyLemonCookiePosition, out teamTowerCoords);
                    break;
            }

            if (teamTowerCoords != null)
            {
                foreach (var coord in teamTowerCoords)
                {
                    if (Vector3.DistanceSquared(tower.Position, coord) < 1.0f)
                    {
                        var dpGuide = accessory.Data.GetDefaultDrawProperties();
                        dpGuide.Name = $"IceTower_Guide_{tower.EntityId}";
                        dpGuide.Owner = accessory.Data.Me;
                        dpGuide.TargetObject = tower.EntityId;
                        dpGuide.Scale = new Vector2(1.5f);
                        dpGuide.ScaleMode |= ScaleMode.YByDistance;
                        dpGuide.Color = accessory.Data.DefaultSafeColor;
                        dpGuide.DestoryAt = 4000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dpGuide);
                        break;
                    }
                }
            }
        }
        [ScriptMethod(
            name: "Mob Grouping (Guidance)",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: ["DataId:14730"]
        )]
        public void OnGroupMarkerSpawn(Event @event, ScriptAccessory accessory)
        {
            var marker = accessory.Data.Objects.SearchById(@event.SourceId);
            if (marker == null) return;

            TeamSelection targetGroup = MyTeam; // Default for ABC_123
            bool shouldDraw = false;


        switch (SelectedStrategy)
        {
            case StrategySelection.ABC_123:
                shouldDraw = true;
                targetGroup = MyTeam;
                break;
                
            case StrategySelection.Pos_152463:
                shouldDraw = true;
                switch (MyPosition)
                {
                    case PositionSelection.Pos1: targetGroup = TeamSelection.A; break;
                    case PositionSelection.Pos5: targetGroup = TeamSelection.B; break;
                    case PositionSelection.Pos2: targetGroup = TeamSelection.C; break;
                    case PositionSelection.Pos3: targetGroup = TeamSelection.One; break;
                    case PositionSelection.Pos6: targetGroup = TeamSelection.Two; break;
                    case PositionSelection.Pos4: targetGroup = TeamSelection.Three; break;
                }
                break;
                
            case StrategySelection.LemonCookie:
                shouldDraw = true;
                // LemonCookie grouping strategy
                // 152463 group number to LemonCookie group number mapping: 1->1, 2->3, 3->6, 4->4, 5->2, 6->5
                // Here we need reverse mapping: LemonCookie group number -> corresponding 152463 position -> corresponding TeamSelection
                switch (MyLemonCookiePosition)
                {
                    case PositionSelection.Pos1: // LemonCookie group 1 -> 152463 group 1 -> TeamSelection.A
                        targetGroup = TeamSelection.A; 
                        break;
                    case PositionSelection.Pos2: // LemonCookie group 2 -> 152463 group 5 -> TeamSelection.B  
                        targetGroup = TeamSelection.B;
                        break;
                    case PositionSelection.Pos3: // LemonCookie group 3 -> 152463 group 2 -> TeamSelection.C
                        targetGroup = TeamSelection.C;
                        break;
                    case PositionSelection.Pos4: // LemonCookie group 4 -> 152463 group 4 -> TeamSelection.Three
                        targetGroup = TeamSelection.Three;
                        break;
                    case PositionSelection.Pos5: // LemonCookie group 5 -> 152463 group 6 -> TeamSelection.Two  
                        targetGroup = TeamSelection.Two;
                        break;
                    case PositionSelection.Pos6: // LemonCookie group 6 -> 152463 group 3 -> TeamSelection.One
                        targetGroup = TeamSelection.One;
                        break;
                }
                break;
        }


            if (shouldDraw)
            {
                foreach (var groupEntry in GroupMarkerPositions)
                {
                    if (Vector3.DistanceSquared(marker.Position, groupEntry.Value) < 1.0f)
                    {
                        if (groupEntry.Key == targetGroup)
                        {
                            var dpCircle = accessory.Data.GetDefaultDrawProperties();
                            dpCircle.Name = $"GroupMarker_Circle_{marker.EntityId}";
                            dpCircle.Owner = marker.EntityId;
                            dpCircle.Scale = new Vector2(3);
                            dpCircle.Color = new Vector4(0, 1, 0, 1);
                            dpCircle.DestoryAt = 4000;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dpCircle);

                            var dpGuide = accessory.Data.GetDefaultDrawProperties();
                            dpGuide.Name = $"GroupMarker_Guide_{marker.EntityId}";
                            dpGuide.Owner = accessory.Data.Me;
                            dpGuide.TargetObject = marker.EntityId;
                            dpGuide.Scale = new Vector2(1.5f);
                            dpGuide.ScaleMode |= ScaleMode.YByDistance;
                            dpGuide.Color = accessory.Data.DefaultSafeColor;
                            dpGuide.DestoryAt = 4000;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dpGuide);
                            break;
                        }
                    }
                }
            }
        }
        #endregion
        #region Final Boss
        [ScriptMethod(
            name: "Final Boss - Initialization",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41572"],
            userControl: false
        )]
        public void OnInitializeBoss4Draw(Event @event, ScriptAccessory accessory)
        {
            // Initialize final boss state
            accessory.Method.RemoveDraw(".*");
            if(Enable_Developer_Mode) accessory.Log.Debug("Final boss initialization complete.");
            _holyWeaponType = HolyWeaponType.None;
            lock(_preyCheckLock)
            {
                _checkedPreyPlayers.Clear(); // Reset checked list
            }
            lock(_lanceShareLock)
            {
                _lanceShareAssignments.Clear(); // Reset lance share record
            }
            _sacredBowPreyRecordedPlayers.Clear();
        }

        [ScriptMethod(
            name: "Seal Release",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(41538|41537)$"]
        )]
        public void UnsealAlert(Event @event, ScriptAccessory accessory)
        {
            var ActionId = @event.ActionId;
            if (ActionId == 41538) //lance
            {
                if(EnableTextBanner) accessory.Method.TextInfo("Lance, far auto-attacks", 5000);
                if (EnableTTS)
                {
                    accessory.Method.EdgeTTS("Far auto-attacks, 3 hits");
                }
            }
            else
            {
                if(EnableTextBanner) accessory.Method.TextInfo("Axe, near auto-attacks", 5000);
                if (EnableTTS)
                {
                    accessory.Method.EdgeTTS("Near auto-attacks, 3 hits");
                }
            }
        }
        [ScriptMethod(
            name: "Forked Fury",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41573"]
        )]
        public void OnForkedFuryAlert(Event @event, ScriptAccessory accessory)
        {
            if(EnableTextBanner) accessory.Method.TextInfo("Near/far tankbuster", 5000);
            if (EnableTTS)
            {
                accessory.Method.EdgeTTS("Near/far tankbuster, then two auto-attacks");
            }
        }
        [ScriptMethod(
            name: "Assassin's Dagger",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41569"] 
        )]
        public void AssassinsDagger(Event @event, ScriptAccessory accessory)
        {
            var caster = accessory.Data.Objects.SearchById(@event.SourceId);
            if (caster == null) return;


            var directionVector = @event.EffectPosition - caster.Position;
            var initialAngle = MathF.Atan2(directionVector.X, directionVector.Z);
            var distance = directionVector.Length();
            var rotationOffset = -50 * MathF.PI / 180.0f;

            for (int i = 0; i < 6; i++)
            {
                var currentAngle = initialAngle + i * rotationOffset;
                var delay = 1100 + (long)(i * 3900);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"AssassinsDagger_{i}";
                dp.Position = caster.Position;
                dp.Scale = new Vector2(6, distance);
                dp.Rotation = currentAngle;
                dp.Delay = delay;
                dp.DestoryAt = 6100;
                dp.Color = accessory.Data.DefaultDangerColor;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }

        [ScriptMethod(
            name: "Deadly Lance/Axe Combo AOE",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(41547|41543)$"]
        )]
        public void CriticalBlow(Event @event, ScriptAccessory accessory)
        {
            var caster = accessory.Data.Objects.SearchById(@event.SourceId);
            if (caster == null) return;

            bool isLance = @event.ActionId == 41547; // Lance

            var mainShapeDuration = isLance ? 6400 : 6100;
            var squareColor = isLance ? accessory.Data.DefaultDangerColor : new Vector4(0f, 0.6f, 0f, 0.8f);

            // Draw main AOE (Donut or Circle)
            if (isLance)
            {
                var dpDonut = accessory.Data.GetDefaultDrawProperties();
                dpDonut.Name = "CriticalLanceblow_Donut";
                dpDonut.Owner = caster.EntityId;
                dpDonut.Scale = new Vector2(32);
                dpDonut.InnerScale = new Vector2(10);
                dpDonut.Radian = 2f * MathF.PI;
                dpDonut.Color = accessory.Data.DefaultDangerColor;
                dpDonut.DestoryAt = mainShapeDuration;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dpDonut);
                // Draw guidance for lance
                var player = accessory.Data.MyObject;
                if (player != null)
                {
                    Vector3 closestPos = CriticalLanceSafePositions[0];
                    float minDistanceSq = Vector3.DistanceSquared(player.Position, closestPos);
                    for (int i = 1; i < CriticalLanceSafePositions.Count; i++)
                    {
                        float distSq = Vector3.DistanceSquared(player.Position, CriticalLanceSafePositions[i]);
                        if (distSq < minDistanceSq)
                        {
                            minDistanceSq = distSq;
                            closestPos = CriticalLanceSafePositions[i];
                        }
                    }
                }
                if (EnableTTS)
                {
                    accessory.Method.EdgeTTS("Donut");
                }
            }
            else // Axe
            {
                var dpCircle = accessory.Data.GetDefaultDrawProperties();
                dpCircle.Name = "CriticalAxeblow_Circle";
                dpCircle.Owner = caster.EntityId;
                dpCircle.Scale = new Vector2(20);
                dpCircle.Color = new Vector4(1f, 0f, 0f, 1.5f);
                dpCircle.DestoryAt = mainShapeDuration;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dpCircle);
                // Draw guidance for axe
                var player = accessory.Data.MyObject;
                if (player != null)
                {
                    Vector3 closestPos = CriticalAxeSafePositions[0];
                    float minDistanceSq = Vector3.DistanceSquared(player.Position, closestPos);
                    for (int i = 1; i < CriticalAxeSafePositions.Count; i++)
                    {
                        float distSq = Vector3.DistanceSquared(player.Position, CriticalAxeSafePositions[i]);
                        if (distSq < minDistanceSq)
                        {
                            minDistanceSq = distSq;
                            closestPos = CriticalAxeSafePositions[i];
                        }
                    }
                }
                if (EnableTTS)
                {
                    accessory.Method.EdgeTTS("Circle");
                }
            }
            // Draw three square AOEs
            for (int i = 0; i < SquarePositions.Count; i++)
            {
                var dpSquare = accessory.Data.GetDefaultDrawProperties();
                dpSquare.Name = $"Square_AOE_{i}";
                dpSquare.Position = SquarePositions[i];
                dpSquare.Rotation = SquareAngles[i];
                dpSquare.Scale = new Vector2(20, 20);
                dpSquare.Color = squareColor;
                dpSquare.DestoryAt = mainShapeDuration;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dpSquare);
            }
        }
        /*
        [ScriptMethod(
            name: "Deadly Lance/Axe (Guidance)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(41547|41543)$"]
        )]
        public void CriticalBlowGuide(Event @event, ScriptAccessory accessory)
        {
            var player = accessory.Data.MyObject;
            if (player == null) return;

            bool isLance = @event.ActionId == 41547; // Lance
            var safePositions = isLance ? CriticalLanceSafePositions : CriticalAxeSafePositions;
            var duration = isLance ? 6400 : 6100;

            Vector3 closestPos = safePositions[0];
            float minDistanceSq = Vector3.DistanceSquared(player.Position, closestPos);
            for (int i = 1; i < safePositions.Count; i++)
            {
                float distSq = Vector3.DistanceSquared(player.Position, safePositions[i]);
                if (distSq < minDistanceSq)
                {
                    minDistanceSq = distSq;
                    closestPos = safePositions[i];
                }
            }

            var dpGuide = accessory.Data.GetDefaultDrawProperties();
            dpGuide.Name = isLance ? "CriticalLance_Guide" : "CriticalAxe_Guide";
            dpGuide.Owner = player.EntityId;
            dpGuide.TargetPosition = closestPos;
            dpGuide.Scale = new Vector2(1.5f);
            dpGuide.ScaleMode |= ScaleMode.YByDistance;
            dpGuide.Color = accessory.Data.DefaultSafeColor;
            dpGuide.DestoryAt = duration;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dpGuide);
        }
        */
        [ScriptMethod(
            name: "Great Axe Prey (9 seconds)",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:4337"]
        )]
        public void GreatAxePrey(Event @event, ScriptAccessory accessory)
        {
            if (PoliceMode)
            {
                if (float.TryParse(@event["Duration"], out var duration1) && Math.Abs(duration1 - 9.0f) < 0.1f)
                {
                    var target = accessory.Data.Objects.SearchById(@event.TargetId);
                    if (target != null)
                    {
                        accessory.Method.SendChat($"/e Large circle (9 seconds) mark: {target.Name}");
                    }
                }
            }
            // Confirm that you yourself have this status
            if (@event.TargetId != accessory.Data.Me) return;

            // Check if buff duration is 9 seconds
            if (float.TryParse(@event["Duration"], out var duration) && Math.Abs(duration - 9.0f) < 0.1f)
            {
                var player = accessory.Data.MyObject;
                if (player == null) return;

                // Find the nearest coordinate point
                Vector3 closestPos = GreataxePreyPositions[0];
                float minDistanceSq = Vector3.DistanceSquared(player.Position, closestPos);

                for (int i = 1; i < GreataxePreyPositions.Count; i++)
                {
                    float distSq = Vector3.DistanceSquared(player.Position, GreataxePreyPositions[i]);
                    if (distSq < minDistanceSq)
                    {
                        minDistanceSq = distSq;
                        closestPos = GreataxePreyPositions[i];
                    }
                }

                // Draw guidance
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "GreataxePrey_Guide";
                dp.Owner = player.EntityId;
                dp.TargetPosition = closestPos;
                dp.Scale = new Vector2(1.5f);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        [ScriptMethod(
            name: "Great Axe Prey (21 seconds)",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:4337"]
        )]
        public async void GreatAxePreyLong(Event @event, ScriptAccessory accessory)
        {
            if (PoliceMode)
            {
                CheckPreyPosition(accessory, @event.TargetId);
                if (float.TryParse(@event["Duration"], out var duration1) && Math.Abs(duration1 - 21.0f) < 0.1f)
                {
                    var target = accessory.Data.Objects.SearchById(@event.TargetId);
                    if (target != null)
                    {
                        accessory.Method.SendChat($"/e Large circle (21 seconds) mark: {target.Name}");
                    }
                }
            }
            // Confirm that you yourself have this status
            if (@event.TargetId != accessory.Data.Me) return;

            // Check if buff duration is 21 seconds
            if (float.TryParse(@event["Duration"], out var duration) && Math.Abs(duration - 21.0f) < 0.1f)
            {
                var player = accessory.Data.MyObject;
                if (player == null) return;
                await Task.Delay(15000);
                // Find the nearest coordinate point
                Vector3 closestPos = GreataxePreyPositions[0];
                float minDistanceSq = Vector3.DistanceSquared(player.Position, closestPos);

                for (int i = 1; i < GreataxePreyPositions.Count; i++)
                {
                    float distSq = Vector3.DistanceSquared(player.Position, GreataxePreyPositions[i]);
                    if (distSq < minDistanceSq)
                    {
                        minDistanceSq = distSq;
                        closestPos = GreataxePreyPositions[i];
                    }
                }

                // Draw guidance
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "GreataxePrey_Guide";
                dp.Owner = player.EntityId;
                dp.TargetPosition = closestPos;
                dp.Scale = new Vector2(1.5f);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        [ScriptMethod(
            name: "Lesser Axe Prey",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:4336"]
        )]
        public void LesserAxePrey(Event @event, ScriptAccessory accessory)
        {
            if (PoliceMode)
            {
                CheckPreyPosition(accessory, @event.TargetId);
                if (float.TryParse(@event["Duration"], out var duration1) && Math.Abs(duration1 - 13.0f) < 0.1f)
                {
                    var target = accessory.Data.Objects.SearchById(@event.TargetId);
                    if (target != null)
                    {
                        accessory.Method.SendChat($"/e Small circle (13 seconds) mark: {target.Name}");
                    }
                }
                else if (Math.Abs(duration1 - 21.0f) < 0.1f)
                {
                    var target = accessory.Data.Objects.SearchById(@event.TargetId);
                    if (target != null)
                    {
                        accessory.Method.SendChat($"/e Small circle (21 seconds) mark: {target.Name}");
                    }
                }
            }
            // Confirm that you yourself have this status
            if (@event.TargetId != accessory.Data.Me) return;
            var player = accessory.Data.MyObject;
            if (float.TryParse(@event["Duration"], out var duration))
            {
                // Check if buff duration is 13 seconds
                if (Math.Abs(duration - 13.0f) < 0.1f)
                {
                    // Find the nearest coordinate point
                    Vector3 closestPos = SquarePositions[0];
                    float minDistanceSq = Vector3.DistanceSquared(player.Position, closestPos);

                    for (int i = 1; i < SquarePositions.Count; i++)
                    {
                        float distSq = Vector3.DistanceSquared(player.Position, SquarePositions[i]);
                        if (distSq < minDistanceSq)
                        {
                            minDistanceSq = distSq;
                            closestPos = SquarePositions[i];
                        }
                    }                    
                    // Draw guidance
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "LesseraxePrey_Guide";
                    dp.Owner = player.EntityId;
                    dp.TargetPosition = closestPos;
                    dp.Scale = new Vector2(1.5f);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 13000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);                    
                    
                    /*
                    // Draw green safe circles at the three square AOE center points
                    for (int i = 0; i < SquarePositions.Count; i++)
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"LittleAxePrey_SafeZone_13s_{i}";
                        dp.Position = SquarePositions[i];
                        dp.Scale = new Vector2(3);
                        dp.Color = new Vector4(0, 1, 0, 1);
                        dp.DestoryAt = 13000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
                    }
                    */
                }
                // Check if buff duration is 21 seconds
                else if (Math.Abs(duration - 21.0f) < 0.1f)
                {
                    if (LongPointName)
                    {
                        var dp1 = accessory.Data.GetDefaultDrawProperties();
                        dp1.Name = "LittleAxePrey_SafeZone_21s_2";
                        dp1.Owner = player.EntityId;
                        dp1.TargetPosition = SquarePositions[1];
                        dp1.Scale = new Vector2(1.5f);
                        dp1.ScaleMode |= ScaleMode.YByDistance;
                        dp1.Color = accessory.Data.DefaultSafeColor;
                        dp1.Delay = 15000;
                        dp1.DestoryAt = 6000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
                        var dp3 = accessory.Data.GetDefaultDrawProperties();
                        dp3.Name = "LittleAxePrey_SafeZone_21s_3";
                        dp3.Owner = player.EntityId;
                        dp3.TargetPosition = SquarePositions[2];
                        dp3.Scale = new Vector2(1.5f);
                        dp3.ScaleMode |= ScaleMode.YByDistance;
                        dp3.Color = accessory.Data.DefaultSafeColor;
                        dp3.Delay = 15000;
                        dp3.DestoryAt = 6000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);
                    }
                    else
                    {
                        // Draw green safe circle at the 2nd square AOE center point
                        var dp1 = accessory.Data.GetDefaultDrawProperties();
                        dp1.Name = "LittleAxePrey_SafeZone_21s_2";
                        dp1.Position = SquarePositions[1];
                        dp1.Scale = new Vector2(3);
                        dp1.Color = new Vector4(0, 1, 0, 1);
                        dp1.Delay = 15000;
                        dp1.DestoryAt = 6000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp1);

                        // Draw green safe circle at the 3rd square AOE center point
                        var dp3 = accessory.Data.GetDefaultDrawProperties();
                        dp3.Name = "LittleAxePrey_SafeZone_21s_3";
                        dp3.Position = SquarePositions[2];
                        dp3.Scale = new Vector2(3);
                        dp3.Color = new Vector4(0, 1, 0, 1);
                        dp3.Delay = 15000;
                        dp3.DestoryAt = 6000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp3);
                    }
                }
            }
        }
        /*
        [ScriptMethod(
            name: "Sacred Bow Prey",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:4338"],
            userControl: false)]
        public void SacredBowPrey(Event @event, ScriptAccessory accessory)
        {
            if (PoliceMode)
            {
                CheckPreyPosition(accessory, @event.TargetId);
                if (float.TryParse(@event["Duration"], out var duration1) && Math.Abs(duration1 - 17.0f) < 0.1f)
                {
                    var target = accessory.Data.Objects.SearchById(@event.TargetId);
                    if (target != null)
                    {
                        accessory.Method.SendChat($"/e Holy Lance share (17 seconds) mark: {target.Name}");
                    }
                }
                else if (Math.Abs(duration1 - 25.0f) < 0.1f)
                {
                    var target = accessory.Data.Objects.SearchById(@event.TargetId);
                    if (target != null)
                    {
                        accessory.Method.SendChat($"/e Holy Lance share (25 seconds) mark: {target.Name}");
                    }
                }
                else if (Math.Abs(duration1 - 33.0f) < 0.1f)
                {
                    var target = accessory.Data.Objects.SearchById(@event.TargetId);
                    if (target != null)
                    {
                        accessory.Method.SendChat($"/e Holy Lance share (33 seconds) mark: {target.Name}");
                    }
                }
            }
        }
        */
        [ScriptMethod(
            name: "Holy Lance (Guidance)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41557"]
        )]
        public void HolyLanceGuide(Event @event, ScriptAccessory accessory)
        {
            lock (_sacredBowPreyLock)
            {
                _sacredBowPreyRecordedPlayers.Clear(); // New Holy Lance mechanic starts, clear record
            }
            var path = new List<DisplacementContainer>();
            // Override logic: only when "TopLeft/TopRight/Bottom" is selected, force use of A/B/C path blocks;
            // When None is selected, keep the original SelectedStrategy complete branch, without any changes.
            if (HolyLanceGroupOverride != LanceGuideOverride.None)
            {
                TeamSelection forcedTeam = TeamSelection.A;
                switch (HolyLanceGroupOverride)
                {
                    case LanceGuideOverride.TopLeft: forcedTeam = TeamSelection.A; break;
                    case LanceGuideOverride.TopRight: forcedTeam = TeamSelection.B; break;
                    case LanceGuideOverride.Bottom:   forcedTeam = TeamSelection.C; break;
                }

                switch (forcedTeam)
                {
                    case TeamSelection.A:
                        path.Add(new DisplacementContainer(SquarePositions[2], 0, 10000));
                        path.Add(new DisplacementContainer(CriticalLanceSafePositions[1], 0, 5000));
                        path.Add(new DisplacementContainer(SquarePositions[2], 0, 17000));
                        path.Add(new DisplacementContainer(RectSideInA, 0, 3000));
                        path.Add(new DisplacementContainer(RectSideOutA, 0, 6000));
                        break;
                    case TeamSelection.B:
                        path.Add(new DisplacementContainer(SquarePositions[1], 0, 10000));
                        path.Add(new DisplacementContainer(CriticalLanceSafePositions[2], 0, 5000));
                        path.Add(new DisplacementContainer(RectSideInB, 0, 4000));
                        path.Add(new DisplacementContainer(RectSideOutB, 0, 6000));
                        path.Add(new DisplacementContainer(SquarePositions[1], 0, 14000));
                        break;
                    case TeamSelection.C:
                        path.Add(new DisplacementContainer(SquarePositions[0], 0, 10000));
                        path.Add(new DisplacementContainer(CriticalLanceSafePositions[0], 0, 5000));
                        path.Add(new DisplacementContainer(SquarePositions[0], 0, 9000));
                        path.Add(new DisplacementContainer(RectSideInC, 0, 3000));
                        path.Add(new DisplacementContainer(RectSideOutC, 0, 6000));
                        path.Add(new DisplacementContainer(SquarePositions[0], 0, 6000));
                        break;
                }
            }
            else
            {
                // None: keep original strategy branch (SelectedStrategy fully effective)
                // Determine path based on grouping
                switch (SelectedStrategy)
                {
                    case StrategySelection.ABC_123:
                        switch (MyTeam)
                        {
                            case TeamSelection.A:
                            case TeamSelection.One:
                                path.Add(new DisplacementContainer(SquarePositions[2], 0, 10000));
                                path.Add(new DisplacementContainer(CriticalLanceSafePositions[1], 0, 5000));
                                path.Add(new DisplacementContainer(SquarePositions[2], 0, 17000));
                                path.Add(new DisplacementContainer(RectSideInA, 0, 3000));
                                path.Add(new DisplacementContainer(RectSideOutA, 0, 6000));
                                break;
                            case TeamSelection.B:
                            case TeamSelection.Two:
                                path.Add(new DisplacementContainer(SquarePositions[1], 0, 10000));
                                path.Add(new DisplacementContainer(CriticalLanceSafePositions[2], 0, 5000));
                                path.Add(new DisplacementContainer(RectSideInB, 0, 4000));
                                path.Add(new DisplacementContainer(RectSideOutB, 0, 6000));
                                path.Add(new DisplacementContainer(SquarePositions[1], 0, 14000));
                                break;
                            case TeamSelection.C:
                            case TeamSelection.Three:
                                path.Add(new DisplacementContainer(SquarePositions[0], 0, 10000));
                                path.Add(new DisplacementContainer(CriticalLanceSafePositions[0], 0, 5000));
                                path.Add(new DisplacementContainer(SquarePositions[0], 0, 9000));
                                path.Add(new DisplacementContainer(RectSideInC, 0, 3000));
                                path.Add(new DisplacementContainer(RectSideOutC, 0, 6000));
                                path.Add(new DisplacementContainer(SquarePositions[0], 0, 6000));
                                break;
                        }
                        break;
                    case StrategySelection.Pos_152463:
                        switch (MyPosition)
                        {
                            case PositionSelection.Pos1:
                            case PositionSelection.Pos2:
                                path.Add(new DisplacementContainer(SquarePositions[2], 0, 10000));
                                path.Add(new DisplacementContainer(CriticalLanceSafePositions[1], 0, 5000));
                                path.Add(new DisplacementContainer(SquarePositions[2], 0, 17000));
                                path.Add(new DisplacementContainer(RectSideInA, 0, 3000));
                                path.Add(new DisplacementContainer(RectSideOutA, 0, 6000));
                                break;
                            case PositionSelection.Pos5:
                            case PositionSelection.Pos6:
                                path.Add(new DisplacementContainer(SquarePositions[1], 0, 10000));
                                path.Add(new DisplacementContainer(CriticalLanceSafePositions[2], 0, 5000));
                                path.Add(new DisplacementContainer(RectSideInB, 0, 4000));
                                path.Add(new DisplacementContainer(RectSideOutB, 0, 6000));
                                path.Add(new DisplacementContainer(SquarePositions[1], 0, 14000));
                                break;
                            case PositionSelection.Pos3:
                            case PositionSelection.Pos4:
                                path.Add(new DisplacementContainer(SquarePositions[0], 0, 10000));
                                path.Add(new DisplacementContainer(CriticalLanceSafePositions[0], 0, 5000));
                                path.Add(new DisplacementContainer(SquarePositions[0], 0, 9000));
                                path.Add(new DisplacementContainer(RectSideInC, 0, 3000));
                                path.Add(new DisplacementContainer(RectSideOutC, 0, 6000));
                                path.Add(new DisplacementContainer(SquarePositions[0], 0, 6000));
                                break;
                        }
                        break;
                    case StrategySelection.LemonCookie:
                        switch (MyLemonCookiePosition)
                        {
                            case PositionSelection.Pos1:
                            case PositionSelection.Pos2:
                                path.Add(new DisplacementContainer(SquarePositions[2], 0, 10000));
                                path.Add(new DisplacementContainer(CriticalLanceSafePositions[1], 0, 5000));
                                path.Add(new DisplacementContainer(SquarePositions[2], 0, 17000));
                                path.Add(new DisplacementContainer(RectSideInA, 0, 3000));
                                path.Add(new DisplacementContainer(RectSideOutA, 0, 6000));
                                break;
                            case PositionSelection.Pos5:
                            case PositionSelection.Pos6:
                                path.Add(new DisplacementContainer(SquarePositions[1], 0, 10000));
                                path.Add(new DisplacementContainer(CriticalLanceSafePositions[2], 0, 5000));
                                path.Add(new DisplacementContainer(RectSideInB, 0, 4000));
                                path.Add(new DisplacementContainer(RectSideOutB, 0, 6000));
                                path.Add(new DisplacementContainer(SquarePositions[1], 0, 14000));
                                break;
                            case PositionSelection.Pos3:
                            case PositionSelection.Pos4:
                                path.Add(new DisplacementContainer(SquarePositions[0], 0, 10000));
                                path.Add(new DisplacementContainer(CriticalLanceSafePositions[0], 0, 5000));
                                path.Add(new DisplacementContainer(SquarePositions[0], 0, 9000));
                                path.Add(new DisplacementContainer(RectSideInC, 0, 3000));
                                path.Add(new DisplacementContainer(RectSideOutC, 0, 6000));
                                path.Add(new DisplacementContainer(SquarePositions[0], 0, 6000));
                                break;
                        }
                        break;                        
                }
            }

            if (path.Count > 0)
            {
                var props = new MultiDisDrawProp
                {
                    Color_GoNow = new Vector4(0, 1, 0, 1), // Green
                    Color_GoLater = new Vector4(1, 1, 0, 1), // Yellow
                    DrawMode = DrawModeEnum.Imgui
                };
                accessory.MultiDisDraw(path, props);
                if(Enable_Developer_Mode) accessory.Log.Debug($"Holy Lance mechanic: Override={HolyLanceGroupOverride}, Strategy={SelectedStrategy}.");
            }
        }
        [ScriptMethod(
            name: "Holy - Record Weapon",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:4339"],
            userControl: false
        )]
        public void OnSealMeltStatus(Event @event, ScriptAccessory accessory)
        {
            if (int.TryParse(@event["Param"], out int paramValue))
            {
                if (paramValue == 851)
                {
                    _holyWeaponType = HolyWeaponType.Axe;
                    if(Enable_Developer_Mode) accessory.Log.Debug("Holy mechanic: Recorded as Axe.");
                }
                else if (paramValue == 852)
                {
                    _holyWeaponType = HolyWeaponType.Lance;
                    if(Enable_Developer_Mode) accessory.Log.Debug("Holy mechanic: Recorded as Lance.");
                }
            }
        }

        [ScriptMethod(
            name: "Hallowed Plume - Prompt",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41562"],
            suppress:2000
        )]
        public void OnHallowedPlumeCast(Event @event, ScriptAccessory accessory)
        {
            string hintText = "";
            switch (_holyWeaponType)
            {
                case HolyWeaponType.Axe:
                    hintText = "Hit the yellow vessel";
                    break;
                case HolyWeaponType.Lance:
                    hintText = "Hit the blue vessel";
                    break;
                default:
                    accessory.Log.Error("Holy mechanic: Failed to obtain weapon type.");
                    return;
            }
            if(EnableTextBanner) accessory.Method.TextInfo(hintText, 5000);
            if (EnableTTS)
            {
                accessory.Method.EdgeTTS(hintText);
            }

            // Reset state
            _holyWeaponType = HolyWeaponType.None;
        }
        [ScriptMethod(
            name: "Holy - Prompt",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:41563"],
            suppress: 2000
        )]
        public void OnHolyCast(Event @event, ScriptAccessory accessory)
        {
            string hintText = "";
            switch (_holyWeaponType)
            {
                case HolyWeaponType.Axe:
                    hintText = "Hit the yellow vessel";
                    break;
                case HolyWeaponType.Lance:
                    hintText = "Hit the blue vessel";
                    break;
                default:
                    accessory.Log.Error("Holy mechanic: Failed to obtain weapon type.");
                    return;
            }
            if(EnableTextBanner) accessory.Method.TextInfo(hintText, 5000);
            if (EnableTTS)
            {
                accessory.Method.EdgeTTS(hintText);
            }

            // Reset state
            _holyWeaponType = HolyWeaponType.None;
        }
        [ScriptMethod(
            name: "Sacred Bow Prey Mark",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:4338"]
        )]
        public void SacredBowPrey_RecordAndBroadcast(Event @event, ScriptAccessory accessory)
        {
            var player = accessory.Data.Objects.SearchById(@event.TargetId);
            if (player == null) return;
            lock (_sacredBowPreyLock)
            {
                if (_sacredBowPreyRecordedPlayers.Contains(player.EntityId)) return;
                _sacredBowPreyRecordedPlayers.Add(player.EntityId);
            }
            if (float.TryParse(@event["Duration"], out var duration))
            {
                int platformIndex = -1;
                lock (_lanceShareLock)
                {
                    bool alreadyRecorded = _lanceShareAssignments.Values.Any(list => list.Any(p => p.PlayerId == player.EntityId));
                    if (!alreadyRecorded)
                    {
                        for (int i = 0; i < SquarePositions.Count; i++)
                        {
                            if (IsPointInRotatedRect(player.Position, SquarePositions[i], 20, 20, SquareAngles[i]))
                            {
                                platformIndex = i;
                                break;
                            }
                        }

                        if (platformIndex != -1)
                        {
                            if (!_lanceShareAssignments.ContainsKey(platformIndex))
                            {
                                _lanceShareAssignments[platformIndex] = new List<(ulong, float)>();
                            }
                            _lanceShareAssignments[platformIndex].Add((player.EntityId, duration));
                            if (Enable_Developer_Mode)
                            {
                                accessory.Log.Debug($"Holy Lance share record: {player.Name.TextValue} on platform {platformIndex + 1}, duration {duration:F2}s");
                            }
                        }
                        else
                        {
                            if (Enable_Developer_Mode)
                            {
                                 accessory.Log.Debug($"Holy Lance share record: {player.Name.TextValue} is a violator, not on any platform.");
                            }
                        }
                    }
                }

                if (PoliceMode)
                {
                    // Check platform position again for broadcast
                    int reportPlatformIndex = -1;
                    for (int i = 0; i < SquarePositions.Count; i++)
                    {
                        if (IsPointInRotatedRect(player.Position, SquarePositions[i], 21, 21, SquareAngles[i]))
                        {
                            reportPlatformIndex = i;
                            break;
                        }
                    }

                    string platformName = reportPlatformIndex switch
                    {
                        0 => "Bottom",
                        1 => "Top-Right",
                        2 => "Top-Left",
                        _ => "Violator"
                    };
                    
                    accessory.Method.SendChat($"/e Holy Lance share mark: {player.Name.TextValue} - {platformName} ({duration:F1}s)");
                }
            }
        }
        [ScriptMethod(
            name: "Holy Lance Share - Edge Check",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:4338"]
        )]
        public void SacredBowPrey_CheckOnExpire(Event @event, ScriptAccessory accessory)
        {
            // Check if the buff expired normally
            if (!float.TryParse(@event["Duration"], out var remainingDuration) || remainingDuration > 0.1f)
            {
                return; // If duration is not 0, it was removed early, don't check
            }

            var player = accessory.Data.Objects.SearchById(@event.TargetId);
            if (player == null || player.IsDead) return;

            lock (_lanceShareLock)
            {
                int initialPlatform = -1;
                (ulong PlayerId, float Duration) assignment = (0, 0);

                foreach (var entry in _lanceShareAssignments)
                {
                    var found = entry.Value.FirstOrDefault(p => p.PlayerId == player.EntityId);
                    if (found.PlayerId != 0)
                    {
                        initialPlatform = entry.Key;
                        assignment = found;
                        break;
                    }
                }

                if (initialPlatform == -1) return;

                var sortedPlayers = _lanceShareAssignments[initialPlatform].OrderBy(p => p.Duration).ToList();
                int orderIndex = sortedPlayers.FindIndex(p => p.PlayerId == player.EntityId);

                bool shouldCheck = false;
                switch (initialPlatform)
                {
                    case 0: // Platform 1 (Bottom)
                        if (orderIndex == 0 || orderIndex == 2) shouldCheck = true;
                        break;
                    case 1: // Platform 2 (Top-Right)
                        if (orderIndex == 1 || orderIndex == 2) shouldCheck = true;
                        break;
                    case 2: // Platform 3 (Top-Left)
                        if (orderIndex == 0 || orderIndex == 1) shouldCheck = true;
                        break;
                }

                if (shouldCheck)
                {
                    if (!IsCircleFullyContainedInAnyPlatform(player.Position))
                    {
                        if (PoliceMode) accessory.Method.SendChat($"/e Share edge: {player.Name.TextValue}");
                    }
                }
            }
        }
        
        
        
        
        
        private void CheckPreyPosition(ScriptAccessory accessory, ulong targetId)
        {
            // If the player has already been checked, return immediately
            lock (_preyCheckLock)
            {
                if (_checkedPreyPlayers.Contains(targetId)) return;
                _checkedPreyPlayers.Add(targetId);
            }
            // This check is uniformly controlled by police mode
            if (!PoliceMode) return;
        
            var player = accessory.Data.Objects.SearchById(targetId);
            if (player == null) return; // If player cannot be found (may have left range), don't process
        
            bool isInAnySquare = false;
            for (int i = 0; i < SquarePositions.Count; i++)
            {
                if (IsPointInRotatedRect(player.Position, SquarePositions[i], 20, 20, SquareAngles[i]))
                {
                    isInAnySquare = true;
                    break;
                }
            }

            if (!isInAnySquare)
            {
                accessory.Method.SendChat($"/e {player.Name} is in wrong position!");
            }
        }
        
        private bool IsPointInRotatedRect(Vector3 point, Vector3 rectCenter, float rectWidth, float rectHeight, float rectAngleRad)
        {
            float translatedX = point.X - rectCenter.X;
            float translatedZ = point.Z - rectCenter.Z;
            
            float cosAngle = MathF.Cos(-rectAngleRad);
            float sinAngle = MathF.Sin(-rectAngleRad);

            float rotatedX = translatedX * cosAngle - translatedZ * sinAngle;
            float rotatedZ = translatedX * sinAngle + translatedZ * cosAngle;


            return (Math.Abs(rotatedX) <= rectWidth / 2) && (Math.Abs(rotatedZ) <= rectHeight / 2);
        }
        private bool IsCircleFullyContainedInAnyPlatform(Vector3 circleCenter)
        {
            const float circleRadius = 6f;
            const float platformSize = 20f;
            // Simplify by shrinking the platform: if the center point is within a smaller rectangle, then the whole circle is within the original rectangle
            float shrunkenSize = platformSize - 2 * circleRadius;

            for (int i = 0; i < SquarePositions.Count; i++)
            {
                if (IsPointInRotatedRect(circleCenter, SquarePositions[i], shrunkenSize, shrunkenSize, SquareAngles[i]))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
        [ScriptMethod(
            name: "Check Resurrection",
            eventType: EventTypeEnum.Chat,
            eventCondition: ["Type:regex:^(Echo|Party)$"]
        )]
        public async void CheckResurrection(Event @event, ScriptAccessory accessory)
        {
            string channel = @event["Type"].ToLower();
            if (!ReceivePartyCheckRequest && channel == "party") return;

            string message = @event["Message"];
            if (!message.StartsWith("Resurrection Check")) return;
            string[] parts = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int? targetCount = null;
            if (parts.Length > 1 && int.TryParse(parts[1], out int count))
            {
                targetCount = count;
            }
            var allResurrectionData = new List<Tuple<string, string, string, int>>();

            foreach (var gameObject in accessory.Data.Objects)
            {
                if (gameObject is IPlayerCharacter player)
                {
                    string playerName = player.Name.TextValue;
                    string classJob = player.ClassJob.Value.Name.ToString();
                    string supportJob = "None";
                    int resurrectionCount = 0;
                    bool hasResDebuff = false;
                    foreach (var status in player.StatusList)
                    {
                        if (status.StatusId == 4262 || status.StatusId == 4263)
                        {
                            resurrectionCount = status.Param; 
                            hasResDebuff = true;
                        }
                        if (_supportJobStatus.TryGetValue(status.StatusId, out var jobName))
                        {
                            supportJob = jobName;
                        }
                    }
                    if (hasResDebuff)
                    {
                        allResurrectionData.Add(new Tuple<string, string, string, int>(playerName, classJob, supportJob, resurrectionCount));
                    }
                }
            }
            var filteredData = targetCount.HasValue
                ? allResurrectionData.Where(t => t.Item4 == targetCount.Value).ToList()
                : allResurrectionData;

            if (filteredData.Count > 0)
            {
                var sortedData = filteredData.OrderBy(t => t.Item4).ToList();

                string title = targetCount.HasValue ? $"--- Players with resurrection count {targetCount.Value} ---" : "--- Resurrection Count Check ---";
                accessory.Method.SendChat($"/{channel} {title}");

                foreach (var data in sortedData)
                {
                    await Task.Delay(10);
                    accessory.Method.SendChat($"/{channel} {data.Item1} ({data.Item2} | {data.Item3}): {data.Item4}");
                }
            }
            else
            {
                await Task.Delay(10);
                string notFoundMessage = targetCount.HasValue ? $"No players with resurrection count {targetCount.Value} found." : "No players with resurrection restriction found.";
                accessory.Method.SendChat($"/{channel} {notFoundMessage}");
            }
        }
        [ScriptMethod(
            name: "Record Throw Money Count",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:41606"],
            userControl: false
        )]
        public void RecordMoneyThrow(Event @event, ScriptAccessory accessory)
        {
            var source = accessory.Data.Objects.SearchById(@event.SourceId);
            var target = accessory.Data.Objects.SearchById(@event.TargetId);

            // Ensure source and target exist, and target is not a player
            if (source == null || target == null || !(source is IBattleChara) || !(target is IBattleChara))
                return;

            string playerName = source.Name.TextValue;
            string bossName = target.Name.TextValue;

            lock (_moneyThrowLock)
            {
                if (!_moneyThrowCounts.ContainsKey(bossName))
                {
                    _moneyThrowCounts[bossName] = new Dictionary<string, int>();
                }
                if (!_moneyThrowCounts[bossName].ContainsKey(playerName))
                {
                    _moneyThrowCounts[bossName][playerName] = 0;
                }
                _moneyThrowCounts[bossName][playerName]++;
            }
        }
        [ScriptMethod(
            name: "Check Throw Money",
            eventType: EventTypeEnum.Chat,
            eventCondition: ["Type:regex:^(Echo|Party)$", "Message:Throw Money Check"]
        )]
        public async void CheckMoneyThrow(Event @event, ScriptAccessory accessory)
        {
            string channel = @event["Type"].ToLower();
            if (!ReceivePartyCheckRequest && channel == "party") return;

            Dictionary<string, List<KeyValuePair<string, int>>> sortedData;
            lock (_moneyThrowLock)
            {
                if (_moneyThrowCounts.Count == 0)
                {
                    accessory.Method.SendChat($"/{channel} No throw money data recorded.");
                    return;
                }

                sortedData = new Dictionary<string, List<KeyValuePair<string, int>>>();
                foreach (var bossEntry in _moneyThrowCounts)
                {
                    sortedData[bossEntry.Key] = bossEntry.Value.OrderBy(kvp => kvp.Value).ToList();
                }
            }

            foreach (var bossEntry in sortedData)
            {
                accessory.Method.SendChat($"/{channel} --- Throw Money Statistics for {bossEntry.Key} ---");
                foreach (var data in bossEntry.Value)
                {
                    await Task.Delay(100);
                    accessory.Method.SendChat($"/{channel} {data.Key}: {data.Value} times");
                }
            }
        }
        [ScriptMethod(
            name: "Clear Throw Money Data",
            eventType: EventTypeEnum.Chat,
            eventCondition: ["Type:regex:^(Echo|Party)$", "Message:Throw Money Clear"]
        )]
        public void ClearMoneyThrowData(Event @event, ScriptAccessory accessory)
        {
            string channel = @event["Type"].ToLower();
            if (!ReceivePartyCheckRequest && channel == "party") return;

            lock (_moneyThrowLock)
            {
                _moneyThrowCounts.Clear();
            }
            accessory.Method.SendChat($"/{channel} Throw money data cleared.");
        }
        [ScriptMethod(
            name: "Record Blue Potion Count",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:41633"],
            userControl: false
        )]
        public void RecordBluePotion(Event @event, ScriptAccessory accessory)
        {
            var source = accessory.Data.Objects.SearchById(@event.SourceId);
            var target = accessory.Data.Objects.SearchById(@event.TargetId);

            // Correction: ensure source and target are both players
            if (source == null || target == null || !(source is IPlayerCharacter) || !(target is IPlayerCharacter))
                return;

            string sourcePlayerName = source.Name.TextValue;
            string targetPlayerName = target.Name.TextValue;

            lock (_bluePotionLock)
            {
                // Outer key is target player, inner key is source player
                if (!_bluePotionCounts.ContainsKey(targetPlayerName))
                {
                    _bluePotionCounts[targetPlayerName] = new Dictionary<string, int>();
                }
                if (!_bluePotionCounts[targetPlayerName].ContainsKey(sourcePlayerName))
                {
                    _bluePotionCounts[targetPlayerName][sourcePlayerName] = 0;
                }
                _bluePotionCounts[targetPlayerName][sourcePlayerName]++;
            }
        }
        [ScriptMethod(
            name: "Check Blue Potion",
            eventType: EventTypeEnum.Chat,
            eventCondition: ["Type:regex:^(Echo|Party)$", "Message:Blue Potion Check"]
        )]
        public async void CheckBluePotion(Event @event, ScriptAccessory accessory)
        {
            string channel = @event["Type"].ToLower();            
            if (!ReceivePartyCheckRequest && channel == "party") return;

            Dictionary<string, List<KeyValuePair<string, int>>> sortedData;
            lock (_bluePotionLock)
            {
                if (_bluePotionCounts.Count == 0)
                {
                    accessory.Method.SendChat($"/{channel} No blue potion data recorded.");
                    return;
                }

                var partyMemberNames = Partycheck ? 
                    accessory.Data.PartyList.Select(id => accessory.Data.Objects.SearchById(id)?.Name.TextValue).Where(name => name != null).ToHashSet()
                    : null;

                sortedData = new Dictionary<string, List<KeyValuePair<string, int>>>();
                foreach (var bossEntry in _bluePotionCounts)
                {
                    var filteredPlayers = Partycheck ? 
                        bossEntry.Value.Where(kvp => partyMemberNames.Contains(kvp.Key) && partyMemberNames.Contains(bossEntry.Key)).ToList() 
                        : bossEntry.Value.ToList();

                    if(filteredPlayers.Count > 0)
                    {
                        sortedData[bossEntry.Key] = filteredPlayers.OrderBy(kvp => kvp.Value).ToList();
                    }
                }
            }

            if (sortedData.Count == 0)
            {
                accessory.Method.SendChat($"/{channel} No eligible blue potion data recorded within current range.");
                return;
            }

            foreach (var bossEntry in sortedData)
            {
                accessory.Method.SendChat($"/{channel} --- Blue Potion Statistics for {bossEntry.Key} ---");

                foreach (var data in bossEntry.Value)
                {
                    await Task.Delay(100);
                    accessory.Method.SendChat($"/{channel} {data.Key}: {data.Value} times");
                }
            }
        }
        [ScriptMethod(
            name: "Clear Blue Potion Data",
            eventType: EventTypeEnum.Chat,
            eventCondition: ["Type:regex:^(Echo|Party)$", "Message:Blue Potion Clear"]
        )]
        public void ClearBluePotionData(Event @event, ScriptAccessory accessory)
        {
            string channel = @event["Type"].ToLower();
            if (!ReceivePartyCheckRequest && channel == "party") return;
            
            lock (_bluePotionLock)
            {
                _bluePotionCounts.Clear();
            }
            accessory.Method.SendChat($"/{channel} Blue potion data cleared.");
        }

        [ScriptMethod(
            name: "Treasure Map",
            eventType: EventTypeEnum.ObjectChanged,
            eventCondition: ["Operate:Add", "DataId:regex:^(1754|1755)$"]
        )]
        public void CheckTreasureChest(Event @event, ScriptAccessory accessory)
        {
            if(Enable_Developer_Mode) accessory.Log.Debug("Found chest");
            //var chestid = @event.SourceId;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "Chest";
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = @event.SourceId;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.ScaleMode = ScaleMode.YByDistance;
            dp.Scale = new Vector2(1.5f);
            dp.DestoryAt = 120000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
        }

        [ScriptMethod(
            name: "Chest Removal",
            eventType: EventTypeEnum.ObjectChanged,
            eventCondition: ["Operate:Remove", "DataId:regex:^(1754|1755)$"],
            userControl: false
        )]
        public void RemoveTreasureChest(Event @event, ScriptAccessory accessory)
        {
            //var chestid = @event.SourceId;
            accessory.Method.RemoveDraw("Chest");
        }
        [ScriptMethod(
            name: "Mark Chemist",
            eventType: EventTypeEnum.Chat,
            eventCondition: ["Type:Echo"]
        )]
        public async void MarkChemists(Event @event, ScriptAccessory accessory)
        {
            if (@event["Message"] != "Mark Chemist") return;
        
            if (Enable_Developer_Mode) accessory.Log.Debug("'Mark Chemist' command detected...");
            accessory.Method.MarkClear();
            await Task.Delay(1000); // Wait for marks to clear
        
            var markType = MarkType.Attack1;
            int chemistsFound = 0;
        
            foreach (var gameObject in accessory.Data.Objects)
            {
                if (gameObject is IPlayerCharacter player)
                {
                    bool isChemist = false;
                    foreach (var status in player.StatusList)
                    {
                        if (status.StatusId == 4367) // Support Chemist Status ID
                        {
                            isChemist = true;
                            break;
                        }
                    }
        
                    if (isChemist)
                    {
                        accessory.Method.Mark(player.EntityId, markType);
                        chemistsFound++;
                        if (markType < MarkType.Attack8)
                        {
                            markType++;
                        }
                    }
                }
            }
        
            accessory.Method.SendChat($"/e Marked {chemistsFound} chemist(s).");
        }
        #region Helper_Functions

        private bool IsTank(IPlayerCharacter player)
        {
            if (player?.ClassJob.Value == null) return false;
            // Tank role ID is 1
            return player.ClassJob.Value.Role == 1;
        }

        private bool IsHealer(IPlayerCharacter player)
        {
            if (player?.ClassJob.Value == null) return false;
            // Healer role ID is 4
            return player.ClassJob.Value.Role == 4;
        }

        private bool IsDps(IPlayerCharacter player)
        {
            if (player?.ClassJob.Value == null) return false;
            // Anything that is not a tank or healer is DPS
            return !IsTank(player) && !IsHealer(player);
        }
        private Vector3 RotatePoint(Vector3 point, Vector3 center, float angleRad)
        {
            float s = MathF.Sin(angleRad);
            float c = MathF.Cos(angleRad);

            // Translate point back to origin
            point.X -= center.X;
            point.Z -= center.Z;

            // Rotate point
            float xnew = point.X * c - point.Z * s;
            float znew = point.X * s + point.Z * c;

            // Translate point back
            point.X = xnew + center.X;
            point.Z = znew + center.Z;
            return point;
        }
        #endregion


    }
    #region Helper_Classes_And_Methods

    public class DisplacementContainer
    {
        public Vector3 Pos;
        public long Delay;
        public long DestoryAt;

        public DisplacementContainer(Vector3 pos, long delay, long destoryAt)
        {
            Pos = pos;
            Delay = delay;
            DestoryAt = destoryAt;
        }
    }

    public class MultiDisDrawProp
    {
        public Vector4 Color_GoNow;
        public Vector4 Color_GoLater;
        public long BaseDelay;
        public float Width;
        public float EndCircleRadius;
        public DrawModeEnum DrawMode;

        public MultiDisDrawProp()
        {
            this.Color_GoNow = new(1, 1, 1, 1);
            this.Color_GoLater = new(0, 1, 1, 1);
            this.BaseDelay = 0;
            this.Width = 1.2f;
            this.EndCircleRadius = 0.65f;
            this.DrawMode = DrawModeEnum.Default;
        }
    }

    public static class HelperExtensions
    {
        internal static void MultiDisDraw(this ScriptAccessory accessory, List<DisplacementContainer> list, MultiDisDrawProp prop)
        {
            long startTimeMillis = prop.BaseDelay;
            const long preMs = 270;
            string guid = Guid.NewGuid().ToString();
            for (int i = 0; i < list.Count; i++)
            {
                int count = 0;
                DisplacementContainer dis = list[i];
                string name = $"_MultiDisDraw Part {i} : {guid} / ";

                // go now line guidance part
                DrawPropertiesEdit dp_goNowLine = accessory.Data.GetDefaultDrawProperties();
                dp_goNowLine.Name = name + count++;
                dp_goNowLine.Owner = (ulong)accessory.Data.Me;
                dp_goNowLine.Scale = new(prop.Width);
                dp_goNowLine.Delay = startTimeMillis;
                dp_goNowLine.DestoryAt = dis.DestoryAt;
                dp_goNowLine.ScaleMode |= ScaleMode.YByDistance;
                dp_goNowLine.TargetPosition = dis.Pos;
                dp_goNowLine.Color = prop.Color_GoNow;
                accessory.Method.SendDraw(prop.DrawMode, DrawTypeEnum.Displacement, dp_goNowLine);

                if (prop.EndCircleRadius > 0)
                {
                    DrawPropertiesEdit dp_goNowCircle = accessory.Data.GetDefaultDrawProperties();
                    dp_goNowCircle.Name = name + count++;
                    dp_goNowCircle.Position = dis.Pos;
                    dp_goNowCircle.Scale = new(prop.EndCircleRadius);
                    dp_goNowCircle.Delay = dp_goNowLine.Delay;
                    dp_goNowCircle.DestoryAt = dp_goNowLine.DestoryAt;
                    dp_goNowCircle.Color = prop.Color_GoNow;
                    accessory.Method.SendDraw(prop.DrawMode, DrawTypeEnum.Circle, dp_goNowCircle);
                }

                //If the current point is not the last point, perform the go later part
                if (i < list.Count - 1)
                {
                    DrawPropertiesEdit dp_goLaterLine = accessory.Data.GetDefaultDrawProperties();
                    dp_goLaterLine.Name = name + count++;
                    dp_goLaterLine.Position = list[i].Pos;
                    dp_goLaterLine.TargetPosition = list[i + 1].Pos;
                    dp_goLaterLine.Scale = new(prop.Width);
                    dp_goLaterLine.ScaleMode |= ScaleMode.YByDistance;
                    dp_goLaterLine.Delay = dp_goNowLine.Delay;
                    dp_goLaterLine.DestoryAt = dp_goNowLine.DestoryAt;
                    dp_goLaterLine.Color = prop.Color_GoLater;
                    accessory.Method.SendDraw(prop.DrawMode, DrawTypeEnum.Displacement, dp_goLaterLine);

                    if (prop.EndCircleRadius > 0)
                    {
                        DrawPropertiesEdit dp_goLaterCircle = accessory.Data.GetDefaultDrawProperties();
                        dp_goLaterCircle.Name = name + count++;
                        dp_goLaterCircle.Position = list[i + 1].Pos;
                        dp_goLaterCircle.Scale = new(prop.EndCircleRadius);
                        dp_goLaterCircle.Delay = dp_goLaterLine.Delay;
                        dp_goLaterCircle.DestoryAt = dp_goLaterLine.DestoryAt;
                        dp_goLaterCircle.Color = prop.Color_GoLater;
                        accessory.Method.SendDraw(prop.DrawMode, DrawTypeEnum.Circle, dp_goLaterCircle);
                    }
                }
                startTimeMillis += dis.DestoryAt;
            }
        }
    }

    #endregion
}
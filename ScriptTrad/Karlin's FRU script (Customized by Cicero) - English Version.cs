using System;
using System.Collections.Concurrent;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using CicerosKodakkuAssist.FuturesRewrittenUltimate;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Module.GameOperate;
using Newtonsoft.Json.Linq;

namespace CicerosKodakkuAssist.FuturesRewrittenUltimate
{

    [ScriptType(name: "Karlin's FRU script (Customized by Cicero, guid: "425a48b5-2a25-4ad4-9898-cfc084cc19f5") - English Version",
        territorys: [1238],
        $17251dc2a-c501-49cf-ba46-95d074e43338",
        version: "0.0.1.19",
        note: notesOfTheScript,
        Author: "Linoa235")]

    public class Futures_Rewritten_Ultimate
    {

        const string notesOfTheScript =
        """
        ***** Please read the note here carefully before running the script! *****
        
        This is the English translated version of the FRU script.
        
        """;
        
        #region User_Settings

        [UserSetting("----- Global Settings -----")]
        public bool _____Global_Settings_____ { get; set; } = true;
        [UserSetting("Enable Text Prompts")]
        public bool Enable_Text_Prompts { get; set; } = true;
        [UserSetting("Prompt Language")]
        public Languages_Of_Prompts Language_Of_Prompts { get; set; }
        [UserSetting("Weird Shenanigans")]
        public Weird_Shenanigans Weird_Shenanigan { get; set; } = Weird_Shenanigans.Astesia_ACR;

        [UserSetting("----- TTS Settings -----")]
        public bool _____TTS_Settings_____ { get; set; } = true;
        [UserSetting("Enable Vanilla TTS")]
        public bool Enable_Vanilla_TTS { get; set; } = true;
        [UserSetting("Enable Daily Routines TTS")]
        public bool Enable_Daily_Routines_TTS { get; set; } = false;

        [UserSetting("----- Phase 1 Settings -----")]
        public bool _____Phase1_Settings_____ { get; set; } = true;
        [UserSetting("P1 Utopian Sky Standby Positions")]
        public Phase1_Standby_Positions_Of_Utopian_Sky Phase1_Standby_Position_Of_Utopian_Sky { get; set; } = Phase1_Standby_Positions_Of_Utopian_Sky.Swap_OT_And_M2;
        [UserSetting("P1 Mark Players In Safe Positions")]
        public bool Phase1_Mark_Players_In_Safe_Positions { get; set; } = false;
        [UserSetting("P1 Burnt Strike Characteristics Color")]
        public ScriptColor Phase1_Colour_Of_Burnt_Strike_Characteristics { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
        [UserSetting("P1 Turn Of The Heavens Groups")]
        public Phase1_Groups_Of_Turn_Of_The_Heavens Phase1_Group_Of_Turn_Of_The_Heavens { get; set; } = Phase1_Groups_Of_Turn_Of_The_Heavens.MTOTH1H2_Go_North_MTM1_vary;
        [UserSetting("P1 Fall Of Faith Strats")]
        public Phase1_Strats_Of_Fall_Of_Faith Phase1_Strat_Of_Fall_Of_Faith { get; set; } = Phase1_Strats_Of_Fall_Of_Faith.Single_Line_In_HTD_Order;
        [UserSetting("P1 Mark Players During Fall Of Faith")]
        public bool Phase1_Mark_Players_During_Fall_Of_Faith { get; set; } = false;
        [UserSetting("P1 Orientation Benchmark During Fall Of Faith")]
        public Phase1_Orientation_Benchmarks_During_Fall_Of_Faith Phase1_Orientation_Benchmark_During_Fall_Of_Faith { get; set; } = Phase1_Orientation_Benchmarks_During_Fall_Of_Faith.High_Priority_Left_Facing_The_Boss;
        [UserSetting("P1 Towers Strat")]
        public Phase1_Strats_Of_Towers Phase1_Strat_Of_Towers { get; set; } = Phase1_Strats_Of_Towers.Completely_Based_On_Priority;

        [UserSetting("----- Phase 2 Settings -----")]
        public bool _____Phase2_Settings_____ { get; set; } = true;
        [UserSetting("P2 Strat After Knockback")]
        public Phase2_Strats_After_Knockback Phase2_Strat_After_Knockback { get; set; } = Phase2_Strats_After_Knockback.Clockwise_Both_Groups_Counterclockwise;
        [UserSetting("P2 Mirror Mirror Strat")]
        public Phase2_Strats_Of_Mirror_Mirror Phase2_Strat_Of_Mirror_Mirror { get; set; } = Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Closest_Red_Right_If_Same;
        [UserSetting("P2 Mirror Rough Guidance Color")]
        public ScriptColor Phase2_Colour_Of_Mirror_Rough_Guidance { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
        [UserSetting("P2 Potential Dangerous Zones Color")]
        public ScriptColor Phase2_Colour_Of_Potential_Dangerous_Zones { get; set; } = new() { V4 = new(1f, 0f, 0f, 1f) };
        [UserSetting("P2 Light Rampant Initial Protean Positions")]
        public Phase2_Initial_Protean_Positions_Of_Light_Rampant Phase2_Initial_Protean_Position_Of_Light_Rampant { get; set; } = Phase2_Initial_Protean_Positions_Of_Light_Rampant.Normal_Protean_Tanks_North_East_For_Both_Grey9;
        [UserSetting("P2 Light Rampant Strat")]
        public Phase2_Strats_Of_Light_Rampant Phase2_Strat_Of_Light_Rampant { get; set; } = Phase2_Strats_Of_Light_Rampant.New_Grey9;
        [UserSetting("P2 Rough Paths Color")]
        public ScriptColor Phase2_Colour_Of_Rough_Paths { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
        [UserSetting("P2 Sphere AOEs Color")]
        public ScriptColor Phase2_Colour_Of_Sphere_AOEs { get; set; } = new() { V4 = new(1f, 0f, 0f, 1f) };

        [UserSetting("----- Phase 3 Settings -----")]
        public bool _____Phase3_Settings_____ { get; set; } = true;
        [UserSetting("P3 First Half Strat")]
        public Phase3_Strats_Of_The_First_Half Phase3_Strat_Of_The_First_Half { get; set; } = Phase3_Strats_Of_The_First_Half.Moogle;
        [UserSetting("P3 Second Half Strat")]
        public Phase3_Strats_Of_The_Second_Half Phase3_Strat_Of_The_Second_Half { get; set; } = Phase3_Strats_Of_The_Second_Half.Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs;
        [UserSetting("P3 Double Group Strat Branch")]
        public Phase3_Branches_Of_The_Double_Group_Strat Phase3_Branch_Of_The_Double_Group_Strat { get; set; } = Phase3_Branches_Of_The_Double_Group_Strat.Based_On_Safe_Positions;
        [UserSetting("P3 Locomotive Strat Branch")]
        public Phase3_Branches_Of_The_Locomotive_Strat Phase3_Branch_Of_The_Locomotive_Strat { get; set; } = Phase3_Branches_Of_The_Locomotive_Strat.Others_As_Locomotives;
        [UserSetting("P3 Zone Division")]
        public Phase3_Divisions_Of_The_Zone Phase3_Division_Of_The_Zone { get; set; } = Phase3_Divisions_Of_The_Zone.North_To_Southwest_For_The_Left_Group;
        [UserSetting("P3 Rough Guidance Color")]
        public ScriptColor Phase3_Colour_Of_Rough_Guidance { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
        [UserSetting("P3 Penultimate Apocalypse Color")]
        public ScriptColor Phase3_Colour_Of_The_Penultimate_Apocalypse { get; set; } = new() { V4 = new(0, 1f, 1f, 1f) };
        [UserSetting("P3 Tank Who Baits Darkest Dance")]
        public Tanks Phase3_Tank_Who_Baits_Darkest_Dance { get; set; }
        [UserSetting("P3 Darkest Dance Color")]
        public ScriptColor Phase3_Colour_Of_Darkest_Dance { get; set; } = new() { V4 = new(1f, 0f, 0f, 1f) };

        [UserSetting("----- Phase 4 Settings -----")]
        public bool _____Phase4_Settings_____ { get; set; } = true;
        [UserSetting("P4 First Half Strat")]
        public Phase4_Strats_Of_The_First_Half Phase4_Strat_Of_The_First_Half { get; set; } = Phase4_Strats_Of_The_First_Half.Single_Swap_Baiting_After;
        [UserSetting("P4 Somber Dance Color")]
        public ScriptColor Phase4_Colour_Of_Somber_Dance { get; set; } = new() { V4 = new(1f, 0f, 0f, 1f) };
        [UserSetting("P4 Mark Players During The Second Half")]
        public bool Phase4_Mark_Players_During_The_Second_Half { get; set; } = false;
        [UserSetting("P4 Player Types To Be Marked")]
        public Phase4_Player_Types_To_Be_Marked Phase4_Player_Type_To_Be_Marked { get; set; }
        [UserSetting("P4 Wyrmclaw Player Priority")]
        public Phase4_Priorities_Of_The_Players_With_Wyrmclaw Phase4_Priority_Of_The_Players_With_Wyrmclaw { get; set; } = Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_THD_Order;
        [UserSetting("P4 Logic For Marking Teammates With Wyrmclaw")]
        public Phase4_Logics_Of_Marking_Teammates_With_Wyrmclaw Phase4_Logic_Of_Marking_Teammates_With_Wyrmclaw { get; set; } = Phase4_Logics_Of_Marking_Teammates_With_Wyrmclaw.Ignore1_And_Bind1_Go_West;
        [UserSetting("P4 Logic For Marking Teammates With Wyrmfang")]
        public Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang { get; set; }
        [UserSetting("P4 Drawing Duration Of Normal And Delayed Lights (seconds)")]
        public float Phase4_Drawing_Duration_Of_Normal_And_Delayed_Lights { get; set; } = 3f;
        [UserSetting("P4 Tidal Light Color")]
        public ScriptColor Phase4_Colour_Of_Tidal_Light { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
        [UserSetting("P4 Positions Before Knockback")]
        public Phase4_Positions_Before_Knockback Phase4_Position_Before_Knockback { get; set; } = Phase4_Positions_Before_Knockback.Normal;
        [UserSetting("P4 Residue Guidance Logic")]
        public Phase4_Logics_Of_Residue_Guidance Phase4_Logic_Of_Residue_Guidance { get; set; } = Phase4_Logics_Of_Residue_Guidance.According_To_Signs_On_Me;
        [UserSetting("P4 Residue Guidance Color")]
        public ScriptColor Phase4_Colour_Of_Residue_Guidance { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
        [UserSetting("P4 Attack1's Residue")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Attack1 { get; set; } = Phase4_Relative_Positions_Of_Residues.Eastmost;
        [UserSetting("P4 Attack2's Residue")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Attack2 { get; set; } = Phase4_Relative_Positions_Of_Residues.About_East;
        [UserSetting("P4 Attack3's Residue")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Attack3 { get; set; } = Phase4_Relative_Positions_Of_Residues.About_West;
        [UserSetting("P4 Attack4's Residue")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Attack4 { get; set; } = Phase4_Relative_Positions_Of_Residues.Westmost;
        [UserSetting("P4 Dark Eruption's Residue")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Dark_Eruption { get; set; } = Phase4_Relative_Positions_Of_Residues.Eastmost;
        [UserSetting("P4 Unholy Darkness's Residue")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Unholy_Darkness { get; set; } = Phase4_Relative_Positions_Of_Residues.About_East;
        [UserSetting("P4 Dark Blizzard III's Residue")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Dark_Blizzard_III { get; set; } = Phase4_Relative_Positions_Of_Residues.About_West;
        [UserSetting("P4 Dark Water III's Residue")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Dark_Water_III { get; set; } = Phase4_Relative_Positions_Of_Residues.Westmost;
        [UserSetting("P4 Drachen Wanderer Hitbox Length (meters)")]
        public float Phase4_Length_Of_Drachen_Wanderer_Hitboxes { get; set; } = 1.5f;
        [UserSetting("P4 Drachen Wanderer Hitbox Color")]
        public ScriptColor Phase4_Colour_Of_Drachen_Wanderer_Hitboxes { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };

        [UserSetting("----- Phase 5 Settings -----")]
        public bool _____Phase5_Settings_____ { get; set; } = true;
        [UserSetting("P5 Fulgent Blade Color")]
        public ScriptColor Phase5_Colour_Of_Fulgent_Blade { get; set; } = new() { V4 = new(0, 1f, 1f, 1f) };
        [UserSetting("P5 Current Guidance Step Color")]
        public ScriptColor Phase5_Colour_Of_The_Current_Guidance_Step { get; set; } = new() { V4 = new(0f, 1f, 0f, 1f) };
        [UserSetting("P5 Next Guidance Step Color")]
        public ScriptColor Phase5_Colour_Of_The_Next_Guidance_Step { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
        [UserSetting("P5 Boss Central Axis Color")]
        public ScriptColor Phase5_Colour_Of_The_Boss_Central_Axis { get; set; } = new() { V4 = new(1f, 0f, 0f, 1f) };
        [UserSetting("P5 Boss Faces Players After Fulgent Blade")]
        public bool Phase5_Boss_Faces_Players_After_Fulgent_Blade { get; set; } = true;
        [UserSetting("P5 Wings Dark And Light Strat")]
        public Phase5_Strats_Of_Wings_Dark_And_Light Phase5_Strat_Of_Wings_Dark_And_Light { get; set; } = Phase5_Strats_Of_Wings_Dark_And_Light.Grey9_Brain_Dead_MT_First_Tower_Opposite;
        [UserSetting("P5 Grey9 Brain Dead Strat Branch")]
        public Phase5_Branches_Of_The_Grey9_Brain_Dead_Strat Phase5_Branch_Of_The_Grey9_Brain_Dead_Strat { get; set; } = Phase5_Branches_Of_The_Grey9_Brain_Dead_Strat.Healers_First_Then_Melees_Left_Ranges_Right;
        [UserSetting("P5 Reverse Triangle Strat Branch")]
        public Phase5_Branches_Of_The_Reverse_Triangle_Strat Phase5_Branch_Of_The_Reverse_Triangle_Strat { get; set; }
        [UserSetting("P5 Provoke Reminder During Wings Dark And Light")]
        public bool Phase5_Reminder_To_Provoke_During_Wings_Dark_And_Light { get; set; } = true;
        [UserSetting("P5 Polarizing Strikes Order")]
        public Phase5_Orders_During_Polarizing_Strikes Phase5_Order_During_Polarizing_Strikes { get; set; } = Phase5_Orders_During_Polarizing_Strikes.Tanks_Melees_Ranges_Healers;

        [UserSetting("----- Developer Settings -----")]
        public bool _____Developer_Settings_____ { get; set; } = true;
        [UserSetting("Enable Developer Mode")]
        public bool Enable_Developer_Mode { get; set; } = false;
        
        #endregion

        #region Variables

        int? firstTargetIcon = null;
        int parse = -1;
        volatile bool isInPhase5 = false;
        System.Threading.AutoResetEvent shenaniganSemaphore = new System.Threading.AutoResetEvent(false);

        int p1UtopianSkyCounter = 0;
        readonly object p1UtopianSkyCounterLock = new object();
        int p1UtopianSkyCounter2 = 0;
        readonly object p1UtopianSkyCounter2Lock = new object();
        List<int> p1UtopianSkyRecord = new List<int> { 0, 0, 0, 0 };
        List<MarkType> phase1SafePositionMarks = [
            MarkType.Attack1,
            MarkType.Attack2,
            MarkType.Attack3,
            MarkType.Attack4,
            MarkType.Attack5,
            MarkType.Attack6,
            MarkType.Attack7,
            MarkType.Attack8
        ];
        bool p1UtopianSkyIsThunder = false;
        List<int> p1TurnOfHeavensTethers = [0, 0, 0, 0, 0, 0, 0, 0];
        volatile int phase1BurnishedGloryCastCount = 0;
        volatile List<int> phase1TetheredPlayersDuringFallOfFaith = [];
        volatile bool phase1IsInFallOfFaith = false;
        List<MarkType> phase1TetheredPlayerMarks = [
            MarkType.Stop1,
            MarkType.Bind1,
            MarkType.Stop2,
            MarkType.Bind2
        ];
        List<MarkType> phase1UntetheredPlayerMarks = [
            MarkType.Attack1,
            MarkType.Attack2,
            MarkType.Attack3,
            MarkType.Attack4
        ];
        volatile int phase1MarkTetheredSemaphore = 0;
        volatile int phase1ShortPromptSemaphore = 0;
        volatile int phase1DrawingSemaphore = 0;
        volatile int phase1MarkUntetheredSemaphore = 0;
        volatile int phase1FinalPromptSemaphore = 0;
        List<int> p1Towers = [0, 0, 0, 0];

        volatile string phase2BossId = "";
        bool p2DiamondDustIsIron = false;
        volatile List<int> phase2IcicleImpactPositions = [];
        Vector3 phase2KnockbackPosition = new Vector3(100, 0, 100);
        System.Threading.AutoResetEvent phase2GuidanceBeforeKnockbackSemaphore = new System.Threading.AutoResetEvent(false);
        System.Threading.AutoResetEvent phase2GuidanceAfterKnockbackSemaphore = new System.Threading.AutoResetEvent(false);
        volatile int phase2ColourlessMirrorProteanPosition = -1;
        System.Threading.AutoResetEvent phase2ColourlessMirrorConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
        volatile List<int> phase2RedMirrorProteanPositions = [];
        System.Threading.AutoResetEvent phase2RedMirrorsConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
        volatile List<int> phase2PlayersWithLuminousHammer = [];
        System.Threading.AutoResetEvent phase2LuminousHammerConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
        volatile List<int> phase2LightsteepedStacks = [0, 0, 0, 0, 0, 0, 0, 0];
        volatile bool phase2LightsteepedWritePermission = true;
        System.Threading.AutoResetEvent phase2FinalLightsteepedConfirmedSemaphore = new System.Threading.AutoResetEvent(false);

        volatile string phase3BossId = "";
        List<int> p3FireBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> p3WaterBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> p3ReturnBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> p3Lamp = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> p3LampDirection = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> p3Stack = [0, 0, 0, 0, 0, 0, 0, 0];
        bool p3ApocalypseDone = false;
        int p3ApocalypseDirection = 0;
        volatile List<Phase3_Types_Of_Dark_Water_III> phase3DarkWaterIIITypes = [
            Phase3_Types_Of_Dark_Water_III.NONE,
            Phase3_Types_Of_Dark_Water_III.NONE,
            Phase3_Types_Of_Dark_Water_III.NONE,
            Phase3_Types_Of_Dark_Water_III.NONE,
            Phase3_Types_Of_Dark_Water_III.NONE,
            Phase3_Types_Of_Dark_Water_III.NONE,
            Phase3_Types_Of_Dark_Water_III.NONE,
            Phase3_Types_Of_Dark_Water_III.NONE
        ];
        volatile List<MarkType> phase3PlayerMarks = [
            MarkType.Stop1,
            MarkType.Stop1,
            MarkType.Stop1,
            MarkType.Stop1,
            MarkType.Stop1,
            MarkType.Stop1,
            MarkType.Stop1,
            MarkType.Stop1
        ];
        volatile int phase3DarkWaterIIIProcessedCount = 0;
        volatile int phase3MarkRecordedCount = 0;
        System.Threading.AutoResetEvent phase3MarksRecordedSemaphore = new System.Threading.AutoResetEvent(false);
        volatile int phase3DarkWaterIIIRound = 0;
        volatile int phase3DarkWaterIIIRangeSemaphore = 0;
        volatile int phase3DarkWaterIIIGuidanceSemaphore = 0;
        List<int> phase3DoubleGroupPriority = [2, 3, 0, 1, 4, 5, 6, 7];
        List<int> phase3LocomotivePriority = [0, 1, 2, 3, 7, 6, 5, 4];
        volatile bool phase3InitialSafePositionsConfirmed = false;
        Vector3 phase3DoubleGroupLeftInitialSafePosition = new Vector3(100, 0, 100);
        Vector3 phase3DoubleGroupRightInitialSafePosition = new Vector3(100, 0, 100);
        Vector3 phase3DoubleGroupLeftSecondRoundStackPosition = new Vector3(100, 0, 100);
        Vector3 phase3DoubleGroupRightSecondRoundStackPosition = new Vector3(100, 0, 100);
        Vector3 phase3LocomotiveLeftInitialSafePosition = new Vector3(100, 0, 100);
        Vector3 phase3LocomotiveRightInitialSafePosition = new Vector3(100, 0, 100);
        Vector3 phase3LocomotiveLeftSecondRoundStackPosition = new Vector3(100, 0, 100);
        Vector3 phase3LocomotiveRightSecondRoundStackPosition = new Vector3(100, 0, 100);
        Vector3 phase3MoglinMeowLeftInitialSafePosition = new Vector3(100, 0, 100);
        Vector3 phase3MoglinMeowRightInitialSafePosition = new Vector3(100, 0, 100);
        Vector3 phase3MoglinMeowLeftSecondRoundStackPosition = new Vector3(100, 0, 100);
        Vector3 phase3MoglinMeowRightSecondRoundStackPosition = new Vector3(100, 0, 100);
        Vector3 phase3FinalBossPosition = new Vector3(100, 0, 100);

        ulong p4FragmentId;
        List<int> p4Tether = [-1, -1, -1, -1, -1, -1, -1, -1];
        List<int> p4Stack = [0, 0, 0, 0, 0, 0, 0, 0];
        bool p4TetherDone = false;
        List<int> p4WyrmclawBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        volatile int phase4MajorDebuffCount = 0;
        readonly object phase4MajorDebuffLock = new object();
        System.Threading.AutoResetEvent phase4MajorDebuffsConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
        volatile int phase4IncidentalDebuffCount = 0;
        readonly object phase4IncidentalDebuffLock = new object();
        System.Threading.AutoResetEvent phase4IncidentalDebuffsConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
        List<MarkType> phase4WyrmfangMarks = [
            MarkType.Attack1,
            MarkType.Attack2,
            MarkType.Attack3,
            MarkType.Attack4
        ];
        List<int> p4OtherBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        volatile List<MarkType> phase4WyrmfangPlayerMarks = [
            MarkType.Cross,
            MarkType.Cross,
            MarkType.Cross,
            MarkType.Cross,
            MarkType.Cross,
            MarkType.Cross,
            MarkType.Cross,
            MarkType.Cross
        ];
        int p4BlueTether = 0;
        List<Vector3> p4WaterPositions = [];
        volatile string phase4DrachenWandererId1 = "";
        volatile string phase4DrachenWandererId2 = "";
        readonly object phase4DrachenWandererIdLock = new object();
        volatile int phase4WyrmclawRemovalCount = 0;
        volatile List<ulong> phase4ResidueIdsEastToWest = [0, 0, 0, 0];
        volatile bool phase4ResidueGuidanceGenerated = false;
        System.Threading.ManualResetEvent phase4ManualReset = new System.Threading.ManualResetEvent(false);
        int phase4TetherCount = 0;
        private static CrystallizeTime _cry = new();
        private static PriorityDict _pd = new();
        private static List<System.Threading.ManualResetEvent> _events = [.. Enumerable.Range(0, 20).Select(_ => new System.Threading.ManualResetEvent(false))];
        
        volatile string phase5BossId = "";
        volatile bool phase5FirstTowerAcquired = false;
        volatile string phase5FirstTowerIndex = "";
        volatile bool phase5InitialPositionConfirmed = false;
        Vector3 phase5LeftSouthPosition = new Vector3(98, 0, 107);
        Vector3 phase5RightSouthPosition = new Vector3(102, 0, 107);
        Vector3 phase5LeftNortheastPosition = new Vector3(107.06f, 0, 98.23f);
        Vector3 phase5RightNortheastPosition = new Vector3(105.06f, 0, 94.77f);
        Vector3 phase5LeftNorthwestPosition = new Vector3(94.94f, 0, 94.77f);
        Vector3 phase5RightNorthwestPosition = new Vector3(92.94f, 0, 98.23f);
        Vector3 phase5StandbySouthNortheast = new Vector3(106.06f, 0, 103.50f);
        Vector3 phase5StandbySouthNorthwest = new Vector3(93.94f, 0, 103.50f);
        Vector3 phase5StandbyNortheastNorthwest = new Vector3(100, 0, 93);
        Vector3 phase5LeftHitPosition = new Vector3(95.93f, 0, 104.07f);
        Vector3 phase5LeftCoverPosition = new Vector3(93.81f, 0, 106.19f);
        Vector3 phase5LeftStandbyPosition = new Vector3(99.24f, 0, 108.72f);
        Vector3 phase5RightHitPosition = new Vector3(104.07f, 0, 104.07f);
        Vector3 phase5RightCoverPosition = new Vector3(106.19f, 0, 106.19f);
        Vector3 phase5RightStandbyPosition = new Vector3(100.76f, 0, 108.72f);
        private string Phase = "";
        private Vector2? Point1 = new Vector2(0f, 0f);
        private Vector2? Point2 = new Vector2(0f, 0f);
        private Vector2? Point3 = new Vector2(0f, 0f);
        private Vector2? MiddlePoint = new Vector2(0f, 0f);
        private onPoint? OnPoint = null;
        private int bladeCount = 0;
        private ConcurrentBag<Blade> blades = new ConcurrentBag<Blade>();
        private List<Blade> p1p3Blades = new List<Blade>();
        private List<onPoint> onPoints = new List<onPoint>();
        private List<Vector2?> BladeRoutes;
        private readonly object bladeLock = new object();
        private readonly object drawLock = new object();
        
        #endregion

        #region Enumerations_And_Classes

        public enum Languages_Of_Prompts
        {
            Simplified_Chinese,
            English
        }

        public enum Weird_Shenanigans
        {
            Disabled,
            Astesia_ACR,
            Res_Gestae_Populi_Romani_II_Bellum_Hannibalicum,
            Helldivers,
            Call_Of_Duty_Death_Quotes,
            StarCraft_SCBoy
        }

        public enum Tanks
        {
            MT,
            OT_ST
        }

        public enum Phase1_Standby_Positions_Of_Utopian_Sky
        {
            Swap_OT_And_M2,
            Both_Tanks_Go_Center
        }

        public enum Phase1_Groups_Of_Turn_Of_The_Heavens
        {
            MTOTH1H2_Go_North_MTM1_vary,
            MTH1M1R1_Go_North_MTOT_vary,
            MTOTR1R2_Go_North_MTM1_vary
        }

        public enum Phase1_Strats_Of_Fall_Of_Faith
        {
            Single_Line_In_THD_Order,
            Single_Line_In_HTD_Order,
            Single_Line_In_H1TDH2_Order,
            Double_Lines_H12MOT_Left_M12R12_Right,
            Double_Lines_MOTH12_Left_M12R12_Right
        }

        public enum Phase1_Orientation_Benchmarks_During_Fall_Of_Faith
        {
            High_Priority_Left_Facing_Due_North,
            High_Priority_Left_Facing_The_Boss
        }

        public enum Phase1_Strats_Of_Towers
        {
            Completely_Based_On_Priority,
            Fixed_H1H2R2_Priority_For_Rest,
            Fixed_H1H2R2_Rest_Fill_Vacancies
        }

        public enum Phase2_Strats_After_Knockback
        {
            Clockwise_One_Group_Counterclockwise,
            Counterclockwise_One_Group_Clockwise,
            Clockwise_Both_Groups_Counterclockwise,
            Counterclockwise_Both_Groups_Clockwise
        }

        public enum Phase2_Strats_Of_Mirror_Mirror
        {
            Melee_Group_Left_Red,
            Melee_Group_Right_Red,
            Melee_Group_Closest_Red_Left_If_Same,
            Melee_Group_Closest_Red_Right_If_Same
        }

        public enum Phase2_Initial_Protean_Positions_Of_Light_Rampant
        {
            Supporters_North_MOTH12_For_JPPF_And_L,
            Supporters_North_H12MOT_For_JPPF_And_L,
            Normal_Protean_Tanks_North_East_For_Both_Grey9
        }

        public enum Phase2_Strats_Of_Light_Rampant
        {
            Star_Of_David_Japanese_PF,
            New_Grey9,
            Lucrezia,
            Obsolete_Old_Grey9
        }

        public enum Phase3_Strats_Of_The_First_Half
        {
            Moogle,
            Other_Strats_Are_Work_In_Progress
        }

        public enum Phase3_Strats_Of_The_Second_Half
        {
            Double_Group,
            High_Priority_As_Locomotives,
            Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs
        }

        public enum Phase3_Branches_Of_The_Double_Group_Strat
        {
            Based_On_Safe_Positions,
            Based_On_The_Second_Apocalypse
        }

        public enum Phase3_Branches_Of_The_Locomotive_Strat
        {
            MT_And_M1_As_Locomotives,
            Others_As_Locomotives
        }

        public enum Phase3_Divisions_Of_The_Zone
        {
            North_To_Southwest_For_The_Left_Group,
            Northwest_To_South_For_The_Left_Group
        }

        public enum Phase3_Types_Of_Dark_Water_III
        {
            LONG,
            MEDIUM,
            SHORT,
            NONE
        }

        public enum Phase4_Strats_Of_The_First_Half
        {
            Single_Swap_Baiting_After,
            Single_Swap_Baiting_First,
            Double_Swaps_Baiting_First
        }

        public enum Phase4_Player_Types_To_Be_Marked
        {
            Both_Wyrmclaw_And_Wyrmfang,
            Only_Wyrmclaw,
            Only_Wyrmfang
        }

        public enum Phase4_Priorities_Of_The_Players_With_Wyrmclaw
        {
            In_THD_Order,
            In_HTD_Order,
            In_H1TDH2_Order
        }

        public enum Phase4_Logics_Of_Marking_Teammates_With_Wyrmclaw
        {
            Ignore1_And_Bind1_Go_West,
            Ignore1_And_Ignore2_Go_West
        }

        public enum Phase4_Logics_Of_Residue_Guidance
        {
            According_To_Signs_On_Me,
            According_To_Debuffs
        }

        public enum Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang
        {
            According_To_Debuffs_1234_From_East_To_West,
            According_To_Debuffs_1342_From_East_To_West,
            According_To_The_Priority_THD,
            According_To_The_Priority_HTD,
            According_To_The_Priority_H1TDH2
        }

        public enum Phase4_Relative_Positions_Of_Residues
        {
            Eastmost,
            About_East,
            About_West,
            Westmost,
            Unknown
        }

        public enum Phase4_Positions_Before_Knockback
        {
            Normal,
            Y_Formation_Japanese_PF
        }

        public enum Phase5_Strats_Of_Wings_Dark_And_Light
        {
            Grey9_Brain_Dead_MT_First_Tower_Opposite,
            Reverse_Triangle_MT_Baits_In_Towers
        }

        public enum Phase5_Branches_Of_The_Grey9_Brain_Dead_Strat
        {
            Healers_First_Then_Melees_Left_Ranges_Right,
            Melees_First_Then_Healers_Left_Ranges_Right,
            Healer_First_Then_Melees_Farther_Ranges_Closer
        }

        public enum Phase5_Branches_Of_The_Reverse_Triangle_Strat
        {
            Healers_First_Then_Melees_Left_Ranges_Right,
            Melees_First_Then_Healers_Left_Ranges_Right
        }

        public enum Phase5_Orders_During_Polarizing_Strikes
        {
            Tanks_Melees_Ranges_Healers,
            Tanks_Healers_Melees_Ranges
        }

        public class Blade
        {
            public UInt32 Id { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Rotation { get; set; }
            public Blade(UInt32 id, double x, double y, double rotation)
            {
                Id = id;
                X = x;
                Y = y;
                Rotation = rotation;
            }
        }

        public class onPoint
        {
            public string Name { get; set; }
            public Vector2 OnCoord { get; set; }
            public Vector2 Coord1 { get; set; }
            public Vector2 Coord2 { get; set; }
            public Vector2 Coord3 { get; set; }
            public Vector2 Coord4 { get; set; }

            public onPoint(string name, Vector2 onCoord, Vector2 coord1, Vector2 coord2, Vector2 coord3, Vector2 coord4)
            {
                Name = name;
                this.OnCoord = onCoord;
                this.Coord1 = coord1;
                this.Coord2 = coord2;
                this.Coord3 = coord3;
                this.Coord4 = coord4;
            }
        }

        #endregion

        #region Initialization

        private void ResetPoints()
        {
            onPoints.Clear();
            onPoints.Add(new onPoint("A", new Vector2(100, 93), new Vector2(100, 91.5f), new Vector2(101.4f, 92.9f), new Vector2(100, 94.3f), new Vector2(98.6f, 92.9f)));
            onPoints.Add(new onPoint("B", new Vector2(107, 100), new Vector2(108.5f, 100), new Vector2(107, 101.4f), new Vector2(105.6f, 100), new Vector2(107, 98.6f)));
            onPoints.Add(new onPoint("C", new Vector2(100, 107), new Vector2(100, 108.5f), new Vector2(98.6f, 107), new Vector2(100, 105.6f), new Vector2(101.4f, 107.1f)));
            onPoints.Add(new onPoint("D", new Vector2(93, 100), new Vector2(91.5f, 100), new Vector2(93, 98.6f), new Vector2(94.4f, 100), new Vector2(93, 101.4f)));
        }

        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(".*");

            if (Phase1_Mark_Players_In_Safe_Positions
               ||
               Phase1_Mark_Players_During_Fall_Of_Faith
               ||
               Phase4_Mark_Players_During_The_Second_Half)
            {
                accessory.Method.MarkClear();
            }

            parse = 1;
            isInPhase5 = false;
            shenaniganSemaphore.Set();

            p1UtopianSkyRecord = new List<int> { 0, 0, 0, 0 };
            p1UtopianSkyCounter = 0;
            p1UtopianSkyCounter2 = 0;
            p1TurnOfHeavensTethers = [0, 0, 0, 0, 0, 0, 0, 0];
            phase1BurnishedGloryCastCount = 0;
            phase1TetheredPlayersDuringFallOfFaith = [];
            phase1IsInFallOfFaith = false;
            phase1MarkTetheredSemaphore = 0;
            phase1ShortPromptSemaphore = 0;
            phase1DrawingSemaphore = 0;
            phase1MarkUntetheredSemaphore = 0;
            phase1FinalPromptSemaphore = 0;
            p1Towers = [0, 0, 0, 0];

            phase2BossId = "";
            phase2IcicleImpactPositions.Clear();
            phase2KnockbackPosition = new Vector3(100, 0, 100);
            phase2GuidanceBeforeKnockbackSemaphore = new System.Threading.AutoResetEvent(false);
            phase2GuidanceAfterKnockbackSemaphore = new System.Threading.AutoResetEvent(false);
            phase2ColourlessMirrorProteanPosition = -1;
            phase2ColourlessMirrorConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
            phase2RedMirrorProteanPositions.Clear();
            phase2RedMirrorsConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
            phase2PlayersWithLuminousHammer.Clear();
            phase2LuminousHammerConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
            phase2LightsteepedStacks = [0, 0, 0, 0, 0, 0, 0, 0];
            phase2LightsteepedWritePermission = true;
            phase2FinalLightsteepedConfirmedSemaphore = new System.Threading.AutoResetEvent(false);

            phase3BossId = "";
            p3ApocalypseDone = false;
            p3Stack = [0, 0, 0, 0, 0, 0, 0, 0];
            phase3DarkWaterIIITypes = [
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE
            ];
            phase3PlayerMarks = [
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1
            ];
            phase3DarkWaterIIIProcessedCount = 0;
            phase3MarkRecordedCount = 0;
            phase3MarksRecordedSemaphore = new System.Threading.AutoResetEvent(false);
            phase3DarkWaterIIIRound = 0;
            phase3DarkWaterIIIRangeSemaphore = 0;
            phase3DarkWaterIIIGuidanceSemaphore = 0;
            phase3InitialSafePositionsConfirmed = false;
            phase3DoubleGroupLeftInitialSafePosition = new Vector3(100, 0, 100);
            phase3DoubleGroupRightInitialSafePosition = new Vector3(100, 0, 100);
            phase3DoubleGroupLeftSecondRoundStackPosition = new Vector3(100, 0, 100);
            phase3DoubleGroupRightSecondRoundStackPosition = new Vector3(100, 0, 100);
            phase3LocomotiveLeftInitialSafePosition = new Vector3(100, 0, 100);
            phase3LocomotiveRightInitialSafePosition = new Vector3(100, 0, 100);
            phase3LocomotiveLeftSecondRoundStackPosition = new Vector3(100, 0, 100);
            phase3LocomotiveRightSecondRoundStackPosition = new Vector3(100, 0, 100);
            phase3MoglinMeowLeftInitialSafePosition = new Vector3(100, 0, 100);
            phase3MoglinMeowRightInitialSafePosition = new Vector3(100, 0, 100);
            phase3MoglinMeowLeftSecondRoundStackPosition = new Vector3(100, 0, 100);
            phase3MoglinMeowRightSecondRoundStackPosition = new Vector3(100, 0, 100);
            phase3FinalBossPosition = new Vector3(100, 0, 100);

            p4FragmentId = 0;
            p4Tether = [-1, -1, -1, -1, -1, -1, -1, -1];
            p4Stack = [0, 0, 0, 0, 0, 0, 0, 0];
            p4TetherDone = false;
            p4WyrmclawBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            phase4MajorDebuffCount = 0;
            phase4MajorDebuffsConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
            phase4IncidentalDebuffCount = 0;
            phase4IncidentalDebuffsConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
            p4OtherBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            phase4WyrmfangPlayerMarks = [
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross
            ];
            p4BlueTether = 0;
            p4WaterPositions = [];
            phase4DrachenWandererId1 = "";
            phase4DrachenWandererId2 = "";
            phase4WyrmclawRemovalCount = 0;
            phase4ResidueIdsEastToWest = [0, 0, 0, 0];
            phase4ResidueGuidanceGenerated = false;
            phase4ManualReset = new System.Threading.ManualResetEvent(false);
            phase4TetherCount = 0;

            phase5BossId = "";
            phase5FirstTowerAcquired = false;
            phase5FirstTowerIndex = "";
            phase5InitialPositionConfirmed = false;
            blades.Clear();
            p1p3Blades.Clear();
            BladeRoutes = Enumerable.Repeat<Vector2?>(null, 7).ToList();
            ResetPoints();
        }

        #endregion

        #region Weird_Shenanigans

        [ScriptMethod(name: "Weird Shenanigans",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: ["DataId:9020"],
            suppress: 15000,
            userControl: false)]

        public void Weird_Shenanigans(Event @event, ScriptAccessory accessory)
        {
            shenaniganSemaphore.WaitOne();
            System.Threading.Thread.MemoryBarrier();
            System.Threading.Thread.Sleep(3500);
            System.Threading.Thread.MemoryBarrier();

            if (Weird_Shenanigan == Weird_Shenanigans.Disabled)
            {
                shenaniganSemaphore = new System.Threading.AutoResetEvent(false);
                return;
            }

            System.Threading.Thread.MemoryBarrier();
            System.Random seed = new System.Random();
            string prompt = "";

            if (Weird_Shenanigan == Weird_Shenanigans.Astesia_ACR)
            {
                int randomNumber = seed.Next(1, 101);
                if (randomNumber <= 25)
                {
                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese)
                    {
                        prompt = "Welcome to Astesia The Piggy's ACR!";
                    }
                    if (Language_Of_Prompts == Languages_Of_Prompts.English)
                    {
                        prompt = "You're now running Astesia The Piggy's ACR!";
                    }
                }
                else
                {
                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese)
                    {
                        prompt = "Welcome to Astesia's ACR!";
                    }
                    if (Language_Of_Prompts == Languages_Of_Prompts.English)
                    {
                        prompt = "You're now running Astesia's ACR!";
                    }
                }
            }

            if (Weird_Shenanigan == Weird_Shenanigans.Res_Gestae_Populi_Romani_II_Bellum_Hannibalicum)
            {
                List<string> englishContents = [
                    "The First Punic War:\nRome and Carthage clashed in their first large-scale land and naval war over the control of Sicily. In its naval debut, Rome nearly annihilated the Carthaginian fleet. Ultimately, Rome emerged victorious and seized Sicily.",
                    "After the First Punic War:\nCarthage shifted its focus to expanding into Spain to compensate for its losses. During this period, Hannibal, the son of General Hamilcar from the First Punic War, made his legendary entrance onto the stage of history.",
                    "Early Phase of the Second Punic War:\nCarthage initiated the Second Punic War. Hannibal led his army through Gaul and over the Alps in a miraculous feat, inflicting devastating defeats on Rome, including the complete annihilation of Roman forces at the Battle of Cannae.",
                    "Middle Phase of the Second Punic War Part I:\nAlthough Hannibal won victory after victory in Italy, he failed to capture Rome. Meanwhile, Rome's counteroffensive in Spain was crushed. At this moment of existential crisis for the Republic, Scipio volunteered before the Senate — the legendary Roman general stepped into the spotlight.",
                    "Middle Phase of the Second Punic War Part II:\nScipio won a series of brilliant victories in Spain, defeating two Carthaginian armies despite being outnumbered. Hannibal's reinforcements entering Italy were intercepted and annihilated. Rome regained control over all major cities in southern Italy.",
                    "Late Phase of the Second Punic War:\nScipio landed in North Africa and took control of Numidia. The Carthaginian elders recalled Hannibal home. In the epic Battle of Zama, the two legendary generals faced off, and Scipio used Hannibal's own tactics to decisively defeat him. Rome triumphed completely.",
                    "After the Punic Wars:\nScipio was forced to resign and retire due to political attacks by his rival Cato. He died shortly after, lamenting: \"Ungrateful country, you won't even have my bones\". Hannibal fled to the Hellenistic Phoenician cities in Greece and eventually took poison to end his life in Asia Minor before being cornered by Roman pursuers.",
                    "The Fall of Macedonia:\nRome defeated the Kingdom of Macedonia in the Third Macedonian War and dissolved it, bringing Greece under Roman control and achieving dominance over the Mediterranean.",
                    "The Fall of Carthage:\nRome launched the Third Punic War. Carthage was captured and utterly destroyed. The Carthaginian state ceased to exist. The Romans, gazing over the Mediterranean, left behind a proud victor's declaration: \"Mare Nostrum (Our Sea)\"."
                ];
                int randomNumber = seed.Next(0, 9);
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    prompt = englishContents[randomNumber];
                }
            }

            if (Weird_Shenanigan == Weird_Shenanigans.Helldivers)
            {
                int randomNumber = seed.Next(1, 101);
                if (randomNumber <= 10)
                {
                    prompt = "Here? Here! Here? What about here? Here? Here! Here! Here? What about here?\nHellpod launch suspended.";
                }
                else
                {
                    List<string> systemNames = [
                        "Malevelon Creek", "Meridian", "Turing", "Angel's Venture", "Hellmire",
                        "Cyberstan", "Calypso", "Moradesh", "Fenrir III", "Chort Bay",
                        "Marfark", "Omicron", "Vernen Wells", "Genesis Prime", "Mog"
                    ];
                    int randomNumber2 = seed.Next(0, 15);
                    prompt = $"Initiating FTL Jump to, the {systemNames[randomNumber2]} system.\nFTL Jump successful.\nHellpods primed.\nMission coordinates locked.";
                }
            }

            if (Weird_Shenanigan == Weird_Shenanigans.Call_Of_Duty_Death_Quotes)
            {
                List<string> englishContents = [
                    "Del Giordano le rive saluta, di Sionne le torri atterrate...",
                    "Through the graves the wind is blowing.",
                    "The enslaved were not bricks in your road, and their lives were not chapters in your redemptive history.",
                    "Thou hast made us for thyself, O Lord, and our heart is restless until it finds its rest in thee.",
                    "No! I'm alive! I will live forever! I have in my heart what does not die!",
                    "The living denied a table; the dead get a whole coffin.",
                    "What was born by the sword shall die by the sword.",
                    "Injustice anywhere is a threat to justice everywhere.",
                    "I die without seeing the dawn brighten over my native land.",
                    "I entered a kind world and loved it wholeheartedly. I leave in an evil one and have nothing to say by way of farewells.",
                    "You cannot nurture a man with pain, nor can you feed him with anger.",
                    "\"Hemos pasado!\"",
                    "The Banteng has been led to slaughter - and the villagers feast on its remnants.",
                    "Those who wear the shirt of fire will realize it burns as much as it warms.",
                    "What is built on sand sooner or later would tumble down.",
                    "A faithful man shall abound with blessings.",
                    "She smiled sadly, as she flew into the night.",
                    "Only in death does duty end.",
                    "The end may justify the means as long as there is something that justifies the end.",
                    "Sing your death song and die like a hero going home.",
                    "The mutineers ride into the night.",
                    "The specter of homicidal violence has appeared in history whenever it was believed that the hypocritical respect for formalities could replace the obedience of moral obligations.",
                    "Nothing more cruel and inhuman than a war. Nothing more desirable than peace. But peace has its causes, it is an effect. The effect of respect for mutual rights.",
                    "One by one the righteous fell, and the ills of ignorance permeated.",
                    "They defended the grains of sand in the desert to the last drop of their blood.",
                    "All history is man's efforts to realise ideals.\n- Éamon de Valera, 1929",
                    "Let us dedicate ourselves to what the Greeks wrote so many years ago: to tame the savageness of man and make gentle the life of this world.\n- Robert F. Kennedy, 1968",
                    "Yesterday is not ours to recover, but tomorrow is ours to win or lose.\n- Lyndon B. Johnson, 1968",
                    "The end of hope is the beginning of death.\n- Charles de Gaulle, 1945",
                    "The day I leave the power, inside my pockets will only be dust.\n- Antonio de Oliveira Salazar, 1968",
                    "When smashing monuments, save the pedestals. They always come in handy.\n- Stanisław Jerzy Lec, 1957",
                    "Fear not the path of truth for the lack of people walking on it.\n- Robert F. Kennedy, 1968",
                    "The rocket worked perfectly, except for landing on the wrong planet.\n- Wernher von Braun upon the first V-2 hitting London, 1944",
                    "A man is not finished when he's defeated. He's finished when he quits.\n- Richard Nixon, 1962",
                    "Do not pray for easy lives, pray to be stronger men.\n- John F. Kennedy, 1963",
                    "Nature does not know extinction, only transformation.\n- Wernher von Braun, 1962",
                    "The optimist thinks this is the best of all possible worlds. The pessimist fears it is true.\n- James Branch Cabell, The Silver Stallion, 1926",
                    "One seldom recognizes the devil when he is putting his hand on your shoulder.\n- Albert Speer, 1972",
                    "Laws are silent in times of war.\n- Marcus Tullius Cicero, 52 BC",
                    "They don't ask much of you. They only want you to hate the things you love and to love the things you despise.\n- Boris Pasternak, 1960",
                    "Most economic fallacies derive from the tendency to assume that there is a fixed pie, that one party can gain only at the expense of another.\n- Milton Friedman, 1980",
                    "There are three kinds of lies: lies, damned lies, and statistics.\n- Mark Twain, 1907",
                    "Bite us once, shame on the dog; bite us repeatedly, shame on us for allowing it.\n- Phyllis Schlafly, 1995",
                    "I know not with what weapons World War III will be fought, but World War IV will be fought with sticks and stones.\n- Albert Einstein, 1949",
                    "You can believe in Feng Shui if you want, but ultimately people control their own fate.\n- Li Ka-shing, 1969",
                    "I believe it is a big mistake to think that money is the only way to compensate a person for his work. People need money, but they also want to be happy in their work and proud of it.\n- Morita Akio, 1966",
                    "A good reputation for yourself and your company is an invaluable asset not reflected in the balance sheets.\n- Li Ka-shing, 1967",
                    "Knowledge is your real companion, your life long companion, not fortune. Fortune can disappear.\n- Stanley Ho, 1966",
                    "People sometimes say: \"we are in a society that is all rotten, all dishonest.\" That is not true. There are still so many good people, so many honest people.\n- John Paul I, 1978",
                    "Half the confusion in the world comes from not knowing how little we need.\n- Admiral Richard E. Byrd on his time in Antarctica, 1935"
                ];
                int randomNumber = seed.Next(0, 50);
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    prompt = englishContents[randomNumber];
                }
            }

            if (Weird_Shenanigan == Weird_Shenanigans.StarCraft_SCBoy)
            {
                List<string> contents = [
                    "Hey, your ally's base is being destroyed!",
                    "Your buddy is getting wrecked, aren't you going to help?",
                    "Your ally is under attack, are you just going to watch?",
                    "The base is being attacked! Better check it out!",
                    "The base is being hit! Time to base trade!",
                    "Can't warp in units without a power field!",
                    "Nuke incoming! GG!",
                    "See that red dot? That's a nuke!",
                    "The enemy ignores you and throws a fusion strike at you.",
                    "Base upgrade complete. I feel great!",
                    "Your command center has been upgraded!",
                    "Five! Four! Three! Two! One! GG!",
                    "This upgrade is critical and timely.",
                    "Hey, a fight broke out? Show off those moves!",
                    "Your forces are taking massive damage!",
                    "Your forces are under attack. What?",
                    "Hey hey, the enemy is hitting your troops, buddy!",
                    "Your noble protoss warriors are about to die!",
                    "Your protoss forces are crying out!",
                    "The enemy is using pesticide on you!",
                    "Your zerg swarm has been destroyed!",
                    "Pause the game.",
                    "Ok, I'm back!",
                    "Feeling like the gas is drained...",
                    "Something's in the way, clear the ground first. APM skyrocket!",
                    "Can't land here, what are you thinking?",
                    "Hey, you're supply capped! Time to attack!",
                    "Supply capped? Just F2 A-move!",
                    "Mineral patches depleted, do you have an expansion?",
                    "Zerg buildings must be placed on creep. You can't even find creep, can you?",
                    "I can't take this anymore, how can you build without creep? Guess!",
                    "Low supply, your unit production is insane!",
                    "Please warp in more pylons. You can try a proxy pylon!",
                    "Nuclear weapon ready, kill them all!",
                    "Low on minerals. Pay attention to your spending!",
                    "Ah, out of minerals! Life without money is hard!",
                    "So sneaky, stealing our SCVs!",
                    "Why do you keep hitting our SCVs? You're asking for it!",
                    "Your SCV is being attacked! No mercy for you!",
                    "They're focus-firing your probe, this doesn't look good!"
                ];
                int randomNumber = seed.Next(0, 40);
                prompt = contents[randomNumber];
            }

            if (!prompt.Equals(""))
            {
                if (Enable_Text_Prompts)
                {
                    accessory.Method.TextInfo(prompt, 11500);
                }
                if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                {
                    accessory.TTS(prompt, Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
                }
            }

            System.Threading.Thread.MemoryBarrier();
            shenaniganSemaphore = new System.Threading.AutoResetEvent(false);
        }

        #endregion

        #region Phase_1

        [ScriptMethod(name: "----- Phase 1 -----",
            eventType: EventTypeEnum.NpcYell,
            eventCondition: ["Give me your tired"])]

        public void Phase1_Placeholder(Event @event, ScriptAccessory accessory) { }

        [ScriptMethod(name: "P1_Eightfold_ThunderFire_Guide_Cone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4014[48])|40329|40330)$"])]
        public void P1_Eightfold_ThunderFire_Guide_Cone(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            foreach (var pm in accessory.Data.PartyList)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_Eightfold_ThunderFire_Guide_Cone";
                dp.Scale = new(60);
                dp.Radian = float.Pi / 8;
                dp.Owner = sid;
                dp.TargetObject = pm;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
        }

        [ScriptMethod(name: "P1_Eightfold_ThunderFire_Subsequent_Cone", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(40145)$", "TargetIndex:1"])]
        public void P1_Eightfold_ThunderFire_Subsequent_Cone(Event @event, ScriptAccessory accessory)
        {
            var dur = 2000;
            if (parse != 1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!float.TryParse(@event["SourceRotation"], out var rot)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_Eightfold_ThunderFire_Subsequent_Cone1";
            dp.Scale = new(60);
            dp.FixRotation = true;
            dp.Rotation = rot;
            dp.Radian = float.Pi / 8;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_Eightfold_ThunderFire_Subsequent_Cone2";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 8;
            dp.FixRotation = true;
            dp.Rotation = rot + float.Pi / -8;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 2000;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_Eightfold_ThunderFire_Subsequent_Cone3";
            dp.Scale = new(60);
            dp.FixRotation = true;
            dp.Rotation = rot + float.Pi / -4;
            dp.Radian = float.Pi / 8;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 4000;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "P1_Eightfold_ThunderFire_Spread_Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4014[48])|40329|40330)$"])]
        public void P1_Eightfold_ThunderFire_Spread_Stack(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            string prompt = "";

            if (@event["ActionId"] == "40148" || @event["ActionId"] == "40330")
            {
                foreach (var pm in accessory.Data.PartyList)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_Eightfold_ThunderFire_Spread";
                    dp.Scale = new(6);
                    dp.Owner = pm;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 5000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    prompt = "Spread";
                }
            }
            else
            {
                int[] group = [6, 7, 4, 5, 2, 3, 0, 1];
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                for (int i = 4; i <= 7; ++i)
                {
                    var ismygroup = myindex == i || group[i] == myindex;
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_Eightfold_ThunderFire_Stack";
                    dp.Scale = new(6);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = ismygroup ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.Delay = 5000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    prompt = "Stack";
                }
            }

            System.Threading.Thread.Sleep(5000);
            if (!prompt.Equals(""))
            {
                if (Enable_Text_Prompts)
                {
                    accessory.Method.TextInfo(prompt, 1500);
                }
                accessory.TTS($"{prompt}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        [ScriptMethod(name: "P1_Eightfold_ThunderFire_Guide_Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4014[48])|40329|40330)$"])]
        public void P1_Eightfold_ThunderFire_Guide_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var spread = @event["ActionId"] == "40148" || @event["ActionId"] == "40330";
            var rot8 = myindex switch
            {
                0 => 0,
                1 => 2,
                2 => 6,
                3 => 4,
                4 => 5,
                5 => 3,
                6 => 7,
                7 => 1,
                _ => 0,
            };
            var outPoint = spread && (myindex == 2 || myindex == 3 || myindex == 6 || myindex == 7);
            var mPosEnd = RotatePoint(outPoint ? new(100, 0, 90) : new(100, 0, 95), new(100, 0, 100), float.Pi / 4 * rot8);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_Eightfold_ThunderFire_Guide_Position";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = mPosEnd;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "P1_Tank_Buster_Buff_Explosion", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4166"])]
        public void P1_Tank_Buster_Buff_Explosion(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            string prompt = "";

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_Tank_Buster_Buff_Explosion1";
            dp.Scale = new(10);
            dp.Owner = tid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = dur - 5000;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_Tank_Buster_Buff_Explosion2";
            dp.Scale = new(10);
            dp.Owner = tid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = dur - 5000;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            if (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 0 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 1)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    prompt = "Get slightly closer to another tank";
                }
            }
            if (2 <= accessory.Data.PartyList.IndexOf(accessory.Data.Me) && accessory.Data.PartyList.IndexOf(accessory.Data.Me) <= 7)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    prompt = "Stay away from both tanks";
                }
            }

            System.Threading.Thread.Sleep(dur - 5000);
            if (!prompt.Equals(""))
            {
                if (Enable_Text_Prompts)
                {
                    accessory.Method.TextInfo(prompt, 1500);
                }
                accessory.TTS($"{prompt}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        [ScriptMethod(name: "P1_Utopian_Sky_Position_Recording", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40158)$"], userControl: false)]
        public void P1_Utopian_Sky_Position_Recording(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            KodakkuAssist.Data.IGameObject? obj = null;
            do
            {
                ++sid;
                obj = accessory.Data.Objects.SearchByEntityId((uint)sid);
            } while (obj == null);
            var dir8 = PositionTo8Dir(obj.Position, new(100, 0, 100));
            p1UtopianSkyRecord[dir8 % 4] = 1;
        }

        [ScriptMethod(name: "P1_Utopian_Sky_ThunderFire_Recording", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4015[45])$"], userControl: false)]
        public void P1_Utopian_Sky_ThunderFire_Recording(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            p1UtopianSkyIsThunder = (@event["ActionId"] == "40155");
        }

        [ScriptMethod(name: "P1_Utopian_Sky_Range", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40158)$"])]
        public void P1_Utopian_Sky_Range(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            KodakkuAssist.Data.IGameObject? obj = null;
            do
            {
                ++sid;
                obj = accessory.Data.Objects.SearchByEntityId((uint)sid);
            } while (obj == null);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_Utopian_Sky_Range";
            dp.Scale = new(16, 50);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "P1_Utopian_Sky_Spread_Stack", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4015[45])$"])]
        public void P1_Utopian_Sky_Spread_Stack(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            string prompt = "";

            if (@event["ActionId"] == "40155")
            {
                foreach (var pm in accessory.Data.PartyList)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_Utopian_Sky_Spread";
                    dp.Scale = new(5);
                    dp.Owner = pm;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 10000;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    prompt = "Spread";
                }
            }
            else
            {
                List<int> h1group = [0, 2, 4, 6];
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var isH1group = h1group.Contains(myindex);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_Utopian_Sky_Stack1";
                dp.Scale = new(6);
                dp.Owner = accessory.Data.PartyList[2];
                dp.Color = isH1group ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                dp.Delay = 10000;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_Utopian_Sky_Stack2";
                dp.Scale = new(6);
                dp.Owner = accessory.Data.PartyList[3];
                dp.Color = !isH1group ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                dp.Delay = 10000;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    prompt = "Stack";
                }
            }

            System.Threading.Thread.Sleep(10000);
            if (!prompt.Equals(""))
            {
                if (Enable_Text_Prompts)
                {
                    accessory.Method.TextInfo(prompt, 1500);
                }
                accessory.TTS($"{prompt}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        [ScriptMethod(name: "Phase1 Standby Position Of Utopian Sky",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(4015[45])$"])]
        public void Phase1_Standby_Position_Of_Utopian_Sky(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            if (Phase1_Standby_Position_Of_Utopian_Sky == Phase1_Standby_Positions_Of_Utopian_Sky.Swap_OT_And_M2)
            {
                int rotationMultiplier = myIndex switch
                {
                    0 => 0,
                    1 => 1,
                    2 => 6,
                    3 => 4,
                    4 => 5,
                    5 => 3,
                    6 => 7,
                    7 => 2
                };
                var myPosition = RotatePoint(new(100, 0, 81), new(100, 0, 100), float.Pi / 4 * rotationMultiplier);
                if (myIndex == 0) myPosition = RotatePoint(myPosition, new(100, 0, 100), float.Pi / 72);
                if (myIndex == 1) myPosition = RotatePoint(myPosition, new(100, 0, 100), -(float.Pi / 72));
                if (myIndex == 6) myPosition = RotatePoint(myPosition, new(100, 0, 100), -(float.Pi / 36));
                if (myIndex == 7) myPosition = RotatePoint(myPosition, new(100, 0, 100), float.Pi / 36);

                var currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase1_Standby_Position_Of_Utopian_Sky";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = myPosition;
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
            }

            if (Phase1_Standby_Position_Of_Utopian_Sky == Phase1_Standby_Positions_Of_Utopian_Sky.Both_Tanks_Go_Center)
            {
                var myPosition = new Vector3(100, 0, 100);
                if (myIndex == 0) myPosition = new Vector3(100f, 0f, 94.5f);
                if (myIndex == 1) myPosition = new Vector3(100f, 0f, 105.5f);
                if (2 <= myIndex && myIndex <= 7)
                {
                    int rotationMultiplier = myIndex switch
                    {
                        2 => 6,
                        3 => 2,
                        4 => 5,
                        5 => 3,
                        6 => 7,
                        7 => 1
                    };
                    myPosition = RotatePoint(new(100, 0, 81), new(100, 0, 100), float.Pi / 4 * rotationMultiplier);
                }
                if (myPosition.Equals(new Vector3(100, 0, 100))) return;

                var currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase1_Standby_Position_Of_Utopian_Sky";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = myPosition;
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
            }
        }

        [ScriptMethod(name: "P1_Utopian_Sky_Process_Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40158)$"])]
        public void P1_Utopian_Sky_Process_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            lock (p1UtopianSkyCounterLock)
            {
                p1UtopianSkyCounter++;
                if (p1UtopianSkyCounter != 3) return;
                Task.Delay(334).ContinueWith(t =>
                {
                    if (!p1UtopianSkyIsThunder)
                    {
                        var safeDir = p1UtopianSkyRecord.IndexOf(0);
                        List<int> h1group = [0, 2, 4, 6];
                        var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                        var isH1group = h1group.Contains(myindex);
                        var rot8 = safeDir switch
                        {
                            0 => isH1group ? 0 : 4,
                            1 => isH1group ? 5 : 1,
                            2 => isH1group ? 6 : 2,
                            3 => isH1group ? 7 : 3,
                            _ => 0
                        };
                        var mPosEnd = RotatePoint(new(100, 0, 84), new(100, 0, 100), float.Pi / 4 * rot8);
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Utopian_Sky_Stack_Process_Position";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = mPosEnd;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 9000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                    else
                    {
                        var safeDir = p1UtopianSkyRecord.IndexOf(0);
                        List<int> h1group = [0, 2, 4, 6];
                        var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                        var isH1group = h1group.Contains(myindex);
                        Vector3 p1 = new(100.0f, 0, 88.0f);
                        Vector3 p2 = new(100.0f, 0, 80.5f);
                        Vector3 p3 = new(106.5f, 0, 81.5f);
                        Vector3 p4 = new(093.5f, 0, 81.5f);
                        var rot8 = safeDir switch
                        {
                            0 => isH1group ? 0 : 4,
                            1 => isH1group ? 5 : 1,
                            2 => isH1group ? 6 : 2,
                            3 => isH1group ? 7 : 3,
                            _ => 0
                        };
                        var myPosA = myindex switch
                        {
                            0 => p2,
                            1 => p2,
                            2 => p1,
                            3 => p1,
                            4 => p3,
                            5 => p3,
                            6 => p4,
                            7 => p4,
                            _ => p1,
                        };
                        var mPosEnd = RotatePoint(myPosA, new(100, 0, 100), float.Pi / 4 * rot8);
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Utopian_Sky_Spread_Process_Position";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = mPosEnd;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 9000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                });
            }
        }

        [ScriptMethod(name: "Phase1 Mark Players In Safe Positions",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40158"],
            userControl: false)]
        public void Phase1_Mark_Players_In_Safe_Positions(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!Phase1_Mark_Players_In_Safe_Positions) return;

            lock (p1UtopianSkyCounter2Lock)
            {
                ++p1UtopianSkyCounter2;
                System.Threading.Thread.MemoryBarrier();
                if (p1UtopianSkyCounter2 != 3) return;

                Task.Delay(334).ContinueWith(t =>
                {
                    int safePositions = p1UtopianSkyRecord.IndexOf(0);
                    List<int> temporaryOrder = [0, 1, 2, 3, 4, 5, 6, 7];
                    string debugOutput = "";

                    if (Phase1_Standby_Position_Of_Utopian_Sky == Phase1_Standby_Positions_Of_Utopian_Sky.Swap_OT_And_M2)
                    {
                        temporaryOrder = [0, 1, 7, 5, 3, 4, 2, 6];
                    }
                    if (Phase1_Standby_Position_Of_Utopian_Sky == Phase1_Standby_Positions_Of_Utopian_Sky.Both_Tanks_Go_Center)
                    {
                        temporaryOrder = [0, 7, 3, 5, 1, 4, 2, 6];
                    }

                    for (int i = 0, j = 0; i < temporaryOrder.Count; ++i)
                    {
                        var currentObject = accessory.Data.Objects.SearchById(accessory.Data.PartyList[temporaryOrder[i]]);
                        if (currentObject != null)
                        {
                            if (PositionTo8Dir(currentObject.Position, new Vector3(100, 0, 100)) == safePositions ||
                                PositionTo8Dir(currentObject.Position, new Vector3(100, 0, 100)) == ((safePositions + 4) % 8))
                            {
                                accessory.Method.Mark(accessory.Data.PartyList[temporaryOrder[i]], phase1SafePositionMarks[j]);
                                debugOutput += $"temporaryOrder[i]={temporaryOrder[i]},phase1SafePositionMarks[j]={phase1SafePositionMarks[j]}\n";
                                ++j;
                            }
                        }
                    }

                    if (Enable_Developer_Mode)
                    {
                        accessory.Method.SendChat($"/e {debugOutput}");
                        accessory.Log.Debug($"{debugOutput}");
                    }
                });
            }
        }

        [ScriptMethod(name: "Phase1 Clear Marks On Players In Safe Positions",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40158"],
            userControl: false,
            suppress: 2000)]
        public void Phase1_Clear_Marks_On_Players_In_Safe_Positions(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (Phase1_Mark_Players_In_Safe_Positions)
            {
                accessory.Method.MarkClear();
            }
        }

        [ScriptMethod(name: "Phase1 Thunder Burnt Strike",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(40164)$"])]
        public void Phase1_Thunder_Burnt_Strike(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase1_Second_Strike_Of_Thunder_Burnt_Strike";
            currentProperty.Scale = new(20, 40);
            currentProperty.Owner = sourceId;
            currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(1f);
            currentProperty.Delay = 4000;
            currentProperty.DestoryAt = 5750;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase1_First_Strike_Of_Thunder_Burnt_Strike";
            currentProperty.Scale = new(10, 40);
            currentProperty.Owner = sourceId;
            currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(3f);
            currentProperty.Delay = 4000;
            currentProperty.DestoryAt = 3750;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);
        }

        [ScriptMethod(name: "Phase1 Fire Burnt Strike",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(40161)$"])]
        public void Phase1_Fire_Burnt_Strike(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase1_First_Strike_Of_Fire_Burnt_Strike";
            currentProperty.Scale = new(10, 40);
            currentProperty.Owner = sourceId;
            currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(3f);
            currentProperty.Delay = 4000;
            currentProperty.DestoryAt = 3750;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase1_Central_Axis_Of_Fire_Burnt_Strike";
            currentProperty.Scale = new(0.5f, 40f);
            currentProperty.Owner = sourceId;
            currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(25f);
            currentProperty.Delay = 4000;
            currentProperty.DestoryAt = 5750;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            for (int i = 6; i <= 34; i += 7)
            {
                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase1_Knockback_Direction_Of_Fire_Burnt_Strike";
                currentProperty.Scale = new(1f, 1.618f);
                currentProperty.Owner = sourceId;
                currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(1f);
                currentProperty.Offset = new Vector3(-5.382f, 0, -i);
                currentProperty.Rotation = float.Pi / 2;
                currentProperty.Delay = 4000;
                currentProperty.DestoryAt = 5750;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase1_Knockback_Direction_Of_Fire_Burnt_Strike";
                currentProperty.Scale = new(1f, 1.618f);
                currentProperty.Owner = sourceId;
                currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(1f);
                currentProperty.Offset = new Vector3(5.382f, 0, -i);
                currentProperty.Rotation = -(float.Pi / 2);
                currentProperty.Delay = 4000;
                currentProperty.DestoryAt = 5750;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, currentProperty);
            }
        }

        [ScriptMethod(name: "P1_Utopian_Sky_Turn_Of_The_Heavens_Range", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4015[23])$"])]
        public void P1_Utopian_Sky_Turn_Of_The_Heavens_Range(Event evt, ScriptAccessory sa)
        {
            if (!ParseObjectId(@evt["SourceId"], out var sid)) return;
            var delay = 4000;
            var dp = sa.Data.GetDefaultDrawProperties();
            dp.Name = "P1_Utopian_Sky_Turn_Of_The_Heavens_Range";
            dp.Owner = sid;
            dp.Scale = new(evt["ActionId"] == "40152" ? 5 : 10);
            dp.Color = sa.Data.DefaultDangerColor;
            dp.Delay = delay;
            dp.DestoryAt = 8000 - delay;
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "P1_Turn_Of_The_Heavens_Tether_Recording", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4165"], userControl: false)]
        public void P1_Turn_Of_The_Heavens_Tether_Recording(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            lock (this)
            {
                p1TurnOfHeavensTethers[accessory.Data.PartyList.IndexOf(((uint)tid))] = 1;
            }
        }

        [ScriptMethod(name: "Phase1 Stack Range Of Turn Of The Heavens",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(40152)$"])]
        public void Phase1_Stack_Range_Of_Turn_Of_The_Heavens(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            var currentPosition = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if (MathF.Abs(currentPosition.Z - 100) > 1) return;

            bool hasSelectedAStrat = false;
            int highPriorityStack = p1TurnOfHeavensTethers.IndexOf(1);
            int lowPriorityStack = p1TurnOfHeavensTethers.LastIndexOf(1);
            List<int> membersOfTheNorthGroup = [];

            if (Phase1_Group_Of_Turn_Of_The_Heavens == Phase1_Groups_Of_Turn_Of_The_Heavens.MTOTH1H2_Go_North_MTM1_vary)
            {
                hasSelectedAStrat = true;
                membersOfTheNorthGroup.Add(highPriorityStack);
                if (1 != highPriorityStack && 1 != lowPriorityStack) membersOfTheNorthGroup.Add(1);
                if (2 != highPriorityStack && 2 != lowPriorityStack) membersOfTheNorthGroup.Add(2);
                if (3 != highPriorityStack && 3 != lowPriorityStack) membersOfTheNorthGroup.Add(3);
                if (membersOfTheNorthGroup.Count < 4 && 0 != highPriorityStack && 0 != lowPriorityStack) membersOfTheNorthGroup.Add(0);
                if (membersOfTheNorthGroup.Count < 4 && 4 != highPriorityStack && 4 != lowPriorityStack) membersOfTheNorthGroup.Add(4);
            }

            if (Phase1_Group_Of_Turn_Of_The_Heavens == Phase1_Groups_Of_Turn_Of_The_Heavens.MTH1M1R1_Go_North_MTOT_vary)
            {
                hasSelectedAStrat = true;
                membersOfTheNorthGroup.Add(highPriorityStack);
                if (2 != highPriorityStack && 2 != lowPriorityStack) membersOfTheNorthGroup.Add(2);
                if (4 != highPriorityStack && 4 != lowPriorityStack) membersOfTheNorthGroup.Add(4);
                if (6 != highPriorityStack && 6 != lowPriorityStack) membersOfTheNorthGroup.Add(6);
                if (membersOfTheNorthGroup.Count < 4 && 0 != highPriorityStack && 0 != lowPriorityStack) membersOfTheNorthGroup.Add(0);
                if (membersOfTheNorthGroup.Count < 4 && 1 != highPriorityStack && 1 != lowPriorityStack) membersOfTheNorthGroup.Add(1);
            }

            if (Phase1_Group_Of_Turn_Of_The_Heavens == Phase1_Groups_Of_Turn_Of_The_Heavens.MTOTR1R2_Go_North_MTM1_vary)
            {
                hasSelectedAStrat = true;
                membersOfTheNorthGroup.Add(highPriorityStack);
                if (1 != highPriorityStack && 1 != lowPriorityStack) membersOfTheNorthGroup.Add(1);
                if (6 != highPriorityStack && 6 != lowPriorityStack) membersOfTheNorthGroup.Add(6);
                if (7 != highPriorityStack && 7 != lowPriorityStack) membersOfTheNorthGroup.Add(7);
                if (membersOfTheNorthGroup.Count < 4 && 0 != highPriorityStack && 0 != lowPriorityStack) membersOfTheNorthGroup.Add(0);
                if (membersOfTheNorthGroup.Count < 4 && 4 != highPriorityStack && 4 != lowPriorityStack) membersOfTheNorthGroup.Add(4);
            }

            if (!hasSelectedAStrat || membersOfTheNorthGroup.Count != 4)
            {
                var currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase1_Stack_Range_Of_Turn_Of_The_Heavens";
                currentProperty.Scale = new(6);
                currentProperty.Owner = accessory.Data.PartyList[highPriorityStack];
                currentProperty.Color = accessory.Data.DefaultDangerColor;
                currentProperty.Delay = 6000;
                currentProperty.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase1_Stack_Range_Of_Turn_Of_The_Heavens";
                currentProperty.Scale = new(6);
                currentProperty.Owner = accessory.Data.PartyList[lowPriorityStack];
                currentProperty.Color = accessory.Data.DefaultDangerColor;
                currentProperty.Delay = 6000;
                currentProperty.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);
            }
            else
            {
                bool inTheNorthGroup = membersOfTheNorthGroup.Contains(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                var currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase1_Stack_Range_Of_Turn_Of_The_Heavens";
                currentProperty.Scale = new(6);
                currentProperty.Owner = accessory.Data.PartyList[highPriorityStack];
                currentProperty.Delay = 6000;
                currentProperty.DestoryAt = 5000;
                currentProperty.Color = inTheNorthGroup ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase1_Stack_Range_Of_Turn_Of_The_Heavens";
                currentProperty.Scale = new(6);
                currentProperty.Owner = accessory.Data.PartyList[lowPriorityStack];
                currentProperty.Delay = 6000;
                currentProperty.DestoryAt = 5000;
                currentProperty.Color = inTheNorthGroup ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);
            }
        }

        [ScriptMethod(name: "P1_Turn_Of_The_Heavens_Knockback_Process_Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40152)$"])]
        public void P1_Turn_Of_The_Heavens_Knockback_Process_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if (MathF.Abs(pos.Z - 100) > 1) return;

            var atEast = pos.X - 100 > 1;
            var o1 = p1TurnOfHeavensTethers.IndexOf(1);
            var o2 = p1TurnOfHeavensTethers.LastIndexOf(1);
            List<int> upGroup = [];
            if (Phase1_Group_Of_Turn_Of_The_Heavens == Phase1_Groups_Of_Turn_Of_The_Heavens.MTOTH1H2_Go_North_MTM1_vary)
            {
                upGroup.Add(o1);
                if (o1 != 1 && o2 != 1) upGroup.Add(1);
                if (o1 != 2 && o2 != 2) upGroup.Add(2);
                if (o1 != 3 && o2 != 3) upGroup.Add(3);
                if (upGroup.Count < 4 && o1 != 0 && o2 != 0) upGroup.Add(0);
                if (upGroup.Count < 4 && o1 != 4 && o2 != 4) upGroup.Add(4);
            }
            if (Phase1_Group_Of_Turn_Of_The_Heavens == Phase1_Groups_Of_Turn_Of_The_Heavens.MTH1M1R1_Go_North_MTOT_vary)
            {
                upGroup.Add(o1);
                if (o1 != 2 && o2 != 2) upGroup.Add(2);
                if (o1 != 4 && o2 != 4) upGroup.Add(4);
                if (o1 != 6 && o2 != 6) upGroup.Add(6);
                if (upGroup.Count < 4 && o1 != 0 && o2 != 0) upGroup.Add(0);
                if (upGroup.Count < 4 && o1 != 1 && o2 != 1) upGroup.Add(1);
            }
            if (Phase1_Group_Of_Turn_Of_The_Heavens == Phase1_Groups_Of_Turn_Of_The_Heavens.MTOTR1R2_Go_North_MTM1_vary)
            {
                upGroup.Add(o1);
                if (o1 != 1 && o2 != 1) upGroup.Add(1);
                if (o1 != 6 && o2 != 6) upGroup.Add(6);
                if (o1 != 7 && o2 != 7) upGroup.Add(7);
                if (upGroup.Count < 4 && o1 != 0 && o2 != 0) upGroup.Add(0);
                if (upGroup.Count < 4 && o1 != 4 && o2 != 4) upGroup.Add(4);
            }

            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var dealpos1 = new Vector3(atEast ? 105.5f : 94.5f, 0, upGroup.Contains(myindex) ? 93 : 107);
            var dealpos2 = new Vector3(atEast ? 102 : 98, 0, upGroup.Contains(myindex) ? 93 : 107);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_Turn_Of_The_Heavens_Knockback_Process_Position1";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos1;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_Turn_Of_The_Heavens_Knockback_Process_Position2";
            dp.Scale = new(2);
            dp.Position = dealpos1;
            dp.TargetPosition = dealpos2;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_Turn_Of_The_Heavens_Knockback_Process_Position3";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos2;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 4000;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "Phase1 Fall Of Faith Control",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(40170)$"],
            userControl: false)]
        public void Phase1_Fall_Of_Faith_Control(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            System.Threading.Thread.MemoryBarrier();
            ++phase1BurnishedGloryCastCount;
            System.Threading.Thread.MemoryBarrier();

            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e phase1BurnishedGloryCastCount={phase1BurnishedGloryCastCount}");
            }

            switch (phase1BurnishedGloryCastCount)
            {
                case 1:
                    phase1TetheredPlayersDuringFallOfFaith.Clear();
                    if (Phase1_Mark_Players_During_Fall_Of_Faith) accessory.Method.MarkClear();
                    phase1MarkTetheredSemaphore = 0;
                    phase1ShortPromptSemaphore = 0;
                    phase1DrawingSemaphore = 0;
                    phase1MarkUntetheredSemaphore = 0;
                    phase1FinalPromptSemaphore = 0;
                    System.Threading.Thread.MemoryBarrier();
                    phase1IsInFallOfFaith = true;
                    break;
                case 2:
                    phase1IsInFallOfFaith = false;
                    System.Threading.Thread.MemoryBarrier();
                    phase1TetheredPlayersDuringFallOfFaith.Clear();
                    if (Phase1_Mark_Players_During_Fall_Of_Faith) accessory.Method.MarkClear();
                    phase1MarkTetheredSemaphore = 0;
                    phase1ShortPromptSemaphore = 0;
                    phase1DrawingSemaphore = 0;
                    phase1MarkUntetheredSemaphore = 0;
                    phase1FinalPromptSemaphore = 0;
                    break;
                default:
                    phase1TetheredPlayersDuringFallOfFaith.Clear();
                    if (Phase1_Mark_Players_During_Fall_Of_Faith) accessory.Method.MarkClear();
                    phase1MarkTetheredSemaphore = 0;
                    phase1ShortPromptSemaphore = 0;
                    phase1DrawingSemaphore = 0;
                    phase1MarkUntetheredSemaphore = 0;
                    phase1FinalPromptSemaphore = 0;
                    break;
            }
        }

        [ScriptMethod(name: "Phase1 Record Tethered Players",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:regex:^(00F9|011F)$"],
            userControl: false)]
        public void Phase1_Record_Tethered_Players(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!phase1IsInFallOfFaith) return;
            if (!ParseObjectId(@event["TargetId"], out var targetId)) return;

            int targetIndex = accessory.Data.PartyList.IndexOf(((uint)targetId));
            var tetherType = (@event["Id"].Equals("00F9")) ? (10) : (20);
            System.Threading.Thread.MemoryBarrier();
            phase1TetheredPlayersDuringFallOfFaith.Add(tetherType + targetIndex);
            System.Threading.Thread.MemoryBarrier();
            phase1MarkTetheredSemaphore = 1;
            phase1ShortPromptSemaphore = 1;
            phase1DrawingSemaphore = 1;
            phase1MarkUntetheredSemaphore = 1;
            phase1FinalPromptSemaphore = 1;
        }

        [ScriptMethod(name: "Phase1 Mark Tethered Players",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:regex:^(00F9|011F)$"],
            userControl: false)]
        public void Phase1_Mark_Tethered_Players(Event @event, ScriptAccessory accessory)
        {
            if (!Phase1_Mark_Players_During_Fall_Of_Faith) return;
            if (parse != 1) return;
            if (!phase1IsInFallOfFaith) return;

            while (System.Threading.Interlocked.CompareExchange(ref phase1MarkTetheredSemaphore, 0, 1) == 0)
            {
                System.Threading.Thread.Sleep(1);
            }
            System.Threading.Thread.MemoryBarrier();

            int copyOfTheCount = phase1TetheredPlayersDuringFallOfFaith.Count;
            int targetIndex = (phase1TetheredPlayersDuringFallOfFaith.Last() % 10);
            MarkType targetMark = phase1TetheredPlayerMarks[copyOfTheCount - 1];
            accessory.Method.Mark(accessory.Data.PartyList[targetIndex], targetMark);

            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e copyOfTheCount-1={copyOfTheCount - 1} targetIndex={targetIndex} targetMark={targetMark}");
            }
        }

        [ScriptMethod(name: "Phase1 Prompt The Type Of The Current Tether",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:regex:^(00F9|011F)$"])]
        public void Phase1_Prompt_The_Type_Of_The_Current_Tether(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!phase1IsInFallOfFaith) return;

            while (System.Threading.Interlocked.CompareExchange(ref phase1ShortPromptSemaphore, 0, 1) == 0)
            {
                System.Threading.Thread.Sleep(1);
            }
            System.Threading.Thread.MemoryBarrier();

            if (1 <= phase1TetheredPlayersDuringFallOfFaith.Count && phase1TetheredPlayersDuringFallOfFaith.Count <= 3)
            {
                bool isFireTether = (phase1TetheredPlayersDuringFallOfFaith.Last() < 20);
                string prompt = "";
                if (isFireTether)
                {
                    if (Language_Of_Prompts == Languages_Of_Prompts.English) prompt = "Fire";
                }
                else
                {
                    if (Language_Of_Prompts == Languages_Of_Prompts.English) prompt = "Thunder";
                }
                if (!prompt.Equals(""))
                {
                    if (Enable_Text_Prompts) accessory.Method.TextInfo(prompt, 1000);
                    if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS) accessory.TTS(prompt, Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
                }
            }
        }

        [ScriptMethod(name: "Phase1 Range Of The Current Tether",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:regex:^(00F9|011F)$"])]
        public void Phase1_Range_Of_The_Current_Tether(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!phase1IsInFallOfFaith) return;

            while (System.Threading.Interlocked.CompareExchange(ref phase1DrawingSemaphore, 0, 1) == 0)
            {
                System.Threading.Thread.Sleep(1);
            }
            System.Threading.Thread.MemoryBarrier();

            bool isFireTether = (phase1TetheredPlayersDuringFallOfFaith.Last() < 20);
            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            if (isFireTether)
            {
                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase1_Range_Of_The_Fire_Tether";
                currentProperty.Scale = new(60);
                currentProperty.Radian = float.Pi / 2;
                currentProperty.Owner = accessory.Data.PartyList[(phase1TetheredPlayersDuringFallOfFaith.Last() % 10)];
                currentProperty.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                currentProperty.TargetOrderIndex = 1;
                currentProperty.Color = accessory.Data.DefaultDangerColor;
                currentProperty.Delay = 9500;
                currentProperty.DestoryAt = 3800;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);
            }
            else
            {
                for (uint i = 1; i <= 3; ++i)
                {
                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase1_Range_Of_The_Thunder_Tether";
                    currentProperty.Scale = new(60);
                    currentProperty.Radian = float.Pi / 3 * 2;
                    currentProperty.Owner = accessory.Data.PartyList[(phase1TetheredPlayersDuringFallOfFaith.Last() % 10)];
                    currentProperty.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    currentProperty.TargetOrderIndex = i;
                    currentProperty.Color = accessory.Data.DefaultDangerColor;
                    currentProperty.Delay = 9500;
                    currentProperty.DestoryAt = 3800;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);
                }
            }
        }

        [ScriptMethod(name: "Phase1 Mark Untethered Players",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:regex:^(00F9|011F)$"],
            userControl: false)]
        public void Phase1_Mark_Untethered_Players(Event @event, ScriptAccessory accessory)
        {
            if (!Phase1_Mark_Players_During_Fall_Of_Faith) return;
            if (parse != 1) return;
            if (!phase1IsInFallOfFaith) return;

            while (System.Threading.Interlocked.CompareExchange(ref phase1MarkUntetheredSemaphore, 0, 1) == 0)
            {
                System.Threading.Thread.Sleep(1);
            }
            System.Threading.Thread.MemoryBarrier();

            if (phase1TetheredPlayersDuringFallOfFaith.Count != 4) return;

            var tetheredPlayers = phase1TetheredPlayersDuringFallOfFaith.Select(o => o % 10).ToList();
            List<int> untetheredPlayers = [];

            if (Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Single_Line_In_THD_Order ||
                Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Double_Lines_MOTH12_Left_M12R12_Right)
            {
                for (int i = 0; i < accessory.Data.PartyList.Count; ++i)
                {
                    if (!tetheredPlayers.Contains(i)) untetheredPlayers.Add(i);
                }
            }

            if (Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Single_Line_In_HTD_Order ||
                Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Double_Lines_H12MOT_Left_M12R12_Right)
            {
                List<int> temporaryPriority = new List<int> { 2, 3, 0, 1, 4, 5, 6, 7 };
                for (int i = 0; i < temporaryPriority.Count; ++i)
                {
                    if (!tetheredPlayers.Contains(temporaryPriority[i])) untetheredPlayers.Add(temporaryPriority[i]);
                }
            }

            if (Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Single_Line_In_H1TDH2_Order)
            {
                List<int> temporaryPriority = new List<int> { 2, 0, 1, 4, 5, 6, 7, 3 };
                for (int i = 0; i < temporaryPriority.Count; ++i)
                {
                    if (!tetheredPlayers.Contains(temporaryPriority[i])) untetheredPlayers.Add(temporaryPriority[i]);
                }
            }

            if (untetheredPlayers.Count != 4) return;

            string debugOutput = "";
            for (int i = 0; i < untetheredPlayers.Count; ++i)
            {
                accessory.Method.Mark(accessory.Data.PartyList[(untetheredPlayers[i])], phase1UntetheredPlayerMarks[i]);
                if (Enable_Developer_Mode)
                {
                    debugOutput += $"(untetheredPlayers[{i}])={(untetheredPlayers[i])}\n";
                    debugOutput += $"phase1UntetheredPlayerMarks[i]={phase1UntetheredPlayerMarks[i]}\n";
                }
            }
            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e {debugOutput}");
                accessory.Log.Debug($"{debugOutput}");
            }
        }

        [ScriptMethod(name: "Phase1 Prompt All The Types Of Tethers",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:regex:^(00F9|011F)$"])]
        public void Phase1_Prompt_All_The_Types_Of_Tethers(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!phase1IsInFallOfFaith) return;

            while (System.Threading.Interlocked.CompareExchange(ref phase1FinalPromptSemaphore, 0, 1) == 0)
            {
                System.Threading.Thread.Sleep(1);
            }
            System.Threading.Thread.MemoryBarrier();

            if (phase1TetheredPlayersDuringFallOfFaith.Count != 4) return;

            var isFireTether = phase1TetheredPlayersDuringFallOfFaith.Select(o => o < 20).ToList();
            if (isFireTether.Count != 4) return;

            string prompt = "";
            if (Language_Of_Prompts == Languages_Of_Prompts.English)
            {
                prompt += (isFireTether[0]) ? "Fire" : "Thunder";
                for (int i = 1; i < isFireTether.Count; ++i)
                {
                    prompt += (isFireTether[i]) ? ", Fire" : ", Thunder";
                }
            }

            if (!prompt.Equals(""))
            {
                if (Enable_Text_Prompts) accessory.Method.TextInfo(prompt, 13300);
                if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS) accessory.TTS(prompt, Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        [ScriptMethod(name: "P1_Four_Tethers_Process_Position", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(00F9|011F)$"])]
        public void P1_Four_Tethers_Process_Position(Event @event, ScriptAccessory accessory)
        {
            if (!phase1IsInFallOfFaith) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var dis = 2.5f;
            var far = 5.25f;
            Task.Delay(334).ContinueWith(t =>
            {
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                Vector3 t1p1 = new(100, 0, 100 - far);
                Vector3 t1p2 = new(100, 0, 100 - far - dis);
                Vector3 t2p1 = new(100, 0, 100 + far);
                Vector3 t2p2 = new(100, 0, 100 + far + dis);
                Vector3 t3p1 = new(100, 0, 100 - far - dis);
                Vector3 t3p2 = new(100, 0, 100 - far);
                Vector3 t4p1 = new(100, 0, 100 + far + dis);
                Vector3 t4p2 = new(100, 0, 100 + far);

                if (phase1TetheredPlayersDuringFallOfFaith.Count == 1 && tid == accessory.Data.Me)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_Four_Tethers_Tether1_Process_Position1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t1p1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 13000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_Four_Tethers_Tether1_Process_Position2";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t1p2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 13000;
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    if (t1p1 != t1p2)
                    {
                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Four_Tethers_Tether1_Process_Position2_Preview";
                        dp.Scale = new(2);
                        dp.Position = t1p1;
                        dp.TargetPosition = t1p2;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.DestoryAt = 13000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                }
                if (phase1TetheredPlayersDuringFallOfFaith.Count == 2 && tid == accessory.Data.Me)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_Four_Tethers_Tether2_Process_Position1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t2p1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 13500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_Four_Tethers_Tether2_Process_Position2";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t2p2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 13500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    if (t2p1 != t2p2)
                    {
                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Four_Tethers_Tether2_Process_Position2_Preview";
                        dp.Scale = new(2);
                        dp.Position = t2p1;
                        dp.TargetPosition = t2p2;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.DestoryAt = 13500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                }
                if (phase1TetheredPlayersDuringFallOfFaith.Count == 3 && tid == accessory.Data.Me)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_Four_Tethers_Tether3_Process_Position1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t3p1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_Four_Tethers_Tether3_Process_Position2";
                    dp.Scale = new(3);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t3p2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 7500;
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    if (t3p1 != t3p2)
                    {
                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Four_Tethers_Tether3_Process_Position2_Preview";
                        dp.Scale = new(2);
                        dp.Position = t3p1;
                        dp.TargetPosition = t3p2;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.DestoryAt = 7500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                }
                if (phase1TetheredPlayersDuringFallOfFaith.Count == 4 && tid == accessory.Data.Me)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_Four_Tethers_Tether4_Process_Position1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t4p1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 8500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_Four_Tethers_Tether4_Process_Position2";
                    dp.Scale = new(3);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t4p2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 8500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    if (t4p1 != t4p2)
                    {
                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Four_Tethers_Tether4_Process_Position2_Preview";
                        dp.Scale = new(2);
                        dp.Position = t4p1;
                        dp.TargetPosition = t4p2;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.DestoryAt = 8500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                }
                if (phase1TetheredPlayersDuringFallOfFaith.Count == 4)
                {
                    var tehterObjIndex = phase1TetheredPlayersDuringFallOfFaith.Select(o => o % 10).ToList();
                    var tehterIsFire = phase1TetheredPlayersDuringFallOfFaith.Select(o => o < 20).ToList();
                    List<int> idleObjIndex = [];
                    if (Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Single_Line_In_THD_Order ||
                        Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Double_Lines_MOTH12_Left_M12R12_Right)
                    {
                        for (int i = 0; i < accessory.Data.PartyList.Count; i++)
                        {
                            if (!tehterObjIndex.Contains(i)) { idleObjIndex.Add(i); }
                        }
                    }

                    if (Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Single_Line_In_HTD_Order ||
                        Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Double_Lines_H12MOT_Left_M12R12_Right)
                    {
                        List<int> htdOrder = new List<int> { 2, 3, 0, 1, 4, 5, 6, 7 };
                        for (int i = 0; i < htdOrder.Count; ++i)
                        {
                            if (!tehterObjIndex.Contains(htdOrder[i])) { idleObjIndex.Add(htdOrder[i]); }
                        }
                    }

                    if (Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Single_Line_In_H1TDH2_Order)
                    {
                        List<int> h1tdh2Order = new List<int> { 2, 0, 1, 4, 5, 6, 7, 3 };
                        for (int i = 0; i < h1tdh2Order.Count; ++i)
                        {
                            if (!tehterObjIndex.Contains(h1tdh2Order[i])) { idleObjIndex.Add(h1tdh2Order[i]); }
                        }
                    }

                    if (!idleObjIndex.Contains(myindex)) return;

                    Vector3 i1p1 = new Vector3(100, 0, 100);
                    Vector3 i1p2 = new Vector3(100, 0, 100);
                    Vector3 i2p1 = new Vector3(100, 0, 100);
                    Vector3 i2p2 = new Vector3(100, 0, 100);
                    Vector3 i3p1 = new Vector3(100, 0, 100);
                    Vector3 i3p2 = new Vector3(100, 0, 100);
                    Vector3 i4p1 = new Vector3(100, 0, 100);
                    Vector3 i4p2 = new Vector3(100, 0, 100);
                    Vector3 dealpos1 = default;
                    Vector3 dealpos2 = default;

                    if (Phase1_Orientation_Benchmark_During_Fall_Of_Faith == Phase1_Orientation_Benchmarks_During_Fall_Of_Faith.High_Priority_Left_Facing_Due_North)
                    {
                        i1p1 = tehterIsFire[0] ? new(100, 0, 100 - far - dis) : new(100 - dis, 0, 100 - far);
                        i1p2 = tehterIsFire[2] ? new(100, 0, 100 - far - dis) : new(100 - dis, 0, 100 - far);
                        i2p1 = tehterIsFire[0] ? new(100, 0, 100 - far - dis) : new(100 + dis, 0, 100 - far);
                        i2p2 = tehterIsFire[2] ? new(100, 0, 100 - far - dis) : new(100 + dis, 0, 100 - far);
                        i3p1 = tehterIsFire[1] ? new(100, 0, 100 + far + dis) : new(100 - dis, 0, 100 + far);
                        i3p2 = tehterIsFire[3] ? new(100, 0, 100 + far + dis) : new(100 - dis, 0, 100 + far);
                        i4p1 = tehterIsFire[1] ? new(100, 0, 100 + far + dis) : new(100 + dis, 0, 100 + far);
                        i4p2 = tehterIsFire[3] ? new(100, 0, 100 + far + dis) : new(100 + dis, 0, 100 + far);
                    }

                    if (Phase1_Orientation_Benchmark_During_Fall_Of_Faith == Phase1_Orientation_Benchmarks_During_Fall_Of_Faith.High_Priority_Left_Facing_The_Boss)
                    {
                        i1p1 = tehterIsFire[0] ? new(100, 0, 100 - far - dis) : new(100 + dis, 0, 100 - far);
                        i1p2 = tehterIsFire[2] ? new(100, 0, 100 - far - dis) : new(100 + dis, 0, 100 - far);
                        i2p1 = tehterIsFire[0] ? new(100, 0, 100 - far - dis) : new(100 - dis, 0, 100 - far);
                        i2p2 = tehterIsFire[2] ? new(100, 0, 100 - far - dis) : new(100 - dis, 0, 100 - far);
                        i3p1 = tehterIsFire[1] ? new(100, 0, 100 + far + dis) : new(100 - dis, 0, 100 + far);
                        i3p2 = tehterIsFire[3] ? new(100, 0, 100 + far + dis) : new(100 - dis, 0, 100 + far);
                        i4p1 = tehterIsFire[1] ? new(100, 0, 100 + far + dis) : new(100 + dis, 0, 100 + far);
                        i4p2 = tehterIsFire[3] ? new(100, 0, 100 + far + dis) : new(100 + dis, 0, 100 + far);
                    }

                    if (i1p1.Equals(new Vector3(100, 0, 100))) return;

                    dealpos1 = idleObjIndex.IndexOf(myindex) switch
                    {
                        0 => i1p1,
                        1 => i2p1,
                        2 => i3p1,
                        3 => i4p1,
                    };
                    dealpos2 = idleObjIndex.IndexOf(myindex) switch
                    {
                        0 => i1p2,
                        1 => i2p2,
                        2 => i3p2,
                        3 => i4p2,
                    };
                    var upgroup = (idleObjIndex.IndexOf(myindex) == 0 || idleObjIndex.IndexOf(myindex) == 1);

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_Four_Tethers_Process_Position1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = upgroup ? 5000 : 8500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_Four_Tethers_Process_Position2";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = upgroup ? 5000 : 8500;
                    dp.DestoryAt = upgroup ? 6000 : 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    if (dealpos1 != dealpos2)
                    {
                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Four_Tethers_Process_Position2_Preview";
                        dp.Scale = new(2);
                        dp.Position = dealpos1;
                        dp.TargetPosition = dealpos2;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.DestoryAt = upgroup ? 5000 : 8500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                }
            });
        }

        [ScriptMethod(name: "P1_Towers_Tower_Recorder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4012[234567]|4013[15])$"], userControl: false)]
        public void P1_Towers_Tower_Recorder(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            lock (this)
            {
                var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                var count = @event["ActionId"] switch
                {
                    "40135" => 1,
                    "40131" => 1,
                    "40122" => 2,
                    "40123" => 3,
                    "40124" => 4,
                    "40125" => 2,
                    "40126" => 3,
                    "40127" => 4,
                };
                if (MathF.Abs(pos.Z - 100) < 1)
                {
                    p1Towers[1] = count;
                }
                else
                {
                    if (pos.Z - 100 > 1) p1Towers[2] = count;
                    else p1Towers[0] = count;
                }
                if (pos.X - 100 > 1)
                {
                    p1Towers[3] = 1;
                }
            }
        }

        [ScriptMethod(name: "Phase1 Burnt Strike With Towers And Tank Busters",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(40134|40129)$"])]
        public void Phase1_Burnt_Strike_With_Towers_And_Tank_Busters(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;

            if (@event["ActionId"].Equals("40134"))
            {
                var currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase1_Second_Strike_Of_Thunder_Burnt_Strike";
                currentProperty.Scale = new(20, 40);
                currentProperty.Owner = sourceId;
                currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(1f);
                currentProperty.DestoryAt = 8200;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase1_First_Strike_Of_Thunder_Burnt_Strike";
                currentProperty.Scale = new(10, 40);
                currentProperty.Owner = sourceId;
                currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(3f);
                currentProperty.DestoryAt = 6500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, currentProperty);
            }

            if (@event["ActionId"].Equals("40129"))
            {
                var currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase1_First_Strike_Of_Fire_Burnt_Strike";
                currentProperty.Scale = new(10, 40);
                currentProperty.Owner = sourceId;
                currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(3f);
                currentProperty.DestoryAt = 6500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase1_Central_Axis_Of_Fire_Burnt_Strike";
                currentProperty.Scale = new(0.5f, 40f);
                currentProperty.Owner = sourceId;
                currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(25f);
                currentProperty.DestoryAt = 8200;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, currentProperty);

                for (int i = -4; i <= 4; ++i)
                {
                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase1_Knockback_Direction_Of_Fire_Burnt_Strike";
                    currentProperty.Scale = new(1f, 1.618f);
                    currentProperty.Owner = sourceId;
                    currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(1f);
                    currentProperty.Offset = new Vector3(-5.382f, 0, (float)(-(i * 4.595d)));
                    currentProperty.Rotation = float.Pi / 2;
                    currentProperty.DestoryAt = 8200;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase1_Knockback_Direction_Of_Fire_Burnt_Strike";
                    currentProperty.Scale = new(1f, 1.618f);
                    currentProperty.Owner = sourceId;
                    currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(1f);
                    currentProperty.Offset = new Vector3(5.382f, 0, (float)(-(i * 4.595d)));
                    currentProperty.Rotation = -(float.Pi / 2);
                    currentProperty.DestoryAt = 8200;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, currentProperty);
                }
            }
        }

        [ScriptMethod(name: "P1_Towers_Tower_Process_Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40134|40129)$"])]
        public void P1_Towers_Tower_Process_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            Task.Delay(334).ContinueWith(t =>
            {
                var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                if (@event["ActionId"] == "40134")
                {
                    var eastTower = p1Towers[3] == 1;
                    if (myIndex == 0 || myIndex == 1)
                    {
                        var dx = eastTower ? -10.5f : 10.5f;
                        var dy = myIndex == 0 ? -5.5f : 5.5f;
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Thunder_Towers_Tower_Process_Position_Tank";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = new(100 + dx, 0, 100 + dy);
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 10500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                    else
                    {
                        int myTowerIndex = myIndex - 1;
                        Vector3 standbyPosition = new Vector3(100, 0, 100);
                        Vector3 towerPosition = new Vector3(100, 0, 100);

                        if (Phase1_Strat_Of_Towers == Phase1_Strats_Of_Towers.Completely_Based_On_Priority)
                        {
                            if (myTowerIndex > 0 && myTowerIndex <= p1Towers[0]) standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                            if (myTowerIndex > p1Towers[0] && myTowerIndex <= p1Towers[0] + p1Towers[1]) standbyPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                            if (myTowerIndex > p1Towers[0] + p1Towers[1] && myTowerIndex <= p1Towers[0] + p1Towers[1] + p1Towers[2]) standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);
                            if (myTowerIndex > 0 && myTowerIndex <= p1Towers[0]) towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                            if (myTowerIndex > p1Towers[0] && myTowerIndex <= p1Towers[0] + p1Towers[1]) towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                            if (myTowerIndex > p1Towers[0] + p1Towers[1] && myTowerIndex <= p1Towers[0] + p1Towers[1] + p1Towers[2]) towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);
                        }

                        if (Phase1_Strat_Of_Towers == Phase1_Strats_Of_Towers.Fixed_H1H2R2_Priority_For_Rest)
                        {
                            bool fixedPartyMember = false;
                            if (myIndex == 2) { fixedPartyMember = true; standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); }
                            if (myIndex == 3) { fixedPartyMember = true; standbyPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); }
                            if (myIndex == 7) { fixedPartyMember = true; standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); }

                            if (!fixedPartyMember)
                            {
                                int newTower0 = p1Towers[0] - 1;
                                int newTower1 = p1Towers[1] - 1;
                                int newTower2 = p1Towers[2] - 1;
                                int myNewTowerIndex = myIndex - 3;
                                if (Enable_Developer_Mode)
                                {
                                    accessory.Method.SendChat($"/e newTower0={newTower0} newTower1={newTower1} newTower2={newTower2} myNewTowerIndex={myNewTowerIndex}");
                                }
                                if (newTower0 > 0 && 0 < myNewTowerIndex && myNewTowerIndex <= newTower0)
                                { standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); }
                                if (newTower1 > 0 && newTower0 < myNewTowerIndex && myNewTowerIndex <= newTower0 + newTower1)
                                { standbyPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); }
                                if (newTower2 > 0 && newTower0 + newTower1 < myNewTowerIndex && myNewTowerIndex <= newTower0 + newTower1 + newTower2)
                                { standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); }
                            }
                        }

                        if (Phase1_Strat_Of_Towers == Phase1_Strats_Of_Towers.Fixed_H1H2R2_Rest_Fill_Vacancies)
                        {
                            bool fixedPartyMember = false;
                            if (myIndex == 2) { fixedPartyMember = true; standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); }
                            if (myIndex == 3) { fixedPartyMember = true; standbyPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); }
                            if (myIndex == 7) { fixedPartyMember = true; standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); }

                            if (!fixedPartyMember)
                            {
                                if (Enable_Developer_Mode)
                                {
                                    accessory.Method.SendChat($"/e p1Towers[0]={p1Towers[0]} p1Towers[1]={p1Towers[1]} p1Towers[2]={p1Towers[2]} myTowerIndex={myTowerIndex}");
                                }
                                if (myIndex == 4)
                                {
                                    if (p1Towers[0] >= 2)
                                    { standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); }
                                    else
                                    {
                                        if (p1Towers[1] >= 3) { standbyPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); }
                                        if (p1Towers[2] >= 3) { standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); }
                                    }
                                }
                                if (myIndex == 5)
                                {
                                    if (p1Towers[1] >= 2)
                                    { standbyPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); }
                                    else
                                    {
                                        if (p1Towers[0] >= 3) { standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); }
                                        if (p1Towers[2] >= 3) { standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); }
                                    }
                                }
                                if (myIndex == 6)
                                {
                                    if (p1Towers[2] >= 2)
                                    { standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); }
                                    else
                                    {
                                        if (p1Towers[0] >= 3) { standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); }
                                        if (p1Towers[1] >= 3) { standbyPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); }
                                    }
                                }
                            }
                        }

                        if (Enable_Developer_Mode)
                        {
                            accessory.Method.SendChat($"/e standbyPosition={standbyPosition} towerPosition={towerPosition}");
                        }
                        if (standbyPosition.Equals(new Vector3(100, 0, 100)) || towerPosition.Equals(new Vector3(100, 0, 100))) return;

                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Thunder_Towers_Tower_Process_Position_NonTank";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = standbyPosition;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 10500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Thunder_Towers_Tower_NonTank";
                        dp.Scale = new(4);
                        dp.Position = towerPosition;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 10500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
                    }
                }
                else
                {
                    var eastTower = p1Towers[3] == 1;
                    if (myIndex == 0 || myIndex == 1)
                    {
                        var dx2 = eastTower ? -2f : 2f;
                        var dx1 = eastTower ? -5.5f : 5.5f;
                        var dy = myIndex == 0 ? -5.5f : 5.5f;
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Fire_Towers_Tower_Process_Position_Tank1";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = new(100 + dx1, 0, 100 + dy);
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 6500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Fire_Towers_Tower_Process_Position_Tank2";
                        dp.Scale = new(2);
                        dp.Position = new(100 + dx1, 0, 100 + dy);
                        dp.TargetPosition = new(100 + dx2, 0, 100 + dy);
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 6500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Fire_Towers_Tower_Process_Position_Tank3";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = new(100 + dx2, 0, 100 + dy);
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.Delay = 6500;
                        dp.DestoryAt = 1700;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                    else
                    {
                        int myTowerIndex = myIndex - 1;
                        Vector3 standbyPosition = new Vector3(100, 0, 100);
                        Vector3 towerPosition = new Vector3(100, 0, 100);

                        if (Phase1_Strat_Of_Towers == Phase1_Strats_Of_Towers.Completely_Based_On_Priority)
                        {
                            if (myTowerIndex > 0 && myTowerIndex <= p1Towers[0]) standbyPosition = new(eastTower ? 102f : 98f, 0, 90.81f);
                            if (myTowerIndex > p1Towers[0] && myTowerIndex <= p1Towers[0] + p1Towers[1]) standbyPosition = new(eastTower ? 102f : 98f, 0, 100f);
                            if (myTowerIndex > p1Towers[0] + p1Towers[1] && myTowerIndex <= p1Towers[0] + p1Towers[1] + p1Towers[2]) standbyPosition = new(eastTower ? 102f : 98f, 0, 109.18f);
                            if (myTowerIndex > 0 && myTowerIndex <= p1Towers[0]) towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                            if (myTowerIndex > p1Towers[0] && myTowerIndex <= p1Towers[0] + p1Towers[1]) towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                            if (myTowerIndex > p1Towers[0] + p1Towers[1] && myTowerIndex <= p1Towers[0] + p1Towers[1] + p1Towers[2]) towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);
                        }

                        if (Phase1_Strat_Of_Towers == Phase1_Strats_Of_Towers.Fixed_H1H2R2_Priority_For_Rest)
                        {
                            bool fixedPartyMember = false;
                            if (myIndex == 2) { fixedPartyMember = true; standbyPosition = new(eastTower ? 102f : 98f, 0, 90.81f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); }
                            if (myIndex == 3) { fixedPartyMember = true; standbyPosition = new(eastTower ? 102f : 98f, 0, 100f); towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); }
                            if (myIndex == 7) { fixedPartyMember = true; standbyPosition = new(eastTower ? 102f : 98f, 0, 109.18f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); }

                            if (!fixedPartyMember)
                            {
                                int newTower0 = p1Towers[0] - 1;
                                int newTower1 = p1Towers[1] - 1;
                                int newTower2 = p1Towers[2] - 1;
                                int myNewTowerIndex = myIndex - 3;
                                if (Enable_Developer_Mode)
                                {
                                    accessory.Method.SendChat($"/e newTower0={newTower0} newTower1={newTower1} newTower2={newTower2} myNewTowerIndex={myNewTowerIndex}");
                                }
                                if (newTower0 > 0 && 0 < myNewTowerIndex && myNewTowerIndex <= newTower0)
                                { standbyPosition = new(eastTower ? 102f : 98f, 0, 90.81f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); }
                                if (newTower1 > 0 && newTower0 < myNewTowerIndex && myNewTowerIndex <= newTower0 + newTower1)
                                { standbyPosition = new(eastTower ? 102f : 98f, 0, 100f); towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); }
                                if (newTower2 > 0 && newTower0 + newTower1 < myNewTowerIndex && myNewTowerIndex <= newTower0 + newTower1 + newTower2)
                                { standbyPosition = new(eastTower ? 102f : 98f, 0, 109.18f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); }
                            }
                        }

                        if (Phase1_Strat_Of_Towers == Phase1_Strats_Of_Towers.Fixed_H1H2R2_Rest_Fill_Vacancies)
                        {
                            bool fixedPartyMember = false;
                            if (myIndex == 2) { fixedPartyMember = true; standbyPosition = new(eastTower ? 102f : 98f, 0, 90.81f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); }
                            if (myIndex == 3) { fixedPartyMember = true; standbyPosition = new(eastTower ? 102f : 98f, 0, 100f); towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); }
                            if (myIndex == 7) { fixedPartyMember = true; standbyPosition = new(eastTower ? 102f : 98f, 0, 109.18f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); }

                            if (!fixedPartyMember)
                            {
                                if (Enable_Developer_Mode)
                                {
                                    accessory.Method.SendChat($"/e p1Towers[0]={p1Towers[0]} p1Towers[1]={p1Towers[1]} p1Towers[2]={p1Towers[2]} myTowerIndex={myTowerIndex}");
                                }
                                if (myIndex == 4)
                                {
                                    if (p1Towers[0] >= 2)
                                    { standbyPosition = new(eastTower ? 102f : 98f, 0, 90.81f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); }
                                    else
                                    {
                                        if (p1Towers[1] >= 3) { standbyPosition = new(eastTower ? 102f : 98f, 0, 100f); towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); }
                                        if (p1Towers[2] >= 3) { standbyPosition = new(eastTower ? 102f : 98f, 0, 109.18f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); }
                                    }
                                }
                                if (myIndex == 5)
                                {
                                    if (p1Towers[1] >= 2)
                                    { standbyPosition = new(eastTower ? 102f : 98f, 0, 100f); towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); }
                                    else
                                    {
                                        if (p1Towers[0] >= 3) { standbyPosition = new(eastTower ? 102f : 98f, 0, 90.81f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); }
                                        if (p1Towers[2] >= 3) { standbyPosition = new(eastTower ? 102f : 98f, 0, 109.18f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); }
                                    }
                                }
                                if (myIndex == 6)
                                {
                                    if (p1Towers[2] >= 2)
                                    { standbyPosition = new(eastTower ? 102f : 98f, 0, 109.18f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f); }
                                    else
                                    {
                                        if (p1Towers[0] >= 3) { standbyPosition = new(eastTower ? 102f : 98f, 0, 90.81f); towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f); }
                                        if (p1Towers[1] >= 3) { standbyPosition = new(eastTower ? 102f : 98f, 0, 100f); towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f); }
                                    }
                                }
                            }
                        }

                        if (Enable_Developer_Mode)
                        {
                            accessory.Method.SendChat($"/e standbyPosition={standbyPosition} towerPosition={towerPosition}");
                        }
                        if (standbyPosition.Equals(new Vector3(100, 0, 100)) || towerPosition.Equals(new Vector3(100, 0, 100))) return;

                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Fire_Towers_Tower_Process_Position_NonTank";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = standbyPosition;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 9000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Fire_Towers_Tower_NonTank";
                        dp.Scale = new(4);
                        dp.Position = towerPosition;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 10500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
                    }
                }
            });
        }

        #endregion

        #region Phase_2

        [ScriptMethod(name: "----- Phase 2 -----",
            eventType: EventTypeEnum.NpcYell,
            eventCondition: ["Your poor"])]
        public void Phase2_Placeholder(Event @event, ScriptAccessory accessory) { }

        [ScriptMethod(name: "P2_Phase_Transition", eventType: EventTypeEnum.Director, eventCondition: ["Instance:800375BF", "Command:8000001E"], userControl: false)]
        public void P2_Phase_Transition(Event @event, ScriptAccessory accessory)
        {
            parse = 2;
        }

        [ScriptMethod(name: "Phase2 Diamond Dust Initialization",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40180"],
            userControl: false)]
        public void Phase2_Diamond_Dust_Initialization(Event @event, ScriptAccessory accessory)
        {
            parse = 21;
            phase2BossId = @event["SourceId"];
            phase2IcicleImpactPositions.Clear();
            phase2KnockbackPosition = new Vector3(100, 0, 100);
            phase2GuidanceBeforeKnockbackSemaphore = new System.Threading.AutoResetEvent(false);
            phase2GuidanceAfterKnockbackSemaphore = new System.Threading.AutoResetEvent(false);
        }

        [ScriptMethod(name: "P2_Diamond_Dust_Iron_Moon_Recording", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4020[23]))$"], userControl: false)]
        public void P2_Diamond_Dust_Iron_Moon_Recording(Event @event, ScriptAccessory accessory)
        {
            p2DiamondDustIsIron = (@event["ActionId"] == "40202");
        }

        [ScriptMethod(name: "P2_Diamond_Dust_Iron_Moon", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4020[23]))$"])]
        public void P2_Diamond_Dust_Iron_Moon(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (@event["ActionId"] == "40202")
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_Diamond_Dust_Iron";
                dp.Scale = new(16);
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_Diamond_Dust_Moon";
                dp.Scale = new(20);
                dp.InnerScale = new(4);
                dp.Radian = float.Pi * 2;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
            }
        }

        [ScriptMethod(name: "P2_Diamond_Dust_Cone_Guide", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4020[23]))$"])]
        public void P2_Diamond_Dust_Cone_Guide(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dur = 3000;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Diamond_Dust_Cone_Guide1";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Diamond_Dust_Cone_Guide2";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Diamond_Dust_Cone_Guide3";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Diamond_Dust_Cone_Guide4";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "P2_Diamond_Dust_Icicle_Placement_Position", eventType: EventTypeEnum.TargetIcon)]
        public void P2_Diamond_Dust_Icicle_Placement_Position(Event @event, ScriptAccessory accessory)
        {
            if (ParsTargetIcon(@event["Id"]) != 127) return;
            if (parse != 21) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var rot = myIndex switch
            {
                0 => 6,
                1 => 0,
                2 => 4,
                3 => 2,
                4 => 4,
                5 => 2,
                6 => 6,
                7 => 0,
                _ => 0,
            };
            Vector3 epos1 = p2DiamondDustIsIron ? new(119.5f, 0, 100.0f) : new(103.5f, 0, 100.0f);
            Vector3 epos2 = p2DiamondDustIsIron ? new(119.5f, 0, 100.0f) : new(108.0f, 0, 100.0f);
            var dir8 = phase2IcicleImpactPositions.FirstOrDefault() % 4;
            var dr = dir8 == 0 || dir8 == 2 ? -1 : 0;
            var dealpos1 = RotatePoint(epos1, new(100, 0, 100), float.Pi / 4 * (rot + dr));
            var dealpos2 = RotatePoint(epos2, new(100, 0, 100), float.Pi / 4 * (rot + dr));
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Diamond_Dust_Icicle_Placement_Position1";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos1;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Diamond_Dust_Icicle_Placement_Position2";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Position = dealpos1;
            dp.TargetPosition = dealpos2;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Diamond_Dust_Icicle_Placement_Position3";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos2;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 5500;
            dp.DestoryAt = 2500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "Phase2 Frigid Needle",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40199"])]
        public void Phase2_Frigid_Needle(Event @event, ScriptAccessory accessory)
        {
            if (parse != 21) return;
            Vector3 center = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            for (int i = 0; i <= 7; ++i)
            {
                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase2_Frigid_Needle";
                currentProperty.Scale = new(5, 40);
                currentProperty.Position = center;
                currentProperty.Color = accessory.Data.DefaultDangerColor;
                currentProperty.Rotation = (float.Pi / 4) * i;
                currentProperty.Delay = 3250;
                currentProperty.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);
            }
        }

        [ScriptMethod(name: "P2_Diamond_Dust_Cone_Guide_Position", eventType: EventTypeEnum.TargetIcon)]
        public void P2_Diamond_Dust_Cone_Guide_Position(Event @event, ScriptAccessory accessory)
        {
            if (ParsTargetIcon(@event["Id"]) != 127) return;
            if (parse != 21) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            int[] group = [6, 7, 4, 5, 2, 3, 0, 1];
            if (accessory.Data.PartyList.IndexOf(((uint)tid)) != group[myIndex]) return;
            var rot = myIndex switch
            {
                0 => 6,
                1 => 0,
                2 => 4,
                3 => 2,
                4 => 4,
                5 => 2,
                6 => 6,
                7 => 0,
                _ => 0,
            };
            var dir8 = phase2IcicleImpactPositions.FirstOrDefault() % 4;
            var dr = dir8 == 0 || dir8 == 2 ? 0 : -1;
            Vector3 epos = p2DiamondDustIsIron ? new(116.5f, 0, 100f) : new(101f, 0, 100f);
            var dealpos = RotatePoint(epos, new(100, 0, 100), float.Pi / 4 * (rot + dr));
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Diamond_Dust_Cone_Guide_Position";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 6500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "Phase2 Record Positions Of Icicle Impact",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40198"],
            userControl: false)]
        public void Phase2_Record_Positions_Of_Icicle_Impact(Event @event, ScriptAccessory accessory)
        {
            if (parse != 21) return;
            Vector3 currentPositions = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            int proteanPosition = PositionTo8Dir(currentPositions, new(100, 0, 100));
            lock (phase2IcicleImpactPositions)
            {
                phase2IcicleImpactPositions.Add(proteanPosition);
            }
            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e currentPositions={currentPositions} proteanPosition={proteanPosition}");
            }
        }

        [ScriptMethod(name: "Phase2 Determine The Position To Be Knocked Back",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40199"],
            userControl: false,
            suppress: 2000)]
        public void Phase2_Determine_The_Position_To_Be_Knocked_Back(Event @event, ScriptAccessory accessory)
        {
            if (parse != 21) return;
            if (phase2IcicleImpactPositions.Count == 0) return;

            int firstIcicleImpact = phase2IcicleImpactPositions.First() % 4;
            bool inStGroup = ((int[])[1, 3, 5, 7]).Contains(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
            int rotation = firstIcicleImpact switch
            {
                0 => 2,
                1 => -1,
                2 => 0,
                3 => 1,
            };
            rotation += ((inStGroup) ? (4) : (0));
            phase2KnockbackPosition = RotatePoint(new Vector3(95, 0, 100), new(100, 0, 100), float.Pi / 4 * rotation);
            System.Threading.Thread.MemoryBarrier();
            phase2GuidanceBeforeKnockbackSemaphore.Set();
            phase2GuidanceAfterKnockbackSemaphore.Set();
        }

        [ScriptMethod(name: "Phase2 Guidance Of The Position To Be Knocked Back",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40199"],
            suppress: 2000)]
        public void Phase2_Guidance_Of_The_Position_To_Be_Knocked_Back(Event @event, ScriptAccessory accessory)
        {
            if (parse != 21) return;
            if (phase2IcicleImpactPositions.Count == 0) return;
            System.Threading.Thread.MemoryBarrier();
            phase2GuidanceBeforeKnockbackSemaphore.WaitOne();
            System.Threading.Thread.MemoryBarrier();

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase2_Guidance_Of_The_Position_To_Be_Knocked_Back";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = phase2KnockbackPosition;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 4500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
        }

        [ScriptMethod(name: "Phase2 Guidance After Knockback",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40208"])]
        public void Phase2_Guidance_After_Knockback(Event @event, ScriptAccessory accessory)
        {
            if (parse != 21) return;
            if (phase2IcicleImpactPositions.Count == 0) return;
            System.Threading.Thread.MemoryBarrier();
            phase2GuidanceAfterKnockbackSemaphore.WaitOne();
            System.Threading.Thread.MemoryBarrier();

            Vector3 positionOfTheReflection = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            int proteanPositionOfTheReflection = PositionTo8Dir(positionOfTheReflection, new(100, 0, 100));
            int proteanPositionOfTheCurrentGroup = PositionTo8Dir(phase2KnockbackPosition, new(100, 0, 100));
            int proteanPositionOfTheOppositeGroup = phase2_getOppositeProteanPosition(proteanPositionOfTheCurrentGroup);
            bool propertyHasBeenConfirmed = false;
            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            string prompt = "";

            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e positionOfTheReflection={positionOfTheReflection} proteanPositionOfTheReflection={proteanPositionOfTheReflection} proteanPositionOfTheCurrentGroup={proteanPositionOfTheCurrentGroup} proteanPositionOfTheOppositeGroup={proteanPositionOfTheOppositeGroup}");
            }

            currentProperty.Name = "Phase2_Guidance_After_Knockback";
            currentProperty.Scale = new(20);
            currentProperty.InnerScale = new(19);
            currentProperty.Position = new Vector3(100, 0, 100);
            currentProperty.Rotation = float.Pi - (float.Pi / 4 * proteanPositionOfTheCurrentGroup);
            currentProperty.Color = accessory.Data.DefaultSafeColor.WithW(25f);
            currentProperty.DestoryAt = 14250;

            if (Phase2_Strat_After_Knockback == Phase2_Strats_After_Knockback.Clockwise_One_Group_Counterclockwise)
            {
                if (((proteanPositionOfTheCurrentGroup + 1) % 8) == proteanPositionOfTheReflection)
                {
                    currentProperty.Radian = float.Pi / 2 - float.Pi / 18;
                    currentProperty.Rotation += (float.Pi / 2 - float.Pi / 18) / 2;
                    if (Language_Of_Prompts == Languages_Of_Prompts.English)
                    {
                        prompt = "Counterclockwise 80 degrees, encountering the opposite group";
                    }
                }
                else
                {
                    if (((proteanPositionOfTheOppositeGroup + 1) % 8) == proteanPositionOfTheReflection)
                    {
                        currentProperty.Radian = float.Pi / 2 - float.Pi / 18;
                        currentProperty.Rotation += -((float.Pi / 2 - float.Pi / 18) / 2);
                        if (Language_Of_Prompts == Languages_Of_Prompts.English)
                        {
                            prompt = "Clockwise 80 degrees, encountering the opposite group";
                        }
                    }
                    else
                    {
                        int rotationOfThePath = 1;
                        while (((proteanPositionOfTheCurrentGroup + rotationOfThePath) % 8) != proteanPositionOfTheReflection &&
                              ((proteanPositionOfTheCurrentGroup + rotationOfThePath) % 8) != phase2_getOppositeProteanPosition(proteanPositionOfTheReflection))
                        {
                            ++rotationOfThePath;
                        }
                        currentProperty.Radian = float.Pi / 4 * rotationOfThePath;
                        currentProperty.Rotation += -((float.Pi / 4 * rotationOfThePath) / 2);
                        rotationOfThePath *= 45;
                        if (Language_Of_Prompts == Languages_Of_Prompts.English)
                        {
                            prompt = $"Clockwise {rotationOfThePath} degrees";
                        }
                    }
                }
                propertyHasBeenConfirmed = true;
            }

            if (Phase2_Strat_After_Knockback == Phase2_Strats_After_Knockback.Counterclockwise_One_Group_Clockwise)
            {
                if (((proteanPositionOfTheCurrentGroup - 1 + 8) % 8) == proteanPositionOfTheReflection)
                {
                    currentProperty.Radian = float.Pi / 2 - float.Pi / 18;
                    currentProperty.Rotation += -((float.Pi / 2 - float.Pi / 18) / 2);
                    if (Language_Of_Prompts == Languages_Of_Prompts.English)
                    {
                        prompt = "Clockwise 80 degrees, encountering the opposite group";
                    }
                }
                else
                {
                    if (((proteanPositionOfTheOppositeGroup - 1 + 8) % 8) == proteanPositionOfTheReflection)
                    {
                        currentProperty.Radian = float.Pi / 2 - float.Pi / 18;
                        currentProperty.Rotation += (float.Pi / 2 - float.Pi / 18) / 2;
                        if (Language_Of_Prompts == Languages_Of_Prompts.English)
                        {
                            prompt = "Counterclockwise 80 degrees, encountering the opposite group";
                        }
                    }
                    else
                    {
                        int rotationOfThePath = 1;
                        while (((proteanPositionOfTheCurrentGroup - rotationOfThePath + 8) % 8) != proteanPositionOfTheReflection &&
                              ((proteanPositionOfTheCurrentGroup - rotationOfThePath + 8) % 8) != phase2_getOppositeProteanPosition(proteanPositionOfTheReflection))
                        {
                            ++rotationOfThePath;
                        }
                        currentProperty.Radian = float.Pi / 4 * rotationOfThePath;
                        currentProperty.Rotation += (float.Pi / 4 * rotationOfThePath) / 2;
                        rotationOfThePath *= 45;
                        if (Language_Of_Prompts == Languages_Of_Prompts.English)
                        {
                            prompt = $"Counterclockwise {rotationOfThePath} degrees";
                        }
                    }
                }
                propertyHasBeenConfirmed = true;
            }

            if (Phase2_Strat_After_Knockback == Phase2_Strats_After_Knockback.Clockwise_Both_Groups_Counterclockwise)
            {
                if (((proteanPositionOfTheCurrentGroup + 1) % 8) == proteanPositionOfTheReflection ||
                    ((proteanPositionOfTheOppositeGroup + 1) % 8) == proteanPositionOfTheReflection)
                {
                    currentProperty.Radian = float.Pi / 4 * 3;
                    currentProperty.Rotation += (float.Pi / 4 * 3) / 2;
                    if (Language_Of_Prompts == Languages_Of_Prompts.English)
                    {
                        prompt = "Counterclockwise 135 degrees";
                    }
                }
                else
                {
                    int rotationOfThePath = 1;
                    while (((proteanPositionOfTheCurrentGroup + rotationOfThePath) % 8) != proteanPositionOfTheReflection &&
                          ((proteanPositionOfTheCurrentGroup + rotationOfThePath) % 8) != phase2_getOppositeProteanPosition(proteanPositionOfTheReflection))
                    {
                        ++rotationOfThePath;
                    }
                    currentProperty.Radian = float.Pi / 4 * rotationOfThePath;
                    currentProperty.Rotation += -((float.Pi / 4 * rotationOfThePath) / 2);
                    rotationOfThePath *= 45;
                    if (Language_Of_Prompts == Languages_Of_Prompts.English)
                    {
                        prompt = $"Clockwise {rotationOfThePath} degrees";
                    }
                }
                propertyHasBeenConfirmed = true;
            }

            if (Phase2_Strat_After_Knockback == Phase2_Strats_After_Knockback.Counterclockwise_Both_Groups_Clockwise)
            {
                if (((proteanPositionOfTheCurrentGroup - 1 + 8) % 8) == proteanPositionOfTheReflection ||
                    ((proteanPositionOfTheOppositeGroup - 1 + 8) % 8) == proteanPositionOfTheReflection)
                {
                    currentProperty.Radian = float.Pi / 4 * 3;
                    currentProperty.Rotation += -((float.Pi / 4 * 3) / 2);
                    if (Language_Of_Prompts == Languages_Of_Prompts.English)
                    {
                        prompt = "Clockwise 135 degrees";
                    }
                }
                else
                {
                    int rotationOfThePath = 1;
                    while (((proteanPositionOfTheCurrentGroup - rotationOfThePath + 8) % 8) != proteanPositionOfTheReflection &&
                          ((proteanPositionOfTheCurrentGroup - rotationOfThePath + 8) % 8) != phase2_getOppositeProteanPosition(proteanPositionOfTheReflection))
                    {
                        ++rotationOfThePath;
                    }
                    currentProperty.Radian = float.Pi / 4 * rotationOfThePath;
                    currentProperty.Rotation += (float.Pi / 4 * rotationOfThePath) / 2;
                    rotationOfThePath *= 45;
                    if (Language_Of_Prompts == Languages_Of_Prompts.English)
                    {
                        prompt = $"Counterclockwise {rotationOfThePath} degrees";
                    }
                }
                propertyHasBeenConfirmed = true;
            }

            if (propertyHasBeenConfirmed)
            {
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, currentProperty);
            }

            if (!prompt.Equals(""))
            {
                if (Enable_Text_Prompts) accessory.Method.TextInfo(prompt, 9000);
                if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS) accessory.TTS(prompt, Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase2_Front_Central_Axis_Of_Oracles_Reflection";
            currentProperty.Scale = new(0.5f, 50f);
            currentProperty.Owner = sourceId;
            currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(25f);
            currentProperty.DestoryAt = 14250;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase2_Rear_Separator_Of_Oracles_Reflection";
            currentProperty.Scale = new(0.3f, 10f);
            currentProperty.Owner = sourceId;
            currentProperty.Rotation = float.Pi / 4 * 3;
            currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(25f);
            currentProperty.DestoryAt = 14250;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase2_Rear_Separator_Of_Oracles_Reflection";
            currentProperty.Scale = new(0.3f, 10f);
            currentProperty.Owner = sourceId;
            currentProperty.Rotation = -(float.Pi / 4 * 3);
            currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(25f);
            currentProperty.DestoryAt = 14250;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);
        }

        private int phase2_getOppositeProteanPosition(int currentProteanPosition)
        {
            return currentProteanPosition switch
            {
                0 => 4,
                1 => 5,
                2 => 6,
                3 => 7,
                4 => 0,
                5 => 1,
                6 => 2,
                7 => 3,
                _ => currentProteanPosition
            };
        }

        [ScriptMethod(name: "Phase2 Prediction Of Skating",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40208"])]
        public void Phase2_Prediction_Of_Skating(Event @event, ScriptAccessory accessory)
        {
            if (parse != 21) return;
            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase2_Prediction_Of_Skating";
            currentProperty.Scale = new(2f, 32f);
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(3f);
            currentProperty.Delay = 14250;
            currentProperty.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, currentProperty);
        }

        [ScriptMethod(name: "P2_Diamond_Dust_Sequential_Blade_Range", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4019[34]$"])]
        public void P2_Diamond_Dust_Sequential_Blade_Range(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var time = 300;
            if (@event["ActionId"] == "40193")
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_Diamond_Dust_Sequential_Blade_Range_Front1";
                dp.Scale = new(30);
                dp.Radian = float.Pi / 2 * 3;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 3500 - time;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_Diamond_Dust_Sequential_Blade_Range_Back2";
                dp.Scale = new(30);
                dp.Radian = float.Pi / 2;
                dp.Rotation = float.Pi;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 3500 - time;
                dp.DestoryAt = 2000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_Diamond_Dust_Sequential_Blade_Range_Back1";
                dp.Scale = new(30);
                dp.Radian = float.Pi / 2;
                dp.Rotation = float.Pi;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 3500 - time;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_Diamond_Dust_Sequential_Blade_Range_Front2";
                dp.Scale = new(30);
                dp.Radian = float.Pi / 2 * 3;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 3500 - time;
                dp.DestoryAt = 2000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
        }

        [ScriptMethod(name: "P2_Diamond_Dust_Boss_Lookaway", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^40208$", "TargetIndex:1"])]
        public void P2_Diamond_Dust_Boss_Lookaway(Event @event, ScriptAccessory accessory)
        {
            if (parse != 21) return;
            if (!ParseObjectId(phase2BossId, out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Diamond_Dust_Boss_Lookaway";
            dp.Scale = new(5);
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.SightAvoid, dp);
        }

        [ScriptMethod(name: "Phase2 Reset Semaphores After Diamond Dust",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40210"],
            userControl: false)]
        public void Phase2_Reset_Semaphores_After_Diamond_Dust(Event @event, ScriptAccessory accessory)
        {
            if (parse != 21 && parse != 22) return;
            phase2GuidanceBeforeKnockbackSemaphore = new System.Threading.AutoResetEvent(false);
            phase2GuidanceAfterKnockbackSemaphore = new System.Threading.AutoResetEvent(false);
        }

        [ScriptMethod(name: "Phase2 Mirror Mirror Initialization",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40179"],
            userControl: false)]
        public void Phase2_Mirror_Mirror_Initialization(Event @event, ScriptAccessory accessory)
        {
            parse = 22;
            phase2ColourlessMirrorProteanPosition = -1;
            phase2ColourlessMirrorConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
            phase2RedMirrorProteanPositions.Clear();
            phase2RedMirrorsConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
        }

        [ScriptMethod(name: "P2_Two_Mirrors_Spread_Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[01])$"])]
        public void P2_Two_Mirrors_Spread_Stack(Event @event, ScriptAccessory accessory)
        {
            if (parse != 22) return;
            string prompt = "";
            if (@event["ActionId"] == "40221")
            {
                foreach (var pm in accessory.Data.PartyList)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_Two_Mirrors_Spread";
                    dp.Scale = new(5);
                    dp.Owner = pm;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    prompt = "Spread";
                }
            }
            else
            {
                int[] group = [4, 5, 6, 7, 0, 1, 2, 3];
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                for (int i = 0; i < 4; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_Two_Mirrors_Stack";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = group[myindex] == i || i == myindex ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    prompt = "Stack";
                }
            }

            if (!prompt.Equals(""))
            {
                if (Enable_Text_Prompts) accessory.Method.TextInfo(prompt, 1500);
                if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS) accessory.TTS(prompt, Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        [ScriptMethod(name: "P2_Two_Mirrors_Blue_Mirror_Moon_Plus_Guide", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375BF", "State:00020001"])]
        public void P2_Two_Mirrors_Blue_Mirror_Moon_Plus_Guide(Event @event, ScriptAccessory accessory)
        {
            if (parse != 22) return;
            if (!int.TryParse(@event["Index"], out var dir8)) return;
            Vector3 npos = new(100, 0, 80);
            dir8--;
            Vector3 dealpos = RotatePoint(npos, new(100, 0, 100), float.Pi / 4 * dir8);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Two_Mirrors_Blue_Mirror_Moon";
            dp.Scale = new(20);
            dp.InnerScale = new(4);
            dp.Radian = float.Pi * 2;
            dp.Position = dealpos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Two_Mirrors_Blue_Mirror_Cone_Guide1";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Two_Mirrors_Blue_Mirror_Cone_Guide2";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Two_Mirrors_Blue_Mirror_Cone_Guide3";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Two_Mirrors_Blue_Mirror_Cone_Guide4";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "P2_Two_Mirrors_Red_Mirror_Moon_Plus_Guide", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375BF", "State:02000100"])]
        public void P2_Two_Mirrors_Red_Mirror_Moon_Plus_Guide(Event @event, ScriptAccessory accessory)
        {
            if (parse != 22) return;
            if (!int.TryParse(@event["Index"], out var dir8)) return;
            Vector3 npos = new(100, 0, 80);
            dir8--;
            Vector3 dealpos = RotatePoint(npos, new(100, 0, 100), float.Pi / 4 * dir8);
            var dur = 4000;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Two_Mirrors_Red_Mirror_Moon";
            dp.Scale = new(20);
            dp.InnerScale = new(4);
            dp.Radian = float.Pi * 2;
            dp.Position = dealpos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 17000;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Two_Mirrors_Red_Mirror_Cone_Guide1";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 23000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Two_Mirrors_Red_Mirror_Cone_Guide2";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 23000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Two_Mirrors_Red_Mirror_Cone_Guide3";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 23000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Two_Mirrors_Red_Mirror_Cone_Guide4";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 23000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Phase2 Determine The Protean Position Of The Colourless Mirror",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:00020001"],
            userControl: false)]
        public void Phase2_Determine_The_Protean_Position_Of_The_Colourless_Mirror(Event @event, ScriptAccessory accessory)
        {
            if (parse != 22) return;
            if (!int.TryParse(@event["Index"], out var proteanPosition)) return;
            --proteanPosition;
            phase2ColourlessMirrorProteanPosition = proteanPosition;
            System.Threading.Thread.MemoryBarrier();
            phase2ColourlessMirrorConfirmedSemaphore.Set();
        }

        [ScriptMethod(name: "Phase2 Rough Guidance Of The Colourless Mirror",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:00020001"])]
        public void Phase2_Rough_Guidance_Of_The_Colourless_Mirror(Event @event, ScriptAccessory accessory)
        {
            if (parse != 22) return;
            if (!int.TryParse(@event["Index"], out var proteanPosition)) return;
            --proteanPosition;

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 rawPosition = new(100, 0, 100);
            bool isMeleeGroup = true;

            if (myIndex == 0 || myIndex == 1 || myIndex == 4 || myIndex == 5)
            {
                isMeleeGroup = true;
                rawPosition = new(100, 0, 85);
            }
            if (myIndex == 2 || myIndex == 3 || myIndex == 6 || myIndex == 7)
            {
                isMeleeGroup = false;
                rawPosition = new(100, 0, 80.5f);
            }
            if (rawPosition.Equals(new Vector3(100, 0, 100))) return;

            Vector3 targetPosition = RotatePoint(rawPosition, new(100, 0, 100), float.Pi / 4 * (proteanPosition + ((isMeleeGroup) ? (4) : (0))));
            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase2_Rough_Guidance_Of_The_Colourless_Mirror";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = targetPosition;
            currentProperty.Color = Phase2_Colour_Of_Mirror_Rough_Guidance.V4.WithW(1f);
            currentProperty.DestoryAt = 13000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            if (!ParseObjectId(phase2BossId, out var bossId)) return;
            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase2_Potential_Dangerous_Zone_Of_The_Colourless_Mirror";
            currentProperty.Scale = new(4);
            currentProperty.Radian = float.Pi;
            currentProperty.Owner = bossId;
            currentProperty.TargetPosition = RotatePoint(new Vector3(100, 0, 80), new Vector3(100, 0, 100), float.Pi / 4 * proteanPosition);
            currentProperty.Color = Phase2_Colour_Of_Potential_Dangerous_Zones.V4.WithW(3f);
            currentProperty.Delay = 6000;
            currentProperty.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase2_Potential_Dangerous_Zone_Of_The_Colourless_Mirror";
            currentProperty.Scale = new(4);
            currentProperty.Radian = float.Pi / 3;
            currentProperty.Position = RotatePoint(new Vector3(100, 0, 80), new Vector3(100, 0, 100), float.Pi / 4 * proteanPosition);
            currentProperty.TargetObject = bossId;
            currentProperty.Color = Phase2_Colour_Of_Potential_Dangerous_Zones.V4.WithW(3f);
            currentProperty.Delay = 6000;
            currentProperty.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);
        }

        [ScriptMethod(name: "Phase2 Determine Protean Positions Of Red Mirrors",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:02000100"],
            userControl: false)]
        public void Phase2_Determine_Protean_Positions_Of_Red_Mirrors(Event @event, ScriptAccessory accessory)
        {
            if (parse != 22) return;
            if (!int.TryParse(@event["Index"], out var proteanPosition)) return;
            --proteanPosition;

            lock (phase2RedMirrorProteanPositions)
            {
                if (phase2RedMirrorProteanPositions.Count < 2)
                {
                    phase2RedMirrorProteanPositions.Add(proteanPosition);
                }
                if (phase2RedMirrorProteanPositions.Count == 2)
                {
                    phase2RedMirrorsConfirmedSemaphore.Set();
                }
            }
        }

        [ScriptMethod(name: "Phase2 Rough Guidance Of Red Mirrors",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:02000100"],
            suppress: 2000)]
        public void Phase2_Rough_Guidance_Of_Red_Mirrors(Event @event, ScriptAccessory accessory)
        {
            if (parse != 22) return;
            System.Threading.Thread.MemoryBarrier();
            phase2ColourlessMirrorConfirmedSemaphore.WaitOne();
            phase2RedMirrorsConfirmedSemaphore.WaitOne();
            System.Threading.Thread.MemoryBarrier();

            if (phase2ColourlessMirrorProteanPosition == -1) return;
            if (phase2RedMirrorProteanPositions.Count != 2) return;

            int colourlessMirror = phase2ColourlessMirrorProteanPosition;
            int redMirror1 = phase2RedMirrorProteanPositions[0];
            int redMirror2 = phase2RedMirrorProteanPositions[1];
            int discreteDistanceToTheNext = 1;
            int leftMirror = -1;
            int rightMirror = -1;

            while (((redMirror1 + discreteDistanceToTheNext) % 8) != redMirror2)
            {
                ++discreteDistanceToTheNext;
            }
            if (discreteDistanceToTheNext != 2 && discreteDistanceToTheNext != 6) return;
            if (discreteDistanceToTheNext == 2) { leftMirror = redMirror1; rightMirror = redMirror2; }
            if (discreteDistanceToTheNext == 6) { leftMirror = redMirror2; rightMirror = redMirror1; }
            if (leftMirror == -1 || rightMirror == -1) return;

            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e leftMirror={leftMirror} rightMirror={rightMirror} discreteDistanceToTheNext={discreteDistanceToTheNext}");
            }

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            bool isMeleeGroup = true;
            if (myIndex == 0 || myIndex == 1 || myIndex == 4 || myIndex == 5) isMeleeGroup = true;
            if (myIndex == 2 || myIndex == 3 || myIndex == 6 || myIndex == 7) isMeleeGroup = false;

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            if (((leftMirror + 1) % 8) == colourlessMirror || ((leftMirror + 1) % 8) == phase2_getOppositeProteanPosition(colourlessMirror))
            {
                if (Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Left_Red ||
                    Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Closest_Red_Left_If_Same)
                {
                    Vector3 targetPosition = new Vector3(100, 0, 100);
                    if (isMeleeGroup) targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * leftMirror);
                    else targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * rightMirror);
                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase2_Rough_Guidance_Of_Red_Mirrors";
                    currentProperty.Scale = new(2);
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = targetPosition;
                    currentProperty.Color = Phase2_Colour_Of_Mirror_Rough_Guidance.V4.WithW(1f);
                    currentProperty.Delay = 13500;
                    currentProperty.DestoryAt = 9500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                }
                if (Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Right_Red ||
                    Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Closest_Red_Right_If_Same)
                {
                    Vector3 targetPosition = new Vector3(100, 0, 100);
                    if (isMeleeGroup) targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * rightMirror);
                    else targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * leftMirror);
                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase2_Rough_Guidance_Of_Red_Mirrors";
                    currentProperty.Scale = new(2);
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = targetPosition;
                    currentProperty.Color = Phase2_Colour_Of_Mirror_Rough_Guidance.V4.WithW(1f);
                    currentProperty.Delay = 13500;
                    currentProperty.DestoryAt = 9500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                }
            }
            else
            {
                if (Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Left_Red)
                {
                    Vector3 targetPosition = new Vector3(100, 0, 100);
                    if (isMeleeGroup) targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * leftMirror);
                    else targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * rightMirror);
                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase2_Rough_Guidance_Of_Red_Mirrors";
                    currentProperty.Scale = new(2);
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = targetPosition;
                    currentProperty.Color = Phase2_Colour_Of_Mirror_Rough_Guidance.V4.WithW(1f);
                    currentProperty.Delay = 13500;
                    currentProperty.DestoryAt = 9500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                }
                if (Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Right_Red)
                {
                    Vector3 targetPosition = new Vector3(100, 0, 100);
                    if (isMeleeGroup) targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * rightMirror);
                    else targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * leftMirror);
                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase2_Rough_Guidance_Of_Red_Mirrors";
                    currentProperty.Scale = new(2);
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = targetPosition;
                    currentProperty.Color = Phase2_Colour_Of_Mirror_Rough_Guidance.V4.WithW(1f);
                    currentProperty.Delay = 13500;
                    currentProperty.DestoryAt = 9500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                }

                int meleeGroup = phase2_getOppositeProteanPosition(colourlessMirror);
                int discreteDistanceToTheLeft = 0;
                int discreteDistanceToTheRight = 0;
                while (((meleeGroup + discreteDistanceToTheLeft) % 8) != leftMirror) ++discreteDistanceToTheLeft;
                while (((meleeGroup - discreteDistanceToTheRight + 8) % 8) != rightMirror) ++discreteDistanceToTheRight;

                if (Enable_Developer_Mode)
                {
                    accessory.Method.SendChat($"/e discreteDistanceToTheLeft={discreteDistanceToTheLeft} discreteDistanceToTheRight={discreteDistanceToTheRight}");
                }

                if (discreteDistanceToTheLeft < discreteDistanceToTheRight)
                {
                    if (Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Closest_Red_Left_If_Same ||
                        Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Closest_Red_Right_If_Same)
                    {
                        Vector3 targetPosition = new Vector3(100, 0, 100);
                        if (isMeleeGroup) targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * leftMirror);
                        else targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * rightMirror);
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase2_Rough_Guidance_Of_Red_Mirrors";
                        currentProperty.Scale = new(2);
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = targetPosition;
                        currentProperty.Color = Phase2_Colour_Of_Mirror_Rough_Guidance.V4.WithW(1f);
                        currentProperty.Delay = 13500;
                        currentProperty.DestoryAt = 9500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }
                }
                if (discreteDistanceToTheLeft > discreteDistanceToTheRight)
                {
                    if (Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Closest_Red_Left_If_Same ||
                        Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Closest_Red_Right_If_Same)
                    {
                        Vector3 targetPosition = new Vector3(100, 0, 100);
                        if (isMeleeGroup) targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * rightMirror);
                        else targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * leftMirror);
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase2_Rough_Guidance_Of_Red_Mirrors";
                        currentProperty.Scale = new(2);
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = targetPosition;
                        currentProperty.Color = Phase2_Colour_Of_Mirror_Rough_Guidance.V4.WithW(1f);
                        currentProperty.Delay = 13500;
                        currentProperty.DestoryAt = 9500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }
                }
            }

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase2_Potential_Dangerous_Zone_Of_Red_Mirrors";
            currentProperty.Scale = new(4);
            currentProperty.Radian = float.Pi / 3;
            currentProperty.Position = RotatePoint(new(100, 0, 80), new(100, 0, 100), float.Pi / 4 * leftMirror);
            currentProperty.Rotation = float.Pi / 6;
            currentProperty.TargetPosition = new Vector3(100, 0, 100);
            currentProperty.Color = Phase2_Colour_Of_Potential_Dangerous_Zones.V4.WithW(3f);
            currentProperty.Delay = 13500;
            currentProperty.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase2_Potential_Dangerous_Zone_Of_Red_Mirrors";
            currentProperty.Scale = new(4);
            currentProperty.Radian = float.Pi / 3;
            currentProperty.Position = RotatePoint(new(100, 0, 80), new(100, 0, 100), float.Pi / 4 * rightMirror);
            currentProperty.Rotation = -(float.Pi / 6);
            currentProperty.TargetPosition = new Vector3(100, 0, 100);
            currentProperty.Color = Phase2_Colour_Of_Potential_Dangerous_Zones.V4.WithW(3f);
            currentProperty.Delay = 13500;
            currentProperty.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);
        }

        [ScriptMethod(name: "Phase2 Reset Semaphores After Mirror Mirror",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40212"],
            userControl: false)]
        public void Phase2_Reset_Semaphores_After_Mirror_Mirror(Event @event, ScriptAccessory accessory)
        {
            if (parse != 22 && parse != 23) return;
            phase2ColourlessMirrorConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
            phase2RedMirrorsConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
        }

        [ScriptMethod(name: "Phase2 Light Rampant Initialization",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40212"],
            userControl: false)]
        public void Phase2_Light_Rampant_Initialization(Event @event, ScriptAccessory accessory)
        {
            parse = 23;
            phase2PlayersWithLuminousHammer.Clear();
            phase2LuminousHammerConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
            phase2LightsteepedStacks = [0, 0, 0, 0, 0, 0, 0, 0];
            phase2LightsteepedWritePermission = true;
            phase2FinalLightsteepedConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
        }

        [ScriptMethod(name: "Phase2 Initial Positions Before Light Rampant",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40212"])]
        public void Phase2_Initial_Positions_Before_Light_Rampant(Event @event, ScriptAccessory accessory)
        {
            if (parse != 22 && parse != 23) return;
            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            double rotation = 0d;

            if (Phase2_Initial_Protean_Position_Of_Light_Rampant == Phase2_Initial_Protean_Positions_Of_Light_Rampant.Normal_Protean_Tanks_North_East_For_Both_Grey9)
            {
                rotation = 0d;
                rotation += myIndex switch
                {
                    0 => 0d,
                    7 => 1d,
                    1 => 2d,
                    5 => 3d,
                    3 => 4d,
                    4 => 5d,
                    2 => 6d,
                    6 => 7d
                };
            }

            if (Phase2_Initial_Protean_Position_Of_Light_Rampant == Phase2_Initial_Protean_Positions_Of_Light_Rampant.Supporters_North_MOTH12_For_JPPF_And_L)
            {
                rotation = -0.5d;
                rotation += myIndex switch
                {
                    0 => -1d,
                    1 => 0d,
                    2 => 1d,
                    3 => 2d,
                    7 => 3d,
                    6 => 4d,
                    5 => 5d,
                    4 => 6d
                };
            }

            if (Phase2_Initial_Protean_Position_Of_Light_Rampant == Phase2_Initial_Protean_Positions_Of_Light_Rampant.Supporters_North_H12MOT_For_JPPF_And_L)
            {
                rotation = -0.5d;
                rotation += myIndex switch
                {
                    2 => -1d,
                    3 => 0d,
                    0 => 1d,
                    1 => 2d,
                    7 => 3d,
                    6 => 4d,
                    5 => 5d,
                    4 => 6d
                };
            }

            var currentproperty = accessory.Data.GetDefaultDrawProperties();
            currentproperty.Name = "Phase2_Initial_Positions_Before_Light_Rampant";
            currentproperty.Scale = new(2);
            currentproperty.Owner = accessory.Data.Me;
            currentproperty.TargetPosition = RotatePoint(new Vector3(100, 0, 95), new Vector3(100, 0, 100), (float)(float.Pi / 4 * rotation));
            currentproperty.ScaleMode |= ScaleMode.YByDistance;
            currentproperty.Color = accessory.Data.DefaultSafeColor;
            currentproperty.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);
        }

        [ScriptMethod(name: "Phase2 Rough Path Of Luminous Hammer",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40212"],
            suppress: 2000)]
        public void Phase2_Rough_Path_Of_Luminous_Hammer(Event @event, ScriptAccessory accessory)
        {
            if (parse != 23) return;
            var currentproperty = accessory.Data.GetDefaultDrawProperties();

            if (Phase2_Strat_Of_Light_Rampant == Phase2_Strats_Of_Light_Rampant.Star_Of_David_Japanese_PF)
            {
                Vector3 point1 = new Vector3(97.321f, 0f, 106.467f);
                Vector3 point1Symmetry = RotatePoint(point1, new Vector3(100, 0, 100), float.Pi);
                Vector3 point2 = new Vector3(93f, 0f, 100f);
                Vector3 point2Symmetry = RotatePoint(point2, new Vector3(100, 0, 100), float.Pi);
                Vector3 point3 = new Vector3(97.321f, 0f, 93.533f);
                Vector3 point3Symmetry = RotatePoint(point3, new Vector3(100, 0, 100), float.Pi);
                Vector3 point4 = new Vector3(97.321f, 0f, 82f);
                Vector3 point4Symmetry = RotatePoint(point4, new Vector3(100, 0, 100), float.Pi);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point1;
                currentproperty.TargetPosition = point2;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point2;
                currentproperty.TargetPosition = point3;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point3;
                currentproperty.TargetPosition = point4;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point1Symmetry;
                currentproperty.TargetPosition = point2Symmetry;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point2Symmetry;
                currentproperty.TargetPosition = point3Symmetry;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point3Symmetry;
                currentproperty.TargetPosition = point4Symmetry;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);
            }

            if (Phase2_Strat_Of_Light_Rampant == Phase2_Strats_Of_Light_Rampant.New_Grey9)
            {
                Vector3 point1 = new Vector3(92f, 0f, 100f);
                Vector3 point1Symmetry = RotatePoint(point1, new Vector3(100, 0, 100), float.Pi);
                Vector3 point2 = new Vector3(94.343f, 0f, 94.343f);
                Vector3 point2Symmetry = RotatePoint(point2, new Vector3(100, 0, 100), float.Pi);
                Vector3 point3 = new Vector3(100f, 0f, 92f);
                Vector3 point3Symmetry = RotatePoint(point3, new Vector3(100, 0, 100), float.Pi);
                Vector3 point4 = new Vector3(106.133f, 0f, 91.97f);
                Vector3 point4Symmetry = RotatePoint(point4, new Vector3(100, 0, 100), float.Pi);
                Vector3 point5 = new Vector3(111.314f, 0f, 88.686f);
                Vector3 point5Symmetry = RotatePoint(point5, new Vector3(100, 0, 100), float.Pi);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point1;
                currentproperty.TargetPosition = point2;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point2;
                currentproperty.TargetPosition = point3;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point3;
                currentproperty.TargetPosition = point4;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point4;
                currentproperty.TargetPosition = point5;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point1Symmetry;
                currentproperty.TargetPosition = point2Symmetry;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point2Symmetry;
                currentproperty.TargetPosition = point3Symmetry;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point3Symmetry;
                currentproperty.TargetPosition = point4Symmetry;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point4Symmetry;
                currentproperty.TargetPosition = point5Symmetry;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);
            }
        }

        [ScriptMethod(name: "Phase2 Determine Luminous Hammer During Light Rampant",
            eventType: EventTypeEnum.TargetIcon,
            userControl: false)]
        public void Phase2_Determine_Luminous_Hammer_During_Light_Rampant(Event @event, ScriptAccessory accessory)
        {
            if (ParsTargetIcon(@event["Id"]) != 157) return;
            if (parse != 23) return;
            if (!ParseObjectId(@event["TargetId"], out var targetId)) return;
            int currentIndex = accessory.Data.PartyList.IndexOf(((uint)targetId));
            lock (phase2PlayersWithLuminousHammer)
            {
                if (phase2PlayersWithLuminousHammer.Count < 2) phase2PlayersWithLuminousHammer.Add(currentIndex);
                if (phase2PlayersWithLuminousHammer.Count == 2) phase2LuminousHammerConfirmedSemaphore.Set();
            }
        }

        [ScriptMethod(name: "Phase2 Determine Stacks Of Lightsteeped During Light Rampant",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:2257"],
            userControl: false)]
        public void Phase2_Determine_Stacks_Of_Lightsteeped_During_Light_Rampant(Event @event, ScriptAccessory accessory)
        {
            if (parse != 23) return;
            if (!phase2LightsteepedWritePermission) return;
            if (!ParseObjectId(@event["TargetId"], out var targetId)) return;
            if (!int.TryParse(@event["StackCount"], out var stacks)) return;
            int currentIndex = accessory.Data.PartyList.IndexOf(((uint)targetId));
            lock (phase2LightsteepedStacks)
            {
                phase2LightsteepedStacks[currentIndex] = stacks;
            }
            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e currentIndex={currentIndex} stacks={stacks}");
            }
        }

        [ScriptMethod(name: "Phase2 Disable The Write Permission For Lightsteeped",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40218"],
            userControl: false)]
        public void Phase2_Disable_The_Write_Permission_For_Lightsteeped(Event @event, ScriptAccessory accessory)
        {
            phase2LightsteepedWritePermission = false;
        }

        [ScriptMethod(name: "P2_Light_Rampant_Spread_Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[01])$"])]
        public void P2_Light_Rampant_Spread_Stack(Event @event, ScriptAccessory accessory)
        {
            if (parse != 23) return;
            string prompt = "";
            if (@event["ActionId"] == "40221")
            {
                foreach (var pm in accessory.Data.PartyList)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_Light_Rampant_Spread";
                    dp.Scale = new(5);
                    dp.Owner = pm;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    prompt = "Spread";
                }
            }
            else
            {
                int[] group = [6, 7, 4, 5, 2, 3, 0, 1];
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                for (int i = 0; i < 4; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_Light_Rampant_Stack";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = group[myindex] == i || i == myindex ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    prompt = "Stack";
                }
            }

            if (!prompt.Equals(""))
            {
                if (Enable_Text_Prompts) accessory.Method.TextInfo(prompt, 1500);
                if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS) accessory.TTS(prompt, Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        [ScriptMethod(name: "P2_Light_Rampant_Stack_Buff", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4159"])]
        public void P2_Light_Rampant_Stack_Buff(Event @event, ScriptAccessory accessory)
        {
            if (parse != 23) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Light_Rampant_Stack_Buff";
            dp.Scale = new(5);
            dp.Owner = tid;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 12000;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Phase2 Guidance Of Towers During Light Rampant",
            eventType: EventTypeEnum.TargetIcon,
            suppress: 2000)]
        public void Phase2_Guidance_Of_Towers_During_Light_Rampant(Event @event, ScriptAccessory accessory)
        {
            if (ParsTargetIcon(@event["Id"]) != 157) return;
            if (parse != 23) return;
            System.Threading.Thread.MemoryBarrier();
            phase2LuminousHammerConfirmedSemaphore.WaitOne();
            System.Threading.Thread.MemoryBarrier();

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (phase2PlayersWithLuminousHammer.Contains(myIndex)) return;

            List<int> playersWithTethers = [];

            if (Phase2_Initial_Protean_Position_Of_Light_Rampant == Phase2_Initial_Protean_Positions_Of_Light_Rampant.Normal_Protean_Tanks_North_East_For_Both_Grey9)
            {
                List<int> orderFromTheWestInclusive = [2, 6, 0, 7, 1, 5, 3, 4];
                for (int i = 0; i < orderFromTheWestInclusive.Count; ++i)
                {
                    if (!phase2PlayersWithLuminousHammer.Contains(orderFromTheWestInclusive[i])) playersWithTethers.Add(orderFromTheWestInclusive[i]);
                }
            }

            if (Phase2_Initial_Protean_Position_Of_Light_Rampant == Phase2_Initial_Protean_Positions_Of_Light_Rampant.Supporters_North_MOTH12_For_JPPF_And_L)
            {
                List<int> orderFromTheWestInclusive = [0, 1, 2, 3, 7, 6, 5, 4];
                for (int i = 0; i < orderFromTheWestInclusive.Count; ++i)
                {
                    if (!phase2PlayersWithLuminousHammer.Contains(orderFromTheWestInclusive[i])) playersWithTethers.Add(orderFromTheWestInclusive[i]);
                }
            }

            if (Phase2_Initial_Protean_Position_Of_Light_Rampant == Phase2_Initial_Protean_Positions_Of_Light_Rampant.Supporters_North_H12MOT_For_JPPF_And_L)
            {
                List<int> orderFromTheWestInclusive = [2, 3, 0, 1, 7, 6, 5, 4];
                for (int i = 0; i < orderFromTheWestInclusive.Count; ++i)
                {
                    if (!phase2PlayersWithLuminousHammer.Contains(orderFromTheWestInclusive[i])) playersWithTethers.Add(orderFromTheWestInclusive[i]);
                }
            }

            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e playersWithTethers.Count={playersWithTethers.Count} playersWithTethers[]={playersWithTethers[0]},{playersWithTethers[1]},{playersWithTethers[2]},{playersWithTethers[3]},{playersWithTethers[4]},{playersWithTethers[5]}");
            }

            int myTetherIndex = playersWithTethers.IndexOf(myIndex);
            Vector3 myTower = new Vector3(100, 0, 100);
            Vector3 myMeetingPoint = new Vector3(100, 0, 100);

            Vector3 tower1 = new Vector3(100.00f, 0, 084.00f);
            Vector3 tower2 = new Vector3(113.85f, 0, 092.00f);
            Vector3 tower3 = new Vector3(113.85f, 0, 108.00f);
            Vector3 tower4 = new Vector3(100.00f, 0, 116.00f);
            Vector3 tower5 = new Vector3(086.14f, 0, 108.00f);
            Vector3 tower6 = new Vector3(086.14f, 0, 092.00f);

            Vector3 northMeetingPoint = new Vector3(100.00f, 0, 82.00f);
            Vector3 eastMeetingPoint = new Vector3(118.00f, 0, 100.00f);
            Vector3 southMeetingPoint = new Vector3(100.00f, 0, 118.00f);
            Vector3 westMeetingPoint = new Vector3(82.00f, 0, 100.00f);

            if (Phase2_Strat_Of_Light_Rampant == Phase2_Strats_Of_Light_Rampant.Star_Of_David_Japanese_PF)
            {
                accessory.Log.Debug("Star_Of_David_Japanese_PF");
                myTower = myTetherIndex switch
                {
                    1 => tower4,
                    4 => tower1,
                    0 => tower6,
                    2 => tower2,
                    3 => tower5,
                    5 => tower3
                };
                if (Vector3.Distance(myTower, tower1) < 1 || Vector3.Distance(myTower, tower2) < 1 || Vector3.Distance(myTower, tower6) < 1)
                {
                    myMeetingPoint = northMeetingPoint;
                }
                else
                {
                    myMeetingPoint = southMeetingPoint;
                }
            }

            if (Phase2_Strat_Of_Light_Rampant == Phase2_Strats_Of_Light_Rampant.New_Grey9)
            {
                foreach (var item in phase2PlayersWithLuminousHammer)
                {
                    accessory.Log.Debug($"{item}");
                }
                int numberOfPlayersWithLuminousHammerBefore = 0;

                if (myIndex == 0)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = tower4;
                }
                numberOfPlayersWithLuminousHammerBefore += (phase2PlayersWithLuminousHammer.Contains(0)) ? (1) : (0);

                if (myIndex == 7)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = (phase2PlayersWithLuminousHammer.Contains(0)) ? (tower4) : (tower6);
                }
                numberOfPlayersWithLuminousHammerBefore += (phase2PlayersWithLuminousHammer.Contains(7)) ? (1) : (0);

                if (myIndex == 1)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower2,
                        1 => tower6,
                        2 => tower4
                    };
                }
                numberOfPlayersWithLuminousHammerBefore += (phase2PlayersWithLuminousHammer.Contains(1)) ? (1) : (0);

                if (myIndex == 5)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower5,
                        1 => tower2,
                        2 => tower6
                    };
                }
                numberOfPlayersWithLuminousHammerBefore += (phase2PlayersWithLuminousHammer.Contains(5)) ? (1) : (0);

                if (myIndex == 3)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower3,
                        1 => tower5,
                        2 => tower2
                    };
                }
                numberOfPlayersWithLuminousHammerBefore += (phase2PlayersWithLuminousHammer.Contains(3)) ? (1) : (0);

                if (myIndex == 4)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower1,
                        1 => tower3,
                        2 => tower5
                    };
                }
                numberOfPlayersWithLuminousHammerBefore += (phase2PlayersWithLuminousHammer.Contains(4)) ? (1) : (0);

                if (myIndex == 2)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = (phase2PlayersWithLuminousHammer.Contains(6)) ? (tower1) : (tower3);
                }
                numberOfPlayersWithLuminousHammerBefore += (phase2PlayersWithLuminousHammer.Contains(2)) ? (1) : (0);

                if (myIndex == 6)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = tower1;
                }
                if (Vector3.Distance(myTower, tower2) < 1 || Vector3.Distance(myTower, tower3) < 1 || Vector3.Distance(myTower, tower4) < 1)
                {
                    myMeetingPoint = eastMeetingPoint;
                }
                else
                {
                    myMeetingPoint = westMeetingPoint;
                }
            }

            if (Phase2_Strat_Of_Light_Rampant == Phase2_Strats_Of_Light_Rampant.Lucrezia)
            {
                accessory.Log.Debug("Lucrezia");
                myTower = myTetherIndex switch
                {
                    1 => tower1,
                    4 => tower4,
                    0 => tower5,
                    2 => tower3,
                    3 => tower6,
                    5 => tower2
                };
                if (Vector3.Distance(myTower, tower1) < 1 || Vector3.Distance(myTower, tower2) < 1 || Vector3.Distance(myTower, tower6) < 1)
                {
                    myMeetingPoint = northMeetingPoint;
                }
                else
                {
                    myMeetingPoint = southMeetingPoint;
                }
            }

            if (Phase2_Strat_Of_Light_Rampant == Phase2_Strats_Of_Light_Rampant.Obsolete_Old_Grey9)
            {
                accessory.Log.Debug("Obsolete_Old_Grey9");
                int numberOfPlayersWithLuminousHammerBefore = 0;

                if (myIndex == 0)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = tower4;
                }
                numberOfPlayersWithLuminousHammerBefore += (phase2PlayersWithLuminousHammer.Contains(0)) ? (1) : (0);

                if (myIndex == 7)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = (phase2PlayersWithLuminousHammer.Contains(0)) ? (tower4) : (tower2);
                }
                numberOfPlayersWithLuminousHammerBefore += (phase2PlayersWithLuminousHammer.Contains(7)) ? (1) : (0);

                if (myIndex == 1)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower6,
                        1 => tower2,
                        2 => tower4
                    };
                }
                numberOfPlayersWithLuminousHammerBefore += (phase2PlayersWithLuminousHammer.Contains(1)) ? (1) : (0);

                if (myIndex == 5)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower3,
                        1 => tower6,
                        2 => tower2
                    };
                }
                numberOfPlayersWithLuminousHammerBefore += (phase2PlayersWithLuminousHammer.Contains(5)) ? (1) : (0);

                if (myIndex == 3)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower5,
                        1 => tower3,
                        2 => tower6
                    };
                }
                numberOfPlayersWithLuminousHammerBefore += (phase2PlayersWithLuminousHammer.Contains(3)) ? (1) : (0);

                if (myIndex == 4)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower1,
                        1 => tower5,
                        2 => tower3
                    };
                }
                numberOfPlayersWithLuminousHammerBefore += (phase2PlayersWithLuminousHammer.Contains(4)) ? (1) : (0);

                if (myIndex == 2)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = (phase2PlayersWithLuminousHammer.Contains(6)) ? (tower1) : (tower5);
                }
                numberOfPlayersWithLuminousHammerBefore += (phase2PlayersWithLuminousHammer.Contains(2)) ? (1) : (0);

                if (myIndex == 6)
                {
                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}");
                    myTower = tower1;
                }
                if (Vector3.Distance(myTower, tower1) < 1 || Vector3.Distance(myTower, tower2) < 1 || Vector3.Distance(myTower, tower3) < 1)
                {
                    myMeetingPoint = eastMeetingPoint;
                }
                else
                {
                    myMeetingPoint = westMeetingPoint;
                }
            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase2_Guidance_1_Of_Towers_During_Light_Rampant";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = myTower;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase2_Highlight_Of_The_Tower_During_Light_Rampant";
            currentProperty.Scale = new(4);
            currentProperty.Position = myTower;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase2_Guidance_2_Preview_Of_Towers_During_Light_Rampant";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Position = myTower;
            currentProperty.TargetPosition = myMeetingPoint;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase2_Guidance_2_Of_Towers_During_Light_Rampant";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = myMeetingPoint;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.Delay = 10000;
            currentProperty.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
        }

        [ScriptMethod(name: "Phase2 Determine Final Lightsteeped",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:00020001", "Index:00000015"],
            userControl: false,
            suppress: 2000)]
        public void Phase2_Determine_Final_Lightsteeped(Event @event, ScriptAccessory accessory)
        {
            if (parse != 23) return;
            lock (phase2PlayersWithLuminousHammer)
            {
                for (int i = 0; i < 8; ++i)
                {
                    lock (phase2LightsteepedStacks)
                    {
                        ++phase2LightsteepedStacks[i];
                    }
                }
            }
            lock (phase2PlayersWithLuminousHammer)
            {
                for (int i = 0; i < 8; ++i)
                {
                    if (!phase2PlayersWithLuminousHammer.Contains(i))
                    {
                        lock (phase2LightsteepedStacks)
                        {
                            ++phase2LightsteepedStacks[i];
                        }
                    }
                }
            }
            System.Threading.Thread.MemoryBarrier();
            phase2FinalLightsteepedConfirmedSemaphore.Set();
            System.Threading.Thread.MemoryBarrier();

            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e phase2LightsteepedStacks[]={phase2LightsteepedStacks[0]},{phase2LightsteepedStacks[1]},{phase2LightsteepedStacks[2]},{phase2LightsteepedStacks[3]},{phase2LightsteepedStacks[4]},{phase2LightsteepedStacks[5]},{phase2LightsteepedStacks[6]},{phase2LightsteepedStacks[7]}");
            }
        }

        [ScriptMethod(name: "Phase2 Guidance Of The Last Tower During Light Rampant",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:00020001", "Index:00000015"])]
        public void Phase2_Guidance_Of_The_Last_Tower_During_Light_Rampant(Event @event, ScriptAccessory accessory)
        {
            if (parse != 23) return;
            System.Threading.Thread.MemoryBarrier();
            phase2FinalLightsteepedConfirmedSemaphore.WaitOne();
            System.Threading.Thread.MemoryBarrier();

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (phase2LightsteepedStacks[myIndex] < 3)
            {
                var currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase2_Guidance_Of_The_Last_Tower_During_Light_Rampant";
                currentProperty.Scale = new(2);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = new Vector3(100, 0, 100);
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.Delay = 2500;
                currentProperty.DestoryAt = 5500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase2_Highlight_Of_The_Last_Tower_During_Light_Rampant";
                currentProperty.Scale = new(4);
                currentProperty.Position = new Vector3(100, 0, 100);
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.Delay = 2500;
                currentProperty.DestoryAt = 5500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);
            }

            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e phase2LightsteepedStacks[myIndex]={phase2LightsteepedStacks[myIndex]}");
            }
        }

        [ScriptMethod(name: "P2_Light_Rampant_Eightfold_Spread_Position", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4022[01])$"])]
        public void P2_Light_Rampant_Eightfold_Spread_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 23) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var rot8 = myindex switch
            {
                0 => 0,
                1 => 2,
                2 => 6,
                3 => 4,
                4 => 5,
                5 => 3,
                6 => 7,
                7 => 1,
                _ => 0,
            };
            var mPosEnd = RotatePoint(new(100, 0, 95), new(100, 0, 100), float.Pi / 4 * rot8);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_Light_Rampant_Eightfold_Spread_Position";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = mPosEnd;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "Phase2 Reset Semaphores After Light Rampant",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40224"],
            userControl: false)]
        public void Phase2_Reset_Semaphores_After_Light_Rampant(Event @event, ScriptAccessory accessory)
        {
            if (parse != 23) return;
            phase2LuminousHammerConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
            phase2FinalLightsteepedConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
        }

        [ScriptMethod(name: "P2_Light_Rampant_Sphere_Explosion_Timer_Drawing", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40219)$"],
            userControl: true)]
        public void P2_Light_Rampant_Sphere_Explosion_Timer_Drawing(Event @ev, ScriptAccessory sa)
        {
            if (!ParseObjectId(@ev["SourceId"], out var sid)) return;
            ScriptColor ColorRed = new ScriptColor { V4 = new Vector4(1.0f, 0f, 0f, 1.0f) };
            var dp = sa.Data.GetDefaultDrawProperties();
            dp.Name = $"Sphere_{sid}";
            dp.Scale = new(11f);
            dp.Owner = sid;
            dp.Color = Phase2_Colour_Of_Sphere_AOEs.V4.WithW(3f);
            dp.Delay = 2500;
            dp.DestoryAt = 2500;
            dp.ScaleMode |= ScaleMode.ByTime;
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        #endregion

        #region Phase_2_Intermission

        [ScriptMethod(name: "----- Phase 2.5 -----",
            eventType: EventTypeEnum.NpcYell,
            eventCondition: ["Your huddled masses yearning to breathe free"])]
        public void Phase2point5_Placeholder(Event @event, ScriptAccessory accessory) { }

        [ScriptMethod(name: "P2.5_Dark_Crystal_AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40262"])]
        public void P2_Dark_Crystal_AOE(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2.5_Dark_Crystal_AOE";
            dp.Scale = new(50);
            dp.Radian = float.Pi / 9;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
        }

        #endregion

        #region Phase_3

        [ScriptMethod(name: "----- Phase 3 -----",
            eventType: EventTypeEnum.NpcYell,
            eventCondition: ["The wretched refuse of your teeming shore"])]
        public void Phase3_Placeholder(Event @event, ScriptAccessory accessory) { }

        [ScriptMethod(name: "P3_Ultimate_Relativity_Phase_Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40266)$"], userControl: false)]
        public void P3_Ultimate_Relativity_Phase_Transition(Event @event, ScriptAccessory accessory)
        {
            parse = 31;
            phase3BossId = @event["SourceId"];
            p3FireBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            p3WaterBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            p3ReturnBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            p3Lamp = [0, 0, 0, 0, 0, 0, 0, 0];
            p3LampDirection = [0, 0, 0, 0, 0, 0, 0, 0];
        }

        [ScriptMethod(name: "P3_Ultimate_Relativity_Buff_Recording", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(2455|2456|2464|2462|2461|2460)$"], userControl: false)]
        public void P3_Ultimate_Relativity_Buff_Recording(Event @event, ScriptAccessory accessory)
        {
            if (parse != 31) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (!float.TryParse(@event["Duration"], out var dur)) return;
            var index = accessory.Data.PartyList.IndexOf(((uint)tid));
            if (index == -1) return;
            if (@event["StatusID"] == "2462")
            {
                lock (p3FireBuff) { p3FireBuff[index] = 4; }
            }
            if (@event["StatusID"] == "2455")
            {
                var count = 1;
                if (dur > 20) count = 2;
                if (dur > 30) count = 3;
                lock (p3FireBuff) { p3FireBuff[index] = count; }
            }
            if (@event["StatusID"] == "2464")
            {
                var count = 1;
                if (dur > 20) count = 3;
                lock (p3ReturnBuff) { p3ReturnBuff[index] = count; }
            }
            if (@event["StatusID"] == "2461")
            {
                lock (p3WaterBuff) { p3WaterBuff[index] = 1; }
            }
            if (@event["StatusID"] == "2460")
            {
                lock (p3WaterBuff) { p3WaterBuff[index] = 2; }
            }
            if (@event["StatusID"] == "2456")
            {
                lock (p3WaterBuff) { p3WaterBuff[index] = 3; }
            }
        }

        [ScriptMethod(name: "P3_Ultimate_Relativity_Lamp_Recording", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(0085|0086)$"], userControl: false)]
        public void P3_Ultimate_Relativity_Lamp_Recording(Event @event, ScriptAccessory accessory)
        {
            if (parse != 31) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var dir8 = PositionTo8Dir(pos, new(100, 0, 100));
            lock (p3Lamp) { p3Lamp[dir8] = @event["Id"] == "0086" ? 1 : 2; }
        }

        [ScriptMethod(name: "P3_Ultimate_Relativity_Lamp_Clockwise_Recording", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970"], userControl: false)]
        public void P3_Ultimate_Relativity_Lamp_Clockwise_Recording(Event @event, ScriptAccessory accessory)
        {
            if (parse != 31) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
            Vector3 centre = new(100, 0, 100);
            var dir8 = PositionTo8Dir(pos, centre);
            p3LampDirection[dir8] = @event["StackCount"] == "92" ? 1 : 0;
        }

        [ScriptMethod(name: "P3_Ultimate_Relativity_Lamp_AOE", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40235", "TargetIndex:1"])]
        public void P3_Ultimate_Relativity_Lamp_AOE(Event @event, ScriptAccessory accessory)
        {
            if (parse != 31) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var rot = JsonConvert.DeserializeObject<float>(@event["SourceRotation"]);
            Vector3 centre = new(100, 0, 100);
            var dir8 = PositionTo8Dir(pos, centre);
            var isWise = p3LampDirection[dir8] == 1;
            for (int i = 0; i < 9; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Lamp_AOE";
                dp.Scale = new(5, 50);
                dp.Position = pos;
                dp.Rotation = rot + (i + 1) * float.Pi / 12 * (isWise ? -1 : 1);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 2000 + (i * 1000);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }

        [ScriptMethod(name: "P3_Ultimate_Relativity_Buff_Process_Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40293"])]
        public void P3_Ultimate_Relativity_Buff_Process_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 31) return;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myIndex == -1) return;
            var myDir8 = MyLampIndex(myIndex);
            if (myDir8 == -1) return;
            var myRot = myDir8 * float.Pi / 4;

            Vector3 centre = new(100, 0, 100);
            Vector3 fireN = new(100, 0, 84.5f);
            Vector3 returnPosN = p3WaterBuff[myIndex] == 2 ? new(100, 0, 91.5f) : new(100, 0, 98);
            Vector3 stopPos = new(100, 0, 101);
            var myFire = p3FireBuff[myIndex];
            if (myFire == 1)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Short_Fire_Place_Fire";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(fireN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Short_Fire_Place_Return";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 7500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Short_Fire_Center_Stack";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 12500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Short_Fire_Output_Position";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 22500;
                dp.DestoryAt = 15000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            if (myFire == 2)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Medium_Fire_Center_Stack";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Medium_Fire_Place_Return";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 7500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Medium_Fire_Place_Fire";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(fireN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 12500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Medium_Fire_Center";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 17500;
                dp.DestoryAt = 10000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Medium_Fire_Output_Position";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 32500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            if (myFire == 3)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Long_Fire_Center_Stack";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Long_Fire_Center_Stack2";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 12500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Long_Fire_Return";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 17500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Long_Fire_Place_Fire";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(fireN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 22500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Long_Fire_Output";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 27500;
                dp.DestoryAt = 10000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            if (myFire == 4)
            {
                if (myIndex < 4)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_Ultimate_Relativity_Ice_TH_Place_Ice";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_Ultimate_Relativity_Ice_TH_Place_Return";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 7500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_Ultimate_Relativity_Ice_TH_Center_Stack";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 12500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_Ultimate_Relativity_Ice_TH_Output_Position";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 22500;
                    dp.DestoryAt = 15000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                else
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_Ultimate_Relativity_Ice_D_Center_Stack";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_Ultimate_Relativity_Ice_D_Center_Stack2";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 12500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_Ultimate_Relativity_Ice_D_Return";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 17500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_Ultimate_Relativity_Ice_D_Place_Ice";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 22500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_Ultimate_Relativity_Long_Fire_Output";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 27500;
                    dp.DestoryAt = 10000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }
        }

        [ScriptMethod(name: "P3_Ultimate_Relativity_Lamp_Process_Position", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970"])]
        public void P3_Ultimate_Relativity_Lamp_Process_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 31) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
            Vector3 centre = new(100, 0, 100);
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var dir8 = PositionTo8Dir(pos, centre);
            Vector3 nPos = @event["StackCount"] == "92" ? new(98, 0, 90) : new(102, 0, 90);
            if (dir8 == MyLampIndex(myIndex))
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Ultimate_Relativity_Lamp_Process_Position";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(nPos, centre, dir8 * float.Pi / 4);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }

        [ScriptMethod(name: "Phase3 Prompt Before Shell Crusher",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40286"])]
        public void Phase3_Prompt_Before_Shell_Crusher(Event @event, ScriptAccessory accessory)
        {
            if (parse != 31) return;
            if (Enable_Text_Prompts)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    accessory.Method.TextInfo("Stack in the center", 3000);
                }
            }
            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    accessory.TTS("Stack in the center", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
                }
            }
        }

        [ScriptMethod(name: "P3_Ultimate_Relativity_Dark_Halo", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40290"])]
        public void P3_Ultimate_Relativity_Dark_Halo(Event @event, ScriptAccessory accessory)
        {
            if (parse != 31) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Ultimate_Relativity_Dark_Halo";
            dp.Scale = new(20);
            dp.Owner = sid;
            dp.TargetObject = tid;
            dp.Color = myindex == 0 || myindex == 1 ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Phase3 Initial Orientation Before The Second Half",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40290"])]
        public void Phase3_Initial_Orientation_Before_The_Second_Half(Event @event, ScriptAccessory accessory)
        {
            if (parse != 31) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;
            if (!accessory.Data.EnmityList.TryGetValue(sourceId, out var enmityListOfBoss)) return;

            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e accessory.Data.Me={accessory.Data.Me} enmityListOfTheBoss[0]={enmityListOfBoss[0]}");
            }

            if (accessory.Data.Me != enmityListOfBoss[0]) return;

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase3_Initial_Orientation_Before_The_Second_Half";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = new Vector3(100, 0, 94);
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 12500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            if (Enable_Text_Prompts)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    accessory.Method.TextInfo("Make the Boss orient to the north", 12500);
                }
            }
            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English)
                {
                    accessory.TTS("Make the Boss orient to the north", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
                }
            }
        }

        [ScriptMethod(name: "P3_Delayed_Chant_Resonance_Phase_Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40269)$"], userControl: false)]
        public void P3_Delayed_Chant_Resonance_Phase_Transition(Event @event, ScriptAccessory accessory)
        {
            parse = 32;
            p3ApocalypseDirection = -1;
            phase3DarkWaterIIITypes = [
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE
            ];
            phase3PlayerMarks = [
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1
            ];
            phase3DarkWaterIIIProcessedCount = 0;
            phase3MarkRecordedCount = 0;
            phase3MarksRecordedSemaphore = new System.Threading.AutoResetEvent(false);
            phase3DarkWaterIIIRound = 0;
            phase3DarkWaterIIIRangeSemaphore = 0;
            phase3DarkWaterIIIGuidanceSemaphore = 0;
            phase3InitialSafePositionsConfirmed = false;
            phase3DoubleGroupLeftInitialSafePosition = new Vector3(100, 0, 100);
            phase3DoubleGroupRightInitialSafePosition = new Vector3(100, 0, 100);
            phase3DoubleGroupLeftSecondRoundStackPosition = new Vector3(100, 0, 100);
            phase3DoubleGroupRightSecondRoundStackPosition = new Vector3(100, 0, 100);
            phase3LocomotiveLeftInitialSafePosition = new Vector3(100, 0, 100);
            phase3LocomotiveRightInitialSafePosition = new Vector3(100, 0, 100);
            phase3LocomotiveLeftSecondRoundStackPosition = new Vector3(100, 0, 100);
            phase3LocomotiveRightSecondRoundStackPosition = new Vector3(100, 0, 100);
            phase3MoglinMeowLeftInitialSafePosition = new Vector3(100, 0, 100);
            phase3MoglinMeowRightInitialSafePosition = new Vector3(100, 0, 100);
            phase3MoglinMeowLeftSecondRoundStackPosition = new Vector3(100, 0, 100);
            phase3MoglinMeowRightSecondRoundStackPosition = new Vector3(100, 0, 100);
            phase3FinalBossPosition = new Vector3(100, 0, 100);
        }

        [ScriptMethod(name: "Phase3 Record Signs On Party Members",
            eventType: EventTypeEnum.Marker,
            userControl: false)]
        public void Phase3_Record_Signs_On_Party_Members(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            if (!ParseObjectId(@event["TargetId"], out var targetId)) return;
            if (!int.TryParse(@event["Id"], out var sign)) return;

            MarkType currentType = sign switch
            {
                1 => MarkType.Attack1,
                2 => MarkType.Attack2,
                3 => MarkType.Attack3,
                4 => MarkType.Attack4,
                6 => MarkType.Bind1,
                7 => MarkType.Bind2,
                8 => MarkType.Bind3,
                11 => MarkType.Square,
                _ => MarkType.Stop1
            };

            int currentIndex = accessory.Data.PartyList.IndexOf(((uint)targetId));
            if (0 <= currentIndex && currentIndex <= 7)
            {
                lock (phase3PlayerMarks)
                {
                    phase3PlayerMarks[currentIndex] = currentType;
                    ++phase3MarkRecordedCount;
                    System.Threading.Thread.MemoryBarrier();
                    if (phase3MarkRecordedCount == 8)
                    {
                        phase3MarksRecordedSemaphore.Set();
                    }
                }
            }
        }

        [ScriptMethod(name: "Phase3 Determine Types Of Dark Water III",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:2461"],
            userControl: false)]
        public void Phase3_Determine_Types_Of_Dark_Water_III(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            if (!ParseObjectId(@event["TargetId"], out var targetId)) return;
            int currentIndex = accessory.Data.PartyList.IndexOf(((uint)targetId));
            int duration = Convert.ToInt32(@event["DurationMilliseconds"], 10);
            if (currentIndex < 0 || currentIndex > 7) return;

            if (duration > 36000)
            {
                lock (phase3DarkWaterIIITypes) { phase3DarkWaterIIITypes[currentIndex] = Phase3_Types_Of_Dark_Water_III.LONG; }
            }
            else
            {
                if (duration > 27000)
                {
                    lock (phase3DarkWaterIIITypes) { phase3DarkWaterIIITypes[currentIndex] = Phase3_Types_Of_Dark_Water_III.MEDIUM; }
                }
                else
                {
                    if (duration > 8000)
                    {
                        lock (phase3DarkWaterIIITypes) { phase3DarkWaterIIITypes[currentIndex] = Phase3_Types_Of_Dark_Water_III.SHORT; }
                    }
                }
            }

            System.Threading.Thread.MemoryBarrier();
            ++phase3DarkWaterIIIProcessedCount;
            System.Threading.Thread.MemoryBarrier();

            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e currentIndex={currentIndex} duration={duration} phase3DarkWaterIIITypes={phase3DarkWaterIIITypes[currentIndex]}");
            }
        }

        [ScriptMethod(name: "Phase3 Prompt Before Dark Water III",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:2461"],
            suppress: 2000)]
        public void Phase3_Prompt_Before_Dark_Water_III(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            while (phase3DarkWaterIIIProcessedCount < 6) { System.Threading.Thread.Sleep(1); }
            System.Threading.Thread.MemoryBarrier();

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Double_Group)
            {
                bool goLeft = phase3_doubleGroup_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                bool stayInTheGroup = phase3_doubleGroup_shouldStayInTheGroup(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                string prompt = "";
                var currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase3_Prompt_Before_Dark_Water_III";
                currentProperty.Scale = new(2);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 100)) : (new Vector3(107, 0, 100));
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                if (goLeft)
                {
                    if (Language_Of_Prompts == Languages_Of_Prompts.English) { prompt += "Go left for the first and third, "; }
                }
                else
                {
                    if (Language_Of_Prompts == Languages_Of_Prompts.English) { prompt += "Go right for the first and third, "; }
                }
                if (stayInTheGroup)
                {
                    if (Language_Of_Prompts == Languages_Of_Prompts.English) { prompt += "stay in the current group for the second"; }
                }
                else
                {
                    if (Language_Of_Prompts == Languages_Of_Prompts.English) { prompt += "move to the opposite group for the second"; }
                }

                if (Enable_Text_Prompts) accessory.Method.TextInfo(prompt, 4000);
                accessory.TTS($"{prompt}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.High_Priority_As_Locomotives)
            {
                int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                bool goLeft = phase3_locomotive_shouldGoLeft(myIndex);
                string prompt = "";
                var currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase3_Prompt_Before_Dark_Water_III";
                currentProperty.Scale = new(2);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 100)) : (new Vector3(107, 0, 100));
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                if (goLeft)
                {
                    if (Language_Of_Prompts == Languages_Of_Prompts.English) { prompt += "Go left to stack, "; }
                }
                else
                {
                    if (Language_Of_Prompts == Languages_Of_Prompts.English) { prompt += "Go right to stack, "; }
                }

                if (Phase3_Branch_Of_The_Locomotive_Strat == Phase3_Branches_Of_The_Locomotive_Strat.MT_And_M1_As_Locomotives)
                {
                    if (myIndex != 0 && myIndex != 4)
                    {
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) { prompt += (goLeft) ? ("follow MT") : ("follow M1"); }
                    }
                    if (myIndex == 0 || myIndex == 4)
                    {
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) { prompt += "you are the locomotive"; }
                    }
                }

                if (Phase3_Branch_Of_The_Locomotive_Strat == Phase3_Branches_Of_The_Locomotive_Strat.Others_As_Locomotives)
                {
                    if (myIndex != 0 && myIndex != 4)
                    {
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) { prompt += "you are one of the locomotives"; }
                    }
                    if (myIndex == 0 || myIndex == 4)
                    {
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) { prompt += "follow others in the group"; }
                    }
                }

                if (Enable_Text_Prompts) accessory.Method.TextInfo(prompt, 3500);
                accessory.TTS($"{prompt}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs)
            {
                System.Threading.Thread.MemoryBarrier();
                phase3MarksRecordedSemaphore.WaitOne();
                System.Threading.Thread.MemoryBarrier();

                int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                bool goLeft = phase3_moglinMeow_shouldGoLeft(myIndex);
                string prompt = "";
                var currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase3_Prompt_Before_Dark_Water_III";
                currentProperty.Scale = new(2);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 100)) : (new Vector3(107, 0, 100));
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                if (goLeft)
                {
                    if (Language_Of_Prompts == Languages_Of_Prompts.English) { prompt += "Always stack on the left"; }
                }
                else
                {
                    if (Language_Of_Prompts == Languages_Of_Prompts.English) { prompt += "Always stack on the right"; }
                }

                if (Enable_Text_Prompts) accessory.Method.TextInfo(prompt, 3500);
                accessory.TTS($"{prompt}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        [ScriptMethod(name: "Phase3 Release The Semaphore Of Dark Water III",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:2458"],
            suppress: 2000,
            userControl: false)]
        public void Phase3_Release_The_Semaphore_Of_Dark_Water_III(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            if (@event["SourceId"].Equals("00000000")) return;
            System.Threading.Thread.MemoryBarrier();
            ++phase3DarkWaterIIIRound;
            System.Threading.Thread.MemoryBarrier();
            phase3DarkWaterIIIRangeSemaphore = 1;
            phase3DarkWaterIIIGuidanceSemaphore = 1;
        }

        [ScriptMethod(name: "Phase3 Range Of Dark Water III",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:2458"],
            suppress: 2000)]
        public void Phase3_Range_Of_Dark_Water_III(Event @event, ScriptAccessory accessory)
        {
            if (@event["SourceId"].Equals("00000000")) return;
            System.Threading.Thread.MemoryBarrier();
            while (System.Threading.Interlocked.CompareExchange(ref phase3DarkWaterIIIRangeSemaphore, 0, 1) == 0)
            {
                System.Threading.Thread.Sleep(1);
            }
            System.Threading.Thread.MemoryBarrier();

            Phase3_Types_Of_Dark_Water_III currentType = Phase3_Types_Of_Dark_Water_III.NONE;
            switch (phase3DarkWaterIIIRound)
            {
                case 1: currentType = Phase3_Types_Of_Dark_Water_III.SHORT; break;
                case 2: currentType = Phase3_Types_Of_Dark_Water_III.MEDIUM; break;
                case 3: currentType = Phase3_Types_Of_Dark_Water_III.LONG; break;
                default: return;
            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Double_Group)
            {
                if (phase3DarkWaterIIIProcessedCount == 6)
                {
                    int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                    bool goLeft = phase3_doubleGroup_shouldGoLeft(myIndex);
                    bool stayInTheGroup = phase3_doubleGroup_shouldStayInTheGroup(myIndex);

                    for (int i = 0; i < 8; ++i)
                    {
                        if (phase3DarkWaterIIITypes[i] == currentType)
                        {
                            currentProperty = accessory.Data.GetDefaultDrawProperties();
                            currentProperty.Name = "Phase3_Range_Of_Dark_Water_III";
                            currentProperty.Scale = new(6);
                            currentProperty.Owner = accessory.Data.PartyList[i];
                            currentProperty.DestoryAt = 5000;

                            if (phase3DarkWaterIIIRound == 1 || phase3DarkWaterIIIRound == 3)
                            {
                                currentProperty.Color = (phase3_doubleGroup_shouldGoLeft(i) == goLeft) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                            }
                            if (phase3DarkWaterIIIRound == 2)
                            {
                                bool endUpWithTheLeftGroup = true;
                                int doubleGroupIndexOfMyMedium = 0;
                                if (0 <= myIndex && myIndex <= 3) endUpWithTheLeftGroup = true;
                                if (4 <= myIndex && myIndex <= 7) endUpWithTheLeftGroup = false;
                                if (!stayInTheGroup) endUpWithTheLeftGroup = (!endUpWithTheLeftGroup);

                                if (endUpWithTheLeftGroup)
                                {
                                    for (doubleGroupIndexOfMyMedium = 0; phase3DarkWaterIIITypes[phase3DoubleGroupPriority[doubleGroupIndexOfMyMedium]] != Phase3_Types_Of_Dark_Water_III.MEDIUM && doubleGroupIndexOfMyMedium < 8; ++doubleGroupIndexOfMyMedium) ;
                                }
                                else
                                {
                                    for (doubleGroupIndexOfMyMedium = 7; phase3DarkWaterIIITypes[phase3DoubleGroupPriority[doubleGroupIndexOfMyMedium]] != Phase3_Types_Of_Dark_Water_III.MEDIUM && doubleGroupIndexOfMyMedium >= 0; --doubleGroupIndexOfMyMedium) ;
                                }

                                if (doubleGroupIndexOfMyMedium < 0 || doubleGroupIndexOfMyMedium > 7)
                                {
                                    currentProperty.Color = accessory.Data.DefaultDangerColor;
                                }
                                else
                                {
                                    currentProperty.Color = (phase3DoubleGroupPriority[doubleGroupIndexOfMyMedium] == i) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                                }
                            }
                            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);
                        }
                    }
                    return;
                }
            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.High_Priority_As_Locomotives)
            {
                if (phase3DarkWaterIIIProcessedCount == 6)
                {
                    bool goLeft = phase3_locomotive_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                    for (int i = 0; i < 8; ++i)
                    {
                        if (phase3DarkWaterIIITypes[i] == currentType)
                        {
                            currentProperty = accessory.Data.GetDefaultDrawProperties();
                            currentProperty.Name = "Phase3_Range_Of_Dark_Water_III";
                            currentProperty.Scale = new(6);
                            currentProperty.Owner = accessory.Data.PartyList[i];
                            currentProperty.DestoryAt = 5000;
                            currentProperty.Color = (phase3_locomotive_shouldGoLeft(i) == goLeft) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);
                        }
                    }
                    return;
                }
            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs)
            {
                if (phase3MarkRecordedCount >= 8)
                {
                    bool goLeft = phase3_moglinMeow_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                    for (int i = 0; i < 8; ++i)
                    {
                        if (phase3DarkWaterIIITypes[i] == currentType)
                        {
                            currentProperty = accessory.Data.GetDefaultDrawProperties();
                            currentProperty.Name = "Phase3_Range_Of_Dark_Water_III";
                            currentProperty.Scale = new(6);
                            currentProperty.Owner = accessory.Data.PartyList[i];
                            currentProperty.DestoryAt = 5000;
                            currentProperty.Color = (phase3_moglinMeow_shouldGoLeft(i) == goLeft) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);
                        }
                    }
                    return;
                }
            }

            for (int i = 0; i < 8; ++i)
            {
                if (phase3DarkWaterIIITypes[i] == currentType)
                {
                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase3_Range_Of_Dark_Water_III";
                    currentProperty.Scale = new(6);
                    currentProperty.Owner = accessory.Data.PartyList[i];
                    currentProperty.Color = accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);
                }
            }

            if (Enable_Text_Prompts)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.Method.TextInfo("Stack", 2000);
            }
            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.TTS("Stack", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        [ScriptMethod(name: "Phase3 Guidance Of Dark Water III",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:2458"],
            suppress: 2000)]
        public void Phase3_Guidance_Of_Dark_Water_III(Event @event, ScriptAccessory accessory)
        {
            if (@event["SourceId"].Equals("00000000")) return;
            System.Threading.Thread.MemoryBarrier();
            while (System.Threading.Interlocked.CompareExchange(ref phase3DarkWaterIIIGuidanceSemaphore, 0, 1) == 0)
            {
                System.Threading.Thread.Sleep(1);
            }
            System.Threading.Thread.MemoryBarrier();

            if (phase3DarkWaterIIIProcessedCount != 6) return;

            bool targetPositionConfirmed = false;
            string prompt = "";
            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase3_Guidance_Of_Dark_Water_III";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 5000;

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Double_Group)
            {
                bool goLeft = phase3_doubleGroup_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                bool stayInTheGroup = phase3_doubleGroup_shouldStayInTheGroup(accessory.Data.PartyList.IndexOf(accessory.Data.Me));

                if (Enable_Developer_Mode)
                {
                    accessory.Method.SendChat($"/e goLeft={goLeft} stayInTheGroup={stayInTheGroup} phase3DarkWaterIIIRound={phase3DarkWaterIIIRound}");
                }

                switch (phase3DarkWaterIIIRound)
                {
                    case 1:
                        currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 100)) : (new Vector3(107, 0, 100));
                        targetPositionConfirmed = true;
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) prompt = (goLeft) ? ("Stack on the left") : ("Stack on the right");
                        break;
                    case 2:
                        if (stayInTheGroup)
                        {
                            if (0 <= accessory.Data.PartyList.IndexOf(accessory.Data.Me) && accessory.Data.PartyList.IndexOf(accessory.Data.Me) <= 3)
                            {
                                currentProperty.TargetPosition = phase3DoubleGroupLeftSecondRoundStackPosition;
                                targetPositionConfirmed = true;
                            }
                            if (4 <= accessory.Data.PartyList.IndexOf(accessory.Data.Me) && accessory.Data.PartyList.IndexOf(accessory.Data.Me) <= 7)
                            {
                                currentProperty.TargetPosition = phase3DoubleGroupRightSecondRoundStackPosition;
                                targetPositionConfirmed = true;
                            }
                        }
                        else
                        {
                            if (0 <= accessory.Data.PartyList.IndexOf(accessory.Data.Me) && accessory.Data.PartyList.IndexOf(accessory.Data.Me) <= 3)
                            {
                                currentProperty.TargetPosition = phase3DoubleGroupRightSecondRoundStackPosition;
                                targetPositionConfirmed = true;
                            }
                            if (4 <= accessory.Data.PartyList.IndexOf(accessory.Data.Me) && accessory.Data.PartyList.IndexOf(accessory.Data.Me) <= 7)
                            {
                                currentProperty.TargetPosition = phase3DoubleGroupLeftSecondRoundStackPosition;
                                targetPositionConfirmed = true;
                            }
                        }
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) prompt = (stayInTheGroup) ? ("Stack in the current group") : ("Stack in the opposite group");
                        break;
                    case 3:
                        if (ParseObjectId(phase3BossId, out var bossId))
                        {
                            var bossObject = accessory.Data.Objects.SearchById(bossId);
                            if (bossObject != null)
                            {
                                float currentRotation = bossObject.Rotation;
                                currentRotation = -(currentRotation - float.Pi);
                                Vector3 groupPosition = new Vector3(100, 0, 100);
                                if (Enable_Developer_Mode) accessory.Method.SendChat($"/e currentRotation={currentRotation}");
                                if (goLeft) groupPosition = new Vector3(bossObject.Position.X - 6.89f, bossObject.Position.Y, bossObject.Position.Z + 6.89f);
                                else groupPosition = new Vector3(bossObject.Position.X + 6.89f, bossObject.Position.Y, bossObject.Position.Z + 6.89f);
                                groupPosition = RotatePoint(groupPosition, bossObject.Position, currentRotation);
                                currentProperty.TargetPosition = groupPosition;
                                targetPositionConfirmed = true;
                            }
                        }
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) prompt = (goLeft) ? ("Stack on the left") : ("Stack on the right");
                        break;
                }
            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.High_Priority_As_Locomotives)
            {
                bool goLeft = phase3_locomotive_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                if (Enable_Developer_Mode)
                {
                    accessory.Method.SendChat($"/e goLeft={goLeft} phase3DarkWaterIIIRound={phase3DarkWaterIIIRound}");
                }
                switch (phase3DarkWaterIIIRound)
                {
                    case 1:
                        currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 100)) : (new Vector3(107, 0, 100));
                        targetPositionConfirmed = true;
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) prompt = (goLeft) ? ("Stack on the left") : ("Stack on the right");
                        break;
                    case 2:
                        currentProperty.TargetPosition = (goLeft) ? phase3LocomotiveLeftSecondRoundStackPosition : phase3LocomotiveRightSecondRoundStackPosition;
                        targetPositionConfirmed = true;
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) prompt = "Stack on this side of the center";
                        break;
                    case 3:
                        if (ParseObjectId(phase3BossId, out var bossId))
                        {
                            var bossObject = accessory.Data.Objects.SearchById(bossId);
                            if (bossObject != null)
                            {
                                float currentRotation = bossObject.Rotation;
                                currentRotation = -(currentRotation - float.Pi);
                                Vector3 groupPosition = new Vector3(100, 0, 100);
                                if (Enable_Developer_Mode) accessory.Method.SendChat($"/e currentRotation={currentRotation}");
                                if (goLeft) groupPosition = new Vector3(bossObject.Position.X - 6.89f, bossObject.Position.Y, bossObject.Position.Z + 6.89f);
                                else groupPosition = new Vector3(bossObject.Position.X + 6.89f, bossObject.Position.Y, bossObject.Position.Z + 6.89f);
                                groupPosition = RotatePoint(groupPosition, bossObject.Position, currentRotation);
                                currentProperty.TargetPosition = groupPosition;
                                targetPositionConfirmed = true;
                            }
                        }
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) prompt = (goLeft) ? ("Stack on the left") : ("Stack on the right");
                        break;
                }
            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs)
            {
                bool goLeft = phase3_moglinMeow_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                if (Enable_Developer_Mode)
                {
                    accessory.Method.SendChat($"/e goLeft={goLeft} phase3DarkWaterIIIRound={phase3DarkWaterIIIRound}");
                }
                switch (phase3DarkWaterIIIRound)
                {
                    case 1:
                        currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 100)) : (new Vector3(107, 0, 100));
                        targetPositionConfirmed = true;
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) prompt = (goLeft) ? ("Stack on the left") : ("Stack on the right");
                        break;
                    case 2:
                        currentProperty.TargetPosition = (goLeft) ? phase3MoglinMeowLeftSecondRoundStackPosition : phase3MoglinMeowRightSecondRoundStackPosition;
                        targetPositionConfirmed = true;
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) prompt = "Stack on this side of the center";
                        break;
                    case 3:
                        if (ParseObjectId(phase3BossId, out var bossId))
                        {
                            var bossObject = accessory.Data.Objects.SearchById(bossId);
                            if (bossObject != null)
                            {
                                float currentRotation = bossObject.Rotation;
                                currentRotation = -(currentRotation - float.Pi);
                                Vector3 groupPosition = new Vector3(100, 0, 100);
                                if (Enable_Developer_Mode) accessory.Method.SendChat($"/e currentRotation={currentRotation}");
                                if (goLeft) groupPosition = new Vector3(bossObject.Position.X - 6.89f, bossObject.Position.Y, bossObject.Position.Z + 6.89f);
                                else groupPosition = new Vector3(bossObject.Position.X + 6.89f, bossObject.Position.Y, bossObject.Position.Z + 6.89f);
                                groupPosition = RotatePoint(groupPosition, bossObject.Position, currentRotation);
                                currentProperty.TargetPosition = groupPosition;
                                targetPositionConfirmed = true;
                            }
                        }
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) prompt = (goLeft) ? ("Stack on the left") : ("Stack on the right");
                        break;
                }
            }

            if (targetPositionConfirmed) accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
            if (!prompt.Equals(""))
            {
                if (Enable_Text_Prompts) accessory.Method.TextInfo(prompt, 2500);
                accessory.TTS($"{prompt}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        private bool phase3_doubleGroup_shouldStayInTheGroup(int currentIndex)
        {
            bool inTheLeftGroup = (0 <= currentIndex && currentIndex <= 3);
            return (inTheLeftGroup == phase3_doubleGroup_shouldGoLeft(currentIndex));
        }

        private bool phase3_doubleGroup_shouldGoLeft(int currentIndex)
        {
            if (currentIndex < 0 || currentIndex > 7) return true;
            int doubleGroupIndex = phase3_doubleGroup_getDoubleGroupIndex(currentIndex);
            Phase3_Types_Of_Dark_Water_III currentType = phase3DarkWaterIIITypes[currentIndex];
            bool goLeft = true;
            for (int i = 0; i < 8; ++i)
            {
                if (phase3DarkWaterIIITypes[phase3DoubleGroupPriority[i]] == currentType && i != doubleGroupIndex)
                {
                    if (i > doubleGroupIndex) { goLeft = true; break; }
                    if (i < doubleGroupIndex) { goLeft = false; break; }
                }
            }
            return goLeft;
        }

        private int phase3_doubleGroup_getDoubleGroupIndex(int currentIndex)
        {
            for (int i = 0; i < 8; ++i) if (currentIndex == phase3DoubleGroupPriority[i]) return i;
            return currentIndex;
        }

        private bool phase3_locomotive_shouldGoLeft(int currentIndex)
        {
            if (currentIndex < 0 || currentIndex > 7) return true;
            int locomotiveIndex = phase3_locomotive_getLocomotiveIndex(currentIndex);
            Phase3_Types_Of_Dark_Water_III currentType = phase3DarkWaterIIITypes[currentIndex];
            bool goLeft = true;
            for (int i = 0; i < 8; ++i)
            {
                if (phase3DarkWaterIIITypes[phase3LocomotivePriority[i]] == currentType && i != locomotiveIndex)
                {
                    if (i > locomotiveIndex) { goLeft = true; break; }
                    if (i < locomotiveIndex) { goLeft = false; break; }
                }
            }
            return goLeft;
        }

        private int phase3_locomotive_getLocomotiveIndex(int currentIndex)
        {
            for (int i = 0; i < 8; ++i) if (currentIndex == phase3LocomotivePriority[i]) return i;
            return currentIndex;
        }

        private bool phase3_moglinMeow_shouldGoLeft(int currentIndex)
        {
            if (currentIndex < 0 || currentIndex > 7) return true;
            if (phase3PlayerMarks[currentIndex] == MarkType.Attack1 || phase3PlayerMarks[currentIndex] == MarkType.Attack2 ||
                phase3PlayerMarks[currentIndex] == MarkType.Attack3 || phase3PlayerMarks[currentIndex] == MarkType.Attack4) return true;
            if (phase3PlayerMarks[currentIndex] == MarkType.Bind1 || phase3PlayerMarks[currentIndex] == MarkType.Bind2 ||
                phase3PlayerMarks[currentIndex] == MarkType.Bind3 || phase3PlayerMarks[currentIndex] == MarkType.Square) return false;
            return true;
        }

        [ScriptMethod(name: "Phase3 Range Of Spirit Taker",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40288"])]
        public void Phase3_Range_Of_Spirit_Taker(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            for (int i = 0; i < 8; ++i)
            {
                var currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase3_Range_Of_Spirit_Taker";
                currentProperty.Scale = new(5);
                currentProperty.Owner = accessory.Data.PartyList[i];
                currentProperty.Color = accessory.Data.DefaultDangerColor;
                currentProperty.Delay = 1250;
                currentProperty.DestoryAt = 2500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);
            }

            System.Threading.Thread.Sleep(1000);
            if (Enable_Text_Prompts)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.Method.TextInfo("Spread", 2000);
            }
            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.TTS("Spread", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        [ScriptMethod(name: "Phase3 Guidance Of Spirit Taker",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40288"])]
        public void Phase3_Guidance_Of_Spirit_Taker(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            bool targetPositionConfirmed = false;
            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase3_Guidance_Of_Spirit_Taker";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.Delay = 1250;
            currentProperty.DestoryAt = 2500;

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Double_Group)
            {
                int myDoubleGroupIndex = phase3_doubleGroup_getDoubleGroupIndex(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                switch (myDoubleGroupIndex)
                {
                    case 0: currentProperty.TargetPosition = new Vector3(85, 0, 100); targetPositionConfirmed = true; break;
                    case 1:
                        bool goLeft = phase3_doubleGroup_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                        if (Enable_Developer_Mode) accessory.Method.SendChat($"/e goLeft={goLeft}");
                        currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 92)) : (new Vector3(107, 0, 92));
                        targetPositionConfirmed = true;
                        break;
                    case 2: currentProperty.TargetPosition = new Vector3(100, 0, 92); targetPositionConfirmed = true; break;
                    case 3: currentProperty.TargetPosition = new Vector3(100, 0, 100); targetPositionConfirmed = true; break;
                    case 4:
                        goLeft = phase3_doubleGroup_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                        if (Enable_Developer_Mode) accessory.Method.SendChat($"/e goLeft={goLeft}");
                        currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 100)) : (new Vector3(107, 0, 100));
                        targetPositionConfirmed = true;
                        break;
                    case 5: currentProperty.TargetPosition = new Vector3(100, 0, 108); targetPositionConfirmed = true; break;
                    case 6:
                        goLeft = phase3_doubleGroup_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                        if (Enable_Developer_Mode) accessory.Method.SendChat($"/e goLeft={goLeft}");
                        currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 108)) : (new Vector3(107, 0, 108));
                        targetPositionConfirmed = true;
                        break;
                    case 7: currentProperty.TargetPosition = new Vector3(115, 0, 100); targetPositionConfirmed = true; break;
                }
            }
            else
            {
                var temporaryProperty = accessory.Data.GetDefaultDrawProperties();
                Vector3 point1 = new Vector3(93f, 0f, 101f);
                Vector3 point1Extension = new Vector3(93f, 0f, 109f);
                Vector3 point2 = new Vector3(93f, 0f, 99f);
                Vector3 point2Extension = new Vector3(93f, 0f, 91f);
                Vector3 point3 = new Vector3(92f, 0f, 101f);
                Vector3 point3Extension = new Vector3(85.072f, 0f, 105f);
                Vector3 point4 = new Vector3(92f, 0f, 99f);
                Vector3 point4Extension = new Vector3(85.072f, 0f, 95f);
                Vector3 point5 = new Vector3(107f, 0f, 101f);
                Vector3 point5Extension = new Vector3(107f, 0f, 109f);
                Vector3 point6 = new Vector3(107f, 0f, 99f);
                Vector3 point6Extension = new Vector3(107f, 0f, 91f);
                Vector3 point7 = new Vector3(108f, 0f, 101f);
                Vector3 point7Extension = new Vector3(114.928f, 0f, 105f);
                Vector3 point8 = new Vector3(108f, 0f, 99f);
                Vector3 point8Extension = new Vector3(114.928f, 0f, 95f);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();
                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point1;
                temporaryProperty.TargetPosition = point1Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();
                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point2;
                temporaryProperty.TargetPosition = point2Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();
                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point3;
                temporaryProperty.TargetPosition = point3Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();
                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point4;
                temporaryProperty.TargetPosition = point4Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();
                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point5;
                temporaryProperty.TargetPosition = point5Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();
                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point6;
                temporaryProperty.TargetPosition = point6Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();
                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point7;
                temporaryProperty.TargetPosition = point7Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();
                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point8;
                temporaryProperty.TargetPosition = point8Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);
            }

            if (targetPositionConfirmed) accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
        }

        [ScriptMethod(name: "Phase3 Determine Initial Safe Positions Of Apocalypse",
            eventType: EventTypeEnum.ObjectEffect,
            eventCondition: ["Id1:4", "Id2:regex:^(16|64)$"],
            userControl: false,
            suppress: 2000)]
        public void Phase3_Determine_Initial_Safe_Positions_Of_Apocalypse(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            if (phase3InitialSafePositionsConfirmed) return;

            Vector3 position1OfTheSecond = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            Vector3 position2OfTheSecond = RotatePoint(position1OfTheSecond, new Vector3(100, 0, 100), float.Pi);
            int clockwise = (@event["Id2"].Equals("64")) ? (-1) : (1);
            Vector3 position1OfTheLast = RotatePoint(position1OfTheSecond, new Vector3(100, 0, 100), float.Pi / 4 * 3 * clockwise);
            Vector3 position2OfTheLast = RotatePoint(position1OfTheSecond, new Vector3(100, 0, 100), float.Pi / 4 * 3 * clockwise + float.Pi);
            Vector3 position1OfThePenultimate = RotatePoint(position1OfTheSecond, new Vector3(100, 0, 100), float.Pi / 2 * clockwise);
            Vector3 position2OfThePenultimate = RotatePoint(position1OfTheSecond, new Vector3(100, 0, 100), float.Pi / 2 * clockwise + float.Pi);
            int direction1OfTheLast = PositionTo8Dir(position1OfTheLast, new Vector3(100, 0, 100));
            int direction1OfThePenultimate = PositionTo8Dir(position1OfThePenultimate, new Vector3(100, 0, 100));
            int direction1OfTheSecond = PositionTo8Dir(position1OfTheSecond, new Vector3(100, 0, 100));

            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e position1OfTheLast={position1OfTheLast} position2OfTheLast={position2OfTheLast} clockwise={clockwise} position1OfThePenultimate={position1OfThePenultimate} position2OfThePenultimate={position2OfThePenultimate} position1OfTheSecond={position1OfTheSecond} position2OfTheSecond={position2OfTheSecond} direction1OfTheLast={direction1OfTheLast} direction1OfThePenultimate={direction1OfThePenultimate} direction1OfTheSecond={direction1OfTheSecond}");
            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Double_Group)
            {
                if (Phase3_Branch_Of_The_Double_Group_Strat == Phase3_Branches_Of_The_Double_Group_Strat.Based_On_Safe_Positions)
                {
                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.North_To_Southwest_For_The_Left_Group)
                    {
                        if (direction1OfTheLast == 0 || direction1OfTheLast == 7 || direction1OfTheLast == 6 || direction1OfTheLast == 5)
                        {
                            phase3DoubleGroupLeftInitialSafePosition = position1OfTheLast;
                            phase3DoubleGroupLeftSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3DoubleGroupRightInitialSafePosition = position2OfTheLast;
                            phase3DoubleGroupRightSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                        if (direction1OfTheLast == 1 || direction1OfTheLast == 2 || direction1OfTheLast == 3 || direction1OfTheLast == 4)
                        {
                            phase3DoubleGroupLeftInitialSafePosition = position2OfTheLast;
                            phase3DoubleGroupLeftSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3DoubleGroupRightInitialSafePosition = position1OfTheLast;
                            phase3DoubleGroupRightSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                    }
                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.Northwest_To_South_For_The_Left_Group)
                    {
                        if (direction1OfTheLast == 7 || direction1OfTheLast == 6 || direction1OfTheLast == 5 || direction1OfTheLast == 4)
                        {
                            phase3DoubleGroupLeftInitialSafePosition = position1OfTheLast;
                            phase3DoubleGroupLeftSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3DoubleGroupRightInitialSafePosition = position2OfTheLast;
                            phase3DoubleGroupRightSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                        if (direction1OfTheLast == 0 || direction1OfTheLast == 1 || direction1OfTheLast == 2 || direction1OfTheLast == 3)
                        {
                            phase3DoubleGroupLeftInitialSafePosition = position2OfTheLast;
                            phase3DoubleGroupLeftSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3DoubleGroupRightInitialSafePosition = position1OfTheLast;
                            phase3DoubleGroupRightSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                    }
                }
                if (Phase3_Branch_Of_The_Double_Group_Strat == Phase3_Branches_Of_The_Double_Group_Strat.Based_On_The_Second_Apocalypse)
                {
                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.North_To_Southwest_For_The_Left_Group)
                    {
                        if (direction1OfTheSecond == 0 || direction1OfTheSecond == 7 || direction1OfTheSecond == 6 || direction1OfTheSecond == 5)
                        {
                            phase3DoubleGroupLeftInitialSafePosition = position2OfTheLast;
                            phase3DoubleGroupLeftSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3DoubleGroupRightInitialSafePosition = position1OfTheLast;
                            phase3DoubleGroupRightSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                        if (direction1OfTheSecond == 1 || direction1OfTheSecond == 2 || direction1OfTheSecond == 3 || direction1OfTheSecond == 4)
                        {
                            phase3DoubleGroupLeftInitialSafePosition = position1OfTheLast;
                            phase3DoubleGroupLeftSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3DoubleGroupRightInitialSafePosition = position2OfTheLast;
                            phase3DoubleGroupRightSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                    }
                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.Northwest_To_South_For_The_Left_Group)
                    {
                        if (direction1OfTheSecond == 7 || direction1OfTheSecond == 6 || direction1OfTheSecond == 5 || direction1OfTheSecond == 4)
                        {
                            phase3DoubleGroupLeftInitialSafePosition = position2OfTheLast;
                            phase3DoubleGroupLeftSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3DoubleGroupRightInitialSafePosition = position1OfTheLast;
                            phase3DoubleGroupRightSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                        if (direction1OfTheSecond == 0 || direction1OfTheSecond == 1 || direction1OfTheSecond == 2 || direction1OfTheSecond == 3)
                        {
                            phase3DoubleGroupLeftInitialSafePosition = position1OfTheLast;
                            phase3DoubleGroupLeftSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3DoubleGroupRightInitialSafePosition = position2OfTheLast;
                            phase3DoubleGroupRightSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                    }
                }
            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.High_Priority_As_Locomotives)
            {
                if (Phase3_Branch_Of_The_Locomotive_Strat == Phase3_Branches_Of_The_Locomotive_Strat.MT_And_M1_As_Locomotives)
                {
                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.North_To_Southwest_For_The_Left_Group)
                    {
                        if (direction1OfThePenultimate == 0 || direction1OfThePenultimate == 7 || direction1OfThePenultimate == 6 || direction1OfThePenultimate == 5)
                        {
                            phase3LocomotiveLeftInitialSafePosition = position1OfThePenultimate;
                            phase3LocomotiveLeftSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3LocomotiveRightInitialSafePosition = position2OfThePenultimate;
                            phase3LocomotiveRightSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                        if (direction1OfThePenultimate == 1 || direction1OfThePenultimate == 2 || direction1OfThePenultimate == 3 || direction1OfThePenultimate == 4)
                        {
                            phase3LocomotiveLeftInitialSafePosition = position2OfThePenultimate;
                            phase3LocomotiveLeftSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3LocomotiveRightInitialSafePosition = position1OfThePenultimate;
                            phase3LocomotiveRightSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                    }
                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.Northwest_To_South_For_The_Left_Group)
                    {
                        if (direction1OfThePenultimate == 7 || direction1OfThePenultimate == 6 || direction1OfThePenultimate == 5 || direction1OfThePenultimate == 4)
                        {
                            phase3LocomotiveLeftInitialSafePosition = position1OfThePenultimate;
                            phase3LocomotiveLeftSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3LocomotiveRightInitialSafePosition = position2OfThePenultimate;
                            phase3LocomotiveRightSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                        if (direction1OfThePenultimate == 0 || direction1OfThePenultimate == 1 || direction1OfThePenultimate == 2 || direction1OfThePenultimate == 3)
                        {
                            phase3LocomotiveLeftInitialSafePosition = position2OfThePenultimate;
                            phase3LocomotiveLeftSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3LocomotiveRightInitialSafePosition = position1OfThePenultimate;
                            phase3LocomotiveRightSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                    }
                }
                if (Phase3_Branch_Of_The_Locomotive_Strat == Phase3_Branches_Of_The_Locomotive_Strat.Others_As_Locomotives)
                {
                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.North_To_Southwest_For_The_Left_Group)
                    {
                        if (direction1OfTheLast == 0 || direction1OfTheLast == 7 || direction1OfTheLast == 6 || direction1OfTheLast == 5)
                        {
                            phase3LocomotiveLeftInitialSafePosition = position1OfTheLast;
                            phase3LocomotiveLeftSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3LocomotiveRightInitialSafePosition = position2OfTheLast;
                            phase3LocomotiveRightSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                        if (direction1OfTheLast == 1 || direction1OfTheLast == 2 || direction1OfTheLast == 3 || direction1OfTheLast == 4)
                        {
                            phase3LocomotiveLeftInitialSafePosition = position2OfTheLast;
                            phase3LocomotiveLeftSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3LocomotiveRightInitialSafePosition = position1OfTheLast;
                            phase3LocomotiveRightSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                    }
                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.Northwest_To_South_For_The_Left_Group)
                    {
                        if (direction1OfTheLast == 7 || direction1OfTheLast == 6 || direction1OfTheLast == 5 || direction1OfTheLast == 4)
                        {
                            phase3LocomotiveLeftInitialSafePosition = position1OfTheLast;
                            phase3LocomotiveLeftSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3LocomotiveRightInitialSafePosition = position2OfTheLast;
                            phase3LocomotiveRightSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                        if (direction1OfTheLast == 0 || direction1OfTheLast == 1 || direction1OfTheLast == 2 || direction1OfTheLast == 3)
                        {
                            phase3LocomotiveLeftInitialSafePosition = position2OfTheLast;
                            phase3LocomotiveLeftSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3LocomotiveRightInitialSafePosition = position1OfTheLast;
                            phase3LocomotiveRightSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3InitialSafePositionsConfirmed = true;
                        }
                    }
                }
            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs)
            {
                if (direction1OfTheLast == 0 || direction1OfTheLast == 7 || direction1OfTheLast == 6 || direction1OfTheLast == 5)
                {
                    phase3MoglinMeowLeftInitialSafePosition = position1OfTheLast;
                    phase3MoglinMeowLeftSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                    phase3MoglinMeowRightInitialSafePosition = position2OfTheLast;
                    phase3MoglinMeowRightSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                    phase3InitialSafePositionsConfirmed = true;
                }
                if (direction1OfTheLast == 1 || direction1OfTheLast == 2 || direction1OfTheLast == 3 || direction1OfTheLast == 4)
                {
                    phase3MoglinMeowLeftInitialSafePosition = position2OfTheLast;
                    phase3MoglinMeowLeftSecondRoundStackPosition = new Vector3((position2OfTheLast.X - 100) / 3 + 100, position2OfTheLast.Y, (position2OfTheLast.Z - 100) / 3 + 100);
                    phase3MoglinMeowRightInitialSafePosition = position1OfTheLast;
                    phase3MoglinMeowRightSecondRoundStackPosition = new Vector3((position1OfTheLast.X - 100) / 3 + 100, position1OfTheLast.Y, (position1OfTheLast.Z - 100) / 3 + 100);
                    phase3InitialSafePositionsConfirmed = true;
                }
            }
        }

        [ScriptMethod(name: "P3_Delayed_Chant_Resonance_Apocalypse", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:4", "Id2:regex:^(16|64)$"], suppress: 2000)]
        public void P3_Delayed_Chant_Resonance_Apocalypse(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            if (p3ApocalypseDone) return;
            p3ApocalypseDone = true;
            Vector3 centre = new(100, 0, 100);
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var clockwise = @event["Id2"] == "64" ? -1 : 1;
            var preTime = 100;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_Center";
            dp.Scale = new(9);
            dp.Position = centre;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 9700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_StartPoint_11";
            dp.Scale = new(9);
            dp.Position = pos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 12000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_StartPoint_12";
            dp.Scale = new(9);
            dp.Position = pos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 17000 - preTime;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_StartPoint_21";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 12000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_StartPoint_22";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 17000 - preTime;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_SecondPoint_11";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * clockwise);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 14000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_SecondPoint_12";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * clockwise);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 19000 - preTime;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_SecondPoint_21";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * clockwise + float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 14000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_SecondPoint_22";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * clockwise + float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 19000 - preTime;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_ThirdPoint_11";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 2 * clockwise);
            dp.Color = Phase3_Colour_Of_The_Penultimate_Apocalypse.V4.WithW(1f);
            dp.Delay = 3000;
            dp.DestoryAt = 8000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_ThirdPoint_12";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 2 * clockwise);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 11000 - preTime;
            dp.DestoryAt = 8000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_ThirdPoint_21";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 2 * clockwise + float.Pi);
            dp.Color = Phase3_Colour_Of_The_Penultimate_Apocalypse.V4.WithW(1f);
            dp.Delay = 3000;
            dp.DestoryAt = 8000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_ThirdPoint_22";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 2 * clockwise + float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 11000 - preTime;
            dp.DestoryAt = 8000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_FourthPoint_11";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * 3 * clockwise);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 15000 - preTime;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Second_Half_Apocalypse_FourthPoint_21";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * 3 * clockwise + float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 15000 - preTime;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Phase3 Rough Guidance Of Initial Safe Positions",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40289"])]
        public void Phase3_Rough_Guidance_Of_Initial_Safe_Positions(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            bool targetPositionConfirmed = false;
            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase3_Rough_Guidance_Of_Initial_Safe_Positions";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
            currentProperty.Delay = 500;
            currentProperty.DestoryAt = 6500;

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Double_Group)
            {
                if (0 <= accessory.Data.PartyList.IndexOf(accessory.Data.Me) && accessory.Data.PartyList.IndexOf(accessory.Data.Me) <= 3)
                {
                    currentProperty.TargetPosition = phase3DoubleGroupLeftInitialSafePosition;
                    targetPositionConfirmed = true;
                }
                if (4 <= accessory.Data.PartyList.IndexOf(accessory.Data.Me) && accessory.Data.PartyList.IndexOf(accessory.Data.Me) <= 7)
                {
                    currentProperty.TargetPosition = phase3DoubleGroupRightInitialSafePosition;
                    targetPositionConfirmed = true;
                }
            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.High_Priority_As_Locomotives)
            {
                bool goLeft = phase3_locomotive_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                if (goLeft) currentProperty.TargetPosition = phase3LocomotiveLeftInitialSafePosition;
                else currentProperty.TargetPosition = phase3LocomotiveRightInitialSafePosition;
                targetPositionConfirmed = true;
            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs)
            {
                bool goLeft = phase3_moglinMeow_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                if (goLeft) currentProperty.TargetPosition = phase3MoglinMeowLeftInitialSafePosition;
                else currentProperty.TargetPosition = phase3MoglinMeowRightInitialSafePosition;
                targetPositionConfirmed = true;
            }

            if (targetPositionConfirmed) accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
        }

        [ScriptMethod(name: "Phase3 Range Of Darkest Dance",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40181"])]
        public void Phase3_Range_Of_Darkest_Dance(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;

            bool goBait = false;
            if (Phase3_Tank_Who_Baits_Darkest_Dance == Tanks.MT && accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 0) goBait = true;
            if (Phase3_Tank_Who_Baits_Darkest_Dance == Tanks.OT_ST && accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 1) goBait = true;

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase3_Range_Of_Darkest_Dance";
            currentProperty.Scale = new(8);
            currentProperty.Owner = sourceId;
            currentProperty.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            currentProperty.Color = Phase3_Colour_Of_Darkest_Dance.V4.WithW(3f);
            currentProperty.Delay = 2200;
            currentProperty.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

            System.Threading.Thread.Sleep(2200);
            if (goBait)
            {
                if (Enable_Text_Prompts)
                {
                    if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.Method.TextInfo("Stay away and bait", 1500);
                }
                if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                {
                    if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.TTS("Stay away and bait", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
                }
            }
            else
            {
                if (Phase3_Tank_Who_Baits_Darkest_Dance == Tanks.MT)
                {
                    if (Enable_Text_Prompts)
                    {
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.Method.TextInfo("Stay away from MT", 1500);
                    }
                    if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                    {
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.TTS("Stay away from MT", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
                    }
                }
                if (Phase3_Tank_Who_Baits_Darkest_Dance == Tanks.OT_ST)
                {
                    if (Enable_Text_Prompts)
                    {
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.Method.TextInfo("Stay away from OT", 1500);
                    }
                    if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                    {
                        if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.TTS("Stay away from OT", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
                    }
                }
            }
        }

        [ScriptMethod(name: "Phase3 Guidance Of Darkest Dance",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40181"])]
        public void Phase3_Guidance_Of_Darkest_Dance(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            var tankWhoBaitsDarkestDance = accessory.Data.Objects.SearchById(accessory.Data.PartyList[1]);
            bool goBait = false;
            if (Phase3_Tank_Who_Baits_Darkest_Dance == Tanks.MT)
            {
                tankWhoBaitsDarkestDance = accessory.Data.Objects.SearchById(accessory.Data.PartyList[0]);
                if (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 0) goBait = true;
            }
            if (Phase3_Tank_Who_Baits_Darkest_Dance == Tanks.OT_ST)
            {
                tankWhoBaitsDarkestDance = accessory.Data.Objects.SearchById(accessory.Data.PartyList[1]);
                if (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 1) goBait = true;
            }
            if (tankWhoBaitsDarkestDance == null) return;

            var dir8 = p3ApocalypseDirection % 10 % 4;
            Vector3 posN = new(100, 0, 86);
            var rot = dir8 switch { 0 => 6, 1 => 7, 2 => 0, 3 => 5 };
            var pos1 = RotatePoint(posN, new(100, 0, 100), float.Pi / 4 * rot);
            var pos2 = RotatePoint(posN, new(100, 0, 100), float.Pi / 4 * rot + float.Pi);
            var dealpos = ((pos1 - tankWhoBaitsDarkestDance.Position).Length() < (pos2 - tankWhoBaitsDarkestDance.Position).Length()) ? (pos1) : (pos2);
            Vector3 positionToBait = new Vector3((dealpos.X - 100) / 3 * 4 + 100, dealpos.Y, (dealpos.Z - 100) / 3 * 4 + 100);

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            if (goBait)
            {
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
            }
            else
            {
                if (Phase3_Tank_Who_Baits_Darkest_Dance == Tanks.MT)
                {
                    currentProperty.Owner = accessory.Data.PartyList[0];
                    currentProperty.Color = Phase3_Colour_Of_Darkest_Dance.V4.WithW(1f);
                }
                else
                {
                    currentProperty.Owner = accessory.Data.PartyList[1];
                    currentProperty.Color = Phase3_Colour_Of_Darkest_Dance.V4.WithW(1f);
                }
            }
            currentProperty.Name = "Phase3_Guidance_Of_Darkest_Dance";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.TargetPosition = positionToBait;
            currentProperty.Delay = 2200;
            currentProperty.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
        }

        [ScriptMethod(name: "P3_Delayed_Chant_Resonance_Knockback_Prompt", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40182", "TargetIndex:1"])]
        public void P3_Delayed_Chant_Resonance_Knockback_Prompt(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Delayed_Chant_Resonance_Knockback_Prompt1";
            dp.Scale = new(2, 21);
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = sid;
            dp.Rotation = float.Pi;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Delayed_Chant_Resonance_Knockback_Prompt2";
            dp.Scale = new(2);
            dp.Owner = sid;
            dp.TargetObject = accessory.Data.Me;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "P3_Delayed_Chant_Resonance_Apocalypse_Recording", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:4", "Id2:regex:^(16|64)$"], userControl: false)]
        public void P3_Delayed_Chant_Resonance_Apocalypse_Recording(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            lock (this)
            {
                if (p3ApocalypseDirection != -1) return;
                Vector3 centre = new(100, 0, 100);
                var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                p3ApocalypseDirection = PositionTo8Dir(pos, new(100, 0, 100));
                p3ApocalypseDirection += @event["Id2"] == "64" ? 10 : 20;
            }
        }

        [ScriptMethod(name: "Phase3 Determine The Final Position Of The Boss",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40300"],
            userControl: false)]
        public void Phase3_Determine_The_Final_Position_Of_The_Boss(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            phase3FinalBossPosition = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        }

        [ScriptMethod(name: "Phase3 Initial Position Of The Boss In Phase4",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40300"])]
        public void Phase3_Initial_Position_Of_The_Boss_In_Phase4(Event @event, ScriptAccessory accessory)
        {
            if (parse != 32) return;
            if (phase3FinalBossPosition.Equals(new Vector3(100, 0, 100))) return;

            bool inTheNorth = phase3FinalBossPosition.Z >= 100;
            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase3_Initial_Position_Of_The_Boss_In_Phase4";
            currentProperty.Scale = new(7);
            currentProperty.Position = (inTheNorth) ? (new Vector3(100, 0, 90)) : (new Vector3(100, 0, 110));
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 9250;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

            System.Threading.Thread.Sleep(2000);
            if (Enable_Text_Prompts)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.Method.TextInfo(((inTheNorth) ? ("The Boss will appear in the north") : ("The Boss will appear in the south")), 7250);
            }
            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.TTS($"{((inTheNorth) ? ("The Boss will appear in the north") : ("The Boss will appear in the south"))}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        private int MyLampIndex(int myPartyIndex)
        {
            var nLampIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                if (p3Lamp[i] == 1 && p3Lamp[(i + 3) % 8] == 1 && p3Lamp[(i + 5) % 8] == 1)
                {
                    nLampIndex = i;
                    break;
                }
            }
            if (Phase3_Strat_Of_The_First_Half == Phase3_Strats_Of_The_First_Half.Moogle)
            {
                if (p3FireBuff[myPartyIndex] == 1)
                {
                    if (myPartyIndex < 4) return (nLampIndex + 4) % 8;
                    else
                    {
                        var lowIndex = p3FireBuff.LastIndexOf(1);
                        if (lowIndex != myPartyIndex) return (nLampIndex + 7) % 8;
                        else return (nLampIndex + 1) % 8;
                    }
                }
                if (p3FireBuff[myPartyIndex] == 2)
                {
                    if (myPartyIndex < 4) return (nLampIndex + 6) % 8;
                    else return (nLampIndex + 2) % 8;
                }
                if (p3FireBuff[myPartyIndex] == 3)
                {
                    if (myPartyIndex < 4)
                    {
                        var highIndex = p3FireBuff.IndexOf(3);
                        if (highIndex == myPartyIndex) return (nLampIndex + 5) % 8;
                        else return (nLampIndex + 3) % 8;
                    }
                    else return (nLampIndex + 0) % 8;
                }
                if (p3FireBuff[myPartyIndex] == 4)
                {
                    if (myPartyIndex < 4) return (nLampIndex + 4) % 8;
                    else return (nLampIndex + 0) % 8;
                }
            }
            return -1;
        }

        #endregion

        #region Phase_4

        [ScriptMethod(name: "----- Phase 4 -----",
            eventType: EventTypeEnum.NpcYell,
            eventCondition: ["Send these, the homeless, tempest-tost to me"])]
        public void Phase4_Placeholder(Event @event, ScriptAccessory accessory) { }

        [ScriptMethod(name: "P4_Manifestation_Phase_Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40246"], userControl: false)]
        public void P4_Manifestation_Phase_Transition(Event @event, ScriptAccessory accessory)
        {
            parse = 41;
        }

        [ScriptMethod(name: "P4_Crystallize_Time_Memory_Crystal_Collection", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40174"], userControl: false)]
        public void P4_Crystallize_Time_Memory_Crystal_Collection(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            p4FragmentId = sid;
        }

        [ScriptMethod(name: "P4_Manifestation_Akh_Rhai", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40237"])]
        public void P4_Manifestation_Akh_Rhai(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Manifestation_Akh_Rhai";
            dp.Scale = new(4);
            dp.Owner = sid;
            dp.TargetObject = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Phase4 Prompt Before Akh Rhai",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40246"])]
        public void Phase4_Prompt_Before_Akh_Rhai(Event @event, ScriptAccessory accessory)
        {
            if (Enable_Text_Prompts)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.Method.TextInfo("Get together and stay away from Fragment of Fate", 9500);
            }
            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.TTS("Get together and stay away from Fragment of Fate", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        [ScriptMethod(name: "Phase4 Prompt To Dodge Akh Rhai",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40186"])]
        public void Phase4_Prompt_To_Dodge_Akh_Rhai(Event @event, ScriptAccessory accessory)
        {
            if (Enable_Text_Prompts)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.Method.TextInfo("Run!", 3000);
            }
            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.TTS("Run!", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        [ScriptMethod(name: "P4_Darklit_Dragonsong_Phase_Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40239"], userControl: false)]
        public void P4_Darklit_Dragonsong_Phase_Transition(Event @event, ScriptAccessory accessory)
        {
            parse = 42;
            p4Tether = [-1, -1, -1, -1, -1, -1, -1, -1];
            p4Stack = [0, 0, 0, 0, 0, 0, 0, 0];
            p4TetherDone = false;
            phase4ManualReset.Reset();
            phase4TetherCount = 0;
        }

        [ScriptMethod(name: "Phase4 Initial Position Before Darklit Dragonsong",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40239"])]
        public void Phase4_Initial_Position_Before_Darklit_Dragonsong(Event @event, ScriptAccessory accessory)
        {
            if (parse != 41 && parse != 42) return;

            List<Vector3> initialPosition = [
                new Vector3(95.5f,0f,94f), new Vector3(98.5f,0f,94f), new Vector3(101.5f,0f,94f), new Vector3(104.5f,0f,94f),
                new Vector3(95.5f,0f,106f), new Vector3(98.5f,0f,106f), new Vector3(101.5f,0f,106f), new Vector3(104.5f,0f,106f),
            ];

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            for (int i = 0; i < initialPosition.Count; ++i)
            {
                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase4_Initial_Position_Before_Darklit_Dragonsong";
                currentProperty.Scale = new(0.5f);
                currentProperty.Position = initialPosition[i];
                currentProperty.Color = (i == myIndex) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                currentProperty.DestoryAt = 5500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);
            }

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase4_Initial_Position_Before_Darklit_Dragonsong";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = initialPosition[myIndex];
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 5500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
        }

        [ScriptMethod(name: "P4_Darklit_Dragonsong_Buff_Recording", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2461"], userControl: false)]
        public void P4_Darklit_Dragonsong_Buff_Recording(Event @event, ScriptAccessory accessory)
        {
            if (parse != 42) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var tIndex = accessory.Data.PartyList.IndexOf(((uint)tid));
            p4Stack[tIndex] = 1;
        }

        [ScriptMethod(name: "P4_Darklit_Dragonsong_Tether_Collection", eventType: EventTypeEnum.Tether, eventCondition: ["Id:006E"], userControl: false)]
        public void P4_Darklit_Dragonsong_Tether_Collection(Event @event, ScriptAccessory accessory)
        {
            if (parse != 42) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var sIndex = accessory.Data.PartyList.IndexOf(((uint)sid));
            var tIndex = accessory.Data.PartyList.IndexOf(((uint)tid));
            lock (this)
            {
                p4Tether[sIndex] = tIndex;
                phase4TetherCount++;
                if (phase4TetherCount == 4) phase4ManualReset.Set();
            }
        }

        [ScriptMethod(name: "P4_Darklit_Dragonsong_Guide_Cone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40187"])]
        public void P4_Darklit_Dragonsong_Guide_Cone(Event @event, ScriptAccessory accessory)
        {
            if (parse != 42) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            for (uint i = 1; i < 5; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Darklit_Dragonsong_Guide_Cone";
                dp.Scale = new(20);
                dp.Radian = float.Pi / 3;
                dp.Owner = sid;
                dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                dp.TargetOrderIndex = i;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 4000;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
        }

        [ScriptMethod(name: "P4_Darklit_Dragonsong_Spirit_Taker", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40187"])]
        public void P4_Darklit_Dragonsong_Spirit_Taker(Event @event, ScriptAccessory accessory)
        {
            if (parse != 42) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Darklit_Dragonsong_Spirit_Taker_Crystal";
            dp.Scale = new(8.5f);
            dp.Owner = p4FragmentId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            for (int i = 0; i < 8; i++)
            {
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Darklit_Dragonsong_Spirit_Taker";
                dp.Scale = new(5);
                dp.Owner = accessory.Data.PartyList[i];
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 3500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }

        [ScriptMethod(name: "P4_Darklit_Dragonsong_Holy_Wings", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[78])$"])]
        public void P4_Darklit_Dragonsong_Holy_Wings(Event @event, ScriptAccessory accessory)
        {
            if (parse != 42) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Darklit_Dragonsong_Holy_Wings";
            dp.Scale = new(40, 20);
            dp.Owner = sid;
            dp.Rotation = @event["ActionId"] == "40227" ? float.Pi / 2 : float.Pi / -2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "P4_Darklit_Dragonsong_Water_Stack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[78])$"])]
        public void P4_Darklit_Dragonsong_Water_Stack(Event @event, ScriptAccessory accessory)
        {
            var tIndex = p4Tether[0] == -1 ? 1 : 0;
            var nIndex = p4Tether[2] == -1 ? 3 : 2;
            var d1Index = -1;
            var d2Index = -1;
            List<int> upGroup = [];
            List<int> downGroup = [];
            for (int i = 4; i < 7; i++)
            {
                for (int j = i + 1; j < 8; j++)
                {
                    if (p4Tether[i] != -1 && p4Tether[j] != -1) { d1Index = i; d2Index = j; }
                }
            }
            if ((p4Tether[tIndex] == d1Index && p4Tether[d2Index] == tIndex) || (p4Tether[tIndex] == d2Index && p4Tether[d1Index] == tIndex))
            { upGroup.Add(tIndex); upGroup.Add(nIndex); downGroup.Add(d1Index); downGroup.Add(d2Index); }
            if ((p4Tether[tIndex] == d1Index && p4Tether[nIndex] == tIndex) || (p4Tether[d1Index] == tIndex && p4Tether[tIndex] == nIndex))
            { upGroup.Add(d1Index); upGroup.Add(nIndex); downGroup.Add(tIndex); downGroup.Add(d2Index); }
            if ((p4Tether[tIndex] == d2Index && p4Tether[nIndex] == tIndex) || (p4Tether[d2Index] == tIndex && p4Tether[tIndex] == nIndex))
            { upGroup.Add(tIndex); upGroup.Add(d1Index); downGroup.Add(nIndex); downGroup.Add(d2Index); }

            var stack1 = p4Stack.IndexOf(1);
            var stack2 = p4Stack.LastIndexOf(1);
            var tetherStack = p4Tether[stack1] == -1 ? stack2 : stack1;
            var idleStack = p4Tether[stack1] == -1 ? stack1 : stack2;

            List<int> idles = [];
            for (int i = 0; i < 8; i++) if (p4Tether[i] == -1) idles.Add(i);
            var ii = idles.IndexOf(idleStack);

            if (Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Double_Swaps_Baiting_First)
            {
                if (upGroup.Contains(tetherStack))
                {
                    if (ii == 0 || ii == 2) { downGroup.Add(idles[0]); downGroup.Add(idles[2]); upGroup.Add(idles[1]); upGroup.Add(idles[3]); }
                    if (ii == 1 || ii == 3) { downGroup.Add(idles[1]); downGroup.Add(idles[3]); upGroup.Add(idles[0]); upGroup.Add(idles[2]); }
                }
                if (downGroup.Contains(tetherStack))
                {
                    if (ii == 0 || ii == 2) { upGroup.Add(idles[0]); upGroup.Add(idles[2]); downGroup.Add(idles[1]); downGroup.Add(idles[3]); }
                    if (ii == 1 || ii == 3) { upGroup.Add(idles[1]); upGroup.Add(idles[3]); downGroup.Add(idles[0]); downGroup.Add(idles[2]); }
                }
            }
            if (Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Single_Swap_Baiting_After || Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Single_Swap_Baiting_First)
            {
                if (upGroup.Contains(tetherStack))
                {
                    if (ii == 0) { downGroup.Add(idles[0]); downGroup.Add(idles[3]); upGroup.Add(idles[2]); upGroup.Add(idles[1]); }
                    if (ii == 1) { upGroup.Add(idles[0]); upGroup.Add(idles[3]); downGroup.Add(idles[2]); downGroup.Add(idles[1]); }
                    if (ii == 2 || ii == 3) { upGroup.Add(idles[0]); downGroup.Add(idles[3]); downGroup.Add(idles[2]); upGroup.Add(idles[1]); }
                }
                if (downGroup.Contains(tetherStack))
                {
                    if (ii == 0 || ii == 1) { upGroup.Add(idles[0]); downGroup.Add(idles[3]); downGroup.Add(idles[2]); upGroup.Add(idles[1]); }
                    if (ii == 2) { downGroup.Add(idles[0]); downGroup.Add(idles[3]); upGroup.Add(idles[2]); upGroup.Add(idles[1]); }
                    if (ii == 3) { upGroup.Add(idles[0]); upGroup.Add(idles[3]); downGroup.Add(idles[2]); downGroup.Add(idles[1]); }
                }
            }

            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Darklit_Dragonsong_Stack";
            dp.Scale = new(6);
            dp.Owner = accessory.Data.PartyList[tetherStack];
            dp.Color = upGroup.Contains(tetherStack) == upGroup.Contains(myindex) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Darklit_Dragonsong_Stack";
            dp.Scale = new(6);
            dp.Owner = accessory.Data.PartyList[idleStack];
            dp.Color = upGroup.Contains(idleStack) == upGroup.Contains(myindex) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Darklit_Dragonsong_Stack_Crystal";
            dp.Scale = new(9.5f);
            dp.Owner = p4FragmentId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "P4_Darklit_Dragonsong_Endless_Insight", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40249"])]
        public void P4_Darklit_Dragonsong_Endless_Insight(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Darklit_Dragonsong_Endless_Insight";
            dp.Scale = new(4);
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.OwnerTarget;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "P4_Darklit_Dragonsong_Far_Jump", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40283"])]
        public void P4_Darklit_Dragonsong_Far_Jump(Event @event, ScriptAccessory accessory)
        {
            if (parse != 42) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Darklit_Dragonsong_Far_Jump";
            dp.Scale = new(8);
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            dp.Color = Phase4_Colour_Of_Somber_Dance.V4.WithW(3f);
            dp.Delay = 2000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "P4_Darklit_Dragonsong_Near_Jump", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40284"])]
        public void P4_Darklit_Dragonsong_Near_Jump(Event @event, ScriptAccessory accessory)
        {
            if (parse != 42) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = "P4_Darklit_Dragonsong_Near_Jump";
            dp2.Scale = new(8);
            dp2.Position = pos;
            dp2.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp2.Color = Phase4_Colour_Of_Somber_Dance.V4.WithW(3f);
            dp2.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);
        }

        [ScriptMethod(name: "P4_Darklit_Dragonsong_Light_Bound_Keep_Distance_Prompt", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40271"], suppress: 2000)]
        public void P4_Darklit_Dragonsong_Light_Bound_Keep_Distance_Prompt(Event @event, ScriptAccessory accessory)
        {
            if (parse != 42) return;
            if (p4Tether[accessory.Data.PartyList.IndexOf(accessory.Data.Me)] == -1) return;
            if (Language_Of_Prompts == Languages_Of_Prompts.English)
            {
                if (Enable_Text_Prompts) accessory.Method.TextInfo("The tether is still, keep your distance", 1500);
                accessory.TTS($"The tether is still, keep your distance", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        [ScriptMethod(name: "P4_Darklit_Dragonsong_Tower_Process_Position", eventType: EventTypeEnum.Tether, eventCondition: ["Id:006E"])]
        public void P4_Darklit_Dragonsong_Tower_Process_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 42) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (sid != accessory.Data.Me) return;

            System.Threading.Thread.MemoryBarrier();
            phase4ManualReset.WaitOne();
            System.Threading.Thread.MemoryBarrier();

            var tIndex = p4Tether[0] == -1 ? 1 : 0;
            var nIndex = p4Tether[2] == -1 ? 3 : 2;
            var d1Index = -1;
            var d2Index = -1;
            List<int> upGroup = [];
            List<int> downGroup = [];
            for (int i = 4; i < 7; i++)
            {
                for (int j = i + 1; j < 8; j++)
                {
                    if (p4Tether[i] != -1 && p4Tether[j] != -1) { d1Index = i; d2Index = j; }
                }
            }
            if ((p4Tether[tIndex] == d1Index && p4Tether[d2Index] == tIndex) || (p4Tether[tIndex] == d2Index && p4Tether[d1Index] == tIndex))
            { upGroup.Add(tIndex); upGroup.Add(nIndex); downGroup.Add(d1Index); downGroup.Add(d2Index); }
            if ((p4Tether[tIndex] == d1Index && p4Tether[nIndex] == tIndex) || (p4Tether[d1Index] == tIndex && p4Tether[tIndex] == nIndex))
            { upGroup.Add(d1Index); upGroup.Add(nIndex); downGroup.Add(tIndex); downGroup.Add(d2Index); }
            if ((p4Tether[tIndex] == d2Index && p4Tether[nIndex] == tIndex) || (p4Tether[d2Index] == tIndex && p4Tether[tIndex] == nIndex))
            { upGroup.Add(tIndex); upGroup.Add(d1Index); downGroup.Add(nIndex); downGroup.Add(d2Index); }

            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 dealpos = upGroup.Contains(myIndex) ? new(100, 0, 92) : new(100, 0, 108);

            var dur = 10000;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Darklit_Dragonsong_Tower_Process_Position";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Darklit_Dragonsong_Tower_Process_Position";
            dp.Scale = new(4);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Position = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "P4_Darklit_Dragonsong_Guide_Process_Position", eventType: EventTypeEnum.Tether, eventCondition: ["Id:006E"], suppress: 2000)]
        public void P4_Darklit_Dragonsong_Guide_Process_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 42) return;
            System.Threading.Thread.MemoryBarrier();
            phase4ManualReset.WaitOne();
            System.Threading.Thread.MemoryBarrier();

            Vector3 dealpos = new();
            if (Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Double_Swaps_Baiting_First || Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Single_Swap_Baiting_First)
            {
                List<int> idles = [];
                for (int i = 0; i < 8; i++) if (p4Tether[i] == -1) idles.Add(i);
                var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                if (!idles.Contains(myIndex)) return;
                dealpos = idles.IndexOf(myIndex) switch
                {
                    0 => new(095.8f, 0, 098.0f),
                    1 => new(104.2f, 0, 098.0f),
                    2 => new(095.8f, 0, 102.0f),
                    3 => new(104.2f, 0, 102.0f),
                };
            }
            if (Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Single_Swap_Baiting_After)
            {
                var tIndex = p4Tether[0] == -1 ? 1 : 0;
                var nIndex = p4Tether[2] == -1 ? 3 : 2;
                var d1Index = -1;
                var d2Index = -1;
                List<int> upGroup = [];
                List<int> downGroup = [];
                for (int i = 4; i < 7; i++)
                {
                    for (int j = i + 1; j < 8; j++)
                    {
                        if (p4Tether[i] != -1 && p4Tether[j] != -1) { d1Index = i; d2Index = j; }
                    }
                }
                if ((p4Tether[tIndex] == d1Index && p4Tether[d2Index] == tIndex) || (p4Tether[tIndex] == d2Index && p4Tether[d1Index] == tIndex))
                { upGroup.Add(tIndex); upGroup.Add(nIndex); downGroup.Add(d1Index); downGroup.Add(d2Index); }
                if ((p4Tether[tIndex] == d1Index && p4Tether[nIndex] == tIndex) || (p4Tether[d1Index] == tIndex && p4Tether[tIndex] == nIndex))
                { upGroup.Add(d1Index); upGroup.Add(nIndex); downGroup.Add(tIndex); downGroup.Add(d2Index); }
                if ((p4Tether[tIndex] == d2Index && p4Tether[nIndex] == tIndex) || (p4Tether[d2Index] == tIndex && p4Tether[tIndex] == nIndex))
                { upGroup.Add(tIndex); upGroup.Add(d1Index); downGroup.Add(nIndex); downGroup.Add(d2Index); }
                var stack1 = p4Stack.IndexOf(1);
                var stack2 = p4Stack.LastIndexOf(1);
                var tetherStack = p4Tether[stack1] == -1 ? stack2 : stack1;
                var idleStack = p4Tether[stack1] == -1 ? stack1 : stack2;

                List<int> idles = [];
                for (int i = 0; i < 8; i++) if (p4Tether[i] == -1) idles.Add(i);
                var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var ii = idles.IndexOf(idleStack);
                if (upGroup.Contains(tetherStack))
                {
                    if (ii == 0) dealpos = idles.IndexOf(myIndex) switch { 2 => new(095.8f, 0, 098.0f), 1 => new(104.2f, 0, 098.0f), 0 => new(095.8f, 0, 102.0f), 3 => new(104.2f, 0, 102.0f), };
                    if (ii == 1) dealpos = idles.IndexOf(myIndex) switch { 0 => new(095.8f, 0, 098.0f), 3 => new(104.2f, 0, 098.0f), 2 => new(095.8f, 0, 102.0f), 1 => new(104.2f, 0, 102.0f), };
                    if (ii == 2 || ii == 3) dealpos = idles.IndexOf(myIndex) switch { 0 => new(095.8f, 0, 098.0f), 1 => new(104.2f, 0, 098.0f), 2 => new(095.8f, 0, 102.0f), 3 => new(104.2f, 0, 102.0f), };
                }
                if (downGroup.Contains(tetherStack))
                {
                    if (ii == 2) dealpos = idles.IndexOf(myIndex) switch { 2 => new(095.8f, 0, 098.0f), 1 => new(104.2f, 0, 098.0f), 0 => new(095.8f, 0, 102.0f), 3 => new(104.2f, 0, 102.0f), };
                    if (ii == 3) dealpos = idles.IndexOf(myIndex) switch { 0 => new(095.8f, 0, 098.0f), 3 => new(104.2f, 0, 098.0f), 2 => new(095.8f, 0, 102.0f), 1 => new(104.2f, 0, 102.0f), };
                    if (ii == 0 || ii == 1) dealpos = idles.IndexOf(myIndex) switch { 0 => new(095.8f, 0, 098.0f), 1 => new(104.2f, 0, 098.0f), 2 => new(095.8f, 0, 102.0f), 3 => new(104.2f, 0, 102.0f), };
                }
            }

            var dur = 10000;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Darklit_Dragonsong_Guide_Process_Position";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "P4_Darklit_Dragonsong_Stack_Process_Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[78])$"])]
        public void P4_Darklit_Dragonsong_Stack_Process_Position(Event @event, ScriptAccessory accessory)
        {
            var tIndex = p4Tether[0] == -1 ? 1 : 0;
            var nIndex = p4Tether[2] == -1 ? 3 : 2;
            var d1Index = -1;
            var d2Index = -1;
            List<int> upGroup = [];
            List<int> downGroup = [];
            for (int i = 4; i < 7; i++)
            {
                for (int j = i + 1; j < 8; j++)
                {
                    if (p4Tether[i] != -1 && p4Tether[j] != -1) { d1Index = i; d2Index = j; }
                }
            }
            if ((p4Tether[tIndex] == d1Index && p4Tether[d2Index] == tIndex) || (p4Tether[tIndex] == d2Index && p4Tether[d1Index] == tIndex))
            { upGroup.Add(tIndex); upGroup.Add(nIndex); downGroup.Add(d1Index); downGroup.Add(d2Index); }
            if ((p4Tether[tIndex] == d1Index && p4Tether[nIndex] == tIndex) || (p4Tether[d1Index] == tIndex && p4Tether[tIndex] == nIndex))
            { upGroup.Add(d1Index); upGroup.Add(nIndex); downGroup.Add(tIndex); downGroup.Add(d2Index); }
            if ((p4Tether[tIndex] == d2Index && p4Tether[nIndex] == tIndex) || (p4Tether[d2Index] == tIndex && p4Tether[tIndex] == nIndex))
            { upGroup.Add(tIndex); upGroup.Add(d1Index); downGroup.Add(nIndex); downGroup.Add(d2Index); }
            var stack1 = p4Stack.IndexOf(1);
            var stack2 = p4Stack.LastIndexOf(1);
            var tetherStack = p4Tether[stack1] == -1 ? stack2 : stack1;
            var idleStack = p4Tether[stack1] == -1 ? stack1 : stack2;

            List<int> idles = [];
            for (int i = 0; i < 8; i++) if (p4Tether[i] == -1) idles.Add(i);
            var ii = idles.IndexOf(idleStack);
            if (Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Double_Swaps_Baiting_First)
            {
                if (upGroup.Contains(tetherStack))
                {
                    if (ii == 0 || ii == 2) { downGroup.Add(idles[0]); downGroup.Add(idles[2]); upGroup.Add(idles[1]); upGroup.Add(idles[3]); }
                    if (ii == 1 || ii == 3) { downGroup.Add(idles[1]); downGroup.Add(idles[3]); upGroup.Add(idles[0]); upGroup.Add(idles[2]); }
                }
                if (downGroup.Contains(tetherStack))
                {
                    if (ii == 0 || ii == 2) { upGroup.Add(idles[0]); upGroup.Add(idles[2]); downGroup.Add(idles[1]); downGroup.Add(idles[3]); }
                    if (ii == 1 || ii == 3) { upGroup.Add(idles[1]); upGroup.Add(idles[3]); downGroup.Add(idles[0]); downGroup.Add(idles[2]); }
                }
            }
            if (Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Single_Swap_Baiting_After || Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Single_Swap_Baiting_First)
            {
                if (upGroup.Contains(tetherStack))
                {
                    if (ii == 0) { downGroup.Add(idles[0]); downGroup.Add(idles[3]); upGroup.Add(idles[2]); upGroup.Add(idles[1]); }
                    if (ii == 1) { upGroup.Add(idles[0]); upGroup.Add(idles[3]); downGroup.Add(idles[2]); downGroup.Add(idles[1]); }
                    if (ii == 2 || ii == 3) { upGroup.Add(idles[0]); downGroup.Add(idles[3]); downGroup.Add(idles[2]); upGroup.Add(idles[1]); }
                }
                if (downGroup.Contains(tetherStack))
                {
                    if (ii == 0 || ii == 1) { upGroup.Add(idles[0]); downGroup.Add(idles[3]); downGroup.Add(idles[2]); upGroup.Add(idles[1]); }
                    if (ii == 2) { downGroup.Add(idles[0]); downGroup.Add(idles[3]); upGroup.Add(idles[2]); upGroup.Add(idles[1]); }
                    if (ii == 3) { upGroup.Add(idles[0]); upGroup.Add(idles[3]); downGroup.Add(idles[2]); downGroup.Add(idles[1]); }
                }
            }

            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 dealpos = new(@event["ActionId"] == "40227" ? 105 : 95, 0, upGroup.Contains(myindex) ? 92.5f : 107.5f);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Darklit_Dragonsong_Stack_Process_Position";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        public class CrystallizeTime
        {
            public ScriptAccessory Sa { get; set; } = null!;
            public PriorityDict Pr { get; set; } = null!;
            public ulong LeftWyrmSid { get; set; } = 0;
            public ulong RightWyrmSid { get; set; } = 0;
            public int LeftIcePlayerIdx { get; set; } = -1;
            public int RightIcePlayerIdx { get; set; } = -1;
            public int LeftWindPlayerIdx { get; set; } = -1;
            public int RightWindPlayerIdx { get; set; } = -1;

            public void Init(ScriptAccessory accessory, PriorityDict priorityDict)
            {
                Sa = accessory;
                Pr = priorityDict;
                LeftWyrmSid = 0;
                RightWyrmSid = 0;
                LeftIcePlayerIdx = -1;
                RightIcePlayerIdx = -1;
                LeftWindPlayerIdx = -1;
                RightWindPlayerIdx = -1;
            }
        }

        public class PriorityDict
        {
            public ScriptAccessory sa { get; set; } = null!;
            public Dictionary<int, int> Priorities { get; set; } = null!;
            public string Annotation { get; set; } = "";
            public int ActionCount { get; set; } = 0;

            public void Init(ScriptAccessory accessory, string annotation, int partyNum = 8)
            {
                sa = accessory;
                Priorities = new Dictionary<int, int>();
                for (var i = 0; i < partyNum; i++) Priorities.Add(i, 0);
                Annotation = annotation;
                ActionCount = 0;
            }

            public void AddPriority(int idx, int priority) { Priorities[idx] += priority; }

            public List<KeyValuePair<int, int>> SelectSmallPriorityIndices(int num) { return SelectMiddlePriorityIndices(0, num); }

            public List<KeyValuePair<int, int>> SelectLargePriorityIndices(int num) { return SelectMiddlePriorityIndices(0, num, true); }

            public List<KeyValuePair<int, int>> SelectMiddlePriorityIndices(int skip, int num, bool descending = false)
            {
                if (Priorities.Count < skip + num) return new List<KeyValuePair<int, int>>();
                IEnumerable<KeyValuePair<int, int>> sortedPriorities;
                if (descending) sortedPriorities = Priorities.OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key).Skip(skip).Take(num);
                else sortedPriorities = Priorities.OrderBy(pair => pair.Value).ThenBy(pair => pair.Key).Skip(skip).Take(num);
                return sortedPriorities.ToList();
            }

            public KeyValuePair<int, int> SelectSpecificPriorityIndex(int idx, bool descending = false)
            {
                var sortedPriorities = SelectMiddlePriorityIndices(0, 8, descending);
                return sortedPriorities[idx];
            }

            public int FindPriorityIndexOfKey(int key, bool descending = false)
            {
                var sortedPriorities = SelectMiddlePriorityIndices(0, 8, descending);
                var i = 0;
                foreach (var dict in sortedPriorities) if (dict.Key == key) return i; else i++;
                return i;
            }

            public void AddPriorities(List<int> priorities)
            {
                if (Priorities.Count != priorities.Count) sa.Log.Error("Input list length does not match internal settings");
                for (var i = 0; i < Priorities.Count; i++) AddPriority(i, priorities[i]);
            }

            public string ShowPriorities()
            {
                var str = $"{Annotation} Priority Dictionary:\n";
                foreach (var pair in Priorities) str += $"Key {pair.Key} ({sa.GetPlayerJobByIndex(pair.Key)}), Value {pair.Value}\n";
                sa.Log.Debug(str);
                return str;
            }

            public string PrintAnnotation() { sa.Log.Debug(Annotation); return Annotation; }

            public PriorityDict DeepCopy() { return JsonConvert.DeserializeObject<PriorityDict>(JsonConvert.SerializeObject(this)) ?? new PriorityDict(); }

            public void AddActionCount(int count = 1) { ActionCount += count; }

            public bool IsActionCountEqualTo(int times) { return ActionCount == times; }
        }

        [ScriptMethod(name: "P4_Crystallize_Time_Phase_Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40240"], userControl: false)]
        public void P4_Crystallize_Time_Phase_Transition(Event @event, ScriptAccessory accessory)
        {
            parse = 43;

            _pd.Init(accessory, "Crystallize Time");
            _cry.Init(accessory, _pd);
            _events = [.. Enumerable.Range(0, 20).Select(_ => new System.Threading.ManualResetEvent(false))];

            List<int> pdList = Phase4_Priority_Of_The_Players_With_Wyrmclaw switch
            {
                Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_THD_Order => [0, 1, 2, 3, 4, 5, 6, 7],
                Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_HTD_Order => [2, 3, 0, 1, 4, 5, 6, 7],
                Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_H1TDH2_Order => [1, 2, 0, 7, 3, 4, 5, 6],
                _ => [2, 3, 0, 1, 4, 5, 6, 7],
            };
            _pd.AddPriorities(pdList);

            p4WyrmclawBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            phase4MajorDebuffCount = 0;
            phase4MajorDebuffsConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
            phase4IncidentalDebuffCount = 0;
            phase4IncidentalDebuffsConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
            phase4WyrmfangPlayerMarks = [MarkType.Cross, MarkType.Cross, MarkType.Cross, MarkType.Cross, MarkType.Cross, MarkType.Cross, MarkType.Cross, MarkType.Cross];
            p4OtherBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            p4WaterPositions = [];
            phase4DrachenWandererId1 = "";
            phase4DrachenWandererId2 = "";
            phase4WyrmclawRemovalCount = 0;
            phase4ResidueIdsEastToWest = [0, 0, 0, 0];
            phase4ResidueGuidanceGenerated = false;
        }

        [ScriptMethod(name: "P4_Crystallize_Time_Buff_Collection", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(326[34]|2454|246[0123])$"], userControl: false)]
        public void P4_Crystallize_Time_Buff_Collection(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            var id = @event["StatusID"];
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(((uint)tid));
            if (id == "3263")
            {
                if (!float.TryParse(@event["Duration"], out float dur)) return;
                p4WyrmclawBuff[index] = dur > 20 ? 2 : 1;
                _pd.AddPriority(index, 0);
            }
            if (id == "3264") { p4WyrmclawBuff[index] = 3; _pd.AddPriority(index, 100); }
            if (id == "2460") { p4OtherBuff[index] = 4; _pd.AddPriority(index, 40); }
            if (id == "2461") { p4OtherBuff[index] = 3; _pd.AddPriority(index, 20); }
            if (id == "2462") { p4OtherBuff[index] = 1; _pd.AddPriority(index, 0); }
            if (id == "2463") { p4OtherBuff[index] = 2; _pd.AddPriority(index, 10); }
            if (id == "2454") { p4OtherBuff[index] = 5; _pd.AddPriority(index, 30); }

            System.Threading.Thread.MemoryBarrier();

            if (id.Equals("3263") || id.Equals("3264"))
            {
                lock (phase4MajorDebuffLock)
                {
                    ++phase4MajorDebuffCount;
                    System.Threading.Thread.MemoryBarrier();
                    if (phase4MajorDebuffCount == 8)
                    {
                        phase4MajorDebuffsConfirmedSemaphore.Set();
                        _events[0].Set();
                    }
                }
            }

            if (id.Equals("2460") || id.Equals("2461") || id.Equals("2462") || id.Equals("2463") || id.Equals("2454"))
            {
                lock (phase4IncidentalDebuffLock)
                {
                    ++phase4IncidentalDebuffCount;
                    System.Threading.Thread.MemoryBarrier();
                    if (phase4IncidentalDebuffCount == 8)
                    {
                        phase4IncidentalDebuffsConfirmedSemaphore.Set();
                        _events[1].Set();
                    }
                }
            }
        }

        [ScriptMethod(name: "P4_Crystallize_Time_Calculate_Group_Assignment",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40298"],
            userControl: false,
            suppress: 10000)]
        public void P4_Crystallize_Time_Calculate_Group_Assignment(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;

            _events[0].WaitOne();
            _events[1].WaitOne();

            _cry.LeftIcePlayerIdx = _pd.SelectSpecificPriorityIndex(0).Key;
            _cry.RightIcePlayerIdx = _pd.SelectSpecificPriorityIndex(1).Key;
            _cry.LeftWindPlayerIdx = _pd.SelectSpecificPriorityIndex(2).Key;
            _cry.RightWindPlayerIdx = _pd.SelectSpecificPriorityIndex(3).Key;
            accessory.Log.Debug($"Recorded Left Ice {_cry.LeftIcePlayerIdx}, Right Ice {_cry.RightIcePlayerIdx}, Left Wind {_cry.LeftWindPlayerIdx}, Right Wind {_cry.RightWindPlayerIdx}");

            _events[2].Set();
        }

        [ScriptMethod(name: "P4_Crystallize_Time_Receive_External_Markers",
            eventType: EventTypeEnum.Marker,
            eventCondition: ["Operate:Add", "Id:regex:^(0[679]|10)$"],
            userControl: false)]
        public void P4_Crystallize_Time_Receive_External_Markers(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (Phase4_Mark_Players_During_The_Second_Half) return;

            _events[2].WaitOne();
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf((uint)tid);
            if (!int.TryParse(@event["Id"], out var sign)) return;

            const int stop1 = 9, stop2 = 10, bind1 = 6, bind2 = 7;

            switch (sign)
            {
                case stop1: _cry.LeftIcePlayerIdx = index; accessory.Log.Debug($"Crystallize Time: Received external stop1 marker, assigned to {index}"); break;
                case stop2: _cry.RightIcePlayerIdx = index; accessory.Log.Debug($"Crystallize Time: Received external stop2 marker, assigned to {index}"); break;
                case bind1: _cry.LeftWindPlayerIdx = index; accessory.Log.Debug($"Crystallize Time: Received external bind1 marker, assigned to {index}"); break;
                case bind2: _cry.RightWindPlayerIdx = index; accessory.Log.Debug($"Crystallize Time: Received external bind2 marker, assigned to {index}"); break;
                default: break;
            }
        }

        [ScriptMethod(name: "Phase4 Mark Teammates During The Second Half",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40298"],
            userControl: false,
            suppress: 2000)]
        public void Phase4_Mark_Teammates_During_The_Second_Half(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (!Phase4_Mark_Players_During_The_Second_Half) return;

            System.Threading.Thread.MemoryBarrier();
            phase4MajorDebuffsConfirmedSemaphore.WaitOne();
            phase4IncidentalDebuffsConfirmedSemaphore.WaitOne();
            System.Threading.Thread.MemoryBarrier();

            List<int> temporaryOrder = [0, 1, 2, 3, 4, 5, 6, 7];
            string debugOutput = "";

            if (Phase4_Player_Type_To_Be_Marked == Phase4_Player_Types_To_Be_Marked.Both_Wyrmclaw_And_Wyrmfang ||
                Phase4_Player_Type_To_Be_Marked == Phase4_Player_Types_To_Be_Marked.Only_Wyrmfang)
            {
                if (Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang == Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang.According_To_Debuffs_1234_From_East_To_West ||
                    Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang == Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang.According_To_Debuffs_1342_From_East_To_West)
                {
                    for (int i = 0; i < 8; ++i)
                    {
                        if (p4WyrmclawBuff[i] == 3)
                        {
                            int markIndex = -1;
                            if (p4OtherBuff[i] == 4) markIndex = phase4_getMarkIndex(Phase4_Residue_Belongs_To_Dark_Eruption);
                            if (p4OtherBuff[i] == 5) markIndex = phase4_getMarkIndex(Phase4_Residue_Belongs_To_Unholy_Darkness);
                            if (p4OtherBuff[i] == 1) markIndex = phase4_getMarkIndex(Phase4_Residue_Belongs_To_Dark_Blizzard_III);
                            if (p4OtherBuff[i] == 3) markIndex = phase4_getMarkIndex(Phase4_Residue_Belongs_To_Dark_Water_III);
                            if (markIndex != -1)
                            {
                                accessory.Method.Mark(accessory.Data.PartyList[i], phase4WyrmfangMarks[markIndex]);
                                debugOutput += $"i={i},markIndex={markIndex},phase4WyrmfangMarks[markIndex]={phase4WyrmfangMarks[markIndex]}\n";
                            }
                        }
                    }
                }

                if (Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang == Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang.According_To_The_Priority_THD)
                {
                    for (int i = 0, j = 0; i < 8; ++i)
                    {
                        if (p4WyrmclawBuff[i] == 3 && j < 4)
                        {
                            accessory.Method.Mark(accessory.Data.PartyList[i], phase4WyrmfangMarks[j]);
                            debugOutput += $"i={i},phase4WyrmfangMarks[j]={phase4WyrmfangMarks[j]}\n";
                            ++j;
                        }
                    }
                }

                if (Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang == Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang.According_To_The_Priority_HTD)
                {
                    temporaryOrder = [2, 3, 0, 1, 4, 5, 6, 7];
                    for (int i = 0, j = 0; i < temporaryOrder.Count; ++i)
                    {
                        if (p4WyrmclawBuff[temporaryOrder[i]] == 3 && j < 4)
                        {
                            accessory.Method.Mark(accessory.Data.PartyList[temporaryOrder[i]], phase4WyrmfangMarks[j]);
                            debugOutput += $"temporaryOrder[i]={temporaryOrder[i]},phase4WyrmfangMarks[j]={phase4WyrmfangMarks[j]}\n";
                            ++j;
                        }
                    }
                }

                if (Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang == Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang.According_To_The_Priority_H1TDH2)
                {
                    temporaryOrder = [2, 0, 1, 4, 5, 6, 7, 3];
                    for (int i = 0, j = 0; i < temporaryOrder.Count; ++i)
                    {
                        if (p4WyrmclawBuff[temporaryOrder[i]] == 3 && j < 4)
                        {
                            accessory.Method.Mark(accessory.Data.PartyList[temporaryOrder[i]], phase4WyrmfangMarks[j]);
                            debugOutput += $"temporaryOrder[i]={temporaryOrder[i]},phase4WyrmfangMarks[j]={phase4WyrmfangMarks[j]}\n";
                            ++j;
                        }
                    }
                }
            }

            if (Phase4_Player_Type_To_Be_Marked == Phase4_Player_Types_To_Be_Marked.Both_Wyrmclaw_And_Wyrmfang ||
                Phase4_Player_Type_To_Be_Marked == Phase4_Player_Types_To_Be_Marked.Only_Wyrmclaw)
            {
                temporaryOrder = [0, 1, 2, 3, 4, 5, 6, 7];
                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_THD_Order) temporaryOrder = [0, 1, 2, 3, 4, 5, 6, 7];
                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_HTD_Order) temporaryOrder = [2, 3, 0, 1, 4, 5, 6, 7];
                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_H1TDH2_Order) temporaryOrder = [2, 0, 1, 4, 5, 6, 7, 3];

                List<MarkType> marksForShortWyrmclaw = [MarkType.Stop1, MarkType.Bind1];
                List<MarkType> marksForLongWyrmclaw = [MarkType.Stop2, MarkType.Bind2];

                if (Phase4_Logic_Of_Marking_Teammates_With_Wyrmclaw == Phase4_Logics_Of_Marking_Teammates_With_Wyrmclaw.Ignore1_And_Bind1_Go_West)
                { marksForShortWyrmclaw = [MarkType.Stop1, MarkType.Stop2]; marksForLongWyrmclaw = [MarkType.Bind1, MarkType.Bind2]; }
                if (Phase4_Logic_Of_Marking_Teammates_With_Wyrmclaw == Phase4_Logics_Of_Marking_Teammates_With_Wyrmclaw.Ignore1_And_Ignore2_Go_West)
                { marksForShortWyrmclaw = [MarkType.Stop1, MarkType.Bind1]; marksForLongWyrmclaw = [MarkType.Stop2, MarkType.Bind2]; }

                for (int i = 0, j = 0, k = 0; i < temporaryOrder.Count; ++i)
                {
                    if (p4WyrmclawBuff[temporaryOrder[i]] == 1 && j < 2)
                    {
                        accessory.Method.Mark(accessory.Data.PartyList[temporaryOrder[i]], marksForShortWyrmclaw[j]);
                        debugOutput += $"temporaryOrder[i]={temporaryOrder[i]},marksForShortWyrmclaw[j]={marksForShortWyrmclaw[j]}\n";
                        ++j;
                    }
                    if (p4WyrmclawBuff[temporaryOrder[i]] == 2 && k < 2)
                    {
                        accessory.Method.Mark(accessory.Data.PartyList[temporaryOrder[i]], marksForLongWyrmclaw[k]);
                        debugOutput += $"temporaryOrder[i]={temporaryOrder[i]},marksForLongWyrmclaw[k]={marksForLongWyrmclaw[k]}\n";
                        ++k;
                    }
                }
            }

            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e {debugOutput}");
                accessory.Log.Debug($"{debugOutput}");
            }
        }

        private int phase4_getMarkIndex(Phase4_Relative_Positions_Of_Residues currentPosition)
        {
            if (Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang == Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang.According_To_Debuffs_1234_From_East_To_West)
            {
                if (currentPosition == Phase4_Relative_Positions_Of_Residues.Eastmost) return 0;
                if (currentPosition == Phase4_Relative_Positions_Of_Residues.About_East) return 1;
                if (currentPosition == Phase4_Relative_Positions_Of_Residues.About_West) return 2;
                if (currentPosition == Phase4_Relative_Positions_Of_Residues.Westmost) return 3;
            }
            if (Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang == Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang.According_To_Debuffs_1342_From_East_To_West)
            {
                if (currentPosition == Phase4_Relative_Positions_Of_Residues.Eastmost) return 0;
                if (currentPosition == Phase4_Relative_Positions_Of_Residues.About_East) return 2;
                if (currentPosition == Phase4_Relative_Positions_Of_Residues.About_West) return 3;
                if (currentPosition == Phase4_Relative_Positions_Of_Residues.Westmost) return 1;
            }
            return -1;
        }

        [ScriptMethod(name: "P4_Crystallize_Time_Blue_Tether_Collection", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0085"], userControl: false)]
        public void P4_Crystallize_Time_Blue_Tether_Collection(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            p4BlueTether = PositionTo6Dir(pos, new(100, 0, 100)) % 3;
        }

        [ScriptMethod(name: "P4_Crystallize_Time_Lamp_AOE", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0085"])]
        public void P4_Crystallize_Time_Lamp_AOE(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            Vector3 normalPos = new(pos.X, 0, 200 - pos.Z);
            Vector3 fastPos = new(100, 0, pos.Z > 100 ? 111 : 89);
            uint actualDuration = (0 <= Phase4_Drawing_Duration_Of_Normal_And_Delayed_Lights && Phase4_Drawing_Duration_Of_Normal_And_Delayed_Lights <= 13) ? (uint)(1000 * Phase4_Drawing_Duration_Of_Normal_And_Delayed_Lights) : 3000;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Crystallize_Time_Lamp_AOE_Fast";
            dp.Scale = new(12);
            dp.Position = fastPos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Crystallize_Time_Lamp_AOE_Medium";
            dp.Scale = new(12);
            dp.Position = normalPos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 13000 - actualDuration;
            dp.DestoryAt = actualDuration;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Crystallize_Time_Lamp_AOE_Slow";
            dp.Scale = new(12);
            dp.Position = pos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 18000 - actualDuration;
            dp.DestoryAt = actualDuration;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "P4_Crystallize_Time_Earth_Stack_Range", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2454"])]
        public void P4_Crystallize_Time_Earth_Stack_Range(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Crystallize_Time_Earth_Stack_Range";
            dp.Scale = new(6);
            dp.Owner = tid;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 14000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Crystallize_Time_Earth_Stack_Range_Crystal";
            dp.Scale = new(9.5f);
            dp.Owner = p4FragmentId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 14000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "P4_Crystallize_Time_Spirit_Taker", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2452"])]
        public void P4_Crystallize_Time_Spirit_Taker(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Crystallize_Time_Spirit_Taker_Crystal";
            dp.Scale = new(8.5f);
            dp.Owner = p4FragmentId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            for (int i = 0; i < 8; i++)
            {
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Crystallize_Time_Spirit_Taker";
                dp.Scale = new(5);
                dp.Owner = accessory.Data.PartyList[i];
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 3500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }

        [ScriptMethod(name: "P4_Crystallize_Time_Buff_Process_Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40293"])]
        public void P4_Crystallize_Time_Buff_Process_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (p4WyrmclawBuff[myIndex] == 1)
            {
                bool isHigh = true;
                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_THD_Order) isHigh = (p4WyrmclawBuff.IndexOf(1) == myIndex);
                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_HTD_Order)
                {
                    List<int> temporaryPriority = [2, 3, 0, 1, 4, 5, 6, 7];
                    for (int i = 0; i < temporaryPriority.Count; ++i)
                    {
                        if (p4WyrmclawBuff[temporaryPriority[i]] == 1)
                        {
                            isHigh = (temporaryPriority[i] == myIndex);
                            break;
                        }
                    }
                }
                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_H1TDH2_Order)
                {
                    List<int> temporaryPriority = [2, 0, 1, 4, 5, 6, 7, 3];
                    for (int i = 0; i < temporaryPriority.Count; ++i)
                    {
                        if (p4WyrmclawBuff[temporaryPriority[i]] == 1)
                        {
                            isHigh = (temporaryPriority[i] == myIndex);
                            break;
                        }
                    }
                }

                Vector3 dealpos = isHigh ? new(87, 0, 100) : new(113, 0, 100);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Crystallize_Time_Buff_Process_Position_Bump_Dragon";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 10500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                Vector3 dealpos2 = isHigh ? (p4BlueTether == 1 ? new(081, 0, 103) : new(088, 0, 085)) : (p4BlueTether == 1 ? new(112, 0, 085) : new(119, 0, 103));
                Vector3 dealpos3 = isHigh ? (p4BlueTether == 1 ? new(081, 0, 097) : new(093, 0, 082)) : (p4BlueTether == 1 ? new(107, 0, 082) : new(119, 0, 097));

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Crystallize_Time_Buff_Process_Position_pos2_Preview_Line";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 10500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Crystallize_Time_Buff_Process_Position_pos2_Position";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 10500;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Crystallize_Time_Buff_Process_Position_pos3_Preview_Line";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos2;
                dp.TargetPosition = dealpos3;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 13500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Crystallize_Time_Buff_Process_Position_pos3_Position";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos3;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 13500;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (p4WyrmclawBuff[myIndex] == 2)
            {
                bool isHigh = true;
                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_THD_Order) isHigh = (p4WyrmclawBuff.IndexOf(2) == myIndex);
                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_HTD_Order)
                {
                    List<int> temporaryPriority = [2, 3, 0, 1, 4, 5, 6, 7];
                    for (int i = 0; i < temporaryPriority.Count; ++i)
                    {
                        if (p4WyrmclawBuff[temporaryPriority[i]] == 2)
                        {
                            isHigh = (temporaryPriority[i] == myIndex);
                            break;
                        }
                    }
                }
                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_H1TDH2_Order)
                {
                    List<int> temporaryPriority = [2, 0, 1, 4, 5, 6, 7, 3];
                    for (int i = 0; i < temporaryPriority.Count; ++i)
                    {
                        if (p4WyrmclawBuff[temporaryPriority[i]] == 2)
                        {
                            isHigh = (temporaryPriority[i] == myIndex);
                            break;
                        }
                    }
                }

                Vector3 dealpos1 = isHigh ? new(088.5f, 0, 115.5f) : new(111.5f, 0, 115.5f);
                Vector3 dealpos2 = isHigh ? new(090.2f, 0, 117.0f) : new(109.8f, 0, 117.0f);
                Vector3 dealpos3 = isHigh ? new(092.5f, 0, 118.0f) : new(107.5f, 0, 118.0f);
                Vector3 dealpos4 = isHigh ? new(092.53f, 0, 110.40f) : new(107.47f, 0, 110.40f);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Crystallize_Time_Buff_Process_Position_Dodge_AC";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos1;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Crystallize_Time_Buff_Process_Position_Dodge_AC_To_Knockback";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos1;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Crystallize_Time_Buff_Process_Position_Knockback";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 7500;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Crystallize_Time_Buff_Process_Position_Knockback_To_Dodge_Diagonal";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos2;
                dp.TargetPosition = dealpos3;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 10500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Crystallize_Time_Buff_Process_Position_Dodge_Diagonal";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos3;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 10500;
                dp.DestoryAt = 2500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Crystallize_Time_Buff_Process_Position_Dodge_Diagonal_To_Bump";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos3;
                dp.TargetPosition = dealpos4;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 13000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_Crystallize_Time_Buff_Process_Position_Bump";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos4;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 13000;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (p4WyrmclawBuff[myIndex] == 3)
            {
                if (p4OtherBuff[myIndex] == 4)
                {
                    Vector3 dealpos1 = p4BlueTether == 1 ? new(112, 0, 85) : new(88, 0, 85);
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_Crystallize_Time_Buff_Process_Position_Dodge_Lamp1";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos1;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 14500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                else
                {
                    Vector3 dealpos1 = p4BlueTether == 1 ? new(88, 0, 115) : new(112, 0, 115);
                    Vector3 dealpos2 = p4BlueTether == 1 ? new(090.8f, 0, 116.0f) : new(109.2f, 0, 116.0f);
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_Crystallize_Time_Buff_Process_Position_Dodge_Lamp_AC";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos1;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_Crystallize_Time_Buff_Process_Position_Dodge_AC_To_Knockback";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Position = dealpos1;
                    dp.TargetPosition = dealpos2;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_Crystallize_Time_Buff_Process_Position_Knockback";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos2;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 7500;
                    dp.DestoryAt = 3000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }
        }

        [ScriptMethod(name: "P4_Crystallize_Time_Place_Return_Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40251"])]
        public void P4_Crystallize_Time_Place_Return_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            p4WaterPositions.Add(pos);
            if (p4WaterPositions.Count == 1) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 centre = new(100, 0, 100);
            if (Phase4_Position_Before_Knockback == Phase4_Positions_Before_Knockback.Normal)
            {
                var dir8 = PositionTo8Dir((p4WaterPositions[0] + p4WaterPositions[1]) / 2, centre) - 1;
                Vector3 mtPos = new(107, 0, 88);
                Vector3 stPos = new(112, 0, 93);
                Vector3 mtgPos = new(106, 0, 92);
                Vector3 stgPos = new(108, 0, 94);
                if (myindex == 0)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_Crystallize_Time_Place_Return_Position_MT";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(mtPos, centre, float.Pi / 4 * dir8);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                if (myindex == 1)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_Crystallize_Time_Place_Return_Position_ST";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(stPos, centre, float.Pi / 4 * dir8);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                if (myindex == 2 || myindex == 4 || myindex == 6)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_Crystallize_Time_Place_Return_Position_MTG";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(mtgPos, centre, float.Pi / 4 * dir8);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                if (myindex == 3 || myindex == 5 || myindex == 7)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_Crystallize_Time_Place_Return_Position_STG";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(stgPos, centre, float.Pi / 4 * dir8);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }
            if (Phase4_Position_Before_Knockback == Phase4_Positions_Before_Knockback.Y_Formation_Japanese_PF)
            {
                Vector3 mtPos = p4WaterPositions[1].Z < 100 ? new(92, 0, 90) : new(108, 0, 110);
                Vector3 stPos = p4WaterPositions[1].Z < 100 ? new(108, 0, 90) : new(92, 0, 110);
                Vector3 gPos = p4WaterPositions[1].Z < 100 ? new(100, 0, 96) : new(100, 0, 104);
                if (myindex == 0)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_Crystallize_Time_Place_Return_Position_MT";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = mtPos;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                if (myindex == 1)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_Crystallize_Time_Place_Return_Position_ST";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = stPos;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                if (myindex == 2 || myindex == 3 || myindex == 4 || myindex == 5 || myindex == 6 || myindex == 7)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_Crystallize_Time_Place_Return_Position_Group";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = gPos;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }
        }

        [ScriptMethod(name: "Phase4 Acquire IDs Of Drachen Wanderers",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: ["DataId:17836"],
            userControl: false)]
        public void Phase4_Acquire_IDs_Of_Drachen_Wanderers(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            lock (phase4DrachenWandererIdLock)
            {
                if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;
                var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                if (spos.X < 100) { _cry.LeftWyrmSid = sourceId; accessory.Log.Debug($"Crystallize Time: Recorded left Drachen Wanderer {spos} ID {sourceId}"); }
                else { _cry.RightWyrmSid = sourceId; accessory.Log.Debug($"Crystallize Time: Recorded right Drachen Wanderer {spos} ID {sourceId}"); }
                if ((_cry.LeftWyrmSid != 0) && (_cry.RightWyrmSid != 0)) { _events[3].Set(); accessory.Log.Debug($"Crystallize Time: Left and right Drachen Wanderers recorded."); }

                if (phase4DrachenWandererId1.Equals("")) phase4DrachenWandererId1 = @event["SourceId"];
                else if (phase4DrachenWandererId2.Equals("")) phase4DrachenWandererId2 = @event["SourceId"];
            }
        }

        [ScriptMethod(name: "Phase4 Hitbox Of Drachen Wanderers",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: ["DataId:17836"])]
        public void Phase4_Hitbox_Of_Drachen_Wanderers(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = $"Phase4_Hitbox_Of_Drachen_Wanderers_{sourceId}";
            currentProperty.Scale = new(2f, Phase4_Length_Of_Drachen_Wanderer_Hitboxes >= 0 ? Phase4_Length_Of_Drachen_Wanderer_Hitboxes : 1.5f);
            currentProperty.Color = Phase4_Colour_Of_Drachen_Wanderer_Hitboxes.V4.WithW(25f);
            currentProperty.Offset = new(0f, 0f, -1f);
            currentProperty.Owner = sourceId;
            currentProperty.DestoryAt = 34000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, currentProperty);
        }

        [ScriptMethod(name: "Phase4 Explosion Range Of Drachen Wanderers",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: ["DataId:17836"])]
        public void Phase4_Explosion_Range_Of_Drachen_Wanderers(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;

            _events[3].WaitOne();

            var myIndex = accessory.GetMyIndex();
            bool isSameSideWyrm = false;
            if (sourceId == _cry.LeftWyrmSid) isSameSideWyrm = (myIndex == _cry.LeftIcePlayerIdx) || (myIndex == _cry.LeftWindPlayerIdx);
            else if (sourceId == _cry.RightWyrmSid) isSameSideWyrm = (myIndex == _cry.RightIcePlayerIdx) || (myIndex == _cry.RightWindPlayerIdx);

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = $"Phase4_Explosion_Range_Of_Drachen_Wanderers_{sourceId}";
            currentProperty.Scale = isSameSideWyrm ? new(1.5f) : new(12);
            currentProperty.Owner = sourceId;
            currentProperty.Color = isSameSideWyrm ? accessory.Data.DefaultSafeColor.WithW(3f) : accessory.Data.DefaultDangerColor;
            currentProperty.DestoryAt = 34000;
            accessory.Method.SendDraw(isSameSideWyrm ? DrawModeEnum.Imgui : DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);
        }

        [ScriptMethod(name: "Phase4 Remove Hitboxes And Explosion Ranges Of Drachen Wanderers",
            eventType: EventTypeEnum.RemoveCombatant,
            eventCondition: ["DataId:17836"],
            userControl: false)]
        public void Phase4_Remove_Hitboxes_And_Explosion_Ranges_Of_Drachen_Wanderers(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;
            accessory.Method.RemoveDraw($"Phase4_Hitbox_Of_Drachen_Wanderers_{sourceId}");
            accessory.Method.RemoveDraw($"Phase4_Explosion_Range_Of_Drachen_Wanderers_{sourceId}");
        }

        [ScriptMethod(name: "Phase4 Remove Hitboxes And Explosion Ranges Of Drachen Wanderers In Advance",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:3263"],
            userControl: false)]
        public void Phase4_Remove_Hitboxes_And_Explosion_Ranges_Of_Drachen_Wanderers_In_Advance(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (!ParseObjectId(@event["TargetId"], out var targetId)) return;
            var targetObject = accessory.Data.Objects.SearchById(targetId);
            if (targetObject == null) return;
            if (targetObject.IsDead) return;

            System.Threading.Thread.MemoryBarrier();
            ++phase4WyrmclawRemovalCount;
            System.Threading.Thread.MemoryBarrier();

            if (phase4WyrmclawRemovalCount < 3 || phase4WyrmclawRemovalCount > 4) return;

            if (!ParseObjectId(phase4DrachenWandererId1, out var drachenWandererId1)) return;
            if (!ParseObjectId(phase4DrachenWandererId2, out var drachenWandererId2)) return;
            var drachenWandererObject1 = accessory.Data.Objects.SearchById(drachenWandererId1);
            var drachenWandererObject2 = accessory.Data.Objects.SearchById(drachenWandererId2);
            if (drachenWandererObject1 == null || drachenWandererObject2 == null) return;

            if (Vector3.Distance(targetObject.Position, drachenWandererObject1.Position) <= Vector3.Distance(targetObject.Position, drachenWandererObject2.Position))
            {
                accessory.Method.RemoveDraw($"Phase4_Hitbox_Of_Drachen_Wanderers_{drachenWandererId1}");
                accessory.Method.RemoveDraw($"Phase4_Explosion_Range_Of_Drachen_Wanderers_{drachenWandererId1}");
            }
            else
            {
                accessory.Method.RemoveDraw($"Phase4_Hitbox_Of_Drachen_Wanderers_{drachenWandererId2}");
                accessory.Method.RemoveDraw($"Phase4_Explosion_Range_Of_Drachen_Wanderers_{drachenWandererId2}");
            }
        }

        [ScriptMethod(name: "Phase4 Tidal Light",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:regex:^(40252|40253)$"])]
        public void Phase4_Tidal_Light(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Owner = sourceId;
            currentProperty.Offset = new Vector3(0, 0, -10);
            currentProperty.Scale = new(40, 10);
            currentProperty.DestoryAt = 2100;
            currentProperty.Color = Phase4_Colour_Of_Tidal_Light.V4.WithW(3f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);
        }

        [ScriptMethod(name: "Phase4 Determine Relative Positions Of Residues",
            eventType: EventTypeEnum.ObjectChanged,
            eventCondition: ["DataId:2014529"],
            userControl: false)]
        public void Phase4_Determine_Relative_Positions_Of_Residues(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (!@event["Operate"].Equals("Add")) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;

            var sourcePositionInJson = JObject.Parse(@event["SourcePosition"]);
            float currentX = sourcePositionInJson["X"]?.Value<float>() ?? 0;

            if (currentX < 100)
            {
                if (phase4ResidueIdsEastToWest[3] != 0) lock (phase4ResidueIdsEastToWest) { phase4ResidueIdsEastToWest[2] = sourceId; }
                else lock (phase4ResidueIdsEastToWest) { phase4ResidueIdsEastToWest[3] = sourceId; }
            }
            if (currentX > 100)
            {
                if (phase4ResidueIdsEastToWest[0] != 0) lock (phase4ResidueIdsEastToWest) { phase4ResidueIdsEastToWest[1] = sourceId; }
                else lock (phase4ResidueIdsEastToWest) { phase4ResidueIdsEastToWest[0] = sourceId; }
            }

            if (Enable_Developer_Mode)
            {
                accessory.Method.SendChat($"/e @event[\"SourceId\"]={@event["SourceId"]} sourceId={sourceId} @event[\"SourcePosition\"]={@event["SourcePosition"]} currentX={currentX}");
            }
        }

        [ScriptMethod(name: "Phase4 Guidance Of Residues",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:regex:^(40252|40253)$"])]
        public void Phase4_Guidance_Of_Residues(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (phase4ResidueGuidanceGenerated) return;

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Phase4_Relative_Positions_Of_Residues relativePositionOfMyResidue = phase4_getRelativePosition(myIndex);
            ulong idOfMyResidue = phase4_getResidueId(relativePositionOfMyResidue);

            if (Enable_Developer_Mode)
            {
                if (Phase4_Logic_Of_Residue_Guidance == Phase4_Logics_Of_Residue_Guidance.According_To_Signs_On_Me)
                {
                    accessory.Method.SendChat($"/e phase4ResidueIdsEastToWest[]={phase4ResidueIdsEastToWest[0]},{phase4ResidueIdsEastToWest[1]},{phase4ResidueIdsEastToWest[2]},{phase4ResidueIdsEastToWest[3]} phase4WyrmfangPlayerMarks[myIndex]={phase4WyrmfangPlayerMarks[myIndex]} relativePositionOfMyResidue={relativePositionOfMyResidue} idOfMyResidue={idOfMyResidue}");
                }
                else
                {
                    accessory.Method.SendChat($"/e phase4ResidueIdsEastToWest[]={phase4ResidueIdsEastToWest[0]},{phase4ResidueIdsEastToWest[1]},{phase4ResidueIdsEastToWest[2]},{phase4ResidueIdsEastToWest[3]} p4WyrmclawBuff={p4WyrmclawBuff[myIndex]} p4OtherBuff={p4OtherBuff[myIndex]} relativePositionOfMyResidue={relativePositionOfMyResidue} idOfMyResidue={idOfMyResidue}");
                }
            }

            if (relativePositionOfMyResidue != Phase4_Relative_Positions_Of_Residues.Unknown && idOfMyResidue != 0)
            {
                var currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase4_Guidance_Of_Residues";
                currentProperty.Scale = new(2);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.Color = Phase4_Colour_Of_Residue_Guidance.V4.WithW(1f);
                currentProperty.DestoryAt = 23000;

                var residueObject = accessory.Data.Objects.SearchById(idOfMyResidue);
                if (residueObject != null)
                {
                    phase4ResidueGuidanceGenerated = true;
                    currentProperty.TargetPosition = residueObject.Position;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    if (Enable_Text_Prompts) accessory.Method.TextInfo(phase4_getResidueDescription(relativePositionOfMyResidue), 2500);
                    accessory.TTS($"{phase4_getResidueDescription(relativePositionOfMyResidue)}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                    if (Enable_Developer_Mode) accessory.Method.SendChat($"/e residueObject.Position={residueObject.Position}");
                }
            }
        }

        [ScriptMethod(name: "Phase4 Remove Guidance Of Residues",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:3264"],
            userControl: false)]
        public void Phase4_Remove_Guidance_Of_Residues(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (!ParseObjectId(@event["TargetId"], out var targetId)) return;
            if (targetId != accessory.Data.Me) return;
            accessory.Method.RemoveDraw("Phase4_Guidance_Of_Residues");
        }

        [ScriptMethod(name: "Phase4 Highlight Of Residues",
            eventType: EventTypeEnum.ObjectChanged,
            eventCondition: ["DataId:2014529"])]
        public void Phase4_Highlight_Of_Residues(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (!@event["Operate"].Equals("Add")) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = $"Phase4_Highlight_Of_Residues_{sourceId}";
            currentProperty.Scale = new(1f);
            currentProperty.InnerScale = new(0.8f);
            currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(25f);
            currentProperty.Radian = float.Pi * 2;
            currentProperty.Owner = sourceId;
            currentProperty.DestoryAt = 17000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, currentProperty);
        }

        [ScriptMethod(name: "Phase4 Remove Highlights Of Residues",
            eventType: EventTypeEnum.ObjectChanged,
            eventCondition: ["DataId:2014529"],
            userControl: false)]
        public void Phase4_Remove_Highlights_Of_Residues(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (!@event["Operate"].Equals("Remove")) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;
            accessory.Method.RemoveDraw($"Phase4_Highlight_Of_Residues_{sourceId}");

            ulong idOfMyResidue = phase4_getResidueId(phase4_getRelativePosition(accessory.Data.PartyList.IndexOf(accessory.Data.Me)));
            if (idOfMyResidue != 0 && idOfMyResidue == sourceId) accessory.Method.RemoveDraw("Phase4_Guidance_Of_Residues");
        }

        [ScriptMethod(name: "Phase4 Remove Highlights Of Residues In Advance",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:3264"],
            userControl: false)]
        public void Phase4_Remove_Highlights_Of_Residues_In_Advance(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (!ParseObjectId(@event["TargetId"], out var targetId)) return;
            var targetObject = accessory.Data.Objects.SearchById(targetId);
            if (targetObject == null) return;
            Vector3 targetPosition = targetObject.Position;
            if (targetObject.IsDead) return;

            int closestResidue = -1;
            float distanceToTheClosestResidue = float.PositiveInfinity;
            for (int i = 0; i < 4; ++i)
            {
                var residueObject = accessory.Data.Objects.SearchById(phase4ResidueIdsEastToWest[i]);
                if (residueObject != null)
                {
                    if (Vector3.Distance(targetPosition, residueObject.Position) < distanceToTheClosestResidue)
                    {
                        closestResidue = i;
                        distanceToTheClosestResidue = Vector3.Distance(targetPosition, residueObject.Position);
                    }
                }
            }

            if (0 <= closestResidue && closestResidue <= 3)
            {
                accessory.Method.RemoveDraw($"Phase4_Highlight_Of_Residues_{phase4ResidueIdsEastToWest[closestResidue]}");
                if (targetId != accessory.Data.Me)
                {
                    ulong idOfMyResidue = phase4_getResidueId(phase4_getRelativePosition(accessory.Data.PartyList.IndexOf(accessory.Data.Me)));
                    if (idOfMyResidue != 0 && idOfMyResidue == phase4ResidueIdsEastToWest[closestResidue]) accessory.Method.RemoveDraw("Phase4_Guidance_Of_Residues");
                }
            }
        }

        [ScriptMethod(name: "Phase4 Record Signs On Party Members",
            eventType: EventTypeEnum.Marker,
            userControl: false)]
        public void Phase4_Record_Signs_On_Party_Members(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            if (!ParseObjectId(@event["TargetId"], out var targetId)) return;
            if (!int.TryParse(@event["Id"], out var sign)) return;

            MarkType currentType = sign switch
            {
                1 => MarkType.Attack1, 2 => MarkType.Attack2, 3 => MarkType.Attack3, 4 => MarkType.Attack4,
                9 => MarkType.Stop1, 10 => MarkType.Stop2, 6 => MarkType.Bind1, 7 => MarkType.Bind2,
                _ => MarkType.Cross
            };

            int currentIndex = accessory.Data.PartyList.IndexOf(((uint)targetId));
            if (0 <= currentIndex && currentIndex <= 7) lock (phase4WyrmfangPlayerMarks) { phase4WyrmfangPlayerMarks[currentIndex] = currentType; }
        }

        private Phase4_Relative_Positions_Of_Residues phase4_getRelativePosition(int currentIndex)
        {
            if (currentIndex < 0 || currentIndex > 7) return Phase4_Relative_Positions_Of_Residues.Unknown;
            if (p4WyrmclawBuff[currentIndex] == 1 || p4WyrmclawBuff[currentIndex] == 2) return Phase4_Relative_Positions_Of_Residues.Unknown;

            if (Phase4_Logic_Of_Residue_Guidance == Phase4_Logics_Of_Residue_Guidance.According_To_Debuffs)
            {
                if (p4WyrmclawBuff[currentIndex] == 3)
                {
                    if (p4OtherBuff[currentIndex] == 4) return Phase4_Residue_Belongs_To_Dark_Eruption;
                    if (p4OtherBuff[currentIndex] == 5) return Phase4_Residue_Belongs_To_Unholy_Darkness;
                    if (p4OtherBuff[currentIndex] == 1) return Phase4_Residue_Belongs_To_Dark_Blizzard_III;
                    if (p4OtherBuff[currentIndex] == 3) return Phase4_Residue_Belongs_To_Dark_Water_III;
                }
            }

            if (Phase4_Logic_Of_Residue_Guidance == Phase4_Logics_Of_Residue_Guidance.According_To_Signs_On_Me)
            {
                if (p4WyrmclawBuff[currentIndex] == 3)
                {
                    if (phase4WyrmfangPlayerMarks[currentIndex] == MarkType.Attack1) return Phase4_Residue_Belongs_To_Attack1;
                    if (phase4WyrmfangPlayerMarks[currentIndex] == MarkType.Attack2) return Phase4_Residue_Belongs_To_Attack2;
                    if (phase4WyrmfangPlayerMarks[currentIndex] == MarkType.Attack3) return Phase4_Residue_Belongs_To_Attack3;
                    if (phase4WyrmfangPlayerMarks[currentIndex] == MarkType.Attack4) return Phase4_Residue_Belongs_To_Attack4;
                }
            }
            return Phase4_Relative_Positions_Of_Residues.Unknown;
        }

        private ulong phase4_getResidueId(Phase4_Relative_Positions_Of_Residues relativePosition)
        {
            switch (relativePosition)
            {
                case Phase4_Relative_Positions_Of_Residues.Eastmost: return phase4ResidueIdsEastToWest[0];
                case Phase4_Relative_Positions_Of_Residues.About_East: return phase4ResidueIdsEastToWest[1];
                case Phase4_Relative_Positions_Of_Residues.About_West: return phase4ResidueIdsEastToWest[2];
                case Phase4_Relative_Positions_Of_Residues.Westmost: return phase4ResidueIdsEastToWest[3];
                default: return 0;
            }
        }

        private String phase4_getResidueDescription(Phase4_Relative_Positions_Of_Residues relativePosition)
        {
            switch (relativePosition)
            {
                case Phase4_Relative_Positions_Of_Residues.Eastmost: return "Leftmost/Eastmost";
                case Phase4_Relative_Positions_Of_Residues.About_East: return "About left/About east";
                case Phase4_Relative_Positions_Of_Residues.About_West: return "About right/About west";
                case Phase4_Relative_Positions_Of_Residues.Westmost: return "Rightmost/Westmost";
                default: return "";
            }
        }

        [ScriptMethod(name: "Phase2 Reset Semaphores After Crystallize Time",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40332"],
            userControl: false,
            suppress: 10000)]
        public void Phase2_Reset_Semaphores_After_Crystallize_Time(Event @event, ScriptAccessory accessory)
        {
            if (parse != 43) return;
            phase4MajorDebuffsConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
            phase4IncidentalDebuffsConfirmedSemaphore = new System.Threading.AutoResetEvent(false);
            if (Phase4_Mark_Players_During_The_Second_Half) accessory.Method.MarkClear();
        }

        #endregion

        #region Phase_5

        [ScriptMethod(name: "----- Phase 5 -----",
            eventType: EventTypeEnum.NpcYell,
            eventCondition: ["I lift my lamp beside the golden door!"])]
        public void Phase5_Placeholder(Event @event, ScriptAccessory accessory) { }

        [ScriptMethod(name: "Phase5 Initialization",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: ["DataId:17839"],
            userControl: false)]
        public void Phase5_Initialization(Event @event, ScriptAccessory accessory)
        {
            phase5BossId = @event["SourceId"];
            phase5FirstTowerAcquired = false;
            phase5FirstTowerIndex = "";
            phase5InitialPositionConfirmed = false;
            System.Threading.Thread.MemoryBarrier();
            isInPhase5 = true;
        }

        [ScriptMethod(name: "Phase5 Destruction",
            eventType: EventTypeEnum.RemoveCombatant,
            eventCondition: ["DataId:17839"],
            userControl: false)]
        public void Phase5_Destruction(Event @event, ScriptAccessory accessory)
        {
            isInPhase5 = false;
            System.Threading.Thread.MemoryBarrier();
            phase5BossId = "";
            phase5FirstTowerAcquired = false;
            phase5FirstTowerIndex = "";
            phase5InitialPositionConfirmed = false;
        }

        [ScriptMethod(name: "P5_Fulgent_Blade", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40118|40307)$"])]
        public void P5_Fulgent_Blade(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_Fulgent_Blade";
            dp.Scale = new(80, 5);
            dp.Owner = sid;
            dp.Color = Phase5_Colour_Of_Fulgent_Blade.V4.WithW(1f);
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P5_Fulgent_Blade_Advance_{@event["SourceId"]}";
            dp.Scale = new(80, 5);
            dp.Offset = new(0, 0, -5);
            dp.Owner = sid;
            dp.Color = Phase5_Colour_Of_Fulgent_Blade.V4.WithW(1f);
            dp.Delay = 7000;
            dp.DestoryAt = 20000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "P5_Fulgent_Blade_Elimination", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(40118|4030[789])$"], userControl: false)]
        public void P5_Fulgent_Blade_Elimination(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5) return;
            if (!float.TryParse(@event["SourceRotation"], out var rot)) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            Vector3 centre = new(100, 0, 100);
            Vector3 posNext = new(pos.X + 5 * MathF.Sin(rot), 0, pos.Z + 5 * MathF.Cos(rot));
            if ((posNext - centre).Length() > 20) accessory.Method.RemoveDraw($"P5_Fulgent_Blade_Advance_{@event["SourceId"]}");
        }

        [ScriptMethod(name: "Phase5 Guidance Of Fulgent Blade", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id2:16"])]
        public void Phase5_Guidance_Of_Fulgent_Blade(Event @event, ScriptAccessory accessory)
        {
            if (Phase == "P5 Fulgent Blade Calculation Complete")
            {
                lock (drawLock)
                {
                    Phase = "P5 Calculation Complete";
                    var id = Convert.ToUInt32(@event["SourceId"], 16);
                    Vector2 FarthestPoint = new Vector2();
                    Vector2 ClosestPoint = new Vector2();
                    if (id == p1p3Blades[0].Id || id == p1p3Blades[1].Id)
                    {
                        FarthestPoint = FindFarthestPoint(OnPoint, Point1);
                        ClosestPoint = FindClosestPoint(OnPoint, Point1);
                    }
                    else if (id == p1p3Blades[2].Id || id == p1p3Blades[3].Id)
                    {
                        FarthestPoint = FindFarthestPoint(OnPoint, Point3);
                        ClosestPoint = FindClosestPoint(OnPoint, Point3);
                    }

                    BladeRoutes.Insert(0, FarthestPoint);
                    BladeRoutes.Insert(1, ClosestPoint);
                    BladeRoutes.Insert(2, FindFarthestPoint(OnPoint, Point2));
                    BladeRoutes.Insert(3, FindClosestPoint(OnPoint, Point2));
                    BladeRoutes.Insert(4, ClosestPoint);
                    BladeRoutes.Insert(5, FarthestPoint);

                    int BladeTimes = 2000;

                    var Goline0 = accessory.Data.GetDefaultDrawProperties();
                    Goline0.Owner = accessory.Data.Me;
                    Goline0.DestoryAt = 9000;
                    Goline0.Color = Phase5_Colour_Of_The_Current_Guidance_Step.V4;
                    Goline0.Scale = new(2);
                    Goline0.ScaleMode |= ScaleMode.YByDistance;
                    Goline0.TargetPosition = Vector3Fucker(BladeRoutes[0]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline0);

                    var line1 = accessory.Data.GetDefaultDrawProperties();
                    line1.Position = Vector3Fucker(BladeRoutes[0]);
                    line1.DestoryAt = 9000;
                    line1.Color = Phase5_Colour_Of_The_Next_Guidance_Step.V4;
                    line1.Scale = new(2);
                    line1.ScaleMode |= ScaleMode.YByDistance;
                    line1.TargetPosition = Vector3Fucker(BladeRoutes[1]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line1);

                    var Goline1 = accessory.Data.GetDefaultDrawProperties();
                    Goline1.Owner = accessory.Data.Me;
                    Goline1.Delay = 9000;
                    Goline1.DestoryAt = BladeTimes;
                    Goline1.Color = Phase5_Colour_Of_The_Current_Guidance_Step.V4;
                    Goline1.Scale = new(2);
                    Goline1.ScaleMode |= ScaleMode.YByDistance;
                    Goline1.TargetPosition = Vector3Fucker(BladeRoutes[1]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline1);

                    var line2 = accessory.Data.GetDefaultDrawProperties();
                    line2.Position = Vector3Fucker(BladeRoutes[1]);
                    line2.Delay = 9000;
                    line2.DestoryAt = BladeTimes;
                    line2.Color = Phase5_Colour_Of_The_Next_Guidance_Step.V4;
                    line2.Scale = new(2);
                    line2.ScaleMode |= ScaleMode.YByDistance;
                    line2.TargetPosition = Vector3Fucker(BladeRoutes[2]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line2);

                    var Goline2 = accessory.Data.GetDefaultDrawProperties();
                    Goline2.Owner = accessory.Data.Me;
                    Goline2.Delay = 9000 + BladeTimes;
                    Goline2.DestoryAt = BladeTimes;
                    Goline2.Color = Phase5_Colour_Of_The_Current_Guidance_Step.V4;
                    Goline2.Scale = new(2);
                    Goline2.ScaleMode |= ScaleMode.YByDistance;
                    Goline2.TargetPosition = Vector3Fucker(BladeRoutes[2]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline2);

                    var line3 = accessory.Data.GetDefaultDrawProperties();
                    line3.Position = Vector3Fucker(BladeRoutes[2]);
                    line3.Delay = 9000 + BladeTimes;
                    line3.DestoryAt = BladeTimes;
                    line3.Color = Phase5_Colour_Of_The_Next_Guidance_Step.V4;
                    line3.Scale = new(2);
                    line3.ScaleMode |= ScaleMode.YByDistance;
                    line3.TargetPosition = Vector3Fucker(BladeRoutes[3]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line3);

                    var Goline3 = accessory.Data.GetDefaultDrawProperties();
                    Goline3.Owner = accessory.Data.Me;
                    Goline3.Delay = 9000 + BladeTimes * 2;
                    Goline3.DestoryAt = BladeTimes;
                    Goline3.Color = Phase5_Colour_Of_The_Current_Guidance_Step.V4;
                    Goline3.Scale = new(2);
                    Goline3.ScaleMode |= ScaleMode.YByDistance;
                    Goline3.TargetPosition = Vector3Fucker(BladeRoutes[3]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline3);

                    var line4 = accessory.Data.GetDefaultDrawProperties();
                    line4.Position = Vector3Fucker(BladeRoutes[3]);
                    line4.Delay = 9000 + BladeTimes * 2;
                    line4.DestoryAt = BladeTimes;
                    line4.Color = Phase5_Colour_Of_The_Next_Guidance_Step.V4;
                    line4.Scale = new(2);
                    line4.ScaleMode |= ScaleMode.YByDistance;
                    line4.TargetPosition = Vector3Fucker(BladeRoutes[4]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line4);

                    var Goline4 = accessory.Data.GetDefaultDrawProperties();
                    Goline4.Owner = accessory.Data.Me;
                    Goline4.Delay = 9000 + BladeTimes * 3;
                    Goline4.DestoryAt = BladeTimes;
                    Goline4.Color = Phase5_Colour_Of_The_Current_Guidance_Step.V4;
                    Goline4.Scale = new(2);
                    Goline4.ScaleMode |= ScaleMode.YByDistance;
                    Goline4.TargetPosition = Vector3Fucker(BladeRoutes[4]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline4);

                    var line5 = accessory.Data.GetDefaultDrawProperties();
                    line5.Position = Vector3Fucker(BladeRoutes[4]);
                    line5.Delay = 9000 + BladeTimes * 3;
                    line5.DestoryAt = BladeTimes;
                    line5.Color = Phase5_Colour_Of_The_Next_Guidance_Step.V4;
                    line5.Scale = new(2);
                    line5.ScaleMode |= ScaleMode.YByDistance;
                    line5.TargetPosition = Vector3Fucker(BladeRoutes[5]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line5);

                    var Goline5 = accessory.Data.GetDefaultDrawProperties();
                    Goline5.Owner = accessory.Data.Me;
                    Goline5.Delay = 9000 + BladeTimes * 4;
                    Goline5.DestoryAt = BladeTimes;
                    Goline5.Color = Phase5_Colour_Of_The_Current_Guidance_Step.V4;
                    Goline5.Scale = new(2);
                    Goline5.ScaleMode |= ScaleMode.YByDistance;
                    Goline5.TargetPosition = Vector3Fucker(BladeRoutes[5]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline5);
                }
            }
        }

        [ScriptMethod(name: "Phase5 Boss Central Axis After Fulgent Blade",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40310"])]
        public void Phase5_Boss_Central_Axis_After_Fulgent_Blade(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase5_Boss_Front_Axis_After_Fulgent_Blade";
            currentProperty.Owner = sourceId;
            currentProperty.Scale = new(0.5f, 10);
            currentProperty.Color = Phase5_Colour_Of_The_Boss_Central_Axis.V4.WithW(25f);
            currentProperty.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase5_Boss_Rear_Axis_After_Fulgent_Blade";
            currentProperty.Owner = sourceId;
            currentProperty.Scale = new(0.5f, 10);
            currentProperty.Rotation = float.Pi;
            currentProperty.Color = Phase5_Colour_Of_The_Boss_Central_Axis.V4.WithW(25f);
            currentProperty.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);
        }

        [ScriptMethod(name: "Phase5 Side To Stack After Fulgent Blade",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40310"])]
        public void Phase5_Side_To_Stack_After_Fulgent_Blade(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myIndex < 0 || myIndex > 7) return;

            bool goLeft = (myIndex == 0 || myIndex == 2 || myIndex == 4 || myIndex == 6);

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase5_Left_Side_After_Fulgent_Blade";
            currentProperty.Owner = sourceId;
            currentProperty.Scale = new(4);
            currentProperty.Radian = float.Pi;
            currentProperty.Rotation = float.Pi / 2;
            currentProperty.Offset = new Vector3(-0.25f, 0, 0);
            currentProperty.DestoryAt = 9000;

            if (Phase5_Boss_Faces_Players_After_Fulgent_Blade) currentProperty.Color = goLeft ? accessory.Data.DefaultDangerColor.WithW(25f) : accessory.Data.DefaultSafeColor.WithW(25f);
            else currentProperty.Color = goLeft ? accessory.Data.DefaultSafeColor.WithW(25f) : accessory.Data.DefaultDangerColor.WithW(25f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase5_Right_Side_After_Fulgent_Blade";
            currentProperty.Owner = sourceId;
            currentProperty.Scale = new(4);
            currentProperty.Radian = float.Pi;
            currentProperty.Rotation = -(float.Pi / 2);
            currentProperty.Offset = new Vector3(0.25f, 0, 0);
            currentProperty.DestoryAt = 9000;

            if (Phase5_Boss_Faces_Players_After_Fulgent_Blade) currentProperty.Color = goLeft ? accessory.Data.DefaultSafeColor.WithW(25f) : accessory.Data.DefaultDangerColor.WithW(25f);
            else currentProperty.Color = goLeft ? accessory.Data.DefaultDangerColor.WithW(25f) : accessory.Data.DefaultSafeColor.WithW(25f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);

            if (Enable_Text_Prompts)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.Method.TextInfo(((goLeft) ? ("Stack on the left") : ("Stack on the right")), 9000);
            }
            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.TTS($"{((goLeft) ? ("Stack on the left") : ("Stack on the right"))}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        [ScriptMethod(name: "Phase5 Initialization Of Wings Dark And Light",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40319"],
            userControl: false)]
        public void Phase5_Initialization_Of_Wings_Dark_And_Light(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5) return;
            phase5FirstTowerAcquired = false;
            phase5FirstTowerIndex = "";
            phase5InitialPositionConfirmed = false;
        }

        [ScriptMethod(name: "P5_Wings_Dark_And_Light", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40313|40233)$"])]
        public void P5_Wings_Dark_And_Light(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var r = 225f;
            var rot = (180 - r / 2) / 180f * float.Pi;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_Wings_Dark_And_Light";
            dp.Scale = new(20);
            dp.Owner = sid;
            dp.Radian = r / 180 * float.Pi;
            dp.TargetObject = accessory.Data.EnmityList[sid][0];
            dp.Rotation = @event["ActionId"] == "40313" ? rot : -rot;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_Wings_Dark_And_Light_Far_Close";
            dp.Scale = new(4);
            dp.Owner = sid;
            dp.CentreResolvePattern = @event["ActionId"] == "40313" ? PositionResolvePatternEnum.PlayerFarestOrder : PositionResolvePatternEnum.PlayerNearestOrder;
            dp.Rotation = @event["ActionId"] == "40313" ? rot : -rot;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_Wings_Dark_And_Light";
            dp.Scale = new(20);
            dp.Owner = sid;
            dp.Radian = r / 180 * float.Pi;
            dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
            dp.Rotation = @event["ActionId"] == "40313" ? -rot : rot;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7300;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_Wings_Dark_And_Light_Far_Close";
            dp.Scale = new(4);
            dp.Owner = sid;
            dp.CentreResolvePattern = @event["ActionId"] == "40313" ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
            dp.Rotation = @event["ActionId"] == "40313" ? rot : -rot;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7300;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Phase5 Acquire The First Tower Of Wings Dark And Light",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:00010004", "Index:regex:^(0000003[012])"],
            userControl: false)]
        public void Phase5_Acquire_The_First_Tower_Of_Wings_Dark_And_Light(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5) return;
            if (!phase5FirstTowerAcquired) { phase5FirstTowerIndex = @event["Index"]; phase5FirstTowerAcquired = true; }
        }

        [ScriptMethod(name: "Phase5 Initial Position Of The Current MT Before Towers",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:00010004", "Index:regex:^(0000003[012])"])]
        public void Phase5_Initial_Position_Of_The_Current_MT_Before_Towers(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5) return;
            if (phase5InitialPositionConfirmed) return;
            else phase5InitialPositionConfirmed = true;

            if (!ParseObjectId(phase5BossId, out var bossId)) return;
            if (!accessory.Data.EnmityList.TryGetValue(bossId, out var enmityListOfBoss)) return;

            if (Enable_Developer_Mode) accessory.Method.SendChat($"/e accessory.Data.Me={accessory.Data.Me} enmityListOfTheBoss[0]={enmityListOfBoss[0]}");
            if (accessory.Data.Me != enmityListOfBoss[0]) return;

            while (!phase5FirstTowerAcquired) System.Threading.Thread.Sleep(1);
            System.Threading.Thread.MemoryBarrier();

            Vector3 positionOfTheFirstTower = new Vector3(100, 0, 100);
            if (phase5FirstTowerIndex.Equals("00000030")) positionOfTheFirstTower = new Vector3(93.94f, 0, 96.50f);
            if (phase5FirstTowerIndex.Equals("00000031")) positionOfTheFirstTower = new Vector3(106.06f, 0, 96.50f);
            if (phase5FirstTowerIndex.Equals("00000032")) positionOfTheFirstTower = new Vector3(100f, 0, 107f);
            if (positionOfTheFirstTower.Equals(new Vector3(100, 0, 100))) return;

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase5_Initial_Position_Of_The_Current_MT_Before_Towers";
            currentProperty.Scale = new(2);
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = RotatePoint(positionOfTheFirstTower, new Vector3(100, 0, 100), float.Pi);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 2300;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
        }

        [ScriptMethod(name: "Phase5 Guidance For Tanks During Towers",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(40313|40233)$"])]
        public void Phase5_Guidance_For_Tanks_During_Towers(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5) return;
            if (!phase5FirstTowerAcquired) return;
            if (accessory.Data.PartyList.IndexOf(accessory.Data.Me) != 0 && accessory.Data.PartyList.IndexOf(accessory.Data.Me) != 1) return;

            bool isCurrentMt = true;
            if (!ParseObjectId(phase5BossId, out var bossId)) return;
            if (!accessory.Data.EnmityList.TryGetValue(bossId, out var enmityListOfBoss)) return;

            if (Enable_Developer_Mode) accessory.Method.SendChat($"/e accessory.Data.Me={accessory.Data.Me} enmityListOfTheBoss[0]={enmityListOfBoss[0]}");
            isCurrentMt = (accessory.Data.Me == enmityListOfBoss[0]);

            bool isLeftFirstAndFarFirst = (@event["ActionId"].Equals("40313"));

            Vector3 positionOfTheFirstTower = new Vector3(100, 0, 100);
            if (phase5FirstTowerIndex.Equals("00000030")) positionOfTheFirstTower = new Vector3(93.94f, 0, 96.50f);
            if (phase5FirstTowerIndex.Equals("00000031")) positionOfTheFirstTower = new Vector3(106.06f, 0, 96.50f);
            if (phase5FirstTowerIndex.Equals("00000032")) positionOfTheFirstTower = new Vector3(100f, 0, 107f);
            if (positionOfTheFirstTower.Equals(new Vector3(100, 0, 100))) return;

            if (Phase5_Strat_Of_Wings_Dark_And_Light == Phase5_Strats_Of_Wings_Dark_And_Light.Grey9_Brain_Dead_MT_First_Tower_Opposite)
            {
                Vector3 position1OfCurrentMt = RotatePoint(positionOfTheFirstTower, new Vector3(100, 0, 100), float.Pi);
                Vector3 position2OfCurrentMt = (isLeftFirstAndFarFirst) ? (new((position1OfCurrentMt.X - 100) / 7 + 100, 0, (position1OfCurrentMt.Z - 100) / 7 + 100)) : (new((position1OfCurrentMt.X - 100) / 7 * 18 + 100, 0, (position1OfCurrentMt.Z - 100) / 7 * 18 + 100));
                Vector3 position2OfCurrentOt = RotatePoint(position1OfCurrentMt, new(100, 0, 100), (isLeftFirstAndFarFirst) ? (convertDegree(120f)) : (convertDegree(-120f)));
                Vector3 position1OfCurrentOt = (isLeftFirstAndFarFirst) ? (new((position2OfCurrentOt.X - 100) / 7 * 18 + 100, 0, (position2OfCurrentOt.Z - 100) / 7 * 18 + 100)) : (new((position2OfCurrentOt.X - 100) / 7 + 100, 0, (position2OfCurrentOt.Z - 100) / 7 + 100));

                if (isCurrentMt)
                {
                    var currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Guidance_1_For_The_Current_MT_During_Towers";
                    currentProperty.Scale = new(2);
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = position1OfCurrentMt;
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Color = accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt = 7150;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Guidance_2_Preview_For_The_Current_MT_During_Towers";
                    currentProperty.Scale = new(2);
                    currentProperty.Position = position1OfCurrentMt;
                    currentProperty.TargetPosition = position2OfCurrentMt;
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Color = accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt = 7150;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Guidance_2_For_The_Current_MT_During_Towers";
                    currentProperty.Scale = new(2);
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = position2OfCurrentMt;
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Color = accessory.Data.DefaultSafeColor;
                    currentProperty.Delay = 7150;
                    currentProperty.DestoryAt = 4250;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    if (Phase5_Reminder_To_Provoke_During_Wings_Dark_And_Light)
                    {
                        System.Threading.Thread.Sleep(1500);
                        if (Enable_Text_Prompts)
                        {
                            if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.Method.TextInfo("Wait for provocation then shirk", 2500);
                        }
                        if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                        {
                            if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.TTS("Wait for provocation then shirk", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
                        }
                    }
                }
                else
                {
                    var currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Guidance_1_For_The_Current_OT_During_Towers";
                    currentProperty.Scale = new(2);
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = position1OfCurrentOt;
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Color = accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt = 7650;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Guidance_2_Preview_For_The_Current_OT_During_Towers";
                    currentProperty.Scale = new(2);
                    currentProperty.Position = position1OfCurrentOt;
                    currentProperty.TargetPosition = position2OfCurrentOt;
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Color = accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt = 7650;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Guidance_2_For_The_Current_OT_During_Towers";
                    currentProperty.Scale = new(2);
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = position2OfCurrentOt;
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Color = accessory.Data.DefaultSafeColor;
                    currentProperty.Delay = 7650;
                    currentProperty.DestoryAt = 3750;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    if (Phase5_Reminder_To_Provoke_During_Wings_Dark_And_Light)
                    {
                        System.Threading.Thread.Sleep(1000);
                        if (Enable_Text_Prompts)
                        {
                            if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.Method.TextInfo("Now provoke!", 2500);
                        }
                        if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                        {
                            if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.TTS("Now provoke!", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
                        }
                    }
                }
            }

            if (Phase5_Strat_Of_Wings_Dark_And_Light == Phase5_Strats_Of_Wings_Dark_And_Light.Reverse_Triangle_MT_Baits_In_Towers)
            {
                Vector3 positionOfTheLeftTower = RotatePoint(positionOfTheFirstTower, new Vector3(100, 0, 100), float.Pi / 3 * 2);
                Vector3 positionOfTheRightTower = RotatePoint(positionOfTheFirstTower, new Vector3(100, 0, 100), -(float.Pi / 3 * 2));
                Vector3 oppositeOfTheFirstTower = RotatePoint(positionOfTheFirstTower, new Vector3(100, 0, 100), float.Pi);

                Vector3 position1OfCurrentMt = (isLeftFirstAndFarFirst) ? (positionOfTheRightTower) : (positionOfTheLeftTower);
                Vector3 position2OfCurrentMt = (isLeftFirstAndFarFirst) ? new Vector3((oppositeOfTheFirstTower.X - 100) / 7 + 100, 0, (oppositeOfTheFirstTower.Z - 100) / 7 + 100) : new Vector3((position1OfCurrentMt.X - 100) / 7 * 18 + 100, 0, (position1OfCurrentMt.Z - 100) / 7 * 18 + 100);
                Vector3 position2OfCurrentOt = (isLeftFirstAndFarFirst) ? (RotatePoint(positionOfTheLeftTower, new Vector3(100, 0, 100), float.Pi)) : (RotatePoint(positionOfTheRightTower, new Vector3(100, 0, 100), float.Pi));
                Vector3 position1OfCurrentOt = (isLeftFirstAndFarFirst) ? new Vector3((position2OfCurrentOt.X - 100) / 7 * 18 + 100, 0, (position2OfCurrentOt.Z - 100) / 7 * 18 + 100) : new Vector3((positionOfTheFirstTower.X - 100) / 7 + 100, 0, (positionOfTheFirstTower.Z - 100) / 7 + 100);

                if (isCurrentMt)
                {
                    var currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Guidance_1_For_The_Current_MT_During_Towers";
                    currentProperty.Scale = new(2);
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = position1OfCurrentMt;
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Color = accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt = 7150;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Guidance_2_Preview_For_The_Current_MT_During_Towers";
                    currentProperty.Scale = new(2);
                    currentProperty.Position = position1OfCurrentMt;
                    currentProperty.TargetPosition = position2OfCurrentMt;
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Color = accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt = 7150;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Guidance_2_For_The_Current_MT_During_Towers";
                    currentProperty.Scale = new(2);
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = position2OfCurrentMt;
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Color = accessory.Data.DefaultSafeColor;
                    currentProperty.Delay = 7150;
                    currentProperty.DestoryAt = 4250;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    if (Phase5_Reminder_To_Provoke_During_Wings_Dark_And_Light)
                    {
                        System.Threading.Thread.Sleep(1500);
                        if (Enable_Text_Prompts)
                        {
                            if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.Method.TextInfo("Wait for provocation then shirk", 2500);
                        }
                        if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                        {
                            if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.TTS("Wait for provocation then shirk", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
                        }
                    }
                }
                else
                {
                    var currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Guidance_1_For_The_Current_OT_During_Towers";
                    currentProperty.Scale = new(2);
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = position1OfCurrentOt;
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Color = accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt = 7650;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Guidance_2_Preview_For_The_Current_OT_During_Towers";
                    currentProperty.Scale = new(2);
                    currentProperty.Position = position1OfCurrentOt;
                    currentProperty.TargetPosition = position2OfCurrentOt;
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Color = accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt = 7650;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Guidance_2_For_The_Current_OT_During_Towers";
                    currentProperty.Scale = new(2);
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = position2OfCurrentOt;
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Color = accessory.Data.DefaultSafeColor;
                    currentProperty.Delay = 7650;
                    currentProperty.DestoryAt = 3750;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    if (Phase5_Reminder_To_Provoke_During_Wings_Dark_And_Light)
                    {
                        System.Threading.Thread.Sleep(1000);
                        if (Enable_Text_Prompts)
                        {
                            if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.Method.TextInfo("Now provoke!", 2500);
                        }
                        if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                        {
                            if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.TTS("Now provoke!", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
                        }
                    }
                }
            }
        }

        [ScriptMethod(name: "Phase5 Guidance For Others During Towers",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(40313|40233)$"])]
        public void Phase5_Guidance_For_Others_During_Towers(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5) return;
            if (!phase5FirstTowerAcquired) return;
            if (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 0 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 1) return;

            bool isLeftFirstAndFarFirst = (@event["ActionId"].Equals("40313"));

            if (Phase5_Strat_Of_Wings_Dark_And_Light == Phase5_Strats_Of_Wings_Dark_And_Light.Grey9_Brain_Dead_MT_First_Tower_Opposite)
            {
                float rotation = 0;
                if (phase5FirstTowerIndex.Equals("00000030")) rotation = float.Pi / 3 * 2;
                if (phase5FirstTowerIndex.Equals("00000031")) rotation = -(float.Pi / 3 * 2);
                if (phase5FirstTowerIndex.Equals("00000032")) rotation = 0;

                Vector3 positionOfTheFirstTower = RotatePoint(new Vector3(100, 0, 107), new Vector3(100, 0, 100), rotation);
                Vector3 positionOfTheLeftTower = RotatePoint(positionOfTheFirstTower, new Vector3(100, 0, 100), float.Pi / 3 * 2);
                Vector3 positionOfTheRightTower = RotatePoint(positionOfTheFirstTower, new Vector3(100, 0, 100), -(float.Pi / 3 * 2));
                Vector3 leftOfTheFirstTower = RotatePoint(phase5LeftSouthPosition, new Vector3(100, 0, 100), rotation);
                Vector3 rightOfTheFirstTower = RotatePoint(phase5RightSouthPosition, new Vector3(100, 0, 100), rotation);
                Vector3 leftOfTheLeftTower = RotatePoint(phase5LeftNorthwestPosition, new Vector3(100, 0, 100), rotation);
                Vector3 rightOfTheRightTower = RotatePoint(phase5RightNortheastPosition, new Vector3(100, 0, 100), rotation);
                Vector3 oppositeStandbyPosition = RotatePoint(positionOfTheFirstTower, new Vector3(100, 0, 100), float.Pi);
                Vector3 leftStandbyPosition = RotatePoint(phase5StandbySouthNorthwest, new Vector3(100, 0, 100), rotation);
                Vector3 rightStandbyPosition = RotatePoint(phase5StandbySouthNortheast, new Vector3(100, 0, 100), rotation);
                var currentProperty = accessory.Data.GetDefaultDrawProperties();

                if (Phase5_Branch_Of_The_Grey9_Brain_Dead_Strat == Phase5_Branches_Of_The_Grey9_Brain_Dead_Strat.Melees_First_Then_Healers_Left_Ranges_Right)
                {
                    bool isMelee = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 4 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 5);
                    bool isRange = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 6 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 7);
                    bool isHealer = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 2 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 3);

                    if (isMelee)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = (isLeftFirstAndFarFirst) ? (rightOfTheFirstTower) : (leftOfTheFirstTower);
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 7300;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = (isLeftFirstAndFarFirst) ? (rightOfTheFirstTower) : (leftOfTheFirstTower);
                        currentProperty.TargetPosition = oppositeStandbyPosition;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 7300;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = oppositeStandbyPosition;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 7300;
                        currentProperty.DestoryAt = 7100;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }
                    if (isRange)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = (isLeftFirstAndFarFirst) ? (rightStandbyPosition) : (leftStandbyPosition);
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = (isLeftFirstAndFarFirst) ? (rightStandbyPosition) : (leftStandbyPosition);
                        currentProperty.TargetPosition = rightOfTheRightTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = rightOfTheRightTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 6900;
                        currentProperty.DestoryAt = 7500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }
                    if (isHealer)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = (isLeftFirstAndFarFirst) ? (rightStandbyPosition) : (leftStandbyPosition);
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = (isLeftFirstAndFarFirst) ? (rightStandbyPosition) : (leftStandbyPosition);
                        currentProperty.TargetPosition = leftOfTheLeftTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = leftOfTheLeftTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 6900;
                        currentProperty.DestoryAt = 7500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheFirstTower;
                    currentProperty.Color = (isMelee) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 7300;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheLeftTower;
                    currentProperty.Color = (isHealer) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 14400;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheRightTower;
                    currentProperty.Color = (isRange) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 14400;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);
                }

                if (Phase5_Branch_Of_The_Grey9_Brain_Dead_Strat == Phase5_Branches_Of_The_Grey9_Brain_Dead_Strat.Healers_First_Then_Melees_Left_Ranges_Right)
                {
                    bool isMelee = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 4 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 5);
                    bool isRange = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 6 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 7);
                    bool isHealer = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 2 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 3);

                    if (isHealer)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = (isLeftFirstAndFarFirst) ? (rightOfTheFirstTower) : (leftOfTheFirstTower);
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 7300;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = (isLeftFirstAndFarFirst) ? (rightOfTheFirstTower) : (leftOfTheFirstTower);
                        currentProperty.TargetPosition = oppositeStandbyPosition;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 7300;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = oppositeStandbyPosition;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 7300;
                        currentProperty.DestoryAt = 7100;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }
                    if (isRange)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = (isLeftFirstAndFarFirst) ? (rightStandbyPosition) : (leftStandbyPosition);
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = (isLeftFirstAndFarFirst) ? (rightStandbyPosition) : (leftStandbyPosition);
                        currentProperty.TargetPosition = rightOfTheRightTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = rightOfTheRightTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 6900;
                        currentProperty.DestoryAt = 7500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }
                    if (isMelee)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = (isLeftFirstAndFarFirst) ? (rightStandbyPosition) : (leftStandbyPosition);
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = (isLeftFirstAndFarFirst) ? (rightStandbyPosition) : (leftStandbyPosition);
                        currentProperty.TargetPosition = leftOfTheLeftTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = leftOfTheLeftTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 6900;
                        currentProperty.DestoryAt = 7500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheFirstTower;
                    currentProperty.Color = (isHealer) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 7300;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheLeftTower;
                    currentProperty.Color = (isMelee) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 14400;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheRightTower;
                    currentProperty.Color = (isRange) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 14400;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);
                }

                if (Phase5_Branch_Of_The_Grey9_Brain_Dead_Strat == Phase5_Branches_Of_The_Grey9_Brain_Dead_Strat.Healer_First_Then_Melees_Farther_Ranges_Closer)
                {
                    Vector3 positionOfTheCloserTower = (isLeftFirstAndFarFirst) ? (positionOfTheRightTower) : (positionOfTheLeftTower);
                    Vector3 positionOfTheFartherTower = (isLeftFirstAndFarFirst) ? (positionOfTheLeftTower) : (positionOfTheRightTower);
                    Vector3 positionToTakeTheCloserTower = (isLeftFirstAndFarFirst) ? (rightOfTheRightTower) : (leftOfTheLeftTower);
                    Vector3 positionToTakeTheFartherTower = (isLeftFirstAndFarFirst) ? (leftOfTheLeftTower) : (rightOfTheRightTower);

                    bool isMelee = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 4 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 5);
                    bool isRange = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 6 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 7);
                    bool isHealer = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 2 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 3);

                    if (isHealer)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = (isLeftFirstAndFarFirst) ? (rightOfTheFirstTower) : (leftOfTheFirstTower);
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 7300;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = (isLeftFirstAndFarFirst) ? (rightOfTheFirstTower) : (leftOfTheFirstTower);
                        currentProperty.TargetPosition = oppositeStandbyPosition;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 7300;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = oppositeStandbyPosition;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 7300;
                        currentProperty.DestoryAt = 7100;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }
                    if (isRange)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = (isLeftFirstAndFarFirst) ? (rightStandbyPosition) : (leftStandbyPosition);
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = (isLeftFirstAndFarFirst) ? (rightStandbyPosition) : (leftStandbyPosition);
                        currentProperty.TargetPosition = positionToTakeTheCloserTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = positionToTakeTheCloserTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 6900;
                        currentProperty.DestoryAt = 7500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }
                    if (isMelee)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = (isLeftFirstAndFarFirst) ? (rightStandbyPosition) : (leftStandbyPosition);
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = (isLeftFirstAndFarFirst) ? (rightStandbyPosition) : (leftStandbyPosition);
                        currentProperty.TargetPosition = positionToTakeTheFartherTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = positionToTakeTheFartherTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 6900;
                        currentProperty.DestoryAt = 7500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheFirstTower;
                    currentProperty.Color = (isHealer) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 7300;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheFartherTower;
                    currentProperty.Color = (isMelee) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 14400;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheCloserTower;
                    currentProperty.Color = (isRange) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 14400;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);
                }
            }

            if (Phase5_Strat_Of_Wings_Dark_And_Light == Phase5_Strats_Of_Wings_Dark_And_Light.Reverse_Triangle_MT_Baits_In_Towers)
            {
                float rotation = 0;
                if (phase5FirstTowerIndex.Equals("00000030")) rotation = float.Pi / 3 * 2;
                if (phase5FirstTowerIndex.Equals("00000031")) rotation = -(float.Pi / 3 * 2);
                if (phase5FirstTowerIndex.Equals("00000032")) rotation = 0;

                Vector3 positionOfTheFirstTower = RotatePoint(new Vector3(100, 0, 107), new Vector3(100, 0, 100), rotation);
                Vector3 positionOfTheLeftTower = RotatePoint(positionOfTheFirstTower, new Vector3(100, 0, 100), float.Pi / 3 * 2);
                Vector3 positionOfTheRightTower = RotatePoint(positionOfTheFirstTower, new Vector3(100, 0, 100), -(float.Pi / 3 * 2));
                Vector3 leftOfTheLeftTower = RotatePoint(phase5LeftNorthwestPosition, new Vector3(100, 0, 100), rotation);
                Vector3 rightOfTheRightTower = RotatePoint(phase5RightNortheastPosition, new Vector3(100, 0, 100), rotation);
                Vector3 oppositeStandbyPosition = RotatePoint(positionOfTheFirstTower, new Vector3(100, 0, 100), float.Pi);
                Vector3 leftStandbyPosition = RotatePoint(phase5StandbySouthNorthwest, new Vector3(100, 0, 100), rotation - (float.Pi / 12));
                Vector3 rightStandbyPosition = RotatePoint(phase5StandbySouthNortheast, new Vector3(100, 0, 100), rotation + (float.Pi / 12));
                var currentProperty = accessory.Data.GetDefaultDrawProperties();

                if (Phase5_Branch_Of_The_Reverse_Triangle_Strat == Phase5_Branches_Of_The_Reverse_Triangle_Strat.Melees_First_Then_Healers_Left_Ranges_Right)
                {
                    bool isMelee = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 4 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 5);
                    bool isRange = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 6 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 7);
                    bool isHealer = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 2 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 3);

                    if (isMelee)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = positionOfTheFirstTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 7300;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = positionOfTheFirstTower;
                        currentProperty.TargetPosition = oppositeStandbyPosition;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 7300;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = oppositeStandbyPosition;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 7300;
                        currentProperty.DestoryAt = 7100;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }
                    if (isRange)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = rightStandbyPosition;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = rightStandbyPosition;
                        currentProperty.TargetPosition = rightOfTheRightTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = rightOfTheRightTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 6900;
                        currentProperty.DestoryAt = 7500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }
                    if (isHealer)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = leftStandbyPosition;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = leftStandbyPosition;
                        currentProperty.TargetPosition = leftOfTheLeftTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = leftOfTheLeftTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 6900;
                        currentProperty.DestoryAt = 7500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheFirstTower;
                    currentProperty.Color = (isMelee) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 7300;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheLeftTower;
                    currentProperty.Color = (isHealer) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 14400;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheRightTower;
                    currentProperty.Color = (isRange) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 14400;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);
                }

                if (Phase5_Branch_Of_The_Reverse_Triangle_Strat == Phase5_Branches_Of_The_Reverse_Triangle_Strat.Healers_First_Then_Melees_Left_Ranges_Right)
                {
                    bool isMelee = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 4 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 5);
                    bool isRange = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 6 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 7);
                    bool isHealer = (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 2 || accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 3);

                    if (isHealer)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = positionOfTheFirstTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 7300;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = positionOfTheFirstTower;
                        currentProperty.TargetPosition = oppositeStandbyPosition;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 7300;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Healers_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = oppositeStandbyPosition;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 7300;
                        currentProperty.DestoryAt = 7100;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }
                    if (isRange)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = rightStandbyPosition;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = rightStandbyPosition;
                        currentProperty.TargetPosition = rightOfTheRightTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Ranges_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = rightOfTheRightTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 6900;
                        currentProperty.DestoryAt = 7500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }
                    if (isMelee)
                    {
                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_1_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = leftStandbyPosition;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_Preview_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Position = leftStandbyPosition;
                        currentProperty.TargetPosition = leftOfTheLeftTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt = 6900;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        currentProperty.Name = "Phase5_Guidance_2_For_Melees_During_Towers";
                        currentProperty.Scale = new(2);
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = leftOfTheLeftTower;
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Color = accessory.Data.DefaultSafeColor;
                        currentProperty.Delay = 6900;
                        currentProperty.DestoryAt = 7500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
                    }

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheFirstTower;
                    currentProperty.Color = (isHealer) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 7300;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheLeftTower;
                    currentProperty.Color = (isMelee) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 14400;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();
                    currentProperty.Name = "Phase5_Range_Of_Towers";
                    currentProperty.Scale = new(3);
                    currentProperty.Position = positionOfTheRightTower;
                    currentProperty.Color = (isRange) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 14400;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);
                }
            }
        }

        [ScriptMethod(name: "Phase5 Boss Central Axis During Polarizing Strikes",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40316"])]
        public void Phase5_Boss_Central_Axis_During_Polarizing_Strikes(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5) return;
            if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase5_Boss_Front_Axis_During_Polarizing_Strikes";
            currentProperty.Owner = sourceId;
            currentProperty.Scale = new(0.5f, 10);
            currentProperty.Color = Phase5_Colour_Of_The_Boss_Central_Axis.V4.WithW(25f);
            currentProperty.DestoryAt = 24000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase5_Boss_Rear_Axis_During_Polarizing_Strikes";
            currentProperty.Owner = sourceId;
            currentProperty.Scale = new(0.5f, 10);
            currentProperty.Rotation = float.Pi;
            currentProperty.Color = Phase5_Colour_Of_The_Boss_Central_Axis.V4.WithW(25f);
            currentProperty.DestoryAt = 24000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);
        }

        [ScriptMethod(name: "Phase5 Guidance Of Polarizing Strikes",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40316"])]
        public void Phase5_Guidance_Of_Polarizing_Strikes(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5) return;
            if (!float.TryParse(@event["SourceRotation"], out float currentRotation)) return;
            currentRotation = -(currentRotation - float.Pi);

            if (Enable_Developer_Mode) accessory.Method.SendChat($"/e currentRotation={currentRotation}");

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            int myRoundToTakeHits = phase5_getRoundToTakeHits(myIndex);
            bool inTheLeftGroup = true;
            int timelineControl = 0;
            int timeToTakeHits = 0;
            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            if (myRoundToTakeHits < 1 || myRoundToTakeHits > 4) return;

            if (myIndex == 0 || myIndex == 2 || myIndex == 4 || myIndex == 6) inTheLeftGroup = true;
            if (myIndex == 1 || myIndex == 3 || myIndex == 5 || myIndex == 7) inTheLeftGroup = false;

            if (myRoundToTakeHits == 1)
            {
                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase5_Initial_Guidance_Of_Polarizing_Strikes";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = inTheLeftGroup ? RotatePoint(phase5LeftHitPosition, new Vector3(100, 0, 100), currentRotation) : RotatePoint(phase5RightHitPosition, new Vector3(100, 0, 100), currentRotation);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.DestoryAt = 4550;
                timelineControl += 4550;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
            }
            else
            {
                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase5_Initial_Guidance_Of_Polarizing_Strikes";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = inTheLeftGroup ? RotatePoint(phase5LeftCoverPosition, new Vector3(100, 0, 100), currentRotation) : RotatePoint(phase5RightCoverPosition, new Vector3(100, 0, 100), currentRotation);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.DestoryAt = 4550;
                timelineControl += 4550;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
            }

            for (int i = 1; i < myRoundToTakeHits; ++i)
            {
                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase5_Inward_Guidance_Of_Polarizing_Strikes_In_The_Current_Group";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = inTheLeftGroup ? RotatePoint(phase5LeftCoverPosition, new Vector3(100, 0, 100), currentRotation) : RotatePoint(phase5RightCoverPosition, new Vector3(100, 0, 100), currentRotation);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.Delay = timelineControl;
                currentProperty.DestoryAt = 2450;
                timelineControl += 2450;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase5_Outward_Guidance_Of_Polarizing_Strikes_In_The_Current_Group";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = inTheLeftGroup ? RotatePoint(phase5LeftStandbyPosition, new Vector3(100, 0, 100), currentRotation) : RotatePoint(phase5RightStandbyPosition, new Vector3(100, 0, 100), currentRotation);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.Delay = timelineControl;
                currentProperty.DestoryAt = 2250;
                timelineControl += 2250;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
            }

            timeToTakeHits = timelineControl - 250;

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase5_Inward_Guidance_Of_Polarizing_Strikes_While_Taking_Hits";
            currentProperty.Scale = new(2);
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = inTheLeftGroup ? RotatePoint(phase5LeftHitPosition, new Vector3(100, 0, 100), currentRotation) : RotatePoint(phase5RightHitPosition, new Vector3(100, 0, 100), currentRotation);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.Delay = timelineControl;
            currentProperty.DestoryAt = 2450;
            timelineControl += 2450;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = "Phase5_Outward_Guidance_Of_Polarizing_Strikes_While_Taking_Hits";
            currentProperty.Scale = new(2);
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = inTheLeftGroup ? RotatePoint(phase5RightStandbyPosition, new Vector3(100, 0, 100), currentRotation) : RotatePoint(phase5LeftStandbyPosition, new Vector3(100, 0, 100), currentRotation);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.Delay = timelineControl;
            currentProperty.DestoryAt = 2250;
            timelineControl += 2250;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            for (int i = myRoundToTakeHits + 1; i <= 4; ++i)
            {
                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase5_Inward_Guidance_Of_Polarizing_Strikes_In_The_Opposite_Group";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = inTheLeftGroup ? RotatePoint(phase5RightCoverPosition, new Vector3(100, 0, 100), currentRotation) : RotatePoint(phase5LeftCoverPosition, new Vector3(100, 0, 100), currentRotation);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.Delay = timelineControl;
                currentProperty.DestoryAt = 2450;
                timelineControl += 2450;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();
                currentProperty.Name = "Phase5_Outward_Guidance_Of_Polarizing_Strikes_In_The_Opposite_Group";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = inTheLeftGroup ? RotatePoint(phase5RightStandbyPosition, new Vector3(100, 0, 100), currentRotation) : RotatePoint(phase5LeftStandbyPosition, new Vector3(100, 0, 100), currentRotation);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.Delay = timelineControl;
                currentProperty.DestoryAt = 2250;
                timelineControl += 2250;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);
            }

            System.Threading.Thread.Sleep(timeToTakeHits);

            if (Enable_Text_Prompts)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.Method.TextInfo("Take hits and swap the group", 1500);
            }
            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {
                if (Language_Of_Prompts == Languages_Of_Prompts.English) accessory.TTS("Take hits and swap the group", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }

        private int phase5_getRoundToTakeHits(int currentIndex)
        {
            if (Phase5_Order_During_Polarizing_Strikes == Phase5_Orders_During_Polarizing_Strikes.Tanks_Melees_Ranges_Healers)
            {
                if (currentIndex == 0 || currentIndex == 1) return 1;
                if (currentIndex == 4 || currentIndex == 5) return 2;
                if (currentIndex == 6 || currentIndex == 7) return 3;
                if (currentIndex == 2 || currentIndex == 3) return 4;
            }
            if (Phase5_Order_During_Polarizing_Strikes == Phase5_Orders_During_Polarizing_Strikes.Tanks_Healers_Melees_Ranges)
            {
                if (currentIndex == 0 || currentIndex == 1) return 1;
                if (currentIndex == 2 || currentIndex == 3) return 2;
                if (currentIndex == 4 || currentIndex == 5) return 3;
                if (currentIndex == 6 || currentIndex == 7) return 4;
            }
            return -1;
        }

        public static Vector2? mathPoint(Blade b1, Blade b2)
        {
            float s1 = (float)Math.Sin(b1.Rotation);
            float c1 = (float)Math.Cos(b1.Rotation);
            float s2 = (float)Math.Sin(b2.Rotation);
            float c2 = (float)Math.Cos(b2.Rotation);

            float x1 = (float)b1.X;
            float y1 = (float)b1.Y;
            float x2 = (float)b2.X;
            float y2 = (float)b2.Y;

            float d = s1 * c2 - s2 * c1;
            if (Math.Abs(d) < 1e-10) return null;

            float X = (x1 * s1 * c2 - x2 * s2 * c1 - (y2 - y1) * c1 * c2) / d;
            float Y = (y2 * c2 * s1 - y1 * c1 * s2 + (x2 - x1) * s1 * s2) / d;
            return new Vector2(X, Y);
        }

        public static Vector2? middlePoint(Vector2? P1, Vector2? P2)
        {
            if (P1.HasValue && P2.HasValue)
            {
                float midX = (P1.Value.X + P2.Value.X) / 2;
                float midY = (P1.Value.Y + P2.Value.Y) / 2;
                return new Vector2(midX, midY);
            }
            return null;
        }

        public static onPoint FindClosestOnPoint(List<onPoint> points, Vector2? target)
        {
            onPoint closestPoint = null;
            float closestDistance = float.MaxValue;
            foreach (var point in points)
            {
                float distance = Vector2.Distance(point.OnCoord, target.Value);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = point;
                }
            }
            return closestPoint;
        }

        public static Vector2 FindFarthestPoint(onPoint point, Vector2? referencePoint)
        {
            Vector2[] coords = { point.Coord1, point.Coord2, point.Coord3, point.Coord4 };
            float maxDistance = float.MinValue;
            Vector2 farthestCoord = Vector2.Zero;
            foreach (var coord in coords)
            {
                float distance = Vector2.Distance(coord, referencePoint.Value);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    farthestCoord = coord;
                }
            }
            return farthestCoord;
        }

        public static Vector2 FindClosestPoint(onPoint point, Vector2? referencePoint)
        {
            Vector2[] coords = { point.Coord1, point.Coord2, point.Coord3, point.Coord4 };
            float minDistance = float.MaxValue;
            Vector2 closestCoord = Vector2.Zero;
            foreach (var coord in coords)
            {
                float distance = Vector2.Distance(coord, referencePoint.Value);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCoord = coord;
                }
            }
            return closestCoord;
        }

        public static Vector3 Vector3Fucker(Vector2? V)
        {
            Vector3 result = new Vector3();
            if (V.HasValue)
            {
                result.X = V.Value.X;
                result.Y = 0;
                result.Z = V.Value.Y;
            }
            return result;
        }

        [ScriptMethod(name: "P5 Fulgent Blade Recording", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40306"], userControl: false)]
        public void Phase_Recording_P5_Fulgent_Blade(Event @event, ScriptAccessory accessory)
        {
            Phase = "P5 Fulgent Blade";
            blades.Clear();
            p1p3Blades.Clear();
            BladeRoutes.Clear();
            bladeCount = 0;
            BladeRoutes = Enumerable.Repeat<Vector2?>(null, 7).ToList();
            ResetPoints();
        }

        [ScriptMethod(name: "Debug Switch", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40306"], userControl: false)]
        public void Phase_Recording_P5_Debug(Event @event, ScriptAccessory accessory)
        {
            if (Enable_Developer_Mode) accessory.Method.SendChat($"/e KnightRider wishes you good luck with Fulgent Blade~");
        }

        [ScriptMethod(name: "Fulgent Blade Data Capture", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:1"], userControl: false)]
        public void Fulgent_Blade_Data_Capture(Event @event, ScriptAccessory accessory)
        {
            if (Phase == "P5 Fulgent Blade")
            {
                lock (bladeLock)
                {
                    if (bladeCount < 7)
                    {
                        var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                        blades.Add(new Blade(
                            id: Convert.ToUInt32(@event["SourceId"], 16),
                            x: Convert.ToDouble(pos.X),
                            y: Convert.ToDouble(pos.Z),
                            rotation: Convert.ToDouble(@event["SourceRotation"])
                        ));
                        bladeCount++;
                    }
                    if (blades.Count == 6) ProcessBlades();
                }
            }
        }

        private void ProcessBlades()
        {
            var sortedBlades = blades.OrderBy(b => b.Id).ToList();
            if (sortedBlades != null)
            {
                p1p3Blades.Add(sortedBlades[0]);
                p1p3Blades.Add(sortedBlades[1]);
                p1p3Blades.Add(sortedBlades[4]);
                p1p3Blades.Add(sortedBlades[5]);
                Point1 = mathPoint(sortedBlades[0], sortedBlades[1]);
                Point2 = mathPoint(sortedBlades[2], sortedBlades[3]);
                Point3 = mathPoint(sortedBlades[4], sortedBlades[5]);
                MiddlePoint = middlePoint(Point1, Point3);
                OnPoint = FindClosestOnPoint(onPoints, MiddlePoint);
                Phase = "P5 Fulgent Blade Calculation Complete";
            }
        }

        #endregion

        #region Common_Mathematical_Wheels

        public static float convertDegree(float degree)
        {
            return degree * float.Pi / 180f;
        }

        private int ParsTargetIcon(string id)
        {
            firstTargetIcon ??= int.Parse(id, System.Globalization.NumberStyles.HexNumber);
            return int.Parse(id, System.Globalization.NumberStyles.HexNumber) - (int)firstTargetIcon;
        }

        private static bool ParseObjectId(string? idStr, out ulong id)
        {
            id = 0;
            if (string.IsNullOrEmpty(idStr)) return false;
            try
            {
                var idStr2 = idStr.Replace("0x", "");
                id = ulong.Parse(idStr2, System.Globalization.NumberStyles.HexNumber);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private int PositionTo8Dir(Vector3 point, Vector3 centre)
        {
            var r = Math.Round(4 - 4 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 8;
            return (int)r;
        }

        private int PositionTo6Dir(Vector3 point, Vector3 centre)
        {
            var r = Math.Round(3 - 3 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 6;
            return (int)r;
        }

        private Vector3 RotatePoint(Vector3 point, Vector3 centre, float radian)
        {
            Vector2 v2 = new(point.X - centre.X, point.Z - centre.Z);
            var rot = (MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian);
            var lenth = v2.Length();
            return new(centre.X + MathF.Sin(rot) * lenth, centre.Y, centre.Z - MathF.Cos(rot) * lenth);
        }

        #endregion

    }

    #region Other_Common_Wheels

    public static class Extensions
    {
        public static void TTS(this ScriptAccessory accessory, string text, bool isTTS, bool isDRTTS)
        {
            if (isTTS && isDRTTS) accessory.Method.TTS(text);
            else if (isDRTTS) accessory.Method.SendChat($"/pdr tts {text}");
            else if (isTTS) accessory.Method.TTS(text);
        }
    }

    public static class IndexHelper
    {
        public static int GetPlayerIdIndex(this ScriptAccessory accessory, uint pid) => accessory.Data.PartyList.IndexOf(pid);
        public static int GetMyIndex(this ScriptAccessory accessory) => accessory.Data.PartyList.IndexOf(accessory.Data.Me);
        public static string GetPlayerJobById(this ScriptAccessory accessory, uint pid) => accessory.GetPlayerJobByIndex(accessory.Data.PartyList.IndexOf(pid));
        public static string GetPlayerJobByIndex(this ScriptAccessory accessory, int idx, bool fourPeople = false) => idx switch
        {
            0 => "MT",
            1 => fourPeople ? "H1" : "ST",
            2 => fourPeople ? "D1" : "H1",
            3 => fourPeople ? "D2" : "H2",
            4 => "D1",
            5 => "D2",
            6 => "D3",
            7 => "D4",
            _ => "unknown"
        };
    }

    #endregion

}
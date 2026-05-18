// File: M7S_ArcadiaSavage_Heavyweight3_Mao.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;


using Newtonsoft.Json;

using KodakkuAssist.Data;
using KodakkuAssist.Extensions;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Script;

using Dalamud.Utility.Numerics;


using Util = TsingNamespace.Dawntrail.Savage.M7S.Utilities_Tsing;
using EX = TsingNamespace.Dawntrail.Savage.M7S.ScriptExtensions_Tsing;

namespace TsingNamespace.Dawntrail.Savage.M7S
{
    [ScriptType(name: "M7SÂ·Arcadia SavageÂ·Heavyweight 3", guid: "aad193df-365f-42ad-87ba-e7d92855d55f", territorys: [1261], version: "0.0.1.0", author: "Linoa235", note: noteStr)]
    public class M7S_Script
    {

        /*
            DONE boss charge skill danger zone warning
            CANCEL second round ice flower position pre-guidance
            DONE P3 spore safe zone guidance
            DONE cyst floor fire danger zone highlight
        */
        const string noteStr =
        """
            [Baby chair users please read sections 3 and 4 carefully]
            1. Guidance section has currently been adapted to the national standard MMW guide
               If you haven't enabled other plugins, the default settings should be sufficient to clear the national standard wild teams
            2. Users who have enabled "Settings - Drawing - [Only draw elements forced to Imgui mode]" function,
               please adjust the drawing type used for guidance in the user settings section at the bottom to Imgui
            3. Regarding the situation where [P2P3 ice flowers have no color guidance or guidance drawing], please ensure that the relevant triggers are enabled (pair ice flowers / two rounds of ice flowers)
               If related issues occur, you can check the
               User setting at the bottom: P3 Two Rounds of Ice Flower Debug Mode
               This will send simple logs information in the echo channel, which can be attached when reporting issues
               If you have BBY installed, you can try disabling BBY and then toggle the KodakkuAssist plugin off and on in the Dalamud plugin interface.
            4. For users who experience [drawing not showing] after long battles, you can also try the above operation.
            5. If you experience game disconnection or client crashes/freezes after enabling experimental features,
               please try disabling the head marking function, or disable all experimental features.
        """;


        [UserSetting("Default Role Order")]
        public PlayerRoleListEnum RoleMarks8 { get; set; } = PlayerRoleListEnum.MT_ST_H1_H2_D1_D2_D3_D4;
        public enum PlayerRoleListEnum
        {
            MT_ST_H1_H2_D1_D2_D3_D4,
        }

        [UserSetting("Strategy Type")]
        public WalkthroughEnum WalkthroughType { get; set; } = WalkthroughEnum.MMW_SPJP;
        public enum WalkthroughEnum { MMW_SPJP }

        [UserSetting("Experimental Feature: Enable Auto Provoke for Wildwinds Mobs (Mob group follows strategy)")]
        public bool AutoProvokeWildwindsMobsEnable { get; set; } = false;
        public enum ProvokeStrategy { ProvokeFurtherMobOnSpawn, ProvokeNonEngagedMob8SecondsAfterSpawn }
        [UserSetting("Experimental Feature: Auto Provoke Strategy for Wildwinds Mobs")]
        public ProvokeStrategy AutoProvokeStrategy { get; set; } = ProvokeStrategy.ProvokeNonEngagedMob8SecondsAfterSpawn;

        [UserSetting("Experimental Feature: Enable Auto Interrupt for Wildwinds Mobs (Interrupt target follows strategy)")]
        public bool AutoInterruptWildwindsMobsEnable { get; set; } = false;
        [UserSetting("Experimental Feature: Enable Auto Tank Support Mitigation")]
        public bool AutoTankSupportEnable { get; set; } = false;
        [UserSetting("Experimental Feature: Enable Head Marking Function for Mob-Related Mechanics")]
        public bool AutoMobsMarkEnable { get; set; } = true;


        [UserSetting("P2 Ice Flower Coloring => Type: Odd Rounds")]
        public ScriptColor StrangeSeedsCountOdd { get; set; } = new() { V4 = new(0, 1, 1, 2) };
        [UserSetting("P2 Ice Flower Coloring => Type: Even Rounds")]
        public ScriptColor StrangeSeedsCountEven { get; set; } = new() { V4 = new(1, 1, 0, 2) };
        [UserSetting("P2 Ice Flower Coloring Color Depth")]
        public float P2StrangeSeedsColorDensity { get; set; } = 2;
        [UserSetting("P2 Ice Flower Position Guidance Simple Style")]
        public bool P2StrangeSeedsSimpleStyle { get; set; } = true;
        [UserSetting("P2 Ice Flower Adjusted to Fixed Style")]
        public bool P2StrangeSeedsFixed { get; set; } = false;
        [UserSetting("P2 Enable Glower Power Guidance")]
        public bool P2GlowerPowerGuideDrawEnabled { get; set; } = true;

        [UserSetting("P3 Second Round Ice Flower MMW Use Chariot Positioning")]
        public bool P3MMWZhuiChe { get; set; } = true;


        [UserSetting("Guidance Color => Type: Immediate Go")]
        public ScriptColor GuideColor_GoNow { get; set; } = new() { V4 = new(0, 1, 1, 2) };
        [UserSetting("Guidance Color => Type: Go Later")]
        public ScriptColor GuideColor_GoLater { get; set; } = new() { V4 = new(1, 1, 0, 2) };
        [UserSetting("Guidance Color Depth")]
        public float GuideColorDensity { get; set; } = 2;
        [UserSetting("Width of Guidance Effect During Vfx Drawing")]
        public float Guide_Width { get; set; } = 1.4f;

        [UserSetting("Drawing Type for Guidance")]
        public DrawModeEnum GuideDrawMode { get; set; } = DrawModeEnum.Imgui;

        [UserSetting("Special Item: P3 Two Rounds of Ice Flower Debug Mode")]
        public bool P3MMWZhuiCheDebug { get; set; } = false;
        [UserSetting("Special Item: Quarry Swamp Safe Zone Force Imgui Drawing")]
        public bool QuarrySwampSafeZoneImgui { get; set; } = false;


        private static readonly object _lock = new object();
        private List<IGameObject> WildwindsMobs = new List<IGameObject>();
        private Dictionary<ulong, Vector3> WildwindsMobsBornPos = new Dictionary<ulong, Vector3>();
        private List<ulong> SinisterSeedTargets = new List<ulong>(); // P1/P3 Ice flower target list, used for marking ice flower targets
        private uint ExplosionCount = 0;
        private uint StrangeSeedsCount = 0;
        private readonly EX.MultiDisDrawProp MultiDisProp = new();
        private uint P2_BrutishSwingCastedCount = 0;
        private uint P3_BrutishSwingCastedCount = 0;
        private uint P3_StoneringerId = 0;




        public void Init(ScriptAccessory accessory)
        {
            // LatestStoneringerId = 0;
            WildwindsMobs.Clear();
            WildwindsMobsBornPos.Clear();
            SinisterSeedTargets.Clear();
            ExplosionCount = 0;
            StrangeSeedsCount = 0;

            P2_BrutishSwingCastedCount = 0;
            P3_BrutishSwingCastedCount = 0;
            P3_StoneringerId = 0;
            accessory.Method.RemoveDraw(".*");
            accessory.Log.Debug($"M7S Script Init");


            MultiDisProp.Color_GoNow = GuideColor_GoNow.V4.WithW(GuideColorDensity);
            MultiDisProp.Color_GoLater = GuideColor_GoLater.V4.WithW(GuideColorDensity);
            MultiDisProp.Width = Guide_Width;
            MultiDisProp.EndCircleRadius = Guide_Width * 0.5f + 0.05f;
            MultiDisProp.DrawMode = GuideDrawMode;

        }


        [ScriptMethod(name: "Brutal Impact Initialization",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.BrutalImpactActionId],
            userControl: false)]
        public void BrutalImpactInit(Event @event, ScriptAccessory accessory)
        {
            if (@event.SourcePosition.Y < -100) return; // Only execute in P1 phase
            Init(accessory);
        }

        [ScriptMethod(name: "Tankbuster Smash Here/There Dangerous Zone Draw",
                    eventType: EventTypeEnum.StartCasting,
                    eventCondition: [DataM7S.SmashHereThereActionId])]
        public void SmashHereThereDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            long destoryAt = 3000 + 900;
            uint actionId = @event.ActionId;
            ulong bossId = @event.SourceId;
            float radius_Smash = 6.0f;
            (long, long) delay_destoryAt = new(0, destoryAt);

            /* 
            Hit near/hit far
            Idea 1: Only mark tankbuster range
            Idea 2: Mark tankbuster range, and for non-tank roles, mark whether to approach or stay away from boss Circle + Arrow
            */
            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = nameof(SmashHereThereDangerousZoneDraw) + Guid.NewGuid().ToString();
            dp.Scale = new Vector2(radius_Smash, radius_Smash);
            dp.Delay = 0;
            dp.DestoryAt = destoryAt + 1550;
            dp.Owner = bossId;
            dp.CentreResolvePattern = actionId == (uint)DataM7S.AID.SmashHere ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            //Mark safe direction for non-tank roles, mark danger direction for tank roles
            bool IsMeTank = accessory.Data.MyObject.IsTank();
            DrawPropertiesEdit dpArrow = accessory.Data.GetDefaultDrawProperties();
            dpArrow.Name = nameof(SmashHereThereDangerousZoneDraw) + "Arrow" + Guid.NewGuid().ToString();
            dpArrow.Delay = dp.Delay;
            dpArrow.DestoryAt = dp.DestoryAt;
            dpArrow.Scale = new Vector2(1, 3);
            dpArrow.Owner = accessory.Data.Me;
            dpArrow.TargetObject = bossId;
            dpArrow.Color = IsMeTank ? accessory.Data.DefaultDangerColor.WithW(0.5f) : accessory.Data.DefaultSafeColor.WithW(0.5f);
            dpArrow.Rotation = (IsMeTank ? MathF.PI : 0) + (actionId == (uint)DataM7S.AID.SmashHere ? MathF.PI : 0);
            accessory.Method.SendDraw(MultiDisProp.DrawMode, DrawTypeEnum.Arrow, dpArrow);
        }

        [ScriptMethod(name: "Tankbuster Brutish Swing With Smash Dangerous Zone Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.BrutishSwingActionId_P1])]
        public void BrutishSwingWithSmashDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            uint actionId = @event.ActionId;
            ulong bossId = @event.SourceId;
            long destoryAt = 4000 - 700;
            (long, long) delay_destoryAt = new(0, destoryAt);
            float radius_Stick = 12.0f;
            float radius_Machete = 9.0f;
            switch (actionId)
            {
                case (uint)DataM7S.AID.BrutishSwingStick_P1:
                    accessory.FastDraw(DrawTypeEnum.Circle, bossId, new Vector2(radius_Stick, radius_Stick), delay_destoryAt, false);
                    break;
                case (uint)DataM7S.AID.BrutishSwingMachete_P1:
                    accessory.FastDraw(DrawTypeEnum.Donut, bossId, new Vector2(radius_Machete * 4, radius_Machete), delay_destoryAt, false);
                    break;
            }

        }

        [ScriptMethod(name: "P1/P3 Pollen Dangerous Zone Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.PollenActionId])]
        public void P1_PollenDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            Vector3 pos = @event.EffectPosition;
            long destoryAt = 3600;
            (long, long) delay_destoryAt = new(0, destoryAt);
            float radius_AOE = 8.0f;
            accessory.FastDraw(DrawTypeEnum.Circle, pos, new Vector2(radius_AOE, radius_AOE), delay_destoryAt, false);
            // DONE Can we know the safe zone type at the moment the cyst entity appears? <= It seems not, cysts only have animations, no entities
        }
        [ScriptMethod(name: "P1 Pollen Guide Draw 1",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.PollenActionId])]
        public void P1_PollenGuideDraw1(Event @event, ScriptAccessory accessory)
        {
            Vector3 pos = @event.EffectPosition;
            if (pos.Y < -100) return; // Only draw in P1 phase
            // Only detect the AOE circle closest to the lower left or lower right corner
            if (Util.DistanceByTwoPoints(pos, DataM7S.P1_FieldCenter) < 22.5f
                || pos.Z < DataM7S.P1_FieldCenter.Z) return;
            SinisterSeedTargets.Clear();
            bool isLeftDownSafe = pos.X > DataM7S.P1_FieldCenter.X;
            // Vector3 myStartPos = DataM7S.P1_FieldCenter;
            EX.PlayerRoleEnum myRole = accessory.GetMyRole();
            Vector3 myStartPos = (myRole, WalkthroughType, isLeftDownSafe) switch
            {
                // MT, ST, D1, D2 share offset
                (EX.PlayerRoleEnum.MT, WalkthroughEnum.MMW_SPJP, true) => DataM7S.P1_FieldCenter + new Vector3(7, 0, 7),
                (EX.PlayerRoleEnum.MT, WalkthroughEnum.MMW_SPJP, false) => DataM7S.P1_FieldCenter + new Vector3(-7, 0, 7),

                (EX.PlayerRoleEnum.ST, WalkthroughEnum.MMW_SPJP, true) => DataM7S.P1_FieldCenter + new Vector3(7, 0, 7),
                (EX.PlayerRoleEnum.ST, WalkthroughEnum.MMW_SPJP, false) => DataM7S.P1_FieldCenter + new Vector3(-7, 0, 7),

                (EX.PlayerRoleEnum.D1, WalkthroughEnum.MMW_SPJP, true) => DataM7S.P1_FieldCenter + new Vector3(7, 0, 7),
                (EX.PlayerRoleEnum.D1, WalkthroughEnum.MMW_SPJP, false) => DataM7S.P1_FieldCenter + new Vector3(-7, 0, 7),

                (EX.PlayerRoleEnum.D2, WalkthroughEnum.MMW_SPJP, true) => DataM7S.P1_FieldCenter + new Vector3(7, 0, 7),
                (EX.PlayerRoleEnum.D2, WalkthroughEnum.MMW_SPJP, false) => DataM7S.P1_FieldCenter + new Vector3(-7, 0, 7),

                // H1 & H2 use the same offset
                (EX.PlayerRoleEnum.H1, WalkthroughEnum.MMW_SPJP, true) => DataM7S.P1_FieldCenter + new Vector3(-17, 0, 17),
                (EX.PlayerRoleEnum.H1, WalkthroughEnum.MMW_SPJP, false) => DataM7S.P1_FieldCenter + new Vector3(17, 0, 17),

                (EX.PlayerRoleEnum.H2, WalkthroughEnum.MMW_SPJP, true) => DataM7S.P1_FieldCenter + new Vector3(-17, 0, 17),
                (EX.PlayerRoleEnum.H2, WalkthroughEnum.MMW_SPJP, false) => DataM7S.P1_FieldCenter + new Vector3(17, 0, 17),

                // D3 & D4
                (EX.PlayerRoleEnum.D3, WalkthroughEnum.MMW_SPJP, true) => DataM7S.P1_FieldCenter + new Vector3(17, 0, -17),
                (EX.PlayerRoleEnum.D3, WalkthroughEnum.MMW_SPJP, false) => DataM7S.P1_FieldCenter + new Vector3(-17, 0, -17),

                (EX.PlayerRoleEnum.D4, WalkthroughEnum.MMW_SPJP, true) => DataM7S.P1_FieldCenter + new Vector3(17, 0, -17),
                (EX.PlayerRoleEnum.D4, WalkthroughEnum.MMW_SPJP, false) => DataM7S.P1_FieldCenter + new Vector3(-17, 0, -17),

                // Default position (other cases)
                _ => DataM7S.P1_FieldCenter
            };

            // First guide to the safe zone, then to the pre-position point after the first explosion
            // accessory.FastDraw(DrawTypeEnum.Circle, myStartPos + new Vector3(0, 0, 5), new Vector2(1, 1), new(0, 5000), false);
            EX.DisplacementContainer myStartPosDC = new(myStartPos, 0, 5000 - 1060);
            switch (WalkthroughType)
            {
                case WalkthroughEnum.MMW_SPJP:
                    float modLength = 10;
                    // Move toward the arena center by modLength on the X axis
                    Vector3 myModPos = new(Math.Sign(DataM7S.P1_FieldCenter.X - myStartPos.X) * modLength + myStartPos.X, myStartPos.Y, myStartPos.Z);
                    EX.DisplacementContainer myModPosDC = new(myModPos, 0, 900);
                    accessory.MultiDisDraw(new List<EX.DisplacementContainer> { myStartPosDC, myModPosDC }, MultiDisProp);
                    break;
            }


        }


        [ScriptMethod(name: "P1 Roots Of Evil Dangerous Zone Pre Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.SinisterSeedsChaseActionId])]
        public void P1_RootsOfEvilDangerousZonePreDraw(Event @event, ScriptAccessory accessory)
        {
            // Multiple burrowing runners, using the same timestamp
            Vector3 pos = @event.EffectPosition;
            lock (_lock)
            {
                if ((DateTime.Now > ChaseDisplayTime))
                {
                    // ChaseDisplayTime hasn't been updated
                    if (pos.Y > -100)
                    {
                        ChaseDisplayTime = DateTime.Now + TimeSpan.FromSeconds(11.5);
                    }
                    else
                    { 
                        ChaseDisplayTime = DateTime.Now + TimeSpan.FromSeconds(8.1);
                    }
                    
                    // Draw warning 10 seconds after first floor fire
                }
            }
            long delay = (long)(ChaseDisplayTime - DateTime.Now).TotalMilliseconds;
            long destoryAt = 4000;
            (long, long) delay_destoryAt = new(delay, destoryAt);
            float radius_AOE = 12.0f;
            accessory.FastDraw(DrawTypeEnum.Circle, pos, new Vector2(radius_AOE, radius_AOE), delay_destoryAt, accessory.Data.DefaultDangerColor.WithW(0.4f));
        }
        private DateTime ChaseDisplayTime = DateTime.Now;


        [ScriptMethod(name: "P1/P3 Roots Of Evil Dangerous Zone Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.RootsOfEvilActionId])]
        public void P1_RootsOfEvilDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            Vector3 pos = @event.EffectPosition;
            long destoryAt = 3000;
            (long, long) delay_destoryAt = new(0, destoryAt);
            float radius_AOE = 12.0f;
            accessory.FastDraw(DrawTypeEnum.Circle, pos, new Vector2(radius_AOE, radius_AOE), delay_destoryAt, false);
        }

        [ScriptMethod(name: "P1 Sinister Seeds Blossom Dangerous Zone Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.SinisterSeedsBlossomActionId])]
        public void P1_SinisterSeedsBlossomDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            // Target places a star-shaped ice flower, P3 disabled to avoid affecting floor pattern visibility
            // The landing point judgment for the ice flower seems to be slightly later than the preceding skill's cast end time
            ulong tarId = @event.TargetId;
            if (tarId != accessory.Data.Me || @event.SourcePosition.Y < -100) return;

            // long destoryAt = (long)@event.DurationMilliseconds();
            long destoryAt = 7000; // P2 yellow circle ice flower -1500ms
            if(@event.SourcePosition.Z < 50) destoryAt -= 1500;
            float radius_AOE = 4f;
            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new Vector2(radius_AOE, 20.0f);
            dp.Owner = tarId;
            dp.Delay = 0;
            dp.DestoryAt = destoryAt;
            dp.FixRotation = true;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.09f);
            for (int i = 0; i < 4; i++)
            {
                dp.Name = $"{nameof(P1_SinisterSeedsBlossomDangerousZoneDraw)}{i}" + Guid.NewGuid().ToString();
                dp.Rotation = MathF.PI * 0.25f * i;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
            }

        }

        [ScriptMethod(name: "P1 Sinister Seeds Blossom Guide Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.SinisterSeedsBlossomActionId42350])]
        public void P1_SinisterSeedsBlossomGuideDraw(Event @event, ScriptAccessory accessory)
        {
            if (@event.SourcePosition.Y < -100) return; // Only draw in P1 phase
            ulong tarId = @event.TargetId;
            int count = -1;
            lock (_lock)
            {
                if (!SinisterSeedTargets.Contains(tarId))
                {
                    SinisterSeedTargets.Add(tarId); // Record ice flower targets
                    count = SinisterSeedTargets.Count;
                }
            }
            if (count != 4 || @event.SourcePosition.Y < -100 || @event.SourcePosition.Z < 50) return;


            bool isMeGetSinisterSeed = SinisterSeedTargets.Contains((ulong)accessory.Data.Me);
            // long destoryAt = (long)@event.DurationMilliseconds();
            long destoryAt = 7000;
            float radius_AOE = 4f;

            // First guide to the ice flower placement point, then to the stack point, then to the west side of the arena, only effective in P1
            Vector3 myStartPos = DataM7S.P1_FieldCenter;
            Vector3 myModPos = DataM7S.P1_FieldCenter; // For non-purple circle players' guidance
            Vector3 myStackPos = DataM7S.P1_FieldCenter;
            Vector3 myEndPos = DataM7S.P1_FieldCenter;


            EX.PlayerRoleEnum myRole = accessory.GetMyRole();
            Vector3 center = DataM7S.P1_FieldCenter;
            Vector3 offset = DataM7S.P1_NailOffset_In;
            Vector3 MMW_Offset_LeftStack = new Vector3(-6, 0, 0);
            Vector3 MMW_Offset_RightStack = new Vector3(6, 0, 0);
            Vector3 MMW_Offset_EndPos = new Vector3(-18, 0, 0);

            (myStartPos, myModPos, myStackPos, myEndPos) = (myRole, WalkthroughType, isMeGetSinisterSeed) switch
            {
                // â†“ Got ice flower
                (EX.PlayerRoleEnum.MT, WalkthroughEnum.MMW_SPJP, true) =>
                    (center + new Vector3(-offset.X, 0, -offset.Z), myModPos, center + MMW_Offset_LeftStack, center + MMW_Offset_EndPos),

                (EX.PlayerRoleEnum.ST, WalkthroughEnum.MMW_SPJP, true) =>
                    (center + new Vector3(offset.X, 0, -offset.Z), myModPos, center + MMW_Offset_RightStack, center + MMW_Offset_EndPos),

                (EX.PlayerRoleEnum.H1, WalkthroughEnum.MMW_SPJP, true) =>
                    (center + new Vector3(-offset.X, 0, offset.Z), myModPos, center + MMW_Offset_LeftStack, center + MMW_Offset_EndPos),

                (EX.PlayerRoleEnum.H2, WalkthroughEnum.MMW_SPJP, true) =>
                    (center + new Vector3(offset.X, 0, offset.Z), myModPos, center + MMW_Offset_RightStack, center + MMW_Offset_EndPos),

                (EX.PlayerRoleEnum.D1, WalkthroughEnum.MMW_SPJP, true) =>
                    (center + new Vector3(-offset.X, 0, offset.Z), myModPos, center + MMW_Offset_LeftStack, center + MMW_Offset_EndPos),

                (EX.PlayerRoleEnum.D2, WalkthroughEnum.MMW_SPJP, true) =>
                    (center + new Vector3(offset.X, 0, offset.Z), myModPos, center + MMW_Offset_RightStack, center + MMW_Offset_EndPos),

                (EX.PlayerRoleEnum.D3, WalkthroughEnum.MMW_SPJP, true) =>
                    (center + new Vector3(-offset.X, 0, -offset.Z), myModPos, center + MMW_Offset_LeftStack, center + MMW_Offset_EndPos),

                (EX.PlayerRoleEnum.D4, WalkthroughEnum.MMW_SPJP, true) =>
                    (center + new Vector3(offset.X, 0, -offset.Z), myModPos, center + MMW_Offset_RightStack, center + MMW_Offset_EndPos),

                // â†“ Didn't get ice flower
                (EX.PlayerRoleEnum.MT, WalkthroughEnum.MMW_SPJP, false) =>
                    (center + new Vector3(0, 0, -1), center + new Vector3(0, 0, -9), center + MMW_Offset_LeftStack, center + MMW_Offset_EndPos),

                (EX.PlayerRoleEnum.ST, WalkthroughEnum.MMW_SPJP, false) =>
                    (center + new Vector3(0, 0, -1), center + new Vector3(0, 0, -9), center + MMW_Offset_RightStack, center + MMW_Offset_EndPos),

                (EX.PlayerRoleEnum.H1, WalkthroughEnum.MMW_SPJP, false) =>
                    (center + new Vector3(0, 0, 18), center + new Vector3(0, 0, 9), center + MMW_Offset_LeftStack, center + MMW_Offset_EndPos),

                (EX.PlayerRoleEnum.H2, WalkthroughEnum.MMW_SPJP, false) =>
                    (center + new Vector3(0, 0, 18), center + new Vector3(0, 0, 9), center + MMW_Offset_RightStack, center + MMW_Offset_EndPos),

                (EX.PlayerRoleEnum.D1, WalkthroughEnum.MMW_SPJP, false) =>
                    (center + new Vector3(0, 0, -1), center + new Vector3(0, 0, -9), center + MMW_Offset_LeftStack, center + MMW_Offset_EndPos),

                (EX.PlayerRoleEnum.D2, WalkthroughEnum.MMW_SPJP, false) =>
                    (center + new Vector3(0, 0, -1), center + new Vector3(0, 0, -9), center + MMW_Offset_RightStack, center + MMW_Offset_EndPos),

                (EX.PlayerRoleEnum.D3, WalkthroughEnum.MMW_SPJP, false) =>
                    (center + new Vector3(0, 0, -18), center + new Vector3(0, 0, -9), center + MMW_Offset_LeftStack, center + MMW_Offset_EndPos),

                (EX.PlayerRoleEnum.D4, WalkthroughEnum.MMW_SPJP, false) =>
                    (center + new Vector3(0, 0, -18), center + new Vector3(0, 0, -9), center + MMW_Offset_RightStack, center + MMW_Offset_EndPos),

                _ => (myStartPos, myModPos, myStackPos, myEndPos)
            };
            switch (WalkthroughType)
            {
                case WalkthroughEnum.MMW_SPJP:
                    if (isMeGetSinisterSeed)
                    {
                        accessory.MultiDisDraw(new List<EX.DisplacementContainer>
                        {
                            new(myStartPos, 0, destoryAt + 1200),
                            new(myStackPos, 0, 4000),
                            new(myEndPos, 0, 5000)
                        }, MultiDisProp);
                    }
                    else
                    {
                        accessory.MultiDisDraw(new List<EX.DisplacementContainer>
                        {
                            new(myStartPos, 800, 3200),
                            new(myModPos, 0, 2000),
                            new(myStackPos, 0, 5900),
                            new(myEndPos, 0, 5000)
                        }, MultiDisProp);
                    }

                    break;
            }

        }

        [ScriptMethod(name: "P1 Tendrils Of Terror Dangerous Zone Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.TendrilsOfTerrorActionId])]
        public void P1_TendrilsOfTerrorDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            // Target places a star-shaped ice flower
            Vector3 pos = @event.EffectPosition;
            // long destoryAt = (long)@event.DurationMilliseconds();
            long destoryAt = 3000;
            float radius_AOE = 4f;
            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new Vector2(radius_AOE, 100.0f);
            dp.Position = pos;
            dp.Delay = 0;
            dp.DestoryAt = destoryAt;
            dp.FixRotation = true;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.8f);
            for (int i = 0; i < 4; i++)
            {
                dp.Name = $"{nameof(P1_TendrilsOfTerrorDangerousZoneDraw)}{i}" + Guid.NewGuid().ToString();
                dp.Rotation = MathF.PI * 0.25f * i;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
            }
        }

        /*
            Mark donut-casting mobs with Attack7 and Attack8?
            Have tanks target the nearest donut-casting mob
            Have ranged target the nearest donut-casting mob?
        */
        [ScriptMethod(name: "P1 Blooming Abomination Add Combatant",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: [DataM7S.BloomingAbominationDataId],
            userControl: false)]
        public void P1_BloomingAbominationAdd(Event @event, ScriptAccessory accessory)
        {
            // Record ID and spawn location
            lock (_lock)
            {
                ulong mobId = @event.SourceId;
                WildwindsMobsBornPos[mobId] = @event.SourcePosition;
            }
        }

        [ScriptMethod(name: "P1/P3 Blooming Abomination Add Combatant Auto Provoke",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: [DataM7S.BloomingAbominationDataId],
            suppress: 10000,
            userControl: true)]
        public async void P1P3_BloomingAbominationAddAutoProvoke(Event @event, ScriptAccessory accessory)
        {
            if (!AutoProvokeWildwindsMobsEnable) return;
            try
            {
                if (!accessory.Data.MyObject.IsTank()) return;
            }
            catch (System.Exception ex)
            {
                return;
            }
            await Task.Delay(2000); // Wait for mobs to enter combat
            EX.PlayerRoleEnum myRole = accessory.GetMyRole();
            List<IGameObject> mobs = accessory.Data.Objects.GetByDataId((uint)DataM7S.OID.BloomingAbomination).ToList();
            Vector3 fieldCenter = @event.SourcePosition.Y > -100 ? DataM7S.P1_FieldCenter : DataM7S.P3_FieldCenter;
            Vector3 myPrefPos = fieldCenter;

            myPrefPos = (myRole, WalkthroughType) switch
            {
                (EX.PlayerRoleEnum.MT, WalkthroughEnum.MMW_SPJP) => fieldCenter + new Vector3(-30, 0, -30),
                (EX.PlayerRoleEnum.ST, WalkthroughEnum.MMW_SPJP) => fieldCenter + new Vector3(30, 0, 30),
                _ => myPrefPos
            };
            mobs = mobs
                .OrderBy(mob => Util.DistanceByTwoPoints(mob.Position, myPrefPos)) // Sort by distance
                .ToList();
            List<IGameObject> myMobs = new List<IGameObject>();
            if (mobs.Count <= 2)
            {
                myMobs = mobs;
            }
            else
            {
                // myMobs = myRole == EX.PlayerRoleEnum.MT ? mobs.Take(2).ToList() : mobs.TakeLast(2).ToList();
                myMobs = mobs.Take(2).ToList();
            }
            // At this point, we should have the two mobs to pull
            List<KodakkuAssist.Module.GameOperate.MarkType> markTypes = new List<KodakkuAssist.Module.GameOperate.MarkType>
            {
                KodakkuAssist.Module.GameOperate.MarkType.Stop1,
                KodakkuAssist.Module.GameOperate.MarkType.Stop2,
                KodakkuAssist.Module.GameOperate.MarkType.Bind1,
                KodakkuAssist.Module.GameOperate.MarkType.Bind2,
                KodakkuAssist.Module.GameOperate.MarkType.Bind3,
            };

            bool isMarkLocal = true;
            for (int i = 0; i < myMobs.Count; i++)
            {
                IGameObject _obj = myMobs[i];
                KodakkuAssist.Module.GameOperate.MarkType markType = markTypes[i];
                if (AutoMobsMarkEnable) accessory.Method.Mark(_obj.EntityId, markType, isMarkLocal);
                await Task.Delay(10); // Wait 10 milliseconds
                // Mark it
            }
            int delayTime = AutoProvokeStrategy switch
            {
                ProvokeStrategy.ProvokeFurtherMobOnSpawn => 100,
                ProvokeStrategy.ProvokeNonEngagedMob8SecondsAfterSpawn => 6000,
                _ => 100,
            };
            await Task.Delay(delayTime); // Wait a while
            IGameObject? provokeTarget = null;
            switch (AutoProvokeStrategy)
            {
                case ProvokeStrategy.ProvokeFurtherMobOnSpawn:
                    try
                    {
                        myMobs = myMobs.OrderBy(mob => Util.DistanceByTwoPoints(mob.Position, accessory.Data.MyObject.Position)).ToList();
                    }
                    finally
                    {
                        provokeTarget = myMobs.LastOrDefault();
                    }
                    break;
                case ProvokeStrategy.ProvokeNonEngagedMob8SecondsAfterSpawn:
                    try
                    {
                        List<IGameObject> nonTankMobs = mobs.Where(mob => mob.TargetObject is null || mob.TargetObject.EntityId != accessory.Data.Me).ToList();
                        // And my current target is not this mob
                        nonTankMobs = nonTankMobs
                            .Where(mob => accessory.Data.MyObject.TargetObject is null || accessory.Data.MyObject.TargetObject.EntityId != mob.EntityId)
                            .ToList();
                        provokeTarget = myMobs.LastOrDefault();
                    }
                    catch
                    {
                        // nothing
                    }
                    // Only provoke non-engaged mobs
                    break;
            }
            if (provokeTarget is not null)
            {
                Task.Run(async () =>
                    {
                        uint provokeActionId = 7533; // Provoke skill ID
                        if (AutoMobsMarkEnable) accessory.Method.Mark(provokeTarget.EntityId, KodakkuAssist.Module.GameOperate.MarkType.Cross, isMarkLocal);
                        accessory.Log.Debug($"Attempting to provoke mob {provokeTarget}");
                        accessory.Method.SendChat($"/e Attempting to provoke \"+\" marked mob {provokeTarget} <se.5>");
                        for (int j = 0; j < 6; j++)
                        {
                            try
                            {
                                if (provokeTarget is IBattleChara _bc && !_bc.IsDead && !accessory.Data.MyObject.IsDead)
                                {
                                    accessory.Method.UseAction(provokeTarget.EntityId, provokeActionId);
                                    accessory.Log.Debug($"Auto provoke => {accessory.GetMyRole()} to {provokeTarget}");
                                }
                                await Task.Delay(500); // Wait 500 milliseconds
                            }
                            catch (System.Exception ex)
                            {
                                accessory.Log.Error($"Auto provoke exception => {ex}");
                            }
                        }
                    });
            }
            await Task.Delay(6000);

            // Clear marks
            accessory.Method.SendChat($"/mk clear <stop1>");
            accessory.Method.SendChat($"/mk clear <stop2>");
            accessory.Method.SendChat($"/mk clear <cross>");
        }


        [ScriptMethod(name: "P1 Mob Winds Casting Mark and Interrupt",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.MobsWindsActionId])]
        public async void MobsWindsCastingMark(Event @event, ScriptAccessory accessory)
        {
            if (!AutoInterruptWildwindsMobsEnable) return;
            try
            {
                if (!accessory.Data.MyObject.IsTank()) return;
            }
            catch (System.Exception ex)
            {
                return;
            }
            uint actionId = @event.ActionId;
            ulong mobId = @event.SourceId;
            if (actionId != (uint)DataM7S.AID.WindingWildwinds) return;
            bool isGetTwoMobs = false;
            IGameObject mobObj = accessory.Data.Objects.SearchById(mobId);
            if (mobObj is not null)
            {
                lock (_lock)
                {
                    WildwindsMobs.Add(mobObj);
                    isGetTwoMobs = WildwindsMobs.Count == 2;
                }
            }
            if (!isGetTwoMobs) return;


            List<uint> castingMobs = new List<uint>();
            List<uint> rawHostileList = new List<uint>();

            try
            {
                unsafe
                {
                    // Get mob IDs from the hate list
                    for (int i = 0; i < 5; i++)
                    {
                        FFXIVClientStructs.FFXIV.Client.UI.Arrays.EnemyListNumberArray.EnemyListEnemyNumberArray* _hostileObj =
                            (FFXIVClientStructs.FFXIV.Client.UI.Arrays.EnemyListNumberArray.EnemyListEnemyNumberArray*)
                            ((byte*)FFXIVClientStructs.FFXIV.Client.UI.Arrays.EnemyListNumberArray.Instance() + 5 * 4 + i * (6 * 4));
                        accessory.Log.Debug($"Test: Getting enemy {i} from list => {_hostileObj->EntityId} = {accessory.Data.Objects.SearchByEntityId((uint)(_hostileObj->EntityId))}");
                        if (_hostileObj->EntityId > 0x40_000_000)
                        {
                            rawHostileList.Add((uint)_hostileObj->EntityId);
                        }


                    }
                    castingMobs = rawHostileList
                        .Where(id => accessory.Data.Objects.SearchByEntityId(id) is IBattleChara bc && bc.IsCasting && bc.CastActionId == (uint)DataM7S.AID.WindingWildwinds)
                        .ToList();
                    // Filter out IDs of donut-casting mobs
                }
            }
            catch (System.Exception ex)
            {
                accessory.Log.Error($"Exception getting donut-casting mobs => {ex}");
            }


            // Sort mobs with spawn location closer to top-left first
            List<IGameObject> mobs = WildwindsMobs.OrderBy(obj =>
            {
                ulong id = obj.GameObjectId;
                Vector3 bornPos = obj.Position;
                // Look up spawn location in the dictionary
                if (WildwindsMobsBornPos.TryGetValue(id, out Vector3 _bornPos))
                {
                    bornPos = _bornPos;
                }
                Vector3 leftTop = DataM7S.P1_FieldCenter + new Vector3(-20f, 0, -20f);
                return Util.DistanceByTwoPoints(bornPos, leftTop);
            }).ToList();


            /*
              Will MT pull both donut-casting mobs?
            */

            KodakkuAssist.Module.GameOperate.MarkType markType = KodakkuAssist.Module.GameOperate.MarkType.Attack1;
            switch (WalkthroughType)
            {
                case WalkthroughEnum.MMW_SPJP:
                    // Mark

                    for (int i = 0; mobs.Count > 0 && i < mobs.Count; i++)
                    {
                        IGameObject _obj = mobs[i];

                        // Find the position of this Obj's ID in castingMobs
                        int indexInHostileList = castingMobs.IndexOf(_obj.EntityId);
                        switch (indexInHostileList)
                        {
                            case -1:
                                // Not in list
                                markType = markType == KodakkuAssist.Module.GameOperate.MarkType.Stop1 ? KodakkuAssist.Module.GameOperate.MarkType.Stop1 : KodakkuAssist.Module.GameOperate.MarkType.Stop2;
                                break;
                            case 0:
                                markType = KodakkuAssist.Module.GameOperate.MarkType.Attack7;
                                break;
                            case 1:
                                markType = KodakkuAssist.Module.GameOperate.MarkType.Attack8;
                                break;
                        }
                        bool isLocal = true;
                        if (AutoMobsMarkEnable) accessory.Method.Mark(_obj.EntityId, markType, isLocal);
                        // Mark + interrupt
                        bool autoInterrupt = AutoInterruptWildwindsMobsEnable;
                        uint InterjectActionId = 7538;
                        uint HeadGrazeActionId = 7551;
                        if (autoInterrupt
                            && accessory.Data.MyObject is not null
                            && accessory.Data.MyObject.IsTank())
                        {
                            // I am a tank and auto interrupt is enabled
                            bool isMyMob = false;
                            switch (accessory.GetMyRole())
                            {
                                case EX.PlayerRoleEnum.MT:
                                    isMyMob = markType == KodakkuAssist.Module.GameOperate.MarkType.Attack7; // MT is first mob
                                    break;
                                case EX.PlayerRoleEnum.ST:
                                    isMyMob = markType == KodakkuAssist.Module.GameOperate.MarkType.Attack8; // ST is second mob
                                    break;
                            }
                            if (isMyMob)
                            {
                                // Auto interrupt for MT or ST
                                Task.Run(async () =>
                                {
                                    accessory.Method.SendChat($"/e Attempting to interrupt {markType} {_obj} <se.5>, it may be the {rawHostileList.IndexOf(_obj.EntityId) + 1}th slot in the hate list");
                                    for (int j = 0; j < 13; j++)
                                    {
                                        try
                                        {
                                            if (_obj is IBattleChara _bc && !_bc.IsDead && _bc.IsCasting && _bc.IsCastInterruptible && !accessory.Data.MyObject.IsDead)
                                            {
                                                accessory.Method.UseAction(_obj.EntityId, InterjectActionId);
                                                accessory.Log.Debug($"Auto interrupt => {accessory.GetMyRole()} to {_obj}");
                                            }
                                            await Task.Delay(500); // Wait 500 milliseconds
                                        }
                                        catch (System.Exception ex)
                                        {
                                            accessory.Log.Error($"Auto interrupt exception => {ex}");
                                        }
                                    }
                                });
                            }
                        }
                        await Task.Delay(10);
                    }
                    break;
            }

            // Automatically select the nearest donut-casting mob for ranged?




        }


        [ScriptMethod(name: "P1/P3 Quarry Swamp Safe Zone Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.QuarrySwampActionId])]
        public void P1P3_QuarrySwampSafeZoneDraw(Event @event, ScriptAccessory accessory)
        {
            /*
                The safe zone for the petrifying eye consists of four donut-shaped sectors
                InnerScale = distance from boss to mob corpse;
                Scale = 50;
                Radian = Atan2(mob hitbox radius, distance from boss to mob corpse) * 2
            */
            Vector3 bossPos = @event.SourcePosition;
            ulong bossId = @event.SourceId;
            // long destoryAt = (long)@event.DurationMilliseconds();
            long destoryAt = 4000;

            // Collect mob entity information
            IEnumerable<IGameObject> mobs = accessory.Data.Objects.GetByDataId((uint)DataM7S.OID.BloomingAbomination);
            foreach (IGameObject mob in mobs)
            {
                float _dis = Util.DistanceByTwoPoints(bossPos, mob.Position);
                DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"{nameof(P1P3_QuarrySwampSafeZoneDraw)}{mob.EntityId}" + Guid.NewGuid().ToString();
                dp.Delay = 0;
                dp.DestoryAt = destoryAt;
                dp.Scale = new Vector2(50.0f, 50.0f);
                dp.InnerScale = new Vector2(_dis, _dis);
                dp.Owner = bossId;
                dp.TargetObject = mob.EntityId;
                dp.Radian = 2 * MathF.Atan2(mob.HitboxRadius, _dis);
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(QuarrySwampSafeZoneImgui ? DrawModeEnum.Imgui : DrawModeEnum.Default,
                    DrawTypeEnum.Donut, dp);
            }
        }


        [ScriptMethod(name: "P1 Explosion (Triple Distance Attenuation) Dangerous Zone Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.ExplosionActionId])]
        public void P1_ExplosionDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            /*
                x Triple attenuation, mark the center position of the first with a larger danger zone, and the center position of the third with a smaller safe zone
                Triple attenuation, based on order, first is darker, third is lighter
            */
            uint count = 0;
            lock (_lock)
            {
                count = ++ExplosionCount;
            }
            Vector3 pos = @event.EffectPosition;
            // (long, long) delay_destoryAtFirst = new(0, (long)@event.DurationMilliseconds());
            (long, long) delay_destoryAtFirst = new(0, 9000);
            float density = count switch
            {
                1 => 2.0f, // First
                2 => 0.7f, // Second
                _ => 0.3f, // Third
            };
            float radius_AOE = 25.0f;
            accessory.FastDraw(DrawTypeEnum.Circle, pos, new Vector2(radius_AOE, radius_AOE), delay_destoryAtFirst,
                accessory.Data.DefaultDangerColor.WithW(density));
        }

        [ScriptMethod(name: "P1/P3 It Came From The Dirt (Healer Stack) Stack Range", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:00A1"])]
        public void P1_HealerStackStackRange(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"HealerStack_StackRange";
            dp.Scale = new(6);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = @event.TargetId;
            // Add delay for P3?
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P1/P3 It Came From The Dirt (Healer Stack) Octa Pre-Guidance", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:00A1"])]
        public void P1_HealerStackOctaPreGuidance(Event @event, ScriptAccessory accessory)
        {
            var tpos = @event.TargetPosition;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var bossPos = accessory.Data.Objects.GetByDataId((uint)DataM7S.OID.BruteAbombinator).FirstOrDefault()?.Position ?? Vector3.Zero;
            var drot = myindex switch
            {
                1 => 4,
                2 => 2,
                3 => 6,
                4 => 3,
                5 => 5,
                6 => 1,
                7 => 7,
                _ => 0
            };
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"HealerStack_OctaPreGuidance";
            dp.Owner = @event.TargetId;
            dp.TargetPosition = bossPos;
            dp.Rotation = float.Pi + float.Pi / 4 * drot;
            dp.Scale = new(2, 8);
            // Add delay for P3?
            dp.DestoryAt = 5200;
            dp.Color = GuideColor_GoLater.V4;
            accessory.Method.SendDraw(GuideDrawMode, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P1/P3 It Came From The Dirt (Healer Stack) Octa Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42362"])]
        public void P1_HealerStackOctaGuidance(Event @event, ScriptAccessory accessory)
        {
            var spos = @event.SourcePosition;
            var srot = @event.SourceRotation;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var drot = myindex switch
            {
                1 => 4,
                2 => 2,
                3 => 6,
                4 => 3,
                5 => 5,
                6 => 1,
                7 => 7,
                _ => 0
            };
            Vector3 tpos = new(spos.X + MathF.Sin(srot + float.Pi / 4 * drot) * 8, spos.Y, spos.Z + MathF.Cos(srot + float.Pi / 4 * drot) * 8);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"HealerStack_OctaGuidance";
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = tpos;
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = GuideColor_GoNow.V4;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(GuideDrawMode, DrawTypeEnum.Displacement, dp);
        }


        [ScriptMethod(name: "P1/P3 It Came From The Dirt (Stack+Octa) Dangerous Zone Draw",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: [DataM7S.PulpSmashActionId])]
        public void P1P3_ItCameFromTheDirtDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            /*
                Draw the small circle underfoot for the stack
                Draw the eight-direction cones for the spread after the stack
                Record the facing before the stack jump
                Record the facing after the stack jump
            */

            // Draw the small circle underfoot
            ulong bossId = @event.SourceId;
            float radius_centreDanger = 6.0f;
            (long Delay, long DestoryAt) delay_destoryAtCentreDanger = new(1800, 2300);
            accessory.FastDraw(DrawTypeEnum.Circle, bossId, new Vector2(radius_centreDanger, radius_centreDanger),
                 delay_destoryAtCentreDanger, accessory.Data.DefaultDangerColor.WithW(1.5f));

            // Draw the eight-direction cones
            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            // dp.Name
            dp.Delay = delay_destoryAtCentreDanger.Delay;
            dp.DestoryAt = delay_destoryAtCentreDanger.DestoryAt;
            dp.Scale = new Vector2(30.0f, 30.0f);
            dp.InnerScale = new Vector2(radius_centreDanger, radius_centreDanger);
            dp.Owner = bossId;
            // dp.TargetObject = mob.EntityId;
            dp.Radian = (3.0f / 18.0f) * MathF.PI;
            dp.Color = accessory.Data.DefaultDangerColor;


            foreach (uint playerId in accessory.Data.PartyList)
            {
                dp.Name = nameof(P1P3_ItCameFromTheDirtDangerousZoneDraw) + playerId.ToString() + Guid.NewGuid().ToString();
                dp.TargetObject = playerId;
                // Filter out dead players
                IGameObject obj = accessory.Data.Objects.SearchById((ulong)playerId);
                if (obj is null || obj.IsDead)
                {
                    continue;
                }
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
            }

        }

        [ScriptMethod(name: "P1/P3 It Came From The Dirt Guide Draw",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: [DataM7S.PulpSmashActionId],
            userControl: false)]
        public void P1P3_ItCameFromTheDirtGuideDraw(Event @event, ScriptAccessory accessory)
        {
            return;
            ulong bossId = @event.SourceId;
            float bossRadius = 8;
            Vector3 offSetBack = new Vector3(0, 0, bossRadius + 2);
            Vector3 offSetFront = new Vector3(0, 0, -(bossRadius + 2));
            Vector3 offSetLeft = new Vector3(-(bossRadius + 2), 0, 0);
            Vector3 offSetRight = new Vector3((bossRadius + 2), 0, 0);
            Vector3 offSetFrontLeft = new Vector3(-0.7f * (bossRadius + 2), 0, -0.7f * (bossRadius + 2));
            Vector3 offSetFrontRight = new Vector3(0.7f * (bossRadius + 2), 0, -0.7f * (bossRadius + 2));
            Vector3 offSetBackLeft = new Vector3(-0.7f * (bossRadius + 2), 0, 0.7f * (bossRadius + 2));
            Vector3 offSetBackRight = new Vector3(0.7f * (bossRadius + 2), 0, 0.7f * (bossRadius + 2));

            // Vector3 myOffset = offSetBack;
            // float myOffsetRot = MathF.Atan2(myOffset.Z, myOffset.X);
            EX.PlayerRoleEnum myRole = accessory.GetMyRole();
            Vector3 myOffset = (myRole, WalkthroughType) switch
            {
                (EX.PlayerRoleEnum.MT, WalkthroughEnum.MMW_SPJP) => offSetFront,
                (EX.PlayerRoleEnum.ST, WalkthroughEnum.MMW_SPJP) => offSetBack,
                (EX.PlayerRoleEnum.H1, WalkthroughEnum.MMW_SPJP) => offSetLeft,
                (EX.PlayerRoleEnum.H2, WalkthroughEnum.MMW_SPJP) => offSetRight,
                (EX.PlayerRoleEnum.D1, WalkthroughEnum.MMW_SPJP) => offSetBackLeft,
                (EX.PlayerRoleEnum.D2, WalkthroughEnum.MMW_SPJP) => offSetBackRight,
                (EX.PlayerRoleEnum.D3, WalkthroughEnum.MMW_SPJP) => offSetFrontLeft,
                (EX.PlayerRoleEnum.D4, WalkthroughEnum.MMW_SPJP) => offSetFrontRight,
                _ => offSetBack // Default value
            };

            DrawPropertiesEdit dpArrow = accessory.Data.GetDefaultDrawProperties();
            dpArrow.Name = "ItCameFromTheDirtGuideDraw 0 " + Guid.NewGuid().ToString();
            // Please fine-tune the following two values for optimal guidance effect
            dpArrow.Delay = 1650 - 1650;
            dpArrow.DestoryAt = 2200 + 1650;
            dpArrow.Scale = new(4);
            dpArrow.ScaleMode |= ScaleMode.YByDistance;
            dpArrow.Owner = bossId;
            dpArrow.TargetObject = (ulong)accessory.Data.Me;
            dpArrow.Offset = myOffset;
            dpArrow.Color = MultiDisProp.Color_GoNow.WithW(MultiDisProp.Color_GoNow.W + 1);
            accessory.Method.SendDraw(GuideDrawMode, DrawTypeEnum.Line, dpArrow);
            dpArrow.Color = MultiDisProp.Color_GoNow.WithW(MultiDisProp.Color_GoNow.W + 4);
            dpArrow.ScaleMode = ScaleMode.None;
            dpArrow.Scale = new(0.05f, 3);
            dpArrow.Rotation = 0.4f;
            dpArrow.Name = "ItCameFromTheDirtGuideDraw 1 " + Guid.NewGuid().ToString();
            accessory.Method.SendDraw(GuideDrawMode, DrawTypeEnum.Rect, dpArrow);
            dpArrow.Rotation = -0.4f;
            dpArrow.Name = "ItCameFromTheDirtGuideDraw 2 " + Guid.NewGuid().ToString();
            accessory.Method.SendDraw(GuideDrawMode, DrawTypeEnum.Rect, dpArrow);

        }

        [ScriptMethod(name: "P1 Neo Bombarian Special Safe Zone Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.NeoBombarianSpecialActionId])]
        public void P1_NeoBombarianSpecialSafeZoneDraw(Event @event, ScriptAccessory accessory)
        {
            /*
                P1 transition knockback draw safe zone range
            */
            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = nameof(P1_NeoBombarianSpecialSafeZoneDraw) + Guid.NewGuid().ToString();
            dp.Delay = 500; // Add a little delay because the boss sometimes turns
            // dp.DestoryAt = (long)@event.DurationMilliseconds() - dp.Delay;
            dp.DestoryAt = 8000 - dp.Delay;
            dp.Scale = new Vector2(8, 30.0f);
            dp.Owner = @event.SourceId;
            dp.Offset = new Vector3(0, 0, -11.5f);
            dp.Color = accessory.Data.DefaultSafeColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }




        [ScriptMethod(name: "P2 Abominable Blink Dangerous Zone Draw",
            eventType: EventTypeEnum.TargetIcon,
            eventCondition: [DataM7S.AbominableBlinkIconId])]
        public void P2_AbominableBlinkDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            accessory.FastDraw(DrawTypeEnum.Circle, @event.TargetId, new Vector2(25, 25), new(0, 6480), false);
            // Go to the boss's own front-right
            Vector3 bossPos = DataM7S.P2_FieldCenter;
            IGameObject bossObj = accessory.Data.Objects.GetByDataId((uint)DataM7S.OID.BruteAbombinator).FirstOrDefault();
            if (bossObj is not null)
            {
                bossPos = bossObj.Position;
            }
            bool isBossNearWall = bossPos.Z < DataM7S.P2_FieldCenter.Z;
            Vector3 myPos = isBossNearWall ? new Vector3(12, 0, -24.5f) + DataM7S.P2_FieldCenter : new Vector3(-12, 0, 24.5f) + DataM7S.P2_FieldCenter;
            if ((ulong)accessory.Data.Me == @event.TargetId)
            {
                // I am the target
                EX.DisplacementContainer myEndPos = new(myPos, 0, 5000);
                accessory.MultiDisDraw(new List<EX.DisplacementContainer> { myEndPos }, MultiDisProp);
            }
        }

        [ScriptMethod(name: "P2 Abominable Blink Tank Support",
            eventType: EventTypeEnum.TargetIcon,
            eventCondition: [DataM7S.AbominableBlinkIconId])]
        public void P2_AbominableBlinkTankSupport(Event @event, ScriptAccessory accessory)
        {
            if (!AutoTankSupportEnable) return;
            uint tarId = (uint)@event.TargetId;
            IPlayerCharacter? myChara = accessory.Data.MyObject;
            if (myChara is null || !myChara.IsTank() || myChara.IsDead) return;
            List<IBattleChara> thornyDeathmatchPlayers = new List<IBattleChara>();
            try
            {
                foreach (uint id in accessory.Data.PartyList)
                {
                    IGameObject obj = accessory.Data.Objects.SearchById((ulong)id);
                    if (obj is IBattleChara bc && !bc.IsDead && (
                        bc.HasStatus((uint)DataM7S.SID.ThornyDeathmatchI)
                        || bc.HasStatus((uint)DataM7S.SID.ThornyDeathmatchII)
                        || bc.HasStatus((uint)DataM7S.SID.ThornyDeathmatchIII)
                        || bc.HasStatus((uint)DataM7S.SID.ThornyDeathmatchIV)
                        || bc.HasStatus((uint)DataM7S.SID.ThornsOfDeathI)
                        || bc.HasStatus((uint)DataM7S.SID.ThornsOfDeathII)
                        || bc.HasStatus((uint)DataM7S.SID.ThornsOfDeathIII)
                        || bc.HasStatus((uint)DataM7S.SID.ThornsOfDeathIV)
                    ))
                    {
                        thornyDeathmatchPlayers.Add(bc);
                    }
                }
            }
            catch (System.Exception ex)
            {
                accessory.Log.Error($"Exception getting thorn entanglement players => {ex}");
            }
            if (thornyDeathmatchPlayers.Count == 0) return;
            uint toSupportId = 0;
            if (thornyDeathmatchPlayers.Count == 1)
            {
                // Only one thorn entanglement player, usually first aggro, give them support mitigation
                if (tarId != accessory.Data.Me) toSupportId = tarId;
            }
            else
            {
                // Check if the player tethered on the short side is closer to the boss.
                IGameObject bossObj = accessory.Data.Objects.GetByDataId((uint)DataM7S.OID.BruteAbombinator).FirstOrDefault();
                Vector3 bossPos = DataM7S.P2_FieldCenter;
                if (bossObj is not null)
                {
                    bossPos = bossObj.Position;
                }
                bool isBossNearWall = bossPos.Z < DataM7S.P2_FieldCenter.Z;
                if (isBossNearWall)
                {
                    // Boss is near the short wall, give support mitigation to the non-tank player on the short side
                    IGameObject? toSupportObj = thornyDeathmatchPlayers.Where(bc => !bc.IsTank())
                                                                       .OrderBy(bc => Util.DistanceByTwoPoints(bc.Position, new Vector3(0, 0, -35) + DataM7S.P2_FieldCenter))
                                                                       .FirstOrDefault();
                    if (toSupportObj is not null)
                    {
                        toSupportId = toSupportObj.EntityId;
                        accessory.Log.Debug($"P2 AbominableBlinkTankSupport: Giving support mitigation to short side tether player {toSupportId}");
                    }
                }
                else
                {
                    // Boss is away from the short wall, give support mitigation to the target tank
                    if (tarId != accessory.Data.Me) toSupportId = tarId;
                }
            }
            if (toSupportId == 0) return;
            // Give support mitigation to target tank
            uint mySupportActionId = accessory.MyJob() switch
            {
                EX.Job.WAR => 16464, // Nascent Flash
                EX.Job.PLD => 7382, // Intervention
                EX.Job.DRK => 7393, // The Blackest Night
                EX.Job.GNB => 25758, // Heart of Corundum
                _ => 0,
            };
            if (mySupportActionId == 0) return;
            accessory.Log.Debug($"P2 AbominableBlinkTankSupport: Automatically giving support mitigation to {toSupportId}");
            Task.Run(async () =>
            {
                await Task.Delay(1800); // Wait 1800 milliseconds
                IGameObject? toSupportObj = accessory.Data.Objects.SearchById((ulong)toSupportId);
                accessory.Method.SendChat($"/e Attempting to give support mitigation to {toSupportObj} <se.5>");
                for (int j = 0; j < 6; j++)
                {
                    try
                    {
                        if (toSupportObj is IBattleChara _bc && !_bc.IsDead && !accessory.Data.MyObject.IsDead)
                        {
                            accessory.Method.UseAction(toSupportObj.EntityId, mySupportActionId);
                            accessory.Log.Debug($"Auto support mitigation => {accessory.MyJob()} to {toSupportObj}");
                        }
                        await Task.Delay(500); // Wait 500 milliseconds
                    }
                    catch (System.Exception ex)
                    {
                        accessory.Log.Error($"Auto support mitigation exception => {ex}");
                    }
                }
            });
        }
        [ScriptMethod(name: "P2/P3 Brutish Swing (Jump + Circle/Donut) Count",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: [DataM7S.BrutishSwingActionId_P2P3],
            userControl: false)]
        public void P2P3_BrutishSwingCastingCountCalc(Event @event, ScriptAccessory accessory)
        {
            if (@event.SourcePosition.Y > -100)
            {
                P2_BrutishSwingCastedCount++;
                P3_BrutishSwingCastedCount = 0;
            }
            else
            {
                P3_BrutishSwingCastedCount++;
                P2_BrutishSwingCastedCount = 0;
            }

        }

        [ScriptMethod(name: "P2/P3 Brutish Swing (Jump + Circle/Donut) Dangerous Zone Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.BrutishSwingActionId_P2P3])]
        public void P2P3_BrutishSwingDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            /*
                Draw weapon + jump danger zone at target location
            */
            uint actionId = @event.ActionId;
            ulong bossId = @event.SourceId;
            Vector3 effectPos = @event.EffectPosition;
            long destoryAt = @event.SourcePosition.Y < -100 ? 5700 : 7600;
            (long, long) delay_destoryAt = new(0, destoryAt);
            float radius_Stick = 25.0f;
            float radius_Machete = 22.0f;

            if (P3_BrutishSwingCastedCount >= 2 && actionId == (uint)DataM7S.AID.BrutishSwingMachete_P3)
            {
                delay_destoryAt = new(1160, destoryAt - 1160);
            }
            else if (P2_BrutishSwingCastedCount >= 1 && actionId == (uint)DataM7S.AID.BrutishSwingMachete_P2)
            {
                delay_destoryAt = new(2460, destoryAt - 2460);
            }
            // if (LashingLariatCastingCount > 1)
            // {
            //     // For the second set of dual-weapon jumps in P3, ice flowers need to be placed, drawing delayed slightly
            //     // Donut delayed, Circle not delayed
            //     if (actionId == (uint)DataM7S.AID.BrutishSwingMachete_P3)
            //     {
            //         delay_destoryAt = new(1160, destoryAt - 1160);
            //     }
            // }
            // else if (StrangeSeedsCount > 0)
            // {
            //     // The weapon jump after the ice flower also needs to be delayed, Donut delayed, Circle not delayed
            //     // â†‘ Oh my, the donut after GA-100's three-through-one also needs to be delayed, otherwise the safe zone on the arena floor won't be visible
            //     if (actionId == (uint)DataM7S.AID.BrutishSwingMachete_P2)
            //     {
            //         delay_destoryAt = new(2460, destoryAt - 2460);
            //     }
            // }
            // else if (IsAbominableBlinkCasting)
            // {
            //     IsAbominableBlinkCasting = false;
            //     if (actionId == (uint)DataM7S.AID.BrutishSwingMachete_P2)
            //     {
            //         delay_destoryAt = new(2460, destoryAt - 2460);
            //     }
            // }
            switch (actionId)
            {
                case (uint)DataM7S.AID.BrutishSwingStick_P2:
                case (uint)DataM7S.AID.BrutishSwingStick_P3:
                    accessory.FastDraw(DrawTypeEnum.Circle, effectPos, new Vector2(radius_Stick, radius_Stick), delay_destoryAt, false);
                    break;
                case (uint)DataM7S.AID.BrutishSwingMachete_P2:
                case (uint)DataM7S.AID.BrutishSwingMachete_P3:
                    accessory.FastDraw(DrawTypeEnum.Donut, effectPos, new Vector2(radius_Machete * 4, radius_Machete), delay_destoryAt, false);
                    break;
            }
        }

        [ScriptMethod(name: "P2 Brutish Swing Guide Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.BrutishSwingActionId_P2P3])]
        public void P2_BrutishSwingGuideDraw(Event @event, ScriptAccessory accessory)
        {

            Vector3 tarPos = @event.EffectPosition;
            if (tarPos.Y < -100) return;
            /*
                There are three situations: on the long wall, close to the short wall
                           on the long wall, far from the short wall
                           on the short wall
            */
            const string JumpToFarWall = "Jump to Far Wall";
            const string JumpToNearWall = "Jump to Near Wall";
            const string JumpToShortWall = "Jump to Short Wall";
            string bossJumpType = JumpToShortWall;
            if (MathF.Abs(tarPos.X - DataM7S.P2_FieldCenter.X) < 0.5f)
            {
                // On the short wall
                bossJumpType = JumpToShortWall;
            }
            else if (tarPos.Z < DataM7S.P2_FieldCenter.Z)
            {
                // On the long wall, close to the short wall
                bossJumpType = JumpToNearWall;
            }
            else
            {
                // On the long wall, far from the short wall
                bossJumpType = JumpToFarWall;
            }
            bool isStick = @event.ActionId == (uint)DataM7S.AID.BrutishSwingStick_P2;
            Vector3 startPos = DataM7S.P2_FieldCenter;

            EX.PlayerRoleEnum myRole = accessory.GetMyRole();

            if (isStick)
            {
                // Many situations
                startPos = (myRole, bossJumpType, WalkthroughType) switch
                {
                    (EX.PlayerRoleEnum.MT, JumpToNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(3, 0, -22.5f) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.MT, JumpToFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-3, 0, 22.5f) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.MT, JumpToShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-11, 0, -16.5f) + DataM7S.P2_FieldCenter,

                    (EX.PlayerRoleEnum.ST, JumpToNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(3, 0, -2.5f) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.ST, JumpToFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-3, 0, 2.5f) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.ST, JumpToShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(11, 0, -16.5f) + DataM7S.P2_FieldCenter,

                    (EX.PlayerRoleEnum.H1, JumpToNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(0, 0, 3) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.H1, JumpToFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(0, 0, -3) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.H1, JumpToShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-11, 0, -10) + DataM7S.P2_FieldCenter,

                    (EX.PlayerRoleEnum.H2, JumpToNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(0, 0, -3) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.H2, JumpToFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(0, 0, 3) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.H2, JumpToShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(11, 0, -10) + DataM7S.P2_FieldCenter,

                    (EX.PlayerRoleEnum.D1, JumpToNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(3, 0, -22.5f) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.D1, JumpToFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-3, 0, 22.5f) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.D1, JumpToShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-11, 0, -16.5f) + DataM7S.P2_FieldCenter,

                    (EX.PlayerRoleEnum.D2, JumpToNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(3, 0, -2.5f) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.D2, JumpToFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-3, 0, 2.5f) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.D2, JumpToShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(11, 0, -16.5f) + DataM7S.P2_FieldCenter,

                    (EX.PlayerRoleEnum.D3, JumpToNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-10, 0, 3) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.D3, JumpToFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(10, 0, -3) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.D3, JumpToShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-11, 0, 3) + DataM7S.P2_FieldCenter,

                    (EX.PlayerRoleEnum.D4, JumpToNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-10, 0, -3) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.D4, JumpToFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(10, 0, 3) + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.D4, JumpToShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(11, 0, 3) + DataM7S.P2_FieldCenter,

                    _ => startPos,
                };
            }
            else
            {
                // Fewer situations, just go to the target circle
                Vector3 inCircleLeft = new Vector3(8, 0, -22);
                Vector3 inCircleRight = new Vector3(8, 0, -3);
                if (bossJumpType == JumpToFarWall)
                {
                    // On the long wall, far from the short wall
                    inCircleLeft = new Vector3(-inCircleLeft.X, 0, -inCircleLeft.Z);
                    inCircleRight = new Vector3(-inCircleRight.X, 0, -inCircleRight.Z);
                }
                else if (bossJumpType == JumpToShortWall)
                {
                    // On the short wall
                    inCircleLeft = new Vector3(-11, 0, -22);
                    inCircleRight = new Vector3(11, 0, -22);
                }
                startPos = (myRole, bossJumpType, WalkthroughType) switch
                {
                    (EX.PlayerRoleEnum.MT, _, WalkthroughEnum.MMW_SPJP) => inCircleLeft + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.ST, _, WalkthroughEnum.MMW_SPJP) => inCircleRight + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.H1, JumpToShortWall, WalkthroughEnum.MMW_SPJP) => inCircleLeft + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.H1, _, WalkthroughEnum.MMW_SPJP) => inCircleRight + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.H2, _, WalkthroughEnum.MMW_SPJP) => inCircleRight + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.D1, _, WalkthroughEnum.MMW_SPJP) => inCircleLeft + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.D2, _, WalkthroughEnum.MMW_SPJP) => inCircleRight + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.D3, JumpToShortWall, WalkthroughEnum.MMW_SPJP) => inCircleLeft + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.D3, _, WalkthroughEnum.MMW_SPJP) => inCircleRight + DataM7S.P2_FieldCenter,
                    (EX.PlayerRoleEnum.D4, _, WalkthroughEnum.MMW_SPJP) => inCircleRight + DataM7S.P2_FieldCenter,
                    _ => startPos,
                };
            }



            /*
               There are three situations: on the long wall, close to the short wall
                          on the long wall, far from the short wall
                          on the short wall
           */
            // Vector3 bossPos = @event.SourcePosition;
            const string OnNearWall = "On Near Wall";
            const string OnFarWall = "On Far Wall";
            const string OnShortWall = "On Short Wall";
            string bossWallType = OnShortWall;
            if (MathF.Abs(tarPos.X - DataM7S.P2_FieldCenter.X) < 0.5f)
            {
                // On the short wall
                bossWallType = OnShortWall;
            }
            else if (tarPos.Z < DataM7S.P2_FieldCenter.Z)
            {
                // On the long wall, close to the short wall
                bossWallType = OnNearWall;
            }
            else
            {
                // On the long wall, far from the short wall
                bossWallType = OnFarWall;
            }
            Vector3 endPos = DataM7S.P2_FieldCenter;
            // EX.PlayerRoleEnum myRole = accessory.GetMyRole();
            endPos = (myRole, bossWallType, WalkthroughType) switch
            {
                (EX.PlayerRoleEnum.MT, OnNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(5, 0, -20) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.MT, OnFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-5, 0, 20) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.MT, OnShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-7.5f, 0, -16.5f) + DataM7S.P2_FieldCenter,

                (EX.PlayerRoleEnum.ST, OnNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(7, 0, -5) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.ST, OnFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-7, 0, 5) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.ST, OnShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(7.5f, 0, -16.5f) + DataM7S.P2_FieldCenter,

                (EX.PlayerRoleEnum.H1, OnNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(0, 0, 10) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.H1, OnFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(0, 0, -10) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.H1, OnShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-12, 0, -8.5f) + DataM7S.P2_FieldCenter,

                (EX.PlayerRoleEnum.H2, OnNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-2, 0, -5) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.H2, OnFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(2, 0, 5) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.H2, OnShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(12, 0, -8.5f) + DataM7S.P2_FieldCenter,

                (EX.PlayerRoleEnum.D1, OnNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(12, 0, -24.5f) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.D1, OnFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-12, 0, 24.5f) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.D1, OnShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-12, 0, -24.5f) + DataM7S.P2_FieldCenter,

                (EX.PlayerRoleEnum.D2, OnNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(12, 0, 3) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.D2, OnFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-12, 0, -3) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.D2, OnShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(12, 0, -24.5f) + DataM7S.P2_FieldCenter,

                (EX.PlayerRoleEnum.D3, OnNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-10, 0, 10) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.D3, OnFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(10, 0, -10) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.D3, OnShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-7.5f, 0, 2) + DataM7S.P2_FieldCenter,

                (EX.PlayerRoleEnum.D4, OnNearWall, WalkthroughEnum.MMW_SPJP) => new Vector3(-12, 0, -5) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.D4, OnFarWall, WalkthroughEnum.MMW_SPJP) => new Vector3(12, 0, 5) + DataM7S.P2_FieldCenter,
                (EX.PlayerRoleEnum.D4, OnShortWall, WalkthroughEnum.MMW_SPJP) => new Vector3(7.5f, 0, 2) + DataM7S.P2_FieldCenter,

                _ => endPos,
            };

            if (P2GlowerPowerGuideDrawEnabled)
            {
                accessory.MultiDisDraw(new List<EX.DisplacementContainer>
                {
                    new(startPos, 0, 8000),
                    new(endPos, 0, 5700)
                }, MultiDisProp);
            }
            else
            {
                accessory.MultiDisDraw(new List<EX.DisplacementContainer>
                {
                    new(startPos, 0, 8000),
                }, MultiDisProp);
            }

        }



        [ScriptMethod(name: "P2 Glower Power Dangerous Zone Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.GlowerPowerActionId])]
        public void P2_GlowerPowerDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            /*
                Mouth cannon draw line + spread following the player
            */
            // long destoryAt = @event.DurationMilliseconds();
            long destoryAt = 4000;
            (long, long) delay_destoryAt = new(0, destoryAt);
            // (long, long) delay_destoryAtRect = new (0, destoryAt - 1300);
            float radius_AOE = 6.0f;

            accessory.FastDraw(DrawTypeEnum.Rect, @event.SourceId, new Vector2(14.0f, 65.0f), delay_destoryAt, false);
            foreach (uint playerId in accessory.Data.PartyList)
            {
                ulong _id = (ulong)playerId;
                // Filter out dead players
                IGameObject obj = accessory.Data.Objects.SearchById(_id);
                if (obj is null || obj.IsDead)
                {
                    continue;
                }
                accessory.FastDraw(DrawTypeEnum.Circle, _id, new Vector2(radius_AOE, radius_AOE), delay_destoryAt, false);
            }

        }

        [ScriptMethod(name: "P2 Demolition Deathmatch Guide Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.DemolitionDeathmatchActionId])]
        public void P2_DemolitionDeathmatchGuideDraw(Event @event, ScriptAccessory accessory)
        {
            Vector3 bossPos = @event.SourcePosition;
            Vector3 bossToC = DataM7S.P2_FieldCenter - bossPos;

            EX.PlayerRoleEnum myRole = accessory.GetMyRole();

            Vector3 myEndPos = (myRole, WalkthroughType) switch
            {
                (EX.PlayerRoleEnum.D3, WalkthroughEnum.MMW_SPJP) => DataM7S.P2_FieldCenter + new Vector3(0, 0, -24),
                (EX.PlayerRoleEnum.D4, WalkthroughEnum.MMW_SPJP) => DataM7S.P2_FieldCenter + new Vector3(0.35f * bossToC.X, 0, 0.35f * bossToC.Z),
                _ => DataM7S.P2_FieldCenter
            };
            if (myEndPos == DataM7S.P2_FieldCenter) return; // Filter out those not inside the arena
            accessory.MultiDisDraw(new List<EX.DisplacementContainer> { new(myEndPos, 0, 4500) }, MultiDisProp);
        }

        [ScriptMethod(name: "P2 Strange Seeds Pre Guide Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.StrangeSeedsVisualActionId_P2])]
        public void P2_StrangeSeedsPreGuideDraw(Event @event, ScriptAccessory accessory)
        {
            bool isBossNearWall = @event.SourcePosition.Z < DataM7S.P2_FieldCenter.Z;
            bool isFixedStrangeSeeds = P2StrangeSeedsFixed;
            bool isHintFull = !P2StrangeSeedsSimpleStyle;
            (long Delay, long DestoryAt) delay_destoryAt = new(0, 5500);
            if (isFixedStrangeSeeds)
            {
                // Boss near wall situation
                Vector3 fixedMT = new Vector3(9.7f, 0, -12.5f);
                Vector3 fixedST = new Vector3(12, 0, -24.5f);
                Vector3 fixedD1 = new Vector3(4.2f, 0, -10);
                Vector3 fixedD2 = new Vector3(7.2f, 0, -2.8f);
                Vector3 fixedH1 = new Vector3(0.9f, 0, 7.2f);
                Vector3 fixedH2 = new Vector3(-1.3f, 0, 13.8f);
                Vector3 fixedD3 = new Vector3(-4.8f, 0, -19.7f);
                Vector3 fixedD4 = new Vector3(-9.2f, 0, 9.7f);

                if (!isBossNearWall)
                {
                    // Boss away from wall situation
                    fixedMT = new Vector3(-fixedMT.X, 0, -fixedMT.Z);
                    fixedST = new Vector3(-fixedST.X, 0, -fixedST.Z);
                    fixedD1 = new Vector3(-fixedD1.X, 0, -fixedD1.Z);
                    fixedD2 = new Vector3(-fixedD2.X, 0, -fixedD2.Z);
                    fixedH1 = new Vector3(-fixedH1.X, 0, -fixedH1.Z);
                    fixedH2 = new Vector3(-fixedH2.X, 0, -fixedH2.Z);
                    fixedD3 = new Vector3(4.6f, 0, fixedD3.Z);
                    fixedD4 = new Vector3(-fixedD4.X, 0, -fixedD4.Z);
                }
                Vector2 _size = new Vector2(0.5f, 0.5f);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedMT + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedST + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedH1 + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedH2 + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedD1 + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedD2 + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedD3 + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedD4 + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);


                return;
            }
            else
            {
                Vector3 oddForMeleeMMW = new Vector3(12, 0, -24.5f);
                Vector3 evenForMeleeMMW = new Vector3(3.4f, 0, -21);
                Vector3 oddForTHMMW = new Vector3(12, 0, 0);
                Vector3 evenForTHMMW = new Vector3(3.4f, 0, -3.4f);
                Vector3 oddForD4MMW = new Vector3(-12, 0, 0);
                Vector3 evenForD4MMW = new Vector3(-7.2f, 0, 14);
                Vector3 oddForD3MMW = new Vector3(-12, 0, -24.5f);
                Vector3 evenForD3MMW = new Vector3(-3.4f, 0, -21);
                Vector3 safePos1MMW = new Vector3(6.5f, 0, -12.5f);
                Vector3 safePos2MMW = new Vector3(7.8f, 0, 10);
                if (!isBossNearWall)
                {
                    // Boss location has no adjacent wall, rotate 180 degrees
                    oddForMeleeMMW = new Vector3(-oddForMeleeMMW.X, 0, -oddForMeleeMMW.Z);
                    evenForMeleeMMW = new Vector3(-evenForMeleeMMW.X, 0, -evenForMeleeMMW.Z);
                    oddForTHMMW = new Vector3(-oddForTHMMW.X, 0, -oddForTHMMW.Z);
                    evenForTHMMW = new Vector3(-evenForTHMMW.X, 0, -evenForTHMMW.Z);
                    oddForD4MMW = new Vector3(-oddForD4MMW.X, 0, -oddForD4MMW.Z);
                    evenForD4MMW = new Vector3(-evenForD4MMW.X, 0, -evenForD4MMW.Z);
                    safePos1MMW = new Vector3(-safePos1MMW.X, 0, -safePos1MMW.Z);
                    safePos2MMW = new Vector3(-safePos2MMW.X, 0, -safePos2MMW.Z);
                }
                else
                {
                    // Boss adjacent to short wall, D3's even round shares a point with melee
                    evenForD3MMW = evenForMeleeMMW;
                }

                if (WalkthroughType == WalkthroughEnum.MMW_SPJP)
                {

                    Vector2 size = new Vector2(0.6f, 0.3f);
                    delay_destoryAt.DestoryAt += 1000; // Draw for one more second
                    accessory.FastDraw(DrawTypeEnum.Circle, safePos1MMW + DataM7S.P2_FieldCenter, new Vector2(2.0f, 2.0f), delay_destoryAt, accessory.Data.DefaultSafeColor.WithW(0.25f), GuideDrawMode);
                    accessory.FastDraw(DrawTypeEnum.Circle, safePos2MMW + DataM7S.P2_FieldCenter, new Vector2(2.0f, 2.0f), delay_destoryAt, accessory.Data.DefaultSafeColor.WithW(0.25f), GuideDrawMode);

                    // Only draw for my own role
                    switch (accessory.GetMyRole())
                    {
                        case EX.PlayerRoleEnum.MT:
                        case EX.PlayerRoleEnum.ST:
                        case EX.PlayerRoleEnum.H1:
                        case EX.PlayerRoleEnum.H2:
                            accessory.FastDraw(DrawTypeEnum.Donut, oddForTHMMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountOdd.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            accessory.FastDraw(DrawTypeEnum.Donut, evenForTHMMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountEven.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            break;
                        case EX.PlayerRoleEnum.D1:
                        case EX.PlayerRoleEnum.D2:
                            accessory.FastDraw(DrawTypeEnum.Donut, oddForMeleeMMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountOdd.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            accessory.FastDraw(DrawTypeEnum.Donut, evenForMeleeMMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountEven.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            break;
                        case EX.PlayerRoleEnum.D3:
                            // If boss is near short wall, D3's point near the center of the arena, shared with melee
                            accessory.FastDraw(DrawTypeEnum.Donut, oddForD3MMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountOdd.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            accessory.FastDraw(DrawTypeEnum.Donut, evenForD3MMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountEven.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            break;
                        case EX.PlayerRoleEnum.D4:
                            accessory.FastDraw(DrawTypeEnum.Donut, oddForD4MMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountOdd.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            accessory.FastDraw(DrawTypeEnum.Donut, evenForD4MMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountEven.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            break;
                    }

                }
            }
        }

        [ScriptMethod(name: "P2 Strange Seeds Counts Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.StrangeSeedsActionId])]
        // suppress : 1000)]
        public void P2_StrangeSeedsCountsDraw(Event @event, ScriptAccessory accessory)
        {
            /*
                When an ice flower is marked, indicate whether it's an odd or even number through coloring
                Since the one marked on yourself might be suppressed, not using that feature
            */
            if (@event.SourcePosition.Y < -100) return; // Filter out events not in the arena
            uint count = 0;
            lock (_lock)
            {
                count = StrangeSeedsCount++;
            }
            ulong tarId = @event.TargetId;
            // (long Delay, long DestoryAt) delay_destoryAt = new(0, (long)@event.DurationMilliseconds());
            (long Delay, long DestoryAt) delay_destoryAt = new(0, 5000);
            bool isOdd = count % 4 == 0 || count % 4 == 1;
            Vector4 color = isOdd ? StrangeSeedsCountOdd.V4.WithW(P2StrangeSeedsColorDensity) : StrangeSeedsCountEven.V4.WithW(P2StrangeSeedsColorDensity).WithW(P2StrangeSeedsColorDensity);
            accessory.Log.Debug($"Strange Seeds Counts Draw : count => {count / 2 + 1}");

            bool isBossNearWall = @event.SourcePosition.Z < DataM7S.P2_FieldCenter.Z;
            bool isFixedStrangeSeeds = P2StrangeSeedsFixed;
            bool isHintFull = !P2StrangeSeedsSimpleStyle;
            if (isFixedStrangeSeeds)
            {
                // Boss near wall situation
                Vector3 fixedMT = new Vector3(9.7f, 0, -12.5f);
                Vector3 fixedST = new Vector3(12, 0, -24.5f);
                Vector3 fixedD1 = new Vector3(4.2f, 0, -10);
                Vector3 fixedD2 = new Vector3(7.2f, 0, -2.8f);
                Vector3 fixedH1 = new Vector3(0.9f, 0, 7.2f);
                Vector3 fixedH2 = new Vector3(-1.3f, 0, 13.8f);
                Vector3 fixedD3 = new Vector3(-4.8f, 0, -19.7f);
                Vector3 fixedD4 = new Vector3(-9.2f, 0, 9.7f);

                if (!isBossNearWall)
                {
                    // Boss away from wall situation
                    fixedMT = new Vector3(-fixedMT.X, 0, -fixedMT.Z);
                    fixedST = new Vector3(-fixedST.X, 0, -fixedST.Z);
                    fixedD1 = new Vector3(-fixedD1.X, 0, -fixedD1.Z);
                    fixedD2 = new Vector3(-fixedD2.X, 0, -fixedD2.Z);
                    fixedH1 = new Vector3(-fixedH1.X, 0, -fixedH1.Z);
                    fixedH2 = new Vector3(-fixedH2.X, 0, -fixedH2.Z);
                    fixedD3 = new Vector3(4.6f, 0, fixedD3.Z);
                    fixedD4 = new Vector3(-fixedD4.X, 0, -fixedD4.Z);
                }
                Vector2 _size = new Vector2(0.5f, 0.5f);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedMT + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedST + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedH1 + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedH2 + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedD1 + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedD2 + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedD3 + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, fixedD4 + DataM7S.P2_FieldCenter, _size, delay_destoryAt, true, GuideDrawMode);


                return;
            }
            if (tarId == accessory.Data.Me)
            {
                accessory.FastDraw(DrawTypeEnum.Circle, tarId, new Vector2(2.0f, 2.0f), delay_destoryAt, color, GuideDrawMode);
            }
            else
            {
                // If I'm not marked with an ice flower, but still give an odd/even indicator
                // If I'm not a tank and have a tether buff
                if (accessory.Data.MyObject is not null
                    && !P2StrangeSeedsSimpleStyle
                    && !accessory.Data.MyObject.IsTank()
                    && (accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchI)
                    || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchII)
                    || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchIII)
                    || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchIV)))
                {
                    color = !isOdd ? StrangeSeedsCountOdd.V4.WithW(P2StrangeSeedsColorDensity) : StrangeSeedsCountEven.V4.WithW(P2StrangeSeedsColorDensity);
                    accessory.FastDraw(DrawTypeEnum.Circle,
                        accessory.Data.Me, new Vector2(1.15f),
                        delay_destoryAt,
                        color.WithW(color.W * 0.3f), GuideDrawMode);
                }
            }


            Vector3 oddForMeleeMMW = new Vector3(12, 0, -24.5f);
            Vector3 evenForMeleeMMW = new Vector3(3.4f, 0, -21);
            Vector3 oddForTHMMW = new Vector3(12, 0, 0);
            Vector3 evenForTHMMW = new Vector3(3.4f, 0, -3.4f);
            Vector3 oddForD4MMW = new Vector3(-12, 0, 0);
            Vector3 evenForD4MMW = new Vector3(-7.2f, 0, 14);
            Vector3 oddForD3MMW = new Vector3(-12, 0, -24.5f);
            Vector3 evenForD3MMW = new Vector3(-3.4f, 0, -21);
            Vector3 safePos1MMW = new Vector3(6.5f, 0, -12.5f);
            Vector3 safePos2MMW = new Vector3(7.8f, 0, 10);

            if (!isBossNearWall)
            {
                // Boss location has no adjacent wall, rotate 180 degrees
                oddForMeleeMMW = new Vector3(-oddForMeleeMMW.X, 0, -oddForMeleeMMW.Z);
                evenForMeleeMMW = new Vector3(-evenForMeleeMMW.X, 0, -evenForMeleeMMW.Z);
                oddForTHMMW = new Vector3(-oddForTHMMW.X, 0, -oddForTHMMW.Z);
                evenForTHMMW = new Vector3(-evenForTHMMW.X, 0, -evenForTHMMW.Z);
                oddForD4MMW = new Vector3(-oddForD4MMW.X, 0, -oddForD4MMW.Z);
                evenForD4MMW = new Vector3(-evenForD4MMW.X, 0, -evenForD4MMW.Z);
                safePos1MMW = new Vector3(-safePos1MMW.X, 0, -safePos1MMW.Z);
                safePos2MMW = new Vector3(-safePos2MMW.X, 0, -safePos2MMW.Z);
            }
            else
            {
                // Boss adjacent to short wall, D3's even round shares a point with melee
                evenForD3MMW = evenForMeleeMMW;
            }

            // Draw odd/even indicator points
            if (WalkthroughType == WalkthroughEnum.MMW_SPJP)
            {
                // (long, long) delay_destoryAt = new(0, 5000);
                Vector2 size = new Vector2(0.6f, 0.3f);
                delay_destoryAt.DestoryAt += 1000; // Draw for one more second
                accessory.FastDraw(DrawTypeEnum.Circle, safePos1MMW + DataM7S.P2_FieldCenter, new Vector2(2.0f, 2.0f), delay_destoryAt, accessory.Data.DefaultSafeColor.WithW(0.25f), GuideDrawMode);
                accessory.FastDraw(DrawTypeEnum.Circle, safePos2MMW + DataM7S.P2_FieldCenter, new Vector2(2.0f, 2.0f), delay_destoryAt, accessory.Data.DefaultSafeColor.WithW(0.25f), GuideDrawMode);
                if (isHintFull)
                {
                    // If drawing everything
                    // Draw all odd/even indicator points
                    accessory.FastDraw(DrawTypeEnum.Donut, oddForMeleeMMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountOdd.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                    accessory.FastDraw(DrawTypeEnum.Donut, evenForMeleeMMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountEven.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                    accessory.FastDraw(DrawTypeEnum.Donut, oddForTHMMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountOdd.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                    accessory.FastDraw(DrawTypeEnum.Donut, evenForTHMMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountEven.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                    accessory.FastDraw(DrawTypeEnum.Donut, oddForD4MMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountOdd.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                    accessory.FastDraw(DrawTypeEnum.Donut, evenForD4MMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountEven.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                    accessory.FastDraw(DrawTypeEnum.Donut, oddForD3MMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountOdd.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                    accessory.FastDraw(DrawTypeEnum.Donut, evenForD3MMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountEven.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                }
                else
                { // Only draw for my own role
                    switch (accessory.GetMyRole())
                    {
                        case EX.PlayerRoleEnum.MT:
                        case EX.PlayerRoleEnum.ST:
                        case EX.PlayerRoleEnum.H1:
                        case EX.PlayerRoleEnum.H2:
                            accessory.FastDraw(DrawTypeEnum.Donut, oddForTHMMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountOdd.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            accessory.FastDraw(DrawTypeEnum.Donut, evenForTHMMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountEven.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            break;
                        case EX.PlayerRoleEnum.D1:
                        case EX.PlayerRoleEnum.D2:
                            accessory.FastDraw(DrawTypeEnum.Donut, oddForMeleeMMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountOdd.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            accessory.FastDraw(DrawTypeEnum.Donut, evenForMeleeMMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountEven.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            break;
                        case EX.PlayerRoleEnum.D3:
                            // If boss is near short wall, D3's point near the center of the arena, shared with melee
                            accessory.FastDraw(DrawTypeEnum.Donut, oddForD3MMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountOdd.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            accessory.FastDraw(DrawTypeEnum.Donut, evenForD3MMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountEven.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            break;
                        case EX.PlayerRoleEnum.D4:
                            accessory.FastDraw(DrawTypeEnum.Donut, oddForD4MMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountOdd.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            accessory.FastDraw(DrawTypeEnum.Donut, evenForD4MMW + DataM7S.P2_FieldCenter, size, delay_destoryAt, StrangeSeedsCountEven.V4.WithW(P2StrangeSeedsColorDensity), GuideDrawMode);
                            break;
                    }
                }
            }
        }
        [ScriptMethod(name: "P2 Killer Seeds Guide Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.KillerSeedsActionId],
            suppress: 1000)]
        public void P2_KillerSeedsGuideDraw(Event @event, ScriptAccessory accessory)
        {
            Vector3 bossPos = @event.SourcePosition;
            if (bossPos.Y < -100) return; // Only draw in P2 phase


            bool isBossNearWall = bossPos.Z < DataM7S.P2_FieldCenter.Z;
            Vector3 groupMT = new Vector3(12, 0, -24.5f);
            Vector3 groupST = new Vector3(12, 0, 0);
            Vector3 groupH1 = new Vector3(-12, 0, -24.5f);
            Vector3 groupH2 = new Vector3(-12, 0, 0);

            if (!isBossNearWall)
            {
                // Boss location has no adjacent wall, rotate 180 degrees
                groupMT = new Vector3(-groupMT.X, 0, -groupMT.Z);
                groupST = new Vector3(-groupST.X, 0, -groupST.Z);
                groupH1 = new Vector3(groupH1.X, 0, groupH1.Z);
                groupH2 = new Vector3(-groupH2.X, 0, -groupH2.Z);
            }


            EX.PlayerRoleEnum myRole = accessory.GetMyRole();

            Vector3 myPos = (WalkthroughType, myRole) switch
            {
                (WalkthroughEnum.MMW_SPJP, EX.PlayerRoleEnum.MT) => groupMT,
                (WalkthroughEnum.MMW_SPJP, EX.PlayerRoleEnum.D1) => groupMT,

                (WalkthroughEnum.MMW_SPJP, EX.PlayerRoleEnum.ST) => groupST,
                (WalkthroughEnum.MMW_SPJP, EX.PlayerRoleEnum.D2) => groupST,

                (WalkthroughEnum.MMW_SPJP, EX.PlayerRoleEnum.H1) => groupH1,
                (WalkthroughEnum.MMW_SPJP, EX.PlayerRoleEnum.D3) => groupH1,

                (WalkthroughEnum.MMW_SPJP, EX.PlayerRoleEnum.H2) => groupH2,
                (WalkthroughEnum.MMW_SPJP, EX.PlayerRoleEnum.D4) => groupH2,
                _ => Vector3.Zero
            };
            // Draw guidance
            EX.DisplacementContainer myEndPos = new(myPos + DataM7S.P2_FieldCenter, 0, 4700);
            accessory.MultiDisDraw(new List<EX.DisplacementContainer> { myEndPos }, MultiDisProp);

        }

        [ScriptMethod(name: "P3 Stoneringer Id Acquisition",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.P3_StoneringerActionId], userControl: false)]
        public void P3_StoneringerActionId(Event @event, ScriptAccessory accessory)
        {
            P3_StoneringerId = @event.ActionId;
            SinisterSeedTargets.Clear(); // Clear ice flower targets
        }

        [ScriptMethod(name: "P3 Lashing Lariat Dangerous Zone Pre Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.BrutishSwingActionId_P2P3])]
        public void P3_LashingLariatDangerousZonePreDraw(Event @event, ScriptAccessory accessory)
        {
            if (@event.SourcePosition.Y > -100) return; // Only draw in P3
            if (P3_BrutishSwingCastedCount == 0 || P3_BrutishSwingCastedCount == 2)
            {
                Vector3 effectPos = @event.EffectPosition;
                uint actionId = @event.ActionId;
                // Determine offset direction, since Owner property cannot be used, must manually calculate coordinates
                // 42400 left hand slash, right hand club
                // 42401 left hand club, right hand slash
                // 42403 club jump
                // 42405 slash jump

                // Assuming jump to the north wall
                Vector3 offset = (P3_StoneringerId, actionId) switch
                {
                    ((uint)DataM7S.AID.Stoneringer2Stoneringers_LStick, (uint)DataM7S.AID.BrutishSwingStick_P3)
                        => new Vector3(9, 0, 0), // left hand club + club jump
                    ((uint)DataM7S.AID.Stoneringer2Stoneringers_LStick, (uint)DataM7S.AID.BrutishSwingMachete_P3)
                        => new Vector3(-9, 0, 0), // left hand club + slash jump
                    ((uint)DataM7S.AID.Stoneringer2Stoneringers_RStick, (uint)DataM7S.AID.BrutishSwingStick_P3)
                        => new Vector3(-9, 0, 0), // right hand club + club jump
                    ((uint)DataM7S.AID.Stoneringer2Stoneringers_RStick, (uint)DataM7S.AID.BrutishSwingMachete_P3)
                        => new Vector3(9, 0, 0), // right hand club + slash jump
                    _ => Vector3.Zero
                };
                Vector3 startPos = new Vector3(0, 0, -35) + DataM7S.P3_FieldCenter + offset;
                Vector3 tarPos = DataM7S.P3_FieldCenter + offset;
                float _rot = MathF.Atan2(effectPos.Z - DataM7S.P3_FieldCenter.Z, effectPos.X - DataM7S.P3_FieldCenter.X);
                float modRot = _rot + MathF.PI / 2;
                startPos = Util.RotatePointInFFXIV(startPos, DataM7S.P3_FieldCenter, modRot);
                tarPos = Util.RotatePointInFFXIV(tarPos, DataM7S.P3_FieldCenter, modRot);

                DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = nameof(P3_LashingLariatDangerousZonePreDraw) + Guid.NewGuid().ToString();
                dp.Delay = 6500;
                dp.DestoryAt = 4000;
                dp.Scale = new Vector2(32.0f, 70.0f);
                dp.Position = startPos;
                dp.TargetPosition = tarPos;
                dp.Color = accessory.Data.DefaultDangerColor.WithW(0.7f);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);


            } 
            

            
        }

        [ScriptMethod(name: "P3 Lashing Lariat Dangerous Zone Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.LashingLariatActionId])]
        public void P3_LashingLariatDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            /*
                P3 charge, draw danger zone
            */

            uint actionId = @event.ActionId;
            ulong bossId = @event.SourceId;
            // Vector3 tarPos = @event.EffectPosition;
            // long destoryAt = (long)@event.DurationMilliseconds();
            long destoryAt = 4000 - 500;
            (long, long) delay_destoryAt = new(0, destoryAt);
            Vector2 scale = new(32.0f, 70.0f);
            // accessory.FastDraw(DrawTypeEnum.Rect, bossId, scale, delay_destoryAt, false);
            float offsetX = actionId == (uint)DataM7S.AID.LashingLariatWithLeftHand ? -9 : 9;

            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = nameof(P3_LashingLariatDangerousZoneDraw) + Guid.NewGuid().ToString();
            dp.Delay = 0;
            dp.DestoryAt = destoryAt;
            dp.Scale = new Vector2(32.0f, 70.0f);
            dp.Owner = @event.SourceId;
            dp.Offset = new Vector3(offsetX, 0, 0);
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }


        [ScriptMethod(name: "P3 Glower Power Dangerous Zone Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.BrutishSwingActionId_P2P3])]
        public void P3_GlowerPowerDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            Vector3 effectPos = @event.EffectPosition;
            if (effectPos.Y > -100) return; // Only draw in P3
            if (P3_BrutishSwingCastedCount == 1)
            {
                long destoryAt = @event.SourcePosition.Y < -100 ? 5700 : 7600;
                (long, long) delay_destoryAtGlower = new(destoryAt, 4100);
                // (long, long) delay_destoryAtRect = new (0, destoryAt - 1300);
                float radius_AOE = 6.0f;
                accessory.FastDraw(DrawTypeEnum.Rect, @event.SourceId, new Vector2(14.0f, 65.0f), delay_destoryAtGlower, false);
                foreach (uint playerId in accessory.Data.PartyList)
                {
                    ulong _id = (ulong)playerId;
                    // Filter out dead players
                    IGameObject obj = accessory.Data.Objects.SearchById(_id);
                    if (obj is null || obj.IsDead)
                    {
                        continue;
                    }
                    accessory.FastDraw(DrawTypeEnum.Circle, _id, new Vector2(radius_AOE, radius_AOE), delay_destoryAtGlower, false);
                }
            }
        }

        [ScriptMethod(name: "P3 Glower Power Guide Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.BrutishSwingActionId_P2P3])]
        public void P3_GlowerPowerGuideDraw(Event @event, ScriptAccessory accessory)
        {
            Vector3 effectPos = @event.EffectPosition;
            if (effectPos.Y > -100) return; // Only draw in P3
            if (P3_BrutishSwingCastedCount == 1)
            {
                // This means it's the second jump, the one with the mouth cannon
                Vector3 myStartPos = DataM7S.P3_FieldCenter;
                Vector3 myEndPos = DataM7S.P3_FieldCenter;
                EX.PlayerRoleEnum myRole = accessory.GetMyRole();
                bool isInSafe = @event.ActionId == (uint)DataM7S.AID.BrutishSwingMachete_P3;
                Vector3 inLeft = new Vector3(-10, 0, -16) + DataM7S.P3_FieldCenter;
                Vector3 inRight = new Vector3(10, 0, -16) + DataM7S.P3_FieldCenter;
                Vector3 outLeft = new Vector3(-10, 0, -10) + DataM7S.P3_FieldCenter;
                Vector3 outRight = new Vector3(10, 0, -10) + DataM7S.P3_FieldCenter;
                // Assume boss jumped to the north wall
                (myStartPos, myEndPos) = (myRole, WalkthroughType, isInSafe) switch
                {
                    (EX.PlayerRoleEnum.MT, WalkthroughEnum.MMW_SPJP, true) => (inLeft, new Vector3(-7.5f, 0, -14) + DataM7S.P3_FieldCenter),
                    (EX.PlayerRoleEnum.MT, WalkthroughEnum.MMW_SPJP, false) => (outLeft, new Vector3(-7.5f, 0, -14) + DataM7S.P3_FieldCenter),

                    (EX.PlayerRoleEnum.ST, WalkthroughEnum.MMW_SPJP, true) => (inRight, new Vector3(7.5f, 0, -14) + DataM7S.P3_FieldCenter),
                    (EX.PlayerRoleEnum.ST, WalkthroughEnum.MMW_SPJP, false) => (outRight, new Vector3(7.5f, 0, -14) + DataM7S.P3_FieldCenter),

                    (EX.PlayerRoleEnum.H1, WalkthroughEnum.MMW_SPJP, true) => (inLeft, new Vector3(-7.5f, 0, -5) + DataM7S.P3_FieldCenter),
                    (EX.PlayerRoleEnum.H1, WalkthroughEnum.MMW_SPJP, false) => (new Vector3(-7.5f, 0, -5) + DataM7S.P3_FieldCenter, new Vector3(-7.5f, 0, -5) + DataM7S.P3_FieldCenter),

                    (EX.PlayerRoleEnum.H2, WalkthroughEnum.MMW_SPJP, true) => (inRight, new Vector3(7.5f, 0, -5) + DataM7S.P3_FieldCenter),
                    (EX.PlayerRoleEnum.H2, WalkthroughEnum.MMW_SPJP, false) => (new Vector3(7.5f, 0, -5) + DataM7S.P3_FieldCenter, new Vector3(7.5f, 0, -5) + DataM7S.P3_FieldCenter),

                    (EX.PlayerRoleEnum.D1, WalkthroughEnum.MMW_SPJP, true) => (inLeft, new Vector3(-15, 0, -19) + DataM7S.P3_FieldCenter),
                    (EX.PlayerRoleEnum.D1, WalkthroughEnum.MMW_SPJP, false) => (outLeft, new Vector3(-15, 0, -19) + DataM7S.P3_FieldCenter),

                    (EX.PlayerRoleEnum.D2, WalkthroughEnum.MMW_SPJP, true) => (inRight, new Vector3(15, 0, -19) + DataM7S.P3_FieldCenter),
                    (EX.PlayerRoleEnum.D2, WalkthroughEnum.MMW_SPJP, false) => (outRight, new Vector3(15, 0, -19) + DataM7S.P3_FieldCenter),

                    (EX.PlayerRoleEnum.D3, WalkthroughEnum.MMW_SPJP, true) => (inLeft, new Vector3(-19, 0, -11) + DataM7S.P3_FieldCenter),
                    (EX.PlayerRoleEnum.D3, WalkthroughEnum.MMW_SPJP, false) => (new Vector3(-10, 0, 10) + DataM7S.P3_FieldCenter, new Vector3(-10, 0, 10) + DataM7S.P3_FieldCenter),

                    (EX.PlayerRoleEnum.D4, WalkthroughEnum.MMW_SPJP, true) => (inRight, new Vector3(15, 0, -9) + DataM7S.P3_FieldCenter),
                    (EX.PlayerRoleEnum.D4, WalkthroughEnum.MMW_SPJP, false) => (new Vector3(10, 0, 10) + DataM7S.P3_FieldCenter, new Vector3(10, 0, 10) + DataM7S.P3_FieldCenter),
                    _ => (myStartPos, myEndPos) // Default value
                };

                // Rotate based on effectPos
                float _rot = MathF.Atan2(effectPos.Z - DataM7S.P3_FieldCenter.Z, effectPos.X - DataM7S.P3_FieldCenter.X);
                float modRot = _rot + MathF.PI / 2;
                myStartPos = Util.RotatePointInFFXIV(myStartPos, DataM7S.P3_FieldCenter, modRot);
                myEndPos = Util.RotatePointInFFXIV(myEndPos, DataM7S.P3_FieldCenter, modRot);
                accessory.MultiDisDraw(new List<EX.DisplacementContainer>
                {
                    new(myStartPos, 0, 5000),
                    new(myEndPos, 0, 5000)
                }, MultiDisProp);
            }
        }

        [ScriptMethod(name: "P3 Debris Deathmatch Guide Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.DebrisDeathmatchActionId])]
        public void P3_DebrisDeathmatchGuideDraw(Event @event, ScriptAccessory accessory)
        {
            Vector3 myEndPos = DataM7S.P3_FieldCenter;
            EX.PlayerRoleEnum myRole = accessory.GetMyRole();
            myEndPos = (myRole, WalkthroughType) switch
            {
                (EX.PlayerRoleEnum.D3, WalkthroughEnum.MMW_SPJP) => DataM7S.P3_FieldCenter + new Vector3(0, 0, -17),
                (EX.PlayerRoleEnum.D4, WalkthroughEnum.MMW_SPJP) => DataM7S.P3_FieldCenter + new Vector3(0, 0, 17),
                (EX.PlayerRoleEnum.H1, WalkthroughEnum.MMW_SPJP) => DataM7S.P3_FieldCenter + new Vector3(-17, 0, 0),
                (EX.PlayerRoleEnum.H2, WalkthroughEnum.MMW_SPJP) => DataM7S.P3_FieldCenter + new Vector3(17, 0, 0),
                _ => myEndPos // Or throw exception, or return default value like DataM7S.P3_FieldCenter
            };
            if (myEndPos == DataM7S.P3_FieldCenter) return;
            accessory.MultiDisDraw(new List<EX.DisplacementContainer> { new(myEndPos, 0, 4000) }, MultiDisProp);
        }
        [ScriptMethod(name: "P3 Pollen Guide Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.PollenActionId])]
        public void P3_PollenGuideDraw(Event @event, ScriptAccessory accessory)
        {
            Vector3 pos = @event.EffectPosition;
            if (pos.Y > -100) return; // Only draw in P3 phase
            if (Util.DistanceByTwoPoints(pos, DataM7S.P3_FieldCenter) < 22.5f
                || pos.Z < DataM7S.P3_FieldCenter.Z) return; // Only detect lower left or lower right corner
            bool isMeGetThornyDeathmatch = accessory.Data.MyObject is not null
                && (accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchI)
                || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchII)
                || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchIII)
                || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchIV));
            if (!isMeGetThornyDeathmatch) return; // Only draw if I have a tether
            bool isLeftDownSafe = pos.X > DataM7S.P3_FieldCenter.X;
            List<Vector3> safePosList = isLeftDownSafe ?
                new()
                {
                    new Vector3(-19, 0, 19) + DataM7S.P3_FieldCenter,
                    new Vector3(19, 0, -19) + DataM7S.P3_FieldCenter,
                } :
                new()
                {
                    new Vector3(19, 0, 19) + DataM7S.P3_FieldCenter,
                    new Vector3(-19, 0, -19) + DataM7S.P3_FieldCenter,
                };
            // Guide to the nearest point
            Vector3 _myPos = safePosList.OrderBy(p => Util.DistanceByTwoPoints(p, accessory.Data.MyObject.Position)).FirstOrDefault();
            // Move vertically or horizontally from the point by a modLength
            List<Vector3> stackPosList = new()
            {
                DataM7S.P3_FieldCenter + new Vector3(0, 0, -25),
                DataM7S.P3_FieldCenter + new Vector3(0, 0, 25),
                DataM7S.P3_FieldCenter + new Vector3(-25, 0, 0),
                DataM7S.P3_FieldCenter + new Vector3(25, 0, 0)
            };
            Vector3 _myStackPos = stackPosList.OrderBy(p => Util.DistanceByTwoPoints(p, accessory.Data.MyObject.Position)).FirstOrDefault();
            Vector3 myStackPos = (_myStackPos - DataM7S.P3_FieldCenter) * 0.76f + DataM7S.P3_FieldCenter; // Scale down to 0.76
            Vector3 myPos = (_myPos - myStackPos) * 0.72f + myStackPos; // Scale down to 0.72
            accessory.MultiDisDraw(new List<EX.DisplacementContainer> {
                new(myPos, 0, 4000),
                new(myStackPos, 0, 4000),
                }, MultiDisProp);

        }
        [ScriptMethod(name: "P3 Killer Seeds Guide Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.KillerSeedsActionId],
            suppress: 1000)]
        public void P3_KillerSeedsGuideDraw(Event @event, ScriptAccessory accessory)
        {
            Vector3 bossPos = @event.SourcePosition;
            if (bossPos.Y > -100) return; // Only draw in P3 phase

            bool _isMeGetThornyDeathmatch = accessory.Data.MyObject is not null
                && (accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchI)
                || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchII)
                || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchIII)
                || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchIV));
            if (_isMeGetThornyDeathmatch) return; // Tether players' drawing is in the previous one

            Vector3 myPos = Vector3.Zero;
            Vector3 groupD3 = new Vector3(0, 0, -19);
            Vector3 groupD4 = new Vector3(0, 0, 19);
            Vector3 groupH1 = new Vector3(-19, 0, 0);
            Vector3 groupH2 = new Vector3(19, 0, 0);
            switch (WalkthroughType)
            {
                case WalkthroughEnum.MMW_SPJP:
                    myPos = accessory.GetMyRole() switch
                    {
                        EX.PlayerRoleEnum.MT => groupD3,
                        EX.PlayerRoleEnum.ST => groupD4,
                        EX.PlayerRoleEnum.H1 => groupH1,
                        EX.PlayerRoleEnum.H2 => groupH2,
                        EX.PlayerRoleEnum.D1 => groupH1,
                        EX.PlayerRoleEnum.D2 => groupH2,
                        EX.PlayerRoleEnum.D3 => groupD3,
                        EX.PlayerRoleEnum.D4 => groupD4,
                        _ => Vector3.Zero
                    };
                    myPos += DataM7S.P3_FieldCenter;
                    bool isMeGetThornyDeathmatch = accessory.Data.MyObject is not null
                        && (accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchI)
                        || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchII)
                        || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchIII)
                        || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchIV));
                    if (isMeGetThornyDeathmatch)
                    {
                        // I have a tether, go to the nearest stack point
                        List<Vector3> posList = new()
                        {
                            new Vector3(0, 0, -25) + DataM7S.P3_FieldCenter,
                            new Vector3(0, 0, 25) + DataM7S.P3_FieldCenter,
                            new Vector3(-25, 0, 0) + DataM7S.P3_FieldCenter,
                            new Vector3(25, 0, 0) + DataM7S.P3_FieldCenter
                        };
                        posList = posList.OrderBy(p => Util.DistanceByTwoPoints(p, accessory.Data.MyObject.Position)).ToList();
                        Vector3 _modPos = posList[0] - DataM7S.P3_FieldCenter;
                        myPos = _modPos * 0.76f + DataM7S.P3_FieldCenter; // Scale down to 0.76
                    }
                    // Draw guidance
                    EX.DisplacementContainer myEndPos = new(myPos, 0, 4700);
                    accessory.MultiDisDraw(new List<EX.DisplacementContainer> { myEndPos }, MultiDisProp);
                    break;
            }

        }

        [ScriptMethod(name: "P3 Sinister Seeds Blossom Pre Guide Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.SinisterSeedsVisualActionId])]
        public void P3_SinisterSeedsBlossomPreGuideDraw(Event @event, ScriptAccessory accessory)
        {
            SinisterSeedTargets.Clear(); // Clear ice flower targets
            if (@event.SourcePosition.Y > -100) return; // Only draw in P3 phase
            bool isMeGetThornyDeathmatch = accessory.Data.MyObject is not null
                && (accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchI)
                || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchII)
                || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchIII)
                || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchIV));
            if (!isMeGetThornyDeathmatch)
            {
                switch (WalkthroughType)
                {
                    case WalkthroughEnum.MMW_SPJP:
                        accessory.MultiDisDraw(new List<EX.DisplacementContainer> {
                            new(new Vector3(10, 0, -4) + DataM7S.P3_FieldCenter, 0, 4900),
                            }, MultiDisProp);
                        break;
                }
            }
        }

        [ScriptMethod(name: "P3 Sinister Seeds Blossom Guide Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.SinisterSeedsBlossomActionId42350])]
        public void P3_SinisterSeedsBlossomGuideDraw(Event @event, ScriptAccessory accessory)
        {
            if (@event.SourcePosition.Y > -100) return; // Only draw in P3 phase
            ulong tarId = @event.TargetId;
            int count = -1;
            lock (_lock)
            {
                if (!SinisterSeedTargets.Contains(tarId))
                {
                    accessory.Log.Debug($"P3 Sinister Seeds Blossom Guide Draw : add targetId => {tarId}");
                    SinisterSeedTargets.Add(tarId); // Record ice flower targets
                    // SinisterSeedTargets.Add(tarId); // Record ice flower targets
                    count = SinisterSeedTargets.Count;
                }
                else
                {
                    accessory.Log.Debug($"P3 Sinister Seeds Blossom Guide Draw : targetId => {tarId} already exists");
                    accessory.Log.Debug($"P3 Sinister Seeds Blossom Guide Draw : SinisterSeedTargets count => {SinisterSeedTargets.Count}, targetId => {tarId}");
                    // count = SinisterSeedTargets.Count; // Record ice flower targets
                }
            }
            accessory.Log.Debug($"P3 Sinister Seeds Blossom Guide Draw : count => {count}, targetId => {tarId}");
            if (count != 4) return;
            bool isMeGetThornyDeathmatch = accessory.Data.MyObject is not null
                && (accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchI)
                || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchII)
                || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchIII)
                || accessory.Data.MyObject.HasStatus((uint)DataM7S.SID.ThornyDeathmatchIV));
            bool isMeGetSinisterSeed = SinisterSeedTargets.Contains((ulong)accessory.Data.Me);
            Vector3 endPos = Vector3.Zero;
            if (isMeGetSinisterSeed)
            {
                // I was marked with a purple circle
                if (isMeGetThornyDeathmatch)
                {
                    // I have a tether, go to the nearest stack point
                    List<Vector3> posList = new()
                    {
                        new Vector3(0, 0, -25) + DataM7S.P3_FieldCenter,
                        new Vector3(0, 0, 25) + DataM7S.P3_FieldCenter,
                        new Vector3(-25, 0, 0) + DataM7S.P3_FieldCenter,
                        new Vector3(25, 0, 0) + DataM7S.P3_FieldCenter
                    };
                    posList = posList.OrderBy(pos => Util.DistanceByTwoPoints(pos, accessory.Data.MyObject.Position)).ToList();
                    Vector3 _modPos = posList[0] - DataM7S.P3_FieldCenter;
                    endPos = _modPos * 0.76f + DataM7S.P3_FieldCenter;
                }
                else
                {
                    endPos = (accessory.GetMyRole(), WalkthroughType) switch
                    {
                        (EX.PlayerRoleEnum.MT, WalkthroughEnum.MMW_SPJP) => DataM7S.P3_FieldCenter + new Vector3(-10, 0, -10),
                        (EX.PlayerRoleEnum.ST, WalkthroughEnum.MMW_SPJP) => DataM7S.P3_FieldCenter + new Vector3(10, 0, -10),
                        (EX.PlayerRoleEnum.D1, WalkthroughEnum.MMW_SPJP) => DataM7S.P3_FieldCenter + new Vector3(-10, 0, 10),
                        (EX.PlayerRoleEnum.D2, WalkthroughEnum.MMW_SPJP) => DataM7S.P3_FieldCenter + new Vector3(10, 0, 10),
                        _ => Vector3.Zero
                    };
                }
                accessory.MultiDisDraw(new List<EX.DisplacementContainer> { new(endPos, 0, 6900) }, MultiDisProp);
            }
            else
            {
                if (isMeGetThornyDeathmatch) return;
                // Not marked, and I don't have a tether
                switch (WalkthroughType)
                {
                    case WalkthroughEnum.MMW_SPJP:
                        accessory.MultiDisDraw(new List<EX.DisplacementContainer> {
                            // new(new Vector3(10, 0, -4) + DataM7S.P3_FieldCenter, 0, 2000),
                            new(new Vector3(16, 0, -10) + DataM7S.P3_FieldCenter, 0, 2000),
                            new(new Vector3(10, 0, -16) + DataM7S.P3_FieldCenter, 0, 2000),
                            new(new Vector3(4, 0, -10) + DataM7S.P3_FieldCenter, 0, 2000)
                            }, MultiDisProp);
                        break;
                }

            }
        }

        [ScriptMethod(name: "P3 Sinister Seeds Blossom (Yellow) Pre Guide Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.StrangeSeedsVisualActionId_P3])]
        public void P3_SinisterSeedsBlossomGuideDraw2_Pre(Event @event, ScriptAccessory accessory)
        {
            // When the boss casts Pollen, draw pre-positioning, transparency adjusted to 50%
            if (@event.SourcePosition.Y > -100) return; // Only draw in P3 phase


            Vector3 endPos = DataM7S.P3_FieldCenter;
            EX.PlayerRoleEnum myRole = accessory.GetMyRole();
            bool isFieldBasis = true;

            endPos = (myRole, WalkthroughType, isFieldBasis) switch
            {
                (EX.PlayerRoleEnum.MT, WalkthroughEnum.MMW_SPJP, true) => new Vector3(-10, 0, -10) * 0.7f + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.ST, WalkthroughEnum.MMW_SPJP, true) => new Vector3(10, 0, -10) * 0.7f + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.H1, WalkthroughEnum.MMW_SPJP, true) => new Vector3(-10, 0, 10) * 0.7f + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.H2, WalkthroughEnum.MMW_SPJP, true) => new Vector3(10, 0, 10) * 0.7f + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.D1, WalkthroughEnum.MMW_SPJP, true) => new Vector3(-10, 0, 10) * 0.7f + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.D2, WalkthroughEnum.MMW_SPJP, true) => new Vector3(10, 0, 10) * 0.7f + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.D3, WalkthroughEnum.MMW_SPJP, true) => new Vector3(-10, 0, -10) * 0.7f + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.D4, WalkthroughEnum.MMW_SPJP, true) => new Vector3(10, 0, -10) * 0.7f + DataM7S.P3_FieldCenter,
                _ => endPos
            };
            EX.MultiDisDrawProp tempProp = new()
            {
                BaseDelay = MultiDisProp.BaseDelay,
                Width = MultiDisProp.Width,
                EndCircleRadius = MultiDisProp.EndCircleRadius,
                Color_GoNow = MultiDisProp.Color_GoNow.WithW(0.5f), // Transparency adjusted to 50%
                Color_GoLater = MultiDisProp.Color_GoLater.WithW(0.5f), // Transparency adjusted to 50%
                DrawMode = MultiDisProp.DrawMode,
            };
            accessory.MultiDisDraw(new List<EX.DisplacementContainer> { new(endPos, 0, 3500) }, tempProp);
        }

        [ScriptMethod(name: "P3 Sinister Seeds Blossom (Yellow) Guide Draw",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: [DataM7S.SinisterSeedsBlossomActionId42392])]
        public void P3_SinisterSeedsBlossomGuideDraw2(Event @event, ScriptAccessory accessory)
        {
            if (@event.SourcePosition.Y > -100)
            {
                if (P3MMWZhuiCheDebug) accessory.Method.SendChat("/e Detected yellow circle ice flower, but not P3 phase");
                return; // Only draw in P3 phase
            }
            if (@event.TargetId != (ulong)accessory.Data.Me)
            {
                // Not my own ice flower, no additional drawing
                if (P3MMWZhuiCheDebug) accessory.Method.SendChat("/e Detected yellow circle ice flower, but this yellow circle is not your target");
                return;
            }
            EX.PlayerRoleEnum myRole = accessory.GetMyRole();
            if (P3MMWZhuiCheDebug) accessory.Method.SendChat($"/e Your role is {myRole}");
            if (P3MMWZhuiCheDebug) accessory.Method.SendChat($"/e Using strategy {WalkthroughType}");
            Vector3 endPos = Vector3.Zero;
            Vector3 bossPos = @event.SourcePosition;
            // Vector3 bossToC = DataM7S.P3_FieldCenter - bossPos;
            // But this is the position before the charge, let the boss charge
            Vector3 P3MMWZhuiChe_MTD1 = new Vector3(10, 0, -10);
            Vector3 P3MMWZhuiChe_STD2 = new Vector3(10, 0, 10);
            Vector3 P3MMWZhuiChe_H1D3 = new Vector3(-10, 0, -10);
            Vector3 P3MMWZhuiChe_H2D4 = new Vector3(-10, 0, 10);

            Vector3 bossPosToC = DataM7S.P3_FieldCenter - bossPos;
            float bossPosRot = MathF.Atan2(bossPosToC.Z, bossPosToC.X);

            bool isSecondRound = false;
            isSecondRound = Util.DistanceByTwoPoints(bossPos, DataM7S.P3_FieldCenter) > 10; //accessory.Data.Objects.GetByDataId((uint)DataM7S.OID.BruteAbombinator).Any(obj => obj is IBattleChara bc && bc.IsCasting);

            bool isFieldBasis = !isSecondRound || (isSecondRound && !P3MMWZhuiChe);
            if (P3MMWZhuiCheDebug) accessory.Method.SendChat($"/e Is {(isSecondRound ? 2 : 1)}nd round ice flower, Chariot?({(isFieldBasis ? "No" : "Yes")})");

            endPos = (myRole, WalkthroughType, isFieldBasis) switch
            {
                (EX.PlayerRoleEnum.MT, WalkthroughEnum.MMW_SPJP, true) => new Vector3(-10, 0, -10) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.MT, WalkthroughEnum.MMW_SPJP, false) => Util.RotatePointInFFXIV(P3MMWZhuiChe_MTD1, Vector3.Zero, bossPosRot) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.ST, WalkthroughEnum.MMW_SPJP, true) => new Vector3(10, 0, -10) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.ST, WalkthroughEnum.MMW_SPJP, false) => Util.RotatePointInFFXIV(P3MMWZhuiChe_STD2, Vector3.Zero, bossPosRot) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.H1, WalkthroughEnum.MMW_SPJP, true) => new Vector3(-10, 0, 10) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.H1, WalkthroughEnum.MMW_SPJP, false) => Util.RotatePointInFFXIV(P3MMWZhuiChe_H1D3, Vector3.Zero, bossPosRot) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.H2, WalkthroughEnum.MMW_SPJP, true) => new Vector3(10, 0, 10) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.H2, WalkthroughEnum.MMW_SPJP, false) => Util.RotatePointInFFXIV(P3MMWZhuiChe_H2D4, Vector3.Zero, bossPosRot) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.D1, WalkthroughEnum.MMW_SPJP, true) => new Vector3(-10, 0, 10) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.D1, WalkthroughEnum.MMW_SPJP, false) => Util.RotatePointInFFXIV(P3MMWZhuiChe_MTD1, Vector3.Zero, bossPosRot) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.D2, WalkthroughEnum.MMW_SPJP, true) => new Vector3(10, 0, 10) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.D2, WalkthroughEnum.MMW_SPJP, false) => Util.RotatePointInFFXIV(P3MMWZhuiChe_STD2, Vector3.Zero, bossPosRot) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.D3, WalkthroughEnum.MMW_SPJP, true) => new Vector3(-10, 0, -10) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.D3, WalkthroughEnum.MMW_SPJP, false) => Util.RotatePointInFFXIV(P3MMWZhuiChe_H1D3, Vector3.Zero, bossPosRot) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.D4, WalkthroughEnum.MMW_SPJP, true) => new Vector3(10, 0, -10) + DataM7S.P3_FieldCenter,
                (EX.PlayerRoleEnum.D4, WalkthroughEnum.MMW_SPJP, false) => Util.RotatePointInFFXIV(P3MMWZhuiChe_H2D4, Vector3.Zero, bossPosRot) + DataM7S.P3_FieldCenter,
                _ => endPos + DataM7S.P3_FieldCenter
            };
            if (P3MMWZhuiCheDebug) accessory.Method.SendChat($"/e Your standby point is {endPos}");
            accessory.MultiDisDraw(new List<EX.DisplacementContainer> { new(endPos, 0, 5200) }, MultiDisProp);
        }


        /*
                P2P3, draw a dividing line for players connected by a tether, 1 layer, 2 layers
            */



        /*
            P3 spore explosion draw danger zone
            â†‘ Same as P1, no additional drawing
        */

        /*
            P3 stack ice flower draw danger zone
            P3 stack ice flower draw danger zone after landing
            â†‘ Same as P2, no additional drawing
        */
        /*
            Petrification draw safe zone
            â†‘ Same as P1, no additional drawing
        */
        /*
            Burrowing cannon + ice flower draw danger zone
            â†‘ Same as P1, no additional drawing
        */

        // Remaining work 
        // P3 mouth cannon guidance
        // P3 Neckbreaker charge whether to pre-position
        // Record dual weapon type, record weapon jump type before Neckbreaker charge
        // P3 yellow circle ice flower second round pre-guidance


    }


    #region Extension Methods
    public static class ScriptExtensions_Tsing
    {

        // Quick drawing
        public static void FastDraw(this ScriptAccessory accessory, DrawTypeEnum drawType, Vector3 position, Vector2 scale, (long Delay, long DestoryAt) delay_destoryAt, Vector4 color, DrawModeEnum drawMode = DrawModeEnum.Default)
        {
            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = Guid.NewGuid().ToString();
            dp.Delay = delay_destoryAt.Delay;
            dp.DestoryAt = delay_destoryAt.DestoryAt;
            dp.Color = color;
            dp.Scale = scale;
            switch (drawType)
            {
                case DrawTypeEnum.Donut:
                    dp.Scale = new(scale.X);
                    dp.InnerScale = new(scale.Y);
                    dp.Radian = 2 * MathF.PI;
                    break;
            }

            dp.Position = position;
            accessory.Method.SendDraw(drawMode, drawType, dp);
        }
        public static void FastDraw(this ScriptAccessory accessory, DrawTypeEnum drawType, ulong ownerId, Vector2 scale, (long Delay, long DestoryAt) delay_destoryAt, Vector4 color, DrawModeEnum drawMode = DrawModeEnum.Default)
        {
            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = Guid.NewGuid().ToString();
            dp.Delay = delay_destoryAt.Delay;
            dp.DestoryAt = delay_destoryAt.DestoryAt;
            dp.Color = color;
            dp.Scale = scale;
            switch (drawType)
            {
                case DrawTypeEnum.Donut:
                    dp.Scale = new(scale.X);
                    dp.InnerScale = new(scale.Y);
                    dp.Radian = 2 * MathF.PI;
                    break;
            }

            dp.Owner = ownerId;
            accessory.Method.SendDraw(drawMode, drawType, dp);
        }
        public static void FastDraw(this ScriptAccessory accessory, DrawTypeEnum drawType, Vector3 position, Vector2 scale, (long Delay, long DestoryAt) delay_destoryAt, bool isSafe, DrawModeEnum drawMode = DrawModeEnum.Default)
        {
            Vector4 color = isSafe ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            accessory.FastDraw(drawType, position, scale, delay_destoryAt, color, drawMode);
        }
        public static void FastDraw(this ScriptAccessory accessory, DrawTypeEnum drawType, ulong ownerId, Vector2 scale, (long Delay, long DestoryAt) delay_destoryAt, bool isSafe, DrawModeEnum drawMode = DrawModeEnum.Default)
        {
            Vector4 color = isSafe ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            accessory.FastDraw(drawType, ownerId, scale, delay_destoryAt, color, drawMode);
        }


        public class DisplacementContainer
        {
            public Vector3 Pos;
            public long Delay;
            public long DestoryAt;

            private DisplacementContainer() { }
            public DisplacementContainer(Vector3 pos, long delay, long destoryAt)
            {
                this.Pos = pos;
                this.Delay = delay;
                this.DestoryAt = destoryAt;
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
        internal static void MultiDisDraw(this ScriptAccessory accessory, List<DisplacementContainer> list, MultiDisDrawProp prop)
        {
            // accessory.Log.Debug("RawMultiDisDraw");
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
                dp_goNowLine.Delay = startTimeMillis + dis.Delay - Math.Sign(i) * preMs;
                dp_goNowLine.DestoryAt = dis.DestoryAt - preMs / 3 - (prop.DrawMode == DrawModeEnum.Imgui ? 200 : 0);
                dp_goNowLine.ScaleMode |= ScaleMode.YByDistance;
                dp_goNowLine.TargetPosition = dis.Pos;
                dp_goNowLine.Color = prop.Color_GoNow;
                accessory.Method.SendDraw(prop.DrawMode, DrawTypeEnum.Displacement, dp_goNowLine);
                // accessory.Log.Debug($"dp_goNowLine.Delay = {dp_goNowLine.Delay}");
                if (prop.EndCircleRadius > 0)
                {
                    DrawPropertiesEdit dp_goNowCircle = accessory.Data.GetDefaultDrawProperties();
                    dp_goNowCircle.Name = name + count++;
                    // dp_goNowCircle.Owner = (ulong)accessory.Data.Me;
                    dp_goNowCircle.Position = dis.Pos;
                    dp_goNowCircle.Scale = new(prop.EndCircleRadius);
                    dp_goNowCircle.Delay = dp_goNowLine.Delay;
                    dp_goNowCircle.DestoryAt = dp_goNowLine.DestoryAt;
                    dp_goNowCircle.Color = prop.Color_GoNow;
                    accessory.Method.SendDraw(prop.DrawMode, DrawTypeEnum.Circle, dp_goNowCircle);
                }

                //If the current point is not the first point, perform the go later part
                if (i >= 1)
                {
                    // DisplacementContainer disBefore = _list[i - 1];
                    DrawPropertiesEdit dp_goLaterLine = accessory.Data.GetDefaultDrawProperties();
                    dp_goLaterLine.Name = name + count++;
                    dp_goLaterLine.Position = list[i - 1].Pos;
                    dp_goLaterLine.TargetPosition = dis.Pos;
                    dp_goLaterLine.Scale = new(prop.Width);
                    dp_goLaterLine.ScaleMode |= ScaleMode.YByDistance;
                    dp_goLaterLine.Delay = prop.BaseDelay + list[0].Delay;
                    dp_goLaterLine.DestoryAt = startTimeMillis - (prop.BaseDelay + list[0].Delay) - 100 - (prop.DrawMode == DrawModeEnum.Imgui ? 200 : 0);
                    dp_goLaterLine.Color = prop.Color_GoLater;
                    accessory.Method.SendDraw(prop.DrawMode, DrawTypeEnum.Displacement, dp_goLaterLine);

                    if (prop.EndCircleRadius > 0)
                    {

                        DrawPropertiesEdit dp_goLaterCircle = accessory.Data.GetDefaultDrawProperties();
                        dp_goLaterCircle.Name = name + count++;
                        dp_goLaterCircle.Position = dis.Pos;
                        dp_goLaterCircle.Scale = new(prop.EndCircleRadius);
                        dp_goLaterCircle.Delay = dp_goLaterLine.Delay;
                        dp_goLaterCircle.DestoryAt = dp_goLaterLine.DestoryAt - 100;
                        dp_goLaterCircle.Color = prop.Color_GoLater;
                        accessory.Method.SendDraw(prop.DrawMode, DrawTypeEnum.Circle, dp_goLaterCircle);
                    }
                }
                startTimeMillis = startTimeMillis + dis.Delay + dis.DestoryAt;

            }

        }

        public enum PlayerRoleEnum
        {
            MT = 0, ST = 1,
            H1 = 2, H2 = 3,
            D1 = 4, D2 = 5, D3 = 6, D4 = 7,
            Unknown = -1
        }

        public static PlayerRoleEnum GetMyRole(this ScriptAccessory accessory)
        {
            uint myId = accessory.Data.Me;
            if (accessory.Data.PartyList is null) return PlayerRoleEnum.MT;
            List<uint> partyList = new List<uint>(accessory.Data.PartyList);
            int myIndex = partyList.IndexOf(myId);
            accessory.Log.Debug($"GetMyRole: myId = {myId}, myIndex = {myIndex}, partyList.Count = {partyList.Count}");
            if (myIndex < 0 || myIndex >= partyList.Count)
            {
                accessory.Method.SendChat($"/e Role acquisition abnormal, your index value is {myIndex} (should be between 0 and 7)");
            }
            if (Enum.IsDefined(typeof(PlayerRoleEnum), myIndex))
            {
                return (PlayerRoleEnum)myIndex;
            }
            else
            {
                return PlayerRoleEnum.Unknown;
            }


        }
        public static Job MyJob(this ScriptAccessory accessory)
        {
            try
            {
                IPlayerCharacter? myChara = accessory.Data.MyObject;
                if (myChara is not null && Enum.IsDefined(typeof(Job), (byte)myChara.ClassJob.RowId))
                {
                    return (Job)myChara.ClassJob.RowId;
                }
                else
                {
                    return Job.ADV;
                }
            }
            catch (Exception ex)
            {
                accessory.Log.Error($"Failed to get job: {ex.Message}");
                return Job.ADV;
            }

        }
        
        public enum Job : byte
        {
            ADV = 0, GLA = 1, PGL = 2, MRD = 3, LNC = 4, ARC = 5, CNJ = 6, THM = 7, CRP = 8, BSM = 9,
            ARM = 10, GSM = 11, LTW = 12, WVR = 13, ALC = 14, CUL = 15, MIN = 16, BTN = 17, FSH = 18,
            PLD = 19, MNK = 20, WAR = 21, DRG = 22, BRD = 23, WHM = 24, BLM = 25,
            ACN = 26,
            SMN = 27, SCH = 28,
            ROG = 29,
            NIN = 30, MCH = 31, DRK = 32, AST = 33, SAM = 34, RDM = 35, BLU = 36, GNB = 37, DNC = 38,
            RPR = 39, SGE = 40, VPR = 41, PCT = 42,
        }
    
    }
    public static class Utilities_Tsing
    {
        public static float DistanceByTwoPoints(Vector3 point1, Vector3 point2)
        {
            float x = point1.X - point2.X;
            float y = point1.Y - point2.Y;
            float z = point1.Z - point2.Z;
            return MathF.Sqrt(MathF.Pow(x, 2) + MathF.Pow(y, 2) + MathF.Pow(z, 2));
        }
        public static Vector3 RotatePointInFFXIV(Vector3 point, Vector3 centre, float radian)
        {
            Vector3 cToP = point - centre;
            Vector2 cToP_v2 = new(cToP.X, cToP.Z);
            float rot = (MathF.Atan2(cToP_v2.Y, cToP_v2.X) + radian);
            float length = cToP_v2.Length();
            return new(centre.X + MathF.Cos(rot) * length, centre.Y, centre.Z + MathF.Sin(rot) * length);
        }
    }
    #endregion

    #region Data Storage
    public static class DataM7S
    {
        public const string BossDataId = $"DataId:regex:^(18307)$";
        public const string BrutalImpactActionId = $"ActionId:regex:^(42331)$";
        public const string SmashHereThereActionId = $"ActionId:regex:^(42335|42336)$";
        public const string BrutishSwingActionId_P1 = $"ActionId:regex:^(42337|42338)$";
        public const string P1_StoneringerActionId = $"ActionId:regex:^(42333|42334)$";
        public const string P3_StoneringerActionId = $"ActionId:regex:^(42401|42400)$";

        public const string PollenActionId = $"ActionId:regex:^(42347)$";
        public const string RootsOfEvilActionId = $"ActionId:regex:^(42354)$";
        public const string SinisterSeedsVisualActionId = $"ActionId:regex:^(42349)$";
        public const string SinisterSeedsBlossomActionId = $"ActionId:regex:^(42350|42392|42395)$";
        public const string SinisterSeedsChaseActionId = $"ActionId:regex:^(42353)$";
        public const string SinisterSeedsBlossomActionId42350 = $"ActionId:regex:^(42350)$";
        public const string SinisterSeedsBlossomActionId42392 = $"ActionId:regex:^(42392)$";
        public const string KillerSeedsActionId = $"ActionId:regex:^(42395)$";
        public const string TendrilsOfTerrorActionId = $"ActionId:regex:^(42351|42393|42396)$";
        public const string QuarrySwampActionId = $"ActionId:regex:^(42357)$";
        public const string BloomingAbominationDataId = $"DataId:regex:^(18308)$";
        
        public const string MobsWindsActionId = $"ActionId:regex:^(43277|43278)$";
        public const string ExplosionActionId = $"ActionId:regex:^(42358)$";
        public const string PulpSmashActionId = $"ActionId:regex:^(42359)$";
        public const string ItCameFromTheDirtActionId = $"ActionId:regex:^(42362)$";
        public const string NeoBombarianSpecialActionId = $"ActionId:regex:^(42364)$";
        public const string BrutishSwingActionId_P2P3 = $"ActionId:regex:^(42386|42387|42403|42405)$";
        public const string GlowerPowerActionId = $"ActionId:regex:^(43340)$";
        public const string DemolitionDeathmatchActionId = $"ActionId:regex:^(42390)$";
        public const string DebrisDeathmatchActionId = $"ActionId:regex:^(42416)$";
        public const string AbominableBlinkActionId = $"ActionId:regex:^(42377)$";
        public const string AbominableBlinkIconId = "Id:regex:^(0147)$";
        public const string StrangeSeedsActionId = $"ActionId:regex:^(42392)$";
        public const string LashingLariatActionId = $"ActionId:regex:^(42408|42410)$";
        public const string StrangeSeedsVisualActionId_P3 = $"ActionId:regex:^(43274)$";
        public const string StrangeSeedsVisualActionId_P2 = $"ActionId:regex:^(42391)$";
        // public const string BrutishSwingActionId_P3 = $"ActionId:regex:^(42403|42405)$";


        // P1 Arena related data
        public static readonly Vector3 P1_FieldCenter = new Vector3(100, 0, 100);
        public static readonly Vector3 P1_NailOffset_In = new Vector3(9, 0, 9); // P1 Nail offset
        public static readonly Vector3 P1_NailOffset_Out = new Vector3(18, 0, 18); // P1 Nail offset
        public static readonly Vector2 P1_FieldSideLength = new Vector2(40, 40); // P1 Arena side length

        // P2 Arena related data
        public static readonly Vector3 P2_FieldCenter = new Vector3(100, 0, 5);
        public static readonly Vector3 P2_NailOffset_In = new Vector3(9, 0, 14); // P2 Nail offset
        public static readonly Vector3 P2_NailOffset_Out = new Vector3(11, 0, 23.5f); // P2 Nail offset
        public static readonly Vector2 P2_FieldSideLength = new Vector2(25, 50); // P2 Arena side length

        // P3 Arena related data
        public static readonly Vector3 P3_FieldCenter = new Vector3(100, -200, 5);
        public static readonly Vector2 P3_SquareSideLength = new Vector2(5, 5); // P3 Arena small square side length
        public static readonly Vector2 P3_FieldSideLength = new Vector2(40, 40); // P3 Arena side length

        public enum AID : uint
        {
            BrutalImpact = 42331, // Boss->self, 5.0s cast, single-target full arena AOE
            StoneringerStick = 42333, // Pull out large club
            StoneringerMachete = 42334, // Pull out large sword
            SmashHere = 42335, // Near tankbuster, cast 2.7s circle 7y
            SmashThere = 42336, // Far tankbuster, cast 2.7s circle 7y
            BrutishSwingStick_P1 = 42337, // P1 club Circle AOE
            BrutishSwingMachete_P1 = 42338, // P2 sword Donut AOE
            Pollen = 42347, // P1 mid-boss places spores on ground, cast 3.7s, circle 8y
            RootsOfEvil = 42354, // P1 burrowing cannon after growing larger AOE, cast 2.7s, circle 12y
            SinisterSeedsBlossom = 42350, // Ice flower mark, cast 6.7s, 4 straight lines, 4y
            SinisterSeedsChase = 42353, // Burrowing cannon mark, cast 2.7s, 8y
            TendrilsOfTerror_P1 = 42351, // AOE spreading after ice flower lands, 4 straight lines, 4y

            WindingWildwinds = 43277, // Mob cast, donut cast 6.7s
            CrossingCrosswinds = 43278, // Mob cast, cross cast 6.7s
            QuarrySwamp = 42357, // Boss petrify cast, cast 3.7s
            Explosion = 42358, // Triple attenuation AOE, cast 8.7s
            PulpSmash = 42359, // Jump stack, cast 2.7s
            ItCameFromTheDirt = 42362, // Small circle underfoot after jump stack + eight-direction cones cast 1.7s
            NeoBombarianSpecial = 42364, // P1 end transition AOE
            BrutishSwingJump_P2 = 42381, // P2 jump to another wall cast 3.7s
            BrutishSwingStick_P2 = 42386, // P2 club Circle AOE
            BrutishSwingMachete_P2 = 42387, // P2 Donut AOE
            GlowerPower_Straight_P2 = 42373, // Boss mouth cannon, cast 2.4s

            AbominableBlink = 42377, // GA-100, cast 5.0s
            DemolitionDeathmatch = 42390, // Boss->self, 3.0s cast, single-target connects three people
            GlowerPower_Electrogenetic_P2 = 43340, // Clone cast spread AOE cast 3.7s
            StrangeSeeds = 42392, // P2 ice flower mark, cast 4.7s
            TendrilsOfTerror_P2_StrangeSeeds = 42393, // Main cast after P2 ice flower mark lands, paired with two 42394 to form star shape cast 2.7s
            KillerSeeds = 42395, // P2P3 stack ice flower, cast 4.7s,
            TendrilsOfTerror_P2_KillerSeeds = 42396, // Paired with two 42397 to form star shape cast 2.7s
            SinisterSeedsVisual = 42349, // Boss->self, 4.0+1.0s cast, single-target

            BrutishSwingJump_P3 = 42402, // P3 jump to another wall cast 2.7s
            DebrisDeathmatch = 42416, // Boss->self, 3.0s cast, single-target connects four people

            /* 
                It's quite possible that the large club and later large club use two different IDs, P3's mouth cannon + spread cast is faster
                â†‘ Not so, it's the same
            */
            Stoneringer2Stoneringers_LStick = 42401, // Boss->self, 2.0+3.5s cast, single-target
            Stoneringer2Stoneringers_RStick = 42400, // Boss->self, 2.0+3.5s cast, single-target
            BrutishSwingStick_P3 = 42403, // P3 club Circle AOE cast 6.4s subsequent club
            BrutishSwingMachete_P3 = 42405, // P3 sword Donut AOE cast 6.4s preceding sword
            LashingLariat_Unknown = 42407, // Charge cast 3.2s
            LashingLariatWithRightHand = 42408, // Charge, using right hand, face boss to the right, cast 3.7s
            LashingLariatWithLeftHand = 42410, // Charge, using left hand, face boss to the left, cast 3.7s
            GlowerPower_Straight_P3 = 43338, // Boss mouth cannon, cast 0.4s
            GlowerPower_Electrogenetic_P3 = 43358, // Clone cast spread AOE cast 1.7s
            P2StrangeSeedsVisual = 42391, // Boss->self, 4.0s cast, single-target yellow circle ice flower mark start cast P2
            P3StrangeSeedsVisual = 43274, // Boss->self, 4.0s cast, single-target yellow circle ice flower mark start cast P3
        }
        public enum OID : uint
        {
            BruteAbombinator = 18307, // BOSS
            BloomingAbomination = 18308, // Mob
        }
        public enum SID : uint
        {
            ThornyDeathmatchI = 4466, // Thorny Deathmatch I, the type that cannot be transferred
            ThornyDeathmatchII = 4467, // Thorny Deathmatch II
            ThornyDeathmatchIII = 4468, // Thorny Deathmatch III
            ThornyDeathmatchIV = 4469, // Thorny Deathmatch IV

            ThornsOfDeathI = 4499, // none->player, extra=0x0 type that can be transferred on tanks
            ThornsOfDeathII = 4500, // none->player, extra=0x0
            ThornsOfDeathIII = 4501, // none->player, extra=0x0
            ThornsOfDeathIV = 4502, // none->player, extra=0x0
        }
    }
    #endregion
}
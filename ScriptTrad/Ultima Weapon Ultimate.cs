using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using System.Numerics;
using System.Linq;
using System.Diagnostics;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Script;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Dalamud.Utility.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.STD.Helper;
using Lumina.Data.Parsing;

namespace LinoaScripts.WeaponsRefrainUltimate
{
    [ScriptType(name: "Ultima Weapon Ultimate",
        territorys: [777],
        guid: "a292407d-5632-41b5-85e3-6232d0229228",
        version: "0.0.0.2",
        note: scriptNotes,
        author: "Linoa235")]

    public class Weapons_Refrain_Ultimate
    {
        public const string scriptNotes =
            """
            Ultima Weapon Ultimate script.
            Since the previous Ultima Weapon Ultimate script (author @baelixac) has been unmaintained for a long time and causes compilation errors on the latest version of Kodakku, I decided to completely rewrite this encounter's script from scratch.

            Construction has just begun. Progress will be sporadic.

            Adapted strategy is Chinese PF standard.
            If the guidance does not fit your strategy, you can turn off the relevant guidance in the method settings. All guidance methods are marked with "(Guidance)" suffix.

            If you encounter any issues while using, please first check that both Kodakku and the script are updated to the latest version, that party roles are correctly set, and whether the issue can be consistently reproduced.
            If none of the above are issues, please contact @_publius_cornelius_scipio_ on the Kodakku Discord with an A Realm Recorded plugin recording of the issue.
            """;

        #region User Settings

        [UserSetting("General Enable Text Prompts")]
        public bool enablePrompts { get; set; } = false;

        [UserSetting("General Enable Vanilla TTS")]
        public bool enableVanillaTts { get; set; } = false;

        [UserSetting("General Enable Daily Routines TTS (Requires Daily Routines plugin installed and enabled!)")]
        public bool enableDailyRoutinesTts { get; set; } = false;

        [UserSetting("General Color of Direction Indicators")]
        public ScriptColor colourOfDirectionIndicators { get; set; } = new() { V4 = new Vector4(1, 1, 0, 1) }; // Yellow by default.

        [UserSetting("General Color of Extremely Dangerous Attacks")]
        public ScriptColor colourOfExtremelyDangerousAttacks { get; set; } = new() { V4 = new Vector4(1, 0, 0, 1) }; // Red by default.

        [UserSetting("General Enable Shenanigans")]
        public bool enableShenanigans { get; set; } = false;

        [UserSetting("Debug Enable Debug Logging and Output to Dalamud Log")]
        public bool enableDebugLogging { get; set; } = false;

        [UserSetting("Debug Ignore Phase Checks in All Methods")]
        public bool skipPhaseChecks { get; set; } = false;

        [UserSetting("Debug Preserve Drawings While Switching Phase")]
        public bool preserveDrawingsWhileSwitchingPhase { get; set; } = false;

        #endregion

        #region Variables and Semaphores

        private volatile int majorPhase = 1;
        private volatile int phase = 1;

        /*

        Major Phase 1 - Garuda:

            Phases are separated by Feather Rain.

        Major Phase 2 - Ifrit:

            Phase 1 - Placeholder
            Phase 2 - Placeholder
            Phase 3 - Placeholder

        Major Phase 3 - Titan:

            Phase 1 - Placeholder
            Phase 2 - Placeholder
            Phase 3 - Placeholder

        Major Phase 4 - Ascian Lahabrea:

            Phase 1 - Placeholder
            Phase 2 - Placeholder
            Phase 3 - Placeholder

        Major Phase 5 - Ultima Weapon:

            Phase 1 - Placeholder
            Phase 2 - Placeholder
            Phase 3 - Placeholder

        */

        // ----- Major Phase 1 -----

        private volatile int phase1_slipstreamCounter = 0;
        private ulong phase1_targetOfMistralSong = 0;
        private System.Threading.AutoResetEvent phase1_mistralSongSemaphore = new System.Threading.AutoResetEvent(false);
        private System.Threading.AutoResetEvent phase1_downburstSemaphore = new System.Threading.AutoResetEvent(false);

        // ----- End Of Major Phase 1 -----

        // ----- Major Phase 2 -----

        // ----- End Of Major Phase 2 -----

        // ----- Major Phase 3 -----

        // ----- End Of Major Phase 3 -----

        // ----- Major Phase 4 -----

        // ----- End Of Major Phase 4 -----

        // ----- Major Phase 5 -----

        // ----- End Of Major Phase 5 -----

        #endregion

        #region Constants and Locks

        private const int MAXIMUM_DURATION = 7200000;
        private const int COMMON_INTERVAL = 2500;

        private static readonly Vector3 ARENA_CENTER = new Vector3(100, 0, 100);
        // The arena is a circle with a radius of 20.

        #endregion

        #region Enumerations and Classes

        #endregion

        #region Initialization

        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(".*");

            VariableAndSemaphoreInitialization();

            if (enableShenanigans)
            {
                shenaniganSemaphore.Set();
            }
        }

        private void VariableAndSemaphoreInitialization()
        {
            majorPhase = 1;
            phase = 1;

            // ----- Major Phase 1 -----

            phase1_slipstreamCounter = 0;
            phase1_targetOfMistralSong = 0;
            phase1_mistralSongSemaphore.Reset();
            phase1_downburstSemaphore.Reset();

            // ----- End Of Major Phase 1 -----

            // ----- Major Phase 2 -----

            // ----- End Of Major Phase 2 -----

            // ----- Major Phase 3 -----

            // ----- End Of Major Phase 3 -----

            // ----- Major Phase 4 -----

            // ----- End Of Major Phase 4 -----

            // ----- Major Phase 5 -----

            // ----- End Of Major Phase 5 -----
        }

        #endregion

        #region Shenanigans

        private System.Threading.AutoResetEvent shenaniganSemaphore = new System.Threading.AutoResetEvent(false);
        private static ImmutableList<string> quotes =
        [
            "Greetings to the banks of the Jordan, and the fallen towers of Zion...",
            "Above the tombs, the wind howls.",
            "The enslaved were not bricks in your road, and their lives were not chapters in your redemptive history.",
            "Lord, you made us for yourself, and our hearts are restless until they find rest in you.",
            "No! I am alive! I will live forever! There is something in my heart that will never die!",
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
            "The end may justify the means as long as there is something that justifies the end.",
            "One by one the righteous fell, and the ills of ignorance permeated.",
            "They defended the grains of sand in the desert to the last drop of their blood.",
            "All history is man's efforts to realise ideals.\n- Ã‰amon de Valera, 1929",
            "Let us dedicate ourselves to what the Greeks wrote so many years ago: to tame the savageness of man and make gentle the life of this world.\n- Robert F. Kennedy, 1968",
            "Yesterday is not ours to recover, but tomorrow is ours to win or lose.\n- Lyndon B. Johnson, 1964",
            "The end of hope is the beginning of death.\n- Charles de Gaulle, 1945",
            "The day I leave the power, inside my pockets will only be dust.\n- Antonio de Oliveira Salazar, 1968",
            "When smashing monuments, save the pedestals. They always come in handy.\n- StanisÅ‚aw Jerzy Lec, 1957",
            "Fear not the path of truth for the lack of people walking on it.\n- Robert F. Kennedy, 1968",
            "The rocket worked perfectly, except for landing on the wrong planet.\n- Wernher von Braun upon the first V-2 hitting London, 1944",
            "Do not pray for easy lives, pray to be stronger men.\n- John F. Kennedy, 1963",
            "The optimist thinks this is the best of all possible worlds. The pessimist fears it is true.\n- James Branch Cabell, The Silver Stallion, 1926",
            "One seldom recognizes the devil when he is putting his hand on your shoulder.\n- Albert Speer, 1972",
            "They don't ask much of you. They only want you to hate the things you love and to love the things you despise.\n- Boris Pasternak, 1960",
            "Most economic fallacies derive from the tendency to assume that there is a fixed pie, that one party can gain only at the expense of another.\n- Milton Friedman, 1980",
            "There are three kinds of lies: lies, damned lies, and statistics.\n- Mark Twain, 1907",
            "Bite us once, shame on the dog; bite us repeatedly, shame on us for allowing it.\n- Phyllis Schlafly, 1995",
            "You can believe in Feng Shui if you want, but ultimately people control their own fate.\n- Li Ka-shing, 1969",
            "A good reputation for yourself and your company is an invaluable asset not reflected in the balance sheets.\n- Li Ka-shing, 1967",
            "Knowledge is your real companion, your life long companion, not fortune. Fortune can disappear.\n- Stanley Ho, 1966",
            "People sometimes say: \"we are in a society that is all rotten, all dishonest.\" That is not true. There are still so many good people, so many honest people.\n- John Paul I, 1978",
            "Half the confusion in the world comes from not knowing how little we need.\n- Admiral Richard E. Byrd on his time in Antarctica, 1935"
        ];

        [ScriptMethod(name: "Shenanigans",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: ["DataId:8722"],
            suppress: 14000,
            userControl: false)]

        public void Shenanigans(Event @event, ScriptAccessory accessory)
        {
            if (!enableShenanigans)
            {
                return;
            }

            bool wasSignalled = shenaniganSemaphore.WaitOne(14000);

            if (!wasSignalled)
            {
                return;
            }

            System.Threading.Thread.Sleep(4000);

            string prompt = quotes[new System.Random().Next(0, quotes.Count)];

            if (!string.IsNullOrWhiteSpace(prompt))
            {
                if (enablePrompts)
                {
                    accessory.Method.TextInfo(prompt, 10000);
                }

                accessory.tts(prompt, enableVanillaTts, enableDailyRoutinesTts);
            }
        }

        #endregion

        #region Garuda

        [ScriptMethod(name: "Garuda Pull Boss East (Guidance, MT Only)",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: ["DataId:8722"],
            suppress: COMMON_INTERVAL)]

        public void Garuda_PullBossEast_Guidance_MTOnly(Event @event, ScriptAccessory accessory)
        {
            if (!convertObjectIdToDecimal(@event["SourceId"], out var sourceId))
            {
                return;
            }

            System.Threading.Tasks.Task.Delay(4000).ContinueWith(_ =>
            {
                if (majorPhase != 1 && !skipPhaseChecks)
                {
                    return;
                }

                if (phase != 1 && !skipPhaseChecks)
                {
                    return;
                }

                int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

                if (!isLegalPartyIndex(myIndex))
                {
                    return;
                }

                if (myIndex != 0)
                {
                    return;
                }

                var currentProperties = accessory.Data.GetDefaultDrawProperties();

                currentProperties.Name = "Garuda_PullBossEast_Guidance_MTOnly";
                currentProperties.Scale = new(2);
                currentProperties.Owner = sourceId;
                currentProperties.TargetPosition = new Vector3(116.25f, 0, 100);
                currentProperties.ScaleMode |= ScaleMode.YByDistance;
                currentProperties.Color = colourOfDirectionIndicators.V4.WithW(1);
                currentProperties.DestoryAt = MAXIMUM_DURATION;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperties);
            });
        }

        [ScriptMethod(name: "Garuda Pull Boss East (Guidance Removal)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:11091"],
            userControl: false)]

        public void Garuda_PullBossEast_GuidanceRemoval(Event @event, ScriptAccessory accessory)
        {
            if (majorPhase != 1 && !skipPhaseChecks)
            {
                return;
            }

            if (phase != 1 && !skipPhaseChecks)
            {
                return;
            }

            accessory.Method.RemoveDraw("Garuda_PullBossEast_Guidance_MTOnly");
        }

        [ScriptMethod(name: "Garuda Slipstream (Range)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:11091"])]

        public void Garuda_Slipstream_Range(Event @event, ScriptAccessory accessory)
        {
            if (majorPhase != 1 && !skipPhaseChecks)
            {
                return;
            }

            if (!convertObjectIdToDecimal(@event["SourceId"], out var sourceId))
            {
                return;
            }

            var currentProperties = accessory.Data.GetDefaultDrawProperties();

            currentProperties.Scale = new(12);
            currentProperties.Owner = sourceId;
            currentProperties.Radian = float.Pi / 2;
            currentProperties.Color = colourOfExtremelyDangerousAttacks.V4.WithW(1);
            currentProperties.DestoryAt = 2500;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperties);
        }

        [ScriptMethod(name: "Garuda Slipstream (Counter)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:11091"],
            userControl: false)]

        public void Garuda_Slipstream_Counter(Event @event, ScriptAccessory accessory)
        {
            if (majorPhase != 1 && !skipPhaseChecks)
            {
                return;
            }

            Interlocked.Increment(ref phase1_slipstreamCounter);

            if (2 <= phase1_slipstreamCounter && phase1_slipstreamCounter <= 4)
            {
                phase1_downburstSemaphore.Set();
            }

            if (enableDebugLogging)
            {
                accessory.Log.Debug($"phase1_slipstreamCounter={phase1_slipstreamCounter}");
            }
        }

        [ScriptMethod(name: "Garuda First Mistral Song (Data Collection)",
            eventType: EventTypeEnum.TargetIcon,
            eventCondition: ["Id:0010"],
            userControl: false)]

        public void Garuda_FirstMistralSong_DataCollection(Event @event, ScriptAccessory accessory)
        {
            if (majorPhase != 1 && !skipPhaseChecks)
            {
                return;
            }

            if (phase != 1 && !skipPhaseChecks)
            {
                return;
            }

            if (phase1_targetOfMistralSong != 0)
            {
                return;
            }

            if (!convertObjectIdToDecimal(@event["TargetId"], out var targetId))
            {
                return;
            }

            phase1_targetOfMistralSong = targetId;

            phase1_mistralSongSemaphore.Set();

            if (enableDebugLogging)
            {
                accessory.Log.Debug($"phase1_targetOfMistralSong={phase1_targetOfMistralSong}");
            }
        }

        [ScriptMethod(name: "Garuda First Mistral Song (Range)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:11091"])]

        public void Garuda_FirstMistralSong_Range(Event @event, ScriptAccessory accessory)
        {
            if (majorPhase != 1 && !skipPhaseChecks)
            {
                return;
            }

            if (phase != 1 && !skipPhaseChecks)
            {
                return;
            }

            if (!convertObjectIdToDecimal(@event["SourceId"], out var sourceId))
            {
                return;
            }

            bool wasSignalled = phase1_mistralSongSemaphore.WaitOne(COMMON_INTERVAL);

            if (!wasSignalled)
            {
                return;
            }

            var currentProperties = accessory.Data.GetDefaultDrawProperties();

            currentProperties.Scale = new(3.5f, 40);
            currentProperties.Owner = sourceId;
            currentProperties.TargetObject = phase1_targetOfMistralSong;
            currentProperties.Color = accessory.Data.DefaultSafeColor;
            currentProperties.DestoryAt = 5250;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperties);
        }

        [ScriptMethod(name: "Garuda First Great Whirlwind (Range)",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:11074"])]

        public void Garuda_FirstGreatWhirlwind_Range(Event @event, ScriptAccessory accessory)
        {
            if (majorPhase != 1 && !skipPhaseChecks)
            {
                return;
            }

            if (!string.Equals(@event["TargetIndex"], "1"))
            {
                return;
            }

            Vector3 targetPosition = ARENA_CENTER;

            try
            {
                targetPosition = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
            }
            catch (Exception e)
            {
                accessory.Log.Error("TargetPosition deserialization failed.");
                return;
            }

            var currentProperties = accessory.Data.GetDefaultDrawProperties();

            currentProperties.Scale = new(8);
            currentProperties.Position = targetPosition;
            currentProperties.Color = accessory.Data.DefaultDangerColor;
            currentProperties.DestoryAt = 18375;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperties);
        }

        [ScriptMethod(name: "Garuda Pull Plume (Guidance, OT Only)",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: ["DataId:8726"])]

        public void Garuda_PullPlume_Guidance_OTOnly(Event @event, ScriptAccessory accessory)
        {
            if (majorPhase != 1 && !skipPhaseChecks)
            {
                return;
            }

            if (phase != 1 && !skipPhaseChecks)
            {
                return;
            }

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            if (!isLegalPartyIndex(myIndex))
            {
                return;
            }

            if (myIndex != 1)
            {
                return;
            }

            if (!convertObjectIdToDecimal(@event["SourceId"], out var sourceId))
            {
                return;
            }

            var currentProperties = accessory.Data.GetDefaultDrawProperties();

            currentProperties.Name = "Garuda_PullPlume_Guidance_OTOnly";
            currentProperties.Scale = new(2);
            currentProperties.Owner = accessory.Data.Me;
            currentProperties.TargetObject = sourceId;
            currentProperties.ScaleMode |= ScaleMode.YByDistance;
            currentProperties.Color = colourOfDirectionIndicators.V4.WithW(1);
            currentProperties.DestoryAt = MAXIMUM_DURATION;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperties);
        }

        [ScriptMethod(name: "Garuda Pull Plume (Guidance Removal)",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:0011"],
            userControl: false)]

        public void Garuda_PullPlume_GuidanceRemoval(Event @event, ScriptAccessory accessory)
        {
            if (majorPhase != 1 && !skipPhaseChecks)
            {
                return;
            }

            if (phase != 1 && !skipPhaseChecks)
            {
                return;
            }

            if (!convertObjectIdToDecimal(@event["SourceId"], out var sourceId))
            {
                return;
            }

            var sourceObject = accessory.Data.Objects.SearchById(sourceId);

            if (sourceObject == null)
            {
                return;
            }

            if (sourceObject.DataId != 8726)
            {
                return;
            }

            accessory.Method.RemoveDraw("Garuda_PullPlume_Guidance_OTOnly");
        }

        [ScriptMethod(name: "Garuda Downburst (Range)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:11091"])]

        public void Garuda_Downburst_Range(Event @event, ScriptAccessory accessory)
        {
            if (majorPhase != 1 && !skipPhaseChecks)
            {
                return;
            }

            bool wasSignalled = phase1_downburstSemaphore.WaitOne(COMMON_INTERVAL);

            if (!wasSignalled)
            {
                return;
            }

            if (!convertObjectIdToDecimal(@event["SourceId"], out var sourceId))
            {
                return;
            }

            var currentProperties = accessory.Data.GetDefaultDrawProperties();

            currentProperties.Scale = new(12);
            currentProperties.Owner = sourceId;
            currentProperties.TargetResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
            currentProperties.TargetOrderIndex = 1;
            currentProperties.Color = colourOfExtremelyDangerousAttacks.V4.WithW(1);
            currentProperties.Delay = 2500;
            currentProperties.DestoryAt = 3500;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperties);
        }

        [ScriptMethod(name: "Garuda Feather Rain (Range)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:11085"])]

        public void Garuda_FeatherRain_Range(Event @event, ScriptAccessory accessory)
        {
            if (majorPhase != 1 && !skipPhaseChecks)
            {
                return;
            }

            Vector3 effectPosition = ARENA_CENTER;

            try
            {
                effectPosition = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
            }
            catch (Exception e)
            {
                accessory.Log.Error("EffectPosition deserialization failed.");
                return;
            }

            var currentProperties = accessory.Data.GetDefaultDrawProperties();

            currentProperties.Scale = new(3);
            currentProperties.Position = effectPosition;
            currentProperties.Color = accessory.Data.DefaultDangerColor;
            currentProperties.DestoryAt = 1000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperties);
        }

        [ScriptMethod(name: "Garuda Feather Rain (Phase Control)",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:11085"],
            suppress: COMMON_INTERVAL,
            userControl: false)]

        public void Garuda_FeatherRain_PhaseControl(Event @event, ScriptAccessory accessory)
        {
            if (majorPhase != 1 && !skipPhaseChecks)
            {
                return;
            }

            Interlocked.Increment(ref phase);

            if (enableDebugLogging)
            {
                accessory.Log.Debug($"majorPhase={majorPhase}\nphase={phase}");
            }
        }

        #endregion

        #region Ifrit

        #endregion

        #region Titan

        #endregion

        #region Ascian Lahabrea

        #endregion

        #region Ultima Weapon

        #endregion

        #region Commons

        public static bool convertObjectIdToDecimal(string? rawHexId, out ulong result)
        {
            result = 0;

            if (string.IsNullOrWhiteSpace(rawHexId))
            {
                return false;
            }

            string hexId = rawHexId.Trim();

            hexId = hexId.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? hexId.Substring(2) : hexId;

            return ulong.TryParse(hexId, System.Globalization.NumberStyles.HexNumber, null, out result);
        }

        public static int discretizePosition(Vector3 position, Vector3 center, int numberOfDirections, bool diagonalSplit = true)
        {
            if (diagonalSplit)
            {
                return (int)(
                    (Math.Round(
                        (numberOfDirections / 2.0d) - (numberOfDirections / 2.0d) * Math.Atan2(position.X - center.X, position.Z - center.Z) / Math.PI
                    ) % numberOfDirections + numberOfDirections) % numberOfDirections
                );
            }
            else
            {
                return (int)(
                    (Math.Floor(
                        (numberOfDirections / 2.0d) - (numberOfDirections / 2.0d) * Math.Atan2(position.X - center.X, position.Z - center.Z) / Math.PI
                    ) % numberOfDirections + numberOfDirections) % numberOfDirections
                );
            }
        }

        public static double getRotation(Vector3 position, Vector3 center)
        {
            return (position.Equals(center)) ?
                (0) :
                ((Math.PI - Math.Atan2(position.X - center.X, position.Z - center.Z) + 2 * Math.PI) % (2 * Math.PI));
        }

        public static double getRotationDifference(Vector3 position1, Vector3 position2, Vector3 center)
        {
            double rawDifference = (getRotation(position2, center) - getRotation(position1, center) + 2 * Math.PI) % (2 * Math.PI);

            return (rawDifference <= Math.PI) ? (rawDifference) : (rawDifference - 2 * Math.PI);
        }

        public static Vector3 rotatePosition(Vector3 position, Vector3 center, double radian, bool preserveHeight = true)
        {
            Vector2 positionInVector2 = new Vector2(position.X - center.X, position.Z - center.Z);
            double polarAngleAfterRotation = Math.PI - Math.Atan2(positionInVector2.X, positionInVector2.Y) + radian;

            return new Vector3((float)(center.X + Math.Sin(polarAngleAfterRotation) * positionInVector2.Length()),
                ((preserveHeight) ? (position.Y) : (center.Y)),
                (float)(center.Z - Math.Cos(polarAngleAfterRotation) * positionInVector2.Length()));
        }

        public static double convertPolarToCartesian(double polarRotation)
        {
            return Math.PI - polarRotation;
        }

        public static double convertDegreesToRadians(double degree)
        {
            return degree * Math.PI / 180.0;
        }

        public static bool isLegalPartyIndex(int partyIndex)
        {
            return (0 <= partyIndex && partyIndex <= 7);
        }

        public static bool isSupporter(int partyIndex)
        {
            return partyIndex switch
            {
                0 => true,
                1 => true,
                2 => true,
                3 => true,
                _ => false
            };
        }

        public static bool isDps(int partyIndex)
        {
            return partyIndex switch
            {
                4 => true,
                5 => true,
                6 => true,
                7 => true,
                _ => false
            };
        }

        public static bool isMelee(int partyIndex)
        {
            return partyIndex switch
            {
                0 => true,
                1 => true,
                4 => true,
                5 => true,
                _ => false
            };
        }

        public static bool isRanged(int partyIndex)
        {
            return partyIndex switch
            {
                2 => true,
                3 => true,
                6 => true,
                7 => true,
                _ => false
            };
        }

        public static bool isTank(int partyIndex)
        {
            return isSupporter(partyIndex) && isMelee(partyIndex);
        }

        public static bool isHealer(int partyIndex)
        {
            return isSupporter(partyIndex) && isRanged(partyIndex);
        }

        public static bool isMeleeDps(int partyIndex)
        {
            return isDps(partyIndex) && isMelee(partyIndex);
        }

        public static bool isRangedDps(int partyIndex)
        {
            return isDps(partyIndex) && isRanged(partyIndex);
        }

        public static bool isInGroup1(int partyIndex)
        {
            return partyIndex switch
            {
                0 => true,
                2 => true,
                4 => true,
                6 => true,
                _ => false
            };
        }

        public static bool isInGroup2(int partyIndex)
        {
            return partyIndex switch
            {
                1 => true,
                3 => true,
                5 => true,
                7 => true,
                _ => false
            };
        }

        #endregion
    }

    #region Extensions

    public static class ScriptAccessoryExtensions
    {
        public static void tts(this ScriptAccessory accessory, string text, bool enableVanillaTts, bool enableDailyRoutinesTts)
        {
            if (enableVanillaTts)
            {
                accessory.Method.TTS(text);
            }
            else
            {
                if (enableDailyRoutinesTts)
                {
                    accessory.Method.SendChat($"/pdr tts {text}");
                }
            }
        }
    }

    #endregion
}
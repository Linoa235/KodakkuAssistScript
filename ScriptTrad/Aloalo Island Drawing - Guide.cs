// File: AloaloIslandDrawingAndGuide_Mao.cs
using System;
using Newtonsoft.Json;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Data;
using Dalamud.Utility.Numerics;
// using ECommons;
// using ECommons.GameFunctions;
// using ECommons.DalamudServices;
using Dalamud.Game;
// using Dalamud.Game.ClientState.Objects.Types;
// using Dalamud.Game.ClientState.Statuses;
// using IBattleCharaDalamud = Dalamud.Game.ClientState.Objects.Types.IBattleChara;
// using System.Reflection;
using Util = TsingNamespace.AloaloIsland.TsingUtilities;
using static TsingNamespace.AloaloIsland.ScriptExtensions_Tsing;
// using Status = Dalamud.Game.ClientState.Statuses.Status;
// using StatusList = Dalamud.Game.ClientState.Statuses.StatusList;

namespace TsingNamespace.AloaloIsland
{

    [ScriptType(name: "Aloalo Island Drawing + Guide", territorys: [1179, 1180], guid: "4430e55f-5199-4ca2-9372-78b8a4624e8d", version: "0.0.1.5", author: "Linoa235", note: noteStr)]
    public class AloaloIslandScript
    {   
        const string noteStr =
        """
        Based on Maomao Nest Guide;
        Mobs: Provides target filtering function, enabled by default, can be disabled in method settings.
            â†‘ When the current group of mobs is not defeated, the next group of mobs cannot be targeted (Crab Boss group will be released when Shrimp auto-attacks).
            â†‘ Tree grouping strategy follows MMW guide; all mobs will forcibly release target selection if triggered accidentally.
        Boss 1 uses fusion method by default, can be modified in user settings.
        Boss 2 provides facing correction function, can be enabled in user settings.
        Boss 2 can set facing indicator color depth separately for double light orb + step floor fire.
        """;
        [UserSetting("Color for guiding => Type: Immediate Go")]
        public ScriptColor GuideColor_GoNow { get; set; } = new() { V4 = new(0, 1, 1, 2) };
        [UserSetting("Color for guiding => Type: Go Later")]
        public ScriptColor GuideColor_GoLater { get; set; } = new() { V4 = new(1, 1, 0, 2) };
        [UserSetting("Width of guide effect during Vfx drawing")]
        public float Guide_Width { get; set; } = 1.4f;
        [UserSetting("Color depth for guiding")]
        public float GuideColorDensity { get; set; } = 2;

        [UserSetting("Boss 1 First Crystal Mechanism Guidance Basis")]
        public Boss1_WalkthroughEnum Boss1_Walkthrough { get; set; } = Boss1_WalkthroughEnum.FusionMethod;
        public enum Boss1_WalkthroughEnum { FusionMethod, NorthSouthMethod, QueueMethod }

        [UserSetting("Boss 2 Planar Rune (Minesweeper) Forced Movement Enable Facing Correction")]
        public bool Boss2_TowardsRound { get; set; } = false;

        [UserSetting("Boss 2 Facing Indicator Color Depth Increment")]
        public float Boss2_PizzaColorDeepen { get; set; } = 2;

        // Kod internal party member order (independent of in-game party list order, related to /KTeam window) and role correspondence.
        // 0-MT,1-H1,2-D1,3-D2
        // Party member index starts from 0
        [UserSetting("Default Role Order")]
        public RoleMarksListEnum RoleMarks4 { get; set; }
        public enum RoleMarksListEnum { MT_H1_D1_D2 }
        public enum RoleMarkEnum { MT, H1, D1, D2 }

        // For storing trigger timestamps
        private ConcurrentDictionary<string, long> invokeTimestamp = new ConcurrentDictionary<string, long>();
        // A CancellationTokenSource for cancelling delayed tasks
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static readonly object _lock = new object();

        // Boss1 Crystal counter, to avoid duplicate guidance
        //private int springCrystalsCount = 0;

        // Boss1 Crystal safe zone storage
        private List<Vector2> boss1_springCrystalsSafePoints = new List<Vector2>();

        // Boss1 Fluke Gale cast count
        // private int boss1_flukeGaleCastingCount = 0;

        // Boss1 Hydrobomb cast count
        //private int boss1_hydrobombCastingCount = 0;

        // Boss1 Marked Small Bubble mechanism movement clockwise/counterclockwise
        private bool boss1_bubbleIsClockwise = false;

        // Boss1 Marked Circle/Donut + Large Bubble (one of the two bubbles near the center on the north side of the arena)
        private Vector3 boss1_twintidesBubbleType = new(0,0,0);

        // Boss1 Marked whether it has reached the phase after the mob guide
        private bool boss1_phaseAfterMob = false;


        // Boss2 boss2 ID
        private uint boss2_bossId = 0;

        // Boss2 Inferno Theorem cast
        private uint boss2_InfernoTheoremCastingCount = 0;
        private bool boss2_InfernoTheoremCasted = false;

        // Boss2 Mine mechanism mine positions
        private List<Vector3> boss2_ArcaneMinesList = new List<Vector3>();
        private Vector3 boss2_myTowardsPoint = new (200,-300,0);


        //Boss3 Trick Reload information
        private List<uint> boss3_trickReloads = new List<uint>();
        private uint boss3_trickReloadsCount = 0;	

        //Boss3 Bomb
        private uint boss3_bombsRound = 0;
        //Boss3 Bomb to number point distance information
        private List<float[]> boss3_rad_distance = new List<float[]>
        {
            new float[]{-0.25f * MathF.PI,100},
            new float[]{0.25f * MathF.PI,100},
            new float[]{0.75f * MathF.PI,100},
            new float[]{-0.75f * MathF.PI,100}
        };
        //Boss3 Second turntable related data
        private List<float> boss3_fireSpreadRotation = new List<float>();
        private List<uint> boss3_burningChainsPlayers = new List<uint>();
        private bool boss3_isFireBallClockwise = false;

        public void Init(ScriptAccessory accessory)
        {
            accessory.Log.Debug($"Initialize! => AloaloIslandScript");
            accessory.Method.RemoveDraw(".*");
            dataId_entityIds.Clear();
            foreach (uint _dataId in Enum.GetValues(typeof(Mob_Bundle.IdData)))
            {
                List<uint> _entityIds = new List<uint>();
                dataId_entityIds.AddOrUpdate(_dataId, key => _entityIds, (key, oldValue) => _entityIds);
            }
            Woodens = new uint[4] {1,1,1,1}; 
            //springCrystalsCount = 0;
            boss1_springCrystalsSafePoints.Clear();
            // boss1_flukeGaleCastingCount = 0;
            //boss1_hydrobombCastingCount = 0;
            boss1_bubbleIsClockwise = false;
            boss1_twintidesBubbleType = new (0,0,0);
            boss1_phaseAfterMob = false;
            boss2_InfernoTheoremCastingCount = 0;
            boss2_InfernoTheoremCasted = false;
            boss2_ArcaneMinesList.Clear();
            boss2_myTowardsPoint = new (200,-300,0);
            boss3_trickReloads.Clear();
            boss3_trickReloadsCount = 0;
            //boss3_bombs.Clear();
            boss3_bombsRound = 0;
            boss3_rad_distance = new List<float[]>{new float[]{-0.25f * MathF.PI,100},new float[]{0.25f * MathF.PI,100},new float[]{0.75f * MathF.PI,100},new float[]{-0.75f * MathF.PI,100}};
            boss3_fireSpreadRotation.Clear();
            boss3_burningChainsPlayers.Clear();
            boss3_isFireBallClockwise = false;


            //Clear trigger timestamp records + delayed tasks
            invokeTimestamp.Clear();
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
        }

        #region Mob1
        /*
            Target filtering
            When the first wave of mobs spawns, only the Shrimp is targetable
            When the Shrimp's first auto-attack triggers, the Crab becomes targetable
            When the Crab's HP drops below 25%, the Triple Water Cannon becomes targetable
            When the Triple Water Cannon's HP drops below 25%, the final mob becomes targetable

            Forced release: when auto-attack triggers, force release (may have been triggered early)
        */
        
        private static class Mob_Bundle
        {
            public enum IdData : uint
            {
                NTwister = 35776, // Wind circle action Helper->self, 0.5s cast, range 6 circle
                STwister = 35791, // Helper->self, 0.5s cast, range 6 circle
                NKiwakin = 0x40C8, // R3.750, x1 Shrimp
                NSnipper = 0x40C9, // R3.600, x1 Large Crab
                NCrab = 0x40CA, // R1.120, x2 Small Crab
                NMonk = 0x40CB, // R3.000, x1 Very painful single-target knockback Octopus
                NRay = 0x40CC, // R3.200, x1 Triple Water Cannon
                NPaddleBiter = 0x40CD, // R1.650, x2 Triple Water Cannon's minions

                SKiwakin = 0x40D2, // R3.750, x1 Shrimp
                SSnipper = 0x40D3, // R3.600, x1 Large Crab
                SCrab = 0x40D4, // R1.120, x2 Small Crab
                SMonk = 0x40D5, // R3.000, x1 Very painful single-target knockback Octopus
                SRay = 0x40D6, // R3.200, x1 Triple Water Cannon
                SPaddleBiter = 0x40D7, // R1.650, x2 Triple Water Cannon's minions
                NWoodGolem = 0x40D0, // R2.660 Tornado Treant
                NIslekeeper = 0x40D1, // R2.550 DOT Treant

                SWoodGolem = 0x40DA, // R2.660 Tornado Treant
                SIslekeeper = 0x40DB, // R2.550 DOT Treant

                AutoAttack1 = 31318, // *Kiwakin/*Crab/*Snipper->player, no cast, single-target
                AutoAttack2 = 31320, // *PaddleBiter/*Ray/*Monk->player, no cast, single-target *WoodGolem/*Islekeeper->player, no cast, single-target

                NAncientAero = 35916, // NWoodGolem->self, 5.0s cast, range 100 circle, interruptible heavy raidwide
                NTornado = 35917, // NWoodGolem->player, 5.0s cast, range 4 circle spread
                NOvation = 35777, // NWoodGolem->self, 4.0s cast, range 12 width 4 rect
                SAncientAero = 35794, // SWoodGolem->self, 5.0s cast, range 100 circle, interruptible heavy raidwide
                STornado = 35795, // SWoodGolem->player, 5.0s cast, range 4 circle spread
                SOvation = 35796, // SWoodGolem->self, 4.0s cast, range 12 width 4 rect

                NIsleDrop = 35951, // NIslekeeper->location, 5.0s cast, range 6 circle puddle
                SIsleDrop = 35900, // SIslekeeper->location, 5.0s cast, range 6 circle puddle

                NLeadHook = 35950, // Shrimp triple tankbuster NKiwakin->player, 4.0s cast, single-target, 3-hit tankbuster
                SLeadHook = 35783, // SKiwakin->player, 4.0s cast, single-target, 3-hit tankbuster
                NSharpStrike = 35939, // Shrimp DOT tankbuster NKiwakin->player, 5.0s cast, single-target, tankbuster
                SSharpStrike = 35784, // SKiwakin->player, 5.0s cast, single-target, tankbuster
                NCrabDribble = 35770, // Crab back slice NSnipper->self, 1.5s cast, range 6 120-degree cone
                SCrabDribble = 35787, // SSnipper->self, 1.5s cast, range 6 120-degree cone
                NCrossAttack = 35771, // Octopus tankbuster NMonk->player, 5.0s cast, single-target tankbuster
                SCrossAttack = 35919, // SMonk->player, 5.0s cast, single-target tankbuster
            }
            public const string FilterDataIdWithKiwakin = "TargetDataId:regex:^(1658[4-9]|1659[4-9]|1659[2-3]|1660[2-3])$";
            public const string FilterDataId = "DataId:regex:^(1658[5-9]|1659[5-9]|1659[2-3]|1660[2-3])$";
            public const string AutoAttackActionId = $"ActionId:regex:^(31318|31320)$";
            public const string TornadoTwisterActionId = $"ActionId:regex:^(35776|35791)$";
            public const string WoodGolemTornadoActionId = $"ActionId:regex:^(35917|35795)$";
            public const string WoodGolemOvationActionId = $"ActionId:regex:^(35777|35796)$";
            public const string IslekeeperIsleDropActionId = $"ActionId:regex:^(35951|35900)$";
            public const string LeadHookActionId = $"ActionId:regex:^(35950|35783)$";
            public const string SharpStrikeActionId = $"ActionId:regex:^(35939|35784)$";
            public const string CrabDribbleActionId = $"ActionId:regex:^(35770|35787)$";
            public const string CrossAttackActionId = $"ActionId:regex:^(35771|35919)$";

            public const string AutoAttackActionId_Player = $"ActionId:regex:^(7|8)$";
            public static Vector3 Mob2_FieldCenter = new (200,-306,128);
            
        }
        private uint[] Woodens = new uint[4] {1,1,1,1};
        private ConcurrentDictionary<uint,List<uint>> dataId_entityIds = new ConcurrentDictionary<uint, List<uint>>();
        [ScriptMethod(name: "Mob Target Disable When Spawning (Except Shrimp and First Treant)", eventType: EventTypeEnum.AddCombatant, eventCondition: [Mob_Bundle.FilterDataId])]
        public async void Mob_TargetDisable(Event @event, ScriptAccessory accessory)
        {
            await Task.Delay(3000);
            uint _id = @event.GetSourceId();
            uint _dataId = @event.GetDataId();
            if(dataId_entityIds.TryGetValue(_dataId, out List<uint> _entityIds))
            {
                lock(_lock){
                    _entityIds.Add(_id);
                }
                DisTargetable(_id,accessory);
            }

            //Number treants by coordinate
            List<uint> _woodens = new List<uint>
            {
                (uint)Mob_Bundle.IdData.NWoodGolem,
                (uint)Mob_Bundle.IdData.SWoodGolem,
                (uint)Mob_Bundle.IdData.NIslekeeper,
                (uint)Mob_Bundle.IdData.SIslekeeper
            };
            if (_woodens.Contains(_dataId))
            {
                lock(_lock)
                {
                    //Is a treant
                    Vector3 _woodenPos = @event.GetSourcePosition();
                    Vector3 centerToWooden = _woodenPos - Mob_Bundle.Mob2_FieldCenter;
                    if(centerToWooden.X < 0)
                    {
                        if(centerToWooden.Z < 0)
                        {
                            Woodens[0] = _id;
                        }
                        else
                        {
                            Woodens[3] = _id;
                        }
                    }
                    else
                    {
                        if(centerToWooden.Z < 0)
                        {
                            Woodens[1] = _id;
                        }
                        else
                        {
                            Woodens[2] = _id;
                        }
                    }
                    if(_id == Woodens[0])
                    {
                        SetTargetable(_id, accessory);
                    }
                }
            }
            
        }
        [ScriptMethod(name: "Mob Target Enable When AutoAttack Triggers", eventType: EventTypeEnum.ActionEffect, eventCondition: [Mob_Bundle.AutoAttackActionId])]
        public void Mob_TargetEnableWhenAutoAttack(Event @event, ScriptAccessory accessory)
        {
            
            uint _id = @event.GetSourceId();
            //Check if Id exists in dict
            IGameObject? _sourceObject = accessory.Data.Objects.SearchByEntityId(_id);
            if(_sourceObject != null && dataId_entityIds.ContainsKey(_sourceObject.DataId))
            {
                SetTargetable(_id, accessory);
            }
            //When Shrimp auto-attack triggers, make Crab targetable
            if(_sourceObject != null && (_sourceObject.DataId == (uint)Mob_Bundle.IdData.NKiwakin || _sourceObject.DataId == (uint)Mob_Bundle.IdData.SKiwakin))
            {
                //Collect Crab group IDs
                accessory.Log.Debug($"Mob Target Enable When AutoAttack => Kiwakin AutoAttack");
                uint[] _dataIds = new uint[]
                {
                    (uint)Mob_Bundle.IdData.NSnipper,(uint)Mob_Bundle.IdData.SSnipper,
                    (uint)Mob_Bundle.IdData.NCrab,(uint)Mob_Bundle.IdData.SCrab
                };
                SetTargetable(_dataIds,dataId_entityIds,accessory);
            }
        }
        //TODO When receiving mob death information, forcibly release the next group
        [ScriptMethod(name: "Mob Target Enable When Current Group Dies", eventType: EventTypeEnum.Death, eventCondition: [Mob_Bundle.FilterDataIdWithKiwakin])]
        public async void Mob_TargetEnableWhenDeath(Event @event, ScriptAccessory accessory)
        {
            uint mobDataId = @event.GetTargetDataId();
            uint _id = @event.GetTargetId();
            accessory.Log.Debug($"Mob Target Enable When Death mobDataId => {mobDataId}");
            uint[] _dataIds = new uint[]{0};
            switch(mobDataId)
            {
                case (uint)Mob_Bundle.IdData.NKiwakin:
                case (uint)Mob_Bundle.IdData.SKiwakin:
                    //Shrimp died, release Crab group
                    _dataIds = new uint[]
                    {
                        (uint)Mob_Bundle.IdData.NSnipper,(uint)Mob_Bundle.IdData.SSnipper,
                        (uint)Mob_Bundle.IdData.NCrab,(uint)Mob_Bundle.IdData.SCrab
                    };
                    break;
                case (uint)Mob_Bundle.IdData.NSnipper:
                case (uint)Mob_Bundle.IdData.SSnipper:
                    //Crab died, release Water Cannon group
                    _dataIds = new uint[]
                    {
                        (uint)Mob_Bundle.IdData.NRay,(uint)Mob_Bundle.IdData.SRay,
                        (uint)Mob_Bundle.IdData.NPaddleBiter,(uint)Mob_Bundle.IdData.SPaddleBiter
                    };
                    break;
                case (uint)Mob_Bundle.IdData.NRay:
                case (uint)Mob_Bundle.IdData.SRay:
                    //Triple Water Cannon died, release Octopus
                    _dataIds = new uint[]
                    {
                        (uint)Mob_Bundle.IdData.NMonk,(uint)Mob_Bundle.IdData.SMonk
                    };
                    break;
                case (uint)Mob_Bundle.IdData.NMonk:
                case (uint)Mob_Bundle.IdData.SMonk:
                    break;
                case (uint)Mob_Bundle.IdData.NWoodGolem:
                case (uint)Mob_Bundle.IdData.SWoodGolem:
                case (uint)Mob_Bundle.IdData.NIslekeeper:
                case (uint)Mob_Bundle.IdData.SIslekeeper:
                    uint[] woodensIndex = Woodens;
                    int _index = Array.IndexOf(woodensIndex, _id);
                    if (_index > -1 && _index < woodensIndex.Length - 1)
                    {
                        uint _nextWoodenId = woodensIndex[_index + 1];
                        SetTargetable(_nextWoodenId, accessory);
                    }
                    break;
            }
            SetTargetable(_dataIds,dataId_entityIds,accessory);
        }
        [ScriptMethod(name: "Mob Target Enable When Current Group About to Die", eventType: EventTypeEnum.ActionEffect, eventCondition: [Mob_Bundle.AutoAttackActionId_Player])]
        public void Mob_TargetEnableWhenRoundEnd(Event @event, ScriptAccessory accessory)
        {
            uint _id = @event.GetTargetId();
            //Check if Id exists in dict
            ICharacter? _targetObject = accessory.Data.Objects.SearchByEntityId(_id) as ICharacter;
            if(_targetObject != null && dataId_entityIds.ContainsKey(_targetObject.DataId))
            {
                float hpPer = (float)_targetObject.CurrentHp / (float)_targetObject.MaxHp;
                if(hpPer < 0.25f)
                {
                    uint[] _dataIds = new uint[]{1};
                    switch(_targetObject.DataId)
                    {
                        case (uint)Mob_Bundle.IdData.NSnipper:
                        case (uint)Mob_Bundle.IdData.SSnipper:
                            accessory.Log.Debug($"Mob Target Enable When Round End hpPer => {hpPer}");
                            //Crab about to die, release Water Cannon group
                            _dataIds = new uint[]
                            {
                                (uint)Mob_Bundle.IdData.NRay,(uint)Mob_Bundle.IdData.SRay,
                                (uint)Mob_Bundle.IdData.NPaddleBiter,(uint)Mob_Bundle.IdData.SPaddleBiter
                            };
                            accessory.Log.Debug($"Mob Target Enable When Round End _dataIds => {_dataIds[0]}");
                            break;
                        case (uint)Mob_Bundle.IdData.NRay:
                        case (uint)Mob_Bundle.IdData.SRay:
                            accessory.Log.Debug($"Mob Target Enable When Round End hpPer => {hpPer}");
                            //Triple Water Cannon about to die, release Octopus
                            _dataIds = new uint[]
                            {
                                (uint)Mob_Bundle.IdData.NMonk,(uint)Mob_Bundle.IdData.SMonk
                            };
                            accessory.Log.Debug($"Mob Target Enable When Round End _dataIds => {_dataIds[0]}");
                            break;
                        case (uint)Mob_Bundle.IdData.NWoodGolem:
                        case (uint)Mob_Bundle.IdData.SWoodGolem:
                        case (uint)Mob_Bundle.IdData.NIslekeeper:
                        case (uint)Mob_Bundle.IdData.SIslekeeper:
                            accessory.Log.Debug($"Mob Target Enable When Round End hpPer => {hpPer}");
                            uint[] woodensIndex = Woodens;
                            int _index = Array.IndexOf(woodensIndex, _id);
                            if (_index > -1 && _index < woodensIndex.Length - 1)
                            {
                                uint _nextWoodenId = woodensIndex[_index + 1];
                                SetTargetable(_nextWoodenId, accessory);
                            }
                            accessory.Log.Debug($"Mob Target Enable When Round End _dataIds => {_dataIds[0]}");
                            break;
                    }
                    SetTargetable(_dataIds,dataId_entityIds,accessory);
                }

            }
        }
        private void SetTargetable(uint _id, ScriptAccessory accessory)
        {
            IGameObject? _targetObject = accessory.Data.Objects.SearchByEntityId(_id);
            if(_targetObject != null && !_targetObject.IsDead && !_targetObject.IsTargetable)
            {
                accessory.Log.Debug($"Set Targetable => {_targetObject}");
                unsafe
                {
                  FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* _targetObjectStruct = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)_targetObject.Address;
                  _targetObjectStruct-> TargetableStatus |= FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectTargetableFlags.IsTargetable;
                }
            }
        }
        private void DisTargetable(uint _id, ScriptAccessory accessory)
        {
            IGameObject? _targetObject = accessory.Data.Objects.SearchByEntityId(_id);
            if(_targetObject != null && _targetObject.IsTargetable)
            {
                accessory.Log.Debug($"Dis Targetable => {_targetObject}");
                unsafe
                {
                  FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* _targetObjectStruct = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)_targetObject.Address;
                  _targetObjectStruct-> TargetableStatus &= ~FFXIVClientStructs.FFXIV.Client.Game.Object.ObjectTargetableFlags.IsTargetable;
                }
            }
        }
        
        private void SetTargetable(uint[] _dataIds,ConcurrentDictionary<uint,List<uint>> _dict, ScriptAccessory accessory)
        {
            List<uint> entityIds = new List<uint>();
            List<IGameObject> entityIBattleCharas = new List<IGameObject>();
            foreach (uint _dataId in _dataIds)
            {
                if(_dict.TryGetValue(_dataId, out List<uint> _entityIds))
                {
                    entityIds = entityIds.Union(_entityIds).ToList();
                }
                // Find current targets of this dataId
                IEnumerable<IGameObject> _temp = accessory.GetEntitiesByDataId(_dataId).Where(obj => obj is IBattleChara bcObj && bcObj.MaxHp > 10000);
                entityIBattleCharas = entityIBattleCharas.Union(_temp).ToList();
            }
            foreach (uint _entityId in entityIds)
            {
                IGameObject? _entityObject = accessory.Data.Objects.SearchByEntityId(_entityId);
                if(_entityObject != null && _dict.ContainsKey(_entityObject.DataId))
                {
                    SetTargetable(_entityId, accessory);
                }
            }
            foreach (IGameObject obj in entityIBattleCharas)
            {
                SetTargetable(obj.EntityId, accessory);
            }
        }




        //Draw danger zone and danger zone pre-alert for the wind circle on the first group of mobs' arena
        [ScriptMethod(name: "Mob 1 Tornado Dangerous Zone Draw", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35776|35791)$"])]
        public void Mob1_TornadoDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            DrawPropertiesEdit propFan = accessory.GetDrawPropertiesEdit(@event.GetSourceId(),new(5.5f),1000,false);
            propFan.Offset = new(0, 0, -2.7f);
            propFan.Radian = MathF.PI;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, propFan);

            DrawPropertiesEdit propDisplacement = accessory.GetDrawPropertiesEdit(propFan.Owner,new(5, 4.75f),propFan.DestoryAt,false);
            propDisplacement.Offset = propFan.Offset;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, propDisplacement);

        }


        //First group mob Tail Screw 35768(4.7s) alert
        [ScriptMethod(name: "Mob 1 Tail Screw Dangerous Zone Draw", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35768|35785)$"])]
        public void Mob1_TailScrewDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            //accessory.Log.Debug($"Actived! => Mob 1 Tail Screw Dangerous Zone Draw");
            accessory.FastDraw(DrawTypeEnum.Circle,@event.GetEffectPosition(),new(4.1f),new (0,4500),false);
        }

        //First group mob Bubble Shower + Crab Dribble alert
        //Bubble Shower + Crab Dribble are consecutive two-hit AOEs, starting with Bubble Shower's cast. Bubble Shower skill ID is 35769, cast duration 4.7s. Crab Dribble skill ID is 35770, cast duration 1.2s.
        [ScriptMethod(name: "Mob 1 Crab Front/Back Slice Dangerous Zone Draw", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35769|35786)$"])]
        public void Mob1_BubbleShowerDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            //accessory.Log.Debug($"Actived! => Mob 1 Bubble Shower Dangerous Zone Draw");
            //Front AOE
            accessory.FastDraw(DrawTypeEnum.Fan,@event.GetSourceId(),new Vector2(9.1f),new (0,4400),false);
            //Back AOE can be considered as a 5.9s cast, alert at 3.5s
            DrawPropertiesEdit propFanBack = accessory.GetDrawPropertiesEdit(@event.GetSourceId(),new(6.1f),4300,false);
            propFanBack.Rotation = MathF.PI;
            propFanBack.Radian = 2 * MathF.PI / 3;
            propFanBack.Delay = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, propFanBack);

        }

        //First group mob Hydrocannon 35773(4.7s)+Banish 35775(4.7s)+Electro Vortex 35774(4.7s) <= triple combo, written together
        [ScriptMethod(name: "Mob 1 Circle Donut Triple Combo Dangerous Zone Draw", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35773|35915)$"])]
        public void Mob1_HydrocannonDangerousZoneDraw(Event @event, ScriptAccessory accessory)
        {
            //accessory.Log.Debug($"Actived! => Mob 1 Hydrocannon Dangerous Zone Draw");
            accessory.FastDraw(DrawTypeEnum.Rect,@event.GetSourceId(),new(6, 15),new (0,4500),false);

            DrawPropertiesEdit propCircle = accessory.GetDrawPropertiesEdit(@event.GetSourceId(),new(8.1f),4500,false);
            propCircle.Delay = 6900;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, propCircle);

            DrawPropertiesEdit propDonut = accessory.GetDrawPropertiesEdit(@event.GetSourceId(),new(30.1f),4500,false);
            propDonut.InnerScale = new(7.9f);
            propDonut.Radian = 2 * MathF.PI;
            propDonut.Delay = 13800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, propDonut);
        }

        //First group mob Hydro Shot 35941(4.7s) knockback direction alert
        //Drawing idea: start point is targetPosition, end point is boss, rotation Pi, width 1, length 7
        [ScriptMethod(name: "Mob 1 Knockback Target Knockback Direction Draw", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35941|35793)$"])]
        public void Mob1_HydroShotRepulseDirectionDraw(Event @event, ScriptAccessory accessory)
        {
            //accessory.Log.Debug($"Actived! => Mob 1 Hydro Shot Repulse Direction Draw");
            DrawPropertiesEdit propDisplacement = accessory.GetDrawPropertiesEdit(@event.GetTargetId(),new(1, 7),4500,false);
            propDisplacement.TargetPosition = @event.GetSourcePosition();
            propDisplacement.Rotation = MathF.PI;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, propDisplacement);
            
        }
        #endregion

        #region TODO
        // Mob 1 Wind Circle Path Indication
        // Boss 1 Circle/Donut Pre-position
        // Boss 1 Raging Seas Stack/Spread Range Indication
        // Boss 1 IDs that need correction: Bubble dataId, Water Wall dataId, Twintails dataId
        // Boss 2 Subtraction Explosion b guidance optimization for 4 mines: if the first group of cross runes has 4 mines with layer count not 2, guide left to eat one more layer before final position
        #endregion

        #region Boss1
        //Boss model removal, usually used to initialize information
        [ScriptMethod(name: "Boss RemoveCombatant", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:regex:^(16540|16547|16446|16604|16477|16485)$"], userControl: false)]
        public async void BossCombatantInitialize(Event @event, ScriptAccessory accessory)
        {
            if (IsInSuppress(2000, nameof(BossCombatantInitialize))){
                return;
            };
            accessory.Log.Debug($"Actived! => Boss RemoveCombatant Initialize");
            //Since there is an operation to clear timestamps in the Init method, delay 1 second to exclude other simultaneous triggers
            await DelayMillisecond(1000);
            Init(accessory);
        }

        /*How to calculate the safe zone for Boss1's first crystal;
        Note: Coordinates in FFXIV are expressed as (a,b,c), where b is the Z-axis coordinate, a is the X-axis, c is the Y-axis;
        Let the center of the Boss arena be origin O, the coordinate of the crystal near the center is P0(x,y);
        In actual testing, the origin O has in-game coordinates (0,0,0), and the absolute values of x and y are 5;
        The four base points are P1(3x,3y), P2(3x,-y), P3(-x,-y), P4(-x,3y);
        Based on the crystal's model, there are vertical increments (distance 2y), increment sign is (int)Math.Round(-y/Math.Abs(y));
        Or horizontal increments (distance 2x), increment sign is (int)Math.Round(-x/Math.Abs(x));
        The incremented point needs to have the following characteristics: 1, the distance from P0 is farther than before increment; 2, the horizontal coordinate relative to origin O is not greater than 3.5x, and the vertical coordinate relative to origin is not greater than 3.5y;
        In-game logic:
        When the crystal spawns on the ground, it releases a no-cast skill Impact 35498, the SourcePosition in the log line is the crystal's landing point. When the absolute values of the crystal's landing point's horizontal and vertical coordinates are both less than 6, it is determined as the base crystal;
        */
        [ScriptMethod(name: "Boss 1 Crystal Safe Zone Draw", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(35498|35546)$"])]
        public void Boss1_FirstSpringCrystalsSafeZoneDraw(Event @event, ScriptAccessory accessory)
        {
            Vector3 crystalPos = @event.GetSourcePosition();
            if(Math.Abs(crystalPos.X) > 6 || Math.Abs(crystalPos.Z) > 6 )
            {
                return;
            }
            accessory.Log.Debug($"Actived! => Boss 1 Spring Crystals Safe Zone Draw");
            //Template is the horizontal crystal near the center in the first quadrant of FFXIV coordinates
            Vector2 safePoint1_template = new (15,15);
            Vector2 safePoint2_template = new (15,-15);
            Vector2 safePoint3_template = new (-5,-15);
            Vector2 safePoint4_template = new (-5,15);

            /*
            1, Calculate the angle value of the center crystal's coordinates
            2, Add the crystal's orientation: 0 for horizontal crystal, -1.57 for vertical crystal
            
            Angle Ï€/4, horizontal crystal, no rotation; vertical crystal, origin rotation +Ï€/2
            Angle 3Ï€/4, horizontal crystal, origin rotation Ï€; vertical crystal, origin rotation Ï€/2
            Angle 5Ï€/4, horizontal crystal, origin rotation Ï€; vertical crystal, origin rotation 3Ï€/2
            Angle 7Ï€/4, horizontal crystal, origin rotation 2Ï€; vertical crystal, origin rotation 3Ï€/2

            */

            double rad = 0 ;
            double crystalPosRad = MathF.Atan2(@event.GetSourcePosition().Z,@event.GetSourcePosition().X);
            crystalPosRad = crystalPosRad < 0 ? crystalPosRad + 2 * Math.PI : crystalPosRad ;
            if(Math.Abs(@event.GetSourceRotation()) > 1){
                //Vertical crystal
                rad = crystalPosRad < Math.PI ? Math.PI * 1f/2 : Math.PI * 3f/2 ;

            }
            else
            {
                rad = ((crystalPosRad > Math.PI * 1f/2)&&(crystalPosRad < Math.PI * 3f/2) ) ? Math.PI : 0; 
                //Horizontal crystal
            }
            accessory.Log.Debug($"Boss 1 Spring Crystals Safe Zone : rotate rad => {rad}");

            List<Vector2> safePoints = new List<Vector2>();
            safePoints.Add(Util.RotatePoint(safePoint1_template,rad));
            safePoints.Add(Util.RotatePoint(safePoint2_template,rad));
            safePoints.Add(Util.RotatePoint(safePoint3_template,rad));
            safePoints.Add(Util.RotatePoint(safePoint4_template,rad));

            boss1_springCrystalsSafePoints.Clear();
            boss1_springCrystalsSafePoints.AddRange(safePoints);

            for (int i = 0; i < safePoints.Count; i++)
            {
                DrawPropertiesEdit propRect = accessory.GetDrawPropertiesEdit(
                    new Vector3(safePoints[i].X,@event.GetSourcePosition().Y,safePoints[i].Y)
                    ,new(10,10),31000,true);
                propRect.Offset = new(0, 0, 5);
                propRect.Delay = 2000;
                propRect.Color = accessory.Data.DefaultSafeColor.WithW(0.4f);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, propRect);
            };
        }

        /*How to assign a safe zone for the first crystal stack or spread; <= using fusion method
        Bubble buff guidance, endpoint has two options: near the boss or less wind; <= using less wind, simpler to implement
        Stop buff guidance, calculate two base points based on the center crystal's position; if spread, go to the nearest second-ma safe zone; if stack, go to the nearest first-ma safe zone
        Need to record four points, each is the center point of a quarter of the arena; need to record the mahjong number of that quarter (or the crystal's orientation, 0 for first-ma, -1.57 for second-ma)
        Need two parameters: 1. buff type 2. role position (MT H1 D1 D2)
        1. Bubble buff + MT/D2 go to the north half (Z-axis coordinate less than 0); Bubble buff + H1/D1 go to the south half (Z-axis coordinate greater than 0);
        2. Stop buff + MT/D2 go to the safe zone near the northwest corner; Stop buff + H1/D2 go to the safe zone near the southeast corner; stack goes to first-ma area, spread goes to second-ma area

        After Boss1 completes the bubble debuff + stack/spread debuff assignment, Boss will cast Fluke Gale 35505(2.7s), using this cast as the starting point for the first crystal mechanism guidance
        Priority MT D2 D1 H1
        Bubble Net Echo debuff=3743(bubble), Bubble Aggregation debuff=3788(stop)
        Selected target Hydrobomb debuff=3748(spread), selected target Waterfall debuff=3747(stack)
        */
        [ScriptMethod(name: "Boss 1 Crystal Guidance (Default Fusion Method)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:35505"])]
        public void Boss1_FirstSpringCrystalsGuide(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"Actived! => Boss 1 Spring Crystals Guide");
            if(IsInSuppress(2000, nameof(Boss1_FirstSpringCrystalsGuide)))
            {
                return;
            }

            //Priority for this mechanism
            //(MT=0,D2=3,D1=2,H1=1)
            int[] roleMarkPriority = new int[] { (int)RoleMarkEnum.MT, (int)RoleMarkEnum.D2, (int)RoleMarkEnum.D1, (int)RoleMarkEnum.H1 };

            //Mark whether the bubble debuff is obtained
            uint bubbleDebuffId = 3743;
            uint fetterDebuffId = 3788;
            bool isMeGetBubbleDebuff =accessory.isMeGetStatus(bubbleDebuffId);

            //Mark whether the stack debuff is obtained
            uint hydrofallDebuffId = 3747;
            bool isMeGetHydrofallDebuff = accessory.isMeGetStatus(hydrofallDebuffId);
            bool isPartyGetHydrofallDebuff = accessory.whoGetStatusInParty(hydrofallDebuffId).Count > 0;

            //Mark whether to go to the northwest half to handle the mechanism
            bool isMeGoToNorthWest = false;

            //Crystal model ID
            uint springCrystalDataId1 = 16542;
            uint springCrystalDataId2 = 16549;
            IEnumerable<IGameObject> _crystals1 = accessory.GetEntitiesByDataId(springCrystalDataId1);
            IEnumerable<IGameObject> _crystals2 = accessory.GetEntitiesByDataId(springCrystalDataId2);
            IEnumerable<IGameObject> crystalsList = _crystals1.Union(_crystals2);

            //My destination
            Vector3 myEndPosition = new (0,0,0);
            

            if (isMeGetBubbleDebuff)
            {
                //I am bubble debuff, bubble players ignore stack/spread
                //Usually you can directly determine which position to go based on role, but to avoid accidents, find another player with the same debuff and their role
                //1. Find another bubble player, result may be null
                List<uint> bubbleDebuffPlayers = accessory.whoGetStatusInPartyWithoutMe(bubbleDebuffId);
                //2. Determine where to go
                if (bubbleDebuffPlayers.Count == 0)
                {
                    isMeGoToNorthWest = Array.IndexOf(roleMarkPriority, accessory.GetMyIndex()) <= 1;
                }
                else
                {
                    isMeGoToNorthWest = Array.IndexOf(roleMarkPriority, accessory.GetMyIndex())
                                        < Array.IndexOf(roleMarkPriority, accessory.GetIndexInParty(bubbleDebuffPlayers[0]));
                }
                
                /*
                3. Find the diagonal of the second-ma crystal (-1.57)
                For fusion method and north-south method, bubble goes to the diagonal of the second-ma crystal in their responsible half without question
                Usually the queue method's northwest group also follows the rule of going to the north half to find the second-ma crystal's diagonal
                There are two exceptions: when the near-center crystal is a vertical crystal located at the lower right or upper left
                */
                IEnumerable<IGameObject> springCrystals = crystalsList.Where(obj => Math.Abs(obj.Rotation) > 1);
                //Sort crystals by Z coordinate value
                springCrystals = isMeGoToNorthWest 
                        ? springCrystals.OrderBy(obj => obj.Position.Z)
                        : springCrystals.OrderByDescending(obj => obj.Position.Z);

                //Queue method related content, queue method exception: near-center crystal is a vertical crystal located at the lower right or upper left (coordinate product positive)
                bool isNanBeiFa = Boss1_Walkthrough != Boss1_WalkthroughEnum.QueueMethod;
                accessory.Log.Debug($"Boss 1 Spring Crystals Guide : isNanBeiFa => {isNanBeiFa}");
                // 25.2.15 In zero-difficulty, the vertical crystal's orientation may be 1.57 or -1.57, not just -1.57 like in normal difficulty
                // List<IGameObject> _centreCrystal = crystalsList.Where(obj => obj.Rotation < -1 && Math.Abs(obj.Position.X) < 6 && Math.Abs(obj.Position.Z) < 6).ToList();
                List<IGameObject> _centreCrystal = crystalsList.Where(obj => Math.Abs(obj.Rotation) > 1 && Math.Abs(obj.Position.X) < 6 && Math.Abs(obj.Position.Z) < 6).ToList();
                bool isNeedChange = !isNanBeiFa && _centreCrystal.Count > 0 && _centreCrystal[0].Position.X * _centreCrystal[0].Position.Z > 0;

                int _count = springCrystals.Count();
                Vector3 springCrystalPos = _count > 0 ? (springCrystals.ToList())[isNeedChange ? _count - 1 : 0].Position : new (0,0,0);
                Vector3 _centre = new (Math.Sign(springCrystalPos.X) * 10, springCrystalPos.Y, Math.Sign(springCrystalPos.Z) * 10);
                accessory.Log.Debug($"Boss 1 Spring Crystals Guide : _count => {_count}");
                myEndPosition = Util.RotatePointInFFXIVCoordinate(springCrystalPos,_centre,MathF.PI);
                if(!isNanBeiFa)
                {
                    //Since the queue method requires going to the first-ma position, but we found the second-ma diagonal position, we need to move 20 units along the X axis towards the Y axis
                    myEndPosition = new (myEndPosition.X - Math.Sign(myEndPosition.X) * 20 ,myEndPosition.Y,myEndPosition.Z);
                }

            }
            else
            {
                //Stop players' fusion method uses the queue method
                //I am not bubble debuff, usually it's stop debuff (or no buff)
                uint anotherOneId = 0;
                if (isPartyGetHydrofallDebuff)
                {
                    //I was assigned stack, find another stack player, result may be null
                    //Or I don't have stack but the party is stack, handle as stack

                    //Another player to compare priority with's party index

                    if (isMeGetHydrofallDebuff)
                    {
                        //I was assigned stack, find another stack player, result may be null
                        List<uint> hydrofallDebuffPlayers = accessory.whoGetStatusInPartyWithoutMe(hydrofallDebuffId);
                        anotherOneId = hydrofallDebuffPlayers.Count > 0 ? hydrofallDebuffPlayers[0] : anotherOneId;
                    }
                    else
                    {
                        //I don't have stack but the party is stack, handle as stack
                        //Find another player without stack buff, result may be null
                        List<uint> noHydrofallDebuffPlayers = accessory.whoNotGetStatusInPartyWithoutMe(hydrofallDebuffId);
                        anotherOneId = noHydrofallDebuffPlayers.Count > 0 ? noHydrofallDebuffPlayers[0] : anotherOneId;
                    }
                }
                else
                {
                    //Party has no stack debuff, and I am not bubble debuff
                    //Usually there will be stop buff, if not, can stand anywhere
                    List<uint> fetterDebuffPlayers = accessory.whoGetStatusInPartyWithoutMe(fetterDebuffId);
                    anotherOneId = fetterDebuffPlayers.Count > 0 ? fetterDebuffPlayers[0] : anotherOneId;
                }
                if (anotherOneId == 0)
                {
                    isMeGoToNorthWest = Array.IndexOf(roleMarkPriority, accessory.GetMyIndex()) <= 1;
                }
                else
                {
                    isMeGoToNorthWest = Array.IndexOf(roleMarkPriority, accessory.GetMyIndex())
                                    < Array.IndexOf(roleMarkPriority, accessory.GetIndexInParty(anotherOneId));
                }
                myEndPosition = isMeGoToNorthWest ? new(16,0,16) : new (-16,0,-16);
                bool isNanBeiFa = Boss1_Walkthrough == Boss1_WalkthroughEnum.NorthSouthMethod;
                accessory.Log.Debug($"Boss 1 Spring Crystals Guide : isNanBeiFa => {isNanBeiFa}");
                //Queue method stack goes to the safe zone in the first-ma area, spread goes to the safe zone in the second-ma area
                foreach(Vector2 safePoint in boss1_springCrystalsSafePoints){
                    //Through the safe zone, find the orientation of the crystal in that safe zone to determine whether it's first-ma or second-ma
                    IEnumerable<IGameObject> _springCrystals1 = accessory.GetEntitiesByDataId(springCrystalDataId1).Where(obj => Math.Sign(obj.Position.X) == Math.Sign(safePoint.X) && Math.Sign(obj.Position.Z) == Math.Sign(safePoint.Y)) ;
                    IEnumerable<IGameObject> _springCrystals2 = accessory.GetEntitiesByDataId(springCrystalDataId2).Where(obj => Math.Sign(obj.Position.X) == Math.Sign(safePoint.X) && Math.Sign(obj.Position.Z) == Math.Sign(safePoint.Y)) ;
                    IEnumerable<IGameObject> springCrystals = _springCrystals1.Union(_springCrystals2);
                    
                    if(springCrystals.Count() > 0 && (Math.Sign(Math.Round(Math.Abs((springCrystals.ToList())[0].Rotation))) == (isPartyGetHydrofallDebuff ? 0 : 1)))
                    {   
                        if(!isNanBeiFa)
                        {
                            //Fusion method or queue method
                            //Northwest player's point should be closer to -16,0,-16, if the current safe zone point is closer, record it
                            Vector3 startPoint = isMeGoToNorthWest ? new( -16, 0, -16 ) : new ( 16, 0, 16 );
                            myEndPosition = Math.Sqrt(Math.Pow(safePoint.X - startPoint.X, 2) + Math.Pow(safePoint.Y - startPoint.Z, 2))
                                            < Math.Sqrt(Math.Pow(myEndPosition.X - startPoint.X, 2) + Math.Pow(myEndPosition.Z - startPoint.Z, 2))
                                            ? new (safePoint.X,0,safePoint.Y)
                                            :myEndPosition;
                        }
                        else
                        {
                            //North-south method
                            //Meets north-south relative position
                            bool isAtCorrectHalf = Math.Sign(Math.Round(safePoint.Y)) == (isMeGoToNorthWest ? -1 : 1);
                            myEndPosition = isAtCorrectHalf ? new (safePoint.X,0,safePoint.Y) : myEndPosition;
                        }
                    }
                  
                }
                
            }
            accessory.Log.Debug($"Boss 1 Spring Crystals Guide : my order number in party => {1 + accessory.GetMyIndex()}");
            accessory.Log.Debug($"Boss 1 Spring Crystals Guide : isMeGetBubbleDebuff => {isMeGetBubbleDebuff}");
            accessory.Log.Debug($"Boss 1 Spring Crystals Guide : isPartyGetHydrofallDebuff => {isPartyGetHydrofallDebuff}");
            accessory.Log.Debug($"Boss 1 Spring Crystals Guide : isMeGetHydrofallDebuff => {isMeGetHydrofallDebuff}");
            accessory.Log.Debug($"Boss 1 Spring Crystals Guide : isMeGoToNorthWest => {isMeGoToNorthWest}");
            accessory.Log.Debug($"Boss 1 Spring Crystals Guide : myEndPosition => ( {myEndPosition.X},{myEndPosition.Y},{myEndPosition.Z} )");
            MultiDisDraw(new List<float[]> { new float[5] { myEndPosition.X, myEndPosition.Y, myEndPosition.Z, 0, 12000 } }, accessory);
        }



        //Determine bubble type for bubble blowing mechanism, also draw semicircle
        [ScriptMethod(name: "Boss 1 Small Bubble Type", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(16544|16551)$"], userControl: false)]
        public async void Boss1_BlowingBubblesType(Event @event, ScriptAccessory accessory)
        {
            //If a bubble at (-20,0,-17.5) appears, it's clockwise
            Vector3 bubblePos = @event.GetSourcePosition();
            lock (_lock){
                boss1_bubbleIsClockwise = boss1_bubbleIsClockwise || ((Math.Abs(bubblePos.X - (-20)) < 0.1) && (Math.Abs(bubblePos.Z - (-17.5)) < 0.1));
            }
        }

        [ScriptMethod(name: "Boss 1 Small Bubble Dangerous Zone", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(16544|16551)$"])]
        public async void Boss1_BlowingBubblesDangerousZone(Event @event, ScriptAccessory accessory)
        {
            /*
            I don't know why sometimes the Owner property doesn't work, and the semicircle is drawn at position (0,0,0)
            Possible reason: the newly added entity hasn't been synchronized to Objects yet
            2025.2.16 attempt:
            Added delay
            2025.2.20
            Added delay 2 - ineffective
            */
            DrawPropertiesEdit propFan = accessory.GetDrawPropertiesEdit(@event.GetSourceId(),new(2.55f),16000,false);
            propFan.Offset = new(0, 0, -1.3f);
            propFan.Radian = MathF.PI;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, propFan);

            DrawPropertiesEdit propDisplacement = accessory.GetDrawPropertiesEdit(propFan.Owner,new(2, 2.3f),propFan.DestoryAt,false);
            propDisplacement.Offset = new(0, 0, -1.3f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, propDisplacement);
        }
        
        /*
        Bubble blowing + stack/spread guidance
        There are two cases for bubble blowing, determined by checking the bubble closest to the lower right corner (20,0,20). If x<z, it's counterclockwise route. If x>z, it's clockwise route
        Everyone's starting point is in each quarter of the arena, the point near the center
        Three water circles (diameter 10), buff1 triggers when the third water circle is placed; after a short delay, for the first water circle of the next group, buff2 triggers when the second water circle is placed
        Boss1 after completing stack/spread buff assignment + summoning bubbles, the boss itself will cast Hydrobomb 35536(1.9s), using this cast as the starting point for bubble blowing + stack/spread guidance
        */
        [ScriptMethod(name: "Boss 1 Small Bubble + Stack/Spread Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35536|35489)$"])]
        public void Boss1_BlowingBubblesGuide(Event @event, ScriptAccessory accessory)
        {
            if(IsInSuppress(60000, nameof(Boss1_BlowingBubblesGuide)))
            {
                return;
            }
            accessory.Log.Debug($"Actived! => Boss 1 Blowing Bubbles Guide ");
            //Stack Debuff Id
            uint hydrofallDebuffId = 3747;
            uint hydrobombDebuffId = 3748;
            //Priority
            int[] roleMarkPriority = new int[] { (int)RoleMarkEnum.MT, (int)RoleMarkEnum.H1, (int)RoleMarkEnum.D1, (int)RoleMarkEnum.D2 };

            //Determine clockwise (horizontal after stack) or counterclockwise (vertical after stack)
            bool isClockwise = boss1_bubbleIsClockwise;
            //Find floating bubbles, floating bubble DataId is 16544

            //Determine whether stack first or spread first; whether I have the stack debuff
            bool isHydrofallFirst = false;
            bool isMeGetHydrofallDebuff = accessory.isMeGetStatus(hydrofallDebuffId);

            //Usually stack will target two DPS or (MT+H1), but when there are unexpected situations (like MT+D2 or H1+D1), need to swap D2 and D1 during stack (or MT and H1)
            bool isHydrofallDebuffNeedChange = false;
            List<uint> hydrofallDebuffPlayers = accessory.whoGetStatusInParty(hydrofallDebuffId);
            List<uint> hydrobombDebuffPlayers = accessory.whoGetStatusInParty(hydrobombDebuffId);

            float hydrofallDebuffTime = hydrofallDebuffPlayers.Count > 0 ? accessory.GetStatusInfo(hydrofallDebuffPlayers[0], hydrofallDebuffId).RemainingTime : -1;
            float hydrobombDebuffTime = hydrobombDebuffPlayers.Count > 0 ? accessory.GetStatusInfo(hydrobombDebuffPlayers[0], hydrobombDebuffId).RemainingTime : -1;
            isHydrofallFirst = hydrofallDebuffTime < hydrobombDebuffTime;
            isHydrofallDebuffNeedChange = (hydrofallDebuffPlayers.Count == 2) && (accessory.GetIndexInParty(hydrofallDebuffPlayers[0]) + accessory.GetIndexInParty(hydrofallDebuffPlayers[1]) == 3);


            //Design path
            //Taking the first quadrant of the FFXIV coordinate system as the pattern quadrant, assuming the starting point is (5f,0,5f)
            Vector2 startPoint_template = new(5f, 5f);
            Vector2 point2_template = isClockwise ? new(15f, 5f) : new(5f, 15f);
            Vector2 point3_template = new(15f, 15f);

            // Vector2 point4_template = new (0,0);
            // Vector2 point5_template = new (0,0);
            // Vector2 endPoint_template = new (0,0);

            //Stack first then spread, players going to spread need to move
            Vector2[] meGo = new Vector2[]{
                isClockwise ? new (1.5f,11.5f) : new (11.5f,1.5f),
                isClockwise ? new (0,6.5f) : new (6.5f,0),
                isClockwise ? new (-18.5f,8.5f) : new (8.5f,-18.5f),
            };
            Vector2[] meStay = new Vector2[]{
                isClockwise ? new (18f,18f) : new (18f,18f),
                isClockwise ? new (8.5f,19f) : new (19f,8.5f),
                new (9f,9f),
            };
            Vector2[] notHydrofallFirst = new Vector2[]{
                isClockwise ? new (6f,11f) :new (11f,6f),
                //isClockwise ? new (6f,10f) :new (10f,6f),
                new (6f,0),
                new (15f,0)
            };


            /* Design node paths
            Point 1, starting point; (wait for the first water circle to be placed)
            Point 2, starting point rotated Ï€/2 around the quarter arena (direction determined by isClockwise); (wait for the second water circle to be placed)
            Point 3, starting point rotated Ï€ around the quarter arena; (wait for the third water circle to be placed, the first buff triggers when the third water circle is placed)
            Point 4, a point 6m from the end point (wait for the fourth water circle to be placed) <= spread first then stack
            Point 4, the "midpoint" between point 3 and the end point moved 5m toward the arena center <= stack first then spread
            Point 5, end point (wait for the fifth water circle to be placed, the second buff triggers when the fifth water circle is placed)
            One more water circle remaining, just dodge it
            */
            int myPriority = Array.IndexOf(roleMarkPriority, accessory.GetMyIndex());

            //If there's an unexpected situation where two stack points go to the same group, need to swap the two DPS

            myPriority = (isHydrofallDebuffNeedChange && (myPriority == 2 || myPriority == 3)) ? (5 - myPriority) : myPriority;
            float radian = (float)(Math.PI * 0.5 * (myPriority - 2));

            Vector2 originPoint = new(0, 0);
            Vector2 myStartPoint = Util.RotatePoint(startPoint_template, radian);
            Vector2 myPoint2 = Util.RotatePoint(point2_template, radian);
            Vector2 myPoint3 = Util.RotatePoint(point3_template, radian);
            Vector2 myPoint4 = new(0, 0);
            Vector2 myPoint5 = new(0, 0);
            Vector2 myEndPoint = new(0, 0);

            if (!isHydrofallFirst)
            {
                // Spread first then stack
                myPoint4 = Util.RotatePoint(notHydrofallFirst[0], radian);
                myPoint5 = Util.RotatePoint(notHydrofallFirst[1], (myPriority == 0 || myPriority == 3) ?  Math.PI : 0);
                myEndPoint = Util.RotatePoint(notHydrofallFirst[2], (myPriority == 0 || myPriority == 3) ?  Math.PI : 0);
            }
            else
            {
                //Stack first then spread
                switch (myPriority)
                {
                    case 0:
                    case 2:
                        myPoint4 = Util.RotatePoint(meGo[0], radian);
                        myPoint5 = Util.RotatePoint(meGo[1], radian);
                        myEndPoint = Util.RotatePoint(meGo[2], radian);
                        break;
                    case 1:
                    case 3:
                        radian += (float)(0.5 * Math.PI);
                        myStartPoint = Util.RotatePoint(startPoint_template, radian);
                        myPoint2 = Util.RotatePoint(point2_template, radian);
                        myPoint3 = Util.RotatePoint(point3_template, radian);
                        myPoint4 = Util.RotatePoint(meStay[0], radian);
                        myPoint5 = Util.RotatePoint(meStay[1], radian);
                        myEndPoint = Util.RotatePoint(meStay[2], radian);
                        break;
                }

            }


            List<float[]> displacementsPointsList = new List<float[]>();
            displacementsPointsList.Add(new float[] { myStartPoint.X, 0, myStartPoint.Y, 0, 3600 });
            displacementsPointsList.Add(new float[] { myPoint2.X, 0, myPoint2.Y, 0, 2000 });
            displacementsPointsList.Add(new float[] { myPoint3.X, 0, myPoint3.Y, 0, 2000 });
            displacementsPointsList.Add(new float[] { myPoint4.X, 0, myPoint4.Y, 0, 3700 });
            displacementsPointsList.Add(new float[] { myPoint5.X, 0, myPoint5.Y, 0, 1800 });
            displacementsPointsList.Add(new float[] { myEndPoint.X, 0, myEndPoint.Y, 0, 2000 });

            //Fine-tune the time intervals for the last two guidance points when stack first then spread + meGo
            if (isHydrofallFirst && (myPriority == 2 || myPriority == 0))
            {
                displacementsPointsList[3][4] = 2400;
                displacementsPointsList[4][4] = 1300;
                displacementsPointsList[5][4] = 2500;
            }

            accessory.Log.Debug($"Boss 1 Blowing Bubbles Guide : my order number in party => {1 + accessory.GetMyIndex()}");
            accessory.Log.Debug($"Boss 1 Blowing Bubbles Guide : isClockwise => {isClockwise}");
            accessory.Log.Debug($"Boss 1 Blowing Bubbles Guide : isHydrofallFirst => {isHydrofallFirst}");
            accessory.Log.Debug($"Boss 1 Blowing Bubbles Guide : isMeGetHydrofallDebuff => {isMeGetHydrofallDebuff}");
            accessory.Log.Debug($"Boss 1 Blowing Bubbles Guide : isHydrofallDebuffNeedChange => {isHydrofallDebuffNeedChange}");
            accessory.Log.Debug($"Boss 1 Blowing Bubbles Guide : point1(start) => ({Math.Round(displacementsPointsList[0][0])},{Math.Round(displacementsPointsList[0][1])},{Math.Round(displacementsPointsList[0][2])})");
            accessory.Log.Debug($"Boss 1 Blowing Bubbles Guide : point2 => ({Math.Round(displacementsPointsList[1][0])},{Math.Round(displacementsPointsList[1][1])},{Math.Round(displacementsPointsList[1][2])})");
            accessory.Log.Debug($"Boss 1 Blowing Bubbles Guide : point3(first debuff) => ({Math.Round(displacementsPointsList[2][0])},{Math.Round(displacementsPointsList[2][1])},{Math.Round(displacementsPointsList[2][2])})");
            accessory.Log.Debug($"Boss 1 Blowing Bubbles Guide : point4 => ({Math.Round(displacementsPointsList[3][0])},{Math.Round(displacementsPointsList[3][1])},{Math.Round(displacementsPointsList[3][2])})");
            accessory.Log.Debug($"Boss 1 Blowing Bubbles Guide : point5 => ({Math.Round(displacementsPointsList[4][0])},{Math.Round(displacementsPointsList[4][1])},{Math.Round(displacementsPointsList[4][2])})");
            accessory.Log.Debug($"Boss 1 Blowing Bubbles Guide : point6(end) => ({Math.Round(displacementsPointsList[5][0])},{Math.Round(displacementsPointsList[5][1])},{Math.Round(displacementsPointsList[5][2])})");


            MultiDisDraw(displacementsPointsList,accessory);

        }




        [ScriptMethod(name: "Boss 1 Water Wall Bubble Type", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2013494", "Operate:Add"], userControl: false)]
        public void Boss1_TwintidesBubbleType(Event @event, ScriptAccessory accessory)
        {
            Vector3 bubblePos = @event.GetSourcePosition();
            if (((Math.Abs(bubblePos.Z - (-20)) < 0.1) && Math.Abs(Math.Abs(bubblePos.X) - 5) < 0.1)
                ||((Math.Abs(bubblePos.X - (-20)) < 0.1) && Math.Abs(Math.Abs(bubblePos.Z) - 5) < 0.1))
            {
                //North-south situation
                //Store bubbles with Z coordinate -20 and X coordinate 5 or -5 into boss1_twintidesBubbleType

                //East-west situation
                //Store bubbles with X coordinate -20 and Z coordinate 5 or -5 into boss1_twintidesBubbleType


                boss1_twintidesBubbleType = bubblePos;
                //The final stored one should be the later added bubble
            }
        }
        //Mark the danger zone of the water wall for circle/donut
        [ScriptMethod(name: "Boss 1 Circle/Donut Water Wall Danger Zone", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:2013494", "Operate:Add"])]
        public async void Boss1_TwintidesBubbleDangerousZone(Event @event, ScriptAccessory accessory)
        {
            DrawPropertiesEdit propRect = accessory.GetDrawPropertiesEdit(@event.GetSourceId(),new(10, 20),6600-1400,false);
            propRect.Delay = 4000 + 1400;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, propRect);
            // accessory.Log.Debug($"Actived! => Boss 1 Twintides Bubble Type");
        }


        /*
        Surge Tide 35532(5.0s) Ebb Tide 35534(5.0s)
        Boss1 circle/donut + stack guidance
        MT D2 D1 H2, assigned to A or C according to priority
        Need to determine whether circle first or donut first
        Need to determine whether left through right or right through left
        Three points
        1. Starting point, dodge the first circle/donut + first water wall
        2. Turning point, vertical movement to dodge the second circle/donut
        3. End point, horizontal movement to dodge the second water wall
        */
        [ScriptMethod(name: "Boss 1 Circle/Donut Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35532|35534|35559|35561)$"])]
        public void Boss1_TwintidesGuide(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"Actived! => Boss 1 Twintides Guide");

            //Is circle first?
            int recedingTwintidesActionId1 = 35532;
            int recedingTwintidesActionId2 = 35559;
            bool isRecedingTwintides = (@event.GetActionId() == recedingTwintidesActionId1) || (@event.GetActionId() == recedingTwintidesActionId2);


            //Determine water wall type + which side first
            bool isVerticalWater = Math.Abs(boss1_twintidesBubbleType.Z - (-20)) < 0.1 ;
            bool isNorth_leftWaterFirst = isVerticalWater ? boss1_twintidesBubbleType.X > 4 : boss1_twintidesBubbleType.Z < -4;

            //Stack Debuff Id
            uint hydrofallDebuffId = 3747;


            int[] roleMarkPriority = new int[] { (int)RoleMarkEnum.MT, (int)RoleMarkEnum.D2, (int)RoleMarkEnum.D1, (int)RoleMarkEnum.H1 };
            //1, Determine whether I should go to A or C
            //2, Determine the starting point's X coordinate
            //3, Determine the absolute value of the starting point's Z coordinate as 15 or 5

            //I am stack target
            bool isMeGoToNorth = false;
            bool isMeGetHydrofallDebuff = accessory.isMeGetStatus(hydrofallDebuffId);
            uint anotherOneId = 0;

            List<uint> players = isMeGetHydrofallDebuff 
                                    ?accessory.whoGetStatusInPartyWithoutMe(hydrofallDebuffId)
                                    :accessory.whoNotGetStatusInPartyWithoutMe(hydrofallDebuffId);

            anotherOneId = players.Count > 0 ? players[0] : anotherOneId;
        
            isMeGoToNorth = Array.IndexOf(roleMarkPriority, accessory.GetMyIndex())
                            < (anotherOneId == 0 ? 2 : Array.IndexOf(roleMarkPriority,accessory.GetIndexInParty(anotherOneId)));



            //Assuming the pattern's starting point is on the C side
            Vector2 originPoint = new(0, 0);
            Vector2 startPoint_template = new((isNorth_leftWaterFirst?1:-1) * (-2), isRecedingTwintides ? 15f : 5f);
            Vector2 point2_template = new((isNorth_leftWaterFirst?1:-1) * (-1), 10f);
            Vector2 endPoint_template = new((isNorth_leftWaterFirst?1:-1) * 2, isRecedingTwintides ? 5f : 15f);


            Vector2 mySatrtPoint = Util.RotatePoint(startPoint_template, isMeGoToNorth ? Math.PI : 0);
            Vector2 myPoint2 = Util.RotatePoint(point2_template, isMeGoToNorth ? Math.PI : 0);
            Vector2 myEndPoint = Util.RotatePoint(endPoint_template, isMeGoToNorth ? Math.PI : 0);

            //If it's east-west side, rotate -0.5Ï€
            if(!isVerticalWater)
            {
                mySatrtPoint = Util.RotatePoint(mySatrtPoint,-0.5 * Math.PI);
                myPoint2 = Util.RotatePoint(myPoint2,-0.5 * Math.PI);
                myEndPoint = Util.RotatePoint(myEndPoint,-0.5 * Math.PI);
            }

            List<float[]> displacementsPointsList = new List<float[]>();
            displacementsPointsList.Add(new float[] { mySatrtPoint.X, 0, mySatrtPoint.Y, 0, 4600 });
            displacementsPointsList.Add(new float[] { myPoint2.X, 0, myPoint2.Y, 0, 1000 });
            displacementsPointsList.Add(new float[] { myEndPoint.X, 0, myEndPoint.Y, 0, 2000 });
            MultiDisDraw(displacementsPointsList, accessory);
            
            //Draw danger zone
            DrawPropertiesEdit dpCircle = accessory.GetDrawPropertiesEdit(@event.GetSourceId(), new (14f), 1000, false);
            DrawPropertiesEdit dpDonut = accessory.GetDrawPropertiesEdit(@event.GetSourceId(), new (30f), 1000, false);
            dpDonut.InnerScale = new (7f);
            dpDonut.Radian = 2 * MathF.PI;
            if(isRecedingTwintides){
                //Circle first
                dpCircle.DestoryAt = 4300;
                dpDonut.Delay = 5000;
                dpDonut.DestoryAt = 2400;
            }else{
                //Donut first
                dpDonut.DestoryAt = 4300;
                dpCircle.Delay = 5000;
                dpCircle.DestoryAt = 2400;
            }
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dpCircle);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dpDonut);


            accessory.Log.Debug($"Boss 1 Twintides Guide : my order number in party => {1 + accessory.GetMyIndex()}");
            accessory.Log.Debug($"Boss 1 Twintides Guide : isRecedingTwintides => {isRecedingTwintides}");
            accessory.Log.Debug($"Boss 1 Twintides Guide : isNorth_leftWaterFirst => {isNorth_leftWaterFirst}");
            accessory.Log.Debug($"Boss 1 Twintides Guide : isMeGoToNorth => {isMeGoToNorth}");
            accessory.Log.Debug($"Boss 1 Twintides Guide : isMeGetHydrofallDebuff => {isMeGetHydrofallDebuff}");
        }


        /*
        Second water crystal + mob guide
        After Boss1 summons crystals and mobs, it will cast Predation Bubble Net 35525(3.8s). This cast is special with a different ID from the previous two Predation Bubble Nets. After a short delay, two players and two mobs are given bubble debuff.
        According to priority, the stop debuff player guides the bubble mob, and the bubble debuff player guides the unbuffed mob
        (Or use Roar 35524 as the start marker)
        */
        [ScriptMethod(name: "Boss 1 Mob Facing Guide", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35525|35575)$"])]
        public async void Boss1_MobGuide(Event @event, ScriptAccessory accessory)
        {
            //Boss1 will cast Predation Bubble Net multiple times throughout the battle, need to mark it
            //++boss1_bubbleNetCastingCount != 2 || 
            if(await DelayMillisecond(5500+500)){
                //Short delay to ensure debuff application is complete
                //2025.2.8 Extended to 6000, 5500 was too marginal, sometimes the mob hadn't completed buff data update
                return;
            }
            boss1_phaseAfterMob = true;
            accessory.Log.Debug($"Actived! => Boss 1 Mob Guide");

            uint bubbleDebuffId = 3743;
            uint fetterDebuffId = 3788;
            int[] roleMarkPriority = new int[] { (int)RoleMarkEnum.MT, (int)RoleMarkEnum.H1, (int)RoleMarkEnum.D1, (int)RoleMarkEnum.D2 };


            //1. Check my debuff situation and compare priority level
            bool isMeGetBubbleDebuff = accessory.isMeGetStatus(bubbleDebuffId);
            bool isMeGetFetterDebuff = accessory.isMeGetStatus(fetterDebuffId);
            uint myDebuffId = isMeGetBubbleDebuff ? bubbleDebuffId : (isMeGetFetterDebuff ? fetterDebuffId : 0);


            List<uint> players = new List<uint>();
            if(myDebuffId == 0){
                players = accessory.whoNotGetStatusInPartyWithoutMe(bubbleDebuffId).Intersect(
                    accessory.whoNotGetStatusInPartyWithoutMe(fetterDebuffId)).ToList();
            }else{
                players = accessory.whoGetStatusInPartyWithoutMe(myDebuffId);
            }
            uint anotherOneId = players.Count > 0 ? players[0] : 0 ;
            int myPriority = Array.IndexOf(roleMarkPriority, accessory.GetMyIndex());
            bool isMeFirst = (anotherOneId != 0)
                                ? myPriority < Array.IndexOf(roleMarkPriority, accessory.GetIndexInParty(anotherOneId))
                                : false;

            //2. Check mob debuff situation, sort by rotation angle - 0.5Ï€ is the first mob DataId 16545, mob bubble debuff Id 3745
            uint mobDataId1 = 16545;
            uint mobDataId2 = 16552;
            uint bubbleDebuffIdOnMob = 3745;
            IEnumerable<IGameObject> _mobs1 = accessory.GetEntitiesByDataId(mobDataId1);
            IEnumerable<IGameObject> _mobs2 = accessory.GetEntitiesByDataId(mobDataId2);
            IEnumerable<IGameObject> mobs = _mobs1.Union(_mobs2);
            List<uint> mobIdsGetbubbleDebuff = accessory.whoGetStatus(bubbleDebuffIdOnMob);
            IEnumerable<IGameObject> mobsGetbubbleDebuff = mobs.Where(obj => obj is IGameObject gameObject && mobIdsGetbubbleDebuff.Contains(gameObject.EntityId));

            List<Vector3> mobsIndex = myDebuffId == bubbleDebuffId 
                                        ? mobs.Except(mobsGetbubbleDebuff).Select(obj => (obj?.Position) ?? new (0,1,0)).ToList()
                                        : mobsGetbubbleDebuff.Select(obj => (obj?.Position) ?? new (0,1,0)).ToList();
            mobsIndex = mobsIndex.OrderBy(v3 =>
                {
                    float angle = MathF.Atan2(v3.Z, v3.X) + 0.6f * MathF.PI;
                    return angle < 0 ? angle + 2 * MathF.PI : angle;
                }  
            ).ToList();
            //3. Determine which mob I should guide, and which number point to go to for spread
            accessory.Log.Debug($"Boss 1 Mob Guide : mobsIndex.Count => {mobsIndex.Count}");
            Vector3 myMob = mobsIndex.Count > 0 
                            ? (isMeFirst ? mobsIndex[0] : mobsIndex[mobsIndex.Count -1])
                            : new (0,0,0) ;

            //4. Find crystal position, calculate safe zone type
            //Using rad = 0 position as safe point template
            Vector2 startPoint_template = new (10,10);
            //Safe zone type 0, not detected; 1 left-right, 2 up-down, 4 four corners
            int safeZoneType = 0;
            //Crystal model ID
            uint springCrystalDataId1 = 16542;
            uint springCrystalDataId2 = 16549;
            IEnumerable<IGameObject> _crystalList1 = accessory.GetEntitiesByDataId(springCrystalDataId1)
                                                .Where(obj => obj is IGameObject gameObject && gameObject.Position.X > 0 && gameObject.Position.Z < 0);
            IEnumerable<IGameObject> _crystalList2 = accessory.GetEntitiesByDataId(springCrystalDataId2)
                                                .Where(obj => obj is IGameObject gameObject && gameObject.Position.X > 0 && gameObject.Position.Z < 0);
                                                
                                                
            List<IGameObject> crystalList = _crystalList1.Union(_crystalList2).ToList();
            if(crystalList.Count > 0)
            {
                if(Math.Abs(crystalList[0].Rotation) > 1 )
                {
                    //Top-right crystal is vertical, left-right safe zone
                    startPoint_template = new (11f, 9f);
                    safeZoneType = 1;
                }
                else
                {
                    //Top-right crystal is horizontal
                    //X coordinate < 10 four-corner safe zone; otherwise up-down safe zone
                    startPoint_template = crystalList[0].Position.X < 10 ? new (11f, 11f) : new (9f, 11f);
                    safeZoneType = crystalList[0].Position.X < 10 ? 4 : 2;
                }
            }

            Vector2 myStartPoint = startPoint_template;
            if(myMob.X < - 5)
            {
                myStartPoint  = Util.RotatePoint(startPoint_template, Math.PI);
            }
            else if(myMob.X < 5 && myMob.X >-5)
            {
                myStartPoint = myMob.Z > 0 ? Util.AxisymmetricPoint(startPoint_template,0.5 * Math.PI) : Util.AxisymmetricPoint(startPoint_template,0);
            }
            List<float[]> pointsList = new List<float[]>();
            pointsList.Add(new float[]{myStartPoint.X,myMob.Y,myStartPoint.Y,0,11100});
            pointsList.Add(new float[]{1.15f * myMob.X,myMob.Y,1.20f * myMob.Z,0,4000});
            MultiDisDraw(pointsList,accessory);

            //Draw mob cone guide
            foreach(IGameObject? mobObject in mobs)
            {
                if(mobObject is IGameObject mobIGameObject){
                    DrawPropertiesEdit dpFan = accessory.GetDrawPropertiesEdit(mobIGameObject.EntityId, new(4f), 8300 - 900, false);
                    dpFan.Delay = 10000 + 900;
                    dpFan.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dpFan.Radian = MathF.PI;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dpFan);
                }
            }
            //Draw crystal safe zone
            Vector4 safeZoneColor = accessory.Data.DefaultSafeColor.WithW(0.3f);
            switch(safeZoneType)
            {
                case 1 :
                    accessory.FastDraw(DrawTypeEnum.Rect,new Vector3(-15,0,-10),new (10,20),new (0,10500),safeZoneColor);
                    accessory.FastDraw(DrawTypeEnum.Rect,new Vector3(15,0,-10),new (10,20),new (0,10500),safeZoneColor);
                    break;
                case 2 : 
                    accessory.FastDraw(DrawTypeEnum.Rect,new Vector3(0,0,-20),new (20,10),new (0,10500),safeZoneColor);
                    accessory.FastDraw(DrawTypeEnum.Rect,new Vector3(0,0,10),new (20,10),new (0,10500),safeZoneColor);                  
                    break;
                case 4 : 
                    accessory.FastDraw(DrawTypeEnum.Rect,new Vector3(-15,0,-20),new (10,10),new (0,10500),safeZoneColor);
                    accessory.FastDraw(DrawTypeEnum.Rect,new Vector3(-15,0,10),new (10,10),new (0,10500),safeZoneColor);
                    accessory.FastDraw(DrawTypeEnum.Rect,new Vector3(15,0,-20),new (10,10),new (0,10500),safeZoneColor);
                    accessory.FastDraw(DrawTypeEnum.Rect,new Vector3(15,0,10),new (10,10),new (0,10500),safeZoneColor);                    
                    break;
                case 0 : 
                default:
                    break;
            }

            accessory.Log.Debug($"Boss 1 Second Spring Crystals Safe Zone Draw : my order number in party => {1 + accessory.GetMyIndex()}");
            accessory.Log.Debug($"Boss 1 Second Spring Crystals Safe Zone Draw : isMeGetBubbleDebuff => {isMeGetBubbleDebuff}");
            accessory.Log.Debug($"Boss 1 Second Spring Crystals Safe Zone Draw : isMeGetFetterDebuff => {isMeGetFetterDebuff}");
            accessory.Log.Debug($"Boss 1 Second Spring Crystals Safe Zone Draw : isMeFirst(high priority) => {isMeFirst}");
            accessory.Log.Debug($"Boss 1 Second Spring Crystals Safe Zone Draw : safeZoneType(1â†â†’,2â†‘â†“) => {safeZoneType}");
            accessory.Log.Debug($"Boss 1 Second Spring Crystals Safe Zone Draw : myMob => {myMob}");
        }

        /*
        Moses' Sea method: after opening the sea, it will cast Predation Bubble Net to summon bubbles, using this cast as the start marker.
        4 crystals will spawn on one side of the arena (round crystal dataId 16541), 2 closer to the Y axis, 2 farther from the Y axis with absolute X coordinate 15. This side is called Side A
        Among the 2 closer to the Y axis, one has Z coordinate 0, and on the opposite side of the other crystal, there will be a bubble that a DPS on Side A needs to enter, called Bubble B
        Draw danger circles for small bubbles and crystals
        4 towers spawn, one tower on Side A, the other three towers on the opposite side of Side A

        Assuming Side A is the east side of the arena, then the towers (radius 4) are on the west side: (-10,-15),(-10,15),(-14,0); east side (14,10)
        MT's behavior (-19,0)=>(-14,0), H1's behavior (6,0)=>(14,0)
        D1 enters Bubble B
        D2's starting point and Bubble B's Z coordinate sign are related, this sign is called C (value 1 or -1);
        D2 behavior (-19, -C* 15)=>(-10, -C* 15)
        There is also a subsequent water wall + circle/donut stack/spread
        */

        /*
        Raging Seas 35520 35523 35553 casts, split the arena, mark the endpoint and danger range for stack/spread,
        TODO Reconfigure the start time point. If using Raging Seas as the starting point, will the time to guide to the safe zone be insufficient?
        Use the first buff application as the judgment timing point and draw. Since there are multiple buff applications, introduce the boss1_phaseAfterMob parameter as a flag to enable this trigger.
        */
        [ScriptMethod(name: "Boss 1 Raging Seas Hydrop Guide", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3747|3748)$"])]
        public async void Boss1_AngrySeasGuideFirst(Event @event, ScriptAccessory accessory)
        {
            if(IsInSuppress(5000, nameof(Boss1_AngrySeasGuideFirst)) || !boss1_phaseAfterMob)
            {
                return;
            }
            if(await DelayMillisecond(3000)){
                return;
            }
            accessory.Log.Debug($"Actived! => Boss 1 Angry Seas Hydrop Guide");
            //Stack/Spread DebuffId
            boss1_phaseAfterMob = false;
            uint hydrofallDebuffId = 3747;
            uint hydrobombDebuffId = 3748;
            bool ishydrofallFirst = @event.GetStatusID() == hydrofallDebuffId;
            bool ishydrofallSameGroup = false;
            List<uint> hydrofallDebuffPlayers = accessory.whoGetStatusInParty(hydrofallDebuffId);
            ishydrofallSameGroup = hydrofallDebuffPlayers.Count == 2 && (accessory.GetIndexInParty(hydrofallDebuffPlayers[0]) + accessory.GetIndexInParty(hydrofallDebuffPlayers[1]) == 3);

            //Determine what the first buff after the 4 mobs is, then you can determine whether it's stack or spread
            //If the stack buff points to the same group, swap the two DPS


            // Stack positions MT,H1,D1,D2
            Vector2[] hydrofallDisperse = new Vector2[] {
                new (-10,0),
                new (10,0),
                ishydrofallSameGroup ?new (-10,0) :new (10,0),
                ishydrofallSameGroup ?new (10,0) :new (-10,0),
            };

            //Spread positions MT,H1,D1,D2
            Vector2[] hydrobombDisperse = new Vector2[] {
                new (-7,-7),
                new (15,-15),
                ishydrofallSameGroup ?new (-15,15) :new (7,7),
                ishydrofallSameGroup ?new (7,7) :new (-15,15),
            };

            long delay_firstMech = 0;
            long dispaly_firstMech = 8000;
            long delay_secondMech = 0;
            long dispaly_secondMech = 5000;

            Vector2 myStartPoint = ishydrofallFirst ? hydrofallDisperse[accessory.GetMyIndex()] : hydrobombDisperse[accessory.GetMyIndex()] ;
            Vector2 myEndPoint = ishydrofallFirst ? hydrobombDisperse[accessory.GetMyIndex()] : hydrofallDisperse[accessory.GetMyIndex()] ;


            //Draw guidance
            List<float[]> pointsList = new List<float[]>();
            pointsList.Add(new float[]{myStartPoint.X, 0 , myStartPoint.Y , delay_firstMech ,dispaly_firstMech});
            pointsList.Add(new float[]{myEndPoint.X, 0 , myEndPoint.Y , delay_secondMech ,dispaly_secondMech});
            MultiDisDraw(pointsList,accessory);


            accessory.Log.Debug($"Boss 1 Angry Seas Hydrop Guide : my order number in party => {1 + accessory.GetMyIndex()}");
            accessory.Log.Debug($"Boss 1 Angry Seas Hydrop Guide : ishydrofallFirst => {ishydrofallFirst}");
            accessory.Log.Debug($"Boss 1 Angry Seas Hydrop Guide : ishydrofallSameGroup=> {ishydrofallSameGroup}");
            accessory.Log.Debug($"Boss 1 Angry Seas Hydrop Guide : myStartPoint => {myStartPoint}");
            accessory.Log.Debug($"Boss 1 Angry Seas Hydrop Guide : myEndPoint => {myEndPoint}");
        }

        [ScriptMethod(name: "Boss 1 Angry Seas Crystal Tower Guidance", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(16541|16548)$"])]
        public void Boss1_AngrySeasGuideSecond(Event @event, ScriptAccessory accessory)
        {
            Vector3 pos = @event.GetSourcePosition();
            bool isNearY = Math.Abs(pos.X) < 12;
            bool isAwayX = Math.Abs(pos.Z) > 10;
            if(!isNearY || !isAwayX)
            {
                return;
            }
            accessory.Log.Debug($"Actived! => Boss 1 Angry Seas Crystals Guide");
            bool isCrystalOnEastSide = pos.X > 0;
            bool isCrystalOnNorthSide = pos.Z < 0;

            Vector3 crystalStay1 = new (isCrystalOnEastSide?15:-15,pos.Y,isCrystalOnNorthSide?-5:5);
            Vector3 crystalStay2 = new (isCrystalOnEastSide?15:-15,pos.Y,isCrystalOnNorthSide?15:-15);
            Vector3 crystalMove1 = new (isCrystalOnEastSide?-10:10,pos.Y,isCrystalOnNorthSide?-15:15);
            Vector3 crystalMove2 = new (isCrystalOnEastSide?-10:10,pos.Y,0);

            Vector3 startPoint_MT = new (isCrystalOnEastSide?-19:-8,pos.Y,0);
            Vector3 endPoint_MT = new (-14,pos.Y,0);

            Vector3 startPoint_H1 = new (isCrystalOnEastSide?8:19,pos.Y,0);
            Vector3 endPoint_H1 = new (14,pos.Y,0);

            Vector3 startPoint_D1 = isCrystalOnEastSide 
                            ? isCrystalOnNorthSide ? new (7.5f,pos.Y,12.5f) : new (7.5f,pos.Y,-12.5f)
                            : isCrystalOnNorthSide ? new (19,pos.Y,-15) : new (19,pos.Y,15);
            Vector3 endPoint_D1 = isCrystalOnEastSide
                            ? isCrystalOnNorthSide ? new (7.5f,pos.Y,12.5f) : new (7.5f,pos.Y,-12.5f)
                            : isCrystalOnNorthSide ? new (13,pos.Y,-15) : new (13,pos.Y,15);

            Vector3 startPoint_D2 = isCrystalOnEastSide
                            ? isCrystalOnNorthSide ? new (-19,pos.Y,-15) : new (-19,pos.Y,15)
                            : isCrystalOnNorthSide ? new (-7.5f,pos.Y,12.5f) : new (-7.5f,pos.Y,-12.5f);
            Vector3 endPoint_D2 = isCrystalOnEastSide
                            ? isCrystalOnNorthSide ? new (-13,pos.Y,-15) : new (-13,pos.Y,15)
                            : isCrystalOnNorthSide ? new (-7.5f,pos.Y,12.5f) : new (-7.5f,pos.Y,-12.5f);

            List<Vector3> startPoints = new List<Vector3>();
            startPoints.Add(startPoint_MT);
            startPoints.Add(startPoint_H1);
            startPoints.Add(startPoint_D1);
            startPoints.Add(startPoint_D2);

            List<Vector3> endPoints = new List<Vector3>();
            endPoints.Add(endPoint_MT);
            endPoints.Add(endPoint_H1);
            endPoints.Add(endPoint_D1);
            endPoints.Add(endPoint_D2);

            Vector3 myStartPoint = startPoints[accessory.GetMyIndex()];
            Vector3 myEndPoint = endPoints[accessory.GetMyIndex()];


            //Drawing part
            List<float[]> pointsList = new List<float[]>();
            pointsList.Add(new float[] {myStartPoint.X,myStartPoint.Y,myStartPoint.Z,5000,18000});
            pointsList.Add(new float[] {myEndPoint.X,myEndPoint.Y,myEndPoint.Z,0,3000});
            if(Math.Abs(myEndPoint.X)-7.5f < 0.2f){
                pointsList[0][4] = 14000;
                pointsList.RemoveAt(pointsList.Count - 1);
            }
            MultiDisDraw(pointsList,accessory);

            //Draw danger zone
            accessory.FastDraw(DrawTypeEnum.Circle,crystalStay1,new(8),new(13000,10100),false);
            accessory.FastDraw(DrawTypeEnum.Circle,crystalStay2,new(8),new(13000,10100),false);
            accessory.FastDraw(DrawTypeEnum.Circle,crystalMove1,new(8),new(13000,10100),false);
            accessory.FastDraw(DrawTypeEnum.Circle,crystalMove2,new(8),new(13000,10100),false);

            accessory.Log.Debug($"Boss 1 Angry Seas Crystals Guide : my order number in party => {1 + accessory.GetMyIndex()}");
            accessory.Log.Debug($"Boss 1 Angry Seas Crystals Guide : isCrystalOnEastSide => {isCrystalOnEastSide}");
            accessory.Log.Debug($"Boss 1 Angry Seas Crystals Guide : isCrystalOnNorthSide => {isCrystalOnNorthSide}");
            accessory.Log.Debug($"Boss 1 Angry Seas Crystals Guide : myStartPoint => {myStartPoint}");
            accessory.Log.Debug($"Boss 1 Angry Seas Crystals Guide : myEndPoint => {myEndPoint}");
        }
        #endregion

        #region Mob2
        [ScriptMethod(name: "Mob 2 Stop Tornado", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35917|35795)$"])]
        public void Mob2_StopTornado(Event @event, ScriptAccessory accessory)
        {
            accessory.FastDraw(DrawTypeEnum.Circle,@event.GetTargetId(),new (4),new(0,4600),false);
        }

        [ScriptMethod(name: "Mob 2 Ovation", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35777|35796)$"])]
        public void Mob2_Ovation(Event @event, ScriptAccessory accessory)
        {
            accessory.FastDraw(DrawTypeEnum.Rect,@event.GetSourceId(),new (4,12),new(0,3900),false);
        }

        [ScriptMethod(name: "Mob 2 Isle Drop", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35951|35900)$"])]
        public void Mob2_IsleDrop(Event @event, ScriptAccessory accessory)
        {
            accessory.FastDraw(DrawTypeEnum.Circle,@event.GetEffectPosition(),new (6),new(0,4900),false);
        }
        #endregion 

        #region Boss2
        // Boss2 arena center coordinates are (200,-300,0), one grid side length is 8

        /*
        Front unanalyzed StatusID3726 StackCount151 Param663
        Back unanalyzed StatusID3727 StackCount152 Param664
        Right unanalyzed StatusID3728 StackCount153 Param665
        Left unanalyzed StatusID3729 StackCount154 Param666
        

        Boss's 3x rotation angle StatusID3938
        Boss's 5x rotation angle StatusID3939

        3x rotation angle on player StatusID3721
        5x rotation angle on player StatusID3790

        Light orb DataId 16448
        Yellow arrow DataId 2013505
        White arrow DataId 2013506

        Get TargetIcon Id
        On player
        Clockwise rotation 01ED
        Counterclockwise rotation 01EE
        X icon 01F8
        âˆš icon 01F7

        On boss
        Clockwise rotation 01E4
        Counterclockwise rotation 01E5
        
        */
        [ScriptMethod(name: "Boss 2 Inferno Theorem", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(34990|35845)$"], userControl: false)]
        public void Boss2_InfernoTheorem(Event @event, ScriptAccessory accessory)
        {
            if(IsInSuppress(10000, nameof(Boss2_InfernoTheorem)))
            {
                return;
            }
            boss2_bossId = @event.GetSourceId();
            boss2_InfernoTheoremCasted =true;
            boss2_InfernoTheoremCastingCount++;
            boss2_ArcaneMinesList.Clear();
            accessory.Log.Debug($"Actived! => Boss 2 Id updated {boss2_bossId}");
        }


        //Rune Cannon safety detection requires obtaining boss cast + boss's target type simultaneously
        //â†‘Not necessary to detect like this, boss's Rune Cannon cast is 34955-34958; and there is an invisible clone cast 34959, the back of that cast unit's facing is the safe zone
        [ScriptMethod(name: "Boss 2 Rune Blight Safe Zone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(34959|35814)$"])]
        public void Boss2_ArcaneBlightSafeZone(Event @event, ScriptAccessory accessory)
        {
            if(@event.GetSourceId() == boss2_bossId)
            {
                return;
            }
            float finalAngle = @event.GetSourceRotation() - MathF.PI;
            DrawPropertiesEdit dpFan = accessory.GetDrawPropertiesEdit(@event.GetSourceId(), new(20), 4500, true);
            dpFan.Radian = 0.5f * MathF.PI;
            dpFan.Rotation = - MathF.PI;
            dpFan.Color = accessory.Data.DefaultSafeColor.WithW(2.0f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dpFan);
        }

        [ScriptMethod(name: "Boss 2 Double Light Orb Guide", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3726|3727|3728|3729)$"])]
        public async void Boss2_doubleArcaneGlobeGuide(Event @event, ScriptAccessory accessory)
        {
            //After 7 seconds, yellow and white arrows and light orbs refresh
            if(@event.GetTargetId() != accessory.Data.Me || !boss2_InfernoTheoremCasted || await DelayMillisecond(7000))
            {
                return;
            }
            accessory.Log.Debug($"Actived! => Boss 2 double arcane globe guide");
            boss2_InfernoTheoremCasted = false;
            /*
            How many degrees to rotate to align the gap target (for handling light orbs)
            Front unanalyzed StatusID3726 => 0
            Back unanalyzed StatusID3727 => PI
            Right unanalyzed StatusID3728 => -0.5PI
            Left unanalyzed StatusID3729 => 0.5PI

            There are three total guidance sessions for the unanalyzed status on players, first two are light orbs, third is high-precision light bomb under rotation angle
            */
            List<float> baseAngles = new List<float>();
            baseAngles.Add(0);
            baseAngles.Add(MathF.PI);
            baseAngles.Add(-0.5f * MathF.PI);
            baseAngles.Add(0.5f * MathF.PI);
            float myBaseAngle = baseAngles[(int)@event.GetStatusID()-3726];


            Vector3 arcaneGlobePos1 = new (0,0,0);
            Vector3 arcaneGlobePos2 = new (0,0,0);


            //Obtain yellow arrow entity

            uint yellowArrowDataId = 2013505;
            IEnumerable<IGameObject> yellowArrows = accessory.GetEntitiesByDataId(yellowArrowDataId);
            uint ballDataId1 = 16448;
            //Zero difficulty is 16606
            uint ballDataId2 = 16606;
            IEnumerable<IGameObject> _balls = accessory.GetEntitiesByDataId(ballDataId1).Union(accessory.GetEntitiesByDataId(ballDataId2));
            List<IGameObject> balls = _balls.Where(obj => obj != null).Select(obj => (IGameObject)obj).ToList();

            foreach(IGameObject? obj in yellowArrows)
            {
                if(obj is IGameObject gameObject)
                {
                    Vector3 startPoint = gameObject.Position;
                    float rot = gameObject.Rotation;
                    Vector3 nextPoint = new (startPoint.X + MathF.Sin(rot) * 8f, startPoint.Y , startPoint.Z + MathF.Cos(rot) * 8f);
                    //If a light orb is found within 2 units of nextPoint, then that light orb is the first to explode
                    foreach(IGameObject ballObject in balls)
                    {
                        Vector3 ballPos = ballObject.Position;
                        if(Math.Sqrt(MathF.Pow(nextPoint.X - ballPos.X ,2) + MathF.Pow(nextPoint.Z - ballPos.Z ,2)) < 2)
                        {
                            //Locate the first light orb
                            arcaneGlobePos1 = ballPos;
                            List<IGameObject> anotherBall = balls.Where(obj => obj != ballObject).ToList();
                            if(anotherBall.Count > 0)
                            {
                                arcaneGlobePos2 = anotherBall[0].Position;
                            }
                            break;

                        }
                    }

                }
            }

            Vector4 color = GuideColor_GoNow.V4;
            accessory.DrawTurnTowards(arcaneGlobePos1,new(20,0.33f*MathF.PI,myBaseAngle),new(9,5f),new(0,5500),color.WithW(color.W + Boss2_PizzaColorDeepen),true);
            accessory.DrawTurnTowards(arcaneGlobePos2,new(20,0.33f*MathF.PI,myBaseAngle),new(9,5f),new(9500,5500),color.WithW(color.W + Boss2_PizzaColorDeepen),true);
            
            accessory.Log.Debug($"Boss 2 double arcane globe guide: arcaneGlobePos1 => {arcaneGlobePos1}");
            accessory.Log.Debug($"Boss 2 double arcane globe guide: arcaneGlobePos2 => {arcaneGlobePos2}");
            accessory.Log.Debug($"Boss 2 double arcane globe guide: myBaseAngle => {myBaseAngle}");
        }

        [ScriptMethod(name: "Boss 2 Targeted Light Guide", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(01ED|01EE)$"])]
        public void Boss2_TargetedLightGuide(Event @event, ScriptAccessory accessory)
        {
            //Players will receive this ICON multiple times, pay attention to distinguishing the count
            if(@event.GetTargetId() != accessory.Data.Me)
            {
                return;
            }

            uint myStatusId = 0;
            uint myStatusOffset = 0;

            for (uint statusId = 3726; statusId <= 3729 ;statusId++)
            {
                bool isMeGet = accessory.isMeGetStatus(statusId);
                myStatusId = isMeGet ? statusId : myStatusId;
            }

            for (uint statusId = 3715; statusId <= 3718 ;statusId++)
            {
                bool isMeGet = accessory.isMeGetStatus(statusId);
                myStatusId = isMeGet ? statusId : myStatusId;
            }

            if(myStatusId == 0)
            {
                return;
            }
            accessory.Log.Debug($"Actived! => Boss 2 Targeted Light Guide");

            /*
            Get TargetIcon Id
            On player
            Clockwise rotation 01ED
            Counterclockwise rotation 01EE

            3x rotation angle on player StatusID3721
            5x rotation angle on player StatusID3790

            Front unanalyzed StatusID3726 StackCount151 Param663
            Back unanalyzed StatusID3727 StackCount152 Param664
            Right unanalyzed StatusID3728 StackCount153 Param665
            Left unanalyzed StatusID3729 StackCount154 Param666

            Before move command StatusID3715
            After move command StatusID3716
            Left move command StatusID3717
            Right move command StatusID3718

                    
            How many degrees to rotate to align the gap target (for handling light orbs)
            Front unanalyzed StatusID3726 => 0
            Back unanalyzed StatusID3727 => PI
            Right unanalyzed StatusID3728 => -0.5PI
            Left unanalyzed StatusID3729 => 0.5PI


            How many degrees to rotate to align the forced move target
            Front move command StatusID3715 => 0
            Back move command StatusID3716 => PI
            Left move command StatusID3717 => 0.5PI
            Right move command StatusID3718 => -0.5PI
            */
            object myTowardsObj = null;
            List<float> baseAngles = new List<float>();
            baseAngles.Add(0);
            baseAngles.Add(MathF.PI);

            if(myStatusId >= 3726)
            {
                //Unanalyzed mechanism
                myTowardsObj = boss2_bossId;
                myStatusOffset = 3726;
                baseAngles.Add(-0.5f * MathF.PI);
                baseAngles.Add(0.5f * MathF.PI);
            }else{
                //Forced movement mechanism
                myTowardsObj = boss2_myTowardsPoint;
                myStatusOffset = 3715; 
                baseAngles.Add(0.5f * MathF.PI);
                baseAngles.Add(-0.5f * MathF.PI);
            }

            uint clockWiseIconId = 0x01ED;
            uint threeTimesStatusId = 3721;

            bool isMeGetClockwiseIcon = @event.GetIconId() == clockWiseIconId;

            //Not 3x angle
            bool isMeGetFive = !accessory.isMeGetStatus(threeTimesStatusId);
            float myBaseAngle = baseAngles[(int)(myStatusId - myStatusOffset)];

            /*
            Determine how the player's gap will rotate due to rotation angle + clockwise/counterclockwise
            If clockwise rotation, the gap angle will rotate +0.5PI, then the player's strategy is to rotate the gap target angle -0.5PI
            If counterclockwise rotation, the gap angle will rotate -0.5PI, then the player's strategy is to rotate the gap target angle +0.5PI
            */
            float myModifyAngle = isMeGetFive 
                                    ? (isMeGetClockwiseIcon ? -0.5f * MathF.PI : 0.5f * MathF.PI)
                                    : (isMeGetClockwiseIcon ? 0.5f * MathF.PI : -0.5f * MathF.PI);
            float myFinalAngleTurnTowards = myBaseAngle + myModifyAngle;

            Vector2 delay_destoryAt = myStatusId >= 3726 ? new(7000,4500) : new (2000,9000);
            Vector4 _color = GuideColor_GoNow.V4;
            Vector4 color = _color.WithW(_color.W + (myStatusId >= 3726 ? Boss2_PizzaColorDeepen : 0));
            accessory.DrawTurnTowards(myTowardsObj,new(20,0.33f*MathF.PI,myFinalAngleTurnTowards),new(9,5f),delay_destoryAt,color,true);

            accessory.Log.Debug($"Boss 2 Targeted Light Guide : myTowardsObj => {myTowardsObj}");
            accessory.Log.Debug($"Boss 2 Targeted Light Guide : isMeGetClockwiseIcon => {isMeGetClockwiseIcon}");
            accessory.Log.Debug($"Boss 2 Targeted Light Guide : isMeGetFive => {isMeGetFive}");
            accessory.Log.Debug($"Boss 2 Targeted Light Guide : myModifyAngle => {myModifyAngle}");
            accessory.Log.Debug($"Boss 2 Targeted Light Guide : myBaseAngle => {myBaseAngle}");
            accessory.Log.Debug($"Boss 2 Targeted Light Guide : myFinalAngleTurnTowards => {myFinalAngleTurnTowards}");
        }

        [ScriptMethod(name: "Boss 2 Calculated Trajectory Towards Round", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(371[5-8])$"])]
        public async void Boss2_CalculatedTrajectoryTowardsRound(Event @event, ScriptAccessory accessory)
        {
            if(!Boss2_TowardsRound) return;
            if(@event.GetTargetId() != accessory.Data.Me)
            {
                return;
            }
            uint delayMs = @event.GetDurationMilliseconds();
            delayMs = delayMs > 5000 ? delayMs : 5000;
            if(await DelayMillisecond((int)delayMs - 5000))
            {
                return;
            }
            int directionCount = 4;
            int per = 100;
            int duration =  5000;
            accessory.Log.Debug($"Boss 2 Calculated Trajectory Towards Round : count_per_duration => {directionCount},{per},{duration}");
            int times = duration / per ;
            for (int i = 0; i < times; i++)
            {
                IGameObject myGameObject = (IGameObject) accessory.Data.Objects.SearchByEntityId(accessory.Data.Me);
                if(myGameObject == null)
                {
                    break;
                }
                float _rot = myGameObject.Rotation;
                int _rotCount = (int)Math.Round(_rot/(2 * MathF.PI / directionCount));
                float rot = _rotCount * (2 * MathF.PI / directionCount);
                unsafe
                {
                    FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* myGameObjectStruct = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)myGameObject.Address;
                    myGameObjectStruct->SetRotation(rot);
                }
                if(await DelayMillisecond(per))
                {
                    break;
                }
            }
        }
        /*
        How to calculate the mine guidance position and facing angle
        Detect cast 34970, EffectPosition position move targetRotation x 4 to get the AOE center position
        The three points farthest from the arena center, do vectors from the point to the center, sum of the three vectors is the vector from the arena center to the gap, get the rotation angle based on this vector, rotate the guidance pattern
        Facing angle calculation is similar to high-precision light bomb guidance
        Subtraction Explosion a StatusID 3724
        Subtraction Explosion b StatusID 3725
        */
        [ScriptMethod(name: "Boss 2 Arcane Mine Guide", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(34970|35825)$"])]
        public void Boss2_ArcaneMineGuide(Event @event, ScriptAccessory accessory)
        {
            lock(_lock)
            {
                Vector3 minePos = @event.GetEffectPosition();
                float rot = @event.GetSourceRotation() + 0.5f * MathF.PI; //<= FFXIV Y-axis positive direction is 0
                Vector3 mineCentrePos = new (minePos.X + 4 * MathF.Cos(rot),minePos.Y, minePos.Z + 4 * MathF.Sin(rot));
                boss2_ArcaneMinesList.Add(mineCentrePos);
                if(boss2_ArcaneMinesList.Count < 8)
                {
                    return;
                }
            }
            
            //If we reach here, it usually means we have collected information on 8 mines
            accessory.Log.Debug($"Actived! => Boss 2 Arcane Mine Guide");
            Vector3 oPoint = new (200,-300,0);
            List<Vector3> mines = boss2_ArcaneMinesList.OrderByDescending(v3 => Util.DistanceBetweenTwoPoints(v3,oPoint)).ToList();
            Vector3 oToCorner = new (3 * oPoint.X - (mines[0].X + mines[1].X + mines[2].X),
                                     3 * oPoint.Y - (mines[0].Y + mines[1].Y + mines[2].Y),
                                     3 * oPoint.Z - (mines[0].Z + mines[1].Z + mines[2].Z));

            float cornerRot = MathF.Atan2(oToCorner.Z,oToCorner.X);
            float myModifyAngle = cornerRot - 0.25f * MathF.PI;
            //Using the gap at the lower right corner of the arena as the pattern, usually at this time cornerRot is 0.25PI
            Vector3 debuffStack1_template = new (216,-300,5.5f);
            Vector3 debuffStack3_template = new (192,-300,12.5f);
            Vector3 debuffStack2_template1 = new (216,-300,1.5f);
            Vector3 debuffStack2_template2 = new (216,-300,-9.5f);

            /*
            If 2+3 stack, the Z coordinate of the 2 stack must be 0
            If 2+2 stack, arbitrary
            If 2+1 stack, the Z coordinate of the 2 stack must be -8
            If 1+3 stack, arbitrary
            */
            uint stackDebuffId = 3724;
            uint surgeVectorDebuffId = 3723 ;
            Status? myDebuffInfo = accessory.GetStatusInfo(accessory.Data.Me,stackDebuffId);
            int myDebuffStack = myDebuffInfo == null ? 0 : ((Status)myDebuffInfo).Param;
            Vector3 myStartPoint = new (200,-300,0);
            Vector3 myTemplate = new (200,-300,0);
            float myTowardsRot = 0;
            bool isMeGetsurgeVectorDebuff = accessory.isMeGetStatus(surgeVectorDebuffId);
            switch(myDebuffStack)
            {
                case 1:
                    myTowardsRot =  0.5f * MathF.PI + 0.5f * MathF.PI;
                    myTemplate = debuffStack1_template;
                    break;
                case 3:
                    myTowardsRot =  1 * MathF.PI + 0.5f * MathF.PI;
                    myTemplate = debuffStack3_template;
                    break;
                case 2:
                    myTowardsRot =  0.5f * MathF.PI + 0.5f * MathF.PI;
                    List<uint> surgeVectorPlayers = accessory.whoGetStatusInParty(surgeVectorDebuffId);
                    

                    if(surgeVectorPlayers.Count == 2)
                    {   
                        int _count1 = accessory.GetStatusInfo(surgeVectorPlayers[0],stackDebuffId)?.Param ?? 100;
                        int _count2 = accessory.GetStatusInfo(surgeVectorPlayers[1],stackDebuffId)?.Param ?? 100;
                        int surgeVectorPlayersStackCount = _count1 + _count2;
                        // int surgeVectorPlayersStackCount = ((accessory.GetStatusInfo(surgeVectorPlayers[0],stackDebuffId)?.Param ?? 100) + 
                        //    (accessory.GetStatusInfo(surgeVectorPlayers[1],stackDebuffId)?.Param ?? 100));
                        //     The following is a code writing error. The first line of the following code ?.Param ?? 100 + 
                        //    accessory.GetStatusInfo(surgeVectorPlayers[1],stackDebuffId)?.Param ?? 100) was treated as a whole, only returning 2
                        //     surgeVectorPlayersStackCount = (accessory.GetStatusInfo(surgeVectorPlayers[0],stackDebuffId)?.Param ?? 100 + 
                        //    accessory.GetStatusInfo(surgeVectorPlayers[1],stackDebuffId)?.Param ?? 100);
                        if( surgeVectorPlayersStackCount == 5)
                        {
                            //If it's 2+3 stack
                           
                            myTemplate = isMeGetsurgeVectorDebuff?debuffStack2_template1:debuffStack2_template2;
                        }
                        else if(surgeVectorPlayersStackCount == 3)
                        {
                            //If it's 1+2 stack
                            myTemplate = isMeGetsurgeVectorDebuff?debuffStack2_template2:debuffStack2_template1;
                        }
                        else{
                            //Based on the other 2-layer buff
                            uint another2stackPlayer = 0;
                            List<uint> others = accessory.Data.PartyList.Except(new List<uint> { accessory.Data.Me }).ToList();
                            foreach (uint playerId in others)
                            {
                                Status? debuffInfo = accessory.GetStatusInfo(playerId,stackDebuffId);
                                bool isStack2 = (debuffInfo?.Param ?? 100) == 2;
                                if(isStack2)
                                {
                                    another2stackPlayer = playerId;
                                    break;
                                }

                            }
                            bool isMePriority = accessory.GetMyIndex() < accessory.GetIndexInParty(another2stackPlayer);
                            myTemplate = isMePriority? debuffStack2_template2 : debuffStack2_template1;
                        }
                         accessory.Log.Debug($"Boss 2 Arcane Mine Guide : surgeVectorPlayersStackCount => {surgeVectorPlayersStackCount}");
                    }
                    break;
                case 0:
                default:break;
            }

            Vector3 myTowardsPoint_template = new (myTemplate.X + MathF.Cos(myTowardsRot) * 80, myTemplate.Y, myTemplate.Z + MathF.Sin(myTowardsRot) * 80);
            myStartPoint = Util.RotatePointInFFXIVCoordinate(myTemplate,oPoint,myModifyAngle);
            boss2_myTowardsPoint = Util.RotatePointInFFXIVCoordinate(myTowardsPoint_template,oPoint,myModifyAngle);

            //Drawing part
            MultiDisDraw(new List<float[]>{new float[]{myStartPoint.X,myStartPoint.Y,myStartPoint.Z,0,10000}},accessory);

            accessory.Log.Debug($"Boss 2 Arcane Mine Guide : my order number in party => {1 + accessory.GetMyIndex()}");
            accessory.Log.Debug($"Boss 2 Arcane Mine Guide : cornerRot => {cornerRot}");
            accessory.Log.Debug($"Boss 2 Arcane Mine Guide : myDebuffStack => {myDebuffStack}");
            accessory.Log.Debug($"Boss 2 Arcane Mine Guide : isMeGetsurgeVectorDebuff => {isMeGetsurgeVectorDebuff}");
            accessory.Log.Debug($"Boss 2 Arcane Mine Guide : myStartPoint => {myStartPoint}");
            accessory.Log.Debug($"Boss 2 Arcane Mine Guide : myTowardsPoint => {boss2_myTowardsPoint}");
        }

        //Spatial Explosion guidance
        [ScriptMethod(name: "Boss 2 Spatial Tactics", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(34976|35831)$"])]
        public async void Boss2_SpatialTactics(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"Actived! => Boss 2 Spatial Tactics");
            if(await DelayMillisecond(12000))
            {
                return;
            }

            uint ballDataId1 = 16448;
            //Zero difficulty is 16606
            uint ballDataId2 = 16606;
            List<IGameObject> balls = accessory.GetEntitiesByDataId(ballDataId1).Union(accessory.GetEntitiesByDataId(ballDataId2)).ToList();
            if(balls.Count < 1)
            {
                return;
            }
            Vector3 originPos = new (200,-300,0);
            Vector3 ballPos = balls[0].Position;
            double _newRightUp = Math.Round((-0.32f + MathF.Atan2(ballPos.Z - originPos.Z,ballPos.X - originPos.X))/(0.25f * MathF.PI));
            float newRightUp = (float)_newRightUp * 0.25f * MathF.PI;
            //Using the ball at the northeast corner of the arena as the template, at this time newRightUp should be -0.25 PI
            Vector3 behindPoint1_template = new (200,-300,12);
            Vector3 behindPoint2_template = new (200,-300,4);
            Vector3 behindPoint1 = Util.RotatePointInFFXIVCoordinate(behindPoint1_template,originPos,newRightUp + 0.25f * MathF.PI);
            Vector3 behindPoint2 = Util.RotatePointInFFXIVCoordinate(behindPoint2_template,originPos,newRightUp + 0.25f * MathF.PI);

            List<Vector3> startPointsList_template = new List<Vector3>{
                new (originPos.X,originPos.Y,originPos.Z - 3.2f),
                new (originPos.X,originPos.Y,originPos.Z - 3.2f),
                new (originPos.X,originPos.Y,originPos.Z + 8f),
                new (originPos.X - 3.2f,originPos.Y,originPos.Z + 11.2f)
            };
            List<Vector3> endPointsList_template = new List<Vector3>{
                new (originPos.X,originPos.Y,originPos.Z - 8f),
                new (originPos.X,originPos.Y,originPos.Z - 3.2f),
                new (originPos.X,originPos.Y,originPos.Z + 8f),
                new (originPos.X- 3.2f,originPos.Y,originPos.Z + 12.8f),
            };
            uint stackDebuffId = 3725;
            Status myStackDebuffInfo = accessory.GetStatusInfo(accessory.Data.Me,stackDebuffId);
            int myStack = myStackDebuffInfo != null ? myStackDebuffInfo.Param : 1;
            myStack = myStack < 1 ? 1 : myStack;
            Vector3 _myStartPoint = startPointsList_template[myStack -1];
            Vector3 myStartPoint = Util.RotatePointInFFXIVCoordinate(_myStartPoint,originPos,newRightUp + 0.25f * MathF.PI);
            Vector3 _myEndPoint = endPointsList_template[myStack -1];
            Vector3 myEndPoint = Util.RotatePointInFFXIVCoordinate(_myEndPoint,originPos,newRightUp + 0.25f * MathF.PI);

            List<float[]> pointsList = new List<float[]>{
                new float[]{myStartPoint.X,myStartPoint.Y,myStartPoint.Z,0,8200},
                new float[]{myEndPoint.X,myEndPoint.Y,myEndPoint.Z,0,1800},
            };
            
            switch(myStack)
            {
                case 2:
                case 3:
                    pointsList.RemoveAt(pointsList.Count - 1);
                    break;
                case 1:
                    break;
                case 4:
                    pointsList[0][4] = 12800;
                    break;

            }
            MultiDisDraw(pointsList,accessory);
            DrawPropertiesEdit dpRect = accessory.GetDrawPropertiesEdit(behindPoint1, new (8,8), 6000, true);
            dpRect.Color = dpRect.Color.WithW(0.5f);
            dpRect.TargetPosition = originPos;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dpRect);
            dpRect.Position = behindPoint2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dpRect);

            accessory.Log.Debug($"Boss 2 Spatial Tactics : newRightUp => {newRightUp}");
            accessory.Log.Debug($"Boss 2 Spatial Tactics : myStack => {myStack}");
        }
        
        [ScriptMethod(name: "Boss 2 Three Treants Guide", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(16449|16607)$"])]
        public async void Boss2_GolemsGuide(Event @event, ScriptAccessory accessory)
        {
            if(IsInSuppress(10000, nameof(Boss2_GolemsGuide)) || await DelayMillisecond(7000))
            {
                return;
            }
            uint yellowArrowDataId = 2013505;
            List<Vector3> cornerYellowArrow = accessory.GetEntitiesByDataId(yellowArrowDataId).Where(obj => obj != null).Select(obj => obj.Position)
                        .Where(v3 => Math.Round(Math.Abs(v3.X - 200) - Math.Abs(v3.Z - 0)) == 0 ).ToList();
                
            if((cornerYellowArrow?.Count ?? 0) < 1)
            {
                return;
            }
            uint golemDataId = @event.GetDataId();
            List<Vector3> golemsPos = accessory.GetEntitiesByDataId(golemDataId).Where(obj => obj != null).Select(obj => obj.Position).ToList();
            List<uint> golemIds = accessory.GetEntitiesByDataId(golemDataId).Where(obj => obj != null).Select(obj => obj.EntityId).ToList();
            //Determine which is the new north side, and whether it's 2+4 treants or 3+5 treants
            //If the distance between two treants is greater than 6 and less than 10, then it's 2+4 treants
            bool is24typeGoloms = false;
            foreach (Vector3 pos in golemsPos)
            {
                foreach (Vector3 posCompareTo in golemsPos)
                {
                    float distance = Util.DistanceBetweenTwoPoints(pos,posCompareTo);
                    is24typeGoloms = is24typeGoloms || (distance > 6 && distance <10);
                }
                if(is24typeGoloms)
                {
                    break;
                }
            }

            List<Vector3> template24_MT = new List<Vector3> 
            {
                new (-16 + 200,-300,0), new (-11.2f + 200,-300,-3.2f), new (-4.8f + 200,-300,4)
            };
            List<Vector3> template35_MT = new List<Vector3> 
            {
                new (-11.2f + 200,-300,0), new (-19.2f + 200,-300,-3.2f), new (-12.8f + 200,-300,4)
            };

            List<Vector3> template24_D1 = new List<Vector3> 
            {
                new (-12.8f + 200,-300,4.8f), new (-11.2f + 200,-300,11.2f), new (-4.8f + 200,-300,4)
            };
            List<Vector3> template35_D1 = new List<Vector3> 
            {
                new (-11.2f + 200,-300,4.8f), new (-19.2f + 200,-300,11.2f), new (-12.8f + 200,-300,4)
            };

            List<Vector3> template24_H1 = new List<Vector3> 
            {
                new (3.2f + 200,-300,-3.2f), new (11.2f + 200,-300,-3.2f), new (4.8f + 200,-300,4)
            };
            List<Vector3> template35_H1 = new List<Vector3> 
            {
                new (4.8f + 200,-300,-3.2f), new (3.2f + 200,-300,-3.2f), new (-3.2f + 200,-300,4)
            };

            List<Vector3> template24_D2 = new List<Vector3> 
            {
                new (3.2f + 200,-300,11.2f), new (11.2f + 200,-300,11.2f), new (4.8f + 200,-300,4)
            };
            List<Vector3> template35_D2 = new List<Vector3> 
            {
                new (4.8f + 200,-300,11.2f), new (3.2f + 200,-300,11.2f), new (-3.2f + 200,-300,4)
            };

            uint surgeVectorDebuffId = 3723 ;
            List<uint> debuffPlayers = accessory.whoGetStatusInParty(surgeVectorDebuffId);
            bool isSurgeDebuffSameGroup = debuffPlayers.Count == 2 && Math.Abs(accessory.GetIndexInParty(debuffPlayers[0]) - accessory.GetIndexInParty(debuffPlayers[1])) == 2;
            
            List<Vector3>[] pointsTable = new List<Vector3>[]
            {
                is24typeGoloms ? template24_MT : template35_MT,
                is24typeGoloms ? template24_H1 : template35_H1,
                is24typeGoloms ? (!isSurgeDebuffSameGroup?template24_D1:template24_D2):(!isSurgeDebuffSameGroup?template35_D1:template35_D2),
                is24typeGoloms ? (!isSurgeDebuffSameGroup?template24_D2:template24_D1):(!isSurgeDebuffSameGroup?template35_D2:template35_D1),
            };
            List<Vector3> _myPointsListTemplate = pointsTable[accessory.GetMyIndex()];
            Vector3 orginPoint = new (200,-300,0);
            float rad = MathF.Atan2(cornerYellowArrow[0].Z,cornerYellowArrow[0].X - 200) - 0.25f * MathF.PI;
            List<Vector3> _myPointsList = new List<Vector3>{
                Util.RotatePointInFFXIVCoordinate(_myPointsListTemplate[0],orginPoint,rad),
                Util.RotatePointInFFXIVCoordinate(_myPointsListTemplate[1],orginPoint,rad),
                Util.RotatePointInFFXIVCoordinate(_myPointsListTemplate[2],orginPoint,rad),
            };
            List<float[]> myPointsList = new List<float[]> 
            {
                new float[] {_myPointsList[0].X,_myPointsList[0].Y,_myPointsList[0].Z,0,12000},
                new float[] {_myPointsList[1].X,_myPointsList[1].Y,_myPointsList[1].Z,0,9000},
                new float[] {_myPointsList[2].X,_myPointsList[2].Y,_myPointsList[2].Z,0,4000},
            };
            MultiDisDraw(myPointsList,accessory);


            //Draw danger zone
            foreach (uint id in golemIds)
            {
                accessory.FastDraw(DrawTypeEnum.Rect,id,new(8,40),new(0,15000 - 3800),false);
            }

            List<Vector3> dangerousZonePoints_template = new List<Vector3>
            {
                new (20+200,-300,16),
                new (20+200,-300,-8)
            };
            List<Vector3> dangerousZonePoints = new List<Vector3>
            {
                Util.RotatePointInFFXIVCoordinate(dangerousZonePoints_template[0],orginPoint,rad),
                Util.RotatePointInFFXIVCoordinate(dangerousZonePoints_template[1],orginPoint,rad),
            };
            DrawPropertiesEdit dpRect = accessory.GetDrawPropertiesEdit(dangerousZonePoints[0],new(8,42),15000 - 3800,false);
            dpRect.Rotation = - 0.5f * MathF.PI - rad;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dpRect);
            dpRect.Position = dangerousZonePoints[1];
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dpRect);

            accessory.Log.Debug($"Actived! => Boss 2 Golems Guide");
            accessory.Log.Debug($"Boss 2 Golems Guide : cornerYellowArrow => {cornerYellowArrow[0]}");
            accessory.Log.Debug($"Boss 2 Golems Guide : is24typeGoloms => {is24typeGoloms}");
            accessory.Log.Debug($"Boss 2 Golems Guide : isSurgeDebuffSameGroup => {isSurgeDebuffSameGroup}");
            accessory.Log.Debug($"Boss 2 Golems Guide : myPointsList0 => {_myPointsList[0]}");
            accessory.Log.Debug($"Boss 2 Golems Guide : myPointsList1 => {_myPointsList[1]}");
            accessory.Log.Debug($"Boss 2 Golems Guide : myPointsList2 => {_myPointsList[2]}");
        }
        #endregion


        #region Boss3
        /*
        Boss3 arena center is (-200,-200,0)
        14.17.51.458 First Trick Reload +14s reload complete
        14.18.06.936 +15s First Trap Shot
        14.18.16.309 +25s Happy Trigger
        14.18.29.502 +39s Bomb appears
        14.18.38.336 +47s Second Trap Shot
        First Trick Reload, no guidance, just prompt AOE range

        14.18.55.311 Second Trick Reload
                     +25s Shuriken buff appears
        14.19.24.769 +29s Bomb rotation complete
        14.19.27.636 +32s First Trap Shot (stack/spread and shuriken judged simultaneously)
        14.19.59.781 +64s Happy Trigger
        14.20.07.736 +72s Second Trap Shot cast
        14.20.13.902 +78s Forced movement judgment
        Second Trick Reload, no guidance for now
        Prompt bomb AOE range
        First Trap Shot, if stack, use the smaller AOE circle underfoot to indicate which color area to go to
        First Trap Shot, if spread, mark the spread AOE circle and color it to guide which color area to go to


        Race, calculate true north
        Prompt bomb AOE range
        Find the exploding bomb, calculate the nearest number point as the true north angle, only provide general direction guidance

        Second turntable, calculate true north
        Shuriken always lands on the inner side, according to the table below, the true north angle can be calculated
        Based on the clockwise/counterclockwise direction of the fire, calculate the stack group's movement
        If shuriken targets the same group (MT+H1) or (D1+D2), the start points of the stack group need to be swapped, end points unchanged


        14.22.01.118 Third Trick Reload
        14.22.15.569 +14.4s First Trap Shot
        14.22.28.836 +27.7s Gift box summon complete, need to pre-guide Happy Trigger
        14.22.36.704 +35.5s Happy Trigger, +2s later short buff judgment
        14.22.43.669 +42.5s Second Trap Shot cast, +5s later long buff judgment, long buff needs to handle stack/spread + bomb AOE
        */


        [ScriptMethod(name: "Boss 3 Trick Reload Init", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35146|35175)$"], userControl: false)]
        public void Boss3_TrickReloadInit(Event @event, ScriptAccessory accessory)
        { 
            boss3_trickReloads.Clear();
            boss3_trickReloadsCount ++ ;
            accessory.Log.Debug($"Actived! => Boss 3 Trick Reload Init {boss3_trickReloadsCount}");
        
        }

        //Record reload failure 35110 + reload success 35109
        [ScriptMethod(name: "Boss 3 Trick Reload Log", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(35110|35109)$"])]
        public async void Boss3_TrickReloadLog(Event @event, ScriptAccessory accessory)
        {

            //Prompt spread, third time prompt Happy Trigger safe zone
            boss3_trickReloads.Add(@event.GetActionId());
            if(boss3_trickReloads.Count!= 8)
            {
                return;
            }
            //Is spread first?
            uint loadedActionId = 35109;
            uint misloadedActionId = 35110;
            bool isDisperseFirst = boss3_trickReloads[0] == loadedActionId;
            int safeNumber = boss3_trickReloads.GetRange(1,6).IndexOf(misloadedActionId);
            Vector2 firstShotDelay_destoryAt = new (0,8000);
            Vector2 secondShotDelay_destoryAt = new(30000,8000);
            switch(boss3_trickReloadsCount)
            {
                case 1:
                    firstShotDelay_destoryAt = new (0,9000);
                    secondShotDelay_destoryAt = new(27000,15000-100);
                    break;
                case 2:
                    firstShotDelay_destoryAt = new (15000,15000-2800);
                    secondShotDelay_destoryAt = new(55000,12500-100);
                    break;
                case 3:
                    firstShotDelay_destoryAt = new (0,9000);
                    secondShotDelay_destoryAt = new(24200,12500);
                    break;
                case 0:
                default:
                    break;
            }


            //Stack/spread drawing, currently only drawing spread
            DrawPropertiesEdit dpCircle = accessory.GetDrawPropertiesEdit(accessory.Data.Me, new(6.1f),8000,false);
            dpCircle.Delay = (long)(isDisperseFirst ? firstShotDelay_destoryAt.X : secondShotDelay_destoryAt.X);
            dpCircle.DestoryAt = (long)(isDisperseFirst ? firstShotDelay_destoryAt.Y : secondShotDelay_destoryAt.Y);
            dpCircle.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
            foreach (uint id in accessory.Data.PartyList)
            {
                dpCircle.Owner = id;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dpCircle);
            }


            //DrawPropertiesEdit dpDount = accessory.GetDrawPropertiesEdit(accessory.Data.Me, new(6.1f),0,false);

            if(boss3_trickReloadsCount == 2){
                //Wait for shuriken buff to appear
                if(await DelayMillisecond(11000)){
                    return;
                }
                //MT H1 D1
                List<Vector4> colors = new List<Vector4>
                {
                    // MT,H1,D1
                    new (0,0,1,10),
                    new (0.83f,0.855f,0.165f,10),
                    new (1,0,0,10),
                };
                //Detect shuriken StatusID 3742
                Vector4 myColor = new (1,1,1,10);
                uint eyeStatusId = 3742 ;
                bool isMeGetEyeStatus = accessory.isMeGetStatus(eyeStatusId);
                if(accessory.GetMyIndex()<=2)
                {
                    myColor = isMeGetEyeStatus ? colors[accessory.GetMyIndex()] : myColor ;
                }
                else
                {
                    //D2
                    if(isMeGetEyeStatus)
                    {
                        //D2 has debuff
                        //If MT or H1 doesn't have debuff, go to that person's color
                        List<uint> others = accessory.whoGetStatusInPartyWithoutMe(eyeStatusId);
                        if(others.Count == 2)
                        {
                            switch(accessory.GetIndexInParty(others[0]) + accessory.GetIndexInParty(others[1]))
                            {
                                case 0 + 1 :
                                    myColor = colors[2];
                                    break;
                                case 0 + 2 : 
                                    myColor = colors[1];
                                    break;
                                case 1 + 2 : 
                                    myColor = colors[0];
                                    break;
                            }
                        }
                                                                                                                           
                    }

                }
                DrawPropertiesEdit dpSmallCircle = accessory.GetDrawPropertiesEdit(accessory.Data.Me, new(1.1f),16000,false);
                dpSmallCircle.Color = myColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dpSmallCircle);
                accessory.Log.Debug($"Boss 3 Trick Reload Log : my order number in party => {1 + accessory.GetMyIndex()}");
                accessory.Log.Debug($"Boss 3 Trick Reload Log : myColor => {myColor}");

            }

            if(boss3_trickReloadsCount == 3)
            {
                //Draw a mahjong safe zone guide, delay 12 seconds, display 10 seconds
                DrawPropertiesEdit dpFan = accessory.GetDrawPropertiesEdit(new Vector3(-200,-200,0), new(20.1f),10000,true);
                dpFan.Radian = MathF.PI / 3f ;
                dpFan.Color = accessory.Data.DefaultSafeColor.WithW(0.4f);
                dpFan.Delay = 12000;
                //Starting coordinate is FFXIV coordinate -0.5Ï€, clockwise/counterclockwise opposite
                float _rot = -1f * MathF.PI + safeNumber * MathF.PI/3f ;
                dpFan.Rotation = -_rot;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dpFan);
            }

        }


        // This part of the code references Cyf5119's AalBomb script
        [ScriptMethod(name: "Boss 3 Bomb", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(16481|16489)$"])]
        public async void Boss3_Bomb(Event @event, ScriptAccessory accessory)
        {

            if(IsInSuppress(5000, nameof(Boss3_Bomb)))
            {
                return;
            }
            boss3_bombsRound++;
            uint bombDataId = @event.GetDataId();

            /*
            First bomb appears to explosion 16s
            Second bomb appears to explosion 19.3s
            Third bomb appears to explosion 18.1s
            Fourth bomb appears to explosion 16.5s
            */

            int bombDelayTime = 0;
            switch(boss3_bombsRound)
            {
                case 1 :
                    bombDelayTime = 16000;
                    break;
                case 2 :
                    bombDelayTime = 19300;
                    break;
                case 3 :
                    bombDelayTime = 18100;
                    break;
                case 4 :
                    bombDelayTime = 16500;
                    break;
                default :
                    bombDelayTime = 19500;
                    break;
            }

            //5-second zero bomb search
            for(int times = 0 ; times < 50 ; times ++)
            {
                IBattleChara _bombsDetect0 = null;
                unsafe
                {
                    List<IBattleChara> _bombsDetect = accessory.GetEntitiesByDataId(bombDataId).Where(obj => obj as IBattleChara != null).Cast<IBattleChara>().ToList();
                    foreach (IBattleChara _bomb in _bombsDetect)
                    {
                        FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* _bombStructPtr = (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)_bomb.Address;
                        if(!object.Equals((*_bombStructPtr), null))
                        {
                            //Bombs that glow have .Timeline.AnimationState[0] value 1, non-glowing ones have 0
                            if((*_bombStructPtr).Timeline.AnimationState[0] > 0)
                            {
                                _bombsDetect0 = _bomb;
                            }
                        }
                    }

                }
                if(_bombsDetect0 != null)
                {
                    bombDelayTime -= times * 100;
                    accessory.Log.Debug($"Boss 3 Bomb : detect times => {times}");
                    break;
                }
                if(await DelayMillisecond(100))
                {
                    break;
                }

            }

            
            if(await DelayMillisecond(300))
            {
                // bombDelayTime -= 300;
                return;
            }

            accessory.Log.Debug($"Actived! => Boss 3 Bomb {boss3_bombsRound}");
            
            List<IBattleChara> _bombsList = accessory.GetEntitiesByDataId(bombDataId).Where(obj => obj as IBattleChara != null).Cast<IBattleChara>().ToList();
            IBattleChara bomb0 = null;

            //Store bombs that will explode
            List<IBattleChara> bombsList = new List<IBattleChara>();
            unsafe
            {
                foreach (IBattleChara _bomb in _bombsList)
                {
                    FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* _bombStructPtr = (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)_bomb.Address;
                    if(!object.Equals((*_bombStructPtr), null))
                    {
                        //Bombs that glow have .Timeline.AnimationState[0] value 1, non-glowing ones have 0
                        if((*_bombStructPtr).Timeline.AnimationState[0] > 0)
                        {
                            bomb0 = _bomb;
                        }
                    }
                }


                if(bomb0 != null)
                {
                    bombsList.Add(bomb0);
                    accessory.Log.Debug($"Boss 3 Bomb : bomb0 Id => {bomb0.EntityId}");
                    FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* bomb0StructPtr = (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)bomb0.Address;
                    /*
                        (*bomb0StructPtr).Vfx.Tethers[0].TargetId.ObjectId is the target object of the tether
                        (*bomb0StructPtr).Vfx.Tethers[1].TargetId.ObjectId is fixed at 0xE0000000, seemingly used to mark the end
                    */

                    //Usually (*bomb0StructPtr).Vfx.Tethers[0].TargetId.ObjectId is the second bomb, use this to find the third bomb
                    uint _bomb1Id = (*bomb0StructPtr).Vfx.Tethers[0].TargetId.ObjectId;
                    List<IBattleChara> _bomb1 = _bombsList.Where(obj => obj.EntityId == _bomb1Id).ToList();
                    if(_bomb1.Count > 0)
                    {
                        //Found the second bomb
                        bombsList.Add(_bomb1[0]);
                        FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* bomb1StructPtr = (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)_bomb1[0].Address;
                        uint _bomb2Id = (*bomb1StructPtr).Vfx.Tethers[0].TargetId.ObjectId;

                        List<IBattleChara> _bomb2 = _bombsList.Where(obj => obj.EntityId == _bomb2Id).ToList();
                        if(_bomb2.Count >0)
                        {
                            bombsList.Add(_bomb2[0]);
                        }
                        
                    }
                }
            }

            if(bombsList.Count != 3)
            {
                return;
            }

            List<IBattleChara> dudsList = _bombsList.Except(bombsList).ToList();

            unsafe
            {
                foreach (IBattleChara dud in dudsList)
                {
                    FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* dudStructPtr = (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)dud.Address;
                    //dudStructPtr->DrawObject->IsVisible = false;
                    dudStructPtr->SetDrawOffset(0,-5,0);
                }
            }


            
            /*
            Pass which bomb is closest to which number point
            Point 1, -0.25Ï€,19
            Point 2, 0.25Ï€,19
            Point 3, 0.75Ï€,19
            Point 4, -0.75Ï€,19
            Calculate the distance from each point to the three bombs, take the shortest distance as the point's identifier distance
            Sort each point by identifier distance
            */
            Vector3 originPos = new (-200,-200,0);
            List<float[]> rad_distance = new List<float[]>
            {
                new float[]{-0.25f * MathF.PI,100},
                new float[]{0.25f * MathF.PI,100},
                new float[]{0.75f * MathF.PI,100},
                new float[]{-0.75f * MathF.PI,100}
            };

            for(int i = 0 ; i < rad_distance.Count ; i++ )
            {
                float[] r_d = rad_distance[i] ;
                foreach (IBattleChara _bomb in bombsList)
                {
                    Vector3 _bombPos = _bomb.Position;
                    Vector3 pointPos = new (MathF.Cos(r_d[0]) * 19f + originPos.X ,_bombPos.Y,MathF.Sin(r_d[0]) * 19f + originPos.Z);
                    float _distance = Util.DistanceBetweenTwoPoints(_bombPos,pointPos);
                    rad_distance[i][1] = _distance < rad_distance[i][1] ? _distance : rad_distance[i][1];
                }
            }
            //Pass parameters
            boss3_rad_distance = rad_distance.OrderBy(r_d => r_d[1]).ToList();


            Vector4 _color = boss3_bombsRound == 2 ? accessory.Data.DefaultDangerColor.WithW(4) : accessory.Data.DefaultDangerColor ;
            //Drawing part
            foreach (IBattleChara bomb in bombsList)
            {
                
                accessory.FastDraw(DrawTypeEnum.Circle,bomb.EntityId,new (12),new (bombDelayTime - 7000 ,7000),_color);
            }

        }

        /*
        The arena's shuriken terrain only has one type, with the FFXIV coordinate X-axis positive direction as 0PI

        *PI           Inner     Outer
        0/12 + 1/24   Blue      Green
        1/12 + 1/24   Red       Blue
        2/12 + 1/24   Green     Red

        Cycle fill up to 11/12
        */
        [ScriptMethod(name: "Boss 3 Fire Spread", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35154|35183)$"], userControl: false)]
        public void Boss3_FireSpread(Event @event, ScriptAccessory accessory)
        { 
            lock(_lock)
            {
                boss3_fireSpreadRotation.Add(0.5f * MathF.PI - @event.GetSourceRotation());
                boss3_burningChainsPlayers.Clear();
                //accessory.Log.Debug($"Actived! => Boss 3 Fire Spread {boss3_fireSpreadRotation.Count}");
            }
            
        }
        [ScriptMethod(name: "Boss 3 Burning Chains", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(0061)$"], userControl: false)]
        public void Boss3_BurningChains(Event @event, ScriptAccessory accessory)
        {
            lock(_lock)
            {
                boss3_burningChainsPlayers.Add(@event.GetTargetId());
                //accessory.Log.Debug($"Actived! => Boss 3 Burning Chains {boss3_burningChainsPlayers.Count}");
            }
        }

        [ScriptMethod(name: "Boss 3 Fire Ball Clockwise", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(009C|009D)$"], userControl: false)]
        public void Boss3_FireBallClockwise(Event @event, ScriptAccessory accessory)
        {
            boss3_isFireBallClockwise = @event.GetIconId() == 0x009C;
            //accessory.Log.Debug($"Actived! => Boss 3 Fire Ball Clockwise {boss3_isFireBallClockwise}");
        }



        //TODO Hide large fireballs in this mechanism
        [ScriptMethod(name: "Boss 3 Second Dart Board", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(16478|16486)$"])]
        public async void Boss3_SecondDartBoard(Event @event, ScriptAccessory accessory)
        {
            if( await DelayMillisecond(2500) || boss3_fireSpreadRotation.Count < 3 )
            {
                //Wait for boss fire spread cast information collection to complete
                return;
            }
            unsafe
            {
                // Hide large fireball
                uint NBallOfFireDataId = 0x4060;
                uint SBallOfFireDataId = 0x4068;
                List<IGameObject> _ball = accessory.GetEntitiesByDataId(NBallOfFireDataId).Union(accessory.GetEntitiesByDataId(SBallOfFireDataId)).ToList();
                if(_ball.Count > 0)
                {
                    accessory.Log.Debug($"Boss 3 Second Dart Board : make ball of fire invisible");
                    FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* _ballStructPtr = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*) (_ball[0].Address);
                    _ballStructPtr->DrawObject->IsVisible = false;
                }
            }
            
            accessory.Log.Debug($"Actived! => Boss 3 Second Dart Board");
            //Fire ball clockwise/counterclockwise
            bool isFireBallClockwise = boss3_isFireBallClockwise;
            //Find where the induction rune is
            Vector3 _homingPatternPos = @event.GetSourcePosition();
            Vector3 originPos = new (-200,-200,0);
            float _homingRot = MathF.Atan2(_homingPatternPos.Z - originPos.Z,_homingPatternPos.X - originPos.X);
            int _homingNum = (int)Math.Round((_homingRot + 2f * MathF.PI - MathF.PI/12f) / (MathF.PI/6f)) % 3;
            //If 0, landing point is blue, new 12 o'clock angle is 0 + (Ï€/2)x integer ;1, red, Ï€/6 + (Ï€/2)x integer ;2, green, Ï€/3 + (Ï€/2)x integer;
            float newNorthRot = -0.5f * MathF.PI;
            foreach (float fireSpreadRot in boss3_fireSpreadRotation)
            {
                newNorthRot = _homingNum == ((int)Math.Round((fireSpreadRot + 2f * MathF.PI)/(MathF.PI/6f)) % 3) ? fireSpreadRot : newNorthRot;
            }
            //Obtained the new true north angle
            //Draw a circle at the new north as a prompt
            Vector3 circlPos = new (MathF.Cos(newNorthRot) * 8f + originPos.X,originPos.Y,MathF.Sin(newNorthRot) * 8f + originPos.Z);
            accessory.FastDraw(DrawTypeEnum.Circle,circlPos,new (2.5f),new (0,10000),accessory.Data.DefaultSafeColor.WithW(8));

            //Wait for tether buff to appear
            if( await DelayMillisecond(9000-2500) || boss3_burningChainsPlayers.Count < 2 )
            {
                //Wait for burning chain mark collection to complete
                return;
            }
            bool isMeGetBurningChains = boss3_burningChainsPlayers.Contains(accessory.Data.Me);
            //Using point B as the new true north point as the template
            Vector3 burningChainsLeft_template = new (-200-0.8f,-200,-18.5f);
            Vector3 burningChainsRight_template = new (-200-0.8f,-200,18.5f);
            Vector3 noBurningChainsLeft_template = new (-200 + 18.5f,-200,-0.8f);
            Vector3 noBurningChainsRight_template = new (-200 + 18.5f,-200,0.8f);
            int[] roleMarkPriority = new int[] { (int)RoleMarkEnum.MT, (int)RoleMarkEnum.H1, (int)RoleMarkEnum.D1, (int)RoleMarkEnum.D2};
            Vector3 myStartPoint = new (-200,-200,0);
            Vector3 myEndPoint = new (-200,-200,0);
            bool isMeGoLeft = false;
            if(isMeGetBurningChains)
            {
                List<uint> anotherOneId = boss3_burningChainsPlayers.Except(new List<uint>{accessory.Data.Me}).ToList();
                isMeGoLeft = anotherOneId.Count > 0 && Array.IndexOf(roleMarkPriority, accessory.GetMyIndex()) < Array.IndexOf(roleMarkPriority, accessory.GetIndexInParty(anotherOneId[0]));
                Vector3 _myStartPoint = isMeGoLeft ? burningChainsLeft_template  :burningChainsRight_template;
                myStartPoint = Util.RotatePointInFFXIVCoordinate(_myStartPoint,originPos,newNorthRot);
            }
            else
            {
                
                List<uint> noburningChainsPlayers = accessory.Data.PartyList.Except(boss3_burningChainsPlayers).ToList();
                List<uint> anotherOneId = noburningChainsPlayers.Except(new List<uint>{accessory.Data.Me}).ToList();
                isMeGoLeft = anotherOneId.Count > 0 && Array.IndexOf(roleMarkPriority, accessory.GetMyIndex()) < Array.IndexOf(roleMarkPriority, accessory.GetIndexInParty(anotherOneId[0]));
                bool isBullEyeNeedChange = false;
                uint eyeStatusId = 3742;
                List<uint> bullEyePlayers = accessory.whoGetStatusInParty(eyeStatusId);
                isBullEyeNeedChange = bullEyePlayers.Count == 2 
                    && (
                        accessory.GetIndexInParty(bullEyePlayers[0]) + accessory.GetIndexInParty(bullEyePlayers[1]) == 1 
                        || accessory.GetIndexInParty(bullEyePlayers[0]) + accessory.GetIndexInParty(bullEyePlayers[1]) == 5
                        );
                accessory.Log.Debug($"Boss 3 Second Dart Board :isMeGoLeft => {isMeGoLeft}");
                accessory.Log.Debug($"Boss 3 Second Dart Board :isBullEyeNeedChange => {isBullEyeNeedChange}");
                isMeGoLeft = isBullEyeNeedChange ? !isMeGoLeft : isMeGoLeft;
                Vector3 _myStartPoint = isMeGoLeft ? noBurningChainsLeft_template : noBurningChainsRight_template;
                myStartPoint = Util.RotatePointInFFXIVCoordinate(_myStartPoint,originPos,newNorthRot);
            }
            
            myEndPoint = Util.RotatePointInFFXIVCoordinate(myStartPoint,originPos,isFireBallClockwise ? 1.05f:-1.05f);
            //Draw second round dart board guidance
            List<float[]> myPointsList = new List<float[]>();
            myPointsList.Add(new float[]{originPos.X,originPos.Y,originPos.Z,0,3300});
            myPointsList.Add(new float[]{myStartPoint.X,myStartPoint.Y,myStartPoint.Z,0,5300});
            myPointsList.Add(new float[]{myEndPoint.X,myEndPoint.Y,myEndPoint.Z,0,5000});
            MultiDisDraw(myPointsList,accessory);
            boss3_fireSpreadRotation.Clear();
            accessory.Log.Debug($"Boss 3 Second Dart Board : my order number in party => {1 + accessory.GetMyIndex()}");
            accessory.Log.Debug($"Boss 3 Second Dart Board :isFireBallClockwise => {isFireBallClockwise}");
            accessory.Log.Debug($"Boss 3 Second Dart Board :isMeGetBurningChains => {isMeGetBurningChains}");
            accessory.Log.Debug($"Boss 3 Second Dart Board :isMeGoLeft => {isMeGoLeft}");
            accessory.Log.Debug($"Boss 3 Second Dart Board :newNorthRot => {newNorthRot}");
        }

        //Using rocket appearance as the starting time for guidance
        // Surprise Missile 16482 Tether 0011 
        // Surprise Claw 16484
        // Surprise Staff 16483
        // Missile and Claw can obtain the tethered target through .TargetObject



        // [ScriptMethod(name: "Boss 3 Surprising Claw Phase", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(16484|16492)$"])]

        
        [ScriptMethod(name: "Boss 3 Surprising Claw Phase",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:regex:^(0011)$"])]
        public async void Boss3_SurprisingClawPhase(Event @event, ScriptAccessory accessory)
        {

            if (@event.TargetId != (ulong)accessory.Data.Me) return;
            accessory.Log.Debug($"Actived! => Boss 3 Surprising Claw Phase");

            bool isMeGetClaw = false;
            bool isMyObjectAtLeft = false;
            Vector3 originPos = new(-200, -200, 0);

            ulong sourceId = @event.SourceId;
            IGameObject? sourceObj = accessory.Data.Objects.SearchById(sourceId);
            if (sourceObj is not null)
            {
                isMeGetClaw = sourceObj.DataId == (uint)0x4064 || sourceObj.DataId == (uint)0x406C;
                Vector3 _pos = sourceObj.Position;
                if (boss3_rad_distance.Count > 0)
                {
                    _pos = Util.RotatePointInFFXIVCoordinate(sourceObj.Position, originPos, -0.5f * MathF.PI - boss3_rad_distance[0][0]);
                }
                isMyObjectAtLeft = _pos.X < originPos.X;
            }

            //Template with 1 point -0.25Ï€ as new north point
            Vector3 claw_startTemplate = new(-200 + 13, -200, -13);
            Vector3 claw_endTemplate = new(-200 - 13, -200, 13);
            Vector3 missile_startTemplate = new(-200, -200, 0);
            Vector3 missile_endTemplateLeft = new(-200 - 13, -200, -13);
            Vector3 missile_endTemplateRight = new(-200 + 13, -200, 13);

            //Draw, if it's a claw, add an intermediate navigation point
            List<float[]> myPointsList = new List<float[]>();

            float _rot = boss3_rad_distance[0][0] + 0.25f * MathF.PI;
            Vector3 _myStartPoint = isMeGetClaw ? claw_startTemplate : missile_startTemplate;
            Vector3 myStartPoint = Util.RotatePointInFFXIVCoordinate(_myStartPoint, originPos, _rot);
            Vector3 _myEndPoint = isMeGetClaw ? claw_endTemplate : missile_endTemplateLeft;
            Vector3 myEndPoint = Util.RotatePointInFFXIVCoordinate(_myEndPoint, originPos, _rot);
            if ((!isMeGetClaw) && isMyObjectAtLeft)
            {
                _myEndPoint = missile_endTemplateRight;
                myEndPoint = Util.RotatePointInFFXIVCoordinate(_myEndPoint, originPos, _rot);
            }
            //Add starting point
            myPointsList.Add(new float[] { myStartPoint.X, myStartPoint.Y, myStartPoint.Z, 0, 8500 });
            //If it's a claw, add turning point
            if (isMeGetClaw)
            {
                myPointsList[0][4] = 4500;
                //Add turning point
                Vector3 _point = Util.RotatePointInFFXIVCoordinate(myStartPoint, originPos, isMyObjectAtLeft ? 0.3f * MathF.PI : -0.3f * MathF.PI);
                myPointsList.Add(new float[] { _point.X, _point.Y, _point.Z, 0, 3000 });
            }
            //Add end point
            myPointsList.Add(new float[] { myEndPoint.X, myEndPoint.Y, myEndPoint.Z, 0, 4000 });
            MultiDisDraw(myPointsList, accessory);
            accessory.Log.Debug($"Boss 3 Surprising Claw Phase : my order number in party => {1 + accessory.GetMyIndex()}");
            accessory.Log.Debug($"Boss 3 Surprising Claw Phase :isMeGetClaw => {isMeGetClaw}");
            accessory.Log.Debug($"Boss 3 Surprising Claw Phase :isMyObjectAtLeft => {isMyObjectAtLeft}");
            accessory.Log.Debug($"Boss 3 Surprising Claw Phase :new North => {boss3_rad_distance[0][0]}");
        }
        
        [ScriptMethod(name: "Boss 3 Happy Trigger Safe Zone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(35207|35208)$"])]
        public void Boss3_TriggerHappySafeZone(Event @event, ScriptAccessory accessory)
        {
            //Safe zone is a 60-degree cone from a boss clone casting 35207
            DrawPropertiesEdit dpFan = accessory.GetDrawPropertiesEdit(@event.GetSourceId(), new(20), 4600, true);
            dpFan.Radian = MathF.PI / 3.0f;
            dpFan.Color = accessory.Data.DefaultSafeColor.WithW(3.5f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dpFan);

        }

        /*
            Move command Forward 3538
            Move command Back 3539
            Move command Left 3540
            Move command Right 3541
            
        */
        [ScriptMethod(name: "Boss 3 Move Command Destination Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3538|3539|3540|3541)$"])]
        public void Boss3_ForwardMarchHint(Event @event, ScriptAccessory accessory)
        {

            if(@event.GetTargetId() != accessory.Data.Me)
            {
                return;
            }
            float[] rad = new float[]
            {
                0,
                MathF.PI,
                0.5f * MathF.PI,
                -0.5f * MathF.PI
            };

            DrawPropertiesEdit dpDis = accessory.GetDrawPropertiesEdit(accessory.Data.Me,new (0.7f,11.5f),5900,false);
            dpDis.Rotation = rad[@event.GetStatusID() - 3538];
            dpDis.Delay = @event.GetDurationMilliseconds() - 6000;
            dpDis.Color = accessory.Data.DefaultDangerColor.WithW(1.5f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dpDis);
        
        }

        [ScriptMethod(name: "Boss 3 Needles", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(16479|16487)$"])]
        public void Boss3_Needles(Event @event, ScriptAccessory accessory)
        {
           accessory.FastDraw(DrawTypeEnum.Rect,@event.GetSourceId(),new(2, 40),new (5000,6000),false);
        }
        #endregion

        private void MultiDisDraw(List<float[]> pointsList, ScriptAccessory accessory)
        {
            Vector4 colorGoNow = GuideColor_GoNow.V4.WithW(GuideColorDensity);
            Vector4 colorGoLater = GuideColor_GoLater.V4.WithW(GuideColorDensity);
            if(boss3_fireSpreadRotation.Count == 3){
                colorGoNow = colorGoNow.WithW(colorGoNow.W + 12);
                colorGoLater = colorGoLater.WithW(colorGoLater.W + 12);
            }
            float _width = Guide_Width > 0.09f ? Guide_Width : 0.1f;
            _width = _width < 10.01f ? _width : 10f;
            accessory.DrawWaypoints(pointsList,new(_width,_width * 0.5f + 0.1f),0,colorGoNow,colorGoLater);
        }
        private bool IsInSuppress(int suppressMillisecond,string methodName) {
            lock(_lock){
                return TsingUtilities.IsInSuppress(invokeTimestamp, methodName, suppressMillisecond);
            }
        }
        private async Task<bool> DelayMillisecond(int delayMillisecond){
            return await TsingUtilities.DelayMillisecond(delayMillisecond,cancellationTokenSource.Token);
        }
    }

    #region Extension Methods
    public static class ScriptExtensions_Tsing
    {

        //Get id
        private static bool ParseHexId(string? idStr, out uint id)
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
        private static string notFound_v3 = JsonConvert.SerializeObject(new Vector3(-404, -404, -404));
        public static uint GetActionId(this Event @event)
        {
            return JsonConvert.DeserializeObject<uint>(@event["ActionId"] ?? "0");
        }
        public static uint GetSourceId(this Event @event)
        {
            return ParseHexId(@event["SourceId"], out uint id) ? id : 0;
        }
        public static uint GetSourceDataId(this Event @event)
        {
            return JsonConvert.DeserializeObject<uint>(@event["SourceDataId"] ?? "0");
        }
        public static uint GetDataId(this Event @event)
        {
            return JsonConvert.DeserializeObject<uint>(@event["DataId"] ?? "0");
        }

        public static uint GetTargetId(this Event @event)
        {
            return ParseHexId(@event["TargetId"], out uint id) ? id : 0;
        }
        public static uint GetTargetDataId(this Event @event)
        {
            return JsonConvert.DeserializeObject<uint>(@event["TargetDataId"] ?? "0");
        }

        public static uint GetTargetIndex(this Event @event)
        {
            return JsonConvert.DeserializeObject<uint>(@event["TargetIndex"] ?? "0");
        }

        public static Vector3 GetSourcePosition(this Event @event)
        {
            return JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"] ?? notFound_v3);
        }

        public static Vector3 GetTargetPosition(this Event @event)
        {
            return JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"] ?? notFound_v3);
        }

        public static Vector3 GetEffectPosition(this Event @event)
        {
            return JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"] ?? notFound_v3);
        }

        public static float GetSourceRotation(this Event @event)
        {
            return JsonConvert.DeserializeObject<float>(@event["SourceRotation"] ?? "404.404");
        }

        public static float GetTargetRotation(this Event @event)
        {
            return JsonConvert.DeserializeObject<float>(@event["TargetRotation"] ?? "404.404");
        }

        public static string GetSourceName(this Event @event)
        {
            return @event["SourceName"] ?? "notFound" ;
        }

        public static string GetTargetName(this Event @event)
        {
            return @event["TargetName"] ?? "notFound" ;
        }
        /*
        When a player applies a permanent status to themselves (like tank stance, PVP sprint)
        @event["DurationMilliseconds"] returns "18446744073709551615", which will throw an exception
        */
        public static uint GetDurationMilliseconds(this Event @event)
        {
            string _dm = @event["DurationMilliseconds"];
            _dm = _dm.Length < 10 ? _dm : "404404404";
            return JsonConvert.DeserializeObject<uint>(_dm);
        }

        public static uint GetIndex(this Event @event)
        {
            return ParseHexId(@event["Index"], out uint id) ? id : 404;
        }

        public static uint GetState(this Event @event)
        {
            return ParseHexId(@event["State"], out uint id) ? id : 0;
        }

        public static uint GetDirectorId(this Event @event)
        {
            return ParseHexId(@event["DirectorId"], out uint id) ? id : 404;
        }

        public static uint GetStatusID(this Event @event)
        {
            return JsonConvert.DeserializeObject<uint>(@event["StatusID"] ?? "0");
        }

        public static uint GetStackCount(this Event @event)
        {
            return JsonConvert.DeserializeObject<uint>(@event["StackCount"] ?? "404");
        }

        public static uint GetParam(this Event @event)
        {
            return JsonConvert.DeserializeObject<uint>(@event["Param"] ?? "404");
        }

        public static uint GetIconId(this Event @event)
        {
            return ParseHexId(@event["Id"], out uint id) ? id : 0;
        }


        //Get party index of a member
        public static int GetIndexInParty (this ScriptAccessory accessory,uint entityId)
        {
            return accessory.Data.PartyList.IndexOf(entityId);
        }
        public static int GetMyIndex(this ScriptAccessory accessory)
        {
            return accessory.GetIndexInParty(accessory.Data.Me);
        }

        //Get party list
        public static IEnumerable<IBattleChara> GetPartyEntities(this ScriptAccessory accessory)
        {
            return accessory.Data.Objects.Where(obj => obj is IBattleChara && accessory.Data.PartyList.Contains(obj.EntityId)).Select(obj => (IBattleChara)obj);
        }

        public static IEnumerable<IGameObject> GetEntitiesByDataId(this ScriptAccessory accessory,uint dataId)
        {
            return accessory.Data.Objects.Where(obj => obj is IGameObject && obj?.DataId == dataId);
        }
        public static List<uint> GetEntityIdsByDataId(this ScriptAccessory accessory,uint dataId)
        {
            return accessory.GetEntitiesByDataId(dataId).Select(obj => (obj?.EntityId) ?? 0).ToList();
        }
        public static Dictionary<uint,IGameObject?> GetEntitiesByIdsList(this ScriptAccessory accessory,List<uint> idsList)
        {
            Dictionary<uint,IGameObject?> dict = new Dictionary<uint,IGameObject?>();
            foreach (uint id in idsList){
                dict[id] = accessory.Data.Objects.SearchByEntityId(id);
            }
            return dict;
        }

        //Get information about a specific status on a specific entity
        public static Status? GetStatusInfo(this ScriptAccessory accessory,uint entityId, uint statusId)
        {
            Status statusInfo = null;

            if(accessory.Data.Objects.SearchByEntityId(entityId) is IBattleChara entityObject)
            {
                unsafe
                {
                    FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* charaStruct = (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)entityObject.Address;
                    StatusList statusList = StatusList.CreateStatusListReference((nint)(charaStruct->GetStatusManager()));
                    foreach (Status status in statusList)
                    {
                        if (status.StatusId == statusId)
                        {
                            statusInfo = status;
                            break;
                        }
                    }
                }

            }
            return statusInfo;
        }

        //Get list of entities holding a specific status by statusId
        public static List<uint> whoGetStatus(this ScriptAccessory accessory,uint statusId)
        {
            List<uint> objectIdsList = new List<uint>();
            foreach (IGameObject entityObject in accessory.Data.Objects)
            {  //IBattleChara
                //subKind 4(Player) 9(Battle NPC) 5,11(Enemy)
                if (Array.Exists(new byte[] { 4, 9, 5, 11 }, subKind => subKind == entityObject.SubKind))
                {
                    if(accessory.GetStatusInfo(entityObject.EntityId,statusId) != null)
                    {
                        objectIdsList.Add(entityObject.EntityId);
                    }
                }
            }
            return objectIdsList;
        }
        public static bool isEntityGetStatus(this ScriptAccessory accessory,uint entityId,uint statusId)
        {
            return (accessory.whoGetStatus(statusId) ?? new List<uint>()).Contains(entityId);
        }
        public static bool isMeGetStatus(this ScriptAccessory accessory,uint statusId)
        {
            return accessory.isEntityGetStatus(accessory.Data.Me,statusId);
        }
        public static List<uint> whoGetStatusInParty(this ScriptAccessory accessory,uint statusId)
        {
            return accessory.whoGetStatus(statusId).Intersect(accessory.Data.PartyList).ToList();
        }
        public static List<uint> whoGetStatusInPartyWithoutMe(this ScriptAccessory accessory,uint statusId)
        {
            return accessory.whoGetStatusInParty(statusId).Except(new List<uint> { accessory.Data.Me }).ToList();
        }
        public static List<uint> whoNotGetStatusInParty(this ScriptAccessory accessory,uint statusId)
        {
            return accessory.Data.PartyList.Except(accessory.whoGetStatusInParty(statusId)).ToList();
        }
        public static List<uint> whoNotGetStatusInPartyWithoutMe(this ScriptAccessory accessory,uint statusId)
        {
            return accessory.whoNotGetStatusInParty(statusId).Except(new List<uint> { accessory.Data.Me }).ToList();
        }

        //Color, the 4 parameters inside color are R,G,B,density (color density, not transparency)
        public enum ColorType {
            Red,Pink,Cyan,Orange
        }
        private static readonly Dictionary<ColorType, ScriptColor> colors = new Dictionary<ColorType, ScriptColor>
        {
            { ColorType.Red, new ScriptColor { V4 = new Vector4(1.0f, 0f, 0f, 1.0f) } },
            { ColorType.Pink, new ScriptColor { V4 = new Vector4(1f, 0f, 1f, 1.0f) } },
            { ColorType.Cyan, new ScriptColor { V4 = new Vector4(0f, 1f, 0.8f, 1.0f) } },
            { ColorType.Orange, new ScriptColor { V4 = new Vector4(1f, 0.8f, 0f, 1.5f) } },
        };
        public static ScriptColor GetColor(this ScriptAccessory accessory, ColorType colorType)
        {
            return colors[colorType];
        }
        //Quick drawing parameters and quick drawing

        public static DrawPropertiesEdit GetDrawPropertiesEdit(this ScriptAccessory accessory,string name,object position, Vector2 scale, long delay, long destoryAt,Vector4 color)
        {
            DrawPropertiesEdit drawPropertiesEdit= accessory.Data.GetDefaultDrawProperties();
            drawPropertiesEdit.Name = name;
            switch (position)
            {
                case Vector3 position_v3:
                    drawPropertiesEdit.Position = position_v3;
                    break;
                case ulong position_id_ulong:
                    drawPropertiesEdit.Owner = position_id_ulong;
                    drawPropertiesEdit.Position = null;
                    break;
                case uint position_id:
                    drawPropertiesEdit.Owner = position_id;
                    drawPropertiesEdit.Position = null;
                    break;
                default:
                    accessory.Log.Debug($"parm type error : position =>{position}");
                    break;
            }
            drawPropertiesEdit.Scale = scale;
            drawPropertiesEdit.Delay = delay;
            drawPropertiesEdit.DestoryAt = destoryAt;
            drawPropertiesEdit.Color = color;
            //drawPropertiesEdit.TargetColor = targetColor;
            return drawPropertiesEdit;
        }
        public static DrawPropertiesEdit GetDrawPropertiesEdit(this ScriptAccessory accessory,object position, Vector2 scale, long destoryAt, bool isSafe)
        {
            return accessory.GetDrawPropertiesEdit(Guid.NewGuid().ToString(),position,scale,0,destoryAt
            ,isSafe?accessory.Data.DefaultSafeColor:accessory.Data.DefaultDangerColor);
        }
        public static void FastDraw(this ScriptAccessory accessory,DrawTypeEnum drawType,object position,Vector2 scale, Vector2 delay_destoryAt, bool isSafe){
            DrawPropertiesEdit drawPropertiesEdit = accessory.GetDrawPropertiesEdit(position,scale,(long)delay_destoryAt.Y,isSafe);
            drawPropertiesEdit.Delay = (long)delay_destoryAt.X;
            if(drawType == DrawTypeEnum.Displacement && (position is Vector3)){
                //If drawing a direction indicator and position is Vector3
                drawPropertiesEdit.Owner = accessory.Data.Me;
                drawPropertiesEdit.Position = null;
                drawPropertiesEdit.ScaleMode |= ScaleMode.YByDistance;
                drawPropertiesEdit.TargetPosition = (Vector3)position;
            }
            //accessory.Log.Debug($"FastDraw {drawType.ToString()} :{drawPropertiesEdit.ToString()}");
            accessory.Method.SendDraw(DrawModeEnum.Default, drawType, drawPropertiesEdit);
        }
        public static void FastDraw(this ScriptAccessory accessory,DrawTypeEnum drawType,object position,Vector2 scale, Vector2 delay_destoryAt, Vector4 color){
            DrawPropertiesEdit drawPropertiesEdit = accessory.GetDrawPropertiesEdit(position,scale,(long)delay_destoryAt.Y,true);
            drawPropertiesEdit.Delay = (long)delay_destoryAt.X;
            if(drawType == DrawTypeEnum.Displacement && (position is Vector3)){
                //If drawing a direction indicator and position is Vector3
                drawPropertiesEdit.Owner = accessory.Data.Me;
                drawPropertiesEdit.Position = null;
                drawPropertiesEdit.ScaleMode |= ScaleMode.YByDistance;
                drawPropertiesEdit.TargetPosition = (Vector3)position;
            }
            drawPropertiesEdit.Color = color;
            //accessory.Log.Debug($"FastDraw {drawType.ToString()} :{drawPropertiesEdit.ToString()}");
            accessory.Method.SendDraw(DrawModeEnum.Default, drawType, drawPropertiesEdit);
        }
        public static void FastDrawDisplacement(this ScriptAccessory accessory,Vector3[] twoPosition,Vector2 scale, long destoryAt, Vector4 color)
        {
            if(twoPosition.Length == 2){
                DrawPropertiesEdit drawPropertiesEdit = accessory.GetDrawPropertiesEdit(twoPosition[0],scale,destoryAt,true);
                drawPropertiesEdit.TargetPosition = twoPosition[1];
                drawPropertiesEdit.Color = color;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, drawPropertiesEdit);
            }
        }
        public static void DrawWaypoints(this ScriptAccessory accessory,List<float[]> pointsList,Vector2 width_radius, long baseDelayMillis, Vector4 color_goNow, Vector4 color_goLater)
        {
            long guideStartTimeMillis = baseDelayMillis;
            string guid = Guid.NewGuid().ToString();
            for (int i = 0; i < pointsList.Count; i++)
            {
                if(pointsList[i].Length < 5){
                    accessory.Log.Debug($"pointsList[{i}]'s length < 5");
                    break;
                }
                int count = 0;
                string name = $"DrawWaypoints go now {i} : {guid}";

                //go now part
                DrawPropertiesEdit drawPropertiesEdit_goNow = accessory.GetDrawPropertiesEdit(
                    name + count++
                    ,accessory.Data.Me
                    ,new (width_radius.X)
                    ,guideStartTimeMillis + (int)pointsList[i][3] - Math.Sign(i) * 270
                    ,(int)pointsList[i][4] - 100
                    ,color_goNow);
                drawPropertiesEdit_goNow.ScaleMode |= ScaleMode.YByDistance;
                drawPropertiesEdit_goNow.TargetPosition = new Vector3(pointsList[i][0], pointsList[i][1], pointsList[i][2]);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, drawPropertiesEdit_goNow);
            
                if(width_radius.Y > 0)
                {
                    DrawPropertiesEdit drawPropertiesEdit_goNowEndCircle = accessory.GetDrawPropertiesEdit(
                    name + count++
                    ,drawPropertiesEdit_goNow.TargetPosition
                    ,new(width_radius.Y)
                    ,drawPropertiesEdit_goNow.Delay
                    ,drawPropertiesEdit_goNow.DestoryAt
                    ,color_goNow);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, drawPropertiesEdit_goNowEndCircle);
                }
                //If the current point is not the first point, perform the go later part
                if( i >= 1)
                {
                    DrawPropertiesEdit drawPropertiesEdit_goLater = accessory.GetDrawPropertiesEdit(
                        name + count++
                        ,new Vector3 (pointsList[i - 1][0], pointsList[i - 1][1], pointsList[i - 1][2])
                        ,new (width_radius.X)
                        ,baseDelayMillis + (int)pointsList[0][3]
                        ,guideStartTimeMillis - (baseDelayMillis + (int)pointsList[0][3]) - 100
                        ,color_goLater);
                    drawPropertiesEdit_goLater.TargetPosition = new Vector3 (pointsList[i][0], pointsList[i][1], pointsList[i][2]);
                    drawPropertiesEdit_goLater.ScaleMode |= ScaleMode.YByDistance;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, drawPropertiesEdit_goLater);

                    if(width_radius.Y > 0)
                    {
                        DrawPropertiesEdit drawPropertiesEdit_goLaterEndCircle = accessory.GetDrawPropertiesEdit(
                        name + count++
                        ,drawPropertiesEdit_goLater.TargetPosition
                        ,new(width_radius.Y)
                        ,drawPropertiesEdit_goLater.Delay
                        ,drawPropertiesEdit_goLater.DestoryAt - 100
                        ,color_goLater);
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, drawPropertiesEdit_goLaterEndCircle);
                    }
                }
                //Prepare the start time node for the next point (guideStartTimeMillis is the end time point for all guidance of the current point)
                guideStartTimeMillis = guideStartTimeMillis + (int)pointsList[i][3] + (int)pointsList[i][4];
            }
        }

        /*
        Draw a set of facing mechanism indicators
        1. The final facing target area indicator is composed of 1 donut drawing + 1 fan drawing + 2 line drawings, resembling a pizza slice but with the middle hollowed out
        2. The current facing indicator is composed of 1 donut drawing, forming the middle fan-shaped part that is hollowed out from the pizza
        3. Can the current facing sector color have two different colors depending on whether it is in a safe zone or not?
        */ 
        public static void DrawTurnTowards(this ScriptAccessory accessory,object position,Vector3 towardsDonutScale_radAndRotation, Vector2 palyerDonutScale,Vector2 delay_destoryAt,Vector4 color,bool palyerDonutOn)
        {
            //1. Draw the hollowed pizza part
            //The radian increase direction in KOD seems to follow the Cartesian coordinate system's radian increase direction, which is opposite to the FFXIV coordinate system's radian increase direction
            Vector3 pizzaDp = towardsDonutScale_radAndRotation;
            DrawPropertiesEdit dptt1 = accessory.GetDrawPropertiesEdit(position,new (pizzaDp.X),(long)delay_destoryAt.Y,true);
            dptt1.Owner = accessory.Data.Me;
            dptt1.Delay = (long)delay_destoryAt.X;
            dptt1.InnerScale = new (palyerDonutScale.X);
            dptt1.Radian = pizzaDp.Y;
            dptt1.Rotation = - pizzaDp.Z;
            dptt1.Color = color;
            switch (position)
            {
                case Vector3 position_v3:
                    dptt1.Position = null;
                    dptt1.TargetPosition = position_v3;
                    break;
                case uint position_id:
                    dptt1.TargetObject = position_id;
                    break;
                default:
                    accessory.Log.Debug($"parm type error : position =>{position}");
                    break;
            }
            accessory.Method.SendDraw(DrawModeEnum.Vfx, DrawTypeEnum.Donut, dptt1);
            dptt1.Scale = new (palyerDonutScale.Y);
            accessory.Method.SendDraw(DrawModeEnum.Vfx, DrawTypeEnum.Fan, dptt1);
            dptt1.Scale = new (2,pizzaDp.X);
            dptt1.Rotation = - pizzaDp.Z + 0.5f * pizzaDp.Y;
            accessory.Method.SendDraw(DrawModeEnum.Vfx, DrawTypeEnum.Line, dptt1);
            dptt1.Rotation = - pizzaDp.Z - 0.5f * pizzaDp.Y;
            accessory.Method.SendDraw(DrawModeEnum.Vfx, DrawTypeEnum.Line, dptt1);

            //Draw the player's facing sector part, sometimes a mechanism has multiple guiding facings
            if(palyerDonutOn){
                DrawPropertiesEdit dptt2 = accessory.GetDrawPropertiesEdit(accessory.Data.Me,new (palyerDonutScale.X),(long)delay_destoryAt.Y,true);
                dptt2.Delay = (long)delay_destoryAt.X;
                dptt2.InnerScale = new (palyerDonutScale.Y);
                dptt2.Radian = pizzaDp.Y - 0.02f;
                dptt2.Color = color;
                accessory.Method.SendDraw(DrawModeEnum.Vfx, DrawTypeEnum.Donut, dptt2);
            }
        }
    }
    #endregion

    #region Utility Classes
    public static class TsingUtilities
    {

        
        public static float DistanceBetweenTwoPoints(Vector3 point1,Vector3 point2)
        {
            float x = point1.X - point2.X;
            float y = point1.Y - point2.Y;
            float z = point1.Z - point2.Z;
            return MathF.Sqrt(MathF.Pow(x, 2) + MathF.Pow(y, 2) + MathF.Pow(z, 2));
        }

        public static float DistanceBetweenTwoPoints(Vector2 point1,Vector2 point2)
        {
            float x = point1.X - point2.X;
            float y = point1.Y - point2.Y;
            return MathF.Sqrt(MathF.Pow(x, 2) + MathF.Pow(y, 2));
        }

        


        //Rotate angle in FFXIV coordinate system. In FFXIV coordinate system, clockwise direction increases radian value, counterclockwise direction decreases radian value
        /*
        The coordinate system in FFXIV is different from the Cartesian coordinate system.
        In FFXIV, the X-axis direction is from left to right, and the Y-axis direction is from top to bottom (the third parameter of the in-game coordinate is the vertical coordinate, not the vertical axis)
        In the Cartesian coordinate system, the X-axis direction is from left to right, and the Y-axis direction is from bottom to top.
        This means that in FFXIV, the quadrant distribution is:
        Third Quadrant | Fourth Quadrant
        ------------------
        Second Quadrant | First Quadrant
        The clockwise direction in the Cartesian coordinate system is opposite to the clockwise direction in the FFXIV coordinate system <= because the Y-axis is inverted
        */
        public static Vector2 RotatePoint(Vector2 point, Vector2 centre, float radian)
        {
            Vector2 centreToPoint_v2 = new(point.X - centre.X, point.Y - centre.Y);
            float rot = (MathF.Atan2(centreToPoint_v2.Y, centreToPoint_v2.X) + radian);
            float length = centreToPoint_v2.Length();
            return new(centre.X + MathF.Cos(rot) * length, centre.Y + MathF.Sin(rot) * length);
        }
        public static Vector2 RotatePoint(Vector2 point, Vector2 centre, double radian)
        {
            return RotatePoint(point, centre, (float)radian);
        }
        public static Vector2 RotatePoint(Vector2 point, float radian)
        {
            return RotatePoint(point, new(0,0),radian);
        }
        public static Vector2 RotatePoint(Vector2 point,double radian)
        {
            return RotatePoint(point, new(0,0),radian);
        }


        public static Vector3 RotatePointInFFXIVCoordinate(Vector3 point, Vector3 centre, double radian)
        {
            Vector2 centreToPoint_v2 = new(point.X - centre.X, point.Z - centre.Z);
            float rot = (float)(MathF.Atan2(centreToPoint_v2.Y, centreToPoint_v2.X) + radian);
            float length = centreToPoint_v2.Length();
            return new(centre.X + MathF.Cos(rot) * length, centre.Y, centre.Z + MathF.Sin(rot) * length);
        }
        public static Vector3 RotatePointInFFXIVCoordinate(Vector3 point, float[] centre, double radian)
        {
            Vector3 centre_v3 = new(centre[0], centre[1], centre[2]);
            return RotatePointInFFXIVCoordinate(point, centre_v3, radian);
        }
        public static float[] RotatePointInFFXIVCoordinate(float[] point, Vector3 centre, double radian)
        {
            Vector3 point_v3 = new(point[0], point[1], point[2]);
            Vector3 resultPoint = RotatePointInFFXIVCoordinate(point_v3, centre, radian);
            return new float[] { resultPoint.X, resultPoint.Y, resultPoint.Z };
        }
        public static float[] RotatePointInFFXIVCoordinate(float[] point, float[] centre, double radian)
        {
            Vector3 point_v3 = new(point[0], point[1], point[2]);
            Vector3 centre_v3 = new(centre[0], centre[1], centre[2]);
            Vector3 resultPoint = RotatePointInFFXIVCoordinate(point_v3, centre_v3, radian);
            return new float[] { resultPoint.X, resultPoint.Y, resultPoint.Z };
        }




        //Point axial symmetry

        public static Vector2 AxisymmetricPoint(Vector2 point, float rot)
        {
            float rotPoint = MathF.Atan2(point.Y,point.X);
            float radian = 2 * rot - 2 * rotPoint;
            return RotatePoint(point,new Vector2(0,0),radian);
        }
        public static Vector2 AxisymmetricPoint(Vector2 point, double rot)
        {
            return AxisymmetricPoint(point, (float)rot);
        }
        
        public static Vector2 AxisymmetricPoint(Vector2 point, Vector2 axis)
        {
            return AxisymmetricPoint(point, MathF.Atan2(axis.Y,axis.X));
        }

        public static Vector2 AxisymmetricPointByX(Vector2 point)
        {
            return AxisymmetricPoint(point,0);
        }

        public static Vector2 AxisymmetricPointByY(Vector2 point)
        {
            return AxisymmetricPoint(point,Math.PI);
        }
        

        public static Vector3 AxisymmetricPointInFFXIVCoordinate(Vector3 point, Vector3 axis)
        {
            Vector2 point_v2 = new(point.X, point.Z);
            Vector2 axis_v2 = new(axis.X, axis.Z);
            Vector2 resultPoint_v2 = AxisymmetricPoint(point_v2, axis_v2);
            return new(resultPoint_v2.X, point.Y, resultPoint_v2.Y);
        }

        public static Vector3 AxisymmetricPointInFFXIVCoordinate(Vector3 point, float[] axis)
        {
            Vector3 axis_v3 = new(axis[0], axis[1], axis[2]);
            return AxisymmetricPointInFFXIVCoordinate(point, axis_v3);
        }

        public static float[] AxisymmetricPointInFFXIVCoordinate(float[] point, Vector3 axis)
        {
            Vector3 point_v3 = new(point[0], point[1], point[2]);
            Vector3 resultPoint = AxisymmetricPointInFFXIVCoordinate(point_v3, axis);
            return new float[] { resultPoint.X, resultPoint.Y, resultPoint.Z };
        }
        public static float[] AxisymmetricPointInFFXIVCoordinate(float[] point, float[] axis)
        {
            Vector3 point_v3 = new(point[0], point[1], point[2]);
            Vector3 axis_v3 = new(axis[0], axis[1], axis[2]);
            Vector3 resultPoint = AxisymmetricPointInFFXIVCoordinate(point_v3, axis_v3);
            return new float[] { resultPoint.X, resultPoint.Y, resultPoint.Z };
        }


        //Create a trigger cooldown
        // When using async and await keywords, the compiler generates a state machine to handle asynchronous operations. This causes method names to change during debugging and runtime. Please try NOT to use MethodBase.GetCurrentMethod().Name as a way to get the method name.
        public static bool IsInSuppress(ConcurrentDictionary<string,long> dict, string methodName,long suppressMillisecond)
        {
            //1. Get the last trigger timestamp
            //2. Compare with current timestamp
            //3. If the time difference is greater than the suppress time, it means not in suppress period, return false
            //4. If the time difference is less than or equal to the suppress time, it means in suppress period, return true
            bool isIn = false;
            if (dict.TryGetValue(methodName, out long result))
            {
                //If the last trigger timestamp is found, compare
                isIn = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - result <= suppressMillisecond;

                //If not in suppress period, update the trigger timestamp
                if(!isIn){
                    dict[methodName] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }
            else 
            {
                //If the last trigger timestamp is not found, add a timestamp record
                //Since this is usually used in concurrent environments
                //If not found and cannot add, it means another concurrent thread completed the addition first, this concurrent thread is invalid
                isIn = !dict.TryAdd(methodName, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            }
            return isIn;
        }

        //Create a delay function with cancellation capability, when interrupted, it returns true;
        public static async Task<bool> DelayMillisecond(int delayMillisecond, CancellationToken cancellationToken)
        {
            try{
                //Wait for delayMillisecond
                await Task.Delay(delayMillisecond,cancellationToken);
                return false;
            }catch(TaskCanceledException){
                //If cancelled
                return true;
            }catch(Exception){
                //Canceled by other unexpected circumstances
                return true;
                // return true;
            }
        }
    }
    #endregion
}
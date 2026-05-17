using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.Draw.Manager;
using KodakkuAssist.Data;
// using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;
using System.Xml.Linq;
using Dalamud.Utility.Numerics;
// using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using KodakkuAssist.Module.GameOperate;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace MyScriptNamespace
{
    
    [ScriptType(name: "Omega Protocol Ultimate Deluxe Edition", territorys: [1122], guid: "e0bfb4db-0d38-909f-5088-b23f09b7585e", version:"0.0.0.19", Author: "Linoa235", note: noteStr, updateInfo: UpdateInfo)]
    public class OmegaProtocolUltimate
    {
        const string noteStr =
        """
        Omega Protocol Ultimate (Based on K's original script with added P5 second/third trios, P6 guidance)
        Thanks to Usami for P5 first trio guidance
        """;
        
        private const string UpdateInfo =
            """
            1. Attempted fix for P6 Exaflare Wave Cannon guidance issue.
            """;

        [UserSetting("P3_Opening Queue Order")]
        public P3SortEnum P3_StackSort { get; set; }
        [UserSetting("P3_Small TV Strategy")]
        public P3TVEnum P3_TV_Strategy { get; set; }
        
        [UserSetting("Cosmo Arrow Color")]
        public ScriptColor ArrorColor { get; set; } = new() { V4 = new(1, 0, 0, 1) };
        [UserSetting("First Cosmo Arrow Follow Crowd")]
        public bool followCrowd { get; set; } = true;
        [UserSetting("Arrow Thickness")]
        public int ArrowScale { get; set; } = 1;    

        List<int> HtdhParty = [2, 0, 1, 4, 5, 6, 7, 3];
        double parse = 0;

        uint P1_BossId = 0;
        List<int> P1_MarkedBuffs = [0, 0, 0, 0, 0, 0, 0, 0];
        List<Vector3> P1_TowerPos = [];
        DateTime P1_TowerTime= DateTime.MinValue;
        DateTime P1_FanTime = DateTime.MinValue;
        int P1_LineRound = 0;
        int P1_FireCount = 0;

        bool P2_PTBuffIsFar=false;
        List<int> P2_Sony= [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P2_Stack = [0, 0, 0, 0, 0, 0, 0, 0];
        Dictionary<uint,uint> P2_SwordDanceTethers = [];

        int P3_ArmCount = 0;
        List <int> P3_StartBuff= [0, 0, 0, 0, 0, 0, 0, 0];
        bool P3_StartPreDone = false;
        bool P3_StartDone=false;
        List<int> P3_TVBuff = [0, 0, 0, 0, 0, 0, 0, 0];

        List<int> P4Stack = [];

        int P51_Eye = 0;
        List<int> P51_Buff = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P51_Fist = [0, 0, 0, 0];
        bool P51_FistDone = false;


        int P52_OmegaMDir = 0;
        bool P52_OmegaFDirDone = false;
        int P52_OmegaFDir = 0;
        bool P5_SigmaBuffIsFar = false;
        Vector3 P52_Self_Pos;
        int P52_Self_Dir = 0;
        int[] P52_Towers = new int[16]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        Vector3[] P52_TowerPos = new Vector3[16];
        AutoResetEvent P52_semaphoreTowersWereConfirmed = new (false);
        private byte? P52_F_TransformationID;
        int P52_MarkType;
        int P53_MarkType;
        float P53_4_HW;
        int P5_3_MF = 0;
        private bool P5_TV_Support_enable = false;
        public bool P52_OmegaM_Skill = false;
        
        public int P5_3_MFT = 0; // Which pair of M/F
        public List<int> MFTransformStates = [0,0,0,0]; // Transformation state list
        public List<int> MFPositions = [0,0,0,0];       // Position list
        public int FPos1, FPos2;                    // F's two positions
        public int Combo1, Combo2;                  // Combo skill types
        
        public enum Pattern { Unknown, InOut, OutIn }
        private Pattern _curPattern = Pattern.Unknown;
        private Vector3 MapCenter = new(100.0f, 0.0f, 100.0f);
        private int ArrowNum = 0;
        // private int CannonNum = 0;
        private bool CannonCasted = false;
		private readonly object CannonNumLock = new object();
        public Vector3[] StepCannon = new Vector3[4];
        public int StepCannonIndex = 0;
        private bool isSet = false;
        System.Threading.AutoResetEvent ArrowModeConfirmed = new System.Threading.AutoResetEvent(false);
        // 0 for cross first, 1 for outer ring first
        public int arrowMode = -1;
        const string InOut = "InOut";
        const string OutIn = "OutIn";
        
        private static readonly Dictionary<(int, int, int, int), Vector3> P53SafePos = 
    new Dictionary<(int, int, int, int), Vector3>
    {
        {(0, 3, 1, 1), new Vector3(100, 0, 81)}, //A far
        {(0, 0, 2, 1), new Vector3(100, 0, 81)}, //A far
        {(0, 3, 1, 0), new Vector3(119, 0, 100)}, //B far
        {(0, 2, 0, 0), new Vector3(119, 0, 100)}, //B far
        {(0, 1, 3, 1), new Vector3(100, 0, 119)}, //C far
        {(0, 2, 0, 1), new Vector3(100, 0, 119)}, //C far
        {(0, 1, 3, 0), new Vector3(81, 0, 100)}, //D far
        {(0, 0, 2, 0), new Vector3(81, 0, 100)}, //D far
        {(2, 1, 3, 1), new Vector3(100, 0, 95.5f)}, //A near
        {(2, 2, 0, 1), new Vector3(100, 0, 95.5f)}, //A near
        {(3, 3, 1, 1), new Vector3(100, 0, 95.5f)}, //A near
        {(3, 0, 2, 1), new Vector3(100, 0, 95.5f)}, //A near
        {(2, 1, 3, 0), new Vector3(104.5f, 0, 100)}, //B near
        {(2, 0, 2, 0), new Vector3(104.5f, 0, 100)}, //B near
        {(3, 3, 1, 0), new Vector3(104.5f, 0, 100)}, //B near
        {(3, 2, 0, 0), new Vector3(104.5f, 0, 100)}, //B near
        {(2, 3, 1, 1), new Vector3(100, 0, 104.5f)}, //C near
        {(2, 0, 2, 1), new Vector3(100, 0, 104.5f)}, //C near
        {(3, 1, 3, 1), new Vector3(100, 0, 104.5f)}, //C near
        {(3, 2, 0, 1), new Vector3(100, 0, 104.5f)}, //C near
        {(2, 3, 1, 0), new Vector3(95.5f, 0, 100)}, //D near
        {(2, 2, 0, 0), new Vector3(95.5f, 0, 100)}, //D near
        {(3, 1, 3, 0), new Vector3(95.5f, 0, 100)}, //D near
        {(3, 0, 2, 0), new Vector3(95.5f, 0, 100)}, //D near
        {(1, 3, 1, 1), new Vector3(100, 0, 88)}, //A mid
        {(1, 0, 2, 1), new Vector3(100, 0, 88)}, //A mid
        {(1, 3, 1, 0), new Vector3(112, 0, 100)}, //B mid
        {(1, 2, 0, 0), new Vector3(112, 0, 100)}, //B mid
        {(1, 1, 3, 1), new Vector3(100, 0, 112)}, //C mid
        {(1, 2, 0, 1), new Vector3(100, 0, 112)}, //C mid
        {(1, 1, 3, 0), new Vector3(88, 0, 100)}, //D mid
        {(1, 0, 2, 0), new Vector3(88, 0, 100)}, //D mid
    };

        public enum P3SortEnum
        {
            HTDH,
            THD
        }
        public enum P3TVEnum
        {
            Normal,
            Static
        }
        
        private enum TopPhase
        {
            Init,                   // Initial
            P5A1_DeltaVersion,        // P5 First Trio
            P5A2_DeltaWorld,          // P5 Second Pass
            P5B1_SigmaVersion,        // P5 Second Trio
            P5B2_SigmaWorld,          // P5 Second Pass
            P5C1_OmegaVersion,        // P5 Third Trio
            P5C2_OmegaWorldA,         // P5 Third Pass
            P5C3_OmegaWorldB,         // P5 Fourth Pass
            P5D_BlindFaith,          // P5 Blind Faith
        }
        private static List<string> _role = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
        private const bool Debugging = false;
        private static readonly Vector3 Center = new Vector3(100, 0, 100);
        private static TopPhase _phase = TopPhase.Init;
        private volatile List<bool> _bools = new bool[20].ToList();
        private List<int> _numbers = Enumerable.Repeat(0, 20).ToList();
        private static List<ManualResetEvent> _events = Enumerable
            .Range(0, 20)
            .Select(_ => new ManualResetEvent(false))
            .ToList();
    
        private static PriorityDict _pd = new PriorityDict();       // Flexible multi-purpose dictionary

        public void Init(ScriptAccessory sa)
        {
            parse = 0;
            arrowMode = -1;
            ArrowNum = 0;
            // CannonNum = 0;
            CannonCasted = false;
            StepCannonIndex = 0;
			StepCannon = new Vector3[4];
            P5_TV_Support_enable = false;
            P52_OmegaMDir = 0;
            P52_OmegaFDirDone = false;
            P52_OmegaFDir = 0;
            P52_OmegaM_Skill = false;
			MFTransformStates = [0,0,0,0]; 
        	MFPositions = [0,0,0,0];
			Array.Clear(P52_Towers, 0, P52_Towers.Length);
			P5_3_MFT = 0;
            ArrowModeConfirmed = new System.Threading.AutoResetEvent(false);
            InitParams();
            _phase = TopPhase.Init;
            sa.Method.RemoveDraw(".*");
        }
        
        private void InitParams()
        {
            _bools = new bool[20].ToList();
            _numbers = Enumerable.Repeat(0, 20).ToList();
            _events = Enumerable
                .Range(0, 20)
                .Select(_ => new ManualResetEvent(false))
                .ToList();
        }
        
        #region P1
        [ScriptMethod(name: "P1_Loop_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31491"],userControl:false)]
        public void P1_Loop_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            P1_BossId=tid;
            parse = 1.1;
            P1_TowerPos = [];
            P1_LineRound = 0;
        }
        [ScriptMethod(name: "P1_Loop_BuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3004|3005|3006|3451)$"],userControl:false)]
        public void P1_Loop_BuffRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            P1_MarkedBuffs[index] = @event["StatusID"] switch
            {
                "3004" => 1,
                "3005" => 2,
                "3006" => 3,
                "3451" => 4,
                _=>0
            };
        }
        [ScriptMethod(name: "P1_Loop_TowerCollection", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Add", "DataId:2013245"], userControl: false)]
        public void P1_Loop_TowerCollection(Event @event, ScriptAccessory accessory)
        {
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            lock (P1_TowerPos)
            {
                P1_TowerPos.Add(pos);
            }
        }
        [ScriptMethod(name: "P1_Loop_StackReminder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31491"])]
        public void P1_Loop_StackReminder(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            accessory.Method.TextInfo("Stack behind boss", 2000);
            accessory.Method.TTS("Stack behind boss");
        }
        [ScriptMethod(name: "P1_Loop_StartPositionReminder", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3004|3005|3006|3451)$"])]
        public void P1_Loop_StartPositionReminder(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            if (@event["StatusID"]=="3006")
            {
                accessory.Method.TextInfo("Forward to tether", 3000);
                accessory.Method.TTS("Forward to tether");
            }
            else
            {
                accessory.Method.TextInfo("Back", 3000);
                accessory.Method.TTS("Back");
            }
           
        }
        [ScriptMethod(name: "P1_Loop_LineTowerHandlingPosition", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Add", "DataId:2013245"])]
        public async void P1_Loop_LineTowerHandlingPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            lock (this)
            {
                if ((DateTime.Now - P1_TowerTime).TotalSeconds < 2) return;
                P1_TowerTime=DateTime.Now;
            }
            await Task.Delay(50);
            Vector3 centre = new(100, 0, 100);
            var towerCount = P1_TowerPos.Count;
            List<int> HtdhParty = [2, 0, 1, 4, 5, 6, 7, 3];
            var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var myBuff = P1_MarkedBuffs[myindex];
            var index1 = P1_MarkedBuffs.IndexOf(myBuff);
            var index2 = P1_MarkedBuffs.LastIndexOf(myBuff);
            var hIndex = HtdhParty.IndexOf(index1) < HtdhParty.IndexOf(index2) ? index1 : index2;
            var meIsHigh = hIndex == myindex;
            var idle = false;
            //Tower
            if (towerCount == myBuff * 2)
            {
                idle=true;
                var hPos=default(Vector3);
                var lPos=default(Vector3);
                if (RoundPositionTo4Dir(P1_TowerPos[towerCount - 2], centre) < RoundPositionTo4Dir(P1_TowerPos[towerCount - 1], centre))
                {
                    hPos = P1_TowerPos[towerCount - 2];
                    lPos = P1_TowerPos[towerCount - 1];
                }
                else
                {
                    hPos = P1_TowerPos[towerCount - 1];
                    lPos = P1_TowerPos[towerCount - 2];
                }
                var dealpos = meIsHigh?hPos:lPos;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_Loop_TowerPosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_Loop_TowerRange";
                dp.Scale = new(3);
                dp.Position = dealpos;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            //Line
            if (towerCount % 8 == (myBuff + 2) * 2 % 8)
            {
                
                idle = true;
                List<int> isTower = [0, 0, 0, 0];
                isTower[RoundPositionTo4Dir(P1_TowerPos[towerCount - 2], centre)] = 1;
                isTower[RoundPositionTo4Dir(P1_TowerPos[towerCount - 1], centre)] = 1;
                var my4Dir = meIsHigh ? isTower.IndexOf(0) : isTower.LastIndexOf(0);
                var dealpos = RotatePoint(new(100, 0, 85), centre, float.Pi / 2 * my4Dir);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_Loop_LinePosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            //Idle
            if (!idle)
            {
                //North point 100,0,86
                var myPos = accessory.Data.Objects.SearchByEntityId(accessory.Data.Me)?.Position??default;
                var drot = (myPos - P1_TowerPos[towerCount - 2]).Length() < (myPos - P1_TowerPos[towerCount - 1]).Length() ? RoundPositionTo4Dir(P1_TowerPos[towerCount - 2],centre) : RoundPositionTo4Dir(P1_TowerPos[towerCount - 1], centre);
                var dealpos=RotatePoint(new(100,0,86),centre,float.Pi/2*drot);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_Loop_IdlePosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        [ScriptMethod(name: "P1_Loop_TetherMarker", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31496", "TargetIndex:1"])]
        public void P1_Loop_TetherMarker(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            P1_LineRound++;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var waitBuff = (P1_LineRound + 1) % 4 + 1;
            var catchBuff = (P1_LineRound + 2) % 4 + 1;
            var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (catchBuff != P1_MarkedBuffs[myindex]) return;

            
            var myBuff = P1_MarkedBuffs[myindex];
            var index1 = P1_MarkedBuffs.IndexOf(myBuff);
            var index2 = P1_MarkedBuffs.LastIndexOf(myBuff);
            var hIndex = HtdhParty.IndexOf(index1) < HtdhParty.IndexOf(index2) ? index1 : index2;
            var meIsHigh = hIndex == myindex;

            var index3 = P1_MarkedBuffs.IndexOf(waitBuff);
            var index4 = P1_MarkedBuffs.LastIndexOf(waitBuff);
            var hWaitIndex = HtdhParty.IndexOf(index3) < HtdhParty.IndexOf(index4) ? index3 : index4;
            var lWaitIndex = HtdhParty.IndexOf(index3) < HtdhParty.IndexOf(index4) ? index4 : index3;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_Loop_TetherMarker";
            dp.Scale = new(10);
            dp.Owner = tid;
            dp.TargetObject = meIsHigh? accessory.Data.PartyList[hWaitIndex]: accessory.Data.PartyList[lWaitIndex];
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = new(1,1,0,1);
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
        }
        [ScriptMethod(name: "P1_Loop_TetherMarkerRemoval", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0059"])]
        public void P1_Loop_TetherMarkerRemoval(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (sid != accessory.Data.Me) return;
            accessory.Method.RemoveDraw("P1_Loop_TetherMarker");
        }

        [ScriptMethod(name: "P1_AlmightyMaster_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31499"], userControl: false)]
        public void P1_AlmightyMaster_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 1.2;
            P1_FireCount = 0;
        }
        [ScriptMethod(name: "P1_AlmightyMaster_BuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3004|3005|3006|3451)$"], userControl: false)]
        public void P1_AlmightyMaster_BuffRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            P1_MarkedBuffs[index] = @event["StatusID"] switch
            {
                "3004" => 1,
                "3005" => 2,
                "3006" => 3,
                "3451" => 4,
                _ => 0
            };
        }
        [ScriptMethod(name: "P1_AlmightyMaster_HighLowPriorityAnnounce", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31499"])]
        public async void P1_AlmightyMaster_HighLowPriorityAnnounce(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            await Task.Delay(100);
            var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var mybuff = P1_MarkedBuffs[myindex];
            var i1 = P1_MarkedBuffs.IndexOf(mybuff);
            var i2 = P1_MarkedBuffs.LastIndexOf(mybuff);
            var hIndex = HtdhParty.IndexOf(i1) < HtdhParty.IndexOf(i2) ? i1 : i2;
            if (hIndex== myindex)
            {
                accessory.Method.TextInfo("High Priority (Top/Right)", 10000);
                accessory.Method.TTS("High Priority (Top/Right)");
            }
            else
            {
                accessory.Method.TextInfo("Low Priority (Bottom/Left)", 10000);
                accessory.Method.TTS("Low Priority (Bottom/Left)");
            }
        }
        [ScriptMethod(name: "P1_AlmightyMaster_SingleTargetHitAnnounce", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31502"])]
        public async void P1_AlmightyMaster_SingleTargetHitAnnounce(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid == accessory.Data.Me) 
            {
                accessory.Method.TextInfo("Turn back", 2000);
                accessory.Method.TTS("Turn back");
            }
        }
        [ScriptMethod(name: "P1_AlmightyMaster_StackRange", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(350[789]|3510)$"])]
        public void P1_AlmightyMaster_StackRange(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_AlmightyMaster_StackRange";
            dp.Scale = new(6,30);
            dp.Owner = P1_BossId;
            dp.TargetObject = tid;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = dur - 3000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P1_AlmightyMaster_FarthestCleave", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"])]
        public void P1_AlmightyMaster_FarthestCleave(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            lock (this)
            {
                if ((DateTime.Now - P1_FanTime).TotalSeconds < 20) return;
                P1_FanTime = DateTime.Now;
            }
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_AlmightyMaster_FarthestCleave1";
            dp.Scale = new(20);
            dp.Radian = float.Pi / 3 * 2;
            dp.Owner = P1_BossId;
            dp.TargetResolvePattern=PositionResolvePatternEnum.PlayerFarestOrder;
            dp.TargetOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 13000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_AlmightyMaster_FarthestCleave2";
            dp.Scale = new(20);
            dp.Radian = float.Pi / 3 * 2;
            dp.Owner = P1_BossId;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            dp.TargetOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 13000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        [ScriptMethod(name: "P1_AlmightyMaster_MarkedLine", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"])]
        public void P1_AlmightyMaster_MarkedLine(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_AlmightyMaster_MarkedLine";
            dp.Scale = new(6,50);
            dp.TargetObject = tid;
            dp.Owner = P1_BossId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P1_AlmightyMaster_TankCleaveBaitPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32368"])]
        public void P1_AlmightyMaster_TankCleaveBaitPosition(Event @event, ScriptAccessory accessory)
        {
            lock (this)
            {
                P1_FireCount++;
                if (P1_FireCount != 26) return;
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                if (myindex != 0 && myindex != 1) return;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_AlmightyMaster_TankCleaveBaitPosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(100,0,86);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 11000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        #endregion

        #region P2
        [ScriptMethod(name: "P2_CooperativeProgramPT_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31550"], userControl: false)]
        public void P2_CooperativeProgramPT_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 2.1;
            P2_Stack = [];
        }
        [ScriptMethod(name: "P2_CooperativeProgramPT_BuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3427|3428)$"], userControl: false)]
        public void P2_CooperativeProgramPT_BuffRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            P2_PTBuffIsFar = @event["StatusID"] == "3428";
        }
        [ScriptMethod(name: "P2_CooperativeProgramPT_SonyRecord", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(01A[0123])$"], userControl: false)]
        public void P2_CooperativeProgramPT_SonyRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            lock (P2_Sony)
            {
                P2_Sony[accessory.Data.PartyList.IndexOf(tid)] = @event["Id"] switch
                {
                    "01A0" => 1,
                    "01A1" => 3,
                    "01A2" => 4,
                    "01A3" => 2,
                    _ => 0
                };
            }
            
        }
        [ScriptMethod(name: "P2_CooperativeProgramPT_MaleFemaleAOE", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7747", "SourceDataId:regex:^(15714|15715)$"])]
        public void P2_CooperativeProgramPT_MaleFemaleAOE(Event @event, ScriptAccessory accessory)
        {
            // 15714 Male
            // 15715 Female
            //Male sword 0 shield 4
            //Female staff 0 kick 4
            if (parse != 2.1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            Vector3 centre = new(100, 0, 100);
            if ((pos - centre).Length() > 12) return;
            var transformationID = GetTransformationID(sid, accessory);
            if (transformationID == null) return;
            if (@event["SourceDataId"] == "15714")
            {
                //Male
                if (transformationID == 0)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_CooperativeProgramPT_MaleCircle";
                    dp.Scale = new(10);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (transformationID == 4)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_CooperativeProgramPT_MaleDonut";
                    dp.Scale = new(40);
                    dp.InnerScale = new(10);
                    dp.Radian = float.Pi * 2;
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                }
            }
            if (@event["SourceDataId"] == "15715")
            {
                if (transformationID == 0)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_CooperativeProgramPT_FemaleCross1";
                    dp.Scale = new(10, 60);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_CooperativeProgramPT_FemaleCross2";
                    dp.Scale = new(10, 60);
                    dp.Rotation = float.Pi / 2;
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                }
                if (transformationID == 4)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_CooperativeProgramPT_FemaleWing1";
                    dp.Scale = new(60, 20);
                    dp.Owner = sid;
                    dp.Rotation = float.Pi / 2;
                    dp.Offset = new(-4, 0, 0);
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_CooperativeProgramPT_FemaleWing2";
                    dp.Scale = new(60, 20);
                    dp.Owner = sid;
                    dp.Rotation = float.Pi / -2;
                    dp.Offset = new(4, 0, 0);
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
            }
        }
        [ScriptMethod(name: "P2_CooperativeProgramPT_EyeLaser", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375AC", "Id:00020001"])]
        public void P2_CooperativeProgramPT_EyeLaser(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var rot = @event["Index"] switch
            {
                "00000001" => 0,
                "00000002" => 1,
                "00000003" => 2,
                "00000004" => 3,
                "00000005" => 4,
                "00000006" => 5,
                "00000007" => 6,
                "00000008" => 7,
                _ => -1
            };
            if (rot == -1) return;
            var pos = RotatePoint(new(100, 0, 80), new(100, 0, 100), float.Pi / 4 * rot);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_CooperativeProgramPT_EyeLaser";
            dp.Scale = new(16,40);
            dp.Position = pos;
            dp.TargetPosition = new(100, 0, 100);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7500;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

          
        }
        [ScriptMethod(name: "P2_CooperativeProgramPT_FiveCircles", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31521", "TargetIndex:1"])]
        public void P2_CooperativeProgramPT_FiveCircles(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            foreach (var c in accessory.Data.Objects)
            {
                if(c.DataId== 15714 || c.DataId == 15713)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_CooperativeProgramPT_FiveCircles";
                    dp.Scale = new(10);
                    dp.Owner = c.GameObjectId;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 11000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
        }
        [ScriptMethod(name: "P2_CooperativeProgramPT_EyeLaserSonyHandlingPosition", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375AC", "Id:00020001"])]
        public async void P2_CooperativeProgramPT_EyeLaserSonyHandlingPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dir = @event["Index"] switch
            {
                "00000001" => 0,
                "00000002" => 1,
                "00000003" => 2,
                "00000004" => 3,
                "00000005" => 4,
                "00000006" => 5,
                "00000007" => 6,
                "00000008" => 7,
                _ => -1
            };
            if (dir == -1) return;
            await Task.Delay(3000);
            Vector3 centre = new(100, 0, 100);

            Vector3 middleLeft1Pos =  RotatePoint(new(088.5f, 0, 085.5f), centre, float.Pi / 4 * dir);
            Vector3 middleRight1Pos = RotatePoint(new(111.5f, 0, 085.5f), centre, float.Pi / 4 * dir);
            Vector3 middleLeft2Pos = RotatePoint(new(088.5f, 0, 095.0f), centre, float.Pi / 4 * dir);
            Vector3 middleRight2Pos = RotatePoint(new(111.5f, 0, 095.0f), centre, float.Pi / 4 * dir);
            Vector3 middleLeft3Pos = RotatePoint(new(088.5f, 0, 105.0f), centre, float.Pi / 4 * dir);
            Vector3 middleRight3Pos = RotatePoint(new(111.5f, 0, 105.0f), centre, float.Pi / 4 * dir);
            Vector3 middleLeft4Pos = RotatePoint(new(088.5f, 0, 114.5f), centre, float.Pi / 4 * dir);
            Vector3 middleRight4Pos = RotatePoint(new(111.5f, 0, 114.5f), centre, float.Pi / 4 * dir);

            Vector3 farLeft1Pos = RotatePoint(new(091.5f, 0, 083.0f), centre, float.Pi / 4 * dir);
            Vector3 farRight1Pos = RotatePoint(new(108.5f, 0, 117.0f), centre, float.Pi / 4 * dir);
            Vector3 farLeft2Pos = RotatePoint(new(082.0f, 0, 093.0f), centre, float.Pi / 4 * dir);
            Vector3 farRight2Pos = RotatePoint(new(118.0f, 0, 107.0f), centre, float.Pi / 4 * dir);
            Vector3 farLeft3Pos = RotatePoint(new(082.0f, 0, 107.0f), centre, float.Pi / 4 * dir);
            Vector3 farRight3Pos = RotatePoint(new(118.0f, 0, 093.0f), centre, float.Pi / 4 * dir);
            Vector3 farLeft4Pos = RotatePoint(new(091.5f, 0, 117.0f), centre, float.Pi / 4 * dir);
            Vector3 farRight4Pos = RotatePoint(new(108.5f, 0, 083.0f), centre, float.Pi / 4 * dir);

            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var mySony = P2_Sony[myindex];
            var myPartnerIndex = P2_Sony.IndexOf(mySony) == myindex ? P2_Sony.LastIndexOf(mySony) : P2_Sony.IndexOf(mySony);
            var meIsHigh = HtdhParty.IndexOf(myindex) < HtdhParty.IndexOf(myPartnerIndex);
            Vector3 dealpos = mySony switch
            {
                1 => P2_PTBuffIsFar ? (meIsHigh ? farLeft1Pos : farRight1Pos) : (meIsHigh ? middleLeft1Pos : middleRight1Pos),
                2 => P2_PTBuffIsFar ? (meIsHigh ? farLeft2Pos : farRight2Pos) : (meIsHigh ? middleLeft2Pos : middleRight2Pos),
                3 => P2_PTBuffIsFar ? (meIsHigh ? farLeft3Pos : farRight3Pos) : (meIsHigh ? middleLeft3Pos : middleRight3Pos),
                4 => P2_PTBuffIsFar ? (meIsHigh ? farLeft4Pos : farRight4Pos) : (meIsHigh ? middleLeft4Pos : middleRight4Pos),
                _ => default
            };
            if (dealpos == default) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_CooperativeProgramPT_EyeLaserSonyHandlingPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P2_CooperativeProgramPT_StackHandlingPosition", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0064"])]
        public void P2_CooperativeProgramPT_StackHandlingPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            lock (P2_Stack)
            {
                P2_Stack.Add(accessory.Data.PartyList.IndexOf(tid));
                if (P2_Stack.Count != 2) return;
            }
            
            var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            List<int> leftGroup = [];
            leftGroup.Add(HtdhParty.IndexOf(P2_Sony.IndexOf(1)) < HtdhParty.IndexOf(P2_Sony.LastIndexOf(1)) ? P2_Sony.IndexOf(1) : P2_Sony.LastIndexOf(1));
            leftGroup.Add(HtdhParty.IndexOf(P2_Sony.IndexOf(2)) < HtdhParty.IndexOf(P2_Sony.LastIndexOf(2)) ? P2_Sony.IndexOf(2) : P2_Sony.LastIndexOf(2));
            leftGroup.Add(HtdhParty.IndexOf(P2_Sony.IndexOf(3)) < HtdhParty.IndexOf(P2_Sony.LastIndexOf(3)) ? P2_Sony.IndexOf(3) : P2_Sony.LastIndexOf(3));
            leftGroup.Add(HtdhParty.IndexOf(P2_Sony.IndexOf(4)) < HtdhParty.IndexOf(P2_Sony.LastIndexOf(4)) ? P2_Sony.IndexOf(4) : P2_Sony.LastIndexOf(4));
            
            //Two stacks on left
            if (leftGroup.Contains(P2_Stack[0]) && leftGroup.Contains(P2_Stack[1]))
            {
                var lowStackIndex = P2_Sony[P2_Stack[0]] < P2_Sony[P2_Stack[1]] ? P2_Stack[1] : P2_Stack[0];
                var lowStackSony = P2_Sony[lowStackIndex];
                var lowStackPartnerIndex = P2_Sony.IndexOf(lowStackSony) == lowStackIndex ? P2_Sony.LastIndexOf(lowStackSony) : P2_Sony.IndexOf(lowStackSony);
                leftGroup.Remove(lowStackIndex);
                leftGroup.Add(lowStackPartnerIndex);
            }
            //Two stacks on right
            if (!leftGroup.Contains(P2_Stack[0]) && !leftGroup.Contains(P2_Stack[1]))
            {
                if (P2_PTBuffIsFar)
                {
                    var lowStackIndex = P2_Sony[P2_Stack[0]] < P2_Sony[P2_Stack[1]] ? P2_Stack[0] : P2_Stack[1];
                    var lowStackSony = P2_Sony[lowStackIndex];
                    var lowStackPartnerIndex = P2_Sony.IndexOf(lowStackSony) == lowStackIndex ? P2_Sony.LastIndexOf(lowStackSony) : P2_Sony.IndexOf(lowStackSony);
                    leftGroup.Remove(lowStackPartnerIndex);
                    leftGroup.Add(lowStackIndex);
                }
                else
                {
                    var lowStackIndex = P2_Sony[P2_Stack[0]] < P2_Sony[P2_Stack[1]] ? P2_Stack[1] : P2_Stack[0];
                    var lowStackSony = P2_Sony[lowStackIndex];
                    var lowStackPartnerIndex = P2_Sony.IndexOf(lowStackSony) == lowStackIndex ? P2_Sony.LastIndexOf(lowStackSony) : P2_Sony.IndexOf(lowStackSony);
                    leftGroup.Remove(lowStackPartnerIndex);
                    leftGroup.Add(lowStackIndex);
                }
                
            }
            
            Vector3 dealpos = default;
            if (P2_PTBuffIsFar)
            {
                dealpos = leftGroup.Contains(myindex) ? new(94, 0, 100) : new(106, 0, 100);
            }
            else
            {
                dealpos = leftGroup.Contains(myindex) ? new(97, 0, 100) : new(100, 0, 103);
            }
            var c = accessory.Data.Objects.Where(o => o.DataId == 15713).FirstOrDefault();
            if (c == null) return;
            var dir8 = RoundPositionTo8Dir(c!.Position, new(100, 0, 100));
            //accessory.Log.Debug($"P2_CooperativeProgramPT {dir8} {leftGroup.Contains(myindex)}");
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_CooperativeProgramPT_StackHandlingPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = RotatePoint(dealpos, new(100, 0, 100), float.Pi / 4 * dir8);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "P2_CooperativeProgramLB_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31544"], userControl: false)]
        public void P2_CooperativeProgramLB_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 2.2;
            P2_SwordDanceTethers = [];
        }
        [ScriptMethod(name: "P2_CooperativeProgramLB_SagittariusArrow", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31539"])]
        public void P2_CooperativeProgramLB_SagittariusArrow(Event @event, ScriptAccessory accessory)
        {
            if(parse != 2.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_CooperativeProgramLB_SagittariusArrow";
            dp.Scale = new(10,42);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 500;
            dp.DestoryAt = 7500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2_CooperativeProgramLB_SwordDance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3154[01])$"])]
        public void P2_CooperativeProgramLB_SwordDance(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;


            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P2_CooperativeProgramLB_SwordDance-{sid}";
            dp.Scale = new(40);
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.TetherTarget;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 8200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        //[ScriptMethod(name: "P2_CooperativeProgramLB_SwordDanceRemoval", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31539"])]
        //public void P2_CooperativeProgramLB_SwordDanceRemoval(Event @event, ScriptAccessory accessory)
        //{
        //    if (parse != 2.2) return;
        //    foreach (var item in P2_SwordDanceTethers)
        //    {
        //        accessory.Method.RemoveDraw($"P2_CooperativeProgramLB_SwordDance-{item.Key}-{item.Value}");
        //    }
        //}
        [ScriptMethod(name: "P2_CooperativeProgramLB_ShieldComboS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31527"])]
        public void P2_CooperativeProgramLB_ShieldComboS(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P2_CooperativeProgramLB_ShieldComboS-1-1";
            dp.Scale = new(5);
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P2_CooperativeProgramLB_ShieldComboS-1-2";
            dp.Scale = new(5);
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P2_CooperativeProgramLB_ShieldComboS-2";
            dp.Scale = new(6);
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 1;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 5200;
            dp.DestoryAt = 2800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P2_CooperativeProgramLB_ShieldComboSHitHint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31528"])]
        public void P2_CooperativeProgramLB_ShieldComboSHitHint(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (accessory.Data.Me != tid) return;
            accessory.Method.TextInfo("Get out get out",3000);
            accessory.Method.TTS("Get out get out");
        }


        [ScriptMethod(name: "P2_CooperativeProgramLB_SagittariusArrowBaitPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31544"])]
        public void P2_CooperativeProgramLB_SagittariusArrowBaitPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_CooperativeProgramLB_SagittariusArrowBaitPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = new(100,0,94.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P2_CooperativeProgramLB_ShieldComboS_MalePositionLine", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32369"])]
        public void P2_CooperativeProgramLB_ShieldComboS_MalePositionLine(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myindex != 0 && myindex != 1) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_CooperativeProgramLB_ShieldComboS_MalePositionLine";
            dp.Scale = new(5);
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = sid;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 8000;
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
        }
        [ScriptMethod(name: "P2_CooperativeProgramLB_ShieldComboSSecondHitHandlingPosition", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31528"])]
        public void P2_CooperativeProgramLB_ShieldComboSSecondHitHandlingPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            Vector3 dealpos = new(100, 0, 100);
            if (accessory.Data.Me == tid) 
            {
                var pos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
                dealpos = RotatePoint(pos, new(100, 0, 100), float.Pi/2);
            }
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_CooperativeProgramLB_ShieldComboSSecondHitHandlingPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 2800;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        #endregion

        #region P3

        [ScriptMethod(name: "P3_Opening_PhaseSplit", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31507"], userControl: false)]
        public void P3_Opening_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 3.0;
            P3_ArmCount = 0;
            P3_StartBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            P3_StartDone = false;
            P3_StartPreDone = false;
            P3_TVBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        }
        [ScriptMethod(name: "P3_Opening_BuffCollection", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3425|3426)$"], userControl: false)]
        public void P3_Opening_BuffCollection(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            if (index == -1) return;
            lock (P3_StartBuff)
            {
                //1 spread 2 stack
                P3_StartBuff[index] = @event["StatusID"] == "3425" ? 1 : 2;
            }
        }
        [ScriptMethod(name: "P3_SmallTV_BuffCollection", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3452|3453)$"], userControl: false)]
        public void P3_SmallTV_BuffCollection(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            if (index == -1) return;
            lock (P3_TVBuff)
            {
                P3_TVBuff[index] = @event["StatusID"] == "3452" ? 1 : 2;
            }
        }
        [ScriptMethod(name: "P3_Opening_ArmAOE", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:regex:^(774[78])$", "SourceDataId:regex:^(1571[89])$"], userControl: false)]
        public void P3_Opening_ArmAOE(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            lock (this)
            {
                P3_ArmCount++;
                if (!ParseObjectId(@event["SourceId"], out var sid)) return;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Opening_ArmAOE";
                dp.Scale = new(11);
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay= P3_ArmCount > 3 ? 11000 : 0;
                dp.DestoryAt = P3_ArmCount > 3 ? 2500 : 14000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }
        [ScriptMethod(name: "P3_Opening_Earthquake", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31567"])]
        public void P3_Opening_Earthquake(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Opening_Earthquake_1";
            dp.Scale = new(6);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 0;
            dp.DestoryAt = 4800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Opening_Earthquake_2";
            dp.InnerScale = new(6);
            dp.Scale = new(12);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 4800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Opening_Earthquake_3";
            dp.InnerScale = new(12);
            dp.Scale = new(18);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Opening_Earthquake_4";
            dp.InnerScale = new(18);
            dp.Scale = new(24);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 8800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        [ScriptMethod(name: "P3_SmallTV_SelfAOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3159[56])$"])]
        public void P3_SmallTV_SelfAOE(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SmallTV_SelfAOE";
            dp.Scale = new(7);
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P3_Opening_BuffPrePositionHandling", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:3426"])]
        public async void P3_Opening_BuffPrePositionHandling(Event @event, ScriptAccessory accessory)
        {
            lock (this)
            {
                if(P3_StartPreDone) return;
                P3_StartPreDone = true;
            }
            await Task.Delay(100);
            List<int> sortOrder = P3_StackSort switch
            {
                P3SortEnum.HTDH => HtdhParty,
                P3SortEnum.THD => [0, 1, 2, 3, 4, 5, 6, 7],
                _ => [0, 1, 2, 3, 4, 5, 6, 7],
            };
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            //1 spread 2 stack
            var myP3StartBuff = P3_StartBuff[myindex];
            var myP3Index = 0;
            for (int i = 0; i < sortOrder.Count; i++)
            {

                var index = sortOrder[i];
                if (myP3StartBuff == P3_StartBuff[index]) myP3Index++;
                //accessory.Log.Debug($"{myindex} {index} {myP3StartBuff} {P3_StartBuff[index]} {myP3Index}");
                if (index == myindex) break;
            }
            Vector3 dealpos = default;
            if (myP3StartBuff == 2 || myP3StartBuff == 0)
            {
                dealpos = myP3Index switch
                {
                    1 =>  new(092.00f, 0, 086.14f),
                    2 =>  new(108.00f, 0, 086.14f),
                    _ => default,
                };
            }
            if (myP3StartBuff == 1)
            {
                dealpos = myP3Index switch
                {
                    1 => new(084.00f, 0, 100.00f),
                    2 => new(092.00f, 0, 113.86f),
                    3 => new(108.00f, 0, 113.86f),
                    4 => new(116.00f, 0, 100.00f),
                    _ => default,
                };
            }
            if (dealpos == default) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_Opening_BuffPrePositionHandling";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 3100;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P3_Opening_HandlingPosition", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:regex:^(774[78])$", "SourceDataId:regex:^(1571[89])$"])]
        public void P3_Opening_HandlingPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if (MathF.Abs(pos.X - 100) > 1) return;
            if (P3_StartDone) return;
            P3_StartDone = true;

            var northCirle = pos.Z < 100;

            List<int> sortOrder = P3_StackSort switch
            {
                P3SortEnum.HTDH => HtdhParty,
                P3SortEnum.THD => [0, 1, 2, 3, 4, 5, 6, 7],
                _ => [0, 1, 2, 3, 4, 5, 6, 7],
            };
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            //1 spread 2 stack
            var myP3StartBuff = P3_StartBuff[myindex];
            var myP3Index = 0;
            for (int i = 0; i < sortOrder.Count; i++)
            {
                
                var index = sortOrder[i];
                if (myP3StartBuff == P3_StartBuff[index]) myP3Index++;
                //accessory.Log.Debug($"{myindex} {index} {myP3StartBuff} {P3_StartBuff[index]} {myP3Index}");
                if (index == myindex) break;
            }
            
            Vector3 dealpos1 = default;
            Vector3 dealpos2 = default;
            Vector3 dealpos3 = default;
            Vector3 dealpos4 = default;
            if (myP3StartBuff == 2 || myP3StartBuff == 0)
            {
                dealpos1 = myP3Index switch
                {
                    1 => northCirle ? new(086.7f, 0, 086.7f) : new(094.8f, 0, 082.0f),
                    2 => northCirle ? new(113.3f, 0, 086.7f) : new(105.2f, 0, 082.0f),
                    _ => default,
                };
                dealpos2 = myP3Index switch
                {
                    1 => northCirle ? new(087.8f, 0, 087.8f) : new(095.0f, 0, 083.5f),
                    2 => northCirle ? new(112.2f, 0, 087.8f) : new(105.0f, 0, 083.5f),
                    _ => default,
                };
                dealpos3 = myP3Index switch
                {
                    1 => northCirle ? new(088.4f, 0, 085.5f) : new(093.1f, 0, 082.8f),
                    2 => northCirle ? new(111.6f, 0, 085.5f) : new(106.9f, 0, 082.8f),
                    _ => default,
                };
                dealpos4 = myP3Index switch
                {
                    1 => northCirle ? new(094.7f, 0, 083.7f) : new(088.5f, 0, 087.0f),
                    2 => northCirle ? new(105.3f, 0, 083.7f) : new(111.5f, 0, 087.0f),
                    _ => default,
                };
            }
            if (myP3StartBuff == 1)
            {
                dealpos1 = myP3Index switch
                {
                    1 => northCirle ? new(082.0f, 0, 095.0f) : new(082.0f, 0, 104.7f),
                    2 => northCirle ? new(095.0f, 0, 118.0f) : new(086.5f, 0, 113.0f),
                    3 => northCirle ? new(105.0f, 0, 118.0f) : new(113.5f, 0, 113.0f),
                    4 => northCirle ? new(118.0f, 0, 095.0f) : new(118.0f, 0, 104.7f),
                    _ => default,
                };
                dealpos2 = myP3Index switch
                {
                    1 => northCirle ? new(083.5f, 0, 095.5f) : new(083.5f, 0, 104.5f),
                    2 => northCirle ? new(095.0f, 0, 116.5f) : new(088.0f, 0, 112.0f),
                    3 => northCirle ? new(105.0f, 0, 116.5f) : new(112.0f, 0, 112.0f),
                    4 => northCirle ? new(116.5f, 0, 095.5f) : new(116.5f, 0, 104.5f),
                    _ => default,
                };
                dealpos3 = myP3Index switch
                {
                    1 => northCirle ? new(081.7f, 0, 097.2f) : new(081.6f, 0, 102.8f),
                    2 => northCirle ? new(093.2f, 0, 117.2f) : new(088.5f, 0, 114.5f),
                    3 => northCirle ? new(106.8f, 0, 117.2f) : new(111.5f, 0, 114.5f),
                    4 => northCirle ? new(118.3f, 0, 097.2f) : new(118.4f, 0, 102.8f),
                    _ => default,
                };
                dealpos4 = myP3Index switch
                {
                    1 => northCirle ? new(083.5f, 0, 104.0f) : new(084.0f, 0, 095.0f),
                    2 => northCirle ? new(088.5f, 0, 112.5f) : new(095.0f, 0, 116.3f),
                    3 => northCirle ? new(111.5f, 0, 112.5f) : new(105.0f, 0, 116.3f),
                    4 => northCirle ? new(116.5f, 0, 104.0f) : new(116.0f, 0, 095.0f),
                    _ => default,
                };
            }

            if (dealpos1 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Opening_HandlingPosition_PrePosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos1;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (dealpos1 != default && dealpos2 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Opening_HandlingPosition_1-2";
                dp.Scale = new(2);
                dp.Position = dealpos1;
                dp.TargetPosition = dealpos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Opening_HandlingPosition_2";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 6000;
                dp.DestoryAt = 2000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (dealpos2 != default && dealpos3 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Opening_HandlingPosition_2-3";
                dp.Scale = new(2);
                dp.Position = dealpos2;
                dp.TargetPosition = dealpos3;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Opening_HandlingPosition_3";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos3;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 8000;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            if (dealpos3 != default && dealpos4 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Opening_HandlingPosition_3-4";
                dp.Scale = new(2);
                dp.Position = dealpos3;
                dp.TargetPosition = dealpos4;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 14000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_Opening_HandlingPosition_4";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos4;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 14000;
                dp.DestoryAt = 2000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        [ScriptMethod(name: "P3_SmallTV_HandlingPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3159[56])$"])]
        public void P3_SmallTV_HandlingPosition(Event @event, ScriptAccessory accessory)
        {
            //31595 East
            //31596 West
            if (parse != 3.0) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var meIsIdle = P3_TVBuff[myindex]==0;
            var myBuffIndex = 0;
            var isEast = @event["ActionId"] == "31595";
            Vector3 dealpos = default;

            for (int i = 0; i < HtdhParty.Count; i++)
            {
                var index = HtdhParty[i];
                var isIdle = P3_TVBuff[index] == 0;
                if (meIsIdle == isIdle) myBuffIndex++;
                if (index == myindex) break;
            }
            if (P3_TV_Strategy==P3TVEnum.Normal)
            {
                if (meIsIdle)
                {
                    dealpos = myBuffIndex switch
                    {
                        1 => isEast ? new(099.0f, 0, 091.0f) : new(101.0f, 0, 091.0f),
                        2 => isEast ? new(104.0f, 0, 100.0f) : new(096.0f, 0, 100.0f),
                        3 => isEast ? new(115.5f, 0, 100.0f) : new(084.5f, 0, 100.0f),
                        4 => isEast ? new(099.0f, 0, 109.0f) : new(101.0f, 0, 109.0f),
                        5 => isEast ? new(099.0f, 0, 119.0f) : new(101.0f, 0, 119.0f),
                        _ => default
                    };
                }
                else
                {
                    dealpos = myBuffIndex switch
                    {
                        1 => isEast ? new(093.0f, 0, 082.0f) : new(107.0f, 0, 082.0f),
                        2 => isEast ? new(086.0f, 0, 092.5f) : new(114.0f, 0, 092.5f),
                        3 => isEast ? new(086.0f, 0, 107.5f) : new(114.0f, 0, 107.5f),
                        _ => default
                    } ;
                }
            }
            if (P3_TV_Strategy == P3TVEnum.Static)
            {
                if (meIsIdle)
                {
                    dealpos = myBuffIndex switch
                    {
                        1 => isEast ? new(099.0f, 0, 091.0f) : new(101.0f, 0, 091.0f),
                        2 => new(109.0f, 0, 100.0f),
                        3 => new(119.0f, 0, 100.0f),
                        4 => isEast ? new(099.0f, 0, 109.0f) : new(101.0f, 0, 109.0f),
                        5 => isEast ? new(099.0f, 0, 119.0f) : new(101.0f, 0, 119.0f),
                        _ => default
                    };
                }
                else
                {
                    dealpos = myBuffIndex switch
                    {
                        1 => isEast ? new(095.0f, 0, 082.0f) : new(105.0f, 0, 082.0f),
                        2 => new(086.0f, 0, 092.0f),
                        3 => new(086.0f, 0, 108.0f),
                        _ => default
                    };
                }
            }

            if (dealpos == default) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SmallTV_HandlingPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P3_SmallTV_FacingAssist", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3159[56])$"])]
        public void P3_SmallTV_FacingAssist(Event @event, ScriptAccessory accessory)
        {
            //31595 East
            //31596 West
            if (parse != 3.0) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var meIsIdle = P3_TVBuff[myindex] == 0;
            if (meIsIdle) return;
            var meLeft = P3_TVBuff[myindex] == 2;
            var myBuffIndex = 0;
            var isEast = @event["ActionId"] == "31595";
            float? seeRot = null;

            for (int i = 0; i < HtdhParty.Count; i++)
            {
                var index = HtdhParty[i];
                var isIdle = P3_TVBuff[index] == 0;
                if (meIsIdle == isIdle) myBuffIndex++;
                if (index == myindex) break;
            }
            //b pi/2

            seeRot = myBuffIndex switch
            {
                1 => isEast ? (meLeft ? float.Pi : 0) : (meLeft ? 0 : float.Pi),
                2 => meLeft ? float.Pi / 2 : float.Pi / -2,
                3 => meLeft ? float.Pi / -2 : float.Pi / 2,
                _ => null
            };
            if (seeRot == null) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SmallTV_FacingAssist_Self1";
            dp.Scale = new(5, 5);
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SmallTV_FacingAssist_Self2";
            dp.Scale = new(5, 1.5f);
            dp.Offset = new(0, 0, -5);
            dp.Rotation = float.Pi / 6 * 5;
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SmallTV_FacingAssist_Self3";
            dp.Scale = new(5, 1.5f);
            dp.Offset = new(0, 0, -5);
            dp.Rotation = float.Pi / 6 * -5;
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_SmallTV_FacingAssist_Point1";
            dp.Scale = new(10,4);
            dp.FixRotation = true;
            dp.Rotation = seeRot.Value;
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            
        }
        #endregion

        #region P4
        [ScriptMethod(name: "P4_PhaseSplit", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31559"], userControl: false)]
        public void P4_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 4.0;
            P4Stack = [];
        }
        [ScriptMethod(name: "P4_StackMarkerRecord", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:22393"],userControl:false)]
        public void P4_StackMarkerRecord(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            if (index == -1) return;
            lock (P4Stack)
            {
                P4Stack.Add(index);
            }
        }
        [ScriptMethod(name: "P4_Earthquake", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31567"])]
        public void P4_Earthquake(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Earthquake_1";
            dp.Scale = new(6);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 0;
            dp.DestoryAt = 4800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Earthquake_2";
            dp.InnerScale = new(6);
            dp.Scale = new(12);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 4800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Earthquake_3";
            dp.InnerScale = new(12);
            dp.Scale = new(18);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Earthquake_4";
            dp.InnerScale = new(18);
            dp.Scale = new(24);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 8800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        [ScriptMethod(name: "P4_FirstWaveCannonHitHint", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31614", "TargetIndex:1"])]
        public void P4_FirstWaveCannonHitHint(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            accessory.Method.TextInfo("Move", 2000, true);
            accessory.Method.TTS("Move");
        }
        [ScriptMethod(name: "P4_SecondWaveCannon", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31616"])]
        public void P4_SecondWaveCannon(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_SecondWaveCannon";
            dp.Scale = new(6,50);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 0;
            dp.DestoryAt = 4800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "P4_FirstWaveCannonBaitPosition", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3161[07])$"])]
        public void P4_FirstWaveCannonBaitPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 dealpos = myindex switch
            {
                0 => new(087.5f, 0, 094.5f),
                6 => new(086.5f, 0, 100.0f),
                2 => new(087.5f, 0, 105.0f),
                4 => new(090.5f, 0, 109.5f),
                1 => new(112.5f, 0, 094.5f),
                7 => new(113.5f, 0, 100.0f),
                3 => new(112.5f, 0, 105.0f),
                5 => new(109.5f, 0, 109.5f),
                _ => default
            };
            if (dealpos == default) return;

            if (@event["ActionId"]== "31610")
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_FirstWaveCannonBaitPosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 14000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_FirstWaveCannonBaitPosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 5500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_FirstWaveCannonBaitPosition";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 15500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            
        }
        [ScriptMethod(name: "P4_SecondWaveCannonStackPosition", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31614", "TargetIndex:1"])]
        public void P4_SecondWaveCannonStackPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;

            var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            var stack1 = P4Stack[^1];
            var stack2 = P4Stack[^2];

            List<int> leftGroup = [0, 6, 2, 4];
            List<int> rightGroup = [1, 7, 3, 5];
            if (leftGroup.Contains(stack1) && leftGroup.Contains(stack2))
            {
                var change = leftGroup.IndexOf(stack1) < leftGroup.IndexOf(stack2) ? stack2 : stack1;
                leftGroup.Remove(change);
                leftGroup.Add(5);
                rightGroup.Remove(5);
                rightGroup.Add(change);
            }
            if (rightGroup.Contains(stack1) && rightGroup.Contains(stack2))
            {
                var change = rightGroup.IndexOf(stack1) < rightGroup.IndexOf(stack2) ? stack2 : stack1;
                rightGroup.Remove(change);
                rightGroup.Add(4);
                leftGroup.Remove(4);
                leftGroup.Add(change);
            }

            Vector3 dealpos = leftGroup.Contains(myindex) ? new(96.5f, 0, 113) : new(103.5f, 0, 113);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_SecondWaveCannonStackPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        #endregion

        #region P5
        [ScriptMethod(name: "P5_Opening_PhaseSplit", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31621"], userControl: false)]
        public void P5_Opening_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 5.0;
        }
        [ScriptMethod(name: "P5_FirstTrio_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31624"], userControl: false)]
        public void P5_FirstTrio_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 5.1;
            P51_Eye = 0;
            P51_Buff = [0, 0, 0, 0, 0, 0, 0, 0];
            P51_Fist = [0, 0, 0, 0];
            
            _phase = TopPhase.P5A1_DeltaVersion;
            InitParams();
            _pd.Init(accessory, "P5 First Trio");
            _pd.AddPriorities([0, 1, 2, 3, 4, 5, 6, 7]);    // Add priority values in role order
            myArmUnitBiasEnable = false;
            accessory.Log.Debug($"Current Phase: {_phase}");
        }
        
        [ScriptMethod(name: "P5_FirstTrio_EyeLaser", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375AC", "Id:00020001"])]
        public void P5_FirstTrio_EyeLaser(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var rot = @event["Index"] switch
            {
                "00000001" => 0,
                "00000002" => 1,
                "00000003" => 2,
                "00000004" => 3,
                "00000005" => 4,
                "00000006" => 5,
                "00000007" => 6,
                "00000008" => 7,
                _ => -1
            };
            if (rot == -1) return;
            var pos = RotatePoint(new(100, 0, 80), new(100, 0, 100), float.Pi / 4 * rot);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_FirstTrio_EyeLaser";
            dp.Scale = new(16,40);
            dp.Position = pos;
            dp.TargetPosition = new(100, 0, 100);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7500;
            dp.DestoryAt = 12500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        
        [ScriptMethod(name: "First Trio Far Tether Record", eventType: EventTypeEnum.Tether, eventCondition: ["Id:00C9"], userControl: Debugging)]
        public void P5_Delta_LocalRemoteTetherRecord(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;

            lock (_pd)
            {
                var targetId = ev.TargetId;
                var targetIdx = sa.GetPlayerIdIndex((uint)targetId);
                var sourceId = ev.SourceId;
                var sourceIdx = sa.GetPlayerIdIndex((uint)sourceId);

                var pdValMax = _pd.SelectSpecificPriorityIndex(0, true).Value;
                _pd.AddPriority(targetIdx, pdValMax >= 1000 ? 2000 : 1000);
                _pd.AddPriority(sourceIdx, pdValMax >= 1000 ? 2000 : 1000);
                _events[2].Set();   // Far tether partner recorded
            }
        }
        
        [ScriptMethod(name: "First Trio Non-Leader Marker Record", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:regex:^(0[123467])$"],
            userControl: Debugging)]
        public void P5_DeltaVersionReceiveMarker(Event ev, ScriptAccessory sa)
        {
            // Only take Attack 1-4 and Chain 1-2
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
        
            lock (_pd)
            {
                var mark = ev.Id0();
                sa.Log.Debug($"Detected ev.Id {mark}");
                var tid = ev.TargetId;
                var tidx = sa.GetPlayerIdIndex((uint)tid);
            
                _pd.AddActionCount();
                var pdVal = mark switch
                {
                    1 => 10,    // Attack 1
                    2 => 20,    // Attack 2
                    3 => 30,    // Attack 3
                    4 => 40,    // Attack 4
                    6 => 100,   // Chain 1
                    7 => 200,   // Chain 2
                    _ => 0
                };
                _pd.AddPriority(tidx, pdVal);
                if (_pd.ActionCount != 6) return;
                sa.Log.Debug($"ev[0] First Trio, Non-leader marker record completed.");
                sa.Log.Debug($"{_pd.ShowPriorities()}");
                _events[0].Set();   // Marker record
            }
        }
        
        [ScriptMethod(name: "First Trio Locate Bald Omega", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["SourceDataId:14669", "Id:7747"],
            userControl: Debugging)]
        public void P5_DeltaVersionFindOmegaBald(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            var spos = ev.SourcePosition;
            var dir = spos.Position2Dirs(Center, 4);
            _numbers[0] = dir;              // Bald
            _numbers[1] = (dir + 2) % 4;    // Beetle
            _events[1].Set();   // Bald/Beetle located
            sa.Log.Debug($"ev[1] Bald Position: {_numbers[0]}, Beetle Position: {_numbers[1]} recorded.");
        } 
        
        [ScriptMethod(name: "First Trio Initial Position Guidance *", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["SourceDataId:14669", "Id:7747"],
        userControl: true)]
        public void P5_DeltaVersionFirstGuidance(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            _events[0].WaitOne(5000);   // When triggering judgment, if Set signal not captured after 5000ms, remove
            _events[1].WaitOne(2000);
            _events[2].WaitOne(2000);
            
            var myIndex = sa.GetMyIndex();
            var myPdVal = _pd.Priorities[myIndex];
            if (myPdVal >= 1000 & myPdVal.GetDecimalDigit(3) == 0)
            {
                // Player needs to find Chain1/Chain2 partner
                // Chain1 partner: same thousands, hundreds is 1. Chain2 partner: same thousands, hundreds is 2.
                // If myPdVal thousands is 1, self is descending 4th, find descending 3rd (index 2) as partner
                // If myPdVal thousands is 2, self is descending 2nd, find descending 1st (index 0) as partner
                var myPartner = _pd.SelectSpecificPriorityIndex(myPdVal >= 2000 ? 0 : 2, true);
                sa.Log.Debug($"Player has far tether no world, found far tether partner {sa.GetPlayerJobByIndex(myPartner.Key)}");
                _pd.AddPriority(myIndex, myPartner.Value.GetDecimalDigit(3) * 100 + 10);  // Add hundreds + 10, e.g., if partner is Chain2, player +210
                sa.Log.Debug($"{_pd.ShowPriorities()}");
            }
            
            // After correction, uniformly change priority values.
            // Note: Only marked when player is Stop1/Stop2, otherwise 0.
            for (int i = 0; i < 8; i++)
            {
                _pd.Priorities[i] = _pd.Priorities[i] % 1000 / 10;
            }
            sa.Log.Debug($"Corrected, {_pd.ShowPriorities()}");
            
            // var markerVal = myPdVal % 1000 / 10;  // Remove thousands and ones
            var markerVal = _pd.Priorities[myIndex];
            var omegaBaldDirection = _numbers[0];
            var beetleDirection = _numbers[1];

            Vector3 tpos1 = new(90f, 0, 94f);
            Vector3 tpos2 = new(110f, 0, 94f);

            tpos1 = markerVal switch
            {
                1 => tpos1.RotatePoint(Center, 90f.DegToRad() * beetleDirection),
                2 => tpos1.RotatePoint(Center, 90f.DegToRad() * beetleDirection),
                3 => (tpos1 - new Vector3(0, 0, 8)).RotatePoint(Center, 90f.DegToRad() *
                    beetleDirection),
                4 => (tpos1 - new Vector3(0, 0, 8)).RotatePoint(Center, 90f.DegToRad() *
                    beetleDirection),
                10 => tpos1.RotatePoint(Center, 90f.DegToRad() * omegaBaldDirection),
                11 => tpos1.RotatePoint(Center, 90f.DegToRad() * omegaBaldDirection),
                20 => (tpos1 - new Vector3(0, 0, 8)).RotatePoint(Center,
                    90f.DegToRad() * omegaBaldDirection),
                21 => (tpos1 - new Vector3(0, 0, 8)).RotatePoint(Center,
                    90f.DegToRad() * omegaBaldDirection),
                _ => new Vector3(100f, 0, 100f),
            };

            tpos2 = markerVal switch
            {
                1 => tpos2.RotatePoint(Center, 90f.DegToRad() * beetleDirection),
                2 => tpos2.RotatePoint(Center, 90f.DegToRad() * beetleDirection),
                3 => (tpos2 - new Vector3(0, 0, 8)).RotatePoint(Center, 90f.DegToRad() *
                    beetleDirection),
                4 => (tpos2 - new Vector3(0, 0, 8)).RotatePoint(Center, 90f.DegToRad() *
                    beetleDirection),
                10 => tpos2.RotatePoint(Center, 90f.DegToRad() * omegaBaldDirection),
                11 => tpos2.RotatePoint(Center, 90f.DegToRad() * omegaBaldDirection),
                20 => (tpos2 - new Vector3(0, 0, 8)).RotatePoint(Center,
                    90f.DegToRad() * omegaBaldDirection),
                21 => (tpos2 - new Vector3(0, 0, 8)).RotatePoint(Center,
                    90f.DegToRad() * omegaBaldDirection),
                _ => new Vector3(100f, 0, 100f),
            };

            var dp = sa.DrawGuidance(tpos1, 0, 5000, $"First Trio Standby Point 1");
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            dp = sa.DrawGuidance(tpos2, 0, 5000, $"First Trio Standby Point 2");
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            _events[0].Reset();
            _events[1].Reset();
            _events[2].Reset();
        }
    
        [ScriptMethod(name: "First Trio Record Fists", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(157(09|10))$"], userControl: Debugging)]
        public void P5_DeltaRocketPunchRecord(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            const uint blue = 15709;
            var dataid = JsonConvert.DeserializeObject<uint>(ev["DataId"]);
            var spos = ev.SourcePosition;
            lock (_pd)
            {
                _pd.AddActionCount();
                var myObj = sa.Data.MyObject;
                if (myObj == null) return;
                var a = myObj.Position;
                // When fist appears, record own quadrant
                if (_pd.ActionCount == 7)
                    _numbers[2] = myObj.Position.Position2Dirs(Center, 4, false);
                if (spos.Position2Dirs(Center, 4, false) == _numbers[2])
                {
                    _numbers[3]++;      // Fist count
                    _numbers[4] += dataid == blue ? 1 : -1; // Fist color
                    sa.Log.Debug($"Captured fist in player quadrant {_numbers[2]}, count {_numbers[3]}, color {(dataid == blue ? "Blue" : "Yellow")}, record value {_numbers[4]}");
                }

                if (_pd.ActionCount == 14)  // 6 markers, 8 fists
                {
                    _events[3].Set();   // Fist recording complete
                    sa.Method.RemoveDraw($"First Trio Standby Point.*");
                }
            }
        }
    
        [ScriptMethod(name: "First Trio Fist Standby Guidance *", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:regex:^(157(09|10))$"],
            userControl: true, suppress: 10000)]
        public void P5_DeltaRocketPunchGuidance(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            _events[3].WaitOne(2000);   // Fist recording complete
            var myIndex = sa.GetMyIndex();
            var markerVal = _pd.Priorities[myIndex];
            var isOutside = markerVal is 3 or 4 or 20 or 21;    // Atk3, Atk4, Chain2, Stop2
            var isRemoteTetherOutside = markerVal is 20 or 21;  // Chain2, Stop2
            
            var omegaBaldDirection = _numbers[0];
            var beetleDirection = _numbers[1];
            var myQuadrant = _numbers[2];
            var punchCountAtMyQuadrant= _numbers[3];
            var punchColorAtMyQuadrant = _numbers[4];
            
            // Find quadrant 0, marker inside. Will shift with boss position.
            var tposOut = new Vector3(108.7f, 0f, 90f).RotatePoint(new Vector3(109.9f, 0f, 90.1f),
                omegaBaldDirection % 2 == 1 ? -90f.DegToRad() : 0);
            var tposIn = new Vector3(102.7f, 0f, 90f).RotatePoint(new Vector3(109.9f, 0f, 90.1f),
                omegaBaldDirection % 2 == 1 ? -90f.DegToRad() : 0);  // Outer chain

            var tposBase = isRemoteTetherOutside ? tposIn : tposOut;
            
            tposBase = myQuadrant switch
            {
                1 => tposBase.FoldPointVertical(Center.Z),
                2 => tposBase.FoldPointVertical(Center.Z).FoldPointHorizon(Center.X),
                3 => tposBase.FoldPointHorizon(Center.X),
                _ => tposBase,
            };
            
            if (!isOutside)
            {
                // "Outside" here means marker is larger, positioned near wall.
                // If player is inside, always go to quadrant point.
                
                var dp = sa.DrawGuidance(tposBase, 0, 5000, $"First Trio Fist");
                sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                sa.Log.Debug($"Player inside, go to quadrant {myQuadrant} point.");
            }
            else
            {
                // If player is outside
                // If fist count in quadrant is not 2, someone didn't pre-position (maybe SAM/PCT), need to observe, no guidance
                if (punchCountAtMyQuadrant != 2)
                {
                    sa.Method.TextInfo($"Observe if same group fist colors swap.", 4000, true);
                    sa.Log.Debug($"Fist count in quadrant {myQuadrant} incorrect, need to observe.");
                }
                // If fist count is 2 and color value not 0, fists same color, need to swap.
                else if (punchColorAtMyQuadrant != 0)
                {
                    var tpos = omegaBaldDirection % 2 == 1
                        ? tposBase.FoldPointVertical(Center.Z)
                        : tposBase.FoldPointHorizon(Center.X);
                    var dp = sa.DrawGuidance(tpos, 0, 5000, $"First Trio Fist");
                    sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    sa.Log.Debug($"Player quadrant {myQuadrant} fists same color, need to swap.");
                }
                else
                {
                    // No swap needed, go directly based on _dv.myQuadrant
                    var dp = sa.DrawGuidance(tposBase, 0, 5000, $"First Trio Fist");
                    sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    sa.Log.Debug($"Player no swap needed, go to quadrant {myQuadrant} point.");
                }
            }
            
            _events[3].Reset();
        }
    
        [ScriptMethod(name: "First Trio Fist Rotation Bait Position", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(009[CD])$"], userControl: true)]
        public void P5_DeltaArmUnitRotate(Event ev, ScriptAccessory sa)
        {            
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            var id = ev.Id0();
            var tid = ev.TargetId;
            var tpos = sa.GetById(tid)?.Position ?? Center;
            
            tpos = tpos.PointInOutside(Center, 1f);
            // RotateCW = 156
            var baitPos = tpos.RotatePoint(Center, id == 156 ? -5f.DegToRad() : 5f.DegToRad());
            var dp = sa.DrawStaticCircle(baitPos, sa.Data.DefaultSafeColor.WithW(3f), 0, 10000, $"Arm Unit Rotate", 0.5f);
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
        }
    
        private bool myArmUnitBiasEnable = false;
        [ScriptMethod(name: "First Trio Player Fist Bait Guidance *", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31587"],
            userControl: true)]
        public void P5_DeltaMyArmUnitBiasGuidance(Event ev, ScriptAccessory sa)
        {            
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            // This check might be returned due to suppress, use scenario needs care, ensure event triggers. Otherwise use bool.
            if (ev.TargetId != sa.Data.Me) return;
            if (_bools[0]) return;  // Has player fist bait guidance been drawn?
            _bools[0] = true;
            myArmUnitBiasEnable = true;
            var myIndex = sa.GetMyIndex();
            var markerVal = _pd.Priorities[myIndex];
            var myPos = ev.TargetPosition;
            
            var omegaBaldDirection = _numbers[0];
            
            var omegaBaldDirection12 = omegaBaldDirection * 3;
            var omegaBaldPos = new Vector3(100, 0, 80).RotatePoint(Center, omegaBaldDirection12 * 30f.DegToRad());
            
            var isShieldTarget = markerVal is 10 or 11;     // Chain1, Stop1, need to go center for shield bash
            var isOutside = markerVal is 3 or 4 or 20 or 21;    // Atk3, Atk4, Chain2, Stop2, towards outside
            var isAtRight = myPos.IsAtRight(omegaBaldPos, Center);
            var isBind = markerVal is 10 or 20 or 11 or 21; // Chain1, Chain2, Stop1, Stop2
            
            var val = 100 * (isOutside ? 1 : 0) + 10 * (isAtRight ? 1 : 0) + 1 * (isBind ? 1 : 0);
            
            // myArmUnit
            _numbers[5] = val switch
            {
                111 => (omegaBaldDirection12 + 1) % 12,
                110 => (omegaBaldDirection12 + 5) % 12,
                101 => (omegaBaldDirection12 + 11) % 12,
                100 => (omegaBaldDirection12 + 7) % 12,
                10 => (omegaBaldDirection12 + 3) % 12,
                0 => (omegaBaldDirection12 + 9) % 12,
                
                11 => (omegaBaldDirection12 + 3) % 12,
                1 => (omegaBaldDirection12 + 9) % 12,
                
                _ => -1
            };
            var myArmUnit = _numbers[5];
            
            sa.Log.Debug(!isShieldTarget ? $"Player needs to bait arm unit at direction {myArmUnit}" : $"Player needs to go center towards direction {myArmUnit}");
            sa.Method.TextInfo($"Group inside, wait for yellow circle", 2000);

            if (!isShieldTarget)
            {
                sa.Log.Debug($"Drawing fist bait position guidance");
                var armUnitPos = new Vector3(100, 0, 84).RotatePoint(Center, myArmUnit * 30f.DegToRad());
                var dp = sa.DrawGuidance(armUnitPos, 0, 4000, $"Fist Bait Guidance", isSafe: false);
                sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            else
            {
                sa.Log.Debug($"Drawing center bait position guidance");
                var armUnitPos = new Vector3(100, 0, 95).RotatePoint(Center, myArmUnit * 30f.DegToRad());
                var dp = sa.DrawGuidance(armUnitPos, 0, 4000, $"Center Bait Guidance", isSafe: false);
                sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        
        [ScriptMethod(name: "First Trio Player Fist Bait Guidance Refresh", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31482"],
            userControl: Debugging, suppress: 10000)]
        public void P5_DeltaMyArmUnitBiasRefresh(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            sa.Method.RemoveDraw($"Fist Bait Guidance");
            sa.Method.RemoveDraw($"Center Bait Guidance");
            
            if (!myArmUnitBiasEnable) return;
            
            var myIndex = sa.GetMyIndex();
            var markerVal = _pd.Priorities[myIndex];
            var isShieldTarget = markerVal is 10 or 11;
            if (isShieldTarget) return;
            
            var myArmUnit = _numbers[5];
            
            sa.Log.Debug($"Go bait fist");
            var armUnitPos = new Vector3(100, 0, 84).RotatePoint(Center, myArmUnit * 30f.DegToRad());
            var dp = sa.DrawGuidance(armUnitPos, 0, 4000, $"Bait Fist", isSafe: true);
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        
        [ScriptMethod(name: "First Trio Rotating Hand Guidance Removal", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31600"],
            userControl: Debugging, suppress: 10000)]
        public void P5_DeltaMyArmUnitBiasRemove(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            sa.Method.RemoveDraw($"Bait Fist");
        }
        
        [ScriptMethod(name: "First Trio Player Center Shield Bait Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31482"],
            userControl: true, suppress: 10000)]
        public void P5_DeltaOmegaCenterShieldBias(Event ev, ScriptAccessory sa)
        {            
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            var myIndex = sa.GetMyIndex();
            var markerVal = _pd.Priorities[myIndex];
            
            if (markerVal is not 10 and not 11) return;
            var myArmUnit = _numbers[5];
            
            var centerBiasPos = new Vector3(100, 0, 95).RotatePoint(Center, myArmUnit * 30f.DegToRad());
            var dp = sa.DrawGuidance(centerBiasPos, 0, 3000, $"Center Shield Combo Bait");
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        
        [ScriptMethod(name: "First Trio After Arm Unit Bait Near Tether Standby Guidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31600"],
            userControl: true, suppress: 10000)]
        public void P5_DeltaAfterArmUnitBiasGuidance(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            var myIndex = sa.GetMyIndex();
            var markerVal = _pd.Priorities[myIndex];

            if (markerVal is not 1 and not 2 and not 3 and not 4) return;
            var myArmUnit = _numbers[5];
        
            var standByPos = new Vector3(100, 0, 86).
                RotatePoint(Center, MathF.Round((float)myArmUnit * 2 / 3) * 45f.DegToRad());
            var dp = sa.DrawGuidance(standByPos, 0, 6000, $"Attack Marker Standby Point");
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
    
        [ScriptMethod(name: "First Trio Bald Left/Right Scan Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3163[89])$"],
            userControl: Debugging)]
        public void P5_DeltaOmegaBaldCannonRecord(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            const uint right = 31638;
            _numbers[6] = ev.ActionId == right ? 1 : 2;
        }
    
        [ScriptMethod(name: "First Trio Player Small TV Buff Record", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(345[23])$"],
            userControl: Debugging)]
        public void P5_DeltaPlayerCannonRecord(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            var tidx = sa.GetPlayerIdIndex((uint)ev.TargetId);
            _pd.AddPriority(tidx, 100);    // Small TV marker +100
            const uint right = 3452;
            _numbers[7] = ev.StatusId == right ? 1 : 2; // Right 1, Left 2
        }
        
        [ScriptMethod(name: "First Trio Shield Combo Target Record", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31528"],
        userControl: Debugging)]
        public void P5_DeltaShieldTargetRecord(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            if (JsonConvert.DeserializeObject<uint>(ev["TargetIndex"]) != 1) return;
            sa.Method.RemoveDraw($"Center Shield Combo Bait");
            
            var tidx = sa.GetPlayerIdIndex((uint)ev.TargetId);
            _pd.AddPriority(tidx, 1000);    // Shield combo target +1000
            // Shield combo is the last part of first trio sequence
            _events[4].Set();   // (int)RecordedIdx.ShieldTargetRecorded
        }
        
        [ScriptMethod(name: "First Trio Stack and Small TV Guidance", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31528"],
            userControl: true)]
        public void P5_DeltaStackAndCannonGuidance(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            _events[4].WaitOne(1000);
            var myIndex = sa.GetMyIndex();
            var markerVal = _pd.Priorities[myIndex] % 100;  // Don't need hundreds digit
            var myPriVal = _pd.Priorities[myIndex];  // Don't need hundreds digit
            
            if (markerVal is 1 or 2 or 3 or 4)
            {
                // Near tether group not involved
                _events[4].Reset();
                return;
            }
            
            // Is shield combo target same as small TV target, is player small TV target
            // Baseline: Bald at A, Bald TV hits Right, rotate with Bald as 12 o'clock.
            
            var isSameTarget = _pd.SelectSpecificPriorityIndex(0, true).Value >= 1100;
            
            // If shield combo target and small TV target same, shield target steps out, stack target steps in
            var shieldTargetPos = new Vector3(101, 0, 85) + (isSameTarget ? new Vector3(3.5f, 0, 0) : new Vector3(0, 0, 0));
            var stackPos = new Vector3(101, 0, 100) + (isSameTarget ? new Vector3(0, 0, 0) : new Vector3(3.5f, 0, 0));

            var omegaBaldDirection = _numbers[0];
            var omegaBaldCannonType = _numbers[6];
            var playerCannonType = _numbers[7];
                
            // _dv.OmegaBaldCannonType 1 Right 2 Left
            if (omegaBaldCannonType == 2)
            {
                // Left blade, fold then rotate
                shieldTargetPos = shieldTargetPos.FoldPointHorizon(Center.X);
                stackPos = stackPos.FoldPointHorizon(Center.X);
            }
            
            var rotateRad = omegaBaldDirection * 90f.DegToRad();
            shieldTargetPos = shieldTargetPos.RotatePoint(Center, rotateRad);
            stackPos = stackPos.RotatePoint(Center, rotateRad);

            if (myPriVal / 1000 == 1)   // >1000, shield combo target
            {
                var dp = sa.DrawGuidance(shieldTargetPos, 0, 5000, $"First Trio Shield Combo Guidance");
                sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            else
            {
                var dp = sa.DrawGuidance(stackPos, 0, 5000, $"First Trio Stack Guidance");
                sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            if (myPriVal % 1000 >= 100)    // >100, small TV target
            {
                var faceDir = (omegaBaldDirection + (omegaBaldCannonType != playerCannonType ? 2 : 0)) % 4;
                var dp = sa.DrawStatic((ulong)sa.Data.Me, (ulong)0, 0, faceDir * 90f.DegToRad().Logic2Game(),
                    1f, 4.5f, sa.Data.DefaultSafeColor, 0, 5000, $"Small TV Facing Assist - Correct Direction");
                dp.FixRotation = true;
                sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp);
                
                // DrawStatic generally for static schemes, so Game2Logic added in rotation. Following units need extra Game2Logic.
                var dp0 = sa.DrawStatic((ulong)sa.Data.Me, (ulong)0, 0, 0f.Logic2Game(),
                    1f, 4.5f, sa.Data.DefaultDangerColor, 0, 5000, $"Small TV Facing Assist - Self");
                sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, dp0);
                
                sa.Method.TextInfo($"Small TV, stand outside", 3000, true);
            }
            else
                sa.Method.TextInfo($"Dodge Small TV, stand inside", 3000, true);
            
            _events[4].Reset();
        }

        [ScriptMethod(name: "P5_FirstTrio_SmallTVAssist", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3163[89])$"], userControl: true)]
        public async void P5_FirstTrio_SmallTVAssist(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            var myIndex = sa.GetMyIndex();
            var myPriVal = _pd.Priorities[myIndex];
            if (myPriVal % 1000 < 100) return;
            var me = sa.Data.MyObject;
            if (me == null) return;
            P5_TV_Support_enable = true;
            bool? oldState = null;
            var rot = ev.SourceRotation;
            if (ev.ActionId == 31638 && _numbers[7] == 1 || ev.ActionId == 31639 && _numbers[7] == 2 )
            {
                rot = rot + float.Pi;
            }

            await Task.Delay(6000);
            while (P5_TV_Support_enable)
            {
                await Task.Delay(100);
                bool state = me.Rotation.Equals(rot);
                if (oldState == state || IsMoving)
                {
                    // sa.Log.Debug($"Facing {me.Rotation}, maintain");
                }   
                else
                {
                    SetRotation(sa, me, rot);
                    oldState = state;
                }
            }
        }
        
        public static bool IsMoving
        {
            get
            {
                bool isMoving = false;
                unsafe
                {
                    FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentMap* ptr = FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentMap.Instance();
                    if (ptr is not null)
                    {
                        isMoving = ptr->IsPlayerMoving;
                    }
                }
                return isMoving;
            }
        }
        
        [ScriptMethod(name: "P5_FirstTrio_SmallTVAssistOff", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3163[89])$"], userControl: false)]
        public async void P5_FirstTrio_SmallTVAssistOff(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            P5_TV_Support_enable = false;
        }
        
        [ScriptMethod(name: "First Trio Drawing Removal, Prepare First Pass", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(31529)$"],
            userControl: Debugging)]
        public void P5_DeltaVersionComplete(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A1_DeltaVersion) return;
            _phase = TopPhase.P5A2_DeltaWorld;
            sa.Method.RemoveDraw("Small TV.*");
            sa.Method.RemoveDraw("First Trio.*");
            
            // Initialize _events
            _events = Enumerable
                .Range(0, 20)
                .Select(_ => new ManualResetEvent(false))
                .ToList();
            
            for (int i = 0; i < 8; i++)
            {
                _pd.Priorities[i] %= 100;    // Keep ones and tens
            }
            sa.Log.Debug($"First Pass: Corrected, {_pd.ShowPriorities()}");
        }
        
        [ScriptMethod(name: "----《P5 First Pass》----", eventType: EventTypeEnum.NpcYell, eventCondition: ["HelloMyWorld"],
            userControl: true)]
        public void SplitLine_DeltaWorld(Event ev, ScriptAccessory sa)
        {
        }
    
        [ScriptMethod(name: "First Pass Beetle Left/Right Swipe Record", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3163[67])$"],
            userControl: Debugging)]
        public void P5_DeltaBeetleSwipeRecord(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A2_DeltaWorld) return;
            const uint right = 31636;
            _numbers[8] = ev.ActionId == right ? 1 : 2;
            _events[0].Set();
        }
        
        [ScriptMethod(name: "First Pass Beetle Left/Right Swipe", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3163[67])$"],
            userControl: true)]
        public void P5_FirstPass_BeetleSwipe(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A2_DeltaWorld) return;
            var rot = ev.ActionId == 31636 ? -float.Pi / 2 : float.Pi / 2;
            var dp = sa.Data.GetDefaultDrawProperties();
            dp.Name = "P5_FirstPass_BeetleSwipe";
            dp.Radian = float.Pi + float.Pi / 6;
            dp.Scale = new(90);
            dp.Rotation = rot;
            dp.Owner = ev.SourceId;
            dp.Color = sa.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        } 

        [ScriptMethod(name: "First Pass Guidance *", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3163[67])$"],
            userControl: true)]
        public void P5_DeltaWorldGuidance(Event ev, ScriptAccessory sa)
        {            
            if (_phase != TopPhase.P5A2_DeltaWorld) return;
            _events[0].WaitOne();
            var myIndex = sa.GetMyIndex();
            var markerVal = _pd.Priorities[myIndex];
            
            // Baseline: Beetle Right Swipe, Beetle at A
            List<Vector3> posList =
            [
                new(102f, 0, 81f),          // Atk1 - FarTarget
                new(110.6f, 0, 116.2f),     // Atk2 - FarTarget
                new(108.9f, 0, 88.9f),      // Atk3 - NearTarget
                new(113.7f, 0, 86.3f),      // Atk4 - NearTarget
                new(119.5f, 0, 100f),       // Bind1 - FarSource
                new(106.5f, 0, 100f),       // Bind2 - NearSource
                new(116.2f, 0, 111f),       // Stop1 - Idle
                new(116.2f, 0, 111f)        // Stop2 - Idle
            ];

            var myPosIdx = markerVal switch
            {
                1 => 0,
                2 => 1,
                3 => 2,
                4 => 3,
                10 => 4,
                20 => 5,
                11 => 6,
                21 => 7,
                _ => -1,
            };
            
            if (myPosIdx == -1)
            {
                sa.Log.Debug($"Player marker info {markerVal} read error");
                return;
            }

            var beetleSwipe = _numbers[8];
            var beetleDirection = _numbers[1];
            
            // Rotate and fold based on Beetle swipe and direction
            var myPos = posList[myPosIdx];
            var isRightSwipe = beetleSwipe == 1;
            if (!isRightSwipe)
                myPos = myPos.FoldPointHorizon(Center.X);
            myPos = myPos.RotatePoint(Center, beetleDirection * 90f.DegToRad());

            var dp = sa.DrawGuidance(myPos, 0, 5000, $"First Pass Guidance");
            sa.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            _events[0].Reset();
        }
        
        [ScriptMethod(name: "First Pass Guidance Removal", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3163[67])$"],
            userControl: Debugging)]
        public void P5_DeltaWorldGuidanceRemove(Event ev, ScriptAccessory sa)
        {
            if (_phase != TopPhase.P5A2_DeltaWorld) return;
            sa.Method.RemoveDraw("First Pass Guidance");
        }
        
        [ScriptMethod(name: "First Pass Near Tether Pull Hint", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:1672"],
            userControl: true)]
        public void P5_DeltaWorldLocalTetherBreakHint(Event ev, ScriptAccessory sa)
        {
            // TODO This feature not implemented, reason unknown
            // Near tether established during DeltaVersion
            if (_phase != TopPhase.P5A2_DeltaWorld) return;

            // Marker taken before tether becomes solid
            var myIndex = sa.GetMyIndex();
            var markerVal = _pd.Priorities[myIndex];
            if (markerVal is not 1 and not 2) return;
            
            // BreakLocalTether = 1
            if (_bools[1]) return;
            _bools[1] = true;

            // Find Attack 1/2
            var myPartner = _pd.SelectSpecificPriorityIndex(markerVal == 1 ? 1 : 0).Key;
            var dur = (int)JsonConvert.DeserializeObject<uint>(ev["DurationMilliseconds"]);
            sa.Log.Debug($"My marker is Attack1 or Attack2, will draw pull hint with {sa.GetPlayerJobByIndex(myPartner)}. {dur}");
            
            // With 10 seconds left, DeltaWorld executing, need to break in last 2 seconds.

            var delay1 = dur - 8000;
            var destroy1 = 6000;
            var delay2 = dur - 3000;
            var destroy2 = 3000;

            // Near tether actual distance ~10, use 11
            var dp1 = sa.DrawCircle(sa.Data.PartyList[myPartner], 11, delay1, destroy1, $"Near Tether Don't Break");
            dp1.Color = sa.Data.DefaultDangerColor;
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp1);
            
            var dp2 = sa.DrawCircle(sa.Data.PartyList[myPartner], 11, delay2, destroy2, $"Near Tether Break");
            dp2.Color = sa.Data.DefaultSafeColor;
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);
        }
        
        [ScriptMethod(name: "P5_SecondTrio_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32788"], userControl: false)]
        public void P5_SecondTrio_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 5.2;
            accessory.Log.Debug($"--- parse changed to {parse}");
            _phase = TopPhase.P5B1_SigmaVersion;
            InitParams();
            _pd.Init(accessory, "P5 Second Trio");
            P52_semaphoreTowersWereConfirmed = new AutoResetEvent(false);
        }
        
        [ScriptMethod(name: "P5_SecondTrio_MalePosition", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:15724"],userControl: false)]
        public void P5_SecondTrio_MalePosition(Event @event, ScriptAccessory sa)
        {
            if (parse != 5.2) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            P52_OmegaMDir = (RoundPositionTo8Dir(pos, new(100, 0, 100))+4)%8;
            sa.Log.Debug($"P5 Second Trio Male Direction: {P52_OmegaMDir}");
            
        }
        
        [ScriptMethod(name: "P5_SecondTrio_FarNearBuff", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3427|3428)$"], userControl: false)]
        public void P5_SecondTrio_FarNearBuff(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            P5_SigmaBuffIsFar = @event["StatusID"] == "3428";
        }
        
        [ScriptMethod(name: "P5_SecondTrio_FirstHalfPosition", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:regex:^(0[1-46-8]|12)$"], userControl: true)]
        public void P5_SecondTrio_FirstHalfPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (accessory.Data.PartyList.IndexOf(tid) != accessory.Data.PartyList.IndexOf(accessory.Data.Me)) return;
            accessory.Log.Debug($"P5 Second Trio Player Marker: {@event["Id"]}");
            P52_Self_Pos = @event["Id"] switch
            {
                "01" => P5_SigmaBuffIsFar
                    ? RotatePoint(new Vector3(92.73f, 0, 82.45f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir)
                    : RotatePoint(new Vector3(95.03f, 0, 87.99f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir),
                "02" => P5_SigmaBuffIsFar
                    ? RotatePoint(new Vector3(83.37f, 0, 93.11f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir)
                    : RotatePoint(new Vector3(89.84f, 0, 95.79f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir),
                "03" => P5_SigmaBuffIsFar
                    ? RotatePoint(new Vector3(82.45f, 0, 107.27f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir)
                    : RotatePoint(new Vector3(89.843f, 0, 104.21f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir),
                "04" => P5_SigmaBuffIsFar
                    ? RotatePoint(new Vector3(92.73f, 0, 117.55f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir)
                    : RotatePoint(new Vector3(95.79f, 0, 110.16f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir),
                "06" => P5_SigmaBuffIsFar
                    ? RotatePoint(new Vector3(107.27f, 0, 82.45f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir)
                    : RotatePoint(new Vector3(104.97f, 0, 87.99f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir),
                "07" => P5_SigmaBuffIsFar
                    ? RotatePoint(new Vector3(116.63f, 0, 93.11f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir)
                    : RotatePoint(new Vector3(110.16f, 0, 95.79f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir),
                "08" => P5_SigmaBuffIsFar
                    ? RotatePoint(new Vector3(117.55f, 0, 107.27f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir)
                    : RotatePoint(new Vector3(110.16f, 0, 104.21f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir),
                "12" => P5_SigmaBuffIsFar
                    ? RotatePoint(new Vector3(107.27f, 0, 117.55f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir)
                    : RotatePoint(new Vector3(104.21f, 0, 110.16f), new(100, 0, 100), float.Pi / 4 * P52_OmegaMDir),
            };
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_SecondTrio_FirstHalfPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = P52_Self_Pos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        
        [ScriptMethod(name: "P5_SecondTrio_TowerPositions", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Add", "DataId:regex:^(2013245|2013246)$"],userControl: false)]
        public void P5_SecondTrio_TowerPositions(Event @event, ScriptAccessory sa)
        {
            if (parse != 5.2) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var dir = RoundPositionTo16Dir(pos, new(100, 0, 100));
            P52_TowerPos[dir] = pos;
            if (@event["DataId"] == "2013245")
            {
                P52_Towers[dir] = 1;
            }else if (@event["DataId"] == "2013246")
            {
                P52_Towers[dir] = 2;
            }
            sa.Log.Debug($"P5 Second Trio Tower Direction {dir} is {P52_Towers[dir]} person tower");
            int count = P52_Towers.Count(x => x != 0);
            if (count == 6 && !P5_SigmaBuffIsFar)
            {
                sa.Log.Debug($"P5 Second Trio Found {count} towers on field");
                P52_semaphoreTowersWereConfirmed.Set();
            }
            else if(count == 5 && P5_SigmaBuffIsFar)
            {
                sa.Log.Debug($"P5 Second Trio Found {count} towers on field");
                P52_semaphoreTowersWereConfirmed.Set();
            }
        }
        
        [ScriptMethod(name: "P5_SecondTrio_KnockbackTowerGuidance", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Add","DataId:regex:^(2013245|2013246)$"],userControl: true)]
        public void P5_SecondTrio_KnockbackTowerGuidance(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            Thread.MemoryBarrier();
            P52_semaphoreTowersWereConfirmed.WaitOne();
            Thread.MemoryBarrier();
            P52_Self_Dir = RoundPositionTo16Dir(P52_Self_Pos, new(100, 0, 100));
            accessory.Log.Debug($"P5 Second Trio Before Knockback Tower Player 16-dir Direction {P52_Self_Dir}");
            
            int tempCwDirIndex;
            int tempCcwDirIndex;
            int tempSide = 0;
            int targetIndex = 0;
            

            if (P5_SigmaBuffIsFar)
            {
                accessory.Log.Debug($"Is Far Buff");
                tempCwDirIndex = (P52_Self_Dir + 15) % 16;
                tempCcwDirIndex = (P52_Self_Dir + 1) % 16;
            }
            else
            {
                accessory.Log.Debug($"Is Near Buff");
                tempCwDirIndex = (P52_Self_Dir + 14) % 16;
                tempCcwDirIndex = (P52_Self_Dir + 2) % 16;
            }

            accessory.Log.Debug($"Will check ({tempCwDirIndex} and {tempCcwDirIndex})");
            accessory.Log.Debug($"CWDir: {P52_Towers[tempCwDirIndex]}, CCWDir: {P52_Towers[tempCcwDirIndex]}");
            
            if (P52_Towers[tempCwDirIndex] == 2)
            {
                accessory.Log.Debug($"Facing tower, counter-clockwise direction ({tempCwDirIndex}), has double tower!");
                tempSide = 1;
            }
            else if (P52_Towers[tempCcwDirIndex] == 2)
            {
                accessory.Log.Debug($"Facing tower, clockwise direction ({tempCcwDirIndex}), has double tower");
                tempSide = 2;
            }
            else
            {
                bool cwIsSingle = P52_Towers[tempCwDirIndex] == 1;
                bool ccwIsNotDouble = P52_Towers[tempCcwDirIndex] != 2;

                if (cwIsSingle && ccwIsNotDouble)
                {
                    accessory.Log.Debug($"Facing tower, counter-clockwise direction ({tempCwDirIndex}), only single tower");
                    tempSide = 1;
                }
                else
                {
                    bool ccwIsSingle = P52_Towers[tempCcwDirIndex] == 1;
                    bool cwIsNotDouble = P52_Towers[tempCwDirIndex] != 2;

                    if (ccwIsSingle && cwIsNotDouble)
                    {
                        accessory.Log.Debug($"Facing tower, clockwise direction ({tempCcwDirIndex}), only single tower");
                        tempSide = 2;
                    }
                }
            }

            if (tempSide != 0)
            {
                targetIndex = tempSide == 1 ? tempCwDirIndex : tempCcwDirIndex;
            }

            var str = "Towers: [";
            for (int i = 0; i < P52_Towers.Length; i++)
            {
                str += $"{P52_Towers[i]}, ";
            }
            str += "]";
            accessory.Log.Debug($"{str}");
            
            accessory.Log.Debug($"Target Tower: {targetIndex}");
            
            var dp1 = accessory.Data.GetDefaultDrawProperties();
            dp1.Name = "P52_KnockbackGuidanceStart";
            dp1.Scale = new(2);
            dp1.Owner = accessory.Data.Me;
            dp1.TargetPosition = RotatePoint(new Vector3(100,0,97), new Vector3(100,0,100), targetIndex * float.Pi / 8);
            dp1.ScaleMode |= ScaleMode.YByDistance;
            dp1.Color = accessory.Data.DefaultSafeColor;
            dp1.DestoryAt = 4500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp1);
            
            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = "P52_KnockbackGuidance";
            dp2.Scale = new(1.5f, 13);
            dp2.ScaleMode |= ScaleMode.YByDistance;
            dp2.Owner = accessory.Data.Me;
            dp2.TargetPosition = P52_TowerPos[targetIndex];
            dp2.Rotation = 0;
            dp2.Color = accessory.Data.DefaultSafeColor;
            dp2.DestoryAt = 6500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp2);
        }
        
        [ScriptMethod(name: "P5_SecondTrio_SecondHalf", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(31555)$"],userControl: false)]
        public void P5_SecondTrio_SecondHalf(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            parse = 5.21;
            accessory.Log.Debug($"--- parse changed to {parse}");
        }
        
        [ScriptMethod(name: "P5_SecondThirdFourthPassMarkerRecord", eventType: EventTypeEnum.Marker, eventCondition: ["Operate:Add", "Id:regex:^(0[1-46-7]|09|10)$"], userControl: false)]
        public void P5_SecondThirdFourthPassMarkerRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse < 5.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (accessory.Data.PartyList.IndexOf(tid) != accessory.Data.PartyList.IndexOf(accessory.Data.Me)) return;
            
            P52_MarkType = @event["Id"] switch
            {
                "01" => 1, //Attack 1
                "02" => 2, //Attack 2
                "03" => 3, //Attack 3
                "04" => 4, //Attack 4
                "06" => 5, //Chain 1
                "07" => 6, //Chain 2
                "09" => 7, //Stop 1
                "10" => 8, //Stop 2
            };
            accessory.Log.Debug($"--- Player marker updated at {parse}, is {P52_MarkType}");
        }
        
        [ScriptMethod(name: "P5_SecondTrio_FemalePosition", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7747", "SourceDataId:15720"], userControl: false)]
        public void P5_SecondTrio_FemalePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.21) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if (!P52_OmegaFDirDone)
            {
                P52_OmegaFDir = RoundPositionTo8Dir(pos, new(100, 0, 100));
                P52_OmegaFDirDone = true;
                accessory.Log.Debug($"P52_OmegaFDir:{P52_OmegaFDir}");
            }
        }
        
        [ScriptMethod(name: "P5_SecondTrio_RotatingLaser", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(009C|009D)$"],userControl: true)]
        public void P5_SecondTrio_RotatingLaser(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.21) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            int P52_OmegaFfixDir = P52_OmegaFDir switch
            {
                0 => 0,
                1 => 3,
                2 => 2,
                3 => 1,
                4 => 0,
                5 => 3,
                6 => 2,
                7 => 1,
            };
            
            int dx = -1;
            if (@event["Id"] == "009D")
            {
                dx = 1;
            }
            for (int i = 0; i < 14; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P5_SecondTrio_RotatingLaser_{i}";
                dp.Scale = new(50,12);
                dp.Owner = tid;
                dp.FixRotation = true;
                dp.Rotation = float.Pi / 2 + float.Pi / 4 * P52_OmegaFfixDir + float.Pi / 20 * i * dx;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 10000 + 580*i;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
            }
        }
        
        [ScriptMethod(name: "P5_SecondTrio_SecondHalfStartPointGuidance", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(009C|009D)$"],userControl: true)]
        public void P5_SecondTrio_SecondHalfStartPointGuidance(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.21) return;
            Vector3 dealpos = Vector3.Zero;
            string eventId = @event["Id"];
            var positionMap = new Dictionary<string, Dictionary<bool, Vector3>>
            {
                ["009C"] = new Dictionary<bool, Vector3>
                {
                    { true, new Vector3(93f, 0f, 82f) },
                    { false, new Vector3(107f, 0f, 118f) }
                },
                ["009D"] = new Dictionary<bool, Vector3>
                {
                    { true, new Vector3(107f, 0f, 82f) },
                    { false, new Vector3(93f, 0f, 118f) }
                }
            };
            Vector3 center = new Vector3(100f, 0f, 100f);
            if (positionMap.TryGetValue(eventId, out var idMap))
            {
                bool isSpecialType = P52_MarkType == 7 || P52_MarkType == 8 || P52_MarkType == 1;
                float angle = float.Pi / 4 * P52_OmegaFDir;
                dealpos = RotatePoint(idMap[isSpecialType], center, angle);
            }
            accessory.Log.Debug($"P5_SecondTrio_SecondHalfStartPoint:{dealpos}");
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_SecondTrio_SecondHalfStartPoint";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "P5_SecondTrio_FemaleSkill", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7747", "SourceDataId:15720"], userControl: true)]
        public void P5_SecondTrio_FemaleSkill(Event @event, ScriptAccessory accessory)
        {
            if (P52_OmegaM_Skill || parse != 5.21) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            P52_F_TransformationID = GetTransformationID(sid, accessory);
            P52_OmegaM_Skill = true;
            accessory.Log.Debug($"P52_F_TransformationID:{P52_F_TransformationID}");
            if (P52_F_TransformationID == null) return;
            accessory.Log.Debug($"--- Entered P5_SecondTrio_FemaleSkill");
            if (P52_F_TransformationID == 4)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P5_SecondTrio_FemaleWing1";
                dp.Scale = new(60, 20);
                dp.Owner = sid;
                dp.Rotation = float.Pi / 2;
                dp.Offset = new(-4, 0, 0);
                dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
                dp.Delay = 12000;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P5_SecondTrio_FemaleWing2";
                dp.Scale = new(60, 20);
                dp.Owner = sid;
                dp.Rotation = float.Pi / -2;
                dp.Offset = new(4, 0, 0);
                dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
                dp.Delay = 12000;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P5_SecondTrio_FemaleCross1";
                dp.Scale = new(10, 60);
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
                dp.Delay = 12000;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P5_SecondTrio_FemaleCross2";
                dp.Scale = new(10, 60);
                dp.Rotation = float.Pi / 2;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
                dp.Delay = 12000;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
            }
        }
        
        [ScriptMethod(name: "P5_SecondTrio_SecondPassStartHint", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31631"], userControl: true)]
        public void P5_SecondTrio_SecondPassStartHint(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.21) return;
            if (P52_F_TransformationID == 4)
            {
                accessory.Method.TextInfo("Wait for laser to resolve then move in", 3000, true);
            }
            else
            {
                accessory.Method.TextInfo("Wait for cross to resolve then move in", 5000, true);
            }
        }
        
        [ScriptMethod(name: "P5_SecondTrio_SecondPassGuidance", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(009C|009D)$"],userControl: true)]
        public void P5_SecondTrio_SecondPassGuidance(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.21) return;
            int dx = 1;
            if (@event["Id"] == "009C")
            {
                dx = -1;
            }
            Vector3 dealpos = Vector3.Zero;
            dealpos = P52_MarkType switch
            {
                1 => RotatePoint(new Vector3(100f+ (-19.5f*dx), 0f, 100f), new Vector3(100, 0, 100),
                    float.Pi / 4 * P52_OmegaFDir),
                2 => RotatePoint(new Vector3(100f+ 19.5f*dx, 0f, 100f), new Vector3(100, 0, 100),
                    float.Pi / 4 * P52_OmegaFDir),
                3 => RotatePoint(new Vector3(102f, 0f, 111f), new Vector3(100, 0, 100),
                    float.Pi / 4 * P52_OmegaFDir),
                4 => RotatePoint(new Vector3(105.26f, 0f, 118.26f), new Vector3(100, 0, 100),
                    float.Pi / 4 * P52_OmegaFDir),
                5 => RotatePoint(new Vector3((100 + dx * 10), 0f, 100f), new Vector3(100, 0, 100),
                    float.Pi / 4 * P52_OmegaFDir),
                6 => RotatePoint(new Vector3(94.74f, 0f, 118.26f), new Vector3(100, 0, 100),
                    float.Pi / 4 * P52_OmegaFDir),
                7 => RotatePoint(new Vector3(86.56f, 0f, 86.56f), new Vector3(100, 0, 100),
                    float.Pi / 4 * P52_OmegaFDir),
                8 => RotatePoint(new Vector3(113.44f, 0f, 86.56f), new Vector3(100, 0, 100),
                    float.Pi / 4 * P52_OmegaFDir),
            };
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_SecondTrio_SecondPass";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = P52_F_TransformationID == 4? 10000 : 13000;
            dp.DestoryAt = P52_F_TransformationID == 4? 9000 : 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        
        [ScriptMethod(name: "P5_ThirdTrio_PhaseSplit", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32789"], userControl: false)]
        public void P5_ThirdTrio_PhaseSplit(Event @event, ScriptAccessory accessory)
        {
            parse = 5.3;
        }
        
        [ScriptMethod(name: "P5_ThirdTrio_DiffuseWaveCannon", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(31643|31644)$"], userControl: true)]
        public void P5_ThirdTrio_DiffuseWaveCannon(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.3) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            float rot = JsonConvert.DeserializeObject<int>(@event["ActionId"]) == 31644 ? float.Pi / 2 : 0;
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_ThirdTrio_DiffuseWaveCannon_First";
            dp.Scale = new(20);
            dp.Radian = float.Pi / 3 * 2;
            dp.Owner = sid;
            dp.Rotation = rot;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_ThirdTrio_DiffuseWaveCannon_First";
            dp.Scale = new(20);
            dp.Radian = float.Pi / 3 * 2;
            dp.Owner = sid;
            dp.Rotation = rot + float.Pi;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_ThirdTrio_DiffuseWaveCannon_Second";
            dp.Scale = new(20);
            dp.Radian = float.Pi / 3 * 2;
            dp.Owner = sid;
            dp.Rotation = rot + float.Pi / 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 9000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_ThirdTrio_DiffuseWaveCannon_Second";
            dp.Scale = new(20);
            dp.Radian = float.Pi / 3 * 2;
            dp.Owner = sid;
            dp.Rotation = rot + float.Pi + float.Pi / 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 9000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            
        }
        
        [ScriptMethod(name: "P5_ThirdTrio_MaleFemaleCombo", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7747", "SourceDataId:regex:^(15721|15722)$"],userControl: true)]
        public void P5_ThirdTrio_MaleFemaleCombo(Event @event, ScriptAccessory accessory)
        {
            if(parse != 5.3) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            P5_3_MF++;
            var transformationID = GetTransformationID(sid, accessory);
            if (transformationID == null) return;
            if (@event["SourceDataId"] == "15721")
            {
                //Male
                if (transformationID == 0)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"P5_ThirdTrio_MaleCircle{P5_3_MF}";
                    dp.Scale = new(10);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = P5_3_MF < 3 ? 0 : 9000;
                    dp.DestoryAt = P5_3_MF < 3? 13000 : 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (transformationID == 4)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"P5_ThirdTrio_MaleDonut{P5_3_MF}";
                    dp.Scale = new(40);
                    dp.InnerScale = new(10);
                    dp.Radian = float.Pi * 2;
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = P5_3_MF < 3 ? 0 : 9000;
                    dp.DestoryAt = P5_3_MF < 3? 13000 : 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                }
            }
            if (@event["SourceDataId"] == "15722")
            {
                if (transformationID == 0)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"P5_ThirdTrio_FemaleCross1_{P5_3_MF}";
                    dp.Scale = new(10, 60);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = P5_3_MF < 3 ? 0 : 9000;
                    dp.DestoryAt = P5_3_MF < 3? 13000 : 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"P5_ThirdTrio_FemaleCross2_{P5_3_MF}";
                    dp.Scale = new(10, 60);
                    dp.Rotation = float.Pi / 2;
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = P5_3_MF < 3 ? 0 : 9000;
                    dp.DestoryAt = P5_3_MF < 3? 13000 : 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                }
                if (transformationID == 4)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"P5_ThirdTrio_FemaleWing1_{P5_3_MF}";
                    dp.Scale = new(60, 20);
                    dp.Owner = sid;
                    dp.Rotation = float.Pi / 2;
                    dp.Offset = new(-4, 0, 0);
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
                    dp.Delay = P5_3_MF < 3 ? 0 : 9000;
                    dp.DestoryAt = P5_3_MF < 3? 13000 : 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"P5_ThirdTrio_FemaleWing2_{P5_3_MF}";
                    dp.Scale = new(60, 20);
                    dp.Owner = sid;
                    dp.Rotation = float.Pi / -2;
                    dp.Offset = new(4, 0, 0);
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
                    dp.Delay = P5_3_MF < 3 ? 0 : 9000;
                    dp.DestoryAt = P5_3_MF < 3? 13000 : 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
            }
        }
        
        [ScriptMethod(name: "P5_ThirdTrio_MaleFemalePositionSkillHandling", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7747", "SourceDataId:regex:^(15721|15722)$"],userControl: false)]
        public void P5_ThirdTrio_MaleFemalePositionSkillHandling(Event @event, ScriptAccessory accessory)
        {
            if(parse != 5.3) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var transformationID = GetTransformationID(sid, accessory);
            if (transformationID == null) return;
            P5_3_MFT++;
			accessory.Log.Debug($"P5_3_MFT:{P5_3_MFT}");
            if (@event["SourceDataId"] == "15721")
            {
                //Male
                if (@event.SourcePosition.X < 100 && @event.SourcePosition.Z < 100)
                {
                    MFPositions[P5_3_MFT <= 2 ? 0 : 2] = 0;
                }else if (@event.SourcePosition.X < 100 && @event.SourcePosition.Z > 100)
                {
                    MFPositions[P5_3_MFT <= 2 ? 0 : 2] = 1;
                }else if (@event.SourcePosition.X > 100 && @event.SourcePosition.Z > 100)
                {
                    MFPositions[P5_3_MFT <= 2 ? 0 : 2] = 2;
                }else if (@event.SourcePosition.X > 100 && @event.SourcePosition.Z < 100)
                {
                    MFPositions[P5_3_MFT <= 2 ? 0 : 2] = 3;
                }
                
                if (transformationID == 0)
                {
                    MFTransformStates[P5_3_MFT <= 2 ? 0 : 2] = 0;
                }
                if (transformationID == 4)
                {
                    MFTransformStates[P5_3_MFT <= 2 ? 0 : 2] = 1;
                }
            }
            if (@event["SourceDataId"] == "15722")
            {
                if (@event.SourcePosition.X < 100 && @event.SourcePosition.Z < 100)
                {
                    MFPositions[P5_3_MFT <= 2 ? 1 : 3] = 0;
                }else if (@event.SourcePosition.X < 100 && @event.SourcePosition.Z > 100)
                {
                    MFPositions[P5_3_MFT <= 2 ? 1 : 3] = 1;
                }else if (@event.SourcePosition.X > 100 && @event.SourcePosition.Z > 100)
                {
                    MFPositions[P5_3_MFT <= 2 ? 1 : 3] = 2;
                }else if (@event.SourcePosition.X > 100 && @event.SourcePosition.Z < 100)
                {
                    MFPositions[P5_3_MFT <= 2 ? 1 : 3] = 3;
                }
                
                if (transformationID == 0)
                {
                    MFTransformStates[P5_3_MFT <= 2 ? 1 : 3] = 0;
                }
                if (transformationID == 4)
                {
                    MFTransformStates[P5_3_MFT <= 2 ? 1 : 3] = 1;
                }
            }
        }
        
        [ScriptMethod(name: "P5_ThirdTrio_FirstHalfGuidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(31643|31644)$"], userControl: true)]
        public void P5_ThirdTrio_FirstHalfGuidance(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.3) return;
            int type = JsonConvert.DeserializeObject<int>(@event["ActionId"]) == 31643 ? 0 : 1;
            int type2 = JsonConvert.DeserializeObject<int>(@event["ActionId"]) == 31643 ? 1 : 0;
            Combo1 = MFTransformStates[0] + 2 * MFTransformStates[1];
            Combo2 = MFTransformStates[2] + 2 * MFTransformStates[3];
            accessory.Log.Debug($"Combo1: {Combo1}, Combo2: {Combo2}");
            accessory.Log.Debug($"Male1:{MFPositions[0]} Female1:{MFPositions[1]} Male2:{MFPositions[2]} Female2:{MFPositions[3]}");
            accessory.Log.Debug($"type:{type},type2:{type2}");
            //0 for front/back, 1 for left/right
            var P53Tuple1 = (Combo1, MFPositions[0], MFPositions[1], type);
            var P53Tuple2 = (Combo2, MFPositions[2], MFPositions[3], type2);
            Vector3 dealpos1 = P53SafePos.TryGetValue(P53Tuple1, out var safePos) ? safePos : default;
            Vector3 dealpos2 = P53SafePos.TryGetValue(P53Tuple2, out var safePos1) ? safePos1 : default;;
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_ThirdTrio_FirstHalfGuidance_1";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos1;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_ThirdTrio_FirstHalfGuidance_1-2";
            dp.Scale = new(2);
            dp.Position = dealpos1;
            dp.TargetPosition = dealpos2;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_ThirdTrio_FirstHalfGuidance_2";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos2;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 9000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        }
        
        [ScriptMethod(name: "P5_ThirdTrio_SearchWaveCannon", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(31638|31639)$"], userControl: true)]
        public void P5_ThirdTrio_SearchWaveCannon(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.3) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            int dir = @event["ActionId"] == "31638" ? 1 : -1;
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_ThirdTrio_SearchWaveCannon";
            dp.Scale = new(20);
            dp.Radian = float.Pi;
            dp.Owner = sid;
            dp.Rotation = float.Pi + float.Pi / 2 * dir;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        
        [ScriptMethod(name: "P5_ThirdTrio_ThirdPassGuidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(31638|31639)$"], userControl: true)]
        public void P5_ThirdTrio_ThirdPass(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(1500).Wait();
            if (parse != 5.3) return;
            int dir = @event["ActionId"] == "31638" ? 2 : -2;

            var dealpos = P52_MarkType switch
            {
                1 => RotatePoint(new Vector3(81f, 0f, 101f), new Vector3(100, 0, 100), float.Pi / 4 * dir),
                2 => RotatePoint(new Vector3(119f, 0f, 101f), new Vector3(100, 0, 100), float.Pi / 4 * dir),
                3 => RotatePoint(new Vector3(102f, 0f, 111f), new Vector3(100, 0, 100), float.Pi / 4 * dir),
                4 => RotatePoint(new Vector3(105.26f, 0f, 118.26f), new Vector3(100, 0, 100), float.Pi / 4 * dir),
                5 => RotatePoint(new Vector3(90f, 0f, 101f), new Vector3(100, 0, 100), float.Pi / 4 * dir),
                6 => RotatePoint(new Vector3(94.74f, 0f, 118.26f), new Vector3(100, 0, 100), float.Pi / 4 * dir),
                7 => RotatePoint(new Vector3(90.8f, 0f, 90.8f), new Vector3(100, 0, 100), float.Pi / 4 * dir),
                8 => RotatePoint(new Vector3(109.2f, 0f, 90.8f), new Vector3(100, 0, 100), float.Pi / 4 * dir)
            };
            
            accessory.Log.Debug($"Drawing Third Pass Guidance to {dealpos}");
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_ThirdTrio_ThirdPass";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 8500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            parse = 5.4;
        }
        
        [ScriptMethod(name: "P5_ThirdTrio_FourthPassGuidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32374"], userControl: true)]
        public void P5_ThirdTrio_FourthPass(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(2500).Wait();
            if (parse != 5.4) return;
            var dealpos = P52_MarkType switch
            {
                1 => RotatePoint(new Vector3(81f, 0f, 101f), new Vector3(100, 0, 100), float.Pi / 4 * P53_4_HW),
                2 => RotatePoint(new Vector3(119f, 0f, 101f), new Vector3(100, 0, 100), float.Pi / 4 * P53_4_HW),
                3 => RotatePoint(new Vector3(102f, 0f, 111f), new Vector3(100, 0, 100), float.Pi / 4 * P53_4_HW),
                4 => RotatePoint(new Vector3(105.26f, 0f, 118.26f), new Vector3(100, 0, 100), float.Pi / 4 * P53_4_HW),
                5 => RotatePoint(new Vector3(90f, 0f, 101f), new Vector3(100, 0, 100), float.Pi / 4 * P53_4_HW),
                6 => RotatePoint(new Vector3(94.74f, 0f, 118.26f), new Vector3(100, 0, 100), float.Pi / 4 * P53_4_HW),
                7 => RotatePoint(new Vector3(89.7f, 0f, 83.5f), new Vector3(100, 0, 100), float.Pi / 4 * P53_4_HW),
                8 => RotatePoint(new Vector3(110.3f, 0f, 83.5f), new Vector3(100, 0, 100), float.Pi / 4 * P53_4_HW)
            };
            accessory.Log.Debug($"Drawing Fourth Pass Guidance to {dealpos}");
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_ThirdTrio_FourthPass";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 8500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        
        [ScriptMethod(name: "P5_ThirdTrio_FourthPassDirectionGet", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:15724"], userControl: false)]
        public void P5_ThirdTrio_FourthPassDirectionGet(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.3) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            P53_4_HW = RoundPositionTo8Dir(pos, new(100, 0, 100));
            accessory.Log.Debug($"Fourth Pass Base Direction is {P53_4_HW}");
        }
        #endregion

        #region P6
        [ScriptMethod(name: "P6TransitionRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31649"], userControl: false)]
        public void P6TransitionRecord(Event @event, ScriptAccessory accessory)
        {
            parse = 6;
        }
	    
        [ScriptMethod(name: "CosmoArrowCount", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31650"], userControl: false)]
        public void CosmoArrowCount(Event @event, ScriptAccessory accessory)
        {
            ArrowNum++;
            isSet = false;
		    ArrowModeConfirmed = new System.Threading.AutoResetEvent(false);
        }

        [ScriptMethod(name: "CosmoArrow", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31651"], userControl: false)]
        public void CosmoArrow(Event @event, ScriptAccessory accessory)
        {
            var casterPos = @event.SourcePosition();
            var center = MapCenter;
            var offset = casterPos - center;
            // Mode detection
            if (Math.Abs(offset.X) < 5) // Central vertical line
            {
                if (!isSet)
                {
                    arrowMode = 0;
                    isSet = true;
				    System.Threading.Thread.MemoryBarrier();
				    ArrowModeConfirmed.Set();
                }
            }
            else if (Math.Abs(offset.Z) < 5) // Central horizontal line
            {
                if (!isSet)
                {
                    arrowMode = 0;
                    isSet = true;
				    System.Threading.Thread.MemoryBarrier();
				    ArrowModeConfirmed.Set();
                }
            }
            else if (Math.Abs(offset.X) < 18) // Side vertical line
            {
                if (!isSet)
                {
                    arrowMode = 1;
                    isSet = true;
				    System.Threading.Thread.MemoryBarrier();
				    ArrowModeConfirmed.Set();
                }
            }
            else if (Math.Abs(offset.Z) < 18) // Side horizontal line
            {
                if (!isSet)
                {
                    arrowMode = 1;
                    isSet = true;
				    System.Threading.Thread.MemoryBarrier();
				    ArrowModeConfirmed.Set();
                }
            }
        }

		[ScriptMethod(name: "CosmoArrowDraw", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31651"], userControl: true)]
        public void CosmoArrowDraw(Event @event, ScriptAccessory accessory)
        {
            var casterPos = @event.SourcePosition();
            var center = MapCenter;
            var offset = casterPos - center;
            // Mode detection
            if (Math.Abs(offset.X) < 5) // Central vertical line
            {
                GenerateLine(accessory, casterPos, new Vector2(1, 0), 4, InOut);
                GenerateLine(accessory, casterPos, new Vector2(-1, 0), 4, InOut);
            }
            else if (Math.Abs(offset.Z) < 5) // Central horizontal line
            {
                GenerateLine(accessory, casterPos, new Vector2(0, 1), 4, InOut);
                GenerateLine(accessory, casterPos, new Vector2(0, -1), 4, InOut);
            }
            else if (Math.Abs(offset.X) < 18) // Side vertical line
            {
                GenerateLine(accessory, casterPos, new Vector2(offset.X < 0 ? 1 : -1, 0), 7, OutIn);
            }
            else if (Math.Abs(offset.Z) < 18) // Side horizontal line
            {
                GenerateLine(accessory, casterPos, new Vector2(0, offset.Z < 0 ? 1 : -1), 7, OutIn);
            }
        }

        private void GenerateLine(ScriptAccessory accessory, Vector3 origin, Vector2 direction, int steps, string pos)
        {
            for (int i = 0; i < steps; i++)
            {
                if (i == 0)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"ArrowSegment_{direction}_{i}_{pos}";
                    dp.Color = ArrorColor.V4;
                    dp.Position = origin;
                    dp.Scale = new Vector2(10, 40); // Width 10, Length 100
                    dp.Rotation = -MathF.Atan2(direction.Y, direction.X);
                    dp.Delay = 0;
                    dp.DestoryAt = 8000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
                else
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"ArrowSegment_{direction}_{i}_{pos}";
                    dp.Color = ArrorColor.V4;
                    dp.Position = origin + new Vector3(direction.X * 5f * i + direction.X * 2.5f , 0, direction.Y * 5f * i + direction.Y * 2.5f);
                    dp.Scale = new Vector2(5, 100); // Width 5, Length 100
                    dp.Rotation = MathF.Atan2(direction.Y, direction.X);
                    dp.Delay = 6000 + i*2000;
                    dp.DestoryAt = 2000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                }
            }
        }

        [ScriptMethod(name: "CosmoArrowGuidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31651"], suppress: 3000)]
        public async void CosmoArrowGuidance(Event @event, ScriptAccessory accessory)
        {
            System.Threading.Thread.MemoryBarrier();
		    
		    ArrowModeConfirmed.WaitOne();
		    System.Threading.Thread.MemoryBarrier();

		    var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
		    if (followCrowd)
		    {
			    myindex = ArrowNum < 1 ? 4 : accessory.Data.PartyList.IndexOf(accessory.Data.Me);
		    }
		    else
		    {
			    myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
		    }
            int offset1 = arrowMode < 1 ? 13 : 17;
            int offset2 = arrowMode < 1 ? 7: 23;
            int offset3 = arrowMode < 1 ? 23 : 17;
            var offset4 = 12.5f;
            var offset5 = 7.5f;
            var delayMode = arrowMode < 1 ? 4000 : 2000;
            var delayModeTN = arrowMode < 1 && myindex < 4 ? 2000 : 0;
            // accessory.Method.TextInfo($"{arrowMode}_{delayMode}_{delayModeTN}", duration: 5000, true);
            
            Vector3 dealpos0 = default; // Start point
            
            Vector3 dealpos1 = default; // First pass through
            Vector3 dealpos2 = default; // Second pass through
            Vector3 dealpos3 = default; // Third pass through
            Vector3 dealpos4 = default; // Fourth pass through
            Vector3 coordinate0 = arrowMode < 1 ? new Vector3(93.5f, 0, 93.5f) : new(91.5f, 0, 91.5f);
            Vector3 coordinate1 = arrowMode < 1 ? new Vector3(96.5f, 0, 96.5f) : new(88.5f, 0, 88.5f);
            Vector3 coordinate2 = arrowMode < 1 ? new Vector3(88.5f, 0, 88.5f) : new(91.5f, 0, 91.5f);
            Vector3 coordinate3 = new Vector3(100.0f, 0, 87.5f);
            Vector3 coordinate4 = new Vector3(100.0f, 0, 92.5f);
            dealpos0 = myindex switch
            {
                // MT and D3 (base coordinate)
                0 or 6 => coordinate0,
                
                // ST and D4 (X axis +offset1)
                1 or 7 => new Vector3(coordinate0.X + offset1, 0, coordinate0.Z),
                
                // H1 and D1 (Z axis +offset1)
                2 or 4 => new Vector3(coordinate0.X, 0, coordinate0.Z + offset1),
                
                // H2 and D2 (X/Z axes +offset1)
                3 or 5 => new Vector3(coordinate0.X + offset1, 0, coordinate0.Z + offset1),
                    
                _ => default
            };
            dealpos1 = myindex switch
            {
                // MT and D3 (base coordinate)
                0 or 6 => coordinate1,
                
                // ST and D4
                1 or 7 => new Vector3(coordinate1.X + offset2, 0, coordinate1.Z),
                
                // H1 and D1
                2 or 4 => new Vector3(coordinate1.X, 0, coordinate1.Z + offset2),
                
                // H2 and D2
                3 or 5 => new Vector3(coordinate1.X + offset2, 0, coordinate1.Z + offset2),
                    
                _ => default
            };
            dealpos2 = myindex switch
            {
                0 => arrowMode < 1 ? new Vector3(94.0f, 0, 90.0f) : coordinate2,
                1 => arrowMode < 1 ? new Vector3(110.0f, 0, 94.0f) : new Vector3(coordinate2.X + offset3, 0, coordinate2.Z),
                2 => arrowMode < 1 ? new Vector3(90.0f, 0, 106.0f) : new Vector3(coordinate2.X, 0, coordinate2.Z + offset3),
                3 => arrowMode < 1 ? new Vector3(106f, 0, 110.0f) : new Vector3(coordinate2.X + offset3, 0, coordinate2.Z + offset3),
                6 => coordinate2,
                7 => new Vector3(coordinate2.X + offset3, 0, coordinate2.Z),
                4 => new Vector3(coordinate2.X, 0, coordinate2.Z + offset3),
                5 => new Vector3(coordinate2.X + offset3, 0, coordinate2.Z + offset3),
                _ => default
            };
            dealpos3 = myindex switch
            {
                0 => coordinate3,
                1 => new Vector3(coordinate3.X + offset4, 0, coordinate3.Z + offset4),
                2 => new Vector3(coordinate3.X - offset4, 0, coordinate3.Z + offset4),
                3 => new Vector3(coordinate3.X, 0, coordinate2.Z + (offset4 * 2)),
                4 => arrowMode < 1 ? new Vector3(88.5f, 0, 111.5f) : new Vector3(coordinate1.X, 0, coordinate1.Z + offset2),
                5 => arrowMode < 1 ? new Vector3(111.5f, 0, 111.5f) : new Vector3(coordinate1.X + offset2, 0, coordinate1.Z + offset2),
                6 => arrowMode < 1 ? new Vector3(88.5f, 0, 88.5f) : coordinate1,
                7 => arrowMode < 1 ? new Vector3(111.5f, 0, 88.5f) : new Vector3(coordinate1.X + offset2, 0, coordinate1.Z),
                _ => default
            };
            dealpos4 = myindex switch
            {
                0 => coordinate4,
                1 => new Vector3(coordinate4.X + offset5, 0, coordinate4.Z + offset5),
                2 => new Vector3(coordinate4.X - offset5, 0, coordinate4.Z + offset5),
                3 => new Vector3(coordinate4.X, 0, coordinate4.Z + (offset5 * 2)),
                4 => new Vector3(91.5f, 0, 108.5f),
                5 => new Vector3(108.5f, 0, 108.5f),
                6 => new Vector3(91.5f, 0, 91.5f),
                7 => new Vector3(108.5f, 0, 91.5f),
                _ => default
            };
            if (dealpos0 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P6_CosmoArrow_PrePosition";
                dp.Scale = new(ArrowScale);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos0;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (dealpos0 != default && dealpos1 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P6_CosmoArrow_1-2";
                dp.Scale = new(ArrowScale);
                dp.Position = dealpos0;
                dp.TargetPosition = dealpos1;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P6_CosmoArrow_2";
                dp.Scale = new(ArrowScale);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos1;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 7500;
                dp.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (dealpos1 != default && dealpos2 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P6_CosmoArrow_2-3";
                dp.Scale = new(ArrowScale);
                dp.Position = dealpos1;
                dp.TargetPosition = dealpos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 7500;
                dp.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P6_CosmoArrow_3";
                dp.Scale = new(ArrowScale);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 11500;
                dp.DestoryAt = 2000 + delayModeTN;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            if (dealpos2 != default && dealpos3 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P6_CosmoArrow_3-4";
                dp.Scale = new(ArrowScale);
                dp.Position = dealpos2;
                dp.TargetPosition = dealpos3;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 11500;
                dp.DestoryAt = 2000 + delayModeTN;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P6_CosmoArrow_4";
                dp.Scale = new(ArrowScale);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos3;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 13500 + delayModeTN;
                dp.DestoryAt = delayMode - delayModeTN;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (dealpos2 != default && dealpos3 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P6_CosmoArrow_4-5";
                dp.Scale = new(ArrowScale);
                dp.Position = dealpos3;
                dp.TargetPosition = dealpos4;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 13500;
                dp.DestoryAt = delayMode;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P6_CosmoArrow_5";
                dp.Scale = new(ArrowScale);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos4;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 13500 + delayMode;
                dp.DestoryAt = 2000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

        }
        
        // [ScriptMethod(name: "Exaflare8DirGuidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31663"], userControl: true)]
        [ScriptMethod(name: "Exaflare8DirGuidance", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31657"], userControl: true)]
        public void ExaflareCount(Event @event, ScriptAccessory accessory)
        {
			if (parse != 6) return;
            
            if (CannonCasted) return;
            CannonCasted = true;
			// lock (CannonNumLock)
			// {
			// 	if (CannonNum < 48) CannonNum++;
			// 	if (CannonNum == 48)
            // {
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var pos = myindex switch
            {
                0 => new Vector3(99.97f, -0.00f, 86.97f),
                1 => new Vector3(113.15f, -0.00f, 100.07f),
                2 => new Vector3(86.91f, 0.00f, 100.03f),
                3 => new Vector3(100.04f, -0.00f, 112.89f),
                4 => new Vector3(90.66f, -0.00f, 109.15f),
                5 => new Vector3(109.40f, -0.00f, 109.29f),
                6 => new Vector3(90.67f, 0.00f, 90.78f),
                7 => new Vector3(109.47f, 0.00f, 90.83f),
                _ => default
            };
        
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P6_Exaflare8Dir";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = pos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            // }
			// } 
        }
        
        [ScriptMethod(name: "SteppingExaflareCount", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31661"], userControl: false)]
        public void SteppingExaflareCount(Event @event, ScriptAccessory accessory)
        {
			if (parse != 6) return;
            var pos = @event.SourcePosition();
            StepCannon[(StepCannonIndex++)%4] = pos;
        }
        
        [ScriptMethod(name: "SteppingExaflareGuidance", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31660"], userControl: true)]
        public void SteppingExaflareGuidance(Event @event, ScriptAccessory accessory)
        {
            
            var c1 = StepCannon[0];
            var c2 = StepCannon[1];
            float a = (MathF.Atan2(c1.X - 100, c1.Z - 100) - MathF.Atan2(c2.X - 100, c2.Z - 100)) / float.Pi * 180;
            if (a>180) a=a-360;
            if (a<-180) a=a+360;
            var c1e = new Vector3((c1.X - 100) / 24 * 18 + 100, 0, (c1.Z - 100) / 24 * 18 + 100);
            
            var end = RotatePointFromCentre(c1e, MapCenter, a*-1.5f);
			if (a < 0) accessory.Method.TextInfo($"Face outward, go left", 8000, true);
            if (a > 0) accessory.Method.TextInfo($"Face outward, go right", 8000, true);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P6_ExaflareStartPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = end;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        private static Vector3 RotatePointFromCentre(Vector3 point, Vector3 centre, float angleDegrees)
        {
            float dx = point.X - centre.X;
            float dz = point.Z - centre.Z;
            float thetaRad = MathF.Atan2(dx, dz);
            float normalizedAngle = (1f - (thetaRad / MathF.PI)) % 2f;
            if (normalizedAngle < 0) normalizedAngle += 2f;
            float baseRotation = normalizedAngle * 180f;
            float totalRotation = (baseRotation + angleDegrees) * MathF.PI / 180f;
            float distance = MathF.Sqrt(dx * dx + dz * dz);
            return new Vector3(
                centre.X + MathF.Sin(totalRotation) * distance,
                0f, 
                centre.Z - MathF.Cos(totalRotation) * distance 
            );
        }
	    
        [ScriptMethod(name: "MeteorFlareMarker", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:015A"], userControl: true)]
        public void MeteorFlareMarker(Event @event, ScriptAccessory accessory)
        {
	        if (parse != 6 || @event.TargetId() == 0) return;
	        var tid = @event.TargetId();
	        var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P6_MeteorFlareMarker";
            dp.Scale = new(20);
            dp.Owner = tid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }    
            

        #endregion
        
        #region Class Functions

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
            /// Add priority for a specific key
            /// </summary>
            /// <param name="idx">key</param>
            /// <param name="priority">priority value</param>
            public void AddPriority(int idx, int priority)
            {
                Priorities[idx] += priority;
            }

            /// <summary>
            /// Find the first num smallest values from Priorities, return new Dict
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public List<KeyValuePair<int, int>> SelectSmallPriorityIndices(int num)
            {
                return SelectMiddlePriorityIndices(0, num);
            }

            /// <summary>
            /// Find the first num largest values from Priorities, return new Dict
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public List<KeyValuePair<int, int>> SelectLargePriorityIndices(int num)
            {
                return SelectMiddlePriorityIndices(0, num, true);
            }

            /// <summary>
            /// Find middle values from Priorities in ascending order, return new Dict
            /// </summary>
            /// <param name="skip">Skip skip elements. If starting from second, skip=1</param>
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
                        .OrderByDescending(pair => pair.Value) // Sort by value first
                        .ThenBy(pair => pair.Key) // Then by key
                        .Skip(skip) // Skip first skip elements
                        .Take(num); // Take first num key-value pairs
                }
                else
                {
                    // Sort by value ascending, then by key
                    sortedPriorities = Priorities
                        .OrderBy(pair => pair.Value) // Sort by value first
                        .ThenBy(pair => pair.Key) // Then by key
                        .Skip(skip) // Skip first skip elements
                        .Take(num); // Take first num key-value pairs
                }

                return sortedPriorities.ToList();
            }

            /// <summary>
            /// Find the idx-th element from Priorities in ascending order, return new Dict
            /// </summary>
            /// <param name="idx"></param>
            /// <param name="descending">Descending order, default false</param>
            /// <returns></returns>
            public KeyValuePair<int, int> SelectSpecificPriorityIndex(int idx, bool descending = false)
            {
                var sortedPriorities = SelectMiddlePriorityIndices(0, Priorities.Count, descending);
                return sortedPriorities[idx];
            }

            /// <summary>
            /// Find data for corresponding key from Priorities, return its position after sorting
            /// </summary>
            /// <param name="key"></param>
            /// <param name="descending">Descending order, default false</param>
            /// <returns></returns>
            public int FindPriorityIndexOfKey(int key, bool descending = false)
            {
                var sortedPriorities = SelectMiddlePriorityIndices(0, Priorities.Count, descending);
                var i = 0;
                foreach (var dict in sortedPriorities)
                {
                    if (dict.Key == key) return i;
                    i++;
                }

                return i;
            }

            /// <summary>
            /// Add priority values at once
            /// Usually for special priorities (like H-T-D-H)
            /// </summary>
            /// <param name="priorities"></param>
            public void AddPriorities(List<int> priorities)
            {
                if (Priorities.Count != priorities.Count)
                    throw new ArgumentException("Input list length differs from internal setting");

                for (var i = 0; i < Priorities.Count; i++)
                    AddPriority(i, priorities[i]);
            }

            /// <summary>
            /// Output priority dictionary key and priority
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

            public void AddActionCount(int count = 1)
            {
                ActionCount += count;
            }

        }
        
        #endregion
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
        private int RoundPositionTo4Dir(Vector3 point, Vector3 centre)
        {
            var r = Math.Round(2 - 2 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 4;
            return (int)r;
        }
        private int RoundPositionTo8Dir(Vector3 point, Vector3 centre)
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r = Math.Round(4 - 4 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 8;
            return (int)r;

        }
        
        private int RoundPositionTo16Dir(Vector3 point, Vector3 centre)
        {
            var r = Math.Round(8 - 8 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 16;
            return (int)r;
        }
        
        private int FloorPositionTo4Dir(Vector3 point, Vector3 centre)
        {
            var r = Math.Floor(2 - 2 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 4;
            return (int)r;
        }
        private int FloorPositionTo8Dir(Vector3 point, Vector3 centre)
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r = Math.Floor(4 - 4 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 8;
            return (int)r;

        }
        private Vector3 RotatePoint(Vector3 point, Vector3 centre, float radian)
        {

            Vector2 v2 = new(point.X - centre.X, point.Z - centre.Z);

            var rot = (MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian);
            var lenth = v2.Length();
            return new(centre.X + MathF.Sin(rot) * lenth, centre.Y, centre.Z - MathF.Cos(rot) * lenth);
        }
        
        private byte? GetTransformationID(uint _id, ScriptAccessory accessory)
        {
            var obj = accessory.Data.Objects.SearchById(_id);
            if (obj != null)
            {
                unsafe
                {
                    FFXIVClientStructs.FFXIV.Client.Game.Character.Character* objStruct = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)obj.Address;
                    return objStruct->Timeline.ModelState;
                }
            }
            return null;
        }
        
        public static void SetRotation(ScriptAccessory sa, IGameObject? obj, float rotation)
        {
            if (obj == null || !obj.IsValid())
            {
                sa.Log.Error($"Input IGameObject invalid.");
                return;
            }
            unsafe
            {
                GameObject* charaStruct = (GameObject*)obj.Address;
                charaStruct->SetRotation(rotation);
            }
            sa.Log.Debug($"SetRotation => {obj.Name.TextValue} | {obj} => {rotation}");
        }
    }
}

public static class EventExtensions
{
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

    public static uint Id0(this Event @event)
    {
        return ParseHexId(@event["Id"], out var id) ? id : 0;
    }
    
    public static uint ActionId(this Event @event)
    {
        return JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
    }

    public static uint SourceId(this Event @event)
    {
        return ParseHexId(@event["SourceId"], out var id) ? id : 0;
    }


    public static string DurationMilliseconds(this Event @event)
    {
        return JsonConvert.DeserializeObject<string>(@event["DurationMilliseconds"]) ?? string.Empty;
    }

    public static float SourceRotation(this Event @event)
    {
        return float.TryParse(@event["SourceRotation"], out var rot) ? rot : 0;
    }

    public static byte Index(this Event @event)
    {
        return (byte)(ParseHexId(@event["Index"], out var index) ? index : 0);
    }

    public static uint State(this Event @event)
    {
        return ParseHexId(@event["State"], out var state) ? state : 0;
    }

    public static string SourceName(this Event @event)
    {
        return JsonConvert.DeserializeObject<string>(@event["SourceName"]) ?? string.Empty;
    }

    public static uint TargetId(this Event @event)
    {
        return ParseHexId(@event["TargetId"], out var id) ? id : 0;
    }

    public static Vector3 SourcePosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
    }

    public static Vector3 TargetPosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
    }

    public static Vector3 EffectPosition(this Event @event)
    {
        return JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
    }
}


#region Calculation Functions
public static class DirectionCalc
{
    public static float DegToRad(this float deg) => (deg + 360f) % 360f / 180f * float.Pi;
    public static float RadToDeg(this float rad) => (rad + 2 * float.Pi) % (2 * float.Pi) / float.Pi * 180f;
    
    // Establish list with North as 0
    // Game         List    Logic
    // 0            - 4     pi
    // 0.25 pi      - 3     0.75pi
    // 0.5 pi       - 2     0.5pi
    // 0.75 pi      - 1     0.25pi
    // pi           - 0     0
    // 1.25 pi      - 7     1.75pi
    // 1.5 pi       - 6     1.5pi
    // 1.75 pi      - 5     1.25pi
    // Logic = Pi - Game (+ 2pi)

    /// <summary>
    /// Convert game base angle (South 0, CCW increase) to logic base angle (North 0, CW increase)
    /// Algorithm identical to Logic2Game, kept separate for code readability.
    /// </summary>
    /// <param name="radian">Game base angle</param>
    /// <returns>Logic base angle</returns>
    public static float Game2Logic(this float radian)
    {
        // if (r < 0) r = (float)(r + 2 * Math.PI);
        // if (r > 2 * Math.PI) r = (float)(r - 2 * Math.PI);

        var r = float.Pi - radian;
        r = (r + float.Pi * 2) % (float.Pi * 2);
        return r;
    }

    /// <summary>
    /// Convert logic base angle (North 0, CW increase) to game base angle (South 0, CCW increase)
    /// Algorithm identical to Game2Logic, kept separate for code readability.
    /// </summary>
    /// <param name="radian">Logic base angle</param>
    /// <returns>Game base angle</returns>
    public static float Logic2Game(this float radian)
    {
        // var r = (float)Math.PI - radian;
        // if (r < Math.PI) r = (float)(r + 2 * Math.PI);
        // if (r > Math.PI) r = (float)(r - 2 * Math.PI);

        return radian.Game2Logic();
    }

    /// <summary>
    /// For rotation, FFXIV game base CW rotation is negative.
    /// </summary>
    /// <param name="radian"></param>
    /// <returns></returns>
    public static float Cw2Ccw(this float radian)
    {
        return -radian;
    }

    /// <summary>
    /// For rotation, FFXIV game base CW rotation is negative.
    /// Identical to Cw2CCw, kept separate for code readability.
    /// </summary>
    /// <param name="radian"></param>
    /// <returns></returns>
    public static float Ccw2Cw(this float radian)
    {
        return -radian;
    }

    /// <summary>
    /// Input logic base angle, get logic direction (Diagonal division: Top 0, Cardinal division: Top-Right 0, CW increase)
    /// </summary>
    /// <param name="radian">Logic base angle</param>
    /// <param name="dirs">Total directions</param>
    /// <param name="diagDivision">Diagonal division, default true</param>
    /// <returns>Logic direction corresponding to logic base angle</returns>
    public static int Rad2Dirs(this float radian, int dirs, bool diagDivision = true)
    {
        var r = diagDivision
            ? Math.Round(radian / (2f * float.Pi / dirs))
            : Math.Floor(radian / (2f * float.Pi / dirs));
        r = (r + dirs) % dirs;
        return (int)r;
    }

    /// <summary>
    /// Input coordinate, get logic direction (Diagonal division: Top 0, Cardinal division: Top-Right 0, CW increase)
    /// </summary>
    /// <param name="point">Coordinate point</param>
    /// <param name="center">Center point</param>
    /// <param name="dirs">Total directions</param>
    /// <param name="diagDivision">Diagonal division, default true</param>
    /// <returns>Logic direction corresponding to coordinate</returns>
    public static int Position2Dirs(this Vector3 point, Vector3 center, int dirs, bool diagDivision = true)
    {
        double dirsDouble = dirs;
        var r = diagDivision
            ? Math.Round(dirsDouble / 2 - dirsDouble / 2 * Math.Atan2(point.X - center.X, point.Z - center.Z) / Math.PI) % dirsDouble
            : Math.Floor(dirsDouble / 2 - dirsDouble / 2 * Math.Atan2(point.X - center.X, point.Z - center.Z) / Math.PI) % dirsDouble;
        return (int)r;
    }

    /// <summary>
    /// Rotate a point by logic base radian
    /// </summary>
    /// <param name="point">Point to rotate</param>
    /// <param name="center">Center</param>
    /// <param name="radian">Rotation radian</param>
    /// <returns>Rotated coordinate point</returns>
    public static Vector3 RotatePoint(this Vector3 point, Vector3 center, float radian)
    {
        // Rotate a point clockwise around center by some radian
        Vector2 v2 = new(point.X - center.X, point.Z - center.Z);
        var rot = MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian;
        var length = v2.Length();
        return new Vector3(center.X + MathF.Sin(rot) * length, center.Y, center.Z - MathF.Cos(rot) * length);
    }

    /// <summary>
    /// Extend outward from a center point by logic base angle
    /// </summary>
    /// <param name="center">Center point to extend from</param>
    /// <param name="radian">Rotation radian</param>
    /// <param name="length">Extension length</param>
    /// <returns>Extended coordinate point</returns>
    public static Vector3 ExtendPoint(this Vector3 center, float radian, float length)
    {
        // Extend a point by some radian for some length
        return new Vector3(center.X + MathF.Sin(radian) * length, center.Y, center.Z - MathF.Cos(radian) * length);
    }

    /// <summary>
    /// Find logic base radian from an outer point to center
    /// </summary>
    /// <param name="center">Center</param>
    /// <param name="newPoint">Outer point</param>
    /// <returns>Logic base radian from outer point to center</returns>
    public static float FindRadian(this Vector3 newPoint, Vector3 center)
    {
        var radian = MathF.PI - MathF.Atan2(newPoint.X - center.X, newPoint.Z - center.Z);
        if (radian < 0)
            radian += 2 * MathF.PI;
        return radian;
    }

    /// <summary>
    /// Fold input point horizontally (left/right)
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerX">Center fold line coordinate</param>
    /// <returns></returns>
    public static Vector3 FoldPointHorizon(this Vector3 point, float centerX)
    {
        return point with { X = 2 * centerX - point.X };
    }

    /// <summary>
    /// Fold input point vertically (up/down)
    /// </summary>
    /// <param name="point">Point to fold</param>
    /// <param name="centerZ">Center fold line coordinate</param>
    /// <returns></returns>
    public static Vector3 FoldPointVertical(this Vector3 point, float centerZ)
    {
        return point with { Z = 2 * centerZ - point.Z };
    }

    /// <summary>
    /// Central symmetry of input point
    /// </summary>
    /// <param name="point">Input point</param>
    /// <param name="center">Center point</param>
    /// <returns></returns>
    public static Vector3 PointCenterSymmetry(this Vector3 point, Vector3 center)
    {
        return point.RotatePoint(center, float.Pi);
    }

    /// <summary>
    /// Extend input point inward/outward towards a center point, default inward
    /// </summary>
    /// <param name="point">Point to extend</param>
    /// <param name="center">Center point</param>
    /// <param name="length">Extension length</param>
    /// <param name="isOutside">Whether to extend outward</param>>
    /// <returns></returns>
    public static Vector3 PointInOutside(this Vector3 point, Vector3 center, float length, bool isOutside = false)
    {
        Vector2 v2 = new(point.X - center.X, point.Z - center.Z);
        var targetPos = (point - center) / v2.Length() * length * (isOutside ? 1 : -1) + point;
        return targetPos;
    }

    /// <summary>
    /// Get distance between two points
    /// </summary>
    /// <param name="point"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static float DistanceTo(this Vector3 point, Vector3 target)
    {
        Vector2 v2 = new(point.X - target.X, point.Z - target.Z);
        return v2.Length();
    }

    /// <summary>
    /// Find angle difference between two points, range 0~360deg
    /// </summary>
    /// <param name="basePoint">Base position</param>
    /// <param name="targetPos">Target position to compare</param>
    /// <param name="center">Arena center</param>
    /// <returns></returns>
    public static float FindRadianDifference(this Vector3 targetPos, Vector3 basePoint, Vector3 center)
    {
        var baseRad = basePoint.FindRadian(center);
        var targetRad = targetPos.FindRadian(center);
        var deltaRad = targetRad - baseRad;
        if (deltaRad < 0)
            deltaRad += float.Pi * 2;
        return deltaRad;
    }

    /// <summary>
    /// From third-person perspective, check if a target is to the right of another target.
    /// </summary>
    /// <param name="basePoint">Base position</param>
    /// <param name="targetPos">Target position to compare</param>
    /// <param name="center">Arena center</param>
    /// <returns></returns>
    public static bool IsAtRight(this Vector3 targetPos, Vector3 basePoint, Vector3 center)
    {
        // Looking from center outward, is it on the right
        return targetPos.FindRadianDifference(basePoint, center) < float.Pi;
    }

    /// <summary>
    /// Get specific digit of a given number
    /// </summary>
    /// <param name="val">Given value</param>
    /// <param name="x">Digit position, ones is 1</param>
    /// <returns></returns>
    public static int GetDecimalDigit(this int val, int x)
    {
        string valStr = val.ToString();
        int length = valStr.Length;

        if (x < 1 || x > length)
        {
            return -1;
        }

        char digitChar = valStr[length - x]; // Get x-th digit from right
        return int.Parse(digitChar.ToString());
    }
}
#endregion Calculation Functions

#region Position Sequence Functions
public static class IndexHelper
{
    public static IGameObject? GetById(this ScriptAccessory sa, ulong gameObjectId)
    {
        return sa.Data.Objects.SearchById(gameObjectId);
    }
    
    /// <summary>
    /// Input player dataId, get corresponding position index
    /// </summary>
    /// <param name="pid">Player SourceId</param>
    /// <param name="accessory"></param>
    /// <returns>Corresponding position index for the player</returns>
    public static int GetPlayerIdIndex(this ScriptAccessory accessory, uint pid)
    {
        // Get player IDX
        return accessory.Data.PartyList.IndexOf(pid);
    }

    /// <summary>
    /// Get the position index corresponding to the local player
    /// </summary>
    /// <param name="accessory"></param>
    /// <returns>Position index corresponding to local player</returns>
    public static int GetMyIndex(this ScriptAccessory accessory)
    {
        return accessory.Data.PartyList.IndexOf(accessory.Data.Me);
    }

    /// <summary>
    /// Input player dataId, get corresponding position name, output string for text output only
    /// </summary>
    /// <param name="pid">Player SourceId</param>
    /// <param name="accessory"></param>
    /// <returns>Corresponding position name for the player</returns>
    public static string GetPlayerJobById(this ScriptAccessory accessory, uint pid)
    {
        // Get player role abbreviation, useless, only for DEBUG output
        var idx = accessory.Data.PartyList.IndexOf(pid);
        var str = accessory.GetPlayerJobByIndex(idx);
        return str;
    }

    /// Input position index, get corresponding position name, output string for text output only
    /// </summary>
    /// <param name="idx">Position index</param>
    /// <param name="fourPeople">Whether it's a 4-man dungeon</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static string GetPlayerJobByIndex(this ScriptAccessory accessory, int idx, bool fourPeople = false)
    {
        List<string> role8 = ["MT", "ST", "H1", "H2", "D1", "D2", "D3", "D4"];
        List<string> role4 = ["T", "H", "D1", "D2"];
        if (idx < 0 || idx >= 8 || (fourPeople && idx >= 4))
            return "Unknown";
        return fourPeople ? role4[idx] : role8[idx];
    }
}
#endregion Position Sequence Functions

#region Drawing Functions
public static class AssignDp
{
    /// <summary>
    /// Return arrow guidance related dp
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="ownerObj">Arrow start, can input uint or Vector3</param>
    /// <param name="targetObj">Arrow target, can input uint or Vector3, 0 means no target</param>
    /// <param name="delay">Drawing appear delay</param>
    /// <param name="destroy">Drawing disappear time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="rotation">Arrow rotation angle</param>
    /// <param name="scale">Arrow width</param>
    /// <param name="isSafe">Use safe color</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static DrawPropertiesEdit DrawGuidance(this ScriptAccessory accessory,
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
            throw new ArgumentException("Invalid ownerObj type input");
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
            throw new ArgumentException("Invalid targetObj type input");
        }

        return dp;
    }

    public static DrawPropertiesEdit DrawGuidance(this ScriptAccessory accessory,
        object targetObj, int delay, int destroy, string name, float rotation = 0, float scale = 1f, bool isSafe = true)
    => accessory.DrawGuidance((ulong)accessory.Data.Me, targetObj, delay, destroy, name, rotation, scale, isSafe);

    /// <summary>
    /// Return fan-shaped left/right cleave
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="ownerId">Start target id, usually boss</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="isLeftCleave">Is left cleave</param>
    /// <param name="radian">Fan angle</param>
    /// <param name="scale">Fan size</param>
    /// <param name="byTime">Animation effect fill over time</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawLeftRightCleave(this ScriptAccessory accessory, ulong ownerId, bool isLeftCleave, int delay, int destroy, string name, float radian = float.Pi, float scale = 60f, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(scale);
        dp.Radian = radian;
        dp.Rotation = isLeftCleave ? float.Pi / 2 : -float.Pi / 2;
        dp.Owner = ownerId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return fan-shaped front/back cleave
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="ownerId">Start target id, usually boss</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="isFrontCleave">Is front cleave</param>
    /// <param name="radian">Fan angle</param>
    /// <param name="scale">Fan size</param>
    /// <param name="byTime">Animation effect fill over time</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawFrontBackCleave(this ScriptAccessory accessory, ulong ownerId, bool isFrontCleave, int delay, int destroy, string name, float radian = float.Pi, float scale = 60f, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(scale);
        dp.Radian = radian;
        dp.Rotation = isFrontCleave ? 0 : -float.Pi;
        dp.Owner = ownerId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return dp for nearest/farthest from an object target
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="ownerId">Start target id, usually boss</param>
    /// <param name="orderIdx">Order, starting from 1</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="width">Drawing width</param>
    /// <param name="length">Drawing length</param>
    /// <param name="isNear">true for nearest, false for farthest</param>
    /// <param name="byTime">Animation effect fill over time</param>
    /// <param name="lengthByDistance">Whether length changes with distance</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawTargetNearFarOrder(this ScriptAccessory accessory, ulong ownerId, uint orderIdx,
        bool isNear, float width, float length, int delay, int destroy, string name, bool lengthByDistance = false, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Owner = ownerId;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.CentreResolvePattern =
            isNear ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
        dp.CentreOrderIndex = orderIdx;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode |= lengthByDistance ? ScaleMode.YByDistance : ScaleMode.None;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return dp for nearest/farthest from a coordinate position
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="position">Specific coordinate point</param>
    /// <param name="orderIdx">Order, starting from 1</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="width">Drawing width</param>
    /// <param name="length">Drawing length</param>
    /// <param name="isNear">true for nearest, false for farthest</param>
    /// <param name="byTime">Animation effect fill over time</param>
    /// <param name="lengthByDistance">Whether length changes with distance</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawPositionNearFarOrder(this ScriptAccessory accessory, Vector3 position, uint orderIdx,
        bool isNear, float width, float length, int delay, int destroy, string name, bool lengthByDistance = false, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Position = position;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.TargetResolvePattern =
            isNear ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
        dp.TargetOrderIndex = orderIdx;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode |= lengthByDistance ? ScaleMode.YByDistance : ScaleMode.None;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return dp for ownerId's cast target
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="ownerId">Start target id, usually boss</param>
    /// <param name="width">Drawing width</param>
    /// <param name="length">Drawing length</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="byTime">Animation effect fill over time</param>
    /// <param name="lengthByDistance">Whether length changes with distance</param>
    /// <param name="name">Drawing name</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawOwnersTarget(this ScriptAccessory accessory, ulong ownerId, float width, float length, int delay,
        int destroy, string name, bool lengthByDistance = false, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Owner = ownerId;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode |= lengthByDistance ? ScaleMode.YByDistance : ScaleMode.None;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return dp related to ownerId's enmity
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="ownerId">Start target id, usually boss</param>
    /// <param name="orderIdx">Enmity order, starting from 1</param>
    /// <param name="width">Drawing width</param>
    /// <param name="length">Drawing length</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="byTime">Animation effect fill over time</param>
    /// <param name="lengthByDistance">Whether length changes with distance</param>
    /// <param name="name">Drawing name</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawOwnersEnmityOrder(this ScriptAccessory accessory, ulong ownerId, uint orderIdx, float width, float length, int delay, int destroy, string name, bool lengthByDistance = false, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Owner = ownerId;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.CentreResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.CentreOrderIndex = orderIdx;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode |= lengthByDistance ? ScaleMode.YByDistance : ScaleMode.None;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return dp between owner and target, can modify dp.Owner, dp.TargetObject, dp.Scale
    /// </summary>
    /// <param name="ownerId">Start target id, usually self</param>
    /// <param name="targetId">Target unit id</param>
    /// <param name="rotation">Drawing rotation angle</param>
    /// <param name="width">Drawing width</param>
    /// <param name="length">Drawing length</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="lengthByDistance">Whether length changes with distance</param>
    /// <param name="byTime">Animation effect fill over time</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawTarget2Target(this ScriptAccessory accessory, ulong ownerId, ulong targetId, float width, float length, int delay, int destroy, string name, float rotation = 0, bool lengthByDistance = false, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Rotation = rotation;
        dp.Owner = ownerId;
        dp.TargetObject = targetId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= lengthByDistance ? ScaleMode.YByDistance : ScaleMode.None;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return fan-shaped drawing towards a target
    /// </summary>
    /// <param name="sourceId">Start target id, usually self</param>
    /// <param name="targetId">Target unit id</param>
    /// <param name="radian">Fan angle</param>
    /// <param name="scale">Fan size</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="color">Drawing color</param>
    /// <param name="rotation">Rotation angle</param>
    /// <param name="lengthByDistance">Whether length changes with distance</param>
    /// <param name="byTime">Animation effect fill over time</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawFanToTarget(this ScriptAccessory accessory, ulong sourceId, ulong targetId, float radian, float scale, int delay, int destroy, string name, Vector4 color, float rotation = 0, bool lengthByDistance = false, bool byTime = false)
    {
        var dp = accessory.DrawTarget2Target(sourceId, targetId, scale, scale, delay, destroy, name, rotation, lengthByDistance, byTime);
        dp.Radian = radian;
        dp.Color = color;
        return dp;
    }

    /// <summary>
    /// Return line dp between owner and target, drawn using Line
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="ownerId">Start target id, usually self</param>
    /// <param name="targetId">Target unit id</param>
    /// <param name="scale">Line width</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawConnectionBetweenTargets(this ScriptAccessory accessory, ulong ownerId,
        ulong targetId, int delay, int destroy, string name, float scale = 1f)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(scale);
        dp.Owner = ownerId;
        dp.TargetObject = targetId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= ScaleMode.YByDistance;
        return dp;
    }

    /// <summary>
    /// Return circle dp, follows owner, can modify dp.Owner, dp.Scale
    /// </summary>
    /// <param name="ownerId">Start target id, usually self or Boss</param>
    /// <param name="scale">Circle size</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="byTime">Animation effect fill over time</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawCircle(this ScriptAccessory accessory, ulong ownerId, float scale, int delay, int destroy, string name, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(scale);
        dp.Owner = ownerId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return donut dp, follows owner, can modify dp.Owner, dp.Scale
    /// </summary>
    /// <param name="ownerId">Start target id, usually self or Boss</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="scale">Outer ring solid size</param>
    /// <param name="innerScale">Inner ring hollow size</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="byTime">Animation effect fill over time</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawDonut(this ScriptAccessory accessory, ulong ownerId, float scale, float innerScale, int delay, int destroy, string name, bool byTime = false)
    {
        var dp = accessory.DrawFan(ownerId, float.Pi * 2, 0, scale, innerScale, delay, destroy, name, byTime);
        return dp;
    }

    /// <summary>
    /// Return static dp, usually for guiding fixed positions. Can modify dp.Position, dp.Rotation, dp.Scale
    /// </summary>
    /// <param name="ownerObj">Drawing start, can input uint or Vector3</param>
    /// <param name="targetObj">Drawing target, can input uint or Vector3, 0 means no target</param>
    /// <param name="radian">Graphic angle</param>
    /// <param name="rotation">Rotation angle, 0 degrees North clockwise</param>
    /// <param name="width">Drawing width</param>
    /// <param name="length">Drawing length</param>
    /// <param name="color">If Vector4 use this color</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawStatic(this ScriptAccessory accessory, object ownerObj, object targetObj,
        float radian, float rotation, float width, float length, object color, int delay, int destroy, string name)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);

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
            throw new ArgumentException("Invalid ownerObj type input");
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
            throw new ArgumentException("Invalid ownerObj type input");
        }

        dp.Radian = radian;
        dp.Rotation = rotation.Logic2Game();

        switch (color)
        {
            case Vector4 clr:
                dp.Color = clr;
                break;
            default:
                dp.Color = accessory.Data.DefaultDangerColor;
                break;
        }
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        return dp;
    }

    /// <summary>
    /// Return static circle dp, usually for guiding fixed positions.
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="center">Circle center position</param>
    /// <param name="color">Circle color</param>
    /// <param name="scale">Circle size, default 1.5f</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawStaticCircle(this ScriptAccessory accessory, Vector3 center, Vector4 color,
        int delay, int destroy, string name, float scale = 1.5f)
        => accessory.DrawStatic(center, (ulong)0, 0, 0, scale, scale, color, delay, destroy, name);
    // {
    //     var dp = accessory.DrawStatic(center, (uint)0, 0, 0, scale, scale, color, delay, destroy, name);
    //     return dp;
    // }

    /// <summary>
    /// Return static donut dp, usually for guiding fixed positions.
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="center">Donut center position</param>
    /// <param name="color">Donut color</param>
    /// <param name="scale">Donut outer radius, default 1.5f</param>
    /// <param name="innerscale">Donut inner radius, default scale-0.05f</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawStaticDonut(this ScriptAccessory accessory, Vector3 center, Vector4 color,
        int delay, int destroy, string name, float scale, float innerscale = 0)
        => accessory.DrawStatic(center, (ulong)0,
        float.Pi * 2, 0, scale, scale, color, delay, destroy, name);

    // {
    //     var dp = accessory.DrawStatic(center, (uint)0, float.Pi * 2, 0, scale, scale, color, delay, destroy, name);
    //     dp.InnerScale = innerscale != 0f ? new Vector2(innerscale) : new Vector2(scale - 0.05f);
    //     return dp;
    // }

    /// <summary>
    /// Return rectangle
    /// </summary>
    /// <param name="ownerId">Start target id, usually self or Boss</param>
    /// <param name="width">Rectangle width</param>
    /// <param name="length">Rectangle length</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="byTime">Animation effect fill over time</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawRect(this ScriptAccessory accessory, ulong ownerId, float width, float length, int delay, int destroy, string name, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Owner = ownerId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return fan shape
    /// </summary>
    /// <param name="ownerId">Start target id, usually self or Boss</param>
    /// <param name="radian">Fan radian</param>
    /// <param name="rotation">Graphic rotation angle</param>
    /// <param name="scale">Fan size</param>
    /// <param name="innerScale">Fan inner hollow size</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="byTime">Animation effect fill over time</param>
    /// <param name="accessory"></param>
    /// <returns></returns>
    public static DrawPropertiesEdit DrawFan(this ScriptAccessory accessory, ulong ownerId, float radian, float rotation, float scale, float innerScale, int delay, int destroy, string name, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(scale);
        dp.InnerScale = new Vector2(innerScale);
        dp.Radian = radian;
        dp.Rotation = rotation;
        dp.Owner = ownerId;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    /// <summary>
    /// Return knockback
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="target">Knockback source, can input uint or Vector3</param>
    /// <param name="width">Knockback drawing width</param>
    /// <param name="length">Knockback drawing length/distance</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="ownerId">Start target ID, usually self or other player</param>
    /// <param name="byTime">Animation effect fill over time</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static DrawPropertiesEdit DrawKnockBack(this ScriptAccessory accessory, ulong ownerId, object target, float length, int delay, int destroy, string name, float width = 1.5f, bool byTime = false)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Scale = new Vector2(width, length);
        dp.Owner = ownerId;

        if (target is uint or ulong)
        {
            dp.TargetObject = (ulong)target;
        }
        else if (target is Vector3 tpos)
        {
            dp.TargetPosition = tpos;
        }
        else
        {
            throw new ArgumentException("Invalid DrawKnockBack target type input");
        }

        dp.Rotation = float.Pi;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Delay = delay;
        dp.DestoryAt = destroy;
        dp.ScaleMode |= byTime ? ScaleMode.ByTime : ScaleMode.None;
        return dp;
    }

    public static DrawPropertiesEdit DrawKnockBack(this ScriptAccessory accessory, object target, float length,
        int delay, int destroy, string name, float width = 1.5f, bool byTime = false)
        => accessory.DrawKnockBack(accessory.Data.Me, target, length, delay, destroy, name, width, byTime);

    /// <summary>
    /// Return gaze avoidance
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="target">Gaze source, can input uint or Vector3</param>
    /// <param name="delay">Delay delay ms to appear</param>
    /// <param name="destroy">Drawing disappears after destroy ms from appearance</param>
    /// <param name="name">Drawing name</param>
    /// <param name="ownerId">Start target ID, usually self or other player</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static DrawPropertiesEdit DrawSightAvoid(this ScriptAccessory accessory, ulong ownerId, object target, int delay, int destroy, string name)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = name;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = ownerId;


        if (target is uint or ulong)
        {
            dp.TargetObject = (ulong)target;
        }
        else if (target is Vector3 tpos)
        {
            dp.TargetPosition = tpos;
        }
        else
        {
            throw new ArgumentException("Invalid DrawKnockBack target type input");
        }

        dp.Delay = delay;
        dp.DestoryAt = destroy;
        return dp;
    }

    public static DrawPropertiesEdit DrawSightAvoid(this ScriptAccessory accessory, object target, int delay,
        int destroy, string name)
        => accessory.DrawSightAvoid(accessory.Data.Me, target, delay, destroy, name);

    /// <summary>
    /// Return multi-direction extend guidance
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="owner">Spread source</param>
    /// <param name="extendDirs">Spread angles</param>
    /// <param name="myDirIdx">Player's corresponding angle idx</param>
    /// <param name="width">Guidance arrow width</param>
    /// <param name="length">Guidance arrow length</param>
    /// <param name="delay">Drawing appear delay</param>
    /// <param name="destroy">Drawing disappear time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="colorPlayer">Player's corresponding arrow guidance color</param>
    /// <param name="colorNormal">Other players' corresponding arrow guidance color</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static List<DrawPropertiesEdit> DrawExtendDirection(this ScriptAccessory accessory, object owner,
        List<float> extendDirs, int myDirIdx, float width, float length, int delay, int destroy, string name,
        Vector4 colorPlayer, Vector4 colorNormal)
    {
        List<DrawPropertiesEdit> dpList = [];


        if (owner is uint or ulong)
        {
            for (var i = 0; i < extendDirs.Count; i++)
            {
                var dp = accessory.DrawRect((ulong)owner, width, length, delay, destroy, $"{name}{i}");
                dp.Rotation = extendDirs[i];
                dp.Color = i == myDirIdx ? colorPlayer : colorNormal;
                dpList.Add(dp);
            }
        }
        else if (owner is Vector3 spos)
        {
            for (var i = 0; i < extendDirs.Count; i++)
            {
                var dp = accessory.DrawGuidance(spos, spos.ExtendPoint(extendDirs[i], length), delay, destroy,
                    $"{name}{i}", 0, width);
                dp.Color = i == myDirIdx ? colorPlayer : colorNormal;
                dpList.Add(dp);
            }
        }
        else
        {
            throw new ArgumentException("Invalid DrawExtendDirection target type input");
        }

        return dpList;
    }

    /// <summary>
    /// Return multi-location guidance list
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="positions">Location positions</param>
    /// <param name="delay">Drawing appear delay</param>
    /// <param name="destroy">Drawing disappear time</param>
    /// <param name="name">Drawing name</param>
    /// <param name="colorPosPlayer">Corresponding position marker action color</param>
    /// <param name="colorPosNormal">Corresponding position marker prepare color</param>
    /// <param name="colorGo">Guidance departure arrow color</param>
    /// <param name="colorPrepare">Guidance prepare arrow color</param>
    /// <returns>Three Lists in dpList: position markers, player guidance arrows, location-to-next-location guidance arrows</returns>
    public static List<List<DrawPropertiesEdit>> DrawMultiGuidance(this ScriptAccessory accessory,
        List<Vector3> positions, List<int> delay, List<int> destroy, string name,
        Vector4 colorGo, Vector4 colorPrepare, Vector4 colorPosNormal, Vector4 colorPosPlayer)
    {
        List<List<DrawPropertiesEdit>> dpList = [[], [], []];
        for (var i = 0; i < positions.Count; i++)
        {
            var dpPos = accessory.DrawStaticCircle(positions[i], colorPosPlayer, delay[i], destroy[i], $"{name}pos{i}");
            dpList[0].Add(dpPos);
            var dpGuide = accessory.DrawGuidance(positions[i], colorGo, delay[i], destroy[i], $"{name}guide{i}");
            dpList[1].Add(dpGuide);
            if (i == positions.Count - 1) break;
            var dpPrep = accessory.DrawGuidance(positions[i], positions[i + 1], delay[i], destroy[i], $"{name}prep{i}");
            dpList[2].Add(dpPrep);
        }
        return dpList;
    }

    public static void DebugMsg(this ScriptAccessory accessory, string str, bool debugMode = false, bool debugChat = false)
    {
        if (!debugMode)
            return;
        accessory.Log.Debug($"/e [DEBUG] {str}");

        if (!debugChat)
            return;
        accessory.Method.SendChat($"/e [DEBUG] {str}");
    }

    /// <summary>
    /// Convert List info to string.
    /// </summary>
    /// <param name="accessory"></param>
    /// <param name="myList"></param>
    /// <param name="isJob">Is job, call job conversion function before converting to string</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string BuildListStr<T>(this ScriptAccessory accessory, List<T> myList, bool isJob = false)
    {
        return string.Join(", ", myList.Select(item =>
        {
            if (isJob && item != null && item is int i)
                return accessory.GetPlayerJobByIndex(i);
            return item?.ToString() ?? "";
        }));
    }
}

#endregion Drawing Functions